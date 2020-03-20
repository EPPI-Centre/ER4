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
using System.Text;

public partial class AccessSettings : System.Web.UI.Page
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
                        lbl.Text = "Access settings";
                    }

                    Telerik.Web.UI.RadTabStrip radTs = (Telerik.Web.UI.RadTabStrip)Master.FindControl("rtsMenu");
                    if (radTs != null)
                    {
                        radTs.SelectedIndex = 4;
                        radTs.Tabs[4].Tabs[4].Selected = true;
                        //radTs.Tabs[3].Tabs[2].Width = 670;
                    }
                    System.Web.UI.WebControls.Label lbl1 = (Label)Master.FindControl("lblHeadingText");
                    if (lbl1 != null)
                    {
                        lbl1.Text = "Message of the day";
                    }

                    updateManagementControls();

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

    private void updateManagementControls()
    {
        string SQL = "SELECT * from TB_MANAGEMENT_SETTINGS";
        bool isAdmDB = true;
        SqlDataReader sdr = Utils.ReturnReader(SQL, isAdmDB);
        if (sdr.Read())
        {
            rblAccountCreation.SelectedValue = sdr["ENABLE_ACCOUNT_CREATION"].ToString();
            rblPasswordReminder.SelectedValue = sdr["ENABLE_SEND_PASSWORD"].ToString();
            rblPurchase.SelectedValue = sdr["ENABLE_PURCHASES"].ToString();
            rblAdmEnable.SelectedValue = sdr["ADM_ENABLE_ALL"].ToString();
            rblExampleReview.SelectedValue = sdr["ENABLE_EXAMPLE_REVIEW_CREATION"].ToString();
            tbExampleReview.Text = sdr["EXAMPLE_NON_SHAREABLE_REVIEW_ID"].ToString();
            rblExampleReviewCopy.SelectedValue = sdr["ENABLE_EXAMPLE_REVIEW_COPY"].ToString();
            rblDataPresenter.SelectedValue = sdr["ENABLE_DATA_PRESENTER"].ToString();
            rblPriorityScreeningEnableEnabler.SelectedValue = sdr["ENABLE_PRIORITY_SCREENING_ENABLER"].ToString();
            rblEnableShopCredit.SelectedValue = sdr["ENABLE_SHOP_CREDIT"].ToString();
            rblEnableShopDebit.SelectedValue = sdr["ENABLE_SHOP_DEBIT"].ToString();
        }
        sdr.Close();
    }

    protected void rblAccountCreation_SelectedIndexChanged(object sender, EventArgs e)
    {
        bool isAdmDB = true;
        string SQL = "update TB_MANAGEMENT_SETTINGS set ENABLE_ACCOUNT_CREATION = '" +
            rblAccountCreation.SelectedValue + "'";
        Utils.ExecuteQuery(SQL, isAdmDB);
    }
    protected void rblPasswordReminder_SelectedIndexChanged(object sender, EventArgs e)
    {
        bool isAdmDB = true;
        string SQL = "update TB_MANAGEMENT_SETTINGS set ENABLE_SEND_PASSWORD = '" +
               rblPasswordReminder.SelectedValue + "'";
        Utils.ExecuteQuery(SQL, isAdmDB);
    }
    protected void rblPurchase_SelectedIndexChanged(object sender, EventArgs e)
    {
        bool isAdmDB = true;
        string SQL = "update TB_MANAGEMENT_SETTINGS set ENABLE_PURCHASES = '" +
            rblPurchase.SelectedValue + "'";
        Utils.ExecuteQuery(SQL, isAdmDB);
    }
    protected void rblAdmEnable_SelectedIndexChanged(object sender, EventArgs e)
    {
        bool isAdmDB = true;
        string SQL = "update TB_MANAGEMENT_SETTINGS set ADM_ENABLE_ALL = '" +
            rblAdmEnable.SelectedValue + "'";
        Utils.ExecuteQuery(SQL, isAdmDB);
    }
    protected void rblExampleReview_SelectedIndexChanged(object sender, EventArgs e)
    {
        bool isAdmDB = true;
        string SQL = "update TB_MANAGEMENT_SETTINGS set ENABLE_EXAMPLE_REVIEW_CREATION = '" +
            rblExampleReview.SelectedValue + "'";
        Utils.ExecuteQuery(SQL, isAdmDB);
    }
    protected void rblExampleReviewCopy_SelectedIndexChanged(object sender, EventArgs e)
    {
        bool isAdmDB = true;
        string SQL = "update TB_MANAGEMENT_SETTINGS set ENABLE_EXAMPLE_REVIEW_COPY = '" +
            rblExampleReviewCopy.SelectedValue + "'";
        Utils.ExecuteQuery(SQL, isAdmDB);
        Utils.SetSessionString("EnableExampleReviewCopy", rblExampleReviewCopy.SelectedValue);
    }
    protected void lbSave_Click(object sender, EventArgs e)
    {
        bool isAdmDB = true;
        string SQL = "update TB_MANAGEMENT_SETTINGS set EXAMPLE_NON_SHAREABLE_REVIEW_ID = '" +
            tbExampleReview.Text + "'";
        Utils.ExecuteQuery(SQL, isAdmDB);
    }
    protected void rblDataPresenter_SelectedIndexChanged(object sender, EventArgs e)
    {
        bool isAdmDB = true;
        string SQL = "update TB_MANAGEMENT_SETTINGS set ENABLE_DATA_PRESENTER = '" +
            rblDataPresenter.SelectedValue + "'";
        Utils.ExecuteQuery(SQL, isAdmDB);
    }
    protected void lbEnterAs_Click(object sender, EventArgs e)
    {
        Utils.SetSessionString("Contact_ID", tbContactID.Text);
        Utils.SetSessionString("IsAdm", "False");
        Utils.SetSessionString("IsSiteLicenseAdm", "0");

        bool isAdmDB = true;
        IDataReader idr = Utils.GetReader(isAdmDB, "st_ContactIsSiteLicenseAdm", tbContactID.Text);
        if (idr.Read())
        {
            Utils.SetSessionString("IsSiteLicenseAdm", "1");
        }
        idr.Close();
        Server.Transfer("Summary.aspx");
    }

    protected void rblPriorityScreeningEnableEnabler_SelectedIndexChanged(object sender, EventArgs e)
    {
        bool isAdmDB = true;
        string SQL = "update TB_MANAGEMENT_SETTINGS set ENABLE_PRIORITY_SCREENING_ENABLER = '" +
            rblPriorityScreeningEnableEnabler.SelectedValue + "'";
        Utils.ExecuteQuery(SQL, isAdmDB);
        Utils.SetSessionString("EnablePSEnabler", rblPriorityScreeningEnableEnabler.SelectedValue);
    }

    protected void rblEnableShopCredit_SelectedIndexChanged(object sender, EventArgs e)
    {
        bool isAdmDB = true;
        string SQL = "update TB_MANAGEMENT_SETTINGS set ENABLE_SHOP_CREDIT = '" +
            rblEnableShopCredit.SelectedValue + "'";
        Utils.ExecuteQuery(SQL, isAdmDB);
        Utils.SetSessionString("EnableShopCredit", rblEnableShopCredit.SelectedValue);
    }

    protected void rblEnableShopDebit_SelectedIndexChanged(object sender, EventArgs e)
    {
        bool isAdmDB = true;
        string SQL = "update TB_MANAGEMENT_SETTINGS set ENABLE_SHOP_DEBIT = '" +
            rblEnableShopDebit.SelectedValue + "'";
        Utils.ExecuteQuery(SQL, isAdmDB);
        Utils.SetSessionString("EnableShopDebit", rblEnableShopCredit.SelectedValue);
    }
}