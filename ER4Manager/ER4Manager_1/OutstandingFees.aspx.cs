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

public partial class OutstandingFees : System.Web.UI.Page
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
                        lbl.Text = "Outstanding fees";
                    }

                    Telerik.Web.UI.RadTabStrip radTs = (Telerik.Web.UI.RadTabStrip)Master.FindControl("rtsMenu");
                    if (radTs != null)
                    {
                        radTs.SelectedIndex = 4;
                        radTs.Tabs[4].Tabs[7].Selected = true;
                        //radTs.Tabs[3].Tabs[2].Width = 670;
                    }
                    System.Web.UI.WebControls.Label lbl1 = (Label)Master.FindControl("lblHeadingText");
                    if (lbl1 != null)
                    {
                        lbl1.Text = "Outstanding fees";
                    }

                    lbLogInAs.Visible = false;
                    cmdDeleteOutstandingFee.Visible = false;

                    if (Request.QueryString.Count == 0)
                    {
                        buildOutstandingFeesGrid();
                    }
                    else
                    {
                        lblEmailSentMsg.Visible = false;
                        lblOutstandingFeeID.Text = Request.QueryString["ID"].ToString();

                        cmdSaveNewOutstandingFee.Enabled = true;
                        cmdDeleteOutstandingFee.Enabled = true;
                        lbSelectAccount.Enabled = true;

                        bool isAdmDB = true;
                        IDataReader idr = Utils.GetReader(isAdmDB, "st_OutstandingFeeGet",
                            lblOutstandingFeeID.Text);
                        if (idr.Read())
                        {

                            lblAccountID.Text = idr["ACCOUNT_ID"].ToString();
                            lblAccountName.Text = idr["CONTACT_NAME"].ToString();
                            lblAccountEmail.Text = idr["EMAIL"].ToString();
                            tbOutstandingFee.Text = idr["AMOUNT"].ToString();
                            tbOutstandingFeeNotes.Text = idr["NOTES"].ToString();
                            tbDateGenerated.Text = idr["DATE_CREATED"].ToString();
                            lblStatus.Text = idr["STATUS"].ToString();
                        }
                        idr.Close();

                        pnlOutstandingFeeDetails.Visible = true;
                        lbNewOutstandingFee.Visible = false;
                        lblPanelText.Text = "Review / edit details";
                        buildOutstandingFeesGrid();
                        lbLogInAs.Visible = true;
                        cmdDeleteOutstandingFee.Visible = true;

                        if (lblStatus.Text == "Paid")
                        {
                            cmdSaveNewOutstandingFee.Enabled = false;
                            cmdDeleteOutstandingFee.Enabled = false;
                            lbSelectAccount.Enabled = false;
                        }
                    }

                }
                
                IBCalendar1.Attributes.Add("onclick", "JavaScript:openCalendar1('" +
                    "!1!" + tbDateGenerated.Text + "')");
                if (lblStatus.Text != "Paid")
                {
                    lbSelectAccount.Attributes.Add("onclick", "JavaScript:openReviewerList('Please select')");
                }
                cmdDeleteOutstandingFee.Attributes.Add("onclick", "if (confirm('Are you sure you wish to delete this outstanding fee?') == false) return false;");

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

    private void buildOutstandingFeesGrid()
    {
        DataTable dt = new DataTable();
        System.Data.DataRow newrow;

        dt.Columns.Add(new DataColumn("OUTSTANDING_FEE_ID", typeof(int)));
        dt.Columns.Add(new DataColumn("CONTACT_NAME", typeof(string)));
        dt.Columns.Add(new DataColumn("EMAIL", typeof(string)));
        dt.Columns.Add(new DataColumn("DATE_CREATED", typeof(DateTime)));
        dt.Columns.Add(new DataColumn("OUTSTANDING_FEE", typeof(string)));
        dt.Columns.Add(new DataColumn("STATUS", typeof(string)));
        dt.Columns.Add(new DataColumn("DETAILS", typeof(string)));

        bool isAdmDB = true;
        IDataReader idr = Utils.GetReader(isAdmDB, "st_OutstandingFeesGetAll", tbFilter.Text);
        while (idr.Read())
        {
            newrow = dt.NewRow();
            newrow["OUTSTANDING_FEE_ID"] = int.Parse(idr["OUTSTANDING_FEE_ID"].ToString());
            newrow["CONTACT_NAME"] = idr["CONTACT_NAME"].ToString();
            newrow["EMAIL"] = idr["EMAIL"].ToString();
            newrow["DATE_CREATED"] = idr["DATE_CREATED"];
            newrow["OUTSTANDING_FEE"] = idr["AMOUNT"].ToString();
            newrow["STATUS"] = idr["STATUS"].ToString();
            newrow["DETAILS"] = "details";
            dt.Rows.Add(newrow);
        }
        idr.Close();

    }


    protected void radGVOutstandingFees_NeedDataSource(object sender, Telerik.Web.UI.GridNeedDataSourceEventArgs e)
    {
        DataTable dt = new DataTable();
        System.Data.DataRow newrow;

        dt.Columns.Add(new DataColumn("OUTSTANDING_FEE_ID", typeof(int)));
        dt.Columns.Add(new DataColumn("CONTACT_NAME", typeof(string)));
        dt.Columns.Add(new DataColumn("EMAIL", typeof(string)));
        dt.Columns.Add(new DataColumn("DATE_CREATED", typeof(DateTime)));
        dt.Columns.Add(new DataColumn("OUTSTANDING_FEE", typeof(string)));
        dt.Columns.Add(new DataColumn("STATUS", typeof(string)));
        dt.Columns.Add(new DataColumn("DETAILS", typeof(string)));

        bool isAdmDB = true;
        IDataReader idr = Utils.GetReader(isAdmDB, "st_OutstandingFeesGetAll", tbFilter.Text);
        while (idr.Read())
        {
            newrow = dt.NewRow();
            newrow["OUTSTANDING_FEE_ID"] = int.Parse(idr["OUTSTANDING_FEE_ID"].ToString());
            newrow["CONTACT_NAME"] = idr["CONTACT_NAME"].ToString();
            newrow["EMAIL"] = idr["EMAIL"].ToString();
            newrow["DATE_CREATED"] = idr["DATE_CREATED"];
            newrow["OUTSTANDING_FEE"] = idr["AMOUNT"].ToString();
            newrow["STATUS"] = idr["STATUS"].ToString();
            newrow["DETAILS"] = "details";
            dt.Rows.Add(newrow);
        }
        idr.Close();

        int test = dt.Rows.Count;
        radGVOutstandingFees.DataSource = dt;
    }


    protected void radGVOutstandingFees_PageIndexChanged(object sender, Telerik.Web.UI.GridPageChangedEventArgs e)
    {
        if (tbFilter.Text == "")
        {
            buildOutstandingFeesGrid();
        }
        else
        {
            radGVOutstandingFees.Rebind();
        }
    }

    protected void RadAjaxManager1_AjaxRequest(object sender, Telerik.Web.UI.AjaxRequestEventArgs e)
    {
        if (e.Argument.IndexOf("FilterGrid") != -1)
        {
            radGVOutstandingFees.Rebind();
        }
    }


    protected void radGVOutstandingFees_ItemDataBound(object sender, Telerik.Web.UI.GridItemEventArgs e)
    {

    }

    protected void radGVOutstandingFees_ItemCommand(object sender, GridCommandEventArgs e)
    {
        if (e.CommandName == "Details")
        {
            string test = e.CommandArgument.ToString();
            pnlOutstandingFeeDetails.Visible = true;
            //UpdateCarRentCounter(e.CommandArgument.ToString());
        }
    }


    protected void cmdAddContact_Click(object sender, EventArgs e)
    {
        if (Utils.GetSessionString("variableID") != "")
        {
            if (Utils.GetSessionString("variableID") != "")
            {
                lblAccountID.Text = Utils.GetSessionString("variableID");

                bool isAdmDB = true;
                IDataReader idr = Utils.GetReader(isAdmDB, "st_ContactDetails", Utils.GetSessionString("variableID"));
                if (idr.Read())
                {
                    lblAccountName.Text = idr["CONTACT_NAME"].ToString();
                    lblAccountEmail.Text = idr["EMAIL"].ToString();
                }
                idr.Close();
                Utils.SetSessionString("variableID", "");
            }
        }
    }

    protected void cmdPlaceDate_Click(object sender, EventArgs e)
    {
        if (Utils.GetSessionString("CalendarDate") != "")
        {
            // a calendar counter is being passed with the date
            int counterLocation = Utils.GetSessionString("CalendarDate").IndexOf("!", 1);
            string calendarCounter = Utils.GetSessionString("CalendarDate").Substring(1, counterLocation - 1);
            if (calendarCounter == "1")
            {
                tbDateGenerated.Text = Utils.GetSessionString("CalendarDate").Substring(counterLocation + 1);
            }
            else
            {
                tbDateGenerated.Text = Utils.GetSessionString("CalendarDate").Substring(counterLocation + 1);
            }
        }
        Utils.SetSessionString("CalendarDate", "");
    }

    protected void lbHideDetails_Click(object sender, EventArgs e)
    {
        pnlOutstandingFeeDetails.Visible = false;
        lbNewOutstandingFee.Visible = true;
    }

    protected void lbNewOutstandingFee_Click(object sender, EventArgs e)
    {
        lblOutstandingFeeError.Visible = false;
        pnlOutstandingFeeDetails.Visible = true;
        lbNewOutstandingFee.Visible = false;
        lblPanelText.Text = "Please enter details of outstanding fee";
        lblOutstandingFeeID.Text = "N/A";
        lblAccountID.Text = "N/A";
        lblAccountName.Text = "N/A";
        lblAccountEmail.Text = "N/A";
        tbOutstandingFee.Text = "";
        tbOutstandingFeeNotes.Text = "";
        tbDateGenerated.Text = "";
        lbLogInAs.Visible = false;
        cmdDeleteOutstandingFee.Visible = false;
        lblEmailSentMsg.Visible = false;
        lblStatus.Text = "Outstanding";
    }

    protected void cmdSaveNewOutstandingFee_Click(object sender, EventArgs e)
    {
        lblOutstandingFeeError.Visible = false;
        lblOutstandingFeeError.Text = "Missing data";
        lbLogInAs.Visible = false;
        cmdDeleteOutstandingFee.Visible = false;
        bool dateOK = true;

        // remove the £ sign
        if (tbOutstandingFee.Text.Contains('£'))
            tbOutstandingFee.Text = tbOutstandingFee.Text.Replace("£", "");

        // remove any commas
        if (tbOutstandingFee.Text.Contains(','))
            tbOutstandingFee.Text = tbOutstandingFee.Text.Replace(",", "");

        if ((Utils.IsNumeric(tbOutstandingFee.Text)) && (lblAccountID.Text != "N/A")
            && (tbDateGenerated.Text != "") && (!tbOutstandingFee.Text.Contains("-")))
        {
            // look for a decimal in the fee
            if (tbOutstandingFee.Text.Contains('.'))
                tbOutstandingFee.Text = tbOutstandingFee.Text.Remove(tbOutstandingFee.Text.IndexOf('.'));
           
            // its an an int so check it is in £1 increments
            if ((int.Parse(tbOutstandingFee.Text) % 1 == 0)) // this check not really needed but the code was already there...
            {
                string dateCreated = "";
                try
                {
                    DateTime registrationDate = Convert.ToDateTime(tbDateGenerated.Text);
                    dateCreated = registrationDate.ToString("yyyy-M-d hh:m:ss.mmm"); // this is case sensitive!
                }
                catch (Exception er)
                {
                    lblOutstandingFeeError.Visible = true;
                    lblOutstandingFeeError.Text = "Incorrect date";
                    dateOK = false;
                }


                if (dateOK)
                {
                    if (lblOutstandingFeeID.Text == "N/A")
                        lblOutstandingFeeID.Text = "0";

                    bool isAdmDB = true;
                    Utils.ExecuteSP(isAdmDB, Server, "st_OutstandingFeeCreateOrEdit", lblOutstandingFeeID.Text,
                            lblAccountID.Text, tbOutstandingFee.Text, dateCreated, tbOutstandingFeeNotes.Text);

                    if (cbWithEmail.Checked)
                    {
                        // send out the email
                        string sendResult = Utils.OutstandingFeeEmail(lblAccountEmail.Text, lblAccountName.Text, tbOutstandingFee.Text);

                        if (sendResult.Contains("ERROR"))
                        {
                            lblOutstandingFeeError.Visible = true;
                            lblOutstandingFeeError.Text = "Unable to send email";
                        }
                        else
                        {
                            lblEmailSentMsg.Visible = true;
                            pnlOutstandingFeeDetails.Visible = false;
                            lbNewOutstandingFee.Visible = true;
                            radGVOutstandingFees.Rebind();
                        }
                    }
                    else
                    {
                        pnlOutstandingFeeDetails.Visible = false;
                        lbNewOutstandingFee.Visible = true;
                        radGVOutstandingFees.Rebind();
                    }
                }
            }
            else
            {
                lblOutstandingFeeError.Visible = true;
                lblOutstandingFeeError.Text = "Must be in £1 increments";
            }

        }
        else
        {
            lblOutstandingFeeError.Visible = true;
            lblOutstandingFeeError.Text = "Missing or invalid data";
        }
    }

    protected void cmdDeleteOutstandingFee_Click(object sender, EventArgs e)
    {
        bool isAdmDB = true;
        DateTime registrationDate = Convert.ToDateTime("01/01/2020 00:00:00");
        string dateCreated = registrationDate.ToString("yyyy-M-d hh:m:ss.mmm");

        Utils.ExecuteSP(isAdmDB, Server, "st_OutstandingFeeCreateOrEdit", lblOutstandingFeeID.Text,
                "0", "0", dateCreated, "DeleteThisFee");

        pnlOutstandingFeeDetails.Visible = false;
        lbNewOutstandingFee.Visible = true;
        radGVOutstandingFees.Rebind();
    }

    protected void lbLogInAs_Click(object sender, EventArgs e)
    {
        Utils.SetSessionString("Contact_ID", lblAccountID.Text);
        Utils.SetSessionString("IsAdm", "False");
        Utils.SetSessionString("IsSiteLicenseAdm", "0");

        bool isAdmDB = true;
        IDataReader idr = Utils.GetReader(isAdmDB, "st_ContactIsSiteLicenseAdm", lblAccountID.Text);
        if (idr.Read())
        {
            Utils.SetSessionString("IsSiteLicenseAdm", "1");
        }
        idr.Close();
        Server.Transfer("Summary.aspx");
    }
}