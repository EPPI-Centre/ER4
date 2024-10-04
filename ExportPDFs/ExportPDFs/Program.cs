using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.IO;
using Microsoft.Extensions.Logging;
using System.Data.SqlClient;
using EPPIDataServices.Helpers;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using System.Text.RegularExpressions;

namespace ExportPDFs
{
    class Program
    {
        private static int RevId;
        private static string DoWhat = "export";
        private static string InclExcl = "incl";
        private static long OnlyThisCode = -1;
        private static bool WhatIf = false;
        private static List<Int64> ItemIDs = new List<long>();
        private static bool exportTXT = false;
        private static bool exportBin = false;
        private static ILogger<Program> _logger = null;
        private static SQLHelper SqlHelper = null;
        private static List<long> AttIDs = new List<long>();
        private static Dictionary<long, string> Columns = new Dictionary<long, string>();
        private static Dictionary<long, List<long>> ItemAtts = new Dictionary<long, List<long>>();

        //EXAMPLE args for exporting data: revid:99 exportbin exporttxt columns:58789,58790,58813,58814,58815,65127,65137,65166
        //EXAMPLE args re-extracting text: revid:7 rebuildtext whatif
        //EXAMPLE args for exporting data: revid:501 exportbin exporttxt columns:95707,95708,95712,95709,95710,95711,95695,95696,95697,95698,115357,95691,95692,95693,95713

        static void Main(string[] args)
        {
            CreateLogFileName();//for some reason, when file does not exist, it gets created but does not write to it, so we create it first!
            // Required for SERILOG
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File(CreateLogFileName())
                .CreateLogger();
            ServiceCollection serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            var serviceProvider = serviceCollection.BuildServiceProvider();
            serviceProvider.GetService<ILogger<Program>>();
            _logger = serviceProvider.GetService<ILogger<Program>>();
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            IConfigurationRoot configuration = builder.Build();
            SqlHelper = new SQLHelper(configuration, _logger);
            Log.Information(".");
            Log.Information(".");
            Log.Information("PDF Export started!");
            foreach (string s in args)
            {//main block to decide what to do!
                if (s.Length > 6 && s.Substring(0, 6).ToLower() == "revid:")
                {
                    if (!int.TryParse(s.Substring(6).Trim(), out RevId))
                    {
                        Log.Fatal("Could not find the review ID parameter, aborting.");
                        return;
                    }//
                }
                else if (s.Length == 9 && s.ToLower() == "exportbin")
                {
                    exportBin = true;
                }
                else if (s.Length == 4 && s.ToLower() == "incl")
                {
                    InclExcl = "incl";
                }
                else if (s.Length == 4 && s.ToLower() == "excl")
                {
                    InclExcl = "excl";
                }
                else if (s.Length == 4 && s.ToLower() == "both")
                {
                    InclExcl = "both";
                }
                else if (s.Length == 9 && s.ToLower() == "exporttxt")
                {
                    exportTXT = true;
                }
                else if (s.Length == 11 && s.ToLower() == "rebuildtext")
                {
                    Program.DoWhat = "rebuildtext";
                }
                else if (s.Length == 6 && s.ToLower() == "whatif")
                {
                    Program.WhatIf = true;
                }
                else if (s.Length > 8 && s.Substring(0, 8).ToLower() == "columns:")
                {
                    string[] aIds = s.Substring(8).Split(',');
                    foreach (string IDst in aIds)
                    {
                        long ID;
                        if (long.TryParse(IDst, out ID))
                        {
                            if (!AttIDs.Contains(ID)) AttIDs.Add(ID);
                        }
                    }
                }
                else if (s.Length > 13 && s.Substring(0, 13).ToLower() == "onlythiscode:")
                {
                    string aIdst = s.Substring(13);
                    if (!long.TryParse(aIdst, out OnlyThisCode))
                    {
                        Log.Fatal("'Onlythiscode' parameter did not parse: aborting.");
                        return;
                    }
                }
                else
                {
                    Log.Fatal("Unrecognised parameter: aborting.");
                    return;
                }
            }
            //block deciding what main branch to use
            if (Program.DoWhat == "export")
            {
                Log.Information("Starting Export. RevId:" + RevId);
                GetAttributes();
                GetItemIds();
                GetItemsAttribute();
                ProduceOutput();
            }
            else if (Program.DoWhat == "rebuildtext")
            {
                Log.Information("Starting re-extraction of full-text from docs. RevId:" + RevId);
                GetItemIdsWithTextToRebuild();
                if (ItemIDs.Count > 0) ExtractTextFromItems();
            }
        }
        private static void GetItemIds()
        {
            string que = "";
            if (OnlyThisCode > 0)
            {//filtering for only this code
                switch (InclExcl)
                {
                    case "incl":
                        que = @"SELECT distinct ir.ITEM_ID from TB_ITEM_REVIEW ir 
                                inner join TB_ITEM_SET tis on tis.ITEM_ID = ir.ITEM_ID and tis.IS_COMPLETED = 1
                                inner join tb_attribute_set tas on tas.SET_ID = tis.SET_ID and tas.ATTRIBUTE_ID = " + OnlyThisCode
                            + @"inner join TB_ITEM_ATTRIBUTE ia on tas.ATTRIBUTE_ID = ia.ATTRIBUTE_ID and ia.ITEM_SET_ID = tis.ITEM_SET_ID
                                    where REVIEW_ID = " + RevId + " AND IS_INCLUDED = 1 and IS_DELETED = 0 and MASTER_ITEM_ID is null";
                        break;
                    case "excl":
                        que = @"SELECT distinct ir.ITEM_ID from TB_ITEM_REVIEW ir 
                                inner join TB_ITEM_SET tis on tis.ITEM_ID = ir.ITEM_ID and tis.IS_COMPLETED = 1
                                inner join tb_attribute_set tas on tas.SET_ID = tis.SET_ID and tas.ATTRIBUTE_ID = " + OnlyThisCode
                            + @"inner join TB_ITEM_ATTRIBUTE ia on tas.ATTRIBUTE_ID = ia.ATTRIBUTE_ID and ia.ITEM_SET_ID = tis.ITEM_SET_ID
                                    where REVIEW_ID = " + RevId + " AND IS_INCLUDED = 0 and IS_DELETED = 0 and MASTER_ITEM_ID is null";
                        break;
                    case "both":
                        que = @"SELECT distinct ir.ITEM_ID from TB_ITEM_REVIEW ir 
                                inner join TB_ITEM_SET tis on tis.ITEM_ID = ir.ITEM_ID and tis.IS_COMPLETED = 1
                                inner join tb_attribute_set tas on tas.SET_ID = tis.SET_ID and tas.ATTRIBUTE_ID = " + OnlyThisCode
                            + @"inner join TB_ITEM_ATTRIBUTE ia on tas.ATTRIBUTE_ID = ia.ATTRIBUTE_ID and ia.ITEM_SET_ID = tis.ITEM_SET_ID
                                    where REVIEW_ID = " + RevId + " AND IS_DELETED = 0 and MASTER_ITEM_ID is null";
                        break;
                }
            }
            else
            {//filtering only on I/E/both not on "with this code".
                switch (InclExcl)
                {
                    case "incl":
                        que = "SELECT ITEM_ID from TB_ITEM_REVIEW where REVIEW_ID = " + RevId + " AND IS_INCLUDED = 1 and IS_DELETED = 0 and MASTER_ITEM_ID is null";
                        break;
                    case "excl":
                        que = "SELECT ITEM_ID from TB_ITEM_REVIEW where REVIEW_ID = " + RevId + " AND IS_INCLUDED = 0 and IS_DELETED = 0 and MASTER_ITEM_ID is null";
                        break;
                    case "both":
                        que = "SELECT ITEM_ID from TB_ITEM_REVIEW where REVIEW_ID = " + RevId + " AND IS_DELETED = 0 and MASTER_ITEM_ID is null";
                        break;
                }
            }
            using (SqlConnection conn = new SqlConnection(Program.SqlHelper.ER4DB))
            {
                using (SqlDataReader reader = SqlHelper.ExecuteQueryNonSP(conn, que))
                {
                    while (reader.Read())// && ItemIDs.Count < 5000)
                    {
                        long id = (Int64)reader["ITEM_ID"];
                        if (!ItemIDs.Contains(id)) ItemIDs.Add(id);
                    }
                }
            }
            Log.Information("Found " + ItemIDs.Count + " items with docs.");
        }
        private static void GetItemIdsWithTextToRebuild()
        {
            string que = @"declare @ids table (ITEM_ID bigint)" + Environment.NewLine
                        + @"insert into @ids select distinct r.item_id from  TB_ITEM_DOCUMENT d
                            inner join TB_ITEM_REVIEW r on d.ITEM_ID = r.ITEM_ID 
	                            AND DOCUMENT_EXTENSION = '.pdf' 
								AND REVIEW_ID = " + RevId + Environment.NewLine
                                +
                                @"Select distinct(r.ITEM_ID)  from TB_ITEM_DOCUMENT d
                            inner join @ids r on d.ITEM_ID = r.ITEM_ID and d.DOCUMENT_TEXT = 'Error: could not find/load an appropriate filter!'";

                //"Select distinct(r.ITEM_ID) from TB_ITEM_DOCUMENT d
                //            inner join TB_ITEM_REVIEW r on d.ITEM_ID = r.ITEM_ID and d.DOCUMENT_TEXT = 'Error: could not find/load an appropriate filter!'
	               //             AND DOCUMENT_EXTENSION = '.pdf' AND REVIEW_ID = " + RevId;// + " AND IS_INCLUDED = 1 and IS_DELETED = 0 and MASTER_ITEM_ID is null";
            using (SqlConnection conn = new SqlConnection(Program.SqlHelper.ER4DB))
            {
                using (SqlDataReader reader = SqlHelper.ExecuteQueryNonSP(conn, que))
                {
                    while (reader.Read())// && ItemIDs.Count < 5000)
                    {
                        long id = (Int64)reader["ITEM_ID"];
                        if (!ItemIDs.Contains(id)) ItemIDs.Add(id);
                    }
                }
            }
            Log.Information("Found " + ItemIDs.Count + " items with docs to extract.");
        }
        private static void ProduceOutput()
        {
            DirectoryInfo FilesDir = System.IO.Directory.CreateDirectory("Files");
            List<ShortDoc> AllDocs = new List<ShortDoc>();
            string firstRow = "ID\tTitle\tAbstract\tAuthors\tJournal\tShortTitle\tType\tYear\tmonth\t"
                + "ISSN/ISBN\tCity\tCountry\tPublisher\tInstitution\tVolume\tPages\tEdition\tIssue\tURL\tDOI\tKeywords\tIncluded\tHas_Documents";
            foreach(KeyValuePair<long,string> kvp in Columns)
            {
                firstRow += "	" + kvp.Value;
            }
            firstRow += Environment.NewLine;
            string SummaryFileN = FilesDir.FullName + @"\" + "summary.csv";
            System.IO.File.WriteAllText(SummaryFileN , firstRow);
            using (SqlConnection conn = new SqlConnection(Program.SqlHelper.ER4DB))
            {
                Log.Information("processing " + ItemIDs.Count + " items");
                foreach (long itemId in ItemIDs)
                {
                    Console.Write('.');
                    bool hasdocs = false;
                    SqlParameter par = new SqlParameter("@ITEM_ID", itemId);
                    using (SqlDataReader reader = SqlHelper.ExecuteQuerySP(conn, "st_ItemDocumentList", par))
                    {
                        while (reader.Read())
                        {
                            hasdocs = true;
                            AllDocs.Add(ShortDoc.GetShortDoc(reader));
                        }
                    }
                    string ItemQue = "Select i.ITEM_ID ,t.TYPE_NAME, dbo.fn_REBUILD_AUTHORS(i.ITEM_ID, 0) as AUTHORS, [TITLE], [PARENT_TITLE], [SHORT_TITLE],"
                        + " [YEAR], [MONTH], [STANDARD_NUMBER], [CITY], [COUNTRY], [PUBLISHER], [INSTITUTION], [VOLUME], [PAGES], [EDITION], [ISSUE], [URL], [ABSTRACT], [DOI], [KEYWORDS], [IS_INCLUDED] as [Included]"
                        + " from tb_ITEM i inner join tb_item_review ir on i.item_id = ir.item_id and ir.Review_id = " + RevId + " AND i.ITEM_ID =" + itemId
                        + " inner join TB_ITEM_TYPE t on i.TYPE_ID = t.TYPE_ID";
                            string summaryline = "";
                    using (SqlDataReader reader = SqlHelper.ExecuteQueryNonSP(conn, ItemQue))
                    {
                        if (reader.Read())
                        {
                            MiniItem miniItem = MiniItem.GetMiniItem(reader);
                            summaryline = miniItem.Id + "	"
                                                + miniItem.Title + "	"
                                                + miniItem.Abstract.Replace('\r', ' ').Replace('\n', ' ').Replace('\t',' ').Replace("  ", " ") + "	"
                                                + miniItem.AUTHORS + "	"
                                                + miniItem.PARENT_TITLE + "	"
                                                + miniItem.SHORT_TITLE + "	"
                                                + miniItem.TYPE_NAME + "	"
                                                + miniItem.Year + "	"
                                                + miniItem.MONTH + "	"
                                                + miniItem.STANDARD_NUMBER + "	"
                                                + miniItem.CITY + "	"
                                                + miniItem.COUNTRY + "	"
                                                + miniItem.PUBLISHER + "	"
                                                + miniItem.INSTITUTION + "	"
                                                + miniItem.VOLUME + "	"
                                                + miniItem.PAGES + "	"
                                                + miniItem.EDITION + "	"
                                                + miniItem.ISSUE + "	"
                                                + miniItem.URL + "	"
                                                + miniItem.DOI + "	"
                                                + miniItem.KEYWORDS.Replace('\r', ' ').Replace('\n', ' ').Replace('\t', ' ').Replace("  ", " ") + "	"
                                                + miniItem.INCLUDED + "	"
                                                + (hasdocs ? 1 : 0) ;
                        }
                        else continue;
                    }
                    foreach (KeyValuePair<long, string> kvp in Columns)
                    {
                        if (ItemAtts.ContainsKey(kvp.Key))
                        { List<long> itemsWithAtt = ItemAtts[kvp.Key];
                            if (itemsWithAtt != null && itemsWithAtt.Contains(itemId)) summaryline += "	1";
                            else summaryline += "	0";
                        }
                        else
                        {
                            summaryline += "	0";
                        }
                    }
                    summaryline += Environment.NewLine;
                    System.IO.File.AppendAllText(SummaryFileN, summaryline);
                }
                Log.Information("Found " + AllDocs.Count + " documents to export.");
                if (exportTXT || exportBin)
                {
                    foreach (ShortDoc doc in AllDocs)
                    {
                        string BaseFilename = FilesDir.FullName + @"\" + doc.itemId + "-" + doc.itemDocumentId;
                        if (exportTXT) System.IO.File.WriteAllText(BaseFilename + ".txt", doc.text);//write the extracted text file...
                        if (exportBin && doc.extension.ToLower() != ".txt")
                        {
                            SqlParameter Docpar = new SqlParameter("@DOC_ID", doc.itemDocumentId);
                            SqlParameter RevIdpar = new SqlParameter("@REV_ID", RevId);
                            using (SqlDataReader reader = SqlHelper.ExecuteQuerySP(conn, "st_ItemDocumentBin", Docpar, RevIdpar))
                            {
                                while (reader.Read())
                                {
                                    File.WriteAllBytes(BaseFilename + doc.extension, (byte[])reader["DOCUMENT_BINARY"]);
                                }
                            }
                        }
                    }
                }
            }

        }
        private static void GetAttributes()
        {
            foreach (long attID in AttIDs)
            {
                string que = "Select ATTRIBUTE_NAME from tb_attribute where attribute_id =" + attID.ToString();
                using (SqlConnection conn = new SqlConnection(Program.SqlHelper.ER4DB))
                {
                    using (SqlDataReader reader = SqlHelper.ExecuteQueryNonSP(conn, que))
                    {
                        if (reader.Read())
                        {
                            if (!Columns.ContainsKey(attID))
                            {
                                Columns.Add(attID, reader["ATTRIBUTE_NAME"].ToString());
                            }
                        }
                    }
                }
            }
        }
        private static void GetItemsAttribute()
        {
            foreach(KeyValuePair<long, string> kvp in Columns)//we know these exist...
            {
                string ItemAttQue = "Select distinct(ir.ITEM_ID) from TB_ITEM_ATTRIBUTE ia" + Environment.NewLine;
                ItemAttQue += "INNER join TB_ITEM_REVIEW ir on ia.ITEM_ID = ir.ITEM_ID and ir.IS_INCLUDED = 1 and ir.IS_DELETED = 0 and ir.REVIEW_ID = " + RevId + Environment.NewLine;
                ItemAttQue += "INNER join TB_ITEM_SET tis on ia.ITEM_SET_ID = tis.ITEM_SET_ID and tis.IS_COMPLETED = 1" + Environment.NewLine;
                ItemAttQue += "WHERE ia.ATTRIBUTE_ID = " + kvp.Key + Environment.NewLine;
                using (SqlConnection conn = new SqlConnection(Program.SqlHelper.ER4DB))
                {
                    using (SqlDataReader reader = SqlHelper.ExecuteQueryNonSP(conn, ItemAttQue))
                    {
                        List<long> ItemIdsWithAtt = new List<long>();
                        while (reader.Read())
                        {
                            ItemIdsWithAtt.Add((Int64)reader["ITEM_ID"]);
                        }
                        if (ItemIdsWithAtt.Count > 0) ItemAtts.Add(kvp.Key, ItemIdsWithAtt);
                    }
                }
            }
        }

        private static void ExtractTextFromItems()
        {
            using (SqlConnection conn = new SqlConnection(Program.SqlHelper.ER4DB))
            {
                using (SqlConnection conn2 = new SqlConnection(Program.SqlHelper.ER4DB))
                {
                    conn2.Open();
                    Log.Information("processing " + ItemIDs.Count + " items");
                    bool StopCondition = false;
                    foreach (long itemId in ItemIDs)
                    {
                        Log.Information("processing item " + itemId);
                        string que = @"Select * from TB_ITEM_DOCUMENT where  DOCUMENT_TEXT = 'Error: could not find/load an appropriate filter!' AND DOCUMENT_EXTENSION = '.pdf' and ITEM_ID =" + itemId;
                        using (SqlDataReader reader = SqlHelper.ExecuteQueryNonSP(conn, que))
                        {
                            while (reader.Read())// && ItemIDs.Count < 5000)
                            {
                                string documentText;
                                EPPIiFilter.FilterResults res = EPPIiFilter.TextFilter.TextFilter1((byte[])reader["DOCUMENT_BINARY"], ".pdf");
                                if (res.ReturnState == "OK")
                                {
                                    documentText = StripIllegalChars(res.SimpleText);
                                    if (documentText != "Error: could not find/load an appropriate filter!")
                                    {
                                        long itemDocID = (long)reader["ITEM_DOCUMENT_ID"];
                                        if (!Program.WhatIf)
                                        {
                                            string updateCmd = @"update TB_ITEM_DOCUMENT set DOCUMENT_TEXT = @text where ITEM_DOCUMENT_ID = " + itemDocID.ToString();
                                            
                                            Log.Information("Updating text for document: " + itemDocID);
                                            //not using SQL helper because we want to use a Query with Parameters
                                            using (SqlCommand cmd = new SqlCommand(updateCmd, conn2))
                                            {
                                                SqlParameter par = new SqlParameter();
                                                par.ParameterName = "@text";
                                                par.Value = documentText;
                                                cmd.Parameters.Add(par);
                                                try
                                                {
                                                    cmd.ExecuteNonQuery();
                                                }
                                                catch (Exception e)
                                                {
                                                    Log.Error(e, "error updating text for doc {0} of item {1}", itemDocID, itemId);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            Log.Information("WHATIF mode - not Updating text for document: " + itemDocID);
                                        }
                                    }
                                    else
                                    {
                                        Log.Error("Text extraction filter failed - Aborting the whole thing.");
                                        StopCondition = true;
                                        break;
                                    }
                                    //return;
                                }
                            }
                        }
                        if (StopCondition)
                        {
                            break;
                        }
                    }
                }
            }
        }
        public static string StripIllegalChars(string text)
        {//from http://codeclarity.blogspot.com/2009/05/c-strip-invalid-xml-10-characters.html
            //text = text.Replace("ﬁ", "fi"); 
            const string illegalXmlChars = @"[\u0000-\u0008]|[\u000B-\u000C]|[\u000E-\u0019]|[\u007F-\u009F]|[\uD800-\uDBFF]|[\uDC00-\uDFFF]";
            //const string illegalXmlChars = @"[\u0000-\u0008]|[\u000B-\u000C]|[\u000E-\u0019]|[\u007F-\u009F]|[\uD800-\uDBFF]";

            //ﬃ ﬁ ﬀ ﬂ ﬄ
            System.Text.RegularExpressions.Regex regex = new Regex("ﬃ");
            if (regex.IsMatch(text))
            {
                text = regex.Replace(text, "ffi");
            }
            regex = new Regex("ﬁ");
            if (regex.IsMatch(text))
            {
                text = regex.Replace(text, "fi");
            }
            regex = new Regex("ﬀ");
            if (regex.IsMatch(text))
            {
                text = regex.Replace(text, "ff");
            }
            regex = new Regex("ﬂ");
            if (regex.IsMatch(text))
            {
                text = regex.Replace(text, "fl");
            }
            regex = new Regex("ﬄ");
            if (regex.IsMatch(text))
            {
                text = regex.Replace(text, "fll");
            }
            regex = new System.Text.RegularExpressions.Regex(illegalXmlChars, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (regex.IsMatch(text))
            {
                text = regex.Replace(text, " ");
            }

            int aiai = text.Length;
            //regex = new Regex(((char)13).ToString());
            //if (regex.IsMatch(text))
            //{
            //    text = regex.Replace(text, "");
            //}
            int returns = text.Split('\r').Length;
            int newlines = text.Split('\n').Length;
            if (returns > 1 && newlines == 1)
            {
                text = text.Replace("\r", System.Environment.NewLine);
            }
            else if (returns == 1 && newlines > 1)
            {
                text = text.Replace("\n", System.Environment.NewLine);
            }
            return text.Trim();
        }
        private static string CreateLogFileName()
        {
            DirectoryInfo logDir = System.IO.Directory.CreateDirectory("LogFiles");
            string LogFilename = logDir.FullName + @"\" + "ExportPDFs-" + DateTime.Now.ToString("dd-MM-yyyy") + ".txt";
            if (!System.IO.File.Exists(LogFilename))
            {
                using (FileStream fs = System.IO.File.Create(LogFilename))
                {
                    fs.Close();
                }
            }
            return LogFilename;
        }
        private static void ConfigureServices(IServiceCollection services)
        {
            //Action<ILoggingBuilder> tester = new Action<ILoggingBuilder>(configure => configure.AddConsole());
            //Action<ILoggingBuilder> tester2 = new Action<ILoggingBuilder>(configure => configure.AddSerilog());

            services.AddLogging(configure => configure.AddConsole()
                    ).AddLogging(configure => configure.AddSerilog());

        }
    }
    class ShortDoc
    {
        public Int64 itemDocumentId = 0;
        public Int64 itemId = 0;
        public string shortTitle = "";
        public string extension = "";
        public string title = "";
        public string text = "";
        public static ShortDoc GetShortDoc(SqlDataReader reader)
        {
            ShortDoc res = new ShortDoc();
            res.itemDocumentId = (Int64)reader["ITEM_DOCUMENT_ID"];
            res.itemId = (Int64)reader["ITEM_ID"];
            res.shortTitle = reader["SHORT_TITLE"].ToString();
            res.title = reader["DOCUMENT_TITLE"].ToString();
            res.extension = reader["DOCUMENT_EXTENSION"].ToString();
            res.text = reader["DOCUMENT_TEXT"].ToString();
            return res;
        }
    }
    class MiniItem
    {
        public string Title = "";
        public string Abstract = "";
        public string Id = "";
        public string Year = "";
        //i.ITEM_ID ,t.TYPE_NAME, dbo.fn_REBUILD_AUTHORS(i.ITEM_ID, 0), [TITLE], [PARENT_TITLE], [SHORT_TITLE]," 
        //[YEAR], [MONTH], [STANDARD_NUMBER], [CITY], [COUNTRY], [PUBLISHER], [INSTITUTION], [VOLUME], 
        //[PAGES], [EDITION], [ISSUE], [URL], [ABSTRACT], [DOI], [KEYWORDS]
        public string TYPE_NAME = "";
        public string AUTHORS = "";
        public string PARENT_TITLE = "";
        public string SHORT_TITLE = "";
        public string MONTH = "";
        public string STANDARD_NUMBER = "";
        public string CITY = "";
        public string COUNTRY = "";
        public string PUBLISHER = "";
        public string INSTITUTION = "";
        public string VOLUME = "";
        public string PAGES = "";
        public string EDITION = "";
        public string ISSUE = "";
        public string URL = "";
        public string DOI = "";
        public string KEYWORDS = "";
        public string INCLUDED = "";
        public static MiniItem GetMiniItem(SqlDataReader reader)
        {
            MiniItem res = new MiniItem();
            res.Title = reader["TITLE"].ToString().Replace('\t', ' ').Replace("\r\n", " ").Replace("\r", " ").Replace("\n", " ");
            res.Abstract = reader["ABSTRACT"].ToString().Replace('\t', ' ').Replace("\r\n", " ").Replace("\r", " ").Replace("\n", " "); 
            res.Id = reader["ITEM_ID"].ToString();
            res.Year = reader["YEAR"].ToString();
            res.TYPE_NAME = reader["TYPE_NAME"].ToString();
            res.AUTHORS = reader["AUTHORS"].ToString();
            res.PARENT_TITLE = reader["PARENT_TITLE"].ToString();
            res.SHORT_TITLE = reader["SHORT_TITLE"].ToString();
            res.MONTH = reader["MONTH"].ToString();
            res.STANDARD_NUMBER = reader["STANDARD_NUMBER"].ToString();
            res.CITY = reader["CITY"].ToString();
            res.COUNTRY = reader["COUNTRY"].ToString();
            res.PUBLISHER = reader["PUBLISHER"].ToString();
            res.INSTITUTION = reader["INSTITUTION"].ToString();
            res.VOLUME = reader["VOLUME"].ToString();
            res.PAGES = reader["PAGES"].ToString();
            res.EDITION = reader["EDITION"].ToString();
            res.ISSUE = reader["ISSUE"].ToString();
            res.URL = reader["URL"].ToString();
            res.DOI = reader["DOI"].ToString();
            res.KEYWORDS = reader["KEYWORDS"].ToString();
            res.INCLUDED = reader["Included"].ToString() == "True" ? "Included" : "Excluded";
            return res;
        }
    }
}
