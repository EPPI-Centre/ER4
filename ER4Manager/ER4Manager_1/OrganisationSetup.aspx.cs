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

public partial class OrganisationSetup : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (Utils.GetSessionString("Contact_ID") != null)
        {
            if (Utils.GetSessionString("IsAdm") == "True")
            {
                if (!IsPostBack)
                {
                    Utils.SetSessionString("organisationID", "0");

                    System.Web.UI.WebControls.Label lbl = (Label)Master.FindControl("lblPageTitle");
                    if (lbl != null)
                    {
                        lbl.Text = "Organisation setup";
                    }

                    Telerik.Web.UI.RadTabStrip radTs = (Telerik.Web.UI.RadTabStrip)Master.FindControl("rtsMenu");
                    if (radTs != null)
                    {
                        radTs.SelectedIndex = 7;
                        radTs.Tabs[7].Tabs[1].Selected = true;
                        radTs.Tabs[7].Tabs[2].Width = 720;
                        if (Utils.GetSessionString("IsAdm") == "True")
                        {
                            radTs.Tabs[7].Tabs[1].Visible = true;
                            radTs.Tabs[7].Tabs[2].Width = 620;
                        }
                    }

                    buildOrganisationGrid(0);
                    //buildLicenseGrid2(0);
                    //buildRadGrid();
                }

                IBCalendar1.Attributes.Add("onclick", "JavaScript:openCalendar1('" +
                    "!1!" + tbDateOrganisationCreated.Text + "')");

                lbSelectAdmin.Attributes.Add("onclick", "JavaScript:openAdminList('Please select')");

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


    private void buildOrganisationGrid(int page)
    {
        DataTable dt = new DataTable();
        System.Data.DataRow newrow;
        dt.Columns.Add(new DataColumn("CONTACT_ID", typeof(string)));
        dt.Columns.Add(new DataColumn("CONTACT_NAME", typeof(string)));
        dt.Columns.Add(new DataColumn("ORGANISATION_ID", typeof(string)));
        dt.Columns.Add(new DataColumn("ORGANISATION_NAME", typeof(string)));

        bool isAdmDB = true;
        IDataReader idr = Utils.GetReader(isAdmDB, "st_Organisation_Get_All");
        while (idr.Read())
        {
            newrow = dt.NewRow();
            newrow["CONTACT_NAME"] = idr["CONTACT_NAME"].ToString();
            newrow["CONTACT_ID"] = idr["CONTACT_ID"].ToString(); // not visible
            newrow["ORGANISATION_ID"] = idr["ORGANISATION_ID"].ToString();
            newrow["ORGANISATION_NAME"] = idr["ORGANISATION_NAME"].ToString();
            dt.Rows.Add(newrow);
        }
        idr.Close();

        gvOrganisations.DataSource = dt;
        gvOrganisations.PageIndex = page;
        gvOrganisations.DataBind();
    }


    protected void gvOrganisations_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        pnlOrganisation.Visible = false;
        pnlOrganisationDetails.Visible = false;
        pnlAccountsAndReviews.Visible = false;

        string itemToSelect = "";
        int index = Convert.ToInt32(e.CommandArgument);
        string adminID = (string)gvOrganisations.DataKeys[index].Value;
        string organisationID = gvOrganisations.Rows[index].Cells[0].Text;
        switch (e.CommandName)
        {
            case "EDT":
                bool isAdmDB = true;
                IDataReader idr = Utils.GetReader(isAdmDB, "st_Organisation_Get_ByID", adminID, organisationID);
                //
                while (idr.Read())
                {
                    tbOrganisationName.Text = idr["ORGANISATION_NAME"].ToString();
                    lblOrganisationID.Text = idr["ORGANISATION_ID"].ToString();
                    tbAddress.Text = idr["ORGANISATION_ADDRESS"].ToString();
                    tbTelephone.Text = idr["TELEPHONE"].ToString();
                    tbNotes.Text = idr["NOTES"].ToString();
                    tbDateOrganisationCreated.Text = idr["DATE_CREATED"].ToString();
                    lblCreatedBy.Text = idr["CREATOR_NAME"].ToString();
                    lblInitialAdministrator.Text = idr["ADMINISTRATOR_NAME"].ToString();
                    lblAdminID.Text = adminID;
                }
                idr.Close();



                pnlOrganisation.Visible = true;
                lbSelectAdmin.Enabled = true;
                //pnlLicenseDetails.Visible = true;

                pnlAccountsAndReviews.Visible = true;
                buildGrids();
                lblAccountMsg.Visible = false;
                lblReviewMsg.Visible = false;
                lblAccountAdmMessage.Visible = false;

                break;
            default:
                break;
        }

    }

    private void buildGrids()
    {
        DataTable dt1 = new DataTable();
        System.Data.DataRow newrow1;
        DataTable dt2 = new DataTable();
        System.Data.DataRow newrow2;
        DataTable dt3 = new DataTable();
        System.Data.DataRow newrow3;

        dt1.Columns.Add(new DataColumn("REVIEW_ID", typeof(string)));
        dt1.Columns.Add(new DataColumn("REVIEW_NAME", typeof(string)));

        dt2.Columns.Add(new DataColumn("CONTACT_ID", typeof(string)));
        dt2.Columns.Add(new DataColumn("CONTACT_NAME", typeof(string)));
        dt2.Columns.Add(new DataColumn("EMAIL", typeof(string)));

        dt3.Columns.Add(new DataColumn("CONTACT_ID", typeof(string)));
        dt3.Columns.Add(new DataColumn("CONTACT_NAME", typeof(string)));
        dt3.Columns.Add(new DataColumn("EMAIL", typeof(string)));

        bool isAdmDB = true;
        IDataReader idr = Utils.GetReader(isAdmDB, "st_Organisation_Get_Accounts",
            lblOrganisationID.Text);
        if (idr != null)
        {
            while (idr.Read())
            {
                newrow1 = dt1.NewRow();
                newrow1["REVIEW_ID"] = idr["REVIEW_ID"].ToString();
                newrow1["REVIEW_NAME"] = idr["REVIEW_NAME"].ToString(); ;
                dt1.Rows.Add(newrow1);
            }
            idr.NextResult();
            while (idr.Read())
            {
                newrow2 = dt2.NewRow();
                newrow2["CONTACT_ID"] = idr["CONTACT_ID"].ToString();
                newrow2["CONTACT_NAME"] = idr["CONTACT_NAME"].ToString();
                newrow2["EMAIL"] = idr["EMAIL"].ToString();
                dt2.Rows.Add(newrow2);
            }
            idr.NextResult();
            while (idr.Read())
            {
                newrow3 = dt3.NewRow();
                newrow3["CONTACT_ID"] = idr["CONTACT_ID"].ToString();
                newrow3["CONTACT_NAME"] = idr["CONTACT_NAME"].ToString();
                newrow3["EMAIL"] = idr["EMAIL"].ToString();
                dt3.Rows.Add(newrow3);
            }
            idr.Close();

            gvReviews.DataSource = dt1;
            gvReviews.DataBind();

            gvAccounts.DataSource = dt2;
            gvAccounts.DataBind();

            gvOrganisationAdms.DataSource = dt3;
            gvOrganisationAdms.DataBind();
        }

    }



    protected void cmdAddAccount_Click(object sender, EventArgs e)
    {
        bool okToProceed = true;
        lblAccountMsg.Visible = false;

        // check if the email is already in this license
        for (int i = 0; i < gvAccounts.Rows.Count; i++)
        {
            if (gvAccounts.Rows[i].Cells[2].Text.Contains(tbEmail.Text))
            {
                lblAccountMsg.Text = "this email is already in the organisation";
                lblAccountMsg.Visible = true;
                lblAccountMsg.ForeColor = System.Drawing.Color.Red;
                okToProceed = false;
                i = gvAccounts.Rows.Count;
            }
        }

        if (okToProceed == true)
        {
            bool isAdmDB = true;
            SqlParameter[] paramList = new SqlParameter[4];
            paramList[1] = new SqlParameter("@org_id", SqlDbType.Int, 8, ParameterDirection.Input,
                true, 0, 0, null, DataRowVersion.Default, lblOrganisationID.Text);
            paramList[0] = new SqlParameter("@admin_ID", SqlDbType.Int, 8, ParameterDirection.Input,
                true, 0, 0, null, DataRowVersion.Default, Utils.GetSessionString("Contact_ID"));
            paramList[2] = new SqlParameter("@contact_email", SqlDbType.NVarChar, 255, ParameterDirection.Input,
                true, 0, 0, null, DataRowVersion.Default, tbEmail.Text);
            paramList[3] = new SqlParameter("@res", SqlDbType.Int, 8, ParameterDirection.Output,
                true, 0, 0, null, DataRowVersion.Default, "");
            Utils.ExecuteSPWithReturnValues(isAdmDB, Server, "st_Organisation_Add_Remove_Contact", paramList);

            if (paramList[3].Value.ToString() != "0")
            {
                switch (paramList[3].Value.ToString())
                {
                    case "-1":
                        lblAccountMsg.Text = "supplied admin_id is not an admin of this organisation";
                        break;
                    case "-2":
                        lblAccountMsg.Text = "contact_id already in a site license";
                        break;
                    case "-3":
                        lblAccountMsg.Text = "No allowance available, all account slots for current license have been used";
                        break;
                    case "-4":
                        lblAccountMsg.Text = "tried to remove account but couldn't write changes! BUG ALERT";
                        break;
                    case "-5":
                        lblAccountMsg.Text = "tried to add account but couldn't write changes! BUG ALERT";
                        break;
                    case "-6":
                        lblAccountMsg.Text = "email check returned no contact_id or multiple contact_ids";
                        break;
                    default:
                        break;
                }
                lblAccountMsg.Visible = true;
                lblAccountMsg.ForeColor = System.Drawing.Color.Red;
            }
            else
            {
                tbEmail.Text = "Enter email address";
                buildGrids();
            }
        }


        /*
        bool okToProceed = true;
        lblAccountMsg.Visible = false;

        lblAccountMsg.Visible = false;
        lblAccountMsg.ForeColor = System.Drawing.Color.Black;

        // check if the email is already in this license
        for (int i = 0; i < gvAccounts.Rows.Count; i++)
        {
            if (gvAccounts.Rows[i].Cells[2].Text.Contains(tbEmail.Text))
            {
                lblAccountMsg.Text = "this email is already in the site license";
                lblAccountMsg.Visible = true;
                lblAccountMsg.ForeColor = System.Drawing.Color.Red;
                okToProceed = false;
                i = gvAccounts.Rows.Count;
            }
        }

        if (okToProceed == true)
        {
            bool isAdmDB = true;
            SqlParameter[] paramList = new SqlParameter[4];
            paramList[1] = new SqlParameter("@lic_id", SqlDbType.Int, 8, ParameterDirection.Input,
                true, 0, 0, null, DataRowVersion.Default, lblOrganisationID.Text);
            paramList[0] = new SqlParameter("@admin_ID", SqlDbType.Int, 8, ParameterDirection.Input,
                true, 0, 0, null, DataRowVersion.Default, Utils.GetSessionString("Contact_ID"));
            paramList[2] = new SqlParameter("@contact_email", SqlDbType.NVarChar, 255, ParameterDirection.Input,
                true, 0, 0, null, DataRowVersion.Default, tbEmail.Text);
            paramList[3] = new SqlParameter("@res", SqlDbType.Int, 8, ParameterDirection.Output,
                true, 0, 0, null, DataRowVersion.Default, "");
            Utils.ExecuteSPWithReturnValues(isAdmDB, Server, "st_Site_Lic_Add_Remove_Contact", paramList);

            if (paramList[3].Value.ToString() != "0")
            {
                switch (paramList[3].Value.ToString())
                {
                    case "-1":
                        lblAccountMsg.Text = "supplied admin_id is not an admin of this site license";
                        break;
                    case "-2":
                        lblAccountMsg.Text = "contact_id already in a site license";
                        break;
                    case "-3":
                        lblAccountMsg.Text = "no allowance available, all account slots for current license have been used";
                        break;
                    case "-4":
                        lblAccountMsg.Text = "tried to remove account but couldn't write changes! BUG ALERT";
                        break;
                    case "-5":
                        lblAccountMsg.Text = "tried to add account but couldn't write changes! BUG ALERT";
                        break;
                    case "-6":
                        lblAccountMsg.Text = "email check returned no contact_id or multiple contact_ids";
                        break;
                    default:
                        break;
                }
                lblAccountMsg.Visible = true;
                lblAccountMsg.ForeColor = System.Drawing.Color.Red;
            }
            else
            {
                tbEmail.Text = "Enter email address";
                buildGrids();
            }
        }
        */
    }
    protected void cmdAddReview_Click(object sender, EventArgs e)
    {
        bool okToProceed = true;
        lblReviewMsg.Visible = false;

        if (Utils.IsNumeric(tbReviewID.Text) != true)
        {
            okToProceed = false;
            lblReviewMsg.Visible = true;
            lblReviewMsg.ForeColor = System.Drawing.Color.Red;
            lblReviewMsg.Text = "This is not a valid ID number";
        }

        if (okToProceed == true)
        {
            bool isAdmDB = true;
            SqlParameter[] paramList = new SqlParameter[4];
            paramList[0] = new SqlParameter("@org_id", SqlDbType.Int, 8, ParameterDirection.Input,
                true, 0, 0, null, DataRowVersion.Default, lblOrganisationID.Text);
            paramList[1] = new SqlParameter("@admin_ID", SqlDbType.Int, 8, ParameterDirection.Input,
                true, 0, 0, null, DataRowVersion.Default, Utils.GetSessionString("Contact_ID"));
            paramList[2] = new SqlParameter("@review_id", SqlDbType.Int, 8, ParameterDirection.Input,
                true, 0, 0, null, DataRowVersion.Default, tbReviewID.Text);
            paramList[3] = new SqlParameter("@res", SqlDbType.Int, 8, ParameterDirection.Output,
                true, 0, 0, null, DataRowVersion.Default, "");
            Utils.ExecuteSPWithReturnValues(isAdmDB, Server, "st_Organisation_Add_Review", paramList);

            if (paramList[3].Value.ToString() != "0")
            {
                switch (paramList[3].Value.ToString())
                {
                    case "-1":
                        lblReviewMsg.Text = "supplied admin_id is not an admin of this organisation";
                        break;
                    case "-2":
                        lblReviewMsg.Text = "review_id does not exist";
                        break;
                    case "-3":
                        lblReviewMsg.Text = "review already in this site_lic";
                        break;
                    case "-4":
                        lblReviewMsg.Text = "review is already in an organisation";
                        break;
                    case "-5":
                        lblReviewMsg.Text = "no allowance available, all review slots for current license have been used";
                        break;
                    case "-6":
                        lblReviewMsg.Text = "all seemed well but couldn't write changes! BUG ALERT";
                        break;
                    default:
                        break;
                }
                lblReviewMsg.Visible = true;
                lblReviewMsg.ForeColor = System.Drawing.Color.Red;
            }
            else
            {
                tbReviewID.Text = "Enter Review ID";
                buildGrids();
            }
        }
    }
    protected void cmdAddAdm_Click(object sender, EventArgs e)
    {
        bool okToProceed = true;
        lblAccountAdmMessage.Visible = false;

        // check if the email is already in this license
        for (int i = 0; i < gvOrganisationAdms.Rows.Count; i++)
        {
            if (gvOrganisationAdms.Rows[i].Cells[2].Text.Contains(tbEmailAdm.Text))
            {
                lblAccountAdmMessage.Text = "this email is already an admin in this site license";
                lblAccountAdmMessage.Visible = true;
                lblAccountAdmMessage.ForeColor = System.Drawing.Color.Red;
                okToProceed = false;
                i = gvOrganisationAdms.Rows.Count;
            }
        }

        if (okToProceed == true)
        {
            bool isAdmDB = true;
            SqlParameter[] paramList = new SqlParameter[4];
            paramList[1] = new SqlParameter("@org_id", SqlDbType.Int, 8, ParameterDirection.Input,
                true, 0, 0, null, DataRowVersion.Default, lblOrganisationID.Text);
            paramList[0] = new SqlParameter("@admin_ID", SqlDbType.Int, 8, ParameterDirection.Input,
                true, 0, 0, null, DataRowVersion.Default, Utils.GetSessionString("Contact_ID"));
            paramList[2] = new SqlParameter("@contact_email", SqlDbType.NVarChar, 255, ParameterDirection.Input,
                true, 0, 0, null, DataRowVersion.Default, tbEmailAdm.Text);
            paramList[3] = new SqlParameter("@res", SqlDbType.Int, 8, ParameterDirection.Output,
                true, 0, 0, null, DataRowVersion.Default, "");
            Utils.ExecuteSPWithReturnValues(isAdmDB, Server, "st_Organisation_Add_Remove_Admin", paramList);

            if (paramList[3].Value.ToString() != "0")
            {
                switch (paramList[3].Value.ToString())
                {
                    case "-1":
                        lblAccountAdmMessage.Text = "supplied admin_id is not an admin of this site license";
                        break;
                    case "-2":
                        lblAccountAdmMessage.Text = "contact_id already in a site license";
                        break;
                    case "-3":
                        lblAccountAdmMessage.Text = "failure to remove the contact from the list! BUG ALERT";
                        break;
                    case "-4":
                        lblAccountAdmMessage.Text = "failure to add the contact to the list! BUG ALERT";
                        break;
                    case "-5":
                        lblAccountAdmMessage.Text = "email check returned no contact_id or multiple contact_ids";
                        break;
                    default:
                        break;
                }
                lblAccountAdmMessage.Visible = true;
                lblAccountAdmMessage.ForeColor = System.Drawing.Color.Red;
            }
            else
            {
                tbEmailAdm.Text = "Enter email address";
                buildGrids();
            }
        }
    }
    protected void gvAccounts_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        lblAccountMsg.Visible = false;
        lblAccountMsg.ForeColor = System.Drawing.Color.Black;
        int index = Convert.ToInt32(e.CommandArgument);
        string email = (string)gvAccounts.DataKeys[index].Value;
        switch (e.CommandName)
        {
            case "REMOVE":
                bool isAdmDB = true;
                SqlParameter[] paramList = new SqlParameter[4];
                paramList[1] = new SqlParameter("@org_id", SqlDbType.Int, 8, ParameterDirection.Input,
                    true, 0, 0, null, DataRowVersion.Default, lblOrganisationID.Text);
                paramList[0] = new SqlParameter("@admin_ID", SqlDbType.Int, 8, ParameterDirection.Input,
                    true, 0, 0, null, DataRowVersion.Default, Utils.GetSessionString("Contact_ID"));
                paramList[2] = new SqlParameter("@contact_email", SqlDbType.NVarChar, 255, ParameterDirection.Input,
                    true, 0, 0, null, DataRowVersion.Default, email);
                paramList[3] = new SqlParameter("@res", SqlDbType.Int, 8, ParameterDirection.Output,
                    true, 0, 0, null, DataRowVersion.Default, "");
                Utils.ExecuteSPWithReturnValues(isAdmDB, Server, "st_Organisation_Add_Remove_Contact", paramList);

                if (paramList[3].Value.ToString() != "0")
                {
                    switch (paramList[3].Value.ToString())
                    {
                        case "-1":
                            lblAccountMsg.Text = "supplied admin_id is not an admin of this site license";
                            break;
                        case "-2":
                            lblAccountMsg.Text = "contact_id already in a site license";
                            break;
                        case "-3":
                            lblAccountMsg.Text = "no allowance available, all account slots for current license have been used";
                            break;
                        case "-4":
                            lblAccountMsg.Text = "tried to remove account but couldn't write changes! BUG ALERT";
                            break;
                        case "-5":
                            lblAccountMsg.Text = "tried to add account but couldn't write changes! BUG ALERT";
                            break;
                        case "-6":
                            lblAccountMsg.Text = "email check returned no contact_id or multiple contact_ids";
                            break;
                        default:
                            break;
                    }
                    lblAccountMsg.Visible = true;
                    lblAccountMsg.ForeColor = System.Drawing.Color.Red;
                }
                else
                {
                    buildGrids();
                    tbEmail.Text = "Enter email address";
                }

                break;
            default:
                break;
        }
    }
    protected void gvAccounts_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            LinkButton lb = (LinkButton)(e.Row.Cells[3].Controls[0]);
            lb.Attributes.Add("onclick", "if (confirm('Are you sure you want to remove this user from this organisation?') == false) return false;");
        }
    }
    protected void gvLicenseAdms_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        int index = Convert.ToInt32(e.CommandArgument);
        string email = (string)gvOrganisationAdms.DataKeys[index].Value;
        string contactID = gvOrganisationAdms.Rows[index].Cells[0].Text;
        switch (e.CommandName)
        {
            case "REMOVE":
                if (gvOrganisationAdms.Rows.Count > 1)
                {
                    bool isAdmDB = true;
                    SqlParameter[] paramList = new SqlParameter[4];
                    paramList[1] = new SqlParameter("@lic_id", SqlDbType.Int, 8, ParameterDirection.Input,
                        true, 0, 0, null, DataRowVersion.Default, lblOrganisationID.Text);
                    paramList[0] = new SqlParameter("@admin_ID", SqlDbType.Int, 8, ParameterDirection.Input,
                        true, 0, 0, null, DataRowVersion.Default, Utils.GetSessionString("Contact_ID"));
                    paramList[2] = new SqlParameter("@contact_email", SqlDbType.NVarChar, 255, ParameterDirection.Input,
                        true, 0, 0, null, DataRowVersion.Default, email);
                    paramList[3] = new SqlParameter("@res", SqlDbType.Int, 8, ParameterDirection.Output,
                        true, 0, 0, null, DataRowVersion.Default, "");
                    Utils.ExecuteSPWithReturnValues(isAdmDB, Server, "st_Site_Lic_Add_Remove_Admin_1", paramList);

                    if (paramList[3].Value.ToString() != "0")
                    {
                        switch (paramList[3].Value.ToString())
                        {
                            case "-1":
                                lblAccountAdmMessage.Text = "supplied admin_id is not an admin of this site license";
                                break;
                            case "-2":
                                lblAccountAdmMessage.Text = "contact_id already in a site license";
                                break;
                            case "-3":
                                lblAccountAdmMessage.Text = "failure to remove the contact from the list! BUG ALERT";
                                break;
                            case "-4":
                                lblAccountAdmMessage.Text = "failure to add the contact to the list! BUG ALERT";
                                break;
                            case "-5":
                                lblAccountAdmMessage.Text = "email check returned no contact_id or multiple contact_ids";
                                break;
                            default:
                                break;
                        }
                        lblAccountMsg.Visible = true;
                        lblAccountMsg.ForeColor = System.Drawing.Color.Red;
                    }
                    else
                    {
                        if (Utils.GetSessionString("Contact_ID") == contactID)
                        {
                            // if you pulled yourself out of the admin list we must remove your access to SiteLicense.aspx
                            Utils.SetSessionString("IsSiteLicenseAdm", "0");
                            Server.Transfer("Summary.aspx");
                        }
                        else
                        {
                            buildGrids();
                        }
                    }
                }
                else
                {
                    lblAccountAdmMessage.Text = "There must be at least one admin in the site license";
                    lblAccountAdmMessage.Visible = true;
                    lblAccountAdmMessage.ForeColor = System.Drawing.Color.Red;
                }

                break;
            default:
                break;
        }
    }
    protected void gvLicenseAdms_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            LinkButton lb = (LinkButton)(e.Row.Cells[3].Controls[0]);
            lb.Attributes.Add("onclick", "if (confirm('Are you sure you want to remove this admin from this organisation?') == false) return false;");
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
                tbDateOrganisationCreated.Text = Utils.GetSessionString("CalendarDate").Substring(counterLocation + 1);
            }
        }
        Utils.SetSessionString("CalendarDate", "");
    }
    protected void cmdPlaceFunder_Click(object sender, EventArgs e)
    {
        lblInitialAdministrator.Text = Utils.GetSessionString("variableID");
        bool isAdmDB = true;
        lblAdminID.Text = Utils.GetSessionString("variableID");
        IDataReader idr = Utils.GetReader(isAdmDB, "st_ContactDetails", Utils.GetSessionString("variableID"));
        if (idr.Read())
        {
            lblInitialAdministrator.Text = idr["CONTACT_NAME"].ToString();
        }
        idr.Close();
        Utils.SetSessionString("variableID", "");
    }
    protected void lbLicenseHistory_Click(object sender, EventArgs e)
    {
        if (lbOrganisationHistory.Text == "View")
        {
            lbOrganisationHistory.Text = "Hide";
            gvOrganisationHistory.Visible = true;
            buildLicenseLogGrid();
        }
        else
        {
            lbOrganisationHistory.Text = "View";
            gvOrganisationHistory.Visible = false;
        }
    }
    private void buildLicenseLogGrid()
    {

        DataTable dt = new DataTable();
        System.Data.DataRow newrow;

        dt.Columns.Add(new DataColumn("SITE_LIC_DETAILS_ID", typeof(string)));
        dt.Columns.Add(new DataColumn("CONTACT_ID", typeof(string)));
        dt.Columns.Add(new DataColumn("AFFECTED_ID", typeof(string)));
        dt.Columns.Add(new DataColumn("CHANGE_TYPE", typeof(string)));
        dt.Columns.Add(new DataColumn("REASON", typeof(string)));
        dt.Columns.Add(new DataColumn("DATE", typeof(string)));

        bool isAdmDB = true;
        IDataReader idr = Utils.GetReader(isAdmDB, "st_Site_Lic_Get_Package_Log", lblOrganisationID.Text);
        while (idr.Read())
        {
            newrow = dt.NewRow();
            newrow["SITE_LIC_DETAILS_ID"] = idr["SITE_LIC_DETAILS_ID"].ToString();
            newrow["CONTACT_ID"] = idr["CONTACT_ID"].ToString();
            newrow["AFFECTED_ID"] = idr["AFFECTED_ID"].ToString();
            newrow["CHANGE_TYPE"] = idr["CHANGE_TYPE"].ToString();
            newrow["REASON"] = idr["REASON"].ToString();
            newrow["DATE"] = idr["DATE"].ToString();
            dt.Rows.Add(newrow);
        }
        idr.Close();

        gvOrganisationHistory.DataSource = dt;
        gvOrganisationHistory.DataBind();

    }


    private void buildExtensionGrid()
    {
        DataTable dt = new DataTable();
        System.Data.DataRow newrow;

        dt.Columns.Add(new DataColumn("ID_EXTENDED", typeof(string)));
        dt.Columns.Add(new DataColumn("DATE_OF_EDIT", typeof(string)));
        dt.Columns.Add(new DataColumn("OLD_EXPIRY_DATE", typeof(string)));
        dt.Columns.Add(new DataColumn("NEW_EXPIRY_DATE", typeof(string)));
        dt.Columns.Add(new DataColumn("CONTACT_NAME", typeof(string)));
        dt.Columns.Add(new DataColumn("EXTENSION_TYPE", typeof(string)));
        dt.Columns.Add(new DataColumn("EXTENSION_NOTES", typeof(string)));

        bool isAdmDB = true;
        IDataReader idr = Utils.GetReader(isAdmDB, "st_Site_Lic_Extension_Record_Get",
            lblOrganisationID.Text);
        while (idr.Read())
        {
            newrow = dt.NewRow();
            newrow["ID_EXTENDED"] = idr["ID_EXTENDED"].ToString();
            newrow["DATE_OF_EDIT"] = idr["DATE_OF_EDIT"].ToString();
            newrow["OLD_EXPIRY_DATE"] = idr["OLD_EXPIRY_DATE"].ToString().Remove(10);
            newrow["NEW_EXPIRY_DATE"] = idr["NEW_EXPIRY_DATE"].ToString().Remove(10);
            newrow["CONTACT_NAME"] = idr["CONTACT_NAME"].ToString();
            newrow["EXTENSION_TYPE"] = idr["EXTENSION_TYPE"].ToString();
            newrow["EXTENSION_NOTES"] = idr["EXTENSION_NOTES"].ToString();
            dt.Rows.Add(newrow);
        }
        idr.Close();

        gvOrganisationHistory.DataSource = dt;
        gvOrganisationHistory.DataBind();
    }



    protected void gvSiteLicenses_RowDataBound(object sender, GridViewRowEventArgs e)
    {

    }
    protected void gvSiteLicenses_RowEditing(object sender, GridViewEditEventArgs e)
    {

    }

    protected void gvReviews_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            LinkButton lb = (LinkButton)(e.Row.Cells[2].Controls[0]);
            lb.Attributes.Add("onclick", "if (confirm('Are you sure you want to remove this review from this organisation?') == false) return false;");
        }
    }
    protected void gvReviews_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        //st_Site_Lic_Remove_Review
        lblReviewMsg.Visible = false;
        lblReviewMsg.ForeColor = System.Drawing.Color.Black;
        int index = Convert.ToInt32(e.CommandArgument);
        string reviewID = (string)gvReviews.DataKeys[index].Value;
        switch (e.CommandName)
        {
            case "REMOVE":
                bool isAdmDB = true;
                SqlParameter[] paramList = new SqlParameter[4];
                paramList[1] = new SqlParameter("@org_id", SqlDbType.Int, 8, ParameterDirection.Input,
                    true, 0, 0, null, DataRowVersion.Default, lblOrganisationID.Text);
                paramList[0] = new SqlParameter("@admin_ID", SqlDbType.Int, 8, ParameterDirection.Input,
                    true, 0, 0, null, DataRowVersion.Default, Utils.GetSessionString("Contact_ID"));
                paramList[2] = new SqlParameter("@review_id", SqlDbType.NVarChar, 255, ParameterDirection.Input,
                    true, 0, 0, null, DataRowVersion.Default, reviewID);
                paramList[3] = new SqlParameter("@res", SqlDbType.Int, 8, ParameterDirection.Output,
                    true, 0, 0, null, DataRowVersion.Default, "");
                Utils.ExecuteSPWithReturnValues(isAdmDB, Server, "st_Organisation_Remove_Review", paramList);

                if (paramList[3].Value.ToString() != "0")
                {
                    switch (paramList[3].Value.ToString())
                    {
                        case "-1":
                            lblReviewMsg.Text = "supplied admin_id is not an admin of this site license";
                            break;
                        case "-2":
                            lblReviewMsg.Text = "review_id does not exist";
                            break;
                        case "-3":
                            lblReviewMsg.Text = "review is not in this this site_lic";
                            break;
                        case "-4":
                            lblReviewMsg.Text = "all seemed well but couldn't write changes! BUG ALERT";
                            break;
                        default:
                            break;
                    }
                    lblReviewMsg.Visible = true;
                    lblReviewMsg.ForeColor = System.Drawing.Color.Red;
                }
                else
                {
                    buildGrids();
                    tbEmail.Text = "Enter Review ID";
                }

                break;
            default:
                break;
        }
    }

    protected void cmdSaveOrganisation_Click(object sender, EventArgs e)
    {
        lblOrganisationMessage.Visible = false;
        lblOrganisationMessage.Text = "Required fields *";
        lblOrganisationMessage.ForeColor = System.Drawing.Color.Black;
        lblOrganisationMessage.Font.Bold = false;
        string organisationCreationDate = "";
        bool organisationConditionsMet = true;

        if ((tbOrganisationName.Text == "") || (tbAddress.Text == "") ||
            (tbTelephone.Text == "") || (tbDateOrganisationCreated.Text == ""))
        {
            lblOrganisationMessage.Visible = true;
            lblOrganisationMessage.Text = "Please fill in all of the required fields *";
            lblOrganisationMessage.ForeColor = System.Drawing.Color.Red;
            lblOrganisationMessage.Font.Bold = true;
            organisationConditionsMet = false;
        }
        else if (lblInitialAdministrator.Text == "N/A")
        {
            lblOrganisationMessage.Visible = true;
            lblOrganisationMessage.Text = "You need to choose a license administrator";
            lblOrganisationMessage.ForeColor = System.Drawing.Color.Red;
            lblOrganisationMessage.Font.Bold = true;
            organisationConditionsMet = false;
        }

        if (organisationConditionsMet == true)
        {
            try
            {
                DateTime DateLicenseCreated = Convert.ToDateTime(tbDateOrganisationCreated.Text);
                organisationCreationDate = DateLicenseCreated.ToString("yyyy-M-d hh:m:ss.mmm"); // this is case sensitive!
            }
            catch (Exception er)
            {
                lblOrganisationMessage.Visible = true;
                lblOrganisationMessage.Text = "Date created error: " + er.Message.ToString();
                lblOrganisationMessage.ForeColor = System.Drawing.Color.Red;
                lblOrganisationMessage.Font.Bold = true;
                organisationConditionsMet = false;
            }
        }

        if (organisationConditionsMet == true)
        {
            bool isAdmDB = true;
            SqlParameter[] paramList = new SqlParameter[9];

            paramList[0] = new SqlParameter("@ORG_ID", SqlDbType.NVarChar, 50, ParameterDirection.Input,
                    true, 0, 0, null, DataRowVersion.Default, lblOrganisationID.Text);
            paramList[1] = new SqlParameter("@creator_id", SqlDbType.NVarChar, 50, ParameterDirection.Input,
                true, 0, 0, null, DataRowVersion.Default, Utils.GetSessionString("Contact_ID"));
            paramList[2] = new SqlParameter("@admin_id", SqlDbType.NVarChar, 255, ParameterDirection.Input,
                true, 0, 0, null, DataRowVersion.Default, lblAdminID.Text);
            paramList[3] = new SqlParameter("@ORGANISATION_NAME", SqlDbType.NVarChar, 50, ParameterDirection.Input,
                    true, 0, 0, null, DataRowVersion.Default, tbOrganisationName.Text);
            paramList[4] = new SqlParameter("@ORGANISATION_ADDRESS", SqlDbType.NVarChar, 500, ParameterDirection.Input,
                    true, 0, 0, null, DataRowVersion.Default, tbAddress.Text);
            paramList[5] = new SqlParameter("@TELEPHONE", SqlDbType.NVarChar, 50, ParameterDirection.Input,
                    true, 0, 0, null, DataRowVersion.Default, tbTelephone.Text);
            paramList[6] = new SqlParameter("@NOTES", SqlDbType.NVarChar, 4000, ParameterDirection.Input,
                    true, 0, 0, null, DataRowVersion.Default, tbNotes.Text);
            paramList[7] = new SqlParameter("@DATE_CREATED", SqlDbType.NVarChar, 50, ParameterDirection.Input,
                    true, 0, 0, null, DataRowVersion.Default, organisationCreationDate);
            paramList[8] = new SqlParameter("@RESULT", SqlDbType.NVarChar, 50, ParameterDirection.Output,
                true, 0, 0, null, DataRowVersion.Default, "");

            Utils.ExecuteSPWithReturnValues(isAdmDB, Server, "st_Organisation_Create_or_Edit", paramList);

            if (paramList[8].Value.ToString() == "invalid adm")
            {
                lblOrganisationMessage.Visible = true;
                lblOrganisationMessage.Text = "The 'First adminstrator' is already a site license adminstrator. Please select another.";
                lblOrganisationMessage.ForeColor = System.Drawing.Color.Red;
                lblOrganisationMessage.Font.Bold = true;
            }
            else if (paramList[8].Value.ToString() == "valid")
            {
                buildOrganisationGrid(gvOrganisations.PageIndex);
            }
            else if (paramList[8].Value.ToString() == "rollback")
            {
                lblOrganisationMessage.Visible = true;
                lblOrganisationMessage.Text = "There was an error and the stored procedure was rolled back";
                lblOrganisationMessage.ForeColor = System.Drawing.Color.Red;
                lblOrganisationMessage.Font.Bold = true;
            }
            else  // we have created a new license
            {
                lblOrganisationID.Text = paramList[8].Value.ToString();
                buildOrganisationGrid(gvOrganisations.PageIndex);
            }


        }
    }





    protected void gvOrganisations_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {
        buildOrganisationGrid(e.NewPageIndex);
    }




    protected void ddlLicenseModel_SelectedIndexChanged(object sender, EventArgs e)
    {

    }


    protected void rblIsOrganisationPublic_SelectedIndexChanged(object sender, EventArgs e)
    {

    }

    protected void lbNewOrganisation_Click(object sender, EventArgs e)
    {
        pnlOrganisation.Visible = true;

        tbOrganisationName.Text = "";
        lblOrganisationID.Text = "N/A";
        tbAddress.Text = "";
        tbTelephone.Text = "";
        tbNotes.Text = "";
        tbDateOrganisationCreated.Text = "";
        lblCreatedBy.Text = "";
        lblInitialAdministrator.Text = "";
        lblAdminID.Text = "";

        // the select funder looks at site license ID so
        // rather than rewriting this function it I will just set this value to 0
        Utils.SetSessionString("siteLicenseID", "0");
    }

    protected void lbOrganisationHistory_Click(object sender, EventArgs e)
    {

    }

    protected void gvOrganisations_RowEditing(object sender, GridViewEditEventArgs e)
    {

    }
}