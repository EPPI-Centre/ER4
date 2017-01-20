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
    public class ItemSet : BusinessBase<ItemSet>
    {

#if SILVERLIGHT
    public ItemSet()
    {   //this is necessary for the coding only interface. In that bit, only itemSets that belong to the current user can be viewed, 
        //hence the object needs to know who the current user is.
        BusinessLibrary.Security.ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as BusinessLibrary.Security.ReviewerIdentity;
        CurrentUser = ri.UserId;
    }
    private int CurrentUser;
    public bool isOwn
    { 
        get 
        {
            return CurrentUser == ContactId;//true if this set belongs to the current user.
        }
    }
#else
        private ItemSet()
        {
            
        }
#endif

        private static PropertyInfo<Int64> ItemSetIdProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("ItemSetId", "ItemSetId"));
        public Int64 ItemSetId
        {
            get
            {
                return GetProperty(ItemSetIdProperty);
            }
        }

        private static PropertyInfo<int> SetIdProperty = RegisterProperty<int>(new PropertyInfo<int>("SetId", "SetId", 0));
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

        private static PropertyInfo<Int64> ItemIdProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("ItemId", "ItemId"));
        public Int64 ItemId
        {
            get
            {
                return GetProperty(ItemIdProperty);
            }
            set
            {
                SetProperty(ItemIdProperty, value);
            }
        }

        private static PropertyInfo<int> ContactIdProperty = RegisterProperty<int>(new PropertyInfo<int>("ContactId", "ContactId", 0));
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

        private static PropertyInfo<string> ContactNameProperty = RegisterProperty<string>(new PropertyInfo<string>("ContactName", "ContactName", string.Empty));
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

        private static PropertyInfo<string> SetNameProperty = RegisterProperty<string>(new PropertyInfo<string>("SetName", "SetName", string.Empty));
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

        private static PropertyInfo<bool> IsCompletedProperty = RegisterProperty<bool>(new PropertyInfo<bool>("IsCompleted", "IsCompleted", false));
        public bool IsCompleted
        {
            get
            {
                return GetProperty(IsCompletedProperty);
            }
            set
            {
                SetProperty(IsCompletedProperty, value);
            }
        }

        private static PropertyInfo<bool> IsLockedProperty = RegisterProperty<bool>(new PropertyInfo<bool>("IsLocked", "IsLocked", true));
        public bool IsLocked
        {
            get
            {
                return GetProperty(IsLockedProperty)||  
                    !Csla.Rules.BusinessRules.HasPermission( AuthorizationActions.EditObject, this);
                //| !Csla.Security.AuthorizationRules.CanEditObject(this.GetType());
            }
            set
            {
                SetProperty(IsLockedProperty, value );
            }
        }

        private static PropertyInfo<ReadOnlyItemAttributeList> ItemAttributesProperty = RegisterProperty<ReadOnlyItemAttributeList>(new PropertyInfo<ReadOnlyItemAttributeList>("ItemAttributes", "ItemAttributes"));
        public ReadOnlyItemAttributeList ItemAttributes
        {
            get
            {
                return GetProperty(ItemAttributesProperty);
            }
            set
            {
                SetProperty(ItemAttributesProperty, value);
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
        private static PropertyInfo<bool> IsSelectedProperty = RegisterProperty<bool>(new PropertyInfo<bool>("IsSelected", "IsSelected", false));
        public bool IsSelected
        {
            get
            {
                return GetProperty(IsSelectedProperty);
            }
            set
            {
                SetProperty(IsSelectedProperty, value);
            }
        }

        private static PropertyInfo<OutcomeItemList> OutcomeItemListProperty = RegisterProperty<OutcomeItemList>(new PropertyInfo<OutcomeItemList>("OutcomeItemList", "OutcomeItemList"));
        [JsonProperty]
        public OutcomeItemList OutcomeItemList
        {
            get
            {
                return GetProperty(OutcomeItemListProperty);
            }
            set
            {
                SetProperty(OutcomeItemListProperty, value);
            }
        }

        //protected override void AddAuthorizationRules()
        //{
        //    //string[] canWrite = new string[] { "AdminUser", "RegularUser", "ReadOnlyUser" };
        //    //string[] canRead = new string[] { "AdminUser", "RegularUser", "ReadOnlyUser" };
        //    //string[] admin = new string[] { "AdminUser" };
        //    //AuthorizationRules.AllowCreate(typeof(ItemSet), admin);
        //    //AuthorizationRules.AllowDelete(typeof(ItemSet), admin);
        //    //AuthorizationRules.AllowEdit(typeof(ItemSet), canWrite);
        //    //AuthorizationRules.AllowGet(typeof(ItemSet), canRead);
        //    string[] denyEditSave = new string[] { "ReadOnlyUser" };
        //    AuthorizationRules.DenyEdit(typeof(ItemSet), denyEditSave);
        //}

        protected override void AddBusinessRules()
        {
            BusinessRules.AddRule(new IsNotInRole(AuthorizationActions.EditObject, "ReadOnlyUser"));
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
                using (SqlCommand command = new SqlCommand("st_ItemSetInsert", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@SET_ID", ReadProperty(SetIdProperty)));
                    command.Parameters.Add(new SqlParameter("@PARENT_ATTRIBUTE_ID", ReadProperty(ParentAttributeIdProperty)));
                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_TYPE_ID", ReadProperty(AttributeTypeIdProperty)));
                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_SET_DESC", ReadProperty(ItemSetDescriptionProperty)));
                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ORDER", ReadProperty(AttributeOrderProperty)));
                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_NAME", ReadProperty(AttributeNameProperty)));
                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_DESC", ReadProperty(AttributeDescriptionProperty)));
                    command.Parameters.Add(new SqlParameter("@CONTACT_ID", ReadProperty(ContactIdProperty)));

                    command.Parameters.Add(new SqlParameter("@NEW_ATTRIBUTE_SET_ID", 0));
                    command.Parameters["@NEW_ATTRIBUTE_SET_ID"].Direction = System.Data.ParameterDirection.Output;
                    command.Parameters.Add(new SqlParameter("@NEW_ATTRIBUTE_ID", 0));
                    command.Parameters["@NEW_ATTRIBUTE_ID"].Direction = System.Data.ParameterDirection.Output;
                    command.ExecuteNonQuery();
                    LoadProperty(ItemSetIdProperty, command.Parameters["@NEW_ATTRIBUTE_SET_ID"].Value);
                    LoadProperty(AttributeIdProperty, command.Parameters["@NEW_ATTRIBUTE_ID"].Value);
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
                using (SqlCommand command = new SqlCommand("st_ItemSetUpdate", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_SET_ID", ReadProperty(ItemSetIdProperty)));
                    command.Parameters.Add(new SqlParameter("@SET_ID", ReadProperty(SetIdProperty)));
                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ID", ReadProperty(AttributeIdProperty)));
                    command.Parameters.Add(new SqlParameter("@PARENT_ATTRIBUTE_ID", ReadProperty(ParentAttributeIdProperty)));
                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_TYPE_ID", ReadProperty(AttributeTypeIdProperty)));
                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_SET_DESC", ReadProperty(ItemSetDescriptionProperty)));
                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ORDER", ReadProperty(AttributeOrderProperty)));
                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_NAME", ReadProperty(AttributeNameProperty)));
                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_DESC", ReadProperty(AttributeDescriptionProperty)));
                    command.Parameters.Add(new SqlParameter("@CONTACT_ID", ReadProperty(ContactIdProperty)));
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
                using (SqlCommand command = new SqlCommand("st_ItemSetDelete", connection))
                {
                    ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_SET_ID", ReadProperty(ItemSetIdProperty)));
                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ID", ReadProperty(AttributeIdProperty)));
                    command.Parameters.Add(new SqlParameter("@PARENT_ATTRIBUTE_ID", ReadProperty(ParentAttributeIdProperty)));
                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ORDER", ReadProperty(AttributeOrderProperty)));
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
            */
        }
        protected void DataPortal_Fetch(ItemSetSelectionCriteria Criteria)
        {

            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ItemSingleSetDataList", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@CONTACT_ID", ri.UserId));
                    command.Parameters.Add(new SqlParameter("@ITEM_ID", Criteria.ItemId));
                    command.Parameters.Add(new SqlParameter("@SET_ID", Criteria.SetId));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        if (reader.Read())
                        {
                            this.LoadProperty<Int64>(ItemSetIdProperty, reader.GetInt64("ITEM_SET_ID"));
                            this.LoadProperty<bool>(IsCompletedProperty, reader.GetBoolean("IS_COMPLETED"));
                            this.LoadProperty<bool>(IsLockedProperty, reader.GetBoolean("IS_LOCKED"));
                            this.LoadProperty<int>(ContactIdProperty, reader.GetInt32("CONTACT_ID"));
                            this.LoadProperty<string>(ContactNameProperty, reader.GetString("CONTACT_NAME"));
                            this.LoadProperty<string>(SetNameProperty, reader.GetString("SET_NAME"));
                            this.ItemAttributes = ReadOnlyItemAttributeList.GetReadOnlyItemAttributeList(this.ItemSetId);
                            this.OutcomeItemList = OutcomeItemList.GetOutcomeItemList(this.ItemSetId);
                            this.MarkOld();
                        }
                        else
                        {//make sure the item_set is not marked as locked if there is no coding for this item
                            this.LoadProperty<bool>(IsLockedProperty, false);
                        }
                        //even if the query returned 0 rows (item doesn't have codes for this set), on client we need this set to have setid and itemid
                        this.LoadProperty<Int64>(ItemIdProperty, Criteria.ItemId);
                        this.LoadProperty<int>(SetIdProperty, Criteria.SetId);
                    }
                }
                connection.Close();
            }
            
            
        }
        internal static ItemSet GetItemSet(SafeDataReader reader)
        {
            ItemSet returnValue = new ItemSet();
            returnValue.LoadProperty<Int64>(ItemSetIdProperty, reader.GetInt64("ITEM_SET_ID"));
            returnValue.LoadProperty<int>(SetIdProperty, reader.GetInt32("SET_ID"));
            returnValue.LoadProperty<Int64>(ItemIdProperty, reader.GetInt64("ITEM_ID"));
            returnValue.LoadProperty<bool>(IsCompletedProperty, reader.GetBoolean("IS_COMPLETED"));
            returnValue.LoadProperty<bool>(IsLockedProperty, reader.GetBoolean("IS_LOCKED"));
            returnValue.LoadProperty<int>(ContactIdProperty, reader.GetInt32("CONTACT_ID"));
            returnValue.LoadProperty<string>(ContactNameProperty, reader.GetString("CONTACT_NAME"));
            returnValue.LoadProperty<string>(SetNameProperty, reader.GetString("SET_NAME"));
            returnValue.ItemAttributes = ReadOnlyItemAttributeList.GetReadOnlyItemAttributeList(returnValue.ItemSetId);
            returnValue.OutcomeItemList = OutcomeItemList.GetOutcomeItemList(returnValue.ItemSetId);
            returnValue.MarkOld();
            return returnValue;
        }

#endif
    }
    [Serializable]
    public class ItemSetSelectionCriteria : Csla.CriteriaBase<ItemSetSelectionCriteria>
    {
        private static PropertyInfo<int> SetIdProperty = RegisterProperty<int>(typeof(ItemSetSelectionCriteria), new PropertyInfo<int>("SetId", "SetId"));
        public int SetId
        {
            get { return ReadProperty(SetIdProperty); }
        }
        private static PropertyInfo<Int64> ItemIdProperty = RegisterProperty<Int64>(typeof(ItemSetSelectionCriteria), new PropertyInfo<Int64>("ItemId", "ItemId"));
        public Int64 ItemId
        {
            get { return ReadProperty(ItemIdProperty); }
        }

        public ItemSetSelectionCriteria(int SetID, Int64 ItemID)
        //: base(type)
        {
            LoadProperty(SetIdProperty, SetID);
            LoadProperty(ItemIdProperty, ItemID);
        }

        
#if !SILVERLIGHT
        public ItemSetSelectionCriteria() { }
#endif
    }
}
