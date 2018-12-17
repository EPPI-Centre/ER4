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

public partial class PurchaseHistory : System.Web.UI.Page
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
                    lbl.Text = "Purchase history";
                }

                Telerik.Web.UI.RadTabStrip radTs = (Telerik.Web.UI.RadTabStrip)Master.FindControl("rtsMenu");
                if (radTs != null)
                {
                    radTs.SelectedIndex = 1;
                    radTs.Tabs[1].Tabs[1].Selected = true;
                    radTs.Tabs[1].Tabs[2].Width = 550;
                }
                System.Web.UI.WebControls.Label lbl1 = (Label)Master.FindControl("lblHeadingText");
                if (lbl1 != null)
                {
                    lbl1.Text = "Purchase history";
                }

                buildBillingHistoryGrid();
            }
        }
        else
        {
            Server.Transfer("Error.aspx");
        }

    }

    private void buildBillingHistoryGrid()
    {
        lblBillee.Text = "No billing history for this account";
        DateTime today = DateTime.Today;
        DataTable dt = new DataTable();
        System.Data.DataRow newrow;

        dt.Columns.Add(new DataColumn("BILL_ID", typeof(string)));
        dt.Columns.Add(new DataColumn("PURCHASER_ID", typeof(string)));
        dt.Columns.Add(new DataColumn("DATE_PURCHASED", typeof(string)));
        dt.Columns.Add(new DataColumn("NOMINAL_PRICE", typeof(string)));
        dt.Columns.Add(new DataColumn("DISCOUNT", typeof(string)));
        dt.Columns.Add(new DataColumn("PRICE_DUE", typeof(string)));
        dt.Columns.Add(new DataColumn("BILL_STATUS", typeof(string)));
        bool isAdmDB = true;
        IDataReader idr = Utils.GetReader(isAdmDB, "st_ContactBills",
            Utils.GetSessionString("Contact_ID"));
        while (idr.Read())
        {
            newrow = dt.NewRow();
            lblBillID.Text = idr["BILL_ID"].ToString();
            lblBillee.Text = idr["CONTACT_NAME"].ToString();
            newrow["BILL_ID"] = idr["BILL_ID"].ToString();
            newrow["PURCHASER_ID"] = idr["PURCHASER_CONTACT_ID"].ToString();
            newrow["DATE_PURCHASED"] = idr["DATE_PURCHASED"].ToString();
            newrow["NOMINAL_PRICE"] = idr["NOMINAL_PRICE"].ToString();
            newrow["DISCOUNT"] = idr["DISCOUNT"].ToString();
            newrow["PRICE_DUE"] = double.Parse(idr["DUE_PRICE"].ToString()) + (idr["VAT"] == null || idr["VAT"].ToString() == "" ? 0 : double.Parse(idr["VAT"].ToString()));
            newrow["BILL_STATUS"] = idr["BILL_STATUS"].ToString();
            dt.Rows.Add(newrow);
        }
        idr.Close();


        gvBills.DataSource = dt;
        gvBills.DataBind();
    }

    protected void gvBills_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        int index = Convert.ToInt32(e.CommandArgument);
        string BillID = (string)gvBills.DataKeys[index].Value;
        switch (e.CommandName)
        {
            case "DETAILS":
                pnlBillDetails.Visible = true;
                gvBills.Visible = true;

                DataTable dt = new DataTable();
                System.Data.DataRow newrow;

                dt.Columns.Add(new DataColumn("LINE_ID", typeof(string)));
                dt.Columns.Add(new DataColumn("TYPE", typeof(string)));
                dt.Columns.Add(new DataColumn("AFFECTED_ID", typeof(string)));
                dt.Columns.Add(new DataColumn("NAME", typeof(string)));
                dt.Columns.Add(new DataColumn("MONTHS", typeof(string)));
                dt.Columns.Add(new DataColumn("COST_PER_MONTH", typeof(string)));
                dt.Columns.Add(new DataColumn("COST", typeof(string)));

                bool isAdmDB = true;
                IDataReader idr = Utils.GetReader(isAdmDB, "st_BillDetailsAccounts",
                    BillID);
                IDataReader idr1 = Utils.GetReader(isAdmDB, "st_BillDetailsReviews",
                    BillID);
                while (idr.Read())
                {
                    newrow = dt.NewRow();
                    newrow["LINE_ID"] = idr["LINE_ID"].ToString();
                    newrow["TYPE"] = idr["DETAILS"].ToString();
                    newrow["AFFECTED_ID"] = idr["AFFECTED_ID"].ToString();
                    if (idr["MONTHS_CREDIT"].ToString() != "0")
                    {
                        newrow["NAME"] = "Not activated"; 
                    }
                    else
                    {
                        if ((idr["NAME"].ToString() == "") || (idr["NAME"].ToString() == null))
                        {
                            newrow["NAME"] = "Not setup"; 
                        }
                        else
                        {
                            newrow["NAME"] = idr["NAME"].ToString();
                        }
                    }
                    newrow["MONTHS"] = idr["MONTHS"].ToString();
                    newrow["COST_PER_MONTH"] = idr["PRICE_PER_MONTH"].ToString();
                    newrow["COST"] = idr["COST"].ToString();
                    dt.Rows.Add(newrow);
                }
                idr.Close();
                while (idr1.Read())
                {
                    newrow = dt.NewRow();
                    newrow["LINE_ID"] = idr1["LINE_ID"].ToString();
                    newrow["TYPE"] = idr1["DETAILS"].ToString();
                    newrow["AFFECTED_ID"] = idr1["AFFECTED_ID"].ToString();
                    if (idr1["MONTHS_CREDIT"].ToString() != "0")
                    {
                        newrow["NAME"] = "Not activated";
                    }
                    else
                    {
                        if ((idr1["NAME"].ToString() == "") || (idr1["NAME"].ToString() == null))
                        {
                            newrow["NAME"] = "Not setup";
                        }
                        else
                        {
                            newrow["NAME"] = idr1["NAME"].ToString();
                        }
                    }




                    /*
                    if (idr1["NAME"].ToString() == "")
                        newrow["NAME"] = "Not setup";
                    else
                        newrow["NAME"] = idr1["NAME"].ToString();
                    */
                    newrow["MONTHS"] = idr1["MONTHS"].ToString();
                    newrow["COST_PER_MONTH"] = idr1["PRICE_PER_MONTH"].ToString();
                    newrow["COST"] = idr1["COST"].ToString();
                    dt.Rows.Add(newrow);
                }
                idr1.Close();

                gvBillDetails.DataSource = dt;
                gvBillDetails.DataBind();


                break;

            default:
                break;
        }
    }
    protected void lbInvoice_Click(object sender, EventArgs e)
    {
        string data = "";
        string discount = "";
        string totalFee = "";
        string vat = "";
        Response.ContentType = "application/x-unknown";
        Response.AddHeader("content-disposition", "attachment; filename=ER4_Invoice_Number_" + 
            lblBillID.Text + ".txt");
        for (int i = 0; i < 3; i++)
        {
            Response.Write(Environment.NewLine);
        }

        bool isAdmDB = true;
        IDataReader idr2 = Utils.GetReader(isAdmDB, "st_BillGet",
            lblBillID.Text);
        IDataReader idr0 = Utils.GetReader(isAdmDB, "st_BillAddressGet",
            Utils.GetSessionString("Contact_ID"));
        IDataReader idr = Utils.GetReader(isAdmDB, "st_BillDetailsAccounts",
            lblBillID.Text);
        IDataReader idr1 = Utils.GetReader(isAdmDB, "st_BillDetailsReviews",
            lblBillID.Text);

        Response.Write("EPPI-Reviewer 4.0 invoice number: " + lblBillID.Text);
        Response.Write(Environment.NewLine);
        while (idr2.Read())
        {
            Response.Write("Dated: " + idr2["DATE_PURCHASED"].ToString());
            discount = idr2["DISCOUNT"].ToString();
            totalFee = idr2["DUE_PRICE"].ToString();
            vat = idr2["VAT"].ToString();
        }
        idr2.Close();
        Response.Write(Environment.NewLine);
        Response.Write(Environment.NewLine);
        Response.Write("Bill to: ");
        Response.Write(Environment.NewLine);
        while (idr0.Read())
        {
            Response.Write(idr0["CONTACT_NAME"].ToString());
            Response.Write(Environment.NewLine);
            Response.Write(idr0["ORGANISATION"].ToString());
            Response.Write(Environment.NewLine);
            Response.Write(idr0["ADDRESS"].ToString());
            Response.Write(Environment.NewLine);
            Response.Write("Country: " + idr0["COUNTRY_NAME"].ToString());
            Response.Write(Environment.NewLine);
            Response.Write("EU VAT Registration: " + idr0["EU_VAT_REG_NUMBER"].ToString());
            Response.Write(Environment.NewLine);
        }
        idr0.Close();
        Response.Write(Environment.NewLine);
        while (idr.Read())
        {
            Response.Write("Item ID: " + idr["LINE_ID"].ToString());
            Response.Write(Environment.NewLine);
            Response.Write("Type: " + idr["DETAILS"].ToString());
            Response.Write(Environment.NewLine);
            Response.Write("ER4 ID: " + idr["AFFECTED_ID"].ToString());
            data = idr["MONTHS_CREDIT"].ToString();
            if (idr["MONTHS_CREDIT"].ToString() != "0")
            {
                
                Response.Write(" - New account: Unactivated at time of purchase");
                Response.Write(Environment.NewLine);            
            }
            else
            {
                data = idr["NAME"].ToString();
                if ((idr["NAME"].ToString() == "") || (idr["NAME"].ToString() == null))
                {
                    Response.Write(" - New account: Name not set at time of purchase");
                    Response.Write(Environment.NewLine);
                }
                else
                {
                    Response.Write(" - Acccount extension for: " + idr["NAME"].ToString());
                    Response.Write(Environment.NewLine);
                }
            }
            Response.Write("Months purchased: " + idr["MONTHS"].ToString());
            Response.Write(Environment.NewLine);
            Response.Write("Cost per month: £" + idr["PRICE_PER_MONTH"].ToString());
            Response.Write(Environment.NewLine);
            Response.Write("Total cost: £" + idr["COST"].ToString());
            Response.Write(Environment.NewLine);
            Response.Write(Environment.NewLine);
        }
        idr.Close();
        while (idr1.Read())
        {
            Response.Write("Item ID: " + idr1["LINE_ID"].ToString());
            Response.Write(Environment.NewLine);
            Response.Write("Type: " + idr1["DETAILS"].ToString());
            Response.Write(Environment.NewLine);
            Response.Write("ER4 ID: " + idr1["AFFECTED_ID"].ToString());
            data = idr1["MONTHS_CREDIT"].ToString();
            if (idr1["MONTHS_CREDIT"].ToString() != "0")
            {

                Response.Write(" - New review: Unactivated at time of purchase");
                Response.Write(Environment.NewLine);
            }
            else
            {
                data = idr1["NAME"].ToString();
                if ((idr1["NAME"].ToString() == "") || (idr1["NAME"].ToString() == null))
                {
                    Response.Write(" - New review: Name not set at time of purchase");
                    Response.Write(Environment.NewLine);
                }
                else
                {
                    Response.Write(" - Review extension for: " + idr1["NAME"].ToString());
                    Response.Write(Environment.NewLine);
                }
            }
            Response.Write("Months purchased: " + idr1["MONTHS"].ToString());
            Response.Write(Environment.NewLine);
            Response.Write("Cost per month: £" + idr1["PRICE_PER_MONTH"].ToString());
            Response.Write(Environment.NewLine);
            Response.Write("Total cost: £" + idr1["COST"].ToString());
            Response.Write(Environment.NewLine);
            Response.Write(Environment.NewLine);
        }
        idr1.Close();

        Response.Write(Environment.NewLine);
        //Response.Write("Discount: £" + discount);
        //Response.Write(Environment.NewLine);
        if (vat != "")
        {
            Response.Write("VAT: £" + vat);
            Response.Write(Environment.NewLine);
        }
        Response.Write("Total fee: £" + totalFee);
        Response.Write(Environment.NewLine);
        Response.End();



    }
}
