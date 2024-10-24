﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Csla;
using Csla.Security;
using Csla.Core;
using Csla.Serialization;
using Csla.Silverlight;
//using Csla.Validation;
using System.ComponentModel;
using Newtonsoft.Json;

#if !SILVERLIGHT
using Csla.Data;
using System.Data.SqlClient;
using BusinessLibrary.Data;
#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public class ReadOnlyItemAttribute : ReadOnlyBase<ReadOnlyItemAttribute>
    {


    public ReadOnlyItemAttribute() { }
    

        //private ItemAttributeTextList _ItemAttributeTextList;
        public ItemAttributeTextList ItemAttributeTextList
        {
            get
            {
                if (this.ItemAttributeFullTextList.SimpleTextList != null)
                {//build list if needed
                    ItemAttributeTextList _ItemAttributeTextList = ItemAttributeTextList.NewItemAttributeTextList();
                    foreach (ItemAttributeFullTextDetails ftd in this.ItemAttributeFullTextList.SimpleTextList)
                    {
                        _ItemAttributeTextList.Add(ItemAttributeText.CreateNew(ftd));
                    }
                    return _ItemAttributeTextList;
                }
                return null;
            }
        }
        public static readonly PropertyInfo<Int64> ItemAttributeIdProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("ItemAttributeId", "ItemAttribute Id"));
#if (CSLA_NETCORE)
        [JsonProperty]
#endif
        public Int64 ItemAttributeId
        {
            get
            {
                return GetProperty(ItemAttributeIdProperty);
            }
        }

        public static readonly PropertyInfo<Int64> ItemIdProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("ItemId", "ItemId"));
        public Int64 ItemId
        {
            get
            {
                return GetProperty(ItemIdProperty);
            }
        }

        public static readonly PropertyInfo<Int64> ItemSetIdProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("ItemSetId", "ItemSetId"));
        public Int64 ItemSetId
        {
            get
            {
                return GetProperty(ItemSetIdProperty);
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
        }

        public static readonly PropertyInfo<string> AdditionalTextProperty = RegisterProperty<string>(new PropertyInfo<string>("AdditionalText", "AdditionalText", string.Empty));
        [JsonProperty]
        public string AdditionalText
        {
            get
            {
                return GetProperty(AdditionalTextProperty);
            }
        }

        public static readonly PropertyInfo<int> ContactIdProperty = RegisterProperty<int>(new PropertyInfo<int>("ContactId", "ContactId", 0));
        public int ContactId
        {
            get
            {
                return GetProperty(ContactIdProperty);
            }
        }

        public static readonly PropertyInfo<Int64> AttributeSetIdProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("AttributeSetId", "AttributeSetId"));
        public Int64 AttributeSetId
        {
            get
            {
                return GetProperty(AttributeSetIdProperty);
            }
        }

        public static readonly PropertyInfo<Int64> ArmIdProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("ArmId", "ArmId"));
        [JsonProperty]
        public Int64 ArmId
        {
            get
            {
                return GetProperty(ArmIdProperty);
            }
        }

        public static readonly PropertyInfo<string> ArmTitleProperty = RegisterProperty<string>(new PropertyInfo<string>("ArmTitle", "ArmTitle", string.Empty));
        [JsonProperty]
        public string ArmTitle
        {
            get
            {
                return GetProperty(ArmTitleProperty);
            }
        }

        public static readonly PropertyInfo<bool> IsCompleteProperty = RegisterProperty<bool>(new PropertyInfo<bool>("IsComplete", "IsComplete"));
        public bool IsComplete
        {
            get
            {
                return GetProperty(IsCompleteProperty);
            }
        }

        public static readonly PropertyInfo<bool> IsLockedProperty = RegisterProperty<bool>(new PropertyInfo<bool>("IsLocked", "IsLocked"));
        public bool IsLocked
        {
            get
            {
                return GetProperty(IsLockedProperty);
            }
        }

        //public static readonly PropertyInfo<ItemAttributeTextList> ItemAttributeTextListProperty = RegisterProperty<ItemAttributeTextList>(new PropertyInfo<ItemAttributeTextList>("ItemAttributeTextList", "ItemAttributeTextList"));
        //public ItemAttributeTextList ItemAttributeTextList
        //{
        //    get
        //    {
        //        return GetProperty(ItemAttributeTextListProperty);
        //    }
        //}
        public static readonly PropertyInfo<ItemAttributeFullTextDetailsList> ItemAttributeFullTextListProperty = RegisterProperty<ItemAttributeFullTextDetailsList>(new PropertyInfo<ItemAttributeFullTextDetailsList>("ItemAttributeFullTextList", "ItemAttributeFullTextList"));
        public ItemAttributeFullTextDetailsList ItemAttributeFullTextList
        {
            get
            { //make sure the list itself is instanced so that we can always add items to it
                ItemAttributeFullTextDetailsList result = GetProperty(ItemAttributeFullTextListProperty);
                if (result != null)
                {
                    return result;
                }
                result = ItemAttributeFullTextDetailsList.NewItemAttributeFullTextDetailsList();
                this.LoadProperty(ItemAttributeFullTextListProperty, result);
                return GetProperty(ItemAttributeFullTextListProperty);
            }
        }
        [JsonProperty]
        public List<ItemAttributeFullTextDetails> ItemAttributeFullTextDetails
        {
            get
            {
                return ItemAttributeFullTextList.ToList<ItemAttributeFullTextDetails>();
            }
        }

        public static ReadOnlyItemAttribute ReadOnlyItemAttribute_From_ItemAttributeData(ItemAttributeData iad)
        {
            ReadOnlyItemAttribute newOb = new ReadOnlyItemAttribute();
            newOb.LoadProperty(ItemAttributeIdProperty, iad.ItemAttributeId);
            newOb.LoadProperty(ItemIdProperty, iad.ItemId);
            newOb.LoadProperty(ItemSetIdProperty, iad.ItemSetId);
            newOb.LoadProperty(AttributeIdProperty, iad.AttributeId);
            newOb.LoadProperty(AdditionalTextProperty, iad.AdditionalText);
            newOb.LoadProperty(IsLockedProperty, iad.IsLocked);
            newOb.LoadProperty(ArmIdProperty, iad.ArmId);
            newOb.LoadProperty(AttributeSetIdProperty, iad.AttributeSetId);
            return newOb;
        }
    

        //protected override void AddAuthorizationRules()
        //{
        //    //string[] canRead = new string[] { "AdminUser", "RegularUser", "ReadOnlyUser" };
        //    //AuthorizationRules.AllowGet(typeof(ReadOnlyItemAttribute), canRead);
        //    //AuthorizationRules.AllowRead(ItemAttributeIdProperty, canRead);
        //}

#if !SILVERLIGHT

        /*
        public static ReadOnlyItemAttribute GetReadOnlyItemAttribute(SafeDataReader reader)
        {
            return DataPortal.FetchChild<ReadOnlyItemAttribute>(reader);
        }

        private void Child_Fetch(SafeDataReader reader)
        {
            LoadProperty<Int64>(ItemAttributeIdProperty, reader.GetInt64("ITEM_ATTRIBUTE_ID"));
            LoadProperty<Int64>(ItemIdProperty, reader.GetInt64("ITEM_ID"));
            LoadProperty<Int64>(ItemSetIdProperty, reader.GetInt64("ITEM_SET_ID"));
            LoadProperty<Int64>(AttributeIdProperty, reader.GetInt64("ATTRIBUTE_ID"));
            LoadProperty<string>(AdditionalTextProperty, reader.GetString("ADDITIONAL_TEXT"));
            LoadProperty<int>(ContactIdProperty, reader.GetInt32("CONTACT_ID"));
            LoadProperty<Int64>(AttributeSetIdProperty, reader.GetInt64("ATTRIBUTE_SET_ID"));
            LoadProperty<ItemAttributeTextList>(ItemAttributeTextListProperty,
                ItemAttributeTextList.GetReadOnlyItemAttributeTextList(ItemAttributeId));
        }
        */

        public static ReadOnlyItemAttribute GetReadOnlyItemAttribute(SafeDataReader reader)
        {
            Random rnd = new Random();

            ReadOnlyItemAttribute returnValue = new ReadOnlyItemAttribute();
            returnValue.LoadProperty<Int64>(ItemAttributeIdProperty, reader.GetInt64("ITEM_ATTRIBUTE_ID"));
            returnValue.LoadProperty<Int64>(ItemIdProperty, reader.GetInt64("ITEM_ID"));
            returnValue.LoadProperty<Int64>(ItemSetIdProperty, reader.GetInt64("ITEM_SET_ID"));
            returnValue.LoadProperty<Int64>(AttributeIdProperty, reader.GetInt64("ATTRIBUTE_ID"));
            returnValue.LoadProperty<string>(AdditionalTextProperty, reader.GetString("ADDITIONAL_TEXT"));
            returnValue.LoadProperty<int>(ContactIdProperty, reader.GetInt32("CONTACT_ID"));
            returnValue.LoadProperty<Int64>(AttributeSetIdProperty, reader.GetInt64("ATTRIBUTE_SET_ID"));
            returnValue.LoadProperty<Int64>(ArmIdProperty, reader.GetInt64("ITEM_ARM_ID"));
            returnValue.LoadProperty<string>(ArmTitleProperty, reader.GetString("ARM_TITLE"));
            //returnValue.LoadProperty<ItemAttributeTextList>(ItemAttributeTextListProperty,
            //  ItemAttributeTextList.GetReadOnlyItemAttributeTextList(returnValue.ItemAttributeId));
            return returnValue;
        }

        private static readonly Random random = new Random();

        public static int RandomNumber(int min, int max)
        {
            // lock (syncLock)
            { // synchronize
                return random.Next(min, max);
            }
        }

#endif
    }
}
