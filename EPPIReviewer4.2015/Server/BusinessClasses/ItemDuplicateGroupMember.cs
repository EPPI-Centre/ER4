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
    public class ItemDuplicateGroupMember : BusinessBase<ItemDuplicateGroupMember>
    {

    public ItemDuplicateGroupMember() { }

        

        #region datamembers
        public override string ToString()
        {
            return Title;
        }

        public readonly static PropertyInfo<int> ItemDuplicateIdProperty = RegisterProperty<int>(new PropertyInfo<int>("ItemDuplicateId", "ItemDuplicateId"));
        public int ItemDuplicateId
        {
            get
            {
                return GetProperty(ItemDuplicateIdProperty);
            }
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

        public readonly static PropertyInfo<bool> IsDuplicateProperty = RegisterProperty<bool>(new PropertyInfo<bool>("IsDuplicate", "IsDuplicate", false));
        public bool IsDuplicate
        {
            get
            {
                return GetProperty(IsDuplicateProperty);
            }
            set
            {
                SetProperty(IsDuplicateProperty, value);
            }
        }

        public readonly static PropertyInfo<bool> IsCheckedProperty = RegisterProperty<bool>(new PropertyInfo<bool>("IsChecked", "IsChecked", false));
        public bool IsChecked
        {
            get
            {
                return GetProperty(IsCheckedProperty);
            }
            set
            {
                SetProperty(IsCheckedProperty, value);
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
        public readonly static PropertyInfo<int> GroupIDProperty = RegisterProperty<int>(new PropertyInfo<int>("GroupID", "GroupID"));
        public int GroupID
        {
            get
            {
                return GetProperty(GroupIDProperty);
            }
            set
            {
                SetProperty(GroupIDProperty, value);
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
        public readonly static PropertyInfo<string> PagesProperty = RegisterProperty<string>(new PropertyInfo<string>("Pages", "Pages", string.Empty));
        public string Pages
        {
            get
            {
                return GetProperty(PagesProperty);
            }
            set
            {
                SetProperty(PagesProperty, value);
            }
        }
        public readonly static PropertyInfo<double> SimilarityScoreProperty = RegisterProperty<double>(new PropertyInfo<double>("SimilarityScore", "SimilarityScore"));
        public double SimilarityScore
        {
            get
            {
                return GetProperty(SimilarityScoreProperty);
            }
            set
            {
                SetProperty(SimilarityScoreProperty, value);
            }
        }
        public readonly static PropertyInfo<string> doiProperty = RegisterProperty<string>(new PropertyInfo<string>("doi", "doi", string.Empty));
        public string doi
        {
            get
            {
                return GetProperty(doiProperty);
            }
            set
            {
                SetProperty(doiProperty, value);
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

        protected override void DataPortal_Update()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
#if OLDDEDUP
                using (SqlCommand command = new SqlCommand("st_ItemDuplicateGroupMemberUpdate", connection))
                {//@memberID //@is_checked //@is_duplicate //@is_master
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add("@groupID", System.Data.SqlDbType.Int);
                    command.Parameters["@groupID"].Value = GroupID;

                    command.Parameters.Add("@memberID", System.Data.SqlDbType.Int);
                    command.Parameters["@memberID"].Value = ItemDuplicateId;

                    command.Parameters.Add("@is_checked", System.Data.SqlDbType.Bit);
                    command.Parameters["@is_checked"].Value = IsChecked;

                    command.Parameters.Add("@is_duplicate", System.Data.SqlDbType.Bit);
                    command.Parameters["@is_duplicate"].Value = IsDuplicate;

                    command.Parameters.Add("@is_master", System.Data.SqlDbType.Bit);
                    command.Parameters["@is_master"].Value = IsMaster;
                    command.ExecuteNonQuery();
                }
                connection.Close();
#else
                using (SqlCommand command = new SqlCommand("st_ItemDuplicateGroupMemberUpdateWithScore", connection))
                {//@memberID //@is_checked //@is_duplicate //@is_master


                    //fake exception for testing...
                    //uncomment code below, put a breakpoint and "drag" execution into the exception rising block, to test how the system reacts...

                    //if (1 == DateTime.Now.Ticks)
                    //{
                    //    Exception e = new Exception("this is fake, deliberately triggered");
                    //    throw e;
                    //}

                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add("@groupID", System.Data.SqlDbType.Int);
                    command.Parameters["@groupID"].Value = GroupID;

                    command.Parameters.Add("@memberID", System.Data.SqlDbType.Int);
                    command.Parameters["@memberID"].Value = ItemDuplicateId;

                    command.Parameters.Add("@is_checked", System.Data.SqlDbType.Bit);
                    command.Parameters["@is_checked"].Value = IsChecked;

                    command.Parameters.Add("@is_duplicate", System.Data.SqlDbType.Bit);
                    command.Parameters["@is_duplicate"].Value = IsDuplicate;

                    command.Parameters.Add("@is_master", System.Data.SqlDbType.Bit);
                    command.Parameters["@is_master"].Value = IsMaster;

                    command.Parameters.Add("@score", System.Data.SqlDbType.Float);
                    command.Parameters["@score"].Value = SimilarityScore;
                    command.ExecuteNonQuery();
                }
                connection.Close();
#endif
            }
        }

        protected override void DataPortal_DeleteSelf()
        {
            
        }

        protected void DataPortal_Fetch() // (not) used to return a specific ItemDuplicate
        {
            
        }

        internal static ItemDuplicateGroupMember GetItemDuplicate(SafeDataReader reader, int groupID)
        {
            ItemDuplicateGroupMember returnValue = new ItemDuplicateGroupMember();
            returnValue.LoadProperty<Int64>(ItemIdProperty, reader.GetInt64("ITEM_ID"));
            returnValue.LoadProperty<string>(AuthorsProperty, reader.GetString("AUTHORS"));
            returnValue.LoadProperty<string>(ParentAuthorsProperty, reader.GetString("PARENT_AUTHORS"));
            returnValue.LoadProperty<string>(TitleProperty, reader.GetString("TITLE"));
            returnValue.LoadProperty<string>(ParentTitleProperty, reader.GetString("PARENT_TITLE"));
            returnValue.LoadProperty<string>(ShortTitleProperty, reader.GetString("SHORT_TITLE"));
            returnValue.LoadProperty<string>(TypeNameProperty, reader.GetString("TYPE_NAME"));
            returnValue.LoadProperty<string>(YearProperty, reader.GetString("YEAR"));
            returnValue.LoadProperty<string>(MonthProperty, reader.GetString("MONTH"));
            returnValue.LoadProperty<bool>(IsDuplicateProperty, reader.GetBoolean("IS_DUPLICATE"));
            returnValue.LoadProperty<bool>(IsCheckedProperty, reader.GetBoolean("IS_CHECKED"));
            returnValue.LoadProperty<bool>(IsMasterProperty, reader.GetInt32("IS_MASTER") == 1 ? true : false);
            returnValue.LoadProperty<bool>(IsExportedProperty, reader.GetInt32("IS_EXPORTED") == 1 ? true : false);
            returnValue.LoadProperty<int>(CodedCountProperty, reader.GetInt32("CODED_COUNT"));
            returnValue.LoadProperty<int>(DocCountProperty, reader.GetInt32("DOC_COUNT"));
            returnValue.LoadProperty<string>(SourceProperty, reader.GetString("SOURCE"));
            returnValue.LoadProperty<string>(PagesProperty, reader.GetString("PAGES"));
            returnValue.LoadProperty<string>(doiProperty, reader.GetString("DOI"));
            returnValue.LoadProperty<int>(ItemDuplicateIdProperty, reader.GetInt32("GROUP_MEMBER_ID"));
            returnValue.LoadProperty<double>(SimilarityScoreProperty, reader.GetDouble("SCORE"));
            returnValue.LoadProperty<int>(GroupIDProperty, groupID);
            returnValue.MarkOld();
            return returnValue;
        }

#endif


                }
}

