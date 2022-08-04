using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Microsoft.Azure.DataLake.Store;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Rest.Azure.Authentication;
using Microsoft.Extensions.Configuration;
using EPPIDataServices.Helpers;
using Microsoft.Extensions.Logging;
using Serilog;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;

namespace AcademicImport
{
    public class Program
    {
        public static SQLHelper SqlHelper;

        private static string CreateLogFileName()
        {
            DirectoryInfo logDir = System.IO.Directory.CreateDirectory("LogFiles");
            string LogFilename = logDir.FullName + @"\" + "AcademicImportLog-" + DateTime.Now.ToString("dd-MM-yyyy") + ".txt";
            if (!System.IO.File.Exists(LogFilename)) System.IO.File.Create(LogFilename);
            return LogFilename;
        }

        // Change these settings for different install locations
        static bool isAlpha = false;

        public static void Main(string[] args)
        {
            string writeToThisFolder = @"E:\MSAcademic\downloads";
            int limit = 0; // ********** use this for testing. 0 = get everything

            if (isAlpha)
            {
                writeToThisFolder = @"L:\MSAcademic\downloads";
            }
            Console.WriteLine("Now opening and writing papers file");
            var fileStream = new FileStream(writeToThisFolder + @"\papers.txt", FileMode.Open, FileAccess.Read);
            using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
            {
                using (StreamWriter outputFile = new StreamWriter(writeToThisFolder + @"\papers2.txt"))
                {
                    string line;
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        string[] f = line.Split('\t');
                        line += '\t' + Truncate(ToShortSearchText(f[4]), 500);
                        outputFile.WriteLine(line);
                    }
                }
            }
            Console.WriteLine("Now opening and writing abstracts file");
            var fileStream2 = new FileStream(writeToThisFolder + @"\PaperAbstractsInvertedIndex.txt", FileMode.Open, FileAccess.Read);
            using (var streamReader = new StreamReader(fileStream2, Encoding.UTF8))
            {
                using (StreamWriter outputFile = new StreamWriter(writeToThisFolder + @"\PaperAbstractsInvertedIndex2.txt"))
                {
                    string line;
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        string[] f = line.Split('\t');
                        var j = (JObject)JsonConvert.DeserializeObject(f[1]);
                        int indexLength = j["IndexLength"].ToObject<int>();
                        Dictionary<string, int[]> invertedIndex = j["InvertedIndex"].ToObject<Dictionary<string, int[]>>();
                        string reconstructedAbstract = ReconstructInvertedAbstract(indexLength, invertedIndex);
                        line = f[0] + '\t' + reconstructedAbstract.Replace(Environment.NewLine, " ").Replace("\n", " ").Replace("\r", " ");
                        outputFile.WriteLine(line);
                    }
                }
            }
            Console.WriteLine("All done");
            Console.ReadLine();
        }

        public static void Main_old(string[] args)
        {

            // Starting from here ==============================================

            // Required for SERILOG 
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File(CreateLogFileName())
                .CreateLogger();

            string appdata = Environment.GetEnvironmentVariable(System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows) ? "APPDATA" : "Home");

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"{appdata}\\Microsoft\\UserSecrets\\AcademicImport.appsettings.User.json", optional: true);

            IConfigurationRoot configuration = builder.Build();

            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            var serviceProvider = serviceCollection.BuildServiceProvider();

            var _logger = serviceProvider.GetService<ILogger<Program>>();

            SqlHelper = new SQLHelper(configuration, _logger);

            // Example of dotnetcore logging: see methods available to _logger.
            _logger.LogInformation(Environment.NewLine);
            _logger.LogInformation(Environment.NewLine);
            _logger.LogInformation(Environment.NewLine);
            _logger.LogInformation("MAG import starts...");

            string blobConnection = configuration["AppSettings:blobConnection"]; 

            

            string writeToThisFolder = @"E:\MSAcademic\downloads";
            string SqlScriptFolder = @"\SQLScripts\";
            int limit = 0; // ********** use this for testing. 0 = get everything

            if (isAlpha)
            {
                writeToThisFolder = @"L:\MSAcademic\downloads";
                SqlScriptFolder = @"\SqlScripts\";
            }

            // Maybe ending here could be in a configuration method ===============================


            // start off by connecting to existing SQL database and getting the name of the datalake folder that was used to create it (this is the date of last update)

            // Get storage account
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(blobConnection);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            try
            {
                CloudBlobContainer latest = null;
                // Enumerate through directory on DataLake and find the most recent dataset
                foreach (CloudBlobContainer entry in blobClient.ListContainers())
                {
                    if (latest == null)
                    {
                        latest = entry;
                    }
                    else
                    {
                        if (Convert.ToInt32(entry.Name.Replace("mag-", "").Replace("-", "")) > 
                            Convert.ToInt32(latest.Name.Replace("mag-", "").Replace("-", "")))
                        {
                            latest = entry;
                        }
                    }
                }

                // When this is a service ADD a check here to see whether we have a new dataset now by comparing
                // with the record that we grabbed from the existing SQL DB above

                // if the most recent on DataLake == our current DB - just exit here

                // if we have a new dataset on DataLake to bring down we have work to do...


                // 1. Create a new SQL Database using the SQL script.
                // TODO = CREATE DATABASE
                // at the moment we're assuming the database has been created


                // 2. Go through each file that we want to download.

                // empty the downloads folder
                Console.WriteLine("Deleting old files...");
                System.IO.DirectoryInfo di = new DirectoryInfo(writeToThisFolder);
                foreach (FileInfo file in di.GetFiles())
                {
                    file.Delete();
                }
                Console.WriteLine("");

                List<string> mag = new List<string>
                {
                    //"Affiliations",
                    //"Authors",
                    //"ConferenceInstances",
                    //"ConferenceSeries",
                    //"FieldsOfStudy",
                    //"Journals",
                    "PaperAuthorAffiliations",
                    "PaperReferences",
                    "PaperResources",
                    "Papers",
                    "PaperUrls"
                };
                List<string> advanced = new List<string>
                {
                    "FieldOfStudyChildren",
                    "PaperFieldsOfStudy",
                    "PaperRecommendations",
                    "RelatedFieldOfStudy"
                };
                List<string> nlp = new List<string>
                {
                    "PaperAbstractsInvertedIndex" //,
                    //"PaperCitationContexts",
                    //"PaperLanguages"
                };

                Console.WriteLine("Starting file download at: " + DateTime.Now.ToString());
                foreach (var item in mag)
                {
                    DownloadThisFile(latest, @"mag/", item + ".txt", writeToThisFolder, limit);
                }
                foreach (var item in advanced)
                {
                    DownloadThisFile(latest, @"advanced/", item + ".txt", writeToThisFolder, limit);
                }
                foreach (var item in nlp)
                {
                    DownloadThisFile(latest, @"nlp/", item + ".txt", writeToThisFolder, limit);
                }
                Console.WriteLine("");
                // once we've downloaded the files, put them into the SQL DB

                // Create all the tables
                Console.WriteLine("Creating tables...");
                using (SqlConnection conn = new SqlConnection(SqlHelper.DataServiceDB))
                {

                    conn.Open();
                    SqlHelper.ExecuteNonQueryNonSP(conn, File.ReadAllText(AppContext.BaseDirectory + SqlScriptFolder + "CreateTables.sql"));
                    SqlHelper.ExecuteNonQueryNonSP(conn, "DROP PROCEDURE IF EXISTS [BulkTextUpload]");
                    SqlHelper.ExecuteNonQueryNonSP(conn, File.ReadAllText(AppContext.BaseDirectory + SqlScriptFolder + "BulkTextUpload.sql").Replace("writeToThisFolder", writeToThisFolder));
                    Console.WriteLine("");

                    // put the files into the DB
                    foreach (var item in mag)
                    {
                        UploadToDatabase(conn, item);
                    }
                    Console.WriteLine("");

                    // CREATE INDEXES ON THE APPROPRIATE TABLES / FIELDS
                    CreateIndexes(conn, SqlScriptFolder);

                }

                // Once the file has been put into the DB, delete it from the local filesystem. e.g.
                /*
                if (File.Exists(@"E:\MSAcademic\Affiliations.txt"))
                {
                    File.Delete(@"E:\MSAcademic\Affiliations.txt");
                }
                */

                // once we've finished downloading all the files, delete all the older databases on DataLake, so we only have the most recent one being stored
                // I think keep the most recent one, in case we need to rebuild the DB for any reason??

                //foreach (DirectoryEntry entry in client.EnumerateDirectory("/graph"))
                //{
                    // store in a string variable the name of the most recent directory
                    // if each directory is not the most current, then delete it using this command
                    // client.DeleteRecursive("/graph/directoryname");
                    // when this service is up and running, there will only ever be one directory deleted
                    // hopefully this doesn't mess up the Enumerable, but if it does, we'll need to enumerate and store a list of folders to delete
                //}

                // We've now got a new clean database. We need to:
                // 1. Record in the Reivewer DB (or somewhere??) the fact that it should now use the new academic SQL DB and not the old onw
                // 2. Delete the old SQL DB


                // We're done :)

            }
            catch (AdlsException e)
            {
                PrintAdlsException(e);
            }

            Console.WriteLine("Done. Press ENTER to continue ...");
            Console.ReadLine();
        }

        private static void ConfigureServices(ServiceCollection serviceCollection)
        {
            serviceCollection.AddLogging(configure => configure.AddConsole())
                .AddLogging(configure => configure.AddSerilog());
        }

        private static bool UploadToDatabase(SqlConnection conn, string FileName)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            Console.WriteLine("Uploading this file now: " + FileName);
            List<SqlParameter> sqlParams = new List<SqlParameter>();
            sqlParams.Add(new SqlParameter("@FileName", FileName));
            SqlParameter[] parameters = new SqlParameter[1];
            parameters = sqlParams.ToArray();
            SqlHelper.ExecuteNonQuerySP(conn, "BulkTextUpload", parameters);
            sw.Stop();
            Console.WriteLine("That took: " + sw.Elapsed);
            return true;
        }

        private static bool CreateIndexes(SqlConnection conn, string SqlScriptFolder)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            Console.WriteLine("Creating indexes...: ");
            SqlHelper.ExecuteNonQueryNonSP(conn, File.ReadAllText(AppContext.BaseDirectory + SqlScriptFolder + "CreateIndexes.sql"));
            sw.Stop();
            Console.WriteLine("Creating indexes took: " + sw.Elapsed);
            return true;
        }

        private static bool DownloadThisFile(CloudBlobContainer container, string graphPath, string fileName, string folder, int limit)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            Int64 count = 0;
            Int64 tempLimit = limit == 0 ? 5000000000 : limit; // i.e. if limit == 0 we download everything. less for testing
            Console.WriteLine("Reading this file now: " + fileName);
            CloudBlockBlob cbb = container.GetBlockBlobReference(graphPath + fileName);
            BlobRequestOptions requestOptions = new BlobRequestOptions();
            requestOptions.MaximumExecutionTime = TimeSpan.FromHours(6);
            cbb.DownloadToFile(folder + @"/" + fileName, FileMode.Create, null, requestOptions);

            /* The below is for downloading a line at a time. 
            CloudBlob blob = container.GetBlobReference(graphPath + fileName);
            using (var readStream = blob.OpenRead())
            {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(folder + @"/" + fileName))
                {
                    using (StreamReader reader = new StreamReader(readStream))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null && count < tempLimit)
                        {
                            if (fileName == "/Papers.txt")
                            {
                                string[] f = line.Split('\t');
                                line += '\t' + Truncate(ToShortSearchText(f[4]), 500);
                            }
                            if (fileName == "/PaperAbstractsInvertedIndex.txt")
                            {
                                string[] f = line.Split('\t');
                                var j = (JObject)JsonConvert.DeserializeObject(f[1]);
                                int indexLength = j["IndexLength"].ToObject<int>();
                                Dictionary<string, int[]> invertedIndex = j["InvertedIndex"].ToObject<Dictionary<string, int[]>>();
                                string reconstructedAbstract = ReconstructInvertedAbstract(indexLength, invertedIndex);
                                line = f[0] + '\t' + reconstructedAbstract.Replace(Environment.NewLine, " ").Replace("\n", " ").Replace("\r", " ");
                            }
                            file.WriteLine(line);
                            count++;
                        }
                    }
                }
            }
            */
            sw.Stop();
            Console.WriteLine("That took: " + sw.Elapsed);
            return true;
        }

        public static string Truncate(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }

        private static readonly Lazy<Regex> alphaNumericRegex = new Lazy<Regex>(() => new Regex("[^a-zA-Z0-9]"));

        public static string RemoveDiacritics(string stIn)
        {
            string stFormD = stIn.Normalize(NormalizationForm.FormD);
            StringBuilder sb = new StringBuilder();
            for (int ich = 0; ich < stFormD.Length; ich++)
            {
                UnicodeCategory uc = CharUnicodeInfo.GetUnicodeCategory(stFormD[ich]);
                if (uc != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(stFormD[ich]);
                }
            }
            return (sb.ToString().Normalize(NormalizationForm.FormC));
        }

        public static string ToShortSearchText(string s)
        {
            return ToSimpleText(RemoveDiacritics(s))
                .Replace("a", "")
                .Replace("e", "")
                .Replace("i", "")
                .Replace("o", "")
                .Replace("u", "");
            //.Replace("ize", "")
            //.Replace("ise", "");
        }

        public static string ToSimpleText(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return "";
            return alphaNumericRegex.Value.Replace(s, "").ToLower();

        }

        public static string ReconstructInvertedAbstract(int indexLength, Dictionary<string, int[]> invertedIndex)
        {
            string[] abstractStr = new string[indexLength];
            foreach (var pair in invertedIndex)
            {
                string word = pair.Key;
                foreach (var index in pair.Value)
                {
                    abstractStr[index] = word;
                }
            }
            return String.Join(" ", abstractStr);
        }

        private static void PrintAdlsException(AdlsException exp)
        {
            Console.WriteLine("ADLException");
            Console.WriteLine($"   Http Status: {exp.HttpStatus}");
            Console.WriteLine($"   Http Message: {exp.HttpMessage}");
            Console.WriteLine($"   Remote Exception Name: {exp.RemoteExceptionName}");
            Console.WriteLine($"   Server Trace Id: {exp.TraceId}");
            Console.WriteLine($"   Exception Message: {exp.Message}");
            Console.WriteLine($"   Exception Stack Trace: {exp.StackTrace}");
            Console.WriteLine();
        }
    }
}

// *********************************************************************************
// From here down - just potentially useful DataLake commands - nothing for our current script
// *********************************************************************************

// Create a file - automatically creates any parent directories that don't exist
// The AdlsOuputStream preserves record boundaries - it does not break records while writing to the store
//using (var stream = client.CreateFile(fileName, IfExists.Overwrite))
//{
//    byte[] textByteArray = Encoding.UTF8.GetBytes("This is test data to write.\r\n");
//    stream.Write(textByteArray, 0, textByteArray.Length);

//    textByteArray = Encoding.UTF8.GetBytes("This is the second line.\r\n");
//    stream.Write(textByteArray, 0, textByteArray.Length);
//}

//// Append to existing file
//using (var stream = client.GetAppendStream(fileName))
//{
//    byte[] textByteArray = Encoding.UTF8.GetBytes("This is the added line.\r\n");
//    stream.Write(textByteArray, 0, textByteArray.Length);
//}

// Get the properties of the file
//var directoryEntry = client.GetDirectoryEntry(fileName);
//PrintDirectoryEntry(directoryEntry);

//// Rename a file
//string destFilePath = "/Test/testRenameDest3.txt";
//client.Rename(fileName, destFilePath, true);

//// Enumerate directory
//foreach (var entry in client.EnumerateDirectory("/Test"))
//{
//    PrintDirectoryEntry(entry);
//}

//// Delete a directory and all it's subdirectories and files
//client.DeleteRecursive("/Test");