using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Csla;
using Csla.Security;
using Csla.Core;
using Csla.Serialization;
using Csla.Silverlight;
using Azure.Storage.Sas;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using static Csla.Security.MembershipIdentity;
using Csla.Data;




//using Csla.Validation;

#if !SILVERLIGHT
using System.Data.SqlClient;
using BusinessLibrary.Data;
using BusinessLibrary.Security;
using BusinessLibrary.BusinessClasses;
#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class RobotOpenAiPromptEvaluationList : DynamicBindingListBase<RobotOpenAiPromptEvaluation>
    {

        public static void GetRobotOpenAiPromptEvaluationList(string SimulationName, EventHandler<DataPortalResult<RobotOpenAiPromptEvaluationList>> handler)
        {
            DataPortal<RobotOpenAiPromptEvaluationList> dp = new DataPortal<RobotOpenAiPromptEvaluationList>();
            dp.FetchCompleted += handler;
            dp.BeginFetch();
        }


#if SILVERLIGHT
        public RobotOpenAiPromptEvaluationList() { }
#else
        public RobotOpenAiPromptEvaluationList() { }
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
                using (SqlCommand command = new SqlCommand("st_RobotOpenAIPromptEvaluationList", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@ReviewId", ri.ReviewId));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        while (reader.Read())
                        {
                            Add(RobotOpenAiPromptEvaluation.GetRobotOpenAiPromptEvaluation(reader));
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
