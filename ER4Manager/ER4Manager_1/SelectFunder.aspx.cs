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

public partial class SelectFunder : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (Utils.GetSessionString("Contact_ID") != null)
        {
            if (Utils.GetSessionString("IsAdm") == "True")
            {
                if (!IsPostBack)
                {
                    if (Request.QueryString["funder"].ToString() == "Please select")
                    {
                        bool ER4AccountsOnly = true;
                        if (!cbValidER4Account.Checked)
                            ER4AccountsOnly = false;
                        buildContactGrid(ER4AccountsOnly);
                    }
                    else
                    {
                        Utils.SetSessionString("variableID", Request.QueryString["funder"].ToString());

                        string scriptString = "<script language=JavaScript>";
                        scriptString += "window.opener.document.forms[0].ctl00$ContentPlaceHolder1$cmdPlaceFunder.click();";
                        scriptString += "window.close();<";
                        scriptString += "/";
                        scriptString += "script>";
                        Type cstype = this.GetType();
                        ClientScriptManager cs = Page.ClientScript;

                        if (!cs.IsStartupScriptRegistered("Startup"))
                            cs.RegisterStartupScript(cstype, "Startup", scriptString);
                    }
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
        DataTable dt = new DataTable();
        System.Data.DataRow newrow;

        dt.Columns.Add(new DataColumn("CONTACT_ID", typeof(Int16)));
        dt.Columns.Add(new DataColumn("EMAIL", typeof(string)));
        dt.Columns.Add(new DataColumn("CONTACT_NAME", typeof(string)));

        bool isAdmDB = true;
        IDataReader idr = Utils.GetReader(isAdmDB, "st_ContactDetailsGetAllFilter_1", ER4AccountsOnly, tbFilter.Text);
        while (idr.Read())
        {
            newrow = dt.NewRow();
            newrow["CONTACT_ID"] = int.Parse(idr["CONTACT_ID"].ToString());
            newrow["EMAIL"] = idr["EMAIL"].ToString();
            newrow["CONTACT_NAME"] = idr["CONTACT_NAME"].ToString();
            dt.Rows.Add(newrow);
        }
        idr.Close();
    }


    protected void cbValidER4Account_CheckedChanged(object sender, EventArgs e)
    {
        bool ER4AccountsOnly = true;
        if (!cbValidER4Account.Checked)
            ER4AccountsOnly = false;

        radGVContacts.Rebind();
    }










    protected void radGVContacts_NeedDataSource(object sender, Telerik.Web.UI.GridNeedDataSourceEventArgs e)
    {
        bool ER4AccountsOnly = true;
        if (!cbValidER4Account.Checked)
            ER4AccountsOnly = false;

        DataTable dt = new DataTable();
        System.Data.DataRow newrow;

        dt.Columns.Add(new DataColumn("CONTACT_ID", typeof(Int16)));
        dt.Columns.Add(new DataColumn("EMAIL", typeof(string)));
        dt.Columns.Add(new DataColumn("CONTACT_NAME", typeof(string)));

        bool isAdmDB = true;
        IDataReader idr = Utils.GetReader(isAdmDB, "st_ContactDetailsGetAllFilter_1", ER4AccountsOnly, tbFilter.Text);
        while (idr.Read())
        {
            newrow = dt.NewRow();
            newrow["CONTACT_ID"] = int.Parse(idr["CONTACT_ID"].ToString());
            newrow["EMAIL"] = idr["EMAIL"].ToString();
            if (idr["CONTACT_NAME"].ToString() == "")
                newrow["CONTACT_NAME"] = "Unactivated account";
            else
                newrow["CONTACT_NAME"] = idr["CONTACT_NAME"].ToString();
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
