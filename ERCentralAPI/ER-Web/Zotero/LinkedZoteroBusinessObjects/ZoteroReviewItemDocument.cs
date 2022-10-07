using System.Text;
using Csla;
using Csla.Security;
using Csla.Core;
using Csla.Serialization;
using Csla.Silverlight;
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
#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class ZoteroReviewItemDocument : BusinessBase<ZoteroReviewItemDocument>
    {
        public static void GetZoteroReviewItem(EventHandler<DataPortalResult<ZoteroReviewItemDocument>> handler)
        {
            DataPortal<ZoteroReviewItemDocument> dp = new DataPortal<ZoteroReviewItemDocument>();
            dp.FetchCompleted += handler;
            dp.BeginFetch();
        }

#if SILVERLIGHT
    public ZoteroReviewItem() { }

        
#else
        public ZoteroReviewItemDocument() { }
#endif


        public static readonly PropertyInfo<string> FileKeyProperty = RegisterProperty<string>(new PropertyInfo<string>("FileKey", "FileKey", ""));
        public string FileKey
        {
            get
            {
                return GetProperty(FileKeyProperty);
            }
            set
            {
                SetProperty(FileKeyProperty, value);
            }
        }

        public static readonly PropertyInfo<long> ITEM_DOCUMENT_IDProperty = RegisterProperty<long>(new PropertyInfo<long>("ITEM_DOCUMENT_ID", "ITEM_DOCUMENT_ID", 0m));
        public long ITEM_DOCUMENT_ID
        {
            get
            {
                return GetProperty(ITEM_DOCUMENT_IDProperty);
            }
            set
            {
                SetProperty(ITEM_DOCUMENT_IDProperty, value);
            }
        }


        public static readonly PropertyInfo<string> parentItemProperty = RegisterProperty<string>(new PropertyInfo<string>("parentItem", "parentItem", ""));
        public string parentItem
        {
            get
            {
                return GetProperty(parentItemProperty);
            }
            set
            {
                SetProperty(parentItemProperty, value);
            }
        }


        public static readonly PropertyInfo<string> VersionProperty = RegisterProperty<string>(new PropertyInfo<string>("Version", "Version", ""));
        public string Version
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


        public static readonly PropertyInfo<DateTime> LAST_MODIFIEDProperty = RegisterProperty<DateTime>(new PropertyInfo<DateTime>("LAST_MODIFIED", "LAST_MODIFIED", 0m));
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


        public static readonly PropertyInfo<string> SimpleTextProperty = RegisterProperty<string>(new PropertyInfo<string>("SimpleText", "SimpleText", 0m));
        public string SimpleText
        {
            get
            {
                return GetProperty(SimpleTextProperty);
            }
            set
            {
                SetProperty(SimpleTextProperty, value);
            }
        }


        public static readonly PropertyInfo<string> FileNameProperty = RegisterProperty<string>(new PropertyInfo<string>("FileName", "FileName", ""));
        public string FileName
        {
            get
            {
                return GetProperty(FileNameProperty);
            }
            set
            {
                SetProperty(FileNameProperty, value);
            }
        }


        public static readonly PropertyInfo<string> ExtensionProperty = RegisterProperty<string>(new PropertyInfo<string>("Extension", "Extension", ""));
        public string Extension
        {
            get
            {
                return GetProperty(ExtensionProperty);
            }
            set
            {
                SetProperty(ExtensionProperty, value);
            }
        }




        public static readonly PropertyInfo<long> ITEM_IDProperty = RegisterProperty<long>(new PropertyInfo<long>("ITEM_ID", "ITEMID", 0m));
        public long ITEM_ID
        {
            get
            {
                return GetProperty(ITEM_IDProperty);
            }
            set
            {
                SetProperty(ITEM_IDProperty, value);
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

        public static readonly PropertyInfo<long> REVIEW_IDProperty = RegisterProperty<long>(new PropertyInfo<long>("REVIEW_ID", "REVIEW_ID", 0m));
        public long REVIEW_ID
        {
            get
            {
                return GetProperty(REVIEW_IDProperty);
            }
            set
            {
                SetProperty(REVIEW_IDProperty, value);
            }
        }




#if !SILVERLIGHT


        protected void DataPortal_Fetch(ZoteroReviewItemDocumentCriteria criteria)
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ItemIDByItemReviewID", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@ITEM_REVIEW_ID", criteria.ItemReviewID));
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", criteria.ReviewID));

                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        if (reader.Read())
                        {
                            if (reader["ITEM_ID"] != null)
                            {
                                this.LoadProperty<Int64>(ITEM_IDProperty, reader.GetInt64("ITEM_ID"));
                            }
                        }
                    }
                }
                connection.Close();
            }
        }

#endif
    }

    [Serializable]
    public class ZoteroReviewItemDocumentCriteria : Csla.CriteriaBase<ZoteroReviewItemDocumentCriteria>
    {
        private static PropertyInfo<long> ITEM_REVIEW_IDProperty = RegisterProperty<long>(typeof(ZoteroReviewItemDocumentCriteria), new PropertyInfo<long>("ITEM_REVIEW_ID", "ITEM_REVIEW_ID"));
        public long ItemReviewID
        {
            get { return ReadProperty(ITEM_REVIEW_IDProperty); }
        }
        private static PropertyInfo<Int64> ReviewIDProperty = RegisterProperty<Int64>(typeof(ZoteroReviewItemDocumentCriteria), new PropertyInfo<Int64>("ReviewID", "ReviewID"));
        public Int64 ReviewID
        {
            get { return ReadProperty(ReviewIDProperty); }
        }

        public ZoteroReviewItemDocumentCriteria(long ItemReviewID, Int64 ReviewID)
        {
            LoadProperty(ITEM_REVIEW_IDProperty, ItemReviewID);
            LoadProperty(ReviewIDProperty, ReviewID);
        }


#if !SILVERLIGHT
        public ZoteroReviewItemDocumentCriteria() { }
#endif
    }
}