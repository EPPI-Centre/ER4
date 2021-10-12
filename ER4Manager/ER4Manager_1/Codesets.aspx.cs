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
using Telerik.Web.UI;

public partial class Codesets : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (Utils.GetSessionString("Contact_ID") != null)
        {
            if (!IsPostBack)
            {

                System.Web.UI.WebControls.Label lbl = (Label)Master.FindControl("lblPageTitle");
                if (lbl != null)
                {
                    lbl.Text = "This utility will copy a codeset across and within reviews.";
                }

                Telerik.Web.UI.RadTabStrip radTs = (Telerik.Web.UI.RadTabStrip)Master.FindControl("rtsMenu");
                if (radTs != null)
                {
                    radTs.SelectedIndex = 5;
                    radTs.Tabs[5].Tabs[1].Selected = true;
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
                    if ((Utils.GetSessionString("EnableDataPresenter") == "True") || (Utils.GetSessionString("AccessWDSetup") == "1"))
                    {
                        radTs.Tabs[5].Tabs[5].Width = 430;
                    }
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
                    lbl1.Text = "This utility will copy a codeset across and within reviews.";
                }


                bool isAdmDB = true;
                IDataReader idr = Utils.GetReader(isAdmDB, "st_ManagementSettings");
                if (idr.Read()) // it exists
                {
                    lblPublicCodesetReviewID.Text = idr["PUBLIC_CODESETS_REVIEW_ID"].ToString();
                }
                idr.Close();

                buildShareableReviewGrid();
                buildNonShareableReviewGrid();
                buildSourceReviewDDL();
                buildDestinationReviewDDL();
                //FillPublicCodesetsTree(lblPublicCodesetReviewID.Text);
                LoadRootNodes(radTVPublicCodesets, TreeNodeExpandMode.ServerSideCallBack, lblPublicCodesetReviewID.Text);
            }
            if (Utils.GetSessionString("IsAdm") == "True")
            {
                //lblLinkPersonalCodeset.Visible = true;
                //cmdLinkPersonalCodeSet.Visible = true;
                //lblLinkPublicCodeset.Visible = true;
                //cmdLinkPublicCodeSet.Visible = true;
            }
        }
        else
        {
            Server.Transfer("Error.aspx");
        }
    }

    private void buildShareableReviewGrid()
    {
        DataTable dt = new DataTable();
        System.Data.DataRow newrow;

        dt.Columns.Add(new DataColumn("REVIEW_ID", typeof(string)));
        dt.Columns.Add(new DataColumn("REVIEW_NAME", typeof(string)));
        dt.Columns.Add(new DataColumn("URL", typeof(string)));
        dt.Columns.Add(new DataColumn("PASSWORD", typeof(string)));
        bool isAdmDB = true;
        IDataReader idr = Utils.GetReader(isAdmDB, "st_ContactReviewsShareable",
            Utils.GetSessionString("Contact_ID"));
        while (idr.Read())
        {
            newrow = dt.NewRow();
            newrow["REVIEW_ID"] = idr["REVIEW_ID"].ToString();

            if ((idr["REVIEW_NAME"].ToString() == null) || (idr["REVIEW_NAME"].ToString() == ""))
            {
                if ((idr["EXPIRY_DATE"].ToString() == null) || (idr["EXPIRY_DATE"].ToString() == ""))
                {
                    newrow["REVIEW_NAME"] = "Not activated";
                }
                else
                {
                    newrow["REVIEW_NAME"] = "Edit name";
                }
            }
            else
                newrow["REVIEW_NAME"] = idr["REVIEW_NAME"].ToString();

            newrow["URL"] = "N/A";
            newrow["PASSWORD"] = "N/A";

            dt.Rows.Add(newrow);
        }
        idr.Close();

        gvReview.DataSource = dt;
        gvReview.DataBind();

        if (dt.Rows.Count == 0)
            lblShareableReviews.Visible = true;
    }

    private void buildNonShareableReviewGrid()
    {
        DataTable dt = new DataTable();
        System.Data.DataRow newrow;

        dt.Columns.Add(new DataColumn("REVIEW_ID", typeof(string)));
        dt.Columns.Add(new DataColumn("REVIEW_NAME", typeof(string)));
        dt.Columns.Add(new DataColumn("URL", typeof(string)));
        dt.Columns.Add(new DataColumn("PASSWORD", typeof(string)));
        bool isAdmDB = true;
        IDataReader idr = Utils.GetReader(isAdmDB, "st_ContactReviewsNonShareable",
            Utils.GetSessionString("Contact_ID"));
        while (idr.Read())
        {
            newrow = dt.NewRow();
            newrow["REVIEW_ID"] = idr["REVIEW_ID"].ToString();

            if ((idr["REVIEW_NAME"].ToString() == null) || (idr["REVIEW_NAME"].ToString() == ""))
            {
                if ((idr["EXPIRY_DATE"].ToString() == null) || (idr["EXPIRY_DATE"].ToString() == ""))
                {
                    newrow["REVIEW_NAME"] = "Not activated";
                }
                else
                {
                    newrow["REVIEW_NAME"] = "Edit name";
                }
            }
            else
                newrow["REVIEW_NAME"] = idr["REVIEW_NAME"].ToString();

            newrow["URL"] = "N/A";
            newrow["PASSWORD"] = "N/A";

            dt.Rows.Add(newrow);
        }
        idr.Close();

        gvReviewNonShareable.DataSource = dt;
        gvReviewNonShareable.DataBind();

        if (dt.Rows.Count == 0)
            lblNonShareableReviews.Visible = true;
    }

    private void buildSourceReviewDDL()
    {
        bool isAdmDB = true;
        IDataReader idr;
        
        DataTable dt1 = new DataTable();
        System.Data.DataRow newrow1;
        dt1.Columns.Add(new DataColumn("REVIEW_ID", typeof(string)));
        dt1.Columns.Add(new DataColumn("REVIEW_NAME", typeof(string)));

        newrow1 = dt1.NewRow();
        newrow1["REVIEW_ID"] = "0";
        newrow1["REVIEW_NAME"] = "Select a source review";
        dt1.Rows.Add(newrow1);

        isAdmDB = true;
        idr = Utils.GetReader(isAdmDB, "st_ContactReviewsShareable",
            Utils.GetSessionString("Contact_ID"));
        while (idr.Read())
        {
            newrow1 = dt1.NewRow();
            newrow1["REVIEW_ID"] = idr["REVIEW_ID"].ToString();
            newrow1["REVIEW_NAME"] = idr["REVIEW_ID"].ToString() + ": " + idr["REVIEW_NAME"].ToString();
            dt1.Rows.Add(newrow1);

        }
        idr.Close();

        isAdmDB = true;
        idr = Utils.GetReader(isAdmDB, "st_ContactReviewsNonShareable",
            Utils.GetSessionString("Contact_ID"));
        while (idr.Read())
        {
            newrow1 = dt1.NewRow();
            newrow1["REVIEW_ID"] = idr["REVIEW_ID"].ToString();
            newrow1["REVIEW_NAME"] = idr["REVIEW_ID"].ToString() + ": " + idr["REVIEW_NAME"].ToString();
            dt1.Rows.Add(newrow1);

        }
        idr.Close();

        ddlSourceReview.DataSource = dt1;
        ddlSourceReview.DataBind();
    }

    private void buildDestinationReviewDDL()
    {

        bool isAdmDB = true;
        IDataReader idr;

        DataTable dt1 = new DataTable();
        System.Data.DataRow newrow1;
        dt1.Columns.Add(new DataColumn("REVIEW_ID", typeof(string)));
        dt1.Columns.Add(new DataColumn("REVIEW_NAME", typeof(string)));

        newrow1 = dt1.NewRow();
        newrow1["REVIEW_ID"] = "0";
        newrow1["REVIEW_NAME"] = "Select a destination review";
        dt1.Rows.Add(newrow1);

        isAdmDB = true;
        idr = Utils.GetReader(isAdmDB, "st_ContactReviewsShareable",
            Utils.GetSessionString("Contact_ID"));
        while (idr.Read())
        {
            newrow1 = dt1.NewRow();
            newrow1["REVIEW_ID"] = idr["REVIEW_ID"].ToString();
            newrow1["REVIEW_NAME"] = idr["REVIEW_ID"].ToString() + ": " + idr["REVIEW_NAME"].ToString();
            dt1.Rows.Add(newrow1);

        }
        idr.Close();

        isAdmDB = true;
        idr = Utils.GetReader(isAdmDB, "st_ContactReviewsNonShareable",
            Utils.GetSessionString("Contact_ID"));
        while (idr.Read())
        {
            newrow1 = dt1.NewRow();
            newrow1["REVIEW_ID"] = idr["REVIEW_ID"].ToString();
            newrow1["REVIEW_NAME"] = idr["REVIEW_ID"].ToString() + ": " + idr["REVIEW_NAME"].ToString();
            dt1.Rows.Add(newrow1);

        }
        idr.Close();

        ddlDestinationReview.DataSource = dt1;
        ddlDestinationReview.DataBind();
    }


    protected void ddlSourceReview_SelectedIndexChanged(object sender, EventArgs e)
    {
        radTVSourceCodesets.Nodes.Clear();
        LoadRootNodes(radTVSourceCodesets, TreeNodeExpandMode.ServerSideCallBack, ddlSourceReview.SelectedValue);
    }
    protected void ddlDestinationReview_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (ddlDestinationReview.SelectedIndex == 0)
        {
            cmdCopyPersonalCodeSet.Enabled = false;
            cmdCopyPublicCodeSet.Enabled = false;
            cmdLinkPersonalCodeSet.Enabled = false;
            cmdLinkPublicCodeSet.Enabled = false;
            //tvDestinationCodesets.Nodes.Clear();
        }
        else
        {  
            cmdCopyPublicCodeSet.Enabled = true;
            cmdLinkPublicCodeSet.Enabled = true;
            if (ddlSourceReview.SelectedIndex != 0)
            {
                cmdCopyPersonalCodeSet.Enabled = true;
                cmdLinkPersonalCodeSet.Enabled = true;
            }
            //tvDestinationCodesets.Nodes.Clear();
            //FillDestinationCodesetTree(ddlDestinationReview.SelectedValue);
            radTVDestinationCodesets.Nodes.Clear();
            LoadRootNodes(radTVDestinationCodesets, TreeNodeExpandMode.ServerSideCallBack, ddlDestinationReview.SelectedValue);
        }
    }








    protected void cmdCopyPersonalCodeSet_Click(object sender, EventArgs e)
    {
        lblErrorMessage.Visible = false;
        //ComponentArt.Web.UI.TreeViewNode theNode = tvSourceCodesets.SelectedNode;
        Telerik.Web.UI.RadTreeNode theNode = radTVSourceCodesets.SelectedNode;
        bool isAdmDB = true;
        if (ddlDestinationReview.SelectedIndex != 0)
        {
            if (theNode != null)
            {
                SqlParameter[] paramList = new SqlParameter[5];
                paramList[0] = new SqlParameter("@SOURCE_SET_ID", SqlDbType.NVarChar, 50, ParameterDirection.Input,
                    true, 0, 0, null, DataRowVersion.Default, theNode.Attributes["SET_ID"].ToString() /*theNode.ToolTip*/);
                paramList[1] = new SqlParameter("@SOURCE_REVIEW_ID", SqlDbType.NVarChar, 255, ParameterDirection.Input,
                    true, 0, 0, null, DataRowVersion.Default, ddlSourceReview.SelectedValue);
                paramList[2] = new SqlParameter("@DESTINATION_REVIEW_ID", SqlDbType.NVarChar, 50, ParameterDirection.Input,
                    true, 0, 0, null, DataRowVersion.Default, ddlDestinationReview.SelectedValue);
                paramList[3] = new SqlParameter("@CONTACT_ID", SqlDbType.NVarChar, 50, ParameterDirection.Input,
                    true, 0, 0, null, DataRowVersion.Default, Utils.GetSessionString("Contact_ID"));
                paramList[4] = new SqlParameter("@RESULT", SqlDbType.NVarChar, 100, ParameterDirection.Output,
                    true, 0, 0, null, DataRowVersion.Default, "");

                Utils.ExecuteSPWithReturnValues(isAdmDB, Server, "st_CopyCodeset", paramList);

                if (paramList[4].Value.ToString() != "0")
                {
                    lblErrorMessage.Text = paramList[4].Value.ToString();
                    lblErrorMessage.Visible = true;
                }
                //FillDestinationCodesetTree(ddlDestinationReview.SelectedValue);
                radTVDestinationCodesets.Nodes.Clear();
                LoadRootNodes(radTVDestinationCodesets, TreeNodeExpandMode.ServerSideCallBack, ddlDestinationReview.SelectedValue);
            }
            else
            {
                lblErrorMessage.Visible = true;
                lblErrorMessage.Text = "Please select a codeset to copy";
            }
        }
        else
        {
            lblErrorMessage.Visible = true;
            lblErrorMessage.Text = "Please select a destination review";
        }
    }
    protected void cmdCopyPublicCodeSet_Click(object sender, EventArgs e)
    {
        //lblErrorMessage.Visible = true;
        //lblErrorMessage.Text = "test";
        
        lblErrorMessage.Visible = false;
        Telerik.Web.UI.RadTreeNode theNode = radTVPublicCodesets.SelectedNode;
        //ComponentArt.Web.UI.TreeViewNode theNode = tvPublicCodesets.SelectedNode;
        bool isAdmDB = true;
        if (ddlDestinationReview.SelectedIndex != 0)
        {
            if (theNode != null)
            {
                SqlParameter[] paramList = new SqlParameter[5];
                paramList[0] = new SqlParameter("@SOURCE_SET_ID", SqlDbType.NVarChar, 50, ParameterDirection.Input,
                    true, 0, 0, null, DataRowVersion.Default, theNode.Attributes["SET_ID"].ToString() /*.ToolTip*/);
                paramList[1] = new SqlParameter("@SOURCE_REVIEW_ID", SqlDbType.NVarChar, 255, ParameterDirection.Input,
                    true, 0, 0, null, DataRowVersion.Default, lblPublicCodesetReviewID.Text);
                paramList[2] = new SqlParameter("@DESTINATION_REVIEW_ID", SqlDbType.NVarChar, 50, ParameterDirection.Input,
                    true, 0, 0, null, DataRowVersion.Default, ddlDestinationReview.SelectedValue);
                paramList[3] = new SqlParameter("@CONTACT_ID", SqlDbType.NVarChar, 50, ParameterDirection.Input,
                    true, 0, 0, null, DataRowVersion.Default, Utils.GetSessionString("Contact_ID"));
                paramList[4] = new SqlParameter("@RESULT", SqlDbType.NVarChar, 100, ParameterDirection.Output,
                    true, 0, 0, null, DataRowVersion.Default, "");

                
                Utils.ExecuteSPWithReturnValues(isAdmDB, Server, "st_CopyCodeset", paramList);

                if (paramList[4].Value.ToString() != "0")
                {
                    lblErrorMessage.Text = paramList[4].Value.ToString();
                    lblErrorMessage.Visible = true;
                }
                
                //FillDestinationCodesetTree(ddlDestinationReview.SelectedValue);
                radTVDestinationCodesets.Nodes.Clear();
                LoadRootNodes(radTVDestinationCodesets, TreeNodeExpandMode.ServerSideCallBack, ddlDestinationReview.SelectedValue);
            }
            else
            {
                lblErrorMessage.Visible = true;
                lblErrorMessage.Text = "Please select a codeset to copy";
            }
        }
        else
        {
            lblErrorMessage.Visible = true;
            lblErrorMessage.Text = "Please select a destination review";
        }
        
    }


    private static void LoadRootNodes(RadTreeView treeView, TreeNodeExpandMode expandMode, string sourceReviewID)
    {
        bool isAdmDB = true;
        using (System.Data.SqlClient.SqlConnection myConnection = new System.Data.SqlClient.SqlConnection())
        {
            IDataReader idr = Utils.GetReader1(isAdmDB, "st_CodesetsGet", myConnection, sourceReviewID);
            while (idr.Read())
            {
                RadTreeNode NewNode = new RadTreeNode();
                if (idr["SET_NAME"].ToString().Length > 50)
                    NewNode.Text = idr["SET_NAME"].ToString().Substring(0, 50) + "...";
                else
                    NewNode.Text = idr["SET_NAME"].ToString();

                //NewNode.Text = idr["ATTRIBUTE_NAME"].ToString();
                NewNode.Attributes["ATTRIBUTE_SET_ID"] = "0";
                //NewNode.Value = idr["ATTRIBUTE_ID"].ToString();
                NewNode.Attributes["ATTRIBUTE_ID"] = "0";
                //NewNode.ToolTip = idr["ATTRIBUTE_NAME"].ToString();
                NewNode.Attributes["SET_ID"] = idr["SET_ID"].ToString();
                NewNode.Attributes["SET_TYPE"] = idr["SET_TYPE"].ToString();
                NewNode.ExpandMode = expandMode;
                //NewNode.CssClass = "NoNodeSelect";
                NewNode.Attributes["IS_ROOT"] = "Yes";
                treeView.Nodes.Add(NewNode);
            }
            idr.Close();
        }
    }



    private static void PopulateNodeOnDemand(RadTreeNodeEventArgs e, TreeNodeExpandMode expandMode, string reviewID)
    {

        bool isAdmDB = true;
        IDataReader idr = null;
        IDataReader idr1 = null;
        using (System.Data.SqlClient.SqlConnection myConnection = new System.Data.SqlClient.SqlConnection())
        {
            string level = "0";
            if (e.Node.Attributes["IS_ROOT"] == "No")
                level = "1";

            string test = e.Node.Attributes["ATTRIBUTE_SET_ID"].ToString();
            //idr = Utils.GetReader1(isWebDB, "st_AttrDataGet", myConnection, e.Node.Attributes["ATTRIBUTE_ID"], 
            //    e.Node.Attributes["ATTRIBUTE_SET_ID"].ToString(), e.Node.Attributes["SET_ID"]);
            idr = Utils.GetReader1(isAdmDB, "st_WDAttrDataGet_1", myConnection, level,
                e.Node.Attributes["ATTRIBUTE_ID"], e.Node.Attributes["SET_ID"]);
            // this gets the data of the children of what we clicked on
            while (idr.Read())
            {
                Telerik.Web.UI.RadTreeNode newNode = new Telerik.Web.UI.RadTreeNode();

                //newNode.Text = idr["ATTRIBUTE_NAME"].ToString();
                if (idr["ATTRIBUTE_NAME"].ToString().Length > 50)
                    newNode.Text = idr["ATTRIBUTE_NAME"].ToString().Substring(0, 50) + "...";
                else
                    newNode.Text = idr["ATTRIBUTE_NAME"].ToString();

                newNode.Attributes["ATTRIBUTE_SET_ID"] = idr["ATTRIBUTE_SET_ID"].ToString(); ;
                newNode.Value = idr["ATTRIBUTE_ID"].ToString();
                newNode.Attributes["ATTRIBUTE_ID"] = idr["ATTRIBUTE_ID"].ToString();
                newNode.Attributes["SET_ID"] = idr["SET_ID"].ToString();
                newNode.ToolTip = idr["ATTRIBUTE_NAME"].ToString();
                newNode.Attributes["IS_ROOT"] = "No";
                using (System.Data.SqlClient.SqlConnection myConnection1 = new System.Data.SqlClient.SqlConnection())
                {
                    idr1 = Utils.GetReader1(isAdmDB, "st_AttrCountChildren", myConnection1,
                        idr["ATTRIBUTE_ID"].ToString(), idr["ATTRIBUTE_SET_ID"].ToString(),
                        idr["PARENT_ATTRIBUTE_ID"].ToString(), idr["SET_ID"].ToString());

                    if (idr1.Read())
                    {
                        string test2 = idr1["NUM_CHILDREN"].ToString();
                        if (idr1["NUM_CHILDREN"].ToString() != "0")
                        {
                            newNode.Enabled = true;
                            newNode.ExpandMode = expandMode;
                            newNode.CssClass = "NoNodeSelect";
                        }
                        else
                        {
                            newNode.Enabled = true;
                            newNode.CssClass = "NoNodeSelect";
                        }
                    }
                    idr1.Close();
                }
                e.Node.Nodes.Add(newNode);
            }
            idr.Close();
        }
        e.Node.Expanded = true;
        
    }
    protected void radTVSourceCodesets_NodeCollapse(object sender, RadTreeNodeEventArgs e)
    {

    }
    protected void radTVSourceCodesets_NodeExpand(object sender, RadTreeNodeEventArgs e)
    {
        PopulateNodeOnDemand(e, TreeNodeExpandMode.ServerSideCallBack, ddlSourceReview.SelectedValue);
    }
    protected void radTVDestinationCodesets_NodeCollapse(object sender, RadTreeNodeEventArgs e)
    {

    }
    protected void radTVDestinationCodesets_NodeExpand(object sender, RadTreeNodeEventArgs e)
    {
        PopulateNodeOnDemand(e, TreeNodeExpandMode.ServerSideCallBack, ddlDestinationReview.SelectedValue);
    }
    protected void radTVPublicCodesets_NodeCollapse(object sender, RadTreeNodeEventArgs e)
    {

    }
    protected void radTVPublicCodesets_NodeExpand(object sender, RadTreeNodeEventArgs e)
    {
        PopulateNodeOnDemand(e, TreeNodeExpandMode.ServerSideCallBack, lblPublicCodesetReviewID.Text);
    }


}