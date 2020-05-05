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
using System.Windows.Data;
using System.IO;
using Csla.Silverlight;
using BusinessLibrary.BusinessClasses;
using BusinessLibrary.Security;
using Telerik.Windows.Controls;
using Telerik.Windows.Input;
using Telerik.Windows.Data;
using Csla;
using Csla.DataPortalClient;
using Telerik.Windows;
using Telerik.Windows.Controls.Docking;
using Telerik.Windows.Controls.GridView;
using System.Xml;
using System.ComponentModel;
using EppiReviewer4.Helpers;
using System.Windows.Media.Imaging;
using Csla.Xaml;
using MindFusion.Diagramming.Silverlight;
using System.Collections.ObjectModel;
using Telerik.Windows.Controls.ChartView;
using Telerik.Charting;
using CircularRelationshipGraph;
using System.Windows.Threading;


namespace EppiReviewer4
{
    public partial class homeDocuments : UserControl
    {
        ImportItems _ImportItems = null;
        
        public event EventHandler<ReviewSelectedEventArgs> LoginToNewReviewRequested;

        private BusinessLibrary.BusinessClasses.Diagram currentDiagram = null;
        private SaveFileDialog sfd;
        private dialogMetaAnalysisSetup dialogMetaAnalysisSetupControl;

        #region RadWindows
        private RadWindow AddSourceW = new RadWindow();
        private RadWindow windowCoding = new RadWindow();
        private EppiReviewer4.dialogCoding dialogCodingControl = new EppiReviewer4.dialogCoding();
        private Grid codingContent = new Grid();
        private RadWindow windowReports = new RadWindow();
        private Grid GridWindowReports = new Grid();
        private dialogReports dialogReportsControl = new dialogReports();
        private RadWindow windowDuplicates = new RadWindow();
        private Grid GridWindowDuplicates = new Grid();
        private dialogDuplicateGroups dialogDuplicatesControl = new dialogDuplicateGroups();
        private RadWindow windowMetaAnalysis = new RadWindow();
        private dialogMetaAnalysis dialogMetaAnalysisControl = new dialogMetaAnalysis();
        private CustomRadWindowControl windowMetaAnalysisTraining = new CustomRadWindowControl();
        private RadWindow windowAxialCoding = new RadWindow();
        private dialogAxialCoding dlgAxialCoding = new dialogAxialCoding();
        private RadWindow windowReportsDocuments = new RadWindow();
        private dialogReportViewer reportViewerControlDocuments = new dialogReportViewer();
        private RadWindow windowSearchDocuments = new RadWindow();
        private dialogSearch dialogSearchControlDocuments = new dialogSearch();
        private RadWindow windowItemReportWriter = new RadWindow();
        private dialogItemReportWriter dialogItemReportWriterControl = new dialogItemReportWriter();

        private RadWindow windowMagBrowser = new RadWindow();
        private dialogMagBrowser MagBrowserControl = new dialogMagBrowser();

        private RadWindow windowPleaseWait = new RadWindow();
        private BusyAnimation BusyPleaseWait = new BusyAnimation();
        
        private RadWDeleteItems windowDeleteItems = new RadWDeleteItems();
        
        private RadWDocumentCluster windowDocumentCluster = new RadWDocumentCluster();
        private RadWLoadDiagram windowLoadDiagram = new RadWLoadDiagram();
        
        private RadWCheckAssignItemsToCode windowCheckAssignItemsToCode = new RadWCheckAssignItemsToCode();
        private RadWCheckRemoveItemsFromCode windowCheckRemoveItemsFromCode = new RadWCheckRemoveItemsFromCode();
        private RadWConfirmDeleteSource windowConfirmDeleteSource = new RadWConfirmDeleteSource();
        private RadWSaveDiagram windowSave = new RadWSaveDiagram();
        private RadWRandomAllocate windowRandomAllocate = new RadWRandomAllocate();
        private RadWColumnSelect windowColumnSelect = new RadWColumnSelect();
        private RadWCreateComparison windowCreateComparison = new RadWCreateComparison();
        private RadWQuickReportComparison windowQuickReportComparison = new RadWQuickReportComparison();
        private RadWComparisonStats windowComparisonStats = new RadWComparisonStats();
        private RadWComparisonComplete windowComparisonComplete = new RadWComparisonComplete();
        private RadWReconcile windowReconcile = new RadWReconcile();
        private RadWItemsGridSelectWarning windowItemsGridSelectWarning = new RadWItemsGridSelectWarning();
        private RadWFindLikeThese windowFindLikeThese = new RadWFindLikeThese();
        private RadWShowCodingAssignment WindowShowCodingAssignment = new RadWShowCodingAssignment();
        private RadWAssignDocuments windowAssignDocuments = new RadWAssignDocuments();
        private RadWMetaAnalysisOptions windowMetaAnalysisOptions = new RadWMetaAnalysisOptions();
        private RadWTrainingResults windowTrainingResults = new RadWTrainingResults();
        private Windows.windowSearchVisualise dlgWindowVisualiseSearch;
        #endregion

        //first bunch of lines to make the read-only UI work
        public BusinessLibrary.Security.ReviewerIdentity ri;
        public bool HasWriteRights
        {
            get { return ri.HasWriteRights(); }
        }
        //end of read-only ui hack

        public homeDocuments()
        {
            InitializeComponent();
            //the following two lines sett the reviewer identity at App level, used also to bind enabled property to "HasWriteRights" (was read-only hack)
            App theApp = (Application.Current as App);
            theApp.ri = Csla.ApplicationContext.User.Identity as BusinessLibrary.Security.ReviewerIdentity;
            //ri is used on this page so we populate also locally, this is fine as it happens once per logon to review.
            ri = theApp.ri;

            Thickness thk = new Thickness( 20);

            //the follwing uses a standard RadWindow (member) to re-create the dialogCoding structure that was previously declared in XAML, see original content there (now commented out)
            windowCoding.Header="Document details";
            windowCoding.WindowStateChanged += new EventHandler(Helpers.WindowHelper.MaxOnly_WindowStateChanged);
            windowCoding.Style = Application.Current.Resources["CustomRadWindowStyle"] as Style;
            windowCoding.WindowState= WindowState.Maximized;
            windowCoding.ResizeMode= ResizeMode.NoResize;
            //not used anymore as annotations are saved transparently
            //windowCoding.Closed +=new EventHandler<WindowClosedEventArgs>(windowCoding_Closed);
            windowCoding.RestrictedAreaMargin= thk;
            windowCoding.IsRestricted = true;
            
            dialogCodingControl.CloseWindowRequest += dCoding_CloseWindowRequest;
            dialogCodingControl.RunTrainingCommandRequest += dCoding_RunTrainingCommandRequest;
            dialogCodingControl.launchMagBrowser += DialogCodingControl_launchMagBrowser;
            codingContent.Children.Add(dialogCodingControl);
            windowCoding.Content = codingContent;
            //end of dialogCoding

            //same trick for windowReports
            windowReports.Header = "Reports";
            windowReports.ResizeMode = ResizeMode.NoResize;
            windowReports.Width = 550;
            //windowReports.Opened += windowReports_Opened; http://www.telerik.com/community/forums/silverlight/general-discussions/radwindow-onopened-method.aspx
            windowReports.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
            dialogReportsControl.CloseWindowRequest +=new EventHandler(dialogReportsControl_CloseWindowRequest);
            GridWindowReports.Children.Add(dialogReportsControl);
            windowReports.Content = GridWindowReports;
            //end of windowReports

            //now with windowDuplicates
            windowDuplicates.Header = "Manage Duplicate Groups";
            windowDuplicates.ResizeMode = ResizeMode.NoResize;
            windowDuplicates.RestrictedAreaMargin = thk;
            windowDuplicates.IsRestricted = true;
            windowDuplicates.WindowStateChanged += new EventHandler(Helpers.WindowHelper.MaxOnly_WindowStateChanged);
            windowDuplicates.Style = Application.Current.Resources["CustomRadWindowStyle"] as Style;
            windowDuplicates.WindowState = WindowState.Maximized;
            windowDuplicates.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
            windowDuplicates.BorderBrush = new SolidColorBrush(Colors.Black);
            windowDuplicates.BorderThickness = new Thickness(1);
            windowDuplicates.Closed +=new EventHandler<WindowClosedEventArgs>(windowDuplicates_Closed);
            GridWindowDuplicates.Children.Add(dialogDuplicatesControl);
            windowDuplicates.Content = GridWindowDuplicates;
            
            
            //end of windowDuplicates

            //prepare windowPleaseWait
            windowPleaseWait.Header = "Please wait...";
            windowPleaseWait.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
            windowPleaseWait.CanClose = false;
            windowPleaseWait.ResizeMode = ResizeMode.NoResize;
            windowPleaseWait.IsRestricted = true;
            BusyPleaseWait.Height = 50;
            BusyPleaseWait.Width = 50;
            BusyPleaseWait.StepInterval = new TimeSpan( 100);
            windowPleaseWait.Content = BusyPleaseWait;
            //end of windowPleaseWait


            //prepare windowMetaAnalysis
            Grid gma = new Grid();
            gma.Children.Add(dialogMetaAnalysisControl);
            windowMetaAnalysis.Header = "Meta-analysis";
            windowMetaAnalysis.WindowStateChanged += new EventHandler(Helpers.WindowHelper.MaxOnly_WindowStateChanged);
            windowMetaAnalysis.Style = Application.Current.Resources["CustomRadWindowStyle"] as Style;
            windowMetaAnalysis.WindowState = WindowState.Maximized;
            windowMetaAnalysis.ResizeMode = ResizeMode.CanResize;
            windowMetaAnalysis.RestrictedAreaMargin = thk;
            windowMetaAnalysis.IsRestricted = true;
            windowMetaAnalysis.Content = gma;
            //end of windowMetaAnalysis

            //prepare windowMetaAnalysisTraining
            windowMetaAnalysisTraining.Header ="Meta-analysis training";
            windowMetaAnalysisTraining.WindowStateChanged += new EventHandler(Helpers.WindowHelper.MaxOnly_WindowStateChanged);
            windowMetaAnalysisTraining.Style = Application.Current.Resources["CustomRadWindowStyle"] as Style;
            windowMetaAnalysisTraining.WindowState = WindowState.Maximized;
            windowMetaAnalysisTraining.ResizeMode = ResizeMode.CanResize;
            windowMetaAnalysisTraining.RestrictedAreaMargin = thk;
            windowMetaAnalysisTraining.IsRestricted = true;
            Grid gmat = new Grid();
            gmat.Children.Add(new MetaAnalysisTraining());
            windowMetaAnalysisTraining.Content = gmat;
            //end of windowMetaAnalysisTraining

            //prepare windowAxialCoding
            windowAxialCoding.Header = "Code within code";
            windowAxialCoding.WindowStateChanged += new EventHandler(Helpers.WindowHelper.MaxOnly_WindowStateChanged);
            windowAxialCoding.Style = Application.Current.Resources["CustomRadWindowStyle"] as Style;
            windowAxialCoding.WindowState = WindowState.Maximized;
            windowAxialCoding.ResizeMode = ResizeMode.CanResize;
            windowAxialCoding.RestrictedAreaMargin = thk;
            windowAxialCoding.IsRestricted = true;
            Grid gac = new Grid();
            gac.Children.Add(dlgAxialCoding);
            windowAxialCoding.Content = gac;
            //end of windowAxialCoding

            //prepare windowReportsDocuments
            windowReportsDocuments.Header = "Report viewer";
            windowReportsDocuments.WindowStateChanged += new EventHandler(Helpers.WindowHelper.MaxOnly_WindowStateChanged);
            windowReportsDocuments.Style = Application.Current.Resources["CustomRadWindowStyle"] as Style;
            windowReportsDocuments.WindowState = WindowState.Maximized;
            windowReportsDocuments.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
            windowReportsDocuments.RestrictedAreaMargin = thk;
            windowReportsDocuments.CanClose = true;
            windowReportsDocuments.Width = 500;
            Grid grd = new Grid();
            grd.Children.Add(reportViewerControlDocuments);
            windowReportsDocuments.Content = grd;
            //end of windowReportsDocuments

            //prepare windowMagBrowser
            windowMagBrowser.Header = "Microsoft Academic Graph Browser. BETA version: all feedback welcome!";
            windowMagBrowser.WindowStateChanged += new EventHandler(Helpers.WindowHelper.MaxOnly_WindowStateChanged);
            windowMagBrowser.Style = Application.Current.Resources["CustomRadWindowStyle"] as Style;
            windowMagBrowser.WindowState = WindowState.Maximized;
            windowMagBrowser.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
            windowMagBrowser.RestrictedAreaMargin = thk;
            windowMagBrowser.CanClose = true;
            windowMagBrowser.Width = 500;
            Grid MagGrid = new Grid();
            MagGrid.Children.Add(MagBrowserControl);
            MagBrowserControl.ListIncludedThatNeedMatching += MagBrowserControl_ListIncludedThatNeedMatching;
            MagBrowserControl.ListExcludedThatNeedMatching += MagBrowserControl_ListExcludedThatNeedMatching;
            MagBrowserControl.ListIncludedNotMatched += MagBrowserControl_ListIncludedNotMatched;
            MagBrowserControl.ListExcludedNotMatched += MagBrowserControl_ListExcludedNotMatched;
            MagBrowserControl.ListIncludedMatched += MagBrowserControl_ListIncludedMatched;
            MagBrowserControl.ListExcludedMatched += MagBrowserControl_ListExcludedMatched;
            MagBrowserControl.ListSimulationTP += MagBrowserControl_ListSimulationTP;
            MagBrowserControl.ListSimulationFN += MagBrowserControl_ListSimulationFN;
            windowMagBrowser.Content = MagGrid;
            //end of windowMagBrowser

            //prepare windowSearchDocuments
            windowSearchDocuments.Header = "Search";
            windowSearchDocuments.WindowState = WindowState.Normal;
            windowSearchDocuments.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
            windowSearchDocuments.RestrictedAreaMargin = thk;
            windowSearchDocuments.CanClose = true;
            windowSearchDocuments.ResizeMode = ResizeMode.NoResize;
            windowSearchDocuments.Width = 500;
            Grid gsd = new Grid();
            dialogSearchControlDocuments.RefreshItemList +=new EventHandler<ItemListRefreshEventArgs>(RefreshItemList);
            dialogSearchControlDocuments.CloseWindowRequest += new EventHandler(dialogSearchControlDocuments_CloseWindowRequest);
            gsd.Children.Add(dialogSearchControlDocuments);
            windowSearchDocuments.Content = gsd;
            //end of windowSearchDocuments

            // prepare windowItemReportWriter
            windowItemReportWriter.Header = "Item coding reports";
            windowItemReportWriter.ResizeMode = ResizeMode.CanResize;
            windowItemReportWriter.Width = 450;
            windowItemReportWriter.Height = 400;
            windowItemReportWriter.RestrictedAreaMargin = thk;
            windowItemReportWriter.IsRestricted = true;
            windowItemReportWriter.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
            Grid girw = new Grid();
            dialogItemReportWriterControl.LaunchReportViewer +=new EventHandler<LaunchReportViewerEventArgs>(dialogItemReportWriterControl_LaunchReportViewer);
            girw.Children.Add(dialogItemReportWriterControl);
            windowItemReportWriter.Content = girw;
            //end of windowItemReportWriter

            // prepare AddSourceW
            AddSourceW.Header = "Add/Manage Source(s)";
            //AddSourceW.Template = Application.Current.Resources["RadWEPPITemplate"] as ControlTemplate;
            AddSourceW.RestrictedAreaMargin = new Thickness(90);
            AddSourceW.WindowStateChanged += new EventHandler(Helpers.WindowHelper.MaxOnly_WindowStateChanged);
            AddSourceW.Style = Application.Current.Resources["CustomRadWindowStyle"] as Style;
            AddSourceW.IsRestricted = true;
            AddSourceW.ResizeMode = ResizeMode.NoResize;
            AddSourceW.WindowState = WindowState.Maximized;
            //end of AddSourceW

            //now with windowReconcile
            windowReconcile.ResizeMode = ResizeMode.NoResize;
            windowReconcile.RestrictedAreaMargin = thk;
            windowReconcile.IsRestricted = true;
            windowReconcile.WindowStateChanged += new EventHandler(Helpers.WindowHelper.MaxOnly_WindowStateChanged);
            windowReconcile.Style = Application.Current.Resources["CustomRadWindowStyle"] as Style;
            windowReconcile.WindowState = WindowState.Maximized;
            windowReconcile.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
            windowReconcile.ItemsGridDataPager_PageIndexChangingEvent +=new EventHandler<PageIndexChangingEventArgs>(windowReconcile_ItemsGridDataPager_PageIndexChanging);
            windowReconcile.HyperlinkButton_Clicked += new EventHandler<RoutedEventArgs>(windowReconcile_HyperlinkButton_Click);
            //windowReconcile.BorderBrush = new SolidColorBrush(Colors.Black);
            //windowReconcile.BorderThickness = new Thickness(1);
            //end of windowReconcile

            //bits that are moved into dialogMyInfo.xaml.cs
            //cmdShowWindowCreateReview.IsEnabled = ri.DaysLeftAccount >= 0;
            //if (ri.AccountExpiration == new DateTime(3001, 1, 1))
            //    AccountExpirationTBMyInfo.Text = "Account Expiration Unkown (logged on as site admin)";
            //else AccountExpirationTBMyInfo.Text = "Your account expires on " + ri.AccountExpiration.ToShortDateString() + ".";
            //if (ri.ReviewExpiration == new DateTime(3000, 1, 1))
            //    ReviewExpirationTBMyInfo.Text = "Current Review is private (Expires with your account)";
            //else if (ri.ReviewExpiration == new DateTime(3001, 1, 1))
            //    ReviewExpirationTBMyInfo.Text = "Review Expiration Unkown (logged on as site admin)";
            //else ReviewExpirationTBMyInfo.Text = "Current (shared) review expires on " + ri.ReviewExpiration.ToShortDateString() + ".";
            //GridViewMyWorkAllocation.AddHandler(GridViewCell.MouseLeftButtonDownEvent, new MouseButtonEventHandler(MouseDownOnWorkAllocation), true);
            //end of bits in dialogMyInfo.xaml.cs

            DialogMyInfo.LoginToNewReviewRequested += new EventHandler<ReviewSelectedEventArgs>(homeDocuments_TriggerLoginToNewReviewRequested);
            DialogMyInfo.MouseDownOnMyInfoWorkAllocation += new EventHandler<MouseEventArgs>(MouseDownOnWorkAllocation);
            //DialogMyInfo.GridViewMyReviews_DataLoad +=new EventHandler<EventArgs>(DialogMyInfo_GridViewMyReviews_DataLoaded);
            cmdAssignIncluded.DataContext = this;
            bool rightClickAllowed = Application.Current.Host.Settings.Windowless;
            this.InitializePalettePresets();

            this.sfd = new SaveFileDialog()
            {
                DefaultExt = "txt",
                Filter = "Text files (*.txt)|*.txt",
                FilterIndex = 1
            };
            RandomAllocateResetVisibility();
            GridViewWorkAllocation.AddHandler(GridViewCell.MouseLeftButtonDownEvent, new MouseButtonEventHandler(MouseDownOnWorkAllocation), true);
            
            ItemAttributeCrosstabsGrid.AddHandler(GridViewCell.MouseLeftButtonDownEvent, new MouseButtonEventHandler(MouseDownOnCrosstabsGridView), true);
            CreateDialogs();
            LoadData();

            // JT - I've put this here so that it loads when the main page does - but maybe it should be put somewhere else?
            CslaDataProvider provider2 = App.Current.Resources["TrainingReviewerTermData"] as CslaDataProvider;
            if (provider2 != null)
                provider2.Refresh();
            provider2 = App.Current.Resources["TrainingListData"] as CslaDataProvider;
            if (provider2 != null)
                provider2.Refresh();

            //the Review Info object is used in screening tab, controls in there also rely on reviewSetsList data to exist, so we want the latter to be loaded before we load reviewInfo and (manually)bind to controls.
            CslaDataProvider CsetsProvider = App.Current.Resources["CodeSetsData"] as CslaDataProvider;
            if (CsetsProvider != null)
                CsetsProvider.DataChanged += CsetsProvider_DataChanged;


            //put all event hooking codes for radWindow-dervied controls in here:

            windowDocumentCluster.ClusterWhat_SelectionChanged += new EventHandler<System.Windows.Controls.SelectionChangedEventArgs>(ComboClusterWhat_SelectionChanged);
            windowDocumentCluster.cmdCluster_Clicked += new EventHandler<RoutedEventArgs>(cmdCluster_Click);
            windowLoadDiagram.cmdLoadDiagram_Clicked += new EventHandler<RoutedEventArgs>(cmdLoadDiagram_Click);
            windowCheckAssignItemsToCode.cmdCancelAssignCode_Clicked += new EventHandler<RoutedEventArgs>(cmdCancelAssignCode_Click);
            windowCheckAssignItemsToCode.cmdAssignCode_Clicked += new EventHandler<RoutedEventArgs>(cmdAssignCode_Click);
            windowCheckRemoveItemsFromCode.cmdCancelCheckRemoveFromCode_Clicked +=new EventHandler<RoutedEventArgs>(cmdCancelCheckRemoveFromCode_Click);
            windowCheckRemoveItemsFromCode.cmdRemoveItemsFromCode_Clicked +=new EventHandler<RoutedEventArgs>(cmdRemoveItemsFromCode_Click);
            windowDeleteItems.cmdDoDeleteSelectedItems_Clicked +=new EventHandler<RoutedEventArgs>(cmdDoDeleteSelectedItems_Click);
            windowDeleteItems.cmdCancelDeleteSelectedItems_Clicked += (o, e2) => { windowDeleteItems.Close(); };
            windowConfirmDeleteSource.cmdCancelDeleteSource_Clicked +=new EventHandler<RoutedEventArgs>(cmdCancelDeleteSource_Click);
            windowConfirmDeleteSource.cmdDoDeleteSource_Clicked +=new EventHandler<RoutedEventArgs>(cmdDoDeleteSource_Click);
            windowSave.cmdSaveDiagram_Clicked +=new EventHandler<RoutedEventArgs>(cmdSaveDiagram_Click);
            //windowRandomAllocate.cmdRandomAllocateSelectCode_Clicked += new EventHandler<RoutedEventArgs>(cmdRandomAllocateSelectCode_Click);
            //windowRandomAllocate.cmdRandomAllocateSelectCodeSet_Clicked +=new EventHandler<RoutedEventArgs>(cmdRandomAllocateSelectCodeSet_Click);
            //windowRandomAllocate.codesSelectControlAllocate_SelectCode_SelectionChanged += new EventHandler<RoutedEventArgs>(windowRandomAllocate_codesSelectControlAllocate_SelectCode_SelectionChanged);
            windowRandomAllocate.cmdRandomAllocationGo_Clicked +=new EventHandler<RoutedEventArgs>(cmdRandomAllocationGo_Click);
            windowRandomAllocate.ComboRandomAllocateSourceSelector_SelectionChanged +=new EventHandler<System.Windows.Controls.SelectionChangedEventArgs>(ComboRandomAllocateSourceSelector_SelectionChanged);
            windowColumnSelect.cmdCloseWindowColumnSelect_Clicked +=new EventHandler<RoutedEventArgs>(cmdCloseWindowColumnSelect_Click);
            windowCreateComparison.cmdCreateComparison_Clicked +=new EventHandler<RoutedEventArgs>(cmdCreateComparison_Click);
            windowCreateComparison.cmdSetResetAttributeComparison_Clicked +=new EventHandler<RoutedEventArgs>(cmdSetResetAttributeComparison_Click);
            windowQuickReportComparison.cmdRunQuickReportComparison_Clicked += new EventHandler<RoutedEventArgs>(cmdRunQuickReportComparison_Click);
            windowQuickReportComparison.cmdSelectCodeQuickReportComparison_Clicked += new EventHandler<RoutedEventArgs>(cmdSelectCodeQuickReportComparison_Click);
            
            windowComparisonStats.cmdCompleteComparisonAgreements1vs2_Clicked +=new EventHandler<RoutedEventArgs>(cmdCompleteComparisonAgreements1vs2_Click);
            windowComparisonStats.cmdListComparisonAgreements1vs2_Clicked +=new EventHandler<RoutedEventArgs>(cmdListComparisonAgreements1vs2_Click);
            windowComparisonStats.cmdReconcileDisagreements_Clicked += new EventHandler<RoutedEventArgs>(windowComparisonStats_cmdReconcileDisagreements_Click);
            windowComparisonComplete.cmdComparisonCompleteCancel_Clicked +=new EventHandler<RoutedEventArgs>(cmdComparisonCompleteCancel_Click);
            windowComparisonComplete.cmdComparisonCompleteGo_Clicked +=new EventHandler<RoutedEventArgs>(cmdComparisonCompleteGo_Click);
            

            windowItemsGridSelectWarning.cmdWindowItemsGridWarningClose_Clicked +=new EventHandler<RoutedEventArgs>(cmdWindowItemsGridWarningClose_Click);
            windowFindLikeThese.RadioTermine_Clicked +=new EventHandler<RoutedEventArgs>(RadioTermine_Click);
            windowFindLikeThese.cmdGetTerms_Clicked +=new EventHandler<RoutedEventArgs>(cmdGetTerms_Click);
            windowFindLikeThese.cmdTermSearch_Clicked +=new EventHandler<RoutedEventArgs>(cmdTermSearch_Click);
            windowFindLikeThese.cmdExportTermList_Clicked +=new EventHandler<RoutedEventArgs>(cmdExportTermList_Click);
            //windowFindLikeThese.cmdDeleteTerm_Clicked +=new EventHandler<RoutedEventArgs>(cmdDeleteTerm_Click);
            windowFindLikeThese.TermsGrid_SelectionChange += new EventHandler<SelectionChangeEventArgs>(TermsGrid_SelectionChanged);
            windowFindLikeThese.TermSearchComboSearchScope_SelectionChange += new EventHandler<System.Windows.Controls.SelectionChangedEventArgs>(TermSearchComboSearchScope_SelectionChanged);
            WindowShowCodingAssignment.cmdAssignWork_Clicked +=new EventHandler<RoutedEventArgs>(cmdAssignWork_Click);
            WindowShowCodingAssignment.cmdCancelAssignWork_Clicked +=new EventHandler<RoutedEventArgs>(cmdCancelAssignWork_Click);
            windowAssignDocuments.ComboSelectAssignmentMethod_SelectionChange += new EventHandler<System.Windows.Controls.SelectionChangedEventArgs>(ComboSelectAssignmentMethod_SelectionChanged);
            windowAssignDocuments.cmdAssignDocuments_Clicked +=new EventHandler<RoutedEventArgs>(cmdAssignDocuments_Click);
            windowAssignDocuments.cmdCancelAssignDocuments_Clicked +=new EventHandler<RoutedEventArgs>(cmdCancelAssignDocuments_Click);
            windowMetaAnalysisOptions.cmdWindowMetaAnalysisOptionsClose_Clicked +=new EventHandler<RoutedEventArgs>(cmdWindowMetaAnalysisOptionsClose_Click);
            windowMetaAnalysisOptions.cmdWindowMetaAnalysisOptionsGo_Clicked +=new EventHandler<RoutedEventArgs>(cmdWindowMetaAnalysisOptionsGo_Click);
            windowMetaAnalysisOptions.cbShowMetaLabels_Clicked +=new EventHandler<RoutedEventArgs>(cbShowMetaLabels_Click);
            AddSourceW.Closed +=new EventHandler<WindowClosedEventArgs>(AddSourceW_Closed);
            //end of event hooking

            //get correct list of reviewers
            CslaDataProvider provider = ((CslaDataProvider)App.Current.Resources["ReviewContactNVLData"]);
            provider.FactoryParameters.Clear();
            provider.FactoryMethod = "GetReviewContactNVL";
            provider.Refresh();

            ResetScreeningUI();
            cmdScreeningRunSimulation.Visibility = ri.IsSiteAdmin ? Visibility.Visible : System.Windows.Visibility.Collapsed;
            cmdScreeningSimulationSave.Visibility = ri.IsSiteAdmin ? Visibility.Visible : System.Windows.Visibility.Collapsed;
            if (ri.UserId == 1451 || ri.UserId == 1576 || ri.UserId == 4688 
                || ri.UserId == 6258 || ri.UserId == 6545 || ri.UserId == 11817) //Alison, Ian, Dylan,  Hollie Melton from York CRD, Joshua Pink from NICE and Albert Harkema
            {
                cmdScreeningRunSimulation.Visibility = Visibility.Visible;
                cmdScreeningSimulationSave.Visibility = Visibility.Visible;
            }

            SetMicrosoftAcademicAlertIcon();
        }

        

        private void SetMicrosoftAcademicAlertIcon()
        {
            DataPortal<MagReviewHasUpdatesToCheckCommand> dp = new DataPortal<MagReviewHasUpdatesToCheckCommand>();
            MagReviewHasUpdatesToCheckCommand check = new MagReviewHasUpdatesToCheckCommand();
            dp.ExecuteCompleted += (o, e2) =>
            {
                //BusyLoading.IsRunning = false;
                if (e2.Error != null)
                {
                    RadWindow.Alert(e2.Error.Message);
                }
                else
                {
                    MagReviewHasUpdatesToCheckCommand chk = e2.Object as MagReviewHasUpdatesToCheckCommand;
                    if (chk != null)
                    {
                        if (chk.HasUpdates)
                        {
                            ImageMAGHasUpdates.Source = new BitmapImage(new Uri("Icons/MicrosoftAcademicICOAlert.png", UriKind.Relative));
                        }
                        else
                        {
                            ImageMAGHasUpdates.Source = new BitmapImage(new Uri("Icons/MicrosoftAcademicICO.png", UriKind.Relative));
                        }
                    }
                }
            };
            //BusyLoading.IsRunning = true;
            dp.BeginExecute(check);

            CslaDataProvider provider = ((CslaDataProvider)App.Current.Resources["MagCurrentInfoData"]);
            provider.FactoryParameters.Clear();
            provider.FactoryMethod = "GetMagCurrentInfo";
            provider.Refresh();
        }

        private void CsetsProvider_DataChanged(object sender, EventArgs e)
        {
            CslaDataProvider CsetsProvider = App.Current.Resources["CodeSetsData"] as CslaDataProvider;
            if (CsetsProvider != null)
            {
                CsetsProvider.DataChanged -= CsetsProvider_DataChanged;
                CslaDataProvider RevInfoprovider = App.Current.Resources["ReviewInfoData"] as CslaDataProvider;
                if (RevInfoprovider != null)
                {

                    RevInfoprovider.DataChanged -= RevInfoprovider_DataChanged;
                    RevInfoprovider.DataChanged -= RevInfoprovider_DataChanged;
                    RevInfoprovider.DataChanged += RevInfoprovider_DataChanged;
                    RevInfoprovider.Refresh();
                }
            }
        }
        private string checker = DateTime.Now.ToString("mm-ss");
        private void RevInfoprovider_DataChanged(object sender, EventArgs e)
        {
            checker = checker.ToLower();
            CslaDataProvider RevInfoprovider = sender as CslaDataProvider;
            RevInfoprovider.DataChanged -= RevInfoprovider_DataChanged;
            RevInfoprovider.DataChanged -= RevInfoprovider_DataChanged;
            RevInfoprovider.DataChanged += RevInfoprovider_DataChanged;
            UpdateReviewInfoForScreening();
        }
        private List<ChartPalette> _palettes;
        public List<ChartPalette> Palettes
        {
            get
            {
                return this._palettes;
            }
            set
            {
                if (this._palettes != value)
                {
                    this._palettes = value;
                    //this.OnPropertyChanged("Palettes");
                }
            }
        }

        private void InitializePalettePresets()
        {
            List<ChartPalette> palettes = new List<ChartPalette>();
            palettes.Add(ChartPalettes.Arctic);
            palettes.Add(ChartPalettes.Autumn);
            palettes.Add(ChartPalettes.Cold);
            palettes.Add(ChartPalettes.Flower);
            palettes.Add(ChartPalettes.Forest);
            palettes.Add(ChartPalettes.Grayscale);
            //palettes.Add(ChartPalettes.Green);
            palettes.Add(ChartPalettes.Ground);
            palettes.Add(ChartPalettes.Lilac);
            palettes.Add(ChartPalettes.Natural);
            palettes.Add(ChartPalettes.Office2013);
            palettes.Add(ChartPalettes.Pastel);
            palettes.Add(ChartPalettes.Rainbow);
            palettes.Add(ChartPalettes.Spring);
            palettes.Add(ChartPalettes.Summer);
            palettes.Add(ChartPalettes.Warm);
            palettes.Add(ChartPalettes.Windows8);
            //palettes.Add(ChartPalettes.VisualStudio2013);

            this.Palettes = palettes;

            this.ComboBoxCrosstabsColours.ItemsSource = palettes;
            this.ComboBoxCrosstabsColours.SelectedIndex = 11;
            this.ComboBoxFrequencyColours.ItemsSource = palettes;
            this.ComboBoxFrequencyColours.SelectedIndex = 11;
        }

        //void windowDuplicates_Opened(object sender, RoutedEventArgs e)
        //{
        //    if (this.windowDuplicates.FindChildByType<dialogDuplicateGroups>().GroupDuplicatesGrid1.ItemsSource == null)
        //        this.windowDuplicates.FindChildByType<dialogDuplicateGroups>().RefreshDuplicates();
        //}

        //private void RefreshReviewList()
        //{
        //    CslaDataProvider provider = this.Resources["ReviewsData"] as CslaDataProvider;
        //    provider.FactoryParameters.Clear();
        //    provider.FactoryParameters.Add(ri.UserId);
        //    provider.FactoryMethod = "GetReviewList";
        //    provider.Refresh();
        //}

        private void CslaDataProvider_EnablerDataChanged(object sender, EventArgs e)
        {
        }

        private void CreateDialogs()
        {
            dialogMetaAnalysisSetupControl = new dialogMetaAnalysisSetup();
            dialogMetaAnalysisSetupControl.Style = Application.Current.Resources["CustomRadWindowStyle"] as Style;
            //dialogMetaAnalysisControl.WindowStateChanged += new EventHandler(Helpers.WindowHelper.MaxOnly_WindowStateChanged);
            dialogMetaAnalysisSetupControl.ReloadMetaAnalyses += new EventHandler(dialogMetaAnalysisSetupControl_ReloadMetaAnalyses);
        }

        void dialogMetaAnalysisSetupControl_ReloadMetaAnalyses(object sender, EventArgs e)
        {
            RefreshMetaAnalysisListData();            
        }

        public void LoadData()
        {
            GetItemListData();
            //CslaDataProvider provider2 = this.Resources["WriteRights"] as CslaDataProvider;
            //provider2.FactoryMethod = "newEnabler";
            //provider2.Refresh();
            //Enabler en = Enabler.newEnabler(new EventHandler < DataPortalResult < Enabler >>());
            TextBlockShowing.Text = "Showing: included documents";
            CslaDataProvider CodeSetsData = App.Current.Resources["CodeSetsData"] as CslaDataProvider;
            if (CodeSetsData != null)
            {
                CodeSetsData.DataChanged += new EventHandler(CodeSetsData_DataChanged);
            }
            LoadCodeSets();
            //refreshSources();
            ReloadSearchList();
            LoadDiagramList();
            windowFindLikeThese.TermsGrid.ItemsSource = null;
            //LoadWorkAllocation();
            //isEn.DataContext = this;
            RefreshReviewList();

        }

        void CodeSetsData_DataChanged(object sender, EventArgs e)
        {//this is used to check if we should propose to open the review configuration wizard
            CslaDataProvider CodeSetsData = App.Current.Resources["CodeSetsData"] as CslaDataProvider;
            if (CodeSetsData != null)
            {
                if (CodeSetsData.IsBusy || CodeSetsData.Error != null) return;
                CodeSetsData.DataChanged -= CodeSetsData_DataChanged;
                ReviewSetsList rSets = CodeSetsData.Data as ReviewSetsList;
                if (HasWriteRights && rSets != null && rSets.Count == 0)
                {//codesets have been loaded and this review contains none: propose to show wizard!
                    StackPanel sPanel = new StackPanel();
                    sPanel.Orientation = System.Windows.Controls.Orientation.Vertical;
                    sPanel.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
                    sPanel.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                    TextBlock title = new TextBlock();
                    title.FontWeight = FontWeights.Bold;
                    title.Text = "Do you wish to configure this review?";
                    sPanel.Children.Add(title);
                    TextBlock body = new TextBlock();
                    body.Text =   "Your review appears to be new as it doesn’t contain any codesets." + Environment.NewLine
                                + "In EPPI-Reviewer codesets (coding tools) are used to store most of the " + Environment.NewLine
                                + "reviewing data so configuring your codesets correctly is an important " + Environment.NewLine
                                + "step in setting up your review." + Environment.NewLine
                                + "Clicking \"OK\" opens a wizard to help set up your review with codesets." + Environment.NewLine
                                + "If you click on \"Cancel\" you will still be able to access this wizard at a  " + Environment.NewLine
                                + "later time by clicking on the icon in the Codes tab.";
                    sPanel.Children.Add(body);
                    RadWindow.Confirm(sPanel, OnShowWizardDialogClosed);
                }
            }
        }
        private void OnShowWizardDialogClosed(object sender, WindowClosedEventArgs e)
        {
            if (e.DialogResult == true)
            {
                codesTreeControl.ShowWizard();
            }
        }
        private void RefreshReviewList()
        {
            CslaDataProvider provider = App.Current.Resources["ReviewsData"] as CslaDataProvider;
            if (provider != null)
            {
                provider.FactoryParameters.Clear();
                provider.FactoryParameters.Add(ri.UserId);
                provider.FactoryMethod = "GetReviewList";
                //provider.DataChanged += new EventHandler(DialogMyInfo_GridViewMyReviews_DataLoaded);
                provider.Refresh();
                
                //not doing it here anymore, see initialisation instead
                //provider = App.Current.Resources["ReviewInfoData"] as CslaDataProvider;
                //if (provider != null)
                //{
                //    provider.Refresh();
                //    provider.DataChanged -= Provider_DataChanged;
                //    provider.DataChanged += Provider_DataChanged;
                //}
            }
        }
        //again, this is handled elsewhere now.
        //private void Provider_DataChanged(object sender, EventArgs e)
        //{
        //    UpdateReviewInfoForScreening();
        //}

        private void LoadWorkAllocation()
        {
            CslaDataProvider provider = ((CslaDataProvider)this.Resources["WorkAllocationListData"]);
            provider.FactoryParameters.Clear();
            provider.FactoryMethod = "GetWorkAllocationList";
            provider.Refresh();
            provider = ((CslaDataProvider)App.Current.Resources["WorkAllocationContactListData"]);
            provider.FactoryParameters.Clear();
            provider.FactoryMethod = "GetWorkAllocationContactList";
            provider.Refresh();
            provider = ((CslaDataProvider)App.Current.Resources["ReviewContactNVLData"]);
            provider.FactoryParameters.Clear();
            provider.FactoryMethod = "GetReviewContactNVL";
            provider.Refresh();
        }

        public void LoadCodeSets()
        {
            (App.Current.Resources["CodeSetsData"] as CslaDataProvider).Refresh();
            (App.Current.Resources["SetTypes"] as CslaDataProvider).Refresh();
        }

        public void ReloadSearchList()
        {
            CslaDataProvider provider = App.Current.Resources["SearchesData"] as CslaDataProvider;
            provider.FactoryParameters.Clear();
            provider.FactoryMethod = "GetSearchList";
            provider.Refresh();
        }

        private void LoadDiagramList()
        {
            CslaDataProvider provider = windowLoadDiagram.Resources["DiagramListData"] as CslaDataProvider;
            provider.FactoryMethod = "GetDiagramList";
            provider.Refresh();
        }

        private void CslaDataProvider_DataChanged(object sender, EventArgs e)
        {
            CslaDataProvider provider = ((CslaDataProvider)this.Resources["ItemListData"]);
            if (provider.Error != null)
            {
                System.Windows.Browser.HtmlPage.Window.Alert(((Csla.Xaml.CslaDataProvider)sender).Error.Message);
            }
            //else
            //{
            //    provider = ((CslaDataProvider)App.Current.Resources["ReviewsData"]);
            //    if (provider.Error != null)
            //    {
            //        System.Windows.Browser.HtmlPage.Window.Alert(((Csla.Xaml.CslaDataProvider)sender).Error.Message);
            //    }
            //}
        }

        private void cmdCluster_Click(object sender, RoutedEventArgs e)
        {
            string item_ids = "";
            ReviewSetsList rsl = (App.Current.Resources["CodeSetsData"] as CslaDataProvider).Data as ReviewSetsList;

            if (windowDocumentCluster.ComboClusterWhat.SelectedIndex == 1)
            {
                item_ids = ItemsGridSelectedItems();
            }
            DataPortal<PerformClusterCommand> dp = new DataPortal<PerformClusterCommand>();
            PerformClusterCommand command = new PerformClusterCommand(
                item_ids,
                Convert.ToInt32(windowDocumentCluster.NumericUpDownMaxHierarchyDepth.Value),
                windowDocumentCluster.NumericUpDownMinClusterSize.Value.Value,
                windowDocumentCluster.NumericUpDownMaxClusterSize.Value.Value,
                windowDocumentCluster.NumericUpDownSingleWordLabelWeight.Value.Value,
                Convert.ToInt32(windowDocumentCluster.NumericUpDownMinLabelLength.Value),
                windowDocumentCluster.ComboClusterWhat.SelectedIndex != 2 ? "" : windowDocumentCluster.codesSelectControlClusterSelect.SelectedAttributeSet().AttributeSetId.ToString(),
                windowDocumentCluster.CheckBoxClusterIncludeDocs.IsChecked == true ? true : false,
                rsl.Count);
                
            dp.ExecuteCompleted += (o, e2) =>
            {
                BusyPleaseWait.IsRunning = false;
                windowDocumentCluster.Close();
                windowPleaseWait.Close();
                (App.Current.Resources["CodeSetsData"] as CslaDataProvider).Refresh();
                DocumentActions.SelectedIndex = 0;
            };
            windowPleaseWait.ShowDialog();
            BusyPleaseWait.IsRunning = true;
            dp.BeginExecute(command);
        }

        private string GetSelectedAttributeSetIds()
        {
            string returnValue = "";
            if (codesTreeControl.SelectedAttributeSet() == null)
                return "";
            foreach (object o in codesTreeControl.SelectedAttributeSets())
            {
                if (o is AttributeSet)
                {
                    if (returnValue == "")
                    {
                        returnValue = (o as AttributeSet).AttributeSetId.ToString();
                    }
                    else
                    {
                        returnValue += "," + (o as AttributeSet).AttributeSetId.ToString();
                    }
                }
            }
            return returnValue;
        }

        private string GetSelectedAttributeNames()
        {
            string returnValue = "";
            if (codesTreeControl.SelectedAttributeSets() != null)
            {
                foreach (object o in codesTreeControl.SelectedAttributeSets())
                {
                    if (o is AttributeSet)
                    {
                        if (returnValue == "")
                        {
                            returnValue = (o as AttributeSet).AttributeName;
                        }
                        else
                        {
                            returnValue += ", " + (o as AttributeSet).AttributeName;
                        }
                    }
                }
            }
            return returnValue;
        }

        private void cmdShowIncluded_Click(object sender, RoutedEventArgs e)
        {
            TextBlockShowing.Text = "Showing: included documents";
            GetItemListData();
        }

        private void cmdShowExcluded_Click(object sender, RoutedEventArgs e)
        {
            SelectionCritieraItemList = new SelectionCriteria();
            SelectionCritieraItemList.OnlyIncluded = false;
            SelectionCritieraItemList.ShowDeleted = false;
            SelectionCritieraItemList.SourceId = 0;
            SelectionCritieraItemList.AttributeSetIdList = "";
            SelectionCritieraItemList.PageNumber = 0;
            SelectionCritieraItemList.ListType = "StandardItemList";
            TextBlockShowing.Text = "Showing: excluded documents";
            LoadItemList();
        }

        private void cmdShowDeleted_Click(object sender, RoutedEventArgs e)
        {
            SelectionCritieraItemList = new SelectionCriteria();
            SelectionCritieraItemList.OnlyIncluded = false;
            SelectionCritieraItemList.ShowDeleted = true;
            SelectionCritieraItemList.SourceId = 0;
            SelectionCritieraItemList.AttributeSetIdList = "";
            SelectionCritieraItemList.PageNumber = 0;
            SelectionCritieraItemList.ListType = "StandardItemList";
            TextBlockShowing.Text = "Showing: deleted documents";
            LoadItemList();
        }

        private void cmdEditCoding_Click(object sender, RoutedEventArgs e)
        {
            ItemList itemList = ItemsGrid.ItemsSource as ItemList;
            DataItemCollection FilteredItemList = ItemsGrid.Items;
            int currentIndex = itemList.IndexOf(((Button)(sender)).DataContext as Item);

            dialogCodingControl.BindList(itemList, currentIndex, FilteredItemList);
            
            windowCoding.ShowDialog();
        }

        void dCoding_CloseWindowRequest(object sender, EventArgs e)
        {
            windowCoding.Close();
        }

        //not used anymore as annotations are saved transparently
        //private void windowCoding_Closed(object sender, WindowClosedEventArgs e)
        //{
        //    dialogCodingControl.CheckNotesMethod(); 
        //}

        void dCoding_RunTrainingCommandRequest(object sender, EventArgs e)
        {

            TrainingList training = ((CslaDataProvider)App.Current.Resources["TrainingListData"]).Data as TrainingList;
            int maxIteration = Convert.ToInt32(training.Max(train => train.Iteration));
            Training t = training.Single(train => train.Iteration == maxIteration);
            int totalScreened = Convert.ToInt32(t.TotalN); // does this work even if the list is empty?
            int currentCount = (e as TrainingEventArgs).currentCount;

            if (totalScreened <= 1000)
            {
                if ((currentCount == 25 || currentCount == 50 || currentCount == 75 || currentCount == 100 || currentCount == 150 || currentCount == 500 ||
                    currentCount == 750))
                {
                    cmdTrainingRunTraining_Click(sender, new RoutedEventArgs());
                }
            }
            else if (totalScreened > 1000 && totalScreened < 5000)
            {
                if ((currentCount == 500 || currentCount == 750 || currentCount == 1000 || currentCount == 2000 || currentCount == 3000))
                {
                    cmdTrainingRunTraining_Click(sender, new RoutedEventArgs());
                }
            }
            else if (totalScreened >= 5000)
            {
                if ((currentCount == 1000 || currentCount == 2000 || currentCount == 2500 || currentCount == 3000 || currentCount == 3500))
                {
                    cmdTrainingRunTraining_Click(sender, new RoutedEventArgs());
                }
            }
        }

        private void cmdNewItem_Click(object sender, RoutedEventArgs e)
        {
            DataItemCollection FilteredItemList = ItemsGrid.Items;
            dialogCodingControl.BindNew(ItemsGrid.ItemsSource as ItemList, FilteredItemList);
            windowCoding.ShowDialog();
        }

        private void btAddS_Click(object sender, RoutedEventArgs e)
        {
            //SourcesPaneGrid.Children.RemoveAt(0);
            _ImportItems = new ImportItems();
            AddSourceW.Content= _ImportItems;
            //_ImportItems.txtb0.IsEnabled = true;
            
            AddSourceW.ShowDialog();
        }

        private void AddSourceW_Closed(object sender, WindowClosedEventArgs e)
        {
            _ImportItems = null;
            AddSourceW.Content = null;
            //AddSourcesGrid.Children.Clear();
            if (!HasWriteRights) return;//no need to refresh lists if user is readonly
            GetItemListData();
            refreshSources();

        }
        private void refreshSources()
        {
            CslaDataProvider provider = this.Resources["SourcesData"] as CslaDataProvider;
            provider.FactoryMethod = "GetSources";
            provider.Refresh();
        }

        private void cmdDelSource_Click(object sender, RoutedEventArgs e)
        {
            string ch = (((System.Windows.Controls.Button)sender).Content as Image).Tag.ToString();
            if ((((System.Windows.Controls.Button)sender).Content as Image).Tag.ToString() == "Delete")
            {
                windowConfirmDeleteSource.deleteSrcTxt1.Visibility = System.Windows.Visibility.Visible;
                windowConfirmDeleteSource.deleteSrcTxt2.Visibility = System.Windows.Visibility.Visible;
                windowConfirmDeleteSource.deleteSrcTxt3.Visibility = System.Windows.Visibility.Collapsed;
                windowConfirmDeleteSource.deleteSrcTxt4.Visibility = System.Windows.Visibility.Collapsed;
                windowConfirmDeleteSource.Header = "Confirm Source Delete";
            }
            else
            {
                windowConfirmDeleteSource.deleteSrcTxt1.Visibility = System.Windows.Visibility.Collapsed;
                windowConfirmDeleteSource.deleteSrcTxt2.Visibility = System.Windows.Visibility.Collapsed;
                windowConfirmDeleteSource.deleteSrcTxt3.Visibility = System.Windows.Visibility.Visible;
                windowConfirmDeleteSource.deleteSrcTxt4.Visibility = System.Windows.Visibility.Visible;
                windowConfirmDeleteSource.Header = "Confirm Source Undelete";
            }
            windowConfirmDeleteSource.cmdDoDeleteSource.Tag = ((System.Windows.Controls.Button)sender).Tag;
            windowConfirmDeleteSource.ShowDialog();
        }
        private void cmdDoDeleteSource_Click(object sender, RoutedEventArgs e)
        {

            windowConfirmDeleteSource.Close();
            int SourceID = (int)((System.Windows.Controls.Button)sender).Tag;
            DataPortal<SourceDeleteCommand> dp = new DataPortal<SourceDeleteCommand>();
            SourceDeleteCommand command = new SourceDeleteCommand(SourceID);
            dp.ExecuteCompleted += (o, e2) =>
            {
                BusyLoading.IsRunning = false;
                if (e2.Error != null)
                {
                    RadWindow.Alert(e2.Error.Message);
                }
                refreshSources();
                //LoadData();
                GetItemListData();//we just want the items list, nothing more to refresh!
            };
            BusyLoading.IsRunning = true;
            dp.BeginExecute(command);
        }
        private void cmdCancelDeleteSource_Click(object sender, RoutedEventArgs e)
        {
            windowConfirmDeleteSource.Close();
        }
        private void OnRightMouseButtonUp(object sender, Telerik.Windows.Input.MouseButtonEventArgs e)
        {
            RadTreeViewItem treeItem = null;
            Point mousePosition = e.GetPosition(null);
            foreach (UIElement item in VisualTreeHelper.FindElementsInHostCoordinates(mousePosition, this))
            {
                treeItem = item as RadTreeViewItem;
                if (treeItem != null)
                {
                    break;
                }
            }

            if (treeItem != null)
            {
                treeItem.IsSelected = true;
            }
        }

        private void CheckAssignItemsToCode()
        {
            AttributeSet attributeSet = codesTreeControl.SelectedAttributeSet();
            if (attributeSet != null)
            {
                windowCheckAssignItemsToCode.TextBlockCheckAssignCode.Text = "Assign items to: " + attributeSet.AttributeName + "?";
                windowCheckAssignItemsToCode.ShowDialog();
            }
            else
            {
                MessageBox.Show("Please click a code, not a code set");
            }
        }

        private void CheckRemoveItemsFromCode()
        {
            AttributeSet attributeSet = codesTreeControl.SelectedAttributeSet();
            if (attributeSet != null)
            {
                windowCheckRemoveItemsFromCode.TextBlockCheckRemoveCode.Text = "Remove selected items from: " + attributeSet.AttributeName + "?";
                windowCheckRemoveItemsFromCode.cmdRemoveItemsFromCode.IsEnabled = true;
                windowCheckRemoveItemsFromCode.ShowDialog();
            }
            else
            {
                MessageBox.Show("Please click a code, not a code set");
            }
        }

        private void ItemsGrid_Filtered(object sender, Telerik.Windows.Controls.GridView.GridViewFilteredEventArgs e)
        {
            ItemList itemList = ItemsGrid.ItemsSource as ItemList;
            DataItemCollection FilteredItemList = ItemsGrid.Items;

            TextBlockDocCount.Text = itemList.Count.ToString() + " documents loaded";

            if (itemList.Count > FilteredItemList.Count)
            {
                TextBlockDocCount.Text += "; filtered to list " + FilteredItemList.Count.ToString() + " documents.";
            }
            else
            {
                TextBlockDocCount.Text += ".";
            }
        }

        private void cmdGetTerms_Click(object sender, RoutedEventArgs e)
        {
            string text = "";
            string getWhat = "";
            if (windowFindLikeThese.RadioTfidf.IsChecked == true)
            {
                // INTEGRATION SERVICES 2008 VERSION
                getWhat = "TFIDF";
                if ((windowFindLikeThese.RadioCurrentDocument.IsChecked == true) )
                {
                    if (ItemsGrid.SelectedItem != null)
                    {
                        System.Collections.ObjectModel.ObservableCollection<object> Selected = ItemsGrid.SelectedItems as System.Collections.ObjectModel.ObservableCollection<object>;
                        foreach (Item currentItem in Selected)
                        {
                            if (text == "")
                            {
                                text = currentItem.ItemId.ToString();
                            }
                            else
                            {
                                text += "," + currentItem.ItemId.ToString();
                            }
                        }
                    }
                    else
                    {
                        RadWindow.Alert("Sorry, nothing is selected");
                        return;
                    }
                }
                else
                {
                    //text = ((ItemsGrid.ItemsSource as Telerik.Windows.Data.QueryableCollectionView).SourceCollection as ItemList).ListIds();
                    text = (ItemsGrid.ItemsSource as ItemList).ListIds();
                }
            }
            else
            {
                if ((windowFindLikeThese.RadioCurrentDocument.IsChecked == true) )
                {
                    if (ItemsGrid.SelectedItem != null)
                    {
                        System.Collections.ObjectModel.ObservableCollection<object> Selected = ItemsGrid.SelectedItems as System.Collections.ObjectModel.ObservableCollection<object>;
                        foreach (Item currentItem in Selected)
                        {
                            if (text == "")
                            {
                                text = (currentItem as Item).Title +
                                    Environment.NewLine + (currentItem as Item).Abstract;
                            }
                            else
                            {
                                text += Environment.NewLine + (currentItem as Item).Title +
                                    Environment.NewLine + (currentItem as Item).Abstract;
                            }
                        }
                    }
                    else
                    {
                        RadWindow.Alert("Sorry, nothing is selected");
                        return;
                    }
                }
                else
                {
                    //foreach (Item item in ItemsGrid.ItemsSource as Telerik.Windows.Data.QueryableCollectionView)
                    foreach (Item item in ItemsGrid.ItemsSource as ItemList)
                    {
                        if (text == "")
                        {
                            text = item.Title + Environment.NewLine + item.Abstract;
                        }
                        else
                        {
                            text += Environment.NewLine + item.Title + Environment.NewLine + item.Abstract;
                        }
                    }
                    
                }
                if (windowFindLikeThese.RadioTermine.IsChecked == true)
                {
                    if (System.Text.Encoding.Unicode.GetByteCount(text) > 1500000)
                    {
                        RadWindow.Alert("SORRY:" 
                            + Environment.NewLine + "you are trying to extract terms from" + Environment.NewLine
                            + "too many items, try with a shorter list");
                        return;
                    }
                    /* USING TERMINE */
                    getWhat = "Termine";
                }
                else
                {
                    if (windowFindLikeThese.RadioZemanta.IsChecked == true)
                    {
                        getWhat = "Zemanta";
                    }
                    else
                    {
                        getWhat = "Yahoo";
                    }
                }
            }
            
            
            if (text != "")
            {
                BusyPleaseWait.IsRunning = true;
                windowPleaseWait.ShowDialog();
                TermList.GetTermList(text, getWhat, TermListLoaded);
            }
            else
            {
                RadWindow.Alert("Sorry, there is no text to use.");
                return;
            }
        }

        void TermListLoaded(object sender, Csla.DataPortalResult<TermList> e)
        {
            BusyPleaseWait.IsRunning = false;
            windowPleaseWait.Close();
            if (e.Error != null)
            {
                string err = e.Error.Message.Remove(e.Error.Message.Length - 1); //remove the last ")"
                RadWindow.Alert(err.Replace("DataPortal.Fetch failed (", ""));//remove the rest of the csla-specific txt
            }
            else if (e.Object != null)
            {
                windowFindLikeThese.TermsGrid.ItemsSource = e.Object;
                windowFindLikeThese.TermsGrid.SortDescriptors.Add(new SortDescriptor()
                {
                    Member = "TermValue",
                    SortDirection = ListSortDirection.Descending
                });
                if ((windowFindLikeThese.TermsGrid.ItemsSource as TermList).Count > 0)
                {
                    windowFindLikeThese.cmdTermSearch.IsEnabled = true & HasWriteRights;
                }
                else
                {
                    windowFindLikeThese.cmdTermSearch.IsEnabled = false;
                }
            }
        }

        private void TermsGrid_SelectionChanged(object sender, SelectionChangeEventArgs e)
        {
            if (windowFindLikeThese.TermsGrid.SelectedItem != null)
                windowFindLikeThese.cmdDeleteTerm.IsEnabled = true;
            else
                windowFindLikeThese.cmdDeleteTerm.IsEnabled = false;
        }



        

        private void TreeView_SelectionChanged(object sender, EventArgs e)
        {
            if (codesTreeControl.SelectedReviewSet() != null)
            {
                TextBlockReportFrequencyCode.Text = codesTreeControl.SelectedReviewSet().SetName;
                cmdReportFrequency.IsEnabled = true;
                homeReportsControl.BindTreeViewSelectedItem(codesTreeControl.SelectedReviewSet());
            }
            else
            {
                if (codesTreeControl.SelectedAttributeSet() != null)
                {
                    TextBlockReportFrequencyCode.Text = codesTreeControl.SelectedAttributeSet().AttributeName;
                    homeReportsControl.BindTreeViewSelectedItem(codesTreeControl.SelectedAttributeSet());
                }
                else
                {
                    homeReportsControl.BindTreeViewSelectedItem(null);
                }
                cmdReportFrequency.IsEnabled = true;
            }
        }

        private void cmdTermSearch_Click(object sender, RoutedEventArgs e)
        {
            string terms = (windowFindLikeThese.TermsGrid.ItemsSource as TermList).ListInSearchFormat();
            string attributeIds = "";

            if (windowFindLikeThese.TermSearchComboSearchScope.SelectedIndex != 0)
            {
                if (windowFindLikeThese.codesSelectControlTermSearch.SelectedAttributeSet() != null)
                {
                    attributeIds = windowFindLikeThese.codesSelectControlTermSearch.SelectedAttributeSet().AttributeSetId.ToString();
                }
                else
                {
                    MessageBox.Show("Please select a code to filter by");
                    return;
                }
            }

            DataPortal<SearchWeightedTermsCommand> dp = new DataPortal<SearchWeightedTermsCommand>();
            SearchWeightedTermsCommand command = new SearchWeightedTermsCommand(
                "Term search",
                terms,
                attributeIds,
                windowFindLikeThese.TermSearchComboSearchScope.SelectedIndex == 0 ? "NONE" : windowFindLikeThese.TermSearchComboSearchScope.SelectedIndex == 2 ? "EXCLUDE" : "INCLUDE",
                windowFindLikeThese.RadioTermSearchIncluded.IsChecked.Value == true ? true : false);
            dp.ExecuteCompleted += (o, e2) =>
            {
                BusyPleaseWait.IsRunning = false;
                windowPleaseWait.Close();
                if (e2.Error != null)
                {
                    MessageBox.Show(e2.Error.Message);
                }
                else
                {
                    ReloadSearchList();
                    SelectionCriteria selcon = new SelectionCriteria();
                    selcon.SearchId = e2.Object.SearchId;
                    selcon.Description = "Find similar documents";
                    selcon.ListType = "GetItemSearchList";
                    ItemListRefreshEventArgs ilrea = new ItemListRefreshEventArgs(selcon);
                    RefreshItemList(null, ilrea);
                    windowFindLikeThese.Close();
                }
            };
            windowPleaseWait.ShowDialog();
            BusyPleaseWait.IsRunning = true;
            dp.BeginExecute(command);
        }

        private void cmdListSearch_Click(object sender, RoutedEventArgs e)
        {
            Search search = (sender as Button).DataContext as Search;

            SelectionCritieraItemList = new SelectionCriteria();
            SelectionCritieraItemList.ListType = "GetItemSearchList";
            SelectionCritieraItemList.SearchId = search.SearchId;
            SelectionCritieraItemList.ShowScoreColumn = true;

            TextBlockShowing.Text = "Showing: " + search.Title;
            DocumentListPane.SelectedIndex = 0;
            LoadItemList();
        }

        private void cmdDeleteSearch_Click(object sender, RoutedEventArgs e)
        {
            if (SearchGrid.SelectedItems.Count > 0 && MessageBox.Show("Delete all selected searches?", "Delete searches?", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                string SearchList = "";
                foreach (Search search in SearchGrid.SelectedItems)
                {
                    if (SearchList == "")
                    {
                        SearchList = search.SearchId.ToString();
                    }
                    else
                    {
                        SearchList += "," + search.SearchId.ToString();
                    }
                }

                DataPortal<SearchDeleteCommand> dp = new DataPortal<SearchDeleteCommand>();
                SearchDeleteCommand command = new SearchDeleteCommand(SearchList);
                dp.ExecuteCompleted += (o, e2) =>
                {
                    if (e2.Error != null)
                    {
                        MessageBox.Show(e2.Error.Message);
                    }
                    ReloadSearchList();
                };
                dp.BeginExecute(command);
            }
        }

        private void btRefreshS_Click(object sender, RoutedEventArgs e)
        {
            refreshSources();
        }

        private void cmdSaveDiagramShowDialog_Click(object sender, RoutedEventArgs e)
        {
            if (currentDiagram != null)
            {
                 windowSave.TextBoxDiagramName.Text = currentDiagram.Name;
            }
            windowSave.ShowDialog();
        }

        private void theDiagram_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                MindFusion.Diagramming.Silverlight.DiagramLink dl = theDiagram.ActiveItem as DiagramLink;
                if (dl != null)
                    theDiagram.Links.Remove(dl);

                DiagramNode dn = theDiagram.ActiveItem as DiagramNode;
                if (dn != null)
                    theDiagram.Nodes.Remove(dn);
            }

        }

        private void cmdSaveDiagram_Click(object sender, RoutedEventArgs e)
        {
            if (windowSave.TextBoxDiagramName.Text == "")
            {
                MessageBox.Show("Please enter a name for the diagram");
                return;
            }

            string detail = theDiagram.SaveToString(MindFusion.Diagramming.Silverlight.SaveToStringFormat.Xml);
            if ((currentDiagram == null) || (windowSave.TextBoxDiagramName.Text != currentDiagram.Name))
            {
                BusinessLibrary.BusinessClasses.Diagram newDiagram = new BusinessLibrary.BusinessClasses.Diagram();
                newDiagram.Name = windowSave.TextBoxDiagramName.Text;
                newDiagram.Detail = detail;
                newDiagram.Saved += (o, e2) =>
                {
                    if (e2.Error != null)
                        MessageBox.Show(e2.Error.Message);
                    windowSave.Close();
                    LoadDiagramList();
                };
                newDiagram.BeginSave();
            }
            else
            {
                currentDiagram.Detail = detail;
                currentDiagram.Saved += (o, e2) =>
                {
                    if (e2.Error != null)
                        MessageBox.Show(e2.Error.Message);
                    windowSave.Close();
                    LoadDiagramList();
                };
                currentDiagram.BeginSave();
            }
        }

        private void cmdLoadDiagramShowDialog_Click(object sender, RoutedEventArgs e)
        {
            windowLoadDiagram.ShowDialog();
        }

        private void cmdLoadDiagram_Click(object sender, RoutedEventArgs e)
        {
            currentDiagram = (sender as Button).DataContext as BusinessLibrary.BusinessClasses.Diagram;
            theDiagram.LoadFromString(currentDiagram.Detail);
            theDiagram.ResizeToFitItems(3);
            windowLoadDiagram.Close();
        }

        private void cmdNewDiagram_Click(object sender, RoutedEventArgs e)
        {
            currentDiagram = null;
            theDiagram.ClearAll();
        }

        private void cmdDeleteSelectedDiagramNode_Click(object sender, RoutedEventArgs e)
        {
            DiagramNode dn = theDiagram.ActiveItem as DiagramNode;
            if (dn != null)
            {
                theDiagram.Nodes.Remove(dn);
            }
            else
            {
                MindFusion.Diagramming.Silverlight.DiagramLink dl = theDiagram.ActiveItem as DiagramLink;
                if (dl != null)
                    theDiagram.Links.Remove(dl);
            }
        }

        private void cmdShowText_Click(object sender, RoutedEventArgs e)
        {
            AttributeSet attributeSet = GetFromSelectedDiagramNode();
            if (attributeSet != null)
            {
                ShowCodedText(attributeSet);
            }
            else
            {
                MessageBox.Show("Please select a code on the diagram first");
            }
        }

        private AttributeSet GetFromSelectedDiagramNode()
        {
            AttributeSet returnValue = null;
            DiagramNode dn = theDiagram.ActiveItem as DiagramNode;
            if ((dn != null) && (dn.Tag != null))
            {
                Int64 AttributeId = Convert.ToInt64(dn.Tag.ToString());
                returnValue = codesTreeControl.GetAttributeSet(AttributeId);
            }
            return returnValue;
        }

        private void ShowCodedText(AttributeSet attributeSet)
        {
            if (attributeSet != null)
            {
                DataPortal<ReadOnlyAttributeTextAllItemsList> dp = new DataPortal<ReadOnlyAttributeTextAllItemsList>();
                dp.FetchCompleted += (o, e2) =>
                {
                    BusyLoading.IsRunning = false;
                    if (e2.Error != null)
                    {
                        MessageBox.Show(e2.Error.Message);
                    }
                    else
                    {
                        ReadOnlyAttributeTextAllItemsList list = e2.Object;
                        string report = "<html><body><p><h3>Items coded with: <i>" + attributeSet.AttributeName + "</i></h3>";
                        Int64 CurrentItemID = 0;
                        foreach (ReadOnlyAttributeTextAllItems textItem in list)
                        {
                            if (textItem.ItemId != CurrentItemID)
                            {
                                //lblReport.Text += "<br />";
                                report += "<h3>" + textItem.ItemTitle + "</h3>";
                                CurrentItemID = textItem.ItemId;
                                report += "<i><span style=\"color: #4544AF\">Characters: " + textItem.TextFrom.ToString() +
                                " to " + textItem.TextTo.ToString() + "</span></i>";
                            }
                            else
                            {
                                report += "<br /><i><span style=\"color: #4544AF\">Characters: " + textItem.TextFrom.ToString() +
                                " to " + textItem.TextTo.ToString() + "</span></i>";
                            }

                            report += "<br />" + textItem.CodedText.Replace("\n", "<br />") + "<br />";
                        }
                        report += "</p>";
                        System.Windows.Browser.HtmlPage.Window.Invoke("ShowPopup", report);
                    }
                };
                BusyLoading.IsRunning = true;
                dp.BeginFetch(new SingleCriteria<ReadOnlyAttributeTextAllItemsList, Int64>(attributeSet.AttributeSetId));
            }
        }

        private void cmdAddNumbersToDiagram_Click(object sender, RoutedEventArgs e)
        {
            string attributes = "";
            foreach (DiagramNode dn in theDiagram.Nodes)
            {
                if ((dn != null) && (dn.Tag != null))
                {
                    if (attributes == "")
                    {
                        attributes = dn.Tag.ToString();
                    }
                    else
                    {
                        attributes += "," + dn.Tag.ToString();
                    }
                }
            }
            if (attributes != "")
            {
                DataPortal<ItemAttributeCountsCommand> dp = new DataPortal<ItemAttributeCountsCommand>();
                ItemAttributeCountsCommand command = new ItemAttributeCountsCommand(attributes);
                dp.ExecuteCompleted += (o, e2) =>
                {
                    BusyPleaseWait.IsRunning = false;
                    windowPleaseWait.Close();
                    if (e2.Error != null)
                    {
                        MessageBox.Show(e2.Error.Message);
                    }
                    else
                    {
                        if (e2.Object.AttributeCounts != "")
                        {
                            string[] attributeCounts = e2.Object.AttributeCounts.Split('¬');
                            for (int i = 0; i < attributeCounts.Length; i++)
                            {
                                string [] attributeC = attributeCounts[i].Split(',');
                                foreach (DiagramNode dn in theDiagram.Nodes)
                                {
                                    if ((dn != null) && (dn.Tag != null) && (dn.Tag.ToString() == attributeC[0].ToString()) && (dn is ShapeNode))
                                    {
                                        string caption = (dn as ShapeNode).Text;
                                        int index = caption.IndexOf(" (");
                                        if (index != -1)
                                        {
                                            caption = caption.Substring(0, index) + " (" + attributeC[1] + ")";
                                        }
                                        else
                                        {
                                            caption += " (" + attributeC[1] + ")";
                                        }
                                        (dn as ShapeNode).Text = caption;
                                    }
                                }
                            }
                        }
                    }
                };
                windowPleaseWait.ShowDialog();
                BusyPleaseWait.IsRunning = true;
                dp.BeginExecute(command);
            }
        }

        private void cmdExportDiagram_Click(object sender, RoutedEventArgs e)
        {
            MemoryStream ms = new MemoryStream();
            theDiagram.SaveToPng(ms);
            BitmapImage bm = new BitmapImage();
            bm.SetSource(ms);

            byte[] store = ms.ToArray();

            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "PNG Files|*.png" + "|All Files|*.*";
            bool? dialogResult = dialog.ShowDialog();

            if (dialogResult == true)
            {
                using (Stream fs = (Stream)dialog.OpenFile())
                {
                    fs.Write(store, 0, store.Length);
                    fs.Close();
                }
            }

        }

        private void grView0_RowLoaded(object sender, Telerik.Windows.Controls.GridView.RowLoadedEventArgs e)
        {
            //ReadOnlySource DE = e.DataElement as ReadOnlySource;
            //if (DE != null)
            //{
            //    if (DE.IsDeleted) e.Row.Background = new SolidColorBrush(Colors.LightGray);
            //}
            /*if (e.Row.Cells.Count < 4) return;
            if (e.Row.Cells[4].Content is bool && Convert.ToBoolean(e.Row.Cells[4].Content))
            {
                e.Row.Background = new SolidColorBrush(Colors.LightGray);
            }
            object newO = sender;
            */
        }

        private void cmdCombineSearches_Click(object sender, RoutedEventArgs e)
        {
            string CombineType = "";
            string Searches = "";
            string Title = "";
            bool included = true;
            if (RadioCombineAND.IsChecked.Value)
            {
                CombineType = "AND";
            }
            else
            {
                if (RadioCombineOR.IsChecked.Value)
                {
                    CombineType = "OR";
                }
                else
                {
                    if (RadioCombineNOTIncluded.IsChecked.Value)
                    {
                        CombineType = "NOT";
                        included = true;
                    }
                    else
                    {
                        CombineType = "NOT";
                        included = false;
                    }
                }
            }
            foreach (Search search in SearchGrid.SelectedItems)
            {
                if (Searches == "")
                {
                    Searches = search.SearchId.ToString();
                    Title = search.SearchNo.ToString();
                }
                else
                {
                    Searches += "," + search.SearchId.ToString();
                    Title += " " + CombineType + " " + search.SearchNo.ToString();
                }
            }
            if (CombineType == "NOT")
            {
                Title = "NOT " + Title;
            }

            if (Searches != "")
            {
                DataPortal<SearchCombineCommand> dp = new DataPortal<SearchCombineCommand>();
                SearchCombineCommand command = new SearchCombineCommand(
                    Title,
                    Searches,
                    CombineType,
                    included);
                dp.ExecuteCompleted += (o, e2) =>
                {
                    BusyPleaseWait.IsRunning = false;
                    windowPleaseWait.Close();
                    if (e2.Error != null)
                    {
                        MessageBox.Show(e2.Error.Message);
                    }
                    else
                    {
                        ReloadSearchList();
                    }
                };
                windowPleaseWait.ShowDialog();
                BusyPleaseWait.IsRunning = true;
                dp.BeginExecute(command);
            }
            else
            {
                MessageBox.Show("You need to select at least one search first");
            }
        }

        private void ColorDiagramBackground_SelectedColorChanged(object sender, EventArgs e)
        {
            if (theDiagram == null || theDiagram.ActiveItem == null) return;
            DiagramNode dn = theDiagram.ActiveItem as DiagramNode;
            if (dn != null)
            {
                dn.Brush = new SolidColorBrush(ColorDiagramBackground.SelectedColor);
            }
            else
            {
                MindFusion.Diagramming.Silverlight.DiagramLink dl = theDiagram.ActiveItem as DiagramLink;
                if (dl != null)
                    dl.Pen = new Pen(new SolidColorBrush(ColorDiagramBackground.SelectedColor));
            }
        }

        private void ColorDiagramText_SelectedColorChanged(object sender, EventArgs e)
        {
            ShapeNode dn = theDiagram.ActiveItem as ShapeNode;
            if (dn != null)
            {
                dn.TextBrush = new SolidColorBrush(ColorDiagramText.SelectedColor);
            }
            else
            {
                MindFusion.Diagramming.Silverlight.DiagramLink dl = theDiagram.ActiveItem as DiagramLink;
                if (dl != null)
                    dl.Foreground = new SolidColorBrush(ColorDiagramText.SelectedColor);
            }
        }

        private void ColorDiagramText_Click(object sender, EventArgs e)
        {
            ShapeNode dn = theDiagram.ActiveItem as ShapeNode;
            if (dn != null)
            {
                dn.TextBrush = new SolidColorBrush(ColorDiagramText.SelectedColor);
            }
            else
            {
                MindFusion.Diagramming.Silverlight.DiagramLink dl = theDiagram.ActiveItem as DiagramLink;
                if (dl != null)
                    dl.Foreground = new SolidColorBrush(ColorDiagramText.SelectedColor);
            }
        }

        private void ColorDiagramBackground_Click(object sender, EventArgs e)
        {
            DiagramNode dn = theDiagram.ActiveItem as DiagramNode;
            if (dn != null)
            {
                dn.Brush = new SolidColorBrush(ColorDiagramBackground.SelectedColor);
            }
            else
            {
                MindFusion.Diagramming.Silverlight.DiagramLink dl = theDiagram.ActiveItem as DiagramLink;
                if (dl != null)
                    dl.Pen = new Pen(new SolidColorBrush(ColorDiagramBackground.SelectedColor));
            }
        }
        
        private void ComboDiagramShapeSelector_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            ShapeNode dn = theDiagram.ActiveItem as ShapeNode;
            if ((dn != null) && (ComboDiagramShapeSelector.SelectedIndex != -1))
            {
                dn.Shape = MindFusion.Diagramming.Silverlight.Shape.Shapes[(ComboDiagramShapeSelector.SelectedItem as ComboBoxItem).Content.ToString()];
            }
            ComboDiagramShapeSelector.SelectedIndex = -1;
        }

        private string ItemsGridSelectedItems()
        {
            string returnList = "";
            foreach (Item item in ItemsGrid.SelectedItems)
            {
                if (returnList == "")
                {
                    returnList = item.ItemId.ToString();
                }
                else
                {
                    returnList += "," + item.ItemId.ToString();
                }
            }
            return returnList;
        }

        private void cmdAssignIncluded_Click(object sender, RoutedEventArgs e)
        {
            ComboSelectAssignmentMethod_SelectionChanged(sender, null);
            windowAssignDocuments.ShowDialog();
        }

        private void cmdListSourceItems_Click(object sender, RoutedEventArgs e)
        {
            SelectionCritieraItemList = new SelectionCriteria();
            SelectionCritieraItemList.OnlyIncluded = false;// included ignore for sources
            SelectionCritieraItemList.ShowDeleted = true; // deleted ignore for sources
            SelectionCritieraItemList.AttributeSetIdList = "";
            ReadOnlySource ros = ((sender as Button).DataContext as ReadOnlySource);
            SelectionCritieraItemList.SourceId = ros.Source_ID;
            SelectionCritieraItemList.ListType = "StandardItemList";
            TextBlockShowing.Text = "Showing: " + 
                (ros.Source_Name == "NN_SOURCELESS_NN" ? "Manually Created (Sourceless) Items." : ros.Source_Name);
            LoadItemList();
        }

        private void SearchGrid_CellEditEnded(object sender, GridViewCellEditEndedEventArgs e)
        {
            /*
            Search s = e.Cell.DataContext as Search;
            if (s != null)
            {
                s.Saved += (o, e2) =>
                {
                    if (e2.Error != null)
                        MessageBox.Show(e2.Error.Message);
                    //windowCodeProperties.Close();
                };
                s.ApplyEdit();
                s.BeginSave(true);
            }
            */
        }

        private void cmdAssignCode_Click(object sender, RoutedEventArgs e)
        {
            AttributeSet attributeSet = codesTreeControl.SelectedAttributeSet();
            if (attributeSet != null)
            {
                string items = ItemsGridSelectedItems();
                if (items != "")
                {
                    DataPortal<ItemAttributeBulkSaveCommand> dp = new DataPortal<ItemAttributeBulkSaveCommand>();
                    ItemAttributeBulkSaveCommand command = new ItemAttributeBulkSaveCommand("Insert",
                        attributeSet.AttributeId, attributeSet.SetId, items, "");
                    dp.ExecuteCompleted += (o, e2) =>
                    {
                        if (e2.Error != null)
                            MessageBox.Show(e2.Error.Message);
                        windowCheckAssignItemsToCode.Close();
                        windowCheckAssignItemsToCode.cmdAssignCode.IsEnabled = true;
                    };
                    windowCheckAssignItemsToCode.cmdAssignCode.IsEnabled = false;
                    dp.BeginExecute(command);
                }
                else
                {
                    MessageBox.Show("No items selected");
                    windowCheckAssignItemsToCode.Close();
                    windowCheckAssignItemsToCode.cmdAssignCode.IsEnabled = true;
                }
            }
            else
            {
                MessageBox.Show("No code selected");
            }
        }

        private void cmdCancelAssignCode_Click(object sender, RoutedEventArgs e)
        {
            windowCheckAssignItemsToCode.Close();
        }

        private void cmdRemoveItemsFromCode_Click(object sender, RoutedEventArgs e)
        {
            AttributeSet attributeSet = codesTreeControl.SelectedAttributeSet();
            if (attributeSet != null)
            {
                string items = ItemsGridSelectedItems();
                if (items != "")
                {
                    DataPortal<ItemAttributeBulkDeleteCommand> dp = new DataPortal<ItemAttributeBulkDeleteCommand>();
                    ItemAttributeBulkDeleteCommand command = new ItemAttributeBulkDeleteCommand(
                        attributeSet.AttributeId,
                        items,
                        attributeSet.SetId,
                        "");
                    dp.ExecuteCompleted += (o, e2) =>
                    {
                        if (e2.Error != null)
                            MessageBox.Show(e2.Error.Message);
                        windowCheckRemoveItemsFromCode.Close();
                    };
                    windowCheckRemoveItemsFromCode.cmdRemoveItemsFromCode.IsEnabled = false;
                    dp.BeginExecute(command);
                }
                else
                {
                    MessageBox.Show("No items selected");
                    windowCheckRemoveItemsFromCode.Close();
                }
            }
            else
            {
                MessageBox.Show("No code selected");
            }
        }

        private void cmdCancelCheckRemoveFromCode_Click(object sender, RoutedEventArgs e)
        {
            windowCheckRemoveItemsFromCode.Close();
        }

        private void cmdSetFilterFrequencies_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button).Tag.ToString() == "Set")
            {
                AttributeSet attributeSet = codesTreeControl.SelectedAttributeSet();
                if (attributeSet != null)
                {
                    TextBlockReportFrequenciesFilter.DataContext = attributeSet;
                    cmdRemoveFilterFrequencies.Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    TextBlockReportFrequenciesFilter.DataContext = null;
                    cmdRemoveFilterFrequencies.Visibility = System.Windows.Visibility.Collapsed;
                }
            }
            else
            {
                TextBlockReportFrequenciesFilter.DataContext = null;
                cmdRemoveFilterFrequencies.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void cmdReportFrequency_Click(object sender, RoutedEventArgs e)
        {
            AttributeSet attributeSet = codesTreeControl.SelectedAttributeSet();
            if (attributeSet != null)
            {
                windowPleaseWait.ShowDialog();
                BusyPleaseWait.IsRunning = true;
                ReadOnlyItemAttributeChildFrequencyList.GetItemAttributeChildFrequencyList(attributeSet.SetId, 
                    attributeSet.AttributeId,
                    RadioButtonFrequenciesIncluded.IsChecked == true ? true : false,
                    TextBlockReportFrequenciesFilter.DataContext == null ? -1 : (TextBlockReportFrequenciesFilter.DataContext as AttributeSet).AttributeId,
                    ItemAttributeChildFrequencyListLoaded);
            }
            else
            {
                ReviewSet rs = codesTreeControl.SelectedReviewSet();
                if (rs != null)
                {
                    windowPleaseWait.ShowDialog();
                    BusyPleaseWait.IsRunning = true;
                    ReadOnlyItemAttributeChildFrequencyList.GetItemAttributeChildFrequencyList(rs.SetId,
                        0,
                        RadioButtonFrequenciesIncluded.IsChecked == true ? true : false,
                        TextBlockReportFrequenciesFilter.DataContext == null ? -1 : (TextBlockReportFrequenciesFilter.DataContext as AttributeSet).AttributeId,
                        ItemAttributeChildFrequencyListLoaded);
                }
            }
        }

        void ItemAttributeChildFrequencyListLoaded(object sender, Csla.DataPortalResult<ReadOnlyItemAttributeChildFrequencyList> e)
        {
            BusyPleaseWait.IsRunning = false;
            windowPleaseWait.Close();
            if (e.Object != null)
            {
                ItemAttributeFrequenciesGrid.ItemsSource = e.Object;
                AttributeSet attributeSet = codesTreeControl.SelectedAttributeSet();
                if (attributeSet != null)
                    ItemAttributeFrequenciesGrid.Columns[0].Header = "Code: " + attributeSet.AttributeName;
                FrequencyChart();
            }
        }

        private void FrequencyChart()
        {
            if (ItemAttributeFrequenciesGrid.ItemsSource != null)
            {
                ReadOnlyItemAttributeChildFrequencyList theList = ItemAttributeFrequenciesGrid.ItemsSource as ReadOnlyItemAttributeChildFrequencyList;
                Telerik.Windows.Controls.ChartView.PieSeries PieS = new Telerik.Windows.Controls.ChartView.PieSeries();
                Telerik.Windows.Controls.ChartView.BarSeries BarS = new Telerik.Windows.Controls.ChartView.BarSeries();
                foreach (ReadOnlyItemAttributeChildFrequency currentItem in theList)
                {
                    if (currentItem.Attribute != "None of the codes above" || (CheckBoxFrequenciesIncludeNoneOfTheAbove.IsChecked == true && currentItem.Attribute == "None of the codes above"))
                    {
                        Telerik.Charting.PieDataPoint pdp = new Telerik.Charting.PieDataPoint();
                        pdp.Value = currentItem.ItemCount;
                        pdp.Label = currentItem.Attribute + " (" + currentItem.ItemCount.ToString() + ")";
                        PieS.DataPoints.Add(pdp);

                        Telerik.Charting.CategoricalDataPoint cdp = new Telerik.Charting.CategoricalDataPoint();
                        cdp.Category = currentItem.Attribute;
                        cdp.Value = currentItem.ItemCount;
                        BarS.DataPoints.Add(cdp);
                        BarS.ShowLabels = true;
                    }
                }
                FrequenciesPieChart.Series.Clear();
                FrequenciesPieChart.Series.Add(PieS);
                FrequenciesBarChart.Series.Clear();
                FrequenciesBarChart.Series.Add(BarS);
            }
        }

        private void cmdReportFrequencies_Click(object sender, RoutedEventArgs e)
        {
            ReadOnlyItemAttributeChildFrequency row = (sender as Button).DataContext as ReadOnlyItemAttributeChildFrequency;
            AttributeSet attributeFilter = codesTreeControl.GetAttributeSet( row.FilterAttributeId);// TextBlockReportFrequenciesFilter.DataContext as AttributeSet;
            if (row.Attribute == "None of the codes above" && row.AttributeSetId < 0)//the special "none of the codes above" row
            {
                TextBlockShowing.Text = "Showing: Frequencies '" + row.Attribute + "'" + (row.FilterAttributeId > 0 ? " (filtered by: " + attributeFilter.AttributeName + ")." : ".");
                SelectionCritieraItemList = new SelectionCriteria();
                SelectionCritieraItemList.ListType = "FrequencyNoneOfTheAbove";
                SelectionCritieraItemList.OnlyIncluded = row.IsIncluded;
                SelectionCritieraItemList.ShowDeleted = false;
                SelectionCritieraItemList.SetId = row.SetId;
                SelectionCritieraItemList.XAxisAttributeId = -row.AttributeId;
                SelectionCritieraItemList.PageNumber = 0;
                SelectionCritieraItemList.FilterAttributeId = row.FilterAttributeId;
                
            }
            else
            {
                if (attributeFilter == null)
                {
                    TextBlockShowing.Text = "Showing: " + row.Attribute + ".";
                    SelectionCritieraItemList = new SelectionCriteria();
                    SelectionCritieraItemList.ListType = "StandardItemList";
                    SelectionCritieraItemList.OnlyIncluded = RadioButtonFrequenciesIncluded.IsChecked == true ? true : false;
                    SelectionCritieraItemList.ShowDeleted = false;
                    SelectionCritieraItemList.SourceId = 0;
                    SelectionCritieraItemList.PageNumber = 0;
                    SelectionCritieraItemList.AttributeSetIdList = row.AttributeSetId.ToString();
                }
                else
                {
                    TextBlockShowing.Text = "Showing: " + row.Attribute + ".";
                    SelectionCritieraItemList = new SelectionCriteria();
                    SelectionCritieraItemList.ListType = "FrequencyWithFilter";
                    SelectionCritieraItemList.OnlyIncluded = row.IsIncluded;
                    SelectionCritieraItemList.ShowDeleted = false;
                    SelectionCritieraItemList.SetId = row.SetId;
                    SelectionCritieraItemList.XAxisAttributeId = row.AttributeId;
                    SelectionCritieraItemList.PageNumber = 0;
                    SelectionCritieraItemList.FilterAttributeId = row.FilterAttributeId;
                    TextBlockShowing.Text = "Showing: " + row.Attribute + "; filtered by: " + attributeFilter.AttributeName + ".";
                }
            }
            LoadItemList();
            DocumentListPane.SelectedIndex = 0;
        }

        private void cmdSetXAxis_Click(object sender, RoutedEventArgs e)
        {
            AttributeSet attributeSet = codesTreeControl.SelectedAttributeSet();
            if (attributeSet != null)
            {
                TextBlockReportCrosstabsXAxis.DataContext = attributeSet;
                TextBlockReportCrosstabsXAxis.Text = attributeSet.AttributeName;
            }
            else
            {
                ReviewSet rs = codesTreeControl.SelectedReviewSet();
                if (rs != null)
                {
                    TextBlockReportCrosstabsXAxis.DataContext = rs;
                    TextBlockReportCrosstabsXAxis.Text = rs.SetName;
                }
                else
                {
                    TextBlockReportCrosstabsXAxis.DataContext = null;
                }
            }
            if ((TextBlockReportCrosstabsYAxis.DataContext != null) && (TextBlockReportCrosstabsXAxis.DataContext != null))
            {
                cmdReportCrosstabs.IsEnabled = true;
            }
            else
            {
                cmdReportCrosstabs.IsEnabled = false;
            }
        }

        private void cmdSetYAxis_Click(object sender, RoutedEventArgs e)
        {
            AttributeSet attributeSet = codesTreeControl.SelectedAttributeSet();
            if (attributeSet != null)
            {
                TextBlockReportCrosstabsYAxis.DataContext = attributeSet;
                TextBlockReportCrosstabsYAxis.Text = attributeSet.AttributeName;
            }
            else
            {
                ReviewSet rs = codesTreeControl.SelectedReviewSet();
                if (rs != null)
                {
                    TextBlockReportCrosstabsYAxis.DataContext = rs;
                    TextBlockReportCrosstabsYAxis.Text = rs.SetName;
                }
                else
                {
                    TextBlockReportCrosstabsYAxis.DataContext = null;
                }
            }
            if ((TextBlockReportCrosstabsYAxis.DataContext != null) && (TextBlockReportCrosstabsXAxis.DataContext != null))
            {
                cmdReportCrosstabs.IsEnabled = true;
            }
            else
            {
                cmdReportCrosstabs.IsEnabled = false;
            }
        }

        private void cmdSetFilter_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button).Tag.ToString() == "Set")
            {
                AttributeSet attributeSet = codesTreeControl.SelectedAttributeSet();
                if (attributeSet != null)
                {
                    TextBlockReportCrosstabsFilter.DataContext = attributeSet;
                    cmdRemoveFilter.Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    TextBlockReportCrosstabsFilter.DataContext = null;
                    cmdRemoveFilter.Visibility = System.Windows.Visibility.Collapsed;
                }
            }
            else
            {
                TextBlockReportCrosstabsFilter.DataContext = null;
                cmdRemoveFilter.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        AttributeSetList XAxisAttributes = null;
        AttributeSetList YAxisAttributes = null;
        AttributeSet asFilter = null;

        private void cmdReportCrosstabs_Click(object sender, RoutedEventArgs e)
        {
            if ((TextBlockReportCrosstabsYAxis.DataContext != null) && (TextBlockReportCrosstabsXAxis.DataContext != null))
            {
                Int64 AttributeIdXaxis = 0;
                int SetIdXaxis = 0;
                Int64 AttributeIdYaxis = 0;
                int SetIdYaxis = 0;
                Int64 AttributeIdFilter = 0;
                int SetIdFilter = 0;
                int NXaxis = 0;
                

                AttributeSet asX = TextBlockReportCrosstabsXAxis.DataContext as AttributeSet;
                if (asX != null)
                {
                    AttributeIdXaxis = asX.AttributeId;
                    SetIdXaxis = asX.SetId;
                    NXaxis = asX.Attributes.Count;
                    XAxisAttributes = asX.Attributes;
                }
                else
                {
                    ReviewSet rsX = TextBlockReportCrosstabsXAxis.DataContext as ReviewSet;
                    if (rsX != null)
                    {
                        AttributeIdXaxis = 0;
                        SetIdXaxis = rsX.SetId;
                        NXaxis = rsX.Attributes.Count;
                        XAxisAttributes = rsX.Attributes;
                    }
                    else
                    {
                        MessageBox.Show("Error: no X-axis set");
                        return;
                    }
                }

                AttributeSet asY = TextBlockReportCrosstabsYAxis.DataContext as AttributeSet;
                if (asY != null)
                {
                    AttributeIdYaxis = asY.AttributeId;
                    SetIdYaxis = asY.SetId;
                    YAxisAttributes = asY.Attributes;
                }
                else
                {
                    ReviewSet rsY = TextBlockReportCrosstabsYAxis.DataContext as ReviewSet;
                    if (rsY != null)
                    {
                        AttributeIdYaxis = 0;
                        SetIdYaxis = rsY.SetId;
                        YAxisAttributes = rsY.Attributes;
                    }
                    else
                    {
                        MessageBox.Show("Error: no Y-axis set");
                        return;
                    }
                }

                asFilter = TextBlockReportCrosstabsFilter.DataContext as AttributeSet;
                if (asFilter != null)
                {
                    AttributeIdFilter = asFilter.AttributeId;
                    SetIdFilter = asFilter.SetId;
                }

                CslaDataProvider provider = this.Resources["ItemAttributeCrosstabListData"] as CslaDataProvider;
                provider.FactoryParameters.Clear();
                provider.FactoryParameters.Add(AttributeIdXaxis);
                provider.FactoryParameters.Add(SetIdXaxis);
                provider.FactoryParameters.Add(AttributeIdYaxis);
                provider.FactoryParameters.Add(SetIdYaxis);
                provider.FactoryParameters.Add(AttributeIdFilter);
                provider.FactoryParameters.Add(SetIdFilter);
                provider.FactoryParameters.Add(NXaxis);
                provider.FactoryMethod = "GetItemAttributeChildCrosstabList";
                provider.Refresh();

                for (int i = 0; i < Math.Min(NXaxis, 50); i++)
                {
                    ItemAttributeCrosstabsGrid.Columns[i + 1].IsVisible = true;
                    ItemAttributeCrosstabsGrid.Columns[i + 1].Header = XAxisAttributes[i].AttributeName;
                    ItemAttributeCrosstabsGrid.Columns[i + 1].UniqueName = XAxisAttributes[i].AttributeId.ToString();
                }
                for (int i = NXaxis; i < 50; i++)
                {
                    ItemAttributeCrosstabsGrid.Columns[i + 1].IsVisible = false;
                }
            }
            else
            {
                MessageBox.Show("Please select codes (or code sets) for the X and Y axes");
            }
        }

        private void CrosstabsChart()
        {
            if (ItemAttributeCrosstabsGrid.ItemsSource != null)
            {
                // get all the business objects referenced
                CslaDataProvider provider = this.Resources["ItemAttributeCrosstabListData"] as CslaDataProvider;
                ReadOnlyItemAttributeCrosstabList crossData = provider.Data as ReadOnlyItemAttributeCrosstabList;
                CrosstabsBarChart.Series.Clear();

                Int64 AttributeIdXaxis = 0;
                int SetIdXaxis = 0;
                int NXaxis = 0;
                AttributeSet asX = TextBlockReportCrosstabsXAxis.DataContext as AttributeSet;
                if (asX != null)
                {
                    AttributeIdXaxis = asX.AttributeId;
                    SetIdXaxis = asX.SetId;
                    NXaxis = asX.Attributes.Count;
                    XAxisAttributes = asX.Attributes;
                }
                else
                {
                    ReviewSet rsX = TextBlockReportCrosstabsXAxis.DataContext as ReviewSet;
                    if (rsX != null)
                    {
                        AttributeIdXaxis = 0;
                        SetIdXaxis = rsX.SetId;
                        NXaxis = rsX.Attributes.Count;
                        XAxisAttributes = rsX.Attributes;
                    }
                    else
                    {
                        MessageBox.Show("Error: no X-axis set");
                        return;
                    }
                }

                // read the data into a list
                CrosstabsChartLegend.Items.Clear();
                List<StackModel> cData = new List<StackModel>();
                foreach (ReadOnlyItemAttributeCrosstab iac in crossData)
                {
                    var stack = new StackModel()
                    {
                        PlotInfos = new List<PlotInfo>()
                    };
                    for (int k = 0; k < NXaxis; k++)
                    {
                        var plotInfo = new PlotInfo()
                        {
                            Value = Convert.ToInt32(iac.GetType().GetProperty("Field" + (k+1).ToString()).GetValue(iac, null).ToString()),
                            Category = XAxisAttributes[k].AttributeName,
                        };
                        stack.PlotInfos.Add(plotInfo);
                    }
                    cData.Add(stack);
                    
                }

                int i = 0;
                // read the list into the chart
                foreach (StackModel model in cData)
                {
                    var series = new BarSeries();
                    SeriesLegendSettings ls = new SeriesLegendSettings();
                    ls.Title = crossData[i].AttributeName;
                    i++;
                    series.LegendSettings = ls;

                    //var combineModeBinding = new Binding("SelectedItem.Content") { Source = this.comboBox };
                    //series.SetBinding(BarSeries.CombineModeProperty,  combineModeBinding);

                    if (RadioButtonCrosstabsShowBar.IsChecked == true)
                    {
                        series.CombineMode = ChartSeriesCombineMode.Cluster;
                    }
                    else
                        if (RadioButtonCrosstabsShowBarStacked.IsChecked ==  true)
                        {
                            series.CombineMode = ChartSeriesCombineMode.Stack;
                        }
                        else
                            series.CombineMode = ChartSeriesCombineMode.Stack100;

                    foreach (PlotInfo info in model.PlotInfos)
                    {
                        var dataPoint = new CategoricalDataPoint();
                        dataPoint.Category = info.Category;
                        dataPoint.Value = info.Value;

                        series.DataPoints.Add(dataPoint);
                    }
                    CrosstabsBarChart.Series.Add(series);
                    cmdExportCrosstabsChart.IsEnabled = true;
                }

                // Different Graph now - circularrelationships graph
                
                i = 0;
                List<CircularRelationshipGraph.Data.Node> nodes = new List<CircularRelationshipGraph.Data.Node>();
                foreach (StackModel model in cData)
                {
                    CircularRelationshipGraph.Data.Node currentNode = new CircularRelationshipGraph.Data.Node();
                    currentNode.Relationships = new List<CircularRelationshipGraph.Data.INodeRelationship>();
                    currentNode.Name = Truncate(crossData[i].AttributeName, 20);
                    i++;
                    foreach (PlotInfo info in model.PlotInfos)
                    {
                        CircularRelationshipGraph.Data.NodeRelationship nr = new CircularRelationshipGraph.Data.NodeRelationship();
                        nr.To = Truncate(info.Category.ToString(), 20);
                        nr.Strength = Convert.ToInt32(info.Value);
                        currentNode.Relationships.Add(nr);
                    }
                    nodes.Add(currentNode);
                }
                i = 0;
                foreach (AttributeSet asl in XAxisAttributes)
                {
                    CircularRelationshipGraph.Data.Node currentNode = new CircularRelationshipGraph.Data.Node();
                    currentNode.Relationships = new List<CircularRelationshipGraph.Data.INodeRelationship>();
                    currentNode.Name = Truncate(asl.AttributeName, 20);
                    foreach (ReadOnlyItemAttributeCrosstab iac in crossData)
                    {
                        CircularRelationshipGraph.Data.NodeRelationship nr = new CircularRelationshipGraph.Data.NodeRelationship();
                        nr.To = Truncate(iac.AttributeName, 20);
                        nr.Strength = Convert.ToInt32(iac.GetType().GetProperty("Field" + (i + 1).ToString()).GetValue(iac, null).ToString());
                        currentNode.Relationships.Add(nr);
                    }
                    i++;
                    nodes.Add(currentNode);
                }
                // now put the count values in

                foreach (CircularRelationshipGraph.Data.Node curNode in nodes)
                {
                    double curCount = 0;
                    for (i = 0; i < curNode.Relationships.Count; i++)
                    {
                        curCount += curNode.Relationships[i].Strength;
                    }
                    curNode.Count = curCount;
                }

                CircularGraphCrosstabs.Data = null;
                try
                {
                    CircularGraphCrosstabs.Data = new CircularRelationshipGraph.Data.NodeList(nodes);
                    //CircularGraphCrosstabs.SortOrderProvider = new MinimisedConnectionLengthSort(true); <-- clusters results, but display not too reliable
                }
                catch
                {
                    // just don't display the circlegraph - don't want the other (bar) graphs failing too
                }
            }
        }

        public string Truncate(string value, int maxLength) // at the moment the truncation is slightly arbitrary, but we could give users the option
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }

        public class StackModel
        {
            public List<PlotInfo> PlotInfos { get; set; }
        }

        public class PlotInfo
        {
            public double Value { get; set; }
            public string Category { get; set; }
        }

        private void MouseDownOnCrosstabsGridView(object sender, MouseEventArgs args)
        {
            if (((UIElement)args.OriginalSource).ParentOfType<GridViewCell>() != null)
            {
                ReadOnlyItemAttributeCrosstab ct = ((UIElement)args.OriginalSource).ParentOfType<GridViewCell>().DataContext as ReadOnlyItemAttributeCrosstab;
                GridViewDataColumn dc = ((UIElement)args.OriginalSource).ParentOfType<GridViewCell>().DataColumn as GridViewDataColumn;
                int i = ItemAttributeCrosstabsGrid.Columns.IndexOf(dc);
                if (i < 1)
                {
                    return;
                }
                AttributeSet asX = XAxisAttributes[i - 1];
                SelectionCritieraItemList = new SelectionCriteria();
                SelectionCritieraItemList.ListType = "CrosstabsList";
                SelectionCritieraItemList.XAxisSetId = asX.SetId;
                SelectionCritieraItemList.YAxisSetId = YAxisAttributes[0].SetId;
                SelectionCritieraItemList.FilterSetId = asFilter == null ? 0 : asFilter.SetId;
                SelectionCritieraItemList.XAxisAttributeId = asX.AttributeId;
                SelectionCritieraItemList.YAxisAttributeId = ct.AttributeId;
                SelectionCritieraItemList.FilterAttributeId = asFilter == null ? 0 : asFilter.AttributeId;
                TextBlockShowing.Text = "Crosstabulating '" + asX.AttributeName + "' against '" + ct.AttributeName + "'";
                if (asFilter != null)
                {
                    TextBlockShowing.Text += ". Filtered by '" + asFilter.AttributeName + "'";
                }
                DocumentListPane.SelectedIndex = 0;
                LoadItemList();
            }
        }

        private void cmdExportToRIS_Click(object sender, RoutedEventArgs e)
        {
            cmdExportToRIS.IsEnabled = false;
            BusyExporting.IsRunning = true;
            bool? proceed = this.sfd.ShowDialog();
            if (proceed == true)
            {
                
                cmdExportToRIS.InvalidateArrange();
                string exRis = "";
                //ItemList itLi = (this.Resources["ItemListData"] as CslaDataProvider).Data as ItemList;
                System.Text.StringBuilder sb = new System.Text.StringBuilder();

                foreach (Item item in ItemsGrid.SelectedItems)
                {
                    sb.Append(ExportItemToRISString.ExportItemToRIS(item));
                }
                exRis = sb.ToString();
                byte[] buff = System.Text.Encoding.UTF8.GetBytes(exRis);

                try
                {
                    using (Stream stream = sfd.OpenFile())
                    {
                        stream.Write(buff, 0, buff.Length);
                        stream.Close();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("SORRY, could not save exported file: \n" + ex.Message);
                }
            }
            BusyExporting.IsRunning = false;
            cmdExportToRIS.IsEnabled = true;
        }
       
        private void CslaDataProvider_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (cmdExportToRIS != null)
                cmdExportToRIS.IsEnabled = !(bool)(sender as CslaDataProvider).IsBusy;
            UpdateDocCount();
        }

        private void cmdMetaAnalysisTraining_Click(object sender, RoutedEventArgs e)
        {
            windowMetaAnalysisTraining.ShowDialog();
        }

        private void RefreshMetaAnalysisListData()
        {
            CslaDataProvider MetaAnalysisListData = ((CslaDataProvider)this.Resources["MetaAnalysisListData"]);
            MetaAnalysisListData.FactoryMethod = "GetMetaAnalysisList";
            MetaAnalysisListData.Refresh();
        }

        private void CslaDataProviderMetaAnalysisListData_DataChanged(object sender, EventArgs e)
        {
            CslaDataProvider provider = ((CslaDataProvider)this.Resources["MetaAnalysisListData"]);
            if (provider.Error != null)
                System.Windows.Browser.HtmlPage.Window.Alert(((Csla.Xaml.CslaDataProvider)sender).Error.Message);
        }

        private void cmdMetaNewMetaAnalysis_Click(object sender, RoutedEventArgs e)
        {
            dialogMetaAnalysisSetupControl.ShowWindow(new MetaAnalysis());
        }

        private void cmdDeleteMetaAnalysis_Click(object sender, RoutedEventArgs e)
        {
            MetaAnalysis _currentSelectedMetaAnalysis = ((Button)(sender)).DataContext as MetaAnalysis;
            BusyPleaseWait.IsRunning = true;
            windowPleaseWait.ShowDialog();

            if (_currentSelectedMetaAnalysis != null && MessageBox.Show("Are you sure you want to delete this meta-analysis?",
                "Confirm delete", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                _currentSelectedMetaAnalysis.Delete();
                _currentSelectedMetaAnalysis.Saved += (o, e2) =>
                {
                    BusyPleaseWait.IsRunning = false;
                    windowPleaseWait.Close();

                    if (e2.Error != null)
                        MessageBox.Show(e2.Error.Message);
                    RefreshMetaAnalysisListData();
                };
                _currentSelectedMetaAnalysis.BeginSave(true);
            }
        }

        private void cmdEditMetaAnalysis_Click(object sender, RoutedEventArgs e)
        {
            MetaAnalysis _currentSelectedMetaAnalysis = ((Button)(sender)).DataContext as MetaAnalysis;
            dialogMetaAnalysisSetupControl.ShowWindow(((Button)(sender)).DataContext as MetaAnalysis);
        }

        private void cmdRunMetaAnalysis_Click(object sender, RoutedEventArgs e)
        {
            MetaAnalysis ma = (sender as Button).DataContext as MetaAnalysis;
            windowMetaAnalysisOptions.GridMAOptions.DataContext = ma;
           
            DataPortal<OutcomeList> dp = new DataPortal<OutcomeList>();
            dp.FetchCompleted += (o, e2) =>
            {
                radBusyEditMAIndicator.IsBusy = false;
                if (e2.Error != null)
                {
                    MessageBox.Show(e2.Error.Message);
                }
                else
                {
                    ma.Outcomes = e2.Object as OutcomeList;
                    windowMetaAnalysisOptions.ShowDialog();
                }
            };
            radBusyEditMAIndicator.IsBusy = true;
            dp.BeginFetch(new BusinessLibrary.BusinessClasses.OutcomeList.OutcomeListSelectionCriteria(typeof(OutcomeList), ma.SetId, ma.AttributeIdIntervention,
                ma.AttributeIdControl, ma.AttributeIdOutcome, 0, ma.MetaAnalysisId, ma.AttributeIdQuestion, ma.AttributeIdAnswer));
        }

        //private void CslaDataProvider_ReviewContactNVLDataDataChanged(object sender, EventArgs e)
        //{
        //    CslaDataProvider provider = ((CslaDataProvider)App.Current.Resources["ReviewContactNVLData"]);
        //    if (provider.Error != null)
        //        System.Windows.Browser.HtmlPage.Window.Alert(((Csla.Xaml.CslaDataProvider)sender).Error.Message);
        //}

        private void CslaDataProvider_WorkAllocationListDataDataChanged(object sender, EventArgs e)
        {
            CslaDataProvider provider = ((CslaDataProvider)this.Resources["WorkAllocationListData"]);
            if (provider.Error != null)
                MessageBox.Show(((Csla.Xaml.CslaDataProvider)sender).Error.Message);
        }

        private void cmdAssignWork_Click(object sender, RoutedEventArgs e)
        {
            AttributeSet attributeSet = WindowShowCodingAssignment.codesSelectControlAssignWork.SelectedAttributeSet();
            ReviewSet rs = WindowShowCodingAssignment.dialogAssignWorkComboSelectCodeSet.SelectedItem as ReviewSet;
            ReviewContactNVL.NameValuePair Contact = WindowShowCodingAssignment.ComboReviewContactsAssignWork.SelectedItem as ReviewContactNVL.NameValuePair;

            if ((attributeSet == null) || (rs == null) || (Contact == null))
            {
                MessageBox.Show("Please select the code, the code set and the person concerned");
                return;
            }
            WorkAllocation wa = new WorkAllocation();
            wa.AttributeId = attributeSet.AttributeId;
            wa.SetId = rs.SetId;
            wa.ContactId = Contact.Key;
            wa.Saved += (o, e2) =>
            {
                if (e2.Error != null)
                    MessageBox.Show(e2.Error.Message);
                LoadWorkAllocation();
                WindowShowCodingAssignment.cmdAssignWork.IsEnabled = true;
                WindowShowCodingAssignment.cmdCancelAssignWork.IsEnabled = true;
                WindowShowCodingAssignment.Close();
            };
            WindowShowCodingAssignment.cmdAssignWork.IsEnabled = false;
            WindowShowCodingAssignment.cmdCancelAssignWork.IsEnabled = false;
            wa.BeginSave();
        }

        private void cmdDeleteWorkAllocation_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to delete this work allocation?", "Delete work allocation?", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                WorkAllocation wa = (sender as Button).DataContext as WorkAllocation;
                if (wa != null)
                {
                    wa.Delete();
                    wa.Saved += (o, e2) =>
                    {
                        if (e2.Error != null)
                            MessageBox.Show(e2.Error.Message);
                        LoadWorkAllocation();
                    };
                    wa.BeginSave();
                }
            }
        }

        //private void CslaDataProvider_WorkAllocationContactListDataDataChanged(object sender, EventArgs e)
        //{
        //    CslaDataProvider provider = ((CslaDataProvider)this.Resources["WorkAllocationContactListData"]);
        //    if (provider.Error != null)
        //        MessageBox.Show(((Csla.Xaml.CslaDataProvider)sender).Error.Message);
        //}

        private void cmdRandomAllocate_Click(object sender, RoutedEventArgs e)
        {
            windowRandomAllocate.ShowDialog();
        }

        private void ComboRandomAllocateSourceSelector_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (windowRandomAllocate.cmdRandomAllocationGo != null)
            {
                RandomAllocateResetVisibility();
                switch (windowRandomAllocate.ComboRandomAllocateSourceSelector.SelectedIndex)
                {
                    case 0:
                        break;
                    case 1:
                        //windowRandomAllocate.TextBlockRandomAllocateSelectCodeSet.Visibility = Visibility.Visible;
                        windowRandomAllocate.codesSelectControlAllocateFilterCodeSet.Visibility = Visibility.Visible;//codesSelectControlAllocateSelectCodeSet
                        windowRandomAllocate.GridRandomAllocate.RowDefinitions[2].Height = new GridLength(35);
                        break;
                    case 2:
                       // windowRandomAllocate.TextBlockRandomAllocateSelectCodeSet.Visibility = Visibility.Visible;
                        windowRandomAllocate.codesSelectControlAllocateFilterCodeSet.Visibility = Visibility.Visible;//codesSelectControlAllocateSelectCodeSet
                        windowRandomAllocate.GridRandomAllocate.RowDefinitions[2].Height = new GridLength(35);
                        break;
                    case 3:
                        //windowRandomAllocate.TextBlockRandomAllocateSelectCode.Visibility = Visibility.Visible;
                        windowRandomAllocate.codesSelectControlAllocateFilterCode.Visibility = Visibility.Visible;//codesSelectControlAllocateSelectCode
                        windowRandomAllocate.GridRandomAllocate.RowDefinitions[1].Height = new GridLength(35);
                        break;
                    case 4:
                        //windowRandomAllocate.TextBlockRandomAllocateSelectCode.Visibility = Visibility.Visible;
                        windowRandomAllocate.codesSelectControlAllocateFilterCode.Visibility = Visibility.Visible;//codesSelectControlAllocateSelectCode
                        windowRandomAllocate.GridRandomAllocate.RowDefinitions[1].Height = new GridLength(35);
                        break;
                    default:
                        break;
                }
            }
        }

        private void RandomAllocateResetVisibility()
        {
            windowRandomAllocate.GridRandomAllocate.RowDefinitions[1].Height = new GridLength(0);
            windowRandomAllocate.GridRandomAllocate.RowDefinitions[2].Height = new GridLength(0);
            //windowRandomAllocate.TextBlockRandomAllocateSelectCode.Visibility = Visibility.Collapsed;
            windowRandomAllocate.codesSelectControlAllocateFilterCode.Visibility = Visibility.Collapsed;
            //windowRandomAllocate.TextBlockRandomAllocateSelectCodeSet.Visibility = Visibility.Collapsed;
            windowRandomAllocate.codesSelectControlAllocateFilterCodeSet.Visibility = Visibility.Collapsed;

        }

        //private void cmdRandomAllocateSelectCode_Click(object sender, RoutedEventArgs e)
        //{
        //    AttributeSet attributeSet = codesTreeControl.SelectedAttributeSet();
        //    if (attributeSet != null)
        //    {
        //        windowRandomAllocate.TextBlockRandomAllocateSelectCode.DataContext = attributeSet;
        //    }
        //}

        //private void cmdRandomAllocateSelectCodeSet_Click(object sender, RoutedEventArgs e)
        //{
        //    ReviewSet reviewSet = codesTreeControl.SelectedReviewSet();
        //    if (reviewSet != null)
        //    {
        //        windowRandomAllocate.TextBlockRandomAllocateSelectCodeSet.DataContext = reviewSet;
        //    }
        //}

        //private void cmdRandomAllocateSelectCreateBelow_Click(object sender, RoutedEventArgs e)
        //{
        //    ReviewSet reviewSet = codesTreeControl.SelectedReviewSet();
        //    if (reviewSet != null)
        //    {
        //        windowRandomAllocate.TextBlockRandomCreateBelow.Text = reviewSet.SetName;
        //        windowRandomAllocate.cmdRandomAllocateSelectCreateBelow.DataContext = reviewSet;
        //    }
        //    else
        //    {
        //        AttributeSet attributeSet = codesTreeControl.SelectedAttributeSet();
        //        if (attributeSet != null)
        //        {
        //            windowRandomAllocate.TextBlockRandomCreateBelow.Text = attributeSet.AttributeName;
        //            windowRandomAllocate.cmdRandomAllocateSelectCreateBelow.DataContext = attributeSet;
        //        }
        //    }
        //}
        
        private void cmdRandomAllocationGo_Click(object sender, RoutedEventArgs e)
        {
            //if (windowRandomAllocate.cmdRandomAllocateSelectCreateBelow.DataContext == null)
            //{
            //    MessageBox.Show("Please select a code set or code for the new codes to be created");
            //    return;
            //}
            AttributeSet DestAttSet = windowRandomAllocate.codesSelectControlAllocate.TreeViewSelectCode.SelectedItem as AttributeSet;
            ReviewSet DestRevSet = windowRandomAllocate.codesSelectControlAllocate.TreeViewSelectCode.SelectedItem as ReviewSet;
            AttributeSet FiltAttSet = windowRandomAllocate.codesSelectControlAllocateFilterCode.TreeViewSelectCode.SelectedItem as AttributeSet;
            ReviewSet FiltRevSet = windowRandomAllocate.codesSelectControlAllocateFilterCodeSet.TreeViewSelectCode.SelectedItem as ReviewSet;
            if (DestAttSet == null && DestRevSet == null)
            {
               RadWindow.Alert("Please select a CodeSet or a Code" + Environment.NewLine + "to contain the new codes to be created");
                return;
            }
            string FilterType = "";
            Int64 attributeIdFilter = 0;
            int setIdFilter = 0;
            Int64 attributeId = 0;
            int setId = 0;
            int howMany = Convert.ToInt32(windowRandomAllocate.numericRandomCreate.Value);

            switch (windowRandomAllocate.ComboRandomAllocateSourceSelector.SelectedIndex)
            {
                case 0:
                    FilterType = "No code / code set filter";
                    break;
                case 1:
                    if (FiltRevSet == null)
                    {
                        MessageBox.Show("Please select a code to filter your documents by");
                        return;
                    }
                    setIdFilter = FiltRevSet.SetId;
                    FilterType = "All without any codes from this set";
                    break;
                case 2:
                    if (FiltRevSet == null)
                    {
                        MessageBox.Show("Please select a code to filter your documents by");
                        return;
                    }
                    setIdFilter = FiltRevSet.SetId;
                    FilterType = "All with any codes from this set";
                    break;
                case 3://codesSelectControlAllocateSelectCode
                    if (FiltAttSet == null)
                    {
                        MessageBox.Show("Please select a code to filter your documents by");
                        return;
                    }
                    attributeIdFilter = FiltAttSet.AttributeId;
                    FilterType = "All with this code";
                    break;
                case 4://codesSelectControlAllocateSelectCode
                    if (FiltAttSet == null)
                    {
                        MessageBox.Show("Please select a code to filter your documents by");
                        return;
                    }
                    attributeIdFilter = FiltAttSet.AttributeId;
                    FilterType = "All without this code";
                    break;
                default:
                    break;
            }

            if (DestAttSet != null)
            {
                attributeId = DestAttSet.AttributeId;
                setId = DestAttSet.SetId;
            }
            else
            {
                setId = DestRevSet.SetId;
                attributeId = 0;
            }
            DataPortal<PerformRandomAllocateCommand> dp = new DataPortal<PerformRandomAllocateCommand>();
            PerformRandomAllocateCommand command = new PerformRandomAllocateCommand(
                FilterType,
                attributeIdFilter,
                setIdFilter,
                attributeId,
                setId,
                howMany,
                Convert.ToInt32(windowRandomAllocate.numericRandomSample.Value),
                windowRandomAllocate.RadioButtonRandomSampleIncluded.IsChecked == true);

            dp.ExecuteCompleted += (o, e2) =>
            {
                BusyPleaseWait.IsRunning = false;
                windowPleaseWait.Close();
                windowRandomAllocate.Close();
                (App.Current.Resources["CodeSetsData"] as CslaDataProvider).Refresh();
                DocumentActions.SelectedIndex = 0;
            };
            windowPleaseWait.ShowDialog();
            BusyPleaseWait.IsRunning = true;
            dp.BeginExecute(command);
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e) // warning - also handles searchGrid
        {
            var checkBox = (CheckBox)sender;
            var parentGrid = checkBox.ParentOfType<GridViewDataControl>();

            if (checkBox.IsChecked.Value)
                parentGrid.SelectAll();
            else
                parentGrid.UnselectAll();
            CslaDataProvider provider = ((CslaDataProvider)this.Resources["ItemListData"]);
            if ((provider != null) && (provider.Data != null) && (checkBox.Tag.ToString() == "ItemsGrid"))
            {
                if (((provider.Data as ItemList).PageCount > 1) && (windowItemsGridSelectWarning.CheckBoxItemsGridWarning.IsChecked == false))
                {
                    windowItemsGridSelectWarning.ShowDialog();
                }
            }
        }

        private void cmdOpenClusterDocumentWindow_Click(object sender, RoutedEventArgs e)
        {
            UpdateGridWindowDocumentCluster();
            windowDocumentCluster.ShowDialog();
        }
        void homeDocuments_TriggerLoginToNewReviewRequested(object sender, ReviewSelectedEventArgs e)
        {
            LoginToNewReviewRequested.Invoke(this, e);
        }
        //moved into dialogMyInfo.xaml.cs
        //private void cmdSelectReview_Click(object sender, RoutedEventArgs e)
        //{
        //    GridViewMyReviews.IsEnabled = false;
        //    int reviewId = ((sender as Button).DataContext as ReadOnlyReview).ReviewId;
        //    string reviewName = ((sender as Button).DataContext as ReadOnlyReview).ReviewName;
        //    //App a = App.Current as EppiReviewer4.App;
        //    //Page pg = (Page)((Grid)((Grid)this.Parent).Parent).Parent;
        //    //pg.LoadCodes();
        //    if (LoginToNewReviewRequested != null)
        //        LoginToNewReviewRequested.Invoke(this, new ReviewSelectedEventArgs(reviewId, reviewName));
        //    //DocumentListPane.SelectedIndex = 0;
        //}

        //moved into dialogMyInfo.xaml.cs
        //private void btCodeOnly_Click(object sender, RoutedEventArgs e)
        //{
        //    GridViewMyReviews.IsEnabled = false;
        //    int reviewId = ((sender as Button).DataContext as ReadOnlyReview).ReviewId;
        //    string reviewName = ((sender as Button).DataContext as ReadOnlyReview).ReviewName;
        //    //App a = App.Current as EppiReviewer4.App;
        //    //Page pg = (Page)((Grid)((Grid)this.Parent).Parent).Parent;
        //    //pg.LoadCodes();
        //    if (LoginToNewReviewRequested != null)
        //        LoginToNewReviewRequested.Invoke(this, new ReviewSelectedEventArgs(reviewId, reviewName, "Coding only"));
        //}
        //moved into dialogMyInfo.xaml.cs
        //private void cmdShowWindowCreateReview_Click(object sender, RoutedEventArgs e)
        //{
        //    Review review = new Review();
        //    review.ContactId = ri.UserId;
        //    GridCreateReview.DataContext = review;
        //    windowCreateReview.ShowDialog();
        //}

        //private void cmdCreateNewReview_Click(object sender, RoutedEventArgs e)
        //{
        //    //Review review = GridCreateReview.DataContext as Review;
        //    Review review = new Review(TextBoxNewReviewName.Text, ri.UserId);
        //    if ((TextBoxNewReviewName.Text != "") && (review != null))
        //    {
        //        review.Saved += (o, e2) =>
        //        {
        //            if (e2.NewObject != null)
        //            {
        //                Review rv = e2.NewObject as Review;
        //                RefreshReviewList();
        //                // (App.Current.Resources["CodeSetsData"] as CslaDataProvider).Refresh(); is this needed?? removed for now.
        //            }
        //            else
        //                if (e2.Error != null)
        //                    MessageBox.Show(e2.Error.Message);
        //            BusyCreateNewReview.IsRunning = false;
        //            cmdCreateNewReview.IsEnabled = true;
        //            windowCreateReview.Close();
        //        };
        //        BusyCreateNewReview.IsRunning = true;
        //        cmdCreateNewReview.IsEnabled = false;
        //        review.BeginSave();
        //    }
        //    else
        //    {
        //        MessageBox.Show("Please enter a name for your review");
        //    }
        //}

        private void cmdDeleteSelectedItems_Click(object sender, RoutedEventArgs e)
        {
            if (ItemsGrid.SelectedItems.Count > 0)
            {
                windowDeleteItems.ShowDialog();
            }
        }

        private void cmdDoDeleteSelectedItems_Click(object sender, RoutedEventArgs e)
        {
            string Delete = (sender as Button).Tag.ToString();
            DataPortal<ItemDeleteUndeleteCommand> dp = new DataPortal<ItemDeleteUndeleteCommand>();
            ItemDeleteUndeleteCommand command = new ItemDeleteUndeleteCommand(
                Delete == "true" ? true : false,
                ItemsGridSelectedItems());
            dp.ExecuteCompleted += (o, e2) =>
            {
                if (e2.Error != null)
                {
                    MessageBox.Show(e2.Error.Message);
                }
                BusyPleaseWait.IsRunning = false;
                windowPleaseWait.Close();
                foreach (Item item in ItemsGrid.SelectedItems)
                {
                    if (!(item.IsItemDeleted == true && item.IsIncluded == true))
                    {
                        if (Delete == "true")
                        {
                            item.IsItemDeleted = true;
                            item.IsIncluded = false;
                        }
                        else
                        {
                            item.IsItemDeleted = false;
                            item.IsIncluded = true;
                        }
                    }
                }
                windowDeleteItems.Close();
            };
            windowPleaseWait.ShowDialog();
            BusyPleaseWait.IsRunning = true;
            dp.BeginExecute(command);
        }

        private void DocumentListPane_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (DocumentListPane.SelectedPane.Name == "PaneActiveScreening")
            {//this block needs to be on top, before setting menus
                CslaDataProvider provider = this.Resources["TrainingScreeningCriteriaData"] as CslaDataProvider;
                if (provider != null)
                    provider.Refresh();

                CslaDataProvider provider3 = App.Current.Resources["TrainingListData"] as CslaDataProvider;
                if (provider3 != null)
                    provider3.Refresh();
                PaneActiveScreening_Activated(sender, e);
            }
            else
            {
                RadSelectionChangedEventArgs selChange = e as RadSelectionChangedEventArgs;
                if (selChange != null)
                {
                    if (selChange.RemovedItems != null && selChange.RemovedItems.Count > 0 && selChange.RemovedItems.Contains(PaneActiveScreening))
                    {
                        CslaDataProvider provider = App.Current.Resources["ReviewInfoData"] as CslaDataProvider;
                        ReviewInfo rInfo = provider.Data as ReviewInfo;
                        if (rInfo != null && rInfo.IsDirty)
                        {
                            RadWindow.Alert("It appears you have changed"
                                 + Environment.NewLine + "Screening Settings."
                                 + Environment.NewLine + "Please Save or Cancel your changes.");
                            DocumentListPane.SelectionChanged -= DocumentListPane_SelectionChanged;
                            DocumentListPane.SelectedItem = PaneActiveScreening;
                            DocumentListPane.SelectionChanged += DocumentListPane_SelectionChanged;
                        }
                    }
                }
            }

            if (DocumentListPane.SelectedPane.Name == "PaneSearch")
            {
                codesTreeControl.ShowSearchMenuOptions = true;
            }
            else
            {
                codesTreeControl.ShowSearchMenuOptions = false;
            }

            if (DocumentListPane.SelectedPane.Name == "PaneDiagrams")
            {
                codesTreeControl.ShowDiagramMenuOptions = true;
            }
            else
            {
                codesTreeControl.ShowDiagramMenuOptions = false;
            }

            if (DocumentListPane.SelectedPane.Name == "PaneReports")
            {
                codesTreeControl.ShowReportMenuOptions = homeReportsControl.activeReport.ReportType;
                codesTreeControl.SetReportAppearance(true);
            }
            else
            {
                codesTreeControl.ShowReportMenuOptions = "";
                codesTreeControl.SetReportAppearance(false);
            }
            if (DocumentListPane.SelectedPane.Name == "PaneReports" && homeReportsControl.InitialisingControl == false) // we may have moved away from 5
            {
                homeReportsControl.SaveReport();
            }

            if (DocumentListPane.SelectedPane.Name == "PaneMetaAnalysis")
            {
                CslaDataProvider provider = this.Resources["MetaAnalysisListData"] as CslaDataProvider;
                if (provider != null)
                    provider.Refresh();
            }

            if (DocumentListPane.SelectedPane.Name == "PaneCollaborate")
            {
                CslaDataProvider provider = this.Resources["WorkAllocationListData"] as CslaDataProvider;
                if (provider != null)
                    provider.Refresh();

                CslaDataProvider provider2 = this.Resources["ReviewContactListData"] as CslaDataProvider;
                if (provider2 != null)
                    provider2.Refresh();

                CslaDataProvider provider3 = this.Resources["ComparisonListData"] as CslaDataProvider;
                if (provider3 != null)
                    provider3.Refresh();
            }

            if (DocumentListPane.SelectedPane.Name == "PaneMyTab")
            {
                CslaDataProvider provider = App.Current.Resources["WorkAllocationContactListData"] as CslaDataProvider;
                if (provider != null)
                {
                    provider.Refresh();
                }
                if (DialogMyInfo != null) DialogMyInfo.RefreshVisibleList();
            }
        }

        private void cmdOpenReportsWindow_Click(object sender, RoutedEventArgs e)
        {
            dialogReportsControl.ItemIdList = ItemsGridSelectedItems();
            windowReports.ShowDialog();
            dialogReportsControl.OnShow();// moved here as per http://www.telerik.com/community/forums/silverlight/general-discussions/radwindow-onopened-method.aspx
        }

        //private void windowReports_Opened(object sender, RoutedEventArgs e)
        //{
        //    dialogReportsControl.OnShow();
        //}

        private void cmdFindDuplicateItems_Click(object sender, RoutedEventArgs e)
        {
            dialogDuplicatesControl.getItemsListData(ItemsGrid.Items, ItemsGrid.SelectedItems as System.Collections.ObjectModel.ObservableCollection<object>);
            windowDuplicates.Loaded -= windowDuplicates_Loaded;
            windowDuplicates.Loaded += new RoutedEventHandler(windowDuplicates_Loaded);
            //if (this.windowDuplicates.FindChildByType<dialogDuplicateGroups>().GroupDuplicatesGrid1.ItemsSource == null)
            //    this.windowDuplicates.FindChildByType<dialogDuplicateGroups>().RefreshDuplicates();
            windowDuplicates.ShowDialog();
            dialogDuplicatesControl.windowGettingDuplicates1.Owner = windowDuplicates;
            //if (dialogDuplicatesControl.GroupDuplicatesGrid1 == null || dialogDuplicatesControl.GroupDuplicatesGrid1.ItemsSource == null)
            //{
            //    dialogDuplicatesControl.RefreshDuplicates();
            //    dialogDuplicatesControl.windowGettingDuplicates1.ShowDialog();
            //}
            
        }

        void windowDuplicates_Loaded(object sender, RoutedEventArgs e)
        {
            if ((dialogDuplicatesControl.GroupDuplicatesGrid1 == null || dialogDuplicatesControl.GroupDuplicatesGrid1.ItemsSource == null) && !dialogDuplicatesControl.windowGettingDuplicates1.IsActiveWindow)
            {
                dialogDuplicatesControl.RefreshDuplicates();
                dialogDuplicatesControl.windowGettingDuplicates1.ShowDialog();
            }
        }

        void windowDuplicates_Activated(object sender, EventArgs e)
        {
            //dialogDuplicatesControl.windowGettingDuplicates1.Owner = windowDuplicates;
            //if (dialogDuplicatesControl.GroupDuplicatesGrid1 == null || dialogDuplicatesControl.GroupDuplicatesGrid1.ItemsSource == null)
            //{
            //    dialogDuplicatesControl.RefreshDuplicates();
            //    dialogDuplicatesControl.windowGettingDuplicates1.ShowDialog();
            //}
            if (dialogDuplicatesControl.windowGettingDuplicates1.IsOpen) 
                dialogDuplicatesControl.windowGettingDuplicates1.ShowDialog();
        }

        private void cmdNewCodeSet_Click(object sender, RoutedEventArgs e)
        {
            codesTreeControl.CreateReviewSet();
        }

        private void cmdCodeProperties_Click(object sender, RoutedEventArgs e)
        {
            codesTreeControl.DoCodeProperties();
        }

        private void codesTreeControl_ListItemsWithCode(object sender, EventArgs e)
        {
            bool ShowIncluded = (sender as string).ToString() == "Included" ? true : false;
            string attributeList = GetSelectedAttributeSetIds();
            if (attributeList == "")
            {
                MessageBox.Show("No codes selected");
            }
            else
            {
                TextBlockShowing.Text = "Showing: " + GetSelectedAttributeNames() + (ShowIncluded ? "" : " (excluded)");
                SelectionCritieraItemList = new SelectionCriteria();
                SelectionCritieraItemList.ListType = "StandardItemList";
                SelectionCritieraItemList.OnlyIncluded = ShowIncluded;
                SelectionCritieraItemList.ShowDeleted = false;
                SelectionCritieraItemList.SourceId = 0;
                SelectionCritieraItemList.PageNumber = 0;
                SelectionCritieraItemList.AttributeSetIdList = attributeList;
                SelectionCritieraItemList.ShowInfoColumn = true;
                LoadItemList();
                DocumentListPane.SelectedIndex = 0;
            }
        }

        private void codesTreeControl_ListItemsWithoutCode(object sender, EventArgs e)
        {
            bool ShowIncluded = (sender as string).ToString() == "Included" ? true : false;
            string attributeList = GetSelectedAttributeSetIds();
            if (attributeList == "")
            {
                MessageBox.Show("No codes selected");
            }
            else
            {
                TextBlockShowing.Text = "Showing: without '" + GetSelectedAttributeNames() + (ShowIncluded ? "'" : "' (excluded)");
                SelectionCritieraItemList = new SelectionCriteria();
                SelectionCritieraItemList.ListType = "ItemListWithoutAttributes";
                SelectionCritieraItemList.OnlyIncluded = ShowIncluded;
                SelectionCritieraItemList.ShowDeleted = false;
                SelectionCritieraItemList.SourceId = 0;
                SelectionCritieraItemList.PageNumber = 0;
                SelectionCritieraItemList.AttributeSetIdList = attributeList;
                SelectionCritieraItemList.ShowInfoColumn = true;
                LoadItemList();
                DocumentListPane.SelectedIndex = 0;
            }
        }

        private void codesTreeControl_DisplayItemFrequencies(object sender, EventArgs e)
        {
            RadioButtonFrequenciesIncluded.IsChecked = true;
            cmdReportFrequency_Click(sender, new RoutedEventArgs());
            DocumentListPane.SelectedIndex = 3;
        }

        private void codesTreeControl_AssignSelected(object sender, EventArgs e)
        {
            CheckAssignItemsToCode();
        }

        private void codesTreeControl_RemoveSelected(object sender, EventArgs e)
        {
            CheckRemoveItemsFromCode();
        }

        private void codesTreeControl_InsertInDiagram(object sender, EventArgs e)
        {
            if (codesTreeControl.SelectedAttributeSet() != null)
            {
                MindFusion.Diagramming.Silverlight.ShapeNode node1 = theDiagram.Factory.CreateShapeNode(0, 0, 60, 60);
                node1.Text = codesTreeControl.SelectedAttributeSet().AttributeName;
                node1.Tag = codesTreeControl.SelectedAttributeSet().AttributeId;
            }
        }

        private void codesTreeControl_InsertChildCodesInDiagram(object sender, EventArgs e)
        {
            AttributeSetList asl = null;
            if (codesTreeControl.SelectedAttributeSet() != null)
                asl = codesTreeControl.SelectedAttributeSet().Attributes;
            if (codesTreeControl.SelectedReviewSet() != null)
                asl = codesTreeControl.SelectedReviewSet().Attributes;

            if (asl != null)
            {
                int pos = 0;
                foreach (AttributeSet attribute in asl)
                {
                    MindFusion.Diagramming.Silverlight.ShapeNode node1 = theDiagram.Factory.CreateShapeNode(pos, pos, 90, 60);
                    node1.Text = attribute.AttributeName;
                    node1.Tag = attribute.AttributeId;
                    pos = pos + 20;
                }
            }
        }

        private void codesTreeControl_InsertInReport(object sender, EventArgs e)
        {
            if (codesTreeControl.SelectedAttributeSet() != null)
            {
                homeReportsControl.BindTreeViewSelectedItem(codesTreeControl.SelectedAttributeSet());
                homeReportsControl.cmdAddCodeToReport_Click(sender, new RoutedEventArgs());
            }
            else
            {
                if (codesTreeControl.SelectedReviewSet() != null)
                {
                    homeReportsControl.BindTreeViewSelectedItem(codesTreeControl.SelectedReviewSet());
                    homeReportsControl.cmdAddCodeToReport_Click(sender, new RoutedEventArgs());
                }
            }
        }

        private void codesTreeControl_CodeWithinCode(object sender, EventArgs e)
        {
            if (codesTreeControl.SelectedAttributeSet() != null)
            {
                dlgAxialCoding.DataContext = codesTreeControl.SelectedAttributeSet();
                windowAxialCoding.ShowDialog();
            }
        }

        public void MouseDownOnWorkAllocation(object sender, MouseEventArgs args)
        {
            if (((UIElement)args.OriginalSource).ParentOfType<GridViewCell>() != null)
            {
                WorkAllocation wa = ((UIElement)args.OriginalSource).ParentOfType<GridViewCell>().DataContext as WorkAllocation;
                string col = ((UIElement)args.OriginalSource).ParentOfType<GridViewCell>().DataColumn.Header.ToString();
                string method = "";
                string desc = "";
                switch (col)
                {
                    case "Allocated":
                        method = "GetItemWorkAllocationList";
                        desc = "Showing total work allocation: ";
                        break;

                    case "Started":
                        method = "GetItemWorkAllocationListStarted";
                        desc = "Showing work allocation started: ";
                        break;

                    case "Remaining":
                        method = "GetItemWorkAllocationListRemaining";
                        desc = "Showing work allocation remaining: ";
                        break;

                    default:
                        break;
                }

                if (method == "")
                {
                    return;
                }
                else
                {
                    SelectionCritieraItemList = new SelectionCriteria();
                    SelectionCritieraItemList.WorkAllocationId = wa.WorkAllocationId;
                    SelectionCritieraItemList.PageNumber = 0;
                    SelectionCritieraItemList.ListType = method;
                    TextBlockShowing.Text = desc + wa.AttributeName;
                    DocumentListPane.SelectedIndex = 0;
                    LoadItemList();
                }
            }
        }

        private void ReviewContactListData_DataChanged(object sender, EventArgs e)
        {
            CslaDataProvider provider = ((CslaDataProvider)this.Resources["ReviewContactListData"]);
            if (provider.Error != null)
                System.Windows.Browser.HtmlPage.Window.Alert(((Csla.Xaml.CslaDataProvider)sender).Error.Message);
        }

        private void ComparisonListData_DataChanged(object sender, EventArgs e)
        {
            CslaDataProvider provider = ((CslaDataProvider)this.Resources["ComparisonListData"]);
            if (provider.Error != null)
                System.Windows.Browser.HtmlPage.Window.Alert(((Csla.Xaml.CslaDataProvider)sender).Error.Message);
        }

        private void cmdShowCreateComparisonWindow_Click(object sender, RoutedEventArgs e)
        {
            ReviewSetsList rsl = (App.Current.Resources["CodeSetsData"] as CslaDataProvider).Data as ReviewSetsList;
            windowCreateComparison.ComboBoxCodeSetsComparison.ItemsSource = rsl;
            windowCreateComparison.TextBlockAttributeComparison.DataContext = null;
            windowCreateComparison.ComboBoxReviewer1Comparison.SelectedIndex = -1;
            windowCreateComparison.ComboBoxReviewer2Comparison.SelectedIndex = -1;
            windowCreateComparison.ComboBoxReviewer3Comparison.SelectedIndex = -1;
            windowCreateComparison.ComboBoxCodeSetsComparison.SelectedIndex = -1;
            windowCreateComparison.Show();
        }

        private void cmdSetResetAttributeComparison_Click(object sender, RoutedEventArgs e)
        {
            AttributeSet attributeSet = codesTreeControl.SelectedAttributeSet();
            if (attributeSet != null)
            {
                windowCreateComparison.TextBlockAttributeComparison.DataContext = attributeSet;
            }
            else
            {
                MessageBox.Show(@"Please select a code using your codes
on the right of the main screen");
            }
        }

        private void cmdCreateComparison_Click(object sender, RoutedEventArgs e)
        {
            if ((windowCreateComparison.ComboBoxReviewer1Comparison.SelectedIndex == -1) || (windowCreateComparison.ComboBoxReviewer2Comparison.SelectedIndex == -1))
            {
                MessageBox.Show("Please select at least two reviewers");
                return;
            }
            if (windowCreateComparison.ComboBoxCodeSetsComparison.SelectedIndex == -1)
            {
                MessageBox.Show("Please select a code set to compare");
                return;
            }
            if ((windowCreateComparison.ComboBoxReviewer1Comparison.SelectedIndex == windowCreateComparison.ComboBoxReviewer2Comparison.SelectedIndex) ||
                (windowCreateComparison.ComboBoxReviewer1Comparison.SelectedIndex == windowCreateComparison.ComboBoxReviewer3Comparison.SelectedIndex) ||
                (windowCreateComparison.ComboBoxReviewer2Comparison.SelectedIndex == windowCreateComparison.ComboBoxReviewer3Comparison.SelectedIndex))
            {
                MessageBox.Show("Each selected reviewer must be different");
                return;
            }

            ReviewContactNVL.NameValuePair reviewer1 = windowCreateComparison.ComboBoxReviewer1Comparison.SelectedItem as ReviewContactNVL.NameValuePair;
            ReviewContactNVL.NameValuePair reviewer2 = windowCreateComparison.ComboBoxReviewer2Comparison.SelectedItem as ReviewContactNVL.NameValuePair;
            ReviewContactNVL.NameValuePair reviewer3 = windowCreateComparison.ComboBoxReviewer3Comparison.SelectedItem as ReviewContactNVL.NameValuePair;
            ReviewSet rs = windowCreateComparison.ComboBoxCodeSetsComparison.SelectedItem as ReviewSet;
            AttributeSet attribute = windowCreateComparison.TextBlockAttributeComparison.DataContext as AttributeSet;

            Comparison comp = new Comparison();
            comp.ContactId1 = reviewer1.Key;
            comp.ContactId2 = reviewer2.Key;
            if (reviewer3 != null)
                comp.ContactId3 = reviewer3.Key;
            else
                comp.ContactId3 = -1;
            comp.SetId = rs.SetId;
            if (attribute != null)
                comp.InGroupAttributeId = attribute.AttributeId;
            else
                comp.InGroupAttributeId = -1;
            comp.Saved += (o, e2) =>
            {
                if (e2.Error != null)
                    MessageBox.Show(e2.Error.Message);
                else
                {
                    CslaDataProvider provider = this.Resources["ComparisonListData"] as CslaDataProvider;
                    if (provider != null)
                    {
                        provider.Refresh();
                    }
                    windowCreateComparison.Close();
                }
            };
            comp.BeginSave();
        }

        private void cmdDeleteComparison_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to delete this comparison?", "Confirm delete", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                Comparison comp = (sender as Button).DataContext as Comparison;
                if (comp != null)
                {
                    comp.Delete();
                    comp.Saved += (o, e2) =>
                    {
                        if (e2.Error != null)
                            MessageBox.Show(e2.Error.Message);
                        else
                        {
                            CslaDataProvider provider = this.Resources["ComparisonListData"] as CslaDataProvider;
                            if (provider != null)
                            {
                                provider.Refresh();
                            }
                            windowCreateComparison.Close();
                        }
                    };
                    comp.BeginSave();
                }
            }
        }

        private void cmdQuickReportComparison_Click(object sender, RoutedEventArgs e)
        {
            windowQuickReportComparison.TextBlockCodeQuickReportComparison.DataContext = null;
            windowQuickReportComparison.TextBlockCodeQuickReportComparison.Text = "";
            windowQuickReportComparison.DataContext = (sender as Button).DataContext;
            windowQuickReportComparison.Show();
        }

        private void cmdStatsComparison_Click(object sender, RoutedEventArgs e)
        {
            Comparison comparison = (sender as Button).DataContext as Comparison;
            if (comparison != null)
            {
                windowComparisonStats.DataContext = comparison;
                windowComparisonStats.TextBlockComparison1vs2.Text = comparison.ContactName1 + " vs " + comparison.ContactName2;
                windowComparisonStats.ScreeningTab.Visibility = System.Windows.Visibility.Collapsed;
                windowComparisonStats.ShowDialog();
                DataPortal<ComparisonStatsCommand> dp = new DataPortal<ComparisonStatsCommand>();
                ComparisonStatsCommand command = new ComparisonStatsCommand(
                    comparison.ComparisonId);
                dp.ExecuteCompleted += (o, e2) =>
                {
                    windowComparisonStats.BusyLoadingComparisonStats.IsRunning = false;
                    if (e2.Error != null)
                    {
                        MessageBox.Show(e2.Error.Message);
                    }
                    else
                    {
                        windowComparisonStats.StaticText.DataContext = e2;
                        windowComparisonStats.TextBlockComparisonReviewer1.Text = "Number of documents coded by " + comparison.ContactName1.ToString() + ": " + e2.Object.NCoded1.ToString();
                        windowComparisonStats.TextBlockComparisonReviewer2.Text = "Number of documents coded by " + comparison.ContactName2.ToString() + ": " + e2.Object.NCoded2.ToString();
                        windowComparisonStats.TextBlockComparisonNumber1vs2.Text = "Number of documents coded by both " + comparison.ContactName1 + " and " + comparison.ContactName2 + ": " + e2.Object.N1vs2.ToString();
                        windowComparisonStats.cmdListComparisonAgreements1vs2.Content = (e2.Object.N1vs2 - e2.Object.Disagreements1vs2).ToString() + " / " + e2.Object.N1vs2.ToString() + " (list)";
                        windowComparisonStats.cmdListComparisonDisagreements1vs2.Content = e2.Object.Disagreements1vs2.ToString() + " / " + e2.Object.N1vs2.ToString() + " (list)";
                        windowComparisonStats.cmdCompleteComparisonAgreements1vs2.IsEnabled = e2.Object.CanComplete1vs2 && HasWriteRights;
                        windowComparisonStats.cmdReconcileDisagreements1vs2.IsEnabled = e2.Object.Disagreements1vs2 > 0;
                        windowComparisonStats.cmdReconcileDisagreements1vs3.IsEnabled = e2.Object.Disagreements1vs3 > 0;
                        windowComparisonStats.cmdReconcileDisagreements2vs3.IsEnabled = e2.Object.Disagreements2vs3 > 0;
                        if (windowComparisonStats.cmdCompleteComparisonAgreements1vs2.IsEnabled)
                        {
                            ToolTipService.SetToolTip(windowComparisonStats.tippercmdCompleteComparisonAgreements1vs2, null);
                        }
                        else 
                        {
                            ToolTipService.SetToolTip(windowComparisonStats.tippercmdCompleteComparisonAgreements1vs2, "Can't complete: coding is either already completed or has changed after this comparison was created."); 
                        }
                        if (comparison.ContactName3 != "")
                        {
                            windowComparisonStats.TextBlockComparisonReviewer3.Visibility = Visibility.Visible;
                            windowComparisonStats.TextBlockComparisonReviewer3.Text = "Number of documents coded by " + comparison.ContactName3.ToString() + ": " + e2.Object.NCoded3.ToString();
                            windowComparisonStats.cmdListComparisonAgreements2vs3.Visibility = Visibility.Visible;
                            windowComparisonStats.cmdListComparisonAgreements2vs3.Content = (e2.Object.N2vs3 - e2.Object.Disagreements2vs3).ToString() + " / " + e2.Object.N2vs3.ToString() + " (list)";
                            windowComparisonStats.TextBlockComparisonNumber2vs3.Visibility = Visibility.Visible;
                            windowComparisonStats.TextBlockComparisonNumber2vs3.Text = "Number of documents coded by both " + comparison.ContactName2 + " and " + comparison.ContactName3 + ": " + e2.Object.N2vs3.ToString();
                            windowComparisonStats.cmdListComparisonDisagreements2vs3.Visibility = Visibility.Visible;
                            windowComparisonStats.cmdListComparisonDisagreements2vs3.Content = e2.Object.Disagreements2vs3.ToString() + " / " + e2.Object.N2vs3.ToString() + " (list)";
                            windowComparisonStats.TextBlockComparison2vs3.Visibility = Visibility.Visible;
                            windowComparisonStats.cmdReconcileDisagreements2vs3.Visibility = Visibility.Visible;
                            windowComparisonStats.TextBlockComparison2vs3.Text = comparison.ContactName2 + " vs " + comparison.ContactName3;
                            windowComparisonStats.cmdCompleteComparisonAgreements2vs3.Visibility = Visibility.Visible;
                            windowComparisonStats.cmdCompleteComparisonAgreements1vs3.Visibility = Visibility.Visible;
                            windowComparisonStats.borderAgr.SetValue(Grid.RowSpanProperty, 4);
                            windowComparisonStats.borderDis.SetValue(Grid.RowSpanProperty, 4);
                            windowComparisonStats.cmdListComparisonAgreements1vs3.Visibility = Visibility.Visible;
                            windowComparisonStats.cmdListComparisonAgreements1vs3.Content = (e2.Object.N1vs3 - e2.Object.Disagreements1vs3).ToString() + " / " + e2.Object.N1vs3.ToString() + " (list)";
                            windowComparisonStats.cmdListComparisonDisagreements1vs3.Visibility = Visibility.Visible;
                            windowComparisonStats.cmdListComparisonDisagreements1vs3.Content = e2.Object.Disagreements1vs3.ToString() + " / " + e2.Object.N1vs3.ToString() + " (list)";
                            windowComparisonStats.TextBlockComparison1vs3.Visibility = Visibility.Visible;
                            windowComparisonStats.cmdReconcileDisagreements1vs3.Visibility = Visibility.Visible;
                            windowComparisonStats.TextBlockComparison1vs3.Text = comparison.ContactName1 + " vs " + comparison.ContactName3;
                            windowComparisonStats.TextBlockComparisonNumber1vs3.Visibility = Visibility.Visible;
                            windowComparisonStats.TextBlockComparisonNumber1vs3.Text = "Number of documents coded by both " + comparison.ContactName1 + " and " + comparison.ContactName3 + ": " + e2.Object.N1vs3.ToString();
                            windowComparisonStats.cmdCompleteComparisonAgreements1vs3.IsEnabled = e2.Object.CanComplete1vs3 && HasWriteRights;
                            windowComparisonStats.tippercmdCompleteComparisonAgreements1vs3.Visibility = System.Windows.Visibility.Visible;
                            windowComparisonStats.tippercmdCompleteComparisonAgreements2vs3.Visibility = System.Windows.Visibility.Visible;
                            if (windowComparisonStats.cmdCompleteComparisonAgreements1vs3.IsEnabled)
                            {
                                ToolTipService.SetToolTip(windowComparisonStats.tippercmdCompleteComparisonAgreements1vs3, null);
                            }
                            else
                            {
                                ToolTipService.SetToolTip(windowComparisonStats.tippercmdCompleteComparisonAgreements1vs3, "Can't complete: coding is either already completed or has changed after this comparison was created.");
                            }
                            windowComparisonStats.cmdCompleteComparisonAgreements2vs3.IsEnabled = e2.Object.CanComplete2vs3 && HasWriteRights;
                            if (windowComparisonStats.cmdCompleteComparisonAgreements2vs3.IsEnabled)
                            {
                                ToolTipService.SetToolTip(windowComparisonStats.tippercmdCompleteComparisonAgreements2vs3, null);
                            }
                            else
                            {
                                ToolTipService.SetToolTip(windowComparisonStats.tippercmdCompleteComparisonAgreements2vs3, "Can't complete: coding is either already completed or has changed after this comparison was created.");
                            }
                        }
                        else
                        {

                            windowComparisonStats.TextBlockComparison2vs3.Visibility = Visibility.Collapsed;
                            windowComparisonStats.TextBlockComparisonReviewer3.Visibility = Visibility.Collapsed;
                            windowComparisonStats.TextBlockComparisonNumber2vs3.Visibility = Visibility.Collapsed;
                            windowComparisonStats.cmdListComparisonAgreements2vs3.Visibility = Visibility.Collapsed;
                            windowComparisonStats.cmdListComparisonDisagreements2vs3.Visibility = Visibility.Collapsed;
                            windowComparisonStats.TextBlockComparisonNumber1vs3.Visibility = Visibility.Collapsed;
                            windowComparisonStats.TextBlockComparison1vs3.Visibility = Visibility.Collapsed;
                            windowComparisonStats.cmdListComparisonAgreements1vs3.Visibility = Visibility.Collapsed;
                            windowComparisonStats.cmdListComparisonDisagreements1vs3.Visibility = Visibility.Collapsed;
                            windowComparisonStats.cmdCompleteComparisonAgreements1vs3.Visibility = Visibility.Collapsed;
                            windowComparisonStats.cmdCompleteComparisonAgreements2vs3.Visibility = Visibility.Collapsed;
                            ToolTipService.SetToolTip(windowComparisonStats.tippercmdCompleteComparisonAgreements2vs3, null);
                            ToolTipService.SetToolTip(windowComparisonStats.tippercmdCompleteComparisonAgreements1vs3, null);
                            windowComparisonStats.cmdReconcileDisagreements1vs3.Visibility = Visibility.Collapsed;
                            windowComparisonStats.cmdReconcileDisagreements2vs3.Visibility = Visibility.Collapsed;
                            windowComparisonStats.tippercmdCompleteComparisonAgreements1vs3.Visibility = System.Windows.Visibility.Collapsed;
                            windowComparisonStats.tippercmdCompleteComparisonAgreements2vs3.Visibility = System.Windows.Visibility.Collapsed;
                            windowComparisonStats.borderAgr.SetValue(Grid.RowSpanProperty, 2);
                            windowComparisonStats.borderDis.SetValue(Grid.RowSpanProperty, 2);
                        }
                        if (e2.Object.isScreening)
                        {
                            windowComparisonStats.NormalTab.Header = "Full";
                            windowComparisonStats.ScreeningTab.Visibility = System.Windows.Visibility.Visible;
                            windowComparisonStats.TextBlockComparison1vs2Sc.Text = comparison.ContactName1 + " vs " + comparison.ContactName2;
                            windowComparisonStats.cmdListComparisonAgreements1vs2Sc.Content = (e2.Object.N1vs2 - e2.Object.ScDisagreements1vs2).ToString() + " / " + e2.Object.N1vs2.ToString() + " (list)";
                            windowComparisonStats.cmdListComparisonDisagreements1vs2Sc.Content = e2.Object.ScDisagreements1vs2.ToString() + " / " + e2.Object.N1vs2.ToString() + " (list)";
                            windowComparisonStats.cmdCompleteComparisonAgreements1vs2Sc.IsEnabled = e2.Object.CanComplete1vs2 && HasWriteRights;
                            windowComparisonStats.cmdReconcileDisagreements1vs2Sc.IsEnabled = e2.Object.ScDisagreements1vs2 > 0;
                            windowComparisonStats.cmdReconcileDisagreements1vs3Sc.IsEnabled = e2.Object.ScDisagreements1vs3 > 0;
                            windowComparisonStats.cmdReconcileDisagreements2vs3Sc.IsEnabled = e2.Object.ScDisagreements2vs3 > 0;
                        
                            if (windowComparisonStats.cmdCompleteComparisonAgreements1vs2Sc.IsEnabled)
                            {
                                ToolTipService.SetToolTip(windowComparisonStats.tippercmdCompleteComparisonAgreements1vs2Sc, null);
                            }
                            else
                            {
                                ToolTipService.SetToolTip(windowComparisonStats.tippercmdCompleteComparisonAgreements1vs2Sc, "Can't complete: coding is either already completed or has changed after this comparison was created.");
                            }
                            if (comparison.ContactName3 != "")
                            {
                                windowComparisonStats.cmdListComparisonAgreements2vs3Sc.Visibility = Visibility.Visible;
                                windowComparisonStats.cmdListComparisonAgreements2vs3Sc.Content = (e2.Object.N2vs3 - e2.Object.ScDisagreements2vs3).ToString() + " / " + e2.Object.N2vs3.ToString() + " (list)";
                                windowComparisonStats.cmdListComparisonDisagreements2vs3Sc.Visibility = Visibility.Visible;
                                windowComparisonStats.cmdListComparisonDisagreements2vs3Sc.Content = e2.Object.ScDisagreements2vs3.ToString() + " / " + e2.Object.N2vs3.ToString() + " (list)";
                                windowComparisonStats.TextBlockComparison2vs3Sc.Visibility = Visibility.Visible;
                                windowComparisonStats.TextBlockComparison2vs3Sc.Text = comparison.ContactName2 + " vs " + comparison.ContactName3;
                                windowComparisonStats.cmdCompleteComparisonAgreements2vs3Sc.Visibility = Visibility.Visible;
                                windowComparisonStats.cmdCompleteComparisonAgreements1vs3Sc.Visibility = Visibility.Visible;
                                windowComparisonStats.cmdReconcileDisagreements2vs3Sc.Visibility = Visibility.Visible;
                                windowComparisonStats.cmdReconcileDisagreements1vs3Sc.Visibility = Visibility.Visible;
                                windowComparisonStats.borderAgrSc.SetValue(Grid.RowSpanProperty, 4);
                                windowComparisonStats.borderDisSc.SetValue(Grid.RowSpanProperty, 4);
                                windowComparisonStats.cmdListComparisonAgreements1vs3Sc.Visibility = Visibility.Visible;
                                windowComparisonStats.cmdListComparisonAgreements1vs3Sc.Content = (e2.Object.N1vs3 - e2.Object.ScDisagreements1vs3).ToString() + " / " + e2.Object.N1vs3.ToString() + " (list)";
                                windowComparisonStats.cmdListComparisonDisagreements1vs3Sc.Visibility = Visibility.Visible;
                                windowComparisonStats.cmdListComparisonDisagreements1vs3Sc.Content = e2.Object.ScDisagreements1vs3.ToString() + " / " + e2.Object.N1vs3.ToString() + " (list)";
                                windowComparisonStats.TextBlockComparison1vs3Sc.Visibility = Visibility.Visible;
                                windowComparisonStats.TextBlockComparison1vs3Sc.Text = comparison.ContactName1 + " vs " + comparison.ContactName3;
                                windowComparisonStats.cmdCompleteComparisonAgreements1vs3Sc.IsEnabled = e2.Object.CanComplete1vs3 && HasWriteRights;
                                windowComparisonStats.tippercmdCompleteComparisonAgreements1vs3Sc.Visibility = System.Windows.Visibility.Visible;
                                windowComparisonStats.tippercmdCompleteComparisonAgreements2vs3Sc.Visibility = System.Windows.Visibility.Visible;
                                if (windowComparisonStats.cmdCompleteComparisonAgreements1vs3Sc.IsEnabled)
                                {
                                    ToolTipService.SetToolTip(windowComparisonStats.tippercmdCompleteComparisonAgreements2vs3Sc, null);
                                    ToolTipService.SetToolTip(windowComparisonStats.tippercmdCompleteComparisonAgreements1vs3Sc, null);
                                }
                                else
                                {
                                    ToolTipService.SetToolTip(windowComparisonStats.tippercmdCompleteComparisonAgreements2vs3Sc, "Can't complete: coding is either already completed or has changed after this comparison was created.");
                                    ToolTipService.SetToolTip(windowComparisonStats.tippercmdCompleteComparisonAgreements1vs3Sc, "Can't complete: coding is either already completed or has changed after this comparison was created.");
                                    
                                }
                                windowComparisonStats.cmdCompleteComparisonAgreements2vs3Sc.IsEnabled = e2.Object.CanComplete2vs3 && HasWriteRights;
                                
                            }
                            else
                            {

                                windowComparisonStats.TextBlockComparison2vs3Sc.Visibility = Visibility.Collapsed;
                                windowComparisonStats.cmdListComparisonAgreements2vs3Sc.Visibility = Visibility.Collapsed;
                                windowComparisonStats.cmdListComparisonDisagreements2vs3Sc.Visibility = Visibility.Collapsed;
                                windowComparisonStats.TextBlockComparison1vs3Sc.Visibility = Visibility.Collapsed;
                                windowComparisonStats.cmdListComparisonAgreements1vs3Sc.Visibility = Visibility.Collapsed;
                                windowComparisonStats.cmdListComparisonDisagreements1vs3Sc.Visibility = Visibility.Collapsed;
                                windowComparisonStats.cmdCompleteComparisonAgreements1vs3Sc.Visibility = Visibility.Collapsed;
                                windowComparisonStats.cmdCompleteComparisonAgreements2vs3Sc.Visibility = Visibility.Collapsed;
                                windowComparisonStats.cmdReconcileDisagreements2vs3Sc.Visibility = Visibility.Collapsed;
                                windowComparisonStats.cmdReconcileDisagreements1vs3Sc.Visibility = Visibility.Collapsed;
                                windowComparisonStats.tippercmdCompleteComparisonAgreements1vs3Sc.Visibility = System.Windows.Visibility.Collapsed;
                                windowComparisonStats.tippercmdCompleteComparisonAgreements2vs3Sc.Visibility = System.Windows.Visibility.Collapsed;
                                windowComparisonStats.borderAgrSc.SetValue(Grid.RowSpanProperty, 2);
                                windowComparisonStats.borderDisSc.SetValue(Grid.RowSpanProperty, 2);
                            }
                        }
                        else
                        {
                            windowComparisonStats.NormalTab.Header = null;
                            windowComparisonStats.CompTabs.SelectedItem = windowComparisonStats.NormalTab;
                        }
                    }
                };
                windowComparisonStats.BusyLoadingComparisonStats.IsRunning = true;
                dp.BeginExecute(command);
            }
            else
            {
                MessageBox.Show("Error: no datacontext for stats");
            }
        }

        private void cmdSelectCodeQuickReportComparison_Click(object sender, RoutedEventArgs e)
        {
            if (codesTreeControl.SelectedAttributeSet() != null)
            {
                windowQuickReportComparison.TextBlockCodeQuickReportComparison.DataContext = codesTreeControl.SelectedAttributeSet();
                windowQuickReportComparison.TextBlockCodeQuickReportComparison.Text = codesTreeControl.SelectedAttributeSet().AttributeName;
            }
            else
            {
                if (codesTreeControl.SelectedReviewSet() != null)
                {
                    windowQuickReportComparison.TextBlockCodeQuickReportComparison.DataContext = codesTreeControl.SelectedReviewSet();
                    windowQuickReportComparison.TextBlockCodeQuickReportComparison.Text = codesTreeControl.SelectedReviewSet().SetName;
                }
            }
        }

        private void cmdRunQuickReportComparison_Click(object sender, RoutedEventArgs e)
        {
            if (windowQuickReportComparison.TextBlockCodeQuickReportComparison.DataContext == null)
            {
                MessageBox.Show("Please select a code or code set first.");
                return;
            }
            Comparison comparison = windowQuickReportComparison.DataContext as Comparison;
            if (comparison == null)
            {
                MessageBox.Show("Error: no data context for report");
                return;
            }
            Int64 ParentAttributeId = 0;
            int SetId = 0;
            if (windowQuickReportComparison.TextBlockCodeQuickReportComparison.DataContext is AttributeSet)
            {
                ParentAttributeId = (windowQuickReportComparison.TextBlockCodeQuickReportComparison.DataContext as AttributeSet).AttributeId;
                SetId = (windowQuickReportComparison.TextBlockCodeQuickReportComparison.DataContext as AttributeSet).SetId;
            }
            else
            {
                if (windowQuickReportComparison.TextBlockCodeQuickReportComparison.DataContext is ReviewSet)
                {
                    SetId = (windowQuickReportComparison.TextBlockCodeQuickReportComparison.DataContext as ReviewSet).SetId;
                }
            }

            DataPortal<ComparisonAttributeList> dp = new DataPortal<ComparisonAttributeList>();
            dp.FetchCompleted += (o, e2) =>
            {
                BusyLoading.IsRunning = false;
                if (e2.Error != null)
                {
                    MessageBox.Show(e2.Error.Message);
                }
                else
                {
                    bool thirdReviewerIncluded = false;
                    string report = "<html><body><p><h3>Comparison report between: <i>" + comparison.ContactName1 + "</i> and <i>" +
                        comparison.ContactName2 + "</i>";
                    if (comparison.ContactName3 != "")
                    {
                        report += " and <i>" + comparison.ContactName3 + "</i>";
                    }
                    report += "</h3>";
                    report += "<P>This report is based on the status of the database at the time the comparison was created. Any coding ‘completed’ after the comparison was created will be displayed also in the Agreed column.</P>";
                    if (windowQuickReportComparison.TextBlockCodeQuickReportComparison.DataContext is AttributeSet)
                    {
                        report += "<h4>" + (windowQuickReportComparison.TextBlockCodeQuickReportComparison.DataContext as AttributeSet).AttributeName + "</h4>";
                    }
                    if (windowQuickReportComparison.TextBlockCodeQuickReportComparison.DataContext is ReviewSet)
                    {
                        report += "<h4>" + (windowQuickReportComparison.TextBlockCodeQuickReportComparison.DataContext as ReviewSet).SetName + "</h4>";
                    }
                    report += "<table border='1'><tr><td>Id</td><td>Item</td><td>" + comparison.ContactName1 + "</td><td>" + comparison.ContactName2 + "</td>";
                    if (comparison.ContactName3 != "")
                    {
                        report += "<td>" + comparison.ContactName3 + "</td>";
                        thirdReviewerIncluded = true;
                    }
                    report += "<td>Agreed version</td></tr>";
                    ComparisonAttributeList list = e2.Object;
                    Int64 CurrentItemId = -1;
                    string CurrentItem = "";
                    string Reviewer1 = "";
                    string Reviewer2 = "";
                    string Reviewer3 = "";
                    string Agreed = "";
                    foreach (ComparisonAttribute item in list)
                    {
                        if (item.ItemId != CurrentItemId)
                        {
                            if (CurrentItemId != -1)
                            {
                                report += "<tr><td valign='top'>" + CurrentItemId.ToString() + "</td><td valign='top'>" + CurrentItem + "</td>";
                                report += "<td valign='top'>" + Reviewer1 + "</td>";
                                report += "<td valign='top'>" + Reviewer2 + "</td>";
                                if (thirdReviewerIncluded == true)
                                {
                                    report += "<td valign='top'>" + Reviewer3 + "</td>";
                                }
                                report += "<td valign='top'>" + Agreed + "</td>";
                                report += "</tr>";
                            }
                            Reviewer1 = "";
                            Reviewer2 = "";
                            Reviewer3 = "";
                            Agreed = "";
                            CurrentItemId = item.ItemId;
                            CurrentItem = item.ItemTitle;
                        }
                        if ((item.ContactId == comparison.ContactId1) && (item.IsCompleted == false))
                        {
                            if (Reviewer1 == "")
                                Reviewer1 = item.AttributeNameWithArm4HTML;
                            else
                                Reviewer1 += "<br><br>" + item.AttributeNameWithArm4HTML;
                            if (item.AdditionalText != "")
                                Reviewer1 += "<br><i> " + item.AdditionalText + "</i>";
                        }
                        else
                        {
                            if ((item.ContactId == comparison.ContactId2) && (item.IsCompleted == false))
                            {
                                if (Reviewer2 == "")
                                    Reviewer2 = item.AttributeNameWithArm4HTML;
                                else
                                    Reviewer2 += "<br><br>" + item.AttributeNameWithArm4HTML;
                                if (item.AdditionalText != "")
                                    Reviewer2 += "<br><i> " + item.AdditionalText + "</i>";
                            }
                            else
                            {
                                if ((item.ContactId == comparison.ContactId3) && (item.IsCompleted == false))
                                {
                                    if (Reviewer3 == "")
                                        Reviewer3 = item.AttributeNameWithArm4HTML;
                                    else
                                        Reviewer3 += "<br><br>" + item.AttributeNameWithArm4HTML;
                                    if (item.AdditionalText != "")
                                        Reviewer3 += "<br><i> " + item.AdditionalText + "</i>";
                                }
                                else
                                {
                                    if (item.IsCompleted == true)
                                    {
                                        if (Agreed == "")
                                            Agreed = item.AttributeNameWithArm4HTML;
                                        else
                                            Agreed += "<br><br>" + item.AttributeNameWithArm4HTML;
                                        if (item.AdditionalText != "")
                                            Agreed += "<br><i> " + item.AdditionalText + "</i>";
                                    }
                                }
                            }
                        }
                    }
                    report += "<tr><td valign='top'>" + CurrentItemId.ToString() + "</td><td valign='top'>" + CurrentItem + "</td>";
                    report += "<td valign='top'>" + Reviewer1 + "</td>";
                    report += "<td valign='top'>" + Reviewer2 + "</td>";
                    if (thirdReviewerIncluded == true)
                    {
                        report += "<td valign='top'>" + Reviewer3 + "</td>";
                    }
                    report += "<td valign='top'>" + Agreed + "</td>";
                    report += "</tr>";
                    report += "</table></p>";
                    System.Windows.Browser.HtmlPage.Window.Invoke("ShowPopup", report);
                    windowQuickReportComparison.Close();
                }
            };
            BusyLoading.IsRunning = true;
            dp.BeginFetch(new ComparisonAttributeSelectionCriteria(typeof(ComparisonAttributeList), comparison.ComparisonId, ParentAttributeId, SetId));
        }

        private void cmdListComparisonAgreements1vs2_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            Comparison comp = windowComparisonStats.DataContext as Comparison;
            if ((btn != null) && (comp != null))
            {
                SelectionCritieraItemList = new SelectionCriteria();
                SelectionCritieraItemList.ComparisonId = comp.ComparisonId;
                SelectionCritieraItemList.PageNumber = 0;
                SelectionCritieraItemList.ListType = "Comparison" + btn.Tag.ToString();
                TextBlockShowing.Text = TextForComparisonItemList(btn.Tag.ToString(), comp);
                windowComparisonStats.Close();
                DocumentListPane.SelectedIndex = 0;
                LoadItemList();
            }
        }
        void windowComparisonStats_cmdReconcileDisagreements_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            Comparison comp = windowComparisonStats.DataContext as Comparison;
            if ((btn != null) && (comp != null))
            {
                windowReconcile.ItemsGrid.IsEnabled = false;
                windowReconcile.ItemsGrid.ItemsSource = null;
                windowReconcile.ItemsGrid.Items.Clear();
                SelectionCritieraItemList = new SelectionCriteria();
                SelectionCritieraItemList.ComparisonId = comp.ComparisonId;
                SelectionCritieraItemList.PageNumber = 0;
                SelectionCritieraItemList.ListType = "Comparison" + btn.Tag.ToString().Replace("Reconcile", "Disagree");
                TextBlockShowing.Text = TextForComparisonItemList(btn.Tag.ToString().Replace("Reconcile", "Disagree"), comp);
                windowComparisonStats.IsEnabled = false;


                


                CslaDataProvider provider = this.Resources["ItemListData"] as CslaDataProvider;
                provider.DataChanged += new EventHandler(ItemListData_DataChanged_1_Off);
                LoadItemList();
            }
        }
        private void ItemListData_DataChanged_1_Off(object sender, EventArgs e)
        {//windowReconcile
            windowComparisonStats.IsEnabled = true;
            windowComparisonStats.Close();

            
            CslaDataProvider provider = ((CslaDataProvider)this.Resources["ItemListData"]);
            
            provider.DataChanged -= ItemListData_DataChanged_1_Off;
            if (provider.Error == null && provider.Data != null)
            {
                if (windowReconcile.ItemsGridDataPager.GetBindingExpression(RadDataPager.SourceProperty) == null)
                {
                    Binding binding = new Binding();
                    binding.Source = provider;
                    binding.Path = new PropertyPath("Data");
                    //binding.Mode = BindingMode.TwoWay;
                    windowReconcile.ItemsGridDataPager.SetBinding(RadDataPager.SourceProperty, binding);
                }
                windowReconcile.ReconcileRoot.DataContext = (provider.Data as ItemList)[0];
                windowReconcile.DataContext = provider.Data;
                Comparison comp = windowComparisonStats.DataContext as Comparison;
                windowReconcile.comparison = comp;
                windowReconcile.reviewSet = codesTreeControl.GetReviewSet(comp.SetId);

                //set up the first part of the possible reconciliation report
                windowReconcile.ComparisonDescription = "<H1>Reconciliation Report</H1>";
                windowReconcile.ComparisonDescription += "<B>Code Set: " + windowReconcile.reviewSet.SetName + ".</B><br />";

                windowReconcile.ComparisonDescription += "Comparison Created on: " + comp.ComparisonDate.ToString() + "; report created on: " 
                    + System.DateTime.Now.ToShortDateString() + ".<br />";
                
                windowReconcile.ComparisonDescription += "Reviewers in this comparison: " + comp.ContactName1;
                if (comp.ContactId3 > 0)
                {
                    windowReconcile.ComparisonDescription += ", " + comp.ContactName2 + " and " + comp.ContactName3 + ".<br />";
                }
                else
                {
                    windowReconcile.ComparisonDescription += " and " + comp.ContactName2 + ".<br />";
                }
                DataPortalResult<ComparisonStatsCommand> stats = windowComparisonStats.StaticText.DataContext as DataPortalResult<ComparisonStatsCommand>;
                if (stats == null) return;//stats should now contain the results of the stats command!
                SelectionCriteria gettingwhat = provider.FactoryParameters[0] as SelectionCriteria;
                windowReconcile.ComparisonDescription += "<br />The Following numbers summarise the Comparison stats as they where when the comparison was created (Snapshot).<br />";

                
                windowReconcile.ComparisonDescription += windowComparisonStats.TextBlockComparisonReviewer1.Text + ".<br />";
                windowReconcile.ComparisonDescription += windowComparisonStats.TextBlockComparisonReviewer2.Text + ".<br />";
                if (comp.ContactId3 > 0)
                {
                    windowReconcile.ComparisonDescription += windowComparisonStats.TextBlockComparisonReviewer3.Text + ".<br />";
                }

                windowReconcile.ComparisonDescription += windowComparisonStats.TextBlockComparisonNumber1vs2.Text + ".<br />";
                if (comp.ContactId3 > 0)
                {
                    windowReconcile.ComparisonDescription += windowComparisonStats.TextBlockComparisonNumber2vs3.Text + ".<br />";
                    windowReconcile.ComparisonDescription += windowComparisonStats.TextBlockComparisonNumber1vs3.Text + ".<br />";
                }
                windowReconcile.ComparisonDescription += "<br />The table below shows the coding status of items as they are now (not a snapshot), the list of items is based on the comparison snapshot.<br />";
                switch (gettingwhat.ListType)
                {
                    case "ComparisonDisagree1vs2":
                        windowReconcile.ComparisonDescription += "Showing all disagreements (" + (stats.Object.Disagreements1vs2).ToString() + " of " + stats.Object.N1vs2.ToString() + " - based on the snapshot) for: " + comp.ContactName1 + " and " + comp.ContactName2 + ".<br />";
                        break;
                    case "ComparisonDisagree2vs3":
                        windowReconcile.ComparisonDescription += "Showing all disagreements (" + (stats.Object.Disagreements2vs3).ToString() + " of " + stats.Object.N2vs3.ToString() + " - based on the snapshot) for: " + comp.ContactName2 + " and " + comp.ContactName3 + ".<br />";
                        break;
                    case "ComparisonDisagree1vs3":
                        windowReconcile.ComparisonDescription += "Showing all disagreements (" + (stats.Object.Disagreements1vs3).ToString() + " of " + stats.Object.N1vs3.ToString() + " - based on the snapshot) for: " + comp.ContactName1 + " and " + comp.ContactName3 + ".<br />";
                        break;
                    case "ComparisonDisagree1vs2Sc":
                        windowReconcile.ComparisonDescription += "Showing Include/Exclude disagreements (" + (stats.Object.ScDisagreements1vs2).ToString() + " of " + stats.Object.N1vs2.ToString() + " - based on the snapshot) for: " + comp.ContactName1 + " and " + comp.ContactName2 + ".<br />";
                        break;
                    case "ComparisonDisagree2vs3Sc":
                        windowReconcile.ComparisonDescription += "Showing Include/Exclude disagreements (" + (stats.Object.ScDisagreements2vs3).ToString() + " of " + stats.Object.N2vs3.ToString() + " - based on the snapshot) for: " + comp.ContactName2 + " and " + comp.ContactName3 + ".<br />";
                        break;
                    case "ComparisonDisagree1vs3Sc":
                        windowReconcile.ComparisonDescription += "Showing Include/Exclude disagreements (" + (stats.Object.ScDisagreements1vs3).ToString() + " of " + stats.Object.N1vs3.ToString() + " - based on the snapshot) for: " + comp.ContactName1 + " and " + comp.ContactName3 + ".<br />";
                        break;
                    default:
                        return;//if none of the above, we shouldn't be doing this!
                }
                //end of setting up report
                
                windowReconcile.StartGettingData();
                windowReconcile.ShowDialog();
            }
            
        }
        private string TextForComparisonItemList(string tag, Comparison comp)
        {
            string retVal = "";
            string type = "";
            if (tag.StartsWith("Agree"))
            {
                type = "Showing agreements between ";
            }
            else
            {
                type = "Showing disagreements between ";
            }
            if (tag.EndsWith("1vs2") || tag.EndsWith("1vs2Sc"))
            {
                retVal = type + comp.ContactName1 + " and " + comp.ContactName2;
            }
            if (tag.EndsWith("2vs3") || tag.EndsWith("2vs3Sc"))
            {
                retVal = type + comp.ContactName2 + " and " + comp.ContactName3;
            }
            if (tag.EndsWith("1vs3") || tag.EndsWith("1vs3Sc"))
            {
                retVal = type + comp.ContactName1 + " and " + comp.ContactName3;
            }
            retVal += " using " + comp.SetName + " on " + comp.ComparisonDate.Text;
            return retVal;
        }

        private void cmdComparisonCompleteCancel_Click(object sender, RoutedEventArgs e)
        {
            windowComparisonComplete.Close();
        }

        private void cmdCompleteComparisonAgreements1vs2_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            Comparison comp = windowComparisonStats.DataContext as Comparison;
            if ((btn != null) && (comp != null))
            {
                ComboBoxItem cbi1 = new ComboBoxItem();
                cbi1.Content = comp.ContactName1;
                cbi1.Tag = comp.ContactId1;
                ComboBoxItem cbi2 = new ComboBoxItem();
                cbi2.Content = comp.ContactName2;
                cbi2.Tag = comp.ContactId2;
                ComboBoxItem cbi3 = new ComboBoxItem();
                cbi3.Content = comp.ContactName3;
                cbi3.Tag = comp.ContactId3;
                windowComparisonComplete.ComboBoxComparisonComplete.Items.Clear();
                switch (btn.Tag.ToString())
                {
                    case "Complete1vs2":
                        windowComparisonComplete.ComboBoxComparisonComplete.Items.Add(cbi1);
                        windowComparisonComplete.ComboBoxComparisonComplete.Items.Add(cbi2);
                        windowComparisonComplete.TextBlockComparisonComplete.Text = "Do you want to use the data entered by " +
                            comp.ContactName1 + " or " + comp.ContactName2 + "? (The codes will be identical, but they may differ in terms of additional text entered.)";
                        break;
                    case "Complete1vs3":
                        windowComparisonComplete.ComboBoxComparisonComplete.Items.Add(cbi1);
                        windowComparisonComplete.ComboBoxComparisonComplete.Items.Add(cbi3);
                        windowComparisonComplete.TextBlockComparisonComplete.Text = "Do you want to use the data entered by " +
                            comp.ContactName1 + " or " + comp.ContactName3 + "? (The codes will be identical, but they may differ in terms of additional text entered.)";
                        break;
                    case "Complete2vs3":
                        windowComparisonComplete.ComboBoxComparisonComplete.Items.Add(cbi2);
                        windowComparisonComplete.ComboBoxComparisonComplete.Items.Add(cbi3);
                        windowComparisonComplete.TextBlockComparisonComplete.Text = "Do you want to use the data entered by " +
                            comp.ContactName2 + " or " + comp.ContactName3 + "? (The codes will be identical, but they may differ in terms of additional text entered.)";
                        break;
                    case "Complete1vs2Sc":
                        windowComparisonComplete.ComboBoxComparisonComplete.Items.Add(cbi1);
                        windowComparisonComplete.ComboBoxComparisonComplete.Items.Add(cbi2);
                        windowComparisonComplete.TextBlockComparisonComplete.Text = "Do you want to use the data entered by " +
                            comp.ContactName1 + " or " + comp.ContactName2 + "? (The Include/Exclude assignment will be identical, but they may differ in terms of specific codes and additional text entered.)";
                        break;
                    case "Complete1vs3Sc":
                        windowComparisonComplete.ComboBoxComparisonComplete.Items.Add(cbi1);
                        windowComparisonComplete.ComboBoxComparisonComplete.Items.Add(cbi3);
                        windowComparisonComplete.TextBlockComparisonComplete.Text = "Do you want to use the data entered by " +
                            comp.ContactName1 + " or " + comp.ContactName3 + "? (The Include/Exclude assignment will be identical, but they may differ in terms of specific codes and additional text entered.)";
                        break;
                    case "Complete2vs3Sc":
                        windowComparisonComplete.ComboBoxComparisonComplete.Items.Add(cbi2);
                        windowComparisonComplete.ComboBoxComparisonComplete.Items.Add(cbi3);
                        windowComparisonComplete.TextBlockComparisonComplete.Text = "Do you want to use the data entered by " +
                            comp.ContactName2 + " or " + comp.ContactName3 + "? (The Include/Exclude assignment will be identical, but they may differ in terms of specific codes and additional text entered.)";
                        break;
                    default:
                        break;
                }
                windowComparisonComplete.ComboBoxComparisonComplete.SelectedIndex = 0;
                windowComparisonComplete.Tag = btn.Tag;
                windowComparisonComplete.ShowDialog();
            }
        }

        private void cmdComparisonCompleteGo_Click(object sender, RoutedEventArgs e)
        {
            Comparison comp = windowComparisonStats.DataContext as Comparison;
            if (MessageBox.Show("Mark codes as complete now?", "Confirm action", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                if (windowComparisonComplete.Tag.ToString().Contains("Sc"))//suffix that tells us we are completing the screening I/E results
                {
                    DataPortal<ComparisonScreeningCompleteCommand> dp = new DataPortal<ComparisonScreeningCompleteCommand>();
                    ComparisonScreeningCompleteCommand command = new ComparisonScreeningCompleteCommand(
                        comp.ComparisonId,
                        windowComparisonComplete.Tag.ToString(),
                        Convert.ToInt32((windowComparisonComplete.ComboBoxComparisonComplete.SelectedItem as ComboBoxItem).Tag));
                    dp.ExecuteCompleted += (o, e2) =>
                    {
                        windowComparisonComplete.BusyLoadingComparisonCompletion.IsRunning = false;
                        if (e2.Error != null)
                        {
                            MessageBox.Show(e2.Error.Message);
                        }
                        else
                        {
                            MessageBox.Show("Complete. " + e2.Object.NumberAffected.ToString() + " records affected.");

                        }
                        windowComparisonComplete.Close();
                        Button btn = new Button();
                        btn.DataContext = comp;
                        cmdStatsComparison_Click(btn, e);
                        //windowComparisonStats.Close();
                    };
                    windowComparisonComplete.BusyLoadingComparisonCompletion.IsRunning = true;
                    dp.BeginExecute(command);
                }
                else
                {
                    DataPortal<ComparisonCompleteCommand> dp = new DataPortal<ComparisonCompleteCommand>();
                    ComparisonCompleteCommand command = new ComparisonCompleteCommand(
                        comp.ComparisonId,
                        windowComparisonComplete.Tag.ToString(),
                        Convert.ToInt32((windowComparisonComplete.ComboBoxComparisonComplete.SelectedItem as ComboBoxItem).Tag));
                    dp.ExecuteCompleted += (o, e2) =>
                    {
                        windowComparisonComplete.BusyLoadingComparisonCompletion.IsRunning = false;
                        if (e2.Error != null)
                        {
                            MessageBox.Show(e2.Error.Message);
                        }
                        else
                        {
                            MessageBox.Show("Complete. " + e2.Object.NumberAffected.ToString() + " records affected.");

                        }
                        windowComparisonComplete.Close();
                        Button btn = new Button();
                        btn.DataContext = comp;
                        cmdStatsComparison_Click(btn, e);
                        //windowComparisonStats.Close();
                    };
                    windowComparisonComplete.BusyLoadingComparisonCompletion.IsRunning = true;
                    dp.BeginExecute(command);
                }
            }
        }

        private void DocumentActions_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (DocumentActions.SelectedIndex == 2)
            {
                homeReviewStatisticsControl.RefreshStatistics();
            }
            else if (DocumentActions.SelectedIndex == 1)
            {
                refreshSources();
            }
        }

        private void RefreshItemList(object sender, ItemListRefreshEventArgs e)
        {
            SelectionCritieraItemList = e.selectionCriteria;
            LoadItemList();
            TextBlockShowing.Text = SelectionCritieraItemList.Description;
            DocumentListPane.SelectedIndex = 0;
        }

        private void ItemAttributeCrosstabListData_DataChanged(object sender, EventArgs e)
        {
            CslaDataProvider provider = ((CslaDataProvider)this.Resources["ItemAttributeCrosstabListData"]);
            if (provider.Error != null)
                MessageBox.Show(((Csla.Xaml.CslaDataProvider)sender).Error.Message);
            else
            {
                CrosstabsChart();
            }
        }

        public SelectionCriteria SelectionCritieraItemList = null;

        private void GetItemListData()
        {
            SelectionCritieraItemList = new SelectionCriteria();
            SelectionCritieraItemList.ListType = "StandardItemList";
            SelectionCritieraItemList.OnlyIncluded = true;
            SelectionCritieraItemList.ShowDeleted = false;
            SelectionCritieraItemList.AttributeSetIdList = "";
            SelectionCritieraItemList.PageNumber = 0;
            LoadItemList();
        }

        private void UpdateDocCount()
        {
            CslaDataProvider provider = ((CslaDataProvider)this.Resources["ItemListData"]);
            if ((provider != null) && (provider.Data != null))
            {
                TextBlockDocCount.Text = (provider.Data as ItemList).Count.ToString() + " documents loaded";
                if ((provider.Data as ItemList).PageCount > 1)
                {
                    TextBlockDocCount.Text += " (out of " + (provider.Data as ItemList).TotalItemCount.ToString() +
                        " in this list in total).";
                }
                else
                {
                    TextBlockDocCount.Text += ".";
                }
            }
        }

        private void CslaDataProviderItemList_DataChanged(object sender, EventArgs e)
        {
            CslaDataProvider provider = ((CslaDataProvider)this.Resources["ItemListData"]);
            
            if (provider.Error != null)
            {
                System.Windows.Browser.HtmlPage.Window.Alert(((Csla.Xaml.CslaDataProvider)sender).Error.Message);
            }
            if (provider.Data != null)
            {
                int I = Convert.ToInt32(windowColumnSelect.UpDownPageSize.Value);
                ItemList iL = provider.Data as ItemList;
                if (iL != null && I != iL.PageSize)
                {
                    iL.PageSize = I;
                    //BindingExpression exp = ItemsGridDataPager..GetBindingExpression(RadDataPager.SourceProperty);
                    ////exp.UpdateSource();
                    ////ItemsGridDataPager.SetBinding(RadDataPager.SourceProperty, null);
                    //Binding b = new Binding("Data");
                    
                    //ItemsGridDataPager.SetBinding(RadDataPager.SourceProperty, new Binding(exp.
                }
                UpdateDocCount();
                TextBoxItemsGridFind.Text = "";
                //ItemsGrid.FilterDescriptors.Clear();
                this.CustomFilterDescriptor.FilterValue = "";
                if ((provider.Data as ItemList).PageCount > 1)
                {
                    ItemsGrid.Columns[3].IsFilterable = false;
                    ItemsGrid.Columns[4].IsFilterable = false;
                    ItemsGrid.Columns[5].IsFilterable = false;
                    ItemsGrid.Columns[6].IsFilterable = false;
                    ItemsGrid.Columns[7].IsFilterable = false;
                    ItemsGrid.Columns[8].IsFilterable = false;
                    ItemsGrid.Columns[9].IsFilterable = false;
                }
                else
                {
                    ItemsGrid.Columns[3].IsFilterable = true;
                    ItemsGrid.Columns[4].IsFilterable = true;
                    ItemsGrid.Columns[5].IsFilterable = true;
                    ItemsGrid.Columns[6].IsFilterable = true;
                    ItemsGrid.Columns[7].IsFilterable = true;
                    ItemsGrid.Columns[8].IsFilterable = true;
                    ItemsGrid.Columns[9].IsFilterable = true;
                }
                // so that the 'score' column is visible when items have a rank (usually from machine learning classification
                // and also from full text search)
                ItemList il = provider.Data as ItemList;
                if (il.Count > 0)
                {
                    if (il[0].Rank > 0)
                    {
                        ItemsGrid.Columns[12].IsVisible = true;
                    }
                }
            }
        }
        private void windowReconcile_HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            HyperlinkButton hbt = sender as HyperlinkButton ;
            if (hbt == null || hbt.Tag == null) return;
            Item itm = hbt.Tag as Item;
            if (itm == null) return;
            ItemsGrid.SelectedItem = itm;
            Button bt = new Button();
            bt.DataContext = itm;
            windowReconcile.Close();
            cmdEditCoding_Click(bt, e);
        }
        private void windowReconcile_ItemsGridDataPager_PageIndexChanging(object sender, PageIndexChangingEventArgs e)
        {
            windowReconcile.ItemsGrid.IsEnabled = false;
            windowReconcile.ItemsGrid.ItemsSource = null;
            windowReconcile.ItemsGrid.Items.Clear();
            CslaDataProvider provider = this.Resources["ItemListData"] as CslaDataProvider;
            provider.DataChanged += new EventHandler(ItemListData_DataChanged_1_Off);
            ItemsGridDataPager_PageIndexChanging(sender, e);
        }
        private void ItemsGridDataPager_PageIndexChanging(object sender, PageIndexChangingEventArgs e)
        {
            SelectionCritieraItemList.PageNumber = e.NewPageIndex;
            LoadItemList();
        }
        /// <summary>
        /// refreshes the current list, keeping selection criteria unaltered
        /// </summary>
        public void LoadItemList()
        {
            CslaDataProvider provider = this.Resources["ItemListData"] as CslaDataProvider;
            provider.FactoryParameters.Clear();
            SelectionCritieraItemList.PageSize = Convert.ToInt32(windowColumnSelect.UpDownPageSize.Value);
            ItemsGrid.Columns[11].IsVisible = SelectionCritieraItemList.ShowInfoColumn && windowColumnSelect.cbDataColumnAdditionalText.IsChecked == true;
            ItemsGrid.Columns[12].IsVisible = SelectionCritieraItemList.ShowScoreColumn && windowColumnSelect.cbDataColumnScore.IsChecked == true;
            provider.FactoryParameters.Add(SelectionCritieraItemList);
            provider.FactoryMethod = "GetItemList";
            provider.Refresh();
        }

        private void cmdColumnSelect_Click(object sender, RoutedEventArgs e)
        {
            windowColumnSelect.cbDataColumnId.IsChecked = ItemsGrid.Columns[3].IsVisible; //DataColumnId.IsVisible;
            windowColumnSelect.cbDataColumnOldId.IsChecked = ItemsGrid.Columns[4].IsVisible;
            windowColumnSelect.cbDataColumnAuthors.IsChecked = ItemsGrid.Columns[5].IsVisible;
            windowColumnSelect.cbDataColumnTitle.IsChecked = ItemsGrid.Columns[6].IsVisible;
            windowColumnSelect.cbDataColumnJournal.IsChecked = ItemsGrid.Columns[7].IsVisible;
            windowColumnSelect.cbDataColumnShortTitle.IsChecked = ItemsGrid.Columns[8].IsVisible;
            windowColumnSelect.cbDataColumnItemType.IsChecked = ItemsGrid.Columns[9].IsVisible;
            windowColumnSelect.cbDataColumnYear.IsChecked = ItemsGrid.Columns[10].IsVisible;
            windowColumnSelect.cbDataColumnAdditionalText.IsChecked = ItemsGrid.Columns[11].IsVisible;
            windowColumnSelect.cbDataColumnScore.IsChecked = ItemsGrid.Columns[12].IsVisible;
            windowColumnSelect.ShowDialog();
        }

        private void cmdCloseWindowColumnSelect_Click(object sender, RoutedEventArgs e)
        {
            ItemsGrid.Columns[3].IsVisible = windowColumnSelect.cbDataColumnId.IsChecked == true;
            ItemsGrid.Columns[4].IsVisible = windowColumnSelect.cbDataColumnOldId.IsChecked == true;
            ItemsGrid.Columns[5].IsVisible = windowColumnSelect.cbDataColumnAuthors.IsChecked == true;
            ItemsGrid.Columns[6].IsVisible = windowColumnSelect.cbDataColumnTitle.IsChecked == true;
            ItemsGrid.Columns[7].IsVisible = windowColumnSelect.cbDataColumnJournal.IsChecked == true;
            ItemsGrid.Columns[8].IsVisible = windowColumnSelect.cbDataColumnShortTitle.IsChecked == true;
            ItemsGrid.Columns[9].IsVisible = windowColumnSelect.cbDataColumnItemType.IsChecked == true;
            ItemsGrid.Columns[10].IsVisible = windowColumnSelect.cbDataColumnYear.IsChecked == true;
            ItemsGrid.Columns[11].IsVisible = windowColumnSelect.cbDataColumnAdditionalText.IsChecked == true;
            ItemsGrid.Columns[12].IsVisible = windowColumnSelect.cbDataColumnScore.IsChecked == true;
            CslaDataProvider provider = ((CslaDataProvider)this.Resources["ItemListData"]);
            ItemList iL = provider.Data as ItemList;
            int I = Convert.ToInt32(windowColumnSelect.UpDownPageSize.Value);
            if (iL != null && I != iL.PageSize)
            {
                iL.PageSize = I;
                SelectionCritieraItemList.PageSize = I;
                provider.Refresh();
            }
            windowColumnSelect.Close();
        }

        private void TextBoxItemsGridFind_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBoxItemsGridFind.SelectAll();
        }

        private void TextBoxItemsGridFind_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.CustomFilterDescriptor.FilterValue = this.TextBoxItemsGridFind.Text;
            this.TextBoxItemsGridFind.Focus();
            ItemsGrid_Filtered(sender, null);
        }

        private CustomFilterDescriptor customFilterDescriptor;
        
        public CustomFilterDescriptor CustomFilterDescriptor
        {
            get
            {
                if (this.customFilterDescriptor == null)
                {
                    this.customFilterDescriptor = new CustomFilterDescriptor(this.ItemsGrid.Columns.OfType<GridViewDataColumn>());
                    this.ItemsGrid.FilterDescriptors.Add(this.customFilterDescriptor);
                }
                return this.customFilterDescriptor;
            }
        }

        private void cmdWindowItemsGridWarningClose_Click(object sender, RoutedEventArgs e)
        {
            windowItemsGridSelectWarning.Close();
        }

        private void cmdExportCrosstabs_Click(object sender, RoutedEventArgs e)
        {
            string extension = "";
            ExportFormat format = ExportFormat.Html;

            RadComboBoxItem comboItem = ComboBoxExportCrosstabs.SelectedItem as RadComboBoxItem;
            string selectedItem = comboItem.Content.ToString();

            switch (selectedItem)
            {
                case "Excel": extension = "xls";
                    format = ExportFormat.Html;
                    break;
                case "ExcelML": extension = "xml";
                    format = ExportFormat.ExcelML;
                    break;
                case "Word": extension = "doc";
                    format = ExportFormat.Html;
                    break;
                case "Csv": extension = "csv";
                    format = ExportFormat.Csv;
                    break;
            }    

            SaveFileDialog dialog = new SaveFileDialog();
			dialog.DefaultExt = extension;
            dialog.Filter = String.Format("{1} files (*.{0})|*.{0}|All files (*.*)|*.*", extension, selectedItem);
			dialog.FilterIndex = 1;

            if (dialog.ShowDialog() == true)
            {
                using (Stream stream = dialog.OpenFile())
                {
                    GridViewExportOptions exportOptions = new GridViewExportOptions();
                    exportOptions.Format = format;
                    //exportOptions.ShowColumnFooters = true;
                    exportOptions.ShowColumnHeaders = true;
                    //exportOptions.ShowGroupFooters = true;

                    ItemAttributeCrosstabsGrid.Export(stream, exportOptions);
                }
            }
        }

        private void cmdExportFrequencies_Click(object sender, RoutedEventArgs e)
        {
            string extension = "";
            ExportFormat format = ExportFormat.Html;

            RadComboBoxItem comboItem = ComboBoxExportFrequencies.SelectedItem as RadComboBoxItem;
            string selectedItem = comboItem.Content.ToString();

            switch (selectedItem)
            {
                case "Excel": extension = "xls";
                    format = ExportFormat.Html;
                    break;
                case "ExcelML": extension = "xml";
                    format = ExportFormat.ExcelML;
                    break;
                case "Word": extension = "doc";
                    format = ExportFormat.Html;
                    break;
                case "Csv": extension = "csv";
                    format = ExportFormat.Csv;
                    break;
            }

            SaveFileDialog dialog = new SaveFileDialog();
            dialog.DefaultExt = extension;
            dialog.Filter = String.Format("{1} files (*.{0})|*.{0}|All files (*.*)|*.*", extension, selectedItem);
            dialog.FilterIndex = 1;

            if (dialog.ShowDialog() == true)
            {
                using (Stream stream = dialog.OpenFile())
                {
                    GridViewExportOptions exportOptions = new GridViewExportOptions();
                    exportOptions.Format = format;
                    //exportOptions.ShowColumnFooters = true;
                    exportOptions.ShowColumnHeaders = true;
                    //exportOptions.ShowGroupFooters = true;
                    //object o = Telerik.Windows.Data.TypeExtensions.DefaultValue;
                    ItemAttributeFrequenciesGrid.Columns[2].IsVisible = false;
                    ItemAttributeFrequenciesGrid.Export(stream, exportOptions);
                    ItemAttributeFrequenciesGrid.Columns[2].IsVisible = true;
                }
            }
        }
        private void cmdExportSourcesStats_Click(object sender, RoutedEventArgs e)
        {
            string extension = "xls";
            ExportFormat format = ExportFormat.Html;

            RadComboBoxItem comboItem = ComboBoxExportFrequencies.SelectedItem as RadComboBoxItem;
            
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.DefaultExt = extension;
            dialog.Filter = String.Format("{1} files (*.{0})|*.{0}|All files (*.*)|*.*", extension, "Excel");
            dialog.FilterIndex = 1;

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    using (Stream stream = dialog.OpenFile())
                    {
                        GridViewExportOptions exportOptions = new GridViewExportOptions();
                        exportOptions.Format = format;
                        exportOptions.ShowColumnHeaders = true;
                        exportOptions.ShowColumnFooters = true;
                        grView0.Export(stream, exportOptions);
                    }
                }
                catch
                {
                    RadWindow.Alert("Could not save File." + Environment.NewLine + "Please ensure that you have selected a writable file.");
                }
            }
        }
        //private void btListSourcelessItems_Click(object sender, RoutedEventArgs e)
        //{
        //    SelectionCritieraItemList = new SelectionCriteria();
        //    SelectionCritieraItemList.OnlyIncluded = false;// included ignore for sources
        //    SelectionCritieraItemList.ShowDeleted = true; // deleted ignore for sources
        //    SelectionCritieraItemList.AttributeSetIdList = "";
        //    SelectionCritieraItemList.SourceId = -1;
        //    SelectionCritieraItemList.ListType = "StandardItemList";
        //    TextBlockShowing.Text = "Showing: source-less Items";
        //    LoadItemList();
        //}

        private void windowDuplicates_Closed(object sender, WindowClosedEventArgs e)
        {
            if (HasWriteRights) LoadItemList();
        }

        private void RadioTermine_Click(object sender, RoutedEventArgs e)
        {
            //RadWindow wnd = new RadWindow();
            RadWindow.Alert("Termine functionality is provided with permission"
                        + Environment.NewLine +"from the National Centre for Text Mining, Manchester."
                        + Environment.NewLine + "Many thanks to NaCTeM for making it available.");
            //wnd.ShowDialog();
            //MessageBox.Show("Termine functionality is provided with permission from the National Centre for Text Mining, Manchester. Many thanks to NaCTeM for making it available.");
        }

        private void cmdBibliography_Click(object sender, RadRoutedEventArgs e)
        {
            RadMenuItem cbi = e.Source as RadMenuItem;
            if (cbi == null) return;
            else if (cbi.Tag.ToString() == "JustTheGrid")
            {
                cmdExportItemsGrid_Click(sender, e);
                return;
            }
            if (ItemsGrid.SelectedItems.Count > 0)
            {
                
                CslaDataProvider provider = App.Current.Resources["ReviewInfoData"] as CslaDataProvider;
                if (provider != null && provider.Data != null)
                {
                    ReviewInfo review = provider.Data as ReviewInfo;
                    if (review != null)
                    {
                        
                        //if (cbi != null)
                        //{ //already checked
                        string report = "";
                        if (cbi.Tag.ToString() == "BL")
                        {
                            report = review.BL_ACCOUNT_CODE + Environment.NewLine +
                                review.BL_AUTH_CODE + Environment.NewLine + Environment.NewLine + Environment.NewLine + Environment.NewLine + Environment.NewLine;
                        }
                        if (cbi.Tag.ToString() == "BLCopyrightCleared")
                        {
                            report = review.BL_CC_ACCOUNT_CODE + Environment.NewLine +
                                review.BL_CC_AUTH_CODE + Environment.NewLine + Environment.NewLine + Environment.NewLine + Environment.NewLine + Environment.NewLine;
                        }
                        foreach (Item i in ItemsGrid.SelectedItems)
                        {
                            switch (cbi.Tag.ToString())
                            {
                                case "Chicago":
                                    report += "<p>" + i.GetCitation() + "</p>";
                                    break;
                                case "Harvard":
                                    report += "<p>" + i.GetHarvardCitation() + "</p>";
                                    break;
                                case "NICE":
                                    report += "<p>" + i.GetNICECitation() + "</p>";
                                    break;
                                case "BL":
                                    report += review.BL_TX + Environment.NewLine +
                                        i.GetBritishLibraryCitation() + Environment.NewLine + Environment.NewLine + Environment.NewLine + Environment.NewLine + Environment.NewLine;
                                    break;
                                case "BLCopyrightCleared":
                                    report += review.BL_CC_TX + Environment.NewLine +
                                        i.GetBritishLibraryCitation() + Environment.NewLine + Environment.NewLine + Environment.NewLine + Environment.NewLine + Environment.NewLine;
                                    break;
                            }
                        }
                        if (cbi.Tag.ToString() == "BL" || cbi.Tag.ToString() == "BLCopyrightCleared")
                        {
                            report += "NNNN" + Environment.NewLine;
                            reportViewerControlDocuments.SetContent("<html><body>" + report.Replace(Environment.NewLine, @"<br />") + "</body></html>");
                        }
                        else
                        {
                            reportViewerControlDocuments.SetContent("<html><body>" + report + "</body></html>");
                        }
                        windowReportsDocuments.ShowDialog();
                        //}
                    }
                }

                else
                {
                    RadWindow.Alert("Sorry there is a confirguation error." + Environment.NewLine
                        + "This is sometimes caused by inconsistent settings" + Environment.NewLine
                        + "in the screening tab." + Environment.NewLine
                        + "Please check your screening preferences and try again." );
                }
            }
            else
            {
                RadWindow.Alert("No references currently selected.");
            }
        }
            

        private void cmdSearch_Click(object sender, RoutedEventArgs e)
        {
            dialogSearchControlDocuments.SetupDialog((sender as Button).Tag.ToString());
            windowSearchDocuments.ShowDialog();
        }

        private void cmdSearchLikeThese_Click(object sender, RoutedEventArgs e)
        {
            windowFindLikeThese.ShowDialog();
        }

        void dialogSearchControlDocuments_CloseWindowRequest(object sender, EventArgs e)
        {
            windowSearchDocuments.Close();
        }

        private void TermSearchComboSearchScope_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (windowFindLikeThese.TermSearchComboSearchScope != null)
            {
                switch (windowFindLikeThese.TermSearchComboSearchScope.SelectedIndex)
                {
                    case 0:
                        windowFindLikeThese.codesSelectControlTermSearch.Visibility = Visibility.Collapsed;
                        break;
                    case 1:
                        windowFindLikeThese.codesSelectControlTermSearch.Visibility = Visibility.Visible;
                        break;
                    case 2:
                        windowFindLikeThese.codesSelectControlTermSearch.Visibility = Visibility.Visible;
                        break;
                }
            }
        }

        private void cmdShowCodingAssignmentWindow_Click(object sender, RoutedEventArgs e)
        {
            WindowShowCodingAssignment.ShowDialog();
        }

        private void cmdCancelAssignWork_Click(object sender, RoutedEventArgs e)
        {
            WindowShowCodingAssignment.Close();
        }

        private void codesTreeControl_AssignSearchSelected(object sender, EventArgs e)
        {
            string confirmString;
            if (SearchGrid.SelectedItems.Count > 1)
            {
                confirmString = "Are you sure you want to assign all the items in these searches to this code?";
            }
            else
            {
                confirmString = "Are you sure you want to assign all the items in this search to this code?";
            }
            if (MessageBox.Show(confirmString, "Confirm assignment", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                AttributeSet attributeSet = codesTreeControl.SelectedAttributeSet();
                string SearchList = "";
                foreach (Search search in SearchGrid.SelectedItems)
                {
                    if (SearchList == "")
                    {
                        SearchList = search.SearchId.ToString();
                    }
                    else
                    {
                        SearchList += "," + search.SearchId.ToString();
                    }
                }
                if (attributeSet != null)
                {
                    if (SearchList != "")
                    {
                        DataPortal<ItemAttributeBulkSaveCommand> dp = new DataPortal<ItemAttributeBulkSaveCommand>();
                        ItemAttributeBulkSaveCommand command = new ItemAttributeBulkSaveCommand("Insert",
                            attributeSet.AttributeId, attributeSet.SetId, "", SearchList);
                        dp.ExecuteCompleted += (o, e2) =>
                        {
                            if (e2.Error != null)
                                MessageBox.Show(e2.Error.Message);
                            BusyPleaseWait.IsRunning = false;
                            windowPleaseWait.Close();
                        };
                        BusyPleaseWait.IsRunning = true;
                        windowPleaseWait.ShowDialog();
                        dp.BeginExecute(command);
                    }
                    else
                    {
                        MessageBox.Show("No searches selected");
                    }
                }
                else
                {
                    MessageBox.Show("No code selected");
                }
            }
        }

        private void codesTreeControl_RemoveSearchSelected(object sender, EventArgs e)
        {
            string confirmString;
            if (SearchGrid.SelectedItems.Count > 1)
            {
                confirmString = "Are you sure you want to remove all the items in these searches from this code?";
            }
            else
            {
                confirmString = "Are you sure you want to remove all the items in this search from this code?";
            }
            if (MessageBox.Show(confirmString, "Confirm removal", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                AttributeSet attributeSet = codesTreeControl.SelectedAttributeSet();
                string SearchList = "";
                foreach (Search search in SearchGrid.SelectedItems)
                {
                    if (SearchList == "")
                    {
                        SearchList = search.SearchId.ToString();
                    }
                    else
                    {
                        SearchList += "," + search.SearchId.ToString();
                    }
                }
                if (attributeSet != null)
                {
                    if (SearchList != "")
                    {
                        DataPortal<ItemAttributeBulkDeleteCommand> dp = new DataPortal<ItemAttributeBulkDeleteCommand>();
                        ItemAttributeBulkDeleteCommand command = new ItemAttributeBulkDeleteCommand(
                            attributeSet.AttributeId,
                            "",
                            attributeSet.SetId, 
                            SearchList);
                        dp.ExecuteCompleted += (o, e2) =>
                        {
                            if (e2.Error != null)
                                MessageBox.Show(e2.Error.Message);
                            BusyPleaseWait.IsRunning = false;
                            windowPleaseWait.Close();
                        };
                        BusyPleaseWait.IsRunning = true;
                        windowPleaseWait.ShowDialog();
                        dp.BeginExecute(command);
                    }
                    else
                    {
                        MessageBox.Show("No searches selected");
                    }
                }
                else
                {
                    MessageBox.Show("No code selected");
                }
            }
        }

        private void SourcesList_DataLoaded(object sender, EventArgs e)
        {

        }

        private void homeReportsControl_ReportTypeChanged(object sender, EventArgs e)
        {
            codesTreeControl.ShowReportMenuOptions = homeReportsControl.activeReport.ReportType;
            codesTreeControl.SetReportAppearance(true);
        }

        private void homeReportsControl_OpenReportWindowCommand(object sender, EventArgs e)
        {
            cmdOpenReportsWindow_Click(sender, null);
        }

        private void dialogReportsControl_CloseWindowRequest(object sender, EventArgs e)
        {
            windowReports.Close();
        }

        private void cmdCancelAssignDocuments_Click(object sender, RoutedEventArgs e)
        {
            windowAssignDocuments.Close();
        }

        private void ComboSelectAssignmentMethod_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (windowAssignDocuments.ComboSelectAssignmentMethod != null)
            {
                if (windowAssignDocuments.ComboSelectAssignmentMethod.SelectedIndex == 0)
                {
                    windowAssignDocuments.GridDocumentAssignment.RowDefinitions[1].MaxHeight = 0;
                }
                else
                {
                    windowAssignDocuments.GridDocumentAssignment.RowDefinitions[1].MaxHeight = 40;
                }
            }
        }

        private void cmdAssignDocuments_Click(object sender, RoutedEventArgs e)
        {
            DataPortal<ItemIncludeExcludeCommand> dp = new DataPortal<ItemIncludeExcludeCommand>();
            ItemIncludeExcludeCommand command = new ItemIncludeExcludeCommand(
               windowAssignDocuments.RadioAssignDocumentIncluded.IsChecked == true,
                windowAssignDocuments.ComboSelectAssignmentMethod.SelectedIndex == 0 ? ItemsGridSelectedItems() : "",
                windowAssignDocuments.codesSelectControlAssignSelect.SelectedAttributeSet() != null ? windowAssignDocuments.codesSelectControlAssignSelect.SelectedAttributeSet().AttributeId : 0,
                windowAssignDocuments.codesSelectControlAssignSelect.SelectedAttributeSet() != null ? windowAssignDocuments.codesSelectControlAssignSelect.SelectedAttributeSet().SetId : 0);
            dp.ExecuteCompleted += (o, e2) =>
            {
                BusyPleaseWait.IsRunning = false;
                windowPleaseWait.Close();
                if (windowAssignDocuments.ComboSelectAssignmentMethod.SelectedIndex == 0)
                {
                    foreach (Item item in ItemsGrid.SelectedItems)
                    {
                        if (!(item.IsItemDeleted == true && item.IsIncluded == true))
                        {
                            if (windowAssignDocuments.RadioAssignDocumentIncluded.IsChecked == true)
                            {
                                item.IsIncluded = true;
                                item.IsItemDeleted = false;
                            }
                            else
                            {
                                item.IsIncluded = false;
                                item.IsItemDeleted = false;
                            }
                        }
                    }
                }
                else
                {
                    LoadItemList(); // just refreshes using the exising list object
                }
                if (e2.Error != null)
                {
                    MessageBox.Show(e2.Error.Message);
                }
                windowAssignDocuments.Close();
            };
            windowPleaseWait.ShowDialog();
            BusyPleaseWait.IsRunning = true;
            dp.BeginExecute(command);
        }

        private void GridViewMetaAnalyses_SelectionChanged(object sender, SelectionChangeEventArgs e)
        {
            while (GridViewMetaAnalyses.SelectedItems.Count > 2)
            {
                GridViewMetaAnalyses.SelectedItems.RemoveAt(0);
            }
        }

        private void cmdMetaSubGroup_Click(object sender, RoutedEventArgs e)
        {
            // sorry this is a bit clunky (i.e. loading the two sets of outcomes, one after another, but at least it's reliable!
            windowMetaAnalysisOptions.GridMAOptions.DataContext = null;

            if (GridViewMetaAnalyses.SelectedItems.Count == 2)
            {
                MetaAnalysis ma1 = GridViewMetaAnalyses.SelectedItems[0] as MetaAnalysis;
                MetaAnalysis ma2 = GridViewMetaAnalyses.SelectedItems[1] as MetaAnalysis;
                if (ma1 != null || ma2 != null)
                {
                    DataPortal<OutcomeList> dp = new DataPortal<OutcomeList>();
                    dp.FetchCompleted += (o, e2) =>
                    {
                        radBusyEditMAIndicator.IsBusy = false;
                        if (e2.Error != null)
                        {
                            MessageBox.Show(e2.Error.Message);
                        }
                        else
                        {
                            ma1.Outcomes = e2.Object as OutcomeList;
                            DataPortal<OutcomeList> dp2 = new DataPortal<OutcomeList>();
                            dp2.FetchCompleted += (o2, e22) =>
                            {
                                radBusyEditMAIndicator.IsBusy = false;
                                if (e22.Error != null)
                                {
                                    MessageBox.Show(e22.Error.Message);
                                }
                                else
                                {
                                    ma2.Outcomes = e22.Object as OutcomeList;
                                    windowMetaAnalysisOptions.ShowDialog();
                                }
                            };
                            radBusyEditMAIndicator.IsBusy = true;
                            dp2.BeginFetch(new BusinessLibrary.BusinessClasses.OutcomeList.OutcomeListSelectionCriteria(typeof(OutcomeList), ma2.SetId, ma2.AttributeIdIntervention,
                                ma2.AttributeIdControl, ma2.AttributeIdOutcome, 0, ma2.MetaAnalysisId, ma2.AttributeIdQuestion, ma2.AttributeIdAnswer));
                        }
                    };
                    radBusyEditMAIndicator.IsBusy = true;
                    dp.BeginFetch(new BusinessLibrary.BusinessClasses.OutcomeList.OutcomeListSelectionCriteria(typeof(OutcomeList), ma1.SetId, ma1.AttributeIdIntervention,
                        ma1.AttributeIdControl, ma1.AttributeIdOutcome, 0, ma1.MetaAnalysisId, ma1.AttributeIdQuestion, ma1.AttributeIdAnswer));
                }
            }
            else
            {
                MessageBox.Show("Please select two meta-analyses first.");
            }
        }

        private void cmdWindowMetaAnalysisOptionsGo_Click(object sender, RoutedEventArgs e)
        {
            if (windowMetaAnalysisOptions.GridMAOptions.DataContext == null)
            {
                MetaAnalysis ma1 = GridViewMetaAnalyses.SelectedItems[0] as MetaAnalysis;
                MetaAnalysis ma2 = GridViewMetaAnalyses.SelectedItems[1] as MetaAnalysis;
                if (ma1 != null || ma2 != null)
                {
                    if (ma1.MetaAnalysisTypeId != ma2.MetaAnalysisTypeId)
                    {
                        MessageBox.Show("You can only run sub-group analyses using the same effect measures.");
                        return;
                    }
                    MetaAnalyse metaAnalyse = new MetaAnalyse();
                    metaAnalyse.SortBy = windowMetaAnalysisOptions.ComboBoxSortBy.Text;
                    metaAnalyse.MetaAnalysis1 = ma1;
                    metaAnalyse.MetaAnalysis2 = ma2;
                    metaAnalyse.ShowMetaLabels = windowMetaAnalysisOptions.cbShowMetaLabels.IsChecked == true;
                    metaAnalyse.ShowPooledES = windowMetaAnalysisOptions.cbShowMetaPooledEffectSize.IsChecked == true;
                    metaAnalyse.ShowSummaryLine = windowMetaAnalysisOptions.cbShowMetaSummaryLine.IsChecked == true;
                    metaAnalyse.LabelLHS = windowMetaAnalysisOptions.TextBoxMetaLHS.Text;
                    metaAnalyse.LabelRHS = windowMetaAnalysisOptions.TextBoxMetaRHS.Text;
                    metaAnalyse.SubGroupMA();
                    metaAnalyse.Saved += (o, e2) =>
                    {
                        if (e2.NewObject != null)
                        {
                            string MA1Title = ma1.Title;
                            string TitleText1 =
                                "Meta-analysis 1:" +  (ma1.OutcomeText != "" ? "Outcome: " + ma1.OutcomeText + ", " : "") +
                                (ma1.InterventionText != "" ? "Intervention: " + ma1.InterventionText + ", " : "") +
                                (ma1.ControlText != "" ? "Comparison: " + ma1.ControlText + ", " : "") +
                                (ma1.MetaAnalysisTypeTitle != "" ? "Measure - " + ma1.MetaAnalysisTypeTitle : "");
                            string MA2Title = ma2.Title;
                            string TitleText2 =
                                "Meta-analysis 2:" + (ma2.OutcomeText != "" ? "Outcome: " + ma2.OutcomeText + ", " : "") +
                                (ma2.InterventionText != "" ? "Intervention: " + ma2.InterventionText + ", " : "") +
                                (ma2.ControlText != "" ? "Comparison: " + ma2.ControlText + ", " : "") +
                                (ma2.MetaAnalysisTypeTitle != "" ? "Measure - " + ma2.MetaAnalysisTypeTitle : "");
                            
                            metaAnalyse = e2.NewObject as MetaAnalyse;
                            MemoryStream ms = new MemoryStream(metaAnalyse.feForestPlot);
                            MemoryStream ms2 = new MemoryStream(metaAnalyse.reForestPlot);

                            double iSquared = metaAnalyse.MetaAnalysisSubGroup.Q == 0 ? 0 : Math.Max(100.0 * (metaAnalyse.MetaAnalysisSubGroup.Q - (metaAnalyse.MetaAnalysisSubGroup.numStudies - 1)) / metaAnalyse.MetaAnalysisSubGroup.Q, 0);
                            double p2 = 1 - StatFunctions.pchisq(Convert.ToDouble(metaAnalyse.MetaAnalysisSubGroup.Q), Convert.ToDouble(metaAnalyse.MetaAnalysisSubGroup.numStudies - 1));

                            string feResults = "Fixed effect model overall effect: " + metaAnalyse.MetaAnalysisSubGroup.feEffect.ToString("G3") + " (" + metaAnalyse.MetaAnalysisSubGroup.feCiLower.ToString("G3") + ", " + metaAnalyse.MetaAnalysisSubGroup.feCiUpper.ToString("G3") + ")";
                            string heterogeneity = "Heterogeneity Q (all studies) = " + metaAnalyse.MetaAnalysisSubGroup.Q.ToString("G3") + "; df = " + (metaAnalyse.MetaAnalysisSubGroup.numStudies - 1).ToString() +
                                "; p = " + p2.ToString("G3") + "; I-squared = " + iSquared.ToString("G3") + "%" + 
                                ". (Group 1 Q = " + metaAnalyse.MetaAnalysis1.Q.ToString("G3") + "; df = " + (metaAnalyse.MetaAnalysis1.numStudies - 1).ToString() +
                                ". Group 2 Q = " + metaAnalyse.MetaAnalysis2.Q.ToString("G3") + "; df = " + (metaAnalyse.MetaAnalysis2.numStudies - 1).ToString() + ")";
                            //string fileDrawer = "File drawer N = " + Math.Abs(metaAnalyse.MetaAnalysisSubGroup.FileDrawerZ).ToString("G3");
                            string reResults = "Random effects model overall effect: " + metaAnalyse.MetaAnalysisSubGroup.reEffect.ToString("G3") + " (" + metaAnalyse.MetaAnalysisSubGroup.reCiLower.ToString("G3") + ", " + metaAnalyse.MetaAnalysisSubGroup.reCiUpper.ToString("G3") + ")";

                            string feDifference = metaAnalyse.SubGroupFeDifference();
                            string reDifference = metaAnalyse.SubGroupReDifference();
                            
                            //DevExpress.XtraRichEdit.API.Native.DocumentImageSource feFP = DevExpress.XtraRichEdit.API.Native.DocumentImageSource.FromStream(ms);
                            //DevExpress.XtraRichEdit.API.Native.DocumentImageSource reFP = DevExpress.XtraRichEdit.API.Native.DocumentImageSource.FromStream(ms2);

                            reportViewerControlDocuments.DisplaySubGroupAnalysis(MA1Title, MA2Title, TitleText1, TitleText2, feResults, reResults,
                                feDifference, reDifference, heterogeneity, ms, ms2);
                            windowReportsDocuments.ShowDialog();
                            BusyPleaseWait.IsRunning = false;
                            windowPleaseWait.Close();
                        }
                    };
                    BusyPleaseWait.IsRunning = true;
                    windowPleaseWait.ShowDialog();
                    metaAnalyse.BeginSave(true);
                }
            }
            else
            {
                //dialogMetaAnalysisControl.RunMetaAnalysis(ma);
                //windowMetaAnalysis.ShowDialog();

                //this.DataContext = ma;
                MetaAnalysis ma = windowMetaAnalysisOptions.GridMAOptions.DataContext as MetaAnalysis;
                if (ma != null)
                {
                    string maTitle = ma.Title;
                    string TitleText =
                        (ma.OutcomeText != "" ? "Outcome: " + ma.OutcomeText + ", " : "") +
                        (ma.InterventionText != "" ? "Intervention: " + ma.InterventionText + ", " : "") +
                        (ma.ControlText != "" ? "Comparison: " + ma.ControlText + ", " : "") +
                        (ma.MetaAnalysisTypeTitle != "" ? "Measure: " + ma.MetaAnalysisTypeTitle : "");
                    MetaAnalyse metaAnalyse = new MetaAnalyse();
                    metaAnalyse.SortBy = windowMetaAnalysisOptions.ComboBoxSortBy.Text;
                    metaAnalyse.MetaAnalysis1 = ma;
                    metaAnalyse.MetaAnalysis2 = null;
                    metaAnalyse.ShowMetaLabels = windowMetaAnalysisOptions.cbShowMetaLabels.IsChecked == true;
                    metaAnalyse.ShowPooledES = windowMetaAnalysisOptions.cbShowMetaPooledEffectSize.IsChecked == true;
                    metaAnalyse.ShowSummaryLine = windowMetaAnalysisOptions.cbShowMetaSummaryLine.IsChecked == true;
                    metaAnalyse.LabelLHS = windowMetaAnalysisOptions.TextBoxMetaLHS.Text;
                    metaAnalyse.LabelRHS = windowMetaAnalysisOptions.TextBoxMetaRHS.Text;
                    ma.run(-1);

                    double iSquared = ma.Q == 0 ? 0 : Math.Max(100.0 * (ma.Q - (ma.numStudies - 1)) / ma.Q, 0);
                    double p2 = 1 - StatFunctions.pchisq(Convert.ToDouble(ma.Q), Convert.ToDouble(ma.numStudies - 1));

                    string feResults = "Fixed effect model: " + ma.feEffect.ToString("G3") + " (" + ma.feCiLower.ToString("G3") + ", " + ma.feCiUpper.ToString("G3") + ")";
                    string heterogeneity = "Heterogeneity: Q = " + ma.Q.ToString("G3") + "; df = " + (ma.numStudies - 1).ToString() +
                        "; p = " + p2.ToString("G3") + "; I-squared = " + iSquared.ToString("G3") + "%; tau-squared = " + ma.tauSquared.ToString("G3");
                    //string fileDrawer = "File drawer N = " + Math.Abs(ma.FileDrawerZ).ToString("G3");
                    string reResults = "Random effects model: " + ma.reEffect.ToString("G3") + " (" + ma.reCiLower.ToString("G3") + ", " + ma.reCiUpper.ToString("G3") + ")";

                    metaAnalyse.Saved += (o, e2) =>
                    {
                        if (e2.NewObject != null)
                        {
                            metaAnalyse = e2.NewObject as MetaAnalyse;
                            MemoryStream ms = new MemoryStream(metaAnalyse.feForestPlot);
                            MemoryStream ms2 = new MemoryStream(metaAnalyse.reForestPlot);
                            MemoryStream ms3 = new MemoryStream(metaAnalyse.feFunnelPlot);
                            //DevExpress.XtraRichEdit.API.Native.DocumentImageSource feFP = DevExpress.XtraRichEdit.API.Native.DocumentImageSource.FromStream(ms);
                            //DevExpress.XtraRichEdit.API.Native.DocumentImageSource reFP = DevExpress.XtraRichEdit.API.Native.DocumentImageSource.FromStream(ms2);
                            //DevExpress.XtraRichEdit.API.Native.DocumentImageSource FP = DevExpress.XtraRichEdit.API.Native.DocumentImageSource.FromStream(ms3);
                            //reportViewerControlDocuments.DisplayMetaAnalysis(maTitle, TitleText, feResults, reResults, heterogeneity, feFP, reFP, FP);
                            reportViewerControlDocuments.DisplayMetaAnalysis(maTitle, TitleText, feResults, reResults, heterogeneity, ms, ms2, ms3);
                            BusyPleaseWait.IsRunning = false;
                            windowPleaseWait.Close();
                            windowReportsDocuments.ShowDialog();
                        }
                    };
                    BusyPleaseWait.IsRunning = true;
                    windowPleaseWait.ShowDialog();
                    metaAnalyse.BeginSave(true);
                }
            }
        }

        private void cmdWindowMetaAnalysisOptionsClose_Click(object sender, RoutedEventArgs e)
        {
            windowMetaAnalysisOptions.Close();
        }

        private void cbShowMetaLabels_Click(object sender, RoutedEventArgs e)
        {
            if (windowMetaAnalysisOptions.cbShowMetaLabels.IsChecked == true)
            {
                windowMetaAnalysisOptions.TextBoxMetaLHS.IsEnabled = true;
                windowMetaAnalysisOptions.TextBoxMetaRHS.IsEnabled = true;
            }
            else
            {
                windowMetaAnalysisOptions.TextBoxMetaLHS.IsEnabled = false;
                windowMetaAnalysisOptions.TextBoxMetaRHS.IsEnabled = false;
            }
        }

        private void cmdOpenWindowItemReportWriter_Click(object sender, RoutedEventArgs e)
        {
            if (ItemsGrid.SelectedItems.Count == 0 )
            {
                MessageBox.Show("Please select one or more documents below");
                return;
            }
            dialogItemReportWriterControl.SetupItemReportWriter(ItemsGrid.SelectedItems);
            windowItemReportWriter.ShowDialog();
        }

        private void dialogItemReportWriterControl_LaunchReportViewer(object sender, LaunchReportViewerEventArgs e)
        {
            reportViewerControlDocuments.SetContent(e.ReportText);
            windowReportsDocuments.ShowDialog();
        }

        private void TrainingScreeningCriteriaData_DataChanged(object sender, EventArgs e)
        {
            CslaDataProvider provider = ((CslaDataProvider)this.Resources["TrainingScreeningCriteriaData"]);
            if (provider.Error != null)
            {
                System.Windows.Browser.HtmlPage.Window.Alert(((Csla.Xaml.CslaDataProvider)sender).Error.Message);
            }
        }

        private void cmdAddAttributeToScreeningCriteria_Click(object sender, RoutedEventArgs e)
        {
            AttributeSet attribute = codesTreeControl.SelectedAttributeSet();
            CslaDataProvider provider = ((CslaDataProvider)this.Resources["TrainingScreeningCriteriaData"]);
            if (attribute != null && provider != null)
            {
                TrainingScreeningCriteriaList thelist = provider.Data as TrainingScreeningCriteriaList;
                TrainingScreeningCriteria tsc = new TrainingScreeningCriteria();
                tsc.AttributeId = attribute.AttributeId;
                tsc.Included = true;
                tsc.AttributeName = attribute.AttributeName;
                foreach (TrainingScreeningCriteria element in thelist)
                {//we should not try adding an element that is already there!
                    if (element.AttributeId == tsc.AttributeId) return;
                }
                thelist.Add(tsc);
                tsc.BeginEdit();
                tsc.ApplyEdit();
            }
        }

        private void cmdDeleteTrainingScreeningCriteria_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to delete this code?", "Confirm delete", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                TrainingScreeningCriteria tsc;
                tsc = (sender as Button).DataContext as TrainingScreeningCriteria;
                tsc.BeginEdit();
                tsc.Delete();
                tsc.ApplyEdit();
                tsc = null;
            }
        }

        private void cmdAddTrainingReviewerTerm_Click(object sender, RoutedEventArgs e)
        {
            TrainingReviewerTerm trt = new TrainingReviewerTerm();
            trt.ReviewerTerm = "Edit term";
            trt.Included = true;
            CslaDataProvider provider = ((CslaDataProvider)App.Current.Resources["TrainingReviewerTermData"]);
            if (provider != null)
            {
                (provider.Data as TrainingReviewerTermList).Add(trt);
                trt.BeginEdit();
                trt.ApplyEdit();
            }
        }

        private void cmdTrainingRunTraining_Click(object sender, RoutedEventArgs e)
        {
            CslaDataProvider provider = App.Current.Resources["ReviewInfoData"] as CslaDataProvider;
            ReviewInfo RevInfo = provider.Data as ReviewInfo;
            if (sender is Button)
            {
                if (MessageBox.Show("Are you sure you want to (re)create the list of items to screen?", "Create screening list?", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
                {
                    return;
                }
            }

            TrainingList training = ((CslaDataProvider)App.Current.Resources["TrainingListData"]).Data as TrainingList;
            DataPortal<TrainingRunCommand> dp = new DataPortal<TrainingRunCommand>();
            TrainingRunCommand command = new TrainingRunCommand();
            dp.ExecuteCompleted += (o, e2) =>
            {
                //BusyLoading.IsRunning = false;
                if (e2.Error != null)
                {
                    if (e2.Error.Message.Contains("has exceeded the allotted timeout") == true)
                    {
                        //RadWindow.Alert("Caught timeout exception");
                    }
                    else
                    {
                        RadWindow.Alert(e2.Error.Message);
                    }
                }
                else
                {
                    (App.Current.Resources["ReviewInfoData"] as CslaDataProvider).Refresh();
                    //if (sender is Button && (sender as Button).Name == "cmdTrainingRunTraining" && (e2.Object as TrainingRunCommand).ReportBack != "")
                    if (sender is Button && (sender as Button).Name == "cmdTrainingRunTraining")
                    {
                        //RadWindow.Alert("The list of items to screen has been created");
                    }
                    CslaDataProvider provider3 = App.Current.Resources["TrainingListData"] as CslaDataProvider;
                    if (provider3 != null)
                        provider3.Refresh();
                }
            };
            command.RevInfo = RevInfo;
            dp.BeginExecute(command);
            if (sender is Button && (sender as Button).Name == "cmdTrainingRunTraining")
            {
            RadWindow.Alert("The list is now being created.\n\rPlease note that this can take quite a long time\n\rPlease do not close EPPI-Reviewer while the list is being created");
        }
        }

        private void cmdTrainingBeginScreening_Click(object sender, RoutedEventArgs e)
        {
            CslaDataProvider provider = App.Current.Resources["ReviewInfoData"] as CslaDataProvider;
            if (provider == null) return;
            ReviewInfo ri = provider.Data as ReviewInfo;
            if (ri == null) return;
            if (ri.IsDirty)
            {
                RadWindow.Alert("It appears you have changed"
                                     + Environment.NewLine + "Screening Settings."
                                     + Environment.NewLine + "Please Save or Cancel your changes.");
                return;
            }
            if (ri.ScreeningIndexed == true)
            {
                dialogCodingControl.BindScreening();
                windowCoding.ShowDialog();
            }
            else
            {
                RadWindow.Alert("Please create the list of items to screen first");
            }
        }
        private void cmdProcessReviewerterms_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to process the terms?", "Process terms?", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                DataPortal<TrainingProcessTermsCommand> dp = new DataPortal<TrainingProcessTermsCommand>();
                TrainingProcessTermsCommand command = new TrainingProcessTermsCommand();
                dp.ExecuteCompleted += (o, e2) =>
                {
                    //BusyLoading.IsRunning = false;
                    if (e2.Error != null)
                    {
                        RadWindow.Alert(e2.Error.Message);
                    }
                    else
                    {
                        MessageBox.Show("Term processing complete");
                    }
                };
                //BusyLoading.IsRunning = true;
                dp.BeginExecute(command);
            }
        }

        private void ComboClusterWhat_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            UpdateGridWindowDocumentCluster();
        }

        private void UpdateGridWindowDocumentCluster()
        {
            if (windowDocumentCluster.GridWindowDocumentCluster != null)
            {
                switch (windowDocumentCluster.ComboClusterWhat.SelectedIndex)
                {
                    case 0:
                        windowDocumentCluster.GridWindowDocumentCluster.RowDefinitions[7].MaxHeight = 0;
                        break;

                    case 1:
                        windowDocumentCluster.GridWindowDocumentCluster.RowDefinitions[7].MaxHeight = 0;
                        break;

                    case 2:
                        windowDocumentCluster.GridWindowDocumentCluster.RowDefinitions[7].MaxHeight = 35;
                        break;
                }
            }
        }

        private void cmdExportTermList_Click(object sender, RoutedEventArgs e)
        {
            string extension = "";
            ExportFormat format = ExportFormat.Html;

            RadComboBoxItem comboItem = windowFindLikeThese.ComboBoxExportTerms.SelectedItem as RadComboBoxItem;
            string selectedItem = comboItem.Content.ToString();

            switch (selectedItem)
            {
                case "Excel": extension = "xls";
                    format = ExportFormat.Html;
                    break;
                case "ExcelML": extension = "xml";
                    format = ExportFormat.ExcelML;
                    break;
                case "Word": extension = "doc";
                    format = ExportFormat.Html;
                    break;
                case "Csv": extension = "csv";
                    format = ExportFormat.Csv;
                    break;
            }

            SaveFileDialog dialog = new SaveFileDialog();
            dialog.DefaultExt = extension;
            dialog.Filter = String.Format("{1} files (*.{0})|*.{0}|All files (*.*)|*.*", extension, selectedItem);
            dialog.FilterIndex = 1;

            if (dialog.ShowDialog() == true)
            {
                using (Stream stream = dialog.OpenFile())
                {
                    GridViewExportOptions exportOptions = new GridViewExportOptions();
                    exportOptions.Format = format;
                    //exportOptions.ShowColumnFooters = true;
                    exportOptions.ShowColumnHeaders = true;
                    //exportOptions.ShowGroupFooters = true;
                    //object o = Telerik.Windows.Data.TypeExtensions.DefaultValue;
                    windowFindLikeThese.TermsGrid.Export(stream, exportOptions);
                }
            }
        }

        private void RadioButtonFrequenciesShowPie_Click(object sender, RoutedEventArgs e)
        {
            ItemAttributeFrequenciesGrid.Visibility = System.Windows.Visibility.Collapsed;
            FrequenciesPieChart.Visibility = System.Windows.Visibility.Visible;
            FrequenciesBarChart.Visibility = System.Windows.Visibility.Collapsed;
            cmdExportFrequencyChart.IsEnabled = true;
            cmdExportFrequencyChart.Tag = "Pie";
        }

        private void RadioButtonFrequenciesShowTable_Click(object sender, RoutedEventArgs e)
        {
            ItemAttributeFrequenciesGrid.Visibility = System.Windows.Visibility.Visible;
            FrequenciesPieChart.Visibility = System.Windows.Visibility.Collapsed;
            FrequenciesBarChart.Visibility = System.Windows.Visibility.Collapsed;
            cmdExportFrequencyChart.IsEnabled = false;
        }

        private void CheckBoxFrequenciesIncludeNoneOfTheAbove_Click(object sender, RoutedEventArgs e)
        {
            FrequencyChart();
        }

        private void RadioButtonFrequenciesShowBar_Click(object sender, RoutedEventArgs e)
        {
            ItemAttributeFrequenciesGrid.Visibility = System.Windows.Visibility.Collapsed;
            FrequenciesPieChart.Visibility = System.Windows.Visibility.Collapsed;
            FrequenciesBarChart.Visibility = System.Windows.Visibility.Visible;
            cmdExportFrequencyChart.IsEnabled = true;
            cmdExportFrequencyChart.Tag = "Bar";
        }

        private void cmdExportFrequencyChart_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.DefaultExt = "*.png";
            dialog.Filter = "Files(*.png)|*.png";
            if (!(bool)dialog.ShowDialog())
                return;
            Telerik.Windows.Media.Imaging.PngBitmapEncoder enc = new Telerik.Windows.Media.Imaging.PngBitmapEncoder();
            using (Stream fileStream = dialog.OpenFile())
            {
                if (cmdExportFrequencyChart.Tag.ToString() == "Pie")
                {
                    Telerik.Windows.Media.Imaging.ExportExtensions.ExportToImage(FrequenciesPieChart, fileStream, enc);
                }
                else
                {
                    Telerik.Windows.Media.Imaging.ExportExtensions.ExportToImage(FrequenciesBarChart, fileStream, enc);
                }
            }
            
        }

        private void RadioButtonCrosstabsShowTable_Click(object sender, RoutedEventArgs e)
        {
            //CrosstabsBarChart.Visibility = System.Windows.Visibility.Collapsed;
            GridCrosstabsChart.Visibility = System.Windows.Visibility.Collapsed;
            ItemAttributeCrosstabsGrid.Visibility = System.Windows.Visibility.Visible;
            CircularGraphCrosstabs.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void RadioButtonCrosstabsShowBar_Click(object sender, RoutedEventArgs e)
        {
            //CrosstabsBarChart.Visibility = System.Windows.Visibility.Visible;
            GridCrosstabsChart.Visibility = System.Windows.Visibility.Visible;
            ItemAttributeCrosstabsGrid.Visibility = System.Windows.Visibility.Collapsed;
            CircularGraphCrosstabs.Visibility = System.Windows.Visibility.Collapsed;

            RadCartesianChart rcc = this.CrosstabsBarChart;
            if (rcc != null && rcc.Series != null && rcc.Series.Count > 0)
            {
                foreach (BarSeries bs in rcc.Series)
                {
                    if (RadioButtonCrosstabsShowBar.IsChecked == true)
                    {
                        bs.CombineMode = ChartSeriesCombineMode.Cluster;
                    }
                    else
                        if (RadioButtonCrosstabsShowBarStacked.IsChecked == true)
                        {
                            bs.CombineMode = ChartSeriesCombineMode.Stack;
                        }
                        else
                            bs.CombineMode = ChartSeriesCombineMode.Stack100;
                }
            }
        }

        private void cmdExportCrosstabsChart_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.DefaultExt = "*.png";
            dialog.Filter = "Files(*.png)|*.png";
            if (!(bool)dialog.ShowDialog())
                return;
            Telerik.Windows.Media.Imaging.PngBitmapEncoder enc = new Telerik.Windows.Media.Imaging.PngBitmapEncoder();
            using (Stream fileStream = dialog.OpenFile())
            {
                if (CircularGraphCrosstabs.Visibility == System.Windows.Visibility.Visible)
                    Telerik.Windows.Media.Imaging.ExportExtensions.ExportToImage(CircularGraphCrosstabs, fileStream, enc);
                else
                    Telerik.Windows.Media.Imaging.ExportExtensions.ExportToImage(GridCrosstabsChart, fileStream, enc);
            }
        }

        private void RadioButtonCrosstabsShowCircularGraph_Click(object sender, RoutedEventArgs e)
        {
            GridCrosstabsChart.Visibility = System.Windows.Visibility.Collapsed;
            ItemAttributeCrosstabsGrid.Visibility = System.Windows.Visibility.Collapsed;
            CircularGraphCrosstabs.Visibility = System.Windows.Visibility.Visible;
        }

        private void ctxMenuTranslate_Opening(object sender, RadRoutedEventArgs e)
        {
            CslaDataProvider provider = App.Current.Resources["ReviewInfoData"] as CslaDataProvider;
            if (provider != null && provider.Data != null)
            {
                ReviewInfo review = provider.Data as ReviewInfo;
                if (review == null)
                {
                    BLdoc.IsEnabled = false;
                    BLdocCC.IsEnabled = false;
                }
                else
                {
                    if (review.BL_ACCOUNT_CODE != null && review.BL_ACCOUNT_CODE.Trim() != "")
                    {
                        BLdoc.IsEnabled = true;
                    }
                    else
                    {
                        BLdoc.IsEnabled = false;
                    }

                    if (review.BL_CC_ACCOUNT_CODE != null && review.BL_CC_ACCOUNT_CODE.Trim() != "")
                    {
                        BLdocCC.IsEnabled = true;
                    }
                    else
                    {
                        BLdocCC.IsEnabled = false;
                    }
                    ctxMenuTranslate.Opening -= ctxMenuTranslate_Opening;
                }
                if (BLdoc.IsEnabled == false)
                {
                    BLdoc.Header += " (no subscription details entered)";
                }
                if (BLdocCC.IsEnabled == false)
                {
                    BLdocCC.Header += " (no subscription details entered)";
                }
            }
        }

        private void ComboBoxCrosstabsColours_SelectionChanged(object sender, Telerik.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (CrosstabsBarChart != null)
            {
                CrosstabsBarChart.Palette = ComboBoxCrosstabsColours.SelectedItem as ChartPalette;
            }
        }

        private void CrossTabsFontSize_ValueChanged(object sender, RadRangeBaseValueChangedEventArgs e)
        {
            if (CrosstabsBarChart != null)
            {
                CrosstabsBarChart.FontSize = Convert.ToDouble(CrossTabsFontSize.Value);
                CrosstabsChartLegend.FontSize = Convert.ToDouble(CrossTabsFontSize.Value);
            }
        }

        private void FrequencyFontSize_ValueChanged(object sender, RadRangeBaseValueChangedEventArgs e)
        {
            if (FrequenciesPieChart != null)
            {
                FrequenciesPieChart.FontSize = Convert.ToDouble(FrequencyFontSize.Value);
            }
            if (FrequenciesBarChart != null)
            {
                FrequenciesBarChart.FontSize = Convert.ToDouble(FrequencyFontSize.Value);
            }
        }

        private void ComboBoxFrequencyColours_SelectionChanged(object sender, Telerik.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (FrequenciesPieChart != null)
            {
                FrequenciesPieChart.Palette = ComboBoxFrequencyColours.SelectedItem as ChartPalette;
            }
            if (FrequenciesBarChart != null)
            {
                FrequenciesBarChart.Palette = ComboBoxFrequencyColours.SelectedItem as ChartPalette;
            }
        }

        private void rbScreeningEverything_Click(object sender, RoutedEventArgs e)
        {
            if (rbScreeningEverything.IsChecked == true)
            {
                codesSelectControlScreening.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                codesSelectControlScreening.Visibility = System.Windows.Visibility.Visible;
            }
            CslaDataProvider RevInfoProvider = App.Current.Resources["ReviewInfoData"] as CslaDataProvider;
            if (RevInfoProvider != null || RevInfoProvider.Data != null)
            {//data isn't here, nothing to do!
                ReviewInfo RevInfo = RevInfoProvider.Data as ReviewInfo;
                if (RevInfo != null)
                {
                    if (rbScreeningEverything.IsChecked == true)
                    {
                        RevInfo.ScreeningWhatAttributeId = 0;
                    }
                    else
                    {
                        if (codesSelectControlScreening.SelectedAttributeSet() != null)
                        {
                            RevInfo.ScreeningWhatAttributeId = codesSelectControlScreening.SelectedAttributeSet().AttributeId;
                        }
                    }
                }
            }
        }
        void codesSelectControlScreening_SelectCode_SelectionChanged(object sender, Telerik.Windows.Controls.SelectionChangedEventArgs e)
        {
            CslaDataProvider RevInfoProvider = App.Current.Resources["ReviewInfoData"] as CslaDataProvider;
            if (RevInfoProvider != null || RevInfoProvider.Data != null)
            {//data isn't here, nothing to do!
                ReviewInfo RevInfo = RevInfoProvider.Data as ReviewInfo;
                if (RevInfo != null)
                {
                    if (rbScreeningEverything.IsChecked == true)
                    {
                        RevInfo.ScreeningWhatAttributeId = 0;
                    }
                    else
                    {
                        if (codesSelectControlScreening.SelectedAttributeSet() != null)
                        {
                            RevInfo.ScreeningWhatAttributeId = codesSelectControlScreening.SelectedAttributeSet().AttributeId;
                        }
                    }
                }
            }
        }
        private void UpDownNScreening_ValueChanged(object sender, RadRangeBaseValueChangedEventArgs e)
        {
            CslaDataProvider RevInfoProvider = App.Current.Resources["ReviewInfoData"] as CslaDataProvider;
            if (RevInfoProvider != null || RevInfoProvider.Data != null)
            {//data isn't here, nothing to do!
                ReviewInfo RevInfo = RevInfoProvider.Data as ReviewInfo;
                if (RevInfo != null)
                {
                    if (rbScreeningEverything.IsChecked == true)
                    {
                        RevInfo.ScreeningWhatAttributeId = 0;
                    }
                    else
                    {
                        if (codesSelectControlScreening.SelectedAttributeSet() != null)
                        {
                            RevInfo.ScreeningWhatAttributeId = codesSelectControlScreening.SelectedAttributeSet().AttributeId;
                        }
                    }
                }
            }
        }
        private void ComboReconcilliationMode_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            CslaDataProvider RevInfoProvider = App.Current.Resources["ReviewInfoData"] as CslaDataProvider;
            if (RevInfoProvider != null || RevInfoProvider.Data != null)
            {//data isn't here, nothing to do!
                ReviewInfo RevInfo = RevInfoProvider.Data as ReviewInfo;
                if (RevInfo != null)
                {
                    switch (ComboReconcilliationMode.SelectedIndex)
                    {
                        case 0:
                            RevInfo.ScreeningReconcilliation = "Single";
                            break;
                        case 1:
                            RevInfo.ScreeningReconcilliation = "no compl";
                            break;
                        case 2:
                            RevInfo.ScreeningReconcilliation = "auto code";
                            break;
                        case 3:
                            RevInfo.ScreeningReconcilliation = "auto excl";
                            break;
                        case 4:
                            RevInfo.ScreeningReconcilliation = "auto safet";
                            break;
                    }
                }
            }
        }
        private void PaneActiveScreening_Activated(object sender, EventArgs e)
        {
            UpdateReviewInfoForScreening();
        }

        private void UpdateReviewInfoForScreening()
        {
            if (PaneActiveScreening != null)
            {
                CslaDataProvider provider = App.Current.Resources["ReviewInfoData"] as CslaDataProvider;
                ReviewInfo rInfo = provider.Data as ReviewInfo;


                if (rInfo != null)
                {
                    // set the CodeSet combo
                    ScreeningCodeSetComboSelectCodeSet.SelectionChanged -= ScreeningCodeSetComboSelectCodeSet_SelectionChanged;
                    if (rInfo.ScreeningCodeSetId != 0 && ScreeningCodeSetComboSelectCodeSet.ItemsSource != null)
                    {
                        int index = 0;
                        foreach (ReviewSet rs in (ScreeningCodeSetComboSelectCodeSet.ItemsSource as ReviewSetsList))
                        {
                            if (rs.SetId == rInfo.ScreeningCodeSetId)
                            {
                                ScreeningCodeSetComboSelectCodeSet.SelectedIndex = index;
                                break;
                            }
                            index++;
                        }
                    }
                    ScreeningCodeSetComboSelectCodeSet.SelectionChanged += ScreeningCodeSetComboSelectCodeSet_SelectionChanged;
                    // set screening mode combo
                    if (rInfo.ScreeningMode == "Random")
                    {
                        ComboScreeningMode.SelectedIndex = 0;
                    }
                    else
                    {
                        ComboScreeningMode.SelectedIndex = 1;
                    }

                    // set what to screen combo
                    if (rInfo.ScreeningWhatAttributeId == 0)
                    {
                        rbScreeningEverything.IsChecked = true;
                        codesSelectControlScreening.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        rbScreeningSelected.IsChecked = true;
                        codesSelectControlScreening.Visibility = Visibility.Visible;
                        codesSelectControlScreening.SelectAttributeSetFromAttributeId(rInfo.ScreeningWhatAttributeId);
                    }

                    // set reconcilliation combo
                    switch (rInfo.ScreeningReconcilliation)
                    {
                        case "Single":
                            ComboReconcilliationMode.SelectedIndex = 0;
                            break;
                        case "no compl":
                            ComboReconcilliationMode.SelectedIndex = 1;
                            break;
                        case "auto code":
                            ComboReconcilliationMode.SelectedIndex = 2;
                            break;
                        case "auto excl":
                            ComboReconcilliationMode.SelectedIndex = 3;
                            break;
                        case "auto safet":
                            ComboReconcilliationMode.SelectedIndex = 4;
                            break;
                    }
                    ResetScreeningUI();
                }
            }
        }

        private void cmdScreeningSaveReviewOptions_Click(object sender, RoutedEventArgs e)
        {
            CslaDataProvider provider = App.Current.Resources["ReviewInfoData"] as CslaDataProvider;
            if (provider == null) return;
            ReviewInfo RevInfo = provider.Data as ReviewInfo;
            if (RevInfo != null && RevInfo.IsDirty)
            {
                if (MessageBox.Show("Are you sure you want to save these options?\n\r\n\r(It's important you're sure, as you're changing options for all users in the review)", "Confirm save", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
                {
                    RevInfo.CancelEdit(); // only seems to work for one 'cancel' in the UI (i.e the values are rolled back the first time, but remain in after that). a bug in CSLA?
                    provider.Refresh();
                    PaneActiveScreening_Activated(sender, e);
                    return;
                }
                // set selected code set
                if (ScreeningCodeSetComboSelectCodeSet.SelectedIndex != -1)
                {
                    RevInfo.ScreeningCodeSetId = (ScreeningCodeSetComboSelectCodeSet.SelectedItem as ReviewSet).SetId;
                }
                else
                {
                    RevInfo.ScreeningCodeSetId = 0;
                    RadWindow.Alert("Sorry, you need to select a code set first");
                    return;
                }

                // set screening mode
                if (ComboScreeningMode.SelectedIndex == 0)
                {
                    RevInfo.ScreeningMode = "Random";
                }
                else
                {
                    RevInfo.ScreeningMode = "Priority";
                }

                // set what to screen
                if (rbScreeningEverything.IsChecked == true)
                {
                    RevInfo.ScreeningWhatAttributeId = 0;
                }
                else
                {
                    if (codesSelectControlScreening.SelectedAttributeSet() != null)
                    {
                        RevInfo.ScreeningWhatAttributeId = codesSelectControlScreening.SelectedAttributeSet().AttributeId;
                    }
                    else
                    {
                        RadWindow.Alert("Please select a code set to filter by (or select 'screen everything')");
                        return;
                    }
                }

                // set reconcilliation
                switch (ComboReconcilliationMode.SelectedIndex)
                {
                    case 0:
                        RevInfo.ScreeningReconcilliation = "Single";
                        break;
                    case 1:
                        RevInfo.ScreeningReconcilliation = "no compl";
                        break;
                    case 2:
                        RevInfo.ScreeningReconcilliation = "auto code";
                        break;
                    case 3:
                        RevInfo.ScreeningReconcilliation = "auto excl";
                        break;
                    case 4:
                        RevInfo.ScreeningReconcilliation = "auto safet";
                        break;
                }
                RevInfo.Saved += (o, e2) =>
                {
                    if (e2.Error != null)
                        MessageBox.Show(e2.Error.Message);
                    else
                        RadWindow.Alert("Settings saved");
                    provider.Refresh();
                };
                RevInfo.ApplyEdit();
                RevInfo.BeginSave(true);
            }
        }

        private void cmdScreeningRefreshReviewInfo_Click(object sender, RoutedEventArgs e)
        {
            CslaDataProvider provider = App.Current.Resources["ReviewInfoData"] as CslaDataProvider;
            provider.Refresh();
        }
        private void cmdScreeningCancelReviewOptions_Click(object sender, RoutedEventArgs e)
        {
            CslaDataProvider provider = App.Current.Resources["ReviewInfoData"] as CslaDataProvider;
            if (provider == null) return;
            ReviewInfo RevInfo = provider.Data as ReviewInfo;
            if (RevInfo == null) return;
            RevInfo.CancelEdit(); // only seems to work for one 'cancel' in the UI (i.e the values are rolled back the first time, but remain in after that). a bug in CSLA?
            provider.Refresh();
            PaneActiveScreening_Activated(sender, e);
            return;
        }
        private void cmdSearchTabRefreshSearchList_Click(object sender, RoutedEventArgs e)
        {
            CslaDataProvider provider = App.Current.Resources["SearchesData"] as CslaDataProvider;
            provider.FactoryParameters.Clear();
            provider.FactoryMethod = "GetSearchList";
            provider.Refresh();
        }

        private void cmdScreeningRunSimulation_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show(@"Are you sure you want to run this simulation? \n\r(This will delete previous simulation runs from this review.)", "Confirm run simulation", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                CslaDataProvider provider = App.Current.Resources["ReviewInfoData"] as CslaDataProvider;
                ReviewInfo RevInfo = provider.Data as ReviewInfo;
                DataPortal<TrainingRunCommand> dp = new DataPortal<TrainingRunCommand>();
                TrainingRunCommand command = new TrainingRunCommand();
                dp.ExecuteCompleted += (o, e2) =>
                {
                    if (e2.Error != null)
                    {
                        if (e2.Error.Message.Contains("has exceeded the allotted timeout") == true)
                        {
                        //RadWindow.Alert("Caught timeout exception");
                    }
                        else
                        {
                            RadWindow.Alert(e2.Error.Message);
                        }
                    }
                };
                command.RevInfo = RevInfo;
                command.Parameters = "DoSimulation";
                dp.BeginExecute(command);
                RadWindow.Alert("Simulations now running. This can take hours...");
            }
        }

        private void ScreeningCodeSetComboSelectCodeSet_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            ResetScreeningUI();
            if (DocumentListPane.SelectedPane.Name != "PaneActiveScreening") return;
            ReviewSet rs = ScreeningCodeSetComboSelectCodeSet.SelectedItem as ReviewSet;
            if (rs == null) return;
            if (cbScreeningAddAttributesAutomatically.IsChecked == true)
            {
                GridViewScreeningCodes.IsEnabled = false;
                CslaDataProvider TsclProvider = this.Resources["TrainingScreeningCriteriaData"] as CslaDataProvider;
                if (TsclProvider == null)
                {
                    RadWindow.Alert("ERROR: could not automatically add screening codes to the bottom"
                        + Environment.NewLine + "left panel."
                        + Environment.NewLine + "Priority Screening will not work if this list isn't correctly populated."
                        // + Environment.NewLine + ""
                        + Environment.NewLine + "You can manually add the codes and click 'Save options'."
                        + Environment.NewLine + "If this doesn't work please contact EPPISupport@ucl.ac.uk.");
                    return;
                }
                TrainingScreeningCriteriaList tscl = TsclProvider.Data as TrainingScreeningCriteriaList;
                if (tscl != null)
                {
                    //DataPortal<TrainingScreeningCriteriaListDeleteAllCommand> dp = new DataPortal<TrainingScreeningCriteriaListDeleteAllCommand>();
                    //TrainingScreeningCriteriaListDeleteAllCommand command = new TrainingScreeningCriteriaListDeleteAllCommand();                        
                    //dp.ExecuteCompleted += (o, e2) =>
                    //{
                    //    foreach (AttributeSet aSet in rs.Attributes)
                    //    {
                    //        TrainingScreeningCriteria newTsc = new TrainingScreeningCriteria();
                    //        newTsc.AttributeId = aSet.AttributeId;
                    //        newTsc.AttributeName = aSet.AttributeName;
                    //        newTsc.Included = aSet.AttributeTypeId == 10 ? true : false; // TypeId of 10 == 'Include' code type
                    //        tscl.Add(newTsc);
                    //        newTsc.BeginEdit();
                    //        newTsc.ApplyEdit();
                    //    }
                    //    TsclProvider.Saved += TsclProvider_Saved;
                    //    TsclProvider.Save(); // not idea, as not all attributes are saved before refresh

                    //};
                    //dp.BeginExecute(command);
                    //while (tscl.Count > 0)
                    foreach (TrainingScreeningCriteria tsc in tscl)
                    {
                        tsc.BeginEdit();
                        tsc.Delete();
                        
                        tsc.Saved += Tsc_Saved;
                        tsc.ApplyEdit();
                    }
                    //tscl.Clear();
                    foreach (AttributeSet aSet in rs.Attributes)
                    {
                        TrainingScreeningCriteria newTsc = new TrainingScreeningCriteria();
                        newTsc.AttributeId = aSet.AttributeId;
                        newTsc.AttributeName = aSet.AttributeName;
                        newTsc.Included = aSet.AttributeTypeId == 10 ? true : false; // TypeId of 10 == 'Include' code type
                        tscl.Add(newTsc);
                        newTsc.BeginEdit();
                        

                        if (rs.Attributes.Count > 0 && aSet == rs.Attributes[rs.Attributes.Count - 1])
                        {
                            System.Threading.Thread.Sleep(100);
                            newTsc.Saved += LastTsc_Saved;
                        }
                        else
                        {
                            newTsc.Saved += Tsc_Saved;
                        }
                        newTsc.ApplyEdit();
                        //newTsc.BeginSave();
                    }
                }
            }
            //NEW (Aug 2017): complements what is done in ResetScreeningUI if selected set is in comparison mode, make sure #people screening is at least two.
            if (!rs.CodingIsFinal) //set is in comparison mode
            {
                SetScreeningToMultipleAnd2Users();
            }
            CslaDataProvider RevInfoProvider = App.Current.Resources["ReviewInfoData"] as CslaDataProvider;
            if (RevInfoProvider != null || RevInfoProvider.Data != null)
            {
                ReviewInfo rInfo = RevInfoProvider.Data as ReviewInfo;
                if (rInfo != null)
                {
                    if (ScreeningCodeSetComboSelectCodeSet.SelectedIndex != -1)
                    {
                        rInfo.ScreeningCodeSetId = (ScreeningCodeSetComboSelectCodeSet.SelectedItem as ReviewSet).SetId;
                    }
                }
            }
            
        }
        private void ComboScreeningMode_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            CslaDataProvider RevInfoProvider = App.Current.Resources["ReviewInfoData"] as CslaDataProvider;
            if (RevInfoProvider != null || RevInfoProvider.Data != null)
            {
                ReviewInfo RevInfo = RevInfoProvider.Data as ReviewInfo;
                if (ComboScreeningMode.SelectedIndex == 0)
                {
                    RevInfo.ScreeningMode = "Random";
                }
                else
                {
                    RevInfo.ScreeningMode = "Priority";
                }
            }
        }
        public void SetScreeningToMultipleAnd2Users()
        {
            if (UpDownNScreening.Value < 2) UpDownNScreening.Value = 2;
            ComboBoxItem SelectedMode = (ComboReconcilliationMode.SelectedItem) as ComboBoxItem;
            if (SelectedMode != null && SelectedMode.Content.ToString() == "Single (auto-completes)")
            {
                ComboReconcilliationMode.SelectedIndex = 1;
            }
        }
        private void LastTsc_Saved(object sender, Csla.Core.SavedEventArgs e)
        {
            Tsc_Saved(sender, e);
            CslaDataProvider TsclProvider = this.Resources["TrainingScreeningCriteriaData"] as CslaDataProvider;
            if (TsclProvider != null)
            {
                TsclProvider.DataChanged += TsclProvider_DataChanged;
                TsclProvider.Refresh();
            }
        }
        private void Tsc_Saved(object sender, Csla.Core.SavedEventArgs e)
        {
            if (e.Error != null)
            {
                RadWindow.Alert("ERROR: could save changes to screening codes (bottom left panel)."
                        + Environment.NewLine + "Priority Screening will not work if the list isn't correctly populated."
                        + Environment.NewLine + "Error returned is:"
                        + Environment.NewLine + e.Error.Message
                        + Environment.NewLine + "If this problem persists please contact EPPISupport@ucl.ac.uk.");
            }
        }
        private void TsclProvider_DataChanged(object sender, EventArgs e)
        {
            GridViewScreeningCodes.IsEnabled = true;
            CslaDataProvider TsclProvider = sender as CslaDataProvider;
            if (TsclProvider != null)
            {
                TsclProvider.DataChanged -= TsclProvider_DataChanged;
                if (TsclProvider.Error != null)
                {
                    RadWindow.Alert("ERROR: could save changes to screening codes (bottom left panel)."
                       + Environment.NewLine + "Priority Screening will not work if the list isn't correctly populated."
                       + Environment.NewLine + "Error returned is:"
                       + Environment.NewLine + TsclProvider.Error.Message
                       + Environment.NewLine + "If this problem persists please contact EPPISupport@ucl.ac.uk.");
                }
                else
                {
                    RadWindow.Alert("PLEASE NOTE: the list of screening codes (the lower left panel ONLY)"
                     + Environment.NewLine + "has been automatically populated and saved."
                     + Environment.NewLine + "However, your other changes, including the Screening Set"
                     + Environment.NewLine + "selection have not been saved!"
                     + Environment.NewLine
                     + Environment.NewLine + "Please review all your settings, including the list of codes on"
                     + Environment.NewLine + "the lower left panel (used by the classifier)."
                     + Environment.NewLine + "If all settings are correct, please click 'Save Options'.");
                }

            }
        }

        public void ResetScreeningUI()
        {
            CslaDataProvider RevInfoProvider = App.Current.Resources["ReviewInfoData"] as CslaDataProvider;
            if (RevInfoProvider == null || RevInfoProvider.Data == null)
            {//data isn't here, nothing to do!
                return;
            }
            ReviewInfo rInfo = RevInfoProvider.Data as ReviewInfo;
            if (rInfo == null)
            {//data isn't here, nothing to do!
                return;
            }
            bool UserCanEditSettings = rInfo.UserCanEditScreeningSetting;
            if (ScreeningCodeSetComboSelectCodeSet.SelectedIndex == -1) // No screening code set selected - all is disabled
            {
                ComboScreeningMode.IsEnabled = false;
                rbScreeningEverything.IsEnabled = false;
                rbScreeningSelected.IsEnabled = false;
                UpDownNScreening.IsEnabled = false;
                codesSelectControlScreening.IsEnabled = false;
                ComboReconcilliationMode.IsEnabled = false;
                cbScreeningAutoExclude.IsEnabled = false;
                cbScreeningFullIndex.IsEnabled = false;
                return;
            }

            ReviewSet rs = ScreeningCodeSetComboSelectCodeSet.SelectedItem as ReviewSet;
            if (rs != null)
            {
                codesSelectControlScreening.IsEnabled = UserCanEditSettings;
                if (rs.CodingIsFinal == true) // i.e. normal, not comparison coding
                {
                    ComboScreeningMode.IsEnabled = UserCanEditSettings;
                    rbScreeningEverything.IsEnabled = UserCanEditSettings;
                    rbScreeningSelected.IsEnabled = UserCanEditSettings;
                    UpDownNScreening.Value = 1;
                    UpDownNScreening.IsEnabled = false;
                    ComboReconcilliationMode.SelectedIndex = 0;
                    ComboReconcilliationMode.IsEnabled = false;
                    cbScreeningAutoExclude.IsEnabled = UserCanEditSettings;
                    cbScreeningFullIndex.IsEnabled = HasWriteRights;
                }
                else // COMPARISON coding
                {
                    ComboScreeningMode.IsEnabled = UserCanEditSettings;
                    rbScreeningEverything.IsEnabled = UserCanEditSettings;
                    rbScreeningSelected.IsEnabled = UserCanEditSettings;
                    UpDownNScreening.IsEnabled = UserCanEditSettings;
                    ComboReconcilliationMode.IsEnabled = UserCanEditSettings;
                    cbScreeningAutoExclude.IsEnabled = UserCanEditSettings;
                    cbScreeningFullIndex.IsEnabled = HasWriteRights;
                }
            }

        }
        public void UnHookMe()
        {
            windowRandomAllocate.codesSelectControlAllocate.UnhookMe();
            windowRandomAllocate.codesSelectControlAllocateFilterCode.UnhookMe();
            windowRandomAllocate.codesSelectControlAllocateFilterCodeSet.UnhookMe();
            homeReportsControl.UnHookMe();
            codesTreeControl.UnHookMe();
            dialogCodingControl.UnHookMe();
            DialogMyInfo.UnHookMe();

            CslaDataProvider CsetsProvider = App.Current.Resources["CodeSetsData"] as CslaDataProvider;
            if (CsetsProvider != null)
            {
                CsetsProvider.DataChanged -= CsetsProvider_DataChanged;
                CsetsProvider.DataChanged -= CsetsProvider_DataChanged;
            }
            CslaDataProvider RevInfoprovider = App.Current.Resources["ReviewInfoData"] as CslaDataProvider;
            if (RevInfoprovider != null)
            {
                RevInfoprovider.DataChanged -= RevInfoprovider_DataChanged;
                RevInfoprovider.DataChanged -= RevInfoprovider_DataChanged;
            }
            CslaDataProvider CodeSetsData = App.Current.Resources["CodeSetsData"] as CslaDataProvider;
            if (CodeSetsData != null)
            {
                CodeSetsData.DataChanged -= CodeSetsData_DataChanged;
                CodeSetsData.DataChanged -= CodeSetsData_DataChanged;
            }
            if (dlgWindowVisualiseSearch != null) dlgWindowVisualiseSearch.UnhookMe();
        }

        private void cmdExportItemsGrid_Click(object sender, RoutedEventArgs e)
        {
            string extension = "html";
            ExportFormat format = ExportFormat.Html;
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.DefaultExt = extension;
            dialog.Filter = String.Format("{1} files (*.{0})|*.{0}|All files (*.*)|*.*", extension, "html");
            dialog.FilterIndex = 1;
            CslaDataProvider provider = ((CslaDataProvider)this.Resources["ItemListData"]);
            if (dialog.ShowDialog() == true)
            {
                bool cleanup = false;
                List<Item> selectedL = new List<Item>();
                BindingExpression BindExp = ItemsGrid.GetBindingExpression(RadGridView.ItemsSourceProperty);
                if (provider != null && provider.Data != null && provider.Data == ItemsGrid.ItemsSource &&
                        ItemsGrid.SelectedItems != null && ItemsGrid.SelectedItems.Count > 0)
                {//export only the selected, we could apply and remove filter, but I can't make it work, so will use a more radical approach
                    cleanup = true;
                    ObservableCollection<object> selected = ItemsGrid.SelectedItems as ObservableCollection<object>;
                    
                    foreach (Item it in selected)
                    {
                        selectedL.Add(it);
                    }

                    ItemsGrid.ItemsSource = selectedL;
                }
                using (Stream stream = dialog.OpenFile())
                {
                    GridViewExportOptions exportOptions = new GridViewExportOptions();
                    exportOptions.Format = format;
                    //exportOptions.ShowColumnFooters = true;
                    exportOptions.ShowColumnHeaders = true;
                    //exportOptions.ShowGroupFooters = true;
                    //object o = Telerik.Windows.Data.TypeExtensions.DefaultValue;
                    ItemsGrid.Columns[0].IsVisible = false;
                    ItemsGrid.Export(stream, exportOptions);
                    ItemsGrid.Columns[0].IsVisible = true;
                }
                if (cleanup)
                {
                    //BindingExpression BindExp2 = ItemsGrid.GetBindingExpression(RadGridView.ItemsSourceProperty);
                    if (BindExp != null)
                    {//there was some binding to start with
                        ItemsGrid.SetBinding(RadGridView.ItemsSourceProperty, BindExp.ParentBinding);
                    }
                    else
                    {//no binding, just put the original itmes in again.
                        ItemsGrid.ItemsSource = provider.Data;
                    }
                    ItemsGrid.Select(selectedL);
                }
            }
        }

        private void cmdScreeningSimulationSave_Click(object sender, RoutedEventArgs e)
        {
           
                CslaDataProvider provider = App.Current.Resources["ReviewInfoData"] as CslaDataProvider;
                ReviewInfo RevInfo = provider.Data as ReviewInfo;
                DataPortal<TrainingRunCommand> dp = new DataPortal<TrainingRunCommand>();
                TrainingRunCommand command = new TrainingRunCommand();
                dp.ExecuteCompleted += (o, e2) =>
                {
                    if (e2.Error != null)
                    {
                        if (e2.Error.Message.Contains("has exceeded the allotted timeout") == true)
                        {
                                //RadWindow.Alert("Caught timeout exception");
                            }
                        else
                        {
                            RadWindow.Alert(e2.Error.Message);
                        }
                    }
                    else
                    {
                        string results = (e2.Object as TrainingRunCommand).SimulationResults;
                        if (results == "")
                        {
                            cmdScreeningSimulationResults.Visibility = Visibility.Collapsed;
                            RadWindow.Alert("No results available." + Environment.NewLine + "Please try again later.");
                        }
                        else
                        {
                            // annoying workaround as you can't open a dialog without user request!
                            cmdScreeningSimulationResults.Tag = results;
                            cmdScreeningSimulationResults.Visibility = Visibility.Visible;
                            RadWindow.Alert("You can download the results now" + Environment.NewLine + "by clicking 'Save results'");
                        }
                    }
                };
                command.RevInfo = RevInfo;
                command.Parameters = "FetchSimulationResults";
                dp.BeginExecute(command);
            }

        private void cmdScreeningSimulationResults_Click(object sender, RoutedEventArgs e)
        {
            string extension = "csv";
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.DefaultExt = extension;
            dialog.Filter = String.Format("{1} files (*.{0})|*.{0}|All files (*.*)|*.*", extension, "csv");
            dialog.FilterIndex = 1;

            if (dialog.ShowDialog() == true)
            {
                StreamWriter writer = new StreamWriter(dialog.OpenFile());
                writer.WriteLine(cmdScreeningSimulationResults.Tag.ToString());
                writer.Dispose();
                writer.Close();
            }
        }

        private void cmdVisualiseSearch_Click(object sender, RoutedEventArgs e)
        {
            if (dlgWindowVisualiseSearch == null)
            {
                dlgWindowVisualiseSearch = new Windows.windowSearchVisualise();
                dlgWindowVisualiseSearch.CodesCreated += DlgWindowVisualiseSearch_CodesCreated;
            }
            Search sch = (sender as Button).DataContext as Search;
            if (sch != null)
            {
                dlgWindowVisualiseSearch.SearchId = sch.SearchId;
                dlgWindowVisualiseSearch.SearchName = sch.Title;
                dlgWindowVisualiseSearch.getSearchData(sch.SearchId);
                dlgWindowVisualiseSearch.Show();
            }
        }

        private void DlgWindowVisualiseSearch_CodesCreated(object sender, RoutedEventArgs e)
        {
            LoadCodeSets();
        }

        private void cmdMagBrowser_Click(object sender, RoutedEventArgs e)
        {
            MagBrowserControl.ShowMagBrowser();
            windowMagBrowser.ShowDialog();
        }

        private void MagBrowserControl_ListIncludedThatNeedMatching(object sender, RoutedEventArgs e)
        {
            windowMagBrowser.Close();
            TextBlockShowing.Text = "Showing: included items with low confidence Microsoft Academic matches (that are unchecked)";
            SelectionCritieraItemList = new SelectionCriteria();
            SelectionCritieraItemList.ListType = "MagMatchesNeedingChecking";
            SelectionCritieraItemList.OnlyIncluded = true;
            SelectionCritieraItemList.ShowDeleted = false;
            SelectionCritieraItemList.AttributeSetIdList = "";
            SelectionCritieraItemList.PageNumber = 0;
            LoadItemList();
        }

        private void MagBrowserControl_ListExcludedThatNeedMatching(object sender, RoutedEventArgs e)
        {
            windowMagBrowser.Close();
            TextBlockShowing.Text = "Showing: excluded items with low confidence Microsoft Academic matches (that are unchecked)";
            SelectionCritieraItemList = new SelectionCriteria();
            SelectionCritieraItemList.ListType = "MagMatchesNeedingChecking";
            SelectionCritieraItemList.OnlyIncluded = false;
            SelectionCritieraItemList.ShowDeleted = false;
            SelectionCritieraItemList.AttributeSetIdList = "";
            SelectionCritieraItemList.PageNumber = 0;
            LoadItemList();
        }

        private void MagBrowserControl_ListExcludedNotMatched(object sender, RoutedEventArgs e)
        {
            windowMagBrowser.Close();
            TextBlockShowing.Text = "Showing: excluded items that are not matched to any Microsoft Academic records";
            SelectionCritieraItemList = new SelectionCriteria();
            SelectionCritieraItemList.ListType = "MagMatchesNotMatched";
            SelectionCritieraItemList.OnlyIncluded = false;
            SelectionCritieraItemList.ShowDeleted = false;
            SelectionCritieraItemList.AttributeSetIdList = "";
            SelectionCritieraItemList.PageNumber = 0;
            LoadItemList();
        }

        private void MagBrowserControl_ListIncludedNotMatched(object sender, RoutedEventArgs e)
        {
            windowMagBrowser.Close();
            TextBlockShowing.Text = "Showing: included items that are not matched to any Microsoft Academic records";
            SelectionCritieraItemList = new SelectionCriteria();
            SelectionCritieraItemList.ListType = "MagMatchesNotMatched";
            SelectionCritieraItemList.OnlyIncluded = true;
            SelectionCritieraItemList.ShowDeleted = false;
            SelectionCritieraItemList.AttributeSetIdList = "";
            SelectionCritieraItemList.PageNumber = 0;
            LoadItemList();
        }

        private void MagBrowserControl_ListExcludedMatched(object sender, RoutedEventArgs e)
        {
            windowMagBrowser.Close();
            TextBlockShowing.Text = "Showing: excluded items that are matched to at least one Microsoft Academic record";
            SelectionCritieraItemList = new SelectionCriteria();
            SelectionCritieraItemList.ListType = "MagMatchesMatched";
            SelectionCritieraItemList.OnlyIncluded = false;
            SelectionCritieraItemList.ShowDeleted = false;
            SelectionCritieraItemList.AttributeSetIdList = "";
            SelectionCritieraItemList.PageNumber = 0;
            LoadItemList();
        }

        private void MagBrowserControl_ListIncludedMatched(object sender, RoutedEventArgs e)
        {
            windowMagBrowser.Close();
            TextBlockShowing.Text = "Showing: included items that are matched to at least one Microsoft Academic record";
            SelectionCritieraItemList = new SelectionCriteria();
            SelectionCritieraItemList.ListType = "MagMatchesMatched";
            SelectionCritieraItemList.OnlyIncluded = true;
            SelectionCritieraItemList.ShowDeleted = false;
            SelectionCritieraItemList.AttributeSetIdList = "";
            SelectionCritieraItemList.PageNumber = 0;
            LoadItemList();
        }

        private void MagBrowserControl_ListSimulationFN(object sender, RoutedEventArgs e)
        {
            windowMagBrowser.Close();
            HyperlinkButton hl = sender as HyperlinkButton;
            if (hl != null)
            {
                MagSimulation ms = hl.DataContext as MagSimulation;
                if (ms != null)
                {
                    TextBlockShowing.Text = "Showing: false negatives from selected simulation";
                    SelectionCritieraItemList = new SelectionCriteria();
                    SelectionCritieraItemList.ListType = "MagSimulationFN";
                    SelectionCritieraItemList.OnlyIncluded = true;
                    SelectionCritieraItemList.ShowDeleted = false;
                    SelectionCritieraItemList.AttributeSetIdList = "";
                    SelectionCritieraItemList.PageNumber = 0;
                    SelectionCritieraItemList.MagSimulationId = ms.MagSimulationId;
                    LoadItemList();
                }
            }
        }

        private void MagBrowserControl_ListSimulationTP(object sender, RoutedEventArgs e)
        {
            windowMagBrowser.Close();
            HyperlinkButton hl = sender as HyperlinkButton;
            if (hl != null)
            {
                MagSimulation ms = hl.DataContext as MagSimulation;
                if (ms != null)
                {
                    TextBlockShowing.Text = "Showing: true positives from selected simulation";
                    SelectionCritieraItemList = new SelectionCriteria();
                    SelectionCritieraItemList.ListType = "MagSimulationTP";
                    SelectionCritieraItemList.OnlyIncluded = true;
                    SelectionCritieraItemList.ShowDeleted = false;
                    SelectionCritieraItemList.AttributeSetIdList = "";
                    SelectionCritieraItemList.PageNumber = 0;
                    SelectionCritieraItemList.MagSimulationId = ms.MagSimulationId;
                    LoadItemList();
                }
            }
        }

        private void DialogCodingControl_launchMagBrowser(object sender, EventArgs e)
        {
            MagPaper mp = sender as MagPaper;
            if (mp != null)
            {
                MagBrowserControl.InitialiseBrowser();
                MagBrowserControl.AddToBrowseHistory("Go to specific Paper Id: " + mp.PaperId.ToString(), "PaperDetail",
                        mp.PaperId, mp.FullRecord, mp.Abstract, mp.LinkedITEM_ID, mp.URLs, mp.FindOnWeb, 0,
                        "", "", 0);
                MagBrowserControl.IncrementHistoryCount();
                MagBrowserControl.ShowPaperDetailsPage(mp.PaperId, mp.FullRecord, mp.Abstract,
                    mp.URLs, mp.FindOnWeb, mp.LinkedITEM_ID);
                windowMagBrowser.ShowDialog();
            }
        }
    }
}