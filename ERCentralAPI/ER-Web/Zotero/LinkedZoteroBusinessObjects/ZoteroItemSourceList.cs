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
    public class ZoteroItemSourceList : DynamicBindingListBase<ZoteroItemSource>
    {

        public static void GetZoteroSourceList(EventHandler<DataPortalResult<ZoteroItemSourceList>> handler)
        {
            DataPortal<ZoteroItemSourceList> dp = new DataPortal<ZoteroItemSourceList>();
            dp.FetchCompleted += handler;
            dp.BeginFetch();
        }


#if SILVERLIGHT
        public MagSearchList() { }
#else
        public ZoteroItemSourceList() { }
#endif


#if SILVERLIGHT
       
#else


        protected void DataPortal_Fetch(SingleCriteria<ZoteroItemSourceList, Int32> criteria)
        {
            RaiseListChangedEvents = false;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ZoteroReviewItemIdsPerSource", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@SOURCE_ID", criteria.Value));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        while (reader.Read())
                        {
                            Add(ZoteroItemSource.GetZoteroItemReviewIdsPerSource(reader));
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