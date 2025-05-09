﻿using System;
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
using System.Globalization;

public partial class Reviews : System.Web.UI.Page
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
                        lbl.Text = "List of reviews";
                    }
                    if (Utils.GetSessionString("ER_Role") == "Admin")
                    {
                        lbNewReview.Enabled = true;
                    }

                    Telerik.Web.UI.RadTabStrip radTs = (Telerik.Web.UI.RadTabStrip)Master.FindControl("rtsMenu");
                    if (radTs != null)
                    {
                        radTs.SelectedIndex = 2;
                        radTs.Tabs[2].Tabs[0].Selected = true;
                        radTs.Tabs[2].Tabs[4].Width = 430;
                        radTs.Tabs[2].Tabs[3].Visible = false;
                    }
                    System.Web.UI.WebControls.Label lbl1 = (Label)Master.FindControl("lblHeadingText");
                    if (lbl1 != null)
                    {
                        lbl1.Text = "List of reviews";
                    }

                    bool shareable = true;
                    buildGrid(shareable);
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
    private void buildGrid(bool shareable)
    {
        DataTable dt = new DataTable();
        System.Data.DataRow newrow;

        dt.Columns.Add(new DataColumn("REVIEW_ID", typeof(int)));
        dt.Columns.Add(new DataColumn("REVIEW_NAME", typeof(string)));
        dt.Columns.Add(new DataColumn("DATE_CREATED", typeof(DateTime)));
        dt.Columns.Add(new DataColumn("EXPIRY_DATE", typeof(DateTime)));
        dt.Columns.Add(new DataColumn("SITE_LIC_ID", typeof(string)));
        dt.Columns.Add(new DataColumn("FUNDER_ID", typeof(string)));
        dt.Columns.Add(new DataColumn("CONTACT_NAME", typeof(string)));
        dt.Columns.Add(new DataColumn("MONTHS_CREDIT", typeof(string)));

        bool isAdmDB = true;
        IDataReader idr = Utils.GetReader(isAdmDB, "st_ReviewDetailsGetAll", shareable);
        while (idr.Read())
        {
            newrow = dt.NewRow();
            newrow["REVIEW_ID"] = int.Parse(idr["REVIEW_ID"].ToString());
            if (idr["REVIEW_NAME"].ToString() == "")
                newrow["REVIEW_NAME"] = "Unactivated review";
            else
                newrow["REVIEW_NAME"] = idr["REVIEW_NAME"].ToString();

            if (idr["DATE_CREATED"].ToString() == "")
                newrow["DATE_CREATED"] = DateTime.Parse("01/01/1901");
            else
                newrow["DATE_CREATED"] = DateTime.Parse(idr["DATE_CREATED"].ToString());
            if (idr["EXPIRY_DATE"].ToString() == "")
                newrow["EXPIRY_DATE"] = DateTime.Parse("01/01/1901");
            else
                newrow["EXPIRY_DATE"] = DateTime.Parse(idr["EXPIRY_DATE"].ToString());
            newrow["SITE_LIC_ID"] = idr["SITE_LIC_ID"];
            newrow["MONTHS_CREDIT"] = idr["MONTHS_CREDIT"].ToString();
            newrow["FUNDER_ID"] = idr["FUNDER_ID"].ToString();
            newrow["CONTACT_NAME"] = idr["CONTACT_NAME"].ToString();
            dt.Rows.Add(newrow);
        }
        idr.Close();
    }
    protected void lbNewReview_Click(object sender, EventArgs e)
    {
        Server.Transfer("ReviewDetails.aspx?ID=New");
    }
    protected void radGVShareableReviews_NeedDataSource(object sender, GridNeedDataSourceEventArgs e)
    {
        bool shareable = true;
        DataTable dt = new DataTable();
        System.Data.DataRow newrow;
        dt.Columns.Add(new DataColumn("REVIEW_ID", typeof(int)));
        dt.Columns.Add(new DataColumn("REVIEW_NAME", typeof(string)));
        dt.Columns.Add(new DataColumn("DATE_CREATED", typeof(DateTime)));
        dt.Columns.Add(new DataColumn("EXPIRY_DATE", typeof(DateTime)));
        dt.Columns.Add(new DataColumn("SITE_LIC_ID", typeof(string)));
        dt.Columns.Add(new DataColumn("FUNDER_ID", typeof(string)));
        dt.Columns.Add(new DataColumn("CONTACT_NAME", typeof(string)));
        dt.Columns.Add(new DataColumn("MONTHS_CREDIT", typeof(string)));

        bool isAdmDB = true;
        IDataReader idr = Utils.GetReader(isAdmDB, "st_ReviewDetailsFilter", shareable, tbFilter.Text);
        while (idr.Read())
        {
            newrow = dt.NewRow();
            newrow["REVIEW_ID"] = int.Parse(idr["REVIEW_ID"].ToString());
            if (idr["REVIEW_NAME"].ToString() == "")
                newrow["REVIEW_NAME"] = "Unactivated review";
            else
                newrow["REVIEW_NAME"] = idr["REVIEW_NAME"].ToString();

            if (idr["DATE_CREATED"].ToString() == "")
                newrow["DATE_CREATED"] = DateTime.Parse("01/01/1901");
            else
                newrow["DATE_CREATED"] = DateTime.Parse(idr["DATE_CREATED"].ToString());
            if (idr["EXPIRY_DATE"].ToString() == "")
                newrow["EXPIRY_DATE"] = DateTime.Parse("01/01/1901");
            else
                newrow["EXPIRY_DATE"] = DateTime.Parse(idr["EXPIRY_DATE"].ToString());
            newrow["SITE_LIC_ID"] = idr["SITE_LIC_ID"];
            newrow["MONTHS_CREDIT"] = idr["MONTHS_CREDIT"].ToString();
            newrow["FUNDER_ID"] = idr["FUNDER_ID"].ToString();
            newrow["CONTACT_NAME"] = idr["CONTACT_NAME"].ToString();
            dt.Rows.Add(newrow);
        }
        idr.Close();

        int test = dt.Rows.Count;
        radGVShareableReviews.DataSource = dt;
    }
    protected void RadAjaxManager1_AjaxRequest(object sender, AjaxRequestEventArgs e)
    {
        if (e.Argument.IndexOf("FilterGrid") != -1)
        {
            radGVShareableReviews.Rebind();
        }
    }
    protected void radGVShareableReviews_PageIndexChanged(object sender, GridPageChangedEventArgs e)
    {
        bool shareable = true;
        if (tbFilter.Text == "")
        {
            buildGrid(shareable);
        }
        else
        {
            radGVShareableReviews.Rebind(); // fires NeedDataSource
        }
    }
    protected void radGVShareableReviews_ItemDataBound(object sender, Telerik.Web.UI.GridItemEventArgs e)
    {

    }

}


