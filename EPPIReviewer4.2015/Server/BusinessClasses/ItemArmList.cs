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
    public class ItemArmList : DynamicBindingListBase<ItemArm>
    {
        public static void GetItemArmList(Int64 Id, EventHandler<DataPortalResult<ItemArmList>> handler)
        {
            DataPortal<ItemArmList> dp = new DataPortal<ItemArmList>();
            dp.FetchCompleted += handler;
            dp.BeginFetch(new SingleCriteria<Item, Int64>(Id));
        }


#if SILVERLIGHT
        public ItemArmList() { }
#else
        private ItemArmList() { }
#endif

        public static ItemArmList NewItemArmList()
        {
            return new ItemArmList();
        }

        public ItemArm GetItemArm(Int64 ItemArmId)
        {
            ItemArm retval = null;
            foreach (ItemArm ia in this)
            {
                if (ItemArmId == ia.ItemArmId)
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
                using (SqlCommand command = new SqlCommand("st_ItemArmList", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@ITEM_ID", criteria.Value));
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        while (reader.Read())
                        {
                            Add(ItemArm.GetItemArm(reader));
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
