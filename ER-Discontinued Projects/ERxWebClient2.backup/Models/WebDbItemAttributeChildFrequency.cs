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
    public class WebDbItemAttributeChildFrequency : ReadOnlyBase<WebDbItemAttributeChildFrequency>
    {

#if SILVERLIGHT
    public ReadOnlyItemAttributeChildFrequency() { }
#else
        public WebDbItemAttributeChildFrequency() { }
#endif

        public static readonly PropertyInfo<Int64> AttributeIdProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("AttributeId", "AttributeId"));
        public Int64 AttributeId
        {
            get
            {
                return GetProperty(AttributeIdProperty);
            }
        }

		//public static readonly PropertyInfo<Int64> AttributeSetIdProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("AttributeSetId", "AttributeSetId"));
  //      public Int64 AttributeSetId
  //      {
  //          get
  //          {
  //              return GetProperty(AttributeSetIdProperty);
  //          }
  //      }
		public static readonly PropertyInfo<Int64> FilterAttributeIdProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("FilterAttributeId", "FilterAttributeId"));
        public Int64 FilterAttributeId
        {
            get
            {
                return GetProperty(FilterAttributeIdProperty);
            }
        }
		public static readonly PropertyInfo<int> SetIdProperty = RegisterProperty<int>(new PropertyInfo<int>("SetId", "SetId"));
        public int SetId
        {
            get
            {
                return GetProperty(SetIdProperty);
            }
        }

		public static readonly PropertyInfo<string> AttributeProperty = RegisterProperty<string>(new PropertyInfo<string>("Attribute", "Attribute", string.Empty));
        public string Attribute
        {
            get
            {
                return GetProperty(AttributeProperty);
            }
        }

		public static readonly PropertyInfo<int> ItemCountProperty = RegisterProperty<int>(new PropertyInfo<int>("ItemCount", "ItemCount"));
        public int ItemCount
        {
            get
            {
                return GetProperty(ItemCountProperty);
            }
        }
		public static readonly PropertyInfo<string> IsIncludedProperty = RegisterProperty<string>(new PropertyInfo<string>("IsIncluded", "IsIncluded", ""));
        public string IsIncluded
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

        public static WebDbItemAttributeChildFrequency GetReadOnlyItemAttributeChildFrequency(string AttName, long AttId, int ItemCound, int SetId, long FilterAttributeId, string isIncluded)
        {
            return DataPortal.FetchChild<WebDbItemAttributeChildFrequency>(AttName,  AttId,  ItemCound, SetId, FilterAttributeId, isIncluded);
        }

        private void Child_Fetch(string AttName, long AttId, int ItemCound, int SetId, Int64 FilterAttributeId, string isIncluded)
        {
            LoadProperty<Int64>(AttributeIdProperty, AttId);
            LoadProperty<int>(ItemCountProperty, ItemCound);
            LoadProperty<string>(AttributeProperty, AttName);
            //LoadProperty<Int64>(AttributeSetIdProperty, reader.GetInt64("ATTRIBUTE_SET_ID"));
            LoadProperty<int>(SetIdProperty, SetId);
            LoadProperty<Int64>(FilterAttributeIdProperty, FilterAttributeId);
            LoadProperty<string>(IsIncludedProperty, isIncluded);
        }


#endif
    }
}
