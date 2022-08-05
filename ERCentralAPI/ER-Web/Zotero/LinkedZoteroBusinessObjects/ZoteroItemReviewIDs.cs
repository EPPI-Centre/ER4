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
using System.Linq;

//using Csla.Configuration;

#if !SILVERLIGHT
using System.Data.SqlClient;
using BusinessLibrary.Data;
#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class ZoteroItemReviewIDs : DynamicBindingListBase<ZoteroItemIDPerItemReview>
    {
        public static void GetZoteroReviewItem(EventHandler<DataPortalResult<ZoteroItemReviewIDs>> handler)
        {
            DataPortal<ZoteroItemReviewIDs> dp = new DataPortal<ZoteroItemReviewIDs>();
            dp.FetchCompleted += handler;
            dp.BeginFetch();
        }

#if SILVERLIGHT
    public ZoteroItemReviewIDs() { }

        
#else
        public ZoteroItemReviewIDs() { }
#endif


#if !SILVERLIGHT


        protected void DataPortal_Fetch(SingleCriteria<string> criteria)
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ZoteroItemReviewIDs", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@ItemIds", criteria.Value));
                                   
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (reader["ITEM_REVIEW_ID"] != null)
                            {
                                var itemReviewID = reader["ITEM_REVIEW_ID"] == DBNull.Value ? 0 : (long)reader["ITEM_REVIEW_ID"];
                                ZoteroItemIDPerItemReview zoteroItemIDPerItemReview = new ZoteroItemIDPerItemReview();
                                zoteroItemIDPerItemReview.ITEM_REVIEW_ID = itemReviewID;
                                Add(zoteroItemIDPerItemReview);
                            }
                        }
                    }
                }
                connection.Close();
            }
        }

#endif
    }
}