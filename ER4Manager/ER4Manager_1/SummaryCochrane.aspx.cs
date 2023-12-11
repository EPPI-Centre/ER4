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

public partial class SummaryCochrane : System.Web.UI.Page
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
                    lbl.Text = "Summary of your Cochrane reviews in EPPI-Reviewer";
                }

                Telerik.Web.UI.RadTabStrip radTs = (Telerik.Web.UI.RadTabStrip)Master.FindControl("rtsMenu");
                if (radTs != null)
                {
                    radTs.SelectedIndex = 0;
                    radTs.Tabs[0].Tabs[3].Selected = true;
                    radTs.Tabs[0].Tabs[1].Visible = false;
                    radTs.Tabs[0].Tabs[4].Width = 500;
                }
                System.Web.UI.WebControls.Label lbl1 = (Label)Master.FindControl("lblHeadingText");
                if (lbl1 != null)
                {
                    lbl1.Text = "Summary of your Cochrane reviews in EPPI-Reviewer";
                }
                buildReviewGridCochraneProspective();
                buildReviewGridCochraneFull();
            }
            Utils.SetSessionString("Credit_Purchase_ID", "");
            rblPSProsCochraneReviewEnable.Attributes.Add("onclick", "if (confirm('If you are turning on Priority Screening please check the user manual for more details on how to use it.') == false) return false;");
        }
        else
        {
            Server.Transfer("Error.aspx");
        }
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
                    string test2 = "";
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
                            lastAccessed = Convert.ToDateTime(idr1["LAST_LOGIN"].ToString());
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
            lblInviteMsgCochrane.Text = "There is more than one account with this email address. Please contact EPPI Reviewer support staff at EPPISupport@ucl.ac.uk.";
            lblInviteMsgCochrane.Visible = true;
        }
        else if (rowCount < 1)
        {
            lblInviteMsgCochrane.Text = "There is no match in the EPPI Reviewer database for this email address";
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
                    string emailAccountMsg = "<b>Your EPPI Reviewer account details need updating</b>.<br>Please log into the EPPI-Reviewer 4.0 gateway at " +
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
                            lastAccessed = Convert.ToDateTime(idr1["LAST_LOGIN"].ToString());
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
        Server.Transfer("SummaryCochrane.aspx");
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
        Server.Transfer("SummaryCochrane.aspx");
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

}