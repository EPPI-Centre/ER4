using EPPIDataServices.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace PubmedImport
{
    public static class FileParser
    {

        public static bool BulkInsertDataTable(DataTable table,
           SqlConnection conn, SqlTransaction tran)
        {

            try
            {

            
                using (SqlBulkCopy bulkCopy =
                        new SqlBulkCopy(conn, SqlBulkCopyOptions.KeepIdentity | SqlBulkCopyOptions.CheckConstraints, tran))
                {
                    bulkCopy.DestinationTableName = table.TableName;
                    bulkCopy.ColumnMappings.Clear();

                    foreach (DataColumn col in table.Columns)
                    {
                        bulkCopy.ColumnMappings.Add(col.ColumnName, col.ColumnName);
                    }

                    bulkCopy.BulkCopyTimeout = 1000;

                    bulkCopy.WriteToServer(table);

                }
                }
                catch (Exception ex)
                {
                    return false;
                }
            // LoadCitationIdentities(lstObjs, Items_S, conn, tran);
            return true;
        }

        public static string GetKeywordsString(ReferenceRecord rec)
        {
            string res = "";
            foreach (KeywordObject Keyw in rec.Keywords)
            {
                res += ((Keyw.Major) ? "*" : "") + Keyw.Name + Environment.NewLine;
            }
            return res;
        }

        static List<ReferenceRecord> Citations = new List<ReferenceRecord>();
        static List<ReferenceRecord> UpdateCitations = new List<ReferenceRecord>();
        static FileParserResult result;
		public static FileParserResult ParseFile(string filepath)
        {

            Program.Logger.LogMessageLine("Parsing: " + filepath + ".");
            DateTime start = DateTime.Now;
            string fileContents;
            string upDelMsg = Program.deleteRecords ? "deleted." : "updated.";//used to report what it is that we're doing!
            result = new FileParserResult(filepath, Program.deleteRecords);//we are going to report success if at least one citation was parsed and saved

            List<XElement> values = new List<XElement>();
            try
            {
                using (var sr = new StreamReader(filepath))
                {
                    fileContents = sr.ReadToEnd();
                }
                XDocument xDoc = XDocument.Parse(fileContents);

                values = (from r in xDoc.Descendants("PubmedArticleSet")
                          from a in r.Elements("PubmedArticle")
                          select a
                             ).ToList();
                Program.Logger.LogMessageLine("File contains " + values.Count.ToString() + " records.");
                result.CitationsInFile = values.Count;
            }
            catch
            {
                result.Success = false;
                Program.Logger.LogMessageLine("Error parsing file: no citation processed.");
                result.Messages.Add("Error parsing file: no citation processed.");
                DeleteParsedFile(filepath);
                return (result);
            }

            if (Program.maxCount != int.MaxValue)
            {
                Program.Logger.LogMessageLine("Limiting import to: " + Program.maxCount.ToString() + " references.");
                result.Messages.Add("Limiting import to: " + Program.maxCount.ToString() + " references.");
            }
            Citations = new List<ReferenceRecord>();
            foreach (XElement xCit in values)
            {
                //Program.Logger.LogMessageLine("Processing record: " + (Citations.Count + 1).ToString() + ".");
                try
                {
                    ReferenceRecord curr = PubMedXMLParser.ParseCitation(xCit);
                    if (curr != null && curr.Type != "Retraction")
                    {//for now, we simply avoid to add retractions, not clear what we should be doing...
                        AddToImportList(curr);//checks if the current PMID has been parsed already within this same file!
                    }
                }
                catch (Exception e)
                {
                    result.Messages.Add(e.Message);
                    Program.Logger.LogMessageLine(e.Message);
                    result.ErrorCount++;
                }
                if (Program.currCount >= Program.maxCount)
                {
                    result.Messages.Add("Maxcount reached, processing stopped after " + Citations.Count.ToString() + " out of " + values.Count.ToString() + " (in file).");
                    break;
                }
            }
            string tmpMsg = "Parsing done: " + Citations.Count.ToString() + " citations will be "
                                               + (Program.deleteRecords ? "deleted (if present)" : "imported/updated.");
            Program.Logger.LogMessageLine(tmpMsg);
            result.Messages.Add(tmpMsg);
            DateTime now = DateTime.Now;
            Program.Logger.LogMessageLine("Finding references that need updating & updating them.");
            int i = 0;
            while (i < Citations.Count)
            {//first pass, see which ones are updates, and save them on a one-by-one basis
                ReferenceRecord rec = Citations[i];


                using (SqlConnection conn = new SqlConnection(Program.SqlHelper.DataServiceDB))
                {
                    string pmid = "";
                    ReferenceRecord ExsistingCit = null;
                    foreach (ExternalID exID in rec.ExternalIDs)
                    {
                        if (exID.Name == "pubmed")
                        {
                            ExsistingCit = GetReferenceRecordByPMID(conn, exID.Value);
                            pmid = exID.Value;
                            break;
                        }
                    }
                    if (ExsistingCit != null)
                    {
                        //we need to check if ExistingCit is newer
                        bool updateExisting = false;
                        if (Program.deleteRecords == false && rec.PubMedDate != null && ExsistingCit.PubMedDate != null)
                        {
                            if (ExsistingCit.PubMedDate == rec.PubMedDate)
                            {//new and old record have same date, see if Version number helps, save changes otherwise...

                                if (ExsistingCit.PubmedPmidVersion == 0 || rec.PubmedPmidVersion == 0)
                                {//we're missing a version number, we'll update
                                    updateExisting = true;
                                }
                                else if (rec.PubmedPmidVersion >= ExsistingCit.PubmedPmidVersion)
                                {//if we do have V numbers, update ONLY if incoming version is bigger or equal
                                    updateExisting = true;
                                }
                            }
                            else if (rec.PubMedDate > ExsistingCit.PubMedDate)
                            {
                                updateExisting = true;
                            }
                            else
                            {//newrecord is older, won't save parsed version
                                Program.Logger.LogMessageLine("Match in DB: " + pmid + ". Citation will be not be updated (DB record is newer).");
                            }
                        }
                        else
                        {//we don't have the dates, we'll update, just in case.
                         //this always happens when we're deleting (we don't check version date/number).
                            updateExisting = true;
                            result.Messages.Add("Match in DB (v. date missing): " + pmid + ". Citation will be " + upDelMsg);
                            Program.Logger.LogMessageLine("Match in DB (v. date missing): " + pmid + ". Citation will be " + upDelMsg);
                            //UpdateExsitingCitation(ExsistingCit, rec); //changes ExsistingCit therein...
                            //if (Program.simulate == false && Program.deleteRecords == false) session.Store(ExsistingCit);
                        }
                        //now decide what we want to do options:
                        //nothing if simulating,
                        //delete the found citation if Program.deleteRecords && not simulating
                        //save updated citation if updateExisting == true
                        //nothing for now, if citation is new.
                        if (Program.deleteRecords)
                        {
                            result.UpdatedPMIDs += pmid + ", ";
                            result.CitationsCommitted++;
                            if (!Program.simulate) ExsistingCit.DeleteSelf(conn, Program.SqlHelper);
                        }
                        else if (updateExisting)
                        {
                            result.UpdatedPMIDs += pmid + ", ";
                            result.CitationsCommitted++;
                            UpdateExsitingCitation(ExsistingCit, rec); //changes ExsistingCit therein...
                            Citations.Remove(rec);//we use this to create new citations...
                            UpdateCitations.Add(ExsistingCit);
                            if (!Program.simulate) ExsistingCit.SaveSelf(conn, Program.SqlHelper);
                        }
                        else if (!updateExisting)
                        {//we don't want this reference to be bulk inserted below! Parser ref is older than the one in DB so nothing should change.
                            Citations.Remove(rec);
                            //i++;
                        }
                    }
                    else
                    {//existing citation is new/unknown to us.
                        //do nothing
                        i++;
                    }

                }
            }
            string savedin = EPPILogger.Duration(now);
            Program.Logger.LogMessageLine("Done updating references in: " + savedin);
            result.Messages.Add("Done updating references in: " + savedin);
            now = DateTime.Now;
            //the new citations have not been saved, we'd like to do this in bulk, probably.
            Program.Logger.LogMessageLine("Done updating references, now saving " + Citations.Count.ToString() + " new citations.");
            if (Program.simulate == false && Program.deleteRecords == false && Citations.Count > 0)
            {//second pass, save all remaining Citations (those that were not updated)
                using (SqlConnection conn = new SqlConnection(Program.SqlHelper.DataServiceDB))
                {
                    //bool fetchIdentities = true;
                    conn.Open();
                    int AuthorsCount = 0;
                    int ExternalCount = 0;
                    Int64 Items_S;
                    Int64 Author_S;
                    Int64 External_S;

                    try
                    {

                        // Authors count required
                        foreach (var item in Citations)
                        {
                            AuthorsCount += item.Authors.Count();
                            ExternalCount += item.ExternalIDs.Count();
                        }

                        using (SqlCommand cmd = new SqlCommand("st_ReferencesImportPrepare", conn))
                        {
                            //prepare all tables
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@Items_Number", Citations.Count));
                            cmd.Parameters.Add(new SqlParameter("@Authors_Number", AuthorsCount));
                            cmd.Parameters.Add(new SqlParameter("@Externals_Number", ExternalCount));
                            cmd.Parameters.Add("@Item_Seed", SqlDbType.BigInt);
                            cmd.Parameters["@Item_Seed"].Direction = ParameterDirection.Output;
                            cmd.Parameters.Add("@Author_Seed", SqlDbType.BigInt);
                            cmd.Parameters["@Author_Seed"].Direction = ParameterDirection.Output;
                            cmd.Parameters.Add("@External_Seed", SqlDbType.BigInt);
                            cmd.Parameters["@External_Seed"].Direction = ParameterDirection.Output;
                            cmd.ExecuteNonQuery();

                            //get seeds values
                            Items_S = (Int64)cmd.Parameters["@Item_Seed"].Value;
                            Author_S = (Int64)cmd.Parameters["@Author_Seed"].Value;
                            External_S = (Int64)cmd.Parameters["@External_Seed"].Value;

                        }

                        var tables = ReferenceRecord.ToDataTables(Citations, Items_S, External_S, Author_S);
                        foreach (DataTable dt in tables)
                        {
                            if (dt.TableName == "TB_REFERENCE")
                            {
                                result.CitationsCommitted += dt.Rows.Count;
                            }
                            var testBool = BulkInsertDataTable(dt, conn, null);
                        }


                        //using (SqlTransaction tran = conn.BeginTransaction())
                        //{

                        //            List<SQLCitationObject> lstCits = BulkInsertFlatReferences(Citations, Items_S, conn, tran);



                        //            BulkInsertFlatAuthors(lstCits, Author_S, conn, tran);

                        //            BulkInsertExternalIDS(lstCits, External_S, conn, tran);


                        //            tran.Commit();
                        //}

                    }
                    catch (SqlException ex)
                    {
                        Program.Logger.LogException(ex, "FATAL ERROR: failed to bulk insert new references.");
                    }
                }
                //ReferenceTables.TB_REFERENCETable. ReferencesRow = new ReferencesTB
            }
            savedin = EPPILogger.Duration(now);
            Program.Logger.LogMessageLine("Saved new references in: " + savedin);
            result.Messages.Add("Saved new references in: " + savedin);
            Program.Logger.LogMessageLine("Updated refs (PMIDs): " + result.UpdatedPMIDs);

            //if (Program.simulate == false)
            //{
            //	try
            //	{
            //		session.Advanced.DocumentStore.SetRequestsTimeoutFor(new TimeSpan(0, 5, 1));
            //		Program.Logger.LogMessageLine("Saving to DB...");
            //		DateTime now = DateTime.Now;
            //		session.SaveChanges();
            //		string savedin = Program.Duration(now);
            //		Program.Logger.LogMessageLine("Saved in: " + savedin);
            //		result.Messages.Add("Saved in: " + savedin);
            //	}
            //	catch (Exception e)
            //	{
            //		result.ErrorCount++;
            //		if (e.Message.Contains("A task was canceled."))
            //		{//try one more time!
            //			try
            //			{

            //				session.Advanced.DocumentStore.SetRequestsTimeoutFor(new TimeSpan(0, 10, 0));
            //				Program.Logger.LogMessageLine("Saving timed out, retrying...");
            //				result.Messages.Add("Saving timed out, retrying...");
            //				DateTime now = DateTime.Now;
            //				session.SaveChanges();
            //				string savedin = Program.Duration(now);
            //				Program.Logger.LogMessageLine("Saved in: " + savedin);
            //				result.Messages.Add("Saved in: " + savedin);
            //			}
            //			catch (Exception e2)
            //			{
            //				result.CitationsCommitted = 0;
            //				result.ErrorCount++;
            //				result.Messages.Add("Catastrophic failure: ERROR saving to DB on both attempts.");
            //				result.Messages.Add("ERROR message: " + e.Message);
            //				Program.Logger.LogMessageLine("Catastrophic failure: ERROR saving to DB on both attempts.");
            //				Program.Logger.LogMessageLine("ERROR message: " + e.Message);
            //				Program.Logger.LogMessageLine("");
            //				DeleteParsedFile(filepath);
            //				result.Success = false;
            //				return result;
            //			}
            //		}
            //		else
            //		{
            //			result.CitationsCommitted = 0;

            //			result.Messages.Add("Catastrophic failure: ERROR saving to DB.");
            //			result.Messages.Add("ERROR message: " + e.Message + ".");
            //			Program.Logger.LogMessageLine("Catastrophic failure: ERROR saving to DB.");
            //			Program.Logger.LogMessageLine("ERROR message: " + e.Message + ".");
            //			Program.Logger.LogMessageLine("");
            //			DeleteParsedFile(filepath);
            //			result.Success = false;
            //			return result;
            //		}
            //	}
            //}

            DeleteParsedFile(filepath);

            string duration = EPPILogger.Duration(start);
            Program.Logger.LogMessageLine("Imported " + Citations.Count.ToString() + " records in " + duration);
            result.Messages.Add("Imported " + Citations.Count.ToString() + " records in " + duration);
            result.EndTime = DateTime.Now;
            if (Citations != null && result.CitationsCommitted > 0 && result.ErrorCount == 0)
            {
                return result;
            }
            else if (Citations != null && result.CitationsCommitted > 0 && result.ErrorCount > 0)
            {
                string tmp = "Non fatal errors count: " + result.ErrorCount.ToString() + ".";
                result.Messages.Add(tmp);
                Program.Logger.LogMessageLine(tmp);
                return result;
            }
            else
            {
                string tmp = "No citation to import found in file";
                result.Messages.Add(tmp);
                result.Success = false;
                Program.Logger.LogMessageLine(tmp);
                return result;
            }
        }



        private static ReferenceRecord GetReferenceRecordByPMID(SqlConnection conn, string pubmedID)
        {
            ReferenceRecord res = null;
            SqlParameter extName = new SqlParameter("@ExternalIDName", "pubmed");
            SqlParameter pmid = new SqlParameter("@ExternalIDValue", pubmedID);
            try
            {
                using (SqlDataReader reader = Program.SqlHelper.ExecuteQuerySP(conn, "st_FindCitationByExternalID", extName, pmid))
                {
                    if (reader.Read()) res = ReferenceRecord.GetReferenceRecord(reader);
                }
            }
            catch (Exception e)
            {
                Program.Logger.LogException(e, "Error fetching existing ref and/or creating local object.");
                result.ErrorCount++;
            }
            return res;
        }
        

        private static void DeleteParsedFile(string filepath)
		{
			if (File.Exists(filepath))
			{
				try
				{
					File.Delete(filepath);
				}
				catch (Exception)
				{
					Program.Logger.LogMessageLine("Warning: could not delete \"" + filepath + "\".");
				}
			}
		}
		private static ReferenceRecord UpdateExsitingCitation(ReferenceRecord oldRecord, ReferenceRecord newRecord)
		{
			oldRecord.Title = newRecord.Title;
			oldRecord.ParentTitle = newRecord.ParentTitle;
			oldRecord.ExternalIDs = newRecord.ExternalIDs;
			oldRecord.PublicationYear = newRecord.PublicationYear;
			oldRecord.Volume = newRecord.Volume;
			oldRecord.Issue = newRecord.Issue;
			oldRecord.Abstract = newRecord.Abstract;
			oldRecord.Edition = newRecord.Edition;
			oldRecord.Urls = newRecord.Urls;
			oldRecord.Country = newRecord.Country;
			oldRecord.Keywords = newRecord.Keywords;
			oldRecord.Authors = newRecord.Authors;
			oldRecord.StartPage = newRecord.StartPage;
			oldRecord.EndPage = newRecord.EndPage;
			oldRecord.Issn = newRecord.Issn;
			oldRecord.PubMedDate = newRecord.PubMedDate;
			oldRecord.SetSearchText();
			oldRecord.AutoSetShortTitle();
			return oldRecord;
		}
		
		private static void AddToImportList(ReferenceRecord curr)
		{
			ExternalID Pmid = curr.ExternalIDByType("pubmed");
			if (Pmid == null)
			{
				result.ErrorCount++;
				result.Messages.Add("!! Error: skipping current citation as it does not have a PMID!");
				Program.Logger.LogMessageLine("!! Error: skipping current citation as it does not have a PMID!");
				return;
			}
			foreach (ReferenceRecord cit in Citations)
			{
				//ExternalID oldPmid = cit.ExternalIDByType("pubmed");
				//if (oldPmid == Pmid)
				if (cit.ExternalIDs.Contains(Pmid))
				{//we have already processed this citation(!), we'll update it
				 //Program.Logger.LogMessageLine("Internal Match: " + Pmid.Value);
					UpdateExsitingCitation(cit, curr);
					return;
				}
			}
			//following block only happens if curr isn't already present in list (based on PMID)
			Citations.Add(curr);
			Program.currCount++;
		}
	}
    
	public class FileParserResult
	{
		public bool Success { get; set; }
		public bool IsDeleting { get; set; }
		public int ErrorCount { get; set; }
		public string FileName { get; set; }
		public string UpdatedPMIDs { get; set; }
		public int CitationsInFile { get; set; }
		public int CitationsCommitted { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }
		public bool HasErrors
		{
			get { return (ErrorCount > 0); }
		}
		public List<string> Messages { get; set; }
		public FileParserResult(string fileName, bool isDeleting)
		{
			if (!fileName.Contains('\\'))
			{
				FileName = fileName;
			}
			else
			{
				string[] splitted = fileName.Split('\\');
				FileName = splitted[splitted.Length - 1];
			}
			IsDeleting = isDeleting;
			StartTime = DateTime.Now;
            EndTime = DateTime.Now;
			Messages = new List<string>();
			Success = true;
			ErrorCount = 0;
		}
	}
}
