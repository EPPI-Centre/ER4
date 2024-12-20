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

        public ZoteroERWebReviewItemList() { }


#if SILVERLIGHT
       
#else


        protected void DataPortal_Fetch(SingleCriteria<ZoteroERWebReviewItemList, string> criteria)
        {
            RaiseListChangedEvents = false;
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ZoteroErWebReviewItemList", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@AttributeId", criteria.Value));
                    command.Parameters.Add(new SqlParameter("@ReviewId", ri.ReviewId));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        while (reader.Read())
                        {
                            Add(ZoteroERWebReviewItem.GetZoteroERWebReviewItem(reader));
                        }

                        reader.NextResult();

                        while (reader.Read())
                        {
                            var itemId = reader.GetInt64("ITEM_ID");
                            var currentItem = this.Where(f => f.ItemID == itemId).FirstOrDefault();
                            if (currentItem != null)
                            {
                                var zoteroDoc = new ZoteroERWebItemDocument(reader.GetInt64("ITEM_DOCUMENT_ID"),  reader.GetString("DocZoteroKey"));
                                if (zoteroDoc != null)
                                {
                                    currentItem.PdfList.Add(zoteroDoc);
                                }                                
                            }
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