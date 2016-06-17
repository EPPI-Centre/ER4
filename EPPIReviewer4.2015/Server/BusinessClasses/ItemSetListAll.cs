//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Csla;
//using Csla.Security;
//using Csla.Core;
//using Csla.Serialization;
//using Csla.Silverlight;
////using Csla.Validation;

//#if!SILVERLIGHT
//using Csla.Data;
//using System.Data.SqlClient;
//using BusinessLibrary.Data;
//using BusinessLibrary.Security;
//#endif

//namespace BusinessLibrary.BusinessClasses
//{
//    [Serializable]
//    //public class ItemSetListAll : BusinessListBase<ItemSetListAll, ItemSet>
//    public class ItemSetListAll : DynamicBindingListBase<ItemSet>
//    {
//        public static void GetItemSetListAll(Int64 itemId, EventHandler<DataPortalResult<ItemSetListAll>> handler)
//        {
//            DataPortal<ItemSetListAll> dp = new DataPortal<ItemSetListAll>();
//            dp.FetchCompleted += handler;
//            dp.BeginFetch(new SingleCriteria<ItemSetListAll, Int64>(itemId));
//        }



//#if SILVERLIGHT
//        public ItemSetListAll() { }
//#else
//        private ItemSetListAll() { }
//#endif

//        internal static ItemSetListAll NewItemSetListAll()
//        {
//            return new ItemSetListAll();
//        }


//#if SILVERLIGHT
    
//#else
//        protected void DataPortal_Fetch(SingleCriteria<ItemSetListAll, Int64> criteria)
//        {
//            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
//            RaiseListChangedEvents = false;
//            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
//            {
//                connection.Open();
//                using (SqlCommand command = new SqlCommand("st_ItemSetDataListAll", connection))
//                {
//                    command.CommandType = System.Data.CommandType.StoredProcedure;
//                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
//                    //command.Parameters.Add(new SqlParameter("@CONTACT_ID", ri.UserId));
//                    command.Parameters.Add(new SqlParameter("@ITEM_ID", criteria.Value));
//                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
//                    {
//                        while (reader.Read())
//                        {
//                            Add(ItemSet.GetItemSet(reader));
//                        }
//                    }
//                }
//                connection.Close();
//            }
//            RaiseListChangedEvents = true;
//        }
//#endif

//    }
//}
