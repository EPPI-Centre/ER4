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
    public class TrainingReviewerTerm : BusinessBase<TrainingReviewerTerm>
    {
#if SILVERLIGHT
    public TrainingReviewerTerm() { }

        
#else
        private TrainingReviewerTerm() { }
#endif

        public override string ToString()
        {
            return ReviewerTerm;
        }

        private static PropertyInfo<int> TrainingReviewerTermIdProperty = RegisterProperty<int>(new PropertyInfo<int>("TrainingReviewerTermId", "TrainingReviewerTermId"));
        public int TrainingReviewerTermId
        {
            get
            {
                return GetProperty(TrainingReviewerTermIdProperty);
            }
        }

        private static PropertyInfo<Int64> ItemTermDictionaryIdProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("ItemTermDictionaryId", "ItemTermDictionaryId"));
        public Int64 ItemTermDictionaryId
        {
            get
            {
                return GetProperty(ItemTermDictionaryIdProperty);
            }
            set
            {
                SetProperty(ItemTermDictionaryIdProperty, value);
            }
        }

        private static PropertyInfo<string> ReviewerTermProperty = RegisterProperty<string>(new PropertyInfo<string>("ReviewerTerm", "ReviewerTerm", string.Empty));
        public string ReviewerTerm
        {
            get
            {
                return GetProperty(ReviewerTermProperty);
            }
            set
            {
                SetProperty(ReviewerTermProperty, value);
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

        private static PropertyInfo<string> TermProperty = RegisterProperty<string>(new PropertyInfo<string>("Term", "Term", string.Empty));
        public string Term
        {
            get
            {
                return GetProperty(TermProperty);
            }
            set
            {
                SetProperty(TermProperty, value);
            }
        }

        //protected override void AddAuthorizationRules()
        //{
        //    //string[] canWrite = new string[] { "AdminUser", "RegularUser" };
        //    //string[] canRead = new string[] { "AdminUser", "RegularUser", "ReadOnlyUser" };
        //    //string[] admin = new string[] { "AdminUser" };
        //    //AuthorizationRules.AllowCreate(typeof(TrainingReviewerTerm), admin);
        //    //AuthorizationRules.AllowDelete(typeof(TrainingReviewerTerm), admin);
        //    //AuthorizationRules.AllowEdit(typeof(TrainingReviewerTerm), canWrite);
        //    //AuthorizationRules.AllowGet(typeof(TrainingReviewerTerm), canRead);

        //    //AuthorizationRules.AllowRead(TrainingReviewerTermIdProperty, canRead);
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
                using (SqlCommand command = new SqlCommand("st_TrainingReviewerTermInsert", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@REVIEWER_TERM", ReadProperty(ReviewerTermProperty)));
                    command.Parameters.Add(new SqlParameter("@INCLUDED", ReadProperty(IncludedProperty)));
                    SqlParameter par = new SqlParameter("@NEW_TRAINING_REVIEWER_TERM_ID", System.Data.SqlDbType.Int);
                    par.Value = 0;
                    command.Parameters.Add(par); 
                    command.Parameters["@NEW_TRAINING_REVIEWER_TERM_ID"].Direction = System.Data.ParameterDirection.Output;
                    command.ExecuteNonQuery();
                    LoadProperty(TrainingReviewerTermIdProperty, command.Parameters["@NEW_TRAINING_REVIEWER_TERM_ID"].Value);
                }
                connection.Close();
            }
        }

        protected override void DataPortal_Update()
        {
            if (ReadProperty(ReviewerTermProperty).Length == 0)
            {
                DataPortal_DeleteSelf();
                return;
            }
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_TrainingReviewerTermUpdate", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@REVIEWER_TERM", ReadProperty(ReviewerTermProperty)));
                    command.Parameters.Add(new SqlParameter("@TRAINING_REVIEWER_TERM_ID", ReadProperty(TrainingReviewerTermIdProperty)));
                    command.Parameters.Add(new SqlParameter("@INCLUDED", ReadProperty(IncludedProperty)));
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        protected override void DataPortal_DeleteSelf()
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_TrainingReviewerTermDelete", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@TRAINING_REVIEWER_TERM_ID", ReadProperty(TrainingReviewerTermIdProperty)));
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        internal static TrainingReviewerTerm GetTrainingReviewerTerm(SafeDataReader reader)
        {
            TrainingReviewerTerm returnValue = new TrainingReviewerTerm();
            returnValue.LoadProperty<int>(TrainingReviewerTermIdProperty, reader.GetInt32("TRAINING_REVIEWER_TERM_ID"));
            returnValue.LoadProperty<string>(ReviewerTermProperty, reader.GetString("REVIEWER_TERM"));
            returnValue.LoadProperty<bool>(IncludedProperty, reader.GetBoolean("INCLUDED"));
            returnValue.LoadProperty<Int64>(ItemTermDictionaryIdProperty, reader.GetInt64("ITEM_TERM_DICTIONARY_ID"));
            returnValue.LoadProperty<string>(TermProperty, reader.GetString("TERM"));
            returnValue.MarkOld();
            return returnValue;
        }

#endif

    }
}
