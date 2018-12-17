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
    //public class ReviewStatisticsCodeSetList : DynamicBindingListBase<ReviewStatisticsCodeSet>
    public class ReviewStatisticsCodeSetList : BusinessListBase<ReviewStatisticsCodeSetList, ReviewStatisticsCodeSet>
    {

        public static void GetReviewStatisticsCodeSetList(bool IsCompleted, EventHandler<DataPortalResult<ReviewStatisticsCodeSetList>> handler)
        {
            DataPortal<ReviewStatisticsCodeSetList> dp = new DataPortal<ReviewStatisticsCodeSetList>();
            dp.FetchCompleted += handler;
            dp.BeginFetch(new SingleCriteria<ReviewStatisticsCodeSetList, bool>(IsCompleted));
        }
        
        public ReviewStatisticsCodeSetList() { }

#if SILVERLIGHT
       
#else
        protected void DataPortal_Fetch(SingleCriteria<ReviewStatisticsCodeSetList, bool> criteria)
        {
            this.FillList(criteria.Value);
        }

        protected void FillList(bool IsCompleted)
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            RaiseListChangedEvents = false;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ReviewStatisticsCodeSetsComplete", connection))
                {
                    if (IsCompleted == false)
                    {
                        command.CommandText = "st_ReviewStatisticsCodeSetsIncomplete";
                    }
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        while (reader.Read())
                        {
                            Add(ReviewStatisticsCodeSet.GetReviewStatisticsCodeSet(reader, IsCompleted));
                        }
                    }
                }
                connection.Close();
            }
            RaiseListChangedEvents = true;
        }

        public static ReviewStatisticsCodeSetList GetReviewStatisticsCodeSetList(bool IsCompleted)
        {
            ReviewStatisticsCodeSetList retVal = new ReviewStatisticsCodeSetList();
            retVal.FillList(IsCompleted);
            return retVal;
        }



#endif



    }
}
