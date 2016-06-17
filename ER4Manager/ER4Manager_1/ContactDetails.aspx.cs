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

public partial class ContactDetails : System.Web.UI.Page
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
                        lbl.Text = "Contact details";
                    }

                    Telerik.Web.UI.RadTabStrip radTs = (Telerik.Web.UI.RadTabStrip)Master.FindControl("rtsMenu");
                    if (radTs != null)
                    {
                        radTs.SelectedIndex = 3;
                        radTs.Tabs[3].Tabs[1].Selected = true;
                        radTs.Tabs[3].Tabs[2].Width = 570;
                    }
                    System.Web.UI.WebControls.Label lbl1 = (Label)Master.FindControl("lblHeadingText");
                    if (lbl1 != null)
                    {
                        lbl1.Text = "Contact details";
                    }

                    DataTable dt = new DataTable();
                    System.Data.DataRow newrow;

                    dt.Columns.Add(new DataColumn("EXTENSION_TYPE_ID", typeof(string)));
                    dt.Columns.Add(new DataColumn("EXTENSION_TYPE", typeof(string)));

                    newrow = dt.NewRow();
                    newrow["EXTENSION_TYPE_ID"] = "0";
                    newrow["EXTENSION_TYPE"] = "Select a type";
                    dt.Rows.Add(newrow);

                    bool isAdmDB = true;
                    IDataReader idr = Utils.GetReader(isAdmDB, "st_ExtensionTypesGet", "Contact");
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
                    lblInSiteLicense.Visible = false;

                    if (Request.QueryString["ID"].ToString() == "New")
                    {
                        tbPassword.Visible = true;
                        cmdShowHidePassword.Text = "Hide";
                        ddlExtensionType.Enabled = false;
                        tbExtensionDetails.Enabled = false;
                        lbExtensionHistory.Enabled = false;
                        cmdGeneratePassword.Visible = true;
                    }
                    else
                    {
                        ddlExtensionType.Enabled = true;
                        tbExtensionDetails.Enabled = true;
                        lbExtensionHistory.Enabled = true;
                        isAdmDB = true;
                        idr = Utils.GetReader(isAdmDB, "st_ContactDetails",
                            Request.QueryString["ID"].ToString());
                        if (idr.Read())
                        {
                            lblContactID.Text = idr["CONTACT_ID"].ToString();
                            tbContactName.Text = idr["CONTACT_NAME"].ToString().Replace("''", "'");
                            tbUserName.Text = idr["USERNAME"].ToString();
                            //tbPassword.Text = idr["PASSWORD"].ToString();
                            tbEmail.Text = idr["EMAIL"].ToString();
                            tbAbout.Text = idr["DESCRIPTION"].ToString();
                            lblHoursActive.Text = idr["active_hours"].ToString();
                            if ((idr["DATE_CREATED"].ToString() == null) || (idr["DATE_CREATED"].ToString() == ""))
                                tbDateCreated.Text = "N/A";
                            else
                                tbDateCreated.Text = idr["DATE_CREATED"].ToString();
                            if ((idr["EXPIRY_DATE"].ToString() == null) || (idr["EXPIRY_DATE"].ToString() == ""))
                                tbDateExpiry.Text = "";
                            else
                                tbDateExpiry.Text = idr["EXPIRY_DATE"].ToString();
                            ddlMonthsCredit.SelectedValue = idr["MONTHS_CREDIT"].ToString();
                            if ((idr["LAST_LOGIN"].ToString() == null) || (idr["LAST_LOGIN"].ToString() == ""))
                                lblLastLogin.Text = "N/A";
                            else
                                lblLastLogin.Text = idr["LAST_LOGIN"].ToString();
                            lblER3ContactID.Text = idr["old_contact_id"].ToString();
                            if (idr["IS_SITE_ADMIN"].ToString() == "True")
                                ddlERRole.SelectedIndex = 1;
                            else
                                ddlERRole.SelectedIndex = 0;
                            if (idr["CREATOR_ID"].ToString() == idr["CONTACT_ID"].ToString())
                            {
                                ddlCreatorID.SelectedIndex = 0;
                            }
                            else
                            {
                                ddlCreatorID.SelectedIndex = 1;
                            }
                            if (idr["SITE_LIC_ID"].ToString() != "")
                            {
                                lblInSiteLicense.Visible = true;
                                lblInSiteLicense.Text = "In license #" + idr["SITE_LIC_ID"].ToString();
                            }
                            if (idr["SEND_NEWSLETTER"].ToString() == "True")
                            {
                                cbSendNewsletter.Checked = true;
                            }
                            tbArchieID.Text = idr["ARCHIE_ID"].ToString();
                        }
                        idr.Close();

                        buildGrid();
                        
                    }
                }
                IBCalendar1.Attributes.Add("onclick", "JavaScript:openCalendar1('" +
                    "!1!" + tbDateCreated.Text + "')");
                IBCalendar2.Attributes.Add("onclick", "JavaScript:openCalendar1('" +
                    "!2!" + tbDateExpiry.Text + "')");
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
        DataTable dt = new DataTable();
        System.Data.DataRow newrow;

        dt.Columns.Add(new DataColumn("REVIEW_ID", typeof(string)));
        dt.Columns.Add(new DataColumn("REVIEW_NAME", typeof(string)));
        dt.Columns.Add(new DataColumn("DATE_CREATED", typeof(string)));
        dt.Columns.Add(new DataColumn("HOURS", typeof(string)));

        bool isAdmDB = true;
        IDataReader idr = Utils.GetReader(isAdmDB, "st_ContactReviews", Request.QueryString["ID"].ToString());
        while (idr.Read())
        {
            newrow = dt.NewRow();
            newrow["REVIEW_ID"] = idr["REVIEW_ID"].ToString();
            newrow["REVIEW_NAME"] = idr["REVIEW_NAME"].ToString();
            newrow["DATE_CREATED"] = idr["DATE_CREATED"].ToString();
            newrow["HOURS"] = idr["HOURS"].ToString();
            dt.Rows.Add(newrow);
        }
        idr.Close();

        //Grid1.DataSource = dt;
        //Grid1.DataBind();
    }

    protected void cmdSave_Click(object sender, EventArgs e)
    {
        lblMissingFields.Visible = false;
        lblMissingFields.Text = "Please fill in all of the fields *";
        lblMissingFields.ForeColor = System.Drawing.Color.Red;
        
        string SQL = "";
        SqlDataReader sdr;
        bool isAdmDB = false;
        bool emailConditionsMet = false;
        bool usernameConditionsMet = false;
        bool passwordConditionsMet = false;
        bool accountConditionsMet = true;

        Regex passwordRegex = new Regex("^.*(?=.{8,})(?=.*\\d)(?=.*[a-z])(?=.*[A-Z]).*$");

        //make sure all of the fields are filled in
        if (tbContactName.Text == "" || tbUserName.Text.Contains("'") || tbUserName.Text == "" /*|| tbPassword.Text == "" ||
            tbPassword.Text.Contains("'")*/ || tbDateCreated.Text == "" || tbEmail.Text == "" || tbEmail.Text.Contains("'"))
        {
            lblMissingFields.Visible = true;
            lblMissingFields.Text = "Please fill in all of the fields *";
            lblMissingFields.ForeColor = System.Drawing.Color.Red;
            accountConditionsMet = false;
        }
        else if (tbUserName.Text.Length < 4)
        {
            lblMissingFields.Visible = true;
            lblMissingFields.Text = "User name must be atleast 4 characters long";
            lblMissingFields.ForeColor = System.Drawing.Color.Red;
            accountConditionsMet = false;
            /*
            accountConditionsMet = false;
            lblUsername.Visible = true;
            lblUsername.Text = "User name must be atleast 4 characters long";
            */
        }
        // check if it has a @ in it with characters on both sides          
        else if (tbEmail.Text.IndexOf("@") < 0 ||
                (tbEmail.Text.IndexOf("@") == tbEmail.Text.Length - 1))
        {
            lblMissingFields.Visible = true;
            lblMissingFields.Text = "The email address does not appear to be valid";
            lblMissingFields.ForeColor = System.Drawing.Color.Red;
            accountConditionsMet = false;
            
            /*
            lblEmailAddress.Visible = true;
            lblEmailAddress.Text = "The email address does not appear to be valid";
            accountConditionsMet = false;
            */
        }
        //passwordConditionsMet = true;

        else if (tbPassword.Text != "")
        {
            Match m = passwordRegex.Match(tbPassword.Text.ToString());
            if (m.Success)
            {
                passwordConditionsMet = true;
            }
            else
            {
                lblMissingFields.Visible = true;
                lblMissingFields.Text = "Passwords must be at least 8 characters and contain at least one one lower case letter, one upper case letter and one digit.";
                lblMissingFields.ForeColor = System.Drawing.Color.Red;
                accountConditionsMet = false;
            }
        }
        

        if ((ddlMonthsCredit.SelectedIndex > 0) && (tbDateExpiry.Text != ""))
        {
            lblMissingFields.Visible = true;
            lblMissingFields.Text = "Months credit must have a null expiry date";
            lblMissingFields.ForeColor = System.Drawing.Color.Red;
            accountConditionsMet = false;
        }

        if ((ddlMonthsCredit.SelectedIndex == 0) && (tbDateExpiry.Text == ""))
        {
            lblMissingFields.Visible = true;
            lblMissingFields.Text = "0 Months credit must have an expiry date";
            lblMissingFields.ForeColor = System.Drawing.Color.Red;
            accountConditionsMet = false;
        }

        if ((ddlExtensionType.SelectedIndex == 0) && (Request.QueryString["ID"].ToString() != "New"))
        {
            lblMissingFields.Visible = true;
            lblMissingFields.Text = "Select an extension type";
            lblMissingFields.ForeColor = System.Drawing.Color.Red;
            accountConditionsMet = false;
            /*
            lblMissingFields.Visible = true;
            accountConditionsMet = false;
            */
        }
        if (accountConditionsMet)
        {
            // data is there. check if email and username is unique and if password conditions are met.
            
            SqlParameter[] paramList = new SqlParameter[4];

            paramList[0] = new SqlParameter("@CONTACT_ID", SqlDbType.Int);
            paramList[0].Direction = ParameterDirection.Input;
            if (lblContactID.Text == "N/A")
                lblContactID.Text = "0";
            paramList[0].Value = lblContactID.Text;

            paramList[1] = new SqlParameter("@USERNAME", SqlDbType.NVarChar);
            paramList[1].Direction = ParameterDirection.Input;
            paramList[1].Value = tbUserName.Text;

            paramList[2] = new SqlParameter("@EMAIL", SqlDbType.NVarChar);
            paramList[2].Direction = ParameterDirection.Input;
            paramList[2].Value = tbEmail.Text;

            paramList[3] = new SqlParameter("@RESULT", SqlDbType.NVarChar);
            paramList[3].Direction = ParameterDirection.Output;
            paramList[3].Size = 100;
            paramList[3].Value = "";

            Utils.ExecuteSPWithReturnValues(true, Server, "st_CheckUserNameAndEmail", paramList);
            string res = paramList[3].Value.ToString();
            if (res != "Valid")
            {
                lblMissingFields.Visible = true;
                lblMissingFields.Text = res;
                lblMissingFields.ForeColor = System.Drawing.Color.Red;
                accountConditionsMet = false;
                /*
                lblUsername.Visible = true;
                lblUsername.Text = res;
                accountConditionsMet = false;
                */
            }
        }

        string startDate = "";
        string expired = "";
        try
        {
            DateTime registrationDate = Convert.ToDateTime(tbDateCreated.Text);
            startDate = registrationDate.ToString("yyyy-M-d hh:m:ss.mmm"); // this is case sensitive!
        }
        catch (Exception er)
        {
            lblMissingFields.Visible = true;
            lblMissingFields.Text = "Date error: " + er.Message.ToString();
            lblMissingFields.ForeColor = System.Drawing.Color.Red;
            accountConditionsMet = false;
        }

        if (tbDateExpiry.Text != "")
        {
            try
            {
                DateTime expiryDate = Convert.ToDateTime(tbDateExpiry.Text);
                expired = expiryDate.ToString("yyyy-M-d hh:m:ss.mmm"); // this is case sensitive!
            }
            catch (Exception er)
            {
                lblMissingFields.Visible = true;
                lblMissingFields.Text = "Date error: " + er.Message.ToString();
                lblMissingFields.ForeColor = System.Drawing.Color.Red;
                accountConditionsMet = false;
            }
        }


        if (accountConditionsMet == true)
        {
            string notes = tbExtensionDetails.Text;
            bool newAccount = false;
            if (Request.QueryString["ID"].ToString() == "New")
                newAccount = true;

            string creatorID = lblContactID.Text;
            if (ddlCreatorID.SelectedValue == "AdminsID")
                creatorID = Utils.GetSessionString("Contact_ID");
            if (tbDateCreated.Text == "N/A")
                tbDateCreated.Text = null;

            // we know it is OK
            //DateTime registrationDate = Convert.ToDateTime(tbDateCreated.Text);
            //string startDate = registrationDate.ToString("yyyy-M-d hh:m:ss.mmm"); // this is case sensitive!
            //DateTime expiryDate = Convert.ToDateTime(tbDateExpiry.Text);
            //string expired = expiryDate.ToString("yyyy-M-d hh:m:ss.mmm"); // this is case sensitive!
            

            isAdmDB = true;
            SqlParameter[] paramList = new SqlParameter[17];

            paramList[0] = new SqlParameter("@NEW_ACCOUNT", SqlDbType.NVarChar, 50, ParameterDirection.Input,
                true, 0, 0, null, DataRowVersion.Default, newAccount);
            paramList[1] = new SqlParameter("@CONTACT_NAME", SqlDbType.NVarChar, 255, ParameterDirection.Input,
                true, 0, 0, null, DataRowVersion.Default, tbContactName.Text.Replace("'", "''"));
            paramList[2] = new SqlParameter("@USERNAME", SqlDbType.NVarChar, 50, ParameterDirection.Input,
                true, 0, 0, null, DataRowVersion.Default, tbUserName.Text);
            paramList[3] = new SqlParameter("@PASSWORD", SqlDbType.NVarChar, 50, ParameterDirection.Input,
                true, 0, 0, null, DataRowVersion.Default, tbPassword.Text);
            paramList[4] = new SqlParameter("@DATE_CREATED", SqlDbType.NVarChar, 50, ParameterDirection.Input,
                true, 0, 0, null, DataRowVersion.Default, startDate);
            paramList[5] = new SqlParameter("@EXPIRY_DATE", SqlDbType.NVarChar, 50, ParameterDirection.Input,
                true, 0, 0, null, DataRowVersion.Default, expired);
            paramList[6] = new SqlParameter("@CREATOR_ID", SqlDbType.NVarChar, 50, ParameterDirection.Input,
                true, 0, 0, null, DataRowVersion.Default, creatorID);
            paramList[7] = new SqlParameter("@EMAIL", SqlDbType.NVarChar, 500, ParameterDirection.Input,
                true, 0, 0, null, DataRowVersion.Default, tbEmail.Text);
            paramList[8] = new SqlParameter("@DESCRIPTION", SqlDbType.NVarChar, 1000, ParameterDirection.Input,
                true, 0, 0, null, DataRowVersion.Default, tbAbout.Text);
            paramList[9] = new SqlParameter("@IS_SITE_ADMIN", SqlDbType.NVarChar, 1, ParameterDirection.Input,
                true, 0, 0, null, DataRowVersion.Default, ddlERRole.SelectedValue);
            if (Request.QueryString["ID"].ToString() == "New")
            {
                paramList[10] = new SqlParameter("@CONTACT_ID", SqlDbType.NVarChar, 50, ParameterDirection.Input,
                    true, 0, 0, null, DataRowVersion.Default, Utils.GetSessionString("Contact_ID")); // this is the creator ID
            }
            else
            {
                paramList[10] = new SqlParameter("@CONTACT_ID", SqlDbType.NVarChar, 50, ParameterDirection.Input,
                    true, 0, 0, null, DataRowVersion.Default, Request.QueryString["ID"].ToString());
            }
            paramList[11] = new SqlParameter("@EXTENSION_TYPE_ID", SqlDbType.NVarChar, 50, ParameterDirection.Input,
                    true, 0, 0, null, DataRowVersion.Default, ddlExtensionType.SelectedValue);
            
            if (tbExtensionDetails.Text == "Enter further details (optional)")
                notes = "";
            paramList[12] = new SqlParameter("@EXTENSION_NOTES", SqlDbType.NVarChar, 500, ParameterDirection.Input,
                    true, 0, 0, null, DataRowVersion.Default, notes);
            paramList[13] = new SqlParameter("@EDITOR_ID", SqlDbType.NVarChar, 50, ParameterDirection.Input,
                    true, 0, 0, null, DataRowVersion.Default, Utils.GetSessionString("Contact_ID"));
            paramList[14] = new SqlParameter("@MONTHS_CREDIT", SqlDbType.NVarChar, 50, ParameterDirection.Input,
                    true, 0, 0, null, DataRowVersion.Default, ddlMonthsCredit.SelectedValue);
            paramList[15] = new SqlParameter("@ARCHIE_ID", SqlDbType.NVarChar, 50, ParameterDirection.Input,
                    true, 0, 0, null, DataRowVersion.Default, tbArchieID.Text);


            paramList[16] = new SqlParameter("@RESULT", SqlDbType.NVarChar, 50, ParameterDirection.Output,
                true, 0, 0, null, DataRowVersion.Default, "");              
             
            //Utils.ExecuteSPWithReturnValues(isAdmDB, Server, "st_ContactFullCreateOrEdit", paramList);
            Utils.ExecuteSPWithReturnValues(isAdmDB, Server, "st_ContactDetailsFullCreateOrEdit", paramList);
            

            if (paramList[16].Value.ToString() == "Invalid")
            {
                lblMissingFields.Visible = true;
                lblMissingFields.Text = "the SQL statement has failed and has been rolled back";
                lblMissingFields.ForeColor = System.Drawing.Color.Red;
                accountConditionsMet = false;
                /*
                lblUsername.Visible = true;
                lblUsername.Text = "the SQL statement has failed and has been rolled back";
                accountConditionsMet = false;
                */
            }
            else
            {
                if (newAccount == true)
                {
                    Server.Transfer("ContactDetails.aspx?ID=" + paramList[16].Value.ToString());
                }
                else
                {
                    Server.Transfer("ContactDetails.aspx?ID=" + Request.QueryString["ID"].ToString());
                }
            }
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
                tbDateCreated.Text = Utils.GetSessionString("CalendarDate").Substring(counterLocation + 1);
            }
            else
            {
                tbDateExpiry.Text = Utils.GetSessionString("CalendarDate").Substring(counterLocation + 1);
            }
        }
        Utils.SetSessionString("CalendarDate", "");
    }

    protected void cmdShowHidePassword_Click(object sender, EventArgs e)
    {
        if (Request.QueryString["ID"].ToString() == "New")
        {
            tbPassword.Visible = true;
            cmdGeneratePassword.Visible = true;
        }

        if (cmdShowHidePassword.Text == "Show")
        {
            tbPassword.Visible = true;
            cmdShowHidePassword.Text = "Hide";
            cmdGeneratePassword.Visible = true;
        }
        else
        {
            tbPassword.Visible = false;
            cmdShowHidePassword.Text = "Show";
            cmdGeneratePassword.Visible = false;
        }
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

        dt.Columns.Add(new DataColumn("EXPIRY_EDIT_ID", typeof(string)));
        dt.Columns.Add(new DataColumn("DATE_OF_EDIT", typeof(string)));
        dt.Columns.Add(new DataColumn("OLD_EXPIRY_DATE", typeof(string)));
        dt.Columns.Add(new DataColumn("NEW_EXPIRY_DATE", typeof(string)));
        dt.Columns.Add(new DataColumn("CONTACT_NAME", typeof(string)));
        dt.Columns.Add(new DataColumn("EXTENSION_TYPE", typeof(string)));
        dt.Columns.Add(new DataColumn("EXTENSION_NOTES", typeof(string)));

        bool isAdmDB = true;
        IDataReader idr = Utils.GetReader(isAdmDB, "st_ExtensionRecordGet_1", Request.QueryString["ID"].ToString(), "1");
        while (idr.Read())
        {
            newrow = dt.NewRow();
            newrow["EXPIRY_EDIT_ID"] = idr["EXPIRY_EDIT_ID"].ToString();
            newrow["DATE_OF_EDIT"] = idr["DATE_OF_EDIT"].ToString();
            if (idr["OLD_EXPIRY_DATE"].ToString() == "")
                newrow["OLD_EXPIRY_DATE"] = "n/a";        
            else
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
    protected void cmdNullExpiry_Click(object sender, EventArgs e)
    {
        tbDateExpiry.Text = null;
    }

    protected void cmdGeneratePassword_Click(object sender, EventArgs e)
    {
        // 1. create a random 8 digit number between 10000000 and 99999999
        // 2. create a random position for the letter E
        // 3. create a random position for the letter r

        // create a password
        int eightdigit = RandomNumber(10000000, 99999999);
        int positionE = RandomNumber(0, 3);
        int positionr = RandomNumber(4, 7);
        string password = eightdigit.ToString().Insert(positionE, "E");
        password = password.Remove(positionE + 1, 1);
        password = password.ToString().Insert(positionr, "r");
        password = password.Remove(positionr + 1, 1);
        tbPassword.Text = password;

    }

    private int RandomNumber(int min, int max)
    {
        Random random = new Random();
        return random.Next(min, max);
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
    protected void radGVReviews_NeedDataSource(object sender, Telerik.Web.UI.GridNeedDataSourceEventArgs e)
    {
        if (Request.QueryString["ID"].ToString() != "New")
        {
            DataTable dt = new DataTable();
            System.Data.DataRow newrow;

            dt.Columns.Add(new DataColumn("REVIEW_ID", typeof(Int16)));
            dt.Columns.Add(new DataColumn("REVIEW_NAME", typeof(string)));
            dt.Columns.Add(new DataColumn("DATE_CREATED", typeof(DateTime)));
            dt.Columns.Add(new DataColumn("HOURS", typeof(Int16)));

            bool isAdmDB = true;
            IDataReader idr = Utils.GetReader(isAdmDB, "st_ContactReviewsFilter", Request.QueryString["ID"].ToString(), tbFilter.Text);
            while (idr.Read())
            {
                newrow = dt.NewRow();
                newrow["REVIEW_ID"] = int.Parse(idr["REVIEW_ID"].ToString());
                newrow["REVIEW_NAME"] = idr["REVIEW_NAME"].ToString();
                newrow["DATE_CREATED"] = idr["DATE_CREATED"].ToString();
                if (idr["HOURS"].ToString() == "")
                    newrow["HOURS"] = 0;
                else
                    newrow["HOURS"] = idr["HOURS"].ToString();
                dt.Rows.Add(newrow);
            }
            idr.Close();

            int test = dt.Rows.Count;
            radGVReviews.DataSource = dt;
        }
    }
    protected void radGVReviews_PageIndexChanged(object sender, Telerik.Web.UI.GridPageChangedEventArgs e)
    {
        if (tbFilter.Text == "")
        {
            buildGrid();
        }
        else
        {
            radGVReviews.Rebind(); // fires NeedDataSource
        }
    }
    protected void radGVReviewss_ItemDataBound(object sender, Telerik.Web.UI.GridItemEventArgs e)
    {

    }
    protected void RadAjaxManager1_AjaxRequest(object sender, Telerik.Web.UI.AjaxRequestEventArgs e)
    {
        if (e.Argument.IndexOf("FilterGrid") != -1)
        {
            radGVReviews.Rebind();
        }
    }
}
