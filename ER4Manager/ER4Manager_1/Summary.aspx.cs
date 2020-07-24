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
                    lbl.Text = "Summary of this user and their reviews";
                }

                Telerik.Web.UI.RadTabStrip radTs = (Telerik.Web.UI.RadTabStrip)Master.FindControl("rtsMenu");
                if (radTs != null)
                {
                    radTs.SelectedIndex = 0;
                    radTs.Tabs[0].Tabs[0].Selected = true;
                    radTs.Tabs[0].Tabs[1].Visible = false;
                    radTs.Tabs[0].Tabs[2].Width = 700;
                }
                System.Web.UI.WebControls.Label lbl1 = (Label)Master.FindControl("lblHeadingText");
                if (lbl1 != null)
                {
                    lbl1.Text = "Summary of this user and their reviews";
                }

                buildContactGrid();
                buildOrganisationGrid();
                buildCreditPurchaseGrid();
                if (Utils.GetSessionString("EnableShopDebit") == "True")
                {
                    buildOutstandingFeeGrid();
                }
                buildContactPurchasesGrid();
                buildShareableReviewGrid();
                buildNonShareableReviewGrid();
                buildOtherShareableReviewGrid();
                buildReviewGridCochraneProspective();
                buildReviewGridCochraneFull();
            }
            Utils.SetSessionString("Credit_Purchase_ID", "");
            rblPSShareableEnable.Attributes.Add("onclick", "if (confirm('If you are turning on Priority Screening please check the user manual for more details on how to use it.') == false) return false;");
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
                newrow["EXPIRY_DATE"] += " In site License (ID:" + idr["SITE_LIC_ID"].ToString()+")";
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
            newrow2["DATE_PURCHASED"] = dayCreated.ToString("dd MMM yyyy");

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
        dt.Columns.Add(new DataColumn("IS_FULLY_ACTIVE", typeof(bool)));
        dt.Columns.Add(new DataColumn("IS_STALE_AGHOST", typeof(bool)));

        bool isAdmDB = true;
        IDataReader idr = Utils.GetReader(isAdmDB, "st_ContactPurchasedDetails",
            Utils.GetSessionString("Contact_ID"));
        while (idr.Read())
        {
            newrow = dt.NewRow();
            newrow["CONTACT_ID"] = idr["CONTACT_ID"].ToString();
            newrow["IS_FULLY_ACTIVE"] = idr["IS_FULLY_ACTIVE"] ;
            newrow["IS_STALE_AGHOST"] = idr["IS_STALE_AGHOST"] ;
            if ((idr["CONTACT_NAME"].ToString() == null) || (idr["CONTACT_NAME"].ToString() == ""))
                newrow["CONTACT_NAME"] = "Not set up";
            else
                newrow["CONTACT_NAME"] = idr["CONTACT_NAME"].ToString();

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
                if  (idr["SITE_LIC_ID"].ToString() != null && idr["SITE_LIC_ID"].ToString() != "")
                {
                    newrow["EXPIRY_DATE"] += " In Site License '" + idr["SITE_LIC_NAME"].ToString() + "' (ID:" + idr["SITE_LIC_ID"].ToString() + ")";
                }
                
            }
            else if ((idr["EXPIRY_DATE"].ToString() == "") && (int.Parse(idr["MONTHS_CREDIT"].ToString()) > 0)
                && idr["EMAIL"].ToString() == "")
            {
                newrow["EXPIRY_DATE"] = "Not activated: " + idr["MONTHS_CREDIT"].ToString() + " months credit";
            }
            else if (newrow["IS_FULLY_ACTIVE"].ToString() == "False" && newrow["IS_STALE_AGHOST"].ToString() == "False")
            {
                newrow["EXPIRY_DATE"] = "Invitation sent: " + idr["MONTHS_CREDIT"].ToString() + " months credit";
            }
            else if (newrow["IS_FULLY_ACTIVE"].ToString() == "False" &&  newrow["IS_STALE_AGHOST"].ToString() == "True")
            {
                newrow["EXPIRY_DATE"] = "Invitation sent (expired): " + idr["MONTHS_CREDIT"].ToString() + " months credit";
            } 
            dt.Rows.Add(newrow);
        }
        idr.Close();

        gvAccountPurchases.DataSource = dt;
        gvAccountPurchases.DataBind();

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
        int index = Convert.ToInt32(e.CommandArgument);
        string ContactID = (string)gvAccountPurchases.DataKeys[index].Value;
        switch (e.CommandName)
        {
            case "EDIT":
                contactData(ContactID, true);
                pnlActivateGhostForm.Visible = true;
                pnlContactDetails.Visible = false;
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



    private void buildShareableReviewGrid()
    {
        DateTime dayExpires;
        DateTime dayCreated;
        DateTime lastLogin;
        DateTime today = DateTime.Today;
        DataTable dt = new DataTable();
        System.Data.DataRow newrow;
        string test = "aa";

        dt.Columns.Add(new DataColumn("REVIEW_ID", typeof(string)));
        dt.Columns.Add(new DataColumn("REVIEW_NAME", typeof(string)));
        dt.Columns.Add(new DataColumn("DATE_CREATED", typeof(string)));
        dt.Columns.Add(new DataColumn("LAST_LOGIN", typeof(string)));
        //dt.Columns.Add(new DataColumn("MONTHS_CREDIT", typeof(string)));
        dt.Columns.Add(new DataColumn("EXPIRY_DATE", typeof(string)));
        dt.Columns.Add(new DataColumn("SITE_LIC_ID", typeof(string)));
        dt.Columns.Add(new DataColumn("SITE_LIC_NAME", typeof(string)));
        bool isAdmDB = true;
        IDataReader idr = Utils.GetReader(isAdmDB, "st_ContactReviewsShareable",
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

            if ((idr["DATE_CREATED"].ToString() == null) || (idr["DATE_CREATED"].ToString() == ""))
            {
                newrow["DATE_CREATED"] = "N/A";
            }
            else
            {
                dayCreated = Convert.ToDateTime(idr["DATE_CREATED"].ToString());
                newrow["DATE_CREATED"] = dayCreated.ToString("dd MMM yyyy");
                //newrow["DATE_CREATED"] = idr["DATE_CREATED"].ToString();
            }

            test = idr["LAST_LOGIN"].ToString();
            if ((idr["LAST_LOGIN"].ToString() == null) || (idr["LAST_LOGIN"].ToString() == ""))
            {
                newrow["LAST_LOGIN"] = "N/A";
            }
            else
            {
                lastLogin = Convert.ToDateTime(idr["LAST_LOGIN"].ToString());
                newrow["LAST_LOGIN"] = lastLogin.ToString("dd MMM yyyy HH:ss");
                //newrow["LAST_LOGIN"] = idr["LAST_LOGIN"].ToString();
            }

            //newrow["MONTHS_CREDIT"] = idr["MONTHS_CREDIT"].ToString();

            if ((idr["EXPIRY_DATE"].ToString() == null) || (idr["EXPIRY_DATE"].ToString() == ""))
            {
                newrow["EXPIRY_DATE"] = "Not activated: " + idr["MONTHS_CREDIT"].ToString() + " months credit";
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

        if (dt.Rows.Count == 0)
            lblShareableReviews.Visible = true;
        else
        {
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
                if (gvReview.Rows[i].Cells[4].Text.Contains("Not activated"))
                {
                    gvReview.Rows[i].Cells[4].BackColor = System.Drawing.Color.Yellow;
                }
                if (gvReview.Rows[i].Cells[4].Text.Contains("Expired"))
                {
                    gvReview.Rows[i].Cells[4].BackColor = System.Drawing.Color.Pink;
                    gvReview.Rows[i].Cells[4].Font.Bold = true;
                }
                if (gvReview.Rows[i].Cells[4].Text.Contains(" In Site License '"))
                {
                    gvReview.Rows[i].Cells[4].BackColor = System.Drawing.Color.Aquamarine;
                }
            }
        }
    }
    private void buildNonShareableReviewGrid()
    {
        DateTime dateCreated;
        DateTime lastAccessed;

        DataTable dt = new DataTable();
        System.Data.DataRow newrow;

        dt.Columns.Add(new DataColumn("REVIEW_ID", typeof(string)));
        dt.Columns.Add(new DataColumn("REVIEW_NAME", typeof(string)));
        dt.Columns.Add(new DataColumn("DATE_CREATED", typeof(string)));
        dt.Columns.Add(new DataColumn("LAST_LOGIN", typeof(string)));
        bool isAdmDB = true;
        IDataReader idr = Utils.GetReader(isAdmDB, "st_ContactReviewsNonShareable",
            Utils.GetSessionString("Contact_ID"));
        while (idr.Read())
        {
            newrow = dt.NewRow();
            newrow["REVIEW_ID"] = idr["REVIEW_ID"].ToString();
            newrow["REVIEW_NAME"] = idr["REVIEW_NAME"].ToString();

            if ((idr["DATE_CREATED"].ToString() == null) || (idr["DATE_CREATED"].ToString() == ""))
            {
                newrow["DATE_CREATED"] = "N/A";
            }
            else
            {
                //newrow["DATE_CREATED"] = idr["DATE_CREATED"].ToString();
                dateCreated = Convert.ToDateTime(idr["DATE_CREATED"].ToString());
                newrow["DATE_CREATED"] = dateCreated.ToString("dd MMM yyyy");
            }

            if ((idr["CREATED"].ToString() == null) || (idr["CREATED"].ToString() == ""))
            {
                newrow["LAST_LOGIN"] = "N/A";
            }
            else
            {
                //newrow["LAST_LOGIN"] = idr["CREATED"].ToString();
                lastAccessed = Convert.ToDateTime(idr["CREATED"].ToString());
                newrow["LAST_LOGIN"] = lastAccessed.ToString("dd MMM yyyy HH:mm");
            }
            dt.Rows.Add(newrow);
        }
        idr.Close();

        gvReviewNonShareable.DataSource = dt;
        gvReviewNonShareable.DataBind();

        if (dt.Rows.Count == 0)
            lblNonShareableReviews.Visible = true;
    }

    private void buildOtherShareableReviewGrid()
    {
        DateTime dateCreated;
        DateTime lastAccessed;
        DataTable dt = new DataTable();
        System.Data.DataRow newrow;

        dt.Columns.Add(new DataColumn("REVIEW_ID", typeof(string)));
        dt.Columns.Add(new DataColumn("REVIEW_NAME", typeof(string)));
        dt.Columns.Add(new DataColumn("REVIEW_OWNER", typeof(string)));
        dt.Columns.Add(new DataColumn("DATE_CREATED", typeof(string)));
        dt.Columns.Add(new DataColumn("LAST_LOGIN", typeof(string)));
        bool isAdmDB = true;
        IDataReader idr = Utils.GetReader(isAdmDB, "st_ContactReviewsOtherShareable",
            Utils.GetSessionString("Contact_ID"));
        while (idr.Read())
        {
            newrow = dt.NewRow();
            newrow["REVIEW_ID"] = idr["REVIEW_ID"].ToString();
            newrow["REVIEW_NAME"] = idr["REVIEW_NAME"].ToString();
            newrow["REVIEW_OWNER"] = idr["REVIEW_OWNER"].ToString();
            if ((idr["DATE_REVIEW_CREATED"].ToString() == null) || (idr["DATE_REVIEW_CREATED"].ToString() == ""))
            {
                newrow["DATE_CREATED"] = "Never";
            }
            else
            {
                dateCreated = Convert.ToDateTime(idr["DATE_REVIEW_CREATED"].ToString());
                newrow["DATE_CREATED"] = dateCreated.ToString("dd MMM yyyy");
            }

            if ((idr["LAST_ACCESSED_BY_CONTACT"].ToString() == null) || (idr["LAST_ACCESSED_BY_CONTACT"].ToString() == ""))
            {
                newrow["LAST_LOGIN"] = "Never";
            }
            else
            {
                lastAccessed = Convert.ToDateTime(idr["LAST_ACCESSED_BY_CONTACT"].ToString());
                newrow["LAST_LOGIN"] = lastAccessed.ToString("dd MMM yyyy HH:mm");
            }
            dt.Rows.Add(newrow);
        }
        idr.Close();

        gvReviewShareableMember.DataSource = dt;
        gvReviewShareableMember.DataBind();

        if (dt.Rows.Count == 0)
            lblNonShareableReviewsMember.Visible = true;
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



    protected void cmdSave_Click(object sender, EventArgs e)
    {
        lblPasswordMsg.Text = "To keep your existing password leave the password fields blank.";
        lblEmailAddress0.Text = "Email address is already in use. Please select another.";
        lblUsername.Visible = false;
        lblUsername.Text = "User name is already in use. Please select another.";
        lblMissingFields.Text = "Please fill in all of the fields. Apostrophes (') are not allowed in User names, Passwords and Emails.";
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
        if ((tbName.Text == "" || tbUserName.Text == "" || tbUserName.Text.Contains("'") || tbEmail.Text == "" || tbEmail.Text.Contains("'") ||
            tbEmailConfirm.Text == "" || tbEmailConfirm.Text.Contains("'") || tbPassword.Text.Contains("'") || tbPasswordConfirm.Text.Contains("'")))
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
    protected void gvReviewer_RowEditing(object sender, GridViewEditEventArgs e)
    {

    }

    protected void gvReviewNonShareable_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        int index = Convert.ToInt32(e.CommandArgument);
        string ReviewID = (string)gvReviewNonShareable.DataKeys[index].Value;
        switch (e.CommandName)
        {
            case "EDT":
                pnlEditNonShareableReview.Visible = true;
                pnlBritLibCodesNonShared.Visible = false;
                lblPSNonShareableEnable.Visible = false;
                rblPSNonShareableEnable.Visible = false;
                lblPSNonShareableEnable.Visible = false;
                rblPSNonShareableEnable.Visible = false;
                lblNonShareableReviewNumber.Text = ReviewID;
                bool isAdmDB = true;
                IDataReader idr = Utils.GetReader(isAdmDB, "st_ReviewDetails",
                    ReviewID);
                if (idr.Read())
                {
                    tbReviewName.Text = idr["REVIEW_NAME"].ToString();
                    if (Utils.GetSessionString("EnablePSEnabler") == "True")
                    {
                        lblPSNonShareableEnable.Visible = true;
                        rblPSNonShareableEnable.Visible = true;
                        rblPSNonShareableEnable.SelectedValue = idr["SHOW_SCREENING"].ToString();
                    }
                    
                }
                idr.Close();
                /*
                string SQL = "select REVIEW_NAME from TB_REVIEW where REVIEW_ID = '" + ReviewID + "'";
                bool isAdmDB = false;
                SqlDataReader sdr = Utils.ReturnReader(SQL, isAdmDB);
                if (sdr.Read())
                {
                    tbReviewName.Text = sdr["REVIEW_NAME"].ToString();
                }
                sdr.Close();
                */
                break;

            default:
                break;
        }
    }
    protected void gvReview_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        int index = Convert.ToInt32(e.CommandArgument);
        string ReviewID = (string)gvReview.DataKeys[index].Value;
        switch (e.CommandName)
        {
            case "EDT":

                buildMembersOfReview(ReviewID);


                /*
                rblPSShareableEnable.Visible = false;
                lblPSShareableReview.Visible = false;
                pnlBritLibCodesShared.Visible = false;
                pnlEditShareableReview.Visible = true;
                lblShareableReviewNumber.Text = ReviewID;
                bool isAdmDB = true;
                IDataReader idr = Utils.GetReader(isAdmDB, "st_ReviewDetails",
                    ReviewID);
                if (idr.Read())
                {
                    if ((idr["REVIEW_NAME"].ToString() == null) || (idr["REVIEW_NAME"].ToString() == ""))
                    {
                        if ((idr["EXPIRY_DATE"].ToString() == null) || (idr["EXPIRY_DATE"].ToString() == ""))
                        {
                            tbShareableReviewName.Text = "Not activated";
                            cmdSaveShareableReview.Text = "Save and activate";
                            lbInviteReviewer.Enabled = false;
                        }
                        else
                        {
                            tbShareableReviewName.Text = "Edit name";
                            cmdSaveShareableReview.Text = "Save";
                            lbInviteReviewer.Enabled = true;
                        }
                    }
                    else
                    {
                        if ((idr["EXPIRY_DATE"].ToString() == null) || (idr["EXPIRY_DATE"].ToString() == ""))
                        {
                            cmdSaveShareableReview.Text = "Save and activate";
                            lbInviteReviewer.Enabled = false;
                        }
                        else
                        {
                            cmdSaveShareableReview.Text = "Save";
                            lbInviteReviewer.Enabled = true;
                        }
                        tbShareableReviewName.Text = idr["REVIEW_NAME"].ToString();
                        if (Utils.GetSessionString("EnablePSEnabler") == "True")
                        {
                            rblPSShareableEnable.Visible = true;
                            lblPSShareableReview.Visible = true;
                            string test1 = idr["SHOW_SCREENING"].ToString();
                            rblPSShareableEnable.SelectedValue = idr["SHOW_SCREENING"].ToString();
                        }
                        
                    }

                    DateTime dayExpires;
                    DateTime today = DateTime.Today;

                    DataTable dt = new DataTable();
                    System.Data.DataRow newrow;

                    dt.Columns.Add(new DataColumn("CONTACT_ID", typeof(string)));
                    dt.Columns.Add(new DataColumn("CONTACT_NAME", typeof(string)));
                    dt.Columns.Add(new DataColumn("EMAIL", typeof(string)));
                    dt.Columns.Add(new DataColumn("LAST_LOGIN", typeof(string)));
                    dt.Columns.Add(new DataColumn("IS_CODING_ONLY", typeof(string)));
                    dt.Columns.Add(new DataColumn("IS_READ_ONLY", typeof(string)));
                    dt.Columns.Add(new DataColumn("IS_ADMIN", typeof(string)));

                    string expiryDate = "";
                    isAdmDB = true;                 
                    IDataReader idr1 = Utils.GetReader(isAdmDB, "st_ReviewMembers",
                        lblShareableReviewNumber.Text);
                    while (idr1.Read())
                    {
                        newrow = dt.NewRow();
                        newrow["CONTACT_ID"] = idr1["CONTACT_ID"].ToString();
                        
                        if (idr1["site_lic_id"].ToString() != "")
                        {
                            newrow["CONTACT_NAME"] = idr1["CONTACT_NAME"].ToString() + " (" + expiryDate + ")" + " in Site License #" +
                                idr1["site_lic_id"].ToString();
                        }
                        else if ((idr1["expiry_date"].ToString() == null) || (idr1["expiry_date"].ToString() == ""))
                        {
                            newrow["CONTACT_NAME"] = idr1["CONTACT_NAME"].ToString() + " (N/A) Not activated";
                        }
                        else
                        {
                            dayExpires = Convert.ToDateTime(idr1["expiry_date"].ToString());

                            expiryDate = dayExpires.ToString();
                            expiryDate = expiryDate.Remove(expiryDate.IndexOf(" "));
                            if (dayExpires < today)
                            {
                                newrow["CONTACT_NAME"] = idr1["CONTACT_NAME"].ToString() + " (" + expiryDate + ")" + " Expired";
                            }
                            else
                            {
                                newrow["CONTACT_NAME"] = idr1["CONTACT_NAME"].ToString() + " (" + expiryDate + ")";
                            }
                        }
                        

                        newrow["EMAIL"] = idr1["EMAIL"].ToString();
                        if ((idr1["LAST_LOGIN"].ToString() == null) || (idr1["LAST_LOGIN"].ToString() == ""))
                            newrow["LAST_LOGIN"] = "Never";
                        else
                            newrow["LAST_LOGIN"] = idr1["LAST_LOGIN"].ToString();
                        
                        if (idr1["IS_CODING_ONLY"].ToString() == "True")
                            newrow["IS_CODING_ONLY"] = "True";
                        if (idr1["IS_READ_ONLY"].ToString() == "True")
                            newrow["IS_READ_ONLY"] = "True";
                        if (idr1["IS_ADMIN"].ToString() == "True")
                            newrow["IS_ADMIN"] = "True";
                        dt.Rows.Add(newrow);
                    }
                    idr1.Close();

                    gvMembersOfReview.DataSource = dt;
                    gvMembersOfReview.DataBind();

                    // loop through gvMembersOfReview and set checkboxes
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (dt.Rows[i]["IS_CODING_ONLY"].ToString() == "True")
                        {
                            GridViewRow row = gvMembersOfReview.Rows[i];
                            CheckBox cb = ((CheckBox)row.FindControl("cbCodingOnly"));
                            cb.Checked = true;
                        }
                    }
                    // loop through gvMembersOfReview and set checkboxes
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (dt.Rows[i]["IS_READ_ONLY"].ToString() == "True")
                        {
                            GridViewRow row = gvMembersOfReview.Rows[i];
                            CheckBox cb = ((CheckBox)row.FindControl("cbReadOnly"));
                            cb.Checked = true;
                        }
                    }
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (dt.Rows[i]["IS_ADMIN"].ToString() == "True")
                        {
                            GridViewRow row = gvMembersOfReview.Rows[i];
                            CheckBox cb = ((CheckBox)row.FindControl("cbReviewAdmin"));
                            cb.Checked = true;
                        }
                    }
                    idr.Close();
                }

                for (int i = 0; i < gvMembersOfReview.Rows.Count; i++)
                {
                    if (gvMembersOfReview.Rows[i].Cells[1].Text.Contains("Edit name"))
                    {
                        gvMembersOfReview.Rows[i].Cells[1].BackColor = System.Drawing.Color.PaleGreen;
                    }
                    if (gvMembersOfReview.Rows[i].Cells[1].Text.Contains("Not activated"))
                    {
                        gvMembersOfReview.Rows[i].Cells[1].BackColor = System.Drawing.Color.Yellow;
                    }
                    if (gvMembersOfReview.Rows[i].Cells[1].Text.Contains("Not activated"))
                    {
                        gvMembersOfReview.Rows[i].Cells[1].BackColor = System.Drawing.Color.Yellow;
                    }
                    if (gvMembersOfReview.Rows[i].Cells[1].Text.Contains("Expired"))
                    {
                        gvMembersOfReview.Rows[i].Cells[1].BackColor = System.Drawing.Color.Pink;
                    }
                    if (gvMembersOfReview.Rows[i].Cells[1].Text.Contains("Site License"))
                    {
                        gvMembersOfReview.Rows[i].Cells[1].BackColor = System.Drawing.Color.Aquamarine;
                    }
                }
                */
                break;

            default:
                break;
        }
    }
    protected void gvReviewNonShareable_RowEditing(object sender, GridViewEditEventArgs e)
    {

    }

    protected void gvReview_RowEditing(object sender, GridViewEditEventArgs e)
    {

    }
    protected void cmdSaveNonShareableReview_Click(object sender, EventArgs e)
    {

        bool isAdmDB = true;
        Utils.ExecuteSP(isAdmDB, Server, "st_ReviewEditName",
             lblNonShareableReviewNumber.Text, tbReviewName.Text);
        /*
        string SQL = "update TB_REVIEW set REVIEW_NAME = '" + tbReviewName.Text.Replace("'", "''") +
            "' where REVIEW_ID = '" + lblNonShareableReviewNumber.Text + "'";
        
        Utils.ExecuteQuery(SQL, isAdmDB);
        */
        pnlEditNonShareableReview.Visible = false;
        pnlChangeToShareable.Visible = false;
        buildNonShareableReviewGrid();
    }
    protected void lbChangeToShareable_Click(object sender, EventArgs e)
    {
        pnlChangeToShareable.Visible = true;
    }
    protected void cmdSaveShareableReview_Click(object sender, EventArgs e)
    {
        /*
        string SQL = "update TB_REVIEW set REVIEW_NAME = '" + tbShareableReviewName.Text.Replace("'", "''") +
            "' where REVIEW_ID = '" + lblShareableReviewNumber.Text + "'";
        Utils.ExecuteQuery(SQL, isAdmDB);
        */
        if ((tbShareableReviewName.Text == "") || (tbShareableReviewName.Text == "Not activated"))
        {
            // maybe add a message in the future but for now do nothing
        }
        else
        {
            bool isAdmDB = true;
            if (cmdSaveShareableReview.Text == "Save and activate")
            {

                Utils.ExecuteSP(isAdmDB, Server, "st_GhostReviewActivate",
                     lblShareableReviewNumber.Text, tbShareableReviewName.Text);
            }
            if (tbShareableReviewName.Text != "")
            {

                Utils.ExecuteSP(isAdmDB, Server, "st_ReviewEditName",
                     lblShareableReviewNumber.Text, tbShareableReviewName.Text);
            }

            pnlEditShareableReview.Visible = false;
            pnlInviteReviewer.Visible = false;
            buildShareableReviewGrid();
        }
    }
    protected void lbInviteReviewer_Click(object sender, EventArgs e)
    {
        pnlInviteReviewer.Visible = true;
    }


    protected void gvReviewShareableMember_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        bool isAdmDB = false;
        int index = Convert.ToInt32(e.CommandArgument);
        string ReviewID = (string)gvReviewShareableMember.DataKeys[index].Value;
        switch (e.CommandName)
        {
            case "REMOVE":
                isAdmDB = true;
                Utils.ExecuteSP(isAdmDB, Server, "st_ReviewRemoveMember_1",
                    ReviewID, Utils.GetSessionString("Contact_ID"));
                /*
                string SQL = "delete TB_CONTACT_REVIEW_ROLE from TB_CONTACT_REVIEW_ROLE crr " +
                    "inner join TB_REVIEW_CONTACT rc " +
                    "on rc.REVIEW_CONTACT_ID = crr.REVIEW_CONTACT_ID " +
                    "and rc.CONTACT_ID = '" + Utils.GetSessionString("Contact_ID") + "' " +
                    "and rc.REVIEW_ID = '" + ReviewID + "'";      
                Utils.ExecuteQuery(SQL, isAdmDB);
                SQL = "delete from TB_REVIEW_CONTACT where CONTACT_ID = '" + 
                    Utils.GetSessionString("Contact_ID") + "' " +
                    "and REVIEW_ID = '" + ReviewID + "'";
                Utils.ExecuteQuery(SQL, isAdmDB);
                */
                buildOtherShareableReviewGrid();
                break;

            default:
                break;
        }
    }


    protected void gvReviewShareableMember_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            LinkButton lb = (LinkButton)(e.Row.Cells[5].Controls[0]);
            lb.Attributes.Add("onclick", "if (confirm('Are you sure you want to remove yourself from this review?') == false) return false;");
        }
    }




    protected void gvMembersOfReview_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        bool isAdmDB = false;
        int index = Convert.ToInt32(e.CommandArgument);
        string ContactID = (string)gvMembersOfReview.DataKeys[index].Value;
        switch (e.CommandName)
        {
            case "REMOVE":
                isAdmDB = true;
                Utils.ExecuteSP(isAdmDB, Server, "st_ReviewRemoveMember_1",
                    lblShareableReviewNumber.Text, ContactID);
                /*
                string SQL = "delete TB_CONTACT_REVIEW_ROLE from TB_CONTACT_REVIEW_ROLE crr " +
                    "inner join TB_REVIEW_CONTACT rc " +
                    "on rc.REVIEW_CONTACT_ID = crr.REVIEW_CONTACT_ID " +
                    "and rc.CONTACT_ID = '" + ContactID + "' " +
                    "and rc.REVIEW_ID = '" + lblShareableReviewNumber.Text + "'";
                Utils.ExecuteQuery(SQL, isAdmDB);
                SQL = "delete from TB_REVIEW_CONTACT where CONTACT_ID = '" +
                    ContactID + "' " +
                    "and REVIEW_ID = '" + lblShareableReviewNumber.Text + "'";
                Utils.ExecuteQuery(SQL, isAdmDB);
                */
                buildMembersOfReview(lblShareableReviewNumber.Text);
                break;

            default:
                break;
        }
    }
    private void buildMembersOfReview(string reviewID)
    {
        rblPSShareableEnable.Visible = false;
        lblPSShareableReview.Visible = false;
        pnlBritLibCodesShared.Visible = false;
        pnlEditShareableReview.Visible = true;
        lblShareableReviewNumber.Text = reviewID;
        bool isAdmDB = true;
        IDataReader idr = Utils.GetReader(isAdmDB, "st_ReviewDetails",
            reviewID);
        if (idr.Read())
        {
            if ((idr["REVIEW_NAME"].ToString() == null) || (idr["REVIEW_NAME"].ToString() == ""))
            {
                if ((idr["EXPIRY_DATE"].ToString() == null) || (idr["EXPIRY_DATE"].ToString() == ""))
                {
                    tbShareableReviewName.Text = "Not activated";
                    cmdSaveShareableReview.Text = "Save and activate";
                    lbInviteReviewer.Enabled = false;
                }
                else
                {
                    tbShareableReviewName.Text = "Edit name";
                    cmdSaveShareableReview.Text = "Save";
                    lbInviteReviewer.Enabled = true;
                }
            }
            else
            {
                if ((idr["EXPIRY_DATE"].ToString() == null) || (idr["EXPIRY_DATE"].ToString() == ""))
                {
                    cmdSaveShareableReview.Text = "Save and activate";
                    lbInviteReviewer.Enabled = false;
                }
                else
                {
                    cmdSaveShareableReview.Text = "Save";
                    lbInviteReviewer.Enabled = true;
                }
                tbShareableReviewName.Text = idr["REVIEW_NAME"].ToString();
                if (Utils.GetSessionString("EnablePSEnabler") == "True")
                {
                    rblPSShareableEnable.Visible = true;
                    lblPSShareableReview.Visible = true;
                    string test1 = idr["SHOW_SCREENING"].ToString();
                    rblPSShareableEnable.SelectedValue = idr["SHOW_SCREENING"].ToString();
                }

            }

            DateTime dayExpires;
            DateTime lastAccessed;
            DateTime today = DateTime.Today;

            DataTable dt = new DataTable();
            System.Data.DataRow newrow;

            dt.Columns.Add(new DataColumn("CONTACT_ID", typeof(string)));
            dt.Columns.Add(new DataColumn("CONTACT_NAME", typeof(string)));
            dt.Columns.Add(new DataColumn("EMAIL", typeof(string)));
            dt.Columns.Add(new DataColumn("LAST_LOGIN", typeof(string)));
            dt.Columns.Add(new DataColumn("IS_CODING_ONLY", typeof(string)));
            dt.Columns.Add(new DataColumn("IS_READ_ONLY", typeof(string)));
            dt.Columns.Add(new DataColumn("IS_ADMIN", typeof(string)));

            string expiryDate = "";
            isAdmDB = true;
            IDataReader idr1 = Utils.GetReader(isAdmDB, "st_ReviewMembers",
                lblShareableReviewNumber.Text);
            while (idr1.Read())
            {
                newrow = dt.NewRow();
                newrow["CONTACT_ID"] = idr1["CONTACT_ID"].ToString();

                if (idr1["site_lic_id"].ToString() != "")
                {
                    newrow["CONTACT_NAME"] = idr1["CONTACT_NAME"].ToString() + " (" + expiryDate + ")" + " in Site License #" +
                        idr1["site_lic_id"].ToString();
                }
                else if ((idr1["expiry_date"].ToString() == null) || (idr1["expiry_date"].ToString() == ""))
                {
                    newrow["CONTACT_NAME"] = idr1["CONTACT_NAME"].ToString() + " (N/A) Not activated";
                }
                else
                {
                    dayExpires = Convert.ToDateTime(idr1["expiry_date"].ToString());

                    expiryDate = dayExpires.ToString("dd MMM yyyy");
                    //expiryDate = dayExpires.ToString();
                    //expiryDate = expiryDate.Remove(expiryDate.IndexOf(" "));
                    if (dayExpires < today)
                    {
                        newrow["CONTACT_NAME"] = idr1["CONTACT_NAME"].ToString() + " (" + expiryDate + ")" + " Expired";
                    }
                    else
                    {
                        newrow["CONTACT_NAME"] = idr1["CONTACT_NAME"].ToString() + " (" + expiryDate + ")";
                    }
                }

                newrow["EMAIL"] = idr1["EMAIL"].ToString();
                if ((idr1["LAST_LOGIN"].ToString() == null) || (idr1["LAST_LOGIN"].ToString() == ""))
                {
                    newrow["LAST_LOGIN"] = "Never";
                }
                else
                {
                    lastAccessed = Convert.ToDateTime(idr1["LAST_LOGIN"].ToString());
                    newrow["LAST_LOGIN"] = lastAccessed.ToString("dd MMM yyyy HH:mm");
                    //newrow["LAST_LOGIN"] = idr1["LAST_LOGIN"].ToString();
                }

                if (idr1["IS_CODING_ONLY"].ToString() == "True")
                    newrow["IS_CODING_ONLY"] = "True";
                if (idr1["IS_READ_ONLY"].ToString() == "True")
                    newrow["IS_READ_ONLY"] = "True";
                if (idr1["IS_ADMIN"].ToString() == "True")
                    newrow["IS_ADMIN"] = "True";
                dt.Rows.Add(newrow);
            }
            idr1.Close();

            gvMembersOfReview.DataSource = dt;
            gvMembersOfReview.DataBind();

            // loop through gvMembersOfReview and set checkboxes
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (dt.Rows[i]["IS_CODING_ONLY"].ToString() == "True")
                {
                    GridViewRow row = gvMembersOfReview.Rows[i];
                    CheckBox cb = ((CheckBox)row.FindControl("cbCodingOnly"));
                    cb.Checked = true;
                }
            }
            // loop through gvMembersOfReview and set checkboxes
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (dt.Rows[i]["IS_READ_ONLY"].ToString() == "True")
                {
                    GridViewRow row = gvMembersOfReview.Rows[i];
                    CheckBox cb = ((CheckBox)row.FindControl("cbReadOnly"));
                    cb.Checked = true;
                }
            }
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (dt.Rows[i]["IS_ADMIN"].ToString() == "True")
                {
                    GridViewRow row = gvMembersOfReview.Rows[i];
                    CheckBox cb = ((CheckBox)row.FindControl("cbReviewAdmin"));
                    cb.Checked = true;
                }
            }
            idr.Close();
        }

        for (int i = 0; i < gvMembersOfReview.Rows.Count; i++)
        {
            if (gvMembersOfReview.Rows[i].Cells[1].Text.Contains("Edit name"))
            {
                gvMembersOfReview.Rows[i].Cells[1].BackColor = System.Drawing.Color.PaleGreen;
            }
            if (gvMembersOfReview.Rows[i].Cells[1].Text.Contains("Not activated"))
            {
                gvMembersOfReview.Rows[i].Cells[1].BackColor = System.Drawing.Color.Yellow;
            }
            if (gvMembersOfReview.Rows[i].Cells[1].Text.Contains("Not activated"))
            {
                gvMembersOfReview.Rows[i].Cells[1].BackColor = System.Drawing.Color.Yellow;
            }
            if (gvMembersOfReview.Rows[i].Cells[1].Text.Contains("Expired"))
            {
                gvMembersOfReview.Rows[i].Cells[1].BackColor = System.Drawing.Color.Pink;
            }
            if (gvMembersOfReview.Rows[i].Cells[1].Text.Contains("Site License"))
            {
                gvMembersOfReview.Rows[i].Cells[1].BackColor = System.Drawing.Color.Aquamarine;
            }
        }








        /*
        DataTable dt = new DataTable();
        System.Data.DataRow newrow;

        dt.Columns.Add(new DataColumn("CONTACT_ID", typeof(string)));
        dt.Columns.Add(new DataColumn("CONTACT_NAME", typeof(string)));
        dt.Columns.Add(new DataColumn("EMAIL", typeof(string)));
        dt.Columns.Add(new DataColumn("LAST_LOGIN", typeof(string)));
        dt.Columns.Add(new DataColumn("IS_CODING_ONLY", typeof(string)));
        dt.Columns.Add(new DataColumn("IS_READ_ONLY", typeof(string)));
        dt.Columns.Add(new DataColumn("IS_ADMIN", typeof(string)));

        string expiryDate = "";
        bool isAdmDB = true;
        IDataReader idr = Utils.GetReader(isAdmDB, "st_ReviewMembers",
            reviewID);
        while (idr.Read())
        {
            newrow = dt.NewRow();
            newrow["CONTACT_ID"] = idr["CONTACT_ID"].ToString();
            if (idr["site_lic_id"].ToString() != "")
            {
                newrow["CONTACT_NAME"] = idr["CONTACT_NAME"].ToString() + " (" + expiryDate + ")" + " in Site License #" +
                    idr["site_lic_id"].ToString();
            }
            else if ((idr["expiry_date"].ToString() == null) || (idr["expiry_date"].ToString() == ""))
            {
                newrow["CONTACT_NAME"] = idr["CONTACT_NAME"].ToString() + " (N/A) Not Activated";
            }
            else
            {
                DateTime dayExpires = Convert.ToDateTime(idr["expiry_date"].ToString());
                DateTime today = DateTime.Today;
                expiryDate = dayExpires.ToString();
                expiryDate = expiryDate.Remove(expiryDate.IndexOf(" "));
                if (dayExpires < today)
                {
                    newrow["CONTACT_NAME"] = idr["CONTACT_NAME"].ToString() + " (" + expiryDate + ")" + " Expired";
                }
                else
                {
                    newrow["CONTACT_NAME"] = idr["CONTACT_NAME"].ToString() + " (" + expiryDate + ")";
                }
            }
            
            newrow["EMAIL"] = idr["EMAIL"].ToString();
            if ((idr["LAST_LOGIN"].ToString() == null) || (idr["LAST_LOGIN"].ToString() == ""))
                newrow["LAST_LOGIN"] = "Never";
            else
                newrow["LAST_LOGIN"] = idr["LAST_LOGIN"].ToString();

            if (idr["IS_CODING_ONLY"].ToString() == "True")
                newrow["IS_CODING_ONLY"] = "True";
            if (idr["IS_READ_ONLY"].ToString() == "True")
                newrow["IS_READ_ONLY"] = "True";
            if (idr["IS_ADMIN"].ToString() == "True")
                newrow["IS_ADMIN"] = "True";
            dt.Rows.Add(newrow);
        }
        idr.Close();

        gvMembersOfReview.DataSource = dt;
        gvMembersOfReview.DataBind();

        // loop through gvMembersOfReview and set checkboxes
        for (int i = 0; i < dt.Rows.Count; i++)
        {
            if (dt.Rows[i]["IS_CODING_ONLY"].ToString() == "True")
            {
                GridViewRow row = gvMembersOfReview.Rows[i];
                CheckBox cb = ((CheckBox)row.FindControl("cbCodingOnly"));
                cb.Checked = true;
            }
        }
        // loop through gvMembersOfReview and set checkboxes
        for (int i = 0; i < dt.Rows.Count; i++)
        {
            if (dt.Rows[i]["IS_READ_ONLY"].ToString() == "True")
            {
                GridViewRow row = gvMembersOfReview.Rows[i];
                CheckBox cb = ((CheckBox)row.FindControl("cbReadOnly"));
                cb.Checked = true;
            }
        }
        for (int i = 0; i < dt.Rows.Count; i++)
        {
            if (dt.Rows[i]["IS_ADMIN"].ToString() == "True")
            {
                GridViewRow row = gvMembersOfReview.Rows[i];
                CheckBox cb = ((CheckBox)row.FindControl("cbReviewAdmin"));
                cb.Checked = true;
            }
        }
        idr.Close();
        */
    }

    protected void cmdInvite_Click(object sender, EventArgs e)
    {
        lblInviteMsg.Visible = false;
        bool isAdmDB = true;
        int rowCount = 0;
        string senderName = "";
        string senderEmail = "";
        IDataReader idr = Utils.GetReader(isAdmDB, "st_ContactDetails",
                    Utils.GetSessionString("Contact_ID"));
        if (idr.Read())
        {
            senderName = idr["CONTACT_NAME"].ToString();
            senderEmail = idr["EMAIL"].ToString();
        }
        idr.Close();

        idr = Utils.GetReader(isAdmDB, "st_ContactDetailsEmail",
                    tbInvite.Text);
        while (idr.Read())
        {
            // check that there is only one account with this email address. Old ER3 accounts may be an issue.
            rowCount += 1;
        }
        idr.Close();
        if (rowCount > 1)
        {
            lblInviteMsg.Text = "There is more than one account with this email address. Please contact EPPI-Reviewer 4 support staff at EPPISupport@ucl.ac.uk.";
            lblInviteMsg.Visible = true;
        }
        else if (rowCount < 1)
        {
            lblInviteMsg.Text = "There is no match in the EPPI-Reviewer 4 database for this email address";
            lblInviteMsg.Visible = true;
        }
        else
        {
            //one last check. is this user already in the review
            bool userAlreadyThere = false;
            for (int i = 0; i < gvMembersOfReview.Rows.Count; i++)
            {
                if (gvMembersOfReview.Rows[i].Cells[2].Text == tbInvite.Text)
                {
                    userAlreadyThere = true;
                }
            }

            if (userAlreadyThere == false)
            {
                idr = Utils.GetReader(isAdmDB, "st_ContactDetailsEmail",
                        tbInvite.Text);
                if (idr.Read())
                {
                    // place the invitee in the review
                    Utils.ExecuteSP(isAdmDB, Server, "st_ReviewAddMember",
                        lblShareableReviewNumber.Text, idr["CONTACT_ID"].ToString());

                    // check that the account meets the new account requirements. 
                    // If it doesn't include this info in the email sent to the invitee.
                    bool accountConditionsMet = true;
                    string emailAccountMsg = "<b>Your EPPI-Reviewer 4 account details need updating</b>.<br>Please log into the EPPI-Reviewer 4.0 gateway at " +
                        //"\http://eppi.ioe.ac.uk/cms/er4 and update your username and password";

                    "<a href='http://eppi.ioe.ac.uk/cms/er4'>eppi.ioe.ac.uk/cms/er4</a> to update your details.";

                    //Regex passwordRegex = new Regex("^.*(?=.{8,})(?=.*\\d)(?=.*[a-z])(?=.*[A-Z]).*$");
                    //Match m = passwordRegex.Match(idr["PASSWORD"].ToString());
                    //if (!m.Success)
                    //{
                    //    accountConditionsMet = false;
                    //}

                    if (idr["USERNAME"].ToString().Length < 4)
                    {
                        accountConditionsMet = false;
                    }

                    if (accountConditionsMet == true)
                    {
                        emailAccountMsg = "";
                    }

                    // send out the email
                    string sendResult = Utils.InviteEmail(tbInvite.Text, idr["CONTACT_NAME"].ToString(), tbShareableReviewName.Text, senderName,
                        senderEmail, emailAccountMsg);
                    lblInviteMsg.Text = sendResult;
                    lblInviteMsg.Visible = true;
                }
                idr.Close();
                // update table
                buildMembersOfReview(lblShareableReviewNumber.Text);
            }
            else
            {
                lblInviteMsg.Text = "This account is already in the review.";
                lblInviteMsg.Visible = true;
            }
        }
    }
    protected void lbCancelAccountEdit_Click(object sender, EventArgs e)
    {
        pnlContactDetails.Visible = false;
        pnlConfirmRevokeGhostActivation.Visible = false;
    }
    protected void lbCancelReviewDetailsEdit_Click(object sender, EventArgs e)
    {
        pnlReviewDetails.Visible = false;
        Server.Transfer("Summary.aspx");
    }
    protected void lbCancelNSReviewDetailsEdit_Click(object sender, EventArgs e)
    {
        pnlEditNonShareableReview.Visible = false;
    }
    protected void gvMembersOfReview_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            LinkButton lb = (LinkButton)(e.Row.Cells[7].Controls[0]);
            lb.Attributes.Add("onclick", "if (confirm('Are you sure you want to remove this user from this review?') == false) return false;");
        }
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
    protected void cbCodingOnly_CheckedChanged(object sender, EventArgs e)
    {
        ContentPlaceHolder mpContentPlaceHolder;
        mpContentPlaceHolder = (ContentPlaceHolder)Master.FindControl("ContentPlaceHolder1");
        if (mpContentPlaceHolder != null)
        {
            CheckBox chkbox = (CheckBox)sender;
            GridViewRow row = (GridViewRow)chkbox.Parent.Parent;
            string ContactID = (string)gvMembersOfReview.DataKeys[row.RowIndex].Value.ToString();
            bool isChecked = chkbox.Checked;
            //update the role for this user
            bool isAdmDB = true;
            Utils.ExecuteSP(isAdmDB, Server, "st_ReviewRoleCodingOnlyUpdate", lblShareableReviewNumber.Text, ContactID, isChecked);
        }
    }
    protected void cbReadOnly_CheckedChanged(object sender, EventArgs e)
    {
        ContentPlaceHolder mpContentPlaceHolder;
        mpContentPlaceHolder = (ContentPlaceHolder)Master.FindControl("ContentPlaceHolder1");
        if (mpContentPlaceHolder != null)
        {
            CheckBox chkbox = (CheckBox)sender;
            GridViewRow row = (GridViewRow)chkbox.Parent.Parent;
            string ContactID = (string)gvMembersOfReview.DataKeys[row.RowIndex].Value.ToString();
            bool isChecked = chkbox.Checked;
            //update the role for this user
            bool isAdmDB = true;
            Utils.ExecuteSP(isAdmDB, Server, "st_ReviewRoleReadOnlyUpdate", lblShareableReviewNumber.Text, ContactID, isChecked);
        }
    }
    protected void cbReviewAdmin_CheckedChanged(object sender, EventArgs e)
    {
        ContentPlaceHolder mpContentPlaceHolder;
        mpContentPlaceHolder = (ContentPlaceHolder)Master.FindControl("ContentPlaceHolder1");
        if (mpContentPlaceHolder != null)
        {
            CheckBox chkbox = (CheckBox)sender;
            GridViewRow row = (GridViewRow)chkbox.Parent.Parent;
            string ContactID = (string)gvMembersOfReview.DataKeys[row.RowIndex].Value.ToString();
            bool isChecked = chkbox.Checked;
            //update the role for this user
            bool isAdmDB = true;
            Utils.ExecuteSP(isAdmDB, Server, "st_ReviewRoleIsAdminUpdate", lblShareableReviewNumber.Text, ContactID, isChecked);
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



    private void buildReviewGridCochraneProspective()
    {
        DateTime dayExpires;
        DateTime today = DateTime.Today;
        DataTable dt = new DataTable();
        System.Data.DataRow newrow;
        string test = "aa";

        DateTime dateCreated;
        DateTime lastAccessed;

        dt.Columns.Add(new DataColumn("REVIEW_ID", typeof(string)));
        dt.Columns.Add(new DataColumn("REVIEW_NAME", typeof(string)));
        dt.Columns.Add(new DataColumn("DATE_CREATED", typeof(string)));
        dt.Columns.Add(new DataColumn("LAST_LOGIN", typeof(string)));
        dt.Columns.Add(new DataColumn("ARCHIE_ID", typeof(string)));
        bool isAdmDB = true;
        IDataReader idr = Utils.GetReader(isAdmDB, "st_ContactReviewsArchieProspective",
            Utils.GetSessionString("Contact_ID"));
        while (idr.Read())
        {
            newrow = dt.NewRow();
            newrow["REVIEW_ID"] = idr["REVIEW_ID"].ToString();
            newrow["REVIEW_NAME"] = idr["REVIEW_NAME"].ToString();

            if ((idr["DATE_CREATED"].ToString() == null) || (idr["DATE_CREATED"].ToString() == ""))
            {
                newrow["DATE_CREATED"] = "N/A";
            }
            else
            {
                dateCreated = Convert.ToDateTime(idr["DATE_CREATED"].ToString());
                newrow["DATE_CREATED"] = dateCreated.ToString("dd MMM yyyy");
                //newrow["DATE_CREATED"] = idr["DATE_CREATED"].ToString();
            }

            if (idr["ARCHIE_ID"].ToString() == "prospective_______")
                newrow["ARCHIE_ID"] = "Prospective";
            else
                newrow["ARCHIE_ID"] = idr["ARCHIE_ID"].ToString();

            if ((idr["CREATED"].ToString() == null) || (idr["CREATED"].ToString() == ""))
            {
                newrow["LAST_LOGIN"] = "N/A";
            }
            else
            {
                lastAccessed = Convert.ToDateTime(idr["CREATED"].ToString());
                newrow["LAST_LOGIN"] = lastAccessed.ToString("dd MMM yyyy HH:mm");
                //newrow["LAST_LOGIN"] = idr["CREATED"].ToString();
            }

            dt.Rows.Add(newrow);
        }
        idr.Close();

        gvReviewCochrane.DataSource = dt;
        gvReviewCochrane.DataBind();

        if (dt.Rows.Count > 0)
            pnlCochraneReviews.Visible = true;

        if (dt.Rows.Count == 0)
            lblShareableReviewsCochrane.Visible = true;
        else
        {
            for (int i = 0; i < gvReviewCochrane.Rows.Count; i++)
            {
                if (gvReviewCochrane.Rows[i].Cells[1].Text.Contains("Edit name"))
                {
                    gvReviewCochrane.Rows[i].Cells[1].BackColor = System.Drawing.Color.PaleGreen;
                }
                if (gvReviewCochrane.Rows[i].Cells[1].Text.Contains("Not activated"))
                {
                    gvReviewCochrane.Rows[i].Cells[1].BackColor = System.Drawing.Color.Yellow;
                }
                if (gvReviewCochrane.Rows[i].Cells[4].Text.Contains("Not activated"))
                {
                    gvReviewCochrane.Rows[i].Cells[4].BackColor = System.Drawing.Color.Yellow;
                }
                if (gvReviewCochrane.Rows[i].Cells[4].Text.Contains("Expired"))
                {
                    gvReviewCochrane.Rows[i].Cells[4].BackColor = System.Drawing.Color.Pink;
                    gvReviewCochrane.Rows[i].Cells[4].Font.Bold = true;
                }
                if (gvReviewCochrane.Rows[i].Cells[4].Text.Contains(" In Site License '"))
                {
                    gvReviewCochrane.Rows[i].Cells[4].BackColor = System.Drawing.Color.Aquamarine;
                }
            }
        }
    }


    protected void gvReviewCochrane_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        int index = Convert.ToInt32(e.CommandArgument);
        string ReviewID = (string)gvReviewCochrane.DataKeys[index].Value;
        bool prospective = false;
        //bool isAdmin = true;
        bool isReviewAdmin = false;
        if (gvReviewCochrane.Rows[index].Cells[5].Text == "Prospective")
            prospective = true;
        switch (e.CommandName)
        {
            case "EDITROW":
                pnlBritLibCodesProCochrane.Visible = false;
                lbInviteReviewerCochrane.Visible = false;
                lblPSProsCochraneReviewEnable.Visible = false;
                rblPSProsCochraneReviewEnable.Visible = false;
                if (prospective == true)
                    lbInviteReviewerCochrane.Visible = true;
                pnlEditShareableReviewCochrane.Visible = true;
                lblShareableReviewNumberCochrane.Text = ReviewID;
                DateTime lastAccessed;
                bool isAdmDB = true;
                IDataReader idr = Utils.GetReader(isAdmDB, "st_ReviewDetails",
                    ReviewID);
                if (idr.Read())
                {
                    tbShareableReviewNameCochrane.Text = idr["REVIEW_NAME"].ToString();
                    if (Utils.GetSessionString("EnablePSEnabler") == "True")
                    {
                        lblPSProsCochraneReviewEnable.Visible = true;
                        rblPSProsCochraneReviewEnable.Visible = true;
                        rblPSProsCochraneReviewEnable.SelectedValue = idr["SHOW_SCREENING"].ToString();
                    }
                    lblFID.Text = idr["FUNDER_ID"].ToString();

                    DataTable dt = new DataTable();
                    System.Data.DataRow newrow;

                    dt.Columns.Add(new DataColumn("CONTACT_ID", typeof(string)));
                    dt.Columns.Add(new DataColumn("CONTACT_NAME", typeof(string)));
                    dt.Columns.Add(new DataColumn("EMAIL", typeof(string)));
                    dt.Columns.Add(new DataColumn("LAST_LOGIN", typeof(string)));
                    dt.Columns.Add(new DataColumn("IS_CODING_ONLY", typeof(string)));
                    dt.Columns.Add(new DataColumn("IS_READ_ONLY", typeof(string)));
                    dt.Columns.Add(new DataColumn("IS_ADMIN", typeof(string)));
                    string test = "A";
                    isAdmDB = true;
                    IDataReader idr1 = Utils.GetReader(isAdmDB, "st_ReviewMembers_2",
                        lblShareableReviewNumberCochrane.Text);
                    while (idr1.Read())
                    {
                        newrow = dt.NewRow();
                        newrow["CONTACT_ID"] = idr1["CONTACT_ID"].ToString();
                        newrow["CONTACT_NAME"] = idr1["CONTACT_NAME"].ToString();
                        test = idr1["CONTACT_NAME"].ToString();
                        newrow["EMAIL"] = idr1["EMAIL"].ToString();

                        if ((idr1["LAST_LOGIN"].ToString() == null) || (idr1["LAST_LOGIN"].ToString() == ""))
                        {
                            newrow["LAST_LOGIN"] = "Never";
                        }
                        else
                        {
                            lastAccessed = Convert.ToDateTime(idr["LAST_LOGIN"].ToString());
                            newrow["LAST_LOGIN"] = lastAccessed.ToString("dd MMM yyyy HH:mm");
                            //newrow["LAST_LOGIN"] = idr1["LAST_LOGIN"].ToString();
                        }

                        test = idr1["IS_CODING_ONLY"].ToString();
                        if (idr1["IS_CODING_ONLY"].ToString() == "True")
                            newrow["IS_CODING_ONLY"] = "True";
                        test = idr1["IS_READ_ONLY"].ToString();
                        if (idr1["IS_READ_ONLY"].ToString() == "True")
                            newrow["IS_READ_ONLY"] = "True";
                        test = idr1["IS_ADMIN"].ToString();
                        if (idr1["IS_ADMIN"].ToString() == "True")
                            newrow["IS_ADMIN"] = "True";

                        if ((idr1["CONTACT_ID"].ToString() == Utils.GetSessionString("Contact_ID")) &&
                            (idr1["IS_ADMIN"].ToString() == "True"))
                            isReviewAdmin = true;

                        dt.Rows.Add(newrow);
                    }
                    idr1.Close();

                    gvMembersOfReviewCochrane.DataSource = dt;
                    gvMembersOfReviewCochrane.DataBind();

                    pnlReviewDetailsCochrane.Visible = true;

                    if (isReviewAdmin == false)
                    {
                        cmdSaveShareableReviewCochrane.Visible = false;
                        lbInviteReviewerCochrane.Visible = false;
                    }
                    else
                    {
                        cmdSaveShareableReviewCochrane.Visible = true;
                        lbInviteReviewerCochrane.Visible = true;
                    }


                    // loop through gvMembersOfReview and set checkboxes
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        GridViewRow row = gvMembersOfReviewCochrane.Rows[i];
                        CheckBox cb = ((CheckBox)row.FindControl("cbCodingOnlyCochrane"));
                        if (isReviewAdmin == false)
                            cb.Enabled = false;
                        if (dt.Rows[i]["IS_CODING_ONLY"].ToString() == "True")
                        {
                            cb.Checked = true;  
                        }
                        
                    }
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {

                        GridViewRow row = gvMembersOfReviewCochrane.Rows[i];
                        CheckBox cb = ((CheckBox)row.FindControl("cbReadOnlyCochrane"));
                        if (isReviewAdmin == false)
                            cb.Enabled = false;
                        if (dt.Rows[i]["IS_READ_ONLY"].ToString() == "True")
                        {
                            cb.Checked = true;
                        }
                        
                    }
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        GridViewRow row = gvMembersOfReviewCochrane.Rows[i];
                        CheckBox cb = ((CheckBox)row.FindControl("cbReviewAdminCochrane"));
                        if (isReviewAdmin == false)
                            cb.Enabled = false;
                        if (dt.Rows[i]["IS_ADMIN"].ToString() == "True")
                        {
                            cb.Checked = true;
                        }
                    }


                }
                idr.Close();
                break;

            default:
                break;
        }
    }
    protected void cmdInviteCochrane_Click(object sender, EventArgs e)
    {
        lblInviteMsgCochrane.Visible = false;
        bool isAdmDB = true;
        int rowCount = 0;
        string senderName = "";
        string senderEmail = "";
        IDataReader idr = Utils.GetReader(isAdmDB, "st_ContactDetails",
                    Utils.GetSessionString("Contact_ID"));
        if (idr.Read())
        {
            senderName = idr["CONTACT_NAME"].ToString();
            senderEmail = idr["EMAIL"].ToString();
        }
        idr.Close();

        idr = Utils.GetReader(isAdmDB, "st_ContactDetailsEmail",
                    tbInviteCochrane.Text);
        while (idr.Read())
        {
            // check that there is only one account with this email address. Old ER3 accounts may be an issue.
            rowCount += 1;
        }
        idr.Close();
        if (rowCount > 1)
        {
            lblInviteMsgCochrane.Text = "There is more than one account with this email address. Please contact EPPI-Reviewer 4 support staff at EPPISupport@ucl.ac.uk.";
            lblInviteMsgCochrane.Visible = true;
        }
        else if (rowCount < 1)
        {
            lblInviteMsgCochrane.Text = "There is no match in the EPPI-Reviewer 4 database for this email address";
            lblInviteMsgCochrane.Visible = true;
        }
        else
        {
            //one last check. is this user already in the review
            bool userAlreadyThere = false;
            for (int i = 0; i < gvMembersOfReviewCochrane.Rows.Count; i++)
            {
                if (gvMembersOfReviewCochrane.Rows[i].Cells[2].Text == tbInviteCochrane.Text)
                {
                    userAlreadyThere = true;
                }
            }

            if (userAlreadyThere == false)
            {
                idr = Utils.GetReader(isAdmDB, "st_ContactDetailsEmail",
                        tbInviteCochrane.Text);
                if (idr.Read())
                {
                    // place the invitee in the review
                    Utils.ExecuteSP(isAdmDB, Server, "st_ReviewAddMember",
                        lblShareableReviewNumberCochrane.Text, idr["CONTACT_ID"].ToString());

                    // check that the account meets the new account requirements. 
                    // If it doesn't include this info in the email sent to the invitee.
                    bool accountConditionsMet = true;
                    string emailAccountMsg = "<b>Your EPPI-Reviewer 4 account details need updating</b>.<br>Please log into the EPPI-Reviewer 4.0 gateway at " +
                        //"\http://eppi.ioe.ac.uk/cms/er4 and update your username and password";

                    "<a href='http://eppi.ioe.ac.uk/cms/er4'>eppi.ioe.ac.uk/cms/er4</a> to update your details.";

                    //Regex passwordRegex = new Regex("^.*(?=.{8,})(?=.*\\d)(?=.*[a-z])(?=.*[A-Z]).*$");
                    //Match m = passwordRegex.Match(idr["PASSWORD"].ToString());
                    //if (!m.Success)
                    //{
                    //    accountConditionsMet = false;
                    //}

                    if (idr["USERNAME"].ToString().Length < 4)
                    {
                        accountConditionsMet = false;
                    }

                    if (accountConditionsMet == true)
                    {
                        emailAccountMsg = "";
                    }

                    // send out the email
                    string sendResult = Utils.InviteEmail(tbInviteCochrane.Text, idr["CONTACT_NAME"].ToString(), tbShareableReviewNameCochrane.Text, senderName,
                        senderEmail, emailAccountMsg);
                    lblInviteMsgCochrane.Text = sendResult;
                    lblInviteMsgCochrane.Visible = true;
                }
                idr.Close();
                // update table
                buildMembersOfReviewCochrane(lblShareableReviewNumberCochrane.Text);
            }
            else
            {
                lblInviteMsgCochrane.Text = "This account is already in the review.";
                lblInviteMsgCochrane.Visible = true;
            }
        }
    }

    private void buildMembersOfReviewCochrane(string reviewID)
    {
        bool isPersonLoggedInAReviewAdmin = false;
        string test;
        DateTime lastAccessed;
        DataTable dt = new DataTable();
        System.Data.DataRow newrow;

        dt.Columns.Add(new DataColumn("CONTACT_ID", typeof(string)));
        dt.Columns.Add(new DataColumn("CONTACT_NAME", typeof(string)));
        dt.Columns.Add(new DataColumn("EMAIL", typeof(string)));
        dt.Columns.Add(new DataColumn("LAST_LOGIN", typeof(string)));
        dt.Columns.Add(new DataColumn("IS_CODING_ONLY", typeof(string)));
        dt.Columns.Add(new DataColumn("IS_READ_ONLY", typeof(string)));
        dt.Columns.Add(new DataColumn("IS_ADMIN", typeof(string)));

        bool isAdmDB = true;
        IDataReader idr = Utils.GetReader(isAdmDB, "st_ReviewMembers_2",
            reviewID);
        while (idr.Read())
        {
            newrow = dt.NewRow();
            newrow["CONTACT_ID"] = idr["CONTACT_ID"].ToString();
            newrow["CONTACT_NAME"] = idr["CONTACT_NAME"].ToString();
            newrow["EMAIL"] = idr["EMAIL"].ToString();


            if ((idr["LAST_LOGIN"].ToString() == null) || (idr["LAST_LOGIN"].ToString() == ""))
            {
                newrow["LAST_LOGIN"] = "Never";
            }
            else
            {
                lastAccessed = Convert.ToDateTime(idr["LAST_LOGIN"].ToString());
                newrow["LAST_LOGIN"] = lastAccessed.ToString("dd MMM yyyy HH:mm");
                //newrow["LAST_LOGIN"] = idr["LAST_LOGIN"].ToString();
            }

            if (idr["IS_CODING_ONLY"].ToString() == "True")
                newrow["IS_CODING_ONLY"] = "True";
            if (idr["IS_READ_ONLY"].ToString() == "True")
                newrow["IS_READ_ONLY"] = "True";
            if (idr["IS_ADMIN"].ToString() == "True")
                newrow["IS_ADMIN"] = "True";
            dt.Rows.Add(newrow);


            test = idr["CONTACT_ID"].ToString();
            test = Utils.GetSessionString("Contact_ID");
            test = idr["IS_ADMIN"].ToString();
            if (isPersonLoggedInAReviewAdmin != true)
            {
                if ((idr["CONTACT_ID"].ToString() == Utils.GetSessionString("Contact_ID")) &&
                                (idr["IS_ADMIN"].ToString() == "True"))
                {
                    // this is the person logged in and they are a review admin
                    isPersonLoggedInAReviewAdmin = true;

                }
            }

            /*
            if (idr["IS_ADMIN"].ToString() == "")
                isReviewAdmin = false;
            if ((idr["CONTACT_ID"].ToString() == Utils.GetSessionString("Contact_ID")) &&
                            (idr["IS_ADMIN"].ToString() == "True"))
                isReviewAdmin = true;
             */

        }
        idr.Close();

        gvMembersOfReviewCochrane.DataSource = dt;
        gvMembersOfReviewCochrane.DataBind();



        if (isPersonLoggedInAReviewAdmin == false)
        {
            cmdSaveShareableReviewCochrane.Visible = false;
            lbInviteReviewerCochrane.Visible = false;
        }
        else
        {
            cmdSaveShareableReviewCochrane.Visible = true;
            lbInviteReviewerCochrane.Visible = true;
        }


        // loop through gvMembersOfReview and set checkboxes
        for (int i = 0; i < dt.Rows.Count; i++)
        {
            GridViewRow row = gvMembersOfReviewCochrane.Rows[i];
            CheckBox cb = ((CheckBox)row.FindControl("cbCodingOnlyCochrane"));
            if (isPersonLoggedInAReviewAdmin == false)
                cb.Enabled = false;
            if (dt.Rows[i]["IS_CODING_ONLY"].ToString() == "True")
            {
                cb.Checked = true;
            }
        }
        for (int i = 0; i < dt.Rows.Count; i++)
        {

            GridViewRow row = gvMembersOfReviewCochrane.Rows[i];
            CheckBox cb = ((CheckBox)row.FindControl("cbReadOnlyCochrane"));
            if (isPersonLoggedInAReviewAdmin == false)
                cb.Enabled = false;
            if (dt.Rows[i]["IS_READ_ONLY"].ToString() == "True")
            {
                cb.Checked = true;
            }
            
        }
        for (int i = 0; i < dt.Rows.Count; i++)
        {
            GridViewRow row = gvMembersOfReviewCochrane.Rows[i];
            CheckBox cb = ((CheckBox)row.FindControl("cbReviewAdminCochrane"));
            if (isPersonLoggedInAReviewAdmin == false)
                cb.Enabled = false;
            if (dt.Rows[i]["IS_ADMIN"].ToString() == "True")
            {
                cb.Checked = true;
            }
        }
    }


    protected void cbCodingOnlyCochrane_CheckedChanged(object sender, EventArgs e)
    {
        ContentPlaceHolder mpContentPlaceHolder;
        mpContentPlaceHolder = (ContentPlaceHolder)Master.FindControl("ContentPlaceHolder1");
        if (mpContentPlaceHolder != null)
        {
            CheckBox chkbox = (CheckBox)sender;
            GridViewRow row = (GridViewRow)chkbox.Parent.Parent;
            string ContactID = (string)gvMembersOfReviewCochrane.DataKeys[row.RowIndex].Value.ToString();
            bool isChecked = chkbox.Checked;
            //update the role for this user
            bool isAdmDB = true;
            Utils.ExecuteSP(isAdmDB, Server, "st_ReviewRoleCodingOnlyUpdate", lblShareableReviewNumberCochrane.Text, ContactID, isChecked);
            buildMembersOfReviewCochrane(lblShareableReviewNumberCochrane.Text);
        }
    }
    protected void cbReadOnlyCochrane_CheckedChanged(object sender, EventArgs e)
    {
        ContentPlaceHolder mpContentPlaceHolder;
        mpContentPlaceHolder = (ContentPlaceHolder)Master.FindControl("ContentPlaceHolder1");
        if (mpContentPlaceHolder != null)
        {
            CheckBox chkbox = (CheckBox)sender;
            GridViewRow row = (GridViewRow)chkbox.Parent.Parent;
            string ContactID = (string)gvMembersOfReviewCochrane.DataKeys[row.RowIndex].Value.ToString();
            bool isChecked = chkbox.Checked;
            //update the role for this user
            bool isAdmDB = true;
            Utils.ExecuteSP(isAdmDB, Server, "st_ReviewRoleReadOnlyUpdate", lblShareableReviewNumberCochrane.Text, ContactID, isChecked);
            buildMembersOfReviewCochrane(lblShareableReviewNumberCochrane.Text);
        }
    }
    protected void cbReviewAdminCochrane_CheckedChanged(object sender, EventArgs e)
    {
        ContentPlaceHolder mpContentPlaceHolder;
        mpContentPlaceHolder = (ContentPlaceHolder)Master.FindControl("ContentPlaceHolder1");
        if (mpContentPlaceHolder != null)
        {
            CheckBox chkbox = (CheckBox)sender;
            GridViewRow row = (GridViewRow)chkbox.Parent.Parent;
            string ContactID = (string)gvMembersOfReviewCochrane.DataKeys[row.RowIndex].Value.ToString();
            bool isChecked = chkbox.Checked;
            //update the role for this user
            bool isAdmDB = true;
            Utils.ExecuteSP(isAdmDB, Server, "st_ReviewRoleIsAdminUpdate", lblShareableReviewNumberCochrane.Text, ContactID, isChecked);
            buildMembersOfReviewCochrane(lblShareableReviewNumberCochrane.Text);
        }
    }
    protected void lbInviteReviewerCochrane_Click(object sender, EventArgs e)
    {
        pnlInviteReviewerCochrane.Visible = true;
    }
    protected void gvMembersOfReviewCochrane_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        bool isAdmDB = false;
        int index = Convert.ToInt32(e.CommandArgument);
        string contactID = (string)gvMembersOfReviewCochrane.DataKeys[index].Value;
        switch (e.CommandName)
        {
            case "REMOVE":
                isAdmDB = true;
                Utils.ExecuteSP(isAdmDB, Server, "st_ReviewRemoveMember_1",
                    lblShareableReviewNumberCochrane.Text, contactID);
                pnlReviewDetailsCochrane.Visible = false;
                buildReviewGridCochraneProspective();
                break;

            default:
                break;
        }
    }
    protected void gvMembersOfReviewCochrane_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            
            bool isReviewAdmin = false;
            bool isCodingOnly = false;
            bool isAdmDB = true;
            IDataReader idr = Utils.GetReader(isAdmDB, "st_ContactReviewRole",
            e.Row.Cells[0].Text, lblShareableReviewNumberCochrane.Text);
            while (idr.Read())
            {
                if (idr["ROLE_NAME"].ToString() == "AdminUser")
                    isReviewAdmin = true;
                if (idr["ROLE_NAME"].ToString() == "CodingOnly")
                    isCodingOnly = true;
            }
            idr.Close();
            
            
            

            CheckBox cbReviewAdmin = (CheckBox)(e.Row.FindControl("cbReviewAdminCochrane"));
            CheckBox cbCodingOnly = (CheckBox)(e.Row.FindControl("cbCodingOnlyCochrane"));
            LinkButton lbRemove = (LinkButton)(e.Row.Cells[7].Controls[0]);
            
            // an admin can remove eveyone except the review owner
            // a normal review only remove themselves

            if (e.Row.Cells[0].Text == lblFID.Text) // are you the review owner?
            {
                lbRemove.Enabled = false; // this is the review owner so they can't remove themselves
            }
            else
            {
                if (isReviewAdmin == true) // are you an admin - then you can remove this person
                {
                    lbRemove.Enabled = true; // an admin so it is someone else so I can remove them
                    lbRemove.Attributes.Add("onclick", "if (confirm('Are you sure you want to remove yourself from this review?') == false) return false;");
                }
                else // not an admin so you can only remove this person if it is you
                {
                    if (e.Row.Cells[0].Text == Utils.GetSessionString("Contact_ID"))
                    {
                        lbRemove.Enabled = true; // this is me so I can remove myself
                        lbRemove.Attributes.Add("onclick", "if (confirm('Are you sure you want to remove yourself from this review?') == false) return false;");
                    }

                }
            }
        }
    }


    private void buildReviewGridCochraneFull()
    {
        DateTime dayExpires;
        DateTime today = DateTime.Today;
        DataTable dt = new DataTable();
        System.Data.DataRow newrow;
        string test = "aa";
        DateTime lastAccessed;
        DateTime dateCreated;

        dt.Columns.Add(new DataColumn("REVIEW_ID", typeof(string)));
        dt.Columns.Add(new DataColumn("REVIEW_NAME", typeof(string)));
        dt.Columns.Add(new DataColumn("DATE_CREATED", typeof(string)));
        dt.Columns.Add(new DataColumn("LAST_LOGIN", typeof(string)));
        dt.Columns.Add(new DataColumn("ARCHIE_ID", typeof(string)));
        bool isAdmDB = true;
        IDataReader idr = Utils.GetReader(isAdmDB, "st_ContactReviewsArchieFull",
            Utils.GetSessionString("Contact_ID"));
        while (idr.Read())
        {
            newrow = dt.NewRow();
            newrow["REVIEW_ID"] = idr["REVIEW_ID"].ToString();
            newrow["REVIEW_NAME"] = idr["REVIEW_NAME"].ToString();


            if ((idr["DATE_CREATED"].ToString() == null) || (idr["DATE_CREATED"].ToString() == ""))
            {
                newrow["DATE_CREATED"] = "N/A";
            }
            else
            {
                dateCreated = Convert.ToDateTime(idr["DATE_CREATED"].ToString());
                newrow["DATE_CREATED"] = dateCreated.ToString("dd MMM yyyy");
                //newrow["DATE_CREATED"] = idr["DATE_CREATED"].ToString();
            }


            if (idr["ARCHIE_ID"].ToString() == "prospective_______")
                newrow["ARCHIE_ID"] = "Prospective";
            else
                newrow["ARCHIE_ID"] = idr["ARCHIE_ID"].ToString();


            if ((idr["CREATED"].ToString() == null) || (idr["CREATED"].ToString() == ""))
            {
                newrow["LAST_LOGIN"] = "N/A";
            }
            else
            {
                lastAccessed = Convert.ToDateTime(idr["CREATED"].ToString());
                newrow["LAST_LOGIN"] = lastAccessed.ToString("dd MMM yyyy HH:mm");
                //newrow["LAST_LOGIN"] = idr["CREATED"].ToString();
            }


            dt.Rows.Add(newrow);
        }
        idr.Close();

        gvReviewCochraneFull.DataSource = dt;
        gvReviewCochraneFull.DataBind();

        if (dt.Rows.Count > 0)
            pnlCochraneReviewsFull.Visible = true;

        if (dt.Rows.Count == 0)
            lblShareableReviewsCochraneFull.Visible = true;
        else
        {
            for (int i = 0; i < gvReviewCochraneFull.Rows.Count; i++)
            {
                if (gvReviewCochraneFull.Rows[i].Cells[1].Text.Contains("Edit name"))
                {
                    gvReviewCochraneFull.Rows[i].Cells[1].BackColor = System.Drawing.Color.PaleGreen;
                }
                if (gvReviewCochraneFull.Rows[i].Cells[1].Text.Contains("Not activated"))
                {
                    gvReviewCochraneFull.Rows[i].Cells[1].BackColor = System.Drawing.Color.Yellow;
                }
                if (gvReviewCochraneFull.Rows[i].Cells[4].Text.Contains("Not activated"))
                {
                    gvReviewCochraneFull.Rows[i].Cells[4].BackColor = System.Drawing.Color.Yellow;
                }
                if (gvReviewCochraneFull.Rows[i].Cells[4].Text.Contains("Expired"))
                {
                    gvReviewCochraneFull.Rows[i].Cells[4].BackColor = System.Drawing.Color.Pink;
                    gvReviewCochraneFull.Rows[i].Cells[4].Font.Bold = true;
                }
                if (gvReviewCochraneFull.Rows[i].Cells[4].Text.Contains(" In Site License '"))
                {
                    gvReviewCochraneFull.Rows[i].Cells[4].BackColor = System.Drawing.Color.Aquamarine;
                }
            }
        }
    }


    protected void gvReviewCochraneFull_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        int index = Convert.ToInt32(e.CommandArgument);
        string ReviewID = (string)gvReviewCochraneFull.DataKeys[index].Value;
        bool prospective = false;
        bool isReviewAdmin = false;
        if (gvReviewCochraneFull.Rows[index].Cells[5].Text == "Prospective")
            prospective = true;
        switch (e.CommandName)
        {
            case "EDITROW":
                pnlEditShareableReviewCochraneFull.Visible = true;
                pnlBritLibCodesFullCochrane.Visible = false;
                lblPSFullCochraneReviewEnable.Visible = false;
                rblPSFullCochraneReviewEnable.Visible = false;
                lblShareableReviewNumberCochraneFull.Text = ReviewID;
                DateTime lastAccessed;
                bool isAdmDB = true;
                IDataReader idr = Utils.GetReader(isAdmDB, "st_ReviewDetails",
                    ReviewID);
                if (idr.Read())
                {
                    tbShareableReviewNameCochraneFull.Text = idr["REVIEW_NAME"].ToString();
                    if (Utils.GetSessionString("EnablePSEnabler") == "True")
                    {
                        lblPSFullCochraneReviewEnable.Visible = true;
                        rblPSFullCochraneReviewEnable.Visible = true;
                        rblPSFullCochraneReviewEnable.SelectedValue = idr["SHOW_SCREENING"].ToString();
                    }

                    DataTable dt = new DataTable();
                    System.Data.DataRow newrow;

                    dt.Columns.Add(new DataColumn("CONTACT_ID", typeof(string)));
                    dt.Columns.Add(new DataColumn("CONTACT_NAME", typeof(string)));
                    dt.Columns.Add(new DataColumn("EMAIL", typeof(string)));
                    dt.Columns.Add(new DataColumn("LAST_LOGIN", typeof(string)));
                    dt.Columns.Add(new DataColumn("IS_CODING_ONLY", typeof(string)));
                    dt.Columns.Add(new DataColumn("IS_READ_ONLY", typeof(string)));
                    dt.Columns.Add(new DataColumn("IS_ADMIN", typeof(string)));
                    string test = "A";
                    isAdmDB = true;
                    IDataReader idr1 = Utils.GetReader(isAdmDB, "st_ReviewMembers_2",
                        lblShareableReviewNumberCochraneFull.Text);
                    while (idr1.Read())
                    {
                        newrow = dt.NewRow();
                        newrow["CONTACT_ID"] = idr1["CONTACT_ID"].ToString();
                        newrow["CONTACT_NAME"] = idr1["CONTACT_NAME"].ToString();
                        test = idr1["CONTACT_NAME"].ToString();
                        newrow["EMAIL"] = idr1["EMAIL"].ToString();

                        if ((idr1["LAST_LOGIN"].ToString() == null) || (idr1["LAST_LOGIN"].ToString() == ""))
                        {
                            newrow["LAST_LOGIN"] = "Never";
                        }
                        else
                        {                            
                            lastAccessed = Convert.ToDateTime(idr["LAST_LOGIN"].ToString());
                            newrow["LAST_LOGIN"] = lastAccessed.ToString("dd MMM yyyy HH:mm");
                            //newrow["LAST_LOGIN"] = idr1["LAST_LOGIN"].ToString();
                        }

                        test = idr1["IS_CODING_ONLY"].ToString();
                        if (idr1["IS_CODING_ONLY"].ToString() == "True")
                            newrow["IS_CODING_ONLY"] = "True";
                        test = idr1["IS_READ_ONLY"].ToString();
                        if (idr1["IS_READ_ONLY"].ToString() == "True")
                            newrow["IS_READ_ONLY"] = "True";
                        test = idr1["IS_ADMIN"].ToString();
                        if (idr1["IS_ADMIN"].ToString() == "True")
                            newrow["IS_ADMIN"] = "True";

                        if ((idr1["CONTACT_ID"].ToString() == Utils.GetSessionString("Contact_ID")) &&
                            (idr1["IS_ADMIN"].ToString() == "True"))
                            isReviewAdmin = true;

                        dt.Rows.Add(newrow);
                    }
                    idr1.Close();

                    gvMembersOfReviewCochraneFull.DataSource = dt;
                    gvMembersOfReviewCochraneFull.DataBind();

                    pnlReviewDetailsCochraneFull.Visible = true;

                    if (isReviewAdmin == false)
                    {
                        cmdSaveShareableReviewCochraneFull.Visible = false;
                        lbInviteReviewerCochraneFull.Visible = false;
                    }
                    else
                    {
                        cmdSaveShareableReviewCochraneFull.Visible = true;
                        //lbInviteReviewerCochraneFull.Visible = true;
                    }


                    // loop through gvMembersOfReview and set checkboxes
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        GridViewRow row = gvMembersOfReviewCochraneFull.Rows[i];
                        CheckBox cb = ((CheckBox)row.FindControl("cbCodingOnlyCochraneFull"));
                        if (isReviewAdmin == false)
                            cb.Enabled = false;
                        if (dt.Rows[i]["IS_CODING_ONLY"].ToString() == "True")
                        {
                            cb.Checked = true;
                        }
                    }
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        GridViewRow row = gvMembersOfReviewCochraneFull.Rows[i];
                        CheckBox cb = ((CheckBox)row.FindControl("cbReadOnlyCochraneFull"));
                        if (isReviewAdmin == false)
                            cb.Enabled = false;
                        if (dt.Rows[i]["IS_READ_ONLY"].ToString() == "True")
                        {
                            cb.Checked = true;
                        }
                    }
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        GridViewRow row = gvMembersOfReviewCochraneFull.Rows[i];
                        CheckBox cb = ((CheckBox)row.FindControl("cbReviewAdminCochraneFull"));
                        if (isReviewAdmin == false)
                            cb.Enabled = false;
                        if (dt.Rows[i]["IS_ADMIN"].ToString() == "True")
                        {
                            cb.Checked = true;
                        }
                    }
                }
                idr.Close();
                break;

            default:
                break;
        }
    }
    protected void cmdSaveShareableReviewCochrane_Click(object sender, EventArgs e)
    {
        bool isAdmDB = true;
        Utils.ExecuteSP(isAdmDB, Server, "st_ReviewEditName",
             lblShareableReviewNumberCochrane.Text, tbShareableReviewNameCochrane.Text);

        pnlEditShareableReviewCochrane.Visible = false;
        buildReviewGridCochraneProspective();
    }
    protected void lbCancelReviewDetailsEdit0_Click(object sender, EventArgs e)
    {
        pnlReviewDetailsCochrane.Visible = false;
        Server.Transfer("Summary.aspx");
    }
    protected void cmdSaveShareableReviewCochraneFull_Click(object sender, EventArgs e)
    {
        bool isAdmDB = true;
        Utils.ExecuteSP(isAdmDB, Server, "st_ReviewEditName",
             lblShareableReviewNumberCochraneFull.Text, tbShareableReviewNameCochraneFull.Text);

        pnlEditShareableReviewCochraneFull.Visible = false;
        buildReviewGridCochraneFull();
    }
    protected void lbCancelReviewDetailsEdit2_Click(object sender, EventArgs e)
    {
        pnlReviewDetailsCochraneFull.Visible = false;
        Server.Transfer("Summary.aspx");
    }
    protected void lbInviteReviewerCochraneFull_Click(object sender, EventArgs e)
    {

    }
    protected void gvMembersOfReviewCochraneFull_RowCommand(object sender, GridViewCommandEventArgs e)
    {

    }
    protected void gvMembersOfReviewCochraneFull_RowDataBound(object sender, GridViewRowEventArgs e)
    {

    }
    protected void cmdInviteCochraneFull_Click(object sender, EventArgs e)
    {

    }
    protected void cbCodingOnlyCochraneFull_CheckedChanged(object sender, EventArgs e)
    {
        ContentPlaceHolder mpContentPlaceHolder;
        mpContentPlaceHolder = (ContentPlaceHolder)Master.FindControl("ContentPlaceHolder1");
        if (mpContentPlaceHolder != null)
        {
            CheckBox chkbox = (CheckBox)sender;
            GridViewRow row = (GridViewRow)chkbox.Parent.Parent;
            string ContactID = (string)gvMembersOfReviewCochraneFull.DataKeys[row.RowIndex].Value.ToString();
            bool isChecked = chkbox.Checked;
            //update the role for this user
            bool isAdmDB = true;
            Utils.ExecuteSP(isAdmDB, Server, "st_ReviewRoleCodingOnlyUpdate", lblShareableReviewNumberCochraneFull.Text, ContactID, isChecked);
            buildMembersOfReviewCochraneFull(lblShareableReviewNumberCochraneFull.Text);
        }
    }
    protected void cbReadOnlyCochraneFull_CheckedChanged(object sender, EventArgs e)
    {
        ContentPlaceHolder mpContentPlaceHolder;
        mpContentPlaceHolder = (ContentPlaceHolder)Master.FindControl("ContentPlaceHolder1");
        if (mpContentPlaceHolder != null)
        {
            CheckBox chkbox = (CheckBox)sender;
            GridViewRow row = (GridViewRow)chkbox.Parent.Parent;
            string ContactID = (string)gvMembersOfReviewCochraneFull.DataKeys[row.RowIndex].Value.ToString();
            bool isChecked = chkbox.Checked;
            //update the role for this user
            bool isAdmDB = true;
            Utils.ExecuteSP(isAdmDB, Server, "st_ReviewRoleReadOnlyUpdate", lblShareableReviewNumberCochraneFull.Text, ContactID, isChecked);
            buildMembersOfReviewCochraneFull(lblShareableReviewNumberCochraneFull.Text);
        }
    }
    protected void cbReviewAdminCochraneFull_CheckedChanged(object sender, EventArgs e)
    {
        ContentPlaceHolder mpContentPlaceHolder;
        mpContentPlaceHolder = (ContentPlaceHolder)Master.FindControl("ContentPlaceHolder1");
        if (mpContentPlaceHolder != null)
        {
            CheckBox chkbox = (CheckBox)sender;
            GridViewRow row = (GridViewRow)chkbox.Parent.Parent;
            string ContactID = (string)gvMembersOfReviewCochraneFull.DataKeys[row.RowIndex].Value.ToString();
            bool isChecked = chkbox.Checked;
            //update the role for this user
            bool isAdmDB = true;
            Utils.ExecuteSP(isAdmDB, Server, "st_ReviewRoleIsAdminUpdate", lblShareableReviewNumberCochraneFull.Text, ContactID, isChecked);
            buildMembersOfReviewCochraneFull(lblShareableReviewNumberCochraneFull.Text);
        }
    }

    private void buildMembersOfReviewCochraneFull(string reviewID)
    {
        string test = "abc";
        DateTime lastAccessed;
        bool isPersonLoggedInAReviewAdmin = false;
        DataTable dt = new DataTable();
        System.Data.DataRow newrow;

        dt.Columns.Add(new DataColumn("CONTACT_ID", typeof(string)));
        dt.Columns.Add(new DataColumn("CONTACT_NAME", typeof(string)));
        dt.Columns.Add(new DataColumn("EMAIL", typeof(string)));
        dt.Columns.Add(new DataColumn("LAST_LOGIN", typeof(string)));
        dt.Columns.Add(new DataColumn("IS_CODING_ONLY", typeof(string)));
        dt.Columns.Add(new DataColumn("IS_READ_ONLY", typeof(string)));
        dt.Columns.Add(new DataColumn("IS_ADMIN", typeof(string)));

        bool isAdmDB = true;
        IDataReader idr = Utils.GetReader(isAdmDB, "st_ReviewMembers_2",
            reviewID);
        while (idr.Read())
        {
            newrow = dt.NewRow();
            newrow["CONTACT_ID"] = idr["CONTACT_ID"].ToString();
            newrow["CONTACT_NAME"] = idr["CONTACT_NAME"].ToString();
            newrow["EMAIL"] = idr["EMAIL"].ToString();

            if ((idr["LAST_LOGIN"].ToString() == null) || (idr["LAST_LOGIN"].ToString() == ""))
            {
                newrow["LAST_LOGIN"] = "Never";
            }
            else
            {
                lastAccessed = Convert.ToDateTime(idr["LAST_LOGIN"].ToString());
                newrow["LAST_LOGIN"] = lastAccessed.ToString("dd MMM yyyy HH:mm");
                //newrow["LAST_LOGIN"] = idr["LAST_LOGIN"].ToString();
            }

            if (idr["IS_CODING_ONLY"].ToString() == "True")
                newrow["IS_CODING_ONLY"] = "True";
            if (idr["IS_READ_ONLY"].ToString() == "True")
                newrow["IS_READ_ONLY"] = "True";
            if (idr["IS_ADMIN"].ToString() == "True")
                newrow["IS_ADMIN"] = "True";
            dt.Rows.Add(newrow);

            test = idr["CONTACT_ID"].ToString();
            test = Utils.GetSessionString("Contact_ID");
            test = idr["IS_ADMIN"].ToString();
            if (isPersonLoggedInAReviewAdmin != true)
            {
                if ((idr["CONTACT_ID"].ToString() == Utils.GetSessionString("Contact_ID")) &&
                                (idr["IS_ADMIN"].ToString() == "True"))
                {
                    // this is the person logged in and they are a review admin
                    isPersonLoggedInAReviewAdmin = true;

                }
            }
        }
        idr.Close();

        gvMembersOfReviewCochraneFull.DataSource = dt;
        gvMembersOfReviewCochraneFull.DataBind();


        if (isPersonLoggedInAReviewAdmin == false)
        {
            cmdSaveShareableReviewCochraneFull.Visible = false;
        }
        else
        {
            cmdSaveShareableReviewCochraneFull.Visible = true;
        }


        // loop through gvMembersOfReview and set checkboxes
        for (int i = 0; i < dt.Rows.Count; i++)
        {
            GridViewRow row = gvMembersOfReviewCochraneFull.Rows[i];
            CheckBox cb = ((CheckBox)row.FindControl("cbCodingOnlyCochraneFull"));
            if (isPersonLoggedInAReviewAdmin == false)
                cb.Enabled = false;
            if (dt.Rows[i]["IS_CODING_ONLY"].ToString() == "True")
            {
                cb.Checked = true;
            }
        }
        for (int i = 0; i < dt.Rows.Count; i++)
        {
            GridViewRow row = gvMembersOfReviewCochraneFull.Rows[i];
            CheckBox cb = ((CheckBox)row.FindControl("cbReadOnlyCochraneFull"));
            if (isPersonLoggedInAReviewAdmin == false)
                cb.Enabled = false;
            if (dt.Rows[i]["IS_READ_ONLY"].ToString() == "True")
            {
                cb.Checked = true;
            }
        }
        for (int i = 0; i < dt.Rows.Count; i++)
        {
            GridViewRow row = gvMembersOfReviewCochraneFull.Rows[i];
            CheckBox cb = ((CheckBox)row.FindControl("cbReviewAdminCochraneFull"));
            if (isPersonLoggedInAReviewAdmin == false)
                cb.Enabled = false;
            if (dt.Rows[i]["IS_ADMIN"].ToString() == "True")
            {
                cb.Checked = true;
            }
        }
    }




    protected void lbBritLibCodesShared_Click(object sender, EventArgs e)
    {
        pnlBritLibCodesShared.Visible = true;
        bool isAdmDB = true;
        IDataReader idr1 = Utils.GetReader(isAdmDB, "st_BritishLibraryValuesGetAll",
            lblShareableReviewNumber.Text);
        if (idr1.Read())
        {
            tbLPC_ACC_Share.Text = idr1["BL_ACCOUNT_CODE"].ToString();
            tbLPC_AUT_Share.Text = idr1["BL_AUTH_CODE"].ToString();
            tbLPC_TX_Share.Text = idr1["BL_TX"].ToString();

            tbCCC_ACC_Share.Text = idr1["BL_CC_ACCOUNT_CODE"].ToString();
            tbCCC_AUT_Share.Text = idr1["BL_CC_AUTH_CODE"].ToString();
            tbCCC_TX_Share.Text = idr1["BL_CC_TX"].ToString();
        }
        idr1.Close();
    }
    protected void lbCancelBLShared_Click(object sender, EventArgs e)
    {
        pnlBritLibCodesShared.Visible = false;
    }
    protected void lbSaveBritLibLPCodesShared_Click(object sender, EventArgs e)
    {
        bool isAdmDB = true;
        Utils.ExecuteSP(isAdmDB, Server, "st_BritishLibraryValuesSet", lblShareableReviewNumber.Text,
            tbLPC_ACC_Share.Text, tbLPC_AUT_Share.Text, tbLPC_TX_Share.Text);
    }
    protected void lbSaveBritLibCCCodesShared_Click(object sender, EventArgs e)
    {
        bool isAdmDB = true;
        Utils.ExecuteSP(isAdmDB, Server, "st_BritishLibraryCCValuesSet", lblShareableReviewNumber.Text,
            tbCCC_ACC_Share.Text, tbCCC_AUT_Share.Text, tbCCC_TX_Share.Text);
    }


    protected void lbBritLibCodesNonShared_Click(object sender, EventArgs e)
    {
        pnlBritLibCodesNonShared.Visible = true;
        bool isAdmDB = true;
        IDataReader idr1 = Utils.GetReader(isAdmDB, "st_BritishLibraryValuesGetAll",
            lblNonShareableReviewNumber.Text);
        if (idr1.Read())
        {
            tbLPC_ACC_NonShare.Text = idr1["BL_ACCOUNT_CODE"].ToString();
            tbLPC_AUT_NonShare.Text = idr1["BL_AUTH_CODE"].ToString();
            tbLPC_TX_NonShare.Text = idr1["BL_TX"].ToString();

            tbCCC_ACC_NonShare.Text = idr1["BL_CC_ACCOUNT_CODE"].ToString();
            tbCCC_AUT_NonShare.Text = idr1["BL_CC_AUTH_CODE"].ToString();
            tbCCC_TX_NonShare.Text = idr1["BL_CC_TX"].ToString();
        }
        idr1.Close();
    }
    protected void lbCancelBLNonShared_Click(object sender, EventArgs e)
    {
        pnlBritLibCodesNonShared.Visible = false;
    }
    protected void lbSaveBritLibLPCodesNonShared_Click(object sender, EventArgs e)
    {
        bool isAdmDB = true;
        Utils.ExecuteSP(isAdmDB, Server, "st_BritishLibraryValuesSet", lblNonShareableReviewNumber.Text,
            tbLPC_ACC_NonShare.Text, tbLPC_AUT_NonShare.Text, tbLPC_TX_NonShare.Text);
    }
    protected void lbSaveBritLibCCCodesNonShared_Click(object sender, EventArgs e)
    {
        bool isAdmDB = true;
        Utils.ExecuteSP(isAdmDB, Server, "st_BritishLibraryCCValuesSet", lblNonShareableReviewNumber.Text,
            tbCCC_ACC_NonShare.Text, tbCCC_AUT_NonShare.Text, tbCCC_TX_NonShare.Text);
    }




    protected void lbBritLibCodesProCochrane_Click(object sender, EventArgs e)
    {
        pnlBritLibCodesProCochrane.Visible = true;
        bool isAdmDB = true;
        IDataReader idr1 = Utils.GetReader(isAdmDB, "st_BritishLibraryValuesGetAll",
            lblShareableReviewNumberCochrane.Text);
        if (idr1.Read())
        {
            tbLPC_ACC_ProCochrane.Text = idr1["BL_ACCOUNT_CODE"].ToString();
            tbLPC_AUT_ProCochrane.Text = idr1["BL_AUTH_CODE"].ToString();
            tbLPC_TX_ProCochrane.Text = idr1["BL_TX"].ToString();

            tbCCC_ACC_ProCochrane.Text = idr1["BL_CC_ACCOUNT_CODE"].ToString();
            tbCCC_AUT_ProCochrane.Text = idr1["BL_CC_AUTH_CODE"].ToString();
            tbCCC_TX_ProCochrane.Text = idr1["BL_CC_TX"].ToString();
        }
        idr1.Close();
    }
    protected void lbCancelBLProCochrane_Click(object sender, EventArgs e)
    {
        pnlBritLibCodesProCochrane.Visible = false;
    }
    protected void lbSaveBritLibLPCodesProCochrane_Click(object sender, EventArgs e)
    {
        bool isAdmDB = true;
        Utils.ExecuteSP(isAdmDB, Server, "st_BritishLibraryValuesSet", lblShareableReviewNumberCochrane.Text,
            tbLPC_ACC_ProCochrane.Text, tbLPC_AUT_ProCochrane.Text, tbLPC_TX_ProCochrane.Text);
    }
    protected void lbSaveBritLibCCCodesProCochrane_Click(object sender, EventArgs e)
    {
        bool isAdmDB = true;
        Utils.ExecuteSP(isAdmDB, Server, "st_BritishLibraryCCValuesSet", lblShareableReviewNumberCochrane.Text,
            tbCCC_ACC_ProCochrane.Text, tbCCC_AUT_ProCochrane.Text, tbCCC_TX_ProCochrane.Text);
    }




    protected void lbBritLibCodesFullCochrane_Click(object sender, EventArgs e)
    {
        pnlBritLibCodesFullCochrane.Visible = true;
        bool isAdmDB = true;
        IDataReader idr1 = Utils.GetReader(isAdmDB, "st_BritishLibraryValuesGetAll",
            lblShareableReviewNumberCochraneFull.Text);
        if (idr1.Read())
        {
            tbLPC_ACC_FullCochrane.Text = idr1["BL_ACCOUNT_CODE"].ToString();
            tbLPC_AUT_FullCochrane.Text = idr1["BL_AUTH_CODE"].ToString();
            tbLPC_TX_FullCochrane.Text = idr1["BL_TX"].ToString();

            tbCCC_ACC_FullCochrane.Text = idr1["BL_CC_ACCOUNT_CODE"].ToString();
            tbCCC_AUT_FullCochrane.Text = idr1["BL_CC_AUTH_CODE"].ToString();
            tbCCC_TX_FullCochrane.Text = idr1["BL_CC_TX"].ToString();
        }
        idr1.Close();
    }
    protected void lbCancelBLFullCochrane_Click(object sender, EventArgs e)
    {
        pnlBritLibCodesFullCochrane.Visible = false;
    }
    protected void lbSaveBritLibLPCodesFullCochrane_Click(object sender, EventArgs e)
    {
        bool isAdmDB = true;
        Utils.ExecuteSP(isAdmDB, Server, "st_BritishLibraryValuesSet", lblShareableReviewNumberCochraneFull.Text,
            tbLPC_ACC_FullCochrane.Text, tbLPC_AUT_FullCochrane.Text, tbLPC_TX_FullCochrane.Text);
    }
    protected void lbSaveBritLibCCCodesFullCochrane_Click(object sender, EventArgs e)
    {
        bool isAdmDB = true;
        Utils.ExecuteSP(isAdmDB, Server, "st_BritishLibraryCCValuesSet", lblShareableReviewNumberCochraneFull.Text,
            tbCCC_ACC_FullCochrane.Text, tbCCC_AUT_FullCochrane.Text, tbCCC_TX_FullCochrane.Text);
    }

    protected void rblPSShareableEnable_SelectedIndexChanged(object sender, EventArgs e)
    {
        bool isAdmDB = true;
        Utils.ExecuteSP(isAdmDB, Server, "st_PriorityScreeningTurnOnOff",
                lblShareableReviewNumber.Text, "ShowScreening", rblPSShareableEnable.SelectedValue);
    }

    protected void rblPSNonShareableEnable_SelectedIndexChanged(object sender, EventArgs e)
    {
        bool isAdmDB = true;
        Utils.ExecuteSP(isAdmDB, Server, "st_PriorityScreeningTurnOnOff",
                lblNonShareableReviewNumber.Text, "ShowScreening", rblPSNonShareableEnable.SelectedValue);
    }

    protected void rblPSProsCochraneReviewEnable_SelectedIndexChanged(object sender, EventArgs e)
    {
        bool isAdmDB = true;
        Utils.ExecuteSP(isAdmDB, Server, "st_PriorityScreeningTurnOnOff",
                lblShareableReviewNumberCochrane.Text, "ShowScreening", rblPSProsCochraneReviewEnable.SelectedValue);
    }

    protected void rblPSFullCochraneReviewEnable_SelectedIndexChanged(object sender, EventArgs e)
    {
        bool isAdmDB = true;
        Utils.ExecuteSP(isAdmDB, Server, "st_PriorityScreeningTurnOnOff",
                lblShareableReviewNumberCochraneFull.Text, "ShowScreening", rblPSFullCochraneReviewEnable.SelectedValue);
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
                lblCreditPruchaseID.Text = creditPurchaseID;
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
                    newrow["TYPE"] = idr["tv_type_extended_name"].ToString();
                    newrow["ID"] = idr["tv_id_extended"].ToString();
                    newrow["NAME"] = idr["tv_name"].ToString();

                    dateExtended = Convert.ToDateTime(idr["tv_date_extended"].ToString());
                    newrow["DATE_EXTENDED"] = dateExtended.ToString("dd MMM yyyy");
                    //newrow["DATE_EXTENDED"] = idr["tv_date_extended"].ToString();

                    newrow["NUMBER_MONTHS"] = idr["tv_months_extended"].ToString();
                    newrow["COST"] = idr["tv_cost"].ToString();
                    dt.Rows.Add(newrow);

                }
                idr.Close();

                gvCreditHistroy.DataSource = dt;
                gvCreditHistroy.DataBind();

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
}
