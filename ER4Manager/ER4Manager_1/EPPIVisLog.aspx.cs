using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Data;
using System.Configuration;
using System.Collections;
using System.Web.Security;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.IO;
using System.Data.SqlClient;
using Telerik.Web.UI;
using System.Drawing;
using System.Globalization;
using Telerik.Web.UI.Calendar;

public partial class EPPIVisLog : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (Utils.GetSessionString("Contact_ID") != null)
        {
            if (Utils.GetSessionString("IsAdm") == "True")
            {
                if (!IsPostBack)
                {
                    System.Web.UI.WebControls.Label lbl = (Label)Master.FindControl("lblPageTitle");
                    if (lbl != null)
                    {
                        lbl.Text = "EPPI-Vis";
                    }

                    Telerik.Web.UI.RadTabStrip radTs = (Telerik.Web.UI.RadTabStrip)Master.FindControl("rtsMenu");
                    if (radTs != null)
                    {
                        radTs.SelectedIndex = 5;
                        radTs.Tabs[5].Tabs[3].Selected = true;
                        radTs.Tabs[5].Tabs[3].Enabled = false;
                        radTs.Tabs[5].Tabs[5].Width = 550;

                        radTs.Tabs[5].Tabs[0].Visible = true;
                        radTs.Tabs[5].Tabs[1].Visible = true;
                        radTs.Tabs[5].Tabs[5].Visible = true;

                        if (Utils.GetSessionString("IsAdm") == "True")
                        {
                            radTs.Tabs[5].Tabs[2].Visible = true;
                            radTs.Tabs[5].Tabs[3].Visible = true;
                            radTs.Tabs[5].Tabs[4].Visible = true;
                            radTs.Tabs[5].Tabs[5].Width = 250;
                        }

                    }

                    if (Request.QueryString["webDB_ID"] != null)
                    {
                        lblWebDBID.Text = Request.QueryString["webDB_ID"];

                        bool isAdmDB = true;
                        SqlParameter[] paramList = new SqlParameter[4];
                        paramList[0] = new SqlParameter("@WEBDB_ID", SqlDbType.NVarChar, 10, ParameterDirection.Input,
                            true, 0, 0, null, DataRowVersion.Default, lblWebDBID.Text);
                        paramList[1] = new SqlParameter("@WEBDB_NAME", SqlDbType.NVarChar, 200, ParameterDirection.Output,
                            true, 0, 0, null, DataRowVersion.Default, "");
                        paramList[2] = new SqlParameter("@REVIEW_NAME", SqlDbType.NVarChar, 200, ParameterDirection.Output,
                            true, 0, 0, null, DataRowVersion.Default, "");
                        paramList[3] = new SqlParameter("@REVIEW_ID", SqlDbType.NVarChar, 10, ParameterDirection.Output,
                            true, 0, 0, null, DataRowVersion.Default, "");
                        Utils.ExecuteSPWithReturnValues(isAdmDB, Server, "st_EPPI_Vis_Name_and_Review", paramList);
                        lblWebDBName.Text = paramList[1].Value.ToString();
                        lblReviewName.Text = paramList[2].Value.ToString();
                        lblReviewID.Text = paramList[3].Value.ToString();

                        DateTime time1 = new DateTime(2021, 01, 01, 12, 12, 12);
                        RadTimePicker1.SelectedDate = time1;

                        DateTime time2 = DateTime.Now;
                        RadTimePicker2.SelectedDate = time2;
                        pnlRadTimePicker2.Visible = false;

                        loadLoggingData();
                    }
                }

            }
            else
            {
                Server.Transfer("Error.aspx");
            }
        }
        else
        {
            Server.Transfer("Error.aspx");
        }
    }

    private void loadLoggingData()
    {
        if (pnlRadTimePicker2.Visible == false)
        {
            DateTime time2 = new DateTime(1980, 01, 01, 00, 00, 00);
            RadTimePicker2.SelectedDate = time2;
        }
        
        DataTable dt = new DataTable();
        System.Data.DataRow newrow;

        dt.Columns.Add(new DataColumn("WEBDB_LOG_IDENTITY", typeof(int)));
        dt.Columns.Add(new DataColumn("CREATED", typeof(string)));
        dt.Columns.Add(new DataColumn("TYPE", typeof(string)));
        dt.Columns.Add(new DataColumn("DETAILS", typeof(string)));




        bool isAdmDB = true;
        IDataReader idr = Utils.GetReader(isAdmDB, "st_Eppi_Vis_Get_Log", lblWebDBID.Text,
            RadTimePicker1.SelectedDate, RadTimePicker2.SelectedDate, ddlTypes.SelectedItem.Text);
        while (idr.Read())
        {
            newrow = dt.NewRow();
            newrow["WEBDB_LOG_IDENTITY"] = idr["tv_webdb_log_identity"].ToString();
            newrow["CREATED"] = idr["tv_created"].ToString();
            newrow["TYPE"] = idr["tv_log_type"].ToString();
            newrow["DETAILS"] = idr["tv_details"].ToString();
            dt.Rows.Add(newrow);
        }
        idr.Close();

    }




    protected void radGVEPPIVisLog_NeedDataSource(object sender, Telerik.Web.UI.GridNeedDataSourceEventArgs e)
    {
        DataTable dt = new DataTable();
        System.Data.DataRow newrow;

        dt.Columns.Add(new DataColumn("WEBDB_LOG_IDENTITY", typeof(int)));
        dt.Columns.Add(new DataColumn("CREATED", typeof(string)));
        dt.Columns.Add(new DataColumn("TYPE", typeof(string)));
        dt.Columns.Add(new DataColumn("DETAILS", typeof(string)));

        bool isAdmDB = true;
        IDataReader idr = Utils.GetReader(isAdmDB, "st_Eppi_Vis_Get_Log", lblWebDBID.Text,
            RadTimePicker1.SelectedDate, RadTimePicker2.SelectedDate, ddlTypes.SelectedItem.Text);
        while (idr.Read())
        {
            newrow = dt.NewRow();
            newrow["WEBDB_LOG_IDENTITY"] = idr["tv_webdb_log_identity"].ToString();
            newrow["CREATED"] = idr["tv_created"].ToString();
            newrow["TYPE"] = idr["tv_log_type"].ToString();
            newrow["DETAILS"] = idr["tv_details"].ToString();
            dt.Rows.Add(newrow);
        }
        idr.Close();

        int test = dt.Rows.Count;
        radGVEPPIVisLog.DataSource = dt;

    }
    protected void radGVEPPIVisLog_PageIndexChanged(object sender, Telerik.Web.UI.GridPageChangedEventArgs e)
    {
        if (tbFilter.Text == "")
        {
            loadLoggingData();
        }
        else
        {
            radGVEPPIVisLog.Rebind();
        }
    }
    protected void RadAjaxManager1_AjaxRequest(object sender, Telerik.Web.UI.AjaxRequestEventArgs e)
    {
        if (e.Argument.IndexOf("FilterGrid") != -1)
        {
            radGVEPPIVisLog.Rebind();
        }
    }
    protected void radGVEPPIVisLog_ItemDataBound(object sender, Telerik.Web.UI.GridItemEventArgs e)
    {

    }

    protected void radGVEPPIVisLog_ItemCommand(object sender, Telerik.Web.UI.GridCommandEventArgs e)
    {

    }


    protected void RadTimePicker1_ItemCreated(object sender, TimePickerEventArgs e)
    {
        //EventLogConsole1.LoggedEvents.Add(String.Format("<b>ItemCreated</b> event fired: ItemIndex is - [{0}]; ItemType is - {1};<br />", e.Item.ItemIndex, e.Item.ItemType));
    }

    protected void RadTimePicker1_ItemDataBound(object sender, TimePickerEventArgs e)
    {
        //EventLogConsole1.LoggedEvents.Add(String.Format("<b>ItemDataBound</b> event fired: ItemIndex is - [{0}]; ItemType is - {1};<br />", e.Item.ItemIndex, e.Item.ItemType));
    }

    protected void RadTimePicker1_SelectedDateChanged(object sender, SelectedDateChangedEventArgs e)
    {
        //EventLogConsole1.LoggedEvents.Add("<b>SelectedDateChanged</b> event fired:<br />");
        //lblDate1.Text = RadTimePicker1.SelectedDate.ToString();
        radGVEPPIVisLog.Rebind();
    }

    protected void RadTimePicker2_ItemCreated(object sender, TimePickerEventArgs e)
    {
        //EventLogConsole1.LoggedEvents.Add(String.Format("<b>ItemCreated</b> event fired: ItemIndex is - [{0}]; ItemType is - {1};<br />", e.Item.ItemIndex, e.Item.ItemType));
        
    }

    protected void RadTimePicker2_ItemDataBound(object sender, TimePickerEventArgs e)
    {
        //EventLogConsole1.LoggedEvents.Add(String.Format("<b>ItemDataBound</b> event fired: ItemIndex is - [{0}]; ItemType is - {1};<br />", e.Item.ItemIndex, e.Item.ItemType));
    }

    protected void RadTimePicker2_SelectedDateChanged(object sender, SelectedDateChangedEventArgs e)
    {
        //EventLogConsole1.LoggedEvents.Add("<b>SelectedDateChanged</b> event fired:<br />");
        //lblDate2.Text = RadTimePicker2.SelectedDate.ToString();
        radGVEPPIVisLog.Rebind();
    }


    protected void rblCalendars_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (rblCalendars.SelectedValue == "0")
        {
            pnlRadTimePicker2.Visible = false;
            DateTime time2 = DateTime.Now;
            RadTimePicker2.SelectedDate = time2;
            radGVEPPIVisLog.Rebind();
        }
        else
        {
            pnlRadTimePicker2.Visible = true;
            DateTime time2 = DateTime.Now;
            RadTimePicker2.SelectedDate = time2;
            radGVEPPIVisLog.Rebind();
        }
    }

    protected void cmdRetrieve_Click(object sender, EventArgs e)
    {
        //loadLoggingData();
        radGVEPPIVisLog.Rebind();
    }



    protected void ddlTypes_SelectedIndexChanged(object sender, EventArgs e)
    {
        radGVEPPIVisLog.Rebind();
    }
}