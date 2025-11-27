using BusinessLibrary.Data;
using BusinessLibrary.Security;
using Microsoft.Azure.DataLake.Store;
using Microsoft.Azure.Management.DataLake.Analytics;
using Microsoft.Azure.Management.DataLake.Analytics.Models;
using Microsoft.Azure.Management.DataLake.Store;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Rest;
using Microsoft.Rest.Azure.Authentication;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
#if (CSLA_NETCORE)
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
#else
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage;
#endif


namespace BusinessLibrary.BusinessClasses
{
    class MagDataLakeHelpers
    {
        public static bool ExecProc(string scriptTxt, bool doLogging, string jobType, int ContactId, int parallelism,
            CancellationToken cancellationToken)
        {
            var adlCreds = GetCreds_SPI_SecretKey();
            var adlaJobClient = new DataLakeAnalyticsJobManagementClient(adlCreds);
            var adlsFileSystemClient = new DataLakeStoreFileSystemManagementClient(adlCreds);


            string dataLakeAccount = AzureSettings.MagDataLakeDataLakeAccount;
            string dataLakestorageAccount = AzureSettings.MagDataLakeStorageAccount;

            try
            {
                var returnJobId = SubmitJObToADA(adlsFileSystemClient,
                dataLakestorageAccount,
                adlaJobClient, dataLakeAccount, scriptTxt, parallelism, jobType);
                if (!WaitForJob(adlaJobClient, dataLakeAccount, returnJobId, doLogging, jobType, ContactId, cancellationToken))
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                MagLog.SaveLogEntry("DataLake: " + jobType, "Error", e.Message, ContactId);
                return false;
            }
            return true;
        }

        public static bool WaitForJob(DataLakeAnalyticsJobManagementClient adlaJobClient,
            string adlaAccountName, Guid jobId, bool doLogging, string jobType,
            int ContactId, CancellationToken cancellationToken)
        {
            var jobInfo = adlaJobClient.Job.Get(adlaAccountName, jobId);
            int MagLogId = MagLog.SaveLogEntry("DataLake: " + jobType, jobInfo.State.ToString(), "", ContactId);
            try
            {
                
                while (jobInfo.State != JobState.Ended && !cancellationToken.IsCancellationRequested)
                {
                    Thread.Sleep(15000);
                    jobInfo = adlaJobClient.Job.Get(adlaAccountName, jobId);
                    MagLog.UpdateLogEntry(jobInfo.State.ToString(), "", MagLogId);
                }
                if (cancellationToken.IsCancellationRequested)
                {
                    MagLog.UpdateLogEntry("Failed", "Thread cancelled", MagLogId);
                    return false;
                }
                if (jobInfo.ErrorMessage != null && jobInfo.ErrorMessage.Count > 0)
                {
                    MagLog.UpdateLogEntry(jobInfo.ErrorMessage[0].Description, jobInfo.ErrorMessage[0].Message, MagLogId);
                }
                else
                {
                    MagLog.UpdateLogEntry(jobInfo.State.ToString(), "", MagLogId);
                }

                return true;
            }
            catch
            {
                MagLog.UpdateLogEntry("Failed", "Error", MagLogId);
                return false;
            }
        }

        public static void CreateAFileOnDataLakeStorageAccount(DataLakeStoreFileSystemManagementClient adlsFileSystemClient,
            string filename, string StorageAccountName)
        {

            using (var memstream = new MemoryStream())
            {
                using (var sw = new StreamWriter(memstream, Encoding.UTF8))
                {
                    //sw.WriteLine("Hello World");
                    sw.Flush();
                    memstream.Position = 0;
                    adlsFileSystemClient.FileSystem.Create(StorageAccountName, filename, memstream);
                }
            }
        }

        public static ServiceClientCredentials GetCreds_SPI_SecretKey(
               //string tenant,
               //Uri tokenAudience,
               //string clientId,
               //string secretKey
               )
        {
            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());

            var serviceSettings = ActiveDirectoryServiceSettings.Azure;
            serviceSettings.TokenAudience = new Uri(@"https://datalake.azure.net/");



            string magDataLakeTenantId = AzureSettings.MagDataLakeTenantId;
            string magDataLakeClientId = AzureSettings.MagDataLakeClientId;
            string magDataLakeSecretKey = AzureSettings.MagDataLakeSecretKey;

            var creds = ApplicationTokenProvider.LoginSilentAsync(
                magDataLakeTenantId,
                magDataLakeClientId,
                magDataLakeSecretKey,
             serviceSettings).GetAwaiter().GetResult();
            return creds;
        }

        public static TokenCache GetTokenCache(string path)
        {
            var tokenCache = new TokenCache();

            tokenCache.BeforeAccess += notificationArgs =>
            {
                if (File.Exists(path))
                {
                    var bytes = File.ReadAllBytes(path);
                    notificationArgs.TokenCache.Deserialize(bytes);
                }
            };

            tokenCache.AfterAccess += notificationArgs =>
            {
                var bytes = notificationArgs.TokenCache.Serialize();
                File.WriteAllBytes(path, bytes);
            };
            return tokenCache;
        }

        //public static AdlsClient ConnectToClient(ServiceClientCredentials adlCreds)
        //{


        //    string dataLakeAccount = AzureSettings.MagDataLakeDataLakeAccount;

        //    string _adlsg1AccountName = dataLakeAccount + ".azuredatalakestore.net";

        //    AdlsClient client = AdlsClient.CreateClient(_adlsg1AccountName, adlCreds);

        //    return client;
        //}

        public static void CreateAFile(AdlsClient client, string fileName)
        {

            fileName = "/eppimag/test3.txt";
            using (var stream = client.CreateFile(fileName, IfExists.Overwrite))
            {
                byte[] textByteArray = Encoding.UTF8.GetBytes("This is test data to write.\r\n");
                stream.Write(textByteArray, 0, textByteArray.Length);

                textByteArray = Encoding.UTF8.GetBytes("This is the second line.\r\n");
                stream.Write(textByteArray, 0, textByteArray.Length);
            }

        }

        public static void ReadFileFromADL(AdlsClient client, string fileName, ServiceClientCredentials adlCreds)
        {

            // Connect
            using (var readStream = new StreamReader(client.GetReadStream(fileName)))
            {
                string line;
                while ((line = readStream.ReadLine()) != null)
                {
                    Console.WriteLine(line);
                }
            }

        }

        /*

        public static (ResourceManagementClient resourceManagementClient,
            DataLakeAnalyticsAccountManagementClient adlaAccountClient,
            DataLakeStoreAccountManagementClient adlsAccountClient,
            DataLakeAnalyticsCatalogManagementClient adlaCatalogClient,
            DataLakeAnalyticsJobManagementClient adlaJobClient,
            DataLakeStoreFileSystemManagementClient adlsFileSystemClient,
            GraphRbacManagementClient graphClient
            )
            CreateClientManagmentObjects(ServiceClientCredentials armCreds,
            ServiceClientCredentials adlCreds, ServiceClientCredentials graphCreds, string subid, string domain)
        {
            var resourceManagementClient = new ResourceManagementClient(armCreds) { SubscriptionId = subid };
            var adlaAccountClient = new DataLakeAnalyticsAccountManagementClient(armCreds);
            adlaAccountClient.SubscriptionId = subid;
            var adlsAccountClient = new DataLakeStoreAccountManagementClient(armCreds);
            adlsAccountClient.SubscriptionId = subid;
            var adlaCatalogClient = new DataLakeAnalyticsCatalogManagementClient(adlCreds);
            var adlaJobClient = new DataLakeAnalyticsJobManagementClient(adlCreds);
            var adlsFileSystemClient = new DataLakeStoreFileSystemManagementClient(adlCreds);
            var graphClient = new GraphRbacManagementClient(graphCreds);
            graphClient.TenantID = domain;

            return (resourceManagementClient, adlaAccountClient, adlsAccountClient, adlaCatalogClient, adlaJobClient, adlsFileSystemClient, graphClient);
        }
        */

        public static bool CheckStorageAccountExists(DataLakeAnalyticsAccountManagementClient adlaAccountClient,
            string rg, string accountName, string storage_account, string storage_container)
        {

            bool accountExists = adlaAccountClient.Account.StorageAccountExists(rg, accountName, storage_account);
            if (accountExists)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        public static void ListDatabasesOnCatalogClient(DataLakeAnalyticsCatalogManagementClient adlaCatalogClient,
            string adla)
        {
            var databases = adlaCatalogClient.Catalog.ListDatabases(adla, null, null, null);
            foreach (var db in databases)
            {
                Console.WriteLine($"Database: {db.Name}");
                Console.WriteLine(" - Schemas:");
                var schemas = adlaCatalogClient.Catalog.ListSchemas(adla, db.Name);
                foreach (var schm in schemas)
                {
                    Console.WriteLine($"\t{schm.Name}");
                }
            }
        }


        public static Guid SubmitJObToADA(DataLakeStoreFileSystemManagementClient adlsFileSystemClient,
            string _adlsAccountName, DataLakeAnalyticsJobManagementClient adlaJobClient, string adla,
            string scriptTxt, int parallelism, string jobName)
        {
            // UNCOMMENT BELOW WHEN THE SCRIPT IS HELD IN BLOB STORAGE AND NOT ON LOCAL MACHINE
            // MINOR CHANGES REQUIRED TO OPEN BLOB SCRIPT
            //string scriptPath = @"C:\Users\Patrick\Source\Repos\USQLApplication1\USQLApplication1\Script.usql";
            //Stream scriptStrm = adlsFileSystemClient.FileSystem.Open(_adlsAccountName, scriptPath);
            //string scriptTxt = string.Empty;
            //using (StreamReader sr = new StreamReader(scriptStrm))
            //{
            //    scriptTxt = sr.ReadToEnd();
            //}
            //string scriptTxt = File.ReadAllText(@"c:/Temp/RemoteTest.usql");

            //scriptTxt = @"[master].[dbo].[GetPaperIdChanges](""doesnt matter either"");";

            var jobId = Guid.NewGuid();
            var properties = new USqlJobProperties(scriptTxt);
            var parameters = new JobInformation(jobName, JobType.USql, properties, priority: 1,
                degreeOfParallelism: parallelism, jobId: jobId);

            var jobInfo = adlaJobClient.Job.Create(adla, jobId, parameters);
            return jobId;
        }
    }
}
