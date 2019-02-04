using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Data;
using System.Xml.Linq;

public partial class ReturnFromPayment : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (Utils.GetSessionString("Contact_ID") != null)
        {
            if (!IsPostBack)
            {
                /*
                ComponentArt.Web.UI.TabStrip ts = (ComponentArt.Web.UI.TabStrip)Master.FindControl("tsMenu");
                if (ts != null)
                {
                    ts.SelectedTab = ts.Tabs[0];
                    ts.SelectedTab = ts.Tabs[0].Tabs[0];
                }
                */

                Telerik.Web.UI.RadTabStrip radTs = (Telerik.Web.UI.RadTabStrip)Master.FindControl("rtsMenu");
                if (radTs != null)
                {
                    radTs.SelectedIndex = 1;
                    radTs.Tabs[1].Selected = true;
                    radTs.Tabs[1].Tabs[2].Width = 600;
                }
                System.Web.UI.WebControls.Label lbl = (Label)Master.FindControl("lblPageTitle");
                if (lbl != null)
                {
                    lbl.Text = "Return to Summary";
                }

                makeScript();
                int tester;
                if (Request.QueryString["ID"] != null && int.TryParse(Request.QueryString["ID"], out tester) )
                {//payment was cancelled: mark bill as cancelled.
                    if (tester.ToString() == Utils.GetSessionString("Draft_Bill_ID"))
                        Utils.ExecuteSP(true, Server, "st_BillMarkAsCancelled", Utils.GetSessionString("Contact_ID"), Utils.GetSessionString("Draft_Bill_ID"));
                    else Server.Transfer("Error.aspx");
                }
            }
            else
            {
                makeScript();
            }
        }
        else
        {
            Server.Transfer("Error.aspx");
        }
    }
    private void makeScript()
    {
        string scriptString = "<script language=JavaScript>";
        //scriptString += "print(sadasdew);" + Environment.NewLine;
        // scriptString += "if (undefined != window.opener | null == window.opener)" + Environment.NewLine;
        // scriptString += "{if (null !== window.opener.document)" + Environment.NewLine;
        // scriptString += "{ if (null !== window.opener.document.getElementById(\"ctl00_NavToSummary\"))" + Environment.NewLine;
        // //scriptString += "var res = alert(\"AAAAA\");" + Environment.NewLine;
        // scriptString += "window.opener.Jump(); }}" + Environment.NewLine;
        //scriptString += "window.opener.document.getElementById(\"ctl00_NavToSummary\").click(); }}" + Environment.NewLine;
        //scriptString += "window.close();";
        scriptString += "</script>";
        Type cstype = this.GetType();
        ClientScriptManager cs = Page.ClientScript;

        //if (!cs.IsStartupScriptRegistered("Startup"))
        //    cs.RegisterStartupScript(cstype, "Startup", scriptString);
    }

}