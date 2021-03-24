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
    public class MagAutoUpdate : BusinessBase<MagAutoUpdate>
    {
#if SILVERLIGHT
    public MagAutoUpdate() { }

        
#else
        public MagAutoUpdate() { }
#endif

        public static readonly PropertyInfo<int> MagAutoUpdateIdProperty = RegisterProperty<int>(new PropertyInfo<int>("MagAutoUpdateId", "MagAutoUpdateId", 0));
        public int MagAutoUpdateId
        {
            get
            {
                return GetProperty(MagAutoUpdateIdProperty);
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
        /*
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

        public static readonly PropertyInfo<int> UserClassifierModelIdProperty = RegisterProperty<int>(new PropertyInfo<int>("UserClassifierModelId", "UserClassifierModelId", 0));
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

        public static readonly PropertyInfo<int> UserClassifierModelReviewIdProperty = RegisterProperty<int>(new PropertyInfo<int>("UserClassifierModelReviewId", "UserClassifierModelReviewId", 0));
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
                using (SqlCommand command = new SqlCommand("st_MagAutoUpdateInsert", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@USER_DESCRIPTION", ReadProperty(UserDescriptionProperty)));
                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ID", ReadProperty(AttributeIdProperty)));
                    command.Parameters.Add(new SqlParameter("@ALL_INCLUDED", ReadProperty(AllIncludedProperty)));
                    command.Parameters.Add(new SqlParameter("@MAG_AUTO_UPDATE_ID", ReadProperty(MagAutoUpdateIdProperty)));
                    command.Parameters["@MAG_AUTO_UPDATE_ID"].Direction = ParameterDirection.Output;
                    command.ExecuteNonQuery();
                    LoadProperty(MagAutoUpdateIdProperty, command.Parameters["@MAG_AUTO_UPDATE_ID"].Value);
                }
                connection.Close();
            }
        }

        protected override void DataPortal_Update()
        {
            // NO UPDATE NECESSARY
            if (this.UserDescription != "")
            {
                using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand("st_MagAutoUpdatesUpdate", connection))
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
                using (SqlCommand command = new SqlCommand("st_MagAutoUpdateDelete", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@MAG_AUTO_UPDATE_ID", ReadProperty(MagAutoUpdateIdProperty)));
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        protected void DataPortal_Fetch(SingleCriteria<MagAutoUpdate, Int64> criteria)
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_MagAutoUpdate", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@MAG_RELATED_RUN_ID", criteria.Value));
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        if (reader.Read())
                        {
                            LoadProperty<Int32>(MagAutoUpdateIdProperty, reader.GetInt32("MAG_AUTO_UPDATE_ID"));
                            LoadProperty<Int32>(ReviewIdIdProperty, reader.GetInt32("REVIEW_ID"));
                            LoadProperty<string>(UserDescriptionProperty, reader.GetString("USER_DESCRIPTION"));
                            LoadProperty<Int64>(AttributeIdProperty, reader.GetInt64("ATTRIBUTE_ID"));
                            LoadProperty<string>(AttributeNameProperty, reader.GetString("ATTRIBUTE_NAME"));
                            LoadProperty<bool>(AllIncludedProperty, reader.GetBoolean("ALL_INCLUDED"));
                            //LoadProperty<string>(StudyTypeClassifierProperty, reader.GetString("STUDY_TYPE_CLASSIFIER"));
                            //LoadProperty<Int32>(UserClassifierModelIdProperty, reader.GetInt32("USER_CLASSIFIER_MODEL_ID"));
                            //LoadProperty<string>(UserClassifierDescriptionProperty, reader.GetString("MODEL_TITLE"));
                            //LoadProperty<Int32>(UserClassifierModelReviewIdProperty, reader.GetInt32("USER_CLASSIFIER_REVIEW_ID"));
                        }
                    }
                }
                connection.Close();
            }
        }

        internal static MagAutoUpdate GetMagAutoUpdate(SafeDataReader reader)
        {
            MagAutoUpdate returnValue = new MagAutoUpdate();
            returnValue.LoadProperty<Int32>(MagAutoUpdateIdProperty, reader.GetInt32("MAG_AUTO_UPDATE_ID"));
            returnValue.LoadProperty<Int32>(ReviewIdIdProperty, reader.GetInt32("REVIEW_ID"));
            returnValue.LoadProperty<string>(UserDescriptionProperty, reader.GetString("USER_DESCRIPTION"));
            returnValue.LoadProperty<Int64>(AttributeIdProperty, reader.GetInt64("ATTRIBUTE_ID"));
            returnValue.LoadProperty<string>(AttributeNameProperty, reader.GetString("ATTRIBUTE_NAME"));
            returnValue.LoadProperty<bool>(AllIncludedProperty, reader.GetBoolean("ALL_INCLUDED"));
            //returnValue.LoadProperty<string>(StudyTypeClassifierProperty, reader.GetString("STUDY_TYPE_CLASSIFIER"));
            //returnValue.LoadProperty<Int32>(UserClassifierModelIdProperty, reader.GetInt32("USER_CLASSIFIER_MODEL_ID"));
            //returnValue.LoadProperty<string>(UserClassifierDescriptionProperty, reader.GetString("MODEL_TITLE"));
            //returnValue.LoadProperty<Int32>(UserClassifierModelReviewIdProperty, reader.GetInt32("USER_CLASSIFIER_REVIEW_ID"));
            returnValue.MarkOld();
            return returnValue;
        }

#endif
    }
}