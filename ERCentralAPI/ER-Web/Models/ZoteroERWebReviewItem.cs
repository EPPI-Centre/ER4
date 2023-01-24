using Csla;
using Csla.Core;
using Newtonsoft.Json;
using BusinessLibrary.Security;

#if !SILVERLIGHT
using System.Data.SqlClient;
using BusinessLibrary.Data;
using Csla.Data;
#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class ZoteroERWebReviewItem : BusinessBase<ZoteroERWebReviewItem>
    {
        public static void GetZoteroERWebReviewItem(EventHandler<DataPortalResult<ZoteroERWebReviewItem>> handler)
        {
            DataPortal<ZoteroERWebReviewItem> dp = new DataPortal<ZoteroERWebReviewItem>();
            dp.FetchCompleted += handler;
            dp.BeginFetch();
        }

#if SILVERLIGHT
        public ZoteroERWebReviewItem() { }        
#else
        public ZoteroERWebReviewItem() {
            this.PdfList = new MobileList<ZoteroERWebItemDocument> ();
        }
#endif

        public static readonly PropertyInfo<long> Zotero_item_review_IDProperty = RegisterProperty<long>(new PropertyInfo<long>("Zotero_item_review_ID",
            "Zotero_item_review_ID", 0m));
        public long Zotero_item_review_ID
        {
            get
            {
                return GetProperty(Zotero_item_review_IDProperty);
            }
            set
            {
                SetProperty(Zotero_item_review_IDProperty, value);
            }
        }


        public static readonly PropertyInfo<string> ItemKeyProperty = RegisterProperty<string>(new PropertyInfo<string>("ItemKey", "ItemKey", ""));
        //[JsonProperty]
        public string ItemKey
        {
            get
            {
                return GetProperty(ItemKeyProperty);
            }
            set
            {
                SetProperty(ItemKeyProperty, value);
            }
        }


        public static readonly PropertyInfo<string> LibraryIDProperty = RegisterProperty<string>(new PropertyInfo<string>("LibraryID", "LibraryID", ""));
        public string LibraryID
        {
            get
            {
                return GetProperty(LibraryIDProperty);
            }
            set
            {
                SetProperty(LibraryIDProperty, value);
            }
        }

        public static readonly PropertyInfo<long> iteM_REVIEW_IDProperty = RegisterProperty<long>(new PropertyInfo<long>("iteM_REVIEW_ID", "iteM_REVIEW_ID", 0m));
        public long iteM_REVIEW_ID
        {
            get
            {
                return GetProperty(iteM_REVIEW_IDProperty);
            }
            set
            {
                SetProperty(iteM_REVIEW_IDProperty, value);
            }
        }

        public static readonly PropertyInfo<long> VersionProperty = RegisterProperty<long>(new PropertyInfo<long>("Version", "Version", 0m));
        public long Version
        {
            get
            {
                return GetProperty(VersionProperty);
            }
            set
            {
                SetProperty(VersionProperty, value);
            }
        }

        public static readonly PropertyInfo<long> ItemIDProperty = RegisterProperty<long>(new PropertyInfo<long>("ITEM_ID", "ITEM_ID", 0m));
        public long ItemID
        {
            get
            {
                return GetProperty(ItemIDProperty);
            }
            set
            {
                SetProperty(ItemIDProperty, value);
            }
        }
        public static readonly PropertyInfo<string> ShortTitleProperty = RegisterProperty<string>(new PropertyInfo<string>("SHORT_TITLE", "SHORT_TITLE", ""));
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

        public static readonly PropertyInfo<string> TitleProperty = RegisterProperty<string>(new PropertyInfo<string>("TITLE", "TITLE", ""));
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


        public static readonly PropertyInfo<DateTime> LAST_MODIFIEDProperty = RegisterProperty<DateTime>(new PropertyInfo<DateTime>("LAST_MODIFIED", "LAST_MODIFIED", DateTime.Now));
        public DateTime LAST_MODIFIED
        {
            get
            {
                return GetProperty(LAST_MODIFIEDProperty);
            }
            set
            {
                SetProperty(LAST_MODIFIEDProperty, value);
            }
        }

        public static readonly PropertyInfo<string> TypeNameProperty = RegisterProperty<string>(new PropertyInfo<string>("TYPE_NAME", "TYPE_NAME", ""));
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

        public enum ErWebState
        {
            notSet,
            upToDate,
            canPush,
            canPull
        }

		public static readonly PropertyInfo<ErWebState> SyncStateProperty = RegisterProperty<ErWebState>(new PropertyInfo<ErWebState>("SyncState", "SyncState", ErWebState.notSet));
		public ErWebState SyncState
		{
			get
			{
				return GetProperty(SyncStateProperty);
			}
			set
			{
				SetProperty(SyncStateProperty, value);
			}
		}



		public static readonly PropertyInfo<MobileList<ZoteroERWebItemDocument>> PdfListProperty = RegisterProperty<MobileList<ZoteroERWebItemDocument>>(new PropertyInfo<MobileList<ZoteroERWebItemDocument>>("PDF_LIST", "PDF_LIST", new MobileList<ZoteroERWebItemDocument>()));
        public MobileList<ZoteroERWebItemDocument> PdfList
        {
            get
            {
                return GetProperty(PdfListProperty);
            }
            set
            {
                SetProperty(PdfListProperty, value);
            }
        }


#if !SILVERLIGHT


        protected void DataPortal_Fetch(SingleCriteria<ZoteroERWebReviewItem, string> criteria)
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ItemInERWebANDZotero", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@ItemReviewId", criteria.Value));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        if (reader.Read())
                        {
                            LoadProperty<long>(Zotero_item_review_IDProperty, reader.GetInt64("Zotero_item_review_ID"));
                            LoadProperty<string>(ItemKeyProperty, reader.GetString("ItemKey"));
                            //LoadProperty<string>(LibraryIDProperty, reader.GetString("LibraryID"));                            
                            LoadProperty<long>(iteM_REVIEW_IDProperty, reader.GetInt64("ITEM_REVIEW_ID"));
                            //LoadProperty<long>(VersionProperty, reader.GetInt64("Version"));
                            LoadProperty<DateTime>(LAST_MODIFIEDProperty, reader.GetDateTime("LAST_MODIFIED"));
                            LoadProperty<long>(ItemIDProperty, reader.GetInt64("ITEM_ID"));
                            LoadProperty<string>(TitleProperty, reader.GetString("TITLE"));
                            LoadProperty<string>(ShortTitleProperty, reader.GetString("SHORT_TITLE"));
                            MarkOld();
                        }
                    }
                }
                connection.Close();
            }
        }


        protected override void DataPortal_Insert()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ZoteroItemReviewCreate", connection))
                {
                    ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@ReviewId", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@ItemKey", ReadProperty(ItemKeyProperty)));
                    //command.Parameters.Add(new SqlParameter("@LibraryID", ReadProperty(LibraryIDProperty)));
                    //command.Parameters.Add(new SqlParameter("@Version", ReadProperty(VersionProperty)));
                    //command.Parameters.Add(new SqlParameter("@LAST_MODIFIED", ReadProperty(LAST_MODIFIEDProperty)));
                    //command.Parameters.Add(new SqlParameter("@ITEM_ID", ReadProperty(ItemIDProperty)));
                    command.Parameters.Add(new SqlParameter("@ITEM_REVIEW_ID", ReadProperty(iteM_REVIEW_IDProperty)));
                    //command.Parameters.Add(new SqlParameter("@TypeName", ReadProperty(TypeNameProperty)));
                    command.ExecuteNonQuery();

                }
                connection.Close();
            }
        }



        protected override void DataPortal_Update()
        {//I don't think we should ever update these - link either exists or doesn't so Create, Read, Delete only...

            //using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            //{
            //    //ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            //    connection.Open();
            //    using (SqlCommand command = new SqlCommand("st_ItemReviewZoteroUpdate", connection))
            //    {
            //        command.CommandType = System.Data.CommandType.StoredProcedure;
            //        command.Parameters.Add(new SqlParameter("@Zotero_item_review_ID", ReadProperty(Zotero_item_review_IDProperty)));
            //        command.Parameters.Add(new SqlParameter("@ItemKey", ReadProperty(ItemKeyProperty)));
            //        command.Parameters.Add(new SqlParameter("@LibraryID", ReadProperty(LibraryIDProperty)));
            //        command.Parameters.Add(new SqlParameter("@Version", ReadProperty(VersionProperty)));
            //        command.Parameters.Add(new SqlParameter("@LAST_MODIFIED", ReadProperty(LAST_MODIFIEDProperty)));
            //        command.Parameters.Add(new SqlParameter("@ITEM_ID", ReadProperty(ItemIDProperty)));
            //        command.Parameters.Add(new SqlParameter("@ITEM_REVIEW_ID", ReadProperty(iteM_REVIEW_IDProperty)));
            //        command.Parameters.Add(new SqlParameter("@TypeName", ReadProperty(TypeNameProperty)));
            //        command.ExecuteNonQuery();
            //    }
            //    connection.Close();
            //}
        }


        protected override void DataPortal_DeleteSelf()
        {

            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ZoteroItemReviewDelete", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;

                    command.Parameters.Add(new SqlParameter("@ItemKey", ReadProperty(ItemKeyProperty)));
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        internal static ZoteroERWebReviewItem GetZoteroERWebReviewItem(SafeDataReader reader)
        {
            ZoteroERWebReviewItem returnValue = new ZoteroERWebReviewItem();
            returnValue.LoadProperty<long>(Zotero_item_review_IDProperty, reader.GetInt64("Zotero_item_review_ID"));
            returnValue.LoadProperty<string>(ItemKeyProperty, reader.GetString("ItemKey"));
            //returnValue.LoadProperty<string>(LibraryIDProperty, reader.GetString("LibraryID"));
            returnValue.LoadProperty<long>(iteM_REVIEW_IDProperty, reader.GetInt64("iteM_REVIEW_ID"));
            //returnValue.LoadProperty<long>(VersionProperty, reader.GetInt64("Version"));
            returnValue.LoadProperty<DateTime>(LAST_MODIFIEDProperty, reader.GetDateTime("DATE_EDITED"));
            returnValue.LoadProperty<long>(ItemIDProperty, reader.GetInt64("ITEM_ID"));
            returnValue.LoadProperty<string>(TypeNameProperty, reader.GetString("TypeName"));
            returnValue.LoadProperty<string>(TitleProperty, reader.GetString("TITLE"));
            returnValue.LoadProperty<string>(ShortTitleProperty, reader.GetString("SHORT_TITLE"));
            returnValue.MarkOld();

            return returnValue;
        }


#endif
    }
}