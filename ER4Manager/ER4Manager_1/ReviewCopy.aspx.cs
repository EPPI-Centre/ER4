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

public partial class ReviewCopy : System.Web.UI.Page
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
                        lbl.Text = "[Adm only] This utility is for copying a non-shareable review";
                        lblContactID.Text = Utils.GetSessionString("Contact_ID");
                    }

                    Telerik.Web.UI.RadTabStrip radTs = (Telerik.Web.UI.RadTabStrip)Master.FindControl("rtsMenu");
                    if (radTs != null)
                    {
                        radTs.SelectedIndex = 5;
                        radTs.Tabs[5].Tabs[4].Selected = true;
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
                        radTs.Tabs[5].Tabs[3].Selected = true;
                        radTs.Tabs[5].Tabs[5].Width = 670;
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
                        lbl1.Text = "[Adm only] This utility is for copying a non-shareable review";
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
        buildExampleReview(tbSourceReview.Text);
    }

    private void buildExampleReview(string sourceReviewID)
    {
        // Step 01: create new review, populate TB_REVIEW, TB_REVIEW_CONTACT, TB_CONTACT_REVIEW_ROLE 
        //  and return new REVIEW_ID
        bool isAdmDB = true;
        lblMessage.Text = "0";

        SqlParameter[] paramList01 = new SqlParameter[3];
        paramList01[0] = new SqlParameter("@CONTACT_ID", SqlDbType.Int, 8, ParameterDirection.Input,
            true, 0, 0, null, DataRowVersion.Default, Utils.GetSessionString("Contact_ID"));
        paramList01[1] = new SqlParameter("@NEW_REVIEW_ID", SqlDbType.BigInt, 8, ParameterDirection.Output,
            true, 0, 0, null, DataRowVersion.Default, "");
        paramList01[2] = new SqlParameter("@RESULT", SqlDbType.NVarChar, 50, ParameterDirection.Output,
            true, 0, 0, null, DataRowVersion.Default, "");
        Utils.ExecuteSPWithReturnValues(isAdmDB, Server, "st_CopyReviewStep01", paramList01);
        if (paramList01[2].Value.ToString() != "0")
        {
            lblMessage.Text = "Step 01 error - " + paramList01[2].Value.ToString();
        }
        else
        {
            lblDestinationReviewID.Text = paramList01[1].Value.ToString();
            // keep going          
            // Step 03: copy the items by populating TB_ITEM, TB_ITEM_REVIEW, TB_ITEM_AUTHOR, TB_ITEM_DOCUMENT, 
            //  TB_ITEM_REVIEW
            lblMessage.Text = "0";

            SqlParameter[] paramList03 = new SqlParameter[4];
            paramList03[0] = new SqlParameter("@CONTACT_ID", SqlDbType.Int, 8, ParameterDirection.Input,
                true, 0, 0, null, DataRowVersion.Default, Utils.GetSessionString("Contact_ID"));
            paramList03[1] = new SqlParameter("@SOURCE_REVIEW_ID", SqlDbType.BigInt, 8, ParameterDirection.Input,
                true, 0, 0, null, DataRowVersion.Default, tbSourceReview.Text);
            paramList03[2] = new SqlParameter("@NEW_REVIEW_ID", SqlDbType.BigInt, 8, ParameterDirection.Input,
                true, 0, 0, null, DataRowVersion.Default, lblDestinationReviewID.Text);
            paramList03[3] = new SqlParameter("@RESULT", SqlDbType.NVarChar, 50, ParameterDirection.Output,
                true, 0, 0, null, DataRowVersion.Default, "");
            Utils.ExecuteSPWithReturnValues(isAdmDB, Server, "st_CopyReviewStep03", paramList03);
            if (paramList03[3].Value.ToString() != "0")
            {
                lblMessage.Text = "Step 03 error - " + paramList03[3].Value.ToString();
            }
            else
            {
                // keep going
                // Step 05: copy the duplicate data from TB_ITEM_DUPLICATE_GROUP, TB_ITEM_DUPLICATE_GROUP_MEMBERS
                lblMessage.Text = "0";
                SqlParameter[] paramList05 = new SqlParameter[4];
                paramList05[0] = new SqlParameter("@CONTACT_ID", SqlDbType.Int, 8, ParameterDirection.Input,
                    true, 0, 0, null, DataRowVersion.Default, Utils.GetSessionString("Contact_ID"));
                paramList05[1] = new SqlParameter("@SOURCE_REVIEW_ID", SqlDbType.BigInt, 8, ParameterDirection.Input,
                    true, 0, 0, null, DataRowVersion.Default, tbSourceReview.Text);
                paramList05[2] = new SqlParameter("@NEW_REVIEW_ID", SqlDbType.BigInt, 8, ParameterDirection.Input,
                    true, 0, 0, null, DataRowVersion.Default, lblDestinationReviewID.Text);
                paramList05[3] = new SqlParameter("@RESULT", SqlDbType.NVarChar, 50, ParameterDirection.Output,
                    true, 0, 0, null, DataRowVersion.Default, "");
                Utils.ExecuteSPWithReturnValues(isAdmDB, Server, "st_CopyReviewStep05", paramList05);
                if (paramList05[3].Value.ToString() != "0")
                {
                    lblMessage.Text = "Step 05 error - " + paramList05[3].Value.ToString();
                }
                else
                {
                    // keep going
                    // Step 07: gather up the set_id's in the the example review 
                    lblMessage.Text = "0";
                    DataTable dtSets7 = new DataTable();
                    System.Data.DataRow newrow7;
                    dtSets7.Columns.Add(new DataColumn("SOURCE_SET_ID", typeof(string)));
                    IDataReader idr = Utils.GetReader(isAdmDB, "st_CopyReviewStep07", tbSourceReview.Text);
                    while (idr.Read())
                    {
                        newrow7 = dtSets7.NewRow();
                        newrow7["SOURCE_SET_ID"] = idr["SET_ID"].ToString();
                        dtSets7.Rows.Add(newrow7);
                    }
                    idr.Close();


                    // Step 09: copy the code sets one at a time (to avoid timeouts)
                    for (int i = 0; i < dtSets7.Rows.Count; i++)
                    {
                        string test = dtSets7.Rows[i]["SOURCE_SET_ID"].ToString();
                        SqlParameter[] paramList09 = new SqlParameter[5];
                        paramList09[0] = new SqlParameter("@CONTACT_ID", SqlDbType.Int, 8, ParameterDirection.Input,
                            true, 0, 0, null, DataRowVersion.Default, Utils.GetSessionString("Contact_ID"));
                        paramList09[1] = new SqlParameter("@SOURCE_REVIEW_ID", SqlDbType.BigInt, 8, ParameterDirection.Input,
                            true, 0, 0, null, DataRowVersion.Default, tbSourceReview.Text);
                        paramList09[2] = new SqlParameter("@DESTINATION_REVIEW_ID", SqlDbType.BigInt, 8, ParameterDirection.Input,
                            true, 0, 0, null, DataRowVersion.Default, lblDestinationReviewID.Text);
                        paramList09[3] = new SqlParameter("@SOURCE_SET_ID", SqlDbType.BigInt, 8, ParameterDirection.Input,
                            true, 0, 0, null, DataRowVersion.Default, dtSets7.Rows[i]["SOURCE_SET_ID"].ToString());
                        paramList09[4] = new SqlParameter("@RESULT", SqlDbType.NVarChar, 50, ParameterDirection.Output,
                            true, 0, 0, null, DataRowVersion.Default, "");
                        Utils.ExecuteSPWithReturnValues(isAdmDB, Server, "st_CopyReviewStep09", paramList09);
                        if (paramList09[4].Value.ToString() != "0")
                        {
                            lblMessage.Text = "Step 09 error - " + paramList09[4].Value.ToString();
                            i = dtSets7.Rows.Count; // stop the loop
                        }
                    }
                    if (lblMessage.Text != "0")
                    {
                        // go no further
                    }
                    else
                    {
                        // keep going
                        // Step 11: copy the data for each set
                        lblMessage.Text = "0";
                        for (int i = 0; i < dtSets7.Rows.Count; i++)
                        {
                            string test = dtSets7.Rows[i]["SOURCE_SET_ID"].ToString();
                            SqlParameter[] paramList11 = new SqlParameter[5];
                            paramList11[0] = new SqlParameter("@CONTACT_ID", SqlDbType.Int, 8, ParameterDirection.Input,
                                true, 0, 0, null, DataRowVersion.Default, Utils.GetSessionString("Contact_ID"));
                            paramList11[1] = new SqlParameter("@SOURCE_REVIEW_ID", SqlDbType.BigInt, 8, ParameterDirection.Input,
                                true, 0, 0, null, DataRowVersion.Default, tbSourceReview.Text);
                            paramList11[2] = new SqlParameter("@DESTINATION_REVIEW_ID", SqlDbType.BigInt, 8, ParameterDirection.Input,
                                true, 0, 0, null, DataRowVersion.Default, lblDestinationReviewID.Text);
                            paramList11[3] = new SqlParameter("@SOURCE_SET_ID", SqlDbType.BigInt, 8, ParameterDirection.Input,
                                true, 0, 0, null, DataRowVersion.Default, dtSets7.Rows[i]["SOURCE_SET_ID"].ToString());
                            paramList11[4] = new SqlParameter("@RESULT", SqlDbType.NVarChar, 50, ParameterDirection.Output,
                                true, 0, 0, null, DataRowVersion.Default, "");
                            Utils.ExecuteSPWithReturnValues(isAdmDB, Server, "st_CopyReviewStep11", paramList11);
                            if (paramList11[4].Value.ToString() != "0")
                            {
                                lblMessage.Text = "Step 11 error - " + paramList11[4].Value.ToString();
                                i = dtSets7.Rows.Count; // stop the loop
                            }
                        }
                        if (lblMessage.Text != "0")
                        {
                            // go no further
                        }
                        else
                        {
                            // keep going
                            // Step 13: work assignments, diagrams, searches
                            lblMessage.Text = "0";
                            SqlParameter[] paramList13 = new SqlParameter[4];
                            paramList13[0] = new SqlParameter("@CONTACT_ID", SqlDbType.Int, 8, ParameterDirection.Input,
                                true, 0, 0, null, DataRowVersion.Default, Utils.GetSessionString("Contact_ID"));
                            paramList13[1] = new SqlParameter("@SOURCE_REVIEW_ID", SqlDbType.BigInt, 8, ParameterDirection.Input,
                                true, 0, 0, null, DataRowVersion.Default, tbSourceReview.Text);
                            paramList13[2] = new SqlParameter("@DESTINATION_REVIEW_ID", SqlDbType.BigInt, 8, ParameterDirection.Input,
                                true, 0, 0, null, DataRowVersion.Default, lblDestinationReviewID.Text);
                            paramList13[3] = new SqlParameter("@RESULT", SqlDbType.NVarChar, 50, ParameterDirection.Output,
                                true, 0, 0, null, DataRowVersion.Default, "");
                            Utils.ExecuteSPWithReturnValues(isAdmDB, Server, "st_CopyReviewStep13", paramList13);
                            if (paramList13[3].Value.ToString() != "0")
                            {
                                lblMessage.Text = "Step 13 error - " + paramList13[3].Value.ToString();
                            }
                            else
                            {
                                // keep going
                                // Step 15: reports, meta-analysis
                                lblMessage.Text = "0";
                                SqlParameter[] paramList15 = new SqlParameter[4];
                                paramList15[0] = new SqlParameter("@CONTACT_ID", SqlDbType.Int, 8, ParameterDirection.Input,
                                    true, 0, 0, null, DataRowVersion.Default, Utils.GetSessionString("Contact_ID"));
                                paramList15[1] = new SqlParameter("@SOURCE_REVIEW_ID", SqlDbType.BigInt, 8, ParameterDirection.Input,
                                    true, 0, 0, null, DataRowVersion.Default, tbSourceReview.Text);
                                paramList15[2] = new SqlParameter("@DESTINATION_REVIEW_ID", SqlDbType.BigInt, 8, ParameterDirection.Input,
                                    true, 0, 0, null, DataRowVersion.Default, lblDestinationReviewID.Text);
                                paramList15[3] = new SqlParameter("@RESULT", SqlDbType.NVarChar, 50, ParameterDirection.Output,
                                    true, 0, 0, null, DataRowVersion.Default, "");
                                Utils.ExecuteSPWithReturnValues(isAdmDB, Server, "st_CopyReviewStep15", paramList15);
                                if (paramList15[3].Value.ToString() != "0")
                                {
                                    lblMessage.Text = "Step 15 error - " + paramList15[3].Value.ToString();
                                }
                                else
                                {
                                    // keep going
                                    // clean up the data (i.e. remove OLD_ATTRIBUTE_ID, OLD_GUIDELINE_ID, OLD_ITEM_ID)
                                    lblMessage.Text = "0";

                                    SqlParameter[] paramListCleanup = new SqlParameter[2];
                                    paramListCleanup[0] = new SqlParameter("@DESTINATION_REVIEW_ID", SqlDbType.BigInt, 8, ParameterDirection.Input,
                                        true, 0, 0, null, DataRowVersion.Default, lblDestinationReviewID.Text);
                                    paramListCleanup[1] = new SqlParameter("@RESULT", SqlDbType.NVarChar, 50, ParameterDirection.Output,
                                        true, 0, 0, null, DataRowVersion.Default, "");
                                    Utils.ExecuteSPWithReturnValues(isAdmDB, Server, "st_CopyReviewStepCleanup", paramListCleanup);
                                    if (paramListCleanup[1].Value.ToString() != "0")
                                    {
                                        lblMessage.Text = "Step cleanup error - " + paramListCleanup[1].Value.ToString();
                                    }
                                    else
                                    {
                                        lblMessage.Text = "Review ID " + tbSourceReview.Text + " has successfully been copied to Review ID " +
                                            lblDestinationReviewID.Text + ". ";
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        if (lblMessage.Text != "0")
        {
            // there has been a problem so run the cleanup
            SqlParameter[] paramListCleanup = new SqlParameter[2];
            paramListCleanup[0] = new SqlParameter("@DESTINATION_REVIEW_ID", SqlDbType.BigInt, 8, ParameterDirection.Input,
                true, 0, 0, null, DataRowVersion.Default, lblDestinationReviewID.Text);
            paramListCleanup[1] = new SqlParameter("@RESULT", SqlDbType.NVarChar, 50, ParameterDirection.Output,
                true, 0, 0, null, DataRowVersion.Default, "");
            Utils.ExecuteSPWithReturnValues(isAdmDB, Server, "st_CopyReviewStepCleanup", paramListCleanup);

            // send an email saying there is a problem

        }
    }

    protected void cmdStep01_Click(object sender, EventArgs e)
    {
        // Step 01: create new review, populate TB_REVIEW, TB_REVIEW_CONTACT, TB_CONTACT_REVIEW_ROLE 
        //  and return new REVIEW_ID
        bool isAdmDB = true;
        lblMessage.Text = "0";

        SqlParameter[] paramList01 = new SqlParameter[3];
        paramList01[0] = new SqlParameter("@CONTACT_ID", SqlDbType.Int, 8, ParameterDirection.Input,
            true, 0, 0, null, DataRowVersion.Default, Utils.GetSessionString("Contact_ID"));
        paramList01[1] = new SqlParameter("@NEW_REVIEW_ID", SqlDbType.BigInt, 8, ParameterDirection.Output,
            true, 0, 0, null, DataRowVersion.Default, "");
        paramList01[2] = new SqlParameter("@RESULT", SqlDbType.NVarChar, 50, ParameterDirection.Output,
            true, 0, 0, null, DataRowVersion.Default, "");
        Utils.ExecuteSPWithReturnValues(isAdmDB, Server, "st_CopyReviewStep01", paramList01);
        if (paramList01[2].Value.ToString() != "0")
        {
            lblMessage.Text = "Step 01 error - " + paramList01[2].Value.ToString();
        }
        else
        {
            tbDestinationReviewID.Text = paramList01[1].Value.ToString();
        }
    }
    protected void cmdStep03_Click(object sender, EventArgs e)
    {
        if ((tbDestinationReviewID.Text == "") || (lblSourceReview.Text == ""))
        {
            lblMessage.Text = "Enter a Source and/or Destination review ID";
        }
        else
        {
            // Step 03: copy the items by populating TB_ITEM, TB_ITEM_REVIEW, TB_ITEM_AUTHOR, TB_ITEM_DOCUMENT, 
            //  TB_ITEM_REVIEW
            bool isAdmDB = true;
            lblMessage.Text = "0";

            SqlParameter[] paramList03 = new SqlParameter[4];
            paramList03[0] = new SqlParameter("@CONTACT_ID", SqlDbType.Int, 8, ParameterDirection.Input,
                true, 0, 0, null, DataRowVersion.Default, Utils.GetSessionString("Contact_ID"));
            paramList03[1] = new SqlParameter("@SOURCE_REVIEW_ID", SqlDbType.BigInt, 8, ParameterDirection.Input,
                true, 0, 0, null, DataRowVersion.Default, tbSourceReview.Text);
            paramList03[2] = new SqlParameter("@NEW_REVIEW_ID", SqlDbType.BigInt, 8, ParameterDirection.Input,
                true, 0, 0, null, DataRowVersion.Default, tbDestinationReviewID.Text);
            paramList03[3] = new SqlParameter("@RESULT", SqlDbType.NVarChar, 50, ParameterDirection.Output,
                true, 0, 0, null, DataRowVersion.Default, "");
            Utils.ExecuteSPWithReturnValues(isAdmDB, Server, "st_CopyReviewStep03", paramList03);
            if (paramList03[3].Value.ToString() != "0")
            {
                lblMessage.Text = "Step 03 error - " + paramList03[3].Value.ToString();
            }
        }
    }
    protected void cmdStep05_Click(object sender, EventArgs e)
    {
        if ((tbDestinationReviewID.Text == "") || (lblSourceReview.Text == ""))
        {
            lblMessage.Text = "Enter a Source and/or Destination review ID";
        }
        else
        {
            // Step 05: copy the duplicate data from TB_ITEM_DUPLICATE_GROUP, TB_ITEM_DUPLICATE_GROUP_MEMBERS
            bool isAdmDB = true;
            lblMessage.Text = "0";

            SqlParameter[] paramList05 = new SqlParameter[4];
            paramList05[0] = new SqlParameter("@CONTACT_ID", SqlDbType.Int, 8, ParameterDirection.Input,
                true, 0, 0, null, DataRowVersion.Default, Utils.GetSessionString("Contact_ID"));
            paramList05[1] = new SqlParameter("@SOURCE_REVIEW_ID", SqlDbType.BigInt, 8, ParameterDirection.Input,
                true, 0, 0, null, DataRowVersion.Default, tbSourceReview.Text);
            paramList05[2] = new SqlParameter("@NEW_REVIEW_ID", SqlDbType.BigInt, 8, ParameterDirection.Input,
                true, 0, 0, null, DataRowVersion.Default, tbDestinationReviewID.Text);
            paramList05[3] = new SqlParameter("@RESULT", SqlDbType.NVarChar, 50, ParameterDirection.Output,
                true, 0, 0, null, DataRowVersion.Default, "");
            Utils.ExecuteSPWithReturnValues(isAdmDB, Server, "st_CopyReviewStep05", paramList05);
            if (paramList05[3].Value.ToString() != "0")
            {
                lblMessage.Text = "Step 05 error - " + paramList05[3].Value.ToString();
            }
        }
    }
    protected void cmdStep0709_Click(object sender, EventArgs e)
    {
        if ((tbDestinationReviewID.Text == "") || (lblSourceReview.Text == ""))
        {
            lblMessage.Text = "Enter a Source and/or Destination review ID";
        }
        else
        {
            // Step 07: gather up the set_id's in the the example review 
            bool isAdmDB = true;
            lblMessage.Text = "0";

            DataTable dtSets = new DataTable();
            System.Data.DataRow newrow;
            dtSets.Columns.Add(new DataColumn("SOURCE_SET_ID", typeof(string)));
            IDataReader idr = Utils.GetReader(isAdmDB, "st_CopyReviewStep07", tbSourceReview.Text);
            while (idr.Read())
            {
                newrow = dtSets.NewRow();
                newrow["SOURCE_SET_ID"] = idr["SET_ID"].ToString();
                dtSets.Rows.Add(newrow);
            }
            idr.Close();


            // Step 09: copy the code sets one at a time (to avoid timeouts)
            for (int i = 0; i < dtSets.Rows.Count; i++)
            {
                string test = dtSets.Rows[i]["SOURCE_SET_ID"].ToString();
                SqlParameter[] paramList09 = new SqlParameter[5];
                paramList09[0] = new SqlParameter("@CONTACT_ID", SqlDbType.Int, 8, ParameterDirection.Input,
                    true, 0, 0, null, DataRowVersion.Default, Utils.GetSessionString("Contact_ID"));
                paramList09[1] = new SqlParameter("@SOURCE_REVIEW_ID", SqlDbType.BigInt, 8, ParameterDirection.Input,
                    true, 0, 0, null, DataRowVersion.Default, tbSourceReview.Text);
                paramList09[2] = new SqlParameter("@DESTINATION_REVIEW_ID", SqlDbType.BigInt, 8, ParameterDirection.Input,
                    true, 0, 0, null, DataRowVersion.Default, tbDestinationReviewID.Text);
                paramList09[3] = new SqlParameter("@SOURCE_SET_ID", SqlDbType.BigInt, 8, ParameterDirection.Input,
                    true, 0, 0, null, DataRowVersion.Default, dtSets.Rows[i]["SOURCE_SET_ID"].ToString());
                paramList09[4] = new SqlParameter("@RESULT", SqlDbType.NVarChar, 50, ParameterDirection.Output,
                    true, 0, 0, null, DataRowVersion.Default, "");
                Utils.ExecuteSPWithReturnValues(isAdmDB, Server, "st_CopyReviewStep09", paramList09);
                if (paramList09[4].Value.ToString() != "0")
                {
                    lblMessage.Text = "Step 09 error - " + paramList09[4].Value.ToString();
                    i = dtSets.Rows.Count; // stop the loop
                }
            }
        }
    }

    protected void cmdStep0711_Click(object sender, EventArgs e)
    {
        if ((tbDestinationReviewID.Text == "") || (lblSourceReview.Text == ""))
        {
            lblMessage.Text = "Enter a Source and/or Destination review ID";
        }
        else
        {
            // Step 07: gather up the set_id's in the the example review 
            bool isAdmDB = true;
            lblMessage.Text = "0";

            DataTable dtSets = new DataTable();
            System.Data.DataRow newrow;
            dtSets.Columns.Add(new DataColumn("SOURCE_SET_ID", typeof(string)));
            IDataReader idr = Utils.GetReader(isAdmDB, "st_CopyReviewStep07", tbSourceReview.Text);
            while (idr.Read())
            {
                newrow = dtSets.NewRow();
                newrow["SOURCE_SET_ID"] = idr["SET_ID"].ToString();
                dtSets.Rows.Add(newrow);
            }
            idr.Close();

            // Step 11: copy the data for each set
            for (int i = 0; i < dtSets.Rows.Count; i++)
            {
                string test = dtSets.Rows[i]["SOURCE_SET_ID"].ToString();
                SqlParameter[] paramList11 = new SqlParameter[5];
                paramList11[0] = new SqlParameter("@CONTACT_ID", SqlDbType.Int, 8, ParameterDirection.Input,
                    true, 0, 0, null, DataRowVersion.Default, Utils.GetSessionString("Contact_ID"));
                paramList11[1] = new SqlParameter("@SOURCE_REVIEW_ID", SqlDbType.BigInt, 8, ParameterDirection.Input,
                    true, 0, 0, null, DataRowVersion.Default, tbSourceReview.Text);
                paramList11[2] = new SqlParameter("@DESTINATION_REVIEW_ID", SqlDbType.BigInt, 8, ParameterDirection.Input,
                    true, 0, 0, null, DataRowVersion.Default, tbDestinationReviewID.Text);
                paramList11[3] = new SqlParameter("@SOURCE_SET_ID", SqlDbType.BigInt, 8, ParameterDirection.Input,
                    true, 0, 0, null, DataRowVersion.Default, dtSets.Rows[i]["SOURCE_SET_ID"].ToString());
                paramList11[4] = new SqlParameter("@RESULT", SqlDbType.NVarChar, 50, ParameterDirection.Output,
                    true, 0, 0, null, DataRowVersion.Default, "");
                Utils.ExecuteSPWithReturnValues(isAdmDB, Server, "st_CopyReviewStep11", paramList11);
                if (paramList11[4].Value.ToString() != "0")
                {
                    lblMessage.Text = "Step 11 error - " + paramList11[4].Value.ToString();
                    i = dtSets.Rows.Count; // stop the loop
                }
            }
        }
    }




    protected void cmdStep13_Click(object sender, EventArgs e)
    {
        if ((tbDestinationReviewID.Text == "") || (lblSourceReview.Text == ""))
        {
            lblMessage.Text = "Enter a Source and/or Destination review ID";
        }
        else
        {
            // Step 13: work assignments, diagrams, searches
            bool isAdmDB = true;
            lblMessage.Text = "0";
            SqlParameter[] paramList13 = new SqlParameter[4];
            paramList13[0] = new SqlParameter("@CONTACT_ID", SqlDbType.Int, 8, ParameterDirection.Input,
                true, 0, 0, null, DataRowVersion.Default, Utils.GetSessionString("Contact_ID"));
            paramList13[1] = new SqlParameter("@SOURCE_REVIEW_ID", SqlDbType.BigInt, 8, ParameterDirection.Input,
                true, 0, 0, null, DataRowVersion.Default, tbSourceReview.Text);
            paramList13[2] = new SqlParameter("@DESTINATION_REVIEW_ID", SqlDbType.BigInt, 8, ParameterDirection.Input,
                true, 0, 0, null, DataRowVersion.Default, tbDestinationReviewID.Text);
            paramList13[3] = new SqlParameter("@RESULT", SqlDbType.NVarChar, 50, ParameterDirection.Output,
                true, 0, 0, null, DataRowVersion.Default, "");
            Utils.ExecuteSPWithReturnValues(isAdmDB, Server, "st_CopyReviewStep13", paramList13);
            if (paramList13[3].Value.ToString() != "0")
            {
                lblMessage.Text = "Step 13 error - " + paramList13[3].Value.ToString();
            }
        }
    }
    protected void cmdStep15_Click(object sender, EventArgs e)
    {
        if ((tbDestinationReviewID.Text == "") || (lblSourceReview.Text == ""))
        {
            lblMessage.Text = "Enter a Source and/or Destination review ID";
        }
        else
        {
            // Step 15: reports, meta-analysis
            bool isAdmDB = true;
            lblMessage.Text = "0";
            SqlParameter[] paramList15 = new SqlParameter[4];
            paramList15[0] = new SqlParameter("@CONTACT_ID", SqlDbType.Int, 8, ParameterDirection.Input,
                true, 0, 0, null, DataRowVersion.Default, Utils.GetSessionString("Contact_ID"));
            paramList15[1] = new SqlParameter("@SOURCE_REVIEW_ID", SqlDbType.BigInt, 8, ParameterDirection.Input,
                true, 0, 0, null, DataRowVersion.Default, tbSourceReview.Text);
            paramList15[2] = new SqlParameter("@DESTINATION_REVIEW_ID", SqlDbType.BigInt, 8, ParameterDirection.Input,
                true, 0, 0, null, DataRowVersion.Default, tbDestinationReviewID.Text);
            paramList15[3] = new SqlParameter("@RESULT", SqlDbType.NVarChar, 50, ParameterDirection.Output,
                true, 0, 0, null, DataRowVersion.Default, "");
            Utils.ExecuteSPWithReturnValues(isAdmDB, Server, "st_CopyReviewStep15", paramList15);
            if (paramList15[3].Value.ToString() != "0")
            {
                lblMessage.Text = "Step 15 error - " + paramList15[3].Value.ToString();
            }
        }
    }
    protected void cmdCleanup_Click(object sender, EventArgs e)
    {
        if ((tbDestinationReviewID.Text == "") || (lblSourceReview.Text == ""))
        {
            lblMessage.Text = "Enter a Source and/or Destination review ID";
        }
        else
        {
            // clean up the data (i.e. remove OLD_ATTRIBUTE_ID, OLD_GUIDELINE_ID, OLD_ITEM_ID)
            bool isAdmDB = true;
            lblMessage.Text = "0";

            SqlParameter[] paramListCleanup = new SqlParameter[2];
            paramListCleanup[0] = new SqlParameter("@DESTINATION_REVIEW_ID", SqlDbType.BigInt, 8, ParameterDirection.Input,
                true, 0, 0, null, DataRowVersion.Default, tbDestinationReviewID.Text);
            paramListCleanup[1] = new SqlParameter("@RESULT", SqlDbType.NVarChar, 50, ParameterDirection.Output,
                true, 0, 0, null, DataRowVersion.Default, "");
            Utils.ExecuteSPWithReturnValues(isAdmDB, Server, "st_CopyReviewStepCleanup", paramListCleanup);
            if (paramListCleanup[1].Value.ToString() != "0")
            {
                lblMessage.Text = "Step cleanup error - " + paramListCleanup[1].Value.ToString();
            }
        }
    }





}