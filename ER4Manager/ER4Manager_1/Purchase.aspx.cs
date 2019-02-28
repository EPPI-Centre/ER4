using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Data;
using System.Data.SqlClient;
using System.Xml.Linq;
using System.Text;

public partial class Purchase : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        string test = Utils.GetSessionString("Contact_ID");
        //if someone is logged on and Purchasing is enabled or Adm see all and current user is admin
        if ((test != null && test !="") && (Utils.GetSessionString("PurchasesEnabled") == "True" || ((Utils.GetSessionString("AdmEnableAll") == "True") &&
                    (Utils.GetSessionString("IsAdm") == "True"))))
        {
            LblAddReviewResult.Visible = false;
            LblAddReviewResult.Visible = false;
            if (!IsPostBack)
            {
                System.Web.UI.WebControls.Label lbl = (Label)Master.FindControl("lblPageTitle");
                if (lbl != null)
                {
                    lbl.Text = "Purchase accounts and shareable reviews";
                }

                Telerik.Web.UI.RadTabStrip radTs = (Telerik.Web.UI.RadTabStrip)Master.FindControl("rtsMenu");
                if (radTs != null)
                {
                    radTs.SelectedIndex = 1;
                    radTs.Tabs[1].Tabs[0].Selected = true;
                    radTs.Tabs[1].Tabs[2].Width = 550;
                }
                System.Web.UI.WebControls.Label lbl1 = (Label)Master.FindControl("lblHeadingText");
                if (lbl1 != null)
                {
                    lbl1.Text = "Purchase accounts and shareable reviews";
                }

                Utils.SetSessionString("Draft_Bill_ID", null);
                pnlContactAddress.Visible = true;
                billingAddressDetails();
                /*
                buildPurchasedAccountsGrid();
                buildNewAccountsGrid();
                buildPurchasedReviewsGrid();
                calculateTotalFees();
                */

                bool isAdmDB = true;
                IDataReader idr = Utils.GetReader(isAdmDB, "st_ManagementSettings");
                if (idr.Read()) // it exists
                {
                    if (idr["ENABLE_UCL_SHOP"].ToString() == "True")
                    {
                        pnlContinueToWPMUCL.Visible = true;
                        pnlContinueToWPM.Visible = false;
                    }
                }
                idr.Close();

            }
            
            if (Utils.GetSessionString("PurchasesEnabled") == "True")
            {
                cmdPurchase.Enabled = true;
            }
            if ((Utils.GetSessionString("AdmEnableAll") == "True") &&
                    (Utils.GetSessionString("IsAdm") == "True"))
            {
                cmdPurchase.Enabled = true;
            }
            /*
            if (Utils.GetSessionString("IsAdm") == "True")
            {
                pnlWPMUCL.Visible = true;
            }
            */
        }
        else
        {
            //no matter what, if we are getting a request for this page from an unauthenticated client, always transfer to the generic error. 
            Server.Transfer("Error.aspx");
        }

        
    }

    private void billingAddressDetails()
    {
        DataTable dt = new DataTable();
        System.Data.DataRow newrow;
        dt.Columns.Add(new DataColumn("COUNTRY_ID", typeof(string)));
        dt.Columns.Add(new DataColumn("COUNTRY_NAME", typeof(string)));

        newrow = dt.NewRow();
        newrow["COUNTRY_ID"] = "0";
        newrow["COUNTRY_NAME"] = "Please select country";
        dt.Rows.Add(newrow);

        bool isAdmDB = true;
        IDataReader idr = Utils.GetReader(isAdmDB, "st_CountriesGet", "");
        while (idr.Read())
        {
            newrow = dt.NewRow();
            newrow["COUNTRY_ID"] = idr["COUNTRY_ID"].ToString();
            newrow["COUNTRY_NAME"] = idr["COUNTRY_NAME"].ToString();
            dt.Rows.Add(newrow);
        }
        idr.Close();

        ddlCountries.DataSource = dt;
        ddlCountries.DataBind();


        isAdmDB = true;
        idr = Utils.GetReader(isAdmDB, "st_BillAddressGet",
            Utils.GetSessionString("Contact_ID"));
        if (idr != null && idr.Read())
        {
            lblName.Text = idr["CONTACT_NAME"].ToString();
            tbOrganization.Text = idr["ORGANISATION"].ToString();
            tbPostalAddress.Text = idr["ADDRESS"].ToString();
            ddlCountries.SelectedValue = idr["COUNTRY_ID"].ToString();
            lblEmailAddress.Text = idr["EMAIL"].ToString(); 
            if (idr["EU_VAT_REG_NUMBER"].ToString() == "")
                tbEuVatNumber.Text = "Enter if Available";
            else
                tbEuVatNumber.Text = idr["EU_VAT_REG_NUMBER"].ToString();
        }
        else
        {
            isAdmDB = true;
            IDataReader idr1 = Utils.GetReader(isAdmDB, "st_ContactDetails",
                Utils.GetSessionString("Contact_ID"));
            if (idr1.Read())
            {
                lblName.Text = idr1["CONTACT_NAME"].ToString();
                lblEmailAddress.Text = idr1["EMAIL"].ToString();
            }

            ddlCountries.SelectedIndex = 0;
            lblContactDetails.Text = "Please enter and save your contact details";
        }
        idr.Close();
    }

    private void buildPurchasedAccountsGrid()
    {
        DateTime dayCreated;
        DateTime dayExpires;
        DateTime today = DateTime.Today;
        DataTable dt = new DataTable();
        System.Data.DataRow newrow;

        dt.Columns.Add(new DataColumn("CONTACT_ID", typeof(string)));
        dt.Columns.Add(new DataColumn("CONTACT_NAME", typeof(string)));
        dt.Columns.Add(new DataColumn("CREATOR_ID", typeof(string)));
        dt.Columns.Add(new DataColumn("DATE_CREATED", typeof(string)));
        dt.Columns.Add(new DataColumn("EXPIRY_DATE", typeof(string)));
        dt.Columns.Add(new DataColumn("MONTHS_CREDIT", typeof(string)));
        dt.Columns.Add(new DataColumn("COST", typeof(string)));
        bool isAdmDB = true;
        IDataReader idr = Utils.GetReader(isAdmDB, "st_ContactPurchasedAccounts",
            Utils.GetSessionString("Contact_ID"));
        while (idr.Read())
        {
            newrow = dt.NewRow();
            newrow["CONTACT_ID"] = idr["CONTACT_ID"].ToString();
            if (idr["CONTACT_NAME"].ToString() == "")
                newrow["CONTACT_NAME"] = "Not set up";
            else
                newrow["CONTACT_NAME"] = idr["CONTACT_NAME"].ToString();
            newrow["CREATOR_ID"] = idr["CREATOR_ID"].ToString();
            dayCreated = Convert.ToDateTime(idr["DATE_CREATED"].ToString());
            newrow["DATE_CREATED"] = idr["DATE_CREATED"].ToString();
            newrow["EXPIRY_DATE"] = idr["EXPIRY_DATE"].ToString();

            if (idr["EXPIRY_DATE"].ToString() == "")
            {
                newrow["EXPIRY_DATE"] = "Not activated: " + idr["MONTHS_CREDIT"].ToString() + " months credit";
                newrow["CONTACT_NAME"] = "Not activated";
            }
            else
            {
                dayExpires = Convert.ToDateTime(idr["EXPIRY_DATE"].ToString());
                if (dayExpires < today)
                {
                    newrow["EXPIRY_DATE"] = dayExpires.ToShortDateString() + "  Expired";
                }
                else
                    newrow["EXPIRY_DATE"] = dayExpires.ToShortDateString();
                if (idr["SITE_LIC_ID"].ToString() != null && idr["SITE_LIC_ID"].ToString() != "")
                {
                    newrow["EXPIRY_DATE"] += " In Site License '" + idr["SITE_LIC_NAME"].ToString() + "' (ID:" + idr["SITE_LIC_ID"].ToString() + ")";

                }
            }
            newrow["COST"] = "0";
            dt.Rows.Add(newrow);
        }
        idr.Close();

        gvPurchasedAccounts.DataSource = dt;
        gvPurchasedAccounts.DataBind();

        //// go through each row and setup the extendby ddl
        //string contactID = "";
        //string accountCreatorID = "";
        //for (int i = 0; i < gvPurchasedAccounts.Rows.Count; i++)
        //{
        //    contactID = gvPurchasedAccounts.Rows[i].Cells[0].Text;
        //    accountCreatorID = gvPurchasedAccounts.Rows[i].Cells[2].Text;
            
        //    isAdmDB = true;
        //    idr = Utils.GetReader(isAdmDB, "st_ContactAccountExtensionGet",
        //        accountCreatorID, contactID);
        //        //Utils.GetSessionString("Contact_ID"));
        //    if (idr.Read())
        //    {
        //        GridViewRow row = gvPurchasedAccounts.Rows[i];
        //        DropDownList ddl = ((DropDownList)row.FindControl("ddlExtendAccount"));
        //        ddl.SelectedValue = idr["EXTEND_BY"].ToString();

        //        gvPurchasedAccounts.Rows[i].Cells[7].Text = idr["COST"].ToString();
        //    }
        //    idr.Close();
        //}

        if (dt.Rows.Count != 0)
        {
            for (int i = 0; i < gvPurchasedAccounts.Rows.Count; i++)
            {
                if (gvPurchasedAccounts.Rows[i].Cells[1].Text.Contains("Not set up"))
                {
                    gvPurchasedAccounts.Rows[i].Cells[1].BackColor = System.Drawing.Color.PaleGreen;
                }
                if (gvPurchasedAccounts.Rows[i].Cells[1].Text.Contains("Not activated"))
                {
                    gvPurchasedAccounts.Rows[i].Cells[1].BackColor = System.Drawing.Color.Yellow;
                }
                if (gvPurchasedAccounts.Rows[i].Cells[4].Text.Contains("Not activated"))
                {
                    gvPurchasedAccounts.Rows[i].Cells[4].BackColor = System.Drawing.Color.Yellow;
                }
                if (gvPurchasedAccounts.Rows[i].Cells[4].Text.Contains("Expired"))
                {
                    gvPurchasedAccounts.Rows[i].Cells[4].BackColor = System.Drawing.Color.Pink;
                    gvPurchasedAccounts.Rows[i].Cells[4].Font.Bold = true;
                }
                if (gvPurchasedAccounts.Rows[i].Cells[4].Text.Contains(" In Site License '"))
                {
                    gvPurchasedAccounts.Rows[i].Cells[4].BackColor = System.Drawing.Color.LightGray;
                    gvPurchasedAccounts.Rows[i].Cells[4].ToolTip = "Subscriptions for accounts associated with a Site License can't be purchased individually";
                    DropDownList ddl = (DropDownList)gvPurchasedAccounts.Rows[i].Cells[5].FindControl("ddlExtendAccount");
                    if (ddl != null)
                    {
                        ddl.Enabled = false;
                        ddl.ToolTip = "Subscriptions for accounts associated with a Site License can't be purchased individually";
                    }

                }
            }
        }
    }

    private void buildPurchasedReviewsGrid()
    {
        DateTime dayExpires;
        DateTime today = DateTime.Today;
        DataTable dt = new DataTable();
        System.Data.DataRow newrow;

        dt.Columns.Add(new DataColumn("REVIEW_ID", typeof(string)));
        dt.Columns.Add(new DataColumn("REVIEW_NAME", typeof(string)));
        dt.Columns.Add(new DataColumn("FUNDER_ID", typeof(string)));
        dt.Columns.Add(new DataColumn("DATE_CREATED", typeof(string)));
        dt.Columns.Add(new DataColumn("EXPIRY_DATE", typeof(string)));
        dt.Columns.Add(new DataColumn("MONTHS_CREDIT", typeof(string)));
        dt.Columns.Add(new DataColumn("COST", typeof(string)));
        bool isAdmDB = true;
        IDataReader idr = Utils.GetReader(isAdmDB, "st_ContactPurchasedReviews",
            Utils.GetSessionString("Contact_ID"));
        while (idr.Read())
        {
            if ((idr["EXPIRY_DATE"].ToString() == "") &&
                (int.Parse(idr["MONTHS_CREDIT"].ToString()) > 0))
            {
                newrow = dt.NewRow();
                newrow["REVIEW_ID"] = idr["REVIEW_ID"].ToString();
                newrow["REVIEW_NAME"] = "Not activated";
                newrow["FUNDER_ID"] = idr["FUNDER_ID"].ToString();
                newrow["DATE_CREATED"] = idr["DATE_CREATED"].ToString();
                newrow["EXPIRY_DATE"] = "Not activated: " + idr["MONTHS_CREDIT"].ToString() + " months credit";
                newrow["MONTHS_CREDIT"] = idr["MONTHS_CREDIT"].ToString();
                newrow["COST"] = "0";
                dt.Rows.Add(newrow);
            }
            else if ((idr["EXPIRY_DATE"].ToString() == "") &&
                (int.Parse(idr["MONTHS_CREDIT"].ToString()) == 0))
            {
                newrow = dt.NewRow();
                newrow["REVIEW_ID"] = idr["REVIEW_ID"].ToString();
                if (idr["REVIEW_NAME"].ToString() == "")
                    newrow["REVIEW_NAME"] = "Not named";
                else
                    newrow["REVIEW_NAME"] = idr["REVIEW_NAME"].ToString();
                newrow["FUNDER_ID"] = idr["FUNDER_ID"].ToString();
                newrow["DATE_CREATED"] = idr["DATE_CREATED"].ToString();
                newrow["EXPIRY_DATE"] = "Private: you can make it shearable by adding some months credit.";
                newrow["MONTHS_CREDIT"] = idr["MONTHS_CREDIT"].ToString();
                newrow["COST"] = "0";
                dt.Rows.Add(newrow);
            }
            else
            {
                newrow = dt.NewRow();
                newrow["REVIEW_ID"] = idr["REVIEW_ID"].ToString();
                if (idr["REVIEW_NAME"].ToString() == "")
                    newrow["REVIEW_NAME"] = "Not named";
                else
                    newrow["REVIEW_NAME"] = idr["REVIEW_NAME"].ToString();
                newrow["FUNDER_ID"] = idr["FUNDER_ID"].ToString();
                newrow["DATE_CREATED"] = idr["DATE_CREATED"].ToString();
                dayExpires = Convert.ToDateTime(idr["EXPIRY_DATE"].ToString());            
                if (dayExpires < today)
                {
                    newrow["EXPIRY_DATE"] = dayExpires.ToShortDateString() + "  Expired";
                }
                else
                    newrow["EXPIRY_DATE"] = dayExpires.ToShortDateString();
                newrow["MONTHS_CREDIT"] = idr["MONTHS_CREDIT"].ToString();
                newrow["COST"] = "0";
                dt.Rows.Add(newrow);
            }
            if (idr["SITE_LIC_ID"].ToString() != "")
            {
                newrow["EXPIRY_DATE"] += " In Site License '" + idr["SITE_LIC_NAME"].ToString() + "' (ID:" + idr["SITE_LIC_ID"].ToString() + ")";
            }
        }
        idr.Close();

        gvPurchasedReviews.DataSource = dt;
        gvPurchasedReviews.DataBind();

        //// go through each row and setup the extendby ddl
        //string reviewID = "";
        //string reviewCreatorID = "";
        //for (int i = 0; i < gvPurchasedReviews.Rows.Count; i++)
        //{
        //    reviewID = gvPurchasedReviews.Rows[i].Cells[0].Text;
        //    reviewCreatorID = gvPurchasedReviews.Rows[i].Cells[2].Text;

        //    isAdmDB = true;
        //    idr = Utils.GetReader(isAdmDB, "st_ContactReviewExtensionGet",
        //        reviewCreatorID, reviewID);
        //    //Utils.GetSessionString("Contact_ID"));
        //    if (idr.Read())
        //    {
        //        GridViewRow row = gvPurchasedReviews.Rows[i];
        //        DropDownList ddl = ((DropDownList)row.FindControl("ddlExtendReview"));
        //        ddl.SelectedValue = idr["EXTEND_BY"].ToString();

        //        gvPurchasedReviews.Rows[i].Cells[7].Text = idr["COST"].ToString();
        //    }
        //    idr.Close();
        //}

        if (dt.Rows.Count != 0)
        {
            for (int i = 0; i < gvPurchasedReviews.Rows.Count; i++)
            {
                if (gvPurchasedReviews.Rows[i].Cells[1].Text.IndexOf("Not named") == 0)
                {
                    gvPurchasedReviews.Rows[i].Cells[1].BackColor = System.Drawing.Color.PaleGreen;
                }
                if (gvPurchasedReviews.Rows[i].Cells[4].Text.IndexOf("Not activated") == 0)
                {
                    gvPurchasedReviews.Rows[i].Cells[4].BackColor = System.Drawing.Color.Yellow;
                }
                else if (gvPurchasedReviews.Rows[i].Cells[4].Text.IndexOf("Private") == 0)
                {
                    gvPurchasedReviews.Rows[i].Cells[4].BackColor = System.Drawing.Color.PaleGreen;
                }
                else if (gvPurchasedReviews.Rows[i].Cells[4].Text.IndexOf("Expired") == 0)
                {
                    gvPurchasedReviews.Rows[i].Cells[4].BackColor = System.Drawing.Color.Pink;
                    gvPurchasedReviews.Rows[i].Cells[4].Font.Bold = true;
                }
                else if (gvPurchasedReviews.Rows[i].Cells[4].Text.Contains(" In Site License '"))
                {
                    gvPurchasedReviews.Rows[i].Cells[4].BackColor = System.Drawing.Color.LightGray;
                    gvPurchasedReviews.Rows[i].Cells[4].ToolTip = "Subscriptions for reviews associated with a Site License can't be purchased individually";
                    DropDownList ddl = (DropDownList)gvPurchasedReviews.Rows[i].Cells[5].FindControl("ddlExtendReview");
                    if (ddl != null)
                    {
                        ddl.Enabled = false;
                        ddl.ToolTip = "Subscriptions for reviews associated with a Site License can't be purchased individually";
                    }
                }
            }
        }
    }

    protected void ddlExtendAccount_SelectedIndexChanged(object sender, EventArgs e)
    {
        GridViewRow gvr = (GridViewRow)(((Control)sender).NamingContainer);
        string contactID = gvr.Cells[0].Text;
        string accountCreatorID = gvr.Cells[2].Text;

        DropDownList dropdownlist1 = (DropDownList)gvr.FindControl("ddlExtendAccount");
        string numberMonths = dropdownlist1.SelectedValue;

        bool isAdmDB = true;
        if (Utils.GetSessionString("Draft_Bill_ID") == null)
            MakeDraftBill();
        Utils.ExecuteSP(isAdmDB, Server, "st_BillAddContactExtension", accountCreatorID,
            contactID, numberMonths, Utils.GetSessionString("Draft_Bill_ID"));
        
        buildPurchasedAccountsGrid();
        getDraftBill();
        //calculateTotalFees();
    }
    protected void lbNewAccount_Click(object sender, EventArgs e)
    {
        pnlPurchaseAccount.Visible = true;
       
        bool isAdmDB = true;
        if (Utils.GetSessionString("Draft_Bill_ID") == null)
            MakeDraftBill();
        Utils.ExecuteSP(isAdmDB, Server, "st_BillAddNewContact",
            Utils.GetSessionString("Contact_ID"), 3, Utils.GetSessionString("Draft_Bill_ID"));//consider revisiting, we are hard coding the minimum amount
        //buildNewAccountsGrid();
        getDraftBill();
        //calculateTotalFees();
    }


    //protected void lbCancel_Click(object sender, EventArgs e)
    //{
    //    pnlPurchaseAccount.Visible = false;
    //}
    protected void lbNewReview_Click(object sender, EventArgs e)
    {
        pnlPurchaseReview.Visible = true;
        
        bool isAdmDB = true;
        if (Utils.GetSessionString("Draft_Bill_ID") == null)
            MakeDraftBill();
        Utils.ExecuteSP(isAdmDB, Server, "st_BillAddNewReview",
            Utils.GetSessionString("Contact_ID"), 3, Utils.GetSessionString("Draft_Bill_ID"));//consider revisiting, we are hard coding the minimum amount
        //buildNewReviewsGrid();
        getDraftBill();
        //calculateTotalFees();
    }

    protected void ddlExtendReview_SelectedIndexChanged(object sender, EventArgs e)
    {
        GridViewRow gvr = (GridViewRow)(((Control)sender).NamingContainer);
        string reviewID = gvr.Cells[0].Text;
        string accountCreatorID = gvr.Cells[2].Text;

        DropDownList dropdownlist1 = (DropDownList)gvr.FindControl("ddlExtendReview");
        string numberMonths = dropdownlist1.SelectedValue;
        if (Utils.GetSessionString("Draft_Bill_ID") == null)
            MakeDraftBill();
        bool isAdmDB = true;
        Utils.ExecuteSP(isAdmDB, Server, "st_BillReviewExtend", accountCreatorID,
            reviewID, numberMonths, Utils.GetSessionString("Draft_Bill_ID"));
        buildPurchasedReviewsGrid();
        getDraftBill();
        //calculateTotalFees();
    }
    protected void dllExtendGhostAccount(object sender, EventArgs e)
    {
        GridViewRow gvr = (GridViewRow)(((Control)sender).NamingContainer);
        string lineID = gvr.Cells[0].Text;
        string accountCreatorID = Utils.GetSessionString("Contact_ID");

        //DropDownList dropdownlist1 = (DropDownList)gvr.FindControl("st_BillExtendGhost");
        string numberMonths = (sender as DropDownList).SelectedValue;
        if (Utils.GetSessionString("Draft_Bill_ID") == null)
            MakeDraftBill();
        bool isAdmDB = true;
        Utils.ExecuteSP(isAdmDB, Server, "st_BillExtendGhost", accountCreatorID, lineID,
            numberMonths, Utils.GetSessionString("Draft_Bill_ID"));

        //buildPurchasedAccountsGrid();
        //buildNewAccountsGrid();
        getDraftBill();
        //calculateTotalFees();
    }
    protected void gvRemoveGhost_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        if (Utils.GetSessionString("Draft_Bill_ID") == null)
            MakeDraftBill(); 
        int index = Convert.ToInt32(e.CommandArgument);
        string lineID = (string)(sender as GridView).DataKeys[index].Value;
        switch (e.CommandName)
        {
            case "REMOVE":
                bool isAdmDB = true;
                Utils.ExecuteSP(isAdmDB, Server, "st_BillRemoveGhost", Utils.GetSessionString("Contact_ID"),
                lineID, Utils.GetSessionString("Draft_Bill_ID"));
                break;

            default:
                break;
        }
        //buildPurchasedAccountsGrid();
        //buildNewAccountsGrid();
        getDraftBill();
        //calculateTotalFees();
    }
    //protected void ddlExtendTmpReview_SelectedIndexChanged(object sender, EventArgs e)
    //{
    //    GridViewRow gvr = (GridViewRow)(((Control)sender).NamingContainer);
    //    string LineID = gvr.Cells[0].Text;
    //    string reviewCreatorID = Utils.GetSessionString("Contact_ID");
    //    DropDownList dropdownlist1 = (DropDownList)gvr.FindControl("ddlExtendTmpReview");
    //    string numberMonths = dropdownlist1.SelectedValue;

    //    bool isAdmDB = true;
    //    Utils.ExecuteSP(isAdmDB, Server, "st_ReviewTmpEdit", reviewIDTmp,
    //        numberMonths, "NUM_MONTHS");

    //    buildPurchasedReviewsGrid();
    //    buildNewReviewsGrid();
    //    calculateTotalFees();
    //}

    private void calculateTotalFees()
    {
        // add up the totals
        int totalAccountFees = 0;
        int totalNewAccountFees = 0;
        int totalReviewFees = 0;
        int totalNewReviewFees = 0;
        bool addVAT = false;
        lblVatPercentage.Visible = false;
        lblVatTotal.Visible = false;
        lblVatMessage.Visible = false;

        bool isAdmDB = true, canBuy = false;
        float vatRate = 0;
        IDataReader idr = Utils.GetReader(isAdmDB, "st_VatGet",
            int.Parse(Utils.GetSessionString("CountryID")));
        if (idr.Read())
        {
            vatRate = float.Parse(idr["VAT_RATE"].ToString());
            addVAT = true;
        }
        idr.Close();



        for (int i = 0; i < gvPurchasedAccounts.Rows.Count; i++)
        {
            totalAccountFees += int.Parse(gvPurchasedAccounts.Rows[i].Cells[7].Text);
        }
        for (int i = 0; i < gvNewAccounts.Rows.Count; i++)
        {
            totalNewAccountFees += int.Parse(gvNewAccounts.Rows[i].Cells[4].Text);
        }
        for (int j = 0; j < gvPurchasedReviews.Rows.Count; j++)
        {
            totalReviewFees += int.Parse(gvPurchasedReviews.Rows[j].Cells[7].Text);
        }
        for (int i = 0; i < gvNewReviews.Rows.Count; i++)
        {
            totalNewReviewFees += int.Parse(gvNewReviews.Rows[i].Cells[4].Text);
        }
        //canBuy = (totalAccountFees > 0 || totalNewAccountFees > 0 || totalNewReviewFees > 0 || totalReviewFees > 0);
        canBuy = (totalAccountFees + totalNewAccountFees + totalNewReviewFees + totalReviewFees >= 10);
        lblAccountFees.Text = "£" + (totalAccountFees + totalNewAccountFees).ToString();
        lblReviewFees.Text = "£" + (totalReviewFees + totalNewReviewFees).ToString();
        lblNominalFee.Text = "£" + (totalAccountFees + totalReviewFees +
            totalNewAccountFees + totalNewReviewFees).ToString();
        if (addVAT == true)
        {
            lblVatPercentage.Text = vatRate.ToString() + "%";
            lblVatPercentage.Visible = true;
            lblVatTotal.Visible = true;
            lblVatMessage.Visible = true;

            lblVatTotal.Text = "£" + (float.Parse(lblNominalFee.Text.Substring(1)) * 
                (float.Parse(lblVatPercentage.Text.Substring(0, lblVatPercentage.Text.Length - 1)) / 100)).ToString("#0.00");
            lblTotalFees.Text = "£" + (float.Parse(lblNominalFee.Text.Substring(1)) + float.Parse(lblVatTotal.Text.Substring(1))).ToString("#0.00");
        }
        else
        {
            lblTotalFees.Text = "£" + (int.Parse(lblNominalFee.Text.Substring(1)));
        }
        cmdPurchase.Enabled = canBuy;
        lblMinAmount.Visible = !canBuy;
    }
    protected void ddlStartDate_SelectedIndexChanged(object sender, EventArgs e)
    {
        //GridViewRow gvr = (GridViewRow)(((Control)sender).NamingContainer);
        //string contactIDTmp = gvr.Cells[0].Text;
        //DropDownList dropdownlist2 = (DropDownList)gvr.FindControl("ddlStartDate");
        //string startDate = dropdownlist2.SelectedValue;

        //bool isAdmDB = true;
        //Utils.ExecuteSP(isAdmDB, Server, "st_ContactTmpEdit", contactIDTmp,
        //    startDate, "STARTDATE");

        ////buildPurchasedAccountsGrid();
        ////buildNewAccountsGrid();
        //getDraftBill();
        //calculateTotalFees();
    }
    
    protected void ddlStartDate1_SelectedIndexChanged(object sender, EventArgs e)
    {
        //GridViewRow gvr = (GridViewRow)(((Control)sender).NamingContainer);
        //string reviewIDTmp = gvr.Cells[0].Text;
        //DropDownList dropdownlist2 = (DropDownList)gvr.FindControl("ddlStartDate1");
        //string startDate = dropdownlist2.SelectedValue;

        //bool isAdmDB = true;
        //Utils.ExecuteSP(isAdmDB, Server, "st_ReviewTmpEdit", reviewIDTmp,
        //    startDate, "STARTDATE");

        //buildPurchasedReviewsGrid();
        ////buildNewReviewsGrid();
        //calculateTotalFees();
    }
    //protected void gvNewReviews_RowCommand(object sender, GridViewCommandEventArgs e)
    //{
    //    int index = Convert.ToInt32(e.CommandArgument);
    //    string ReviewID = (string)gvNewReviews.DataKeys[index].Value;
    //    switch (e.CommandName)
    //    {
    //        case "REMOVE":
    //            bool isAdmDB = true;
    //            Utils.ExecuteSP(isAdmDB, Server, "st_ReviewTmpClear",
    //            ReviewID, "REVIEW");
    //            break;

    //        default:
    //            break;
    //    }
    //    buildPurchasedReviewsGrid();
    //    buildNewReviewsGrid();
    //    calculateTotalFees();
    //}
    protected void cmdPurchase_Click(object sender, EventArgs e)
    {
        if (lblTotalFees.Text != "£0")
        {
            TableRow tableRow;
            TableCell tableCell;
            tableRow = new TableRow();
            
            tableRow.BackColor = System.Drawing.Color.White;
            tableRow.Font.Bold = false;
            tableRow.Height = 400;

            tableCell = new TableCell();
            bool isAdmDB = true;
            IDataReader idr = Utils.GetReader(isAdmDB, "st_ConditionsGet", "0");
            while (idr.Read())
            {
                tableCell.Text = idr["CONDITIONS"].ToString();
            }
            idr.Close();
            
            tableRow.Cells.Add(tableCell);
            tbTerms.Rows.Add(tableRow);
            
            pnTermsAndConditions.Visible = true;

            pnlPurchasedAccounts.ForeColor = System.Drawing.Color.SlateGray;
            gvPurchasedAccounts.Enabled = false;
            lbNewAccount.Enabled = false;

            pnlPurchaseAccount.ForeColor = System.Drawing.Color.SlateGray;
            gvNewAccounts.Enabled = false;

            pnlExistingReviews.ForeColor = System.Drawing.Color.SlateGray;
            gvPurchasedReviews.Enabled = false;
            lbNewReview.Enabled = false;

            pnlPurchaseReview.ForeColor = System.Drawing.Color.SlateGray;
            gvNewReviews.Enabled = false;

            cmdPurchase.Enabled = false;
            BTAddExistingReview.Enabled = false;
            BTAddExistingAccount.Enabled = false;
        }
    }
    protected void cmdAgree_Click(object sender, EventArgs e)
    {

        //Context.Items["lblName"] = lblName.Text;
        
        Context.Items["lblEmailAddress"] = lblEmailAddress.Text;
        Utils.SetSessionString("lblEmailAddress", lblEmailAddress.Text);
        cmdPurchase.Enabled = false;
        pnTermsAndConditions.Visible = false;
        pnlProceedToWPM.Visible = true;
        BTAddExistingReview.Enabled = false;
        BTAddExistingAccount.Enabled = false;


        /*
        // added for New purchasing test
        pnlTestContinue.Visible = false;
        if (Utils.GetSessionString("IsAdm") == "True")
        {
            pnlTestContinue.Visible = true;
        }*/

            //Server.Transfer("JumpToWPM.aspx?ID=" ,true);

        }
    //protected void WPMSuccess(object sender, EventArgs e)
    //{
    //    bool isAdmDB = true;
    //    // 1. update the expiry date of the existing accounts and reviews
    //    Utils.ExecuteSP(isAdmDB, Server, "st_ContactExtendAccount",
    //        Utils.GetSessionString("Contact_ID"));
    //    Utils.ExecuteSP(isAdmDB, Server, "st_ContactExtendReview",
    //        Utils.GetSessionString("Contact_ID"));

    //    // 2. change the tmp accounts and reviews into real accounts and reviews
    //    Utils.ExecuteSP(isAdmDB, Server, "st_ContactTmpMakeReal",
    //        Utils.GetSessionString("Contact_ID"));
    //    Utils.ExecuteSP(isAdmDB, Server, "st_ReviewTmpMakeReal",
    //        Utils.GetSessionString("Contact_ID"));

    //    // 3. fill the billing information in TB_BILL
    //    string Review_ID = Utils.GetSessionString("Review_ID");
    //    SqlParameter[] paramList = new SqlParameter[5];
    //    paramList[0] = new SqlParameter("@CONTACT_ID", SqlDbType.BigInt, 8, ParameterDirection.Input,
    //        true, 0, 0, null, DataRowVersion.Default, Utils.GetSessionString("Contact_ID"));
    //    paramList[1] = new SqlParameter("@NOMINAL_PRICE", SqlDbType.BigInt, 8, ParameterDirection.Input,
    //        true, 0, 0, null, DataRowVersion.Default, lblNominalFee.Text.Substring(1));
    //    paramList[2] = new SqlParameter("@VAT", SqlDbType.NVarChar,50, ParameterDirection.Input,
    //        true, 0, 0, null, DataRowVersion.Default, lblVatTotal.Text.Substring(1));
    //    paramList[3] = new SqlParameter("@DUE_PRICE", SqlDbType.NVarChar, 50, ParameterDirection.Input,
    //        true, 0, 0, null, DataRowVersion.Default, lblTotalFees.Text.Substring(1));
    //    paramList[4] = new SqlParameter("@BILL_ID", SqlDbType.BigInt, 8, ParameterDirection.Output,
    //        true, 0, 0, null, DataRowVersion.Default, "");
    //    Utils.ExecuteSPWithReturnValues(isAdmDB, Server, "st_BillCreate", paramList);
    //    int billID = int.Parse(paramList[4].Value.ToString());

    //    // 4. create a line for each purchase in TB_BILL_LINE
    //    // put the account extension purchases into TB_BILL_LINE
    //    string numberMonths = "0";
    //    for (int i = 0; i < gvPurchasedAccounts.Rows.Count; i++)
    //    {
    //        GridViewRow row = gvPurchasedAccounts.Rows[i];
    //        DropDownList ddl = ((DropDownList)row.FindControl("ddlExtendAccount"));
    //        numberMonths = ddl.SelectedValue;
    //        if (numberMonths != "0")
    //        {
    //            Utils.ExecuteSP(isAdmDB, Server, "st_BillLineCreate",
    //                billID, gvPurchasedAccounts.Rows[i].Cells[0].Text, numberMonths,
    //                "ExtendAccount");
    //        }
    //    }
    //    // put the review extension purchases into TB_BILL_LINE
    //    for (int i = 0; i < gvPurchasedReviews.Rows.Count; i++)
    //    {
    //        GridViewRow row = gvPurchasedReviews.Rows[i];
    //        DropDownList ddl = ((DropDownList)row.FindControl("ddlExtendReview"));
    //        numberMonths = ddl.SelectedValue;
    //        if (numberMonths != "0")
    //        {
    //            Utils.ExecuteSP(isAdmDB, Server, "st_BillLineCreate",
    //                billID, gvPurchasedReviews.Rows[i].Cells[0].Text, numberMonths,
    //                "ExtendReview");
    //        }
    //    }
    //    // put the new accounts into TB_BILL_LINE
    //    Utils.ExecuteSP(isAdmDB, Server, "st_BillLineCreate",
    //        billID, Utils.GetSessionString("Contact_ID"), "0", "NewAccounts");
    //    // put the new reviews into TB_BILL_LINE
    //    Utils.ExecuteSP(isAdmDB, Server, "st_BillLineCreate",
    //        billID, Utils.GetSessionString("Contact_ID"), "0", "NewReviews");

    //    // 5. send an email to the user saying what was purchased and the totals. Would/Could this
    //    //    be the invoice?


    //    // 6. clear all of the extensions and temp tables
    //    isAdmDB = true;
    //    Utils.ExecuteSP(isAdmDB, Server, "st_ContactAccountExtensionClear",
    //        Utils.GetSessionString("Contact_ID"));

    //    Utils.ExecuteSP(isAdmDB, Server, "st_ContactTmpClear",
    //        Utils.GetSessionString("Contact_ID"), "CREATOR");

    //    Utils.ExecuteSP(isAdmDB, Server, "st_ContactReviewExtensionClear",
    //        Utils.GetSessionString("Contact_ID"));

    //    Utils.ExecuteSP(isAdmDB, Server, "st_ReviewTmpClear",
    //        Utils.GetSessionString("Contact_ID"), "CREATOR");

        
        
    //    Response.Redirect("Summary.aspx");
        
        
    //}
    protected void cmdDisagree_Click(object sender, EventArgs e)
    {
        pnTermsAndConditions.Visible = false;
        
        pnlPurchasedAccounts.ForeColor = System.Drawing.Color.Black;
        gvPurchasedAccounts.Enabled = true;
        lbNewAccount.Enabled = true;

        pnlPurchaseAccount.ForeColor = System.Drawing.Color.Black;
        gvNewAccounts.Enabled = true;

        pnlExistingReviews.ForeColor = System.Drawing.Color.Black;
        gvPurchasedReviews.Enabled = true;
        lbNewReview.Enabled = true;

        pnlPurchaseReview.ForeColor = System.Drawing.Color.Black;
        gvNewReviews.Enabled = true;

        cmdPurchase.Enabled = true;
    }
    protected void cmdCancel_Click(object sender, EventArgs e)
    {
        
        /*
        pnlContactAddress.Visible = false;

        pnlPurchasedAccounts.ForeColor = System.Drawing.Color.Black;
        gvPurchasedAccounts.Enabled = true;
        lbNewAccount.Enabled = true;

        pnlPurchaseAccount.ForeColor = System.Drawing.Color.Black;
        gvNewAccounts.Enabled = true;

        pnlExistingReviews.ForeColor = System.Drawing.Color.Black;
        gvPurchasedReviews.Enabled = true;
        lbNewReview.Enabled = true;

        pnlPurchaseReview.ForeColor = System.Drawing.Color.Black;
        gvNewReviews.Enabled = true;

        cmdPurchase.Enabled = true;
        */
        Response.Redirect("Summary.aspx");
    }
    protected void cmdSaveVerify_Click(object sender, EventArgs e)
    {
        
        if (tbEuVatNumber.Text != "Enter if available")
        {
            //euVAT.checkVatRequest req = new euVAT.checkVatRequest();
            string cCode = "GB", VatN = "645119738", name, address;
            bool result;
            //req.countryCode = "GB";
            //req.vatNumber = "645119738";
            euVAT.checkVatPortTypeClient client = new euVAT.checkVatPortTypeClient("checkVatPort");

            try
            {
                string something = client.checkVat(ref cCode, ref VatN, out result, out name, out address);
                Console.WriteLine(name);
            }
            catch (Exception )
            {
                // we are offline so we need to work around this.
               
            }

            //string something = client.checkVat(ref cCode, ref VatN, out result, out name, out address);
            //Console.WriteLine(name);
        }
        
        
        //bool isAdmDB;
        if ((tbPostalAddress.Text == "") || (ddlCountries.SelectedIndex == 0))
        {
            lblContactDetails.Text = "Please enter a valid postal address and select your country";
        }
        else
        {
            
            bool isAdmDB = true;
            Utils.ExecuteSP(isAdmDB, Server, "st_BillAddressEdit",
                Utils.GetSessionString("Contact_ID"), tbOrganization.Text,
                tbPostalAddress.Text, ddlCountries.SelectedValue, tbEuVatNumber.Text);
            Utils.SetSessionString("CountryID", ddlCountries.SelectedValue);
            
            pnlContactAddress.Visible = false;

            buildPurchasedAccountsGrid();
            //buildNewAccountsGrid();
            buildPurchasedReviewsGrid();
            //calculateTotalFees();
            getDraftBill();
            //calculateTotalFees();
            pnlPurchaseAccount.Visible = true;
            pnlPurchasedAccounts.Visible = true;
            pnlPurchaseReview.Visible = true;
            pnlExistingReviews.Visible = true;
            pnlTotals.Visible = true;
        }
    }
    protected void getDraftBill()
    {
        //DateTime dayCreated;
        //DateTime dayExpires;
        DateTime today = DateTime.Today;
        string reviewID = "";
        string contactID = "";
        // create the data source for new accounts
        DataTable dt = new DataTable();
        System.Data.DataRow newrow;

        dt.Columns.Add(new DataColumn("Bill_Line_ID", typeof(string)));
        dt.Columns.Add(new DataColumn("CREATOR_ID", typeof(string)));
        dt.Columns.Add(new DataColumn("DATE_CREATED", typeof(string)));
        dt.Columns.Add(new DataColumn("MONTHS_CREDIT", typeof(string)));
        dt.Columns.Add(new DataColumn("COST", typeof(string)));
        Dictionary<string, string> newAccounts = new Dictionary<string,string>();
        // create the data source for new reviews
        DataTable dt2 = new DataTable();
        System.Data.DataRow newrow2;
        dt2.Columns.Add(new DataColumn("BILL_LINE_ID", typeof(string)));
        dt2.Columns.Add(new DataColumn("CREATOR_ID", typeof(string)));
        dt2.Columns.Add(new DataColumn("DATE_CREATED", typeof(string)));
        dt2.Columns.Add(new DataColumn("MONTHS_CREDIT", typeof(string)));
        dt2.Columns.Add(new DataColumn("COST", typeof(string)));
        Dictionary<string, string> newReviews = new Dictionary<string, string>();
        
        bool isAdmDB = true;
        IDataReader idr = Utils.GetReader(isAdmDB, "st_BillGetDraft", Utils.GetSessionString("Contact_ID"));
        if (idr.Read())
        {
            //we could get more details on the bill here, think about this
            if(idr["BILL_ID"].ToString() != null && idr["BILL_ID"].ToString() != "")
                Utils.SetSessionString("Draft_Bill_ID", idr["BILL_ID"].ToString());
            idr.NextResult(); //second reader returns the bill lines...
            while (idr.Read())
            {
                //option 1: if it's an account
                if (idr["TYPE_NAME"].ToString() == "Professional")
                {
                    if (idr["AFFECTED_ID"].ToString() == null || idr["AFFECTED_ID"].ToString() == "")
                    {//it's a ghost account
                        newrow = dt.NewRow();
                        newrow["Bill_Line_ID"] = idr["LINE_ID"].ToString();
                        newrow["CREATOR_ID"] = idr["PURCHASER_CONTACT_ID"].ToString();
                        newrow["MONTHS_CREDIT"] = idr["MONTHS_CREDIT"].ToString();
                        newrow["COST"] = idr["COST"].ToString();
                        dt.Rows.Add(newrow);
                        pnlPurchaseAccount.Visible = true;
                        newAccounts.Add(newrow["Bill_Line_ID"].ToString(), newrow["MONTHS_CREDIT"].ToString());
                    }
                    else 
                    {//it's an existing account
                        
                        string accountCreatorID = "";
                        for (int i = 0; i < gvPurchasedAccounts.Rows.Count; i++)
                        {
                            contactID = gvPurchasedAccounts.Rows[i].Cells[0].Text;
                            accountCreatorID = gvPurchasedAccounts.Rows[i].Cells[2].Text;
                            if (idr["AFFECTED_ID"].ToString() == contactID)
                            {
                                GridViewRow row = gvPurchasedAccounts.Rows[i];
                                DropDownList ddl = ((DropDownList)row.FindControl("ddlExtendAccount"));
                                if (ddl.Enabled == true)
                                {
                                    ddl.SelectedValue = idr["MONTHS_CREDIT"].ToString();
                                    gvPurchasedAccounts.Rows[i].Cells[7].Text = idr["COST"].ToString();
                                }
                                else if (idr["MONTHS_CREDIT"].ToString() != "0")
                                {
                                    //remove site license account
                                    Utils.ExecuteSP(isAdmDB, Server, "st_BillAddContactExtension", accountCreatorID,
                                                contactID, 0, Utils.GetSessionString("Draft_Bill_ID"));
                                    gvPurchasedAccounts.Rows[i].Cells[7].Text = "0";
                                }
                                else gvPurchasedAccounts.Rows[i].Cells[7].Text = idr["COST"].ToString();
                            }
                        }
                    }
                }
                else if (idr["TYPE_NAME"].ToString() == "Shareable Review")
                { //option 2: if it's a review
                    if (idr["AFFECTED_ID"].ToString() == null || idr["AFFECTED_ID"].ToString() == "")
                    {//it's a ghost review
                        newrow2 = dt2.NewRow();
                        newrow2["BILL_LINE_ID"] = idr["LINE_ID"].ToString();
                        newrow2["CREATOR_ID"] = idr["PURCHASER_CONTACT_ID"].ToString();
                        newrow2["MONTHS_CREDIT"] = idr["MONTHS_CREDIT"].ToString();
                        //newrow["COST"] = (int.Parse(idr["MONTHS_CREDIT"].ToString()) * 10).ToString();
                        newrow2["COST"] = idr["COST"].ToString();
                        dt2.Rows.Add(newrow2);
                        pnlPurchaseReview.Visible = true;
                        newReviews.Add(newrow2["BILL_LINE_ID"].ToString(), newrow2["MONTHS_CREDIT"].ToString());
                    }
                    else
                    {//it's an existing review
                        
                        for (int i = 0; i < gvPurchasedReviews.Rows.Count; i++)
                        {
                            reviewID = gvPurchasedReviews.Rows[i].Cells[0].Text;
                            if (idr["AFFECTED_ID"].ToString() == reviewID)
                            {
                                GridViewRow row = gvPurchasedReviews.Rows[i];
                                DropDownList ddl = ((DropDownList)row.FindControl("ddlExtendReview"));
                                if (ddl.Enabled == true)
                                {
                                    ddl.SelectedValue = idr["MONTHS_CREDIT"].ToString();
                                    gvPurchasedReviews.Rows[i].Cells[7].Text = idr["COST"].ToString();
                                }
                                else if (idr["MONTHS_CREDIT"].ToString() != "0")
                                {
                                    Utils.ExecuteSP(isAdmDB, Server, "st_BillReviewExtend", gvPurchasedReviews.Rows[i].Cells[2].Text,
                                            reviewID, 0, Utils.GetSessionString("Draft_Bill_ID"));
                                    gvPurchasedReviews.Rows[i].Cells[7].Text = "0";
                                }
                                else gvPurchasedReviews.Rows[i].Cells[7].Text = idr["COST"].ToString();
                            }
                            //idr.Close();
                        }
                    }
                }
            }
        }
        idr.Close();
        gvNewAccounts.DataSource = dt;
        gvNewAccounts.DataBind();
        if (dt.Rows.Count > 0) gvNewAccounts.Visible = true;
        gvNewReviews.DataSource = dt2;
        gvNewReviews.DataBind();
        if (dt2.Rows.Count > 0) gvNewReviews.Visible = true;
        //// go through each row and setup the extendby ddl, do it twice, for new(ghost)accounts and new(ghost)reviews
        for (int i = 0; i < gvNewAccounts.Rows.Count; i++)
        {
            DropDownList ddl = ((DropDownList)gvNewAccounts.Rows[i].FindControl("ddlExtendTmpAccount"));
            string mavala = gvNewAccounts.Rows[i].Cells[0].Text;
            ddl.SelectedValue = newAccounts[gvNewAccounts.Rows[i].Cells[0].Text];
        }
        for (int i = 0; i < gvNewReviews.Rows.Count; i++)
        {
            DropDownList ddl = ((DropDownList)gvNewReviews.Rows[i].FindControl("ddlExtendTmpReview"));
            ddl.SelectedValue = newReviews[gvNewReviews.Rows[i].Cells[0].Text];
        }
        calculateTotalFees();
    }
    protected void MakeDraftBill()
    {
        bool isAdmDB = true;
        string Review_ID = Utils.GetSessionString("Review_ID");
        SqlParameter[] paramList = new SqlParameter[5];
        paramList[0] = new SqlParameter("@CONTACT_ID", SqlDbType.BigInt, 8, ParameterDirection.Input,
            true, 0, 0, null, DataRowVersion.Default, Utils.GetSessionString("Contact_ID"));
        paramList[1] = new SqlParameter("@NOMINAL_PRICE", SqlDbType.BigInt, 8, ParameterDirection.Input,
            true, 0, 0, null, DataRowVersion.Default, lblNominalFee.Text.Substring(1));
        paramList[2] = new SqlParameter("@VAT", SqlDbType.NVarChar, 50, ParameterDirection.Input,
            true, 0, 0, null, DataRowVersion.Default, lblVatTotal.Text.Substring(1));
        paramList[3] = new SqlParameter("@DUE_PRICE", SqlDbType.NVarChar, 50, ParameterDirection.Input,
            true, 0, 0, null, DataRowVersion.Default, lblTotalFees.Text.Substring(1));
        paramList[4] = new SqlParameter("@BILL_ID", SqlDbType.BigInt, 8, ParameterDirection.Output,
            true, 0, 0, null, DataRowVersion.Default, "");
        Utils.ExecuteSPWithReturnValues(isAdmDB, Server, "st_BillCreate", paramList);
        int billID = int.Parse(paramList[4].Value.ToString());
        Utils.SetSessionString("Draft_Bill_ID", billID.ToString());
    }
    protected XElement MakeXMLRequest()
    {
        string billID = Utils.GetSessionString("Draft_Bill_ID");
        if (billID == null || billID == "") return null;
        
//        string msg = @"<wpmpaymentrequest msgid='000106e097b6d08e5549fcc9feb21e61'>
//                        <clientid>20023</clientid> 
//                        <pathwayid>999</pathwayid> 
//                        <pspid /> 
//                        <style><![CDATA[filename.css]]></style>
//                          <departmentid>1</departmentid> 
//                        <staffid><![CDATA[]]></staffid>
//                        <customerid><![CDATA[960]]></customerid>
//                        <title><![CDATA[]]></title>
//                        <firstname><![CDATA[WPM]]></firstname>
//                        <middlename><![CDATA[]]></middlename>
//                        <lastname><![CDATA[TEST]]></lastname>
//                        <emailfrom><![CDATA[]]></emailfrom>
//                        <toemail><![CDATA[aboffee@wpmeducation.com]]></toemail>
//                        <ccemail><![CDATA[]]></ccemail>
//                        <emailfooter><![CDATA[]]></emailfooter>
//                        <retryemailto><![CDATA[]]></retryemailto>
//                        <transactionreference><![CDATA[R960-096268]]></transactionreference >
//                        <redirecturl><![CDATA[http://redirectURL.com]]></redirecturl>
//                        <callbackurl><![CDATA[https://callbackURL.com]]></callbackurl>
//                        <cancelurl><![CDATA[http://cancelURL.com]]></cancelurl>
//                          <live>1</live> 
//                          <customfield1 /> 
//                          <customfield2 /> 
//                          <customfield3 /> 
//                          <customfield4 /> 
//                          <customfield5 /> 
//                        <payments id='1' type='P' payoption='AD'>
//                          <customfield1 /> 
//                          <customfield2 /> 
//                          <customfield3 /> 
//                          <customfield4 /> 
//                          <customfield5 /> 
//                          <customfield6 /> 
//                          <customfield7 /> 
//                          <customfield8 /> 
//                          <customfield9 /> 
//                          <customfield10 /> 
//                        <description>
//                        <![CDATA[Accommodation Deposit]]></description>
//                          <mandatory>1</mandatory> 
//                          <editable minamount='' maxamount=''>0</editable> 
//                        <payment payid='1'>
//                          <customfield1 /> 
//                          <customfield2 /> 
//                          <customfield3 /> 
//                          <customfield4 /> 
//                          <customfield5 /> 
//                          <amounttopay>125.00</amounttopay> 
//                          <amounttopayvat>18.62</amounttopayvat> 
//                          <amounttopayexvat>106.38</amounttopayexvat> 
//                          <vatrate>17.50</vatrate> 
//                          <dateofpayment>2010-05-19 10:05:09</dateofpayment> 
//                          </payment>
//                          </payments>
//                        </wpmpaymentrequest>";

        string clientID = Utils.WPMclientID;//this should be provided by WPM

        string msgid = Utils.getMD5Hash(clientID + billID);
        string pathwayid = Utils.WPMpathwayid;//this should come from WPM
        bool isAdmDB = true; 
        float VatRate = 0, currentAmount;
        int numberMonths;

        IDataReader idr = Utils.GetReader(isAdmDB, "st_VatGet",
            Utils.GetSessionString("CountryID"));
        if (idr.Read())
        {
            VatRate = float.Parse(idr["VAT_RATE"].ToString()) / 100;
        }
        else VatRate = 0;
        idr.Close();

        XElement res = new XElement("wpmpaymentrequest", new XAttribute("msgid", msgid));
        XElement tmpX = new XElement("clientid", clientID);
        //XElement paymentsX;
        res.Add(tmpX);
        res.Add(new XElement("requesttype", 1));
        res.Add(new XElement("pathwayid", pathwayid));
        res.Add(new XElement("pspid", ""));
        res.Add(new XElement("style", new XCData(Utils.WPMstyle)));//this could be styleID(int) ask WPM!
        res.Add(new XElement("departmentid", Utils.WPMdepartmentid));//this should come from WPM
        res.Add(new XElement("staffid", ""));//this should come from WPM
        res.Add(new XElement("customerid", new XCData(Utils.GetSessionString("Contact_ID"))));
        res.Add(new XElement("title", ""));
        res.Add(new XElement("firstname", new XCData(lblName.Text)));
        res.Add(new XElement("middlename", ""));
        res.Add(new XElement("lastname", ""));
        res.Add(new XElement("emailfrom", new XCData("EPPISupport@ucl.ac.uk")));
        res.Add(new XElement("toemail", new XCData(lblEmailAddress.Text)));
        res.Add(new XElement("ccemail", new XCData("EPPISupport@ucl.ac.uk")));
        res.Add(new XElement("emailfooter", new XCData("<br><br>In case this message is unexpected, please don't hesitate to contact our support staff:<br><a href='mailto:EPPISupport@ucl.ac.uk'>EPPISupport@ucl.ac.uk</a><br>")));
        res.Add(new XElement("retryemailto", new XCData("EPPISupport@ucl.ac.uk")));
        res.Add(new XElement("transactionreference", new XCData(billID)));
        res.Add(new XElement("redirecturl", new XCData(HttpContext.Current.Request.Url.ToString().Replace("Purchase.aspx", "Summary.aspx"))));
        if (Utils.USEproxyIN) res.Add(new XElement("callbackurl", new XCData(Utils.ProxyURL)));
        else res.Add(new XElement("callbackurl", new XCData(HttpContext.Current.Request.Url.ToString())));
        res.Add(new XElement("cancelurl", new XCData(HttpContext.Current.Request.Url.ToString().Replace("Purchase.aspx", "Summary.aspx"))));
        res.Add(new XElement("live", Utils.WPMisLive));
        res.Add(new XElement("customfield1", ""));
        res.Add(new XElement("customfield2", ""));
        res.Add(new XElement("customfield3", ""));
        res.Add(new XElement("customfield4", ""));
        res.Add(new XElement("customfield5", ""));
        
        idr = Utils.GetReader(isAdmDB, "st_BillGetDraft", Utils.GetSessionString("Contact_ID"));
        if (idr.Read())
        {
            //we could get more details on the bill here, think about this
            
            idr.NextResult(); //second reader returns the bill lines...
            while (idr.Read())
            {
                numberMonths = int.Parse(idr["MONTHS_CREDIT"].ToString());
                currentAmount = float.Parse(idr["COST"].ToString());
                if (numberMonths == 0 || currentAmount == 0) continue;
                //option 1: if it's an account
                if (idr["TYPE_NAME"].ToString() == "Professional")
                {
                    if (idr["AFFECTED_ID"].ToString() == null || idr["AFFECTED_ID"].ToString() == "")
                    {//it's a ghost account
                        res.Add(buildXMLRequestLine
                            (//string LineID, string description, string AmountEx, string VAT, string VATRate, string AmountInc
                                idr["LINE_ID"].ToString(),
                                "New account: " + numberMonths + " months.",
                                currentAmount.ToString(),
                                (VatRate * currentAmount).ToString(),
                                VatRate.ToString(), (VatRate * currentAmount + currentAmount).ToString()
                           )
                       );
                    }
                    else
                    {//it's an existing account
                        res.Add(buildXMLRequestLine
                                (
                                    idr["LINE_ID"].ToString(),
                                    "Account Extension: ID = " + idr["AFFECTED_ID"].ToString() + ", " + numberMonths + " months.",
                                    currentAmount.ToString(),
                                    (VatRate * currentAmount).ToString(),
                                    VatRate.ToString(), (VatRate * currentAmount + currentAmount).ToString()
                               )
                           );
                    }
                }
                else if (idr["TYPE_NAME"].ToString() == "Shareable Review")
                { //option 2: if it's a review
                    if (idr["AFFECTED_ID"].ToString() == null || idr["AFFECTED_ID"].ToString() == "")
                    {//it's a ghost review
                        res.Add(buildXMLRequestLine
                            (
                               idr["LINE_ID"].ToString(),
                                "New account: " + numberMonths + " months.",
                                currentAmount.ToString(),
                                (VatRate * currentAmount).ToString(),
                                VatRate.ToString(), (VatRate * currentAmount + currentAmount).ToString()
                           )
                       );
                    }
                    else
                    {//it's an existing review
                        res.Add(buildXMLRequestLine
                            (
                                idr["LINE_ID"].ToString(),
                                "Review Extension: ID = " + idr["AFFECTED_ID"].ToString() + ", " + numberMonths + " months.",
                                currentAmount.ToString(),
                                (VatRate * currentAmount).ToString(),
                                VatRate.ToString(), (VatRate * currentAmount + currentAmount).ToString()
                           )
                       );
                    }
                }
            }
        }
        


        return res;
    }
    protected XElement buildXMLRequestLine(string LineID, string description, string AmountEx, string VAT, string VATRate, string AmountInc )
    {// what is payoption ????
        XElement res = new XElement("payments", new XAttribute("id", LineID), new XAttribute("type", "PN"), new XAttribute("payoption", "G"));
        res.Add(new XElement("customfield1", ""));
        res.Add(new XElement("customfield2", ""));
        res.Add(new XElement("customfield3", ""));
        res.Add(new XElement("customfield4", ""));
        res.Add(new XElement("customfield5", ""));
        res.Add(new XElement("customfield6", ""));
        res.Add(new XElement("customfield7", ""));
        res.Add(new XElement("customfield8", ""));
        res.Add(new XElement("customfield9", ""));
        res.Add(new XElement("customfield10", ""));
        res.Add(new XElement("description", new XCData(description)));
        res.Add(new XElement("mandatory", "1"));
        res.Add(new XElement("editable", 0, new XAttribute("minamount", ""), new XAttribute("maxamount", "")));
        XElement subEl = new XElement("payment", new XAttribute("payid", "1"));
        subEl.Add(new XElement("customfield1", ""));
        subEl.Add(new XElement("customfield2", ""));
        subEl.Add(new XElement("customfield3", ""));
        subEl.Add(new XElement("customfield4", ""));
        subEl.Add(new XElement("customfield5", ""));

        subEl.Add(new XElement("amounttopay", AmountInc));
        subEl.Add(new XElement("amounttopayvat", VAT == "0" ? "" : VAT));
        subEl.Add(new XElement("amounttopayexvat", AmountEx));
        subEl.Add(new XElement("vatrate", VATRate == "0" ? "" : VATRate));
        subEl.Add(new XElement("dateofpayment", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
        res.Add(subEl);
        
        return res;
    }
    protected string getMD5HashOld(string input)
    {
        // step 1, calculate MD5 hash from input
        string sharedS = "WpM55ecr3tWithUs";
        System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();
        byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input + sharedS);
        byte[] hash = md5.ComputeHash(inputBytes);

        // step 2, convert byte array to hex string
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < hash.Length; i++)
        {
            sb.Append(hash[i].ToString());
        }
        return sb.ToString();
    }
    
    
    protected void BTAddExistingReview_Click(object sender, EventArgs e)
    {
        int check;
        LblAddReviewResult.Visible = false;
        LblAddReviewResult.Text = "";
        if (TbxAddReviewID.Text != "" && TbxAddReviewName.Text != "" && int.TryParse(TbxAddReviewID.Text, out check))
        {
            for (int i = 0; i < gvPurchasedReviews.Rows.Count; i++)
            {
                if (gvPurchasedReviews.Rows[i].Cells[0].Text == check.ToString()) return;
            }
            if (Utils.GetSessionString("Draft_Bill_ID") == null)
                MakeDraftBill();
            SqlParameter[] paramList = new SqlParameter[4];
            paramList[0] = new SqlParameter("@bill_ID", SqlDbType.Int);
            paramList[0].Direction = ParameterDirection.Input;
            paramList[0].Value = Utils.GetSessionString("Draft_Bill_ID");

            paramList[1] = new SqlParameter("@revID", SqlDbType.Int);
            paramList[1].Direction = ParameterDirection.Input;
            paramList[1].Value = int.Parse(TbxAddReviewID.Text);

            paramList[2] = new SqlParameter("@Rev_name", SqlDbType.NVarChar);
            paramList[2].Direction = ParameterDirection.Input;
            paramList[2].Value = TbxAddReviewName.Text;

            paramList[3] = new SqlParameter("@Result", SqlDbType.NVarChar);
            paramList[3].Direction = ParameterDirection.Output;
            paramList[3].Size = 100;
            paramList[3].Value = "";

            Utils.ExecuteSPWithReturnValues(true, Server, "st_BillAddExistingReview", paramList);
            string res = paramList[3].Value.ToString();
            if (res != "Success")
            {
                LblAddReviewResult.Visible = true;
                LblAddReviewResult.Text = res;
            }
            else
            {
                buildPurchasedReviewsGrid();
                getDraftBill();
            }
        }
    }
    protected void BTAddExistingAccount_Click(object sender, EventArgs e)
    {
        int check; 
        LblAddAccountResult.Visible = false;
        LblAddAccountResult.Text = "";
        if (TbxAddAccountID.Text != "" && TbxAddAccountEMail.Text != "" && int.TryParse(TbxAddAccountID.Text, out check))
        {
            for (int i = 0; i < gvPurchasedAccounts.Rows.Count; i++)
            {
                if (gvPurchasedAccounts.Rows[i].Cells[0].Text == check.ToString()) return;
            }
            if (Utils.GetSessionString("Draft_Bill_ID") == null)
                MakeDraftBill();
            SqlParameter[] paramList = new SqlParameter[4];
            paramList[0] = new SqlParameter("@bill_ID", SqlDbType.Int);
            paramList[0].Direction = ParameterDirection.Input;
            paramList[0].Value = Utils.GetSessionString("Draft_Bill_ID");

            paramList[1] = new SqlParameter("@ContactID", SqlDbType.Int);
            paramList[1].Direction = ParameterDirection.Input;
            paramList[1].Value = int.Parse(TbxAddAccountID.Text);

            paramList[2] = new SqlParameter("@Email", SqlDbType.NVarChar);
            paramList[2].Direction = ParameterDirection.Input;
            paramList[2].Value = TbxAddAccountEMail.Text;

            paramList[3] = new SqlParameter("@Result", SqlDbType.NVarChar);
            paramList[3].Direction = ParameterDirection.Output;
            paramList[3].Size = 100;
            paramList[3].Value = "";

            Utils.ExecuteSPWithReturnValues(true, Server, "st_BillAddExistingAccount", paramList);
            string res = paramList[3].Value.ToString();
            if (res != "Success")
            {
                LblAddAccountResult.Visible = true;
                LblAddAccountResult.Text = res;
            }
            else
            {
                buildPurchasedAccountsGrid();
                getDraftBill();
            }
        }
    }
}



//private void buildNewAccountsGrid()
//{
//    // get the tmp accounts the user is setting up
//    DataTable dt = new DataTable();
//    System.Data.DataRow newrow;

//    dt.Columns.Add(new DataColumn("CONTACT_ID", typeof(string)));
//    dt.Columns.Add(new DataColumn("CREATOR_ID", typeof(string)));
//    dt.Columns.Add(new DataColumn("DATE_CREATED", typeof(string)));
//    dt.Columns.Add(new DataColumn("MONTHS_CREDIT", typeof(string)));
//    dt.Columns.Add(new DataColumn("COST", typeof(string)));
//    bool isAdmDB = true;
//    IDataReader idr = Utils.GetReader(isAdmDB, "st_ContactTmpDetails",
//        Utils.GetSessionString("Contact_ID"), "CREATOR");
//    while (idr.Read())
//    {
//        newrow = dt.NewRow();
//        newrow["CONTACT_ID"] = idr["CONTACT_ID_TMP"].ToString();
//        newrow["CREATOR_ID"] = idr["PURCHASER_ID"].ToString();
//        newrow["MONTHS_CREDIT"] = idr["MONTHS_CREDIT"].ToString();
//        //newrow["COST"] = (int.Parse(idr["MONTHS_CREDIT"].ToString()) * 10).ToString();
//        newrow["COST"] = idr["COST"].ToString();
//        dt.Rows.Add(newrow);
//        pnlPurchaseAccount.Visible = true;
//    }
//    idr.Close();

//    gvNewAccounts.DataSource = dt;
//    gvNewAccounts.DataBind();


//    // go through each row and setup the extendby ddl
//    string contactID = "";
//    string accountCreatorID = "";
//    if (gvNewAccounts.Rows.Count == 0)
//        pnlPurchaseAccount.Visible = false;
//    for (int i = 0; i < gvNewAccounts.Rows.Count; i++)
//    {
//        contactID = gvNewAccounts.Rows[i].Cells[0].Text;
//        accountCreatorID = gvNewAccounts.Rows[i].Cells[2].Text;

//        isAdmDB = true;
//        idr = Utils.GetReader(isAdmDB, "st_ContactTmpDetails",
//            contactID, "CONTACT");
//        //Utils.GetSessionString("Contact_ID"));
//        if (idr.Read())
//        {
//            GridViewRow row = gvNewAccounts.Rows[i];
//            DropDownList ddl1 = ((DropDownList)row.FindControl("ddlStartDate"));
//            DropDownList ddl2 = ((DropDownList)row.FindControl("ddlExtendTmpAccount"));

//            ddl1.SelectedValue = idr["WHEN_TO_START"].ToString();

//            ddl2.SelectedValue = idr["NUMBER_MONTHS"].ToString();

//            gvNewAccounts.Rows[i].Cells[4].Text =
//                (int.Parse(idr["NUMBER_MONTHS"].ToString()) * 10).ToString();
//        }
//        idr.Close();
//    }
//}
//private void buildNewReviewsGrid()
//{
//    // get the tmp accounts the user is setting up
//    DataTable dt = new DataTable();
//    System.Data.DataRow newrow;

//    dt.Columns.Add(new DataColumn("REVIEW_ID", typeof(string)));
//    dt.Columns.Add(new DataColumn("CREATOR_ID", typeof(string)));
//    dt.Columns.Add(new DataColumn("DATE_CREATED", typeof(string)));
//    dt.Columns.Add(new DataColumn("MONTHS_CREDIT", typeof(string)));
//    dt.Columns.Add(new DataColumn("COST", typeof(string)));
//    bool isAdmDB = true;
//    IDataReader idr = Utils.GetReader(isAdmDB, "st_ReviewTmpDetails",
//        Utils.GetSessionString("Contact_ID"), "CREATOR");
//    while (idr.Read())
//    {
//        newrow = dt.NewRow();
//        newrow["REVIEW_ID"] = idr["REVIEW_ID_TMP"].ToString();
//        newrow["CREATOR_ID"] = idr["PURCHASER_ID"].ToString();
//        newrow["MONTHS_CREDIT"] = idr["MONTHS_CREDIT"].ToString();
//        //newrow["COST"] = (int.Parse(idr["MONTHS_CREDIT"].ToString()) * 10).ToString();
//        newrow["COST"] = idr["COST"].ToString();
//        dt.Rows.Add(newrow);
//        pnlPurchaseReview.Visible = true;
//    }
//    idr.Close();

//    gvNewReviews.DataSource = dt;
//    gvNewReviews.DataBind();


//    // go through each row and setup the extendby ddl
//    string reviewID = "";
//    string reviewCreatorID = "";
//    if (gvNewReviews.Rows.Count == 0)
//        pnlPurchaseReview.Visible = false;
//    for (int i = 0; i < gvNewReviews.Rows.Count; i++)
//    {
//        reviewID = gvNewReviews.Rows[i].Cells[0].Text;
//        reviewCreatorID = gvNewReviews.Rows[i].Cells[2].Text;

//        isAdmDB = true;
//        idr = Utils.GetReader(isAdmDB, "st_ReviewTmpDetails",
//            reviewID, "REVIEW");
//        //Utils.GetSessionString("Contact_ID"));
//        if (idr.Read())
//        {
//            GridViewRow row = gvNewReviews.Rows[i];
//            DropDownList ddl1 = ((DropDownList)row.FindControl("ddlStartDate1"));
//            DropDownList ddl2 = ((DropDownList)row.FindControl("ddlExtendTmpReview"));

//            ddl1.SelectedValue = idr["WHEN_TO_START"].ToString();

//            ddl2.SelectedValue = idr["NUMBER_MONTHS"].ToString();

//            gvNewReviews.Rows[i].Cells[4].Text =
//                (int.Parse(idr["NUMBER_MONTHS"].ToString()) * 35).ToString();
//        }
//        idr.Close();
//    }
//}
//protected void cmdAgree_Click(object sender, EventArgs e)
//    {
//        string msg = MakeXMLRequest().ToString();
//        //XDocument xd = new XDocument(MakeXMLRequest());
//        //string msg = MakeXMLRequest().ToString(SaveOptions.None).Replace("\"", "'");
//        //msg = HttpUtility.UrlEncode(msg);
//        string url = Utils.WPMServerUrl;
//        HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
//        request.Method = "POST";
//        request.ContentType = "application/x-www-form-urlencoded";
//        //The encoding might have to be chaged based on requirement
//        msg = HttpUtility.UrlEncode(msg);
//        System.Text.UTF8Encoding encoder = new System.Text.UTF8Encoding();
//        msg = "xml=" + msg ;
//        byte[] data = encoder.GetBytes(msg); //postbody is plain string of xml
//        //byte[] data = HttpUtility.UrlEncodeToBytes(msg, encoder);
//        request.ContentLength = data.Length;
//        //add session cookie
//        CookieContainer ckc = new CookieContainer(1);
//        HttpCookie co = Request.Cookies[0];
//        Cookie coo = new Cookie(co.Name, co.Value);
//        coo.Domain = "epay.ioe.ac.uk";
//        ckc.Add(coo);
//        request.CookieContainer = ckc;
//        //end of add session cookie
//        Stream reqStream = request.GetRequestStream();//add into a try to handle "server not found!" and timeouts
//        reqStream.Write(data, 0, data.Length);
//        reqStream.Close();
//        System.Net.WebResponse response = request.GetResponse();//should go into the same try to handle timeouts!!!!
//        System.IO.StreamReader reader = new System.IO.StreamReader(response.GetResponseStream());
//        string str = reader.ReadToEnd();
//        string clientID = Utils.WPMclientID;//this should be provided by WPM
//        string msgid = getMD5Hash(clientID + Utils.GetSessionString("Draft_Bill_ID"));
//        if (str == "<wpmpaymentrequest msgid='"+msgid+"'>VALID</wpmpaymentrequest>")
//        {
//            request = WebRequest.Create(url) as HttpWebRequest;
//            request.Timeout = 1000 * 60 * 10;//ten minutes
//            request.Method = "Post";
//            request.ContentType = "text/xml";
//            //The encoding might have to be chaged based on requirement
//            //encoder = new System.Text.UTF8Encoding();
//            //data = encoder.GetBytes("<wpmpaymentrequest msgid='"+msgid+"'>ROGER</wpmpaymentrequest>"); //postbody is plain string of xml
//            //request.ContentLength = data.Length;
//            //reqStream = request.GetRequestStream();
//            //reqStream.Write(data, 0, data.Length);
//            //reqStream.Close();//not sure if GetResponse is needed to let WPM get the roger! not even sure they need it...
//            //response = request.GetResponse();

//            //mark the bill as submitted
//            Utils.ExecuteSP(true, Server, "st_BillMarkAsSubmitted", Utils.GetSessionString("Contact_ID"), Utils.GetSessionString("Draft_Bill_ID"));
//            //move on to WPM
//            Server.Transfer("JumpToWPM.aspx?ID=" + msgid);
//        }
//    }