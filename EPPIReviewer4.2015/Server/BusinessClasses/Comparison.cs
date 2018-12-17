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
    public class Comparison : BusinessBase<Comparison>
    {
#if SILVERLIGHT
    public Comparison() { }

        
#else
        private Comparison() { }
#endif

        private static PropertyInfo<int> ComparisonIdProperty = RegisterProperty<int>(new PropertyInfo<int>("ComparisonId", "ComparisonId"));
        public int ComparisonId
        {
            get
            {
                return GetProperty(ComparisonIdProperty);
            }
        }
        private static PropertyInfo<bool> IsScreeningProperty = RegisterProperty<bool>(new PropertyInfo<bool>("IsScreening", "IsScreening"));
        public bool IsScreening
        {
            get
            {
                return GetProperty(IsScreeningProperty);
            }
        }
        private static PropertyInfo<int> ReviewIdProperty = RegisterProperty<int>(new PropertyInfo<int>("ReviewId", "ReviewId"));
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

        private static PropertyInfo<Int64> InGroupAttributeIdProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("InGroupAttributeId", "InGroupAttributeId", -1));
        public Int64 InGroupAttributeId
        {
            get
            {
                return GetProperty(InGroupAttributeIdProperty);
            }
            set
            {
                SetProperty(InGroupAttributeIdProperty, value);
            }
        }

        private static PropertyInfo<int> SetIdProperty = RegisterProperty<int>(new PropertyInfo<int>("SetId", "SetId"));
        public int SetId
        {
            get
            {
                return GetProperty(SetIdProperty);
            }
            set
            {
                SetProperty(SetIdProperty, value);
            }
        }

        private static PropertyInfo<Csla.SmartDate> ComparisonDateProperty = RegisterProperty<SmartDate>(new PropertyInfo<SmartDate>("ComparisonDate", "ComparisonDate"));
        public SmartDate ComparisonDate
        {
            get
            {
                return GetProperty(ComparisonDateProperty);
            }
            set
            {
                SetProperty(ComparisonDateProperty, value);
            }
        }

        private static PropertyInfo<int> ContactId1Property = RegisterProperty<int>(new PropertyInfo<int>("ContactId1", "ContactId1"));
        public int ContactId1
        {
            get
            {
                return GetProperty(ContactId1Property);
            }
            set
            {
                SetProperty(ContactId1Property, value);
            }
        }

        private static PropertyInfo<int> ContactId2Property = RegisterProperty<int>(new PropertyInfo<int>("ContactId2", "ContactId2"));
        public int ContactId2
        {
            get
            {
                return GetProperty(ContactId2Property);
            }
            set
            {
                SetProperty(ContactId2Property, value);
            }
        }

        private static PropertyInfo<int> ContactId3Property = RegisterProperty<int>(new PropertyInfo<int>("ContactId3", "ContactId3"));
        public int ContactId3
        {
            get
            {
                return GetProperty(ContactId3Property);
            }
            set
            {
                SetProperty(ContactId3Property, value);
            }
        }

        private static PropertyInfo<string> ContactName1Property = RegisterProperty<string>(new PropertyInfo<string>("ContactName1", "ContactName1"));
        public string ContactName1
        {
            get
            {
                return GetProperty(ContactName1Property);
            }
            set
            {
                SetProperty(ContactName1Property, value);
            }
        }

        private static PropertyInfo<string> ContactName2Property = RegisterProperty<string>(new PropertyInfo<string>("ContactName2", "ContactName2"));
        public string ContactName2
        {
            get
            {
                return GetProperty(ContactName2Property);
            }
            set
            {
                SetProperty(ContactName2Property, value);
            }
        }

        private static PropertyInfo<string> ContactName3Property = RegisterProperty<string>(new PropertyInfo<string>("ContactName3", "ContactName3"));
        public string ContactName3
        {
            get
            {
                return GetProperty(ContactName3Property);
            }
            set
            {
                SetProperty(ContactName3Property, value);
            }
        }

        private static PropertyInfo<string> AttributeNameProperty = RegisterProperty<string>(new PropertyInfo<string>("AttributeName", "AttributeName"));
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

        private static PropertyInfo<string> SetNameProperty = RegisterProperty<string>(new PropertyInfo<string>("SetName", "SetName"));
        public string SetName
        {
            get
            {
                return GetProperty(SetNameProperty);
            }
            set
            {
                SetProperty(SetNameProperty, value);
            }
        }

        //protected override void AddAuthorizationRules()
        //{
        //    //string[] canWrite = new string[] { "AdminUser", "RegularUser" };
        //    //string[] canRead = new string[] { "AdminUser", "RegularUser", "ReadOnlyUser" };
        //    //string[] admin = new string[] { "AdminUser" };
        //    //AuthorizationRules.AllowCreate(typeof(Comparison), admin);
        //    //AuthorizationRules.AllowDelete(typeof(Comparison), admin);
        //    //AuthorizationRules.AllowEdit(typeof(Comparison), canWrite);
        //    //AuthorizationRules.AllowGet(typeof(Comparison), canRead);

        //    //AuthorizationRules.AllowRead(ComparisonIdProperty, canRead);
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
                using (SqlCommand command = new SqlCommand("st_ComparisonInsert", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@IN_GROUP_ATTRIBUTE_ID", ReadProperty(InGroupAttributeIdProperty)));
                    command.Parameters.Add(new SqlParameter("@SET_ID", ReadProperty(SetIdProperty)));
                    command.Parameters.Add(new SqlParameter("@COMPARISON_DATE", DateTime.Now.Date));
                    command.Parameters.Add(new SqlParameter("@CONTACT_ID1", ReadProperty(ContactId1Property)));
                    command.Parameters.Add(new SqlParameter("@CONTACT_ID2", ReadProperty(ContactId2Property)));
                    command.Parameters.Add(new SqlParameter("@CONTACT_ID3", ReadProperty(ContactId3Property)));
                    SqlParameter par = new SqlParameter("@NEW_COMPARISON_ID", System.Data.SqlDbType.Int);
                    par.Value = 0;
                    command.Parameters.Add(par);
                    command.Parameters["@NEW_COMPARISON_ID"].Direction = System.Data.ParameterDirection.Output;
                    SqlParameter par2 = new SqlParameter("@Is_Screening", System.Data.SqlDbType.Bit);
                    par2.Value = 0;
                    par2.Direction = System.Data.ParameterDirection.Output;
                    command.Parameters.Add(par2);
                    
                    command.ExecuteNonQuery();
                    LoadProperty(ComparisonIdProperty, command.Parameters["@NEW_COMPARISON_ID"].Value);
                    LoadProperty(IsScreeningProperty, (bool)par2.Value );
                }
                connection.Close();
            }
        }

        protected override void DataPortal_Update()
        {
            /*
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ComparisonUpdate", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@Comparison_NAME", ReadProperty(NameProperty)));
                    command.Parameters.Add(new SqlParameter("@Comparison_ID", ReadProperty(ComparisonIdProperty)));
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
             */
        }

        protected override void DataPortal_DeleteSelf()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ComparisonDelete", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@COMPARISON_ID", ReadProperty(ComparisonIdProperty)));
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        internal static Comparison GetComparison(SafeDataReader reader)
        {
            Comparison returnValue = new Comparison();
            returnValue.LoadProperty<int>(ComparisonIdProperty, reader.GetInt32("COMPARISON_ID"));
            returnValue.LoadProperty<int>(ReviewIdProperty, reader.GetInt32("REVIEW_ID"));
            returnValue.LoadProperty<Int64>(InGroupAttributeIdProperty, reader.GetInt64("IN_GROUP_ATTRIBUTE_ID"));
            returnValue.LoadProperty<int>(SetIdProperty, reader.GetInt32("SET_ID"));
            returnValue.LoadProperty<SmartDate>(ComparisonDateProperty, reader.GetSmartDate("COMPARISON_DATE"));
            returnValue.LoadProperty<int>(ContactId1Property, reader.GetInt32("CONTACT_ID1"));
            returnValue.LoadProperty<int>(ContactId2Property, reader.GetInt32("CONTACT_ID2"));
            returnValue.LoadProperty<int>(ContactId3Property, reader.GetInt32("CONTACT_ID3"));
            returnValue.LoadProperty<string>(ContactName1Property, reader.GetString("CONTACT_NAME1"));
            returnValue.LoadProperty<string>(ContactName2Property, reader.GetString("CONTACT_NAME2"));
            returnValue.LoadProperty<string>(ContactName3Property, reader.GetString("CONTACT_NAME3"));
            returnValue.LoadProperty<string>(AttributeNameProperty, reader.GetString("ATTRIBUTE_NAME"));
            returnValue.LoadProperty<string>(SetNameProperty, reader.GetString("SET_NAME"));

            returnValue.MarkOld();
            return returnValue;
        }

#endif

    }
}
