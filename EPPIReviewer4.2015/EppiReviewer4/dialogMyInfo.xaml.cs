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
using Telerik.Windows.Controls.GridView;
using BusinessLibrary.BusinessClasses;
using Csla.Silverlight;
using Csla.Xaml;
using Telerik.Windows.Controls;
using System.Windows.Browser;
using Csla;

namespace EppiReviewer4
{
    public partial class dialogMyInfo : UserControl
    {
        public event EventHandler<ReviewSelectedEventArgs> LoginToNewReviewRequested;
        public event EventHandler<MouseEventArgs> MouseDownOnMyInfoWorkAllocation;
        public event EventHandler<EventArgs> GridViewMyWorkAllocation_DataLoad;
        public event EventHandler<EventArgs> GridViewMyReviews_DataLoad;
        public event EventHandler<EventArgs> cmdStartScreening_Clicked;
        //first bunch of lines to make the read-only UI work
        public BusinessLibrary.Security.ReviewerIdentity ri;
        public bool HasWriteRights
        {
            get
            {
                if (ri == null) return false;
                else return ri.HasWriteRights();
            }
        }
        //end of read-only ui hack
        private RadWCreateReview windowCreateReview;


        public dialogMyInfo()
        {
            InitializeComponent();
            //add <Button x:Name="isEn" IsEnabled="{Binding HasWriteRights, Mode=OneWay}" ></Button>
            //to the xaml resources section!!
            ri = Csla.ApplicationContext.User.Identity as BusinessLibrary.Security.ReviewerIdentity;
            isEn.DataContext = this;
            //end of read-only ui hack
            
            if (ri != null && ri.Ticket != null)
            {
                cmdShowWindowCreateReview.IsEnabled = ri.DaysLeftAccount >= 0;
                windowCreateReview = new RadWCreateReview();
                if (ri.AccountExpiration == new DateTime(3001, 1, 1))
                    AccountExpirationTBMyInfo.Text = "Account Expiration Unkown (logged on as site admin)";
                else AccountExpirationTBMyInfo.Text = "Your account expires on " + ri.AccountExpiration.ToShortDateString() + ".";
                if (ri.ReviewExpiration == new DateTime(3000, 1, 1))
                    ReviewExpirationTBMyInfo.Text = "Current Review is private (Expires with your account)";
                else if (ri.ReviewExpiration == new DateTime(3001, 1, 1))
                    ReviewExpirationTBMyInfo.Text = "Review Expiration Unkown (logged on as site admin)";
                else ReviewExpirationTBMyInfo.Text = "Current (shared) review expires on " + ri.ReviewExpiration.ToShortDateString() + ".";
            
            
                windowCreateReview.cmdCreateNewReview_Clicked +=new EventHandler<RoutedEventArgs>(cmdCreateNewReview_Click);
            
                GridViewMyWorkAllocation.AddHandler(GridViewCell.MouseLeftButtonDownEvent, new MouseButtonEventHandler(MouseDownOnWorkAllocation), true);
                //RefreshReviewList();
                if (ri.IsCochraneUser)
                {
                    ArchieReviewsTab.Visibility = System.Windows.Visibility.Visible;
                    er4ReviewsTab.Visibility = System.Windows.Visibility.Visible;
                }
            }

        }
        
        public void setForCodingOnly()
        {
            Tabhost.SetValue(Grid.RowProperty, 3);
            GridViewMyWorkAllocation.SetValue(Grid.RowProperty, 1);
            txtAlloc.SetValue(Grid.RowProperty, 0);
            //txtRevs.SetValue(Grid.RowProperty, 2);
            //if (ri.ShowScreening) cmdStartScreening.Visibility = System.Windows.Visibility.Visible;
        }
        private void cmdSelectReview_Click(object sender, RoutedEventArgs e)
        {
            GridViewMyReviews.IsEnabled = false;
            int reviewId = ((sender as Button).DataContext as ReadOnlyReview).ReviewId;
            string reviewName = ((sender as Button).DataContext as ReadOnlyReview).ReviewName;
            //App a = App.Current as EppiReviewer4.App;
            //Page pg = (Page)((Grid)((Grid)this.Parent).Parent).Parent;
            //pg.LoadCodes();
            if (LoginToNewReviewRequested != null)
                LoginToNewReviewRequested.Invoke(this, new ReviewSelectedEventArgs(reviewId, reviewName));
            //DocumentListPane.SelectedIndex = 0;
        }
        private void btCodeOnly_Click(object sender, RoutedEventArgs e)
        {
            GridViewMyReviews.IsEnabled = false;
            int reviewId = ((sender as Button).DataContext as ReadOnlyReview).ReviewId;
            string reviewName = ((sender as Button).DataContext as ReadOnlyReview).ReviewName;
            //App a = App.Current as EppiReviewer4.App;
            //Page pg = (Page)((Grid)((Grid)this.Parent).Parent).Parent;
            //pg.LoadCodes();
            if (LoginToNewReviewRequested != null)
                LoginToNewReviewRequested.Invoke(this, new ReviewSelectedEventArgs(reviewId, reviewName, "Coding only"));
        }
        private void cmdShowWindowCreateReview_Click(object sender, RoutedEventArgs e)
        {
            Review review = new Review();
            review.ContactId = ri.UserId;
            windowCreateReview.GridCreateReview.DataContext = review;
            windowCreateReview.ShowDialog();
        }
        private void cmdCreateNewReview_Click(object sender, RoutedEventArgs e)
        {
            //Review review = GridCreateReview.DataContext as Review;
            Review review = new Review(windowCreateReview.TextBoxNewReviewName.Text, ri.UserId);
            if ((windowCreateReview.TextBoxNewReviewName.Text != "") && (review != null))
            {
                review.Saved += (o, e2) =>
                {
                    if (e2.NewObject != null)
                    {
                        Review rv = e2.NewObject as Review;
                        RefreshReviewList();
                        // (App.Current.Resources["CodeSetsData"] as CslaDataProvider).Refresh(); is this needed?? removed for now.
                    }
                    else
                        if (e2.Error != null)
                            MessageBox.Show(e2.Error.Message);
                    windowCreateReview.BusyCreateNewReview.IsRunning = false;
                    windowCreateReview.cmdCreateNewReview.IsEnabled = true;
                    windowCreateReview.Close();
                };
                windowCreateReview.BusyCreateNewReview.IsRunning = true;
                windowCreateReview.cmdCreateNewReview.IsEnabled = false;
                review.BeginSave();
            }
            else
            {
                MessageBox.Show("Please enter a name for your review");
            }
        }
        
        public void MouseDownOnWorkAllocation(object sender, MouseEventArgs args)
        {
            if (MouseDownOnMyInfoWorkAllocation != null) MouseDownOnMyInfoWorkAllocation.Invoke(sender, args);
        }

        private void GridViewMyWorkAllocation_DataLoaded(object sender, EventArgs e)
        {
            if (GridViewMyWorkAllocation_DataLoad != null) GridViewMyWorkAllocation_DataLoad.Invoke(sender, e);
        }

        private void cmdStartScreening_Click(object sender, RoutedEventArgs e)
        {
            if (cmdStartScreening_Clicked != null) cmdStartScreening_Clicked.Invoke(sender, e); 
        }

        private void GridViewMyReviews_DataLoaded(object sender, EventArgs e)
        {
            if (GridViewMyReviews_DataLoad != null) GridViewMyReviews_DataLoad.Invoke(sender, e);
        }
        private void ArchieReviewsProvider_DataChanged(object sender, EventArgs e)
        {
            GridViewArchieReviewList.IsEnabled = true;
            //busyanimation.IsRunning = false;
            CslaDataProvider provider = sender as CslaDataProvider;
            if (provider == null)
            {//uh? how is this possible?
                RadWindow.Alert("Uknown error, please reload EPPI-Reviewer");
                return;
            }
            provider.DataChanged -= ArchieReviewsProvider_DataChanged;
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
                        + Environment.NewLine + "otherwise click 'Cancel'";
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
            ReadOnlyArchieReview roar = (sender as FrameworkElement).DataContext as ReadOnlyArchieReview;
            if (roar == null || roar.ArchieReviewId == null || roar.ArchieReviewId.Length < 18) return;
            arpc = new ArchieReviewPrepareCommand(roar.ArchieReviewId);
            DialogParameters parameters = new DialogParameters();
            parameters.Header = "Activate Review?";
            parameters.Content = "This creates the review record in the "
                + Environment.NewLine + "EPPI-Reviewer database."
                + Environment.NewLine + "It currently does not import"
                + Environment.NewLine + "any review data from RevMan/Archie."
                + Environment.NewLine
                + Environment.NewLine + "Activating this review will also"
                + Environment.NewLine + "give you (its first user, in EPPI-Reviewer)"
                + Environment.NewLine + "the 'Review Administrator' role.";
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
            //busyanimation.IsRunning = true;
            DataPortal<ArchieReviewPrepareCommand> dp = new DataPortal<ArchieReviewPrepareCommand>();
            dp.ExecuteCompleted += (o, e2) =>
            {
                GridViewArchieReviewList.IsEnabled = true;
                //busyanimation.IsRunning = false;
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
        private void ActionButtonArchieUndoCheckout_Click(object sender, RoutedEventArgs e)
        {
            ReadOnlyArchieReview roar = (sender as FrameworkElement).DataContext as ReadOnlyArchieReview;
            if (roar == null || roar.ArchieReviewId == null || roar.ArchieReviewId.Length < 18) return;
            GridViewArchieReviewList.IsEnabled = false;
            //busyanimation.IsRunning = true;
            arUco = new ArchieReviewUndoCheckoutCommand(roar.ArchieReviewId);
            UndoCheckoutSelectedArchieReview();
        }

        ArchieReviewUndoCheckoutCommand arUco;
        private void UndoCheckoutSelectedArchieReview()
        {
            if (arUco == null || arUco.ArchieReviewID == null || arUco.ArchieReviewID.Length < 18) return;

            GridViewArchieReviewList.IsEnabled = false;
            //busyanimation.IsRunning = true;
            DataPortal<ArchieReviewUndoCheckoutCommand> dp = new DataPortal<ArchieReviewUndoCheckoutCommand>();
            dp.ExecuteCompleted += (o, e2) =>
            {
                GridViewArchieReviewList.IsEnabled = true;
                //busyanimation.IsRunning = false;
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
        private void ActionButtonArchieOpen_Click(object sender, RoutedEventArgs e)
        {
            GridViewMyReviews.IsEnabled = false;
            int reviewId = ((sender as Button).DataContext as ReadOnlyArchieReview).ReviewId;
            string reviewName = ((sender as Button).DataContext as ReadOnlyArchieReview).ReviewName;
            if (LoginToNewReviewRequested != null)
                LoginToNewReviewRequested.Invoke(this, new ReviewSelectedEventArgs(reviewId, reviewName));
        }

        private void btArchieCodeOnly_Click(object sender, RoutedEventArgs e)
        {
            GridViewMyReviews.IsEnabled = false;
            int reviewId = ((sender as Button).DataContext as ReadOnlyArchieReview).ReviewId;
            string reviewName = ((sender as Button).DataContext as ReadOnlyArchieReview).ReviewName;
            if (LoginToNewReviewRequested != null)
                LoginToNewReviewRequested.Invoke(this, new ReviewSelectedEventArgs(reviewId, reviewName, "Coding only"));
        }
        public void RefreshVisibleList()
        {
            if (ArchieReviewsTab == null || er4ReviewsTab == null ) return;
            if (er4ReviewsTab.Visibility == System.Windows.Visibility.Collapsed || er4ReviewsTab.IsSelected)
            {
                RefreshReviewList();
            }
            else if (ArchieReviewsTab.IsSelected)
            {
                refreshArchieReviews();
            }
            else
            {//just in case the logic above fails
                RefreshReviewList();
                refreshArchieReviews();
            }
        }
        private void Tabhost_SelectionChanged(object sender, RadSelectionChangedEventArgs e)
        {
            e.Handled = true;
            RefreshVisibleList();
        }
        private void refreshArchieReviews()
        {
            CslaDataProvider provider2 = App.Current.Resources["ArchieReviewsData"] as CslaDataProvider;
            provider2.FactoryParameters.Clear();
            provider2.FactoryMethod = "GetReviewList";
            GridViewArchieReviewList.IsEnabled = false;
            provider2.DataChanged -= ArchieReviewsProvider_DataChanged;
            provider2.DataChanged += new EventHandler(ArchieReviewsProvider_DataChanged);
            //busyanimation.IsRunning = true;
            provider2.Refresh();
        }
        public void RefreshReviewList()
        {
            CslaDataProvider provider = App.Current.Resources["ReviewsData"] as CslaDataProvider;
            if (provider != null)
            {
                provider.FactoryParameters.Clear();
                provider.FactoryParameters.Add(ri.UserId);
                provider.FactoryMethod = "GetReviewList";
                provider.Refresh();

            }
        }
        public void UnHookMe()
        {
            CslaDataProvider provider2 = App.Current.Resources["ArchieReviewsData"] as CslaDataProvider;
            if (provider2 != null)
            {
                provider2.DataChanged -= ArchieReviewsProvider_DataChanged;
                provider2.DataChanged -= ArchieReviewsProvider_DataChanged;
            }
        }
    }
}
