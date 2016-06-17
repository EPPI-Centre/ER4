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

public partial class RecentActivity : System.Web.UI.Page
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
                        lbl.Text = "Recent activity";
                    }

                    Telerik.Web.UI.RadTabStrip radTs = (Telerik.Web.UI.RadTabStrip)Master.FindControl("rtsMenu");
                    if (radTs != null)
                    {
                        radTs.SelectedIndex = 4;
                        radTs.Tabs[4].Tabs[0].Selected = true;
                        radTs.Tabs[4].Tabs[8].Width = 90;
                        //radTs.Tabs[3].Tabs[2].Width = 670;
                    }
                    System.Web.UI.WebControls.Label lbl1 = (Label)Master.FindControl("lblHeadingText");
                    if (lbl1 != null)
                    {
                        lbl1.Text = "Recent activity";
                    }

                    buildGridNew();
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


    private void buildGridNew()
    {
        DataTable dt = new DataTable();
        System.Data.DataRow newrow;
        
        dt.Columns.Add(new DataColumn("CONTACT_NAME", typeof(string)));
        dt.Columns.Add(new DataColumn("EMAIL", typeof(string)));
        dt.Columns.Add(new DataColumn("CONTACT_ID", typeof(Int16)));
        dt.Columns.Add(new DataColumn("REVIEW_ID", typeof(Int16)));
        dt.Columns.Add(new DataColumn("CREATED", typeof(string)));
        dt.Columns.Add(new DataColumn("LAST_RENEWED", typeof(string)));
        string SQL = "SELECT c.CONTACT_NAME, c.EMAIL, c.[CONTACT_ID],[REVIEW_ID],[CREATED],[LAST_RENEWED]";
        SQL += " FROM [ReviewerAdmin].[dbo].[TB_LOGON_TICKET] lt Inner Join Reviewer.dbo.TB_CONTACT c on lt.CONTACT_ID = c.CONTACT_ID";
        SQL += " Where STATE = 1 AND LAST_RENEWED > DATEADD(HH, -3, GETDATE())";
        bool isAdmDB = false;
        SqlDataReader sdr = Utils.ReturnReader(SQL, isAdmDB);
        while (sdr.Read())
        {
            newrow = dt.NewRow();
            newrow["CONTACT_NAME"] = sdr["CONTACT_NAME"].ToString();
            newrow["EMAIL"] = sdr["EMAIL"].ToString();
            newrow["CONTACT_ID"] = int.Parse(sdr["CONTACT_ID"].ToString());
            newrow["REVIEW_ID"] = int.Parse(sdr["REVIEW_ID"].ToString());
            newrow["CREATED"] = sdr["CREATED"].ToString();
            newrow["LAST_RENEWED"] = sdr["LAST_RENEWED"].ToString();
            dt.Rows.Add(newrow);
        }
        sdr.Close();
    }


    protected void RadAjaxManager1_AjaxRequest(object sender, Telerik.Web.UI.AjaxRequestEventArgs e)
    {
        if (e.Argument.IndexOf("FilterGrid") != -1)
        {
            radGVContacts.Rebind();
        }
    }
    protected void radGVContacts_NeedDataSource(object sender, Telerik.Web.UI.GridNeedDataSourceEventArgs e)
    {
        DataTable dt = new DataTable();
        System.Data.DataRow newrow;

        dt.Columns.Add(new DataColumn("CONTACT_NAME", typeof(string)));
        dt.Columns.Add(new DataColumn("EMAIL", typeof(string)));
        dt.Columns.Add(new DataColumn("CONTACT_ID", typeof(Int16)));
        dt.Columns.Add(new DataColumn("REVIEW_ID", typeof(Int16)));
        dt.Columns.Add(new DataColumn("CREATED", typeof(string)));
        dt.Columns.Add(new DataColumn("LAST_RENEWED", typeof(string)));
        string SQL = "SELECT c.CONTACT_NAME, c.EMAIL, c.[CONTACT_ID],[REVIEW_ID],[CREATED],[LAST_RENEWED]";
        SQL += " FROM [ReviewerAdmin].[dbo].[TB_LOGON_TICKET] lt Inner Join Reviewer.dbo.TB_CONTACT c on lt.CONTACT_ID = c.CONTACT_ID";
        SQL += " Where STATE = 1 AND LAST_RENEWED > DATEADD(HH, -3, GETDATE())";
        SQL += "and ((c.CONTACT_NAME like '%" + tbFilter.Text + "%') OR " +
                "(c.EMAIL like '%" + tbFilter.Text + "%') OR " +
                "([REVIEW_ID] like '%" + tbFilter.Text + "%') OR " +
                "([CREATED] like '%" + tbFilter.Text + "%')" +
                ") ";
        bool isAdmDB = false;
        SqlDataReader sdr = Utils.ReturnReader(SQL, isAdmDB);
        while (sdr.Read())
        {
            newrow = dt.NewRow();
            newrow["CONTACT_NAME"] = sdr["CONTACT_NAME"].ToString();
            newrow["EMAIL"] = sdr["EMAIL"].ToString();
            newrow["CONTACT_ID"] = int.Parse(sdr["CONTACT_ID"].ToString());
            newrow["REVIEW_ID"] = int.Parse(sdr["REVIEW_ID"].ToString());
            newrow["CREATED"] = sdr["CREATED"].ToString();
            newrow["LAST_RENEWED"] = sdr["LAST_RENEWED"].ToString();
            dt.Rows.Add(newrow);
        }
        sdr.Close();

        int test = dt.Rows.Count;
        radGVContacts.DataSource = dt;
    }
    protected void radGVContacts_PageIndexChanged(object sender, Telerik.Web.UI.GridPageChangedEventArgs e)
    {
        if (tbFilter.Text == "")
        {
            buildGridNew();
        }
        else
        {
            radGVContacts.Rebind();
        }
    }
    protected void radGVContacts_ItemDataBound(object sender, Telerik.Web.UI.GridItemEventArgs e)
    {

    }
}
