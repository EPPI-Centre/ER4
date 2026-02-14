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

#if !SILVERLIGHT
using System.Data.SqlClient;
using BusinessLibrary.Data;
using BusinessLibrary.Security;
using BusinessLibrary.BusinessClasses;
#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class RobotOpenAiPromptEvaluationDataList : DynamicBindingListBase<RobotOpenAiPromptEvaluationData>
    {

        public static void GetRobotOpenAiPromptEvaluationDataList(int OpenAiPromptEvaluationId, EventHandler<DataPortalResult<RobotOpenAiPromptEvaluationDataList>> handler)
        {
            DataPortal<RobotOpenAiPromptEvaluationDataList> dp = new DataPortal<RobotOpenAiPromptEvaluationDataList>();
            dp.FetchCompleted += handler;
            dp.BeginFetch(new RobotOpenAiPromptEvaluationDataSelectionCriteria(typeof(RobotOpenAiPromptEvaluationDataList), OpenAiPromptEvaluationId));
        }


#if SILVERLIGHT
        public RobotOpenAiPromptEvaluationDataList() { }
#else
        public RobotOpenAiPromptEvaluationDataList() { }
#endif

#if SILVERLIGHT
       
#else
        protected void DataPortal_Fetch(RobotOpenAiPromptEvaluationDataSelectionCriteria criteria)
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            RaiseListChangedEvents = false;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_RobotOpenAiPromptEvaluationDataList", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@OPENAI_PROMPT_EVALUATION_ID", criteria.OpenAiPromptEvaluationId));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        while (reader.Read())
                        {
                            Add(RobotOpenAiPromptEvaluationData.GetRobotOpenAiPromptEvaluationData(reader));
                        }
                    }
                }
                connection.Close();
            }
            RaiseListChangedEvents = true;
        }

#endif

        [Serializable]
        public class RobotOpenAiPromptEvaluationDataSelectionCriteria : Csla.CriteriaBase<RobotOpenAiPromptEvaluationDataSelectionCriteria>
        {
            private static PropertyInfo<int> OpenAiPromptEvaluationIdProperty = RegisterProperty<int>(typeof(RobotOpenAiPromptEvaluationDataSelectionCriteria), new PropertyInfo<int>("OpenAiPromptEvaluationId", "OpenAiPromptEvaluationId"));
            public int OpenAiPromptEvaluationId
            {
                get { return ReadProperty(OpenAiPromptEvaluationIdProperty); }
            }
            public RobotOpenAiPromptEvaluationDataSelectionCriteria(Type type, int openAiPromptEvaluationId)
            {
                LoadProperty(OpenAiPromptEvaluationIdProperty, openAiPromptEvaluationId);
            }
            public RobotOpenAiPromptEvaluationDataSelectionCriteria() { }
        }
    }
}
