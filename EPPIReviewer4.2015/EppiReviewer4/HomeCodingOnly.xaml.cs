using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Linq;
using BusinessLibrary.BusinessClasses;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.Docking;
using Telerik.Windows.Controls.GridView;
using Csla.Silverlight;
using Telerik.Windows.Data;
using System.Windows.Media;
using Csla.Xaml;

namespace EppiReviewer4
{
    public partial class HomeCodingOnly : UserControl
    {
        public event EventHandler<ReviewSelectedEventArgs> LoginToNewReviewRequested;
        public SelectionCriteria SelectionCritieraItemList = null;
        private RadWColumnSelect windowColumnSelect = new RadWColumnSelect();
        private CustomFilterDescriptor customFilterDescriptor;
        private RadWindow windowCoding = new RadWindow();
        private EppiReviewer4.dialogCoding dialogCodingControl = new EppiReviewer4.dialogCoding();
        private Grid codingContent = new Grid();
        private RadWindow windowReportsDocuments = new RadWindow();
        private dialogReportViewer reportViewerControlDocuments = new dialogReportViewer();
        private RadWindow windowItemReportWriter = new RadWindow();
        private dialogItemReportWriter dialogItemReportWriterControl = new dialogItemReportWriter();
        private RadWCodingOnlyHelp COHelpWindow = new RadWCodingOnlyHelp();
        private RadWItemsGridSelectWarning windowItemsGridSelectWarning = new RadWItemsGridSelectWarning();
        private int ToDoSetID = -1;

        //first bunch of lines to make the read-only UI work
        //public BusinessLibrary.Security.ReviewerIdentity ri;
        //public bool HasWriteRights
        //{
        //    get { return ri.HasWriteRights(); }
        //}
        //end of read-only ui hack
        public HomeCodingOnly()
        {
            InitializeComponent();
            //the following two lines sett the reviewer identity at App level, used also to bind enabled property to "HasWriteRights" (was read-only hack)
            App theApp = (Application.Current as App);
            theApp.ri = Csla.ApplicationContext.User.Identity as BusinessLibrary.Security.ReviewerIdentity;
            
            DialogMyInfo2.setForCodingOnly();
            DialogMyInfo2.cmdStartScreening_Clicked += new EventHandler<EventArgs>(DialogMyInfo2_cmdStartScreening_Clicked);
            Thickness thk = new Thickness(20);

            //dialogCoding
            windowCoding.Header = "Document details";
            windowCoding.WindowStateChanged += new EventHandler(Helpers.WindowHelper.MaxOnly_WindowStateChanged);
            windowCoding.Style = Application.Current.Resources["CustomRadWindowStyle"] as Style;
            windowCoding.WindowState = WindowState.Maximized;
            windowCoding.ResizeMode = ResizeMode.NoResize;
            windowCoding.Closed += new EventHandler<WindowClosedEventArgs>(windowCoding_Closed);
            windowCoding.RestrictedAreaMargin = thk;
            windowCoding.IsRestricted = true;
            dialogCodingControl.CloseWindowRequest += dCoding_CloseWindowRequest;
            dialogCodingControl.PrepareCodingOnly();
            //dialogCodingControl.RunTrainingCommandRequest += dCoding_RunTrainingCommandRequest;
            codingContent.Children.Add(dialogCodingControl);
            windowCoding.Content = codingContent;
            //end of dialogCoding

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

            // prepare windowItemReportWriter
            windowItemReportWriter.Header = "Item coding reports";
            windowItemReportWriter.ResizeMode = ResizeMode.CanResize;
            windowItemReportWriter.Width = 450;
            windowItemReportWriter.Height = 400;
            windowItemReportWriter.RestrictedAreaMargin = thk;
            windowItemReportWriter.IsRestricted = true;
            windowItemReportWriter.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
            Grid girw = new Grid();
            dialogItemReportWriterControl.LaunchReportViewer += new EventHandler<LaunchReportViewerEventArgs>(dialogItemReportWriterControl_LaunchReportViewer);
            girw.Children.Add(dialogItemReportWriterControl);
            windowItemReportWriter.Content = girw;
            //end of windowItemReportWriter

            //COHelpWindow
            
            //end of COHelpWindow


            windowColumnSelect.cmdCloseWindowColumnSelect_Clicked += new EventHandler<RoutedEventArgs>(cmdCloseWindowColumnSelect_Click);
            DialogMyInfo2.LoginToNewReviewRequested += new EventHandler<ReviewSelectedEventArgs>(homeDocuments_TriggerLoginToNewReviewRequested);
            DialogMyInfo2.MouseDownOnMyInfoWorkAllocation += new EventHandler<MouseEventArgs>(MouseDownOnWorkAllocation);
            DialogMyInfo2.GridViewMyWorkAllocation_DataLoad += new EventHandler<EventArgs>(DialogMyInfo2_GridViewMyWorkAllocation_DataLoad);
            windowItemsGridSelectWarning.cmdWindowItemsGridWarningClose_Clicked += new EventHandler<RoutedEventArgs>(cmdWindowItemsGridWarningClose_Click);
            LoadWorkAllocation();
            LoadCodeSetsAndTermsList();
        }

        void DialogMyInfo2_cmdStartScreening_Clicked(object sender, EventArgs e)
        {
            dialogCodingControl.BindScreening();
            windowCoding.ShowDialog();
        }

        void DialogMyInfo2_GridViewMyWorkAllocation_DataLoad(object sender, EventArgs e)
        {
            if (DialogMyInfo2.GridViewMyWorkAllocation.HasItems)
            {
                WorkAllocationContactList wac = DialogMyInfo2.GridViewMyWorkAllocation.ItemsSource as WorkAllocationContactList;
                //unhook this event
                DialogMyInfo2.GridViewMyWorkAllocation_DataLoad -= DialogMyInfo2_GridViewMyWorkAllocation_DataLoad;
                foreach (WorkAllocation wa in wac)
                {
                    if (wa.TotalRemaining > 0)
                    {
                        
                        //load this!
                        GetWorkAllocation(wa, "Remaining");
                        break;
                    }
                }
            }
        }
        private void GetWorkAllocation(WorkAllocation wa, string col)
        {
            string method = "", desc = "";
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
                //DocumentListPane.SelectedIndex = 0;
                LoadItemList();
            }
        }
        public void MouseDownOnWorkAllocation(object sender, MouseEventArgs args)
        {
            if (((UIElement)args.OriginalSource).ParentOfType<GridViewCell>() != null)
            {
                WorkAllocation wa = ((UIElement)args.OriginalSource).ParentOfType<GridViewCell>().DataContext as WorkAllocation;
                string col = ((UIElement)args.OriginalSource).ParentOfType<GridViewCell>().DataColumn.Header.ToString();
                GetWorkAllocation(wa, col);
            }
        }
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
        public void LoadCodeSetsAndTermsList()
        {
            CslaDataProvider provider = App.Current.Resources["CodeSetsData"] as CslaDataProvider;
            if (provider != null)
                provider.Refresh();
            CslaDataProvider provider2 = App.Current.Resources["TrainingReviewerTermData"] as CslaDataProvider;
            if (provider2 != null)
                provider2.Refresh();
        }
        void homeDocuments_TriggerLoginToNewReviewRequested(object sender, ReviewSelectedEventArgs e)
        {
            LoginToNewReviewRequested.Invoke(this, e);
        }
        
        private void LoadItemList()
        {
            if (SelectionCritieraItemList == null) return;
            CslaDataProvider provider = this.Resources["ItemListData"] as CslaDataProvider;
            provider.FactoryParameters.Clear();
            SelectionCritieraItemList.PageSize = Convert.ToInt32(windowColumnSelect.UpDownPageSize.Value);
            provider.FactoryParameters.Add(SelectionCritieraItemList);
            provider.FactoryMethod = "GetItemList";
            provider.Refresh();
        }
        private void LoadWorkAllocation()
        {
            CslaDataProvider provider = ((CslaDataProvider)App.Current.Resources["WorkAllocationContactListData"]);
            provider.FactoryParameters.Clear();
            provider.FactoryMethod = "GetWorkAllocationContactList";
            provider.Refresh();
            CslaDataProvider provider2 = App.Current.Resources["ReviewInfoData"] as CslaDataProvider;
            provider2.Refresh();
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
            }
        }

        private void CslaDataProvider_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            //if (e.PropertyName == "Data") UpdateDocCount();
        }


        private void ItemsGrid_Filtered(object sender, GridViewFilteredEventArgs e)
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
            CslaDataProvider provider = ((CslaDataProvider)this.Resources["ItemListData"]);
            ItemList iL = provider.Data as ItemList;
            int I = Convert.ToInt32(windowColumnSelect.UpDownPageSize.Value);
            if (iL != null)
            {
                iL.PageSize = I;
                SelectionCritieraItemList.PageSize = I;
                provider.Refresh();
            }
            windowColumnSelect.Close();
        }


        private void TextBoxItemsGridFind_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.CustomFilterDescriptor.FilterValue = this.TextBoxItemsGridFind.Text;
            this.TextBoxItemsGridFind.Focus();
            ItemsGrid_Filtered(sender, null);
        }

        private void cmdBibliography_Click(object sender, RoutedEventArgs e)
        {
            if (ItemsGrid.SelectedItems.Count > 0)
            {
                string report = "";
                foreach (Item i in ItemsGrid.SelectedItems)
                {
                    report += "<p>" + i.GetCitation() + "</p>";
                }
                reportViewerControlDocuments.SetContent("<html><body>" + report + "</body></html>");
                windowReportsDocuments.ShowDialog();
            }
            else
            {
                RadWindow.Alert("No references currently selected.");
            }
        }

        private void cmdOpenWindowItemReportWriter_Click(object sender, RoutedEventArgs e)
        {
            if (ItemsGrid.SelectedItems.Count == 0)
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
        private void TextBoxItemsGridFind_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBoxItemsGridFind.SelectAll();
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
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
        private void cmdWindowItemsGridWarningClose_Click(object sender, RoutedEventArgs e)
        {
            windowItemsGridSelectWarning.Close();
        }
        private void ItemsGridDataPager_PageIndexChanging(object sender, PageIndexChangingEventArgs e)
        {
            SelectionCritieraItemList.PageNumber = e.NewPageIndex;
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
            if (SelectionCritieraItemList != null && SelectionCritieraItemList.WorkAllocationId != 0)
            {//find the corresponding cell in myworkallocations and highlight it
                int colIndex = 2;
                switch (SelectionCritieraItemList.ListType)
                {
                    case "GetItemWorkAllocationList":
                        colIndex = 2;
                        break;

                    case "GetItemWorkAllocationListStarted":
                        colIndex = 3;
                        break;

                    case "GetItemWorkAllocationListRemaining":
                        colIndex = 4;
                        break;

                    default:
                        break;
                }
                if (colIndex > 1)
                {
                    SolidColorBrush wh = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
                    SolidColorBrush bl = new SolidColorBrush(Color.FromArgb(255, 187, 221, 255));
                    WorkAllocationContactList wac = DialogMyInfo2.GridViewMyWorkAllocation.ItemsSource as WorkAllocationContactList;
                    foreach (WorkAllocation wa in wac)
                    {
                        GridViewRow gvr = DialogMyInfo2.GridViewMyWorkAllocation.GetRowForItem(wa);
                        
                        if (wa.WorkAllocationId == SelectionCritieraItemList.WorkAllocationId)
                        {
                            //save the setId to expand and highlight the codeset that should be used.
                            ToDoSetID = wa.SetId;
                            if (gvr == null) continue;
                            switch (colIndex)
                            {
                                case 3:
                                    gvr.Cells[2].Background = wh;
                                    gvr.Cells[2].FontWeight = FontWeights.Normal;
                                    gvr.Cells[3].Background = bl;
                                    gvr.Cells[3].FontWeight = FontWeights.Bold;
                                    gvr.Cells[4].Background = wh;
                                    gvr.Cells[4].FontWeight = FontWeights.Normal;
                                    break;
                                case 4:
                                    gvr.Cells[2].Background = wh;
                                    gvr.Cells[2].FontWeight = FontWeights.Normal;
                                    gvr.Cells[3].Background = wh;
                                    gvr.Cells[3].FontWeight = FontWeights.Normal;
                                    gvr.Cells[4].Background = bl;
                                    gvr.Cells[4].FontWeight = FontWeights.Bold;
                                    break;
                                default: 
                                    gvr.Cells[2].Background = bl;
                                    gvr.Cells[2].FontWeight = FontWeights.Bold;
                                    gvr.Cells[3].Background = wh;
                                    gvr.Cells[3].FontWeight = FontWeights.Normal;
                                    gvr.Cells[4].Background = wh;
                                    gvr.Cells[4].FontWeight = FontWeights.Normal;
                                    break;
                            
                            }
                        }
                        else
                        {
                            if (gvr == null) continue;
                            gvr.Cells[2].Background = wh;
                            gvr.Cells[2].FontWeight = FontWeights.Normal;
                            gvr.Cells[3].Background = wh;
                            gvr.Cells[3].FontWeight = FontWeights.Normal;
                            gvr.Cells[4].Background = wh;
                            gvr.Cells[4].FontWeight = FontWeights.Normal;
                        }
                    }
                }
                
            }
        }

        private void cmdEditCoding_Click(object sender, RoutedEventArgs e)
        {
            ItemList itemList = ItemsGrid.ItemsSource as ItemList;
            DataItemCollection FilteredItemList = ItemsGrid.Items;
            int currentIndex = itemList.IndexOf(((Button)(sender)).DataContext as Item);

            dialogCodingControl.BindList(itemList, currentIndex, FilteredItemList);
            dialogCodingControl.ExpandByID(ToDoSetID);
            
            windowCoding.ShowDialog();
        }
        
        private void windowCoding_Closed(object sender, WindowClosedEventArgs e)
        {   //not used anymore as annotations are saved transparently
            //dialogCodingControl.CheckNotesMethod();
            LoadWorkAllocation();
            LoadItemList();
        }
        void dCoding_CloseWindowRequest(object sender, EventArgs e)
        {
            
            windowCoding.Close();
           
        }

        private void btCodeOnlyHelp_Click(object sender, RoutedEventArgs e)
        {
            COHelpWindow.ShowDialog();
        }
        public void UnHookMe()
        {
            dialogCodingControl.UnHookMe();
            DialogMyInfo2.UnHookMe();
        }
    }
}
