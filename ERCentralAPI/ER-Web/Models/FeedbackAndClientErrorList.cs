using BusinessLibrary.Security;
using Csla;
using Csla.Serialization;
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
        public static void GetFeedbackAndClientErrorList(EventHandler<DataPortalResult<FeedbackAndClientErrorList>> handler)
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            if (ri != null && ri.IsAuthenticated)
            {
                DataPortal<FeedbackAndClientErrorList> dp = new DataPortal<FeedbackAndClientErrorList>();
                dp.FetchCompleted += handler;
                dp.BeginFetch(new SingleCriteria<FeedbackAndClientErrorList, int>(ri.UserId));
            }
        }
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

        private void DataPortal_Fetch(SingleCriteria<FeedbackAndClientErrorList, int> criteria)
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            //we'll check that user is the expected user!
            if (!ri.IsAuthenticated || ri.UserId != criteria.Value) throw new UnauthorizedAccessException("FeedbackAndClientErrorList can only be fetched for the currently logged on user");

            RaiseListChangedEvents = false;
            IsReadOnly = false;
            using (SqlConnection connection = new SqlConnection(DataConnection.AdmConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_OnlineFeedbackListByUser", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@CONTACT_ID", ri.UserId));
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
