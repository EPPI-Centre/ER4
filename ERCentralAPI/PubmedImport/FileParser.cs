using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace PubmedImport
{
    public static class FileParser
    {
		static List<CitationRecord> Citations = new List<CitationRecord>();
        static List<CitationRecord> UpdateCitations = new List<CitationRecord>();
        static FileParserResult result;
		public static FileParserResult ParseFile(string filepath)
		{

			Program.LogMessageLine("Parsing: " + filepath + ".");
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
				Program.LogMessageLine("File contains " + values.Count.ToString() + " records.");
				result.CitationsInFile = values.Count;
			}
			catch
			{
				result.Success = false;
				Program.LogMessageLine("Error parsing file: no citation processed.");
				result.Messages.Add("Error parsing file: no citation processed.");
				DeleteParsedFile(filepath);
				return (result);
			}

			if (Program.maxCount != int.MaxValue)
			{
				Program.LogMessageLine("Limiting import to: " + Program.maxCount.ToString() + " references.");
				result.Messages.Add("Limiting import to: " + Program.maxCount.ToString() + " references.");
			}
			Citations = new List<CitationRecord>();
			foreach (XElement xCit in values)
			{
				//Program.LogMessageLine("Processing record: " + (Citations.Count + 1).ToString() + ".");
				try
				{
					CitationRecord curr = PubMedXMLParser.ParseCitation(xCit);
					if (curr != null && curr.Type != "Retraction")
					{//for now, we simply avoid to add retractions, not clear what we should be doing...
						AddToImportList(curr);//checks if the current PMID has been parsed already within this same file!
					}
				}
				catch (Exception e)
				{
					result.Messages.Add(e.Message);
					Program.LogMessageLine(e.Message);
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
			Program.LogMessageLine(tmpMsg);
			result.Messages.Add(tmpMsg);
            DateTime now = DateTime.Now;
            Program.LogMessageLine("Finding references that need updating & updating them.");
            int i = 0;
            while (i < Citations.Count)
            {//first pass, see which ones are updates, and save them on a one-by-one basis
                CitationRecord rec = Citations[i];


                using (SqlConnection conn = new SqlConnection(Program.SqlHelper.DataServiceDB))
                {
                    string pmid = "";
                    CitationRecord ExsistingCit = null;
                    foreach (ExternalID exID in rec.ExternalIDs)
                    {
                        if (exID.Name == "pubmed")
                        {
                            ExsistingCit = GetCitationRecordByPMID(conn, exID.Value);
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
                                Program.LogMessageLine("Match in DB: " + pmid + ". Citation will be not be updated (DB record is newer).");
                            }
                        }
                        else
                        {//we don't have the dates, we'll update, just in case.
                         //this always happens when we're deleting (we don't check version date/number).
                            updateExisting = true;
                            result.Messages.Add("Match in DB (v. date missing): " + pmid + ". Citation will be " + upDelMsg);
                            Program.LogMessageLine("Match in DB (v. date missing): " + pmid + ". Citation will be " + upDelMsg);
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
                            if (!Program.simulate) ExsistingCit.DeleteSelf(conn);
                        }
                        else if (updateExisting)
                        {
                            result.UpdatedPMIDs += pmid + ", ";
                            result.CitationsCommitted++;
                            UpdateExsitingCitation(ExsistingCit, rec); //changes ExsistingCit therein...
                            Citations.Remove(rec);//we use this to create new citations...
                            UpdateCitations.Add(ExsistingCit);
                            if (!Program.simulate) ExsistingCit.SaveSelf(conn);
                        }
                        else if (!updateExisting)
                        {
                            i++;
                        }
                    }
                    else
                    {//existing citation is new/unknown to us.
                        //do nothing
                        i++;
                    }

                }
            }
            string savedin = Program.Duration(now);
            Program.LogMessageLine("Done updating references in: " + savedin);
            result.Messages.Add("Done updating references in: " + savedin);
            now = DateTime.Now;
            //the new citations have not been saved, we'd like to do this in bulk, probably.
            if (Program.simulate == false && Program.deleteRecords == false)
            {//second pass, save all remaining Citations (those that were not updated)
                Program.LogMessageLine("Done updating references, now saving " + Citations.Count.ToString() + " new citations.");
                using (SqlConnection conn = new SqlConnection(Program.SqlHelper.DataServiceDB))
                {
                    foreach (CitationRecord rec in Citations)
                    {//!!!!!!!!!! we should prepare in-code tables and then update in bulk, for speed...
                        rec.SaveSelf(conn);
                        result.CitationsCommitted++;
                    }
                }
                savedin = Program.Duration(now);
                Program.LogMessageLine("Saved new references in: " + savedin);
                result.Messages.Add("Saved new references in: " + savedin);
                Program.LogMessageLine("Updated refs (PMIDs): " + result.UpdatedPMIDs);
            }
            
			//if (Program.simulate == false)
			//{
			//	try
			//	{
			//		session.Advanced.DocumentStore.SetRequestsTimeoutFor(new TimeSpan(0, 5, 1));
			//		Program.LogMessageLine("Saving to DB...");
			//		DateTime now = DateTime.Now;
			//		session.SaveChanges();
			//		string savedin = Program.Duration(now);
			//		Program.LogMessageLine("Saved in: " + savedin);
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
			//				Program.LogMessageLine("Saving timed out, retrying...");
			//				result.Messages.Add("Saving timed out, retrying...");
			//				DateTime now = DateTime.Now;
			//				session.SaveChanges();
			//				string savedin = Program.Duration(now);
			//				Program.LogMessageLine("Saved in: " + savedin);
			//				result.Messages.Add("Saved in: " + savedin);
			//			}
			//			catch (Exception e2)
			//			{
			//				result.CitationsCommitted = 0;
			//				result.ErrorCount++;
			//				result.Messages.Add("Catastrophic failure: ERROR saving to DB on both attempts.");
			//				result.Messages.Add("ERROR message: " + e.Message);
			//				Program.LogMessageLine("Catastrophic failure: ERROR saving to DB on both attempts.");
			//				Program.LogMessageLine("ERROR message: " + e.Message);
			//				Program.LogMessageLine("");
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
			//			Program.LogMessageLine("Catastrophic failure: ERROR saving to DB.");
			//			Program.LogMessageLine("ERROR message: " + e.Message + ".");
			//			Program.LogMessageLine("");
			//			DeleteParsedFile(filepath);
			//			result.Success = false;
			//			return result;
			//		}
			//	}
			//}
			
			DeleteParsedFile(filepath);
            
			string duration = Program.Duration(start);
			Program.LogMessageLine("Imported " + Citations.Count.ToString() + " records in " + duration);
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
				Program.LogMessageLine(tmp);
				return result;
			}
			else
			{
				string tmp = "No citation to import found in file";
				result.Messages.Add(tmp);
				result.Success = false;
				Program.LogMessageLine(tmp);
				return result;
			}
		}
        private static CitationRecord GetCitationRecordByPMID(SqlConnection conn, string pubmedID)
        {
            CitationRecord res = null;
            SqlParameter extName = new SqlParameter("@ExternalIDName", "pubmed");
            SqlParameter pmid = new SqlParameter("@ExternalIDValue", pubmedID);
            try
            {
                using (SqlDataReader reader = SQLHelper.ExecuteQuerySP(conn, "st_findCitationByExternalID", extName, pmid))
                {
                    if (reader.Read()) res = CitationRecord.GetCitationRecord(reader);
                }
            }
            catch (Exception e)
            {
                Program.LogException(e, "Error fetching existing ref and/or creating local object.");
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
					Program.LogMessageLine("Warning: could not delete \"" + filepath + "\".");
				}
			}
		}
		private static CitationRecord UpdateExsitingCitation(CitationRecord oldRecord, CitationRecord newRecord)
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
		
		private static void AddToImportList(CitationRecord curr)
		{
			ExternalID Pmid = curr.ExternalIDByType("pubmed");
			if (Pmid == null)
			{
				result.ErrorCount++;
				result.Messages.Add("!! Error: skipping current citation as it does not have a PMID!");
				Program.LogMessageLine("!! Error: skipping current citation as it does not have a PMID!");
				return;
			}
			foreach (CitationRecord cit in Citations)
			{
				//ExternalID oldPmid = cit.ExternalIDByType("pubmed");
				//if (oldPmid == Pmid)
				if (cit.ExternalIDs.Contains(Pmid))
				{//we have already processed this citation(!), we'll update it
				 //Program.LogMessageLine("Internal Match: " + Pmid.Value);
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
			Messages = new List<string>();
			Success = true;
			ErrorCount = 0;
		}
	}
}
