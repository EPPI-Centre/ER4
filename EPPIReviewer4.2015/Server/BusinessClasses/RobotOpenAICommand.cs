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
        private string _message;

        public string ReturnMessage
        {
            get { return _message; }
        }

        public RobotOpenAICommand(int reviewSetId, Int64 itemId, Int64 itemDocumentId)
        {
            _reviewSetId = reviewSetId;
            _itemId = itemId;
            _itemDocumentId = itemDocumentId;
            _message = "";
        }

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_reviewSetId", _reviewSetId);
            info.AddValue("_itemId", _itemId);
            info.AddValue("_itemDocumentId", _itemDocumentId);
            info.AddValue("_message", _message);
        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, StateMode mode)
        {
            _reviewSetId = info.GetValue<int>("_reviewSetId");
            _itemId = info.GetValue<Int64>("_itemId");
            _itemDocumentId = info.GetValue<Int64>("_itemDocumentId");
            _message = info.GetValue<string>("_message");
        }


#if !SILVERLIGHT

        protected override void DataPortal_Execute()
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            bool result = Task.Run(() => DoRobot(ri.ReviewId, ri.UserId)).GetAwaiter().GetResult();
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

            int errors = 0;

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
            int wordCount = i.Abstract.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length + i.Title.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
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
            {
                string newPrompt = getPrompts("", subSet);
                if (newPrompt != "")
                {
                    prompt += newPrompt;
                }
            }

            if (prompt == "")
            {
                _message = "No valid prompts in codeset";
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
            try
            {
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
                _message = "Completed without errors. (Tokens: prompt: " + generatedText.usage.prompt_tokens.ToString() + ", total: " + generatedText.usage.total_tokens.ToString() + ")";
            }
            catch
            {
                _message = "Sorry there was an error";
            }
            
            if (errors > 0)
            {
                _message = "Completed with " + errors.ToString() + " errors";
            }
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
                    command.Parameters.Add(new SqlParameter("@NEW_ITEM_ATTRIBUTE_ID", 0));
                    command.Parameters["@NEW_ITEM_ATTRIBUTE_ID"].Direction = System.Data.ParameterDirection.Output;
                    command.Parameters.Add(new SqlParameter("@NEW_ITEM_SET_ID", 0));
                    command.Parameters["@NEW_ITEM_SET_ID"].Direction = System.Data.ParameterDirection.Output;
                    command.ExecuteNonQuery();
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
