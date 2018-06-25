using CsvHelper;
using CsvHelper.Configuration;
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

        static void Main(string[] args)
        {
            Console.WriteLine("Checking for new yearly import files");
            (bool check, string yearlyFile) = CheckYearlyFiles();
            if (check)
            {
                // There is a new yearly file to be imported
                Console.WriteLine("Decompressing the yearly gz file");
                string decompressedYearlyFile = Decompress(yearlyFile);

                Console.WriteLine("Importing the yearly gz file into SQL");
                Import_Yearly_RCT_Tagger(decompressedYearlyFile);

                Console.WriteLine("Finished with yearly import");
            }
            else if(check == false && yearlyFile != "error")
            {
                // Consider the update files
                Console.WriteLine("Checking update files");
                List<string> Update_Files = checkUpdateFiles();

                Console.WriteLine("Importing update files");
                Import_Update_Files(Update_Files);

                Console.WriteLine("Finished with update imports");
            }
            else
            {
                Console.WriteLine("Request has timed out to arrowsmith server");
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
            Console.WriteLine("Decompressing " + yearlyFile + "....");
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
                            Console.WriteLine(ex.InnerException.ToString(), "uncompressing file to parse.");
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

                    Console.WriteLine(ex.InnerException.ToString(), "deleting the compressed file.");
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

            string tablename = "[DataService].[dbo].[TB_TMP_CSV]";

            // Whilst testing mock fake file, 
            // after server comes back, remove this part
            decompressedFile = Directory.GetCurrentDirectory()  +  "\\testRCT.csv";
            IEnumerable<RCT_Tag> recs;
            var RCT_TABLE = new DataTable();

            // Look at how to import csv into SQL in the right way for our tables....
            SqlConnection conn = new SqlConnection("Server = localhost; Database = DataService; Integrated Security = True; ");
            conn.Open();
            SqlTransaction transaction = conn.BeginTransaction();
            try
            {

                using (var sr = new StreamReader(@"D:\Github\eppi\ERCentralAPI\RCT_Tagger_Import\Files\testRCT.csv"))
                {
                    var reader = new CsvReader(sr);

                    //CSVReader will now read the whole file into an enumerable
                    recs = reader.GetRecords<RCT_Tag>();


                    RCT_TABLE.Columns.Add("PMID", typeof(string));
                    RCT_TABLE.Columns.Add("RCT_SCORE", typeof(int));

                    foreach (var entity in recs)
                    {
                        var row = RCT_TABLE.NewRow();
                        row["PMID"] = entity.PMID;
                        row["RCT_SCORE"] = entity.RCT_SCORE;
                        RCT_TABLE.Rows.Add(row);
                    }
                }

                SqlBulkCopy copy = new SqlBulkCopy(conn, SqlBulkCopyOptions.KeepIdentity, transaction);
                copy.DestinationTableName = tablename;

                // want the csv converted to datatable and then use a sp to update the REFs table

                // WRITE TO OWN TABLE FOR NOW

                // WHEN ARROWSMITH SERVER IS BACK UP INSTEAD UPDATE REFERENCE TABLE WITH AN SP

                copy.ColumnMappings.Add("PMID", "PMID");
                copy.ColumnMappings.Add("RCT_SCORE", "RCT_SCORE");

                copy.WriteToServer(RCT_TABLE);

                transaction.Commit();

            }
            catch (Exception ex)
            {
                transaction.Rollback();
            }
            finally
            {
                conn.Close();
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
                Console.WriteLine(e.InnerException.ToString(), "Error exectuing SP: " + SPname);
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

                return (true, "");
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
                Console.WriteLine("Have the latest gz yearly file imported already!");
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
        public int RCT_SCORE { get; set; }
    }

    public sealed class MyClassMap : ClassMap<RCT_Tag>
    {
        public MyClassMap()
        {
            Map(m => m.PMID);
            Map(m => m.RCT_SCORE);
        }
    }


}
