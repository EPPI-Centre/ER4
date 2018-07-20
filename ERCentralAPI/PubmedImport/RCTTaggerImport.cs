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
using System.Security.Cryptography;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections;

namespace PubmedImport
{

    public class RCTTaggerImport
    {
        private readonly ILogger _logger;
        public PubMedUpdateFileImportJobLog _jobLogResult;

        public RCTTaggerImport(ILogger<EPPILogger> logger)
        {
            _logger = logger;
        }

         SQLHelper SqlHelper;

        private  int upMonth;

        private  int loMonth;

        private  readonly int maxRequestTries = 3;

         string TmpFolderPath;

         bool yearly = false;

        public  string StringTrimmer(string inputStr, string Ex)
        {
            int tmp = inputStr.LastIndexOf(Ex);
            string tmpStr = inputStr.Substring(tmp + 1, inputStr.Length - tmp - 1);
            return tmpStr;
        }

        public DateTime GetDate(string str)
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
                    string tmpStr = Regex.Match(str, @"\d{4}").Value;
                    DateTime tmpDate = DateTime.Parse("31-12-" + tmpStr);
                    return tmpDate;
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Format of files from arrowsmith must have changed; converting to year errors");
               
                return DateTime.Now;
            }
        }

        private string GetYear(string str)
        {
            try
            {
                int count = str.Count(Char.IsDigit);
                if (count > 7)
                {
                    return Regex.Match(StringTrimmer(str, "-"), @"\d{4}").Value;
                }
                else
                {
                    return Regex.Match(str, @"\d{4}").Value;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Format of files from arrowsmith must have changed; converting to year errors");
                
                return "";
            }

        }

        private string GetMonth(string str)
        {
            try
            {
                return Regex.Match(str, @"\d{2}").Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Format of files from arrowsmith must have changed; converting to year errors");
                return "";
            }
        }

        private string GetDay(string str)
        {
            try
            {
                return Regex.Match(str, @"\d{2}").Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Format of files from arrowsmith must have changed; converting to year errors");
                return "";
            }
        }

        private void Yearly_Compressed_Files(PubMedUpdateFileImportJobLog jobLogResult, string yearlyfile)
        {

            bool res = false;
            string fileName = "";
            try
            {
                (res, fileName) = DownloadCSVFiles(yearlyfile);
                if (res == false) { return; }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Format of files from arrowsmith must have changed; converting to year errors");
                throw;
            }

            _logger.LogInformation("Decompressing the yearly gz file");
            string decompressedYearlyFile = Decompress(yearlyfile);

            if (decompressedYearlyFile is null)
            {
                _logger.LogInformation("Error with decompressing file");
                return;
            }
            _logger.LogInformation("Importing the yearly gz file into SQL");

            if (yearlyfile.Contains("rct"))
            {
                Import_Tag_File< RCT_Tag>(jobLogResult, decompressedYearlyFile, typeof(RCT_Tag));
            }
            else
            {
                Import_Tag_File<Human_Tag>(jobLogResult, decompressedYearlyFile, typeof(Human_Tag));
            }
            _logger.LogInformation("Finished with this yearly import file");

        }

        private async Task<List<string>> GetHTMLLinksAsync(string baseURL)
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
                _logger.LogError(ex, "error scraping links");

                links.Add("error scraping links");

            }
            return links;
        }

        private void Weekly_Update_files(PubMedUpdateFileImportJobLog jobLogResult, string weeklyFile)
        {
            _logger.LogInformation("Checking weekly update files");

            bool res = false;
            _logger.LogInformation("Importing weekly update files");

            (res, weeklyFile) = DownloadCSVFiles(weeklyFile);
            if (res)
            {
                if (weeklyFile.Contains("rct"))
                {
                    Import_Tag_File<RCT_Tag>(jobLogResult, weeklyFile, typeof(RCT_Tag));
                    
                }
                else
                {
                    Import_Tag_File<Human_Tag>(jobLogResult, weeklyFile, typeof(Human_Tag));
                   
                }
            }
            _logger.LogInformation("Finished with this weekly update file");
        }

        private void Log_Import_Job( string filename)
        {

            _logger.LogInformation("Scores have been imported into SQL for the following file: " + filename);

            using (SqlConnection conn = new SqlConnection(SqlHelper.DataServiceDB))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();
                List<SqlParameter> sqlParams = new List<SqlParameter>();
                try
                {

                    filename = StringTrimmer(filename, "/");
                    filename = StringTrimmer(filename, @"\");
                    int count = filename.Count(Char.IsDigit);

                    DateTime date = DateTime.Now;
                    if (filename == Program.ArrowsmithRCTBaselineFile)
                    {
                        // long yearly file en-GB
                        System.Globalization.CultureInfo UK = System.Globalization.CultureInfo.CreateSpecificCulture("en-GB");
                        date = DateTime.Parse("31-12-2016", UK);
                        sqlParams.Add(new SqlParameter("@RCT_FILE_NAME", filename + ".gz"));
                        sqlParams.Add(new SqlParameter("@RCT_IMPORT_DATE", DateTime.Now));
                        sqlParams.Add(new SqlParameter("@RCT_UPLOAD_DATE", date));
                    }
                    else if (yearly == true)
                    {
                        //short yearly file
                        string tempStr = GetYear(filename) + "-12-30";
                        date = Convert.ToDateTime(tempStr);
                        sqlParams.Add(new SqlParameter("@RCT_FILE_NAME", filename + ".gz"));
                        sqlParams.Add(new SqlParameter("@RCT_IMPORT_DATE", DateTime.Now));
                        sqlParams.Add(new SqlParameter("@RCT_UPLOAD_DATE", date));

                    }
                    else
                    {
                        //weekly file
                        sqlParams.Add(new SqlParameter("@RCT_FILE_NAME", filename));
                        sqlParams.Add(new SqlParameter("@RCT_IMPORT_DATE", DateTime.Now));
                        sqlParams.Add(new SqlParameter("@RCT_UPLOAD_DATE", date));

                    }


                    SqlParameter[] parameters = new SqlParameter[3];
                    parameters = sqlParams.ToArray();

                    int res = SqlHelper.ExecuteNonQuerySP(conn.ConnectionString, "[dbo].[st_RCT_IMPORT_UPDATE_INSERT]", parameters);

                    //===============================================
                    _logger.LogInformation("Job record inserted into DB");

                    transaction.Commit();

                }
                catch (SqlException sqlex)
                {
                    _logger.LogError(sqlex, "", sqlParams.ToArray());
                    transaction.Rollback();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "");
                    transaction.Rollback();
                }
                conn.Close();
            }
        }

        public  string UnZip(string value)
        {
            //Transform string into byte[]
            byte[] byteArray = new byte[value.Length];
            int indexBA = 0;
            foreach (char item in value.ToCharArray())
            {
                byteArray[indexBA++] = (byte)item;
            }

            //Prepare for decompress
            System.IO.MemoryStream ms = new System.IO.MemoryStream(byteArray);
            System.IO.Compression.GZipStream sr = new System.IO.Compression.GZipStream(ms,
                System.IO.Compression.CompressionMode.Decompress);

            //Reset variable to collect uncompressed result
            byteArray = new byte[byteArray.Length];

            //Decompress
            int rByte = sr.Read(byteArray, 0, byteArray.Length);

            //Transform byte[] unzip data to string
            System.Text.StringBuilder sB = new System.Text.StringBuilder(rByte);
            //Read the number of bytes GZipStream red and do not a for each bytes in
            //resultByteArray;
            for (int i = 0; i < rByte; i++)
            {
                sB.Append((char)byteArray[i]);
            }
            sr.Close();
            ms.Close();
            sr.Dispose();
            ms.Dispose();
            return sB.ToString();
        }

        private  string Decompress(string yearlyFile)
        {
            yearlyFile = StringTrimmer(yearlyFile, "/");

            string unZippedFileName = TmpFolderPath + "\\" + yearlyFile.Substring(0, yearlyFile.Length - 3);
            FileInfo fileToBeUnGZipped = new FileInfo(TmpFolderPath + "\\" + yearlyFile);
            if (!fileToBeUnGZipped.Exists)
            {
                return null;
            }

            unZippedFileName = unZippedFileName.Replace("\\", "//");
            _logger.LogInformation("Decompressing " + yearlyFile + "....");

            using (FileStream fileToDecompressAsStream = fileToBeUnGZipped.OpenRead())
            {
                string decompressedFileName = unZippedFileName;

                using (FileStream decompressedStream = File.Create(decompressedFileName))
                {
                    using (GZipStream decompressionStream = new GZipStream(fileToDecompressAsStream, CompressionMode.Decompress))
                    {
                        try
                        {
                            decompressionStream.CopyTo(decompressedStream);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
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
                    _logger.LogError(ex, "deleting the compressed file.");
                }
            }
            return unZippedFileName;
        }

        private  (bool, string) DownloadCSVFiles(string filename)
        {
            bool success = false;

            string url = filename;
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
                        _logger.LogInformation("Downloaded the following file: " + filename);
                        return (success, filename);
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "HTTP request error");
                    exceptions.Add(e);
                }
            }
            while (remainingTries > 0);
            return (false, filename);
        }
        
        private void Import_Tag_File<T>(PubMedUpdateFileImportJobLog jobLogResult, string filename, Type value)
        {
            DataTable dt = new DataTable();

            List<Tag> recs = new List<Tag>();

            // After downloading the file import at that point
            SqlTransaction transaction;
            FileParserResult fileParser = new FileParserResult(filename, false);
            fileParser.StartTime = DateTime.Now;

            using (SqlConnection conn = new SqlConnection(SqlHelper.DataServiceDB))
            {
                conn.Open();
                int todo = 0;
                List<string> messages = new List<string>();
                messages.Add("Imported scores for the following file: " + filename);
                try
                {
                    if (filename.Contains("\\"))
                    {
                        filename = filename.Replace("\\", "//");
                    }

                    if (!filename.Contains("//"))
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
                                if (value ==  typeof(RCT_Tag))
                                {
                                   
                                    RCT_Tag record = new RCT_Tag();
                                    record.PMID = _values[0];
                                    record.RCT_SCORE = _values[1];
                                    recs.Add(record);
                                }
                                else
                                {
                                    
                                    Human_Tag record = new Human_Tag();
                                    record.PMID = _values[0];
                                    record.HUMAN_PRECICTION = _values[1];
                                    recs.Add(record);
                                }

                            }
                            x++;
                        }
                        sr.Close();

                    }


                    double divisor = 0.0;
                    int skip = 0;
                    int page = 0;
                    int done = 0;

                    if (recs.Count < 1000)
                    {
                        divisor = 0;
                        page = recs.Count;
                    }
                    else
                    {
                        divisor = Math.Floor((double)recs.Count / 1000);
                        page = 1000;
                        skip = 1000;
                    }
                    for (int i = 0; i <= divisor; i++)
                    {
                        List<SqlParameter> sqlParams = new List<SqlParameter>();
                        transaction = conn.BeginTransaction();
                        try
                        {

                            List<string> tmpL = recs.Select(x => x.ID).Skip(skip * i).Take(page).ToList();
                            sqlParams.Add(new SqlParameter("@ids", string.Join(",", tmpL)));
                            sqlParams.Add(new SqlParameter("@scores", string.Join(",", recs.Select(x => x.Score).Skip(skip * i).Take(page).ToList())));
                            sqlParams.Add(new SqlParameter("@ID", "RCT"));

                            int res = SqlHelper.ExecuteNonQuerySP(conn.ConnectionString, "[dbo].[st_ReferenceUpdate_Arrow_Scores]", sqlParams.ToArray());

                            transaction.Commit();
                            done += tmpL.Count();
                            messages.Add(" " + done);


                        }
                        catch (SqlException sqlex)
                        {
                            _logger.LogError(sqlex, "", sqlParams.ToArray());
                            transaction.Rollback();
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "");
                            transaction.Rollback();
                        }
                    }

                    todo = recs.Count;

                    _logger.LogInformation("The total number of scores updated is: " + done);

                    _logger.LogInformation("Successfully imported an RCT file");

                    fileParser.Messages = messages;
                    fileParser.EndTime = DateTime.Now;
                    fileParser.CitationsInFile = todo;
                    fileParser.CitationsCommitted = todo;


                    jobLogResult.ProcessedFilesResults.Add(fileParser);

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Catch all try block");
                }
                finally
                {
                    // Log job
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
        
     
        private  (bool, string) Execute(Uri urlCheck, string fileName)
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

                fileName = StringTrimmer(fileName, "/");

                // This destination path needs to be sorted out...
                string _destinationPath = TmpFolderPath + "\\" + fileName + "";
                if (File.Exists(_destinationPath)) return (true, fileName);
                else
                {
                    _client.DownloadFile(urlCheck, _destinationPath); //Download the file. 

                    return (true, fileName);
                }
            }
            else
            {
                _logger.LogInformation("Error Contacting server");
                return (false, "error");
            }
        }

        private  string LatestRctUPDATEFile()
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
                            fileName = res["FILE_NAME"].ToString();
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
                _logger.SQLActionFailed("", null, sqlex);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "");

            }
            return fileName;

        }

        private  string LatestHumanUPDATEFile()
        {
            string fileName = "";
            try
            {
                using (SqlConnection conn = new SqlConnection("Server=localhost; Database = DataService; Integrated Security = True; "))
                {
                    conn.Open();
                    var res = SqlHelper.ExecuteQuerySP(conn, "[dbo].[st_HUMAN_GET_LATEST_UPLOAD_FILE_NAME]");
                    if (res.HasRows)
                    {
                        while (res.Read())
                        {
                            fileName = res["FILE_NAME"].ToString();
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
                _logger.SQLActionFailed("LatestHumanUPDATEFile(): ", null,  sqlex);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "");

            }
            return fileName;
        }

        public void RunRCTTaggerImport( ServiceProvider serviceProvider, PubMedUpdateFileImportJobLog jobLogResult)
        {
            _jobLogResult = jobLogResult;
            // Setup the logger
            var _logger = serviceProvider.GetService<ILogger<EPPILogger>>();
            
            // Warning this a terrible way to do this; if Arrowsmith change the format
            // of their links all of this will break.
            DirectoryInfo tempDir = System.IO.Directory.CreateDirectory("Tmpfiles");
            TmpFolderPath = tempDir.FullName;

            Task<List<string>> taskRCT = GetHTMLLinksAsync(Program.ArrowsmithRCTbaseURL);
            taskRCT.Wait();
            List<string> htmlRCTLinks = taskRCT.Result.Where(x => x.Contains("arrowsmith")).ToList();

            Task<List<string>> taskHuman = GetHTMLLinksAsync(Program.ArrowsmithHumanbaseURL);
            taskHuman.Wait();
            List<string> htmlHumanLinks = taskHuman.Result.Where(x => x.Contains("arrowsmith")).ToList();

            //Logger = Program.Logger;

            SqlHelper = Program.SqlHelper;

            _logger.LogInformation("Checking for new yearly import files");

            List<string> yearlyRCTLinks = htmlRCTLinks.Where(x => x.Contains(".gz")).ToList();
            List<string> weeklyRCTLinks = htmlRCTLinks.Where(x => !x.Contains(".gz")).ToList();

            List<string> yearlyHumanLinks = htmlHumanLinks.Where(x => x.Contains(".gz")).ToList();
            List<string> weeklyHumanLinks = htmlHumanLinks.Where(x => !x.Contains(".gz")).ToList();


            if (yearlyRCTLinks.Count() > 0)
            {
                yearlyRCTLinks.Reverse();
            }
            List<string> yearlyFilesDownloadedFullPath = GetAllYearlyFiles();
            List<string> yearlyFileNames = new List<string>();
            List<string> yearlyLinksShort = new List<string>();
            int cnt = 0;
            foreach (var item in yearlyRCTLinks)
            {
                yearlyLinksShort.Add(StringTrimmer(item, "/"));
                cnt++;
            }
            cnt = 0;
            if (yearlyFilesDownloadedFullPath.Count > 0)
            {
                foreach (var item in yearlyFilesDownloadedFullPath)
                {
                    string tmpStr = item.Substring(9, item.Length - 9);
                    yearlyFileNames.Add(item);
                    cnt++;
                }
            }
            cnt = 0;
            foreach (var item in yearlyLinksShort)
            {
                
                string present = yearlyFileNames.FirstOrDefault(s => s.Equals(item));
                if (present == null)
                {
                    _logger.LogInformation("Downloading, decompressing and importing into SQL this yearly RCT import file: " + item);
                    yearly = true;
                    string tmpItem = Program.ArrowsmithRCTfileBaseURL + "/" + item;
                    Yearly_Compressed_Files(_jobLogResult, tmpItem);
                    yearly = false;
                }
                cnt++;
            }
            string strDate = LatestRctUPDATEFile();

            DateTime currDate = GetDate(strDate);
            int startInd = weeklyRCTLinks.FirstOrDefault().LastIndexOf("/");
            weeklyRCTLinks.Where(y => GetDate(y) > currDate).ToList().ForEach(x => Weekly_Update_files(_jobLogResult, Program.ArrowsmithRCTfileBaseURL + x.Substring(startInd + 1, x.Length - startInd - 1)));

            _logger.LogInformation("Finished all RCT Score updates");
            _logger.LogInformation("Starting Human score imports");

            List<string> yearlyFilesHumanDownloadedFullPath = GetAllYearlyFiles().Where(x => x.Contains("human") || x.Contains("tagger")).ToList();
            List<string> yearlyHumanFileNames = new List<string>();
            List<string> yearlyHumanLinksShort = new List<string>();
            cnt = 0;
            foreach (var item in yearlyHumanLinks)
            {
                yearlyHumanLinksShort.Add(StringTrimmer(item, "/"));
                cnt++;
            }
            cnt = 0;
            if (yearlyFilesHumanDownloadedFullPath.Count() > 0)
            {
                foreach (var item in yearlyFilesHumanDownloadedFullPath)
                {
                    string tmpStr = item.Substring(9, item.Length - 9);
                    yearlyHumanFileNames.Add(item);
                    cnt++;
                }

            }

            cnt = 0;
            foreach (var item in yearlyHumanLinksShort)
            {
                string present = yearlyHumanFileNames.FirstOrDefault(s => s.Equals(item));
                if (present == null)
                {
                    _logger.LogInformation("Downloading, decompressing and importing into SQL this HUMAN tagger yearly import file: " + item);
                    yearly = true;
                    string tmpItem = Program.ArrowsmithHumanbaseURL + "/" + item;
                    Yearly_Compressed_Files(_jobLogResult, tmpItem);
                    yearly = false;
                }
                cnt++;
            }

            strDate = LatestHumanUPDATEFile();

            currDate = GetDate(strDate);
            startInd = weeklyHumanLinks.FirstOrDefault().LastIndexOf("/");
            weeklyHumanLinks.Where(y => GetDate(y) > currDate).ToList().ForEach(x => Weekly_Update_files(_jobLogResult, Program.ArrowsmithHumanURL + x.Substring(startInd + 1, x.Length - startInd - 1)));

            _logger.LogInformation("Finished all HUMAN Score updates");

            _logger.LogInformation("Logging all file results into SQL");

            _jobLogResult.EndTime = DateTime.Now;
        }

        private  List<string> GetAllYearlyFiles()
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
                            fileNames.Add(res["FILE_NAME"].ToString());
                        }
                        res.Close();
                    }
                }
            }
            catch (SqlException sqlex)
            {
                _logger.SQLActionFailed("", null, sqlex);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "");

            }
            return fileNames;
        }
    }

    class Tag
    {
        public string ID { get; set; }
        public string Score { get; set; }
    }

    class RCT_Tag : Tag
    {
        public string RCT_SCORE
        {
            get { return Score; }
            set { Score = value; }
        }

        public string PMID
        {
            get { return ID; }
            set { ID = value; }
        }
    }

    class Human_Tag : Tag
    {
        public string HUMAN_PRECICTION
        {
            get { return Score; }
            set { Score = value; }
        }

        public string PMID
        {
            get { return ID; }
            set { ID = value; }
        }
    }

}
