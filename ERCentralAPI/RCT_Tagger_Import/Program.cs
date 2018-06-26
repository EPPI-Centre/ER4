using EPPIDataServices.Helpers;
using Microsoft.AspNetCore.Hosting.Server;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;


namespace RCT_Tagger_Import
{
    class Program
    {
        private static readonly int maxRequestTries = 3;

        public static string GetYear(string str)
        {
            return Regex.Match(str, @"\d{4}").Value;
        }
        static EPPILogger Logger;
        static SQLHelper SqlHelper;
        static void Main(string[] args)
        {
            Logger = new EPPILogger(false);
            SqlHelper = new SQLHelper(Logger);
            Logger.LogMessageLine("Checking for new yearly import files");
            (bool check, string yearlyFile) = CheckYearlyFiles();
            if (check)
            {
                // There is a new yearly file to be imported
                Logger.LogMessageLine("Decompressing the yearly gz file");
                string decompressedYearlyFile = Decompress(yearlyFile);

                Logger.LogMessageLine("Importing the yearly gz file into SQL");
                Import_Yearly_RCT_Tagger(decompressedYearlyFile);

                Logger.LogMessageLine("Finished with yearly import");
            }
            else if(check == false && yearlyFile != "error")
            {
                // Consider the update files
                Logger.LogMessageLine("Checking update files");
                List<string> Update_Files = checkUpdateFiles();

                Logger.LogMessageLine("Importing update files");
                Import_Update_Files(Update_Files);

                Logger.LogMessageLine("Finished with update imports");
            }
            else
            {
                Logger.LogMessageLine("Request has timed out to arrowsmith server");
                Console.ReadLine();
            }
        }

        private static string Decompress(string yearlyFile)
        {
            // Use gz decompression c# code already written in the
            // PubmedImport project
            string unZippedFileName = yearlyFile.Substring(0, yearlyFile.Length - 3);
            FileInfo fileToBeUnGZipped = new FileInfo(yearlyFile);
            //string decompressedFileName = Directory.GetCurrentDirectory() + @"\Files\" + unZippedFileName;
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

        private static void Import_Update_Files(List<string> update_Files)
        {
            // Use the url to download the files and an sql sp
            // to insert into the correct tables
            throw new NotImplementedException();
        }

        private static List<string> checkUpdateFiles()
        {
            // check the url to obtain a list of new update files
            // check the sql log table for which files are relevant
            throw new NotImplementedException();
        }

        private static void Import_Yearly_RCT_Tagger( string decompressedFile)
        {
            List<RCT_Tag> recs = new List<RCT_Tag>();
            var RCT_TABLE = new DataTable();
            SqlTransaction transaction;

            using (SqlConnection conn = new SqlConnection(SqlHelper.DataServiceDB))
            {

                conn.Open();
                try
                {
                    decompressedFile = @"D:\Github\eppi\ERCentralAPI\RCT_Tagger_Import\Files\rct_predictions_2017.csv";

                    using (var sr = new StreamReader(decompressedFile))
                    {
                        string strline = "";
                        string[] _values = null;

                        int x = 0;
                        while (!sr.EndOfStream)
                        {
                            strline = sr.ReadLine();
                            _values = strline.Split('\t');

                            if (x > 0 )
                            {
                                RCT_Tag record = new RCT_Tag();

                                record.PMID = _values[0];
                                //PMIDStr += "," + _values[0];
                                record.RCT_SCORE = _values[1];
                                //RCT_ScoreStr += "," + _values[1];

                                recs.Add(record);
                            }
                            x++;
                        }
                        sr.Close();
                    
                        //RCT_TABLE.Columns.Add("PMID", typeof(string));
                        //RCT_TABLE.Columns.Add("RCT_SCORE", typeof(string));

                        //foreach (var entity in recs)
                        //{
                        //    var row = RCT_TABLE.NewRow();
                        //    row["PMID"] = entity.PMID;
                        //    row["RCT_SCORE"] = entity.RCT_SCORE;
                        //    RCT_TABLE.Rows.Add(row);
                        //}
                    }

                    int skip = 100;
                    // Change this now to run Sergio's SP ================================================
                    // This is first attempt; check needs to be made on arithmetic of files
                    // Also a check needs to be made on the results in the SQL DB
                    for (int i = 1; i < Math.Floor((double)recs.Count()/1000); i++)
                    {
                        transaction = conn.BeginTransaction();

                        try
                        {
                            
                            List<SqlParameter> sqlParams = new List<SqlParameter>();

                            sqlParams.Add(new SqlParameter("@ids", string.Join(",", recs.Select(x => x.PMID).Skip(skip*i).Take(1000).ToList())));
                            sqlParams.Add(new SqlParameter("@scores", string.Join(",", recs.Select(x => x.RCT_SCORE).Skip(skip*i).Take(1000).ToList())));

                            SqlParameter[] parameters = new SqlParameter[2];
                            parameters = sqlParams.ToArray();

                            int res = SqlHelper.ExecuteNonQuerySP(conn.ConnectionString, "[dbo].[st_ReferenceUpdate_Arrow_Scores]", parameters);

                            //===============================================
                            Logger.LogMessageLine("Done " + i*skip + " records...");

                            transaction.Commit();

                        }
                        catch (Exception)
                        {
                            transaction.Rollback();
                        }
                    }

                }
                catch (Exception ex)
                {
                    Logger.LogMessageLine(ex.InnerException.ToString());
                }
                finally
                {
                    conn.Close();
                }
            }
        }

        public static SqlDataReader ExecuteQuerySP(SqlConnection connection, string SPname, params SqlParameter[] parameters)
        {
            try
            {
                using (SqlCommand command = new SqlCommand(SPname, connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }
                    return command.ExecuteReader();
                }
            }
            catch (Exception e)
            {
                Logger.LogException(e, "Error exectuing SP: " + SPname);
                return null;
            }
        }

        public static (bool, string) Execute(Uri urlCheck, string fileName)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlCheck);
            request.Timeout = 15000;
            request.Method = "HEAD";
            HttpWebResponse response;
            response = (HttpWebResponse)request.GetResponse();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                // Download the new yealry file ready for decompressing.
                WebClient _client = new WebClient();

                // This destination path needs to be sorted out...
                string _destinationPath = @"D:\Github\eppi\ERCentralAPI\RCT_Tagger_Import\Files\" + fileName + "";

                _client.DownloadFile(urlCheck, _destinationPath); //Download the file. 

                return (true, _destinationPath);
            }
            else
            {
                return (false, "error");
            }
        }

        private static (bool, string) CheckYearlyFiles()
        {
            // logic for searching URL for relevant files
            // using sql log table for relevant info
            long currentYear = Convert.ToInt32(DateTime.Now.Year) - 1;
            string fileName = "";
            using (SqlConnection conn = new SqlConnection("Server = localhost; Database = DataService; Integrated Security = True; "))
            {
                conn.Open();
                var res = ExecuteQuerySP(conn, "[dbo].[st_RCT_GET_LATEST_UPLOAD_FILE_NAME]", null);
                if (res.HasRows)
                {
                    while (res.Read())
                    {
                        fileName = res["RCT_FILE_NAME"].ToString();
                    }
                    // Call Close when done reading.
                    res.Close();
                }
                else
                {
                    fileName = "0000";
                }
            }

            // Compare fileName year with the current year...
            string latestSQLYear = Program.GetYear(fileName);
            if (Convert.ToInt64(latestSQLYear) >= Convert.ToInt64(currentYear))
            {
                Logger.LogMessageLine("Have the latest gz yearly file imported already!");
                return (false, "Yearly file imported already");
            }
            else
            {
                fileName  = "rct_predictions_" + currentYear + ".csv.gz";

                // build the url from the sql query result
                string url = "http://arrowsmith.psych.uic.edu/arrowsmith_uic/download/RCT_Tagger/rct_predictions_" + currentYear + ".csv.gz";
                Uri urlCheck = new Uri(url);
                
                // replace
                var remainingTries = maxRequestTries;
                var exceptions = new List<Exception>();
                do
                {
                    --remainingTries;
                    try
                    {
                        return Execute(urlCheck, fileName);
                    }
                    catch (Exception e)
                    {
                        exceptions.Add(e);
                    }
                }
                while (remainingTries > 0);

                return (false, "error");
            }
        }
    }

    public class RCT_Tag
    {
        public string PMID { get; set; }
        public string RCT_SCORE { get; set; }
    }

    //public class MyClassMap : ClassMap<RCT_Tag>
    //{
    //    public MyClassMap()
    //    {
    //        Map(m => m.PMID).Name("PMID");
    //        Map(m => m.RCT_SCORE).Name("RCT Prediction");
    //    }
    //}


}
