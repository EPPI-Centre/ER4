using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WcfHostPortal
{
    public partial class ArchieCallBack : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            lblErrorMsg.Text = "";
            string code = Request.QueryString["code"];
            string state = Request.QueryString["state"];
            string error = Request.QueryString["error"];
            string errorDescr = Request.QueryString["error_description"];
            if (error != null && error != "")
            {
                HttpCookie ck = new HttpCookie("oAuthCochraneEPPI");
                ck.Values["code"] = "";
                ck.Values["state"] = "";
                ck.Values["error"] = error;
                if (errorDescr != null) ck.Values["error_description"] = errorDescr;
                else ck.Values["error_description"] = "";
                ck.Path = "/";
                ck.Expires = System.DateTime.Now.AddMinutes(1);
                this.Response.Cookies.Add(ck);
                lblErrorMsg.Text = "Your Archie authentication did not succeed. Please close this window and try again. <br />If the problem persists, contact EPPISupport";
            }
            else if ((code == null || code == "") || (state == null || state == ""))
            {//not a valid attempt, a bot or something: we didn't get the expected query strings from Archie.
                lblErrorMsg.Text = "Not a valid request: this page is not intended to be browseable.";
            }
            else
            {
                //if (state.Length == 12)//temporary!!!
                //{
                //    Response.Redirect("https://ssru38.ioe.ac.uk/ERx/ArchieCallBack?code=" + code + "&state=" + state);
                //}
                //else
                //{
                    //HiddenField1.Value = code;
                    //HiddenField2.Value = state;
                    HttpCookie ck = new HttpCookie("oAuthCochraneEPPI");
                    ck.Values["code"] = code;
                    ck.Values["state"] = state;
                    ck.Path = "/";
                    ck.Expires = System.DateTime.Now.AddMinutes(61);
                    //ck.Expires = System.DateTime.Now.AddMinutes(1);
                    this.Response.Cookies.Add(ck);
                //}
            }
        }
    }
}