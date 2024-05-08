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
using System.Threading.Tasks;
using Csla.Data;
using System.IO;
using Newtonsoft.Json.Linq;
using static Csla.Security.MembershipIdentity;



#if !SILVERLIGHT
using System.Data.SqlClient;
using BusinessLibrary.Data;
using BusinessLibrary.Security;
using System.Configuration;
using System.Data;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;

#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class RobotOpenAICommand : CommandBase<RobotOpenAICommand>
    {

        public RobotOpenAICommand() { }
        private int _reviewSetId;
        private Int64 _itemDocumentId;
        private Int64 _itemId;
        private string _message = "";
        private bool _isLastInBatch = true;
        private int _jobId = 0;
        private int _robotContactId = 0;
        private bool _onlyCodeInTheRobotName = true;
        private bool _lockTheCoding = true;

        public string ReturnMessage
        {
            get { return _message; }
        }
        public int RobotContactId { get { return _robotContactId; } }
        public RobotOpenAICommand(int reviewSetId, Int64 itemId, Int64 itemDocumentId, bool onlyCodeInTheRobotName = true, bool lockTheCoding = true)
        {
            _reviewSetId = reviewSetId;
            _itemId = itemId;
            _itemDocumentId = itemDocumentId;
            _message = "";
            _onlyCodeInTheRobotName = onlyCodeInTheRobotName;
            _lockTheCoding = lockTheCoding;
        }
        public RobotOpenAICommand(int reviewSetId, Int64 itemId, Int64 itemDocumentId, bool isLastInBatch, int JobId, int robotContactId, bool onlyCodeInTheRobotName = true, bool lockTheCoding = true)
        {
            _reviewSetId = reviewSetId;
            _itemId = itemId;
            _itemDocumentId = itemDocumentId;
            _message = "";
            _isLastInBatch = isLastInBatch;
            _jobId = JobId;
            _robotContactId = robotContactId;
            _onlyCodeInTheRobotName = onlyCodeInTheRobotName;
            _lockTheCoding = lockTheCoding;
        }

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_reviewSetId", _reviewSetId);
            info.AddValue("_itemId", _itemId);
            info.AddValue("_itemDocumentId", _itemDocumentId);
            info.AddValue("_message", _message);
            info.AddValue("_isLastInBatch", _isLastInBatch);
            info.AddValue("_jobId", _jobId);
            info.AddValue("_robotContactId", _robotContactId);
            info.AddValue("_onlyCodeInTheRobotName", _onlyCodeInTheRobotName);
            info.AddValue("_lockTheCoding", _lockTheCoding);
        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, StateMode mode)
        {
            _reviewSetId = info.GetValue<int>("_reviewSetId");
            _itemId = info.GetValue<Int64>("_itemId");
            _itemDocumentId = info.GetValue<Int64>("_itemDocumentId");
            _message = info.GetValue<string>("_message");
            _isLastInBatch = info.GetValue<bool>("_isLastInBatch");
            _jobId = info.GetValue<int>("_jobId");
            _robotContactId = info.GetValue<int>("_robotContactId");
            _onlyCodeInTheRobotName = info.GetValue<bool>("_onlyCodeInTheRobotName");
            _lockTheCoding = info.GetValue<bool>("_lockTheCoding");
        }


#if !SILVERLIGHT

        private int _inputTokens = 0;
        private int _outputTokens = 0;
        private Int64 _Item_set_id = 0;
        private bool _CodingIsFinal = false;
        private bool hasSavedSomeCodes = false;
        private int errors = 0;

        protected override void DataPortal_Execute()
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            ReviewInfo rInfo = DataPortal.Fetch<ReviewInfo>();
            if (!rInfo.CanUseRobots)
            {
                _message = "Failed: GPT4 is disabled or there is no credit.";
                return;
            }
            else
            {
                if (_jobId == 0)
                {//we need to create a record for this in TB_ROBOT_API_CALL_LOG - it's a single call doing one item!
                    CreditForRobots CfR = rInfo.CreditForRobotsList.FirstOrDefault(f => f.AmountRemaining > 0.0001);
                    int creditPurchaseId = 0;
                    if (CfR == null && rInfo.OpenAIEnabled == false)
                    {
                        _message = "Failed: did not find suitable credit to use.";
                        return;
                    }
                    else if (CfR != null)
                    {
                        creditPurchaseId = CfR.CreditPurchaseId;
                    }
                    using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand("st_CreateRobotApiCallLog", connection))
                        {
                            command.CommandType = System.Data.CommandType.StoredProcedure;
                            command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                            command.Parameters.Add(new SqlParameter("@CREDIT_PURCHASE_ID", creditPurchaseId));
                            command.Parameters.Add(new SqlParameter("@ROBOT_NAME", "OpenAI GPT4"));
                            command.Parameters.Add(new SqlParameter("@CRITERIA", "ItemIds: " + _itemId));
                            command.Parameters.Add(new SqlParameter("@REVIEW_SET_ID", _reviewSetId));
                            command.Parameters.Add(new SqlParameter("@CURRENT_ITEM_ID", _itemId));
                            command.Parameters.Add(new SqlParameter("@FORCE_CODING_IN_ROBOT_NAME", _onlyCodeInTheRobotName));
                            command.Parameters.Add(new SqlParameter("@LOCK_CODING", _lockTheCoding));
                            command.Parameters.Add(new SqlParameter("@result", SqlDbType.VarChar));
                            command.Parameters["@result"].Size = 50;
                            command.Parameters["@result"].Direction = System.Data.ParameterDirection.Output;
                            command.Parameters.Add(new SqlParameter("@JobId", SqlDbType.Int));
                            command.Parameters["@JobId"].Direction = System.Data.ParameterDirection.Output;
                            command.Parameters.Add(new SqlParameter("@RobotContactId", SqlDbType.Int));
                            command.Parameters["@RobotContactId"].Direction = System.Data.ParameterDirection.Output;
                            command.ExecuteNonQuery();
                            if (command.Parameters["@result"].Value.ToString() != "Success")
                            {
                                _message = "Failure. " + command.Parameters["@result"].Value.ToString();
                                return;
                            }
                            _jobId = (int)command.Parameters["@JobId"].Value;
                            _robotContactId = (int)command.Parameters["@RobotContactId"].Value;
                        }
                    }
                }
                //we have a jobID so now we can (and want) to catch and log exceptions
                try
                {
                    bool result = Task.Run(() => DoRobot(ri.ReviewId, _robotContactId)).GetAwaiter().GetResult();
                    if (errors > 0)
                    {
                        _message += Environment.NewLine + "Error(s) occurred. Could not save " + errors.ToString() + " code(s).";
                        //TODO: also check if we need to delete a newly created (but empty) record in TB_ITEM_SET
                        if (hasSavedSomeCodes == false && _Item_set_id > 0)
                        {
                            //not yet implemented!
                            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
                            {
                                connection.Open();
                                using (SqlCommand command = new SqlCommand("st_ItemSetDeleteIfEmpty", connection))
                                {
                                    command.CommandType = System.Data.CommandType.StoredProcedure;
                                    command.Parameters.Add(new SqlParameter("@ITEM_ID", _itemId));
                                    command.Parameters.Add(new SqlParameter("@ITEM_SET_ID", _Item_set_id));
                                    command.ExecuteNonQuery();
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    _message = "Failure. " + Environment.NewLine + e.Message;
                    using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
                    {
                        string SavedMsg = e.Message;
                        if (SavedMsg.Length > 200) SavedMsg = SavedMsg.Substring(0, 200);
                        connection.Open();
                        using (SqlCommand command = new SqlCommand("st_UpdateRobotApiCallLog", connection))
                        {//this is to update the token numbers, and thus the cost, if we can
                            command.Parameters.Add(new SqlParameter("@REVIEW_ID ", ri.ReviewId));
                            command.Parameters.Add(new SqlParameter("@ROBOT_API_CALL_ID", _jobId));
                            command.Parameters.Add(new SqlParameter("@STATUS", _isLastInBatch ? "Failed" : "Running"));
                            command.Parameters.Add(new SqlParameter("@CURRENT_ITEM_ID", _itemId));
                            command.Parameters.Add(new SqlParameter("@INPUT_TOKENS_COUNT", _inputTokens));
                            command.Parameters.Add(new SqlParameter("@OUTPUT_TOKENS_COUNT", _outputTokens));
                            command.Parameters.Add(new SqlParameter("@ERROR_MESSAGE", SavedMsg));
                            command.Parameters.Add(new SqlParameter("@STACK_TRACE", e.StackTrace));
                            command.ExecuteNonQuery();
                        }
                        
                    }
                    return;
                }
                
                //no point trying to log exceptions happening when we're only updating the call log, likely to fail too!
                using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand("st_UpdateRobotApiCallLog", connection))
                    {
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                        command.Parameters.Add(new SqlParameter("@ROBOT_API_CALL_ID", _jobId));
                        command.Parameters.Add(new SqlParameter("@STATUS", _isLastInBatch ? "Finished" : "Running"));
                        command.Parameters.Add(new SqlParameter("@CURRENT_ITEM_ID", _itemId));
                        command.Parameters.Add(new SqlParameter("@INPUT_TOKENS_COUNT", _inputTokens));
                        command.Parameters.Add(new SqlParameter("@OUTPUT_TOKENS_COUNT", _outputTokens));
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        private class OpenAIChatClass
        {
            public string role { get; set; }
            public string content { get; set; }
        }

        private string getPrompts(string currentPrompt, AttributeSet aSet)
        {
            foreach (AttributeSet subSet in aSet.Attributes)
            {
                currentPrompt = getPrompts(currentPrompt, subSet);
            }
            string possiblePrompt = aSet.AttributeSetDescription;
            if (possiblePrompt.IndexOf(':') > 1 && possiblePrompt.IndexOf("//") > possiblePrompt.IndexOf(':')) // checking it's in the right format
            {
                possiblePrompt = possiblePrompt.Replace("'", "").Replace(",", "").Replace("{", "").Replace("}",""); // once out of Alpha we could make this more complete
                currentPrompt += possiblePrompt + ",\n";
                return currentPrompt;
            }
            else
            {
                return currentPrompt;
            }
        }

        private AttributeSet getAttributeFromPromptKey(string key, AttributeSet aSet)
        {
            int columnIndex = aSet.AttributeSetDescription.IndexOf(':');
            if (columnIndex > 0)
            {
                string label = aSet.AttributeSetDescription.Substring(0, columnIndex);
                if (label == key)
                {
                    return aSet;
                }
            }
            foreach (AttributeSet subSet in aSet.Attributes)
            {
                AttributeSet possibleMatch = getAttributeFromPromptKey(key, subSet);
                if (possibleMatch != null)
                {
                    return possibleMatch;
                }
            }
            return null;
        }
        private async Task<bool> DoRobot(int ReviewId, int UserId)
        {
            bool result = true;            

            string endpoint = AzureSettings.RobotOpenAIEndpoint;
            string key = AzureSettings.RobotOpenAIKey2;
            //string document = GetDoc(_itemDocumentId, ReviewId);  // when we re-enable this, we need to check the stored proc (below) will return the text


            // *** Get item and check that it has an abstract
            Item i = Item.GetItemById(_itemId, ReviewId);
            if (i == null)
            {
                _message = "Null item";
                return false;
            }
            if (i.Abstract.Trim().Length + i.Title.Trim().Length < 50)
            {
                _message = "Short or non-existent title and abstract";
                return false;
            }
            char[] chars = { ' ' };
            int wordCount = i.Abstract.Split(chars, StringSplitOptions.RemoveEmptyEntries).Length + i.Title.Split(chars, StringSplitOptions.RemoveEmptyEntries).Length;
            if (wordCount > 3500)
            {
                _message = "Maximum word count is currently 3500 words. This title+abstract is " + wordCount.ToString() + " words long.";
                return false;
            }
            

            // *** Get the codeset, build the list of prompts, and return if no valid prompts are present
            ReviewSet rs = null;
            rs = ReviewSet.GetReviewSet(_reviewSetId);
            if (rs == null)
            {
                _message = "Error: could not get code set";
                return false;
            }

            string prompt = "";
            foreach (AttributeSet subSet in rs.Attributes)
            { //maybe add code to check if we have 2 codes with the same prompt, which would break the "saving results" phase, somewhat (only the first will be coded)
                string newPrompt = getPrompts("", subSet);
                if (newPrompt != "")
                {
                    prompt += newPrompt;
                }
            }

            if (prompt == "")
            {
                _message = "Error: No valid prompts in codeset";
                return false;
            }


            // *** Create the prompt for the LLM

            bool hastitle = true;
            bool hasabstract = true;
            if (i.Title.Trim().Length <= 1) { hastitle = false; }
            if (i.Abstract.Trim().Length <= 1) { hasabstract = false; }
            string userprompt = "";
            string sysprompt = "You extract data from the text provided below into a JSON object of the shape provided below. If the data is not in the text return 'false' for that field. \nShape: {" + prompt + "}";
            if (hasabstract && hastitle) 
            {
                sysprompt = "You extract data from the title and text provided below into a JSON object of the shape provided below. If the data is not in the text return 'false' for that field. \nShape: {" + prompt + "}";
                userprompt = "Title: " + i.Title + "\nText: " + i.Abstract;
            }
            else if (hastitle == true && hasabstract == false)
            {
                userprompt = "Text: " + i.Title;
            }
            else if (hastitle == false && hasabstract == true)
            {
                userprompt = "Text: " + i.Abstract;
            }
            List<OpenAIChatClass> messages = new List<OpenAIChatClass>
            {
                new OpenAIChatClass { role = "system", content = sysprompt}, // {participants: number // total number of participants,\n arm_count: string // number of study arms,\n intervention: string // description of intervention,\n comparison: string // description of comparison }" },
                new OpenAIChatClass { role = "user", content = userprompt},
            };


            // *** additional params (modifiable in web.config)
            double temperature = Convert.ToDouble(AzureSettings.RobotOpenAITemperature);
            int frequency_penalty = Convert.ToInt16(AzureSettings.RobotOpenAIFrequencyPenalty);
            int presence_penalty = Convert.ToInt16(AzureSettings.RobotOpenAIPresencePenalty);
            double top_p = Convert.ToDouble(AzureSettings.RobotOpenAITopP);


            // *** Create the client and submit the request to the LLM
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("api-key", $"{key}");
            var requestBody = new { messages, temperature, frequency_penalty, presence_penalty, top_p};
            var json = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await client.PostAsync(endpoint, content);
            if (response.IsSuccessStatusCode == false)
            {
                _message = "Error: " + response.ReasonPhrase;
                result = false;
                return result;
            }
            var responseString = await response.Content.ReadAsStringAsync();
            var generatedText = Newtonsoft.Json.JsonConvert.DeserializeObject<OpenAIResult>(responseString);
            var responses = generatedText.choices[0].message.content;
            var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(responses);


            // *** Go through each of the responses, find the right code, and save results
            foreach (var kv in dict)
            {
                AttributeSet matched = null;
                foreach (AttributeSet subSet in rs.Attributes)
                {
                    matched = getAttributeFromPromptKey(kv.Key, subSet);
                    if (matched != null)
                    {
                        SaveAttribute(matched, _itemId, kv.Value, ReviewId, UserId);
                        break;
                    }
                }
                    
            }
            if (_Item_set_id > 0 && _onlyCodeInTheRobotName == false && _lockTheCoding == true)
            {//_Item_set_id > 0 => there is coding to lock/unlock
                //_onlyCodeInTheRobotName == false && _lockTheCoding == true => we did not call st_ItemSetPrepareForRobot so we may need to lock the coding still
                using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
                {
                    connection.Open();
                    string sql = "UPDATE TB_ITEM_SET set IS_LOCKED = @IS_LOCKED where ITEM_SET_ID = @ITEM_SET_ID";
                    using (SqlCommand commandEx = new SqlCommand(sql, connection))
                    {
                        commandEx.Parameters.Add(new SqlParameter("@IS_LOCKED", System.Data.SqlDbType.Bit));
                        commandEx.Parameters["@IS_LOCKED"].Value = _lockTheCoding;
                        commandEx.Parameters.Add(new SqlParameter("@ITEM_SET_ID", System.Data.SqlDbType.BigInt));
                        commandEx.Parameters["@ITEM_SET_ID"].Value = _Item_set_id;
                        commandEx.ExecuteNonQuery();
                    }
                }
            }
            _inputTokens = generatedText.usage.prompt_tokens;
            _outputTokens = generatedText.usage.total_tokens - generatedText.usage.prompt_tokens;
            _message = "Completed without errors. (Tokens: prompt: " + generatedText.usage.prompt_tokens.ToString() + ", total: " + generatedText.usage.total_tokens.ToString() + ")";
           
            return result;
        }

        private void SaveAttribute(AttributeSet aSet, Int64 ItemId, string info, int ReviewId, int ContactId)
        {
            // i.e. it's a Boolean type field, or the attribute wasn't found and we don't want the box 'ticked'
            if (info == "False" || info == "false")
            {
                return;
            }

            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                if (_Item_set_id == 0 && _onlyCodeInTheRobotName == true)
                {//this condition evaluates to TRUE only the first time we try saving a code, after which _Item_set_id will have a value > 0
                    //we do this special thing, only for ROBOTS, so to have the Robot Coding always created in the ROBOT's name
                    using (SqlCommand command = new SqlCommand("st_ItemSetPrepareForRobot", connection))
                    {
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@REVIEW_ID", ReviewId));
                        command.Parameters.Add(new SqlParameter("@ROBOT_CONTACT_ID", ContactId));
                        command.Parameters.Add(new SqlParameter("@ITEM_ID", ItemId));
                        command.Parameters.Add(new SqlParameter("@REVIEW_SET_ID", _reviewSetId));
                        command.Parameters.Add(new SqlParameter("@IS_LOCKED", _lockTheCoding));
                        command.Parameters.Add(new SqlParameter("@NEW_ITEM_SET_ID", 0));
                        command.Parameters["@NEW_ITEM_SET_ID"].Direction = System.Data.ParameterDirection.Output; 
                        //command.Parameters.Add(new SqlParameter("@IS_CODING_FINAL", false));
                        //command.Parameters["@IS_CODING_FINAL"].Direction = System.Data.ParameterDirection.Output; 
                        command.ExecuteNonQuery();
                        Int64 Item_set_id = (Int64)command.Parameters["@NEW_ITEM_SET_ID"].Value;
                        if (Item_set_id < 1)
                        {//can't save this code, st_ItemSetPrepareForRobot failed
                            throw new Exception("Could not create the coding record for the Robot");
                        }
                        //_CodingIsFinal = (bool)command.Parameters["@IS_CODING_FINAL"].Value;
                        _Item_set_id = Item_set_id;
                    }
                }
                try
                {

                    using (SqlCommand command = new SqlCommand("st_ItemAttributeInsert", connection))
                    {
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@CONTACT_ID", ContactId));
                        command.Parameters.Add(new SqlParameter("@ADDITIONAL_TEXT", info));
                        command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ID", aSet.AttributeId));
                        command.Parameters.Add(new SqlParameter("@SET_ID", aSet.SetId));
                        command.Parameters.Add(new SqlParameter("@ITEM_ID", ItemId));
                        command.Parameters.Add(new SqlParameter("@REVIEW_ID", ReviewId));
                        command.Parameters.Add(new SqlParameter("@ITEM_ARM_ID", (object)DBNull.Value));
                        if (_onlyCodeInTheRobotName == true) command.Parameters.Add(new SqlParameter("@ITEM_SET_ID", _Item_set_id)); //special param only for robots
                        command.Parameters.Add(new SqlParameter("@NEW_ITEM_ATTRIBUTE_ID", 0));
                        command.Parameters["@NEW_ITEM_ATTRIBUTE_ID"].Direction = System.Data.ParameterDirection.Output;
                        command.Parameters.Add(new SqlParameter("@NEW_ITEM_SET_ID", 0));
                        command.Parameters["@NEW_ITEM_SET_ID"].Direction = System.Data.ParameterDirection.Output;
                        command.ExecuteNonQuery();
                        if (_Item_set_id < 1) _Item_set_id = (Int64)command.Parameters["@NEW_ITEM_SET_ID"].Value;
                        if (hasSavedSomeCodes == false)
                        {
                            if ((Int64)command.Parameters["@NEW_ITEM_ATTRIBUTE_ID"].Value > 0) hasSavedSomeCodes = true;
                        }
                    }
                } 
                catch (Exception e)
                {
                    errors++;
                    using (SqlCommand command = new SqlCommand("st_CreateRobotApiCallError", connection))
                    {
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@JobId", _jobId));
                        command.Parameters.Add(new SqlParameter("@ERROR_MESSAGE", e.Message));
                        command.Parameters.Add(new SqlParameter("@STACK_TRACE", "Not-halting exception:" + Environment.NewLine + e.StackTrace));
                        command.Parameters.Add(new SqlParameter("@CURRENT_ITEM_ID", _itemId));
                        command.Parameters.Add(new SqlParameter("@IS_FAILURE", 0));
                        command.ExecuteNonQuery();
                    }
                }
                connection.Close();
            }
        }

        private string GetDoc(Int64 DocumentId, int ReviewId)
        {
            string ret = null;
            using (SqlConnection conn = new SqlConnection(BusinessLibrary.Data.DataConnection.ConnectionString))
            {
                conn.Open();
                using (SqlCommand command = new SqlCommand("st_ItemDocumentText", conn)) // N.B. this needs to be changed to return the text
                {
                    command.Parameters.Add(new SqlParameter("@DOC_ID", DocumentId));
                    command.Parameters.Add(new SqlParameter("@REV_ID", ReviewId));
                    command.CommandType = CommandType.StoredProcedure;
                    SafeDataReader dr = new SafeDataReader(command.ExecuteReader());
                    if (dr.Read())
                    {
                        ret = dr.GetString("DOCUMENT_TEXT");
                    }
                    conn.Close();
                }
            }
            return ret;
        }



        public class OpenAIResult
        {
            public string id { get; set; }
            public string _object { get; set; }
            public int created { get; set; }
            public string model { get; set; }
            public Prompt_Filter_Results[] prompt_filter_results { get; set; }
            public Choice[] choices { get; set; }
            public Usage usage { get; set; }
        }

        public class Usage
        {
            public int prompt_tokens { get; set; }
            public int completion_tokens { get; set; }
            public int total_tokens { get; set; }
        }

        public class Prompt_Filter_Results
        {
            public int prompt_index { get; set; }
            public Content_Filter_Results content_filter_results { get; set; }
        }

        public class Content_Filter_Results
        {
            public Hate hate { get; set; }
            public Self_Harm self_harm { get; set; }
            public Sexual sexual { get; set; }
            public Violence violence { get; set; }
        }

        public class Hate
        {
            public bool filtered { get; set; }
            public string severity { get; set; }
        }

        public class Self_Harm
        {
            public bool filtered { get; set; }
            public string severity { get; set; }
        }

        public class Sexual
        {
            public bool filtered { get; set; }
            public string severity { get; set; }
        }

        public class Violence
        {
            public bool filtered { get; set; }
            public string severity { get; set; }
        }

        public class Choice
        {
            public int index { get; set; }
            public string finish_reason { get; set; }
            public Message message { get; set; }
            public Content_Filter_Results content_filter_results { get; set; }
        }

        public class Message
        {
            public string role { get; set; }
            public string content { get; set; }
        }


#endif


    }

}
