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
using Csla.DataPortalClient;

#if!SILVERLIGHT
using System.Data.SqlClient;
using BusinessLibrary.Data;
using Csla.Data;
using BusinessLibrary.Security;
using AuthorsHandling;
#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class ItemDuplicateManualGroupMember : ReadOnlyBase<ItemDuplicateManualGroupMember>
    {
#if SILVERLIGHT
    public ItemDuplicateManualGroupMember() { }

        
#else
        private ItemDuplicateManualGroupMember() { }

#endif
        #region datamembers
        public override string ToString()
        {
            return Title;
        }

        private static PropertyInfo<Int64> ItemIdProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("ItemId", "ItemId"));
        public Int64 ItemId
        {
            get
            {
                return GetProperty(ItemIdProperty);
            }
        }


        private static PropertyInfo<string> TitleProperty = RegisterProperty<string>(new PropertyInfo<string>("Title", "Title", string.Empty));
        public string Title
        {
            get
            {
                return GetProperty(TitleProperty);
            }
        }

        private static PropertyInfo<string> ParentTitleProperty = RegisterProperty<string>(new PropertyInfo<string>("ParentTitle", "ParentTitle", string.Empty));
        public string ParentTitle
        {
            get
            {
                return GetProperty(ParentTitleProperty);
            }
        }

        private static PropertyInfo<string> ShortTitleProperty = RegisterProperty<string>(new PropertyInfo<string>("ShortTitle", "ShortTitle", string.Empty));
        public string ShortTitle
        {
            get
            {
                return GetProperty(ShortTitleProperty);
            }
        }

        private static PropertyInfo<string> YearProperty = RegisterProperty<string>(new PropertyInfo<string>("Year", "Year", string.Empty));
        public string Year
        {
            get
            {
                return GetProperty(YearProperty);
            }
        }

        private static PropertyInfo<string> MonthProperty = RegisterProperty<string>(new PropertyInfo<string>("Month", "Month", string.Empty));
        public string Month
        {
            get
            {
                return GetProperty(MonthProperty);
            }
        }

        private static PropertyInfo<string> AuthorsProperty = RegisterProperty<string>(new PropertyInfo<string>("Authors", "Authors", string.Empty));
        public string Authors
        {
            get
            {
                return GetProperty(AuthorsProperty);
            }
        }
        private static PropertyInfo<string> ParentAuthorsProperty = RegisterProperty<string>(new PropertyInfo<string>("ParentAuthors", "ParentAuthors", string.Empty));
        public string ParentAuthors
        {
            get
            {
                return GetProperty(ParentAuthorsProperty);
            }
        }

        private static PropertyInfo<int> GroupIDProperty = RegisterProperty<int>(new PropertyInfo<int>("GroupID", "GroupID"));
        public int GroupID
        {
            get
            {
                return GetProperty(GroupIDProperty);
            }
        }
        private static PropertyInfo<string> SourceProperty = RegisterProperty<string>(new PropertyInfo<string>("Source", "Source"));
        public string Source
        {
            get
            {
                return GetProperty(SourceProperty);
            }
        }

        private static PropertyInfo<string> TypeNameProperty = RegisterProperty<string>(new PropertyInfo<string>("TypeName", "TypeName", string.Empty));
        public string TypeName
        {
            get
            {
                return GetProperty(TypeNameProperty);
            }
        }
        
        #endregion
        //protected override void AddAuthorizationRules()
        //{
        //    //string[] canWrite = new string[] { "AdminUser", "RegularUser" };
        //    //string[] canRead = new string[] { "AdminUser", "RegularUser", "ReadOnlyUser" };
        //    //string[] admin = new string[] { "AdminUser" };
        //    //AuthorizationRules.AllowCreate(typeof(ItemDuplicate), admin);
        //    //AuthorizationRules.AllowDelete(typeof(ItemDuplicate), admin);
        //    //AuthorizationRules.AllowEdit(typeof(ItemDuplicate), canWrite);
        //    //AuthorizationRules.AllowGet(typeof(ItemDuplicate), canRead);
        //}

        

#if !SILVERLIGHT

       

        protected void DataPortal_Fetch() // (not) used to return a specific ItemDuplicate
        {

        }

        internal static ItemDuplicateManualGroupMember GetItemDuplicate(SafeDataReader reader, int groupID)
        {
            ItemDuplicateManualGroupMember returnValue = new ItemDuplicateManualGroupMember();
            returnValue.LoadProperty<Int64>(ItemIdProperty, reader.GetInt64("ITEM_ID"));
            returnValue.LoadProperty<string>(AuthorsProperty, reader.GetString("AUTHORS"));
            returnValue.LoadProperty<string>(ParentAuthorsProperty, reader.GetString("PARENT_AUTHORS"));
            returnValue.LoadProperty<string>(TitleProperty, reader.GetString("TITLE"));
            returnValue.LoadProperty<string>(ParentTitleProperty, reader.GetString("PARENT_TITLE"));
            returnValue.LoadProperty<string>(ShortTitleProperty, reader.GetString("SHORT_TITLE"));
            returnValue.LoadProperty<string>(TypeNameProperty, reader.GetString("TYPE_NAME"));
            returnValue.LoadProperty<string>(YearProperty, reader.GetString("YEAR"));
            returnValue.LoadProperty<string>(MonthProperty, reader.GetString("MONTH"));
            //returnValue.LoadProperty<string>(SourceProperty, reader.GetString("SOURCE"));
            returnValue.LoadProperty<int>(GroupIDProperty, groupID);
            return returnValue;
        }

#endif


    }
}

