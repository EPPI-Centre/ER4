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
using Newtonsoft.Json;
//using DeployR;


#if !SILVERLIGHT
using System.Data.SqlClient;
using BusinessLibrary.Data;
using BusinessLibrary.Security;
using System.IO;
using System.Xml;

using System.Diagnostics;
using System.Globalization;
using System.Net.Http;

using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Threading.Tasks;
//using Microsoft.Azure;
using System.Configuration;
using System.Text.RegularExpressions;
using System.Data;
using System.Reflection.Emit;


#endif



#if (!SILVERLIGHT && !CSLA_NETCORE)
using Microsoft.VisualBasic.FileIO;
#endif

namespace BusinessLibrary.BusinessClasses
{

    [Serializable]
    public class TrainingRunCommandV2 : LongLastingFireAndForgetCommand<TrainingRunCommandV2>
    {

    public TrainingRunCommandV2(){}


        private long _TriggeringItemId;
        private bool _included;

        private int _currentTrainingId;

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

        public static readonly PropertyInfo<string> ParametersProperty = RegisterProperty<string>(new PropertyInfo<string>("Parameters", "Parameters"));
        public string Parameters
        {
            get { return ReadProperty(ParametersProperty); }
            set { LoadProperty(ParametersProperty, value); }
        }

        public static readonly PropertyInfo<string> SimulationResultsProperty = RegisterProperty<string>(new PropertyInfo<string>("SimulationResults", "SimulationResults"));
        public string SimulationResults
        {
            get { return ReadProperty(SimulationResultsProperty); }
            set { LoadProperty(SimulationResultsProperty, value); }
        }
        /// <summary>
        /// Used only by st_ScreeningCreateNonMLList to figure whether the random list really needs to be re-shuffled or not
        /// </summary>
        public long TriggeringItemId
        {
            get
            {
                return _TriggeringItemId;
            }
            set { _TriggeringItemId = value; }
        }

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_TriggeringItemId", _TriggeringItemId);
            info.AddValue("_included", _included);
        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            _TriggeringItemId = info.GetValue<long>("_TriggeringItemId");
            _included = info.GetValue<bool>("_included");
        }


#if !SILVERLIGHT

        protected override void DataPortal_Execute() // can I override and make it async?? looks like it works
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            if (Parameters == "DoSimulation")
            {
                //DoSimulation(ri.ReviewId, ri.UserId);
                return;
            }
            if (Parameters == "FetchSimulationResults")
            {
                //FetchSimulationResults(ri.ReviewId);
                return;
            }
            
            if (RevInfo.ScreeningMode == "Random") // || RevInfo.ScreeningWhatAttributeId > 0)
            {
                CreateNonMLLIst(ri.ReviewId, ri.UserId);
                ReportBack = "Done (Random List)";
                return;
            }
            if (ri.ReviewId == 21823) return;
            int NewJobId = 0;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                // ************* STAGE 1: check that we have sufficient data to run the ML
                // ************* a minimum of 5 includes and 5 excludes is currently looked for

                //ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                int ReviewID = ri.ReviewId;
                int n_includes = 0;
                int n_excludes = 0;
                using (SqlCommand command = new SqlCommand("st_TrainingCheckData", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ReviewID));
                    command.Parameters.Add(new SqlParameter("@N_INCLUDES", 0));
                    command.Parameters.Add(new SqlParameter("@N_EXCLUDES", 0));
                    command.Parameters["@N_INCLUDES"].Direction = System.Data.ParameterDirection.Output;
                    command.Parameters["@N_EXCLUDES"].Direction = System.Data.ParameterDirection.Output;
                    command.ExecuteNonQuery();
                    n_includes = Convert.ToInt32(command.Parameters["@N_INCLUDES"].Value);
                    n_excludes = Convert.ToInt32(command.Parameters["@N_EXCLUDES"].Value);
                }

                // if we don't have enough data, we default to creating a non-ML list
                if (n_includes < 6 || n_excludes < 6)
                {
                    //RevInfo.ScreeningMode = "Random";
                    CreateNonMLLIst(ri.ReviewId, ri.UserId);
                    ReportBack = "Done (Random List)";
                    return;
                }


                //OK, so we do want to do the training, but is a training round already running?
                //this produces a new line in the log table and in TB_TRAINING
                using (SqlCommand command = new SqlCommand("st_TrainingScreeningCheckOngoingLog", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add("@revID", System.Data.SqlDbType.Int);
                    command.Parameters["@revID"].Value = ReviewID;
                    command.Parameters.Add(new SqlParameter("@CONTACT_ID", ri.UserId));
                    command.Parameters.Add(new SqlParameter("@NewJobId", System.Data.SqlDbType.Int));
                    command.Parameters["@NewJobId"].Direction = System.Data.ParameterDirection.Output;
                    command.Parameters.Add(new SqlParameter("@NewTrainingId", System.Data.SqlDbType.Int));
                    command.Parameters["@NewTrainingId"].Direction = System.Data.ParameterDirection.Output; 
                    command.Parameters.Add("@RETURN_VALUE", System.Data.SqlDbType.Int);
                    command.Parameters["@RETURN_VALUE"].Direction = System.Data.ParameterDirection.ReturnValue;
                    command.ExecuteNonQuery();
                    string retVal = command.Parameters["@RETURN_VALUE"].Value.ToString();
                    if (retVal == "-1")
                    {
                        this.ReportBack = "Already Running";
                        return;
                    }
                    else if (retVal == "1" )
                    {//either all good, or prev. attempt failed and we try again
                        ReportBack = "Starting...";
                        NewJobId = (int)command.Parameters["@NewJobId"].Value;
                        _currentTrainingId = (int)command.Parameters["@NewTrainingId"].Value; 
                        Task.Run(() => DoBuildAndScore(ReviewID, ri.UserId, NewJobId, n_excludes + n_includes));
                    }
                    else //we assume this will never happen, SP must have returned -4!
                    {
                        throw new DataPortalException("Unable to check if Training is running!" + Environment.NewLine
                            + "This indicates there is a problem with this review." + Environment.NewLine
                            + "Please contact EPPI Reviewer Support.", this);
                    }
                }
            }

        }
        private void CreateNonMLLIst(int ReviewId, int UserId)
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();

                //ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                using (SqlCommand command = new SqlCommand("st_ScreeningCreateNonMLList", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ReviewId));
                    command.Parameters.Add(new SqlParameter("@CONTACT_ID", UserId));
                    command.Parameters.Add(new SqlParameter("@WHAT_ATTRIBUTE_ID", RevInfo.ScreeningWhatAttributeId));
                    command.Parameters.Add(new SqlParameter("@SCREENING_MODE", RevInfo.ScreeningMode));
                    command.Parameters.Add(new SqlParameter("@CODE_SET_ID", RevInfo.ScreeningCodeSetId));
                    command.Parameters.Add(new SqlParameter("@TRIGGERING_ITEM_ID", TriggeringItemId));
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        private async void DoBuildAndScore(int ReviewId, int UserId, int LogId, int AlreadyScreenedNumber)
        {
            string RemoteFileName = "";
            string LocalFileName = "";
            bool DataFactoryRes = false;
            string ScoresFile = "";
            int TotalLines = 0;
            try
            {
#if (!CSLA_NETCORE)
                LocalFileName = System.Web.HttpRuntime.AppDomainAppPath + TempPath + "PS-ReviewId" + RevInfo.ReviewId + "ContactId" + UserId.ToString() + ".tsv";
#else
                LocalFileName = "";
                DirectoryInfo tmpDir = System.IO.Directory.CreateDirectory("UserTempUploads");
                LocalFileName = tmpDir.FullName + @"\" + "PS-ReviewId" + RevInfo.ReviewId + "ContactId" + UserId.ToString() + ".tsv";
#endif
                RemoteFileName = "priority_screening/" + DataFactoryHelper.NameBase + "ReviewId" + RevInfo.ReviewId + ".tsv";
            }
            catch (Exception ex)
            {
                DataFactoryHelper.UpdateReviewJobLog(LogId, ReviewId, "Failed setting up files", "", "TrainingRunCommandV2", true, false);
                DataFactoryHelper.LogExceptionToFile(ex, ReviewId, LogId, "TrainingRunCommandV2");
                return;
            }
            try
            {
                using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand("st_TrainingWriteDataToAzure", connection))
                    {
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@REVIEW_ID", ReviewId));
                        //NEW: we are setting DB flag to true as we are actually indexing right now! 
                        //Flag can only be re-flipped by users, when they want to. In the future might be flipped when importing new items or editing existing ones...

                        command.CommandTimeout = 600;
                        using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                        {

                            using (System.IO.StreamWriter file = new System.IO.StreamWriter(LocalFileName, false))
                            {
                                file.WriteLine("PaperId\tPaperTitle\tAbstract\tIncl");
                                while (reader.Read())
                                {
                                    TotalLines++;
                                    file.WriteLine(reader["item_id"].ToString() + "\t" +
                                        ClassifierCommandV2.CleanText(reader, "title") + "\t" +
                                        ClassifierCommandV2.CleanText(reader, "abstract") + "\t" +
                                        ClassifierCommandV2.CleanText(reader, "INCLUDED"));
                                }
                            }
                        }
                    }
                    connection.Close();
                }
                if (AppIsShuttingDown)
                {
                    DataFactoryHelper.UpdateReviewJobLog(LogId, ReviewId, "Cancelled before upload", "", "TrainingRunCommandV2", true, false);
                    return;
                }
                if (AlreadyScreenedNumber >= TotalLines)
                {
                    File.Delete(LocalFileName);
                    DoScreeningIsFinished(ReviewId, UserId, LogId);
                    return;
                }
                DataFactoryHelper.UpdateReviewJobLog(LogId, ReviewId, "Uploading", "", "TrainingRunCommandV2");
                using (var fileStream = System.IO.File.OpenRead(LocalFileName))
                {
                    BlobOperations.UploadStream(blobConnection, "eppi-reviewer-data", RemoteFileName, fileStream); //, CancelToken);
                }
                if (AppIsShuttingDown)
                {
                    DataFactoryHelper.UpdateReviewJobLog(LogId, ReviewId, "Cancelled after upload", "", "TrainingRunCommandV2", true, false);
                    return;
                }
            }
            catch (Exception ex)
            {
                DataFactoryHelper.UpdateReviewJobLog(LogId, ReviewId, "Failed to get/upload data", "", "TrainingRunCommandV2", true, false);
                DataFactoryHelper.LogExceptionToFile(ex, ReviewId, LogId, "TrainingRunCommandV2");
                return;
            }
            try
            {
                DataFactoryHelper DFH = new DataFactoryHelper();
                string BatchGuid = Guid.NewGuid().ToString();
                ScoresFile = "priority_screening/" + DataFactoryHelper.NameBase + ReviewId.ToString() + "Scores.tsv";
                //string VecFile = "priority_screening/" + NameBase + ReviewId.ToString() + "Vectors.tsv";
                //string ClfFile = "priority_screening/" + NameBase + ReviewId.ToString() + "Clf.tsv";
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    {"do_build_and_score_log_reg", true },
                    {"DataFile", RemoteFileName },
                    {"EPPIReviewerApiRunId", BatchGuid},
                    {"do_build_log_reg", false},
                    {"do_score_log_reg", false},
                    {"ScoresFile", ScoresFile},
                    //{"VecFile", VecFile},
                    //{"ClfFile", ClfFile}
                };
                DataFactoryRes = await DFH.RunDataFactoryProcessV2("EPPI-Reviewer_API", parameters, ReviewId, LogId, "TrainingRunCommandV2", this.CancelToken);

                File.Delete(LocalFileName);
            }
            catch (Exception ex)
            {
                DataFactoryHelper.UpdateReviewJobLog(LogId, ReviewId, "Failed to run DF", "", "TrainingRunCommandV2", true, false);
                DataFactoryHelper.LogExceptionToFile(ex, ReviewId, LogId, "TrainingRunCommandV2");
                return;
            }

            if (DataFactoryRes == true)
            {
                try
                {
                    //we delete the "input" data file here because RunDataFactoryProcessV2 can be interrupted in IIS, while ML job is starting
                    //in case of an app pool recycle. Thus, we don't want to delete the remote file when the ML job might still need it!
                    //we expect to "pick up" the results when the job resumes... And we'll delete this file here, in the resumed run.
                    BlobOperations.DeleteIfExists(blobConnection, "eppi-reviewer-data", RemoteFileName);
                    if (AppIsShuttingDown)
                    {
                        DataFactoryHelper.UpdateReviewJobLog(LogId, ReviewId, "Cancelled at/during download", "", "TrainingRunCommandV2", true, false);
                        return;
                    }
                    else DataFactoryHelper.UpdateReviewJobLog(LogId, ReviewId, "Downloading Results", "", "TrainingRunCommandV2");
                    MemoryStream ms = BlobOperations.DownloadBlobAsMemoryStream(blobConnection, "eppi-reviewer-data", ScoresFile);

                    DataTable dt = new DataTable("Scores");
                    dt.Columns.Add("SCORE");
                    dt.Columns.Add("ITEM_ID");
                    dt.Columns.Add("REVIEW_ID");

                    using (StreamReader tsvReader = new StreamReader(ms))
                    {
                        //csvReader.SetDelimiters(new string[] { "," });
                        //csvReader.HasFieldsEnclosedInQuotes = false;
                        string line;
                        var throwaway = tsvReader.ReadLine();//headers line!!
                        while ((line = tsvReader.ReadLine()) != null)
                        {
                            string[] data = line.Split('\t');
                            if (data.Length == 3 && data[0].Length > 0 && data[2].Length > 0)
                            {
                                if (data[2] == "1")
                                {
                                    data[2] = "0.999999";
                                }
                                else if (data[2] == "0")
                                {
                                    data[2] = "0.000001";
                                }
                                else if (data[2].Length > 2 && data[2].Contains("E"))
                                {
                                    double dbl = 0;
                                    double.TryParse(data[2], out dbl);
                                    //if (dbl == 0.0) throw new Exception("Gotcha!");
                                    data[2] = dbl.ToString("F10");
                                }
                                dt.Rows.Add(data[2], data[0], ReviewId);
                            }
                        }
                    }
                    if (AppIsShuttingDown)
                    {
                        DataFactoryHelper.UpdateReviewJobLog(LogId, ReviewId, "Cancelled at/during download", "", "TrainingRunCommandV2", true, false); 
                        return;
                    }
                    else DataFactoryHelper.UpdateReviewJobLog(LogId, ReviewId, "Saving Results", "", "TrainingRunCommandV2");
                    using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
                    {
                        connection.Open();
                        using (SqlBulkCopy sbc = new SqlBulkCopy(connection))
                        {
                            sbc.DestinationTableName = "TB_SCREENING_ML_TEMP";
                            sbc.ColumnMappings.Clear();
                            sbc.ColumnMappings.Add("SCORE", "SCORE");
                            sbc.ColumnMappings.Add("ITEM_ID", "ITEM_ID");
                            sbc.ColumnMappings.Add("REVIEW_ID", "REVIEW_ID");
                            sbc.BatchSize = 1000;
                            sbc.WriteToServer(dt);
                        }
                        using (SqlCommand command = new SqlCommand("st_ScreeningCreateMLList", connection))
                        {
                            command.CommandType = System.Data.CommandType.StoredProcedure;
                            command.Parameters.Add(new SqlParameter("@REVIEW_ID", ReviewId));
                            command.Parameters.Add(new SqlParameter("@CONTACT_ID", UserId));
                            command.Parameters.Add(new SqlParameter("@WHAT_ATTRIBUTE_ID", RevInfo.ScreeningWhatAttributeId));
                            command.Parameters.Add(new SqlParameter("@SCREENING_MODE", RevInfo.ScreeningMode));
                            command.Parameters.Add(new SqlParameter("@CODE_SET_ID", RevInfo.ScreeningCodeSetId));
                            command.Parameters.Add(new SqlParameter("@TRAINING_ID", _currentTrainingId));
                            command.CommandTimeout = 145;
                            if (dt.Rows.Count > 30000)
                            {//adjust timeout for large reviews: we don't care if this is slow, as it's a costly operation anyway.
                                int adjuster = dt.Rows.Count - 29999;
                                command.CommandTimeout = command.CommandTimeout + (int)Math.Round(((double)adjuster / 1000));
                            }
                            command.ExecuteNonQuery();

                            //code used to trigger an excemption almost always, used to test if logging does work...
                            //naturally this code should never be active in production!!
                            //Random r = new Random();
                            //if (r.Next() > 0.0000001) throw new Exception("done manually for testing purpose...", new Exception("this is the inner exception"));

                        }
                        connection.Close();
                    }
                    BlobOperations.DeleteIfExists(blobConnection, "eppi-reviewer-data", ScoresFile);
                }
                catch (Exception ex)
                {
                    DataFactoryHelper.UpdateReviewJobLog(LogId, ReviewId, "Failed after DF", "", "TrainingRunCommandV2", true, false);
                    DataFactoryHelper.LogExceptionToFile(ex, ReviewId, LogId, "TrainingRunCommandV2");
                    return;
                }
            }
            if (DataFactoryRes == false && !AppIsShuttingDown)
            {
                BlobOperations.DeleteIfExists(blobConnection, "eppi-reviewer-data", ScoresFile);
            }
            else if (DataFactoryRes == true) DataFactoryHelper.UpdateReviewJobLog(LogId, ReviewId, "Ended", "", "TrainingRunCommandV2", true, true);
            
        }

        private void DoScreeningIsFinished(int ReviewId, int UserId, int LogId)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand("st_ScreeningCreateMLList", connection))
                    {
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@REVIEW_ID", ReviewId));
                        command.Parameters.Add(new SqlParameter("@CONTACT_ID", UserId));
                        command.Parameters.Add(new SqlParameter("@WHAT_ATTRIBUTE_ID", RevInfo.ScreeningWhatAttributeId));
                        command.Parameters.Add(new SqlParameter("@SCREENING_MODE", RevInfo.ScreeningMode));
                        command.Parameters.Add(new SqlParameter("@CODE_SET_ID", RevInfo.ScreeningCodeSetId));
                        command.Parameters.Add(new SqlParameter("@TRAINING_ID", _currentTrainingId));
                        command.CommandTimeout = 145;
                        
                        command.ExecuteNonQuery();
                    }
                    connection.Close();
                }
                DataFactoryHelper.UpdateReviewJobLog(LogId, ReviewId, "Ended", "", "TrainingRunCommandV2", true, true);
            }
            catch (Exception ex)
            {
                DataFactoryHelper.UpdateReviewJobLog(LogId, ReviewId, "Failed at ScreeningIsFinished", "", "TrainingRunCommandV2", true, false);
                DataFactoryHelper.LogExceptionToFile(ex, ReviewId, LogId, "TrainingRunCommandV2");
                return;
            }
        }
        

        

        private async void DoSimulation(int ReviewID, int UserId)
        {
            
        }

        private void FetchSimulationResults(int ReviewID)
        {
            
        }

 
        static string blobConnection = AzureSettings.blobConnection;
        static string BaseUrlBuildAndScore = AzureSettings.BaseUrlBuildAndScore;
        static string apiKeyBuildAndScore = AzureSettings.apiKeyBuildAndScore;
        static string BaseUrlVectorise = AzureSettings.BaseUrlVectorise;
        static string apiKeyVectorise = AzureSettings.apiKeyVectorise;
        static string BaseUrlSimulation5 = AzureSettings.BaseUrlSimulation5;
        static string apiKeySimulation5 = AzureSettings.apiKeySimulation5;
        const string TempPath = @"UserTempUploads\";


#endif
    }
}
