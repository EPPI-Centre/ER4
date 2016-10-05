using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Browser;
using BusinessLibrary.Security;
using Telerik.Windows.Controls;

namespace EppiReviewer4
{
    public partial class ArchieLogin : UserControl
    {
        public ArchieLogin()
        {
            InitializeComponent();
        }
        EppiReviewer4.App app = Application.Current as EppiReviewer4.App;
        public event EventHandler<LoginSuccessfulEventArgs> ArchieLoginSuccessful;
        public LoginSuccessfulEventArgs LoginSEA;
        private void BtLoginOnArchie_Click(object sender, RoutedEventArgs e)
        {
            if (app == null) return;
            app.jsb.AuthenticationIsDoneEvent -= jsb_AuthenticationIsDoneEvent;
            app.jsb.AuthenticationIsDoneEvent += new EventHandler<EventArgs>(jsb_AuthenticationIsDoneEvent);
            //System.Windows.Browser.HtmlPage.Window.Navigate(new Uri("Step1SetString.aspx", UriKind.Relative), "_blank");
            //System.Windows.Browser.HtmlPage.Window.Navigate(ArchieConnector.NewRequest, "_blank");
            HtmlPage.Window.Invoke("PopUpAndMonitor", ArchieConnector.NewRequest.ToString());
        }
        void jsb_AuthenticationIsDoneEvent(object sender, EventArgs e)
        {
            
            app.jsb.AuthenticationIsDoneEvent -= jsb_AuthenticationIsDoneEvent;
            Border bd = sender as Border;
            //RadWindow.Alert("bd.ToString: " + bd.ToString());
            //RadWindow.Alert("bd.Tag.ToString(): " + bd.Tag.ToString());
            //RadWindow.Alert("bd.Resources[status]: " + bd.Resources["status"].ToString());
            
            if (bd != null && bd.Tag != null && bd.Tag.ToString() != "" && bd.Resources["status"] != null)
            {
                txtArchieMsg.Text = "Please wait...";// + bd.Tag;
                //this is the actual Cochrane logon route, so now we start the route to server-side;
                //fire a SL event when logging on is done
                ReviewerPrincipal.Login(bd.Tag.ToString(), bd.Resources["status"].ToString(), "Archie", 0, (o1, e1) =>
                    {

                        if (Csla.ApplicationContext.User.Identity.IsAuthenticated)
                        {
                            txtArchieMsg.Text = "User authenticated.";
                        
                            if (ArchieLoginSuccessful != null)
                            {
                                LoginSEA = new LoginSuccessfulEventArgs("", "", bd.Tag.ToString(), bd.Resources["status"].ToString());
                                ArchieLoginSuccessful.Invoke(this, LoginSEA);
                            }
                        
                        }
                        else
                        {
                            txtArchieMsg.Text = "Invalid login. Please try again.";
                            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                            //txtArchieMsg.Text = ri.Ticket;
                            if (ri.Ticket.IndexOf("Access denied, not an Author") == 0 || ri.Ticket.IndexOf("Access denied, can't verify") == 0)
                            {//specific error, stuff worked user did authenticate, but user isn't a Cochrane author, so we will explain why this is happening
                                StackPanel sp = new StackPanel();
                                sp.Orientation = Orientation.Vertical;
                                TextBlock tb1 = new TextBlock();
                                tb1.Text = "Your Cochrane account does not grant access to EPPI-Reviewer.";
                                tb1.FontWeight = FontWeights.Bold;
                                sp.Children.Add(tb1);
                                TextBlock tb2 = new TextBlock();
                                tb2.Text = "Access is restricted to Cochrane authors and others involved in the review" + Environment.NewLine
                                         + "writing process. If you believe you should have access, please contact" + Environment.NewLine 
                                         + "your review group to verify you have been assigned the correct roles.";
                                sp.Children.Add(tb2);
                                TextBlock tb3 = new TextBlock();
                                tb3.Text = "Otherwise, the details below will help the EPPI-Support team to understand" + Environment.NewLine + "the issue:";
                                sp.Children.Add(tb3);
                                TextBlock tb4 = new TextBlock();
                                tb4.Text = ri.Ticket;
                                sp.Children.Add(tb4);
                                RadWindow.Alert(sp);
                            }
                            //we ignore all other error details and just say "Invalid login. Please try again."
                        }
                    }
                );

                return;
            }
            else
            {// see if the sender is an ellipse, if it is, it may have some info on what went wrong.
                Ellipse el = sender as Ellipse;
                if (el != null)
                {
                    string error = el.Tag.ToString();
                    string errorDescr = el.Resources["error_description"].ToString();
                    RadWindow.Alert("Authentication in Archie failed" + Environment.NewLine
                        + "Error message is:" + Environment.NewLine +
                        error + Environment.NewLine + errorDescr);
                }
                txtArchieMsg.Text = "Cochrane authentication failed.";
            }
        }
    }
    public static class ArchieConnector
    {
        private static string _state;
        private static string host = System.Windows.Application.Current.Host.Source.DnsSafeHost.ToLower();

        public static string state
        {
            get { return _state; }
        }
        public static string BasePath
        {
            get
            {
                string dest = "";
                if (host == "eppi.ioe.ac.uk" | host == "epi2" | host == "epi2.ioe.ac.uk")
                {//use live address: this is the real published ER4
                    dest = "https://account.cochrane.org/oauth2/auth?client_id=";
                }
                else if (host == "bk-epi" | host == "bk-epi.ioead" | host == "bk-epi.inst.ioe.ac.uk")
                {//this is our testing environment, the first tests should be against the test archie, otherwise the real one
                    //changes are to be made here depending on what test we're doing
                    dest = "https://test-account.cochrane.org/oauth2/auth?client_id=";
                }
                else
                {//not a live publish, use test archie
                    dest = "https://test-account.cochrane.org/oauth2/auth?client_id=";
                }
                return dest;
            }
        }
        public static Uri NewRequest
        {
            get
            {
                //var authServerHost = location.hostname;
                string clientId = Uri.EscapeUriString("eppi");
                string redirect = System.Windows.Browser.HtmlPage.Document.DocumentUri.ToString();
                if (!redirect.Contains("TestingSLandCSLA"))
                {//this is the real ER4
                    
                    //System (1): https://ssru38.ioe.ac.uk/TestingSLandCSLA.Web/ArchieCallBack.aspx
                    //System (2): https://ssru38.ioe.ac.uk/WcfHostPortal/ArchieCallBack.aspx
                    //System (3): https://bk-epi.ioe.ac.uk/testing/er4/ArchieCallBack.aspx
                    //Live System (4): https://eppi.ioe.ac.uk/eppireviewer4/ArchieCallBack.aspx
                    //System (5) testing: https://ssru30.ioe.ac.uk/WcfHostPortal/ArchieCallBack.aspx
                    //we need to make sure that the redirect URL matches the case of what is known in archie
                    if (redirect.Contains("eppi.ioe.ac.uk"))
                    {
                        redirect = "https://eppi.ioe.ac.uk/eppireviewer4/ArchieCallBack.aspx";
                    }
                    else if (redirect.Contains("ssru38.ioe.ac.uk"))
                    {
                        redirect = "https://ssru38.ioe.ac.uk/WcfHostPortal/ArchieCallBack.aspx";
                    }
                    else if (redirect.Contains("ssru30.ioe.ac.uk"))
                    {
                        redirect = "https://ssru30.ioe.ac.uk/WcfHostPortal/ArchieCallBack.aspx";
                    }
                    else if (redirect.Contains("bk-epi.ioe.ac.uk.ioe.ac.uk"))
                    {
                        redirect = "https://bk-epi.ioe.ac.uk/testing/er4/ArchieCallBack.aspx";
                    }
                    else
                    {//we try to do something meaningful, but it will fail on the archie side!
                        if (redirect.ToLower().Contains("eppireviewer4.aspx"))
                        {
                            string tpp = redirect.ToLower();
                            redirect = redirect.Remove(tpp.IndexOf("eppireviewer4.aspx"));//leave the root of the app
                        }

                        redirect = redirect.Replace("http://", "https://");
                        redirect += "ArchieCallBack.aspx";
                    }
                }
                else
                {//we are using the little test app my machine only, for now
                    redirect = "https://ssru38.ioe.ac.uk/TestingSLandCSLA.Web/";
                    //redirect = redirect.Remove(redirect.IndexOf("TestingSLandCSLA.aspx"));//leave the root of the app
                }
                redirect = Uri.EscapeUriString(redirect);
                const string allowedChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz0123456789";
                char[] chars = new char[10];
                var rd = new Random();

                for (int i = 0; i < chars.Length; i++)
                {
                    chars[i] = allowedChars[rd.Next(0, allowedChars.Length)];
                }
                _state = Uri.EscapeUriString(new string(chars));
                //string scope = "all";
                string scope = "document person";
                string combined = BasePath + clientId + "&response_type=code&redirect_uri=" + redirect
                                    + "&scope=" + scope + "&state=" + _state + "&access_type=offline";
                Uri res = new Uri(combined, UriKind.Absolute);
                return res;

            }
        }

    }
}
