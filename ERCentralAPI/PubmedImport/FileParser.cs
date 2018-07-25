using EPPIDataServices.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
    public class FileParser
    {

        private static void ExecuteSqlTransactionBulkActions(ILogger _logger, List<ReferenceRecord> updateCitations)
        {
            using (SqlConnection connection = new SqlConnection(Program.SqlHelper.DataServiceDB))
            {
                connection.Open();

                // Start a local transaction.
                SqlTransaction transaction = connection.BeginTransaction();

                int success = 0;

                SqlParameter RefIDs = new SqlParameter("@RefIDs", String.Join(",", updateCitations.Select(x => x.CitationId).ToList()));

                if (RefIDs.Size != 0)
                {
                    if (updateCitations.Count() > 1000)
                    {
                        for (int i = 0; i <= Math.Floor((double)updateCitations.Count() / 1000); i += 1000)
                        {
                            string tmpStrIDs = String.Join(",", updateCitations.Select(x => x.CitationId).ToList().Skip(i * 1000).Take(1000));
                            SqlParameter tmpRefIDs = new SqlParameter("@RefIDs", tmpStrIDs);
                            success = Program.SqlHelper.ExecuteNonQuerySPWtrans(connection, "st_DeleteReferencesByREFID", transaction, tmpRefIDs);
                        }
                    }
                    else
                    {
                        success = Program.SqlHelper.ExecuteNonQuerySPWtrans(connection, "st_DeleteReferencesByREFID", transaction, RefIDs);
                    }
                }

                try
                {

                    if (success == -1)
                    {
                        List<Author> authors = new List<Author>();
                        List<ExternalID> externals = new List<ExternalID>();

                        int AuthorsCount = 0;
                        int ExternalCount = 0;
                        //Int64 Items_S;
                        Int64 Author_S;
                        Int64 External_S;

                        try
                        {
                            // Authors count required
                            // SHould this be UpdateCitations or Citations
                            foreach (var item in updateCitations)
                            {
                                AuthorsCount += item.Authors.Count();
                                ExternalCount += item.ExternalIDs.Count();
                            }

                            using (SqlCommand cmd = new SqlCommand("st_ReferencesImportPrepare", connection))
                            {
                                //prepare all tables
                                cmd.Transaction = transaction;
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.Add(new SqlParameter("@Items_Number", 0));
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
                                //Items_S = (Int64)cmd.Parameters["@Item_Seed"].Value;
                                Author_S = (Int64)cmd.Parameters["@Author_Seed"].Value;
                                External_S = (Int64)cmd.Parameters["@External_Seed"].Value;

                            }

                            List<DataTable> tables = ReferenceRecord.ToDataTablesUpdate(updateCitations, authors, externals, Author_S, External_S);
                            foreach (DataTable dt in tables)
                            {
                                if (!BulkInsertDataTable(dt, connection, transaction))
                                {
                                    success = -2;
                                }
                            }
                        }
                        catch (SqlException ex)
                        {
                            _logger.Log(LogLevel.Error, ex, "FATAL ERROR: failed to bulk insert updated references.");
                            success = -2;
                        }
                        if (success == -2)
                        {
                            _logger.Log(LogLevel.Error, "Error happened in bulk Update phase: rolling back transaction.");
                            transaction.Rollback();
                        }
                        else
                        {
                            transaction.Commit();
                        }
                    }

                }
                catch (Exception ex)
                {

                    _logger.LogInformation("Commit Exception Type: {0}", ex.GetType());
                    _logger.LogInformation("  Message: {0}", ex.Message);

                    // roll back the transaction.
                    try
                    {
                        transaction.Rollback();
                    }
                    catch (Exception ex2)
                    {
                        _logger.LogInformation("Rollback Exception Type: {0}", ex2.GetType());
                        _logger.LogInformation("  Message: {0}", ex2.Message);
                    }
                }
            }
        }

        private readonly ILogger _logger;

        public FileParser(ILogger<EPPILogger> logger)
        {
            _logger = logger;
        }

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
        public FileParserResult ParseFile(string filepath)
        {
            _logger.LogInformation("Parsing: " + filepath + ".");
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
                _logger.Log(LogLevel.Information, "File contains " + values.Count.ToString() + " records.");
                //Program.Logger.LogMessageLine("File contains " + values.Count.ToString() + " records.");
                result.CitationsInFile = values.Count;
            }
            catch
            {
                result.Success = false;
                _logger.Log(LogLevel.Information, "Error parsing file: no citation processed.");
                //Program.Logger.LogMessageLine("Error parsing file: no citation processed.");
                result.Messages.Add("Error parsing file: no citation processed.");
                DeleteParsedFile(filepath);
                return (result);
            }

            if (Program.maxCount != int.MaxValue)
            {
                _logger.Log(LogLevel.Information, "Limiting import to: " + Program.maxCount.ToString() + " references.");
                //                Program.Logger.LogMessageLine("Limiting import to: " + Program.maxCount.ToString() + " references.");
                result.Messages.Add("Limiting import to: " + Program.maxCount.ToString() + " references.");
            }
            Citations = new List<ReferenceRecord>();
            foreach (XElement xCit in values)
            {
                //_logger.Log(LogLevel.Information, "Processing record: " + (Citations.Count + 1).ToString() + ".");
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
                    _logger.Log(LogLevel.Error, e.Message);
                    //                    Program.Logger.LogMessageLine(e.Message);
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
            _logger.Log(LogLevel.Information, tmpMsg);
            //            Program.Logger.LogMessageLine(tmpMsg);
            result.Messages.Add(tmpMsg);
            DateTime now = DateTime.Now;
            _logger.Log(LogLevel.Information, "Finding references that need updating & updating them.");
            //            Program.Logger.LogMessageLine("Finding references that need updating & updating them.");
            int i = 0;
            while (i < Citations.Count)
            {//first pass, see which ones are updates, we'll bulk update them later on.
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
                            {   //newrecord is older, won't save parsed version
                                _logger.Log(LogLevel.Information, "Match in DB: " + pmid + ". Citation will be not be updated (DB record is newer).");
                                //Program.Logger.LogMessageLine("Match in DB: " + pmid + ". Citation will be not be updated (DB record is newer).");
                            }
                        }
                        else
                        {//we don't have the dates, we'll update, just in case.
                         //this always happens when we're deleting (we don't check version date/number).
                            updateExisting = true;
                            result.Messages.Add("Match in DB (v. date missing): " + pmid + ". Citation will be " + upDelMsg);
                            _logger.Log(LogLevel.Information, "Match in DB (v. date missing): " + pmid + ". Citation will be " + upDelMsg);
                            //                            Program.Logger.LogMessageLine("Match in DB (v. date missing): " + pmid + ". Citation will be " + upDelMsg);
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
                            // need to replace what the line below is doing to a bulk update of some sort
                            // rather than one by one....remove the ;ine below and bulk update a list
                            // of existin citations....

                            // if (!Program.simulate) ExsistingCit.SaveSelf(conn, Program.SqlHelper);
                        }
                        else if (!updateExisting)
                        {//we don't want this reference to be bulk inserted below! Parser ref is older than the one in DB so nothing should change.
                            Citations.Remove(rec);
                        }
                    }
                    else
                    {//existing citation is new/unknown to us.
                        //do nothing
                        i++;
                    }

                }
            }


            if (Program.simulate == false && Program.deleteRecords == false && UpdateCitations.Count > 0)
            {
                ExecuteSqlTransactionBulkActions(_logger, UpdateCitations);
            }
            string savedin = EPPILogger.Duration(now);
            //================================================================
            _logger.Log(LogLevel.Information, "Done updating references in: " + savedin);
            //Program.Logger.LogMessageLine("Done updating references in: " + savedin);
            result.Messages.Add("Done updating references in: " + savedin);
            now = DateTime.Now;
            //the new citations have not been saved, we'd like to do this in bulk, probably.
            _logger.Log(LogLevel.Information, "Done updating references, now saving " + Citations.Count.ToString() + " new citations.");
            //            Program.Logger.LogMessageLine("Done updating references, now saving " + Citations.Count.ToString() + " new citations.");
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

                    }
                    catch (SqlException ex)
                    {
                        _logger.Log(LogLevel.Error, ex, "FATAL ERROR: failed to bulk insert new references.");
                    }
                }
            }
            savedin = EPPILogger.Duration(now);
            _logger.Log(LogLevel.Information, "Saved new references in: " + savedin);

            result.Messages.Add("Saved new references in: " + savedin);
            _logger.Log(LogLevel.Information, "Updated refs (PMIDs): " + result.UpdatedPMIDs);

            DeleteParsedFile(filepath);

            string duration = EPPILogger.Duration(start);
            _logger.Log(LogLevel.Information, "Imported " + Citations.Count.ToString() + " (new) records in " + duration);

            result.Messages.Add("Imported " + Citations.Count.ToString() + " (new) records in " + duration);
            result.EndTime = DateTime.Now;
            if (Citations != null && result.CitationsCommitted > 0 && result.ErrorCount == 0)
            {
                return result;
            }
            else if (Citations != null && result.CitationsCommitted > 0 && result.ErrorCount > 0)
            {
                string tmp = "Non fatal errors count: " + result.ErrorCount.ToString() + ".";
                result.Messages.Add(tmp);
                _logger.Log(LogLevel.Information, tmp);
                return result;
            }
            else
            {
                string tmp = "No citation to import found in file";
                result.Messages.Add(tmp);
                result.Success = false;
                _logger.Log(LogLevel.Information, tmp);
                return result;
            }
        }

        //private void BulkInsertTheUpdates(List<ReferenceRecord> updateCitations, SqlTransaction transaction, SqlConnection conn)
        //{

        //        List<Author> authors = new List<Author>();
        //        List<ExternalID> externals = new List<ExternalID>();

        //        foreach (var item in updateCitations)
        //        {
        //            authors.AddRange(item.Authors);
        //            externals.AddRange(item.ExternalIDs);
        //        }

        //        int AuthorsCount = 0;
        //        int ExternalCount = 0;
        //        Int64 Items_S;
        //        Int64 Author_S;
        //        Int64 External_S;

        //        try
        //        {
        //            // Authors count required
        //            foreach (var item in Citations)
        //            {
        //                AuthorsCount += item.Authors.Count();
        //                ExternalCount += item.ExternalIDs.Count();
        //            }

        //            using (SqlCommand cmd = new SqlCommand("st_ReferencesImportPrepare", conn))
        //            {
        //                //prepare all tables
        //                cmd.Transaction = transaction;
        //                cmd.CommandType = CommandType.StoredProcedure;
        //                cmd.Parameters.Add(new SqlParameter("@Items_Number", Citations.Count));
        //                cmd.Parameters.Add(new SqlParameter("@Authors_Number", AuthorsCount));
        //                cmd.Parameters.Add(new SqlParameter("@Externals_Number", ExternalCount));
        //                cmd.Parameters.Add("@Item_Seed", SqlDbType.BigInt);
        //                cmd.Parameters["@Item_Seed"].Direction = ParameterDirection.Output;
        //                cmd.Parameters.Add("@Author_Seed", SqlDbType.BigInt);
        //                cmd.Parameters["@Author_Seed"].Direction = ParameterDirection.Output;
        //                cmd.Parameters.Add("@External_Seed", SqlDbType.BigInt);
        //                cmd.Parameters["@External_Seed"].Direction = ParameterDirection.Output;
        //                cmd.ExecuteNonQuery();

        //                //get seeds values
        //                Items_S = (Int64)cmd.Parameters["@Item_Seed"].Value;
        //                Author_S = (Int64)cmd.Parameters["@Author_Seed"].Value;
        //                External_S = (Int64)cmd.Parameters["@External_Seed"].Value;

        //            }

        //            var tables = ReferenceRecord.ToDataTablesUpdate(updateCitations, authors, externals, Items_S, External_S);
        //            foreach (DataTable dt in tables)
        //            {
        //                var resBulkInsert = BulkInsertDataTable(dt, conn, transaction);
        //            }
        //        }
        //        catch (SqlException ex)
        //        {
        //            _logger.Log(LogLevel.Error,ex, "FATAL ERROR: failed to bulk insert updated references.");
        //        }
        //}

        //// This method is next
        //private int BulkDeleteExistingCitations(SqlConnection conn, List<ReferenceRecord> updateCitations, SqlTransaction transaction)
        //{
        //        int success = 0;

        //        SqlParameter RefIDs = new SqlParameter("@RefIDs", String.Join(",", updateCitations.Select(x => x.CitationId).ToList()));
        //        try
        //        {
        //                if (RefIDs.Size != 0)
        //                {
        //                    success = Program.SqlHelper.ExecuteNonQuerySPWtrans(conn, "st_DeleteReferencesByREFID", transaction, RefIDs);
        //                }
        //        }
        //        catch (Exception e)
        //        {
        //            _logger.Log(LogLevel.Error, e, "SQL Error");
        //        }

        //    return success;
        //}

        private ReferenceRecord GetReferenceRecordByPMID(SqlConnection conn, string pubmedID)
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
                _logger.Log(LogLevel.Error, e, "Error fetching existing ref and/or creating local object.");
                //                Program.Logger.LogException(e, "Error fetching existing ref and/or creating local object.");
                result.ErrorCount++;
            }
            return res;
        }

        private void DeleteParsedFile(string filepath)
        {
            if (File.Exists(filepath))
            {
                try
                {
                    File.Delete(filepath);
                }
                catch (Exception)
                {
                    _logger.Log(LogLevel.Information, "Warning: could not delete \"" + filepath + "\".");
                    //                    Program.Logger.LogMessageLine("Warning: could not delete \"" + filepath + "\".");
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

        private void AddToImportList(ReferenceRecord curr)
        {
            ExternalID Pmid = curr.ExternalIDByType("pubmed");
            if (Pmid == null)
            {
                result.ErrorCount++;
                result.Messages.Add("!! Error: skipping current citation as it does not have a PMID!");
                _logger.Log(LogLevel.Information, "!! Error: skipping current citation as it does not have a PMID!");
                //                Program.Logger.LogMessageLine("!! Error: skipping current citation as it does not have a PMID!");
                return;
            }
            foreach (ReferenceRecord cit in Citations)
            {
                //ExternalID oldPmid = cit.ExternalIDByType("pubmed");
                //if (oldPmid == Pmid)
                if (cit.ExternalIDs.Contains(Pmid))
                {//we have already processed this citation(!), we'll update it
                    _logger.Log(LogLevel.Information, "Internal Match: " + Pmid.Value);
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
