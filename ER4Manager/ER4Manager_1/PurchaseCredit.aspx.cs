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


public partial class PurchaseCredit : System.Web.UI.Page
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
                        lbl.Text = "Invoiced credit";
                    }

                    Telerik.Web.UI.RadTabStrip radTs = (Telerik.Web.UI.RadTabStrip)Master.FindControl("rtsMenu");
                    if (radTs != null)
                    {
                        radTs.SelectedIndex = 4;
                        radTs.Tabs[4].Tabs[6].Selected = true;
                        //radTs.Tabs[3].Tabs[2].Width = 670;
                    }
                    System.Web.UI.WebControls.Label lbl1 = (Label)Master.FindControl("lblHeadingText");
                    if (lbl1 != null)
                    {
                        lbl1.Text = "Invoiced credit";
                    }


                    if (Request.QueryString.Count == 0)
                    {
                        buildPurchaseGrid();
                        
                    }
                    else
                    {
                        lblPurchaseID.Text = Request.QueryString["ID"].ToString();

                        bool isAdmDB = true;
                        IDataReader idr = Utils.GetReader(isAdmDB, "st_CreditPurchaseGet",
                            lblPurchaseID.Text);
                        if (idr.Read())
                        {

                            lblPurchaserID.Text = idr["PURCHASER_CONTACT_ID"].ToString();
                            lblPurchaserName.Text = idr["CONTACT_NAME"].ToString();
                            lblPurchaserEmail.Text = idr["EMAIL"].ToString();
                            tbCreditPurchased.Text = idr["CREDIT_PURCHASED"].ToString();
                            tbPurchaseNotes.Text = idr["NOTES"].ToString();
                            tbDatePurchased.Text = idr["DATE_PURCHASED"].ToString();
                        }
                        idr.Close();

                        pnlPurchaseDetails.Visible = true;
                        lbNewInvoicedCreditPurchase.Visible = false;
                        lblPanelText.Text = "Review / edit details";
                        buildPurchaseGrid();
                    }

                }
                IBCalendar1.Attributes.Add("onclick", "JavaScript:openCalendar1('" +
                    "!1!" + tbDatePurchased.Text + "')");
                lbAddPurchaser.Attributes.Add("onclick", "JavaScript:openReviewerList('Please select')");
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


    private void buildPurchaseGrid()
    {
        DataTable dt = new DataTable();
        System.Data.DataRow newrow;

        dt.Columns.Add(new DataColumn("CREDIT_PURCHASE_ID", typeof(int)));
        dt.Columns.Add(new DataColumn("PURCHASER_CONTACT_NAME", typeof(string)));
        dt.Columns.Add(new DataColumn("EMAIL", typeof(string)));
        dt.Columns.Add(new DataColumn("DATE_PURCHASED", typeof(DateTime)));
        dt.Columns.Add(new DataColumn("CREDIT_PURCHASED", typeof(string)));
        dt.Columns.Add(new DataColumn("CREDIT_REMAINING", typeof(string)));
        dt.Columns.Add(new DataColumn("DETAILS", typeof(string)));

        bool isAdmDB = true;
        IDataReader idr = Utils.GetReader(isAdmDB, "st_CreditPurchasesGetAll", tbFilter.Text);
        while (idr.Read())
        {
            if (rblCreditType.SelectedValue == "Invoice")
            { 
                if (idr["PURCHASE_TYPE"].ToString() == "Invoice")
                {
                    newrow = dt.NewRow();
                    newrow["CREDIT_PURCHASE_ID"] = int.Parse(idr["CREDIT_PURCHASE_ID"].ToString());
                    newrow["PURCHASER_CONTACT_NAME"] = idr["CONTACT_NAME"].ToString();
                    newrow["EMAIL"] = idr["EMAIL"].ToString();
                    newrow["DATE_PURCHASED"] = idr["DATE_PURCHASED"];
                    newrow["CREDIT_PURCHASED"] = idr["CREDIT_PURCHASED"].ToString();
                    newrow["CREDIT_REMAINING"] = "--";
                    newrow["DETAILS"] = "details";
                    dt.Rows.Add(newrow);
                }
            }
            if (rblCreditType.SelectedValue == "Shop")
            {
                if (idr["PURCHASE_TYPE"].ToString() == "Shop")
                {
                    newrow = dt.NewRow();
                    newrow["CREDIT_PURCHASE_ID"] = int.Parse(idr["CREDIT_PURCHASE_ID"].ToString());
                    newrow["PURCHASER_CONTACT_NAME"] = idr["CONTACT_NAME"].ToString();
                    newrow["EMAIL"] = idr["EMAIL"].ToString();
                    newrow["DATE_PURCHASED"] = idr["DATE_PURCHASED"];
                    newrow["CREDIT_PURCHASED"] = idr["CREDIT_PURCHASED"].ToString();
                    newrow["CREDIT_REMAINING"] = "--";
                    newrow["DETAILS"] = "details";
                    dt.Rows.Add(newrow);
                }
            }
            if (rblCreditType.SelectedValue == "Both")
            {
                newrow = dt.NewRow();
                newrow["CREDIT_PURCHASE_ID"] = int.Parse(idr["CREDIT_PURCHASE_ID"].ToString());
                newrow["PURCHASER_CONTACT_NAME"] = idr["CONTACT_NAME"].ToString();
                newrow["EMAIL"] = idr["EMAIL"].ToString();
                newrow["DATE_PURCHASED"] = idr["DATE_PURCHASED"];
                newrow["CREDIT_PURCHASED"] = idr["CREDIT_PURCHASED"].ToString();
                newrow["CREDIT_REMAINING"] = "--";
                newrow["DETAILS"] = "details";
                dt.Rows.Add(newrow);
            }
        }
        idr.Close();
    }


    protected void lbNewInvoicedCreditPurchase_Click(object sender, EventArgs e)
    {
        lblPurchaserError.Visible = false;
        pnlPurchaseDetails.Visible = true;
        lbNewInvoicedCreditPurchase.Visible = false;
        lblPanelText.Text = "Please enter details";
        lblPurchaseID.Text = "N/A";
        lblPurchaserID.Text = "N/A";
        lblPurchaserName.Text = "N/A";
        lblPurchaserEmail.Text = "N/A";
        tbCreditPurchased.Text = "";
        tbPurchaseNotes.Text = "";
        tbDatePurchased.Text = "";
    }


    protected void radGVInvoicedCreditPurchases_NeedDataSource(object sender, Telerik.Web.UI.GridNeedDataSourceEventArgs e)
    {
        DataTable dt = new DataTable();
        System.Data.DataRow newrow;

        dt.Columns.Add(new DataColumn("CREDIT_PURCHASE_ID", typeof(int)));
        dt.Columns.Add(new DataColumn("PURCHASER_CONTACT_NAME", typeof(string)));
        dt.Columns.Add(new DataColumn("EMAIL", typeof(string)));      
        dt.Columns.Add(new DataColumn("DATE_PURCHASED", typeof(DateTime)));
        dt.Columns.Add(new DataColumn("CREDIT_PURCHASED", typeof(string)));
        dt.Columns.Add(new DataColumn("CREDIT_REMAINING", typeof(string)));
        dt.Columns.Add(new DataColumn("DETAILS", typeof(string)));

        bool isAdmDB = true;
        IDataReader idr = Utils.GetReader(isAdmDB, "st_CreditPurchasesGetAll", tbFilter.Text);
        while (idr.Read())
        {
            if (rblCreditType.SelectedValue == "Invoice")
            {
                if (idr["PURCHASE_TYPE"].ToString() == "Invoice")
                {
                    newrow = dt.NewRow();
                    newrow["CREDIT_PURCHASE_ID"] = int.Parse(idr["CREDIT_PURCHASE_ID"].ToString());
                    newrow["PURCHASER_CONTACT_NAME"] = idr["CONTACT_NAME"].ToString();
                    newrow["EMAIL"] = idr["EMAIL"].ToString();
                    newrow["DATE_PURCHASED"] = idr["DATE_PURCHASED"];
                    newrow["CREDIT_PURCHASED"] = idr["CREDIT_PURCHASED"].ToString();
                    newrow["CREDIT_REMAINING"] = "--";
                    newrow["DETAILS"] = "details";
                    dt.Rows.Add(newrow);
                }
            }
            if (rblCreditType.SelectedValue == "Shop")
            {
                if (idr["PURCHASE_TYPE"].ToString() == "Shop")
                {
                    newrow = dt.NewRow();
                    newrow["CREDIT_PURCHASE_ID"] = int.Parse(idr["CREDIT_PURCHASE_ID"].ToString());
                    newrow["PURCHASER_CONTACT_NAME"] = idr["CONTACT_NAME"].ToString();
                    newrow["EMAIL"] = idr["EMAIL"].ToString();
                    newrow["DATE_PURCHASED"] = idr["DATE_PURCHASED"];
                    newrow["CREDIT_PURCHASED"] = idr["CREDIT_PURCHASED"].ToString();
                    newrow["CREDIT_REMAINING"] = "--";
                    newrow["DETAILS"] = "details";
                    dt.Rows.Add(newrow);
                }
            }
            if (rblCreditType.SelectedValue == "Both")
            {
                newrow = dt.NewRow();
                newrow["CREDIT_PURCHASE_ID"] = int.Parse(idr["CREDIT_PURCHASE_ID"].ToString());
                newrow["PURCHASER_CONTACT_NAME"] = idr["CONTACT_NAME"].ToString();
                newrow["EMAIL"] = idr["EMAIL"].ToString();
                newrow["DATE_PURCHASED"] = idr["DATE_PURCHASED"];
                newrow["CREDIT_PURCHASED"] = idr["CREDIT_PURCHASED"].ToString();
                newrow["CREDIT_REMAINING"] = "--";
                newrow["DETAILS"] = "details";
                dt.Rows.Add(newrow);
            }
        }
        idr.Close();

        int test = dt.Rows.Count;
        radGVInvoicedCreditPurchases.DataSource = dt;
    }
    protected void radGVInvoicedCreditPurchases_PageIndexChanged(object sender, Telerik.Web.UI.GridPageChangedEventArgs e)
    {
        if (tbFilter.Text == "")
        {
            buildPurchaseGrid();
        }
        else
        {
            radGVInvoicedCreditPurchases.Rebind();
        }
    }
    protected void RadAjaxManager1_AjaxRequest(object sender, Telerik.Web.UI.AjaxRequestEventArgs e)
    {
        if (e.Argument.IndexOf("FilterGrid") != -1)
        {
            radGVInvoicedCreditPurchases.Rebind();
        }
    }

    protected void radGVInvoicedCreditPurchases_ItemDataBound(object sender, Telerik.Web.UI.GridItemEventArgs e)
    {

    }

    protected void radGVInvoicedCreditPurchases_ItemCommand(object sender, GridCommandEventArgs e)
    {
        if (e.CommandName == "Details")
        {
            string test = e.CommandArgument.ToString();
            pnlPurchaseDetails.Visible = true;
            //UpdateCarRentCounter(e.CommandArgument.ToString());
        }
    }

    protected void cmdAddContact_Click(object sender, EventArgs e)
    {
        if (Utils.GetSessionString("variableID") != "")
        {
            if (Utils.GetSessionString("variableID") != "")
            {
                lblPurchaserID.Text = Utils.GetSessionString("variableID");

                bool isAdmDB = true;
                IDataReader idr = Utils.GetReader(isAdmDB, "st_ContactDetails", Utils.GetSessionString("variableID"));
                if (idr.Read())
                {
                    lblPurchaserName.Text = idr["CONTACT_NAME"].ToString();
                    lblPurchaserEmail.Text = idr["EMAIL"].ToString();
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
                tbDatePurchased.Text = Utils.GetSessionString("CalendarDate").Substring(counterLocation + 1);
            }
            else
            {
                tbDatePurchased.Text = Utils.GetSessionString("CalendarDate").Substring(counterLocation + 1);
            }
        }
        Utils.SetSessionString("CalendarDate", "");
    }

    protected void cmdSaveNewPurchaser_Click(object sender, EventArgs e)
    {
        lblPurchaserError.Visible = false;
        lblPurchaserError.Text = "Missing data";
        bool dateOK = true;
        if ((Utils.IsNumeric(tbCreditPurchased.Text)) && (lblPurchaserID.Text != "N/A")
            && (tbDatePurchased.Text != ""))
        {
            // look for a decimal in the fee
            if (tbCreditPurchased.Text.Contains('.'))
                tbCreditPurchased.Text = tbCreditPurchased.Text.Remove(tbCreditPurchased.Text.IndexOf('.'));

            // amount must be in £5 increments
            if ((int.Parse(tbCreditPurchased.Text) % 5 == 0))
            {
                string datePurchased = "";
                try
                {
                    DateTime registrationDate = Convert.ToDateTime(tbDatePurchased.Text);
                    datePurchased = registrationDate.ToString("yyyy-M-d hh:m:ss.mmm"); // this is case sensitive!
                }
                catch (Exception er)
                {
                    lblPurchaserError.Visible = true;
                    lblPurchaserError.Text = "Incorrect date";
                    dateOK = false;
                }


                if (dateOK)
                {
                    if (lblPurchaseID.Text == "N/A")
                        lblPurchaseID.Text = "0";

                    bool isAdmDB = true;
                    Utils.ExecuteSP(isAdmDB, Server, "st_CreditPurchaseCreateOrEdit", lblPurchaseID.Text,
                         lblPurchaserID.Text, tbCreditPurchased.Text, datePurchased, tbPurchaseNotes.Text, "Invoice");

                    pnlPurchaseDetails.Visible = false;
                    lbNewInvoicedCreditPurchase.Visible = true;
                    radGVInvoicedCreditPurchases.Rebind();
                }
            }
            else
            {
                lblPurchaserError.Visible = true;
                lblPurchaserError.Text = "Must be in £5 increments";
            }
        }
        else
        {
            lblPurchaserError.Visible = true;
            lblPurchaserError.Text = "Missing or invalid data";
        }

    }

    protected void lbHideDetails_Click(object sender, EventArgs e)
    {
        pnlPurchaseDetails.Visible = false;
        lbNewInvoicedCreditPurchase.Visible = true;
    }

    protected void rblCreditType_SelectedIndexChanged(object sender, EventArgs e)
    {
        radGVInvoicedCreditPurchases.Rebind();
    }
}