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
            string applicationId = configuration["AppSettings:applicationId"];     // Also called client id
            string clientSecret = configuration["AppSettings:clientSecret"];
            string tenantId = configuration["AppSettings:tenantId"];
            string adlsAccountFQDN = configuration["AppSettings:adlsAccountFQDN"];   // full account FQDN, not just the account name like example.azure.datalakestore.net

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
                // Now create all the tables - see 'CreateTables.sql' for current shape of data



                // 2. Go through each file that we want to download. As there are a number of files for which we'll follow the same
                // procedure, we should probably put this in a separate method.
                string fileName = "/graph/2018-07-19/Affiliations.txt";
                int count = 0;
                int tempLimit = 10000; // ********** use this for testing so that there's no need to download the whole database!
                Console.WriteLine("Reading file next");
                using (var readStream = new StreamReader(client.GetReadStream(fileName)))
                {
                    using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"E:\MSAcademic\Affiliations.txt"))
                    {
                        string line;
                        //while ((line = readStream.ReadLine()) != null) // for downloading the whole database
                        while ((line = readStream.ReadLine()) != null || count == tempLimit)
                        {
                            file.WriteLine(line);
                            Console.WriteLine(count.ToString());
                            count++;
                        }
                    }
                }

                // once we've downloaded the file, put it into the SQL DB
                // See the BulkTextUpload.sql file for an example of this. (We should probably put these into stored procedures)

                // Once the file has been put into the DB, delete it from the local filesystem
                if (File.Exists(@"E:\MSAcademic\Affiliations.txt"))
                {
                    File.Delete(@"E:\MSAcademic\Affiliations.txt");
                }

                // ... get next file

                // once we've finished downloading all the files, delete all the older databases on DataLake, so we only have the most recent one being stored
                // I think keep the most recent one, in case we need to rebuild the DB for any reason??
                foreach (DirectoryEntry entry in client.EnumerateDirectory("/graph"))
                {
                    // store in a string variable the name of the most recent directory
                    // if each directory is not the most current, then delete it using this command
                    client.DeleteRecursive("/graph/directoryname");
                    // when this service is up and running, there will only ever be one directory deleted
                    // hopefully this doesn't mess up the Enumerable, but if it does, we'll need to enumerate and store a list of folders to delete
                }

                // We've now got a new clean database. We need to:
                // 1. Record in the Reivewer DB (or somewhere??) the fact that it should now use the new academic SQL DB and not the old onw
                // 2. Delete the old SQL DB


                // We're done :)


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

            }
            catch (AdlsException e)
            {
                PrintAdlsException(e);
            }

            Console.WriteLine("Done. Press ENTER to continue ...");
            Console.ReadLine();
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

