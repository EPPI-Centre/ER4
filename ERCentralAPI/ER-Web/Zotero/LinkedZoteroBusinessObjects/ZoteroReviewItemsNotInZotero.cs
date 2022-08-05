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
    public class ZoteroReviewItemsNotInZotero : DynamicBindingListBase<ZoteroItemIDPerItemReview>
    {
        public static void GetZoteroReviewItemsNotInZotero(EventHandler<DataPortalResult<ZoteroReviewItemsNotInZotero>> handler)
        {
            DataPortal<ZoteroReviewItemsNotInZotero> dp = new DataPortal<ZoteroReviewItemsNotInZotero>();
            dp.FetchCompleted += handler;
            dp.BeginFetch();
        }

#if SILVERLIGHT
    public ZoteroReviewItemsNotInZotero() { }

        
#else
        public ZoteroReviewItemsNotInZotero() { }
#endif

        //public static readonly PropertyInfo<long> ITEM_IDProperty = RegisterProperty<long>(new PropertyInfo<long>("ITEM_ID", "ITEM_ID", 0m));
        //public long ITEM_ID
        //{
        //    get
        //    {
        //        return GetProperty(ITEM_IDProperty);
        //    }
        //    set
        //    {
        //        SetProperty(ITEM_IDProperty, value);
        //    }
        //}

        //public static readonly PropertyInfo<long> ITEM_REVIEW_IDProperty = RegisterProperty<long>(new PropertyInfo<long>("ITEM_REVIEW_ID", "ITEM_REVIEW_ID", 0m));
        //public long ITEM_REVIEW_ID
        //{
        //    get
        //    {
        //        return GetProperty(ITEM_REVIEW_IDProperty);
        //    }
        //    set
        //    {
        //        SetProperty(ITEM_REVIEW_IDProperty, value);
        //    }
        //}



#if !SILVERLIGHT


        protected void DataPortal_Fetch(SingleCriteria<Item, long> criteria)
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ItemsNotInZotero", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@ReviewID", criteria.Value)); 
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        while (reader.Read())
                        {
                            var itemReviewID = reader["ITEM_REVIEW_ID"] == DBNull.Value ? 0 : (long)reader["ITEM_REVIEW_ID"];
                            var itemID = reader["ITEM_ID"] == DBNull.Value ? 0 : (long)reader["ITEM_ID"];
                            var itemDocumentID = reader.IsDBNull(2) ? 0 : (long)reader["ITEM_DOCUMENT_ID"];
                            ZoteroItemIDPerItemReview zoteroItemIDPerItemReview = new ZoteroItemIDPerItemReview
                            {
                                ITEM_REVIEW_ID = itemReviewID,
                                ITEM_ID = itemID,
                                ITEM_DOCUMENT_ID = itemDocumentID
                            };
                            Add(zoteroItemIDPerItemReview);
                        }
                    }
                }
                connection.Close();
            }
        }

#endif
    }
}