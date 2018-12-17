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
    //public class ReviewStatisticsReviewerList : DynamicBindingListBase<ReviewStatisticsReviewer>
    public class ReviewStatisticsReviewerList : BusinessListBase<ReviewStatisticsReviewerList, ReviewStatisticsReviewer>
    {

        public static void GetReviewStatisticsReviewerList(EventHandler<DataPortalResult<ReviewStatisticsReviewerList>> handler)
        {
            DataPortal<ReviewStatisticsReviewerList> dp = new DataPortal<ReviewStatisticsReviewerList>();
            dp.FetchCompleted += handler;
            dp.BeginFetch();
        }


#if SILVERLIGHT
        public ReviewStatisticsReviewerList() { }
#else
        public ReviewStatisticsReviewerList() { }
#endif


#if SILVERLIGHT
       
#else
        protected void DataPortal_Fetch()
        {
            
        }

        protected void FillList(bool IsCompleted, int SetId)
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            RaiseListChangedEvents = false;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ReviewStatisticsCodeSetsReviewersComplete", connection))
                {
                    if (IsCompleted == false)
                    {
                        command.CommandText = "st_ReviewStatisticsCodeSetsReviewersIncomplete";
                    }
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@SET_ID", SetId));
                    command.CommandTimeout = 300;
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        while (reader.Read())
                        {
                            Add(ReviewStatisticsReviewer.GetReviewStatisticsReviewer(reader, IsCompleted));
                        }
                    }
                }
                connection.Close();
            }
            RaiseListChangedEvents = true;
        }

        public static ReviewStatisticsReviewerList GetReviewStatisticsReviewerList(bool IsCompleted, int SetId)
        {
            ReviewStatisticsReviewerList retval = new ReviewStatisticsReviewerList();
            retval.FillList(IsCompleted, SetId);
            return retval;
        }
        

#endif



    }
}
