using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Collections.Specialized;
using System.Data.SqlClient;
using System.Text.RegularExpressions;

public partial class LinkCheck : System.Web.UI.Page
{
    
    string Validity;
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
            Utils.SetSessionString("LUID", "");
            Utils.SetSessionString("LinkCID", "");
            Utils.SetSessionString("LinkType", "");
            GetLinkDetails();
        }
    }
    protected void GetLinkDetails()
    {
        string linkUI = Request.QueryString["LUID"];
        string Cid = Request.QueryString["CID"];
        Guid pGuid;
        try
        {
            pGuid = new Guid(linkUI);
        }
        catch
        {
            Validity = "Invalid GUI";
            ErrorHandler(Validity);
            return;
        }
        int intCid;
        int.TryParse(Cid, out intCid);
        if (intCid == 0)
        {
            Validity = "Invalid Cid";
            ErrorHandler(Validity);
            return;
        }
        SqlParameter[] paramList = new SqlParameter[3];

        paramList[0] = new SqlParameter("@CID", SqlDbType.Int);
        paramList[0].Direction = ParameterDirection.Input;
        paramList[0].Value = intCid;

        paramList[1] = new SqlParameter("@UID", SqlDbType.UniqueIdentifier);
        paramList[1].Direction = ParameterDirection.Input;
        paramList[1].Value = new Guid(linkUI);

        paramList[2] = new SqlParameter("@RESULT", SqlDbType.NVarChar);
        paramList[2].Direction = ParameterDirection.Output;
        paramList[2].Size = 15;
        paramList[2].Value = "";

        Utils.ExecuteSPWithReturnValues(true, Server, "st_CheckLinkCheck", paramList);
        string res = paramList[2].Value.ToString();
        switch (res)
        {
            case "CheckEmail":
                Utils.SetSessionString("LinkType", "CheckEmail");
                ActivateAccount(intCid);
                break;
            case "ResetPw":
                PnlResetPassword.Visible = true;
                Utils.SetSessionString("LinkType", "ResetPw");
                break;
            case "ActivateGhost":
                PnlActivateGhost.Visible = true;
                PopulateActivateGhost(intCid);
                Utils.SetSessionString("LinkType", "ActivateGhost");
                break;
            case "Not found":
                Utils.SetSessionString("LinkType", "Not found");
                ErrorHandler("Invalid query values: combination of CID and UID was not found, or link is stale");
                break;
            case "ExpiredCkEmail":
                Utils.SetSessionString("LinkType", "CheckEmail");
                ErrorHandler("Expired Check Email Link");
                break;
            case "ExpiredResetPw":
                Utils.SetSessionString("LinkType", "ResetPw");
                ErrorHandler("Expired Reset Password Link");
                break;
            case "ExpiredActGhost":
                Utils.SetSessionString("LinkType", "ActivateGhost");
                ErrorHandler("Expired Activate Ghost Link");
                break;
            default:
                Utils.SetSessionString("LinkType", "Unknown");
                ErrorHandler("Uh? st_CheckLinkCheck returned an unexpected value.");
                break;
        }
        Utils.SetSessionString("LinkCID", Cid);
        Utils.SetSessionString("LUID", linkUI);
        
        
    }
    protected void ActivateAccount(int Cid)
    {//this will set the expiry date of a newly created account after confriming that the email is valid.
        object[] paramList = new object[1];

        paramList[0] = Cid;
        Utils.ExecuteSP(true, Server, "st_ContactActivate", paramList);
        PnlConfirmEmail.Visible = true;
        //send "Welcome" email, this includes username and expiry date, so we need the contact details.
        
        bool isAdmDB = true;
        
        using (System.Data.SqlClient.SqlConnection myConnection = new System.Data.SqlClient.SqlConnection())
        {
            IDataReader idr = Utils.GetReader1(isAdmDB, "st_ContactDetails", myConnection, paramList);
            if (idr.Read())
            {
                string expDstr = "Not Set";
                DateTime expDT = (DateTime)idr["EXPIRY_DATE"];
                if (expDT != null && expDT > new DateTime(2009, 1, 1))
                {
                    expDstr = expDT.ToString("d MMM yyyy");
                }
                string fullname = idr["CONTACT_NAME"].ToString();
                string Username = idr["USERNAME"].ToString();
                string Email = idr["EMAIL"].ToString();
                Utils.WelcomeEmail(Email, fullname, Username, expDstr, "");
                
            }
            else
            {//what? we could not get the account details...
                ErrorHandler("ACCOUNT Activation: account was probably activated, but welcome email was not sent as account details could not be retrieved, this IS ODD!");
            }
        }
    }

    protected void btPasswordReset_Click(object sender, EventArgs e)
    {
        string linkUI = Request.QueryString["LUID"];
        lblPwResetResult.Visible = false;
        //0. check all field where used
        if (tbxUnamePwReset.Text.Length == 0 || tbxNewPw1.Text.Length == 0 || tbxNewPw2.Text.Length == 0)
        {
            lblPwResetResult.Text = "At least one field was not filled. All fields in this form are required";
            lblPwResetResult.Visible = true;
            return;
        }
        //1. check that PWs match
        if (tbxNewPw1.Text != tbxNewPw2.Text)
        {
            lblPwResetResult.Text = "The two password values did not match, please try again.";
            lblPwResetResult.Visible = true;
            return;
        }
        //2. check that PWs are complex enough
        Regex passwordRegex = new Regex("^.*(?=.{8,})(?=.*\\d)(?=.*[a-z])(?=.*[A-Z]).*$");
        Match m = passwordRegex.Match(tbxNewPw1.Text.ToString());
        if (!m.Success)
        {
            lblPwResetResult.Text = "Passwords must be at least 8 characters and contain at least one one lower case letter, one upper case letter and one digit.";
            lblPwResetResult.Visible = true;
            return;
        }
        //3. run st_ResetPassword & find out if it worked
        int intCid;
        int.TryParse(Utils.GetSessionString("LinkCID"), out intCid);
        if (intCid == 0)
        {
            ErrorHandler("Uh? couldn't retrieve the Contact ID...");
            return; //this shouldn't happen so I don't know what to do!
        }
        Guid pGuid;
        try
        {//Utils.SetSessionString("LUID", linkUI);
            pGuid = new Guid(Utils.GetSessionString("LUID"));
        }
        catch
        {
            ErrorHandler("Uh? couldn't retrieve the Link UID...");
            return;
        }

        SqlParameter[] paramList = new SqlParameter[5];

        paramList[0] = new SqlParameter("@CID", SqlDbType.Int);
        paramList[0].Direction = ParameterDirection.Input;
        paramList[0].Value = intCid;

        paramList[1] = new SqlParameter("@UID", SqlDbType.UniqueIdentifier);
        paramList[1].Direction = ParameterDirection.Input;
        paramList[1].Value = pGuid;

        paramList[2] = new SqlParameter("@UNAME", SqlDbType.VarChar);
        paramList[2].Direction = ParameterDirection.Input;
        paramList[2].Size = 50;
        paramList[2].Value = tbxUnamePwReset.Text.Trim();

        paramList[3] = new SqlParameter("@PW", SqlDbType.VarChar);
        paramList[3].Direction = ParameterDirection.Input;
        paramList[3].Size = 50;
        paramList[3].Value = tbxNewPw1.Text;

        paramList[4] = new SqlParameter("@RES", SqlDbType.Int);
        paramList[4].Direction = ParameterDirection.Output;


        Utils.ExecuteSPWithReturnValues(true, Server, "st_ResetPassword", paramList);
        int? res = paramList[4].Value as int?;
        //4. show msg about success or failure
        lblresetPWinstructions.Visible = false;
        tableResetPW.Visible = false;
        lblPwResetResult.Visible = true;
        PnlResetPWfinalMsg.Visible = true;
        if (res != null && res == 1)
        {//this worked!
            lblPwResetResult.Text = "Your Password has been successfully changed.";
        }
        else
        {
            lblPwResetResult.Text = "Your Password was NOT changed! Are you sure you entered the correct username?";
            switch (res)
            {
                case 0://means the failure happened when trying to validate the link/account info
                    LogError("RESET PASSWORD Error: Failed to validate link and/or account info when trying to acutally change the PW");
                    break;
                case -1://means the failure happened when trying to change the password
                    LogError("RESET PASSWORD Error: Failed to acutally change the PW (link and account info looked valid)");
                    break;
                default://uh? means we don't know what went wrong!!
                    LogError("RESET PASSWORD Error: st_ResetPassword failed in a not anticipated way?!?");
                    break;
            }
        }
    }
    protected void ErrorHandler(string errorMsg)
    {
        PnlAnyError.Visible = true;
        PnlConfirmEmail.Visible = false;
        PnlResetPassword.Visible = false;
        PnlActivateGhost.Visible = false;
        //TODO log the error via st_CheckLinkAddFailureLog
        LogError(errorMsg);
    }
    protected void LogError(string errorMsg)
    {
        int intCid;
        int.TryParse(Utils.GetSessionString("LinkCID"), out intCid);

        Guid pGuid = new Guid();
        try
        {//Utils.SetSessionString("LUID", linkUI);
            pGuid = new Guid(Utils.GetSessionString("LUID"));
        }
        catch
        {
            //pGuid = null;
        }
        
        string type = Utils.GetSessionString("LinkType");

        string UserIPAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
        if (UserIPAddress == null || UserIPAddress == "")
        {
            UserIPAddress = Request.ServerVariables["REMOTE_ADDR"];
        }
        string[] IpStrArr = UserIPAddress.Split('.');
        byte[] IpArr = new byte[4];
        if (IpStrArr == null || IpStrArr.Length != 4)
        {//couldn't get the IP???
            IpArr[0] = 0;
            IpArr[1] = 0;
            IpArr[2] = 0;
            IpArr[3] = 0;
        }
        else
        {
            byte.TryParse(IpStrArr[0], out IpArr[0]);
            byte.TryParse(IpStrArr[1], out IpArr[1]);
            byte.TryParse(IpStrArr[2], out IpArr[2]);
            byte.TryParse(IpStrArr[3], out IpArr[3]);
        }
        object[] paramList = new object[9];
        paramList[0] = intCid;
        paramList[1] = pGuid;
        paramList[2] = type;
        paramList[3] = ClientQueryString;
        paramList[4] = IpArr[0];
        paramList[5] = IpArr[1];
        paramList[6] = IpArr[2];
        paramList[7] = IpArr[3];
        paramList[8] = errorMsg;
       
        string ErrEmail = "This is the latest List of Errors generated by the LinkCheck.aspx page, please review carefully:<br><br><br>";
        bool isAdmDB = true;
        using (System.Data.SqlClient.SqlConnection myConnection = new System.Data.SqlClient.SqlConnection())
        {
            IDataReader idr = Utils.GetReader1(isAdmDB, "st_CheckLinkAddFailureLog", myConnection, paramList);
            int count = 1;
            while (idr.Read())
            {
                ErrEmail += "Line " + count.ToString() + " - CHECKLINK_UID:" + idr["CHECKLINK_UID"].ToString() 
                    + "; TYPE:" + idr["TYPE"].ToString() 
                    + "; QUERY_STRING:" + idr["QUERY_STRING"].ToString() 
                    + "; CONTACT_ID:" + idr["CONTACT_ID"].ToString() 
                    + "; ALREADY_SENT:" + idr["ALREADY_SENT"].ToString() 
                    + "; DATE_CREATED:" + idr["DATE_CREATED"].ToString()
                    + "; IP:" + idr["IP_B1"].ToString() + "." + idr["IP_B2"].ToString() + "." + idr["IP_B3"].ToString() + "." + idr["IP_B4"].ToString() + "<br>"
                    + "; FAILURE_REASON: <i>" + idr["FAILURE_REASON"].ToString() + "</i><BR>----------------------------------------------------<br><br>" ;
                count++;
            }
        }
        if (ErrEmail.Length > 110)
        {
            Utils.EmailNewsletter("EPPISupport@ucl.ac.uk", "CheckLink page error LOG", ErrEmail);
        }
        Utils.SetSessionString("LUID", "");
        Utils.SetSessionString("LinkCID", "");
        Utils.SetSessionString("LinkType", "");
    }

    protected void PopulateActivateGhost(int Cid)
    {
        lblContactID.Text = Cid.ToString();
        object[] paramList = new object[1];

        paramList[0] = Cid;
        bool isAdmDB = true;

        using (System.Data.SqlClient.SqlConnection myConnection = new System.Data.SqlClient.SqlConnection())
        {
            IDataReader idr = Utils.GetReader1(isAdmDB, "st_ContactDetails", myConnection, paramList);
            if (idr.Read())
            {
                if (idr["MONTHS_CREDIT"] == null || idr["MONTHS_CREDIT"].ToString() == "0")
                {
                    ErrorHandler("Trying to activate a ghost account, but this isn't a ghost account!");
                    return;
                }
                int Credit;
                int.TryParse(idr["MONTHS_CREDIT"].ToString(), out Credit);
                if (Credit < 1)
                {
                    ErrorHandler("Trying to activate a ghost account, but this isn't a ghost account!");
                    return;
                }
                string expDstr = "Not Set";
                DateTime expDT;
                DateTime.TryParse(idr["EXPIRY_DATE"].ToString(), out expDT);
                if (idr["EXPIRY_DATE"] == null)
                {
                    expDT = DateTime.Now.AddMonths(Credit);
                }
                else
                {
                    if (expDT > DateTime.Now)
                    {
                        expDT = expDT.AddMonths(Credit);
                    }
                    else
                    {
                        expDT = DateTime.Now.AddMonths(Credit);
                    }
                }
                if (expDT != null && expDT > new DateTime(2009, 1, 1))
                {
                    expDstr = expDT.ToString("d MMM yyyy");
                    lblCredit.Text = Credit.ToString();
                    lblExpiryForecast.Text = expDstr;
                }
                string Email = idr["EMAIL"].ToString();
                tbNewEmail.Text = Email;
                tbNewEmailConfirm.Text = Email;
            }
            else
            {//what? we could not get the account details...
                ErrorHandler("Ghost Activation: could not get the account details to work with!");
            }
        }
    }
    protected void cmdActivateGhost_Click(object sender, EventArgs e)
    {
        // create a new account
        //System.Threading.Thread.Sleep(5000);
        string strCid = Utils.GetSessionString("LinkCID");
        int CID;
        int.TryParse(strCid, out CID);
        if (CID == 0)
        {//we don't know whose account we should change!
            ErrorHandler("Account activation ERROR: we don't know now what account to update");
            return;
        }
        Guid pGuid;
        try
        {//Utils.SetSessionString("LUID", linkUI);
            pGuid = new Guid(Utils.GetSessionString("LUID"));
        }
        catch
        {
            ErrorHandler("Account activation ERROR: couldn't retrieve the Link UID...");
            return;
        }
        lblUsername.Visible = false;
        lblUsername.Text = "Username is already in use. Please select another.";
        lblMissingFields.Text = "Please fill in all of the fields. Note: apostrophes (') are not allowed in Username and Emails.";
        lblNewPassword.Text = "Passwords must be at least 8 characters and contain and at least one one lower case letter, one upper case letter and one digit and no spaces.";
        lblEmailAddress.Visible = false;
        lblNewPassword.Visible = false;
        lblMissingFields.Visible = false;
        bool isAdmDB = false;
        bool emailConditionsMet = false;
        bool usernameConditionsMet = false;
        bool passwordConditionsMet = false;
        bool accountConditionsMet = false;

        

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
                return;
            }
            else
            {
                usernameConditionsMet = true;
            }
            idr.Close();

            idr = Utils.GetReader1(isAdmDB, "st_ContactTableByEmail", myConnection, tbNewEmail.Text.Trim());
            while (idr.Read()) // we expect one line: the one for the current account (email field is set when sending the activation link)
            {
                if (idr["CONTACT_ID"].ToString() != strCid)// email is in use by someone else!
                {
                    lblEmailAddress.Visible = true;
                    return;
                }
            }
            idr.Close();
        }
        //all checks went well, so we can now update the account
        
       
        string contactName = tbFirstName.Text + " " + tbLastName.Text;
        isAdmDB = true;
        SqlParameter[] paramList = new SqlParameter[8];
        paramList[0] = new SqlParameter("@CONTACT_NAME", SqlDbType.NVarChar, 255, ParameterDirection.Input,
            true, 0, 0, null, DataRowVersion.Default, contactName);
        paramList[1] = new SqlParameter("@USERNAME", SqlDbType.NVarChar, 50, ParameterDirection.Input,
            true, 0, 0, null, DataRowVersion.Default, tbNewUserName.Text.Trim());
        paramList[2] = new SqlParameter("@PASSWORD", SqlDbType.NVarChar, 50, ParameterDirection.Input,
            true, 0, 0, null, DataRowVersion.Default, tbNewPassword.Text);
        paramList[3] = new SqlParameter("@EMAIL", SqlDbType.NVarChar, 500, ParameterDirection.Input,
            true, 0, 0, null, DataRowVersion.Default, tbNewEmail.Text.Trim());
        paramList[4] = new SqlParameter("@DESCRIPTION", SqlDbType.NVarChar, 1000, ParameterDirection.Input,
            true, 0, 0, null, DataRowVersion.Default, tbDescription.Text + "  Profession: " + ddlProfession.SelectedValue +
            "  Area of Reasearch: " + ddlAreaOfResearch.SelectedValue + "  Hear about us: " + ddlHearAboutUs.SelectedValue);
        paramList[5] = new SqlParameter("@CONTACT_ID", SqlDbType.Int);
        paramList[5].Direction = ParameterDirection.Input;
        paramList[5].Value = CID;

        paramList[6] = new SqlParameter("@UID", SqlDbType.UniqueIdentifier);
        paramList[6].Direction = ParameterDirection.Input;
        paramList[6].Value = pGuid;

        paramList[7] = new SqlParameter("@RES", SqlDbType.NVarChar);
        paramList[7].Direction = ParameterDirection.Output;
        paramList[7].Size = 50;
        paramList[7].Value = null;

        Utils.ExecuteSPWithReturnValues(isAdmDB, Server, "st_GhostContactActivate", paramList);
        string res = paramList[7].Value.ToString();
        if (res == "Done")
        {//all is well
            using (System.Data.SqlClient.SqlConnection myConnection = new System.Data.SqlClient.SqlConnection())
            {
                IDataReader idr = Utils.GetReader1(isAdmDB, "st_ContactDetails", myConnection, strCid);
                if (idr.Read())
                {
                    string expDstr = "Not Set";
                    DateTime expDT = (DateTime)idr["EXPIRY_DATE"];
                    if (expDT != null && expDT > new DateTime(2009, 1, 1))
                    {
                        expDstr = expDT.ToString("d MMM yyyy");
                    }
                    string fullname = idr["CONTACT_NAME"].ToString();
                    string Username = idr["USERNAME"].ToString();
                    string Email = idr["EMAIL"].ToString();
                    Utils.WelcomeEmail(Email, fullname, Username, expDstr, "");
                    string adminMsg = "ACCOUNT CREATED DETAILS (ghost activation):<BR> CONTACT_ID = " + strCid
                                + "<BR> USERNAME = " + tbNewUserName.Text.Trim()
                                + "<BR> EMAIL = " + tbNewEmail.Text.Trim()
                                + "<BR> DESCRIPTION = " + tbDescription.Text + "  Profession: " + ddlProfession.SelectedValue
                                + "  Area of Reasearch: " + ddlAreaOfResearch.SelectedValue
                                + "  Hear about us: " + ddlHearAboutUs.SelectedValue;
                    Utils.WelcomeEmail("EPPISupport@ucl.ac.uk", fullname, tbNewUserName.Text.Trim(), expDstr, adminMsg);
                }
                else
                {//what? we could not get the account details...
                    ErrorHandler("GHOST ACCOUNT Activation: account was probably activated, but welcome email was not sent as account details could not be retrieved, this IS ODD!");
                    return;
                }
            }
        }
        else 
        {
            ErrorHandler("GHOST ACCOUNT Activation: " + res);
            return;
        }

        //3. make the example review if requested
        Utils.SetSessionString("Contact_ID", strCid);
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
        //4.  show success message 
        PnlConfirmEmail.Visible = true;
        PnlActivateGhost.Visible = false;
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
    protected void cbShowResetPassword_CheckedChanged(object sender, EventArgs e)
    {
        string t1, t2;
        if (cbShowResetPassword.Checked)
        {
            tbxNewPw1.TextMode = TextBoxMode.SingleLine;
            tbxNewPw2.TextMode = TextBoxMode.SingleLine;
        }
        else
        {
            t1 = tbxNewPw1.Text;
            t2 = tbxNewPw2.Text;
            tbxNewPw1.TextMode = TextBoxMode.Password;
            tbxNewPw1.Text = t1;
            tbxNewPw1.Attributes.Add("value", t1);
            tbxNewPw2.TextMode = TextBoxMode.Password;
            tbxNewPw2.Text = t2;
            tbxNewPw2.Attributes.Add("value", t2);
        }
    }
}