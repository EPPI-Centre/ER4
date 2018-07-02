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

        static bool yearly = false;

        private static DateTime GetDate(string str)
        {
            try
            {

                int count = str.Count(Char.IsDigit);
                if (count > 7)
                {
                    DateTime curr = (DateTime.Parse(Regex.Match(str, @"\d{4}-\d{2}-\d{2}").Value));
                    loMonth = curr.Month;
                    upMonth = DateTime.Now.Month;

                    return curr;
                }
                else
                {
                    string tmpStr =  Regex.Match(str, @"\d{4}").Value;
                    DateTime tmpDate = DateTime.Parse("31-12-" + tmpStr);
                    return tmpDate;
                }

            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Format of files from arrowsmith must have changed; converting to year errors");
                return DateTime.Now;
            }
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
            try
            {
                return Regex.Match(str, @"\d{2}").Value;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Format of files from arrowsmith must have changed; converting to year errors");
                return "";
            }
        }

        private static string GetDay(string str)
        {
            try
            { 
                return Regex.Match(str, @"\d{2}").Value;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Format of files from arrowsmith must have changed; converting to year errors");
                return "";
            }
        }

        private static void Yearly_Compressed_Files(string yearlyfile)
        {

            bool res = false;
            string fileName = "";
            try
            {
                (res, fileName) = DownloadCSVFiles(yearlyfile);
                if (res == false){  return; }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Format of files from arrowsmith must have changed; converting to year errors");
                throw;
            }

            Logger.LogMessageLine("Decompressing the yearly gz file");
            string decompressedYearlyFile = Decompress(yearlyfile);

            if (decompressedYearlyFile is null)
            {
                Logger.LogMessageLine("Error with decompressing file");
                return;
            }

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
            Logger.LogMessageLine("Scores have been imported into SQL for the following file: " + filename);

            using (SqlConnection conn = new SqlConnection(SqlHelper.DataServiceDB))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();
                List<SqlParameter> sqlParams = new List<SqlParameter>();
                try
                {

                    int start = filename.LastIndexOf("/");
                    int length = filename.Length;
                    filename = filename.Substring(start + 1, length - start - 1);
                    int count = filename.Count(Char.IsDigit);
                   
                    DateTime date = DateTime.Now;
                    if (filename == Program.ArrowsmithRCTBaselineFile)
                    {
                        // long yearly file
                        date = DateTime.Parse("31-12-2016");
                        sqlParams.Add(new SqlParameter("@RCT_FILE_NAME",  filename + ".gz"));
                        sqlParams.Add(new SqlParameter("@RCT_IMPORT_DATE", DateTime.Now));
                        sqlParams.Add(new SqlParameter("@RCT_UPLOAD_DATE", date));
                    }
                    else if (yearly == true)
                    {
                        //short yearly file
                        string tempStr = GetYear(filename) + "-12-30";
                        date = Convert.ToDateTime(tempStr);
                        sqlParams.Add(new SqlParameter("@RCT_FILE_NAME",  filename + ".gz"));
                        sqlParams.Add(new SqlParameter("@RCT_IMPORT_DATE", DateTime.Now));
                        sqlParams.Add(new SqlParameter("@RCT_UPLOAD_DATE", date));

                    }
                    else
                    {
                        start = filename.LastIndexOf("\\");
                        length = filename.Length;
                        filename = filename.Substring(start + 1, length - start - 1);
                        //weekly file
                        sqlParams.Add(new SqlParameter("@RCT_FILE_NAME", filename));
                        sqlParams.Add(new SqlParameter("@RCT_IMPORT_DATE", DateTime.Now));
                        sqlParams.Add(new SqlParameter("@RCT_UPLOAD_DATE", date));

                    }


                    SqlParameter[] parameters = new SqlParameter[3];
                    parameters = sqlParams.ToArray();

                    int res = SqlHelper.ExecuteNonQuerySP(conn.ConnectionString, "[dbo].[st_RCT_IMPORT_UPDATE_INSERT]", parameters);

                    //===============================================
                    Logger.LogMessageLine("Job record inserted into DB");

                    transaction.Commit();
                   
                }
                catch (SqlException sqlex)
                {
                    Logger.LogSQLException(sqlex, "", sqlParams.ToArray());
                    transaction.Rollback();
                }
                catch(Exception ex)
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
                return null;
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

            string url = Program.ArrowsmithRCTyearlyfileBaseURL + filename;
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
                    Logger.LogException(e, "HTTP request error");
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

                    if (!filename.Contains("\\") && !filename.Contains("//"))
                    {
                        filename = TmpFolderPath + "\\" + filename;
                    }
                    
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
                    int done = 0;
                    if (recs.Count() < 1000)
                    {
                        divisor = 0;
                        page = recs.Count();
                    }
                    else
                    {
                        divisor = Math.Floor((double)recs.Count() / 1000);
                        page = 1000;
                        skip = 1000;
                    }
                    for (int i = 0; i <= divisor; i++)
                    {
                        List<SqlParameter> sqlParams = new List<SqlParameter>();
                        transaction = conn.BeginTransaction();
                        try
                        {

                            List<string> tmpL = recs.Select(x => x.PMID).Skip(skip * i).Take(page).ToList();
                            sqlParams.Add(new SqlParameter("@ids", string.Join(",", tmpL)));
                            sqlParams.Add(new SqlParameter("@scores", string.Join(",", recs.Select(x => x.RCT_SCORE).Skip(skip * i).Take(page).ToList())));

                            int res = SqlHelper.ExecuteNonQuerySP(conn.ConnectionString, "[dbo].[st_ReferenceUpdate_Arrow_Scores]", sqlParams.ToArray());

                            transaction.Commit();
                            done += tmpL.Count();
                        }
                        catch (SqlException sqlex)
                        {
                            Logger.LogSQLException(sqlex, "", sqlParams.ToArray());
                            transaction.Rollback();
                        }
                        catch (Exception ex)
                        {
                            Logger.LogException(ex, "");
                            transaction.Rollback();
                        }
                    }
                    int todo = recs.Count();
                    Logger.LogMessageLine("The total number of scores updated is: " + done);

                }
                catch (Exception ex)
                {
                    Logger.LogException(ex, "Catch all try block");
                }
                finally
                {
                    Log_Import_Job(filename);
                    if (!filename.Contains("Tmpfiles"))
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
            try
            {
                using (SqlConnection conn = new SqlConnection("Server=localhost; Database = DataService; Integrated Security = True; "))
                {
                    conn.Open();
                    var res = SqlHelper.ExecuteQuerySP(conn, "[dbo].[st_RCT_GET_LATEST_UPLOAD_FILE_NAME]");
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
            }
            catch (SqlException sqlex)
            {
                Logger.LogSQLException(sqlex, "");

            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "");

            }
            return fileName;

        }
               

        public static void RunRCTTaggerImport()
        {
            // Warning this a terrible way to do this; if Arrowsmith change the format
            // of their links all of this will break.
            DirectoryInfo tempDir = System.IO.Directory.CreateDirectory("Tmpfiles");
            TmpFolderPath = tempDir.FullName;
           
            Task<List<string>> task = GetHTMLLinksAsync(Program.ArrowsmithRCTbaseURL);
            task.Wait();
            List<string> htmlLinks = task.Result.Where(x => x.Contains("arrowsmith")).ToList();

            Logger = Program.Logger;

            SqlHelper = Program.SqlHelper;

            Logger.LogMessageLine("Checking for new yearly import files");

            List<string> yearlyLinks = htmlLinks.Where(x => x.Contains(".gz")).ToList();
            List<string> weeklyLinks = htmlLinks.Where(x => !x.Contains(".gz")).ToList();
            if (yearlyLinks.Count() > 0)
            {
                yearlyLinks.Reverse();
            }
            List<string> yearlyFilesDownloadedFullPath = GetAllYearlyFiles();
            List<string> yearlyFileNames = new List<string>();
            List<string> yearlyLinksShort = new List<string>();
            int cnt = 0;
            foreach (var item in yearlyLinks)
            {
                int start = item.LastIndexOf("/");
                int length = item.Length;
                string tmpStr = item.Substring(start + 1, length - start -1);
                yearlyLinksShort.Add(tmpStr);
                cnt++;
            }
            cnt = 0;
            foreach (var item in yearlyFilesDownloadedFullPath)
            {
                string tmpStr = item.Substring(9, item.Length - 9);
                yearlyFileNames.Add(item);
                cnt++;
            }

            cnt = 0;
            foreach (var item in yearlyLinksShort)
            {

                string present = yearlyFileNames.FirstOrDefault(s => s.Equals(item));
                if (present == null)
                {
                    yearly = true;
                    Yearly_Compressed_Files(item );
                    yearly = false;
                }
                cnt++;
            }

            string strDate = LastUPDATEFileUploaded();

            DateTime currDate = GetDate(strDate);
            int startInd = weeklyLinks.FirstOrDefault().LastIndexOf("/");
            weeklyLinks.Where(y => GetDate(y) > currDate).ToList().ForEach(x => Weekly_Update_files(x.Substring(startInd + 1, x.Length - startInd-1)));

            Logger.LogMessageLine("Finished all RCT Score updates");

        }

        private static List<string> GetAllYearlyFiles()
        {
            
            List<string> fileNames = new List<string>();
            try
            {
                using (SqlConnection conn = new SqlConnection("Server = localhost; Database = DataService; Integrated Security = True; "))
                {
                    conn.Open();
                    var res = SqlHelper.ExecuteQuerySP(conn, "[dbo].[st_RCT_GET_LATEST_YEARLY_FILE_NAMES]");
                    if (res.HasRows)
                    {
                        while (res.Read())
                        {
                            fileNames.Add(res["RCT_FILE_NAME"].ToString());
                        }
                        res.Close();
                    }
                }
            }
            catch (SqlException sqlex)
            {
                Logger.LogSQLException(sqlex, "");

            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "");

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
