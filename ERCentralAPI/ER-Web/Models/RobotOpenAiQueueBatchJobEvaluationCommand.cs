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
using static BusinessLibrary.BusinessClasses.ImportJsonCommand;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Data;
using Csla.Rules.CommonRules;


#if !SILVERLIGHT
using System.Data.SqlClient;
using BusinessLibrary.Data;
using BusinessLibrary.Security;
using System.Text.RegularExpressions;
#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class RobotOpenAiQueueBatchJobEvaluationCommand : CommandBase<RobotOpenAiQueueBatchJobEvaluationCommand>
    {
        public RobotOpenAiQueueBatchJobEvaluationCommand() { }

        private int _creditPurchaseId;
        private int _reviewSetId;
        private bool _useFullTextDocument;
        private string _result = "";
        private int _jobId = 0;
        private string _RobotName = "";
        private int _n_iterations = 0;
        private int _n_codes = 0;
        private string _attributeIdsWithPrompts = "";
        private string _reviewSetHtml = "";
        private string _evaluationName = "";
        private Int64 _gold_standard_attribute_id = 0;
        private string _gold_standard_attribute_name = "";

        public Int64 JobId
        {
            get { return _jobId; }
        }

        public string Result
        {
            get { return _result; }
        }

        public RobotOpenAiQueueBatchJobEvaluationCommand(string evaluationName, string robotName, int creditPurchaseId, int reviewSetId,
            string reviewSetHtml, Int64 goldStandardAttributeId, string goldStandardAttributeName, 
            bool useFullTextDocument, int n_iterations, int n_codes, string attributeIdsWithPrompts)
        {
            _evaluationName = evaluationName;
            _RobotName = robotName;
            _creditPurchaseId = creditPurchaseId;
            _reviewSetId = reviewSetId;
            _reviewSetHtml = reviewSetHtml;
            _gold_standard_attribute_id = goldStandardAttributeId;
            _gold_standard_attribute_name = goldStandardAttributeName;
            _useFullTextDocument = useFullTextDocument;
            _n_iterations = n_iterations;
            _n_codes = n_codes;
            _attributeIdsWithPrompts = attributeIdsWithPrompts; 
        }

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_evaluationName", _evaluationName);
            info.AddValue("_creditPurchaseId", _creditPurchaseId);
            info.AddValue("_reviewSetId", _reviewSetId);
            info.AddValue("_gold_standard_attribute_id", _gold_standard_attribute_id);
            info.AddValue("_gold_standard_attribute_name", _gold_standard_attribute_name);
            info.AddValue("_useFullTextDocument", _useFullTextDocument);
            info.AddValue("_jobId", _jobId);
            info.AddValue("_result", _result);
            info.AddValue("_RobotName", _RobotName);
            info.AddValue("_n_iterations", _n_iterations);
            info.AddValue("_n_codes", _n_codes);
            info.AddValue("_attributeIdsWithPrompts", _attributeIdsWithPrompts); 
            info.AddValue("_reviewSetHtml", _reviewSetHtml); 

        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            _creditPurchaseId = info.GetValue<int>("_creditPurchaseId");
            _reviewSetId = info.GetValue<int>("_reviewSetId");
            _useFullTextDocument = info.GetValue<bool>("_useFullTextDocument");
            _jobId = info.GetValue<int>("_jobId");
            _result = info.GetValue<string>("_result");
            _RobotName = info.GetValue<string>("_RobotName");
            _n_iterations = info.GetValue<int>("_n_iterations");
            _n_codes = info.GetValue<int>("_n_codes");
            _attributeIdsWithPrompts = info.GetValue<string>("_attributeIdsWithPrompts"); 
            _reviewSetHtml = info.GetValue<string>("_reviewSetHtml");
            _evaluationName = info.GetValue<string>("_evaluationName");
            _gold_standard_attribute_id = info.GetValue<Int64>("_gold_standard_attribute_id");
            _gold_standard_attribute_name = info.GetValue<string>("_gold_standard_attribute_name");
        }


#if !SILVERLIGHT

        protected override void DataPortal_Execute()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                int NewOpenAiPromptEvaluationId = 0;
                string itemList = "";

                connection.Open();
                // get list of item_ids for 'criteria' field
                using (SqlCommand command = new SqlCommand("st_ItemIDPerAttributeID", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@ReviewId", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@AttributeId", _gold_standard_attribute_id));
                    command.Parameters.Add(new SqlParameter("@Result", System.Data.SqlDbType.NVarChar, -1));
                    command.Parameters["@Result"].Direction = System.Data.ParameterDirection.Output;
                    command.ExecuteNonQuery();

                    itemList = (string)command.Parameters["@RESULT"].Value;
                }

                if (itemList == "")
                {
                    _result = "Error. No items found";
                    _jobId = -1;
                    return;
                }

                // create record in TB_OPENAI_PROMPT_EVALUATION
                using (SqlCommand command = new SqlCommand("st_RobotOpenAIPromptEvaluationCreate", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@TITLE", _evaluationName));
                    command.Parameters.Add(new SqlParameter("@ROBOT_NAME", _RobotName));
                    command.Parameters.Add(new SqlParameter("@REVIEW_SET_ID", _reviewSetId));
                    command.Parameters.Add(new SqlParameter("@GOLD_STANDARD_ATTRIBUTE_ID", _gold_standard_attribute_id));
                    command.Parameters.Add(new SqlParameter("@GOLD_STANDARD_ATTRIBUTE_NAME", _gold_standard_attribute_name));
                    command.Parameters.Add(new SqlParameter("@N_RECORDS", itemList.Count(c => c == ',') + 1));
                    command.Parameters.Add(new SqlParameter("@N_CODES", _n_codes));
                    command.Parameters.Add(new SqlParameter("@REVIEW_SET_HTML", _reviewSetHtml));
                    command.Parameters.Add(new SqlParameter("@ATTRIBUTES_WITH_PROMPTS", _attributeIdsWithPrompts));
                    command.Parameters.Add(new SqlParameter("@USE_PDFS", _useFullTextDocument));
                    command.Parameters.Add(new SqlParameter("@CONTACT_ID", ri.UserId));
                    command.Parameters.Add(new SqlParameter("@CONTACT_NAME", ri.Name));
                    command.Parameters.Add(new SqlParameter("@N_ITERATIONS", _n_iterations));
                    command.Parameters.Add(new SqlParameter("@NEW_OPENAI_PROMPT_EVALUATION_ID", System.Data.SqlDbType.Int));
                    command.Parameters["@NEW_OPENAI_PROMPT_EVALUATION_ID"].Direction = System.Data.ParameterDirection.Output;
                    command.ExecuteNonQuery();

                    NewOpenAiPromptEvaluationId = (int)command.Parameters["@NEW_OPENAI_PROMPT_EVALUATION_ID"].Value;
                }

                using (SqlCommand command = new SqlCommand("st_RobotApiCreateQueuedJob", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@ROBOT_NAME", _RobotName));
                    command.Parameters.Add(new SqlParameter("@CRITERIA", "ItemIds: " + itemList));
                    command.Parameters.Add(new SqlParameter("@CREDIT_PURCHASE_ID", _creditPurchaseId));
                    command.Parameters.Add(new SqlParameter("@REVIEW_SET_ID", _reviewSetId));
                    command.Parameters.Add(new SqlParameter("@FORCE_CODING_IN_ROBOT_NAME", false));
                    command.Parameters.Add(new SqlParameter("@LOCK_CODING", false));
                    command.Parameters.Add(new SqlParameter("@USE_PDFS", _useFullTextDocument));
                    command.Parameters.Add(new SqlParameter("@CONTACT_ID", ri.UserId));
                    command.Parameters.Add(new SqlParameter("@N_ITERATIONS", _n_iterations));
                    command.Parameters.Add(new SqlParameter("@OPENAI_PROMPT_EVALUATION_ID", NewOpenAiPromptEvaluationId));
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
