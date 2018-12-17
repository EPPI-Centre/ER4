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
using BusinessLibrary.Security;
using System.Text.RegularExpressions;
using Csla;
using Telerik.Windows.Controls;

namespace EppiReviewer4
{
    public partial class ArchieUserSetupNew : UserControl
    {
        //this shows up if and only if a user has logged on via Archie and we didn't already know the ArchieID
        //ask user whether to create a new ER4 account (1) or to link to an exsiting account(2)
        //(1) ask for email, username and PW. Check they are OK, create user. User will have to activate the ER4 side of the account (activation email needs to be sent!).
        //tell the user: if you don't receive the email please contact EPPISupport
        //(2) provide username-password-confirm controls, user logs-on via a special criteria which contains uname, pw, archieCode and staus
        //Ri grabs user info via standard user logon, then the archie info from the "unrecognised" table, updates tb_contact returns as authenticated all is well
        public event EventHandler<LoginSuccessfulEventArgs> LinkToArchieLoginSuccessful;
        public event EventHandler LinkToArchieCancelled;
        public string username;
        public string password;
        public string ArchieCode, ArchieStatus;
        public ArchieUserSetupNew()
        {
            InitializeComponent();
        }

        private void btRoute1Start_Click(object sender, RoutedEventArgs e)
        {
            IntroGr.Visibility = System.Windows.Visibility.Collapsed;
            CreateAccountStage1Gr.Visibility = System.Windows.Visibility.Visible;
        }

        private void btRoute2Start_Click(object sender, RoutedEventArgs e)
        {
            IntroGr.Visibility = System.Windows.Visibility.Collapsed;
            LinkAccountStage1Gr.Visibility = System.Windows.Visibility.Visible;
        }

        private void btER4Logon_Click(object sender, RoutedEventArgs e)
        {
            tBoxER4Username.IsReadOnly = true;
            UserPwdBox.IsEnabled = false;
            btER4Logon.IsEnabled = false;
            ReviewerPrincipal.Login(tBoxER4Username.Text.Trim(), UserPwdBox.Password.Trim(), ArchieCode, ArchieStatus, "Archie", 0, (o1, e1) =>
            {
                tBoxER4Username.IsReadOnly = false;
                UserPwdBox.IsEnabled = true;
                btER4Logon.IsEnabled = true;
                if (Csla.ApplicationContext.User.Identity.IsAuthenticated)
                {
                    username = tBoxER4Username.Text.Trim();
                    password = UserPwdBox.Password.Trim();
                    LinkAccountStage1Gr.Visibility = System.Windows.Visibility.Collapsed;
                    LinkAccountDoneGr.Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    //do something, maybe allow 3 attempts?
                }
            }
            );
        }

        private void btLinkAccountDone_Click(object sender, RoutedEventArgs e)
        {
            if (LinkToArchieLoginSuccessful != null)
            {
                LoginSuccessfulEventArgs lsea = new LoginSuccessfulEventArgs(username, password, ArchieCode, ArchieStatus);
                LinkToArchieLoginSuccessful.Invoke(this, lsea);
            }
        }
        private void btCreateAccountDone_Click(object sender, RoutedEventArgs e)
        {
            if (LinkToArchieLoginSuccessful != null)
            {
                LoginSuccessfulEventArgs LoginSEA = new LoginSuccessfulEventArgs("", "", ArchieCode, ArchieCode);
                LinkToArchieLoginSuccessful.Invoke(this, LoginSEA);
            }
        }
        private void UserPwdBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key.ToString() == "Enter")
            {
                RoutedEventArgs rea = new RoutedEventArgs();
                btER4Logon_Click(sender, rea);
            }
        }

        private void UserPwdBox_GotFocus(object sender, RoutedEventArgs e)
        {
            UserPwdBox.SelectAll();
        }

        private void cbShowPassword_Checked(object sender, RoutedEventArgs e)
        {
            tboxPassword1.Text = pboxPassword1.Password;
            tboxPassword2.Text = pboxPassword2.Password;
            tboxPassword1.Visibility = System.Windows.Visibility.Visible;
            tboxPassword2.Visibility = System.Windows.Visibility.Visible;
            pboxPassword1.Visibility = System.Windows.Visibility.Collapsed;
            pboxPassword2.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void cbShowPassword_Unchecked(object sender, RoutedEventArgs e)
        {
            pboxPassword1.Password = tboxPassword1.Text;
            pboxPassword2.Password = tboxPassword2.Text;
            tboxPassword1.Visibility = System.Windows.Visibility.Collapsed;
            tboxPassword2.Visibility = System.Windows.Visibility.Collapsed;
            pboxPassword1.Visibility = System.Windows.Visibility.Visible;
            pboxPassword2.Visibility = System.Windows.Visibility.Visible;
        }

        private void btCheckandCreate_Click(object sender, RoutedEventArgs e)
        {
            //first part, client side checking that all fields are correct
            BusyLoading.IsRunning = true;
            this.IsEnabled = false;
            string errorMsg = "";
            Color mycolor = new Color();
            mycolor.R = 255;
            mycolor.G = 100;
            mycolor.B = 100;
            mycolor.A = 155;
            SolidColorBrush scb = new SolidColorBrush(mycolor);
            if (pboxPassword1.Visibility == System.Windows.Visibility.Visible)
            {
                tboxPassword1.Text = pboxPassword1.Password;
                tboxPassword2.Text = pboxPassword2.Password;
            }
            else
            {
                pboxPassword1.Password = tboxPassword1.Text;
                pboxPassword2.Password = tboxPassword2.Text;
            }
            if (tboxFirstname.Text.Trim().Length < 1)
            {
                errorMsg = "1.";
                tboxFirstname.Background = scb;
            }
            else
            {
                tboxFirstname.Background = null;
            }
            if (tboxLastname.Text.Trim().Length < 1)
            {
                errorMsg += "2.";
                tboxLastname.Background = scb;
            }
            else
            {
                tboxLastname.Background = null;
            }
            if (tboxUsername.Text.Trim().Length < 4)
            {
                errorMsg += "3.";
                tboxUsername.Background = scb;
            }
            else
            {
                tboxUsername.Background = null;
            }
            if (tboxEmail1.Text.Trim().Length < 1 //too short
                || 
                ((tboxEmail1.Text.Trim().IndexOf("@") < 2) ||
                    (tboxEmail1.Text.Trim().IndexOf("@") >= tboxEmail1.Text.Trim().Length - 1)
                )//@ symbol absent or too close to the string edges
                ||
                ((tboxEmail1.Text.Trim().LastIndexOf(".") < 2) ||
                    (tboxEmail1.Text.Trim().LastIndexOf(".") >= tboxEmail1.Text.Trim().Length - 1)
                )//. symbol absent or too close to the string edges
                )
            {
                errorMsg += ".4";
                tboxEmail1.Background = scb;
                tboxEmail2.Background = scb;
            }
            else if (tboxEmail1.Text.Trim() != tboxEmail2.Text.Trim())
            {
                errorMsg += ".4";
                tboxEmail1.Background = scb;
                tboxEmail2.Background = scb;
            }
            else
            {
                tboxEmail1.Background = null;
                tboxEmail2.Background = null;
            }
            Regex passwordRegex = new Regex("^.*(?=.{8,})(?=.*\\d)(?=.*[a-z])(?=.*[A-Z]).*$");
            Match m = passwordRegex.Match(tboxPassword1.Text.Trim());
            if (!m.Success || tboxPassword1.Text.Trim().Length < 8 || tboxPassword1.Text.Trim() != tboxPassword2.Text.Trim())
            {
                errorMsg += ".5";
                tboxPassword1.Background = scb;
                tboxPassword2.Background = scb;
                pboxPassword1.Background = scb;
                pboxPassword2.Background = scb;

            }
            else
            {
                tboxPassword1.Background = null;
                tboxPassword2.Background = null;
                pboxPassword1.Background = null;
                pboxPassword2.Background = null;
            }
            if (errorMsg != "")
            {
                BusyLoading.IsRunning = false;
                this.IsEnabled = true;
                return;
            }
            //end of client side checking (the same will be repeated on server side!)
            CreateER4ContactViaArchieCommand createComm = new CreateER4ContactViaArchieCommand(ArchieCode, ArchieStatus, tboxUsername.Text.Trim(), tboxEmail1.Text.Trim(),
                                                                tboxFirstname.Text.Trim() + " " + tboxLastname.Text.Trim(), tboxPassword1.Text.Trim()
                                                                , cbNoNewsletter.IsChecked == false, cbIncludeExampleReview.IsChecked == true);
            DataPortal<CreateER4ContactViaArchieCommand> dp = new DataPortal<CreateER4ContactViaArchieCommand>();
            dp.ExecuteCompleted += new EventHandler<DataPortalResult<CreateER4ContactViaArchieCommand>>(dp_ExecuteCompleted);
            dp.BeginExecute(createComm);
        }

        void dp_ExecuteCompleted(object sender, DataPortalResult<CreateER4ContactViaArchieCommand> e)
        {
            BusyLoading.IsRunning = false;
            this.IsEnabled = true;
            
            if (e.Error != null)
            {
                RadWindow.Alert("Unspecified Error:" + Environment.NewLine + "Please contact EPPISupport@ioe.ac.uk."
                    + Environment.NewLine + "Error Details:" + Environment.NewLine
                    + e.Error.Message);
            }
            else
            {
                CreateER4ContactViaArchieCommand res = e.Object as CreateER4ContactViaArchieCommand;
                if (res != null)
                {
                    if (res.Result == "Done")
                    {//all worked out fine, let's try to fetch our newly formed Ri
                        BusyLoading.IsRunning = true;
                        this.IsEnabled = false;
                        ReviewerPrincipal.Login(ArchieCode, ArchieStatus, "Archie0", 0, (o1, e1) =>
                            {
                                BusyLoading.IsRunning = false;
                                this.IsEnabled = true;
                                if (Csla.ApplicationContext.User.Identity.IsAuthenticated)
                                {
                                    CreateAccountStage1Gr.Visibility = System.Windows.Visibility.Collapsed;
                                    CreateAccountDoneGr.Visibility = System.Windows.Visibility.Visible;
                                    

                                    //if (LinkToArchieLoginSuccessful != null)
                                    //{
                                    //    LoginSuccessfulEventArgs LoginSEA = new LoginSuccessfulEventArgs((Csla.ApplicationContext.User.Identity as BusinessLibrary.Security.ReviewerIdentity).Name
                                    //                    ,ArchieCode, ArchieCode);
                                    //    LinkToArchieLoginSuccessful.Invoke(this, LoginSEA);
                                    //}

                                }
                                else
                                {//not good, a new account was created, but trying to fetch the resulting Ri failed. Reload and retry.
                                    RadWMOTD rw = new RadWMOTD();
                                    rw.Closed += new EventHandler<WindowClosedEventArgs>(rw_Closed);
                                    rw.cmdCloseMOTD_Clicked += (o, e2) =>
                                    {
                                        rw.Close();
                                    };
                                    rw.MOTDtextBlock.Text = "Error: your new account was created,"
                                        + Environment.NewLine + "but EPPI-Reviewer failed to automatically log you on."
                                        + Environment.NewLine + "Please check your inbox and Spam folders:"
                                        + Environment.NewLine + "you should receive an 'Account Activation' email shortly."
                                        + Environment.NewLine + "EPPI-Reviewer will reload when you close this window,"
                                        + Environment.NewLine + "you will need to re-logon manually.";
                                    rw.ShowDialog();
                                }
                            }
                        );
                    }
                    else
                    {   
                        //Name is missing or too short
                        //Username is missing or too short
                        //Email is missing or invalid
                        //Password is missing or invalid
                        //Username is already in use. Please select another.
                        //Email is already in use. Please select another or link to the (already) existing account.
                        //Failed to create account, please contact EPPISupport@ioe.ac.uk
                        //Account not created: failed to generate the activation link
                        //Account not created: failed to link to the Archie Identity

                        switch (res.Result)
                        {
                            case "Account not created: failed to link to the Archie Identity":
                                //most likely, the user took more than 60 minutes to complete this, kick user out
                                RadWMOTD rw = new RadWMOTD();
                                rw.Closed += new EventHandler<WindowClosedEventArgs>(rw_Closed);
                                rw.cmdCloseMOTD_Clicked += (o, e2) =>
                                {
                                    rw.Close();
                                };
                                rw.MOTDtextBlock.Text = "ERROR: No account was created."
                                    + Environment.NewLine + "Your Archie Identity could not be verified."
                                    + Environment.NewLine + "EPPI-Reviewer will reload when you close this window.";
                                rw.ShowDialog();
                                break;
                            case "Account not created: failed to generate the activation link":
                                RadWindow.Alert("Account not created: to prepare the Activation email."  + Environment.NewLine + "Please contact EPPISupport@ioe.ac.uk.");
                                break;
                            default:
                                RadWindow.Alert(res.Result  + Environment.NewLine + "Please amend your details and try again.");
                                break;
                        }



                    }
                }
                else
                {
                    RadWindow.Alert("Unspecified Error:" + Environment.NewLine + "Please contact EPPISupport@ioe.ac.uk.");
                }
            }
            
        }

        void rw_Closed(object sender, WindowClosedEventArgs e)
        {
            System.Windows.Browser.HtmlPage.Window.Invoke("Refresh");
        }

        private void btBack_Click(object sender, RoutedEventArgs e)
        {
            IntroGr.Visibility = System.Windows.Visibility.Visible;
            CreateAccountStage1Gr.Visibility = System.Windows.Visibility.Collapsed;
            CreateAccountDoneGr.Visibility = System.Windows.Visibility.Collapsed;
            LinkAccountStage1Gr.Visibility = System.Windows.Visibility.Collapsed;
            LinkAccountDoneGr.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void btCancel_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Browser.HtmlPage.Window.Invoke("Refresh");
        }

        private void pboxPassword2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key.ToString() == "Enter")
            {
                RoutedEventArgs rea = new RoutedEventArgs();
                btCheckandCreate_Click(sender, rea);
            }
        }

        
        
    }
}
