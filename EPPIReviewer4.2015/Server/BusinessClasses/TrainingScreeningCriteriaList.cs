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
    public class TrainingScreeningCriteriaList : DynamicBindingListBase<TrainingScreeningCriteria>
    {
        public static void GetTrainingScreeningCriteriaList(EventHandler<DataPortalResult<TrainingScreeningCriteriaList>> handler)
        {
            DataPortal<TrainingScreeningCriteriaList> dp = new DataPortal<TrainingScreeningCriteriaList>();
            dp.FetchCompleted += handler;
            dp.BeginFetch();
        }
        
        public TrainingScreeningCriteriaList() { }


#if SILVERLIGHT
       
#else
        protected void DataPortal_Fetch()
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            RaiseListChangedEvents = false;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_TrainingScreeningCriteriaList", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        while (reader.Read())
                        {
                            Add(TrainingScreeningCriteria.GetTrainingScreeningCriteria(reader));
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
