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

public partial class TermsAndConditions : System.Web.UI.Page
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
                        lbl.Text = "Terms and conditions";
                    }

                    Telerik.Web.UI.RadTabStrip radTs = (Telerik.Web.UI.RadTabStrip)Master.FindControl("rtsMenu");
                    if (radTs != null)
                    {
                        radTs.SelectedIndex = 4;
                        radTs.Tabs[4].Tabs[2].Selected = true;
                        //radTs.Tabs[3].Tabs[2].Width = 670;
                    }
                    System.Web.UI.WebControls.Label lbl1 = (Label)Master.FindControl("lblHeadingText");
                    if (lbl1 != null)
                    {
                        lbl1.Text = "Terms and conditions";
                    }

                    /*string SQL = "select top 1 CONDITIONS from ReviewerAdmin.dbo.TB_TERMS_AND_CONDITIONS order by DATE_CREATED desc";
                    bool isAdmDB = true;
                    SqlDataReader sdr = Utils.ReturnReader(SQL, isAdmDB);
                    if (sdr.Read())
                    {
                        tbTandA.Text = sdr["CONDITIONS"].ToString();
                    }
                    sdr.Close();*/
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





    protected void cmdUploadFile_Click(object sender, EventArgs e)
    {
        long lMaxFileSize = 4096000;

        if (fDocument.PostedFile.ContentLength <= lMaxFileSize)
        {
            if (fDocument.PostedFile.FileName.ToString() != "")
            {
                // now upload the file
                string MyString = "";

                StringBuilder sb = new StringBuilder();
                string lfcr = "\r\n";
                string MyString1 = "";
                string MyString2 = "";

                System.IO.StreamReader s = new System.IO.StreamReader(fDocument.PostedFile.InputStream, Encoding.Default);

                try
                {
                    do
                    {
                        MyString1 = "";
                        MyString2 = "";
                        MyString = s.ReadLine();
                        // {} brackets cause problems for the Append function so I've replaced them with ()
                        MyString1 = MyString.Replace("{", "(");
                        MyString2 = MyString1.Replace("}", ")");
                        sb = sb.AppendFormat(MyString2);
                        sb = sb.AppendFormat(lfcr);
                    }
                    while (s.Peek() != -1);
                }
                catch
                {
                    MyString = "";
                }
                finally
                {
                    s.Close();
                }

                string fileString = sb.ToString();

                bool isAdmDB = true;
                Utils.ExecuteSP(isAdmDB, Server, "st_ConditionsUploadNew", fileString);

                tbTandA.Text = fileString;

            }
        }
    }


    protected void cmdSaveEdits_Click(object sender, EventArgs e)
    {
        //bool isAdmDB = true;
        //Utils.ExecuteSP(isAdmDB, Server, "st_ConditionsUploadNew", tbTandA.Text);
    }
}
