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
using Csla.Silverlight;
using BusinessLibrary.BusinessClasses;
using Csla;
using Csla.DataPortalClient;
using Telerik.Windows.Controls.GridView;
using Telerik.Windows.Controls;
using Csla.Xaml;

namespace EppiReviewer4
{
    public partial class homeReviewStatistics : UserControl
    {
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
        private RadWCompleteUncompleteCoding WCompleteUncompleteCoding = new RadWCompleteUncompleteCoding();
        public homeReviewStatistics()
        {
            InitializeComponent();
            TreeListViewReviewerCompleteStatistics.AddHandler(GridViewCell.MouseLeftButtonDownEvent, new MouseButtonEventHandler(MouseDownOnGridViewReviewerStatistics), true);
            TreeListViewReviewerUnCompleteStatistics.AddHandler(GridViewCell.MouseLeftButtonDownEvent, new MouseButtonEventHandler(MouseDownOnGridViewReviewerStatistics), true);
            //add <Button x:Name="isEn" IsEnabled="{Binding HasWriteRights, Mode=OneWay}" ></Button>
            //to the xaml resources section!!
            ri = Csla.ApplicationContext.User.Identity as BusinessLibrary.Security.ReviewerIdentity;
            isEn.DataContext = this;
            //end of read-only ui hack
           
            WCompleteUncompleteCoding.Closed += WCompleteUncompleteCoding_Closed;
        }

        private void WCompleteUncompleteCoding_Closed(object sender, WindowClosedEventArgs e)
        {
            if (WCompleteUncompleteCoding.HasDoneSomething) RefreshStatistics();
        }

        public event EventHandler<ItemListRefreshEventArgs> RefreshItemList;
        
        private void MouseDownOnGridViewReviewerStatistics(object sender, MouseEventArgs args)
        {
            if (args.OriginalSource is Image)
            {
                return;
            }

            if (((UIElement)args.OriginalSource).ParentOfType<GridViewCell>() != null)
            {
                ReviewStatisticsReviewer rsr = ((UIElement)args.OriginalSource).ParentOfType<GridViewCell>().DataContext as ReviewStatisticsReviewer;
                string col = ((UIElement)args.OriginalSource).ParentOfType<GridViewCell>().DataColumn.Header.ToString();
                if ((col == "Count") && (rsr != null))
                {
                    SelectionCriteria sc = new SelectionCriteria();
                    sc.ContactId = rsr.ContactId;
                    sc.SetId = rsr.SetId;
                    if (rsr.IsCompleted == true)
                    {
                        sc.ListType = "ReviewerCodingCompleted";
                        sc.Description = rsr.ContactName + ": documents with completed coding using '" + rsr.SetName + "'";
                    }
                    else if (rsr.IsCompleted == false)
                    {
                        sc.ListType = "ReviewerCodingIncomplete";
                        sc.Description = rsr.ContactName + ": documents with incomplete (but started) coding using '" + rsr.SetName + "'";
                    }
                    if (sc.ListType != "")
                    {
                        if (RefreshItemList != null)
                        {
                            ItemListRefreshEventArgs ilrea = new ItemListRefreshEventArgs(sc);
                            RefreshItemList.Invoke(this, ilrea);
                        }
                    }
                }
            }
        }

        public void RefreshStatistics()
        {
            if (ri != null)
            {
                Control Header = (TreeListViewReviewerUnCompleteStatistics.Columns["ButtonsColumn"].Header as Button);
                if (Header != null)
                {
                    Header.Visibility = ri.Roles.Contains("AdminUser") || ri.IsSiteAdmin ? Visibility.Visible : Visibility.Collapsed;
                    //Button bt = Header.FindChildByType<Button>();
                    //if (bt != null) bt.Visibility = ri.Roles.Contains("AdminUser") || ri.IsSiteAdmin ? Visibility.Visible : Visibility.Collapsed;
                }
                Header = (TreeListViewReviewerCompleteStatistics.Columns["ButtonsColumn"].Header as Button);
                if (Header != null)
                {
                    Header.Visibility = ri.Roles.Contains("AdminUser") || ri.IsSiteAdmin ? Visibility.Visible : Visibility.Collapsed;
                    //Button bt = Header.FindChildByType<Button>();
                    //if (bt != null) bt.Visibility = ri.Roles.Contains("AdminUser") || ri.IsSiteAdmin ? Visibility.Visible : Visibility.Collapsed;
                }
                //cmdUnCompleteCodingsOnAttribute.Visibility = ri.Roles.Contains("AdminUser") || ri.IsSiteAdmin ? Visibility.Visible : Visibility.Collapsed;
                //cmdCompleteCodingsOnAttribute.Visibility = ri.Roles.Contains("AdminUser") || ri.IsSiteAdmin ? Visibility.Visible : Visibility.Collapsed;
            }
            DataPortal<ReviewStatisticsCountsCommand> dp = new DataPortal<ReviewStatisticsCountsCommand>();
            ReviewStatisticsCountsCommand command = new ReviewStatisticsCountsCommand();
            dp.ExecuteCompleted += (o, e2) =>
            {
                BusyLoading.IsRunning = false;
                if (e2.Error != null)
                {
                    MessageBox.Show(e2.Error.Message);
                }
                else
                {
                    TextBlockIncludedCount.Text = "Number of included documents: " + e2.Object.ItemsIncluded.ToString();
                    TextBlockExcludedCount.Text = "Number of excluded documents: " + e2.Object.ItemsExcluded.ToString();
                    TextBlockDeletedCount.Text = "Number of deleted documents: " + e2.Object.ItemsDeleted.ToString();
                    TextBlockDuplicatesCount.Text = "Number of duplicates: " + e2.Object.DuplicateItems.ToString();
                }
            };
            BusyLoading.IsRunning = true;
            dp.BeginExecute(command);
            CslaDataProvider provider = ((CslaDataProvider)this.Resources["ReviewStatisticsCodeSetCompleteData"]);
            if (provider != null)
            {
                provider.FactoryParameters.Clear();
                provider.FactoryParameters.Add(true);
                provider.FactoryMethod = "GetReviewStatisticsCodeSetList";
                provider.Refresh();
            }
            CslaDataProvider provider2 = ((CslaDataProvider)this.Resources["ReviewStatisticsCodeSetIncompleteData"]);
            if (provider2 != null)
            {
                provider2.FactoryParameters.Clear();
                provider2.FactoryParameters.Add(false);
                provider2.FactoryMethod = "GetReviewStatisticsCodeSetList";
                provider2.Refresh();
            }
        }

        private void CslaDataProvider_DataChanged(object sender, EventArgs e)
        {
            CslaDataProvider provider = ((CslaDataProvider)this.Resources["ReviewStatisticsCodeSetCompleteData"]);
            if (provider.Error != null)
                MessageBox.Show(provider.Error.Message);
            CslaDataProvider provider3 = ((CslaDataProvider)this.Resources["ReviewStatisticsCodeSetIncompleteData"]);
            if (provider.Error != null)
                MessageBox.Show(provider3.Error.Message);
        }

        private void cmdUnCompleteCodings_Click(object sender, RoutedEventArgs e)
        {
            ReviewStatisticsReviewer rsr = (sender as Button).DataContext as ReviewStatisticsReviewer;
            if (rsr != null)
            {
                if (MessageBox.Show("Are you sure you want to set these codings by the selected reviewer to being ‘uncompleted’?" +Environment.NewLine
                    +"Note: If these are double coded items and are reconciled disagreements you will be loosing the reconcile information." +Environment.NewLine+Environment.NewLine
                    + "Please check in the manual if you are unsure about the implications." +Environment.NewLine
                    +"‘Uncompleted’ items will no longer be visible in searches and reports.", "Change coding status?", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    CompleteCoding(rsr.ContactId, rsr.SetId, false);
                }
            }
        }

        private void CompleteCoding(int ContactId, int SetId, bool ToComplete)
        {
            DataPortal<ItemSetBulkCompleteCommand> dp = new DataPortal<ItemSetBulkCompleteCommand>();
            ItemSetBulkCompleteCommand command = new ItemSetBulkCompleteCommand(SetId, ContactId, ToComplete);
            dp.ExecuteCompleted += (o, e2) =>
            {
                BusyLoading.IsRunning = false;
                if (e2.Error != null)
                {
                    MessageBox.Show(e2.Error.Message);
                }
                else
                {
                    RefreshStatistics();
                }
            };
            BusyLoading.IsRunning = true;
            dp.BeginExecute(command);
        }

        private void cmdCompleteCodings_Click(object sender, RoutedEventArgs e)
        {
            ReviewStatisticsReviewer rsr = (sender as Button).DataContext as ReviewStatisticsReviewer;
            if (rsr != null)
            {
                if (MessageBox.Show("Are you sure you want to set these codings by the selected reviewer to being ‘complete’?" +Environment.NewLine
                    +"Note: If these items have been double coded you may be ‘completing’ un-reconciled disagreements." +Environment.NewLine
                     +Environment.NewLine
                    +"Please check in the manual if you are unsure about the implications." +Environment.NewLine
                    + "‘Completed’ items will be visible in searches and reports." + Environment.NewLine
                    , "Change coding status?", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    CompleteCoding(rsr.ContactId, rsr.SetId, true);
                }
            }
        }

        private void cmdUnCompleteCodingsOnAttribute_Click(object sender, RoutedEventArgs e)
        {
            WCompleteUncompleteCoding.SetMode(false);
            WCompleteUncompleteCoding.ShowDialog();
        }

        private void cmdCompleteCodingsOnAttribute_Click(object sender, RoutedEventArgs e)
        {
            WCompleteUncompleteCoding.SetMode(true);
            WCompleteUncompleteCoding.ShowDialog();
        }
    }
}
