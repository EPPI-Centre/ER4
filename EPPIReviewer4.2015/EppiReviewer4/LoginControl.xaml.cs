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
using Telerik.Windows;
using Telerik.Windows.Controls;
using BusinessLibrary.BusinessClasses;
using Csla;

namespace EppiReviewer4
{
    public partial class LoginControl : RadWindow
    {
        public LoginControl()
        {
            InitializeComponent();
            NormalLogin.Visibility = System.Windows.Visibility.Visible;
            ArchieLogin.Visibility = System.Windows.Visibility.Collapsed;
            //remove from here!
            
            //end of remove!
            LoadMessage();
            MainArchieLogin.ArchieLoginSuccessful += new EventHandler<LoginSuccessfulEventArgs>(MainArchieLogin_ArchieLoginSuccessful);
        }
        public LoginSuccessfulEventArgs LoginSEA;
        public event EventHandler<LoginSuccessfulEventArgs> LoginSuccessful;
        public event EventHandler<LoginSuccessfulEventArgs> ArchieLoginSuccessful;
        
        void MainArchieLogin_ArchieLoginSuccessful(object sender, LoginSuccessfulEventArgs e)
        {
            LoginSEA = e;
            if (ArchieLoginSuccessful != null)
            {
                ArchieLoginSuccessful.Invoke(this, LoginSEA);
            }
        }
        private void LoadMessage()
        {
            DataPortal<GetLatestUpdateMsgCommand> dp = new DataPortal<GetLatestUpdateMsgCommand>();
            GetLatestUpdateMsgCommand command = new GetLatestUpdateMsgCommand();
            dp.ExecuteCompleted += (o, e2) =>
            {
                //System.Threading.Thread.CurrentThread.Join(3600);
                PreAnimation.Visibility = System.Windows.Visibility.Collapsed;
                PreAnimation.IsRunning = false;
                if (e2.Error != null)
                {//ouch, something is wrong: our first chat with the DB failed...
                    //txtLatest.Text = "AAA " + command.Date + "|" + e2.Error.Message + Environment.NewLine;
                    txtLatest.Text = "ERROR: could not communicate with DataBase." + Environment.NewLine + "This is usually due to some technical difficulty: logging-on may fail as a consequence."
                        + Environment.NewLine + "Please try reloading this page in a few minutes. If the problem persists, please contact EPPISupport@ioe.ac.uk and/or check our Twitter feed (@EPPIReviewer).";
                    BorderLatest.Background = new SolidColorBrush(Colors.Yellow);
                }
                else
                {
                    command = e2.Object as GetLatestUpdateMsgCommand;
                    if (command != null)
                    {
                        DateTime now = DateTime.Now;
                        System.Globalization.CultureInfo culture = new System.Globalization.CultureInfo("en-GB");
                        DateTime then = DateTime.Parse(command.Date, culture.DateTimeFormat);
                        System.TimeSpan diff = now - then;
                        txtDate.Text = then.ToString("MMM dd, yyyy");
                        if (diff.Days > 14)//message is old, remove animation
                        {
                            //storyBoard = null;
                            AnCol.RepeatBehavior = new RepeatBehavior(0);
                            AnSize.RepeatBehavior = new RepeatBehavior(0);
                        }
                        hlbURL.NavigateUri = new Uri(command.URL, UriKind.Absolute);
                        hlbURL.Visibility = System.Windows.Visibility.Visible;
                        txtLatest.Text = command.Description;
                        string vNumber = command.VersionN;
                        if (vNumber.StartsWith("6."))
                        {
                            vNumber = "4" + vNumber.Substring(1);
                        }
                        txtVer.Text = "Version: " + vNumber;
                    }
                }
            };
            PreAnimation.Visibility = System.Windows.Visibility.Visible;
            PreAnimation.IsRunning = true;
            dp.BeginExecute(command);
        }

        private void LogInButton_Click(object sender, RoutedEventArgs e)
        {
            LogInButton.IsEnabled = false;
            Status.Text = "Validating credentials...";
            animation.Visibility = Visibility.Visible;
            Status.Visibility = Visibility.Visible;
            animation.IsRunning = true;
            UserIdBox.IsReadOnly = true;
            UserPwdBox.IsEnabled = false;
            ReviewerPrincipal.Login(UserIdBox.Text.Trim(), UserPwdBox.Password.Trim(), 0, "aa", (o1, e1) =>
            {
                animation.IsRunning = false;
                UserIdBox.IsReadOnly = false;
                UserPwdBox.IsEnabled = true;
                if (Csla.ApplicationContext.User.Identity.IsAuthenticated)
                {
                    Status.Text = "User authenticated.";
                    Status.Text += " Welcome to ER4, " + Csla.ApplicationContext.User.Identity.Name;
                    if (LoginSuccessful != null)
                    {
                        LoginSEA = new LoginSuccessfulEventArgs(UserIdBox.Text.Trim(), UserPwdBox.Password.Trim());
                        LoginSuccessful.Invoke(this, LoginSEA);
                    }
                }
                else
                {
                    Status.Text = "Invalid login. Please try again.";
                    LogInButton.IsEnabled = true;
                }
            }
            );
        }

        private void UserPwdBox_GotFocus(object sender, RoutedEventArgs e)
        {
            UserPwdBox.SelectAll();
        }

        private void UserPwdBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key.ToString() == "Enter")
            {
                RoutedEventArgs rea = new RoutedEventArgs();
                LogInButton_Click(sender, rea);
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            NormalLogin.Visibility = System.Windows.Visibility.Visible;
            ArchieLogin.Visibility = System.Windows.Visibility.Collapsed;

        }

        private void swithcToArche_Click(object sender, RoutedEventArgs e)
        {
            NormalLogin.Visibility = System.Windows.Visibility.Collapsed;
            ArchieLogin.Visibility = System.Windows.Visibility.Visible;
        }

        

    }
}
