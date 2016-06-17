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
    public class TrainingReviewerTermList : DynamicBindingListBase<TrainingReviewerTerm>
    {
        public static void GetTrainingReviewerTermList(EventHandler<DataPortalResult<TrainingReviewerTermList>> handler)
        {
            DataPortal<TrainingReviewerTermList> dp = new DataPortal<TrainingReviewerTermList>();
            dp.FetchCompleted += handler;
            dp.BeginFetch();
        }


#if SILVERLIGHT
        public TrainingReviewerTermList() { }
#else
        private TrainingReviewerTermList() { }
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
                using (SqlCommand command = new SqlCommand("st_TrainingReviewerTermList", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        while (reader.Read())
                        {
                            Add(TrainingReviewerTerm.GetTrainingReviewerTerm(reader));
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
