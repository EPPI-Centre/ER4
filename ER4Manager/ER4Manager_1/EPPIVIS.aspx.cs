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
using Telerik.Web.UI;
using System.Drawing;
using System.Globalization;

public partial class EPPIVIS : System.Web.UI.Page
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
                        lbl.Text = "EPPI-Vis";
                    }

                    Telerik.Web.UI.RadTabStrip radTs = (Telerik.Web.UI.RadTabStrip)Master.FindControl("rtsMenu");
                    if (radTs != null)
                    {
                        radTs.SelectedIndex = 5;
                        radTs.Tabs[5].Tabs[2].Selected = true;
                        radTs.Tabs[5].Tabs[6].Width = 550;

                        radTs.Tabs[5].Tabs[0].Visible = true;
                        radTs.Tabs[5].Tabs[1].Visible = true;
                        radTs.Tabs[5].Tabs[6].Visible = true;

                        if (Utils.GetSessionString("IsAdm") == "True")
                        {
                            radTs.Tabs[5].Tabs[2].Visible = true;
                            radTs.Tabs[5].Tabs[3].Visible = true;
                            radTs.Tabs[5].Tabs[4].Visible = true;
                            radTs.Tabs[5].Tabs[5].Visible = true;
                            radTs.Tabs[5].Tabs[6].Width = 200;
                        }

                    }

                    buildRadEPPIVisGrid();
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

    private void buildRadEPPIVisGrid()
    {
        DataTable dt = new DataTable();
        System.Data.DataRow newrow;

        dt.Columns.Add(new DataColumn("WEBDB_ID", typeof(int)));
        dt.Columns.Add(new DataColumn("WEBDB_NAME", typeof(string)));
        dt.Columns.Add(new DataColumn("REVIEW_ID", typeof(int)));
        dt.Columns.Add(new DataColumn("REVIEW_NAME", typeof(string)));
        dt.Columns.Add(new DataColumn("CONTACT_NAME", typeof(string)));
        dt.Columns.Add(new DataColumn("IS_OPEN", typeof(string)));

        bool isAdmDB = true;
        IDataReader idr = Utils.GetReader(isAdmDB, "st_Eppi_Vis_Get_All", tbFilter.Text);
        while (idr.Read())
        {
            newrow = dt.NewRow();
            newrow["WEBDB_ID"] = idr["tv_webdb_id"].ToString();
            newrow["WEBDB_NAME"] = idr["tv_webdb_name"].ToString();
            newrow["REVIEW_ID"] = idr["tv_review_id"].ToString();
            newrow["REVIEW_NAME"] = idr["tv_review_name"].ToString();
            newrow["CONTACT_NAME"] = idr["tv_contact_name"].ToString();
            newrow["IS_OPEN"] = idr["tv_is_open"].ToString();
            dt.Rows.Add(newrow);
        }
        idr.Close();
    }


    protected void radGVEPPIVis_NeedDataSource(object sender, Telerik.Web.UI.GridNeedDataSourceEventArgs e)
    {
        DataTable dt = new DataTable();
        System.Data.DataRow newrow;

        dt.Columns.Add(new DataColumn("WEBDB_ID", typeof(int)));
        dt.Columns.Add(new DataColumn("WEBDB_NAME", typeof(string)));
        dt.Columns.Add(new DataColumn("REVIEW_ID", typeof(int)));
        dt.Columns.Add(new DataColumn("REVIEW_NAME", typeof(string)));
        dt.Columns.Add(new DataColumn("CONTACT_NAME", typeof(string)));
        dt.Columns.Add(new DataColumn("IS_OPEN", typeof(string)));

        bool isAdmDB = true;
        IDataReader idr = Utils.GetReader(isAdmDB, "st_Eppi_Vis_Get_All", tbFilter.Text);
        while (idr.Read())
        {
            newrow = dt.NewRow();
            newrow["WEBDB_ID"] = idr["tv_webdb_id"].ToString();
            newrow["WEBDB_NAME"] = idr["tv_webdb_name"].ToString();
            newrow["REVIEW_ID"] = idr["tv_review_id"].ToString();
            newrow["REVIEW_NAME"] = idr["tv_review_name"].ToString();
            newrow["CONTACT_NAME"] = idr["tv_contact_name"].ToString();
            newrow["IS_OPEN"] = idr["tv_is_open"].ToString();
            dt.Rows.Add(newrow);
        }
        idr.Close();

        int test = dt.Rows.Count;
        radGVEPPIVis.DataSource = dt;
        //radGVEPPIVis.MasterTableView.GetColumn("CONTACT_ID").Display = false;
    }
    protected void radGVEPPIVis_PageIndexChanged(object sender, Telerik.Web.UI.GridPageChangedEventArgs e)
    {
        if (tbFilter.Text == "")
        {
            buildRadEPPIVisGrid();
        }
        else
        {
            radGVEPPIVis.Rebind();
        }
    }
    protected void RadAjaxManager1_AjaxRequest(object sender, Telerik.Web.UI.AjaxRequestEventArgs e)
    {
        if (e.Argument.IndexOf("FilterGrid") != -1)
        {
            radGVEPPIVis.Rebind();
        }
    }
    protected void radGVEPPIVis_ItemDataBound(object sender, Telerik.Web.UI.GridItemEventArgs e)
    {

    }

    protected void radGVEPPIVis_ItemCommand(object sender, Telerik.Web.UI.GridCommandEventArgs e)
    {

    }


}