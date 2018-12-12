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
    public class ItemAttributeText : BusinessBase<ItemAttributeText>
    {
    public ItemAttributeText() { }

        internal static ItemAttributeText CreateNew(ItemAttributeFullTextDetails source)
        {
            if (source.IsFromPDF) return null;
            else
            {
                ItemAttributeText result = new ItemAttributeText();
                result.ItemAttributeId = source.ItemAttributeId;
                result.ItemAttributeTextId = source.ItemAttributeTextId;
                result.ItemDocumentId = source.ItemDocumentId;
                result.TextFrom = source.TextFrom;
                result.TextTo = source.TextTo;
                result.MarkOld();
                return result;
            }
        }
        public static readonly PropertyInfo<Int64> ItemAttributeTextIdProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("ItemAttributeTextId", "ItemAttributeTextId"));
        public Int64 ItemAttributeTextId
        {
            get
            {
                return GetProperty(ItemAttributeTextIdProperty);
            }
            set
            {
                SetProperty(ItemAttributeTextIdProperty, value);
            }
        }

        public static readonly PropertyInfo<Int64> ItemDocumentIdProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("ItemDocumentId", "ItemDocumentId"));
        public Int64 ItemDocumentId
        {
            get
            {
                return GetProperty(ItemDocumentIdProperty);
            }
            set
            {
                SetProperty(ItemDocumentIdProperty, value);
            }
        }

        public static readonly PropertyInfo<Int64> ItemAttributeIdProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("ItemAttributeId", "ItemAttributeId"));
        public Int64 ItemAttributeId
        {
            get
            {
                return GetProperty(ItemAttributeIdProperty);
            }
            set
            {
                SetProperty(ItemAttributeIdProperty, value);
            }
        }

        public static readonly PropertyInfo<int> TextFromProperty = RegisterProperty<int>(new PropertyInfo<int>("TextFrom", "TextFrom", 0));
        public int TextFrom
        {
            get
            {
                return GetProperty(TextFromProperty);
            }
            set
            {
                SetProperty(TextFromProperty, value);
            }
        }

        public static readonly PropertyInfo<int> TextToProperty = RegisterProperty<int>(new PropertyInfo<int>("TextTo", "TextTo", 0));
        public int TextTo
        {
            get
            {
                return GetProperty(TextToProperty);
            }
            set
            {
                SetProperty(TextToProperty, value);
            }
        }

        //protected override void AddAuthorizationRules()
        //{
        //    //string[] canRead = new string[] { "AdminUser", "RegularUser", "ReadOnlyUser" };
        //    //AuthorizationRules.AllowGet(typeof(ItemAttributeText), canRead);
        //    //AuthorizationRules.AllowRead(ItemAttributeIdProperty, canRead);
        //}

#if !SILVERLIGHT

        public static ItemAttributeText GetItemAttributeText(SafeDataReader reader)
        {
            ItemAttributeText returnValue = new ItemAttributeText();
            returnValue.LoadProperty<Int64>(ItemAttributeTextIdProperty, reader.GetInt64("ITEM_ATTRIBUTE_TEXT_ID"));
            returnValue.LoadProperty<Int64>(ItemDocumentIdProperty, reader.GetInt64("ITEM_DOCUMENT_ID"));
            returnValue.LoadProperty<Int64>(ItemAttributeIdProperty, reader.GetInt64("ITEM_ATTRIBUTE_ID"));
            returnValue.LoadProperty<int>(TextFromProperty, reader.GetInt32("TEXT_FROM"));
            returnValue.LoadProperty<int>(TextToProperty, reader.GetInt32("TEXT_TO"));
            return returnValue;
        }

        private void Child_Fetch(SafeDataReader reader)
        {
            LoadProperty<Int64>(ItemAttributeTextIdProperty, reader.GetInt64("ITEM_ATTRIBUTE_TEXT_ID"));
            LoadProperty<Int64>(ItemDocumentIdProperty, reader.GetInt64("ITEM_DOCUMENT_ID"));
            LoadProperty<Int64>(ItemAttributeIdProperty, reader.GetInt64("ITEM_ATTRIBUTE_ID"));
            LoadProperty<int>(TextFromProperty, reader.GetInt32("TEXT_FROM"));
            LoadProperty<int>(TextToProperty, reader.GetInt32("TEXT_TO"));
        }


#endif
    }
}
