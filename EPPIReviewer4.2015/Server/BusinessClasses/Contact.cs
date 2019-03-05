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
#if SILVERLIGHT
    public Contact() { }

        
#else
        public Contact() { }
#endif

        public override string ToString()
        {
            return ContactName;
        }

        public static readonly PropertyInfo<int> ContactIdProperty = RegisterProperty<int>(new PropertyInfo<int>("ContactId", "ContactId"));
        public int ContactId
        {
            get
            {
                return GetProperty(ContactIdProperty);
            }
        }

		public static readonly PropertyInfo<string> ContactNameProperty = RegisterProperty<string>(new PropertyInfo<string>("ContactName", "ContactName", string.Empty));
        public string ContactName
        {
            get
            {
                return GetProperty(ContactNameProperty);
            }
            set
            {
                SetProperty(ContactNameProperty, value);
            }
        }

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
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ContactInsert", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    /*
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
                    */
                }
                connection.Close();
            }
        }

        protected override void DataPortal_Update()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                /*
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ContactUpdate", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@ITEM_ID", ReadProperty(ContactIdProperty)));
                    command.Parameters.Add(new SqlParameter("@TITLE", ReadProperty(TitleProperty)));
                    //command.Parameters.Add(new SqlParameter("@AUTHORS", ReadProperty(AllowCodingEditsProperty)));
                    command.ExecuteNonQuery();
                }
                connection.Close();
                */
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

        protected void DataPortal_Fetch(SingleCriteria<Contact, int> criteria) // used to return a specific item
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_Contact", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@CONTACT_ID", criteria.Value));
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        if (reader.Read())
                        {
                            LoadProperty<int>(ContactIdProperty, reader.GetInt32("CONTACT_ID"));
                            LoadProperty<string>(ContactNameProperty, reader.GetString("CONTACT_NAME"));
                        }
                    }
                }
                connection.Close();
            }
        }

        internal static Contact GetContact(SafeDataReader reader)
        {
            Contact returnValue = new Contact();
            returnValue.LoadProperty<int>(ContactIdProperty, reader.GetInt32("CONTACT_ID"));
            returnValue.LoadProperty<string>(ContactNameProperty, reader.GetString("CONTACT_NAME"));
            returnValue.MarkOld();
            return returnValue;
        }

#endif

    }
}
