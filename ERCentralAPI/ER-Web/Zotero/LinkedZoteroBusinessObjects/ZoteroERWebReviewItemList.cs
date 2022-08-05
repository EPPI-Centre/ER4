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

#if!SILVERLIGHT
using System.Data.SqlClient;
using BusinessLibrary.Data;
using BusinessLibrary.Security;
#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class ZoteroERWebReviewItemList : DynamicBindingListBase<ZoteroERWebReviewItem>
    {
        public static void GetZoteroERWebReviewItemList(EventHandler<DataPortalResult<ZoteroERWebReviewItemList>> handler)
        {
            DataPortal<ZoteroERWebReviewItemList> dp = new DataPortal<ZoteroERWebReviewItemList>();
            dp.FetchCompleted += handler;
            dp.BeginFetch();
        }


#if SILVERLIGHT
        public ZoteroERWebReviewItemList() { }
#else
        public ZoteroERWebReviewItemList() { }
#endif


#if SILVERLIGHT
       
#else


        protected void DataPortal_Fetch()
        {
            RaiseListChangedEvents = false;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ItemsInERWebANDZotero", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        while (reader.Read())
                        {
                            Add(ZoteroERWebReviewItem.GetZoteroERWebReviewItem(reader));
                        }
                    }
                }
                connection.Close();
            }
            RaiseListChangedEvents = true;
        }
#endif

    }
}