using EPPIDataServices.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using System.Net.Http;
using System.Threading.Tasks;


namespace RCT_Tagger_Import
{
    class Program
    {
        //        static EPPILogger Logger;

        //        static SQLHelper SqlHelper;

        //        private static int upMonth;

        //        private static int loMonth;

        //        private static readonly int maxRequestTries = 3;

        //        private static DateTime GetDate(string str)
        //        {

        //            DateTime curr = (DateTime.Parse(Regex.Match(str, @"\d{4}-\d{2}-\d{2}").Value));
        //            loMonth = curr.Month;
        //            upMonth = DateTime.Now.Month;

        //            return curr;
        //        }

        //        public static string GetYear(string str)
        //        {
        //            return Regex.Match(str, @"\d{4}").Value;
        //        }

        //        public static string GetMonth(string str)
        //        {
        //            return Regex.Match(str, @"\d{2}").Value;
        //        }

        //        public static string GetDay(string str)
        //        {
        //            return Regex.Match(str, @"\d{2}").Value;
        //        }
        //        static string TmpFolderPath;
        static void Main(string[] args)
    {
    //            DirectoryInfo tempDir = System.IO.Directory.CreateDirectory("TmpFiles");
    //            TmpFolderPath = tempDir.FullName;
    //            string baseURL = $"http://arrowsmith.psych.uic.edu/cgi-bin/arrowsmith_uic/rct_download.cgi";

    //            Task<List<string>> task = GetHTMLLinksAsync(baseURL);
    //            task.Wait();
    //            List<string> htmlLinks = task.Result.Where(x => x.Contains("arrowsmith")).ToList();

    //            Logger = new EPPILogger(false);

    //            SqlHelper = new SQLHelper(Logger);

    //            Logger.LogMessageLine("Checking for new yearly import files");

    //            List<string> yearlyLinks = htmlLinks.Where(x => x.Contains(".gz")).ToList();
    //            List<string> weeklyLinks = htmlLinks.Where(x => !x.Contains(".gz")).ToList();

    //            string strYear = LastYEARLYFileUploaded();

    //            int currYear = Convert.ToInt16(GetYear(strYear));

    //            yearlyLinks.Where(y => int.Parse(GetYear(y)) > currYear).ToList().ForEach(x => Yearly_Compressed_Files(x));

    //            string strDate = LastUPDATEFileUploaded();

    //            DateTime currDate = GetDate(strDate);

    //            weeklyLinks.Where(y => GetDate(y) > currDate).ToList().ForEach(x => Weekly_Update_files(x));

    //            Console.WriteLine("Finished Imports");
    //            Console.ReadLine();
}

        //        private static void Yearly_Compressed_Files(string yearlyfile)
        //        {

        //            bool res = false;
        //            string fileName = "";
        //            try
        //            {
        //                // Get file from htmlLink
        //                (res, fileName) =  DownloadCSVFiles(yearlyfile);
        //                if (res == false)
        //                {
        //                    return;
        //                }
        //            }
        //            catch (Exception)
        //            {

        //                throw;
        //            }

        //            Logger.LogMessageLine("Decompressing the yearly gz file");
        //            string decompressedYearlyFile = Decompress(yearlyfile);

        //            Logger.LogMessageLine("Importing the yearly gz file into SQL");
        //            Import_RCT_Tagger_File(decompressedYearlyFile);

        //            Logger.LogMessageLine("Finished with yearly import");

        //        }

        //        public static async Task<List<string>> GetHTMLLinksAsync(string baseURL)
        //        {
        //            List<string> links = new List<string>();
        //            try
        //            {
        //                HttpClient hc = new HttpClient();
        //                HttpResponseMessage result = await hc.GetAsync(baseURL);
        //                Stream stream = await result.Content.ReadAsStreamAsync();
        //                HtmlDocument doc = new HtmlDocument();
        //                doc.Load(stream);

        //                foreach (HtmlNode link in doc.DocumentNode.SelectNodes("//a[@href]"))
        //                {
        //                    HtmlAttribute att = link.Attributes["href"];
        //                    links.Add(att.Value);
        //                }
        //                links.Reverse();
        //            }
        //            catch (Exception)
        //            {
        //                links.Add("error getting links");

        //            }
        //            return links;
        //        }

        //        private static void Weekly_Update_files(string weeklyFile)
        //        {
        //            Logger.LogMessageLine("Checking update files");
        //            //List<string> Update_Files = weeklyFiles;
        //            //string weeklyFileName = "";
        //            bool res = false;
        //            Logger.LogMessageLine("Importing update files");
        //            //foreach (var item in Update_Files)
        //            //{
        //                (res, weeklyFile) = DownloadCSVFiles(weeklyFile);
        //                if (res)
        //                {
        //                    Import_RCT_Tagger_File(weeklyFile);
        //                }
        //            //}
        //            Logger.LogMessageLine("Finished with update imports");
        //        }

        //        private static void Log_Import_Job(string filename)
        //        {
        //            Console.WriteLine("Scores and PMIDs have been imported into SQL for the follwing  file: " + filename);
        //            // If successful call a SP to insert the Import job into the relevant table
        //            // 3 fields: RCT_FILE_NAME, RCT_IMPORT_DATE and RCT_UPLOAD_DATE
        //            using (SqlConnection conn = new SqlConnection(SqlHelper.DataServiceDB))
        //            {
        //                conn.Open();
        //                SqlTransaction transaction = conn.BeginTransaction();
        //                try
        //                {
        //                    List<SqlParameter> sqlParams = new List<SqlParameter>();

        //                    // the date field needs to change based on whether it is a yearky or weekly file
        //                    DateTime date = DateTime.Now;
        //                    if (filename.Count(Char.IsDigit)  > 6)
        //                    {
        //                        int tmpStart = filename.LastIndexOf("rct_predictions");
        //                        int fullLength = filename.Length;
        //                        string tmpStr = filename.Substring(tmpStart + 1, fullLength - tmpStart - 1);
        //                        date = Convert.ToDateTime(GetDate(tmpStr));
        //                    }
        //                    else
        //                    {
        //                        int tmpStart = filename.LastIndexOf("rct_predictions");
        //                        int fullLength = filename.Length;
        //                        string tmpStr = filename.Substring(tmpStart + 1, fullLength - tmpStart - 1);
        //                        string tempStr = GetYear(tmpStr) + "-12-30";
        //                        date = Convert.ToDateTime(tempStr);
        //                    }

        //                    sqlParams.Add(new SqlParameter("@RCT_FILE_NAME", filename ));
        //                    sqlParams.Add(new SqlParameter("@RCT_IMPORT_DATE", DateTime.Now));
        //                    sqlParams.Add(new SqlParameter("@RCT_UPLOAD_DATE", date));

        //                    SqlParameter[] parameters = new SqlParameter[3];
        //                    parameters = sqlParams.ToArray();

        //                    int res = SqlHelper.ExecuteNonQuerySP(conn.ConnectionString, "[dbo].[st_RCT_IMPORT_UPDATE_INSERT]", parameters);

        //                    //===============================================
        //                    Logger.LogMessageLine("Job record inserted into DB");

        //                    transaction.Commit();
        //                    Console.WriteLine("Imported the follwing into SQL: " + filename);
        //                }
        //                catch (Exception)
        //                {
        //                    transaction.Rollback();
        //                }
        //                conn.Close();
        //            }
        //        }

        //        private static string Decompress(string yearlyFile)
        //        {
        //            // String manip
        //            int endStr = yearlyFile.Length;
        //            int startStr = yearlyFile.LastIndexOf("/");
        //            yearlyFile = yearlyFile.Substring(startStr + 1, endStr - startStr - 1);

        //            string unZippedFileName = TmpFolderPath + "\\" + yearlyFile.Substring(0, yearlyFile.Length - 3);
        //            FileInfo fileToBeUnGZipped = new FileInfo(TmpFolderPath + "\\" + yearlyFile);
        //            if (!fileToBeUnGZipped.Exists)
        //            {
        //                return "null";
        //            }
        //            unZippedFileName = unZippedFileName.Replace("\\", "//");
        //            Logger.LogMessageLine("Decompressing " + yearlyFile + "....");
        //            using (FileStream fileToDecompressAsStream = fileToBeUnGZipped.OpenRead())
        //            {
        //                using (FileStream decompressedStream = File.Create(unZippedFileName))
        //                {
        //                    using (GZipStream decompressionStream = new GZipStream(fileToDecompressAsStream, CompressionMode.Decompress))
        //                    {
        //                        try
        //                        {
        //                            decompressionStream.CopyTo(decompressedStream);
        //                        }
        //                        catch (Exception ex)
        //                        {
        //                            Logger.LogException(ex, "uncompressing file to parse.");
        //                        }
        //                    }
        //                }
        //            }
        //            if (File.Exists(fileToBeUnGZipped.FullName))
        //            {
        //                try
        //                {
        //                    File.Delete(fileToBeUnGZipped.FullName);
        //                }
        //                catch (Exception ex)
        //                {

        //                    Logger.LogException(ex, "deleting the compressed file.");
        //                }
        //            }
        //            return unZippedFileName;
        //        }

        //        private static (bool, string) DownloadCSVFiles(string filename)
        //        {
        //            string filePath = "";
        //            bool success = false;
        //            // build the url from the sql query result
        //            string url = "http://arrowsmith.psych.uic.edu" + filename + "";
        //            Uri urlCheck = new Uri(url);

        //            // replace
        //            var remainingTries = maxRequestTries;
        //            var exceptions = new List<Exception>();
        //            do
        //            {
        //                --remainingTries;
        //                try
        //                {
        //                    (success, filename) = Execute(urlCheck, filename);
        //                    if (success)
        //                    {
        //                        Console.WriteLine("Downloaded the following file: " + filename);
        //                        return (success, filename);
        //                    }
        //                }
        //                catch (Exception e)
        //                {
        //                    exceptions.Add(e);
        //                }
        //            }
        //            while (remainingTries > 0);
        //            return (false, filename);
        //        }

        //        private static void Import_RCT_Tagger_File(string filename)
        //        {
        //            // After downloading the file import at that point
        //            List<RCT_Tag> recs = new List<RCT_Tag>();
        //            var RCT_TABLE = new DataTable();
        //            SqlTransaction transaction;

        //            using (SqlConnection conn = new SqlConnection(SqlHelper.DataServiceDB))
        //            {
        //                conn.Open();
        //                try
        //                {
        //                    string filePath = TmpFolderPath + "\\";
        //                    string decompressedFile = "";
        //                    if (!filename.Contains("Files"))
        //                    {
        //                        decompressedFile = filePath + filename;
        //                    }
        //                    else
        //                    {
        //                        decompressedFile = filename;
        //                    }

        //                    using (var sr = new StreamReader( decompressedFile))
        //                    {
        //                        string strline = "";
        //                        string[] _values = null;

        //                        int x = 0;
        //                        while (!sr.EndOfStream)
        //                        {
        //                            strline = sr.ReadLine();
        //                            _values = strline.Split('\t');

        //                            if (x > 0 )
        //                            {
        //                                RCT_Tag record = new RCT_Tag();
        //                                record.PMID = _values[0];
        //                                record.RCT_SCORE = _values[1];
        //                                recs.Add(record);
        //                            }
        //                            x++;
        //                        }
        //                        sr.Close();

        //                    }

        //                    double divisor = 0.0;
        //                    int skip = 0;
        //                    int page = 0;
        //                    if (recs.Count() < 1000)
        //                    {
        //                        divisor = Math.Floor((double)recs.Count());
        //                        page = recs.Count();
        //                    }
        //                    else
        //                    {
        //                        divisor = Math.Floor((double)recs.Count() / 1000);
        //                        page = 1000;
        //                        skip = 1000;
        //                    }
        //                    for (int i = 1; i < divisor; i++)
        //                    {
        //                        transaction = conn.BeginTransaction();
        //                        try
        //                        {
        //                            List<SqlParameter> sqlParams = new List<SqlParameter>();

        //                            sqlParams.Add(new SqlParameter("@ids", string.Join(",", recs.Select(x => x.PMID).Skip(skip*i).Take(page).ToList())));
        //                            sqlParams.Add(new SqlParameter("@scores", string.Join(",", recs.Select(x => x.RCT_SCORE).Skip(skip*i).Take(page).ToList())));

        //                            SqlParameter[] parameters = new SqlParameter[2];
        //                            parameters = sqlParams.ToArray();

        //                            int res = SqlHelper.ExecuteNonQuerySP(conn.ConnectionString, "[dbo].[st_ReferenceUpdate_Arrow_Scores]", parameters);

        //                            //===============================================
        //                            Logger.LogMessageLine("Done " + i*skip + " records...");

        //                            transaction.Commit();

        //                        }
        //                        catch (Exception)
        //                        {
        //                            transaction.Rollback();
        //                        }
        //                    }

        //                }
        //                catch (Exception ex)
        //                {
        //                    Logger.LogException(ex, "Catch all try block");
        //                }
        //                finally
        //                {
        //                    Log_Import_Job(filename);
        //                    if (!filename.Contains("TmpFiles"))
        //                    {
        //                        filename = TmpFolderPath + "\\" + filename;
        //                    }
        //                    if (File.Exists(filename))
        //                    {
        //                        File.Delete(filename);
        //                    }
        //                    conn.Close();

        //                }
        //            }
        //        }

        //        public static (bool, string) Execute(Uri urlCheck, string fileName)
        //        {
        //            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlCheck);
        //            request.Timeout = 4000;
        //            request.Method = "HEAD";
        //            HttpWebResponse response;
        //            response = (HttpWebResponse)request.GetResponse();
        //            if (response.StatusCode == HttpStatusCode.OK)
        //            {
        //                // Download the new yealry file ready for decompressing.
        //                WebClient _client = new WebClient();

        //                int startStr = fileName.LastIndexOf("/");
        //                int endStr = fileName.Length;
        //                fileName = fileName.Substring(startStr+1, endStr-startStr-1);

        //                // This destination path needs to be sorted out...
        //                string _destinationPath = TmpFolderPath + "\\" + fileName + "";

        //                _client.DownloadFile(urlCheck, _destinationPath); //Download the file. 

        //                return (true, fileName);
        //                //return (true, _destinationPath);
        //            }
        //            else
        //            {
        //                return (false, "error");
        //            }
        //        }

        //        private static string LastUPDATEFileUploaded()
        //        {           
        //            string fileName = "";
        //            using (SqlConnection conn = new SqlConnection("Server = localhost; Database = DataService; Integrated Security = True; "))
        //            {
        //                conn.Open();
        //                var res = SqlHelper.ExecuteQuerySPNoParam(conn, "[dbo].[st_RCT_GET_LATEST_UPLOAD_FILE_NAME]");
        //                if (res.HasRows)
        //                {
        //                    while (res.Read())
        //                    {
        //                        fileName = res["RCT_FILE_NAME"].ToString();
        //                    }
        //                    res.Close();
        //                }
        //                else
        //                {
        //                    fileName = "1900-01-01";
        //                }
        //            }
        //            return fileName;
        //        }

        //        private static string LastYEARLYFileUploaded()
        //        {
        //            string fileName = "";
        //            using (SqlConnection conn = new SqlConnection("Server = localhost; Database = DataService; Integrated Security = True; "))
        //            {
        //                conn.Open();
        //                var res = SqlHelper.ExecuteQuerySPNoParam(conn, "[dbo].[st_RCT_GET_LATEST_YEARLY_FILE_NAME]");
        //                if (res.HasRows)
        //                {
        //                    while (res.Read())
        //                    {
        //                        fileName = res["RCT_FILE_NAME"].ToString();
        //                    }
        //                    res.Close();
        //                }
        //                else
        //                {
        //                    fileName = "1900-01-01";
        //                }
        //            }
        //            return fileName;
        //        }

        //        private static List<string> CheckYearlyFiles(List<string> AllLinks)
        //        {
        //            //long currentYear = Convert.ToInt32(DateTime.Now.Year) - 1;
        //            //string fileName = LastYEARLYFileUploaded();

        //            //// Compare fileName year with the current year...
        //            //string latestfileYear = Program.GetYear(fileName);
        //            //if (Convert.ToInt64(latestfileYear) >= Convert.ToInt64(currentYear))
        //            //{
        //            //    Logger.LogMessageLine("Already have the latest gz yearly file imported!");
        //            //    return (false, "Yearly file imported already");
        //            //}
        //            //else
        //            //{
        //            //    fileName  = "rct_predictions_" + currentYear + ".csv.gz";
        //            //    return DownloadCSVFiles(fileName);
        //            //}

        //            //Get the links that are compressed as these are the yearly ones
        //            return AllLinks.Where(x => x.Contains(".gz")).ToList();

        //        }

        //        private static List<string> CheckUpdateFiles()
        //        {

        //            List<string> fileNames = new List<string>();
        //            string currentFileName = "";

        //            currentFileName = LastUPDATEFileUploaded();

        //            if (DateTime.Compare(GetDate(currentFileName), DateTime.Now) > 0)
        //            {
        //                // Then when up to date download the following:
        //                fileNames.Add("http://arrowsmith.psych.uic.edu/api/download_weekly_csv");
        //            }
        //            else
        //            {
        //                // Go from most up to date file and forwards
        //                long currentYear = Convert.ToInt32(DateTime.Now.Year);
        //                string updateFileName = "rct_predictions_";
        //                string day = "";
        //                string month = "";
        //                string dateStr = "";
        //                List<int> days = new List<int>();
        //                for (int i = 1; i <= 31; i++)
        //                {
        //                    days.Add(i);
        //                }
        //                List<int> months = new List<int>();
        //                for (int i = 1; i <= 12; i++)
        //                {
        //                    months.Add(i);
        //                }
        //                for (int i = loMonth; i <= upMonth; i++)
        //                {
        //                    for (int j = 1; j <= 31; j++)
        //                    {
        //                        month = string.Format("{0:D2}", months[i - 1]);
        //                        day = string.Format("{0:D2}", days[j - 1]);
        //                        dateStr = "" + currentYear + "-" + month + "-" + day + ".csv";
        //                        fileNames.Add(updateFileName + dateStr);
        //                    }
        //                }
        //            }
        //            return fileNames;
        //        }
    }

    //    public class RCT_Tag
    //    {
    //        public string PMID { get; set; }
    //        public string RCT_SCORE { get; set; }
    //    }

}
