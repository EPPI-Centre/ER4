using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Specialized;
using System.IO;
using System.Configuration;
using Azure.Storage;
using Azure.Core;
#if (CSLA_NETCORE)
using Microsoft.Extensions.Configuration;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
#else
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage;
#endif

namespace BusinessLibrary.BusinessClasses
{
    public class AzureSettings
    {
#if (CSLA_NETCORE)

        public static void SetValues(IConfiguration config)
        {
            _AzureContReviewSettings = config.GetSection("AzureContReviewSettings");
            _AzureMagSettings = config.GetSection("AzureMagSettings");
            _ClassifierSettings = config.GetSection("ClassifierSettings");
            _AppSettings = config.GetSection("AppSettings");
        }

        private static Microsoft.Extensions.Configuration.IConfigurationSection _AppSettings;
        private static Microsoft.Extensions.Configuration.IConfigurationSection AppSettings
        {
            get
            {
#if WEBDB
                if (_AppSettings == null) _AppSettings = WebDatabasesMVC.Startup.Configuration.GetSection("AppSettings");
#endif
                return _AppSettings;
            }
        }

        private static Microsoft.Extensions.Configuration.IConfigurationSection _AzureContReviewSettings;
        private static Microsoft.Extensions.Configuration.IConfigurationSection AzureContReviewSettings
        {
            get
            {
#if WEBDB
                if (_AzureContReviewSettings == null) _AzureContReviewSettings = WebDatabasesMVC.Startup.Configuration.GetSection("AzureContReviewSettings");
//#else
//                if (_AzureContReviewSettings == null) _AzureContReviewSettings = ERxWebClient2.Startup.Configuration.GetSection("AzureContReviewSettings");
#endif
                return _AzureContReviewSettings;
            }
        }
        private static Microsoft.Extensions.Configuration.IConfigurationSection _AzureMagSettings;
        private static Microsoft.Extensions.Configuration.IConfigurationSection AzureMagSettings
        {
            get
            {
#if WEBDB
                if (_AzureMagSettings == null) _AzureMagSettings = WebDatabasesMVC.Startup.Configuration.GetSection("AzureMagSettings");
//#else
//                if (_AzureMagSettings == null) _AzureMagSettings = ERxWebClient2.Startup.Configuration.GetSection("AzureMagSettings");
#endif
                return _AzureMagSettings;
            }
        }
        private static Microsoft.Extensions.Configuration.IConfigurationSection _ClassifierSettings;
        private static Microsoft.Extensions.Configuration.IConfigurationSection ClassifierSettings
        {
            get
            {
#if WEBDB
                if (_AzureContReviewSettings == null) _ClassifierSettings = WebDatabasesMVC.Startup.Configuration.GetSection("ClassifierSettings");
//#else
//                if (_AzureContReviewSettings == null) _ClassifierSettings = ERxWebClient2.Startup.Configuration.GetSection("ClassifierSettings");
#endif
                return _ClassifierSettings;
            }
        }
#else
        private static NameValueCollection _AllAppSettings;
        private static NameValueCollection AppSettings
        {
            get
            {
                if (_AllAppSettings == null) _AllAppSettings = ConfigurationManager.AppSettings;
                return _AllAppSettings;
            }
        }
        private static NameValueCollection AzureContReviewSettings
        {
            get
            {
                if (_AllAppSettings == null) _AllAppSettings = ConfigurationManager.AppSettings;
                return _AllAppSettings;
            }
        }
        private static NameValueCollection AzureMagSettings
        {
            get
            {
                if (_AllAppSettings == null) _AllAppSettings = ConfigurationManager.AppSettings;
                return _AllAppSettings;
            }
        }
        private static NameValueCollection ClassifierSettings
        {
            get
            {
                if (_AllAppSettings == null) _AllAppSettings = ConfigurationManager.AppSettings;
                return _AllAppSettings;
            }
        }
#endif



        public static string MagMatchItemsMaxThreadCount { get { return AppSettings["MagMatchItemsMaxThreadCount"]; } }

        public static string tenantID { get { return AzureContReviewSettings["tenantID"]; } }
        public static string appClientId { get { return AzureContReviewSettings["appClientId"]; } }
        public static string appClientSecret { get { return AzureContReviewSettings["appClientSecret"]; } }
        public static string subscriptionId { get { return AzureContReviewSettings["subscriptionId"]; } }
        public static string resourceGroup { get { return AzureContReviewSettings["resourceGroup"]; } }
        public static string dataFactoryName { get { return AzureContReviewSettings["dataFactoryName"]; } }
        public static string covidClassifierPipelineName { get { return AzureContReviewSettings["covidClassifierPipelineName"]; } }
        public static string covidLongCovidPipelineName { get { return AzureContReviewSettings["covidLongCovidPipelineName"]; } }
        public static string progressPlusPipelineName { get { return AzureContReviewSettings["progressPlusPipelineName"]; } }
        public static string pubMedStudyTypesPipelineName { get { return AzureContReviewSettings["pubMedStudyTypesPipelineName"]; } }
        public static string pipelineName { get { return AzureContReviewSettings["pipelineName"]; } }

        

        public static string MagDataLakeDataLakeAccount { get { return AzureMagSettings["MagDataLakeDataLakeAccount"]; } }
        public static string MagDataLakeStorageAccount { get { return AzureMagSettings["MagDataLakeStorageAccount"]; } }
        public static string MagDataLakeTenantId { get { return AzureMagSettings["MagDataLakeTenantId"]; } }
        public static string MagDataLakeClientId { get { return AzureMagSettings["MagDataLakeClientId"]; } }
        public static string MagDataLakeSecretKey { get { return AzureMagSettings["MagDataLakeSecretKey"]; } }
        public static string MAGStorageAccount { get { return AzureMagSettings["MAGStorageAccount"]; } }
        public static string MAGStorageAccountKey { get { return AzureMagSettings["MAGStorageAccountKey"]; } }
        public static string databricks_cluster_id { get { return AzureMagSettings["databricks_cluster_id"]; } }
        public static string DynamicAnnotation { get { return AzureMagSettings["DynamicAnnotation"]; } }
        public static string ml_pipeline_id { get { return AzureMagSettings["ml_pipeline_id"]; } }
        public static string sa_account_name { get { return AzureMagSettings["sa_account_name"]; } }
        public static string sa_secret_scope_name { get { return AzureMagSettings["sa_secret_scope_name"]; } }
        public static string sa_secret_scope_key { get { return AzureMagSettings["sa_secret_scope_key"]; } }
        public static string db_database_schema_version { get { return AzureMagSettings["db_database_schema_version"]; } }
        public static string exp_experiments_container { get { return AzureMagSettings["exp_experiments_container"]; } }
        public static string exp_run_suffix { get { return AzureMagSettings["exp_run_suffix"]; } }
        public static string AzureSearchMAGApiKey { get { return AzureMagSettings["AzureSearchMAGApi-key"]; } }
        public static string OpenAlexEndpoint { get { return AzureMagSettings["OpenAlexEndpoint"]; } }
        public static string OpenAlexEmailHeader { get { return AzureMagSettings["OpenAlexEmailHeader"]; } }
        public static string OpenAlexApiKey { get { return AzureMagSettings["OpenAlexApiKey"]; } }


        public static string blobConnection { get { return ClassifierSettings["blobConnection"]; } }
        public static string BaseUrlScoreModel { get { return ClassifierSettings["BaseUrlScoreModel"]; } }
        public static string apiKeyScoreModel { get { return ClassifierSettings["apiKeyScoreModel"]; } }
        public static string BaseUrlBuildModel { get { return ClassifierSettings["BaseUrlBuildModel"]; } }
        public static string apiKeyBuildModel { get { return ClassifierSettings["apiKeyBuildModel"]; } }
        public static string BaseUrlScoreNewRCTModel { get { return ClassifierSettings["BaseUrlScoreNewRCTModel"]; } }
        public static string apiKeyScoreNewRCTModel { get { return ClassifierSettings["apiKeyScoreNewRCTModel"]; } }

        public static string BaseUrlBuildAndScore { get { return ClassifierSettings["BaseUrlBuildAndScore"]; } }
        public static string apiKeyBuildAndScore { get { return ClassifierSettings["apiKeyBuildAndScore"]; } }
        public static string BaseUrlVectorise { get { return ClassifierSettings["BaseUrlVectorise"]; } }
        public static string apiKeyVectorise { get { return ClassifierSettings["apiKeyVectorise"]; } }
        public static string BaseUrlSimulation5 { get { return ClassifierSettings["BaseUrlSimulation5"]; } }
        public static string apiKeySimulation5 { get { return ClassifierSettings["apiKeySimulation5"]; } }
    }
    public class BlobOperations
    {
#if (!CSLA_NETCORE)
        public static bool ThisBlobExist(string blobConnectionStr, string containerName, string blobName)
        {
            return GetBlockBlobReference(blobConnectionStr, containerName, blobName).Exists();
        }
        public static Dictionary<string, bool> TheseBlobsExist(string blobConnectionStr, string containerName, List<string> blobNames)
        {
            Dictionary<string, bool> res = new Dictionary<string, bool>();
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(blobConnectionStr);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            foreach (string bName in blobNames)
            {
                CloudBlobContainer container = blobClient.GetContainerReference(containerName);
                CloudBlockBlob cbl = container.GetBlockBlobReference(bName);
                res.Add(bName, cbl.Exists());
            }
            return res;
        }
        public static MemoryStream DownloadBlobAsMemoryStream(string blobConnectionStr, string containerName, string blobName)
        {
            MemoryStream res = new MemoryStream();
            CloudBlockBlob blockBlob = GetBlockBlobReference(blobConnectionStr, containerName, blobName);
            blockBlob.DownloadToStream(res);
            res.Position = 0;
            return res;
        }
        public static void UploadStream(string blobConnectionStr, string containerName, string blobName, Stream Data)
        {
            CloudBlockBlob blob = GetBlockBlobReference(blobConnectionStr, containerName, blobName);
            blob.UploadFromStream(Data);
        }
        public static void UploadString(string blobConnectionStr, string containerName, string blobName, string Data)
        {
            CloudBlockBlob blob = GetBlockBlobReference(blobConnectionStr, containerName, blobName);
            blob.UploadText(Data);
        }
        public static void DeleteIfExists(string blobConnectionStr, string containerName, string blobName)
        {
            CloudBlockBlob blob = GetBlockBlobReference(blobConnectionStr, containerName, blobName);
            blob.DeleteIfExists();
        }
        public static bool ThisBlobHasData(string blobConnectionStr, string containerName, string blobName)
        {
            CloudBlockBlob blob = GetBlockBlobReference(blobConnectionStr, containerName, blobName);
            blob.FetchAttributes();
            return blob.Properties.Length > 0;
        }
        public static List<BlobInHierarchy> Blobfilenames(string blobConnectionStr, string containerName, string prefix)
        {
            List<BlobInHierarchy> result = new List<BlobInHierarchy>();
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(blobConnectionStr);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(containerName);
            BlobContinuationToken continuationToken = null;
            BlobResultSegment resultSegment = container.ListBlobsSegmented(prefix,
            true, BlobListingDetails.Metadata, 100, continuationToken, null, null);
            foreach (var blobItem in resultSegment.Results)
            {
                if (blobItem is CloudBlobDirectory)
                {
                    CloudBlobDirectory dir = (CloudBlobDirectory)blobItem;
                    result.Add(new BlobInHierarchy(dir.Uri.ToString(), true));//not sure Uri is the right thing, but we don't use this data, so I'll go with it
                }
                else if (blobItem is CloudBlob)
                {
                    CloudBlob cb = (CloudBlob)blobItem;
                    result.Add(new BlobInHierarchy(cb.Name, false));
                }
            }
            return result;
        }
        

        private static CloudBlockBlob GetBlockBlobReference(string blobConnectionStr, string containerName, string blobName)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(blobConnectionStr);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(containerName);
            return container.GetBlockBlobReference(blobName);
        }
#else
        public static bool ThisBlobExist(string blobConnectionStr, string containerName, string blobName)
        {
            Task<bool> task = ThisBlobExistTask(blobConnectionStr, containerName, blobName);
            //task.RunSynchronously();
            return task.Result;
        }

        public static Dictionary<string, bool> TheseBlobsExist(string blobConnectionStr, string containerName, List<string> blobNames)
        {
            Task<Dictionary<string, bool>> task = TheseBlobsExistTask(blobConnectionStr, containerName, blobNames);
            return task.Result;
        }
        public static async Task<bool> ThisBlobExistTask(string blobConnectionStr, string containerName, string blobName)
        {
            BlobClient blobC = GetBlob(blobConnectionStr, containerName, blobName);
            return await blobC.ExistsAsync();
        }
        public static MemoryStream DownloadBlobAsMemoryStream(string blobConnectionStr, string containerName, string blobName)
        {
            MemoryStream res = new MemoryStream();
            BlobClient blobC = GetBlob(blobConnectionStr, containerName, blobName);
            //task.RunSynchronously();
            blobC.DownloadTo(res);
            res.Position = 0;
            return res;
        }

        public static void UploadStream(string blobConnectionStr, string containerName, string blobName, Stream Data)
        {
            BlobClient blob = GetBlob(blobConnectionStr, containerName, blobName);
            blob.DeleteIfExists();
            //task.RunSynchronously();
            //BlobClient blob = task.Result;

            blob.Upload(Data);
        }
        public static void UploadString(string blobConnectionStr, string containerName, string blobName, string Data)
        {
            using (var stream = GenerateStreamFromString(Data))
            {
                UploadStream(blobConnectionStr, containerName, blobName, stream);
            }
        }
        private static Stream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
        public static void DeleteIfExists(string blobConnectionStr, string containerName, string blobName)
        {
            BlobClient blob = GetBlob(blobConnectionStr, containerName, blobName);
            //task.RunSynchronously();
            blob.DeleteIfExists();
        }
        
        public static bool ThisBlobHasData(string blobConnectionStr, string containerName, string blobName)
        {
            BlobClient blob = GetBlob(blobConnectionStr, containerName, blobName);
            BlobProperties props = blob.GetProperties().Value;
            return props.ContentLength > 0;
        }
        public static List<BlobInHierarchy> Blobfilenames(string blobConnectionStr, string containerName, string prefix)
        {
            Task<List<BlobInHierarchy>> task = BlobfilenamesTask(blobConnectionStr, containerName, prefix);
            //task.RunSynchronously();
            List<BlobInHierarchy> res = task.Result;
            return res;
        }


        private static BlobClient GetBlob(string blobConnectionStr, string containerName, string blobName)
        {
            BlobClientOptions opts = new BlobClientOptions()
            {
                Retry = {
                    Delay = TimeSpan.FromSeconds(2),
                    MaxRetries = 2,
                    Mode = RetryMode.Exponential,
                    MaxDelay = TimeSpan.FromSeconds(10),
                    NetworkTimeout = TimeSpan.FromMinutes(5)
                },
            };
            BlobServiceClient blobServiceClient = new BlobServiceClient(blobConnectionStr, opts);
            BlobContainerClient container = blobServiceClient.GetBlobContainerClient(containerName);
            //await container.CreateIfNotExistsAsync();
            BlobClient blob = container.GetBlobClient(blobName);
            return blob;
        }
        private static async Task<Dictionary<string, bool>> TheseBlobsExistTask(string blobConnectionStr, string containerName, List<string> blobNames)
        {
            Dictionary<string, bool> res = new Dictionary<string, bool>();
            BlobServiceClient blobServiceClient = new BlobServiceClient(blobConnectionStr);
            BlobContainerClient container = blobServiceClient.GetBlobContainerClient(containerName);
            await container.CreateIfNotExistsAsync();
            foreach (string bName in blobNames)
            {
                BlobClient blob = container.GetBlobClient(bName);
                bool exists = await blob.ExistsAsync();
                res.Add(bName, exists);
            }
            return res;
        }
        private static async Task<List<BlobInHierarchy>> BlobfilenamesTask(string blobConnectionStr, string containerName, string prefix)
        {
            List<BlobInHierarchy> result = new List<BlobInHierarchy>();
            BlobServiceClient blobServiceClient = new BlobServiceClient(blobConnectionStr);
            BlobContainerClient container = blobServiceClient.GetBlobContainerClient(containerName);
            await container.CreateIfNotExistsAsync();
            await ListBlobsHierarchicalListingTask(container, prefix, result);
            return result;
        }

        private static async Task<List<BlobInHierarchy>> ListBlobsHierarchicalListingTask(BlobContainerClient container, string prefix, List<BlobInHierarchy> result)
        {
            //see https://docs.microsoft.com/en-us/azure/storage/blobs/storage-blobs-list
            // Call the listing operation and return pages of the specified size.
            var resultSegment = container.GetBlobsByHierarchyAsync(prefix: prefix, delimiter: "/")
                    .AsPages();

            // Enumerate the blobs returned for each page.
            await foreach (Azure.Page<BlobHierarchyItem> blobPage in resultSegment)
            {
                // A hierarchical listing may return both virtual directories and blobs.
                foreach (BlobHierarchyItem blobhierarchyItem in blobPage.Values)
                {
                    if (blobhierarchyItem.IsPrefix)
                    {
                        result.Add(new BlobInHierarchy(blobhierarchyItem.Blob.Name, true));
                        await ListBlobsHierarchicalListingTask(container, blobhierarchyItem.Prefix, result);
                    }
                    else
                    {
                        result.Add(new BlobInHierarchy(blobhierarchyItem.Blob.Name, false));
                    }
                }
            }
            return result;
        }
#endif
    }
    public class BlobInHierarchy
    {
        public BlobInHierarchy(string blobName, bool isVirtualFolder = false)
        {
            _BlobName = blobName;
            _IsVirtualFolder = isVirtualFolder;
        }
        private bool _IsVirtualFolder = false;
        public bool IsVirtualFolder { get { return _IsVirtualFolder; } }
        public bool IsFile { get { return !_IsVirtualFolder; } }

        private string _BlobName = "";
        public string BlobName { get { return _BlobName; } }

    }
}
