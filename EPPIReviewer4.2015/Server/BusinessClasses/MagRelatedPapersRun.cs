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
		/*
        private static PropertyInfo<bool> CheckedProperty = RegisterProperty<bool>(new PropertyInfo<bool>("Checked", "Checked", false));
        public bool Checked
        {
            get
            {
                return GetProperty(CheckedProperty);
            }
            set
            {
                SetProperty(CheckedProperty, value);
            }
        }

        private static PropertyInfo<bool> IrrelevantProperty = RegisterProperty<bool>(new PropertyInfo<bool>("Irrelevant", "Irrelevant", false));
        public bool Irrelevant
        {
            get
            {
                return GetProperty(IrrelevantProperty);
            }
            set
            {
                SetProperty(IrrelevantProperty, value);
            }
        }
        */
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

        /*
        public static readonly PropertyInfo<MagRelatedPapersRunList> CitationsProperty = RegisterProperty<MagRelatedPapersRunList>(new PropertyInfo<MagRelatedPapersRunList>("Citations", "Citations"));
        public MagRelatedPapersRunList Citations
        {
            get
            {
                return GetProperty(CitationsProperty);
            }
            set
            {
                SetProperty(CitationsProperty, value);
            }
        }

        public static readonly PropertyInfo<MagRelatedPapersRunList> CitedByProperty = RegisterProperty<MagRelatedPapersRunList>(new PropertyInfo<MagRelatedPapersRunList>("CitedBy", "CitedBy"));
        public MagRelatedPapersRunList CitedBy
        {
            get
            {
                return GetProperty(CitedByProperty);
            }
            set
            {
                SetProperty(CitedByProperty, value);
            }
        }

        public static readonly PropertyInfo<MagRelatedPapersRunList> RecommendedProperty = RegisterProperty<MagRelatedPapersRunList>(new PropertyInfo<MagRelatedPapersRunList>("Recommended", "Recommended"));
        public MagRelatedPapersRunList Recommended
        {
            get
            {
                return GetProperty(RecommendedProperty);
            }
            set
            {
                SetProperty(RecommendedProperty, value);
            }
        }

        public static readonly PropertyInfo<MagRelatedPapersRunList> RecommendedByProperty = RegisterProperty<MagRelatedPapersRunList>(new PropertyInfo<MagRelatedPapersRunList>("RecommendedBy", "RecommendedBy"));
        public MagRelatedPapersRunList RecommendedBy
        {
            get
            {
                return GetProperty(RecommendedByProperty);
            }
            set
            {
                SetProperty(RecommendedByProperty, value);
            }
        }
        
        public void GetRelatedFieldOfStudyList(string listType)
        {
            DataPortal<MagRelatedPapersRunList> dp = new DataPortal<MagRelatedPapersRunList>();
            dp.FetchCompleted += (o, e2) =>
            {
                if (e2.Object != null)
                {
                    if (e2.Error == null)
                    {
                        this.Citations = e2.Object;
                        //this.MarkClean(); // don't want the object marked as 'dirty' just because it's loaded a new list
                    }
                }
                if (e2.Error != null)
                {
#if SILVERLIGHT
                    System.Windows.MessageBox.Show(e2.Error.Message);
#endif
                }
            };
            MagRelatedPapersRunListSelectionCriteria sc = new BusinessClasses.MagRelatedPapersRunListSelectionCriteria();
            sc.MagRelatedPapersRunId = this.FieldOfStudyId;
            sc.ListType = listType;
            dp.BeginFetch(sc);
        }
        */



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
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_MagRelatedPapersRunsInsert", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@USER_DESCRIPTION", ReadProperty(UserDescriptionProperty)));
                    command.Parameters.Add(new SqlParameter("@PaperIdList", ""));
                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ID", ReadProperty(AttributeIdProperty)));
                    command.Parameters.Add(new SqlParameter("@ALL_INCLUDED", ReadProperty(AllIncludedProperty)));
                    command.Parameters.Add(new SqlParameter("@DATE_FROM", DateFrom.DBValue));
                    command.Parameters.Add(new SqlParameter("@AUTO_RERUN", ReadProperty(AutoReRunProperty)));
                    command.Parameters.Add(new SqlParameter("@MODE", ReadProperty(ModeProperty)));
                    command.Parameters.Add(new SqlParameter("@FILTERED", ReadProperty(FilteredProperty)));
                    command.Parameters.Add(new SqlParameter("@MAG_RELATED_RUN_ID", ReadProperty(MagRelatedRunIdProperty)));
                    command.Parameters["@MAG_RELATED_RUN_ID"].Direction = System.Data.ParameterDirection.Output;
                    command.ExecuteNonQuery();
                    LoadProperty(MagRelatedRunIdProperty, command.Parameters["@MAG_RELATED_RUN_ID"].Value);
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
                        command.Parameters.Add(new SqlParameter("@AUTO_RERUN", ReadProperty(AutoReRunProperty)));
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
                            LoadProperty<bool>(AutoReRunProperty, reader.GetBoolean("AUTO_RERUN"));
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
            returnValue.LoadProperty<bool>(AutoReRunProperty, reader.GetBoolean("AUTO_RERUN"));
            returnValue.LoadProperty<string>(StatusProperty, reader.GetString("STATUS"));
            returnValue.LoadProperty<string>(UserStatusProperty, reader.GetString("USER_STATUS"));
            returnValue.LoadProperty<int>(NPapersProperty, reader.GetInt32("N_PAPERS"));
            returnValue.LoadProperty<string>(ModeProperty, reader.GetString("MODE"));
            returnValue.LoadProperty<string>(FilteredProperty, reader.GetString("FILTERED"));
            returnValue.MarkOld();
            return returnValue;
        }

#endif
    }
}
