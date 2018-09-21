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
    public class ComparisonAttribute : BusinessBase<ComparisonAttribute>
    {
#if SILVERLIGHT
    public ComparisonAttribute() { }

        
#else
        private ComparisonAttribute() { }
#endif

        private static PropertyInfo<Int64> ComparisonAttributeIdProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("ComparisonAttributeId", "ComparisonAttributeId"));
        public Int64 ComparisonAttributeId
        {
            get
            {
                return GetProperty(ComparisonAttributeIdProperty);
            }
        }

        private static PropertyInfo<int> ComparisonIdProperty = RegisterProperty<int>(new PropertyInfo<int>("ComparisonId", "ComparisonId"));
        public int ComparisonId
        {
            get
            {
                return GetProperty(ComparisonIdProperty);
            }
        }

        private static PropertyInfo<Int64> ItemIdProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("ItemId", "ItemId"));
        public Int64 ItemId
        {
            get
            {
                return GetProperty(ItemIdProperty);
            }
        }

        private static PropertyInfo<Int64> AttributeIdProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("AttributeId", "AttributeId"));
        public Int64 AttributeId
        {
            get
            {
                return GetProperty(AttributeIdProperty);
            }
        }

        private static PropertyInfo<string> AdditionalTextProperty = RegisterProperty<string>(new PropertyInfo<string>("AdditionalText", "AdditionalText"));
        public string AdditionalText
        {
            get
            {
                return GetProperty(AdditionalTextProperty);
            }
        }

        private static PropertyInfo<int> ContactIdProperty = RegisterProperty<int>(new PropertyInfo<int>("ContactId", "ContactId"));
        public int ContactId
        {
            get
            {
                return GetProperty(ContactIdProperty);
            }
        }

        private static PropertyInfo<int> SetIdProperty = RegisterProperty<int>(new PropertyInfo<int>("SetId", "SetId"));
        public int SetId
        {
            get
            {
                return GetProperty(SetIdProperty);
            }
        }

        private static PropertyInfo<string> AttributeNameProperty = RegisterProperty<string>(new PropertyInfo<string>("AttributeName", "AttributeName"));
        public string AttributeName
        {
            get
            {
                return GetProperty(AttributeNameProperty);
            }
        }
        public string AttributeNameWithArm4HTML
        {
            get
            {
                if (GetProperty(ItemArmProperty) != "") return GetProperty(AttributeNameProperty) + " " +
                        "<span style='font-family:Times, serif; font-size: 76%;'>[Arm: " + GetProperty(ItemArmProperty) + "]</span>";
                else return GetProperty(AttributeNameProperty);
            }
        }


        private static PropertyInfo<string> ItemTitleProperty = RegisterProperty<string>(new PropertyInfo<string>("ItemTitle", "ItemTitle"));
        public string ItemTitle
        {
            get
            {
                return GetProperty(ItemTitleProperty);
            }
        }

        private static PropertyInfo<string> ItemArmProperty = RegisterProperty<string>(new PropertyInfo<string>("ItemArm", "ItemArm"));
        public string ItemArm
        {
            get
            {
                return GetProperty(ItemArmProperty);
            }
        }

        private static PropertyInfo<bool> IsCompletedProperty = RegisterProperty<bool>(new PropertyInfo<bool>("IsCompleted", "IsCompleted"));
        public bool IsCompleted
        {
            get
            {
                return GetProperty(IsCompletedProperty);
            }
        }        

        //protected override void AddAuthorizationRules()
        //{
        //    //string[] canWrite = new string[] { "AdminUser", "RegularUser" };
        //    //string[] canRead = new string[] { "AdminUser", "RegularUser", "ReadOnlyUser" };
        //    //string[] admin = new string[] { "AdminUser" };
        //    //AuthorizationRules.AllowCreate(typeof(ComparisonAttribute), admin);
        //    //AuthorizationRules.AllowDelete(typeof(ComparisonAttribute), admin);
        //    //AuthorizationRules.AllowEdit(typeof(ComparisonAttribute), canWrite);
        //    //AuthorizationRules.AllowGet(typeof(ComparisonAttribute), canRead);

        //    //AuthorizationRules.AllowRead(ComparisonAttributeIdProperty, canRead);
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
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ComparisonAttributeInsert", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@IN_GROUP_ATTRIBUTE_ID", ReadProperty(InGroupAttributeIdProperty)));
                    command.Parameters.Add(new SqlParameter("@SET_ID", ReadProperty(SetIdProperty)));
                    command.Parameters.Add(new SqlParameter("@ComparisonAttribute_DATE", ReadProperty(ComparisonAttributeDateProperty)));
                    command.Parameters.Add(new SqlParameter("@CONTACT_ID1", ReadProperty(ContactId1Property)));
                    command.Parameters.Add(new SqlParameter("@CONTACT_ID2", ReadProperty(ContactId2Property)));
                    command.Parameters.Add(new SqlParameter("@CONTACT_ID3", ReadProperty(ContactId3Property)));

                    command.Parameters.Add(new SqlParameter("@NEW_ComparisonAttribute_ID", 0));
                    command.Parameters["@NEW_ComparisonAttribute_ID"].Direction = System.Data.ParameterDirection.Output;
                    command.ExecuteNonQuery();
                    LoadProperty(ComparisonAttributeIdProperty, command.Parameters["@NEW_ComparisonAttribute_ID"].Value);
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
                using (SqlCommand command = new SqlCommand("st_ComparisonAttributeUpdate", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@ComparisonAttribute_NAME", ReadProperty(NameProperty)));
                    command.Parameters.Add(new SqlParameter("@ComparisonAttribute_ID", ReadProperty(ComparisonAttributeIdProperty)));
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
                using (SqlCommand command = new SqlCommand("st_ComparisonAttributeDelete", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@ComparisonAttribute_ID", ReadProperty(ComparisonAttributeIdProperty)));
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
            */
        }

        internal static ComparisonAttribute GetComparisonAttribute(SafeDataReader reader)
        {
            ComparisonAttribute returnValue = new ComparisonAttribute();
            returnValue.LoadProperty<Int64>(ComparisonAttributeIdProperty, reader.GetInt64("COMPARISON_ITEM_ATTRIBUTE_ID"));
            returnValue.LoadProperty<int>(ComparisonIdProperty, reader.GetInt32("COMPARISON_ID"));
            returnValue.LoadProperty<Int64>(ItemIdProperty, reader.GetInt64("ITEM_ID"));
            returnValue.LoadProperty<Int64>(AttributeIdProperty, reader.GetInt64("ATTRIBUTE_ID"));
            returnValue.LoadProperty<string>(AdditionalTextProperty, reader.GetString("ADDITIONAL_TEXT"));
            returnValue.LoadProperty<int>(ContactIdProperty, reader.GetInt32("CONTACT_ID"));
            returnValue.LoadProperty<int>(SetIdProperty, reader.GetInt32("SET_ID"));
            returnValue.LoadProperty<string>(AttributeNameProperty, reader.GetString("ATTRIBUTE_NAME"));
            returnValue.LoadProperty<string>(ItemTitleProperty, reader.GetString("TITLE"));
            returnValue.LoadProperty<bool>(IsCompletedProperty, reader.GetBoolean("IS_COMPLETED"));
            returnValue.LoadProperty<string>(ItemArmProperty, reader.GetString("ARM_NAME"));
            returnValue.MarkOld();
            return returnValue;
        }

#endif

    }
}
