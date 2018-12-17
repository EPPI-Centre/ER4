using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WcfHostPortal
{
    public partial class EppiReviewer4StartPage : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string pageURL = Request.Url.AbsoluteUri.ToLower();
            System.Configuration.Configuration rootWebConfig1 = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("/WcfHostPortal");
            System.Configuration.KeyValueConfigurationElement customSetting = rootWebConfig1.AppSettings.Settings["DomainRoot4Redirect"];
            //string badURLStart = "https://"+ customSetting.Value +"/", goodURLstart = "http://"+ customSetting.Value +"/";
            string badURLStart = "https://eppi.ioe.ac.uk/", goodURLstart = "http://eppi.ioe.ac.uk/";

            if (pageURL.IndexOf(badURLStart) == 0)
            {
                Response.BufferOutput = true;
                Response.Redirect(pageURL.Replace(badURLStart, goodURLstart));
            }
            else
            {
                string ver = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
                string[] splittedVer = ver.Split('.');
                if (splittedVer[1] == "0")
                {
                    Title = "EPPI-Reviewer4 Beta " + splittedVer[2] + " (V." + ver + ")";
                }
                else
                {
                    Title = "EPPI-Reviewer4 (V." + ver + ")";
                }

                this.idSource.Attributes["value"] += "?" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }
    }
}