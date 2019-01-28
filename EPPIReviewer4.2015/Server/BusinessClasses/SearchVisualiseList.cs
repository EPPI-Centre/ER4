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
    public class SearchVisualiseList : DynamicBindingListBase<SearchVisualise>
    //public class SearchVisualiseList : BusinessListBase<SearchVisualiseList, SearchVisualise>
    {

        public static void GetSearchVisualiseList(int SearchId, EventHandler<DataPortalResult<SearchVisualiseList>> handler)
        {
            DataPortal<SearchVisualiseList> dp = new DataPortal<SearchVisualiseList>();
            dp.FetchCompleted += handler;
            dp.BeginFetch(new SingleCriteria<SearchVisualiseList, int>(SearchId));
        }


#if SILVERLIGHT
        public SearchVisualiseList() { }
#else
        public SearchVisualiseList() { }
#endif


#if SILVERLIGHT
       
#else
        protected void DataPortal_Fetch(SingleCriteria<SearchVisualiseList, int> criteria)
        {
            RaiseListChangedEvents = false;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_SearchVisualise", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@SEARCH_ID", criteria.Value));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        while (reader.Read())
                        {
                            Add(SearchVisualise.GetSearchVisualise(reader));
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
