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
using Csla.Data;


#if !SILVERLIGHT
using System.Data.SqlClient;
using BusinessLibrary.Data;
using BusinessLibrary.Security;
#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class ReportAllCodingCommand : CommandBase<ReportAllCodingCommand>
    {
        public ReportAllCodingCommand() { _result = ""; }

        private int _setId;
        private string _result;
        private bool _buildReport = true;


        public ReportAllCodingCommand(int setId)
        {
            _result = "";
            _setId = setId;
        }
        public ReportAllCodingCommand(int setId, bool buildReport)
        {
            _result = "";
            _setId = setId;
            _buildReport = buildReport;
        }
        public string Result
        {
            get { return _result; }
        }


        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_result", _result);
            info.AddValue("_setId", _setId);
            info.AddValue("_buildReport", _buildReport);
        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            _result = info.GetValue<string>("_result");
            _setId = info.GetValue<int>("_setId"); ;
            _buildReport = info.GetValue<bool>("_buildReport"); 
        }


#if !SILVERLIGHT
        public List<MiniAtt> Attributes = new List<MiniAtt>();
        public List<MiniItem> Items = new List<MiniItem>();
        private bool HasOutcomes = false; //this is used to add a second table for outcomes, if necessary.
        protected override void DataPortal_Execute()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ReportAllCodingCommand", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@ReviewId", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@SetId", _setId));
                    command.CommandTimeout = 60;//we might have to retreive hundred of thousands of rows...
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        while (reader.Read())
                        {//the list of attributes in this set
                            Attributes.Add(
                                new MiniAtt( reader.GetInt64("a_id"), reader.GetString("a_name"), reader.GetString("full_path") )
                               );
                        }
                        reader.NextResult();//coding data...
                        MiniItem tmpItem = new MiniItem(-1, "", "", "");
                        while (reader.Read()) 
                        {
                            long tmpId = reader.GetInt64("ItemId");
                            if (tmpItem.ItemId != tmpId)//we've encountered a new item...
                            {
                                if (tmpItem.ItemId > 0)
                                {
                                    Items.Add(tmpItem);
                                }
                                tmpItem = new MiniItem(tmpId, reader.GetString("TITLE"), reader.GetString("SHORT_TITLE"), reader.GetString("State"));
                            }
                            long tmpAttId = reader.GetInt64("ATTRIBUTE_ID");
                            MiniAtt CurrAtt = Attributes.Find(f => f.AttId == tmpAttId);
                            if (CurrAtt == null) continue;//shouldn't really happen, but adding this for safety...
                            CurrAtt.IsInReport = true;
                            if (tmpItem.Codings.ContainsKey(CurrAtt))
                            {//we'll add the coding for this attribute to this item
                                tmpItem.Codings[CurrAtt].Add(new MiniCoding(reader));
                            }
                            else
                            {// add new element in the dictionary, we don't have coding for this attribute in the current item...
                                List<MiniCoding> vals = new List<MiniCoding>();
                                vals.Add(new MiniCoding(reader));
                                tmpItem.Codings.Add(CurrAtt, vals);
                            }
                        }
                        Items.Add(tmpItem);
                        reader.NextResult();//PDF coding data...
                        while (reader.Read())
                        {
                            long tmpId = reader.GetInt64("ItemId");
                            long tmpAttId = reader.GetInt64("ATTRIBUTE_ID");
                            long tmpItemAttId = reader.GetInt64("ITEM_ATTRIBUTE_ID");
                            int miIndex = Items.FindIndex(f => f.ItemId == tmpId);
                            if (miIndex >= 0)
                            {//found the item
                                MiniItem mi = Items[miIndex];
                                KeyValuePair<MiniAtt, List<MiniCoding>> kvp = mi.Codings.FirstOrDefault(f => f.Key.AttId == tmpAttId);
                                if (kvp.Key != null)
                                {//found the list of ItemAttributes (coding) for this item
                                    MiniCoding mc = kvp.Value.FirstOrDefault(f => f.ItemAttId == tmpItemAttId);
                                    if (mc != null)
                                    {//found the exact ItemAttribute that has this PDF coding
                                        mc.PDF.Add(
                                            new MiniPDFatt(reader.GetString("DOCUMENT_TITLE"), reader.GetInt32("PAGE"), reader.GetString("TEXT"))
                                            );
                                    }
                                }
                            }
                        }
                        reader.NextResult();//Outcomes coding data...
                        while (reader.Read())
                        {
                            HasOutcomes = true;
                            long tmpId = reader.GetInt64("ItemId");
                            int tmpContactId = reader.GetInt32("ContactId");

                            int miIndex = Items.FindIndex(f => f.ItemId == tmpId);
                            if (miIndex >= 0)
                            {//found the item
                                MiniItem mi = Items[miIndex];
                                if (mi.Outcomes.ContainsKey(tmpContactId))
                                {//additional outcomes for an existing coder
                                    mi.Outcomes[tmpContactId].Add(new MaxiOutcome(reader.GetBoolean("Completed"), reader.GetString("ContactName"), reader));
                                }
                                else
                                {//new coder with new outcome value to add...
                                    List<MaxiOutcome> newVal = new List<MaxiOutcome>();
                                    newVal.Add(new MaxiOutcome(reader.GetBoolean("Completed"), reader.GetString("ContactName"), reader));
                                    KeyValuePair<int, List<MaxiOutcome>> kvp = new KeyValuePair<int, List<MaxiOutcome>>(tmpContactId, new List<MaxiOutcome>());
                                    mi.Outcomes.Add(tmpContactId, newVal);
                                }
                            }
                        }
                    }
                }
                connection.Close();
            }
            Attributes = Attributes.FindAll(f => f.IsInReport == true);
            if (_buildReport) BuildReport();
        }
        private void BuildReport()
        {
            _result = "<HTML><head><title>Full Comparison Report</title>";
            string commonstyle = @"<style>
                                    br { mso-data-placement: same-cell;}  
                                    .complete {color: green; font-weight: bold;} 
                                    .codedText {font-family: 'Courier New', monospace; font-size: 0.9em;}
                                    table, th, td { border: 1px solid black;  border-collapse: collapse; }                              
                                   </style>";
            _result += commonstyle + "</head><body>";
            if (Attributes.Count > 0 && Items.Count > 0)
            {
                StringBuilder sb = new StringBuilder("<table><tr><th>ItemId</th><th>ShortTitle</th><th>Title</th><th>I/E/D/S flag</th>");
                //_result += "<p>This is a Comparison report for all codings in this Set</p>"
                //        + "<table><tr><td>ItemId</td><td>ShortTitle</td><td>Title</td>";
                string secondTableLine = "<tr><td colspan='4'><div style='display: flex; justify-content: flex-end; margin-right:0.25em;'>FullPath:</div></td>";

                foreach (MiniAtt a in Attributes)
                {
                    sb.Append("<th>" + a.AttName + "</th>");
                    secondTableLine += "<td>" + a.FullPath + "</td>";
                }
                sb.Append("</tr>" + secondTableLine + "</tr>");
                //done the headers, on with the list of items...
                foreach (MiniItem itm in Items)
                {
                    sb.Append("<tr><td>" + itm.ItemId.ToString() + "</td><td>" + itm.ShortTitle + "</td><td>" + itm.Title + "</td><td>" + itm.State + "</td>");
                    foreach (MiniAtt a in Attributes)
                    {
                        if (itm.Codings.ContainsKey(a))
                        {
                            sb.Append("<td>");
                            //string restofline = "<td>";
                            foreach (MiniCoding mc in itm.Codings[a])
                            {
                                sb.Append("<span " + (mc.IsComplete ? "class='complete'>" : ">") + mc.ContactName);
                                if (mc.ArmName != "") sb.Append(" [Arm: " + mc.ArmName + "] ");
                                if (mc.InfoBox != "")
                                {
                                    sb.Append("<br />[info]: " + mc.InfoBox);
                                }
                                if (mc.PDF.Count > 0)
                                {
                                    foreach (MiniPDFatt mpa in mc.PDF)
                                    {
                                        sb.Append("<br />[text]: <span class='codedText'>" + mpa.Text + "</span> (" + mpa.DocName + ", page " + mpa.Page.ToString() + ")");
                                    }
                                }
                                sb.Append("</span><br />");
                            }
                            sb.Remove(sb.Length - 6, 6);
                            //restofline = restofline.Substring(0, restofline.Length - 6);
                            //_result += restofline + "</td>";
                            sb.Append("</td>");
                        }
                        else
                        {//there is no coding for this item in this code
                            sb.Append("<td></td>");
                        }
                    }
                    sb.Append("</tr>");
                }
                if (HasOutcomes)
                {//adding a second outcomes table!
                    _result += sb.Append("</table><br /> Outcomes:<br />"
                        + "<table><tr><th>ItemId</th><th>ShortTitle</th><th>Coder</th><th>IsComplete</th>"
                        + "<th>Outcome title</th><th>Outcome description</th><th>Timepoint</th><th>Outcome</th><th>Intervention</th><th>Comparison</th><th>Arm 1</th><th>Arm 2</th><th>Outcome type</th>"
                        + "<th>Data 1</th><th>Data 2</th><th>Data 3</th><th>Data 4</th><th>Data 5</th><th>Data 6</th><th>Data 7</th><th>Data 8</th><th>Data 9</th><th>Data 10</th>"
                        + "<th>Data 11</th><th>Data 12</th><th>Data 13</th><th>Data 14</th><th>ES</th><th>SE</th><th>Outcome Codes</th></tr>");
                    List<MiniItem> itemsWithOutcomes = Items.FindAll(f => f.Outcomes.Count > 0);
                    foreach (MiniItem mi in itemsWithOutcomes)
                    {
                        foreach (KeyValuePair<int, List<MaxiOutcome>> kvp in mi.Outcomes)
                        {
                            foreach (MaxiOutcome mxo in kvp.Value)
                            {
                                sb.Append("<tr><td>" + mi.ItemId.ToString() + "</td><td>" + mi.ShortTitle + "</td><td>"
                                    + mxo.ContactName + "</td><td>" + (mxo.IsComplete ? "<span class='complete'>Yes</span>" : "No") + "</td><td>" + mxo.Outcome.Title + "</td><td>" + mxo.Outcome.OutcomeDescription.Replace("\r", "<br />")
                                    + "</td><td>" + mxo.Outcome.TimepointDisplayValue + "</td><td>" + mxo.Outcome.OutcomeText + "</td><td>" + mxo.Outcome.InterventionText
                                    + "</td><td>" + mxo.Outcome.ControlText + "</td><td>" + mxo.Outcome.grp1ArmName + "</td><td>" + mxo.Outcome.grp2ArmName);
                                switch (mxo.Outcome.OutcomeTypeId)
                                {
                                    case 0: // manual entry
                                        sb.Append("<td>Manual entry</td>");
                                        break;

                                    case 1: // n, mean, SD
                                        sb.Append("<td>Continuous: Ns, means and SD</td>");
                                        break;

                                    case 2: // binary 2 x 2 table
                                        sb.Append("<td>Binary: 2 x 2 table</td>");
                                        break;

                                    case 3: //n, mean SE
                                        sb.Append("<td>Continuous: N, Mean, SE</td>");
                                        break;

                                    case 4: //n, mean CI
                                        sb.Append("<td>Continuous: N, Mean, CI</td>");
                                        break;

                                    case 5: //n, t or p value
                                        sb.Append("<td>Continuous: N, t- or p-value</td>");
                                        break;

                                    case 6: // binary 2 x 2 table
                                        sb.Append("<td>Diagnostic test: 2 x 2 table</td>");
                                        break;

                                    case 7: // correlation coefficient r
                                        sb.Append("<td>Correlation coefficient r</td>");
                                        break;

                                    default:
                                        sb.Append("<td>N/A</td>");
                                        break;
                                }
                                sb.Append("<td>" + mxo.Outcome.Data1.ToString("G3") + "</td>" +
                                            "<td>" + mxo.Outcome.Data2.ToString("G3") + "</td>" +
                                            "<td>" + mxo.Outcome.Data3.ToString("G3") + "</td>" +
                                            "<td>" + mxo.Outcome.Data4.ToString("G3") + "</td>" +
                                            "<td>" + mxo.Outcome.Data5.ToString("G3") + "</td>" +
                                            "<td>" + mxo.Outcome.Data6.ToString("G3") + "</td>" +
                                            "<td>" + mxo.Outcome.Data7.ToString("G3") + "</td>" +
                                            "<td>" + mxo.Outcome.Data8.ToString("G3") + "</td>" +
                                            "<td>" + mxo.Outcome.Data9.ToString("G3") + "</td>" +
                                            "<td>" + mxo.Outcome.Data10.ToString("G3") + "</td>" +
                                            "<td>" + mxo.Outcome.Data11.ToString("G3") + "</td>" +
                                            "<td>" + mxo.Outcome.Data12.ToString("G3") + "</td>" +
                                            "<td>" + mxo.Outcome.Data13.ToString("G3") + "</td>" +
                                            "<td>" + mxo.Outcome.Data14.ToString("G3") + "</td>" +
                                            "<td>" + mxo.Outcome.ES.ToString("G3") + "</td>" +
                                            "<td>" + mxo.Outcome.SEES.ToString("G3") + "</td><td>");
                                foreach (OutcomeItemAttribute OIA in mxo.Outcome.OutcomeCodes)
                                {
                                    sb.Append(OIA.AttributeName + "<br>");
                                }
                                sb.Append("</td></tr>");
                            }
                        }
                    }
                }
                _result += sb.Append("</table></body></html>").ToString();
            }
            else _result = "No data to report</body></html>";
        }
        public class MiniAtt
        {
            public MiniAtt(long attId, string attName, string fullPath)
            {
                AttId = attId;
                AttName = attName;
                FullPath = fullPath;
                IsInReport = false;
                //Codings = new Dictionary<MiniItem, List<MiniCoding>>();
            }
            public long AttId;
            public string AttName;
            public string FullPath;
            public bool IsInReport;
        }
        public class MiniCoding 
        {
            public long ItemAttId;
            public string ContactName;
            public int ContactId;
            public string ArmName;
            public bool IsComplete;
            public string InfoBox;
            public MiniCoding(SafeDataReader reader) 
            {
                ItemAttId = reader.GetInt64("ITEM_ATTRIBUTE_ID");
                ContactName = reader.GetString("ContactName"); 
                ContactId = reader.GetInt32("ContactId");
                IsComplete = reader.GetBoolean("Completed");
                InfoBox = reader.GetString("ADDITIONAL_TEXT");
                ArmName = reader.GetString("ARM_NAME");
                PDF = new List<MiniPDFatt>();
            }
            public List<MiniPDFatt> PDF;//key is the ItemAttribute
        }
        public class MiniPDFatt
        {
            public MiniPDFatt(string docName, int page, string text)
            {
                DocName = docName;
                Page = page;
                Text = text;
            }
            public string DocName;
            public int Page;
            public string Text;

        }
        public class MiniItem
        {
            public MiniItem(long Id, string title, string shortTitle, string state)
            {
                ItemId = Id;
                Title = title;
                ShortTitle = shortTitle;
                State = state;
                Codings = new Dictionary<MiniAtt, List<MiniCoding>>();
                Outcomes = new Dictionary<int, List<MaxiOutcome>>();
            }
            public long ItemId;
            public string Title;
            public string ShortTitle;
            public string State;
            [NonSerialized()]
            public Dictionary<MiniAtt, List<MiniCoding>> Codings;
            public Dictionary<int, List<MaxiOutcome>> Outcomes; //int is the contact ID
            public List<KeyValuePair<MiniAtt, List<MiniCoding>>> CodingsList 
            {
                get
                {
                    return this.Codings.ToList();
                }
            }
        }
        public class MaxiOutcome
        {
            public bool IsComplete;
            public string ContactName;
            public Outcome Outcome;
            public MaxiOutcome(bool isComplete, string contactName, Csla.Data.SafeDataReader reader)
            {
                IsComplete = isComplete;
                ContactName = contactName;
                Outcome = Outcome.GetOutcome(reader);
            }
        }
#endif
    }
    
}
