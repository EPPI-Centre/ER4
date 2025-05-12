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

#if !CSLA_NETCORE
using System.Web.Hosting;
#endif
using System.Data;
#endif


namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class MagAddClassifierScoresCommand : LongLastingFireAndForgetCommand
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
                System.Threading.Tasks.Task.Run(() => AddClassifierScores(ri.ReviewId, ri.UserId));
#else
                //see: https://codingcanvas.com/using-hostingenvironment-queuebackgroundworkitem-to-run-background-tasks-in-asp-net/
                HostingEnvironment.QueueBackgroundWorkItem(cancellationToken => AddClassifierScores(ri.ReviewId, ri.UserId, cancellationToken));
#endif
            
        }

        private async Task AddClassifierScores(int ReviewId, int ContactId, CancellationToken cancellationToken = default(CancellationToken))
        {
            List<Int64> Ids = new List<long>();
            int ModelReviewId = -1; //will be used later
            int NewJobId = 0;

            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                try //2 checks: can this user run this model, and then is another job of this type running?
                {
                    connection.Open();
                    if (_UserClassifierModelId > 0) {
                        using (SqlCommand command = new SqlCommand("st_ClassifierContactModels", connection))
                        {
                            command.CommandType = System.Data.CommandType.StoredProcedure;
                            command.Parameters.Add(new SqlParameter("@CONTACT_ID", ContactId));
                            using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                            {
                                while (reader.Read())
                                {
                                    int tempModelid = reader.GetInt32("MODEL_ID");
                                    if (tempModelid == _UserClassifierModelId)
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
                            //the query above didn't find the current model, so we can't/should not continue...
                            return;
                        }
                    }
                    
                    using (SqlCommand command2 = new SqlCommand("st_ClassifierCanRunCheckAndMarkAsStarting", connection))
                    {//this SP also checks if a Apply (or build) job is running and creates the new job record if not
                        command2.CommandType = System.Data.CommandType.StoredProcedure;

                        command2.Parameters.Add(new SqlParameter("@MODEL_ID", _UserClassifierModelId));
                        command2.Parameters.Add(new SqlParameter("@REVIEW_ID", ReviewId));
                        command2.Parameters.Add(new SqlParameter("@REVIEW_ID_OF_MODEL", ModelReviewId));
                        command2.Parameters.Add(new SqlParameter("@CONTACT_ID", ContactId));
                        command2.Parameters.Add(new SqlParameter("@TITLE", "[Apply to OpenAlex Auto Update]"));
                        command2.Parameters.Add(new SqlParameter("@JobType", "Apply")); //"Apply", "Build" or "ChckS" (for "Check Screening")
                        command2.Parameters.Add(new SqlParameter("@NewJobId", 0));
                        command2.Parameters["@NewJobId"].Direction = System.Data.ParameterDirection.Output;
                        command2.ExecuteNonQuery();
                        NewJobId = Convert.ToInt32(command2.Parameters["@NewJobId"].Value);
                        if (NewJobId == 0)
                        {
                            //another job of this type is running
                            return;
                        }
                    }
                }

                catch (Exception ex)
                {
                    DataFactoryHelper.LogExceptionToFile(ex, ReviewId, -1, "MagAddClassifierScoresCommand");
                    return;
                }
            }
            try
            {
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
            }
            catch (Exception ex) 
            {
                DataFactoryHelper.UpdateReviewJobLog(NewJobId, ReviewId, "Failed at st_MagAutoUpdateRunResults", "", "MagAddClassifierScoresCommand", true, false);
                DataFactoryHelper.LogExceptionToFile(ex, ReviewId, NewJobId, "MagAddClassifierScoresCommand");
                return;
            }

            if (Ids.Count == 0)
            {
                DataFactoryHelper.UpdateReviewJobLog(NewJobId, ReviewId, "No papers to score", "", "MagAddClassifierScoresCommand", true, false);
                return;
            }
            if (AppIsShuttingDown || cancellationToken.IsCancellationRequested)
            {
                DataFactoryHelper.UpdateReviewJobLog(NewJobId, ReviewId, "Cancelled before data-fetch", "", "MagAddClassifierScoresCommand", true, false);
                return;
            }

#if (!CSLA_NETCORE)
            string fName = System.Web.HttpRuntime.AppDomainAppPath + @"UserTempUploads/Cont" + _MagAutoUpdateRunId.ToString() + ".tsv";
#else
            DirectoryInfo tmpDir = System.IO.Directory.CreateDirectory("UserTempUploads");
                string fName = tmpDir.FullName + "/Cont" + _MagAutoUpdateRunId.ToString() + ".tsv";
#endif
            try
            {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(fName))
                {
                    //file.WriteLine("\"ITEM_ID\",\"LABEL\",\"TITLE\",\"ABSTRACT\",\"KEYWORDS\",\"REVIEW_ID\"");
                    file.WriteLine("PaperId\tPaperTitle\tAbstract\tIncl");
                    int count = 0;
                    bool isFirstPaper = true;
                    while (count < Ids.Count)
                    {
                        string query = "";
                        for (int i = count; i < Ids.Count && i < count + 50; i++)
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

                        MagMakesHelpers.OaPaperFilterResult resp = MagMakesHelpers.EvaluateOaPaperFilter("openalex_id:https://openalex.org/" + query, "50", "1", false);
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
                            ClassifierCommandV2.WriteDataLineInFileToUpload(pm.id.Replace("https://openalex.org/W", "")
                                , MagMakesHelpers.CleanText(pm.title)
                                , MagMakesHelpers.CleanText(MagMakesHelpers.ReconstructInvertedAbstract(pm.abstract_inverted_index))
                                , "99", file);
                            //file.WriteLine(pm.id.Replace("https://openalex.org/W", "") + "\t" +
                            //                MagMakesHelpers.CleanText(pm.title) + "\t" +
                            //                MagMakesHelpers.CleanText(MagMakesHelpers.ReconstructInvertedAbstract(pm.abstract_inverted_index)) + "\t" +
                            //                "99");
                        }
                        count += 50;
                        if (AppIsShuttingDown || cancellationToken.IsCancellationRequested)
                        {
                           break;//can't delete the file inside the "using file" block!
                        }
                    }
                }
                if (AppIsShuttingDown || cancellationToken.IsCancellationRequested)
                {//now we can delete the file!
                    DataFactoryHelper.UpdateReviewJobLog(NewJobId, ReviewId, "Cancelled during data-fetch", "", "MagAddClassifierScoresCommand", true, false);
                    File.Delete(fName);
                    return;
                }
            }
            catch (Exception ex)
            {
                DataFactoryHelper.UpdateReviewJobLog(NewJobId, ReviewId, "Failed at fetching data from OA", "", "MagAddClassifierScoresCommand", true, false);
                DataFactoryHelper.LogExceptionToFile(ex, ReviewId, NewJobId, "MagAddClassifierScoresCommand");
                return;
            }
            string FolderAndFileName;
            string RemoteFolder;
            string RemoteFileName;
            string resultsFile1;
            if (_UserClassifierModelId > 0)
            {
                FolderAndFileName = DataFactoryHelper.NameBase + "ReviewId" + ModelReviewId.ToString() + "ModelId" + _UserClassifierModelId;
                RemoteFolder = "user_models/" + FolderAndFileName + "/";
                RemoteFileName = RemoteFolder + FolderAndFileName + "StudyModelToScore.tsv";
                resultsFile1 = RemoteFolder + "Cont" + _MagAutoUpdateRunId.ToString() + "Results.tsv";
            }
            else
            {
                FolderAndFileName = DataFactoryHelper.NameBase + "ReviewId" + ReviewId.ToString() + _StudyTypeClassifier + "Model";
                RemoteFolder = "user_models/" + FolderAndFileName + "/";
                RemoteFileName = RemoteFolder + FolderAndFileName + "StudyModelToScore.tsv";
                resultsFile1 = RemoteFolder + "Cont" + _MagAutoUpdateRunId.ToString() + "Results.tsv";
            }
            bool DataFactoryRes = false;
            string blobConnection = AzureSettings.blobConnection;
            try 
            {
                using (var fileStream = System.IO.File.OpenRead(fName))
                {
                    BlobOperations.UploadStream(blobConnection,
                        "eppi-reviewer-data"
                        , RemoteFileName
                        , fileStream);
                }
                File.Delete(fName);
            }
            catch (Exception ex)
            {
                try { File.Delete(fName); } catch { }
                DataFactoryHelper.UpdateReviewJobLog(NewJobId, ReviewId, "Failed to uplodad data to score", "", "MagAddClassifierScoresCommand", true, false);
                DataFactoryHelper.LogExceptionToFile(ex, ReviewId, NewJobId, "MagAddClassifierScoresCommand");
                return;
            }
            if (AppIsShuttingDown || cancellationToken.IsCancellationRequested)
            {//now we can delete the file!
                DataFactoryHelper.UpdateReviewJobLog(NewJobId, ReviewId, "Cancelled after upload", "", "MagAddClassifierScoresCommand", true, false);
                try { File.Delete(fName); } catch { }
                return;
            }
            try
            {
                DataFactoryHelper DFH = new DataFactoryHelper();
                string BatchGuid = Guid.NewGuid().ToString();
                string VecFile = RemoteFolder + "Vectors.pkl";
                string ClfFile = RemoteFolder + "Clf.pkl";
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                if (_UserClassifierModelId > 0)
                {
                    parameters = new Dictionary<string, object>
                    {
                        {"do_build_and_score_log_reg", false },
                        {"DataFile", RemoteFileName },
                        {"EPPIReviewerApiRunId", BatchGuid},
                        {"do_build_log_reg", false},
                        {"do_score_log_reg", true},
                        {"ScoresFile", resultsFile1},
                        {"VecFile", VecFile},
                        {"ClfFile", ClfFile}
                    };
                }
                else
                {
                    switch (_StudyTypeClassifier)
                    {
                        case "RCT": //classifierId = -1;
                            parameters = new Dictionary<string, object>
                            {
                                {"do_build_and_score_log_reg", false },
                                {"DataFile", RemoteFileName },
                                {"EPPIReviewerApiRunId", BatchGuid},
                                {"ScoresFile", resultsFile1},
                                {"do_original_rct_classifier_b", true }
                            };
                            break;
                        case "Cochrane RCT"://classifierId = -4;
                            parameters = new Dictionary<string, object>
                            {
                                {"do_build_and_score_log_reg", false },
                                {"DataFile", RemoteFileName },
                                {"EPPIReviewerApiRunId", BatchGuid},
                                {"ScoresFile", resultsFile1},
                                {"do_cochrane_rct_classifier_b", true }
                            };
                            break;
                        case "Economic evaluation"://classifierId = -3;
                            parameters = new Dictionary<string, object>
                            {
                                {"do_build_and_score_log_reg", false },
                                {"DataFile", RemoteFileName },
                                {"EPPIReviewerApiRunId", BatchGuid},
                                {"ScoresFile", resultsFile1},
                                {"do_economic_eval_classifier_b", true }
                            };
                            break;
                        case "Systematic review"://classifierId = -2;
                            parameters = new Dictionary<string, object>
                            {
                                {"do_build_and_score_log_reg", false },
                                {"DataFile", RemoteFileName },
                                {"EPPIReviewerApiRunId", BatchGuid},
                                {"ScoresFile", resultsFile1},
                                {"do_systematic_reviews_classifier_b", true }
                            };
                            break;
                        default:
                            DataFactoryHelper.UpdateReviewJobLog(NewJobId, ReviewId, "Failed: unknown model", "", "MagAddClassifierScoresCommand", true, false);
                            return;
                    }
                }
                DataFactoryRes = await DFH.RunDataFactoryProcessV2("EPPI-Reviewer_API", parameters, ReviewId, NewJobId, "MagAddClassifierScoresCommand", this.CancelToken);
            }
            catch (Exception ex)
            {
                DataFactoryHelper.UpdateReviewJobLog(NewJobId, ReviewId, "Failed to run DF", "", "MagAddClassifierScoresCommand", true, false);
                DataFactoryHelper.LogExceptionToFile(ex, ReviewId, NewJobId, "MagAddClassifierScoresCommand");
                return;
            }
            if (DataFactoryRes)
            {
                try
                {
                    insertResults(blobConnection, "eppi-reviewer-data"
                        , resultsFile1, _UserClassifierModelId > 0 ? "UserClassifierScore" : "StudyTypeClassifierScore"
                        , ReviewId, ContactId, NewJobId, cancellationToken);
                    BlobOperations.DeleteIfExists(blobConnection, "eppi-reviewer-data", RemoteFileName);
                    BlobOperations.DeleteIfExists(blobConnection, "eppi-reviewer-data", resultsFile1);
                    if (DataFactoryRes == true) DataFactoryHelper.UpdateReviewJobLog(NewJobId, ReviewId, "Ended", "", "MagAddClassifierScoresCommand", true, true);
                }
                catch (Exception ex)
                {
                    DataFactoryHelper.UpdateReviewJobLog(NewJobId, ReviewId, "Failed to run insert results", "", "MagAddClassifierScoresCommand", true, false);
                    DataFactoryHelper.LogExceptionToFile(ex, ReviewId, NewJobId, "MagAddClassifierScoresCommand");
                    return;
                }
            }
        }

        private async Task AddPreBuiltClassifierScores(int ReviewId, int ContactId, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (_StudyTypeClassifier == "None") return;//shouldn't happen
            else 
            {
                //int classifierId = 0;
                //string classifierName = "RCTModel";
                //switch (_StudyTypeClassifier)
                //{
                //    case "RCT":
                //        classifierId = 0;
                //        classifierName = "RCTModel";
                //        break;
                //    case "Cochrane RCT":
                //        classifierId = -4;
                //        classifierName = "NewRCTModel";
                //        break;
                //    case "Economic evaluation":
                //        classifierId = -3;
                //        classifierName = "NHSEEDModel";
                //        break;
                //    case "Systematic review":
                //        classifierId = -2;
                //        classifierName = "DAREModel";
                //        break;
                //}
                //await ClassifierCommand.InvokeBatchExecutionService(ReviewId, "ScoreModel", classifierId, @"attributemodeldata/" + uploadedFileName,
                //    @"attributemodels/" + classifierName + ".csv", resultsFile1, resultsFile2, cancellationToken);

                //if (cancellationToken.IsCancellationRequested) //InvokeBatchExecutionService "just" returns if cancelled, so we find out if cancellation happened in here.
                //{
                //    MagLog.SaveLogEntry("Add classifier scores", "Cancelled", "Cancellation token, after InvokeBatchExecution. Review: " + ReviewId, ContactId);
                //    return;
                //}

                //insertResults(TrainingRunCommand.NameBase + "Cont" + _MagAutoUpdateRunId.ToString() +
                //    "Results1.csv", "StudyTypeClassifierScore", ReviewId, ContactId);
                //if (classifierId == -4)
                //{
                //    insertResults(TrainingRunCommand.NameBase + "Cont" + _MagAutoUpdateRunId.ToString() +
                //        "Results2.csv", "StudyTypeClassifierScore", ReviewId, ContactId);
                //}
            }
        }

        private void insertResults(string blobConnection, string container, string FileName, string Field, int ReviewId, int ContactId, int NewJobId, CancellationToken cancellationToken)
        {
            DataTable dt = new DataTable("Ids");
            dt.Columns.Add("MAG_AUTO_UPDATE_RUN_ID");
            dt.Columns.Add("PaperId");
            dt.Columns.Add("SCORE");

            try
            {
                MemoryStream msIds = BlobOperations.DownloadBlobAsMemoryStream(blobConnection, container, FileName);
                using (var readerIds = new StreamReader(msIds))
                {
                    string line = readerIds.ReadLine(); //score is data[4], ID is data[0]
                    if (_UserClassifierModelId > 0)
                    { 
                        while ((line = readerIds.ReadLine()) != null)
                        {
                            string[] fields = line.Split('\t');

                            // this check for extreme values copied from ClassifierCommand
                            if (fields[4] == "1")
                            {
                                fields[4] = "0.999999";
                            }
                            else if (fields[4] == "0")
                            {
                                fields[4] = "0.000001";
                            }
                            else if (fields[4].Length > 2 && fields[4].Contains("E"))
                            {
                                double dbl = 0;
                                double.TryParse(fields[4], out dbl);
                                //if (dbl == 0.0) throw new Exception("Gotcha!");
                                fields[4] = dbl.ToString("F10");
                            }

                            DataRow newRow = dt.NewRow();
                            newRow["MAG_AUTO_UPDATE_RUN_ID"] = _MagAutoUpdateRunId;
                            newRow["PaperId"] = fields[0];
                            newRow["SCORE"] = fields[4];
                            dt.Rows.Add(newRow);
                        }
                    }
                    else
                    {
                        while ((line = readerIds.ReadLine()) != null)
                        {
                            //fields[2] is paperId, fields[3] is score
                            string[] fields = line.Split('\t');

                            // this check for extreme values copied from ClassifierCommand
                            if (fields[3] == "1")
                            {
                                fields[3] = "0.999999";
                            }
                            else if (fields[3] == "0")
                            {
                                fields[3] = "0.000001";
                            }
                            else if (fields[3].Length > 2 && fields[3].Contains("E"))
                            {
                                double dbl = 0;
                                double.TryParse(fields[3], out dbl);
                                //if (dbl == 0.0) throw new Exception("Gotcha!");
                                fields[3] = dbl.ToString("F10");
                            }

                            DataRow newRow = dt.NewRow();
                            newRow["MAG_AUTO_UPDATE_RUN_ID"] = _MagAutoUpdateRunId;
                            newRow["PaperId"] = fields[2];
                            newRow["SCORE"] = fields[3];
                            dt.Rows.Add(newRow);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DataFactoryHelper.UpdateReviewJobLog(NewJobId, ReviewId, "Failed to parse results", "", "MagAddClassifierScoresCommand", true, false);
                DataFactoryHelper.LogExceptionToFile(ex, ReviewId, NewJobId, "MagAddClassifierScoresCommand");
                return;
            }
            if (AppIsShuttingDown || cancellationToken.IsCancellationRequested)
            {
                DataFactoryHelper.UpdateReviewJobLog(NewJobId, ReviewId, "Cancelled after parsing results", "", "MagAddClassifierScoresCommand", true, false);
                return;
            }
            if (dt.Rows.Count == 0)
            {
                DataFactoryHelper.UpdateReviewJobLog(NewJobId, ReviewId, "Failed: no results parsed", "", "MagAddClassifierScoresCommand", true, false);
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
            catch (Exception ex)
            {
                DataFactoryHelper.UpdateReviewJobLog(NewJobId, ReviewId, "Failed to save results", "", "MagAddClassifierScoresCommand", true, false);
                DataFactoryHelper.LogExceptionToFile(ex, ReviewId, NewJobId, "MagAddClassifierScoresCommand");
                return;
            }
        }
#endif


    }
}
