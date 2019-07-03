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
using Newtonsoft.Json;

#if !SILVERLIGHT
using System.Data.SqlClient;
using BusinessLibrary.Data;
using Csla.Data;
using BusinessLibrary.Security;
#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public class OutcomeItemAttribute : BusinessBase<OutcomeItemAttribute>
    {

    public OutcomeItemAttribute() { }

        public static readonly PropertyInfo<int> OutcomeItemAttributeIdProperty = RegisterProperty<int>(new PropertyInfo<int>("OutcomeItemAttributeId", "OutcomeItemAttributeId"));
        [JsonProperty]
        public int OutcomeItemAttributeId
        {
            get
            {
                return GetProperty(OutcomeItemAttributeIdProperty);
            }
        }

        public static readonly PropertyInfo<int> OutcomeIdProperty = RegisterProperty<int>(new PropertyInfo<int>("OutcomeId", "OutcomeId"));
        [JsonProperty]
        public int OutcomeId
        {
            get
            {
                return GetProperty(OutcomeIdProperty);
            }
            set
            {
                SetProperty(OutcomeIdProperty, value);
            }
        }

        public static readonly PropertyInfo<Int64> AttributeIdProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("AttributeId", "AttributeId"));
        [JsonProperty]
        public Int64 AttributeId
        {
            get
            {
                return GetProperty(AttributeIdProperty);
            }
            set
            {
                SetProperty(AttributeIdProperty, value);
            }
        }

        public static readonly PropertyInfo<string> AdditionalTextProperty = RegisterProperty<string>(new PropertyInfo<string>("AdditionalText", "AdditionalText", string.Empty));
        [JsonProperty]
        public string AdditionalText
        {
            get
            {
                return GetProperty(AdditionalTextProperty);
            }
            set
            {
                SetProperty(AdditionalTextProperty, value);
            }
        }
        public static readonly PropertyInfo<string> AttributeNameProperty = RegisterProperty<string>(new PropertyInfo<string>("Classifications", "Classifications"));
        [JsonProperty]
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
        //protected override void AddAuthorizationRules()
        //{
        //    //string[] canWrite = new string[] { "AdminUser", "RegularUser" };
        //    //string[] canRead = new string[] { "AdminUser", "RegularUser", "ReadOnlyUser" };
        //    //string[] admin = new string[] { "AdminUser" };
        //    //AuthorizationRules.AllowCreate(typeof(OutcomeItemAttribute), admin);
        //    //AuthorizationRules.AllowDelete(typeof(OutcomeItemAttribute), admin);
        //    //AuthorizationRules.AllowEdit(typeof(OutcomeItemAttribute), canWrite);
        //    //AuthorizationRules.AllowGet(typeof(OutcomeItemAttribute), canRead);

        //    //AuthorizationRules.AllowRead(OutcomeItemAttributeIdProperty, canRead);
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
                using (SqlCommand command = new SqlCommand("st_OutcomeItemAttributeInsert", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@ITEM_ID_PRIMARY", ReadProperty(ItemIdPrimaryProperty)));
                    command.Parameters.Add(new SqlParameter("@ITEM_ID_SECONDARY", ReadProperty(ItemIdSecondaryProperty)));
                    command.Parameters.Add(new SqlParameter("@LINK_DESCRIPTION", ReadProperty(DescriptionProperty)));
                    command.Parameters.Add(new SqlParameter("@NEW_ITEM_LINK_ID", 0));
                    command.Parameters["@NEW_ITEM_LINK_ID"].Direction = System.Data.ParameterDirection.Output;
                    command.ExecuteNonQuery();
                    LoadProperty(OutcomeItemAttributeIdProperty, command.Parameters["@NEW_ITEM_LINK_ID"].Value);
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
                using (SqlCommand command = new SqlCommand("st_OutcomeItemAttributeUpdate", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@ITEM_LINK_ID", ReadProperty(OutcomeItemAttributeIdProperty)));
                    command.Parameters.Add(new SqlParameter("@ITEM_LINK_SECONDARY", ReadProperty(ItemIdSecondaryProperty)));
                    command.Parameters.Add(new SqlParameter("@LINK_DESCRIPTION", ReadProperty(DescriptionProperty)));
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
                using (SqlCommand command = new SqlCommand("st_OutcomeItemAttributeDelete", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@ITEM_LINK_ID", ReadProperty(OutcomeItemAttributeIdProperty)));
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
             */
        }

        internal static OutcomeItemAttribute GetOutcomeItemAttribute(SafeDataReader reader)
        {
            OutcomeItemAttribute returnValue = new OutcomeItemAttribute();
            returnValue.LoadProperty<int>(OutcomeItemAttributeIdProperty, reader.GetInt32("ITEM_OUTCOME_ATTRIBUTE_ID"));
            returnValue.LoadProperty<int>(OutcomeIdProperty, reader.GetInt32("OUTCOME_ID"));
            returnValue.LoadProperty<Int64>(AttributeIdProperty, reader.GetInt64("ATTRIBUTE_ID"));
            returnValue.LoadProperty<string>(AdditionalTextProperty, reader.GetString("ADDITIONAL_TEXT"));
            returnValue.LoadProperty(AttributeNameProperty, reader.GetString("ATTRIBUTE_NAME"));
            returnValue.MarkOld();
            return returnValue;
        }

#endif

    }
}
