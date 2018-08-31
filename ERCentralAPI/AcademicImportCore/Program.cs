using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.DataLake.Store;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Rest.Azure.Authentication;
using Microsoft.Extensions.Configuration;
using EPPIDataServices.Helpers;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Sinks.File;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;

namespace AcademicImport
{
    public class Program
    {
        public static SQLHelper SqlHelper = null;

        private static string CreateLogFileName()
        {
            DirectoryInfo logDir = System.IO.Directory.CreateDirectory("LogFiles");
            string LogFilename = logDir.FullName + @"\" + "AcademicImportLog-" + DateTime.Now.ToString("dd-MM-yyyy") + ".txt";
            if (!System.IO.File.Exists(LogFilename)) System.IO.File.Create(LogFilename);
            return LogFilename;
        }

        public static void Main(string[] args)
        {
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
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var _logger = serviceProvider.GetService<ILogger<Program>>();
            SqlHelper = new SQLHelper(configuration, _logger); 
            //SqlHelper = new SQLHelper(configuration, null);

            string applicationId = configuration["AppSettings:applicationId"];     // Also called client id
            string clientSecret = configuration["AppSettings:clientSecret"];
            string tenantId = configuration["AppSettings:tenantId"];
            string adlsAccountFQDN = configuration["AppSettings:adlsAccountFQDN"];   // full account FQDN, not just the account name like example.azure.datalakestore.net


            // Change these settings for different install locations
            bool isAlpha = false;

            string writeToThisFolder = @"E:\MSAcademic\downloads";
            string SqlScriptFolder = @"\SQLScripts\";
            int limit = 0; // ********** use this for testing. 0 = get everything

            if (isAlpha)
            {
                writeToThisFolder = @"L:\MSAcademic\downloads";
                SqlScriptFolder = @"\SqlScripts\";
            }

            // start off by connecting to existing SQL database and getting the name of the datalake folder that was used to create it (this is the date of last update)

            // Obtain AAD token
            var creds = new ClientCredential(applicationId, clientSecret);
            var clientCreds = ApplicationTokenProvider.LoginSilentAsync(tenantId, creds).GetAwaiter().GetResult();

            // Create ADLS client object
            AdlsClient client = AdlsClient.CreateClient(adlsAccountFQDN, clientCreds);

            try
            {
                DirectoryEntry latest = null;
                // Enumerate through directory on DataLake and find the most recent dataset
                foreach (DirectoryEntry entry in client.EnumerateDirectory("/graph"))
                {
                    if (latest == null)
                    {
                        latest = entry;
                    }
                    else
                    {
                        if (entry.LastModifiedTime > latest.LastModifiedTime)
                        {
                            latest = entry;
                        }
                    }
                    PrintDirectoryEntry(entry);
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
                DownloadThisFile(client, latest.FullName, "/Papers.txt", writeToThisFolder, limit);
                DownloadThisFile(client, latest.FullName, "/PaperAbstractsInvertedIndex.txt", writeToThisFolder, limit);
                DownloadThisFile(client, latest.FullName, "/PaperFieldsOfStudy.txt", writeToThisFolder, limit);
                DownloadThisFile(client, latest.FullName, "/PaperRecommendations.txt", writeToThisFolder, limit);
                DownloadThisFile(client, latest.FullName, "/PaperReferences.txt", writeToThisFolder, limit);
                DownloadThisFile(client, latest.FullName, "/PaperUrls.txt", writeToThisFolder, limit);
                DownloadThisFile(client, latest.FullName, "/FieldOfStudyChildren.txt", writeToThisFolder, limit);
                DownloadThisFile(client, latest.FullName, "/FieldOfStudyRelationship.txt", writeToThisFolder, limit);
                DownloadThisFile(client, latest.FullName, "/FieldsOfStudy.txt", writeToThisFolder, limit);
                DownloadThisFile(client, latest.FullName, "/Journals.txt", writeToThisFolder, limit);
                DownloadThisFile(client, latest.FullName, "/Authors.txt", writeToThisFolder, limit);
                DownloadThisFile(client, latest.FullName, "/Affiliations.txt", writeToThisFolder, limit);
                Console.WriteLine("");
                
                // once we've downloaded the files, put them into the SQL DB

                // Create all the tables
                Console.WriteLine("Creating tables...");
                SqlConnection conn = new SqlConnection(Program.SqlHelper.AcademicDB);
                conn.Open();
                SqlHelper.ExecuteNonQueryNonSP(conn, File.ReadAllText(AppContext.BaseDirectory + SqlScriptFolder + "CreateTables.sql"));
                SqlHelper.ExecuteNonQueryNonSP(conn, "DROP PROCEDURE IF EXISTS [BulkTextUpload]");
                SqlHelper.ExecuteNonQueryNonSP(conn, File.ReadAllText(AppContext.BaseDirectory + SqlScriptFolder + "BulkTextUpload.sql").Replace("writeToThisFolder", writeToThisFolder));
                Console.WriteLine("");

                // put the files into the DB
                UploadToDatabase(conn, "Papers");
                UploadToDatabase(conn, "PaperAbstractsInvertedIndex");
                UploadToDatabase(conn, "PaperFieldsOfStudy");
                UploadToDatabase(conn, "PaperRecommendations");
                UploadToDatabase(conn, "PaperReferences");
                UploadToDatabase(conn, "PaperUrls");
                UploadToDatabase(conn, "FieldOfStudyChildren");
                UploadToDatabase(conn, "FieldOfStudyRelationship");
                UploadToDatabase(conn, "FieldsOfStudy");
                UploadToDatabase(conn, "Journals");
                UploadToDatabase(conn, "Authors");
                UploadToDatabase(conn, "Affiliations");
                Console.WriteLine("");

                // CREATE INDEXES ON THE APPROPRIATE TABLES / FIELDS
                CreateIndexes(conn, SqlScriptFolder);

                conn.Close();

                // Once the file has been put into the DB, delete it from the local filesystem. e.g.
                /*
                if (File.Exists(@"E:\MSAcademic\Affiliations.txt"))
                {
                    File.Delete(@"E:\MSAcademic\Affiliations.txt");
                }
                */

                // once we've finished downloading all the files, delete all the older databases on DataLake, so we only have the most recent one being stored
                // I think keep the most recent one, in case we need to rebuild the DB for any reason??
                foreach (DirectoryEntry entry in client.EnumerateDirectory("/graph"))
                {
                    // store in a string variable the name of the most recent directory
                    // if each directory is not the most current, then delete it using this command
                    // client.DeleteRecursive("/graph/directoryname");
                    // when this service is up and running, there will only ever be one directory deleted
                    // hopefully this doesn't mess up the Enumerable, but if it does, we'll need to enumerate and store a list of folders to delete
                }

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

        private static bool DownloadThisFile(AdlsClient client, string graphPath, string fileName, string folder, int limit)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            Int64 count = 0;
            Int64 tempLimit = limit == 0 ? 5000000000 : limit; // i.e. if limit == 0 we download everything. less for testing
            Console.WriteLine("Reading this file now: " + fileName);
            using (var readStream = new StreamReader(client.GetReadStream(graphPath + fileName)))
            {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(folder + fileName))
                {
                    string line;
                    while ((line = readStream.ReadLine()) != null && count < tempLimit)
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

        private static void PrintDirectoryEntry(DirectoryEntry entry)
        {
            Console.WriteLine($"Name: {entry.Name}");
            Console.WriteLine($"FullName: {entry.FullName}");
            Console.WriteLine($"Length: {entry.Length}");
            Console.WriteLine($"Type: {entry.Type}");
            Console.WriteLine($"User: {entry.User}");
            Console.WriteLine($"Group: {entry.Group}");
            Console.WriteLine($"Permission: {entry.Permission}");
            Console.WriteLine($"Modified Time: {entry.LastModifiedTime}");
            Console.WriteLine($"Last Accessed Time: {entry.LastAccessTime}");
            Console.WriteLine();
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