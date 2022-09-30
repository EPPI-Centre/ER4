using System.Text;
using Csla;
using Csla.Security;
using Csla.Core;
using Csla.Serialization;
using Csla.Silverlight;
//using Csla.Validation;
using Csla.DataPortalClient;
using System.IO;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

//using Csla.Configuration;

#if !SILVERLIGHT
using System.Data.SqlClient;
using BusinessLibrary.Data;
using Csla.Data;
using BusinessLibrary.Security;
using System.Configuration;
using System.Data;
using System.Threading;
using System.Collections;
using System.Globalization;
using System.Linq;
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
        public ZoteroERWebReviewItem() { }
#endif

        public static readonly PropertyInfo<long> Zotero_item_review_IDProperty = RegisterProperty<long>(new PropertyInfo<long>("Zotero_item_review_ID", "Zotero_item_review_ID", 0m));
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
        [JsonProperty]
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

        public static readonly PropertyInfo<long> ITEM_REVIEW_IDProperty = RegisterProperty<long>(new PropertyInfo<long>("ITEM_REVIEW_ID", "ITEM_REVIEW_ID", 0m));
        public long ITEM_REVIEW_ID
        {
            get
            {
                return GetProperty(ITEM_REVIEW_IDProperty);
            }
            set
            {
                SetProperty(ITEM_REVIEW_IDProperty, value);
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
            doesNotExist,
            behind,
            upToDate,
            ahead,
            pdfDoesNotExist,
            pdfExists
        }


        public static readonly PropertyInfo<ErWebState> StateProperty = RegisterProperty<ErWebState>(new PropertyInfo<ErWebState>("SYNC_STATE", "SYNC_STATE", ErWebState.doesNotExist));
        public ErWebState SyncState
        {
            get
            {
                return GetProperty(StateProperty);
            }
            set
            {
                SetProperty(StateProperty, value);
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

        //public static readonly PropertyInfo<string> FileKeyProperty = RegisterProperty<string>(new PropertyInfo<string>("FILE_KEY", "FILE_KEY", ""));
        //public string FileKey
        //{
        //    get
        //    {
        //        return GetProperty(FileKeyProperty);
        //    }
        //    set
        //    {
        //        SetProperty(FileKeyProperty, value);
        //    }
        //}

        //public static readonly PropertyInfo<long> ItemDocumentIDProperty = RegisterProperty<long>(new PropertyInfo<long>("ITEM_DOCUMENT_ID", "ITEM_DOCUMENT_ID", 0m));
        //public long ItemDocumentID
        //{
        //    get
        //    {
        //        return GetProperty(ItemDocumentIDProperty);
        //    }
        //    set
        //    {
        //        SetProperty(ItemDocumentIDProperty, value);
        //    }
        //}


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
                            LoadProperty<string>(LibraryIDProperty, reader.GetString("LibraryID"));                            
                            LoadProperty<long>(ITEM_REVIEW_IDProperty, reader.GetInt64("ITEM_REVIEW_ID"));
                            LoadProperty<long>(VersionProperty, reader.GetInt64("Version"));
                            LoadProperty<DateTime>(LAST_MODIFIEDProperty, reader.GetDateTime("LAST_MODIFIED"));
                            LoadProperty<long>(ItemIDProperty, reader.GetInt64("ITEM_ID"));
                            LoadProperty<string>(TitleProperty, reader.GetString("TITLE"));
                            LoadProperty<string>(ShortTitleProperty, reader.GetString("SHORT_TITLE"));
                            LoadProperty<ErWebState>(StateProperty, (ErWebState)reader.GetInt32("SyncState"));
                            //LoadProperty<List<int>>(PdfListProperty, GetPdfList(criteria));
                            MarkOld();
                        }
                    }
                }
                connection.Close();
            }
        }
       

        //internal static List<int> GetPdfList(SingleCriteria<ZoteroERWebReviewItem, string> criteria)
        //{
        //    var documentList = new List<int>();
        //    using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
        //    {
        //        connection.Open();
        //        using (SqlCommand command = new SqlCommand("st_ZoteroDocumentIdsPerItemReviewId", connection))
        //        {
        //            command.CommandType = System.Data.CommandType.StoredProcedure;
        //            command.Parameters.Add(new SqlParameter("@ItemReviewId", criteria.Value));
        //            using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
        //            {
        //                while (reader.Read())
        //                {
        //                    documentList.Add(reader.GetInt32("ITEM_DCOUMENT_ID"));
        //                }
        //            }
        //        }
        //        connection.Close();
        //    }
        //    return documentList;
        //}

        internal static ZoteroERWebReviewItem GetZoteroERWebReviewItem(SafeDataReader reader)
        {
            ZoteroERWebReviewItem returnValue = new ZoteroERWebReviewItem();
            //returnValue.LoadProperty<long>(Zotero_item_review_IDProperty, reader.GetInt64("Zotero_item_review_ID"));
            returnValue.LoadProperty<string>(ItemKeyProperty, reader.GetString("ItemKey"));
            //returnValue.LoadProperty<string>(LibraryIDProperty, reader.GetString("LibraryID"));
            //returnValue.LoadProperty<long>(ITEM_REVIEW_IDProperty, reader.GetInt64("ITEM_REVIEW_ID"));
            //returnValue.LoadProperty<long>(VersionProperty, reader.GetInt64("Version"));
            returnValue.LoadProperty<DateTime>(LAST_MODIFIEDProperty, reader.GetDateTime("DATE_EDITED"));
            returnValue.LoadProperty<long>(ItemIDProperty, reader.GetInt64("ITEM_ID"));


            //returnValue.LoadProperty<string>(ShortTitleProperty, reader.GetString("SHORT_TITLE"));
            //returnValue.LoadProperty<string>(TitleProperty, reader.GetString("TITLE"));
            //returnValue.LoadProperty<string>(TypeNameProperty, reader.GetString("TypeName"));
            //returnValue.LoadProperty<ErWebState>(StateProperty, (ErWebState)reader.GetInt32("SyncState"));
            returnValue.MarkOld();

            return returnValue;
        }

        //internal static ZoteroERWebReviewItem GetZoteroERWebReviewItemBottomSet(SafeDataReader reader)
        //{
        //    ZoteroERWebReviewItem returnValue = new ZoteroERWebReviewItem();

        //    returnValue.LoadProperty<long>(ItemDocumentIDProperty, reader.GetInt64("ITEM_DOCUMENT_ID"));
        //    returnValue.LoadProperty<string>(FileKeyProperty, reader.GetString("FileKey"));

        //    returnValue.MarkOld();

        //    return returnValue;

        //}
#endif
    }
}