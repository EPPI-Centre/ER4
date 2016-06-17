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
using System.ComponentModel;

#if!SILVERLIGHT
using Csla.Data;
using System.Data.SqlClient;
using BusinessLibrary.Data;
#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class ReadOnlyItemAttributeChildFrequency : ReadOnlyBase<ReadOnlyItemAttributeChildFrequency>
    {

#if SILVERLIGHT
    public ReadOnlyItemAttributeChildFrequency() { }
#else
        private ReadOnlyItemAttributeChildFrequency() { }
#endif

        private static PropertyInfo<Int64> AttributeIdProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("AttributeId", "AttributeId"));
        public Int64 AttributeId
        {
            get
            {
                return GetProperty(AttributeIdProperty);
            }
        }

        private static PropertyInfo<Int64> AttributeSetIdProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("AttributeSetId", "AttributeSetId"));
        public Int64 AttributeSetId
        {
            get
            {
                return GetProperty(AttributeSetIdProperty);
            }
        }
        private static PropertyInfo<Int64> FilterAttributeIdProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("FilterAttributeId", "FilterAttributeId"));
        public Int64 FilterAttributeId
        {
            get
            {
                return GetProperty(FilterAttributeIdProperty);
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

        private static PropertyInfo<string> AttributeProperty = RegisterProperty<string>(new PropertyInfo<string>("Attribute", "Attribute", string.Empty));
        public string Attribute
        {
            get
            {
                return GetProperty(AttributeProperty);
            }
        }

        private static PropertyInfo<int> ItemCountProperty = RegisterProperty<int>(new PropertyInfo<int>("ItemCount", "ItemCount"));
        public int ItemCount
        {
            get
            {
                return GetProperty(ItemCountProperty);
            }
        }
        private static PropertyInfo<bool> IsIncludedProperty = RegisterProperty<bool>(new PropertyInfo<bool>("IsIncluded", "IsIncluded"));
        public bool IsIncluded
        {
            get
            {
                return GetProperty(IsIncludedProperty);
            }
        }
        //protected override void AddAuthorizationRules()
        //{
        //    //string[] canRead = new string[] { "AdminUser", "RegularUser", "ReadOnlyUser" };
        //    //AuthorizationRules.AllowGet(typeof(ReadOnlyItemAttributeChildFrequency), canRead);
        //}

#if !SILVERLIGHT

        public static ReadOnlyItemAttributeChildFrequency GetReadOnlyItemAttributeChildFrequency(SafeDataReader reader, int SetId, Int64 FilterAttributeId, bool isIncluded)
        {
            return DataPortal.FetchChild<ReadOnlyItemAttributeChildFrequency>(reader, SetId, FilterAttributeId, isIncluded);
        }

        private void Child_Fetch(SafeDataReader reader, int SetId, Int64 FilterAttributeId, bool isIncluded)
        {
            LoadProperty<Int64>(AttributeIdProperty, reader.GetInt64("ATTRIBUTE_ID"));
            LoadProperty<int>(ItemCountProperty, reader.GetInt32("ITEM_COUNT"));
            LoadProperty<string>(AttributeProperty, reader.GetString("ATTRIBUTE_NAME"));
            LoadProperty<Int64>(AttributeSetIdProperty, reader.GetInt64("ATTRIBUTE_SET_ID"));
            LoadProperty<int>(SetIdProperty, SetId);
            LoadProperty<Int64>(FilterAttributeIdProperty, FilterAttributeId);
            LoadProperty<bool>(IsIncludedProperty, isIncluded);
        }


#endif
    }
}
