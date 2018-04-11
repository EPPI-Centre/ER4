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
using System.Drawing;

public partial class Organisation : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (Utils.GetSessionString("Contact_ID") != null)
        {
            if ((Utils.GetSessionString("IsOrganisationAdm") == "1") || (Utils.GetSessionString("IsAdm") == "True"))
            {
                if (!IsPostBack)
                {
                    System.Web.UI.WebControls.Label lbl = (Label)Master.FindControl("lblPageTitle");
                    if (lbl != null)
                    {
                        lbl.Text = "Organisation details";
                    }


                    Telerik.Web.UI.RadTabStrip radTs = (Telerik.Web.UI.RadTabStrip)Master.FindControl("rtsMenu");
                    if (radTs != null)
                    {
                        radTs.SelectedIndex = 7;
                        radTs.Tabs[7].Tabs[0].Selected = true;
                        radTs.Tabs[7].Tabs[2].Width = 680;
                        if (Utils.GetSessionString("IsAdm") == "True")
                        {
                            radTs.Tabs[7].Tabs[1].Visible = true;
                            radTs.Tabs[7].Tabs[2].Width = 620;
                        }
                    }
                    System.Web.UI.WebControls.Label lbl1 = (Label)Master.FindControl("lblHeadingText");
                    if (lbl1 != null)
                    {
                        lbl1.Text = "Organisation management";
                    }


                    string selectedOrganisationID = "0";

                    if ((Utils.GetSessionString("IsOrganisationAdm") != "1") && (Utils.GetSessionString("IsAdm") == "True"))
                    {
                        // not a site license admin but they are ER4 admin (i.e. J or J or S or Z) so we don't know what data to 
                        // display. Tell the IsAdm=true person (i.e. J or J or S or Z) to go to the setup page!
                        pnlMessage.Visible = true;
                    }
                    else // the person is a site license adm (and could also be an ER4 admin)
                    {
                        // check how many site licenses this person is an administrator of
                        bool isAdmDB = true;
                        DataTable dt2 = new DataTable();
                        System.Data.DataRow newrow2;
                        dt2.Columns.Add(new DataColumn("ORGANISATION_ID", typeof(string)));
                        dt2.Columns.Add(new DataColumn("ORGANISATION_NAME", typeof(string)));
                        IDataReader idr1 = Utils.GetReader(isAdmDB, "st_Organisation_Get_By_Admin",
                                Utils.GetSessionString("Contact_ID"));
                        while (idr1.Read())
                        {
                            newrow2 = dt2.NewRow();
                            newrow2["ORGANISATION_ID"] = idr1["ORGANISATION_ID"].ToString();
                            newrow2["ORGANISATION_NAME"] = idr1["ORGANISATION_NAME"].ToString();
                            dt2.Rows.Add(newrow2);
                        }
                        idr1.Close();
                        ddlYourOrganisations.DataSource = dt2;
                        ddlYourOrganisations.DataBind();

                        if (ddlYourOrganisations.Items.Count > 1)
                        {
                            pnlMultipleOrganisation.Visible = true;
                            ddlYourOrganisations.SelectedIndex = 0;
                            selectedOrganisationID = ddlYourOrganisations.SelectedValue;
                        }



                        isAdmDB = true;




                        pnlOrganisationExists.Visible = true;

                        IDataReader idr;

                        if (selectedOrganisationID != "0")
                        {
                            idr = Utils.GetReader(isAdmDB, "st_Organisation_Get_ByID", Utils.GetSessionString("Contact_ID"),
                                selectedOrganisationID);
                        }
                        else
                        {
                            idr = Utils.GetReader(isAdmDB, "st_Organisation_Get", Utils.GetSessionString("Contact_ID"));
                        }
                        //

                        while (idr.Read())
                        {
                            lblOrganisationName.Text = idr["ORGANISATION_NAME"].ToString();
                            tbAddress.Text = idr["ORGANISATION_ADDRESS"].ToString();
                            tbTelephone.Text = idr["TELEPHONE"].ToString();
                            tbNotes.Text = idr["NOTES"].ToString();
                            lblOrganisationID.Text = idr["ORGANISATION_ID"].ToString();
                            lblDateCreated.Text = idr["DATE_CREATED"].ToString();
                        }
                        idr.Close();

                    }

                    buildGrids();
                    pnlAccountsAndReviews.Visible = true;

                    lblAccountMsg.Visible = false;
                    lblAccountMsg.ForeColor = System.Drawing.Color.Black;
                    lblReviewMsg.Visible = false;
                    lblReviewMsg.ForeColor = System.Drawing.Color.Black;
                    lblAccountAdmMessage.Visible = false;
                    lblAccountAdmMessage.ForeColor = System.Drawing.Color.Black;

                }
                cmdAddAdm.Attributes.Add("onclick", "if (confirm('Are you sure you want to make this person an organisation admin?') == false) return false;");
            }
            else
            {
                Server.Transfer("Summary.aspx");
            }
        }
        else
        {
            Server.Transfer("Error.aspx");
        }
    }

    private void buildGrids()
    {
        DataTable dt = new DataTable();
        System.Data.DataRow newrow;
        DataTable dt1 = new DataTable();
        System.Data.DataRow newrow1;
        DataTable dt2 = new DataTable();
        System.Data.DataRow newrow2;
        DataTable dt3 = new DataTable();
        System.Data.DataRow newrow3;

        dt.Columns.Add(new DataColumn("REVIEW_ID", typeof(string)));
        dt.Columns.Add(new DataColumn("REVIEW_NAME", typeof(string)));
        //dt.Columns.Add(new DataColumn("CONTACT_NAME", typeof(string)));

        dt1.Columns.Add(new DataColumn("REVIEW_ID", typeof(string)));
        dt1.Columns.Add(new DataColumn("REVIEW_NAME", typeof(string)));
        //dt1.Columns.Add(new DataColumn("CONTACT_NAME", typeof(string)));

        dt2.Columns.Add(new DataColumn("CONTACT_ID", typeof(string)));
        dt2.Columns.Add(new DataColumn("CONTACT_NAME", typeof(string)));
        dt2.Columns.Add(new DataColumn("EMAIL", typeof(string)));

        dt3.Columns.Add(new DataColumn("CONTACT_ID", typeof(string)));
        dt3.Columns.Add(new DataColumn("CONTACT_NAME", typeof(string)));
        dt3.Columns.Add(new DataColumn("EMAIL", typeof(string)));

        bool isAdmDB = true;
        IDataReader idr = Utils.GetReader(isAdmDB, "st_Organisation_Get_Accounts",
            lblOrganisationID.Text);
        while (idr.Read())
        {
            newrow1 = dt1.NewRow();
            newrow1["REVIEW_ID"] = idr["REVIEW_ID"].ToString();
            newrow1["REVIEW_NAME"] = idr["REVIEW_NAME"].ToString();
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
            paramList[1] = new SqlParameter("@lic_id", SqlDbType.Int, 8, ParameterDirection.Input,
                true, 0, 0, null, DataRowVersion.Default, lblOrganisationID.Text);
            paramList[0] = new SqlParameter("@admin_ID", SqlDbType.Int, 8, ParameterDirection.Input,
                true, 0, 0, null, DataRowVersion.Default, Utils.GetSessionString("Contact_ID"));
            paramList[2] = new SqlParameter("@review_id", SqlDbType.Int, 8, ParameterDirection.Input,
                true, 0, 0, null, DataRowVersion.Default, tbReviewID.Text);
            paramList[3] = new SqlParameter("@res", SqlDbType.Int, 8, ParameterDirection.Output,
                true, 0, 0, null, DataRowVersion.Default, "");
            Utils.ExecuteSPWithReturnValues(isAdmDB, Server, "st_Site_Lic_Add_Review", paramList);

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
                        lblReviewMsg.Text = "review already in this site_lic";
                        break;
                    case "-4":
                        lblReviewMsg.Text = "review is in some other site_lic";
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
                lblAccountAdmMessage.Text = "this email is already an admin in this organisation";
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
            //Utils.ExecuteSPWithReturnValues(isAdmDB, Server, "st_Site_Lic_Add_Remove_Admin", paramList);
            Utils.ExecuteSPWithReturnValues(isAdmDB, Server, "st_Organisation_Add_Remove_Admin", paramList);

            if (paramList[3].Value.ToString() != "0")
            {
                switch (paramList[3].Value.ToString())
                {
                    case "-1":
                        lblAccountAdmMessage.Text = "supplied admin_id is not an admin of this organisation";
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
                    lblAccountAdmMessage.Visible = true;
                    lblAccountAdmMessage.ForeColor = System.Drawing.Color.Red;
                }
                else
                {
                    buildGrids();
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
            lb.Attributes.Add("onclick", "if (confirm('Are you sure you want to remove this user from the license?') == false) return false;");
        }
    }
    protected void gvOrganisationAdms_RowCommand(object sender, GridViewCommandEventArgs e)
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
                    paramList[1] = new SqlParameter("@org_id", SqlDbType.Int, 8, ParameterDirection.Input,
                        true, 0, 0, null, DataRowVersion.Default, lblOrganisationID.Text);
                    paramList[0] = new SqlParameter("@admin_ID", SqlDbType.Int, 8, ParameterDirection.Input,
                        true, 0, 0, null, DataRowVersion.Default, Utils.GetSessionString("Contact_ID"));
                    paramList[2] = new SqlParameter("@contact_email", SqlDbType.NVarChar, 255, ParameterDirection.Input,
                        true, 0, 0, null, DataRowVersion.Default, email);
                    paramList[3] = new SqlParameter("@res", SqlDbType.Int, 8, ParameterDirection.Output,
                        true, 0, 0, null, DataRowVersion.Default, "");
                    Utils.ExecuteSPWithReturnValues(isAdmDB, Server, "st_Organisation_Add_Remove_Admin", paramList);

                    if (paramList[3].Value.ToString() != "0")
                    {
                        switch (paramList[3].Value.ToString())
                        {
                            case "-1":
                                lblAccountAdmMessage.Text = "supplied admin_id is not an admin of this organisation";
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
                            // if you pulled yourself out of the admin list we must remove your access to Organisation.aspx
                            Utils.SetSessionString("IsOrganisationAdm", "0");
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
                    lblAccountAdmMessage.Text = "There must be at least one admin in the organisation";
                    lblAccountAdmMessage.Visible = true;
                    lblAccountAdmMessage.ForeColor = System.Drawing.Color.Red;
                }

                break;
            default:
                break;
        }
    }
    protected void gvOrganisationAdms_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            LinkButton lb = (LinkButton)(e.Row.Cells[3].Controls[0]);
            lb.Attributes.Add("onclick", "if (confirm('Are you sure you want to remove this admin from the license?') == false) return false;");
        }
    }



    protected void cmdSaveOrganisation_Click(object sender, EventArgs e)
    {
        lblOrganisationMessage.Visible = false;
        lblOrganisationMessage.Text = "Required fields *";
        lblOrganisationMessage.ForeColor = System.Drawing.Color.Black;
        lblOrganisationMessage.Font.Bold = false;
        bool licenseConditionsMet = true;

        if ((tbAddress.Text == "") || (tbTelephone.Text == ""))
        {
            lblOrganisationMessage.Visible = true;
            lblOrganisationMessage.Text = "Please fill in all of the required fields *";
            lblOrganisationMessage.ForeColor = System.Drawing.Color.Red;
            lblOrganisationMessage.Font.Bold = true;
            licenseConditionsMet = false;
        }

        if (licenseConditionsMet == true)
        {
            bool isAdmDB = true;

            Utils.ExecuteSP(isAdmDB, Server, "st_Organisation_Edit_By_OrgAdm",
              lblOrganisationID.Text, tbAddress.Text,
              tbTelephone.Text, tbNotes.Text);
        }
    }






    

    protected void ddlYourOrganisations_SelectedIndexChanged(object sender, EventArgs e)
    {
        bool isAdmDB = true;
        IDataReader idr = Utils.GetReader(isAdmDB, "st_Organisation_Get_ByID", Utils.GetSessionString("Contact_ID"),
                ddlYourOrganisations.SelectedValue);


        while (idr.Read())
        {
            lblOrganisationName.Text = idr["ORGANISATION_NAME"].ToString();
            tbAddress.Text = idr["ORGANISATION_ADDRESS"].ToString();
            tbTelephone.Text = idr["TELEPHONE"].ToString();
            tbNotes.Text = idr["NOTES"].ToString();
            lblOrganisationID.Text = idr["ORGANISATION_ID"].ToString();
            lblDateCreated.Text = idr["DATE_CREATED"].ToString();
        }
        idr.Close();

        lblOrganisationID.Text = ddlYourOrganisations.SelectedValue;

        buildGrids();
        pnlAccountsAndReviews.Visible = true;

        lblAccountMsg.Visible = false;
        lblAccountMsg.ForeColor = System.Drawing.Color.Black;
        lblReviewMsg.Visible = false;
        lblReviewMsg.ForeColor = System.Drawing.Color.Black;
        lblAccountAdmMessage.Visible = false;
        lblAccountAdmMessage.ForeColor = System.Drawing.Color.Black;
        
        
        
        
        
        
        /*
        lblDetailsHeading.Text = "Packages";
        string selectedSiteLicenseID = ddlYourOrganisations.SelectedValue;
        bool isAdmDB = true;
        string itemToSelect = "";
        DataTable dt1 = new DataTable();
        System.Data.DataRow newrow1;
        dt1.Columns.Add(new DataColumn("SITE_LIC_DETAILS_ID", typeof(string)));
        dt1.Columns.Add(new DataColumn("DATE_CREATED", typeof(string)));
        int counter = 0;

        // Why am I getting the package data first? If there are not any packages
        // it affects what is displayed in the sections that follow. So, perhaps there
        // is a valid reason.

        // orginal JB Jun26
        //IDataReader idr = Utils.GetReader(isAdmDB, "st_Site_Lic_Get_all_Packages", "0", 
        //    Utils.GetSessionString("Contact_ID"));
        // 

        // added JB Jun26
        IDataReader idr;
        if (selectedSiteLicenseID != "0")
        {
            idr = Utils.GetReader(isAdmDB, "st_Site_Lic_Get_all_Packages", selectedSiteLicenseID, "0");
        }
        else
        {
            idr = Utils.GetReader(isAdmDB, "st_Site_Lic_Get_all_Packages", "0",
                Utils.GetSessionString("Contact_ID"));
        }
        //
        while (idr.Read())
        {
            newrow1 = dt1.NewRow();
            newrow1["SITE_LIC_DETAILS_ID"] = idr["SITE_LIC_DETAILS_ID"].ToString();
            if (idr["IS_ACTIVE"].ToString() == "True")
                newrow1["DATE_CREATED"] = "Offer - " + idr["DATE_CREATED"].ToString();
            if (idr["IS_ACTIVE"].ToString() == "False")
            {
                if (counter == 0)
                {
                    newrow1["DATE_CREATED"] = "Latest - " + idr["DATE_CREATED"].ToString();
                    itemToSelect = idr["SITE_LIC_DETAILS_ID"].ToString();
                    counter += 1;
                }
                else
                    newrow1["DATE_CREATED"] = "Expired - " + idr["DATE_CREATED"].ToString();
            }
            dt1.Rows.Add(newrow1);
        }
        idr.Close();
        ddlPackages.DataSource = dt1;
        ddlPackages.DataBind();
        ddlPackages.Enabled = true;

        pnlSiteLicenseExists.Visible = true;

        lblExpiryDate.Font.Bold = false;
        lblExpiryDate.BackColor = ColorTranslator.FromHtml("#E2E9EF");


        if (ddlPackages.Items.Count != 0)
        {
            // license details are available
            if (itemToSelect != "")
                ddlPackages.SelectedValue = itemToSelect;
            else
                ddlPackages.SelectedIndex = 0;

            pnlSiteLicense.Visible = true;
            pnlPackages.Visible = true;
            pnlAccountsAndReviews.Visible = true;
            lbPackageHistory.Enabled = true;

            // orginal JB Jun26
            //idr = Utils.GetReader(isAdmDB, "st_Site_Lic_Get_Details",
            //        Utils.GetSessionString("Contact_ID"));
            //

            // added JB Jun26 
            if (selectedSiteLicenseID != "0")
            {
                idr = Utils.GetReader(isAdmDB, "st_Site_Lic_Get_Details_1",
                    Utils.GetSessionString("Contact_ID"), selectedSiteLicenseID);
            }
            else
            {
                idr = Utils.GetReader(isAdmDB, "st_Site_Lic_Get_Details",
                    Utils.GetSessionString("Contact_ID"));
            }
            //
            if (idr.Read())
            {
                lblLicenseName.Text = idr["SITE_LIC_NAME"].ToString();
                tbOrganisation.Text = idr["COMPANY_NAME"].ToString();
                tbAddress.Text = idr["COMPANY_ADDRESS"].ToString();
                tbTelephone.Text = idr["TELEPHONE"].ToString();
                tbNotes.Text = idr["NOTES"].ToString();
                lblSiteLicID.Text = idr["SITE_LIC_ID"].ToString();
                lblDateCreated.Text = idr["DATE_CREATED"].ToString();

                lblSiteLicenseDetailsID.Text = idr["SITE_LIC_DETAILS_ID"].ToString();
                lblNumberMonths.Text = idr["MONTHS"].ToString();
                lblNumberAccounts.Text = idr["ACCOUNTS_ALLOWANCE"].ToString();
                lblNumberReviews.Text = idr["REVIEWS_ALLOWANCE"].ToString();
                lblTotalFee.Text = (int.Parse(idr["PRICE_PER_MONTH"].ToString()) * int.Parse(lblNumberMonths.Text)).ToString();
                lblValidFrom.Text = idr["VALID_FROM"].ToString();

                if (idr["ALLOW_REVIEW_OWNERSHIP_CHANGE"].ToString() == "True")
                {
                    lblSiteLicenceID.Text = "Site license #";
                }
                else
                {
                    lblSiteLicenceID.Text = "Site license ID";
                }

                if (ddlPackages.SelectedItem.Text.StartsWith("Expired"))
                {
                    lblExpiryDate.Text = "Expired";
                    lblExpiryDate.BackColor = Color.Pink;
                    lblExpiryDate.Font.Bold = true;
                    lblSiteLicenceID.Text = "Site license ID";
                }
                else
                {
                    // check if the latest is expired
                    DateTime expired = Convert.ToDateTime(idr["EXPIRY_DATE"].ToString());
                    DateTime today = DateTime.Today;
                    if (expired < today)
                    {
                        lblExpiryDate.BackColor = Color.Pink;
                        lblExpiryDate.Font.Bold = true;
                        lblSiteLicenceID.Text = "Site license ID";
                    }
                    else
                    {
                        lblExpiryDate.BackColor = ColorTranslator.FromHtml("#E2E9EF");
                        lblExpiryDate.Font.Bold = false;
                    }
                    lblExpiryDate.Text = idr["EXPIRY_DATE"].ToString();
                }
                lblIsActivated.Text = "Yes";
                lbPackageHistory.Enabled = true;
                lbExtensionHistory.Enabled = true;
            }
            else
            {
                // read the offers as there aren't any active packages
                idr.NextResult();
                if (idr.Read())
                {
                    lblLicenseName.Text = idr["SITE_LIC_NAME"].ToString();
                    tbOrganisation.Text = idr["COMPANY_NAME"].ToString();
                    tbAddress.Text = idr["COMPANY_ADDRESS"].ToString();
                    tbTelephone.Text = idr["TELEPHONE"].ToString();
                    tbNotes.Text = idr["NOTES"].ToString();
                    lblSiteLicID.Text = idr["SITE_LIC_ID"].ToString();
                    lblDateCreated.Text = idr["DATE_CREATED"].ToString();

                    lblSiteLicenseDetailsID.Text = idr["SITE_LIC_DETAILS_ID"].ToString();
                    lblNumberMonths.Text = idr["MONTHS"].ToString();
                    lblNumberAccounts.Text = idr["ACCOUNTS_ALLOWANCE"].ToString();
                    lblNumberReviews.Text = idr["REVIEWS_ALLOWANCE"].ToString();
                    lblTotalFee.Text = (int.Parse(idr["PRICE_PER_MONTH"].ToString()) * int.Parse(lblNumberMonths.Text)).ToString();
                    lblValidFrom.Text = idr["VALID_FROM"].ToString();
                    if (lblValidFrom.Text == "")
                    {
                        lblValidFrom.Text = "N/A";
                        cmdAddAccount.Enabled = false;
                        cmdAddReview.Enabled = false;
                        lblDetailsHeading.Text = "There are not any activated packages associated with this license";
                    }
                    lblExpiryDate.Text = idr["EXPIRY_DATE"].ToString();
                    if (lblExpiryDate.Text == "")
                        lblExpiryDate.Text = "N/A";
                    lblIsActivated.Text = "No";
                    lbPackageHistory.Enabled = true;
                    lbExtensionHistory.Enabled = true;

                    lblSiteLicenceID.Text = "Site license ID";
                }
            }
            idr.Close();
        }
        else
        {
            // site license only - no details/packages available


            // orginal JB Jun26
            //iidr = Utils.GetReader(isAdmDB, "st_Site_Lic_Get", Utils.GetSessionString("Contact_ID"));
            //

            // added JB Jun26 
            if (selectedSiteLicenseID != "0")
            {
                idr = Utils.GetReader(isAdmDB, "st_Site_Lic_Get_1", Utils.GetSessionString("Contact_ID"),
                    selectedSiteLicenseID);
            }
            else
            {
                idr = Utils.GetReader(isAdmDB, "st_Site_Lic_Get", Utils.GetSessionString("Contact_ID"));
            }
            //

            while (idr.Read())
            {
                lblLicenseName.Text = idr["SITE_LIC_NAME"].ToString();
                tbOrganisation.Text = idr["COMPANY_NAME"].ToString();
                tbAddress.Text = idr["COMPANY_ADDRESS"].ToString();
                tbTelephone.Text = idr["TELEPHONE"].ToString();
                tbNotes.Text = idr["NOTES"].ToString();
                lblSiteLicID.Text = idr["SITE_LIC_ID"].ToString();
                lblDateCreated.Text = idr["DATE_CREATED"].ToString();
            }
            idr.Close();

            cmdAddAccount.Enabled = false;
            cmdAddReview.Enabled = false;
            lblDetailsHeading.Text = "There are no packages associated with this license";
        }

        pnlPackages.Visible = true;

        buildGrids();
        pnlAccountsAndReviews.Visible = true;

        lblAccountMsg.Visible = false;
        lblAccountMsg.ForeColor = System.Drawing.Color.Black;
        lblReviewMsg.Visible = false;
        lblReviewMsg.ForeColor = System.Drawing.Color.Black;
        lblAccountAdmMessage.Visible = false;
        lblAccountAdmMessage.ForeColor = System.Drawing.Color.Black;

        */
    }
    


    protected void gvReviews_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            LinkButton reviewOwner = (LinkButton)e.Row.Cells[2].FindControl("lbReviewOwner");

            bool isAdmDB = true;
            IDataReader idr = Utils.GetReader(isAdmDB, "st_GetReviewOwner", e.Row.Cells[0].Text);
            if (idr.Read())
            {
                reviewOwner.Text = idr["CONTACT_NAME"].ToString();
            }
            idr.Close();
            reviewOwner.Attributes.Add("onclick", "JavaScript:openContactList('" + e.Row.Cells[0].Text + " - Please select from site license members')");
            Utils.SetSessionString("siteLicenseID", lblOrganisationID.Text);

            if (lblOrganisationID.Text == "Site license #")
            {
                //e.Row.Cells[2].Visible = true;
                this.gvReviews.Columns[2].Visible = true;
            }
            else
            {
                this.gvReviews.Columns[2].Visible = false;
            }


        }
    }



    protected void cmdPlaceFunder_Click(object sender, EventArgs e)
    {
        string contactID = Utils.GetSessionString("variableID");

        // change the owner and make sure new owner is in review and has admin access
        bool isAdmDB = true;
        Utils.ExecuteSP(isAdmDB, Server, "st_ChangeReviewOwner", contactID, Utils.GetSessionString("siteLicenseReviewID"));

        // reset your session variables
        Utils.SetSessionString("siteLicenseID", "0");
        Utils.SetSessionString("siteLicenseReviewID", "0");

        // reload grids so they show the updated information
        buildGrids();
    }



    protected void cmdAddAccount_Click2(object sender, EventArgs e)
    {
        bool okToProceed = true;
        lblAccountMsg.Visible = false;

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
    }
}