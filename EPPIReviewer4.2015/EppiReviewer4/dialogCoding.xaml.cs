using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Csla.Silverlight;
using BusinessLibrary.BusinessClasses;
using BusinessLibrary.Security;
using Telerik.Windows.Controls;
using Telerik.Windows.Data;
using Telerik.Windows.Input;
using Telerik.Windows;
using Csla;
using Csla.DataPortalClient;
using System.IO.IsolatedStorage;
using System.IO;
using System.Windows.Browser;
using System.Windows.Data;
using System.Windows.Threading;
//using PDFTron.SilverDox.Controls;
//using PDFTron.SilverDox.Documents.Annotations;
//using PDFTron.SilverDox.Documents.Text;
//using PDFTron.SilverDox.IO;
using System.ComponentModel;
using Telerik.Windows.Documents.Fixed;
using Telerik.Windows.Documents.Fixed.FormatProviders.Pdf;
using Telerik.Windows.Documents.Fixed.FormatProviders;
using System.Windows.Resources;
using Telerik.Windows.Documents.Fixed.Model;
using System.Xml.Linq;
using Telerik.Windows.Documents.Fixed.UI.Extensibility;
using Telerik.Windows.Documents.Fixed.Text;
using Csla.Xaml;
using Telerik.Windows.Documents.FormatProviders.Txt;
using Telerik.Windows.Documents.TextSearch;
using Telerik.Windows.Documents;
using Telerik.Windows.Documents.UI;
using Telerik.Windows.Controls.RichTextBoxUI;
using System.Globalization;

using System.Text;
using System.Threading.Tasks;
using Telerik.Windows.Documents.Fixed.FormatProviders.Text;

namespace EppiReviewer4
{
    public class TrainingEventArgs : EventArgs
    {
        public int currentCount { get; set; }
    }

    public partial class dialogCoding : UserControl, INotifyPropertyChanged
    {//INotifyPropertyChanged is needed to notify UI that haswriterights may have changed (rebind isEn)
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<EventArgs> highlightsChanged;
        public ItemList itemList;
        public DataItemCollection filteredItemList;
        private Int64 currentDocID = 0,  DocToLoadID = 0; 
        //private bool checkNotes = false;
        private bool CodingOnlyMode = false;
        private bool isCTRLpressed = false;
        private NotesCs notesCs;
        private RadWEditAnnotation EditAnnotation;
        private RadWResetPdfCoding windowResetPdfCoding = new RadWResetPdfCoding();
        private Highlights highlights;

        private RadWindow windowReports = new RadWindow();
        private dialogReportViewer reportViewerControl = new dialogReportViewer();
        private RadWConfirmDocDelete windowConfirmDocDelete = new RadWConfirmDocDelete();
        
        private RadWindow WindowRaduploadContainer = new RadWindow();
        private RadUpload RadUp = new RadUpload();
        private RadWCheckArmDelete WindowCheckArmDelete = new RadWCheckArmDelete();
        private RadWCheckTimepointDelete WindowCheckTimepointDelete = new RadWCheckTimepointDelete();

        private double CurrentcodesTreeContainerWidth = 330;

        public List<Int64> ScreenedItemIds;
        public event EventHandler<EventArgs> launchMagBrowser;

        // maybe we should move these to a central location for this kind of thing??
        class TimepointMetricsClass
        {
            public string[] TimepointMetrics
            {
                get { return new string[] { "seconds", "minutes", "hours", "days", "weeks", "months", "years" }; }
            }
            
        }

        //first bunch of lines to make the read-only UI work, modified to make it possible to do PDF text coding possible in Coding-Only
        private BusinessLibrary.Security.ReviewerIdentity ri;
        public bool HasWriteRights
        {
            get
            {
                if (ri == null) return false;
                else
                {
                    return ri.HasWriteRights();//false if user does not have write rights.
                }
            }
        }
        public bool HasWriteRightsOrCodingOnly
        {//used by most controls in here (needed to allow enabling "add/delete code" from text in PDFs
            get
            {
                if (ri == null) return false;
                else
                {
                    return ri.HasWriteRights() && !CodingOnlyMode;//false if user does not have write rights or if control is set for coding only mode.
                }
            }
        }
        //end of read-only ui hack

        public dialogCoding()
        {
            InitializeComponent();
            //two lines to make the read-only UI work
            //add <Button x:Name="isEn" IsEnabled="{Binding HasWriteRights, Mode=OneWay}" ></Button>
            //to the xaml resources section!!
            ri = Csla.ApplicationContext.User.Identity as BusinessLibrary.Security.ReviewerIdentity;
            isEn.DataContext = this;
            isEnorCodingOnly.DataContext = this;
            //end of read-only ui hack
            //pdf coding
            highlights = new Highlights();
            this.notesCs = new NotesCs();
            this.EditAnnotation = new RadWEditAnnotation();
            this.EditAnnotation.Closed += new EventHandler<Telerik.Windows.Controls.WindowClosedEventArgs>(EditAnnotation_Closed);
            ExtensibilityManager.RegisterLayersBuilder(new CustomUILayersBuilder(this.highlights, notesCs, this));
            //end of pdf coding

            //enter RadWindow properties...
            Thickness thk = new Thickness(20);
            windowReports.Header = "Report viewer";
            windowReports.WindowStateChanged += new EventHandler(Helpers.WindowHelper.MaxOnly_WindowStateChanged);
            windowReports.Style = Application.Current.Resources["CustomRadWindowStyle"] as Style;
            windowReports.WindowState = WindowState.Maximized;
            windowReports.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
            windowReports.RestrictedAreaMargin = thk;
            windowReports.IsRestricted = true;
            windowReports.CanClose = true;
            windowReports.Width = 500;
            Grid g1 = new Grid();
            g1.Children.Add(reportViewerControl);
            windowReports.Content = g1;

            WindowRaduploadContainer.Header="Upload Documents";
            WindowRaduploadContainer.ResizeMode= ResizeMode.NoResize;
            WindowRaduploadContainer.WindowStartupLocation= Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
            WindowRaduploadContainer.CanClose= false; 
            WindowRaduploadContainer.CanMove= false;
            WindowRaduploadContainer.IsRestricted = true;
            Grid g2 = new Grid();
            g2.Children.Add(RadUp);
            RadUp.UploadServiceUrl = "../RadUploadHandler.ashx";
            RadUp.BufferSize = 150000;
            RadUp.IsMultiselect = false;
            RadUp.Visibility = System.Windows.Visibility.Visible;
            RadUp.MaxFileSize = 15000000;
            RadUp.IsDeleteEnabled = false;
            RadUp.OverwriteExistingFiles = true;
            RadUp.IsAppendFilesEnabled = false;
            RadUp.FileUploadStarting += RadUp_FileUploadStarting;
            RadUp.LostFocus += RadUp_LostFocus;
            RadUp.FilesSelected += RadUp_FilesSelected;
            RadUp.FileUploaded += RadUp_FileUploaded;
            RadUp.FileUploadFailed += RadUp_FileUploadFailed;
            RadUp.UploadCanceled += RadUp_UploadCanceled;
            RadUp.TargetFolder = "./UserTempUploads";
            WindowRaduploadContainer.Content = g2;
            //end RadWindow properties

            //hook up radW events
            windowConfirmDocDelete.cmdCancelDeleteDoc_Clicked +=new EventHandler<RoutedEventArgs>(cmdCancelDeleteDoc_Click);
            windowConfirmDocDelete.cmdDeleteDoc_Clicked+=new EventHandler<RoutedEventArgs>(cmdDeleteDoc_Click);
            windowResetPdfCoding.Closed += new EventHandler<WindowClosedEventArgs>(windowResetPdfCoding_Closed);
            WindowCheckArmDelete.cmdArmDeletedInWindow += WindowCheckArmDelete_cmdArmDeletedInWindow;
            WindowCheckTimepointDelete.cmdTimepointDeletedInWindow += WindowCheckTimepointDelete_cmdTimepointDeletedInWindow;

            //end hooking up radW events

            this.CloseWindowRequest += new EventHandler(dialogCoding_CloseWindowRequest);
            rich.Loaded += new RoutedEventHandler(rich_Loaded);
            rich.ApplyTemplate();
            codesTreeControl.rich = rich;
            //rich.RichControl.PreparePopupMenu += new PreparePopupMenuEventHandler(RichControl_PreparePopupMenu);
            string filefilt = "Documents|";
            foreach (string flt in Cryptography.whitelist)//this is where the list of allowed extensions is kept, the same list is used server-side.
            {
                if (flt != "txt") filefilt += "*"+flt+";";
            }
            RadUp.Filter = filefilt.Trim(';');

            codesTreeControl.RequestLargerOutcomePane += CodesTreeControl_RequestLargerOutcomePane;
            codesTreeControl.RequestReturnOutcomePaneToNormal += CodesTreeControl_RequestReturnOutcomePaneToNormal;

            TimePointTypeList tptl = new BusinessLibrary.BusinessClasses.TimePointTypeList();
            ComboTimepointMetricSelection.ItemsSource = new TimePointTypeList().TimepointTypes;
            ComboTimepointMetricSelection.SelectedIndex = 5;
        }

        

        private void CodesTreeControl_RequestReturnOutcomePaneToNormal(object sender, EventArgs e)
        {
            codesTreeContainer.Width = CurrentcodesTreeContainerWidth;
        }

        private void CodesTreeControl_RequestLargerOutcomePane(object sender, EventArgs e)
        {
            CurrentcodesTreeContainerWidth = codesTreeContainer.Width;
            codesTreeContainer.Width = 500;
        }

        public void PrepareCodingOnly()
        {
            CodingOnlyMode = true;
            cmdOkAndClose.Visibility = System.Windows.Visibility.Collapsed;
            cmdOk.Visibility = System.Windows.Visibility.Collapsed;
            cmdCancelEditItem.Visibility = System.Windows.Visibility.Collapsed;
            codesTreeControl.ControlContext = "CodingOnly";
            
            //cellStyleCodingReportCodingOnly
            ComparisonButtons.Visibility = System.Windows.Visibility.Collapsed;
            dialogLinkedItemsControl.PrepareCodingOnly();
            dialogItemDetailsControl.PrepareCodingOnly();
            reviewerTerms.PrepareCodingOnly();
            NotifyPropertyChanged("HasWriteRights");//coding only changes the value of haswriterights
            NotifyPropertyChanged("HasWriteRightsOrCodingOnly");
        }
        private void CodingRecordGrid_DataLoaded(object sender, EventArgs e)
        {
            if (CodingOnlyMode)//change the style of the "viewCodingRecord" column so that user can see reports only for his own itemSets.
            {
                GridViewColumn o = CodingRecordGrid.ColumnFromDisplayIndex(5);
                string s = o.ToString();
                o.CellStyle = Resources["cellStyleCodingReportCodingOnly"] as Style;//this style binds isEnabled to isOwn property of ItemSet
            }
            CodingRecordGrid.DataLoaded -= CodingRecordGrid_DataLoaded;//unhook this handler, as the new style is now set and will not change for current instance
        }
        public void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }  
        //void RichControl_PreparePopupMenu(object sender, PreparePopupMenuEventArgs e)
        //{
        //    e.Menu.Items.Clear();
        //}

        void dialogCoding_CloseWindowRequest(object sender, EventArgs e)
        {
            PaneItemDetails[1].Control.IsEnabled = false;
        }

        ItemDocument CurrentTextDocument;
        string NextItemAction = "";

        void rich_Loaded(object sender, RoutedEventArgs e)
        {
            TxtFormatProvider provider = new TxtFormatProvider();
            rich.Document = provider.Import(CurrentTextDocument == null ? "" : CurrentTextDocument.Text);
            //rich.RichControl.Text = CurrentTextDocument == null ? "" : CurrentTextDocument.Text;
        }

        public void BindList(ItemList TheItemList, int index, DataItemCollection TheFilteredList)
        {
            cmdPrevious.Visibility = System.Windows.Visibility.Visible;
            cmdNext.Visibility = System.Windows.Visibility.Visible;
            cmdNextScreening.Visibility = System.Windows.Visibility.Collapsed;
            cmdPreviousScreening.Visibility = System.Windows.Visibility.Collapsed;
            this.dialogItemDetailsControl.IsPriorityScreening = false;
            if (!CodingOnlyMode)
            {
                cmdOk.Visibility = System.Windows.Visibility.Visible;
                cmdOkAndClose.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                cmdOk.Visibility = System.Windows.Visibility.Collapsed;
                cmdOkAndClose.Visibility = System.Windows.Visibility.Collapsed;
            }
            itemList = TheItemList;
            filteredItemList = TheFilteredList;
            EnableControls();
            BindTree(itemList[index] as Item);
            //if (SilverdoxDocumentViewer != null)
                //SilverdoxDocumentViewer = new DocumentViewer();
        }

        public void BindNew(ItemList TheItemList, DataItemCollection TheFilteredList)
        {
            cmdPrevious.Visibility = System.Windows.Visibility.Visible;
            cmdNext.Visibility = System.Windows.Visibility.Visible;
            cmdNextScreening.Visibility = System.Windows.Visibility.Collapsed;
            cmdPreviousScreening.Visibility = System.Windows.Visibility.Collapsed;
            this.dialogItemDetailsControl.IsPriorityScreening = false;
            if (!CodingOnlyMode)
            {
                cmdOk.Visibility = System.Windows.Visibility.Visible;
                cmdOkAndClose.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                cmdOk.Visibility = System.Windows.Visibility.Collapsed;
                cmdOkAndClose.Visibility = System.Windows.Visibility.Collapsed;
            }
            itemList = TheItemList;
            filteredItemList = TheFilteredList;
            Item i = new Item();
            i.TypeId = 14;
            i.CreatedBy = ri.Name;
            i.IsIncluded = true;
            i.DateCreated = DateTime.Now;
            TextBlockIndexInList.Text = "New item";

            this.DataContext = i;
            DisableControls();
            codesTreeControl.BindNew();
            ClearCurrentTextDocument();
            PaneItemDetails.SelectedIndex = 0;
            GetItemDocumentList(DataContext as Item);
            GetMagPaperListData(DataContext as Item);
            tbQuickCitation.DataContext = i;
            GetItemArmList(DataContext as Item);
            GetItemTimepointList(DataContext as Item);
            dialogItemDetailsControl.BindNew(DataContext as Item);
        }

        public void BindScreening()
        {
            cmdPrevious.Visibility = System.Windows.Visibility.Collapsed;
            cmdNext.Visibility = System.Windows.Visibility.Collapsed;
            cmdOk.Visibility = System.Windows.Visibility.Collapsed;
            cmdOkAndClose.Visibility = System.Windows.Visibility.Collapsed;
            cmdNextScreening.Visibility = System.Windows.Visibility.Visible;
            cmdPreviousScreening.Visibility = System.Windows.Visibility.Visible;
            this.dialogItemDetailsControl.IsPriorityScreening = true;
            this.DataContext = null;
            cmdNextScreening_Click(null, new RoutedEventArgs());
            // ADD? do we need new event as above for previous?
        }

        public void DisableControls()
        {
            cmdUploadFile.IsEnabled = false;
            cmdNext.IsEnabled = false;
            cmdPrevious.IsEnabled = false;
            codesTreeControl.IsEnabled = false;
            CodingRecordGrid.DataContext = null;
        }

        public void EnableControls()
        {
            cmdUploadFile.IsEnabled = HasWriteRights;
            codesTreeControl.IsEnabled = true;
            if ((PaneItemDetails.SelectedIndex == 0) && (ScrollViewerCitationDetails.VerticalOffset > 0))
            {
                ScrollViewerCitationDetails.ScrollToVerticalOffset(0);
            }
        }

        public void BindTree(Item i)
        {
            int currentIndex;
            if (PaneItemDetails[5] != null) PaneItemDetails[5].Control.IsEnabled = false;
            codesTreeControl.ShowCodeTxtOptions = false;
            currentIndex = itemList.IndexOf(i);
            if ((currentIndex > -1) && (currentIndex < itemList.Count))
            {
                Item currentItem = itemList[currentIndex] as Item;
                codesTreeControl.MakeBusy();
                currentItem.BeginEdit(); // MUST be before the item is bound to the form
                this.DataContext = itemList[currentIndex];
                if (GetPreviousItem() == null)
                {
                    cmdPrevious.IsEnabled = false;
                }
                else
                {
                    cmdPrevious.IsEnabled = true;
                }
                if (GetNextItem() == null)
                {
                    cmdNext.IsEnabled = false;
                }
                else
                {
                    cmdNext.IsEnabled = true;
                }
                int z = filteredItemList.IndexOf(DataContext as Item);
                int IndexInFilteredList = filteredItemList.IndexOf(DataContext as Item);
                TextBlockIndexInList.Text = "Item " + Convert.ToString(IndexInFilteredList + 1) + " / " + filteredItemList.Count.ToString();

                EnableControls();
                //codesTreeControl.BindItem(DataContext as Item);
                LoadAllItemSets((DataContext as Item).ItemId);
                ClearCurrentTextDocument();
                PaneItemDetails.SelectedIndex = 0;
                
                //(DataContext as Item).GetDocumentList();
                GetItemDocumentList(DataContext as Item);
                GetItemArmList(DataContext as Item);
                GetItemTimepointList(DataContext as Item);
                //GetMagPaperListData(DataContext as Item); now only loading data when tab is clicked
                tbQuickCitation.DataContext = i;


                dialogItemDetailsControl.BindTree(DataContext as Item);
            }
            else
            {
                MessageBox.Show("Error: index of item not in list");
            }
        }

        private Item GetNextItem()
        {
            Item nextItem = null;
            int i = filteredItemList.IndexOf(DataContext as Item);
            if (i + 1 < filteredItemList.Count)
            {
                nextItem = filteredItemList[i + 1] as Item;
            }
            return nextItem;
        }

        private Item GetPreviousItem()
        {
            Item PreviousItem = null;
            int i = filteredItemList.IndexOf(DataContext as Item);
            if (i > 0)
            {
                PreviousItem = filteredItemList[i - 1] as Item;
            }
            return PreviousItem;
        }

        private void ClearCurrentTextDocument()
        {
            CurrentTextDocument = null;
            codesTreeControl.CurrentTextDocument = null;
            codesTreeControl.ShowCodeTxtOptions = false;
            if (PaneItemDetails[1] != null)
            {
                PaneItemDetails[1].Control.IsEnabled = false;
            }
            if (PaneFilesOrComparisons.Header.ToString() != "Live comparisons")
            {
                GridFilesManagement.Visibility = Visibility.Visible;
                GridLiveComparisons.Visibility = Visibility.Collapsed;
                GridShowCodedText.Visibility = Visibility.Collapsed;
                PaneFilesOrComparisons.Header = "Files";
            }
        }

        private void GetItemDocumentList(Item item)
        {
            CslaDataProvider provider = this.Resources["ItemDocumentListData"] as CslaDataProvider;
            provider.FactoryParameters.Clear();
            provider.FactoryParameters.Add(item.ItemId);
            provider.FactoryMethod = "GetItemDocumentList";
            //busyUploading.IsRunning = true;
            GridDocuments.IsEnabled = false;
            provider.Refresh();
        }

        private void GetItemArmList(Item item)
        {
            CslaDataProvider provider = App.Current.Resources["ItemArmsData"] as CslaDataProvider;
            provider.DataChanged -= ItemArmsDataChanged;
            provider.DataChanged += ItemArmsDataChanged;
            provider.FactoryParameters.Clear();
            provider.FactoryParameters.Add(item.ItemId);
            provider.FactoryMethod = "GetItemArmList";
            GridArms.IsEnabled = false;
            provider.Refresh();
        }

        
        private void GetItemTimepointList(Item item)
        {
            CslaDataProvider provider = App.Current.Resources["ItemTimepointsData"] as CslaDataProvider;
            provider.DataChanged -= ItemTimepointsDataChanged;
            provider.DataChanged += ItemTimepointsDataChanged;
            provider.FactoryParameters.Clear();
            provider.FactoryParameters.Add(item.ItemId);
            provider.FactoryMethod = "GetItemTimepointList";
            GridArms.IsEnabled = false;
            provider.Refresh();
        }

        private void LoadAllItemSets(Int64 ItemId)
        {
            DataPortal<ItemSetList> dp = new DataPortal<ItemSetList>();
            dp.FetchCompleted += (o, e2) =>
            {
                if (e2.Object != null)
                {
                    ItemSetList isl = e2.Object as ItemSetList;
                    codesTreeControl.LoadItemAttributes(isl.SetsVisibleToUser);
                    CodingRecordGrid.ItemsSource = isl;
                    CodingRecordGrid.IsEnabled = true;
                    BusyLoadingCodingRecord.IsRunning = false;
                    DoLiveComparisons();
                }
            };
            codesTreeControl.MakeBusy();
            BusyLoadingCodingRecord.IsRunning = true;
            CodingRecordGrid.IsEnabled = false;
            dp.BeginFetch(new SingleCriteria<ItemSetList, Int64>(ItemId));
        }

        private void Saveitem()
        {
            Item thisItem = DataContext as Item;
            thisItem.DateEdited = DateTime.MaxValue;
            thisItem.DateEdited = DateTime.Now;
            thisItem.EditedBy = ri.Name;

            // Check whether 'save and close' has been clicked, but there's nothing to save
            if ((thisItem.IsDirty == false) && (NextItemAction == "Close"))
            {
                if (this.CloseWindowRequest != null)
                    this.CloseWindowRequest.Invoke(this, EventArgs.Empty);
                return;
            }
            int ti =  filteredItemList.IndexOf(thisItem);
            if (itemList.HasSavedHandler == false)
            {
                itemList.HasSavedHandler = true;
                itemList.Saved += (o, e2) =>
                {
                    if (e2.Error != null)
                    {
                        MessageBox.Show(e2.Error.Message);
                    }
                    else
                    {
                        int i = -1;
                        switch (NextItemAction)
                        {
                            case "Close":
                                if (this.CloseWindowRequest != null)
                                    this.CloseWindowRequest.Invoke(this, EventArgs.Empty);
                                return;

                            case "Next":
                                i = filteredItemList.IndexOf(e2.NewObject as Item) + 1;
                                break;

                            case "Previous":
                                i = filteredItemList.IndexOf(e2.NewObject as Item) - 1;
                                break;

                            case "Open":
                                i = filteredItemList.IndexOf(e2.NewObject as Item);
                                break;

                            default:
                                break;
                        }
                        if (i > -1)
                        {
                            BindTree(filteredItemList[i] as Item);
                        }
                        else
                        {
                            if (this.CloseWindowRequest != null)
                                this.CloseWindowRequest.Invoke(this, EventArgs.Empty);
                        }
                    }
                };
            }
            else
            {
                object ooio = itemList.AllowEdit;
                string ts = thisItem.Abstract;
            }
            if (thisItem.IsNew == true)
            {
                itemList.Add(thisItem);
            }
            thisItem.ApplyEdit();
        }

        private void cmdNext_Click(object sender, RoutedEventArgs e)
        {
            if (cmdNext.IsEnabled == true)
            {
                Item thisItem = DataContext as Item;
                if ((thisItem != null) && (thisItem.IsDirty == true))
                {
                    if (MessageBox.Show("Save changes?", "Confirm document save", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                    {
                        NextItemAction = "Next";
                        Saveitem();
                    }
                    else
                    {
                        thisItem.CancelEdit();
                        BindTree(GetNextItem());
                    }
                }
                else
                {
                    thisItem.CancelEdit();
                    BindTree(GetNextItem());
                }
            }
        }

        private void cmdPrevious_Click(object sender, RoutedEventArgs e)
        {
            if (cmdPrevious.IsEnabled == true)
            {
                Item thisItem = DataContext as Item;
                if ((thisItem != null) && (thisItem.IsDirty == true))
                {
                    if (MessageBox.Show("Save changes?", "Confirm document save", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                    {
                        NextItemAction = "Previous";
                        Saveitem();
                    }
                    else
                    {
                        thisItem.CancelEdit();
                        BindTree(GetPreviousItem());
                    }
                }
                else
                {
                    thisItem.CancelEdit();
                    BindTree(GetPreviousItem());
                }
            }
        }

        public event EventHandler CloseWindowRequest;
        private void cmdOk_Click(object sender, RoutedEventArgs e)
        {
            NextItemAction = (sender as Button).Tag.ToString();
            Saveitem();
        }

        private void cmdCancelEditItem_Click(object sender, RoutedEventArgs e)
        {
            Item thisItem = DataContext as Item;
            thisItem.CancelEdit();
            if (this.CloseWindowRequest != null)
                this.CloseWindowRequest.Invoke(this, EventArgs.Empty);
        }

        
        private void contextMenuRich_ItemClick(object sender, Telerik.Windows.RadRoutedEventArgs e)
        {
            RadMenuItem item = e.OriginalSource as RadMenuItem;
            if (item == null) return;
            if (item.Header == null) return;
            else if (item.Header.ToString() == "Send Selection to Reference Search")
            {
                //ReferenceSearch.inTxt.Text = rich.RichControl.Document.GetText(rich.RichControl.Document.Selection);
                ReferenceSearch.inTxt.Text = rich.Document.Selection.GetSelectedText();
                PaneItemDetails.SelectedIndex = 2;
            }
            else if (item.Header.ToString() == "Hightlight All")
            {
                codesTreeControl.HighlightAll();
            }
        }

        private void rich_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            rich_MouseLeftButtonUp2();
        }
        private void rich_MouseLeftButtonUp2()
        {
            string codes = "";
            codes = codesTreeControl.CodesInSelection();
            //}
            TextBox tb = new TextBox();
            tb.IsReadOnly = true;
            tb.TextWrapping = TextWrapping.Wrap;
            tb.Text = codes;
            GridShowCodedTextContent.Children.Clear();
            GridShowCodedTextContent.Children.Add(tb);
        }
        //private void rich_SelectionChanged(object sender, EventArgs e)
        //{
        //    rich.SelectionChanged -= rich_SelectionChanged;
        //    string codes = "";
        //    //if (rich.Document.Selection.GetSelectedText().Length > 0)
        //    //{
        //    codes = codesTreeControl.CodesInSelection();
        //    //}
        //    TextBox tb = new TextBox();
        //    tb.IsReadOnly = true;
        //    tb.TextWrapping = TextWrapping.Wrap;
        //    tb.Text = codes;
        //    GridShowCodedTextContent.Children.Clear();
        //    GridShowCodedTextContent.Children.Add(tb);
        //    rich.SelectionChanged += new EventHandler(rich_SelectionChanged);
        //}
        private void cmdViewText_Click(object sender, RoutedEventArgs e)
        {
            CurrentTextDocument = (sender as Button).DataContext as ItemDocument;
            codesTreeControl.ShowCodeTxtOptions = true;
            TxtFormatProvider provider = new TxtFormatProvider();
            rich.Document = provider.Import(CurrentTextDocument == null ? "" : CurrentTextDocument.Text);
            //ReplaceAllMatches("\r\n", "\n");
            codesTreeControl.CurrentTextDocument = CurrentTextDocument;
            //rich.RichControl.Text = CurrentTextDocument.Text;
            // new method:
            //1 fetch itemattributeallfulltextDetails
            DataPortal<ItemAttributesAllFullTextDetailsList> dp = new DataPortal<ItemAttributesAllFullTextDetailsList>();
            dp.FetchCompleted += (o, e2) =>
            {
                if (e2.Error != null)
                {
                    RadWindow.Alert(e2.Error.Message);
                }
                else
                {//2 place that info into the ItemSets
                    ItemSetList isla = CodingRecordGrid.ItemsSource as ItemSetList;
                    isla.AddFullTextData(e2.Object as ItemAttributesAllFullTextDetailsList);
                    //3 call the new system to loadItemAttributes in codestreecontrol
                    codesTreeControl.LoadItemAttributes(isla.SetsVisibleToUser);
                    CodingRecordGrid.ItemsSource = isla;
                }
                PaneItemDetails[1].Control.IsEnabled = true;
                PaneItemDetails.SelectedIndex = 1;
                GridFilesManagement.Visibility = Visibility.Collapsed;
                GridLiveComparisons.Visibility = Visibility.Collapsed;
                GridShowCodedText.Visibility = Visibility.Visible;
                GridShowCodedTextContent.Children.Clear();
                PaneFilesOrComparisons.Header = "Codes applied in selection";
            };
            //BusyLoadingCodingRecord.IsRunning = true;
            //CodingRecordGrid.IsEnabled = false;
            dp.BeginFetch(new SingleCriteria<ItemAttributesAllFullTextDetailsList, Int64>(-(DataContext as Item).ItemId));
            
            //4 proceed as before
            
        }
        //private void ReplaceAllMatches(string toSearch, string toReplaceWith)
        //{
        //    this.rich.Document.Selection.Clear(); // this clears the selection before processing
        //    DocumentTextSearch search = new DocumentTextSearch(this.rich.Document);
        //    List<Telerik.Windows.Documents.TextSearch.TextRange> rangesTrackingDocumentChanges = new List<Telerik.Windows.Documents.TextSearch.TextRange>();
        //    foreach (var textRange in search.FindAll(toSearch))
        //    {
        //        if (textRange == null) continue;
        //        Telerik.Windows.Documents.TextSearch.TextRange newRange =
        //            new Telerik.Windows.Documents.TextSearch.TextRange(new DocumentPosition(textRange.StartPosition, true), new DocumentPosition(textRange.EndPosition, true));
        //        rangesTrackingDocumentChanges.Add(newRange);
        //    }
        //    foreach (var textRange in rangesTrackingDocumentChanges)
        //    {
        //        this.rich.Document.Selection.AddSelectionStart(textRange.StartPosition);
        //        this.rich.Document.Selection.AddSelectionEnd(textRange.EndPosition);
        //        this.rich.Insert(toReplaceWith);
        //        textRange.StartPosition.Dispose();
        //        textRange.EndPosition.Dispose();
        //    }
        //}
        //private void cmdViewPdf_Click(object sender, RoutedEventArgs e)
        //{
        //    Button bt = (sender as Button);
        //    if (bt.Content.ToString() == "") return;
        //    string passwd = (App.Current.Resources["UserLogged"] as LoginSuccessfulEventArgs).Password;
        //    string revID = Cryptography.Encrypt(ri.ReviewId.ToString(), passwd);
        //    revID = HttpUtility.UrlEncode(revID);
        //    string DocID = Cryptography.Encrypt((sender as Button).Tag.ToString(), passwd);
        //    DocID = HttpUtility.UrlEncode(DocID);
        //    string boh = Application.Current.Host.Source.AbsolutePath;
        //    Uri tmpuri = new Uri( "getbin.aspx?U=" + HttpUtility.UrlEncode(ri.Ticket) 
        //                           + "&ID="+ revID
        //                           + "&DID=" + DocID, UriKind.Relative);
        //    System.Windows.Browser.HtmlPage.Window.Invoke("LoadURLNewWindow", tmpuri);
            
        //}
        //private void cmdViewPdf_Click_1(object sender, RoutedEventArgs e)
        //{
        //    HyperlinkButton hb = sender as HyperlinkButton;
        //    string w = hb.TargetName;
        //    //hb.NavigateUri.IsAbsoluteUri
        //}
        
        private void ViewDox_Click(object sender, RoutedEventArgs e)
        {
            Button bt = (sender as Button);
            Int64 bigI;
            //check that current tag makes sense and save current doc ID to use it when saving free notes
            if (bt.Tag != null && Int64.TryParse(bt.Tag.ToString(), out bigI))
                DocToLoadID = Int64.Parse(bt.Tag.ToString());
            else return;
            ViewDoxLoad(DocToLoadID);
            
        }
        //private void CheckNotesMethod(object sender)
        //{
            
            
        //}
        //public void OnLoadAsyncCallback(Exception ex)
        //{
        //    BusySilverdoxDocumentViewer.IsRunning = false;
        //    DocToLoadID = 0;
        //    if (ex != null)
        //    {
        //        //An error has occurred
        //        //PaneItemDetails[5].IsEnabled = false;
        //        //PaneItemDetails.SelectedIndex = 0;
        //        SilverdoxDocumentViewerTitle.Text = "";
        //        Telerik.Windows.Controls.RadWindow.Alert("Error loading document: " 
        //            + Environment.NewLine
        //            + "This might happen if the PDF you are using is malformed or does not follow"
        //            + Environment.NewLine
        //            + "the correct PDF specifications."
        //            + Environment.NewLine
        //            + "Please ignore this error if the document does display correctly."
        //            + Environment.NewLine
        //            + "Otherwise, please contact user support. The internal error message is:"
        //            + Environment.NewLine
        //            + ">" +ex.Message +"<");
        //        return;
        //    }
        //    SilverdoxDocumentViewer.IsEnabled = true;
        //    SilverdoxDocumentViewer.SetFitMode(DocumentViewer.FitModes.None, DocumentViewer.FitModes.None);
        //    //SilverdoxDocumentViewer.ToolMode = DocumentViewer.ToolModes.AnnotationCreateSticky;
        //    SilverdoxDocumentViewer.ToolMode = DocumentViewer.ToolModes.PanAndAnnotationEdit;
        //    SilverdoxDocumentViewer.LayoutUpdated -= loadAnnotations;
        //    SilverdoxDocumentViewer.LayoutUpdated += loadAnnotations;
            
            
        //}

        //private void DoxBtAdd_Click(object sender, RoutedEventArgs e)
        //{
        //    SilverdoxDocumentViewer.ToolMode = DocumentViewer.ToolModes.AnnotationCreateSticky;
        //    SilverdoxDocumentViewer.Cursor = Cursors.Stylus;
        //    checkNotes = true;
        //    if (SilverdoxDocumentViewer.AnnotationManager.DefaultAnnotation != null)
        //    {
        //        SilverdoxDocumentViewer.AnnotationManager.DefaultAnnotation.Subject = "Annotation";
        //    }
        //}

        //private void DoxBtDelete_Click(object sender, RoutedEventArgs e)
        //{
        //    foreach (Annotation a in SilverdoxDocumentViewer.AnnotationManager.SelectedAnnotations)
        //    { 
        //        SilverdoxDocumentViewer.AnnotationManager.RemoveAnnotation(a);
        //        checkNotes = true;
        //    }
        //}

        //private void DoxBtSaveAnnotations_Click(object sender, RoutedEventArgs e)
        //{
        //    GetCurrentDocBO().FreeNotesStream = SilverdoxDocumentViewer.AnnotationManager.SaveAnnotations();
        //    GetCurrentDocBO().BeginSave();
        //}
        private ItemDocument GetCurrentDocBO()
        {
            ItemDocumentList idl = GridDocuments.ItemsSource as ItemDocumentList;
            if (idl == null || currentDocID == 0) return null;
            foreach (ItemDocument iDoc in idl)
            {
                if (iDoc.ItemDocumentId == currentDocID) return iDoc;
            }
            return null;
        }
        void RegisterForNotification(string propertyName, FrameworkElement element, PropertyChangedCallback callback)
        {
            Binding b = new Binding(propertyName) { Source = element };
            var prop = DependencyProperty.RegisterAttached(
                "ListenAttached" + propertyName,
                typeof(object),
                typeof(UserControl),
                new System.Windows.PropertyMetadata(callback));

            element.SetBinding(prop, b);
        }
        
        //private void loadAnnotations(object sender, EventArgs e)
        //{
            
            
        //    ItemDocument iDoc = GetCurrentDocBO();
        //    SilverdoxDocumentViewerTitle.Text = iDoc.Title;
        //    if (SilverdoxDocumentViewer.Document != null
        //        && SilverdoxDocumentViewer.Document.Pages != null
        //        && SilverdoxDocumentViewer.Document.Pages.Count > 0
        //        && iDoc != null
        //        && iDoc.FreeNotesStream != null
        //        && iDoc.FreeNotesStream.Length > 0)
        //    {
        //        if (SilverdoxDocumentViewer.AnnotationManager != null) SilverdoxDocumentViewer.AnnotationManager.ClearAnnotations();
        //        try
        //        {
        //            SilverdoxDocumentViewer.AnnotationManager.LoadAnnotations(new MemoryStream(iDoc.FreeNotesStream));
        //            SilverdoxDocumentViewer.LayoutUpdated -= loadAnnotations;
        //            checkNotes = true;
        //        }
        //        catch
        //        {
        //            if (iDoc.FreeNotesStream == null || iDoc.FreeNotesStream.Length < 1)
        //                SilverdoxDocumentViewer.LayoutUpdated -= loadAnnotations;// stop listening
        //        }
        //        //SilverdoxDocumentViewer.AnnotationManager.CurrentAnnotationPage.LayoutUpdated +=new EventHandler(CurrentAnnotationPage_LayoutUpdated);
        //            //+= new TextCompositionEventHandler(CurrentAnnotationPage_TextInput);
        //    }
        //    else if (iDoc.FreeNotesStream == null || iDoc.FreeNotesStream.Length < 1)
        //        SilverdoxDocumentViewer.LayoutUpdated -= loadAnnotations;// stop listening

        //    if (SilverdoxDocumentViewer.AnnotationManager.DefaultAnnotation != null)
        //    {
        //        SilverdoxDocumentViewer.AnnotationManager.DefaultAnnotation.Subject = "Annotation";
        //    }
        //}

        public void CheckNotesMethod()
        {
            //if (checkNotes)
            //{
            //    checkNotes = false;
            //    byte[] b = GetCurrentDocBO().FreeNotesStream;
            //    byte[] b2 = SilverdoxDocumentViewer.AnnotationManager.SaveAnnotations();
            //    if (b.Length != b2.Length && b2.Length != 163)
            //    {
            //        RadWindow.Confirm("There may be unsaved changes to your PDF annotations:" + Environment.NewLine + "Do you want to save?", this.CheckNotesWClosed);
            //        return;
            //    }
            //    for (int i = 0; i < b.Length; i++)
            //    {
            //        if (b[i] != b2[i])
            //        {
            //            RadWindow.Confirm("There may be unsaved changes to your annotations:" + Environment.NewLine + "Do you want to save?", this.CheckNotesWClosed);
            //            break;
            //        }
            //    }
            //    //if (GetCurrentDocBO().FreeNotesStream != SilverdoxDocumentViewer.AnnotationManager.SaveAnnotations())
            //    //{
            //    //    RadWindow.Confirm("There may be unsaved changes to your PDF annotations:" + Environment.NewLine + "Do you want to save?", this.CheckNotesWClosed);
            //    //}
            //}
            
        }
        private void ViewDoxLoad(Int64 DocIDin)
        {
            currentDocID = DocIDin;
            
            
            //string passwd = (App.Current.Resources["UserLogged"] as LoginSuccessfulEventArgs).Password;
            //string revID = Cryptography.Encrypt(ri.ReviewId.ToString(), passwd);
            //revID = HttpUtility.UrlEncode(revID);
            //string DocID = Cryptography.Encrypt(DocIDin.ToString(), passwd);
            //DocID = HttpUtility.UrlEncode(DocID);
            //string boh = Application.Current.Host.Source.AbsolutePath;

            

            //Uri tmpuri = new Uri("../getbin.aspx?V=" + HttpUtility.UrlEncode(ri.Ticket)
            //                       + "&ID=" + revID
            //                       + "&DID=" + DocID, UriKind.Relative);
            //System.Windows.Browser.HtmlPage.Window.Invoke("LoadURLNewWindow", tmpuri);
           // HttpStreamingPartRetriever myHttpPartRetriever = new HttpStreamingPartRetriever(tmpuri);
            //HttpPartRetriever myHttpPartRetriever = new HttpPartRetriever(tmpuri);
            //SilverdoxDocumentViewer = new DocumentViewer();
            //PaneItemDetails[5].Control.IsEnabled = true;
            //PaneItemDetails.SelectedIndex = 5;
            //BusySilverdoxDocumentViewer.IsRunning = true;
           // SilverdoxDocumentViewer.IsEnabled = false;
            //SilverdoxDocumentViewer.SetFitMode(DocumentViewer.FitModes.Panel, DocumentViewer.FitModes.None);
            //myHttpPartRetriever.OnPartReady += new EventHandler<OnPartReadyEventArgs>(myHttpPartRetriever_OnPartReady);
            
            
            
            //SilverdoxDocumentViewer.LoadAsync(myHttpPartRetriever, OnLoadAsyncCallback);


            //code added for Telerik PDF viewer

            string DocID = DocIDin.ToString();
            Uri tmpuri = new Uri("../getbin.aspx?P=" + HttpUtility.UrlEncode(ri.Ticket)
                                   + "&ID=" + ri.UserId.ToString()
                                   + "&DID=" + DocID, UriKind.Relative);
            //Uri tmpuri = new Uri("../getbin.aspx?P=" + HttpUtility.UrlEncode(ri.Ticket)
            //                       + "&ID=" + revID
            //                       + "&DID=" + DocID, UriKind.Relative);


            this.pdfViewer.DocumentSource = new PdfDocumentSource(tmpuri);
            PaneItemDetails[5].Control.IsEnabled = true;
            PaneItemDetails.SelectedIndex = 5;
            //WebClient webClient = new WebClient();
            //webClient.OpenReadCompleted += this.ReadCompleted;
            //webClient.OpenReadAsync(tmpuri); //end of telerik PDF viewer
            //checkNotes = true;
        }


        
        //private void CheckNotesWClosed( object sender, WindowClosedEventArgs e )
        //{
        //    if (e.DialogResult == true)
        //    {
        //        GetCurrentDocBO().FreeNotesStream = SilverdoxDocumentViewer.AnnotationManager.SaveAnnotations();
        //        GetCurrentDocBO().BeginSave();
        //        //currentDocID = 0;
        //    }
        //    if (DocToLoadID != 0) 
        //    {
        //        Int64 t = DocToLoadID;
        //        DocToLoadID = 0;
        //        ViewDoxLoad(t);
                
        //    }
        //}
        
        //private void SilverdoxDocumentViewer_ReCheck(object sender, RoutedEventArgs e)
        //{
        //    if (SilverdoxDocumentViewer.AnnotationManager != null && SilverdoxDocumentViewer.AnnotationManager.SelectedAnnotations != null) checkNotes = true;
        //}
        //void loadAnnotations(object sender, RoutedEventArgs e)
        //{
        //    ItemDocument iDoc = GetCurrentDocBO();
        //    if (SilverdoxDocumentViewer.Document != null
        //        && SilverdoxDocumentViewer.Document.Pages != null
        //        && SilverdoxDocumentViewer.Document.Pages.Count > 0 
        //        && iDoc != null 
        //        && iDoc.FreeNotesStream != null 
        //        && iDoc.FreeNotesStream.Length > 0)
        //    {
        //        if (SilverdoxDocumentViewer.AnnotationManager != null) SilverdoxDocumentViewer.AnnotationManager.ClearAnnotations();
        //        SilverdoxDocumentViewer.AnnotationManager.LoadAnnotations(new MemoryStream(iDoc.FreeNotesStream));
        //    }
        //    if (SilverdoxDocumentViewer.AnnotationManager.DefaultAnnotation != null)
        //    {
        //        SilverdoxDocumentViewer.AnnotationManager.DefaultAnnotation.Subject = "Free Note";
        //    }
        //}

        //private string fileContent;
        //private byte[] fileBcontent;
        //old version without raduploader
        //private void cmdUploadFile_Click(object sender, RoutedEventArgs e)
        //{
        //    fileContent = "";
        //    fileBcontent = null;
        //    TextBoxFileTitle.Text = "";
        //    TextBoxFileType.Text = "";
        //    TextBoxFileName.Text = "(No file selected)";
        //    windowUploadFile.ShowDialog();
        //}
        

        
//        private void ButtonUploadFile_Click(object sender, RoutedEventArgs e)
//        {
//            if ((TextBoxFileTitle.Text == "") || (TextBoxFileType.Text == ""))
//            {
//                MessageBox.Show("Please enter a file title and type");
//                return;
//            }
//            if (fileContent == "" & fileBcontent == null)
//            {
//                MessageBox.Show(@"Please select a file first.
//(This message may also appear if the selected file is locked by another application.)");
//                return;
//            }
//            else if (fileContent != "")
//            {
//                BusyUpLoading.IsRunning = true;
//                windowUploadFile.IsEnabled = false;
//                DataPortal<ItemDocumentSaveCommand> dp = new DataPortal<ItemDocumentSaveCommand>();
//                ItemDocumentSaveCommand command = new ItemDocumentSaveCommand(
//                    (DataContext as Item).ItemId,
//                    TextBoxFileTitle.Text,
//                    TextBoxFileType.Text,
//                    BusinessLibrary.BusinessClasses.ImportItems.ImportRefs.StripIllegalChars(fileContent));
//                dp.ExecuteCompleted += (o, e2) =>
//                {
//                    BusyUpLoading.IsRunning = false;
//                    windowUploadFile.IsEnabled = true;
//                    if (e2.Error != null)
//                    {
//                        MessageBox.Show(e2.Error.Message);
//                    }
//                    else
//                    {
//                        //(DataContext as Item).GetDocumentList();
//                        GetItemDocumentList(DataContext as Item);

//                        windowUploadFile.Close();
//                    }
//                };
//                dp.BeginExecute(command);
//            }
//            else
//            {
//                BusyUpLoading.IsRunning = true;
//                windowUploadFile.IsEnabled = false;
//                DataPortal<ItemDocumentSaveBinCommand> dp = new DataPortal<ItemDocumentSaveBinCommand>();
//                ItemDocumentSaveBinCommand command = new ItemDocumentSaveBinCommand(
//                    (DataContext as Item).ItemId,
//                    TextBoxFileTitle.Text,
//                    TextBoxFileType.Text,
//                    fileBcontent);
//                dp.ExecuteCompleted += (o, e2) =>
//                {
//                    BusyUpLoading.IsRunning = false;
//                    windowUploadFile.IsEnabled = true;
//                    if (e2.Error != null)
//                    {
//                        string errmsg = "Detail ";
//                        foreach (System.Collections.DictionaryEntry de in e2.Error.Data)
//                            errmsg += String.Format("    The key is '{0}' and the value is: {1}",
//                                                                de.Key, de.Value) + "\n";
//                        errmsg += " INNER: ";
//                        foreach (System.Collections.DictionaryEntry de in e2.Error.InnerException.Data)
//                            errmsg += String.Format("    The key is '{0}' and the value is: {1}",
//                                                                de.Key, de.Value) + "\n";
//                        errmsg += " AAA " + e2.Error.InnerException.Message;
//                        if (e2.Error.InnerException.Message.IndexOf("NotFound") > -1)
//                            MessageBox.Show("Sorry, the file you are uploading is too big, please try with a smaller file.");
//                        else 
//                        MessageBox.Show(errmsg);
//                        //tmp.Text = errmsg;
//                    }
//                    else
//                    {
//                        //(DataContext as Item).GetDocumentList();
//                        GetItemDocumentList(DataContext as Item);
                        
//                        windowUploadFile.Close();
//                    }
//                };
//                dp.BeginExecute(command);
//            }
//        }

//        private void ButtonUploadFileCancel_Click(object sender, RoutedEventArgs e)
//        {
//            windowUploadFile.Close();
//        }

//        private void ButtonOpenFileDialog_Click(object sender, RoutedEventArgs e)
//        {
//            fileBcontent = null;
//            fileContent = "";
//            OpenFileDialog openFileDialog1 = new OpenFileDialog();
//            openFileDialog1.Filter = "All Files (*.*)|*.*";
//            openFileDialog1.Multiselect = false;
//            bool? userClickedOK = openFileDialog1.ShowDialog();
//            if (userClickedOK == true)
//            {
//                TextBoxFileType.Text = openFileDialog1.File.Extension.ToLower();
//                TextBoxFileName.Text = openFileDialog1.File.Name;
//                TextBoxFileTitle.Text = openFileDialog1.File.Name;
//                if (openFileDialog1.File.Extension.ToLower() != ".txt")
//                {
//                    try
//                    {
//                        FileStream fs =  (FileStream)(openFileDialog1.File.OpenRead());
//                        fileBcontent = new byte[fs.Length];
//                        int offset = 0;
//                        int remaining = fileBcontent.Length;
//                        while (remaining > 0)
//                        {
//                            int read = fs.Read(fileBcontent, offset, remaining);
//                            if (read <= 0)
//                                throw new EndOfStreamException
//                                    (String.Format("End of stream reached with {0} bytes left to read", remaining));
//                            remaining -= read;
//                            offset += read;
//                        }
//                    }
//                    catch (Exception ex)
//                    {
//                        MessageBox.Show(ex.Message);
//                    }
//                }
//                else 
//                {
//                    StreamReader reader = new StreamReader(openFileDialog1.File.OpenRead());
//                    while (!reader.EndOfStream)
//                    {
//                        fileContent = reader.ReadToEnd();
//                    }
//                    reader.Close();
//                }
//            }
//        }

        private void cmdDelFile_Click(object sender, RoutedEventArgs e)
        {
            bool warn = false;
            windowConfirmDocDelete.cmdDeleteDoc.Tag = (sender as Button).Tag;
            ItemDocumentDeleteWarningCommand cmd = new ItemDocumentDeleteWarningCommand((long)windowConfirmDocDelete.cmdDeleteDoc.Tag);
            DataPortal<ItemDocumentDeleteWarningCommand> dp = new DataPortal<ItemDocumentDeleteWarningCommand>();
            dp.ExecuteCompleted += (o, e2) =>
            {
                windowConfirmDocDelete.BusyCheckDocDelete.IsRunning = false;
                if (e2.Error != null)
                {
                    windowConfirmDocDelete.Close();
                    RadWindow.Alert(e2.Error.Message);
                }
                else 
                {
                    cmd = e2.Object as ItemDocumentDeleteWarningCommand;
                    if (cmd == null)
                    {//just being safe here, shouldn't happen...
                        windowConfirmDocDelete.Close();
                        return;
                    }
                    if (cmd.NumCodings > 0)
                    {
                        windowConfirmDocDelete.WarningBorder.Background = new SolidColorBrush(Colors.Orange);
                        windowConfirmDocDelete.TextBlockCheckDeleteDocDetails.Text = "This Document appears to have been coded " + cmd.NumCodings + " time(s)." + Environment.NewLine
                                    +"If you delete this file, its coding information will be lost. This action cannot be undone." + Environment.NewLine
                                    + "Proceed?";
                    }
                    else
                    {
                        windowConfirmDocDelete.TextBlockCheckDeleteDocDetails.Text = "This Document does not appear to have been coded. No additional information will be deleted when deleting this Document." + Environment.NewLine
                                                                                    +"Proceed?";
                    }
                }
            };
            
            SolidColorBrush AllClearBg = new SolidColorBrush(Colors.Transparent);
            windowConfirmDocDelete.WarningBorder.Background = AllClearBg;
            windowConfirmDocDelete.BusyCheckDocDelete.IsRunning = true;
            windowConfirmDocDelete.TextBlockCheckDeleteDocDetails.Text = "Loading...";
            windowConfirmDocDelete.ShowDialog();
            dp.BeginExecute(cmd);
        }

        private void cmdDeleteDoc_Click(object sender, RoutedEventArgs e)
        {
            DataPortal<ItemDocumentDeleteCommand> dp = new DataPortal<ItemDocumentDeleteCommand>();
            ItemDocumentDeleteCommand command = new ItemDocumentDeleteCommand((long)((sender as Button).Tag));
            dp.ExecuteCompleted += (o, e2) =>
            {
                if (e2.Error != null)
                    MessageBox.Show(e2.Error.Message);
                //(DataContext as Item).GetDocumentList();
                GetItemDocumentList(DataContext as Item);
                if ((CurrentTextDocument != null) && (CurrentTextDocument.ItemDocumentId == (long)((sender as Button).Tag)))
                {
                    CurrentTextDocument = null;
                    codesTreeControl.CurrentTextDocument = null;
                    codesTreeControl.ShowCodeTxtOptions = false;
                    rich.Document = null;
                }
                if (PaneItemDetails[5] != null && PaneItemDetails[5].Control.IsEnabled)
                {
                    if ((long)(sender as Button).Tag == currentDocID)
                    {
                        pdfViewer.Document = null;
                    }
                }
                windowConfirmDocDelete.Close();
            };
            dp.BeginExecute(command);
        }

        private void cmdCancelDeleteDoc_Click(object sender, RoutedEventArgs e)
        {
            windowConfirmDocDelete.Close();
        }

        private void cmdCodingReport_Click(object sender, RoutedEventArgs e)
        {
            ItemSet itemSet = (sender as Button).DataContext as ItemSet;
            Item item = this.DataContext as Item;
            string Report = "<h1>" + item.ShortTitle + "</h1>";
            //ReviewSetsList rsl = App.Current.Resources["CodeSets"] as ReviewSetsList;
            DataPortal<ItemAttributesAllFullTextDetailsList> dp = new DataPortal<ItemAttributesAllFullTextDetailsList>();
            dp.FetchCompleted += (o, e2) =>
            {
                ItemSetList isla = CodingRecordGrid.ItemsSource as ItemSetList;
                //CodingRecordGrid.ItemsSource = null;
                //CodingRecordGrid.UpdateLayout();
                isla.AddFullTextData(e2.Object as ItemAttributesAllFullTextDetailsList);
                Report += writeCodingRecord(itemSet);
                //System.Windows.Browser.HtmlPage.Window.Invoke("ShowPopup", Report);
                reportViewerControl.SetContent("<html><body>" + Report + "</body></html>");
                windowReports.ShowDialog();
                CodingRecordGrid.IsEnabled = true;
                BusyLoadingCodingRecord.IsRunning = false;
            };
            BusyLoadingCodingRecord.IsRunning = true;
            CodingRecordGrid.IsEnabled = false;
            dp.BeginFetch(new SingleCriteria<ItemAttributesAllFullTextDetailsList, Int64>((DataContext as Item).ItemId));
        }

        private string writeCodingRecord(ItemSet itemSet)
        {
            string report = "<h2>Reviewer: " + itemSet.ContactName + "</h2>" + "<h3>Date: " + DateTime.Now.ToShortDateString() + "</h3>";

            int SetId = itemSet.SetId;
            //ReviewSetsList rsl = App.Current.Resources["CodeSets"] as ReviewSetsList;

            ReviewSet reviewSet = codesTreeControl.GetReviewSet(SetId);
            if (reviewSet != null)
            {
                report += "<p><h1>" + reviewSet.SetName + "</h1></p><p><ul>";
                foreach (AttributeSet attributeSet in reviewSet.Attributes)
                {
                    report += dialogItemReportWriter.writeCodingReportAttributesWithArms(itemSet, attributeSet, "");
                }
                report += "</ul></p>";
                report += itemSet.OutcomeItemList.OutcomesTable();
            }
            return report;
        }

        //private string writeCodingReportAttributes(ItemSet itemSet, AttributeSet attributeSet, string report)
        //{
        //    if (attributeSet.AttributeTypeId > 1)
        //    {
        //        ReadOnlyItemAttribute roia = itemSet.GetItemAttribute(attributeSet.AttributeId);
        //        if (roia != null)
        //        {
        //            report += "<li>" + attributeSet.AttributeName + "<br /><i>" + roia.AdditionalText.Replace("\n", "<br />") + "</i></li>";
        //            report += "<ul>";
        //            foreach (AttributeSet child in attributeSet.Attributes)
        //            {
        //                report = writeCodingReportAttributes(itemSet, child, report);
        //            }
        //            report += "</ul>";
        //        }
        //        else
        //        {
        //            if (CodingReportCheckChildSelected(itemSet, attributeSet) == true) // ie an attribute below this is selected, even though this one isn't
        //            {
        //                report += "<li>" + attributeSet.AttributeName + "</li>";
        //                report += "<ul>";
        //                foreach (AttributeSet child in attributeSet.Attributes)
        //                {
        //                    report = writeCodingReportAttributes(itemSet, child, report);
        //                }
        //                report += "</ul>";
        //            }
        //        }
        //    }
        //    else
        //    {
        //        report += "<li>" + attributeSet.AttributeName + "</li>";
        //        report += "<ul>";
        //        foreach (AttributeSet child in attributeSet.Attributes)
        //        {
        //            report = writeCodingReportAttributes(itemSet, child, report);
        //        }
        //        report += "</ul>";
        //    }
        //    return report;
        //}

        private bool CodingReportCheckChildSelected(ItemSet itemSet, AttributeSet attributeSet)
        {
            if (itemSet != null)
            {
                if (itemSet.GetItemAttribute(attributeSet.AttributeId) != null)
                {
                    return true;
                }
                foreach (AttributeSet child in attributeSet.Attributes)
                {
                    if (CodingReportCheckChildSelected(itemSet, child) == true)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private ItemSet comparison1 = null;
        private ItemSet comparison2 = null;
        private ItemSet comparison3 = null;

        private bool SetComparisons()
        {
            comparison1 = null;
            comparison2 = null;
            comparison3 = null;

            ItemSetList isla = CodingRecordGrid.ItemsSource as ItemSetList;
            if (isla != null)
            {
                foreach (ItemSet itemSet in isla)
                {
                    if (itemSet.IsSelected == true)
                    {
                        if (comparison1 == null)
                            comparison1 = itemSet;
                        else
                            if (comparison2 == null)
                                comparison2 = itemSet;
                            else
                                if (comparison3 == null)
                                    comparison3 = itemSet;
                                else
                                {
                                    MessageBox.Show("A maximum of three comparisons can be run at once.");
                                    return false;
                                }
                    }
                }
                if (comparison1 == null)
                {
                    MessageBox.Show("Nothing selected to compare");
                    return false;
                }
                if (comparison2 == null)
                {
                    MessageBox.Show("You need to select at least two lines to compare");
                    return false;
                }
                if (comparison2.SetId != comparison1.SetId)
                {
                    MessageBox.Show("Selected items must be the same code set");
                    return false;
                }
                if ((comparison3 != null) && (comparison3.SetId != comparison2.SetId))
                {
                    MessageBox.Show("Selected items must be the same code set");
                    return false;
                }
            }
            return true;
        }

        private void cmdRunComparison_Click(object sender, RoutedEventArgs e)
        {
            if (SetComparisons() == true)
            {
                //ReviewSetsList rsl = App.Current.Resources["CodeSets"] as ReviewSetsList;
                DataPortal<ItemAttributesAllFullTextDetailsList> dp = new DataPortal<ItemAttributesAllFullTextDetailsList>();
                dp.FetchCompleted += (o, e2) =>
                    {
                        ItemSetList isla = CodingRecordGrid.ItemsSource as ItemSetList;
                        //CodingRecordGrid.ItemsSource = null;
                        //CodingRecordGrid.UpdateLayout();
                        isla.AddFullTextData(e2.Object as ItemAttributesAllFullTextDetailsList);
                        //CodingRecordGrid.ItemsSource = isla;
                        //CodingRecordGrid.Rebind();
                        SetComparisons();
                        ReviewSet reviewSet = codesTreeControl.GetReviewSet(comparison1.SetId);
                        if (reviewSet != null)
                        {
                            string report = "<p><h1>" + reviewSet.SetName + "</h1></p><p><ul>";
                            foreach (AttributeSet attributeSet in reviewSet.Attributes)
                            {
                                report += writeComparisonReportAttributes(comparison1, comparison2, comparison3, attributeSet);
                            }
                            report += "</ul></p>";
                            System.Windows.Browser.HtmlPage.Window.Invoke("ShowPopup", report);
                        }
                        CodingRecordGrid.IsEnabled = true;
                        BusyLoadingCodingRecord.IsRunning = false;
                    };
                BusyLoadingCodingRecord.IsRunning = true;
                CodingRecordGrid.IsEnabled = false;
                dp.BeginFetch(new SingleCriteria<ItemAttributesAllFullTextDetailsList, Int64>((DataContext as Item).ItemId));
            }
        }

        // structure for this should be the same as for writeCodingRecord. Only difference is no of reviewers (itemsets)
        private string writeComparisonReportAttributes(ItemSet comparison1, ItemSet comparison2, ItemSet comparison3, AttributeSet attributeSet)
        {
            string report = "";
            if (attributeSet.AttributeTypeId > 1)
            {
                bool oneReviewerHasSelected = false;
                // JT changed this 31/01/2019 - it was calling GetItemAttribute - so only bringing back 1 attribute; need a list to account for arms
                List<ReadOnlyItemAttribute> roias = comparison1.GetItemAttributes(attributeSet.AttributeId).OrderBy(o => o.ArmId).ToList();
                foreach (ReadOnlyItemAttribute roia in roias)
                {
                    report += "<li><FONT COLOR='BLUE'>[" + comparison1.ContactName + "] " +
                        attributeSet.AttributeName +
                        (roia.ArmTitle == "" ? "" : " (" + roia.ArmTitle + ")") +
                        "<br /><i>" + roia.AdditionalText + "</i></font></li>";
                    oneReviewerHasSelected = true;
                    if (roia.ItemAttributeFullTextList != null && roia.ItemAttributeFullTextList.Count > 0)
                    {
                        List<ItemAttributeFullTextDetails> ll = roia.ItemAttributeFullTextList.ToList();
                        ll.Sort();
                        report += "<FONT COLOR='BLUE'>" + addFullTextToComparisonReport(ll) + "</FONT>";
                    }
                }
                roias = comparison2.GetItemAttributes(attributeSet.AttributeId).OrderBy(o => o.ArmId).ToList();
                foreach (ReadOnlyItemAttribute roia in roias)
                {
                    report += "<li><FONT COLOR='RED'>[" + comparison2.ContactName + "] " +
                        attributeSet.AttributeName + 
                        (roia.ArmTitle == "" ? "" : " (" + roia.ArmTitle + ")") +
                        "<br /><i>" +  roia.AdditionalText + "</i></font></li>";
                    oneReviewerHasSelected = true;
                    if (roia.ItemAttributeFullTextList != null && roia.ItemAttributeFullTextList.Count > 0)
                    {
                        List<ItemAttributeFullTextDetails> ll = roia.ItemAttributeFullTextList.ToList();
                        ll.Sort();
                        report += "<FONT COLOR='RED'>" + addFullTextToComparisonReport(ll) + "</FONT>";
                    }
                }
                if (comparison3 != null)
                {
                    roias = comparison3.GetItemAttributes(attributeSet.AttributeId).OrderBy(o => o.ArmId).ToList();
                    foreach (ReadOnlyItemAttribute roia in roias)
                    {
                        report += "<li><FONT COLOR='GREEN'>[" + comparison3.ContactName + "] " +
                            attributeSet.AttributeName + 
                            (roia.ArmTitle == "" ? "" : " (" + roia.ArmTitle + ")") +
                            "<br /><i>" +  roia.AdditionalText + "</i></font></li>";
                        oneReviewerHasSelected = true;
                        if (roia.ItemAttributeFullTextList != null && roia.ItemAttributeFullTextList.Count > 0)
                        {
                            List<ItemAttributeFullTextDetails> ll = roia.ItemAttributeFullTextList.ToList();
                            ll.Sort();
                            report += "<FONT COLOR='GREEN'>" + addFullTextToComparisonReport(ll) + "</FONT>";
                        }
                    }
                }
                if (oneReviewerHasSelected == false)
                {
                    if ((CodingReportCheckChildSelected(comparison1, attributeSet) == true) ||
                        (CodingReportCheckChildSelected(comparison2, attributeSet) == true) ||
                        (CodingReportCheckChildSelected(comparison3, attributeSet) == true)) // ie an attribute below this is selected, even though this one isn't
                    {
                        report += "<li>" + attributeSet.AttributeName + "</li>";
                        report += "<ul>";
                        foreach (AttributeSet child in attributeSet.Attributes)
                        {
                            report += writeComparisonReportAttributes(comparison1, comparison2, comparison3, child);
                        }
                        report += "</ul>";
                    }
                }
                else
                {
                    report += "<ul>";
                    foreach (AttributeSet child in attributeSet.Attributes)
                    {
                        report += writeComparisonReportAttributes(comparison1, comparison2, comparison3, child);
                    }
                    report += "</ul>";
                }
            }
            else
            {
                report += "<li>" + attributeSet.AttributeName + "</li>";
                report += "<ul>";
                foreach (AttributeSet child in attributeSet.Attributes)
                {
                    report += writeComparisonReportAttributes(comparison1, comparison2, comparison3, child);
                }
                report += "</ul>";
            }
            return report;
        }
        public static string addFullTextToComparisonReport(List<ItemAttributeFullTextDetails> list)
        {
            string result = "";
            foreach (ItemAttributeFullTextDetails ftd in list)
            {
                result += "<br>" + ftd.DocTitle + ": ";
                if (ftd.IsFromPDF)
                {
                    result += "<span style='font-size:15px;'>" + ftd.Text.Replace("[¬s]", "").Replace("[¬e]", "") + "</span>";
                }
                else
                {
                    result += "<span style='font-family:Courier New; font-size:12px;'>" + ftd.Text + "(from char " + ftd.TextFrom.ToString() + " to char " + ftd.TextTo.ToString()
                                + ")</span>";
                }
            }
            return result;
        }
        

        private void cmdDoLiveComparison_Click(object sender, RoutedEventArgs e)
        {
            GridFilesManagement.Visibility = Visibility.Collapsed;
            GridLiveComparisons.Visibility = Visibility.Visible;
            GridShowCodedText.Visibility = Visibility.Collapsed;
            PaneFilesOrComparisons.Header = "Live comparisons";
            DoLiveComparisons();
        }

        private void cmdCloseLiveComparisons_Click(object sender, RoutedEventArgs e)
        {
            GridLiveComparisons.Visibility = Visibility.Collapsed;
            GridShowCodedText.Visibility = Visibility.Collapsed;
            GridFilesManagement.Visibility = Visibility.Visible;
            PaneFilesOrComparisons.Header = "Files";
        }

        private void DoLiveComparisons()
        {
            if (GridLiveComparisons.Visibility == Visibility.Collapsed)
            {
                return;
            }
            AttributeSetList asl = null;
            AttributeSet attributeSet = codesTreeControl.SelectedAttributeSet();
            if (attributeSet != null)
            {
                asl = attributeSet.Attributes;
            }
            else
            {
                ReviewSet rs = codesTreeControl.SelectedReviewSet();
                if (rs != null)
                {
                    asl = rs.Attributes;
                }
            }
            if ((asl != null) && (asl.Count > 0))
            {
                Grid grid = new Grid();
                RowDefinition rd = new RowDefinition();
                //rd.Height = new GridLength(*);
                grid.RowDefinitions.Add(rd);
                int ColumnCount = -1;

                ItemSetList isla = CodingRecordGrid.ItemsSource as ItemSetList;
                if (isla != null)
                {
                    foreach (ItemSet itemSet in isla)
                    {
                        if (itemSet.SetId == asl[0].SetId)
                        {
                            ColumnDefinition cd = new ColumnDefinition();
                            grid.ColumnDefinitions.Add(cd);
                            ColumnCount++;
                            TextBlock tb1 = new TextBlock();
                            tb1.FontWeight = System.Windows.FontWeights.Bold;
                            tb1.TextWrapping = TextWrapping.Wrap;
                            tb1.Text = itemSet.ContactName;
                            StackPanel sp1 = new StackPanel();
                            sp1.Margin = new Thickness(0, 0, 10, 0);
                            sp1.SetValue(Grid.RowProperty, 0);
                            sp1.SetValue(Grid.ColumnProperty, ColumnCount);
                            sp1.Orientation = Orientation.Vertical;
                            sp1.Children.Add(tb1);
                            foreach (ReadOnlyItemAttribute roia in itemSet.ItemAttributes)
                            {
                                foreach (AttributeSet attribute in asl)
                                {
                                    if (attribute.AttributeId == roia.AttributeId)
                                    {
                                        TextBlock tb = new TextBlock();
                                        tb.Text = attribute.AttributeName;
                                        tb.TextWrapping = TextWrapping.Wrap;
                                        TextBox tba = new TextBox();
                                        tba.FontStyle = FontStyles.Italic;
                                        tba.TextWrapping = TextWrapping.Wrap;
                                        tba.Margin = new Thickness(0, 0, 0, 8);
                                        tba.BorderThickness = new Thickness(0);
                                        tba.AcceptsReturn = true;
                                        tba.IsReadOnly = true;
                                        tba.Text = roia.AdditionalText;
                                        sp1.Children.Add(tb);
                                        sp1.Children.Add(tba);
                                    }
                                }
                            }
                            grid.Children.Add(sp1);
                        }
                    }
                }
                GridLiveComparisonsContent.Children.Clear();
                GridLiveComparisonsContent.Children.Add(grid);
            }
        }

        

        private void codesTreeControl_SelectedItemChanged(object sender, EventArgs e)
        {
            //tbCurrentCode
            
            if (PaneItemDetails[5] != null && PaneItemDetails[5].Control.IsEnabled == true)
            {
                System.Collections.ObjectModel.ObservableCollection<object> selCds = codesTreeControl.SelectedAttributeSets();
                if (selCds != null)
                {
                    foreach (object o in codesTreeControl.SelectedAttributeSets())
                    {
                        if (o is AttributeSet)
                        {
                            tbCurrentCode.DataContext = o;
                            break;
                        }
                    }
                    GetPDFCoding();
                }
            }
            if (GridLiveComparisons.Visibility == Visibility.Visible)
            {
                DoLiveComparisons();
            }
        }
        private void GetPDFCoding()
        {
            highlights.Clear();
            if (highlightsChanged != null) highlightsChanged.Invoke(tbCurrentCode, new EventArgs());
            ItemDocument idoc = GetCurrentDocBO();
            if (idoc == null) return;
            if (this.pdfViewer.Document == null)
            {
                return;
            }
            Object o = tbCurrentCode.DataContext;
            if (o is AttributeSet)
            {
                AttributeSet aset = (AttributeSet)o;
                CslaDataProvider provider = this.Resources["ItemAttributePDFData"] as CslaDataProvider;
                provider.FactoryParameters.Clear();
                if (aset.ItemData == null)
                {
                    ItemAttributePDFList codinglist = provider.Data as ItemAttributePDFList;
                    if (codinglist != null)
                    {
                        provider.DataChanged -= ItemAttributePDFList_DataChanged;
                        codinglist.Clear();
                        provider.DataChanged += new EventHandler(ItemAttributePDFList_DataChanged);
                    }
                    provider.FactoryParameters.Add(idoc.ItemDocumentId);
                    provider.FactoryParameters.Add(-1);
                }
                else
                {
                    provider.FactoryParameters.Add(idoc.ItemDocumentId);
                    provider.FactoryParameters.Add(aset.ItemData.ItemAttributeId);
                }
                
                provider.FactoryMethod = "GetItemAttributePDFList";
                //busyUploading.IsRunning = true;
                pdfViewerToggleBusy(true);
                provider.Refresh();
            }
            if (highlightsChanged != null) highlightsChanged.Invoke(o, new EventArgs());
        }

        private void codesTreeControl_RequestItemAdvance(object sender, EventArgs e)
        {
            if ((cmdNext.IsEnabled == true) && (cmdNext.Visibility == System.Windows.Visibility.Visible))
            {
                cmdNext_Click(sender, new RoutedEventArgs());
            }
            else
            {
                if (cmdNextScreening.Visibility == System.Windows.Visibility.Visible)
                {
                    cmdNextScreening_Click(sender, new RoutedEventArgs());
                }
            }
        }

        private void PaneItemDetails_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (PaneItemDetails.SelectedIndex != 1)
            {
                codesTreeControl.ShowCodeTxtOptions = false;
            }
            else
            {
                codesTreeControl.ShowCodeTxtOptions = true;
            }
            //not used anymore as annotations are saved transparently
            //if (PaneItemDetails.SelectedIndex != 5) 
            //    CheckNotesMethod();
            if (PaneItemDetails.SelectedIndex == 4)
            {
                dialogLinkedItemsControl.RefreshLinkedItemList();
            }
            
        }

        private void Grid_KeyUp(object sender, KeyEventArgs e)
        {
            if (((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift) &&
                ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control))
            {
                switch (e.Key)
                {
                    case Key.D1:
                        codesTreeControl.doCodeKeyDown(0);
                        break;
                    case Key.D2:
                        codesTreeControl.doCodeKeyDown(1);
                        break;
                    case Key.D3:
                        codesTreeControl.doCodeKeyDown(2);
                        break;
                    case Key.D4:
                        codesTreeControl.doCodeKeyDown(3);
                        break;
                    case Key.D5:
                        codesTreeControl.doCodeKeyDown(4);
                        break;
                    case Key.D6:
                        codesTreeControl.doCodeKeyDown(5);
                        break;
                    case Key.D7:
                        codesTreeControl.doCodeKeyDown(6);
                        break;
                    case Key.D8:
                        codesTreeControl.doCodeKeyDown(7);
                        break;
                    case Key.D9:
                        codesTreeControl.doCodeKeyDown(8);
                        break;
                    case Key.D0:
                        codesTreeControl.doCodeKeyDown(9);
                        break;
                    case Key.Right:
                        if (cmdNext.Visibility == System.Windows.Visibility.Visible)
                            cmdNext_Click(sender, new RoutedEventArgs());
                        else
                            cmdNextScreening_Click(sender, new RoutedEventArgs());
                        break;
                    case Key.Up:
                        if (cmdNext.Visibility == System.Windows.Visibility.Visible)
                            cmdNext_Click(sender, new RoutedEventArgs());
                        else
                            cmdNextScreening_Click(sender, new RoutedEventArgs());
                        break;
                    case Key.Left:
                        if (cmdPrevious.Visibility == System.Windows.Visibility.Visible)
                            cmdPrevious_Click(sender, new RoutedEventArgs());
                        break;
                    case Key.Down:
                        if (cmdPrevious.Visibility == System.Windows.Visibility.Visible)
                            cmdPrevious_Click(sender, new RoutedEventArgs());
                        break;
                }
            }
        }

        private void CslaDataProvider_DataChanged(object sender, EventArgs e)
        {
            CslaDataProvider provider = ((CslaDataProvider)this.Resources["ItemDocumentListData"]);
            if (provider.Error != null)
                Telerik.Windows.Controls.RadWindow.Alert(((Csla.Xaml.CslaDataProvider)sender).Error.Message);
            if (provider.IsBusy == false)
                GridDocuments.IsEnabled = true;
        }

        private void cmdCloseShowCodedText_Click(object sender, RoutedEventArgs e)
        {
            GridShowCodedText.Visibility = Visibility.Collapsed;
            GridFilesManagement.Visibility = Visibility.Visible;
            PaneFilesOrComparisons.Header = "Files";
        }

        private void cmdUploadFile_Click(object sender, RoutedEventArgs e)
        {
            

            //RadUpload RadUp = new RadUpload();
            //RadUp.IsMultiselect = false;
            //RadUp.UploadServiceUrl="/RadUploadHandler.ashx";
            //RadUp.TargetFolder = "UserTempUploads";
            //RadUp.AdditionalPostFields.Add("itemId", (DataContext as Item).ItemId.ToString());
            //RadUp.FileUploaded += new EventHandler<FileUploadedEventArgs>(RadUp_FileUploaded);
            //RadUp.ShowFileDialog();

            //RadUp.StartUpload();
            RadUp.OverwriteExistingFiles = true;
            WindowRaduploadContainer.CanClose = true;
            WindowRaduploadContainer.ShowDialog();
            //RadUp.Visibility = System.Windows.Visibility.Visible;
           
            RadUp.ShowFileDialog();
            //RadUp.FilesSelected += new EventHandler<FilesSelectedEventArgs>(RadUp_FilesSelected);
        }

        void RadUp_FilesSelected(object sender, FilesSelectedEventArgs e)
        {
            //RadUp.Visibility = System.Windows.Visibility.Visible;
            if (e.SelectedFiles[0].File == null) return;
            if (!Cryptography.IsAllowedExtension(e.SelectedFiles[0].File.Extension))//this same check is executed also on server side!
            {
                RadUp.Items.Clear();
                WindowRaduploadContainer.Close();
                //RadUp.Visibility = System.Windows.Visibility.Collapsed;
                Telerik.Windows.Controls.RadWindow.Alert("Sorry, upload can't be started:" + Environment.NewLine
                    + e.SelectedFiles[0].File.Extension + Environment.NewLine +
                    "is an unsupported file extension.");
                return;
            }
        }
        private void RadUp_FileUploadFailed(object sender, FileUploadFailedEventArgs e)
        {
            Telerik.Windows.Controls.RadWindow.Alert("Sorry, upload failed." + Environment.NewLine + "Please contact the support team");
            busyUploading.IsRunning = false;
            WindowRaduploadContainer.CanClose = true;
            WindowRaduploadContainer.Close();
        }

        private void RadUp_FileUploadStarting(object sender, FileUploadStartingEventArgs e)
        {
            if (!HasWriteRights)
            {
                RadUp.Items.Clear();
                WindowRaduploadContainer.Close();
                //RadUp.Visibility = System.Windows.Visibility.Collapsed;
                return;
            }
            if (!Cryptography.IsAllowedExtension(e.SelectedFile.File.Extension))//this same check is executed also on server side!
            {
                Telerik.Windows.Controls.RadWindow.Alert("Sorry, upload can't be started:" + Environment.NewLine 
                    +e.SelectedFile.File.Extension + Environment.NewLine +
                    "is an unsupported file extension.");
                //following line should cancel the upload in Q3 release
                //e.Handled = true;
                return;
            }
            WindowRaduploadContainer.CanClose = false;
            busyUploading.IsRunning = true;
            //RadUp.Visibility = System.Windows.Visibility.Visible;
            string id = (DataContext as Item).ItemId.ToString();
            e.FileParameters.Add("itemId", id);
            e.FileParameters.Add("tGuid", ri.Ticket);
            e.FileParameters.Add("UserId", ri.UserId.ToString());


            //string passwd = (App.Current.Resources["UserLogged"] as LoginSuccessfulEventArgs).Password;
            //e.FileParameters.Add("itemId", Cryptography.Encrypt(id, passwd));
            //e.FileParameters.Add("tGuid", ri.Ticket);
            //e.FileParameters.Add("RevID", Cryptography.Encrypt(ri.ReviewId.ToString(), passwd));
        }

        void RadUp_FileUploaded(object sender, FileUploadedEventArgs e)
        {
            
            //Binding b = new Binding("IsBusy");
            //b.Mode = BindingMode.OneWay;
            //b.Source = (this.Resources["ItemDocumentListData"] as ItemDocumentList);
            //busyUploading.SetBinding(Csla.Silverlight.BusyAnimation.IsRunningProperty, b);
            RadUp.Items.Clear();
            WindowRaduploadContainer.Close();
            WindowRaduploadContainer.CanClose = true;
            //RadUp.Visibility = System.Windows.Visibility.Collapsed;
            GetItemDocumentList(DataContext as Item);
            busyUploading.IsRunning = false;
        }

        private void RadUp_UploadCanceled(object sender, RoutedEventArgs e)
        {
            busyUploading.IsRunning = false;
            RadUp.Items.Clear();
            WindowRaduploadContainer.CanClose = true;
            WindowRaduploadContainer.Close();
            //RadUp.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void RadUp_LostFocus(object sender, RoutedEventArgs e)
        {
            //if (RadUp.Items == null || RadUp.Items.Count == 0 || RadUp.CurrentSession.CurrentFile == null)
            //{
            //    RadUp.Items.Clear();
            //    RadUp.Visibility = System.Windows.Visibility.Collapsed;
            //}
            //else RadUp.Visibility = System.Windows.Visibility.Visible;
        }

        private void cmdFindOnGoogle_Click(object sender, RoutedEventArgs e)
        {
            Item i = this.DataContext as Item;
            if (i != null)
            {
                string query = "http://www.google.co.uk/search?q=\"" + i.Title + "\"+" + i.Authors;
                System.Windows.Browser.HtmlPage.Window.Invoke("LoadURLNewWindow", query);
            }
        }

        private void RadUp_ProgressChanged(object sender, EventArgs e)
        {
            
            //RadUp.Visibility = System.Windows.Visibility.Visible;
        }


        private void cmdNextScreening_Click(object sender, RoutedEventArgs e)
        {
            if (ScreenedItemIds == null)
            {
                ScreenedItemIds = new List<Int64>();
            }
            else
            {
                if (this.DataContext as Item != null)
                {
                    int currentIndex = ScreenedItemIds.IndexOf((this.DataContext as Item).ItemId);
                    if (ScreenedItemIds.Count != 0 && currentIndex != ScreenedItemIds.Count - 1)
                    {
                        getItemFromScreenedIndex(currentIndex + 1);
                        return;
                    }
                }
            }

            if (cmdNextScreening.Visibility == System.Windows.Visibility.Visible)
            {
                cmdPrevious.Visibility = System.Windows.Visibility.Collapsed;
                cmdNext.Visibility = System.Windows.Visibility.Collapsed;
                cmdOk.Visibility = System.Windows.Visibility.Collapsed;
                cmdOkAndClose.Visibility = System.Windows.Visibility.Collapsed;
                cmdNextScreening.Visibility = System.Windows.Visibility.Visible;
                itemList = null;
                filteredItemList = null;

                CslaDataProvider provider = App.Current.Resources["ReviewInfoData"] as CslaDataProvider;
                ReviewInfo ri = provider.Data as ReviewInfo;

                DataPortal<TrainingNextItem> dp = new DataPortal<TrainingNextItem>();
                dp.FetchCompleted += (o, e2) =>
                {
                    //BusyLoading.IsRunning = false;
                    if (e2.Error != null)
                    {
                        RadWindow.Alert(e2.Error.Message);
                    }
                    else
                    {
                        if ((e2.Object.Item != null) && (e2.Object.Item.ItemId != 0))
                        {
                            Item previousItem = this.DataContext as Item;
                            Item currentItem = e2.Object.Item;
                           
                            if (ScreenedItemIds.IndexOf(e2.Object.Item.ItemId) == -1)
                            {
                                ScreenedItemIds.Add(e2.Object.Item.ItemId);
                            }

                            if (previousItem != null && (previousItem.ItemId == currentItem.ItemId))
                            {
                                MessageBox.Show("You haven't screened this item yet!");
                                //CloseWindowRequest.Invoke(this, EventArgs.Empty);
                            }
                            else
                            {
                                currentItem.BeginEdit(); // MUST be before the item is bound to the form
                                this.DataContext = currentItem;
                                TextBlockIndexInList.Text = "Item " + e2.Object.Rank.ToString() + " in current list";
                                EnableControls();
                                //codesTreeControl.BindItem(DataContext as Item);
                                LoadAllItemSets((DataContext as Item).ItemId);
                                ClearCurrentTextDocument();
                                PaneItemDetails.SelectedIndex = 0;
                                GetItemDocumentList(DataContext as Item);
                                GetItemArmList(DataContext as Item);
                                //GetMagPaperListData(DataContext as Item); now only loading data when tab is clicked
                                tbQuickCitation.DataContext = DataContext as Item;
                                GetItemTimepointList(DataContext as Item);
                                dialogItemDetailsControl.BindTree(DataContext as Item);
                                GridDocuments.IsEnabled = true;
                                CheckRunTraining(e2.Object.Rank);
                            }
                        }
                        else
                        {
                            MessageBox.Show("Nothing else to screen");
                            CloseWindowRequest.Invoke(this, EventArgs.Empty);
                        }
                    }
                };
                //BusyLoading.IsRunning = true;
                GridDocuments.IsEnabled = false;
                dp.BeginFetch(new SingleCriteria<TrainingNextItem, int>(ri.ScreeningCodeSetId));
            }
        }

        public event EventHandler RunTrainingCommandRequest;
        private void CheckRunTraining(int currentCount)
        {
            TrainingEventArgs tea = new TrainingEventArgs();
            tea.currentCount = currentCount;
            if (RunTrainingCommandRequest != null)
            {
                RunTrainingCommandRequest.Invoke(this, tea);
            }
        }

        private void cmdPreviousScreening_Click(object sender, RoutedEventArgs e)
        {
            int currentIndex = ScreenedItemIds.IndexOf((this.DataContext as Item).ItemId);
            if (currentIndex == 0 || currentIndex == -1)
            {
                MessageBox.Show("No previous item to view");
            }
            else
            {
                getItemFromScreenedIndex(currentIndex - 1);
            }
        }

        private void getItemFromScreenedIndex(int index)
        {
            if (cmdPreviousScreening.Visibility == System.Windows.Visibility.Visible)
            {
                cmdPrevious.Visibility = System.Windows.Visibility.Collapsed;
                cmdNext.Visibility = System.Windows.Visibility.Collapsed;
                cmdOk.Visibility = System.Windows.Visibility.Collapsed;
                cmdOkAndClose.Visibility = System.Windows.Visibility.Collapsed;
                cmdNextScreening.Visibility = System.Windows.Visibility.Visible;
                itemList = null;
                filteredItemList = null;

                DataPortal<TrainingPreviousItem> dp = new DataPortal<TrainingPreviousItem>();
                dp.FetchCompleted += (o, e2) =>
                {
                    if (e2.Error != null)
                    {
                        RadWindow.Alert(e2.Error.Message);
                    }
                    else
                    {
                        if ((e2.Object.Item != null) && (e2.Object.Item.ItemId != 0))
                        {
                            Item previousItem = this.DataContext as Item;
                            Item currentItem = e2.Object.Item;

                            if (previousItem != null && (previousItem.ItemId == currentItem.ItemId))
                            {
                                MessageBox.Show("No previous item to view");
                                CloseWindowRequest.Invoke(this, EventArgs.Empty);
                            }
                            else
                            {
                                currentItem.BeginEdit(); // MUST be before the item is bound to the form
                                this.DataContext = currentItem;
                                TextBlockIndexInList.Text = "(a previous item)";
                                EnableControls();
                                //codesTreeControl.BindItem(DataContext as Item);
                                LoadAllItemSets((DataContext as Item).ItemId);
                                ClearCurrentTextDocument();
                                PaneItemDetails.SelectedIndex = 0;
                                GetItemDocumentList(DataContext as Item);
                                GetItemArmList(DataContext as Item);
                                //GetMagPaperListData(DataContext as Item); now only loading data when tab is clicked
                                tbQuickCitation.DataContext = DataContext as Item;
                                GetItemTimepointList(DataContext as Item);
                                dialogItemDetailsControl.BindTree(DataContext as Item);
                                GridDocuments.IsEnabled = true;
                            }
                        }
                        else
                        {
                            MessageBox.Show("No previous item to screen...");
                            CloseWindowRequest.Invoke(this, EventArgs.Empty);
                        }
                    }
                };
                GridDocuments.IsEnabled = false;
                dp.BeginFetch(new SingleCriteria<TrainingPreviousItem, Int64>(ScreenedItemIds[index]));
            }
        }

        public void UnMarkAsDuplicate(object sender, RoutedEventArgs e)
        {
            Button bt = sender as Button;
            if (bt == null) return;
            Int64 id = (Int64)(bt.Tag);
            if (id == 0) return;
            DataPortal<ItemDuplicateUnDUPCommand> dp = new DataPortal<ItemDuplicateUnDUPCommand>();
            ItemDuplicateUnDUPCommand command = new ItemDuplicateUnDUPCommand(id);
            dp.ExecuteCompleted += (o, e2) =>
            {
                if (e2.Error != null)
                {
                    Telerik.Windows.Controls.RadWindow.Alert(e2.Error.Message);
                }
                else
                {
                    Item itm = DataContext as Item;
                    itm.DateEdited = DateTime.Now;
                    itm.IsItemDeleted = false;
                    NextItemAction = "Open";
                    Saveitem();

                }
            };
            dp.BeginExecute(command);
        }
        //private void ZoomTextBox_KeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.Key == Key.Enter)
        //    {
        //        DocumentViewer viewer = this.DataContext as DocumentViewer;
        //        ((TextBox)sender).GetBindingExpression(TextBox.TextProperty).UpdateSource();
        //    }
        //}

        private void ZoomTextBox_MouseEnter(object sender, MouseEventArgs e)
        {
            if (Application.Current.Host.Content.IsFullScreen)
            {
                //ZoomToolTextBlock.Text = "For security purposes, Silverlight restricts keyboard access during full-screen mode.";
            }
            else
            {
                //ZoomToolTextBlock.Text = "Zoom";
            }
        }

        public void ExpandByID(int SetId)
        {
            codesTreeControl.ExpandByID(SetId);
        }
        private void ItemAttributePDFList_DataChanged(object sender, EventArgs e)
        {
            CslaDataProvider provider = ((CslaDataProvider)this.Resources["ItemAttributePDFData"]);
            if (provider.Error != null)
                Telerik.Windows.Controls.RadWindow.Alert(((Csla.Xaml.CslaDataProvider)sender).Error.Message);
            else if (provider.IsBusy == false) 
            {
                ItemAttributePDFList codinglist = provider.Data as ItemAttributePDFList;
                if (codinglist == null || codinglist.Count < 1 || this.pdfViewer.Document.Pages.Count < 1)
                {
                    pdfViewerToggleBusy(false);
                    return;
                }
                string documentContent = "";
                bool AlreadyTriedToGetContent = false;
                foreach (RadFixedPage page in this.pdfViewer.Document.Pages)
                {
                    if (codinglist.containsPage(page.PageNo))
                    {
                        ItemAttributePDF codingInPage = codinglist.FindPerPage(page.PageNo);
                        if (codingInPage.HasAngularSelections)
                        {
                            //at least one selection was created in the Angular UI => we don't have the start-end char offset values for it...
                            //we will try to reconstruct the intervals in char offsets hoping to get them right, otherwise signal we didn't get them right!
                            //this is necessary to make the ER4 UI cope with selections made in the Angular UI.
                            if (documentContent == "" && !AlreadyTriedToGetContent)
                            {
                                AlreadyTriedToGetContent = true;
                                TextFormatProvider tfProv = new TextFormatProvider();
                                documentContent = tfProv.Export(this.pdfViewer.Document);//this might not work, if Telerik components can't get the text...
                            }
                            if (documentContent != "")
                            {
                                //get the page in question
                                int startOfPage = documentContent.IndexOf("----------- Page" + page.PageNo + " ------------");
                                if (startOfPage != -1)
                                {//the "else" case shouldn't ever happen, if it happens we can't even try
                                    startOfPage = startOfPage + ("----------- Page" + page.PageNo + " ------------").Length + 2;//otherwise our "page" will contain the separating TXT... +2 is because of \r\n...
                                    int endOfPage = documentContent.IndexOf("----------- Page" + (page.PageNo + 1) + " ------------");
                                    if (endOfPage == -1) endOfPage = documentContent.Length;
                                    //else endOfPage = endOfPage - ("----------- Page" + (page.PageNo + 1) + " ------------").Length;
                                    string pageString = documentContent.Substring(startOfPage, endOfPage - startOfPage);
                                    pageString = pageString.Replace("\r\n", " ");
                                    foreach (InPageSelections inPageSel in codingInPage.inPageSelections)
                                    {
                                        if (inPageSel.Start == 0 && inPageSel.End == 0)
                                        {//this was created in Angular, let's try to fix it...
                                            int FoundCount = 0;
                                            int internalIndex = 0, prevIndex = 0;
                                            while (internalIndex != -1 && (FoundCount == 0 || FoundCount == 1))
                                            {
                                                prevIndex = internalIndex;
                                                internalIndex = pageString.IndexOf(inPageSel.SelTxt, prevIndex + (FoundCount == 0 ? 0 : inPageSel.SelTxt.Length));
                                                if (internalIndex != -1)
                                                {
                                                    FoundCount++;
                                                }
                                            }
                                            if (FoundCount == 1)
                                            {//GOOD. This means we found only one instance of the text that was selected: we can update it
                                                inPageSel.Start = prevIndex;
                                                inPageSel.End = inPageSel.Start + inPageSel.SelTxt.Length;
                                                
                                            }
                                        }
                                    }
                                    if (!codingInPage.HasAngularSelections)
                                    {
                                        codingInPage.Shape = new PathGeometry();//see code re PathGeometry just below. We changed this codingInPage by adding new start/end offsets, and we managed to get it all done :-).
                                        //so, given the code below, doing this will make sure we save the changes
                                    }
                                }
                            }
                        }
                        PathGeometry pg = codingInPage.Shape;
                        if (pg == null || pg.Figures == null || pg.Figures.Count == 0)
                        {//for some reason, the shape txt is missing in the db, rebuild it!
                            ItemAttributePDF iap = codinglist.FindPerPage(page.PageNo);
                            PDFCodingHelper.RebuildShape(pdfViewer, page, ref iap);
                            codinglist.SaveItem(iap);
                        }
                        this.highlights.Add(page, pg);
                        if (highlightsChanged != null) highlightsChanged.Invoke(sender, e);
                    }
                    else
                    {
                        this.highlights.Add(page, new PathGeometry { FillRule = FillRule.Nonzero });
                    }
                }
                if (highlightsChanged != null) highlightsChanged.Invoke(sender, e);
                
            }
            pdfViewerToggleBusy(false);
        }
        private void PdfViewer_DocumentChanged(object sender, EventArgs e)
        {
            if (notesCs.Count > 0)
            {
                //SaveAnnotations();
                notesCs.Clear();
            }
            //highlights.Clear();

            if (this.pdfViewer.Document == null)
            {
                return;
            }
            codesTreeControl_SelectedItemChanged(sender, e);
            //PdfMockName = PdfCodingList.GetFakeTitle(this.pdfViewer);
            //codinglist = PdfCodingList.Fetch(PdfMockName);
            GetNotes();
            foreach (RadFixedPage page in this.pdfViewer.Document.Pages)
            {
                //if (codinglist.containsPage(page.PageNo))
                //{
                //    this.highlights.Add(page, codinglist.FindPerPage(page.PageNo).Shape);
                //}
                //else
                //{
                //    this.highlights.Add(page, new PathGeometry { FillRule = FillRule.Nonzero });
                //}
                if (!notesCs.ContainsKey(page))
                {
                    NotesColumn ncl = new NotesColumn(page.PageNo);
                    ncl.DoubleClick += new EventHandler<System.Windows.Input.MouseEventArgs>(nc_DoubleClick);
                    if (tgglShowAnn.Content.ToString() == "Show Annotations")
                    {
                        ncl.Visibility = System.Windows.Visibility.Collapsed;
                    }
                    this.notesCs.Add(page, ncl);
                }
            }
        }
        private void GetNotes()
        {
            if (notesCs == null) notesCs = new NotesCs();
            XDocument doc;
            ItemDocument iDoc = GetCurrentDocBO();
            Stream stream = new MemoryStream(iDoc.FreeNotesStream);
            if (stream.Length > 0) doc = XDocument.Load(stream);
            else return;

            XElement xlist = doc.Element("ArrayOfAnnotation");
            foreach (XElement c in xlist.Elements("Annotation"))
            {
                xAnnotation a = new xAnnotation(c, iDoc.ItemDocumentId);
                a.DoubleClick += new EventHandler<System.Windows.Input.MouseEventArgs>(a_DoubleClick);
                a.dragged += new EventHandler<EventArgs>(AnnotChanged);
                NotesColumn nc;

                if (notesCs.ContainsKey(this.pdfViewer.Document.Pages[a.page - 1]))
                {
                    nc = notesCs[this.pdfViewer.Document.Pages[a.page - 1]];
                    if (tgglShowAnn.Content.ToString() == "Show Annotations")
                    {
                        nc.Visibility = System.Windows.Visibility.Collapsed;
                    }
                }
                else
                {
                    nc = new NotesColumn(a.page);
                    notesCs.Add(this.pdfViewer.Document.Pages[a.page - 1], nc);
                    if (tgglShowAnn.Content.ToString() == "Show Annotations")
                    {
                        nc.Visibility = System.Windows.Visibility.Collapsed;
                    }
                }
                nc.DoubleClick += new EventHandler<System.Windows.Input.MouseEventArgs>(nc_DoubleClick);
                
                nc.ColumnContent.Children.Add(a);
            }
        }
        private void AnnotChanged(object sender, EventArgs e)
        {
            //checkNotes = true;
            SaveAnnotations();
        }
        void nc_DoubleClick(object sender, System.Windows.Input.MouseEventArgs e)
        {
            EditAnnotation.Header = "New Annotation";
            xAnnotation a = new xAnnotation((sender as NotesColumn).Page, e.GetPosition(pdfViewer).Y, "Please Edit", GetCurrentDocBO().ItemDocumentId);
            a.isDeleted = true;
            EditAnnotation.DataContext = a;
            EditAnnotation.ShowDialog();
        }
        void a_DoubleClick(object sender, System.Windows.Input.MouseEventArgs e)
        {
            EditAnnotation.DataContext = sender;
            EditAnnotation.Header = "Edit Annotation";
            EditAnnotation.ShowDialog();
        }
        void EditAnnotation_Closed(object sender, Telerik.Windows.Controls.WindowClosedEventArgs e)
        {
            xAnnotation a = EditAnnotation.DataContext as xAnnotation;
            if ((string)EditAnnotation.Header == "New Annotation")
            {//save new annotation 
                if (a.isDeleted) return;
                
                NotesColumn nc = notesCs[pdfViewer.Document.Pages[a.page - 1]];
                a.DoubleClick += new EventHandler<System.Windows.Input.MouseEventArgs>(a_DoubleClick);
                a.dragged += new EventHandler<EventArgs>(AnnotChanged);
                nc.Children.Add(a);
                nc.Redraw();
            }
            else if (a.isDeleted)
            {
                NotesColumn nc = notesCs[pdfViewer.Document.Pages[(EditAnnotation.DataContext as xAnnotation).page - 1]];
                nc.Children.Remove(EditAnnotation.DataContext as xAnnotation);

            }
            //checkNotes = true;
            SaveAnnotations();
        }
        private void SaveAnnotations()
        {
            //if (checkNotes)
            
            XDocument doc = XDocument.Parse("<?xml version=\"1.0\" encoding=\"utf-8\"?><ArrayOfAnnotation xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"></ArrayOfAnnotation>");
            XElement rootEl = doc.Element("ArrayOfAnnotation");
            foreach (NotesColumn nc in notesCs.Values)
            {
                foreach (xAnnotation a in nc.Children)
                {
                    rootEl.Add(a.ToXML);
                }
            }
            ItemDocument idoc = GetCurrentDocBO();
            if (idoc.FreeNotesXML != doc.ToString())
            {
                idoc.FreeNotesXML = doc.ToString();
                idoc.BeginSave(true);
            }
            
            //checkNotes = false;
        }
        private void pdfViewer_KeyUp(object sender, KeyEventArgs e)
        {
            if (!pdfViewerToolbar.IsEnabled || ! HasWriteRights) return;
            if (e.Key == System.Windows.Input.Key.A && !isCTRLpressed)
            {
                cmdApplyCodeClick(sender, e);
            }
            else if (e.Key == System.Windows.Input.Key.D && !isCTRLpressed)
            {
                cmdRemoveCodeClick(sender, e);
            }
            else if (e.Key == System.Windows.Input.Key.Ctrl)
            {
                isCTRLpressed = false;
            }
        }
        private void pdfViewer_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Ctrl)
            {
                isCTRLpressed = true;
            }
        }
        private void tbFind_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter )
            {
                this.pdfViewer.CommandDescriptors.FindCommandDescriptor.Command.Execute(this.tbFind.Text);
                //this.pdfViewer.Commands.FindCommand.Execute(this.tbFind.Text);
                this.btnPrev.Visibility = System.Windows.Visibility.Visible;
                this.btnNext.Visibility = System.Windows.Visibility.Visible;
            }
        }
        private void cmdApplyCodeClick(object sender, RoutedEventArgs e)
        {
            /* JT experimentation on getting an image from the current page
            ThumbnailFactory fact = new ThumbnailFactory();
            RadFixedPage ss = this.pdfViewer.CurrentPage;
            ImageSource imsource = fact.CreateThumbnail(ss, this.pdfViewer.CurrentPage.Size);
            imagetest.Source = imsource;
            return;
            */

            ReviewInfo rInfo = ((App)(Application.Current)).GetReviewInfo();

            // JT experimenting with document conversion
            /*
            string html = "<html><body><table><tr><td>hello</td><td> world</td></tr></table></body></html>";
            Telerik.Windows.Documents.FormatProviders.Html.HtmlFormatProvider htmlprov = new Telerik.Windows.Documents.FormatProviders.Html.HtmlFormatProvider();
            Telerik.Windows.Documents.Model.RadDocument doc = new Telerik.Windows.Documents.Model.RadDocument();
            doc = htmlprov.Import(html);
            Telerik.Windows.Documents.FormatProviders.Xaml.XamlFormatProvider prov = new Telerik.Windows.Documents.FormatProviders.Xaml.XamlFormatProvider();
            string output = prov.Export(doc);
            return;
            */

            if (this.pdfViewer.Document == null || this.pdfViewer.Document.Selection.IsEmpty)
            {
                return;
            }
            CslaDataProvider provider = ((CslaDataProvider)this.Resources["ItemAttributePDFData"]);
            if (provider.IsBusy ) return;
            ItemAttributePDFList codinglist = provider.Data as ItemAttributePDFList;
            if (codinglist == null)
            {
                codinglist = new ItemAttributePDFList();
            }
            AttributeSet aset;
            Object o = tbCurrentCode.DataContext;
            if (o is AttributeSet)
            {
                aset = (AttributeSet)o;
            }
            else return;
            if (aset.ItemData == null)//item doesn't have the code yet, add, refresh and retry
            {
                ItemAttributeData itemData = new ItemAttributeData();
                itemData.ItemAttributeId = 0;
                itemData.ItemId = (codesTreeControl.DataContext as Item).ItemId;
                itemData.ItemSetId = 0;
                itemData.SetId = aset.SetId;
                itemData.AttributeId = aset.AttributeId;
                itemData.AttributeSetId = aset.AttributeSetId;
                itemData.AdditionalText = "";
                itemData.ArmId = codesTreeControl.ComboArms.Visibility ==
                    Visibility.Visible ? Convert.ToInt64((codesTreeControl.ComboArms.SelectedItem as ComboBoxItem).Tag) : 0;
                DataPortal<ItemAttributeSaveCommand> dp = new DataPortal<ItemAttributeSaveCommand>();
                ItemAttributeSaveCommand command = new ItemAttributeSaveCommand("Insert",
                    itemData.AttributeId,
                    itemData.ItemSetId,
                    itemData.AdditionalText,
                    itemData.AttributeId,
                    itemData.SetId,
                    itemData.ItemId,
                    itemData.ArmId,
                    rInfo);
                dp.ExecuteCompleted += (o2, e2) =>
                {
                    codesTreeControl.BusyLoading.IsRunning = false;
                    if (e2.Error != null)
                    {
                        //MessageBox.Show(e2.Error.Message);
                        RadWindow.Alert("Warning: there was a problem saving your data." + Environment.NewLine + "Please close this window and try again.");
                        //MessageBox.Show("Warning: there was a problem saving your data. Please close this window and try again.");
                    }
                    else
                    {
                        itemData.ItemAttributeId = e2.Object.ItemAttributeId;
                        itemData.ItemSetId = e2.Object.ItemSetId;
                        foreach (ItemSet iSet in codesTreeControl.CurrentItemData) // need to add it to the list as well as a pointer from the codestree for when people switch between arms / study
                        {
                            if (iSet.SetId == itemData.SetId)
                            {
                                // a bit painful - we need to create a readonly object and add to a read only list...
                                ReadOnlyItemAttribute newIa = ReadOnlyItemAttribute.ReadOnlyItemAttribute_From_ItemAttributeData(itemData);
                                iSet.ItemAttributes.AddToReadOnlyItemAttributeList(newIa);
                                break;
                            }
                        }
                        aset.IsSelected = true;
                        aset.ItemData = itemData;
                        cmdApplyCodeClick(sender, e);
                    }
                };
                codesTreeControl.BusyLoading.IsRunning = true;
                dp.BeginExecute(command);
                return;//we exit here as we'll recall the whole thing in the ExecuteCompleted block above.
            }
            List<int[]> perPageIndex = PerPageSelections();//this gets the start/end list of existing selections.
            RadFixedPage start = this.pdfViewer.Document.Selection.StartPosition.Page;
            RadFixedPage end = this.pdfViewer.Document.Selection.EndPosition.Page;
            TextPosition tp = this.pdfViewer.Document.Selection.EndPosition;
            ItemDocument iDoc = GetCurrentDocBO();
            int ii = 0;
            //cycle through pages in selection.
            for (int i = start.PageNo - 1; i <= end.PageNo - 1; i++)
            {
                ItemAttributePDF ppas = codinglist.FindPerPage(i + 1);
                if (ppas != null && ppas.HasAngularSelections)
                {//we can't (reliably) do this, show an alert and let the user deal with it.
                    string Msg = "Unfortunately, this code has some selected text from the Web interface" + Environment.NewLine
                                + "that EPPI-Reviewer 4 cannot interpret. This will make it impossible " + Environment.NewLine
                                + "to select further text in EPPI-Reviewer 4 using this code. If you need" + Environment.NewLine
                                + "to make further edits using this code in EPPI-Reviewer 4 you can use " + Environment.NewLine
                                + "the 'Reset' button (page only) and redo the selections." + Environment.NewLine;
                    RadWindow.Alert(Msg);
                    return;
                }
                //use char "start/end" offsets to create page selections, one by one
                PDFCodingHelper.selectOnPage(this.pdfViewer, this.pdfViewer.Document.Pages[i], perPageIndex[ii][0], perPageIndex[ii][1]);
                PathGeometry s = this.pdfViewer.Document.Selection.GetSelectionGeometry(this.pdfViewer.Document.Pages[i]);
                if (!highlights.ContainsKey(this.pdfViewer.Document.Pages[i]))
                {
                    highlights.Add(this.pdfViewer.Document.Pages[i], new PathGeometry { FillRule = FillRule.Nonzero });
                }

                
                if (ppas == null)
                {
                    ppas = new ItemAttributePDF(iDoc.ItemDocumentId, aset.ItemData.ItemAttributeId, i + 1);
                }
                
                PDFCodingHelper.AddSel(s, perPageIndex[ii][0]
                                , perPageIndex[ii][1]
                                , this.pdfViewer.Document.Selection.GetSelectedText()
                                , this.pdfViewer.Document.Pages[i]
                                , this.pdfViewer, ref ppas);
                ii++;
                if (!codinglist.containsPage(i + 1))
                {
                    codinglist.Add(ppas);
                }
                
                ppas.Saved -= ppas_Saved;
                ppas.Saved +=new EventHandler<Csla.Core.SavedEventArgs>(ppas_Saved);
                
                pdfViewerToggleBusy(true);
                codinglist.SaveItem(ppas);
                //ppas.BeginSave(true);//check this, you may want to do it always
                
                this.highlights[this.pdfViewer.Document.Pages[i]].Figures.Clear();
                //this.highlights[this.pdfViewer.Document.Pages[i]] = ppas.Shape;
                PathGeometry pg = ItemAttributePDF.makeGeom(ppas.ShapeTxt);
                while (pg.Figures.Count > 0)
                {
                    PathFigure pf = pg.Figures[0];
                    pg.Figures.RemoveAt(0);
                    this.highlights[this.pdfViewer.Document.Pages[i]].Figures.Add(pf);
                }
            }
            this.pdfViewer.Document.CaretPosition.MoveToPosition(tp);
            if (highlightsChanged != null) highlightsChanged.Invoke(sender, e);
        }
        private void ppas_Saved(object o, Csla.Core.SavedEventArgs e2)
        {
            pdfViewerToggleBusy(false);
            if (e2.Error != null)
            {
                //MessageBox.Show(e2.Error.Message);
                RadWindow.Alert("Warning: there was a problem saving your data." + Environment.NewLine + "Please close this window and try again.");
                //MessageBox.Show("Warning: there was a problem saving your data. Please close this window and try again.");
            }
            //else if (e2.NewObject != null)
            //{
            //    CslaDataProvider provider = ((CslaDataProvider)this.Resources["ItemAttributePDFData"]);
            //    //if (provider.IsBusy) return;
            //    ItemAttributePDFList codinglist = provider.Data as ItemAttributePDFList;
            //    if (codinglist != null)
            //    {
            //        if (codinglist.containsPage((e2.NewObject as ItemAttributePDF).Page))
            //        {
            //            //codinglist.Remove(codinglist.FindPerPage((e2.NewObject as ItemAttributePDF).Page));
            //            ItemAttributePDF old = codinglist.FindPerPage((e2.NewObject as ItemAttributePDF).Page);
                        
            //            old = e2.NewObject as ItemAttributePDF;
            //        }
            //        else
            //        {
            //            codinglist.Add(e2.NewObject as ItemAttributePDF);
            //        }
            //    }
            //}
        }
        private void pdfViewerToggleBusy(bool isBusy)
        {
            if (isBusy)
            {
                pdfViewerToolbar.IsEnabled = false;
                pdfViewer.IsEnabled = false;
                pdfBusy.Visibility = System.Windows.Visibility.Visible;
                pdfBusy.IsRunning = true;
            }
            else
            {
                pdfViewerToolbar.IsEnabled = true;
                pdfViewer.IsEnabled = true;
                pdfBusy.Visibility = System.Windows.Visibility.Collapsed;
                pdfBusy.IsRunning = false;
            }
        }
        private void cmdRemoveCodeClick(object sender, RoutedEventArgs e)
        {
            if (this.pdfViewer.Document == null || this.pdfViewer.Document.Selection.IsEmpty)
            {
                return;
            }
            CslaDataProvider provider = ((CslaDataProvider)this.Resources["ItemAttributePDFData"]);
            if (provider.IsBusy || provider == null || provider.Data == null) return;
            ItemAttributePDFList codinglist = provider.Data as ItemAttributePDFList;
            if (codinglist.Count < 1) return;
            AttributeSet aset;
            Object o = tbCurrentCode.DataContext;
            if (o is AttributeSet)
            {
                aset = (AttributeSet)o;
            }
            else return;
            TextPosition tp = this.pdfViewer.Document.Selection.EndPosition;
            List<int[]> perPageIndex = PerPageSelections();
            int start = this.pdfViewer.Document.Selection.StartPosition.Page.PageNo;
            int end = this.pdfViewer.Document.Selection.EndPosition.Page.PageNo;
            ItemDocument iDoc = GetCurrentDocBO();
            int ii = 0;
            for (int i = start - 1; i <= end - 1; i++)
            {
                ItemAttributePDF ppas = codinglist.FindPerPage(i + 1);
                if (ppas != null && ppas.HasAngularSelections)
                {//we can't do this, show an alert and let the user deal with it.
                    string Msg = "Unfortunately, this code has some selected text from the Web interface" + Environment.NewLine
                                + "that EPPI-Reviewer 4 cannot interpret. This will make it impossible " + Environment.NewLine
                                + "to un-select text in EPPI-Reviewer 4 using this code. If you need" + Environment.NewLine
                                + "to make further edits using this code in EPPI-Reviewer 4 you can use " + Environment.NewLine
                                + "the 'Reset' button (page only) and redo the selections." + Environment.NewLine;
                    RadWindow.Alert(Msg);
                    return;
                }
                PDFCodingHelper.selectOnPage(this.pdfViewer, this.pdfViewer.Document.Pages[i], perPageIndex[ii][0], perPageIndex[ii][1]);
                PathGeometry s = this.pdfViewer.Document.Selection.GetSelectionGeometry(this.pdfViewer.Document.Pages[i]);
                if (!highlights.ContainsKey(this.pdfViewer.Document.Pages[i]))
                {
                    highlights.Add(this.pdfViewer.Document.Pages[i], new PathGeometry { FillRule = FillRule.Nonzero });
                }
                if (ppas == null)
                {//there is no selection in the page, so there is nothing to unselect here!
                    continue;
                    //ppas = new PdfPageAttrSel(i + 1);
                    ////this.highlights[this.pdfViewer.Document.Pages[i]] = ppas;
                }
                PDFCodingHelper.RemoveSel(s, perPageIndex[ii][0]
                                , perPageIndex[ii][1]
                                , this.pdfViewer.Document.Selection.GetSelectedText()
                                , this.pdfViewer.Document.Pages[i]
                                , this.pdfViewer, ref ppas);
                ii++;
               //if (ppas.inPageSelections.Count != 0 && !codinglist.containsPage(i + 1))
                if (!codinglist.containsPage(i + 1))
                {
                    codinglist.Add(ppas);
                }
                pdfViewerToggleBusy(true);
                ppas.Saved -= ppas_Saved;
                ppas.Saved += new EventHandler<Csla.Core.SavedEventArgs>(ppas_Saved);
                codinglist.SaveItem(ppas);
                //ppas.BeginSave(true);
                //this.highlights[this.pdfViewer.Document.Pages[i]] = new PathGeometry { FillRule = FillRule.Nonzero };
                this.highlights[this.pdfViewer.Document.Pages[i]].Figures.Clear();
                //this.highlights[this.pdfViewer.Document.Pages[i]] = ppas.Shape;
                PathGeometry pg = ItemAttributePDF.makeGeom(ppas.ShapeTxt);
                while (pg.Figures.Count > 0)
                {
                    PathFigure pf = pg.Figures[0];
                    pg.Figures.RemoveAt(0);
                    this.highlights[this.pdfViewer.Document.Pages[i]].Figures.Add(pf);
                }
            }
            this.pdfViewer.Document.CaretPosition.MoveToPosition(tp);
            if (highlightsChanged != null) highlightsChanged.Invoke(sender, e);
        }

        private void ShowAnnotationsClick(object sender, RoutedEventArgs e)
        {
            foreach (NotesColumn cl in this.notesCs.Values)
            {
                if (cl.Visibility == System.Windows.Visibility.Collapsed)
                {
                    tgglShowAnn.Content = "Hide Annotations";
                    cl.Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    tgglShowAnn.Content = "Show Annotations";
                    cl.Visibility = System.Windows.Visibility.Collapsed;
                }
            }
        }
        private List<int[]> PerPageSelections()
        {
            List<int[]> res = new List<int[]>();

            RadFixedPage start = this.pdfViewer.Document.Selection.StartPosition.Page;
            RadFixedPage end = this.pdfViewer.Document.Selection.EndPosition.Page;
            TextPosition tp = this.pdfViewer.Document.Selection.EndPosition;


            //Commented code which I've used to verify that getting text from selection uses the same "text extractor" as textFormatProvider.Export(this.pdfViewer.Document);
            //TextFormatProvider textFormatProvider = new TextFormatProvider();
            //string text = textFormatProvider.Export(this.pdfViewer.Document);
            ////----------- Page1 ------------
            //List<string> pages = text.Split(new[] { "----------- Page1 ------------\r\n", "----------- Page2 ------------" }, StringSplitOptions.RemoveEmptyEntries).ToList<string>();
            //this.pdfViewer.Document.Selection.SetSelectionStart(new TextPosition(this.pdfViewer.Document.Pages[0], 0));
            //this.pdfViewer.Document.Selection.SetSelectionEnd(new TextPosition(this.pdfViewer.Document.Pages[0], pages[0].Length - 100));
            //string wholeselPg = this.pdfViewer.Document.Selection.GetSelectedText();
            //bool areEq = (pages[0].Substring(0, wholeselPg.Length) == wholeselPg);
            //RadWindow.Alert("Are selections equal? " + areEq);
            //Console.Write(text);
            if (start.PageNo > end.PageNo)
            {

                int sind = this.pdfViewer.Document.Selection.EndPosition.Index;
                int eind = this.pdfViewer.Document.Selection.StartPosition.Index;
                //RadFixedPage t = start;
                //start = end;
                //end = t;
                this.pdfViewer.Document.Selection.SetSelectionStart(new TextPosition(end, sind));
                this.pdfViewer.Document.Selection.SetSelectionEnd(new TextPosition(start, eind));
                start = this.pdfViewer.Document.Selection.StartPosition.Page;
                end = this.pdfViewer.Document.Selection.EndPosition.Page;
                tp = this.pdfViewer.Document.Selection.EndPosition;
            }
            else if (start.PageNo == end.PageNo && this.pdfViewer.Document.Selection.StartPosition.Index > this.pdfViewer.Document.Selection.EndPosition.Index)
            {
                PDFCodingHelper.selectOnPage(this.pdfViewer
                                            , this.pdfViewer.Document.Selection.StartPosition.Page
                                            , this.pdfViewer.Document.Selection.EndPosition.Index
                                            , this.pdfViewer.Document.Selection.StartPosition.Index);
                start = this.pdfViewer.Document.Selection.StartPosition.Page;
                end = this.pdfViewer.Document.Selection.EndPosition.Page;
                tp = this.pdfViewer.Document.Selection.EndPosition;
            }

            for (int i = start.PageNo - 1; i <= end.PageNo - 1; i++)
            {
                int[] tarr = { 0, 0 };
                if (i == start.PageNo - 1 && i == end.PageNo - 1)
                {//only one page!
                    tarr[0] = this.pdfViewer.Document.Selection.StartPosition.Index;
                    tarr[1] = this.pdfViewer.Document.Selection.EndPosition.Index;
                }
                else if (i == start.PageNo - 1 && i < end.PageNo - 1)
                {//first page of more than 1
                    tarr[0] = this.pdfViewer.Document.Selection.StartPosition.Index;
                    TextPosition tempTp = new TextPosition(this.pdfViewer.Document.Pages[i + 1], 0);
                    this.pdfViewer.Document.CaretPosition.MoveToPosition(tempTp);
                    int counter = 0;
                    while (this.pdfViewer.Document.CaretPosition.Page.PageNo > i + 1 && counter < 200)
                    {
                        this.pdfViewer.Document.CaretPosition.MoveLineUp();
                        this.pdfViewer.Document.CaretPosition.MoveToLineEnd();
                        counter++;
                    }
                    tarr[1] = this.pdfViewer.Document.CaretPosition.Index;
                }
                else if (i > start.PageNo - 1 && i < end.PageNo - 1)
                {//neither first nor final page
                    tarr[0] = 0;
                    TextPosition tempTp = new TextPosition(this.pdfViewer.Document.Pages[i + 1], 0);
                    this.pdfViewer.Document.CaretPosition.MoveToPosition(tempTp);
                    int counter = 0;
                    while (this.pdfViewer.Document.CaretPosition.Page.PageNo > i + 1 && counter < 200)
                    {
                        this.pdfViewer.Document.CaretPosition.MoveLineUp();
                        this.pdfViewer.Document.CaretPosition.MoveToLineEnd();
                        counter++;
                    }
                    tarr[1] = this.pdfViewer.Document.CaretPosition.Index;
                }
                else if (i > start.PageNo - 1 && i == end.PageNo - 1)
                {//final page of more than 1
                    tarr[0] = 0;
                    tarr[1] = tp.Index;
                }
                res.Add(tarr);
            }
            this.pdfViewer.Document.Selection.SetSelectionStart(new TextPosition(start, res[0][0]));
            this.pdfViewer.Document.Selection.SetSelectionEnd(new TextPosition(end, res[res.Count - 1][1]));
            return res;
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            Object o = tbCurrentCode.DataContext;
            if (!(o is AttributeSet) || pdfViewer.CurrentPage == null) return;
            AttributeSet aset = o as AttributeSet;
            if (aset.ItemData == null) return;
            ItemDocument idoc = GetCurrentDocBO();
            if (idoc == null) return;
            windowResetPdfCoding.hasActed = false;
            windowResetPdfCoding.tbCurrentPage.Text = "PAGE " + pdfViewer.CurrentPage.PageNo.ToString();
            windowResetPdfCoding.tbCurrentPage.Tag = pdfViewer.CurrentPage.PageNo;
            windowResetPdfCoding.ItemAttributeID = aset.ItemData.ItemAttributeId;
            windowResetPdfCoding.ItemDocumentID = idoc.ItemDocumentId;
            windowResetPdfCoding.tbCurrentCode.DataContext = tbCurrentCode.DataContext;
            windowResetPdfCoding.ShowDialog();
        }
        void windowResetPdfCoding_Closed(object sender, WindowClosedEventArgs e)
        {
            if (windowResetPdfCoding.hasActed) GetPDFCoding();
        }

        private void btnFindTxt_Click(object sender, RoutedEventArgs e)
        {
            if (tbFindTxt.Text.Length == 0) return;
            //rich.Document.Selection.Clear(); // this clears the selection before processing
            DocumentTextSearch search = new DocumentTextSearch(rich.Document);
            IEnumerable<Telerik.Windows.Documents.TextSearch.TextRange> result = search.FindAll(System.Text.RegularExpressions.Regex.Escape(tbFindTxt.Text));
            tbFindTxt.DataContext = result;
            //IEnumerable<Telerik.Windows.Documents.TextSearch.TextRange>
            
            foreach (Telerik.Windows.Documents.TextSearch.TextRange tr in result)
            {
                if (rich.Document.CaretPosition < tr.StartPosition)
                {
                    rich.Document.Selection.Clear();
                    rich.Document.CaretPosition.MoveToPosition(tr.EndPosition);
                    rich.Document.Selection.AddSelectionStart(tr.StartPosition);
                    rich.Document.Selection.AddSelectionEnd(tr.EndPosition);
                    btnPrevTxt.Visibility = System.Windows.Visibility.Visible;
                    btnNextTxt.Visibility = System.Windows.Visibility.Visible;
                    rich_MouseLeftButtonUp2();
                    return;
                }
            }
            RadWindow.Alert("Not found");
        }

        private void tbFindTxt_KeyUp(object sender, KeyEventArgs e)
        {
            tbFindTxt.DataContext = null;
            btnPrevTxt.Visibility = System.Windows.Visibility.Collapsed;
            btnNextTxt.Visibility = System.Windows.Visibility.Collapsed;
            if (tbFindTxt.Text.Length < 1)
            {
                return;
            }
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                btnFindTxt_Click(sender, new RoutedEventArgs());
            }
        }


        private void btnPrevTxt_Click(object sender, RoutedEventArgs e)
        {
            IEnumerable<Telerik.Windows.Documents.TextSearch.TextRange> result = tbFindTxt.DataContext as IEnumerable<Telerik.Windows.Documents.TextSearch.TextRange>;
            if (result == null)
            {
                btnPrevTxt.Visibility = System.Windows.Visibility.Collapsed;
                btnNextTxt.Visibility = System.Windows.Visibility.Collapsed;
                return;
            }
            List<Telerik.Windows.Documents.TextSearch.TextRange> li = result as List<Telerik.Windows.Documents.TextSearch.TextRange>;
            int i = 0;
            while (i < li.Count)
            {
                Telerik.Windows.Documents.TextSearch.TextRange tr = li[i];
                if (rich.Document.CaretPosition <= tr.EndPosition)
                {
                    if (i == 0)
                    {
                        RadWindow.Alert("Reached the Start of the" + Environment.NewLine + "Document");
                        return;
                    }
                    tr = li[i - 1];
                    rich.Document.Selection.Clear();
                    rich.Document.CaretPosition.MoveToPosition(tr.EndPosition);
                    rich.Document.Selection.AddSelectionStart(tr.StartPosition);
                    rich.Document.Selection.AddSelectionEnd(tr.EndPosition);
                    btnPrevTxt.Visibility = System.Windows.Visibility.Visible;
                    btnNextTxt.Visibility = System.Windows.Visibility.Visible;
                    rich_MouseLeftButtonUp2();
                    return;
                }
                else i++;
            }
            RadWindow.Alert("Not found");
        }

        private void btnNextTxt_Click(object sender, RoutedEventArgs e)
        {
            IEnumerable<Telerik.Windows.Documents.TextSearch.TextRange> result = tbFindTxt.DataContext as IEnumerable<Telerik.Windows.Documents.TextSearch.TextRange>;
            if (result == null)
            {
                btnPrevTxt.Visibility = System.Windows.Visibility.Collapsed;
                btnNextTxt.Visibility = System.Windows.Visibility.Collapsed;
                return;
            }
            foreach (Telerik.Windows.Documents.TextSearch.TextRange tr in result)
            {
                if (rich.Document.CaretPosition < tr.StartPosition)
                {
                    rich.Document.Selection.Clear();
                    rich.Document.CaretPosition.MoveToPosition(tr.EndPosition);
                    rich.Document.Selection.AddSelectionStart(tr.StartPosition);
                    rich.Document.Selection.AddSelectionEnd(tr.EndPosition);
                    btnPrevTxt.Visibility = System.Windows.Visibility.Visible;
                    btnNextTxt.Visibility = System.Windows.Visibility.Visible;
                    rich_MouseLeftButtonUp2();
                    return;
                }
            }
            RadWindow.Alert("Reached the End of the" + Environment.NewLine + "Document");
        }

        private void cmdShowTextHighlight_Click(object sender, RoutedEventArgs e)
        {
            dialogItemDetailsControl.ShowBoldTerms();
            cmdShowEditableBoxes.Visibility = Visibility.Visible;
            cmdShowTextHighlight.Visibility = Visibility.Collapsed;
        }

        private void cmdShowEditableBoxes_Click(object sender, RoutedEventArgs e)
        {
            dialogItemDetailsControl.ShowEditableBoxes();
            cmdShowEditableBoxes.Visibility = Visibility.Collapsed;
            cmdShowTextHighlight.Visibility = Visibility.Visible;
            reviewerTerms.Visibility = Visibility.Collapsed;
        }

        private void dialogItemDetailsControl_ShowTermsClicked(object sender, RoutedEventArgs e)
        {
            if (reviewerTerms.Visibility == Visibility.Collapsed)
            {
                reviewerTerms.Visibility = Visibility.Visible;
                //dialogItemDetailsControl.
            }
            else
            {
                reviewerTerms.Visibility = Visibility.Collapsed;

            }
        }

        private void reviewerTerms_TermsChanged(object sender, RoutedEventArgs e)
        {
            dialogItemDetailsControl.RefreshHighlights();
        }

        //private void cmdTranslate_Click(object sender, RoutedEventArgs e)
        private void cmdTranslate_Click(object sender, RadRoutedEventArgs e)
        {
            Item currentItem = this.DataContext as Item;

            // don't bother translating anything that's only a few characters long
            if (currentItem.Title.Length + currentItem.Abstract.Length < 5)
                return;
            //ComboBoxItem cbi = comboTranslate.SelectedItem as ComboBoxItem;
            RadMenuItem cbi = e.Source as RadMenuItem;
            if (cbi == null) return;

            this.IsEnabled = false;
            dialogItemDetailsControl.BusyTranslate.Visibility = System.Windows.Visibility.Visible;
            dialogItemDetailsControl.BusyTranslate.IsRunning = true;
            DataPortal<TranslationCommand> dp = new DataPortal<TranslationCommand>();
            TranslationCommand command = new TranslationCommand(
                currentItem.Title + Environment.NewLine + "ER4Ab " + currentItem.Abstract, "", cbi.Tag.ToString());
            dp.ExecuteCompleted += (o, e2) =>
            {
                this.IsEnabled = true;
                dialogItemDetailsControl.BusyTranslate.Visibility = System.Windows.Visibility.Collapsed;
                dialogItemDetailsControl.BusyTranslate.IsRunning = false;

                if (e2.Error != null)
                    MessageBox.Show(e2.Error.Message);
                else
                {
                    if ((e2.Object as TranslationCommand).TranslatedText != null && (e2.Object as TranslationCommand).TranslatedText != "")
                    {
                        string trans = (e2.Object as TranslationCommand).TranslatedText;

                        // in some languages the abstract marker is removed
                        if (trans.IndexOf("ER4Ab") > -1)
                        {
                            string[] sep = { "ER4Ab" };
                            string[] transs = trans.Split(sep, StringSplitOptions.None);
                            if (transs.Length > 1)
                            {
                                transs[0] = transs[0].Length > 0 ? "[Translated title] " + transs[0] : "";
                                transs[1] = transs[1].Length > 0 ? "[Translated abstract] " + transs[1] : "";
                                currentItem.Abstract = transs[0] + transs[1] + Environment.NewLine +
                                    "[End translation]" + Environment.NewLine + Environment.NewLine + currentItem.Abstract;
                            }
                            else
                            {
                                currentItem.Abstract = "[Translated text] " + trans + Environment.NewLine +
                                    "[End translation]" + Environment.NewLine + Environment.NewLine + currentItem.Abstract;
                            }
                        }
                        else
                        {
                            currentItem.Abstract = "[Translated text] " + trans + Environment.NewLine +
                                    "[End translation]" + Environment.NewLine + Environment.NewLine + currentItem.Abstract;
                        }
                            
                        NextItemAction = "Open";
                        Saveitem();
                    }
                    else
                    {
                        MessageBox.Show("Sorry, no translation was returned");
                    }
                }
            };
            dp.BeginExecute(command);
        }

        private void ItemArmsDataChanged(object sender, EventArgs e)
        {
            this.IsEnabled = true;
            CslaDataProvider provider = App.Current.Resources["ItemArmsData"] as CslaDataProvider; ;
            if (provider.Error != null)
                Telerik.Windows.Controls.RadWindow.Alert(((Csla.Xaml.CslaDataProvider)sender).Error.Message);
            if (provider.IsBusy == false)
            {
                GridArms.IsEnabled = true;
                tbArmDescriptor.DataContext = null;
                codesTreeControl.ResetArms(provider.Data as ItemArmList);
            }
        }

        private void NewArm_Click(object sender, RoutedEventArgs e)
        {
            CslaDataProvider provider = App.Current.Resources["ItemArmsData"] as CslaDataProvider; ;
            if (tbNewArm.Text != null && provider != null)
            {
                if (tbNewArm.Text == "")
                {
                    RadWindow.Alert("Arm name cannot be empty");
                    return;
                }
                foreach (ItemArm arm in (provider.Data as ItemArmList))
                {
                    if (arm.Title == tbNewArm.Text)
                    {
                        RadWindow.Alert("Arm names must be unique");
                        return;
                    }
                }
                ItemArmList thelist = provider.Data as ItemArmList;
                ItemArm ia = null;

                if (tbArmDescriptor.DataContext == null)
                {
                    ia = new ItemArm();
                    ia.ItemId = (DataContext as Item).ItemId;
                    ia.Ordering = thelist.Count;
                    thelist.Add(ia);
                }
                else
                {
                    ia = tbArmDescriptor.DataContext as ItemArm;
                }
                ia.Title = tbNewArm.Text;
                ia.Saved -= Ia_Saved;
                ia.Saved += Ia_Saved;
                ia.BeginEdit();
                ia.ApplyEdit();
                tbNewArm.Text = "";
                tbArmDescriptor.DataContext = null;
                tbArmDescriptor.Text = "New arm";
            }
        }

        private void Ia_Saved(object sender, Csla.Core.SavedEventArgs e)
        {
            if (e.Error != null)
                Telerik.Windows.Controls.RadWindow.Alert(e.Error.Message);
            ItemArmsDataChanged(sender, e);
        }

        private void CancelArm_Click(object sender, RoutedEventArgs e)
        {
            tbArmDescriptor.Text = "New arm";
            tbArmDescriptor.DataContext = null;
            tbNewArm.Text = "";
        }

        private void cmdEditarm_Click(object sender, RoutedEventArgs e)
        {
            tbArmDescriptor.Text = "Edit arm";
            tbArmDescriptor.DataContext = (sender as Button).DataContext;
            tbNewArm.Text = ((sender as Button).DataContext as ItemArm).Title;
        }

        private void cmdDeletearm_Click(object sender, RoutedEventArgs e)
        {
            ItemArm Deleting = (sender as Button).DataContext as ItemArm;
            if (Deleting == null) return;
            WindowCheckArmDelete.StartChecking(Deleting);
            WindowCheckArmDelete.ShowDialog();
        }
        private void WindowCheckArmDelete_cmdArmDeletedInWindow(object sender, RoutedEventArgs e)
        {
            this.IsEnabled = false;
            WindowCheckArmDelete.Close();
            ItemArm ia = sender as ItemArm;
            if (ia == null) return;
            ia.Saved -= Ia_Saved;
            ia.Saved += Ia_Saved;
            ia.BeginEdit();
            ia.ApplyEdit();
        }

        private void ItemTimepointsDataChanged(object sender, EventArgs e)
        {
            this.IsEnabled = true;
            CslaDataProvider provider = ((CslaDataProvider)App.Current.Resources["ItemTimepointsData"]);
            if (provider.Error != null)
                Telerik.Windows.Controls.RadWindow.Alert(((Csla.Xaml.CslaDataProvider)sender).Error.Message);
            if (provider.IsBusy == false)
            {
                GridTimepoints.IsEnabled = true;
                tbNewTimepoint.DataContext = null;
            }
        }

        private void CancelTimepoint_Click(object sender, RoutedEventArgs e)
        {
            tbNewTimepointValue.Text = "";
            tbNewTimepoint.DataContext = null;
            tbNewTimepoint.Text = "New timepoint";
        }

        private void NewTimepoint_Click(object sender, RoutedEventArgs e)
        {
            CslaDataProvider provider = ((CslaDataProvider)App.Current.Resources["ItemTimepointsData"]);
            if (tbNewTimepoint.Text != null && provider != null)
            {
                float result = 0;
                if (!float.TryParse(tbNewTimepointValue.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out result))
                {
                    RadWindow.Alert("Timepoint value cannot be empty and must be a number");
                    return;
                }
                foreach (ItemTimepoint timepoint in (provider.Data as ItemTimepointList))
                {
                    if (timepoint.TimepointValue.ToString() == tbNewTimepointValue.Text && 
                        timepoint.TimepointMetric == ComboTimepointMetricSelection.SelectedValue.ToString())
                    {
                        RadWindow.Alert("Timepoints must be unique");
                        return;
                    }
                }
                ItemTimepointList thelist = provider.Data as ItemTimepointList;
                ItemTimepoint it = null;

                if (tbNewTimepoint.DataContext == null)
                {
                    it = new ItemTimepoint();
                    it.ItemId = (DataContext as Item).ItemId;
                    thelist.Add(it);
                }
                else
                {
                    it = tbNewTimepoint.DataContext as ItemTimepoint;
                }
                it.TimepointValue = float.Parse(tbNewTimepointValue.Text, CultureInfo.InvariantCulture.NumberFormat);
                it.TimepointMetric = ComboTimepointMetricSelection.SelectedValue.ToString();
                it.Saved -= Ia_Saved;
                it.Saved += Ia_Saved;
                it.BeginEdit();
                it.ApplyEdit();
                tbNewTimepointValue.Text = "";
                tbNewTimepoint.DataContext = null;
                tbNewTimepoint.Text = "New timepoint";
            }
        }

        private void cmdEditTimepoint_Click(object sender, RoutedEventArgs e)
        {
            ItemTimepoint it = (ItemTimepoint)(sender as Button).DataContext;
            if (it != null)
            {
                tbNewTimepoint.Text = "Edit timepoint";
                tbNewTimepointValue.Text = it.TimepointValue.ToString();
                /*
                for (int i=0; i < ComboTimepointMetricSelection.Items.Count; i++)
                {
                    if (ComboTimepointMetricSelection.Items[i].ToString() == it.TimepointMetric)
                    {
                        ComboTimepointMetricSelection.SelectedIndex = i;
                    }
                }
                */
                ComboTimepointMetricSelection.SelectedValue = it.TimepointMetric;
                tbNewTimepoint.DataContext = it;
            }
        }

        public void UnHookMe()
        {
            codesTreeControl.UnHookMe();
        }

        private void WindowCheckTimepointDelete_cmdTimepointDeletedInWindow(object sender, RoutedEventArgs e)
        {
            this.IsEnabled = false;
            WindowCheckTimepointDelete.Close();
            ItemTimepoint it = sender as ItemTimepoint;
            if (it == null) return;
            it.Saved -= It_Saved;
            it.Saved += It_Saved;
            it.BeginEdit();
            it.ApplyEdit();
        }

        private void cmdDeleteTimepoint_Click(object sender, RoutedEventArgs e)
        {
            ItemTimepoint Deleting = (sender as Button).DataContext as ItemTimepoint;
            if (Deleting == null) return;
            WindowCheckTimepointDelete.StartChecking(Deleting);
            WindowCheckTimepointDelete.ShowDialog();
        }

        private void GetMagPaperListData(Item i)
        {
            CslaDataProvider provider = this.Resources["MagPaperListData"] as CslaDataProvider;
            provider.FactoryParameters.Clear();
            MagPaperListSelectionCriteria selectionCriteria = new MagPaperListSelectionCriteria();
            selectionCriteria.ListType = "ItemMatchedPapersList";
            selectionCriteria.ITEM_ID = i.ItemId;
            provider.FactoryParameters.Add(selectionCriteria);
            provider.FactoryMethod = "GetMagPaperList";
            provider.Refresh();
        }

        private void CslaDataProvider_DataChanged_1(object sender, EventArgs e)
        {
            CslaDataProvider provider = ((CslaDataProvider)this.Resources["MagPaperListData"]);
            if (provider.Error != null)
            {
                RadWindow.Alert(provider.Error.Message);
            }
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            if (rb != null)
            {
                MagPaper mp = rb.DataContext as MagPaper;
                if (mp != null)
                {
                    mp.BeginSave();
                }
            }
        }

        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            this.launchMagBrowser.Invoke((sender as HyperlinkButton).DataContext, e);
        }

        private void HyperlinkButton_Click_1(object sender, RoutedEventArgs e)
        {
            Int64 id;
            if (!Int64.TryParse(tbFindMagPaperById.Text, out id))
            {
                RadWindow.Alert("Please enter a valid number");
                return;
            }
            DataPortal<MagPaper> dp = new DataPortal<MagPaper>();
            dp.FetchCompleted += (o, e2) =>
            {
                if (e2 != null && e2.Object.PaperId != 0)
                {
                    GridLookupMagPaper.DataContext = e2.Object;
                    GridLookupMagPaper.Visibility = Visibility.Visible;
                    lbHideMagPaperPreview.Visibility = Visibility.Visible;
                }
                else
                {
                    RadWindow.Alert("This Paper ID cannot be found in the database");
                    GridLookupMagPaper.Visibility = Visibility.Collapsed;
                    lbHideMagPaperPreview.Visibility = Visibility.Collapsed;
                }
            };
            dp.BeginFetch(new SingleCriteria<MagPaper, Int64>(id));
        }

        private void HyperlinkButton_Click_2(object sender, RoutedEventArgs e)
        {
            GridLookupMagPaper.Visibility = Visibility.Collapsed;
            lbHideMagPaperPreview.Visibility = Visibility.Collapsed;
        }

        private void CheckBox_Click_1(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            if (rb != null)
            {
                MagPaper mp = rb.DataContext as MagPaper;
                if (mp != null)
                {
                    Item i = this.DataContext as Item;
                    if (i != null)
                    {
                        DataPortal<MagMatchItemToPaperManualCommand> dp = new DataPortal<MagMatchItemToPaperManualCommand>();
                        MagMatchItemToPaperManualCommand mm = new MagMatchItemToPaperManualCommand(i.ItemId,
                            mp.PaperId, mp.ManualTrueMatch, mp.ManualFalseMatch);
                        dp.ExecuteCompleted += (o, e2) =>
                        {
                            if (e2.Error != null)
                            {
                                RadWindow.Alert(e2.Error.ToErrorInfo());
                            }
                            else
                            {
                                GridLookupMagPaper.Visibility = Visibility.Collapsed;
                                tbFindMagPaperById.Text = "";
                                lbHideMagPaperPreview.Visibility = Visibility.Collapsed;
                                GetMagPaperListData(i);
                            }
                        };
                        dp.BeginExecute(mm);
                    }
                }
            }
        }

        private void hlManualMagLookup_Click(object sender, RoutedEventArgs e)
        {
            Item i = this.DataContext as Item;
            DataPortal<MagMatchItemsToPapersCommand> dp = new DataPortal<MagMatchItemsToPapersCommand>();
            MagMatchItemsToPapersCommand GetMatches = new MagMatchItemsToPapersCommand("FindMatches",
                false, i.ItemId, 0);
            dp.ExecuteCompleted += (o, e2) =>
            {
                if (e2.Error != null)
                {
                    RadWindow.Alert(e2.Error.Message);
                }
                else
                {
                    MagMatchItemsToPapersCommand res = e2.Object as MagMatchItemsToPapersCommand;
                    CslaDataProvider provider = this.Resources["MagPaperListData"] as CslaDataProvider;
                    provider.FactoryParameters.Clear();
                    MagPaperListSelectionCriteria selectionCriteria = new MagPaperListSelectionCriteria();
                    selectionCriteria.ListType = "ItemMatchedPapersList";
                    selectionCriteria.ITEM_ID = i.ItemId;
                    provider.FactoryParameters.Add(selectionCriteria);
                    provider.FactoryMethod = "GetMagPaperList";
                    provider.Refresh();
                    RadWindow.Alert(res.currentStatus);
                }
            };
            dp.BeginExecute(GetMatches);
        }

        private void MicrosoftAcademic_Activated(object sender, EventArgs e)
        {
            GetMagPaperListData(DataContext as Item);
        }

        private void It_Saved(object sender, Csla.Core.SavedEventArgs e)
        {
            if (e.Error != null)
                Telerik.Windows.Controls.RadWindow.Alert(e.Error.Message);
            ItemTimepointsDataChanged(sender, e);
        }



    } // END DIALOG CONTROL
    
    
    //public class EppiDocumentViewer : DocumentViewer
    //{
        
    //    public EppiDocumentViewer()
    //    { }
    //    public event RoutedEventHandler ReCheck;
    //    protected override void OnContentMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
    //    {
    //        base.OnContentMouseLeftButtonUp(sender, e);
    //        if (ToolMode == DocumentViewer.ToolModes.AnnotationCreateSticky)
    //        {
    //            ToolMode = DocumentViewer.ToolModes.PanAndAnnotationEdit;
    //        }
            
    //    }
    //    protected override void OnContentMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    //    {
    //        base.OnContentMouseLeftButtonDown(sender, e);
    //        //e.Handled = false;
    //        this.ReCheck.Invoke(sender, e);
    //    }
    //}
}