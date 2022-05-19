using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Csla;
using Csla.Security;
using Csla.Core;
using Csla.Serialization;
using Csla.Silverlight;
//using Csla.Validation;
using System.ComponentModel;
using Csla.DataPortalClient;
using System.Threading;
using System.Threading.Tasks;
using Csla.Data;
using System.IO;

#if !SILVERLIGHT
using System.Data.SqlClient;
using BusinessLibrary.Data;
using BusinessLibrary.Security;
#if !CSLA_NETCORE
using System.Web.Hosting;
#endif
using System.Data;
#endif


namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class MagContReviewPipelineRunCommand : CommandBase<MagContReviewPipelineRunCommand>
    {

#if SILVERLIGHT
    public MagContReviewPipelineRunCommand(){}
#else
        public MagContReviewPipelineRunCommand() { }
#endif
        private string _CurrentMagVersion;
        private string _NextMagVersion;
        private double _scoreThreshold;
        private double _fosThreshold;
        private string _specificFolder;
        private int _reviewSampleSize;
        private int _MagLogId;
        private string _doWhat;

        public MagContReviewPipelineRunCommand(string CurrentMagVersion, string NextMagVersion,
            double scoreThreshold, double fosThreshold, string specificFolder, int magLogId, int reviewSampleSize,
            string doWhat = "")
        {
            _CurrentMagVersion = CurrentMagVersion;
            _NextMagVersion = NextMagVersion;
            _scoreThreshold = scoreThreshold;
            _fosThreshold = fosThreshold;
            _specificFolder = specificFolder;
            _MagLogId = magLogId;
            _reviewSampleSize = reviewSampleSize;
            _doWhat = doWhat;
        }

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_CurrentMagVersion", _CurrentMagVersion);
            info.AddValue("_NextMagVersion", _NextMagVersion);
            info.AddValue("_scoreThreshold", _scoreThreshold);
            info.AddValue("_fosThreshold", _fosThreshold);
            info.AddValue("_specificFolder", _specificFolder);
            info.AddValue("_reviewSampleSize", _reviewSampleSize);
            info.AddValue("_MagLogId", _MagLogId);
            info.AddValue("_doWhat", _doWhat);

        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, StateMode mode)
        {
            _CurrentMagVersion = info.GetValue<string>("_CurrentMagVersion");
            _NextMagVersion = info.GetValue<string>("_NextMagVersion");
            _scoreThreshold = info.GetValue<double>("_scoreThreshold");
            _fosThreshold = info.GetValue<double>("_fosThreshold");
            _specificFolder = info.GetValue<string>("_specificFolder");
            _reviewSampleSize = info.GetValue<int>("_reviewSampleSize");
            _MagLogId = info.GetValue<int>("_MagLogId");
            _doWhat = info.GetValue<string>("_doWhat");
        }


#if !SILVERLIGHT

        protected override void DataPortal_Execute()
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;

            if (_doWhat == "Prepare parquet")
            {
#if CSLA_NETCORE
            System.Threading.Tasks.Task.Run(() => doRunPipelinePrepareParquet(ri.ReviewId, ri.UserId));
#else
                //see: https://codingcanvas.com/using-hostingenvironment-queuebackgroundworkitem-to-run-background-tasks-in-asp-net/
                
                HostingEnvironment.QueueBackgroundWorkItem(cancellationToken => doRunPipelinePrepareParquet(ri.ReviewId, ri.UserId, cancellationToken));
#endif
                return;
            }

            if (_doWhat == "GetNewPaperIds")
            {
#if CSLA_NETCORE
            System.Threading.Tasks.Task.Run(() => doRunPipelineGetNewIds(ri.UserId));
#else
                HostingEnvironment.QueueBackgroundWorkItem(cancellationToken => doRunPipelineGetNewIds(ri.UserId, cancellationToken));
#endif
                return;
            }

            if (_doWhat == "CopyOpenAlexDataToAzureBlob")
            {
#if CSLA_NETCORE
            System.Threading.Tasks.Task.Run(() => doRunPipelineCopyOpenAlexDataToAzureBlob(ri.UserId));
#else
                HostingEnvironment.QueueBackgroundWorkItem(cancellationToken => doRunPipelineCopyOpenAlexDataToAzureBlob(ri.UserId, cancellationToken));
#endif
                return;
            }


            if (_specificFolder == "")
            {
#if CSLA_NETCORE
            System.Threading.Tasks.Task.Run(() => doRunPipeline(ri.ReviewId, ri.UserId));
#else
                //see: https://codingcanvas.com/using-hostingenvironment-queuebackgroundworkitem-to-run-background-tasks-in-asp-net/
                HostingEnvironment.QueueBackgroundWorkItem(cancellationToken => doRunPipeline(ri.ReviewId, ri.UserId, cancellationToken));
#endif
            }
            else
            {
#if CSLA_NETCORE
            System.Threading.Tasks.Task.Run(() => doDownloadResultsOnly(ri.ReviewId, ri.UserId));
#else
                //see: https://codingcanvas.com/using-hostingenvironment-queuebackgroundworkitem-to-run-background-tasks-in-asp-net/
                HostingEnvironment.QueueBackgroundWorkItem(cancellationToken => doDownloadResultsOnly(ri.ReviewId, ri.UserId, cancellationToken));
#endif
            }
        }

        private async void doRunPipeline(int ReviewId, int ContactId, CancellationToken cancellationToken = default(CancellationToken))
        {
            string prepare_data = "false";
            string process_train = "true";
            string process_inference = "true";
            string train_model = "true";
            string score_papers = "true";
            string uploadFileName = "";

#if (!CSLA_NETCORE)

            uploadFileName = System.Web.HttpRuntime.AppDomainAppPath + @"UserTempUploads/" + "crSeeds.tsv";
#else
            const string TempPath = @"UserTempUploads/";
            uploadFileName = TempPath + "crSeeds.tsv";

            if (Directory.Exists(TempPath))
            {
                uploadFileName = @"./" + TempPath + "/" + "crSeeds.tsv";
            }
            else
            {
                DirectoryInfo tmpDir = System.IO.Directory.CreateDirectory(TempPath);
                uploadFileName = tmpDir.FullName + "/" + @"./" + TempPath + "/" + "crSeeds.tsv";
            }
#endif
            int logId = MagLog.SaveLogEntry("ContReview process", "running", "Main update. starting", ContactId);
            
            string folderPrefix = TrainingRunCommand.NameBase + Guid.NewGuid().ToString();
            int SeedIds = WriteSeedIdsFile(uploadFileName);
            if (SeedIds == -1)
            {
                MagLog.UpdateLogEntry("Stopped", "Main update. SeedIds: " + SeedIds.ToString() + " Folder:" + folderPrefix,
                ContactId);
                return;
            }
            MagLog.UpdateLogEntry("running", "Main update. SeedIds: " + SeedIds.ToString() + " Folder:" + folderPrefix,
                ContactId);
            
            if (!await UploadSeedIdsFileToBlobAsync(uploadFileName, folderPrefix))
            {
                MagLog.UpdateLogEntry("Stopped", "Failed upload seed ids" + " Folder:" + folderPrefix,
                ContactId);
                return;
            }
            MagLog.UpdateLogEntry("running", "Main update. SeedIds uploaded: " + SeedIds.ToString() +
                " Folder:" + folderPrefix, logId);

            if (!WriteNewIdsFileOnBlob(uploadFileName, ContactId, folderPrefix, cancellationToken))
            {
                MagLog.UpdateLogEntry("Stopped", "Failed getting new IDs" + " Folder:" + folderPrefix,
                ContactId);
                return;
            }
            MagLog.UpdateLogEntry("running", "Main update. NewIds written (" + SeedIds.ToString() + ")" +
                " Folder:" + folderPrefix, logId);

            string result = MagContReviewPipeline.runADFPipeline(ContactId, Path.GetFileName(uploadFileName), "NewPapers.tsv",
                "crResults.tsv", "cr_per_paper_tfidf.pickle", _NextMagVersion, _fosThreshold.ToString(), folderPrefix,
                _scoreThreshold.ToString(), "v1", "True", _reviewSampleSize.ToString(), prepare_data, process_train,
                process_inference, train_model, score_papers, cancellationToken);
            if (result == "Succeeded")
            {
                MagLog.UpdateLogEntry("running", "Main update. ADFPipelineComplete (" + SeedIds.ToString() + ")" +
                    " Folder:" + folderPrefix, logId);
                int NewIds = DownloadResultsAsync(folderPrefix, ReviewId);

                if (NewIds == -1)
                {
                    MagLog.UpdateLogEntry("failed", "On downloading IDs. ADFPipelineComplete (" + SeedIds.ToString() + ")" +
                    " Folder:" + folderPrefix, logId);
                }
                else
                {
                    MagLog.UpdateLogEntry("Complete", "MAG version: " + _NextMagVersion + "; SeedIds: " + SeedIds.ToString() + "; NewIds: " +
                        NewIds.ToString() + " FoS:" + _fosThreshold.ToString() + "; Score threshold: " + _scoreThreshold.ToString() +
                        " Folder:" + folderPrefix, logId);
                }
            }
            else
            {
                MagLog.UpdateLogEntry("failed", "Main update failed at run contreview. Error: " + result + " Folder:" + folderPrefix, logId);
            }
        }

        private async void doRunPipelineGetNewIds(int ContactId, CancellationToken cancellationToken = default(CancellationToken))
        {
            string folderName;
#if (!CSLA_NETCORE)

            folderName = System.Web.HttpRuntime.AppDomainAppPath + @"UserTempUploads";
#else
                // same as comment above for same line
                //SG Edit:
                DirectoryInfo tmpDir = System.IO.Directory.CreateDirectory("UserTempUploads");
                folderName = tmpDir.FullName + "/" + @"UserTempUploads";
#endif
            int logId = MagLog.SaveLogEntry("New MAG setup", "running", "Getting new Ids: " + _NextMagVersion, ContactId);
            string lastSavedMagVersion = "";
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_MagCheckNewPapersAlreadyDownloaded", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        if (reader.Read())
                        {
                            lastSavedMagVersion = reader["MagVersion"].ToString();
                        }
                    }
                }
                connection.Close();
            }
            if (lastSavedMagVersion == _NextMagVersion)
            {
                MagLog.UpdateLogEntry("Complete", "Process aborted: new papers already downloaded", logId);
            }
            else
            {
                if (!MagDataLakeHelpers.ExecProc(@"[master].[dbo].[GetNewPaperIds](""OpenAlexData/" + this._CurrentMagVersion + "\",\"OpenAlexData/" +
                this._NextMagVersion + "\");", true, "GetNewPaperIds", ContactId, 10, cancellationToken))
                {
                    MagLog.UpdateLogEntry("Stopped", "Data lake job failed", logId);
                    return;
                }

                downloadNewIds(logId);
            }
        }

        private bool downloadNewIds(int logId)
        {
            int paperCount = 0;


            //var configuration = ERxWebClient2.Startup.Configuration.GetSection("AzureMagSettings");
            string storageAccountName = AzureSettings.MAGStorageAccount;
            string storageAccountKey = AzureSettings.MAGStorageAccountKey;
            string storageAccountConnectionString = "DefaultEndpointsProtocol=https;AccountName=" + storageAccountName
                + ";AccountKey=" + storageAccountKey;
            
            //CloudStorageAccount storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=" +
            //    ERxWebClient2.Startup.Configuration.GetSection("MAGStorageAccount") + ";AccountKey=" +
            //    ERxWebClient2.Startup.Configuration.GetSection("MAGStorageAccountKey"));

            //CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            //CloudBlobContainer container = blobClient.GetContainerReference("open-alex");
            //CloudBlockBlob blockBlobDataResults = container.GetBlockBlobReference("OpenAlexData/" + _NextMagVersion + "/NewPaperIds.tsv");

            try
            {
                //string Results1 = await blockBlobDataResults.DownloadTextAsync();
                //byte[] myFile = Encoding.UTF8.GetBytes(Results1);

                DataTable dt = new DataTable("TB_MAG_NEW_PAPERS");
                dt.Columns.Add("MAG_NEW_PAPERS_ID");
                dt.Columns.Add("PaperId");
                dt.Columns.Add("MagVersion");

                MemoryStream msIds = BlobOperations.DownloadBlobAsMemoryStream(storageAccountConnectionString, "open-alex", "OpenAlexData/" + _NextMagVersion + "/NewPaperIds.tsv"); 
                using (var readerIds = new StreamReader(msIds))
                {
                    string line;
                    while ((line = readerIds.ReadLine()) != null)
                    {
                        DataRow newRow = dt.NewRow();
                        newRow["MAG_NEW_PAPERS_ID"] = 0; // IDENTITY COLUMN - AUTO-GENERATED
                        newRow["PaperId"] = Convert.ToInt64(line);
                        newRow["MagVersion"] = _NextMagVersion;
                        dt.Rows.Add(newRow);
                        paperCount++;
                    }
                }

                using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
                {
                    connection.Open();
                    using (SqlBulkCopy sbc = new SqlBulkCopy(connection))
                    {
                        sbc.DestinationTableName = "TB_MAG_NEW_PAPERS";
                        sbc.BatchSize = 1000;
                        sbc.WriteToServer(dt);
                    }
                    connection.Close();
                    MagLog.UpdateLogEntry("Complete", "New Ids saved: " + _NextMagVersion + ", count=" + paperCount.ToString(), logId);
                }
            }
            catch
            {
                MagLog.UpdateLogEntry("Stopped", "Error downloading new IDs", logId);
                return false;
            }
            return true;
        }

        private async void doRunPipelineCopyOpenAlexDataToAzureBlob(int ContactId, CancellationToken cancellationToken = default(CancellationToken))
        {
#if (!CSLA_NETCORE)
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"S3Path", "data_dump_v1/" + _NextMagVersion},
                {"BlobPath", "OpenAlexData/" + _NextMagVersion},
                {"In_MagRootUri", "wasb://open-alex@eppimag/OpenAlexData/" + _NextMagVersion + "/" },
                {"Out_OutputPath", "wasb://open-alex@eppimag/makes/" + _NextMagVersion + "/microsoft/entities" }
            };
            DataFactoryHelper.RunDataFactoryProcess("Get OpenAlex data", parameters, true, ContactId, cancellationToken);
#endif
        }

        private async void doRunPipelinePrepareParquet(int ReviewId, int ContactId, CancellationToken cancellationToken = default(CancellationToken))
        {
//            string folderName;
//#if (!CSLA_NETCORE)

//            folderName = System.Web.HttpRuntime.AppDomainAppPath + @"UserTempUploads";
//#else
//                // same as comment above for same line
//                //SG Edit:
//                DirectoryInfo tmpDir = System.IO.Directory.CreateDirectory("UserTempUploads");
//                folderName = tmpDir.FullName + "/" + @"UserTempUploads";
//#endif
            int logId = MagLog.SaveLogEntry("ContReview process", "running", "Updating parquet to: " + _NextMagVersion, ContactId);
            string result = MagContReviewPipeline.runADFPipeline(ContactId, "NoTrainFile", "NoInferenceFile",
                "NoResultsFile", "NoModelFile", _NextMagVersion, _fosThreshold.ToString(), "",
                _scoreThreshold.ToString(), "v1", "True", _reviewSampleSize.ToString(), "true", "false",
                "false", "false", "false");
            if (result == "Succeeded")
            {
                MagLog.UpdateLogEntry("Complete", "Updated parquet to: " + _NextMagVersion, logId);
            }
            else
            {
                MagLog.UpdateLogEntry("failed", "Update parquet failed. Error: " + result, logId);
            }
        }

        private void doDownloadResultsOnly(int ReviewId, int ContactId, CancellationToken cancellationToken = default(CancellationToken))
        {
            int NewIds = DownloadResultsAsync(_specificFolder, ReviewId);
            if (NewIds == -1)
            {
                MagLog.UpdateLogEntry("Stopped", "IDs downloaded = -1. Folder:" +
                _specificFolder, _MagLogId);
                return;
            }
            if (cancellationToken.IsCancellationRequested)
            {
                MagLog.UpdateLogEntry("Stopped", "Cancellation token received. Folder:" +
                _specificFolder, _MagLogId);
                return;
            }
            MagLog.UpdateLogEntry("Complete", "Main update downloaded on request. Folder:" +
                _specificFolder, _MagLogId);
        }


        // the stored procedure also creates a line in TB_MAG_AUTOUPDATE_RUN for each update in each review
        private int WriteSeedIdsFile(string uploadFileName)
        {
            int lineCount = 0;
            try
            {
                using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand("st_MagContReviewRunGetSeedIds", connection))
                    {
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@MAG_VERSION", _NextMagVersion));
                        using (SafeDataReader reader = new SafeDataReader(command.ExecuteReader()))
                        {
                            if (reader.Read())
                            {
                                using (StreamWriter file = new StreamWriter(uploadFileName, false))
                                {
                                    while (reader.Read())
                                    {
                                        file.WriteLine(reader["PaperId"].ToString() + "\t" +
                                            reader["AutoUpdateId"].ToString() + "\t" +
                                            reader["Included"].ToString());
                                        lineCount++;
                                    }
                                }
                            }
                        }
                    }
                    connection.Close();
                }
            }
            catch
            {
                lineCount = -1;
            }
            return lineCount;
        }

        private async Task<bool> UploadSeedIdsFileToBlobAsync(string fileName, string folderPrefix)
        {
            string storageAccountName = AzureSettings.MAGStorageAccount;
            string storageAccountKey = AzureSettings.MAGStorageAccountKey;

            string storageConnectionString =
                "DefaultEndpointsProtocol=https;AccountName=" + storageAccountName + ";AccountKey=";
            storageConnectionString += storageAccountKey;

            //CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            //CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            //CloudBlobContainer container = blobClient.GetContainerReference("experiments");
            //CloudBlockBlob blockBlobData;

            try
            {
                //blockBlobData = container.GetBlockBlobReference(folderPrefix + "/" + Path.GetFileName(fileName));
                using (var fileStream = System.IO.File.OpenRead(fileName))
                {
                    BlobOperations.UploadStream(storageConnectionString, "experiments", folderPrefix + "/" + Path.GetFileName(fileName), fileStream);
                }
                File.Delete(fileName);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool WriteNewIdsFileOnBlob(string uploadFileName, int ContactId, string folderPrefix, CancellationToken cancellationToken)
        {
            if (MagDataLakeHelpers.ExecProc(@"[master].[dbo].[GetNewPaperIdsPreviousYear](""OpenAlexData/" + this._CurrentMagVersion + "\",\"OpenAlexData/" +
                this._NextMagVersion + "\",\"" + folderPrefix + "/NewPapers.tsv" + "\",\"" + "experiments" + "\");", true,
                "GetNewPaperIds", ContactId, 10, cancellationToken))
                return true;
            else
                return false;
        }

        private int DownloadResultsAsync(string folder, int ReviewId)
        {
            string storageAccountName = AzureSettings.MAGStorageAccount;
            string storageAccountKey = AzureSettings.MAGStorageAccountKey;

            //CloudBlobDirectory dir;
            //CloudBlob blob;
            //BlobContinuationToken continuationToken = null;
            int lineCount = 0;

            string storageConnectionString =
                "DefaultEndpointsProtocol=https;AccountName=" + storageAccountName + ";AccountKey=";
            storageConnectionString += storageAccountKey;

            try
            {
                //CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);
                //CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                //CloudBlobContainer container = blobClient.GetContainerReference("experiments");
                //string JobId = Guid.NewGuid().ToString();
                //do
                //{
                    List<BlobInHierarchy> resultSegment = BlobOperations.Blobfilenames(storageConnectionString, "experiments", folder + "/tmp");
                    //BlobResultSegment resultSegment = await container.ListBlobsSegmentedAsync(folder + "/tmp",
                    //true, BlobListingDetails.Metadata, 100, continuationToken, null, null);

                    //foreach (var blobItem in resultSegment.Results)
                    foreach (BlobInHierarchy blobItem in resultSegment)
                    {
                        //// A hierarchical listing may return both virtual directories and blobs.
                        //if (blobItem is CloudBlobDirectory)
                        //{
                        //    dir = (CloudBlobDirectory)blobItem;
                        //}
                        //else
                        if (blobItem.IsFile)
                        {
                            //blob = (CloudBlob)blobItem;
                            //if (blob.Name.StartsWith(folder + "/tmp/part"))
                            if (blobItem.BlobName.StartsWith(folder + "/tmp"))
                            {
                                //CloudBlockBlob blockBlobDownloadData = container.GetBlockBlobReference(blob.Name);
                                //string resultantString = await blockBlobDownloadData.DownloadTextAsync();
                                //byte[] myFile = Encoding.UTF8.GetBytes(resultantString);

                                MemoryStream ms = BlobOperations.DownloadBlobAsMemoryStream(storageConnectionString, "experiments", blobItem.BlobName) ;// new MemoryStream(myFile);

                                DataTable dt = new DataTable("Ids");
                                dt.Columns.Add("MAG_AUTO_UPDATE_RUN_PAPER_ID");
                                dt.Columns.Add("MAG_AUTO_UPDATE_RUN_ID");
                                dt.Columns.Add("REVIEW_ID");
                                dt.Columns.Add("PaperId");
                                dt.Columns.Add("ContReviewScore");
                                dt.Columns.Add("UserClassifierScore");
                                dt.Columns.Add("StudyTypeClassiferScore");

                                using (var reader = new StreamReader(ms))
                                {
                                    string line;
                                    bool firstTime = true; // column headers
                                    while ((line = reader.ReadLine()) != null)
                                    {
                                        if (firstTime == false)
                                        {
                                            string[] fields = line.Split('\t');
                                            DataRow newRow = dt.NewRow();
                                            newRow["MAG_AUTO_UPDATE_RUN_PAPER_ID"] = 0;
                                            newRow["MAG_AUTO_UPDATE_RUN_ID"] = fields[1];
                                            newRow["REVIEW_ID"] = null;
                                            newRow["PaperId"] = fields[0];
                                            newRow["ContReviewScore"] = fields[2];
                                            newRow["UserClassifierScore"] = 0;
                                            newRow["StudyTypeClassiferScore"] = 0;
                                            dt.Rows.Add(newRow);
                                            lineCount++;
                                        }
                                        else
                                        {
                                            firstTime = false;
                                        }
                                    }
                                }

                                using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
                                {
                                    connection.Open();
                                    using (SqlBulkCopy sbc = new SqlBulkCopy(connection))
                                    {
                                        sbc.DestinationTableName = "TB_MAG_AUTO_UPDATE_RUN_PAPER";
                                        sbc.BatchSize = 1000;
                                        sbc.WriteToServer(dt);
                                    }
                                }
                                //blockBlobDownloadData.DeleteIfExists();
                            }

                        }
                    }

                    // Get the continuation token and loop until it is null.
                    //continuationToken = resultSegment.ContinuationToken;

                //} while (continuationToken != null);
                using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand("st_MagContReviewInsertResults", connection))
                    {
                        command.Parameters.Add(new SqlParameter("@MAG_VERSION", _NextMagVersion));
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.ExecuteNonQuery();
                    }
                    connection.Close();
                }
            }
            catch
            {
                lineCount = -1;
            }
            return lineCount;
        }







#endif


        }
    }
