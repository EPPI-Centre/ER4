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
    public class ItemDuplicateReadOnlyGroup : ReadOnlyBase<ItemDuplicateReadOnlyGroup>, INotifyPropertyChanged

    {

#if SILVERLIGHT
    public ItemDuplicateReadOnlyGroup() { }
    public event PropertyChangedEventHandler PropertyChanged;

#else
        private ItemDuplicateReadOnlyGroup() { }
        public new event PropertyChangedEventHandler PropertyChanged;
#endif
        
        public void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }  

        public override string ToString()
        {
            return this.ShortTitle;
        }

        private static PropertyInfo<int> GroupIdProperty = RegisterProperty<int>(new PropertyInfo<int>("GroupId", "GroupId"));
        public int GroupId
        {
            get
            {
                return GetProperty(GroupIdProperty);
            }
        }

        private static PropertyInfo<Int64> MasterItemIdProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("MasterItemId", "MasterItemId"));
        public Int64 MasterItemId
        {
            get
            {
                return GetProperty(MasterItemIdProperty);
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
        private static PropertyInfo<int> ReviewIdProperty = RegisterProperty<int>(new PropertyInfo<int>("ReviewId", "ReviewId"));
        public int ReviewId
        {
            get
            {
                return GetProperty(ReviewIdProperty);
            }
        }
        private static PropertyInfo<bool> IsCompleteProperty = RegisterProperty<bool>(new PropertyInfo<bool>("IsComplete", "IsComplete"));
        public bool IsComplete
        {
            get
            {
                return GetProperty(IsCompleteProperty);
            }
            set
            {
                LoadProperty(IsCompleteProperty, value);
                NotifyPropertyChanged("IsComplete");
            }
        }
        //protected override void AddAuthorizationRules()
        //{
        //    //string[] canRead = new string[] { "AdminUser", "RegularUser", "ReadOnlyUser" };
        //    //AuthorizationRules.AllowGet(typeof(ReadOnlyReview), canRead);
        //    //AuthorizationRules.AllowRead(ReviewNameProperty, canRead);
        //    //AuthorizationRules.AllowRead(ReviewIdProperty, canRead);
        //}

#if !SILVERLIGHT

        public static ItemDuplicateReadOnlyGroup GetItemDuplicateReadOnlyGroup(SafeDataReader reader)
        {
            return DataPortal.FetchChild<ItemDuplicateReadOnlyGroup>(reader);
        }
        //GROUP_ID, IR.ITEM_ID MASTER_ITEM_ID, SHORT_TITLE, g.REVIEW_ID
        private void Child_Fetch(SafeDataReader reader)
        {
            int id = reader.GetInt32("GROUP_ID");
            LoadProperty<int>(GroupIdProperty, id);
            id = reader.GetInt32("REVIEW_ID");
            LoadProperty<int>(ReviewIdProperty, id);
            Int64 big = reader.GetInt64("MASTER_ITEM_ID");
            LoadProperty(MasterItemIdProperty, big);
            string nm = reader.GetString("SHORT_TITLE");
            LoadProperty<string>(ShortTitleProperty, nm);
            LoadProperty<bool>(IsCompleteProperty, (reader.GetInt32("IS_COMPLETE") == 1));
        }


#endif
    }
}
