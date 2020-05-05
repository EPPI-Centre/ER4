using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Csla;

using Csla.Serialization;
using Csla.Silverlight;
using System.ComponentModel;
using Csla.DataPortalClient;
using System.Threading;
using System.Configuration;

#if !SILVERLIGHT
using BusinessLibrary.Data;
using BusinessLibrary.Security;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class MagBlobDataCommand : CommandBase<MagBlobDataCommand>
    {

#if SILVERLIGHT
    public MagBlobDataCommand(){}
#else
        public MagBlobDataCommand() { }
#endif

        private string _ReleaseNotes;
        private string _LatestMagSasUri;
        private string _LatestMAGName;
        [Newtonsoft.Json.JsonProperty]
        public string ReleaseNotes
        {
            get
            {
                return _ReleaseNotes;
            }
        }

        public string LatestMagSasUri
        {
            get
            {
                return _LatestMagSasUri;
            }
        }
        public string LatestMAGName
        {
            get
            {
                return _LatestMAGName;
            }
        }

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_ReleaseNotes", _ReleaseNotes);
            info.AddValue("_LatestMagSasUri", _LatestMagSasUri);
            info.AddValue("_LatestMAGName", _LatestMAGName);
        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            _ReleaseNotes = info.GetValue<string>("_ReleaseNotes");
            _LatestMagSasUri = info.GetValue<string>("_LatestMagSasUri");
            _LatestMAGName = info.GetValue<string>("_LatestMAGName");
        }


#if !SILVERLIGHT

        protected override void DataPortal_Execute()
        {
            var task = ListMagDbsAndMeta();
            task.Wait();
        }

        private static CloudBlob CurrentReleaseNotes { get; set; }
        private static CloudBlobContainer containerLatest { get; set; }
        

        private async Task ListMagDbsAndMeta()
        {
            string storageAccountName = ConfigurationManager.AppSettings["MAGStorageAccount"];
            string storageAccountKey = ConfigurationManager.AppSettings["MAGStorageAccountKey"];

            string storageConnectionString =
                "DefaultEndpointsProtocol=https;AccountName=" + storageAccountName + ";AccountKey=";
            storageConnectionString += storageAccountKey;

            // Check whether the connection string can be parsed.
            CloudStorageAccount storageAccount;
            if (CloudStorageAccount.TryParse(storageConnectionString, out storageAccount))
            {
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

                List<string> ListContainers = await ListContainersWithPrefixAsync(blobClient, "mag-");

                DateTime LatestDateMAG = new DateTime(2017, 1, 1);
                foreach (var item in ListContainers)
                {
                    CloudBlobContainer container = blobClient.GetContainerReference(item);

                    var length = container.Name.Length;
                    int ind = container.Name.IndexOf("-", 0);
                    string dateStr = container.Name.Substring(ind + 1, length - ind - 1);
                    DateTime date;
                    if (DateTime.TryParse(dateStr, out date)) // might be some other folder than MAG
                    {
                        if (date > LatestDateMAG)
                        {
                            LatestDateMAG = date;
                            _LatestMAGName = item;
                        }
                    }
                }
                containerLatest = blobClient.GetContainerReference(_LatestMAGName);

                //Console.WriteLine("The relevant SAS key is: ");
                _LatestMagSasUri = GetContainerSasUri(containerLatest);

                var rootDirectory = containerLatest.GetDirectoryReference("");

                BlobContinuationToken token = new BlobContinuationToken();
                var results = await rootDirectory.ListBlobsSegmentedAsync(token);

                await ListBlobsFlatListingAsync(containerLatest, null);

                if (CurrentReleaseNotes != null)
                {
                    CloudBlockBlob blockBlob = CurrentReleaseNotes as CloudBlockBlob;
                    _ReleaseNotes = await blockBlob.DownloadTextAsync();
                }

            }
        }

        private async Task<List<string>> ListContainersWithPrefixAsync(CloudBlobClient blobClient, string prefix)
        {
            List<string> ListContainers = new List<string>();
            //Console.WriteLine("List all containers beginning with prefix {0}, plus container metadata:", prefix);

            BlobContinuationToken continuationToken = null;
            ContainerResultSegment resultSegment = null;

            try
            {
                do
                {
                    // List containers beginning with the specified prefix, returning segments of 5 results each. 
                    // Note that passing in null for the maxResults parameter returns the maximum number of results (up to 5000).
                    // Requesting the container's metadata as part of the listing operation populates the metadata, 
                    // so it's not necessary to call FetchAttributes() to read the metadata.
                    resultSegment = await blobClient.ListContainersSegmentedAsync(
                        prefix, ContainerListingDetails.Metadata, 5, continuationToken, null, null);

                    // Enumerate the containers returned.
                    foreach (var container in resultSegment.Results)
                    {
                        //Console.WriteLine("\tContainer:" + container.Name);
                        ListContainers.Add(container.Name);
                        // Write the container's metadata keys and values.
                        foreach (var metadataItem in container.Metadata)
                        {
                            //Console.WriteLine("\t\tMetadata key: " + metadataItem.Key);
                            //Console.WriteLine("\t\tMetadata value: " + metadataItem.Value);
                        }
                    }

                    // Get the continuation token.
                    continuationToken = resultSegment.ContinuationToken;

                } while (continuationToken != null);

                //Console.WriteLine();
                return ListContainers;
            }
            catch (StorageException e)
            {
                Console.WriteLine(e.Message);
                Console.ReadLine();
                throw;
            }
        }

        private string GetContainerSasUri(CloudBlobContainer container, string storedPolicyName = null)
        {
            string sasContainerToken;

            // If no stored policy is specified, create a new access policy and define its constraints.
            if (storedPolicyName == null)
            {
                // Note that the SharedAccessBlobPolicy class is used both to define the parameters of an ad-hoc SAS, and 
                // to construct a shared access policy that is saved to the container's shared access policies. 
                SharedAccessBlobPolicy adHocPolicy = new SharedAccessBlobPolicy()
                {
                    // When the start time for the SAS is omitted, the start time is assumed to be the time when the storage service receives the request. 
                    // Omitting the start time for a SAS that is effective immediately helps to avoid clock skew.
                    SharedAccessExpiryTime = DateTime.UtcNow.AddHours(24),
                    Permissions = SharedAccessBlobPermissions.Read | SharedAccessBlobPermissions.List
                };

                // Generate the shared access signature on the container, setting the constraints directly on the signature.
                sasContainerToken = container.GetSharedAccessSignature(adHocPolicy, null);

                Console.WriteLine("SAS Container Token: {0}", sasContainerToken);
                Console.WriteLine();
            }
            else
            {
                // Generate the shared access signature on the container. In this case, all of the constraints for the
                // shared access signature are specified on the stored access policy, which is provided by name.
                // It is also possible to specify some constraints on an ad-hoc SAS and others on the stored access policy.
                sasContainerToken = container.GetSharedAccessSignature(null, storedPolicyName);
                Console.WriteLine("SAS for blob container (stored access policy): {0}", sasContainerToken);
                Console.WriteLine();
            }

            // Return the URI string for the container, including the SAS token.
            return container.Uri + sasContainerToken;
        }

        private async Task ListBlobsFlatListingAsync(CloudBlobContainer container, int? segmentSize)
        {
            // List blobs to the console window.
            //Console.WriteLine("List blobs in segments (flat listing):");
            //Console.WriteLine();

            int i = 0;
            BlobContinuationToken continuationToken = null;
            BlobResultSegment resultSegment = null;

            try
            {
                // Call ListBlobsSegmentedAsync and enumerate the result segment returned, while the continuation token is non-null.
                // When the continuation token is null, the last segment has been returned and execution can exit the loop.
                do
                {
                    // This overload allows control of the segment size. You can return all remaining results by passing null for the maxResults parameter, 
                    // or by calling a different overload.
                    // Note that requesting the blob's metadata as part of the listing operation 
                    // populates the metadata, so it's not necessary to call FetchAttributes() to read the metadata.
                    resultSegment = await container.ListBlobsSegmentedAsync(string.Empty, true, BlobListingDetails.Metadata, segmentSize, continuationToken, null, null);
                    if (resultSegment.Results.Count() > 0)
                    {
                        //Console.WriteLine("Page {0}:", ++i);
                    }

                    foreach (var blobItem in resultSegment.Results)
                    {
                        //Console.WriteLine("************************************");
                        //Console.WriteLine(blobItem.Uri);

                        if (blobItem.Uri.ToString().Contains("ReleaseNote.txt"))
                        {
                            CurrentReleaseNotes = (CloudBlob)blobItem;
                        }
                        // A flat listing operation returns only blobs, not virtual directories.
                        // Write out blob properties and metadata.
                        //if (blobItem is CloudBlob)
                        //{
                        //	PrintBlobPropertiesAndMetadata((CloudBlob)blobItem);
                        //}
                    }

                    //Console.WriteLine();

                    // Get the continuation token.
                    continuationToken = resultSegment.ContinuationToken;

                } while (continuationToken != null);
            }
            catch (StorageException e)
            {
                Console.WriteLine(e.Message);
                Console.ReadLine();
                throw;
            }
        }

#endif


    }



}
