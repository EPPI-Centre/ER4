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
using Csla;
using Telerik.Windows.Controls;
using Csla.Xaml;

namespace EppiReviewer4
{
    public partial class dialogMagBrowser : UserControl
    {
        public dialogMagBrowser()
        {
            InitializeComponent();
        }

        public void ShowMagBrowser()
        {
            HLShowSummary_Click(null, null);
        }

        private void HLShowSummary_Click(object sender, RoutedEventArgs e)
        {
            StatusGrid.Visibility = Visibility.Visible;
            PaperGrid.Visibility = Visibility.Collapsed;
            TopicsGrid.Visibility = Visibility.Collapsed;
            PaperListGrid.Visibility = Visibility.Collapsed;
            HistoryGrid.Visibility = Visibility.Collapsed;

            DataPortal<MagCurrentInfo> dp = new DataPortal<MagCurrentInfo>();
            MagCurrentInfo mci = new MagCurrentInfo();
            dp.FetchCompleted += (o, e2) =>
            {
                //BusyLoading.IsRunning = false;
                if (e2.Error != null)
                {
                    RadWindow.Alert(e2.Error.Message);
                }
                else
                {
                    MagCurrentInfo mci2 = e2.Object as MagCurrentInfo;
                    if (mci2.CurrentAvailability == "available")
                    {
                        tbAcademicTitle.Text = "Microsoft Academic dataset last updated: " + mci2.LastUpdated.ToString();
                    }
                    else
                    {
                        tbAcademicTitle.Text = "Microsoft Academic dataset currently unavailable";
                    }

                }
            };
            //BusyLoading.IsRunning = true;
            dp.BeginFetch(mci);

            DataPortal<MAgReviewMagInfoCommand> dp2 = new DataPortal<MAgReviewMagInfoCommand>();
            MAgReviewMagInfoCommand mrmic = new MAgReviewMagInfoCommand();
            dp2.ExecuteCompleted += (o, e2) =>
            {
                //BusyLoading.IsRunning = false;
                if (e2.Error != null)
                {
                    RadWindow.Alert(e2.Error.Message);
                }
                else
                {
                    MAgReviewMagInfoCommand mrmic2 = e2.Object as MAgReviewMagInfoCommand;
                    TBNumInReview.Text = mrmic2.NInReviewIncluded.ToString() + " / " + mrmic2.NInReviewExcluded.ToString();
                    LBListMatchesIncluded.Content = mrmic2.NMatchedAccuratelyIncluded.ToString();
                    LBListMatchesExcluded.Content = mrmic2.NMatchedAccuratelyExcluded.ToString();
                    LBManualCheckIncluded.Content = mrmic2.NRequiringManualCheckIncluded.ToString();
                    LBManualCheckExcluded.Content = mrmic2.NRequiringManualCheckExcluded.ToString();
                }
            };
            //BusyLoading.IsRunning = true;
            dp2.BeginExecute(mrmic);
        }

        private void HLShowHistory_Click(object sender, RoutedEventArgs e)
        {
            StatusGrid.Visibility = Visibility.Collapsed;
            PaperGrid.Visibility = Visibility.Collapsed;
            TopicsGrid.Visibility = Visibility.Collapsed;
            PaperListGrid.Visibility = Visibility.Collapsed;
            HistoryGrid.Visibility = Visibility.Visible;
        }

        private void HLShowSelected_Click(object sender, RoutedEventArgs e)
        {
            StatusGrid.Visibility = Visibility.Collapsed;
            PaperGrid.Visibility = Visibility.Collapsed;
            TopicsGrid.Visibility = Visibility.Collapsed;
            PaperListGrid.Visibility = Visibility.Visible;
            HistoryGrid.Visibility = Visibility.Collapsed;
        }

        private void LBBrowseByTopic_Click(object sender, RoutedEventArgs e)
        {
            StatusGrid.Visibility = Visibility.Collapsed;
            PaperGrid.Visibility = Visibility.Collapsed;
            TopicsGrid.Visibility = Visibility.Visible;
            PaperListGrid.Visibility = Visibility.Collapsed;
            HistoryGrid.Visibility = Visibility.Collapsed;
        }

        private void LBListMatchesIncluded_Click(object sender, RoutedEventArgs e)
        {
            HLShowSelected_Click(null, null);

            CslaDataProvider provider = this.Resources["PaperListData"] as CslaDataProvider;
            provider.FactoryParameters.Clear();
            MagPaperListSelectionCriteria selectionCriteria = new MagPaperListSelectionCriteria();
            selectionCriteria.PageSize = 20;
            selectionCriteria.PageNumber = 0;
            selectionCriteria.ListType = "ReviewMatchedPapers";
            selectionCriteria.Included = "included";
            provider.FactoryParameters.Add(selectionCriteria);
            provider.FactoryMethod = "GetMagPaperList";
            provider.Refresh();
        }

        private void CslaDataProvider_DataChanged(object sender, EventArgs e)
        {
            CslaDataProvider provider = ((CslaDataProvider)this.Resources["PaperListData"]);
            if (provider.Error != null)
            {
                RadWindow.Alert(provider.Error.Message);
            }
            else
            {
                GetAssociatedTopics();
            }
        }

        private void GetAssociatedTopics()
        {
            MagPaperList paperlist = ((CslaDataProvider)this.Resources["PaperListData"]).Data as MagPaperList;
            CslaDataProvider provider = this.Resources["PaperListAssociatedTopicsData"] as CslaDataProvider;
            provider.FactoryParameters.Clear();
            MagFieldOfStudyListSelectionCriteria selectionCriteria = new MagFieldOfStudyListSelectionCriteria();
            selectionCriteria.ListType = "PaperFieldOfStudyList";
            foreach (MagPaper paper in paperlist)
            {
                if (selectionCriteria.PaperIdList == "")
                {
                    selectionCriteria.PaperIdList = paper.PaperId.ToString();
                }
                else
                {
                    selectionCriteria.PaperIdList += "," + paper.PaperId.ToString();
                }
            }
            provider.FactoryParameters.Add(selectionCriteria);
            provider.FactoryMethod = "GetMagFieldOfStudyList";
            provider.Refresh();
        }

        private void CslaDataProvider_DataChanged_1(object sender, EventArgs e)
        {
            CslaDataProvider provider = ((CslaDataProvider)this.Resources["PaperListAssociatedTopicsData"]);
            if (provider.Error != null)
            {
                RadWindow.Alert(provider.Error.Message);
            }
            else
            {
                WPTopTopics.Children.Clear();
                MagFieldOfStudyList FosList = provider.Data as MagFieldOfStudyList;
                double i = 15;
                foreach (MagFieldOfStudy fos in FosList)
                {
                    HyperlinkButton hl = new HyperlinkButton();
                    hl.Content = fos.DisplayName;
                    hl.Tag = fos.FieldOfStudyId.ToString();
                    hl.Click += HlNavigateToTopic_Click;
                    hl.FontSize = i;
                    hl.Margin = new Thickness(5, 5, 5, 5);
                    WPTopTopics.Children.Add(hl);
                    if (i > 10)
                    {
                        i -= 0.5;
                    }
                }
            }
        }

        private void HlNavigateToTopic_Click(object sender, RoutedEventArgs e)
        {
            StatusGrid.Visibility = Visibility.Collapsed;
            PaperGrid.Visibility = Visibility.Collapsed;
            TopicsGrid.Visibility = Visibility.Visible;
            PaperListGrid.Visibility = Visibility.Collapsed;
            HistoryGrid.Visibility = Visibility.Collapsed;

            HyperlinkButton hl = sender as HyperlinkButton;
            TBMainTopic.Text = hl.Content.ToString();

            getParentAndChildFieldsOfStudy("FieldOfStudyParentsList", Convert.ToInt64(hl.Tag), WPParentTopics, "Parent topics");
            getParentAndChildFieldsOfStudy("FieldOfStudyChildrenList", Convert.ToInt64(hl.Tag), WPChildTopics, "Child topics");
            getPaperListForTopic(Convert.ToInt64(hl.Tag));
        }

        private void getParentAndChildFieldsOfStudy(string ListType, Int64 FieldOfStudyId, WrapPanel wp, string desc)
        {
            MagFieldOfStudyListSelectionCriteria selectionCriteria = new MagFieldOfStudyListSelectionCriteria();
            selectionCriteria.ListType = ListType;
            selectionCriteria.FieldOfStudyId = FieldOfStudyId;
            DataPortal<MagFieldOfStudyList> dp = new DataPortal<MagFieldOfStudyList>();
            MagFieldOfStudyList mfsl = new MagFieldOfStudyList();
            dp.FetchCompleted += (o, e2) =>
            {
                wp.Children.Clear();
                TextBlock tb = new TextBlock();
                tb.Text = desc;
                tb.FontSize = 12;
                tb.FontStyle = FontStyles.Italic;
                tb.Margin = new Thickness(5, 5, 15, 5);
                wp.Children.Add(tb);
                MagFieldOfStudyList FosList = e2.Object as MagFieldOfStudyList;
                foreach (MagFieldOfStudy fos in FosList)
                {
                    HyperlinkButton newHl = new HyperlinkButton();
                    newHl.Content = fos.DisplayName;
                    newHl.FontSize = 12;
                    newHl.Tag = fos.FieldOfStudyId.ToString();
                    newHl.Click += HlNavigateToTopic_Click;
                    newHl.Margin = new Thickness(5, 5, 5, 5);
                    wp.Children.Add(newHl);
                }
            };
            dp.BeginFetch(selectionCriteria);
        }

        private void getPaperListForTopic(Int64 FieldOfStudyId)
        {
            CslaDataProvider provider = this.Resources["TopicPaperListData"] as CslaDataProvider;
            provider.FactoryParameters.Clear();
            MagPaperListSelectionCriteria selectionCriteria = new MagPaperListSelectionCriteria();
            selectionCriteria.PageSize = 20;
            selectionCriteria.PageNumber = 0;
            selectionCriteria.ListType = "PaperFieldsOfStudyList";
            selectionCriteria.FieldOfStudyId = FieldOfStudyId;
            provider.FactoryParameters.Add(selectionCriteria);
            provider.FactoryMethod = "GetMagPaperList";
            provider.Refresh();
        }

        private void LBListMatchesExcluded_Click(object sender, RoutedEventArgs e)
        {
            

        }

        private void CslaDataProvider_DataChanged_2(object sender, EventArgs e)
        {

        }

        private void TopicPaperListData_DataChanged(object sender, EventArgs e)
        {

        }

        private void PaperListBibliographyGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            MagPaper paper = (sender as TextBlock).DataContext as MagPaper;
            StatusGrid.Visibility = Visibility.Collapsed;
            PaperGrid.Visibility = Visibility.Visible;
            TopicsGrid.Visibility = Visibility.Collapsed;
            PaperListGrid.Visibility = Visibility.Collapsed;
            HistoryGrid.Visibility = Visibility.Collapsed;
            CitationPane.SelectedIndex = 0;

            RTBPaperInfo.Text = paper.FullRecord;

            CslaDataProvider provider = this.Resources["CitationPaperListData"] as CslaDataProvider;
            provider.FactoryParameters.Clear();
            MagPaperListSelectionCriteria selectionCriteria = new MagPaperListSelectionCriteria();
            selectionCriteria.PageSize = 20;
            selectionCriteria.PageNumber = 0;
            selectionCriteria.ListType = "CitationsList";
            selectionCriteria.MagPaperId = paper.PaperId;
            provider.FactoryParameters.Add(selectionCriteria);
            provider.FactoryMethod = "GetMagPaperList";
            provider.Refresh();

            CslaDataProvider provider2 = this.Resources["CitedByListData"] as CslaDataProvider;
            provider2.FactoryParameters.Clear();
            MagPaperListSelectionCriteria selectionCriteria2 = new MagPaperListSelectionCriteria();
            selectionCriteria2.PageSize = 20;
            selectionCriteria2.PageNumber = 0;
            selectionCriteria2.ListType = "CitedByList";
            selectionCriteria2.MagPaperId = paper.PaperId;
            provider2.FactoryParameters.Add(selectionCriteria2);
            provider2.FactoryMethod = "GetMagPaperList";
            provider2.Refresh();

            CslaDataProvider provider3 = this.Resources["RecommendationsListData"] as CslaDataProvider;
            provider3.FactoryParameters.Clear();
            MagPaperListSelectionCriteria selectionCriteria3 = new MagPaperListSelectionCriteria();
            selectionCriteria3.PageSize = 20;
            selectionCriteria3.PageNumber = 0;
            selectionCriteria3.ListType = "RecommendationsList";
            selectionCriteria3.MagPaperId = paper.PaperId;
            provider3.FactoryParameters.Add(selectionCriteria3);
            provider3.FactoryMethod = "GetMagPaperList";
            provider3.Refresh();

            MagFieldOfStudyListSelectionCriteria selectionCriteria4 = new MagFieldOfStudyListSelectionCriteria();
            selectionCriteria4.ListType = "PaperFieldOfStudyList";
            selectionCriteria4.PaperIdList = paper.PaperId.ToString();
            DataPortal<MagFieldOfStudyList> dp = new DataPortal<MagFieldOfStudyList>();
            MagFieldOfStudyList mfsl = new MagFieldOfStudyList();
            dp.FetchCompleted += (o, e2) =>
            {
                WPPaperTopics.Children.Clear();
                TextBlock tb = new TextBlock();
                tb.Text = "Topics";
                tb.FontSize = 15;
                tb.FontStyle = FontStyles.Italic;
                tb.Margin = new Thickness(5, 5, 15, 5);
                WPPaperTopics.Children.Add(tb);
                MagFieldOfStudyList FosList = e2.Object as MagFieldOfStudyList;
                double i = 15;
                foreach (MagFieldOfStudy fos in FosList)
                {
                    HyperlinkButton newHl = new HyperlinkButton();
                    newHl.Content = fos.DisplayName;
                    newHl.Tag = fos.FieldOfStudyId.ToString();
                    newHl.Click += HlNavigateToTopic_Click;
                    newHl.FontSize = i;
                    newHl.Margin = new Thickness(5, 5, 5, 5);
                    WPPaperTopics.Children.Add(newHl);
                    if (i > 10)
                    {
                        i -= 0.5;
                    }
                }
            };
            dp.BeginFetch(selectionCriteria4);
        }

        private void PaperListBibliographyPager_PageIndexChanging(object sender, PageIndexChangingEventArgs e)
        {
            CslaDataProvider provider = this.Resources["PaperListData"] as CslaDataProvider;
            provider.FactoryParameters.Clear();
            MagPaperListSelectionCriteria selectionCriteria = new MagPaperListSelectionCriteria();
            selectionCriteria.PageSize = 20;
            selectionCriteria.PageNumber = e.NewPageIndex;
            selectionCriteria.ListType = "ReviewMatchedPapers";
            selectionCriteria.Included = "included";
            provider.FactoryParameters.Add(selectionCriteria);
            provider.FactoryMethod = "GetMagPaperList";
            provider.Refresh();
        }
    }
}
