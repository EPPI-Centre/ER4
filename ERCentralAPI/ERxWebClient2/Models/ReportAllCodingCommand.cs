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

#if!SILVERLIGHT
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


        public ReportAllCodingCommand(int setId)
        {
            _result = "";
            _setId = setId;
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
        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            _result = info.GetValue<string>("_result");
            _setId = info.GetValue<int>("_setId");
        }


#if !SILVERLIGHT

        protected override void DataPortal_Execute()
        {
            List<MiniAtt> atts = new List<MiniAtt>();
            List<MiniItem> Items = new List<MiniItem>();
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ReportAllCodingCommand", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@ReviewId", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@SetId", _setId));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        while (reader.Read())
                        {//the list of attributes in this set
                            atts.Add(
                                new MiniAtt( reader.GetInt64("a_id"), reader.GetString("a_name"), reader.GetString("full_path") )
                               );
                        }
                        reader.NextResult();//coding data...
                        MiniItem tmpItem = new MiniItem(-1, "", "");
                        while (reader.Read()) 
                        {
                            long tmpId = reader.GetInt64("ItemId");
                            if (tmpItem.ItemId != tmpId)//we've encountered a new item...
                            {
                                if (tmpItem.ItemId > 0)
                                {
                                    Items.Add(tmpItem);
                                }
                                tmpItem = new MiniItem(tmpId, reader.GetString("TITLE"), reader.GetString("SHORT_TITLE"));
                            }
                            long tmpAttId = reader.GetInt64("ATTRIBUTE_ID");
                            MiniAtt CurrAtt = atts.Find(f => f.AttId == tmpAttId);
                            if (CurrAtt == null) continue;//shouldn't really happen, but adding this for safety...
                            CurrAtt.IsInReport = true;
                            if (tmpItem.Codings.ContainsKey(CurrAtt))
                            {//we'll add the coding for this attribute to this item
                                tmpItem.Codings[CurrAtt].Add(new MiniCoding(reader.GetInt64("ITEM_ATTRIBUTE_ID"), reader.GetString("ContactName"), reader.GetBoolean("Completed"), reader.GetString("ADDITIONAL_TEXT"), reader.GetString("ARM_NAME")));
                            }
                            else
                            {// add new element in the dictionary, we don't have coding for this attribute in the current item...
                                List<MiniCoding> vals = new List<MiniCoding>();
                                vals.Add(new MiniCoding(reader.GetInt64("ITEM_ATTRIBUTE_ID"), reader.GetString("ContactName"), reader.GetBoolean("Completed"), reader.GetString("ADDITIONAL_TEXT"), reader.GetString("ARM_NAME")));
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
                    }
                }
                connection.Close();
            }
            atts = atts.FindAll(f => f.IsInReport == true);
            _result = "<HTML><head><title>Full Comparison Report</title>";
            string commonstyle = @"<style>
                                    br { mso-data-placement: same-cell;}  
                                    .complete {color: green; font-weight: bold;} 
                                    .codedText {font-family: 'Courier New', monospace; font-size: 0.9em;}
                                    table, th, td { border: 1px solid black;  border-collapse: collapse; }                              
                                   </style>";
            _result += commonstyle + "</head><body>";
            if (atts.Count > 0 && Items.Count > 0)
            {
                StringBuilder sb = new StringBuilder("<p>This is a Comparison report for all codings in this Set</p>"
                        + "<table><tr><th>ItemId</th><th>ShortTitle</th><th>Title</th>");
                //_result += "<p>This is a Comparison report for all codings in this Set</p>"
                //        + "<table><tr><td>ItemId</td><td>ShortTitle</td><td>Title</td>";
                string secondTableLine = "<tr><td></td><td></td><td>FullPath:</td>";
                foreach (MiniAtt a in atts)
                {
                    sb.Append("<th>" + a.AttName + "</th>");
                    secondTableLine += "<td>" + a.FullPath + "</td>";
                }
                sb.Append("</tr>" + secondTableLine + "</tr>");
                //done the headers, on with the list of items...
                foreach (MiniItem itm in Items)
                {
                    sb.Append("<tr><td>" + itm.ItemId.ToString() + "</td><td>" + itm.ShortTitle + "</td><td>" + itm.Title + "</td>");
                    foreach (MiniAtt a in atts)
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
                                    foreach(MiniPDFatt mpa in mc.PDF)
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
                            sb.Append("<td>.</td>");
                        }
                    }
                    sb.Append("</tr>");
                }
                _result += sb.Append("</table></body></html>").ToString();
            }
            else _result = "No data to report</body></html>";
        }
        private class MiniAtt
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
            //public bool IsInReport {
            //    get {
            //        return Codings.Count > 0;
            //    } 
            //}
            //public Dictionary<MiniItem, List<MiniCoding>> Codings;
        }
        private class MiniCoding 
        {
            public long ItemAttId;
            public string ContactName;
            public string ArmName;
            public bool IsComplete;
            public string InfoBox;
            public MiniCoding(long itemAttId, string contact, bool isComplete, string infoBox, string armName) 
            {
                ItemAttId = itemAttId;
                ContactName = contact;
                IsComplete = isComplete;
                InfoBox = infoBox;
                ArmName = armName;
                PDF = new List<MiniPDFatt>();
            }
            public List<MiniPDFatt> PDF;//key is the ItemAttribute
        }
        private class MiniPDFatt
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
        private class MiniItem
        {
            public MiniItem(long Id, string title, string shortTitle)
            {
                ItemId = Id;
                Title = title;
                ShortTitle = shortTitle;
                Codings = new Dictionary<MiniAtt, List<MiniCoding>>();
            }
            public long ItemId;
            public string Title;
            public string ShortTitle;
            public Dictionary<MiniAtt, List<MiniCoding>> Codings;
        }
#endif
    }
    
}
