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
    public class ReviewStatisticsReviewer2 : BusinessBase<ReviewStatisticsReviewer2>
    {

        public ReviewStatisticsReviewer2() { }

        public override string ToString()
        {
            return ContactName;
        }

        public string Title
        {
            get
            {
                return ContactName;
            }
        }

        public static readonly PropertyInfo<int> ContactIdProperty = RegisterProperty<int>(new PropertyInfo<int>("ContactId", "ContactId"));
        public int ContactId
        {
            get
            {
                return GetProperty(ContactIdProperty);
            }
        }

        public static readonly PropertyInfo<string> ContactNameProperty = RegisterProperty<string>(new PropertyInfo<string>("ContactName", "ContactName", ""));
        public string ContactName
        {
            get
            {
                return GetProperty(ContactNameProperty);
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

        public static readonly PropertyInfo<int> NumItemsCompletedProperty = RegisterProperty<int>(new PropertyInfo<int>("NumItemsCompleted", "NumItemsCompleted"));
        public int NumItemsCompleted
        {
            get
            {
                return GetProperty(NumItemsCompletedProperty);
            }
            set
            {
                LoadProperty<int>(NumItemsCompletedProperty, value);
            }
        }
        public static readonly PropertyInfo<int> NumItemsIncompleteProperty = RegisterProperty<int>(new PropertyInfo<int>("NumItemsIncomplete", "NumItemsIncomplete"));
        public int NumItemsIncomplete
        {
            get
            {
                return GetProperty(NumItemsIncompleteProperty);
            }
            set
            {
                LoadProperty<int>(NumItemsIncompleteProperty, value);
            }
        }
        //protected override void AddAuthorizationRules()
        //{
        //    string[] canWrite = new string[] { "AdminUser", "RegularUser" };
        //    string[] canRead = new string[] { "AdminUser", "RegularUser", "ReadOnlyUser" };
        //    string[] admin = new string[] { "AdminUser" };
        //    AuthorizationRules.AllowCreate(typeof(ReviewStatisticsReviewer), admin);
        //    AuthorizationRules.AllowDelete(typeof(ReviewStatisticsReviewer), admin);
        //    AuthorizationRules.AllowEdit(typeof(ReviewStatisticsReviewer), canWrite);
        //    AuthorizationRules.AllowGet(typeof(ReviewStatisticsReviewer), canRead);


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
                using (SqlCommand command = new SqlCommand("st_ReviewStatisticsReviewerInsert", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ReadProperty(ReviewIdProperty)));
                    command.Parameters.Add(new SqlParameter("@ReviewStatisticsReviewer_NAME", ReadProperty(NameProperty)));
                    command.Parameters.Add(new SqlParameter("@ReviewStatisticsReviewer_DETAIL", ReadProperty(DetailProperty)));
                    command.Parameters.Add(new SqlParameter("@NEW_ReviewStatisticsReviewer_ID", 0));
                    command.Parameters["@NEW_ReviewStatisticsReviewer_ID"].Direction = System.Data.ParameterDirection.Output;
                    command.ExecuteNonQuery();
                    LoadProperty(ReviewStatisticsReviewerIdProperty, command.Parameters["@NEW_ReviewStatisticsReviewer_ID"].Value);
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
                using (SqlCommand command = new SqlCommand("st_ReviewStatisticsReviewerUpdate", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@ReviewStatisticsReviewer_ID", ReadProperty(ReviewStatisticsReviewerIdProperty)));
                    command.Parameters.Add(new SqlParameter("@ReviewStatisticsReviewer_TITLE", ReadProperty(TitleProperty)));
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
                using (SqlCommand command = new SqlCommand("st_ReviewStatisticsReviewerDelete", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@ReviewStatisticsReviewer_ID", ReadProperty(ReviewStatisticsReviewerIdProperty)));
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
            */
        }

        internal static ReviewStatisticsReviewer2 GetReviewStatisticsReviewer(int setId, string setName,int cId, string cName)
        {
            ReviewStatisticsReviewer2 returnValue = new ReviewStatisticsReviewer2();
            returnValue.LoadProperty<int>(ContactIdProperty, cId);
            returnValue.LoadProperty<string>(ContactNameProperty, cName);
            returnValue.LoadProperty<int>(SetIdProperty, setId);
            returnValue.LoadProperty<string>(SetNameProperty, setName);
            returnValue.LoadProperty<int>(NumItemsCompletedProperty, 0);
            returnValue.LoadProperty<int>(NumItemsIncompleteProperty, 0);
            returnValue.MarkAsChild();
            returnValue.MarkOld();
            return returnValue;
        }

#endif

    }
}
