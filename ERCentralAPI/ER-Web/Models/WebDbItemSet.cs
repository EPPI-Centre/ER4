using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Csla;
using Csla.Security;
using Csla.Core;
using Csla.Serialization;
using Csla.Silverlight;
using Csla.Rules;
using Csla.Rules.CommonRules;
//using Csla.Validation;
using System.ComponentModel;
using Newtonsoft.Json;

#if !SILVERLIGHT
using System.Data.SqlClient;
using BusinessLibrary.Data;
using BusinessLibrary.Security;
using Csla.Data;

#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public class WebDbItemSet : ReadOnlyBase<WebDbItemSet>
    {

        public WebDbItemSet()
        {
            
        }

        //consider changing this by applying the following implementation inside ER4!
        //custom runtime JSON serialisation:
        //https://blog.rsuter.com/advanced-newtonsoft-json-dynamically-rename-or-ignore-properties-without-changing-the-serialized-class/
        public static readonly PropertyInfo<Int64> ItemSetIdProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("ItemSetId", "ItemSetId"));
#if (CSLA_NETCORE)
        [JsonProperty]
#endif
        public Int64 ItemSetId
        {
            get
            {
                return GetProperty(ItemSetIdProperty);
            }
        }

        public static readonly PropertyInfo<int> SetIdProperty = RegisterProperty<int>(new PropertyInfo<int>("SetId", "SetId", 0));
#if (CSLA_NETCORE)
        [JsonProperty]
#endif
        public int SetId
        {
            get
            {
                return GetProperty(SetIdProperty);
            }
        }

        public static readonly PropertyInfo<Int64> ItemIdProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("ItemId", "ItemId"));
#if (CSLA_NETCORE)
        [JsonProperty]
#endif
        public Int64 ItemId
        {
            get
            {
                return GetProperty(ItemIdProperty);
            }
        }

//        public static readonly PropertyInfo<int> ContactIdProperty = RegisterProperty<int>(new PropertyInfo<int>("ContactId", "ContactId", 0));
//#if (CSLA_NETCORE)
//        [JsonProperty]
//#endif
//        public int ContactId
//        {
//            get
//            {
//                return GetProperty(ContactIdProperty);
//            }
//        }

//        public static readonly PropertyInfo<string> ContactNameProperty = RegisterProperty<string>(new PropertyInfo<string>("ContactName", "ContactName", string.Empty));
//#if (CSLA_NETCORE)
//        [JsonProperty]
//#endif
//        public string ContactName
//        {
//            get
//            {
//                return GetProperty(ContactNameProperty);
//            }
//        }

        public static readonly PropertyInfo<string> SetNameProperty = RegisterProperty<string>(new PropertyInfo<string>("SetName", "SetName", string.Empty));
#if (CSLA_NETCORE)
        [JsonProperty]
#endif
        public string SetName
        {
            get
            {
                return GetProperty(SetNameProperty);
            }
        }

        public static readonly PropertyInfo<bool> IsCompletedProperty = RegisterProperty<bool>(new PropertyInfo<bool>("IsCompleted", "IsCompleted", false));
#if (CSLA_NETCORE)
        [JsonProperty]
#endif
        public bool IsCompleted
        {
            get
            {
                return GetProperty(IsCompletedProperty);
            }
        }

        public static readonly PropertyInfo<bool> IsLockedProperty = RegisterProperty<bool>(new PropertyInfo<bool>("IsLocked", "IsLocked", true));
#if (CSLA_NETCORE)
        [JsonProperty]
#endif
        public bool IsLocked
        {
            get
            {
                return GetProperty(IsLockedProperty);
            }
        }

        public static readonly PropertyInfo<WebDbReadOnlyItemAttributeList> ItemAttributesProperty = RegisterProperty<WebDbReadOnlyItemAttributeList>(new PropertyInfo<WebDbReadOnlyItemAttributeList>("ItemAttributes", "ItemAttributes"));
        public WebDbReadOnlyItemAttributeList ItemAttributes
        {
            get
            {
                return GetProperty(ItemAttributesProperty);
            }
        }
        [JsonProperty]
        public List<ReadOnlyItemAttribute> ItemAttributesList
        {
            get
            {
                return ItemAttributes.ToList<ReadOnlyItemAttribute>();
            }
        }

        public ReadOnlyItemAttribute GetItemAttributeFromIAID(Int64 ItemAttributeId)
        {
            foreach (ReadOnlyItemAttribute roia in ItemAttributes)
            {
                if (roia.ItemAttributeId == ItemAttributeId)
                    return roia;
            }
            return null;
        }
        public ReadOnlyItemAttribute GetItemAttribute(Int64 AttributeId)
        {
            foreach (ReadOnlyItemAttribute roia in ItemAttributes)
            {
                if (roia.AttributeId == AttributeId)
                    return roia;
            }
            return null;
        }
        public List<ReadOnlyItemAttribute> GetItemAttributes(Int64 AttributeId)
        {
            List<ReadOnlyItemAttribute> retVal = new List<ReadOnlyItemAttribute>();
            foreach (ReadOnlyItemAttribute roia in ItemAttributes)
            {
                if (roia.AttributeId == AttributeId)
                {
                    retVal.Add(roia);
                }
            }
            return retVal;
        }
        public static readonly PropertyInfo<bool> IsSelectedProperty = RegisterProperty<bool>(new PropertyInfo<bool>("IsSelected", "IsSelected", false));

#if (CSLA_NETCORE)
        [JsonProperty]
#endif
        public bool IsSelected
        {//refers to it being selected in the coding record tab
            get
            {
                return GetProperty(IsSelectedProperty);
            }
        }

        public static readonly PropertyInfo<OutcomeItemList> OutcomeItemListProperty = RegisterProperty<OutcomeItemList>(new PropertyInfo<OutcomeItemList>("OutcomeItemList", "OutcomeItemList"));
        [JsonProperty]
        public OutcomeItemList OutcomeItemList
        {
            get
            {
                return GetProperty(OutcomeItemListProperty);
            }
        }

        protected override void AddBusinessRules()
        {
            //BusinessRules.AddRule(new IsNotInRole(AuthorizationActions.EditObject, "ReadOnlyUser"));
        }

#if !SILVERLIGHT

        
        protected void DataPortal_Fetch(ItemSetSelectionCriteria Criteria)
        {

            //ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            
            //using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            //{
            //    connection.Open();
            //    using (SqlCommand command = new SqlCommand("st_ItemSingleSetDataList", connection))
            //    {
            //        command.CommandType = System.Data.CommandType.StoredProcedure;
            //        command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
            //        command.Parameters.Add(new SqlParameter("@CONTACT_ID", ri.UserId));
            //        command.Parameters.Add(new SqlParameter("@ITEM_ID", Criteria.ItemId));
            //        command.Parameters.Add(new SqlParameter("@SET_ID", Criteria.SetId));
            //        using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
            //        {
            //            if (reader.Read())
            //            {
            //                this.LoadProperty<Int64>(ItemSetIdProperty, reader.GetInt64("ITEM_SET_ID"));
            //                this.LoadProperty<bool>(IsCompletedProperty, reader.GetBoolean("IS_COMPLETED"));
            //                this.LoadProperty<bool>(IsLockedProperty, reader.GetBoolean("IS_LOCKED"));
            //                this.LoadProperty<int>(ContactIdProperty, reader.GetInt32("CONTACT_ID"));
            //                this.LoadProperty<string>(ContactNameProperty, reader.GetString("CONTACT_NAME"));
            //                this.LoadProperty<string>(SetNameProperty, reader.GetString("SET_NAME"));
            //                this.LoadProperty(ItemAttributesProperty, ReadOnlyItemAttributeList.GetReadOnlyItemAttributeList(this.ItemSetId));
            //                this.LoadProperty(OutcomeItemListProperty, OutcomeItemList.GetOutcomeItemList(this.ItemSetId));
            //            }
            //            else
            //            {//make sure the item_set is not marked as locked if there is no coding for this item
            //                this.LoadProperty<bool>(IsLockedProperty, false);
            //            }
            //            //even if the query returned 0 rows (item doesn't have codes for this set), on client we need this set to have setid and itemid
            //            this.LoadProperty<Int64>(ItemIdProperty, Criteria.ItemId);
            //            this.LoadProperty<int>(SetIdProperty, Criteria.SetId);
            //        }
            //    }
            //    connection.Close();
            //}
            
            
        }
        internal static WebDbItemSet GetItemSet(SafeDataReader reader, int WebDbId)
        {
            WebDbItemSet returnValue = new WebDbItemSet();
            returnValue.LoadProperty<Int64>(ItemSetIdProperty, reader.GetInt64("ITEM_SET_ID"));
            returnValue.LoadProperty<int>(SetIdProperty, reader.GetInt32("SET_ID"));
            returnValue.LoadProperty<Int64>(ItemIdProperty, reader.GetInt64("ITEM_ID"));
            returnValue.LoadProperty<bool>(IsCompletedProperty, reader.GetBoolean("IS_COMPLETED"));
            returnValue.LoadProperty<bool>(IsLockedProperty, reader.GetBoolean("IS_LOCKED"));
            //returnValue.LoadProperty<int>(ContactIdProperty, reader.GetInt32("CONTACT_ID"));
            //returnValue.LoadProperty<string>(ContactNameProperty, reader.GetString("CONTACT_NAME"));
            returnValue.LoadProperty<string>(SetNameProperty, reader.GetString("SET_NAME"));
            returnValue.LoadProperty(ItemAttributesProperty, WebDbReadOnlyItemAttributeList.GetReadOnlyItemAttributeList(returnValue.ItemSetId, WebDbId));
            returnValue.LoadProperty(OutcomeItemListProperty, OutcomeItemList.GetOutcomeItemList(returnValue.ItemSetId));
            return returnValue;
        }

#endif
    }
}
 