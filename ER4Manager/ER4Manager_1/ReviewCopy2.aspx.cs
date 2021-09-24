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

public partial class ReviewCopy2 : System.Web.UI.Page
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
                        lbl.Text = "[Adm only] This utility is for copying a shareable review. UNDER DEVELOPMENT!";
                        lblContactID.Text = Utils.GetSessionString("Contact_ID");
                    }

                    Telerik.Web.UI.RadTabStrip radTs = (Telerik.Web.UI.RadTabStrip)Master.FindControl("rtsMenu");
                    if (radTs != null)
                    {
                        radTs.SelectedIndex = 5;
                        radTs.Tabs[5].Tabs[5].Selected = true;
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

                        /*
                        radTs.SelectedIndex = 5;
                        radTs.Tabs[5].Tabs[4].Selected = true;
                        radTs.Tabs[5].Tabs[5].Width = 500;
                        if (Utils.GetSessionString("IsAdm") == "True")
                        {
                            radTs.Tabs[5].Tabs[1].Visible = true;
                            radTs.Tabs[5].Tabs[3].Visible = true;
                            radTs.Tabs[5].Tabs[4].Visible = true;
                            radTs.Tabs[5].Tabs[5].Width = 220;
                        }
                        */
                    }
                    System.Web.UI.WebControls.Label lbl1 = (Label)Master.FindControl("lblHeadingText");
                    if (lbl1 != null)
                    {
                        lbl1.Text = "[Adm only] This utility is for copying a shareable review. UNDER DEVELOPMENT!";
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
    protected void cmdTestReviewCopy_Click(object sender, EventArgs e)
    {

    }
    protected void cmdStep01_Click(object sender, EventArgs e)
    {
        // Step 01: create new review, populate TB_REVIEW, TB_REVIEW_CONTACT, TB_CONTACT_REVIEW_ROLE 
        //  and return new REVIEW_ID
        bool isAdmDB = true;
        lblMessage.Text = "0";

        SqlParameter[] paramList01 = new SqlParameter[4];
        paramList01[0] = new SqlParameter("@CONTACT_ID", SqlDbType.Int, 8, ParameterDirection.Input,
            true, 0, 0, null, DataRowVersion.Default, Utils.GetSessionString("Contact_ID"));
        paramList01[1] = new SqlParameter("@SOURCE_REVIEW_ID", SqlDbType.Int, 8, ParameterDirection.Input,
            true, 0, 0, null, DataRowVersion.Default, tbSourceReview.Text);
        paramList01[2] = new SqlParameter("@NEW_REVIEW_ID", SqlDbType.BigInt, 8, ParameterDirection.Output,
            true, 0, 0, null, DataRowVersion.Default, "");
        paramList01[3] = new SqlParameter("@RESULT", SqlDbType.NVarChar, 50, ParameterDirection.Output,
            true, 0, 0, null, DataRowVersion.Default, "");
        Utils.ExecuteSPWithReturnValues(isAdmDB, Server, "st_CopyReviewShareableStep01", paramList01);
        if (paramList01[3].Value.ToString() != "0")
        {
            lblMessage.Text = "Step 01 error - " + paramList01[3].Value.ToString();
        }
        else
        {
            tbDestinationReviewID.Text = paramList01[2].Value.ToString();
        }
    }
    protected void cmdStep03_Click(object sender, EventArgs e)
    {
        tbDestinationReviewID.Text = "2109";  
        if ((tbDestinationReviewID.Text == "") || (tbSourceReview.Text == ""))
        {
            lblMessage.Text = "Enter a Source and/or Destination review ID";
        }
        else
        {
            // Step 03: copy the items by populating TB_ITEM, TB_ITEM_REVIEW, TB_ITEM_AUTHOR, TB_ITEM_DOCUMENT, 
            //  TB_ITEM_REVIEW
            bool isAdmDB = true;
            lblMessage.Text = "0";

            SqlParameter[] paramList03 = new SqlParameter[5];
            paramList03[0] = new SqlParameter("@CONTACT_ID", SqlDbType.Int, 8, ParameterDirection.Input,
                true, 0, 0, null, DataRowVersion.Default, Utils.GetSessionString("Contact_ID"));
            paramList03[1] = new SqlParameter("@SOURCE_REVIEW_ID", SqlDbType.BigInt, 8, ParameterDirection.Input,
                true, 0, 0, null, DataRowVersion.Default, tbSourceReview.Text);
            paramList03[2] = new SqlParameter("@NEW_REVIEW_ID", SqlDbType.BigInt, 8, ParameterDirection.Input,
                true, 0, 0, null, DataRowVersion.Default, tbDestinationReviewID.Text);
            paramList03[3] = new SqlParameter("@ATTRIBUTE_FOR_COPY", SqlDbType.BigInt, 8, ParameterDirection.Input,
                true, 0, 0, null, DataRowVersion.Default, tbItemsToCopy.Text);
            paramList03[4] = new SqlParameter("@RESULT", SqlDbType.NVarChar, 50, ParameterDirection.Output,
                true, 0, 0, null, DataRowVersion.Default, "");
            Utils.ExecuteSPWithReturnValues(isAdmDB, Server, "st_CopyReviewShareableStep03", paramList03);
            if (paramList03[4].Value.ToString() != "0")
            {
                lblMessage.Text = "Step 03 error - " + paramList03[4].Value.ToString();
            }
        }
    }
    protected void cmdStep05_Click(object sender, EventArgs e)
    {

    }
    protected void cmdStep0709_Click(object sender, EventArgs e)
    {

    }
    protected void cmdStep0711_Click(object sender, EventArgs e)
    {

    }
    protected void cmdStep13_Click(object sender, EventArgs e)
    {

    }
    protected void cmdStep15_Click(object sender, EventArgs e)
    {

    }
    protected void cmdCleanup_Click(object sender, EventArgs e)
    {

    }
    protected void cmdGetItemAttrID_Click(object sender, EventArgs e)
    {

        DataTable dt = new DataTable();
        System.Data.DataRow newrow;

        dt.Columns.Add(new DataColumn("ITEM_ID", typeof(string)));
        dt.Columns.Add(new DataColumn("SHORT_TITLE", typeof(string)));
        dt.Columns.Add(new DataColumn("INFO", typeof(string)));
        bool isAdmDB = true;
        IDataReader idr = Utils.GetReader(isAdmDB, "st_GetInfo",
            Utils.GetSessionString("Contact_ID"));
        while (idr.Read())
        {
            newrow = dt.NewRow();
            newrow["ITEM_ID"] = idr["REVIEW_ID"].ToString();
            newrow["SHORT_TITLE"] = idr["REVIEW_NAME"].ToString();
            newrow["INFO"] = idr["REVIEW_NAME"].ToString();
            dt.Rows.Add(newrow);
        }
        idr.Close();



    }
}