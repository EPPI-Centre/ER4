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
    public class ItemTimepointList : DynamicBindingListBase<ItemTimepoint>
    {
        public static void GetItemTimepointList(Int64 Id, EventHandler<DataPortalResult<ItemTimepointList>> handler)
        {
            DataPortal<ItemTimepointList> dp = new DataPortal<ItemTimepointList>();
            dp.FetchCompleted += handler;
            dp.BeginFetch(new SingleCriteria<Item, Int64>(Id));
        }
        public ItemTimepointList() { }

        public static ItemTimepointList NewItemTimepointList()
        {
            return new ItemTimepointList();
        }

        public ItemTimepoint GetItemTimepoint(Int64 ItemTimepointId)
        {
            ItemTimepoint retval = null;
            foreach (ItemTimepoint ia in this)
            {
                if (ItemTimepointId == ia.ItemTimepointId)
                {
                    retval = ia;
                    break;
                }
            }
            return retval;
        }

#if SILVERLIGHT
       
#else
        protected void DataPortal_Fetch(SingleCriteria<Item, Int64> criteria)
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            RaiseListChangedEvents = false;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ItemTimepointList", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@ITEM_ID", criteria.Value));
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        while (reader.Read())
                        {
                            Add(ItemTimepoint.GetItemTimepoint(reader));
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
