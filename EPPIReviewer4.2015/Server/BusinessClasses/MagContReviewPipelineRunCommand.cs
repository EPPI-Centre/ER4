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

        public MagContReviewPipelineRunCommand(string CurrentMagVersion, string NextMagVersion)
        {
            _CurrentMagVersion = CurrentMagVersion;
            _NextMagVersion = NextMagVersion;
        }

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_CurrentMagVersion", _CurrentMagVersion);
            info.AddValue("_NextMagVersion", _NextMagVersion);
        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, StateMode mode)
        {
            _CurrentMagVersion = info.GetValue<string>("_CurrentMagVersion");
            _NextMagVersion = info.GetValue<string>("_NextMagVersion");
        }


#if !SILVERLIGHT

        protected override void DataPortal_Execute()
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            Task.Run(() => { doRunPipeline(ri.ReviewId, ri.UserId); });
        }

        private void doRunPipeline(int ReviewId, int ContactId)
        {

            string uploadFileName = "";
            if (Directory.Exists("UserTempUploads"))
            {
                uploadFileName = @"UserTempUploads/" + "crSeeds.tsv";
            }
            else
            {
                DirectoryInfo tmpDir = System.IO.Directory.CreateDirectory("UserTempUploads");
                uploadFileName = tmpDir.FullName + "/" + @"UserTempUploads/" + "crSeeds.tsv";

            }
            string folderPrefix = Guid.NewGuid().ToString();
            int SeedIds = WriteSeedIdsFile(uploadFileName);
            int logId = MagLog.SaveLogEntry("ContReview", "started", "SeedIds: " + SeedIds.ToString(), ContactId);

            UploadSeedIdsFileToBlobAsync(uploadFileName, folderPrefix);
            MagLog.UpdateLogEntry("Running", "SeedIds uploaded: " + SeedIds.ToString(), logId);

            WriteNewIdsFileOnBlob(uploadFileName, ContactId, folderPrefix);
            MagLog.UpdateLogEntry("Running", "NewIds written (" + SeedIds.ToString() + ")", logId);

            MagContReviewPipeline.runADFPieline(ContactId, Path.GetFileName(uploadFileName), "NewPapers.tsv",
                "crResults.tsv", "cr_per_paper_tfidf.pickle", _NextMagVersion, "1", folderPrefix, "0.01");
            MagLog.UpdateLogEntry("Running", "ADFPipelineComplete (" + SeedIds.ToString() + ")", logId);

            //folderPrefix = "56760d2d-e044-4f6f-8718-9a43c4e30a77";
            Task<int> NewIds = DownloadResultsAsync(folderPrefix + "/crResults.tsv", ReviewId);
            NewIds.Wait();
            MagLog.UpdateLogEntry("Complete", "SeedIds: " + SeedIds.ToString() + "; NewIds: " + NewIds.Result.ToString(), logId);
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
            var configuration = ERxWebClient2.Startup.Configuration.GetSection("AzureMagSettings");
            string storageAccountName = configuration["MAGStorageAccount"];
            string storageAccountKey = configuration["MAGStorageAccountKey"];

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
            var configuration = ERxWebClient2.Startup.Configuration.GetSection("AzureMagSettings");
            string storageAccountName = configuration["MAGStorageAccount"];
            string storageAccountKey = configuration["MAGStorageAccountKey"];

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
