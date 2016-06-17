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
using BusinessLibrary.BusinessClasses;
using BusinessLibrary.Security;
using Csla;
using Telerik.Windows.Controls;
using Csla.Silverlight;
using System.ComponentModel;
using Csla.Xaml;
using System.Windows.Browser;

namespace EppiReviewer4
{
    public partial class App : Application, INotifyPropertyChanged

    {

        public event PropertyChangedEventHandler PropertyChanged; //INotifyPropertyChanged is needed to let the isEn resouce notice that the review has changed and therefore also write rights
        public void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        //jsb is used for arche authentication, it includes an event and code will hook it up when opening the Archie authentication popup
        public JavaScriptBridge jsb = new JavaScriptBridge();
        public RadWArchieAuthenticationSignal WaitForArchieWindow;
        public App()
        {
            this.Startup += this.Application_Startup;
            this.Exit += this.Application_Exit;
            this.UnhandledException += this.Application_UnhandledException;
            Telerik.Windows.Controls.StyleManager.ApplicationTheme = new Telerik.Windows.Controls.VistaTheme();
            //new Windows7Theme().IsApplicationTheme = true;
            //new VistaTheme().IsApplicationTheme = true;
            //new MetroTheme().IsApplicationTheme = true;
            //new SummerTheme().IsApplicationTheme = true;
            InitializeComponent();
            //to the xaml resources section!!
            ri = Csla.ApplicationContext.User.Identity as BusinessLibrary.Security.ReviewerIdentity;
            (App.Current.Resources["isEn"] as Button).DataContext = this;
            //end of read-only ui hack
            WaitForArchieWindow = new RadWArchieAuthenticationSignal();
        }
        //first bunch of lines to make the read-only UI work
        private BusinessLibrary.Security.ReviewerIdentity _ri;
        public BusinessLibrary.Security.ReviewerIdentity ri
        {
            get {return _ri;}
            set 
            {
                _ri = value;
                NotifyPropertyChanged("HasWriteRights");//this makes the isEn resouce notice that review has changed, therefore also HasWriteRights may be different.
            }
        }
        public bool HasWriteRights
        {
            get 
            {
                if (ri != null) return ri.HasWriteRights();
                else return false;
            }
        }
        //end of read-only ui hack
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // use compressed proxy
            Csla.DataPortal.ProxyTypeName = typeof(BusinessLibrary.Compression.CompressedProxy<>).AssemblyQualifiedName;

            this.RootVisual = new Page();
            //to use for ArchieAuthentication.
            HtmlPage.RegisterScriptableObject("jsb", jsb);
        }

        private void Application_Exit(object sender, EventArgs e)
        {

        }
        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            // If the app is running outside of the debugger then report the exception using
            // the browser's exception mechanism. On IE this will display it a yellow alert 
            // icon in the status bar and Firefox will display a script error.
            if (!System.Diagnostics.Debugger.IsAttached)
            {

                // NOTE: This will allow the application to continue running after an exception has been thrown
                // but not handled. 
                // For production applications this error handling should be replaced with something that will 
                // report the error to the website and stop the application.
                e.Handled = true;
                Deployment.Current.Dispatcher.BeginInvoke(delegate { ReportErrorToDOM(e); });
            }
        }
        private void ReportErrorToDOM(ApplicationUnhandledExceptionEventArgs e)
        {
            try
            {
                string errorMsg = e.ExceptionObject.Message + e.ExceptionObject.StackTrace;
                errorMsg = errorMsg.Replace('"', '\'').Replace("\r\n", @"\n");

                System.Windows.Browser.HtmlPage.Window.Eval("throw new Error(\"Unhandled Error in Silverlight 2 Application " + errorMsg + "\");");
            }
            catch (Exception)
            {
            }
        }

        private void CslaDataProvider_DataChanged(object sender, EventArgs e)
        {
            CslaDataProvider provider = ((CslaDataProvider)this.Resources["CodeSetsData"]);
            if (provider.Error != null)
                RadWindow.Alert(provider.Error.Message);
        }

        private void CslaDataProvider_SearchesDataDataChanged(object sender, EventArgs e)
        {
            CslaDataProvider provider = ((CslaDataProvider)this.Resources["SearchesData"]);
            if (provider.Error != null)
                RadWindow.Alert(((Csla.Xaml.CslaDataProvider)sender).Error.Message);
        }

        private void CslaDataProviderReportListData_DataChanged(object sender, EventArgs e)
        {
            CslaDataProvider provider = ((CslaDataProvider)this.Resources["ReportListData"]);
            if (provider.Error != null)
                RadWindow.Alert(((Csla.Xaml.CslaDataProvider)sender).Error.Message);
        }

        private void CslaDataProvider_AttributeTypesDataDataChanged(object sender, EventArgs e)
        {
            CslaDataProvider provider = ((CslaDataProvider)this.Resources["AttributeTypesData"]);
            if (provider.Error != null)
                RadWindow.Alert(((Csla.Xaml.CslaDataProvider)sender).Error.Message);
        }
        private void CslaDataProvider_ReviewsDataChanged(object sender, EventArgs e)
        {
            CslaDataProvider provider = ((CslaDataProvider)this.Resources["ReviewsData"]);
            if (provider.Error != null)
                RadWindow.Alert(((Csla.Xaml.CslaDataProvider)sender).Error.Message);
        }
        private void ArchieReviewsProvider_DataChanged(object sender, EventArgs e)
        {
            CslaDataProvider provider = ((CslaDataProvider)this.Resources["ArchieReviewsData"]);
            if (provider.Error != null)
                RadWindow.Alert(((Csla.Xaml.CslaDataProvider)sender).Error.Message);
        }
        private void CslaDataProvider_WorkAllocationContactListDataDataChanged(object sender, EventArgs e)
        {
            CslaDataProvider provider = ((CslaDataProvider)this.Resources["WorkAllocationContactListData"]);
            if (provider.Error != null)
                RadWindow.Alert(((Csla.Xaml.CslaDataProvider)sender).Error.Message);
        }
        private void CslaDataProvider_ReviewContactNVLDataDataChanged(object sender, EventArgs e)
        {
            CslaDataProvider provider = ((CslaDataProvider)App.Current.Resources["ReviewContactNVLData"]);
            if (provider.Error != null)
                RadWindow.Alert(((Csla.Xaml.CslaDataProvider)sender).Error.Message);
        }

        private void TrainingReviewerTermData_DataChanged(object sender, EventArgs e)
        {
            CslaDataProvider provider = ((CslaDataProvider)this.Resources["TrainingReviewerTermData"]);
            if (provider.Error != null)
            {
                System.Windows.Browser.HtmlPage.Window.Alert(((Csla.Xaml.CslaDataProvider)sender).Error.Message);
            }
        }

        private void TrainingListData_DataChanged(object sender, EventArgs e)
        {
            CslaDataProvider provider = ((CslaDataProvider)this.Resources["TrainingListData"]);
            if (provider.Error != null)
            {
                System.Windows.Browser.HtmlPage.Window.Alert(((Csla.Xaml.CslaDataProvider)sender).Error.Message);
            }
        }

        private void ReadOnlySetTypeList_DataChanged(object sender, EventArgs e)
        {
            CslaDataProvider provider = ((CslaDataProvider)App.Current.Resources["SetTypes"]);
            if (provider.Error != null)
                RadWindow.Alert(((Csla.Xaml.CslaDataProvider)sender).Error.Message);
        }
        public void ShowWaitingForArchieAuthentication()
        {
            jsb.AuthenticationIsDoneEvent -= jsb_AuthenticationIsDone_CloseWindow;
            jsb.AuthenticationIsDoneEvent += new EventHandler<EventArgs>(jsb_AuthenticationIsDone_CloseWindow);
            WaitForArchieWindow.ShowDialog();
        }
        
        void jsb_AuthenticationIsDone_CloseWindow(object sender, EventArgs e)
        {
            WaitForArchieWindow.Close();
        }

        private void CslaDataProvider_DataChanged_1(object sender, EventArgs e)
        {

        }

        private void CslaDataProvider_DataChanged_2(object sender, EventArgs e)
        {
            CslaDataProvider provider = ((CslaDataProvider)App.Current.Resources["ReviewInfoData"]);
            if (provider.Error != null)
                RadWindow.Alert(((Csla.Xaml.CslaDataProvider)sender).Error.Message);
        }

        public ReviewInfo GetReviewInfo()
        {
            CslaDataProvider Riprovider = App.Current.Resources["ReviewInfoData"] as CslaDataProvider;
            if (Riprovider != null)
            {
                return Riprovider.Data as ReviewInfo;
            }
            else
            {
                return null;
            }
        }
    }
    public class JavaScriptBridge
    {
        public event EventHandler<EventArgs> AuthenticationIsDoneEvent;
        [ScriptableMember()]
        public void SignalAuthenticationIsDone()
        {//this method gets executed by javascript when authentication in archie is done or failed
            if (AuthenticationIsDoneEvent != null)
            {
                //RadWindow.Alert("0: " + HtmlPage.Document.Cookies);
                string code = "", status = "", error = "", errorDescr = "";
                string[] cookies = HtmlPage.Document.Cookies.Split(';');
                int i = 0;
                foreach (string cookie in cookies)
                {
                    i++;
                    //RadWindow.Alert(i.ToString() + ": " + cookie);
                    if (!cookie.Contains("oAuthCochraneEPPI="))
                    {
                        continue;
                    }
                    else
                    {
                        string line = cookie.Replace("oAuthCochraneEPPI=", "");
                        //RadWindow.Alert(line);
                        string[] keyvalues = line.Split('&');
                        foreach (string keyValue in keyvalues)
                        {
                            string[] OneKeyVal = keyValue.Split('=');
                            if (OneKeyVal.Length == 2)
                            {
                                //RadWindow.Alert("key: " + OneKeyVal[0] + Environment.NewLine + "Val: " + OneKeyVal[1]);
                                if (OneKeyVal[0].Trim().ToLower() == "code")
                                {
                                    code = OneKeyVal[1];
                                }
                                else if (OneKeyVal[0].Trim().ToLower() == "state")
                                {
                                    status = OneKeyVal[1];
                                }
                                else if (OneKeyVal[0].Trim().ToLower() == "error")
                                {
                                    error = OneKeyVal[1];
                                }
                                else if (OneKeyVal[0].Trim().ToLower() == "error_description")
                                {
                                    errorDescr = OneKeyVal[1];
                                }

                            }
                        }
                        break;
                    }
                }
                EventArgs eA = new EventArgs();
                if (error != "")
                {//if the sender is an ellipse the data therein is an error
                    //what to do with this info depends on who's listening
                    Ellipse bd = new Ellipse();
                    bd.Tag = error;
                    bd.Resources.Add("error_description", errorDescr);
                    AuthenticationIsDoneEvent.Invoke(bd, eA);
                }
                else
                {//if the sender is a border, all is well
                    Border bd = new Border();
                    bd.Tag = code;
                    bd.Resources.Add("status", status);
                    //piece of code that tells the client to talk via HTTPS as archie strings should never travel in the clear
                    object o = Application.Current.Resources["UseHTTPSuntil"];
                    int Increment = 5;//time in seconds to perform all comms with server via https
                    if (o == null)
                    {
                        Application.Current.Resources.Add("UseHTTPSuntil", DateTime.Now.AddSeconds(Increment));//means that for the next 5 seconds all objects will be exchanged via https
                    }
                    else
                    {
                        DateTime? currentTr = o as DateTime?;
                        if (currentTr == null || DateTime.Now.AddSeconds(Increment) > currentTr)
                        {//do not reset the current time threshold if it means making it closer
                            //this check is necessary for when we'll enable a "always use https option" (sets Application.Current.Resources["UseHTTPSuntil"] one year in the future)
                            try
                            {
                                //Application.Current.Resources["UseHTTPSuntil"] = DateTime.Now.AddSeconds(5);
                                Application.Current.Resources.Remove("UseHTTPSuntil");
                                Application.Current.Resources.Add("UseHTTPSuntil", DateTime.Now.AddSeconds(Increment));
                            }
                            catch (Exception err)
                            {
                                Telerik.Windows.Controls.RadWindow.Alert(err.Message);
                                return;
                            }
                        }
                    }


                    AuthenticationIsDoneEvent.Invoke(bd, eA);
                    //RadWindow.Alert("so far so good");
                    
                }
                
            }
        }
    }
}
