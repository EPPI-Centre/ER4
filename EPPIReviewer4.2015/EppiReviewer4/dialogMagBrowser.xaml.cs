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

namespace EppiReviewer4
{
    public partial class dialogMagBrowser : UserControl
    {
        public event EventHandler<RoutedEventArgs> ListIncludedThatNeedMatching;
        public event EventHandler<RoutedEventArgs> ListExcludedThatNeedMatching;
        public event EventHandler<RoutedEventArgs> ListIncludedNotMatched;
        public event EventHandler<RoutedEventArgs> ListExcludedNotMatched;
        private DispatcherTimer timer;
        private int CurrentBrowsePosition = 0;
        private List<Int64> SelectedPaperIds;
        public dialogMagBrowser()
        {
            InitializeComponent();
        }

        public void InitialiseBrowser()
        {
            SelectedPaperIds = new List<Int64>();
            UpdateSelectedCount();
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
        }

        public void ShowMagBrowser()
        {
            //HLShowAdvanced_Click(null, null);
            LBManageRelatedPapersRun_Click(null, null);
            InitialiseBrowser();
            //CslaDataProvider provider = this.Resources["RelatedPapersRunListData"] as CslaDataProvider;
            //provider.Refresh();
        }

        // ************************** Top navigation button events **************************
        private void HLShowAdvanced_Click(object sender, RoutedEventArgs e)
        {
            IncrementHistoryCount();
            AddToBrowseHistory("Advanced page", "Advanced", 0, "", "", 0, "", 0, "", "", 0);
            ShowAdvancedPage();
        }

        private void HLShowHistory_Click(object sender, RoutedEventArgs e)
        {
            IncrementHistoryCount();
            AddToBrowseHistory("View browse history", "History", 0, "", "", 0, "", 0, "", "", 0);
            ShowHistoryPage();
        }

        private void HLShowSelected_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedPaperIds.Count == 0)
            {
                RadWindow.Alert("You don't have anything selected.");
                return;
            }
            IncrementHistoryCount();
            AddToBrowseHistory("List of all selected papers", "SelectedPapers", 0, "", "", 0, "", 0, "", "", 0);
            TBPaperListTitle.Text = "List of all selected papers";
            ShowSelectedPapersPage();
        }
        private void LBManageRelatedPapersRun_Click(object sender, RoutedEventArgs e)
        {
            IncrementHistoryCount();
            AddToBrowseHistory("Manage review updates / find related papers", "RelatedPapers", 0, "", "", 0, "", 0, "", "", 0);
            ShowRelatedPapersPage();
        }

        // ************************************* Advanced PAGE ******************************************

        private void ShowAdvancedPage()
        {
            StatusGrid.Visibility = Visibility.Visible;
            PaperGrid.Visibility = Visibility.Collapsed;
            TopicsGrid.Visibility = Visibility.Collapsed;
            PaperListGrid.Visibility = Visibility.Collapsed;
            HistoryGrid.Visibility = Visibility.Collapsed;
            RelatedPapersGrid.Visibility = Visibility.Collapsed;

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
                    //TBNumInReview.Text = mrmic2.NInReviewIncluded.ToString() + " / " + mrmic2.NInReviewExcluded.ToString();
                    LBListMatchesIncluded.Content = mrmic2.NMatchedAccuratelyIncluded.ToString();
                    LBListMatchesExcluded.Content = mrmic2.NMatchedAccuratelyExcluded.ToString();
                    LBListAllInReview.Content = (mrmic2.NMatchedAccuratelyIncluded + mrmic2.NMatchedAccuratelyExcluded).ToString();
                    LBManualCheckIncluded.Content = mrmic2.NRequiringManualCheckIncluded.ToString();
                    LBManualCheckExcluded.Content = mrmic2.NRequiringManualCheckExcluded.ToString();
                    LBMNotMatchedIncluded.Content = mrmic2.NNotMatchedIncluded.ToString();
                    LBMNotMatchedExcluded.Content = mrmic2.NNotMatchedExcluded.ToString();
                }
            };
            //BusyLoading.IsRunning = true;
            dp2.BeginExecute(mrmic);
        }

        // ********************************* HISTORY PAGE **********************************

        private void ShowHistoryPage()
        {
            StatusGrid.Visibility = Visibility.Collapsed;
            PaperGrid.Visibility = Visibility.Collapsed;
            TopicsGrid.Visibility = Visibility.Collapsed;
            PaperListGrid.Visibility = Visibility.Collapsed;
            HistoryGrid.Visibility = Visibility.Visible;
            RelatedPapersGrid.Visibility = Visibility.Collapsed;
        }

        // ******************************** PAPER DETAILS PAGE **************************************
        public void ShowPaperDetailsPage(Int64 PaperId, string FullRecord, string Abstract, string URLs, Int64 LinkedITEM_ID)
        {
            StatusGrid.Visibility = Visibility.Collapsed;
            PaperGrid.Visibility = Visibility.Visible;
            TopicsGrid.Visibility = Visibility.Collapsed;
            PaperListGrid.Visibility = Visibility.Collapsed;
            HistoryGrid.Visibility = Visibility.Collapsed;
            RelatedPapersGrid.Visibility = Visibility.Collapsed;
            CitationPane.SelectedIndex = 0;

            RTBPaperInfo.Text = FullRecord;
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
                    for (int i = 0; i < splitted.Length; i++)
                    {
                        if (splitted[i].Length > 7)
                        {
                            HyperlinkButton newHl = new HyperlinkButton();
                            newHl.Content = splitted[i];
                            newHl.NavigateUri = new Uri(splitted[i]);
                            newHl.TargetName = "_blank";
                            newHl.Margin = new Thickness(2, 1, 1, 1);
                            WPPaperURLs.Children.Add(newHl);
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
                        e2.Object.PaperId, e2.Object.FullRecord, e2.Object.Abstract, e2.Object.LinkedITEM_ID, e2.Object.URLs, 0,
                        "", "", 0);
                    ShowPaperDetailsPage(e2.Object.PaperId, e2.Object.FullRecord, e2.Object.Abstract,
                        e2.Object.URLs, e2.Object.LinkedITEM_ID);
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
            StatusGrid.Visibility = Visibility.Collapsed;
            PaperGrid.Visibility = Visibility.Collapsed;
            TopicsGrid.Visibility = Visibility.Collapsed;
            PaperListGrid.Visibility = Visibility.Visible;
            HistoryGrid.Visibility = Visibility.Collapsed;
            RelatedPapersGrid.Visibility = Visibility.Collapsed;

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

        private void ShowAutoIdentifiedMatches(int MagRelatedRunId)
        {
            StatusGrid.Visibility = Visibility.Collapsed;
            PaperGrid.Visibility = Visibility.Collapsed;
            TopicsGrid.Visibility = Visibility.Collapsed;
            PaperListGrid.Visibility = Visibility.Visible;
            HistoryGrid.Visibility = Visibility.Collapsed;
            RelatedPapersGrid.Visibility = Visibility.Collapsed;

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

        private void ShowAllWithThisCode(string AttributeIds)
        {
            StatusGrid.Visibility = Visibility.Collapsed;
            PaperGrid.Visibility = Visibility.Collapsed;
            TopicsGrid.Visibility = Visibility.Collapsed;
            PaperListGrid.Visibility = Visibility.Visible;
            HistoryGrid.Visibility = Visibility.Collapsed;
            RelatedPapersGrid.Visibility = Visibility.Collapsed;

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

        // *********************************** Topic page *********************************
        private void ShowTopicPage(Int64 FieldOfStudyId, string FieldOfStudy)
        {
            StatusGrid.Visibility = Visibility.Collapsed;
            PaperGrid.Visibility = Visibility.Collapsed;
            TopicsGrid.Visibility = Visibility.Visible;
            PaperListGrid.Visibility = Visibility.Collapsed;
            HistoryGrid.Visibility = Visibility.Collapsed;
            RelatedPapersGrid.Visibility = Visibility.Collapsed;

            TBMainTopic.Text = FieldOfStudy;

            getParentAndChildFieldsOfStudy("FieldOfStudyParentsList", FieldOfStudyId, WPParentTopics, "Parent topics");
            getParentAndChildFieldsOfStudy("FieldOfStudyChildrenList", FieldOfStudyId, WPChildTopics, "Child topics");
            getPaperListForTopic(FieldOfStudyId);
        }

        // ***************************** List selected papers page **************************************

        private void ShowSelectedPapersPage()
        {
            StatusGrid.Visibility = Visibility.Collapsed;
            PaperGrid.Visibility = Visibility.Collapsed;
            TopicsGrid.Visibility = Visibility.Collapsed;
            PaperListGrid.Visibility = Visibility.Visible;
            HistoryGrid.Visibility = Visibility.Collapsed;
            RelatedPapersGrid.Visibility = Visibility.Collapsed;

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
            CslaDataProvider provider = this.Resources["RelatedPapersRunListData"] as CslaDataProvider;
            provider.Refresh();
            StatusGrid.Visibility = Visibility.Collapsed;
            PaperGrid.Visibility = Visibility.Collapsed;
            TopicsGrid.Visibility = Visibility.Collapsed;
            PaperListGrid.Visibility = Visibility.Collapsed;
            HistoryGrid.Visibility = Visibility.Collapsed;
            RelatedPapersGrid.Visibility = Visibility.Visible;
        }

        private void LBListMatchesIncluded_Click(object sender, RoutedEventArgs e)
        {
            IncrementHistoryCount();
            AddToBrowseHistory("List of all included matches", "MatchesIncluded", 0, "", "", 0, "", 0, "", "", 0);
            TBPaperListTitle.Text = "List of all included matches";
            ShowIncludedMatchesPage("included");
        }

        private void LBListMatchesExcluded_Click(object sender, RoutedEventArgs e)
        {
            IncrementHistoryCount();
            AddToBrowseHistory("List of all excluded matches", "MatchesExcluded", 0, "", "", 0, "", 0, "", "", 0);
            TBPaperListTitle.Text = "List of all excluded matches";
            ShowIncludedMatchesPage("excluded");
        }

        private void LBListAllInReview_Click(object sender, RoutedEventArgs e)
        {
            IncrementHistoryCount();
            AddToBrowseHistory("List of all matches in review (included and excluded)", "MatchesIncludedAndExcluded",
                0, "", "", 0, "", 0, "", "", 0);
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
                    attributeIDs = codesSelectControlMAGSelect.SelectedAttributeSet().AttributeSetId.ToString();
                    attributeNames = codesSelectControlMAGSelect.SelectedAttributeSet().AttributeName.ToString();
                }
                foreach (AttributeSet attribute in codesSelectControlMAGSelect.SelectedAttributes())
                {
                    if (attributeIDs == "")
                    {
                        attributeIDs = attribute.AttributeSetId.ToString();
                        attributeNames = attribute.AttributeName;
                    }
                    else
                    {
                        attributeIDs += "," + attribute.AttributeSetId.ToString();
                        attributeNames += ", OR " + attribute.AttributeName;
                    }
                }
                IncrementHistoryCount();
                AddToBrowseHistory("List of all item matches with this code", "ReviewMatchedPapersWithThisCode", 0,
                    "", "", 0, "", 0, "", attributeIDs, 0);
                ShowAllWithThisCode(attributeIDs);
            }
        }

        private void LBManualCheckIncluded_Click(object sender, RoutedEventArgs e)
        {
            this.ListIncludedThatNeedMatching.Invoke(sender, e);
        }

        private void LBManualCheckExcluded_Click(object sender, RoutedEventArgs e)
        {
            this.ListExcludedThatNeedMatching.Invoke(sender, e);            
        }

        private void LBMNotMatchedIncluded_Click(object sender, RoutedEventArgs e)
        {
            this.ListIncludedNotMatched.Invoke(sender, e);
        }

        private void LBMNotMatchedExcluded_Click(object sender, RoutedEventArgs e)
        {
            this.ListExcludedNotMatched.Invoke(sender, e);
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
            IncrementHistoryCount();
            HyperlinkButton hl = sender as HyperlinkButton;
            if (hl != null)
            {
                AddToBrowseHistory("Browse topic: " + hl.Content.ToString(), "BrowseTopic", 0, "", "", 0, "",
                    Convert.ToInt64(hl.Tag), hl.Content.ToString(), "", 0);
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

        private void PaperListBibliographyGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            MagPaper paper = (sender as TextBlock).DataContext as MagPaper;
            IncrementHistoryCount();
            AddToBrowseHistory("Browse paper: " + paper.FullRecord, "PaperDetail", paper.PaperId, paper.FullRecord,
                paper.Abstract, paper.LinkedITEM_ID, paper.URLs, 0, "", "", 0);
            ShowPaperDetailsPage(paper.PaperId, paper.FullRecord, paper.Abstract, paper.URLs, paper.LinkedITEM_ID);
        }

        // **************************** Managing page changes on the paper grid list views *****************

        private void PaperListBibliographyPager_PageIndexChanging(object sender, PageIndexChangingEventArgs e)
        {
            CslaDataProvider provider = this.Resources["PaperListData"] as CslaDataProvider;
            provider.FactoryParameters.Clear();
            MagPaperList mpl = provider.Data as MagPaperList;
            MagPaperListSelectionCriteria selectionCriteria = new MagPaperListSelectionCriteria();
            selectionCriteria.PageSize = 20;
            selectionCriteria.PageNumber = e.NewPageIndex;

            if (mpl.PaperIds == "" && (mpl.AttributeIds == "" || mpl.AttributeIds == null) && mpl.MagRelatedRunId == 0)
            {
                selectionCriteria.ListType = "ReviewMatchedPapers";
                selectionCriteria.Included = mpl.IncludedOrExcluded;
            }
            else if (mpl.PaperIds != "")
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
            provider.FactoryParameters.Add(selectionCriteria);
            provider.FactoryMethod = "GetMagPaperList";
            provider.Refresh();
        }

        private void TopicPaperListBibliographyPager_PageIndexChanging(object sender, PageIndexChangingEventArgs e)
        {
            CslaDataProvider provider = this.Resources["TopicPaperListData"] as CslaDataProvider;
            MagPaperList mpl = provider.Data as MagPaperList;
            provider.FactoryParameters.Clear();
            MagPaperListSelectionCriteria selectionCriteria = new MagPaperListSelectionCriteria();
            selectionCriteria.PageSize = 20;
            selectionCriteria.PageNumber = e.NewPageIndex;
            selectionCriteria.ListType = "PaperFieldsOfStudyList";
            selectionCriteria.FieldOfStudyId = mpl.FieldOfStudyId;
            provider.FactoryParameters.Add(selectionCriteria);
            provider.FactoryMethod = "GetMagPaperList";
            provider.Refresh();
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


        // *********************** Showing / hiding abstracts *********************************
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
        private void AddToBrowseHistory(string title, string browseType, Int64 PaperId, string PaperFullRecord,
            string PaperAbstract, Int64 LinkedITEM_ID, string URLs, Int64 FieldOfStudyId, string FieldOfStudy, string AttributeIds, int MagRelatedRunId)
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
            mbh.LinkedITEM_ID = LinkedITEM_ID;
            mbh.URLs = URLs;
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
                                ShowHistoryPage();
                                break;
                            case "Advanced":
                                ShowAdvancedPage();
                                break;
                            case "PaperDetail":
                                ShowPaperDetailsPage(mbh.PaperId, mbh.PaperFullRecord, mbh.PaperAbstract, mbh.URLs, mbh.LinkedITEM_ID);
                                break;
                            case "MatchesIncluded":
                                TBPaperListTitle.Text = mbh.Title;
                                ShowIncludedMatchesPage("included");
                                break;
                            case "MatchesExcluded":
                                TBPaperListTitle.Text = mbh.Title;
                                ShowIncludedMatchesPage("excluded");
                                break;
                            case "MatchesIncludedAndExcluded":
                                TBPaperListTitle.Text = mbh.Title;
                                ShowIncludedMatchesPage("all");
                                break;
                            case "ReviewMatchedPapersWithThisCode":
                                ShowAllWithThisCode(mbh.AttributeIds);
                                break;
                            case "MagRelatedPapersRunList":
                                TBPaperListTitle.Text = mbh.Title;
                                ShowAutoIdentifiedMatches(mbh.MagRelatedRunId);
                                break;
                            case "BrowseTopic":
                                ShowTopicPage(mbh.FieldOfStudyId, mbh.FieldOfStudy);
                                break;
                            case "SelectedPapers":
                                TBPaperListTitle.Text = mbh.Title;
                                ShowSelectedPapersPage();
                                break;
                            case "RelatedPapers":
                                ShowRelatedPapersPage();
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
                    HLShowHistory.Content = "Show history (" + CurrentBrowsePosition.ToString() + " / " +
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

        private void IncrementHistoryCount()
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
            HLShowSelected.Content = "Show selected (" + SelectedPaperIds.Count.ToString() + ")";
        }

        private void ClearSelectionsFromPaperLists()
        {
            ResetSelected("PaperListData");
            ResetSelected("TopicPaperListData");
            ResetSelected("CitationPaperListData");
            ResetSelected("CitedByListData");
            ResetSelected("RecommendationsListData");
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

        private void AddToSelectedList(Int64 PaperId)
        {
            if (!IsInSelectedList(PaperId))
            {
                SelectedPaperIds.Add(PaperId);
                UpdateSelectedCount();
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
            if (SelectedPaperIds.Count == 0)
            {
                RadWindow.Alert("You don't have anything selected");
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
                MagItemPaperInsertCommand command = new MagItemPaperInsertCommand(GetSelectedIds());
                dp2.ExecuteCompleted += (o, e2) =>
                {
                    //BusyLoading.IsRunning = false;
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
                        
                        SelectedPaperIds.Clear();
                        ClearSelectionsFromPaperLists();
                        UpdateSelectedCount();
                        HLImportSelected.IsEnabled = true;
                    }
                };
                //BusyLoading.IsRunning = true;
                HLImportSelected.IsEnabled = false;
                dp2.BeginExecute(command);
            }
        }

        // ******************************* Find topics using search box ********************************

        private void tbFindTopics_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (tbFindTopics.Text.Length > 2)
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
                            newHl.Click += HlNavigateToTopic_Click;
                            newHl.FontSize = i;
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
                tb.Text = "Search for topics in the box above. Wildcards work e.g. physic*";
                tb.Margin = new Thickness(5, 5, 5, 5);
                WPFindTopics.Children.Add(tb);
            }
        }

        // ********************************** Matching items from review to MAG *******************************

        private void LBMatchAllIncluded_Click(object sender, RoutedEventArgs e)
        {
            RadWindow.Confirm("Are you sure you want to match all the items in your review\n to Microsoft Academic records?", OnShowWizardDialogClosed);
        }

        private void OnShowWizardDialogClosed(object sender, WindowClosedEventArgs e)
        {
            if (e.DialogResult == true)
            {
                DataPortal<MagMatchItemsToPapersCommand> dp = new DataPortal<MagMatchItemsToPapersCommand>();
                MagMatchItemsToPapersCommand GetMatches = new MagMatchItemsToPapersCommand("FindMatches",
                    true, 0, 0);
                dp.ExecuteCompleted += (o, e2) =>
                {
                    if (e2.Error != null)
                    {
                        RadWindow.Alert(e2.Error.Message);
                    }
                    else
                    {
                        MagMatchItemsToPapersCommand res = e2.Object as MagMatchItemsToPapersCommand;
                        RadWindow.Alert(res.currentStatus);
                    }
                };
                dp.BeginExecute(GetMatches);
            }
        }


        // ************************** Managing related papers / review auto-updates **********************

        private void LBAddNewRagRelatedPapersRun_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as HyperlinkButton).Tag.ToString() == "ClickToOpen")
            {
                RowCreateNewRelatedPapersRun.Height = new GridLength(50, GridUnitType.Auto);
                LBAddNewRagRelatedPapersRun.Content = "Adding new search for related papers / auto update search for review (Click to close)";
                LBAddNewRagRelatedPapersRun.Tag = "ClickToClose";
                tbRelatedPapersRunDescription.Text = "";
            }
            else
            {
                RowCreateNewRelatedPapersRun.Height = new GridLength(0);
                LBAddNewRagRelatedPapersRun.Content = "Add new search for related papers / auto update search for review";
                LBAddNewRagRelatedPapersRun.Tag = "ClickToOpen";
            }
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (RowSelectCodeRelatedPapersRun != null)
            {
                RowSelectCodeRelatedPapersRun.Height = new GridLength(0);
            }
        }

        private void RadioButton_Checked_1(object sender, RoutedEventArgs e)
        {
            if (RowSelectCodeRelatedPapersRun != null)
            {
                RowSelectCodeRelatedPapersRun.Height = new GridLength(50, GridUnitType.Auto);
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

        private void LBAddRelatedPapersRun_Click(object sender, RoutedEventArgs e)
        {
            MagRelatedPapersRun mrpr = new MagRelatedPapersRun();
            if (RadioButtonRelatedPapersRunAllIncluded.IsChecked == true)
            {
                mrpr.AllIncluded = true;
                mrpr.AttributeId = 0;
            }
            else
            {
                if (codesSelectControlRelatedPapersRun.SelectedAttributeSet() == null)
                {
                    RadWindow.Alert("Please select an attribute");
                    return;
                }
                else
                {
                    mrpr.AttributeId = codesSelectControlRelatedPapersRun.SelectedAttributeSet().AttributeId;
                    mrpr.AttributeName = codesSelectControlRelatedPapersRun.SelectedAttributeSet().AttributeName;
                    mrpr.AllIncluded = false;
                }
            }
            mrpr.AutoReRun = cbRelatedPapersRunAutoRun.IsChecked == true ? true : false;
            if (RadioButtonRelatedPapersRunNoDateRestriction.IsChecked == true)
            {
                mrpr.DateFrom = null;
            }
            else
            {
                if (DatePickerRelatedPapersRun.SelectedDate != null)
                {
                    mrpr.DateFrom = DatePickerRelatedPapersRun.SelectedDate;
                }
                else
                {
                    RadWindow.Alert("Please select a date to date items from");
                    return;
                }
            }
            if (tbRelatedPapersRunDescription.Text == "")
            {
                RadWindow.Alert("Please enter a description");
                return;
            }
            else
            {
                mrpr.UserDescription = tbRelatedPapersRunDescription.Text;
            }
            CslaDataProvider provider = this.Resources["RelatedPapersRunListData"] as CslaDataProvider;
            if (provider != null)
            {
                MagRelatedPapersRunList mrprl = provider.Data as MagRelatedPapersRunList;
                if (mrprl != null)
                {
                    mrpr.Status = "Pending";
                    mrpr.Filtered = RBRelatedRCTFilterNone.IsChecked.Value ? "None" : RBRelatedRCTFilterPrecise.IsChecked.Value ? "Precise" : "Sensitive";
                    mrpr.Mode = (ComboRelatedPapersMode.SelectedItem as ComboBoxItem).Tag.ToString();
                    mrprl.Add(mrpr);
                    mrprl.SaveItem(mrpr);
                    RowCreateNewRelatedPapersRun.Height = new GridLength(0);
                    LBAddNewRagRelatedPapersRun.Content = "Add new search for related papers / auto update search for review";
                    LBAddNewRagRelatedPapersRun.Tag = "ClickToOpen";
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
                    RememberThisMagRelatedPapersRun = pr;
                    RadWindow.Confirm("Are you sure you want to delete this row?", this.doDeleteMagRelatedPapersRun);
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
                    AddToBrowseHistory("Papers identified from auto-identification run", "MagRelatedPapersRunList", 0,
                        "", "", 0, "", 0, "", "", pr.MagRelatedRunId);
                    TBPaperListTitle.Text = "Papers identified from auto-identification run";
                    ShowAutoIdentifiedMatches(pr.MagRelatedRunId);
                }
            }
        }

        private void HyperlinkButton_Click_4(object sender, RoutedEventArgs e)
        {
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
                    if (pr.UserStatus == "Checked")
                    {
                        RememberThisMagRelatedPapersRun = pr;
                        RadWindow.Confirm("Are you sure you want to import these items?\n(This set is already marked as 'checked'.)", this.DoImportItems);
                    }
                    else if (pr.UserStatus == "Unchecked")
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
                    DataPortal<MagItemMagRelatedPaperInsertCommand> dp2 = new DataPortal<MagItemMagRelatedPaperInsertCommand>();
                    MagItemMagRelatedPaperInsertCommand command = new MagItemMagRelatedPaperInsertCommand(RememberThisMagRelatedPapersRun.MagRelatedRunId);
                    dp2.ExecuteCompleted += (o, e2) =>
                    {
                        //BusyLoading.IsRunning = false;
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
                                RadWindow.Alert("Some of these items were already in your review.\n\nImported " +
                                    e2.Object.NImported.ToString() + " out of " + num_in_run.ToString() +
                                    " new items");
                            }
                            else
                            {
                                RadWindow.Alert("All of these records were already in your review.");
                            }
                            //SelectedLinkButton.IsEnabled = true; - no need to renable, as it's destroyed in the refresh below
                            CslaDataProvider provider = this.Resources["RelatedPapersRunListData"] as CslaDataProvider;
                            provider.Refresh();
                        }
                    };
                    //BusyLoading.IsRunning = true;
                    SelectedLinkButton.IsEnabled = false;
                    dp2.BeginExecute(command);
                }
            }
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

        // **************************** Simulation studies in research & development *******************************

        private void rbSimulationYear_Click(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            if (rb != null)
            {
                if (rb.Tag.ToString() == "ShowCodesControl")
                {
                    DatePickerSimulation.Visibility = Visibility.Collapsed;
                    codesSelectControlSimulation.Visibility = Visibility.Visible;
                }
                else
                {
                    DatePickerSimulation.Visibility = Visibility.Visible;
                    codesSelectControlSimulation.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void lbRunSimulation_Click(object sender, RoutedEventArgs e)
        {
            DateTime SimulationYear = Convert.ToDateTime("1/1/1753");
            DateTime CreatedDate = Convert.ToDateTime("1/1/1753");
            Int64 AttributeId = 0;
            if (rbSimulationYear.IsChecked == true)
            {
                SimulationYear = DatePickerSimulation.SelectedValue.Value;
                AttributeId = 0;
            }
            if (rbSimulationCreatedDate.IsChecked == true)
            {
                CreatedDate = DatePickerSimulation.SelectedValue.Value;
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
            DataPortal<MagRunSimulationCommand> dp2 = new DataPortal<MagRunSimulationCommand>();
            MagRunSimulationCommand mrsc = new MagRunSimulationCommand(SimulationYear, CreatedDate, AttributeId);
            dp2.ExecuteCompleted += (o, e2) =>
            {
                //BusyLoading.IsRunning = false;
                if (e2.Error != null)
                {
                    RadWindow.Alert(e2.Error.Message);
                    tbSimulationResults.Text = "error";
                }
                else
                {
                    MagRunSimulationCommand mrsc2 = e2.Object as MagRunSimulationCommand;
                    if (mrsc2 != null)
                    {
                        lbRunSimulation.IsEnabled = true;
                        tbSimulationResults.Text = mrsc2.GetReport();
                    }
                }
            };
            //BusyLoading.IsRunning = true;
            tbSimulationResults.Text = "Working...";
            lbRunSimulation.IsEnabled = false;
            dp2.BeginExecute(mrsc);
        }

        






        // 88888888888888888888888888888 NOT IMPLEMENTED YET 8888888888888888888888888888888888888888


    }
}