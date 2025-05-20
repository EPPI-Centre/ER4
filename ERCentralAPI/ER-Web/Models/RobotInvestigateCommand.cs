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
using Markdig;
#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class RobotInvestigateCommand : LongLastingFireAndForgetCommand
    {

        public RobotInvestigateCommand() { }
        private string _queryForRobot = "";
        private string _getTextFrom = "";
        private Int64 _itemsWithThisAttribute;
        private Int64 _textFromThisAttribute;
        private int _sampleSize = 50;
        private string _returnMessage = "";
        private string _returnResultText = "";
        private string _returnItemIdList = "";

        private string _UserPrivateOpenAIKey = "";//will be filled in automatically if/when it's present in ReviewInfo
        private string _ExplicitEndpoint = "";
        private string _ExplicitEndpointKey = "";
        private int _jobId = 0;
        private int _reviewId = 0;
        private int _jobOwnerId = 0;
        private int _robotContactId = 0;
        private bool _Succeded = false;

        public static readonly PropertyInfo<RobotCoderReadOnly> RobotCoderProperty = RegisterProperty<RobotCoderReadOnly>(new PropertyInfo<RobotCoderReadOnly>("RobotCoder", "RobotCoder"));
        public RobotCoderReadOnly RobotCoder
        {
            get { return ReadProperty(RobotCoderProperty); }
            set { LoadProperty(RobotCoderProperty, value); }
        }

        public string ReturnMessage
        {
            get { return _returnMessage; }
        }
        public string queryForRobot
        {
            get { return _queryForRobot; }
        }
        public string getTextFrom
        {
            get { return _getTextFrom; }
        }
        public long textFromThisAttribute
        {
            get { return _textFromThisAttribute; }
        }
        public int sampleSize
        {
            get { return _sampleSize; }
        }
        
        public string returnResultText
        {
            get { return _returnResultText; }
        }
        public string returnItemIdList
        {
            get { return _returnItemIdList; }
        }
        public bool Succeded
        {
            get { return _Succeded; }
        }
        
        public int RobotContactId { get { return _robotContactId; } }
        public RobotInvestigateCommand(RobotCoderReadOnly robot, string queryForRobot, string getTextFrom, Int64 itemsWithThisAttribute, Int64 textFromThisAttribute, int sampleSize)
        {
            RobotCoder = robot;
            _queryForRobot = queryForRobot;
            _getTextFrom = getTextFrom;
            _itemsWithThisAttribute = itemsWithThisAttribute;
            _textFromThisAttribute = textFromThisAttribute;
            _sampleSize = sampleSize;
            _returnMessage = "";
            _returnResultText = "";
        }
        

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_queryForRobot", _queryForRobot);
            info.AddValue("_getTextFrom", _getTextFrom);
            info.AddValue("_itemsWithThisAttribute", _itemsWithThisAttribute);
            info.AddValue("_textFromThisAttribute", _textFromThisAttribute);
            info.AddValue("_sampleSize", _sampleSize);
            info.AddValue("_returnMessage", _returnMessage);
            info.AddValue("_returnResultText", _returnResultText);
            info.AddValue("_returnItemIdList", _returnItemIdList);
            info.AddValue("_UserPrivateOpenAIKey", _UserPrivateOpenAIKey);
            info.AddValue("_ExplicitEndpoint", _ExplicitEndpoint);
            info.AddValue("_ExplicitEndpointKey", _ExplicitEndpointKey);
            info.AddValue("_jobId", _jobId);
            info.AddValue("_reviewId", _reviewId);
            info.AddValue("_jobOwnerId", _jobOwnerId);
            info.AddValue("_robotContactId", _robotContactId);
            info.AddValue("_Succeded", _Succeded);
        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, StateMode mode)
        {
            _queryForRobot = info.GetValue<string>("_queryForRobot");
            _getTextFrom = info.GetValue<string>("_getTextFrom");
            _itemsWithThisAttribute = info.GetValue<Int64>("_itemsWithThisAttribute");
            _textFromThisAttribute = info.GetValue<Int64>("_textFromThisAttribute");
            _sampleSize = info.GetValue<Int16>("_sampleSize");
            _returnMessage = info.GetValue<string>("_returnMessage");
            _returnResultText = info.GetValue<string>("_returnResultText");
            _returnItemIdList = info.GetValue<string>("_returnItemIdList");
            _UserPrivateOpenAIKey = info.GetValue<string>("_UserPrivateOpenAIKey");
            _ExplicitEndpoint = info.GetValue<string>("_ExplicitEndpoint");
            _ExplicitEndpointKey = info.GetValue<string>("_ExplicitEndpointKey");
            _jobId = info.GetValue<int>("_jobId");
            _reviewId = info.GetValue<int>("_reviewId");
            _jobOwnerId = info.GetValue<int>("_jobOwnerId");
            _robotContactId = info.GetValue<int>("_robotContactId");
            _Succeded = info.GetValue<bool>("_Succeded");
        }


#if !SILVERLIGHT

        private int _inputTokens = 0;
        private int _outputTokens = 0;
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
                _returnMessage = "Error: GPT4 is disabled or there is no credit.";
                return;
            }
            else
            {
                if (_jobId == 0)
                {//we need to create a record for this in TB_ROBOT_API_CALL_LOG - it's a single call doing one item!
                    CreditForRobots CfR = rInfo.CreditForRobotsList.FirstOrDefault(f => f.AmountRemaining >= 0.01);
                    int creditPurchaseId = 0;
                    if (CfR == null && rInfo.OpenAIEnabled == false)
                    {
                        _returnMessage = "Error: did not find suitable credit to use.";
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
                            command.Parameters.Add(new SqlParameter("@CRITERIA", "Robot investigate single query"));
                            command.Parameters.Add(new SqlParameter("@REVIEW_SET_ID", (object)0));  //see https://learn.microsoft.com/en-us/dotnet/api/system.data.sqlclient.sqlparameter.-ctor?view=net-9.0-pp&redirectedfrom=MSDN#system-data-sqlclient-sqlparameter-ctor(system-string-system-object)
                            command.Parameters.Add(new SqlParameter("@CURRENT_ITEM_ID", (object)0)); 
                            command.Parameters.Add(new SqlParameter("@FORCE_CODING_IN_ROBOT_NAME", false));
                            command.Parameters.Add(new SqlParameter("@LOCK_CODING", false));
                            command.Parameters.Add(new SqlParameter("@USE_PDFS", false));
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
                                _returnMessage = "Error. " + command.Parameters["@result"].Value.ToString();
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
                        MarkAsCancelled();
                        return;
                    }
                    _Succeded = Task.Run(() => DoRobot(_reviewId, _robotContactId)).GetAwaiter().GetResult();//this runs synchronously, hence the catch will work
                }
                catch (Exception e)
                {

                    _Succeded = false;
                    _returnMessage = "Error. " + Environment.NewLine + e.Message;
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
                            command.Parameters.Add(new SqlParameter("@STATUS",  "Failed"));
                            command.Parameters.Add(new SqlParameter("@CURRENT_ITEM_ID", (object)0));
                            command.Parameters.Add(new SqlParameter("@INPUT_TOKENS_COUNT", _inputTokens));
                            command.Parameters.Add(new SqlParameter("@OUTPUT_TOKENS_COUNT", _outputTokens));
                            command.Parameters.Add(new SqlParameter("@ERROR_MESSAGE", SavedMsg));
                            command.Parameters.Add(new SqlParameter("@STACK_TRACE", e.StackTrace));
                            command.ExecuteNonQuery();
                        }
                    }
                    
                    return;
                }
                if (AppIsShuttingDown && _Succeded == false)
                {
                    MarkAsCancelled();
                    return;
                }
                using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand("st_UpdateRobotApiCallLog", connection))
                    {
                        string status;
                        if (_Succeded == false) status = "Failed";
                        else status = "Finished";
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@REVIEW_ID", _reviewId));
                        command.Parameters.Add(new SqlParameter("@ROBOT_API_CALL_ID", _jobId));
                        command.Parameters.Add(new SqlParameter("@STATUS", status));
                        command.Parameters.Add(new SqlParameter("@CURRENT_ITEM_ID", (object)0)); // using _Item_set_id for an int64 value of 0
                        command.Parameters.Add(new SqlParameter("@INPUT_TOKENS_COUNT", _inputTokens));
                        command.Parameters.Add(new SqlParameter("@OUTPUT_TOKENS_COUNT", _outputTokens));
                        command.ExecuteNonQuery();
                    }
                }
            }
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
                endpoint = RobotCoder.EndPoint;
                key = AzureSettings.RobotAPIKeyByRobotName(RobotCoder.RobotName);
            }


            // ************************ from here: the new RobotInvestigate code *****************************

            // get the text for the query - whether from titles & abstracts, info box, or coded pdf files
            // if no text return error to say so

            string userprompt = getUserPrompt(ReviewId);
            if (AppIsShuttingDown)//getting the user prompt might have taken considerable time, so we check if in the meantime ER6 is trying to recycle its app pool
            {
                return false;//we'll mark as cancelled in the parent method
            }
            if (userprompt == "")
            {
                _returnMessage = "Error: No text retrieved from items";
                return false;
            }
            string sysprompt = _queryForRobot;

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
            List<RobotOpenAICommand.OpenAIChatClass> messages = new List<RobotOpenAICommand.OpenAIChatClass>
            {
                new RobotOpenAICommand.OpenAIChatClass { role = "system", content = sysprompt}, // {participants: number // total number of participants,\n arm_count: string // number of study arms,\n intervention: string // description of intervention,\n comparison: string // description of comparison }" },
                new RobotOpenAICommand.OpenAIChatClass { role = "user", content = userprompt},
            };

            // *** additional params (modifiable in web.config)
            double temperature = Convert.ToDouble(RobotCoder.Temperature);
            int frequency_penalty = Convert.ToInt16(RobotCoder.FrequencyPenalty);
            int presence_penalty = Convert.ToInt16(RobotCoder.PresencePenalty);
            double top_p = Convert.ToDouble(RobotCoder.TopP);

            // *** Create the client and submit the request to the LLM
            var client = new HttpClient();
            string json;
            if (_UserPrivateOpenAIKey == "")
            {
                client.DefaultRequestHeaders.Add("api-key", $"{key}");
                //string type = "json_object"; as in this case, we're not requesting JSON in response and get an error if we do
                //var response_format = new { type };
                var requestBody = new { /*response_format, */ messages, temperature, frequency_penalty, presence_penalty, top_p };
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
                _returnMessage = "Error: " + response.ReasonPhrase;
                result = false;
                return result;
            }


            var responseString = await response.Content.ReadAsStringAsync();
            var generatedText = Newtonsoft.Json.JsonConvert.DeserializeObject<RobotOpenAICommand.OpenAIResult>(responseString);
            _inputTokens = generatedText.usage.prompt_tokens;
            _outputTokens = generatedText.usage.total_tokens - generatedText.usage.prompt_tokens;
            MarkdownPipeline pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
            var responses = Markdown.ToHtml(generatedText.choices[0].message.content, pipeline);
            //var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(responses);

            _returnMessage = "Completed " + "without errors. (Tokens: prompt: " + generatedText.usage.prompt_tokens.ToString() + ", total: " + generatedText.usage.total_tokens.ToString() + ")";
            _returnResultText = responses.ToString();
            //if (!_returnResultText.Contains("<TABLE") || !_returnResultText.Contains("<table"))
            //{
            //    _returnResultText = _returnResultText.Replace(Environment.NewLine, "<br/>").Replace("\n", "<br/>");
            //}
            return result;
        }

        private string getUserPrompt(int ReviewId)
        {
            string retVal = "";
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ItemTextSample", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ReviewId));
                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ID_LIST", _itemsWithThisAttribute.ToString()));
                    command.Parameters.Add(new SqlParameter("@WhichText", _getTextFrom));
                    command.Parameters.Add(new SqlParameter("@WhichAttribute", _textFromThisAttribute));
                    command.Parameters.Add(new SqlParameter("@SampleSize", _sampleSize));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        while (reader.Read())
                        {
                            if (retVal != "")
                            {
                                retVal += Environment.NewLine;
                            }
                            retVal += "ID: " + reader.GetInt64("ITEM_ID").ToString() + Environment.NewLine;
                            if (_returnItemIdList == "")
                            {
                                _returnItemIdList = reader.GetInt64("ITEM_ID").ToString();
                            }
                            else
                            {
                                _returnItemIdList += "," + reader.GetInt64("ITEM_ID").ToString();
                            }
                            switch (_getTextFrom)
                            {
                                case "title":
                                    retVal += "Title: " + reader.GetString("TITLE") + Environment.NewLine;
                                    retVal += "Abstract: " + reader.GetString("ABSTRACT") + Environment.NewLine;
                                    break;
                                case "info":
                                    retVal += "Snippet: " + reader.GetString("ADDITIONAL_TEXT") + Environment.NewLine;
                                    break;
                                case "highlighted":
                                    retVal += "Snippet: " + reader.GetString("SELECTION_TEXTS") + Environment.NewLine;
                                    break;
                                default: // this should never happen
                                    break;
                            }
                            
                        }
                    }
                }
                connection.Close();
            }
            return retVal;
        }

        private static readonly Regex BooleanPromptRx = new Regex(@": ?boolean ?\/\/");

        private string blobConnection = AzureSettings.blobConnection;
        

        private void MarkAsCancelled()
        {
            _Succeded = false;
            _returnMessage = "Error: cancelled (app is restarting, please try again)";
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_UpdateRobotApiCallLog", connection))
                {//this is to update the token numbers, and thus the cost, if we can
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID ", _reviewId));
                    command.Parameters.Add(new SqlParameter("@ROBOT_API_CALL_ID", _jobId));
                    command.Parameters.Add(new SqlParameter("@STATUS", "Failed"));
                    command.Parameters.Add(new SqlParameter("@CURRENT_ITEM_ID", (object)0));
                    command.Parameters.Add(new SqlParameter("@INPUT_TOKENS_COUNT", _inputTokens));
                    command.Parameters.Add(new SqlParameter("@OUTPUT_TOKENS_COUNT", _outputTokens));
                    command.Parameters.Add(new SqlParameter("@ERROR_MESSAGE", "App restarting while job was running"));
                    command.Parameters.Add(new SqlParameter("@STACK_TRACE", "N/A"));
                    command.ExecuteNonQuery();
                }
            }
        }

#endif


    }

}
