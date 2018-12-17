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
    public class ItemDuplicateList : DynamicBindingListBase<ItemDuplicate>
    {

        public static void GetItemDuplicateList(bool GetNewDuplicates, EventHandler<DataPortalResult<ItemDuplicateList>> handler)
        {
            DataPortal<ItemDuplicateList> dp = new DataPortal<ItemDuplicateList>();
            dp.FetchCompleted += handler;
            dp.BeginFetch(new SingleCriteria<ItemDuplicateList, bool>(GetNewDuplicates));
        }


#if SILVERLIGHT
        public ItemDuplicateList() { }
        
#else
        private ItemDuplicateList() { }
        public void ClearList()
        {
            this.Clear();
        }
#endif


#if SILVERLIGHT
       
#else


        protected void DataPortal_Fetch(SingleCriteria<ItemDuplicateList, bool> criteria)
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            RaiseListChangedEvents = false;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ItemDuplicatesCheckOngoing", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add("@revID", System.Data.SqlDbType.Int);
                    command.Parameters["@revID"].Value =  ri.ReviewId;
                    command.Parameters.Add("@RETURN_VALUE", System.Data.SqlDbType.Int);
                    command.Parameters["@RETURN_VALUE"].Direction = System.Data.ParameterDirection.ReturnValue;
                     command.ExecuteNonQuery();
                     if (command.Parameters["@RETURN_VALUE"].Value.ToString() == "-2")
                    { //make a mock return list and return
                        this.Clear();
                        throw new DataPortalException("Execution still Running", this);
                    }
                }
                if (criteria.Value == true)
                {
                    FindNewDuplicates(connection);
                    if (this.Count == 1)
                    {
                        //RaiseListChangedEvents = true;
                        this.Clear();
                        
                        throw new DataPortalException("Execution still Running", this);
                    }
                }
                using (SqlCommand command = new SqlCommand("st_ItemDuplicatesList", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        while (reader.Read())
                        {
                            Add(ItemDuplicate.GetItemDuplicate(reader));
                        }
                    }
                }
                connection.Close();
            }
            RaiseListChangedEvents = true;
        }

        protected void FindNewDuplicates(SqlConnection connection)
        {
            // At the moment, this doesn't find duplicates at all - but it will soon!
            using (SqlCommand command = new SqlCommand("st_ItemDuplicatesInsert", connection))
            {
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                command.CommandType = System.Data.CommandType.StoredProcedure;
                command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                command.CommandTimeout = 300;
                try
                {
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    //if ((ex as System.Data.SqlClient.SqlException)
                    //make a mock return list and return
                    this.Clear();
                    this.Add(ItemDuplicate.MakeWaitingResult());
                    return;
                }
            }
        }

#endif



    }
}
