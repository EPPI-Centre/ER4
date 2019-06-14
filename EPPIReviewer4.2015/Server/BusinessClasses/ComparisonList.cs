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
    public class ComparisonList : DynamicBindingListBase<Comparison>
    {

        public static void GetComparisonList(EventHandler<DataPortalResult<ComparisonList>> handler)
        {
            DataPortal<ComparisonList> dp = new DataPortal<ComparisonList>();
            dp.FetchCompleted += handler;
            dp.BeginFetch();
        }


#if SILVERLIGHT
        public ComparisonList() { }
#else
        public ComparisonList() { }
#endif


#if SILVERLIGHT
       
#else


        protected void DataPortal_Fetch()
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            RaiseListChangedEvents = false;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ComparisonList", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        while (reader.Read())
                        {
                            Add(Comparison.GetComparison(reader));
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
