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

            this.idSource.Attributes["value"] += "?"+System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }
    }
}