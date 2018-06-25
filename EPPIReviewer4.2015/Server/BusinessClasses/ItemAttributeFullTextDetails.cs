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
    public class ItemAttributeFullTextDetails : BusinessBase<ItemAttributeFullTextDetails>, IComparable
    {

#if SILVERLIGHT
    public ItemAttributeFullTextDetails() { }
#else
        private ItemAttributeFullTextDetails() { }
#endif
        public int CompareTo(object y)
        {//implements IComparable, used to sort items!
            if (y == null) return 1;
            ItemAttributeFullTextDetails yy = y as ItemAttributeFullTextDetails;
            if (yy == null) return 1;
            
            // ...and y is not null, compare for real
            if (this.IsFromPDF && !yy.IsFromPDF)
            {//put simple text on top
                return 1;
            }
            else if (!this.IsFromPDF && yy.IsFromPDF)
            {
                return -1;
            }
            else
            {//both are from the same source (txt or PDF)
                //next level is the document
                if (this.ItemDocumentId != yy.ItemDocumentId)
                {
                    return this.DocTitle.CompareTo(yy.DocTitle);
                }
                else
                {// same doc, now look at offsets
                    if (!this.IsFromPDF)//both aren't!
                    {
                        return this.TextFrom.CompareTo(yy.TextFrom);
                    }
                    else
                    {// need a little effort to get the page number...
                        string s1 = this.Text.Substring(this.Text.IndexOf("Page") + 5,this.Text.Length > 55? 50 : this.Text.Length - 5);
                        s1 = s1.Substring(0, s1.IndexOf("[¬s]") -2);
                        int i1;
                        int.TryParse(s1, out i1);
                        string s2 = yy.Text.Substring(yy.Text.IndexOf("Page") + 5, yy.Text.Length > 55 ? 50 : yy.Text.Length - 5);
                        s2 = s2.Substring(0, s2.IndexOf("[¬s]") -2);
                        int i2;
                        int.TryParse(s2, out i2);
                        return i1.CompareTo(i2);
                    }
                }
            }

        }
        

        private static PropertyInfo<Int64> ItemDocumentIdProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("ItemDocumentId", "ItemDocumentId"));
        [JsonProperty]
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

        private static PropertyInfo<Int64> ItemAttributeIdProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("ItemAttributeId", "ItemAttributeId"));
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
        private static PropertyInfo<Int64> ItemSetIdProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("ItemSetId", "ItemSetId"));
        public Int64 ItemSetId
        {
            get
            {
                return GetProperty(ItemSetIdProperty);
            }
            set
            {
                SetProperty(ItemSetIdProperty, value);
            }
        }
        private static PropertyInfo<Int64> ItemAttributeTextIdProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("ItemAttributeTextId", "ItemAttributeTextId"));
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
        private static PropertyInfo<int> TextFromProperty = RegisterProperty<int>(new PropertyInfo<int>("TextFrom", "TextFrom", 0));
        [JsonProperty]
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

        private static PropertyInfo<int> TextToProperty = RegisterProperty<int>(new PropertyInfo<int>("TextTo", "TextTo", 0));
        [JsonProperty]
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
       

        private static PropertyInfo<string> TextProperty = RegisterProperty<string>(new PropertyInfo<string>("Text", "Text", 0));
        [JsonProperty]
        public string Text
        {
            get
            {
                return GetProperty(TextProperty);
            }
            set
            {
                SetProperty(TextProperty, value);
            }
        }

        private static PropertyInfo<bool> IsFromPDFProperty = RegisterProperty<bool>(new PropertyInfo<bool>("IsFromPDF", "IsFromPDF", 0));
        [JsonProperty]
        public bool IsFromPDF
        {
            get
            {
                return GetProperty(IsFromPDFProperty);
            }
            set
            {
                SetProperty(IsFromPDFProperty, value);
            }
        }
        private static PropertyInfo<string> DocTitleProperty = RegisterProperty<string>(new PropertyInfo<string>("DocTitle", "DocTitle", 0));
        [JsonProperty]
        public string DocTitle
        {
            get
            {
                return GetProperty(DocTitleProperty);
            }
            set
            {
                SetProperty(DocTitleProperty, value);
            }
        }
        private static PropertyInfo<string> ItemArmProperty = RegisterProperty<string>(new PropertyInfo<string>("ItemArm", "ItemArm", ""));
        [JsonProperty]
        public string ItemArm
        {
            get
            {
                return GetProperty(ItemArmProperty);
            }
            set
            {
                SetProperty(ItemArmProperty, value);
            }
        }
        //protected override void AddAuthorizationRules()
        //{
        //    //string[] canRead = new string[] { "AdminUser", "RegularUser", "ReadOnlyUser" };
        //    //AuthorizationRules.AllowGet(typeof(ItemAttributeText), canRead);
        //    //AuthorizationRules.AllowRead(ItemAttributeIdProperty, canRead);
        //}

#if !SILVERLIGHT

        public static ItemAttributeFullTextDetails GetItemAttributeText(SafeDataReader reader)
        {
            ItemAttributeFullTextDetails result = new ItemAttributeFullTextDetails();
            //result.LoadProperty<Int64>(ItemAttributeTextIdProperty, reader.GetInt64("ITEM_ATTRIBUTE_TEXT_ID"));
            result.LoadProperty<Int64>(ItemDocumentIdProperty, reader.GetInt64("ITEM_DOCUMENT_ID"));
            result.LoadProperty<Int64>(ItemSetIdProperty, reader.GetInt64("ITEM_SET_ID"));
            result.LoadProperty<Int64>(ItemAttributeIdProperty, reader.GetInt64("ITEM_ATTRIBUTE_ID"));
            result.LoadProperty<Int64>(ItemAttributeTextIdProperty, reader.GetInt64("ID"));
            result.LoadProperty(TextProperty, reader.GetString("TEXT"));
            result.LoadProperty(DocTitleProperty, reader.GetString("DOCUMENT_TITLE"));
            //result.LoadProperty(ItemArmProperty, reader.GetString("ARM_NAME"));
            
            if (reader.GetInt32("IS_FROM_PDF") == 1)
            {
                result.LoadProperty(IsFromPDFProperty, true);
            }
            else
            {
                result.LoadProperty(IsFromPDFProperty, false);
                result.LoadProperty<int>(TextFromProperty, reader.GetInt32("TEXT_FROM"));
                result.LoadProperty<int>(TextToProperty, reader.GetInt32("TEXT_TO"));
            }
            return result;
        }

        


#endif
    }
}
