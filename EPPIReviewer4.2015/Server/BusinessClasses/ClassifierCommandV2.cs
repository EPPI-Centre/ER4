using System;
using Csla;


#if !SILVERLIGHT
using System.Data.SqlClient;
using BusinessLibrary.Data;
using BusinessLibrary.Security;
using System.Text.RegularExpressions;
using Microsoft.Azure.Management.DataFactory;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Rest;
using System.Data;

#if (!CSLA_NETCORE)
using Microsoft.VisualBasic.FileIO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
#else
using System.Net.Http.Json;
#endif


#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class ClassifierCommandV2 : LongLastingFireAndForgetCommand<ClassifierCommandV2>
    {

        public ClassifierCommandV2() { }
        // variables for training the classifier
        private string _title;
        private Int64 _attributeIdOn;
        private Int64 _attributeIdNotOn;
        private Int64 _attributeIdClassifyTo;
        private int _sourceId;

        // variables for applying the classifier
        private int _classifierId = -1;

        private string _returnMessage;

        public ClassifierCommandV2(string title, Int64 attributeIdOn, Int64 attributeIdNotOn, Int64 attributeIdClassifyTo, int classiferId, int sourceId)
        {
            _title = title;
            _attributeIdOn = attributeIdOn;
            _attributeIdNotOn = attributeIdNotOn;
            _returnMessage = "Success";
            _classifierId = classiferId;
            _attributeIdClassifyTo = attributeIdClassifyTo;
            _sourceId = sourceId;
        }

        public string ReturnMessage
        {
            get
            {
                return _returnMessage;
            }
        }

        public static readonly PropertyInfo<ReviewInfo> RevInfoProperty = RegisterProperty<ReviewInfo>(new PropertyInfo<ReviewInfo>("RevInfo", "RevInfo"));
        public ReviewInfo RevInfo
        {
            get { return ReadProperty(RevInfoProperty); }
            set { LoadProperty(RevInfoProperty, value); }
        }

        public static readonly PropertyInfo<string> ReportBackProperty = RegisterProperty<string>(new PropertyInfo<string>("ReportBack", "ReportBack"));
        public string ReportBack
        {
            get { return ReadProperty(ReportBackProperty); }
            set { LoadProperty(ReportBackProperty, value); }
        }

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_title", _title);
            info.AddValue("_attributeIdOn", _attributeIdOn);
            info.AddValue("_attributeIdNotOn", _attributeIdNotOn);
            info.AddValue("_returnMessage", _returnMessage);
            info.AddValue("_classifierId", _classifierId);
            info.AddValue("_attributeIdClassifyTo", _attributeIdClassifyTo);
            info.AddValue("_sourceId", _sourceId);
        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            _title = info.GetValue<string>("_title");
            _attributeIdOn = info.GetValue<Int64>("_attributeIdOn");
            _attributeIdNotOn = info.GetValue<Int64>("_attributeIdNotOn");
            _returnMessage = info.GetValue<string>("_returnMessage");
            _classifierId = info.GetValue<int>("_classifierId");
            _attributeIdClassifyTo = info.GetValue<Int64>("_attributeIdClassifyTo");
            _sourceId = info.GetValue<int>("_sourceId");
        }


#if !SILVERLIGHT

        protected override void DataPortal_Execute()
        {
            if (_title == "DeleteThisModel~~")
            {
                DeleteModel();
                return;
            }
            if (_title.Contains("¬¬CheckScreening"))
            {
                DoApplyCheckScreening();
                return;
            }
            if (_attributeIdOn + _attributeIdNotOn != -2)
            {
                DoTrainClassifier();
            }
            else
            {
                DoApplyClassifier(_classifierId);
            }
        }
        string LocalFileName = "";
        string VecFile = "";
        string ClfFile = "";
        string ScoresFile = "";
        private void DoTrainClassifier()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                int newModelId = 0;
                int NewJobId = 0;
                int ReviewId = ri.ReviewId;
                List<Int64> ItemIds = new List<Int64>();
                int positiveClassCount = 0;
                int negativeClasscount = 0;
                int sampleSize = 0;

                connection.Open();

                if (_classifierId == -1) // building a new classifier
                {
                    using (SqlCommand command = new SqlCommand("st_ClassifierSaveModel", connection))//Also checks if some classifier build job is already running
                    {//we do the check and job creation in a single SP as we need the operation to be "all or nothing"
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@REVIEW_ID", ReviewId));
                        command.Parameters.Add(new SqlParameter("@MODEL_TITLE", _title + " (in progress...)"));
                        command.Parameters.Add(new SqlParameter("@CONTACT_ID", ri.UserId));
                        command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ID_ON", _attributeIdOn));
                        command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ID_NOT_ON", _attributeIdNotOn));
                        command.Parameters.Add(new SqlParameter("@NEW_MODEL_ID", 0));
                        command.Parameters["@NEW_MODEL_ID"].Direction = System.Data.ParameterDirection.Output;
                        command.Parameters.Add(new SqlParameter("@NewJobId", System.Data.SqlDbType.Int));
                        command.Parameters["@NewJobId"].Direction = System.Data.ParameterDirection.Output;
                        command.ExecuteNonQuery();
                        newModelId = Convert.ToInt32(command.Parameters["@NEW_MODEL_ID"].Value);
                        if (newModelId == 0) // i.e. another train session is running / it's not been the specified length of time between running training yet
                        {
                            _returnMessage = "Already running";
                            return;
                        }
                        else
                        {
                            _returnMessage = "Starting...";
                            NewJobId = (int)command.Parameters["@NewJobId"].Value;
                        }
                    }
                }
                else
                {
                    _returnMessage = "";
                    newModelId = _classifierId; // we're rebuilding an existing classifier

                    using (SqlCommand command2 = new SqlCommand("st_ClassifierCanRunCheckAndMarkAsStarting", connection))
                    {//this SP also checks if a build (or Apply) job is running and creates the new job record if not
                        command2.CommandType = System.Data.CommandType.StoredProcedure;

                        command2.Parameters.Add(new SqlParameter("@MODEL_ID", _classifierId));
                        command2.Parameters.Add(new SqlParameter("@REVIEW_ID", ReviewId));
                        command2.Parameters.Add(new SqlParameter("@REVIEW_ID_OF_MODEL", ReviewId));
                        command2.Parameters.Add(new SqlParameter("@CONTACT_ID", ri.UserId));
                        command2.Parameters.Add(new SqlParameter("@TITLE", _title.Contains(" (rebuilding...)") ? _title : _title + " (rebuilding...)"));
                        command2.Parameters.Add(new SqlParameter("@IsApply", false));
                        command2.Parameters.Add(new SqlParameter("@NewJobId", 0));
                        command2.Parameters["@NewJobId"].Direction = System.Data.ParameterDirection.Output;
                        command2.ExecuteNonQuery();
                        NewJobId = Convert.ToInt32(command2.Parameters["@NewJobId"].Value);
                        if (NewJobId == 0)
                        {
                            _returnMessage = "Already running";
                            return;
                        }
                        else
                        {
                            _returnMessage = "Starting...";
                        }
                    }
                }
#if (!CSLA_NETCORE)
				LocalFileName = System.Web.HttpRuntime.AppDomainAppPath + TempPath 
                    + "ReviewID" + ri.ReviewId + "ContactId" + ri.UserId.ToString() + ".tsv";
#else
                DirectoryInfo tmpDir = System.IO.Directory.CreateDirectory("UserTempUploads");
                LocalFileName = tmpDir.FullName + "\\ReviewID" + ri.ReviewId + "ContactId" + ri.UserId.ToString() + ".tsv";
#endif
                using (SqlCommand command = new SqlCommand("st_ClassifierGetTrainingData", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ID_ON", _attributeIdOn));
                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ID_NOT_ON", _attributeIdNotOn));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        using (System.IO.StreamWriter file = new System.IO.StreamWriter(LocalFileName, false))
                        {
                            file.WriteLine("PaperId\tPaperTitle\tAbstract\tIncl");
                            while (reader.Read())
                            {
                                if (ItemIds.IndexOf(reader.GetInt64("ITEM_ID")) == -1)
                                {
                                    sampleSize++;
                                    ItemIds.Add(reader.GetInt64("ITEM_ID"));
                                    file.WriteLine(reader["item_id"].ToString() + "\t" +
                                        CleanText(reader, "title") + "\t" +
                                        CleanText(reader, "abstract") + "\t" +
                                        CleanText(reader, "LABEL"));
                                    if (reader["LABEL"].ToString() == "1")
                                        positiveClassCount++;
                                    else
                                        negativeClasscount++;
                                }
                            }
                        }
                    }
                }
                if (positiveClassCount < 7 || negativeClasscount < 7 || sampleSize < 20)//at least 7 examples in each class and at least 20 records in total
                {
                    _returnMessage = "Insufficient data";
                    if (_classifierId == -1) //building a new classifier, there is not enough data, so we're not saving it
                    {
                        using (SqlCommand command = new SqlCommand("st_ClassifierDeleteModel", connection))
                        {
                            command.CommandType = System.Data.CommandType.StoredProcedure;
                            command.Parameters.Add(new SqlParameter("@REVIEW_ID", ReviewId));
                            command.Parameters.Add(new SqlParameter("@MODEL_ID", newModelId));
                            command.ExecuteNonQuery();
                        }
                    }
                    else
                    {//update: we were rebuilding it, re-set the model name
                        using (SqlCommand command = new SqlCommand("st_ClassifierUpdateModelTitle", connection))
                        {
                            command.CommandType = System.Data.CommandType.StoredProcedure;
                            command.Parameters.Add(new SqlParameter("@MODEL_ID", newModelId));
                            command.Parameters.Add(new SqlParameter("@TITLE", _title));
                            command.ExecuteNonQuery();
                        }
                    }
                    File.Delete(LocalFileName);
                    DataFactoryHelper.UpdateReviewJobLog(NewJobId, ReviewId, "Ended", _returnMessage, "ClassifierCommandV2", true, false);
                    return;
                }
                connection.Close();
                Task.Run(() => UploadDataAndBuildModelAsync(ReviewId, NewJobId, newModelId));


                //if (applyToo == true)
                //{
                //    DoApplyClassifier(ModelId);
                //}
            }
        }

        private async void UploadDataAndBuildModelAsync(int ReviewId, int LogId, int modelId)
        {
            if (AppIsShuttingDown)
            {
                DataFactoryHelper.UpdateReviewJobLog(LogId, ReviewId, "Cancelled before upload", "", "TrainingRunCommandV2", true, false);
                return;
            }

            string FolderAndFileName = DataFactoryHelper.NameBase + "ReviewId" + ReviewId.ToString() + "ModelId" + modelId;
            string RemoteFolder = "user_models/" + FolderAndFileName + "/";
            string RemoteFileName = RemoteFolder + FolderAndFileName + "DataForBuilding.tsv";

            bool DataFactoryRes = false;
            try
            {
                DataFactoryHelper.UpdateReviewJobLog(LogId, ReviewId, "Uploading", "", "TrainingRunCommandV2");
                using (var fileStream = System.IO.File.OpenRead(LocalFileName))
                {
                    BlobOperations.UploadStream(blobConnection, "eppi-reviewer-data", RemoteFileName, fileStream);
                }
                File.Delete(LocalFileName);
            }
            catch (Exception ex)
            {
                DataFactoryHelper.LogExceptionToFile(ex, ReviewId, LogId, "ClassifierCommandV2");
                if (File.Exists(LocalFileName))
                {
                    try
                    {
                        File.Delete(LocalFileName);
                    }
                    catch { }
                }
                BlobOperations.DeleteIfExists(blobConnection, "eppi-reviewer-data", RemoteFileName);
                DataFactoryHelper.UpdateReviewJobLog(LogId, ReviewId, "Failed to upload data", "", "ClassifierCommandV2", true, false);
                return;
            }
            if (AppIsShuttingDown)
            {
                DataFactoryHelper.UpdateReviewJobLog(LogId, ReviewId, "Cancelled after upload", "", "TrainingRunCommandV2", true, false);
                return;
            }
            try
            {
                DataFactoryHelper DFH = new DataFactoryHelper();
                string BatchGuid = Guid.NewGuid().ToString();
                VecFile = RemoteFolder + "Vectors.pkl";
                ClfFile = RemoteFolder + "Clf.pkl";
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    {"do_build_and_score_log_reg", false },
                    {"DataFile", RemoteFileName },
                    {"EPPIReviewerApiRunId", BatchGuid},
                    {"do_build_log_reg", true},
                    {"do_score_log_reg", false},
                    //{"ScoresFile", ScoresFile},
                    {"VecFile", VecFile},
                    {"ClfFile", ClfFile}
                };
                DataFactoryRes = await DFH.RunDataFactoryProcessV2("EPPI-Reviewer_API", parameters, ReviewId, LogId, "TrainingRunCommandV2", this.CancelToken);
            }
            catch (Exception ex)
            {
                DataFactoryHelper.LogExceptionToFile(ex, ReviewId, LogId, "ClassifierCommandV2");
                //BlobOperations.DeleteIfExists(blobConnection, "eppi-reviewer-data", RemoteFileName);
                DataFactoryHelper.UpdateReviewJobLog(LogId, ReviewId, "Failed to (re)build classifier", "", "ClassifierCommandV2", true, false);
                return;
            }
            //no check for AppIsShuttingDown: these happen inside RunDataFactoryProcessV2
            //what happens after this is fast, so no need for checking from here on
            if (DataFactoryRes == true)
            {
                try
                {
                    double accuracy = 0;
                    double precision = 0;
                    double recall = 0;
                    double auc = 0;
                    MemoryStream ms = BlobOperations.DownloadBlobAsMemoryStream(blobConnection, "eppi-reviewer-data", RemoteFolder + "stats.tsv");
                    using (StreamReader tsvReader = new StreamReader(ms))
                    {
                        //csvReader.SetDelimiters(new string[] { "," });
                        //csvReader.HasFieldsEnclosedInQuotes = false;
                        string line = tsvReader.ReadLine();//headers line!!
                        line = tsvReader.ReadLine();//data line!!
                        if (line != null)
                        {
                            string[] data = line.Split('\t');
                            accuracy = GetSafeValue(data[0]);
                            precision = GetSafeValue(data[1]);
                            recall = GetSafeValue(data[2]);
                            auc = GetSafeValue(data[3]);
                        }
                    }
                    using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
                    {
                        connection.Open();
                        using (SqlCommand command2 = new SqlCommand("st_ClassifierUpdateModel", connection))
                        {
                            command2.CommandType = System.Data.CommandType.StoredProcedure;

                            command2.Parameters.Add(new SqlParameter("@MODEL_ID", modelId));
                            command2.Parameters.Add(new SqlParameter("@TITLE", _title));
                            command2.Parameters.AddWithValue("@ACCURACY", accuracy);
                            command2.Parameters.AddWithValue("@AUC", auc);
                            command2.Parameters.AddWithValue("@PRECISION", precision);
                            command2.Parameters.AddWithValue("@RECALL", recall);
                            command2.Parameters.Add(new SqlParameter("@CHECK_MODEL_ID_EXISTS", 0));
                            command2.Parameters["@CHECK_MODEL_ID_EXISTS"].Direction = System.Data.ParameterDirection.Output;
                            command2.ExecuteNonQuery();
                            //if (Convert.ToInt32(command2.Parameters["@CHECK_MODEL_ID_EXISTS"].Value) == 0)
                            //{
                            //	DeleteModelAsync();
                            //}
                        }
                        connection.Close();
                    }

                    DataFactoryHelper.UpdateReviewJobLog(LogId, ReviewId, "Ended", "", "ClassifierCommandV2", true, true);
                }
                catch (Exception ex)
                {
                    DataFactoryHelper.LogExceptionToFile(ex, ReviewId, LogId, "ClassifierCommandV2");
                    DataFactoryHelper.UpdateReviewJobLog(LogId, ReviewId, "Failed to download data", "", "ClassifierCommandV2", true, false);
                }
            }
            //BlobOperations.DeleteIfExists(blobConnection, "eppi-reviewer-data", RemoteFileName);
        }

        private void DoApplyClassifier(int modelId)
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            int ReviewId = ri.ReviewId;
            int ContactId = ri.UserId;
            int NewJobId = 0;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
#if (!CSLA_NETCORE)

                LocalFileName = System.Web.HttpRuntime.AppDomainAppPath + TempPath + "ReviewID" + ri.ReviewId + "ContactId" + ri.UserId.ToString() + ".csv";
#else
                // same as comment above for same line
                //SG Edit:
                DirectoryInfo tmpDir = System.IO.Directory.CreateDirectory("UserTempUploads");
                LocalFileName = tmpDir.FullName + "\\ReviewID" + ReviewId + "ContactId" + ri.UserId.ToString() + ".tsv";
#endif
                //[SG]: new 27/09/2021: find out the reviewId for this model, as it might be from a different review
                //added bonus, ensures the current user has access to this model, I guess.
                int ModelReviewId = -1; //will be used later
                if (modelId > 0) //no need to check for the general pre-built models which are less than zero...
                {
                    try
                    {
                        using (SqlCommand command = new SqlCommand("st_ClassifierContactModels", connection))
                        {
                            command.CommandType = System.Data.CommandType.StoredProcedure;
                            command.Parameters.Add(new SqlParameter("@CONTACT_ID", ri.UserId));
                            using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                            {
                                while (reader.Read())
                                {
                                    int tempModelid = reader.GetInt32("MODEL_ID");
                                    if (tempModelid == modelId)
                                    {
                                        //we found it, we can stop after getting the actual ReviewId where this model was built: we need it for the filename of the model in the blob
                                        ModelReviewId = reader.GetInt32("REVIEW_ID");
                                        break;
                                    }
                                }
                            }
                            command.Cancel();
                        }
                    }
                    catch (Exception ex)
                    {
                        _returnMessage = "Error at ClassifierContactModels:" + ex.Message;
                        return;
                    }
                    if (ModelReviewId == -1)
                    {
                        _returnMessage = "Error, Model not found";
                        //the query above didn't find the current model, so we can't/should not continue...
                        return;
                    }
                }
                //end of 27/09/2021 addition


                using (SqlCommand command2 = new SqlCommand("st_ClassifierCanRunCheckAndMarkAsStarting", connection))
                {//this SP also checks if a Apply (or build) job is running and creates the new job record if not
                    command2.CommandType = System.Data.CommandType.StoredProcedure;

                    command2.Parameters.Add(new SqlParameter("@MODEL_ID", _classifierId));
                    command2.Parameters.Add(new SqlParameter("@REVIEW_ID", ReviewId));
                    command2.Parameters.Add(new SqlParameter("@REVIEW_ID_OF_MODEL", ModelReviewId));
                    command2.Parameters.Add(new SqlParameter("@CONTACT_ID", ri.UserId));
                    command2.Parameters.Add(new SqlParameter("@IsApply", true));
                    command2.Parameters.Add(new SqlParameter("@NewJobId", 0));
                    command2.Parameters["@NewJobId"].Direction = System.Data.ParameterDirection.Output;
                    command2.ExecuteNonQuery();
                    NewJobId = Convert.ToInt32(command2.Parameters["@NewJobId"].Value);
                    if (NewJobId == 0)
                    {
                        _returnMessage = "Already running";
                        return;
                    }
                    else
                    {
                        _returnMessage = "Starting...";
                    }
                }

                if (modelId == -5 || modelId == -6 || modelId == -7 || modelId == -8 || modelId == -9)
                {// the covid19,  progress-plus using the BERT model, pubmed study types, pubmed study designs (public), via AzureSQL database.
                    Task.Run(() => ApplyPreBuiltClassifiersAsync(modelId, _attributeIdClassifyTo, ReviewId, ri.UserId, NewJobId));
                    _returnMessage = "The data will be submitted and scored. Please monitor the list of search results for output.";
                    return;
                }
                else if (modelId == -4 || modelId == -3 || modelId == -2 || modelId == -1)
                {//older pre-built classifiers RCT (-1), Cochrane RCT(-4), Economic Evaluation (-3), Systematic Review (-2), via AzureSQL database.
                    Task.Run(() => ApplyPreBuiltClassifiersAsync(modelId, _attributeIdClassifyTo, ReviewId, ri.UserId, NewJobId));
                    _returnMessage = "The data will be submitted and scored. Please monitor the list of search results for output.";
                    return;
                }
                else
                {//has to be a positive model ID, so a custom built one
                    Task.Run(() => UploadDataAndScoreCustomModelAsync(ReviewId, NewJobId, modelId, ContactId, ModelReviewId));
                    _returnMessage = "The data will be submitted and scored. Please monitor the list of search results for output.";
                    return;
                }
            } // end if check for using covid categories / BERT models / SQL database
        }
        private async void UploadDataAndScoreCustomModelAsync(int ReviewId, int LogId, int modelId, int ContactId, int ModelReviewId)
        {
            List<Int64> ItemIds = new List<Int64>();
            try
            {
                using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand("st_ClassifierGetClassificationData", connection))// also deletes data from the classification temp table
                    {
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@REVIEW_ID", ReviewId));
                        command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ID_CLASSIFY_TO", _attributeIdClassifyTo));
                        command.Parameters.Add(new SqlParameter("@SOURCE_ID", _sourceId));
                        using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                        {
                            using (System.IO.StreamWriter file = new System.IO.StreamWriter(LocalFileName, false))
                            {
                                file.WriteLine("PaperId\tPaperTitle\tAbstract\tIncl");
                                while (reader.Read())
                                {
                                    if (ItemIds.IndexOf(reader.GetInt64("ITEM_ID")) == -1)
                                    {
                                        ItemIds.Add(reader.GetInt64("ITEM_ID"));
                                        file.WriteLine(reader["item_id"].ToString() + "\t" +
                                            CleanText(reader, "title") + "\t" +
                                            CleanText(reader, "abstract") + "\t" +
                                            "99");
                                        //file.WriteLine("\"" + reader["item_id"].ToString() + "\"," +
                                        //	"\"" + reader["LABEL"].ToString() + "\"," +
                                        //	"\"" + CleanText(reader, "title") + "\"," +
                                        //	"\"" + CleanText(reader, "abstract") + "\"," +
                                        //	"\"" + CleanText(reader, "keywords") + "\"," + "\"" + RevInfo.ReviewId.ToString() + "\"");
                                    }
                                }
                            }
                            command.Cancel();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DataFactoryHelper.UpdateReviewJobLog(LogId, ReviewId, "Failed to get data to score", "", "ClassifierCommandV2", true, false);
                DataFactoryHelper.LogExceptionToFile(ex, ReviewId, LogId, "ClassifierCommandV2");
                return;
            }
            if (AppIsShuttingDown)
            {
                DataFactoryHelper.UpdateReviewJobLog(LogId, ReviewId, "Cancelled before upload", "", "ClassifierCommandV2", true, false);
                return;
            }

            string FolderAndFileName = DataFactoryHelper.NameBase + "ReviewId" + ModelReviewId.ToString() + "ModelId" + modelId;
            string RemoteFolder = "user_models/" + FolderAndFileName + "/";
            string RemoteFileName = RemoteFolder + FolderAndFileName + "DataForScoring.tsv";
            bool DataFactoryRes = false;
            // upload data to blob
            try
            {
                using (var fileStream = System.IO.File.OpenRead(LocalFileName))
                {
                    BlobOperations.UploadStream(blobConnection,
                        "eppi-reviewer-data"
                        , RemoteFileName
                        , fileStream);
                }
                File.Delete(LocalFileName);
            }
            catch (Exception ex)
            {
                DataFactoryHelper.UpdateReviewJobLog(LogId, ReviewId, "Failed to uplodad data to score", "", "ClassifierCommandV2", true, false);
                DataFactoryHelper.LogExceptionToFile(ex, ReviewId, LogId, "ClassifierCommandV2");
                return;
            }


            DataFactoryHelper DFH = new DataFactoryHelper();
            string BatchGuid = Guid.NewGuid().ToString();
            VecFile = RemoteFolder + "Vectors.pkl";
            ClfFile = RemoteFolder + "Clf.pkl";
            ScoresFile = RemoteFolder + "ScoresForReview" + ReviewId + ".tsv";
            Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    {"do_build_and_score_log_reg", false },
                    {"DataFile", RemoteFileName },
                    {"EPPIReviewerApiRunId", BatchGuid},
                    {"do_build_log_reg", false},
                    {"do_score_log_reg", true},
                    {"ScoresFile", ScoresFile},
                    {"VecFile", VecFile},
                    {"ClfFile", ClfFile}
                };

            if (AppIsShuttingDown)
            {
                DataFactoryHelper.UpdateReviewJobLog(LogId, ReviewId, "Cancelled after upload", "", "ClassifierCommandV2", true, false);
                return;
            }
            try
            {
                DataFactoryRes = await DFH.RunDataFactoryProcessV2("EPPI-Reviewer_API", parameters, ReviewId, LogId, "ClassifierCommandV2", this.CancelToken);

            }
            catch (Exception ex)
            {
                DataFactoryHelper.UpdateReviewJobLog(LogId, ReviewId, "Failed to run DF", "", "ClassifierCommandV2", true, false);
                DataFactoryHelper.LogExceptionToFile(ex, ReviewId, LogId, "ClassifierCommandV2");
                return;
            }
            DataTable Scores = new DataTable();
            if (DataFactoryRes == true)
            {
                try
                {

                    Scores = DownloadResults("eppi-reviewer-data", ScoresFile);
                    if (AppIsShuttingDown)
                    {
                        DataFactoryHelper.UpdateReviewJobLog(LogId, ReviewId, "Cancelled after DF", "", "ClassifierCommandV2", true, false);
                        return;
                    }
                    LoadResultsIntoDatabase(Scores, ContactId);
                }
                catch (Exception ex)
                {
                    DataFactoryHelper.UpdateReviewJobLog(LogId, ReviewId, "Failed after DF", "", "ClassifierCommandV2", true, false);
                    DataFactoryHelper.LogExceptionToFile(ex, ReviewId, LogId, "ClassifierCommandV2");
                    return;
                }
            }
            try
            {
                BlobOperations.DeleteIfExists(blobConnection, "eppi-reviewer-data", RemoteFileName);
                BlobOperations.DeleteIfExists(blobConnection, "eppi-reviewer-data", ScoresFile);
                if (DataFactoryRes == true) DataFactoryHelper.UpdateReviewJobLog(LogId, ReviewId, "Ended", "", "ClassifierCommandV2", true, true);
            }
            catch (Exception ex)
            {
                DataFactoryHelper.UpdateReviewJobLog(LogId, ReviewId, "Failed deleting remote files", "", "ClassifierCommandV2", true, false);
                DataFactoryHelper.LogExceptionToFile(ex, ReviewId, LogId, "ClassifierCommandV2");
                return;
            }


            //string DataFile = @"attributemodeldata/" + TrainingRunCommand.NameBase + "ReviewId" + RevInfo.ReviewId.ToString() + "ModelId" + ModelIdForScoring(modelId) + "ToScore.csv";
            //string ModelFile = @"attributemodels/" + (modelId > 0 ? TrainingRunCommand.NameBase : "") + ReviewIdForScoring(modelId, ModelReviewId.ToString()) + ".csv";
            //string ResultsFile1 = @"attributemodels/" + TrainingRunCommand.NameBase + "ReviewId" + RevInfo.ReviewId.ToString() + "ModelId" + ModelIdForScoring(modelId) + "Scores.csv";
            //string ResultsFile2 = "";
            //if (modelId == -4)
            //{
            //    ResultsFile1 = "attributemodels/" + TrainingRunCommand.NameBase + "ReviewId" + RevInfo.ReviewId.ToString() + "ModelId" + ModelIdForScoring(modelId) + "RCTScores.csv";
            //    ResultsFile2 = @"attributemodels/" + TrainingRunCommand.NameBase + "ReviewId" + RevInfo.ReviewId.ToString() + "ModelId" + ModelIdForScoring(modelId) + "NonRCTScores.csv";
            //}
            //await InvokeBatchExecutionService(RevInfo.ReviewId.ToString(), "ScoreModel", modelId, DataFile, ModelFile, ResultsFile1, ResultsFile2);

            //if (modelId == -4) // new RCT model = two searches to create, one for the RCTs, one for the non-RCTs
            //{
            //    // load RCTs
            //    DataTable RCTs = DownloadResults( "attributemodels", TrainingRunCommand.NameBase + "ReviewId" + RevInfo.ReviewId.ToString() + "ModelId" + ModelIdForScoring(modelId) + "RCTScores.csv");
            //    _title = "Cochrane RCT Classifier: may be RCTs";
            //    LoadResultsIntoDatabase(RCTs, connection, ri);

            //    // load non-RCTs
            //    DataTable nRCTs = DownloadResults( "attributemodels", TrainingRunCommand.NameBase + "ReviewId" + RevInfo.ReviewId.ToString() + "ModelId" + ModelIdForScoring(modelId) + "NonRCTScores.csv");
            //    _title = "Cochrane RCT Classifier: unlikely to be RCTs";
            //    LoadResultsIntoDatabase(nRCTs, connection, ri);
            //}
            //else
            //{
            //    DataTable Scores = DownloadResults( "attributemodels", TrainingRunCommand.NameBase + "ReviewId" + RevInfo.ReviewId.ToString() + "ModelId" + ModelIdForScoring(modelId) + "Scores.csv");
            //    LoadResultsIntoDatabase(Scores, connection, ri);
            //}
            //connection.Close();
        }

        private void DoApplyCheckScreening()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                int NewJobId = 0;
                int ReviewId = ri.ReviewId;
                List<Int64> ItemIds = new List<Int64>();
                int positiveClassCount = 0;
                int negativeClasscount = 0;
                int sampleSize = 0;

                Guid thisGuid = Guid.NewGuid();

                connection.Open();

                // ************************************ SOMWHERE HERE I NEED TO GET A NewJobId ***************************************
                // ************************************ Can't see a generic "get jobid" stored proc? *********************************

                DirectoryInfo tmpDir = System.IO.Directory.CreateDirectory("UserTempUploads");
                LocalFileName = tmpDir.FullName + "\\ReviewID" + ri.ReviewId + "ContactId" + ri.UserId.ToString() + "_" + thisGuid.ToString() + ".tsv";

                using (SqlCommand command = new SqlCommand("st_ClassifierGetTrainingData", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ID_ON", _attributeIdOn));
                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ID_NOT_ON", _attributeIdNotOn));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        using (System.IO.StreamWriter file = new System.IO.StreamWriter(LocalFileName, false))
                        {
                            file.WriteLine("PaperId\tPaperTitle\tAbstract\tIncl");
                            while (reader.Read())
                            {
                                if (ItemIds.IndexOf(reader.GetInt64("ITEM_ID")) == -1)
                                {
                                    sampleSize++;
                                    ItemIds.Add(reader.GetInt64("ITEM_ID"));
                                    file.WriteLine(reader["item_id"].ToString() + "\t" +
                                        CleanText(reader, "title") + "\t" +
                                        CleanText(reader, "abstract") + "\t" +
                                        CleanText(reader, "LABEL"));
                                    if (reader["LABEL"].ToString() == "1")
                                        positiveClassCount++;
                                    else
                                        negativeClasscount++;
                                }
                            }
                        }
                    }
                }
                if (positiveClassCount < 7 || negativeClasscount < 7 || sampleSize < 20)//at least 7 examples in each class and at least 20 records in total
                {
                    _returnMessage = "Insufficient data";
                    File.Delete(LocalFileName);
                    // **************************************** NEED TO UPDATE THE JOB ONCE i HAVE A JOB ID ****************************
                    // ********************************************************************************************************************

                    //DataFactoryHelper.UpdateReviewJobLog(NewJobId, ReviewId, "Ended", _returnMessage, "ClassifierCommandV2", true, false);
                    return;
                }
                connection.Close();
                Task.Run(() => UploadDataAndCheckScreeningAsync(ReviewId, ri.UserId, thisGuid, NewJobId));
            }
        }

        private async void UploadDataAndCheckScreeningAsync(int ReviewId, int ContactId, Guid thisGuid, int LogId)
        {
            if (AppIsShuttingDown)
            {
                // ********************************************** NEED TO DO THIS ONCE i HAVE A LOG ID *************************

                // DataFactoryHelper.UpdateReviewJobLog(LogId, ReviewId, "Cancelled before upload", "", "TrainingRunCommandV2", true, false);
                return;
            }
            string FolderAndFileName = DataFactoryHelper.NameBase + "ReviewId" + ReviewId.ToString() + "ContactId" + ContactId.ToString() + "_" + thisGuid.ToString();
            string RemoteFolder = "check_screening/" + FolderAndFileName + "/";
            string RemoteFileName = RemoteFolder + "ScreeningCheckData.tsv";
            ScoresFile = RemoteFolder + "ScreeningCheckScores" + ".tsv";

            bool DataFactoryRes = false;
            try
            {
                // ************************************************** NEED LOG ID *********************************************
                // DataFactoryHelper.UpdateReviewJobLog(LogId, ReviewId, "Uploading", "", "TrainingRunCommandV2");

                using (var fileStream = System.IO.File.OpenRead(LocalFileName))
                {
                    BlobOperations.UploadStream(blobConnection, "eppi-reviewer-data", RemoteFileName, fileStream);
                }
                File.Delete(LocalFileName);
            }
            catch (Exception ex)
            {
                // ****************************************** NB LODID **********************************************
                // DataFactoryHelper.LogExceptionToFile(ex, ReviewId, LogId, "ClassifierCommandV2");
                if (File.Exists(LocalFileName))
                {
                    try
                    {
                        File.Delete(LocalFileName);
                    }
                    catch { }
                }
                BlobOperations.DeleteIfExists(blobConnection, "eppi-reviewer-data", RemoteFileName);
                // ******************************************************************* MORE FOR LOGID ***********************************
                // DataFactoryHelper.UpdateReviewJobLog(LogId, ReviewId, "Failed to upload data", "", "ClassifierCommandV2", true, false);
                return;
            }
            if (AppIsShuttingDown)
            {
                // ************************************************* LOG ID **********************************************************
                //DataFactoryHelper.UpdateReviewJobLog(LogId, ReviewId, "Cancelled after upload", "", "TrainingRunCommandV2", true, false);
                return;
            }
            try
            {
                DataFactoryHelper DFH = new DataFactoryHelper();
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    {"do_check_screening", true },
                    {"DataFile", RemoteFileName },
                    {"EPPIReviewerApiRunId", thisGuid.ToString()},
                    {"ScoresFile", ScoresFile},
                };
                // ******************************************** LOG ID N.B. IT'S ZERO AT THE MOMENT!  **************************************************
                DataFactoryRes = await DFH.RunDataFactoryProcessV2("EPPI-Reviewer_API", parameters, ReviewId, LogId, "TrainingRunCommandV2", this.CancelToken);
            }
            catch (Exception ex)
            {
                // ******************************************** LOG ID ********************************************************
                //DataFactoryHelper.LogExceptionToFile(ex, ReviewId, LogId, "ClassifierCommandV2");
                //BlobOperations.DeleteIfExists(blobConnection, "eppi-reviewer-data", RemoteFileName); -- this was already commented out?
                // ******************************************** log id **********************************************************
                //DataFactoryHelper.UpdateReviewJobLog(LogId, ReviewId, "Failed to (re)build classifier", "", "ClassifierCommandV2", true, false);
                return;
            }
            //no check for AppIsShuttingDown: these happen inside RunDataFactoryProcessV2
            //what happens after this is fast, so no need for checking from here on

            DataTable Scores = new DataTable();
            if (DataFactoryRes == true)
            {
                try
                {

                    Scores = DownloadResults("eppi-reviewer-data", ScoresFile);
                    if (AppIsShuttingDown)
                    {
                        // ************************************** log id **********************************************************
                        DataFactoryHelper.UpdateReviewJobLog(LogId, ReviewId, "Cancelled after DF", "", "ClassifierCommandV2", true, false);
                        return;
                    }
                    LoadResultsIntoDatabase(Scores, ContactId);
                }
                catch (Exception ex)
                {
                    // ****************************************************** log id **************************************************
                    DataFactoryHelper.UpdateReviewJobLog(LogId, ReviewId, "Failed after DF", "", "ClassifierCommandV2", true, false);
                    DataFactoryHelper.LogExceptionToFile(ex, ReviewId, LogId, "ClassifierCommandV2");
                    return;
                }
            }
            try
            {
                BlobOperations.DeleteIfExists(blobConnection, "eppi-reviewer-data", RemoteFileName);
                BlobOperations.DeleteIfExists(blobConnection, "eppi-reviewer-data", ScoresFile);
                if (DataFactoryRes == true) DataFactoryHelper.UpdateReviewJobLog(LogId, ReviewId, "Ended", "", "ClassifierCommandV2", true, true);
            }
            catch (Exception ex)
            {
                DataFactoryHelper.UpdateReviewJobLog(LogId, ReviewId, "Failed deleting remote files", "", "ClassifierCommandV2", true, false);
                DataFactoryHelper.LogExceptionToFile(ex, ReviewId, LogId, "ClassifierCommandV2");
                return;
            }
        }

        /// <summary>
        /// Uses AzureSQL instance to ship data to and from the ML workspace
        /// </summary>
        /// <param name="modelId"></param>
        /// <param name="ApplyToAttributeId"></param>
        /// <param name="ReviewId"></param>
        /// <param name="ContactId"></param>
        /// <param name="LogId"></param>
        /// <returns></returns>
        private async Task ApplyPreBuiltClassifiersAsync(int modelId, Int64 ApplyToAttributeId, int ReviewId, int ContactId, int LogId)
        {
            string BatchGuid = Guid.NewGuid().ToString();
            int rowcount = 0;
            try
            {
                using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand("st_ClassifierGetClassificationDataToSQL", connection))
                    {
                        command.CommandTimeout = 6000; // 10 minutes - if there are tens of thousands of items it can take a while
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@REVIEW_ID", ReviewId));
                        command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ID_CLASSIFY_TO", ApplyToAttributeId));
                        command.Parameters.Add(new SqlParameter("@ITEM_ID_LIST", ""));
                        command.Parameters.Add(new SqlParameter("@SOURCE_ID", _sourceId));
                        command.Parameters.Add(new SqlParameter("@BatchGuid", BatchGuid));
                        command.Parameters.Add(new SqlParameter("@ContactId", ContactId));
                        command.Parameters.Add(new SqlParameter("@MachineName", TrainingRunCommand.NameBase));
                        command.Parameters.Add(new SqlParameter("@ROWCOUNT", 0));
                        command.Parameters["@ROWCOUNT"].Direction = System.Data.ParameterDirection.Output;
                        command.ExecuteNonQuery();
                        rowcount = Convert.ToInt32(command.Parameters["@ROWCOUNT"].Value);
                    }
                }
                if (rowcount == 0)
                {
                    DataFactoryHelper.UpdateReviewJobLog(LogId, ReviewId, "Failed: no data to score", "", "ClassifierCommandV2", true, false);
                    return;
                }
            }
            catch (Exception ex)
            {
                DataFactoryHelper.UpdateReviewJobLog(LogId, ReviewId, "Failed to get data to score", "", "ClassifierCommandV2", true, false);
                DataFactoryHelper.LogExceptionToFile(ex, ReviewId, LogId, "ClassifierCommandV2");
                return;
            }
            if (AppIsShuttingDown)
            {
                DataFactoryHelper.UpdateReviewJobLog(LogId, ReviewId, "Cancelled after uploading data", "", "ClassifierCommandV2", true, false);
                return;
            }
            string tenantID = AzureSettings.tenantID;
            string appClientId = AzureSettings.appClientId;
            string appClientSecret = AzureSettings.appClientSecret;
            string subscriptionId = AzureSettings.subscriptionId;
            string resourceGroup = AzureSettings.resourceGroup;
            string dataFactoryName = AzureSettings.dataFactoryName;

            string covidClassifierPipelineName = AzureSettings.covidClassifierPipelineName;
            string covidLongCovidPipelineName = AzureSettings.covidLongCovidPipelineName;
            string progressPlusPipelineName = AzureSettings.progressPlusPipelineName;
            string pubMedStudyTypesPipelineName = AzureSettings.pubMedStudyTypesPipelineName;
            string pubMedStudyDesignsPipelineName = AzureSettings.pubMedStudyDesignsPipelineName;

            string ClassifierPipelineName = "";
            string SearchTitle = "";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            switch (modelId)
            {
                case -1:
                    ClassifierPipelineName = "EPPI-Reviewer_API";
                    SearchTitle = "Items classified according to model: " + _title;
                    parameters.Add("EPPIReviewerApiRunId", BatchGuid);
                    parameters.Add("do_original_rct_classifier", true);
                    break;
                case -2:
                    ClassifierPipelineName = "EPPI-Reviewer_API";
                    SearchTitle = "Items classified according to model: " + _title;
                    parameters.Add("EPPIReviewerApiRunId", BatchGuid);
                    parameters.Add("do_systematic_reviews_classifier", true);
                    break;
                case -3:
                    ClassifierPipelineName = "EPPI-Reviewer_API";
                    SearchTitle = "Items classified according to model: " + _title;
                    parameters.Add("EPPIReviewerApiRunId", BatchGuid);
                    parameters.Add("do_economic_eval_classifier", true);
                    break;
                case -4:
                    ClassifierPipelineName = "EPPI-Reviewer_API";
                    SearchTitle = "Items classified according to model: " + _title;
                    parameters.Add("EPPIReviewerApiRunId", BatchGuid);
                    parameters.Add("do_cochrane_rct_classifier", true);
                    break;
                case -5:
                    ClassifierPipelineName = covidClassifierPipelineName;
                    SearchTitle = "COVID-19 map category: ";
                    parameters.Add("BatchGuid", BatchGuid);
                    break;
                case -6:
                    ClassifierPipelineName = covidLongCovidPipelineName;
                    SearchTitle = "Long COVID model: ";
                    parameters.Add("BatchGuid", BatchGuid);
                    break;
                case -7:
                    ClassifierPipelineName = progressPlusPipelineName;
                    SearchTitle = "PROGRESS-Plus model: ";
                    parameters.Add("BatchGuid", BatchGuid);
                    break;
                case -8:
                    ClassifierPipelineName = pubMedStudyTypesPipelineName;
                    SearchTitle = "PubMed study type model: ";
                    parameters.Add("BatchGuid", BatchGuid);
                    break;
                case -9:
                    ClassifierPipelineName = pubMedStudyDesignsPipelineName;
                    SearchTitle = "PubMed study designs model: ";
                    parameters.Add("BatchGuid", BatchGuid);
                    break;
                default:
                    return;
            }
            bool DataFactoryRes = false;
            DataFactoryHelper DFH = new DataFactoryHelper();
            try
            {
                DataFactoryRes = await DFH.RunDataFactoryProcessV2(ClassifierPipelineName, parameters, ReviewId, LogId, "ClassifierCommandV2", this.CancelToken);
            }
            catch (Exception ex)
            {
                DataFactoryHelper.UpdateReviewJobLog(LogId, ReviewId, "Failed during DF", "", "ClassifierCommandV2", true, false);
                DataFactoryHelper.LogExceptionToFile(ex, ReviewId, LogId, "ClassifierCommandV2");
            }
            if (DataFactoryRes == true)
            {
                try
                {
                    using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand("st_ClassifierInsertSearchAndScores", connection))
                        {
                            command.CommandType = System.Data.CommandType.StoredProcedure;
                            command.CommandTimeout = 300; // 5 mins to be safe. I've seen queries with large numbers of searches / items take about 30 seconds, which times out live
                            command.Parameters.Add(new SqlParameter("@BatchGuid", BatchGuid));
                            command.Parameters.Add(new SqlParameter("@REVIEW_ID", ReviewId));
                            command.Parameters.Add(new SqlParameter("@CONTACT_ID", ContactId));
                            command.Parameters.Add(new SqlParameter("@SearchTitle", SearchTitle));
                            command.ExecuteNonQuery();
                        }
                    }
                    DataFactoryHelper.UpdateReviewJobLog(LogId, ReviewId, "Ended", "", "ClassifierCommandV2", true, true);
                }
                catch (Exception ex)
                {
                    DataFactoryHelper.UpdateReviewJobLog(LogId, ReviewId, "Failed after DF", "", "ClassifierCommandV2", true, false);
                    DataFactoryHelper.LogExceptionToFile(ex, ReviewId, LogId, "ClassifierCommandV2");
                }
            }
        }

        private DataTable DownloadResults(string container, string filename)
        {
            MemoryStream ms = BlobOperations.DownloadBlobAsMemoryStream(blobConnection, container, filename);

            DataTable dt = new DataTable("Scores");
            dt.Columns.Add("SCORE", System.Type.GetType("System.Decimal"));
            dt.Columns.Add("ITEM_ID");
            dt.Columns.Add("REVIEW_ID");

#if (!CSLA_NETCORE)

			using (TextFieldParser csvReader = new TextFieldParser(ms))
			{
				csvReader.SetDelimiters(new string[] { "\t" });
				csvReader.HasFieldsEnclosedInQuotes = false;
				while (!csvReader.EndOfData)
				{
					string[] data = csvReader.ReadFields();
					if (data.Length == 3)
					{
						if (data[0] == "1")
						{
							data[0] = "0.999999";
						}
						else if (data[0] == "0")
						{
							data[0] = "0.000001";
						}
						else if (data[0].Length > 2 && data[0].Contains("E"))
						{
							double dbl = 0;
							double.TryParse(data[0], out dbl);
							//if (dbl == 0.0) throw new Exception("Gotcha!");
							data[0] = dbl.ToString("F10");
						}
						dt.Rows.Add(data);
					}

					//var data1 = csvReader.ReadFields();
					//for (var i = 0; i < data1.Length; i++)
					//{
					//    if (data1[i] == "")
					//    {
					//        data1[i] = null;
					//    }
					//}
					//dt.Rows.Add(data);
				}
			}

#else
            using (StreamReader tsvReader = new StreamReader(ms))
            {
                //csvReader.SetDelimiters(new string[] { "," });
                //csvReader.HasFieldsEnclosedInQuotes = false;
                string line = tsvReader.ReadLine();//headers line!!
                while ((line = tsvReader.ReadLine()) != null)
                {
                    string[] data = line.Split('\t');
                    dt.Rows.Add(GetSafeValue(data[4]), long.Parse(data[0]), RevInfo.ReviewId);
                }
            }
#endif
            return dt;
        }

        private void LoadResultsIntoDatabase(DataTable dt, int ContactId)
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlBulkCopy sbc = new SqlBulkCopy(connection))
                {
                    sbc.DestinationTableName = "TB_CLASSIFIER_ITEM_TEMP";
                    sbc.ColumnMappings.Clear();
                    sbc.ColumnMappings.Add("SCORE", "SCORE");
                    sbc.ColumnMappings.Add("ITEM_ID", "ITEM_ID");
                    sbc.ColumnMappings.Add("REVIEW_ID", "REVIEW_ID");
                    sbc.BatchSize = 1000;
                    sbc.WriteToServer(dt);
                }
                string SearchTitle = "Items classified according to model: " + _title;
                if (_title.Contains("¬¬CheckScreening"))
                {
                    SearchTitle = "Screening check results ordered with those mostly likely relevant at the top";
                }
                // Create a new search to 'hold' the results
                //int searchId = 0;
                using (SqlCommand command = new SqlCommand("st_ClassifierCreateSearchList", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.CommandTimeout = 300;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", RevInfo.ReviewId));
                    command.Parameters.Add(new SqlParameter("@CONTACT_ID", ContactId));
                    command.Parameters.Add(new SqlParameter("@SEARCH_TITLE", SearchTitle));
                    command.Parameters.Add(new SqlParameter("@HITS_NO", dt.Rows.Count));
                    command.Parameters.Add(new SqlParameter("@NEW_SEARCH_ID", 0));
                    command.Parameters["@NEW_SEARCH_ID"].Direction = System.Data.ParameterDirection.Output;
                    command.ExecuteNonQuery();
                    //searchId = Convert.ToInt32(command.Parameters["@NEW_SEARCH_ID"].Value); not sure we need this any more
                }
            }
        }

        private void DeleteModel()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("st_ClassifierDeleteModel", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", RevInfo.ReviewId));
                    command.Parameters.Add(new SqlParameter("@MODEL_ID", _classifierId));
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
            try
            {
                string RemoteFolder = "user_models/" + DataFactoryHelper.NameBase + "ModelId" + _classifierId + "/";
                List<BlobInHierarchy> list = BlobOperations.Blobfilenames(blobConnection, "eppi-reviewer-data", RemoteFolder);
                foreach (var toDel in list)
                {
                    Console.WriteLine("Will delete " + toDel.BlobName);
                    BlobOperations.DeleteIfExists(blobConnection, "eppi-reviewer-data", toDel.BlobName);
                }
            }
            catch (Exception ex)
            {
                DataFactoryHelper.LogExceptionToFile(ex, RevInfo.ReviewId, _classifierId, "Delete custom model");
                _returnMessage = "Error deleting models files from Azure storage.";
            }

        }

        private double GetSafeValue(string data)
        {

            if (data == "1" || data == "1.0")
            {
                data = "0.99999999";
            }
            else if (data == "0" || data == "1.0")
            {
                data = "0.00000001";
            }
            //else if (data.Length > 2 && data.Contains("E"))
            //{
            //	double dbl = 0;
            //	double.TryParse(data, out dbl);
            //	//if (dbl == 0.0) throw new Exception("Gotcha!");
            //	if (dbl < 0.000001) dbl = 0.000001;

            //             data = dbl.ToString("F10");
            //}
            return Convert.ToDouble(data);
        }

        public static string CleanText(Csla.Data.SafeDataReader reader, string field)
        {
            string text = reader.GetString(field);

            // Strip all HTML.
            text = Regex.Replace(text, "<[^<>]+>", "");

            // Strip numbers.
            //text = Regex.Replace(text, "[0-9]+", "number");

            // Strip urls.
            text = Regex.Replace(text, @"(http|https)://[^\s]*", "httpaddr");

            // Strip email addresses.
            text = Regex.Replace(text, @"[^\s]+@[^\s]+", "emailaddr");

            // Strip dollar sign.
            text = Regex.Replace(text, "[$]+", "dollar");

            // Strip usernames.
            text = Regex.Replace(text, @"@[^\s]+", "username");

            // Strip annoying punctuation and tabs
            text = text.Replace("'", " ").Replace("\"", " ").Replace(",", " ").Replace("\t", " ");

            // Strip newlines
            text = text.Replace(Environment.NewLine, " ").Replace("\n\r", " ").Replace("\n", " ").Replace("\r", " ");

            return text;

            // Tokenize and also get rid of any punctuation
            //return text.Split(" @$/#.-:&*+=[]?!(){},''\">_<;%\\".ToCharArray());
        }


        static string blobConnection = AzureSettings.blobConnection;
        
        //vals we can remove from config files? (Aug 2024)
        //static string BaseUrlScoreModel = AzureSettings.BaseUrlScoreModel;
        //static string apiKeyScoreModel = AzureSettings.apiKeyScoreModel;
        //static string BaseUrlBuildModel = AzureSettings.BaseUrlBuildModel;
        //static string apiKeyBuildModel = AzureSettings.apiKeyBuildModel;
        //static string BaseUrlScoreNewRCTModel = AzureSettings.BaseUrlScoreNewRCTModel;
        //static string apiKeyScoreNewRCTModel = AzureSettings.apiKeyScoreNewRCTModel;// Cochrane RCT Classifier v.2 (ensemble) blob storage
        const string TempPath = @"UserTempUploads\";

#endif
    }
}    


public class SVMModel2
{
	public double accuracy { get; set; }
	public double auc { get; set; }
	public double precision { get; set; }
	public double recall { get; set; }
	
}
