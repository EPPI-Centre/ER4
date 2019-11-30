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
using System.Data;
#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class ReportExecuteCommand : CommandBase<ReportExecuteCommand>
    {
#if SILVERLIGHT
    public ReportExecuteCommand(){}
#else
        public ReportExecuteCommand() { }
#endif

        private string _report_type;
        private string _codes;
        private int _report_id;
        private string _return_report;
        private bool _show_itemId;
        private bool _show_old_itemId;
        private bool _show_outcomes;
        private bool _is_horizontal;
        private string _order_by;
        private string _report_title;
        private Int64 _attribute_id;
        private int _set_id;

        public ReportExecuteCommand(string reportType, string codes, int reportId, bool showItemId, bool showOldItemId,
            bool showOutcomes, bool isHorizontal, string orderBy, string title, Int64 attributeId, int setId)
        {
            _report_type = reportType;
            _codes = codes;
            _report_id = reportId;
            _return_report = "";
            _show_itemId = showItemId;
            _show_old_itemId = showOldItemId;
            _show_outcomes = showOutcomes;
            _is_horizontal = isHorizontal;
            _order_by = orderBy;
            _report_title = title;
            _attribute_id = attributeId;
            _set_id = setId;
        }

        public string ReturnReport
        {
            get { return _return_report; }
        }

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_report_type", _report_type);
            info.AddValue("_codes", _codes);
            info.AddValue("_report_id", _report_id);
            info.AddValue("_return_report", _return_report);
            info.AddValue("_show_itemId", _show_itemId);
            info.AddValue("_show_old_itemId", _show_old_itemId);
            info.AddValue("_show_outcomes", _show_outcomes);
            info.AddValue("_is_horizontal", _is_horizontal);
            info.AddValue("_order_by", _order_by);
            info.AddValue("_report_title", _report_title);
            info.AddValue("_attribute_id", _attribute_id);
            info.AddValue("_set_id", _set_id);
        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            _report_type = info.GetValue<string>("_report_type");
            _codes = info.GetValue<string>("_codes");
            _report_id = info.GetValue<int>("_report_id");
            _return_report = info.GetValue<string>("_return_report");
            _show_itemId = info.GetValue<bool>("_show_itemId");
            _show_old_itemId = info.GetValue<bool>("_show_old_itemId");
            _show_outcomes = info.GetValue<bool>("_show_outcomes");
            _is_horizontal = info.GetValue<bool>("_is_horizontal");
            _order_by = info.GetValue<string>("_order_by");
            _report_title = info.GetValue<string>("_report_title");
            _attribute_id = info.GetValue<Int64>("_attribute_id");
            _set_id = info.GetValue<int>("_set_id");
        }

#if !SILVERLIGHT

        protected override void DataPortal_Execute()
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ReportExecute", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    if (_report_type != "Question")
                    {
                        if (_show_outcomes)
                        {
                            command.CommandText = "st_ReportExecuteSingleWithOutcomes";
                        }
                        else
                        {
                            command.CommandText = "st_ReportExecuteSingleWithoutOutcomes";
                        }
                    }
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@ITEM_IDS", _codes));
                    command.Parameters.Add(new SqlParameter("@REPORT_ID", _report_id));
                    command.Parameters.Add(new SqlParameter("@ORDER_BY", _order_by));
                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ID", _attribute_id));
                    command.Parameters.Add(new SqlParameter("@SET_ID", _set_id));
                    _return_report = "";
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        if (_report_type == "Question")
                        {
                            if (_is_horizontal == true)
                            {
                                HorizontalReport(reader);
                            }
                            else
                            {
                                VerticalReport(reader);
                            }
                        }
                        else
                        {
                            if (_show_outcomes)
                            {
                                SingleReportOutcomes(reader);
                            }
                            else
                            {
                                SingleReportWithoutOutcomes(reader);
                            }
                        }
                    }
                }
                connection.Close();
            }
        }

        protected void HorizontalReport(Csla.Data.SafeDataReader reader)
        {
            ReportColumnList rcl = ReportColumnList.NewReportColumnList();
            _return_report = "";
            while (reader.Read())
            {
                ReportColumn newReportColumn = ReportColumn.GetReportColumn(reader);
                rcl.Add(newReportColumn);
            }
            if (rcl.Count > 0)
            {
                int columnCount = 0;
                _return_report = "<p align='center'><h1>" + _report_title + "</h1></p><table border='1'><tr>";
                if (_show_itemId == true)
                {
                    _return_report += "<td>Item Id</td>";
                }
                if (_show_old_itemId == true)
                {
                    _return_report += "<td>Imported Id</td>";
                }
                _return_report += "<td>Title</td>";
                foreach (ReportColumn rc in rcl)
                {
                    _return_report += "<td valign='top'>" + rc.Name + "</td>";
                    columnCount++;
                }

                reader.NextResult();
                int currentColumn = 0;
                Int64 currentItemId = 0;
                string currentCell = "";
                string currentRow = "";
                string currentParentAttribute = "";
                while (reader.Read())
                {
                    // Set up new rows
                    if (Convert.ToInt64(reader["ITEM_ID"].ToString()) != currentItemId)
                    {
                        // finish off current cell if needed
                        if (currentCell != "")
                        {
                            currentRow += "<td valign='top'>" + currentCell + "</td>";
                            currentColumn++;

                            while ((currentColumn <= columnCount ) &&
                            (currentColumn < rcl.Count)) // <-- shouldn't be needed, but to avoid infinite loops
                            {
                                currentRow += "<td>&nbsp</td>";
                                currentColumn++;
                            }
                        }
                        // finish current row (first one finishes off the header row)
                        currentRow += "</tr>";
                        // add it to report
                        _return_report += currentRow;
                        // begin a new row
                        currentRow = "";
                        _return_report += "<tr>";
                        // add a cell with the item ids / title etc
                        if (_show_itemId == true)
                        {
                            _return_report += "<td valign='top'>" + reader["ITEM_ID"].ToString() + "</td>";
                        }
                        if (_show_old_itemId == true)
                        {
                            _return_report += "<td valign='top'>" + reader["OLD_ITEM_ID"].ToString() + "</td>";
                        }
                        _return_report += "<td valign='top'>" + reader["SHORT_TITLE"].ToString() + "</td>";
                        // reset column to first
                        currentColumn = 0;
                        currentCell = "";
                        currentParentAttribute = "";
                        currentItemId = Convert.ToInt64(reader["ITEM_ID"].ToString());
                    }

                    if (currentColumn != Convert.ToInt32(reader["COLUMN_ORDER"].ToString()))
                    {
                        currentRow += "<td valign='top'>" + currentCell + "</td>";
                        currentCell = "";
                        currentParentAttribute = "";
                        currentColumn++;

                        while ((currentColumn != Convert.ToInt32(reader["COLUMN_ORDER"].ToString())) &&
                            (currentColumn < rcl.Count)) // <-- shouldn't be needed, but to avoid infinite loops
                        {
                            currentRow += "<td>&nbsp</td>";
                            currentColumn++;
                        }
                    }
                    if (currentColumn < rcl.Count)
                    {
                        if (currentCell == "")
                        {
                            currentCell = "<p><b>" + reader["USER_DEF_TEXT"].ToString() + "</b></p>";
                            currentParentAttribute = reader["USER_DEF_TEXT"].ToString();
                            if (Convert.ToBoolean(reader["DISPLAY_CODE"].ToString()) == true)
                            {
                                string armText = reader["ARM_NAME"].ToString();
                                if (armText == "")
                                {
                                    currentCell += reader["ATTRIBUTE_NAME"].ToString();
                                }
                                else
                                {
                                    currentCell += reader["ATTRIBUTE_NAME"].ToString() + " [" + armText + "]";
                                }
                            }
                            if (Convert.ToBoolean(reader["DISPLAY_ADDITIONAL_TEXT"].ToString()) == true)
                            {
                                if (reader.IsDBNull("ADDITIONAL_TEXT")) { }
                                else
                                {

                                    currentCell += "<br /><i>" + MakeReportSafe(reader["ADDITIONAL_TEXT"].ToString()) + "</i>";
                                }
                            }
                            if (Convert.ToBoolean(reader["DISPLAY_CODED_TEXT"].ToString()) == true)
                            {
                                if (reader.IsDBNull("CODED_TEXT")) { }
                                else
                                {
                                    currentCell += "<br /><i>'" + MakeReportSafe(reader["CODED_TEXT"].ToString()) + "'</i>";
                                }
                            }
                        }
                        else
                        {
                            if (reader["USER_DEF_TEXT"].ToString() != currentParentAttribute)
                            {
                                currentParentAttribute = reader["USER_DEF_TEXT"].ToString();
                                currentCell += "<p><b>" + reader["USER_DEF_TEXT"].ToString() + "</b></p>";
                            }
                            if (Convert.ToBoolean(reader["DISPLAY_CODE"].ToString()) == true)
                            {
                                string armText = reader["ARM_NAME"].ToString();
                                if (armText == "")
                                {
                                    currentCell += "<p>" + reader["ATTRIBUTE_NAME"].ToString();
                                }
                                else
                                {
                                    currentCell += "<p>" + reader["ATTRIBUTE_NAME"].ToString() + " [" + armText + "]";
                                }
                            }
                            if (Convert.ToBoolean(reader["DISPLAY_ADDITIONAL_TEXT"].ToString()) == true )
                            {
                                if (reader.IsDBNull("ADDITIONAL_TEXT")) { }
                                else
                                {
                                    currentCell += "</br><i>" + MakeReportSafe(reader["ADDITIONAL_TEXT"].ToString()) + "</i>";
                                }
                            }
                            if (Convert.ToBoolean(reader["DISPLAY_CODED_TEXT"].ToString()) == true)
                            {
                                if (reader.IsDBNull("CODED_TEXT")) { }
                                else
                                {
                                    currentCell += "</br><i>'" + MakeReportSafe(reader["CODED_TEXT"].ToString()) + "'</i>";
                                }
                            }
                            currentCell += "</p>";
                        }
                    }
                }
                // finish off current cell if needed
                if (currentCell != "")
                {
                    currentRow += "<td valign='top'>" + currentCell + "</td>";
                    currentColumn++;

                    while ((currentColumn <= columnCount) &&
                    (currentColumn < rcl.Count)) // <-- shouldn't be needed, but to avoid infinite loops
                    {
                        currentRow += "<td>&nbsp</td>";
                        currentColumn++;
                    }
                }
                currentRow += "</tr>";
                _return_report += currentRow;
                _return_report += "</table>";
            }
        }

        protected void VerticalReport(Csla.Data.SafeDataReader reader)
        {
            _return_report = "<p align='center'><h1>" + _report_title + "</h1></p><table cellpadding='5'>";
            reader.NextResult();
            string currentColumn = "";
            Int64 currentItemId = 0;
            string currentCell = "";
            while (reader.Read())
            {
                if (Convert.ToInt64(reader["ITEM_ID"].ToString()) != currentItemId)
                {
                    if (currentCell != "")
                    {
                        _return_report += currentCell + "</td></tr>";
                    }
                    _return_report += "<tr><td>&nbsp</td></tr><tr><td valign='top'><h2>Item</h2></td><td valign='top'><h2>" +
                        reader["SHORT_TITLE"].ToString() + "</h2></td></tr>";

                    if (_show_itemId == true)
                    {
                        _return_report += "<tr><td>Item Id</td><td>" + reader["ITEM_ID"].ToString() + "</td></tr>";
                    }
                    if (_show_old_itemId == true)
                    {
                        _return_report += "<tr><td>Imported Id</td><td>" + reader["OLD_ITEM_ID"].ToString() + "</td></tr>";
                    }

                    currentColumn = "";
                    currentCell = "";
                    currentItemId = Convert.ToInt64(reader["ITEM_ID"].ToString());
                }

                if (currentColumn != reader["REPORT_COLUMN_NAME"].ToString())
                {
                    if (currentCell != "")
                    {
                        _return_report += currentCell + "</td></tr>";
                    }

                    currentCell = "<tr><td valign='top'>" + reader["REPORT_COLUMN_NAME"].ToString() + "</td>";
                    currentColumn = reader["REPORT_COLUMN_NAME"].ToString();

                    currentCell += "<td valign='top'><b>" + reader["USER_DEF_TEXT"].ToString() + "</b>";
                    if (Convert.ToBoolean(reader["DISPLAY_CODE"].ToString()) == true)
                    {
                        string armText = reader["ARM_NAME"].ToString();
                        if (armText == "")
                        {
                            currentCell += "<p>" + reader["ATTRIBUTE_NAME"].ToString();
                        }
                        else
                        {
                            currentCell += "<p>" + reader["ATTRIBUTE_NAME"].ToString() + " [" + armText + "]";
                        }
                    }
                    if (Convert.ToBoolean(reader["DISPLAY_ADDITIONAL_TEXT"].ToString()) == true)
                    {
                        if (reader.IsDBNull("ADDITIONAL_TEXT")) { }
                        else
                        {
                            currentCell += "<i> " + MakeReportSafe(reader["ADDITIONAL_TEXT"].ToString()) + "</i>";
                        }
                    }
                    if (Convert.ToBoolean(reader["DISPLAY_CODED_TEXT"].ToString()) == true)
                    {
                        if (reader.IsDBNull("CODED_TEXT")) { }
                        else
                        {
                            currentCell += "<i> '" + MakeReportSafe(reader["CODED_TEXT"].ToString()) + "'</i>";
                        }
                    }
                }
                else
                {
                    currentCell += "<br /><b>" + reader["USER_DEF_TEXT"].ToString() + "</b>";
                    if (Convert.ToBoolean(reader["DISPLAY_CODE"].ToString()) == true)
                    {
                        string armText = reader["ARM_NAME"].ToString();
                        if (armText == "")
                        {
                            currentCell += "<p>" + reader["ATTRIBUTE_NAME"].ToString();
                        }
                        else
                        {
                            currentCell += "<p>" + reader["ATTRIBUTE_NAME"].ToString() + " [" + armText + "]";
                        }
                    }
                    if (Convert.ToBoolean(reader["DISPLAY_ADDITIONAL_TEXT"].ToString()) == true)
                    {
                        if (reader.IsDBNull("ADDITIONAL_TEXT")) { }
                        else
                        {
                            currentCell += "<i> " + MakeReportSafe(reader["ADDITIONAL_TEXT"].ToString()) + "</i>";
                        }
                    }
                    if (Convert.ToBoolean(reader["DISPLAY_CODED_TEXT"].ToString()) == true)
                    {
                        if (reader.IsDBNull("CODED_TEXT")) { }
                        else
                        {
                            currentCell += "<i> '" + MakeReportSafe(reader["CODED_TEXT"].ToString()) + "'</i>";
                        }
                    }
                }
            }
            _return_report += currentCell + "</td></tr></table>";
        }

        protected void SingleReportOutcomes(Csla.Data.SafeDataReader reader)
        {
            ReportColumnList rcl = ReportColumnList.NewReportColumnList();
            _return_report = "";

            DataTable dt = new DataTable();
            DataColumn dc = null;
            if (_show_itemId == true)
            {
                dt.Columns.Add(new DataColumn("Item Id", System.Type.GetType("System.String")));
            }
            if (_show_old_itemId == true)
            {
                dt.Columns.Add(new DataColumn("Imported Id", System.Type.GetType("System.String")));
            }
            dt.Columns.Add(new DataColumn("Title", System.Type.GetType("System.String")));
            

            int attributeCount = 0;
            int outcomeAttributeCount = 0;
            while (reader.Read())
            {
                ReportColumn newReportColumn = ReportColumn.GetReportColumn(reader);
                rcl.Add(newReportColumn);
                if (newReportColumn.Codes.Count > 0)
                {
                    dc = new DataColumn(newReportColumn.Codes[0].ParentAttributeText +"¬|¬" + newReportColumn.Codes[0].ReportColumnId.ToString() , System.Type.GetType("System.String"));
                    dt.Columns.Add(dc);
                }
                attributeCount++;
            }
            dt.Columns.Add(new DataColumn("Outcome description", System.Type.GetType("System.String")));
            dt.Columns.Add(new DataColumn("Timepoint", System.Type.GetType("System.String")));
            dt.Columns.Add(new DataColumn("Outcome type", System.Type.GetType("System.String")));
            dt.Columns.Add(new DataColumn("Outcome", System.Type.GetType("System.String")));
            dt.Columns.Add(new DataColumn("Intervention", System.Type.GetType("System.String")));
            dt.Columns.Add(new DataColumn("Comparison", System.Type.GetType("System.String")));
            dt.Columns.Add(new DataColumn("Arm 1", System.Type.GetType("System.String")));
            dt.Columns.Add(new DataColumn("Arm 2", System.Type.GetType("System.String")));
            dt.Columns.Add(new DataColumn("Data 1", System.Type.GetType("System.String")));
            dt.Columns.Add(new DataColumn("Data 2", System.Type.GetType("System.String")));
            dt.Columns.Add(new DataColumn("Data 3", System.Type.GetType("System.String")));
            dt.Columns.Add(new DataColumn("Data 4", System.Type.GetType("System.String")));
            dt.Columns.Add(new DataColumn("Data 5", System.Type.GetType("System.String")));
            dt.Columns.Add(new DataColumn("Data 6", System.Type.GetType("System.String")));
            dt.Columns.Add(new DataColumn("Data 7", System.Type.GetType("System.String")));
            dt.Columns.Add(new DataColumn("Data 8", System.Type.GetType("System.String")));
            dt.Columns.Add(new DataColumn("Data 9", System.Type.GetType("System.String")));
            dt.Columns.Add(new DataColumn("Data 10", System.Type.GetType("System.String")));
            dt.Columns.Add(new DataColumn("Data 11", System.Type.GetType("System.String")));
            dt.Columns.Add(new DataColumn("Data 12", System.Type.GetType("System.String")));
            dt.Columns.Add(new DataColumn("Data 13", System.Type.GetType("System.String")));
            dt.Columns.Add(new DataColumn("Data 14", System.Type.GetType("System.String")));
            dt.Columns.Add(new DataColumn("ES", System.Type.GetType("System.String")));
            dt.Columns.Add(new DataColumn("SE", System.Type.GetType("System.String")));
            if (rcl.Count > 0)
            {
                reader.NextResult();
                List<string> OutcomeAttributes = new List<string>();
                while (reader.Read())
                {
                    OutcomeAttributes.Add(reader["ATTRIBUTE_NAME"].ToString());
                    dc = new DataColumn(reader["ATTRIBUTE_NAME"].ToString(), System.Type.GetType("System.String"));
                    dt.Columns.Add(dc);
                    outcomeAttributeCount++;
                }
                reader.NextResult();
                string currentOutcome = "-1";
                DataRow dr = null;
                while (reader.Read())
                {
                    if (reader["OUTCOME_ID"].ToString() + reader["ITEM_ID"].ToString() != currentOutcome)
                    {
                        if (dr != null)
                        {
                            dt.Rows.Add(dr);
                        }
                        dr = dt.NewRow();
                        currentOutcome = reader["OUTCOME_ID"].ToString() + reader["ITEM_ID"].ToString();
                        dr["Title"] = reader["SHORT_TITLE"].ToString();
                        if (_show_itemId == true)
                        {
                            dr["Item Id"] = reader["ITEM_ID"].ToString();
                        }
                        if (_show_old_itemId == true)
                        {
                            dr["Imported Id"] = reader["OLD_ITEM_ID"].ToString();
                        }
                        dr["Outcome description"] = reader["OUTCOME_TITLE"].ToString();

                        string OutcomeId = reader.GetValue("OUTCOME_ID").ToString();
                        Outcome o = Outcome.GetSingleOutcome(Convert.ToInt32(OutcomeId));
                        dr["Outcome description"] = o.Title;
                        dr["Timepoint"] = o.TimepointDisplayValue;
                        dr["Outcome"] = o.OutcomeText;
                        dr["Intervention"] = o.InterventionText;
                        dr["Comparison"] = o.ControlText;
                        dr["Outcome type"] = o.OutcomeTypeName;

                        dr["Arm 1"] = o.grp1ArmName;
                        dr["Arm 2"] = o.grp2ArmName;

                        dr["Data 1"] = o.Data1;
                        dr["Data 2"] = o.Data2;
                        dr["Data 3"] = o.Data3;
                        dr["Data 4"] = o.Data4;
                        dr["Data 5"] = o.Data5;
                        dr["Data 6"] = o.Data6;
                        dr["Data 7"] = o.Data7;
                        dr["Data 8"] = o.Data8;
                        dr["Data 9"] = o.Data9;
                        dr["Data 10"] = o.Data10;
                        dr["Data 11"] = o.Data11;
                        dr["Data 12"] = o.Data12;
                        dr["Data 13"] = o.Data13;
                        dr["Data 14"] = o.Data14;
                        dr["ES"] = o.ES;
                        dr["SE"] = o.SEES;
                    }

                    if (reader.GetValue("ATTRIBUTE_NAME") != null)
                    {//newReportColumn.Codes[0].ParentAttributeText +"¬|¬" + newReportColumn.Codes[0].ReportColumnId.ToString() 
                        dr[reader["ATTRIBUTE_NAME"].ToString() + "¬|¬" + reader["REPORT_COLUMN_ID"].ToString()] = "1";
                    }
                    if (reader.GetValue("OUTCOME_ATTRIBUTE") != null)
                    {
                        dr[reader["OUTCOME_ATTRIBUTE"].ToString()] = "1";
                    }
                }
                if (dr != null && dt.Rows.IndexOf(dr) == -1)
                {
                    dt.Rows.Add(dr);
                }
                _return_report = "<p align='center'><h1>" + _report_title + "</h1></p><table border='1'><tr>";
                
                foreach (DataColumn dc1 in dt.Columns)
                {
                    if (dc1.ColumnName.Contains("¬|¬"))
                    {
                        _return_report += "<td valign='top'>" + dc1.ColumnName.Substring(0, dc1.ColumnName.IndexOf("¬|¬")).Trim() + "</td>";
                    }
                    else
                    {
                        _return_report += "<td valign='top'>" + dc1.ColumnName + "</td>";
                    }
                }
                _return_report += "</tr>";
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string row = "<tr>";
                    for (int z = 0; z < dt.Columns.Count; z++)
                    {
                        row += "<td>" + dt.Rows[i][z].ToString() + "</td>";
                    }
                    _return_report += row + "</tr>";
                }
                _return_report += "</table>";
            }
        }

        protected void SingleReportWithoutOutcomes(Csla.Data.SafeDataReader reader)
        {
            ReportColumnList rcl = ReportColumnList.NewReportColumnList();
            _return_report = "";

            DataTable dt = new DataTable();
            DataColumn dc = null;
            if (_show_itemId == true)
            {
                dt.Columns.Add(new DataColumn("Item Id", System.Type.GetType("System.String")));
            }
            if (_show_old_itemId == true)
            {
                dt.Columns.Add(new DataColumn("Imported Id", System.Type.GetType("System.String")));
            }
            dt.Columns.Add(new DataColumn("Title", System.Type.GetType("System.String")));

            while (reader.Read())
            {
                ReportColumn newReportColumn = ReportColumn.GetReportColumn(reader);
                rcl.Add(newReportColumn);
                if (newReportColumn.Codes.Count > 0)
                {
                    dc = new DataColumn(newReportColumn.Codes[0].ParentAttributeText, System.Type.GetType("System.String"));
                    dt.Columns.Add(dc);
                }
            }
            if (rcl.Count > 0)
            {
                reader.NextResult();
                string currentItem = "-1";
                DataRow dr = null;
                while (reader.Read())
                {
                    if (reader["ITEM_ID"].ToString() != currentItem)
                    {
                        if (dr != null)
                        {
                            dt.Rows.Add(dr);
                        }
                        dr = dt.NewRow();
                        currentItem = reader["ITEM_ID"].ToString();
                        dr["Title"] = reader["SHORT_TITLE"].ToString();
                        if (_show_itemId == true)
                        {
                            dr["Item Id"] = reader["ITEM_ID"].ToString();
                        }
                        if (_show_old_itemId == true)
                        {
                            dr["Imported Id"] = reader["OLD_ITEM_ID"].ToString();
                        }
                    }

                    if (reader.GetValue("ATTRIBUTE_NAME") != null)
                    {
                        
                        string temp = reader["USER_DEF_TEXT"].ToString() + "</b>";
                        if (Convert.ToBoolean(reader["DISPLAY_ADDITIONAL_TEXT"].ToString()) == true)
                        {
                            if (reader.IsDBNull("ADDITIONAL_TEXT")) { }
                            else
                            {
                                temp += "<i> " + MakeReportSafe(reader["ADDITIONAL_TEXT"].ToString()) + "</i>";
                            }
                        }
                        if (Convert.ToBoolean(reader["DISPLAY_CODED_TEXT"].ToString()) == true)
                        {
                            if (reader.IsDBNull("CODED_TEXT")) { }
                            else
                            {
                                temp += "<i> '" + MakeReportSafe(reader["CODED_TEXT"].ToString()) + "'</i>";
                            }
                        }
                        dr[reader["ATTRIBUTE_NAME"].ToString()] = temp;
                    }
                }
                if (dr != null && dt.Rows.IndexOf(dr) == -1)
                {
                    dt.Rows.Add(dr);
                }
                _return_report = "<p align='center'><h1>" + _report_title + "</h1></p><table border='1'><tr>";

                foreach (DataColumn dc1 in dt.Columns)
                {
                    _return_report += "<td valign='top'>" + dc1.ColumnName + "</td>";
                }
                _return_report += "</tr>";
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string row = "<tr>";
                    for (int z = 0; z < dt.Columns.Count; z++)
                    {
                        row += "<td>" + dt.Rows[i][z].ToString() + "</td>";
                    }
                    _return_report += row + "</tr>";
                }
                _return_report += "</table>";
            }
        }
        private string MakeReportSafe(string input)
        {
            input = System.Web.HttpUtility.HtmlEncode(input);
            input = input.Replace("\r\n", "\n");
            input = input.Replace("\r", "\n");
            input = input.Replace("\n", "<br />");
            return input;
        }

#endif
    }
}
