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
    public class TrainingScreeningCriteria : BusinessBase<TrainingScreeningCriteria>
    {
#if SILVERLIGHT
    public TrainingScreeningCriteria() { }

        
#else
        private TrainingScreeningCriteria() { }
#endif

        public override string ToString()
        {
            return AttributeName;
        }

        private static PropertyInfo<int> TrainingScreeningCriteriaIdProperty = RegisterProperty<int>(new PropertyInfo<int>("TrainingScreeningCriteriaId", "TrainingScreeningCriteriaId"));
        public int TrainingScreeningCriteriaId
        {
            get
            {
                return GetProperty(TrainingScreeningCriteriaIdProperty);
            }
        }

        private static PropertyInfo<Int64> AttributeIdProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("AttributeId", "AttributeId"));
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

        private static PropertyInfo<string> AttributeNameProperty = RegisterProperty<string>(new PropertyInfo<string>("AttributeName", "AttributeName", string.Empty));
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

        private static PropertyInfo<bool> IncludedProperty = RegisterProperty<bool>(new PropertyInfo<bool>("Included", "Included"));
        public bool Included
        {
            get
            {
                return GetProperty(IncludedProperty);
            }
            set
            {
                SetProperty(IncludedProperty, value);
            }
        }

        //protected override void AddAuthorizationRules()
        //{
        //    //string[] canWrite = new string[] { "AdminUser", "RegularUser" };
        //    //string[] canRead = new string[] { "AdminUser", "RegularUser", "ReadOnlyUser" };
        //    //string[] admin = new string[] { "AdminUser" };
        //    //AuthorizationRules.AllowCreate(typeof(TrainingScreeningCriteria), admin);
        //    //AuthorizationRules.AllowDelete(typeof(TrainingScreeningCriteria), admin);
        //    //AuthorizationRules.AllowEdit(typeof(TrainingScreeningCriteria), canWrite);
        //    //AuthorizationRules.AllowGet(typeof(TrainingScreeningCriteria), canRead);

        //    //AuthorizationRules.AllowRead(TrainingScreeningCriteriaIdProperty, canRead);
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
                using (SqlCommand command = new SqlCommand("st_TrainingScreeningCriteriaInsert", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ID", ReadProperty(AttributeIdProperty)));
                    command.Parameters.Add(new SqlParameter("@INCLUDED", ReadProperty(IncludedProperty)));
                    SqlParameter par = new SqlParameter("@NEW_TRAINING_SCREENING_CRITERIA_ID", System.Data.SqlDbType.Int);
                    par.Value = 0;
                    command.Parameters.Add(par); 
                    command.Parameters["@NEW_TRAINING_SCREENING_CRITERIA_ID"].Direction = System.Data.ParameterDirection.Output;
                    command.ExecuteNonQuery();
                    LoadProperty(TrainingScreeningCriteriaIdProperty, command.Parameters["@NEW_TRAINING_SCREENING_CRITERIA_ID"].Value);
                }
                connection.Close();
            }
        }

        protected override void DataPortal_Update()
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_TrainingScreeningCriteriaUpdate", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@TRAINING_SCREENING_CRITERIA_ID", ReadProperty(TrainingScreeningCriteriaIdProperty)));
                    command.Parameters.Add(new SqlParameter("@INCLUDED", ReadProperty(IncludedProperty)));
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        protected override void DataPortal_DeleteSelf()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_TrainingScreeningCriteriaDelete", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@TRAINING_SCREENING_CRITERIA_ID", ReadProperty(TrainingScreeningCriteriaIdProperty)));
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        internal static TrainingScreeningCriteria GetTrainingScreeningCriteria(SafeDataReader reader)
        {
            TrainingScreeningCriteria returnValue = new TrainingScreeningCriteria();
            returnValue.LoadProperty<int>(TrainingScreeningCriteriaIdProperty, reader.GetInt32("TRAINING_SCREENING_CRITERIA_ID"));
            returnValue.LoadProperty<Int64>(AttributeIdProperty, reader.GetInt64("ATTRIBUTE_ID"));
            returnValue.LoadProperty<bool>(IncludedProperty, reader.GetBoolean("INCLUDED"));
            returnValue.LoadProperty<string>(AttributeNameProperty, reader.GetString("ATTRIBUTE_NAME"));
            returnValue.MarkOld();
            return returnValue;
        }

#endif

    }
}
