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
using BusinessLibrary.Security;
using System.Collections.ObjectModel;

#if!SILVERLIGHT
using System.Data.SqlClient;
using BusinessLibrary.Data;
using Csla.Data;
#endif


namespace BusinessLibrary.BusinessClasses
{

    [Serializable]

    public class ReviewMembers : BusinessBase<ReviewMembers>
    {

        public ReviewMembers(string Role, int ContactID)
        {
            MarkOld();
            ReviewRole = Role;
            ContactId = ContactID;
            MarkDirty();
        }

        public ReviewMembers(string EmailToUse)
        {
            Email = EmailToUse;
        }




        public ReviewMembers() { }


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

        public static readonly PropertyInfo<string> ReviewRoleProperty = RegisterProperty<string>(new PropertyInfo<string>("ReviewRole", "ReviewRole", string.Empty));
        public string ReviewRole
        {
            get
            {
                return GetProperty(ReviewRoleProperty);
            }
            set
            {
                SetProperty(ReviewRoleProperty, value);
            }
        }


        public static readonly PropertyInfo<string> EmailProperty = RegisterProperty<string>(new PropertyInfo<string>("email", "email"));
        public string Email
        {
            get
            {
                return GetProperty(EmailProperty);
            }
            set
            {
                SetProperty(EmailProperty, value);
            }
        }

        public static readonly PropertyInfo<Boolean> ResultProperty = RegisterProperty<Boolean>(new PropertyInfo<Boolean>("Result", "Result"));
        public Boolean Result
        {
            get
            {
                return GetProperty(ResultProperty);
            }
        }

        public static readonly PropertyInfo<int> ResultValueProperty = RegisterProperty<int>(new PropertyInfo<int>("ResultValue", "ResultValue"));
        public int ResultValue
        {
            get
            {
                return GetProperty(ResultValueProperty);
            }
        }


        public static readonly PropertyInfo<string> contactNameProperty = RegisterProperty<string>(new PropertyInfo<string>("ContactName", "ContactName", string.Empty));
        public string contactName
        {
            get
            {
                return GetProperty(contactNameProperty);
            }
            set
            {
                SetProperty(contactNameProperty, value);
            }
        }

        public static readonly PropertyInfo<string> RoleProperty = RegisterProperty<string>(new PropertyInfo<string>("role", "role"));
        public string Role
        {
            get
            {
                return GetProperty(RoleProperty);
            }
            set
            {
                SetProperty(RoleProperty, value);
            }
        }

        public static readonly PropertyInfo<string> ExpiryProperty = RegisterProperty<string>(new PropertyInfo<string>("expiry", "expiry"));
        public string Expiry
        {
            get
            {
                return GetProperty(ExpiryProperty);
            }
            set
            {
                SetProperty(ExpiryProperty, value);
            }
        }

        public static readonly PropertyInfo<int> IsExpiredProperty = RegisterProperty<int>(new PropertyInfo<int>("isExpired", "isExpired"));
        public int IsExpired
        {
            get
            {
                return GetProperty(IsExpiredProperty);
            }
        }


        protected override void DataPortal_Update()
        {

            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            using (SqlConnection connection = new SqlConnection(DataConnection.AdmConnectionString))
            {
                connection.Open();
                //using (SqlCommand command = new SqlCommand("st_ReviewUpdate", connection))
                using (SqlCommand command = new SqlCommand("st_ReviewRoleUpdateByContactID", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@CONTACT_ID", ReadProperty(ContactIdProperty)));
                    command.Parameters.Add(new SqlParameter("@ROLE_NAME", ReadProperty(ReviewRoleProperty)));
                    command.ExecuteNonQuery();
                    LoadProperty(ResultProperty, true); // routine doesn't return anything but it is updating the review you are
                                                        // presently in so what could cause a sql error?
                }
                connection.Close();
            }
        }


        protected override void DataPortal_Insert()
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            LoadProperty(ReviewIdProperty, ri.ReviewId);
            using (SqlConnection connection = new SqlConnection(DataConnection.AdmConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ContactAddToReview", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@EMAIL", ReadProperty(EmailProperty)));
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@RESULT", 3)); // 3 is a fail for default?...
                    command.Parameters["@RESULT"].Direction = System.Data.ParameterDirection.Output;
                    command.ExecuteNonQuery();
                    LoadProperty(ResultValueProperty, command.Parameters["@RESULT"].Value);
                }
                connection.Close();
            }
        }


        protected void DataPortal_Fetch()
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ReviewContactList", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        while (reader.Read())
                        {
                            LoadProperty<int>(ContactIdProperty, reader.GetInt32("CONTACT_ID"));
                            LoadProperty<string>(contactNameProperty, reader.GetString("CONTACT_NAME"));
                            LoadProperty<string>(EmailProperty, reader.GetString("EMAIL"));
                            LoadProperty<string>(ExpiryProperty, reader.GetString("EXPIRY_DATE"));
                            LoadProperty<string>(RoleProperty, reader.GetString("ROLE_NAME"));
                            LoadProperty<int>(IsExpiredProperty, reader.GetInt32("IS_EXPIRED"));
                        }
                    }
                }
                connection.Close();
            }
        }


        protected override void DataPortal_DeleteSelf()
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            LoadProperty(ReviewIdProperty, ri.ReviewId);
            using (SqlConnection connection = new SqlConnection(DataConnection.AdmConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ReviewRemoveMember_1", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@CONTACT_ID", ReadProperty(ContactIdProperty)));
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
            
        }


    }
}
