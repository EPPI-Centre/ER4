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

#if!SILVERLIGHT
using System.Data.SqlClient;
using BusinessLibrary.Data;
using Csla.Data;
using BusinessLibrary.Security;
#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class ReviewInfo : BusinessBase<ReviewInfo>
    {
        public static void GetReviewInfo(EventHandler<DataPortalResult<ReviewInfo>> handler)
        {
            DataPortal<ReviewInfo> dp = new DataPortal<ReviewInfo>();
            dp.FetchCompleted += handler;
            dp.BeginFetch();
        }

#if SILVERLIGHT
        public ReviewInfo() { }

        public bool UserCanEditScreeningSetting
        {
            get
            {
                BusinessLibrary.Security.ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as BusinessLibrary.Security.ReviewerIdentity;
                return (ri.Roles.Contains("AdminUser") || ri.IsSiteAdmin) && ri.HasWriteRights();
            }
        }
#else
        public ReviewInfo() { }
#endif

        public static readonly  PropertyInfo<int> ReviewIdProperty = RegisterProperty<int>(new PropertyInfo<int>("ReviewId", "ReviewId", 0));
        public int ReviewId
        {
            get
            {
                return GetProperty(ReviewIdProperty);
            }
            set
            {
                SetProperty(ReviewIdProperty, value);
            }
        }

        public static readonly  PropertyInfo<string> ReviewNameProperty = RegisterProperty<string>(new PropertyInfo<string>("ReviewName", "ReviewName", string.Empty));
        public string ReviewName
        {
            get
            {
                return GetProperty(ReviewNameProperty);
            }
            set
            {
                SetProperty(ReviewNameProperty, value);
            }
        }

        public static readonly PropertyInfo<bool> ShowScreeningProperty = RegisterProperty<bool>(new PropertyInfo<bool>("ShowScreening", "ShowScreening"));
        public bool ShowScreening
        {
            get
            {
                return GetProperty<bool>(ShowScreeningProperty);
            }
            set
            {
                SetProperty(ShowScreeningProperty, value);
            }
        }
        public static readonly  PropertyInfo<int> ScreeningCodeSetIdProperty = RegisterProperty<int>(new PropertyInfo<int>("ScreeningCodeSetId", "ScreeningCodeSetId Id", 0));
        public int ScreeningCodeSetId
        {
            get
            {
                return GetProperty(ScreeningCodeSetIdProperty);
            }
            set
            {
                SetProperty(ScreeningCodeSetIdProperty, value);
            }
        }
        public static readonly  PropertyInfo<string> ScreeningModeProperty = RegisterProperty<string>(new PropertyInfo<string>("ScreeningMode", "ScreeningMode Id", ""));
        public string ScreeningMode
        {
            get
            {
                return GetProperty(ScreeningModeProperty);
            }
            set
            {
                SetProperty(ScreeningModeProperty, value);
            }
        }
        public static readonly  PropertyInfo<string> ScreeningReconcilliationProperty = RegisterProperty<string>(new PropertyInfo<string>("ScreeningReconcilliation", "ScreeningReconcilliation Id", ""));
        public string ScreeningReconcilliation
        {
            get
            {
                return GetProperty(ScreeningReconcilliationProperty);
            }
            set
            {
                SetProperty(ScreeningReconcilliationProperty, value);
            }
        }
        public static readonly  PropertyInfo<Int64> ScreeningWhatAttributeIdProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("ScreeningWhatAttributeId", "ScreeningWhatAttributeId Id", Convert.ToInt64(0)));
        public Int64 ScreeningWhatAttributeId
        {
            get
            {
                return GetProperty(ScreeningWhatAttributeIdProperty);
            }
            set
            {
                SetProperty(ScreeningWhatAttributeIdProperty, value);
            }
        }
        public static readonly  PropertyInfo<int> ScreeningNPeopleProperty = RegisterProperty<int>(new PropertyInfo<int>("ScreeningNPeople", "ScreeningNPeople Id", 0));
        public int ScreeningNPeople
        {
            get
            {
                return GetProperty(ScreeningNPeopleProperty);
            }
            set
            {
                SetProperty(ScreeningNPeopleProperty, value);
            }
        }
        public static readonly  PropertyInfo<bool> ScreeningAutoExcludeProperty = RegisterProperty<bool>(new PropertyInfo<bool>("ScreeningAutoExclude", "ScreeningAutoExclude Id", false));
        public bool ScreeningAutoExclude
        {
            get
            {
                return GetProperty(ScreeningAutoExcludeProperty);
            }
            set
            {
                SetProperty(ScreeningAutoExcludeProperty, value);
            }
        }
        public static readonly  PropertyInfo<bool> ScreeningModelRunningProperty = RegisterProperty<bool>(new PropertyInfo<bool>("ScreeningModelRunning", "ScreeningModelRunning Id", false));
        public bool ScreeningModelRunning
        {
            get
            {
                return GetProperty(ScreeningModelRunningProperty);
            }
            set
            {
                SetProperty(ScreeningModelRunningProperty, value);
            }
        }
        //public static readonly  PropertyInfo<bool> ScreeningIndexedProperty = RegisterProperty<bool>(new PropertyInfo<bool>("ScreeningIndexed", "ScreeningIndexed Id", false));
        //public bool ScreeningIndexed
        //{
        //    get
        //    {
        //        return GetProperty(ScreeningIndexedProperty);
        //    }
        //    set
        //    {
        //        SetProperty(ScreeningIndexedProperty, value);
        //    }
        //}
        public static readonly  PropertyInfo<bool> ScreeningListIsGoodProperty = RegisterProperty<bool>(new PropertyInfo<bool>("ScreeningListIsGood", "ScreeningListIsGood", false));
        public bool ScreeningListIsGood
        {
            get
            {
                return GetProperty(ScreeningListIsGoodProperty);
            }
            set
            {
                SetProperty(ScreeningListIsGoodProperty, value);
            }
        }
        //public static readonly  PropertyInfo<string> ScreeningDataFileProperty = RegisterProperty<string>(new PropertyInfo<string>("ScreeningDataFile", "ScreeningDataFile Id", ""));
        //public string ScreeningDataFile
        //{
        //    get
        //    {
        //        return GetProperty(ScreeningDataFileProperty);
        //    }
        //    set
        //    {
        //        SetProperty(ScreeningDataFileProperty, value);
        //    }
        //}
        public static readonly  PropertyInfo<string> BL_ACCOUNT_CODEProperty = RegisterProperty<string>(new PropertyInfo<string>("BL_ACCOUNT_CODE", "BL_ACCOUNT_CODE", string.Empty));
        public string BL_ACCOUNT_CODE
        {
            get
            {
                return GetProperty(BL_ACCOUNT_CODEProperty);
            }
            set
            {
                SetProperty(BL_ACCOUNT_CODEProperty, value);
            }
        }
        public static readonly  PropertyInfo<string> BL_AUTH_CODEProperty = RegisterProperty<string>(new PropertyInfo<string>("BL_AUTH_CODE", "BL_AUTH_CODE", string.Empty));
        public string BL_AUTH_CODE
        {
            get
            {
                return GetProperty(BL_AUTH_CODEProperty);
            }
            set
            {
                SetProperty(BL_AUTH_CODEProperty, value);
            }
        }
        public static readonly  PropertyInfo<string> BL_TXProperty = RegisterProperty<string>(new PropertyInfo<string>("BL_TX", "BL_TX", string.Empty));
        public string BL_TX
        {
            get
            {
                return GetProperty(BL_TXProperty);
            }
            set
            {
                SetProperty(BL_TXProperty, value);
            }
        }

        public static readonly  PropertyInfo<string> BL_CC_ACCOUNT_CODEProperty = RegisterProperty<string>(new PropertyInfo<string>("BL_CC_ACCOUNT_CODE", "BL_CC_ACCOUNT_CODE", string.Empty));
        public string BL_CC_ACCOUNT_CODE
        {
            get
            {
                return GetProperty(BL_CC_ACCOUNT_CODEProperty);
            }
            set
            {
                SetProperty(BL_CC_ACCOUNT_CODEProperty, value);
            }
        }
        public static readonly  PropertyInfo<string> BL_CC_AUTH_CODEProperty = RegisterProperty<string>(new PropertyInfo<string>("BL_CC_AUTH_CODE", "BL_CC_AUTH_CODE", string.Empty));
        public string BL_CC_AUTH_CODE
        {
            get
            {
                return GetProperty(BL_CC_AUTH_CODEProperty);
            }
            set
            {
                SetProperty(BL_CC_AUTH_CODEProperty, value);
            }
        }
        public static readonly  PropertyInfo<string> BL_CC_TXProperty = RegisterProperty<string>(new PropertyInfo<string>("BL_CC_TX", "BL_CC_TX", string.Empty));
        public string BL_CC_TX
        {
            get
            {
                return GetProperty(BL_CC_TXProperty);
            }
            set
            {
                SetProperty(BL_CC_TXProperty, value);
            }
        }
        public static readonly PropertyInfo<int> MagEnabledProperty = RegisterProperty<int>(new PropertyInfo<int>("MagEnabled", "MagEnabled Id", 0));
        public int MagEnabled
        {
            get
            {
                return GetProperty(MagEnabledProperty);
            }
            set
            {
                SetProperty(MagEnabledProperty, value);
            }
        }

        public static readonly PropertyInfo<bool> ComparisonsInCodingOnlyProperty = RegisterProperty<bool>(new PropertyInfo<bool>("ComparisonsInCodingOnly", "ComparisonsInCodingOnly", false));
        public bool ComparisonsInCodingOnly
        {
            get
            {
                return GetProperty(ComparisonsInCodingOnlyProperty);
            }
            set
            {
                SetProperty(ComparisonsInCodingOnlyProperty, value);
            }
        }
#if !WEBDB
        public static readonly PropertyInfo<bool> OpenAIEnabledProperty = RegisterProperty<bool>(new PropertyInfo<bool>("OpenAIEnabled", "OpenAIEnabled", false));
        public bool OpenAIEnabled
        {
            get
            {
                return GetProperty(OpenAIEnabledProperty);
            }
            set
            {
                SetProperty(OpenAIEnabledProperty, value);
            }
        }
        public static readonly PropertyInfo<CreditForRobotsList> CreditForRobotsListProperty = RegisterProperty<CreditForRobotsList>(new PropertyInfo<CreditForRobotsList>("CreditForRobotsList", "CreditForRobotsList", new CreditForRobotsList()));
        [Newtonsoft.Json.JsonIgnore]
        public CreditForRobotsList CreditForRobotsList
        {
            get
            {
                return GetProperty(CreditForRobotsListProperty);
            }
            set
            {
                SetProperty(CreditForRobotsListProperty, value);
            }
        }
        public bool HasCreditForRobots
        {
            get
            {
                foreach (CreditForRobots CfR in CreditForRobotsList)
                {
                    if (CfR.AmountRemaining >= 0.01) return true;
                }
                return false;
            }
        }
        public bool CanUseRobots
        {
            get
            {
                if (OpenAIEnabled) return true;
                return HasCreditForRobots;
            }
        }
#endif
        //protected override void AddAuthorizationRules()
        //{
        //    //string[] canWrite = new string[] { "AdminUser", "RegularUser" };
        //    //string[] canRead = new string[] { "AdminUser", "RegularUser", "ReadOnlyUser" };
        //    //string[] admin = new string[] { "AdminUser" };
        //    //AuthorizationRules.AllowCreate(typeof(ReviewInfo), admin);
        //    //AuthorizationRules.AllowDelete(typeof(ReviewInfo), admin);
        //    //AuthorizationRules.AllowEdit(typeof(ReviewInfo), canWrite);
        //    //AuthorizationRules.AllowGet(typeof(ReviewInfo), canRead);

        //    //AuthorizationRules.AllowRead(NameProperty, canRead);

        //    //AuthorizationRules.AllowWrite(NameProperty, canWrite);
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
            /*
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            LoadProperty(ReviewIdProperty, ri.ReviewId);
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ReviewInfoInsert", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ReadProperty(ReviewIdProperty)));
                    command.Parameters.Add(new SqlParameter("@ReviewInfo_NAME", ReadProperty(NameProperty)));
                    command.Parameters.Add(new SqlParameter("@ReviewInfo_DETAIL", ReadProperty(DetailProperty)));
                    command.Parameters.Add(new SqlParameter("@NEW_ReviewInfo_ID", 0));
                    command.Parameters["@NEW_ReviewInfo_ID"].Direction = System.Data.ParameterDirection.Output;
                    command.ExecuteNonQuery();
                    LoadProperty(ReviewInfoIdProperty, command.Parameters["@NEW_ReviewInfo_ID"].Value);
                }
                connection.Close();
            }
             */
        }

        protected override void DataPortal_Update()
        {
            
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ReviewInfoUpdate", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ReadProperty(ReviewIdProperty)));
                    command.Parameters.Add(new SqlParameter("@SCREENING_CODE_SET_ID", ReadProperty(ScreeningCodeSetIdProperty)));
                    command.Parameters.Add(new SqlParameter("@SCREENING_MODE", ReadProperty(ScreeningModeProperty)));
                    command.Parameters.Add(new SqlParameter("@SCREENING_RECONCILLIATION", ReadProperty(ScreeningReconcilliationProperty)));
                    command.Parameters.Add(new SqlParameter("@SCREENING_WHAT_ATTRIBUTE_ID", ReadProperty(ScreeningWhatAttributeIdProperty)));
                    command.Parameters.Add(new SqlParameter("@SCREENING_N_PEOPLE", ReadProperty(ScreeningNPeopleProperty)));
                    command.Parameters.Add(new SqlParameter("@SCREENING_AUTO_EXCLUDE", ReadProperty(ScreeningAutoExcludeProperty)));
                    //command.Parameters.Add(new SqlParameter("@SCREENING_MODEL_RUNNING", ReadProperty(ScreeningModelRunningProperty)));
                    //command.Parameters.Add(new SqlParameter("@SCREENING_INDEXED", ReadProperty(ScreeningIndexedProperty)));
                    command.Parameters.Add(new SqlParameter("@MAG_ENABLED", ReadProperty(MagEnabledProperty)));
                    command.Parameters.Add(new SqlParameter("@SHOW_SCREENING", ReadProperty(ShowScreeningProperty)));
                    command.Parameters.Add(new SqlParameter("@ENABLE_COMPARISON_IN_CODING_ONLY", ReadProperty(ComparisonsInCodingOnlyProperty)));
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        protected override void DataPortal_DeleteSelf()
        {
            /*
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ReviewInfoDelete", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@ReviewInfo_ID", ReadProperty(ReviewInfoIdProperty)));
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
             */
        }

        protected void DataPortal_Fetch()
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            Internal_Fetch(ri.ReviewId, ri.UserId);            
        }
        protected void DataPortal_Fetch(SingleCriteria<ReviewInfo, int> criteria)
        {
            Internal_Fetch(criteria.Value, 0);
        }
        private void Internal_Fetch(int ReviewId, int ContactId)
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                //ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                using (SqlCommand command = new SqlCommand("st_ReviewInfo", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ReviewId));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        if (reader.Read())
                        {
                            LoadProperty<string>(ReviewNameProperty, reader.GetString("REVIEW_NAME"));
                            LoadProperty<int>(ReviewIdProperty, reader.GetInt32("REVIEW_ID"));
                            LoadProperty(ShowScreeningProperty, reader.GetBoolean("SHOW_SCREENING"));
                            LoadProperty<int>(ScreeningCodeSetIdProperty, reader.GetInt32("SCREENING_CODE_SET_ID"));
                            LoadProperty<string>(ScreeningModeProperty, reader.GetString("SCREENING_MODE"));
                            LoadProperty<string>(ScreeningReconcilliationProperty, reader.GetString("SCREENING_RECONCILLIATION"));
                            LoadProperty<Int64>(ScreeningWhatAttributeIdProperty, reader.GetInt64("SCREENING_WHAT_ATTRIBUTE_ID"));
                            LoadProperty<int>(ScreeningNPeopleProperty, reader.GetInt32("SCREENING_N_PEOPLE"));
                            LoadProperty<bool>(ScreeningAutoExcludeProperty, reader.GetBoolean("SCREENING_AUTO_EXCLUDE"));
                            LoadProperty<bool>(ScreeningModelRunningProperty, reader.GetBoolean("SCREENING_MODEL_RUNNING_V2"));
                            //LoadProperty<bool>(ScreeningIndexedProperty, reader.GetBoolean("SCREENING_INDEXED"));
                            //LoadProperty<string>(ScreeningDataFileProperty, reader.GetString("SCREENING_DATA_FILE"));

                            LoadProperty<string>(BL_ACCOUNT_CODEProperty, reader.GetString("BL_ACCOUNT_CODE"));
                            LoadProperty<string>(BL_AUTH_CODEProperty, reader.GetString("BL_AUTH_CODE"));
                            LoadProperty<string>(BL_TXProperty, reader.GetString("BL_TX"));
                            LoadProperty<string>(BL_CC_ACCOUNT_CODEProperty, reader.GetString("BL_CC_ACCOUNT_CODE"));
                            LoadProperty<string>(BL_CC_AUTH_CODEProperty, reader.GetString("BL_CC_AUTH_CODE"));
                            LoadProperty<string>(BL_CC_TXProperty, reader.GetString("BL_CC_TX"));
                            LoadProperty<int>(MagEnabledProperty, reader.GetInt32("MAG_ENABLED"));
                            LoadProperty<bool>(ComparisonsInCodingOnlyProperty, reader.GetBoolean("ENABLE_COMPARISON_IN_CODING_ONLY"));
#if !WEBDB
                            LoadProperty<bool>(OpenAIEnabledProperty, reader.GetBoolean("OPEN_AI_ENABLED"));
#endif
                        }
#if !WEBDB
                        reader.NextResult();

                        LoadProperty(CreditForRobotsListProperty, DataPortal.FetchChild<CreditForRobotsList>(reader));

#endif
                    }
                }
                if (ContactId > 0)
                {
                    //(Apr 2017): see if there is a screening list that will actually work
                    using (SqlCommand command = new SqlCommand("st_TrainingNextItem", connection))
                    {
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@REVIEW_ID", ReviewId));
                        command.Parameters.Add(new SqlParameter("@CONTACT_ID", ContactId));
                        command.Parameters.Add(new SqlParameter("@TRAINING_CODE_SET_ID", ScreeningCodeSetId));
                        command.Parameters.Add(new SqlParameter("@SIMULATE", 1));
                        using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                        {
                            if (reader.Read())
                            {
                                LoadProperty<bool>(ScreeningListIsGoodProperty, true);
                            }
                            else
                            {
                                LoadProperty<bool>(ScreeningListIsGoodProperty, false);
                            }
                        }
                    }
                }
                connection.Close();
            }
        }
#endif

    }
    
}
