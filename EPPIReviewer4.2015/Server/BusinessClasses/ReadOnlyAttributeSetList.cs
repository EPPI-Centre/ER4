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
#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class ReadOnlyAttributeSetList : ReadOnlyListBase<ReadOnlyAttributeSetList, ReadOnlyAttributeSet>
    {
        public static void GetReadOnlyAttributeSetList(int reviewId, EventHandler<DataPortalResult<ReadOnlyAttributeSetList>> handler)
        {
            DataPortal<ReadOnlyAttributeSetList> dp = new DataPortal<ReadOnlyAttributeSetList>();
            dp.FetchCompleted += handler;
            dp.BeginFetch(new SingleCriteria<ReadOnlyAttributeSetList, int>(reviewId));
        }

#if SILVERLIGHT
        public ReadOnlyAttributeSetList() { }
#else
        private ReadOnlyAttributeSetList() { }
#endif

        internal static ReadOnlyAttributeSetList NewReadOnlyAttributeSetList()
        {
            return new ReadOnlyAttributeSetList();
        }

#if SILVERLIGHT
    public static void GetReadOnlyAttributeSetList(EventHandler<DataPortalResult<ReadOnlyAttributeSetList>> handler, int reviewId)
    {
      DataPortal<ReadOnlyAttributeSetList> dp = new DataPortal<ReadOnlyAttributeSetList>();
      dp.FetchCompleted += handler;
      dp.BeginFetch(new SingleCriteria<ReadOnlyAttributeSetList, int>(reviewId));
    }
#else
        protected void DataPortal_Fetch(SingleCriteria<ReadOnlyAttributeSetList, int> criteria)
        {
            RaiseListChangedEvents = false;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_AttributeSet", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", criteria.Value));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        while (reader.Read())
                        {
                            Add(ReadOnlyAttributeSet.GetReadOnlyAttributeSet(reader));
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
