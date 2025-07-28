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
using System.Reflection.Emit;


#if (!CSLA_NETCORE)
using Microsoft.VisualBasic.FileIO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        private string _title = "";
        private string _mlModelName = "oldLogReg";
        private Int64 _attributeIdOn;
        private Int64 _attributeIdNotOn;
        private Int64 _attributeIdClassifyTo;
        private int _sourceId;

        // variables for applying the classifier
        private int _classifierId = -1;

        private string _returnMessage = "";

        public ClassifierCommandV2(string title, Int64 attributeIdOn, Int64 attributeIdNotOn, Int64 attributeIdClassifyTo, int classiferId, int sourceId, string mlModelName = "oldLogReg")
        {
            _title = title;
            _mlModelName = mlModelName;
            _attributeIdOn = attributeIdOn;
            _attributeIdNotOn = attributeIdNotOn;
            _returnMessage = "Success";
            _classifierId = classiferId;
            _attributeIdClassifyTo = attributeIdClassifyTo;
            _sourceId = sourceId;
            _mlModelName = mlModelName;
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
            info.AddValue("_mlModelName", _mlModelName); 
            info.AddValue("_attributeIdOn", _attributeIdOn);
            info.AddValue("_attributeIdNotOn", _attributeIdNotOn);
            info.AddValue("_returnMessage", _returnMessage);
            info.AddValue("_classifierId", _classifierId);
            info.AddValue("_attributeIdClassifyTo", _attributeIdClassifyTo);
            info.AddValue("_sourceId", _sourceId); // can also be an autoupdate Run ID if this is applying classifiers to an OpenAlex autoupdate
        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            _title = info.GetValue<string>("_title");
            _mlModelName = info.GetValue<string>("_mlModelName");
            _attributeIdOn = info.GetValue<Int64>("_attributeIdOn");
            _attributeIdNotOn = info.GetValue<Int64>("_attributeIdNotOn");
            _returnMessage = info.GetValue<string>("_returnMessage");
            _classifierId = info.GetValue<int>("_classifierId");
            _attributeIdClassifyTo = info.GetValue<Int64>("_attributeIdClassifyTo");
            _sourceId = info.GetValue<int>("_sourceId");
        }


#if !SILVERLIGHT

        /* *********************************************************************************************
         * ******************************** NEW REFACTORED VERSION**************************************
         * ********************************************************************************************/

        // Filenames and full paths for remote blob storage
        string DataFile = "";
        string ScoresFile = "";
        string VecFile = "";
        string ClfFile = "";
        string LocalFileName = ""; // local temp file for uploading data
        string RemoteFolder = "";
        string RunType = "";
        int ModelReviewId = -1;
        string BatchGuid = "";
        bool OpenAlexAutoUpdate = false;
        bool buildingNewModel = false;

        //dictionary used to translate from strings saved in `tb_CLASSIFIER_MODEL` (and the UI) to strings expected by the datafactory
        //keys are the former (DB and UI), values the latter (expected by DF)
        public static Dictionary<string, string> MlModels = new Dictionary<string, string>{
              { "oldLogReg", "Original EPPI Reviewer Classifier (LogReg)" }
            , { "lightgbm", "lightgbm"} //AKA "Light Gradient-Boosting Machine"
            , { "rfc", "RandomForestClassifier" } //AKA
            , { "xgboost", "xgboost" } //AKA "eXtreme Gradient Boosting"
            , { "svc", "SVC" } //AKA "Support Vector Clustering"
        };

        protected override void DataPortal_Execute()
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            int ReviewId = ri.ReviewId;
            int ContactId = ri.UserId;
            if (_title.StartsWith("[Apply to OpenAlex Auto Update]"))
            {
                OpenAlexAutoUpdate = true;
            }

            // simply delete a model - no further processing required
            if (_title == "DeleteThisModel~~")
            {
                SetRemoteFileNames(ReviewId, ContactId, "user_models/", "DataForScoring.tsv");
                DeleteModel();
                return;
            }

            if (_title.Contains("¬¬CheckScreening"))
            {
                SetLocalTempFilename(ReviewId, ContactId, "ChckS");
                SetRemoteFileNames(ReviewId, ContactId, "check_screening/", "");
                DoApplyCheckOrPriorityScreening2(ReviewId, ContactId, "ChckS");
                return;
            }
            if (_title.Contains("¬¬PriorityScreening"))
            {
                SetLocalTempFilename(ReviewId, ContactId, "PrioS");
                SetRemoteFileNames(ReviewId, ContactId, "priority_screening_simulation/", "");
                DoApplyCheckOrPriorityScreening2(ReviewId, ContactId, "PrioS");
                return;
            }
            // We're setting attributes, and therefore building or rebuilding a model
            if (_attributeIdOn + _attributeIdNotOn != -2)
            {
                SetLocalTempFilename(ReviewId, ContactId, "Build");
                // setting filenames later once we have a modelid
                DoTrainClassifier(ReviewId, ContactId);
                return;
            }
            
            // if we're not setting attributes AND classifierId is positive, we're scoring using an existing user model
            if (_attributeIdOn + _attributeIdNotOn == -2 && _classifierId > 0)
            {
                SetLocalTempFilename(ReviewId, ContactId, "Apply");
                // setting remote filenames a bit later for this one, as we don't know yet whether the current reviewid is correct, or if the selected model is from another review
                DoApplyClassifier2(ReviewId, ContactId, "user_models/");
                return;
            }
            // if we're not setting attributes AND classifierId is NEGATIVE, we're scoring using a built-in model
            if (_attributeIdOn + _attributeIdNotOn == -2 && _classifierId < 0)
            {
                SetLocalTempFilename(ReviewId, ContactId, "Apply");
                // setting remote filenames a bit later
                DoApplyClassifier2(ReviewId, ContactId, "builtin_model/");
                return;
            }

        }
        private void DoTrainClassifier(int ReviewId, int ContactId, bool IsOld = true) // building a classifier
        {
            RunType = "TrainClassifier";
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                int NewJobId = 0;
                List<Int64> ItemIds = new List<Int64>();
                connection.Open();
                
                if (_classifierId == -1) // building a new classifier: we save a new model into the database
                {
                    buildingNewModel = true;
                    using (SqlCommand command = new SqlCommand("st_ClassifierSaveModel", connection))//Also checks if some classifier build job is already running
                    {//we do the check and job creation in a single SP as we need the operation to be "all or nothing"
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@REVIEW_ID", ReviewId));
                        command.Parameters.Add(new SqlParameter("@MODEL_TITLE", _title + " (in progress...)"));
                        command.Parameters.Add(new SqlParameter("@CONTACT_ID", ContactId));
                        command.Parameters.Add(new SqlParameter("@ML_MODEL_NAME", _mlModelName));
                        command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ID_ON", _attributeIdOn));
                        command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ID_NOT_ON", _attributeIdNotOn));
                        command.Parameters.Add(new SqlParameter("@NEW_MODEL_ID", 0));
                        command.Parameters["@NEW_MODEL_ID"].Direction = System.Data.ParameterDirection.Output;
                        command.Parameters.Add(new SqlParameter("@NewJobId", System.Data.SqlDbType.Int));
                        command.Parameters["@NewJobId"].Direction = System.Data.ParameterDirection.Output;
                        command.ExecuteNonQuery();
                        _classifierId = Convert.ToInt32(command.Parameters["@NEW_MODEL_ID"].Value);
                        if (_classifierId == 0) // i.e. another train session is running / it's not been the specified length of time between running training yet
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
                    //newModelId = _classifierId; // we're rebuilding an existing classifier - we mark this is starting and check it's not already running
                    NewJobId = CanRunCheckAndMarkAsStarting(ReviewId, ContactId, "Build", ReviewId, (_title.Contains(" (rebuilding...)") ? _title : _title + " (rebuilding...)"));
                    if (NewJobId == 0)
                    {
                        return;
                    }
                }

                // we have a modelid now, so will set the remote filenames
                ModelReviewId = ReviewId; // when building a model, this is always the same as the current review
                SetRemoteFileNames(ReviewId, ContactId, "user_models/", "DataForTraining.tsv");

                // now save the temp file with labels for training
                if (!QueryDbAndSaveTempFileWithLabels(ReviewId, ContactId, NewJobId)) // there wasn't enough data
                {
                    UndoChangesToClassifierRecord(NewJobId, false);
                    return;
                }
                // there's enough data, so we keep going
                if (_mlModelName != "oldLogReg") //new models require two files - training data and one to apply the model to
                {
                    LocalFileName = LocalFileName.Replace("Build.tsv", "Apply4Build.tsv");
                    if (!QueryDbAndSaveTempFileWithoutLabels(ReviewId,ContactId))
                    {
                        UndoChangesToClassifierRecord(NewJobId, false);
                        return;
                    }
                    LocalFileName = LocalFileName.Replace("Apply4Build.tsv", "Build.tsv");//set the filename to what it usually is
                }
                Task.Run(() => FireAndForget(ReviewId, NewJobId, ContactId));
            }
        }
        private void UndoChangesToClassifierRecord(int JobId, bool Leave_a_trace = true)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
                {
                    connection.Open();
                    if (buildingNewModel) //building a new classifier
                    {
                        if (!Leave_a_trace)//not leaving traces, we'll delete the record of the empty model
                        {
                            using (SqlCommand command = new SqlCommand("st_ClassifierDeleteModel", connection))
                            {
                                command.CommandType = System.Data.CommandType.StoredProcedure;
                                command.Parameters.Add(new SqlParameter("@REVIEW_ID", ModelReviewId));
                                command.Parameters.Add(new SqlParameter("@MODEL_ID", _classifierId));
                                command.ExecuteNonQuery();
                            }
                        }
                        else //leaving a trace, we mark the failed model as failed
                        {
                            using (SqlCommand command = new SqlCommand("st_ClassifierUpdateModelTitle", connection))
                            {
                                command.CommandType = System.Data.CommandType.StoredProcedure;
                                command.Parameters.Add(new SqlParameter("@MODEL_ID", _classifierId));
                                command.Parameters.Add(new SqlParameter("@TITLE", _title + " (failed)"));
                                command.ExecuteNonQuery();
                            }
                        }
                    }
                    else
                    {//update: we were rebuilding it, re-set the model name
                        using (SqlCommand command = new SqlCommand("st_ClassifierUpdateModelTitle", connection))
                        {
                            command.CommandType = System.Data.CommandType.StoredProcedure;
                            command.Parameters.Add(new SqlParameter("@MODEL_ID", _classifierId));
                            if (!Leave_a_trace)//not leaving traces, rename the model to its original name
                            {
                                command.Parameters.Add(new SqlParameter("@TITLE", _title));
                            }
                            else //leaving a trace...
                            {
                                command.Parameters.Add(new SqlParameter("@TITLE", _title + " (model failed to rebuild)"));
                            }
                            command.ExecuteNonQuery();
                        }
                    }
                }
                if (File.Exists(LocalFileName)) File.Delete(LocalFileName);

            }
            catch (Exception ex)
            {
                DataFactoryHelper.LogExceptionToFile(ex, ModelReviewId, JobId, "ClassifierCommandV2");
            }
        }
        private void DoApplyClassifier2(int ReviewId, int ContactId, string modelFolder)
        {
            RunType = "ApplyClassifier";
            int NewJobId = 0;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();

                //[SG]: new 27/09/2021: find out the reviewId for this model, as it might be from a different review
                //added bonus, ensures the current user has access to this model, I guess.
                ModelReviewId = -1; //will be used later
                if (_classifierId > 0) //no need to check for the general pre-built models which are less than zero...
                {
                    using (SqlCommand command = new SqlCommand("st_ClassifierContactModels", connection))
                    {
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@CONTACT_ID", ContactId));
                        using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                        {
                            while (reader.Read())
                            {
                                int tempModelid = reader.GetInt32("MODEL_ID");
                                if (tempModelid == _classifierId)
                                {
                                    //we found it, we can stop after getting the actual ReviewId where this model was built: we need it for the filename of the model in the blob
                                    ModelReviewId = reader.GetInt32("REVIEW_ID");
                                    break;
                                }
                            }
                        }
                        command.Cancel();
                    }
                    if (ModelReviewId == -1)
                    {
                        _returnMessage = "Error, Model not found";
                        //the query above didn't find the current model, so we can't/should not continue...
                        return;
                    }
                }
                //end of 27/09/2021 addition

                NewJobId = CanRunCheckAndMarkAsStarting(ReviewId, ContactId, "Apply", ModelReviewId, "Apply model");
                if (NewJobId == 0)
                {
                    return;
                }
                // setting RemoteFileNames now that we know the right review id for the model
                SetRemoteFileNames(ReviewId, ContactId, modelFolder, "DataForScoring.tsv");
                if (!OpenAlexAutoUpdate)
                {
                    if (!QueryDbAndSaveTempFileWithoutLabels(ReviewId, NewJobId))
                    {
                        return;
                    } //if (OpenAlexAutoUpdate), we create/fill the local file inside FireAndForget, as it can take about 5s per 1000 works, so can easily take minutes.
                } 
                
                Task.Run(() => FireAndForget(ReviewId, NewJobId, ContactId));
            }
        }

        private void DoApplyCheckOrPriorityScreening2(int ReviewId, int ContactId, string CheckOrPriority)
        {
            RunType = CheckOrPriority;
            int NewJobId = CanRunCheckAndMarkAsStarting(ReviewId, ContactId, CheckOrPriority, ModelReviewId, _title);
            if (NewJobId == 0)
            {
                return;
            }
            int MinPositiveClass = 7;
            int MinNegativeClass = 10;
            int MinSampleSize = 20;
            if (CheckOrPriority == "PrioS")
            {
                MinPositiveClass = 20;
                MinNegativeClass = 40;
                MinSampleSize = 200;
            }
            if (!QueryDbAndSaveTempFileWithLabels(ReviewId, ContactId, NewJobId, MinPositiveClass, MinNegativeClass, MinSampleSize)) // there wasn't enough data
            {
                File.Delete(LocalFileName);
                return;
            }
            Task.Run(() => FireAndForget(ReviewId, NewJobId, ContactId));
        }

        private int CanRunCheckAndMarkAsStarting(int ReviewId, int ContactId, string JobType, int ReviewIdOfModel, string title)
        {
            int NewJobId = 0;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command2 = new SqlCommand("st_ClassifierCanRunCheckAndMarkAsStarting", connection))
                {//this SP also checks if a Apply (or build) job is running and creates the new job record if not
                    command2.CommandType = System.Data.CommandType.StoredProcedure;

                    command2.Parameters.Add(new SqlParameter("@MODEL_ID", SqlDbType.Int));
                    command2.Parameters["@MODEL_ID"].Value = _classifierId;
                    command2.Parameters.Add(new SqlParameter("@REVIEW_ID", ReviewId));
                    command2.Parameters.Add(new SqlParameter("@REVIEW_ID_OF_MODEL", SqlDbType.Int));
                    command2.Parameters["@REVIEW_ID_OF_MODEL"].Value = ReviewIdOfModel;
                    command2.Parameters.Add(new SqlParameter("@CONTACT_ID", ContactId));
                    command2.Parameters.Add(new SqlParameter("@JobType", JobType)); //"Apply", "Build", "ChckS" (for "Check Screening") "PrioS" (for "priority screening simulation)
                    command2.Parameters.Add(new SqlParameter("@TITLE", title));
                    command2.Parameters.Add(new SqlParameter("@NewJobId", 0));
                    command2.Parameters["@NewJobId"].Direction = System.Data.ParameterDirection.Output;
                    command2.ExecuteNonQuery();
                    NewJobId = Convert.ToInt32(command2.Parameters["@NewJobId"].Value);
                    if (NewJobId == 0)
                    {
                        _returnMessage = "Already running";
                    }
                    else
                    {
                        _returnMessage = "Starting...";
                    }
                }
            }
            return NewJobId;
        }

        private void SetLocalTempFilename(int ReviewId, int ContactId, string JobType)
        {
#if (!CSLA_NETCORE)
				LocalFileName = System.Web.HttpRuntime.AppDomainAppPath + TempPath 
                    + "ReviewID" + ReviewId + "ContactId" + ContactId.ToString() + JobType + ".tsv";
#else
            DirectoryInfo tmpDir = System.IO.Directory.CreateDirectory("UserTempUploads");
            LocalFileName = tmpDir.FullName + "\\ReviewID" + ReviewId + "ContactId" + ContactId.ToString() + JobType + ".tsv";
#endif
        }
        private void SetRemoteFileNames(int ReviewId, int ContactId, string rootFolder, string DataFileName)
        {
            BatchGuid = Guid.NewGuid().ToString();
            if (rootFolder == "user_models/" && _mlModelName == "oldLogReg") //original system
            {
                RemoteFolder = rootFolder + DataFactoryHelper.NameBase + "ReviewId" + ReviewId.ToString() + "ModelId" + _classifierId + "/";
                string RemoteModelFolder = rootFolder + DataFactoryHelper.NameBase + "ReviewId" + ModelReviewId.ToString() + "ModelId" + _classifierId + "/";
                DataFile = RemoteFolder + DataFileName;
                VecFile = RemoteModelFolder + "Vectors.pkl";
                ClfFile = RemoteModelFolder + "Clf.pkl";
                ScoresFile = RemoteFolder + "ScoresFile.tsv";
            }
            else if (rootFolder == "user_models/") //must be one of the new ML models (lightgbm | rfc | xgboost | SVC)
            {
                RemoteFolder = rootFolder + DataFactoryHelper.NameBase + "ReviewId" + ReviewId.ToString() + "ModelId" + _classifierId + "/";
                string RemoteModelFolder = rootFolder + DataFactoryHelper.NameBase + "ReviewId" + ModelReviewId.ToString() + "ModelId" + _classifierId + "/";
                DataFile = RemoteFolder + DataFileName;
                VecFile = RemoteModelFolder + "Vectors.pkl";
                ClfFile = RemoteModelFolder + "Clf.pkl";
                ScoresFile = RemoteFolder + "ScoresFile.tsv";
            }
            else if (rootFolder == "priority_screening_simulation/")
            {
                RemoteFolder = rootFolder + DataFactoryHelper.NameBase + "ReviewId" + ReviewId.ToString() + "/";
                DataFile = RemoteFolder + "PriorityScreeningSimulationData_" + BatchGuid + ".tsv";
                ScoresFile = RemoteFolder + _title.Replace("¬¬PriorityScreening¬¬", "") + ".tsv";
            }
            else if (rootFolder == "check_screening/")
            {
                RemoteFolder = rootFolder + DataFactoryHelper.NameBase + "ReviewId" + ReviewId.ToString() + "ContactId" + ContactId.ToString() + "_" + BatchGuid + "/";
                DataFile = RemoteFolder + "ScreeningCheckData.tsv";
                ScoresFile = RemoteFolder + "ScreeningCheckScores.tsv";
            }
            else if (rootFolder == "builtin_model/")
            {
                RemoteFolder = rootFolder + DataFactoryHelper.NameBase + "ReviewId" + ReviewId.ToString() + "ContactId" + ContactId.ToString() + "_" + BatchGuid + "/";
                DataFile = RemoteFolder + DataFileName;
                VecFile = RemoteFolder + "Vectors.pkl";
                ClfFile = RemoteFolder + "Clf.pkl";
                ScoresFile = RemoteFolder + "ScoresFile.tsv";
            }
        }
        private bool QueryDbAndSaveTempFileWithLabels(int ReviewId, int ContactId, int LogId, int MinPositiveClass = 7, int MinNegativeClass = 7, int MinSampleSize = 20)
        {
            List<Int64> ItemIds = new List<Int64>();
            int positiveClassCount = 0;
            int negativeClasscount = 0;
            int sampleSize = 0;
            try
            {
                using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand("st_ClassifierGetTrainingData", connection))
                    {
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@REVIEW_ID", ReviewId));
                        command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ID_ON", _attributeIdOn));
                        command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ID_NOT_ON", _attributeIdNotOn));
                        using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                        {
                            using (System.IO.StreamWriter file = new System.IO.StreamWriter(LocalFileName, false))
                            {
                                file.WriteLine("PaperId\tPaperTitle\tAbstract\tIncl");
                                //file.WriteLine("0\tand\tand\t98");
                                while (reader.Read())
                                {
                                    if (ItemIds.IndexOf(reader.GetInt64("ITEM_ID")) == -1)
                                    {
                                        sampleSize++;
                                        ItemIds.Add(reader.GetInt64("ITEM_ID"));
                                        WriteDataLineInFileToUpload(reader.GetInt64("ITEM_ID").ToString(), CleanText(reader, "title"),
                                            CleanText(reader, "abstract"), CleanText(reader, "LABEL"), file);
                                        if (reader["LABEL"].ToString() == "1")
                                            positiveClassCount++;
                                        else
                                            negativeClasscount++;
                                    }
                                }
                            }
                        }
                    }
                    if (positiveClassCount < MinPositiveClass) 
                    {
                        _returnMessage = "Insufficient Data (Positives count is " + positiveClassCount.ToString() + ").\r\nThis task requires at least "
                            + MinPositiveClass.ToString() + " positive items, "
                            + MinNegativeClass.ToString() + " negative items and "
                            + MinSampleSize.ToString() + " total sample size.";
                        DataFactoryHelper.UpdateReviewJobLog(LogId, ReviewId, "Failed", _returnMessage, "ClassifierCommandV2", true, false);
                        return false;
                    }
                    else if (negativeClasscount < MinNegativeClass) 
                    {
                        _returnMessage = "Insufficient Data (Negatives count is " + negativeClasscount.ToString() + ").\r\nThis task requires at least "
                            + MinPositiveClass.ToString() + " positive items, "
                            + MinNegativeClass.ToString() + " negative items and "
                            + MinSampleSize.ToString() + " total sample size.";
                        DataFactoryHelper.UpdateReviewJobLog(LogId, ReviewId, "Failed", _returnMessage, "ClassifierCommandV2", true, false);
                        return false;
                    }
                    else if (sampleSize < MinSampleSize)
                    {
                        _returnMessage = "Insufficient Data (Sample size is " + sampleSize.ToString() + ").\r\nThis task requires at least "
                            + MinPositiveClass.ToString() + " positive items, "
                            + MinNegativeClass.ToString() + " negative items and "
                            + MinSampleSize.ToString() + " total sample size.";
                        DataFactoryHelper.UpdateReviewJobLog(LogId, ReviewId, "Failed", _returnMessage, "ClassifierCommandV2", true, false);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                _returnMessage = "Failed to get data to train classifier";
                DataFactoryHelper.UpdateReviewJobLog(LogId, ReviewId, "Failed to get data to train classifier", "", "ClassifierCommandV2", true, false);
                DataFactoryHelper.LogExceptionToFile(ex, ReviewId, LogId, "ClassifierCommandV2");
                return false;
            }
            if (AppIsShuttingDown)
            {
                _returnMessage = "Cancelled after writing temp file";
                DataFactoryHelper.UpdateReviewJobLog(LogId, ReviewId, "Cancelled after writing temp file", "", "ClassifierCommandV2", true, false);
                return false;
            }
            return true;
        }
        private bool QueryDbAndSaveTempFileWithoutLabels(int ReviewId, int LogId)
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
                                string LabelSt = "99";

                                while (reader.Read())
                                {
                                    if (ItemIds.IndexOf(reader.GetInt64("ITEM_ID")) == -1)
                                    {
                                        ItemIds.Add(reader.GetInt64("ITEM_ID"));
                                        WriteDataLineInFileToUpload(reader.GetInt64("ITEM_ID").ToString(), CleanText(reader, "title"), 
                                            CleanText(reader, "abstract"), LabelSt, file);
                                       
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
                _returnMessage = "Failed to get data to score";
                try { File.Delete(LocalFileName); }
                catch { }
                DataFactoryHelper.UpdateReviewJobLog(LogId, ReviewId, "Failed to get data to score", "", "ClassifierCommandV2", true, false);
                DataFactoryHelper.LogExceptionToFile(ex, ReviewId, LogId, "ClassifierCommandV2");
                return false;
            }
            if (ItemIds.Count < 1)
            {
                _returnMessage = "Failed: no data to score";
                try { File.Delete(LocalFileName); }
                catch { }
                DataFactoryHelper.UpdateReviewJobLog(LogId, ReviewId, "Failed", "No data to score", "ClassifierCommandV2", true, false);
                return false;
            }
            if (AppIsShuttingDown)
            {
                _returnMessage = "Job cancelled: app is restarting";
                try { File.Delete(LocalFileName); }
                catch { }
                DataFactoryHelper.UpdateReviewJobLog(LogId, ReviewId, "Cancelled after writing temp file", "", "ClassifierCommandV2", true, false);
                return false;
            }
            return true;
        }

        private bool QueryDbAndSaveOpenAlexTempFileWithoutLabels(int ReviewId, int ContactId, int LogId)
        {
            List<Int64> Ids = new List<long>();
            try
            {
                using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand("st_MagAutoUpdateRunResults", connection))
                    {
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@MagAutoUpdateRunId", _sourceId));
                        command.Parameters.Add(new SqlParameter("@OrderBy", "AutoUpdate"));
                        command.Parameters.Add(new SqlParameter("@AutoUpdateScore", Convert.ToDouble(0.0)));
                        command.Parameters.Add(new SqlParameter("@StudyTypeClassifierScore", Convert.ToDouble(0.0)));
                        command.Parameters.Add(new SqlParameter("@UserClassifierScore", Convert.ToDouble(0.0)));
                        //command.Parameters.Add(new SqlParameter("@TopN", _TopN)); // edited SQL file to give this a default
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
            }
            catch (Exception ex)
            {
                DataFactoryHelper.UpdateReviewJobLog(LogId, ReviewId, "Failed", "Failed to get data to score", "ClassifierCommandV2", true, false);
                DataFactoryHelper.LogExceptionToFile(ex, ReviewId, LogId, "ClassifierCommandV2");
                return false;
            }

            if (Ids.Count == 0)
            {
                DataFactoryHelper.UpdateReviewJobLog(LogId, ReviewId, "Failed", "No papers to score", "ClassifierCommandV2", true, false);
                return false;
            }
            if (AppIsShuttingDown)
            {
                DataFactoryHelper.UpdateReviewJobLog(LogId, ReviewId, "Cancelled before data-fetch", "", "ClassifierCommandV2", true, false);
                return false;
            }
            try
            {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(LocalFileName, false))
                {
                    //file.WriteLine("\"ITEM_ID\",\"LABEL\",\"TITLE\",\"ABSTRACT\",\"KEYWORDS\",\"REVIEW_ID\"");
                    file.WriteLine("PaperId\tPaperTitle\tAbstract\tIncl");
                    int count = 0; 
                    int batchSize = 100;//max number of terms in a query is 100. Max page size is 200, but we're restricted by max N of Ids we can ask for
                    bool isFirstPaper = true;
                    while (count < Ids.Count)
                    {
                        string query = "";
                        for (int i = count; i < Ids.Count && i < count + batchSize; i++)
                        {
                            if (query == "")
                            {
                                query = "W" + Ids[i].ToString();
                            }
                            else
                            {
                                query += "|W" + Ids[i].ToString();
                            }
                        }
                        MagMakesHelpers.OaPaperFilterResult resp = MagMakesHelpers.EvaluateOaPaperFilter("openalex_id:https://openalex.org/" + query, batchSize.ToString(), "1", false);
                        //"resp" can be null, if the communication failed, would trigger an exception logged via the catch
                        foreach (MagMakesHelpers.OaPaper pm in resp.results)
                        {
                            if (isFirstPaper)
                            {
                                isFirstPaper = false;
                                if (pm.title == "" && (pm.abstract_inverted_index == null || pm.abstract_inverted_index.Count == 0))
                                {//we spoof the first paper if and only if both title and abstract are empty, otherwise the file can't be parsed by the ML side 
                                    file.WriteLine(pm.id.Replace("https://openalex.org/W", "") + "\t" +
                                            "and" + "\t" + //"and" is a stopword, so equivalent to the empty field
                                            "and" + "\t" +
                                            "99");
                                    continue;
                                }
                            }
                            WriteDataLineInFileToUpload(pm.id.Replace("https://openalex.org/W", "")
                                , MagMakesHelpers.CleanText(pm.title)
                                , MagMakesHelpers.CleanText(MagMakesHelpers.ReconstructInvertedAbstract(pm.abstract_inverted_index))
                                , "99", file);

                            //file.WriteLine(pm.id.Replace("https://openalex.org/W", "") + "\t" +
                            //                MagMakesHelpers.CleanText(pm.title) + "\t" +
                            //                MagMakesHelpers.CleanText(MagMakesHelpers.ReconstructInvertedAbstract(pm.abstract_inverted_index)) + "\t" +
                            //                "99");
                        }
                        count += batchSize;
                        if (AppIsShuttingDown)
                        {
                            file.Close();
                            break;//can't delete the file inside the "using file" block!
                        }
                    }
                }
                if (AppIsShuttingDown)
                {//now we can delete the file!
                    DataFactoryHelper.UpdateReviewJobLog(LogId, ReviewId, "Cancelled during data-fetch", "", "ClassifierCommandV2", true, false);
                    File.Delete(LocalFileName);                     
                    return false;
                }
            }
            catch (Exception ex)
            {
                try { File.Delete(LocalFileName); }
                catch { }//empty catch, not worth the effort!
                DataFactoryHelper.UpdateReviewJobLog(LogId, ReviewId, "Failed", "Failed at fetching data from OA", "ClassifierCommandV2", true, false);
                DataFactoryHelper.LogExceptionToFile(ex, ReviewId, LogId, "ClassifierCommandV2");
                return false;
            }
            return true;
        }

        // ************************* Now the async stuff starts. All types of calls end above with a call to FireAndForget, which then controlls program flow from here on ******************
        // ************************* Exceptions need to be caught within local code as there is nothing else than can catch them now, and if not caught, can bring ER down ******************
        private async void FireAndForget(int ReviewId, int LogId, int ContactId)
        {
            if (RunType == "ApplyClassifier" && OpenAlexAutoUpdate)
            {//getting data from OpenAlex, which is slow, so we do it here, rather than when the user is still waiting for the ER API to answer.
                if (!QueryDbAndSaveOpenAlexTempFileWithoutLabels(ReviewId, ContactId, LogId))
                {
                    return;
                }
            }

            // everything uploads a temp file to blob
            if (!UploadTempFileToBlob(ReviewId, LogId))
            {
                if (RunType == "TrainClassifier")
                {
                    using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
                    {
                        connection.Open();
                        UndoChangesToClassifierRecord(LogId, true);
                    }
                }
                return;
            }
            //but for new classifier system, we upload also a file to apply the new model to
            if (RunType == "TrainClassifier" && _mlModelName != "oldLogReg")
            {
                LocalFileName = LocalFileName.Replace("Build.tsv", "Apply4Build.tsv");
                DataFile = DataFile.Replace("DataForTraining.tsv", "DataForTrainingEvaluation.tsv");
                if (!UploadTempFileToBlob(ReviewId, LogId))
                {
                    using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
                    {
                        connection.Open();
                        UndoChangesToClassifierRecord(LogId, true);
                    }
                    return;
                }
                LocalFileName = LocalFileName.Replace("Apply4Build.tsv", "Build.tsv");//set the filenames back to what it usually is
                DataFile = DataFile.Replace("DataForTrainingEvaluation.tsv", "DataForTraining.tsv");
            }
            Dictionary<string, object> parameters = GetAdfParameters(RunType);

            if (parameters.Count == 0 && RunType == "TrainClassifier")
            {
                UndoChangesToClassifierRecord(LogId, true);
                return;
            }

            // everything triggers the ADF API
            bool DFresult = await RunDataFactoryJobAsync(ReviewId, LogId, parameters);
            if (DFresult) //if DF didn't work, we trust the reason was logged appropriately either in DFHelper or in RunDataFactoryJobAsync
            {
                switch (RunType)// ADF has completed, so we now process results
                {
                    case "TrainClassifier":
                        //IMPORTANT! We do NOT delete training data as having it allows to rebuild models at ANY time, and it did save the day already once in the past
                        DownloadTrainClassifierResults(ReviewId, LogId);
                        break;
                    case "ApplyClassifier":
                        DownloadApplyClassifierResults(LogId, ContactId, ReviewId);//will "cleanup" data uploaded
                        break;
                    case "PrioS":
                        // We literally do fire and forget here, as we don't need to do anything with the results
                        //we do delete the uploaded file, though
                        try
                        {
                            BlobOperations.DeleteIfExists(blobConnection, "eppi-reviewer-data", DataFile);
                        }
                        catch (Exception ex)
                        {
                            DataFactoryHelper.UpdateReviewJobLog(LogId, ReviewId, "Failed to delete uploaded file", "", "ClassifierCommandV2", true, false);
                            DataFactoryHelper.LogExceptionToFile(ex, ReviewId, LogId, "ClassifierCommandV2");
                            break;
                        }
                        DataFactoryHelper.UpdateReviewJobLog(LogId, ReviewId, "Ended", "", "ClassifierCommandV2", true, true);
                        break;
                    case "ChckS":
                        DownloadApplyClassifierResults(LogId, ContactId, ReviewId);//will "cleanup" data uploaded                        
                        break;
                    default: // should never hit this
                        break;
                }
            }
            else //DF did not work
            {
                if (RunType == "TrainClassifier")
                {
                    UndoChangesToClassifierRecord(LogId, true);
                }
            }
        }
        private bool UploadTempFileToBlob(int ReviewId, int LogId)
        {
            if (AppIsShuttingDown)
            {
                DataFactoryHelper.UpdateReviewJobLog(LogId, ReviewId, "Cancelled before upload", "", "ClassifierCommandV2", true, false);
                return false;
            }
            try
            {
                DataFactoryHelper.UpdateReviewJobLog(LogId, ReviewId, "Uploading", "", "ClassifierCommandV2");
                using (var fileStream = System.IO.File.OpenRead(LocalFileName))
                {
                    BlobOperations.UploadStream(blobConnection, "eppi-reviewer-data", DataFile, fileStream);
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
                BlobOperations.DeleteIfExists(blobConnection, "eppi-reviewer-data", DataFile);
                DataFactoryHelper.UpdateReviewJobLog(LogId, ReviewId, "Failed to upload data", "", "ClassifierCommandV2", true, false);
                return false;
            }
            if (AppIsShuttingDown)
            {
                DataFactoryHelper.UpdateReviewJobLog(LogId, ReviewId, "Cancelled after upload", "", "TrainingRunCommandV2", true, false);
                return false;
            }
            return true;
        }

        private Dictionary<string, object> GetAdfParameters(string jobType)
        {
            if (jobType == "TrainClassifier" && _mlModelName != "oldLogReg")
            {//completely different parameters are used for the newer classifiers (25/07/2025)
                string modelName = "N/A";
                var mdl = MlModels[_mlModelName];
                if (mdl == null)
                {//something is wrong, we're trying to train for a model we did not recognise
                    //code calling this method will detect the case where we return no params at all
                    return new Dictionary<string, object>();
                }
                else
                {
                    modelName = mdl;
                }
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { "labelled_data_path", DataFile.Replace(RemoteFolder, "")}
                    ,{ "unlabelled_data_path", DataFile.Replace("DataForTraining.tsv", "DataForTrainingEvaluation.tsv").Replace(RemoteFolder, "")}
                    ,{ "label_header", "Incl" }
                    ,{ "positive_class_value", 1 }
                    ,{ "title_header", "PaperTitle"  }
                    ,{ "abstract_header", "Abstract" } 
                    ,{ "model_name", modelName}
                    ,{ "working_container_url", "https://eppimlprod.blob.core.windows.net/eppi-reviewer-data/" + RemoteFolder}
                    ,{ "output_container_path", "Output"}
                    , {"managed_identity_client_id", AzureSettings.dataFactoryManagedIdentity}
                };
                return parameters;
            }
            else
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    {"DataFile", DataFile },
                    {"ScoresFile", ScoresFile },
                    {"EPPIReviewerApiRunId", BatchGuid},
                    {"VecFile", VecFile},
                    {"ClfFile", ClfFile}
                };
                switch (jobType)
                {
                    case "TrainClassifier":
                        parameters.Add("do_build_log_reg", true);
                        break;
                    case "ApplyClassifier":
                        parameters.Add(GetModelParam(), true);
                        break;
                    case "PrioS":
                        parameters.Add("do_priority_screening_simulation", true);
                        break;
                    case "ChckS":
                        parameters.Add("do_check_screening", true);
                        break;

                }
                return parameters;
            }
        }

        private string GetModelParam()
        {
            switch (_classifierId)
            {
                case -1:
                    return "do_original_rct_classifier_b";
                case -2:
                    return "do_systematic_reviews_classifier_b";
                case -3:
                    return "do_economic_eval_classifier_b";
                case -4:
                    return "do_cochrane_rct_classifier_b";
                case -5:
                    return "do_covid_map_categories_b";
                case -6:
                    return "do_long_covid_svm_b";
                case -7:
                    return "do_progress_plus"; // this one isn't deployed on ER6
                case -8:
                    return "do_pubmed_study_designs_b"; // this one is in ER4 only for admins, but it's not very good, so I didn't deploy to ER6
                case -9:
                    return "do_pubmed_study_designs_b";

                default: return "do_score_log_reg"; // i.e. a user custom model
            }
        }

        private async Task<bool> RunDataFactoryJobAsync(int ReviewId, int LogId, Dictionary<string, object> parameters)
        {
            bool DataFactoryRes = false;
            try
            {
                DataFactoryHelper DFH = new DataFactoryHelper();
                string pipelineName = "EPPI-Reviewer_API";
                if (RunType == "TrainClassifier" && _mlModelName != "oldLogReg") pipelineName = "Sam Find Model Pipeline";
                DataFactoryRes = await DFH.RunDataFactoryProcessV2(pipelineName, parameters, ReviewId, LogId, "ClassifierCommandV2", this.CancelToken);
            }
            catch (Exception ex)
            {
                DataFactoryHelper.LogExceptionToFile(ex, ReviewId, LogId, "ClassifierCommandV2");
                DataFactoryHelper.UpdateReviewJobLog(LogId, ReviewId, "Failed at DataFactory", "", "ClassifierCommandV2", true, false);
                return false;
            }
            if (DataFactoryRes)
            {
                if (AppIsShuttingDown)//DF will log if ER signalled to shut down while DF is running, so here we're only checking if the shut down signal happened After DF last checked
                {
                    DataFactoryHelper.UpdateReviewJobLog(LogId, ReviewId, "Cancelled after DF", "", "ClassifierCommandV2", true, false);
                    return false;
                }
                return true;
            }
            else
            {
                return false;
            }
        }
        private bool DownloadTrainClassifierResults(int ReviewId, int LogId)
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

                        command2.Parameters.Add(new SqlParameter("@MODEL_ID", _classifierId));
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
                //IMPORTANT! We do NOT delete training data as having it allows to rebuild models at ANY time, and it did save the day already once in the past
                DataFactoryHelper.UpdateReviewJobLog(LogId, ReviewId, "Ended", "", "ClassifierCommandV2", true, true);
            }
            catch (Exception ex)
            {
                UndoChangesToClassifierRecord(LogId, true);
                DataFactoryHelper.LogExceptionToFile(ex, ReviewId, LogId, "ClassifierCommandV2");
                DataFactoryHelper.UpdateReviewJobLog(LogId, ReviewId, "Failed to download data", "", "ClassifierCommandV2", true, false);
                return false;
            }
            return true;
        }

        private bool DownloadApplyClassifierResults(int LogId, int ContactId, int ReviewId)
        {
            DataTable Scores = new DataTable();
            try
            {
                Scores = LoadResultsIntoDatatable(ReviewId);
                if (AppIsShuttingDown)//Last "graceful shutdown" chance
                {
                    DataFactoryHelper.UpdateReviewJobLog(LogId, ReviewId, "Cancelled after Downloading results", "", "ClassifierCommandV2", true, false);
                    return false;
                }
                // some classifiers just return a single value; others return multiple ones, with each category in 'PredictedLabel'
                // datatables with the 'predictedlabel' field have 4 columns
                if (Scores.Columns.Count > 3)
                {
                    // get a list of the unique values
                    var uniqueValues = Scores.AsEnumerable()
                                .Select(row => row.Field<string>("PredictedLabel"))
                                .Distinct();

                    // go through each value and create a search + results set for each
                    foreach (var value in uniqueValues)
                    {
                        DataTable subset = Scores.AsEnumerable()
                                                .Where(row => row.Field<string>("PredictedLabel") == value)
                                                .CopyToDataTable();
                        subset.Columns.Remove("PredictedLabel");
                        LoadDataTableIntoDatabase(subset, ContactId, value.ToString(), LogId, ReviewId);
                    }
                }
                else
                {
                    LoadDataTableIntoDatabase(Scores, ContactId, _title, LogId, ReviewId);
                }
                
                DataFactoryHelper.UpdateReviewJobLog(LogId, ReviewId, "Ended", "", "ClassifierCommandV2", true, true);
            }
            catch (Exception ex)
            {
                DataFactoryHelper.UpdateReviewJobLog(LogId, ReviewId, "Failed at downloading/saving results", "", "ClassifierCommandV2", true, false);
                DataFactoryHelper.LogExceptionToFile(ex, ReviewId, LogId, "ClassifierCommandV2");
                return false;
            }

            try
            {
                BlobOperations.DeleteIfExists(blobConnection, "eppi-reviewer-data", DataFile);
                BlobOperations.DeleteIfExists(blobConnection, "eppi-reviewer-data", ScoresFile);
            }
            catch (Exception ex)
            {
                DataFactoryHelper.UpdateReviewJobLog(LogId, ReviewId, "Failed deleting remote files", "", "ClassifierCommandV2", true, false);
                DataFactoryHelper.LogExceptionToFile(ex, ReviewId, LogId, "ClassifierCommandV2");
            }
            return true;
        }
        private DataTable LoadResultsIntoDatatable(int ReviewId)
        {
            MemoryStream ms = BlobOperations.DownloadBlobAsMemoryStream(blobConnection, "eppi-reviewer-data", ScoresFile);

            DataTable dt = new DataTable("Scores");
            int indexScore = -1;
            int IndexItemId = -1;
            int IndexPredictedLabel = -1;

            using (StreamReader tsvReader = new StreamReader(ms))
            {
                string line = tsvReader.ReadLine();//headers line
                string[] lines = line.Split('\t');
                // we do this because we want to make sure we have the right indexes for each column (used next) and they can vary depending on classifier
                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i] == "SCORE" || lines[i] == "Score" || lines[i] == "probabilities")
                    {
                        indexScore = i;
                    }
                    if (lines[i] == "ITEM_ID" ||  lines[i] == "PaperId")
                    {
                        IndexItemId = i;
                    }
                    if (lines[i] == "PredictedLabel")
                    {
                        IndexPredictedLabel = i;
                    }
                }
                dt.Columns.Add("SCORE", System.Type.GetType("System.Decimal"));
                dt.Columns.Add("ITEM_ID");
                dt.Columns.Add(!OpenAlexAutoUpdate ? "REVIEW_ID" : "MAG_AUTO_UPDATE_RUN_ID");
                if (IndexPredictedLabel != -1)
                {
                    dt.Columns.Add("PredictedLabel");
                }

                while ((line = tsvReader.ReadLine()) != null)
                {
                    string[] data = line.Split('\t');
                    if (IndexPredictedLabel == -1)
                    {
                        if (data[IndexItemId].Length > 2) // protection against any empty rows
                        {
                            dt.Rows.Add(GetSafeValue(data[indexScore]), long.Parse(data[IndexItemId]), !OpenAlexAutoUpdate ? ReviewId : _sourceId);
                        }
                    }
                    else
                    {
                        if (data[IndexItemId].Length > 2)
                        {
                            dt.Rows.Add(GetSafeValue(data[indexScore]), long.Parse(data[IndexItemId]), !OpenAlexAutoUpdate ? ReviewId : _sourceId, data[IndexPredictedLabel]); // (OpenAlexUpdates have no PredictedLabel column)
                        }
                    }
                }
            }

            // Code above is NOT tested on ER4 :( My feeling is that we don't need the compiler switch and that the above should do for both?

//#if (!CSLA_NETCORE)

//			using (TextFieldParser csvReader = new TextFieldParser(ms))
//			{
//				csvReader.SetDelimiters(new string[] { "\t" });
//				csvReader.HasFieldsEnclosedInQuotes = false;
//				while (!csvReader.EndOfData)
//				{
//					string[] data = csvReader.ReadFields();
//					if (data.Length == 3)
//					{
//						if (data[0] == "1")
//						{
//							data[0] = "0.999999";
//						}
//						else if (data[0] == "0")
//						{
//							data[0] = "0.000001";
//						}
//						else if (data[0].Length > 2 && data[0].Contains("E"))
//						{
//							double dbl = 0;
//							double.TryParse(data[0], out dbl);
//							//if (dbl == 0.0) throw new Exception("Gotcha!");
//							data[0] = dbl.ToString("F10");
//						}
//						dt.Rows.Add(data);
//					}

//					//var data1 = csvReader.ReadFields();
//					//for (var i = 0; i < data1.Length; i++)
//					//{
//					//    if (data1[i] == "")
//					//    {
//					//        data1[i] = null;
//					//    }
//					//}
//					//dt.Rows.Add(data);
//				}
//			}

//#else

//#endif
            return dt;
        }
        private void LoadDataTableIntoDatabase(DataTable dt, int ContactId, string title, int LogId, int ReviewId)
        {
            if (OpenAlexAutoUpdate)
            {
                LoadDataTableIntoOpenAlexAutoUpdate(dt, ContactId, title, LogId, ReviewId);
                return;
            }
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
                string SearchTitle = title;
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

        private void LoadDataTableIntoOpenAlexAutoUpdate(DataTable dt, int ContactId, string title, int LogId, int ReviewId)
        {
            string field = "UserClassifierScore";
            string StudyTypeClassifier = "";
            if (_classifierId < 0)
            {
                field = "StudyTypeClassifierScore";
            }
            if (_classifierId < 0)
            {
                switch (_classifierId)
                {
                    case -1:
                        StudyTypeClassifier = "RCT";
                        break;
                    case -2:
                        StudyTypeClassifier = "Systematic review";
                        break;
                    case -3:
                        StudyTypeClassifier = "Economic evaluation";
                        break;
                    case -4:
                        StudyTypeClassifier = "Cochrane RCT";
                        break;
                }
            }
            try
            {
                using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
                {
                    connection.Open();
                    using (SqlBulkCopy sbc = new SqlBulkCopy(connection))
                    {
                        sbc.DestinationTableName = "TB_MAG_AUTO_UPDATE_CLASSIFIER_TEMP";
                        sbc.ColumnMappings.Clear();
                        sbc.ColumnMappings.Add("SCORE", "Score");
                        sbc.ColumnMappings.Add("ITEM_ID", "PaperId");
                        sbc.ColumnMappings.Add("MAG_AUTO_UPDATE_RUN_ID", "MAG_AUTO_UPDATE_RUN_ID");
                        sbc.BatchSize = 1000;
                        sbc.WriteToServer(dt);
                    }

                    using (SqlCommand command = new SqlCommand("st_MagAutoUpdateClassifierScoresUpdate", connection))
                    {
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@MAG_AUTO_UPDATE_RUN_ID", _sourceId));
                        command.Parameters.Add(new SqlParameter("@Field", field));
                        command.Parameters.Add(new SqlParameter("@StudyTypeClassifier", StudyTypeClassifier));
                        command.Parameters.Add(new SqlParameter("@UserClassifierModelId", _classifierId));
                        command.Parameters.Add(new SqlParameter("@UserClassifierReviewId", ModelReviewId));
                        command.ExecuteNonQuery();
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                DataFactoryHelper.UpdateReviewJobLog(LogId, ReviewId, "Failed to save results", "", "ClassifierCommandV2", true, false);
                DataFactoryHelper.LogExceptionToFile(ex, ReviewId, LogId, "ClassifierCommandV2");
                return;
            }
        }

        /*************************************************************************************************
         * *********************************** END NEW REFACTORED VERSION ********************************
         * **********************************************************************************************/


            //        private void DoTrainClassifier()
            //        {
            //            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            //            {
            //                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            //                int newModelId = 0;
            //                int NewJobId = 0;
            //                int ReviewId = ri.ReviewId;
            //                List<Int64> ItemIds = new List<Int64>();
            //                int positiveClassCount = 0;
            //                int negativeClasscount = 0;
            //                int sampleSize = 0;

            //                connection.Open();

            //                if (_classifierId == -1) // building a new classifier
            //                {
            //                    using (SqlCommand command = new SqlCommand("st_ClassifierSaveModel", connection))//Also checks if some classifier build job is already running
            //                    {//we do the check and job creation in a single SP as we need the operation to be "all or nothing"
            //                        command.CommandType = System.Data.CommandType.StoredProcedure;
            //                        command.Parameters.Add(new SqlParameter("@REVIEW_ID", ReviewId));
            //                        command.Parameters.Add(new SqlParameter("@MODEL_TITLE", _title + " (in progress...)"));
            //                        command.Parameters.Add(new SqlParameter("@CONTACT_ID", ri.UserId));
            //                        command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ID_ON", _attributeIdOn));
            //                        command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ID_NOT_ON", _attributeIdNotOn));
            //                        command.Parameters.Add(new SqlParameter("@NEW_MODEL_ID", 0));
            //                        command.Parameters["@NEW_MODEL_ID"].Direction = System.Data.ParameterDirection.Output;
            //                        command.Parameters.Add(new SqlParameter("@NewJobId", System.Data.SqlDbType.Int));
            //                        command.Parameters["@NewJobId"].Direction = System.Data.ParameterDirection.Output;
            //                        command.ExecuteNonQuery();
            //                        newModelId = Convert.ToInt32(command.Parameters["@NEW_MODEL_ID"].Value);
            //                        if (newModelId == 0) // i.e. another train session is running / it's not been the specified length of time between running training yet
            //                        {
            //                            _returnMessage = "Already running";
            //                            return;
            //                        }
            //                        else
            //                        {
            //                            _returnMessage = "Starting...";
            //                            NewJobId = (int)command.Parameters["@NewJobId"].Value;
            //                        }
            //                    }
            //                }
            //                else
            //                {
            //                    _returnMessage = "";
            //                    newModelId = _classifierId; // we're rebuilding an existing classifier

            //                    using (SqlCommand command2 = new SqlCommand("st_ClassifierCanRunCheckAndMarkAsStarting", connection))
            //                    {//this SP also checks if a build (or Apply) job is running and creates the new job record if not
            //                        command2.CommandType = System.Data.CommandType.StoredProcedure;

            //                        command2.Parameters.Add(new SqlParameter("@MODEL_ID", _classifierId));
            //                        command2.Parameters.Add(new SqlParameter("@REVIEW_ID", ReviewId));
            //                        command2.Parameters.Add(new SqlParameter("@REVIEW_ID_OF_MODEL", ReviewId));
            //                        command2.Parameters.Add(new SqlParameter("@CONTACT_ID", ri.UserId));
            //                        command2.Parameters.Add(new SqlParameter("@TITLE", _title.Contains(" (rebuilding...)") ? _title : _title + " (rebuilding...)"));
            //                        command2.Parameters.Add(new SqlParameter("@JobType", "Build"));
            //                        command2.Parameters.Add(new SqlParameter("@NewJobId", 0));
            //                        command2.Parameters["@NewJobId"].Direction = System.Data.ParameterDirection.Output;
            //                        command2.ExecuteNonQuery();
            //                        NewJobId = Convert.ToInt32(command2.Parameters["@NewJobId"].Value);
            //                        if (NewJobId == 0)
            //                        {
            //                            _returnMessage = "Already running";
            //                            return;
            //                        }
            //                        else
            //                        {
            //                            _returnMessage = "Starting...";
            //                        }
            //                    }
            //                }
            //#if (!CSLA_NETCORE)
            //				LocalFileName = System.Web.HttpRuntime.AppDomainAppPath + TempPath 
            //                    + "ReviewID" + ri.ReviewId + "ContactId" + ri.UserId.ToString() + ".tsv";
            //#else
            //                DirectoryInfo tmpDir = System.IO.Directory.CreateDirectory("UserTempUploads");
            //                LocalFileName = tmpDir.FullName + "\\ReviewID" + ri.ReviewId + "ContactId" + ri.UserId.ToString() + ".tsv";
            //#endif
            //                using (SqlCommand command = new SqlCommand("st_ClassifierGetTrainingData", connection))
            //                {
            //                    command.CommandType = System.Data.CommandType.StoredProcedure;
            //                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
            //                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ID_ON", _attributeIdOn));
            //                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ID_NOT_ON", _attributeIdNotOn));
            //                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
            //                    {
            //                        using (System.IO.StreamWriter file = new System.IO.StreamWriter(LocalFileName, false))
            //                        {
            //                            file.WriteLine("PaperId\tPaperTitle\tAbstract\tIncl");
            //                            while (reader.Read())
            //                            {
            //                                if (ItemIds.IndexOf(reader.GetInt64("ITEM_ID")) == -1)
            //                                {
            //                                    sampleSize++;
            //                                    ItemIds.Add(reader.GetInt64("ITEM_ID"));
            //                                    file.WriteLine(reader["item_id"].ToString() + "\t" +
            //                                        CleanText(reader, "title") + "\t" +
            //                                        CleanText(reader, "abstract") + "\t" +
            //                                        CleanText(reader, "LABEL"));
            //                                    if (reader["LABEL"].ToString() == "1")
            //                                        positiveClassCount++;
            //                                    else
            //                                        negativeClasscount++;
            //                                }
            //                            }
            //                        }
            //                    }
            //                }
            //                if (positiveClassCount < 7 || negativeClasscount < 7 || sampleSize < 20)//at least 7 examples in each class and at least 20 records in total
            //                {
            //                    _returnMessage = "Insufficient data";
            //                    if (_classifierId == -1) //building a new classifier, there is not enough data, so we're not saving it
            //                    {
            //                        using (SqlCommand command = new SqlCommand("st_ClassifierDeleteModel", connection))
            //                        {
            //                            command.CommandType = System.Data.CommandType.StoredProcedure;
            //                            command.Parameters.Add(new SqlParameter("@REVIEW_ID", ReviewId));
            //                            command.Parameters.Add(new SqlParameter("@MODEL_ID", newModelId));
            //                            command.ExecuteNonQuery();
            //                        }
            //                    }
            //                    else
            //                    {//update: we were rebuilding it, re-set the model name
            //                        using (SqlCommand command = new SqlCommand("st_ClassifierUpdateModelTitle", connection))
            //                        {
            //                            command.CommandType = System.Data.CommandType.StoredProcedure;
            //                            command.Parameters.Add(new SqlParameter("@MODEL_ID", newModelId));
            //                            command.Parameters.Add(new SqlParameter("@TITLE", _title));
            //                            command.ExecuteNonQuery();
            //                        }
            //                    }
            //                    File.Delete(LocalFileName);
            //                    DataFactoryHelper.UpdateReviewJobLog(NewJobId, ReviewId, "Ended", _returnMessage, "ClassifierCommandV2", true, false);
            //                    return;
            //                }
            //                connection.Close();
            //                Task.Run(() => UploadDataAndBuildModelAsync(ReviewId, NewJobId, newModelId));


            //                //if (applyToo == true)
            //                //{
            //                //    DoApplyClassifier(ModelId);
            //                //}
            //            }
            //        }

            //        private async void UploadDataAndBuildModelAsync(int ReviewId, int LogId, int modelId)
            //        {
            //            if (AppIsShuttingDown)
            //            {
            //                DataFactoryHelper.UpdateReviewJobLog(LogId, ReviewId, "Cancelled before upload", "", "TrainingRunCommandV2", true, false);
            //                return;
            //            }

            //            string FolderAndFileName = DataFactoryHelper.NameBase + "ReviewId" + ReviewId.ToString() + "ModelId" + modelId;
            //            string RemoteFolder = "user_models/" + FolderAndFileName + "/";
            //            string RemoteFileName = RemoteFolder + FolderAndFileName + "DataForBuilding.tsv";

            //            bool DataFactoryRes = false;
            //            try
            //            {
            //                DataFactoryHelper.UpdateReviewJobLog(LogId, ReviewId, "Uploading", "", "ClassifierCommandV2");
            //                using (var fileStream = System.IO.File.OpenRead(LocalFileName))
            //                {
            //                    BlobOperations.UploadStream(blobConnection, "eppi-reviewer-data", RemoteFileName, fileStream);
            //                }
            //                File.Delete(LocalFileName);
            //            }
            //            catch (Exception ex)
            //            {
            //                DataFactoryHelper.LogExceptionToFile(ex, ReviewId, LogId, "ClassifierCommandV2");
            //                if (File.Exists(LocalFileName))
            //                {
            //                    try
            //                    {
            //                        File.Delete(LocalFileName);
            //                    }
            //                    catch { }
            //                }
            //                BlobOperations.DeleteIfExists(blobConnection, "eppi-reviewer-data", RemoteFileName);
            //                DataFactoryHelper.UpdateReviewJobLog(LogId, ReviewId, "Failed to upload data", "", "ClassifierCommandV2", true, false);
            //                return;
            //            }
            //            if (AppIsShuttingDown)
            //            {
            //                DataFactoryHelper.UpdateReviewJobLog(LogId, ReviewId, "Cancelled after upload", "", "ClassifierCommandV2", true, false);
            //                return;
            //            }
            //            try
            //            {
            //                DataFactoryHelper DFH = new DataFactoryHelper();
            //                string BatchGuid = Guid.NewGuid().ToString();
            //                VecFile = RemoteFolder + "Vectors.pkl";
            //                ClfFile = RemoteFolder + "Clf.pkl";
            //                Dictionary<string, object> parameters = new Dictionary<string, object>
            //                {
            //                    {"do_build_and_score_log_reg", false },
            //                    {"DataFile", RemoteFileName },
            //                    {"EPPIReviewerApiRunId", BatchGuid},
            //                    {"do_build_log_reg", true},
            //                    {"do_score_log_reg", false},
            //                    //{"ScoresFile", ScoresFile},
            //                    {"VecFile", VecFile},
            //                    {"ClfFile", ClfFile}
            //                };
            //                DataFactoryRes = await DFH.RunDataFactoryProcessV2("EPPI-Reviewer_API", parameters, ReviewId, LogId, "TrainingRunCommandV2", this.CancelToken);
            //            }
            //            catch (Exception ex)
            //            {
            //                DataFactoryHelper.LogExceptionToFile(ex, ReviewId, LogId, "ClassifierCommandV2");
            //                //BlobOperations.DeleteIfExists(blobConnection, "eppi-reviewer-data", RemoteFileName);
            //                DataFactoryHelper.UpdateReviewJobLog(LogId, ReviewId, "Failed to (re)build classifier", "", "ClassifierCommandV2", true, false);
            //                return;
            //            }
            //            //no check for AppIsShuttingDown: these happen inside RunDataFactoryProcessV2
            //            //what happens after this is fast, so no need for checking from here on
            //            if (DataFactoryRes == true)
            //            {
            //                try
            //                {
            //                    double accuracy = 0;
            //                    double precision = 0;
            //                    double recall = 0;
            //                    double auc = 0;
            //                    MemoryStream ms = BlobOperations.DownloadBlobAsMemoryStream(blobConnection, "eppi-reviewer-data", RemoteFolder + "stats.tsv");
            //                    using (StreamReader tsvReader = new StreamReader(ms))
            //                    {
            //                        //csvReader.SetDelimiters(new string[] { "," });
            //                        //csvReader.HasFieldsEnclosedInQuotes = false;
            //                        string line = tsvReader.ReadLine();//headers line!!
            //                        line = tsvReader.ReadLine();//data line!!
            //                        if (line != null)
            //                        {
            //                            string[] data = line.Split('\t');
            //                            accuracy = GetSafeValue(data[0]);
            //                            precision = GetSafeValue(data[1]);
            //                            recall = GetSafeValue(data[2]);
            //                            auc = GetSafeValue(data[3]);
            //                        }
            //                    }
            //                    using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            //                    {
            //                        connection.Open();
            //                        using (SqlCommand command2 = new SqlCommand("st_ClassifierUpdateModel", connection))
            //                        {
            //                            command2.CommandType = System.Data.CommandType.StoredProcedure;

            //                            command2.Parameters.Add(new SqlParameter("@MODEL_ID", modelId));
            //                            command2.Parameters.Add(new SqlParameter("@TITLE", _title));
            //                            command2.Parameters.AddWithValue("@ACCURACY", accuracy);
            //                            command2.Parameters.AddWithValue("@AUC", auc);
            //                            command2.Parameters.AddWithValue("@PRECISION", precision);
            //                            command2.Parameters.AddWithValue("@RECALL", recall);
            //                            command2.Parameters.Add(new SqlParameter("@CHECK_MODEL_ID_EXISTS", 0));
            //                            command2.Parameters["@CHECK_MODEL_ID_EXISTS"].Direction = System.Data.ParameterDirection.Output;
            //                            command2.ExecuteNonQuery();
            //                            //if (Convert.ToInt32(command2.Parameters["@CHECK_MODEL_ID_EXISTS"].Value) == 0)
            //                            //{
            //                            //	DeleteModelAsync();
            //                            //}
            //                        }
            //                        connection.Close();
            //                    }

            //                    DataFactoryHelper.UpdateReviewJobLog(LogId, ReviewId, "Ended", "", "ClassifierCommandV2", true, true);
            //                }
            //                catch (Exception ex)
            //                {
            //                    DataFactoryHelper.LogExceptionToFile(ex, ReviewId, LogId, "ClassifierCommandV2");
            //                    DataFactoryHelper.UpdateReviewJobLog(LogId, ReviewId, "Failed to download data", "", "ClassifierCommandV2", true, false);
            //                }
            //            }
            //            //BlobOperations.DeleteIfExists(blobConnection, "eppi-reviewer-data", RemoteFileName);
            //        }

            //        private void DoApplyClassifier(int modelId)
            //        {
            //            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            //            int ReviewId = ri.ReviewId;
            //            int ContactId = ri.UserId;
            //            int NewJobId = 0;
            //            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            //            {
            //                connection.Open();
            //#if (!CSLA_NETCORE)

            //                LocalFileName = System.Web.HttpRuntime.AppDomainAppPath + TempPath + "ReviewID" + ri.ReviewId + "ContactId" + ri.UserId.ToString() + ".csv";
            //#else
            //                // same as comment above for same line
            //                //SG Edit:
            //                DirectoryInfo tmpDir = System.IO.Directory.CreateDirectory("UserTempUploads");
            //                LocalFileName = tmpDir.FullName + "\\ReviewID" + ReviewId + "ContactId" + ri.UserId.ToString() + ".tsv";
            //#endif
            //                //[SG]: new 27/09/2021: find out the reviewId for this model, as it might be from a different review
            //                //added bonus, ensures the current user has access to this model, I guess.
            //                int ModelReviewId = -1; //will be used later
            //                if (modelId > 0) //no need to check for the general pre-built models which are less than zero...
            //                {
            //                    try
            //                    {
            //                        using (SqlCommand command = new SqlCommand("st_ClassifierContactModels", connection))
            //                        {
            //                            command.CommandType = System.Data.CommandType.StoredProcedure;
            //                            command.Parameters.Add(new SqlParameter("@CONTACT_ID", ri.UserId));
            //                            using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
            //                            {
            //                                while (reader.Read())
            //                                {
            //                                    int tempModelid = reader.GetInt32("MODEL_ID");
            //                                    if (tempModelid == modelId)
            //                                    {
            //                                        //we found it, we can stop after getting the actual ReviewId where this model was built: we need it for the filename of the model in the blob
            //                                        ModelReviewId = reader.GetInt32("REVIEW_ID");
            //                                        break;
            //                                    }
            //                                }
            //                            }
            //                            command.Cancel();
            //                        }
            //                    }
            //                    catch (Exception ex)
            //                    {
            //                        _returnMessage = "Error at ClassifierContactModels:" + ex.Message;
            //                        return;
            //                    }
            //                    if (ModelReviewId == -1)
            //                    {
            //                        _returnMessage = "Error, Model not found";
            //                        //the query above didn't find the current model, so we can't/should not continue...
            //                        return;
            //                    }
            //                }
            //                //end of 27/09/2021 addition


            //                using (SqlCommand command2 = new SqlCommand("st_ClassifierCanRunCheckAndMarkAsStarting", connection))
            //                {//this SP also checks if a Apply (or build) job is running and creates the new job record if not
            //                    command2.CommandType = System.Data.CommandType.StoredProcedure;

            //                    command2.Parameters.Add(new SqlParameter("@MODEL_ID", _classifierId));
            //                    command2.Parameters.Add(new SqlParameter("@REVIEW_ID", ReviewId));
            //                    command2.Parameters.Add(new SqlParameter("@REVIEW_ID_OF_MODEL", ModelReviewId));
            //                    command2.Parameters.Add(new SqlParameter("@CONTACT_ID", ri.UserId));
            //                    command2.Parameters.Add(new SqlParameter("@JobType", "Apply")); //"Apply", "Build" or "ChckS" (for "Check Screening")
            //                    command2.Parameters.Add(new SqlParameter("@NewJobId", 0));
            //                    command2.Parameters["@NewJobId"].Direction = System.Data.ParameterDirection.Output;
            //                    command2.ExecuteNonQuery();
            //                    NewJobId = Convert.ToInt32(command2.Parameters["@NewJobId"].Value);
            //                    if (NewJobId == 0)
            //                    {
            //                        _returnMessage = "Already running";
            //                        return;
            //                    }
            //                    else
            //                    {
            //                        _returnMessage = "Starting...";
            //                    }
            //                }

            //                if (modelId == -5 || modelId == -6 || modelId == -7 || modelId == -8 || modelId == -9)
            //                {// the covid19,  progress-plus using the BERT model, pubmed study types, pubmed study designs (public), via AzureSQL database.
            //                    Task.Run(() => ApplyPreBuiltClassifiersAsync(modelId, _attributeIdClassifyTo, ReviewId, ri.UserId, NewJobId));
            //                    _returnMessage = "The data will be submitted and scored. Please monitor the list of search results for output.";
            //                    return;
            //                }
            //                else if (modelId == -4 || modelId == -3 || modelId == -2 || modelId == -1)
            //                {//older pre-built classifiers RCT (-1), Cochrane RCT(-4), Economic Evaluation (-3), Systematic Review (-2), via AzureSQL database.
            //                    Task.Run(() => ApplyPreBuiltClassifiersAsync(modelId, _attributeIdClassifyTo, ReviewId, ri.UserId, NewJobId));
            //                    _returnMessage = "The data will be submitted and scored. Please monitor the list of search results for output.";
            //                    return;
            //                }
            //                else
            //                {//has to be a positive model ID, so a custom built one
            //                    Task.Run(() => UploadDataAndScoreCustomModelAsync(ReviewId, NewJobId, modelId, ContactId, ModelReviewId));
            //                    _returnMessage = "The data will be submitted and scored. Please monitor the list of search results for output.";
            //                    return;
            //                }
            //            } // end if check for using covid categories / BERT models / SQL database
            //        }
            //        private async void UploadDataAndScoreCustomModelAsync(int ReviewId, int LogId, int modelId, int ContactId, int ModelReviewId)
            //        {
            //            List<Int64> ItemIds = new List<Int64>();
            //            try
            //            {
            //                using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            //                {
            //                    connection.Open();
            //                    using (SqlCommand command = new SqlCommand("st_ClassifierGetClassificationData", connection))// also deletes data from the classification temp table
            //                    {
            //                        command.CommandType = System.Data.CommandType.StoredProcedure;
            //                        command.Parameters.Add(new SqlParameter("@REVIEW_ID", ReviewId));
            //                        command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ID_CLASSIFY_TO", _attributeIdClassifyTo));
            //                        command.Parameters.Add(new SqlParameter("@SOURCE_ID", _sourceId));
            //                        using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
            //                        {
            //                            using (System.IO.StreamWriter file = new System.IO.StreamWriter(LocalFileName, false))
            //                            {
            //                                file.WriteLine("PaperId\tPaperTitle\tAbstract\tIncl");
            //                                while (reader.Read())
            //                                {
            //                                    if (ItemIds.IndexOf(reader.GetInt64("ITEM_ID")) == -1)
            //                                    {
            //                                        ItemIds.Add(reader.GetInt64("ITEM_ID"));
            //                                        file.WriteLine(reader["item_id"].ToString() + "\t" +
            //                                            CleanText(reader, "title") + "\t" +
            //                                            CleanText(reader, "abstract") + "\t" +
            //                                            "99");
            //                                        //file.WriteLine("\"" + reader["item_id"].ToString() + "\"," +
            //                                        //	"\"" + reader["LABEL"].ToString() + "\"," +
            //                                        //	"\"" + CleanText(reader, "title") + "\"," +
            //                                        //	"\"" + CleanText(reader, "abstract") + "\"," +
            //                                        //	"\"" + CleanText(reader, "keywords") + "\"," + "\"" + RevInfo.ReviewId.ToString() + "\"");
            //                                    }
            //                                }
            //                            }
            //                            command.Cancel();
            //                        }
            //                    }
            //                }
            //            }
            //            catch (Exception ex)
            //            {
            //                DataFactoryHelper.UpdateReviewJobLog(LogId, ReviewId, "Failed to get data to score", "", "ClassifierCommandV2", true, false);
            //                DataFactoryHelper.LogExceptionToFile(ex, ReviewId, LogId, "ClassifierCommandV2");
            //                return;
            //            }
            //            if (AppIsShuttingDown)
            //            {
            //                DataFactoryHelper.UpdateReviewJobLog(LogId, ReviewId, "Cancelled before upload", "", "ClassifierCommandV2", true, false);
            //                return;
            //            }

            //            string FolderAndFileName = DataFactoryHelper.NameBase + "ReviewId" + ModelReviewId.ToString() + "ModelId" + modelId;
            //            string RemoteFolder = "user_models/" + FolderAndFileName + "/";
            //            string RemoteFileName = RemoteFolder + FolderAndFileName + "DataForScoring.tsv";
            //            bool DataFactoryRes = false;
            //            // upload data to blob
            //            try
            //            {
            //                using (var fileStream = System.IO.File.OpenRead(LocalFileName))
            //                {
            //                    BlobOperations.UploadStream(blobConnection,
            //                        "eppi-reviewer-data"
            //                        , RemoteFileName
            //                        , fileStream);
            //                }
            //                File.Delete(LocalFileName);
            //            }
            //            catch (Exception ex)
            //            {
            //                DataFactoryHelper.UpdateReviewJobLog(LogId, ReviewId, "Failed to uplodad data to score", "", "ClassifierCommandV2", true, false);
            //                DataFactoryHelper.LogExceptionToFile(ex, ReviewId, LogId, "ClassifierCommandV2");
            //                return;
            //            }


            //            DataFactoryHelper DFH = new DataFactoryHelper();
            //            string BatchGuid = Guid.NewGuid().ToString();
            //            VecFile = RemoteFolder + "Vectors.pkl";
            //            ClfFile = RemoteFolder + "Clf.pkl";
            //            ScoresFile = RemoteFolder + "ScoresForReview" + ReviewId + ".tsv";
            //            Dictionary<string, object> parameters = new Dictionary<string, object>
            //                {
            //                    {"do_build_and_score_log_reg", false },
            //                    {"DataFile", RemoteFileName },
            //                    {"EPPIReviewerApiRunId", BatchGuid},
            //                    {"do_build_log_reg", false},
            //                    {"do_score_log_reg", true},
            //                    {"ScoresFile", ScoresFile},
            //                    {"VecFile", VecFile},
            //                    {"ClfFile", ClfFile}
            //                };

            //            if (AppIsShuttingDown)
            //            {
            //                DataFactoryHelper.UpdateReviewJobLog(LogId, ReviewId, "Cancelled after upload", "", "ClassifierCommandV2", true, false);
            //                return;
            //            }
            //            try
            //            {
            //                DataFactoryRes = await DFH.RunDataFactoryProcessV2("EPPI-Reviewer_API", parameters, ReviewId, LogId, "ClassifierCommandV2", this.CancelToken);

            //            }
            //            catch (Exception ex)
            //            {
            //                DataFactoryHelper.UpdateReviewJobLog(LogId, ReviewId, "Failed to run DF", "", "ClassifierCommandV2", true, false);
            //                DataFactoryHelper.LogExceptionToFile(ex, ReviewId, LogId, "ClassifierCommandV2");
            //                return;
            //            }
            //            DataTable Scores = new DataTable();
            //            if (DataFactoryRes == true)
            //            {
            //                try
            //                {

            //                    Scores = DownloadResults("eppi-reviewer-data", ScoresFile);
            //                    if (AppIsShuttingDown)
            //                    {
            //                        DataFactoryHelper.UpdateReviewJobLog(LogId, ReviewId, "Cancelled after DF", "", "ClassifierCommandV2", true, false);
            //                        return;
            //                    }
            //                    LoadResultsIntoDatabase(Scores, ContactId);
            //                }
            //                catch (Exception ex)
            //                {
            //                    DataFactoryHelper.UpdateReviewJobLog(LogId, ReviewId, "Failed after DF", "", "ClassifierCommandV2", true, false);
            //                    DataFactoryHelper.LogExceptionToFile(ex, ReviewId, LogId, "ClassifierCommandV2");
            //                    return;
            //                }
            //            }
            //            try
            //            {
            //                BlobOperations.DeleteIfExists(blobConnection, "eppi-reviewer-data", RemoteFileName);
            //                BlobOperations.DeleteIfExists(blobConnection, "eppi-reviewer-data", ScoresFile);
            //                if (DataFactoryRes == true) DataFactoryHelper.UpdateReviewJobLog(LogId, ReviewId, "Ended", "", "ClassifierCommandV2", true, true);
            //            }
            //            catch (Exception ex)
            //            {
            //                DataFactoryHelper.UpdateReviewJobLog(LogId, ReviewId, "Failed deleting remote files", "", "ClassifierCommandV2", true, false);
            //                DataFactoryHelper.LogExceptionToFile(ex, ReviewId, LogId, "ClassifierCommandV2");
            //                return;
            //            }


            //            //string DataFile = @"attributemodeldata/" + TrainingRunCommand.NameBase + "ReviewId" + RevInfo.ReviewId.ToString() + "ModelId" + ModelIdForScoring(modelId) + "ToScore.csv";
            //            //string ModelFile = @"attributemodels/" + (modelId > 0 ? TrainingRunCommand.NameBase : "") + ReviewIdForScoring(modelId, ModelReviewId.ToString()) + ".csv";
            //            //string ResultsFile1 = @"attributemodels/" + TrainingRunCommand.NameBase + "ReviewId" + RevInfo.ReviewId.ToString() + "ModelId" + ModelIdForScoring(modelId) + "Scores.csv";
            //            //string ResultsFile2 = "";
            //            //if (modelId == -4)
            //            //{
            //            //    ResultsFile1 = "attributemodels/" + TrainingRunCommand.NameBase + "ReviewId" + RevInfo.ReviewId.ToString() + "ModelId" + ModelIdForScoring(modelId) + "RCTScores.csv";
            //            //    ResultsFile2 = @"attributemodels/" + TrainingRunCommand.NameBase + "ReviewId" + RevInfo.ReviewId.ToString() + "ModelId" + ModelIdForScoring(modelId) + "NonRCTScores.csv";
            //            //}
            //            //await InvokeBatchExecutionService(RevInfo.ReviewId.ToString(), "ScoreModel", modelId, DataFile, ModelFile, ResultsFile1, ResultsFile2);

            //            //if (modelId == -4) // new RCT model = two searches to create, one for the RCTs, one for the non-RCTs
            //            //{
            //            //    // load RCTs
            //            //    DataTable RCTs = DownloadResults( "attributemodels", TrainingRunCommand.NameBase + "ReviewId" + RevInfo.ReviewId.ToString() + "ModelId" + ModelIdForScoring(modelId) + "RCTScores.csv");
            //            //    _title = "Cochrane RCT Classifier: may be RCTs";
            //            //    LoadResultsIntoDatabase(RCTs, connection, ri);

            //            //    // load non-RCTs
            //            //    DataTable nRCTs = DownloadResults( "attributemodels", TrainingRunCommand.NameBase + "ReviewId" + RevInfo.ReviewId.ToString() + "ModelId" + ModelIdForScoring(modelId) + "NonRCTScores.csv");
            //            //    _title = "Cochrane RCT Classifier: unlikely to be RCTs";
            //            //    LoadResultsIntoDatabase(nRCTs, connection, ri);
            //            //}
            //            //else
            //            //{
            //            //    DataTable Scores = DownloadResults( "attributemodels", TrainingRunCommand.NameBase + "ReviewId" + RevInfo.ReviewId.ToString() + "ModelId" + ModelIdForScoring(modelId) + "Scores.csv");
            //            //    LoadResultsIntoDatabase(Scores, connection, ri);
            //            //}
            //            //connection.Close();
            //        }
            //        // does both priority screening simulation and check screening

            //        private void DoApplyCheckOrPriorityScreening(string CheckOrPriority)
            //        {
            //            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            //            {
            //                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            //                int NewJobId = 0;
            //                int ReviewId = ri.ReviewId;
            //                List<Int64> ItemIds = new List<Int64>();
            //                int positiveClassCount = 0;
            //                int negativeClasscount = 0;
            //                int sampleSize = 0;

            //                Guid thisGuid = Guid.NewGuid();

            //                connection.Open();

            //                // ************************************ Check if job of this kind is running, and GET A NewJobId if not, fail otherwise***************************************
            //                using (SqlCommand command2 = new SqlCommand("st_ClassifierCanRunCheckAndMarkAsStarting", connection))
            //                {//this SP also checks if a Apply (or build) job is running and creates the new job record if not
            //                    command2.CommandType = System.Data.CommandType.StoredProcedure;

            //                    command2.Parameters.Add(new SqlParameter("@MODEL_ID", SqlDbType.Int ));
            //                    command2.Parameters["@MODEL_ID"].Value = 0;
            //                    command2.Parameters.Add(new SqlParameter("@REVIEW_ID", ReviewId));
            //                    command2.Parameters.Add(new SqlParameter("@REVIEW_ID_OF_MODEL", SqlDbType.Int));
            //                    command2.Parameters["@REVIEW_ID_OF_MODEL"].Value = 0;
            //                    command2.Parameters.Add(new SqlParameter("@CONTACT_ID", ri.UserId));
            //                    command2.Parameters.Add(new SqlParameter("@JobType", CheckOrPriority)); //"Apply", "Build" or "ChckS" (for "Check Screening") "PrioS" (for "priority screening simulation)
            //                    command2.Parameters.Add(new SqlParameter("@NewJobId", 0));
            //                    command2.Parameters["@NewJobId"].Direction = System.Data.ParameterDirection.Output;
            //                    command2.ExecuteNonQuery();
            //                    NewJobId = Convert.ToInt32(command2.Parameters["@NewJobId"].Value);
            //                    if (NewJobId == 0)
            //                    {
            //                        _returnMessage = "Already running";
            //                        return;
            //                    }
            //                    else
            //                    {
            //                        _returnMessage = "Starting...";
            //                    }
            //                }

            //                DirectoryInfo tmpDir = System.IO.Directory.CreateDirectory("UserTempUploads");
            //                LocalFileName = tmpDir.FullName + "\\ReviewID" + ri.ReviewId + "ContactId" + ri.UserId.ToString() + "_" + thisGuid.ToString() + ".tsv";

            //                using (SqlCommand command = new SqlCommand("st_ClassifierGetTrainingData", connection))
            //                {
            //                    command.CommandType = System.Data.CommandType.StoredProcedure;
            //                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
            //                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ID_ON", _attributeIdOn));
            //                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ID_NOT_ON", _attributeIdNotOn));
            //                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
            //                    {
            //                        using (System.IO.StreamWriter file = new System.IO.StreamWriter(LocalFileName, false))
            //                        {
            //                            file.WriteLine("PaperId\tPaperTitle\tAbstract\tIncl");
            //                            while (reader.Read())
            //                            {
            //                                if (ItemIds.IndexOf(reader.GetInt64("ITEM_ID")) == -1)
            //                                {
            //                                    sampleSize++;
            //                                    ItemIds.Add(reader.GetInt64("ITEM_ID"));
            //                                    file.WriteLine(reader["item_id"].ToString() + "\t" +
            //                                        CleanText(reader, "title") + "\t" +
            //                                        CleanText(reader, "abstract") + "\t" +
            //                                        CleanText(reader, "LABEL"));
            //                                    if (reader["LABEL"].ToString() == "1")
            //                                        positiveClassCount++;
            //                                    else
            //                                        negativeClasscount++;
            //                                }
            //                            }
            //                        }
            //                    }
            //                }
            //                if (positiveClassCount < 7 || negativeClasscount < 7 || sampleSize < 20)//at least 7 examples in each class and at least 20 records in total
            //                {
            //                    _returnMessage = "Insufficient data";
            //                    File.Delete(LocalFileName);
            //                    DataFactoryHelper.UpdateReviewJobLog(NewJobId, ReviewId, "Ended", _returnMessage, "ClassifierCommandV2", true, false);
            //                    return;
            //                }
            //                connection.Close();
            //                Task.Run(() => UploadDataAndCheckOrPriorityScreeningSimulationAsync(ReviewId, ri.UserId, thisGuid, NewJobId, CheckOrPriority));
            //            }
            //        }

            //        // does both priority screening simulation and check screening
            //        private async void UploadDataAndCheckOrPriorityScreeningSimulationAsync(int ReviewId, int ContactId, Guid thisGuid, int LogId, string CheckOrPriority)
            //        {
            //            if (AppIsShuttingDown)
            //            {
            //                DataFactoryHelper.UpdateReviewJobLog(LogId, ReviewId, "Cancelled before upload", "", "ClassifierCommandV2", true, false);
            //                return;
            //            }
            //            string FolderAndFileName = "";
            //            string RemoteFolder = "";
            //            string RemoteFileName = "";

            //            if ((CheckOrPriority == "PrioS"))
            //            {
            //                FolderAndFileName = DataFactoryHelper.NameBase + "ReviewId" + ReviewId.ToString();
            //                RemoteFolder = "priority_screening_simulation/" + FolderAndFileName + "/";
            //                RemoteFileName = RemoteFolder + "PriorityScreeningSimulationData_" + thisGuid.ToString() + ".tsv";
            //                ScoresFile = RemoteFolder + _title.Replace("¬¬PriorityScreening¬¬", "") + ".tsv" ;
            //            }
            //            else
            //            {
            //                FolderAndFileName = DataFactoryHelper.NameBase + "ReviewId" + ReviewId.ToString() + "ContactId" + ContactId.ToString() + "_" + thisGuid.ToString();
            //                RemoteFolder = "check_screening /" + FolderAndFileName + "/";
            //                RemoteFileName = RemoteFolder + "ScreeningCheckData.tsv";
            //                ScoresFile = RemoteFolder + "ScreeningCheckScores.tsv";
            //            }

            //            bool DataFactoryRes = false;
            //            try
            //            {
            //                DataFactoryHelper.UpdateReviewJobLog(LogId, ReviewId, "Uploading", "", "ClassifierCommandV2");

            //                using (var fileStream = System.IO.File.OpenRead(LocalFileName))
            //                {
            //                    BlobOperations.UploadStream(blobConnection, "eppi-reviewer-data", RemoteFileName, fileStream);
            //                }
            //                File.Delete(LocalFileName);
            //            }
            //            catch (Exception ex)
            //            {
            //                DataFactoryHelper.LogExceptionToFile(ex, ReviewId, LogId, "ClassifierCommandV2");
            //                if (File.Exists(LocalFileName))
            //                {
            //                    try
            //                    {
            //                        File.Delete(LocalFileName);
            //                    }
            //                    catch { }
            //                }
            //                BlobOperations.DeleteIfExists(blobConnection, "eppi-reviewer-data", RemoteFileName);
            //                DataFactoryHelper.UpdateReviewJobLog(LogId, ReviewId, "Failed to upload data", ex.Message, "ClassifierCommandV2", true, false);
            //                return;
            //            }
            //            if (AppIsShuttingDown)
            //            {
            //                DataFactoryHelper.UpdateReviewJobLog(LogId, ReviewId, "Cancelled after upload", "", "ClassifierCommandV2", true, false);
            //                return;
            //            }
            //            try
            //            {
            //                DataFactoryHelper DFH = new DataFactoryHelper();
            //                string endpoint = (CheckOrPriority == "PrioS" ? "do_priority_screening_simulation" : "do_check_screening");
            //                Dictionary<string, object> parameters = new Dictionary<string, object>
            //                {
            //                    {endpoint, true },
            //                    {"DataFile", RemoteFileName },
            //                    {"EPPIReviewerApiRunId", thisGuid.ToString()},
            //                    {"ScoresFile", ScoresFile},
            //                };

            //                DataFactoryRes = await DFH.RunDataFactoryProcessV2("EPPI-Reviewer_API", parameters, ReviewId, LogId, "ClassifierCommandV2", this.CancelToken);
            //            }
            //            catch (Exception ex)
            //            {
            //                DataFactoryHelper.LogExceptionToFile(ex, ReviewId, LogId, "ClassifierCommandV2");
            //                //BlobOperations.DeleteIfExists(blobConnection, "eppi-reviewer-data", RemoteFileName); -- this was already commented out?
            //                DataFactoryHelper.UpdateReviewJobLog(LogId, ReviewId, "Failed to (re)build classifier", "", "ClassifierCommandV2", true, false);
            //                return;
            //            }

            //            if (CheckOrPriority == "PrioS")
            //            {
            //                DataFactoryHelper.UpdateReviewJobLog(LogId, ReviewId, "Ended", "", "ClassifierCommandV2", true, true);
            //                _returnMessage = "success";
            //                return; // we're done!
            //            }


            //            DataTable Scores = new DataTable();
            //            if (DataFactoryRes == true)
            //            {
            //                try
            //                {

            //                    Scores = DownloadResults("eppi-reviewer-data", ScoresFile);
            //                    if (AppIsShuttingDown)
            //                    {
            //                        // ************************************** log id **********************************************************
            //                        DataFactoryHelper.UpdateReviewJobLog(LogId, ReviewId, "Cancelled after DF", "", "ClassifierCommandV2", true, false);
            //                        return;
            //                    }
            //                    LoadResultsIntoDatabase(Scores, ContactId);
            //                }
            //                catch (Exception ex)
            //                {
            //                    DataFactoryHelper.UpdateReviewJobLog(LogId, ReviewId, "Failed after DF", "", "ClassifierCommandV2", true, false);
            //                    DataFactoryHelper.LogExceptionToFile(ex, ReviewId, LogId, "ClassifierCommandV2");
            //                    return;
            //                }
            //            }
            //            try
            //            {
            //                BlobOperations.DeleteIfExists(blobConnection, "eppi-reviewer-data", RemoteFileName);
            //                BlobOperations.DeleteIfExists(blobConnection, "eppi-reviewer-data", ScoresFile);
            //                if (DataFactoryRes == true) DataFactoryHelper.UpdateReviewJobLog(LogId, ReviewId, "Ended", "", "ClassifierCommandV2", true, true);
            //            }
            //            catch (Exception ex)
            //            {
            //                DataFactoryHelper.UpdateReviewJobLog(LogId, ReviewId, "Failed deleting remote files", "", "ClassifierCommandV2", true, false);
            //                DataFactoryHelper.LogExceptionToFile(ex, ReviewId, LogId, "ClassifierCommandV2");
            //                return;
            //            }
            //        }

            //        /// <summary>
            //        /// Uses AzureSQL instance to ship data to and from the ML workspace
            //        /// </summary>
            //        /// <param name="modelId"></param>
            //        /// <param name="ApplyToAttributeId"></param>
            //        /// <param name="ReviewId"></param>
            //        /// <param name="ContactId"></param>
            //        /// <param name="LogId"></param>
            //        /// <returns></returns>
            //        private async Task ApplyPreBuiltClassifiersAsync(int modelId, Int64 ApplyToAttributeId, int ReviewId, int ContactId, int LogId)
            //        {
            //            string BatchGuid = Guid.NewGuid().ToString();
            //            int rowcount = 0;
            //            try
            //            {
            //                using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            //                {
            //                    connection.Open();
            //                    using (SqlCommand command = new SqlCommand("st_ClassifierGetClassificationDataToSQL", connection))
            //                    {
            //                        command.CommandTimeout = 6000; // 10 minutes - if there are tens of thousands of items it can take a while
            //                        command.CommandType = System.Data.CommandType.StoredProcedure;
            //                        command.Parameters.Add(new SqlParameter("@REVIEW_ID", ReviewId));
            //                        command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ID_CLASSIFY_TO", ApplyToAttributeId));
            //                        command.Parameters.Add(new SqlParameter("@ITEM_ID_LIST", ""));
            //                        command.Parameters.Add(new SqlParameter("@SOURCE_ID", _sourceId));
            //                        command.Parameters.Add(new SqlParameter("@BatchGuid", BatchGuid));
            //                        command.Parameters.Add(new SqlParameter("@ContactId", ContactId));
            //                        command.Parameters.Add(new SqlParameter("@MachineName", TrainingRunCommand.NameBase));
            //                        command.Parameters.Add(new SqlParameter("@ROWCOUNT", 0));
            //                        command.Parameters["@ROWCOUNT"].Direction = System.Data.ParameterDirection.Output;
            //                        command.ExecuteNonQuery();
            //                        rowcount = Convert.ToInt32(command.Parameters["@ROWCOUNT"].Value);
            //                    }
            //                }
            //                if (rowcount == 0)
            //                {
            //                    DataFactoryHelper.UpdateReviewJobLog(LogId, ReviewId, "Failed: no data to score", "", "ClassifierCommandV2", true, false);
            //                    return;
            //                }
            //            }
            //            catch (Exception ex)
            //            {
            //                DataFactoryHelper.UpdateReviewJobLog(LogId, ReviewId, "Failed to get data to score", "", "ClassifierCommandV2", true, false);
            //                DataFactoryHelper.LogExceptionToFile(ex, ReviewId, LogId, "ClassifierCommandV2");
            //                return;
            //            }
            //            if (AppIsShuttingDown)
            //            {
            //                DataFactoryHelper.UpdateReviewJobLog(LogId, ReviewId, "Cancelled after uploading data", "", "ClassifierCommandV2", true, false);
            //                return;
            //            }
            //            string tenantID = AzureSettings.tenantID;
            //            string appClientId = AzureSettings.appClientId;
            //            string appClientSecret = AzureSettings.appClientSecret;
            //            string subscriptionId = AzureSettings.subscriptionId;
            //            string resourceGroup = AzureSettings.resourceGroup;
            //            string dataFactoryName = AzureSettings.dataFactoryName;

            //            string covidClassifierPipelineName = AzureSettings.covidClassifierPipelineName;
            //            string covidLongCovidPipelineName = AzureSettings.covidLongCovidPipelineName;
            //            string progressPlusPipelineName = AzureSettings.progressPlusPipelineName;
            //            string pubMedStudyTypesPipelineName = AzureSettings.pubMedStudyTypesPipelineName;
            //            string pubMedStudyDesignsPipelineName = AzureSettings.pubMedStudyDesignsPipelineName;

            //            string ClassifierPipelineName = "";
            //            string SearchTitle = "";
            //            Dictionary<string, object> parameters = new Dictionary<string, object>();
            //            switch (modelId)
            //            {
            //                case -1:
            //                    ClassifierPipelineName = "EPPI-Reviewer_API";
            //                    SearchTitle = "Items classified according to model: " + _title;
            //                    parameters.Add("EPPIReviewerApiRunId", BatchGuid);
            //                    parameters.Add("do_original_rct_classifier", true);
            //                    break;
            //                case -2:
            //                    ClassifierPipelineName = "EPPI-Reviewer_API";
            //                    SearchTitle = "Items classified according to model: " + _title;
            //                    parameters.Add("EPPIReviewerApiRunId", BatchGuid);
            //                    parameters.Add("do_systematic_reviews_classifier", true);
            //                    break;
            //                case -3:
            //                    ClassifierPipelineName = "EPPI-Reviewer_API";
            //                    SearchTitle = "Items classified according to model: " + _title;
            //                    parameters.Add("EPPIReviewerApiRunId", BatchGuid);
            //                    parameters.Add("do_economic_eval_classifier", true);
            //                    break;
            //                case -4:
            //                    ClassifierPipelineName = "EPPI-Reviewer_API";
            //                    SearchTitle = "Items classified according to model: " + _title;
            //                    parameters.Add("EPPIReviewerApiRunId", BatchGuid);
            //                    parameters.Add("do_cochrane_rct_classifier", true);
            //                    break;
            //                case -5:
            //                    ClassifierPipelineName = covidClassifierPipelineName;
            //                    SearchTitle = "COVID-19 map category: ";
            //                    parameters.Add("BatchGuid", BatchGuid);
            //                    break;
            //                case -6:
            //                    ClassifierPipelineName = covidLongCovidPipelineName;
            //                    SearchTitle = "Long COVID model: ";
            //                    parameters.Add("BatchGuid", BatchGuid);
            //                    break;
            //                case -7:
            //                    ClassifierPipelineName = progressPlusPipelineName;
            //                    SearchTitle = "PROGRESS-Plus model: ";
            //                    parameters.Add("BatchGuid", BatchGuid);
            //                    break;
            //                case -8:
            //                    ClassifierPipelineName = pubMedStudyTypesPipelineName;
            //                    SearchTitle = "PubMed study type model: ";
            //                    parameters.Add("BatchGuid", BatchGuid);
            //                    break;
            //                case -9:
            //                    ClassifierPipelineName = pubMedStudyDesignsPipelineName;
            //                    SearchTitle = "PubMed study designs model: ";
            //                    parameters.Add("BatchGuid", BatchGuid);
            //                    break;
            //                default:
            //                    return;
            //            }
            //            bool DataFactoryRes = false;
            //            DataFactoryHelper DFH = new DataFactoryHelper();
            //            try
            //            {
            //                DataFactoryRes = await DFH.RunDataFactoryProcessV2(ClassifierPipelineName, parameters, ReviewId, LogId, "ClassifierCommandV2", this.CancelToken);
            //            }
            //            catch (Exception ex)
            //            {
            //                DataFactoryHelper.UpdateReviewJobLog(LogId, ReviewId, "Failed during DF", "", "ClassifierCommandV2", true, false);
            //                DataFactoryHelper.LogExceptionToFile(ex, ReviewId, LogId, "ClassifierCommandV2");
            //            }
            //            if (DataFactoryRes == true)
            //            {
            //                try
            //                {
            //                    using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            //                    {
            //                        connection.Open();
            //                        using (SqlCommand command = new SqlCommand("st_ClassifierInsertSearchAndScores", connection))
            //                        {
            //                            command.CommandType = System.Data.CommandType.StoredProcedure;
            //                            command.CommandTimeout = 300; // 5 mins to be safe. I've seen queries with large numbers of searches / items take about 30 seconds, which times out live
            //                            command.Parameters.Add(new SqlParameter("@BatchGuid", BatchGuid));
            //                            command.Parameters.Add(new SqlParameter("@REVIEW_ID", ReviewId));
            //                            command.Parameters.Add(new SqlParameter("@CONTACT_ID", ContactId));
            //                            command.Parameters.Add(new SqlParameter("@SearchTitle", SearchTitle));
            //                            command.ExecuteNonQuery();
            //                        }
            //                    }
            //                    DataFactoryHelper.UpdateReviewJobLog(LogId, ReviewId, "Ended", "", "ClassifierCommandV2", true, true);
            //                }
            //                catch (Exception ex)
            //                {
            //                    DataFactoryHelper.UpdateReviewJobLog(LogId, ReviewId, "Failed after DF", "", "ClassifierCommandV2", true, false);
            //                    DataFactoryHelper.LogExceptionToFile(ex, ReviewId, LogId, "ClassifierCommandV2");
            //                }
            //            }
            //        }

            //        private DataTable DownloadResults(string container, string filename)
            //        {
            //            MemoryStream ms = BlobOperations.DownloadBlobAsMemoryStream(blobConnection, container, filename);

            //            DataTable dt = new DataTable("Scores");
            //            dt.Columns.Add("SCORE", System.Type.GetType("System.Decimal"));
            //            dt.Columns.Add("ITEM_ID");
            //            dt.Columns.Add("REVIEW_ID");

            //#if (!CSLA_NETCORE)

            //			using (TextFieldParser csvReader = new TextFieldParser(ms))
            //			{
            //				csvReader.SetDelimiters(new string[] { "\t" });
            //				csvReader.HasFieldsEnclosedInQuotes = false;
            //				while (!csvReader.EndOfData)
            //				{
            //					string[] data = csvReader.ReadFields();
            //					if (data.Length == 3)
            //					{
            //						if (data[0] == "1")
            //						{
            //							data[0] = "0.999999";
            //						}
            //						else if (data[0] == "0")
            //						{
            //							data[0] = "0.000001";
            //						}
            //						else if (data[0].Length > 2 && data[0].Contains("E"))
            //						{
            //							double dbl = 0;
            //							double.TryParse(data[0], out dbl);
            //							//if (dbl == 0.0) throw new Exception("Gotcha!");
            //							data[0] = dbl.ToString("F10");
            //						}
            //						dt.Rows.Add(data);
            //					}

            //					//var data1 = csvReader.ReadFields();
            //					//for (var i = 0; i < data1.Length; i++)
            //					//{
            //					//    if (data1[i] == "")
            //					//    {
            //					//        data1[i] = null;
            //					//    }
            //					//}
            //					//dt.Rows.Add(data);
            //				}
            //			}

            //#else
            //            using (StreamReader tsvReader = new StreamReader(ms))
            //            {
            //                //csvReader.SetDelimiters(new string[] { "," });
            //                //csvReader.HasFieldsEnclosedInQuotes = false;
            //                string line = tsvReader.ReadLine();//headers line!!
            //                while ((line = tsvReader.ReadLine()) != null)
            //                {
            //                    string[] data = line.Split('\t');
            //                    dt.Rows.Add(GetSafeValue(data[4]), long.Parse(data[0]), RevInfo.ReviewId);
            //                }
            //            }
            //#endif
            //            return dt;
            //        }

            //        private void LoadResultsIntoDatabase(DataTable dt, int ContactId)
            //        {
            //            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            //            {
            //                connection.Open();
            //                using (SqlBulkCopy sbc = new SqlBulkCopy(connection))
            //                {
            //                    sbc.DestinationTableName = "TB_CLASSIFIER_ITEM_TEMP";
            //                    sbc.ColumnMappings.Clear();
            //                    sbc.ColumnMappings.Add("SCORE", "SCORE");
            //                    sbc.ColumnMappings.Add("ITEM_ID", "ITEM_ID");
            //                    sbc.ColumnMappings.Add("REVIEW_ID", "REVIEW_ID");
            //                    sbc.BatchSize = 1000;
            //                    sbc.WriteToServer(dt);
            //                }
            //                string SearchTitle = "Items classified according to model: " + _title;
            //                if (_title.Contains("¬¬CheckScreening"))
            //                {
            //                    SearchTitle = "Screening check results ordered with those mostly likely relevant at the top";
            //                }
            //                // Create a new search to 'hold' the results
            //                //int searchId = 0;
            //                using (SqlCommand command = new SqlCommand("st_ClassifierCreateSearchList", connection))
            //                {
            //                    command.CommandType = System.Data.CommandType.StoredProcedure;
            //                    command.CommandTimeout = 300;
            //                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", RevInfo.ReviewId));
            //                    command.Parameters.Add(new SqlParameter("@CONTACT_ID", ContactId));
            //                    command.Parameters.Add(new SqlParameter("@SEARCH_TITLE", SearchTitle));
            //                    command.Parameters.Add(new SqlParameter("@HITS_NO", dt.Rows.Count));
            //                    command.Parameters.Add(new SqlParameter("@NEW_SEARCH_ID", 0));
            //                    command.Parameters["@NEW_SEARCH_ID"].Direction = System.Data.ParameterDirection.Output;
            //                    command.ExecuteNonQuery();
            //                    //searchId = Convert.ToInt32(command.Parameters["@NEW_SEARCH_ID"].Value); not sure we need this any more
            //                }
            //            }
            //        }

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
            //SG Feb 2025 removed try...catch here as it's OK to let the controller do the catch and log
            List<BlobInHierarchy> list = BlobOperations.Blobfilenames(blobConnection, "eppi-reviewer-data", RemoteFolder);
            foreach (var toDel in list)
            {
                Console.WriteLine("Will delete " + toDel.BlobName);
                BlobOperations.DeleteIfExists(blobConnection, "eppi-reviewer-data", toDel.BlobName);
            }
            
        }

        private double GetSafeValue(string data)
        {

            if (data == "1" || data == "1.0" || data == "1.000000000000000000")
            {
                data = "0.99999999";
            }
            else if (data == "0" || data == "0.000000000000000000")
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
            double res = Convert.ToDouble(data);
            if (res < 0.00000001) res = 0.00000001;
            else if (res > 0.99999999) res = 0.99999999;
            return res;
        }
        public static void WriteDataLineInFileToUpload(string ItemIdSt, string Title, string Abstract, string Label, System.IO.StreamWriter file)
        {
            //int MaxLineLength = 15000;//9000;
            //if (ItemIdSt.Length + Title.Length + Abstract.Length + Label.Length + 6 > MaxLineLength)
            //{
            //    //Accoding to in-house testing Azure file parser can't handle files with one or more lines longer than ~16000 chars,
            //    //so we'll truncate the Abstract, when needed
            //    int offset = ItemIdSt.Length + Title.Length + Label.Length + 6;
            //    int maxAbstractLen = MaxLineLength - offset;
            //    Abstract = Abstract.Substring(0, maxAbstractLen);
            //    int lastSpaceIndex = Abstract.LastIndexOf(" ");
            //    if (lastSpaceIndex != -1)
            //    {
            //        Abstract = Abstract.Substring(0, lastSpaceIndex);
            //    }
            //}
            file.WriteLine(ItemIdSt + "\t" +
                Title + "\t" +
                Abstract + "\t" +
                Label);
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
