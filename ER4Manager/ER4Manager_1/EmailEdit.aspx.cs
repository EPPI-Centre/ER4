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
        bool isAdmDB = true;
        Utils.ExecuteSP(isAdmDB, Server, "st_EmailUpdate",
            lblEmailID.Text, tbEmailName.Text, RadEditor.Content); ///*RadEditor.Content*/
        //Utils.ExecuteSP(isAdmDB, Server, "st_EmailUpdate_1", lblEmailID.Text, "abc");
                                                       
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
        switch (lblEmailID.Text)
        {
            case ("1"): // new account email
                sendResult = Utils.VerifyAccountEmail("EPPISupport@ucl.ac.uk",
                    "EPPISupport", "EPPISupport", "FAKE_NEWAccount_UI", "0", "InvalidURL", "THIS is a test from the edit Email page");
                return;
            case ("2"): // forgotten password
                sendResult = Utils.ForgottenPmail("EPPISupport@ucl.ac.uk", "EPPISupport", "FAKE_LINK_UI", "0", "InvalidURL", "THIS is a test from the edit Email page");
                return;
            case ("3"): // review invitation
                sendResult = Utils.InviteEmail("EPPISupport@ucl.ac.uk",
                    "EPPISupport", "test review 1", "Jeff Brunton", "j.brunton@ucl.ac.uk", "");
                return;
            default:
                return;
        }
    }
}