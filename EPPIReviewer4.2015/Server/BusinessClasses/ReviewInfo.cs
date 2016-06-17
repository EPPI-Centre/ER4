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

        
#else
        private ReviewInfo() { }
#endif

        private static PropertyInfo<int> ReviewIdProperty = RegisterProperty<int>(new PropertyInfo<int>("ReviewId", "ReviewId", 0));
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

        private static PropertyInfo<string> ReviewNameProperty = RegisterProperty<string>(new PropertyInfo<string>("ReviewName", "ReviewName", string.Empty));
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

        private static readonly PropertyInfo<bool> ShowScreeningProperty = RegisterProperty<bool>(new PropertyInfo<bool>("ShowScreening", "ShowScreening"));
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
        private static PropertyInfo<int> ScreeningCodeSetIdProperty = RegisterProperty<int>(new PropertyInfo<int>("ScreeningCodeSetId", "ScreeningCodeSetId Id", 0));
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
        private static PropertyInfo<string> ScreeningModeProperty = RegisterProperty<string>(new PropertyInfo<string>("ScreeningMode", "ScreeningMode Id", ""));
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
        private static PropertyInfo<string> ScreeningReconcilliationProperty = RegisterProperty<string>(new PropertyInfo<string>("ScreeningReconcilliation", "ScreeningReconcilliation Id", ""));
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
        private static PropertyInfo<Int64> ScreeningWhatAttributeIdProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("ScreeningWhatAttributeId", "ScreeningWhatAttributeId Id", Convert.ToInt64(0)));
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
        private static PropertyInfo<int> ScreeningNPeopleProperty = RegisterProperty<int>(new PropertyInfo<int>("ScreeningNPeople", "ScreeningNPeople Id", 0));
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
        private static PropertyInfo<bool> ScreeningAutoExcludeProperty = RegisterProperty<bool>(new PropertyInfo<bool>("ScreeningAutoExclude", "ScreeningAutoExclude Id", false));
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
        private static PropertyInfo<bool> ScreeningModelRunningProperty = RegisterProperty<bool>(new PropertyInfo<bool>("ScreeningModelRunning", "ScreeningModelRunning Id", false));
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
        private static PropertyInfo<bool> ScreeningIndexedProperty = RegisterProperty<bool>(new PropertyInfo<bool>("ScreeningIndexed", "ScreeningIndexed Id", false));
        public bool ScreeningIndexed
        {
            get
            {
                return GetProperty(ScreeningIndexedProperty);
            }
            set
            {
                SetProperty(ScreeningIndexedProperty, value);
            }
        }
        private static PropertyInfo<string> ScreeningDataFileProperty = RegisterProperty<string>(new PropertyInfo<string>("ScreeningDataFile", "ScreeningDataFile Id", ""));
        public string ScreeningDataFile
        {
            get
            {
                return GetProperty(ScreeningDataFileProperty);
            }
            set
            {
                SetProperty(ScreeningDataFileProperty, value);
            }
        }
        private static PropertyInfo<string> BL_ACCOUNT_CODEProperty = RegisterProperty<string>(new PropertyInfo<string>("BL_ACCOUNT_CODE", "BL_ACCOUNT_CODE", string.Empty));
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
        private static PropertyInfo<string> BL_AUTH_CODEProperty = RegisterProperty<string>(new PropertyInfo<string>("BL_AUTH_CODE", "BL_AUTH_CODE", string.Empty));
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
        private static PropertyInfo<string> BL_TXProperty = RegisterProperty<string>(new PropertyInfo<string>("BL_TX", "BL_TX", string.Empty));
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

        private static PropertyInfo<string> BL_CC_ACCOUNT_CODEProperty = RegisterProperty<string>(new PropertyInfo<string>("BL_CC_ACCOUNT_CODE", "BL_CC_ACCOUNT_CODE", string.Empty));
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
        private static PropertyInfo<string> BL_CC_AUTH_CODEProperty = RegisterProperty<string>(new PropertyInfo<string>("BL_CC_AUTH_CODE", "BL_CC_AUTH_CODE", string.Empty));
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
        private static PropertyInfo<string> BL_CC_TXProperty = RegisterProperty<string>(new PropertyInfo<string>("BL_CC_TX", "BL_CC_TX", string.Empty));
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
                    command.Parameters.Add(new SqlParameter("@SCREENING_MODEL_RUNNING", ReadProperty(ScreeningModelRunningProperty)));
                    command.Parameters.Add(new SqlParameter("@SCREENING_INDEXED", ReadProperty(ScreeningIndexedProperty)));
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
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                using (SqlCommand command = new SqlCommand("st_ReviewInfo", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
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
                            LoadProperty<bool>(ScreeningModelRunningProperty, reader.GetBoolean("SCREENING_MODEL_RUNNING"));
                            LoadProperty<bool>(ScreeningIndexedProperty, reader.GetBoolean("SCREENING_INDEXED"));
                            //LoadProperty<string>(ScreeningDataFileProperty, reader.GetString("SCREENING_DATA_FILE"));

                            LoadProperty<string>(BL_ACCOUNT_CODEProperty, reader.GetString("BL_ACCOUNT_CODE"));
                            LoadProperty<string>(BL_AUTH_CODEProperty, reader.GetString("BL_AUTH_CODE"));
                            LoadProperty<string>(BL_TXProperty, reader.GetString("BL_TX"));
                            LoadProperty<string>(BL_CC_ACCOUNT_CODEProperty, reader.GetString("BL_CC_ACCOUNT_CODE"));
                            LoadProperty<string>(BL_CC_AUTH_CODEProperty, reader.GetString("BL_CC_AUTH_CODE"));
                            LoadProperty<string>(BL_CC_TXProperty, reader.GetString("BL_CC_TX"));
                        }
                    }
                }
                connection.Close();
            }
        }

#endif

    }
}
