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
    public class ItemDuplicatesReadOnly : ReadOnlyBase<ItemDuplicatesReadOnly>
    {

#if SILVERLIGHT
    public ItemDuplicatesReadOnly() { }
#else
        private ItemDuplicatesReadOnly() { }
#endif
        public override string ToString()
        {
            return this.ShortTitle;
        }

        //private static PropertyInfo<int> GroupIdProperty = RegisterProperty<int>(new PropertyInfo<int>("GroupId", "GroupId"));
        //public int GroupId
        //{
        //    get
        //    {
        //        return GetProperty(GroupIdProperty);
        //    }
        //}
        //SHORT_TITLE, I.ITEM_ID, SOURCE_NAME, ITEM_DUPLICATE_GROUP_ID GROUP_ID
        private static PropertyInfo<Int64> ItemIdProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("ItemId", "ItemId"));
        public Int64 ItemId
        {
            get
            {
                return GetProperty(ItemIdProperty);
            }
        }
        private static PropertyInfo<string> ShortTitleProperty = RegisterProperty<string>(new PropertyInfo<string>("ShortTitle", "ShortTitle"));
        public string ShortTitle
        {
            get
            {
                return GetProperty(ShortTitleProperty);
            }
        }
        private static PropertyInfo<string> SourceNameProperty = RegisterProperty<string>(new PropertyInfo<string>("SourceName", "SourceName"));
        public string SourceName
        {
            get
            {
                return GetProperty(SourceNameProperty);
            }
        }
        //private static PropertyInfo<int> ReviewIdProperty = RegisterProperty<int>(new PropertyInfo<int>("ReviewId", "ReviewId"));
        //public int ReviewId
        //{
        //    get
        //    {
        //        return GetProperty(ReviewIdProperty);
        //    }
        //}
        
        //protected override void AddAuthorizationRules()
        //{
        //    //string[] canRead = new string[] { "AdminUser", "RegularUser", "ReadOnlyUser" };
        //    //AuthorizationRules.AllowGet(typeof(ReadOnlyReview), canRead);
        //    //AuthorizationRules.AllowRead(ReviewNameProperty, canRead);
        //    //AuthorizationRules.AllowRead(ReviewIdProperty, canRead);
        //}

#if !SILVERLIGHT

        public static ItemDuplicatesReadOnly GetItemDuplicateReadOnlyGroup(SafeDataReader reader)
        {
            return DataPortal.FetchChild<ItemDuplicatesReadOnly>(reader);
        }
        //SHORT_TITLE, I.ITEM_ID, SOURCE_NAME, ITEM_DUPLICATE_GROUP_ID GROUP_ID
        private void Child_Fetch(SafeDataReader reader)
        {
            //int id = reader.GetInt32("GROUP_ID");
            //LoadProperty<int>(GroupIdProperty, id);

            Int64 big = reader.GetInt64("ITEM_ID");
            LoadProperty(ItemIdProperty, big);

            string nm = reader.GetString("SHORT_TITLE");
            LoadProperty<string>(ShortTitleProperty, nm);

            nm = reader.GetString("SOURCE_NAME");
            LoadProperty<string>(SourceNameProperty, nm);
            
        }


#endif
    }
}
