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
using BusinessLibrary.Security;
#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class ReadOnlyReviewList : ReadOnlyListBase<ReadOnlyReviewList, ReadOnlyReview>
    {
#if SILVERLIGHT
    public ReadOnlyReviewList() { }
#else
        public ReadOnlyReviewList() { }
#endif

        public static void GetReviewList(int contactId, EventHandler<DataPortalResult<ReadOnlyReviewList>> handler)
        {
            DataPortal<ReadOnlyReviewList> dp = new DataPortal<ReadOnlyReviewList>();
            dp.FetchCompleted += handler;
            dp.BeginFetch(new SingleCriteria<ReadOnlyReviewList, int>(contactId));
        }
        public ReadOnlyReview GetReview(int Rid)
        {
            foreach (ReadOnlyReview ror in this)
            {
                if (ror.ReviewId == Rid || ror.ReviewId == -Rid)
                {
                    return ror;
                }
            }
            return null;
        }
#if !SILVERLIGHT

        private void DataPortal_Fetch(SingleCriteria<ReadOnlyReviewList, int> criteria)
        {
#if !CSLA_NETCORE
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
#elif CSLA_NETCORE
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
#endif
            int currentRid = ri.ReviewId;
            RaiseListChangedEvents = false;
            IsReadOnly = false;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ReviewContact", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@CONTACT_ID", criteria.Value));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        while (reader.Read())
                        {
                            Add(ReadOnlyReview.GetReadOnlyReview(reader));
                        }
                    }
                }
                connection.Close();
            }
            if (currentRid != 0 && ri.IsSiteAdmin && GetReview(currentRid) == null)
            {//review is being accessed as siteadmin login to any review, we'll add this review to the list.
                using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand("st_ReviewContactForSiteAdmin", connection))
                    {
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@CONTACT_ID", criteria.Value));
                        command.Parameters.Add(new SqlParameter("@REVIEW_ID", currentRid));
                        using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                        {
                            while (reader.Read())
                            {
                                Add(ReadOnlyReview.GetReadOnlyReview(reader));
                            }
                        }
                    }
                    connection.Close();
                }
            }
            IsReadOnly = true;
            RaiseListChangedEvents = true;
        }

#endif
        }
}
