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
    public class RobotHBCPCommand : CommandBase<RobotHBCPCommand>
    {

#if SILVERLIGHT
    public RobotHBCPCommand(){}
#else
        public RobotHBCPCommand() { }
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

        public RobotHBCPCommand(string ntoppassages, string wsizes, string threshold)
        {
            _ntoppassages = ntoppassages;
            _wsizes = wsizes;
            _threshold = threshold;
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

            string endpoint = configuration["RobotHBCPEndpoint"];
            byte[] document = GetPdfDoc(SelectedItemDocument.ItemDocumentId, ReviewId);

            MultipartFormDataContent mpf = new MultipartFormDataContent("file");
            mpf.Add(new StreamContent(new MemoryStream(document)), "file", SelectedItemDocument.Title);
            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var response = await client.PostAsync(endpoint + "all?ntoppassages=" + _ntoppassages +
                "&wsizes=10%2C20&threshold=" + _threshold, mpf);
            var partial = await response.Content.ReadAsStringAsync();
            var job = JsonConvert.DeserializeObject(partial);
            JArray jArray = JArray.Parse(job.ToString());
            int errors = 0;

            List<HBCPRoot> attributeList = new List<HBCPRoot>();


            foreach (dynamic d in jArray)
            {
                try
                {
                    HBCPRoot atr = JsonConvert.DeserializeObject<HBCPRoot>(d.ToString());
                    attributeList.Add(atr as HBCPRoot);
                }
                catch
                {
                    result = false;
                    errors++;
                }
            }
            for (int i = 0; i < attributeList.Count; i++)
            {
                if (attributeList[i].attribute.id == "")
                {
                    continue; // i.e. we've already saved it
                }
                attributeList[i].context = "Value: " + attributeList[i].value + Environment.NewLine +
                    "Context: " + attributeList[i].context + Environment.NewLine +
                    "Page number: " + attributeList[i].page;

                // consolidate any other occurences of the same attribute
                for (int c = i + 1; c < attributeList.Count; c++)
                {
                    if (attributeList[i].attribute.id == attributeList[c].attribute.id &&
                        attributeList[i].attribute.id != "")
                    {
                        attributeList[i].context += Environment.NewLine + Environment.NewLine +
                            "Value: " + attributeList[c].value + Environment.NewLine +
                            "Context: " + attributeList[c].context + Environment.NewLine +
                            "Page number: " + attributeList[c].page;
                        attributeList[c].attribute.id = "";
                    }
                }
                SaveAttribute(attributeList[i].attribute.name, attributeList[i].context, ReviewId, UserId);
            }
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

        private byte[] GetPdfDoc(Int64 DocumentId, int ReviewId)
        {
            byte[] ret = null;
            using (SqlConnection conn = new SqlConnection(BusinessLibrary.Data.DataConnection.ConnectionString))
            {
                conn.Open();
                using (SqlCommand command = new SqlCommand("st_ItemDocumentBin", conn))
                {
                    command.Parameters.Add(new SqlParameter("@DOC_ID", DocumentId));
                    command.Parameters.Add(new SqlParameter("@REV_ID", ReviewId));
                    command.CommandType = CommandType.StoredProcedure;
                    SqlDataReader dr = command.ExecuteReader();
                    if (dr.Read())
                    {
                        ret = (byte[])dr["DOCUMENT_BINARY"];
                    }
                    conn.Close();
                }
            }
            return ret;
        }


#endif


    }



    public class HBCPRoot
    {
        public HBCPAttribute attribute { get; set; }
        public string value { get; set; }
        public string docname { get; set; }
        public Arm arm { get; set; }
        public string context { get; set; }
        public string page { get; set; }
    }

    public class HBCPAttribute
    {
        public string id { get; set; }
        public string type { get; set; }
        public string name { get; set; }
        public string shortName { get; set; }
    }

    public class Arm
    {
        public string id { get; set; }
        public string name { get; set; }
        public string names { get; set; }
    }



}
