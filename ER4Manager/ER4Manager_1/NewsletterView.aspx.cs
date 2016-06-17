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

public partial class NewsletterView : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (Utils.GetSessionString("Contact_ID") != null)
        {
            if (Utils.GetSessionString("IsAdm") == "True")
            {
                if (!IsPostBack)
                {
                    lblNewsletterID.Text = Request.QueryString.Get("NewsletterID").ToString();
                    bool isAdmDB = true;
                    IDataReader idr = Utils.GetReader(isAdmDB, "st_NewsletterGet", lblNewsletterID.Text);
                    if (idr.Read())
                    {
                        RadEditor.Content = idr["NEWSLETTER"].ToString();
                    }
                    idr.Close();

                    cmdSave.Attributes.Add("onclick", "if (confirm('Are you sure you are updating the correct newsletter? Do you wish to continue?') == false) return false;");
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
        Utils.ExecuteSP(isAdmDB, Server, "st_NewsletterUpdate",
            lblNewsletterID.Text, RadEditor.Content);
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

    }
}