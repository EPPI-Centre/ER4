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
    public class ZoteroItemIDByItemReview : BusinessBase<ZoteroItemIDByItemReview>
    {
        public static void GetZoteroReviewItem(EventHandler<DataPortalResult<ZoteroItemIDByItemReview>> handler)
        {
            DataPortal<ZoteroItemIDByItemReview> dp = new DataPortal<ZoteroItemIDByItemReview>();
            dp.FetchCompleted += handler;
            dp.BeginFetch();
        }

#if SILVERLIGHT
        public ZoteroItemIDByItemReview() { }

        
#else
        public ZoteroItemIDByItemReview() { }
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


        protected void DataPortal_Fetch(ZoteroItemIDByItemReviewCriteria criteria)
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
    public class ZoteroItemIDByItemReviewCriteria : Csla.CriteriaBase<ZoteroItemIDByItemReviewCriteria>
    {
        private static PropertyInfo<long> ITEM_REVIEW_IDProperty = RegisterProperty<long>(typeof(ZoteroItemIDByItemReviewCriteria), new PropertyInfo<long>("ITEM_REVIEW_ID", "ITEM_REVIEW_ID"));
        public long ItemReviewID
        {
            get { return ReadProperty(ITEM_REVIEW_IDProperty); }
        }
        private static PropertyInfo<Int64> ReviewIDProperty = RegisterProperty<Int64>(typeof(ZoteroItemIDByItemReviewCriteria), new PropertyInfo<Int64>("ReviewID", "ReviewID"));
        public Int64 ReviewID
        {
            get { return ReadProperty(ReviewIDProperty); }
        }

        public ZoteroItemIDByItemReviewCriteria(long ItemReviewID, Int64 ReviewID)
        {
            LoadProperty(ITEM_REVIEW_IDProperty, ItemReviewID);
            LoadProperty(ReviewIDProperty, ReviewID);
        }


#if !SILVERLIGHT
        public ZoteroItemIDByItemReviewCriteria() { }
#endif
    }
}