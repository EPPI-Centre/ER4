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
using System.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
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

        public MagContReviewPipelineRunCommand(string CurrentMagVersion, string NextMagVersion,
            double scoreThreshold, double fosThreshold, string specificFolder, int magLogId, int reviewSampleSize)
        {
            _CurrentMagVersion = CurrentMagVersion;
            _NextMagVersion = NextMagVersion;
            _scoreThreshold = scoreThreshold;
            _fosThreshold = fosThreshold;
            _specificFolder = specificFolder;
            _MagLogId = magLogId;
            _reviewSampleSize = reviewSampleSize;
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
        }


#if !SILVERLIGHT

        protected override void DataPortal_Execute()
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            if (_specificFolder == "")
            {
                Task.Run(() => { doRunPipeline(ri.ReviewId, ri.UserId); });
            }
            else
            {
                Task.Run(() => { doDownloadResultsOnly(ri.ReviewId, ri.UserId); });
            }
        }

        private async void doRunPipeline(int ReviewId, int ContactId)
        {

            string uploadFileName = "";
            /* Commenting out to see if the alternative below works on Jeff's laptop
            if (Directory.Exists("UserTempUploads"))
            {
                uploadFileName = @"UserTempUploads/" + "crSeeds.tsv";
            }
            else
            {
                DirectoryInfo tmpDir = System.IO.Directory.CreateDirectory("UserTempUploads");
                uploadFileName = tmpDir.FullName + "/" + @"UserTempUploads/" + "crSeeds.tsv";

            }
            */

#if (!CSLA_NETCORE)

            uploadFileName = System.Web.HttpRuntime.AppDomainAppPath + @"UserTempUploads/" + "crSeeds.tsv";
#else
            if (Directory.Exists("UserTempUploads"))
            {
                uploadFileName = @"UserTempUploads\" + "crSeeds.tsv";
            }
            else
            {
                DirectoryInfo tmpDir = System.IO.Directory.CreateDirectory("UserTempUploads");
                uploadFileName = tmpDir.FullName + @"\UserTempUploads\crSeeds.tsv";

            }
#endif
            int logId = MagLog.SaveLogEntry("ContReview process", "running", "Main update. starting", ContactId);
            
            string folderPrefix = TrainingRunCommand.NameBase + Guid.NewGuid().ToString();
            int SeedIds = WriteSeedIdsFile(uploadFileName);
            MagLog.UpdateLogEntry("running", "Main update. SeedIds: " + SeedIds.ToString() + " Folder:" + folderPrefix,
                ContactId);
            
            await UploadSeedIdsFileToBlobAsync(uploadFileName, folderPrefix);
            MagLog.UpdateLogEntry("running", "Main update. SeedIds uploaded: " + SeedIds.ToString() +
                " Folder:" + folderPrefix, logId);

            WriteNewIdsFileOnBlob(uploadFileName, ContactId, folderPrefix);
            MagLog.UpdateLogEntry("running", "Main update. NewIds written (" + SeedIds.ToString() + ")" +
                " Folder:" + folderPrefix, logId);

            if ((MagContReviewPipeline.runADFPieline(ContactId, Path.GetFileName(uploadFileName), "NewPapers.tsv",
                "crResults.tsv", "cr_per_paper_tfidf.pickle", _NextMagVersion, _fosThreshold.ToString(), folderPrefix,
                _scoreThreshold.ToString(), "v1", "True", _reviewSampleSize.ToString())) == "Succeeded")
            {
                MagLog.UpdateLogEntry("running", "Main update. ADFPipelineComplete (" + SeedIds.ToString() + ")" +
                    " Folder:" + folderPrefix, logId);
                int NewIds = await DownloadResultsAsync(folderPrefix + "/crResults.tsv", ReviewId);
                MagLog.UpdateLogEntry("Complete", "Main update. SeedIds: " + SeedIds.ToString() + "; NewIds: " +
                    NewIds.ToString() + " FoS:" + _fosThreshold.ToString() + "Score threshold: " + _scoreThreshold.ToString() +
                    " Folder:" + folderPrefix, logId);
            }
            else
            {
                MagLog.UpdateLogEntry("failed", "Main update failed at run contreview", logId);
            }
            //Thread.Sleep(30 * 1000); int NewIds = 10; int SeedIds = 10; // this line for testing - can be deleted after publish
            
        }

        private async void doDownloadResultsOnly(int ReviewId, int ContactId)
        {
            int NewIds = await DownloadResultsAsync(_specificFolder + "/crResults.tsv", ReviewId);
            MagLog.UpdateLogEntry("Complete", "Main update downloaded on request. Folder:" +
                _specificFolder, _MagLogId);
        }

        private int WriteSeedIdsFile(string uploadFileName)
        {
            int lineCount = 0;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_MagContReviewRunGetSeedIds", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    using (SafeDataReader reader = new SafeDataReader(command.ExecuteReader()))
                    {
                        if (reader.Read())
                        {
                            using (StreamWriter file = new StreamWriter(uploadFileName, false))
                            {
                                while (reader.Read())
                                {
                                    file.WriteLine(reader["PaperId"].ToString() + "\t" +
                                        reader["RelatedRunId"].ToString() + "\t" +
                                        reader["Included"].ToString());
                                    lineCount++;
                                }
                            }
                        }
                    }
                }
                connection.Close();
            }
            return lineCount;
        }

        private async Task UploadSeedIdsFileToBlobAsync(string fileName, string folderPrefix)
        {
#if (CSLA_NETCORE)

            var configuration = ERxWebClient2.Startup.Configuration.GetSection("AzureMagSettings");
            string storageAccountName = configuration["MAGStorageAccount"];
            string storageAccountKey = configuration["MAGStorageAccountKey"];

#else
            string storageAccountName = ConfigurationManager.AppSettings["MAGStorageAccount"];
            string storageAccountKey = ConfigurationManager.AppSettings["MAGStorageAccountKey"];
#endif

            string storageConnectionString =
                "DefaultEndpointsProtocol=https;AccountName=" + storageAccountName + ";AccountKey=";
            storageConnectionString += storageAccountKey;

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("experiments");
            CloudBlockBlob blockBlobData;

            blockBlobData = container.GetBlockBlobReference(folderPrefix + "/" + Path.GetFileName(fileName));
            using (var fileStream = System.IO.File.OpenRead(fileName))
            {


#if (!CSLA_NETCORE)
                blockBlobData.UploadFromStream(fileStream);
#else

					await blockBlobData.UploadFromFileAsync(fileName);
#endif

            }
            File.Delete(fileName);
        }

        private void WriteNewIdsFileOnBlob(string uploadFileName, int ContactId, string folderPrefix)
        {
            MagDataLakeHelpers.ExecProc(@"[master].[dbo].[GetNewPaperIds](""" + this._CurrentMagVersion + "\",\"" +
                this._NextMagVersion + "\",\"" + folderPrefix + "/NewPapers.tsv" + "\",\"" + "experiments" + "\");", true, "GetNewPaperIds", ContactId, 10);
        }

        private async Task<int> DownloadResultsAsync(string fileName, int ReviewId)
        {
#if (CSLA_NETCORE)

            var configuration = ERxWebClient2.Startup.Configuration.GetSection("AzureMagSettings");
            string storageAccountName = configuration["MAGStorageAccount"];
            string storageAccountKey = configuration["MAGStorageAccountKey"];

#else
            string storageAccountName = ConfigurationManager.AppSettings["MAGStorageAccount"];
            string storageAccountKey = ConfigurationManager.AppSettings["MAGStorageAccountKey"];
#endif

            string storageConnectionString =
                "DefaultEndpointsProtocol=https;AccountName=" + storageAccountName + ";AccountKey=";
            storageConnectionString += storageAccountKey;

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("experiments");

            CloudBlockBlob blockBlobDownloadData = container.GetBlockBlobReference(fileName);
            string resultantString = await blockBlobDownloadData.DownloadTextAsync();
            byte[] myFile = Encoding.UTF8.GetBytes(resultantString);
            
            MemoryStream ms = new MemoryStream(myFile);

            DataTable dt = new DataTable("Ids");
            dt.Columns.Add("MAG_RELATED_RUN_ID");
            dt.Columns.Add("PaperId");
            dt.Columns.Add("SimilarityScore");
            dt.Columns.Add("JobId");

            string JobId = Guid.NewGuid().ToString();
            int lineCount = 0;

            using (var reader = new StreamReader(ms))
            {
                string line;
                bool firstTime = true; // column headers
                while ((line = reader.ReadLine()) != null)
                {
                    if (firstTime == false)
                    {
                        string [] fields = line.Split('\t');
                        DataRow newRow = dt.NewRow();
                        newRow["MAG_RELATED_RUN_ID"] = fields[1];
                        newRow["PaperId"] = fields[0];
                        newRow["SimilarityScore"] = fields[2];
                        newRow["JobId"] = JobId;
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
                    sbc.DestinationTableName = "TB_MAG_RELATED_PAPERS_TEMP";
                    sbc.BatchSize = 1000;
                    sbc.WriteToServer(dt);
                }

                using (SqlCommand command = new SqlCommand("st_MagContReviewInsertResults", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@JobId", JobId));
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
            //blockBlobDownloadData.DeleteIfExists();
            return lineCount;
        }







#endif


    }
}
