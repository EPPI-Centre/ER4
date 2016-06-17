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

public partial class Presenter : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (Utils.GetSessionString("Contact_ID") != null)
        {
            //if (Utils.GetSessionString("IsAdm") == "True")
            //{
                if (!IsPostBack)
                {
                    System.Web.UI.WebControls.Label lbl = (Label)Master.FindControl("lblPageTitle");
                    if (lbl != null)
                    {
                        lbl.Text = "EPPI Data viewer allows you to display your review data in a web database.";
                    }

                    Telerik.Web.UI.RadTabStrip radTs = (Telerik.Web.UI.RadTabStrip)Master.FindControl("rtsMenu");
                    if (radTs != null)
                    {
                        radTs.SelectedIndex = 5;
                        radTs.Tabs[5].Tabs[1].Selected = true;
                        radTs.Tabs[5].Tabs[5].Width = 550;
                        if (Utils.GetSessionString("EnableDataPresenter") == "True")
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
                    }
                    System.Web.UI.WebControls.Label lbl1 = (Label)Master.FindControl("lblHeadingText");
                    if (lbl1 != null)
                    {
                        lbl1.Text = "EPPI Data viewer allows you to display your review data in a web database.";
                    }


                    buildShareableReviewGrid();
                    buildNonShareableReviewGrid();
                    //buildOtherShareableReviewGrid();
                    if (Utils.GetSessionString("IsAdm") == "True")
                    {
                        pnlAdmin.Visible = true;
                    }
                }
                lbDelete.Attributes.Add("onclick", "if (confirm('Are you sure you want to delete this web database? " +
                    "This will include the header and introduction text and your choice of codes selected for display.') == false) return false;");
            }
            else
            {
                Server.Transfer("Error.aspx");
            }
        //}
        //else
        //{
        //    Server.Transfer("Error.aspx");
        //}
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
        else
        {
            /*
            for (int i = 0; i < gvReview.Rows.Count; i++)
            {
                if (gvReview.Rows[i].Cells[1].Text.Contains("Edit name"))
                {
                    gvReview.Rows[i].Cells[1].BackColor = System.Drawing.Color.PaleGreen;
                }
                if (gvReview.Rows[i].Cells[1].Text.Contains("Not activated"))
                {
                    gvReview.Rows[i].Cells[1].BackColor = System.Drawing.Color.Yellow;
                }
                if (gvReview.Rows[i].Cells[4].Text.Contains("Not activated"))
                {
                    gvReview.Rows[i].Cells[4].BackColor = System.Drawing.Color.Yellow;
                }
                if (gvReview.Rows[i].Cells[4].Text.Contains("Expired"))
                {
                    gvReview.Rows[i].Cells[4].BackColor = System.Drawing.Color.Pink;
                    gvReview.Rows[i].Cells[4].Font.Bold = true;
                }
            }
            */
        }
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
        else
        {
            /*
            for (int i = 0; i < gvReview.Rows.Count; i++)
            {
                if (gvReview.Rows[i].Cells[1].Text.Contains("Edit name"))
                {
                    gvReview.Rows[i].Cells[1].BackColor = System.Drawing.Color.PaleGreen;
                }
                if (gvReview.Rows[i].Cells[1].Text.Contains("Not activated"))
                {
                    gvReview.Rows[i].Cells[1].BackColor = System.Drawing.Color.Yellow;
                }
                if (gvReview.Rows[i].Cells[4].Text.Contains("Not activated"))
                {
                    gvReview.Rows[i].Cells[4].BackColor = System.Drawing.Color.Yellow;
                }
                if (gvReview.Rows[i].Cells[4].Text.Contains("Expired"))
                {
                    gvReview.Rows[i].Cells[4].BackColor = System.Drawing.Color.Pink;
                    gvReview.Rows[i].Cells[4].Font.Bold = true;
                }
            }
            */
        }
    }



    protected void gvReview_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        int index = Convert.ToInt32(e.CommandArgument);
        string ReviewID = (string)gvReview.DataKeys[index].Value;
        lblReviewID.Text = ReviewID;
        bool isAdmDB = true;
        IDataReader idr;

        /*

        */

        switch (e.CommandName)
        {
            case "SELECT":
                //EmptyCodesetTree();
                pnlEditShareable.Visible = true;
                idr = Utils.GetReader(isAdmDB, "st_ReviewDetails",
                    ReviewID);
                if (idr.Read())
                {
                    lblReviewName.Text = idr["REVIEW_NAME"].ToString();
                    
                }
                idr.Close();

                DataTable dt1 = new DataTable();
                System.Data.DataRow newrow1;
                dt1.Columns.Add(new DataColumn("WEBDB_ID", typeof(string)));
                dt1.Columns.Add(new DataColumn("WEBDB_NAME", typeof(string)));

                newrow1 = dt1.NewRow();
                newrow1["WEBDB_ID"] = "0";
                newrow1["WEBDB_NAME"] = "Select or create a web database";
                dt1.Rows.Add(newrow1);

                isAdmDB = true;
                idr = Utils.GetReader(isAdmDB, "st_WDGetWebDatabases", ReviewID);
                while (idr.Read())
                {
                    newrow1 = dt1.NewRow();
                    newrow1["WEBDB_ID"] = idr["WEBDB_ID"].ToString();
                    newrow1["WEBDB_NAME"] = idr["WEBDB_NAME"].ToString();
                    dt1.Rows.Add(newrow1);
                    
                }
                idr.Close();
                ddlWebDatabases.DataSource = dt1;
                ddlWebDatabases.DataBind();

                pnlIntroText.Visible = false;
                pnlChooseCodes.Visible = false;


/*

                FillCodesetTree(ReviewID);
                FillWebDBTree(ReviewID);
                pnlChooseCodes.Visible = true;
 */
                break;

            case "EDIT":
                pnlEditShareable.Visible = true;
                isAdmDB = true;
                idr = Utils.GetReader(isAdmDB, "st_WDDescriptionGet",
                    ReviewID);
                if (idr.Read())
                {
                    //tbIntroduction.Text = idr["DESCRIPTION"].ToString();

                }
                idr.Close();
                pnlIntroText.Visible = true;
                break;

            default:
                break;
        }
    }
    protected void gvReviewNonShareable_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        int index = Convert.ToInt32(e.CommandArgument);
        string ReviewID = (string)gvReviewNonShareable.DataKeys[index].Value;
        lblReviewID.Text = ReviewID;
        bool isAdmDB = true;
        IDataReader idr;

        /*

        */

        switch (e.CommandName)
        {
            case "SELECT":
                //EmptyCodesetTree();
                pnlEditShareable.Visible = true;
                idr = Utils.GetReader(isAdmDB, "st_ReviewDetails",
                    ReviewID);
                if (idr.Read())
                {
                    lblReviewName.Text = idr["REVIEW_NAME"].ToString();

                }
                idr.Close();

                DataTable dt1 = new DataTable();
                System.Data.DataRow newrow1;
                dt1.Columns.Add(new DataColumn("WEBDB_ID", typeof(string)));
                dt1.Columns.Add(new DataColumn("WEBDB_NAME", typeof(string)));

                newrow1 = dt1.NewRow();
                newrow1["WEBDB_ID"] = "0";
                newrow1["WEBDB_NAME"] = "Select or create a web database";
                dt1.Rows.Add(newrow1);

                isAdmDB = true;
                idr = Utils.GetReader(isAdmDB, "st_WDGetWebDatabases", ReviewID);
                while (idr.Read())
                {
                    newrow1 = dt1.NewRow();
                    newrow1["WEBDB_ID"] = idr["WEBDB_ID"].ToString();
                    newrow1["WEBDB_NAME"] = idr["WEBDB_NAME"].ToString();
                    dt1.Rows.Add(newrow1);

                }
                idr.Close();
                ddlWebDatabases.DataSource = dt1;
                ddlWebDatabases.DataBind();

                pnlIntroText.Visible = false;
                pnlChooseCodes.Visible = false;
                break;

            default:
                break;
        }
    }
    protected void gvReview_RowEditing(object sender, GridViewEditEventArgs e)
    {

    }
    protected void cmdSaveText_Click(object sender, EventArgs e)
    {
        bool isAdmDB = true;
        Utils.ExecuteSP(isAdmDB, Server, "st_WDSaveIntroduction", ddlWebDatabases.SelectedValue, tbWebDatabaseName.Text);
    }

    /*
    protected void FillCodesetTree(string reviewID)
    {
        bool isAdmDB = true;
        IDataReader idr = Utils.GetReader(isAdmDB, "st_WDCodesetsGet", reviewID);
        while (idr.Read())
        {
            ComponentArt.Web.UI.TreeViewNode NewNode = new ComponentArt.Web.UI.TreeViewNode();
            NewNode.Text = idr["SET_NAME"].ToString();
            // added
            NewNode.ID = idr["SET_ID"].ToString();
            NewNode.Value = "0"; //level
            NewNode.EditingEnabled = false;
            NewNode.ContentCallbackUrl = "WebDBXmlRetrieve.aspx?Side=ER&Level=0&SID=" +
                idr["SET_ID"].ToString().Trim();
            tvReviewGuidelines.Nodes.Add(NewNode);
        }
        idr.Close();
    }
    */

    /*
    protected void FillIncludeCodeTree(string reviewID)
    {
        // Node.ID = ATTRIBUTE_SET_ID
        // Node.Text = ATTRIBUTE_NAME
        // Node.Value = ATTRIBUTE_ID
        // Node.ToolTip = SET_ID
        
        tvIncludeCode.Nodes.Clear();
        bool isAdmDB = true;
        IDataReader idr = Utils.GetReader(isAdmDB, "st_WDCodesetsGet", reviewID);
        while (idr.Read())
        {
            ComponentArt.Web.UI.TreeViewNode NewNode = new ComponentArt.Web.UI.TreeViewNode();

            

            
            NewNode.Text = idr["SET_NAME"].ToString();
            //NewNode.ID = idr["SET_ID"].ToString();
            NewNode.Value = "0"; 
            NewNode.Selectable = false;
            NewNode.EditingEnabled = false;
            NewNode.ContentCallbackUrl = "WebDBXmlRetrieve.aspx?Side=ERIncludeCode&Level=0&SID=" +
                idr["SET_ID"].ToString().Trim();
            
            
            tvIncludeCode.Nodes.Add(NewNode);
        }
        idr.Close();
    }
    */

    /*
    protected void FillWebDBTree(string webDatabaseID)
    {
        bool isAdmDB = true;
        tvWebDatabase.Nodes.Clear();
        IDataReader idr = Utils.GetReader(isAdmDB, "st_WDTopLevelGetFromWD", webDatabaseID);
        while (idr.Read())
        {
            ComponentArt.Web.UI.TreeViewNode NewNode = new ComponentArt.Web.UI.TreeViewNode();
            NewNode.Text = idr["ATTRIBUTE_NAME"].ToString();
            NewNode.ID = idr["ATTRIBUTE_ID"].ToString();
            NewNode.Value = idr["ATTRIBUTE_SET_ID"].ToString();
            NewNode.ToolTip = idr["SET_ID"].ToString();
            NewNode.Selectable = true;
            NewNode.EditingEnabled = true;
            NewNode.ContentCallbackUrl = "WebDBXMLRetrieve.aspx?Side=WD" +
                "&ID=" + idr["ATTRIBUTE_ID"].ToString().Trim() +
                "&ASID=" + idr["ATTRIBUTE_SET_ID"].ToString().Trim() +
                "&SID=" + idr["SET_ID"].ToString().Trim() +
                "&webDatabaseID=" + webDatabaseID;
            tvWebDatabase.Nodes.Add(NewNode);
        }
        idr.Close(); idr.Close();
    }
     */

    /*
    protected void EmptyCodesetTree()
    {
        tvReviewGuidelines.Nodes.Clear();
    }
    */
    protected void cmdCopy_Click(object sender, EventArgs e)
    {
        // Rules on copying.
        // 1. what you are copying must have child codes
        // 2. any thing you copy also copies everything below it (could be multiple levels)
        // 3. no part of what you are copying (lower levels) can already be in the WD table. Must check for this!
        // 4. 
        
        //ComponentArt.Web.UI.TreeViewNode theNode = tvReviewGuidelines.SelectedNode;
        Telerik.Web.UI.RadTreeNode theNode = RadTVReviewGuidelines.SelectedNode;
        IDataReader idr = null;
        bool isAdmDB = true;
        bool hasChildCodes = false;
        lblNoCopy.Visible = false;
        bool childrenExist = false;
        string parentAttributeID = "";
        string level = "0";
        if (theNode != null)
        {
            // this isn't really needed as we force a root selection in javascript
            string test = theNode.Attributes["IS_ROOT"].ToString();
            if (theNode.Attributes["IS_ROOT"].ToString() == "No")
            {
                while (theNode.ParentNode.ToString() != null)
                {
                    parentAttributeID = theNode.ParentNode.Attributes["ATTRIBUTE_ID"];
                }

            }
     
            string test1 = theNode.FullPath.ToString();
            //idr = Utils.GetReader(isAdmDB, "st_WDAttrCountChildren", theNode.ID, theNode.Value);
            idr = Utils.GetReader(isAdmDB, "st_WDAttrCountChildren", theNode.Attributes["SET_ID"], level);
            if (idr.Read())
            {
                string test33 = idr["NUM_CHILDREN"].ToString();
                if (idr["NUM_CHILDREN"].ToString() != "0")
                    hasChildCodes = true;
                else
                    lblNoCopy.Visible = true;
            }
            idr.Close();

            if (hasChildCodes == true)
            {
                SqlParameter[] paramList = new SqlParameter[4];
                paramList[0] = new SqlParameter("@SET_ID", SqlDbType.BigInt, 8, ParameterDirection.Input,
                    true, 0, 0, null, DataRowVersion.Default, theNode.Attributes["SET_ID"]);
                paramList[1] = new SqlParameter("@REVIEW_ID", SqlDbType.BigInt, 8, ParameterDirection.Input,
                    true, 0, 0, null, DataRowVersion.Default, lblReviewID.Text);
                paramList[2] = new SqlParameter("@WEBDB_ID", SqlDbType.BigInt, 8, ParameterDirection.Input,
                    true, 0, 0, null, DataRowVersion.Default, ddlWebDatabases.SelectedValue);
                paramList[3] = new SqlParameter("@INSERTED", SqlDbType.Bit, 1, ParameterDirection.Output,
                    true, 0, 0, null, DataRowVersion.Default, "");
                Utils.ExecuteSPWithReturnValues(isAdmDB, Server, "st_WDSetCopyToWebDB", paramList);
                bool inserted = bool.Parse(paramList[3].Value.ToString());
                //FillWebDBTree(ddlWebDatabases.SelectedValue);          
            }
            ddlWebDatabases_SelectedIndexChanged(sender, e);
        }
    }
    protected void cmdDelete_Click(object sender, EventArgs e)
    {
        //ComponentArt.Web.UI.TreeViewNode theNode = tvWebDatabase.SelectedNode;
        Telerik.Web.UI.RadTreeNode theNode = RadTVWebDatabase.SelectedNode;
        bool isAdmDB = true;
        if (theNode != null)
        {
            if (theNode.Value == "") 
            {
                // it's at the codeset level so we can remove everything with that code set.
                Utils.ExecuteSP(isAdmDB, Server, "st_WDDeleteFromWebDB", ddlWebDatabases.SelectedValue, 
                    theNode.Attributes["SET_ID"]/*theNode.ID**/, "whole set");
            }
            else
            {
                // delete the selected code and everything below it
                Utils.ExecuteSP(isAdmDB, Server, "st_WDDeleteChildrenFromCodeset", ddlWebDatabases.SelectedValue,
                    theNode.Attributes["ATTRIBUTE_SET_ID"]/*theNode.ID*/ /*attribute_set_id*/, 
                    theNode.Attributes["SET_ID"]/*theNode.ToolTip*/ /*set_id*/);
            }
            //FillWebDBTree(lblReviewID.Text);
            ddlWebDatabases_SelectedIndexChanged(sender, e);
        }
    }




    protected bool DoesAttrHaveChildren(int AttrbuteID)
    {
        bool childrenExist = false;
        bool isAdmDB = true;
        IDataReader idr = Utils.GetReader(isAdmDB, "st_WDAttrCountChildren", AttrbuteID, 1);
        if (idr.Read())
        {
            if (idr["NUM_CHILDREN"].ToString() != "0")
            {
                childrenExist = true;
                GetAttrsChildren(AttrbuteID);
            }
        }
        idr.Close();
        return childrenExist;
    }

    protected bool GetAttrsChildren(int AttrbuteID)
    {
        string codeName = "";
        bool childrenExist = false;
        bool isAdmDB = true;
        IDataReader idr = Utils.GetReader(isAdmDB, "st_WDAttrDataGet_1", 1, AttrbuteID);
        while (idr.Read())
        {
            // does it have children?
            codeName = idr["ATTRIBUTE_NAME"].ToString();
            childrenExist = DoesAttrHaveChildren(int.Parse(idr["ATTRIBUTE_ID"].ToString()));
            if (childrenExist)
            {
                DoesAttrHaveChildren(AttrbuteID);
            }
            else
            {
                // no kids so put it in the table

            }
        }
        idr.Close();
        return childrenExist;
    }


    protected void lbAddWebDatabase_Click(object sender, EventArgs e)
    {
        pnlNewWebdatabase.Visible = true;
        pnlIntroText.Visible = false;
        pnlChooseCodes.Visible = false;
        ddlWebDatabases.SelectedIndex = 0;
    }
    protected void lbCancel_Click(object sender, EventArgs e)
    {
        pnlNewWebdatabase.Visible = false;
    }

    protected void ddlWebDatabases_SelectedIndexChanged(object sender, EventArgs e)
    {
        pnlPassword.Visible = false;
        tbPassword.Text = "";
        tbUserName.Text = "";
        string test = "";
        if (ddlWebDatabases.SelectedIndex != 0)
        {
            bool isAdmDB = true;
            IDataReader idr;
            idr = Utils.GetReader(isAdmDB, "st_WDDescriptionGet_2",
                ddlWebDatabases.SelectedValue);
            if (idr.Read())
            {
                Utils.SetSessionString("WebDatabaseID", ddlWebDatabases.SelectedValue);
                // to work locally start ER4WebDB in another session and get the localhost number
                //hlWebDBUrl.NavigateUrl = "http://localhost:57035/ER4WebDB/Intro.aspx?ID=" + ddlWebDatabases.SelectedValue;
                //hlWebDBUrl.Text = "http://localhost:57035/ER4WebDB/Intro.aspx?ID=" + ddlWebDatabases.SelectedValue;

                // this is what should be there when running live
                hlWebDBUrl.NavigateUrl = "http://eppi.ioe.ac.uk/webdatabases4/Intro.aspx?ID=" + ddlWebDatabases.SelectedValue;
                hlWebDBUrl.Text = "http://eppi.ioe.ac.uk/webdatabases4/Intro.aspx?ID=" + ddlWebDatabases.SelectedValue;

                //tbIntroduction.Text = idr["DESCRIPTION"].ToString();
                tbWebDatabaseName.Text = idr["WEBDB_NAME"].ToString();
                tbPassword.Text = idr["PASSWD"].ToString();
                tbUserName.Text = idr["USERNAME"].ToString();
                if (idr["ATTRIBUTE_NAME"].ToString() != "")
                    lblIncludeCode.Text = idr["ATTRIBUTE_NAME"].ToString();
                else
                    lblIncludeCode.Text = "No code selected (include all 'completed' studies)";
                //JB
                Utils.SetSessionString("DescriptionAdminEdit", idr["DESCRIPTION_ADMIN_EDIT"].ToString());
                test = Utils.GetSessionString("DescriptionAdminEdit");
                test = idr["RESTRICT_ACCESS"].ToString();
                if (bool.Parse(idr["RESTRICT_ACCESS"].ToString()) == true)
                {
                    pnlPassword.Visible = true;
                    rblRestrictAccess.SelectedIndex = 0;
                }
                else
                {
                    rblRestrictAccess.SelectedIndex = 1;
                }

                //JBtest = idr["SHOW_CODING"].ToString();
                //JB
                
                if (idr["SHOW_CODING"].ToString() == "True")
                {
                    cbShowCoding.Checked = true;
                }
                else
                {
                    cbShowCoding.Checked = false;
                }
                if (idr["DISPLAY_SAVED_CROSSTABS"].ToString() == "True")
                {
                    cbDisplaySavedCrosstabs.Checked = true;
                }
                else
                {
                    cbDisplaySavedCrosstabs.Checked = false;
                }
                if (idr["SAVE_CROSSTABS"].ToString() == "True")
                {
                    cbSaveCrosstabs.Checked = true;
                }
                else
                {
                    cbSaveCrosstabs.Checked = false;
                }
                //*/
                //lbViewEditHeader.Attributes.Add("href", @"javascript:openEditIntro('intro','" + ddlWebDatabases.SelectedValue + "')");
                lbViewEditHeader.Attributes.Add("href", @"javascript:openEditIntro('intro','" + ddlWebDatabases.SelectedValue + "')");
                lbViewEditHeaderImage.Attributes.Add("href", @"javascript:openEditHeaderImage('intro','" + ddlWebDatabases.SelectedValue + "')");
                //lbViewEditHeader.Attributes.Add("href", @"javascript:openEditIntro('header','" + ddlWebDatabases.SelectedValue + "')");

                if (Utils.GetSessionString("IsAdm") == "True")
                {
                    cbAdminEdit.Visible = true;
                    pnlCrosstabs.Visible = true;
                    //lblReportText.Visible = true;
                    //lbViewEditReportIntro.Visible = true;
                    //lbViewEditReportIntro.Attributes.Add("href", @"javascript:openEditIntro('report','" + ddlWebDatabases.SelectedValue + "')");
                }
                
                if (Utils.GetSessionString("DescriptionAdminEdit") == "True")
                    cbAdminEdit.Checked = true;
                else
                    cbAdminEdit.Checked = false;
            }
            idr.Close();
            pnlIntroText.Visible = true;
            //tvReviewGuidelines.Nodes.Clear();
            //tvWebDatabase.Nodes.Clear();

            
            //FillCodesetTree(lblReviewID.Text);
            //FillIncludeCodeTree(lblReviewID.Text);
            //FillWebDBTree(ddlWebDatabases.SelectedValue);

            
            RadTVReviewGuidelines.Nodes.Clear();
            LoadRootNodesadTVReviewGuidelines(RadTVReviewGuidelines, TreeNodeExpandMode.ServerSideCallBack, lblReviewID.Text);
            
            RadTVIncludeCode.Nodes.Clear();
            LoadRootNodesRadTVIncludeCode(RadTVIncludeCode, TreeNodeExpandMode.ServerSideCallBack, lblReviewID.Text);
            
            RadTVWebDatabase.Nodes.Clear();
            LoadRootNodesRadTVWebDatabase(RadTVWebDatabase, TreeNodeExpandMode.ServerSideCallBack, ddlWebDatabases.SelectedValue);
            
            
            pnlChooseCodes.Visible = true;
        }
        else
        {
            pnlChooseCodes.Visible = false;
            pnlIntroText.Visible = false;
        }
    }

    private static void LoadRootNodesadTVReviewGuidelines(RadTreeView treeView, TreeNodeExpandMode expandMode, string sourceReviewID)
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
                NewNode.ToolTip = idr["SET_NAME"].ToString();
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

    private static void LoadRootNodesRadTVIncludeCode(RadTreeView treeView, TreeNodeExpandMode expandMode, string sourceReviewID)
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
                NewNode.ToolTip = idr["SET_NAME"].ToString();
                //NewNode.Text = idr["ATTRIBUTE_NAME"].ToString();
                NewNode.Attributes["ATTRIBUTE_SET_ID"] = "0";
                //NewNode.Value = idr["ATTRIBUTE_ID"].ToString();
                NewNode.Attributes["ATTRIBUTE_ID"] = "0";
                //NewNode.ToolTip = idr["ATTRIBUTE_NAME"].ToString();
                NewNode.Attributes["SET_ID"] = idr["SET_ID"].ToString();
                NewNode.Attributes["SET_TYPE"] = idr["SET_TYPE"].ToString();
                NewNode.ExpandMode = expandMode;
                NewNode.CssClass = "NoNodeSelect";
                NewNode.Attributes["IS_ROOT"] = "Yes";
                treeView.Nodes.Add(NewNode);
            }
            idr.Close();
        }
    }

    private static void LoadRootNodesRadTVWebDatabase(RadTreeView treeView, TreeNodeExpandMode expandMode, string webDatabaseID)
    {
        bool isAdmDB = true;
        using (System.Data.SqlClient.SqlConnection myConnection = new System.Data.SqlClient.SqlConnection())
        {
            IDataReader idr = Utils.GetReader1(isAdmDB, "st_WDTopLevelGetFromWD", myConnection, webDatabaseID);
            while (idr.Read())
            {
                RadTreeNode NewNode = new RadTreeNode();
                if (idr["ATTRIBUTE_NAME"].ToString().Length > 50)
                    NewNode.Text = idr["ATTRIBUTE_NAME"].ToString().Substring(0, 50) + "...";
                else
                    NewNode.Text = idr["ATTRIBUTE_NAME"].ToString();
                NewNode.ToolTip = idr["ATTRIBUTE_NAME"].ToString();
                //NewNode.Text = idr["ATTRIBUTE_NAME"].ToString();
                NewNode.Attributes["ATTRIBUTE_SET_ID"] = idr["ATTRIBUTE_SET_ID"].ToString();
                //NewNode.Value = idr["ATTRIBUTE_ID"].ToString();
                NewNode.Attributes["ATTRIBUTE_ID"] = idr["ATTRIBUTE_ID"].ToString();
                //NewNode.ToolTip = idr["ATTRIBUTE_NAME"].ToString();
                NewNode.Attributes["SET_ID"] = idr["SET_ID"].ToString();
                //NewNode.Attributes["SET_TYPE"] = idr["SET_TYPE"].ToString();
                NewNode.ExpandMode = expandMode;
                //NewNode.CssClass = "NoNodeSelect";
                NewNode.Attributes["IS_ROOT"] = "Yes";
                treeView.Nodes.Add(NewNode);
            }
            idr.Close();
        }
    }

    /*
            bool isAdmDB = true;
        tvWebDatabase.Nodes.Clear();
        IDataReader idr = Utils.GetReader(isAdmDB, "st_WDTopLevelGetFromWD", webDatabaseID);
        while (idr.Read())
        {
            ComponentArt.Web.UI.TreeViewNode NewNode = new ComponentArt.Web.UI.TreeViewNode();
            NewNode.Text = idr["ATTRIBUTE_NAME"].ToString();
            NewNode.ID = idr["ATTRIBUTE_ID"].ToString();
            NewNode.Value = idr["ATTRIBUTE_SET_ID"].ToString();
            NewNode.ToolTip = idr["SET_ID"].ToString();
            NewNode.Selectable = true;
            NewNode.EditingEnabled = true;
            NewNode.ContentCallbackUrl = "WebDBXMLRetrieve.aspx?Side=WD" +
                "&ID=" + idr["ATTRIBUTE_ID"].ToString().Trim() +
                "&ASID=" + idr["ATTRIBUTE_SET_ID"].ToString().Trim() +
                "&SID=" + idr["SET_ID"].ToString().Trim() +
                "&webDatabaseID=" + webDatabaseID;
            tvWebDatabase.Nodes.Add(NewNode);
        }
        idr.Close(); idr.Close();
    */




    protected void lbDelete_Click(object sender, EventArgs e)
    {
        bool isAdmDB = true;
        Utils.ExecuteSP(isAdmDB, Server, "st_WDDeleteWebDB", ddlWebDatabases.SelectedValue);
        
        reloadWebDBList();
        tbNewWebDatabase0.Text = "Enter new web database name";
        pnlNewWebdatabase.Visible = false;
        pnlChooseCodes.Visible = false;
        pnlIntroText.Visible = false;
        ddlWebDatabases.SelectedIndex = 0;
    }
    protected void cmdAdd_Click(object sender, EventArgs e)
    {
        // check if the Web database exists. If not then create it.
        lblMessage.Visible = false;
        int webDatabaseID = 0;
        if (tbNewWebDatabase0.Text != "Enter new web database name")
        {
            bool isAdmDB = true;
            SqlParameter[] paramList = new SqlParameter[3];
            paramList[0] = new SqlParameter("@WEBDB_NAME", SqlDbType.NVarChar, 100, ParameterDirection.Input,
                true, 0, 0, null, DataRowVersion.Default, tbNewWebDatabase0.Text);
            paramList[1] = new SqlParameter("@REVIEW_ID", SqlDbType.Int, 8, ParameterDirection.Input,
                true, 0, 0, null, DataRowVersion.Default, lblReviewID.Text);
            paramList[2] = new SqlParameter("@WEBDB_ID", SqlDbType.Int, 8, ParameterDirection.Output,
                true, 0, 0, null, DataRowVersion.Default, "");
            Utils.ExecuteSPWithReturnValues(isAdmDB, Server, "st_WDCreateIfNotExist", paramList);
            webDatabaseID = int.Parse(paramList[2].Value.ToString());

            if (webDatabaseID == 0)
            {
                lblMessage.Visible = true;
            }
            else
            {
                reloadWebDBList();
                tbNewWebDatabase0.Text = "Enter new web database name";
                pnlNewWebdatabase.Visible = false;
                pnlChooseCodes.Visible = false;
                pnlIntroText.Visible = false;
            }
        }
        
    }
    protected void reloadWebDBList()
    {
        bool isAdmDB = true;
        DataTable dt1 = new DataTable();
        System.Data.DataRow newrow1;
        dt1.Columns.Add(new DataColumn("WEBDB_ID", typeof(string)));
        dt1.Columns.Add(new DataColumn("WEBDB_NAME", typeof(string)));

        newrow1 = dt1.NewRow();
        newrow1["WEBDB_ID"] = "0";
        newrow1["WEBDB_NAME"] = "Select a Web database";
        dt1.Rows.Add(newrow1);

        isAdmDB = true;
        IDataReader idr = Utils.GetReader(isAdmDB, "st_WDGetWebDatabases", lblReviewID.Text);
        while (idr.Read())
        {
            newrow1 = dt1.NewRow();
            newrow1["WEBDB_ID"] = idr["WEBDB_ID"].ToString();
            newrow1["WEBDB_NAME"] = idr["WEBDB_NAME"].ToString();
            dt1.Rows.Add(newrow1);

        }
        idr.Close();
        ddlWebDatabases.DataSource = dt1;
        ddlWebDatabases.DataBind();
    }
    protected void lbHideCodes_Click(object sender, EventArgs e)
    {
        pnlChooseCodes.Visible = false;
    }
    protected void lbHideText_Click(object sender, EventArgs e)
    {
        pnlIntroText.Visible = false;
    }

    protected void rblRestrictAccess_SelectedIndexChanged(object sender, EventArgs e)
    {
        lblPasswordMessage.Visible = false;
        bool isAdmDB = true;
        if (rblRestrictAccess.SelectedValue == "1")
        {
            pnlPassword.Visible = true;
            Utils.ExecuteSP(isAdmDB, Server, "st_WDSetRestrictAccess", true, ddlWebDatabases.SelectedValue);
        }
        else
        {
            pnlPassword.Visible = false;
            Utils.ExecuteSP(isAdmDB, Server, "st_WDSetRestrictAccess", false, ddlWebDatabases.SelectedValue);
        }
    }

    protected void blCancelPassword_Click(object sender, EventArgs e)
    {
        pnlPassword.Visible = false;
        // reload the rbl
        bool isAdmDB = true;
        IDataReader idr;
        idr = Utils.GetReader(isAdmDB, "st_WDDescriptionGet",
            ddlWebDatabases.SelectedValue);
        if (idr.Read())
        {
            if (bool.Parse(idr["RESTRICT_ACCESS"].ToString()) == true)
            {
                pnlPassword.Visible = true;
                rblRestrictAccess.SelectedIndex = 0;
                tbPassword.Text = idr["PASSWD"].ToString();
                tbUserName.Text = idr["USERNAME"].ToString();
            }
            else
            {
                rblRestrictAccess.SelectedIndex = 1;
            }

        }
        idr.Close();

    }
    protected void cmdSaveLogin_Click(object sender, EventArgs e)
    {
        lblPasswordMessage.Visible = false;
        bool isAdmDB = true;
        if ((tbPassword.Text != "") && (tbUserName.Text != ""))
        {
            Utils.ExecuteSP(isAdmDB, Server, "st_WDSetPassword", true,
                tbPassword.Text, tbUserName.Text, ddlWebDatabases.SelectedValue);
        }
        else
        {
            lblPasswordMessage.Visible = true;
        }
    }
    protected void cmdCopyCode_Click(object sender, EventArgs e)
    {
        //ComponentArt.Web.UI.TreeViewNode theNode = tvIncludeCode.SelectedNode;
        Telerik.Web.UI.RadTreeNode theNode = RadTVIncludeCode.SelectedNode;
        if (theNode != null)
        {
            bool isAdmDB = true;
            Utils.ExecuteSP(isAdmDB, Server, "st_WDSetIncludeCode",
                ddlWebDatabases.SelectedValue, theNode.Attributes["ATTRIBUTE_ID"]);
            ddlWebDatabases_SelectedIndexChanged(sender, e);
        }
    }
    protected void cmdDeleteCode_Click(object sender, EventArgs e)
    {
        //ComponentArt.Web.UI.TreeViewNode theNode = tvIncludeCode.SelectedNode;
        //Telerik.Web.UI.RadTreeNode theNode = RadTVIncludeCode.SelectedNode;
            bool isAdmDB = true;
            Utils.ExecuteSP(isAdmDB, Server, "st_WDDeleteIncludeCode",
                ddlWebDatabases.SelectedValue);
            ddlWebDatabases_SelectedIndexChanged(sender, e);
    }
    protected void lbSaveWebDatabaseName_Click(object sender, EventArgs e)
    {
        bool isAdmDB = true;
        Utils.ExecuteSP(isAdmDB, Server, "st_WDSaveName", ddlWebDatabases.SelectedValue, tbWebDatabaseName.Text);

        DataTable dt1 = new DataTable();
        System.Data.DataRow newrow1;
        dt1.Columns.Add(new DataColumn("WEBDB_ID", typeof(string)));
        dt1.Columns.Add(new DataColumn("WEBDB_NAME", typeof(string)));

        newrow1 = dt1.NewRow();
        newrow1["WEBDB_ID"] = "0";
        newrow1["WEBDB_NAME"] = "Select a Web database";
        dt1.Rows.Add(newrow1);

        isAdmDB = true;
        IDataReader idr = Utils.GetReader(isAdmDB, "st_WDGetWebDatabases", lblReviewID.Text);
        while (idr.Read())
        {
            newrow1 = dt1.NewRow();
            newrow1["WEBDB_ID"] = idr["WEBDB_ID"].ToString();
            newrow1["WEBDB_NAME"] = idr["WEBDB_NAME"].ToString();
            dt1.Rows.Add(newrow1);

        }
        idr.Close();
        ddlWebDatabases.DataSource = dt1;
        ddlWebDatabases.DataBind();
        ddlWebDatabases.SelectedItem.Text = tbWebDatabaseName.Text;

    }
    protected void lbSaveLogin_Click(object sender, EventArgs e)
    {
        lblPasswordMessage.Visible = false;
        bool isAdmDB = true;
        if ((tbPassword.Text != "") && (tbUserName.Text != ""))
        {
            Utils.ExecuteSP(isAdmDB, Server, "st_WDSetPassword", true,
                tbPassword.Text, tbUserName.Text, ddlWebDatabases.SelectedValue);
        }
        else
        {
            lblPasswordMessage.Visible = true;
        }
    }
    protected void cbAdminEdit_CheckedChanged(object sender, EventArgs e)
    {
        bool isAdmDB = true;
        Utils.ExecuteSP(isAdmDB, Server, "st_WDSetAdminEditIntro",
                cbAdminEdit.Checked, ddlWebDatabases.SelectedValue);
        Utils.SetSessionString("DescriptionAdminEdit", cbAdminEdit.Checked.ToString());
    }
    protected void lbRetrieve_Click(object sender, EventArgs e)
    {
        bool isAdmDB = true;
        pnlEditShareable.Visible = true;
        IDataReader idr = Utils.GetReader(isAdmDB, "st_ReviewDetails",
            tbReviewID.Text);
        if (idr.Read())
        {
            lblReviewName.Text = idr["REVIEW_NAME"].ToString();

        }
        idr.Close();
        lblReviewID.Text = tbReviewID.Text;

        DataTable dt1 = new DataTable();
        System.Data.DataRow newrow1;
        dt1.Columns.Add(new DataColumn("WEBDB_ID", typeof(string)));
        dt1.Columns.Add(new DataColumn("WEBDB_NAME", typeof(string)));

        newrow1 = dt1.NewRow();
        newrow1["WEBDB_ID"] = "0";
        newrow1["WEBDB_NAME"] = "Select or create a web database";
        dt1.Rows.Add(newrow1);

        isAdmDB = true;
        idr = Utils.GetReader(isAdmDB, "st_WDGetWebDatabases", tbReviewID.Text);
        while (idr.Read())
        {
            newrow1 = dt1.NewRow();
            newrow1["WEBDB_ID"] = idr["WEBDB_ID"].ToString();
            newrow1["WEBDB_NAME"] = idr["WEBDB_NAME"].ToString();
            dt1.Rows.Add(newrow1);

        }
        idr.Close();
        ddlWebDatabases.DataSource = dt1;
        ddlWebDatabases.DataBind();

        pnlIntroText.Visible = false;
        pnlChooseCodes.Visible = false;
    }
    protected void cbShowCoding_CheckedChanged(object sender, EventArgs e)
    {
        
        bool isAdmDB = true;
        if (cbShowCoding.Checked == true)
        {
            Utils.ExecuteSP(isAdmDB, Server, "st_ShowCodingStatus", 
                ddlWebDatabases.SelectedValue, "True");
        }
        else
        {
            Utils.ExecuteSP(isAdmDB, Server, "st_ShowCodingStatus", 
                ddlWebDatabases.SelectedValue, "False");
        }
    }
    protected void cbDisplaySavedCrosstabs_CheckedChanged(object sender, EventArgs e)
    {
        if (cbDisplaySavedCrosstabs.Checked == true)
        {
            bool isAdmDB = true;
            Utils.ExecuteSP(isAdmDB, Server, "st_WDDisplaySavedCrosstabs",
                ddlWebDatabases.SelectedValue, "True");
        }
        else
        {
            bool isAdmDB = true;
            Utils.ExecuteSP(isAdmDB, Server, "st_WDDisplaySavedCrosstabs",
                ddlWebDatabases.SelectedValue, "False");
        }
    }
    protected void cbSaveCrosstabs_CheckedChanged(object sender, EventArgs e)
    {
        if (cbSaveCrosstabs.Checked == true)
        {
            bool isAdmDB = true;
            Utils.ExecuteSP(isAdmDB, Server, "st_WDSaveCrosstabs",
                ddlWebDatabases.SelectedValue, "True");
        }
        else
        {
            bool isAdmDB = true;
            Utils.ExecuteSP(isAdmDB, Server, "st_WDSaveCrosstabs",
                ddlWebDatabases.SelectedValue, "False");
        }
    }

    protected void RadTVReviewGuidelines_NodeCollapse(object sender, RadTreeNodeEventArgs e)
    {

    }
    protected void RadTVReviewGuidelines_NodeExpand(object sender, RadTreeNodeEventArgs e)
    {
        PopulateNodeOnDemandRadTVReviewGuidelines(e, TreeNodeExpandMode.ServerSideCallBack, lblReviewID.Text);
    }
    protected void RadTVWebDatabase_NodeCollapse(object sender, RadTreeNodeEventArgs e)
    {

    }
    protected void RadTVWebDatabase_NodeExpand(object sender, RadTreeNodeEventArgs e)
    {
        PopulateNodeOnDemandRadTVWebDatabase(e, TreeNodeExpandMode.ServerSideCallBack, ddlWebDatabases.SelectedValue);
    }
    protected void RadTVIncludeCode_NodeCollapse(object sender, RadTreeNodeEventArgs e)
    {

    }
    protected void RadTVIncludeCode_NodeExpand(object sender, RadTreeNodeEventArgs e)
    {
        PopulateNodeOnDemandRadTVIncludeCode(e, TreeNodeExpandMode.ServerSideCallBack, lblReviewID.Text);
    }

    private static void PopulateNodeOnDemandRadTVReviewGuidelines(RadTreeNodeEventArgs e, TreeNodeExpandMode expandMode, string reviewID)
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
                //newNode.ToolTip = idr["SET_ID"].ToString();
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

    private static void PopulateNodeOnDemandRadTVIncludeCode(RadTreeNodeEventArgs e, TreeNodeExpandMode expandMode, string reviewID)
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
                //newNode.ToolTip = idr["SET_ID"].ToString();
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
                            //newNode.CssClass = "NoNodeSelect";
                        }
                        else
                        {
                            newNode.Enabled = true;
                            //newNode.CssClass = "NoNodeSelect";
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


    private static void PopulateNodeOnDemandRadTVWebDatabase(RadTreeNodeEventArgs e, TreeNodeExpandMode expandMode, string reviewID)
    {

        bool isWebDB = true;
        IDataReader idr = null;
        IDataReader idr1 = null;
        using (System.Data.SqlClient.SqlConnection myConnection = new System.Data.SqlClient.SqlConnection())
        {
            string test = e.Node.Attributes["ATTRIBUTE_SET_ID"].ToString();
            idr = Utils.GetReader1(isWebDB, "st_WDAttrDataGetFromWD", myConnection,
                e.Node.Attributes["ATTRIBUTE_ID"], e.Node.Attributes["ATTRIBUTE_SET_ID"].ToString(), 
                Utils.GetSessionString("WebDatabaseID"));
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
                //newNode.ToolTip = idr["SET_ID"].ToString();
                newNode.ToolTip = idr["ATTRIBUTE_NAME"].ToString();
                newNode.Attributes["IS_ROOT"] = "No";
                using (System.Data.SqlClient.SqlConnection myConnection1 = new System.Data.SqlClient.SqlConnection())
                {
                    idr1 = Utils.GetReader1(isWebDB, "st_WDAttrCountChildrenFromWD", myConnection1,
                        idr["ATTRIBUTE_ID"].ToString(), idr["ATTRIBUTE_SET_ID"].ToString(),
                        idr["PARENT_ATTRIBUTE_ID"].ToString());

                    if (idr1.Read())
                    {
                        string test2 = idr1["NUM_CHILDREN"].ToString();
                        if (idr1["NUM_CHILDREN"].ToString() != "0")
                        {
                            newNode.Enabled = true;
                            newNode.ExpandMode = expandMode;
                            //newNode.CssClass = "NoNodeSelect";
                        }
                        else
                        {
                            newNode.Enabled = true;
                            //newNode.CssClass = "NoNodeSelect";
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

    protected void RadTVWebDatabase_NodeEdit(object sender, RadTreeNodeEditEventArgs e)
    {
        bool isAdmDB = true;
        if (e.Node.Text != "")
        {
            string attr1 = e.Node.Attributes["ATTRIBUTE_SET_ID"];
            if (e.Node.Attributes["ATTRIBUTE_SET_ID"] == "")
                attr1 = e.Node.Attributes["ATTRIBUTE_ID"];
            
            Utils.ExecuteSP(isAdmDB, Server, "st_WDRenameAttribute", e.Node.Attributes["ATTRIBUTE_SET_ID"],
                attr1, e.Text, e.Node.Attributes["SET_ID"]);
            // level = 0 e.Node.Value = attribute_set_id (i.e. ''), e.Node.ID = attribute_id, e.Node.ToolTip = set_id
            // level > 0 e.Node.Value = attribute_set_id,           e.Node.ID = attribute_set_id, e.Node.ToolTip = set_id
            e.Node.Text = e.Text;
        }
    }
}