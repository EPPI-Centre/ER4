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

public partial class Calendar_window_1 : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            string date_found;
            if (Request.QueryString.Get("date").StartsWith("!"))
            {
                // a calendar counter is being passed with the date
                int counterLocation = Request.QueryString.Get("date").IndexOf("!", 1);
                lblCalendarCounter.Text = Request.QueryString.Get("date").Substring(1, counterLocation - 1);
                date_found = Request.QueryString.Get("date").Substring(counterLocation + 1);
            }
            else
            {
                date_found = Request.QueryString.Get("date");
            }

            if ((date_found == "") || (date_found == " "))
            {
                // no date has been set yet so set drop down menue to today
                string month = DateTime.Today.Month.ToString();
                if (month.Length < 2)
                {
                    month = "0" + month;
                }
                ddlChangeMonth.SelectedValue = month;
                ddlChangeYear.SelectedValue = DateTime.Today.Year.ToString();
                DateTime date = DateTime.Parse(DateTime.Today.ToShortDateString());
                Calendar1.TodaysDate = date;
            }
            else
            {
                bool validDate = true;
                try
                {
                    DateTime registrationDate = Convert.ToDateTime(date_found);
                }
                catch (Exception er)
                {
                    // there is a problem with the date so set to today
                    string month = DateTime.Today.Month.ToString();
                    if (month.Length < 2)
                    {
                        month = "0" + month;
                    }
                    ddlChangeMonth.SelectedValue = month;
                    ddlChangeYear.SelectedValue = DateTime.Today.Year.ToString();
                    DateTime date = DateTime.Parse(DateTime.Today.ToShortDateString());
                    Calendar1.TodaysDate = date;
                    validDate = false;
                }

                if (validDate == true)
                {
                    // set dropdown menu
                    ddlChangeMonth.SelectedValue = date_found.Substring(3, 2);
                    ddlChangeYear.SelectedValue = date_found.Substring(6, 4);

                    // set calendar to whatever date is presently in the field
                    DateTime date = DateTime.Parse(date_found);
                    Calendar1.TodaysDate = date;
                }
            }

        }
    }
    protected void Calendar1_SelectionChanged(object sender, EventArgs e)
    {
        if (lblDidDDLChange.Text == "No")
        {
            Utils.SetSessionString("CalendarDate", "!" + lblCalendarCounter.Text +
                "!" + Calendar1.SelectedDate.ToShortDateString());

            // close window
            string scriptString = "<script language=JavaScript>";

            if (Request.QueryString.Get("calling_function") == "popup")
            {
                // this line is for calling back to a popup window
                scriptString += "window.opener.document.forms[0].cmdPlaceDate.click();";
            }
            else
            {
                // this line is for calling back to a master page
                //scriptString += "window.opener.document.forms[0].ctl00_ContentPlaceHolder1_cmdPlaceDate.click();";
                scriptString += "window.opener.document.forms[0].ctl00$ContentPlaceHolder1$cmdPlaceDate.click();";
            }
            scriptString += "window.close();<";
            scriptString += "/";
            scriptString += "script>";
            Type cstype = this.GetType();
            ClientScriptManager cs = Page.ClientScript;

            if (!cs.IsStartupScriptRegistered("Startup"))
                cs.RegisterStartupScript(cstype, "Startup", scriptString);
        }
        else
        {
            // let calendar change date but do not react to it
            // also reset flag
            lblDidDDLChange.Text = "No";
        }
    }
    protected void ddlChangeMonth_SelectedIndexChanged(object sender, EventArgs e)
    {
        lblDidDDLChange.Text = "Yes";

        string presentDate = Calendar1.TodaysDate.ToShortDateString();
        string month = presentDate.Substring(3, 2);
        presentDate = presentDate.Replace("/" + month + "/", "/" + ddlChangeMonth.SelectedValue.ToString() + "/");

        DateTime date = DateTime.Parse(presentDate);
        Calendar1.TodaysDate = date;

        Calendar1_SelectionChanged(sender, e);
    }
    protected void ddlChangeYear_SelectedIndexChanged(object sender, EventArgs e)
    {
        lblDidDDLChange.Text = "Yes";

        string presentDate = Calendar1.TodaysDate.ToShortDateString();
        string year = presentDate.Substring(6, 4);
        presentDate = presentDate.Replace("/" + year, "/" + ddlChangeYear.SelectedValue.ToString());

        DateTime date = DateTime.Parse(presentDate);
        Calendar1.TodaysDate = date;

        Calendar1_SelectionChanged(sender, e);
    }
    protected void cmdToday_Click(object sender, EventArgs e)
    {
        //DateTime date = DateTime.Today;
        DateTime date = DateTime.Now;
        string presentDate = Calendar1.TodaysDate.ToShortDateString();
        //Utils.SetSessionString("CalendarDate", "!" + lblCalendarCounter.Text +
        //        "!" + date.ToShortDateString());
        Utils.SetSessionString("CalendarDate", "!" + lblCalendarCounter.Text +
                "!" + date);

        string scriptString = "<script language=JavaScript>";

        if (Request.QueryString.Get("calling_function") == "popup")
        {
            // this line is for calling back to a popup window
            scriptString += "window.opener.document.forms[0].cmdPlaceDate.click();";
        }
        else
        {
            // this line is for calling back to a master page
            //scriptString += "window.opener.document.forms[0].ctl00_ContentPlaceHolder1_cmdPlaceDate.click();";
            scriptString += "window.opener.document.forms[0].ctl00$ContentPlaceHolder1$cmdPlaceDate.click();";
        }
        scriptString += "window.close();<";
        scriptString += "/";
        scriptString += "script>";
        Type cstype = this.GetType();
        ClientScriptManager cs = Page.ClientScript;

        if (!cs.IsStartupScriptRegistered("Startup"))
            cs.RegisterStartupScript(cstype, "Startup", scriptString);
    }
    protected void cmdClearDate_Click(object sender, EventArgs e)
    {

    }
}
