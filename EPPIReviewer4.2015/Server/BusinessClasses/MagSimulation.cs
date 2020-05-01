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
using BusinessLibrary;
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
        public MagSimulation() { }
#endif

        public static readonly PropertyInfo<int> MagSimulationIdProperty = RegisterProperty<int>(new PropertyInfo<int>("MagSimulationId", "MagSimulationId", 0));
        public int MagSimulationId
        {
            get
            {
                return GetProperty(MagSimulationIdProperty);
            }
        }

        public static readonly PropertyInfo<int> ReviewIdProperty = RegisterProperty<int>(new PropertyInfo<int>("ReviewId", "ReviewId", 0));
        public int ReviewId
        {
            get
            {
                return GetProperty(ReviewIdProperty);
            }
        }

        public static readonly PropertyInfo<int> YearProperty = RegisterProperty<int>(new PropertyInfo<int>("Year", "Year"));
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

        public static readonly PropertyInfo<int> YearEndProperty = RegisterProperty<int>(new PropertyInfo<int>("YearEnd", "YearEnd"));
        public int YearEnd
        {
            get
            {
                return GetProperty(YearEndProperty);
            }
            set
            {
                SetProperty(YearEndProperty, value);
            }
        }

        public static readonly PropertyInfo<SmartDate> CreatedDateProperty = RegisterProperty<SmartDate>(new PropertyInfo<SmartDate>("CreatedDate", "CreatedDate"));
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

        public static readonly PropertyInfo<SmartDate> CreatedDateEndProperty = RegisterProperty<SmartDate>(new PropertyInfo<SmartDate>("CreatedDateEnd", "CreatedDateEnd"));
        public SmartDate CreatedDateEnd
        {
            get
            {
                return GetProperty(CreatedDateEndProperty);
            }
            set
            {
                SetProperty(CreatedDateEndProperty, value);
            }
        }

        public static readonly PropertyInfo<Int64> WithThisAttributeIdProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("WithThisAttributeId", "WithThisAttributeId"));
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

        public static readonly PropertyInfo<Int64> FilteredByAttributeIdProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("FilteredByAttributeId", "FilteredByAttributeId"));
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

        public static readonly PropertyInfo<string> SearchMethodProperty = RegisterProperty<string>(new PropertyInfo<string>("SearchMethod", "SearchMethod"));
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

        public static readonly PropertyInfo<string> NetworkStatisticProperty = RegisterProperty<string>(new PropertyInfo<string>("NetworkStatistic", "NetworkStatistic"));
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

        public static readonly PropertyInfo<string> StudyTypeClassifierProperty = RegisterProperty<string>(new PropertyInfo<string>("StudyTypeClassifier", "StudyTypeClassifier"));
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

        public static readonly PropertyInfo<int> UserClassifierModelIdProperty = RegisterProperty<int>(new PropertyInfo<int>("UserClassifierModelId", "UserClassifierModelId"));
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

        public static readonly PropertyInfo<int> UserClassifierReviewIdProperty = RegisterProperty<int>(new PropertyInfo<int>("UserClassifierReviewId", "UserClassifierReviewId"));
        public int UserClassifierReviewId
        {
            get
            {
                return GetProperty(UserClassifierReviewIdProperty);
            }
            set
            {
                SetProperty(UserClassifierReviewIdProperty, value);
            }
        }

        public static readonly PropertyInfo<string> StatusProperty = RegisterProperty<string>(new PropertyInfo<string>("Status", "Status", ""));
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

        public static readonly PropertyInfo<string> WithThisAttributeProperty = RegisterProperty<string>(new PropertyInfo<string>("WithThisAttribute", "WithThisAttribute", ""));
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

        public static readonly PropertyInfo<string> SeedTextProperty = RegisterProperty<string>(new PropertyInfo<string>("SeedText", "SeedText", ""));
        public string SeedText
        {
            get
            {
                if (GetProperty(YearProperty) != 1753)
                {
                    return "Pub before/end: " + GetProperty(YearProperty) + "/" + GetProperty(YearEndProperty);
                }
                if (GetProperty(CreatedDateProperty) != Convert.ToDateTime("1/1/1753"))
                {
                    return "Created before/end: " + GetProperty(CreatedDateProperty).ToString() + "/" + GetProperty(CreatedDateEndProperty);
                }
                return "With code: " + GetProperty(WithThisAttributeProperty);
            }
        }

        public static readonly PropertyInfo<string> FilteredByAttributeProperty = RegisterProperty<string>(new PropertyInfo<string>("FilteredByAttribute", "FilteredByAttribute", ""));
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

        public static readonly PropertyInfo<string> UserClassifierModelProperty = RegisterProperty<string>(new PropertyInfo<string>("UserClassifierModel", "UserClassifierModel", ""));
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

        public static readonly PropertyInfo<int> TPProperty = RegisterProperty<int>(new PropertyInfo<int>("TP", "TP"));
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

        public static readonly PropertyInfo<int> FPProperty = RegisterProperty<int>(new PropertyInfo<int>("FP", "FP"));
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

        public static readonly PropertyInfo<int> FNProperty = RegisterProperty<int>(new PropertyInfo<int>("FN", "FN"));
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

        public static readonly PropertyInfo<int> NSeedsProperty = RegisterProperty<int>(new PropertyInfo<int>("NSeeds", "NSeeds"));
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

        public static readonly PropertyInfo<float> PrecisionProperty = RegisterProperty<float>(new PropertyInfo<float>("Precision", "Precision"));
        public float Precision
        {
            get
            {
                if ((TP + FP)==0)
                {
                    return 0;
                }
                return TP / (TP + FP);
            }
        }

        public static readonly PropertyInfo<float> RecallProperty = RegisterProperty<float>(new PropertyInfo<float>("Recall", "Recall"));
        public float Recall
        {
            get
            {
                if ((TP + FN) == 0)
                {
                    return 0;
                }
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
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@YEAR", ReadProperty(YearProperty)));
                    command.Parameters.Add(new SqlParameter("@YEAR_END", ReadProperty(YearEndProperty)));
                    command.Parameters.Add(new SqlParameter("@CREATED_DATE", CreatedDate.DBValue));
                    command.Parameters.Add(new SqlParameter("@CREATED_DATE_END", CreatedDateEnd.DBValue));
                    command.Parameters.Add(new SqlParameter("@WITH_THIS_ATTRIBUTE_ID", ReadProperty(WithThisAttributeIdProperty)));
                    command.Parameters.Add(new SqlParameter("@FILTERED_BY_ATTRIBUTE_ID", ReadProperty(FilteredByAttributeIdProperty)));
                    command.Parameters.Add(new SqlParameter("@SEARCH_METHOD", ReadProperty(SearchMethodProperty)));
                    command.Parameters.Add(new SqlParameter("@NETWORK_STATISTIC", ReadProperty(NetworkStatisticProperty)));
                    command.Parameters.Add(new SqlParameter("@STUDY_TYPE_CLASSIFIER", ReadProperty(StudyTypeClassifierProperty)));
                    command.Parameters.Add(new SqlParameter("@USER_CLASSIFIER_MODEL_ID", ReadProperty(UserClassifierModelIdProperty)));
                    command.Parameters.Add(new SqlParameter("@USER_CLASSIFIER_REVIEW_ID", ReadProperty(UserClassifierReviewIdProperty)));
                    command.Parameters.Add(new SqlParameter("@STATUS", ReadProperty(StatusProperty)));
                    command.Parameters.Add(new SqlParameter("@MAG_SIMULATION_ID", newid));
                    command.Parameters["@MAG_SIMULATION_ID"].Direction = ParameterDirection.Output;
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
                            LoadProperty<Int32>(YearEndProperty, reader.GetInt32("YEAR_END"));
                            LoadProperty<SmartDate>(CreatedDateProperty, reader.GetSmartDate("CREATED_DATE"));
                            LoadProperty<SmartDate>(CreatedDateEndProperty, reader.GetSmartDate("CREATED_DATE_END"));
                            LoadProperty<Int64>(WithThisAttributeIdProperty, reader.GetInt64("WITH_THIS_ATTRIBUTE_ID"));
                            LoadProperty<Int64>(FilteredByAttributeIdProperty, reader.GetInt64("FILTERED_BY_ATTRIBUTE_ID"));
                            LoadProperty<string>(SearchMethodProperty, reader.GetString("SEARCH_METHOD"));
                            LoadProperty<string>(NetworkStatisticProperty, reader.GetString("NETWORK_STATISTIC"));
                            LoadProperty<string>(StudyTypeClassifierProperty, reader.GetString("STUDY_TYPE_CLASSIFIER"));
                            LoadProperty<Int32>(UserClassifierModelIdProperty, reader.GetInt32("USER_CLASSIFIER_MODEL_ID"));
                            LoadProperty<Int32>(UserClassifierReviewIdProperty, reader.GetInt32("USER_CLASSIFIER_REVIEW_ID"));
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
            returnValue.LoadProperty<Int32>(YearEndProperty, reader.GetInt32("YEAR_END"));
            returnValue.LoadProperty<SmartDate>(CreatedDateProperty, reader.GetSmartDate("CREATED_DATE"));
            returnValue.LoadProperty<SmartDate>(CreatedDateEndProperty, reader.GetSmartDate("CREATED_DATE_END"));
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


        private async void RunSimulation(int ReviewId, int ContactId)
        {
            MagCurrentInfo mci = MagCurrentInfo.GetMagCurrentInfoServerSide("LIVE");
            

#if (!CSLA_NETCORE)
            string uploadFileName = System.Web.HttpRuntime.AppDomainAppPath + @"UserTempUploads/Simulation" + MagSimulationId.ToString() + ".tsv";
            /*
            SetProperty(MagSimulationIdProperty, 245);
            UserClassifierModelId = 1;
            UserClassifierReviewId = 10;
            StudyTypeClassifier = "RCT";
            await AddClassifierScores(ReviewId.ToString());
            return;
            */
#else       
            string uploadFileName = "";
            if (Directory.Exists("UserTempUploads"))
            {
                uploadFileName =  @"./UserTempUploads/Simulation" + MagSimulationId.ToString() + ".tsv";
            }
            else
            {
                DirectoryInfo tmpDir = System.IO.Directory.CreateDirectory("UserTempUploads");
                uploadFileName = tmpDir.FullName + "/" + @"UserTempUploads/Simulation" + MagSimulationId.ToString() + ".tsv";

            }

#endif

            /*
            if (Directory.Exists("UserTempUploads"))
            {
                uploadFileName = @"UserTempUploads/" + TrainingRunCommand.NameBase + "_" +
                "Simulation" + MagSimulationId.ToString() + ".tsv";
            }
            else
            {
                DirectoryInfo tmpDir = System.IO.Directory.CreateDirectory("UserTempUploads");
                uploadFileName = tmpDir.FullName + "/" + @"UserTempUploads/" + TrainingRunCommand.NameBase + "_" +
                "Simulation" + MagSimulationId.ToString() + ".tsv";

            }
            */
            string folderPrefix = TrainingRunCommand.NameBase + "_Sim" + this.MagSimulationId.ToString();

            WriteIdFiles(ReviewId, ContactId, uploadFileName);
            await UploadIdsFileAsync(uploadFileName, folderPrefix);
            SubmitCreatTrainFileJob(ContactId, folderPrefix);
            SubmitCreatInferenceFileJob(ContactId, folderPrefix);

            if ((await CheckTrainAndInferenceFilesOk(folderPrefix)) == false)
            {
                MagLog.SaveLogEntry("Simulation", "Failed", "Rev:" + ReviewId.ToString() + " Training files not uploaded / empty", ContactId);
                return;
            }
            
            if (MagContReviewPipeline.runADFPieline(ContactId, "Train.tsv",
                "Inference.tsv",
                "Results.tsv",
                "Sim" + this.MagSimulationId.ToString() + "per_paper_tfidf.pickle", mci.MagFolder, "0", folderPrefix, "0",
                "Sim" + this.MagSimulationId.ToString(), "False") == "Succeeded")
            {
                await DownloadResultsAsync(folderPrefix, ReviewId);
                await AddClassifierScores(ReviewId.ToString());
            }
            else
            {
                // add log entry? (The ContReview object will already have logged an error)
            }
            
            // need to add cleaning up the files, but only once we've seen it in action for a while to help debugging
        }

        private void WriteIdFiles(int ReviewId, int UserId, string fileName)
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

        private async Task UploadIdsFileAsync(string fileName, string FolderPrefix)
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

            blockBlobData = container.GetBlockBlobReference(FolderPrefix + "/SeedIds.tsv");
            using (var fileStream = System.IO.File.OpenRead(fileName))
            {

                await blockBlobData.UploadFromStreamAsync(fileStream);
                
            }
            File.Delete(fileName);
        }

        private void SubmitCreatTrainFileJob(int ContactId, string folderPrefix)
        {
            MagCurrentInfo MagInfo = MagCurrentInfo.GetMagCurrentInfoServerSide("LIVE");
            MagDataLakeHelpers.ExecProc(@"[master].[dbo].[GenerateTrainFile](""" + folderPrefix + "/SeedIds.tsv\",\"" +
                folderPrefix + "/Train.tsv" + "\", \"" + MagInfo.MagFolder + "\",\"" + this.SearchMethod + "\"," +
                this.Year.ToString() + ",\"" + this.CreatedDate.ToString() + 
                "\");", true, "GenerateTrainFile", ContactId, 10);
        }

        private void SubmitCreatInferenceFileJob(int ContactId, string folderPrefix)
        {
            MagCurrentInfo MagInfo = MagCurrentInfo.GetMagCurrentInfoServerSide("LIVE");
            MagDataLakeHelpers.ExecProc(@"[master].[dbo].[GenerateInferenceFile](""" + folderPrefix + "/Train.tsv\",\"" +
                folderPrefix + "/Inference.tsv" + "\", \"" + MagInfo.MagFolder + "\",\"" + this.SearchMethod + "\"," +
                (this.WithThisAttributeId == 0 ? this.Year.ToString() : "1753") + ",\"" +
                (this.CreatedDate.Date.Year == 1753 ? "" : this.CreatedDate.ToString()) + "\"," +
                this.YearEnd.ToString() + ",\"" + this.CreatedDateEnd.ToString() + "\");",
                true, "GenerateInferenceFile", ContactId, 10);
        }

        private async Task<bool> CheckTrainAndInferenceFilesOk(string folderPrefix)
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
            CloudBlobContainer containerDown = blobClient.GetContainerReference("experiments");

            CloudBlockBlob blockBlobIds = containerDown.GetBlockBlobReference(folderPrefix + "/Train.tsv");
            CloudBlockBlob blockBlobInferenceIds = containerDown.GetBlockBlobReference(folderPrefix + "/Inference.tsv");
            try
            {
                await blockBlobIds.FetchAttributesAsync();
                await blockBlobInferenceIds.FetchAttributesAsync();
            }
            catch
            {
                return false;
            }
            if (blockBlobIds.Properties.Length > 0 && blockBlobInferenceIds.Properties.Length > 0)
                return true;
            else
                return false;
        }

        private async Task<int> DownloadResultsAsync(string folderPrefix, int ReviewId)
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
            CloudBlobContainer containerDown = blobClient.GetContainerReference("experiments");

            CloudBlockBlob blockBlobDownloadData = containerDown.GetBlockBlobReference(folderPrefix + "/Results.tsv");
            CloudBlockBlob blockBlobIds = containerDown.GetBlockBlobReference(folderPrefix + "/Train.tsv"); // we get the list of IDs so we know how many were selected by the datalake on publication / created date
            CloudBlockBlob blockBlobInferenceIds = containerDown.GetBlockBlobReference(folderPrefix + "/Inference.tsv");

            string resultantString = await blockBlobDownloadData.DownloadTextAsync();
            byte[] myFile = Encoding.UTF8.GetBytes(resultantString);

            string resultantStringFileIds = await blockBlobIds.DownloadTextAsync();
            byte[] myFileIds = Encoding.UTF8.GetBytes(resultantStringFileIds);

            string resultantStringInferenceIds = await blockBlobInferenceIds.DownloadTextAsync();
            byte[] myFileInferenceIds = Encoding.UTF8.GetBytes(resultantStringInferenceIds);

            string SeedIds = "";
            int SeedIdsCount = 0;
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
                    SeedIdsCount++;
                }
            }

            string InferenceIds = "";
            MemoryStream msInferenceIds = new MemoryStream(myFileInferenceIds);
            using (var readerInferenceIds = new StreamReader(msInferenceIds))
            {
                string line;
                bool firstTime = true;
                while ((line = readerInferenceIds.ReadLine()) != null)
                {
                    if (firstTime == true)
                    {
                        InferenceIds = line;
                        firstTime = false;
                    }
                    else
                    {
                        InferenceIds += "," + line;
                    }
                }
            }

            MemoryStream ms = new MemoryStream(myFile);

            DataTable dt = new DataTable("Ids");
            dt.Columns.Add("MAG_SIMULATION_RESULT_ID"); // can be anything - this is an identity column
            dt.Columns.Add("MAG_SIMULATION_ID");
            dt.Columns.Add("PaperId");
            dt.Columns.Add("INCLUDED");
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
                    command.Parameters.Add(new SqlParameter("@NSeeds", SeedIdsCount));
                    command.Parameters.Add(new SqlParameter("@InferenceIds", InferenceIds));
                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ID_FILTER", this.FilteredByAttributeId));
                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ID_SEED", this.WithThisAttributeId));
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
            return lineCount;
        }

        private async Task AddClassifierScores(string ReviewId)
        {
            // just return if we don't need to run the classifier(s)
            if (UserClassifierModelId == 0 && StudyTypeClassifier == "None")
            {
                return;
            }

            List<Int64> Ids = new List<long>();
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_MagSimulationResults", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@MagSimulationId", MagSimulationId));
                    command.Parameters.Add(new SqlParameter("@OrderBy", "Network"));
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
                return;

#if (!CSLA_NETCORE)

            string fName = System.Web.HttpRuntime.AppDomainAppPath + @"UserTempUploads/Sim" + MagSimulationId.ToString() + ".csv";
#else
                // same as comment above for same line
                //SG Edit:
                DirectoryInfo tmpDir = System.IO.Directory.CreateDirectory("UserTempUploads");
                string fName = tmpDir.FullName + "/Sim" + MagSimulationId.ToString() + ".csv";
                //string fName = AppDomain.CurrentDomain.BaseDirectory + TempPath + "ReviewID" + ri.ReviewId + "ContactId" + ri.UserId.ToString() + ".csv";
#endif

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
            // this copied from ClassifierCommand. The keys should move to web.config
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse("***REMOVED***");
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("attributemodeldata");
            CloudBlockBlob blockBlobData;

            string uploadedFileName = TrainingRunCommand.NameBase + "Sim" + this.MagSimulationId.ToString() + "StudyModelToScore.csv";
            string resultsFile1 = @"attributemodels/" + TrainingRunCommand.NameBase + "Sim" + this.MagSimulationId.ToString() + "Results1.csv";
            string resultsFile2 = @"attributemodels/" + TrainingRunCommand.NameBase + "Sim" + this.MagSimulationId.ToString() + "Results2.csv";
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

            if (this.StudyTypeClassifier != "None")
            {
                int classifierId = 0;
                string classifierName = "RCTModel";
                switch (StudyTypeClassifier)
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
                        classifierName = "NHSEED";
                        break;
                    case "Systematic review":
                        classifierId = -2;
                        classifierName = "DARE";
                        break;
                }
                await ClassifierCommand.InvokeBatchExecutionService(ReviewId, "ScoreModel", classifierId, @"attributemodeldata/" + uploadedFileName,
                    @"attributemodels/" + classifierName + ".csv", resultsFile1, resultsFile2);

                await insertResults(blobClient.GetContainerReference("attributemodels"), TrainingRunCommand.NameBase + "Sim" + this.MagSimulationId.ToString() + "Results1.csv", "STUDY_TYPE_CLASSIFIER_SCORE");
                if (classifierId == -4)
                {
                    await insertResults(blobClient.GetContainerReference("attributemodels"), TrainingRunCommand.NameBase + "Sim" + this.MagSimulationId.ToString() + "Results2.csv", "STUDY_TYPE_CLASSIFIER_SCORE");
                }
            }

            if (UserClassifierModelId > 0)
            {
                await ClassifierCommand.InvokeBatchExecutionService(ReviewId, "ScoreModel",  UserClassifierModelId, @"attributemodeldata/" + uploadedFileName,
                    @"attributemodels/" + TrainingRunCommand.NameBase + "ReviewId" + UserClassifierReviewId.ToString() + "ModelId" + UserClassifierModelId.ToString() + ".csv", resultsFile1, resultsFile2);
                await insertResults(blobClient.GetContainerReference("attributemodels"), TrainingRunCommand.NameBase + "Sim" + this.MagSimulationId.ToString() + "Results1.csv", "USER_CLASSIFIER_MODEL_SCORE");
            }
        }

        private async Task insertResults(CloudBlobContainer container, string FileName, string Field)
        {
            CloudBlockBlob blockBlobDataResults = container.GetBlockBlobReference(FileName);

            string Results1 = await blockBlobDataResults.DownloadTextAsync();
            byte[] myFile = Encoding.UTF8.GetBytes(Results1);

            DataTable dt = new DataTable("Ids");
            dt.Columns.Add("MAG_SIMULATION_ID");
            dt.Columns.Add("PaperId");
            dt.Columns.Add("SCORE");

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
                    newRow["MAG_SIMULATION_ID"] = this.MagSimulationId;
                    newRow["PaperId"] = fields[1];
                    newRow["SCORE"] = fields[0];
                    dt.Rows.Add(newRow);
                }
            }

            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlBulkCopy sbc = new SqlBulkCopy(connection))
                {
                    sbc.DestinationTableName = "TB_MAG_SIMULATION_CLASSIFIER_TEMP";
                    sbc.BatchSize = 1000;
                    sbc.WriteToServer(dt);
                }

                using (SqlCommand command = new SqlCommand("st_MagSimulationClassifierScoresUpdate", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@MAG_SIMULATION_ID", this.MagSimulationId));
                    command.Parameters.Add(new SqlParameter("@Field", Field));
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }




#endif
    }
}