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
    public class ItemDuplicateDirtyGroupMember : BusinessBase<ItemDuplicateDirtyGroupMember>
    {
        public ItemDuplicateDirtyGroupMember() { }


        #region datamembers
        public override string ToString()
        {
            return Title;
        }

        
        public readonly static PropertyInfo<Int64> ItemIdProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("ItemId", "ItemId"));
        public Int64 ItemId
        {
            get
            {
                return GetProperty(ItemIdProperty);
            }
        }


        public readonly static PropertyInfo<string> TitleProperty = RegisterProperty<string>(new PropertyInfo<string>("Title", "Title", string.Empty));
        public string Title
        {
            get
            {
                return GetProperty(TitleProperty);
            }
            set
            {
                SetProperty(TitleProperty, value);
            }
        }

        public readonly static PropertyInfo<string> ParentTitleProperty = RegisterProperty<string>(new PropertyInfo<string>("ParentTitle", "ParentTitle", string.Empty));
        public string ParentTitle
        {
            get
            {
                return GetProperty(ParentTitleProperty);
            }
            set
            {
                SetProperty(ParentTitleProperty, value);
            }
        }

        public readonly static PropertyInfo<string> ShortTitleProperty = RegisterProperty<string>(new PropertyInfo<string>("ShortTitle", "ShortTitle", string.Empty));
        public string ShortTitle
        {
            get
            {
                return GetProperty(ShortTitleProperty);
            }
            set
            {
                SetProperty(ShortTitleProperty, value);
            }
        }

        public readonly static PropertyInfo<string> YearProperty = RegisterProperty<string>(new PropertyInfo<string>("Year", "Year", string.Empty));
        public string Year
        {
            get
            {
                return GetProperty(YearProperty);
            }
            set
            {
                SetProperty(YearProperty, value);
            }
        }

        public readonly static PropertyInfo<string> MonthProperty = RegisterProperty<string>(new PropertyInfo<string>("Month", "Month", string.Empty));
        public string Month
        {
            get
            {
                return GetProperty(MonthProperty);
            }
            set
            {
                SetProperty(MonthProperty, value);
            }
        }
        
        public readonly static PropertyInfo<string> AuthorsProperty = RegisterProperty<string>(new PropertyInfo<string>("Authors", "Authors", string.Empty));
        public string Authors
        {
            get
            {
                return GetProperty(AuthorsProperty);
            }
            set
            {
                SetProperty(AuthorsProperty, value);
            }
        }
        public readonly static PropertyInfo<string> ParentAuthorsProperty = RegisterProperty<string>(new PropertyInfo<string>("ParentAuthors", "ParentAuthors", string.Empty));
        public string ParentAuthors
        {
            get
            {
                return GetProperty(ParentAuthorsProperty);
            }
            set
            {
                SetProperty(ParentAuthorsProperty, value);
            }
        }
        public readonly static PropertyInfo<bool> IsMasterProperty = RegisterProperty<bool>(new PropertyInfo<bool>("IsMaster", "IsMaster", false));
        public bool IsMaster
        {
            get
            {
                return GetProperty(IsMasterProperty);
            }
            set
            {
                SetProperty(IsMasterProperty, value);
            }
        }
        public readonly static PropertyInfo<bool> IsExportedProperty = RegisterProperty<bool>(new PropertyInfo<bool>("IsExported", "IsExported", false));
        public bool IsExported
        {
            get
            {
                return GetProperty(IsExportedProperty);
            }
            set
            {
                SetProperty(IsExportedProperty, value);
            }
        }
        public bool IsEditable
        {
            get
            {
                return !GetProperty(IsExportedProperty);
            }
        }
        public readonly static PropertyInfo<int> CodedCountProperty = RegisterProperty<int>(new PropertyInfo<int>("CodedCount", "CodedCount"));
        public int CodedCount
        {
            get
            {
                return GetProperty(CodedCountProperty);
            }
            set
            {
                SetProperty(CodedCountProperty, value);
            }
        }
        public readonly static PropertyInfo<int> DocCountProperty = RegisterProperty<int>(new PropertyInfo<int>("DocCount", "DocCount"));
        public int DocCount
        {
            get
            {
                return GetProperty(DocCountProperty);
            }
            set
            {
                SetProperty(DocCountProperty, value);
            }
        }
       
        public readonly static PropertyInfo<string> SourceProperty = RegisterProperty<string>(new PropertyInfo<string>("Source", "Source"));
        public string Source
        {
            get
            {
                return GetProperty(SourceProperty);
            }
            set
            {
                SetProperty(SourceProperty, value);
            }
        }

        public readonly static PropertyInfo<string> TypeNameProperty = RegisterProperty<string>(new PropertyInfo<string>("TypeName", "TypeName", string.Empty));
        public string TypeName
        {
            get
            {
                return GetProperty(TypeNameProperty);
            }
            set
            {
                SetProperty(TypeNameProperty, value);
            }
        }
        
        public readonly static PropertyInfo<int> RelatedGroupsCountProperty = RegisterProperty<int>(new PropertyInfo<int>("RelatedGroupsCount", "RelatedGroupsCount"));
        public int RelatedGroupsCount
        {
            get
            {
                return GetProperty(RelatedGroupsCountProperty);
            }
            set
            {
                SetProperty(RelatedGroupsCountProperty, value);
            }
        }
        
        public readonly static PropertyInfo<bool> IsAvailableProperty = RegisterProperty<bool>(new PropertyInfo<bool>("IsAvailable", "IsAvailable"));
        public bool IsAvailable
        {
            get
            {
                return GetProperty(IsAvailableProperty);
            }
            set
            {
                SetProperty(IsAvailableProperty, value);
            }
        }/// <summary>
        /// used as a bitmask to make the right comments appear in the UI.
        /// </summary>
        /// <remarks>
        /// 0 = perfect: item does not belong to any group;
        /// 1 = item is on some other group but is not a master, consider using this group instead;
        /// 2 = stopper: item is master of another group;
        /// -1 = should not happen: if item is master of another group, shurely should be also exported.
        /// </remarks>
        //
        public int PropertiesConverter
        {
            get
            {
                if (IsAvailable & IsExported)
                {
                    return 1; //item is on some other group but is not a master
                }
                else if (IsAvailable & !IsExported)
                {
                    return 0; //perfect: item does not belong to any group
                }
                else if (!IsAvailable & IsExported)
                {
                    return 2; //stopper: item is master of another group
                }
                else return -1; //should not happen: if item is master of another group, shurely should be also exported.
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

        protected override void AddBusinessRules()
        {
            //ValidationRules.AddRule(Csla.Validation.CommonRules.MaxValue<decimal>, new Csla.Validation.CommonRules.MaxValueRuleArgs<decimal>(ReviewCodeSetFte1Property, 1));
            //ValidationRules.AddRule(Csla.Validation.CommonRules.StringRequired, new Csla.Validation.RuleArgs(ReviewCodeSetNameProperty));
            //ValidationRules.AddRule(Csla.Validation.CommonRules.StringMaxLength, new Csla.Validation.CommonRules.MaxLengthRuleArgs(ReviewCodeSetNameProperty, 20));
        }

#if !SILVERLIGHT

        protected override void DataPortal_Insert()
        {
            
        }

        

        protected void DataPortal_Fetch() // (not) used to return a specific ItemDuplicate
        {
            
        }

        internal static ItemDuplicateDirtyGroupMember GetItemDuplicate(SafeDataReader reader)
        {
            ItemDuplicateDirtyGroupMember returnValue = new ItemDuplicateDirtyGroupMember();
            returnValue.LoadProperty<Int64>(ItemIdProperty, reader.GetInt64("ITEM_ID"));
            returnValue.LoadProperty<string>(AuthorsProperty, reader.GetString("AUTHORS"));
            returnValue.LoadProperty<string>(ParentAuthorsProperty, reader.GetString("PARENT_AUTHORS"));
            returnValue.LoadProperty<string>(TitleProperty, reader.GetString("TITLE"));
            returnValue.LoadProperty<string>(ParentTitleProperty, reader.GetString("PARENT_TITLE"));
            returnValue.LoadProperty<string>(ShortTitleProperty, reader.GetString("SHORT_TITLE"));
            returnValue.LoadProperty<string>(TypeNameProperty, reader.GetString("TYPE_NAME"));
            returnValue.LoadProperty<string>(YearProperty, reader.GetString("YEAR"));
            returnValue.LoadProperty<string>(MonthProperty, reader.GetString("MONTH"));
            returnValue.LoadProperty<bool>(IsMasterProperty, false);
            returnValue.LoadProperty<bool>(IsExportedProperty, reader.GetInt32("IS_EXPORTED") == 1 ? true : false);
            returnValue.LoadProperty<int>(CodedCountProperty, reader.GetInt32("CODED_COUNT"));
            returnValue.LoadProperty<int>(DocCountProperty, reader.GetInt32("DOC_COUNT"));
            returnValue.LoadProperty<string>(SourceProperty, reader.GetString("SOURCE"));
            returnValue.LoadProperty<int>(RelatedGroupsCountProperty, reader.GetInt32("RELATED_COUNT"));
            returnValue.LoadProperty<bool>(IsAvailableProperty, reader.GetInt32("IS_AVAILABLE") == 1 ? true : false);
            returnValue.MarkOld();
            return returnValue;
        }

#endif


    }
}

