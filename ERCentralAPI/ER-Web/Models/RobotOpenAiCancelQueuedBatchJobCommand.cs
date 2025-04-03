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
using System.ComponentModel;
using Csla.DataPortalClient;
using System.Threading;

#if!SILVERLIGHT
using System.Data.SqlClient;
using BusinessLibrary.Data;
using BusinessLibrary.Security;
#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class RobotOpenAiCancelQueuedBatchJobCommand : CommandBase<RobotOpenAiCancelQueuedBatchJobCommand>
    {
        public RobotOpenAiCancelQueuedBatchJobCommand(){}

        public RobotOpenAiCancelQueuedBatchJobCommand(int jobId)
        {
            _jobId = jobId;
        }

        private bool _success = false;
        private int _jobId = 0;

        public Int64 JobId
        {
            get { return _jobId; }
        }

        public bool Success
        {
            get { return _success; }
        }


        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_jobId", _jobId);
            info.AddValue("_success", _success);
        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            _jobId = info.GetValue<int>("_jobId");
            _success = info.GetValue<bool>("_success");
        }


#if !SILVERLIGHT

        protected override void DataPortal_Execute()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_RobotOpenAiCancelQueuedBatchJob", connection))
                {
                    ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@ROBOT_API_CALL_ID", _jobId)); 
                    command.Parameters.Add(new SqlParameter("@CONTACT_ID", ri.UserId));
                    if (ri.IsSiteAdmin) command.Parameters.Add(new SqlParameter("@isSiteAdmin", true));

                    command.Parameters.Add(new SqlParameter("@ReturnValue", System.Data.SqlDbType.Int));
                    command.Parameters["@ReturnValue"].Direction = System.Data.ParameterDirection.Output;
                    command.ExecuteNonQuery();

                    if (command.Parameters["@ReturnValue"].Value.ToString() != "1")
                    {
                        _success = false;
                    }
                    else
                    {
                        _success = true;
                    }
                }
                connection.Close();
            }
        }

#endif
    }
}
