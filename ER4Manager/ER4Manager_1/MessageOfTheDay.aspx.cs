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

public partial class MessageOfTheDay : System.Web.UI.Page
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
                        lbl.Text = "Message of the day";
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


                    LoadGrid();
                }
                IBCalendar1.Attributes.Add("onclick", "JavaScript:openCalendar1('" +
                    "!1!" + tbDate.Text + "')");
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

    private void LoadGrid()
    {

        DataTable dt = new DataTable();
        System.Data.DataRow newrow;

        dt.Columns.Add(new DataColumn("MESSAGE", typeof(string)));
        dt.Columns.Add(new DataColumn("INSERT_TIME", typeof(string)));

        string SQL = "select top 20 * from TB_LATEST_SERVER_MESSAGE order by INSERT_TIME DESC";

        bool isAdmDB = true;
        SqlDataReader sdr = Utils.ReturnReader(SQL, isAdmDB);
        while (sdr.Read())
        {
            newrow = dt.NewRow();
            newrow["MESSAGE"] = sdr["MESSAGE"].ToString();
            newrow["INSERT_TIME"] = sdr["INSERT_TIME"].ToString();
            dt.Rows.Add(newrow);
        }
        sdr.Close();

        gvMessage.DataSource = dt;
        gvMessage.DataBind();
    }

    protected void cmdSave_Click(object sender, EventArgs e)
    {
        string SQL = "insert into TB_LATEST_SERVER_MESSAGE (MESSAGE) values ('" + tbMessage.Text + "')";
        bool isAdmDB = true;
        Utils.ExecuteQuery(SQL, isAdmDB);
        LoadGrid();
    }
    protected void cmdMarkBillAsPaid_Click(object sender, EventArgs e)
    {
        if (tbBillID.Text != "Enter bill ID")
        {
            Utils.ExecuteSP(true, Server, "st_BillMarkAsPaid", tbBillID.Text);
        }
    }
    protected void cmdGo_Click(object sender, EventArgs e)
    {
        lblMessage.Visible = false;
        lblMessage.Text = "";
        lblMessage.ForeColor = System.Drawing.Color.Red;
        bool validDate = true;
        string startDate = "";
        if ((tbDays.Text == "") || (tbDate.Text == ""))
        {
            lblMessage.Visible = true;
            lblMessage.Text = "Be sure to enter the days and date";
        }
        else
        {
            try
            {
                DateTime registrationDate = Convert.ToDateTime(tbDate.Text);
                startDate = registrationDate.ToString("yyyy-M-d hh:m:ss.mmm"); // this is case sensitive!
            }
            catch (Exception er)
            {
                lblMessage.Visible = true;
                lblMessage.Text = "Invalid date format";
                validDate = false;
            }

            if (validDate == true)
            {
                if (Utils.IsNumeric(tbDays.Text.ToString()))
                {
                    bool isAdmDB = true;
                    SqlParameter[] paramList = new SqlParameter[8];
                    paramList[0] = new SqlParameter("@ADD_OR_REMOVE", SqlDbType.NVarChar, 10, ParameterDirection.Input,
                        true, 0, 0, null, DataRowVersion.Default, rblAddRemove.SelectedValue);
                    paramList[1] = new SqlParameter("@NUMBER_DAYS", SqlDbType.NVarChar, 50, ParameterDirection.Input,
                        true, 0, 0, null, DataRowVersion.Default, tbDays.Text);
                    paramList[2] = new SqlParameter("@DATE", SqlDbType.NVarChar, 50, ParameterDirection.Input,
                        true, 0, 0, null, DataRowVersion.Default, startDate);
                    paramList[3] = new SqlParameter("@CONTACT_ID", SqlDbType.NVarChar, 50, ParameterDirection.Input,
                        true, 0, 0, null, DataRowVersion.Default, Utils.GetSessionString("Contact_ID"));
                    paramList[4] = new SqlParameter("@EXTENSION_NOTES", SqlDbType.NVarChar, 500, ParameterDirection.Input,
                        true, 0, 0, null, DataRowVersion.Default, tbNotes.Text);
                    paramList[5] = new SqlParameter("@RESULT_C", SqlDbType.NVarChar, 50, ParameterDirection.Output,
                        true, 0, 0, null, DataRowVersion.Default, "");
                    paramList[6] = new SqlParameter("@RESULT_R", SqlDbType.NVarChar, 50, ParameterDirection.Output,
                        true, 0, 0, null, DataRowVersion.Default, "");
                    paramList[7] = new SqlParameter("@RESULT_SL", SqlDbType.NVarChar, 50, ParameterDirection.Output,
                        true, 0, 0, null, DataRowVersion.Default, "");
                    Utils.ExecuteSPWithReturnValues(isAdmDB, Server, "st_ExpiryDateBulkAdjust", paramList);

                    if (paramList[5].Value.ToString() == "Invalid")
                    {
                        lblMessage.Visible = true;
                        lblMessage.Text = "SQL failed and was rolled back";
                    }
                    else
                    {
                        lblMessage.Visible = true;
                        lblMessage.Text = "Contacts affected: " + paramList[5].Value.ToString() +
                            ".         Reviews affected: " + paramList[6].Value.ToString() +
                            ".         Site licenses affected: " + paramList[7].Value.ToString() +
                            ".";
                    }
                }
                else
                {
                    lblMessage.Visible = true;
                    lblMessage.Text = "Enter a valid number of days";
                }
            }
        }
    }
    protected void cmdPlaceDate_Click(object sender, EventArgs e)
    {
        if (Utils.GetSessionString("CalendarDate") != "")
        {
            // a calendar counter is being passed with the date
            int counterLocation = Utils.GetSessionString("CalendarDate").IndexOf("!", 1);
            string calendarCounter = Utils.GetSessionString("CalendarDate").Substring(1, counterLocation - 1);

            if (calendarCounter == "1")
            {
                tbDate.Text = Utils.GetSessionString("CalendarDate").Substring(counterLocation + 1);
            }
            else
            {
                tbDate.Text = Utils.GetSessionString("CalendarDate").Substring(counterLocation + 1);
            }
        }
        Utils.SetSessionString("CalendarDate", "");
    }
}