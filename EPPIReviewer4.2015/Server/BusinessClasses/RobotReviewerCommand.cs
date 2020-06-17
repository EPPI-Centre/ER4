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

#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class RobotReviewerCommand : CommandBase<RobotReviewerCommand>
    {

#if SILVERLIGHT
    public RobotReviewerCommand(){}
#else
        public RobotReviewerCommand() { }
#endif
        private string _title;
        private string _abstract;
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

        public RobotReviewerCommand(string title, string ab)
        {
            _title = title;
            _abstract = ab;
        }

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_title", _title);
            info.AddValue("_abstract", _abstract);
            info.AddValue("_message", _message);
        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, StateMode mode)
        {
            _title = info.GetValue<string>("_title");
            _abstract = info.GetValue<string>("_abstract");
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
            bool res = true;
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

            SelectedReviewSet.Attributes.RaiseListChangedEvents = false;

            var UploadData = new upload_data();
            List<RequestDoc> ls = new List<RequestDoc>();
            string[] s = { "rct_bot", "pico_span_bot", "bias_bot", "pico_bot", "sample_size_bot", "punchline_bot", "bias_ab_bot", "human_bot" };
            var submitDoc = new RequestDoc();
            submitDoc.ti = _title;
            submitDoc.ab = _abstract;
            submitDoc.fullText = SelectedItemDocument.Text;
            ls.Add(submitDoc);

            UploadData.articles = ls;
            UploadData.robots = s;
            UploadData.filter_rcts = "none";

            var json = JsonConvert.SerializeObject(UploadData);
            var data = new StringContent(json, Encoding.UTF8, "application/json");
            string endpoint = configuration["RobotReviewerEndpoint"];

            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var response = await client.PostAsync(endpoint + "queue-documents", data);

            var partial = await response.Content.ReadAsAsync<object>();
            var reportId = JsonConvert.DeserializeObject<Dictionary<string, string>>(partial.ToString());

            bool keepWating = true; int counter = 0;
            while (keepWating && counter <= 60)// wait 1s for no more than 60 times
            {
                counter++;
                Thread.Sleep(1000);
                var c2 = new HttpClient();
                var r2 = await c2.GetAsync(endpoint + "report-status/" + reportId["report_id"]);
                string partial2 = await r2.Content.ReadAsStringAsync();
                var job = JsonConvert.DeserializeObject(partial2);
                CurrentStatus stat = JsonConvert.DeserializeObject<CurrentStatus>(job.ToString());
                if (stat.state == "SUCCESS")
                {
                    keepWating = false;
                }
            }

            var c3 = new HttpClient();
            var r3 = await c3.GetAsync(endpoint + "report/" + reportId["report_id"]);
            var part3 = await r3.Content.ReadAsStringAsync();
            var job3 = JsonConvert.DeserializeObject(part3);
            JArray jArray = JArray.Parse(job3.ToString());
            dynamic d = jArray.First;

            string errors = "";

            // ********** RCT bot first ********
            try
            {
                Rct_Bot rct = JsonConvert.DeserializeObject<Rct_Bot>(d.rct_bot.ToString());
                if (rct.is_rct_balanced == true)
                {
                    SaveAttribute("RR2", rct.score.ToString(), ReviewId, UserId);
                }
                if (rct.is_rct_precise == true)
                {
                    SaveAttribute("RR3", rct.score.ToString(), ReviewId, UserId);
                }
                if (rct.is_rct_sensitive == true)
                {
                    SaveAttribute("RR4", rct.score.ToString(), ReviewId, UserId);
                }
                if (rct.is_rct == true)
                {
                    SaveAttribute("RR5", rct.score.ToString(), ReviewId, UserId);
                }
            }
            catch
            {
                res = false;
                errors = "Error processing rct_bot";
            }
            // ***** PICO span bot - includes PICO pdf spans and  MeSH terms
            try
            {
                Pico_Span_Bot pico_span = JsonConvert.DeserializeObject<Pico_Span_Bot>(d.pico_span_bot.ToString());
                string tmp = GetTextSpans(pico_span.population);
                if (tmp != "")
                {
                    Int64 ItemAttributeId = SaveAttribute("RR8", tmp, ReviewId, UserId);
                    for (int i = 0; i < pico_span.population.Count(); i++)
                    {
                        SaveAnnotation(pico_span.population[i], ItemAttributeId);
                    }
                }
                tmp = GetTextSpans(pico_span.interventions);
                if (tmp != "")
                {
                    Int64 ItemAttributeId = SaveAttribute("RR9", tmp, ReviewId, UserId);
                    for (int i = 0; i < pico_span.interventions.Count(); i++)
                    {
                        SaveAnnotation(pico_span.interventions[i], ItemAttributeId);
                    }
                }
                tmp = GetTextSpans(pico_span.outcomes);
                if (tmp != "")
                {
                    Int64 ItemAttributeId = SaveAttribute("RR10", tmp, ReviewId, UserId);
                    for (int i = 0; i < pico_span.outcomes.Count(); i++)
                    {
                        SaveAnnotation(pico_span.outcomes[i], ItemAttributeId);
                    }
                }

                // Now go through the MeSH terms - part of the same bot

                AttributeSet p_mesh = SelectedReviewSet.GetSetByExt_URL("RR12");
                foreach (Population_Mesh p in pico_span.population_mesh)
                {
                    AttributeSet aMesh = p_mesh.GetSetByExt_URL(p.mesh_ui);
                    if (aMesh == null)
                    {
                        AddMeSHAttribute(p.mesh_term, p.mesh_ui, p.cui, p_mesh, UserId, ReviewId);
                    }
                    else
                    {
                        SaveAttribute(p.mesh_ui, "", ReviewId, UserId);
                    }
                }
                AttributeSet i_mesh = SelectedReviewSet.GetSetByExt_URL("RR13");
                foreach (Interventions_Mesh i in pico_span.interventions_mesh)
                {
                    AttributeSet aMesh = i_mesh.GetSetByExt_URL(i.mesh_ui);
                    if (aMesh == null)
                    {
                        AddMeSHAttribute(i.mesh_term, i.mesh_ui, i.cui, i_mesh, UserId, ReviewId);
                    }
                    else
                    {
                        SaveAttribute(i.mesh_ui, "", ReviewId, UserId);
                    }
                }
                AttributeSet o_mesh = SelectedReviewSet.GetSetByExt_URL("RR14");
                foreach (Outcomes_Mesh o in pico_span.outcomes_mesh)
                {
                    AttributeSet aMesh = o_mesh.GetSetByExt_URL(o.mesh_ui);
                    if (aMesh == null)
                    {
                        AddMeSHAttribute(o.mesh_term, o.mesh_ui, o.cui, o_mesh, UserId, ReviewId);
                    }
                    else
                    {
                        SaveAttribute(o.mesh_ui, "", ReviewId, UserId);
                    }
                }
            }
            catch
            {
                res = false;
                if (errors == "")
                {
                    errors = "Error processing pico_span_bot";
                }
                else
                {
                    errors += ", pico_span_bot";
                }
            }

            try // PICO bot - text spans incl pdf annotations
            {
                Pico_Bot pico = JsonConvert.DeserializeObject<Pico_Bot>(d.pico_bot.ToString());
                string tmp = "";
                foreach (Annotation4 a5 in pico.participants.annotations)
                {
                    if (tmp == "")
                    {
                        tmp = a5.text;
                    }
                    else
                    {
                        tmp += Environment.NewLine + Environment.NewLine + a5.text;
                    }
                }
                if (tmp != "")
                {
                    Int64 itemAttributeId = SaveAttribute("RR47", tmp, ReviewId, UserId);
                    foreach (Annotation4 a in pico.participants.annotations)
                    {
                        SaveAnnotation(a.text, itemAttributeId);
                    }

                }
                tmp = "";
                foreach (Annotation5 a5 in pico.interventions.annotations)
                {
                    if (tmp == "")
                    {
                        tmp = a5.text;
                    }
                    else
                    {
                        tmp += Environment.NewLine + Environment.NewLine + a5.text;
                    }
                }
                if (tmp != "")
                {
                    Int64 itemAttributeId = SaveAttribute("RR48", tmp, ReviewId, UserId);
                    foreach (Annotation5 a in pico.interventions.annotations)
                    {
                        SaveAnnotation(a.text, itemAttributeId);
                    }
                }
                tmp = "";
                foreach (Annotation6 a5 in pico.outcomes.annotations)
                {
                    if (tmp == "")
                    {
                        tmp = a5.text;
                    }
                    else
                    {
                        tmp += Environment.NewLine + Environment.NewLine + a5.text;
                    }
                }
                if (tmp != "")
                {
                    Int64 itemAttributeId = SaveAttribute("RR49", tmp, ReviewId, UserId);
                    foreach (Annotation6 a in pico.outcomes.annotations)
                    {
                        SaveAnnotation(a.text, itemAttributeId);
                    }
                }
            }
            catch
            {
                res = false;
                if (errors == "")
                {
                    errors = "Error processing pico_bot";
                }
                else
                {
                    errors += ", pico_bot";
                }
            }

            // *********** bias bot (on full text, including pdf snippets) ************
            try
            {
                Bias_Bot bias = JsonConvert.DeserializeObject<Bias_Bot>(d.bias_bot.ToString());
                string ExtURL = bias.random_sequence_generation.judgement == "low" ? "RR17" : "RR18";
                string tmp = "";
                foreach (Annotation a in bias.random_sequence_generation.annotations)
                {
                    if (tmp == "")
                    {
                        tmp = a.text;
                    }
                    else
                    {
                        tmp += Environment.NewLine + Environment.NewLine + a.text;
                    }
                }
                Int64 ItemAttributeId = SaveAttribute(ExtURL, tmp, ReviewId, UserId);
                foreach (Annotation a in bias.random_sequence_generation.annotations)
                {
                    SaveAnnotation(a.text, ItemAttributeId);
                }

                ExtURL = bias.allocation_concealment.judgement == "low" ? "RR20" : "RR21";
                tmp = "";
                foreach (Annotation1 a in bias.allocation_concealment.annotations)
                {
                    if (tmp == "")
                    {
                        tmp = a.text;
                    }
                    else
                    {
                        tmp += Environment.NewLine + Environment.NewLine + a.text;
                    }
                }
                ItemAttributeId = SaveAttribute(ExtURL, tmp, ReviewId, UserId);
                foreach (Annotation1 a in bias.allocation_concealment.annotations)
                {
                    SaveAnnotation(a.text, ItemAttributeId);
                }

                ExtURL = bias.blinding_participants_personnel.judgement == "low" ? "RR23" : "RR24";
                tmp = "";
                foreach (Annotation2 a in bias.blinding_participants_personnel.annotations)
                {
                    if (tmp == "")
                    {
                        tmp = a.text;
                    }
                    else
                    {
                        tmp += Environment.NewLine + Environment.NewLine + a.text;
                    }
                }
                ItemAttributeId = SaveAttribute(ExtURL, tmp, ReviewId, UserId);
                foreach (Annotation2 a in bias.blinding_participants_personnel.annotations)
                {
                    SaveAnnotation(a.text, ItemAttributeId);
                }

                ExtURL = bias.blinding_outcome_assessment.judgement == "low" ? "RR26" : "RR27";
                tmp = "";
                foreach (Annotation3 a in bias.blinding_outcome_assessment.annotations)
                {
                    if (tmp == "")
                    {
                        tmp = a.text;
                    }
                    else
                    {
                        tmp += Environment.NewLine + Environment.NewLine + a.text;
                    }
                }
                ItemAttributeId = SaveAttribute(ExtURL, tmp, ReviewId, UserId);
                foreach (Annotation3 a in bias.blinding_outcome_assessment.annotations)
                {
                    SaveAnnotation(a.text, ItemAttributeId);
                }
            }
            catch
            {
                res = false;
                if (errors == "")
                {
                    errors = "Error processing bias_bot";
                }
                else
                {
                    errors += ", bias_bot";
                }
            }

            // SAMPLE SIZE BOT
            try
            {
                Sample_Size_Bot sample = JsonConvert.DeserializeObject<Sample_Size_Bot>(d.sample_size_bot.ToString());
                SaveAttribute("RR42", sample.num_randomized, ReviewId, UserId);
            }
            catch
            {
                res = false;
                if (errors == "")
                {
                    errors = "Error processing sample_size_bot";
                }
                else
                {
                    errors += ", sample_size_bot";
                }
            }

            // PUNCHLINE BOT
            try
            {
                Punchline_Bot punch = JsonConvert.DeserializeObject<Punchline_Bot>(d.punchline_bot.ToString());
                SaveAttribute("RR44", punch.punchline_text, ReviewId, UserId);
                SaveAttribute("RR45", punch.effect, ReviewId, UserId);
            }
            catch
            {
                res = false;
                if (errors == "")
                {
                    errors = "Error processing punchline_bot";
                }
                else
                {
                    errors += ", punchline_bot";
                }
            }

            // BIAS ON ABSTRACT
            try
            {
                Bias_Ab_Bot bias_ab = JsonConvert.DeserializeObject<Bias_Ab_Bot>(d.bias_ab_bot.ToString());
                if (bias_ab.random_sequence_generation.judgement == "low")
                {
                    SaveAttribute("RR30", "", ReviewId, UserId);
                }
                else
                {
                    SaveAttribute("RR31", "", ReviewId, UserId);
                }
                if (bias_ab.allocation_concealment.judgement == "low")
                {
                    SaveAttribute("RR33", "", ReviewId, UserId);
                }
                else
                {
                    SaveAttribute("RR34", "", ReviewId, UserId);
                }
                if (bias_ab.blinding_participants_personnel.judgement == "low")
                {
                    SaveAttribute("RR36", "", ReviewId, UserId);
                }
                else
                {
                    SaveAttribute("RR37", "", ReviewId, UserId);
                }
                if (bias_ab.blinding_outcome_assessment.judgement == "low")
                {
                    SaveAttribute("RR39", "", ReviewId, UserId);
                }
                else
                {
                    SaveAttribute("RR40", "", ReviewId, UserId);
                }
            }
            catch
            {
                res = false;
                if (errors == "")
                {
                    errors = "Error processing bias_ab_bot";
                }
                else
                {
                    errors += ", bias_ab_bot";
                }
            }

            // Human study classifier
            try
            {
                Human_Bot h = JsonConvert.DeserializeObject<Human_Bot>(d.human_bot.ToString());
                if (h.is_human)
                {
                    SaveAttribute("RR6", "", ReviewId, UserId);
                }
            }
            catch
            {
                res = false;
                if (errors == "")
                {
                    errors = "Error processing human_bot";
                }
                else
                {
                    errors += ", human_bot";
                }
            }
            
            if (errors != "")
            {
                _message = "RobotReviewer completed with errors: " + errors;
            }
            else
            {
                _message = "RobotReviewer completed with no errors";
            }
            return res;
        }

        private string GetTextSpans(string [] strings)
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

        private Int64 SaveAttribute(string ExtUrl, string info, int ReviewId, int ContactId)
        {
            Int64 ItemAttributeId = 0;
            AttributeSet aset = SelectedReviewSet.GetSetByExt_URL(ExtUrl);
            if (aset != null)
            {
                if (aset.IsSelected == false) // i.e. no item_attribute record exists - we just save a new one
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
                        }
                        connection.Close();
                    }
                }
                else // i.e. we can't save a new item attribute record - need to update an existing one
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

        private void AddMeSHAttribute(string name, string ExtURL, string desc, AttributeSet parent,
            int ContactId, int ReviewId)
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                Int64 NewAttributeSetId;
                Int64 NewAttributeId; ;
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_AttributeSetInsert", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@SET_ID", parent.SetId));
                    command.Parameters.Add(new SqlParameter("@PARENT_ATTRIBUTE_ID", parent.AttributeId));
                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_TYPE_ID", 2));
                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_SET_DESC", "CUI:" + desc));
                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_NAME", name));
                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_DESC", "CUI:" + desc));
                    command.Parameters.Add(new SqlParameter("@CONTACT_ID", ContactId));
                    command.Parameters.Add(new SqlParameter("@Ext_URL", ExtURL));
                    command.Parameters.Add(new SqlParameter("@Ext_Type", "MeSH Term"));
                    command.Parameters.Add(new SqlParameter("@NEW_ATTRIBUTE_SET_ID", 0));
                    command.Parameters.Add(new SqlParameter("@NEW_ATTRIBUTE_ID", 0));
                    command.Parameters["@NEW_ATTRIBUTE_SET_ID"].Direction = System.Data.ParameterDirection.Output;
                    command.Parameters["@NEW_ATTRIBUTE_ID"].Direction = System.Data.ParameterDirection.Output;
                    command.ExecuteNonQuery();
                    NewAttributeSetId = (Int64)command.Parameters["@NEW_ATTRIBUTE_SET_ID"].Value;
                    NewAttributeId = (Int64)command.Parameters["@NEW_ATTRIBUTE_ID"].Value;
                }
                using (SqlCommand command = new SqlCommand("st_ItemAttributeInsert", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@CONTACT_ID", ContactId));
                    command.Parameters.Add(new SqlParameter("@ADDITIONAL_TEXT", ""));
                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ID", NewAttributeId));
                    command.Parameters.Add(new SqlParameter("@SET_ID", parent.SetId));
                    command.Parameters.Add(new SqlParameter("@ITEM_ID", SelectedItemDocument.ItemId));
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ReviewId));
                    command.Parameters.Add(new SqlParameter("@ITEM_ARM_ID", (object)DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@NEW_ITEM_ATTRIBUTE_ID", 0));
                    command.Parameters["@NEW_ITEM_ATTRIBUTE_ID"].Direction = System.Data.ParameterDirection.Output;
                    command.Parameters.Add(new SqlParameter("@NEW_ITEM_SET_ID", 0));
                    command.Parameters["@NEW_ITEM_SET_ID"].Direction = System.Data.ParameterDirection.Output;
                    command.ExecuteNonQuery();
                    Int64 _ItemAttributeId = (Int64)command.Parameters["@NEW_ITEM_ATTRIBUTE_ID"].Value;
                    Int64 _itemSetId = (Int64)command.Parameters["@NEW_ITEM_SET_ID"].Value;
                }
                connection.Close();
            }
        }




#endif


    }

    public class RobotReviewerReturn
    {
        public RobotReviewerRoot[] Property1 { get; set; }
    }

    public class RobotReviewerRoot
    {
        public string ti { get; set; }
        public string ab { get; set; }
        public string fullText { get; set; }
        public Rct_Bot rct_bot { get; set; }
        public Pico_Span_Bot pico_span_bot { get; set; }
        public Bias_Bot bias_bot { get; set; }
        public Pico_Bot pico_bot { get; set; }
        public Sample_Size_Bot sample_size_bot { get; set; }
        public Punchline_Bot punchline_bot { get; set; }
        public Bias_Ab_Bot bias_ab_bot { get; set; }
        public Human_Bot human_bot { get; set; }
    }

    public class Rct_Bot
    {
        public string model { get; set; }
        public float score { get; set; }
        public string threshold_type { get; set; }
        public float threshold_value { get; set; }
        public bool is_rct { get; set; }
        public bool is_rct_precise { get; set; }
        public bool is_rct_balanced { get; set; }
        public bool is_rct_sensitive { get; set; }
        public int ptyp_rct { get; set; }
    }

    public class Pico_Span_Bot
    {
        public string[] population { get; set; }
        public string[] interventions { get; set; }
        public string[] outcomes { get; set; }
        public float[][] population_berts { get; set; }
        public float[][] interventions_berts { get; set; }
        public float[][] outcomes_berts { get; set; }
        public Population_Mesh[] population_mesh { get; set; }
        public Interventions_Mesh[] interventions_mesh { get; set; }
        public Outcomes_Mesh[] outcomes_mesh { get; set; }
    }

    public class Population_Mesh
    {
        public string mesh_term { get; set; }
        public string mesh_ui { get; set; }
        public string cui { get; set; }
    }

    public class Interventions_Mesh
    {
        public string mesh_term { get; set; }
        public string mesh_ui { get; set; }
        public string cui { get; set; }
    }

    public class Outcomes_Mesh
    {
        public string mesh_term { get; set; }
        public string mesh_ui { get; set; }
        public string cui { get; set; }
    }

    public class Bias_Bot
    {
        public Random_Sequence_Generation random_sequence_generation { get; set; }
        public Allocation_Concealment allocation_concealment { get; set; }
        public Blinding_Participants_Personnel blinding_participants_personnel { get; set; }
        public Blinding_Outcome_Assessment blinding_outcome_assessment { get; set; }
    }

    public class Random_Sequence_Generation
    {
        public string judgement { get; set; }
        public Annotation[] annotations { get; set; }
    }

    public class Annotation
    {
        public string text { get; set; }
        public int start_index { get; set; }
    }

    public class Allocation_Concealment
    {
        public string judgement { get; set; }
        public Annotation1[] annotations { get; set; }
    }

    public class Annotation1
    {
        public string text { get; set; }
        public int start_index { get; set; }
    }

    public class Blinding_Participants_Personnel
    {
        public string judgement { get; set; }
        public Annotation2[] annotations { get; set; }
    }

    public class Annotation2
    {
        public string text { get; set; }
        public int start_index { get; set; }
    }

    public class Blinding_Outcome_Assessment
    {
        public string judgement { get; set; }
        public Annotation3[] annotations { get; set; }
    }

    public class Annotation3
    {
        public string text { get; set; }
        public int start_index { get; set; }
    }

    public class Pico_Bot
    {
        public Participants participants { get; set; }
        public Interventions interventions { get; set; }
        public Outcomes outcomes { get; set; }
    }

    public class Participants
    {
        public Annotation4[] annotations { get; set; }
    }

    public class Annotation4
    {
        public string text { get; set; }
        public int start_index { get; set; }
    }

    public class Interventions
    {
        public Annotation5[] annotations { get; set; }
    }

    public class Annotation5
    {
        public string text { get; set; }
        public int start_index { get; set; }
    }

    public class Outcomes
    {
        public Annotation6[] annotations { get; set; }
    }

    public class Annotation6
    {
        public string text { get; set; }
        public int start_index { get; set; }
    }

    public class Sample_Size_Bot
    {
        public string num_randomized { get; set; }
    }

    public class Punchline_Bot
    {
        public string punchline_text { get; set; }
        public string effect { get; set; }
    }

    public class Bias_Ab_Bot
    {
        public Random_Sequence_Generation1 random_sequence_generation { get; set; }
        public Allocation_Concealment1 allocation_concealment { get; set; }
        public Blinding_Participants_Personnel1 blinding_participants_personnel { get; set; }
        public Blinding_Outcome_Assessment1 blinding_outcome_assessment { get; set; }
    }

    public class Random_Sequence_Generation1
    {
        public string judgement { get; set; }
    }

    public class Allocation_Concealment1
    {
        public string judgement { get; set; }
    }

    public class Blinding_Participants_Personnel1
    {
        public string judgement { get; set; }
    }

    public class Blinding_Outcome_Assessment1
    {
        public string judgement { get; set; }
    }

    public class Human_Bot
    {
        public bool is_human { get; set; }
    }




    public class CurrentStatus
    {
        public string state { get; set; }
        public Meta meta { get; set; }
    }

    public class Meta
    {
        public string status { get; set; }
        public string task { get; set; }
    }


    class ReportId
    {
        public string key { get; set; }
        public string value { get; set; }
    }

    class Waiting
    {
        public string state { get; set; }
    }

    class upload_data
    {
        public List<RequestDoc> articles { get; set; }
        public string[] robots { get; set; }
        public string filter_rcts { get; set; }
    }

    class RequestDoc
    {
        public string ti { get; set; }
        public string ab { get; set; }
        public string fullText { get; set; }
    }
}
