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
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.Docking;
using BusinessLibrary.Security;
using Telerik.Windows;
using BusinessLibrary.BusinessClasses;
using Csla;

namespace EppiReviewer4
{
    public partial class Page : UserControl
    {
        LoginControl loginControl;
        SelectReview selectReview;
        private RadWindow windowReviews = new RadWindow();
        private Grid ReviewsGrid = new Grid();
        private RadWMOTD windowMOTD = new RadWMOTD();
        public Page()
        {
            InitializeComponent();
            windowReviews.Header = "Please select review";
            windowReviews.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
            windowReviews.ResizeMode = ResizeMode.NoResize;
            windowReviews.CanClose = false;
            windowReviews.CanMove = true;
            windowReviews.IsRestricted = true;
            windowReviews.RestrictedAreaMargin = new Thickness(10);
            ReviewsGrid.Height = double.NaN;
            ReviewsGrid.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            windowReviews.Height = double.NaN;
            windowReviews.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            
            ScrollViewer.SetVerticalScrollBarVisibility(windowReviews, ScrollBarVisibility.Auto );
            ScrollViewer.SetVerticalScrollBarVisibility(ReviewsGrid, ScrollBarVisibility.Auto);
            
            windowReviews.Content = ReviewsGrid;
            windowReviews.Closed += new EventHandler<WindowClosedEventArgs>(windowReviews_Closed);
            loginControl = new LoginControl();
            loginControl.LoginSuccessful += new EventHandler<LoginSuccessfulEventArgs>(login_LoginSuccessful);
            //real code used by actual users (not siteadmins only)
            loginControl.ArchieLoginSuccessful += new EventHandler<LoginSuccessfulEventArgs>(ArchieLogonControl_ArchieLoginSuccessful);
            loginControl.RestrictedAreaMargin = new Thickness(10);
            loginControl.IsRestricted = true;
            loginControl.ShowDialog();
            windowMOTD.cmdCloseMOTD_Clicked +=new EventHandler<RoutedEventArgs>(cmdCloseMOTD_Click);
            windowMOTD.windowMOTD_Close +=new EventHandler<WindowClosedEventArgs>(windowMOTD_Closed);
        }

        void windowReviews_Closed(object sender, WindowClosedEventArgs e)
        {
            //we need this because the review lists are in App.resources but the initial windows should stop caring about it.

            selectReview.UnhookEvents();
        }
        private DataPortal<CheckTicketExpirationCommand> TicketCheck;
        private CheckTicketExpirationCommand chk;
        //private int pendingchecks = 0;
        System.Windows.Threading.DispatcherTimer myDispatcherTimer;

        private string username;
        private string password;
        private string ArchieCode, ArchieStatus;

        private void ShowControl(UserControl control, StackPanel stackPanel)
        {
            while (stackPanel.Children.Count > 0)
                stackPanel.Children.Remove(stackPanel.Children[0]);
            stackPanel.Children.Add(control);
        }
        
        public homeDocuments _homeDocuments = null;
        public HomeCodingOnly _homeCodingOnly = null;
        void login_LoginSuccessful(object sender, LoginSuccessfulEventArgs e)
        {

            //after adding the Archie logon, we can hit this code in 3 situations
            //S1: as before, load list of reviews, let user in (username and pw were used and will be saved)
            //S2: user is a recognised ArchieUser, save ArchieCode, ArchieStatus for later use (when changing review)
            //S3: user is NOT a recognised ArchieUser and has re-provided username and password, ArchieCode, ArchieStatus were saved as well
            //in S3 we need to: link the current Ri to the archieID we do this in Ri.DataFetch by sending Uname, Pw, ArchieCode, ArchieStatus all in the same criteria!

            username = e.Username;
            password = e.Password;
            if (ArchieCode == null || ArchieCode == "") ArchieCode = e.Code;
            if (ArchieStatus == null || ArchieStatus == "") ArchieStatus = e.Status;
            if (App.Current.Resources.Contains("UserLogged"))
            {
                App.Current.Resources.Remove("UserLogged");
            }
            App.Current.Resources.Add("UserLogged", e);
            textBlockStatus.Text = "User: " + Csla.ApplicationContext.User.Identity.Name;
            selectReview = new SelectReview();
            System.Windows.Data.Binding bn = new System.Windows.Data.Binding("ActualHeight");
            bn.Source = LayoutRoot;
            windowReviews.SetBinding(MaxHeightProperty, bn);
            //windowReviews.MaxHeight = this.ActualHeight - 20;
            selectReview.ReviewSelected += new EventHandler<ReviewSelectedEventArgs>(selectReview_ReviewSelected);
            loginControl.Close();

            ReviewsGrid.Children.Add(selectReview);
            windowReviews.SizeToContent = false;
            windowReviews.Header = "Please select review";
            windowReviews.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            
            //for TESTING code: hook up to the logging on via Archie thing. When ready, this will happen against the same control, but placed within Logincontrol not SelectReview
            selectReview.ArchieLogonControl.ArchieLoginSuccessful -= ArchieLogonControl_ArchieLoginSuccessful;
            selectReview.ArchieLogonControl.ArchieLoginSuccessful += new EventHandler<LoginSuccessfulEventArgs>(ArchieLogonControl_ArchieLoginSuccessful);
            
            

        }

        void ArchieLogonControl_ArchieLoginSuccessful(object sender, LoginSuccessfulEventArgs e)
        {
            if (Csla.ApplicationContext.User.Identity.IsAuthenticated)
            {
                //bit of FOR testing code, things to do when the logon via archie button only appears to SiteAdmins after logging on once.
                if (selectReview != null)//already logged on...
                {
                    ReviewsGrid.Children.Clear();
                }
                //end of FOR testing
                //real produciton code, it will happen in Page.xaml.cs also in the final version
                //check if we know who this archie user is
                if (Csla.ApplicationContext.User.Identity.Name == "{UnidentifiedArchieUser}")
                {//we don't know who this user is, we need show a dialog to decide whether to create a new account or to link to an existing account (requires to log on again)
                    ArchieUserSetupNew ausn = new ArchieUserSetupNew();
                    if (ArchieCode == null || ArchieCode == "") ArchieCode = e.Code;
                    if (ArchieStatus == null || ArchieStatus == "") ArchieStatus = e.Status;
                    ausn.ArchieCode = ArchieCode;
                    ausn.ArchieStatus = ArchieStatus;
                    ausn.LinkToArchieLoginSuccessful += new EventHandler<LoginSuccessfulEventArgs>(ausn_LinkToArchieLoginSuccessful);
                    ReviewsGrid.Children.Add(ausn);
                    windowReviews.SizeToContent = true;
                    windowReviews.Header = "Linking Archie and EPPI-Reviewer Accounts";
                    if (loginControl.IsOpen)
                    {
                        loginControl.Close();
                    }
                    if (!windowReviews.IsOpen) windowReviews.ShowDialog();
                }
                else
                {
                    textBlockStatus.Text += "Welcome to ER4, " + Csla.ApplicationContext.User.Identity.Name;
                    login_LoginSuccessful(sender, e);
                }
            }
            else
            {
                textBlockStatus.Text = "Invalid login. Please try again.";
            }
        }

        void ausn_LinkToArchieLoginSuccessful(object sender, LoginSuccessfulEventArgs e)
        {
            //user has been logged on after successfully linking an archie account to a (new or existing) ER4 account
            //clean up a bit, then go back to the ArchieLoginSuccessful method
            ArchieUserSetupNew ausn = ReviewsGrid.FindChildByType<ArchieUserSetupNew>();
            if (ausn != null)
            {
                ausn.LinkToArchieLoginSuccessful -= ausn_LinkToArchieLoginSuccessful;
            }
            ausn = null;
            ReviewsGrid.Children.Clear();
            ArchieLogonControl_ArchieLoginSuccessful(sender, e);
        }

        void selectReview_ReviewSelected(object sender, ReviewSelectedEventArgs e)
        {
            if (password != null && password != "")
            {
                ReviewerPrincipal.Login(username, password, e.ReviewID, e.LoginMode, (o1, e1) =>
                {
                    if (Csla.ApplicationContext.User.Identity.IsAuthenticated)
                    {
                        windowReviews.Close();
                        SetUserReviewStatus(e);
                        BusinessLibrary.Security.ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as BusinessLibrary.Security.ReviewerIdentity;
                        if (ri.LoginMode != "Coding only")
                        {
                            _homeDocuments = new homeDocuments();
                            _homeDocuments.LoginToNewReviewRequested += new EventHandler<ReviewSelectedEventArgs>(homeDocuments_NewReviewSelected);
                            _gridDocuments.Children.Add(_homeDocuments);
                        }
                        else
                        {
                            _homeCodingOnly = new HomeCodingOnly();
                            _homeCodingOnly.LoginToNewReviewRequested += new EventHandler<ReviewSelectedEventArgs>(homeDocuments_NewReviewSelected);
                            _gridDocuments.Children.Add(_homeCodingOnly);
                        }
                    }
                    else
                    {
                        textBlockReview.Text = "Invalid login. Try again.";
                    }
                }
                );
            }
            else
            {
                ReviewerPrincipal.Login(ArchieCode, ArchieStatus , e.LoginMode, e.ReviewID,  (o1, e1) =>
                {
                    if (Csla.ApplicationContext.User.Identity.IsAuthenticated)
                    {
                        windowReviews.Close();
                        SetUserReviewStatus(e);
                        BusinessLibrary.Security.ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as BusinessLibrary.Security.ReviewerIdentity;
                        if (ri.LoginMode != "Coding only")
                        {
                            _homeDocuments = new homeDocuments();
                            _homeDocuments.LoginToNewReviewRequested += new EventHandler<ReviewSelectedEventArgs>(homeDocuments_NewReviewSelected);
                            _gridDocuments.Children.Add(_homeDocuments);
                        }
                        else
                        {
                            _homeCodingOnly = new HomeCodingOnly();
                            _homeCodingOnly.LoginToNewReviewRequested += new EventHandler<ReviewSelectedEventArgs>(homeDocuments_NewReviewSelected);
                            _gridDocuments.Children.Add(_homeCodingOnly);
                        }
                    }
                    else
                    {
                        textBlockReview.Text = "Invalid login. Try again.";
                    }
                }
                );
            }
            
        }
        void homeDocuments_NewReviewSelected(object sender, ReviewSelectedEventArgs e)
        {
            if (password != null && password != "")
            {//user is logged with ER4 credentials
                if (myDispatcherTimer != null) myDispatcherTimer.Stop();
                ReviewerPrincipal.Login(username, password, e.ReviewID, e.LoginMode, (o1, e1) =>
                {
                    if (Csla.ApplicationContext.User.Identity.IsAuthenticated)
                    {
                        ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as BusinessLibrary.Security.ReviewerIdentity;
                        SetUserReviewStatus(e);
                        if (_homeDocuments != null)
                        {
                            _homeDocuments.UnHookMe();
                            _homeDocuments = null;
                            _gridDocuments.Children.Remove(_homeDocuments);
                        }
                        else if (_homeCodingOnly != null)
                        {
                            //_homeCodingOnly.UnHookMe();
                            _homeCodingOnly = null;
                            _gridDocuments.Children.Remove(_homeCodingOnly);
                        }
                        if (ri.LoginMode != "Coding only")
                        {
                            _homeDocuments = new homeDocuments();
                            _homeDocuments.LoginToNewReviewRequested += new EventHandler<ReviewSelectedEventArgs>(homeDocuments_NewReviewSelected);
                            _homeDocuments.ri = Csla.ApplicationContext.User.Identity as BusinessLibrary.Security.ReviewerIdentity;
                            _gridDocuments.Children.Add(_homeDocuments);
                            _homeDocuments.DocumentListPane.SelectedIndex = 0;
                        }
                        else
                        {
                            _homeCodingOnly = new HomeCodingOnly();
                            _homeCodingOnly.LoginToNewReviewRequested += new EventHandler<ReviewSelectedEventArgs>(homeDocuments_NewReviewSelected);
                            _gridDocuments.Children.Add(_homeCodingOnly);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Error logging in to new review. Please refresh your browser page.");
                    }
                    if (myDispatcherTimer != null) myDispatcherTimer.Start();
                });
            }
            else
            {//user is logged with ArchieCredentials
                ReviewerPrincipal.Login(ArchieCode, ArchieStatus , e.LoginMode, e.ReviewID,  (o1, e1) =>
                {
                    if (Csla.ApplicationContext.User.Identity.IsAuthenticated)
                    {
                        ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as BusinessLibrary.Security.ReviewerIdentity;
                        SetUserReviewStatus(e);
                        if (_homeDocuments != null)
                        {
                            _homeDocuments.UnHookMe();
                            _homeDocuments = null;
                            _gridDocuments.Children.Remove(_homeDocuments);
                        }
                        else if (_homeCodingOnly != null)
                        {
                            _homeCodingOnly.UnHookMe();
                            _homeCodingOnly = null;
                            _gridDocuments.Children.Remove(_homeCodingOnly);
                        }
                        if (ri.LoginMode != "Coding only")
                        {
                            _homeDocuments = new homeDocuments();
                            _homeDocuments.LoginToNewReviewRequested += new EventHandler<ReviewSelectedEventArgs>(homeDocuments_NewReviewSelected);
                            _homeDocuments.ri = Csla.ApplicationContext.User.Identity as BusinessLibrary.Security.ReviewerIdentity;
                            _gridDocuments.Children.Add(_homeDocuments);
                            _homeDocuments.DocumentListPane.SelectedIndex = 0;
                        }
                        else
                        {
                            _homeCodingOnly = new HomeCodingOnly();
                            _homeCodingOnly.LoginToNewReviewRequested += new EventHandler<ReviewSelectedEventArgs>(homeDocuments_NewReviewSelected);
                            _gridDocuments.Children.Add(_homeCodingOnly);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Error logging in to new review. Please refresh your browser page.");
                    }
                    if (myDispatcherTimer != null) myDispatcherTimer.Start();
                });
            }
        }
        protected void SetUserReviewStatus(ReviewSelectedEventArgs e)
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as BusinessLibrary.Security.ReviewerIdentity;
            StartTimer(this, null);
            textBlockReview.Text = "Review: " + e.ReviewName;
            if (ri.DaysLeftReview < 0)
            {
                textBlockReview.Text += " (Expired)"; 
                Color mycolor = new Color();
                mycolor.A = 255;
                mycolor.B = 100;
                mycolor.G = 100;
                mycolor.R = 255;
                CurrentReviewStatusContainer.Background = new SolidColorBrush(mycolor);
                ToolTipService.SetToolTip(textBlockReview, null);
            }
            else if (ri.DaysLeftReview <= 1)
            {
                ToolTip toolTip = new ToolTip();
                toolTip.Content = "This Review will expire" + (ri.DaysLeftReview == 0 ? " today!" : " tomorrow!");
                ToolTipService.SetToolTip(textBlockReview, toolTip);
                Color mycolor = new Color();
                mycolor.A = 255;
                mycolor.B = 100;
                mycolor.G = 100;
                mycolor.R = 255;
                CurrentReviewStatusContainer.Background = new SolidColorBrush(mycolor);
            }
            else if (ri.DaysLeftReview < 14)
            {
                ToolTip toolTip = new ToolTip();
                toolTip.Content = "Your Review will expire in " + ri.DaysLeftReview + " days!";
                ToolTipService.SetToolTip(textBlockReview, toolTip);
                CurrentReviewStatusContainer.Background = new SolidColorBrush(Colors.Yellow);
            }
            else
            {
                CurrentReviewStatusContainer.Background = null;
                ToolTipService.SetToolTip(textBlockReview, null);
            }
            if (ri.DaysLeftAccount < 0)
            {
                Color mycolor = new Color();
                mycolor.A = 255;
                mycolor.B = 100;
                mycolor.G = 100;
                mycolor.R = 255;
                CurrentUserStatusContainer.Background = new SolidColorBrush(mycolor);
                textBlockStatus.Text = "User: " + Csla.ApplicationContext.User.Identity.Name + " (Expired)";
                ToolTipService.SetToolTip(textBlockStatus, null);
            }
            else if (ri.DaysLeftAccount <= 1)
            {
                ToolTip toolTip = new ToolTip();
                toolTip.Content = "Your Account will expire" + (ri.DaysLeftAccount == 0 ? " today!" : " tomorrow!");
                ToolTipService.SetToolTip(textBlockStatus, toolTip);
                Color mycolor = new Color(); 
                mycolor.A = 255;
                mycolor.B = 100;
                mycolor.G = 100;
                mycolor.R = 255;
                CurrentUserStatusContainer.Background = new SolidColorBrush(mycolor);
            }
            else if (ri.DaysLeftAccount < 14)
            {
                ToolTip toolTip = new ToolTip();
                toolTip.Content = "Your account will expire in " + ri.DaysLeftAccount + " days!";
                ToolTipService.SetToolTip(textBlockStatus, toolTip);
                CurrentUserStatusContainer.Background = new SolidColorBrush(Colors.Yellow);
            }
            else
            {
                CurrentUserStatusContainer.Background = null;
                ToolTipService.SetToolTip(textBlockStatus, null);
            }
        }


        public void StartTimer(object o, RoutedEventArgs sender)
        {
            //System.Reflection.AssemblyName assemblyName = new System.Reflection.AssemblyName(System.Reflection.Assembly.GetExecutingAssembly().FullName);
            //textBlockVersion.Text = " | V:" + assemblyName.Version;
            myDispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            myDispatcherTimer.Interval = new TimeSpan(0, 0, 0, 30, 0); // 30 seconds
            myDispatcherTimer.Tick += new EventHandler(Each_Tick);
            myDispatcherTimer.Start();
        }

        // A variable to count with.
        //int i = 0;

        // Fires every myDispatcherTimer.Interval while the DispatcherTimer is active.
        public void Each_Tick(object o, EventArgs sender)
        {
            
            //textBlockMsg.Text = " | Ticks Count up: " + i++.ToString();
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as BusinessLibrary.Security.ReviewerIdentity;
            
            if (ri != null && ri.ReviewId != 0)
            {
                if (chk == null)
                    chk = new CheckTicketExpirationCommand(ri.UserId, ri.Ticket);
                else chk.GUID = ri.Ticket;
                //if (TicketCheck == null)
                //{
                //we should create a new dataportal to make sure comms go via http or https depending on setting, otherwise they are fixed at when the timer starts
                if (TicketCheck != null) TicketCheck.ExecuteCompleted -= TicketCheck_ExecuteCompleted;
                TicketCheck = new DataPortal<CheckTicketExpirationCommand>();
                TicketCheck.ExecuteCompleted += new EventHandler<DataPortalResult<CheckTicketExpirationCommand>>(TicketCheck_ExecuteCompleted);
                //}
                TicketCheck.BeginExecute(chk);
            }
        }

        void TicketCheck_ExecuteCompleted(object sender, DataPortalResult<CheckTicketExpirationCommand> e)
        {
            if (e.Error == null)
            {
                chk = (CheckTicketExpirationCommand)e.Object;
                if (chk.Result == "Valid")
                {
                    UpdateStatus(chk.ServerMessage);
                }
                else
                {
                    string msg = "Sorry, you have been logged off automatically." + Environment.NewLine;
                    switch (chk.Result)
                    {
                        case "Expired":
                            msg += "Your session has been inactive for too long." + Environment.NewLine;
                            break;
                        case "Invalid":
                            msg += "Someone has logged on with the same credentials you are using." + Environment.NewLine;
                            msg += "This is not allowed in ER4. If you believe that someone is using your credentials without permission, ";
                            msg += "you should contact the ER4 support." + Environment.NewLine;
                            break;
                        case "None":
                            msg += "Your session has become invalid for unrecognised reasons (Return code = NONE)." + Environment.NewLine;
                            msg += "Please contact the ER4 support team." + Environment.NewLine;
                            break;
                        case "Multiple":
                            BusinessLibrary.Security.ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as BusinessLibrary.Security.ReviewerIdentity;
                            if (ri.IsCochraneUser)
                            {
                                msg += "Your session has become invalid." + Environment.NewLine;
                                msg += "Most likely, the Cochrane review you have open has become 'Read-Only'." + Environment.NewLine;
                                msg += "This would happen if the review got Checked-In in Archie" + Environment.NewLine;
                                msg += "(or someone undid the check-out)." + Environment.NewLine;
                                msg += "If you think this wasn't the case, please contact EPPISupport." + Environment.NewLine;
                            }
                            else
                            {
                                msg += "Your session has become invalid for unrecognised reasons (Return code = MULTIPLE)." + Environment.NewLine;
                                msg += "Please contact the ER4 support team." + Environment.NewLine;
                            }
                            break;
                    }
                    string res = MessageBox.Show(msg + "You will be asked to logon again when you close this message.").ToString();
                    System.Windows.Browser.HtmlPage.Window.Invoke("Refresh");
                }
            }
            else
            {
                if (e.Error.GetType() == (new System.Reflection.TargetInvocationException(new Exception()).GetType()))
                {
                    UpdateStatus("!You have lost the connection with our server, please check your Internet connection." + Environment.NewLine +
                        "This message will revert to normal when the connection will be re-established:" + Environment.NewLine +
                        "Please keep in mind that data changes made while disconnected cannot be saved." + Environment.NewLine +
                        "If your Internet connection is working, we might be experiencing some technical problems," + Environment.NewLine +
                        "We apologise for the inconvenience.");
                    return;
                }
                //string res = 338954
                windowMOTD.Tag = "failure";
                windowMOTD.MOTDtextBlock.Text = "We are sorry, you have lost communication with the server. To avoid data corruption, the page will now reload."
                    + Environment.NewLine + "This message may appear if you didn't log out during a software update."
                    + Environment.NewLine + "Note that Eppi-Reviewer might fail to load until the update is completed, please wait a couple of minutes and try again.";
                windowMOTD.Show();
               
            }
        }
        void UpdateStatus(string Message)
        {
            string msgSt = "";
            if (Message.Substring(0, 1) == "!")
            {
                StatusContainer.Background = new SolidColorBrush(Colors.Yellow);
                msgSt = "Status: " + Message.Substring(1).Trim();
            }
            else
            {//textBlockMsg.Text 
                msgSt = "Status: " + Message;
                StatusContainer.Background = null;
            }
            if (msgSt.Length > 80)
            {
                int ii = msgSt.LastIndexOf(" ", 80, 79);
                string tmpStr = msgSt.Substring(0, msgSt.LastIndexOf(" ", 80, 79)) + "...";
                windowMOTD.MOTDtextBlock.Text = msgSt.Replace(@"\n", Environment.NewLine);
                textBlockMsg.Text = tmpStr.Replace(@"\n", "");
                viewFullMOTD_hlink.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                viewFullMOTD_hlink.Visibility = System.Windows.Visibility.Collapsed;
                textBlockMsg.Text = msgSt.Replace(@"\n", "");
            }
        }
        private void cmdCloseMOTD_Click(object sender, RoutedEventArgs e)
        {
            windowMOTD.Close();
        }

        private void viewFullMOTD_hlink_Click(object sender, RoutedEventArgs e)
        {
            windowMOTD.ShowDialog();
        }

        private void windowMOTD_Closed(object sender, WindowClosedEventArgs e)
        {
            if (windowMOTD.Tag != null && windowMOTD.Tag.ToString() == "failure")
                System.Windows.Browser.HtmlPage.Window.Invoke("Refresh");
        }

    }
}
