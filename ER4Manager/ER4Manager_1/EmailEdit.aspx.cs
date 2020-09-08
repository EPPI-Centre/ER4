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
using System.Text;
using System.Threading;
using System.Xml;


public partial class EmailEdit : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (Utils.GetSessionString("Contact_ID") != null)
        {
            if (Utils.GetSessionString("IsAdm") == "True")
            {
                if (!IsPostBack)
                {
                    lblEmailID.Text = Request.QueryString.Get("EmailID").ToString();
                    bool isAdmDB = true;
                    IDataReader idr = Utils.GetReader(isAdmDB, "st_EmailGet", lblEmailID.Text);
                    if (idr.Read())
                    {
                        RadEditor.Content = idr["EMAIL_MESSAGE"].ToString();
                        tbEmailName.Text = idr["EMAIL_NAME"].ToString();
                    }
                    idr.Close();

                    if ((lblEmailID.Text == "9") || (lblEmailID.Text == "10"))
                        lbSendTestEmail.Enabled = false;

                    cmdSave.Attributes.Add("onclick", "if (confirm('Are you sure you are updating the correct email? Do you wish to continue?') == false) return false;");
                }
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

    protected void cmdSave_Click(object sender, EventArgs e)
    {
        int length = RadEditor.Content.Length;
        
        bool isAdmDB = true;
        Utils.ExecuteSP(isAdmDB, Server, "st_EmailUpdate",
            lblEmailID.Text, tbEmailName.Text, RadEditor.Content);

        /*
        lblEmailTooLong.Visible = false;
        if (length <= 4000)
        {
            bool isAdmDB = true;
            Utils.ExecuteSP(isAdmDB, Server, "st_EmailUpdate",
                lblEmailID.Text, tbEmailName.Text, RadEditor.Content);
        }
        else
        {
            lblEmailTooLong.Visible = true;
            lblEmailTooLong.Text = "Message is too long: " + length + "characters. (> 4000)";
        }
        //<asp:Label ID="lblEmailTooLong" runat="server" Visible="false" Font-Bold="True" Text="" ForeColor="#FF3300"></asp:Label>
        */
    }
    protected void lbClose_Click(object sender, EventArgs e)
    {
        string scriptString = "";
        Type cstype = this.GetType();
        ClientScriptManager cs = Page.ClientScript;
        scriptString = "<script language=JavaScript>";
        scriptString += "window.close();</script>";

        if (!cs.IsStartupScriptRegistered("Startup"))
            cs.RegisterStartupScript(cstype, "Startup", scriptString);
    }
    protected void lbSendTestEmail_Click(object sender, EventArgs e)
    {
        string sendResult = "";
        //string emailTo = "EPPISupport@ucl.ac.uk";
        string emailTo = "eppisupport@ucl.ac.uk";
        switch (lblEmailID.Text)
        {
            case ("1"): // WelcomeEmail(string mailTo, string newUser, string userName, string exDate, string stAdditional)
                sendResult = Utils.WelcomeEmail(emailTo,
                    "FakeName", "FakeUsername", "01/01/2020", "");
                return;
            case ("2"): // ForgottenPmail(string mailTo, string stCont, string LinkUI, string CID, string BaseUrl, string stAdditional)
                sendResult = Utils.ForgottenPmail(emailTo, 
                    "FakeName", "FAKE_LINK_UI", "0", "FakeInvalidURL", "");
                return;
            case ("3"): // InviteEmail(string mailTo, string inviteeName, string reviewName, string inviterName,
                            //string inviterEmail, string emailAccountMsg)
                sendResult = Utils.InviteEmail(emailTo,
                    "FakeInviteeName", "FakeReviewName", "FakeInviterName", "FakeInviterEmail", "");
                return;
            case ("4"): // GhostUserActivationEmail(string mailTo, string Fullname, string PurcharserName, string LinkUI, 
                            //string CID, string BaseUrl, string stAdditional)
                sendResult = Utils.GhostUserActivationEmail(emailTo,
                    "FakeName", "FakePurchaserName", "FAKE_LINK_UI", "0", "FakeInvalidURL", "");
                return;
            case ("5"): // ReviewCreationErrorEmail(string mailTo, string contactID, string errorMessage)
                sendResult = Utils.ReviewCreationErrorEmail(emailTo,
                    "0", "Fake message about why the review creation failed");
                return;
            case ("6"): // ForgottenUsernameMail(string mailTo, string stCont, string UserName, string stAdditional)
                sendResult = Utils.ForgottenUsernameMail(emailTo,
                    "FakeName", "FakeUserName", "");
                return;
            case ("7"): // VerifyAccountEmail(string mailTo, string newUser, string userName, string LinkUI, string CID, 
                            //string BaseUrl, string stAdditional)
                sendResult = Utils.VerifyAccountEmail(emailTo,
                    "FakeName", "FakeUserName", "FAKE_LINK_UI", "0", "FakeInvalidURL", "");
                return;
            case ("8"): // GhostCreditTransferEmail(string mailTo, string Fullname, string PurcharserName, string Months, 
                            //string NewDate, string stAdditional)
                sendResult = Utils.GhostCreditTransferEmail(emailTo,
                    "FakeName", "FakePurchaserName", "3", "25/10/2020", "");
                return;
            // 9 and 10 are not emails
            case ("11"): // OutstandingFeeEmail(string mailTo, string contactName, string outstandingFee)
                sendResult = Utils.OutstandingFeeEmail(emailTo,
                    "FakeName", "120");
                return;
            default:
                return;
        }
    }
}