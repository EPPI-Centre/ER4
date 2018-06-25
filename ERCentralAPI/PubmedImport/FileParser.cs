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

        //public static void LoadCitationIdentities(List<SQLCitationObject> Citations, long Items_S,
        //        SqlConnection conn, SqlTransaction tran)
        //{

        //    SqlCommand cmd = new SqlCommand(
        //        "SELECT [REFERENCE_ID] " +
        //        "FROM [DataService].[dbo].[TB_REFERENCE] " +
        //        "WHERE [REFERENCE_ID] >= " + Items_S
        //         , conn, tran);

        //    SQLCitationObject[] citations =
        //        (from u in Citations
        //         orderby u.REFERENCE_ID
        //         select u).ToArray();

        //    using (SqlDataReader reader = cmd.ExecuteReader())
        //    {
        //        int index = 0;
        //        while (reader.Read())
        //        {
        //            Int64 id = (Int64)reader[0];

        //            foreach (var au in citations[index].Authors)
        //                au.REFERENCE_ID = id;
        //            citations[index++].REFERENCE_ID = (int)id;
        //        }
        //    }

        //}

        //public static int InitializeBulkInsertSession(SqlConnection conn, SqlTransaction tran)
        //{
        //    SqlCommand cmd = new SqlCommand(
        //        "INSERT INTO [DataService].[dbo].[BulkInsertSession]([TsCreated]) VALUES (CURRENT_TIMESTAMP)", conn, tran);

        //    cmd.ExecuteNonQuery();

        //    cmd = new SqlCommand(
        //        "SELECT [BulkInsertSessionID] FROM [DataService].[dbo].[BulkInsertSession] " +
        //        "WHERE @@ROWCOUNT > 0 and [BulkInsertSessionID] = SCOPE_IDENTITY()", conn, tran);

        //    int bulkInsertSessionId = (int)cmd.ExecuteScalar();

        //    return bulkInsertSessionId;

        //}

        //public static void CompleteBulkInsertSession(int bulkInsertSessionId,
        //           SqlConnection conn, SqlTransaction tran)
        //{

        //    SqlCommand cmd = new SqlCommand(
        //        "UPDATE [DataService].[dbo].[TB_REFERENCE] " +
        //        "SET [BulkInsertSessionID] = NULL " +
        //        "WHERE [BulkInsertSessionID] = @bulkInsertSessionId", conn, tran);
        //    cmd.Parameters.Add(new SqlParameter("@bulkInsertSessionId", bulkInsertSessionId));
        //    cmd.ExecuteNonQuery();

        //    cmd = new SqlCommand(
        //        "DELETE FROM [DataService].[dbo].[BulkInsertSession] " +
        //        "WHERE [BulkInsertSessionID] = @bulkInsertSessionId", conn, tran);
        //    cmd.Parameters.Add(new SqlParameter("@bulkInsertSessionId", bulkInsertSessionId));
        //    cmd.ExecuteNonQuery();

        //}

        //public static void BulkInsertExternalIDS(List<SQLCitationObject> lstCits, long External_S, SqlConnection conn, SqlTransaction tran)
        //{

        //    List<SQLExternal> lstExternalIDS = new List<SQLExternal>();

        //    foreach (var item in lstCits)
        //    {
        //        lstExternalIDS.AddRange(ConvertExternalToSQLObject(item.ExternalIDs, item.REFERENCE_ID));
        //    }

        //    DataTable table = ListToDataTable(lstExternalIDS);

        //    table.Columns["EXTERNALID_ID"].AutoIncrement = true;
        //    table.Columns["EXTERNALID_ID"].AutoIncrementSeed = External_S + 1;
        //    table.Columns["EXTERNALID_ID"].AutoIncrementStep = 1;

        //    using (SqlBulkCopy bulkCopy =
        //            new SqlBulkCopy(conn, SqlBulkCopyOptions.Default, tran))
        //    {

        //        bulkCopy.DestinationTableName = "[DataService].[dbo].[TB_EXTERNALID]";

        //        bulkCopy.ColumnMappings.Clear();
        //        bulkCopy.ColumnMappings.Add("EXTERNALID_ID", "EXTERNALID_ID");
        //        bulkCopy.ColumnMappings.Add("REFERENCE_ID", "REFERENCE_ID");
        //        bulkCopy.ColumnMappings.Add("TYPE", "TYPE");
        //        bulkCopy.ColumnMappings.Add("VALUE", "VALUE");

        //        bulkCopy.BulkCopyTimeout = 1000;

        //        bulkCopy.WriteToServer(table);

        //    }
           
        //}


        //public static void BulkInsertFlatAuthors(List<SQLCitationObject> Citations, Int64 Author_S,
        //                SqlConnection conn, SqlTransaction tran)
        //{

        //    List<SQLAuthorObject> lstAuths = new List<SQLAuthorObject>();

        //    foreach (var item in Citations)
        //    {
        //        lstAuths.AddRange(item.Authors);
        //    }

        //    DataTable table = ListToDataTable(lstAuths);

        //    table.Columns["REFERENCE_AUTHOR_ID"].AutoIncrement = true;
        //    table.Columns["REFERENCE_AUTHOR_ID"].AutoIncrementSeed = Author_S + 1;
        //    table.Columns["REFERENCE_AUTHOR_ID"].AutoIncrementStep = 1;


        //    using (SqlBulkCopy bulkCopy =
        //            new SqlBulkCopy(conn, SqlBulkCopyOptions.Default , tran))
        //    {

        //        bulkCopy.DestinationTableName = "[DataService].[dbo].[TB_REFERENCE_AUTHOR]";

        //        bulkCopy.ColumnMappings.Clear();
        //        bulkCopy.ColumnMappings.Add("REFERENCE_AUTHOR_ID", "REFERENCE_AUTHOR_ID");
        //        bulkCopy.ColumnMappings.Add("REFERENCE_ID", "REFERENCE_ID");
        //        bulkCopy.ColumnMappings.Add("LAST", "LAST");
        //        bulkCopy.ColumnMappings.Add("FIRST", "FIRST");
        //        bulkCopy.ColumnMappings.Add("ROLE", "ROLE");
        //        bulkCopy.ColumnMappings.Add("RANK", "RANK");

        //        bulkCopy.BulkCopyTimeout = 1000;

        //        bulkCopy.WriteToServer(table);

        //    }

        //     //LoadAuthorsIdentities(Citations, Author_S, conn, tran);

        //}

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

                    //bulkCopy.ColumnMappings.Add("REFERENCE_ID", "REFERENCE_ID");
                    //bulkCopy.ColumnMappings.Add("TYPE_ID", "TYPE_ID");
                    //bulkCopy.ColumnMappings.Add("TITLE", "TITLE");
                    //bulkCopy.ColumnMappings.Add("PARENT_TITLE", "PARENT_TITLE");
                    //bulkCopy.ColumnMappings.Add("SHORT_TITLE", "SHORT_TITLE");
                    //bulkCopy.ColumnMappings.Add("DATE_CREATED", "DATE_CREATED");
                    //bulkCopy.ColumnMappings.Add("CREATED_BY", "CREATED_BY");
                    //bulkCopy.ColumnMappings.Add("DATE_EDITED", "DATE_EDITED");
                    //bulkCopy.ColumnMappings.Add("EDITED_BY", "EDITED_BY");
                    //bulkCopy.ColumnMappings.Add("PUBMED_REVISED", "PUBMED_REVISED");
                    //bulkCopy.ColumnMappings.Add("PUBMED_PMID_VERSION", "PUBMED_PMID_VERSION");
                    //bulkCopy.ColumnMappings.Add("YEAR", "YEAR");
                    //bulkCopy.ColumnMappings.Add("MONTH", "MONTH");
                    //bulkCopy.ColumnMappings.Add("STANDARD_NUMBER", "STANDARD_NUMBER");
                    //bulkCopy.ColumnMappings.Add("CITY", "CITY");
                    //bulkCopy.ColumnMappings.Add("COUNTRY", "COUNTRY");
                    //bulkCopy.ColumnMappings.Add("PUBLISHER", "PUBLISHER");
                    //bulkCopy.ColumnMappings.Add("INSTITUTION", "INSTITUTION");
                    //bulkCopy.ColumnMappings.Add("VOLUME", "VOLUME");
                    //bulkCopy.ColumnMappings.Add("PAGES", "PAGES");
                    //bulkCopy.ColumnMappings.Add("EDITION", "EDITION");
                    //bulkCopy.ColumnMappings.Add("ISSUE", "ISSUE");
                    //bulkCopy.ColumnMappings.Add("URLS", "URLS");
                    //bulkCopy.ColumnMappings.Add("ABSTRACT", "ABSTRACT");
                    //bulkCopy.ColumnMappings.Add("COMMENTS", "COMMENTS");
                    //bulkCopy.ColumnMappings.Add("MESHTERMS", "MESHTERMS");
                    //bulkCopy.ColumnMappings.Add("KEYWORDS", "KEYWORDS");

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


        //public static List<SQLCitationObject> BulkInsertFlatReferences(List<CitationRecord> Citations, long Items_S,
        //       SqlConnection conn, SqlTransaction tran)
        //{

        //    List<Author> authors = new List<Author>();
        //    List<SQLCitationObject> lstObjs = new List<SQLCitationObject>();
        //    // Convert Citation object into sqlObject
        //    foreach (CitationRecord rec in Citations)
        //    {
        //        lstObjs.Add(ConvertCitationToSQLObject(rec));
        //    }

        //    DataTable table = ListToDataTable(lstObjs);

        //    table.Columns["REFERENCE_ID"].AutoIncrement = true;
        //    table.Columns["REFERENCE_ID"].AutoIncrementSeed = Items_S + 1;
        //    table.Columns["REFERENCE_ID"].AutoIncrementStep = 1;

        //    using (SqlBulkCopy bulkCopy =
        //            new SqlBulkCopy(conn, SqlBulkCopyOptions.Default, tran))
        //    {
        //        bulkCopy.DestinationTableName = "[DataService].[dbo].[TB_REFERENCE]";
        //        bulkCopy.ColumnMappings.Clear();

        //        bulkCopy.ColumnMappings.Add("REFERENCE_ID", "REFERENCE_ID");
        //        bulkCopy.ColumnMappings.Add("TYPE_ID", "TYPE_ID");
        //        bulkCopy.ColumnMappings.Add("TITLE", "TITLE");
        //        bulkCopy.ColumnMappings.Add("PARENT_TITLE", "PARENT_TITLE");
        //        bulkCopy.ColumnMappings.Add("SHORT_TITLE", "SHORT_TITLE");
        //        bulkCopy.ColumnMappings.Add("DATE_CREATED", "DATE_CREATED");
        //        bulkCopy.ColumnMappings.Add("CREATED_BY", "CREATED_BY");
        //        bulkCopy.ColumnMappings.Add("DATE_EDITED", "DATE_EDITED");
        //        bulkCopy.ColumnMappings.Add("EDITED_BY", "EDITED_BY");
        //        bulkCopy.ColumnMappings.Add("PUBMED_REVISED", "PUBMED_REVISED");
        //        bulkCopy.ColumnMappings.Add("PUBMED_PMID_VERSION", "PUBMED_PMID_VERSION");
        //        bulkCopy.ColumnMappings.Add("YEAR", "YEAR");
        //        bulkCopy.ColumnMappings.Add("MONTH", "MONTH");
        //        bulkCopy.ColumnMappings.Add("STANDARD_NUMBER", "STANDARD_NUMBER");
        //        bulkCopy.ColumnMappings.Add("CITY", "CITY");
        //        bulkCopy.ColumnMappings.Add("COUNTRY", "COUNTRY");
        //        bulkCopy.ColumnMappings.Add("PUBLISHER", "PUBLISHER");
        //        bulkCopy.ColumnMappings.Add("INSTITUTION", "INSTITUTION");
        //        bulkCopy.ColumnMappings.Add("VOLUME", "VOLUME");
        //        bulkCopy.ColumnMappings.Add("PAGES", "PAGES");
        //        bulkCopy.ColumnMappings.Add("EDITION", "EDITION");
        //        bulkCopy.ColumnMappings.Add("ISSUE", "ISSUE");
        //        bulkCopy.ColumnMappings.Add("URLS", "URLS");
        //        bulkCopy.ColumnMappings.Add("ABSTRACT", "ABSTRACT");
        //        bulkCopy.ColumnMappings.Add("COMMENTS", "COMMENTS");
        //        bulkCopy.ColumnMappings.Add("MESHTERMS", "MESHTERMS");
        //        bulkCopy.ColumnMappings.Add("KEYWORDS", "KEYWORDS");
        //        bulkCopy.BulkCopyTimeout = 1000;

        //        bulkCopy.WriteToServer(table);

        //    }

        //     LoadCitationIdentities(lstObjs, Items_S, conn, tran);

        //    return lstObjs;
        //}


        //public static void LoadAuthorsIdentities(List<SQLCitationObject> Citations, long Author_S,
        //        SqlConnection conn, SqlTransaction tran)
        //{

        //    SqlCommand cmd = new SqlCommand(
        //        "SELECT [REFERENCE_AUTHOR_ID] " +
        //        "FROM [DataService].[dbo].[TB_REFERENCE] INNER JOIN " +
        //        " [DataService].[dbo].[TB_REFERENCE_AUTHOR] ON [DataService].[dbo].[TB_REFERENCE].[REFERENCE_ID] = [DataService].[dbo].[TB_REFERENCE_AUTHOR].[REFERENCE_ID]  " +
        //        "WHERE [DataService].[dbo].[TB_REFERENCE].[REFERENCE_AUTHOR_ID] >= " + Author_S +
        //        "ORDER BY [REFERENCE_AUTHOR_ID] ASC", conn, tran);

        //    SQLAuthorObject[] sortedAuthors =
        //        (from u in Citations
        //         from e in u.Authors
        //         select e).ToArray();


        //    //using (SqlDataReader reader = cmd.ExecuteReader())
        //    //{
        //    //    int index = 0;
        //    //    while (reader.Read())
        //    //        sortedAuthors[index++].REFERENCE_AUTHOR_ID = (int)reader[0];
        //    //}

        //}

        //public class SQLAuthorObject
        //{
        //    public Int64 REFERENCE_AUTHOR_ID { get; set; }
        //    public Int64 REFERENCE_ID { get; set; }
        //    public string LAST { get; set; }
        //    public string FIRST { get; set; }
        //    public int ROLE { get; set; }
        //    public int RANK { get; set; }

        //    public override bool Equals(object obj)
        //    {
        //        SQLAuthorObject author = obj as SQLAuthorObject;
        //        if (author == null) return false;
        //        return author.FIRST == this.FIRST && author.LAST == this.LAST && author.REFERENCE_ID == this.REFERENCE_ID;
        //    }

        //    public override int GetHashCode()
        //    {
        //        return  this.FIRST.GetHashCode() + this.LAST.GetHashCode() + this.REFERENCE_ID.GetHashCode() ;
        //    }
        //}

        //public class SQLExternal
        //{
        //    public Int64 EXTERNALID_ID { get; set; }
        //    public Int64 REFERENCE_ID { get; set; }
        //    public string TYPE { get; set; }
        //    public string VALUE { get; set; }
        //}

        //public class SQLCitationObject
        //{
        //    public int REFERENCE_ID { get; set; }
        //    public int TYPE_ID { get; set; }
        //    public string TITLE { get; set; }
        //    public string PARENT_TITLE { get; set; }
        //    public string SHORT_TITLE { get; set; }
        //    public DateTime DATE_CREATED { get; set; }
        //    public string CREATED_BY { get; set; }
        //    public DateTime DATE_EDITED { get; set; }
        //    public string EDITED_BY { get; set; }
        //    public DateTime PUBMED_REVISED { get; set; }
        //    public int PUBMED_PMID_VERSION { get; set; }
        //    public char[] YEAR { get; set; }
        //    public char[] MONTH { get; set; }
        //    public string STANDARD_NUMBER { get; set; }
        //    public string CITY { get; set; }
        //    public string COUNTRY { get; set; }
        //    public string PUBLISHER { get; set; }
        //    public string INSTITUTION { get; set; }
        //    public string VOLUME { get; set; }
        //    public string PAGES { get; set; }
        //    public string EDITION { get; set; }
        //    public string ISSUE { get; set; }
        //    public string URLS { get; set; }
        //    public string ABSTRACT { get; set; }
        //    public string COMMENTS { get; set; }
        //    public string MESHTERMS { get; set; }
        //    public string KEYWORDS { get; set; }
        //    public List<SQLAuthorObject> Authors { get; set; }
        //    public List<ExternalID> ExternalIDs { get; set; }

        //}

        public static string GetKeywordsString(CitationRecord rec)
        {
            string res = "";
            foreach (KeywordObject Keyw in rec.Keywords)
            {
                res += ((Keyw.Major) ? "*" : "") + Keyw.Name + Environment.NewLine;
            }
            return res;
        }

        //public static List<SQLAuthorObject> ConvertAuthorToSQLObject(List<Author> auths, long REFERENCE_ID)
        //{
      
        //    List<SQLAuthorObject> authors = new List<SQLAuthorObject>();
        //    foreach (var auth in auths)
        //    {
        //        SQLAuthorObject author = new SQLAuthorObject();
        //        author.REFERENCE_AUTHOR_ID = 0;
        //        author.REFERENCE_ID = REFERENCE_ID;
        //        author.LAST = auth.FamilyName;
        //        author.FIRST = auth.GivenName;
        //        author.ROLE = auth.AuthorshipLevel;
        //        author.RANK = auth.AuthorshipLevel;
        //        authors.Add(author);
        //    }
           
        //    return authors;
        //}

        //public static List<SQLExternal> ConvertExternalToSQLObject(List<ExternalID> externals, int REFERENCE_ID)
        //{
        //    List<SQLExternal> exts = new List<SQLExternal>();
        //    foreach (var ext in externals)
        //    {
        //        SQLExternal outExt = new SQLExternal();
        //        outExt.EXTERNALID_ID = 0;
        //        outExt.REFERENCE_ID = REFERENCE_ID;
        //        outExt.TYPE = ext.Name;
        //        outExt.VALUE = ext.Value;

        //        exts.Add(outExt);
        //    }

        //    return exts;
        //}

        //public static SQLCitationObject ConvertCitationToSQLObject(CitationRecord rec)
        //{

        //    SQLCitationObject convertedObject = new SQLCitationObject();
        //    convertedObject.ABSTRACT = rec.Abstract.Trim();
        //    convertedObject.CITY = rec.City.Trim();
        //    convertedObject.COMMENTS = "";
        //    convertedObject.COUNTRY = rec.Country.Trim();
        //    convertedObject.CREATED_BY = "";
        //    convertedObject.DATE_CREATED = DateTime.Now;
        //    convertedObject.DATE_EDITED = DateTime.Now;
        //    convertedObject.EDITED_BY = "";
        //    convertedObject.EDITION = rec.Edition.Trim();
        //    convertedObject.INSTITUTION = "";
        //    convertedObject.ISSUE = rec.Issue.Trim();
        //    convertedObject.KEYWORDS = GetKeywordsString(rec);
        //    convertedObject.MESHTERMS = "";
        //    convertedObject.MONTH = new char[1]; // rec.Month.ToCharArray();
        //    convertedObject.PAGES = rec.Pages.Trim();
        //    convertedObject.PARENT_TITLE = rec.ParentTitle.Trim();
        //    convertedObject.PUBLISHER = rec.Publisher.Trim();
        //    convertedObject.PUBMED_PMID_VERSION = rec.PubmedPmidVersion;
        //    convertedObject.PUBMED_REVISED = DateTime.Now;
        //    convertedObject.SHORT_TITLE = rec.ShortTitle.Trim();
        //    convertedObject.STANDARD_NUMBER = rec.Issn.Trim();
        //    convertedObject.TITLE = rec.Title.Trim();
        //    convertedObject.TYPE_ID = rec.TypeID;
        //    convertedObject.URLS = string.Join("¬", rec.Urls.ToArray());
        //    convertedObject.VOLUME = rec.Volume.Trim();
        //    convertedObject.YEAR = rec.PublicationYear.ToCharArray();
        //    List<SQLAuthorObject> auths = new List<SQLAuthorObject>();
        //    convertedObject.Authors = ConvertAuthorToSQLObject(rec.Authors, convertedObject.REFERENCE_ID);
        //    convertedObject.ExternalIDs = rec.ExternalIDs;

        //    return convertedObject;
        //}
      
        //public static DataTable ListToDataTable<T>(IList<T> data)
        //    {
        //    DataTable table = new DataTable();

        //    //special handling for value types and string
        //    if (typeof(T).IsValueType || typeof(T).Equals(typeof(string)))
        //    {

        //        DataColumn dc = new DataColumn("Value");
        //        table.Columns.Add(dc);
        //        foreach (T item in data)
        //        {
        //            DataRow dr = table.NewRow();
        //            dr[0] = item;
        //            table.Rows.Add(dr);
        //        }
        //    }
        //    else
        //    {
        //        PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(T));
        //        foreach (PropertyDescriptor prop in properties)
        //        {
        //                table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
        //        }
               
        //        foreach (T item in data)
        //        {
        //            DataRow row = table.NewRow();
        //            foreach (PropertyDescriptor prop in properties)
        //            {

        //                    try
        //                    {
        //                        row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
        //                    }
        //                    catch (Exception ex)
        //                    {
        //                        row[prop.Name] = DBNull.Value;
        //                    }
        //            }
        //            table.Rows.Add(row);

        //        }
        //    }
        //    return table;
        //}

        static List<CitationRecord> Citations = new List<CitationRecord>();
        static List<CitationRecord> UpdateCitations = new List<CitationRecord>();
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
            Citations = new List<CitationRecord>();
            foreach (XElement xCit in values)
            {
                //Program.Logger.LogMessageLine("Processing record: " + (Citations.Count + 1).ToString() + ".");
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

                        var tables = CitationRecord.ToDataTables(Citations, Items_S, External_S, Author_S);
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



        private static CitationRecord GetCitationRecordByPMID(SqlConnection conn, string pubmedID)
        {
            CitationRecord res = null;
            SqlParameter extName = new SqlParameter("@ExternalIDName", "pubmed");
            SqlParameter pmid = new SqlParameter("@ExternalIDValue", pubmedID);
            try
            {
                using (SqlDataReader reader = Program.SqlHelper.ExecuteQuerySP(conn, "st_FindCitationByExternalID", extName, pmid))
                {
                    if (reader.Read()) res = CitationRecord.GetCitationRecord(reader);
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
				Program.Logger.LogMessageLine("!! Error: skipping current citation as it does not have a PMID!");
				return;
			}
			foreach (CitationRecord cit in Citations)
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
