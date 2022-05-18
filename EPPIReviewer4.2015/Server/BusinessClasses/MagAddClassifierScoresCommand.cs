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
using Azure.Storage.Blobs;
using Azure.Storage;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
#if !CSLA_NETCORE
using System.Web.Hosting;
#endif
using System.Data;
#endif


namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class MagAddClassifierScoresCommand : CommandBase<MagAddClassifierScoresCommand>
    {

#if SILVERLIGHT
    public MagAddClassifierScoresCommand(){}
#else
        public MagAddClassifierScoresCommand() { }
#endif
        private int _MagAutoUpdateRunId;
        private int _TopN;
        private string _StudyTypeClassifier;
        private int _UserClassifierModelId;
        private int _UserClassifierReviewId;

        public MagAddClassifierScoresCommand(int MagAutoUpdateRunId, int TopN, string StudyTypeClassifier,
            int UserClassifierModelId, int UserClassifierReviewId)
        {
            _MagAutoUpdateRunId = MagAutoUpdateRunId;
            _TopN = TopN;
            _StudyTypeClassifier = StudyTypeClassifier;
            _UserClassifierModelId = UserClassifierModelId;
            _UserClassifierReviewId = UserClassifierReviewId;
        }

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_MagAutoUpdateRunId", _MagAutoUpdateRunId);
            info.AddValue("_TopN", _TopN);
            info.AddValue("_StudyTypeClassifier", _StudyTypeClassifier);
            info.AddValue("_UserClassifierModelId", _UserClassifierModelId);
            info.AddValue("_UserClassifierReviewId", _UserClassifierReviewId);
        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, StateMode mode)
        {
            _MagAutoUpdateRunId = info.GetValue<int>("_MagAutoUpdateRunId");
            _TopN = info.GetValue<int>("_TopN");
            _StudyTypeClassifier = info.GetValue<string>("_StudyTypeClassifier");
            _UserClassifierModelId = info.GetValue<int>("_UserClassifierModelId");
            _UserClassifierReviewId = info.GetValue<int>("_UserClassifierReviewId");
        }


#if !SILVERLIGHT

        protected override void DataPortal_Execute()
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;

            // Run in separate thread and return this object to client
#if CSLA_NETCORE
            System.Threading.Tasks.Task.Run(() => AddClassifierScores(ri.ReviewId.ToString(), ri.UserId));
#else
            //see: https://codingcanvas.com/using-hostingenvironment-queuebackgroundworkitem-to-run-background-tasks-in-asp-net/
            HostingEnvironment.QueueBackgroundWorkItem(cancellationToken => AddClassifierScores(ri.ReviewId.ToString(), ri.UserId, cancellationToken));
#endif
            
        }

        private async Task AddClassifierScores(string ReviewId, int ContactId, CancellationToken cancellationToken = default(CancellationToken))
        {
            //UpdateSimulationRecord("Running classifiers");
            List<Int64> Ids = new List<long>();
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_MagAutoUpdateRunResults", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@MagAutoUpdateRunId", _MagAutoUpdateRunId));
                    command.Parameters.Add(new SqlParameter("@OrderBy", "AutoUpdate"));
                    command.Parameters.Add(new SqlParameter("@AutoUpdateScore", Convert.ToDouble(0.0)));
                    command.Parameters.Add(new SqlParameter("@StudyTypeClassifierScore", Convert.ToDouble(0.0)));
                    command.Parameters.Add(new SqlParameter("@UserClassifierScore", Convert.ToDouble(0.0)));
                    command.Parameters.Add(new SqlParameter("@TopN", _TopN));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        while (reader.Read())
                        {
                            Ids.Add(reader.GetInt64("PaperId"));
                        }
                    }
                }
                connection.Close();
            }

            if (Ids.Count == 0)
            {
                MagLog.SaveLogEntry("Add classifier scores", "Failed", "No IDs. Review: " + ReviewId, ContactId);
                return;
            }

#if (!CSLA_NETCORE)
            //string filePrefix = TrainingRunCommand.NameBase + Guid.NewGuid().ToString();
            string fName = System.Web.HttpRuntime.AppDomainAppPath + @"UserTempUploads/Cont" + _MagAutoUpdateRunId.ToString() + ".csv";
#else
                // same as comment above for same line
                //SG Edit:
                DirectoryInfo tmpDir = System.IO.Directory.CreateDirectory("UserTempUploads");
                string fName = tmpDir.FullName + "/Cont" + _MagAutoUpdateRunId.ToString() + ".csv";
                //string fName = AppDomain.CurrentDomain.BaseDirectory + TempPath + "ReviewID" + ri.ReviewId + "ContactId" + ri.UserId.ToString() + ".csv";
#endif
            try
            {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(fName))
                {
                    file.WriteLine("\"ITEM_ID\",\"LABEL\",\"TITLE\",\"ABSTRACT\",\"KEYWORDS\",\"REVIEW_ID\"");
                    int count = 0;
                    while (count < Ids.Count)
                    {
                        string query = "";
                        for (int i = count; i < Ids.Count && i < count + 100; i++)
                        {
                            if (query == "")
                            {
                                query = "Id=" + Ids[i].ToString();
                            }
                            else
                            {
                                query += ",Id=" + Ids[i].ToString();
                            }
                        }
                        MagMakesHelpers.PaperMakesResponse resp = MagMakesHelpers.EvaluateExpressionNoPagingWithCount("OR(" + query + ")", "100");

                        foreach (MagMakesHelpers.PaperMakes pm in resp.entities)
                        {
                            file.WriteLine("\"" + pm.Id.ToString() + "\"," +
                                            "\"" + "99" + "\"," +
                                            "\"" + MagMakesHelpers.CleanText(pm.Ti) + "\"," +
                                            "\"" + MagMakesHelpers.CleanText(MagMakesHelpers.ReconstructInvertedAbstract(pm.IA)) + "\"," +
                                            "\"" + "" + "\"," +
                                            "\"" + ReviewId + "\"");
                        }
                        count += 100;
                    }
                }
                if (cancellationToken.IsCancellationRequested)
                {
                    MagLog.SaveLogEntry("Add classifier scores", "Cancelled", "Cancellation token, before uploading. Review: " + ReviewId, ContactId);
                    File.Delete(fName);
                    return;
                }
            }
            catch
            {
                MagLog.SaveLogEntry("Add classifier scores", "Failed", "Writing local file. Review: " + ReviewId, ContactId);
                return;
            }

            // this copied from ClassifierCommand. The keys should move to web.config
            Microsoft.Azure.Storage.CloudStorageAccount storageAccount = CloudStorageAccount.Parse("***REMOVED***");
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("attributemodeldata");
            CloudBlockBlob blockBlobData;

            try
            {
                string uploadedFileName = TrainingRunCommand.NameBase + "Cont" + _MagAutoUpdateRunId.ToString() + "StudyModelToScore.csv";
                string resultsFile1 = @"attributemodels/" + TrainingRunCommand.NameBase + "Cont" + _MagAutoUpdateRunId.ToString() + "Results1.csv";
                string resultsFile2 = @"attributemodels/" + TrainingRunCommand.NameBase + "Cont" + _MagAutoUpdateRunId.ToString() + "Results2.csv";
                blockBlobData = container.GetBlockBlobReference(uploadedFileName);
                using (var fileStream = System.IO.File.OpenRead(fName))
                {


#if (!CSLA_NETCORE)
                    blockBlobData.UploadFromStream(fileStream);
#else

					await blockBlobData.UploadFromFileAsync(fName);
#endif

                }
                File.Delete(fName);

                if (cancellationToken.IsCancellationRequested)
                {
                    MagLog.SaveLogEntry("Add classifier scores", "Cancelled", "Cancellation token, before InvokeBatchExecution. Review: " + ReviewId, ContactId);
                    return;
                } else {
                    MagLog.SaveLogEntry("Add classifier scores", "Started", "Data is ok, will InvokeBatchExecution. Review:" + ReviewId, ContactId);
                }

                if (_StudyTypeClassifier != "None")
                {
                    int classifierId = 0;
                    string classifierName = "RCTModel";
                    switch (_StudyTypeClassifier)
                    {
                        case "RCT":
                            classifierId = 0;
                            classifierName = "RCTModel";
                            break;
                        case "Cochrane RCT":
                            classifierId = -4;
                            classifierName = "NewRCTModel";
                            break;
                        case "Economic evaluation":
                            classifierId = -3;
                            classifierName = "NHSEEDModel";
                            break;
                        case "Systematic review":
                            classifierId = -2;
                            classifierName = "DAREModel";
                            break;
                    }
                    await ClassifierCommand.InvokeBatchExecutionService(ReviewId, "ScoreModel", classifierId, @"attributemodeldata/" + uploadedFileName,
                        @"attributemodels/" + classifierName + ".csv", resultsFile1, resultsFile2, cancellationToken);

                    if (cancellationToken.IsCancellationRequested) //InvokeBatchExecutionService "just" returns if cancelled, so we find out if cancellation happened in here.
                    {
                        MagLog.SaveLogEntry("Add classifier scores", "Cancelled", "Cancellation token, after InvokeBatchExecution. Review: " + ReviewId, ContactId);
                        return;
                    }

                    await insertResults(blobClient.GetContainerReference("attributemodels"),
                        TrainingRunCommand.NameBase + "Cont" + _MagAutoUpdateRunId.ToString() +
                        "Results1.csv", "StudyTypeClassifierScore", ReviewId, ContactId);
                    if (classifierId == -4)
                    {
                        await insertResults(blobClient.GetContainerReference("attributemodels"),
                            TrainingRunCommand.NameBase + "Cont" + _MagAutoUpdateRunId.ToString() +
                            "Results2.csv", "StudyTypeClassifierScore", ReviewId, ContactId);
                    }
                }

                if (_UserClassifierModelId > 0)
                {
                    await ClassifierCommand.InvokeBatchExecutionService(ReviewId, "ScoreModel", _UserClassifierModelId, @"attributemodeldata/" + uploadedFileName,
                        @"attributemodels/" + TrainingRunCommand.NameBase + "ReviewId" + _UserClassifierReviewId.ToString() + "ModelId" + _UserClassifierModelId.ToString() + ".csv", resultsFile1, resultsFile2, cancellationToken);

                    if (cancellationToken.IsCancellationRequested) //InvokeBatchExecutionService "just" returns if cancelled, so we find out if cancellation happened in here.
                    {
                        MagLog.SaveLogEntry("Add classifier scores", "Cancelled", "Cancellation token, after InvokeBatchExecution. Review: " + ReviewId, ContactId);
                        return;
                    }

                    await insertResults(blobClient.GetContainerReference("attributemodels"),
                        TrainingRunCommand.NameBase + "Cont" + _MagAutoUpdateRunId.ToString() +
                        "Results1.csv", "UserClassifierScore", ReviewId, ContactId);
                }
                MagLog.SaveLogEntry("Add classifier scores", "Finished", "Successful. Review: " + ReviewId, ContactId);

            }
            catch
            {
                MagLog.SaveLogEntry("Add classifier scores", "Failed", "At running classifier. Review: " + ReviewId, ContactId);
                return;
            }
        }

        private async Task insertResults(CloudBlobContainer container, string FileName, string Field, string ReviewId, int ContactId)
        {
            CloudBlockBlob blockBlobDataResults = container.GetBlockBlobReference(FileName);

            string Results1 = await blockBlobDataResults.DownloadTextAsync();
            byte[] myFile = Encoding.UTF8.GetBytes(Results1);

            DataTable dt = new DataTable("Ids");
            dt.Columns.Add("MAG_AUTO_UPDATE_RUN_ID");
            dt.Columns.Add("PaperId");
            dt.Columns.Add("SCORE");

            try
            {
                MemoryStream msIds = new MemoryStream(myFile);
                using (var readerIds = new StreamReader(msIds))
                {
                    string line;
                    while ((line = readerIds.ReadLine()) != null)
                    {
                        string[] fields = line.Split(',');

                        // this check for extreme values copied from ClassifierCommand
                        if (fields[0] == "1")
                        {
                            fields[0] = "0.999999";
                        }
                        else if (fields[0] == "0")
                        {
                            fields[0] = "0.000001";
                        }
                        else if (fields[0].Length > 2 && fields[0].Contains("E"))
                        {
                            double dbl = 0;
                            double.TryParse(fields[0], out dbl);
                            //if (dbl == 0.0) throw new Exception("Gotcha!");
                            fields[0] = dbl.ToString("F10");
                        }

                        DataRow newRow = dt.NewRow();
                        newRow["MAG_AUTO_UPDATE_RUN_ID"] = _MagAutoUpdateRunId;
                        newRow["PaperId"] = fields[1];
                        newRow["SCORE"] = fields[0];
                        dt.Rows.Add(newRow);
                    }
                }
            }
            catch
            {
                MagLog.SaveLogEntry("Add classifier scores", "Failed", "Downloading scores. Review: " + ReviewId, ContactId);
                return;
            }

            if (dt.Rows.Count == 0)
            {
                MagLog.SaveLogEntry("Add classifier scores", "Failed", "Rowcount = 0. Review: " + ReviewId, ContactId);
                return;
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
                {
                    connection.Open();
                    using (SqlBulkCopy sbc = new SqlBulkCopy(connection))
                    {
                        sbc.DestinationTableName = "TB_MAG_AUTO_UPDATE_CLASSIFIER_TEMP";
                        sbc.BatchSize = 1000;
                        sbc.WriteToServer(dt);
                    }

                    using (SqlCommand command = new SqlCommand("st_MagAutoUpdateClassifierScoresUpdate", connection))
                    {
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@MAG_AUTO_UPDATE_RUN_ID", _MagAutoUpdateRunId));
                        command.Parameters.Add(new SqlParameter("@Field", Field));
                        command.Parameters.Add(new SqlParameter("@StudyTypeClassifier", _StudyTypeClassifier));
                        command.Parameters.Add(new SqlParameter("@UserClassifierModelId", _UserClassifierModelId));
                        command.Parameters.Add(new SqlParameter("@UserClassifierReviewId", _UserClassifierReviewId));
                        command.ExecuteNonQuery();
                    }
                    connection.Close();
                }
            }
            catch
            {
                MagLog.SaveLogEntry("Add classifier scores", "Failed", "Inserting scores. Review: " + ReviewId, ContactId);
            }
        }
#endif


    }
}
