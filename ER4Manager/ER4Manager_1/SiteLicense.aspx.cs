﻿using System;
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

public partial class SiteLicense : System.Web.UI.Page
{

    protected void Page_Load(object sender, EventArgs e)
    {
        if (Utils.GetSessionString("Contact_ID") != null)
        {
            if ((Utils.GetSessionString("IsSiteLicenseAdm") == "1") || (Utils.GetSessionString("IsAdm") == "True"))
            {
                if (!IsPostBack)
                {
                    System.Web.UI.WebControls.Label lbl = (Label)Master.FindControl("lblPageTitle");
                    if (lbl != null)
                    {
                        lbl.Text = "Site license details";
                    }


                    Telerik.Web.UI.RadTabStrip radTs = (Telerik.Web.UI.RadTabStrip)Master.FindControl("rtsMenu");
                    if (radTs != null)
                    {
                        radTs.SelectedIndex = 6;
                        radTs.Tabs[6].Tabs[0].Selected = true;
                        radTs.Tabs[6].Tabs[3].Width = 650;
                        radTs.Tabs[6].Tabs[2].Visible = false;
                        if (Utils.GetSessionString("IsAdm") == "True")
                        {
                            radTs.Tabs[6].Tabs[1].Visible = true;
                            radTs.Tabs[6].Tabs[3].Width = 600;
                        }
                    }
                    System.Web.UI.WebControls.Label lbl1 = (Label)Master.FindControl("lblHeadingText");
                    if (lbl1 != null)
                    {
                        lbl1.Text = "This utility will copy a codeset across and within reviews.";
                    }


                    string selectedSiteLicenseID = "0";
                    
                    if ((Utils.GetSessionString("IsSiteLicenseAdm") != "1") && (Utils.GetSessionString("IsAdm") == "True"))
                    {
                        // not a site license admin but they are ER4 admin (i.e. J or S or J) so we don't know what data to 
                        // display. Tell the IsAdm=true person (i.e. J or S or J) to go to the setup page!
                        pnlMessage.Visible = true;
                    }
                    else // the person is a site license adm (and could also be an ER4 admin)
                    {

                        // load help text
                        bool isAdmDB = true;
                        IDataReader idr0 = Utils.GetReader(isAdmDB, "st_GetHelpText", "HELP: Using the Site License");
                        while (idr0.Read())
                        {
                            lblLicenseDetailsHelp.Text = idr0["EMAIL_MESSAGE"].ToString();
                        }
                        idr0.Close();

                        // check how many site licenses this person is an administrator of
                        isAdmDB = true;
                        DataTable dt2 = new DataTable();
                        System.Data.DataRow newrow2;
                        dt2.Columns.Add(new DataColumn("SITE_LIC_ID", typeof(string)));
                        dt2.Columns.Add(new DataColumn("SITE_LIC_NAME", typeof(string)));
                        IDataReader idr1 = Utils.GetReader(isAdmDB, "st_Site_Lic_Get_By_Admin",
                                Utils.GetSessionString("Contact_ID"));
                        while (idr1.Read())
                        {
                            newrow2 = dt2.NewRow();
                            newrow2["SITE_LIC_ID"] = idr1["SITE_LIC_ID"].ToString();
                            newrow2["SITE_LIC_NAME"] = idr1["SITE_LIC_NAME"].ToString();
                            dt2.Rows.Add(newrow2);
                        }
                        idr1.Close();
                        ddlYourSiteLicenses.DataSource = dt2;
                        ddlYourSiteLicenses.DataBind();

                        if (ddlYourSiteLicenses.Items.Count > 1)
                        {
                            pnlMultipleSiteLicense.Visible = true;
                            selectedSiteLicenseID = ddlYourSiteLicenses.SelectedValue;
                        }


                        
                        isAdmDB = true;
                        string itemToSelect = "";
                        DataTable dt1 = new DataTable();
                        System.Data.DataRow newrow1;
                        dt1.Columns.Add(new DataColumn("SITE_LIC_DETAILS_ID", typeof(string)));
                        dt1.Columns.Add(new DataColumn("DATE_CREATED", typeof(string)));
                        int counter = 0;

                        DateTime dateCreated;
                        DateTime dateOfferCreated;
                        DateTime validFrom;
                        DateTime expiryDate;


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
                            dateOfferCreated = Convert.ToDateTime(idr["DATE_CREATED"].ToString());
 
                            newrow1["SITE_LIC_DETAILS_ID"] = idr["SITE_LIC_DETAILS_ID"].ToString();
                            if (idr["IS_ACTIVE"].ToString() == "True")
                                newrow1["DATE_CREATED"] = "Offer - " + dateOfferCreated.ToString("dd MMM yyyy");
                            if (idr["IS_ACTIVE"].ToString() == "False")
                            {
                                if (counter == 0)
                                {
                                    newrow1["DATE_CREATED"] = "Latest - " + dateOfferCreated.ToString("dd MMM yyyy");
                                    itemToSelect = idr["SITE_LIC_DETAILS_ID"].ToString();
                                    counter += 1;
                                }
                                else
                                    newrow1["DATE_CREATED"] = "Expired - " + dateOfferCreated.ToString("dd MMM yyyy");
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
                                idr = Utils.GetReader(isAdmDB, "st_Site_Lic_Get_Details_By_ID",
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

                                dateCreated = Convert.ToDateTime(idr["DATE_CREATED"].ToString());
                                lblDateCreated.Text = dateCreated.ToString("dd MMM yyyy");
                                //lblDateCreated.Text = idr["DATE_CREATED"].ToString();

                                lblSiteLicenseDetailsID.Text = idr["SITE_LIC_DETAILS_ID"].ToString();
                                lblNumberMonths.Text = idr["MONTHS"].ToString();
                                lblNumberAccounts.Text = idr["ACCOUNTS_ALLOWANCE"].ToString();
                                lblNumberReviews.Text = idr["REVIEWS_ALLOWANCE"].ToString();
                                lblTotalFee.Text = (int.Parse(idr["PRICE_PER_MONTH"].ToString()) * int.Parse(lblNumberMonths.Text)).ToString();

                                validFrom = Convert.ToDateTime(idr["VALID_FROM"].ToString());
                                lblValidFrom.Text = validFrom.ToString("dd MMM yyyy");
                                //lblValidFrom.Text = idr["VALID_FROM"].ToString();

                                string test = idr["ALLOW_REVIEW_OWNERSHIP_CHANGE"].ToString();
                                if (idr["ALLOW_REVIEW_OWNERSHIP_CHANGE"].ToString() == "True")
                                {
                                    lblSiteLicenceID.Text = "Site license #";
                                }
                                else
                                {
                                    lblSiteLicenceID.Text = "Site license ID";
                                }

                                string test2 = idr["SITE_LIC_MODEL"].ToString();
                                if (idr["SITE_LIC_MODEL"].ToString() == "1")
                                {
                                    lblLicModel.Text = "Fixed";
                                }
                                else
                                {
                                    lblLicModel.Text = "Removeable";

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

                                    expiryDate = Convert.ToDateTime(idr["EXPIRY_DATE"].ToString());
                                    lblExpiryDate.Text = expiryDate.ToString("dd MMM yyyy");
                                    //lblExpiryDate.Text = idr["EXPIRY_DATE"].ToString();
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

                                    dateCreated = Convert.ToDateTime(idr["DATE_CREATED"].ToString());
                                    lblDateCreated.Text = dateCreated.ToString("dd MMM yyyy");
                                    //lblDateCreated.Text = idr["DATE_CREATED"].ToString();

                                    lblSiteLicenseDetailsID.Text = idr["SITE_LIC_DETAILS_ID"].ToString();
                                    lblNumberMonths.Text = idr["MONTHS"].ToString();
                                    lblNumberAccounts.Text = idr["ACCOUNTS_ALLOWANCE"].ToString();
                                    lblNumberReviews.Text = idr["REVIEWS_ALLOWANCE"].ToString();
                                    lblTotalFee.Text = (int.Parse(idr["PRICE_PER_MONTH"].ToString()) * int.Parse(lblNumberMonths.Text)).ToString();

                                    if (idr["VALID_FROM"].ToString() == "")
                                    {
                                        lblValidFrom.Text = "N/A";
                                        cmdAddAccount.Enabled = false;
                                        cmdAddReview.Enabled = false;
                                        lblDetailsHeading.Text = "There are not any activated packages associated with this license";
                                    }
                                    else
                                    {
                                        validFrom = Convert.ToDateTime(idr["VALID_FROM"].ToString());
                                        lblValidFrom.Text = validFrom.ToString("dd MMM yyyy");
                                    }

                                    /*
                                    lblValidFrom.Text = idr["VALID_FROM"].ToString();
                                    if (lblValidFrom.Text == "")
                                    {
                                        lblValidFrom.Text = "N/A";
                                        cmdAddAccount.Enabled = false;
                                        cmdAddReview.Enabled = false;
                                        lblDetailsHeading.Text = "There are not any activated packages associated with this license";
                                    }
                                    */

                                    if (idr["EXPIRY_DATE"].ToString() == "")
                                    {
                                        lblExpiryDate.Text = "N/A";
                                    }
                                    else
                                    {
                                        expiryDate = Convert.ToDateTime(idr["EXPIRY_DATE"].ToString());
                                        lblExpiryDate.Text = expiryDate.ToString("dd MMM yyyy");
                                    }

                                    /*
                                    lblExpiryDate.Text = idr["EXPIRY_DATE"].ToString();
                                    if (lblExpiryDate.Text == "")
                                        lblExpiryDate.Text = "N/A";
                                    */

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
                                //idr = Utils.GetReader(isAdmDB, "st_Site_Lic_Get_1", Utils.GetSessionString("Contact_ID"), 
                                //    selectedSiteLicenseID);
                                idr = Utils.GetReader(isAdmDB, "st_Site_Lic_Get_By_ID", Utils.GetSessionString("Contact_ID"),
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

                                dateCreated = Convert.ToDateTime(idr["DATE_CREATED"].ToString());
                                lblDateCreated.Text = dateCreated.ToString("dd MMM yyyy");
                                //lblDateCreated.Text = idr["DATE_CREATED"].ToString();
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
                    }
                    if (Utils.GetSessionString("EnableChatGPTEnabler") == "True")
                    {
                        pnlGPTcredit.Visible = true;
                        getOpenAIDetails();
                        getCreditPurchases();
                        if (lblDetailsHeading.Text == "There are no packages associated with this license")
                        {
                            ddlCreditPurchases.Enabled = false;
                        }
                        else
                        {
                            ddlCreditPurchases.Enabled = true;
                        }
                    }
                }                
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

        dt3.Columns.Add(new DataColumn("SITE_LIC_ADMIN_ID", typeof(string)));
        dt3.Columns.Add(new DataColumn("CONTACT_ID", typeof(string)));
        dt3.Columns.Add(new DataColumn("CONTACT_NAME", typeof(string)));
        dt3.Columns.Add(new DataColumn("EMAIL", typeof(string)));

        bool isAdmDB = true;
        IDataReader idr = Utils.GetReader(isAdmDB, "st_Site_Lic_Get_Accounts",
            lblSiteLicID.Text);
        while (idr.Read())
        {
            newrow = dt.NewRow();
            newrow["REVIEW_ID"] = idr["REVIEW_ID"].ToString();
            newrow["REVIEW_NAME"] = idr["REVIEW_NAME"].ToString();
            //newrow["CONTACT_NAME"] = idr["CONTACT_NAME"].ToString();
            dt.Rows.Add(newrow);
        }
        idr.NextResult();
        while (idr.Read())
        {
            newrow1 = dt1.NewRow();
            newrow1["REVIEW_ID"] = idr["REVIEW_ID"].ToString();
            newrow1["REVIEW_NAME"] = idr["REVIEW_NAME"].ToString();
            //newrow1["CONTACT_NAME"] = idr["CONTACT_NAME"].ToString();
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
            newrow3["SITE_LIC_ADMIN_ID"] = idr["SITE_LIC_ADMIN_ID"].ToString();
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

        gvLicenseAdms.DataSource = dt3;
        gvLicenseAdms.DataBind();

        gvReviewsPastLicense.DataSource = dt;
        gvReviewsPastLicense.DataBind();

        
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
                true, 0, 0, null, DataRowVersion.Default, lblSiteLicID.Text);
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
                true, 0, 0, null, DataRowVersion.Default, lblSiteLicID.Text);
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
        for (int i = 0; i < gvLicenseAdms.Rows.Count; i++)
        {
            if (gvLicenseAdms.Rows[i].Cells[3].Text.Contains(tbEmailAdm.Text))
            {
                lblAccountAdmMessage.Text = "this email is already an admin in this site license";
                lblAccountAdmMessage.Visible = true;
                lblAccountAdmMessage.ForeColor = System.Drawing.Color.Red;
                okToProceed = false;
                i = gvLicenseAdms.Rows.Count;
            }
        }

        if (okToProceed == true)
        {
            bool isAdmDB = true;
            SqlParameter[] paramList = new SqlParameter[4];
            paramList[1] = new SqlParameter("@lic_id", SqlDbType.Int, 8, ParameterDirection.Input,
                true, 0, 0, null, DataRowVersion.Default, lblSiteLicID.Text);
            paramList[0] = new SqlParameter("@admin_ID", SqlDbType.Int, 8, ParameterDirection.Input,
                true, 0, 0, null, DataRowVersion.Default, Utils.GetSessionString("Contact_ID"));
            paramList[2] = new SqlParameter("@contact_email", SqlDbType.NVarChar, 255, ParameterDirection.Input,
                true, 0, 0, null, DataRowVersion.Default, tbEmailAdm.Text);
            paramList[3] = new SqlParameter("@res", SqlDbType.Int, 8, ParameterDirection.Output,
                true, 0, 0, null, DataRowVersion.Default, "");
            //Utils.ExecuteSPWithReturnValues(isAdmDB, Server, "st_Site_Lic_Add_Remove_Admin", paramList);
            Utils.ExecuteSPWithReturnValues(isAdmDB, Server, "st_Site_Lic_Add_Remove_Admin", paramList);

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
        int index = Convert.ToInt32(e.CommandArgument);
        string email = (string)gvAccounts.DataKeys[index].Value;
        switch (e.CommandName)
        {
            case "REMOVE":
                bool isAdmDB = true;
                SqlParameter[] paramList = new SqlParameter[4];
                paramList[1] = new SqlParameter("@lic_id", SqlDbType.Int, 8, ParameterDirection.Input,
                    true, 0, 0, null, DataRowVersion.Default, lblSiteLicID.Text);
                paramList[0] = new SqlParameter("@admin_ID", SqlDbType.Int, 8, ParameterDirection.Input,
                    true, 0, 0, null, DataRowVersion.Default, Utils.GetSessionString("Contact_ID"));
                paramList[2] = new SqlParameter("@contact_email", SqlDbType.NVarChar, 255, ParameterDirection.Input,
                    true, 0, 0, null, DataRowVersion.Default, email);
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
    protected void gvLicenseAdms_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        int index = Convert.ToInt32(e.CommandArgument);
        //string email = (string)gvLicenseAdms.DataKeys[index].Value;
        string site_lic_admin_id = (string)gvLicenseAdms.DataKeys[index].Value;
        switch (e.CommandName)
        {
            case "REMOVE":
                if (gvLicenseAdms.Rows.Count > 1)
                {
                    bool isAdmDB = true;
                    Utils.ExecuteSP(isAdmDB, Server, "st_Site_Lic_Remove_Admin", site_lic_admin_id);
                    buildGrids();
                    lblAccountAdmMessage.Visible = false;
                    tbEmailAdm.Text = "Enter email address";

                    /*
                    SqlParameter[] paramList = new SqlParameter[4];
                    paramList[1] = new SqlParameter("@lic_id", SqlDbType.Int, 8, ParameterDirection.Input,
                        true, 0, 0, null, DataRowVersion.Default, lblSiteLicID.Text);
                    paramList[0] = new SqlParameter("@admin_ID", SqlDbType.Int, 8, ParameterDirection.Input,
                        true, 0, 0, null, DataRowVersion.Default, Utils.GetSessionString("Contact_ID"));
                    paramList[2] = new SqlParameter("@contact_email", SqlDbType.NVarChar, 255, ParameterDirection.Input,
                        true, 0, 0, null, DataRowVersion.Default, email);
                    paramList[3] = new SqlParameter("@res", SqlDbType.Int, 8, ParameterDirection.Output,
                        true, 0, 0, null, DataRowVersion.Default, "");
                    Utils.ExecuteSPWithReturnValues(isAdmDB, Server, "st_Site_Lic_Add_Remove_Admin", paramList);

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
                    */
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
            LinkButton lb = (LinkButton)(e.Row.Cells[4].Controls[0]);
            lb.Attributes.Add("onclick", "if (confirm('Are you sure you want to remove this admin from the license?') == false) return false;");
        }
    }

    protected void ddlPackages_SelectedIndexChanged(object sender, EventArgs e)
    {
        cmdAddAccount.Enabled = true;
        cmdAddReview.Enabled = true;
        lblExpiryDate.Font.Bold = false;
        lblExpiryDate.BackColor = ColorTranslator.FromHtml("#E2E9EF");
        DateTime validFrom;
        DateTime expiryDate;
        bool isAdmDB = true;
        IDataReader idr = Utils.GetReader(isAdmDB, "st_Site_Lic_Get_Package", ddlPackages.SelectedValue);
        while (idr.Read())
        {
            lblSiteLicenseDetailsID.Text = idr["SITE_LIC_DETAILS_ID"].ToString();
            lblNumberMonths.Text = idr["MONTHS"].ToString();
            lblNumberAccounts.Text = idr["ACCOUNTS_ALLOWANCE"].ToString();
            lblNumberReviews.Text = idr["REVIEWS_ALLOWANCE"].ToString();
            lblTotalFee.Text = (int.Parse(idr["PRICE_PER_MONTH"].ToString()) * 
                int.Parse(idr["MONTHS"].ToString())).ToString();
            if (idr["IS_ACTIVE"].ToString() == "False")
            {

                validFrom = Convert.ToDateTime(idr["VALID_FROM"].ToString());
                lblValidFrom.Text = validFrom.ToString("dd MMM yyyy");
                //lblValidFrom.Text = idr["VALID_FROM"].ToString();

                if (ddlPackages.SelectedItem.Text.StartsWith("Expired"))
                {
                    lblExpiryDate.Text = "Expired";
                    lblExpiryDate.BackColor = Color.Pink;
                    lblExpiryDate.Font.Bold = true;
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
                    }
                    else
                    {
                        lblExpiryDate.BackColor = ColorTranslator.FromHtml("#E2E9EF");
                        lblExpiryDate.Font.Bold = false;
                    }

                    expiryDate = Convert.ToDateTime(idr["EXPIRY_DATE"].ToString());
                    lblExpiryDate.Text = expiryDate.ToString("dd MMM yyyy");
                    //lblExpiryDate.Text = idr["EXPIRY_DATE"].ToString();
                }
                lblIsActivated.Text = "Yes";
            }
            else
            {
                lblValidFrom.Text = "N/A";
                lblExpiryDate.Text = "N/A";
                lblIsActivated.Text = "No";
                cmdAddAccount.Enabled = false;
                cmdAddReview.Enabled = false;
            }

            buildGrids();
        }
        idr.Close();

        // if looking at an expired package disable adding reviews and accounts
        if (ddlPackages.SelectedItem.Text.StartsWith("Expired"))
        {
            cmdAddAccount.Enabled = false;
            cmdAddReview.Enabled = false;
        }

    }
    protected void lbPackageHistory_Click(object sender, EventArgs e)
    {
        if (lbPackageHistory.Text == "View")
        {
            lbPackageHistory.Text = "Hide";
            gvLicenseHistory.Visible = true;
            buildLicenseLogGrid();
        }
        else
        {
            lbPackageHistory.Text = "View";
            gvLicenseHistory.Visible = false;
        }
    }
    protected void cmdSaveLicense_Click(object sender, EventArgs e)
    {
        lblLicenseMessage.Visible = false;
        lblLicenseMessage.Text = "Required fields *";
        lblLicenseMessage.ForeColor = System.Drawing.Color.Black;
        lblLicenseMessage.Font.Bold = false;
        bool licenseConditionsMet = true;

        if ((tbOrganisation.Text == "") || (tbAddress.Text == "") || (tbTelephone.Text == ""))
        {
            lblLicenseMessage.Visible = true;
            lblLicenseMessage.Text = "Please fill in all of the required fields *";
            lblLicenseMessage.ForeColor = System.Drawing.Color.Red;
            lblLicenseMessage.Font.Bold = true;
            licenseConditionsMet = false;
        }

        if (licenseConditionsMet == true)
        {
            bool isAdmDB = true;

            Utils.ExecuteSP(isAdmDB, Server, "st_Site_Lic_Edit_By_LicAdm",
              lblSiteLicID.Text, tbOrganisation.Text, tbAddress.Text, 
              tbTelephone.Text, tbNotes.Text);
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
        IDataReader idr = Utils.GetReader(isAdmDB, "st_Site_Lic_Get_Package_Log", lblSiteLicenseDetailsID.Text);
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

        gvLicenseHistory.DataSource = dt;
        gvLicenseHistory.DataBind();

    }
    protected void lbExtensionHistory_Click(object sender, EventArgs e)
    {
        if (lbExtensionHistory.Text == "View")
        {
            lbExtensionHistory.Text = "Hide";
            gvExtensionHistory.Visible = true;
            buildExtensionGrid();
        }
        else
        {
            lbExtensionHistory.Text = "View";
            gvExtensionHistory.Visible = false;
        }
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
            lblSiteLicenseDetailsID.Text);
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

        gvExtensionHistory.DataSource = dt;
        gvExtensionHistory.DataBind();
    }

    protected void ddlYourSiteLicenses_SelectedIndexChanged1(object sender, EventArgs e)
    {
        lblDetailsHeading.Text = "Packages";
        string selectedSiteLicenseID = ddlYourSiteLicenses.SelectedValue;
        bool isAdmDB = true;
        string itemToSelect = "";
        DateTime validFrom;
        DateTime expiryDate;
        DateTime dateOfferCreated;

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
            dateOfferCreated = Convert.ToDateTime(idr["DATE_CREATED"].ToString());
            newrow1["SITE_LIC_DETAILS_ID"] = idr["SITE_LIC_DETAILS_ID"].ToString();
            if (idr["IS_ACTIVE"].ToString() == "True")
                newrow1["DATE_CREATED"] = "Offer - " + dateOfferCreated.ToString("dd MMM yyyy");
            if (idr["IS_ACTIVE"].ToString() == "False")
            {
                if (counter == 0)
                {
                    newrow1["DATE_CREATED"] = "Latest - " + dateOfferCreated.ToString("dd MMM yyyy");
                    itemToSelect = idr["SITE_LIC_DETAILS_ID"].ToString();
                    counter += 1;
                }
                else
                    newrow1["DATE_CREATED"] = "Expired - " + dateOfferCreated.ToString("dd MMM yyyy");
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
                idr = Utils.GetReader(isAdmDB, "st_Site_Lic_Get_Details_By_ID",
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

                validFrom = Convert.ToDateTime(idr["VALID_FROM"].ToString());
                lblValidFrom.Text = validFrom.ToString("dd MMM yyyy");
                //lblValidFrom.Text = idr["VALID_FROM"].ToString();

                string test = idr["ALLOW_REVIEW_OWNERSHIP_CHANGE"].ToString();
                if (idr["ALLOW_REVIEW_OWNERSHIP_CHANGE"].ToString() == "True")
                {
                    lblSiteLicenceID.Text = "Site license #";
                }
                else
                {
                    lblSiteLicenceID.Text = "Site license ID";
                }

                string test2 = idr["SITE_LIC_MODEL"].ToString();
                if (idr["SITE_LIC_MODEL"].ToString() == "1")
                {
                    lblLicModel.Text = "Fixed";
                }
                else
                {
                    lblLicModel.Text = "Removeable";

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
                    expiryDate = Convert.ToDateTime(idr["EXPIRY_DATE"].ToString());
                    lblExpiryDate.Text = expiryDate.ToString("dd MMM yyyy");
                    //lblExpiryDate.Text = idr["EXPIRY_DATE"].ToString();
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


                    if (idr["VALID_FROM"].ToString() == "")
                    {
                        lblValidFrom.Text = "N/A";
                        cmdAddAccount.Enabled = false;
                        cmdAddReview.Enabled = false;
                        lblDetailsHeading.Text = "There are not any activated packages associated with this license";
                    }
                    else
                    {
                        validFrom = Convert.ToDateTime(idr["VALID_FROM"].ToString());
                        lblValidFrom.Text = validFrom.ToString("dd MMM yyyy");
                    }

                    /*
                    lblValidFrom.Text = idr["VALID_FROM"].ToString();
                    if (lblValidFrom.Text == "")
                    {
                        lblValidFrom.Text = "N/A";
                        cmdAddAccount.Enabled = false;
                        cmdAddReview.Enabled = false;
                        lblDetailsHeading.Text = "There are not any activated packages associated with this license";
                    }
                    */

                    if (idr["EXPIRY_DATE"].ToString() == "")
                    {
                        lblExpiryDate.Text = "N/A";
                    }
                    else
                    {
                        expiryDate = Convert.ToDateTime(idr["EXPIRY_DATE"].ToString());
                        lblExpiryDate.Text = expiryDate.ToString("dd MMM yyyy");
                    }
                    /*
                    lblExpiryDate.Text = idr["EXPIRY_DATE"].ToString();
                    if (lblExpiryDate.Text == "")
                        lblExpiryDate.Text = "N/A";
                    */

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
                idr = Utils.GetReader(isAdmDB, "st_Site_Lic_Get_By_ID", Utils.GetSessionString("Contact_ID"),
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

        if (Utils.GetSessionString("EnableChatGPTEnabler") == "True")
        { 
            pnlGPTcredit.Visible = true;
            getOpenAIDetails();
            getCreditPurchases();
            if (lblDetailsHeading.Text == "There are no packages associated with this license")
            {
                ddlCreditPurchases.Enabled = false;
            }
            else
            {
                ddlCreditPurchases.Enabled = true;
            }                
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
    }
    protected void lbShowBLCodes_Click(object sender, EventArgs e)
    {
        if (lbShowBLCodes.Text == "Show/Edit")
        {
            pnlBriLibCodes.Visible = true;
            lbShowBLCodes.Text = "Hide";

            bool isAdmDB = true;
            IDataReader idr1 = Utils.GetReader(isAdmDB, "st_BritishLibraryValuesGetFromLicense",
                lblSiteLicID.Text);
            if (idr1.Read())
            {
                tbBritLibPrivilegeAccountCode.Text = idr1["BL_ACCOUNT_CODE"].ToString();
                tbBritLibPrivilegeAuthCode.Text = idr1["BL_AUTH_CODE"].ToString();
                tbBritLibPrivilegeTxLine.Text = idr1["BL_TX"].ToString();

                tbBritLibCRClearedAccountCode.Text = idr1["BL_CC_ACCOUNT_CODE"].ToString();
                tbBritLibCRClearedAuthCode.Text = idr1["BL_CC_AUTH_CODE"].ToString();
                tbBritLibCRClearedTxLine.Text = idr1["BL_CC_TX"].ToString();
            }
            idr1.Close();
        }
        else
        {
            pnlBriLibCodes.Visible = false;
            lbShowBLCodes.Text = "Show/Edit";
        }
    }
    protected void lbSaveBritLibPrivilege_Click(object sender, EventArgs e)
    {
        bool isAdmDB = true;
        Utils.ExecuteSP(isAdmDB, Server, "st_BritishLibraryValuesSetOnLicense", lblSiteLicID.Text,
            tbBritLibPrivilegeAccountCode.Text, tbBritLibPrivilegeAuthCode.Text, tbBritLibPrivilegeTxLine.Text);
    }
    protected void lbSaveBritLibCopyrightCleard_Click(object sender, EventArgs e)
    {
        bool isAdmDB = true;
        Utils.ExecuteSP(isAdmDB, Server, "st_BritishLibraryCCValuesSetOnLicense", lblSiteLicID.Text,
            tbBritLibCRClearedAccountCode.Text, tbBritLibCRClearedAuthCode.Text, tbBritLibCRClearedTxLine.Text);
    }

    protected void gvReviews_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            LinkButton lb = (LinkButton)(e.Row.Cells[3].Controls[0]);
            lb.Attributes.Add("onclick", "if (confirm('Are you sure you want to remove this review from the package?') == false) return false;");


            LinkButton reviewOwner = (LinkButton) e.Row.Cells[2].FindControl("lbReviewOwner");

            bool isAdmDB = true;
            IDataReader idr = Utils.GetReader(isAdmDB, "st_GetReviewOwner", e.Row.Cells[0].Text);
            if (idr.Read())
            {
                reviewOwner.Text = idr["CONTACT_NAME"].ToString();
            }
            idr.Close();
            reviewOwner.Attributes.Add("onclick", "JavaScript:openContactList('" + e.Row.Cells[0].Text + " - Please select from site license members')");
            Utils.SetSessionString("siteLicenseID", lblSiteLicID.Text);

            if (lblSiteLicenceID.Text == "Site license #")
            {
                //e.Row.Cells[2].Visible = true;
                this.gvReviews.Columns[2].Visible = true;
            }
            else
            {
                this.gvReviews.Columns[2].Visible = false;
            }

            if (lblLicModel.Text == "Removeable")
            {
                this.gvReviews.Columns[3].Visible = true;
            }

        }
    }

    protected void gvReviewsPastLicense_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            LinkButton reviewOwner = (LinkButton)e.Row.Cells[2].FindControl("lbReviewOwnerPast");

            bool isAdmDB = true;
            IDataReader idr = Utils.GetReader(isAdmDB, "st_GetReviewOwner", e.Row.Cells[0].Text);
            if (idr.Read())
            {
                reviewOwner.Text = idr["CONTACT_NAME"].ToString();
            }
            idr.Close();
            reviewOwner.Attributes.Add("onclick", "JavaScript:openContactList('" + e.Row.Cells[0].Text + " - Please select from site license members')");
            Utils.SetSessionString("siteLicenseID", lblSiteLicID.Text);

            if (lblSiteLicenceID.Text == "Site license #")
            {
                //e.Row.Cells[2].Visible = true;
                this.gvReviewsPastLicense.Columns[2].Visible = true;
            }
            else
            {
                this.gvReviewsPastLicense.Columns[2].Visible = false;
            }

            if (lblLicModel.Text == "Removeable")
            {
                this.gvReviews.Columns[3].Visible = true;
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



    protected void gvReviews_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        lblReviewMsg.Visible = false;
        lblReviewMsg.ForeColor = System.Drawing.Color.Black;
        int index = Convert.ToInt32(e.CommandArgument);
        string reviewID = (string)gvReviews.DataKeys[index].Value;
        switch (e.CommandName)
        {
            case "REMOVE":
                bool isAdmDB = true;
                SqlParameter[] paramList = new SqlParameter[4];
                paramList[1] = new SqlParameter("@lic_id", SqlDbType.Int, 8, ParameterDirection.Input,
                    true, 0, 0, null, DataRowVersion.Default, lblSiteLicID.Text);
                paramList[0] = new SqlParameter("@admin_ID", SqlDbType.Int, 8, ParameterDirection.Input,
                    true, 0, 0, null, DataRowVersion.Default, Utils.GetSessionString("Contact_ID"));
                paramList[2] = new SqlParameter("@review_id", SqlDbType.NVarChar, 255, ParameterDirection.Input,
                    true, 0, 0, null, DataRowVersion.Default, reviewID);
                paramList[3] = new SqlParameter("@res", SqlDbType.Int, 8, ParameterDirection.Output,
                    true, 0, 0, null, DataRowVersion.Default, "");
                Utils.ExecuteSPWithReturnValues(isAdmDB, Server, "st_Site_Lic_Remove_Review", paramList);

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


    protected void gvCreditForRobots_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        bool isAdmDB = true;
        int index = Convert.ToInt32(e.CommandArgument);
        string creditForRobotsID = (string)gvCreditForRobots.DataKeys[index].Value;
        switch (e.CommandName)
        {
            case "REMOVE":
                isAdmDB = true;
                Utils.ExecuteSP(isAdmDB, Server, "st_RemoveCreditPurchaseIDForOpenAI", creditForRobotsID,
                    Utils.GetSessionString("Contact_ID"), 0, lblSiteLicID.Text);
                getOpenAIDetails();
                /*if (lbLicenseHistory.Text == "Hide")
                {
                    buildLicenseLogGrid();
                }*/
                break;

            default:
                break;
        }
    }

    protected void gvCreditForRobots_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            LinkButton lb = (LinkButton)(e.Row.Cells[4].Controls[0]);
            lb.Attributes.Add("onclick", "if (confirm('Are you sure you want to remove this CreditID from this review?') == false) return false;");
        }
    }

    private void getOpenAIDetails()
    {
        string contact_name = "";
        string contact_id = "";

        DataTable dt = new DataTable();
        System.Data.DataRow newrow;

        dt.Columns.Add(new DataColumn("CREDIT_FOR_ROBOTS_ID", typeof(string)));
        dt.Columns.Add(new DataColumn("CREDIT_PURCHASE_ID", typeof(string)));
        dt.Columns.Add(new DataColumn("CREDIT_PURCHASER", typeof(string)));
        dt.Columns.Add(new DataColumn("REMAINING", typeof(string)));

        bool isAdmDB = true;
        IDataReader idr = Utils.GetReader(isAdmDB, "st_GetCreditPurchaseIDsForOpenAI",
            "0", lblSiteLicID.Text);
        while (idr.Read())
        {
            newrow = dt.NewRow();
            newrow["CREDIT_FOR_ROBOTS_ID"] = idr["tv_credit_for_robots_id"].ToString();
            newrow["CREDIT_PURCHASE_ID"] = idr["tv_credit_purchase_id"].ToString();

            contact_name = idr["tv_credit_purchaser_contact_name"].ToString();
            contact_id = idr["tv_credit_purchaser_contact_id"].ToString();
            newrow["CREDIT_PURCHASER"] = contact_name + " (" + contact_id + ")";

            newrow["REMAINING"] = "£" + idr["tv_remaining"].ToString();
            dt.Rows.Add(newrow);
        }
        idr.Close();

        gvCreditForRobots.DataSource = dt;
        gvCreditForRobots.DataBind();

    }

    protected void lbSavePurchaseCreditID_Click(object sender, EventArgs e)
    {
        if ((tbCreditPurchaseID.Text.Trim() != "") && (Utils.IsNumeric(tbCreditPurchaseID.Text) == true))
        {

            bool isAdmDB = true;
            SqlParameter[] paramList = new SqlParameter[5];
            paramList[0] = new SqlParameter("@CREDIT_PURCHASE_ID", SqlDbType.Int, 8, ParameterDirection.Input,
                true, 0, 0, null, DataRowVersion.Default, tbCreditPurchaseID.Text.Trim());
            paramList[1] = new SqlParameter("@REVIEW_ID", SqlDbType.Int, 8, ParameterDirection.Input,
                true, 0, 0, null, DataRowVersion.Default, 0);
            paramList[2] = new SqlParameter("@LICENSE_ID", SqlDbType.Int, 8, ParameterDirection.Input,
                true, 0, 0, null, DataRowVersion.Default, lblSiteLicID.Text);
            paramList[3] = new SqlParameter("@CONTACT_ID", SqlDbType.Int, 8, ParameterDirection.Input,
                true, 0, 0, null, DataRowVersion.Default, Utils.GetSessionString("Contact_ID"));
            paramList[4] = new SqlParameter("@RESULT", SqlDbType.NVarChar, 100, ParameterDirection.Output,
                true, 0, 0, null, DataRowVersion.Default, "");


            Utils.ExecuteSPWithReturnValues(isAdmDB, Server, "st_SetCreditPurchaseIDForOpenAIByPurchaserID", paramList);

            if (paramList[4].Value.ToString() == "SUCCESS")
            {
                getOpenAIDetails();
                lblInvalidID.Visible = false;
                tbCreditPurchaseID.Text = "";
                /*if (lbLicenseHistory.Text == "Hide")
                {
                    buildLicenseLogGrid();
                }*/
            }
            else
            {
                lblInvalidID.Visible = true;
            }
        }
    }

    private void getCreditPurchases()
    {
        string remaining = "";
        string purchaseID = "";
        bool isAdmDB = true;
        DataTable dt2 = new DataTable();
        System.Data.DataRow newrow2;
        dt2.Columns.Add(new DataColumn("CREDIT_PURCHASE_ID", typeof(string)));
        dt2.Columns.Add(new DataColumn("CREDIT_ID_REMAINING", typeof(string)));

        newrow2 = dt2.NewRow();
        newrow2["CREDIT_PURCHASE_ID"] = "0";
        newrow2["CREDIT_ID_REMAINING"] = "PurchaseID - (£ Remaining)";
        dt2.Rows.Add(newrow2);

        IDataReader idr1 = Utils.GetReader(isAdmDB, "st_CreditPurchasesByPurchaser", Utils.GetSessionString("Contact_ID"));
        while (idr1.Read())
        {
            newrow2 = dt2.NewRow();
            newrow2["CREDIT_PURCHASE_ID"] = idr1["tv_credit_purchase_id"].ToString();
            purchaseID = idr1["tv_credit_purchase_id"].ToString();
            remaining = idr1["tv_credit_remaining"].ToString();
            if (remaining == "")
                remaining = idr1["tb_credit_purchased"].ToString();
            newrow2["CREDIT_ID_REMAINING"] = "PurchaseID: " + purchaseID + " - (£" + remaining + ")";
            dt2.Rows.Add(newrow2);
        }
        idr1.Close();
        ddlCreditPurchases.DataSource = dt2;
        ddlCreditPurchases.DataBind();
        ddlCreditPurchases.Attributes["onchange"] = "DisableControls();";
    }


    protected void ddlCreditPurchases_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (ddlCreditPurchases.SelectedIndex != 0)
        {
            bool isAdmDB = true;
            SqlParameter[] paramList = new SqlParameter[5];
            paramList[0] = new SqlParameter("@CREDIT_PURCHASE_ID", SqlDbType.Int, 8, ParameterDirection.Input,
                true, 0, 0, null, DataRowVersion.Default, ddlCreditPurchases.SelectedValue);
            paramList[1] = new SqlParameter("@REVIEW_ID", SqlDbType.Int, 8, ParameterDirection.Input,
                true, 0, 0, null, DataRowVersion.Default, 0);
            paramList[2] = new SqlParameter("@LICENSE_ID", SqlDbType.Int, 8, ParameterDirection.Input,
                true, 0, 0, null, DataRowVersion.Default, lblSiteLicID.Text);
            paramList[3] = new SqlParameter("@CONTACT_ID", SqlDbType.Int, 8, ParameterDirection.Input,
                true, 0, 0, null, DataRowVersion.Default, Utils.GetSessionString("Contact_ID"));
            paramList[4] = new SqlParameter("@RESULT", SqlDbType.NVarChar, 100, ParameterDirection.Output,
                true, 0, 0, null, DataRowVersion.Default, "");

            //System.Threading.Thread.Sleep(5000);
            Utils.ExecuteSPWithReturnValues(isAdmDB, Server, "st_SetCreditPurchaseIDForOpenAIByPurchaserID", paramList);

            if (paramList[4].Value.ToString() == "SUCCESS")
            {
                ddlCreditPurchases.SelectedValue = ddlCreditPurchases.Items[0].Value;
                getOpenAIDetails();
            }
            else
            {
                // not much to do if it fails
            }
        }
    }
}
