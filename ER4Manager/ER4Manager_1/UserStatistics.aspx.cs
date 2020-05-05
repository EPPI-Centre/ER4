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

public partial class UserStatistics : System.Web.UI.Page
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
                        lbl.Text = "User statistics";
                    }


                    Telerik.Web.UI.RadTabStrip radTs = (Telerik.Web.UI.RadTabStrip)Master.FindControl("rtsMenu");
                    if (radTs != null)
                    {
                        radTs.SelectedIndex = 4;
                        radTs.Tabs[4].Tabs[1].Selected = true;
                        radTs.Tabs[1].Tabs[1].Width = 550;
                    }
                    System.Web.UI.WebControls.Label lbl1 = (Label)Master.FindControl("lblHeadingText");
                    if (lbl1 != null)
                    {
                        lbl1.Text = "User statistics";
                    }

                    string SQL = "select count(distinct CONTACT_ID) as NUMBER_LOGINS from TB_LOGON_TICKET " +
                        "where CREATED > '2010-07-01 00:00:01'";
                    bool isAdmDB = true;
                    SqlDataReader sdr = Utils.ReturnReader(SQL, isAdmDB);
                    if (sdr.Read())  // already exists
                    {
                        lblNumActiveUsers.Text = sdr["NUMBER_LOGINS"].ToString();
                    }
                    sdr.Close();
                    SQL = "select count(distinct CONTACT_ID) as NUMBER_ACCOUNTS from TB_CONTACT " +
                        "where EXPIRY_DATE > '2010-03-20 00:00:01'";
                    isAdmDB = false;
                    sdr = Utils.ReturnReader(SQL, isAdmDB);
                    if (sdr.Read())  // already exists
                    {
                        lblAccountsCreated.Text = sdr["NUMBER_ACCOUNTS"].ToString();
                    }
                    sdr.Close();

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

    
    private void buildGridNew()
    {
        DataTable dt = new DataTable();
        System.Data.DataRow newrow;

        dt.Columns.Add(new DataColumn("CONTACT_ID", typeof(int)));
        dt.Columns.Add(new DataColumn("CONTACT_NAME", typeof(string)));
        dt.Columns.Add(new DataColumn("NUMBER_LOGINS", typeof(int)));
        dt.Columns.Add(new DataColumn("HOURS", typeof(int)));
        dt.Columns.Add(new DataColumn("DAYS_SINCE_CREATION", typeof(int)));
        dt.Columns.Add(new DataColumn("DAYS_REMAINING", typeof(int)));
        dt.Columns.Add(new DataColumn("LAST_ACCESS", typeof(string)));
        string SQL = "declare @tb_user_stats table " +
                    "(contact_id int,  contact_name nvarchar(255), number_logins int, " +
                    "days_since_creation int,  days_remaining int, last_access datetime, number_hours int)" +

                    "insert into @tb_user_stats (contact_id, contact_name, number_logins, days_since_creation, " +
                    "days_remaining, last_access, number_hours) ";

        if (cbWithLogin.Checked == false)
        {
            SQL += "SELECT c.[CONTACT_ID],c.CONTACT_NAME, COUNT(lt.TICKET_GUID) AS NUMBER_LOGINS, " +
                "DATEDIFF (day, c.DATE_CREATED, GETDATE()) as DAYS_SINCE_CREATION, " +
                "DATEDIFF (DAY, GETDATE(), c.EXPIRY_DATE) as DAYS_REMAINING, " +
                "MAX(lt.CREATED)as LAST_ACCESS, " +
                "SUM(DATEDIFF(HOUR,lt.CREATED ,lt.LAST_RENEWED )) " +
                /*"FROM [ReviewerAdmin].[dbo].[TB_LOGON_TICKET] lt " +*/
                "FROM Reviewer.dbo.TB_CONTACT c " +
                "left join Revieweradmin.dbo.TB_LOGON_TICKET lt " +
                "on lt.CONTACT_ID = c.CONTACT_ID " +
                "where c.EXPIRY_DATE > '2010-03-20 00:00:01' ";
            if (cbRemaining.Checked == true)
            {
                SQL += "and DATEDIFF (DAY, GETDATE(), c.EXPIRY_DATE) > 0 ";
            }
            SQL += "and ((c.CONTACT_ID like '%" + tbFilter.Text + "%') OR " +
                    "(c.CONTACT_NAME like '%" + tbFilter.Text + "%')) ";
            SQL += "group by c.CONTACT_ID, c.CONTACT_NAME, c.DATE_CREATED, c.EXPIRY_DATE  ";
        }
        else
        {
            SQL += "SELECT c.[CONTACT_ID],c.CONTACT_NAME, COUNT(lt.TICKET_GUID) AS NUMBER_LOGINS, " +
                "DATEDIFF (day, c.DATE_CREATED, GETDATE()) as DAYS_SINCE_CREATION, " +
                "DATEDIFF (DAY, GETDATE(), c.EXPIRY_DATE) as DAYS_REMAINING, " +
                "MAX(CREATED)as LAST_ACCESS, " +
                "SUM(DATEDIFF(HOUR,lt.CREATED ,lt.LAST_RENEWED )) FROM [ReviewerAdmin].[dbo].[TB_LOGON_TICKET] lt " +
                "Inner Join Reviewer.dbo.TB_CONTACT c " +
                "on lt.CONTACT_ID = c.CONTACT_ID " +
                "Where CREATED > DATEADD(DD, -" + ddlTimePeriod.SelectedValue + ", GETDATE()) " +
                "and CREATED > '2010-07-01 00:00:01' " +
                "and c.EXPIRY_DATE > '2010-03-20 00:00:01' ";
            if (cbRemaining.Checked == true)
            {
                SQL += "and DATEDIFF (DAY, GETDATE(), c.EXPIRY_DATE) > 0 ";
            }
            SQL += "and ((c.CONTACT_ID like '%" + tbFilter.Text + "%') OR " +
                   "(c.CONTACT_NAME like '%" + tbFilter.Text + "%')) ";
            SQL += "group by c.CONTACT_ID, c.CONTACT_NAME, c.DATE_CREATED, c.EXPIRY_DATE  ";
        }

        SQL += "select * from @tb_user_stats";

        bool isAdmDB = false;
        SqlDataReader sdr = Utils.ReturnReader(SQL, isAdmDB);
        while (sdr.Read())
        {
            newrow = dt.NewRow();
            newrow["CONTACT_ID"] = sdr["contact_id"].ToString();
            newrow["CONTACT_NAME"] = sdr["contact_name"].ToString();
            newrow["NUMBER_LOGINS"] = sdr["number_logins"].ToString();
            if (sdr["number_hours"].ToString() == "")
                newrow["HOURS"] = 0;
            else
                newrow["HOURS"] = sdr["number_hours"].ToString();
            newrow["DAYS_SINCE_CREATION"] = sdr["days_since_creation"].ToString();
            newrow["DAYS_REMAINING"] = sdr["days_remaining"].ToString();
            newrow["LAST_ACCESS"] = sdr["last_access"].ToString();
            dt.Rows.Add(newrow);
        }
        sdr.Close();
        
        
        
        
        
        
        /*
        DataTable dt = new DataTable();
        System.Data.DataRow newrow;

        dt.Columns.Add(new DataColumn("CONTACT_ID", typeof(string)));
        dt.Columns.Add(new DataColumn("CONTACT_NAME", typeof(string)));
        dt.Columns.Add(new DataColumn("NUMBER_LOGINS", typeof(Int16)));
        dt.Columns.Add(new DataColumn("HOURS", typeof(string)));
        dt.Columns.Add(new DataColumn("DAYS_SINCE_CREATION", typeof(string)));
        dt.Columns.Add(new DataColumn("DAYS_REMAINING", typeof(string)));
        dt.Columns.Add(new DataColumn("LAST_ACCESS", typeof(string)));
        string SQL = "";
        string SQL2 = "";
        if (cbWithLogin.Checked == false)
        {
            SQL = "SELECT c.[CONTACT_ID],c.CONTACT_NAME, COUNT(lt.TICKET_GUID) AS NUMBER_LOGINS, " +
                "DATEDIFF (day, c.DATE_CREATED, GETDATE()) as DAYS_SINCE_CREATION, " +
                "DATEDIFF (DAY, GETDATE(), c.EXPIRY_DATE) as DAYS_REMAINING, " +
                "MAX(lt.CREATED)as LAST_ACCESS " +
                "FROM Reviewer.dbo.TB_CONTACT c " +
                "left join Revieweradmin.dbo.TB_LOGON_TICKET lt " +
                "on lt.CONTACT_ID = c.CONTACT_ID " +
                "where c.EXPIRY_DATE > '2010-03-20 00:00:01' ";
            if (cbRemaining.Checked == true)
            {
                SQL += "and DATEDIFF (DAY, GETDATE(), c.EXPIRY_DATE) > 0 ";
            }
            SQL += "group by c.CONTACT_ID, c.CONTACT_NAME, c.DATE_CREATED, c.EXPIRY_DATE  ";
        }
        else
        {
            SQL = "SELECT c.[CONTACT_ID],c.CONTACT_NAME, COUNT(lt.TICKET_GUID) AS NUMBER_LOGINS, " +
                "DATEDIFF (day, c.DATE_CREATED, GETDATE()) as DAYS_SINCE_CREATION, " +
                "DATEDIFF (DAY, GETDATE(), c.EXPIRY_DATE) as DAYS_REMAINING, " +
                "MAX(CREATED)as LAST_ACCESS FROM [ReviewerAdmin].[dbo].[TB_LOGON_TICKET] lt " +
                "Inner Join Reviewer.dbo.TB_CONTACT c " +
                "on lt.CONTACT_ID = c.CONTACT_ID " +
                "Where CREATED > DATEADD(DD, -" + ddlTimePeriod.SelectedValue + ", GETDATE()) " +
                "and CREATED > '2010-07-01 00:00:01' " +
                "and c.EXPIRY_DATE > '2010-03-20 00:00:01' ";
            if (cbRemaining.Checked == true)
            {
                SQL += "and DATEDIFF (DAY, GETDATE(), c.EXPIRY_DATE) > 0 ";
            }
            SQL += "group by c.CONTACT_ID, c.CONTACT_NAME, c.DATE_CREATED, c.EXPIRY_DATE  ";
        }

        bool isAdmDB = false;
        bool isAdmDB2 = true;
        SqlDataReader sdr = Utils.ReturnReader(SQL, isAdmDB);
        SqlDataReader sdr2;
        while (sdr.Read())
        {
            newrow = dt.NewRow();
            newrow["CONTACT_ID"] = sdr["CONTACT_ID"].ToString();
            newrow["CONTACT_NAME"] = sdr["CONTACT_NAME"].ToString();
            newrow["NUMBER_LOGINS"] = sdr["NUMBER_LOGINS"].ToString();

            SQL2 = "SELECT SUM(DATEDIFF(HOUR,t.CREATED ,t.LAST_RENEWED )) as HOURS " +
            "FROM [TB_LOGON_TICKET] t Inner JOIN Reviewer.dbo.TB_CONTACT c on t.CONTACT_ID = c.CONTACT_ID " +
            "where c.CONTACT_ID = '" + sdr["CONTACT_ID"].ToString() + "'";
            sdr2 = Utils.ReturnReader(SQL2, isAdmDB2);
            if (sdr2.Read())
            {
                newrow["HOURS"] = sdr2["HOURS"].ToString();
            }
            sdr2.Close();

            newrow["DAYS_SINCE_CREATION"] = sdr["DAYS_SINCE_CREATION"].ToString();
            newrow["DAYS_REMAINING"] = sdr["DAYS_REMAINING"].ToString();
            newrow["LAST_ACCESS"] = sdr["LAST_ACCESS"].ToString();
            dt.Rows.Add(newrow);
        }
        sdr.Close();
        */
    }
    
    protected void radGVContacts_NeedDataSource(object sender, Telerik.Web.UI.GridNeedDataSourceEventArgs e)
    {
        DataTable dt = new DataTable();
        System.Data.DataRow newrow;

        dt.Columns.Add(new DataColumn("CONTACT_ID", typeof(int)));
        dt.Columns.Add(new DataColumn("CONTACT_NAME", typeof(string)));
        dt.Columns.Add(new DataColumn("NUMBER_LOGINS", typeof(int)));
        dt.Columns.Add(new DataColumn("HOURS", typeof(int)));
        dt.Columns.Add(new DataColumn("DAYS_SINCE_CREATION", typeof(int)));
        dt.Columns.Add(new DataColumn("DAYS_REMAINING", typeof(int)));
        dt.Columns.Add(new DataColumn("LAST_ACCESS", typeof(string)));
        string SQL = "declare @tb_user_stats table " +
                    "(contact_id int,  contact_name nvarchar(255), number_logins int, " + 
                    "days_since_creation int,  days_remaining int, last_access datetime, number_hours int)" +

                    "insert into @tb_user_stats (contact_id, contact_name, number_logins, days_since_creation, " +
                    "days_remaining, last_access, number_hours) ";

        if (cbWithLogin.Checked == false)
        {
            SQL += "SELECT c.[CONTACT_ID],c.CONTACT_NAME, COUNT(lt.TICKET_GUID) AS NUMBER_LOGINS, " +
                "DATEDIFF (day, c.DATE_CREATED, GETDATE()) as DAYS_SINCE_CREATION, " +
                "DATEDIFF (DAY, GETDATE(), c.EXPIRY_DATE) as DAYS_REMAINING, " +
                "MAX(lt.CREATED)as LAST_ACCESS, " +
                "SUM(DATEDIFF(HOUR,lt.CREATED ,lt.LAST_RENEWED )) " +
                /*"FROM [ReviewerAdmin].[dbo].[TB_LOGON_TICKET] lt " +*/
                "FROM Reviewer.dbo.TB_CONTACT c " +

                "left join Reviewer.dbo.TB_SITE_LIC_CONTACT slc on slc.CONTACT_ID = c.CONTACT_ID " +
                "left join Reviewer.dbo.TB_SITE_LIC sl on sl.SITE_LIC_ID = slc.SITE_LIC_ID " +

                "left join Revieweradmin.dbo.TB_LOGON_TICKET lt " +
                "on lt.CONTACT_ID = c.CONTACT_ID " +
                "where c.EXPIRY_DATE > '2010-03-20 00:00:01' ";
            if (cbRemaining.Checked == true)
            {
                SQL += "and (DATEDIFF (DAY, GETDATE(), c.EXPIRY_DATE) > 0 or DATEDIFF (DAY, GETDATE(), sl.EXPIRY_DATE) > 0) ";
                //SQL += "and DATEDIFF (DAY, GETDATE(), c.EXPIRY_DATE) > 0 ";
            }
            SQL += "and ((c.CONTACT_ID like '%" + tbFilter.Text + "%') OR " +
                    "(c.CONTACT_NAME like '%" + tbFilter.Text + "%')) ";
            SQL += "group by c.CONTACT_ID, c.CONTACT_NAME, c.DATE_CREATED, c.EXPIRY_DATE  ";
        }
        else
        {
            SQL += "SELECT c.[CONTACT_ID],c.CONTACT_NAME, COUNT(lt.TICKET_GUID) AS NUMBER_LOGINS, " +
                "DATEDIFF (day, c.DATE_CREATED, GETDATE()) as DAYS_SINCE_CREATION, " +
                "DATEDIFF (DAY, GETDATE(), c.EXPIRY_DATE) as DAYS_REMAINING, " +
                "MAX(CREATED)as LAST_ACCESS, " +
                "SUM(DATEDIFF(HOUR,lt.CREATED ,lt.LAST_RENEWED )) FROM [ReviewerAdmin].[dbo].[TB_LOGON_TICKET] lt " +
                "Inner Join Reviewer.dbo.TB_CONTACT c " +
                "on lt.CONTACT_ID = c.CONTACT_ID " +

                "left join Reviewer.dbo.TB_SITE_LIC_CONTACT slc on slc.CONTACT_ID = c.CONTACT_ID " +
                "left join Reviewer.dbo.TB_SITE_LIC sl on sl.SITE_LIC_ID = slc.SITE_LIC_ID " +


                "Where CREATED > DATEADD(DD, -" + ddlTimePeriod.SelectedValue + ", GETDATE()) " +
                "and CREATED > '2010-07-01 00:00:01' " +
                "and c.EXPIRY_DATE > '2010-03-20 00:00:01' ";
            if (cbRemaining.Checked == true)
            {
                SQL += "and (DATEDIFF (DAY, GETDATE(), c.EXPIRY_DATE) > 0 or DATEDIFF (DAY, GETDATE(), sl.EXPIRY_DATE) > 0) ";
                //SQL += "and DATEDIFF (DAY, GETDATE(), c.EXPIRY_DATE) > 0 ";
            }
            SQL += "and ((c.CONTACT_ID like '%" + tbFilter.Text + "%') OR " +
                   "(c.CONTACT_NAME like '%" + tbFilter.Text + "%')) ";
            SQL += "group by c.CONTACT_ID, c.CONTACT_NAME, c.DATE_CREATED, c.EXPIRY_DATE  ";
        }

        SQL += "select * from @tb_user_stats";

        bool isAdmDB = false;
        SqlDataReader sdr = Utils.ReturnReader(SQL, isAdmDB);
        while (sdr.Read())
        {
            newrow = dt.NewRow();
            newrow["CONTACT_ID"] = sdr["contact_id"].ToString();
            newrow["CONTACT_NAME"] = sdr["contact_name"].ToString();
            newrow["NUMBER_LOGINS"] = sdr["number_logins"].ToString();
            if (sdr["number_hours"].ToString() == "")
                newrow["HOURS"] = 0;
            else
                newrow["HOURS"] = sdr["number_hours"].ToString();
            newrow["DAYS_SINCE_CREATION"] = sdr["days_since_creation"].ToString();
            newrow["DAYS_REMAINING"] = sdr["days_remaining"].ToString();
            newrow["LAST_ACCESS"] = sdr["last_access"].ToString();
            dt.Rows.Add(newrow);
        }
        sdr.Close();

        int test = dt.Rows.Count;
        radGVContacts.DataSource = dt;




        /*
        DataTable dt = new DataTable();
        System.Data.DataRow newrow;

        dt.Columns.Add(new DataColumn("CONTACT_ID", typeof(string)));
        dt.Columns.Add(new DataColumn("CONTACT_NAME", typeof(string)));
        dt.Columns.Add(new DataColumn("NUMBER_LOGINS", typeof(Int16)));
        dt.Columns.Add(new DataColumn("HOURS", typeof(Int16)));
        dt.Columns.Add(new DataColumn("DAYS_SINCE_CREATION", typeof(string)));
        dt.Columns.Add(new DataColumn("DAYS_REMAINING", typeof(string)));
        dt.Columns.Add(new DataColumn("LAST_ACCESS", typeof(string)));
        string SQL = "";
        string SQL2 = "";
        if (cbWithLogin.Checked == false)
        {
            SQL = "SELECT c.[CONTACT_ID],c.CONTACT_NAME, COUNT(lt.TICKET_GUID) AS NUMBER_LOGINS, " +
                "DATEDIFF (day, c.DATE_CREATED, GETDATE()) as DAYS_SINCE_CREATION, " +
                "DATEDIFF (DAY, GETDATE(), c.EXPIRY_DATE) as DAYS_REMAINING, " +
                "MAX(lt.CREATED)as LAST_ACCESS " +
                "FROM Reviewer.dbo.TB_CONTACT c " +
                "left join Revieweradmin.dbo.TB_LOGON_TICKET lt " +
                "on lt.CONTACT_ID = c.CONTACT_ID " +
                "where c.EXPIRY_DATE > '2010-03-20 00:00:01' ";
            if (cbRemaining.Checked == true)
            {
                SQL += "and DATEDIFF (DAY, GETDATE(), c.EXPIRY_DATE) > 0 ";
            }
            SQL += "and ((c.CONTACT_ID like '%" + tbFilter.Text + "%') OR " +
                    "(c.CONTACT_NAME like '%" + tbFilter.Text + "%')) ";
            SQL += "group by c.CONTACT_ID, c.CONTACT_NAME, c.DATE_CREATED, c.EXPIRY_DATE  ";
        }
        else
        {
            SQL = "SELECT c.[CONTACT_ID],c.CONTACT_NAME, COUNT(lt.TICKET_GUID) AS NUMBER_LOGINS, " +
                "DATEDIFF (day, c.DATE_CREATED, GETDATE()) as DAYS_SINCE_CREATION, " +
                "DATEDIFF (DAY, GETDATE(), c.EXPIRY_DATE) as DAYS_REMAINING, " +
                "MAX(CREATED)as LAST_ACCESS FROM [ReviewerAdmin].[dbo].[TB_LOGON_TICKET] lt " +
                "Inner Join Reviewer.dbo.TB_CONTACT c " +
                "on lt.CONTACT_ID = c.CONTACT_ID " +
                "Where CREATED > DATEADD(DD, -" + ddlTimePeriod.SelectedValue + ", GETDATE()) " +
                "and CREATED > '2010-07-01 00:00:01' " +
                "and c.EXPIRY_DATE > '2010-03-20 00:00:01' ";
            if (cbRemaining.Checked == true)
            {
                SQL += "and DATEDIFF (DAY, GETDATE(), c.EXPIRY_DATE) > 0 ";
            }
            SQL += "and ((c.CONTACT_ID like '%" + tbFilter.Text + "%') OR " +
                   "(c.CONTACT_NAME like '%" + tbFilter.Text + "%')) ";
            SQL += "group by c.CONTACT_ID, c.CONTACT_NAME, c.DATE_CREATED, c.EXPIRY_DATE  ";
        }

        bool isAdmDB = false;
        bool isAdmDB2 = true;
        SqlDataReader sdr = Utils.ReturnReader(SQL, isAdmDB);
        SqlDataReader sdr2;
        while (sdr.Read())
        {
            newrow = dt.NewRow();
            newrow["CONTACT_ID"] = sdr["CONTACT_ID"].ToString();
            newrow["CONTACT_NAME"] = sdr["CONTACT_NAME"].ToString();
            newrow["NUMBER_LOGINS"] = sdr["NUMBER_LOGINS"].ToString();

            SQL2 = "SELECT SUM(DATEDIFF(HOUR,t.CREATED ,t.LAST_RENEWED )) as HOURS " +
            "FROM [TB_LOGON_TICKET] t Inner JOIN Reviewer.dbo.TB_CONTACT c on t.CONTACT_ID = c.CONTACT_ID " +
            "where c.CONTACT_ID = '" + sdr["CONTACT_ID"].ToString() + "'";
            sdr2 = Utils.ReturnReader(SQL2, isAdmDB2);
            if (sdr2.Read())
            {
                newrow["HOURS"] = sdr2["HOURS"].ToString();
            }
            sdr2.Close();

            newrow["DAYS_SINCE_CREATION"] = sdr["DAYS_SINCE_CREATION"].ToString();
            newrow["DAYS_REMAINING"] = sdr["DAYS_REMAINING"].ToString();
            newrow["LAST_ACCESS"] = sdr["LAST_ACCESS"].ToString();
            dt.Rows.Add(newrow);
        }
        sdr.Close();

        int test = dt.Rows.Count;
        radGVContacts.DataSource = dt;
        */






    }
    protected void radGVContacts_PageIndexChanged(object sender, Telerik.Web.UI.GridPageChangedEventArgs e)
    {
        if (tbFilter.Text == "")
        {
            buildGridNew();
        }
        else
        {
            radGVContacts.Rebind();
        }
    }
    protected void radGVContacts_ItemDataBound(object sender, Telerik.Web.UI.GridItemEventArgs e)
    {

    }
    protected void cbValidER4Account_CheckedChanged(object sender, EventArgs e)
    {
        radGVContacts.Rebind();
    }
    protected void cbRemaining_CheckedChanged(object sender, EventArgs e)
    {
        if (cbRemaining.Checked == true)
            lblSiteLicenceMsg.Visible = true;
        else
            lblSiteLicenceMsg.Visible = false;
        radGVContacts.Rebind();
    }
    protected void ddlTimePeriod_SelectedIndexChanged(object sender, EventArgs e)
    {
        radGVContacts.Rebind();
    }
    protected void RadAjaxManager1_AjaxRequest(object sender, Telerik.Web.UI.AjaxRequestEventArgs e)
    {
        if (e.Argument.IndexOf("FilterGrid") != -1)
        {
            radGVContacts.Rebind();
        }
    }


}
