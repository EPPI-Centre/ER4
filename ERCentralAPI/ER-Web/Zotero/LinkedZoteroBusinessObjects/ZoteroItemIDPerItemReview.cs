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
    public class ZoteroItemIDPerItemReview : BusinessBase<ZoteroItemIDPerItemReview>
    {
        public static void GetZoteroReviewItem(EventHandler<DataPortalResult<ZoteroItemIDPerItemReview>> handler)
        {
            DataPortal<ZoteroItemIDPerItemReview> dp = new DataPortal<ZoteroItemIDPerItemReview>();
            dp.FetchCompleted += handler;
            dp.BeginFetch();
        }

#if SILVERLIGHT
    public ZoteroItemIDPerItemReview() { }

        
#else
        public ZoteroItemIDPerItemReview() { }
#endif


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




#if !SILVERLIGHT


        protected void DataPortal_Fetch(SingleCriteria<Int64> criteria)
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ItemIDPerItemReviewID", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@ItemReviewID", criteria.Value));

                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        if (reader.Read())
                        {
                            if (reader["ITEM_ID"] != null)
                            {
                                LoadProperty<Int64>(ITEM_IDProperty, reader.GetInt64("ITEM_ID"));
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