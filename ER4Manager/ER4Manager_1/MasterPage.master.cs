using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class MasterPage : System.Web.UI.MasterPage
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            lblLoggedInAs.Text = Utils.GetSessionString("ContactName");

            if (Utils.GetSessionString("PurchasesEnabled") == "False")
            {
                rtsMenu.Tabs[1].Visible = false;
            }
            else
            {
                if (rtsMenu.Tabs[9].Selected != true) // i.e. if there isn't an error and logout isn't selected
                {
                    rtsMenu.Tabs[1].Visible = true;
                }
            }

            if (Utils.GetSessionString("IsAdm") == "True")
            {
                rtsMenu.Tabs[0].Visible = true;
                rtsMenu.Tabs[1].Visible = true;
                rtsMenu.Tabs[2].Visible = true;
                rtsMenu.Tabs[3].Visible = true;
                rtsMenu.Tabs[4].Visible = true;
                rtsMenu.Tabs[5].Visible = true;
                rtsMenu.Tabs[6].Visible = true;
                rtsMenu.Tabs[7].Visible = false;
                rtsMenu.Tabs[8].Visible = true;
            }

            if (Utils.GetSessionString("IsSiteLicenseAdm") == "1")
            {
                rtsMenu.Tabs[6].Visible = true;
            }

            /* // Removed Organisations from menu on 21/08/2019 as it isn't being used
            if (Utils.GetSessionString("IsOrganisationAdm") == "1")
            {
                rtsMenu.Tabs[7].Visible = false;
            }
            */

            //string test = (Utils.GetSessionString("EnableDataPresenter").ToString());
            if (Utils.GetSessionString("EnableDataPresenter") == "True")
            {
                rtsMenu.Tabs[5].Tabs[1].Visible = true;
            }

            if (Utils.GetSessionString("AccessWDSetup") == "1")
            {
                rtsMenu.Tabs[5].Tabs[1].Visible = true;
            }

            //switch (ContentPlaceHolder1.ID)
            //{
            //    case "RootSummary":
            //        // clear TB_ACCOUNT_EXTENSION
            //        bool isAdmDB = true;
            //        Utils.ExecuteSP(isAdmDB, Server, "st_ContactAccountExtensionClear",
            //            Utils.GetSessionString("Contact_ID"));

            //        break;

            //    default:
            //        break;
            //}

            /*
            if (Session["WebDBID"] != null)
            {
                IDataReader idr = DBAccess.GetReader(Server, "st_REVIEW_WEBDB_SINGLE", Session["WebDBID"]);
                if (idr.Read())
                {
                    lblTitle.Text = idr["WEB_DATABASE"].ToString();
                    if (idr["GROUP_ID"].ToString() != "")
                    {
                        Session["GROUP_ID"] = idr["GROUP_ID"].ToString();
                    }
                    else
                    {
                        Session["REVIEW_ID"] = idr["REVIEW_ID"].ToString();
                    }
                    Session["LIST_ALL_VISIBLE"] = idr["LIST_ALL_VISIBLE"].ToString();
                }
                idr.Close();
                tsMenu.Tabs[0].NavigateUrl = "Intro.aspx?ID=" + Session["WebDBID"].ToString();
            }
            if (Session["report"] != null)
            {
                //tsMenu.Tabs[3].Enabled = true;
            }
            if (Session["ITEM_ID"] != null)
            {
                //tsMenu.Tabs[4].Enabled = true;
            }
            if (Session["QUESTION_IDX"] != null)
            {
                tsMenu.Tabs[2].Tabs[3].Enabled = true;
            }
            if (Session["ReportAttributes"] != null)
            {
                tsMenu.Tabs[2].Tabs[1].Enabled = true;
            }*/
        }
    }
}
