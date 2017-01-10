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

                    buildGridNew0();  // newer
                    //pnlOldDisplay.Visible = false;
                    pnlNewDisplay.Visible = true;

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



    private void buildGridNew0()
    {
        DataTable dt = new DataTable();
        System.Data.DataRow newrow;
        dt.Columns.Add(new DataColumn("CONTACT_ID", typeof(Int16)));
        dt.Columns.Add(new DataColumn("REVIEW_ID", typeof(Int16)));
        dt.Columns.Add(new DataColumn("CREATED", typeof(string)));
        dt.Columns.Add(new DataColumn("LAST_RENEWED", typeof(string)));
        dt.Columns.Add(new DataColumn("CONTACT_NAME", typeof(string)));
        dt.Columns.Add(new DataColumn("EMAIL", typeof(string)));
        dt.Columns.Add(new DataColumn("REVIEW_NAME", typeof(string)));
        dt.Columns.Add(new DataColumn("REVIEW_TYPE", typeof(string)));
        dt.Columns.Add(new DataColumn("ACTIVE_HOURS", typeof(string)));

        bool isAdmDB = true;
        IDataReader idr = Utils.GetReader(isAdmDB, "st_RecentActivityGetAllFilter", tbFilter0.Text);
        while (idr.Read())
        {
            newrow = dt.NewRow();
            newrow["CONTACT_ID"] = idr["C_ID"].ToString();
            newrow["REVIEW_ID"] = idr["R_ID"].ToString();
            newrow["CREATED"] = idr["CREATED"].ToString();
            newrow["LAST_RENEWED"] = idr["LAST_RENEWED"].ToString();
            newrow["CONTACT_NAME"] = idr["CONTACT_NAME"].ToString();
            newrow["EMAIL"] = idr["EMAIL"].ToString();
            newrow["REVIEW_NAME"] = idr["REVIEW_NAME"].ToString();
            newrow["REVIEW_TYPE"] = idr["rev type"].ToString();
            newrow["ACTIVE_HOURS"] = idr["active hours"].ToString();
            dt.Rows.Add(newrow);
        }
        idr.Close();
    }

    protected void RadAjaxManager1_AjaxRequest(object sender, Telerik.Web.UI.AjaxRequestEventArgs e)
    {
        if (e.Argument.IndexOf("FilterGrid") != -1)
        {
            radGVContacts0.Rebind();
        }

    }


    protected void radGVContacts0_NeedDataSource(object sender, Telerik.Web.UI.GridNeedDataSourceEventArgs e)
    {
        DataTable dt = new DataTable();
        System.Data.DataRow newrow;
        dt.Columns.Add(new DataColumn("CONTACT_ID", typeof(Int16)));
        dt.Columns.Add(new DataColumn("REVIEW_ID", typeof(Int16)));
        dt.Columns.Add(new DataColumn("CREATED", typeof(string)));
        dt.Columns.Add(new DataColumn("LAST_RENEWED", typeof(string)));
        dt.Columns.Add(new DataColumn("CONTACT_NAME", typeof(string)));
        dt.Columns.Add(new DataColumn("EMAIL", typeof(string)));
        dt.Columns.Add(new DataColumn("REVIEW_NAME", typeof(string)));
        dt.Columns.Add(new DataColumn("REVIEW_TYPE", typeof(string)));
        dt.Columns.Add(new DataColumn("ACTIVE_HOURS", typeof(Int16)));

        bool isAdmDB = true;
        IDataReader idr = Utils.GetReader(isAdmDB, "st_RecentActivityGetAllFilter", tbFilter0.Text);
        while (idr.Read())
        {
            newrow = dt.NewRow();
            newrow["CONTACT_ID"] = idr["C_ID"].ToString();
            newrow["REVIEW_ID"] = idr["R_ID"].ToString();
            newrow["CREATED"] = idr["CREATED"].ToString();
            newrow["LAST_RENEWED"] = idr["LAST_RENEWED"].ToString();
            newrow["CONTACT_NAME"] = idr["CONTACT_NAME"].ToString();
            newrow["EMAIL"] = idr["EMAIL"].ToString();
            newrow["REVIEW_NAME"] = idr["REVIEW_NAME"].ToString();
            newrow["REVIEW_TYPE"] = idr["rev type"].ToString();
            newrow["ACTIVE_HOURS"] = idr["active hours"].ToString();
            dt.Rows.Add(newrow);
        }
        idr.Close();
        int test = dt.Rows.Count;
        radGVContacts0.DataSource = dt;
    }

    protected void radGVContacts0_PageIndexChanged(object sender, Telerik.Web.UI.GridPageChangedEventArgs e)
    {
        if (tbFilter0.Text == "")
        {
            buildGridNew0();
        }
        else
        {
            radGVContacts0.Rebind();
        }
    }
    protected void radGVContacts_ItemDataBound(object sender, Telerik.Web.UI.GridItemEventArgs e)
    {

    }

    protected void radGVContacts0_ItemDataBound(object sender, Telerik.Web.UI.GridItemEventArgs e)
    {

    }


    protected void rblWhichDisplay_SelectedIndexChanged(object sender, EventArgs e)
    {
        buildGridNew0();  // newer
        pnlNewDisplay.Visible = true;

    }

}
