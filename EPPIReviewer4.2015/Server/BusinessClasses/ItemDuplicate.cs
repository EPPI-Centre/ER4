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
    public class ItemDuplicate : BusinessBase<ItemDuplicate>
    {
#if SILVERLIGHT
    public ItemDuplicate() { }

        
#else
        private ItemDuplicate() { }
        public static ItemDuplicate MakeWaitingResult()
        {//returns a special ItemDuplicate: an ItemDuplicateList with only this item means that the SISS duplicate checking is still running and that the user should try again later
            ItemDuplicate res = new ItemDuplicate();
            res.LoadProperty<Int64>(ItemId1Property, -1);
            return res;
        }
#endif

        public override string ToString()
        {
            return Title1;
        }

        private static PropertyInfo<Int64> ItemDuplicateId1Property = RegisterProperty<Int64>(new PropertyInfo<Int64>("ItemDuplicateId1", "ItemDuplicateId1"));
        public Int64 ItemDuplicateId1
        {
            get
            {
                return GetProperty(ItemDuplicateId1Property);
            }
        }

        private static PropertyInfo<Int64> ItemId1Property = RegisterProperty<Int64>(new PropertyInfo<Int64>("ItemId1", "ItemId1"));
        public Int64 ItemId1
        {
            get
            {
                return GetProperty(ItemId1Property);
            }
        }

        private static PropertyInfo<int> TypeId1Property = RegisterProperty<int>(new PropertyInfo<int>("TypeId1", "TypeId1"));
        public int TypeId1
        {
            get
            {
                return GetProperty(TypeId1Property);
            }
            set
            {
                SetProperty(TypeId1Property, value);
            }
        }

        private static PropertyInfo<string> Title1Property = RegisterProperty<string>(new PropertyInfo<string>("Title1", "Title1", string.Empty));
        public string Title1
        {
            get
            {
                return GetProperty(Title1Property);
            }
            set
            {
                SetProperty(Title1Property, value);
            }
        }

        private static PropertyInfo<string> ParentTitle1Property = RegisterProperty<string>(new PropertyInfo<string>("ParentTitle1", "ParentTitle1", string.Empty));
        public string ParentTitle1
        {
            get
            {
                return GetProperty(ParentTitle1Property);
            }
            set
            {
                SetProperty(ParentTitle1Property, value);
            }
        }

        private static PropertyInfo<string> ShortTitle1Property = RegisterProperty<string>(new PropertyInfo<string>("ShortTitle1", "ShortTitle1", string.Empty));
        public string ShortTitle1
        {
            get
            {
                return GetProperty(ShortTitle1Property);
            }
            set
            {
                SetProperty(ShortTitle1Property, value);
            }
        }

        private static PropertyInfo<SmartDate> DateCreated1Property = RegisterProperty<SmartDate>(new PropertyInfo<SmartDate>("DateCreated1", "DateCreated1"));
        public SmartDate DateCreated1
        {
            get
            {
                return GetProperty(DateCreated1Property);
            }
            set
            {
                SetProperty(DateCreated1Property, value);
            }
        }

        private static PropertyInfo<string> CreatedBy1Property = RegisterProperty<string>(new PropertyInfo<string>("CreatedBy1", "CreatedBy1", string.Empty));
        public string CreatedBy1
        {
            get
            {
                return GetProperty(CreatedBy1Property);
            }
            set
            {
                SetProperty(CreatedBy1Property, value);
            }
        }

        private static PropertyInfo<SmartDate> DateEdited1Property = RegisterProperty<SmartDate>(new PropertyInfo<SmartDate>("DateEdited1", "DateEdited1"));
        public SmartDate DateEdited1
        {
            get
            {
                return GetProperty(DateEdited1Property);
            }
            set
            {
                SetProperty(DateEdited1Property, value);
            }
        }

        private static PropertyInfo<string> EditedBy1Property = RegisterProperty<string>(new PropertyInfo<string>("EditedBy1", "EditedBy1", string.Empty));
        public string EditedBy1
        {
            get
            {
                return GetProperty(EditedBy1Property);
            }
            set
            {
                SetProperty(EditedBy1Property, value);
            }
        }

        private static PropertyInfo<string> Year1Property = RegisterProperty<string>(new PropertyInfo<string>("Year1", "Year1", string.Empty));
        public string Year1
        {
            get
            {
                return GetProperty(Year1Property);
            }
            set
            {
                SetProperty(Year1Property, value);
            }
        }

        private static PropertyInfo<string> Month1Property = RegisterProperty<string>(new PropertyInfo<string>("Month1", "Month1", string.Empty));
        public string Month1
        {
            get
            {
                return GetProperty(Month1Property);
            }
            set
            {
                SetProperty(Month1Property, value);
            }
        }

        private static PropertyInfo<string> StandardNumber1Property = RegisterProperty<string>(new PropertyInfo<string>("StandardNumber1", "StandardNumber1", string.Empty));
        public string StandardNumber1
        {
            get
            {
                return GetProperty(StandardNumber1Property);
            }
            set
            {
                SetProperty(StandardNumber1Property, value);
            }
        }

        private static PropertyInfo<string> City1Property = RegisterProperty<string>(new PropertyInfo<string>("City1", "City1", string.Empty));
        public string City
        {
            get
            {
                return GetProperty(City1Property);
            }
            set
            {
                SetProperty(City1Property, value);
            }
        }

        private static PropertyInfo<string> Country1Property = RegisterProperty<string>(new PropertyInfo<string>("Country1", "Country1", string.Empty));
        public string Country1
        {
            get
            {
                return GetProperty(Country1Property);
            }
            set
            {
                SetProperty(Country1Property, value);
            }
        }

        private static PropertyInfo<string> Publisher1Property = RegisterProperty<string>(new PropertyInfo<string>("Publisher1", "Publisher1", string.Empty));
        public string Publisher1
        {
            get
            {
                return GetProperty(Publisher1Property);
            }
            set
            {
                SetProperty(Publisher1Property, value);
            }
        }

        private static PropertyInfo<string> Institution1Property = RegisterProperty<string>(new PropertyInfo<string>("Institution1", "Institution1", string.Empty));
        public string Institution1
        {
            get
            {
                return GetProperty(Institution1Property);
            }
            set
            {
                SetProperty(Institution1Property, value);
            }
        }

        private static PropertyInfo<string> Volume1Property = RegisterProperty<string>(new PropertyInfo<string>("Volume1", "Volume1", string.Empty));
        public string Volume1
        {
            get
            {
                return GetProperty(Volume1Property);
            }
            set
            {
                SetProperty(Volume1Property, value);
            }
        }

        private static PropertyInfo<string> Pages1Property = RegisterProperty<string>(new PropertyInfo<string>("Pages1", "Pages1", string.Empty));
        public string Pages1
        {
            get
            {
                return GetProperty(Pages1Property);
            }
            set
            {
                SetProperty(Pages1Property, value);
            }
        }

        private static PropertyInfo<string> Edition1Property = RegisterProperty<string>(new PropertyInfo<string>("Edition1", "Edition1", string.Empty));
        public string Edition1
        {
            get
            {
                return GetProperty(Edition1Property);
            }
            set
            {
                SetProperty(Edition1Property, value);
            }
        }

        private static PropertyInfo<string> Issue1Property = RegisterProperty<string>(new PropertyInfo<string>("Issue1", "Issue1", string.Empty));
        public string Issue1
        {
            get
            {
                return GetProperty(Issue1Property);
            }
            set
            {
                SetProperty(Issue1Property, value);
            }
        }

        private static PropertyInfo<string> URL1Property = RegisterProperty<string>(new PropertyInfo<string>("URL1", "URL1", string.Empty));
        public string URL1
        {
            get
            {
                return GetProperty(URL1Property);
            }
            set
            {
                SetProperty(URL1Property, value);
            }
        }

        private static PropertyInfo<string> OldItemId1Property = RegisterProperty<string>(new PropertyInfo<string>("OldItemId1", "OldItemId1", string.Empty));
        public string OldItemId1
        {
            get
            {
                return GetProperty(OldItemId1Property);
            }
        }

        private static PropertyInfo<string> Abstract1Property = RegisterProperty<string>(new PropertyInfo<string>("Abstract1", "Abstract1", string.Empty));
        public string Abstract1
        {
            get
            {
                return GetProperty(Abstract1Property);
            }
            set
            {
                SetProperty(Abstract1Property, value);
            }
        }

        private static PropertyInfo<string> Comments1Property = RegisterProperty<string>(new PropertyInfo<string>("Comments1", "Comments1", string.Empty));
        public string Comments1
        {
            get
            {
                return GetProperty(Comments1Property);
            }
            set
            {
                SetProperty(Comments1Property, value);
            }
        }

        private static PropertyInfo<string> TypeName1Property = RegisterProperty<string>(new PropertyInfo<string>("TypeName1", "TypeName1", string.Empty));
        public string TypeName1
        {
            get
            {
                return GetProperty(TypeName1Property);
            }
            set
            {
                SetProperty(TypeName1Property, value);
            }
        }

        private static PropertyInfo<string> Authors1Property = RegisterProperty<string>(new PropertyInfo<string>("Authors1", "Authors1", string.Empty));
        public string Authors1
        {
            get
            {
                return GetProperty(Authors1Property);
            }
            set
            {
                SetProperty(Authors1Property, value);
            }
        }
        private static PropertyInfo<string> ParentAuthors1Property = RegisterProperty<string>(new PropertyInfo<string>("ParentAuthors1", "ParentAuthors1", string.Empty));
        public string ParentAuthors1
        {
            get
            {
                return GetProperty(ParentAuthors1Property);
            }
            set
            {
                SetProperty(ParentAuthors1Property, value);
            }
        }

        private static PropertyInfo<bool> IsDuplicate1Property = RegisterProperty<bool>(new PropertyInfo<bool>("IsDuplicate1", "IsDuplicate1", false));
        public bool IsDuplicate1
        {
            get
            {
                return GetProperty(IsDuplicate1Property);
            }
            set
            {
                SetProperty(IsDuplicate1Property, value);
            }
        }

        private static PropertyInfo<bool> IsChecked1Property = RegisterProperty<bool>(new PropertyInfo<bool>("IsChecked1", "IsChecked1", false));
        public bool IsChecked1
        {
            get
            {
                return GetProperty(IsChecked1Property);
            }
            set
            {
                SetProperty(IsChecked1Property, value);
            }
        }

        private static PropertyInfo<int> CodedCount1Property = RegisterProperty<int>(new PropertyInfo<int>("CodedCount1", "CodedCount1"));
        public int CodedCount1
        {
            get
            {
                return GetProperty(CodedCount1Property);
            }
            set
            {
                SetProperty(CodedCount1Property, value);
            }
        }
        private static PropertyInfo<int> DocCount1Property = RegisterProperty<int>(new PropertyInfo<int>("DocCount1", "DocCount1"));
        public int DocCount1
        {
            get
            {
                return GetProperty(DocCount1Property);
            }
            set
            {
                SetProperty(DocCount1Property, value);
            }
        }
        private static PropertyInfo<string> Source1Property = RegisterProperty<string>(new PropertyInfo<string>("Source1", "Source1"));
        public string Source1
        {
            get
            {
                return GetProperty(Source1Property);
            }
            set
            {
                SetProperty(Source1Property, value);
            }
        }
        private static PropertyInfo<Int64> ItemDuplicateId2Property = RegisterProperty<Int64>(new PropertyInfo<Int64>("ItemDuplicateId2", "ItemDuplicateId2"));
        public Int64 ItemDuplicateId2
        {
            get
            {
                return GetProperty(ItemDuplicateId2Property);
            }
        }

        private static PropertyInfo<Int64> ItemId2Property = RegisterProperty<Int64>(new PropertyInfo<Int64>("ItemId2", "ItemId2"));
        public Int64 ItemId2
        {
            get
            {
                return GetProperty(ItemId2Property);
            }
        }

        private static PropertyInfo<int> TypeId2Property = RegisterProperty<int>(new PropertyInfo<int>("TypeId2", "TypeId2"));
        public int TypeId2
        {
            get
            {
                return GetProperty(TypeId2Property);
            }
            set
            {
                SetProperty(TypeId2Property, value);
            }
        }

        private static PropertyInfo<string> Title2Property = RegisterProperty<string>(new PropertyInfo<string>("Title2", "Title2", string.Empty));
        public string Title2
        {
            get
            {
                return GetProperty(Title2Property);
            }
            set
            {
                SetProperty(Title2Property, value);
            }
        }

        private static PropertyInfo<string> ParentTitle2Property = RegisterProperty<string>(new PropertyInfo<string>("ParentTitle2", "ParentTitle2", string.Empty));
        public string ParentTitle2
        {
            get
            {
                return GetProperty(ParentTitle2Property);
            }
            set
            {
                SetProperty(ParentTitle2Property, value);
            }
        }

        private static PropertyInfo<string> ShortTitle2Property = RegisterProperty<string>(new PropertyInfo<string>("ShortTitle2", "ShortTitle2", string.Empty));
        public string ShortTitle2
        {
            get
            {
                return GetProperty(ShortTitle2Property);
            }
            set
            {
                SetProperty(ShortTitle2Property, value);
            }
        }

        private static PropertyInfo<SmartDate> DateCreated2Property = RegisterProperty<SmartDate>(new PropertyInfo<SmartDate>("DateCreated2", "DateCreated2"));
        public SmartDate DateCreated2
        {
            get
            {
                return GetProperty(DateCreated2Property);
            }
            set
            {
                SetProperty(DateCreated2Property, value);
            }
        }

        private static PropertyInfo<string> CreatedBy2Property = RegisterProperty<string>(new PropertyInfo<string>("CreatedBy2", "CreatedBy2", string.Empty));
        public string CreatedBy2
        {
            get
            {
                return GetProperty(CreatedBy2Property);
            }
            set
            {
                SetProperty(CreatedBy2Property, value);
            }
        }

        private static PropertyInfo<SmartDate> DateEdited2Property = RegisterProperty<SmartDate>(new PropertyInfo<SmartDate>("DateEdited2", "DateEdited2"));
        public SmartDate DateEdited2
        {
            get
            {
                return GetProperty(DateEdited2Property);
            }
            set
            {
                SetProperty(DateEdited2Property, value);
            }
        }

        private static PropertyInfo<string> EditedBy2Property = RegisterProperty<string>(new PropertyInfo<string>("EditedBy2", "EditedBy2", string.Empty));
        public string EditedBy2
        {
            get
            {
                return GetProperty(EditedBy2Property);
            }
            set
            {
                SetProperty(EditedBy2Property, value);
            }
        }

        private static PropertyInfo<string> Year2Property = RegisterProperty<string>(new PropertyInfo<string>("Year2", "Year2", string.Empty));
        public string Year2
        {
            get
            {
                return GetProperty(Year2Property);
            }
            set
            {
                SetProperty(Year2Property, value);
            }
        }

        private static PropertyInfo<string> Month2Property = RegisterProperty<string>(new PropertyInfo<string>("Month2", "Month2", string.Empty));
        public string Month2
        {
            get
            {
                return GetProperty(Month2Property);
            }
            set
            {
                SetProperty(Month2Property, value);
            }
        }

        private static PropertyInfo<string> StandardNumber2Property = RegisterProperty<string>(new PropertyInfo<string>("StandardNumber2", "StandardNumber2", string.Empty));
        public string StandardNumber2
        {
            get
            {
                return GetProperty(StandardNumber2Property);
            }
            set
            {
                SetProperty(StandardNumber2Property, value);
            }
        }

        private static PropertyInfo<string> City2Property = RegisterProperty<string>(new PropertyInfo<string>("City2", "City2", string.Empty));
        public string City2
        {
            get
            {
                return GetProperty(City2Property);
            }
            set
            {
                SetProperty(City2Property, value);
            }
        }

        private static PropertyInfo<string> Country2Property = RegisterProperty<string>(new PropertyInfo<string>("Country2", "Country2", string.Empty));
        public string Country2
        {
            get
            {
                return GetProperty(Country2Property);
            }
            set
            {
                SetProperty(Country2Property, value);
            }
        }

        private static PropertyInfo<string> Publisher2Property = RegisterProperty<string>(new PropertyInfo<string>("Publisher2", "Publisher2", string.Empty));
        public string Publisher2
        {
            get
            {
                return GetProperty(Publisher2Property);
            }
            set
            {
                SetProperty(Publisher2Property, value);
            }
        }

        private static PropertyInfo<string> Institution2Property = RegisterProperty<string>(new PropertyInfo<string>("Institution2", "Institution2", string.Empty));
        public string Institution2
        {
            get
            {
                return GetProperty(Institution2Property);
            }
            set
            {
                SetProperty(Institution2Property, value);
            }
        }

        private static PropertyInfo<string> Volume2Property = RegisterProperty<string>(new PropertyInfo<string>("Volume2", "Volume2", string.Empty));
        public string Volume2
        {
            get
            {
                return GetProperty(Volume2Property);
            }
            set
            {
                SetProperty(Volume2Property, value);
            }
        }

        private static PropertyInfo<string> Pages2Property = RegisterProperty<string>(new PropertyInfo<string>("Pages2", "Pages2", string.Empty));
        public string Pages2
        {
            get
            {
                return GetProperty(Pages2Property);
            }
            set
            {
                SetProperty(Pages2Property, value);
            }
        }

        private static PropertyInfo<string> Edition2Property = RegisterProperty<string>(new PropertyInfo<string>("Edition2", "Edition2", string.Empty));
        public string Edition2
        {
            get
            {
                return GetProperty(Edition2Property);
            }
            set
            {
                SetProperty(Edition2Property, value);
            }
        }

        private static PropertyInfo<string> Issue2Property = RegisterProperty<string>(new PropertyInfo<string>("Issue2", "Issue2", string.Empty));
        public string Issue2
        {
            get
            {
                return GetProperty(Issue2Property);
            }
            set
            {
                SetProperty(Issue2Property, value);
            }
        }

        private static PropertyInfo<string> URL2Property = RegisterProperty<string>(new PropertyInfo<string>("URL2", "URL2", string.Empty));
        public string URL2
        {
            get
            {
                return GetProperty(URL2Property);
            }
            set
            {
                SetProperty(URL2Property, value);
            }
        }

        private static PropertyInfo<string> OldItemId2Property = RegisterProperty<string>(new PropertyInfo<string>("OldItemId2", "OldItemId2", string.Empty));
        public string OldItemId2
        {
            get
            {
                return GetProperty(OldItemId2Property);
            }
        }

        private static PropertyInfo<string> Abstract2Property = RegisterProperty<string>(new PropertyInfo<string>("Abstract2", "Abstract2", string.Empty));
        public string Abstract2
        {
            get
            {
                return GetProperty(Abstract2Property);
            }
            set
            {
                SetProperty(Abstract2Property, value);
            }
        }

        private static PropertyInfo<string> Comments2Property = RegisterProperty<string>(new PropertyInfo<string>("Comments2", "Comments2", string.Empty));
        public string Comments2
        {
            get
            {
                return GetProperty(Comments2Property);
            }
            set
            {
                SetProperty(Comments2Property, value);
            }
        }

        private static PropertyInfo<string> TypeName2Property = RegisterProperty<string>(new PropertyInfo<string>("TypeName2", "TypeName2", string.Empty));
        public string TypeName2
        {
            get
            {
                return GetProperty(TypeName2Property);
            }
            set
            {
                SetProperty(TypeName2Property, value);
            }
        }

        private static PropertyInfo<string> Authors2Property = RegisterProperty<string>(new PropertyInfo<string>("Authors2", "Authors2", string.Empty));
        public string Authors2
        {
            get
            {
                return GetProperty(Authors2Property);
            }
            set
            {
                SetProperty(Authors2Property, value);
            }
        }
        private static PropertyInfo<string> ParentAuthors2Property = RegisterProperty<string>(new PropertyInfo<string>("ParentAuthors2", "ParentAuthors2", string.Empty));
        public string ParentAuthors2
        {
            get
            {
                return GetProperty(ParentAuthors2Property);
            }
            set
            {
                SetProperty(ParentAuthors2Property, value);
            }
        }

        private static PropertyInfo<bool> IsDuplicate2Property = RegisterProperty<bool>(new PropertyInfo<bool>("IsDuplicate2", "IsDuplicate2", false));
        public bool IsDuplicate2
        {
            get
            {
                return GetProperty(IsDuplicate2Property);
            }
            set
            {
                SetProperty(IsDuplicate2Property, value);
            }
        }

        private static PropertyInfo<bool> IsChecked2Property = RegisterProperty<bool>(new PropertyInfo<bool>("IsChecked2", "IsChecked2", false));
        public bool IsChecked2
        {
            get
            {
                return GetProperty(IsChecked2Property);
            }
            set
            {
                SetProperty(IsChecked2Property, value);
            }
        }

        private static PropertyInfo<int> CodedCount2Property = RegisterProperty<int>(new PropertyInfo<int>("CodedCount2", "CodedCount2"));
        public int CodedCount2
        {
            get
            {
                return GetProperty(CodedCount2Property);
            }
            set
            {
                SetProperty(CodedCount2Property, value);
            }
        }
        private static PropertyInfo<int> DocCount2Property = RegisterProperty<int>(new PropertyInfo<int>("DocCount2", "DocCount2"));
        public int DocCount2
        {
            get
            {
                return GetProperty(DocCount2Property);
            }
            set
            {
                SetProperty(DocCount2Property, value);
            }
        }
        private static PropertyInfo<string> Source2Property = RegisterProperty<string>(new PropertyInfo<string>("Source2", "Source2"));
        public string Source2
        {
            get
            {
                return GetProperty(Source2Property);
            }
            set
            {
                SetProperty(Source2Property, value);
            }
        }

        private static PropertyInfo<double> SimilarityScoreProperty = RegisterProperty<double>(new PropertyInfo<double>("SimilarityScore", "SimilarityScore"));
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
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ItemDuplicatesInsert", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    /*
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ReadProperty(ReviewIdProperty)));
                    command.Parameters.Add(new SqlParameter("@SET_TYPE_ID", 3)); // All set to review specific keywords atm
                    command.Parameters.Add(new SqlParameter("@ALLOW_CODING_EDITS", ReadProperty(AllowCodingEditsProperty)));
                    command.Parameters.Add(new SqlParameter("@SET_NAME", ReadProperty(SetNameProperty)));
                    command.Parameters.Add(new SqlParameter("@CODING_IS_FINAL", ReadProperty(CodingIsFinalProperty)));

                    command.Parameters.Add(new SqlParameter("@NEW_REVIEW_SET_ID", 0));
                    command.Parameters["@NEW_REVIEW_SET_ID"].Direction = System.Data.ParameterDirection.Output;
                    command.Parameters.Add(new SqlParameter("@NEW_SET_ID", 0));
                    command.Parameters["@NEW_SET_ID"].Direction = System.Data.ParameterDirection.Output;
                    command.ExecuteNonQuery();
                    LoadProperty(ItemDuplicateIdProperty, command.Parameters["@NEW_REVIEW_SET_ID"].Value);
                    LoadProperty(SetIdProperty, command.Parameters["@NEW_SET_ID"].Value);
                    */
                }
                connection.Close();
            }
        }

        protected override void DataPortal_Update()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                using (SqlCommand command = new SqlCommand("st_ItemDuplicatesUpdate", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@ITEM_ID1", ReadProperty(ItemId1Property)));
                    command.Parameters.Add(new SqlParameter("@ITEM_ID2", ReadProperty(ItemId2Property)));
                    command.Parameters.Add(new SqlParameter("@IS_CHECKED", true));
                    command.Parameters.Add(new SqlParameter("@IS_DUPLICATE1", ReadProperty(IsDuplicate1Property)));
                    command.Parameters.Add(new SqlParameter("@IS_DUPLICATE2", ReadProperty(IsDuplicate2Property)));
                    command.Parameters.Add(new SqlParameter("@ITEM_DUPLICATES_ID1", ReadProperty(ItemDuplicateId1Property)));
                    command.Parameters.Add(new SqlParameter("@ITEM_DUPLICATES_ID2", ReadProperty(ItemDuplicateId2Property)));
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        protected override void DataPortal_DeleteSelf()
        {
            /*
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ItemDuplicateDelete", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_SET_ID", ReadProperty(ItemDuplicateIdProperty)));
                    command.Parameters.Add(new SqlParameter("@SET_ID", ReadProperty(SetIdProperty)));
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
             */
        }

        protected void DataPortal_Fetch(SingleCriteria<ItemDuplicate, Int64> criteria) // used to return a specific ItemDuplicate
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ItemDuplicate", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@ItemDuplicate_ID", criteria.Value));
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        if (reader.Read())
                        {
                            /*
                            LoadProperty<Int64>(ItemDuplicateIdProperty, reader.GetInt64("ItemDuplicate_ID"));
                            LoadProperty<Int64>(ItemDuplicateIdProperty, reader.GetInt32("TYPE_ID"));

                            LoadProperty<string>(TitleProperty, reader.GetString("TITLE"));

                            LoadProperty<string>(OldItemDuplicateIdProperty, reader.GetString("OLD_ItemDuplicate_ID"));
                            LoadProperty<string>(AbstractProperty, reader.GetString("ABSTRACT"));
                            */
                        }
                    }
                }
                connection.Close();
            }
        }

        internal static ItemDuplicate GetItemDuplicate(SafeDataReader reader)
        {
            ItemDuplicate returnValue = new ItemDuplicate();
            returnValue.LoadProperty<Int64>(ItemId1Property, reader.GetInt64("ITEM_ID1"));
            returnValue.LoadProperty<int>(TypeId1Property, reader.GetInt32("TYPE_ID1"));
            returnValue.LoadProperty<string>(Authors1Property, reader.GetString("AUTHORS1"));
            returnValue.LoadProperty<string>(ParentAuthors1Property, reader.GetString("PARENT_AUTHORS1"));
            returnValue.LoadProperty<string>(Title1Property, reader.GetString("TITLE1"));
            returnValue.LoadProperty<string>(ParentTitle1Property, reader.GetString("PARENT_TITLE1"));
            //returnValue.LoadProperty<string>(ShortTitle1Property, reader.GetString("SHORT_TITLE1"));
            //returnValue.LoadProperty<SmartDate>(DateCreated1Property, reader.GetSmartDate("DATE_CREATED1"));
            //returnValue.LoadProperty<string>(CreatedBy1Property, reader.GetString("CREATED_BY1"));
            //returnValue.LoadProperty<SmartDate>(DateEdited1Property, reader.GetSmartDate("DATE_EDITED1"));
            //returnValue.LoadProperty<string>(EditedBy1Property, reader.GetString("EDITED_BY1"));
            //returnValue.LoadProperty<string>(Year1Property, reader.GetString("YEAR1"));
            //returnValue.LoadProperty<string>(Month1Property, reader.GetString("MONTH1"));
            //returnValue.LoadProperty<string>(StandardNumber1Property, reader.GetString("STANDARD_NUMBER1"));
            //returnValue.LoadProperty<string>(City1Property, reader.GetString("CITY1"));
            //returnValue.LoadProperty<string>(Country1Property, reader.GetString("COUNTRY1"));
            //returnValue.LoadProperty<string>(Publisher1Property, reader.GetString("PUBLISHER1"));
            //returnValue.LoadProperty<string>(Institution1Property, reader.GetString("INSTITUTION1"));
            //returnValue.LoadProperty<string>(Volume1Property, reader.GetString("VOLUME1"));
            //returnValue.LoadProperty<string>(Pages1Property, reader.GetString("PAGES1"));
            //returnValue.LoadProperty<string>(Edition1Property, reader.GetString("EDITION1"));
            //returnValue.LoadProperty<string>(Issue1Property, reader.GetString("ISSUE1"));
            //returnValue.LoadProperty<string>(URL1Property, reader.GetString("URL1"));
            //returnValue.LoadProperty<string>(Comments1Property, reader.GetString("COMMENTS1"));
            returnValue.LoadProperty<string>(TypeName1Property, reader.GetString("TYPE_NAME1"));
            //returnValue.LoadProperty<string>(Abstract1Property, reader.GetString("ABSTRACT1"));
            returnValue.LoadProperty<string>(OldItemId1Property, reader.GetString("OLD_ITEM_ID1"));
            returnValue.LoadProperty<bool>(IsDuplicate1Property, reader.GetBoolean("IS_DUPLICATE1"));
            returnValue.LoadProperty<bool>(IsChecked1Property, reader.GetBoolean("IS_CHECKED1"));
            returnValue.LoadProperty<int>(CodedCount1Property, reader.GetInt32("CODED_COUNT1"));
            returnValue.LoadProperty<int>(DocCount1Property, reader.GetInt32("DOC_COUNT1"));
            returnValue.LoadProperty<string>(Source1Property, reader.GetString("SOURCE1"));
            returnValue.LoadProperty<Int64>(ItemDuplicateId1Property, reader.GetInt64("ITEM_DUPLICATES_ID1"));

            returnValue.LoadProperty<Int64>(ItemId2Property, reader.GetInt64("ITEM_ID2"));
            returnValue.LoadProperty<int>(TypeId2Property, reader.GetInt32("TYPE_ID2"));
            returnValue.LoadProperty<string>(Authors2Property, reader.GetString("AUTHORS2"));
            returnValue.LoadProperty<string>(ParentAuthors2Property, reader.GetString("PARENT_AUTHORS2"));
            returnValue.LoadProperty<string>(Title2Property, reader.GetString("TITLE2"));
            returnValue.LoadProperty<string>(ParentTitle2Property, reader.GetString("PARENT_TITLE2"));
            //returnValue.LoadProperty<string>(ShortTitle2Property, reader.GetString("SHORT_TITLE2"));
            //returnValue.LoadProperty<SmartDate>(DateCreated2Property, reader.GetSmartDate("DATE_CREATED2"));
            //returnValue.LoadProperty<string>(CreatedBy2Property, reader.GetString("CREATED_BY2"));
            //returnValue.LoadProperty<SmartDate>(DateEdited2Property, reader.GetSmartDate("DATE_EDITED2"));
            //returnValue.LoadProperty<string>(EditedBy2Property, reader.GetString("EDITED_BY2"));
            //returnValue.LoadProperty<string>(Year2Property, reader.GetString("YEAR2"));
            //returnValue.LoadProperty<string>(Month2Property, reader.GetString("MONTH2"));
            //returnValue.LoadProperty<string>(StandardNumber2Property, reader.GetString("STANDARD_NUMBER2"));
            //returnValue.LoadProperty<string>(City2Property, reader.GetString("CITY2"));
            //returnValue.LoadProperty<string>(Country2Property, reader.GetString("COUNTRY2"));
            //returnValue.LoadProperty<string>(Publisher2Property, reader.GetString("PUBLISHER2"));
            //returnValue.LoadProperty<string>(Institution2Property, reader.GetString("INSTITUTION2"));
            //returnValue.LoadProperty<string>(Volume2Property, reader.GetString("VOLUME2"));
            //returnValue.LoadProperty<string>(Pages2Property, reader.GetString("PAGES2"));
            //returnValue.LoadProperty<string>(Edition2Property, reader.GetString("EDITION2"));
            //returnValue.LoadProperty<string>(Issue2Property, reader.GetString("ISSUE2"));
            //returnValue.LoadProperty<string>(URL2Property, reader.GetString("URL2"));
            //returnValue.LoadProperty<string>(Comments2Property, reader.GetString("COMMENTS2"));
            returnValue.LoadProperty<string>(TypeName2Property, reader.GetString("TYPE_NAME2"));
            //returnValue.LoadProperty<string>(Abstract2Property, reader.GetString("ABSTRACT2"));
            returnValue.LoadProperty<string>(OldItemId2Property, reader.GetString("OLD_ITEM_ID2"));
            returnValue.LoadProperty<bool>(IsDuplicate2Property, reader.GetBoolean("IS_DUPLICATE2"));
            returnValue.LoadProperty<bool>(IsChecked2Property, reader.GetBoolean("IS_CHECKED1"));
            //returnValue.LoadProperty<bool>(IsDuplicate2Property, reader.GetBoolean("IS_DUPLICATE2"));
            //returnValue.LoadProperty<bool>(IsChecked2Property, reader.GetBoolean("IS_CHECKED2"));
            returnValue.LoadProperty<int>(CodedCount2Property, reader.GetInt32("CODED_COUNT2"));
            returnValue.LoadProperty<int>(DocCount2Property, reader.GetInt32("DOC_COUNT2"));
            returnValue.LoadProperty<string>(Source2Property, reader.GetString("SOURCE2"));
            returnValue.LoadProperty<Int64>(ItemDuplicateId2Property, reader.GetInt64("ITEM_DUPLICATES_ID1"));
            //returnValue.LoadProperty<Int64>(ItemDuplicateId2Property, reader.GetInt64("ITEM_DUPLICATES_ID2"));
            
            //returnValue.LoadProperty<double>(SimilarityScoreProperty, reader.GetDouble("SCORE2"));
            returnValue.LoadProperty<double>(SimilarityScoreProperty, reader.GetDouble("SCORE1"));
            
            returnValue.MarkOld();
            return returnValue;
        }

#endif

        
    }
}
