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
    public class ScreeningFromSearchIterationList : DynamicBindingListBase<ScreeningFromSearchIteration>
    {
        public int LastSearchId
        {
            get
            {
                if (Count == 0) return -1;
                else
                {
                    return this[Count - 1].SearchId;
                }
            }
        } 
        public List<int> AllSearchIds
        {
            get
            {
                List<int> res = new List<int>();
                foreach (ScreeningFromSearchIteration sfsi in this)
                {
                    if (!res.Contains(sfsi.SearchId)) res.Add(sfsi.SearchId);
                }
                return res;
            }
        }
        
        public ScreeningFromSearchIterationList() { }

#if !SILVERLIGHT
        protected void DataPortal_Fetch()
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            RaiseListChangedEvents = false;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_TrainingFromSearchList", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        while (reader.Read())
                        {
                            if (reader.GetInt32("ITERATION") != 0) // so ignoring failed iterations, and those currently in progress
                                Add(ScreeningFromSearchIteration.GetTraining(reader));
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
