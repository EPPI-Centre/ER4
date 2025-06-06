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
using System.Text.RegularExpressions;
using Telerik.Web.UI.ExportInfrastructure;
using Telerik.Web.UI.PivotGrid.Core.Fields;
using Telerik.Web.UI.com.hisoftware.api2;
using System.Runtime.InteropServices.ComTypes;
using Telerik.Web.UI;
using System.Security.Cryptography;

public partial class Summary : System.Web.UI.Page
{

    protected void Page_Load(object sender, EventArgs e)
    {
        if (Utils.GetSessionString("Contact_ID") != null)
        {
            if (!IsPostBack)
            {
                System.Web.UI.WebControls.Label lbl = (Label)Master.FindControl("lblPageTitle");
                if (lbl != null)
                {
                    lbl.Text = "Summary of your account(s)";
                }

                Telerik.Web.UI.RadTabStrip radTs = (Telerik.Web.UI.RadTabStrip)Master.FindControl("rtsMenu");
                if (radTs != null)
                {
                    radTs.SelectedIndex = 0;
                    radTs.Tabs[0].Tabs[0].Selected = true;
                    radTs.Tabs[0].Tabs[1].Visible = false;
                    radTs.Tabs[0].Tabs[4].Width = 500;
                }
                System.Web.UI.WebControls.Label lbl1 = (Label)Master.FindControl("lblHeadingText");
                if (lbl1 != null)
                {
                    lbl1.Text = "Summary of your account(s)";
                }

                buildContactGrid();
                //buildOrganisationGrid();
                buildCreditPurchaseGrid();
                if (Utils.GetSessionString("EnableShopDebit") == "True")
                {
                    buildOutstandingFeeGrid();
                }
                buildContactPurchasesGrid();
                if (Utils.GetSessionString("HasCochraneReviews") == "Unknown")
                {
                    countCochraneReviews();
                }
            }
            Utils.SetSessionString("Credit_Purchase_ID", "");
        }
        else
        {
            Server.Transfer("Error.aspx");
        }
    }

    private void contactData(string contactID, bool isGhost)
    {
        cmdSave.Text = "Save";
        lblNewPassword.Visible = false;
        lblUsername.Visible = false;
        lblEmailAddress0.Visible = false;
        lblNewPassword.Visible = false;
        lblMissingFields.Visible = false;
        pnlAccountMessages.Visible = false;

        DateTime dayCreated;
        DateTime dayExpires;

        lblPasswordMsg.Text = "To keep your existing password leave the password fields blank.";
        Regex passwordRegex = new Regex("^.*(?=.{8,})(?=.*\\d)(?=.*[a-z])(?=.*[A-Z]).*$");

        bool isAdmDB = true;
        using (System.Data.SqlClient.SqlConnection myConnection = new System.Data.SqlClient.SqlConnection())
        {
            IDataReader idr = Utils.GetReader1(isAdmDB, "st_ContactDetails", myConnection, contactID);
            //Utils.GetSessionString("Contact_ID"));
            if (idr.Read())
            {
                if (isGhost)
                {
                    lblActivateGhostContactID.Text = contactID.ToString();
                }
                else
                {
                    lblContactID.Text = contactID.ToString();
                    tbName.Text = idr["CONTACT_NAME"].ToString();
                    tbUserName.Text = idr["USERNAME"].ToString();
                    tbEmail.Text = idr["EMAIL"].ToString();
                    tbEmailConfirm.Text = idr["EMAIL"].ToString();
                    //Match m = passwordRegex.Match(idr["PASSWORD"].ToString());
                    //if (!m.Success)
                    //{
                    //    lblNewPassword.Visible = true;
                    //    lblPasswordMsg.Text = "Your password does not meet the requirements. Please enter a new one.";
                    //    tbPassword.Text = idr["PASSWORD"].ToString();
                    //    tbPasswordConfirm.Text = idr["PASSWORD"].ToString();
                    //}
                    dayCreated = Convert.ToDateTime(idr["DATE_CREATED"].ToString());

                    if (idr["EXPIRY_DATE"].ToString() == "") dayExpires = dayCreated.Date;
                    else dayExpires = Convert.ToDateTime(idr["EXPIRY_DATE"].ToString());
                    if (dayCreated.Date == dayExpires.Date)
                    {
                        cmdSave.Text = "Save and activate";
                        lblPasswordMsg.Text = "Two notification emails will be sent to the email above <br>(username and password are sent separately for security resons).";
                        lblNewPassword.Visible = false;
                    }
                    if (idr["SEND_NEWSLETTER"].ToString() == "True")
                    {
                        cbSendNewsletter.Checked = true;
                    }
                }
            }
            idr.Close();
        }
    }

    private void buildContactGrid()
    {
        DateTime dayCreated;
        DateTime dayExpires;
        DateTime lastLogin;
        DateTime today = DateTime.Today;
        DataTable dt = new DataTable();
        System.Data.DataRow newrow;

        dt.Columns.Add(new DataColumn("CONTACT_ID", typeof(string)));
        dt.Columns.Add(new DataColumn("CONTACT_NAME", typeof(string)));
        dt.Columns.Add(new DataColumn("EMAIL", typeof(string)));
        dt.Columns.Add(new DataColumn("LAST_LOGIN", typeof(string)));
        dt.Columns.Add(new DataColumn("DATE_CREATED", typeof(string)));
        dt.Columns.Add(new DataColumn("EXPIRY_DATE", typeof(string)));
        dt.Columns.Add(new DataColumn("HOURS", typeof(string)));
        //string SQL = "SELECT CONTACT_ID, CONTACT_NAME, EMAIL, LAST_LOGIN, DATE_CREATED, MONTHS_CREDIT, " +
        //    "EXPIRY_DATE FROM tb_CONTACT WHERE CONTACT_ID = '" + Utils.GetSessionString("Contact_ID") + "'";
        //SqlDataReader sdr = Utils.ReturnReader(SQL, isAdmDB);
        bool isAdmDB = true;
        IDataReader idr = Utils.GetReader(isAdmDB, "st_ContactDetails",
            Utils.GetSessionString("Contact_ID"));
        if (idr.Read())
        {
            newrow = dt.NewRow();
            newrow["CONTACT_ID"] = idr["CONTACT_ID"].ToString();
            newrow["CONTACT_NAME"] = idr["CONTACT_NAME"].ToString();
            newrow["EMAIL"] = idr["EMAIL"].ToString();
            if ((idr["LAST_LOGIN"].ToString() == null) || (idr["LAST_LOGIN"].ToString() == ""))
                newrow["DATE_CREATED"] = "N/A";
            else
            {
                lastLogin = Convert.ToDateTime(idr["LAST_LOGIN"].ToString());
                newrow["LAST_LOGIN"] = lastLogin.ToString("dd MMM yyyy HH:mm");
            }
            newrow["HOURS"] = idr["active_hours"].ToString();

            dayCreated = Convert.ToDateTime(idr["DATE_CREATED"].ToString());
            newrow["DATE_CREATED"] = dayCreated.ToString("dd MMM yyyy");

            dayExpires = Convert.ToDateTime(idr["EXPIRY_DATE"].ToString());
            newrow["EXPIRY_DATE"] = dayExpires.ToString("dd MMM yyyy");
            if (dayExpires < today)
            {
                newrow["EXPIRY_DATE"] = dayExpires.ToString("dd MMM yyyy") + " Expired";
            }
            if (idr["SITE_LIC_ID"] != null && idr["SITE_LIC_ID"].ToString() != "")
            {
                newrow["EXPIRY_DATE"] += " In site License (ID:" + idr["SITE_LIC_ID"].ToString() + ")";
            }
            dt.Rows.Add(newrow);
        }
        idr.Close();


        gvReviewer.DataSource = dt;
        gvReviewer.DataBind();

        if (gvReviewer.Rows[0].Cells[5].Text.Contains("Expired"))
        {
            gvReviewer.Rows[0].Cells[5].BackColor = System.Drawing.Color.Pink;
            gvReviewer.Rows[0].Cells[5].Font.Bold = true;
        }
    }

    private void buildOrganisationGrid()
    {
        bool isAdmDB = true;
        DataTable dt2 = new DataTable();
        System.Data.DataRow newrow2;
        dt2.Columns.Add(new DataColumn("ORGANISATION_ID", typeof(string)));
        dt2.Columns.Add(new DataColumn("ORGANISATION_NAME", typeof(string)));
        IDataReader idr1 = Utils.GetReader(isAdmDB, "st_Organisation_Get_By_ContactID",
                Utils.GetSessionString("Contact_ID"));
        while (idr1.Read())
        {
            newrow2 = dt2.NewRow();
            newrow2["ORGANISATION_ID"] = idr1["ORGANISATION_ID"].ToString();
            newrow2["ORGANISATION_NAME"] = idr1["ORGANISATION_NAME"].ToString();
            dt2.Rows.Add(newrow2);
        }
        idr1.Close();
        gvOrganisations.DataSource = dt2;
        gvOrganisations.DataBind();

        if (dt2.Rows.Count != 0)
            pnlOrganisations.Visible = true;
    }



    private void buildCreditPurchaseGrid()
    {
        // adjust fn_CreditRemainingDetails to handle credit transfers
        lblCreditTransferInstructions1.Visible = false;
        lblCreditTransferInstructions1.Text = "&nbsp;&nbsp;&nbsp; -&nbsp;&nbsp;&nbsp; If needed, you can";
        lblCreditTransferInstructions2.Visible = false;
        lblCreditTransferInstructions2.Text = "credit between purchases.";
        lbMoveCredit.Visible = false;

        DateTime dayPurchased;
        string remaining = "";
        bool isAdmDB = true;
        DataTable dt2 = new DataTable();
        System.Data.DataRow newrow2;
        dt2.Columns.Add(new DataColumn("CREDIT_PURCHASE_ID", typeof(string)));
        dt2.Columns.Add(new DataColumn("DATE_PURCHASED", typeof(string)));
        dt2.Columns.Add(new DataColumn("CREDIT_PURCHASED", typeof(string)));
        dt2.Columns.Add(new DataColumn("CREDIT_REMAINING", typeof(string)));
        IDataReader idr1 = Utils.GetReader(isAdmDB, "st_CreditPurchasesByPurchaser", Utils.GetSessionString("Contact_ID"));
        while (idr1.Read())
        {
            newrow2 = dt2.NewRow();
            newrow2["CREDIT_PURCHASE_ID"] = idr1["tv_credit_purchase_id"].ToString();

            dayPurchased = Convert.ToDateTime(idr1["tv_date_purchased"].ToString());
            newrow2["DATE_PURCHASED"] = dayPurchased.ToString("dd MMM yyyy");

            //newrow2["DATE_PURCHASED"] = idr1["tv_date_purchased"].ToString();
            newrow2["CREDIT_PURCHASED"] = idr1["tb_credit_purchased"].ToString();
            remaining = idr1["tv_credit_remaining"].ToString();
            if (remaining == "")
                remaining = idr1["tb_credit_purchased"].ToString();
            newrow2["CREDIT_REMAINING"] = remaining;
            dt2.Rows.Add(newrow2);
        }
        idr1.Close();
        gvCreditPurchases.DataSource = dt2;
        gvCreditPurchases.DataBind();

        if (dt2.Rows.Count > 1)
        {
            // with 2 or more purchases you can now transfer between purchases
            lblCreditTransferInstructions1.Visible = true;
            lblCreditTransferInstructions2.Visible = true;
            lbMoveCredit.Visible = true;
        }

        if (dt2.Rows.Count == 0)
            lblNoCreditPurchases.Visible = true;

        for (int i = 0; i < gvCreditPurchases.Rows.Count; i++)
        {
            int test = -1;
            int.TryParse(gvCreditPurchases.Rows[i].Cells[3].Text, out test);//if parsing fails, test becomes 0, which is OK, in this case
            if (test <= 0)
            {
                gvCreditPurchases.Rows[i].Cells[5].Enabled = false;
            }
        }

    }


    private void buildOutstandingFeeGrid()
    {
        DateTime dayCreated;
        bool isAdmDB = true;
        DataTable dt2 = new DataTable();
        System.Data.DataRow newrow2;
        dt2.Columns.Add(new DataColumn("OUTSTANDING_FEE_ID", typeof(string)));
        dt2.Columns.Add(new DataColumn("DATE_CREATED", typeof(string)));
        dt2.Columns.Add(new DataColumn("OUTSTANDING_FEE", typeof(string)));
        IDataReader idr1 = Utils.GetReader(isAdmDB, "st_OutstandingFeeByAccountID", Utils.GetSessionString("Contact_ID"));
        while (idr1.Read())
        {
            newrow2 = dt2.NewRow();
            newrow2["OUTSTANDING_FEE_ID"] = idr1["OUTSTANDING_FEE_ID"].ToString();

            dayCreated = Convert.ToDateTime(idr1["DATE_CREATED"].ToString());
            newrow2["DATE_CREATED"] = dayCreated.ToString("dd MMM yyyy");

            //newrow2["DATE_CREATED"] = idr1["DATE_CREATED"].ToString();
            newrow2["OUTSTANDING_FEE"] = idr1["AMOUNT"].ToString();
            dt2.Rows.Add(newrow2);
        }
        idr1.Close();
        gvOutstandingFees.DataSource = dt2;
        gvOutstandingFees.DataBind();

        if (dt2.Rows.Count != 0)
            pnlOutstandingFees.Visible = true;
    }




    private void buildContactPurchasesGrid()
    {
        lblActivateInstructions.Visible = false;
        lblActivateInstructions.Text = "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<b>Activate</b> for a new account&nbsp;&nbsp;&nbsp;or &nbsp; <b>Transfer</b> to an existing account";
        DateTime dayCreated;
        DateTime dayExpires;
        DateTime lastLogin;
        DateTime today = DateTime.Today;
        string transferredToId = "";
        DataTable dt = new DataTable();
        System.Data.DataRow newrow;

        dt.Columns.Add(new DataColumn("CONTACT_ID", typeof(string)));
        dt.Columns.Add(new DataColumn("CONTACT_NAME", typeof(string)));
        dt.Columns.Add(new DataColumn("EMAIL", typeof(string)));
        dt.Columns.Add(new DataColumn("LAST_LOGIN", typeof(string)));
        dt.Columns.Add(new DataColumn("DATE_CREATED", typeof(string)));
        dt.Columns.Add(new DataColumn("EXPIRY_DATE", typeof(string)));
        dt.Columns.Add(new DataColumn("IS_FULLY_ACTIVE", typeof(bool)));
        dt.Columns.Add(new DataColumn("IS_STALE_AGHOST", typeof(bool)));

        bool isAdmDB = true;
        IDataReader idr = Utils.GetReader(isAdmDB, "st_ContactPurchasedDetails",
            Utils.GetSessionString("Contact_ID"));
        while (idr.Read())
        {
            newrow = dt.NewRow();
            newrow["CONTACT_ID"] = idr["CONTACT_ID"].ToString();
            newrow["IS_FULLY_ACTIVE"] = idr["IS_FULLY_ACTIVE"];
            newrow["IS_STALE_AGHOST"] = idr["IS_STALE_AGHOST"];
            if ((idr["CONTACT_NAME"].ToString() == null) || (idr["CONTACT_NAME"].ToString() == ""))
                newrow["CONTACT_NAME"] = "Not set up";
            else
                newrow["CONTACT_NAME"] = idr["CONTACT_NAME"].ToString();

            if (idr["CONTACT_NAME"].ToString().Contains("Transferred to ID:"))
                transferredToId = idr["CONTACT_NAME"].ToString().Substring(idr["CONTACT_NAME"].ToString().IndexOf(":"));

            if ((idr["EMAIL"].ToString() == null) || (idr["EMAIL"].ToString() == ""))
                newrow["EMAIL"] = "";
            else
                newrow["EMAIL"] = idr["EMAIL"].ToString();

            if ((idr["LAST_LOGIN"].ToString() == null) || (idr["LAST_LOGIN"].ToString() == ""))
            {
                newrow["LAST_LOGIN"] = "N/A";
            }
            else
            {
                lastLogin = Convert.ToDateTime(idr["LAST_LOGIN"].ToString());
                newrow["LAST_LOGIN"] = lastLogin.ToString("dd MMM yyyy HH:mm");
            }

            dayCreated = Convert.ToDateTime(idr["DATE_CREATED"].ToString());
            newrow["DATE_CREATED"] = dayCreated.ToString("dd MMM yyyy");

            if (idr["EXPIRY_DATE"].ToString() != "")
            {
                dayExpires = Convert.ToDateTime(idr["EXPIRY_DATE"].ToString());
                if (dayExpires < today)
                {
                    newrow["EXPIRY_DATE"] = dayExpires.ToString("dd MMM yyyy") + "  Expired";
                }
                else
                {
                    newrow["EXPIRY_DATE"] = dayExpires.ToString("dd MMM yyyy");
                }
                if (idr["SITE_LIC_ID"].ToString() != null && idr["SITE_LIC_ID"].ToString() != "")
                {
                    newrow["EXPIRY_DATE"] += " In Site License '" + idr["SITE_LIC_NAME"].ToString() + "' (ID:" + idr["SITE_LIC_ID"].ToString() + ")";
                }

            }
            else if ((idr["EXPIRY_DATE"].ToString() == "") && (int.Parse(idr["MONTHS_CREDIT"].ToString()) > 0)
                && idr["EMAIL"].ToString() == "")
            {
                newrow["EXPIRY_DATE"] = "Not activated: " + idr["MONTHS_CREDIT"].ToString() + " months credit";
            }
            else if ((idr["EXPIRY_DATE"].ToString() == "") && (int.Parse(idr["MONTHS_CREDIT"].ToString()) < 0)
                && idr["EMAIL"].ToString() == "")
            {
                newrow["EXPIRY_DATE"] = "Transferred " + idr["MONTHS_CREDIT"].ToString().Remove(0,1) + " months";
            }
            else if (newrow["IS_FULLY_ACTIVE"].ToString() == "False" && newrow["IS_STALE_AGHOST"].ToString() == "False")
            {
                newrow["EXPIRY_DATE"] = "Invitation sent: " + idr["MONTHS_CREDIT"].ToString() + " months credit";
            }
            else if (newrow["IS_FULLY_ACTIVE"].ToString() == "False" && newrow["IS_STALE_AGHOST"].ToString() == "True")
            {
                newrow["EXPIRY_DATE"] = "Invitation sent (expired): " + idr["MONTHS_CREDIT"].ToString() + " months credit";
            }
            dt.Rows.Add(newrow);
        }
        idr.Close();

        gvAccountPurchases.DataSource = dt;
        gvAccountPurchases.DataBind();

        if (dt.Rows.Count > 0)
        {
            lblActivateInstructions.Visible = true;
        }
        


        if (dt.Rows.Count == 0)
            lblOtherPurchasedAccounts.Visible = true;
        else
        {
            for (int i = 0; i < gvAccountPurchases.Rows.Count; i++)
            {
                if (gvAccountPurchases.Rows[i].Cells[1].Text.Contains("Not set up"))
                {
                    gvAccountPurchases.Rows[i].Cells[1].BackColor = System.Drawing.Color.PaleGreen;
                }
                if (gvAccountPurchases.Rows[i].Cells[1].Text.Contains("Not activated"))
                {
                    gvAccountPurchases.Rows[i].Cells[1].BackColor = System.Drawing.Color.Yellow;
                }
                if (gvAccountPurchases.Rows[i].Cells[5].Text.Contains("Not activated"))
                {
                    gvAccountPurchases.Rows[i].Cells[5].BackColor = System.Drawing.Color.Yellow;
                }
                if (gvAccountPurchases.Rows[i].Cells[5].Text.Contains("Transferred"))
                {
                    gvAccountPurchases.Rows[i].Cells[5].BackColor = System.Drawing.Color.Aquamarine;
                    gvAccountPurchases.Rows[i].Cells[6].Enabled = false;
                    gvAccountPurchases.Rows[i].Cells[7].Enabled = false;
                }
                if (gvAccountPurchases.Rows[i].Cells[5].Text.Contains("Invitation sent:"))
                {
                    gvAccountPurchases.Rows[i].Cells[5].BackColor = System.Drawing.Color.DarkCyan;
                    TableCell cell = gvAccountPurchases.Rows[i].Cells[6];

                    LinkButton lb = cell.Controls[0] as LinkButton;
                    if (lb != null)
                    {
                        lb.CommandName = "CANCEL1";
                        lb.Text = "Cancel";

                    }
                }

                if (gvAccountPurchases.Rows[i].Cells[5].Text.Contains("Invitation sent (expired):"))
                {
                    gvAccountPurchases.Rows[i].Cells[5].BackColor = System.Drawing.Color.Orange;
                }

                if (gvAccountPurchases.Rows[i].Cells[5].Text.Contains("Expired"))
                {
                    gvAccountPurchases.Rows[i].Cells[5].BackColor = System.Drawing.Color.Pink;
                    gvAccountPurchases.Rows[i].Cells[5].Font.Bold = true;
                }
                if (gvAccountPurchases.Rows[i].Cells[5].Text.Contains(" In Site License '"))
                {
                    gvAccountPurchases.Rows[i].Cells[5].BackColor = System.Drawing.Color.Aquamarine;
                }
                if (dt.Rows[i].ItemArray[6] != null)
                {
                    if (dt.Rows[i].ItemArray[6].ToString() == "True")
                    {
                        gvAccountPurchases.Rows[i].Cells[6].Enabled = false;
                        gvAccountPurchases.Rows[i].Cells[7].Enabled = false;
                    }
                }

            }
        }
    }
    protected void gvAccountPurchases_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        pnlActivateGhostForm.Visible = false;
        pnlActivateGhostEmailinUse.Visible = false;
        pnlActivateGhostIsDone.Visible = false;
        pnlActivateIntoExistingAccount.Visible = false;
        int index = Convert.ToInt32(e.CommandArgument);
        string ContactID = (string)gvAccountPurchases.DataKeys[index].Value;
        string monthsCredit = gvAccountPurchases.Rows[index].Cells[5].Text;
        switch (e.CommandName)
        {
            case "EDIT":
                contactData(ContactID, true);
                pnlActivateGhostForm.Visible = true;
                pnlContactDetails.Visible = false;              
                break;
            case "TRANSFER":
                //Not activated: 4 months credit
                monthsCredit = monthsCredit.Remove(0, monthsCredit.IndexOf(':') + 1).Trim();
                monthsCredit = monthsCredit.Remove(monthsCredit.IndexOf(' ')).Trim();
                monthsCredit = (int.Parse(monthsCredit) - 1).ToString();
                lblMonthsCredit.Text = monthsCredit;
                lblMonthsCredit2.Text = monthsCredit;
                lblSourceGhostAccountID.Text = ContactID;
                pnlActivateIntoExistingAccount.Visible = true;
                //pnlContactDetails.Visible = false;
                break;
            case "CANCEL1":
                pnlConfirmRevokeGhostActivation.Visible = true;
                pnlContactDetails.Visible = false;
                cmdConfirmRevokeGhostActivation.CommandArgument = ContactID;
                break;
            default:
                break;
        }
    }
    protected void cmdConfirmRevokeGhostActivation_Click(object sender, EventArgs e)
    {
        Button wb = sender as Button;
        if (wb == null) return;
        if (wb.CommandArgument != "")
        {//run the SP to revoke the ghost activation link and re-set the email address to null
            int Cid;
            int.TryParse(wb.CommandArgument, out Cid);
            if (Cid == 0) return;
            object[] paramList = new object[1];
            paramList[0] = Cid;
            Utils.ExecuteSP(true, Server, "st_GhostContactActivateRevoke", paramList);
            pnlConfirmRevokeGhostActivation.Visible = false;
            buildContactPurchasesGrid();
        }
    }








    protected void gvReviewer_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        int index = Convert.ToInt32(e.CommandArgument);
        string ContactID = (string)gvReviewer.DataKeys[index].Value;
        switch (e.CommandName)
        {
            case "EDT":
                contactData(ContactID, false);
                pnlContactDetails.Visible = true;
                pnlActivateGhostForm.Visible = false;
                break;
            default:
                break;
        }
    }

    protected void gvReviewer_RowEditing(object sender, GridViewEditEventArgs e)
    {

    }

    protected void cmdSave_Click(object sender, EventArgs e)
    {
        lblPasswordMsg.Text = "To keep your existing password leave the password fields blank.";
        lblEmailAddress0.Text = "Email address is already in use. Please select another.";
        lblUsername.Visible = false;
        lblUsername.Text = "User name is already in use. Please select another.";
        //lblMissingFields.Text = "Please fill in all of the fields. Apostrophes (') are not allowed in User names, Passwords and Emails.";
        lblMissingFields.Text = "Please fill in all of the fields. Apostrophes (') are not allowed in Usernames and Passwords.";
        lblEmailAddress0.Visible = false;
        lblNewPassword.Visible = false;
        lblMissingFields.Visible = false;
        pnlAccountMessages.Visible = false;

        bool isAdmDB = false;
        bool emailConditionsMet = true;
        bool usernameConditionsMet = true;
        bool passwordConditionsMet = true;
        bool accountConditionsMet = true;

        Regex passwordRegex = new Regex("^.*(?=.{8,})(?=.*\\d)(?=.*[a-z])(?=.*[A-Z]).*$");

        //make sure all of the fields are filled in
        //if ((tbName.Text == "" || tbUserName.Text == "" || tbUserName.Text.Contains("'") || tbEmail.Text == "" || tbEmail.Text.Contains("'") ||
        //    tbEmailConfirm.Text == "" || tbEmailConfirm.Text.Contains("'") || tbPassword.Text.Contains("'") || tbPasswordConfirm.Text.Contains("'")))
        if ((tbName.Text == "" || tbUserName.Text == "" || tbUserName.Text.Contains("'") || tbEmail.Text == "" ||
            tbEmailConfirm.Text == "" || tbPassword.Text.Contains("'") || tbPasswordConfirm.Text.Contains("'")))
        {
            pnlAccountMessages.Visible = true;
            lblMissingFields.Visible = true;
            accountConditionsMet = false;
        }
        else if (tbEmail.Text != tbEmailConfirm.Text)
        {
            pnlAccountMessages.Visible = true;
            lblMissingFields.Visible = true;
            lblMissingFields.Text = "The email and Re-enter email do not match";
            accountConditionsMet = false;
        }
        else if (tbUserName.Text.Length < 4)
        {
            pnlAccountMessages.Visible = true;
            accountConditionsMet = false;
            lblUsername.Visible = true;
            lblUsername.Text = "User name must be atleast 4 characters long";
        }
        else if (tbPassword.Text != tbPasswordConfirm.Text)
        {
            pnlAccountMessages.Visible = true;
            lblMissingFields.Visible = true;
            lblMissingFields.Text = "The password and password confirm do not match";
            accountConditionsMet = false;
        }
        // check if it has a @ in it with characters on both sides          
        else if (tbEmail.Text.IndexOf("@") < 0 ||
                (tbEmail.Text.IndexOf("@") == tbEmail.Text.Length - 1))
        {
            pnlAccountMessages.Visible = true;
            lblEmailAddress0.Visible = true;
            lblEmailAddress0.Text = "The email address does not appear to be valid";
            accountConditionsMet = false;
        }
        else if (tbPassword.Text != "")
        {
            Match m = passwordRegex.Match(tbPassword.Text.ToString());
            if (m.Success)
            {
                if (tbPassword.Text.Contains(" "))
                {
                    lblNewPassword.Visible = true;
                    accountConditionsMet = false;
                }
                else
                {
                    passwordConditionsMet = true;
                }

            }
            else
            {
                accountConditionsMet = false;
                lblNewPassword.Visible = true;
            }

        }
        if (accountConditionsMet)
        {
            // data is there. check if email and username is unique and if password conditions are met.
            string email = tbEmail.Text.Trim();
            //first go, check the username
            SqlParameter[] paramListCheckUserDetails = new SqlParameter[4];
            paramListCheckUserDetails[0] = new SqlParameter("@Uname", tbUserName.Text.Trim());
            paramListCheckUserDetails[0].Direction = ParameterDirection.InputOutput;
            paramListCheckUserDetails[0].Size = 50;

            paramListCheckUserDetails[1] = new SqlParameter("@Email", "");
            paramListCheckUserDetails[1].Direction = ParameterDirection.InputOutput;
            paramListCheckUserDetails[1].Size = 500;

            paramListCheckUserDetails[2] = new SqlParameter("@CID", SqlDbType.Int);
            paramListCheckUserDetails[2].Direction = ParameterDirection.Output;

            paramListCheckUserDetails[3] = new SqlParameter("@CONTACT_NAME", SqlDbType.NVarChar);
            paramListCheckUserDetails[3].Size = 255;
            paramListCheckUserDetails[3].Direction = ParameterDirection.Output;

            Utils.ExecuteSPWithReturnValues(true, Server, "st_ContactCheckUnameOrEmail", paramListCheckUserDetails);
            int CID = (int)paramListCheckUserDetails[2].Value;
            if (CID > 0 && CID.ToString() != Utils.GetSessionString("Contact_ID"))
            {//we found a contact with the updated(?) Username, and the ID is not the one of the current user
                pnlAccountMessages.Visible = true;
                accountConditionsMet = false;
                lblUsername.Visible = true;
                lblUsername.Text = "The Username is already in use for a different account. Please select another.";
            }
            else
            {//second go, check if the email is in use
                paramListCheckUserDetails = new SqlParameter[4];
                paramListCheckUserDetails[0] = new SqlParameter("@Uname", "");
                paramListCheckUserDetails[0].Direction = ParameterDirection.InputOutput;
                paramListCheckUserDetails[0].Size = 50;

                paramListCheckUserDetails[1] = new SqlParameter("@Email", tbEmail.Text.Trim());
                paramListCheckUserDetails[1].Direction = ParameterDirection.InputOutput;
                paramListCheckUserDetails[1].Size = 500;

                paramListCheckUserDetails[2] = new SqlParameter("@CID", SqlDbType.Int);
                paramListCheckUserDetails[2].Direction = ParameterDirection.Output;

                paramListCheckUserDetails[3] = new SqlParameter("@CONTACT_NAME", SqlDbType.NVarChar);
                paramListCheckUserDetails[3].Size = 255;
                paramListCheckUserDetails[3].Direction = ParameterDirection.Output;


                Utils.ExecuteSPWithReturnValues(true, Server, "st_ContactCheckUnameOrEmail", paramListCheckUserDetails);
                CID = 0;
                CID = (int)paramListCheckUserDetails[2].Value;
                if (CID > 0 && CID.ToString() != Utils.GetSessionString("Contact_ID"))
                {//we found a contact with the updated(?) EMAIL, and the ID is not the one of the current user
                    pnlAccountMessages.Visible = true;
                    accountConditionsMet = false;
                    lblEmailAddress0.Visible = true;
                    lblEmailAddress0.Text = "Email address is already in use for a different account. Please select another.";
                }
            }

        }

        if (accountConditionsMet)
        {
            isAdmDB = true;
            Utils.ExecuteSP(isAdmDB, Server, "st_ContactEdit",
                             tbName.Text.Trim(), tbUserName.Text.Trim(), tbEmail.Text.Trim(), tbPassword.Text.Trim(),
                             lblContactID.Text);
            pnlContactDetails.Visible = false;
            buildContactGrid();
            //buildContactPurchasesGrid();
        }
    }

    protected void lbCancelAccountEdit_Click(object sender, EventArgs e)
    {
        pnlContactDetails.Visible = false;
        pnlConfirmRevokeGhostActivation.Visible = false;
    }





    protected void cbSendNewsletter_CheckedChanged(object sender, EventArgs e)
    {
        bool isAdmDB = true;
        if (cbSendNewsletter.Checked == true)
        {
            Utils.ExecuteSP(isAdmDB, Server, "st_SendNewsletterStatus", lblContactID.Text, "True");
        }
        else
        {
            Utils.ExecuteSP(isAdmDB, Server, "st_SendNewsletterStatus", lblContactID.Text, "False");
        }
    }

    protected void cmdActivatGhost_Click(object sender, EventArgs e)
    {
        //check the email address
        pnlActivateGhostEmailinUse.Visible = false;
        lblActivateGhostMessages.Visible = false;
        string strCid = lblActivateGhostContactID.Text;
        int CID;
        int.TryParse(strCid, out CID);
        if (CID == 0)
        {//we don't know whose account we should change!
            return;
        }

        if (tbActivateGhostEmail.Text != tbActivateGhostEmail1.Text) // do not trim in the comparison
        {
            lblActivateGhostMessages.Visible = true;
            lblActivateGhostMessages.Text = "The two email fields do not match.";
            return;
        }
        if (tbActivateGhostEmail.Text.Length < 7 || tbActivateGhostEmail.Text.Trim().IndexOf("@") < 1 || tbActivateGhostEmail.Text.Trim().IndexOf("@") >= tbActivateGhostEmail.Text.Trim().Length - 2)
        {
            lblActivateGhostMessages.Visible = true;
            lblActivateGhostMessages.Text = "This email address appears to be invalid";
            return;
        }

        SqlParameter[] paramList = new SqlParameter[3];

        paramList[0] = new SqlParameter("@CONTACT_ID", SqlDbType.Int);
        paramList[0].Direction = ParameterDirection.Input;
        paramList[0].Value = CID;

        paramList[1] = new SqlParameter("@EMAIL", SqlDbType.NVarChar);
        paramList[1].Direction = ParameterDirection.Input;
        paramList[1].Value = tbActivateGhostEmail.Text;

        paramList[2] = new SqlParameter("@RESULT", SqlDbType.NVarChar);
        paramList[2].Direction = ParameterDirection.Output;
        paramList[2].Size = 100;
        paramList[2].Value = "";


        //st_CheckGhostAccountBeforeActivation will set the email field if all checks go well
        Utils.ExecuteSPWithReturnValues(true, Server, "st_CheckGhostAccountBeforeActivation", paramList);
        string res = paramList[2].Value.ToString();

        //if email is in use, suggest to transfer the credit
        if (res == "E-Mail is already in use")
        {
            pnlActivateGhostEmailinUse.Visible = true;
            lblGhostAccountTranferCredit.Text = tbActivateGhostEmail.Text.Trim();
            return;
        }
        else if (res == "E-Mail is not in use, but Contact_ID does not match a Ghost account")
        {//this means the account has been activated after the current user got the details on ghost accounts
            //refresh the ghost accounts table
            buildContactPurchasesGrid();
            lblActivateGhostMessages.Visible = true;
            lblActivateGhostMessages.Text = "This account does not appear to be valid for activation, please review the information above and try again.";
            return;
        }
        else if (res == "Valid")//if email is not in use
        {

            //show confirmation msg, refresh the ghost accounts table, this includes a system to show half/ghost accounts (those waiting for activation!)
            //send email to final user

            string LinkUI = Utils.CreateLink(CID, "ActivateGhost", "", Server);
            string BaseUrl = Request.Url.AbsoluteUri.Substring(0, Request.Url.AbsoluteUri.ToLower().IndexOf("summary.aspx"));
            BaseUrl = Utils.FixURLForHTTPS(BaseUrl);
            string CurrentUserFName = Utils.GetSessionString("ContactName");
            if (LinkUI != null && LinkUI != "" && CurrentUserFName != null && CurrentUserFName != "")
            {//send the email...
                Utils.GhostUserActivationEmail(tbActivateGhostEmail.Text, tbActivateGhostFullName.Text, CurrentUserFName, LinkUI, CID.ToString(), BaseUrl, "");
            }
            pnlActivateGhostIsDone.Visible = true;
            pnlActivateGhostForm.Visible = false;
            buildContactPurchasesGrid();
            return;

        }
        //Watch out: the follwing code is to show some error, based on the expectation that all other code branches above end with a "return;" 
        //e.g. if what follows is executed, that's because something unanticipated went wrong
        lblActivateGhostMessages.Text = "Some error occurred, please reload this page and try again. If the problem persists, please contact EPPISupport@ucl.ac.uk";
        lblActivateGhostMessages.Visible = true;
    }
    protected void cmdTransferCredit_Click(object sender, EventArgs e)
    {
        //use some new st_transferAccountCredit
        //send confirmation of transfer if the account is not already activated (EXPIRY_DATE is null), then it's added to the credit line.
        lblActivateGhostMessages.Visible = false;

        string strCid = lblActivateGhostContactID.Text;
        int CID;//this is the source (GHOST) account
        int.TryParse(strCid, out CID);
        if (CID == 0)
        {//we don't know whose account we should change!
            pnlActivateGhostEmailinUse.Visible = false;
            return;
        }
        string email = tbActivateGhostEmail.Text; //this is the email of the person that will receive the credit
        string strPCid = Utils.GetSessionString("Contact_ID");
        int PCID;//this is the source (GHOST) account
        int.TryParse(strCid, out PCID);
        if (PCID == 0)
        {//current user is no-one???!
            Server.Transfer("Error.aspx");
            return;
        }
        SqlParameter[] paramList = new SqlParameter[7];

        paramList[0] = new SqlParameter("@CONTACT_ID", SqlDbType.Int);
        paramList[0].Direction = ParameterDirection.Input;
        paramList[0].Value = CID;

        paramList[1] = new SqlParameter("@PURCHASER_ID", SqlDbType.Int);
        paramList[1].Direction = ParameterDirection.Input;
        paramList[1].Value = PCID;

        paramList[2] = new SqlParameter("@EMAIL", SqlDbType.NVarChar);
        paramList[2].Direction = ParameterDirection.Input;
        paramList[2].Value = lblGhostAccountTranferCredit.Text;

        paramList[3] = new SqlParameter("@RESULT", SqlDbType.NVarChar);
        paramList[3].Direction = ParameterDirection.Output;
        paramList[3].Size = 100;
        paramList[3].Value = "";

        paramList[4] = new SqlParameter("@CREDIT", SqlDbType.SmallInt);
        paramList[4].Direction = ParameterDirection.Output;

        paramList[5] = new SqlParameter("@NEWDATE", SqlDbType.Date);
        paramList[5].Direction = ParameterDirection.Output;

        paramList[6] = new SqlParameter("@CONTACT_NAME", SqlDbType.NVarChar);
        paramList[6].Direction = ParameterDirection.Output;
        paramList[6].Size = 255;

        Utils.ExecuteSPWithReturnValues(true, Server, "st_transferAccountCredit", paramList);
        string res = paramList[3].Value.ToString();

        if (res == "The destination account was not found")
        {
            lblActivateGhostMessages.Visible = true;
            lblActivateGhostMessages.Text = res + ", please try again or contact EPPISupport@ucl.ac.uk.";
        }
        else if (res == "There is no credit to transfer")
        {
            lblActivateGhostMessages.Visible = true;
            lblActivateGhostMessages.Text = res + ", please try again or contact EPPISupport@ucl.ac.uk.";
        }
        else if (res == "Success")
        {
            string Fullname = paramList[6].Value.ToString();
            string credit = paramList[4].Value == null ? "0" : paramList[4].Value.ToString();
            DateTime newDate;
            string StnewDate = paramList[5].Value.ToString();
            if (paramList[5].Value != null && paramList[5].Value.ToString() != "")
            {
                newDate = Convert.ToDateTime(paramList[5].Value.ToString());
                StnewDate = newDate.ToString("d MMM yyyy");
            }
            else
            {//destination account is still inactive: months credit was added instead
                StnewDate = "[Not a date: account needs to be activated.]";
            }
            Utils.GhostCreditTransferEmail(email, Fullname, gvReviewer.Rows[0].Cells[1].Text, credit, StnewDate, "");
            pnlActivateGhostEmailinUse.Visible = false;
            pnlActivateGhostForm.Visible = false;
            pnlTransferFromGhostIsDone.Visible = true;
            buildContactPurchasesGrid();
        }
        else
        {//uh? some other error!
        }
    }


    protected void cmdCancelActivateAccount_Click(object sender, EventArgs e)
    {
        pnlActivateGhostForm.Visible = false;
        pnlActivateGhostEmailinUse.Visible = false;
        pnlActivateGhostIsDone.Visible = false;
    }


















    protected void gvOrganisations_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        int index = Convert.ToInt32(e.CommandArgument);
        string organisationID = (string)gvOrganisations.DataKeys[index].Value;
        switch (e.CommandName)
        {
            case "REMOVE":
                bool isAdmDB = true;
                Utils.ExecuteSP(isAdmDB, Server, "st_OrganisationRemoveMember",
                    organisationID, Utils.GetSessionString("Contact_ID"));

                buildOrganisationGrid();

                break;
            default:
                break;
        }
    }

    protected void gvOrganisations_RowEditing(object sender, GridViewEditEventArgs e)
    {

    }

    protected void gvOrganisations_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            LinkButton lb = (LinkButton)(e.Row.Cells[2].Controls[0]);
            lb.Attributes.Add("onclick", "if (confirm('Are you sure you want to remove yourself from this organisation?') == false) return false;");
        }
    }


    protected void gvCreditPurchases_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        int index = Convert.ToInt32(e.CommandArgument);
        string creditPurchaseID = (string)gvCreditPurchases.DataKeys[index].Value;
        switch (e.CommandName)
        {
            case "DETAILS":
                Utils.SetSessionString("Credit_Purchase_ID", creditPurchaseID);
                Utils.SetSessionString("Remaining_Credit", gvCreditPurchases.Rows[index].Cells[3].Text);
                Utils.SetSessionString("Purchased_Credit", gvCreditPurchases.Rows[index].Cells[2].Text);
                Server.Transfer("AssignCredit.aspx");
                break;
            case "HISTORY":
                pnlHistory.Visible = true;
                DateTime dateExtended;
                DateTime dateExtendedPlus2Days;
                DateTime now = DateTime.Now;
                lblCreditPurchaseID.Text = creditPurchaseID;
                string sourceID = "";
                string destinationID = "";
                string amount = "";
                string numberOfMonthsExtended = "";
                string numberOfMonthsHolder = "";

                DataTable dt = new DataTable();
                System.Data.DataRow newrow;

                dt.Columns.Add(new DataColumn("CREDIT_EXTENSION_ID", typeof(string)));
                dt.Columns.Add(new DataColumn("TYPE", typeof(string)));
                dt.Columns.Add(new DataColumn("ID", typeof(string)));
                dt.Columns.Add(new DataColumn("NAME", typeof(string)));
                dt.Columns.Add(new DataColumn("DATE_EXTENDED", typeof(string)));
                dt.Columns.Add(new DataColumn("NUMBER_MONTHS", typeof(string)));
                dt.Columns.Add(new DataColumn("COST", typeof(string)));

                bool isAdmDB = true;
                IDataReader idr = Utils.GetReader(isAdmDB, "st_CreditHistoryByPurchase",
                    creditPurchaseID);
                while (idr.Read())
                {
                    newrow = dt.NewRow();
                    newrow["CREDIT_EXTENSION_ID"] = idr["tv_credit_extension_id"].ToString();
                    

                    if (idr["tv_id_extended"].ToString() == "0")
                    {
                        // 000000130000001400000010
                        sourceID = idr["tv_log_notes"].ToString().Substring(0, 8).TrimStart('0');
                        destinationID = idr["tv_log_notes"].ToString().Substring(8, 8).TrimStart('0');
                        amount = idr["tv_log_notes"].ToString().Substring(16, 8).TrimStart('0');

                        newrow["NAME"] = "Transfer £" + amount + " from PurchaseID " + sourceID + " to PurchaseID " + destinationID;

                        if (destinationID == creditPurchaseID)
                        {
                            newrow["COST"] = "-" + amount;
                        }
                        else
                        {
                            newrow["COST"] = amount;
                        }
                    }
                    else
                    {    
                        newrow["NAME"] = idr["tv_name"].ToString();
                        newrow["COST"] = idr["tv_cost"].ToString();
                    }
                    newrow["TYPE"] = idr["tv_type_extended_name"].ToString();



                    dateExtended = Convert.ToDateTime(idr["tv_date_extended"].ToString());
                    // a couple days grace...
                    dateExtendedPlus2Days = dateExtended.AddDays(2);
                    newrow["DATE_EXTENDED"] = dateExtended.ToString("dd MMM yyyy");

                    numberOfMonthsExtended = idr["tv_months_extended"].ToString();
                    if (numberOfMonthsExtended == "")
                    {
                        numberOfMonthsExtended = "0";
                    }
                    string test = idr["tv_type_extended_name"].ToString();
                    if ((idr["tv_type_extended_name"].ToString() == "Account") || (idr["tv_type_extended_name"].ToString() == "Review"))
                    {
                        if (now <= dateExtendedPlus2Days)
                        {
                            // they noticed they issue within 2 days so they can get back all of the credit
                            numberOfMonthsHolder = numberOfMonthsExtended + "!" + numberOfMonthsExtended;
                        }
                        else if (dateExtended.AddMonths(int.Parse(numberOfMonthsExtended)) > now)
                        {
                            // we might be able to return some credit so figure out how many months
                            int months = 0;
                            for (int i = 1; ; ++i)
                            {
                                // using the 2 grace days again so there really is a visual difference
                                if (now.AddMonths(i).AddDays(-2) <= dateExtended.AddMonths(int.Parse(numberOfMonthsExtended)))
                                {
                                    if (int.Parse(numberOfMonthsExtended) == i)
                                    {
                                        break;
                                    }
                                    else
                                    {
                                        months = i;
                                    }
                                }
                                else { break; }
                            }
                            if (months > 0)
                            {
                                numberOfMonthsHolder = idr["tv_months_extended"].ToString() + "!" + months.ToString();
                            }
                            else
                            {
                                numberOfMonthsHolder = idr["tv_months_extended"].ToString();
                            }
                        }
                        else
                        {
                            numberOfMonthsHolder = idr["tv_months_extended"].ToString();
                        }
                    }
                    else
                    {
                        numberOfMonthsHolder = idr["tv_months_extended"].ToString();
                    }

                    // append numberOfMonthsHolder to another field for later use
                    newrow["ID"] = idr["tv_id_extended"].ToString() + "-" + numberOfMonthsHolder;
                    dt.Rows.Add(newrow);

                }
                idr.Close();

                gvCreditHistory.DataSource = dt;
                gvCreditHistory.DataBind();

                break;
            default:
                break;
        }
    }

    protected void gvCreditPurchases_RowEditing(object sender, GridViewEditEventArgs e)
    {

    }

    protected void gvCreditPurchases_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {


            //LinkButton lb = (LinkButton)(e.Row.Cells[2].Controls[0]);
            //lb.Attributes.Add("onclick", "if (confirm('Are you sure you want to remove yourself from this organisation?') == false) return false;");
        }
    }


    protected void gvCreditHistory_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            Label lblNumberMonths = (Label)e.Row.FindControl("lblNumberMonths");
            LinkButton lbReturnToCreditMonths = (LinkButton)e.Row.FindControl("lbReturnToCreditMonths");

            // get what is in column 2
            //lblNumberMonths.Text = e.Row.Cells[2].Text;
            lblNumberMonths.Text = e.Row.Cells[2].Text.Substring(e.Row.Cells[2].Text.IndexOf("-")+1);

            if (lblNumberMonths.Text.Contains("!"))
            {
                // there is some months that can be returned for credit
                if (lblNumberMonths.Text.Substring(lblNumberMonths.Text.IndexOf("!") + 1) != "0")
                {
                    lbReturnToCreditMonths.Text = "RTC " + lblNumberMonths.Text.Substring(lblNumberMonths.Text.IndexOf("!") + 1) + " months";
                    lblNumberMonths.Text = lblNumberMonths.Text.Remove(lblNumberMonths.Text.IndexOf("!")).Trim();

                    LinkButton lb = (LinkButton)e.Row.FindControl("lbReturnToCreditMonths");
                    lb.Attributes.Add("onclick", "if (confirm('Are you sure you want to return-to-credit these months?') == false) return false;");
                }
                else
                {
                    lblNumberMonths.Text = lblNumberMonths.Text.Remove(lblNumberMonths.Text.IndexOf("!")).Trim();
                    lbReturnToCreditMonths.Visible = false;
                }
            }
            // clean up the ID row
            e.Row.Cells[2].Text = e.Row.Cells[2].Text.Remove(e.Row.Cells[2].Text.IndexOf("-"));
        }
    }

    protected void lbReturnToCreditMonths_Click(object sender, EventArgs e)
    {
        lblRTCError.Visible = false;
        LinkButton btn = (LinkButton)sender;
        GridViewRow row = (GridViewRow)btn.NamingContainer;
        int rowNumber = row.RowIndex;
        string dataKey = gvCreditHistory.DataKeys[row.RowIndex].Value.ToString();

        GridViewRow selectedRow = gvCreditHistory.Rows[rowNumber];
        TableCell type = selectedRow.Cells[1];     
        TableCell iD = selectedRow.Cells[2];

        string creditExtensionID = dataKey;
        string typeToCredit = type.Text.ToString();
        string contactOrReviewID = iD.Text.ToString();

        // RTC 2 months
        string monthsToCredit = btn.Text.ToString().Replace("RTC ", "");
        monthsToCredit = monthsToCredit.Replace("months", "").Trim();

        // makes the changes in the database
        //bool isAdmDB = true;
        //Utils.ExecuteSP(isAdmDB, Server, "st_ReturnToCredit", creditExtensionID, typeToCredit, contactOrReviewID, monthsToCredit);

        SqlParameter[] paramList = new SqlParameter[5];

        paramList[0] = new SqlParameter("@CREDIT_EXTENSION_ID", SqlDbType.Int);
        paramList[0].Direction = ParameterDirection.Input;
        paramList[0].Value = creditExtensionID;

        paramList[1] = new SqlParameter("@TYPE_TO_CREDIT", SqlDbType.NVarChar);
        paramList[1].Direction = ParameterDirection.Input;
        paramList[1].Value = typeToCredit;
        paramList[1].Size = 10;

        paramList[2] = new SqlParameter("@CONTACT_OR_REVIEW_ID", SqlDbType.Int);
        paramList[2].Direction = ParameterDirection.Input;
        paramList[2].Value = contactOrReviewID;

        paramList[3] = new SqlParameter("@MONTHS_TO_CREDIT", SqlDbType.Int);
        paramList[3].Direction = ParameterDirection.Input;
        paramList[3].Value = monthsToCredit;

        paramList[4] = new SqlParameter("@RESULT", SqlDbType.NVarChar);
        paramList[4].Direction = ParameterDirection.Output;
        paramList[4].Value = "";
        paramList[4].Size = 100;


        bool isAdmDB = true;
        Utils.ExecuteSPWithReturnValues(isAdmDB, Server, "st_ReturnToCredit", paramList);
        string res = paramList[4].Value.ToString();

        if (res == "Success")
        {
            // close the history pael and reload the credit table
            pnlHistory.Visible = false;
            buildCreditPurchaseGrid();
        }
        else
        {
            lblRTCError.Visible = true;
            lblRTCError.Text = res;
        }


    }


    protected void gvOutstandingFees_RowCommand(object sender, GridViewCommandEventArgs e)
    {

    }

    protected void gvOutstandingFees_RowEditing(object sender, GridViewEditEventArgs e)
    {

    }

    protected void gvOutstandingFees_RowDataBound(object sender, GridViewRowEventArgs e)
    {

    }

    protected void lbHideHistory_Click(object sender, EventArgs e)
    {
        pnlHistory.Visible = false;
    }

    private void countCochraneReviews()
    {
        Utils.SetSessionString("HasCochraneReviews", "No");
        bool isAdmDB = true;
        IDataReader idr = Utils.GetReader(isAdmDB, "st_ContactReviewsArchieProspective",
            Utils.GetSessionString("Contact_ID"));
        if (idr.Read())
        {
            Utils.SetSessionString("HasCochraneReviews", "Yes");
        }
        idr.Close();

        if (Utils.GetSessionString("HasCochraneReviews") == "No")
        {
            idr = Utils.GetReader(isAdmDB, "st_ContactReviewsArchieFull",
                Utils.GetSessionString("Contact_ID"));
            if (idr.Read())
            {
                Utils.SetSessionString("HasCochraneReviews", "Yes");
            }
            idr.Close();
        }

    }


    protected void lbMoveCredit_Click(object sender, EventArgs e)
    {
        pnlMoveCredit.Visible = true;

        DataTable dtS = new DataTable();
        System.Data.DataRow newrowS;
        dtS.Columns.Add(new DataColumn("PURCHASE_ID_SOURCE", typeof(string)));
        dtS.Columns.Add(new DataColumn("PURCHASE_ID_SOURCE_REMAINING", typeof(string)));

        DataTable dtD = new DataTable();
        System.Data.DataRow newrowD;
        dtD.Columns.Add(new DataColumn("PURCHASE_ID_DESTINATION", typeof(string)));
        dtD.Columns.Add(new DataColumn("PURCHASE_ID_DESTINATION_REMAINING", typeof(string)));


        newrowS = dtS.NewRow();
        newrowS["PURCHASE_ID_SOURCE"] = "Source ID";
        newrowS["PURCHASE_ID_SOURCE_REMAINING"] = "NoValue";
        dtS.Rows.Add(newrowS);

        newrowD = dtD.NewRow();
        newrowD["PURCHASE_ID_DESTINATION"] = "Destination ID";
        newrowD["PURCHASE_ID_DESTINATION_REMAINING"] = "NoValue";
        dtD.Rows.Add(newrowD);

        // we can grab the purchaseIDs from gvCreditPurchases
        for (int i = 0; i < gvCreditPurchases.Rows.Count; i++)
        {
            newrowS = dtS.NewRow();
            newrowS["PURCHASE_ID_SOURCE"] = gvCreditPurchases.Rows[i].Cells[0].Text;
            //value below is used for the element DataValueField, needs to be unique
            newrowS["PURCHASE_ID_SOURCE_REMAINING"] = gvCreditPurchases.Rows[i].Cells[3].Text + "|Id:" + gvCreditPurchases.Rows[i].Cells[0].Text;
            dtS.Rows.Add(newrowS);

            newrowD = dtD.NewRow();
            newrowD["PURCHASE_ID_DESTINATION"] = gvCreditPurchases.Rows[i].Cells[0].Text;
            //value below is used for the element DataValueField, needs to be unique
            newrowD["PURCHASE_ID_DESTINATION_REMAINING"] = gvCreditPurchases.Rows[i].Cells[3].Text + "|Id:" + gvCreditPurchases.Rows[i].Cells[0].Text;
            dtD.Rows.Add(newrowD);
        }

        ddlSourcePurchaseID.DataSource = dtS;
        ddlSourcePurchaseID.DataBind();

        ddlDestinationPurchaseID.DataSource = dtD;
        ddlDestinationPurchaseID.DataBind();

        tbAmountToTransfer.Text = "";
        ddlSourcePurchaseID.SelectedIndex = 0;
        ddlDestinationPurchaseID.SelectedIndex = 0;
    }

    protected void cmdTransferCreditPurchase_Click(object sender, EventArgs e)
    {
        lblTransferCreditPurchaseResult.Visible = false;
        if ((ddlSourcePurchaseID.SelectedIndex != 0) && (ddlDestinationPurchaseID.SelectedIndex != 0) &&
            (Utils.IsNumeric(tbAmountToTransfer.Text)))
        {
            int toTransfer;
            if (!int.TryParse(tbAmountToTransfer.Text, out toTransfer))
            {
                lblTransferCreditPurchaseResult.Text = "Transfer amount is not valid - please enter a non-decimal amount";
                lblTransferCreditPurchaseResult.Visible = true;
                return;
            }
            // if the amount > 0
            if (toTransfer > 0)
            {           
                // something is selected in each control
                if (ddlSourcePurchaseID.SelectedIndex != ddlDestinationPurchaseID.SelectedIndex)
                {
                    string[] splitted = ddlSourcePurchaseID.SelectedItem.Value.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                    string SourceRemainingAmountString = splitted[0];
                    if (toTransfer <= int.Parse(SourceRemainingAmountString))
                    {
                        // there is enough in source to handle the transfer request
                        // so create the transfer details for TB_EXPIRY_EDIT_LOG
                        // format 000000130000001400000010 (S, D, Amount)
                        string fill = "0000000";
                        // source
                        string pt1 = fill + ddlSourcePurchaseID.SelectedItem.Text;
                        pt1 = pt1.Remove(0, pt1.Length - 8);
                        // destination
                        string pt2 = fill + ddlDestinationPurchaseID.SelectedItem.Text;
                        pt2 = pt2.Remove(0, pt2.Length - 8);
                        // amount
                        string pt3 = fill + tbAmountToTransfer.Text;
                        pt3 = pt3.Remove(0, pt3.Length - 8);

                        string extensionNotes = pt1 + pt2 + pt3;

                        // we will log the transaction in TB_EXPIRY_EDIT_LOG and then TB_CREDIT_EXTENSIONS
                        bool isAdmDB = true;
                        Utils.ExecuteSP(isAdmDB, Server, "st_TransferCredit", ddlSourcePurchaseID.SelectedItem.Text,
                            ddlDestinationPurchaseID.SelectedItem.Text, extensionNotes, Utils.GetSessionString("Contact_ID"));

                        // reload gvCreditPurchases
                        buildCreditPurchaseGrid();
                        pnlMoveCredit.Visible = false;
                        pnlHistory.Visible = false;
                    }
                    else
                    {
                        lblTransferCreditPurchaseResult.Text = "Insufficient funds in Source";
                        lblTransferCreditPurchaseResult.Visible = true;
                    }
                }
                else
                {
                    lblTransferCreditPurchaseResult.Text = "Source and Destination are the same";
                    lblTransferCreditPurchaseResult.Visible = true;
                }
            }
            else
            {
                lblTransferCreditPurchaseResult.Text = "Transfer amount too low";
                lblTransferCreditPurchaseResult.Visible = true;
            }
        }
        else
        {
            lblTransferCreditPurchaseResult.Text = "Invalid selections";
            lblTransferCreditPurchaseResult.Visible = true;
        }

    }

    protected void lbCancelTransfer_Click(object sender, EventArgs e)
    {
        pnlMoveCredit.Visible = false;

    }

    protected void cmdTransferAccountPurchase_Click(object sender, EventArgs e)
    {
        lblTransferErrorMsg.Visible = false;
        bool continueProcess = false;
        string transferFromId = "";
        string transferToId = "";
        string numberOfMonths = "";

        if (tbTransferEmail.Text == tbTransferEmailConfirmation.Text)
        {

            // check that the email entered exists in the system using st_ContactDetailsEmail
            bool isAdmDB = true;
            IDataReader idr = Utils.GetReader(isAdmDB, "st_ContactDetailsEmail",
                tbTransferEmail.Text);
            if (idr.Read())
            {
                // the account exists grab the account info you need
                continueProcess = true;
                transferToId = idr["CONTACT_ID"].ToString();
            }
            else
            {
                // there is no account with that email address
                lblTransferErrorMsg.Visible = true;
                lblTransferErrorMsg.Text = "There is no account with that email address";
            }
            idr.Close();
            if (continueProcess)
            {
                // now make the transfer

                // in the original ghost account
                //   - change the MONTHS_CREDIT to its negative value minus the free month (ex: 4 to -3) 
                //   - change the CONTACT_NAME to "Transferred to ID:" + the account you are tranferring to
                //         ex: Transferred to ID:649

                // in the account you are transferring to
                //   - adjust the EXPIRY_DATE field in TB_CONTACT
                //   - add a new line to TB_EXPIRY_EDIT_LOG with the correct information
                //         - do we need a new Extension type?

                object[] paramList = new object[4];
                paramList[0] = lblSourceGhostAccountID.Text;
                paramList[1] = transferToId;
                paramList[2] = lblMonthsCredit.Text;
                paramList[3] = Utils.GetSessionString("Contact_ID");
                Utils.ExecuteSP(isAdmDB, Server, "st_TransferCreditFromGhostAccount", paramList);

                // if it all works then...
                pnlActivateIntoExistingAccount.Visible = false;
                tbTransferEmailConfirmation.Text = "";
                tbTransferEmail.Text = "";
                buildContactPurchasesGrid();
            }

        }
        else
        {
            lblTransferErrorMsg.Visible = true;
            lblTransferErrorMsg.Text = "Emails do not match";
        }

    }





}
