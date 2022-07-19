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
    public class Contact : BusinessBase<Contact>
    {
        /*
        #if SILVERLIGHT
            public Contact() { }


        #else
                public Contact() { }
        #endif
        */

        /*
        public Contact(int contactId, string ContactName, string username, string email, string OldPassword, string NewPassword)
        {
            SetProperty(ContactIdProperty, contactId);
            SetProperty(contactNameProperty, ContactName);
            SetProperty(UsernameProperty, username);
            SetProperty(EmailProperty, email);
            SetProperty(OldPasswordProperty, OldPassword);
            SetProperty(NewPasswordProperty, NewPassword);
        }
        */
        public Contact() { }

        public override string ToString()
        {
            return contactName;
        }



        #region properties





        public static readonly PropertyInfo<int> ContactIdProperty = RegisterProperty<int>(new PropertyInfo<int>("contactId", "contactId"));
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

        public static readonly PropertyInfo<string> UsernameProperty = RegisterProperty<string>(new PropertyInfo<string>("username", "username"));
        public string Username
        {
            get
            {
                return GetProperty(UsernameProperty);
            }
            set
            {
                SetProperty(UsernameProperty, value);
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

        public static readonly PropertyInfo<string> OldPasswordProperty = RegisterProperty<string>(new PropertyInfo<string>("OldPassword", "OldPassword"));
        public string OldPassword
        {
            get
            {
                return GetProperty(OldPasswordProperty);
            }
            set
            {
                SetProperty(OldPasswordProperty, value);
            }
        }

        public static readonly PropertyInfo<string> NewPasswordProperty = RegisterProperty<string>(new PropertyInfo<string>("NewPassword", "NewPassword"));
        public string NewPassword
        {
            get
            {
                return GetProperty(NewPasswordProperty);
            }
            set
            {
                SetProperty(NewPasswordProperty, value);
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
            set
            {
                SetProperty(IsExpiredProperty, value);
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

        public static readonly PropertyInfo<Int64> ResultProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("Result", "Result"));
        public Int64 Result
        {
            get
            {
                return GetProperty(ResultProperty);
            }
        }

        #endregion
        //protected override void AddAuthorizationRules()
        //{
        //    //string[] canWrite = new string[] { "AdminUser", "RegularUser" };
        //    //string[] canRead = new string[] { "AdminUser", "RegularUser", "ReadOnlyUser" };
        //    //string[] admin = new string[] { "AdminUser" };
        //    //AuthorizationRules.AllowCreate(typeof(Contact), admin);
        //    //AuthorizationRules.AllowDelete(typeof(Contact), admin);
        //    //AuthorizationRules.AllowEdit(typeof(Contact), canWrite);
        //    //AuthorizationRules.AllowGet(typeof(Contact), canRead);

        //    //AuthorizationRules.AllowRead(ContactIdProperty, canRead);
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
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ContactInsert", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ReadProperty(ReviewIdProperty)));
                    command.Parameters.Add(new SqlParameter("@SET_TYPE_ID", 3)); // All set to review specific keywords atm
                    command.Parameters.Add(new SqlParameter("@ALLOW_CODING_EDITS", ReadProperty(AllowCodingEditsProperty)));
                    command.Parameters.Add(new SqlParameter("@SET_NAME", ReadProperty(SetNameProperty)));
                    command.Parameters.Add(new SqlParameter("@CODING_IS_FINAL", ReadProperty(CodingIsFinalProperty)));

                    command.Parameters.Add(new SqlParameter("@NEW_REVIEW_SET_ID", 0));
                    command.Parameters["@NEW_REVIEW_SET_ID"].Direction = System.Data.ParameterDirection.Output;
                    command.Parameters.Add(new SqlParameter("@NEW_SET_ID", 0));
                    command.Parameters["@NEW_SET_ID"].Direction = System.Data.ParameterDirection.Output;
                    command.ExecuteNonQuery();
                    LoadProperty(ContactIdProperty, command.Parameters["@NEW_REVIEW_SET_ID"].Value);
                    LoadProperty(SetIdProperty, command.Parameters["@NEW_SET_ID"].Value);
                    
                }
                connection.Close();
            }*/
        }

        
 
       
        protected override void DataPortal_Update()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.AdmConnectionString))
            {
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ContactDetailsEdit", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@CONTACT_ID", ri.UserId));//guarantee we change things ONLY for the current user
                    command.Parameters.Add(new SqlParameter("@CONTACT_NAME", ReadProperty(contactNameProperty)));
                    command.Parameters.Add(new SqlParameter("@USERNAME", ReadProperty(UsernameProperty)));
                    command.Parameters.Add(new SqlParameter("@EMAIL", ReadProperty(EmailProperty)));
                    if (ReadProperty(NewPasswordProperty).Length > 0) //only pass the password data IF a new password is supplied
                    { 
                        command.Parameters.Add(new SqlParameter("@OLD_PASSWORD", ReadProperty(OldPasswordProperty)));
                        command.Parameters.Add(new SqlParameter("@NEW_PASSWORD", ReadProperty(NewPasswordProperty)));
                    }
                    else
                    {
                        command.Parameters.Add(new SqlParameter("@OLD_PASSWORD", ""));
                        command.Parameters.Add(new SqlParameter("@NEW_PASSWORD", ""));
                    }
                    command.Parameters.Add(new SqlParameter("@RESULT", 0));
                    command.Parameters["@RESULT"].Direction = System.Data.ParameterDirection.Output;
                    command.ExecuteNonQuery();
                    LoadProperty(ResultProperty, command.Parameters["@RESULT"].Value);
                }
                connection.Close();
            }
        }

        protected override void DataPortal_DeleteSelf()
        {
            /*
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ContactDelete", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_SET_ID", ReadProperty(ContactIdProperty)));
                    command.Parameters.Add(new SqlParameter("@SET_ID", ReadProperty(SetIdProperty)));
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
             */
        }



        protected void DataPortal_Fetch(SingleCriteria<Contact, int> criteria)
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.AdmConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ContactDetails", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add("@CONTACT_ID", System.Data.SqlDbType.Int);
                    command.Parameters["@CONTACT_ID"].Value = criteria.Value;
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        while (reader.Read())
                        {
                            Child_Fetch(reader);
                        }
                    }
                }
                connection.Close();
            }
        }

        internal static Contact GetUserAccountDetails(SafeDataReader reader)
        {
            return DataPortal.FetchChild<Contact>(reader);
        }

        private void Child_Fetch(SafeDataReader reader)
        {
            LoadProperty<int>(ContactIdProperty, reader.GetInt32("CONTACT_ID"));
            LoadProperty<string>(contactNameProperty, reader.GetString("CONTACT_NAME"));
            LoadProperty<string>(EmailProperty, reader.GetString("EMAIL"));
            LoadProperty<string>(UsernameProperty, reader.GetString("USERNAME"));
        }

        internal static Contact GetContact(SafeDataReader reader)
        {
            Contact returnValue = new Contact();
            returnValue.LoadProperty<int>(ContactIdProperty, reader.GetInt32("CONTACT_ID"));
            returnValue.LoadProperty<string>(contactNameProperty, reader.GetString("CONTACT_NAME"));
            returnValue.LoadProperty<string>(EmailProperty, reader.GetString("EMAIL"));
            returnValue.LoadProperty<string>(ExpiryProperty, reader.GetString("EXPIRY_DATE"));
            returnValue.LoadProperty<string>(RoleProperty, reader.GetString("ROLE_NAME"));
            returnValue.LoadProperty<int>(IsExpiredProperty, reader.GetInt32("IS_EXPIRED"));
            returnValue.MarkOld();
            return returnValue;
        }

#endif

    }
}
