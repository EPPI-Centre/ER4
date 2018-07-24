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

public partial class Contacts : System.Web.UI.Page
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
                        lbl.Text = "List of contacts";
                    }

                    Telerik.Web.UI.RadTabStrip radTs = (Telerik.Web.UI.RadTabStrip)Master.FindControl("rtsMenu");
                    if (radTs != null)
                    {
                        radTs.SelectedIndex = 3;
                        radTs.Tabs[3].Tabs[0].Selected = true;
                        radTs.Tabs[3].Tabs[2].Width = 670;
                        radTs.Tabs[3].Tabs[1].Visible = false;
                    }
                    System.Web.UI.WebControls.Label lbl1 = (Label)Master.FindControl("lblHeadingText");
                    if (lbl1 != null)
                    {
                        lbl1.Text = "List of contacts";
                    }



                    if (Utils.GetSessionString("ER_Role") == "Admin")
                    {
                        lbNewAccount.Enabled = true;
                    }

                    bool ER4AccountsOnly = true;
                    if (!cbValidER4Account.Checked)
                        ER4AccountsOnly = false;

                    buildContactGrid(ER4AccountsOnly);
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


    private void buildContactGrid(bool ER4AccountsOnly)
    {
        string siteLicense = "0";

        DataTable dt = new DataTable();
        System.Data.DataRow newrow;

        dt.Columns.Add(new DataColumn("CONTACT_ID", typeof(int)));
        dt.Columns.Add(new DataColumn("OLD_CONTACT_ID", typeof(string)));
        dt.Columns.Add(new DataColumn("EMAIL", typeof(string)));
        dt.Columns.Add(new DataColumn("CONTACT_NAME", typeof(string)));
        dt.Columns.Add(new DataColumn("DATE_CREATED", typeof(DateTime)));
        dt.Columns.Add(new DataColumn("EXPIRY_DATE", typeof(DateTime)));
        dt.Columns.Add(new DataColumn("MONTHS_CREDIT", typeof(string)));
        dt.Columns.Add(new DataColumn("SITE_LIC_ID", typeof(string)));
        dt.Columns.Add(new DataColumn("CREATOR_ID", typeof(string)));

        bool isAdmDB = true;
        IDataReader idr = Utils.GetReader(isAdmDB, "st_ContactDetailsGetAllFilter_2", ER4AccountsOnly, tbFilter.Text, siteLicense);
        while (idr.Read())
        {
            newrow = dt.NewRow();
            newrow["CONTACT_ID"] = int.Parse(idr["CONTACT_ID"].ToString());
            newrow["OLD_CONTACT_ID"] = idr["old_contact_id"].ToString();
            newrow["EMAIL"] = idr["EMAIL"].ToString();
            newrow["CONTACT_NAME"] = idr["CONTACT_NAME"].ToString();
            newrow["DATE_CREATED"] = idr["DATE_CREATED"];
            newrow["EXPIRY_DATE"] = idr["EXPIRY_DATE"];
            newrow["SITE_LIC_ID"] = idr["SITE_LIC_ID"].ToString();
            newrow["MONTHS_CREDIT"] = idr["MONTHS_CREDIT"].ToString();
            newrow["CREATOR_ID"] = idr["CREATOR_ID"].ToString();
            dt.Rows.Add(newrow);
        }
        idr.Close();
    }


    protected void lbNewAccount_Click(object sender, EventArgs e)
    {
        Server.Transfer("ContactDetails.aspx?ID=New");
    }
    protected void cbValidER4Account_CheckedChanged(object sender, EventArgs e)
    {
        bool ER4AccountsOnly = true;
        if (!cbValidER4Account.Checked)
            ER4AccountsOnly = false;

        //buildContactGrid(ER4AccountsOnly);
        radGVContacts.Rebind();
    }

    protected void radGVContacts_NeedDataSource(object sender, Telerik.Web.UI.GridNeedDataSourceEventArgs e)
    {
        bool ER4AccountsOnly = true;
        if (!cbValidER4Account.Checked)
            ER4AccountsOnly = false;
        string siteLicense = "0";

        DataTable dt = new DataTable();
        System.Data.DataRow newrow;

        dt.Columns.Add(new DataColumn("CONTACT_ID", typeof(int)));
        dt.Columns.Add(new DataColumn("OLD_CONTACT_ID", typeof(string)));
        dt.Columns.Add(new DataColumn("EMAIL", typeof(string)));
        dt.Columns.Add(new DataColumn("CONTACT_NAME", typeof(string)));
        dt.Columns.Add(new DataColumn("DATE_CREATED", typeof(DateTime)));
        dt.Columns.Add(new DataColumn("EXPIRY_DATE", typeof(DateTime)));
        dt.Columns.Add(new DataColumn("MONTHS_CREDIT", typeof(string)));
        dt.Columns.Add(new DataColumn("SITE_LIC_ID", typeof(string)));
        dt.Columns.Add(new DataColumn("CREATOR_ID", typeof(string)));

        bool isAdmDB = true;
        IDataReader idr = Utils.GetReader(isAdmDB, "st_ContactDetailsGetAllFilter_2", ER4AccountsOnly, tbFilter.Text, siteLicense);
        while (idr.Read())
        {
            newrow = dt.NewRow();
            newrow["CONTACT_ID"] = int.Parse(idr["CONTACT_ID"].ToString());
            newrow["OLD_CONTACT_ID"] = idr["old_contact_id"].ToString();
            newrow["EMAIL"] = idr["EMAIL"].ToString();
            if (idr["CONTACT_NAME"].ToString() == "")
                newrow["CONTACT_NAME"] = "Unactivated account";
            else
                newrow["CONTACT_NAME"] = idr["CONTACT_NAME"].ToString();
            //newrow["CONTACT_NAME"] = idr["CONTACT_NAME"].ToString();
            newrow["DATE_CREATED"] = idr["DATE_CREATED"];
            newrow["EXPIRY_DATE"] = idr["EXPIRY_DATE"];
            newrow["SITE_LIC_ID"] = idr["SITE_LIC_ID"].ToString();
            newrow["MONTHS_CREDIT"] = idr["MONTHS_CREDIT"].ToString();
            newrow["CREATOR_ID"] = idr["CREATOR_ID"].ToString();
            dt.Rows.Add(newrow);
        }
        idr.Close();

        int test = dt.Rows.Count;
        radGVContacts.DataSource = dt;
    }
    protected void radGVContacts_PageIndexChanged(object sender, Telerik.Web.UI.GridPageChangedEventArgs e)
    {
        bool ER4AccountsOnly = true;
        if (tbFilter.Text == "")
        {
            if (!cbValidER4Account.Checked)
                ER4AccountsOnly = false;
            buildContactGrid(ER4AccountsOnly);
        }
        else
        {
            radGVContacts.Rebind(); 
        }
    }
    protected void RadAjaxManager1_AjaxRequest(object sender, Telerik.Web.UI.AjaxRequestEventArgs e)
    {
        if (e.Argument.IndexOf("FilterGrid") != -1)
        {
            radGVContacts.Rebind();
        }
    }
    protected void radGVContacts_ItemDataBound(object sender, Telerik.Web.UI.GridItemEventArgs e)
    {

    }

}