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

#if!SILVERLIGHT
using System.Data.SqlClient;
using BusinessLibrary.Data;
using Csla.Data;
#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class ReadOnlyAttributeTextAllItems : ReadOnlyBase<ReadOnlyAttributeTextAllItems>
    {
#if SILVERLIGHT
    public ReadOnlyAttributeTextAllItems()
    {
        
    }
#else
        private ReadOnlyAttributeTextAllItems()
        {
            
        }
#endif
        public override string ToString()
        {
            return Snippet;
        }

        private static PropertyInfo<string> ItemTitleProperty = RegisterProperty<string>(new PropertyInfo<string>("ItemTitle", "ItemTitle", string.Empty));
        public string ItemTitle
        {
            get
            {
                return GetProperty(ItemTitleProperty);
            }
        }

        private static PropertyInfo<string> ItemShortTitleProperty = RegisterProperty<string>(new PropertyInfo<string>("ItemShortTitle", "ItemShortTitle", string.Empty));
        public string ItemShortTitle
        {
            get
            {
                return GetProperty(ItemShortTitleProperty);
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

        private static PropertyInfo<int> TextFromProperty = RegisterProperty<int>(new PropertyInfo<int>("TextFrom", "TextFrom", 0));
        public int TextFrom
        {
            get
            {
                return GetProperty(TextFromProperty);
            }
        }

        private static PropertyInfo<int> TextToProperty = RegisterProperty<int>(new PropertyInfo<int>("TextTo", "TextTo", 0));
        public int TextTo
        {
            get
            {
                return GetProperty(TextToProperty);
            }
        }

        private static PropertyInfo<string> AdditionalTextProperty = RegisterProperty<string>(new PropertyInfo<string>("AdditionalText", "AdditionalText", string.Empty));
        public string AdditionalText
        {
            get
            {
                return GetProperty(AdditionalTextProperty);
            }
        }
        private static PropertyInfo<string> OriginProperty = RegisterProperty<string>(new PropertyInfo<string>("Origin", "Origin", string.Empty));
        public string Origin
        {
            get
            {
                return GetProperty(OriginProperty);
            }
        }
        private static PropertyInfo<string> CodedTextProperty = RegisterProperty<string>(new PropertyInfo<string>("CodedText", "CodedText", string.Empty));
        public string CodedText
        {
            get
            {
                return GetProperty(CodedTextProperty);
            }
        }

        private static PropertyInfo<string> SnippetProperty = RegisterProperty<string>(new PropertyInfo<string>("Snippet", "Snippet", string.Empty));
        public string Snippet
        {
            get
            {
                return GetProperty(SnippetProperty);
            }
        }

        private static PropertyInfo<Int64> ItemDocumentIdProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("ItemDocumentId", "ItemDocumentId"));
        public Int64 ItemDocumentId
        {
            get
            {
                return GetProperty(ItemDocumentIdProperty);
            }
        }

        private static PropertyInfo<string> ItemDocumentTitleProperty = RegisterProperty<string>(new PropertyInfo<string>("ItemDocumentTitle", "ItemDocumentTitle", string.Empty));
        public string ItemDocumentTitle
        {
            get
            {
                return GetProperty(ItemDocumentTitleProperty);
            }
        }

        //protected override void AddAuthorizationRules()
        //{
        //    //string[] canWrite = new string[] { "AdminUser", "RegularUser" };
        //    //string[] canRead = new string[] { "AdminUser", "RegularUser", "ReadOnlyUser" };
        //    //string[] admin = new string[] { "AdminUser" };
        //    //AuthorizationRules.AllowCreate(typeof(ReadOnlyAttributeTextAllItems), admin);
        //    //AuthorizationRules.AllowDelete(typeof(ReadOnlyAttributeTextAllItems), admin);
        //    //AuthorizationRules.AllowEdit(typeof(ReadOnlyAttributeTextAllItems), canWrite);
        //    //AuthorizationRules.AllowGet(typeof(ReadOnlyAttributeTextAllItems), canRead);

        //    //AuthorizationRules.AllowRead(ItemTitleProperty, canRead);
        //    //AuthorizationRules.AllowRead(ItemIdProperty, canRead);
        //    //AuthorizationRules.AllowRead(TextFromProperty, canRead);
        //    //AuthorizationRules.AllowRead(TextToProperty, canRead);
        //    //AuthorizationRules.AllowRead(CodedTextProperty, canRead);
        //}



#if !SILVERLIGHT

        internal static ReadOnlyAttributeTextAllItems GetReadOnlyAttributeTextAllItems(SafeDataReader reader)
        {
            ReadOnlyAttributeTextAllItems returnValue = new ReadOnlyAttributeTextAllItems();
            returnValue.LoadProperty<string>(ItemTitleProperty, reader.GetString("TITLE"));
            returnValue.LoadProperty<string>(ItemShortTitleProperty, reader.GetString("SHORT_TITLE"));
            returnValue.LoadProperty<Int64>(ItemIdProperty, reader.GetInt64("ITEM_ID"));
            returnValue.LoadProperty<string>(CodedTextProperty, reader.GetString("CODED_TEXT"));
            returnValue.LoadProperty<string>(OriginProperty, reader.GetString("ORIGIN"));
            returnValue.LoadProperty<int>(TextFromProperty, reader.GetInt32("TEXT_FROM"));
            returnValue.LoadProperty<int>(TextToProperty, reader.GetInt32("TEXT_TO"));
            if (returnValue.CodedText.Length > 0)
            {
                returnValue.LoadProperty<string>(SnippetProperty, returnValue.CodedText.Substring(0, Math.Min(30, returnValue.CodedText.Length - 1)) + "...");
            }
            returnValue.LoadProperty<Int64>(ItemDocumentIdProperty, reader.GetInt64("ITEM_DOCUMENT_ID"));
            returnValue.LoadProperty<string>(ItemDocumentTitleProperty, reader.GetString("DOCUMENT_TITLE"));
            returnValue.LoadProperty<string>(AdditionalTextProperty, reader.GetString("ADDITIONAL_TEXT"));

            return returnValue;
        }

#endif
    }
}
