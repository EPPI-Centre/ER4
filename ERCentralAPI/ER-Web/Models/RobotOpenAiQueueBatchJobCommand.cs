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
    public class RobotOpenAiQueueBatchJobCommand : CommandBase<RobotOpenAiQueueBatchJobCommand>
    {
        public RobotOpenAiQueueBatchJobCommand(){}

        private string _criteria = "";
        private int _creditPurchaseId;
        private int _reviewSetId;
        private bool _onlyCodeInTheRobotName;
        private bool _lockTheCoding;
        private bool _useFullTextDocument;
        private string _result = "";
        private int _jobId = 0;
        private string _RobotName = "";

        public Int64 JobId
        {
            get { return _jobId; }
        }

        public string Result
        {
            get { return _result; }
        }

        public RobotOpenAiQueueBatchJobCommand(string robotName, string criteria, int creditPurchaseId, int reviewSetId, bool onlyCodeInTheRobotName, bool lockTheCoding, bool useFullTextDocument)
        {
            _RobotName = robotName;
            _criteria = criteria;
            _creditPurchaseId = creditPurchaseId;
            _reviewSetId = reviewSetId;
            _onlyCodeInTheRobotName = onlyCodeInTheRobotName;
            _lockTheCoding = lockTheCoding;
            _useFullTextDocument = useFullTextDocument;
        }

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_criteria", _criteria);
            info.AddValue("_creditPurchaseId", _creditPurchaseId);
            info.AddValue("_reviewSetId", _reviewSetId);
            info.AddValue("_onlyCodeInTheRobotName", _onlyCodeInTheRobotName);
            info.AddValue("_lockTheCoding", _lockTheCoding);
            info.AddValue("_useFullTextDocument", _useFullTextDocument); 
            info.AddValue("_jobId", _jobId);
            info.AddValue("_result", _result);
            info.AddValue("_RobotName", _RobotName);
        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            _criteria = info.GetValue<string>("_criteria");
            _creditPurchaseId = info.GetValue<int>("_creditPurchaseId");
            _reviewSetId = info.GetValue<int>("_reviewSetId");
            _onlyCodeInTheRobotName = info.GetValue<bool>("_onlyCodeInTheRobotName");
            _lockTheCoding = info.GetValue<bool>("_lockTheCoding");
            _useFullTextDocument = info.GetValue<bool>("_useFullTextDocument"); 
            _jobId = info.GetValue<int>("_jobId");
            _result = info.GetValue<string>("_result");
            _RobotName = info.GetValue<string>("_RobotName");
        }


#if !SILVERLIGHT

        protected override void DataPortal_Execute()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_RobotApiCreateQueuedJob", connection))
                {
                    ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@ROBOT_NAME", _RobotName));
                    command.Parameters.Add(new SqlParameter("@CRITERIA", _criteria));
                    command.Parameters.Add(new SqlParameter("@CREDIT_PURCHASE_ID", _creditPurchaseId));
                    command.Parameters.Add(new SqlParameter("@REVIEW_SET_ID", _reviewSetId));
                    command.Parameters.Add(new SqlParameter("@FORCE_CODING_IN_ROBOT_NAME", _onlyCodeInTheRobotName));
                    command.Parameters.Add(new SqlParameter("@LOCK_CODING", _lockTheCoding));
                    command.Parameters.Add(new SqlParameter("@USE_PDFS", _useFullTextDocument)); 
                    command.Parameters.Add(new SqlParameter("@CONTACT_ID", ri.UserId));
                    command.Parameters.Add(new SqlParameter("@RESULT", System.Data.SqlDbType.VarChar));
                    command.Parameters["@RESULT"].Size = 100;
                    command.Parameters["@RESULT"].Direction = System.Data.ParameterDirection.Output;
                    command.Parameters.Add(new SqlParameter("@ROBOT_API_CALL_ID", System.Data.SqlDbType.Int));
                    command.Parameters["@ROBOT_API_CALL_ID"].Direction = System.Data.ParameterDirection.Output;
                    command.ExecuteNonQuery();

                    if (command.Parameters["@RESULT"].Value.ToString() == "Success")
                    {
                        _result = "Success.";
                        _jobId = (int)command.Parameters["@ROBOT_API_CALL_ID"].Value;
                    }
                    else
                    {
                        _result = "Error. " + command.Parameters["@RESULT"].Value.ToString();
                        _jobId = -1;
                    }
                }
                connection.Close();
            }
        }

#endif
    }
}
