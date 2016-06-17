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



public partial class Newsletter : System.Web.UI.Page
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
                        lbl.Text = "EPPI-Reviewer 4 newsletter";
                    }


                    Telerik.Web.UI.RadTabStrip radTs = (Telerik.Web.UI.RadTabStrip)Master.FindControl("rtsMenu");
                    if (radTs != null)
                    {
                        radTs.SelectedIndex = 4;
                        radTs.Tabs[4].Tabs[6].Selected = true;
                        //radTs.Tabs[3].Tabs[2].Width = 670;
                    }
                    System.Web.UI.WebControls.Label lbl1 = (Label)Master.FindControl("lblHeadingText");
                    if (lbl1 != null)
                    {
                        lbl1.Text = "EPPI-Reviewer 4 newsletter";
                    }


                    cmdUpload.Attributes.Add("onclick", "if (confirm('Uploading will overwrite the exising newsletter for the selected year and month. Do you wish to continue?') == false) return false;");
                    cmdEmailNewsletter.Attributes.Add("onclick", "if (confirm('Are you absolutely sure that you wish to send out the newsletter!? Are you sure you are sending the correct one?') == false) return false;");

                    buildNewsletterGrid();

                    updateMailingStatus();
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

    private void buildNewsletterGrid()
    {        
        DataTable dt1 = new DataTable();
        System.Data.DataRow newrow1;

        dt1.Columns.Add(new DataColumn("NEWSLETTER_ID", typeof(string)));
        dt1.Columns.Add(new DataColumn("MONTH", typeof(string)));
        dt1.Columns.Add(new DataColumn("YEAR", typeof(string)));
        dt1.Columns.Add(new DataColumn("NUMBER_SENT", typeof(string)));

        bool isAdmDB = true;
        IDataReader idr = Utils.GetReader(isAdmDB, "st_NewslettersGet");
        while (idr.Read())
        {
            newrow1 = dt1.NewRow();
            newrow1["NEWSLETTER_ID"] = idr["NEWSLETTER_ID"].ToString();
            switch (idr["MONTH"].ToString())
            {
                case "1":
                    newrow1["MONTH"] = "January";
                    break;
                case "2":
                    newrow1["MONTH"] = "February";
                    break;
                case "3":
                    newrow1["MONTH"] = "March";
                    break;
                case "4":
                    newrow1["MONTH"] = "April";
                    break;
                case "5":
                    newrow1["MONTH"] = "May";
                    break;
                case "6":
                    newrow1["MONTH"] = "June";
                    break;
                case "7":
                    newrow1["MONTH"] = "July";
                    break;
                case "8":
                    newrow1["MONTH"] = "August";
                    break;
                case "9":
                    newrow1["MONTH"] = "September";
                    break;
                case "10":
                    newrow1["MONTH"] = "October";
                    break;
                case "11":
                    newrow1["MONTH"] = "November";
                    break;
                case "12":
                    newrow1["MONTH"] = "December";
                    break;
                default:
                    break;
            }
            newrow1["YEAR"] = idr["YEAR"].ToString();
            newrow1["NUMBER_SENT"] = idr["NUMBER_SENT"].ToString();
            dt1.Rows.Add(newrow1);
        }
        idr.Close();

        gvNewsletters.DataSource = dt1;
        gvNewsletters.DataBind();
    }



    protected void cmdUpload_Click(object sender, EventArgs e)
    {
        lblUploadMessage.Visible = false;
        lblUploadMessage.Text = "Please select a year and month";
        lblViewMessage.Visible = false;
        lblViewMessage.Text = "Please select a year and month";
        
        if ((ddlMonth.SelectedIndex == 0) || (ddlYear.SelectedIndex == 0))
        {
            lblUploadMessage.Visible = true;
        }
        else
        {
            if (fDocument.PostedFile.FileName.ToString() != "")
            {
                // put file into string
                string myString = "";
                StringBuilder sb = new StringBuilder();
                System.IO.StreamReader s = new System.IO.StreamReader(fDocument.PostedFile.InputStream, Encoding.Default);
                try
                {
                    do
                    {
                        myString = s.ReadLine();
                        sb = sb.Append(myString);
                    }
                    while (s.Peek() != -1);
                }
                catch
                {
                    myString = "";
                }
                finally
                {
                    s.Close();
                }

                string test = sb.ToString();
                bool isAdmDB = true;
                //Utils.ExecuteSP(isAdmDB, Server, "st_NewsletterUpload", sb.ToString(), ddlYear.SelectedValue, ddlMonth.SelectedValue);

                SqlParameter[] paramList01 = new SqlParameter[4];
                paramList01[0] = new SqlParameter("@NEWSLETTER", SqlDbType.NVarChar, 50000, ParameterDirection.Input,
                    true, 0, 0, null, DataRowVersion.Default, sb.ToString());
                paramList01[1] = new SqlParameter("@YEAR", SqlDbType.Int, 8, ParameterDirection.Input,
                    true, 0, 0, null, DataRowVersion.Default, ddlYear.SelectedValue);
                paramList01[2] = new SqlParameter("@MONTH", SqlDbType.Int, 8, ParameterDirection.Input,
                    true, 0, 0, null, DataRowVersion.Default, ddlMonth.SelectedValue);
                paramList01[3] = new SqlParameter("@RESULT", SqlDbType.NVarChar, 50, ParameterDirection.Output,
                    true, 0, 0, null, DataRowVersion.Default, "");
                Utils.ExecuteSPWithReturnValues(isAdmDB, Server, "st_NewsletterUpload", paramList01);
                if (paramList01[3].Value.ToString() != "0")
                {
                    lblUploadMessage.Visible = true;
                    lblUploadMessage.Text = paramList01[2].Value.ToString();
                }
                else
                {
                    lblUploadMessage.Visible = true;
                    lblUploadMessage.Text = "File uploaded";
                    buildNewsletterGrid();
                    updateMailingStatus();
                }
            }
            else
            {
                lblUploadMessage.Visible = false;
                lblUploadMessage.Text = "Cannot find the file to upload";
            }
        }
    }
    protected void cmdView_Click(object sender, EventArgs e)
    {
        lblUploadMessage.Visible = false;
        lblUploadMessage.Text = "Please select a year and month";
        lblViewMessage.Visible = false;
        lblViewMessage.Text = "Please select a year and month";
        
        if ((ddlMonth.SelectedIndex == 0) || (ddlYear.SelectedIndex == 0))
        {
            lblViewMessage.Visible = true;
        }
        else
        {
            bool isAdmDB = true;
            string newsletter = "";
            IDataReader idr = Utils.GetReader(isAdmDB, "st_NewsletterGet", ddlMonth.SelectedValue, ddlYear.SelectedValue);
            if (idr.Read())
            {
                newsletter = idr["NEWSLETTER"].ToString();
            }
            idr.Close();

            /*
            PlaceHolder1.Controls.Clear();
            Label lb = new Label();
            lb.Text = newsletter;
            PlaceHolder1.Controls.Add(lb);

            dlgNewsletter.IsShowing = true;
            */
        }
    }

    protected void cmdEmailNewsletter_Click(object sender, EventArgs e)
    {
        DataTable dt = new DataTable();
        System.Data.DataRow newrow;

        dt.Columns.Add(new DataColumn("CONTACT_ID", typeof(string)));
        dt.Columns.Add(new DataColumn("CONTACT_NAME", typeof(string)));
        dt.Columns.Add(new DataColumn("EMAIL", typeof(string)));

        bool testSend = false;
        bool isAdmDB = true;
        IDataReader idr = Utils.GetReader(isAdmDB, "st_NewsletterContacts", testSend);
        while (idr.Read())
        {
            newrow = dt.NewRow();
            newrow["CONTACT_ID"] = idr["CONTACT_ID"].ToString();
            newrow["CONTACT_NAME"] = idr["CONTACT_NAME"].ToString();
            newrow["EMAIL"] = idr["EMAIL"].ToString();
            dt.Rows.Add(newrow);
        }
        idr.Close();

        string newsletter = "";
        string month = "";
        string year = "";
        idr = Utils.GetReader(isAdmDB, "st_NewsletterGet", "0"); // zero means most recent
        if (idr.Read())
        {
            newsletter = idr["NEWSLETTER"].ToString();
            switch (idr["MONTH"].ToString())
            {
                case "1":
                    month = "January";
                    break;
                case "2":
                    month = "February";
                    break;
                case "3":
                    month = "March";
                    break;
                case "4":
                    month = "April";
                    break;
                case "5":
                    month = "May";
                    break;
                case "6":
                    month = "June";
                    break;
                case "7":
                    month = "July";
                    break;
                case "8":
                    month = "August";
                    break;
                case "9":
                    month = "September";
                    break;
                case "10":
                    month = "October";
                    break;
                case "11":
                    month = "November";
                    break;
                case "12":
                    month = "December";
                    break;
                default:
                    break;
            }
            year = idr["YEAR"].ToString();

        }
        idr.Close();

        string subject = "EPPI-Reviewer 4 newsletter " + month + " " + year;

        string sendResult = "";
   
        if (newsletter != "")
        {
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                sendResult = Utils.EmailNewsletter(dt.Rows[i]["EMAIL"].ToString(), subject, newsletter);
                if (sendResult == "The email was sent successfully")
                {
                    Utils.ExecuteSP(isAdmDB, Server, "st_NewsletterContactUpdate", dt.Rows[i]["CONTACT_ID"].ToString());
                    //Thread.Sleep(5000); //1 second delay between each send
                    Utils.ExecuteSP(isAdmDB, Server, "st_NewsletterDelay", rblDelay.SelectedValue);
                    if (i == int.Parse(rblBatch.SelectedValue) - 1)
                    {
                        i = dt.Rows.Count;
                    }
                }
                else
                {
                    lblEmailMessage.Visible = true;
                    lblEmailMessage.Text = "There was a problem in sending the newsletter to " + dt.Rows[i]["EMAIL"].ToString();
                    i = dt.Rows.Count;
                }
            }
            if (sendResult == "The email was sent successfully")
            {
                buildNewsletterGrid();
                updateMailingStatus();             
            }
        }
        else
        {
            lblEmailMessage.Visible = true;
            lblEmailMessage.Text = "The newsletter is blank. No emails have been sent";
        }
    }
    /*
    protected void gvNewsletters_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        bool isAdmDB = true;
        int index = Convert.ToInt32(e.CommandArgument);
        string newsletterID = (string)gvNewsletters.DataKeys[index].Value;
        switch (e.CommandName)
        {
            case "VIEW":
                string newsletter = "";
                IDataReader idr = Utils.GetReader(isAdmDB, "st_NewsletterGet", newsletterID);
                if (idr.Read())
                {
                    newsletter = idr["NEWSLETTER"].ToString();
                }
                idr.Close();

                
                PlaceHolder1.Controls.Clear();
                Label lb = new Label();
                lb.Text = newsletter;
                PlaceHolder1.Controls.Add(lb);

                dlgNewsletter.IsShowing = true;
                //Server.Transfer("Newsletter.aspx");
                
                break;
       
            default:
                break;
        }
    }
    */
    private void updateMailingStatus()
    {
        bool isAdmDB = true;
        IDataReader idr = Utils.GetReader(isAdmDB, "st_NewsletterStatus");
        if (idr.Read())
        {
            lblToBeSent.Text = idr["numToBeSent"].ToString();
            lblNumEligible.Text = idr["numEligible"].ToString();
            lblNumRecipients.Text = idr["numRecipients"].ToString();
        }
        idr.Close();
    }
    protected void ddlYear_SelectedIndexChanged(object sender, EventArgs e)
    {
        if ((ddlYear.SelectedIndex > 0) && (ddlMonth.SelectedIndex > 0))
        {
            cmdUpload.Enabled = true;
        }
        else
        {
            cmdUpload.Enabled = false;
        }
    }
    protected void ddlMonth_SelectedIndexChanged(object sender, EventArgs e)
    {
        if ((ddlYear.SelectedIndex > 0) && (ddlMonth.SelectedIndex > 0))
        {
            cmdUpload.Enabled = true;
        }
        else
        {
            cmdUpload.Enabled = false;
        }
    }
    protected void cmdTest_Click(object sender, EventArgs e)
    {
        
        DataTable dt = new DataTable();
        System.Data.DataRow newrow;

        dt.Columns.Add(new DataColumn("CONTACT_ID", typeof(string)));
        dt.Columns.Add(new DataColumn("CONTACT_NAME", typeof(string)));
        dt.Columns.Add(new DataColumn("EMAIL", typeof(string)));

        bool testSend = true;
        bool isAdmDB = true;
        IDataReader idr = Utils.GetReader(isAdmDB, "st_NewsletterContacts", testSend);
        while (idr.Read())
        {
            newrow = dt.NewRow();
            newrow["CONTACT_ID"] = idr["CONTACT_ID"].ToString();
            newrow["CONTACT_NAME"] = idr["CONTACT_NAME"].ToString();
            newrow["EMAIL"] = idr["EMAIL"].ToString();
            dt.Rows.Add(newrow);
        }
        idr.Close();

        string newsletter = "";
        string month = "";
        string year = "";
        idr = Utils.GetReader(isAdmDB, "st_NewsletterGet", "0"); // zero means most recent
        if (idr.Read())
        {
            newsletter = idr["NEWSLETTER"].ToString();
            switch (idr["MONTH"].ToString())
            {
                case "1":
                    month = "January";
                    break;
                case "2":
                    month = "February";
                    break;
                case "3":
                    month = "March";
                    break;
                case "4":
                    month = "April";
                    break;
                case "5":
                    month = "May";
                    break;
                case "6":
                    month = "June";
                    break;
                case "7":
                    month = "July";
                    break;
                case "8":
                    month = "August";
                    break;
                case "9":
                    month = "September";
                    break;
                case "10":
                    month = "October";
                    break;
                case "11":
                    month = "November";
                    break;
                case "12":
                    month = "December";
                    break;
                default:
                    break;
            }
            year = idr["YEAR"].ToString();

        }
        idr.Close();

        string subject = "EPPI-Reviewer 4 newsletter " + month + " " + year;

        string sendResult = "";

        if (newsletter != "")
        {
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                sendResult = Utils.EmailNewsletter(dt.Rows[i]["EMAIL"].ToString(), subject, newsletter);
                if (sendResult == "The email was sent successfully")
                {
                    Utils.ExecuteSP(isAdmDB, Server, "st_NewsletterContactUpdate", dt.Rows[i]["CONTACT_ID"].ToString());
                    //Thread.Sleep(5000); //1 second delay between each send
                    Utils.ExecuteSP(isAdmDB, Server, "st_NewsletterDelay", rblDelay.SelectedValue);
                    if (i == int.Parse(rblBatch.SelectedValue) - 1)
                    {
                        i = dt.Rows.Count;
                    }
                }
                else
                {
                    lblEmailMessage.Visible = true;
                    lblEmailMessage.Text = "There was a problem in sending the newsletter to " + dt.Rows[i]["EMAIL"].ToString();
                    i = dt.Rows.Count;
                }
            }
            if (sendResult == "The email was sent successfully")
            {
                buildNewsletterGrid();
                updateMailingStatus();
            }
        }
        else
        {
            lblEmailMessage.Visible = true;
            lblEmailMessage.Text = "The newsletter is blank. No emails have been sent";
        }
        
    }
    protected void gvNewsletters_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            System.Web.UI.WebControls.HyperLink hl = (System.Web.UI.WebControls.HyperLink)e.Row.FindControl("hlViewNewsletter");
            hl.Attributes.Add("href", @"javascript:openViewNewsletter('" + DataBinder.Eval(e.Row.DataItem, "NEWSLETTER_ID").ToString() + "')");
        }
    }
}