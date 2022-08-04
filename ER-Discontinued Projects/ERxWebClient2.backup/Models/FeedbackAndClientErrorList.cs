using Csla;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
#if!SILVERLIGHT
using System.Data.SqlClient;
using BusinessLibrary.Data;
using BusinessLibrary.Security;
#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class FeedbackAndClientErrorList : ReadOnlyListBase<FeedbackAndClientErrorList, FeedbackAndClientError>
    {
        public FeedbackAndClientErrorList() { }

#if !SILVERLIGHT
        private void DataPortal_Fetch()
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            //we'll check that user is fully logged on and is a site admin
            if (ri.ReviewId == 0 || !ri.IsSiteAdmin) throw new UnauthorizedAccessException("FeedbackAndClientErrorList can only be fetched by fully logged on Admins");

            RaiseListChangedEvents = false;
            IsReadOnly = false;
            using (SqlConnection connection = new SqlConnection(DataConnection.AdmConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_OnlineFeedbackList", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        while (reader.Read())
                        {
                            Add(FeedbackAndClientError.GetFeedbackAndClientError(reader));
                        }
                    }
                }
                connection.Close();
            }
            IsReadOnly = true;
            RaiseListChangedEvents = true;
        }
#endif
    }
}
