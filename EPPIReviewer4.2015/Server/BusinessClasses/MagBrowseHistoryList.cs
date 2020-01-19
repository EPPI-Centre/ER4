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
    public class MagBrowseHistoryList : DynamicBindingListBase<MagBrowseHistory>
    {
        public static void GetMagBrowseHistoryList(EventHandler<DataPortalResult<MagBrowseHistoryList>> handler)
        {
            DataPortal<MagBrowseHistoryList> dp = new DataPortal<MagBrowseHistoryList>();
            dp.FetchCompleted += handler;
            dp.BeginFetch();
        }


#if SILVERLIGHT
        public MagBrowseHistoryList() { }
#else
        private MagBrowseHistoryList() { }
#endif


#if SILVERLIGHT
       
#else


        protected void DataPortal_Fetch()
        {
            /* Not implemented yet - we can amend if we decide to save the browse history to the DB
             * 
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            RaiseListChangedEvents = false;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_MagBrowseHistoryList", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@ITEM_ID", criteria.Value));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        while (reader.Read())
                        {
                            Add(MagBrowseHistory.GetMagBrowseHistory(reader));
                        }
                    }
                }
                connection.Close();
            }
            RaiseListChangedEvents = true;
            */
        }
#endif



    }
}
