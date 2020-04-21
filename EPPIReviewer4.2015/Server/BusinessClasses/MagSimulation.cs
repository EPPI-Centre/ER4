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
using Csla.DataPortalClient;
using System.IO;
using System.Configuration;


#if !SILVERLIGHT
using System.Data.SqlClient;
using BusinessLibrary.Data;
using Csla.Data;
using BusinessLibrary.Security;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Data;
#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class MagSimulation : BusinessBase<MagSimulation>
    {
#if SILVERLIGHT
    public MagSimulation() { }

        
#else
        private MagSimulation() { }
#endif

        private static PropertyInfo<int> MagSimulationIdProperty = RegisterProperty<int>(new PropertyInfo<int>("MagSimulationId", "MagSimulationId", 0));
        public int MagSimulationId
        {
            get
            {
                return GetProperty(MagSimulationIdProperty);
            }
        }

        private static PropertyInfo<int> ReviewIdProperty = RegisterProperty<int>(new PropertyInfo<int>("ReviewId", "ReviewId", 0));
        public int ReviewId
        {
            get
            {
                return GetProperty(ReviewIdProperty);
            }
        }

        private static PropertyInfo<int> YearProperty = RegisterProperty<int>(new PropertyInfo<int>("Year", "Year"));
        public int Year
        {
            get
            {
                return GetProperty(YearProperty);
            }
            set
            {
                SetProperty(YearProperty, value);
            }
        }

        private static PropertyInfo<SmartDate> CreatedDateProperty = RegisterProperty<SmartDate>(new PropertyInfo<SmartDate>("CreatedDate", "CreatedDate"));
        public SmartDate CreatedDate
        {
            get
            {
                return GetProperty(CreatedDateProperty);
            }
            set
            {
                SetProperty(CreatedDateProperty, value);
            }
        }

        private static PropertyInfo<Int64> WithThisAttributeIdProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("WithThisAttributeId", "WithThisAttributeId"));
        public Int64 WithThisAttributeId
        {
            get
            {
                return GetProperty(WithThisAttributeIdProperty);
            }
            set
            {
                SetProperty(WithThisAttributeIdProperty, value);
            }
        }

        private static PropertyInfo<Int64> FilteredByAttributeIdProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("FilteredByAttributeId", "FilteredByAttributeId"));
        public Int64 FilteredByAttributeId
        {
            get
            {
                return GetProperty(FilteredByAttributeIdProperty);
            }
            set
            {
                SetProperty(FilteredByAttributeIdProperty, value);
            }
        }

        private static PropertyInfo<string> SearchMethodProperty = RegisterProperty<string>(new PropertyInfo<string>("SearchMethod", "SearchMethod"));
        public string SearchMethod
        {
            get
            {
                return GetProperty(SearchMethodProperty);
            }
            set
            {
                SetProperty(SearchMethodProperty, value);
            }
        }

        private static PropertyInfo<string> NetworkStatisticProperty = RegisterProperty<string>(new PropertyInfo<string>("NetworkStatistic", "NetworkStatistic"));
        public string NetworkStatistic
        {
            get
            {
                return GetProperty(NetworkStatisticProperty);
            }
            set
            {
                SetProperty(NetworkStatisticProperty, value);
            }
        }

        private static PropertyInfo<string> StudyTypeClassifierProperty = RegisterProperty<string>(new PropertyInfo<string>("StudyTypeClassifier", "StudyTypeClassifier"));
        public string StudyTypeClassifier
        {
            get
            {
                return GetProperty(StudyTypeClassifierProperty);
            }
            set
            {
                SetProperty(StudyTypeClassifierProperty, value);
            }
        }

        private static PropertyInfo<int> UserClassifierModelIdProperty = RegisterProperty<int>(new PropertyInfo<int>("UserClassifierModelId", "UserClassifierModelId"));
        public int UserClassifierModelId
        {
            get
            {
                return GetProperty(UserClassifierModelIdProperty);
            }
            set
            {
                SetProperty(UserClassifierModelIdProperty, value);
            }
        }

        private static PropertyInfo<string> StatusProperty = RegisterProperty<string>(new PropertyInfo<string>("Status", "Status", ""));
        public string Status
        {
            get
            {
                return GetProperty(StatusProperty);
            }
            set
            {
                SetProperty(StatusProperty, value);
            }
        }

        private static PropertyInfo<string> WithThisAttributeProperty = RegisterProperty<string>(new PropertyInfo<string>("WithThisAttribute", "WithThisAttribute", ""));
        public string WithThisAttribute
        {
            get
            {
                return GetProperty(WithThisAttributeProperty);
            }
            set
            {
                SetProperty(WithThisAttributeProperty, value);
            }
        }

        private static PropertyInfo<string> SeedTextProperty = RegisterProperty<string>(new PropertyInfo<string>("SeedText", "SeedText", ""));
        public string SeedText
        {
            get
            {
                if (GetProperty(YearProperty) != 1753)
                {
                    return "Publication before: " + GetProperty(YearProperty);
                }
                if (GetProperty(CreatedDateProperty) != Convert.ToDateTime("1/1/1753"))
                {
                    return "Created before: " + GetProperty(CreatedDateProperty).ToString();
                }
                return "With code: " + GetProperty(WithThisAttributeProperty);
            }
        }

        private static PropertyInfo<string> FilteredByAttributeProperty = RegisterProperty<string>(new PropertyInfo<string>("FilteredByAttribute", "FilteredByAttribute", ""));
        public string FilteredByAttribute
        {
            get
            {
                return GetProperty(FilteredByAttributeProperty);
            }
            set
            {
                SetProperty(FilteredByAttributeProperty, value);
            }
        }

        private static PropertyInfo<string> UserClassifierModelProperty = RegisterProperty<string>(new PropertyInfo<string>("UserClassifierModel", "UserClassifierModel", ""));
        public string UserClassifierModel
        {
            get
            {
                return GetProperty(UserClassifierModelProperty);
            }
            set
            {
                SetProperty(UserClassifierModelProperty, value);
            }
        }

        private static PropertyInfo<int> TPProperty = RegisterProperty<int>(new PropertyInfo<int>("TP", "TP"));
        public int TP
        {
            get
            {
                return GetProperty(TPProperty);
            }
            set
            {
                SetProperty(TPProperty, value);
            }
        }

        private static PropertyInfo<int> FPProperty = RegisterProperty<int>(new PropertyInfo<int>("FP", "FP"));
        public int FP
        {
            get
            {
                return GetProperty(FPProperty);
            }
            set
            {
                SetProperty(FPProperty, value);
            }
        }

        private static PropertyInfo<int> FNProperty = RegisterProperty<int>(new PropertyInfo<int>("FN", "FN"));
        public int FN
        {
            get
            {
                return GetProperty(FNProperty);
            }
            set
            {
                SetProperty(FNProperty, value);
            }
        }

        private static PropertyInfo<int> NSeedsProperty = RegisterProperty<int>(new PropertyInfo<int>("NSeeds", "NSeeds"));
        public int NSeeds
        {
            get
            {
                return GetProperty(NSeedsProperty);
            }
            set
            {
                SetProperty(NSeedsProperty, value);
            }
        }

        private static PropertyInfo<float> PrecisionProperty = RegisterProperty<float>(new PropertyInfo<float>("Precision", "Precision"));
        public float Precision
        {
            get
            {
                return TP / (TP + FP);
            }
        }

        private static PropertyInfo<float> RecallProperty = RegisterProperty<float>(new PropertyInfo<float>("Recall", "Recall"));
        public float Recall
        {
            get
            {
                return TP / (TP + FN);
            }
        }


        //protected override void AddAuthorizationRules()
        //{
        //    //string[] canWrite = new string[] { "AdminUser", "RegularUser" };
        //    //string[] canRead = new string[] { "AdminUser", "RegularUser", "ReadOnlyUser" };
        //    //string[] admin = new string[] { "AdminUser" };
        //    //AuthorizationRules.AllowCreate(typeof(MagSimulation), admin);
        //    //AuthorizationRules.AllowDelete(typeof(MagSimulation), admin);
        //    //AuthorizationRules.AllowEdit(typeof(MagSimulation), canWrite);
        //    //AuthorizationRules.AllowGet(typeof(MagSimulation), canRead);

        //    //AuthorizationRules.AllowRead(MagSimulationIdProperty, canRead);
        //    //AuthorizationRules.AllowRead(ReviewIdProperty, canRead);
        //    //AuthorizationRules.AllowRead(NameProperty, canRead);
        //    //AuthorizationRules.AllowRead(DetailProperty, canRead);

        //    //AuthorizationRules.AllowWrite(NameProperty, canWrite);
        //    //AuthorizationRules.AllowRead(DetailProperty, canRead);
        //}

        protected override void AddBusinessRules()
        {
            //ValidationRules.AddRule(Csla.Validation.CommonRules.MaxValue<decimal>, new Csla.Validation.CommonRules.MaxValueRuleArgs<decimal>(ReviewCodeSetFte1Property, 1));
            //ValidationRules.AddRule(Csla.Validation.CommonRules.StringRequired, new Csla.Validation.RuleArgs(ReviewCodeSetNameProperty));
            //ValidationRules.AddRule(Csla.Validation.CommonRules.StringMaxLength, new Csla.Validation.CommonRules.MaxLengthRuleArgs(ReviewCodeSetNameProperty, 20));
        }

#if !SILVERLIGHT

        protected override void DataPortal_Insert()
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_MagSimulationInsert", connection))
                {
                    int newid = 0;
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@YEAR", ReadProperty(YearProperty)));
                    command.Parameters.Add(new SqlParameter("@CREATED_DATE", CreatedDate.DBValue));
                    command.Parameters.Add(new SqlParameter("@WITH_THIS_ATTRIBUTE_ID", ReadProperty(WithThisAttributeIdProperty)));
                    command.Parameters.Add(new SqlParameter("@FILTERED_BY_ATTRIBUTE_ID", ReadProperty(FilteredByAttributeIdProperty)));
                    command.Parameters.Add(new SqlParameter("@SEARCH_METHOD", ReadProperty(SearchMethodProperty)));
                    command.Parameters.Add(new SqlParameter("@NETWORK_STATISTIC", ReadProperty(NetworkStatisticProperty)));
                    command.Parameters.Add(new SqlParameter("@STUDY_TYPE_CLASSIFIER", ReadProperty(StudyTypeClassifierProperty)));
                    command.Parameters.Add(new SqlParameter("@USER_CLASSIFIER_MODEL_ID", ReadProperty(UserClassifierModelIdProperty)));
                    command.Parameters.Add(new SqlParameter("@STATUS", ReadProperty(StatusProperty)));
                    command.Parameters.Add(new SqlParameter("@MAG_SIMULATION_ID", newid));
                    command.Parameters["@MAG_SIMULATION_ID"].Direction = System.Data.ParameterDirection.Output;
                    command.ExecuteNonQuery();
                    LoadProperty(MagSimulationIdProperty, command.Parameters["@MAG_SIMULATION_ID"].Value);
                }
                connection.Close();
                // Run in separate thread and return this object to client
                Task.Run(() => { RunSimulation(ri.ReviewId, ri.UserId); });
            }
        }

        protected override void DataPortal_Update()
        {
            // There's nothing to update
        }

        protected override void DataPortal_DeleteSelf()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_MagSimulationDelete", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@MAG_SIMULATION_ID", ReadProperty(MagSimulationIdProperty)));
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        protected void DataPortal_Fetch(SingleCriteria<MagSimulation, Int64> criteria)
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_MagSimulation", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@MAG_SIMULATION_ID", criteria.Value));
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        if (reader.Read())
                        {
                            LoadProperty<Int32>(ReviewIdProperty, reader.GetInt32("REVIEW_ID"));
                            LoadProperty<Int32>(MagSimulationIdProperty, reader.GetInt32("MAG_SIMULATION_ID"));
                            LoadProperty<Int32>(YearProperty, reader.GetInt32("YEAR"));
                            LoadProperty<SmartDate>(CreatedDateProperty, reader.GetSmartDate("CREATED_DATE"));
                            LoadProperty<Int64>(WithThisAttributeIdProperty, reader.GetInt64("WITH_THIS_ATTRIBUTE_ID"));
                            LoadProperty<Int64>(FilteredByAttributeIdProperty, reader.GetInt64("FILTERED_BY_ATTRIBUTE_ID"));
                            LoadProperty<string>(SearchMethodProperty, reader.GetString("SEARCH_METHOD"));
                            LoadProperty<string>(NetworkStatisticProperty, reader.GetString("NETWORK_STATISTIC"));
                            LoadProperty<string>(StudyTypeClassifierProperty, reader.GetString("STUDY_TYPE_CLASSIFIER"));
                            LoadProperty<Int32>(UserClassifierModelIdProperty, reader.GetInt32("USER_CLASSIFIER_MODEL_ID"));
                            LoadProperty<string>(StatusProperty, reader.GetString("STATUS"));
                            LoadProperty<Int32>(TPProperty, reader.GetInt32("TP"));
                            LoadProperty<Int32>(FPProperty, reader.GetInt32("FP"));
                            LoadProperty<Int32>(FNProperty, reader.GetInt32("FN"));
                            LoadProperty<Int32>(NSeedsProperty, reader.GetInt32("NSEEDS"));
                            LoadProperty<string>(WithThisAttributeProperty, reader.GetString("WithThisAttribute"));
                            LoadProperty<string>(FilteredByAttributeProperty, reader.GetString("FilteredByAttribute"));
                            LoadProperty<string>(UserClassifierModelProperty, reader.GetString("MODEL_TITLE"));
                        }
                    }
                }
                connection.Close();
            }
        }

        internal static MagSimulation GetMagSimulation(SafeDataReader reader)
        {
            MagSimulation returnValue = new MagSimulation();
            returnValue.LoadProperty<Int32>(ReviewIdProperty, reader.GetInt32("REVIEW_ID"));
            returnValue.LoadProperty<Int32>(MagSimulationIdProperty, reader.GetInt32("MAG_SIMULATION_ID"));
            returnValue.LoadProperty<Int32>(YearProperty, reader.GetInt32("YEAR"));
            returnValue.LoadProperty<SmartDate>(CreatedDateProperty, reader.GetSmartDate("CREATED_DATE"));
            returnValue.LoadProperty<Int64>(WithThisAttributeIdProperty, reader.GetInt64("WITH_THIS_ATTRIBUTE_ID"));
            returnValue.LoadProperty<Int64>(FilteredByAttributeIdProperty, reader.GetInt64("FILTERED_BY_ATTRIBUTE_ID"));
            returnValue.LoadProperty<string>(SearchMethodProperty, reader.GetString("SEARCH_METHOD"));
            returnValue.LoadProperty<string>(NetworkStatisticProperty, reader.GetString("NETWORK_STATISTIC"));
            returnValue.LoadProperty<string>(StudyTypeClassifierProperty, reader.GetString("STUDY_TYPE_CLASSIFIER"));
            returnValue.LoadProperty<Int32>(UserClassifierModelIdProperty, reader.GetInt32("USER_CLASSIFIER_MODEL_ID"));
            returnValue.LoadProperty<string>(StatusProperty, reader.GetString("STATUS"));
            returnValue.LoadProperty<Int32>(TPProperty, reader.GetInt32("TP"));
            returnValue.LoadProperty<Int32>(FPProperty, reader.GetInt32("FP"));
            returnValue.LoadProperty<Int32>(FNProperty, reader.GetInt32("FN"));
            returnValue.LoadProperty<Int32>(NSeedsProperty, reader.GetInt32("NSEEDS"));
            returnValue.LoadProperty<string>(WithThisAttributeProperty, reader.GetString("WithThisAttribute"));
            returnValue.LoadProperty<string>(FilteredByAttributeProperty, reader.GetString("FilteredByAttribute"));
            returnValue.LoadProperty<string>(UserClassifierModelProperty, reader.GetString("MODEL_TITLE"));
            returnValue.MarkOld();
            return returnValue;
        }


        private void RunSimulation(int ReviewId, int ContactId)
        {
            MagCurrentInfo mci = MagCurrentInfo.GetMagCurrentInfoServerSide("LIVE");
            string uploadFileName = System.Web.HttpRuntime.AppDomainAppPath + @"UserTempUploads/" + TrainingRunCommand.NameBase + "_" +
                "Simulation" + MagSimulationId.ToString() + ".tsv";
            string folderPrefix = /*"experiment-v2/" +*/ TrainingRunCommand.NameBase + "_Sim" + this.MagSimulationId.ToString();
            WriteIdFile(ReviewId, ContactId, uploadFileName);
            UploadIdsFile(uploadFileName, folderPrefix);
            SubmitCreatTrainFileJob(ContactId, folderPrefix);
            SubmitCreatInferenceFileJob(ContactId, folderPrefix);
            
            if (MagContReviewPipeline.runADFPieline(ContactId, "Train.tsv",
                "Inference.tsv",
                "Results.tsv",
                "Sim" + this.MagSimulationId.ToString() + "per_paper_tfidf.pickle", mci.MagFolder, "0", folderPrefix, "0") == "Succeeded")
            {
                DownloadResults(folderPrefix, ReviewId);
            }
            else
            {
                // add log entry? (The ContReview object will already have logged an error)
            }

            // need to add cleaning up the files, but only once we've seen it in action for a while to help debugging
            DownloadResults(folderPrefix, ReviewId);
        }

        private void WriteIdFile(int ReviewId, int UserId, string fileName)
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_MagSimulationGetIds", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ReviewId));
                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ID_FILTER", this.FilteredByAttributeId));
                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ID_SEED", this.WithThisAttributeId));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        if (reader.Read())
                        {
                            using (StreamWriter file = new StreamWriter(fileName, false))
                            {
                                while (reader.Read())
                                {
                                    file.WriteLine(reader["PaperId"].ToString()+ "\t" + ReviewId.ToString() + "\t" + "1");
                                }
                            }
                        }
                    }
                }
            }
        }

        private void UploadIdsFile(string fileName, string FolderPrefix)
        {
            string storageAccountName = ConfigurationManager.AppSettings["MAGStorageAccount"];
            string storageAccountKey = ConfigurationManager.AppSettings["MAGStorageAccountKey"];

            string storageConnectionString =
                "DefaultEndpointsProtocol=https;AccountName=" + storageAccountName + ";AccountKey=";
            storageConnectionString += storageAccountKey;

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("experiments");
            CloudBlockBlob blockBlobData;

            blockBlobData = container.GetBlockBlobReference(FolderPrefix + "/SeedIds.tsv");
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

        private void SubmitCreatTrainFileJob(int ContactId, string folderPrefix)
        {
            MagCurrentInfo MagInfo = MagCurrentInfo.GetMagCurrentInfoServerSide("LIVE");
            MagDataLakeHelpers.ExecProc(@"[master].[dbo].[GenerateTrainFile](""" + folderPrefix + "/SeedIds.tsv\",\"" +
                folderPrefix + "/Train.tsv" + "\", \"" + MagInfo.MagFolder + "\",\"" + this.SearchMethod + "\"," +
                this.Year.ToString() + ");", true, "GenerateTrainFile", ContactId, 10);
        }

        private void SubmitCreatInferenceFileJob(int ContactId, string folderPrefix)
        {
            MagCurrentInfo MagInfo = MagCurrentInfo.GetMagCurrentInfoServerSide("LIVE");
            MagDataLakeHelpers.ExecProc(@"[master].[dbo].[GenerateInferenceFile](""" + folderPrefix + "/Train.tsv\",\"" +
                folderPrefix + "/Inference.tsv" + "\", \"" + MagInfo.MagFolder + "\",\"" + this.SearchMethod + "\"," +
                (this.WithThisAttributeId == 0 ? this.Year.ToString() : "1753") + ");", true, "GenerateInferenceFile", ContactId, 10);
        }

        private int DownloadResults(string folderPrefix, int ReviewId)
        {
            string storageAccountName = ConfigurationManager.AppSettings["MAGStorageAccount"];
            string storageAccountKey = ConfigurationManager.AppSettings["MAGStorageAccountKey"];

            string storageConnectionString =
                "DefaultEndpointsProtocol=https;AccountName=" + storageAccountName + ";AccountKey=";
            storageConnectionString += storageAccountKey;

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobClient blobClientIds = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer containerDown = blobClient.GetContainerReference("experiments");

            CloudBlockBlob blockBlobDownloadData = containerDown.GetBlockBlobReference(folderPrefix + "/Results.tsv");
            CloudBlockBlob blockBlobIds = containerDown.GetBlockBlobReference(folderPrefix + "/Train.tsv");
            byte[] myFile = Encoding.UTF8.GetBytes(blockBlobDownloadData.DownloadText());
            byte[] myFileIds = Encoding.UTF8.GetBytes(blockBlobIds.DownloadText());

            string SeedIds = "";
            MemoryStream msIds = new MemoryStream(myFileIds);
            using (var readerIds = new StreamReader(msIds))
            {
                string line;
                bool firstTime = true;
                while ((line = readerIds.ReadLine()) != null)
                {
                    string [] fields = line.Split('\t');
                    if (firstTime == true)
                    {
                        SeedIds = fields[0];
                        firstTime = false;
                    }
                    else
                    {
                        SeedIds += "," + fields[0];
                    }
                }
            }

            MemoryStream ms = new MemoryStream(myFile);

            DataTable dt = new DataTable("Ids");
            dt.Columns.Add("MAG_SIMULATION_RESULT_ID"); // can be anything
            dt.Columns.Add("MAG_SIMULATION_ID");
            dt.Columns.Add("PaperId");
            dt.Columns.Add("INCLUDED");
            dt.Columns.Add("FOUND");
            dt.Columns.Add("SEED");
            dt.Columns.Add("STUDY_TYPE_CLASSIFIER_SCORE");
            dt.Columns.Add("USER_CLASSIFIER_MODEL_SCORE");
            dt.Columns.Add("NETWORK_STATISTIC_SCORE");
            dt.Columns.Add("FOS_DISTANCE_SCORE");
            dt.Columns.Add("ENSEMBLE_SCORE");

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
                        string[] fields = line.Split('\t');
                        DataRow newRow = dt.NewRow();
                        newRow["MAG_SIMULATION_RESULT_ID"] = 0; // identity column
                        newRow["MAG_SIMULATION_ID"] = this.MagSimulationId;
                        newRow["PaperId"] = fields[0];
                        newRow["INCLUDED"] = "False";
                        newRow["FOUND"] = "False";
                        newRow["SEED"] = "False";
                        newRow["STUDY_TYPE_CLASSIFIER_SCORE"] = fields[2];
                        newRow["USER_CLASSIFIER_MODEL_SCORE"] = fields[2];
                        newRow["NETWORK_STATISTIC_SCORE"] = fields[2];
                        newRow["FOS_DISTANCE_SCORE"] = fields[2];
                        newRow["ENSEMBLE_SCORE"] = fields[2];
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
                    sbc.DestinationTableName = "TB_MAG_SIMULATION_RESULT";
                    sbc.BatchSize = 1000;
                    sbc.WriteToServer(dt);
                }

                using (SqlCommand command = new SqlCommand("st_MagSimulationUpdatePostRun", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@MAG_SIMULATION_ID", this.MagSimulationId));
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ReviewId));
                    command.Parameters.Add(new SqlParameter("@SeedIds", SeedIds));
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
            return lineCount;
        }



#endif
    }
}