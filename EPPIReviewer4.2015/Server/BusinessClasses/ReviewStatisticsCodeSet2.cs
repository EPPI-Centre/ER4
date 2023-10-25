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
    public class ReviewStatisticsCodeSet2 : BusinessBase<ReviewStatisticsCodeSet2>
    {

        public ReviewStatisticsCodeSet2() { }

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

        public static readonly PropertyInfo<ReviewStatisticsReviewerList2> ReviewerStatisticsProperty = RegisterProperty<ReviewStatisticsReviewerList2>(new PropertyInfo<ReviewStatisticsReviewerList2>("ReviewerStatistics", "ReviewerStatistics"));
        public ReviewStatisticsReviewerList2 ReviewerStatistics
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

        internal static ReviewStatisticsCodeSet2 GetReviewStatisticsCodeSet(int SetId, string SetName)
        {
            ReviewStatisticsCodeSet2 returnValue = new ReviewStatisticsCodeSet2();
            returnValue.LoadProperty<int>(SetIdProperty, SetId);
            returnValue.LoadProperty<string>(SetNameProperty, SetName);
            returnValue.LoadProperty<int>(NumItemsCompletedProperty, 0);
            returnValue.LoadProperty<int>(NumItemsIncompleteProperty, 0);
            returnValue.LoadProperty<ReviewStatisticsReviewerList2>(ReviewerStatisticsProperty, new ReviewStatisticsReviewerList2());
            returnValue.MarkAsChild();
            returnValue.MarkOld();
            return returnValue;
        }
        /// <summary>
        /// Automatically produces new ReviewStatisticsCodeSet2 lines if needed
        /// </summary>
        /// <param name="ContactId"></param>
        /// <param name="IsComplete"></param>
        /// <param name="ContactName"></param>
        internal void AddReviewerCount(int ContactId, bool IsComplete, string ContactName)
        {
            ReviewStatisticsReviewer2 LineToEdit = new ReviewStatisticsReviewer2();
            foreach(ReviewStatisticsReviewer2 line in this.ReviewerStatistics)
            {
                if (line.ContactId == ContactId) { LineToEdit = line; break; }
            }
            if (LineToEdit.ContactId != ContactId)
            {
                LineToEdit = ReviewStatisticsReviewer2.GetReviewStatisticsReviewer(this.SetId, this.SetName, ContactId, ContactName);
                this.ReviewerStatistics.Add(LineToEdit);
            }
            if (IsComplete) LineToEdit.NumItemsCompleted++;
            else LineToEdit.NumItemsIncomplete++;
        }
#endif

    }
}
