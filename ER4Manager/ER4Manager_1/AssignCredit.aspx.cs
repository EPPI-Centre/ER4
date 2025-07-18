using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Data;
using System.Data.SqlClient;

public partial class AssignCredit : System.Web.UI.Page
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
                    lbl.Text = "Assign credit purchase";
                }

                Telerik.Web.UI.RadTabStrip radTs = (Telerik.Web.UI.RadTabStrip)Master.FindControl("rtsMenu");
                if (radTs != null)
                {
                    radTs.SelectedIndex = 0;
                    radTs.Tabs[0].Tabs[1].Selected = true;
                    radTs.Tabs[0].Tabs[1].Visible = true;
                    radTs.Tabs[0].Tabs[4].Width = 400;
                }
                System.Web.UI.WebControls.Label lbl1 = (Label)Master.FindControl("lblHeadingText");
                if (lbl1 != null)
                {
                    lbl1.Text = "Assign credit purchase";
                }

                lblRemainingCredit.Text = "£" + Utils.GetSessionString("Remaining_Credit");
                lblPurchasedCredit.Text = "£" + Utils.GetSessionString("Purchased_Credit");
                buildReviewGrid();
            }
            //Utils.SetSessionString("Credit_Purchase_ID", "");
            cmdComplete.Attributes.Add("onclick", "if (confirm('Are you sure you wish to make these extensions. You may be unable to undo them without contacting EPPI-Support.') == false) return false;");
        }
        else
        {
            Server.Transfer("Error.aspx");
        }
    }

    private void buildReviewGrid()
    {
        DateTime dayExpires;
        DateTime today = DateTime.Today;
        DataTable dt = new DataTable();
        System.Data.DataRow newrow;

        dt.Columns.Add(new DataColumn("REVIEW_ID", typeof(string)));
        dt.Columns.Add(new DataColumn("REVIEW_NAME", typeof(string)));
        dt.Columns.Add(new DataColumn("EXPIRY_DATE", typeof(string)));
        dt.Columns.Add(new DataColumn("SITE_LIC_ID", typeof(string)));
        dt.Columns.Add(new DataColumn("SITE_LIC_NAME", typeof(string)));
        bool isAdmDB = true;
        IDataReader idr = Utils.GetReader(isAdmDB, "st_ContactReviewsAdmin",
            Utils.GetSessionString("Contact_ID"));
        while (idr.Read())
        {
            newrow = dt.NewRow();
            newrow["REVIEW_ID"] = idr["REVIEW_ID"].ToString();

            if ((idr["REVIEW_NAME"].ToString() == null) || (idr["REVIEW_NAME"].ToString() == ""))
            {
                if ((idr["EXPIRY_DATE"].ToString() == null) || (idr["EXPIRY_DATE"].ToString() == ""))
                {
                    newrow["REVIEW_NAME"] = "Not activated";
                }
                else
                {
                    newrow["REVIEW_NAME"] = "Edit name";
                }
            }
            else
                newrow["REVIEW_NAME"] = idr["REVIEW_NAME"].ToString();

            if ((idr["EXPIRY_DATE"].ToString() == null) || (idr["EXPIRY_DATE"].ToString() == ""))
            {
                if (idr["MONTHS_CREDIT"].ToString() == "0")
                {
                    newrow["EXPIRY_DATE"] = "Non-shareable review";
                }
                else
                {
                    newrow["EXPIRY_DATE"] = "Not activated: " + idr["MONTHS_CREDIT"].ToString() + " months credit";
                }
            }
            else
            {
                dayExpires = Convert.ToDateTime(idr["EXPIRY_DATE"].ToString());
                if (dayExpires < today)
                {
                    newrow["EXPIRY_DATE"] = dayExpires.ToString("dd MMM yyyy") + "  Expired";
                }
                else
                {
                    newrow["EXPIRY_DATE"] = dayExpires.ToString("dd MMM yyyy");
                    //newrow["EXPIRY_DATE"] = dayExpires.ToShortDateString();
                }

                if (idr["SITE_LIC_ID"].ToString() != null && idr["SITE_LIC_ID"].ToString() != "")
                {
                    newrow["EXPIRY_DATE"] += " In Site License '" + idr["SITE_LIC_NAME"].ToString() + "' (ID:" + idr["SITE_LIC_ID"].ToString() + ")";
                }
            }

            dt.Rows.Add(newrow);
        }
        idr.Close();

        gvReview.DataSource = dt;
        gvReview.DataBind();


        for (int i = 0; i < gvReview.Rows.Count; i++)
        {
            if (gvReview.Rows[i].Cells[1].Text.Contains("Edit name"))
            {
                gvReview.Rows[i].Cells[1].BackColor = System.Drawing.Color.PaleGreen;
            }
            if (gvReview.Rows[i].Cells[1].Text.Contains("Not activated"))
            {
                gvReview.Rows[i].Cells[1].BackColor = System.Drawing.Color.Yellow;
            }
            if (gvReview.Rows[i].Cells[2].Text.Contains("Not activated"))
            {
                gvReview.Rows[i].Cells[2].BackColor = System.Drawing.Color.Yellow;
            }
            if (gvReview.Rows[i].Cells[2].Text.Contains("Expired"))
            {
                gvReview.Rows[i].Cells[2].BackColor = System.Drawing.Color.Pink;
                gvReview.Rows[i].Cells[2].Font.Bold = true;
            }
            if (gvReview.Rows[i].Cells[2].Text.Contains(" In Site License '"))
            {
                gvReview.Rows[i].Cells[2].BackColor = System.Drawing.Color.Aquamarine;
            }
        }

    }



    protected void gvReview_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        int index = Convert.ToInt32(e.CommandArgument);
        string ReviewID = (string)gvReview.DataKeys[index].Value;
        switch (e.CommandName)
        {
            case "EDT":
                pnlInstructions.Visible = false;
                pnlBottom.Visible = false;
                lblAvailable.Text = "£" + Utils.GetSessionString("Remaining_Credit");
                lblRemaining.Text = "£" + Utils.GetSessionString("Remaining_Credit");

                DateTime lastAccessed;
                DateTime dayExpires;
                DateTime today = DateTime.Today;

                DataTable dt = new DataTable();
                System.Data.DataRow newrow;

                dt.Columns.Add(new DataColumn("CONTACT_ID", typeof(string)));
                dt.Columns.Add(new DataColumn("CONTACT_NAME", typeof(string)));
                dt.Columns.Add(new DataColumn("EMAIL", typeof(string)));
                dt.Columns.Add(new DataColumn("LAST_LOGIN", typeof(string)));
                dt.Columns.Add(new DataColumn("EXPIRY_DATE", typeof(string)));
                dt.Columns.Add(new DataColumn("COST", typeof(string)));

                string expiryDate = "";
                bool isAdmDB = true;
                IDataReader idr1 = Utils.GetReader(isAdmDB, "st_ReviewMembers", ReviewID);
                while (idr1.Read())
                {
                    newrow = dt.NewRow();

                    newrow["CONTACT_ID"] = idr1["CONTACT_ID"].ToString();
                    newrow["CONTACT_NAME"] = idr1["CONTACT_NAME"].ToString();

                    expiryDate = idr1["expiry_date"].ToString();
                    //expiryDate = expiryDate.Remove(expiryDate.IndexOf(" "));
                    
                    if (expiryDate != "")
                    {
                        expiryDate = expiryDate.Remove(expiryDate.IndexOf(" "));
                        dayExpires = Convert.ToDateTime(idr1["EXPIRY_DATE"].ToString());
                        if (dayExpires < today)
                        {
                            newrow["EXPIRY_DATE"] = dayExpires.ToString("dd MMM yyyy") + "  Expired";
                            //newrow["EXPIRY_DATE"] = dayExpires.ToShortDateString() + "  Expired";
                        }
                        else
                        {
                            newrow["EXPIRY_DATE"] = dayExpires.ToString("dd MMM yyyy");
                            //newrow["EXPIRY_DATE"] = expiryDate;
                        }

                        if (idr1["site_lic_id"].ToString() != "")
                        {
                            newrow["EXPIRY_DATE"] = dayExpires.ToString("dd MMM yyyy") + " in Site License #" +
                                idr1["site_lic_id"].ToString();
                            //newrow["EXPIRY_DATE"] = expiryDate + " in Site License #" +
                            //    idr1["site_lic_id"].ToString();
                        }
                    }
                    else
                    {
                        // it's an unactivated account
                        newrow["CONTACT_NAME"] = "Unactivated";

                    }




                    newrow["EMAIL"] = idr1["EMAIL"].ToString();

                    if ((idr1["LAST_LOGIN"].ToString() == null) || (idr1["LAST_LOGIN"].ToString() == ""))
                    {
                        newrow["LAST_LOGIN"] = "Never";
                    }
                    else
                    {
                        lastAccessed = Convert.ToDateTime(idr1["LAST_LOGIN"].ToString());
                        newrow["LAST_LOGIN"] = lastAccessed.ToString("dd MMM yyyy HH:ss");
                        //newrow["LAST_LOGIN"] = idr1["LAST_LOGIN"].ToString();
                    }

                    newrow["COST"] = "0";

                    dt.Rows.Add(newrow);
                }
                idr1.Close();

                gvMembersOfReview.DataSource = dt;
                gvMembersOfReview.DataBind();


                for (int i = 0; i < gvMembersOfReview.Rows.Count; i++)
                {
                    if (gvMembersOfReview.Rows[i].Cells[2].Text.Contains("Edit name"))
                    {
                        gvMembersOfReview.Rows[i].Cells[2].BackColor = System.Drawing.Color.PaleGreen;
                    }
                    if (gvMembersOfReview.Rows[i].Cells[2].Text.Contains("Not activated"))
                    {
                        gvMembersOfReview.Rows[i].Cells[2].BackColor = System.Drawing.Color.Yellow;
                    }
                    if (gvMembersOfReview.Rows[i].Cells[2].Text.Contains("Not activated"))
                    {
                        gvMembersOfReview.Rows[i].Cells[2].BackColor = System.Drawing.Color.Yellow;
                    }
                    if (gvMembersOfReview.Rows[i].Cells[2].Text.Contains("Expired"))
                    {
                        gvMembersOfReview.Rows[i].Cells[2].BackColor = System.Drawing.Color.Pink;
                        gvMembersOfReview.Rows[i].Cells[2].Font.Bold = true;
                    }
                    if (gvMembersOfReview.Rows[i].Cells[2].Text.Contains("Site License"))
                    {
                        gvMembersOfReview.Rows[i].Cells[2].BackColor = System.Drawing.Color.Aquamarine;
                        DropDownList ddl = (DropDownList)gvMembersOfReview.Rows[i].Cells[2].FindControl("ddlExtendAccount");
                        if (ddl != null)
                        {
                            ddl.Enabled = false;
                        }
                    }
                }
                pnlReviewDetails.Visible = true;
                pnlAssign.Visible = true;
                pnlReviewMembers.Visible = true;
                lblExpiryDate.Text = gvReview.Rows[index].Cells[2].Text;
                ddlExtendReview.Enabled = true;
                if (lblExpiryDate.Text.Contains("Site License"))
                {
                    ddlExtendReview.Enabled = false;
                }
                lblReviewName.Text = gvReview.Rows[index].Cells[1].Text;

                lblReviewCost.Text = "£0";
                ddlExtendReview.SelectedIndex = 0;
                lblTotal.Text = "£0";
                lblRemaining.Text = lblAvailable.Text;
                lblReviewID.Text = ReviewID;
                cmdComplete.Enabled = false;
                pnlReviewManual.Visible = false;
                pnlAccountManual.Visible = false;

                break;
            

            default:
                break;
        }
    }

    protected void gvMembersOfReview_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            DropDownList ddlExtendAccount = (DropDownList)e.Row.FindControl("ddlExtendAccount");
            string nameColumn = e.Row.Cells[1].Text;
            if (nameColumn == "Unactivated")
            {
                ddlExtendAccount.Enabled = false;
            }
        }

    }

    protected void ddlExtendAccount_SelectedIndexChanged(object sender, EventArgs e)
    {
        GridViewRow gvr = (GridViewRow)(((Control)sender).NamingContainer);
        string contactID = gvr.Cells[0].Text;
        string accountCreatorID = gvr.Cells[2].Text;

        DropDownList dropdownlist1 = (DropDownList)gvr.FindControl("ddlExtendAccount");

        //if (dropdownlist1.SelectedIndex != 0)
        //{
            string numberMonths = dropdownlist1.SelectedValue;
            gvMembersOfReview.Rows[gvr.RowIndex].Cells[5].Text = (int.Parse(numberMonths) * 10).ToString();
            AddUpFees();
        //}
    }



    protected void cmdComplete_Click(object sender, EventArgs e)
    {
        bool reviewExtended = false;
        lblExtensionError.Visible = false;
        if (lblTotal.Text == "£0")
        {
            lblExtensionError.Visible = true;
        }
        else
        {
            bool isAdmDB = true;

            // extend the review (if needed)
            if (ddlExtendReview.SelectedIndex != 0)
            {
                Utils.ExecuteSP(isAdmDB, Server, "st_ApplyCreditToReview",
                    Utils.GetSessionString("Credit_Purchase_ID"), lblReviewID.Text,
                    ddlExtendReview.SelectedValue, Utils.GetSessionString("Contact_ID"));
                reviewExtended = true;
            }


            // extend the accounts (if needed)
            DropDownList monthsExtended;
            for (int i = 0; i < gvMembersOfReview.Rows.Count; i++)
            {
                monthsExtended = (DropDownList)gvMembersOfReview.Rows[i].FindControl("ddlExtendAccount");
                if (monthsExtended.SelectedIndex != 0)
                {
                    Utils.ExecuteSP(isAdmDB, Server, "st_ApplyCreditToAccount",
                        Utils.GetSessionString("Credit_Purchase_ID"), gvMembersOfReview.Rows[i].Cells[0].Text,
                        monthsExtended.SelectedValue, Utils.GetSessionString("Contact_ID"));
                }
            }

            Utils.SetSessionString("Remaining_Credit", lblRemaining.Text.Replace("£", ""));
            lblRemainingCredit.Text = lblRemaining.Text;
            pnlInstructions.Visible = true;
            pnlAssign.Visible = false;
            pnlReviewDetails.Visible = false;
            pnlReviewMembers.Visible = false;

            // reload the review grid so the updated review expiry date is shown
            if (reviewExtended == true)
                buildReviewGrid();

        }
    }

    protected void lbCancel_Click(object sender, EventArgs e)
    {
        pnlReviewDetails.Visible = false;
        pnlAssign.Visible = false;
        pnlReviewMembers.Visible = false;
        pnlInstructions.Visible = true;
        pnlBottom.Visible = false;
    }

    protected void ddlExtendReview_SelectedIndexChanged(object sender, EventArgs e)
    {
        //if (ddlExtendReview.SelectedIndex != 0)
        //{
            lblReviewCost.Text = "£" + (int.Parse(ddlExtendReview.SelectedValue) * 35).ToString();
            AddUpFees();
        //}
    }

    private void AddUpFees()
    {     
        string fee = "";
        int totalFee = 0;
        //cmdComplete.Enabled = true;

        fee = lblReviewCost.Text.Replace("£", "");
        totalFee = int.Parse(fee);

        for (int i = 0; i < gvMembersOfReview.Rows.Count; i++)
        {
            fee = gvMembersOfReview.Rows[i].Cells[5].Text;
            totalFee += int.Parse(fee);
        }

        lblTotal.Text = "£" + totalFee.ToString();
        lblRemaining.Text = "£" + (int.Parse(Utils.GetSessionString("Remaining_Credit")) - totalFee).ToString();
        //if ((lblRemaining.Text.Contains("-")) || (lblRemaining.Text == "£0"))
        if (lblRemaining.Text.Contains("-"))
            cmdComplete.Enabled = false;
        else
            cmdComplete.Enabled = true;

        if (lblTotal.Text == "£0")
            cmdComplete.Enabled = false;
    }

    protected void cmdAddExistingAccount_Click(object sender, EventArgs e)
    {
        lblAccountMsg.Visible = false;
        if (tbAddAccountID.Text.Contains(","))
            tbAddAccountID.Text = tbAddAccountID.Text.Replace(",", "");

        if ((tbAddAccountID.Text == "") || (tbAddAccountEmail.Text == ""))
        {
            lblAccountMsg.Visible = true;
            lblAccountMsg.Text = "Missing details";
        }
        else if ((tbAddAccountID.Text.Contains(".")) || (tbAddAccountID.Text.Contains("-")))
        {
            lblAccountMsg.Visible = true;
            lblAccountMsg.Text = "Invalid account ID";
        }
        else
        {
            if (Utils.IsNumeric(tbAddAccountID.Text))
            {
                string name = "";
                string email = "";
                string expiryDate = "";
                string siteLicID = "";
                string siteLicName = "";
                bool idFound = false;

                bool isAdmDB = true;
                IDataReader idr1 = Utils.GetReader(isAdmDB, "st_ContactDetails", tbAddAccountID.Text);
                if (idr1.Read())
                {
                    idFound = true;
                    siteLicID = idr1["SITE_LIC_ID"].ToString();
                    siteLicName = idr1["SITE_LIC_NAME"].ToString();
                    name = idr1["CONTACT_NAME"].ToString();
                    email = idr1["EMAIL"].ToString();
                    expiryDate = idr1["EXPIRY_DATE"].ToString();
                }
                idr1.Close();

                if (idFound == true)
                {
                    if (email == tbAddAccountEmail.Text)
                    {
                        // there is a match!
                        pnlReviewMembers.Visible = true;
                        pnlAssign.Visible = true;
                        pnlInstructions.Visible = false;
                        pnlAccountManual.Visible = false;
                        lblAvailable.Text = "£" + Utils.GetSessionString("Remaining_Credit");
                        lblRemaining.Text = "£" + Utils.GetSessionString("Remaining_Credit");
                        lblTotal.Text = "£0";
                        lblReviewCost.Text = "£0";
                        cmdComplete.Enabled = false;

                        DateTime dayExpires;
                        DateTime today = DateTime.Today;

                        DataTable dt = new DataTable();
                        System.Data.DataRow newrow;

                        dt.Columns.Add(new DataColumn("CONTACT_ID", typeof(string)));
                        dt.Columns.Add(new DataColumn("CONTACT_NAME", typeof(string)));
                        dt.Columns.Add(new DataColumn("EMAIL", typeof(string)));
                        dt.Columns.Add(new DataColumn("EXPIRY_DATE", typeof(string)));
                        dt.Columns.Add(new DataColumn("COST", typeof(string)));

                        newrow = dt.NewRow();
                        expiryDate = expiryDate.Remove(expiryDate.IndexOf(" "));

                        newrow["CONTACT_ID"] = tbAddAccountID.Text;
                        newrow["CONTACT_NAME"] = name;
                        dayExpires = Convert.ToDateTime(expiryDate);

                        if (dayExpires < today)
                        {
                            newrow["EXPIRY_DATE"] = dayExpires.ToString("dd MMM yyyy") + "  Expired";
                            //newrow["EXPIRY_DATE"] = dayExpires.ToShortDateString() + "  Expired";
                        }
                        else
                        {
                            newrow["EXPIRY_DATE"] = dayExpires.ToString("dd MMM yyyy");
                            //newrow["EXPIRY_DATE"] = expiryDate;
                        }

                        if (siteLicID != "")
                        {
                            newrow["EXPIRY_DATE"] = dayExpires.ToString("dd MMM yyyy") + " in Site License #" + siteLicID;
                            //newrow["EXPIRY_DATE"] = expiryDate + " in Site License #" + siteLicID;
                        }
                        newrow["EMAIL"] = email;

                        newrow["COST"] = "0";
                        dt.Rows.Add(newrow);

                        gvMembersOfReview.DataSource = dt;
                        gvMembersOfReview.DataBind();

                        for (int i = 0; i < gvMembersOfReview.Rows.Count; i++)
                        {
                            if (gvMembersOfReview.Rows[i].Cells[2].Text.Contains("Edit name"))
                            {
                                gvMembersOfReview.Rows[i].Cells[2].BackColor = System.Drawing.Color.PaleGreen;
                            }
                            if (gvMembersOfReview.Rows[i].Cells[2].Text.Contains("Not activated"))
                            {
                                gvMembersOfReview.Rows[i].Cells[2].BackColor = System.Drawing.Color.Yellow;
                            }
                            if (gvMembersOfReview.Rows[i].Cells[2].Text.Contains("Not activated"))
                            {
                                gvMembersOfReview.Rows[i].Cells[2].BackColor = System.Drawing.Color.Yellow;
                            }
                            if (gvMembersOfReview.Rows[i].Cells[2].Text.Contains("Expired"))
                            {
                                gvMembersOfReview.Rows[i].Cells[2].BackColor = System.Drawing.Color.Pink;
                                gvMembersOfReview.Rows[i].Cells[2].Font.Bold = true;
                            }
                            if (gvMembersOfReview.Rows[i].Cells[2].Text.Contains("Site License"))
                            {
                                gvMembersOfReview.Rows[i].Cells[2].BackColor = System.Drawing.Color.Aquamarine;
                                DropDownList ddl = (DropDownList)gvMembersOfReview.Rows[i].Cells[2].FindControl("ddlExtendAccount");
                                if (ddl != null)
                                {
                                    ddl.Enabled = false;
                                }
                            }
                        }
                    }
                    else
                    {
                        lblAccountMsg.Visible = true;
                        lblAccountMsg.Text = "No match found";
                    }
                }
                else
                {
                    lblAccountMsg.Visible = true;
                    lblAccountMsg.Text = "No account found";
                }
            }
            else
            {
                lblAccountMsg.Visible = true;
                lblAccountMsg.Text = "Not a valid ID";
            }
        }
    }

    protected void lbReviewManual_Click(object sender, EventArgs e)
    {
        pnlAccountManual.Visible = false;
        if (pnlReviewManual.Visible == true)
        {
            pnlReviewManual.Visible = false;
        }
        else
        {
            pnlReviewManual.Visible = true;
            tbReviewID.Text = "";
            lblReviewMsg.Visible = false;
        }
    }

    protected void lbAccountManual_Click(object sender, EventArgs e)
    {
        pnlReviewManual.Visible = false;
        if (pnlAccountManual.Visible == true)
        {
            pnlAccountManual.Visible = false;
        }
        else
        {
            pnlAccountManual.Visible = true;
            tbAddAccountEmail.Text = "";
            tbAddAccountID.Text = "";
            lblAccountMsg.Visible = false;
        }
    }

    protected void cmdAddExistingReview_Click(object sender, EventArgs e)
    {
        lblReviewMsg.Visible = false;
        ddlExtendReview.Enabled = true;
        if (tbReviewID.Text.Contains(","))
            tbReviewID.Text = tbReviewID.Text.Replace(",", "");

        bool reviewFound = false;
        if (tbReviewID.Text == "")
        {
            lblReviewMsg.Visible = true;
            lblReviewMsg.Text = "Missing review ID";
        }
        else if ((tbReviewID.Text.Contains(".")) || (tbReviewID.Text.Contains("-")))
        {
            lblReviewMsg.Visible = true;
            lblReviewMsg.Text = "Invalid review ID";
        }
        else
        {
            DateTime dayExpires;
            DateTime today = DateTime.Today;

            lblReviewMsg.Visible = false;
            if (Utils.IsNumeric(tbReviewID.Text))
            {
                bool isAdmDB = true;
                IDataReader idr = Utils.GetReader(isAdmDB, "st_ReviewDetails", tbReviewID.Text);
                if (idr.Read())
                {
                    reviewFound = true;
                    lblReviewID.Text = tbReviewID.Text;
                    lblReviewName.Text = idr["REVIEW_NAME"].ToString();

                    if ((idr["EXPIRY_DATE"].ToString() == null) || (idr["EXPIRY_DATE"].ToString() == ""))
                    {
                        if (idr["MONTHS_CREDIT"].ToString() == "0")
                        {
                            lblExpiryDate.Text = "Non-shareable review";
                        }
                        else
                        {
                            lblExpiryDate.Text = "Not activated: " + idr["MONTHS_CREDIT"].ToString() + " months credit";
                        }
                    }
                    else
                    {
                        dayExpires = Convert.ToDateTime(idr["EXPIRY_DATE"].ToString());
                        if (dayExpires < today)
                        {
                            lblExpiryDate.Text = dayExpires.ToString("dd MMM yyyy") + "  Expired";
                            //lblExpiryDate.Text = dayExpires.ToShortDateString() + "  Expired";
                        }
                        else
                        {
                            lblExpiryDate.Text = dayExpires.ToString("dd MMM yyyy");
                            //lblExpiryDate.Text = dayExpires.ToShortDateString();
                        }
                    }

                    if (idr["SITE_LIC_ID"].ToString() != null && idr["SITE_LIC_ID"].ToString() != "")
                    {
                        lblExpiryDate.Text += " In Site License '" + idr["SITE_LIC_NAME"].ToString() + "' (ID:" + idr["SITE_LIC_ID"].ToString() + ")";
                        ddlExtendReview.Enabled = false;
                    }
                }
                else
                {
                    lblReviewMsg.Visible = true;
                    lblReviewMsg.Text = "No review found";
                }
                idr.Close();

                if(reviewFound == true)
                {
                    pnlReviewManual.Visible = false;
                    pnlInstructions.Visible = false;
                    pnlAssign.Visible = true;
                    pnlReviewDetails.Visible = true;
                    pnlBottom.Visible = true;
                    lblAvailable.Text = "£" + Utils.GetSessionString("Remaining_Credit");
                    lblRemaining.Text = "£" + Utils.GetSessionString("Remaining_Credit");
                    ddlExtendReview.SelectedIndex = 0;
                    lblTotal.Text = "£0";
                    lblReviewCost.Text = "£0";
                    cmdComplete.Enabled = false;
                }

            }
            else
            {
                lblReviewMsg.Visible = true;
                lblReviewMsg.Text = "Invalid review ID";
            }
        }
    }
}
 