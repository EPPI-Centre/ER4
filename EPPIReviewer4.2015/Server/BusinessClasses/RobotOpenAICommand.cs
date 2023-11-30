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
        private string _ntoppassages;
        private string _wsizes;
        private string _threshold;
        private string _message;

        private static PropertyInfo<ReviewSet> SelectedReviewSetProperty = RegisterProperty<ReviewSet>(new PropertyInfo<ReviewSet>("SelectedReviewSet", "SelectedReviewSet"));
        public ReviewSet SelectedReviewSet
        {
            get { return ReadProperty(SelectedReviewSetProperty); }
            set { LoadProperty(SelectedReviewSetProperty, value); }
        }

        private static PropertyInfo<ItemDocument> SelectedItemDocumentProperty = RegisterProperty<ItemDocument>(new PropertyInfo<ItemDocument>("SelectedItemDocument", "SelectedItemDocument"));
        public ItemDocument SelectedItemDocument
        {
            get { return ReadProperty(SelectedItemDocumentProperty); }
            set { LoadProperty(SelectedItemDocumentProperty, value); }
        }

        public string ReturnMessage
        {
            get { return _message; }
        }

        public RobotOpenAICommand(string ntoppassages)
        {
            _ntoppassages = ntoppassages;
        }

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_ntoppassages", _ntoppassages);
            info.AddValue("_wsizes", _wsizes);
            info.AddValue("_threshold", _threshold);
            info.AddValue("_message", _message);
        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, StateMode mode)
        {
            _ntoppassages = info.GetValue<string>("_ntoppassages");
            _wsizes = info.GetValue<string>("_wsizes");
            _threshold = info.GetValue<string>("_threshold");
            _message = info.GetValue<string>("_message");
        }


#if !SILVERLIGHT

        protected override void DataPortal_Execute()
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            bool result = Task.Run(() => DoRobot(ri.ReviewId, ri.UserId)).GetAwaiter().GetResult();
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
            string document = GetDoc(SelectedItemDocument.ItemDocumentId, ReviewId);

            /*
            string prompt = "Tell me a joke!";

            string messages = "[{\"role\":\"system\",\"content\":\"You are an AI assistant that helps people find information.\"},{\"role\":\"user\",\"content\":\"Hi - tell me a joke!\"},{\"role\":\"assistant\",\"content\":\"Sure! Why did the tomato turn red? Because it saw the salad dressing\"}],\"max_tokens\": 800,\"temperature\": 0.7,  \"frequency_penalty\": 0,  \"presence_penalty\": 0, \"top_p\": 0.95, \"stop\": null}";

            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("api-key", $"{key}");
            client.DefaultRequestHeaders.Add("Content-Type", $"application/json");
            var requestBody = new { messages };
            var json = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(endpoint, content);
            var responseString = await response.Content.ReadAsStringAsync();
            var generatedText = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(responseString).text;
            */


            
            if (errors == 0)
            {
                _message = "Completed without errors";
            }
            else
            {
                _message = "Completed with " + errors.ToString() + " errors";
            }
            return result;
        }

        private string GetTextSpans(string[] strings)
        {
            string s = "";
            foreach (string ss in strings)
            {
                if (s == "")
                {
                    s = ss;
                }
                else
                {
                    s += Environment.NewLine + Environment.NewLine + ss;
                }
            }
            return s;
        }

        private Int64 SaveAttribute(string AttributeName, string info, int ReviewId, int ContactId)
        {
            Int64 ItemAttributeId = 0;
            AttributeSet aset = SelectedReviewSet.GetSetByName(AttributeName);
            if (aset != null)
            {
                if (aset.IsSelected == false) // i.e. no attribute exists - we just save a new attribute
                {
                    using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand("st_ItemAttributeInsert", connection))
                        {
                            command.CommandType = System.Data.CommandType.StoredProcedure;
                            command.Parameters.Add(new SqlParameter("@CONTACT_ID", ContactId));
                            command.Parameters.Add(new SqlParameter("@ADDITIONAL_TEXT", info));
                            command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ID", aset.AttributeId));
                            command.Parameters.Add(new SqlParameter("@SET_ID", aset.SetId));
                            command.Parameters.Add(new SqlParameter("@ITEM_ID", SelectedItemDocument.ItemId));
                            command.Parameters.Add(new SqlParameter("@REVIEW_ID", ReviewId));
                            command.Parameters.Add(new SqlParameter("@ITEM_ARM_ID", (object)DBNull.Value));
                            command.Parameters.Add(new SqlParameter("@NEW_ITEM_ATTRIBUTE_ID", 0));
                            command.Parameters["@NEW_ITEM_ATTRIBUTE_ID"].Direction = System.Data.ParameterDirection.Output;
                            command.Parameters.Add(new SqlParameter("@NEW_ITEM_SET_ID", 0));
                            command.Parameters["@NEW_ITEM_SET_ID"].Direction = System.Data.ParameterDirection.Output;
                            command.ExecuteNonQuery();
                            ItemAttributeId = (Int64)command.Parameters["@NEW_ITEM_ATTRIBUTE_ID"].Value;
                            Int64 _itemSetId = (Int64)command.Parameters["@NEW_ITEM_SET_ID"].Value;
                            aset.IsSelected = true;
                        }
                        connection.Close();
                    }
                }
                else // i.e. we can't save a new attribute - need to update an existing one
                {
                    using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand("st_ItemAttributeUpdateWithoutKey", connection))
                        {
                            command.CommandType = CommandType.StoredProcedure;
                            command.Parameters.Add(new SqlParameter("@CONTACT_ID", ContactId));
                            command.Parameters.Add(new SqlParameter("@ADDITIONAL_TEXT", info));
                            command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ID", aset.AttributeId));
                            command.Parameters.Add(new SqlParameter("@SET_ID", aset.SetId));
                            command.Parameters.Add(new SqlParameter("@ITEM_ID", SelectedItemDocument.ItemId));
                            command.Parameters.Add(new SqlParameter("@REVIEW_ID", ReviewId));
                            command.Parameters.Add(new SqlParameter("@ITEM_ATTRIBUTE_ID", 0));
                            command.Parameters["@ITEM_ATTRIBUTE_ID"].Direction = ParameterDirection.Output;
                            command.ExecuteNonQuery();
                            ItemAttributeId = (Int64)command.Parameters["@ITEM_ATTRIBUTE_ID"].Value;
                        }
                        connection.Close();
                    }
                }
            }
            return ItemAttributeId;
        }

        private void SaveAnnotation(string text, Int64 ItemAttributeId)
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ItemAttributePDFInsert", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@ITEM_ATTRIBUTE_ID", ItemAttributeId));
                    command.Parameters.Add(new SqlParameter("@ITEM_DOCUMENT_ID", SelectedItemDocument.ItemDocumentId));
                    command.Parameters.Add(new SqlParameter("@PAGE", 1));
                    command.Parameters.Add(new SqlParameter("@SHAPE_TEXT", ""));
                    command.Parameters.Add(new SqlParameter("@INTERVALS", "0;0"));
                    command.Parameters.Add(new SqlParameter("@TEXTS", text));
                    command.Parameters.Add(new SqlParameter("@PDFTRON_XML", ""));
                    command.Parameters.Add(new SqlParameter("@ITEM_ATTRIBUTE_PDF_ID", 0));
                    command.Parameters["@ITEM_ATTRIBUTE_PDF_ID"].Direction = System.Data.ParameterDirection.Output;
                    //command.ExecuteNonQuery();
                    //Int64 ItemAttributePdfId = (Int64)command.Parameters["@ITEM_ATTRIBUTE_PDF_ID"].Value;
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
                using (SqlCommand command = new SqlCommand("st_ItemDocumentText", conn))
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


#endif


    }

}
