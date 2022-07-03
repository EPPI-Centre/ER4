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
using System.Threading.Tasks;

//using Csla.Configuration;

#if !SILVERLIGHT
using System.Data.SqlClient;
using BusinessLibrary.Data;
using Csla.Data;
using BusinessLibrary.Security;
using System.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Data;
using System.Threading;
#if !CSLA_NETCORE
using System.Web.Hosting;
#endif
#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class MagRelatedPapersRun : BusinessBase<MagRelatedPapersRun>
    {
#if SILVERLIGHT
    public MagRelatedPapersRun() { }

        
#else
        public MagRelatedPapersRun() { }
#endif

        public static readonly PropertyInfo<int> MagRelatedRunIdProperty = RegisterProperty<int>(new PropertyInfo<int>("MagRelatedRunId", "MagRelatedRunId", 0));
        public int MagRelatedRunId
        {
            get
            {
                return GetProperty(MagRelatedRunIdProperty);
            }
        }

        public static readonly PropertyInfo<int> ReviewIdIdProperty = RegisterProperty<int>(new PropertyInfo<int>("ReviewIdId", "ReviewIdId", 0));
        public int ReviewIdId
        {
            get
            {
                return GetProperty(ReviewIdIdProperty);
            }
        }

        public static readonly PropertyInfo<string> UserDescriptionProperty = RegisterProperty<string>(new PropertyInfo<string>("UserDescription", "UserDescription", string.Empty));
        public string UserDescription
        {
            get
            {
                return GetProperty(UserDescriptionProperty);
            }
            set
            {
                SetProperty(UserDescriptionProperty, value);
            }
        }

        public static readonly PropertyInfo<Int64> AttributeIdProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("AttributeId", "AttributeId"));
        public Int64 AttributeId
        {
            get
            {
                return GetProperty(AttributeIdProperty);
            }
            set
            {
                SetProperty(AttributeIdProperty, value);
            }
        }

        public static readonly PropertyInfo<string> AttributeNameProperty = RegisterProperty<string>(new PropertyInfo<string>("AttributeName", "AttributeName"));
        public string AttributeName
        {
            get
            {
                return GetProperty(AttributeNameProperty);
            }
            set
            {
                SetProperty(AttributeNameProperty, value);
            }
        }

        public static readonly PropertyInfo<bool> AllIncludedProperty = RegisterProperty<bool>(new PropertyInfo<bool>("AllIncluded", "AllIncluded", false));
        public bool AllIncluded
        {
            get
            {
                return GetProperty(AllIncludedProperty);
            }
            set
            {
                SetProperty(AllIncludedProperty, value);
            }
        }

        public static readonly PropertyInfo<SmartDate> DateFromProperty = RegisterProperty<SmartDate>(new PropertyInfo<SmartDate>("DateFrom", "DateFrom"));
        public SmartDate DateFrom
        {
            get
            {
                return GetProperty(DateFromProperty);
            }
            set
            {
                SetProperty(DateFromProperty, value);
            }
        }

        public static readonly PropertyInfo<SmartDate> DateRunProperty = RegisterProperty<SmartDate>(new PropertyInfo<SmartDate>("DateRun", "DateRun"));
        public SmartDate DateRun
        {
            get
            {
                return GetProperty(DateRunProperty);
            }
            set
            {
                SetProperty(DateRunProperty, value);
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

        public static readonly PropertyInfo<string> UserStatusProperty = RegisterProperty<string>(new PropertyInfo<string>("UserStatus", "UserStatus", ""));
        public string UserStatus
        {
            get
            {
                return GetProperty(UserStatusProperty);
            }
            set
            {
                SetProperty(UserStatusProperty, value);
            }
        }
        public static readonly PropertyInfo<int> NPapersProperty = RegisterProperty<int>(new PropertyInfo<int>("NPapers", "NPapers", 0));
        public int NPapers
        {
            get
            {
                return GetProperty(NPapersProperty);
            }
            set
            {
                SetProperty(NPapersProperty, value);
            }
        }
        public static readonly PropertyInfo<string> ModeProperty = RegisterProperty<string>(new PropertyInfo<string>("Mode", "Mode"));
        public string Mode
        {
            get
            {
                return GetProperty(ModeProperty);
            }
            set
            {
                SetProperty(ModeProperty, value);
            }
        }

        public static readonly PropertyInfo<string> FilteredProperty = RegisterProperty<string>(new PropertyInfo<string>("Filtered", "Filtered"));
        public string Filtered
        {
            get
            {
                return GetProperty(FilteredProperty);
            }
            set
            {
                SetProperty(FilteredProperty, value);
            }
        }

        public static readonly PropertyInfo<string> PmidsProperty = RegisterProperty<string>(new PropertyInfo<string>("Pmids", "Pmids"));
        public string Pmids
        {
            get
            {
                return GetProperty(PmidsProperty);
            }
            set
            {
                SetProperty(PmidsProperty, value);
            }
        }
        
        public static readonly PropertyInfo<bool> AutoReRunProperty = RegisterProperty<bool>(new PropertyInfo<bool>("AutoReRun", "AutoReRun", false));
        public bool AutoReRun
        {
            get
            {
                return GetProperty(AutoReRunProperty);
            }
            set
            {
                SetProperty(AutoReRunProperty, value);
            }
        }

        



        //protected override void AddAuthorizationRules()
        //{
        //    //string[] canWrite = new string[] { "AdminUser", "RegularUser" };
        //    //string[] canRead = new string[] { "AdminUser", "RegularUser", "ReadOnlyUser" };
        //    //string[] admin = new string[] { "AdminUser" };
        //    //AuthorizationRules.AllowCreate(typeof(MagRelatedPapersRun), admin);
        //    //AuthorizationRules.AllowDelete(typeof(MagRelatedPapersRun), admin);
        //    //AuthorizationRules.AllowEdit(typeof(MagRelatedPapersRun), canWrite);
        //    //AuthorizationRules.AllowGet(typeof(MagRelatedPapersRun), canRead);

        //    //AuthorizationRules.AllowRead(MagRelatedPapersRunIdProperty, canRead);
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
            if (Mode == "")
            {
                // Once UI behaves this shouldn't be possible
                return;
            }
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_MagRelatedPapersRunsInsert", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@USER_DESCRIPTION", ReadProperty(UserDescriptionProperty)));
                    command.Parameters.Add(new SqlParameter("@PaperIdList", ""));
                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ID", ReadProperty(AttributeIdProperty)));
                    command.Parameters.Add(new SqlParameter("@ALL_INCLUDED", ReadProperty(AllIncludedProperty)));
                    command.Parameters.Add(new SqlParameter("@DATE_FROM", DateFrom.DBValue));
                    //command.Parameters.Add(new SqlParameter("@AUTO_RERUN", ReadProperty(AutoReRunProperty)));
                    command.Parameters.Add(new SqlParameter("@MODE", ReadProperty(ModeProperty)));
                    command.Parameters.Add(new SqlParameter("@FILTERED", ReadProperty(FilteredProperty)));
                    command.Parameters.Add(new SqlParameter("@STATUS", ReadProperty(StatusProperty)));
                    command.Parameters.Add(new SqlParameter("@MAG_RELATED_RUN_ID", ReadProperty(MagRelatedRunIdProperty)));
                    command.Parameters["@MAG_RELATED_RUN_ID"].Direction = ParameterDirection.Output;
                    command.ExecuteNonQuery();
                    LoadProperty(MagRelatedRunIdProperty, command.Parameters["@MAG_RELATED_RUN_ID"].Value);

                    // Run in separate thread and return this object to client
                    if (this.Mode != "New items in MAG") // This mode is likely never used now, so the if can go
                    {
#if CSLA_NETCORE
            System.Threading.Tasks.Task.Run(() => RunOpenAlexRelatedPapersRun(ri.UserId, ri.ReviewId));
#else
                        //see: https://codingcanvas.com/using-hostingenvironment-queuebackgroundworkitem-to-run-background-tasks-in-asp-net/
                        HostingEnvironment.QueueBackgroundWorkItem(cancellationToken => RunOpenAlexRelatedPapersRun(ri.UserId, ri.ReviewId, cancellationToken));
#endif
                    }
                }
                connection.Close();
            }
        }

        protected override void DataPortal_Update()
        {
            if (this.UserDescription != "")
            {
                using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand("st_MagRelatedPapersRunsUpdate", connection))
                    {
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@MAG_RELATED_RUN_ID", ReadProperty(MagRelatedRunIdProperty)));
                        //command.Parameters.Add(new SqlParameter("@AUTO_RERUN", ReadProperty(AutoReRunProperty)));
                        command.Parameters.Add(new SqlParameter("@USER_DESCRIPTION", ReadProperty(UserDescriptionProperty)));
                        command.ExecuteNonQuery();
                    }
                    connection.Close();
                }
            }
        }

        protected override void DataPortal_DeleteSelf()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_MagRelatedPapersRunsDelete", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@MAG_RELATED_RUN_ID", ReadProperty(MagRelatedRunIdProperty)));
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        protected void DataPortal_Fetch(SingleCriteria<MagRelatedPapersRun, Int64> criteria)
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_MagRelatedPapersRun", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@MAG_RELATED_RUN_ID", criteria.Value));
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        if (reader.Read())
                        {
                            LoadProperty<Int32>(MagRelatedRunIdProperty, reader.GetInt32("MAG_RELATED_RUN_ID"));
                            LoadProperty<Int32>(ReviewIdIdProperty, reader.GetInt32("REVIEW_ID"));
                            LoadProperty<string>(UserDescriptionProperty, reader.GetString("USER_DESCRIPTION"));
                            LoadProperty<Int64>(AttributeIdProperty, reader.GetInt64("ATTRIBUTE_ID"));
                            LoadProperty<string>(AttributeNameProperty, reader.GetString("ATTRIBUTE_NAME"));
                            LoadProperty<bool>(AllIncludedProperty, reader.GetBoolean("ALL_INCLUDED"));
                            LoadProperty<SmartDate>(DateFromProperty, reader.GetSmartDate("DATE_FROM"));
                            LoadProperty<SmartDate>(DateRunProperty, reader.GetSmartDate("DATE_RUN"));
                            //LoadProperty<bool>(AutoReRunProperty, reader.GetBoolean("AUTO_RERUN"));
                            LoadProperty<string>(StatusProperty, reader.GetString("STATUS"));
                            LoadProperty<string>(UserStatusProperty, reader.GetString("USER_STATUS"));
                            LoadProperty<int>(NPapersProperty, reader.GetInt32("N_PAPERS"));
                            LoadProperty<string>(ModeProperty, reader.GetString("MODE"));
                            LoadProperty<string>(FilteredProperty, reader.GetString("FILTERED"));
                        }
                    }
                }
                connection.Close();
            }
        }

        internal static MagRelatedPapersRun GetMagRelatedPapersRun(SafeDataReader reader)
        {
            MagRelatedPapersRun returnValue = new MagRelatedPapersRun();
            returnValue.LoadProperty<Int32>(MagRelatedRunIdProperty, reader.GetInt32("MAG_RELATED_RUN_ID"));
            returnValue.LoadProperty<Int32>(ReviewIdIdProperty, reader.GetInt32("REVIEW_ID"));
            returnValue.LoadProperty<string>(UserDescriptionProperty, reader.GetString("USER_DESCRIPTION"));
            returnValue.LoadProperty<Int64>(AttributeIdProperty, reader.GetInt64("ATTRIBUTE_ID"));
            returnValue.LoadProperty<string>(AttributeNameProperty, reader.GetString("ATTRIBUTE_NAME"));
            returnValue.LoadProperty<bool>(AllIncludedProperty, reader.GetBoolean("ALL_INCLUDED"));
            returnValue.LoadProperty<SmartDate>(DateFromProperty, reader.GetSmartDate("DATE_FROM"));
            returnValue.LoadProperty<SmartDate>(DateRunProperty, reader.GetSmartDate("DATE_RUN"));
            //returnValue.LoadProperty<bool>(AutoReRunProperty, reader.GetBoolean("AUTO_RERUN"));
            returnValue.LoadProperty<string>(StatusProperty, reader.GetString("STATUS"));
            returnValue.LoadProperty<string>(UserStatusProperty, reader.GetString("USER_STATUS"));
            returnValue.LoadProperty<int>(NPapersProperty, reader.GetInt32("N_PAPERS"));
            returnValue.LoadProperty<string>(ModeProperty, reader.GetString("MODE"));
            returnValue.LoadProperty<string>(FilteredProperty, reader.GetString("FILTERED"));
            returnValue.MarkOld();
            return returnValue;
        }

        private async void RunOpenAlexRelatedPapersRun(int ContactId, int ReviewId, CancellationToken cancellationToken = default(CancellationToken))
        {
            // 1. Get a list of the Seed IDs
            List<string> Ids = new List<string>(); 
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_MagRelatedPapersRunGetSeedIds", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@MAG_RELATED_RUN_ID", this.MagRelatedRunId));
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ReviewId));
                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ID", this.AttributeId));
                    using (SafeDataReader reader = new SafeDataReader(command.ExecuteReader()))
                    {
                        while (reader.Read())
                        {
                            Ids.Add(reader.GetInt64("PaperId").ToString());
                        }
                    }
                }
                connection.Close();
            }
            // 1.5: check that there aren't crazy numbers of seed items!
            if (Ids.Count > 500)
            {
                using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand("st_MagRelatedPapersRunsUpdatePostRun", connection))
                    {
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@MAG_RELATED_RUN_ID", ReadProperty(MagRelatedRunIdProperty)));
                        command.Parameters.Add(new SqlParameter("@N_PAPERS", 0));
                        command.ExecuteNonQuery();
                    }
                    connection.Close();
                }
                return;
            }

            // 2. Look up each of the IDs in OpenAlex and download the relevant data
            List<MagMakesHelpers.OaPaper> seeds = MagMakesHelpers.downloadTheseOpenAlexPapers(Ids.ToArray());
            List<string> results = new List<string>();
            if (seeds.Count > 0)
            {
                foreach (MagMakesHelpers.OaPaper pm in seeds)
                {
                    if (pm.id != null)
                    {
                        switch (this.Mode)
                        {
                            case "Recommended by":
                                foreach (string s in pm.related_works)
                                {
                                    addUniqueToList(results, s.Replace("https://openalex.org/W", ""));
                                    
                                }
                                results = DoDateFilter(results);
                                break;
                            case "Bibliography":
                                foreach (string s in pm.referenced_works)
                                {
                                    addUniqueToList(results, s.Replace("https://openalex.org/W", ""));
                                }
                                results = DoDateFilter(results);
                                break;
                            case "Cited by":
                                getCitedWorks(pm.id, results);
                                break;
                            case "BiCitation":
                                foreach (string s in pm.referenced_works)
                                {
                                    addUniqueToList(results, s.Replace("https://openalex.org/W", ""));
                                }
                                results = DoDateFilter(results);
                                getCitedWorks(pm.id, results);
                                break;
                            case "Bi-Citation AND Recommendations":
                                foreach (string s in pm.referenced_works)
                                {
                                    addUniqueToList(results, s.Replace("https://openalex.org/W", ""));
                                }
                                foreach (string s in pm.related_works)
                                {
                                    addUniqueToList(results, s.Replace("https://openalex.org/W", ""));
                                }
                                results = DoDateFilter(results);
                                getCitedWorks(pm.id, results);
                                break;
                        }
                    }
                }
                

            
                // 3. save the IDs in the database
                DataTable dt = new DataTable("Ids");
                dt.Columns.Add("MAG_RELATED_PAPERS_ID");
                dt.Columns.Add("REVIEW_ID");
                dt.Columns.Add("MAG_RELATED_RUN_ID");
                dt.Columns.Add("PaperId");
                dt.Columns.Add("SimilarityScore");

                foreach (string s in results)
                {
                    DataRow newRow = dt.NewRow();
                    newRow["MAG_RELATED_PAPERS_ID"] = 0; // doesn't matter what is in here - it's auto-assigned in the database
                    newRow["REVIEW_ID"] = ReviewId;
                    newRow["MAG_RELATED_RUN_ID"] = this.MagRelatedRunId;
                    newRow["PaperId"] = s;
                    newRow["SimilarityScore"] = 0;
                    dt.Rows.Add(newRow);
                }
                
                using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
                {
                    connection.Open();
                    using (SqlBulkCopy sbc = new SqlBulkCopy(connection))
                    {
                        sbc.DestinationTableName = "TB_MAG_RELATED_PAPERS";
                        sbc.BatchSize = 1000;
                        sbc.WriteToServer(dt);
                    }

                    using (SqlCommand command = new SqlCommand("st_MagRelatedPapersRunsUpdatePostRun", connection))
                    {
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@MAG_RELATED_RUN_ID", ReadProperty(MagRelatedRunIdProperty)));
                        command.Parameters.Add(new SqlParameter("@N_PAPERS", dt.Rows.Count));
                        command.ExecuteNonQuery();
                    }
                    connection.Close();
                }
            }
            else
            { // nothing in the seeds, so nothing in the results (UI should prevent this)
                using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
                {
                    connection.Open();
                    
                    using (SqlCommand command = new SqlCommand("st_MagRelatedPapersRunsUpdatePostRun", connection))
                    {
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@MAG_RELATED_RUN_ID", ReadProperty(MagRelatedRunIdProperty)));
                        command.Parameters.Add(new SqlParameter("@N_PAPERS", (int)0));
                        command.ExecuteNonQuery();
                    }
                    connection.Close();
                }
            }
        }

        private void addUniqueToList(List<string> Ids, string Id)
        {
            if (!Ids.Contains(Id))
            {
                Ids.Add(Id);
            }
        }

        private void getCitedWorks(string Id, List<string> results)
        {
            string query = "cites:" + Id;
            if (this.DateFrom != null)
            {
                query += ",from_publication_date:" + DateFrom.Date.Year.ToString() + "-" + DateFrom.Date.Month.ToString("D2") + "-" + DateFrom.Date.Day.ToString("D2");
            }
            List<MagMakesHelpers.OaPaperFilterResult> res = MagMakesHelpers.downloadOaPaperFilterUsingCursor(query, false);
            foreach (MagMakesHelpers.OaPaperFilterResult fr in res)
            {
                foreach (MagMakesHelpers.OaPaper p in fr.results)
                {
                    addUniqueToList(results, p.id.Replace("https://openalex.org/W", ""));
                }
            }
        }

        private List<string> DoDateFilter(List<string> results)
        {
            if (DateFrom == null)
            {
                return results;
            }
            else
            {
                List<MagMakesHelpers.OaPaper> papers = MagMakesHelpers.downloadTheseOpenAlexPapers(results.ToArray());
                results.Clear();
                foreach (MagMakesHelpers.OaPaper p in papers)
                {
                    if (Convert.ToDateTime(p.publication_date) >= DateFrom.Date)
                    {
                        addUniqueToList(results, p.IdInteger);
                    }
                }
                return results;
            }
        }

        private async void RunMagRelatedPapersRun(int ContactId, int ReviewId, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {

#if (!CSLA_NETCORE)
                //string folderPrefix = TrainingRunCommand.NameBase;
                string uploadFileName = System.Web.HttpRuntime.AppDomainAppPath + @"UserTempUploads/" + TrainingRunCommand.NameBase +
                    "RelatedRun" + MagRelatedRunId.ToString() + ".csv";
                string downloadFilename = TrainingRunCommand.NameBase +
                    "RelatedRun" + MagRelatedRunId.ToString() + ".csv"; ;

#else
            string downloadFilename = TrainingRunCommand.NameBase + "RelatedRun" + MagRelatedRunId.ToString() + ".csv";
            string uploadFileName = "";
            if (Directory.Exists("UserTempUploads"))
            {
                uploadFileName =  @"./UserTempUploads/" + TrainingRunCommand.NameBase + "RelatedRun" + MagRelatedRunId.ToString() + ".csv";
            }
            else
            {
                DirectoryInfo tmpDir = System.IO.Directory.CreateDirectory("UserTempUploads");
                uploadFileName = tmpDir.FullName + "/" + @"UserTempUploads/" + TrainingRunCommand.NameBase + "RelatedRun" + MagRelatedRunId.ToString() + ".csv";

            }

#endif

                if (!WriteSeedIdsFile(uploadFileName, ReviewId))
                {
                    UpdateMAGRelatedRecord("No seed Ids written", "Stopped", ReviewId);
                    return;
                }
                if (IsThreadCancelled(cancellationToken, ReviewId))
                    return;
                if (!await UploadSeedIdsFileAsync(uploadFileName, ReviewId))
                {
                    UpdateMAGRelatedRecord("No seed Ids uploaded", "Stopped", ReviewId);
                    return;
                }
                if (IsThreadCancelled(cancellationToken, ReviewId))
                    return;
                TriggerDataLakeJob(uploadFileName, ContactId, cancellationToken);
                if (IsThreadCancelled(cancellationToken, ReviewId))
                    return;
                if ((await CheckResultsFileOk(downloadFilename)) == false)
                {
                    UpdateMAGRelatedRecord("No results", "Finished", ReviewId);
                    return;
                }
                if (IsThreadCancelled(cancellationToken, ReviewId))
                    return;
                if (!await DownloadResultsAsync(downloadFilename, ReviewId))
                {
                    UpdateMAGRelatedRecord("Download failed", "Finished", ReviewId);
                    return;
                }
                if (IsThreadCancelled(cancellationToken, ReviewId))
                    return;
            }
            catch (Exception e)
            {
                try
                {
                    string msg = "Error: " + e.Message;
                    if (msg.Length > 50) msg = msg.Substring(0, 50);
                    UpdateMAGRelatedRecord(msg, "Failed", ReviewId);
                }
                catch { }
            }
        }

        private bool IsThreadCancelled(CancellationToken cancellationToken, int ReviewId)
        {

            //Random r = new Random();
            //if (r.Next(1, 3) > 1) throw new Exception("what happens now??");
            if (cancellationToken.IsCancellationRequested)
            {
                UpdateMAGRelatedRecord("Thread cancelled", "Stopped", ReviewId);
                return true;
            }
            return false;
        }

        private async Task<bool> CheckResultsFileOk(string resultsFile)
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
            CloudBlobContainer containerDown = blobClient.GetContainerReference("results");

            CloudBlockBlob blockBlobResults = containerDown.GetBlockBlobReference(resultsFile);
            try
            {
                await blockBlobResults.FetchAttributesAsync();
                if (blockBlobResults.Properties.Length > 0)
                    return true;
                else
                    return false;
            }
            catch
            {
                return false;
            }
        }

        private void UpdateMAGRelatedRecord(string userStatus, string Status, int ReviewId)
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_MagRelatedRun_Update", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@USER_STATUS", userStatus));
                    command.Parameters.Add(new SqlParameter("@STATUS", Status));
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ReviewId));
                    command.Parameters.Add(new SqlParameter("@MAG_RELATED_RUN_ID", this.MagRelatedRunId));
                    
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        private bool WriteSeedIdsFile(string uploadFileName, int ReviewId)
        {
            int c = 0;
            
            try
            {
                if (this.Mode != "PubMed ID search")
                {
                    using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand("st_MagRelatedPapersRunGetSeedIds", connection))
                        {
                            command.CommandType = System.Data.CommandType.StoredProcedure;
                            command.Parameters.Add(new SqlParameter("@MAG_RELATED_RUN_ID", this.MagRelatedRunId));
                            command.Parameters.Add(new SqlParameter("@REVIEW_ID", ReviewId));
                            command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ID", this.AttributeId));
                            using (SafeDataReader reader = new SafeDataReader(command.ExecuteReader()))
                            {
                                using (StreamWriter file = new StreamWriter(uploadFileName, false))
                                {
                                    while (reader.Read())
                                    {
                                        file.WriteLine(reader["PaperId"].ToString());
                                        c++;
                                    }
                                }
                            }
                        }
                        connection.Close();
                    }
                }
                else
                {
                    using (StreamWriter file = new StreamWriter(uploadFileName, false)) 
                    {
                        file.WriteLine("Pmid");
                        foreach (string s in this.Pmids.Split(','))
                        {
                            file.WriteLine(s.Trim());
                            c++;
                        }
                    }
                }
            }
            catch
            {
                return false;
            }
            if (c > 0)
                return true;
            else
                return false;
        }


        private async Task<bool> UploadSeedIdsFileAsync(string fileName, int ReviewId)
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

            try
            {
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                CloudBlobContainer container = blobClient.GetContainerReference("uploads");
                CloudBlockBlob blockBlobData;

                blockBlobData = container.GetBlockBlobReference(Path.GetFileName(fileName));
                using (var fileStream = System.IO.File.OpenRead(fileName))
                {


#if (!CSLA_NETCORE)
                    blockBlobData.UploadFromStream(fileStream);
#else

                await blockBlobData.UploadFromFileAsync(fileName);
#endif

                }
                File.Delete(fileName);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void TriggerDataLakeJob(string uploadFileName, int ContactId, CancellationToken cancellationToken)
        {
            MagCurrentInfo MagInfo = MagCurrentInfo.GetMagCurrentInfoServerSide("LIVE");
            if (this.Mode == "PubMed ID search")
            {
                MagDataLakeHelpers.ExecProc(@"[master].[dbo].[GetPaperIdsFromPmids](""" + Path.GetFileName(uploadFileName) + "\",\"" +
                    Path.GetFileName(uploadFileName) + "\", \"OpenAlexData/" + MagInfo.MagFolder + "\");", true, "RelatedRunPmid",
                    ContactId, 3, cancellationToken);
            }
            else
            {
                
                MagDataLakeHelpers.ExecProc(@"[master].[dbo].[RelatedRun](""" + Path.GetFileName(uploadFileName) + "\",\"" +
                    Path.GetFileName(uploadFileName) + "\", \"OpenAlexData/" + MagInfo.MagFolder + "\",\"" + this.Mode + "\"," +
                    (this.DateFrom.ToString() != "" ? DateFrom.Date.Year.ToString() : "1753") + ");", true, "RelatedRun",
                    ContactId, 10, cancellationToken);
            }
        }

        private async Task<bool> DownloadResultsAsync(string fileName, int ReviewId)
        {

#if (CSLA_NETCORE)

            var configuration = ERxWebClient2.Startup.Configuration.GetSection("AzureMagSettings");
            string storageAccountName = configuration["MAGStorageAccount"];
            string storageAccountKey = configuration["MAGStorageAccountKey"];

#else
            string storageAccountName = ConfigurationManager.AppSettings["MAGStorageAccount"];
            string storageAccountKey = ConfigurationManager.AppSettings["MAGStorageAccountKey"];
#endif
            try
            {
                string storageConnectionString =
                    "DefaultEndpointsProtocol=https;AccountName=" + storageAccountName + ";AccountKey=";
                storageConnectionString += storageAccountKey;

                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                CloudBlobContainer containerUp = blobClient.GetContainerReference("uploads");
                CloudBlobContainer containerDown = blobClient.GetContainerReference("results");

                // cleaning up the file that was uploaded
                CloudBlockBlob blockBlobUploadData = containerUp.GetBlockBlobReference(Path.GetFileName(fileName));
                //blockBlobUploadData.DeleteIfExistsAsync();

                CloudBlockBlob blockBlobDownloadData = containerDown.GetBlockBlobReference(Path.GetFileName(fileName));
                string resultantString = await blockBlobDownloadData.DownloadTextAsync();
                byte[] myFile = Encoding.UTF8.GetBytes(resultantString);

                MemoryStream ms = new MemoryStream(myFile);

                DataTable dt = new DataTable("Ids");
                dt.Columns.Add("MAG_RELATED_PAPERS_ID");
                dt.Columns.Add("REVIEW_ID");
                dt.Columns.Add("MAG_RELATED_RUN_ID");
                dt.Columns.Add("PaperId");
                dt.Columns.Add("SimilarityScore");
                //dt.Columns.Add("PARENT_MAG_RELATED_RUN_ID");

                using (var reader = new StreamReader(ms))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        DataRow newRow = dt.NewRow();
                        newRow["MAG_RELATED_PAPERS_ID"] = 0; // doesn't matter what is in here - it's auto-assigned in the database
                        newRow["REVIEW_ID"] = ReviewId;
                        newRow["MAG_RELATED_RUN_ID"] = this.MagRelatedRunId;
                        newRow["PaperId"] = Convert.ToInt64(line);
                        newRow["SimilarityScore"] = 0;
                        //newRow["PARENT_MAG_RELATED_RUN_ID"] = DBNull.Value;
                        dt.Rows.Add(newRow);
                    }
                }

                using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
                {
                    connection.Open();
                    using (SqlBulkCopy sbc = new SqlBulkCopy(connection))
                    {
                        sbc.DestinationTableName = "TB_MAG_RELATED_PAPERS";
                        sbc.BatchSize = 1000;
                        sbc.WriteToServer(dt);
                    }

                    using (SqlCommand command = new SqlCommand("st_MagRelatedPapersRunsUpdatePostRun", connection))
                    {
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@MAG_RELATED_RUN_ID", ReadProperty(MagRelatedRunIdProperty)));
                        command.Parameters.Add(new SqlParameter("@N_PAPERS", dt.Rows.Count));
                        command.ExecuteNonQuery();
                    }
                    connection.Close();
                }
                return true;
            }
            catch
            {
                return false;
            }
        }


#endif
    }
}