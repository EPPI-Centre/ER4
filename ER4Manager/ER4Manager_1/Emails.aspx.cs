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




public partial class Emails : System.Web.UI.Page
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
                        lbl.Text = "EPPI-Reviewer emails";
                    }

                    Telerik.Web.UI.RadTabStrip radTs = (Telerik.Web.UI.RadTabStrip)Master.FindControl("rtsMenu");
                    if (radTs != null)
                    {
                        radTs.SelectedIndex = 4;
                        radTs.Tabs[4].Tabs[5].Selected = true;
                        //radTs.Tabs[3].Tabs[2].Width = 670;
                    }
                    System.Web.UI.WebControls.Label lbl1 = (Label)Master.FindControl("lblHeadingText");
                    if (lbl1 != null)
                    {
                        lbl1.Text = "EPPI Reviewer newsletter";
                    }


                    buildEmailGrid();

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

    private void buildEmailGrid()
    {
        DataTable dt1 = new DataTable();
        System.Data.DataRow newrow1;

        dt1.Columns.Add(new DataColumn("EMAIL_ID", typeof(string)));
        dt1.Columns.Add(new DataColumn("EMAIL_NAME", typeof(string)));

        bool isAdmDB = true;
        IDataReader idr = Utils.GetReader(isAdmDB, "st_EmailsGet");
        while (idr.Read())
        {
            newrow1 = dt1.NewRow();
            newrow1["EMAIL_ID"] = idr["EMAIL_ID"].ToString();
            newrow1["EMAIL_NAME"] = idr["EMAIL_NAME"].ToString();
            dt1.Rows.Add(newrow1);
        }
        idr.Close();

        gvEmails.DataSource = dt1;
        gvEmails.DataBind();
    }


    protected void gvEmails_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        /*
        bool isAdmDB = true;
        int index = Convert.ToInt32(e.CommandArgument);
        string emailID = (string)gvEmails.DataKeys[index].Value;
        switch (e.CommandName)
        {
            case "VIEW":
                
                IDataReader idr = Utils.GetReader(isAdmDB, "st_EmailGet", emailID);
                if (idr.Read())
                {
                    //tbText.Text = idr["EMAIL_MESSAGE"].ToString();
                    lblEmailName.Text = idr["EMAIL_NAME"].ToString();
                    lblEmailID.Text = emailID;

                }
                idr.Close();
                pnlHtml.Visible = true;
                pnlEmail.Visible = true;
                
                hlEditEmail.NavigateUrl = "JavaScript:openEditEmail('" + emailID + "')";
                break;

            default:
                break;
        }
        */
    }



    protected void gvEmails_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            System.Web.UI.WebControls.HyperLink hl = (System.Web.UI.WebControls.HyperLink)e.Row.FindControl("hlEditEmail");
            hl.Attributes.Add("href", @"javascript:openEditEmail('" + DataBinder.Eval(e.Row.DataItem, "EMAIL_ID").ToString() + "')");
        }
    }
}