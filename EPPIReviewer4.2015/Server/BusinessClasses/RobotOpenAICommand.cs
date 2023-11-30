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

#if SILVERLIGHT
    public RobotOpenAICommand(){}
#else
        public RobotOpenAICommand() { }
#endif
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
                string newPrompt = getPrompts(currentPrompt, subSet);
                if (newPrompt != "")
                {
                    currentPrompt += newPrompt + ",";
                }
            }
            return aSet.AttributeSetDescription;
        }

        private AttributeSet getAttributeFromPromptKey(string key, AttributeSet aSet)
        {
            if (aSet.AttributeSetDescription.StartsWith(key))
            {
                return aSet;
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
#if (CSLA_NETCORE)

            var configuration = ERxWebClient2.Startup.Configuration.GetSection("AzureMagSettings");

#else
            var configuration = ConfigurationManager.AppSettings;

#endif
            var jsonsettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };
            int errors = 0;

            string endpoint = configuration["RobotOpenAIEndpoint"];
            string key = configuration["RobotOpenAIKey2"];
            //string document = GetDoc(_itemDocumentId, ReviewId);  // when we re-enable this, we need to check the stored proc (below) will return the text

            Item i = Item.GetItemById(_itemId, ReviewId);
            if (i == null)
            {
                _message = "Null item";
                return false;
            }
            if (i.Abstract.Length < 50)
            {
                _message = "Short or non-existent abstract";
                return false;
            }
            int wordCount = i.Abstract.Split(' ').Length;
            if (wordCount > 3500)
            {
                _message = "Maximum word count is currently 3500 words. This abstract is " + wordCount.ToString() + " words long.";
                return false;
            }
            
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
                string newPrompt = getPrompts(prompt, subSet);
                if (newPrompt != "")
                {
                    prompt += newPrompt + ",\n";
                }
            }

            //document = "Internet-based interventions for smoking cessation could help millions of people stop smoking at very low unit costs; however, long-term biochemically verified evidence is scarce and such interventions might be less effective for smokers with low socioeconomic status than for those with high status because of lower online literacy to engage with websites. We aimed to assess a new interactive internet-based intervention (StopAdvisor) for smoking cessation that was designed with particular attention directed to people with low socioeconomic status. We did this online randomised controlled trial between Dec 6, 2011, and Oct 11, 2013, in the UK. Participants aged 18 years and older who smoked every day were randomly assigned (1:1) to receive treatment with StopAdvisor or an information-only website. Randomisation was automated with an unseen random number function embedded in the website to establish which treatment was revealed after the online baseline assessment. Recruitment continued until the required sample size had been achieved from both high and low socioeconomic status subpopulations. Participants, and researchers who obtained data and did laboratory analyses, were masked to treatment allocation. The primary outcome was 6 month sustained, biochemically verified abstinence. The main secondary outcome was 6 month, 7 day biochemically verified point prevalence. Analysis was by intention to treat. Homogeneity of intervention effect across the socioeconomic subsamples was first assessed to establish whether overall or separate subsample analyses were appropriate. The study is registered as an International Standard Randomised Controlled Trial, number ISRCTN99820519. We randomly assigned 4613 participants to the StopAdvisor group (n=2321) or the control group (n=2292); 2142 participants were of low socioeconomic status and 2471 participants were of high status. The overall rate of smoking cessation was similar between participants in the StopAdvisor and control groups for the primary (237 [10%] vs 220 [10%] participants; relative risk [RR] 1·06, 95% CI 0·89–1·27; p=0·49) and the secondary (358 [15%] vs 332 [15%] participants; 1·06, 0·93–1·22; p=0·37) outcomes; however, the intervention effect differed across socioeconomic status subsamples (1·44, 0·99–2·09; p=0·0562 and 1·37, 1·02–1·84; p=0·0360, respectively). StopAdvisor helped participants with low socioeconomic status stop smoking compared with the information-only website (primary outcome: 90 [8%] of 1088 vs 64 [6%] of 1054 participants; RR 1·36, 95% CI 1·00–1·86; p=0·0499; secondary outcome: 136 [13%] vs 100 [10%] participants; 1·32, 1·03–1·68, p=0·0267), but did not improve cessation rates in those with high socioeconomic status (147 [12%] of 1233 vs 156 [13%] of 1238 participants; 0·95, 0·77–1·17; p=0·61 and 222 [18%] vs 232 [19%] participants; 0·96, 0·81–1·13, p=0·64, respectively).";

            List<OpenAIChatClass> messages = new List<OpenAIChatClass>
            {
                new OpenAIChatClass { role = "system", content = "You extract data from the text provided below into a JSON object of the shape provided below. If the data is not in the text return 'null' for that field. \nShape: {" + prompt + "}"}, // {participants: number // total number of participants,\n arm_count: string // number of study arms,\n intervention: string // description of intervention,\n comparison: string // description of comparison }" },
                new OpenAIChatClass { role = "user", content = "Text: " + i.Abstract},
            };

            string engine = "gpt35";
            int max_tokens = 800;
            double temperature = 0.7;
            int frequency_penalty = 0;
            int presence_penalty = 0;
            double top_p = 0.95;
            object stop = null;

            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("api-key", $"{key}");
            var requestBody = new { messages };
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
