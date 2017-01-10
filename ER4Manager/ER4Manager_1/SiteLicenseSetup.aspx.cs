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

public partial class SiteLicenseSetup : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (Utils.GetSessionString("Contact_ID") != null)
        {
            if (Utils.GetSessionString("IsAdm") == "True")
            {
                if (!IsPostBack)
                {
                    Utils.SetSessionString("siteLicenseID", "0");

                    System.Web.UI.WebControls.Label lbl = (Label)Master.FindControl("lblPageTitle");
                    if (lbl != null)
                    {
                        lbl.Text = "Site license setup";
                    }

                    Telerik.Web.UI.RadTabStrip radTs = (Telerik.Web.UI.RadTabStrip)Master.FindControl("rtsMenu");
                    if (radTs != null)
                    {
                        radTs.SelectedIndex = 6;
                        radTs.Tabs[6].Tabs[1].Selected = true;
                        radTs.Tabs[6].Tabs[2].Width = 670;
                        if (Utils.GetSessionString("IsAdm") == "True")
                        {
                            radTs.Tabs[6].Tabs[1].Visible = true;
                            radTs.Tabs[6].Tabs[2].Width = 570;
                        }
                    }

                    buildLicenseGrid(0);
                    //buildLicenseGrid2(0);
                    //buildRadGrid();
                }

                IBCalendar1.Attributes.Add("onclick", "JavaScript:openCalendar1('" +
                    "!1!" + tbDateLicenseCreated.Text + "')");
                IBCalendar2.Attributes.Add("onclick", "JavaScript:openCalendar1('" +
                    "!2!" + tbValidFrom.Text + "')");
                IBCalendar3.Attributes.Add("onclick", "JavaScript:openCalendar1('" +
                    "!3!" + tbExpiryDate.Text + "')");
                IBCalendar4.Attributes.Add("onclick", "JavaScript:openCalendar1('" +
                    "!4!" + tbDatePackageCreated.Text + "')");

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

    private void buildLicenseGrid(int page)
    {
        DataTable dt = new DataTable();
        System.Data.DataRow newrow;
        dt.Columns.Add(new DataColumn("CONTACT_ID", typeof(string)));
        dt.Columns.Add(new DataColumn("CONTACT_NAME", typeof(string)));
        dt.Columns.Add(new DataColumn("SITE_LIC_ID", typeof(string)));
        dt.Columns.Add(new DataColumn("SITE_LIC_NAME", typeof(string)));
        dt.Columns.Add(new DataColumn("MONTHS", typeof(string)));
        dt.Columns.Add(new DataColumn("EXPIRY_DATE", typeof(string)));

        bool isAdmDB = true;
        IDataReader idr = Utils.GetReader(isAdmDB, "st_Site_Lic_Get_All");
        while (idr.Read())
        {
            newrow = dt.NewRow();
            newrow["CONTACT_NAME"] = idr["CONTACT_NAME"].ToString();
            newrow["CONTACT_ID"] = idr["CONTACT_ID"].ToString(); // not visible
            newrow["SITE_LIC_ID"] = idr["SITE_LIC_ID"].ToString();
            newrow["SITE_LIC_NAME"] = idr["SITE_LIC_NAME"].ToString();
            //newrow["MONTHS"] = idr["MONTHS"].ToString();
            newrow["MONTHS"] = "";
            newrow["EXPIRY_DATE"] = idr["EXPIRY_DATE"].ToString();
            dt.Rows.Add(newrow);
        }
        idr.Close();

        gvSiteLicenses.DataSource = dt;
        gvSiteLicenses.PageIndex = page;
        gvSiteLicenses.DataBind();
    }

    /*
    private void buildLicenseGrid2(int pageNumber)
    {
        DataTable dt = new DataTable();
        System.Data.DataRow newrow;
        dt.Columns.Add(new DataColumn("CONTACT_ID", typeof(string)));
        dt.Columns.Add(new DataColumn("CONTACT_NAME", typeof(string)));
        dt.Columns.Add(new DataColumn("SITE_LIC_ID", typeof(string)));
        dt.Columns.Add(new DataColumn("SITE_LIC_NAME", typeof(string)));
        dt.Columns.Add(new DataColumn("MONTHS", typeof(string)));
        dt.Columns.Add(new DataColumn("EXPIRY_DATE", typeof(string)));

        bool isAdmDB = true;
        IDataReader idr = Utils.GetReader(isAdmDB, "st_Site_Lic_Get_All");
        while (idr.Read())
        {
            newrow = dt.NewRow();
            newrow["CONTACT_NAME"] = idr["CONTACT_NAME"].ToString();
            newrow["CONTACT_ID"] = idr["CONTACT_ID"].ToString(); // not visible
            newrow["SITE_LIC_ID"] = idr["SITE_LIC_ID"].ToString();
            newrow["SITE_LIC_NAME"] = idr["SITE_LIC_NAME"].ToString();
            //newrow["MONTHS"] = idr["MONTHS"].ToString();
            newrow["MONTHS"] = "";
            newrow["EXPIRY_DATE"] = idr["EXPIRY_DATE"].ToString();
            dt.Rows.Add(newrow);
        }
        idr.Close();

        gvSiteLicenses0.DataSource = dt;
        gvSiteLicenses0.PageIndex = pageNumber;
        gvSiteLicenses0.DataBind();
    }
    private void buildRadGrid()
    {
        DataTable dt = new DataTable();
        System.Data.DataRow newrow;

        dt.Columns.Add(new DataColumn("CONTACT_ID", typeof(string)));
        dt.Columns.Add(new DataColumn("CONTACT_NAME", typeof(string)));
        dt.Columns.Add(new DataColumn("SITE_LIC_ID", typeof(string)));
        dt.Columns.Add(new DataColumn("SITE_LIC_NAME", typeof(string)));
        dt.Columns.Add(new DataColumn("EXPIRY_DATE", typeof(string)));

        bool isAdmDB = true;
        IDataReader idr = Utils.GetReader(isAdmDB, "st_Site_Lic_Get_All");
        while (idr.Read())
        {
            newrow = dt.NewRow();
            newrow["CONTACT_NAME"] = idr["CONTACT_NAME"].ToString();
            newrow["CONTACT_ID"] = idr["CONTACT_ID"].ToString(); // not visible
            newrow["SITE_LIC_ID"] = idr["SITE_LIC_ID"].ToString();
            newrow["SITE_LIC_NAME"] = idr["SITE_LIC_NAME"].ToString();
            newrow["EXPIRY_DATE"] = idr["EXPIRY_DATE"].ToString();
            dt.Rows.Add(newrow);
        }
        idr.Close();
    }
    */
    protected void gvSiteLicenses_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        pnlSiteLicense.Visible = false;
        pnlLicenseDetails.Visible = false;
        pnlAccountsAndReviews.Visible = false;

        string itemToSelect = "";
        int index = Convert.ToInt32(e.CommandArgument);
        string adminID = (string)gvSiteLicenses.DataKeys[index].Value;
        string siteLicID = gvSiteLicenses.Rows[index].Cells[0].Text;
        switch (e.CommandName)
        {
            case "EDT":
                bool isAdmDB = true;

                DataTable dt1 = new DataTable();
                System.Data.DataRow newrow1;
                dt1.Columns.Add(new DataColumn("SITE_LIC_DETAILS_ID", typeof(string)));
                dt1.Columns.Add(new DataColumn("DATE_CREATED", typeof(string)));
                int counter = 0;

                IDataReader idr = Utils.GetReader(isAdmDB, "st_Site_Lic_Get_all_Packages", siteLicID, "0");
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
                if (ddlPackages.Items.Count != 0)
                {
                    if (itemToSelect != "")
                        ddlPackages.SelectedValue = itemToSelect;
                    else
                        ddlPackages.SelectedIndex = 0;
                }


                if (ddlPackages.Items.Count != 0)
                {
                    // license details are available
                    pnlSiteLicense.Visible = true;
                    pnlLicenseDetails.Visible = true;
                    pnlAccountsAndReviews.Visible = true;
                    lbLicenseHistory.Enabled = true;

                    lbSelectAdmin.Enabled = false;
                    tbExpiryDate.Enabled = true;
                    IBCalendar3.Enabled = true;
                    tbValidFrom.Enabled = true;
                    IBCalendar2.Enabled = true;
                    

                    int idrCounter = 0;
                    idr = Utils.GetReader(isAdmDB, "st_Site_Lic_Get_Details_1", adminID, siteLicID);
                    if (idr.Read())
                    {
                        tbLicenseName.Text = idr["SITE_LIC_NAME"].ToString();
                        lblSiteLicID.Text = idr["SITE_LIC_ID"].ToString();
                        tbOrganisation.Text = idr["COMPANY_NAME"].ToString();
                        tbAddress.Text = idr["COMPANY_ADDRESS"].ToString();
                        tbTelephone.Text = idr["TELEPHONE"].ToString();
                        tbNotes.Text = idr["NOTES"].ToString();
                        tbDateLicenseCreated.Text = idr["DATE_CREATED"].ToString();
                        lblCreatedBy.Text = idr["CREATOR_ID"].ToString();
                        lblInitialAdministrator.Text = idr["ADMIN_NAME"].ToString();
                        lblAdminID.Text = adminID;

                        string test22 = idr["ALLOW_REVIEW_OWNERSHIP_CHANGE"].ToString();

                        if (idr["ALLOW_REVIEW_OWNERSHIP_CHANGE"].ToString() == "True")
                            cbAllowReviewOwnershipChange.Checked = true;
                        else
                            cbAllowReviewOwnershipChange.Checked = false;

                        lblSiteLicenseDetailsID.Text = idr["SITE_LIC_DETAILS_ID"].ToString();
                        tbNumberMonths.Text = idr["MONTHS"].ToString();
                        tbNumberAccounts.Text = idr["ACCOUNTS_ALLOWANCE"].ToString();
                        tbNumberReviews.Text = idr["REVIEWS_ALLOWANCE"].ToString();
                        lblPricePerMonth.Text = idr["PRICE_PER_MONTH"].ToString();
                        tbTotalFee.Text = (int.Parse(lblPricePerMonth.Text) * int.Parse(tbNumberMonths.Text)).ToString();
                        tbDatePackageCreated.Text = idr["DATE_PACKAGE_CREATED"].ToString();
                        tbValidFrom.Text = idr["VALID_FROM"].ToString();

                        if (ddlPackages.SelectedItem.Text.StartsWith("Expired"))
                        {
                            tbExpiryDate.Text = "Expired";
                            tbExpiryDate.BackColor = Color.Pink;
                            tbExpiryDate.Font.Bold = true;
                        }
                        else
                        {
                            // check if the latest is expired
                            string test = idr["EXPIRY_DATE"].ToString();
                            DateTime expired = Convert.ToDateTime(idr["EXPIRY_DATE"].ToString());
                            DateTime today = DateTime.Today;
                            if (expired < today)
                            {
                                tbExpiryDate.BackColor = Color.Pink;
                                tbExpiryDate.Font.Bold = true;
                            }
                            else
                            {
                                tbExpiryDate.BackColor = Color.White;
                                tbExpiryDate.Font.Bold = false;
                            }
                            tbExpiryDate.Text = idr["EXPIRY_DATE"].ToString();
                        }

                        //tbExpiryDate.Text = idr["EXPIRY_DATE"].ToString();
                        lblForSaleID.Text = idr["FOR_SALE_ID"].ToString();
                        idrCounter += 1;
                        lblIsActivated.Text = "Yes";
                        lbExtensionHistory.Enabled = true;
                    }
                    else //(idrCounter == 0)
                    {
                        // read the offers as there aren't any active packages
                        idr.NextResult();
                        if (idr.Read())
                        {
                            tbLicenseName.Text = idr["SITE_LIC_NAME"].ToString();
                            lblSiteLicID.Text = idr["SITE_LIC_ID"].ToString();
                            tbOrganisation.Text = idr["COMPANY_NAME"].ToString();
                            tbAddress.Text = idr["COMPANY_ADDRESS"].ToString();
                            tbTelephone.Text = idr["TELEPHONE"].ToString();
                            tbNotes.Text = idr["NOTES"].ToString();
                            tbDateLicenseCreated.Text = idr["DATE_CREATED"].ToString();
                            lblCreatedBy.Text = idr["CREATOR_ID"].ToString();
                            lblInitialAdministrator.Text = idr["ADMIN_NAME"].ToString();
                            lblAdminID.Text = adminID;

                            lblSiteLicenseDetailsID.Text = idr["SITE_LIC_DETAILS_ID"].ToString();
                            tbNumberMonths.Text = idr["MONTHS"].ToString();
                            tbNumberAccounts.Text = idr["ACCOUNTS_ALLOWANCE"].ToString();
                            tbNumberReviews.Text = idr["REVIEWS_ALLOWANCE"].ToString();
                            lblPricePerMonth.Text = idr["PRICE_PER_MONTH"].ToString();
                            tbTotalFee.Text = (int.Parse(lblPricePerMonth.Text) * int.Parse(tbNumberMonths.Text)).ToString();
                            tbDatePackageCreated.Text = idr["DATE_PACKAGE_CREATED"].ToString();
                            tbValidFrom.Text = idr["VALID_FROM"].ToString();
                            tbExpiryDate.Text = idr["EXPIRY_DATE"].ToString();
                            lblForSaleID.Text = idr["FOR_SALE_ID"].ToString();
                        }
                    }
                    idr.Close();

                    DataTable dt = new DataTable();
                    System.Data.DataRow newrow;
                    dt.Columns.Add(new DataColumn("EXTENSION_TYPE_ID", typeof(string)));
                    dt.Columns.Add(new DataColumn("EXTENSION_TYPE", typeof(string)));
                    newrow = dt.NewRow();
                    newrow["EXTENSION_TYPE_ID"] = "0";
                    newrow["EXTENSION_TYPE"] = "Select a type";
                    dt.Rows.Add(newrow);
                    idr = Utils.GetReader(isAdmDB, "st_ExtensionTypesGet", "SiteLic");
                    while (idr.Read())
                    {
                        newrow = dt.NewRow();
                        newrow["EXTENSION_TYPE_ID"] = idr["EXTENSION_TYPE_ID"].ToString();
                        newrow["EXTENSION_TYPE"] = idr["EXTENSION_TYPE"].ToString();
                        dt.Rows.Add(newrow);
                    }
                    idr.Close();
                    ddlExtensionType.DataSource = dt;
                    ddlExtensionType.DataBind();
                    ddlExtensionType.Enabled = true;

                    buildGrids();
                    pnlAccountsAndReviews.Visible = true;

                    lblAccountMsg.Visible = false;
                    lblAccountMsg.ForeColor = System.Drawing.Color.Black;
                    lblReviewMsg.Visible = false;
                    lblReviewMsg.ForeColor = System.Drawing.Color.Black;
                    lblAccountAdmMessage.Visible = false;
                    lblAccountAdmMessage.ForeColor = System.Drawing.Color.Black;
                }
                else
                {
                    // site license only - no details/packages available  siteLicID
                    // orginal JB Jun26
                    //idr = Utils.GetReader(isAdmDB, "st_Site_Lic_Get", adminID);
                    //
                    // added JB Jun26
                    idr = Utils.GetReader(isAdmDB, "st_Site_Lic_Get_1", adminID, siteLicID);
                    //
                    while (idr.Read())
                    {
                        tbLicenseName.Text = idr["SITE_LIC_NAME"].ToString();
                        lblSiteLicID.Text = idr["SITE_LIC_ID"].ToString();
                        tbOrganisation.Text = idr["COMPANY_NAME"].ToString();
                        tbAddress.Text = idr["COMPANY_ADDRESS"].ToString();
                        tbTelephone.Text = idr["TELEPHONE"].ToString();
                        tbNotes.Text = idr["NOTES"].ToString();
                        tbDateLicenseCreated.Text = idr["DATE_CREATED"].ToString();
                        lblCreatedBy.Text = idr["CREATOR_NAME"].ToString();
                        lblInitialAdministrator.Text = idr["ADMINISTRATOR_NAME"].ToString();
                        lblAdminID.Text = adminID;
                    }
                    idr.Close();


                    lblSiteLicenseDetailsID.Text = "N/A";
                    tbNumberMonths.Text = "";
                    tbNumberAccounts.Text = "";
                    tbNumberReviews.Text = "";
                    lblPricePerMonth.Text = "N/A";
                    tbTotalFee.Text = "";
                    tbDatePackageCreated.Text = "";
                    tbValidFrom.Text = "";
                    tbExpiryDate.Text = "";
                    lblForSaleID.Text = "N/A";

                    pnlSiteLicense.Visible = true;
                    lbSelectAdmin.Enabled = true;
                    //pnlLicenseDetails.Visible = true;
                }

                break;
            default:
                break;
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

        dt1.Columns.Add(new DataColumn("REVIEW_ID", typeof(string)));
        dt1.Columns.Add(new DataColumn("REVIEW_NAME", typeof(string)));

        dt2.Columns.Add(new DataColumn("CONTACT_ID", typeof(string)));
        dt2.Columns.Add(new DataColumn("CONTACT_NAME", typeof(string)));
        dt2.Columns.Add(new DataColumn("EMAIL", typeof(string)));

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
            dt.Rows.Add(newrow);
        }
        idr.NextResult();
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

        gvReviewsPastLicense.DataSource = dt;
        gvReviewsPastLicense.DataBind();

        gvReviews.DataSource = dt1;
        gvReviews.DataBind();

        gvAccounts.DataSource = dt2;
        gvAccounts.DataBind();
        
        gvLicenseAdms.DataSource = dt3;
        gvLicenseAdms.DataBind();

        if (gvAccounts.Rows.Count < int.Parse(tbNumberAccounts.Text))
        {
            cmdAddAccount.Enabled = true;
            tbEmail.Enabled = true;
        }
        else
        {
            cmdAddAccount.Enabled = false;
            tbEmail.Enabled = false;
        }
        if (gvReviews.Rows.Count < int.Parse(tbNumberReviews.Text))
        {
            cmdAddReview.Enabled = true;
            tbReviewID.Enabled = true;
        }
        else
        {
            cmdAddReview.Enabled = false;
            tbReviewID.Enabled = false;
        }
        

    }


    protected void lbNewSiteLicense_Click(object sender, EventArgs e)
    {
        DataTable dt = new DataTable();
        System.Data.DataRow newrow;

        dt.Columns.Add(new DataColumn("EXTENSION_TYPE_ID", typeof(string)));
        dt.Columns.Add(new DataColumn("EXTENSION_TYPE", typeof(string)));

        newrow = dt.NewRow();
        newrow["EXTENSION_TYPE_ID"] = "0";
        newrow["EXTENSION_TYPE"] = "Select a type";
        dt.Rows.Add(newrow);

        bool isAdmDB = true;
        IDataReader idr = Utils.GetReader(isAdmDB, "st_ExtensionTypesGet", "Review");
        while (idr.Read())
        {
            newrow = dt.NewRow();
            newrow["EXTENSION_TYPE_ID"] = idr["EXTENSION_TYPE_ID"].ToString();
            newrow["EXTENSION_TYPE"] = idr["EXTENSION_TYPE"].ToString();
            dt.Rows.Add(newrow);
        }
        idr.Close();
        ddlExtensionType.DataSource = dt;
        ddlExtensionType.DataBind();


        pnlSiteLicense.Visible = true;
        lbCreatePackage.Enabled = false;
        //pnlLicenseDetails.Visible = true;
    }

    protected void cmdAddAccount_Click(object sender, EventArgs e)
    {
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
            paramList[0] = new SqlParameter("@lic_id", SqlDbType.Int, 8, ParameterDirection.Input,
                true, 0, 0, null, DataRowVersion.Default, lblSiteLicID.Text);
            paramList[1] = new SqlParameter("@admin_ID", SqlDbType.Int, 8, ParameterDirection.Input,
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
            if (gvLicenseAdms.Rows[i].Cells[2].Text.Contains(tbEmailAdm.Text))
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
        lblAccountMsg.Visible = false;
        lblAccountMsg.ForeColor = System.Drawing.Color.Black;
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
            lb.Attributes.Add("onclick", "if (confirm('Are you sure you want to remove this user from the license?') == false) return false;");
        }
    }
    protected void gvLicenseAdms_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        int index = Convert.ToInt32(e.CommandArgument);
        string email = (string)gvLicenseAdms.DataKeys[index].Value;
        string contactID = gvLicenseAdms.Rows[index].Cells[0].Text;
        switch (e.CommandName)
        {
            case "REMOVE":
                if (gvLicenseAdms.Rows.Count > 1)
                {
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
            lb.Attributes.Add("onclick", "if (confirm('Are you sure you want to remove this admin from the license?') == false) return false;");
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
                tbDateLicenseCreated.Text = Utils.GetSessionString("CalendarDate").Substring(counterLocation + 1);
            }
            else if (calendarCounter == "2")
            {
                tbValidFrom.Text = Utils.GetSessionString("CalendarDate").Substring(counterLocation + 1).Substring(0, 10);
            }
            else if (calendarCounter == "3")
            {
                tbExpiryDate.Text = Utils.GetSessionString("CalendarDate").Substring(counterLocation + 1).Substring(0, 10);              
            }
            else
            {
                tbDatePackageCreated.Text = Utils.GetSessionString("CalendarDate").Substring(counterLocation + 1).Substring(0, 10);
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
        if (lbLicenseHistory.Text == "View")
        {
            lbLicenseHistory.Text = "Hide";
            gvLicenseHistory.Visible = true;
            buildLicenseLogGrid();
        }
        else
        {
            lbLicenseHistory.Text = "View";
            gvLicenseHistory.Visible = false;
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



    protected void gvSiteLicenses_RowDataBound(object sender, GridViewRowEventArgs e)
    {

    }
    protected void gvSiteLicenses_RowEditing(object sender, GridViewEditEventArgs e)
    {

    }
    protected void ddlPackages_SelectedIndexChanged(object sender, EventArgs e)
    {
        /*
        if (ddlPackages.SelectedIndex > 0)
        {
            lblPackageTitle.Text = "Earlier expired package";
            cmdSaveLicenseDetails.Enabled = false;
        }
        else
        {
            lblPackageTitle.Text = "Most recent package";
            cmdSaveLicenseDetails.Enabled = true;
        }
        */
        tbExpiryDate.BackColor = Color.White;
        tbExpiryDate.Font.Bold = false;
        bool isAdmDB = true;
        IDataReader idr = Utils.GetReader(isAdmDB, "st_Site_Lic_Get_Package", ddlPackages.SelectedValue);
        while (idr.Read())
        {
            lblSiteLicenseDetailsID.Text = idr["SITE_LIC_DETAILS_ID"].ToString();
            tbNumberMonths.Text = idr["MONTHS"].ToString();
            tbNumberAccounts.Text = idr["ACCOUNTS_ALLOWANCE"].ToString();
            tbNumberReviews.Text = idr["REVIEWS_ALLOWANCE"].ToString();
            lblPricePerMonth.Text = idr["PRICE_PER_MONTH"].ToString();
            tbTotalFee.Text = (int.Parse(lblPricePerMonth.Text) * int.Parse(tbNumberMonths.Text)).ToString();
            tbDatePackageCreated.Text = idr["DATE_CREATED"].ToString();      
            lblForSaleID.Text = idr["FOR_SALE_ID"].ToString();
            if (idr["IS_ACTIVE"].ToString() == "False")
            {
                tbValidFrom.Text = idr["VALID_FROM"].ToString();
                if (ddlPackages.SelectedItem.Text.StartsWith("Expired"))
                {
                    tbExpiryDate.Text = "Expired";
                    tbExpiryDate.BackColor = Color.Pink;
                    tbExpiryDate.Font.Bold = true;
                }
                else
                {
                    // check if the latest is expired
                    DateTime expired = Convert.ToDateTime(idr["EXPIRY_DATE"].ToString());
                    DateTime today = DateTime.Today;
                    if (expired < today)
                    {
                        tbExpiryDate.BackColor = Color.Pink;
                        tbExpiryDate.Font.Bold = true;
                    }
                    else
                    {
                        tbExpiryDate.BackColor = Color.White;
                        tbExpiryDate.Font.Bold = false;
                    }
                    tbExpiryDate.Text = idr["EXPIRY_DATE"].ToString();
                }
                lblIsActivated.Text = "Yes";
            }
            else
            {
                tbValidFrom.Text = "";
                tbExpiryDate.Text = "";
                lblIsActivated.Text = "No";
            }


            

        }
        idr.Close();
    }
    protected void gvReviews_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            LinkButton lb = (LinkButton)(e.Row.Cells[2].Controls[0]);
            lb.Attributes.Add("onclick", "if (confirm('Are you sure you want to remove this review from the package?') == false) return false;");
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

    protected void cmdSaveLicense_Click(object sender, EventArgs e)
    {
        lblLicenseMessage.Visible = false;
        lblLicenseMessage.Text = "Required fields *";
        lblLicenseMessage.ForeColor = System.Drawing.Color.Black;
        lblLicenseMessage.Font.Bold = false;
        string licenseCreationDate = "";
        bool licenseConditionsMet = true;

        if ((tbLicenseName.Text == "") || (tbOrganisation.Text == "") || (tbAddress.Text == "") ||
            (tbTelephone.Text == "") || (tbDateLicenseCreated.Text == ""))
        {
            lblLicenseMessage.Visible = true;
            lblLicenseMessage.Text = "Please fill in all of the required fields *";
            lblLicenseMessage.ForeColor = System.Drawing.Color.Red;
            lblLicenseMessage.Font.Bold = true;
            licenseConditionsMet = false;
        }
        else if (lblInitialAdministrator.Text == "N/A")
        {
            lblLicenseMessage.Visible = true;
            lblLicenseMessage.Text = "You need to choose a license administrator";
            lblLicenseMessage.ForeColor = System.Drawing.Color.Red;
            lblLicenseMessage.Font.Bold = true;
            licenseConditionsMet = false;
        }

        if (licenseConditionsMet == true)
        {  
            try
            {
                DateTime DateLicenseCreated = Convert.ToDateTime(tbDateLicenseCreated.Text);
                licenseCreationDate = DateLicenseCreated.ToString("yyyy-M-d hh:m:ss.mmm"); // this is case sensitive!
            }
            catch (Exception er)
            {
                lblLicenseMessage.Visible = true;
                lblLicenseMessage.Text = "Date created error: " + er.Message.ToString();
                lblLicenseMessage.ForeColor = System.Drawing.Color.Red;
                lblLicenseMessage.Font.Bold = true;
                licenseConditionsMet = false;
            }
        }

        if (licenseConditionsMet == true)
        {
            bool isAdmDB = true;
            SqlParameter[] paramList = new SqlParameter[10];

            paramList[0] = new SqlParameter("@LIC_ID", SqlDbType.NVarChar, 50, ParameterDirection.Input,
                    true, 0, 0, null, DataRowVersion.Default, lblSiteLicID.Text);
            paramList[1] = new SqlParameter("@creator_id", SqlDbType.NVarChar, 50, ParameterDirection.Input,
                true, 0, 0, null, DataRowVersion.Default, Utils.GetSessionString("Contact_ID"));
            paramList[2] = new SqlParameter("@admin_id", SqlDbType.NVarChar, 255, ParameterDirection.Input,
                true, 0, 0, null, DataRowVersion.Default, lblAdminID.Text);
            paramList[3] = new SqlParameter("@SITE_LIC_NAME", SqlDbType.NVarChar, 50, ParameterDirection.Input,
                    true, 0, 0, null, DataRowVersion.Default, tbLicenseName.Text);
            paramList[4] = new SqlParameter("@COMPANY_NAME", SqlDbType.NVarChar, 50, ParameterDirection.Input,
                    true, 0, 0, null, DataRowVersion.Default, tbOrganisation.Text);
            paramList[5] = new SqlParameter("@COMPANY_ADDRESS", SqlDbType.NVarChar, 500, ParameterDirection.Input,
                    true, 0, 0, null, DataRowVersion.Default, tbAddress.Text);
            paramList[6] = new SqlParameter("@TELEPHONE", SqlDbType.NVarChar, 50, ParameterDirection.Input,
                    true, 0, 0, null, DataRowVersion.Default, tbTelephone.Text);
            paramList[7] = new SqlParameter("@NOTES", SqlDbType.NVarChar, 4000, ParameterDirection.Input,
                    true, 0, 0, null, DataRowVersion.Default, tbNotes.Text);
            paramList[8] = new SqlParameter("@DATE_CREATED", SqlDbType.NVarChar, 50, ParameterDirection.Input,
                    true, 0, 0, null, DataRowVersion.Default, licenseCreationDate);
            paramList[9] = new SqlParameter("@RESULT", SqlDbType.NVarChar, 50, ParameterDirection.Output,
                true, 0, 0, null, DataRowVersion.Default, "");

            // original JB Jun26
            //Utils.ExecuteSPWithReturnValues(isAdmDB, Server, "st_Site_Lic_Create_or_Edit", paramList);
            // added JB Jun26
            Utils.ExecuteSPWithReturnValues(isAdmDB, Server, "st_Site_Lic_Create_or_Edit_1", paramList);

            if (paramList[9].Value.ToString() == "invalid adm")
            {
                lblLicenseMessage.Visible = true;
                lblLicenseMessage.Text = "The 'First adminstrator' is already a site license adminstrator. Please select another.";
                lblLicenseMessage.ForeColor = System.Drawing.Color.Red;
                lblLicenseMessage.Font.Bold = true;
            }
            else if (paramList[9].Value.ToString() == "valid")
            {
                buildLicenseGrid(gvSiteLicenses.PageIndex);
                //buildRadGrid();
                lbCreatePackage.Enabled = true;
            }
            else if (paramList[9].Value.ToString() == "rollback")
            {
                lblLicenseMessage.Visible = true;
                lblLicenseMessage.Text = "There was an error and the stored procedure was rolled back";
                lblLicenseMessage.ForeColor = System.Drawing.Color.Red;
                lblLicenseMessage.Font.Bold = true;
            }
            else  // we have created a new license
            {
                lblSiteLicID.Text = paramList[9].Value.ToString();
                buildLicenseGrid(gvSiteLicenses.PageIndex);
                //buildRadGrid();
                lbCreatePackage.Enabled = true;
            }


        }
    }
    protected void cmdSaveLicenseDetails_Click(object sender, EventArgs e)
    {
        string datePackageCreated = "";
        string validFrom = "";
        string expiryDate = "";

        lblLicenseDetailsMessage.Visible = false;
        lblLicenseDetailsMessage.Text = "Required fields *";
        lblLicenseDetailsMessage.ForeColor = System.Drawing.Color.Black;
        lblLicenseDetailsMessage.Font.Bold = false;
        bool licenseConditionsMet = true;

        if ((tbNumberMonths.Text == "") || (tbNumberAccounts.Text == "") || (tbNumberReviews.Text == "") ||
            (tbTotalFee.Text == "") || (tbDatePackageCreated.Text == ""))
        {
            lblLicenseDetailsMessage.Visible = true;
            lblLicenseDetailsMessage.Text = "Please fill in all of the required fields *";
            lblLicenseDetailsMessage.ForeColor = System.Drawing.Color.Red;
            lblLicenseDetailsMessage.Font.Bold = true;
            licenseConditionsMet = false;
        }
        else if ((Utils.IsNumeric(tbNumberMonths.Text) != true) || (Utils.IsNumeric(tbNumberAccounts.Text) != true) ||
            (Utils.IsNumeric(tbNumberReviews.Text) != true))
        {
            lblLicenseDetailsMessage.Visible = true;
            lblLicenseDetailsMessage.Text = "Make sure that the number of months, accounts and reviews are numbers";
            lblLicenseDetailsMessage.ForeColor = System.Drawing.Color.Red;
            lblLicenseDetailsMessage.Font.Bold = true;
            licenseConditionsMet = false;
        }
        else if (Utils.IsNumeric(tbTotalFee.Text) != true)
        {
            lblLicenseDetailsMessage.Visible = true;
            lblLicenseDetailsMessage.Text = "Make the total fee is a number";
            lblLicenseDetailsMessage.ForeColor = System.Drawing.Color.Red;
            lblLicenseDetailsMessage.Font.Bold = true;
            licenseConditionsMet = false;
        }
        else if ((int.Parse(tbTotalFee.Text) % int.Parse(tbNumberMonths.Text)) != 0)
        {
            lblLicenseDetailsMessage.Visible = true;
            lblLicenseDetailsMessage.Text = "Make sure the total fee can be divided evenly by the number of months";
            lblLicenseDetailsMessage.ForeColor = System.Drawing.Color.Red;
            lblLicenseDetailsMessage.Font.Bold = true;
            licenseConditionsMet = false;
        }

        if (licenseConditionsMet == true)
        {  
            try
            {
                DateTime packageCreated = Convert.ToDateTime(tbDatePackageCreated.Text);
                datePackageCreated = packageCreated.ToString("yyyy-M-d hh:m:ss.mmm"); // this is case sensitive!
            }
            catch (Exception er)
            {
                lblLicenseDetailsMessage.Visible = true;
                lblLicenseDetailsMessage.Text = "Date created error: " + er.Message.ToString();
                lblLicenseDetailsMessage.ForeColor = System.Drawing.Color.Red;
                lblLicenseDetailsMessage.Font.Bold = true;
                licenseConditionsMet = false;
            }

            if ((tbValidFrom.Text != "") || (tbExpiryDate.Text != ""))
            {
                try
                {
                    DateTime validDate = Convert.ToDateTime(tbValidFrom.Text);
                    validFrom = validDate.ToString("yyyy-M-d hh:m:ss.mmm"); // this is case sensitive!
                }
                catch (Exception er)
                {
                    lblLicenseDetailsMessage.Visible = true;
                    lblLicenseDetailsMessage.Text = "Date valid from error: " + er.Message.ToString();
                    lblLicenseDetailsMessage.ForeColor = System.Drawing.Color.Red;
                    lblLicenseDetailsMessage.Font.Bold = true;
                    licenseConditionsMet = false;
                }
                try
                {
                    DateTime expired = Convert.ToDateTime(tbExpiryDate.Text);
                    expiryDate = expired.ToString("yyyy-M-d hh:m:ss.mmm"); // this is case sensitive!
                }
                catch (Exception er)
                {
                    lblLicenseDetailsMessage.Visible = true;
                    lblLicenseDetailsMessage.Text = "Date of expiry error: " + er.Message.ToString();
                    lblLicenseDetailsMessage.ForeColor = System.Drawing.Color.Red;
                    lblLicenseDetailsMessage.Font.Bold = true;
                    licenseConditionsMet = false;
                }
            }            
        }


        if (licenseConditionsMet == true)
        {
            int pricePerMonth = (int.Parse(tbTotalFee.Text)) / (int.Parse(tbNumberMonths.Text));
            bool isAdmDB = true;
            SqlParameter[] paramList = new SqlParameter[15];

            paramList[0] = new SqlParameter("@LIC_ID", SqlDbType.NVarChar, 50, ParameterDirection.Input,
                    true, 0, 0, null, DataRowVersion.Default, lblSiteLicID.Text);
            paramList[1] = new SqlParameter("@SITE_LIC_DETAILS_ID", SqlDbType.NVarChar, 50, ParameterDirection.Input,
                    true, 0, 0, null, DataRowVersion.Default, lblSiteLicenseDetailsID.Text);
            paramList[2] = new SqlParameter("@creator_id", SqlDbType.NVarChar, 50, ParameterDirection.Input,
                true, 0, 0, null, DataRowVersion.Default, Utils.GetSessionString("Contact_ID"));
            paramList[3] = new SqlParameter("@Accounts", SqlDbType.NVarChar, 50, ParameterDirection.Input,
                true, 0, 0, null, DataRowVersion.Default, tbNumberAccounts.Text);
            paramList[4] = new SqlParameter("@reviews", SqlDbType.NVarChar, 50, ParameterDirection.Input,
                true, 0, 0, null, DataRowVersion.Default, tbNumberReviews.Text);
            paramList[5] = new SqlParameter("@months", SqlDbType.NVarChar, 50, ParameterDirection.Input,
                true, 0, 0, null, DataRowVersion.Default, tbNumberMonths.Text);
            paramList[6] = new SqlParameter("@Totalprice", SqlDbType.NVarChar, 50, ParameterDirection.Input,
                true, 0, 0, null, DataRowVersion.Default, tbTotalFee.Text);
            paramList[7] = new SqlParameter("@pricePerMonth", SqlDbType.NVarChar, 50, ParameterDirection.Input,
                true, 0, 0, null, DataRowVersion.Default, pricePerMonth); 
            paramList[8] = new SqlParameter("@dateCreated", SqlDbType.NVarChar, 50, ParameterDirection.Input,
                    true, 0, 0, null, DataRowVersion.Default, datePackageCreated);
            paramList[9] = new SqlParameter("@forSaleID", SqlDbType.NVarChar, 50, ParameterDirection.Input,
                true, 0, 0, null, DataRowVersion.Default, lblForSaleID.Text);
            paramList[10] = new SqlParameter("@VALID_FROM", SqlDbType.NVarChar, 50, ParameterDirection.Input,
                    true, 0, 0, null, DataRowVersion.Default, validFrom);
            paramList[11] = new SqlParameter("@EXPIRY_DATE", SqlDbType.NVarChar, 50, ParameterDirection.Input,
                true, 0, 0, null, DataRowVersion.Default, expiryDate);
            paramList[12] = new SqlParameter("@ER4_ADM", SqlDbType.NVarChar, 50, ParameterDirection.Input,
                true, 0, 0, null, DataRowVersion.Default, Utils.GetSessionString("Contact_ID"));
            paramList[13] = new SqlParameter("@EXTENSION_TYPE", SqlDbType.NVarChar, 50, ParameterDirection.Input,
                true, 0, 0, null, DataRowVersion.Default, ddlExtensionType.SelectedValue);
            paramList[14] = new SqlParameter("@RESULT", SqlDbType.NVarChar, 50, ParameterDirection.Output,
                true, 0, 0, null, DataRowVersion.Default, "");

            Utils.ExecuteSPWithReturnValues(isAdmDB, Server, "st_Site_Lic_Details_Create_or_Edit_AndOr_Activate", paramList);


            if (paramList[14].Value.ToString() == "Invalid")
            {
                lblLicenseDetailsMessage.Visible = true;
                lblLicenseDetailsMessage.Text = "the SQL statement has failed and has been rolled back";
                lblLicenseDetailsMessage.ForeColor = System.Drawing.Color.Red;
            }
            else if (paramList[14].Value.ToString() == "rollback")
            {
                lblLicenseDetailsMessage.Visible = true;
                lblLicenseDetailsMessage.Text = "the SQL statement has failed and has been rolled back";
                lblLicenseDetailsMessage.ForeColor = System.Drawing.Color.Red;
            }
            else // the save was successful
            {
                lblLicenseDetailsMessage.Visible = false;
                lblLicenseDetailsMessage.Text = "Please fill in all of the fields *";
                lblLicenseDetailsMessage.ForeColor = System.Drawing.Color.Black;
                pnlAccountsAndReviews.Visible = true;
            }
        }
    }
    protected void lbCreatePackage_Click(object sender, EventArgs e)
    {
        lblSiteLicenseDetailsID.Text = "N/A";
        tbNumberMonths.Text = "";
        tbNumberAccounts.Text = "";
        tbNumberReviews.Text = "";
        lblPricePerMonth.Text = "N/A";
        tbTotalFee.Text = "";
        tbDatePackageCreated.Text = "";
        tbValidFrom.Text = "";
        tbExpiryDate.Text = "";
        lblForSaleID.Text = "N/A";

        pnlSiteLicense.Visible = true;
        pnlLicenseDetails.Visible = true;
        lbSelectAdmin.Enabled = false;
    }

    protected void lbLicense_Click(object sender, EventArgs e)
    {
        LinkButton btn = sender as LinkButton;
        GridDataItem item = btn.NamingContainer as GridDataItem;
        string siteLicID = item.GetDataKeyValue("SITE_LIC_ID").ToString();
        string adminID = item.GetDataKeyValue("CONTACT_ID").ToString();
        //string adminID = item["CONTACT_ID"].Text;
        
        bool isAdmDB = true;
        pnlSiteLicense.Visible = false;
        pnlLicenseDetails.Visible = false;
        pnlAccountsAndReviews.Visible = false;

        
        string itemToSelect = "";
        

        DataTable dt1 = new DataTable();
        System.Data.DataRow newrow1;
        dt1.Columns.Add(new DataColumn("SITE_LIC_DETAILS_ID", typeof(string)));
        dt1.Columns.Add(new DataColumn("DATE_CREATED", typeof(string)));
        int counter = 0;

        IDataReader idr = Utils.GetReader(isAdmDB, "st_Site_Lic_Get_all_Packages", siteLicID, "0");
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
        if (ddlPackages.Items.Count != 0)
        {
            if (itemToSelect != "")
                ddlPackages.SelectedValue = itemToSelect;
            else
                ddlPackages.SelectedIndex = 0;
        }


        if (ddlPackages.Items.Count != 0)
        {
            // license details are available
            pnlSiteLicense.Visible = true;
            pnlLicenseDetails.Visible = true;
            pnlAccountsAndReviews.Visible = true;
            lbLicenseHistory.Enabled = true;

            lbSelectAdmin.Enabled = false;
            tbExpiryDate.Enabled = true;
            IBCalendar3.Enabled = true;
            tbValidFrom.Enabled = true;
            IBCalendar2.Enabled = true;


            int idrCounter = 0;
            idr = Utils.GetReader(isAdmDB, "st_Site_Lic_Get_Details_1", adminID, siteLicID);
            if (idr.Read())
            {
                tbLicenseName.Text = idr["SITE_LIC_NAME"].ToString();
                lblSiteLicID.Text = idr["SITE_LIC_ID"].ToString();
                tbOrganisation.Text = idr["COMPANY_NAME"].ToString();
                tbAddress.Text = idr["COMPANY_ADDRESS"].ToString();
                tbTelephone.Text = idr["TELEPHONE"].ToString();
                tbNotes.Text = idr["NOTES"].ToString();
                tbDateLicenseCreated.Text = idr["DATE_CREATED"].ToString();
                lblCreatedBy.Text = idr["CREATOR_ID"].ToString();
                lblInitialAdministrator.Text = idr["ADMIN_NAME"].ToString();
                lblAdminID.Text = adminID;

                lblSiteLicenseDetailsID.Text = idr["SITE_LIC_DETAILS_ID"].ToString();
                tbNumberMonths.Text = idr["MONTHS"].ToString();
                tbNumberAccounts.Text = idr["ACCOUNTS_ALLOWANCE"].ToString();
                tbNumberReviews.Text = idr["REVIEWS_ALLOWANCE"].ToString();
                lblPricePerMonth.Text = idr["PRICE_PER_MONTH"].ToString();
                tbTotalFee.Text = (int.Parse(lblPricePerMonth.Text) * int.Parse(tbNumberMonths.Text)).ToString();
                tbDatePackageCreated.Text = idr["DATE_PACKAGE_CREATED"].ToString();
                tbValidFrom.Text = idr["VALID_FROM"].ToString();

                if (ddlPackages.SelectedItem.Text.StartsWith("Expired"))
                {
                    tbExpiryDate.Text = "Expired";
                    tbExpiryDate.BackColor = Color.Pink;
                    tbExpiryDate.Font.Bold = true;
                }
                else
                {
                    // check if the latest is expired
                    string test = idr["EXPIRY_DATE"].ToString();
                    DateTime expired = Convert.ToDateTime(idr["EXPIRY_DATE"].ToString());
                    DateTime today = DateTime.Today;
                    if (expired < today)
                    {
                        tbExpiryDate.BackColor = Color.Pink;
                        tbExpiryDate.Font.Bold = true;
                    }
                    else
                    {
                        tbExpiryDate.BackColor = Color.White;
                        tbExpiryDate.Font.Bold = false;
                    }
                    tbExpiryDate.Text = idr["EXPIRY_DATE"].ToString();
                }

                //tbExpiryDate.Text = idr["EXPIRY_DATE"].ToString();
                lblForSaleID.Text = idr["FOR_SALE_ID"].ToString();
                idrCounter += 1;
                lblIsActivated.Text = "Yes";
                lbExtensionHistory.Enabled = true;
            }
            else //(idrCounter == 0)
            {
                // read the offers as there aren't any active packages
                idr.NextResult();
                if (idr.Read())
                {
                    tbLicenseName.Text = idr["SITE_LIC_NAME"].ToString();
                    lblSiteLicID.Text = idr["SITE_LIC_ID"].ToString();
                    tbOrganisation.Text = idr["COMPANY_NAME"].ToString();
                    tbAddress.Text = idr["COMPANY_ADDRESS"].ToString();
                    tbTelephone.Text = idr["TELEPHONE"].ToString();
                    tbNotes.Text = idr["NOTES"].ToString();
                    tbDateLicenseCreated.Text = idr["DATE_CREATED"].ToString();
                    lblCreatedBy.Text = idr["CREATOR_ID"].ToString();
                    lblInitialAdministrator.Text = idr["ADMIN_NAME"].ToString();
                    lblAdminID.Text = adminID;

                    lblSiteLicenseDetailsID.Text = idr["SITE_LIC_DETAILS_ID"].ToString();
                    tbNumberMonths.Text = idr["MONTHS"].ToString();
                    tbNumberAccounts.Text = idr["ACCOUNTS_ALLOWANCE"].ToString();
                    tbNumberReviews.Text = idr["REVIEWS_ALLOWANCE"].ToString();
                    lblPricePerMonth.Text = idr["PRICE_PER_MONTH"].ToString();
                    tbTotalFee.Text = (int.Parse(lblPricePerMonth.Text) * int.Parse(tbNumberMonths.Text)).ToString();
                    tbDatePackageCreated.Text = idr["DATE_PACKAGE_CREATED"].ToString();
                    tbValidFrom.Text = idr["VALID_FROM"].ToString();
                    tbExpiryDate.Text = idr["EXPIRY_DATE"].ToString();
                    lblForSaleID.Text = idr["FOR_SALE_ID"].ToString();
                }
            }
            idr.Close();

            DataTable dt = new DataTable();
            System.Data.DataRow newrow;
            dt.Columns.Add(new DataColumn("EXTENSION_TYPE_ID", typeof(string)));
            dt.Columns.Add(new DataColumn("EXTENSION_TYPE", typeof(string)));
            newrow = dt.NewRow();
            newrow["EXTENSION_TYPE_ID"] = "0";
            newrow["EXTENSION_TYPE"] = "Select a type";
            dt.Rows.Add(newrow);
            idr = Utils.GetReader(isAdmDB, "st_ExtensionTypesGet", "SiteLic");
            while (idr.Read())
            {
                newrow = dt.NewRow();
                newrow["EXTENSION_TYPE_ID"] = idr["EXTENSION_TYPE_ID"].ToString();
                newrow["EXTENSION_TYPE"] = idr["EXTENSION_TYPE"].ToString();
                dt.Rows.Add(newrow);
            }
            idr.Close();
            ddlExtensionType.DataSource = dt;
            ddlExtensionType.DataBind();
            ddlExtensionType.Enabled = true;

            buildGrids();
            pnlAccountsAndReviews.Visible = true;

            lblAccountMsg.Visible = false;
            lblAccountMsg.ForeColor = System.Drawing.Color.Black;
            lblReviewMsg.Visible = false;
            lblReviewMsg.ForeColor = System.Drawing.Color.Black;
            lblAccountAdmMessage.Visible = false;
            lblAccountAdmMessage.ForeColor = System.Drawing.Color.Black;
        }
        else
        {
            // site license only - no details/packages available  siteLicID
            // orginal JB Jun26
            //idr = Utils.GetReader(isAdmDB, "st_Site_Lic_Get", adminID);
            //
            // added JB Jun26
            idr = Utils.GetReader(isAdmDB, "st_Site_Lic_Get_1", adminID, siteLicID);
            //
            while (idr.Read())
            {
                tbLicenseName.Text = idr["SITE_LIC_NAME"].ToString();
                lblSiteLicID.Text = idr["SITE_LIC_ID"].ToString();
                tbOrganisation.Text = idr["COMPANY_NAME"].ToString();
                tbAddress.Text = idr["COMPANY_ADDRESS"].ToString();
                tbTelephone.Text = idr["TELEPHONE"].ToString();
                tbNotes.Text = idr["NOTES"].ToString();
                tbDateLicenseCreated.Text = idr["DATE_CREATED"].ToString();
                lblCreatedBy.Text = idr["CREATOR_NAME"].ToString();
                lblInitialAdministrator.Text = idr["ADMINISTRATOR_NAME"].ToString();
                lblAdminID.Text = adminID;
            }
            idr.Close();


            lblSiteLicenseDetailsID.Text = "N/A";
            tbNumberMonths.Text = "";
            tbNumberAccounts.Text = "";
            tbNumberReviews.Text = "";
            lblPricePerMonth.Text = "N/A";
            tbTotalFee.Text = "";
            tbDatePackageCreated.Text = "";
            tbValidFrom.Text = "";
            tbExpiryDate.Text = "";
            lblForSaleID.Text = "N/A";

            pnlSiteLicense.Visible = true;
            lbSelectAdmin.Enabled = true;
            //pnlLicenseDetails.Visible = true;
        }





    }

    /*
    protected void radGVSiteLicense_NeedDataSource(object sender, Telerik.Web.UI.GridNeedDataSourceEventArgs e)
    {
        DataTable dt = new DataTable();
        System.Data.DataRow newrow;

        dt.Columns.Add(new DataColumn("CONTACT_ID", typeof(string)));
        dt.Columns.Add(new DataColumn("CONTACT_NAME", typeof(string)));
        dt.Columns.Add(new DataColumn("SITE_LIC_ID", typeof(string)));
        dt.Columns.Add(new DataColumn("SITE_LIC_NAME", typeof(string)));
        dt.Columns.Add(new DataColumn("EXPIRY_DATE", typeof(string)));

        bool isAdmDB = true;
        IDataReader idr = Utils.GetReader(isAdmDB, "st_Site_Lic_Get_Filter", tbFilter.Text);
        while (idr.Read())
        {
            newrow = dt.NewRow();
            newrow["CONTACT_NAME"] = idr["CONTACT_NAME"].ToString();
            newrow["CONTACT_ID"] = idr["CONTACT_ID"].ToString(); // not visible
            newrow["SITE_LIC_ID"] = idr["SITE_LIC_ID"].ToString();
            newrow["SITE_LIC_NAME"] = idr["SITE_LIC_NAME"].ToString();
            newrow["EXPIRY_DATE"] = idr["EXPIRY_DATE"].ToString();
            dt.Rows.Add(newrow);
        }
        idr.Close();

        radGVSiteLicense.DataSource = dt;
    }
    protected void RadAjaxManager1_AjaxRequest(object sender, AjaxRequestEventArgs e)
    {
        if (e.Argument.IndexOf("FilterGrid") != -1)
        {
            radGVSiteLicense.Rebind();
        }
    }
    protected void radGVSiteLicense_PageIndexChanged(object sender, Telerik.Web.UI.GridPageChangedEventArgs e)
    {
        if (tbFilter.Text == "")
        {
            buildRadGrid();
        }
        else
        {
            radGVSiteLicense.Rebind(); // fires NeedDataSource
        }
    }
    protected void radGVSiteLicense_ItemDataBound(object sender, Telerik.Web.UI.GridItemEventArgs e)
    {
        string test;
    }

    protected void radGVSiteLicense_ItemCommand(object sender, GridCommandEventArgs e)
    {
        if (e.CommandName == RadGrid.SelectCommandName) //"Add new" button clicked
        {

            GridBoundColumn detailColumn = (GridBoundColumn)radGVSiteLicense.MasterTableView.GetColumn("SITE_LIC_ID");

            

            pnlSiteLicense.Visible = true;

        }
    }

    protected void gvSiteLicenses0_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {
        buildLicenseGrid2(e.NewPageIndex);
    }
    protected void gvSiteLicenses0_RowCommand(object sender, GridViewCommandEventArgs e)
    {

    }
    protected void gvSiteLicenses0_Sorting(object sender, GridViewSortEventArgs e)
    {
        string test = "";
    }
    */
    protected void gvSiteLicenses_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {
        buildLicenseGrid(e.NewPageIndex); 
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

    protected void cbAllowReviewOwnershipChange_CheckedChanged(object sender, EventArgs e)
    {
        bool isChecked = false;
        if (cbAllowReviewOwnershipChange.Checked == true)
            isChecked = true;

        bool isAdmDB = true;
        Utils.ExecuteSP(isAdmDB, Server, "st_AllowReviewOwnershipChangeInLicense", lblSiteLicID.Text, isChecked);
    }
}