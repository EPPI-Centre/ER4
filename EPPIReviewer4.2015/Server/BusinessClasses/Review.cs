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
    public class Review : BusinessBase<Review>
    {
        public Review(string Name, int ContactID) 
        {
            SetProperty(ReviewNameProperty, Name);
            SetProperty(ContactIdProperty, ContactID);
        }
#if SILVERLIGHT
    public Review() { }  
#else
		public Review() { }
#endif

        public override string ToString()
        {
            return ReviewName;
        }

        public static readonly PropertyInfo<int> ReviewIdProperty = RegisterProperty<int>(new PropertyInfo<int>("ReviewId", "ReviewId"));
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

		public static readonly PropertyInfo<int> ContactIdProperty = RegisterProperty<int>(new PropertyInfo<int>("ContactId", "ContactId"));
        public int ContactId
        {
            get
            {
                return GetProperty(ContactIdProperty);
            }
            set
            {
                SetProperty(ContactIdProperty, value);
            }
        }

		public static readonly PropertyInfo<string> ReviewNameProperty = RegisterProperty<string>(new PropertyInfo<string>("ReviewName", "ReviewName", string.Empty));
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
        public static readonly PropertyInfo<Int64> ResultProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("Result", "Result"));
        public Int64 Result
        {
            get
            {
                return GetProperty(ResultProperty);
            }
        }

        //protected override void AddAuthorizationRules()
        //{
        //    //string[] canWrite = new string[] { "AdminUser", "RegularUser" };
        //    //string[] canRead = new string[] { "AdminUser", "RegularUser", "ReadOnlyUser" };
        //    //string[] admin = new string[] { "AdminUser", "RegularUser" };
        //    //AuthorizationRules.AllowCreate(typeof(Review), admin);
        //    //AuthorizationRules.AllowDelete(typeof(Review), admin);
        //    //AuthorizationRules.AllowEdit(typeof(Review), canWrite);
        //    //AuthorizationRules.AllowGet(typeof(Review), canRead);

        //    //AuthorizationRules.AllowRead(ReviewIdProperty, canRead);
        //    //AuthorizationRules.AllowRead(ReviewIdProperty, canRead);

        //    ////AuthorizationRules.AllowWrite(NameProperty, canWrite);
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
            LoadProperty(ReviewIdProperty, ri.ReviewId);
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ReviewInsert", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_NAME", ReadProperty(ReviewNameProperty)));
                    command.Parameters.Add(new SqlParameter("@CONTACT_ID", ReadProperty(ContactIdProperty)));
                    command.Parameters.Add(new SqlParameter("@NEW_REVIEW_ID",System.Data.SqlDbType.Int));
                    command.Parameters["@NEW_REVIEW_ID"].Direction = System.Data.ParameterDirection.Output;
                    command.ExecuteNonQuery();
                    LoadProperty(ReviewIdProperty, command.Parameters["@NEW_REVIEW_ID"].Value);
                }
                connection.Close();
            }
        }

        protected override void DataPortal_Update()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open(); 
                //using (SqlCommand command = new SqlCommand("st_ReviewUpdate", connection))
                using (SqlCommand command = new SqlCommand("st_ReviewEditName", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ReadProperty(ReviewIdProperty)));
                    command.Parameters.Add(new SqlParameter("@REVIEW_NAME", ReadProperty(ReviewNameProperty)));
                    command.ExecuteNonQuery();
                    LoadProperty(ReviewIdProperty, true); // routine doesn't return anything but it is updating the review you are
                                                            // presently in so what could cause a sql error?
                }
                connection.Close();
            }
        }

        protected override void DataPortal_DeleteSelf()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ReviewDelete", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@Review_ID", ReadProperty(ReviewIdProperty)));
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }
        /* at the moment not used in a list
        internal static Review GetReview(SafeDataReader reader)
        {
            
            Review returnValue = new Review();
            returnValue.LoadProperty<int>(ReviewIdProperty, reader.GetInt32("Review_ID"));
            returnValue.LoadProperty<int>(ReviewIdProperty, reader.GetInt32("REVIEW_ID"));
            returnValue.LoadProperty<int>(ContactIdProperty, reader.GetInt32("CONTACT_ID"));
            returnValue.LoadProperty<string>(TitleProperty, reader.GetString("Review_TITLE"));
            returnValue.LoadProperty<int>(ReviewNoProperty, reader.GetInt32("Review_NO"));
            returnValue.LoadProperty<string>(AnswersProperty, reader.GetString("ANSWERS"));
            returnValue.LoadProperty<int>(HitsNoProperty, reader.GetInt32("HITS_NO"));

            returnValue.MarkOld();
            return returnValue;
            
        }*/

#endif

    }
}
