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
using System.Text.RegularExpressions;








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
    public class RobotOpenAICommand : LongLastingFireAndForgetCommand<RobotOpenAICommand>
    {

        public RobotOpenAICommand() { }
        private int _reviewSetId;
        private Int64 _itemDocumentId;
        private Int64 _itemId;
        private string _message = "";
        private string _UserPrivateOpenAIKey = "";//will be filled in automatically if/when it's present in ReviewInfo
        private string _ExplicitEndpoint = "";
        private string _ExplicitEndpointKey = "";
        private bool _isLastInBatch = true;
        private int _jobId = 0;
        private int _reviewId = 0;
        private int _jobOwnerId = 0;
        private int _robotContactId = 0;
        private bool _onlyCodeInTheRobotName = true;
        private bool _lockTheCoding = true;
        private bool _useFullTextDocument = false;
        private string _DocsList = "";
        private bool _Succeded = false;
        private int errors = 0;

        public string ReturnMessage
        {
            get { return _message; }
        }
        public bool Succeded
        {
            get { return _Succeded; }
        }
        public int NonFatalErrors
        {
            get { return errors; }
        }
        public int RobotContactId { get { return _robotContactId; } }
        public RobotOpenAICommand(int reviewSetId, Int64 itemId, Int64 itemDocumentId, bool onlyCodeInTheRobotName = true, bool lockTheCoding = true, bool useFullTextDocument = false)
        {
            _reviewSetId = reviewSetId;
            _itemId = itemId;
            _itemDocumentId = itemDocumentId;
            _message = "";
            _onlyCodeInTheRobotName = onlyCodeInTheRobotName;
            _lockTheCoding = lockTheCoding;
            _useFullTextDocument = useFullTextDocument;
        }
        public RobotOpenAICommand(int reviewSetId, Int64 itemId, Int64 itemDocumentId, bool isLastInBatch, int JobId, int robotContactId, int reviewId, 
            int JobOwnerId, bool onlyCodeInTheRobotName = true, bool lockTheCoding = true, bool useFullTextDocument = false, string docsList = "",
            string ExplicitEndpoint = "", string ExplicitEndpointKey = "")
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
            _reviewId = reviewId;
            _jobOwnerId = JobOwnerId;
            _ExplicitEndpoint = ExplicitEndpoint;
            _ExplicitEndpointKey = ExplicitEndpointKey;
            _useFullTextDocument = useFullTextDocument;
            _DocsList = docsList;
        }

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_reviewSetId", _reviewSetId);
            info.AddValue("_itemId", _itemId);
            info.AddValue("_itemDocumentId", _itemDocumentId);
            info.AddValue("_message", _message);
            info.AddValue("_UserPrivateOpenAIKey", _UserPrivateOpenAIKey);
            info.AddValue("_ExplicitEndpoint", _ExplicitEndpoint);
            info.AddValue("_ExplicitEndpointKey", _ExplicitEndpointKey);
            info.AddValue("_isLastInBatch", _isLastInBatch);
            info.AddValue("_jobId", _jobId);
            info.AddValue("_reviewId", _reviewId);
            info.AddValue("_jobOwnerId", _jobOwnerId);
            info.AddValue("_robotContactId", _robotContactId);
            info.AddValue("_onlyCodeInTheRobotName", _onlyCodeInTheRobotName);
            info.AddValue("_lockTheCoding", _lockTheCoding);
            info.AddValue("_Succeded", _Succeded);
            info.AddValue("_useFullTextDocument", _useFullTextDocument); 
            info.AddValue("_DocsList", _DocsList);
            info.AddValue("errors", errors);
        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, StateMode mode)
        {
            _reviewSetId = info.GetValue<int>("_reviewSetId");
            _itemId = info.GetValue<Int64>("_itemId");
            _itemDocumentId = info.GetValue<Int64>("_itemDocumentId");
            _message = info.GetValue<string>("_message");
            _UserPrivateOpenAIKey = info.GetValue<string>("_UserPrivateOpenAIKey");
            _ExplicitEndpoint = info.GetValue<string>("_ExplicitEndpoint");
            _ExplicitEndpointKey = info.GetValue<string>("_ExplicitEndpointKey");
            _isLastInBatch = info.GetValue<bool>("_isLastInBatch");
            _jobId = info.GetValue<int>("_jobId");
            _reviewId = info.GetValue<int>("_reviewId");
            _jobOwnerId = info.GetValue<int>("_jobOwnerId");
            _robotContactId = info.GetValue<int>("_robotContactId");
            _onlyCodeInTheRobotName = info.GetValue<bool>("_onlyCodeInTheRobotName");
            _lockTheCoding = info.GetValue<bool>("_lockTheCoding");
            _Succeded = info.GetValue<bool>("_Succeded");
            _useFullTextDocument = info.GetValue<bool>("_useFullTextDocument");
            _DocsList = info.GetValue<string>("_DocsList");
            errors = info.GetValue<int>("errors");
        }


#if !SILVERLIGHT

        private int _inputTokens = 0;
        private int _outputTokens = 0;
        private Int64 _Item_set_id = 0;
        private bool hasSavedSomeCodes = false;

        protected override void DataPortal_Execute()
        {
            ReviewInfo rInfo;
            if (Csla.ApplicationContext.User != null && Csla.ApplicationContext.User.Identity != null && Csla.ApplicationContext.User.Identity.IsAuthenticated == true)
            {
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                _reviewId = ri.ReviewId;
                _jobOwnerId = ri.UserId;
                rInfo = DataPortal.Fetch<ReviewInfo>();
            }
            else if (_reviewId > 0)
            {
                rInfo = DataPortal.Fetch<ReviewInfo>(new SingleCriteria<ReviewInfo, int>(_reviewId));
            }
            else
            {
                throw new System.Security.Authentication.AuthenticationException("RobotOpenAICommand attempted to execute for unknown Review and/or user.");
            }
            if (!rInfo.CanUseRobots)
            {
                _message = "Error: GPT4 is disabled or there is no credit.";
                return;
            }
            if (_jobId == 0)
            {//we need to create a record for this in TB_ROBOT_API_CALL_LOG - it's a single call doing one item!
                CreditForRobots CfR = rInfo.CreditForRobotsList.FirstOrDefault(f => f.AmountRemaining >= 0.01);
                int creditPurchaseId = 0;
                if (CfR == null && rInfo.OpenAIEnabled == false)
                {
                    _message = "Error: did not find suitable credit to use.";
                    return;
                }
                else if (CfR != null)
                {
                    creditPurchaseId = CfR.CreditPurchaseId;
                }
                using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand("st_RobotApiCallLogCreate", connection))
                    {
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@REVIEW_ID", _reviewId));
                        command.Parameters.Add(new SqlParameter("@CONTACT_ID", _jobOwnerId));
                        command.Parameters.Add(new SqlParameter("@CREDIT_PURCHASE_ID", creditPurchaseId));
                        command.Parameters.Add(new SqlParameter("@ROBOT_NAME", "OpenAI GPT4"));
                        command.Parameters.Add(new SqlParameter("@CRITERIA", "ItemIds: " + _itemId));
                        command.Parameters.Add(new SqlParameter("@REVIEW_SET_ID", _reviewSetId));
                        command.Parameters.Add(new SqlParameter("@CURRENT_ITEM_ID", _itemId));
                        command.Parameters.Add(new SqlParameter("@FORCE_CODING_IN_ROBOT_NAME", _onlyCodeInTheRobotName));
                        command.Parameters.Add(new SqlParameter("@LOCK_CODING", _lockTheCoding));
                        command.Parameters.Add(new SqlParameter("@USE_PDFS", _useFullTextDocument));
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
                            _message = "Error. " + command.Parameters["@result"].Value.ToString();
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
                if (AppIsShuttingDown)
                {
                    MarkAsPaused();
                    return;
                }
                _Succeded = Task.Run(() => DoRobot(_reviewId, _robotContactId)).GetAwaiter().GetResult();//this runs synchronously, hence the catch will work
                if (errors > 0)
                {
                    _message += Environment.NewLine + "Error(s) occurred. Could not save " + errors.ToString() + " code(s).";
                    if (hasSavedSomeCodes == false && _Item_set_id > 0)
                    {
                        DeleteItemSetIfEmpty();
                    }
                    if (AppIsShuttingDown)
                    {
                        MarkAsPaused();
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                _Succeded = false;
                _message = "Error. " + Environment.NewLine + e.Message;
                ErrorLogSink(e.Message);
                if (e.StackTrace != null) ErrorLogSink(e.StackTrace);
                using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
                {
                    string SavedMsg = e.Message;
                    if (SavedMsg.Length > 200) SavedMsg = SavedMsg.Substring(0, 200);
                    connection.Open();
                    using (SqlCommand command = new SqlCommand("st_UpdateRobotApiCallLog", connection))
                    {//this is to update the token numbers, and thus the cost, if we can
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@REVIEW_ID ", _reviewId));
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
                if (hasSavedSomeCodes == false && _Item_set_id > 0)
                {
                    DeleteItemSetIfEmpty();
                }
                return;
            }
            if (AppIsShuttingDown && _Succeded == false)
            {
                MarkAsPaused();
                return;
            }
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_UpdateRobotApiCallLog", connection))
                {
                    string status;
                    if (_isLastInBatch == false) status = "Running";
                    else
                    {
                        if (_Succeded == false) status = "Failed";
                        else status = "Finished";
                    }
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", _reviewId));
                    command.Parameters.Add(new SqlParameter("@ROBOT_API_CALL_ID", _jobId));
                    command.Parameters.Add(new SqlParameter("@STATUS", status));
                    command.Parameters.Add(new SqlParameter("@CURRENT_ITEM_ID", _itemId));
                    command.Parameters.Add(new SqlParameter("@INPUT_TOKENS_COUNT", _inputTokens));
                    command.Parameters.Add(new SqlParameter("@OUTPUT_TOKENS_COUNT", _outputTokens));
                    command.ExecuteNonQuery();
                }
            }
        }

        internal class OpenAIChatClass
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
                possiblePrompt = possiblePrompt.Replace("'", "").Replace(",", "").Replace("{", "").Replace("}","").Replace("\"", ""); // once out of Alpha we could make this more complete
                int firstIndexOfColumn = possiblePrompt.IndexOf(":");
                if (firstIndexOfColumn == -1) { return currentPrompt; }
                possiblePrompt = "\"" + possiblePrompt.Insert(firstIndexOfColumn, "\"");
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
            //
            string endpoint;
            string key;
            if (_UserPrivateOpenAIKey != "")
            {
                //TEMPORARY (May 2024): use RobotOpenAIDirectEndpoint when we have an ad-hoc OpenAIKey for it
                endpoint = AzureSettings.RobotOpenAIDirectEndpoint;
                key = _UserPrivateOpenAIKey;

            }
            else if (_ExplicitEndpoint != "" && _ExplicitEndpointKey != "")
            {
                endpoint = _ExplicitEndpoint;
                key = _ExplicitEndpointKey;
            }
            else
            {
                endpoint = AzureSettings.RobotOpenAIEndpoint;
                key = AzureSettings.RobotOpenAIKey2;
            }
            //string document = GetDoc(_itemDocumentId, ReviewId);  // when we re-enable this, we need to check the stored proc (below) will return the text
            Item i = null;
            if (_useFullTextDocument == false)
            {
                // *** Get item and check that it has an abstract
                i = Item.GetItemById(_itemId, ReviewId);
                if (i == null)
                {
                    _message = "Error: Null item";
                    return false;
                }
                if (i.Abstract.Trim().Length + i.Title.Trim().Length < 50)
                {
                    _message = "Error: Short or non-existent title and abstract";
                    return false;
                }
                //SG Feb 2025 commented out: this check is redundant, given much bigger context window (compared to when this check was written) and automatic truncation added later on
                //char[] chars = { ' ' };
                //int wordCount = i.Abstract.Split(chars, StringSplitOptions.RemoveEmptyEntries).Length + i.Title.Split(chars, StringSplitOptions.RemoveEmptyEntries).Length;
                //if (wordCount > 3500)
                //{
                //    _message = "Error: Maximum word count is currently 3500 words. This title+abstract is " + wordCount.ToString() + " words long.";
                //    return false;
                //}
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

            string userprompt = "";
            string sysprompt = "";

            // *** Create the prompt for the LLM
            if (_useFullTextDocument == false)
            {
                if (i != null)
                {
                    bool hastitle = true;
                    bool hasabstract = true;
                    if (i.Title.Trim().Length <= 1) { hastitle = false; }
                    if (i.Abstract.Trim().Length <= 1) { hasabstract = false; }
                    userprompt = "";
                    sysprompt = "You extract data from the text provided below into a JSON object of the shape provided below. If the data is not in the text return 'false' for that field. \nShape: {" + prompt + "}";
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
                }
                else
                {
                    _message = "Error: Null item";
                    return false;
                }
            }
            else if (_DocsList != "")
            {
                string AllText = "";
                string[] Filenames = _DocsList.Split(new[] { ','}, StringSplitOptions.RemoveEmptyEntries);
                List<string> DocsContent = new List<string>();

                foreach (string Filename in Filenames)
                {
                    DocsContent.Add(GetDoc(Filename, _reviewId));
                }
                //we order the docs by length of the markdown, longer on top!
                DocsContent.Sort((x, y) => y.Length.CompareTo(x.Length));
                foreach (string FileContent in DocsContent)
                {
                    AllText += FileContent;
                    if (FileContent != DocsContent[DocsContent.Count - 1])
                    {
                        AllText += Environment.NewLine + "-----------------------" + Environment.NewLine + Environment.NewLine;
                    }
                }
                if (AllText == "")
                {
                    _message = "Error: no PDF text to process";
                    return false;
                }
                sysprompt = "You extract data from the markdown text provided below into a JSON object of the shape provided below. If the data is not in the text return 'false' for that field. \nShape: {" + prompt + "}";
                userprompt = AllText;
               
            }
            else
            {
                _message = "Error: no PDFs to process";
                return false;
            }
            if (AppIsShuttingDown)
            {
                ErrorLogSink("Cancelling RobotOpenAICommand after preparing most of the prompts.");
                return false;
            }
            //we add lenght checks here and simply truncate the userprompt if things are too long.
            int limit = 556522; //128000 / 0.23 we calculated this using 2 tests that had 100K++ tokens each - they had 0.209696 and 0.213639 tokens per char, rounded up for safety
            if (sysprompt.Length + userprompt.Length > limit)
            {//call is likely to fail because it's too long - we truncate the userprompt and hope for the best!
                if (sysprompt.Length > limit)
                {//oh my, the prompts themselves are too many. Deliberately rise an exception which produces the highest level of visibility: gets saved in TB_ROBOT_API_CALL_ERROR_LOG
                    //we will eventually show errors collected there to users!
                    throw new Exception("There are too many prompts, leaving no room for the data to classify!");
                }
                int excess = sysprompt.Length + userprompt.Length - limit;
                userprompt = userprompt.Substring(0, userprompt.Length - excess);//this will work, having checked if sysprompt is too long in itself!
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
            string json;
            if (_UserPrivateOpenAIKey == "")
            {
                client.DefaultRequestHeaders.Add("api-key", $"{key}");
                string type = "json_object";
                var response_format = new { type };
                var requestBody = new { response_format, messages, temperature, frequency_penalty, presence_penalty, top_p };
                //var requestBody = new { messages, temperature, frequency_penalty, presence_penalty, top_p };
                json = JsonConvert.SerializeObject(requestBody);
            }
            else
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", key);
                string model = "gpt-4-0613";
                // gpt-4o, gpt-4-turbo, or gpt-3.5-turbo can use the response_format: "json_object" option, not "gpt-4-0613" at this time
                //string type = "json_object";
                //var response_format = new { type };
                //var requestBody = new { model, response_format, messages, temperature, frequency_penalty, presence_penalty, top_p };
                var requestBody = new { model, messages, temperature, frequency_penalty, presence_penalty, top_p };
                json = JsonConvert.SerializeObject(requestBody);
            }
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            ////code to test what happens if we send too many requests
            //for (int ii = 0; ii < 10; ii++)
            //{
            //    if (ii == 9) { var response0 = await client.PostAsync(endpoint, content); }
            //    else { var response0 = client.PostAsync(endpoint, content); }
            //}
            HttpResponseMessage response = null;
            try
            {
                response = await client.PostAsync(endpoint, content, CancelToken);
            }
            catch (OperationCanceledException e)
            {
                ErrorLogSink("Cancelling RobotOpenAICommand while awaiting for the OpenAI API to answer.");
                return false; //we'll detect the cancellation request elsewhere
            }
            if (response.IsSuccessStatusCode == false)
            {
                _message = "Error: " + response.ReasonPhrase;
                result = false;
                return result;
            }

            if (AppIsShuttingDown)//last time we check, if we need to cancel while we're saving results, it's best to have a go and try to save all results, not just some!
            {
                ErrorLogSink("Cancelling RobotOpenAICommand after getting the OpenAI API answer.");
                return false;//ditto, code calling this will notice the app-cancellation request
            }

            var responseString = await response.Content.ReadAsStringAsync();
            var generatedText = Newtonsoft.Json.JsonConvert.DeserializeObject<OpenAIResult>(responseString);
            _inputTokens = generatedText.usage.prompt_tokens;
            _outputTokens = generatedText.usage.total_tokens - generatedText.usage.prompt_tokens;
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
            _message = "Completed " + (errors > 0 ? "with" : "without") + " errors. (Tokens: prompt: " + generatedText.usage.prompt_tokens.ToString() + ", total: " + generatedText.usage.total_tokens.ToString() + ")";
           
            return result;
        }

        private static readonly Regex BooleanPromptRx = new Regex(@": ?boolean ?\/\/");
        private void SaveAttribute(AttributeSet aSet, Int64 ItemId, string info, int ReviewId, int ContactId)
        {
            
            string LowerCaseInfo = info.ToLower();
            string desc = aSet.AttributeSetDescription.ToLower();
            if (LowerCaseInfo == "false")
            {// i.e. it's a Boolean type field, or the attribute wasn't found and we don't want the box 'ticked'
                return;
            }
            if ((LowerCaseInfo == "true") && RobotOpenAICommand.BooleanPromptRx.IsMatch(desc))
            { 
                //we Will NOT put anything in the infobox if the prompt is genuinely a boolean one
                //no need to tick the checkbox and then say "true" in the infobox
                info = "";
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
                            if (
                                command.Parameters["@NEW_ITEM_ATTRIBUTE_ID"].Value != System.DBNull.Value
                                && (Int64)command.Parameters["@NEW_ITEM_ATTRIBUTE_ID"].Value > 0) hasSavedSomeCodes = true;
                        }
                    }
                } 
                catch (Exception e)
                {
                    errors++;
                    using (SqlCommand command = new SqlCommand("st_UpdateRobotApiCallLog", connection))
                    {//this should NOT update the token numbers, and thus the cost, as it will be done later
                        string SavedMsg = e.Message;
                        if (SavedMsg.Length > 200) SavedMsg = SavedMsg.Substring(0, 200);
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@REVIEW_ID ", ReviewId));
                        command.Parameters.Add(new SqlParameter("@ROBOT_API_CALL_ID", _jobId));
                        command.Parameters.Add(new SqlParameter("@STATUS", "Running"));
                        command.Parameters.Add(new SqlParameter("@CURRENT_ITEM_ID", _itemId));
                        command.Parameters.Add(new SqlParameter("@INPUT_TOKENS_COUNT", 0));
                        command.Parameters.Add(new SqlParameter("@OUTPUT_TOKENS_COUNT", 0));
                        command.Parameters.Add(new SqlParameter("@ERROR_MESSAGE", SavedMsg));
                        command.Parameters.Add(new SqlParameter("@STACK_TRACE", e.StackTrace));
                        command.ExecuteNonQuery();
                    }
                }
                connection.Close();
            }
        }

        private void DeleteItemSetIfEmpty()
        {
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
        private string blobConnection = AzureSettings.blobConnection;
        private string GetDoc(string filename, int ReviewId)
        {            
            string containerName = "eppi-reviewer-data";
            string FileNamePrefix = "eppi-rag-pdfs/" + DataFactoryHelper.NameBase + "ReviewId" + ReviewId + "/";
            MemoryStream stream = BlobOperations.DownloadBlobAsMemoryStream(blobConnection, containerName, FileNamePrefix + filename);
            string ret = Encoding.UTF8.GetString(stream.GetBuffer(), 0, (int)stream.Length);
            return ret;
        }

        private void MarkAsPaused()
        {
            _Succeded = false;
            _message = "Cancelled";
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_UpdateRobotApiCallLog", connection))
                {//this is to update the token numbers, and thus the cost, if we can
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID ", _reviewId));
                    command.Parameters.Add(new SqlParameter("@ROBOT_API_CALL_ID", _jobId));
                    command.Parameters.Add(new SqlParameter("@STATUS", "Paused"));
                    command.Parameters.Add(new SqlParameter("@CURRENT_ITEM_ID", _itemId));
                    command.Parameters.Add(new SqlParameter("@INPUT_TOKENS_COUNT", _inputTokens));
                    command.Parameters.Add(new SqlParameter("@OUTPUT_TOKENS_COUNT", _outputTokens));
                    command.ExecuteNonQuery();
                }
            }
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
