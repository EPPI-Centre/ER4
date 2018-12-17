using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class error : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            Telerik.Web.UI.RadTabStrip radTs = (Telerik.Web.UI.RadTabStrip)Master.FindControl("rtsMenu");
            if (radTs != null)
            {
                radTs.SelectedIndex = 8;
                radTs.Tabs[8].Visible = true;
                radTs.Tabs[0].Visible = false;
                radTs.Tabs[1].Visible = false;
                radTs.Tabs[5].Visible = false;
                radTs.Tabs[7].Visible = false;
            }
            System.Web.UI.WebControls.Label lbl1 = (Label)Master.FindControl("lblHeadingText");
            if (lbl1 != null)
            {
                lbl1.Text = "Summary of this user and their reviews";
            }
        }
    }
}
