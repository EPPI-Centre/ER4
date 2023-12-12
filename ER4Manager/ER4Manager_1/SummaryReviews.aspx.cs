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

public partial class SummaryReviews : System.Web.UI.Page
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
                    lbl.Text = "Summary of your reviews";
                }

                Telerik.Web.UI.RadTabStrip radTs = (Telerik.Web.UI.RadTabStrip)Master.FindControl("rtsMenu");
                if (radTs != null)
                {
                    radTs.SelectedIndex = 0;
                    radTs.Tabs[0].Tabs[2].Selected = true;
                    radTs.Tabs[0].Tabs[1].Visible = false;
                    radTs.Tabs[0].Tabs[4].Width = 500;
                }
                System.Web.UI.WebControls.Label lbl1 = (Label)Master.FindControl("lblHeadingText");
                if (lbl1 != null)
                {
                    lbl1.Text = "Summary of your reviews";
                }

                buildShareableReviewGrid(0);
                buildNonShareableReviewGrid(0);
                buildOtherShareableReviewGrid(0);
            }
            rblPSShareableEnable.Attributes.Add("onclick", "if (confirm('If you are turning on Priority Screening please check the user manual for more details on how to use it.') == false) return false;");
        }
        else
        {
            Server.Transfer("Error.aspx");
        }
    }

    private void buildShareableReviewGrid(int pageNumber)
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
        gvReview.PageIndex = pageNumber;
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

    private void buildNonShareableReviewGrid(int pageNumber)
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
        gvReviewNonShareable.PageIndex = pageNumber;
        gvReviewNonShareable.DataBind();

        if (dt.Rows.Count == 0)
            lblNonShareableReviews.Visible = true;
    }

    private void buildOtherShareableReviewGrid(int pageNumber)
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
        gvReviewShareableMember.PageIndex = pageNumber;
        gvReviewShareableMember.DataBind();

        if (dt.Rows.Count == 0)
            lblNonShareableReviewsMember.Visible = true;
    }




    protected void gvReviewNonShareable_RowCommand(object sender, GridViewCommandEventArgs e)
    {      
        switch (e.CommandName)
        {
            case "EDT":
                int index = Convert.ToInt32(e.CommandArgument);
                string ReviewID = (string)gvReviewNonShareable.DataKeys[index].Value;
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

        
        switch (e.CommandName)
        {
            case "EDT":
                int index = Convert.ToInt32(e.CommandArgument);
                string ReviewID = (string)gvReview.DataKeys[index].Value;
                buildMembersOfReview(ReviewID);

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
        buildNonShareableReviewGrid(0);
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
                cmdSaveShareableReview.Text = "Save";
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
            buildShareableReviewGrid(0);
        }
    }
    protected void lbInviteReviewer_Click(object sender, EventArgs e)
    {
        pnlInviteReviewer.Visible = true;
    }


    protected void gvReviewShareableMember_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        switch (e.CommandName)
        {
            case "REMOVE":
                int index = Convert.ToInt32(e.CommandArgument);
                string ReviewID = (string)gvReviewShareableMember.DataKeys[index].Value;
                bool isAdmDB = true;
                Utils.ExecuteSP(isAdmDB, Server, "st_ReviewRemoveMember_1",
                    ReviewID, Utils.GetSessionString("Contact_ID"));

                buildOtherShareableReviewGrid(0);
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
        switch (e.CommandName)
        {
            case "REMOVE":
                int index = Convert.ToInt32(e.CommandArgument);
                string ContactID = (string)gvMembersOfReview.DataKeys[index].Value;
                bool isAdmDB = true;
                Utils.ExecuteSP(isAdmDB, Server, "st_ReviewRemoveMember_1",
                    lblShareableReviewNumber.Text, ContactID);

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



            // loop through the table and set the role ddl 
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                GridViewRow row = gvMembersOfReview.Rows[i];
                DropDownList ddl = ((DropDownList)row.FindControl("ddlRole"));

                // doing this in a hierarchal way...
                if (dt.Rows[i]["IS_ADMIN"].ToString() == "True")
                {
                    ddl.SelectedValue = "1";
                }
                else if (dt.Rows[i]["IS_CODING_ONLY"].ToString() == "True")
                {
                    ddl.SelectedValue = "2";
                }
                else if (dt.Rows[i]["IS_READ_ONLY"].ToString() == "True")
                {
                    ddl.SelectedValue = "3";
                }
                else // regular reviewer
                {
                    ddl.SelectedValue = "4";
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
            lblInviteMsg.Text = "There is more than one account with this email address. Please contact EPPI Reviewer support staff at EPPISupport@ucl.ac.uk.";
            lblInviteMsg.Visible = true;
        }
        else if (rowCount < 1)
        {
            lblInviteMsg.Text = "There is no match in the EPPI Reviewer database for this email address";
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
                    string emailAccountMsg = "<b>Your EPPI Reviewer account details need updating</b>.<br>Please log into the EPPI Reviewer gateway at " +
                    //"\http://eppi.ioe.ac.uk/cms/er4 and update your username and password";

                    "<a href='https://eppi.ioe.ac.uk/cms/er4'>eppi.ioe.ac.uk/cms/er4</a> to update your details.";

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


    protected void lbCancelReviewDetailsEdit_Click(object sender, EventArgs e)
    {
        pnlReviewDetails.Visible = false;
        Server.Transfer("SummaryReviews.aspx");
    }
    protected void lbCancelNSReviewDetailsEdit_Click(object sender, EventArgs e)
    {
        pnlEditNonShareableReview.Visible = false;
    }
    protected void gvMembersOfReview_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            LinkButton lb = (LinkButton)(e.Row.Cells[5].Controls[0]);
            lb.Attributes.Add("onclick", "if (confirm('Are you sure you want to remove this user from this review?') == false) return false;");
        }
    }


    protected void ddlRole_SelectedIndexChanged(object sender, EventArgs e)
    {
        ContentPlaceHolder mpContentPlaceHolder;
        mpContentPlaceHolder = (ContentPlaceHolder)Master.FindControl("ContentPlaceHolder1");
        if (mpContentPlaceHolder != null)
        {
            DropDownList ddlRole = (DropDownList)sender;
            GridViewRow row = (GridViewRow)ddlRole.Parent.Parent;
            string ContactID = (string)gvMembersOfReview.DataKeys[row.RowIndex].Value.ToString();
            string roleNum = ddlRole.SelectedValue;

            string role = "RegularUser";
            switch (roleNum)
            {
                case "1":
                    role = "AdminUser";
                    break;
                case "2":
                    role = "Coding only";
                    break;
                case "3":
                    role = "ReadOnlyUser";
                    break;
                default:
                    role = "RegularUser";
                    break;
            }

            //update the role for this user
            bool isAdmDB = true;
            Utils.ExecuteSP(isAdmDB, Server, "st_ReviewRoleUpdateByContactID", lblShareableReviewNumber.Text, ContactID, role);
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

    protected void rblPSShareableEnable_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (cmdSaveShareableReview.Enabled == true)
        {
            bool isAdmDB = true;
            Utils.ExecuteSP(isAdmDB, Server, "st_PriorityScreeningTurnOnOff",
                    lblShareableReviewNumber.Text, "ShowScreening", rblPSShareableEnable.SelectedValue);
        }
    }

    protected void rblPSNonShareableEnable_SelectedIndexChanged(object sender, EventArgs e)
    {
        bool isAdmDB = true;
        Utils.ExecuteSP(isAdmDB, Server, "st_PriorityScreeningTurnOnOff",
                lblNonShareableReviewNumber.Text, "ShowScreening", rblPSNonShareableEnable.SelectedValue);
    }

    protected void gvReview_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {
        buildShareableReviewGrid(e.NewPageIndex);
    }

    protected void gvReviewNonShareable_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {
        buildNonShareableReviewGrid(e.NewPageIndex);
    }

    protected void gvReviewShareableMember_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {
        buildOtherShareableReviewGrid(e.NewPageIndex);
    }


    protected void gvReview_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        /*if (e.Row.RowType == DataControlRowType.Pager)
        {
            e.Row.Cells[0].Text = "Page " + (gvReview.PageIndex + 1) + " of " + gvReview.PageCount;
        }*/
    }

    protected void gvReviewShareableMember_PageIndexChanging1(object sender, GridViewPageEventArgs e)
    {
        buildOtherShareableReviewGrid(e.NewPageIndex);
    }
}