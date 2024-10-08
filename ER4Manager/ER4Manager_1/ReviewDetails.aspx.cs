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

public partial class ReviewDetails : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (Utils.GetSessionString("Contact_ID") != null)
        {
            if (Utils.GetSessionString("IsAdm") == "True")
            {
                if (!IsPostBack)
                {
                    System.Web.UI.WebControls.Label lbl = (Label)Master.FindControl("lblPageTitle");
                    if (lbl != null)
                    {
                        lbl.Text = "Review details";
                    }


                    Telerik.Web.UI.RadTabStrip radTs = (Telerik.Web.UI.RadTabStrip)Master.FindControl("rtsMenu");
                    if (radTs != null)
                    {
                        radTs.SelectedIndex = 2;
                        radTs.Tabs[2].Tabs[3].Selected = true;
                        radTs.Tabs[2].Tabs[4].Width = 330;
                    }
                    System.Web.UI.WebControls.Label lbl1 = (Label)Master.FindControl("lblHeadingText");
                    if (lbl1 != null)
                    {
                        lbl1.Text = "Review details";
                    }

                    DataTable dt = new DataTable();
                    System.Data.DataRow newrow;

                    dt.Columns.Add(new DataColumn("EXTENSION_TYPE_ID", typeof(string)));
                    dt.Columns.Add(new DataColumn("EXTENSION_TYPE", typeof(string)));

                    newrow = dt.NewRow();
                    newrow["EXTENSION_TYPE_ID"] = "0";
                    newrow["EXTENSION_TYPE"] = "Select a type";
                    dt.Rows.Add(newrow);

                    lblInSiteLicense.Visible = false;
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


                    if (Request.QueryString["ID"].ToString() == "New")
                    {
                        ddlExtensionType.Enabled = false;
                        tbExtensionDetails.Enabled = false;
                        lbExtensionHistory.Enabled = false;
                    }
                    else
                    {
                        ddlExtensionType.Enabled = true;
                        tbExtensionDetails.Enabled = true;
                        lbExtensionHistory.Enabled = true;

                        isAdmDB = true;
                        idr = Utils.GetReader(isAdmDB, "st_ReviewDetailsCochrane",
                            Request.QueryString["ID"].ToString());
                        if (idr.Read())
                        {
                            tbReviewTitle.Text = idr["REVIEW_NAME"].ToString();
                            tbRegistrationDate.Text = idr["DATE_CREATED"].ToString();
                            tbExpiryDate.Text = idr["EXPIRY_DATE"].ToString();
                            tbReviewNumber.Text = idr["REVIEW_NUMBER"].ToString();
                            lblReviewID.Text = idr["REVIEW_ID"].ToString();
                            lblER3ReviewID.Text = idr["OLD_REVIEW_ID"].ToString();
                            lblER3ReviewGroupID.Text = idr["OLD_REVIEW_GROUP_ID"].ToString();
                            tbRegistrationDate.Text = idr["DATE_CREATED"].ToString();
                            lblFunderID.Text = idr["FUNDER_ID"].ToString();
                            lblFunderName.Text = idr["CONTACT_NAME"].ToString();
                            ddlMonthsCredit.SelectedValue = idr["MONTHS_CREDIT"].ToString();
                            //put this back when you are done - JB
                            if (idr["SITE_LIC_ID"].ToString() != "")
                            {
                                lblInSiteLicense.Visible = true;
                                lblInSiteLicense.Text = "In license #" + idr["SITE_LIC_ID"].ToString();
                            }
                            if (idr["SHOW_SCREENING"].ToString() == "True")
                                cbShowScreening.Checked = true;                          
                            if (idr["ALLOW_REVIEWER_TERMS"].ToString() == "True")
                                cbAllowReviewerTerms.Checked = true;
                            if (idr["ALLOW_CLUSTERED_SEARCH"].ToString() == "True")
                                cbAllowClusteredSearch.Checked = true;

                            if (idr["MAG_ENABLED"].ToString() == "1")
                                cbEnableMag.Checked = true;

                            if (idr["OPEN_AI_ENABLED"].ToString() == "True")
                                cbEnableOpenAI.Checked = true;

                            if (idr["ARCHIE_ID"].ToString() == "prospective_______")
                            {
                                cbPotential.Checked = true;
                                tbArchieID.Enabled = false;
                            }
                            else
                            {
                                cbPotential.Checked = false;
                                tbArchieID.Enabled = true;
                            }
                            tbArchieID.Text = idr["ARCHIE_ID"].ToString();
                            tbArchieCD.Text = idr["ARCHIE_CD"].ToString();
                            tbCheckedOutBy.Text = idr["CHECKED_OUT_BY"].ToString();

                            if (idr["IS_CHECKEDOUT_HERE"].ToString() == "True")
                                cbIsCheckedOutHere.Checked = true;
                            
                            
                        }
                        idr.Close();


                        IDataReader idr1 = Utils.GetReader(isAdmDB, "st_BritishLibraryValuesGetAll",
                            Request.QueryString["ID"].ToString());
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

                        /*
                        string SQL = "select REVIEW_ID, REVIEW_NAME, OLD_REVIEW_ID, OLD_REVIEW_GROUP_ID, " +
                                "DATE_CREATED, EXPIRY_DATE, " +
                            "FUNDER_ID, REVIEW_NUMBER from TB_REVIEW " +
                            "where REVIEW_ID = '" + Request.QueryString["ID"].ToString() + "'";
                        bool isAdmDB = false;
                        SqlDataReader sdr = Utils.ReturnReader(SQL, isAdmDB);
                        if (sdr.Read())
                        {
                            tbReviewTitle.Text = sdr["REVIEW_NAME"].ToString();
                            tbRegistrationDate.Text = sdr["DATE_CREATED"].ToString();
                            tbExpiryDate.Text = sdr["EXPIRY_DATE"].ToString();
                            tbReviewNumber.Text = sdr["REVIEW_NUMBER"].ToString();
                            lblReviewID.Text = sdr["REVIEW_ID"].ToString();
                            lblER3ReviewID.Text = sdr["OLD_REVIEW_ID"].ToString();
                            lblER3ReviewGroupID.Text = sdr["OLD_REVIEW_GROUP_ID"].ToString();
                            tbRegistrationDate.Text = sdr["DATE_CREATED"].ToString();
                            lblFunderID.Text = sdr["FUNDER_ID"].ToString();
                            SQL = "select CONTACT_NAME from TB_CONTACT where CONTACT_ID = '" +
                                sdr["FUNDER_ID"].ToString() + "'";
                            SqlDataReader sdr1 = Utils.ReturnReader(SQL, isAdmDB);
                            if (sdr1.Read())
                            {
                                lblFunderName.Text = sdr1["CONTACT_NAME"].ToString();
                            }
                            sdr1.Close();
                        }
                        sdr.Close();
                        */

                        /*
                        if (lblReviewID.Text != "N / A")
                        {
                            lbAddReviewer.Visible = true;
                        }
                        */
                        if (tbExpiryDate.Text == "")
                        {
                            lbAddReviewer.Visible = false;
                            if (tbArchieID.Text != "")
                                lbAddReviewer.Visible = true;
                        }
                        else
                        {
                            lbAddReviewer.Visible = true;
                        }

                        buildGrid();

                        getOpenAIDetails();

                    }
                }
                IBCalendar1.Attributes.Add("onclick", "JavaScript:openCalendar1('" +
                    "!1!" + tbRegistrationDate.Text + "')");
                IBCalendar2.Attributes.Add("onclick", "JavaScript:openCalendar1('" +
                    "!2!" + tbExpiryDate.Text + "')");

                lbAddReviewer.Attributes.Add("onclick", "JavaScript:openReviewerList('Please select')");
                lbSelectFunder.Attributes.Add("onclick", "JavaScript:openFunderList('Please select')");
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
    private void buildGrid()
    {
        string ReviewID = Request.QueryString["ID"].ToString();
        DataTable dt1 = new DataTable();
        System.Data.DataRow newrow1;

        dt1.Columns.Add(new DataColumn("CONTACT_ID", typeof(string)));
        dt1.Columns.Add(new DataColumn("CONTACT", typeof(string)));
        dt1.Columns.Add(new DataColumn("EMAIL", typeof(string)));
        dt1.Columns.Add(new DataColumn("EXPIRY_DATE", typeof(string)));
        dt1.Columns.Add(new DataColumn("LAST_LOGIN", typeof(string)));
        dt1.Columns.Add(new DataColumn("HOURS", typeof(string)));

        bool isAdmDB = true;
        IDataReader idr = Utils.GetReader(isAdmDB, "st_ReviewContacts", Request.QueryString["ID"].ToString());
        while (idr.Read())
        {
            newrow1 = dt1.NewRow();
            newrow1["CONTACT_ID"] = idr["CONTACT_ID"].ToString();
            newrow1["CONTACT"] = idr["CONTACT_NAME"].ToString();
            newrow1["EMAIL"] = idr["EMAIL"].ToString();
            if (idr["LAST_LOGIN"].ToString() == "")
                newrow1["LAST_LOGIN"] = "N/A";
            else
                newrow1["LAST_LOGIN"] = idr["LAST_LOGIN"].ToString();
            if (idr["EXPIRY_DATE"].ToString() != "")
                newrow1["EXPIRY_DATE"] = idr["EXPIRY_DATE"].ToString().Substring(0, 10);
            else
                newrow1["EXPIRY_DATE"] = "N/A";
            if (idr["HOURS"].ToString() == null)
                newrow1["HOURS"] = "N/A";
            else
                newrow1["HOURS"] = idr["HOURS"].ToString();
            dt1.Rows.Add(newrow1);
        }
        idr.Close();

        gvContacts.DataSource = dt1;
        gvContacts.DataBind();


        string contactIDFromGridView = "";
        for (int i = 0; i < gvContacts.Rows.Count; i++)  // through each row in grid
        {
            contactIDFromGridView = (string)gvContacts.DataKeys[i].Value;
            GridViewRow row = gvContacts.Rows[i];
            DropDownList ddl = ((DropDownList)row.FindControl("ddlRole"));

            DataTable dt = new DataTable();
            System.Data.DataRow newrow;
            dt.Columns.Add(new DataColumn("ROLE", typeof(string)));

            idr = Utils.GetReader(isAdmDB, "st_ContactReviewRole", contactIDFromGridView, Request.QueryString["ID"].ToString());            
            while (idr.Read()) 
            {
                newrow = dt.NewRow();
                newrow["ROLE"] = idr["ROLE_NAME"].ToString();
                dt.Rows.Add(newrow);
            }
            idr.Close();

            // there are some users missing roles as well as users with multiple roles (which is no longer needed/wanted)
            // but we still need to make sense of it...
            // If the user doesn't have a role then go with RegularUser
            // If a user has multiple roles there is a hierarchal order and luckily that order is in alphabetical order
            // AdminUser, Coding only, ReadOnlyUser, RegularUser  
            // so we just need to sort the table and take the top one!
            dt.DefaultView.Sort = "ROLE asc";
            dt = dt.DefaultView.ToTable();

            if (dt.Rows.Count == 0)
            {
                // If the user doesn't have a role then go with RegularUser
                ddl.SelectedValue = "4";
            }
            else
            {
                switch (dt.Rows[0]["ROLE"].ToString())
                {
                    case "AdminUser":
                        ddl.SelectedValue = "1";
                        break;
                    case "Coding only":
                        ddl.SelectedValue = "2";
                        break;
                    case "ReadOnlyUser":
                        ddl.SelectedValue = "3";
                        break;
                    default:
                        ddl.SelectedValue = "4";
                        break;
                }
            }
        }


        /*   this is no longer needed JB 10/12/2001 */
        /**/
        DataTable dt2 = new DataTable();
        System.Data.DataRow newrow2;
        dt2.Columns.Add(new DataColumn("ROLE_NAME", typeof(string)));
        dt2.Columns.Add(new DataColumn("SELECTED", typeof(string)));

        isAdmDB = true;
        idr = Utils.GetReader(isAdmDB, "st_ReviewRolesAll");
        while (idr.Read())
        {
            newrow2 = dt2.NewRow();
            newrow2["ROLE_NAME"] = idr["ROLE_NAME"].ToString();
            dt2.Rows.Add(newrow2);
        }
        idr.Close();


        for (int i = 0; i < gvContacts.Rows.Count; i++)  // through each row in grid
        {
            GridViewRow row = gvContacts.Rows[i];
            CheckBoxList cbl = ((CheckBoxList)row.FindControl("cblContactReviewRole"));
            cbl.DataSource = dt2;
            cbl.DataBind();
        }

        
        //string contactIDFromGridView = "";
        for (int i = 0; i < gvContacts.Rows.Count; i++)  // through each row in grid
        {
            contactIDFromGridView = (string)gvContacts.DataKeys[i].Value;
            GridViewRow row = gvContacts.Rows[i];
            CheckBoxList cbl = ((CheckBoxList)row.FindControl("cblContactReviewRole"));
            idr = Utils.GetReader(isAdmDB, "st_ContactReviewRole", contactIDFromGridView, Request.QueryString["ID"].ToString());
            while (idr.Read())
            {
                for (int x = 0; x < dt2.Rows.Count; x++) // through each role
                {
                    if (cbl.Items[x].Text == idr["ROLE_NAME"].ToString())
                    {
                        cbl.Items[x].Selected = true;
                    }
                }
            }
            idr.Close();
        } 

        /**/



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
            lblReviewID.Text, "0");
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


    protected void cmdSave_Click(object sender, EventArgs e)
    {
        lblMissingFields.Visible = false;
        lblMissingFields.Text = "Please fill in all of the fields *";
        lblMissingFields.ForeColor = System.Drawing.Color.Black;
        bool reviewConditionsMet = true;
        
        if ((ddlExtensionType.SelectedIndex == 0) && (Request.QueryString["ID"].ToString() != "New"))
        {
            lblMissingFields.Visible = true;
            lblMissingFields.Text = "Select an extension type";
            lblMissingFields.ForeColor = System.Drawing.Color.Red;
            reviewConditionsMet = false;
        }     
        //make sure all of the fields are filled in
        else if (tbReviewTitle.Text == "" || tbRegistrationDate.Text == "" || lblFunderID.Text == "0")
        {
            lblMissingFields.Visible = true;
            lblMissingFields.Text = "Please fill in all of the necessary fields *";
            lblMissingFields.ForeColor = System.Drawing.Color.Red;
            reviewConditionsMet = false;
        }

        if ((ddlMonthsCredit.SelectedIndex > 0) && (tbExpiryDate.Text != ""))
        {
            lblMissingFields.Visible = true;
            lblMissingFields.Text = "Months credit must have a null expiry date";
            lblMissingFields.ForeColor = System.Drawing.Color.Red;
            reviewConditionsMet = false;
        }


        string startDate = "";
        string expired = "";
        try
        {
            DateTime registrationDate = Convert.ToDateTime(tbRegistrationDate.Text);
            startDate = registrationDate.ToString("yyyy-M-d hh:m:ss.mmm"); // this is case sensitive!
        }
        catch (Exception er)
        {
            lblMissingFields.Visible = true;
            lblMissingFields.Text = "Date created error: " + er.Message.ToString();
            lblMissingFields.ForeColor = System.Drawing.Color.Red;
            reviewConditionsMet = false;
        }

        if (tbExpiryDate.Text != "")
        {
            try
            {
                DateTime expiryDate = Convert.ToDateTime(tbExpiryDate.Text);
                expired = expiryDate.ToString("yyyy-M-d hh:m:ss.mmm"); // this is case sensitive!
            }
            catch (Exception er)
            {
                lblMissingFields.Visible = true;
                lblMissingFields.Text = "Expiry date error: " + er.Message.ToString();
                lblMissingFields.ForeColor = System.Drawing.Color.Red;
                reviewConditionsMet = false;
            }
        }



        if (reviewConditionsMet == true)
        {
            string notes = tbExtensionDetails.Text;
            bool newReview = false;
            if (Request.QueryString["ID"].ToString() == "New")
                newReview = true;

            
            /*
            DateTime registrationDate = Convert.ToDateTime(tbRegistrationDate.Text);
            startDate = registrationDate.ToString("yyyy-M-d hh:m:ss.mmm"); // this is case sensitive!

            expired = "";
            if (tbExpiryDate.Text != "")
            {
                DateTime expiryDate = Convert.ToDateTime(tbExpiryDate.Text);
                expired = expiryDate.ToString("yyyy-M-d hh:m:ss.mmm"); // this is case sensitive!
            }
            */

            bool isAdmDB = true;
            SqlParameter[] paramList = new SqlParameter[12];

            paramList[0] = new SqlParameter("@NEW_REVIEW", SqlDbType.NVarChar, 50, ParameterDirection.Input,
                true, 0, 0, null, DataRowVersion.Default, newReview);
            paramList[1] = new SqlParameter("@REVIEW_NAME", SqlDbType.NVarChar, 255, ParameterDirection.Input,
                true, 0, 0, null, DataRowVersion.Default, tbReviewTitle.Text);//.Replace("'", "''"));
            paramList[2] = new SqlParameter("@DATE_CREATED", SqlDbType.NVarChar, 50, ParameterDirection.Input,
                true, 0, 0, null, DataRowVersion.Default, startDate);
            paramList[3] = new SqlParameter("@EXPIRY_DATE", SqlDbType.NVarChar, 50, ParameterDirection.Input,
                true, 0, 0, null, DataRowVersion.Default, expired);
            paramList[4] = new SqlParameter("@FUNDER_ID", SqlDbType.NVarChar, 50, ParameterDirection.Input,
                true, 0, 0, null, DataRowVersion.Default, lblFunderID.Text);
            paramList[5] = new SqlParameter("@REVIEW_NUMBER", SqlDbType.NVarChar, 500, ParameterDirection.Input,
                true, 0, 0, null, DataRowVersion.Default, tbReviewNumber.Text);
            if (Request.QueryString["ID"].ToString() == "New")
            {
                paramList[6] = new SqlParameter("@REVIEW_ID", SqlDbType.NVarChar, 50, ParameterDirection.Input,
                    true, 0, 0, null, DataRowVersion.Default, Utils.GetSessionString("Contact_ID")); // this is the creator ID and not used
            }
            else
            {
                paramList[6] = new SqlParameter("@REVIEW_ID", SqlDbType.NVarChar, 50, ParameterDirection.Input,
                    true, 0, 0, null, DataRowVersion.Default, Request.QueryString["ID"].ToString());
            }
            paramList[7] = new SqlParameter("@EXTENSION_TYPE_ID", SqlDbType.NVarChar, 50, ParameterDirection.Input,
                    true, 0, 0, null, DataRowVersion.Default, ddlExtensionType.SelectedValue);

            if (tbExtensionDetails.Text == "Enter further details (optional)")
                notes = "";
            paramList[8] = new SqlParameter("@EXTENSION_NOTES", SqlDbType.NVarChar, 500, ParameterDirection.Input,
                    true, 0, 0, null, DataRowVersion.Default, notes);
            paramList[9] = new SqlParameter("@EDITOR_ID", SqlDbType.NVarChar, 50, ParameterDirection.Input,
                    true, 0, 0, null, DataRowVersion.Default, Utils.GetSessionString("Contact_ID"));
            paramList[10] = new SqlParameter("@MONTHS_CREDIT", SqlDbType.NVarChar, 50, ParameterDirection.Input,
                    true, 0, 0, null, DataRowVersion.Default, ddlMonthsCredit.SelectedValue);

            paramList[11] = new SqlParameter("@RESULT", SqlDbType.NVarChar, 50, ParameterDirection.Output,
                true, 0, 0, null, DataRowVersion.Default, "");

            Utils.ExecuteSPWithReturnValues(isAdmDB, Server, "st_ReviewFullCreateOrEdit", paramList);


            if (paramList[11].Value.ToString() == "Invalid")
            {
                lblMissingFields.Visible = true;
                lblMissingFields.Text = "the SQL statement has failed and has been rolled back";
                lblMissingFields.ForeColor = System.Drawing.Color.Red;
            }
            else // the save was successful
            {              
                lblMissingFields.Visible = false;
                lblMissingFields.Text = "Please fill in all of the fields *";
                lblMissingFields.ForeColor = System.Drawing.Color.Black;

                if (newReview == true)
                {             
                    // if this is a new non-shareable review we need to place the funder in it
                    Utils.ExecuteSP(isAdmDB, Server, "st_ReviewNewNonShareableAddContact",
                                paramList[11].Value.ToString(), lblFunderID.Text);
                    Server.Transfer("ReviewDetails.aspx?ID=" + paramList[11].Value.ToString());
                }
                else
                {
                    Server.Transfer("ReviewDetails.aspx?ID=" + Request.QueryString["ID"].ToString());
                }
            }
        }
        
        
        
        
        
        
        
        
        /*
        string SQL = "";
        if (Request.QueryString["ID"].ToString() == "New")
        {
            //make sure all of the fields are filled in
            if (tbReviewTitle.Text == "" || tbRegistrationDate.Text == "")
            {
                lblMissingFields.Visible = true;
            }
            else
            {
                string expiryDate = null;
                DateTime registrationDate = Convert.ToDateTime(tbRegistrationDate.Text);
                string startDate = registrationDate.ToString("yyyy-M-d hh:m:ss.mmm"); // this is case sensitive!
                if (tbExpiryDate.Text != "")
                {
                    DateTime expirationDate = Convert.ToDateTime(tbExpiryDate.Text);
                    expiryDate = expirationDate.ToString("yyyy-M-d hh:m:ss.mmm"); // this is case sensitive!
                }
                if (expiryDate == null)
                {
                    SQL = "insert into TB_REVIEW (REVIEW_NAME, DATE_CREATED, REVIEW_NUMBER) " +
                        "values ('" + tbReviewTitle.Text.Replace("'", "''") + "', '" + startDate + "', '" +
                        tbReviewNumber.Text + "')";
                }
                else
                {
                    SQL = "insert into TB_REVIEW (REVIEW_NAME, DATE_CREATED, EXPIRY_DATE, REVIEW_NUMBER, FUNDER_ID) " +
                        "values ('" + tbReviewTitle.Text.Replace("'", "''") + "', '" + startDate + "', '" + expiryDate + "', '" +
                        tbReviewNumber.Text + "', '" + lblFunderID.Text + "')";
                }
                bool isAdmDB = false;
                Utils.ExecuteQuery(SQL, isAdmDB);
                // find the new review_id
                string reviewID = "";
                SQL = "SELECT REVIEW_ID from TB_REVIEW " +
                    "where REVIEW_NAME = '" + tbReviewTitle.Text.Replace("'", "''") + "' and DATE_CREATED = '" + startDate + "'";
                isAdmDB = false;
                SqlDataReader sdr = Utils.ReturnReader(SQL, isAdmDB);
                if (sdr.Read())
                {
                    reviewID = sdr["REVIEW_ID"].ToString();
                }
                sdr.Close();
                Server.Transfer("ReviewDetails.aspx?ID=" + reviewID);
            }
        }
        else
        {
            // make sure all of the fields are filled in. Old reviews may have this issue.
            if (tbReviewTitle.Text == "" || tbRegistrationDate.Text == "")
            {
                lblMissingFields.Visible = true;
            }
            else
            {
                string expiryDate = null;
                DateTime registrationDate = Convert.ToDateTime(tbRegistrationDate.Text);
                string startDate = registrationDate.ToString("yyyy-M-d hh:m:ss.mmm"); // this is case sensitive!
                if (tbExpiryDate.Text != "")
                {
                    DateTime expirationDate = Convert.ToDateTime(tbExpiryDate.Text);
                    expiryDate = expirationDate.ToString("yyyy-M-d hh:m:ss.mmm"); // this is case sensitive!
                }

                SQL = "update TB_REVIEW set " +
                    "REVIEW_NAME = '" + tbReviewTitle.Text.Replace("'", "''") + "', " +
                    "DATE_CREATED = '" + startDate + "', ";
                if (expiryDate == null)
                    SQL += "EXPIRY_DATE = null, ";
                else
                {
                    SQL += "EXPIRY_DATE = '" + expiryDate + "', FUNDER_ID = '" + 
                        lblFunderID.Text + "', ";
                }
                SQL += "REVIEW_NUMBER = '" + tbReviewNumber.Text + "' " +
                    "where REVIEW_ID = '" + Request.QueryString["ID"].ToString() + "'";
                bool isAdmDB = false;
                Utils.ExecuteQuery(SQL, isAdmDB);
                Server.Transfer("ReviewDetails.aspx?ID=" + Request.QueryString["ID"].ToString());
            }
        }
        */
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
                tbRegistrationDate.Text = Utils.GetSessionString("CalendarDate").Substring(counterLocation + 1);
            }
            else
            {
                tbExpiryDate.Text = Utils.GetSessionString("CalendarDate").Substring(counterLocation + 1);
            }
        }
        Utils.SetSessionString("CalendarDate", "");
    }

    protected void cmdPlaceFunder_Click(object sender, EventArgs e)
    {

        lblFunderID.Text = Utils.GetSessionString("variableID");
        bool isAdmDB = true;
        IDataReader idr = Utils.GetReader(isAdmDB, "st_ContactDetails", Utils.GetSessionString("variableID"));
        if (idr.Read())
        {
            lblFunderName.Text = idr["CONTACT_NAME"].ToString();
        }
        idr.Close();
        Utils.SetSessionString("variableID", "");


        
        /*
        lblFunderID.Text = Utils.GetSessionString("variableID");
        string SQL = "select CONTACT_NAME from TB_CONTACT where CONTACT_ID = '" +
            Utils.GetSessionString("variableID") + "'";
        bool isAdmDB = false;
        SqlDataReader sdr = Utils.ReturnReader(SQL, isAdmDB);
        if (sdr.Read())
        {
            lblFunderName.Text = sdr["CONTACT_NAME"].ToString();            
        }
        sdr.Close(); 
        Utils.SetSessionString("variableID", "");
        */
    }

    protected void cmdAddContact_Click(object sender, EventArgs e)
    {
        if (Utils.GetSessionString("variableID") != "")
        {
            if (Utils.GetSessionString("variableID") != "")
            {
                bool isAdmDB = true;
                Utils.ExecuteSP(isAdmDB, Server, "st_ReviewAddContact",
                     lblReviewID.Text, Utils.GetSessionString("variableID"), lblER3ReviewID.Text);
            }
            Utils.SetSessionString("variableID", "");
            buildGrid();
        }
        

        /*
        if (Utils.GetSessionString("variableID") != "")
        {
            // see if user is already in review
            string SQL = "select * from TB_REVIEW_CONTACT where CONTACT_ID = '" + Utils.GetSessionString("variableID") +
                "' and REVIEW_ID = '" + lblReviewID.Text + "'";
            bool isAdmDB = false;
            SqlDataReader sdr = Utils.ReturnReader(SQL, isAdmDB);
            if (sdr.Read())
            {
                // it is in the review already so do nothing            
            }
            else
            {
                string oldContactID = "";
                SQL = "select old_contact_id from TB_CONTACT where CONTACT_ID = '" + Utils.GetSessionString("variableID") + "'";
                SqlDataReader sdr1 = Utils.ReturnReader(SQL, isAdmDB);
                if (sdr1.Read())
                {
                    oldContactID = sdr1["old_contact_id"].ToString();            
                }
                sdr1.Close();  
                SQL = "insert into TB_REVIEW_CONTACT (REVIEW_ID, CONTACT_ID, old_review_id, old_contact_id) " +
                "values ('" + lblReviewID.Text + "', '" + Utils.GetSessionString("variableID") +
                "', '" + lblER3ReviewID.Text + "', '" + oldContactID + "')";
                Utils.ExecuteQuery(SQL, isAdmDB);

                // get the new REVIEW_CONTACT_ID and add an entry to TB_CONTACT_REVIEW_ROLE
                SQL = "select * from TB_REVIEW_CONTACT where CONTACT_ID = '" + Utils.GetSessionString("variableID") +
                "' and REVIEW_ID = '" + lblReviewID.Text + "'";
                SqlDataReader sdr2 = Utils.ReturnReader(SQL, isAdmDB);
                if (sdr2.Read())
                {
                    SQL = "insert into TB_CONTACT_REVIEW_ROLE (REVIEW_CONTACT_ID, ROLE_NAME) " +
                        "values ('" + sdr2["REVIEW_CONTACT_ID"].ToString() + "', 'RegularUser')";
                    Utils.ExecuteQuery(SQL, isAdmDB);            
                }
                sdr2.Close();
                
            }
            sdr.Close();         
        }
        Utils.SetSessionString("variableID", "");
        buildGrid();
        */
    }

    protected void gvContacts_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        string SQL = "";
        bool isAdmDB = false;
        string reviewContactID = "";
        //SqlDataReader sdr;
        int index = Convert.ToInt32(e.CommandArgument);
        string contactID = (string)gvContacts.DataKeys[index].Value;
        switch (e.CommandName)
        {
            case "REMOVE":
                isAdmDB = true;
                Utils.ExecuteSP(isAdmDB, Server, "st_ReviewRemoveMember_1",
                    lblReviewID.Text, contactID);



                /*
                reviewContactID = "";
                isAdmDB = false;
                SQL = "select * from TB_REVIEW_CONTACT where CONTACT_ID = '" + contactID +
                "' and REVIEW_ID = '" + lblReviewID.Text + "'";
                sdr = Utils.ReturnReader(SQL, isAdmDB);
                if (sdr.Read())
                {
                    reviewContactID = sdr["REVIEW_CONTACT_ID"].ToString();
                }
                sdr.Close();
                SQL = "delete from TB_CONTACT_REVIEW_ROLE where REVIEW_CONTACT_ID = '" + reviewContactID + "'";
                Utils.ExecuteQuery(SQL, isAdmDB);
                SQL = "delete from TB_REVIEW_CONTACT where CONTACT_ID = '" + contactID +
                    "' and REVIEW_ID = '" + lblReviewID.Text + "'";
                Utils.ExecuteQuery(SQL, isAdmDB);
                */
                
                Server.Transfer("ReviewDetails.aspx?ID=" + Request.QueryString["ID"].ToString());
                break;

            case "SAVE_ROLE":    
                isAdmDB = true;

                SqlParameter[] paramList = new SqlParameter[3];
                paramList[0] = new SqlParameter("@REVIEW_ID", SqlDbType.NVarChar, 50, ParameterDirection.Input,
                    true, 0, 0, null, DataRowVersion.Default, lblReviewID.Text);
                paramList[1] = new SqlParameter("@CONTACT_ID", SqlDbType.NVarChar, 255, ParameterDirection.Input,
                    true, 0, 0, null, DataRowVersion.Default, contactID);
                paramList[2] = new SqlParameter("@REVIEW_CONTACT_ID", SqlDbType.NVarChar, 50, ParameterDirection.Output,
                true, 0, 0, null, DataRowVersion.Default, "");
                Utils.ExecuteSPWithReturnValues(isAdmDB, Server, "st_ContactReviewDeleteRoles", paramList);

                reviewContactID = paramList[2].Value.ToString();

                // collect and save selected roles
                GridViewRow row = gvContacts.Rows[index];
                CheckBoxList cbl = ((CheckBoxList)row.FindControl("cblContactReviewRole"));
                for (int x = 0; x < cbl.Items.Count; x++) // through each role
                {
                    if (cbl.Items[x].Selected == true)
                    {
                        Utils.ExecuteSP(isAdmDB, Server, "st_ReviewRoleUpdate",
                            reviewContactID, cbl.Items[x].Text);
                    }
                }
                Server.Transfer("ReviewDetails.aspx?ID=" + Request.QueryString["ID"].ToString());


                /*
                // delete existing roles
                reviewContactID = "";
                isAdmDB = false;
                SQL = "delete from TB_CONTACT_REVIEW_ROLE where REVIEW_CONTACT_ID in " +
                    "(select REVIEW_CONTACT_ID from TB_REVIEW_CONTACT where CONTACT_ID = '" +
                    contactID + "' and REVIEW_ID = '" + lblReviewID.Text + "')";
                Utils.ExecuteQuery(SQL, isAdmDB);
                // insert new roles
                SQL = "select REVIEW_CONTACT_ID from TB_REVIEW_CONTACT where CONTACT_ID = '" + 
                    contactID + "' and REVIEW_ID = '" + lblReviewID.Text + "'";
                sdr = Utils.ReturnReader(SQL, isAdmDB);
                if (sdr.Read())
                {
                    reviewContactID = sdr["REVIEW_CONTACT_ID"].ToString();
                }
                sdr.Close();

                GridViewRow row = gvContacts.Rows[index];
                CheckBoxList cbl = ((CheckBoxList)row.FindControl("cblContactReviewRole"));
                for (int x = 0; x < cbl.Items.Count; x++) // through each role
                {
                    if (cbl.Items[x].Selected == true)
                    {
                        SQL = "insert into TB_CONTACT_REVIEW_ROLE (REVIEW_CONTACT_ID, ROLE_NAME) " +
                            "values ('" + reviewContactID + "', '" + cbl.Items[x].Text + "')";
                        Utils.ExecuteQuery(SQL, isAdmDB);
                    }
                }
                
                Server.Transfer("ReviewDetails.aspx?ID=" + Request.QueryString["ID"].ToString());
                */ 
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
                    Utils.GetSessionString("Contact_ID"), lblReviewID.Text, 0);
                getOpenAIDetails();
                break;

            default:
                break;
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
            string ContactID = (string)gvContacts.DataKeys[row.RowIndex].Value.ToString();
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
            Utils.ExecuteSP(isAdmDB, Server, "st_ReviewRoleUpdateByContactID", lblReviewID.Text, ContactID, role);
        }
    }

    protected void gvContacts_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            LinkButton lb = (LinkButton)(e.Row.Cells[6].Controls[0]);
            lb.Attributes.Add("onclick", "if (confirm('Are you sure you want to remove this user from this review?') == false) return false;");
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

    protected void cmdNullExpiry_Click(object sender, EventArgs e)
    {
        tbExpiryDate.Text = null;
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

    protected void lbBritishLibrary_Click(object sender, EventArgs e)
    {
        if (lbBritishLibrary.Text == "View")
        {
            lbBritishLibrary.Text = "Hide";
            pnlBL1.Visible = true;
            pnlBL2.Visible = true;
        }
        else
        {
            lbBritishLibrary.Text = "View";
            pnlBL1.Visible = false;
            pnlBL2.Visible = false;
        }
    }

    private void buildExtensionGrid()
    {
        DataTable dt = new DataTable();
        System.Data.DataRow newrow;

        dt.Columns.Add(new DataColumn("EXPIRY_EDIT_ID", typeof(string)));
        dt.Columns.Add(new DataColumn("DATE_OF_EDIT", typeof(string)));
        dt.Columns.Add(new DataColumn("OLD_EXPIRY_DATE", typeof(string)));
        dt.Columns.Add(new DataColumn("NEW_EXPIRY_DATE", typeof(string)));
        dt.Columns.Add(new DataColumn("CONTACT_NAME", typeof(string)));
        dt.Columns.Add(new DataColumn("EXTENSION_TYPE", typeof(string)));
        dt.Columns.Add(new DataColumn("EXTENSION_NOTES", typeof(string)));

        bool isAdmDB = true;
        IDataReader idr = Utils.GetReader(isAdmDB, "st_ExtensionRecordGet_1", Request.QueryString["ID"].ToString(), "0");
        while (idr.Read())
        {
            newrow = dt.NewRow();
            newrow["EXPIRY_EDIT_ID"] = idr["EXPIRY_EDIT_ID"].ToString();
            newrow["DATE_OF_EDIT"] = idr["DATE_OF_EDIT"].ToString();
            if (idr["OLD_EXPIRY_DATE"].ToString() == "")
                newrow["OLD_EXPIRY_DATE"] = "n/a";
            else
                newrow["OLD_EXPIRY_DATE"] = idr["OLD_EXPIRY_DATE"].ToString().Remove(10);
            if (idr["NEW_EXPIRY_DATE"].ToString() == "")
                newrow["NEW_EXPIRY_DATE"] = "n/a";
            else
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

    protected void cbShowScreening_CheckedChanged(object sender, EventArgs e)
    {
        bool isAdmDB = true;
        Utils.ExecuteSP(isAdmDB, Server, "st_PriorityScreeningTurnOnOff",
                lblReviewID.Text, "ShowScreening", cbShowScreening.Checked);
    }
    protected void cbAllowReviewerTerms_CheckedChanged(object sender, EventArgs e)
    {
        bool isAdmDB = true;
        Utils.ExecuteSP(isAdmDB, Server, "st_PriorityScreeningTurnOnOff",
                lblReviewID.Text, "AllowReviewerTerms", cbAllowReviewerTerms.Checked);
    }
    protected void cbAllowClusteredSearch_CheckedChanged(object sender, EventArgs e)
    {
        bool isAdmDB = true;
        Utils.ExecuteSP(isAdmDB, Server, "st_PriorityScreeningTurnOnOff",
                lblReviewID.Text, "AllowClusteredSearch", cbAllowClusteredSearch.Checked);
    }
    protected void cbEnableMag_CheckedChanged(object sender, EventArgs e)
    {       
        int setting = 0;
        if (cbEnableMag.Checked)
            setting = 1;

        bool isAdmDB = true;
        Utils.ExecuteSP(isAdmDB, Server, "st_EnableMag",
                lblReviewID.Text, setting);
    }

    protected void cbEnableOpenAI_CheckedChanged(object sender, EventArgs e)
    {
        bool setting = false;
        if (cbEnableOpenAI.Checked)
            setting = true;

        bool isAdmDB = true;
        Utils.ExecuteSP(isAdmDB, Server, "st_EnableOpenAI",
                lblReviewID.Text, setting);
    }
    protected void cbPotential_CheckedChanged(object sender, EventArgs e)
    {
        if (cbPotential.Checked == true)
        {
            tbArchieID.Text = "prospective_______";
            tbArchieID.Enabled = false;
        }
        else
        {
            tbArchieID.Enabled = true;
            tbArchieID.Text = "";
        }
    }
    protected void lbSave_Click(object sender, EventArgs e)
    {
        bool isAdmDB = true;
        Utils.ExecuteSP(isAdmDB, Server, "st_ArchieIDSave", lblReviewID.Text, tbArchieID.Text);
    }
    protected void cmdNullExpiry0_Click(object sender, EventArgs e)
    {
        tbArchieID.Text = null;
        tbArchieID.Enabled = true;
        cbPotential.Checked = false;
    }
    protected void lbSave0_Click(object sender, EventArgs e)
    {
        bool isAdmDB = true;
        Utils.ExecuteSP(isAdmDB, Server, "st_ArchieCDSave", lblReviewID.Text, tbArchieCD.Text);
    }
    protected void cbIsCheckedOutHere_CheckedChanged(object sender, EventArgs e)
    {
        bool isAdmDB = true;
        Utils.ExecuteSP(isAdmDB, Server, "st_ArchieIsCheckedOutHere", lblReviewID.Text, cbIsCheckedOutHere.Checked);
    }
    protected void lbSave1_Click(object sender, EventArgs e)
    {
        bool isAdmDB = true;
        Utils.ExecuteSP(isAdmDB, Server, "st_ArchieCheckedOutByID", lblReviewID.Text, tbCheckedOutBy.Text);
    }
    protected void lbSaveBritLibPrivilege_Click(object sender, EventArgs e)
    {
        bool isAdmDB = true;
        Utils.ExecuteSP(isAdmDB, Server, "st_BritishLibraryValuesSet", lblReviewID.Text,
            tbBritLibPrivilegeAccountCode.Text, tbBritLibPrivilegeAuthCode.Text, tbBritLibPrivilegeTxLine.Text);
    }
    protected void lbSaveBritLibCopyrightCleard_Click(object sender, EventArgs e)
    {
        bool isAdmDB = true;
        Utils.ExecuteSP(isAdmDB, Server, "st_BritishLibraryCCValuesSet", lblReviewID.Text,
            tbBritLibCRClearedAccountCode.Text, tbBritLibCRClearedAuthCode.Text, tbBritLibCRClearedTxLine.Text);
    }



    protected void lbShowHide_Click(object sender, EventArgs e)
    {
        if (lbShowHide.Text == "Show")
        {
            pnlDetailedExtensionHistory.Visible = true;
            lbShowHide.Text = "Hide";


            DataTable dt = new DataTable();
            System.Data.DataRow newrow;

            DataTable dt1 = new DataTable();
            System.Data.DataRow newrow1;

            dt.Columns.Add(new DataColumn("CONTACT_NAME", typeof(string)));
            dt.Columns.Add(new DataColumn("CONTACT_ID", typeof(string)));
            dt.Columns.Add(new DataColumn("EMAIL", typeof(string)));
            dt.Columns.Add(new DataColumn("EXPIRY_EDIT_ID", typeof(string)));
            dt.Columns.Add(new DataColumn("DATE_OF_EDIT", typeof(string)));
            dt.Columns.Add(new DataColumn("TYPE_EXTENDED", typeof(string)));
            dt.Columns.Add(new DataColumn("ID_EXTENDED", typeof(string)));
            dt.Columns.Add(new DataColumn("OLD_EXPIRY_DATE", typeof(string)));
            dt.Columns.Add(new DataColumn("NEW_EXPIRY_DATE", typeof(string)));
            dt.Columns.Add(new DataColumn("EXTENDED_BY_ID", typeof(string)));
            dt.Columns.Add(new DataColumn("EXTENSION_TYPE_ID", typeof(string)));
            dt.Columns.Add(new DataColumn("EXTENSION_NOTES", typeof(string)));
            dt.Columns.Add(new DataColumn("EXTENSION_TYPE", typeof(string)));
            dt.Columns.Add(new DataColumn("Months_Ext", typeof(string)));
            dt.Columns.Add(new DataColumn("£", typeof(string)));

            dt1.Columns.Add(new DataColumn("REVIEW_NAME", typeof(string)));
            dt1.Columns.Add(new DataColumn("REVIEW_ID", typeof(string)));
            dt1.Columns.Add(new DataColumn("EXPIRY_EDIT_ID", typeof(string)));
            dt1.Columns.Add(new DataColumn("DATE_OF_EDIT", typeof(string)));
            dt1.Columns.Add(new DataColumn("TYPE_EXTENDED", typeof(string)));
            dt1.Columns.Add(new DataColumn("ID_EXTENDED", typeof(string)));
            dt1.Columns.Add(new DataColumn("OLD_EXPIRY_DATE", typeof(string)));
            dt1.Columns.Add(new DataColumn("NEW_EXPIRY_DATE", typeof(string)));
            dt1.Columns.Add(new DataColumn("EXTENDED_BY_ID", typeof(string)));
            dt1.Columns.Add(new DataColumn("EXTENSION_TYPE_ID", typeof(string)));
            dt1.Columns.Add(new DataColumn("EXTENSION_NOTES", typeof(string)));
            dt1.Columns.Add(new DataColumn("EXTENSION_TYPE", typeof(string)));
            dt1.Columns.Add(new DataColumn("Months_Ext", typeof(string)));
            dt1.Columns.Add(new DataColumn("£", typeof(string)));
            /*
            CONTACT_NAME
            CONTACT_ID
            EMAIL
            EXPIRY_EDIT_ID
            DATE_OF_EDIT
            TYPE_EXTENDED
            ID_EXTENDED
            OLD_EXPIRY_DATE
            NEW_EXPIRY_DATE
            EXTENDED_BY_ID
            EXTENSION_TYPE_ID
            EXTENSION_NOTES
            EXTENSION_TYPE
            Months_Ext
            £
            */
            bool isAdmDB = true;
            IDataReader idr = Utils.GetReader(isAdmDB, "st_DetailedExtensionRecordGet", Request.QueryString["ID"].ToString(), "0");
            while (idr.Read())
            {
                newrow = dt.NewRow();
                newrow["CONTACT_NAME"] = idr["CONTACT_NAME"].ToString();
                newrow["CONTACT_ID"] = idr["CONTACT_ID"].ToString();
                newrow["EMAIL"] = idr["EMAIL"].ToString();
                newrow["EXPIRY_EDIT_ID"] = idr["EXPIRY_EDIT_ID"].ToString();
                newrow["DATE_OF_EDIT"] = idr["DATE_OF_EDIT"].ToString();
                newrow["TYPE_EXTENDED"] = idr["TYPE_EXTENDED"].ToString();
                newrow["ID_EXTENDED"] = idr["ID_EXTENDED"].ToString();
                newrow["OLD_EXPIRY_DATE"] = idr["OLD_EXPIRY_DATE"].ToString();
                newrow["NEW_EXPIRY_DATE"] = idr["NEW_EXPIRY_DATE"].ToString();
                newrow["EXTENDED_BY_ID"] = idr["EXTENDED_BY_ID"].ToString();
                newrow["EXTENSION_TYPE_ID"] = idr["EXTENSION_TYPE_ID"].ToString();
                newrow["EXTENSION_NOTES"] = idr["EXTENSION_NOTES"].ToString();
                newrow["EXTENSION_TYPE"] = idr["EXTENSION_TYPE"].ToString();
                newrow["Months_Ext"] = idr["Months_Ext"].ToString();
                newrow["£"] = idr["£"].ToString();
                dt.Rows.Add(newrow);
            }
            idr.NextResult();
            while (idr.Read())
            {
                newrow1 = dt1.NewRow();
                newrow1["REVIEW_NAME"] = idr["REVIEW_NAME"].ToString();
                newrow1["REVIEW_ID"] = idr["REVIEW_ID"].ToString();
                newrow1["EXPIRY_EDIT_ID"] = idr["EXPIRY_EDIT_ID"].ToString();
                newrow1["DATE_OF_EDIT"] = idr["DATE_OF_EDIT"].ToString();
                newrow1["TYPE_EXTENDED"] = idr["TYPE_EXTENDED"].ToString();
                newrow1["ID_EXTENDED"] = idr["ID_EXTENDED"].ToString();
                newrow1["OLD_EXPIRY_DATE"] = idr["OLD_EXPIRY_DATE"].ToString();
                newrow1["NEW_EXPIRY_DATE"] = idr["NEW_EXPIRY_DATE"].ToString();
                newrow1["EXTENDED_BY_ID"] = idr["EXTENDED_BY_ID"].ToString();
                newrow1["EXTENSION_TYPE_ID"] = idr["EXTENSION_TYPE_ID"].ToString();
                newrow1["EXTENSION_NOTES"] = idr["EXTENSION_NOTES"].ToString();
                newrow1["EXTENSION_TYPE"] = idr["EXTENSION_TYPE"].ToString();
                newrow1["Months_Ext"] = idr["MonthsExt"].ToString();
                newrow1["£"] = idr["£"].ToString();
                dt1.Rows.Add(newrow1);
            }
            idr.Close();

            gvDetailedContactExtension.DataSource = dt;
            gvDetailedContactExtension.DataBind();

            gvDetailedReviewExtension.DataSource = dt1;
            gvDetailedReviewExtension.DataBind();
        }
        else
        {
            pnlDetailedExtensionHistory.Visible = false;
            lbShowHide.Text = "Show";
        }

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
                true, 0, 0, null, DataRowVersion.Default, lblReviewID.Text);
            paramList[2] = new SqlParameter("@LICENSE_ID", SqlDbType.Int, 8, ParameterDirection.Input,
                true, 0, 0, null, DataRowVersion.Default, 0);
            paramList[3] = new SqlParameter("@CONTACT_ID", SqlDbType.Int, 8, ParameterDirection.Input,
                true, 0, 0, null, DataRowVersion.Default, Utils.GetSessionString("Contact_ID"));
            paramList[4] = new SqlParameter("@RESULT", SqlDbType.NVarChar, 100, ParameterDirection.Output,
                true, 0, 0, null, DataRowVersion.Default, "");


            Utils.ExecuteSPWithReturnValues(isAdmDB, Server, "st_SetCreditPurchaseIDForOpenAI", paramList);

            if (paramList[4].Value.ToString() == "SUCCESS")
            {
                getOpenAIDetails();
                lblInvalidID.Visible = false;
                tbCreditPurchaseID.Text = "";
            }
            else
            {
                lblInvalidID.Visible = true;
            }
        }
    }
}
