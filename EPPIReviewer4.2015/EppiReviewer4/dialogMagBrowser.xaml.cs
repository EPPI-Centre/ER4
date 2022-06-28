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
using BusinessLibrary.Security;
using System.Windows.Threading;
using Telerik.Windows.Controls.ChartView;
using System.IO;
using System.Text.RegularExpressions;
using Telerik.Windows.Controls.GridView;

namespace EppiReviewer4
{
    public partial class dialogMagBrowser : UserControl
    {
        public event EventHandler<RoutedEventArgs> ListIncludedThatNeedMatching;
        public event EventHandler<RoutedEventArgs> ListExcludedThatNeedMatching;
        public event EventHandler<RoutedEventArgs> ListIncludedNotMatched;
        public event EventHandler<RoutedEventArgs> ListExcludedNotMatched;
        public event EventHandler<RoutedEventArgs> ListIncludedMatched;
        public event EventHandler<RoutedEventArgs> ListExcludedMatched;
        public event EventHandler<RoutedEventArgs> ListPreviouslyMatched;
        public event EventHandler<RoutedEventArgs> ListSimulationTP;
        public event EventHandler<RoutedEventArgs> ListSimulationFN;
        public bool MagBrowserImportedItems = false;

        private DispatcherTimer timer;
        private DispatcherTimer timer2;
        private DispatcherTimer timerAutoUpdateClassifierRun;
        private DispatcherTimer AdminLogTimer;
        private DispatcherTimer DataLakeTimer;
        private int CurrentBrowsePosition = 0;
        private List<Int64> SelectedPaperIds;
        //private int _maxFieldOfStudyPaperCount = 1000000;
        //public MagCurrentInfo CurrentMagInfo;
        private int nMatchedRecords;

        public dialogMagBrowser()
        {
            InitializeComponent();
            SelectedPaperIds = new List<Int64>();
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;

            timer2 = new DispatcherTimer();
            timer2.Interval = TimeSpan.FromSeconds(1);
            timer2.Tick += Timer2_Tick;

            AdminLogTimer = new DispatcherTimer();
            AdminLogTimer.Interval = TimeSpan.FromSeconds(10);
            AdminLogTimer.Tick += AdminLogTimer_Tick;

            timerAutoUpdateClassifierRun = new DispatcherTimer();
            timerAutoUpdateClassifierRun.Interval = TimeSpan.FromSeconds(300);
            timerAutoUpdateClassifierRun.Tick += TimerAutoUpdateClassifierRun_Tick;

            DataLakeTimer = new DispatcherTimer();
            DataLakeTimer.Interval = TimeSpan.FromSeconds(30);
            DataLakeTimer.Tick += DataLakeTimer_Tick;
        }

        public void InitialiseBrowser()
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            if (!ri.IsSiteAdmin)
            {
                AdminPane.IsEnabled = false;
            }
            UpdateSelectedCount();

            GridPaperInfoBackground.Background = new SolidColorBrush(SystemColors.ControlColor);
            RefreshCounts();
            MagBrowserImportedItems = false;
        }

        public void ShowMagBrowser()
        {
            //HLShowAdvanced_Click(null, null);
            //LBManageRelatedPapersRun_Click(null, null);
            Panes.SelectedIndex = 0;
            InitialiseBrowser();
            //CslaDataProvider provider = this.Resources["RelatedPapersRunListData"] as CslaDataProvider;
            //provider.Refresh();
        }

        // ************************** Top navigation button events **************************


        private void HLShowSelected_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedPaperIds.Count == 0)
            {
                RadWindow.Alert("You don't have anything selected.");
                return;
            }
            IncrementHistoryCount();
            AddToBrowseHistory("List of all selected papers", "SelectedPapers", 0, "", "", 0, "", "", 0, "", "", 0, 0, "", 0, 0, 0, 0, "");
            TBPaperListTitle.Text = "List of all selected papers";
            ShowSelectedPapersPage();
        }

        private void Panes_SelectionChanged(object sender, RadSelectionChangedEventArgs e)
        {
            RadDocumentPane rdp = Panes.SelectedItem as RadDocumentPane;
            if (rdp != null)
            {
                switch (rdp.Tag.ToString())
                {
                    case "BringUpToDate":
                        IncrementHistoryCount();
                        AddToBrowseHistory("Bring review up to date", "RelatedPapers", 0, "", "", 0, "", "", 0, "", "", 0, 0, "", 0, 0, 0, 0, "");
                        ShowRelatedPapersPage();
                        break;

                    case "AutoUpdate":
                        IncrementHistoryCount();
                        AddToBrowseHistory("Keep review up to date", "AutoUpdate", 0, "", "", 0, "", "", 0, "", "", 0, 0, "", 0, 0, 0, 0, "");
                        ShowAutoUpdatePage();
                        break;

                    case "MatchItems":
                        RefreshCounts();
                        break;

                    case "Simulation":
                        IncrementHistoryCount();
                        AddToBrowseHistory("Advanced page", "Advanced", 0, "", "", 0, "", "", 0, "", "", 0, 0, "", 0, 0, 0, 0, "");
                        ShowAdvancedPage();
                        break;

                    case "SearchBrowse":
                        ShowSearchPage();
                        break;

                    case "History":
                        IncrementHistoryCount();
                        AddToBrowseHistory("View browse history", "History", 0, "", "", 0, "", "", 0, "", "", 0, 0, "", 0, 0, 0, 0, "");
                        break;

                    case "Admin":
                        ShowAdminPage();
                        break;

                    default:
                        break;

                }
            }
        }

        // ************************************* Simulation studies PAGE ******************************************

        private void ShowAdvancedPage()
        {
            CslaDataProvider provider3 = this.Resources["ClassifierContactModelListData"] as CslaDataProvider;
            provider3.Refresh();
            CslaDataProvider provider1 = this.Resources["MagSimulationListData"] as CslaDataProvider;
            provider1.Refresh();
        }

        private void RefreshCounts()
        {
            DataPortal<MagReviewMagInfoCommand> dp2 = new DataPortal<MagReviewMagInfoCommand>();
            MagReviewMagInfoCommand mrmic = new MagReviewMagInfoCommand();
            dp2.ExecuteCompleted += (o, e2) =>
            {
                busyIndicatorMatches.IsBusy = false;
                BusyImportingRecords.IsRunning = false;
                if (e2.Error != null)
                {
                    RadWindow.Alert(e2.Error.Message);
                }
                else
                {
                    MagReviewMagInfoCommand mrmic2 = e2.Object as MagReviewMagInfoCommand;
                    //TBNumInReview.Text = mrmic2.NInReviewIncluded.ToString() + " / " + mrmic2.NInReviewExcluded.ToString();
                    tbMatchedRecordsTitle.Text = "Matched records: " + mrmic2.NMatchedAccuratelyIncluded.ToString();
                    nMatchedRecords = mrmic2.NMatchedAccuratelyIncluded;
                    LBListMatchesIncluded.Content = mrmic2.NMatchedAccuratelyIncluded.ToString();
                    LBListMatchesExcluded.Content = mrmic2.NMatchedAccuratelyExcluded.ToString();
                    LBListAllInReview.Content = (mrmic2.NMatchedAccuratelyIncluded + mrmic2.NMatchedAccuratelyExcluded).ToString();
                    LBManualCheckIncluded.Content = mrmic2.NRequiringManualCheckIncluded.ToString();
                    LBManualCheckExcluded.Content = mrmic2.NRequiringManualCheckExcluded.ToString();
                    LBMNotMatchedIncluded.Content = mrmic2.NNotMatchedIncluded.ToString();
                    LBMNotMatchedExcluded.Content = mrmic2.NNotMatchedExcluded.ToString();
                    if (mrmic2.NPreviouslyMatched == 0)
                    {
                        tbPreviouslyMatchedRecords.Visibility = Visibility.Collapsed;
                        LBListPreviouslyMatchedRecords.Visibility = Visibility.Collapsed;
                        LBListPreviouslyMatchedRecords.IsEnabled = false;
                    }
                    else
                    {
                        tbPreviouslyMatchedRecords.Visibility = Visibility.Visible;
                        tbPreviouslyMatchedRecords.Text = "Previously matched records: " + mrmic2.NPreviouslyMatched.ToString();
                        LBListPreviouslyMatchedRecords.Visibility = Visibility.Visible;
                        LBListPreviouslyMatchedRecords.IsEnabled = true;
                    }
                }
            };
            busyIndicatorMatches.IsBusy = true;
            BusyImportingRecords.IsRunning = true;
            dp2.BeginExecute(mrmic);
        }

        // ********************************* SEARCH PAGE *********************************

        private void ShowSearchPage()
        {
            //PaperGrid.Visibility = Visibility.Collapsed;
            //TopicsGrid.Visibility = Visibility.Collapsed;
            //PaperListGrid.Visibility = Visibility.Collapsed;
            //SearchGrid.Visibility = Visibility.Visible;

            CslaDataProvider provider = this.Resources["MagSearchListData"] as CslaDataProvider;
            provider.FactoryParameters.Clear();
            MagPaperListSelectionCriteria selectionCriteria = new MagPaperListSelectionCriteria();
            provider.FactoryMethod = "GetMagSearchList";
            provider.Refresh();
        }

        private void HLButtonBackToSearch_Click(object sender, RoutedEventArgs e)
        {
            PaperGrid.Visibility = Visibility.Collapsed;
            TopicsGrid.Visibility = Visibility.Collapsed;
            PaperListGrid.Visibility = Visibility.Collapsed;
            SearchGrid.Visibility = Visibility.Visible;
            HLButtonBackToSearch.Visibility = Visibility.Collapsed;
            ShowSearchPage();
        }

        // ********************************* ADMIN PAGE **********************************

        private void ShowAdminPage()
        {
            CslaDataProvider provider = this.Resources["MagReviewListData"] as CslaDataProvider;
            provider.FactoryParameters.Clear();
            MagPaperListSelectionCriteria selectionCriteria = new MagPaperListSelectionCriteria();
            provider.FactoryMethod = "GetMagReviewList";
            provider.Refresh();

            CslaDataProvider provider2 = this.Resources["MagLogListData"] as CslaDataProvider;
            provider2.FactoryParameters.Clear();
            provider2.FactoryMethod = "GetMagLogList";
            provider2.Refresh();

            CslaDataProvider provider3 = this.Resources["MagCurrentInfoListData"] as CslaDataProvider;
            provider3.FactoryParameters.Clear();
            provider3.FactoryMethod = "GetMagCurrentInfoList";
            provider3.Refresh();

            DataPortal<MagBlobDataCommand> dp2 = new DataPortal<MagBlobDataCommand>();
            MagBlobDataCommand command = new MagBlobDataCommand();
            dp2.ExecuteCompleted += (o, e2) =>
            {
                //BusyLoading.IsRunning = false;
                if (e2.Error != null)
                {
                    RadWindow.Alert(e2.Error.Message);
                }
                else
                {
                    MagBlobDataCommand mb = e2.Object as MagBlobDataCommand;
                    //tbMagSas.Text = mb.LatestMagSasUri;
                    tbLatestMag.Text = mb.LatestMAGName;
                    //tbReleaseNotes.Text = mb.ReleaseNotes;
                    tbPreviousMAG.Text = mb.PreviousMAGName;

                    CslaDataProvider provider4 = this.Resources["MagCurrentInfoListData"] as CslaDataProvider;
                    if (provider4 != null)
                    {
                        MagCurrentInfoList mcil = provider4.Data as MagCurrentInfoList;
                        if (mcil != null)
                        {
                            bool found = false;
                            foreach (MagCurrentInfo mci in mcil)
                            {
                                if (mci.MagFolder == mb.LatestMAGName)
                                {
                                    found = true;
                                    break;
                                }
                            }
                            if (!found)
                            {
                                LBCurrentInfoCreate.Visibility = Visibility.Visible;
                                LBCopyNewOaData.Visibility = Visibility.Visible;
                            }
                            else
                            {
                                LBCurrentInfoCreate.Visibility = Visibility.Collapsed;
                                LBCopyNewOaData.Visibility = Visibility.Collapsed;
                            }
                        }
                    }
                }
            };
            //BusyLoading.IsRunning = true;
            dp2.BeginExecute(command);
        }

        // ******************************** PAPER DETAILS PAGE **************************************
        public void ShowPaperDetailsPage(Int64 PaperId, string FullRecord, string Abstract, string URLs,
            string FindOnWeb, Int64 LinkedITEM_ID, string FieldsOfStudyListString)
        {
            PaperGrid.Visibility = Visibility.Visible;
            TopicsGrid.Visibility = Visibility.Collapsed;
            PaperListGrid.Visibility = Visibility.Collapsed;
            SearchGrid.Visibility = Visibility.Collapsed;
            HLButtonBackToSearch.Visibility = Visibility.Visible;
            CitationPane.SelectedIndex = 0;
            Panes.SelectedIndex = 4;

            RTBPaperInfo.Text = FullRecord;
            hlFindOnWeb.NavigateUri = new Uri(FindOnWeb);
            tbAbstract.Text = Abstract;
            if (tbAbstract.Text == "")
            {
                tbAbstract.Visibility = Visibility.Collapsed;
            }
            else
            {
                tbAbstract.Visibility = Visibility.Visible;
            }
            tbPaperId.Text = PaperId.ToString();
            WPPaperURLs.Children.Clear();
            if (URLs != "")
            {
                string[] splitted = URLs.Split(';');
                if (splitted.Length > 0)
                {
                    for (int ii = 0; ii < splitted.Length; ii++)
                    {
                        if (splitted[ii].Length > 7)
                        {
                            try
                            {
                                HyperlinkButton newHl = new HyperlinkButton();
                                Uri uri = new Uri(splitted[ii]);
                                newHl.Content = uri.Host;
                                newHl.NavigateUri = new Uri(splitted[ii]);
                                newHl.TargetName = "_blank";
                                newHl.IsTabStop = false;
                                newHl.Margin = new Thickness(2, 1, 1, 1);
                                WPPaperURLs.Children.Add(newHl);
                            }
                            catch
                            {
                                continue;
                            }
                        }
                    }
                }
            }
            if (WPPaperURLs.Children.Count == 0 && tbAbstract.Text == "")
            {
                HLExpandContract.Visibility = Visibility.Collapsed;
            }
            else
            {
                HLExpandContract.Visibility = Visibility.Visible;
            }
            if (LinkedITEM_ID == 0)
            {
                tbPaperAlreadyInReview.Text = "This paper is not currently in your review.";
                hlAddPaperToSelectedList.Visibility = Visibility.Visible;
                hlAddPaperToSelectedList.Tag = PaperId;
            }
            else
            {
                tbPaperAlreadyInReview.Text = "This paper is already in your review.";
                hlAddPaperToSelectedList.Visibility = Visibility.Collapsed;
            }

            if (SelectedPaperIds.IndexOf(PaperId) > -1)
            {
                hlAddPaperToSelectedList.Content = "Remove from selected list";
            }
            else
            {
                hlAddPaperToSelectedList.Content = "Add to selected list";
            }


            CslaDataProvider provider = this.Resources["CitationPaperListData"] as CslaDataProvider;
            provider.FactoryParameters.Clear();
            MagPaperListSelectionCriteria selectionCriteria = new MagPaperListSelectionCriteria();
            selectionCriteria.PageSize = 20;
            selectionCriteria.PageNumber = 0;
            selectionCriteria.ListType = "CitationsList";
            selectionCriteria.MagPaperId = PaperId;
            provider.FactoryParameters.Add(selectionCriteria);
            provider.FactoryMethod = "GetMagPaperList";
            provider.Refresh();

            CslaDataProvider provider2 = this.Resources["CitedByListData"] as CslaDataProvider;
            provider2.FactoryParameters.Clear();
            MagPaperListSelectionCriteria selectionCriteria2 = new MagPaperListSelectionCriteria();
            selectionCriteria2.PageSize = 20;
            selectionCriteria2.PageNumber = 0;
            selectionCriteria2.ListType = "CitedByList";
            selectionCriteria2.MagPaperId = PaperId;
            provider2.FactoryParameters.Add(selectionCriteria2);
            provider2.FactoryMethod = "GetMagPaperList";
            provider2.Refresh();

            CslaDataProvider provider3 = this.Resources["RecommendationsListData"] as CslaDataProvider;
            provider3.FactoryParameters.Clear();
            MagPaperListSelectionCriteria selectionCriteria3 = new MagPaperListSelectionCriteria();
            selectionCriteria3.PageSize = 20;
            selectionCriteria3.PageNumber = 0;
            selectionCriteria3.ListType = "RecommendationsList";
            selectionCriteria3.MagPaperId = PaperId;
            provider3.FactoryParameters.Add(selectionCriteria3);
            provider3.FactoryMethod = "GetMagPaperList";
            provider3.Refresh();

            MagFieldOfStudyListSelectionCriteria selectionCriteria4 = new MagFieldOfStudyListSelectionCriteria();
            selectionCriteria4.ListType = "PaperFieldOfStudyList";
            selectionCriteria4.PaperIdList = PaperId.ToString();
            DataPortal<MagFieldOfStudyList> dp = new DataPortal<MagFieldOfStudyList>();
            MagFieldOfStudyList FosList = MagPaper.GetFieldOfStudyAsList(FieldsOfStudyListString);
            double i = 15;
            foreach (MagFieldOfStudy fos in FosList)
            {
                HyperlinkButton newHl = new HyperlinkButton();
                newHl.Content = fos.DisplayName;
                newHl.Tag = fos.FieldOfStudyId.ToString();
                newHl.IsTabStop = false;
                newHl.FontSize = i;
                newHl.Margin = new Thickness(5, 5, 5, 5);
                    //newHl.NavigateUri = new Uri("https://academic.microsoft.com/topic/" +
                    //    fos.FieldOfStudyId.ToString());
                    //newHl.TargetName = "_blank";
                    //newHl.Foreground = new SolidColorBrush(Colors.DarkGray);
                    //newHl.FontStyle = FontStyles.Italic;
                newHl.Click += HlNavigateToTopic_Click;
                WPPaperTopics.Children.Add(newHl);
                if (i > 10)
                {
                    i -= 0.5;
                }
            }

            /* OLD STYLE USING MAKES
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
                    newHl.IsTabStop = false;
                    newHl.FontSize = i;
                    newHl.Margin = new Thickness(5, 5, 5, 5);
                    if (fos.PaperCount > _maxFieldOfStudyPaperCount)
                    {
                        newHl.NavigateUri = new Uri("https://academic.microsoft.com/topic/" +
                            fos.FieldOfStudyId.ToString());
                        newHl.TargetName = "_blank";
                        newHl.Foreground = new SolidColorBrush(Colors.DarkGray);
                        newHl.FontStyle = FontStyles.Italic;
                    }
                    else
                    {
                        newHl.Click += HlNavigateToTopic_Click;
                    }
                    WPPaperTopics.Children.Add(newHl);
                    if (i > 10)
                    {
                        i -= 0.5;
                    }
                }
            };
            dp.BeginFetch(selectionCriteria4);
            */
        }

        private void lbGoToPaperId_Click(object sender, RoutedEventArgs e)
        {
            Int64 id;
            if (!Int64.TryParse(tbGoToPaperId.Text, out id))
            {
                RadWindow.Alert("Please enter a valid number");
                return;
            }
            DataPortal<MagPaper> dp = new DataPortal<MagPaper>();
            dp.FetchCompleted += (o, e2) =>
            {
                if (e2 != null)
                {
                    IncrementHistoryCount();
                    AddToBrowseHistory("Go to specific Paper Id: " + e2.Object.PaperId.ToString(), "PaperDetail",
                        e2.Object.PaperId, e2.Object.FullRecord, e2.Object.Abstract, e2.Object.LinkedITEM_ID,
                        e2.Object.AllLinks, e2.Object.FindOnWeb, 0, "", "", 0, 0, "", 0, 0, 0, 0, e2.Object.FieldsOfStudyList);
                    Panes.SelectedIndex = 4;
                    ShowPaperDetailsPage(e2.Object.PaperId, e2.Object.FullRecord, e2.Object.Abstract,
                        e2.Object.AllLinks, e2.Object.FindOnWeb, e2.Object.LinkedITEM_ID, e2.Object.FieldsOfStudyList);
                }
                else
                {
                    RadWindow.Alert("This Paper ID cannot be found in the database");
                }
            };
            dp.BeginFetch(new SingleCriteria<MagPaper, Int64>(id));
        }

        // ***************************************** Included matches page *************************
        private void ShowIncludedMatchesPage(string IncludedOrExcluded)
        {
            Panes.SelectedIndex = 4;
            PaperGrid.Visibility = Visibility.Collapsed;
            TopicsGrid.Visibility = Visibility.Collapsed;
            PaperListGrid.Visibility = Visibility.Visible;
            SearchGrid.Visibility = Visibility.Collapsed;
            HLButtonBackToSearch.Visibility = Visibility.Visible;

            //SimilarityScoreColumn.IsVisible = false;

            CslaDataProvider provider = this.Resources["PaperListData"] as CslaDataProvider;
            provider.FactoryParameters.Clear();
            MagPaperListSelectionCriteria selectionCriteria = new MagPaperListSelectionCriteria();
            selectionCriteria.PageSize = 20;
            selectionCriteria.PageNumber = 0;
            selectionCriteria.ListType = "ReviewMatchedPapers";
            selectionCriteria.Included = IncludedOrExcluded;
            provider.FactoryParameters.Add(selectionCriteria);
            provider.FactoryMethod = "GetMagPaperList";
            provider.Refresh();
        }

        private void ShowRelatedRunPapers(int MagRelatedRunId)
        {
            PaperGrid.Visibility = Visibility.Collapsed;
            TopicsGrid.Visibility = Visibility.Collapsed;
            PaperListGrid.Visibility = Visibility.Visible;
            SearchGrid.Visibility = Visibility.Collapsed;
            HLButtonBackToSearch.Visibility = Visibility.Visible;
            Panes.SelectedIndex = 4;

            //SimilarityScoreColumn.IsVisible = true;

            CslaDataProvider provider = this.Resources["PaperListData"] as CslaDataProvider;
            provider.FactoryParameters.Clear();
            MagPaperListSelectionCriteria selectionCriteria = new MagPaperListSelectionCriteria();
            selectionCriteria.PageSize = 20;
            selectionCriteria.PageNumber = 0;
            selectionCriteria.ListType = "MagRelatedPapersRunList";
            selectionCriteria.MagRelatedRunId = MagRelatedRunId;
            provider.FactoryParameters.Add(selectionCriteria);
            provider.FactoryMethod = "GetMagPaperList";
            provider.Refresh();
        }

        private void ShowAutoUpdateIdentifiedItems(int MagAutoUpdateRunId, string OrderBy,
            double AutoUpdateScore, double StudyTypeClassifierScore, double UserClassifierScore, int AutoUpdateUserTopN)
        {
            PaperGrid.Visibility = Visibility.Collapsed;
            TopicsGrid.Visibility = Visibility.Collapsed;
            PaperListGrid.Visibility = Visibility.Visible;
            SearchGrid.Visibility = Visibility.Collapsed;
            HLButtonBackToSearch.Visibility = Visibility.Visible;
            Panes.SelectedIndex = 4;

            //SimilarityScoreColumn.IsVisible = true;

            CslaDataProvider provider = this.Resources["PaperListData"] as CslaDataProvider;
            provider.FactoryParameters.Clear();
            MagPaperListSelectionCriteria selectionCriteria = new MagPaperListSelectionCriteria();
            selectionCriteria.PageSize = 20;
            selectionCriteria.PageNumber = 0;
            selectionCriteria.ListType = "MagAutoUpdateRunPapersList";
            selectionCriteria.MagAutoUpdateRunId = MagAutoUpdateRunId;
            selectionCriteria.AutoUpdateOrderBy = OrderBy;
            selectionCriteria.AutoUpdateAutoUpdateScore = AutoUpdateScore;
            selectionCriteria.AutoUpdateStudyTypeClassifierScore = StudyTypeClassifierScore;
            selectionCriteria.AutoUpdateUserClassifierScore = UserClassifierScore;
            selectionCriteria.AutoUpdateUserTopN = AutoUpdateUserTopN;
            provider.FactoryParameters.Add(selectionCriteria);
            provider.FactoryMethod = "GetMagPaperList";
            provider.Refresh();
        }

        private void ShowAllWithThisCode(string AttributeIds)
        {
            Panes.SelectedIndex = 4;
            PaperGrid.Visibility = Visibility.Collapsed;
            TopicsGrid.Visibility = Visibility.Collapsed;
            PaperListGrid.Visibility = Visibility.Visible;
            SearchGrid.Visibility = Visibility.Collapsed;
            HLButtonBackToSearch.Visibility = Visibility.Visible;

            //SimilarityScoreColumn.IsVisible = false;

            CslaDataProvider provider = this.Resources["PaperListData"] as CslaDataProvider;
            provider.FactoryParameters.Clear();
            MagPaperListSelectionCriteria selectionCriteria = new MagPaperListSelectionCriteria();
            selectionCriteria.PageSize = 20;
            selectionCriteria.PageNumber = 0;
            selectionCriteria.ListType = "ReviewMatchedPapersWithThisCode";
            selectionCriteria.AttributeIds = AttributeIds;
            provider.FactoryParameters.Add(selectionCriteria);
            provider.FactoryMethod = "GetMagPaperList";
            provider.Refresh();
        }

        private void ShowSearchResults(string MagSearchId)
        {
            PaperGrid.Visibility = Visibility.Collapsed;
            TopicsGrid.Visibility = Visibility.Collapsed;
            PaperListGrid.Visibility = Visibility.Visible;
            SearchGrid.Visibility = Visibility.Collapsed;
            HLButtonBackToSearch.Visibility = Visibility.Visible;

            //SimilarityScoreColumn.IsVisible = false;

            CslaDataProvider provider = this.Resources["PaperListData"] as CslaDataProvider;
            provider.FactoryParameters.Clear();
            MagPaperListSelectionCriteria selectionCriteria = new MagPaperListSelectionCriteria();
            selectionCriteria.PageSize = 20;
            selectionCriteria.PageNumber = 0;
            selectionCriteria.ListType = "MagSearchResultsList";
            selectionCriteria.MagSearchId = MagSearchId;
            provider.FactoryParameters.Add(selectionCriteria);
            provider.FactoryMethod = "GetMagPaperList";
            provider.Refresh();
        }

        // *********************************** Topic page *********************************
        private void ShowTopicPage(Int64 FieldOfStudyId, string FieldOfStudy)
        {
            PaperGrid.Visibility = Visibility.Collapsed;
            TopicsGrid.Visibility = Visibility.Visible;
            PaperListGrid.Visibility = Visibility.Collapsed;
            SearchGrid.Visibility = Visibility.Collapsed;
            HLButtonBackToSearch.Visibility = Visibility.Visible;

            TBMainTopic.Text = FieldOfStudy;

            getParentAndChildFieldsOfStudy("FieldOfStudyParentsList", FieldOfStudyId, WPParentTopics, "Parent topics");
            getParentAndChildFieldsOfStudy("FieldOfStudyChildrenList", FieldOfStudyId, WPChildTopics, "Child topics");
            getPaperListForTopic(FieldOfStudyId);
        }

        // ***************************** List selected papers page **************************************

        private void ShowSelectedPapersPage()
        {
            Panes.SelectedIndex = 4;
            PaperGrid.Visibility = Visibility.Collapsed;
            TopicsGrid.Visibility = Visibility.Collapsed;
            PaperListGrid.Visibility = Visibility.Visible;
            SearchGrid.Visibility = Visibility.Collapsed;
            HLButtonBackToSearch.Visibility = Visibility.Visible;

            //SimilarityScoreColumn.IsVisible = false;

            CslaDataProvider provider = this.Resources["PaperListData"] as CslaDataProvider;
            provider.FactoryParameters.Clear();
            MagPaperListSelectionCriteria selectionCriteria = new MagPaperListSelectionCriteria();
            selectionCriteria.PageSize = 20;
            selectionCriteria.PageNumber = 0;
            selectionCriteria.ListType = "PaperListById";
            selectionCriteria.PaperIds = GetSelectedIds();
            provider.FactoryParameters.Add(selectionCriteria);
            provider.FactoryMethod = "GetMagPaperList";
            provider.Refresh();
        }

        private void ShowRelatedPapersPage()
        {
            CslaDataProvider prov = ((CslaDataProvider)App.Current.Resources["MagCurrentInfoData"]);
            MagCurrentInfo mci = prov.Data as MagCurrentInfo;
            if (mci != null)
            {
                if (mci.MagOnline == true)
                {
                    tbAcademicTitle.Text = "OpenAlex dataset: " + mci.MagFolder;
                }
                else
                {
                    tbAcademicTitle.Text = "OpenAlex dataset currently unavailable";
                }
            }

            CslaDataProvider provider = this.Resources["RelatedPapersRunListData"] as CslaDataProvider;
            provider.Refresh();
        }

        private void ShowAutoUpdatePage()
        {
            CslaDataProvider provider1 = this.Resources["MagAutoUpdateListData"] as CslaDataProvider;
            provider1.Refresh();
            CslaDataProvider provider2 = this.Resources["MagAutoUpdateRunListData"] as CslaDataProvider;
            provider2.Refresh();
            CslaDataProvider provider3 = this.Resources["ClassifierContactModelListData"] as CslaDataProvider;
            provider3.Refresh();
        }

        private void LBListMatchesIncluded_Click(object sender, RoutedEventArgs e)
        {
            IncrementHistoryCount();
            AddToBrowseHistory("List of all included matches", "MatchesIncluded", 0, "", "", 0, "", "", 0, "", "", 0, 0, "", 0, 0, 0, 0, "");
            TBPaperListTitle.Text = "List of all included matches";
            ShowIncludedMatchesPage("included");
        }

        private void LBListMatchesExcluded_Click(object sender, RoutedEventArgs e)
        {
            IncrementHistoryCount();
            AddToBrowseHistory("List of all excluded matches", "MatchesExcluded", 0, "", "", 0, "", "", 0, "", "", 0, 0, "", 0, 0, 0, 0, "");
            TBPaperListTitle.Text = "List of all excluded matches";
            ShowIncludedMatchesPage("excluded");
        }

        private void LBListAllInReview_Click(object sender, RoutedEventArgs e)
        {
            IncrementHistoryCount();
            AddToBrowseHistory("List of all matches in review (included and excluded)", "MatchesIncludedAndExcluded",
                0, "", "", 0, "", "", 0, "", "", 0, 0, "", 0, 0, 0, 0, "");
            TBPaperListTitle.Text = "List of all matches in review (included and excluded)";
            ShowIncludedMatchesPage("all");
        }

        private void LBListAllRelatedItemsWithThisCode_Click(object sender, RoutedEventArgs e)
        {
            string attributeIDs = "";
            string attributeNames = "";
            if ((codesSelectControlMAGSelect.SelectedAttributeSet() == null) && codesSelectControlMAGSelect.SelectedAttributes().Count == 0)
            {
                MessageBox.Show("Please select one or more codes");
            }
            else
            {
                if (codesSelectControlMAGSelect.SelectedAttributes().Count == 0)
                {
                    attributeIDs = codesSelectControlMAGSelect.SelectedAttributeSet().AttributeId.ToString();
                    attributeNames = codesSelectControlMAGSelect.SelectedAttributeSet().AttributeName.ToString();
                }
                foreach (AttributeSet attribute in codesSelectControlMAGSelect.SelectedAttributes())
                {
                    if (attributeIDs == "")
                    {
                        attributeIDs = attribute.AttributeId.ToString();
                        attributeNames = attribute.AttributeName;
                    }
                    else
                    {
                        attributeIDs += "," + attribute.AttributeId.ToString();
                        attributeNames += ", OR " + attribute.AttributeName;
                    }
                }
                IncrementHistoryCount();
                AddToBrowseHistory("List of all item matches with this code", "ReviewMatchedPapersWithThisCode", 0,
                    "", "", 0, "", "", 0, "", attributeIDs, 0, 0, "", 0, 0, 0, 0, "");
                ShowAllWithThisCode(attributeIDs);
            }
        }

        private void LBManualCheckIncluded_Click(object sender, RoutedEventArgs e)
        {
            MagBrowserImportedItems = false;
            this.ListIncludedThatNeedMatching.Invoke(sender, e);
        }

        private void LBManualCheckExcluded_Click(object sender, RoutedEventArgs e)
        {
            MagBrowserImportedItems = false;
            this.ListExcludedThatNeedMatching.Invoke(sender, e);
        }

        private void LBMNotMatchedIncluded_Click(object sender, RoutedEventArgs e)
        {
            MagBrowserImportedItems = false;
            this.ListIncludedNotMatched.Invoke(sender, e);
        }

        private void LBMNotMatchedExcluded_Click(object sender, RoutedEventArgs e)
        {
            MagBrowserImportedItems = false;
            this.ListExcludedNotMatched.Invoke(sender, e);
        }

        private void lbItemListMatchesIncluded_Click(object sender, RoutedEventArgs e)
        {
            MagBrowserImportedItems = false;
            this.ListIncludedMatched.Invoke(sender, e);
        }

        private void lbItemListMatchesExcluded_Click(object sender, RoutedEventArgs e)
        {
            MagBrowserImportedItems = false;
            this.ListExcludedMatched.Invoke(sender, e);
        }

        private void LBListPreviouslyMatchedRecords_Click(object sender, RoutedEventArgs e)
        {
            MagBrowserImportedItems = false;
            this.ListPreviouslyMatched.Invoke(sender, e);
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
                MagPaperList mpl = provider.Data as MagPaperList;
                if (mpl != null)
                {
                    tbCountPaperListGrid.Text = "Total number of items in this list: " + mpl.TotalItemCount.ToString();
                }
                SetSelected(provider);
                GetAssociatedTopics();

                if (provider.FactoryParameters != null)
                {
                    MagPaperListSelectionCriteria selectionCriteria = provider.FactoryParameters[0] as MagPaperListSelectionCriteria;
                    if (selectionCriteria != null)
                    {
                        if (selectionCriteria.ListType == "MagRelatedPapersRunList" ||
                            selectionCriteria.ListType == "MagAutoUpdateRunPapersList")
                        {
                            PaperListBibliographyGrid.Columns["SimilarityScoreColumn"].IsVisible = true;
                        }
                        else
                        {
                            PaperListBibliographyGrid.Columns["SimilarityScoreColumn"].IsVisible = false;
                        }
                    }
                }

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

        private void lbSelectAll_Click(object sender, RoutedEventArgs e)
        {
            CslaDataProvider provider = ((CslaDataProvider)this.Resources["TopicPaperListData"]);
            if (provider != null && provider.Data != null)
            {
                AddAllToSelectedList(provider.Data as MagPaperList);
            }
        }

        private void lbSelectAllPapers_Click(object sender, RoutedEventArgs e)
        {
            CslaDataProvider provider = ((CslaDataProvider)this.Resources["PaperListData"]);
            if (provider != null && provider.Data != null)
            {
                AddAllToSelectedList(provider.Data as MagPaperList);
            }
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
                    hl.IsTabStop = false;
                    hl.FontSize = i;
                    hl.Margin = new Thickness(5, 5, 5, 5);
                    //if (fos.PaperCount > _maxFieldOfStudyPaperCount)
                    //{
                    //    hl.NavigateUri = new Uri("https://academic.microsoft.com/topic/" +
                    //        fos.FieldOfStudyId.ToString());
                    //    hl.TargetName = "_blank";
                    //    hl.Foreground = new SolidColorBrush(Colors.DarkGray);
                    //    hl.FontStyle = FontStyles.Italic;
                    //}
                    hl.Click += HlNavigateToTopic_Click;
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
            IncrementHistoryCount();
            HyperlinkButton hl = sender as HyperlinkButton;
            if (hl != null)
            {
                AddToBrowseHistory("Browse topic: " + hl.Content.ToString(), "BrowseTopic", 0, "", "", 0, "", "",
                    Convert.ToInt64(hl.Tag), hl.Content.ToString(), "", 0, 0, "", 0, 0, 0, 0, "");
                Panes.SelectedIndex = 4;
                ShowTopicPage(Convert.ToInt64(hl.Tag), hl.Content.ToString());
            }
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
                    newHl.IsTabStop = false;
                    newHl.Tag = fos.FieldOfStudyId.ToString();
                    //if (fos.PaperCount > _maxFieldOfStudyPaperCount)
                    //{
                    //    newHl.NavigateUri = new Uri("https://academic.microsoft.com/topic/" +
                    //        fos.FieldOfStudyId.ToString());
                    //    newHl.TargetName = "_blank";
                    //    newHl.Foreground = new SolidColorBrush(Colors.DarkGray);
                    //    newHl.FontStyle = FontStyles.Italic;
                    //}
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
            selectionCriteria.PageSize = Convert.ToInt32((comboTopicPageSize.SelectedItem as ComboBoxItem).Tag);
            selectionCriteria.PageNumber = 0;
            selectionCriteria.ListType = "PaperFieldsOfStudyList";
            selectionCriteria.FieldOfStudyId = FieldOfStudyId;
            selectionCriteria.DateFrom = datePickerFilterTopicPapersFrom.SelectedDate == null ? "" :
                datePickerFilterTopicPapersFrom.SelectedDate.Value.Year.ToString() + "-" +
                PadZero(datePickerFilterTopicPapersFrom.SelectedDate.Value.Month.ToString()) + "-" +
                PadZero(datePickerFilterTopicPapersFrom.SelectedDate.Value.Day.ToString());
            selectionCriteria.DateTo = datePickerFilterTopicPapersTo.SelectedDate == null ? "" :
                datePickerFilterTopicPapersTo.SelectedDate.Value.Year.ToString() + "-" +
                PadZero(datePickerFilterTopicPapersTo.SelectedDate.Value.Month.ToString()) + "-" +
                PadZero(datePickerFilterTopicPapersTo.SelectedDate.Value.Day.ToString());
            provider.FactoryParameters.Add(selectionCriteria);
            provider.FactoryMethod = "GetMagPaperList";
            provider.Refresh();
        }

        private string PadZero(string s)
        {
            if (s.Length == 1)
            {
                s = "0" + s;
            }
            return s;
        }

        private void PaperListBibliographyGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            MagPaper paper = (sender as TextBlock).DataContext as MagPaper;
            IncrementHistoryCount();
            AddToBrowseHistory("Browse paper: " + paper.FullRecord, "PaperDetail", paper.PaperId, paper.FullRecord,
                paper.Abstract, paper.LinkedITEM_ID, paper.AllLinks, paper.FindOnWeb, 0, "", "", 0, 0, "", 0, 0, 0, 0, paper.FieldsOfStudyList);
            ShowPaperDetailsPage(paper.PaperId, paper.FullRecord, paper.Abstract, paper.AllLinks, 
                paper.FindOnWeb, paper.LinkedITEM_ID, paper.FieldsOfStudyList);
        }

        private void datePickerFilterTopics_SelectionChanged(object sender, Telerik.Windows.Controls.SelectionChangedEventArgs e)
        {
            //getPaperListForTopic(FieldOfStudyId);
        }

        // **************************** Managing page changes on the paper grid list views *****************

        private void PaperListBibliographyPager_PageIndexChanging(object sender, PageIndexChangingEventArgs e)
        {
            CslaDataProvider provider = this.Resources["PaperListData"] as CslaDataProvider;
            //provider.FactoryParameters.Clear();
            //MagPaperList mpl = provider.Data as MagPaperList;
            MagPaperListSelectionCriteria selectionCriteria = provider.FactoryParameters[0] as MagPaperListSelectionCriteria;
            if (selectionCriteria != null)
            {
                //selectionCriteria.PageSize = 20;
                selectionCriteria.PageNumber = e.NewPageIndex;
            }

            /*
            if (mpl.PaperIds == "" && (mpl.AttributeIds == "" || mpl.AttributeIds == null) && mpl.MagRelatedRunId == 0)
            {
                selectionCriteria.ListType = "ReviewMatchedPapers";
                selectionCriteria.Included = mpl.IncludedOrExcluded;
            }
            else if (mpl.PaperIds != null && mpl.PaperIds != "")
            {
                selectionCriteria.ListType = "PaperListById";
                selectionCriteria.PaperIds = mpl.PaperIds;
            }
            else if (mpl.AttributeIds != "" && mpl.AttributeIds != null)
            {
                selectionCriteria.ListType = "ReviewMatchedPapersWithThisCode";
                selectionCriteria.AttributeIds = mpl.AttributeIds;
            }
            else if (mpl.MagRelatedRunId != 0)
            {
                selectionCriteria.ListType = "MagRelatedPapersRunList";
                selectionCriteria.MagRelatedRunId = mpl.MagRelatedRunId;
            }
            else if (mpl.MagSearchText != "")
            {
                selectionCriteria.ListType = "MagSearchResultsList";
                selectionCriteria.MagSearchText = mpl.MagSearchText;
            }
            */
            //provider.FactoryParameters.Add(selectionCriteria);
            provider.FactoryMethod = "GetMagPaperList";
            provider.Refresh();
        }

        private void TopicPaperListBibliographyPager_PageIndexChanging(object sender, PageIndexChangingEventArgs e)
        {
            if (e.NewPageIndex - e.OldPageIndex > 100)
            {
                RadWindow.Alert("Sorry, moving forward more than 100 pages at a time is not possible at present");
            }
            else
            {
                CslaDataProvider provider = this.Resources["TopicPaperListData"] as CslaDataProvider;
                MagPaperList mpl = provider.Data as MagPaperList;
                mpl.PageSize = Convert.ToInt32((comboTopicPageSize.SelectedItem as ComboBoxItem).Tag);
                TopicPaperListBibliographyPager.PageSize = Convert.ToInt32((comboTopicPageSize.SelectedItem as ComboBoxItem).Tag);
                //TopicPaperListBibliographyPager.MoveToFirstPage();
                provider.FactoryParameters.Clear();
                MagPaperListSelectionCriteria selectionCriteria = new MagPaperListSelectionCriteria();
                selectionCriteria.PageSize = Convert.ToInt32((comboTopicPageSize.SelectedItem as ComboBoxItem).Tag);
                selectionCriteria.PageNumber = e.NewPageIndex;
                selectionCriteria.DateFrom = datePickerFilterTopicPapersFrom.SelectedDate == null ? "" :
                datePickerFilterTopicPapersFrom.SelectedDate.Value.Year.ToString() + "-" +
                PadZero(datePickerFilterTopicPapersFrom.SelectedDate.Value.Month.ToString()) + "-" +
                PadZero(datePickerFilterTopicPapersFrom.SelectedDate.Value.Day.ToString());
                selectionCriteria.DateTo = datePickerFilterTopicPapersTo.SelectedDate == null ? "" :
                    datePickerFilterTopicPapersTo.SelectedDate.Value.Year.ToString() + "-" +
                    PadZero(datePickerFilterTopicPapersTo.SelectedDate.Value.Month.ToString()) + "-" +
                    PadZero(datePickerFilterTopicPapersTo.SelectedDate.Value.Day.ToString());
                selectionCriteria.ListType = "PaperFieldsOfStudyList";
                selectionCriteria.FieldOfStudyId = mpl.FieldOfStudyId;
                provider.FactoryParameters.Add(selectionCriteria);
                provider.FactoryMethod = "GetMagPaperList";
                provider.Refresh();
            }
        }

        private void comboTopicPageSize_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (comboTopicPageSize != null && comboTopicPageSize.Items.Count > 0)
            {
                CslaDataProvider provider = this.Resources["TopicPaperListData"] as CslaDataProvider;
                MagPaperList mpl = provider.Data as MagPaperList;
                mpl.PageSize = Convert.ToInt32((comboTopicPageSize.SelectedItem as ComboBoxItem).Tag);
                TopicPaperListBibliographyPager.PageSize = Convert.ToInt32((comboTopicPageSize.SelectedItem as ComboBoxItem).Tag);
                provider.FactoryParameters.Clear();
                MagPaperListSelectionCriteria selectionCriteria = new MagPaperListSelectionCriteria();
                selectionCriteria.PageSize = Convert.ToInt32((comboTopicPageSize.SelectedItem as ComboBoxItem).Tag);
                selectionCriteria.PageNumber = 0;
                selectionCriteria.DateFrom = datePickerFilterTopicPapersFrom.SelectedDate == null ? "" :
                datePickerFilterTopicPapersFrom.SelectedDate.Value.Year.ToString() + "-" +
                PadZero(datePickerFilterTopicPapersFrom.SelectedDate.Value.Month.ToString()) + "-" +
                PadZero(datePickerFilterTopicPapersFrom.SelectedDate.Value.Day.ToString());
                selectionCriteria.DateTo = datePickerFilterTopicPapersTo.SelectedDate == null ? "" :
                    datePickerFilterTopicPapersTo.SelectedDate.Value.Year.ToString() + "-" +
                    PadZero(datePickerFilterTopicPapersTo.SelectedDate.Value.Month.ToString()) + "-" +
                    PadZero(datePickerFilterTopicPapersTo.SelectedDate.Value.Day.ToString());
                selectionCriteria.ListType = "PaperFieldsOfStudyList";
                selectionCriteria.FieldOfStudyId = mpl.FieldOfStudyId;
                provider.FactoryParameters.Add(selectionCriteria);
                provider.FactoryMethod = "GetMagPaperList";
                provider.Refresh();
            }
        }

        private void hlRefreshTopicPaperListData_Click(object sender, RoutedEventArgs e)
        {
            comboTopicPageSize_SelectionChanged(sender, null);
        }

        private void CitedByPager_PageIndexChanging(object sender, PageIndexChangingEventArgs e)
        {
            CslaDataProvider provider = this.Resources["CitedByListData"] as CslaDataProvider;
            MagPaperList mpl = provider.Data as MagPaperList;
            provider.FactoryParameters.Clear();
            MagPaperListSelectionCriteria selectionCriteria = new MagPaperListSelectionCriteria();
            selectionCriteria.PageSize = 20;
            selectionCriteria.PageNumber = e.NewPageIndex;
            selectionCriteria.ListType = "CitedByList";
            selectionCriteria.MagPaperId = mpl.PaperId;
            provider.FactoryParameters.Add(selectionCriteria);
            provider.FactoryMethod = "GetMagPaperList";
            provider.Refresh();
        }

        private void BibliographyPager_PageIndexChanging(object sender, PageIndexChangingEventArgs e)
        {
            CslaDataProvider provider = this.Resources["CitationPaperListData"] as CslaDataProvider;
            MagPaperList mpl = provider.Data as MagPaperList;
            provider.FactoryParameters.Clear();
            MagPaperListSelectionCriteria selectionCriteria = new MagPaperListSelectionCriteria();
            selectionCriteria.PageSize = 20;
            selectionCriteria.PageNumber = e.NewPageIndex;
            selectionCriteria.ListType = "CitationsList";
            selectionCriteria.MagPaperId = mpl.PaperId;
            provider.FactoryParameters.Add(selectionCriteria);
            provider.FactoryMethod = "GetMagPaperList";
            provider.Refresh();
        }

        // *************************** Showing / hiding abstracts *********************************
        private void HLExpandContract_Click(object sender, RoutedEventArgs e)
        {
            HyperlinkButton hl = sender as HyperlinkButton;
            if (hl.Tag.ToString() == "expand")
            {
                hl.Tag = "contract";
                hl.Content = "^^^ Contract ^^^";
                PaperGridAbstractRow.Height = new GridLength(50, GridUnitType.Auto);
            }
            else
            {
                hl.Tag = "expand";
                hl.Content = "<<< Expand >>>";
                PaperGridAbstractRow.Height = new GridLength(0);
            }
        }

        // Generic error handler for DataProviders
        private void CslaDataProvider_HandleDataChangeError(object sender, EventArgs e)
        {
            CslaDataProvider provider = sender as CslaDataProvider;
            if (provider != null)
            {
                if (provider.Error != null)
                {
                    RadWindow.Alert(provider.Error.Message);
                }
                else
                {
                    SetSelected(provider);
                    MagPaperList mpl = provider.Data as MagPaperList;
                    if (mpl != null)
                    {
                        tbCountTopicPaperListBibliographyGrid.Text = "Total number of items in this list: " + mpl.TotalItemCount.ToString();
                    }
                }
            }
        }

        // ***************************** Keeping track of, and navigating within, browsing history ***************************************
        public void AddToBrowseHistory(string title, string browseType, Int64 PaperId, string PaperFullRecord,
            string PaperAbstract, Int64 LinkedITEM_ID, string URLs, string FindOnWeb, Int64 FieldOfStudyId,
            string FieldOfStudy, string AttributeIds, int MagRelatedRunId, int MagAutoUpdateRunId, string AutoUpdateOrderBy,
            double AutoUpdateAutoUpdateScore, double AutoUpdateStudyTypeClassifierScore, double AutoUpdateUserClassifierScore,
            int AutoUpdateImportTopN, string FieldsOfStudyList)
        {
            MagBrowseHistory mbh = new MagBrowseHistory();
            mbh.Title = title;
            mbh.BrowseType = browseType;
            mbh.PaperId = PaperId;
            mbh.PaperFullRecord = PaperFullRecord;
            mbh.PaperAbstract = PaperAbstract;
            mbh.FieldOfStudyId = FieldOfStudyId;
            mbh.FieldOfStudy = FieldOfStudy;
            mbh.AttributeIds = AttributeIds;
            mbh.MagRelatedRunId = MagRelatedRunId;
            mbh.MagAutoUpdateRunId = MagAutoUpdateRunId;
            mbh.AutoUpdateAutoUpdateScore = AutoUpdateAutoUpdateScore;
            mbh.AutoUpdateOrderBy = AutoUpdateOrderBy;
            mbh.AutoUpdateStudyTypeClassifierScore = AutoUpdateStudyTypeClassifierScore;
            mbh.AutoUpdateUserClassifierScore = AutoUpdateUserClassifierScore;
            mbh.AutoUpdateUserTopN = AutoUpdateImportTopN;
            mbh.LinkedITEM_ID = LinkedITEM_ID;
            mbh.URLs = URLs;
            mbh.FindOnWeb = FindOnWeb;
            mbh.FieldsOfStudyListString = FieldsOfStudyList;
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            mbh.ContactId = ri.UserId;
            mbh.DateBrowsed = DateTime.Now;
            CslaDataProvider provider = this.Resources["HistoryListData"] as CslaDataProvider;
            if (provider != null)
            {
                MagBrowseHistoryList mbhl = provider.Data as MagBrowseHistoryList;
                if (mbhl != null)
                {
                    mbhl.Add(mbh);
                }
            }
            CheckForwardAndBackButtonState();
        }

        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            MagBrowseHistory mbh = (sender as HyperlinkButton).DataContext as MagBrowseHistory;
            if (mbh != null)
            {
                CslaDataProvider provider = this.Resources["HistoryListData"] as CslaDataProvider;
                if (provider != null)
                {
                    MagBrowseHistoryList mbhl = provider.Data as MagBrowseHistoryList;
                    if (mbhl != null)
                    {
                        mbhl.Remove(mbh);
                        CheckForwardAndBackButtonState();
                    }
                }
            }
        }

        private void lbClearHistory_Click(object sender, RoutedEventArgs e)
        {
            RadWindow.Confirm("Are you sure you want to clear your history?", this.doClearHistory);
        }

        private void doClearHistory(object sender, WindowClosedEventArgs e)
        {
            var result = e.DialogResult;
            if (result == true)
            {
                CslaDataProvider provider = this.Resources["HistoryListData"] as CslaDataProvider;
                if (provider != null)
                {
                    MagBrowseHistoryList mbhl = provider.Data as MagBrowseHistoryList;
                    if (mbhl != null)
                    {
                        while (mbhl.Count > 1)
                        {
                            mbhl.RemoveAt(0);
                        }
                        CurrentBrowsePosition = 1;
                        CheckForwardAndBackButtonState();
                    }
                }
            }
        }

        private void NavigateToThisPoint(int BrowsePosition)
        {
            if (BrowsePosition > 0)
            {
                CurrentBrowsePosition = BrowsePosition;
                CslaDataProvider provider = this.Resources["HistoryListData"] as CslaDataProvider;
                if (provider != null)
                {
                    MagBrowseHistoryList mbhl = provider.Data as MagBrowseHistoryList;
                    if (mbhl != null && BrowsePosition <= mbhl.Count)
                    {
                        MagBrowseHistory mbh = mbhl[BrowsePosition - 1];
                        switch (mbh.BrowseType)
                        {
                            case "History":
                                //ShowHistoryPage();
                                break;
                            case "Advanced":
                                Panes.SelectedIndex = 3;
                                ShowAdvancedPage();
                                break;
                            case "PaperDetail":
                                Panes.SelectedIndex = 4;
                                ShowPaperDetailsPage(mbh.PaperId, mbh.PaperFullRecord, mbh.PaperAbstract, mbh.URLs,
                                    mbh.FindOnWeb, mbh.LinkedITEM_ID, mbh.FieldsOfStudyListString);
                                break;
                            case "MatchesIncluded":
                                Panes.SelectedIndex = 4;
                                TBPaperListTitle.Text = mbh.Title;
                                ShowIncludedMatchesPage("included");
                                break;
                            case "MatchesExcluded":
                                Panes.SelectedIndex = 4;
                                TBPaperListTitle.Text = mbh.Title;
                                ShowIncludedMatchesPage("excluded");
                                break;
                            case "MatchesIncludedAndExcluded":
                                Panes.SelectedIndex = 4;
                                TBPaperListTitle.Text = mbh.Title;
                                ShowIncludedMatchesPage("all");
                                break;
                            case "ReviewMatchedPapersWithThisCode":
                                Panes.SelectedIndex = 4;
                                ShowAllWithThisCode(mbh.AttributeIds);
                                break;
                            case "MagRelatedPapersRunList":
                                Panes.SelectedIndex = 4;
                                TBPaperListTitle.Text = mbh.Title;
                                ShowRelatedRunPapers(mbh.MagRelatedRunId);
                                break;
                            case "BrowseTopic":
                                Panes.SelectedIndex = 4;
                                ShowTopicPage(mbh.FieldOfStudyId, mbh.FieldOfStudy);
                                break;
                            case "SelectedPapers":
                                TBPaperListTitle.Text = mbh.Title;
                                ShowSelectedPapersPage();
                                break;
                            case "RelatedPapers":
                                Panes.SelectedIndex = 0;
                                ShowRelatedPapersPage();
                                break;
                            case "AutoUpdate":
                                Panes.SelectedIndex = 1;
                                ShowAutoUpdatePage();
                                break;
                            case "MagAutoUpdateRunList":
                                Panes.SelectedIndex = 4;
                                TBPaperListTitle.Text = mbh.Title;
                                ShowAutoUpdateIdentifiedItems(mbh.MagAutoUpdateRunId, mbh.AutoUpdateOrderBy, mbh.AutoUpdateAutoUpdateScore,
                                    mbh.AutoUpdateStudyTypeClassifierScore, mbh.AutoUpdateUserClassifierScore, mbh.AutoUpdateUserTopN);
                                break;
                        }
                    }
                    else
                    {
                        MessageBox.Show("Index point too high");
                    }
                }
            }
            else
            {
                MessageBox.Show("Index point too low");
            }
            CheckForwardAndBackButtonState();
        }

        private void HyperlinkButton_Click_1(object sender, RoutedEventArgs e)
        {
            MagBrowseHistory mbh = (sender as HyperlinkButton).DataContext as MagBrowseHistory;
            if (mbh != null)
            {
                CslaDataProvider provider = this.Resources["HistoryListData"] as CslaDataProvider;
                if (provider != null)
                {
                    MagBrowseHistoryList mbhl = provider.Data as MagBrowseHistoryList;
                    if (mbhl != null)
                    {
                        NavigateToThisPoint(mbhl.IndexOf(mbh) + 1);
                    }
                }
            }
        }

        private void HLBack_Click(object sender, RoutedEventArgs e)
        {
            NavigateToThisPoint(CurrentBrowsePosition - 1);
        }

        private void HLForward_Click(object sender, RoutedEventArgs e)
        {
            NavigateToThisPoint(CurrentBrowsePosition + 1);
        }

        private void CheckForwardAndBackButtonState()
        {
            CslaDataProvider provider = this.Resources["HistoryListData"] as CslaDataProvider;
            if (provider != null)
            {
                MagBrowseHistoryList mbhl = provider.Data as MagBrowseHistoryList;
                if (mbhl != null)
                {
                    HistoryPane.Header = "History (" + CurrentBrowsePosition.ToString() + " / " +
                        mbhl.Count.ToString() + ")";
                    if (CurrentBrowsePosition > 1)
                    {
                        HLBack.IsEnabled = true;
                    }
                    else
                    {
                        HLBack.IsEnabled = false;
                    }
                    if (CurrentBrowsePosition < mbhl.Count)
                    {
                        HLForward.IsEnabled = true;
                    }
                    else
                    {
                        HLForward.IsEnabled = false;
                    }
                }
            }
        }

        public void IncrementHistoryCount()
        {
            CslaDataProvider provider = this.Resources["HistoryListData"] as CslaDataProvider;
            if (provider != null)
            {
                MagBrowseHistoryList mbhl = provider.Data as MagBrowseHistoryList;
                if (mbhl != null)
                {
                    CurrentBrowsePosition = mbhl.Count + 1; // otherwise we leave it where it is (i.e. user has navigated 'back')
                }
            }
        }

        // ******************************** Selected paper list handling *******************************

        private void ClearSelectedPaperList()
        {
            if (SelectedPaperIds.Count == 0)
            {
                RadWindow.Alert("You don't have anything selected");
                return;
            }
            RadWindow.Confirm("Are you sure you want to clear your list of selected items?", this.doClearSelectedPaperList);
        }

        private void doClearSelectedPaperList(object sender, WindowClosedEventArgs e)
        {
            var result = e.DialogResult;
            if (result == true)
            {
                SelectedPaperIds.Clear();
                UpdateSelectedCount();
                ClearSelectionsFromPaperLists();
            }
        }

        private void UpdateSelectedCount()
        {
            HLShowSelected.Content = "Selected (" + SelectedPaperIds.Count.ToString() + ")";
        }

        private void ClearSelectionsFromPaperLists()
        {
            ResetSelected("PaperListData");
            ResetSelected("TopicPaperListData");
            ResetSelected("CitationPaperListData");
            ResetSelected("CitedByListData");
            //ResetSelected("RecommendationsListData");
        }

        private void ResetSelected(string ProviderName)
        {
            CslaDataProvider provider = this.Resources[ProviderName] as CslaDataProvider;
            if (provider != null)
            {
                MagPaperList mpl = provider.Data as MagPaperList;
                if (mpl != null)
                {
                    mpl.ResetSelected();
                }
            }
        }

        private void HLClearSelected_Click(object sender, RoutedEventArgs e)
        {
            ClearSelectedPaperList();
        }

        private void RemoveFromSelectedList(Int64 PaperId)
        {
            int pos = SelectedPaperIds.IndexOf(PaperId);
            if (pos > -1)
                SelectedPaperIds.RemoveAt(pos);
            UpdateSelectedCount();
        }

        private bool IsInSelectedList(Int64 PaperId)
        {
            if (SelectedPaperIds.IndexOf(PaperId) > -1)
                return true;
            else
                return false;
        }

        private bool AddToSelectedList(Int64 PaperId)
        {
            bool ret = false;
            if (!IsInSelectedList(PaperId) && PaperId > 0)
            {
                SelectedPaperIds.Add(PaperId);
                UpdateSelectedCount();
                ret = true;
            }
            return ret;
        }

        private void AddAllToSelectedList(MagPaperList paperlist)
        {
            foreach (MagPaper p in paperlist)
            {
                AddToSelectedList(p.PaperId);
            }
        }

        private void HLSelectUnSelect_Click(object sender, RoutedEventArgs e)
        {
            MagPaper paper = (sender as HyperlinkButton).DataContext as MagPaper;
            if (paper != null)
            {
                if (paper.LinkedITEM_ID == 0)
                {
                    if (paper.IsSelected)
                    {
                        RemoveFromSelectedList(paper.PaperId);
                        paper.IsSelected = false;
                    }
                    else
                    {
                        AddToSelectedList(paper.PaperId);
                        paper.IsSelected = true;
                    }
                }
                else
                {
                    RadWindow.Alert("This paper is already in your review");
                }
            }
        }

        private void SetSelected(CslaDataProvider provider)
        {
            MagPaperList mpl = provider.Data as MagPaperList;
            if (mpl != null)
            {
                foreach (MagPaper paper in mpl)
                {
                    if (IsInSelectedList(paper.PaperId))
                    {
                        paper.IsSelected = true;
                    }
                    else
                    {
                        paper.IsSelected = false;
                    }
                }
            }
        }

        private string GetSelectedIds()
        {
            string ids = "";
            for (int i = 0; i < SelectedPaperIds.Count; i++)
            {
                if (i == 0)
                    ids = SelectedPaperIds[i].ToString();
                else
                    ids += "," + SelectedPaperIds[i].ToString();
            }
            return ids;
        }

        private void hlAddPaperToSelectedList_Click(object sender, RoutedEventArgs e)
        {
            HyperlinkButton hl = sender as HyperlinkButton;
            if (hl != null)
            {
                if (hl.Content.ToString() == "Add to selected list")
                {
                    AddToSelectedList(Convert.ToInt64(hl.Tag.ToString()));
                    hlAddPaperToSelectedList.Content = "Remove from selected list";
                }

                else
                {
                    RemoveFromSelectedList(Convert.ToInt64(hl.Tag.ToString()));
                    hlAddPaperToSelectedList.Content = "Add to selected list";
                }
            }
        }

        private void HLImportSelected_Click(object sender, RoutedEventArgs e)
        {
            if (BusyImportingRecords.IsRunning == true)
            {
                RadWindow.Alert("Importing currently in progress");
                return;
            }
            if (SelectedPaperIds.Count == 0)
            {
                RadWindow.Alert("You don't have anything selected");
                return;
            }
            if (SelectedPaperIds.Count > 20000)
            {
                RadWindow.Alert("Sorry. You can't import more than 20k items at a time.");
                return;
            }
            RadWindow.Confirm("Are you sure you want to import these items?", this.ImportSelected);
        }

        private void ImportSelected(object sender, WindowClosedEventArgs e)
        {
            var result = e.DialogResult;
            if (result == true)
            {
                DataPortal<MagItemPaperInsertCommand> dp2 = new DataPortal<MagItemPaperInsertCommand>();
                MagItemPaperInsertCommand command = new MagItemPaperInsertCommand(GetSelectedIds(), "SelectedPapers", 0, 0, "", 0, 0, 0, 0,
                    "", "", "", "", "", "","");
                dp2.ExecuteCompleted += (o, e2) =>
                {
                    BusyImportingRecords.IsRunning = false;
                    tbImportingRecords.Visibility = Visibility.Collapsed;
                    if (e2.Error != null)
                    {
                        RadWindow.Alert(e2.Error.Message);
                    }
                    else
                    {
                        if (e2.Object.NImported == SelectedPaperIds.Count)
                        {
                            RadWindow.Alert("Imported " + e2.Object.NImported.ToString() + " out of " +
                                SelectedPaperIds.Count.ToString() + " items");
                        }
                        else if (e2.Object.NImported != 0)
                        {
                            RadWindow.Alert("Some of these items were already in your review.\n\nImported " +
                                e2.Object.NImported.ToString() + " out of " + SelectedPaperIds.Count.ToString() +
                                " selected items");
                        }
                        else
                        {
                            RadWindow.Alert("All of these records were already in your review.");
                        }

                        MagBrowserImportedItems = true;
                        SelectedPaperIds.Clear();
                        ClearSelectionsFromPaperLists();
                        UpdateSelectedCount();
                        HLImportSelected.IsEnabled = true;
                    }
                };
                BusyImportingRecords.IsRunning = true;
                tbImportingRecords.Visibility = Visibility.Visible;
                HLImportSelected.IsEnabled = false;
                dp2.BeginExecute(command);
            }
        }

        // ******************************* Find topics using search box ********************************

        private void tbFindTopics_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (CleanText(tbFindTopics.Text).Length > 2)
            {
                if (this.timer != null && this.timer.IsEnabled)
                {
                    this.timer.Stop();
                    this.timer.Start();
                }
                else
                {
                    if (this.timer != null)
                    {
                        this.timer.Start();
                    }
                }
            }
            else
            {
                WPFindTopics.Children.Clear();
            }
        }

        public static string CleanText(string text)
        {
            Regex rgx = new Regex("[^a-zA-Z0-9 ]");

            text = rgx.Replace(text, " ").ToLower().Trim();
            while (text.IndexOf("  ") != -1)
            {
                text = text.Replace("  ", " ");
            }
            return text;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            this.timer.Stop();
            if (tbFindTopics.Text.Length > 2)
            {
                CslaDataProvider provider = this.Resources["SearchTopicsData"] as CslaDataProvider;
                if (provider != null)
                {
                    MagFieldOfStudyListSelectionCriteria selectionCriteria = new MagFieldOfStudyListSelectionCriteria();
                    selectionCriteria.ListType = "FieldOfStudySearchList";
                    selectionCriteria.SearchText = tbFindTopics.Text;
                    DataPortal<MagFieldOfStudyList> dp = new DataPortal<MagFieldOfStudyList>();
                    MagFieldOfStudyList mfsl = new MagFieldOfStudyList();
                    dp.FetchCompleted += (o, e2) =>
                    {
                        WPFindTopics.Children.Clear();
                        MagFieldOfStudyList FosList = e2.Object as MagFieldOfStudyList;
                        double i = 15;
                        foreach (MagFieldOfStudy fos in FosList)
                        {
                            HyperlinkButton newHl = new HyperlinkButton();
                            newHl.Content = fos.DisplayName;
                            newHl.Tag = fos.FieldOfStudyId.ToString();
                            newHl.FontSize = i;
                            newHl.IsTabStop = false;
                            //if (fos.PaperCount > _maxFieldOfStudyPaperCount)
                            //{
                            //    newHl.NavigateUri = new Uri("https://academic.microsoft.com/topic/" +
                            //        fos.FieldOfStudyId.ToString());
                            //    newHl.TargetName = "_blank";
                            //    newHl.Foreground = new SolidColorBrush(Colors.DarkGray);
                            //    newHl.FontStyle = FontStyles.Italic;
                            //}
                            //else
                            //{
                            newHl.Click += HlNavigateToTopic_Click;
                            newHl.Margin = new Thickness(5, 5, 5, 5);
                            WPFindTopics.Children.Add(newHl);
                            if (i > 10)
                            {
                                i -= 0.5;
                            }
                        }
                    };
                    dp.BeginFetch(selectionCriteria);
                }
            }
            else
            {
                WPFindTopics.Children.Clear();
                TextBlock tb = new TextBlock();
                tb.Text = "Search for topics in the box above";
                tb.Margin = new Thickness(5, 5, 5, 5);
                WPFindTopics.Children.Add(tb);
            }
        }

        // ********************************** Matching items from review to MAG *******************************

        private void LBMatchAllIncluded_Click(object sender, RoutedEventArgs e)
        {
            MatchOnAllOrFiltered = ((HyperlinkButton)sender).Tag.ToString();
            if (MatchOnAllOrFiltered == "All")
            {
                RadWindow.Confirm("Are you sure you want to match all the items in your review\n to OpenAlex records?", OnShowWizardDialogClosed);
            }
            else
            {
                if (codesSelectControlMAGSelect.SelectedAttributeSet() == null)
                {
                    RadWindow.Alert("Please select a code");
                }
                else
                {
                    RadWindow.Confirm("Are you sure you want to match the items with this code\n to OpenAlex records?", OnShowWizardDialogClosed);
                }
            }
        }

        private string MatchOnAllOrFiltered;

        private void OnShowWizardDialogClosed(object sender, WindowClosedEventArgs e)
        {
            if (e.DialogResult == true)
            {
                Int64 AttributeId = 0;
                if (MatchOnAllOrFiltered != "All")
                {
                    AttributeId = codesSelectControlMAGSelect.SelectedAttributeSet().AttributeId;
                }
                DataPortal<MagMatchItemsToPapersCommand> dp = new DataPortal<MagMatchItemsToPapersCommand>();
                MagMatchItemsToPapersCommand GetMatches = new MagMatchItemsToPapersCommand("FindMatches",
                    true, 0, AttributeId);
                dp.ExecuteCompleted += (o, e2) =>
                {
                    if (e2.Error != null)
                    {
                        RadWindow.Alert(e2.Error.Message);
                    }
                    else
                    {
                        MagMatchItemsToPapersCommand res = e2.Object as MagMatchItemsToPapersCommand;
                        RadWindow.Alert("Records submitted for matching. This can take a while...");
                    }
                };
                lbRefreshCounts.Visibility = Visibility.Visible;
                dp.BeginExecute(GetMatches);
            }
        }

        private void LbClearAllMatchesInReview_Click(object sender, RoutedEventArgs e)
        {
            HyperlinkButton hl = sender as HyperlinkButton;
            MatchOnAllOrFiltered = ((HyperlinkButton)sender).Tag.ToString();
            if (hl == null)
                return;
            string message = "";

            switch (MatchOnAllOrFiltered)
            {
                case "ALL":
                    RadWindow.Confirm("Are you sure you want to clear all matches in your review?", this.OnShowCheckClearMatchesDialogClosed);
                    break;
                case "ALL NON-MANUAL":
                    RadWindow.Confirm("Are you sure you want to clear all non-manual matches in your review?", this.OnShowCheckClearMatchesDialogClosed);
                    break;
                case "ALL WITH THIS CODE":
                    if (codesSelectControlMAGSelect.SelectedAttributeSet() == null)
                    {
                        RadWindow.Alert("Please select a code");
                        return;
                    }
                    RadWindow.Confirm("Are you sure you want to clear all matches with this code?", this.OnShowCheckClearMatchesDialogClosed);
                    break;
                case "ALL NON-MANUAL WITH THIS CODE":
                    if (codesSelectControlMAGSelect.SelectedAttributeSet() == null)
                    {
                        RadWindow.Alert("Please select a code");
                        return;
                    }
                    RadWindow.Confirm("Are you sure you want to clear all non-manual matches with this code?", this.OnShowCheckClearMatchesDialogClosed);
                    break;
            }
        }

        private void OnShowCheckClearMatchesDialogClosed(object sender, WindowClosedEventArgs e)
        {
            if (e.DialogResult == true)
            {
                Int64 AttributeId = 0;
                if (MatchOnAllOrFiltered.Contains("WITH THIS CODE"))
                {
                    AttributeId = codesSelectControlMAGSelect.SelectedAttributeSet().AttributeId;
                }
                DataPortal<MagMatchItemsToPapersCommand> dp = new DataPortal<MagMatchItemsToPapersCommand>();
                MagMatchItemsToPapersCommand ClearMatches = new MagMatchItemsToPapersCommand(MatchOnAllOrFiltered,
                    true, 0, AttributeId);
                dp.ExecuteCompleted += (o, e2) =>
                {
                    if (e2.Error != null)
                    {
                        RadWindow.Alert(e2.Error.Message);
                    }
                    else
                    {
                        MagMatchItemsToPapersCommand res = e2.Object as MagMatchItemsToPapersCommand;
                        RadWindow.Alert("Record(s) cleared");
                        RefreshCounts();
                    }
                };
                //lbRefreshCounts.Visibility = Visibility.Visible;
                dp.BeginExecute(ClearMatches);
            }
        }

        private void lbRefreshCounts_Click(object sender, RoutedEventArgs e)
        {
            RefreshCounts();
        }


        // ************************** Bring review up to date **********************

        private void LBAddNewRagRelatedPapersRun_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as HyperlinkButton).Tag.ToString() == "ClickToOpen")
            {
                RowCreateNewRelatedPapersRun.Height = new GridLength(50, GridUnitType.Auto);
                LBAddNewRagRelatedPapersRun.Content = "Adding new search for related papers (Click to close)";
                LBAddNewRagRelatedPapersRun.Tag = "ClickToClose";
                tbRelatedPapersRunDescription.Text = "";
            }
            else
            {
                RowCreateNewRelatedPapersRun.Height = new GridLength(0);
                LBAddNewRagRelatedPapersRun.Content = "Add new search for related papers";
                LBAddNewRagRelatedPapersRun.Tag = "ClickToOpen";
            }
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (RowSelectCodeRelatedPapersRun != null)
            {
                //RowSelectCodeRelatedPapersRun.Height = new GridLength(0);
                codesSelectControlRelatedPapersRun.Visibility = Visibility.Collapsed;
            }
        }

        private void RadioButton_Checked_1(object sender, RoutedEventArgs e)
        {
            if (RowSelectCodeRelatedPapersRun != null)
            {
                //RowSelectCodeRelatedPapersRun.Height = new GridLength(50, GridUnitType.Auto);
                codesSelectControlRelatedPapersRun.Visibility = Visibility.Visible;
            }
        }

        private void RadioButton_Checked_2(object sender, RoutedEventArgs e)
        {
            if (DatePickerRelatedPapersRun != null)
            {
                DatePickerRelatedPapersRun.Visibility = Visibility.Collapsed;
            }
        }

        private void RadioButton_Checked_3(object sender, RoutedEventArgs e)
        {
            if (DatePickerRelatedPapersRun != null)
            {
                DatePickerRelatedPapersRun.Visibility = Visibility.Visible;
            }
        }

        private MagRelatedPapersRun GetMagRelatedPapersRunValues(AttributeSet attribute)
        {
            MagRelatedPapersRun mrpr = new MagRelatedPapersRun();
            mrpr.UserDescription = tbRelatedPapersRunDescription.Text;
            if (attribute == null)
            {
                mrpr.AllIncluded = true;
                mrpr.AttributeId = 0;
            }
            else
            {
                mrpr.AttributeId = attribute.AttributeId;
                mrpr.AttributeName = attribute.AttributeName;
                mrpr.AllIncluded = false;
            }
            //mrpr.AutoReRun = cbRelatedPapersRunAutoRun.IsChecked == true ? true : false;
            if (RadioButtonRelatedPapersRunNoDateRestriction.IsChecked == true)
            {
                mrpr.DateFrom = null;
            }
            else
            {
                mrpr.DateFrom = DatePickerRelatedPapersRun.SelectedDate;
            }
            mrpr.Status = "Running";
            mrpr.Filtered = "";
            mrpr.DateRun = DateTime.Now;
            mrpr.Mode = (ComboRelatedPapersMode.SelectedItem as ComboBoxItem).Tag.ToString();
            if (mrpr.Mode == "New items in OpenAlex")
            {
                mrpr.Status = "Pending";
            }
            return mrpr;
        }

        private void DoAddRelatedRun(object sender, WindowClosedEventArgs e)
        {
            var result = e.DialogResult;
            if (result == true)
            {
                MagRelatedPapersRun mrpr = null;
                CslaDataProvider provider = this.Resources["RelatedPapersRunListData"] as CslaDataProvider;
                if (provider != null)
                {
                    MagRelatedPapersRunList mrprl = provider.Data as MagRelatedPapersRunList;
                    if (mrprl != null)
                    {
                        if (RadioButtonRelatedPapersRunAllIncluded.IsChecked == true)
                        {
                            mrpr = GetMagRelatedPapersRunValues(null);
                            mrprl.Add(mrpr);
                            mrprl.SaveItem(mrpr);
                        }
                        else
                        {
                            if (RadioButtonRelatedPapersRunWithCode.IsChecked == true)
                            {
                                mrpr = GetMagRelatedPapersRunValues(codesSelectControlRelatedPapersRun.SelectedAttributeSet());
                                mrprl.Add(mrpr);
                                mrprl.SaveItem(mrpr);
                            }
                            else
                            {
                                foreach (AttributeSet aSet in codesSelectControlRelatedPapersRun.SelectedAttributeSet().Attributes)
                                {
                                    mrpr = GetMagRelatedPapersRunValues(aSet);
                                    mrprl.Add(mrpr);
                                    mrprl.SaveItem(mrpr);
                                }
                            }
                        }
                        DataLakeTimer.Start();
                        RadWindow.Alert("Running... (The grid below will refresh every 30 seconds)");
                        RowCreateNewRelatedPapersRun.Height = new GridLength(0);
                        LBAddNewRagRelatedPapersRun.Content = "Add new search for related papers / auto update search for review";
                        LBAddNewRagRelatedPapersRun.Tag = "ClickToOpen";
                    }
                }
            }
        }

        private void DataLakeTimer_Tick(object sender, EventArgs e)
        {
            CslaDataProvider provider = this.Resources["RelatedPapersRunListData"] as CslaDataProvider;
            if (provider != null)
            {
                provider.Refresh();
            }
        }


        private void CslaDataProvider_DataChanged_2(object sender, EventArgs e)
        {
            CslaDataProvider provider = this.Resources["RelatedPapersRunListData"] as CslaDataProvider;
            if (provider != null)
            {
                if (provider.Error != null)
                {
                    RadWindow.Alert(provider.Error.Message);
                }
                else
                {
                    MagRelatedPapersRunList rprl = provider.Data as MagRelatedPapersRunList;
                    if (rprl != null && rprl.Count > 0)
                    {
                        MagRelatedPapersRun rpr = rprl[rprl.Count - 1];
                        if (rpr.Status != "Running")
                            DataLakeTimer.Stop();
                    }
                }
            }
        }

        private void LBAddRelatedPapersRun_Click(object sender, RoutedEventArgs e)
        {
            if (nMatchedRecords == 0)
            {
                RadWindow.Alert("Please match records before running a search");
                return;
            }

            if (tbRelatedPapersRunDescription.Text == "")
            {
                RadWindow.Alert("Please enter a description");
                return;
            }

            if (RadioButtonRelatedPapersRunNoDateRestriction.IsChecked == false &&
                DatePickerRelatedPapersRun.SelectedDate == null)
            {
                RadWindow.Alert("Please select a date to date items from");
                return;
            }
            if (RadioButtonRelatedPapersRunWithCode.IsChecked == true &&
                   codesSelectControlRelatedPapersRun.SelectedAttributeSet() == null)
            {
                RadWindow.Alert("Please select a code to filter by");
                return;
            }
            RadWindow.Confirm("Are you sure you want to create this search?", this.DoAddRelatedRun);
        }


        private void ComboRelatedPapersMode_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (ComboRelatedPapersMode != null && ComboRelatedPapersMode.Items != null &&
                ComboRelatedPapersMode.SelectedIndex == 7)
            {
                //cbRelatedPapersRunAutoRun.IsEnabled = true;
                RadioButtonRelatedPapersRunNoDateRestriction.IsChecked = true;
                //RadioButtonRelatedPapersRunChildrenOfCode.IsEnabled = true;
                RadioButtonRelatedPapersRunNoDateRestriction.IsChecked = true;
                RadioButtonRelatedPapersRunDateFilter.IsEnabled = false;
                DatePickerRelatedPapersRun.SelectedDate = null;
            }
            else
            {
                if (RadioButtonRelatedPapersRunDateFilter != null)
                {
                    RadioButtonRelatedPapersRunDateFilter.IsEnabled = true;
                }
            }
        }

        private void HyperlinkButton_Click_2(object sender, RoutedEventArgs e)
        {
            HyperlinkButton hlb = sender as HyperlinkButton;
            if (hlb != null)
            {
                MagRelatedPapersRun pr = hlb.DataContext as MagRelatedPapersRun;
                if (pr != null)
                {
                    if (pr.Status == "Running" && pr.DateRun > DateTime.Now.AddHours(-2))
                    {
                        RadWindow.Alert("Sorry - can't delete while running when less than two hours old");
                        return;
                    }
                    else
                    {
                        RememberThisMagRelatedPapersRun = pr;
                        RadWindow.Confirm("Are you sure you want to delete this row?", this.doDeleteMagRelatedPapersRun);
                    }
                }
            }
        }

        MagRelatedPapersRun RememberThisMagRelatedPapersRun; // temporary variable to store a specific row while a dialog is showing

        private void doDeleteMagRelatedPapersRun(object sender, WindowClosedEventArgs e)
        {
            var result = e.DialogResult;
            if (result == true)
            {
                CslaDataProvider provider = this.Resources["RelatedPapersRunListData"] as CslaDataProvider;
                if (provider != null)
                {
                    MagRelatedPapersRunList runList = provider.Data as MagRelatedPapersRunList;
                    if (runList != null)
                    {
                        runList.Remove(RememberThisMagRelatedPapersRun);
                    }
                }
            }
        }

        private void HyperlinkButton_Click_3(object sender, RoutedEventArgs e)
        {
            HyperlinkButton hlb = sender as HyperlinkButton;
            if (hlb != null)
            {
                MagRelatedPapersRun pr = hlb.DataContext as MagRelatedPapersRun;
                if (pr != null)
                {
                    IncrementHistoryCount();
                    AddToBrowseHistory("Papers identified from related papers search", "MagRelatedPapersRunList", 0,
                        "", "", 0, "", "", 0, "", "", pr.MagRelatedRunId, 0, "", 0, 0, 0, 0, "");
                    TBPaperListTitle.Text = "Papers identified from auto-identification run";
                    ShowRelatedRunPapers(pr.MagRelatedRunId);
                }
            }
        }

        private void lbRelatedRunSearchPmids_Click(object sender, RoutedEventArgs e)
        {
            if (tbRelatedRunPmids.Text != "")
            {
                CslaDataProvider provider = this.Resources["RelatedPapersRunListData"] as CslaDataProvider;
                if (provider != null)
                {
                    MagRelatedPapersRunList mrprl = provider.Data as MagRelatedPapersRunList;
                    if (mrprl != null)
                    {
                        MagRelatedPapersRun mrpr = new MagRelatedPapersRun();
                        mrpr.UserDescription = "Search for PubMed IDs";
                        mrpr.AllIncluded = true;
                        mrpr.AttributeId = 0;
                        mrpr.DateFrom = null;
                        mrpr.Status = "Running";
                        mrpr.Filtered = "";
                        mrpr.DateRun = DateTime.Now;
                        mrpr.Mode = "PubMed ID search";
                        mrpr.Pmids = tbRelatedRunPmids.Text;

                        mrprl.Add(mrpr);
                        mrprl.SaveItem(mrpr);
                        DataLakeTimer.Start();
                        RadWindow.Alert("Running... (The grid below will refresh every 30 seconds)");
                        RowCreateNewPmidSearch.Height = new GridLength(0);
                        //LBOpenPmidSearch.Content = "Add search by PubMed ID";
                        //LBOpenPmidSearch.Tag = "ClickToOpen";
                    }
                }
            }
            else
            {
                RadWindow.Alert("Please enter some PubMed Ids!");
            }
        }

        private void LBOpenPmidSearch_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as HyperlinkButton).Tag.ToString() == "ClickToOpen")
            {
                RowCreateNewPmidSearch.Height = new GridLength(50, GridUnitType.Auto);
                //LBOpenPmidSearch.Content = "Adding search by PubMed ID (Click to close)";
                //LBOpenPmidSearch.Tag = "ClickToClose";
                tbRelatedPapersRunDescription.Text = "";
            }
            else
            {
                RowCreateNewPmidSearch.Height = new GridLength(0);
                //LBOpenPmidSearch.Content = "Add search by PubMed ID";
                //LBOpenPmidSearch.Tag = "ClickToOpen";
            }
        }

        private void HyperlinkButton_Click_4(object sender, RoutedEventArgs e)
        {
            if (BusyImportingRecords.IsRunning == true)
            {
                RadWindow.Alert("Importing currently in progress");
                return;
            }

            HyperlinkButton hlb = sender as HyperlinkButton;
            if (hlb != null)
            {
                MagRelatedPapersRun pr = hlb.DataContext as MagRelatedPapersRun;
                if (pr != null)
                {
                    SelectedLinkButton = hlb;
                    if (pr.NPapers == 0)
                    {
                        RadWindow.Alert("There are no items to import.");
                        return;
                    }
                    if (pr.NPapers > 20000)
                    {
                        RadWindow.Alert("Sorry. You can't import more than 20k items at a time.");
                        return;
                    }
                    if (pr.UserStatus == "Checked")
                    {
                        RememberThisMagRelatedPapersRun = pr;
                        RadWindow.Confirm("Are you sure you want to import these items?\n(This set is already marked as 'checked'.)", this.DoImportItems);
                    }
                    else if (pr.UserStatus == "Unchecked" || pr.UserStatus == "Not imported") // retaining 'unchecked' for legacy purposes
                    {
                        RememberThisMagRelatedPapersRun = pr;
                        RadWindow.Confirm("Are you sure you want to import these items?", this.DoImportItems);
                    }
                    else if (pr.UserStatus == "Imported")
                    {
                        RadWindow.Alert("These items are already imported");
                    }
                }
            }
        }

        private HyperlinkButton SelectedLinkButton;

        private void DoImportItems(object sender, WindowClosedEventArgs e)
        {
            var result = e.DialogResult;
            if (result == true)
            {
                if (RememberThisMagRelatedPapersRun != null)
                {
                    int num_in_run = RememberThisMagRelatedPapersRun.NPapers;
                    string pubTypeFilters = getRelatedRunPubTypeFilters();
                    DataPortal<MagItemPaperInsertCommand> dp2 = new DataPortal<MagItemPaperInsertCommand>();
                    MagItemPaperInsertCommand command = new MagItemPaperInsertCommand("", "RelatedPapersSearch",
                        RememberThisMagRelatedPapersRun.MagRelatedRunId, 0, "", 0, 0, 0, 0, RelatedRunTextFilterJournal.Text,
                        RelatedRunTextFilterDOI.Text, RelatedRunTextFilterURL.Text, RelatedRunTextFilterTitle.Text, "", "", pubTypeFilters);
                    string Filtered = RelatedRunTextFilterDOI.Text != "" || RelatedRunTextFilterURL.Text != "" || RelatedRunTextFilterTitle.Text != "" ||
                        RelatedRunTextFilterJournal.Text != "" || pubTypeFilters != "" ? " or filtered out" : ".";
                    dp2.ExecuteCompleted += (o, e2) =>
                    {
                        BusyImportingRecords.IsRunning = false;
                        tbImportingRecords.Visibility = Visibility.Collapsed;
                        if (e2.Error != null)
                        {
                            RadWindow.Alert(e2.Error.Message);
                        }
                        else
                        {
                            if (e2.Object.NImported == num_in_run)
                            {
                                RadWindow.Alert("Imported " + e2.Object.NImported.ToString() + " out of " +
                                    num_in_run.ToString() + " items");
                            }
                            else if (e2.Object.NImported != 0)
                            {
                                RadWindow.Alert("Some of these items were already in your review" + Filtered + "\n\nImported " +
                                    e2.Object.NImported.ToString() + " out of " + num_in_run.ToString() +
                                    " new items");
                            }
                            else
                            {
                                RadWindow.Alert("All of these records were already in your review" + Filtered);
                            }
                            //SelectedLinkButton.IsEnabled = true; - no need to renable, as it's destroyed in the refresh below
                            CslaDataProvider provider = this.Resources["RelatedPapersRunListData"] as CslaDataProvider;
                            provider.Refresh();
                            MagBrowserImportedItems = true;
                        }
                    };
                    BusyImportingRecords.IsRunning = true;
                    tbImportingRecords.Visibility = Visibility.Visible;
                    SelectedLinkButton.IsEnabled = false;
                    dp2.BeginExecute(command);
                }
            }
        }

        private string getRelatedRunPubTypeFilters()
        {
            /*
            string ret = addPubTypeFilter(cbRelatedRunFilterPubTypeJournal, "");
            ret = addPubTypeFilter(cbRelatedRunFilterPubTypeUnknown, ret);
            ret = addPubTypeFilter(cbRelatedRunFilterPubTypeConferencePaper, ret);
            ret = addPubTypeFilter(cbRelatedRunFilterPubTypeBookChapter, ret);
            ret = addPubTypeFilter(cbRelatedRunFilterPubTypeBook, ret);
            ret = addPubTypeFilter(cbRelatedRunFilterPubTypeDataset, ret);
            ret = addPubTypeFilter(cbRelatedRunFilterPubTypeRepository, ret);
            ret = addPubTypeFilter(cbRelatedRunFilterPubTypeThesis, ret);
            return ret;
            */
            return "";
        }

        private void HyperlinkButton_Click_5(object sender, RoutedEventArgs e)
        {
            HyperlinkButton hlb = sender as HyperlinkButton;
            if (hlb != null)
            {
                MagRelatedPapersRun pr = hlb.DataContext as MagRelatedPapersRun;
                if (pr != null)
                {
                    if (pr.UserStatus == "Unchecked")
                    {
                        RememberThisMagRelatedPapersRun = pr;
                        RadWindow.Confirm("Are you sure you want to mark this set as having been checked?", this.DoSetRelatedRunToChecked);
                    }
                    else if (pr.UserStatus == "Imported")
                    {
                        RadWindow.Alert("You have imported these records already");
                    }
                    else
                        RadWindow.Alert("You have marked this set as having been checked.");
                }
            }
        }

        private void DoSetRelatedRunToChecked(object sender, WindowClosedEventArgs e)
        {
            var result = e.DialogResult;
            if (result == true)
            {
                RememberThisMagRelatedPapersRun.UserStatus = "Checked";
                RememberThisMagRelatedPapersRun.BeginSave();
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            if (cb != null)
            {
                MagRelatedPapersRun r = cb.DataContext as MagRelatedPapersRun;
                if (r != null)
                {
                    r.AutoReRun = true;
                    r.BeginSave();
                }
            }
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            if (cb != null)
            {
                MagRelatedPapersRun r = cb.DataContext as MagRelatedPapersRun;
                if (r != null)
                {
                    r.AutoReRun = false;
                    r.BeginSave();
                }
            }
        }


        private void LBOpenFiltersRelatedRun_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as HyperlinkButton).Tag.ToString() == "Open")
            {
                ColumnFiltersRelatedRun.Width = new GridLength(250, GridUnitType.Auto);
                LBOpenFiltersRelatedRun.Content = "Close and clear filters";
                LBOpenFiltersRelatedRun.Tag = "Close";
                tbAutoUpdateDescription.Text = "";
            }
            else
            {
                ColumnFiltersRelatedRun.Width = new GridLength(0);
                LBOpenFiltersRelatedRun.Content = "Open filters";
                LBOpenFiltersRelatedRun.Tag = "Open";
                RelatedRunTextFilterJournal.Text = "";
                RelatedRunTextFilterURL.Text = "";
                RelatedRunTextFilterDOI.Text = "";
                RelatedRunTextFilterTitle.Text = "";
                /*
                cbRelatedRunFilterPubTypeJournal.IsChecked = false;
                cbRelatedRunFilterPubTypeConferencePaper.IsChecked = false;
                cbRelatedRunFilterPubTypeBookChapter.IsChecked = false;
                cbRelatedRunFilterPubTypeBook.IsChecked = false;
                cbRelatedRunFilterPubTypeDataset.IsChecked = false;
                cbRelatedRunFilterPubTypeRepository.IsChecked = false;
                cbRelatedRunFilterPubTypeThesis.IsChecked = false;
                cbRelatedRunFilterPubTypeUnknown.IsChecked = false;
                */
            }
        }


        // **************************** Simulation studies in research & development *******************************

        private void rbSimulationYear_Click(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            if (rb != null)
            {
                if (rb.Tag.ToString() == "ShowCodesControl")
                {
                    DatePickerSimulation.Visibility = Visibility.Collapsed;
                    DatePickerSimulationEnd.Visibility = Visibility.Collapsed;
                    tbEndDate.Visibility = Visibility.Collapsed;
                    codesSelectControlSimulation.Visibility = Visibility.Visible;
                }
                else
                {
                    DatePickerSimulation.Visibility = Visibility.Visible;
                    DatePickerSimulationEnd.Visibility = Visibility.Visible;
                    tbEndDate.Visibility = Visibility.Visible;
                    codesSelectControlSimulation.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void lbRunSimulation_Click(object sender, RoutedEventArgs e)
        {
            if (nMatchedRecords == 0)
            {
                RadWindow.Alert("Please match records before running simulations");
                return;
            }

            RadWindow.Confirm("Are you sure you want to create and run this simulation study?", this.CreateAndRunSimulation);
        }

        private void CreateAndRunSimulation(object sender, WindowClosedEventArgs e)
        {
            var result = e.DialogResult;
            if (result == true)
            {
                DataPortal<MagCheckContReviewRunningCommand> dp = new DataPortal<MagCheckContReviewRunningCommand>();
                MagCheckContReviewRunningCommand check = new MagCheckContReviewRunningCommand();
                dp.ExecuteCompleted += (o, e2) =>
                {
                    //BusyLoading.IsRunning = false;
                    if (e2.Error != null)
                    {
                        RadWindow.Alert(e2.Error.Message);
                    }
                    else
                    {
                        MagCheckContReviewRunningCommand chk = e2.Object as MagCheckContReviewRunningCommand;
                        if (chk != null)
                        {
                            if (chk.IsRunningMessage == "running")
                            {
                                RadWindow.Alert("Sorry, another pipeline is currently running");
                            }
                            else
                            {
                                RunSimulation();
                            }
                        }
                    }
                };
                //BusyLoading.IsRunning = true;
                dp.BeginExecute(check);
            }
        }

        private void RunSimulation()
        {
            DateTime SimulationYear = Convert.ToDateTime("1/1/1753");
            DateTime CreatedDate = Convert.ToDateTime("1/1/1753");
            DateTime SimulationYearEnd = Convert.ToDateTime("1/1/2025");
            DateTime CreatedDateEnd = Convert.ToDateTime("1/1/2025");
            Int64 AttributeId = 0;
            Int64 AttributeIdFilter = 0;
            ClassifierContactModel UserModel = null;
            if (rbSimulationYear.IsChecked == true)
            {
                SimulationYear = DatePickerSimulation.SelectedValue.Value;
                SimulationYearEnd = DatePickerSimulationEnd.SelectedValue.Value;
                AttributeId = 0;
            }
            if (rbSimulationCreatedDate.IsChecked == true)
            {
                CreatedDate = DatePickerSimulation.SelectedValue.Value;
                CreatedDateEnd = DatePickerSimulationEnd.SelectedValue.Value;
                AttributeId = 0;
            }
            if (rbSimulationWithThisCode.IsChecked == true)
            {
                if (codesSelectControlSimulation.SelectedAttributeSet() != null)
                {
                    AttributeId = codesSelectControlSimulation.SelectedAttributeSet().AttributeId;
                }
                else
                {
                    RadWindow.Alert("Please select a code");
                    return;
                }
            }
            if (cbSimulationFilterByThisCode.IsChecked == true)
            {
                if (codesSelectControlSimulationFilter.SelectedAttributeSet() != null)
                {
                    AttributeIdFilter = codesSelectControlSimulationFilter.SelectedAttributeSet().AttributeId;
                }
                else
                {
                    RadWindow.Alert("Please select a code if you want to filter by a code");
                    return;
                }
            }
            UserModel = comboSimulationUserModels.SelectedItem as ClassifierContactModel;

            MagSimulation newSimulation = new MagSimulation();
            newSimulation.Year = SimulationYear.Year;
            newSimulation.YearEnd = SimulationYearEnd.Year;
            newSimulation.CreatedDate = CreatedDate;
            newSimulation.CreatedDateEnd = CreatedDateEnd;
            newSimulation.WithThisAttributeId = AttributeId;
            newSimulation.FilteredByAttributeId = AttributeIdFilter;
            newSimulation.SearchMethod = (comboSimulationSearchMethod.SelectedItem as ComboBoxItem).Content.ToString();
            newSimulation.NetworkStatistic = (comboSimulationNetworkStats.SelectedItem as ComboBoxItem).Content.ToString();
            newSimulation.StudyTypeClassifier = (comboSimulationStudyTypeClassifier.SelectedItem as ComboBoxItem).Content.ToString();
            newSimulation.UserClassifierModelId = (UserModel != null ? UserModel.ModelId : 0);
            newSimulation.UserClassifierReviewId = (UserModel != null ? UserModel.ReviewId : 0);
            newSimulation.FosThreshold = SimulationEditFoSThreshold.Value.Value;
            newSimulation.ScoreThreshold = SimulationEditScoreThreshold.Value.Value;
            newSimulation.Status = "Pending";

            CslaDataProvider provider = this.Resources["MagSimulationListData"] as CslaDataProvider;
            if (provider != null)
            {
                MagSimulationList SimList = provider.Data as MagSimulationList;
                if (SimList != null)
                {
                    SimList.Add(newSimulation);
                    SimList.SaveItem(newSimulation);
                }
            }
        }

        private void cbSimulationFilterByThisCode_Checked(object sender, RoutedEventArgs e)
        {
            codesSelectControlSimulationFilter.Visibility = Visibility.Visible;
        }

        private void cbSimulationFilterByThisCode_Unchecked(object sender, RoutedEventArgs e)
        {
            codesSelectControlSimulationFilter.Visibility = Visibility.Collapsed;
        }

        private MagSimulation CurrentlySelectedMagSimulation;

        private void HyperlinkButton_Click_8(object sender, RoutedEventArgs e)
        {
            HyperlinkButton hl = sender as HyperlinkButton;
            if (hl == null)
                return;
            MagSimulation ms = hl.DataContext as MagSimulation;
            if (ms == null)
                return;
            if (ms.Status == "Running" || ms.Status == "Pending")
            {
                RadWindow.Alert("Sorry, can't delete this simulation");
                return;
            }
            CurrentlySelectedMagSimulation = ms;
            RadWindow.Confirm("Are you sure you want to delete this simulation?", this.DoDeleteSimulation);
        }

        private void DoDeleteSimulation(object sender, WindowClosedEventArgs e)
        {
            var result = e.DialogResult;
            if (result == true)
            {
                if (CurrentlySelectedMagSimulation == null)
                    return;
                CslaDataProvider provider = this.Resources["MagSimulationListData"] as CslaDataProvider;
                if (provider != null)
                {
                    MagSimulationList SimList = provider.Data as MagSimulationList;
                    if (SimList != null)
                    {
                        SimList.Remove(CurrentlySelectedMagSimulation);
                        //SimList.SaveItem(ms);
                    }
                }
            }
        }

        private void HyperlinkButton_Click_12(object sender, RoutedEventArgs e)
        {
            HyperlinkButton hl = sender as HyperlinkButton;
            if (hl == null)
                return;
            MagSimulation ms = hl.DataContext as MagSimulation;
            if (ms == null)
                return;
            if (ms.TP > 0 || ms.FN > 0 || ms.FP > 0) // i.e. data are downloaded already
                return;
            if (ms.Status == "Pending" || ms.Status == "Complete" || ms.Status == "Failed")
            {
                RadWindow.Alert("Data not available for download");
                return;
            }
            if (ms.CreatedDate.Date.AddHours(3) > DateTime.Now)
            {
                RadWindow.Alert("Job running less than 3 hours");
                return;
            }
            CurrentlySelectedMagSimulation = ms;
            RadWindow.Confirm("Are you sure you want to download these data?", this.DoDownloadSimulationDataOnFail);
        }

        private void DoDownloadSimulationDataOnFail(object sender, WindowClosedEventArgs e)
        {
            var result = e.DialogResult;
            if (result == true)
            {
                if (CurrentlySelectedMagSimulation == null)
                    return;

                CurrentlySelectedMagSimulation.BeginEdit();
                CurrentlySelectedMagSimulation.Status = "Attempting download";
                CurrentlySelectedMagSimulation.BeginSave();
                CslaDataProvider provider = this.Resources["MagSimulationListData"] as CslaDataProvider;
                if (provider != null)
                {
                    MagSimulationList SimList = provider.Data as MagSimulationList;
                    if (SimList != null)
                    {
                        SimList.SaveItem(CurrentlySelectedMagSimulation);
                    }
                }
            }
        }

        private int CurrentMagSimulationId; // Just stores the current SimulationId for the graph being displayed (the download data feature uses this)
        private void HyperlinkButton_Click_9(object sender, RoutedEventArgs e)
        {
            MagSimulation ms = (sender as HyperlinkButton).DataContext as MagSimulation;
            if (ms != null)
            {
                CurrentMagSimulationId = ms.MagSimulationId;
                GridSimulationResults.Visibility = Visibility.Visible;
                GridCreateSimulations.Visibility = Visibility.Collapsed;
                GetDataBySpecifiedScore();
            }
        }

        private void GetDataBySpecifiedScore()
        {
            hlDownloadDataFromBrowser.Visibility = Visibility.Collapsed;
            CslaDataProvider provider = this.Resources["MagSimulationResultListData"] as CslaDataProvider;
            if (provider != null)
            {
                string OrderBy = "Network"; // i.e. rbROCNetwork.IsChecked == true

                if (rbROCDistance.IsChecked == true)
                {
                    OrderBy = "FoS";
                }
                if (rbROCUserClassifier.IsChecked == true)
                {
                    OrderBy = "User";
                }
                if (rbROCStudyClassifier.IsChecked == true)
                {
                    OrderBy = "StudyType";
                }
                if (rbROCEnsemble.IsChecked == true)
                {
                    OrderBy = "Ensemble";
                }
                provider.FactoryParameters.Clear();
                provider.FactoryParameters.Add(CurrentMagSimulationId);
                provider.FactoryParameters.Add(OrderBy);
                provider.FactoryMethod = "GetMagSimulationResultList";
                provider.Refresh();

                GridSimulationResults.Visibility = Visibility.Visible;
                GridCreateSimulations.Visibility = Visibility.Collapsed;
            }
        }

        private void hlCloseGraph_Click(object sender, RoutedEventArgs e)
        {
            GridSimulationResults.Visibility = Visibility.Collapsed;
            GridCreateSimulations.Visibility = Visibility.Visible;
        }

        private void rbROCNetwork_Click(object sender, RoutedEventArgs e)
        {
            GetDataBySpecifiedScore();
        }

        private void hlDownloadData_Click(object sender, RoutedEventArgs e)
        {
            DataPortal<MagDownloadSimulationDataCommand> dp2 = new DataPortal<MagDownloadSimulationDataCommand>();
            MagDownloadSimulationDataCommand mdsdc = new MagDownloadSimulationDataCommand(CurrentMagSimulationId);
            hlDownloadDataFromBrowser.Visibility = Visibility.Collapsed;
            dp2.ExecuteCompleted += (o, e2) =>
            {
                BusyLoadingSimulationData.IsRunning = false;
                hlDownloadData.IsEnabled = true;
                if (e2.Error != null)
                {
                    RadWindow.Alert(e2.Error.Message);
                }
                else
                {
                    MagDownloadSimulationDataCommand mdsdc2 = e2.Object as MagDownloadSimulationDataCommand;
                    if (mdsdc2 != null)
                    {
                        hlDownloadDataFromBrowser.Tag = mdsdc2.Data;
                        RadWindow.Alert("Data downloaded to your browser.\n\rClick the link to download to your computer");
                        hlDownloadDataFromBrowser.Visibility = Visibility.Visible;
                    }
                }
            };
            BusyLoadingSimulationData.IsRunning = true;
            hlDownloadData.IsEnabled = false;
            dp2.BeginExecute(mdsdc);
        }

        private void hlDownloadDataFromBrowser_Click(object sender, RoutedEventArgs e)
        {
            string extension = "tsv";
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.DefaultExt = extension;
            dialog.Filter = String.Format("{1} files (*.{0})|*.{0}|All files (*.*)|*.*", extension, "tsv");
            dialog.FilterIndex = 1;

            if (dialog.ShowDialog() == true)
            {
                StreamWriter writer = new StreamWriter(dialog.OpenFile());
                writer.WriteLine(hlDownloadDataFromBrowser.Tag.ToString());
                writer.Dispose();
                writer.Close();
            }
        }

        private void HyperlinkButton_Click_10(object sender, RoutedEventArgs e)
        {
            MagBrowserImportedItems = false;
            this.ListSimulationFN.Invoke(sender, e);
        }

        private void HyperlinkButton_Click_11(object sender, RoutedEventArgs e)
        {
            MagBrowserImportedItems = false;
            this.ListSimulationTP.Invoke(sender, e);
        }


        // ********************************** ADMIN PAGE ***********************************

        private void HyperlinkButton_Click_6(object sender, RoutedEventArgs e)
        {
            MagReview mr = (sender as HyperlinkButton).DataContext as MagReview;
            if (mr != null)
            {
                RememberThisMagReview = mr;
                RadWindow.Confirm("Are you sure you want to remove OpenAlex access from this review?", this.RemoveMagReview);
            }
        }

        private MagReview RememberThisMagReview;

        private void RemoveMagReview(object sender, WindowClosedEventArgs e)
        {
            var result = e.DialogResult;
            if (result == true)
            {
                CslaDataProvider provider = this.Resources["MagReviewListData"] as CslaDataProvider;
                if (provider != null)
                {
                    MagReviewList mrl = provider.Data as MagReviewList;
                    if (mrl != null)
                    {
                        mrl.Remove(RememberThisMagReview);
                    }
                }
            }
        }

        private void hlAddMagReview_Click(object sender, RoutedEventArgs e)
        {
            int id;
            if (int.TryParse(tbAddMagReview.Text, out id))
            {
                MagReview mr = new MagReview();
                mr.ReviewId = id;
                mr.Name = "adding review...";
                CslaDataProvider provider = this.Resources["MagReviewListData"] as CslaDataProvider;
                if (provider != null)
                {
                    MagReviewList mrl = provider.Data as MagReviewList;
                    if (mrl != null)
                    {
                        mrl.Add(mr);
                        mrl.SaveItem(mr);
                    }
                }
            }
        }

        private void HyperlinkButton_Click_7(object sender, RoutedEventArgs e)
        {
            CslaDataProvider provider = this.Resources["MagReviewListData"] as CslaDataProvider;
            if (provider != null)
            {
                provider.Refresh();
            }
        }

        private string CurrentMag()
        {
            string currentMag = "";
            CslaDataProvider provider = this.Resources["MagCurrentInfoListData"] as CslaDataProvider;
            if (provider != null && provider.Data != null && provider.Data is MagCurrentInfoList)
            {
                foreach (MagCurrentInfo mci in provider.Data as MagCurrentInfoList)
                {
                    if (mci.MakesDeploymentStatus == "LIVE")
                    {
                        currentMag = mci.MagFolder;
                        break;
                    }
                } 
            }
            return currentMag;
        }

        private string PendingMag()
        {
            string pendingMag = "";
            CslaDataProvider provider = this.Resources["MagCurrentInfoListData"] as CslaDataProvider;
            if (provider != null && provider.Data != null && provider.Data is MagCurrentInfoList)
            {
                foreach (MagCurrentInfo mci in provider.Data as MagCurrentInfoList)
                {
                    if (mci.MakesDeploymentStatus == "PENDING")
                    {
                        pendingMag = mci.MagFolder;
                        break;
                    }
                }
            }
            return pendingMag;
        }

        private void CheckChangedPaperIds_Click(object sender, RoutedEventArgs e)
        {
            RadWindow.Confirm("Are you sure?\nPlease check it is not already running first!\nOld: " + CurrentMag() + " new: " + PendingMag(),
                this.DoCheckChangedPaperIds);
        }

        private void DoCheckChangedPaperIds(object sender, WindowClosedEventArgs e)
        {
            var result = e.DialogResult;
            if (result == true)
            {
                DataPortal<MagCheckPaperIdChangesCommand> dp = new DataPortal<MagCheckPaperIdChangesCommand>();
                MagCheckPaperIdChangesCommand magCheck = new MagCheckPaperIdChangesCommand(PendingMag());
                dp.ExecuteCompleted += (o, e2) =>
                {
                    LBCheckChangedPaperIds.IsEnabled = true;
                    if (e2.Error != null)
                    {
                        RadWindow.Alert(e2.Error.Message);
                    }
                    else
                    {
                        RadWindow.Alert("Ok, process running: see log below");
                        SwitchOnAutoRefreshLogList();
                    }
                };
                LBCheckChangedPaperIds.IsEnabled = false;
                dp.BeginExecute(magCheck);
            }
        }

        private void LBRefreshMagLogList_Click(object sender, RoutedEventArgs e)
        {
            CslaDataProvider provider = this.Resources["MagLogListData"] as CslaDataProvider;
            provider.FactoryParameters.Clear();
            provider.FactoryMethod = "GetMagLogList";
            provider.Refresh();
        }

        private void AdminLogTimer_Tick(object sender, EventArgs e)
        {
            LBRefreshMagLogList_Click(sender, new RoutedEventArgs());
        }

        private void SwitchOnAutoRefreshLogList()
        {
            if (this.AdminLogTimer != null && this.AdminLogTimer.IsEnabled)
            {
                this.AdminLogTimer.Stop();
                this.AdminLogTimer.Start();
            }
            else
            {
                if (this.AdminLogTimer != null)
                {
                    this.AdminLogTimer.Start();
                }
            }
            LBSwitchOffAutoRefreshLogList.Visibility = Visibility.Visible;
            LBRefreshMagLogList_Click(null, new RoutedEventArgs());
        }

        private void LBSwitchOffAutoRefreshLogList_Click(object sender, RoutedEventArgs e)
        {
            if (this.AdminLogTimer != null && this.AdminLogTimer.IsEnabled)
            {
                this.AdminLogTimer.Stop();
            }
            LBSwitchOffAutoRefreshLogList.Visibility = Visibility.Collapsed;
            RadWindow.Alert("Ok, auto-refresh is switched off");
        }

        private void LBRunContReviewPipeline_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentMag() == PendingMag())
            {
                RadWindow.Alert("Both OpenAlex versions are the same!");
                return;
            }
            RadWindow.Confirm("Are you sure you want to run the pipeline?!\nOld OpenAlex: " + CurrentMag() + " new OpenAlex: " + PendingMag(), this.checkRunContReviewPipeline);
        }

        private void checkRunContReviewPipeline(object sender, WindowClosedEventArgs e)
        {
            var result = e.DialogResult;
            if (result == true)
            {

                DataPortal<MagCheckContReviewRunningCommand> dp = new DataPortal<MagCheckContReviewRunningCommand>();
                MagCheckContReviewRunningCommand check = new MagCheckContReviewRunningCommand();
                dp.ExecuteCompleted += (o, e2) =>
                {
                    //BusyLoading.IsRunning = false;
                    if (e2.Error != null)
                    {
                        RadWindow.Alert(e2.Error.Message);
                    }
                    else
                    {
                        MagCheckContReviewRunningCommand chk = e2.Object as MagCheckContReviewRunningCommand;
                        if (chk != null)
                        {
                            if (chk.IsRunningMessage == "running")
                            {
                                RadWindow.Alert("Request submitted. n.b. another pipeline is currently running");
                                DoRunContReviewPipeline("", 0, "Pipeline running...");
                            }
                            else
                            {
                                DoRunContReviewPipeline("", 0, "Pipeline running...");
                            }
                        }
                    }
                };
                //BusyLoading.IsRunning = true;
                dp.BeginExecute(check);
            }
        }

        private void DoRunContReviewPipeline(string specificFolder, int MagLogId, string AlertText)
        {
            CslaDataProvider provider = ((CslaDataProvider)App.Current.Resources["MagCurrentInfoData"]);
            MagCurrentInfo mci = provider.Data as MagCurrentInfo;
            DataPortal<MagContReviewPipelineRunCommand> dp2 = new DataPortal<MagContReviewPipelineRunCommand>();
            MagContReviewPipelineRunCommand RunPipelineCommand =
                new MagContReviewPipelineRunCommand(
                    CurrentMag(),
                    PendingMag(),
                    EditScoreThreshold.Value.Value,
                    EditFoSThreshold.Value.Value,
                    specificFolder,
                    MagLogId,
                    Convert.ToInt32(EditReviewSampleSize.Value.Value));
            dp2.ExecuteCompleted += (o, e2) =>
            {
                if (e2.Error != null)
                {
                    RadWindow.Alert(e2.Error.Message);
                }
                else
                {
                    RadWindow.Alert(AlertText);
                    SwitchOnAutoRefreshLogList();
                }
            };
            LBRunContReviewPipeline.IsEnabled = false;
            dp2.BeginExecute(RunPipelineCommand);
        }

        private void LBCopyNewOaData_Click(object sender, RoutedEventArgs e)
        {
            RadWindow.Confirm("Are you sure you want to download new data from OpenAlex for " + tbLatestMag.Text + "?", this.doRunDownloadNewOpenAlexData);
        }

        private void doRunDownloadNewOpenAlexData(object sender, WindowClosedEventArgs e)
        {
            var result = e.DialogResult;
            if (result == true)
            {
                DataPortal<MagContReviewPipelineRunCommand> dp2 = new DataPortal<MagContReviewPipelineRunCommand>();
                MagContReviewPipelineRunCommand RunPipelineCommand =
                    new MagContReviewPipelineRunCommand(
                        "",
                        tbLatestMag.Text,
                        0,
                        0,
                        "",
                        0,
                        0,
                        "CopyOpenAlexDataToAzureBlob");
                dp2.ExecuteCompleted += (o, e2) =>
                {
                    if (e2.Error != null)
                    {
                        RadWindow.Alert(e2.Error.Message);
                    }
                    else
                    {
                        RadWindow.Alert("Ok. Process to download new OpenAlex data is started\nThe MAKES json will then be created, so this process\n can take about 4 hours.");
                        SwitchOnAutoRefreshLogList();
                        LBCopyNewOaData.IsEnabled = false;
                    }
                };
                LBRunContReviewPipeline.IsEnabled = false;
                dp2.BeginExecute(RunPipelineCommand);
            }
        }

        private void LBCreateParquetFiles_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as HyperlinkButton).Tag.ToString() == "Prepare parquet")
            {
                RadWindow.Confirm("Are you sure you want to create parquet files for\n" + PendingMag(), this.doRunCreateParquetFiles);
            }
            else
            {
                RadWindow.Confirm("Are you sure you want to download new PaperIds appearing in\n" + PendingMag() + ", which are not in " + CurrentMag(), this.doRunDownloadNewPaperIds);
            }
        }

        private void doRunCreateParquetFiles(object sender, WindowClosedEventArgs e)
        {
            var result = e.DialogResult;
            if (result == true)
            {
                //CslaDataProvider provider = ((CslaDataProvider)App.Current.Resources["MagCurrentInfoData"]);
                //MagCurrentInfo mci = provider.Data as MagCurrentInfo;
                DataPortal<MagContReviewPipelineRunCommand> dp2 = new DataPortal<MagContReviewPipelineRunCommand>();
                MagContReviewPipelineRunCommand RunPipelineCommand =
                    new MagContReviewPipelineRunCommand(
                        CurrentMag(),
                        PendingMag(),
                        EditScoreThreshold.Value.Value,
                        EditFoSThreshold.Value.Value,
                        "",
                        0,
                        Convert.ToInt32(EditReviewSampleSize.Value.Value),
                        "Prepare parquet");
                dp2.ExecuteCompleted += (o, e2) =>
                {
                    if (e2.Error != null)
                    {
                        RadWindow.Alert(e2.Error.Message);
                    }
                    else
                    {
                        RadWindow.Alert("Ok. Process to generate the parquet files has been started");
                        SwitchOnAutoRefreshLogList();
                    }
                };
                LBRunContReviewPipeline.IsEnabled = false;
                dp2.BeginExecute(RunPipelineCommand);
            }
        }

        private void LBGetMissingAbstracts_Click(object sender, RoutedEventArgs e)
        {
            RadWindow.Confirm("Are you sure you want to hunt for missing abstracts in \n" + CurrentMag(), this.doDownloadMissingAbstracts);
        }

        private void doDownloadMissingAbstracts(object sender, WindowClosedEventArgs e)
        {
            var result = e.DialogResult;
            if (result == true)
            {
                DataPortal<MagNewPapersUpdateAbstractsCommand> dp2 = new DataPortal<MagNewPapersUpdateAbstractsCommand>();
                MagNewPapersUpdateAbstractsCommand updateAbstractsCommand =
                    new MagNewPapersUpdateAbstractsCommand();
                dp2.ExecuteCompleted += (o, e2) =>
                {
                    if (e2.Error != null)
                    {
                        RadWindow.Alert(e2.Error.Message);
                    }
                    else
                    {
                        RadWindow.Alert("Ok. Process to find missing abstracts has started");
                        SwitchOnAutoRefreshLogList();
                    }
                };
                LBGetMissingAbstracts.IsEnabled = false;
                dp2.BeginExecute(updateAbstractsCommand);
            }
        }

        private void doRunDownloadNewPaperIds(object sender, WindowClosedEventArgs e)
        {
            var result = e.DialogResult;
            if (result == true)
            {
                DataPortal<MagContReviewPipelineRunCommand> dp2 = new DataPortal<MagContReviewPipelineRunCommand>();
                MagContReviewPipelineRunCommand RunPipelineCommand =
                    new MagContReviewPipelineRunCommand(
                        CurrentMag(),
                        PendingMag(),
                        EditScoreThreshold.Value.Value,
                        EditFoSThreshold.Value.Value,
                        "",
                        0,
                        Convert.ToInt32(EditReviewSampleSize.Value.Value),
                        "GetNewPaperIds");
                dp2.ExecuteCompleted += (o, e2) =>
                {
                    if (e2.Error != null)
                    {
                        RadWindow.Alert(e2.Error.Message);
                    }
                    else
                    {
                        RadWindow.Alert("Ok. Process to download new PaperIds is started");
                        SwitchOnAutoRefreshLogList();
                    }
                };
                LBRunContReviewPipeline.IsEnabled = false;
                dp2.BeginExecute(RunPipelineCommand);
            }
        }

        private void LBCurrentInfoCreate_Click(object sender, RoutedEventArgs e)
        {
            RadWindow.Confirm("Are you sure you want to create a new record for: " + tbLatestMag.Text, this.doCurrentInfoCreate);
        }

        private void doCurrentInfoCreate(object sender, WindowClosedEventArgs e)
        {
            var result = e.DialogResult;
            if (result == true)
            {
                CslaDataProvider provider = this.Resources["MagCurrentInfoListData"] as CslaDataProvider;
                if (provider != null)
                {
                    MagCurrentInfoList infList = provider.Data as MagCurrentInfoList;
                    if (infList != null)
                    {
                        string[] versionElements = PendingMag().Replace("mag-", "").Split('-');
                        MagCurrentInfo mci = new MagCurrentInfo();
                        mci.MagFolder = tbLatestMag.Text; // versionElements[2] + "/" + versionElements[1] + "/" + versionElements[0];
                        mci.WhenLive = DateTime.Now;
                        mci.MatchingAvailable = true;
                        mci.MakesEndPoint = "http://eppioa" + tbLatestMag.Text.Replace("-", "") + ".westeurope.cloudapp.azure.com";
                        mci.MakesDeploymentStatus = "PENDING";
                        infList.Add(mci);
                        infList.Saved -= InfList_Saved;
                        infList.Saved += InfList_Saved;
                        infList.SaveItem(mci);
                        LBCurrentInfoCreate.Visibility = Visibility.Collapsed;
                    }
                }
            }
        }

        private void HyperlinkButton_Click_13(object sender, RoutedEventArgs e)
        {
            HyperlinkButton hl = sender as HyperlinkButton;
            if (hl == null)
                return;
            MagLog ml = hl.DataContext as MagLog;
            if (ml == null)
                return;

            if (ml.TimeUpdated.AddHours(1) > DateTime.Now)
            {
                RadWindow.Alert("Time since last log update < 2 hours");
                return;
            }
            if (ml.JobStatus.ToLower() == "Complete")
            {
                RadWindow.Alert("Data already downloaded");
                return;
            }
            /*
            if (ml.JobStatus.ToLower() != "running")
            {
                RadWindow.Alert("Job not 'running'! Are you sure?");
                //return;
            }
            */
            CurrentlySelectedMagLogForFailedDataDownload = ml;
            RadWindow.Confirm("Are you sure you want to download these data?", this.DoDownloadContReviewDataOnFail);
        }

        private MagLog CurrentlySelectedMagLogForFailedDataDownload;

        private void DoDownloadContReviewDataOnFail(object sender, WindowClosedEventArgs e)
        {
            var result = e.DialogResult;
            if (result == true && CurrentlySelectedMagLogForFailedDataDownload != null)
            {
                MagLog ml = CurrentlySelectedMagLogForFailedDataDownload;
                if (ml.JobMessage.IndexOf("Folder:") < 0)
                {
                    RadWindow.Alert("Can't find folder id");
                    return;
                }
                string folder = ml.JobMessage.Substring(ml.JobMessage.IndexOf("Folder:")).Replace("Folder:", "");
                DoRunContReviewPipeline(folder, ml.MagLogId, "Downloading data from folder: " + folder);
            }
        }

        private void LBCheckForNewOA_Click(object sender, RoutedEventArgs e)
        {

        }

        // *********************************** MagSearch page ***********************************
        private void ComboMagSearchSelect_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (ComboMagSearchSelect != null)
            {
                MagSearchClearTopics();
                if (ComboMagSearchSelect.SelectedIndex == 2)
                {
                    RowMagSearchTopics.Height = new GridLength(120, GridUnitType.Pixel);
                }
                else
                {
                    RowMagSearchTopics.Height = new GridLength(0);
                }
                if (ComboMagSearchSelect.SelectedIndex == 1)
                {
                    ComboMagSearchDateLimit.SelectedIndex = 0;
                    ComboMagSearchDateLimit.IsEnabled = false;
                    //ComboMagSearchPubTypeLimit.SelectedIndex = 0;
                    //ComboMagSearchPubTypeLimit.IsEnabled = false;
                }
                else
                {
                    ComboMagSearchDateLimit.IsEnabled = true;
                    //ComboMagSearchPubTypeLimit.IsEnabled = true;
                }
            }
        }

        private void TextBoxMagSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ComboMagSearchSelect != null && ComboMagSearchSelect.SelectedIndex != 2)
            {
                return;
            }
            if (CleanText(TextBoxMagSearch.Text).Length > 2)
            {
                if (this.timer2 != null && this.timer2.IsEnabled)
                {
                    this.timer2.Stop();
                    this.timer2.Start();
                }
                else
                {
                    if (this.timer2 != null)
                    {
                        this.timer2.Start();
                    }
                }
            }
            else
            {
                WPMagSearchFindTopics.Children.Clear();
            }
        }

        private void ComboMagSearchDateLimit_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (ComboMagSearchDateLimit != null)
            {
                if (ComboMagSearchDateLimit.SelectedIndex == 0)
                {
                    StackPanelMagSearchDates.Visibility = Visibility.Collapsed;
                }
                else
                {
                    if (ComboMagSearchDateLimit.SelectedIndex > 4)
                    {
                        MagSearchDate1.DateSelectionMode = Telerik.Windows.Controls.Calendar.DateSelectionMode.Year;
                        MagSearchDate2.DateSelectionMode = Telerik.Windows.Controls.Calendar.DateSelectionMode.Year;
                    }
                    else
                    {
                        MagSearchDate1.DateSelectionMode = Telerik.Windows.Controls.Calendar.DateSelectionMode.Day;
                        MagSearchDate2.DateSelectionMode = Telerik.Windows.Controls.Calendar.DateSelectionMode.Day;
                    }
                    if (MagSearchDate1.SelectedDate == null)
                    {
                        MagSearchDate1.SelectedDate = DateTime.Now;
                    }
                    StackPanelMagSearchDates.Visibility = Visibility.Visible;
                    if (ComboMagSearchDateLimit.SelectedIndex == 4 || ComboMagSearchDateLimit.SelectedIndex == 8)
                    {
                        MagSearchDateText1.Visibility = Visibility.Visible;
                        MagSearchDateText2.Visibility = Visibility.Visible;
                        MagSearchDate2.Visibility = Visibility.Visible;
                        if (MagSearchDate1.SelectedDate == null)
                        {
                            MagSearchDate1.SelectedDate = DateTime.Now;
                        }
                        if (MagSearchDate2.SelectedDate == null)
                        {
                            MagSearchDate2.SelectedDate = DateTime.Now;
                        }
                    }
                    else
                    {
                        MagSearchDateText1.Visibility = Visibility.Collapsed;
                        MagSearchDateText2.Visibility = Visibility.Collapsed;
                        MagSearchDate2.Visibility = Visibility.Collapsed;
                    }
                }
            }
        }
        /* No adding date limiters to combining any more
        private void ComboMagSearchDateLimitFilter_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (ComboMagSearchDateLimitFilter != null)
            {
                if (ComboMagSearchDateLimitFilter.SelectedIndex == 0)
                {
                    StackPanelMagSearchDatesFilter.Visibility = Visibility.Collapsed;
                }
                else
                {
                    if (ComboMagSearchDateLimitFilter.SelectedIndex > 4)
                    {
                        MagSearchDate1Filter.DateSelectionMode = Telerik.Windows.Controls.Calendar.DateSelectionMode.Year;
                        MagSearchDate2Filter.DateSelectionMode = Telerik.Windows.Controls.Calendar.DateSelectionMode.Year;
                    }
                    else
                    {
                        MagSearchDate1Filter.DateSelectionMode = Telerik.Windows.Controls.Calendar.DateSelectionMode.Day;
                        MagSearchDate2Filter.DateSelectionMode = Telerik.Windows.Controls.Calendar.DateSelectionMode.Day;
                    }
                    if (MagSearchDate1Filter.SelectedDate == null)
                    {
                        MagSearchDate1Filter.SelectedDate = DateTime.Now;
                    }
                    StackPanelMagSearchDatesFilter.Visibility = Visibility.Visible;
                    if (ComboMagSearchDateLimitFilter.SelectedIndex == 4 || ComboMagSearchDateLimit.SelectedIndex == 8)
                    {
                        MagSearchDateText1Filter.Visibility = Visibility.Visible;
                        MagSearchDateText2Filter.Visibility = Visibility.Visible;
                        MagSearchDate2Filter.Visibility = Visibility.Visible;
                        if (MagSearchDate1Filter.SelectedDate == null)
                        {
                            MagSearchDate1Filter.SelectedDate = DateTime.Now;
                        }
                        if (MagSearchDate2Filter.SelectedDate == null)
                        {
                            MagSearchDate2Filter.SelectedDate = DateTime.Now;
                        }
                    }
                    else
                    {
                        MagSearchDateText1Filter.Visibility = Visibility.Collapsed;
                        MagSearchDateText2Filter.Visibility = Visibility.Collapsed;
                        MagSearchDate2Filter.Visibility = Visibility.Collapsed;
                    }
                }
            }
        }
        */
        private void HyperLinkMagSearchDoSearch_Click(object sender, RoutedEventArgs e)
        {
            if (HyperLinkMagSearchDoSearch.IsEnabled == false)
            {
                RadWindow.Alert("A search is already running");
                return;
            }
            if (HyperLinkMagSearchDoSearch.IsEnabled == false)
            {
                return; // for some reason disabled hyperlinks still work??
            }
            if (ComboMagSearchSelect.SelectedIndex != 2 && TextBoxMagSearch.Text == "")
            {
                RadWindow.Alert("Search text is blank");
                return;
            }
            if (ComboMagSearchSelect.SelectedIndex == 2 && MagSearchCurrentTopic.Tag.ToString() == "")
            {
                RadWindow.Alert("Please select a topic");
                return;
            }

            MagSearch newSearch = new MagSearch();
            switch (ComboMagSearchSelect.SelectedIndex)
            {
                case 0:
                    newSearch.MagSearchText = TextBoxMagSearch.Text; // newSearch.GetSearchTextTitle(TextBoxMagSearch.Text);
                    newSearch.SearchText = "¬Title: " + TextBoxMagSearch.Text; // + TextBoxMagSearch.Text;
                    break;
                case 1:
                    newSearch.MagSearchText = TextBoxMagSearch.Text; // newSearch.GetSearchTextAbstract(TextBoxMagSearch.Text);
                    newSearch.SearchText = "¬Title and abstract: " + TextBoxMagSearch.Text; // + TextBoxMagSearch.Text;
                    break;
                //case 2:
                //    newSearch.MagSearchText = TextBoxMagSearch.Text; // newSearch.GetSearchTextAuthors(TextBoxMagSearch.Text);
                //    newSearch.SearchText = "Authors"; // + TextBoxMagSearch.Text;
                //    break;
                case 2:
                    newSearch.MagSearchText = MagSearchCurrentTopic.Tag.ToString(); // newSearch.GetSearchTextFieldOfStudy(MagSearchCurrentTopic.Tag.ToString());
                    newSearch.SearchText = "¬Topic: " + MagSearchCurrentTopic.Text; // + MagSearchCurrentTopic.Text;
                    break;
                case 3:
                    newSearch.MagSearchText = newSearch.GetSearchTextMagIds(TextBoxMagSearch.Text);
                    if (newSearch.MagSearchText.Contains("Error"))
                    {
                        RadWindow.Alert(newSearch.MagSearchText);
                        return;
                    }
                    newSearch.SearchText = "¬OpenAlex ID(s): " + TextBoxMagSearch.Text;
                    break;
                case 4:
                    //newSearch.MagSearchText = TextBoxMagSearch.Text; // newSearch.GetSearchTextJournals(TextBoxMagSearch.Text);
                    //newSearch.SearchText = "Journal"; // + TextBoxMagSearch.Text;
                    break;
                default:
                    RadWindow.Alert("No search specified");
                    return;
            }
            if (ComboMagSearchDateLimit.SelectedIndex > 0)
            {
                switch (ComboMagSearchDateLimit.SelectedIndex)
                {
                    case 1:
                        newSearch.Date1 = MagSearchDate1.SelectedDate.Value.ToString("yyyy-MM-dd");
                        newSearch.DateFilter = "Published on";
                        break;
                    case 2:
                        newSearch.Date1 = MagSearchDate1.SelectedDate.Value.ToString("yyyy-MM-dd");
                        newSearch.DateFilter = "Published before";
                        break;
                    case 3:
                        newSearch.Date1 = MagSearchDate1.SelectedDate.Value.ToString("yyyy-MM-dd");
                        newSearch.DateFilter = "Published after";
                        break;
                    case 4:
                        newSearch.Date1 = MagSearchDate1.SelectedDate.Value.ToString("yyyy-MM-dd");
                        newSearch.Date2 = MagSearchDate2.SelectedDate.Value.ToString("yyyy-MM-dd");
                        newSearch.DateFilter = "Published between";
                        break;
                    case 5:
                        newSearch.Date1 = MagSearchDate1.SelectedDate.Value.Year.ToString();
                        newSearch.DateFilter = "Publication year";
                        break;
                        /* I don't think we need these
                    case 6:
                        newSearch.Date1 = MagSearchDate1.SelectedDate.Value.Year.ToString("yyyy");
                        newSearch.DateFilter = "Publication year before";
                        break;
                        
                    case 7:
                        newSearch.MagSearchText = "AND(" + newSearch.MagSearchText + "," +
                            newSearch.GetSearchTextYearAfter(MagSearchDate1.SelectedDate.Value.Year.ToString()) + ")";
                        newSearch.SearchText += " AND year of publication after: " + MagSearchDate1.SelectedDate.Value.Year.ToString();
                        break;
                    case 8:
                        newSearch.MagSearchText = "AND(" + newSearch.MagSearchText + "," +
                            newSearch.GetSearchTextYearBetween(MagSearchDate1.SelectedDate.Value.Year.ToString(),
                            MagSearchDate2.SelectedDate.Value.Year.ToString()) + ")";
                        newSearch.SearchText += " AND year of publication between: " + MagSearchDate1.SelectedDate.Value.Year.ToString() + " and " +
                            MagSearchDate2.SelectedDate.Value.Year.ToString();
                        break;
                        */
                }
            }
            else
            {
                newSearch.DateFilter = "";
            }
            if (ComboMagSearchPubTypeLimit != null && ComboMagSearchPubTypeLimit.SelectedIndex > 0)
            {
                newSearch.PublicationTypeFilter = newSearch.GetSearchTextPublicationType((ComboMagSearchPubTypeLimit.SelectedIndex - 1).ToString());
                newSearch.PublicationTypeTextFilter = newSearch.GetPublicationType(ComboMagSearchPubTypeLimit.SelectedIndex - 1);
            }
            if (newSearch.MagSearchText.Length > 2000)
            {
                RadWindow.Alert("Sorry, search string is too long");
                return;
            }
            newSearch.Saved += NewSearch_Saved;
            BusyRunningMagSearch.IsRunning = true;
            HyperLinkMagSearchDoSearch.IsEnabled = false;
            SearchDataGrid.IsEnabled = false;
            newSearch.BeginSave();
        }

        private void NewSearch_Saved(object sender, Csla.Core.SavedEventArgs e)
        {
            CslaDataProvider provider = this.Resources["MagSearchListData"] as CslaDataProvider;
            provider.FactoryParameters.Clear();
            MagPaperListSelectionCriteria selectionCriteria = new MagPaperListSelectionCriteria();
            provider.FactoryMethod = "GetMagSearchList";
            provider.Refresh();
            BusyRunningMagSearch.IsRunning = false;
            HyperLinkMagSearchDoSearch.IsEnabled = true;
            SearchDataGrid.IsEnabled = true;
            MagSearchComboCombine.IsEnabled = true;
            HyperLinkMagSearchDoSearch.IsEnabled = true;
        }

        private void HyperlinkButton_Click_15(object sender, RoutedEventArgs e)
        {
            HyperlinkButton btn = sender as HyperlinkButton;
            if (btn != null)
            {
                MagSearch search = btn.DataContext as MagSearch;
                if (search != null)
                {
                    MagSearch newSearch = new MagSearch();
                    newSearch.SetToRerun(search);
                    newSearch.Saved += NewSearch_Saved;
                    SearchDataGrid.IsEnabled = false;
                    newSearch.BeginSave();
                }
            }
        }

        private void MagSearchComboCombine_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            const int MaxHitCount = 40000; // We could put this in web.config
            if (MagSearchComboCombine.SelectedIndex == -1)
            {
                return;
            }
            if (SearchDataGrid.SelectedItems.Count < 2)
            {
                Dispatcher.BeginInvoke(() => RadWindow.Alert("Please select at least two searches to combine"));
                MagSearchComboCombine.SelectedIndex = -1;
                return;
            }
            List<MagSearch> searches = new List<MagSearch>();
            int hitCount = 0;
            string combined = "";
            string searchDesc = "";
            foreach (MagSearch ms in SearchDataGrid.SelectedItems)
            {
                searches.Add(ms);
                if (!ms.SearchIdsStored)
                {
                    hitCount += ms.HitsNo;
                }
                if (combined == "")
                {
                    combined = ms.MagSearchId.ToString();
                    searchDesc = ms.SearchNo.ToString();
                }
                else
                {
                    combined += (MagSearchComboCombine.SelectedIndex == 0 ? "AND" : "OR") + ms.MagSearchId.ToString();
                    searchDesc += (MagSearchComboCombine.SelectedIndex == 0 ? "AND" : "OR") + ms.SearchNo.ToString();
                }
                
            }
            if (hitCount > MaxHitCount)
            {
                RadWindow.Alert("Sorry, too many hits. Please combine fewer records");
                MagSearchComboCombine.SelectedIndex = -1;
                return;
            }
            MagSearch newSearch = new MagSearch();
            //newSearch.SetCombinedSearches(searches, MagSearchComboCombine.SelectedIndex == 0 ? "AND" : "OR");
            newSearch.MagSearchText = combined;
            newSearch.SearchText = "¬COMBINE SEARCHES" + searchDesc;
            //newSearch = AddDateFilter(newSearch);
            //if (newSearch.MagSearchText.Length > 2000)
            //{
            //    Dispatcher.BeginInvoke(() => RadWindow.Alert("Sorry, this search string is too long"));
            //    MagSearchComboCombine.SelectedIndex = -1;
            //    return;
            //}
            newSearch.Saved += NewSearch_Saved;
            newSearch.BeginSave();
            MagSearchComboCombine.SelectedIndex = -1;
            MagSearchComboCombine.IsEnabled = false;
            SearchDataGrid.IsEnabled = false;
            HyperLinkMagSearchDoSearch.IsEnabled = false;
            RadWindow.Alert("Combining searches now\nThis can take a while...");
        }

        private void hlMagSearchDateLimitFilter2_Click(object sender, RoutedEventArgs e)
        {
            if (SearchDataGrid.SelectedItems.Count > 1)
            {
                RadWindow.Alert("Please combine more than one search with a date filter using AND or OR");
                return;
            }
            MagSearch originalSearch = SearchDataGrid.SelectedItem as MagSearch;
            if (originalSearch != null)
            {
                MagSearch newSearch = new MagSearch();
                newSearch.MagSearchText = originalSearch.MagSearchText;
                newSearch.SearchText = "#" + originalSearch.SearchNo.ToString();
                //newSearch = AddDateFilter(newSearch);
                newSearch.Saved += NewSearch_Saved;
                newSearch.BeginSave();
                MagSearchComboCombine.SelectedIndex = -1;
                MagSearchComboCombine.IsEnabled = false;
                SearchDataGrid.IsEnabled = false;
            }
        }

        /*
        private MagSearch AddDateFilter(MagSearch newSearch)
        {
            if (ComboMagSearchDateLimitFilter.SelectedIndex > 0)
            {
                switch (ComboMagSearchDateLimitFilter.SelectedIndex)
                {
                    case 1:
                        newSearch.MagSearchText = "AND(" + newSearch.MagSearchText + "," +
                            newSearch.GetSearchTextPubDateExactly(MagSearchDate1Filter.SelectedDate.Value.ToString("yyyy-MM-dd")) + ")";
                        newSearch.SearchText += " AND published on: " + MagSearchDate1Filter.SelectedDate.Value.ToString("yyyy-MM-dd");
                        break;
                    case 2:
                        newSearch.MagSearchText = "AND(" + newSearch.MagSearchText + "," +
                            newSearch.GetSearchTextPubDateBefore(MagSearchDate1Filter.SelectedDate.Value.ToString("yyyy-MM-dd")) + ")";
                        newSearch.SearchText += " AND published before: " + MagSearchDate1Filter.SelectedDate.Value.ToString("yyyy-MM-dd");
                        break;
                    case 3:
                        newSearch.MagSearchText = "AND(" + newSearch.MagSearchText + "," +
                            newSearch.GetSearchTextPubDateFrom(MagSearchDate1Filter.SelectedDate.Value.ToString("yyyy-MM-dd")) + ")";
                        newSearch.SearchText += " AND published after: " + MagSearchDate1Filter.SelectedDate.Value.ToString("yyyy-MM-dd");
                        break;
                    case 4:
                        newSearch.MagSearchText = "AND(" + newSearch.MagSearchText + "," +
                            newSearch.GetSearchTextPubDateBetween(MagSearchDate1Filter.SelectedDate.Value.ToString("yyyy-MM-dd"),
                            MagSearchDate2Filter.SelectedDate.Value.ToString("yyyy-MM-dd")) + ")";
                        newSearch.SearchText += " AND published between: " + MagSearchDate1Filter.SelectedDate.Value.ToString("yyyy-MM-dd") + " and " +
                            MagSearchDate2Filter.SelectedDate.Value.ToString("yyyy-MM-dd");
                        break;
                    case 5:
                        newSearch.MagSearchText = "AND(" + newSearch.MagSearchText + "," +
                            newSearch.GetSearchTextYearExactly(MagSearchDate1Filter.SelectedDate.Value.Year.ToString()) + ")";
                        newSearch.SearchText += " AND year of publication: " + MagSearchDate1Filter.SelectedDate.Value.Year.ToString();
                        break;
                    case 6:
                        newSearch.MagSearchText = "AND(" + newSearch.MagSearchText + "," +
                            newSearch.GetSearchTextYearBefore(MagSearchDate1Filter.SelectedDate.Value.Year.ToString()) + ")";
                        newSearch.SearchText += " AND year of publication before: " + MagSearchDate1Filter.SelectedDate.Value.Year.ToString();
                        break;
                    case 7:
                        newSearch.MagSearchText = "AND(" + newSearch.MagSearchText + "," +
                            newSearch.GetSearchTextYearAfter(MagSearchDate1Filter.SelectedDate.Value.Year.ToString()) + ")";
                        newSearch.SearchText += " AND year of publication after: " + MagSearchDate1Filter.SelectedDate.Value.Year.ToString();
                        break;
                    case 8:
                        newSearch.MagSearchText = "AND(" + newSearch.MagSearchText + "," +
                            newSearch.GetSearchTextYearBetween(MagSearchDate1Filter.SelectedDate.Value.Year.ToString(),
                            MagSearchDate2Filter.SelectedDate.Value.Year.ToString()) + ")";
                        newSearch.SearchText += " AND year of publication between: " + MagSearchDate1Filter.SelectedDate.Value.Year.ToString() + " and " +
                            MagSearchDate2Filter.SelectedDate.Value.Year.ToString();
                        break;
                }
            }
            return newSearch;
        }
        */

        private MagSearch TempDeleteMagSearch;
        private void cmdDeleteSearch_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn != null)
            {
                MagSearch s = btn.DataContext as MagSearch;
                if (s != null)
                {
                    TempDeleteMagSearch = s;
                    RadWindow.Confirm("Are you sure you want to delete the selected search?", this.doDeleteSingleSearch);
                }
            }
        }
        private void doDeleteSingleSearch(object sender, WindowClosedEventArgs e)
        {
            var result = e.DialogResult;
            if (result == true)
            {
                CslaDataProvider provider = this.Resources["MagSearchListData"] as CslaDataProvider;
                MagSearchList searchList = provider.Data as MagSearchList;
                if (TempDeleteMagSearch != null)
                {
                    searchList.Remove(TempDeleteMagSearch);
                }
            }
        }

        private void hlDeleteSelectedMagSearches_Click(object sender, RoutedEventArgs e)
        {
            if (SearchDataGrid.SelectedItems.Count < 1)
            {
                RadWindow.Alert("You need to select at least one search");
                return;
            }
            RadWindow.Confirm("Are you sure you want to delete the selected search(es)?", this.doDeleteSearch);
        }

        private void doDeleteSearch(object sender, WindowClosedEventArgs e)
        {
            var result = e.DialogResult;
            if (result == true)
            {
                CslaDataProvider provider = this.Resources["MagSearchListData"] as CslaDataProvider;
                MagSearchList searchList = provider.Data as MagSearchList;
                if (searchList != null)
                {
                    foreach (MagSearch ms in SearchDataGrid.SelectedItems)
                    {
                        searchList.Remove(ms);
                    }
                }
            }
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            var checkBox = (CheckBox)sender;
            var parentGrid = checkBox.ParentOfType<GridViewDataControl>();

            if (checkBox.IsChecked.Value)
                parentGrid.SelectAll();
            else
                parentGrid.UnselectAll();
        }

        private void HyperlinkButton_Click_14(object sender, RoutedEventArgs e)
        {
            HyperlinkButton btn = sender as HyperlinkButton;
            if (btn != null)
            {
                MagSearch search = btn.DataContext as MagSearch;

                if (search.HitsNo == 0)
                {
                    RadWindow.Alert("No results");
                    return;
                }

                /*
                CslaDataProvider prov = ((CslaDataProvider)App.Current.Resources["MagCurrentInfoData"]);
                MagCurrentInfo mci = prov.Data as MagCurrentInfo;
                if (mci != null)
                {
                    if (search.MagFolder != mci.MagFolder)
                    {
                        RadWindow.Alert("This search was run against a prevous version of OpenAlex\nPlease re-run before listing results.");
                        return;
                    }
                }
                */

                if (search != null)
                {
                    TBPaperListTitle.Text = search.HitsNo.ToString() + " hits (in original search)";
                    ShowSearchResults(search.MagSearchId.ToString());
                }
            }
        }

        private void HyperlinkButton_Click_16(object sender, RoutedEventArgs e)
        {
            if (BusyImportingRecords.IsRunning == true)
            {
                RadWindow.Alert("Importing currently in progress");
                return;
            }
            HyperlinkButton hlb = sender as HyperlinkButton;
            if (hlb != null)
            {
                MagSearch ms = hlb.DataContext as MagSearch;
                if (ms != null)
                {
                    CslaDataProvider prov = ((CslaDataProvider)App.Current.Resources["MagCurrentInfoData"]);
                    MagCurrentInfo mci = prov.Data as MagCurrentInfo;
                    /*if (mci != null)
                    {
                        
                        if (ms.MagFolder != mci.MagFolder)
                        {
                            RadWindow.Alert("This search was run against an earlier version of OpenAlex\nPlease re-run before importing");
                            return;
                        }
                        

                    }*/
                    if (ms.HitsNo == 0)
                    {
                        RadWindow.Alert("No hits to import");
                        return;
                    }
                    if (ms.HitsNo > 20000)
                    {
                        RadWindow.Alert("Sorry. You can't import more than 20k records at a time.\nYou could try breaking up your search e.g. by date?");
                    }
                    else
                    {
                        SelectedLinkButton = hlb;
                        RadWindow.Confirm("Are you sure you want to import this search result?", this.doImportMagSearchResults);
                    }
                }
            }
        }

        private void doImportMagSearchResults(object sender, WindowClosedEventArgs e)
        {
            var result = e.DialogResult;
            if (result == true)
            {
                MagSearch ms = SelectedLinkButton.DataContext as MagSearch;
                if (ms != null)
                {
                    DataPortal<MagItemPaperInsertCommand> dp2 = new DataPortal<MagItemPaperInsertCommand>();
                    MagItemPaperInsertCommand command = new MagItemPaperInsertCommand("",
                        SelectedLinkButton.Tag.ToString(),
                        0, 0, "", 0, 0, 0, 0,
                        (cbMagSearchShowTextFilters.IsChecked == true ? MagSearchTextFilterJournal.Text : ""),
                        (cbMagSearchShowTextFilters.IsChecked == true ? MagSearchTextFilterDOI.Text : ""),
                        (cbMagSearchShowTextFilters.IsChecked == true ? MagSearchTextFilterURL.Text : ""),
                        (cbMagSearchShowTextFilters.IsChecked == true ? MagSearchTextFilterTitle.Text : ""),
                        ms.MagSearchId.ToString(),
                        "OpenAlex search: " + ms.SearchText + 
                            (SelectedLinkButton.Tag.ToString() == "MagSearchResultsLatestMAG" ? " (filtered to latest deployment)" : "") +
                            (cbMagSearchShowTextFilters.IsChecked == true ? " (with source filters applied)" : ""),
                        "");
                    dp2.ExecuteCompleted += (o, e2) =>
                    {
                        BusyImportingRecords.IsRunning = false;
                        tbImportingRecords.Visibility = Visibility.Collapsed;
                        if (e2.Error != null)
                        {
                            RadWindow.Alert(e2.Error.Message);
                        }
                        else
                        {
                            int num_in_run = ms.HitsNo;
                            MagBrowserImportedItems = true;
                            if (e2.Object.NImported == num_in_run)
                            {
                                RadWindow.Alert("Imported " + e2.Object.NImported.ToString() + " out of " +
                                    num_in_run.ToString() + " items");
                            }
                            else
                            {
                                string filteredText = "";
                                if (cbMagSearchShowTextFilters.IsChecked == true)
                                    filteredText = " / were removed by filters";
                                string latestMagFilter = "";
                                if (SelectedLinkButton.Tag.ToString() != "MagSearchResults")
                                    latestMagFilter = " / were not in latest OpenAlex";

                                if (e2.Object.NImported == 0)
                                {
                                    RadWindow.Alert("All records were already in your review" + filteredText + latestMagFilter);
                                }
                                else
                                {
                                    RadWindow.Alert("Imported " + e2.Object.NImported.ToString() + " out of " +
                                        num_in_run.ToString() +" new items.\n\nSome were already in your review." + filteredText + latestMagFilter);
                                }
                            }
                        }
                    };
                    BusyImportingRecords.IsRunning = true;
                    tbImportingRecords.Visibility = Visibility.Visible;
                    SelectedLinkButton.IsEnabled = false;
                    dp2.BeginExecute(command);
                }
            }
        }

        


        private void cbMagSearchShowTextFilters_Checked(object sender, RoutedEventArgs e)
        {
            if (cbMagSearchShowTextFilters.IsChecked == true)
            {
                RowMagSearchFilterJournal.Height = new GridLength(35, GridUnitType.Auto);
                RowMagSearchFilterUrl.Height = new GridLength(35, GridUnitType.Auto);
                RowMagSearchFilterDoi.Height = new GridLength(35, GridUnitType.Auto);
                RowMagSearchFilterTitle.Height = new GridLength(35, GridUnitType.Auto);
            }
        }

        private void cbMagSearchShowTextFilters_Unchecked(object sender, RoutedEventArgs e)
        {
            if (cbMagSearchShowTextFilters.IsChecked == false)
            {
                RowMagSearchFilterJournal.Height = new GridLength(0);
                RowMagSearchFilterUrl.Height = new GridLength(0);
                RowMagSearchFilterDoi.Height = new GridLength(0);
                RowMagSearchFilterTitle.Height = new GridLength(0);
            }
        }

        private void Timer2_Tick(object sender, EventArgs e)
        {
            this.timer2.Stop();
            if (TextBoxMagSearch.Text.Length > 1)
            {
                CslaDataProvider provider = this.Resources["SearchTopicsData"] as CslaDataProvider;
                if (provider != null)
                {
                    MagFieldOfStudyListSelectionCriteria selectionCriteria = new MagFieldOfStudyListSelectionCriteria();
                    selectionCriteria.ListType = "FieldOfStudySearchList";
                    selectionCriteria.SearchText = TextBoxMagSearch.Text;
                    DataPortal<MagFieldOfStudyList> dp = new DataPortal<MagFieldOfStudyList>();
                    MagFieldOfStudyList mfsl = new MagFieldOfStudyList();
                    dp.FetchCompleted += (o, e2) =>
                    {
                        WPMagSearchFindTopics.Children.Clear();
                        MagFieldOfStudyList FosList = e2.Object as MagFieldOfStudyList;
                        double i = 15;
                        foreach (MagFieldOfStudy fos in FosList)
                        {
                            HyperlinkButton newHl = new HyperlinkButton();
                            newHl.Content = fos.DisplayName;
                            newHl.Tag = fos.FieldOfStudyId.ToString();
                            newHl.FontSize = i;
                            newHl.IsTabStop = false;
                            newHl.Click += MagSearchSelectTopic_Click;
                            newHl.Margin = new Thickness(5, 5, 5, 5);
                            WPMagSearchFindTopics.Children.Add(newHl);
                            if (i > 10)
                            {
                                i -= 0.5;
                            }
                        }
                    };
                    dp.BeginFetch(selectionCriteria);
                }
            }
            else
            {
                MagSearchClearTopics();
            }
        }

        private void MagSearchClearTopics()
        {
            MagSearchCurrentTopic.Text = "";
            MagSearchCurrentTopic.Tag = "";
            WPMagSearchFindTopics.Children.Clear();
            TextBlock tb = new TextBlock();
            tb.Text = "Search for topics in the box above.";
            tb.Margin = new Thickness(5, 5, 5, 5);
            WPMagSearchFindTopics.Children.Add(tb);
        }

        private void MagSearchSelectTopic_Click(object sender, RoutedEventArgs e)
        {
            HyperlinkButton hl = sender as HyperlinkButton;
            if (hl != null)
            {
                MagSearchCurrentTopic.Text = hl.Content.ToString();
                MagSearchCurrentTopic.Tag = hl.Tag;
            }
        }

        /* ************************* Auto update page / tab *********************************************************** */

        private void RadioButtonAutoUpdateAllIncluded_Checked(object sender, RoutedEventArgs e)
        {
            if (RowCreateNewAutoUpdate != null)
            {
                //RowSelectCodeRelatedPapersRun.Height = new GridLength(0);
                codesSelectControlAutoUpdate.Visibility = Visibility.Collapsed;
            }
        }

        private void RadioButtonAutoUpdateWithCode_Checked(object sender, RoutedEventArgs e)
        {
            if (RowCreateNewAutoUpdate != null)
            {
                //RowSelectCodeRelatedPapersRun.Height = new GridLength(0);
                codesSelectControlAutoUpdate.Visibility = Visibility.Visible;
            }
        }

        private void LBAddNewAutoUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (nMatchedRecords == 0)
            {
                RadWindow.Alert("Please match records before subscribing to an auto-update");
                return;
            }

            if ((sender as HyperlinkButton).Tag.ToString() == "ClickToOpen")
            {
                RowCreateNewAutoUpdate.Height = new GridLength(50, GridUnitType.Auto);
                LBAddNewAutoUpdate.Content = "Adding new auto update (Click to close)";
                LBAddNewAutoUpdate.Tag = "ClickToClose";
                tbAutoUpdateDescription.Text = "";
            }
            else
            {
                RowCreateNewAutoUpdate.Height = new GridLength(0);
                LBAddNewAutoUpdate.Content = "Add new review auto update";
                LBAddNewAutoUpdate.Tag = "ClickToOpen";
            }
        }

        private void LBAddNewAutoUpdateDoAdd_Click(object sender, RoutedEventArgs e)
        {
            if (tbAutoUpdateDescription.Text == "")
            {
                RadWindow.Alert("Please enter a description");
                return;
            }


            if (RadioButtonAutoUpdateWithCode.IsChecked == true &&
                   codesSelectControlAutoUpdate.SelectedAttributeSet() == null)
            {
                RadWindow.Alert("Please select a code to filter by");
                return;
            }
            RadWindow.Confirm("Are you sure you want to create this auto-update?", this.DoAddAutoUpdate);
        }

        private void DoAddAutoUpdate(object sender, WindowClosedEventArgs e)
        {
            var result = e.DialogResult;
            if (result == true)
            {
                CslaDataProvider provider = this.Resources["MagAutoUpdateListData"] as CslaDataProvider;
                if (provider != null)
                {
                    MagAutoUpdateList maol = provider.Data as MagAutoUpdateList;
                    if (maol != null)
                    {
                        MagAutoUpdate mao = new MagAutoUpdate();
                        mao.UserDescription = tbAutoUpdateDescription.Text;
                        if (RadioButtonAutoUpdateAllIncluded.IsChecked == true)
                        {
                            mao.AllIncluded = true;
                        }
                        else
                        {
                            mao.AllIncluded = false;
                            mao.AttributeId = codesSelectControlAutoUpdate.SelectedAttributeSet().AttributeId;
                            mao.AttributeName = codesSelectControlAutoUpdate.SelectedAttributeSet().AttributeName;
                        }
                        /*
                        if (comboAutoUpdateStudyTypeClassifier.SelectedIndex != -1)
                        {
                            mao.StudyTypeClassifier = (comboAutoUpdateStudyTypeClassifier.SelectedItem as ComboBoxItem).Content.ToString();
                        }
                        
                        if (comboAutoUpdateUserModels.SelectedItem != null)
                        {
                            ClassifierContactModel ccm = comboAutoUpdateUserModels.SelectedItem as ClassifierContactModel;
                            if (ccm != null)
                            {
                                mao.UserClassifierModelId = ccm.ModelId;
                                mao.UserClassifierModelReviewId = ccm.ReviewId;
                            }
                        }
                        */
                        maol.Add(mao);
                        maol.SaveItem(mao);

                        RowCreateNewAutoUpdate.Height = new GridLength(0);
                        LBAddNewAutoUpdate.Content = "Add new review auto update";
                        LBAddNewAutoUpdate.Tag = "ClickToOpen";
                    }
                }
            }
        }

        private void HyperlinkButton_Click_17(object sender, RoutedEventArgs e)
        {
            HyperlinkButton hlb = sender as HyperlinkButton;
            if (hlb != null)
            {
                MagAutoUpdate mao = hlb.DataContext as MagAutoUpdate;
                if (mao != null)
                {

                    RememberThisMagAutoUpdate = mao;
                    RadWindow.Confirm("Are you sure you want to delete this row?", this.doDeleteMagAutoUpdate);
                }
            }
        }

        MagAutoUpdate RememberThisMagAutoUpdate; // temporary variable to store a specific row while a dialog is showing

        private void doDeleteMagAutoUpdate(object sender, WindowClosedEventArgs e)
        {
            var result = e.DialogResult;
            if (result == true)
            {
                CslaDataProvider provider = this.Resources["MagAutoUpdateListData"] as CslaDataProvider;
                if (provider != null)
                {
                    MagAutoUpdateList autoUpdateList = provider.Data as MagAutoUpdateList;
                    if (autoUpdateList != null)
                    {
                        autoUpdateList.Remove(RememberThisMagAutoUpdate);
                    }
                }
            }
        }

        private void HyperlinkButton_Click_18(object sender, RoutedEventArgs e)
        {
            HyperlinkButton hlb = sender as HyperlinkButton;
            if (hlb != null)
            {
                MagAutoUpdateRun maur = hlb.DataContext as MagAutoUpdateRun;
                if (maur != null)
                {
                    if (maur.NPapers < 1)
                    {
                        RadWindow.Alert("Nothing to display");
                    }
                    else
                    {
                        RowAutoUpdateImport.Height = new GridLength(400, GridUnitType.Auto);
                        GridAutoUpdateImport.DataContext = maur;
                        AutoUpdateImportTopN.Value = maur.NPapers;
                        AutoUpdateImportTopN.Maximum = maur.NPapers;
                        RefreshAutoUpdateGraph();
                        AutoUpdateImportCount();
                        comboAutoUpdateUserModels.SelectedIndex = -1;
                        comboAutoUpdateStudyTypeClassifier.SelectedIndex = -1;
                        if (maur.StudyTypeClassifier == "")
                        {
                            AutoUpdateStudyTypeScoreThreshold.Value = 0;
                            AutoUpdateStudyTypeScoreThreshold.IsEnabled = false;
                            AutoUpdateGraphShowStudyTypeModel.IsEnabled = false;
                        }
                        else
                        {
                            AutoUpdateStudyTypeScoreThreshold.IsEnabled = true;
                            AutoUpdateGraphShowStudyTypeModel.IsEnabled = true;
                            foreach (ComboBoxItem cbi in comboAutoUpdateStudyTypeClassifier.Items)
                            {
                                if (cbi.Tag.ToString() == maur.StudyTypeClassifier)
                                    comboAutoUpdateStudyTypeClassifier.SelectedItem = cbi;
                            }
                        }
                        if (maur.UserClassifierModelId == 0)
                        {
                            AutoUpdateUserScoreThreshold.Value = 0;
                            AutoUpdateUserScoreThreshold.IsEnabled = false;
                            AutoUpdateGraphShowUserModel.IsEnabled = false;
                        }
                        else
                        {
                            AutoUpdateUserScoreThreshold.IsEnabled = true;
                            AutoUpdateGraphShowUserModel.IsEnabled = true;
                            CslaDataProvider provider = this.Resources["ClassifierContactModelListData"] as CslaDataProvider;
                            if (provider != null)
                            {
                                ClassifierContactModelList ccml = provider.Data as ClassifierContactModelList;
                                if (ccml != null)
                                {
                                    foreach (ClassifierContactModel ccm in ccml)
                                    {
                                        if (ccm.ModelId == maur.UserClassifierModelId)
                                            comboAutoUpdateUserModels.SelectedItem = ccm;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void RefreshAutoUpdateGraph()
        {
            if (GridAutoUpdateImport != null && GridAutoUpdateImport.DataContext != null)
            {
                MagAutoUpdateRun maur = GridAutoUpdateImport.DataContext as MagAutoUpdateRun;
                if (maur != null)
                {
                    string field = "";
                    if (AutoUpdateGraphShowAutoUpdateModel.IsChecked == true)
                    {
                        field = "AutoUpdate";
                    }
                    else if (AutoUpdateGraphShowStudyTypeModel.IsChecked == true)
                    {
                        field = "StudyType";
                    }
                    else if (AutoUpdateGraphShowUserModel.IsChecked == true)
                    {
                        field = "User";
                    }
                    CslaDataProvider provider = App.Current.Resources["MagAutoUpdateVisualiseData"] as CslaDataProvider;
                    provider.FactoryParameters.Clear();
                    MagAutoUpdateVisualiseSelectionCriteria selectionCriteria = new MagAutoUpdateVisualiseSelectionCriteria();
                    selectionCriteria.MagAutoUpdateRunId = maur.MagAutoUpdateRunId;
                    selectionCriteria.Field = field;
                    provider.FactoryParameters.Add(selectionCriteria);
                    provider.FactoryMethod = "GetMagAutoUpdateVisualiseList";
                    provider.Refresh();
                }
            }


        }

        private void AutoUpdateGraphShowAutoUpdateModel_Checked(object sender, RoutedEventArgs e)
        {
            RefreshAutoUpdateGraph();
        }

        private void AutoUpdateOpenClassifiers_Click(object sender, RoutedEventArgs e)
        {
            if (AutoUpdateOpenClassifiers.Tag.ToString() == "Open")
            {
                RowAutoUpdateRunStudyClassifiers.Height = new GridLength(35, GridUnitType.Auto);
                RowAutoUpdateRunUserClassifiers.Height = new GridLength(35, GridUnitType.Auto);
                AutoUpdateOpenClassifiers.Content = "Close classifier selection";
                AutoUpdateOpenClassifiers.Tag = "Close";
            }
            else
            {
                RowAutoUpdateRunStudyClassifiers.Height = new GridLength(0);
                RowAutoUpdateRunUserClassifiers.Height = new GridLength(0);
                AutoUpdateOpenClassifiers.Content = "Select and run classifiers";
                AutoUpdateOpenClassifiers.Tag = "Open";
            }
        }

        private void AutoUpdateCloseImport_Click(object sender, RoutedEventArgs e)
        {
            RowAutoUpdateImport.Height = new GridLength(0);
        }

        private void AutoUpdateOpenTextFilters_Click(object sender, RoutedEventArgs e)
        {
            if (AutoUpdateOpenTextFilters.Tag.ToString() == "Open")
            {
                RowAutoUpdateTextFilterJournal.Height = new GridLength(35, GridUnitType.Auto);
                RowAutoUpdateTextFilterURL.Height = new GridLength(35, GridUnitType.Auto);
                RowAutoUpdateTextFilterDOI.Height = new GridLength(35, GridUnitType.Auto);
                RowAutoUpdateTextFilterTitle.Height = new GridLength(35, GridUnitType.Auto);
                RowAutoUpdatePubTypeFilter.Height = new GridLength(35, GridUnitType.Auto);
                AutoUpdateOpenTextFilters.Content = "Close and clear filters";
                AutoUpdateOpenTextFilters.Tag = "Close";
            }
            else
            {
                RowAutoUpdateTextFilterJournal.Height = new GridLength(0);
                RowAutoUpdateTextFilterURL.Height = new GridLength(0);
                RowAutoUpdateTextFilterDOI.Height = new GridLength(0);
                RowAutoUpdateTextFilterTitle.Height = new GridLength(0);
                RowAutoUpdatePubTypeFilter.Height = new GridLength(0);
                cbFilterPubTypeJournal.IsChecked = false;
                cbFilterPubTypeConferencePaper.IsChecked = false;
                cbFilterPubTypeBookChapter.IsChecked = false;
                cbFilterPubTypeBook.IsChecked = false;
                cbFilterPubTypeDataset.IsChecked = false;
                cbFilterPubTypeRepository.IsChecked = false;
                cbFilterPubTypeThesis.IsChecked = false;
                AutoUpdateOpenTextFilters.Content = "Open filters";
                AutoUpdateOpenTextFilters.Tag = "Open";
                AutoUpdateTextFilterJournal.Text = "";
                AutoUpdateTextFilterURL.Text = "";
                AutoUpdateTextFilterDOI.Text = "";
            }
        }

        private void hlAutoUpdateStudyClassifierRun_Click(object sender, RoutedEventArgs e)
        {
            MagAutoUpdateRun maur = GridAutoUpdateImport.DataContext as MagAutoUpdateRun;
            if (maur != null)
            {
                DataPortal<MagAddClassifierScoresCommand> dp2 = new DataPortal<MagAddClassifierScoresCommand>();
                MagAddClassifierScoresCommand mrmic = new MagAddClassifierScoresCommand(maur.MagAutoUpdateRunId,
                    maur.NPapers, (comboAutoUpdateStudyTypeClassifier.SelectedItem as ComboBoxItem).Content.ToString(),
                    0, 0);
                dp2.ExecuteCompleted += (o, e2) =>
                {
                    busyIndicatorMatches.IsBusy = false;
                    if (e2.Error != null)
                    {
                        RadWindow.Alert(e2.Error.Message);
                    }
                    else
                    {
                        RadWindow.Alert("Classifier running\nThe list below will refresh in 5 minutes and\nthe selected model name will appear.");
                    }
                };
                //busyIndicatorMatches.IsBusy = true;
                hlAutoUpdateStudyClassifierRun.IsEnabled = false;
                hlAutoUpdateUserClassifierRun.IsEnabled = false;
                timerAutoUpdateClassifierRun.Start();
                BusyImportingRecords.IsEnabled = true;
                RowAutoUpdateImport.Height = new GridLength(0);
                dp2.BeginExecute(mrmic);
            }
        }

        private void hlAutoUpdateUserClassifierRun_Click(object sender, RoutedEventArgs e)
        {
            MagAutoUpdateRun maur = GridAutoUpdateImport.DataContext as MagAutoUpdateRun;
            ClassifierContactModel ccm = comboAutoUpdateUserModels.SelectedItem as ClassifierContactModel;

            if (maur != null && ccm != null)
            {
                DataPortal<MagAddClassifierScoresCommand> dp2 = new DataPortal<MagAddClassifierScoresCommand>();
                MagAddClassifierScoresCommand mrmic = new MagAddClassifierScoresCommand(maur.MagAutoUpdateRunId,
                    maur.NPapers, "None", ccm.ModelId, ccm.ReviewId);
                dp2.ExecuteCompleted += (o, e2) =>
                {
                    busyIndicatorMatches.IsBusy = false;
                    if (e2.Error != null)
                    {
                        RadWindow.Alert(e2.Error.Message);
                    }
                    else
                    {
                        RadWindow.Alert("Classifier running. Please check back in a while\nRunning further classifiers disabled for 5 minutes");
                    }
                };
                //busyIndicatorMatches.IsBusy = true;
                hlAutoUpdateStudyClassifierRun.IsEnabled = false;
                hlAutoUpdateUserClassifierRun.IsEnabled = false;
                timerAutoUpdateClassifierRun.Start();
                dp2.BeginExecute(mrmic);
            }
        }

        private void HyperlinkButton_Click_19(object sender, RoutedEventArgs e)
        {
            HyperlinkButton hlb = sender as HyperlinkButton;
            if (hlb != null)
            {
                MagAutoUpdateRun maur = hlb.DataContext as MagAutoUpdateRun;
                if (maur != null)
                {
                    if (maur.NPapers < 1)
                    {
                        RadWindow.Alert("Zero items to list");
                        return;
                    }
                    IncrementHistoryCount();
                    AddToBrowseHistory("Papers identified from auto-update run", "MagAutoUpdateRunList", 0,
                        "", "", 0, "", "", 0, "", "", 0, maur.MagAutoUpdateRunId, "AutoUpdate", 0, 0, 0, -1, "");
                    TBPaperListTitle.Text = "Papers identified from auto-update run";
                    ShowAutoUpdateIdentifiedItems(maur.MagAutoUpdateRunId, "AutoUpdate", 0, 0, 0, -1);
                }
            }
        }

        private void AutoUpdateRefreshImportCount_Click(object sender, RoutedEventArgs e)
        {
            AutoUpdateImportCount();
        }

        private void AutoUpdateImportCount()
        {
            MagAutoUpdateRun maur = GridAutoUpdateImport.DataContext as MagAutoUpdateRun;
            if (maur != null)
            {
                DataPortal<MagAutoUpdateRunCountResultsCommand> dp2 = new DataPortal<MagAutoUpdateRunCountResultsCommand>();
                MagAutoUpdateRunCountResultsCommand maurcrc = new MagAutoUpdateRunCountResultsCommand(maur.MagAutoUpdateRunId,
                    AutoUpdateAutoScoreThreshold.Value.Value, AutoUpdateStudyTypeScoreThreshold.Value.Value, AutoUpdateUserScoreThreshold.Value.Value);
                dp2.ExecuteCompleted += (o, e2) =>
                {
                    busyIndicatorMatches.IsBusy = false;
                    if (e2.Error != null)
                    {
                        RadWindow.Alert(e2.Error.Message);
                    }
                    else
                    {
                        AutoUpdateNRecordsFiltered.Text = (maur.NPapers - e2.Object.ResultsCount).ToString();
                        AutoUpdateImportTopN.Maximum = e2.Object.ResultsCount;
                        //AutoUpdateImportTopN.Value = Math.Min(e2.Object.ResultsCount, maur.NPapers);
                        
                    }
                };
                busyIndicatorMatches.IsBusy = true;
                dp2.BeginExecute(maurcrc);
            }
        }

        private void AutoUpdateImportTopN_ValueChanged(object sender, RadRangeBaseValueChangedEventArgs e)
        {
            if (GridAutoUpdateImport != null)
            {
                MagAutoUpdateRun maur = GridAutoUpdateImport.DataContext as MagAutoUpdateRun;
                if (maur != null)
                {
                    AutoUpdateNumberToImport.Text = "Number to import: " + AutoUpdateImportTopN.Value.Value.ToString() +
                        " out of " + maur.NPapers.ToString();
                }
            }
        }

        private void AutoUpdateImport_Click(object sender, RoutedEventArgs e)
        {
            if (BusyImportingRecords.IsRunning == true)
            {
                RadWindow.Alert("Importing currently in progress");
                return;
            }
            MagAutoUpdateRun maur = GridAutoUpdateImport.DataContext as MagAutoUpdateRun;
            if (maur != null)
            {
                if (AutoUpdateImportTopN.Value.Value > 20000)
                {
                    RadWindow.Alert("Sorry. You can't import more than 20k records at a time.\nYou could try breaking up your search e.g. by date?");
                }
                else
                if (AutoUpdateImportTopN.Value.Value < 1)
                {
                    RadWindow.Alert("No items to import");
                }
                else
                {
                    RadWindow.Confirm("Are you sure you want to import these records?", this.doImportAutoRunResults);
                }
            }
        }

        private string addPubTypeFilter(CheckBox cb, string s)
        {
            if (cb.IsChecked == false)
            {
                return s;
            }
            if (s == "")
            {
                return cb.Tag.ToString();
            }
            return s + "," + cb.Tag.ToString();
        }

        private string getPubTypeFilters()
        {
            string ret = addPubTypeFilter(cbFilterPubTypeUnknown, "");
            ret = addPubTypeFilter(cbFilterPubTypeJournal, ret); 
            ret = addPubTypeFilter(cbFilterPubTypeConferencePaper, ret);
            ret = addPubTypeFilter(cbFilterPubTypeBookChapter, ret);
            ret = addPubTypeFilter(cbFilterPubTypeBook, ret);
            ret = addPubTypeFilter(cbFilterPubTypeDataset, ret);
            ret = addPubTypeFilter(cbFilterPubTypeRepository, ret);
            ret = addPubTypeFilter(cbFilterPubTypeThesis, ret);
            return ret;
        }

        private void doImportAutoRunResults(object sender, WindowClosedEventArgs e)
        {
            var result = e.DialogResult;
            if (result == true)
            {
                MagAutoUpdateRun maur = GridAutoUpdateImport.DataContext as MagAutoUpdateRun;
                if (maur != null)
                {
                    string pubTypeFilters = getPubTypeFilters();
                    DataPortal<MagItemPaperInsertCommand> dp2 = new DataPortal<MagItemPaperInsertCommand>();
                    MagItemPaperInsertCommand command = new MagItemPaperInsertCommand("", "AutoUpdateRun", 0, maur.MagAutoUpdateRunId,
                        (comboAutoUpdateImportOptions.SelectedItem as ComboBoxItem).Tag.ToString(), AutoUpdateAutoScoreThreshold.Value.Value,
                        AutoUpdateStudyTypeScoreThreshold.Value.Value, AutoUpdateUserScoreThreshold.Value.Value,
                        Convert.ToInt32(AutoUpdateImportTopN.Value.Value), AutoUpdateTextFilterJournal.Text,
                        AutoUpdateTextFilterDOI.Text, AutoUpdateTextFilterURL.Text, AutoUpdateTextFilterTitle.Text, "", "", pubTypeFilters);
                    dp2.ExecuteCompleted += (o, e2) =>
                    {
                        BusyImportingRecords.IsRunning = false;
                        tbImportingRecords.Visibility = Visibility.Collapsed;
                        if (e2.Error != null)
                        {
                            RadWindow.Alert(e2.Error.Message);
                        }
                        else
                        {
                            MagBrowserImportedItems = true;
                            int num_in_import = Convert.ToInt32(AutoUpdateImportTopN.Value.Value);
                            if (e2.Object.NImported == num_in_import)
                            {
                                RadWindow.Alert("Imported " + e2.Object.NImported.ToString() + " out of " +
                                    num_in_import.ToString() + " items");
                            }
                            else if (e2.Object.NImported != 0)
                            {
                                if (AutoUpdateTextFilterJournal.Text != "" ||
                                    AutoUpdateTextFilterDOI.Text != "" ||
                                    AutoUpdateTextFilterURL.Text != "" ||
                                    AutoUpdateTextFilterTitle.Text != "" ||
                                    getPubTypeFilters() != "")
                                {
                                    RadWindow.Alert("Some of these items were already in your review or were filtered out.\n\nImported " +
                                    e2.Object.NImported.ToString() + " out of " + num_in_import.ToString() +
                                    " new items");
                                }
                                else
                                {
                                    RadWindow.Alert("Some of these items were already in your review.\n\nImported " +
                                    e2.Object.NImported.ToString() + " out of " + num_in_import.ToString() +
                                    " new items");
                                }
                            }
                            else
                            {
                                RadWindow.Alert("All of these records were already in your review.");
                            }
                        }
                    };
                    BusyImportingRecords.IsRunning = true;
                    tbImportingRecords.Visibility = Visibility.Visible;
                    dp2.BeginExecute(command);
                }
            }
        }

        private void HyperlinkButton_Click_20(object sender, RoutedEventArgs e)
        {
            HyperlinkButton hlb = sender as HyperlinkButton;
            if (hlb != null)
            {
                MagAutoUpdateRun au = hlb.DataContext as MagAutoUpdateRun;
                if (au != null)
                {
                    if (au.NPapers == -1)
                    {
                        RadWindow.Alert("Sorry - this row can't be deleted until it has finished running");
                        return;
                    }
                    else
                    {
                        RememberThisMagAutoUpdateRun = au;
                        RadWindow.Confirm("Are you sure you want to delete this row?", this.doDeleteMagAutoUpdateRun);
                    }
                }
            }
        }

        MagAutoUpdateRun RememberThisMagAutoUpdateRun; // temporary variable to store a specific row while a dialog is showing

        private void doDeleteMagAutoUpdateRun(object sender, WindowClosedEventArgs e)
        {
            var result = e.DialogResult;
            if (result == true)
            {
                CslaDataProvider provider = this.Resources["MagAutoUpdateRunListData"] as CslaDataProvider;
                if (provider != null)
                {
                    MagAutoUpdateRunList runList = provider.Data as MagAutoUpdateRunList;
                    if (runList != null)
                    {
                        runList.Remove(RememberThisMagAutoUpdateRun);
                    }
                }
            }
        }

        private void AutoUpdatePreviewPaperList_Click(object sender, RoutedEventArgs e)
        {
            if (AutoUpdateImportTopN.Value.Value == 0)
            {
                RadWindow.Alert("Zero items to list");
                return;
            }
            HyperlinkButton hlb = sender as HyperlinkButton;
            if (hlb != null)
            {
                MagAutoUpdateRun maur = hlb.DataContext as MagAutoUpdateRun;
                if (maur != null)
                {
                    IncrementHistoryCount();
                    AddToBrowseHistory("Papers identified from auto-update run", "MagAutoUpdateRunList", 0,
                        "", "", 0, "", "", 0, "", "", 0, maur.MagAutoUpdateRunId,
                        (comboAutoUpdateImportOptions.SelectedItem as ComboBoxItem).Tag.ToString(),
                        AutoUpdateAutoScoreThreshold.Value.Value,
                        AutoUpdateStudyTypeScoreThreshold.Value.Value,
                        AutoUpdateUserScoreThreshold.Value.Value,
                        Convert.ToInt32(AutoUpdateImportTopN.Value.Value), "");
                    TBPaperListTitle.Text = "Papers identified from auto-update run";
                    ShowAutoUpdateIdentifiedItems(maur.MagAutoUpdateRunId,
                        (comboAutoUpdateImportOptions.SelectedItem as ComboBoxItem).Tag.ToString(),
                        AutoUpdateAutoScoreThreshold.Value.Value,
                        AutoUpdateStudyTypeScoreThreshold.Value.Value,
                        AutoUpdateUserScoreThreshold.Value.Value,
                        Convert.ToInt32(AutoUpdateImportTopN.Value.Value));
                }
            }
        }

        private void TimerAutoUpdateClassifierRun_Tick(object sender, EventArgs e)
        {
            this.timerAutoUpdateClassifierRun.Stop();
            BusyImportingRecords.IsEnabled = false;
            CslaDataProvider provider = this.Resources["MagAutoUpdateRunListData"] as CslaDataProvider;
            if (provider != null)
            {
                provider.Refresh();
            }
            hlAutoUpdateUserClassifierRun.IsEnabled = true;
            hlAutoUpdateStudyClassifierRun.IsEnabled = true;
        }

        /***************** Add list of IDs to selected items list ************************/

        private void LBUploadIDsFile_Click(object sender, RoutedEventArgs e)
        {
            if (TBUploadIDs.Text != "")
            {
                string idStr = TBUploadIDs.Text.Replace("\n\r", "¬").Replace("r\n", "¬").Replace("\n", "¬").Replace("\r", "¬").Replace(",", "¬");
                string[] IDs = idStr.Split('¬');
                Int64 testInt64 = 0;
                int count = 0;
                foreach (string s in IDs)
                {
                    if (s.Trim() != "" && Int64.TryParse(s, out testInt64))
                    {
                        if (AddToSelectedList(testInt64))
                        {
                            count++;
                        }
                    }
                }
                RadWindow.Alert(count.ToString() + " IDs added to selected list");
            }
        }

        
        private void InfList_Saved(object sender, Csla.Core.SavedEventArgs e)
        {
            CslaDataProvider provider = this.Resources["MagCurrentInfoListData"] as CslaDataProvider;
            {
                if (provider != null)
                {
                    provider.Refresh();
                }
            }
        }

        MagCurrentInfo CurrentTempMagCurrentInfo;

        private void HyperlinkButton_Click_21(object sender, RoutedEventArgs e)
        {
            MagCurrentInfo mci = (sender as HyperlinkButton).DataContext as MagCurrentInfo;
            if (mci != null)
            {
                if (mci.MakesDeploymentStatus == "LIVE")
                {
                    RadWindow.Alert("This endpoint is already LIVE");
                    return;
                }
                CurrentTempMagCurrentInfo = mci;
                RadWindow.Confirm("Are you sure you want to make this endpoint live?", this.doCurrentInfoLive);
            }
        }

        private void doCurrentInfoLive(object sender, WindowClosedEventArgs e)
        {
            var result = e.DialogResult;
            if (result == true && CurrentTempMagCurrentInfo != null)
            {
                CurrentTempMagCurrentInfo.MakesDeploymentStatus = "LIVE";
                CurrentTempMagCurrentInfo.Saved -= CurrentTempMagCurrentInfo_Saved;
                CurrentTempMagCurrentInfo.Saved += CurrentTempMagCurrentInfo_Saved;
                CurrentTempMagCurrentInfo.BeginSave();
            }
        }

        private void CurrentTempMagCurrentInfo_Saved(object sender, Csla.Core.SavedEventArgs e)
        {
            CslaDataProvider provider = this.Resources["MagCurrentInfoListData"] as CslaDataProvider;
            {
                if (provider != null)
                {
                    provider.Refresh();
                }
            }
            tbAcademicTitle.Text = "OpenAlex dataset: " + CurrentTempMagCurrentInfo.MagFolder;
            CurrentTempMagCurrentInfo = null;
            CslaDataProvider provider2 = ((CslaDataProvider)App.Current.Resources["MagCurrentInfoData"]);
            provider2.Refresh();
        }

        MagCurrentInfo CurrentTempMagCurrentInfoPending;

        private void HyperlinkButton_Click_22(object sender, RoutedEventArgs e)
        {
            MagCurrentInfo mci = (sender as HyperlinkButton).DataContext as MagCurrentInfo;
            if (mci != null)
            {
                if (mci.MakesDeploymentStatus == "PENDING")
                {
                    RadWindow.Alert("This endpoint is already PENDING");
                    return;
                }
                CurrentTempMagCurrentInfoPending = mci;
                RadWindow.Confirm("Are you sure you want to make this endpoint pending?", this.doCurrentInfoPending);
            }
        }

        private void doCurrentInfoPending(object sender, WindowClosedEventArgs e)
        {
            var result = e.DialogResult;
            if (result == true)
            {
                CurrentTempMagCurrentInfoPending.MakesDeploymentStatus = "PENDING";
                CurrentTempMagCurrentInfoPending.Saved -= CurrentTempMagCurrentInfo_SavedPending;
                CurrentTempMagCurrentInfoPending.Saved += CurrentTempMagCurrentInfo_SavedPending;
                CurrentTempMagCurrentInfoPending.BeginSave();
            }
        }

        private void CurrentTempMagCurrentInfo_SavedPending(object sender, Csla.Core.SavedEventArgs e)
        {
            CslaDataProvider provider = this.Resources["MagCurrentInfoListData"] as CslaDataProvider;
            {
                if (provider != null)
                {
                    provider.Refresh();
                }
            }
            CurrentTempMagCurrentInfoPending = null;
        }

        private void HyperlinkButton_Click_23(object sender, RoutedEventArgs e)
        {
            MagCurrentInfo mci = (sender as HyperlinkButton).DataContext as MagCurrentInfo;
            if (mci != null)
            {
                if (mci.MakesDeploymentStatus == "LIVE")
                {
                    RadWindow.Alert("Sorry, you can't delete the LIVE endpoint");
                    return;
                }
                CurrentTempMagCurrentInfo = mci;
                RadWindow.Confirm("Are you sure you want to delete this endpoint?", this.doCurrentInfoDelete);
            }
        }

        private void doCurrentInfoDelete(object sender, WindowClosedEventArgs e)
        {
            var result = e.DialogResult;
            if (result == true && CurrentTempMagCurrentInfo != null)
            {
                CslaDataProvider provider = this.Resources["MagCurrentInfoListData"] as CslaDataProvider;
                {
                    if (provider != null)
                    {
                        MagCurrentInfoList mcil = provider.Data as MagCurrentInfoList;
                        if (mcil != null)
                        {
                            mcil.Saved -= InfList_Saved;
                            mcil.Saved += InfList_Saved;
                            mcil.Remove(CurrentTempMagCurrentInfo);
                        }
                    }
                }
            }
        }

        private void LBCreateCodeSetFromFosIds_Click(object sender, RoutedEventArgs e)
        {
            if (TBUploadIDs.Text == "")
            {
                RadWindow.Alert("You need to enter some IDs first");
                return;
            }
            RadWindow.Confirm("Are you sure you want to create a codeset from these IDs?", this.doCreateCodeSetFromFosIds);
        }

        private void doCreateCodeSetFromFosIds(object sender, WindowClosedEventArgs e)
        {
            var result = e.DialogResult;
            if (result == true)
            {
                ReviewSetsList rsl = (App.Current.Resources["CodeSetsData"] as CslaDataProvider).Data as ReviewSetsList;
                DataPortal<MagImportFieldsOfStudyCommand> dp2 = new DataPortal<MagImportFieldsOfStudyCommand>();
                MagImportFieldsOfStudyCommand command = new MagImportFieldsOfStudyCommand(
                    "",
                    -1,
                    rsl.Count,
                    0,
                    0,
                    0,
                    "Use OpenAlex assigned topics",
                    TBUploadIDs.Text);
                dp2.ExecuteCompleted += (o, e2) =>
                {
                    BusyImportingRecords.IsRunning = false;
                    tbImportingRecords.Visibility = Visibility.Collapsed;
                    if (e2.Error != null)
                    {
                        RadWindow.Alert(e2.Error.Message);
                    }
                    else
                    {
                        
                    }
                };
                BusyImportingRecords.IsRunning = true;
                tbImportingRecords.Visibility = Visibility.Visible;
                dp2.BeginExecute(command);
            }
        }

    }
}