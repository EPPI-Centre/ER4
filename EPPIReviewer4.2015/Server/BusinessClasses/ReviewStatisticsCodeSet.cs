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
    public class ReviewStatisticsCodeSet : BusinessBase<ReviewStatisticsCodeSet>
    {

        public ReviewStatisticsCodeSet() { }

        public override string ToString()
        {
            return SetName;
        }

        public string Title
        {
            get
            {
                return SetName;
            }
        }

        public static readonly PropertyInfo<int> SetIdProperty = RegisterProperty<int>(new PropertyInfo<int>("SetId", "SetId"));
        public int SetId
        {
            get
            {
                return GetProperty(SetIdProperty);
            }
        }

        public static readonly PropertyInfo<string> SetNameProperty = RegisterProperty<string>(new PropertyInfo<string>("SetName", "SetName", ""));
        public string SetName
        {
            get
            {
                return GetProperty(SetNameProperty);
            }
        }

        public static readonly PropertyInfo<int> NumItemsProperty = RegisterProperty<int>(new PropertyInfo<int>("NumItems", "NumItems"));
        public int NumItems
        {
            get
            {
                return GetProperty(NumItemsProperty);
            }
        }

        public static readonly PropertyInfo<bool> CompletedProperty = RegisterProperty<bool>(new PropertyInfo<bool>("Completed", "Completed"));
        public bool Completed
        {
            get
            {
                return GetProperty(CompletedProperty);
            }
        }

        public static readonly PropertyInfo<ReviewStatisticsReviewerList> ReviewerStatisticsProperty = RegisterProperty<ReviewStatisticsReviewerList>(new PropertyInfo<ReviewStatisticsReviewerList>("ReviewerStatistics", "ReviewerStatistics"));
        public ReviewStatisticsReviewerList ReviewerStatistics
        {
            get
            {
                return GetProperty(ReviewerStatisticsProperty);
            }
        }

        //protected override void AddAuthorizationRules()
        //{
        //    string[] canWrite = new string[] { "AdminUser", "RegularUser" };
        //    string[] canRead = new string[] { "AdminUser", "RegularUser", "ReadOnlyUser" };
        //    string[] admin = new string[] { "AdminUser" };
        //    AuthorizationRules.AllowCreate(typeof(ReviewStatisticsCodeSet), admin);
        //    AuthorizationRules.AllowDelete(typeof(ReviewStatisticsCodeSet), admin);
        //    AuthorizationRules.AllowEdit(typeof(ReviewStatisticsCodeSet), canWrite);
        //    AuthorizationRules.AllowGet(typeof(ReviewStatisticsCodeSet), canRead);


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
            CodeSetIdentity ri = Csla.ApplicationContext.User.Identity as CodeSetIdentity;
            LoadProperty(ReviewIdProperty, ri.ReviewId);
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ReviewStatisticsCodeSetInsert", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ReadProperty(ReviewIdProperty)));
                    command.Parameters.Add(new SqlParameter("@ReviewStatisticsCodeSet_NAME", ReadProperty(NameProperty)));
                    command.Parameters.Add(new SqlParameter("@ReviewStatisticsCodeSet_DETAIL", ReadProperty(DetailProperty)));
                    command.Parameters.Add(new SqlParameter("@NEW_ReviewStatisticsCodeSet_ID", 0));
                    command.Parameters["@NEW_ReviewStatisticsCodeSet_ID"].Direction = System.Data.ParameterDirection.Output;
                    command.ExecuteNonQuery();
                    LoadProperty(ReviewStatisticsCodeSetIdProperty, command.Parameters["@NEW_ReviewStatisticsCodeSet_ID"].Value);
                }
                connection.Close();
            }
             */
        }

        protected override void DataPortal_Update()
        {
            /*
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ReviewStatisticsCodeSetUpdate", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@ReviewStatisticsCodeSet_ID", ReadProperty(ReviewStatisticsCodeSetIdProperty)));
                    command.Parameters.Add(new SqlParameter("@ReviewStatisticsCodeSet_TITLE", ReadProperty(TitleProperty)));
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
            */
        }

        protected override void DataPortal_DeleteSelf()
        {
            /*
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ReviewStatisticsCodeSetDelete", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@ReviewStatisticsCodeSet_ID", ReadProperty(ReviewStatisticsCodeSetIdProperty)));
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
            */
        }

        internal static ReviewStatisticsCodeSet GetReviewStatisticsCodeSet(SafeDataReader reader, bool IsCompleted)
        {
            ReviewStatisticsCodeSet returnValue = new ReviewStatisticsCodeSet();
            returnValue.LoadProperty<int>(SetIdProperty, reader.GetInt32("SET_ID"));
            returnValue.LoadProperty<string>(SetNameProperty, reader.GetString("SET_NAME"));
            returnValue.LoadProperty<bool>(CompletedProperty, IsCompleted);
            returnValue.LoadProperty<int>(NumItemsProperty, reader.GetInt32("TOTAL"));
            returnValue.LoadProperty <ReviewStatisticsReviewerList>(ReviewerStatisticsProperty, ReviewStatisticsReviewerList.GetReviewStatisticsReviewerList(IsCompleted, returnValue.SetId));
            returnValue.MarkAsChild();
            returnValue.MarkOld();
            return returnValue;
        }

#endif

    }
}
