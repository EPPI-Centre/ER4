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
using Csla.Data;
using System.Data.SqlClient;
using BusinessLibrary.Data;
using BusinessLibrary.Security;
#endif


namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class ItemDuplicatesReadOnlyList : ReadOnlyListBase<ItemDuplicatesReadOnlyList, ItemDuplicatesReadOnly>
    {
#if SILVERLIGHT
        public ItemDuplicatesReadOnlyList() { }
#else
        private ItemDuplicatesReadOnlyList() { }
#endif

        public static void getItemDuplicatesReadOnlyList(long ItemID, EventHandler<DataPortalResult<ItemDuplicatesReadOnlyList>> handler)
        {
            DataPortal<ItemDuplicatesReadOnlyList> dp = new DataPortal<ItemDuplicatesReadOnlyList>();
            dp.FetchCompleted += handler;
            dp.BeginFetch(new SingleCriteria<ItemDuplicatesReadOnlyList, long>(ItemID));
        }

#if !SILVERLIGHT

        private void DataPortal_Fetch(SingleCriteria<ItemDuplicatesReadOnlyList, long> criteria)
        {
            IsReadOnly = false;
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            RaiseListChangedEvents = false;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ItemDuplicatesReadOnlyList", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@RevID", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@ItemID", criteria.Value));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        while (reader.Read())
                        {
                            Add(ItemDuplicatesReadOnly.GetItemDuplicateReadOnlyGroup(reader));
                        }
                    }
                }
                connection.Close();
            }
            IsReadOnly = true;
            RaiseListChangedEvents = true;
        }

        


#endif
    }
}
