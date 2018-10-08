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
    public class ItemDocumentList : DynamicBindingListBase<ItemDocument>
    {

        public static void GetItemDocumentList(Int64 ItemId, EventHandler<DataPortalResult<ItemDocumentList>> handler)
        {
            DataPortal<ItemDocumentList> dp = new DataPortal<ItemDocumentList>();
            dp.FetchCompleted += handler;
            dp.BeginFetch(new SingleCriteria<ItemDocumentList, Int64>(ItemId));
        }


#if SILVERLIGHT
        public ItemDocumentList() { }
#else
        public ItemDocumentList() { }
#endif


#if SILVERLIGHT
       
#else


        protected void DataPortal_Fetch(SingleCriteria<ItemDocumentList, Int64> criteria)
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            RaiseListChangedEvents = false;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ItemDocumentList", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@ITEM_ID", criteria.Value));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        while (reader.Read())
                        {
                            Add(ItemDocument.GetItemDocument(reader));
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
