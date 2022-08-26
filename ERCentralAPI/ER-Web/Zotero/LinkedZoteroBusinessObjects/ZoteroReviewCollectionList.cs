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
//using System.Data.SqlClient;
//using BusinessLibrary.Data;
//using BusinessLibrary.Security;
//#endif

//namespace BusinessLibrary.BusinessClasses
//{
//    [Serializable]
//    public class ZoteroReviewCollectionList : DynamicBindingListBase<ZoteroReviewCollection>
//    {

//        public static void GetMagSearchList(EventHandler<DataPortalResult<ZoteroReviewCollectionList>> handler)
//        {
//            DataPortal<ZoteroReviewCollectionList> dp = new DataPortal<ZoteroReviewCollectionList>();
//            dp.FetchCompleted += handler;
//            dp.BeginFetch();
//        }


//#if SILVERLIGHT
//        public MagSearchList() { }
//#else
//        public ZoteroReviewCollectionList() { }
//#endif


//#if SILVERLIGHT
       
//#else


//        protected void DataPortal_Fetch(SingleCriteria<ZoteroReviewCollectionList, long> criteria)
//        {
//            RaiseListChangedEvents = false;
//            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
//            {
//                connection.Open();
//                using (SqlCommand command = new SqlCommand("st_ZoteroReviewConnection", connection))
//                {
//                    command.CommandType = System.Data.CommandType.StoredProcedure;
//                    command.Parameters.Add(new SqlParameter("@ReviewID", criteria.Value));
//                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
//                    {
//                        while (reader.Read())
//                        {
//                            Add(ZoteroReviewCollection.GetZoteroReviewCollection(reader));
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