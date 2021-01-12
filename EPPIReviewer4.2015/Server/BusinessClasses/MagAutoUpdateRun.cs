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
#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class MagAutoUpdateRun : BusinessBase<MagAutoUpdateRun>
    {
#if SILVERLIGHT
    public MagAutoUpdateRun() { }

        
#else
        public MagAutoUpdateRun() { }
#endif

        public static readonly PropertyInfo<int> MagAutoUpdateRunIdProperty = RegisterProperty<int>(new PropertyInfo<int>("MagAutoUpdateRunId", "MagAutoUpdateRunId", 0));
        public int MagAutoUpdateRunId
        {
            get
            {
                return GetProperty(MagAutoUpdateRunIdProperty);
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

        public static readonly PropertyInfo<string> UserClassifierDescriptionProperty = RegisterProperty<string>(new PropertyInfo<string>("UserClassifierDescription", "UserClassifierDescription"));
        public string UserClassifierDescription
        {
            get
            {
                return GetProperty(UserClassifierDescriptionProperty);
            }
            set
            {
                SetProperty(UserClassifierDescriptionProperty, value);
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

        public static readonly PropertyInfo<int> UserClassifierModelReviewIdProperty = RegisterProperty<int>(new PropertyInfo<int>("UserClassifierModelReviewId", "UserClassifierModelReviewId"));
        public int UserClassifierModelReviewId
        {
            get
            {
                return GetProperty(UserClassifierModelReviewIdProperty);
            }
            set
            {
                SetProperty(UserClassifierModelReviewIdProperty, value);
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
        public static readonly PropertyInfo<int> NPapersProperty = RegisterProperty<int>(new PropertyInfo<int>("NPapers", "NPapers"));
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
        public static readonly PropertyInfo<int> MagAutoUpdateIdProperty = RegisterProperty<int>(new PropertyInfo<int>("MagAutoUpdateId", "MagAutoUpdateId"));
        public int MagAutoUpdateId
        {
            get
            {
                return GetProperty(MagAutoUpdateIdProperty);
            }
            set
            {
                SetProperty(MagAutoUpdateIdProperty, value);
            }
        }

        /*
        public static readonly PropertyInfo<bool> CheckedProperty = RegisterProperty<bool>(new PropertyInfo<bool>("Checked", "Checked", false));
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
        public static readonly PropertyInfo<bool> IrrelevantProperty = RegisterProperty<bool>(new PropertyInfo<bool>("Irrelevant", "Irrelevant", false));
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

        /*
        public static readonly PropertyInfo<MagAutoUpdateRunList> CitationsProperty = RegisterProperty<MagAutoUpdateRunList>(new PropertyInfo<MagAutoUpdateRunList>("Citations", "Citations"));
        public MagAutoUpdateRunList Citations
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
        public static readonly PropertyInfo<MagAutoUpdateRunList> CitedByProperty = RegisterProperty<MagAutoUpdateRunList>(new PropertyInfo<MagAutoUpdateRunList>("CitedBy", "CitedBy"));
        public MagAutoUpdateRunList CitedBy
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
        public static readonly PropertyInfo<MagAutoUpdateRunList> RecommendedProperty = RegisterProperty<MagAutoUpdateRunList>(new PropertyInfo<MagAutoUpdateRunList>("Recommended", "Recommended"));
        public MagAutoUpdateRunList Recommended
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
        public static readonly PropertyInfo<MagAutoUpdateRunList> RecommendedByProperty = RegisterProperty<MagAutoUpdateRunList>(new PropertyInfo<MagAutoUpdateRunList>("RecommendedBy", "RecommendedBy"));
        public MagAutoUpdateRunList RecommendedBy
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
            DataPortal<MagAutoUpdateRunList> dp = new DataPortal<MagAutoUpdateRunList>();
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
            MagAutoUpdateRunListSelectionCriteria sc = new BusinessClasses.MagAutoUpdateRunListSelectionCriteria();
            sc.MagAutoUpdateRunId = this.FieldOfStudyId;
            sc.ListType = listType;
            dp.BeginFetch(sc);
        }
        */



        //protected override void AddAuthorizationRules()
        //{
        //    //string[] canWrite = new string[] { "AdminUser", "RegularUser" };
        //    //string[] canRead = new string[] { "AdminUser", "RegularUser", "ReadOnlyUser" };
        //    //string[] admin = new string[] { "AdminUser" };
        //    //AuthorizationRules.AllowCreate(typeof(MagAutoUpdateRun), admin);
        //    //AuthorizationRules.AllowDelete(typeof(MagAutoUpdateRun), admin);
        //    //AuthorizationRules.AllowEdit(typeof(MagAutoUpdateRun), canWrite);
        //    //AuthorizationRules.AllowGet(typeof(MagAutoUpdateRun), canRead);

        //    //AuthorizationRules.AllowRead(MagAutoUpdateRunIdProperty, canRead);
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
            // NOT CALLED FROM UI
            /*
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_MagAutoUpdateRunsInsert", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@MAG_AUTO_UPDATE_ID", ReadProperty(MagAutoUpdateIdProperty));
                    command.Parameters.Add(new SqlParameter("@USER_DESCRIPTION", ReadProperty(UserDescriptionProperty)));
                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ID", ReadProperty(AttributeIdProperty)));
                    command.Parameters.Add(new SqlParameter("@ALL_INCLUDED", ReadProperty(AllIncludedProperty)));
                    command.Parameters.Add(new SqlParameter("@STUDY_TYPE_CLASSIFIER", ReadProperty(StudyTypeClassifierProperty)));
                    command.Parameters.Add(new SqlParameter("@USER_CLASSIFIER_MODEL_ID", ReadProperty(UserClassifierModelIdProperty)));
                    command.Parameters.Add(new SqlParameter("@USER_CLASSIFIER_MODEL_REVIEW_ID", ReadProperty(UserClassifierModelReviewIdProperty)));
                    command.Parameters.Add(new SqlParameter("@MAG_AUTO_UPDATE_ID", ReadProperty(MagAutoUpdateRunIdProperty)));
                    command.Parameters["@MAG_AUTO_UPDATE_ID"].Direction = ParameterDirection.Output;
                    command.ExecuteNonQuery();
                    LoadProperty(MagAutoUpdateRunIdProperty, command.Parameters["@MAG_AUTO_UPDATE_ID"].Value);
                }
                connection.Close();
            }
            */
        }

        protected override void DataPortal_Update()
        {
            // NO UPDATE NECESSARY FROM UI
            if (this.UserDescription != "")
            {
                using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand("st_MagAutoUpdateRunsUpdate", connection))
                    {
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        //command.Parameters.Add(new SqlParameter("@MAG_RELATED_RUN_ID", ReadProperty(MagRelatedRunIdProperty)));
                        //command.Parameters.Add(new SqlParameter("@AUTO_RERUN", ReadProperty(AutoReRunProperty)));
                        //command.Parameters.Add(new SqlParameter("@USER_DESCRIPTION", ReadProperty(UserDescriptionProperty)));
                        //command.ExecuteNonQuery();
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
                using (SqlCommand command = new SqlCommand("st_MagAutoUpdateRunDelete", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@MAG_AUTO_UPDATE_RUN_ID", ReadProperty(MagAutoUpdateRunIdProperty)));
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        protected void DataPortal_Fetch(SingleCriteria<MagAutoUpdateRun, Int64> criteria)
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_MagAutoUpdateRun", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@MAG_RELATED_RUN_ID", criteria.Value));
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        if (reader.Read())
                        {
                            LoadProperty<Int32>(MagAutoUpdateRunIdProperty, reader.GetInt32("MAG_AUTO_UPDATE_RUN_ID"));
                            LoadProperty<Int32>(MagAutoUpdateIdProperty, reader.GetInt32("MAG_AUTO_UPDATE_ID"));
                            LoadProperty<Int32>(ReviewIdIdProperty, reader.GetInt32("REVIEW_ID"));
                            LoadProperty<string>(UserDescriptionProperty, reader.GetString("USER_DESCRIPTION"));
                            LoadProperty<Int64>(AttributeIdProperty, reader.GetInt64("ATTRIBUTE_ID"));
                            LoadProperty<string>(AttributeNameProperty, reader.GetString("ATTRIBUTE_NAME"));
                            LoadProperty<bool>(AllIncludedProperty, reader.GetBoolean("ALL_INCLUDED"));
                            LoadProperty<string>(StudyTypeClassifierProperty, reader.GetString("STUDY_TYPE_CLASSIFIER"));
                            LoadProperty<Int32>(UserClassifierModelIdProperty, reader.GetInt32("USER_CLASSIFIER_MODEL_ID"));
                            LoadProperty<Int32>(UserClassifierModelReviewIdProperty, reader.GetInt32("USER_CLASSIFIER_REVIEW_ID"));
                            LoadProperty<SmartDate>(DateRunProperty, reader.GetSmartDate("DATE_RUN"));
                            LoadProperty<Int32>(NPapersProperty, reader.GetInt32("N_PAPERS"));
                        }
                    }
                }
                connection.Close();
            }
        }

        internal static MagAutoUpdateRun GetMagAutoUpdateRun(SafeDataReader reader)
        {
            MagAutoUpdateRun returnValue = new MagAutoUpdateRun();
            returnValue.LoadProperty<Int32>(MagAutoUpdateRunIdProperty, reader.GetInt32("MAG_AUTO_UPDATE_RUN_ID"));
            returnValue.LoadProperty<Int32>(MagAutoUpdateIdProperty, reader.GetInt32("MAG_AUTO_UPDATE_ID"));
            returnValue.LoadProperty<Int32>(ReviewIdIdProperty, reader.GetInt32("REVIEW_ID"));
            returnValue.LoadProperty<string>(UserDescriptionProperty, reader.GetString("USER_DESCRIPTION"));
            returnValue.LoadProperty<Int64>(AttributeIdProperty, reader.GetInt64("ATTRIBUTE_ID"));
            returnValue.LoadProperty<string>(AttributeNameProperty, reader.GetString("ATTRIBUTE_NAME"));
            returnValue.LoadProperty<bool>(AllIncludedProperty, reader.GetBoolean("ALL_INCLUDED"));
            returnValue.LoadProperty<string>(UserClassifierDescriptionProperty, reader.GetString("MODEL_TITLE"));
            returnValue.LoadProperty<string>(StudyTypeClassifierProperty, reader.GetString("STUDY_TYPE_CLASSIFIER"));
            returnValue.LoadProperty<Int32>(UserClassifierModelIdProperty, reader.GetInt32("USER_CLASSIFIER_MODEL_ID"));
            returnValue.LoadProperty<Int32>(UserClassifierModelReviewIdProperty, reader.GetInt32("USER_CLASSIFIER_REVIEW_ID"));
            returnValue.LoadProperty<SmartDate>(DateRunProperty, reader.GetSmartDate("DATE_RUN"));
            returnValue.LoadProperty<Int32>(NPapersProperty, reader.GetInt32("N_PAPERS"));
            returnValue.MarkOld();
            return returnValue;
        }

#endif
    }
}