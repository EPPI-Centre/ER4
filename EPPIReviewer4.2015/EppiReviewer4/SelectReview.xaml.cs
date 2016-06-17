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
using Csla.DataPortalClient;
using Csla.Silverlight;
using Csla;
using BusinessLibrary;
using BusinessLibrary.BusinessClasses;
using Csla.Xaml;
using Telerik.Windows.Controls;
using BusinessLibrary.Security;
using System.Windows.Browser;

namespace EppiReviewer4
{
    public partial class SelectReview : UserControl
    {
        public event EventHandler<ReviewSelectedEventArgs> ReviewSelected;
        //CslaDataProvider provider;
        BusinessLibrary.Security.ReviewerIdentity ri;
        public SelectReview()
        {
            InitializeComponent();

            CslaDataProvider provider = App.Current.Resources["ReviewsData"] as CslaDataProvider;
            ri = Csla.ApplicationContext.User.Identity as BusinessLibrary.Security.ReviewerIdentity;
            if (ri.IsSiteAdmin)
            {
                ExpRow.Height = new GridLength( 30);
                LogonToRevIDstackp.Visibility = System.Windows.Visibility.Visible;
                LogonToRevIDBT.IsEnabled = true;
            }
            if (provider.FactoryParameters == null || provider.FactoryParameters.Count < 1 || provider.FactoryParameters[0].ToString() != ri.UserId.ToString())
            {
                provider.FactoryParameters.Clear();
                provider.FactoryParameters.Add(ri.UserId);
            }
            provider.FactoryMethod = "GetReviewList";
            provider.DataChanged += new EventHandler(CslaDataProvider_DataChanged);
            provider.Refresh();
            if (ri.IsCochraneUser)
            {
                ArchieReviewsTab.Visibility = System.Windows.Visibility.Visible;
                er4ReviewsTab.Visibility = System.Windows.Visibility.Visible;
                CslaDataProvider provider2 = App.Current.Resources["ArchieReviewsData"] as CslaDataProvider;
                provider2.DataChanged += new EventHandler(ArchieReviewsProvider_DataChanged);
                provider2.FactoryMethod = "GetReviewList";
                provider2.Refresh();
            }
            
        }

        private void CslaDataProvider_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Error" && ((Csla.Xaml.CslaDataProvider)sender).Error != null)
              System.Windows.Browser.HtmlPage.Window.Alert(((Csla.Xaml.CslaDataProvider)sender).Error.Message);
        }

        public void UnhookEvents()
        {
            CslaDataProvider provider = App.Current.Resources["ReviewsData"] as CslaDataProvider;
            provider.DataChanged -= CslaDataProvider_DataChanged;
            CslaDataProvider provider2 = App.Current.Resources["ArchieReviewsData"] as CslaDataProvider;
            provider2.DataChanged -= ArchieReviewsProvider_DataChanged;
        }

        private void CslaDataProvider_DataChanged(object sender, EventArgs e)
        {
            if (((Csla.Xaml.CslaDataProvider)sender).Error == null)
            {
                Telerik.Windows.Controls.RadWindow w = ((Grid)Parent).Parent as Telerik.Windows.Controls.RadWindow;
                if (w != null && GridViewReviewList.Items != null && ((Csla.Xaml.CslaDataProvider)sender).Data != null) w.ShowDialog();
            }
        }

        private void ActionButton_Click(object sender, RoutedEventArgs e)
        {
            GridViewReviewList.IsEnabled = false;
            TextBlockLoading.Visibility = Visibility.Visible;
            string reviewName = "";
            CslaDataProvider provider = App.Current.Resources["ReviewsData"] as CslaDataProvider;
            foreach (ReadOnlyReview ror in provider.Data as ReadOnlyReviewList)
            {
                if (ror.ReviewId == (int)((Button)sender).Tag)
                {
                    reviewName = ror.ReviewName;
                    break;
                }
            }
            if (ReviewSelected != null)
                ReviewSelected.Invoke(this, new ReviewSelectedEventArgs((int)((Button)sender).Tag, reviewName));
        }

        private void btCodeOnly_Click(object sender, RoutedEventArgs e)
        {
            GridViewReviewList.IsEnabled = false;
            TextBlockLoading.Visibility = Visibility.Visible;
            string reviewName = "";
            CslaDataProvider provider = App.Current.Resources["ReviewsData"] as CslaDataProvider;
            foreach (ReadOnlyReview ror in provider.Data as ReadOnlyReviewList)
            {
                if (ror.ReviewId == (int)((Button)sender).Tag)
                {
                    reviewName = ror.ReviewName;
                    break;
                }
            }
            if (ReviewSelected != null)
                ReviewSelected.Invoke(this, new ReviewSelectedEventArgs((int)((Button)sender).Tag, reviewName, "Coding only"));
        }
        private void RadGridView_DataLoaded(object sender, EventArgs e)
        {
            CslaDataProvider provider = App.Current.Resources["ReviewsData"] as CslaDataProvider;
            if (provider != null)
            {
                if ((provider.Data as ReadOnlyReviewList).Count == 0)
                {
                    //ExpRow.Height = new GridLength(30);
                    //MainRow.Height = new GridLength(0);
                    //MainRow.MaxHeight = 0;
                    GridViewReviewList.Visibility = System.Windows.Visibility.Collapsed;
                    createRevStack.Visibility = System.Windows.Visibility.Visible;

                    Telerik.Windows.Controls.RadWindow w = ((Grid)Parent).Parent as Telerik.Windows.Controls.RadWindow;
                    if (w != null)
                        w.Header = "Create New Review";
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            NewRevBT.IsEnabled = false;
            Review review = new Review(newReviewTB.Text, ri.UserId);
            if ((newReviewTB.Text != "") && (review != null))
            {
                review.Saved += (o, e2) =>
                {
                    if (e2.NewObject != null)
                    {
                        Review rv = e2.NewObject as Review;
                        ReviewSelected.Invoke(this, new ReviewSelectedEventArgs(rv.ReviewId, rv.ReviewName));
                    }
                    else
                        if (e2.Error != null)
                            Telerik.Windows.Controls.RadWindow.Alert(e2.Error.Message);
                    //BusyCreateNewReview.IsRunning = false;
                    //cmdCreateNewReview.IsEnabled = true;
                    //windowCreateReview.Close();
                };
                //BusyCreateNewReview.IsRunning = true;
                //cmdCreateNewReview.IsEnabled = false;
                review.BeginSave();
            }
            else
            {
                NewRevBT.IsEnabled = true;
                Telerik.Windows.Controls.RadWindow.Alert("Please enter a name for your review");
            }
        }

        private void newReviewTB_TextChanged(object sender, TextChangedEventArgs e)
        {
            NewRevBT.IsEnabled = true;
        }

        private void LogonToRevIDBT_Click(object sender, RoutedEventArgs e)
        {
            GridViewReviewList.IsEnabled = false;
            TextBlockLoading.Visibility = Visibility.Visible;
            int revID;
            int.TryParse(LogonToRevIDTB.Text, out revID);
            if (revID >0 && ri.IsSiteAdmin)
                ReviewSelected.Invoke(this, new ReviewSelectedEventArgs(-revID, ri));
        }
        private void LogonToRevIDTB_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key.ToString() == "Enter")
            {
                RoutedEventArgs rea = new RoutedEventArgs();
                LogonToRevIDBT_Click(sender, rea);
            }
        }

        private void ArchieReviewsProvider_DataChanged(object sender, EventArgs e)
        {
            GridViewArchieReviewList.IsEnabled = true;
            busyanimation.IsRunning = false;
            CslaDataProvider provider = sender as CslaDataProvider;
            if (provider == null)
            {//uh? how is this possible?
                RadWindow.Alert("Uknown error, please reload EPPI-Reviewer");
                return;
            }
            if (((Csla.Xaml.CslaDataProvider)sender).Error == null)
            {//we need to check if the user is able to talk with Archie, if it isn't we send the user down the authentication path
                ReadOnlyArchieReviewList rarl = provider.Data as ReadOnlyArchieReviewList;
                if (rarl == null)
                {
                    RadWindow.Alert("Uknown error, please reload EPPI-Reviewer");
                }
                else if (!rarl.IsAuthenticated)
                {//user could not get Archie data start the re-authentication route.
                    DialogParameters parameters = new DialogParameters();
                    parameters.Header = "Authenticate on Archie?";
                    parameters.Content = "In order to fetch the list of your registered Archie reviews,"
                        + Environment.NewLine + "your Archie authentication needs to be refreshed:"
                        + Environment.NewLine + "Please click 'OK' to authenticate on Archie now,"
                        + Environment.NewLine +"otherwise click 'Cancel'";
                    parameters.Closed = OnClosed;
                    RadWindow.Confirm(parameters);
                }
            }
        }
        private void OnClosed(object sender, WindowClosedEventArgs e)
        {
            if (e.DialogResult == true)
            {
                EppiReviewer4.App app = Application.Current as EppiReviewer4.App;
                app.jsb.AuthenticationIsDoneEvent -= jsb_AuthenticationIsDoneEvent;
                app.jsb.AuthenticationIsDoneEvent += new EventHandler<EventArgs>(jsb_AuthenticationIsDoneEvent);
                //minimal version, ask for authentication without explaining why. User experience needs to be better than this.
                HtmlPage.Window.Invoke("PopUpAndMonitor", ArchieConnector.NewRequest.ToString());
            }
        }
        void jsb_AuthenticationIsDoneEvent(object sender, EventArgs e)
        {
            EppiReviewer4.App app = Application.Current as EppiReviewer4.App;
            app.jsb.AuthenticationIsDoneEvent -= jsb_AuthenticationIsDoneEvent;
            Border bd = sender as Border;
            if (bd != null && bd.Tag != null && bd.Tag.ToString() != "" && bd.Resources["status"] != null)
            {//we have the data we need
                CslaDataProvider provider = App.Current.Resources["ArchieReviewsData"] as CslaDataProvider;
                provider.FactoryParameters.Add(new ReadOnlyArchieReviewListCriteria(bd.Tag.ToString(), bd.Resources["status"].ToString()));
                provider.FactoryMethod = "GetReviewList";
                provider.Refresh();
                return;
            }
            else
            {//authentication in Archie didn't work, or some other error.
                
            }
        }
        ArchieReviewPrepareCommand arpc;
        private void ActionButtonArchieCheckout_Click(object sender, RoutedEventArgs e)
        {
            ReadOnlyArchieReview  roar  = (sender as FrameworkElement).DataContext as ReadOnlyArchieReview;
            if (roar == null || roar.ArchieReviewId == null || roar.ArchieReviewId.Length < 18) return;
            arpc = new ArchieReviewPrepareCommand(roar.ArchieReviewId);
            DialogParameters parameters = new DialogParameters();
            parameters.Header = "Checkout Review?";
            parameters.Content = "Checking out a Review will make it"
                + Environment.NewLine + "editable in EPPI-Reviewer, but read-only"
                + Environment.NewLine + "in all other Cochrane Systems (Archie and RevMan)."
                + Environment.NewLine + "You should not check out this review if you are"
                + Environment.NewLine + "not planning to use EPPI-Reviewer as your"
                + Environment.NewLine + "chosen CAST tool."
                + Environment.NewLine + "Note that EPPI-Reviewer is currently unable"
                + Environment.NewLine + "to fetch study data from Archie/Revman/CRS"
                + Environment.NewLine + "(these features are under development).";
            parameters.Closed = OnConfirmCheckoutClosed;
            RadWindow.Confirm(parameters);

            
        }
        private void OnConfirmCheckoutClosed(object sender, WindowClosedEventArgs e)
        {
            if (e.DialogResult == true)
            {
                CheckoutSelectedArchieReview();
            }
        }
        private void CheckoutSelectedArchieReview()
        {
            if (arpc == null || arpc.ArchieReviewID == null || arpc.ArchieReviewID.Length < 18) return;

            GridViewArchieReviewList.IsEnabled = false;
            busyanimation.IsRunning = true;
            //arpc = new ArchieReviewPrepareCommand(roar.ArchieReviewId);
            DataPortal<ArchieReviewPrepareCommand> dp = new DataPortal<ArchieReviewPrepareCommand>();
            dp.ExecuteCompleted += (o, e2) =>
            {
                GridViewArchieReviewList.IsEnabled = true;
                busyanimation.IsRunning = false;
                if (e2.Error != null)
                {
                    RadWindow.Alert("Error, please reload EPPI-Reviewer"
                                    + Environment.NewLine + "Error details:"
                                    + Environment.NewLine + e2.Error);
                    return;
                }

                arpc = e2.Object as ArchieReviewPrepareCommand;
                if (arpc == null)
                {//uh? how is this possible?
                    RadWindow.Alert("Uknown error, please reload EPPI-Reviewer");
                    return;
                }
                if (!arpc.Identity.IsAuthenticated)
                {//user could not get Archie data start the re-authentication route.
                    DialogParameters parameters = new DialogParameters();
                    parameters.Header = "Authenticate on Archie?";
                    parameters.Content = "In order to operate on your Archie review"
                        + Environment.NewLine + "your Archie authentication needs to be refreshed:"
                        + Environment.NewLine + "Please click 'OK' to authenticate on Archie now,"
                        + Environment.NewLine + "otherwise click 'Cancel'";
                    parameters.Closed = OnClosed4checkout;
                    RadWindow.Confirm(parameters);
                }
                else
                {
                    refreshArchieReviews();
                }
            };
            dp.BeginExecute(arpc);
        }
        private void OnClosed4checkout(object sender, WindowClosedEventArgs e)
        {
            if (e.DialogResult == true)
            {
                EppiReviewer4.App app = Application.Current as EppiReviewer4.App;
                app.jsb.AuthenticationIsDoneEvent -= jsb_AuthenticationIsDoneEvent4checkout;
                app.jsb.AuthenticationIsDoneEvent += new EventHandler<EventArgs>(jsb_AuthenticationIsDoneEvent4checkout);
                app.ShowWaitingForArchieAuthentication();
                HtmlPage.Window.Invoke("PopUpAndMonitor", ArchieConnector.NewRequest.ToString());
            }
            else
            {
                arpc = null;//to remove stale stuff (could contain archie code and state?)
            }
        }
        void jsb_AuthenticationIsDoneEvent4checkout(object sender, EventArgs e)
        {
            EppiReviewer4.App app = Application.Current as EppiReviewer4.App;
            app.jsb.AuthenticationIsDoneEvent -= jsb_AuthenticationIsDoneEvent4checkout;
            Border bd = sender as Border;
            if (bd != null && bd.Tag != null && bd.Tag.ToString() != "" && bd.Resources["status"] != null)
            {//we have the data we need
                arpc = new ArchieReviewPrepareCommand(arpc.ArchieReviewID);
                arpc.ArchieCode = bd.Tag.ToString();
                arpc.ArchieState = bd.Resources["status"].ToString();
                CheckoutSelectedArchieReview();
                return;
            }
            else
            {//authentication in Archie didn't work, or some other error.

            }
        }


        ArchieReviewUndoCheckoutCommand arUco;
        private void ActionButtonArchieUndoCheckout_Click(object sender, RoutedEventArgs e)
        {
            ReadOnlyArchieReview roar = (sender as FrameworkElement).DataContext as ReadOnlyArchieReview;
            if (roar == null || roar.ArchieReviewId == null || roar.ArchieReviewId.Length < 18) return;
            GridViewArchieReviewList.IsEnabled = false;
            busyanimation.IsRunning = true;
            arUco = new ArchieReviewUndoCheckoutCommand(roar.ArchieReviewId);
            UndoCheckoutSelectedArchieReview();
        }

        private void UndoCheckoutSelectedArchieReview()
        {
            if (arUco == null || arUco.ArchieReviewID == null || arUco.ArchieReviewID.Length < 18) return;

            GridViewArchieReviewList.IsEnabled = false;
            busyanimation.IsRunning = true;
            DataPortal<ArchieReviewUndoCheckoutCommand> dp = new DataPortal<ArchieReviewUndoCheckoutCommand>();
            dp.ExecuteCompleted += (o, e2) =>
            {
                GridViewArchieReviewList.IsEnabled = true;
                busyanimation.IsRunning = false;
                if (e2.Error != null)
                {
                    RadWindow.Alert("Error, please reload EPPI-Reviewer"
                                    + Environment.NewLine + "Error details:"
                                    + Environment.NewLine + e2.Error);
                    return;
                }

                arUco = e2.Object as ArchieReviewUndoCheckoutCommand;
                if (arUco == null)
                {//uh? how is this possible?
                    RadWindow.Alert("Uknown error, please reload EPPI-Reviewer");
                    return;
                }
                //remove information that is either secret or stale
                arUco.ArchieCode = "";
                arUco.ArchieState = "";
                if (!arUco.Identity.IsAuthenticated)
                {//user could not get Archie data start the re-authentication route.
                    DialogParameters parameters = new DialogParameters();
                    parameters.Header = "Authenticate on Archie?";
                    parameters.Content = "In order to operate on your Archie review"
                        + Environment.NewLine + "your Archie authentication needs to be refreshed:"
                        + Environment.NewLine + "Please click 'OK' to authenticate on Archie now,"
                        + Environment.NewLine + "otherwise click 'Cancel'";
                    parameters.Closed = OnClosed4UndoCheckout;
                    RadWindow.Confirm(parameters);
                }
                else
                {
                    refreshArchieReviews();
                }
            };
            dp.BeginExecute(arUco);
        }
        private void OnClosed4UndoCheckout(object sender, WindowClosedEventArgs e)
        {
            if (e.DialogResult == true)
            {
                EppiReviewer4.App app = Application.Current as EppiReviewer4.App;
                app.jsb.AuthenticationIsDoneEvent -= jsb_AuthenticationIsDoneEvent4UndoCeckout;
                app.jsb.AuthenticationIsDoneEvent += new EventHandler<EventArgs>(jsb_AuthenticationIsDoneEvent4UndoCeckout);
                app.ShowWaitingForArchieAuthentication();
                HtmlPage.Window.Invoke("PopUpAndMonitor", ArchieConnector.NewRequest.ToString());
            }
            else
            {
                arUco = null;//to remove stale stuff (could contain archie code and state?)
            }
        }
        void jsb_AuthenticationIsDoneEvent4UndoCeckout(object sender, EventArgs e)
        {
            EppiReviewer4.App app = Application.Current as EppiReviewer4.App;
            app.jsb.AuthenticationIsDoneEvent -= jsb_AuthenticationIsDoneEvent4UndoCeckout;
            Border bd = sender as Border;
            if (bd != null && bd.Tag != null && bd.Tag.ToString() != "" && bd.Resources["status"] != null)
            {//we have the data we need
                arUco = new ArchieReviewUndoCheckoutCommand(arUco.ArchieReviewID);
                arUco.ArchieCode = bd.Tag.ToString();
                arUco.ArchieState = bd.Resources["status"].ToString();
                UndoCheckoutSelectedArchieReview();
                return;
            }
            else
            {//authentication in Archie didn't work, or some other error.

            }
        }


        private void refreshArchieReviews()
        {
            CslaDataProvider provider2 = App.Current.Resources["ArchieReviewsData"] as CslaDataProvider;
            provider2.FactoryMethod = "GetReviewList";
            GridViewArchieReviewList.IsEnabled = false;
            busyanimation.IsRunning = true;
            provider2.Refresh();
        }

        private void GridViewReviewList_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            
            if (GridViewReviewList != null && GridViewArchieReviewList != null)
            {
                double max = GridViewReviewList.ActualHeight > GridViewArchieReviewList.ActualHeight ? GridViewReviewList.ActualHeight : GridViewArchieReviewList.ActualHeight;
                selrevG.MinHeight = max;
                GridViewArchieReviewList.MinHeight = max;
            }
        }

        private void ActionButtonArchieOpen_Click(object sender, RoutedEventArgs e)
        {
            ReadOnlyArchieReview roar = (sender as FrameworkElement).DataContext as ReadOnlyArchieReview;
            if (roar == null || roar.ReviewId < 1) return;//access to any review (for admins) is done via the other tab
            //GridViewReviewList.IsEnabled = false;
            //GridViewArchieReviewList.IsEnabled = false;
            TextBlockLoading.Visibility = Visibility.Visible;
           ReviewSelected.Invoke(this, new ReviewSelectedEventArgs(roar.ReviewId, roar.ReviewName));
        }

        private void btArchieCodeOnly_Click(object sender, RoutedEventArgs e)
        {
            ReadOnlyArchieReview roar = (sender as FrameworkElement).DataContext as ReadOnlyArchieReview;
            if (roar == null || roar.ReviewId < 1) return;//access to any review (for admins) is done via the other tab
            //GridViewReviewList.IsEnabled = false;
            //GridViewArchieReviewList.IsEnabled = false;
            TextBlockLoading.Visibility = Visibility.Visible;
            ReviewSelected.Invoke(this, new ReviewSelectedEventArgs(roar.ReviewId, roar.ReviewName, "Coding only"));
        }

        
        
    }
}
