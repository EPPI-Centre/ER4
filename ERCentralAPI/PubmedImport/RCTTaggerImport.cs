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


namespace PubmedImport
{
   
    class RCTTaggerImport
    {
        static EPPILogger Logger;

        static SQLHelper SqlHelper;

        private static int upMonth;

        private static int loMonth;

        private static readonly int maxRequestTries = 3;

        static string TmpFolderPath;

        private static string baseURL = $"http://arrowsmith.psych.uic.edu/cgi-bin/arrowsmith_uic/rct_download.cgi";

        private static string yearlyfileBaseURL = $"http://arrowsmith.psych.uic.edu/arrowsmith_uic/download/RCT_Tagger/";

        private static string domainURL = $"http://arrowsmith.psych.uic.edu";

        private static DateTime GetDate(string str)
        {
            // Make a check 
            DateTime curr = (DateTime.Parse(Regex.Match(str, @"\d{4}-\d{2}-\d{2}").Value));
            loMonth = curr.Month;
            upMonth = DateTime.Now.Month;

            return curr;
        }

        private static string GetYear(string str)
        {
            try
            {

                int count  = str.Count(Char.IsDigit);
                if (count > 7)
                {
                    int tmp = str.LastIndexOf("-");
                    string tmpStr = str.Substring(tmp + 1, str.Length - tmp - 1);
                    return Regex.Match(tmpStr, @"\d{4}").Value;
                }
                else
                {
                    return Regex.Match(str, @"\d{4}").Value;
                }

            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Format of files from arrowsmith must have changed; converting to year errors");
                return "";
            }

        }

        private static string GetMonth(string str)
        {
            return Regex.Match(str, @"\d{2}").Value;
        }

        private static string GetDay(string str)
        {
            return Regex.Match(str, @"\d{2}").Value;
        }

        private static void Yearly_Compressed_Files(string yearlyfile)
        {

            bool res = false;
            string fileName = "";
            try
            {
                // Get file from htmlLink
                (res, fileName) = DownloadCSVFiles(yearlyfile);
                if (res == false)
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Format of files from arrowsmith must have changed; converting to year errors");
                throw;
            }

            Logger.LogMessageLine("Decompressing the yearly gz file");
            string decompressedYearlyFile = Decompress(yearlyfile);

            Logger.LogMessageLine("Importing the yearly gz file into SQL");
            Import_RCT_Tagger_File(decompressedYearlyFile);

            Logger.LogMessageLine("Finished with yearly import");

        }

        private static async Task<List<string>> GetHTMLLinksAsync(string baseURL)
        {
            List<string> links = new List<string>();
            try
            {
                HttpClient hc = new HttpClient();
                HttpResponseMessage result = await hc.GetAsync(baseURL);
                Stream stream = await result.Content.ReadAsStreamAsync();
                HtmlDocument doc = new HtmlDocument();
                doc.Load(stream);

                foreach (HtmlNode link in doc.DocumentNode.SelectNodes("//a[@href]"))
                {
                    HtmlAttribute att = link.Attributes["href"];
                    links.Add(att.Value);
                }
                links.Reverse();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "error scraping links");

                links.Add("error scraping links");

            }
            return links;
        }

        private static void Weekly_Update_files(string weeklyFile)
        {
            Logger.LogMessageLine("Checking update files");

            bool res = false;
            Logger.LogMessageLine("Importing update files");

            (res, weeklyFile) = DownloadCSVFiles(weeklyFile);
            if (res)
            {
                Import_RCT_Tagger_File(weeklyFile);
            }

            Logger.LogMessageLine("Finished with update imports");
        }

        private static void Log_Import_Job(string filename)
        {
            Logger.LogMessageLine("Scores and PMIDs have been imported into SQL for the follwing file: " + filename);

            using (SqlConnection conn = new SqlConnection(SqlHelper.DataServiceDB))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();
                try
                {
                    List<SqlParameter> sqlParams = new List<SqlParameter>();

                    // the date field needs to change based on whether it is a yearky or weekly file
                    DateTime date = DateTime.Now;
                    int tmpStart = filename.LastIndexOf("rct_predictions");
                    int fullLength = filename.Length;
                    string tmpStr = filename.Substring(tmpStart + 1, fullLength - tmpStart - 1);
                    if (tmpStr.Count(Char.IsDigit) > 6)
                    {
                        date = Convert.ToDateTime(GetDate(tmpStr));
                    }
                    else
                    {
                        string tempStr = GetYear(tmpStr) + "-12-30";
                        date = Convert.ToDateTime(tempStr);
                    }
                    
                    int start = filename.LastIndexOf("/");
                    int length = filename.Length;
                    filename = filename.Substring(start + 1, length - start-1);

                    sqlParams.Add(new SqlParameter("@RCT_FILE_NAME", filename));
                    sqlParams.Add(new SqlParameter("@RCT_IMPORT_DATE", DateTime.Now));
                    sqlParams.Add(new SqlParameter("@RCT_UPLOAD_DATE", date));

                    SqlParameter[] parameters = new SqlParameter[3];
                    parameters = sqlParams.ToArray();

                    int res = SqlHelper.ExecuteNonQuerySP(conn.ConnectionString, "[dbo].[st_RCT_IMPORT_UPDATE_INSERT]", parameters);

                    //===============================================
                    Logger.LogMessageLine("Job record inserted into DB");

                    transaction.Commit();
                   
                }
                catch (Exception ex)
                {
                    Logger.LogException(ex, "");
                    transaction.Rollback();
                }
                conn.Close();
            }
        }

        private static string Decompress(string yearlyFile)
        {
            // String manip
            int endStr = yearlyFile.Length;
            int startStr = yearlyFile.LastIndexOf("/");
            yearlyFile = yearlyFile.Substring(startStr + 1, endStr - startStr - 1);

            string unZippedFileName = TmpFolderPath + "\\" + yearlyFile.Substring(0, yearlyFile.Length-3);
            FileInfo fileToBeUnGZipped = new FileInfo(TmpFolderPath + "\\" + yearlyFile );
            if (!fileToBeUnGZipped.Exists)
            {
                return "null";
            }
            unZippedFileName = unZippedFileName.Replace("\\", "//");
            Logger.LogMessageLine("Decompressing " + yearlyFile + "....");
            using (FileStream fileToDecompressAsStream = fileToBeUnGZipped.OpenRead())
            {
                using (FileStream decompressedStream = File.Create(unZippedFileName))
                {
                    using (GZipStream decompressionStream = new GZipStream(fileToDecompressAsStream, CompressionMode.Decompress))
                    {
                        try
                        {
                            decompressionStream.CopyTo(decompressedStream);
                        }
                        catch (Exception ex)
                        {
                            Logger.LogException(ex, "uncompressing file to parse.");
                        }
                    }
                }
            }
            if (File.Exists(fileToBeUnGZipped.FullName))
            {
                try
                {
                    File.Delete(fileToBeUnGZipped.FullName);
                }
                catch (Exception ex)
                {

                    Logger.LogException(ex, "deleting the compressed file.");
                }
            }
            return unZippedFileName;
        }

        private static (bool, string) DownloadCSVFiles(string filename)
        {
            bool success = false;

            string url = yearlyfileBaseURL + filename + "";
            Uri urlCheck = new Uri(url);

            var remainingTries = maxRequestTries;
            var exceptions = new List<Exception>();
            do
            {
                --remainingTries;
                try
                {
                    (success, filename) = Execute(urlCheck, filename);
                    if (success)
                    {
                        Logger.LogMessageLine("Downloaded the following file: " + filename);
                        return (success, filename);
                    }
                }
                catch (Exception e)
                {
                    Logger.LogException(e, "");
                    exceptions.Add(e);
                }
            }
            while (remainingTries > 0);
            return (false, filename);
        }

        private static void Import_RCT_Tagger_File(string filename)
        {
            // After downloading the file import at that point
            List<RCT_Tag> recs = new List<RCT_Tag>();
            var RCT_TABLE = new DataTable();
            SqlTransaction transaction;

            using (SqlConnection conn = new SqlConnection(SqlHelper.DataServiceDB))
            {
                conn.Open();
                try
                {
                    string filePath = TmpFolderPath + "\\";
                    string decompressedFile = filename;

                    using (var sr = new StreamReader(decompressedFile))
                    {
                        string strline = "";
                        string[] _values = null;

                        int x = 0;
                        while (!sr.EndOfStream)
                        {
                            strline = sr.ReadLine();
                            _values = strline.Split('\t');

                            if (x > 0)
                            {
                                RCT_Tag record = new RCT_Tag();
                                record.PMID = _values[0];
                                record.RCT_SCORE = _values[1];
                                recs.Add(record);
                            }
                            x++;
                        }
                        sr.Close();

                    }

                    double divisor = 0.0;
                    int skip = 0;
                    int page = 0;
                    if (recs.Count() < 1000)
                    {
                        divisor = Math.Floor((double)recs.Count());
                        page = recs.Count();
                    }
                    else
                    {
                        divisor = Math.Floor((double)recs.Count() / 1000);
                        page = 1000;
                        skip = 1000;
                    }
                    for (int i = 1; i < divisor; i++)
                    {
                        transaction = conn.BeginTransaction();
                        try
                        {
                            List<SqlParameter> sqlParams = new List<SqlParameter>();

                            sqlParams.Add(new SqlParameter("@ids", string.Join(",", recs.Select(x => x.PMID).Skip(skip * i).Take(page).ToList())));
                            sqlParams.Add(new SqlParameter("@scores", string.Join(",", recs.Select(x => x.RCT_SCORE).Skip(skip * i).Take(page).ToList())));

                            SqlParameter[] parameters = new SqlParameter[2];
                            parameters = sqlParams.ToArray();

                            int res = SqlHelper.ExecuteNonQuerySP(conn.ConnectionString, "[dbo].[st_ReferenceUpdate_Arrow_Scores]", parameters);

                            transaction.Commit();

                        }
                        catch (Exception ex)
                        {
                            Logger.LogException(ex, "");
                            transaction.Rollback();
                        }
                    }
                    Logger.LogMessageLine("The total number of scores updated is: " + recs.Count());

                }
                catch (Exception ex)
                {
                    Logger.LogException(ex, "Catch all try block");
                }
                finally
                {
                    Log_Import_Job(filename);
                    if (!filename.Contains("TmpFiles"))
                    {
                        filename = TmpFolderPath + "\\" + filename;
                    }
                    if (File.Exists(filename))
                    {
                        File.Delete(filename);
                    }
                    conn.Close();

                }
            }
        }

        private static (bool, string) Execute(Uri urlCheck, string fileName)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlCheck);
            request.Timeout = 4000;
            request.Method = "HEAD";
            HttpWebResponse response;
            response = (HttpWebResponse)request.GetResponse();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                // Download the new yealry file ready for decompressing.
                WebClient _client = new WebClient();

                int startStr = fileName.LastIndexOf("/");
                int endStr = fileName.Length;
                fileName = fileName.Substring(startStr + 1, endStr - startStr - 1);

                // This destination path needs to be sorted out...
                string _destinationPath = TmpFolderPath + "\\" + fileName + "";

                _client.DownloadFile(urlCheck, _destinationPath); //Download the file. 

                return (true, fileName);
            }
            else
            {
                Logger.LogMessageLine("Error Contacting server");
                return (false, "error");
            }
        }

        private static string LastUPDATEFileUploaded()
        {
            string fileName = "";
            using (SqlConnection conn = new SqlConnection("Server = localhost; Database = DataService; Integrated Security = True; "))
            {
                conn.Open();
                var res = SqlHelper.ExecuteQuerySPNoParam(conn, "[dbo].[st_RCT_GET_LATEST_UPLOAD_FILE_NAME]");
                if (res.HasRows)
                {
                    while (res.Read())
                    {
                        fileName = res["RCT_FILE_NAME"].ToString();
                    }
                    res.Close();
                }
                else
                {
                    fileName = "1900-01-01";
                }
            }
            return fileName;
        }
               
        private static string LastYEARLYFileUploaded()
        {
            string fileName = "";
            using (SqlConnection conn = new SqlConnection("Server = localhost; Database = DataService; Integrated Security = True; "))
            {
                conn.Open();
                var res = SqlHelper.ExecuteQuerySPNoParam(conn, "[dbo].[st_RCT_GET_LATEST_YEARLY_FILE_NAME]");
                if (res.HasRows)
                {
                    while (res.Read())
                    {
                        fileName = res["RCT_FILE_NAME"].ToString();
                    }
                    res.Close();
                }
            }
            return fileName;
        }

        public static void RunRCTTaggerImport()
        {
            DirectoryInfo tempDir = System.IO.Directory.CreateDirectory("Tmpfiles");
            TmpFolderPath = tempDir.FullName;
           
            Task<List<string>> task = GetHTMLLinksAsync(baseURL);
            task.Wait();
            List<string> htmlLinks = task.Result.Where(x => x.Contains("arrowsmith")).ToList();

            Logger = Program.Logger;

            SqlHelper = Program.SqlHelper;

            Logger.LogMessageLine("Checking for new yearly import files");

            List<string> yearlyLinks = htmlLinks.Where(x => x.Contains(".gz")).ToList();
            List<string> weeklyLinks = htmlLinks.Where(x => !x.Contains(".gz")).ToList();

            List<string> yearlyFilesDownloadedFullPath = GetAllYearlyFiles();
            List<string> yearlyFileNames = new List<string>();
            List<string> yearlyLinksShort = new List<string>();
            int cnt = 0;
            foreach (var item in yearlyLinks)
            {
                int start = item.LastIndexOf("/");
                int length = item.Length;
                string tmpStr = item.Substring(start + 1, length - start - 4);
                yearlyLinksShort.Add(tmpStr);
                cnt++;
            }
            cnt = 0;
            foreach (var item in yearlyFilesDownloadedFullPath)
            {
                int start = item.LastIndexOf("/");
                int length = item.Length;
                string tmpStr = item.Substring(start + 1, length - start - 1);
                yearlyFileNames.Add(tmpStr);
                cnt++;
            }

            string lastUploadedYearlyFile = LastYEARLYFileUploaded();
            cnt = 0;
            foreach (var item in yearlyLinksShort)
            {
                string present = yearlyFileNames.FirstOrDefault(s => s.Equals(item));
                if (present == null)
                {
                    Yearly_Compressed_Files(item + ".gz");
                }
                cnt++;
            }

            string strDate = LastUPDATEFileUploaded();

            DateTime currDate = GetDate(strDate);

            weeklyLinks.Where(y => GetDate(y) > currDate).ToList().ForEach(x => Weekly_Update_files(x));

            Logger.LogMessageLine("Finished all RCT Score updates");

        }

        private static List<string> GetAllYearlyFiles()
        {
            List<string> fileNames = new List<string>();
            using (SqlConnection conn = new SqlConnection("Server = localhost; Database = DataService; Integrated Security = True; "))
            {
                conn.Open();
                var res = SqlHelper.ExecuteQuerySPNoParam(conn, "[dbo].[st_RCT_GET_LATEST_YEARLY_FILE_NAMES]");
                if (res.HasRows)
                {
                    while (res.Read())
                    {
                        fileNames.Add(res["RCT_FILE_NAME"].ToString());
                    }
                    res.Close();
                }
            }
            return fileNames;
        }
    }

    public class RCT_Tag
    {
        public string PMID { get; set; }
        public string RCT_SCORE { get; set; }
    }

}
