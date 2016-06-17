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
    public class ItemLinkList : DynamicBindingListBase<ItemLink>
    {
        public static void GetItemLinkList(Int64 ItemId, EventHandler<DataPortalResult<ItemLinkList>> handler)
        {
            DataPortal<ItemLinkList> dp = new DataPortal<ItemLinkList>();
            dp.FetchCompleted += handler;
            dp.BeginFetch(new SingleCriteria<AttributeSetList, Int64>(ItemId));
        }


#if SILVERLIGHT
        public ItemLinkList() { }
#else
        private ItemLinkList() { }
#endif


#if SILVERLIGHT
       
#else


        protected void DataPortal_Fetch(SingleCriteria<AttributeSetList, Int64> criteria)
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            RaiseListChangedEvents = false;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ItemLinkList", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@ITEM_ID", criteria.Value));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        while (reader.Read())
                        {
                            Add(ItemLink.GetItemLink(reader));
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
