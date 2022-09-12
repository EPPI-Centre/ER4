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
    public class ZoteroReviewItem : BusinessBase<ZoteroReviewItem>
    {
        public static void GetZoteroReviewItem(EventHandler<DataPortalResult<ZoteroReviewItem>> handler)
        {
            DataPortal<ZoteroReviewItem> dp = new DataPortal<ZoteroReviewItem>();
            dp.FetchCompleted += handler;
            dp.BeginFetch();
        }

#if SILVERLIGHT
    public ZoteroReviewItem() { }

        
#else
        public ZoteroReviewItem() { }
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

        public static readonly PropertyInfo<string> TypeNameProperty = RegisterProperty<string>(new PropertyInfo<string>("TypeName", "TypeName", ""));
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


#if !SILVERLIGHT


        protected void DataPortal_Fetch(SingleCriteria<ZoteroReviewItem, string> criteria)
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ItemReviewZotero", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@ItemKey", criteria.Value)); 
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
                            LoadProperty<string>(TypeNameProperty, reader.GetString("TypeName"));
                            MarkOld();
                        }
                    }
                }
                connection.Close();
            }
        }

        protected override void DataPortal_Insert()
        {
            AddNew();
        }

        private void AddNew()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ZoteroItemReviewCreate", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@ItemKey", ReadProperty(ItemKeyProperty)));
                    command.Parameters.Add(new SqlParameter("@LibraryID", ReadProperty(LibraryIDProperty)));
                    command.Parameters.Add(new SqlParameter("@Version", ReadProperty(VersionProperty)));
                    command.Parameters.Add(new SqlParameter("@LAST_MODIFIED", ReadProperty(LAST_MODIFIEDProperty)));
                    command.Parameters.Add(new SqlParameter("@ITEM_REVIEW_ID", ReadProperty(ITEM_REVIEW_IDProperty)));
                    command.Parameters.Add(new SqlParameter("@TypeName", ReadProperty(TypeNameProperty)));
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }


        protected override void DataPortal_Update()
        {
 
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                //ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ItemReviewZoteroUpdate", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@Zotero_item_review_ID", ReadProperty(Zotero_item_review_IDProperty)));
                    command.Parameters.Add(new SqlParameter("@ItemKey", ReadProperty(ItemKeyProperty)));
                    command.Parameters.Add(new SqlParameter("@LibraryID", ReadProperty(LibraryIDProperty)));
                    command.Parameters.Add(new SqlParameter("@Version", ReadProperty(VersionProperty)));
                    command.Parameters.Add(new SqlParameter("@LAST_MODIFIED", ReadProperty(LAST_MODIFIEDProperty)));
                    command.Parameters.Add(new SqlParameter("@ITEM_REVIEW_ID", ReadProperty(ITEM_REVIEW_IDProperty)));
                    command.Parameters.Add(new SqlParameter("@TypeName", ReadProperty(TypeNameProperty)));
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
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

        internal static ZoteroReviewItem GetZoteroReviewItem(SafeDataReader reader)
        {
            ZoteroReviewItem returnValue = new ZoteroReviewItem();
            returnValue.LoadProperty<long>(Zotero_item_review_IDProperty, reader.GetInt64("Zotero_item_review_ID"));
            returnValue.LoadProperty<string>(ItemKeyProperty, reader.GetString("ItemKey"));
            returnValue.LoadProperty<string>(LibraryIDProperty, reader.GetString("LibraryID"));
            returnValue.LoadProperty<long>(ITEM_REVIEW_IDProperty, reader.GetInt64("ITEM_REVIEW_ID"));
            returnValue.LoadProperty<long>(VersionProperty, reader.GetInt64("Version"));
            returnValue.LoadProperty<DateTime>(LAST_MODIFIEDProperty, reader.GetDateTime("LAST_MODIFIED"));
            returnValue.LoadProperty<string>(TypeNameProperty, reader.GetString("TypeName"));

            returnValue.MarkOld();

            return returnValue;
        }
#endif
    }
}