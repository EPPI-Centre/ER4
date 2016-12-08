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

public partial class Login : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            // set all session variables
            Utils.SetSessionString("Contact_ID", ""); 
            Utils.SetSessionString("Contact_ID_focus", "");
            Utils.SetSessionString("Review_ID_focus", "");
            Utils.SetSessionString("IsAdm", "0");
            Utils.SetSessionString("ContactName", "");
            Utils.SetSessionString("variableID", "");
            Utils.SetSessionString("AccountCreationEnabled", "");
            Utils.SetSessionString("PurchasesEnabled", "");
            Utils.SetSessionString("SendPasswordEnabled", "");
            Utils.SetSessionString("AdmEnableAll", "");
            Utils.SetSessionString("CountryID", "");
            Utils.SetSessionString("IsSiteLicenseAdm", "0");
            Utils.SetSessionString("EnableExampleReviewCopy", "");
            Utils.SetSessionString("EnableDataPresenter", "");
            Utils.SetSessionString("WebDatabaseID", "");
            Utils.SetSessionString("DescriptionAdminEdit", "0");
            Utils.SetSessionString("siteLicenseID", "0");
            Utils.SetSessionString("siteLicenseReviewID", "0");

            try
            {
                lblMOTD.Text = "";
                string fileName = Request.PhysicalApplicationPath + "//motd.txt";
                StreamReader reader = new StreamReader(fileName);

                // File exists, so we try to read it
                try
                {
                    do
                    {
                        lblMOTD.Text = lblMOTD.Text + reader.ReadLine();
                    }
                    while (reader.Peek() != -1);
                }
                catch
                {
                    lblMOTD.Text = "";
                }
                finally
                {
                    reader.Close();
                }
            }
            catch
            {
                lblMOTD.Text = "";
            }

            string test1 = "hello";
            bool isAdmDB = true;
            IDataReader idr = Utils.GetReader(isAdmDB, "st_ManagementSettings");
            if (idr.Read()) // it exists
            {
                Utils.SetSessionString("AccountCreationEnabled", idr["ENABLE_ACCOUNT_CREATION"].ToString());
                Utils.SetSessionString("PurchasesEnabled", idr["ENABLE_PURCHASES"].ToString());
                Utils.SetSessionString("SendPasswordEnabled", idr["ENABLE_SEND_PASSWORD"].ToString());
                Utils.SetSessionString("AdmEnableAll", idr["ADM_ENABLE_ALL"].ToString());
                test1 = idr["ENABLE_EXAMPLE_REVIEW_CREATION"].ToString();
                if (idr["ENABLE_EXAMPLE_REVIEW_CREATION"].ToString() == "True")
                {
                    cbIncludeExampleReview.Checked = true;
                    cbIncludeExampleReview.Enabled = true;
                }
                Utils.SetSessionString("EnableExampleReviewCopy", idr["ENABLE_EXAMPLE_REVIEW_COPY"].ToString());
                //JB
                Utils.SetSessionString("EnableDataPresenter", idr["ENABLE_DATA_PRESENTER"].ToString());
            }
            idr.Close();
            string test = Utils.GetSessionString("AccountCreationEnabled");
            if (Utils.GetSessionString("AccountCreationEnabled") == "True")
            {
                cmdNewAccountScreen0.Enabled = true;
            }
            if (Utils.GetSessionString("SendPasswordEnabled") == "True")
            {
                //cmdRetrieve.Enabled = true;
                //pnlForgottenPassword.Visible = true;
            }
            //lblEmailSent.Visible = true;
        }
    }
    protected void cmdLogin_Click(object sender, EventArgs e)
    {
        bool isAdmDB = true;
        string ContactID = Utils.Login(tbUserID.Text, tbPasswd.Text, Request.UserHostAddress, isAdmDB);

        if (ContactID == null)
        {
            lblOutcome.Text = "Login failed.  Your username and/or password were not recognised. The password is case sensitive.";
            lblOutcome.Visible = true;
            //pnlSendPassword.Visible = true;

        }
        else
        {
            //// clear TB_ACCOUNT_EXTENSION
            //isAdmDB = true;
            //Utils.ExecuteSP(isAdmDB, Server, "st_ContactAccountExtensionClear",
            //    Utils.GetSessionString("Contact_ID"));

            //Utils.ExecuteSP(isAdmDB, Server, "st_ContactTmpClear",
            //    Utils.GetSessionString("Contact_ID"), "CREATOR");

            //Utils.ExecuteSP(isAdmDB, Server, "st_ContactReviewExtensionClear",
            //    Utils.GetSessionString("Contact_ID"));

            //Utils.ExecuteSP(isAdmDB, Server, "st_ReviewTmpClear",
            //    Utils.GetSessionString("Contact_ID"), "CREATOR");

            ////Server.Transfer("Summary.aspx");

            
            if ((Utils.GetSessionString("AdmEnableAll") == "True") && 
                (Utils.GetSessionString("IsAdm") == "True") &&
                (Utils.GetSessionString("AccountCreationEnabled") == "False"))
            {
                cmdNewAccountScreen0.Enabled = true;
                cmdRetrieve.Enabled = true;
                ShowJustThisPanel(pnlChoose);
                //pnlChoose.Visible = true;
                lblNewAccountTest.Visible = true;
                lbGoToSummary.Visible = true;
                //pnlLogin.Visible = false;
                //pnlSendPassword.Visible = true;
                //pnlForgottenPassword.Visible = true;
                lbNewAccountScreen.Visible = true;
            }
            else
            {
                Server.Transfer("Summary.aspx");
            }
            
            
            
        }
    }
    protected void cmdLoginScreen_Click(object sender, EventArgs e)
    {
        ShowJustThisPanel(pnlLogin);
        //pnlChoose.Visible = false;
        //pnlLogin.Visible = true;
    }
    protected void cmdNewAccountScreen_Click(object sender, EventArgs e)
    {
        ShowJustThisPanel(pnlNewAccount);
        //pnlChoose.Visible = false;
        //pnlNewAccount.Visible = true;
    }
    protected void lbReturnToLogin_Click(object sender, EventArgs e)
    {
        ShowJustThisPanel(pnlLogin);
        //pnlChoose.Visible = false;
        //pnlLogin.Visible = true;
        //pnlNewAccount.Visible = false;
        //pnlSendPassword.Visible = false;
    }
    protected void lbNewAccountScreen_Click(object sender, EventArgs e)
    {
        ShowJustThisPanel(pnlNewAccount);
        //pnlChoose.Visible = false;
        //pnlNewAccount.Visible = true;
        //pnlLogin.Visible = false;
    }
    protected void cmdCreate_Click(object sender, EventArgs e)
    {
        // create a new account
        lblUsername.Visible = false;
        lblUsername.Text = "Username is already in use. Please select another.";
        lblMissingFields.Text = "Please fill in all of the fields. Note: apostrophes (') are not allowed in the Username and Emails.";
        lblNewPassword.Text = "Passwords must be at least 8 characters and contain and at least one one lower case letter, one upper case letter and one digit and no spaces.";
        lblEmailAddress.Visible = false;
        lblNewPassword.Visible = false;
        lblMissingFields.Visible = false;
        bool isAdmDB = false;
        bool emailConditionsMet = false;
        bool usernameConditionsMet = false;
        bool passwordConditionsMet = false;
        bool accountConditionsMet = false;
        //System.Threading.Thread.Sleep(5000);

        //// 1. create a random 8 digit number between 10000000 and 99999999
        //// 2. create a random position for the letter E
        //// 3. create a random position for the letter r

        //// create a password
        //int eightdigit = RandomNumber(10000000, 99999999);
        //int positionE = RandomNumber(0, 3);
        //int positionr = RandomNumber(4, 7);
        //string password = eightdigit.ToString().Insert(positionE, "E");
        //password = password.Remove(positionE + 1, 1);
        //password = password.ToString().Insert(positionr, "r");
        //password = password.Remove(positionr + 1, 1);

        //tbDescription.Text = "T3stPassword";
        
        //make sure all of the fields are filled in
        if (tbFirstName.Text == "" || tbLastName.Text == "" || tbNewUserName.Text == "" || tbNewUserName.Text.Contains("'") ||
            tbDescription.Text == "" || tbNewEmail.Text == "" || tbNewEmail.Text.Contains("'") ||
            tbNewEmailConfirm.Text == "" || tbNewEmailConfirm.Text.Contains("'") || 
            ddlAreaOfResearch.SelectedIndex == 0 || ddlHearAboutUs.SelectedIndex == 0 || ddlProfession.SelectedIndex == 0)
        {
            lblMissingFields.Text = "Please fill in all of the fields and drop down lists. Apostrophes (') are not allowed in Username and Emails.";
            lblMissingFields.Visible = true;
            return;
        }
        if (tbNewEmail.Text != tbNewEmailConfirm.Text) // do not trim in the comparison
        {
            lblMissingFields.Visible = true;
            lblMissingFields.Text = "The email and Re-enter email do not match";
            return;
        }
        if (tbNewPassword.Text != tbNewPassword1.Text)
        {
            lblMissingFields.Visible = true;
            lblMissingFields.Text = "The Password and Re-enter Password did not match";
            return;
        }
        Regex passwordRegex = new Regex("^.*(?=.{8,})(?=.*\\d)(?=.*[a-z])(?=.*[A-Z]).*$");
        Match m = passwordRegex.Match(tbNewPassword.Text.ToString());
        if (!m.Success)
        {
            lblMissingFields.Text = "Passwords must be at least 8 characters and contain at least one one lower case letter, one upper case letter and one digit.";
            lblMissingFields.Visible = true;
            return;
        }
        if (tbNewUserName.Text.Trim().Length < 4)
        {
            usernameConditionsMet = false;
            lblUsername.Visible = true;
            lblUsername.Text = "Username must be atleast 4 characters long";
            return;
        }

        // check if email has a @ in it with characters on both sides          
        if ((tbNewEmail.Text.Trim().IndexOf("@") > 0) &&
            (tbNewEmail.Text.Trim().IndexOf("@") < tbNewEmail.Text.Trim().Length - 1))
        {
            emailConditionsMet = true;
        }
        else
        {
            lblNewPassword.Visible = true;
            lblNewPassword.Text = "This email address appears to be invalid";
            return;
        }

        // data is there. check if email and username are unique.
        isAdmDB = true;
        using (System.Data.SqlClient.SqlConnection myConnection = new System.Data.SqlClient.SqlConnection())
        {
            IDataReader idr = Utils.GetReader1(isAdmDB, "st_ContactTableByName", myConnection, tbNewUserName.Text.Trim());
            if (idr.Read()) // it exists
            {
                lblUsername.Visible = true;
                lblUsername.Text = "Username is already in use. Please select another.";
                return;
            }
            else
            {
                usernameConditionsMet = true;
            }
            idr.Close();

            idr = Utils.GetReader1(isAdmDB, "st_ContactTableByEmail", myConnection, tbNewEmail.Text.Trim());
            if (idr.Read()) // it exists
            {
                lblEmailAddress.Visible = true;
                return;
            }
            idr.Close();
        }
        //all checks went well, so we can now create the account
        //1. Create account

        
        DateTime registrationDate = DateTime.Now;
        DateTime expiryDate = DateTime.Now.AddMonths(1);   

        string contactName = tbFirstName.Text + " " + tbLastName.Text;
        isAdmDB = true;
        SqlParameter[] paramList = new SqlParameter[7];
        paramList[0] = new SqlParameter("@CONTACT_NAME", SqlDbType.NVarChar, 255, ParameterDirection.Input,
            true, 0, 0, null, DataRowVersion.Default, contactName);
        paramList[1] = new SqlParameter("@USERNAME", SqlDbType.NVarChar, 50, ParameterDirection.Input,
            true, 0, 0, null, DataRowVersion.Default, tbNewUserName.Text.Trim());
        paramList[2] = new SqlParameter("@PASSWORD", SqlDbType.NVarChar, 50, ParameterDirection.Input,
            true, 0, 0, null, DataRowVersion.Default, tbNewPassword.Text);
        paramList[3] = new SqlParameter("@DATE_CREATED", SqlDbType.DateTime, 8, ParameterDirection.Input,
            true, 0, 0, null, DataRowVersion.Default, registrationDate);
        paramList[4] = new SqlParameter("@EMAIL", SqlDbType.NVarChar, 500, ParameterDirection.Input,
            true, 0, 0, null, DataRowVersion.Default, tbNewEmail.Text.Trim());
        paramList[5] = new SqlParameter("@DESCRIPTION", SqlDbType.NVarChar, 1000, ParameterDirection.Input,
            true, 0, 0, null, DataRowVersion.Default, tbDescription.Text + "  Profession: " + ddlProfession.SelectedValue + 
            "  Area of Reasearch: " + ddlAreaOfResearch.SelectedValue + "  Hear about us: " + ddlHearAboutUs.SelectedValue);
        paramList[6] = new SqlParameter("@CONTACT_ID", SqlDbType.BigInt, 8, ParameterDirection.Output,
            true, 0, 0, null, DataRowVersion.Default, "");
        Utils.ExecuteSPWithReturnValues(isAdmDB, Server, "st_ContactCreate", paramList);
        int CID = int.Parse(paramList[6].Value.ToString());
        
        //2. use C_ID to create the linkcheck record, send welcome email with linkcheck link
        string LinkUI = Utils.CreateLink(CID, "CheckEmail", null, Server);
        string BaseUrl = Request.Url.AbsoluteUri.Substring(0, Request.Url.AbsoluteUri.ToLower().IndexOf("login.aspx"));
        BaseUrl = Utils.FixURLForHTTPS(BaseUrl);
        if (LinkUI != null && LinkUI != "")
        {
            Utils.VerifyAccountEmail(tbNewEmail.Text.Trim(), contactName, tbNewUserName.Text.Trim(), LinkUI, CID.ToString(), BaseUrl, "");
            string adminMsg = "ACCOUNT CREATED DETAILS:<BR> CONTACT_ID = " + CID.ToString()
                                + "<BR> USERNAME = " + tbNewUserName.Text.Trim()
                                + "<BR> EMAIL = " + tbNewEmail.Text.Trim()
                                + "<BR> DESCRIPTION = " + tbDescription.Text + "  Profession: " + ddlProfession.SelectedValue 
                                + "  Area of Reasearch: " + ddlAreaOfResearch.SelectedValue 
                                + "  Hear about us: " + ddlHearAboutUs.SelectedValue;
            Utils.VerifyAccountEmail("EPPISupport@ucl.ac.uk", contactName, tbNewUserName.Text.Trim(), LinkUI, CID.ToString(), BaseUrl, adminMsg);
        }
        else
        {//the attempt wasn't succesful, send email to eppisupport to let us know
            Utils.VerifyAccountEmail("EPPISupport@ucl.ac.uk", contactName, tbNewUserName.Text.Trim(), "", CID.ToString(), BaseUrl, "NOT SENT! <BR> Account was created but could not create link <BR>");
            lblMissingFields.Text = "There was an error: we could not send your confirmation email. Please try again, if the problem persists, please contact EPPISupport@ucl.ac.uk";
            lblMissingFields.Visible = true;
            return;
        }
        //3. create example review if requested.
        string buildER = "Success";
        if (cbIncludeExampleReview.Checked)
        {
            buildER = Utils.buildExampleReview(CID, Server);
            if (buildER != "Success")
            {
                lblErrorCreatingExampleReview.Text = buildER;
                lblErrorCreatingExampleReview.Visible = true;
            }
        }
        //4. show success message W/O the "new account" panel so that people will stop creating multiple accounts by mistake!!
        lblCreatedEmail.Text = tbNewEmail.Text.Trim();
        lblCreatedFullName.Text = contactName;
        lblCreatedUsername.Text = tbNewUserName.Text.Trim();
        ShowJustThisPanel(pnlAccountCreated);
        
    }
    protected void cmdSendPassword_Click(object sender, EventArgs e)
    {
        //bool isAdmDB = false;
        //string adminTxt = "Somebody is trying to use the forgotten password function, with the above credentials:<br>";
        //adminTxt = adminTxt + "UserID: " + tbUserName.Text + "<br>";
        //adminTxt = adminTxt + "Email: " + tbEmail0.Text + "<br>";
        //string failMess = "<b>Sorry, could not send a reminder email.</B><br> " +
        //"Your user ID and email may match our records but we were unable to send an email at this time.";
        ////I need the DB session info to make the Utils.GetReader to work

        //IDataReader reader = Utils.GetReader(isAdmDB, "st_CheckEmail", tbUserName.Text.Replace("'", "''"), tbEmail0.Text.Replace("'", "''"));
        //if ((reader != null) && reader.Read())
        //{
        //    string stEmail = reader["EMAIL"].ToString();
        //    string stPass = reader["PASSWORD"].ToString();
        //    string stContact = reader["CONTACT_NAME"].ToString();
        //    if (!reader.Read()) //I want one and one result only
        //    {
        //        string sendResult = Utils.ForgottenPmail(stEmail, stContact, stPass, "");
        //        if (sendResult == "The email was sent successfully")
        //        {
        //            lblNoPasswordSent.Text = sendResult;
        //            lblNoPasswordSent.Visible = true;
        //        }
        //        else
        //        {
        //            lblNoPasswordSent.Text = failMess;
        //            lblNoPasswordSent.Visible = true;
        //        }
        //        adminTxt = adminTxt + "the Email was sent with the following result:<br>" + sendResult + ".<BR><BR><BR>The sent message is:<BR>";
        //        Utils.ForgottenPmail("EPPISupport@ioe.ac.uk", stContact, stPass, adminTxt);
        //        tbEmail0.Text = "Email";
        //        tbUserName.Text = "User name";
        //        return;
        //    }
        //    //lblOutcome.Text = failMess;
        //    adminTxt = adminTxt + "the Email was not sent because st_CHECK_EMAIL returned more than one record.<BR><BR><BR>The message that would have been sent is:<BR>";
        //    Utils.ForgottenPmail("EPPISupport@ioe.ac.uk", stContact, stPass, adminTxt);
        //    tbEmail0.Text = "Email";
        //    tbUserName.Text = "User name";
        //}
        //else
        //{
        //    //lblOutcome.Text = failMess;
        //    adminTxt = adminTxt + "the Email was not sent because st_CHECK_EMAIL returned no records.<BR><BR><BR>The message that would have been sent is:<BR>";
        //    Utils.ForgottenPmail("EPPISupport@ioe.ac.uk", tbUserName.Text, "NONE", adminTxt);
        //    lblNoPasswordSent.Text = "The Username and email did not match our records";
        //    lblNoPasswordSent.Visible = true;
        //    tbEmail0.Text = "Email";
        //    tbUserName.Text = "User name";
        //}

        /*
        bool isAdmDB = false;
        string adminTxt = "Somebody is trying to use the forgotten password function, with the above credentials:<br>";
        adminTxt = adminTxt + "UserID: " + tbUserName.Text + "<br>";
        adminTxt = adminTxt + "Email: " + tbEmail0.Text + "<br>";
        string failMess = "<b>Sorry, could not send a reminder email.</B><br> " +
        "Your user ID and email may match our records but we were unable to send an email at this time.";
        //I need the DB session info to make the Utils.GetReader to work

        IDataReader reader = Utils.GetReader(isAdmDB, "st_CheckEmail", tbUserName.Text, tbEmail0.Text);
        if ((reader != null) && reader.Read())
        {
            string stEmail = reader["EMAIL"].ToString();
            string stPass = reader["PASSWORD"].ToString();
            string stContact = reader["CONTACT_NAME"].ToString();
            if (!reader.Read()) //I want one and one result only
            {
                string sendResult = Utils.ForgottenPmail(stEmail, stContact, stPass, "");
                if (sendResult == "The email was sent successfully")
                {
                    lblNoPasswordSent.Text = sendResult;
                    lblNoPasswordSent.Visible = true;
                }
                else
                {
                    lblNoPasswordSent.Text = failMess;
                    lblNoPasswordSent.Visible = true;
                }
                adminTxt = adminTxt + "the Email was sent with the following result:<br>" + sendResult + ".<BR><BR><BR>The sent message is:<BR>";
                Utils.ForgottenPmail("EPPISupport@ioe.ac.uk", stContact, stPass, adminTxt);
                tbEmail0.Text = "Email";
                tbUserName.Text = "User name";
                return;
            }
            lblOutcome.Text = failMess;
            adminTxt = adminTxt + "the Email was not sent because st_CHECK_EMAIL returned more than one record.<BR><BR><BR>The message that would have been sent is:<BR>";
            Utils.ForgottenPmail("EPPISupport@ioe.ac.uk", stContact, stPass, adminTxt);
            tbEmail0.Text = "Email";
            tbUserName.Text = "User name";
        }
        else
        {
            lblOutcome.Text = failMess;
            adminTxt = adminTxt + "the Email was not sent because st_CHECK_EMAIL returned no records.<BR><BR><BR>The message that would have been sent is:<BR>";
            Utils.ForgottenPmail("EPPISupport@ioe.ac.uk", tbUserName.Text, "NONE", adminTxt);
            lblNoPasswordSent.Text = "The Username and password did not match our records";
            lblNoPasswordSent.Visible = true;
            tbEmail0.Text = "Email";
            tbUserName.Text = "User name";
        }
        //tbUserName.Text = tbUserID.Text;
        //tbEmail0.Text = "";
        */
    }
    protected void cmdRetrieve_Click(object sender, EventArgs e)
    {
        //bool isAdmDB = false;
        //string adminTxt = "Somebody is trying to use the forgotten password function, with the above credentials:<br>";
        //adminTxt = adminTxt + "UserID: " + tbUserID0.Text + "<br>";
        //adminTxt = adminTxt + "Email: " + tbEmailCheck.Text + "<br>";
        //string failMess = "<b>Sorry, could not send a reminder email.</B><br> " +
        //"Your user ID and email may match our records but we were unable to send an email at this time.";
        ////I need the DB session info to make the Utils.GetReader to work

        //IDataReader reader = Utils.GetReader(isAdmDB, "st_CheckEmail", tbUserID0.Text.Replace("'", "''"), tbEmailCheck.Text.Replace("'", "''"));
        //if ((reader != null) && reader.Read())
        //{
        //    string stEmail = reader["EMAIL"].ToString();
        //    string stPass = reader["PASSWORD"].ToString();
        //    string stContact = reader["CONTACT_NAME"].ToString();
        //    if (!reader.Read()) //I want one and one result only
        //    {
        //        string sendResult = Utils.ForgottenPmail(stEmail, stContact, stPass, "");
        //        if (sendResult == "The email was sent successfully")
        //        {
        //            lblErrorMessage.Text = sendResult;
        //            lblErrorMessage.Visible = true;
        //        }
        //        else
        //        {
        //            lblErrorMessage.Text = failMess;
        //            lblErrorMessage.Visible = true;
        //        }
        //        adminTxt = adminTxt + "the Email was sent with the following result:<br>" + sendResult + ".<BR><BR><BR>The sent message is:<BR>";
        //        Utils.ForgottenPmail("EPPISupport@ioe.ac.uk", stContact, stPass, adminTxt);
        //        tbEmail0.Text = "Email";
        //        tbUserName.Text = "User name";
        //        return;
        //    }
        //    //lblOutcome.Text = failMess;
        //    adminTxt = adminTxt + "the Email was not sent because st_CHECK_EMAIL returned more than one record.<BR><BR><BR>The message that would have been sent is:<BR>";
        //    Utils.ForgottenPmail("EPPISupport@ioe.ac.uk", stContact, stPass, adminTxt);
        //    tbEmailCheck.Text = "Email";
        //    tbUserID0.Text = "User name";
        //}
        //else
        //{
        //    //lblOutcome.Text = failMess;
        //    adminTxt = adminTxt + "the Email was not sent because st_CHECK_EMAIL returned no records.<BR><BR><BR>The message that would have been sent is:<BR>";
        //    Utils.ForgottenPmail("EPPISupport@ioe.ac.uk", tbUserID0.Text, "NONE", adminTxt);
        //    lblErrorMessage.Text = "The Username and email did not match our records";
        //    lblErrorMessage.Visible = true;
        //    tbEmailCheck.Text = "Email";
        //    tbUserID0.Text = "User name";
        //}
    }
    protected void cmdGoToLoginScreen_Click(object sender, EventArgs e)
    {
        lblOutcome.Text = "";
        lblOutcome.Visible = false;
        ShowJustThisPanel(pnlLogin);
        //pnlChoose.Visible = false;
        //pnlLogin.Visible = true;

        if (Utils.GetSessionString("AccountCreationEnabled") == "False")
        {
            lbNewAccountScreen.Visible = false;
        }
    }
    protected void lbGoToSummary_Click(object sender, EventArgs e)
    {
        Server.Transfer("Summary.aspx");
    }
    

    
    protected void ShowJustThisPanel(Panel Showing)
    {
        pnlChoose.Visible = false;
        pnlNewAccount.Visible = false;
        pnlLogin.Visible = false;
        pnlAccountCreated.Visible = false;
        pnlLinkCreator.Visible = false;
        if (pnlLogin == Showing)
        {
            pnlLogin.Visible = true;
        }
        else if (pnlChoose == Showing)
        {
            pnlChoose.Visible = true;
        }
        else if (pnlNewAccount == Showing)
        {
            pnlNewAccount.Visible = true;
        }
        else if (pnlLinkCreator == Showing)
        {
            pnlLinkCreator.Visible = true;
        }
        else if (pnlAccountCreated == Showing)
        {
            pnlAccountCreated.Visible = true;
        }
    }
    protected void lnkbtForgottenpw_Click(object sender, EventArgs e)
    {
        //1. get the user to fill the email or uname fields,
        ShowJustThisPanel(pnlLinkCreator);
        tc00.Visible = true;
        tc01.Visible = true;
        tc10.Visible = true;
        tc11.Visible = true;
        tcOR.Visible = true;
        tcOR1.Visible = true;
        tbxEmailLinkCreate.Enabled = true;
        tbxUnameLinkCreate.Enabled = true;
        btForgottenPwLinkCreate.Visible = true;
        btForgottenUnameEmailCreate.Visible = false;
        btForgottenToActivateLinkCreate.Visible = false;
        lblInstructionsLinkCreate.Text = "Please fill in either the Username, Email, or both fields to generate a Reset-Password email.";
        lblInstructionsLinkCreate.Visible = true;
        lblResultLinkCreate.Visible = false;
        
    }
    protected void lnkbtForgottenUname_Click(object sender, EventArgs e)
    {
        //1. get the user to fill the email field,
        ShowJustThisPanel(pnlLinkCreator);
        tc00.Visible = false;
        tc01.Visible = true;
        tc10.Visible = false;
        tc11.Visible = true;
        tcOR.Visible = false;
        tcOR1.Visible = false;
        tbxEmailLinkCreate.Enabled = true;
        tbxUnameLinkCreate.Enabled = false;
        btForgottenPwLinkCreate.Visible = false;
        btForgottenUnameEmailCreate.Visible = true;
        btForgottenToActivateLinkCreate.Visible = false;
        lblInstructionsLinkCreate.Text = "Please fill in the Email field. If correct, your username will be sent to you via Email.";
        lblInstructionsLinkCreate.Visible = true;
        lblResultLinkCreate.Visible = false;
        //2. check with some SP if the email is OK, if so
        //3. send email
        //4. and show a suitable msg telling the user to check their email, do NOT say if this worked!!
    }
    protected void lnkbtForgottenToActivate_Click(object sender, EventArgs e)
    {
        //1. get the user to fill the email or uname fields,
        ShowJustThisPanel(pnlLinkCreator);
        tc00.Visible = true;
        tc01.Visible = true;
        tc10.Visible = true;
        tc11.Visible = true;
        tcOR.Visible = true;
        tcOR1.Visible = true;
        tbxEmailLinkCreate.Enabled = true;
        tbxUnameLinkCreate.Enabled = true;
        btForgottenPwLinkCreate.Visible = false;
        btForgottenUnameEmailCreate.Visible = false;
        btForgottenToActivateLinkCreate.Visible = true;
        lblInstructionsLinkCreate.Text = "Please fill in either the Username, Email, or both fields to generate a new 'Validate Account' Email message. This is necessary to ensure that your email address is valid. This function will send the message only if the details provided are correct and your account wasn't validated already.";
        lblInstructionsLinkCreate.Visible = true;
        lblResultLinkCreate.Visible = false;
        //2. check with some SP if they are OK, if they are
        //3. create the link via st_CheckLinkCreate, send email
        //4. and show a suitable msg telling the user to check their email, do NOT say if this worked!!
    }
    
    protected void btForgottenPwLinkCreate_Click(object sender, EventArgs e)
    {
        //2. check if account details are OK, if they are
        string email = tbxEmailLinkCreate.Text.Trim();
        SqlParameter[] paramListCheckUserDetails = new SqlParameter[5];
        if (tbxUnameLinkCreate.Text.Trim() != "")
        {
            paramListCheckUserDetails[0] = new SqlParameter("@Uname", tbxUnameLinkCreate.Text.Trim());
        }
        else
        {
            paramListCheckUserDetails[0] = new SqlParameter("@Uname", null);
        }
        paramListCheckUserDetails[0].Direction = ParameterDirection.InputOutput;
        paramListCheckUserDetails[0].Size = 50;
        if (email != "" && email.Contains('@') && email.IndexOf('@') > 1 && email.IndexOf('@') < email.Length - 1)
        {
            paramListCheckUserDetails[1] = new SqlParameter("@Email", email);
        }
        else
        {
            paramListCheckUserDetails[1] = new SqlParameter("@Email", "");
        }
        paramListCheckUserDetails[1].Direction = ParameterDirection.InputOutput;
        paramListCheckUserDetails[1].Size = 500;

        paramListCheckUserDetails[2] = new SqlParameter("@CID", SqlDbType.Int);
        paramListCheckUserDetails[2].Direction = ParameterDirection.Output;

        paramListCheckUserDetails[3] = new SqlParameter("@CONTACT_NAME", SqlDbType.NVarChar);
        paramListCheckUserDetails[3].Size = 255;
        paramListCheckUserDetails[3].Direction = ParameterDirection.Output;

        paramListCheckUserDetails[4] = new SqlParameter("@IS_ACTIVE", SqlDbType.Bit);
        paramListCheckUserDetails[4].Direction = ParameterDirection.Output;

        Utils.ExecuteSPWithReturnValues(true, Server, "st_ContactCheckUnameOrEmail", paramListCheckUserDetails);
        int CID = (int)paramListCheckUserDetails[2].Value;
        string ContName = paramListCheckUserDetails[3].Value.ToString();
        //3. create the link via st_CheckLinkCreate, send email 
        string BaseUrl = Request.Url.AbsoluteUri.Substring(0,Request.Url.AbsoluteUri.ToLower().IndexOf("login.aspx"));
        BaseUrl = Utils.FixURLForHTTPS(BaseUrl);
        if (CID > 0)//only if we found the user details!
        {
            if (email == "" || !email.Contains('@'))//if the email field wasn't used
            {
                email = paramListCheckUserDetails[1].Value.ToString();
            }

            string LinkUI = Utils.CreateLink(CID, "ResetPw", "", Server);
            if (LinkUI != null && LinkUI != "")
            {
                Utils.ForgottenPmail(email, ContName, LinkUI, CID.ToString(), BaseUrl, ""); 
            }
            else
            {//the attempt wasn't succesful, send email to eppisupport to let us know
                Utils.ForgottenPmail("EPPISupport@ucl.ac.uk", ContName, "", CID.ToString(), BaseUrl, "NOT SENT! <BR> Account was found but could not create link <BR>");
            }
        }
        else
        {//the attempt wasn't succesful, send email to eppisupport to let us know
            Utils.ForgottenPmail("EPPISupport@ucl.ac.uk", ContName, "", CID.ToString(), BaseUrl, "NOT SENT! <BR> Could not find the requested account, USERNAME = '" + tbxUnameLinkCreate.Text + "', EMAIL = " + tbxEmailLinkCreate.Text + "<BR>");
        }
        //4. show a suitable msg telling the user to check their email, do NOT say if this worked!!
        
        //lblResultLinkCreate.Text = "Please check your email (and your SPAM filter). If your account details were valid you should receive a 'Reset Password' message shortly. This message will contain a unique link that will remain valid for 10 minutes from now.";
        lblResultLinkCreate.Text = "Please check your email. If your account details were valid you should receive a " + 
            "'Reset Password' message shortly. This message will contain a unique link that will remain valid for " + 
            "10 minutes (from now). If you do not receive the email you may need to check your spam filter." ;
        tc00.Visible = false;
        tc01.Visible = false;
        tc10.Visible = false;
        tc11.Visible = false;
        tcOR.Visible = false;
        tcOR1.Visible = false;
        linkReturn.Visible = false;
        lblInstructionsLinkCreate.Visible = false;
        lblResultLinkCreate.Visible = true;
        tbxEmailLinkCreate.Visible = false;
        tbxUnameLinkCreate.Visible = false;
        btForgottenPwLinkCreate.Visible = false;
        btForgottenUnameEmailCreate.Visible = false;
        btForgottenToActivateLinkCreate.Visible = false;
    }
    protected void btForgottenUnameEmailCreate_Click(object sender, EventArgs e)
    {
        //2. check with some SP if the email is OK, if so
        string email = tbxEmailLinkCreate.Text.Trim();
        SqlParameter[] paramListCheckUserDetails = new SqlParameter[4];
        paramListCheckUserDetails[0] = new SqlParameter("@Uname", "");
        paramListCheckUserDetails[0].Direction = ParameterDirection.InputOutput;
        paramListCheckUserDetails[0].Size = 50;
        if (email != "" && email.Contains('@') && email.IndexOf('@') > 1 && email.IndexOf('@') < email.Length - 1)
        {
            paramListCheckUserDetails[1] = new SqlParameter("@Email", tbxEmailLinkCreate.Text);
        }
        else
        {
            lblInstructionsLinkCreate.ForeColor = System.Drawing.Color.FromArgb(255, 21, 0);
            lblInstructionsLinkCreate.Font.Bold = true;
            lblInstructionsLinkCreate.Text = "Email isn't valid. " + lblInstructionsLinkCreate.Text;
            return;
        }
        paramListCheckUserDetails[1].Direction = ParameterDirection.InputOutput;
        paramListCheckUserDetails[1].Size = 500;
        paramListCheckUserDetails[2] = new SqlParameter("@CID", SqlDbType.Int);
        paramListCheckUserDetails[2].Direction = ParameterDirection.Output;
        paramListCheckUserDetails[3] = new SqlParameter("@CONTACT_NAME", SqlDbType.NVarChar);
        paramListCheckUserDetails[3].Size = 255;
        paramListCheckUserDetails[3].Direction = ParameterDirection.Output;
        Utils.ExecuteSPWithReturnValues(true, Server, "st_ContactCheckUnameOrEmail", paramListCheckUserDetails);
        int? CID = paramListCheckUserDetails[2].Value as int?;
        string ContName = paramListCheckUserDetails[3].Value.ToString();
        //3. send email
        if (CID != null && CID > 0)//only if we found the user details!
        {
            Utils.ForgottenUsernameMail(email, ContName, paramListCheckUserDetails[0].Value.ToString(), "");
        }
        else
        {//let us know that someone tried and failed
            Utils.ForgottenUsernameMail("EPPISupport@ucl.ac.uk", "", "", "NOT SENT! <BR> Account was not found in SQL, email used was: " + email + "<BR>");
        }
        //4. and show a suitable msg telling the user to check their email, do NOT say if this worked!!
        lblResultLinkCreate.Text = "Please check your email (and your SPAM filter). If your email address was recognised you should receive a 'Username Reminder' message shortly";
        tc00.Visible = false;
        tc01.Visible = false;
        tc10.Visible = false;
        tc11.Visible = false;
        tcOR.Visible = false;
        tcOR1.Visible = false;
        linkReturn.Visible = true;
        lblInstructionsLinkCreate.Visible = false;
        lblResultLinkCreate.Visible = true;
        tbxEmailLinkCreate.Visible = false;
        tbxUnameLinkCreate.Visible = false;
        btForgottenPwLinkCreate.Visible = false;
        btForgottenUnameEmailCreate.Visible = false;
        btForgottenToActivateLinkCreate.Visible = false;
    }
    protected void btForgottenToActivateLinkCreate_Click(object sender, EventArgs e)
    {
        string email = tbxEmailLinkCreate.Text.Trim();
        lblMissingFields.Visible = false;
        SqlParameter[] paramListCheckUserDetails = new SqlParameter[5];
        if (tbxUnameLinkCreate.Text.Trim() != "")
        {
            paramListCheckUserDetails[0] = new SqlParameter("@Uname", tbxUnameLinkCreate.Text.Trim());
        }
        else
        {
            paramListCheckUserDetails[0] = new SqlParameter("@Uname", null);
        }
        paramListCheckUserDetails[0].Direction = ParameterDirection.InputOutput;
        paramListCheckUserDetails[0].Size = 50;
        if (email != "" && email.Contains('@') && email.IndexOf('@') > 1 && email.IndexOf('@') < email.Length - 1)
        {
            paramListCheckUserDetails[1] = new SqlParameter("@Email", email);
        }
        else
        {
            paramListCheckUserDetails[1] = new SqlParameter("@Email", "");
        }
        if (paramListCheckUserDetails[1].ToString() == "" && paramListCheckUserDetails[0] == null)
        {//both boxes where empty or email was filled with something that isn't an email address
            lblMissingFields.Text = "Please provide a username and/or a valid email address.";
            lblMissingFields.Visible = true;
            return;
        }
        paramListCheckUserDetails[1].Direction = ParameterDirection.InputOutput;
        paramListCheckUserDetails[1].Size = 500;

        paramListCheckUserDetails[2] = new SqlParameter("@CID", SqlDbType.Int);
        paramListCheckUserDetails[2].Direction = ParameterDirection.Output;

        paramListCheckUserDetails[3] = new SqlParameter("@CONTACT_NAME", SqlDbType.NVarChar);
        paramListCheckUserDetails[3].Size = 255;
        paramListCheckUserDetails[3].Direction = ParameterDirection.Output;

        paramListCheckUserDetails[4] = new SqlParameter("@IS_ACTIVE", SqlDbType.Bit);
        paramListCheckUserDetails[4].Direction = ParameterDirection.Output;

        Utils.ExecuteSPWithReturnValues(true, Server, "st_ContactCheckUnameOrEmail", paramListCheckUserDetails);
        int CID = (int)paramListCheckUserDetails[2].Value;
        string contactName = paramListCheckUserDetails[3].Value.ToString();
        if (CID > 0 && paramListCheckUserDetails[4].Value as bool? == false)
        {
            //3. create the link via st_CheckLinkCreate, send email 
            string LinkUI = Utils.CreateLink(CID, "CheckEmail", null, Server);
            string BaseUrl = Request.Url.AbsoluteUri.Substring(0, Request.Url.AbsoluteUri.ToLower().IndexOf("login.aspx"));
            BaseUrl = Utils.FixURLForHTTPS(BaseUrl);
            if (LinkUI != null && LinkUI != "")
            {
                Utils.VerifyAccountEmail(paramListCheckUserDetails[1].Value.ToString().Trim(), contactName, paramListCheckUserDetails[0].Value.ToString().Trim(), 
                    LinkUI, CID.ToString(), BaseUrl, "");
            }
            else
            {//the attempt wasn't succesful link was not created, uh? send email to eppisupport to let us know
                Utils.VerifyAccountEmail("EPPISupport@ucl.ac.uk", contactName, tbxUnameLinkCreate.Text.Trim() == "" ? "UnknownUserName" : tbxUnameLinkCreate.Text.Trim()
                    , "", CID.ToString(), BaseUrl, "NOT SENT! <BR> User tried to re-activate an account, but we could not create the checklink.<BR>");
                lblMissingFields.Text = "There was an error: we could not send your confirmation email. Please try again, if the problem persists, please contact EPPISupport@ucl.ac.uk";
                lblMissingFields.Visible = true;
                return;
            }
        }
        else
        {//the attempt wasn't succesful, send email to eppisupport to let us know
            Utils.VerifyAccountEmail("EPPISupport@ucl.ac.uk", "UnknownName", 
                tbxUnameLinkCreate.Text.Trim() == "" ? "UnknownUserName" : tbxUnameLinkCreate.Text.Trim(),
                "",CID.ToString(), "", "NOT SENT! <BR> User tried to re-activate an account, but could not find the requested (inactive) account, USERNAME = '" + tbxUnameLinkCreate.Text + "', EMAIL = " + tbxEmailLinkCreate.Text 
                            + "<BR>It's possible that the account was already active.");
            lblMissingFields.Text = "There was an error: your account details could not be found or the account doesn't need to be activated. Please try again, if the problem persists, please contact EPPISupport@ucl.ac.uk";
            lblMissingFields.Visible = true;
            return;
        }
        //4. show a suitable msg telling the user to check their email, do NOT say if this worked!!

        lblResultLinkCreate.Text = "Please check your email (and your SPAM filter). If your account details were valid you should receive an 'Account Activation' message shortly.";
        tc00.Visible = false;
        tc01.Visible = false;
        tc10.Visible = false;
        tc11.Visible = false;
        tcOR.Visible = false;
        tcOR1.Visible = false;
        linkReturn.Visible = false;
        lblInstructionsLinkCreate.Visible = false;
        lblResultLinkCreate.Visible = true;
        tbxEmailLinkCreate.Visible = false;
        tbxUnameLinkCreate.Visible = false;
        btForgottenPwLinkCreate.Visible = false;
        btForgottenUnameEmailCreate.Visible = false;
        btForgottenToActivateLinkCreate.Visible = false;
    }

    
    
    protected void cbShowPassword_CheckedChanged(object sender, EventArgs e)
    {
        string t1, t2;
        if (cbShowPassword.Checked)
        {
            tbNewPassword.TextMode = TextBoxMode.SingleLine;
            tbNewPassword1.TextMode = TextBoxMode.SingleLine;
        }
        else
        {
            t1 = tbNewPassword.Text;
            t2 = tbNewPassword1.Text;
            tbNewPassword.TextMode = TextBoxMode.Password;
            tbNewPassword.Text = t1;
            tbNewPassword.Attributes.Add("value", t1);
            tbNewPassword1.TextMode = TextBoxMode.Password;
            tbNewPassword1.Text = t2;
            tbNewPassword1.Attributes.Add("value", t2);
        }
    }
}
