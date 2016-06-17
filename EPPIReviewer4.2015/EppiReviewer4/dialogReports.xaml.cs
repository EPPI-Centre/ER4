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
using Telerik.Windows.Controls;
using Csla;
using Csla.DataPortalClient;
using Csla.Xaml;

namespace EppiReviewer4
{
    public partial class dialogReports : UserControl
    {
        public string ItemIdList;
        public event EventHandler CloseWindowRequest;

        private RadWindow windowReports2 = new RadWindow();
        private dialogReportViewer reportViewerControl2 = new dialogReportViewer();

        public dialogReports()
        {
            InitializeComponent();
            Grid g = new Grid();
            g.Children.Add(reportViewerControl2);
            windowReports2.Header="Report viewer";
            windowReports2.WindowState= WindowState.Maximized;
            windowReports2.WindowStartupLocation= Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
            windowReports2.RestrictedAreaMargin= new Thickness(20);
            windowReports2.Width=500;
            windowReports2.CanClose = true;
            windowReports2.WindowStateChanged += new EventHandler(Helpers.WindowHelper.MaxOnly_WindowStateChanged);
            windowReports2.Style = Application.Current.Resources["CustomRadWindowStyle"] as Style;
            windowReports2.Content = g;
            codesSelectControlReports.SelectCode_SelectionChanged += new EventHandler<Telerik.Windows.Controls.SelectionChangedEventArgs>(codesSelectControlReports_SelectCode_SelectionChanged);
        }

        void codesSelectControlReports_SelectCode_SelectionChanged(object sender, Telerik.Windows.Controls.SelectionChangedEventArgs e)
        {
            cmdGo.DataContext = null;
        }

        public void OnShow()
        {
            //CslaDataProvider provider = ((CslaDataProvider)this.Resources["ReportListData"]);
            //provider.Refresh();
            comboBoxSelectWhichItems_SelectionChanged(null, null);
        }

        private void cmdGo_Click(object sender, RoutedEventArgs e)
        {
            if (ItemIdList == "" && comboBoxSelectWhichItems.SelectedIndex == 1)
            {
                MessageBox.Show("Sorry: you don't have any items selected.");
            }
            else
            {
                if (ComboBoxReports.SelectedItem != null)
                {
                    cmdGo.IsEnabled = false;
                    if (CheckBoxShowOutcomes.IsChecked == true && (ComboBoxReports.SelectedItem as Report).ReportType == "Answer")
                    {
                        cmdGo.DataContext = null;
                        DataPortal<ReportExecuteCommand> dp = new DataPortal<ReportExecuteCommand>();
                        ReportExecuteCommand command = new ReportExecuteCommand(
                            (ComboBoxReports.SelectedItem as Report).ReportType,
                            ItemIdList,
                            (ComboBoxReports.SelectedItem as Report).ReportId,
                            CheckBoxShowId.IsChecked == true ? true : false,
                            CheckBoxShowOldId.IsChecked == true ? true : false,
                            CheckBoxShowOutcomes.IsChecked == true ? true : false,
                            RadioAlignHorizontal.IsChecked == true ? true : false,
                            (ComboBoxOrderBy.SelectedItem as ComboBoxItem).Content.ToString(),
                            (ComboBoxReports.SelectedItem as Report).Name,
                            codesSelectControlReports.SelectedAttributeSet() != null ?
                                codesSelectControlReports.SelectedAttributeSet().AttributeId : 0,
                            codesSelectControlReports.SelectedAttributeSet() != null ?
                                codesSelectControlReports.SelectedAttributeSet().SetId : 0);
                        dp.ExecuteCompleted += (o, e2) =>
                        {
                            BusyLoading.IsRunning = false;
                            if (e2.Error != null)
                            {
                                if (e2.Error.GetType() == new System.InvalidOperationException().GetType())
                                {
                                    RadWindow.Alert("Unable to Show report: this usually happens because "
                                        + Environment.NewLine + "the popup-report window was blocked by your browser."
                                        + Environment.NewLine + "Please enable popups for 'eppi.ioe.ac.uk'."
                                        + Environment.NewLine + "NOTE: most browsers will show a notification bar"
                                        + Environment.NewLine + "on top of the content area when a popup is blocked."
                                        + Environment.NewLine + "To enable popups, click on the 'Settings' or 'Options' button.");
                                }
                                else
                                {
                                    RadWindow.Alert(e2.Error.Message);
                                }
                                //MessageBox.Show(e2.Error.Message);
                            }
                            else
                            {
                                //System.Windows.Browser.HtmlPage.Window.Invoke("ShowPopup", e2.Object.ReturnReport);
                                reportViewerControl2.SetContent("<html><body>" + e2.Object.ReturnReport + "</body></html>");
                                windowReports2.ShowDialog();
                                cmdGo.IsEnabled = true;
                            }
                        };
                        BusyLoading.IsRunning = true;
                        dp.BeginExecute(command);
                    }
                    else if (cmdGo.DataContext != null) 
                    {//this happens if the report data is already on client side and we're only changing options that can be addressed on client side
                        ReportData rd = cmdGo.DataContext as ReportData;
                        cmdGo.DataContext = rd;
                        reportViewerControl2.SetContent(rd.ReportContent(RadioAlignHorizontal.IsChecked == true, CheckBoxShowId.IsChecked == true
                                                , CheckBoxShowOldId.IsChecked == true
                                                , CheckBoxShowUncoded.IsChecked != true, CheckBoxBullets.IsChecked == true, tboxInfoTag.Text
                                                , (ComboBoxOrderBy.SelectedItem as ComboBoxItem).Content.ToString()
                                                , cbRiskOfBias.IsChecked == true
                                                , (App.Current.Resources["CodeSetsData"] as CslaDataProvider).Data as ReviewSetsList
                                                , CheckBoxShowTitle.IsChecked == true ? true : false
                                                , CheckBoxShowAbstract.IsChecked == true ? true : false
                                                , CheckBoxShowYear.IsChecked == true ? true : false
                                                , CheckBoxShowShortTitle.IsChecked == true ? true : false
                                                ) );
                        windowReports2.ShowDialog();
                        cmdGo.IsEnabled = true;
                    }
                    else
                    {
                        DataPortal<ReportData> dp = new DataPortal<ReportData>();
                        dp.FetchCompleted += (o, e2) =>
                            {
                                BusyLoading.IsRunning = false;
                                if (e2.Error != null)
                                {
                                    RadWindow.Alert(e2.Error.Message);
                                }
                                else if (e2.Object != null)
                                {
                                    ReportData rd = e2.Object as ReportData;
                                    cmdGo.DataContext = rd;
                                    reportViewerControl2.SetContent(rd.ReportContent(RadioAlignHorizontal.IsChecked == true, CheckBoxShowId.IsChecked == true
                                                , CheckBoxShowOldId.IsChecked == true
                                                , CheckBoxShowUncoded.IsChecked != true, CheckBoxBullets.IsChecked == true, tboxInfoTag.Text
                                                , (ComboBoxOrderBy.SelectedItem as ComboBoxItem).Content.ToString()
                                                , cbRiskOfBias.IsChecked == true
                                                , (App.Current.Resources["CodeSetsData"] as CslaDataProvider).Data as ReviewSetsList
                                                , CheckBoxShowTitle.IsChecked == true ? true : false
                                                , CheckBoxShowAbstract.IsChecked == true ? true : false
                                                , CheckBoxShowYear.IsChecked == true ? true : false
                                                , CheckBoxShowShortTitle.IsChecked == true ? true : false
                                                ) );
                                    windowReports2.ShowDialog();
                                    cmdGo.IsEnabled = true;
                                }
                            };
                        BusyLoading.IsRunning = true;
                        // if showoutcomes
                        //if question -> get reportdata, showing outcomes summary
                        //if answer -> use the old system to get the export table.
                        //else use new sytem
                        //all you need is if (CheckBoxShowOutcomes.IsChecked && (ComboBoxReports.SelectedItem as Report).ReportType == "Answer") -> use old code.

                        dp.BeginFetch(new ReportDataSelectionCriteria((ComboBoxReports.SelectedItem as Report).ReportType == "Question"
                                                                     , ItemIdList
                                                                     , (ComboBoxReports.SelectedItem as Report).ReportId
                                                                     , (ComboBoxOrderBy.SelectedItem as ComboBoxItem).Content.ToString()
                                                                     , codesSelectControlReports.SelectedAttributeSet() != null ?
                                                                            codesSelectControlReports.SelectedAttributeSet().AttributeId : 0
                                                                     , codesSelectControlReports.SelectedAttributeSet() != null ?
                                                                            codesSelectControlReports.SelectedAttributeSet().SetId : 0
                                                                     , RadioAlignHorizontal.IsChecked == true ? true : false
                                                                     , CheckBoxShowId.IsChecked == true ? true : false
                                                                     , CheckBoxShowOldId.IsChecked == true ? true : false
                                                                     , CheckBoxShowOutcomes.IsChecked == true ? true : false
                                                                     , CheckBoxShowTitle.IsChecked == true ? true : false
                                                                     , CheckBoxShowAbstract.IsChecked == true ? true : false
                                                                     , CheckBoxShowYear.IsChecked == true ? true : false
                                                                     , CheckBoxShowShortTitle.IsChecked == true ? true : false
                                                                     ));
                    }
                }

            }
        }

        private void ComboBoxReports_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            Report report = ComboBoxReports.SelectedItem as Report;
            if (report != null)
            {
                if (report.ReportType == "Question")
                {
                    cbRiskOfBias.IsEnabled = true;
                }
                else
                {
                    cbRiskOfBias.IsChecked = false;
                    cbRiskOfBias.IsEnabled = false;
                }
            }

            cmdGo.IsEnabled = true;
            cmdGo.DataContext = null;
        }

        private void comboBoxSelectWhichItems_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (cmdGo != null) cmdGo.DataContext = null;
            if (comboBoxSelectWhichItems != null)
            {
                switch (comboBoxSelectWhichItems.SelectedIndex)
                {
                    case 0:
                        dialogReportsGrid.RowDefinitions[2].MaxHeight = 0;
                        codesSelectControlReports.ClearSelection();
                        break;
                    case 1:
                        dialogReportsGrid.RowDefinitions[2].MaxHeight = 0;
                        codesSelectControlReports.ClearSelection();
                        break;
                    case 2:
                        dialogReportsGrid.RowDefinitions[2].MaxHeight = 35;
                        break;
                }
            }
        }

        

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (CloseWindowRequest != null)
                CloseWindowRequest.Invoke(this, EventArgs.Empty);
        }

        private void CheckBoxShowTitle_Checked(object sender, RoutedEventArgs e)
        {
            if (cmdGo != null) cmdGo.DataContext = null;
        }

        private void ComboBoxOrderBy_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (ComboBoxOrderBy == null) return;
            ComboBoxItem selection = ComboBoxOrderBy.SelectedItem as ComboBoxItem;
            if (selection == null) return;
            string SortBy = selection.Content.ToString();
            if (SortBy == "Title")
            {
                CheckBoxShowTitle.IsChecked = true;
                CheckBoxShowTitle.IsEnabled = false;
                CheckBoxShowYear.IsEnabled = true;
                CheckBoxShowId.IsEnabled = true;
                CheckBoxShowOldId.IsEnabled = true;
                CheckBoxShowShortTitle.IsEnabled = true;
            }
            else if (SortBy == "Year")
            {
                CheckBoxShowYear.IsChecked = true;
                CheckBoxShowTitle.IsEnabled = true;
                CheckBoxShowYear.IsEnabled = false;
                CheckBoxShowId.IsEnabled = true;
                CheckBoxShowOldId.IsEnabled = true;
                CheckBoxShowShortTitle.IsEnabled = true;
            }
            else if (SortBy == "Short title")
            {
                CheckBoxShowShortTitle.IsChecked = true;
                CheckBoxShowTitle.IsEnabled = true;
                CheckBoxShowYear.IsEnabled = true;
                CheckBoxShowId.IsEnabled = true;
                CheckBoxShowOldId.IsEnabled = true;
                CheckBoxShowShortTitle.IsEnabled = false;
            }
            else if (SortBy == "Item Id")
            {
                CheckBoxShowId.IsChecked = true;
                CheckBoxShowYear.IsEnabled = true;
                CheckBoxShowTitle.IsEnabled = true;
                CheckBoxShowId.IsEnabled = false;
                CheckBoxShowOldId.IsEnabled = true;
                CheckBoxShowShortTitle.IsEnabled = true;
            }
            else if (SortBy == "Imported Id")
            {
                CheckBoxShowOldId.IsChecked = true;
                CheckBoxShowYear.IsEnabled = true;
                CheckBoxShowTitle.IsEnabled = true;
                CheckBoxShowId.IsEnabled = true;
                CheckBoxShowOldId.IsEnabled = false;
                CheckBoxShowShortTitle.IsEnabled = true;
            }
            else
            {
                CheckBoxShowYear.IsEnabled = true;
                CheckBoxShowTitle.IsEnabled = true;
                CheckBoxShowId.IsEnabled = true;
                CheckBoxShowOldId.IsEnabled = true;
                CheckBoxShowShortTitle.IsEnabled = true;
            }
        }        
    }
}
