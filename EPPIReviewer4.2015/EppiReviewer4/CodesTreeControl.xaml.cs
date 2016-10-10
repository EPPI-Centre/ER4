using System;
using System.Collections.Generic;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Markup;
using BusinessLibrary.BusinessClasses;
using Telerik.Windows.Controls;
using Telerik.Windows;
//using DevExpress.Xpf.RichEdit.Extensions;
//using DevExpress.XtraRichEdit;
//using DevExpress.XtraRichEdit.Commands;
//using DevExpress.XtraRichEdit.API.Native;
using Csla;
using System.Windows.Data;
using Csla.Xaml;
using Csla.Rules;
using Telerik.Windows.Documents.TextSearch;
using Telerik.Windows.Documents;
using Telerik.Windows.Documents.Selection;
using System.Text.RegularExpressions;
using Telerik.Windows.DragDrop;
using Telerik.Windows.Controls.TreeView;


namespace EppiReviewer4
{
    public partial class CodesTreeControl : UserControl
    {
        private ItemDocument _CurrentTextDocument;
        private NewLinesHelper newLinesHelper;
        //public RichEdit rich;
        private RadRichTextBox _rich;
        private DocumentTextMap documentMap;
        public bool ShowDiagramMenuOptions = false;
        public string ShowReportMenuOptions = "";
        public bool ShowSearchMenuOptions = false;
        public bool ShowCodeTxtOptions = false;
        private string _controlContext;
        private RadTreeView TreeView;
        private ReviewSet tempRS;
        private Int64 copiedAttributeSet;
        private System.Windows.Threading.DispatcherTimer myDispatcherTimer;
        private RadWAdditionalText windowAdditionalText = new RadWAdditionalText();
        private RadWCodeProperties windowCodeProperties = new RadWCodeProperties();
        private RadWItemSetProperties windowItemSetProperties = new RadWItemSetProperties();
        private RadWNewCodeSet windowNewCodeSet = new RadWNewCodeSet();
        private RadWEditCodeSet windowEditCodeSet = new RadWEditCodeSet();
        private RadWChangeMethodToSingle windowChangeMethodToSingle = new RadWChangeMethodToSingle();
        private RadWCheckAttributeDelete windowCheckAttributeDelete = new RadWCheckAttributeDelete();
        private RadWCopyingCodes windowCopyingCodes = new RadWCopyingCodes();
        private RadWReviewWizard windowWizard;
        private RadWPrintCodesetOptions WindowPrintCodesetOptions;
        private RadWindow windowEditOutcomes = new RadWindow();
        private Grid GridEditOutcomes = new Grid();
        private dialogEditOutcomes dialogEditOutcomesControl = new dialogEditOutcomes();

        private RadWindow windowReports = new RadWindow();
        private dialogReportViewer reportViewerControl = new dialogReportViewer();
        private int ToDoSetID = -1;
        private AttributeSet ToDoAtt = null;
        AttributeSetToPasteList toPlist;

        private RadWClassifier windowClassifier = new RadWClassifier();

        //first bunch of lines to make the read-only UI work
        //public BusinessLibrary.Security.ReviewerIdentity ri;
        public bool HasWriteRights
        {
            get 
            { 
                if (ri == null) return false;
                 else return ri.HasWriteRights(); 
            }
        }
        //end of read-only ui hack
        public string ControlContext
        {
            get { return _controlContext; }
            set { _controlContext = value; }
        }

        //private bool LoadingAttributes = false;
        private BusinessLibrary.Security.ReviewerIdentity ri;

        public CodesTreeControl()
        {
            InitializeComponent();
            this.Loaded += new System.Windows.RoutedEventHandler(ThisControl_Loaded);
            //add <Button x:Name="isEn" IsEnabled="{Binding HasWriteRights, Mode=OneWay}" ></Button>
            //to the xaml resources section!!
            ri = Csla.ApplicationContext.User.Identity as BusinessLibrary.Security.ReviewerIdentity;
            isEn.DataContext = this;
            //end of read-only ui hack
            //cmdShowClassificationWindow.Visibility = ri.IsSiteAdmin ? Visibility.Visible : System.Windows.Visibility.Collapsed;
            //enter RadWindow properties...
            Thickness thk = new Thickness(20);
            windowEditOutcomes.Header = "Edit outcomes";
            windowEditOutcomes.ResizeMode = ResizeMode.CanResize;
            windowEditOutcomes.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
            windowEditOutcomes.Width= 500;
            //windowEditOutcomes.RestrictedAreaMargin = thk;
            windowEditOutcomes.IsRestricted = true;
            //windowEditOutcomes.Opened="windowEditOutcomes_Opened"
            GridEditOutcomes.Children.Add(dialogEditOutcomesControl);
            windowEditOutcomes.Content = GridEditOutcomes;

            windowReports.Header = "Report viewer";
            windowReports.WindowStateChanged += new EventHandler(Helpers.WindowHelper.MaxOnly_WindowStateChanged);
            windowReports.Style = Application.Current.Resources["CustomRadWindowStyle"] as Style;
            windowReports.WindowState = WindowState.Maximized;
            windowReports.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
            //windowReports.RestrictedAreaMargin = thk;
            windowReports.IsRestricted = true;
            windowReports.CanClose = true;
            windowReports.Width = 500;
            Grid g1 = new Grid();
            g1.Children.Add(reportViewerControl);
            windowReports.Content = g1;



            windowClassifier.WindowStateChanged += new EventHandler(Helpers.WindowHelper.MaxOnly_WindowStateChanged);
            windowClassifier.Style = Application.Current.Resources["CustomRadWindowStyle"] as Style;
            //windowClassifier.WindowState = WindowState.Maximized;
            //windowClassifier.ResizeMode = ResizeMode.NoResize;
            windowClassifier.RestrictedAreaMargin = thk;
            windowClassifier.IsRestricted = true;
            //end of dialogCoding

            //end RadWindow properties

            //hook up RadW events 
            windowAdditionalText.cancelAdditionaltext_Clicked +=new EventHandler<RoutedEventArgs>(cancelAdditionaltext_Click);
            windowAdditionalText.saveAdditionaltext_Clicked +=new EventHandler<RoutedEventArgs>(saveAdditionaltext_Click);
            windowAdditionalText.Closed += new EventHandler<WindowClosedEventArgs>(windowAdditionalText_Closed);
            windowCodeProperties.cmdCancelCodeSettings_Clicked += new EventHandler<RoutedEventArgs>(cmdCancelCodeSettings_Click);
            windowCodeProperties.cmdSaveCodeSettings_Clicked +=new EventHandler<RoutedEventArgs>(cmdSaveCodeSettings_Click);
            windowItemSetProperties.cmdCancelCodeSetSettings_Clicked +=new EventHandler<RoutedEventArgs>(cmdCancelCodeSetSettings_Click);
            windowItemSetProperties.cmdSaveCodeSetSettings_Clicked +=new EventHandler<RoutedEventArgs>(cmdSaveCodeSetSettings_Click);
            windowNewCodeSet.cmdCancelNewCodeset_Clicked +=new EventHandler<RoutedEventArgs>(cmdCancelNewCodeset_Click);
            windowNewCodeSet.cmdCreateNewCodeSet_Clicked +=new EventHandler<RoutedEventArgs>(cmdCreateNewCodeSet_Click);
            windowEditCodeSet.HyperLinkChangeMethodToMultiple_Clicked +=new EventHandler<RoutedEventArgs>(HyperLinkChangeMethodToMultiple_Click);
            windowEditCodeSet.HyperLinkChangeMethodToSingle_Clicked +=new EventHandler<RoutedEventArgs>(HyperLinkChangeMethodToSingle_Click);
            windowEditCodeSet.cmdCancelEditCodeset_Clicked +=new EventHandler<RoutedEventArgs>(cmdCancelEditCodeset_Click);
            windowEditCodeSet.cmdSaveEditCodeSet_Clicked +=new EventHandler<RoutedEventArgs>(cmdSaveEditCodeSet_Click);
            windowChangeMethodToSingle.HyperLinkCancelChangeMethodToSingle_Clicked +=new EventHandler<RoutedEventArgs>(HyperLinkCancelChangeMethodToSingle_Click);
            windowChangeMethodToSingle.HyperLinkDoChangeMethodToSingle_Clicked +=new EventHandler<RoutedEventArgs>(HyperLinkDoChangeMethodToSingle_Click);
            windowCheckAttributeDelete.cmdCancelDeleteCode_Clicked +=new EventHandler<RoutedEventArgs>(cmdCancelDeleteCode_Click);
            windowCheckAttributeDelete.cmdDeleteCode_Clicked +=new EventHandler<RoutedEventArgs>(cmdDeleteCode_Click);

            //end of RadW hookers
        }

        

        
        public RadRichTextBox rich
        {
            get { return _rich; }
            set
            {
                _rich = value;
            }
        }
        public ItemDocument CurrentTextDocument
        {
            get { return _CurrentTextDocument; }
            set
            {
                _CurrentTextDocument = value;
                if (value != null && value.Text != null) newLinesHelper = new NewLinesHelper(value.Text);
                else newLinesHelper = new NewLinesHelper("");
            }
        }
        private void ThisControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (TreeView == null)
            {
                MakeTree();
                MakeMenu();
            }
        }

        private void MakeTree()
        {
            TreeView = new RadTreeView();
            TreeView.IsExpandOnSingleClickEnabled = true;
            TreeView.IsLineEnabled = true;
            TreeView.IsEditable = true;
            TreeView.Edited += new RadTreeViewItemEditedEventHandler(TreeView_Edited);
            if (ControlContext != "CodingOnly")
            {
                TreeView.IsDragDropEnabled = true;
            }
            else
            {
                cmdNewCodeSet.Visibility = System.Windows.Visibility.Collapsed;
                cmdCodeProperties.Visibility = System.Windows.Visibility.Collapsed;
            }
            TreeView.PreviewDragEnded += new RadTreeViewDragEndedEventHandler(TreeView_PreviewDragEnded);
            //TreeView.Drop += new DragEventHandler(TreeView_PreviewDragEnded);
            TreeViewSettings.SetDragDropExecutionMode(TreeView, TreeViewDragDropExecutionMode.Legacy);
            //DragDropManager.AddDragDropCompletedHandler(TreeView, TreeView_PreviewDragEnded, true);

            TreeView.SelectionChanged += new Telerik.Windows.Controls.SelectionChangedEventHandler(TreeView_SelectionChanged);
            TreeView.PreviewEditStarted += new RadTreeViewItemEditedEventHandler(TreeView_PreviewEditStarted);
            TreeView.SelectionMode = Telerik.Windows.Controls.SelectionMode.Single;
            if (ControlContext == "dialogCoding" || ControlContext == "CodingOnly")
            {
                TreeView.ItemTemplateSelector = this.Resources["myAttributeCodingTemplateSelector"] as AttributeCodingTemplateSelector;
                cmdConfigureCodesets.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                TreeView.ItemTemplateSelector = this.Resources["myReadOnlyTemplateSelector"] as ReadOnlyTemplateSelector;
            }
            CodingGrid.Children.Add(TreeView);
            Grid.SetRow(TreeView, 2);
            TreeView.ItemsSource = (App.Current.Resources["CodeSetsData"] as CslaDataProvider).Data as ReviewSetsList;

            CslaDataProvider provider = (App.Current.Resources["CodeSetsData"] as CslaDataProvider);
            if (provider != null)
            {
                provider.DataChanged -= CodeSetsProvider_DataChanged;
                provider.DataChanged += new EventHandler(CodeSetsProvider_DataChanged);
            }
            Helpers.ReverseBooleanConverter rbc = new Helpers.ReverseBooleanConverter();
            Binding binding = new Binding();
            binding.Converter = rbc;
            binding.ConverterParameter = "IsBusy";
            binding.Source = App.Current.Resources["CodeSetsData"] as CslaDataProvider;
            binding.Path = new PropertyPath("IsBusy");
            TreeView.SetBinding(RadTreeView.IsEnabledProperty, binding);
            if (ControlContext == "dialogCoding" || ControlContext == "CodingOnly")
            {
                TreeView.IsEnabled = false;
            }
        }

        private void CodeSetsProvider_DataChanged(object sender, EventArgs e)
        {
            CslaDataProvider provider = (App.Current.Resources["CodeSetsData"] as CslaDataProvider);
            if (provider.Error != null)
                MessageBox.Show(provider.Error.Message);
            else
                TreeView.ItemsSource = (App.Current.Resources["CodeSetsData"] as CslaDataProvider).Data as ReviewSetsList;
        }

        void TreeView_PreviewEditStarted(object sender, RadTreeViewItemEditedEventArgs e)
        {
            ReviewSet rs = TreeView.SelectedItem as ReviewSet;
            if (rs == null)
            {
                AttributeSet attributeSet = TreeView.SelectedItem as AttributeSet;
                if (attributeSet != null)
                {
                    ReviewSetsList rsl = TreeView.ItemsSource as ReviewSetsList;
                    rs = rsl.GetReviewSet(attributeSet.SetId);
                }
            }
            if (rs != null)
            {
                if (rs.AllowCodingEdits == false)
                {
                    e.Handled = true;
                }
                else
                {
                    e.Handled = false;
                }
            }
        }

        private RadMenuItem contextMenuCodeSelected;
        private RadMenuItem contextMenuUncodeSelected;
        private RadMenuItem contextMenuShowText;
        private RadMenuItem contextMenuReport;
        private RadMenuItem contextMenuAddChild;
        private RadMenuItem contextMenuNewCodeSet;
        private RadMenuItem contextMenuDeleteCode;
        private RadMenuItem contextMenuDeleteCodeSet;
        private RadMenuItem contextMenuProperties;
        private RadMenuItem contextMenuListItems;
        private RadMenuItem contextMenuListItemsExcluded;
        private RadMenuItem contextMenuListItemsWithout;
        private RadMenuItem contextMenuListItemsWithoutExcluded;
        private RadMenuItem contextMenuDisplayFrequencies;
        private RadMenuItem contextMenuAssignItems;
        private RadMenuItem contextMenuRemoveItems;
        private RadMenuItem contextMenuAssignSearchItems;
        private RadMenuItem contextMenuRemoveSearchItems;
        private RadMenuItem contextMenuInsertInDiagram;
        private RadMenuItem contextMenuInsertChildCodesInDiagram;
        //private RadMenuItem contextMenuInsertInReport;
        //private RadMenuItem contextMenuAxialCoding;
        private RadMenuItem contextMenuReviewSetPrint;
        private RadMenuItem contextMenuReviewSetCopy;
        private RadMenuItem contextMenuReviewSetPaste;
        
        private void MakeMenu()
        {
            if (ControlContext == "CodingOnly") return;
            RadContextMenu cm = new RadContextMenu();
            cm.Opened += new RoutedEventHandler(contextMenu_Opened);
            //cm.ItemClick += new RadRoutedEventHandler(contextMenu_ItemClick);
            
            contextMenuCodeSelected = new RadMenuItem(); contextMenuCodeSelected.Header = "Code selected text";
            contextMenuCodeSelected.IsEnabled = true;
            cm.Items.Add(contextMenuCodeSelected); 
            contextMenuUncodeSelected = new RadMenuItem(); contextMenuUncodeSelected.Header = "Uncode selected text";
            contextMenuUncodeSelected.IsEnabled = true;
            cm.Items.Add(contextMenuUncodeSelected);
            contextMenuAddChild = new RadMenuItem(); contextMenuAddChild.Header = "Add child code"; cm.Items.Add(contextMenuAddChild);
            if (ControlContext == "homeDocuments")
            {
                contextMenuListItems = new RadMenuItem(); contextMenuListItems.Header = "List items with this code"; 
                cm.Items.Add(contextMenuListItems);
                contextMenuListItemsExcluded = new RadMenuItem(); contextMenuListItemsExcluded.Header = "List items with this code (excluded)";
                cm.Items.Add(contextMenuListItemsExcluded);
                contextMenuListItemsWithout = new RadMenuItem(); contextMenuListItemsWithout.Header = "List items without this code";
                cm.Items.Add(contextMenuListItemsWithout);
                contextMenuListItemsWithoutExcluded = new RadMenuItem(); contextMenuListItemsWithoutExcluded.Header = "List items without this code (excluded)";
                cm.Items.Add(contextMenuListItemsWithoutExcluded);
                contextMenuDisplayFrequencies = new RadMenuItem(); contextMenuDisplayFrequencies.Header = "Display included item frequencies (children)"; 
                cm.Items.Add(contextMenuDisplayFrequencies);

                contextMenuAssignItems = new RadMenuItem(); contextMenuAssignItems.Header = "Assign selected items to this code";
                contextMenuAssignItems.IsEnabled = HasWriteRights;
                cm.Items.Add(contextMenuAssignItems);

                contextMenuRemoveItems = new RadMenuItem(); contextMenuRemoveItems.Header = "Remove selected items from this code";
                contextMenuRemoveItems.IsEnabled = HasWriteRights;
                cm.Items.Add(contextMenuRemoveItems);

                contextMenuAssignSearchItems = new RadMenuItem(); contextMenuAssignSearchItems.Header = "Assign items in selected searches to this code";
                contextMenuAssignSearchItems.IsEnabled = HasWriteRights;
                cm.Items.Add(contextMenuAssignSearchItems);

                contextMenuRemoveSearchItems = new RadMenuItem(); contextMenuRemoveSearchItems.Header = "Remove items in selected searches from this code";
                contextMenuRemoveSearchItems.IsEnabled = HasWriteRights;
                cm.Items.Add(contextMenuRemoveSearchItems);

                contextMenuInsertInDiagram = new RadMenuItem(); contextMenuInsertInDiagram.Header = "Insert in diagram";
                contextMenuInsertInDiagram.IsEnabled = HasWriteRights;
                cm.Items.Add(contextMenuInsertInDiagram);

                contextMenuInsertChildCodesInDiagram = new RadMenuItem(); contextMenuInsertChildCodesInDiagram.Header = "Insert child codes in diagram";
                contextMenuInsertChildCodesInDiagram.IsEnabled = HasWriteRights;
                cm.Items.Add(contextMenuInsertChildCodesInDiagram);
                
                //contextMenuInsertInReport = new RadMenuItem(); contextMenuInsertInReport.Header = "Insert in report";
                //contextMenuInsertInReport.IsEnabled = HasWriteRights;
                //cm.Items.Add(contextMenuInsertInReport);

                //contextMenuAxialCoding = new RadMenuItem(); contextMenuAxialCoding.Header = "Code within code (axial coding)";
                //contextMenuAxialCoding.IsEnabled = HasWriteRights;
                //contextMenuAxialCoding.Visibility = System.Windows.Visibility.Collapsed;
                //cm.Items.Add(contextMenuAxialCoding);

                CheckBoxAutoAdvance.Visibility = Visibility.Collapsed;
                CheckBoxHotkeys.Visibility = Visibility.Collapsed;
            }

            contextMenuShowText = new RadMenuItem(); contextMenuShowText.Header = "Show text coded with this code";
            contextMenuShowText.IsEnabled = true;
            cm.Items.Add(contextMenuShowText);
            contextMenuReport = new RadMenuItem(); contextMenuReport.Header = "Report: all text coded with this code (all PDFs)"; cm.Items.Add(contextMenuReport);

            
            contextMenuNewCodeSet = new RadMenuItem(); contextMenuNewCodeSet.Header = "Add new code set";
            contextMenuNewCodeSet.IsEnabled = HasWriteRights;
            cm.Items.Add(contextMenuNewCodeSet);

            contextMenuDeleteCode = new RadMenuItem(); contextMenuDeleteCode.Header = "Delete code";
            contextMenuDeleteCode.IsEnabled = HasWriteRights;
            cm.Items.Add(contextMenuDeleteCode);

            contextMenuDeleteCodeSet = new RadMenuItem(); contextMenuDeleteCodeSet.Header = "Delete code set";
            contextMenuDeleteCodeSet.IsEnabled = HasWriteRights;
            cm.Items.Add(contextMenuDeleteCodeSet);

            RadMenuItem mi = new RadMenuItem(); mi.IsSeparator = true; cm.Items.Add(mi);
            contextMenuReviewSetPrint = new RadMenuItem(); contextMenuReviewSetPrint.Header = "Print..."; cm.Items.Add(contextMenuReviewSetPrint);
            contextMenuProperties = new RadMenuItem(); contextMenuProperties.Header = "Properties..."; cm.Items.Add(contextMenuProperties);

            RadMenuItem mi2 = new RadMenuItem(); mi2.IsSeparator = true; cm.Items.Add(mi2);

            contextMenuReviewSetCopy = new RadMenuItem(); contextMenuReviewSetCopy.Header = "Copy";
            contextMenuReviewSetCopy.IsEnabled = HasWriteRights;
            cm.Items.Add(contextMenuReviewSetCopy);

            contextMenuReviewSetPaste = new RadMenuItem(); contextMenuReviewSetPaste.Header = "Paste...";
            contextMenuReviewSetPaste.IsEnabled = false;
            cm.Items.Add(contextMenuReviewSetPaste);

            //Helpers.ReverseBooleanConverter rbc = new Helpers.ReverseBooleanConverter();
            //Binding binding = new Binding();
            //binding.Converter = rbc;
            //binding.ConverterParameter = "IsBusy";
            //binding.Source = App.Current.Resources["CodeSetsData"] as CslaDataProvider;
            //binding.Path = new PropertyPath("IsBusy");
            //TreeView.SetBinding(RadTreeView.IsEnabledProperty, binding);

            RadContextMenu.SetContextMenu(TreeView, cm);
            TreeView.AddHandler(RadMenuItem.ClickEvent, new RoutedEventHandler(OnMenuItemClicked));
            this.MouseRightButtonDown += OnRightMouseButtonUp1;
            //Mouse.AddMouseDownHandler(this, OnRightMouseButtonUp);
        }

        public void BindNew()
        {
            if (TreeView == null)
            {
                MakeTree();
                MakeMenu();
            }
            ReviewSetsList reviewSets = TreeView.ItemsSource as ReviewSetsList;
            if (reviewSets != null)
            {
                reviewSets.LoadingAttributes = true;
                reviewSets.ClearItemData();
                reviewSets.LoadingAttributes = false;
            }
            TreeView.IsEnabled = true;
        }

        public void BindItem(Item Item)
        {
            if (TreeView == null)
            {
                MakeTree();
                MakeMenu();
            }
            BindCodes();
            LoadItemAttributes((DataContext as Item).ItemId);
            CurrentTextDocument = null;
        }

        public void BindItemIdAndDocument(Int64 ItemId, ItemDocument itemDocument)
        {
            ReviewSetsList reviewSets = TreeView.ItemsSource as ReviewSetsList;
            reviewSets.LoadingAttributes = true;
            if (TreeView == null)
            {
                MakeTree();
                MakeMenu();
            }
            BindCodes();
            LoadItemAttributes(ItemId);
            CurrentTextDocument = itemDocument;
            reviewSets.LoadingAttributes = false;
        }

        public void BindCodes()
        {
            if (TreeView == null)
            {
                MakeTree();
                MakeMenu();
            }
        }

        public AttributeSet SelectedAttributeSet()
        {
            if (TreeView.SelectedItem != null)
            {
                return TreeView.SelectedItem as AttributeSet;
            }
            else
            {
                return null;
            }
        }

        public System.Collections.ObjectModel.ObservableCollection<object> SelectedAttributeSets()
        {
            if (TreeView.SelectedItems.Count > 0)
            {
                return TreeView.SelectedItems;
            }
            else
            {
                return null;
            }
        }

        public ReviewSet SelectedReviewSet()
        {
            if (TreeView.SelectedItem != null)
            {
                return TreeView.SelectedItem as ReviewSet;
            }
            else
            {
                return null;
            }
        }
        public ReviewSet ActiveReviewSet 
            //returns the ReviewSet that contains the currently selected item (or the first selected one if more than one are selected - not sure this is possible)
        {
            get
            {
                if (TreeView.SelectedItem != null)
                {
                    ReviewSet result = TreeView.SelectedItem as ReviewSet;
                    if (result == null)//the selected node is an AttributeSet
                    {
                        AttributeSet attSet = TreeView.SelectedItem as AttributeSet;
                        if (attSet == null) return null;
                        int SetId = attSet.SetId;
                        return this.GetReviewSet(attSet.SetId);
                    }
                    else return result;
                }
                else
                {
                    return null;
                }
            }
        }
        public AttributeSet GetAttributeSet(Int64 AttributeId)
        {
            AttributeSet returnValue = null;
            foreach (ReviewSet rs in TreeView.ItemsSource as ReviewSetsList)
            {
                returnValue = LocateInTree(rs.Attributes, AttributeId);
                if (returnValue != null)
                    break;
            }
            return returnValue;
        }

        public ReviewSet GetReviewSet(int SetId)
        {
            ReviewSetsList rsl = TreeView.ItemsSource as ReviewSetsList;
            return rsl.GetReviewSet(SetId);
        }

        private AttributeSet LocateInTree(AttributeSetList list, Int64 AttributeId)
        {
            AttributeSet returnValue = null;
            foreach (AttributeSet attribute in list)
            {
                if (attribute.AttributeId == AttributeId)
                {
                    return attribute;
                }
                else
                {
                    if (attribute.Attributes.Count > 0)
                    {
                        returnValue = LocateInTree(attribute.Attributes, AttributeId);
                        if (returnValue != null)
                            break;
                    }
                }

            }
            return returnValue;
        }

        public void DeleteReviewSet(ReviewSet reviewSet)
        {
            ReviewSetsList setsList = TreeView.ItemsSource as ReviewSetsList;
            setsList.Remove(reviewSet);
        }

        public void CreateReviewSet()
        {
            DoNewCodeSet();
        }

        public void DoCodeProperties()
        {
            DoNodeProperties();
        }

        public bool isItemDocumentTextCoded(Int64 ItemDocumentTextID)
        {
            bool returnValue = false;
            foreach (ReviewSet rs in TreeView.ItemsSource as ReviewSetsList)
            {
                returnValue = isItemDocumentTextCodedRec(rs.Attributes, ItemDocumentTextID);   
            }
            return returnValue;
        }

        private bool isItemDocumentTextCodedRec(AttributeSetList list, Int64 ItemDocumentTextID)
        {
            bool returnValue = false;
            foreach (AttributeSet attribute in list)
            {
                if ((attribute.ItemData != null) && (attribute.ItemData.ItemAttributeTextList != null))
                {
                    foreach (ItemAttributeText iat in attribute.ItemData.ItemAttributeTextList)
                    {
                        if (iat.ItemDocumentId == ItemDocumentTextID)
                            return true;
                    }
                }
                if (attribute.Attributes.Count > 0)
                {
                    returnValue = isItemDocumentTextCodedRec(attribute.Attributes, ItemDocumentTextID);
                    if (returnValue)
                        return returnValue;
                }
            }
            return returnValue;
        }

        public event EventHandler SelectedItemChanged;
        public event EventHandler ListItemsWithCode;
        public event EventHandler ListItemsWithCodeExcluded;
        public event EventHandler ListItemsWithoutCode;
        public event EventHandler ListItemsWithoutCodeExcluded;
        public event EventHandler DisplayItemFrequencies;
        public event EventHandler AssignSelected;
        public event EventHandler RemoveSelected;
        public event EventHandler AssignSearchSelected;
        public event EventHandler RemoveSearchSelected;
        public event EventHandler InsertInDiagram;
        public event EventHandler InsertChildCodesInDiagram;
        public event EventHandler InsertInReport;
        public event EventHandler CodeWithinCode;
        public event EventHandler RequestItemAdvance;


        // END PUBLIC PROPERTIES / EVENTS ******************************************************************************

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
        private void OnRightMouseButtonUp1(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            e.Handled = true;
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

        private void OnMenuItemClicked(object sender, RoutedEventArgs args)
        {
            RadRoutedEventArgs e = args as RadRoutedEventArgs;
            RadMenuItem item = e.OriginalSource as RadMenuItem;
            if ((item != null))
            {
                if (item.Header != null)
                {
                    if (item.Header.ToString() == "Show text coded with this code")
                    {
                        HightlightSelectedCode();
                    }

                    if (item.Header.ToString() == "Report: all text coded with this code (all PDFs)")
                    {
                        AttributeSet attributeSet = TreeView.SelectedItem as AttributeSet;
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
                                    string report = "<html><body><h2 style='color: #4544AF'>Text coded with: <i>" + attributeSet.AttributeName + "</i></h2>";
                                    Int64 CurrentItemDocumentID = 0;
                                    Int64 CurrentItemId = -1;
                                    foreach (ReadOnlyAttributeTextAllItems textItem in list)
                                    {
                                        if (textItem.ItemId != CurrentItemId)
                                        {
                                            if (CurrentItemId != -1) report += "<br>";
                                            report += "<br><h3><i>" + textItem.ItemShortTitle + "</i> – " + textItem.ItemTitle + " (ID: " + textItem.ItemId.ToString() + ")</h3>";
                                            CurrentItemId = textItem.ItemId;
                                            if (textItem.AdditionalText != "")
                                            {
                                                report += "[Info]&nbsp;" + textItem.AdditionalText.Replace("\n", "<br>") + "<br>";
                                            }
                                        }
                                        if (textItem.ItemDocumentId != CurrentItemDocumentID)
                                        {
                                            report += "<br><b>[" + textItem.ItemDocumentTitle + "]</b><br>";
                                            CurrentItemDocumentID = textItem.ItemDocumentId;
                                            
                                        }
                                        if (textItem.Snippet != "")
                                        {
                                            if (textItem.Origin == "Text")
                                            {
                                                report += "<span style='font-weight:bold; color: #4544AF'>Characters " + textItem.TextFrom.ToString() +
                                                            " to " + textItem.TextTo.ToString() + ":</span> ";
                                                //<span style='font-family:Courier New; font-size:12px;'>" 
                                                //        + rct.CodedText + "</span><br>";
                                                report += "<span style='font-family:Courier New; font-size:12px;'>" + textItem.CodedText.Replace("\n", " ").Replace(@"\n", " ") + "</span><br>";
                                            }
                                            else if (textItem.Origin == "Pdf")
                                            {
                                                int s = textItem.CodedText.IndexOf('\n');
                                                s = textItem.CodedText.IndexOf("\"");
                                                s = textItem.CodedText.IndexOf("\"\n");
                                                report += "<span style ='font-weight:bold; color: #4544AF'>" + textItem.CodedText.Replace("[¬s]\"", "</span>&nbsp;<i>").Replace("[¬e]\"", "</i>").Replace("\"\n\"", "<br>[...]") + "<br>";
                                            }
                                        }
                                    }
                                    report += "</body></html>";
                                    //System.Windows.Browser.HtmlPage.Window.Invoke("ShowPopup", report);
                                    reportViewerControl.SetContent(report);
                                    windowReports.ShowDialog();
                                }
                            };
                            BusyLoading.IsRunning = true;
                            dp.BeginFetch(new SingleCriteria<ReadOnlyAttributeTextAllItemsList, Int64>(attributeSet.AttributeSetId));
                        }
                    }
                    ReviewInfo rInfo = ((App)(Application.Current)).GetReviewInfo();

                    if (item.Header.ToString() == "Code selected text")
                    {
                        AttributeSet attributeSet = TreeView.SelectedItem as AttributeSet;
                        if (attributeSet != null)
                        {
                            if (CurrentTextDocument != null)
                            {
                                //HightlightSelectedCode();
                                //HighlightSelectedText();
                                if (attributeSet.ItemData == null)
                                {
                                    ItemAttributeData itemData = CreateItemData();
                                    DataPortal<ItemAttributeSaveCommand> dp = new DataPortal<ItemAttributeSaveCommand>();
                                    ItemAttributeSaveCommand command = new ItemAttributeSaveCommand("Insert",
                                        itemData.AttributeId,
                                        itemData.ItemSetId,
                                        "",
                                        itemData.AttributeId,
                                        itemData.SetId,
                                        itemData.ItemId,
                                        rInfo);
                                    dp.ExecuteCompleted += (o, e2) =>
                                    {
                                        BusyLoading.IsRunning = false;
                                        if (e2.Error != null)
                                        {
                                            MessageBox.Show(e2.Error.Message);
                                        }
                                        else
                                        {
                                            ReviewSetsList reviewSets = TreeView.ItemsSource as ReviewSetsList;
                                            itemData.ItemAttributeId = e2.Object.ItemAttributeId;
                                            itemData.ItemSetId = e2.Object.ItemSetId;
                                            reviewSets.LoadingAttributes = true; // setting this so that the autosave insert doesn't kick in
                                            attributeSet.ItemData = itemData;
                                            attributeSet.IsSelected = true;
                                            reviewSets.LoadingAttributes = false;
                                            CodeOrUnCodeSelectedText(attributeSet, "Code");
                                        }
                                    };
                                    BusyLoading.IsRunning = true;
                                    dp.BeginExecute(command);
                                }
                                else
                                {
                                    CodeOrUnCodeSelectedText(attributeSet, "Code");
                                }
                            }
                        }
                    }

                    if (item.Header.ToString() == "Uncode selected text")
                    {
                        AttributeSet attributeSet = TreeView.SelectedItem as AttributeSet;
                        if (attributeSet != null)
                        {
                            if (CurrentTextDocument != null)
                            {
                                UnHighlightSelectedText();
                                if (attributeSet.ItemData == null)
                                {
                                    ItemAttributeData iad = null;
                                    iad = CreateItemData();
                                    // need lots more here
                                }
                                else
                                {
                                    CodeOrUnCodeSelectedText(attributeSet, "UnCode");
                                }
                            }
                        }
                    }

                    if (item.Header.ToString() == "List items with this code")
                    {
                        this.ListItemsWithCode.Invoke("Included", EventArgs.Empty);
                    }

                    if (item.Header.ToString() == "List items with this code (excluded)")
                    {
                        this.ListItemsWithCodeExcluded.Invoke("Excluded", EventArgs.Empty);
                    }

                    if (item.Header.ToString() == "List items without this code")
                    {
                        this.ListItemsWithoutCode.Invoke("Included", EventArgs.Empty);
                    }

                    if (item.Header.ToString() == "List items without this code (excluded)")
                    {
                        this.ListItemsWithoutCodeExcluded.Invoke("Excluded", EventArgs.Empty);
                    }

                    if (item.Header.ToString() == "Display included item frequencies (children)")
                    {
                        this.DisplayItemFrequencies.Invoke(this, EventArgs.Empty);
                    }

                    if (item.Header.ToString() == "Assign selected items to this code")
                    {
                        this.AssignSelected.Invoke(this, EventArgs.Empty);
                    }

                    if (item.Header.ToString() == "Remove selected items from this code")
                    {
                        this.RemoveSelected.Invoke(this, EventArgs.Empty);
                    }

                    if (item.Header.ToString() == "Assign items in selected searches to this code")
                    {
                        this.AssignSearchSelected.Invoke(this, EventArgs.Empty);
                    }

                    if (item.Header.ToString() == "Remove items in selected searches from this code")
                    {
                        this.RemoveSearchSelected.Invoke(this, EventArgs.Empty);
                    }

                    if (item.Header.ToString() == "Insert in diagram")
                    {
                        this.InsertInDiagram.Invoke(this, EventArgs.Empty);
                    }

                    if (item.Header.ToString() == "Insert child codes in diagram")
                    {
                        this.InsertChildCodesInDiagram.Invoke(this, EventArgs.Empty);
                    }

                    if (item.Header.ToString() == "Add child code")
                    {
                        DoNewAttribute();
                    }

                    if (item.Header.ToString() == "Insert in report")
                    {
                        this.InsertInReport.Invoke(this, EventArgs.Empty);
                    }

                    if (item.Header.ToString() == "Code within code (axial coding)")
                    {
                        this.CodeWithinCode.Invoke(this, EventArgs.Empty);
                    }

                    if (item.Header.ToString() == "Add new code set")
                    {
                        DoNewCodeSet();
                    }

                    if ((item.Header.ToString() == "Delete code") || (item.Header.ToString() == "Delete code set"))
                    {
                        CheckDeleteAttributeSet();
                    }

                    if (item.Header.ToString() == "Print...")
                    {
                        PrintSelectedReviewSet();
                    }

                    if (item.Header.ToString() == "Properties...")
                    {
                        DoNodeProperties();
                    }

                    if (item.Header.ToString() == "Copy")
                    {
                         AttributeSet attributeSet = TreeView.SelectedItem as AttributeSet;
                         if (attributeSet != null)
                         {
                             copiedAttributeSet = attributeSet.AttributeId;
                             contextMenuReviewSetPaste.IsEnabled = true;
                         }
                         else contextMenuReviewSetPaste.IsEnabled = false;
                    }

                    if (item.Header.ToString() == "Paste...")
                    {
                        AttributeSet attributeSet = TreeView.SelectedItem as AttributeSet;
                        AttributeSet source;
                        ReviewSet reviewSet = TreeView.SelectedItem as ReviewSet;
                        
                        if ((attributeSet != null || reviewSet != null) && copiedAttributeSet > 0)
                        {
                            //find where you should paste (last place!)
                            int ord = 0;
                            if (reviewSet != null)
                            {
                                ord = reviewSet.Attributes.Count;
                                
                            }
                            else
                            {
                                ord = attributeSet.Attributes.Count;
                                ReviewSetsList rsl = TreeView.ItemsSource as ReviewSetsList;
                                foreach (ReviewSet rs in rsl)
                                {
                                    if (rs.SetId == attributeSet.SetId)
                                    {
                                        reviewSet = rs;
                                        break;//ensures reviewSet is the currently active one
                                    }
                                }
                                //? add a check if the reviewset is still null??
                            }

                            //build the list of items
                            source = reviewSet.GetAttributeSetFromAttributeId(copiedAttributeSet);
                            if (source == null)
                            {
                                foreach (ReviewSet reviewSet2 in (App.Current.Resources["CodeSetsData"] as CslaDataProvider).Data as ReviewSetsList)
                                {
                                    source = reviewSet2.GetAttributeSetFromAttributeId(copiedAttributeSet);
                                    if (source != null) 
                                    {//check if source and dest Sets are of the same type
                                        if (reviewSet.SetTypeId != reviewSet2.SetTypeId)
                                        {
                                            RadWindow.Alert("Sorry, you can't paste between codesets of different types");
                                            return;
                                        }
                                        break;
                                    }
                                }
                            }
                            //check if the new pasted tree will be too deep
                            int destLevel = 0;
                            if (attributeSet != null)
                            {//not pasting to the root of the set
                                destLevel = attributeSet.AttributeTreeLevel;
                            }
                            if (reviewSet.SetType.MaxDepth != 0 && reviewSet.SetType.MaxDepth <= destLevel + source.ChildrenDepth + 1)
                                //level of the receiving + levels contained by the pasted code + 1 for the pasted code itself
                            {
                                RadWindow.Alert("Sorry the pasted tree would be too deep." + Environment.NewLine
                                           + "Max depth for this set is: " + reviewSet.SetType.MaxDepth.ToString());
                                return;
                            }
                            toPlist = new AttributeSetToPasteList();
                            ////get the starting node and all children (recursive)
                            AttributeSet newAset = new AttributeSet();
                            if (attributeSet == null)
                            {//pasting to the root of the set
                                buildToPasteFlatUnsortedList(source, reviewSet.SetId, 0);
                                newAset.SetId = reviewSet.SetId;
                                newAset.ParentAttributeId = 0;
                            }
                            else
                            {
                                buildToPasteFlatUnsortedList(source, attributeSet.SetId, 0);
                                newAset.SetId = attributeSet.SetId;
                                newAset.ParentAttributeId = attributeSet.AttributeId;
                            }
                            //sort the list: so that it starts with parents and ends with the furhterst children
                            toPlist.Sort();
                            //create new AttSet based on list.getNext();
                            newAset.AttributeOrder = ord;
                            newAset.AttributeTypeId = source.AttributeTypeId;
                            newAset.ContactId = ri.UserId;
                            
                            newAset.AttributeSetDescription = source.AttributeSetDescription;
                            newAset.AttributeDescription = source.AttributeDescription;
                            newAset.AttributeName = source.AttributeName;
                            //hook AttSet.Saved to AttSetSavedForCopying
                            newAset.Saved += new EventHandler<Csla.Core.SavedEventArgs>(AttSetSavedForCopying);
                            //open copying window;
                            windowCopyingCodes.ProgressTxt.Text = "1 of " + toPlist.Count;
                            windowCopyingCodes.BusyAnimation.IsRunning = true;
                            windowCopyingCodes.ShowDialog();
                            //begin save;
                            newAset.BeginSave();
                        }
                    }
                }
            }
        }
        private void buildToPasteFlatUnsortedList(AttributeSet aSet, int setID, int RelativeLevel)
        {//this is recursive!!
            AttributeSetToPaste astp = new AttributeSetToPaste(aSet, setID, RelativeLevel);
            toPlist.Add(astp);
            foreach (AttributeSet CaSet in aSet.Attributes)
            {
                buildToPasteFlatUnsortedList(CaSet, setID, RelativeLevel + 1);
            }

        }
        private void AttSetSavedForCopying(object sender, Csla.Core.SavedEventArgs e)
        {
            //if error
                //show warning and abort
            if (e.Error != null)
            {
                RadWindow.Alert(
                                "Error: the copy operation failed with"
                                +Environment.NewLine + "the following message:"
                                + Environment.NewLine + e.Error.Message
                                + Environment.NewLine + "Please contact EPPI-Support if"
                                + Environment.NewLine + "this problem persists"
                                );
                windowCopyingCodes.BusyAnimation.IsRunning = false;
                windowCopyingCodes.Close();
                toPlist = null;
                return;
            }
            //else
            //list.getNext()
            toPlist.GetCurrent().AttributeID = (e.NewObject as AttributeSet).AttributeId;
            AttributeSetToPaste astp = toPlist.GetNext();
            if (astp != null)
            {
                AttributeSet newAset = new AttributeSet();
                newAset.ParentAttributeId = toPlist.getParentID(astp);
                if (newAset.ParentAttributeId < 0)
                {//this went wrong!
                    RadWindow.Alert(
                                    "Error: the copy operation failed with"
                                    + Environment.NewLine + "an unexpected error."
                                    + Environment.NewLine + "Please contact EPPI-Support."
                                    );
                    windowCopyingCodes.BusyAnimation.IsRunning = false;
                    windowCopyingCodes.Close();
                    toPlist = null;
                    return;
                }
                newAset.AttributeOrder = astp.Original.AttributeOrder;
                newAset.AttributeTypeId = astp.Original.AttributeTypeId;
                newAset.ContactId = ri.UserId;
                newAset.SetId = astp.SetID;

                //should check that ParentAttributeId > 0!!
                newAset.AttributeSetDescription = astp.Original.AttributeSetDescription;
                newAset.AttributeDescription = astp.Original.AttributeDescription;
                newAset.AttributeName = astp.Original.AttributeName;
                //hook AttSet.Saved to AttSetSavedForCopying
                newAset.Saved += new EventHandler<Csla.Core.SavedEventArgs>(AttSetSavedForCopying);
                windowCopyingCodes.ProgressTxt.Text = (toPlist.IndexOf(astp) +1).ToString() + " of " + toPlist.Count;
                newAset.BeginSave();
                //if nextAtt != null
                //update counter in window
                //create new AttSet based on list.getNext();
                //hook AttSet.Saved to AttSetSavedForCopying
                //+= new EventHandler<Csla.Core.SavedEventArgs>(AttSetSavedForCopying);
                //begin save;
                //else finish it all
                //close window
                //delete list
                //reload ReviewSet
            }
            else
            {
                //all Done!
                CslaDataProvider prov4paste = App.Current.Resources["CodeSetsData"] as CslaDataProvider;
                prov4paste.DataChanged += new EventHandler(prov4paste_DataChanged);
                prov4paste.Refresh();
                return;
            }
        }

        void prov4paste_DataChanged(object sender, EventArgs e)
        {
            ToDoAtt = null;
            CslaDataProvider prov4paste = App.Current.Resources["CodeSetsData"] as CslaDataProvider;
            prov4paste.DataChanged -= prov4paste_DataChanged;
            windowCopyingCodes.BusyAnimation.IsRunning = false;
            windowCopyingCodes.Close();
            foreach (ReviewSet reviewSet2 in prov4paste.Data as ReviewSetsList)
            {
                ToDoAtt = reviewSet2.GetAttributeSetFromAttributeId(toPlist[0].AttributeID);
                if (ToDoAtt != null) break;
            }
            ExpandByAtt();
            prov4paste.DataChanged -= prov4paste_DataChanged;
            toPlist = null;
        }
        void ExpandByAtt()
        {
            ItemCollection ic = TreeView.Items;
             
            foreach (ReviewSet rs in ic)
            {
                
                if (rs.SetId == ToDoAtt.SetId)
                {
                    string PathToExpand = @"\" + ToDoAtt.AttributeName;
                    if (ToDoAtt.ParentAttributeId == 0)
                    {
                        PathToExpand = rs.SetName + PathToExpand;

                    }
                    else
                    {
                        AttributeSet someparent = rs.GetAttributeSetFromAttributeId(ToDoAtt.ParentAttributeId);
                        PathToExpand = @"\" + someparent.AttributeName + PathToExpand;

                        while (someparent.ParentAttributeId != 0)
                        {
                            someparent = rs.GetAttributeSetFromAttributeId(someparent.ParentAttributeId);
                            PathToExpand = @"\" + someparent.AttributeName + PathToExpand;
                        }
                    
                        PathToExpand = rs.SetName + PathToExpand;
                    }
                    TreeView.ExpandItemByPath(PathToExpand, @"\");
                    TreeView.BringPathIntoView(PathToExpand);
                    TreeView.SelectItemByPath(PathToExpand);
                    break;
                }

            }

        }
        void TreeView_Loaded4Paste(object sender, RoutedEventArgs e)
        {
            ExpandByAtt();
            TreeView.Loaded -= TreeView_Loaded4Paste;
        }
        private void DoNewCodeSet()
        {
            ReadOnlySetTypeList rostl = ((App.Current.Resources["SetTypes"] as CslaDataProvider).Data as ReadOnlySetTypeList);
            if (rostl == null) return;//should never happen, but a cunning user may create a new set before the releavant data got loaded.
            ReviewSet newCodeSet = new ReviewSet();
            newCodeSet.Attributes = new AttributeSetList();
            newCodeSet.CodingIsFinal = true;
            newCodeSet.AllowCodingEdits = true;
            newCodeSet.ReviewId = ri.ReviewId;
            newCodeSet.SetOrder = ((App.Current.Resources["CodeSetsData"] as CslaDataProvider).Data as ReviewSetsList).Count;
            newCodeSet.SetName = "Edit code set title";
            newCodeSet.SetTypeId = 3;
            foreach (ReadOnlySetType rost in rostl)
            {
                if (rost.SetTypeId == newCodeSet.SetTypeId)
                {
                    newCodeSet.SetType = rost;
                    break;
                }
            }
            windowNewCodeSet.GridEditOrCreateCodeSet.DataContext = newCodeSet;
            windowNewCodeSet.GotFocus += new RoutedEventHandler(windowNewCodeSet.RadWindow_GotFocus);
            windowNewCodeSet.ShowDialog();
        }

        private void DoNodeProperties()
        {
            AttributeSet attributeSet = TreeView.SelectedItem as AttributeSet;
            ReviewSetsList rsl = TreeView.ItemsSource as ReviewSetsList;
            if (attributeSet != null)
            {
                attributeSet.BeginEdit();
                ReviewSet rs = rsl.GetReviewSet(attributeSet.SetId);
                Binding binding = new Binding();
                binding.Source = rs.SetType;
                binding.Path = new PropertyPath("AllowedCodeTypes");
                windowCodeProperties.editCodeType.SetBinding(ComboBox.ItemsSourceProperty, binding);
                int aaa = attributeSet.AttributeTreeLevel;
                //windowCodeProperties.editCodeType.ItemsSource = rs.SetType.AllowedCodeTypes;
                if (rs == null || !rs.AllowCodingEdits || !HasWriteRights)
                {
                    windowCodeProperties.EnOrDisableEdit(false);
                }
                else windowCodeProperties.EnOrDisableEdit(true);
                windowCodeProperties.GridCodeProperties.DataContext = attributeSet;
                //Csla.NameValueListBase<int, string>.NameValuePair t = (Csla.NameValueListBase<int, string>.NameValuePair)windowCodeProperties.editCodeType.SelectedItem;
                foreach (Csla.NameValueListBase<int, string>.NameValuePair t in rs.SetType.AllowedCodeTypes)
                {
                    if (t.Key == attributeSet.AttributeTypeId)
                    {
                        windowCodeProperties.editCodeType.SelectedItem = t;
                        break;
                    }
                }
                //windowCodeProperties.editCodeType.SelectedIndex = attributeSet.AttributeTypeId;
                windowCodeProperties.ShowDialog();
            }
            else
            {
                ReviewSet rs = TreeView.SelectedItem as ReviewSet;
                if (rs != null)
                {
                    if (ControlContext == "homeDocuments")
                    {
                        rsl.LoadingAttributes = true;
                        rs.ClearItemData();
                        rsl.LoadingAttributes = false;
                        rs.BeginEdit();
                        windowEditCodeSet.GridEditCodeSet.DataContext = rs;
                        if (rs.CodingIsFinal == true)
                        {
                            windowEditCodeSet.TextBlockEditCodeSetMethodSingle.Visibility = System.Windows.Visibility.Visible;
                            windowEditCodeSet.HyperLinkChangeMethodToMultiple.Visibility = System.Windows.Visibility.Visible;
                            windowEditCodeSet.TextBlockEditCodeSetMethodMultiple.Visibility = System.Windows.Visibility.Collapsed;
                            windowEditCodeSet.HyperLinkChangeMethodToSingle.Visibility = System.Windows.Visibility.Collapsed;
                        }
                        else
                        {
                            windowEditCodeSet.TextBlockEditCodeSetMethodSingle.Visibility = System.Windows.Visibility.Collapsed;
                            windowEditCodeSet.HyperLinkChangeMethodToMultiple.Visibility = System.Windows.Visibility.Collapsed;
                            windowEditCodeSet.TextBlockEditCodeSetMethodMultiple.Visibility = System.Windows.Visibility.Visible;
                            windowEditCodeSet.HyperLinkChangeMethodToSingle.Visibility = System.Windows.Visibility.Visible;
                        }
                        windowEditCodeSet.EnOrDisableEdit(HasWriteRights && rs.AllowCodingEdits);
                        windowEditCodeSet.ShowDialog();
                    }
                    else
                    {
                        rs.BeginEdit();
                        windowItemSetProperties.GridItemSetProperties.DataContext = rs;
                        windowItemSetProperties.TextBlockCodingCompleted.Visibility = Visibility.Visible;
                        windowItemSetProperties.CheckBoxCodingCompleted.Visibility = Visibility.Visible;
                        windowItemSetProperties.TextBlockCodingIsLocked.Visibility = Visibility.Visible;
                        windowItemSetProperties.CheckBoxCodingIsLocked.Visibility = Visibility.Visible;
                        windowEditCodeSet.EnOrDisableEdit(HasWriteRights && rs.AllowCodingEdits);
                        windowItemSetProperties.ShowDialog();
                    }
                }
            }
        }

        private void DoNewAttribute()
        {
            AttributeSetList attributesToAddTo = null;
            AttributeSet attributeSet = TreeView.SelectedItem as AttributeSet;
            int setId = 0, maxDepth = 0;
            ReviewSetsList rsl = TreeView.ItemsSource as ReviewSetsList;
            ReviewSet reviewSet;
            if (attributeSet != null)
            {
                //attributeSet.AddNewChildAttribute();
                attributesToAddTo = attributeSet.Attributes;
                setId = attributeSet.SetId;
                maxDepth = attributeSet.MaxDepth;
                reviewSet = rsl.GetReviewSet(attributeSet.SetId);
            }
            else
            {
                reviewSet = TreeView.SelectedItem as ReviewSet;
                if (reviewSet != null)
                {
                    //reviewSet.AddNewChildAttribute();
                    attributesToAddTo = reviewSet.Attributes;
                    setId = reviewSet.SetId;
                    maxDepth = reviewSet.SetType.MaxDepth;
                }
                else return;
            }
            if (attributesToAddTo != null)
            {
                AttributeSet newAttribute = new AttributeSet();
                newAttribute.AttributeOrder = attributesToAddTo.Count;
                newAttribute.AttributeTypeId = 1;
                newAttribute.ContactId = ri.UserId;
                newAttribute.SetId = setId;
                newAttribute.MaxDepth = maxDepth;
                Binding binding = new Binding();
                binding.Source = reviewSet.SetType;
                binding.Path = new PropertyPath("AllowedCodeTypes");
                windowCodeProperties.editCodeType.SetBinding(ComboBox.ItemsSourceProperty, binding);
                windowCodeProperties.GridCodeProperties.DataContext = newAttribute;
                if (attributeSet != null)
                {
                    newAttribute.ParentAttributeId = attributeSet.AttributeId;
                }
                else
                {
                    newAttribute.ParentAttributeId = 0;
                }
                if (windowCodeProperties.editCodeType.SelectedIndex < 0) //there is no default value for the code type, not good cause all new codes should have a type
                {
                    if (reviewSet.SetTypeId == 5)//screening!
                    {//we want the default to be "Excluded"
                        windowCodeProperties.editCodeType.SelectedItem = reviewSet.SetType.AllowedCodeTypes.GetItemByKey(11);
                    }
                    else
                    {
                        windowCodeProperties.editCodeType.SelectedIndex = 0;//the first in the list
                    }

                }
                windowCodeProperties.ShowDialog();
            }
        }

        private void TreeView_Edited(object sender, RadTreeViewItemEditedEventArgs e)
        {
            RadTreeViewItem treeItem = e.Source as RadTreeViewItem;
            AttributeSet attributeSet = treeItem.Item as AttributeSet;
            if (attributeSet != null)
            {
                attributeSet.AttributeName = e.NewValue.ToString() == "" ? e.OldValue.ToString() : e.NewValue.ToString();
                if (e.NewValue.ToString() != "")
                {
                    attributeSet.Saved += (o, e2) =>
                    {
                        BusyLoading.IsRunning = false;
                        if (e2.Error != null)
                            MessageBox.Show(e2.Error.Message);
                    };
                    attributeSet.BeginSave(true);
                    BusyLoading.IsRunning = true;
                }
            }
            else
            {
                ReviewSet reviewSet = treeItem.Item as ReviewSet;
                if (reviewSet != null)
                {
                    reviewSet.SetName = e.NewValue.ToString() == "" ? e.OldValue.ToString() : e.NewValue.ToString();
                    if (e.NewValue.ToString() != "")
                    {
                        reviewSet.Saved += (o, e2) =>
                        {
                            BusyLoading.IsRunning = false;
                            if (e2.Error != null)
                                MessageBox.Show(e2.Error.Message);
                        };
                        reviewSet.BeginSave(true);
                        BusyLoading.IsRunning = true;
                    }
                }
            }
        }
        public void MakeBusy()
        {
            BusyLoading.IsRunning = true;
            if (TreeView != null) TreeView.IsEnabled = false;
        }
        public void LoadItemAttributes(List<ItemSet> data)
        {
            ReviewSetsList reviewSets = TreeView.ItemsSource as ReviewSetsList;
            reviewSets.LoadingAttributes = true;
            reviewSets.SetItemData(data);
            reviewSets.LoadingAttributes = false;
            BusyLoading.IsRunning = false;
            //BusyLoadingAllAttributes.IsRunning = false;
            //DoLiveComparisons();
            TreeView.IsEnabled = true;
        }
        private void LoadItemAttributes(Int64 ItemId)
        {
            DataPortal<ItemSetList> dp = new DataPortal<ItemSetList>();
            dp.FetchCompleted += (o, e2) =>
            {
                if (e2.Object != null)
                {
                    ReviewSetsList reviewSets = TreeView.ItemsSource as ReviewSetsList;
                    reviewSets.LoadingAttributes = true;
                    ItemSetList isl = e2.Object as ItemSetList;
                    reviewSets.SetItemData(isl.SetsVisibleToUser);
                    reviewSets.LoadingAttributes = false;
                    BusyLoading.IsRunning = false;
                    //BusyLoadingAllAttributes.IsRunning = false;
                    //DoLiveComparisons();
                    TreeView.IsEnabled = true;
                }
            };
            BusyLoading.IsRunning = true;
            //BusyLoadingAllAttributes.IsRunning = true;
            TreeView.IsEnabled = false;
            dp.BeginFetch(new SingleCriteria<ItemSetList, Int64>(ItemId));
        }

        private AttributeSet currentAttributeSet;
        private CheckBox currentCheckBox;

        private void bt_Click(object sender, RoutedEventArgs e)
        {
            Button bt = sender as Button;
            currentAttributeSet = bt.DataContext as AttributeSet;
            if (currentAttributeSet == null) return;
            TreeView.SelectedItem = currentAttributeSet;
            StackPanel sp = bt.Parent as StackPanel;
            currentCheckBox = sp.Children[0] as CheckBox;
            if ((currentCheckBox.IsChecked == true) && (currentAttributeSet.ItemData != null))
            {
                windowAdditionalText.TextBoxAdditionalText.Text = currentAttributeSet.ItemData.AdditionalText;
            }
            else
            {
                windowAdditionalText.TextBoxAdditionalText.Text = "";
            }
            windowAdditionalText.saveAdditionaltext.IsEnabled = !currentAttributeSet.IsLocked;
            windowAdditionalText.ShowDialog();
            
        }

        private void saveAdditionaltext_Click(object sender, RoutedEventArgs e)
        {
            ReviewInfo rInfo = ((App)(Application.Current)).GetReviewInfo();
            if (currentAttributeSet.ItemData != null)
            {
                DataPortal<ItemAttributeSaveCommand> dp = new DataPortal<ItemAttributeSaveCommand>();
                ItemAttributeSaveCommand command = new ItemAttributeSaveCommand("Update",
                    currentAttributeSet.ItemData.ItemAttributeId,
                    currentAttributeSet.ItemData.ItemSetId,
                    windowAdditionalText.TextBoxAdditionalText.Text,
                    currentAttributeSet.ItemData.AttributeId,
                    currentAttributeSet.ItemData.SetId,
                    currentAttributeSet.ItemData.ItemId,
                    rInfo);
                dp.ExecuteCompleted += (o, e2) =>
                {
                    BusyLoading.IsRunning = false;
                    if (e2.Error != null)
                    {
                        MessageBox.Show(e2.Error.Message);
                    }
                    else
                    {
                        currentAttributeSet.ItemData.AdditionalText = windowAdditionalText.TextBoxAdditionalText.Text;
                    }
                };
                BusyLoading.IsRunning = true;
                dp.BeginExecute(command);
            }
            else
            {
                ItemAttributeData itemData = new ItemAttributeData();
                itemData.ItemAttributeId = 0;
                itemData.ItemId = (DataContext as Item).ItemId;
                itemData.ItemSetId = 0;
                itemData.SetId = currentAttributeSet.SetId;
                itemData.AttributeId = currentAttributeSet.AttributeId;
                itemData.AdditionalText = windowAdditionalText.TextBoxAdditionalText.Text;

                DataPortal<ItemAttributeSaveCommand> dp = new DataPortal<ItemAttributeSaveCommand>();
                ItemAttributeSaveCommand command = new ItemAttributeSaveCommand("Insert",
                    itemData.AttributeId,
                    itemData.ItemSetId,
                    itemData.AdditionalText,
                    itemData.AttributeId,
                    itemData.SetId,
                    itemData.ItemId,
                    rInfo);
                dp.ExecuteCompleted += (o, e2) =>
                {
                    BusyLoading.IsRunning = false;
                    if (e2.Error != null)
                    {
                        MessageBox.Show(e2.Error.Message);
                    }
                    else
                    {
                        ReviewSetsList reviewSets = TreeView.ItemsSource as ReviewSetsList;
                        itemData.ItemAttributeId = e2.Object.ItemAttributeId;
                        itemData.ItemSetId = e2.Object.ItemSetId;
                        reviewSets.LoadingAttributes = true; // setting this so that the autosave insert doesn't kick in
                        currentAttributeSet.IsSelected = true;
                        currentAttributeSet.ItemData = itemData;
                        reviewSets.LoadingAttributes = false;
                    }
                };
                BusyLoading.IsRunning = true;
                dp.BeginExecute(command);
            }
            windowAdditionalText.Close();
        }

        private void cancelAdditionaltext_Click(object sender, RoutedEventArgs e)
        {
            windowAdditionalText.Close();
        }

        void windowAdditionalText_Closed(object sender, WindowClosedEventArgs e)
        {
            windowAdditionalText.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.Manual;
        }

        private ItemAttributeData CreateItemData()
        {
            ItemAttributeData itemData = null;
            AttributeSet iad = TreeView.SelectedItem as AttributeSet;
            if (iad != null)
            {
                itemData = new ItemAttributeData();
                itemData.ItemAttributeId = 0;
                itemData.ItemId = (DataContext as Item).ItemId;
                itemData.ItemSetId = 0;
                itemData.SetId = iad.SetId;
                itemData.AttributeId = iad.AttributeId;
                itemData.AdditionalText = "";
                itemData.IsCompleted = false;
                itemData.IsLocked = true;
            }
            return itemData;
        }

        private void HighlightSelectedText()
        {
            if (CurrentTextDocument != null)
            {
                _rich.ChangeTextHighlightColor(Colors.Yellow);
            }
        }

        private void UnHighlightSelectedText()
        {
            if (CurrentTextDocument != null)
            {
                _rich.ChangeTextHighlightColor(Colors.White);
                //CharacterProperties charProperties = rich.RichControl.Document.BeginUpdateCharacters(rich.RichControl.Document.Selection);
                //charProperties.BackColor = Color.FromArgb(255, 255, 255, 255);
                //rich.RichControl.Document.EndUpdateCharacters(charProperties);
            }
        }

        public void HighlightAll()
        {
            if (CurrentTextDocument != null)
            {
                ClearHighlights();
                byte r = 255;
                byte g = 255;

                foreach (ReviewSet rs in TreeView.ItemsSource as ReviewSetsList)
                {
                    foreach (AttributeSet attributeSet in rs.Attributes)
                    {
                        HighlightText(attributeSet, Color.FromArgb(200, r, g, 0));
                        r = Convert.ToByte(r - 5);
                        g = Convert.ToByte(g - 10);
                        if (r < 10) r = 255;
                        if (g < 10) g = 255;
                        
                    }
                }
            }
        }

        private void HighlightText(AttributeSet attributeSet, Color colour)
        {
            if (attributeSet.ItemData != null)
            {
                //List<DocumentPosition> Ostart = new List<DocumentPosition>(), Oend = new List<DocumentPosition>();
                List<SelectionRange> Oranges = new List<SelectionRange>();
                if (!_rich.Document.Selection.IsEmpty)
                {
                    foreach (SelectionRange range in _rich.Document.Selection.Ranges)
                    {
                        //Ostart.Add(range.StartPosition);
                        //Oend.Add(range.EndPosition);
                        Oranges.Add(range);
                    }
                }
                foreach (ItemAttributeText iat in attributeSet.ItemData.ItemAttributeTextList)
                {
                    if (CurrentTextDocument.ItemDocumentId == iat.ItemDocumentId)
                    {
                        // All of the counting returns is for imported coding (from html). No need for natively coded items in this control.
                        //int numReturnsFrom = GetNewlineCount(rich.RichControl.Document.Text.Substring(0, iat.TextFrom));
                        //int numReturnsTo = GetNewlineCount(rich.RichControl.Document.Text.Substring(0, iat.TextFrom));
                        //DocumentRange sel = rich.RichControl.Document.CreateRange(iat.TextFrom - numReturnsFrom, iat.TextTo - iat.TextFrom);

                        TextRange textRange = documentMap.MapFromIndexInText(
                            Math.Max
                            (
                                newLinesHelper.FromDBToViewerIndex(iat.TextFrom) - newLinesHelper.FromDBToViewerIndex(CurrentTextDocument.TextFrom), 
                                0
                            ),
                            newLinesHelper.FromDBToViewerIndex(iat.TextTo) - newLinesHelper.FromDBToViewerIndex(iat.TextFrom));
                        _rich.Document.Selection.Clear();
                        _rich.Document.Selection.AddSelectionStart(textRange.StartPosition);
                        _rich.Document.Selection.AddSelectionEnd(textRange.EndPosition);
                        _rich.ChangeTextHighlightColor(Colors.Yellow);
                        //DocumentRange sel = rich.RichControl.Document.CreateRange(Math.Max(iat.TextFrom - CurrentTextDocument.TextFrom, 0),
                        //    Math.Min(iat.TextTo - iat.TextFrom, CurrentTextDocument.Text.Length));
                        //CharacterProperties charProperties = rich.RichControl.Document.BeginUpdateCharacters(sel);
                        //charProperties.BackColor = colour;
                        //rich.RichControl.Document.EndUpdateCharacters(charProperties);
                    }
                }
                _rich.Document.Selection.Clear();
                foreach (SelectionRange range in Oranges)
                {
                    _rich.Document.Selection.AddSelectionStart(range.StartPosition);
                    _rich.Document.Selection.AddSelectionEnd(range.EndPosition);
                }
            }
        }

        private void HightlightSelectedCode()
        {
            if (CurrentTextDocument != null)
            {
                ClearHighlights();

                AttributeSet attributeSet = TreeView.SelectedItem as AttributeSet;
                if (attributeSet != null)
                {
                    documentMap = new DocumentTextMap(_rich.Document);
                    DocumentPosition pos1 = new DocumentPosition(_rich.Document);
                    DocumentPosition pos2 = new DocumentPosition(_rich.Document);
                    pos1.MoveToFirstPositionInDocument();
                    pos2.MoveToLastPositionInDocument();
                    documentMap.InitMap(pos1, pos2);
                    if ((attributeSet.ItemData != null) && (attributeSet.ItemData.ItemAttributeTextList != null) && (CurrentTextDocument != null))
                    {
                        List<SelectionRange> Oranges = new List<SelectionRange>();
                        if (!_rich.Document.Selection.IsEmpty)
                        {
                            foreach (SelectionRange range in _rich.Document.Selection.Ranges)
                            {
                                //Ostart.Add(range.StartPosition);
                                //Oend.Add(range.EndPosition);
                                Oranges.Add(range);
                            }
                        }
                        foreach (ItemAttributeText iat in attributeSet.ItemData.ItemAttributeTextList)
                        {
                            if (CurrentTextDocument.ItemDocumentId == iat.ItemDocumentId)
                            {
                                // All of the counting returns is for imported coding (from html). No need for natively coded items in this control.
                                //int numReturnsFrom = GetNewlineCount(rich.RichControl.Document.Text.Substring(0, iat.TextFrom));
                                //int numReturnsTo = GetNewlineCount(rich.RichControl.Document.Text.Substring(0, iat.TextFrom));
                                //DocumentRange sel = rich.RichControl.Document.CreateRange(iat.TextFrom - numReturnsFrom, iat.TextTo - iat.TextFrom);
                                
 
                                TextRange textRange = documentMap.MapFromIndexInText(
                                    Math.Max(
                                            newLinesHelper.FromDBToViewerIndex(iat.TextFrom)
                                            - newLinesHelper.FromDBToViewerIndex(CurrentTextDocument.TextFrom), 0
                                            ),
                                    newLinesHelper.FromDBToViewerIndex(iat.TextTo) - newLinesHelper.FromDBToViewerIndex(iat.TextFrom));
                                _rich.Document.Selection.BeginUpdate();
                                _rich.Document.Selection.Clear();
                                //_rich.Document.CaretPosition.MoveToPosition(textRange.StartPosition);
                                _rich.Document.Selection.SetSelectionStart(textRange.StartPosition);
                                //_rich.Document.CaretPosition.MoveToPosition(textRange.EndPosition);
                                _rich.Document.Selection.AddSelectionEnd(textRange.EndPosition);
                                _rich.Document.Selection.EndUpdate();
                                _rich.ChangeTextHighlightColor(Colors.Yellow);
                                //_rich.Insert(" FOTTITI ");
                                
                                //DocumentRange sel = rich.RichControl.Document.CreateRange(Math.Max(iat.TextFrom - CurrentTextDocument.TextFrom, 0), 
                                //    Math.Min(iat.TextTo - iat.TextFrom, CurrentTextDocument.Text.Length));
                                //CharacterProperties charProperties = rich.RichControl.Document.BeginUpdateCharacters(sel);
                                //charProperties.BackColor = Color.FromArgb(200, 255, 255, 0);
                                //rich.RichControl.Document.EndUpdateCharacters(charProperties);
                            }
                        }
                        _rich.Document.Selection.Clear();
                        foreach (SelectionRange range in Oranges)
                        {
                            _rich.Document.Selection.AddSelectionStart(range.StartPosition);
                            _rich.Document.Selection.AddSelectionEnd(range.EndPosition);
                        }
                        
                    }
                }
            } 
        }

        //private int GetCorrectPosition(string text, int ToPostion)
        //{
        //    int returnValue = 0;
        //    int startingPoint = 0;
        //    //if (ToPostion + 1 >= text.Length) ToPostion++;
        //    Regex re1 = new Regex("\n");
        //    Regex re2 = new Regex("\r");
        //    Regex re3 = new Regex("\r\n");
        //    MatchCollection ma1 = re1.Matches(text.Substring(0, ToPostion ));
        //    MatchCollection ma2 = re2.Matches(text.Substring(0, ToPostion ));
        //    MatchCollection ma3 = re3.Matches(text.Substring(0, ToPostion ));

        //    if (ma1.Count == ma2.Count && ma1.Count == ma3.Count)
        //    {//all newlines are made of 2 chars: it's all right
        //        return ToPostion;
        //    }
        //    else if (ma3.Count > 0) //it's a mix!!!
        //    {
        //        return ToPostion + ma1.Count - ma3.Count;
        //    }
        //    else
        //    {//all newlines are made of one char add 1 per newline
        //        return ToPostion + ma1.Count;
        //    }
        //    //while ((text.IndexOf("\r\n", startingPoint) != -1))
        //    //{
        //    //    returnValue++;
        //    //    startingPoint = text.IndexOf("\r\n", startingPoint) + 1;
        //    //}
        //    return returnValue;
        //}

        /* Added to deal with imported data from ER3.
        private int GetNewlineCount(string text)
        {
            int returnValue = 0;
            int startingPoint = 0;
            while ((text.IndexOf("\r\n", startingPoint) != -1))
            {
                returnValue++;
                startingPoint = text.IndexOf("\r\n", startingPoint) + 1;
            }
            return returnValue;
        }
        */

        private void ClearHighlights()
        {
            if (CurrentTextDocument != null)
            {
                List<SelectionRange> Oranges = new List<SelectionRange>();
                if (!_rich.Document.Selection.IsEmpty)
                {
                    foreach (SelectionRange range in _rich.Document.Selection.Ranges)
                    {
                        //Ostart.Add(range.StartPosition);
                        //Oend.Add(range.EndPosition);
                        Oranges.Add(range);
                    }
                }
                _rich.Document.Selection.Clear();
                _rich.Document.Selection.SelectAll();
                _rich.ChangeTextHighlightColor(Colors.White);
                _rich.Document.Selection.Clear();
                foreach (SelectionRange range in Oranges)
                {
                    _rich.Document.Selection.AddSelectionStart(range.StartPosition);
                    _rich.Document.Selection.AddSelectionEnd(range.EndPosition);
                }
                //CharacterProperties charProperties = rich.RichControl.Document.BeginUpdateCharacters(rich.RichControl.Document.Range);
                //charProperties.BackColor = Color.FromArgb(255, 255, 255, 255);
                //rich.RichControl.Document.EndUpdateCharacters(charProperties);
            }
        }

        public string CodesInSelection()
        {
            if (_rich.Document.Selection.GetSelectedText() == null || _rich.Document.Selection.GetSelectedText().Length < 1) return "";
            string codes = "";
            documentMap = new DocumentTextMap(_rich.Document);
            DocumentPosition pos1 = new DocumentPosition(_rich.Document);
            DocumentPosition pos2 = new DocumentPosition(_rich.Document);
            pos1.MoveToFirstPositionInDocument();
            pos2.MoveToLastPositionInDocument();
            documentMap.InitMap(pos1, pos2);
            List<SelectionRange> Oranges = new List<SelectionRange>();
            Dictionary<int, int> IntRanges = new Dictionary<int, int>();
            int TextFrom, TextTo;
            foreach (SelectionRange range in _rich.Document.Selection.Ranges)
            {
                Oranges.Add(range);//used to rebuild selection when done and to build the list of char indexes
            }
            foreach (SelectionRange range in Oranges)
            {//need a second cycle as the following modifies _rich.Document.Selection
                _rich.Document.Selection.AddSelectionStart(range.StartPosition);
                _rich.Document.Selection.AddSelectionEnd(range.EndPosition);
                string tmp = _rich.Document.Selection.GetSelectedText();
                int currI = documentMap.Text.IndexOf(tmp, 0);
                while (currI >= 0)
                {
                    TextRange textRange = documentMap.MapFromIndexInText(currI, tmp.Length);
                    if (range.StartPosition.Location.X == textRange.StartPosition.Location.X && range.StartPosition.Location.Y == textRange.StartPosition.Location.Y)
                    {//this is the text we were looking for, so we know start and end index
                        TextFrom = currI + CurrentTextDocument.TextFrom;
                        TextTo = currI + CurrentTextDocument.TextFrom + tmp.Length;
                        currI = -1;//stop cycling we've found the piece of text we wanted.
                        IntRanges.Add(TextFrom, TextTo);
                    }
                    else
                    {//find next string in text
                        currI = CurrentTextDocument.Text.IndexOf(tmp, currI + tmp.Length);
                    }
                }
            }
            foreach (ReviewSet rs in TreeView.ItemsSource as ReviewSetsList)
            {
                if (rs.ItemSetId == 0) continue;
                foreach (AttributeSet attributeSet in rs.Attributes)
                {
                    string tempstr = ListUsedCodes(attributeSet, IntRanges);//called recursively to get all tree-levels
                    if ((tempstr != "") && (codes == ""))
                    {
                        codes = tempstr;
                    }
                    else if (tempstr != "")
                    {
                        codes = codes + " | " + tempstr;
                    }
                }
            }
            foreach (SelectionRange range in Oranges)
            {
                _rich.Document.Selection.AddSelectionStart(range.StartPosition);
                _rich.Document.Selection.AddSelectionEnd(range.EndPosition);
            }
            return codes;
        }

        private string ListUsedCodes(AttributeSet attributeSet, Dictionary<int, int> IntRanges)
        {
            string codes = "";
            int TextFrom, TextTo;
            _rich.Document.Selection.Clear();
            foreach (KeyValuePair<int, int> range in IntRanges)
            {   
                TextFrom = range.Key;
                TextTo = range.Value;
                foreach (AttributeSet atSet in attributeSet.Attributes)
                {
                    string tempstr = ListUsedCodes(atSet, IntRanges);
                    if ((tempstr != "") && (codes == ""))
                    {
                        codes = tempstr;
                    }
                    else if (tempstr != "")
                    {
                        codes = codes + " | " + tempstr;
                    }
                }
                if (attributeSet.ItemData != null && attributeSet.ItemData.ItemAttributeTextList != null)
                {
                    foreach (ItemAttributeText iat in attributeSet.ItemData.ItemAttributeTextList)
                    {
                        if ((CurrentTextDocument.ItemDocumentId == iat.ItemDocumentId) &&
                            (((TextFrom >= iat.TextFrom) && (TextTo <= iat.TextTo)) ||
                            ((TextFrom <= iat.TextFrom) && (TextTo >= iat.TextFrom)) ||
                            (TextFrom <= iat.TextTo) && (TextTo >= iat.TextTo)))
                        {
                            if (codes == "")
                            {
                                codes = attributeSet.AttributeName;
                            }
                            else
                            {
                                codes = codes + " | " + attributeSet.AttributeName;
                            }
                        }
                    }                                               
                }
            }
            return codes;
        }

        private void CodeOrUnCodeSelectedText(AttributeSet attributeSet, string CodeOrUnCode)
        {
            //int numReturnsStart = GetNewlineCount(rich.RichControl.Document.Text.Substring(0, rich.RichControl.Document.Selection.Start.ToInt()));
            //int numReturnsEnd = GetNewlineCount(rich.RichControl.Document.Text.Substring(0, rich.RichControl.Document.Selection.End.ToInt()));
            List<SelectionRange> Oranges = new List<SelectionRange>();
            if (!_rich.Document.Selection.IsEmpty)
            {
                foreach (SelectionRange range in _rich.Document.Selection.Ranges)
                {
                    //Ostart.Add(range.StartPosition);
                    //Oend.Add(range.EndPosition);
                    Oranges.Add(range);
                }
            }
            int TextFrom = 0 , TextTo = 0;
            if (Oranges.Count > 0)
            {
                documentMap = new DocumentTextMap(_rich.Document);
                DocumentPosition pos1 = new DocumentPosition(_rich.Document);
                DocumentPosition pos2 = new DocumentPosition(_rich.Document);
                pos1.MoveToFirstPositionInDocument();
                pos2.MoveToLastPositionInDocument();
                documentMap.InitMap(pos1, pos2);
                _rich.Document.Selection.Clear();
                SelectionRange range = Oranges[0];
                //will only consider the first selected text, if CTRL+MouseDrag is used to create multiple selections, then too bad, only the first will be processed

                _rich.Document.Selection.AddSelectionStart(range.StartPosition);
                _rich.Document.Selection.AddSelectionEnd(range.EndPosition);
                string tmp = _rich.Document.Selection.GetSelectedText();
                tmp = tmp.Replace("\r", "");//we now that most er4 docs use the single version so we'll match this first
                //int currI =  CurrentTextDocument.Text.IndexOf(tmp, 0);
                int currI = CurrentTextDocument.Text.IndexOf(tmp, 0);//here is a problem: texts won't match if there is a different use of \n and \r\n!!
                if (currI == -1)
                {
                    tmp = tmp.Replace("\n", "\r\n");//we try reverting to the double version
                    currI = CurrentTextDocument.Text.IndexOf(tmp, 0);
                }
                if (currI == -1)
                {//wow! neither \n nor \r\n versions work, document must be using a mixture: we should fix this on the backend as it would be too difficult to handle in-code!
                    RadWindow.Alert("Sorry: this text document has some unusual" + Environment.NewLine
                                    + "characteristics. Please contact EPPI-Support team:" + Environment.NewLine
                                    + "we can rectify this on a per-document basis.");
                    return;
                }
                while (currI >= 0)
                {//this cycle is needed to find the Index value of the selected text (in the viewer) within the actual text in the DB.
                    //will keep cycling until it finds a correspondence with what the user selected in the viewer
                    //stops cycling if no more matches are found (currI == -1)
                    //1 use documentMap to define a textRange in the viewer, based on the Index value of the string found in the DB version of the text.
                    TextRange textRange = documentMap.MapFromIndexInText(newLinesHelper.FromDBToViewerIndex(currI), tmp.Length);
                    //newLinesHelper converts currI to the Index value valid within the viewer
                    if (range.StartPosition.Location.X == textRange.StartPosition.Location.X && range.StartPosition.Location.Y == textRange.StartPosition.Location.Y)
                    {//true if range (what was selected by the user) is the same as textRange (built by matching the DB text)
                        TextFrom = currI + CurrentTextDocument.TextFrom;
                        //the following is not needed because currI applies to the DB version of the txt
                        //TextFrom = newLinesHelper.FromViewerToDBIndex(TextFrom);
                        TextTo = currI + CurrentTextDocument.TextFrom + tmp.Length;
                        //TextTo = newLinesHelper.FromViewerToDBIndex(TextTo);
                        currI = -1;//stop cycling we've found the piece of text we wanted.
                    }
                    else
                    {
                        currI = CurrentTextDocument.Text.IndexOf(tmp, currI + tmp.Length);
                    }
                }
            }
            //int TextFrom = rich.RichControl.Document.Selection.Start.ToInt() + CurrentTextDocument.TextFrom;
            //int TextTo = rich.RichControl.Document.Selection.Start.ToInt() + CurrentTextDocument.TextFrom + rich.RichControl.Document.Selection.Length;

            if (TextTo - TextFrom < 1)
            {
                RadWindow.Alert("No text selected");
                return;
            }

            DataPortal<ItemAttributeTextSaveCommand> dp = new DataPortal<ItemAttributeTextSaveCommand>();
            ItemAttributeTextSaveCommand command = new ItemAttributeTextSaveCommand(CodeOrUnCode,
                attributeSet.ItemData.ItemAttributeId,
                CurrentTextDocument.ItemDocumentId,
                // numReturns are needed for imported data - no need with data coded with this control.
                //rich.RichControl.Document.Selection.Start.ToInt() + numReturnsStart,
                //rich.RichControl.Document.Selection.Start.ToInt() + rich.RichControl.Document.Selection.Length + numReturnsEnd - numReturnsStart,
                TextFrom,
                TextTo,
                0);
            dp.ExecuteCompleted += (o, e2) =>
            {
                
                BusyLoading.IsRunning = false;
                if (e2.Error != null)
                {
                    MessageBox.Show(e2.Error.Message);
                }
                else
                {
                    LoadItemAttributeTextList(attributeSet);
                    
                }
            };
            BusyLoading.IsRunning = true;
            dp.BeginExecute(command);
        }

        private void LoadItemAttributeTextList(AttributeSet attributeSet)
        {
            DataPortal<ItemAttributeTextList> dp = new DataPortal<ItemAttributeTextList>();
            dp.FetchCompleted += (o, e2) =>
            {
                BusyLoading.IsRunning = false;
                if (e2.Error != null)
                {
                    MessageBox.Show(e2.Error.Message);
                }
                else
                {
                    attributeSet.ItemData.ItemAttributeTextList = e2.Object;
                    HightlightSelectedCode();
                }
            };
            BusyLoading.IsRunning = true;
            dp.BeginFetch(new SingleCriteria<ItemAttributeTextList, Int64>(attributeSet.ItemData.ItemAttributeId));
        }

        private void TreeView_PreviewDragEnded(object sender, Telerik.Windows.DragDrop.DragDropCompletedEventArgs e)
        {
            e.Handled = true;
            TreeViewDragDropOptions options = DragDropPayloadManager.GetDataFromObject(e.Data, TreeViewDragDropOptions.Key) as TreeViewDragDropOptions;
            if (options == null) return;

            if (options.DropTargetItem == null && !(options.DropTargetTree == TreeView && options.DropPosition == DropPosition.Inside))
            {// if the drop target is null and the target location is inside the codestree, the codeset was dragged past the last codeset
                // MessageBox.Show("Sorry, you cannot drag to the root position"); can't show this message, as drag drop is also into report
                return;
            }

            ReviewSetsList rsl = TreeView.ItemsSource as ReviewSetsList;
            ReviewSet rs;
            AttributeSet draggedAttribute;
            using(IEnumerator<object> iter = options.DraggedItems.GetEnumerator())
            {
                iter.MoveNext();
                rs = iter.Current as ReviewSet;
                draggedAttribute = iter.Current as AttributeSet;//this will be used below, after handling the case where the dragged item was a reviewSet
            }
            if (rs != null)
            {
                rs.SetIsNew();
                ReviewSet rs2;
                if (options.DropTargetItem == null)
                {
                    rs2 = rsl[rsl.Count - 1];
                }
                else
                {
                    rs2 = options.DropTargetItem.DataContext as ReviewSet;
                    options.DropPosition = DropPosition.After;
                    //manually adjusting for when the codeset was dragged past the last codeset
                }
                if (rs != null && rs2 != null)
                {
                    int newPos = 0;
                    int oldPos = rs.SetOrder;
                    switch (options.DropPosition)
                    {
                        case DropPosition.Inside:
                            return;
                            break;
                        case DropPosition.Before:
                            newPos = rs2.SetOrder;
                            break;
                        case DropPosition.After:
                            newPos = rs2.SetOrder + 1;
                            break;
                        default:
                            break;
                    }
                    if (newPos > oldPos)
                    {
                        newPos--;
                    }
                    string errorMsg = rsl.MoveReviewSet(rs, newPos);
                    if (errorMsg != "")
                    {
                        MessageBox.Show(errorMsg);
                    }
                    else
                    {
                        DataPortal<ReviewSetMoveCommand> dp = new DataPortal<ReviewSetMoveCommand>();
                        ReviewSetMoveCommand command = new ReviewSetMoveCommand(
                            rs.ReviewSetId,
                            oldPos,
                            newPos
                            );
                        dp.ExecuteCompleted += (o, e2) =>
                        {
                            if (e2.Error != null)
                                MessageBox.Show("Sorry - the move command failed on the server.\nPlease go to the 'my info tab' and reload your review");

                            BusyLoading.IsRunning = false;
                            TreeView.IsEnabled = true;
                        };
                        TreeView.IsEnabled = false;
                        BusyLoading.IsRunning = true;
                        dp.BeginExecute(command);
                    }
                    return; // don't want to get into the rest of the proc
                }

                return; // possibly someone dragging an item from a report back onto the codes tree!
            }
            
            if (draggedAttribute == null)
            {
                return;
            }
            AttributeSet targetAttribute = options.DropTargetItem.DataContext as AttributeSet;
            //ReviewSet draggedReviewSet = e.DraggedItems[0] as ReviewSet;
            ReviewSet targetReviewSet = options.DropTargetItem.DataContext as ReviewSet;
            AttributeSetList listTo = null;
            AttributeSetList listFrom = null;
            int position = 0;
            ReviewSet activeReviewSet = rsl.GetReviewSet(draggedAttribute.SetId);

            if (activeReviewSet != null)
            {
                if (activeReviewSet.AllowCodingEdits == false)
                {
                    MessageBox.Show("You have not enabled editing on this code set.");
                    return;
                }
                else if (targetAttribute != null && targetAttribute.MaxDepth != 0)
                {
                    if ((options.DropPosition == DropPosition.After || options.DropPosition == DropPosition.Before) && targetAttribute.MaxDepth < targetAttribute.AttributeTreeLevel + draggedAttribute.ChildrenDepth )
                    //levels depth: ChildrenDepth does not iclude the conating attr and in this case the targetAttr.AttributeTreeLevel is 1 level more than where we are moving, so its: 
                    // targetAttr.AttributeTreeLevel - 1 (we are pasting on the parent of this!) + 1 (for the draggedAttribute) + draggedAttribute.ChildrenDepth = targetAttr.AttributeTreeLevel + draggedAttribute.ChildrenDepth
                    {

                        RadWindow.Alert("New position would make this tree too deep." + Environment.NewLine
                                           + "Max depth for this set is: " + targetAttribute.MaxDepth.ToString());
                        return;
                    }
                    else if ((options.DropPosition == DropPosition.Inside) && targetAttribute.MaxDepth < targetAttribute.AttributeTreeLevel + draggedAttribute.ChildrenDepth + 1)
                    {

                        RadWindow.Alert("New position would make this tree too deep." + Environment.NewLine
                                           + "Max depth for this set is: " + targetAttribute.MaxDepth.ToString());
                        return;
                    }

                }
            }

            if (draggedAttribute == null)
            {
                MessageBox.Show("Sorry, can't see what was dragged");
                return;
            }

            if (draggedAttribute.HostAttribute == null)
            {
                listFrom = rsl.GetReviewSet(draggedAttribute.SetId).Attributes;
            }
            else
            {
                listFrom = draggedAttribute.HostAttribute.Attributes;
            }

            if (listFrom == null)
            {
                MessageBox.Show("Sorry, could not find host list. (a)");
                return;
            }

            if ((targetAttribute != null) && (draggedAttribute.SetId != targetAttribute.SetId))
            {
                MessageBox.Show("Sorry, you can't move codes between code sets.");
                return;
            }

            if ((targetReviewSet != null) && (draggedAttribute.SetId != targetReviewSet.SetId))
            {
                MessageBox.Show("Sorry, you can't move codes between code sets.");
                return;
            }

            // attribute to reviewset (inside)
            // attribute to attribute (inside)
            // attribute to attribute (before / after)
            // attribute to reviewset (via drag to attribute before / after)


            switch (options.DropPosition)
            {
                case DropPosition.Inside:
                    if (targetAttribute != null)
                    {
                        listTo = targetAttribute.Attributes;
                        position = targetAttribute.Attributes.Count;
                    }
                    else
                    {
                        if (targetReviewSet != null)
                        {
                            listTo = targetReviewSet.Attributes;
                            position = listTo.Count;
                        }
                        else
                        {
                            MessageBox.Show("Can't find destination");
                            return;
                        }
                    }
                    break;

                case DropPosition.After:
                    if (targetAttribute.HostAttribute != null)
                    {
                        listTo = targetAttribute.HostAttribute.Attributes;
                        position = listTo.IndexOf(targetAttribute) + 1;
                        targetAttribute = targetAttribute.HostAttribute; // **********
                    }
                    else
                    {
                        listTo = rsl.GetReviewSet(targetAttribute.SetId).Attributes;
                        position = listTo.IndexOf(targetAttribute) + 1;
                        targetAttribute = null;
                        if (listTo == null)
                        {
                            MessageBox.Show("Sorry, can't find the root node");
                            return;
                        }
                    }
                    if ((listTo == listFrom) && (position > listTo.IndexOf(draggedAttribute)))
                    {
                        position--;
                    }
                    break;

                case DropPosition.Before:
                    if (targetAttribute.HostAttribute != null)
                    {
                        listTo = targetAttribute.HostAttribute.Attributes;
                        position = listTo.IndexOf(targetAttribute);
                        targetAttribute = targetAttribute.HostAttribute; // *** important
                    }
                    else
                    {
                        listTo = rsl.GetReviewSet(targetAttribute.SetId).Attributes;
                        position = listTo.IndexOf(targetAttribute);
                        targetAttribute = null;
                        if (listTo == null)
                        {
                            MessageBox.Show("Sorry, can't find the root node");
                            return;
                        }
                    }
                    if ((listTo == listFrom) && (position > listTo.IndexOf(draggedAttribute)))
                    {
                        position--;
                    }
                    break;

                default:
                    break;
            }
            Int64 From = draggedAttribute.ParentAttributeId; // because DoMoveBetweenLists changes this!
            if (DoMoveBetweenLists(listFrom, listTo, targetAttribute, draggedAttribute, position) == true)
            {
                DoMove(From,
                    targetAttribute != null ? targetAttribute.AttributeId : 0,
                    draggedAttribute.AttributeSetId,
                    position);
            }

        }
        private void TreeView_PreviewDragEnded(object sender, RadTreeViewDragEndedEventArgs e)
        {
            e.Handled = true;

            if (e.TargetDropItem != null && e.TargetDropItem.ParentTreeView != TreeView)
            {
                return;
            }

            if (e.TargetDropItem == null)
            {
                // MessageBox.Show("Sorry, you cannot drag to the root position"); can't show this message, as drag drop is also into report
                return;
            }

            ReviewSetsList rsl = TreeView.ItemsSource as ReviewSetsList;
            ReviewSet rs = e.DraggedItems[0] as ReviewSet;
            if (rs != null)
            {
                ReviewSet rs2 = e.TargetDropItem.DataContext as ReviewSet;
                if (rs != null && rs2 != null)
                {
                    int newPos = 0;
                    int oldPos = rs.SetOrder;
                    switch (e.DropPosition)
                    {
                        case DropPosition.Inside:
                            return;
                            break;
                        case DropPosition.Before:
                            newPos = rs2.SetOrder;
                            break;
                        case DropPosition.After:
                            newPos = rs2.SetOrder + 1;
                            break;
                        default:
                            break;
                    }
                    if (newPos > oldPos)
                    {
                        newPos--;
                    }
                    string errorMsg = rsl.MoveReviewSet(rs, newPos);
                    if (errorMsg != "")
                    {
                        MessageBox.Show(errorMsg);
                    }
                    else
                    {
                        DataPortal<ReviewSetMoveCommand> dp = new DataPortal<ReviewSetMoveCommand>();
                        ReviewSetMoveCommand command = new ReviewSetMoveCommand(
                            rs.ReviewSetId,
                            oldPos,
                            newPos
                            );
                        dp.ExecuteCompleted += (o, e2) =>
                        {
                            if (e2.Error != null)
                                MessageBox.Show("Sorry - the move command failed on the server.\nPlease go to the 'my info tab' and reload your review");

                            BusyLoading.IsRunning = false;
                            TreeView.IsEnabled = true;
                        };
                        TreeView.IsEnabled = false;
                        BusyLoading.IsRunning = true;
                        dp.BeginExecute(command);
                    }
                    return; // don't want to get into the rest of the proc
                }

                return; // possibly someone dragging an item from a report back onto the codes tree!
            }
            AttributeSet draggedAttribute = e.DraggedItems[0] as AttributeSet;
            if (draggedAttribute == null)
            {
                return;
            }
            AttributeSet targetAttribute = e.TargetDropItem.DataContext as AttributeSet;
            //ReviewSet draggedReviewSet = e.DraggedItems[0] as ReviewSet;
            ReviewSet targetReviewSet = e.TargetDropItem.DataContext as ReviewSet;
            AttributeSetList listTo = null;
            AttributeSetList listFrom = null;
            int position = 0;
            ReviewSet activeReviewSet = rsl.GetReviewSet(draggedAttribute.SetId);

            if (activeReviewSet != null)
            {
                if (activeReviewSet.AllowCodingEdits == false)
                {
                    MessageBox.Show("You have not enabled editing on this code set.");
                    return;
                }
                else if (targetAttribute != null && targetAttribute.MaxDepth != 0)
                {
                    if ((e.DropPosition == DropPosition.After || e.DropPosition == DropPosition.Before) && targetAttribute.MaxDepth < targetAttribute.AttributeTreeLevel + draggedAttribute.ChildrenDepth - 1)
                    {

                        RadWindow.Alert("New position would make this tree too deep." + Environment.NewLine
                                           + "Max depth for this set is: " + targetAttribute.MaxDepth.ToString());
                        return;
                    }
                    else if ((e.DropPosition == DropPosition.Inside) && targetAttribute.MaxDepth < targetAttribute.AttributeTreeLevel + draggedAttribute.ChildrenDepth + 1)
                    {

                        RadWindow.Alert("New position would make this tree too deep." + Environment.NewLine
                                           + "Max depth for this set is: " + targetAttribute.MaxDepth.ToString());
                        return;
                    }

                }
            }

            if (draggedAttribute == null)
            {
                MessageBox.Show("Sorry, can't see what was dragged");
                return;
            }

            if (draggedAttribute.HostAttribute == null)
            {
                listFrom = rsl.GetReviewSet(draggedAttribute.SetId).Attributes;
            }
            else
            {
                listFrom = draggedAttribute.HostAttribute.Attributes;
            }

            if (listFrom == null)
            {
                MessageBox.Show("Sorry, could not find host list. (a)");
                return;
            }

            if ((targetAttribute != null) && (draggedAttribute.SetId != targetAttribute.SetId))
            {
                MessageBox.Show("Sorry, you can't move codes between code sets.");
                return;
            }

            if ((targetReviewSet != null) && (draggedAttribute.SetId != targetReviewSet.SetId))
            {
                MessageBox.Show("Sorry, you can't move codes between code sets.");
                return;
            }

            // attribute to reviewset (inside)
            // attribute to attribute (inside)
            // attribute to attribute (before / after)
            // attribute to reviewset (via drag to attribute before / after)


            switch (e.DropPosition)
            {
                case DropPosition.Inside:
                    if (targetAttribute != null)
                    {
                        listTo = targetAttribute.Attributes;
                        position = targetAttribute.Attributes.Count;
                    }
                    else
                    {
                        if (targetReviewSet != null)
                        {
                            listTo = targetReviewSet.Attributes;
                            position = listTo.Count;
                        }
                        else
                        {
                            MessageBox.Show("Can't find destination");
                            return;
                        }
                    }
                    break;

                case DropPosition.After:
                    if (targetAttribute.HostAttribute != null)
                    {
                        listTo = targetAttribute.HostAttribute.Attributes;
                        position = listTo.IndexOf(targetAttribute) + 1;
                        targetAttribute = targetAttribute.HostAttribute; // **********
                    }
                    else
                    {
                        listTo = rsl.GetReviewSet(targetAttribute.SetId).Attributes;
                        position = listTo.IndexOf(targetAttribute) + 1;
                        targetAttribute = null;
                        if (listTo == null)
                        {
                            MessageBox.Show("Sorry, can't find the root node");
                            return;
                        }
                    }
                    if ((listTo == listFrom) && (position > listTo.IndexOf(draggedAttribute)))
                    {
                        position--;
                    }
                    break;

                case DropPosition.Before:
                    if (targetAttribute.HostAttribute != null)
                    {
                        listTo = targetAttribute.HostAttribute.Attributes;
                        position = listTo.IndexOf(targetAttribute);
                        targetAttribute = targetAttribute.HostAttribute; // *** important
                    }
                    else
                    {
                        listTo = rsl.GetReviewSet(targetAttribute.SetId).Attributes;
                        position = listTo.IndexOf(targetAttribute);
                        targetAttribute = null;
                        if (listTo == null)
                        {
                            MessageBox.Show("Sorry, can't find the root node");
                            return;
                        }
                    }
                    if ((listTo == listFrom) && (position > listTo.IndexOf(draggedAttribute)))
                    {
                        position--;
                    }
                    break;

                default:
                    break;
            }
            Int64 From = draggedAttribute.ParentAttributeId; // because DoMoveBetweenLists changes this!
            if (DoMoveBetweenLists(listFrom, listTo, targetAttribute, draggedAttribute, position) == true)
            {
                DoMove(From,
                    targetAttribute != null ? targetAttribute.AttributeId : 0,
                    draggedAttribute.AttributeSetId,
                    position);
            }
        }

        private bool DoMoveBetweenLists(AttributeSetList fromList, AttributeSetList toList, AttributeSet toAttribute, AttributeSet attributeSet, int position)
        {
            bool retVal = true;
            string returnString = "";
            if (fromList == toList)
            {
                returnString = fromList.MoveWithinList(attributeSet, position);
            }
            else
            {
                returnString = fromList.RemoveAttribute(attributeSet);
                if (returnString == "")
                {
                    returnString = toList.AddAttribute(attributeSet, toAttribute, position);
                }
            }
            if (returnString != "")
            {
                MessageBox.Show(returnString + Environment.NewLine + "This change will not be saved to the database.");
                retVal = false;
            }
            return retVal;
        }

        private void DoMove(Int64 fromAttributeSetId, Int64 toAttributeSetId, Int64 AttributeSetId, int attributeOrder)
        {
            DataPortal<AttributeMoveCommand> dp = new DataPortal<AttributeMoveCommand>();
            AttributeMoveCommand command = new AttributeMoveCommand(
                fromAttributeSetId,
                toAttributeSetId,
                AttributeSetId,
                attributeOrder);
            dp.ExecuteCompleted += (o, e2) =>
            {
                if (e2.Error != null)
                    MessageBox.Show(e2.Error.Message);

                BusyLoading.IsRunning = false;
                TreeView.IsEnabled = true;
            };
            TreeView.IsEnabled = false;
            BusyLoading.IsRunning = true;
            dp.BeginExecute(command);
        }

        private void CheckDeleteAttributeSet()
        {
            AttributeSet attributeSet = TreeView.SelectedItem as AttributeSet;
            if (attributeSet != null)
            {
                windowCheckAttributeDelete.TextBlockCheckDeleteCode.Text = "Are you sure you want to delete '" + attributeSet.AttributeName + "'?";
                windowCheckAttributeDelete.Header = "Delete code?";
                windowCheckAttributeDelete.TextBlockCheckDeleteCodeDetails.Text = "Loading...";
                DataPortal<AttributeSetDeleteWarningCommand> dp = new DataPortal<AttributeSetDeleteWarningCommand>();
                AttributeSetDeleteWarningCommand command = new AttributeSetDeleteWarningCommand(
                    attributeSet.AttributeSetId, 0);
                dp.ExecuteCompleted += (o, e2) =>
                {
                    windowCheckAttributeDelete.BusyCheckAttributeDelete.IsRunning = false;
                    if (e2.Error != null)
                    {
                        MessageBox.Show(e2.Error.Message);
                    }
                    else
                    {
                        windowCheckAttributeDelete.TextBlockCheckDeleteCodeDetails.Text = "This will directly affect codes you have assigned to " + e2.Object.NumItems.ToString() + " items";
                        if ((attributeSet.Attributes != null) && (attributeSet.Attributes.Count > 0))
                        {
                            windowCheckAttributeDelete.TextBlockCheckDeleteCodeDetails.Text += " plus any codes associated with its 'child' codes.";
                        }
                        else
                        {
                            windowCheckAttributeDelete.TextBlockCheckDeleteCodeDetails.Text += ".";
                            windowCheckAttributeDelete.TextBlockCheckDeleteCodeDetails.Text.Replace("directly ", "");
                        }
                    }
                };
                windowCheckAttributeDelete.BusyCheckAttributeDelete.IsRunning = true;
                dp.BeginExecute(command);
                windowCheckAttributeDelete.ShowDialog();
            }
            else
            {
                CheckDeleteReviewSet();
            }
        }

        private void CheckDeleteReviewSet()
        {
            ReviewSet reviewSet = TreeView.SelectedItem as ReviewSet;
            if (reviewSet != null)
            {
                windowCheckAttributeDelete.Header = "Delete code set?";
                windowCheckAttributeDelete.TextBlockCheckDeleteCode.Text = "Are you sure you want to delete '" + reviewSet.SetName + "' and all its codes?";
                windowCheckAttributeDelete.TextBlockCheckDeleteCodeDetails.Text = "Loading...";
                DataPortal<AttributeSetDeleteWarningCommand> dp = new DataPortal<AttributeSetDeleteWarningCommand>();
                AttributeSetDeleteWarningCommand command = new AttributeSetDeleteWarningCommand(
                    0, reviewSet.SetId);
                dp.ExecuteCompleted += (o, e2) =>
                {
                    windowCheckAttributeDelete.BusyCheckAttributeDelete.IsRunning = false;
                    if (e2.Error != null)
                    {
                        MessageBox.Show(e2.Error.Message);
                    }
                    else
                    {
                        windowCheckAttributeDelete.TextBlockCheckDeleteCodeDetails.Text = "This will affect codes you have assigned to " + e2.Object.NumItems.ToString() + " items.";
                    }
                };
                windowCheckAttributeDelete.BusyCheckAttributeDelete.IsRunning = true;
                dp.BeginExecute(command);
                windowCheckAttributeDelete.ShowDialog();
            }
        }

        private void cmdDeleteCode_Click(object sender, RoutedEventArgs e)
        {
            ReviewSetsList rsl = TreeView.ItemsSource as ReviewSetsList; AttributeSetList hostList = null;
            AttributeSet attributeSet = TreeView.SelectedItem as AttributeSet;
            if (attributeSet != null)
            {
                if (attributeSet.HostAttribute != null)
                {
                    hostList = attributeSet.HostAttribute.Attributes;
                    //attributeSet.HostAttribute.DeleteChildAttribute(attributeSet);
                }
                else
                {
                    if (rsl != null)
                    {
                        ReviewSet reviewSet = rsl.GetReviewSet(attributeSet.SetId);
                        if (reviewSet != null)
                        {
                            //reviewSet.DeleteChildAttribute(attributeSet);
                            hostList = reviewSet.Attributes;
                        }
                    }
                }
                DataPortal<AttributeSetDeleteCommand> dp = new DataPortal<AttributeSetDeleteCommand>();
                AttributeSetDeleteCommand command = new AttributeSetDeleteCommand(attributeSet.AttributeSetId,
                    attributeSet.ParentAttributeId,
                    attributeSet.AttributeId,
                    attributeSet.AttributeOrder);
                dp.ExecuteCompleted += (o, e2) =>
                {
                    if (e2.Error != null)
                    {
                        MessageBox.Show(e2.Error.Message);
                    }
                    else
                    {
                        if (e2.Object.Successful != true)
                        {
                            MessageBox.Show("Sorry, server could not delete the code.");
                        }
                        else
                        {
                            string retString = hostList.RemoveAttribute(attributeSet);
                            if (retString == "")
                            {
                                // no need to say anything - the code is gone
                            }
                            else
                            {
                                MessageBox.Show("Sorry. The server deleted the code, but it's still visible here. Please go to the 'my info' tab and reload your review.");
                            }
                        }
                    }
                    windowCheckAttributeDelete.cmdDeleteCode.IsEnabled = true;
                    windowCheckAttributeDelete.cmdCancelDeleteCode.IsEnabled = true;
                    windowCheckAttributeDelete.busyIndicatorDelete.IsBusy = false;
                    windowCheckAttributeDelete.Close();
                };
                windowCheckAttributeDelete.cmdDeleteCode.IsEnabled = false;
                windowCheckAttributeDelete.cmdCancelDeleteCode.IsEnabled = false;
                windowCheckAttributeDelete.busyIndicatorDelete.IsBusy = true;
                dp.BeginExecute(command);
            }
            else
            {
                ReviewSet reviewSet = TreeView.SelectedItem as ReviewSet;
                if (reviewSet != null)
                {
                    //ReviewSetsList setsList = TreeView.ItemsSource as ReviewSetsList;
                    //setsList.Remove(reviewSet);
                    //hostList = reviewSet.Attributes;
                    DataPortal<ReviewSetDeleteCommand> dp = new DataPortal<ReviewSetDeleteCommand>();
                    ReviewSetDeleteCommand command = new ReviewSetDeleteCommand(reviewSet.ReviewSetId,
                        reviewSet.SetId, reviewSet.SetOrder);
                    dp.ExecuteCompleted += (o, e2) =>
                    {
                        if (e2.Error != null)
                        {
                            MessageBox.Show(e2.Error.Message);
                        }
                        else
                        {
                            if (e2.Object.Successful != true)
                            {
                                MessageBox.Show("Sorry, server could not delete the code.");
                            }
                            else
                            {
                                string retString = rsl.RemoveReviewSet(reviewSet);
                                if (retString == "")
                                {
                                    // no need to say anything - the code is gone
                                }
                                else
                                {
                                    MessageBox.Show(retString);
                                }
                            }
                        }
                        windowCheckAttributeDelete.cmdDeleteCode.IsEnabled = true;
                        windowCheckAttributeDelete.cmdCancelDeleteCode.IsEnabled = true;
                        windowCheckAttributeDelete.busyIndicatorDelete.IsBusy = false;
                        windowCheckAttributeDelete.Close();
                    };
                    windowCheckAttributeDelete.cmdDeleteCode.IsEnabled = false;
                    windowCheckAttributeDelete.cmdCancelDeleteCode.IsEnabled = false;
                    windowCheckAttributeDelete.busyIndicatorDelete.IsBusy = true;
                    dp.BeginExecute(command);
                }
            }
            
        }

        private void cmdCancelDeleteCode_Click(object sender, RoutedEventArgs e)
        {
            windowCheckAttributeDelete.Close();
        }

        private void cmdEditOutcomes_Click(object sender, RoutedEventArgs e)
        {
            Button bt = sender as Button;
            currentAttributeSet = bt.DataContext as AttributeSet;

            if ((currentAttributeSet != null) && (currentAttributeSet.ItemData != null))
            {
                GridEditOutcomes.DataContext = currentAttributeSet;
                windowEditOutcomes.ShowDialog();
                dialogEditOutcomesControl.RefreshDataProvider();
            }   
            else
            {
                MessageBox.Show("No data associated with this code set for this item yet");
            }
        }

        private void TreeView_SelectionChanged(object sender, Telerik.Windows.Controls.SelectionChangedEventArgs e)
        {
            if ((TreeView.SelectedItem != null))   
            {
                if (TreeView.SelectedItem is AttributeSet)
                {
                    TextBoxGuidance.Text = (TreeView.SelectedItem as AttributeSet).AttributeSetDescription;
                }
                else if (TreeView.SelectedItem is ReviewSet)
                {
                    TextBoxGuidance.Text = (TreeView.SelectedItem as ReviewSet).SetDescription;
                }
            }
            else
            {
                TextBoxGuidance.Text = "";
            }

            SetHotKeys();
            
            if (SelectedItemChanged != null)
            {
                this.SelectedItemChanged.Invoke(this, EventArgs.Empty);
            }
        }

        AttributeSetList CurrentHotKeysList = null;
        private void ClearCurrentHotKeysList()
        {
            if (CurrentHotKeysList != null)
            {
                foreach (AttributeSet attribute in CurrentHotKeysList)
                {
                    attribute.CurrentHotKeyText = "";
                }
            }
            CurrentHotKeysList = null;
        }

        private void SetHotKeys()
        {
            ClearCurrentHotKeysList();
            if (CheckBoxHotkeys.IsChecked == true)
            {
                CurrentHotKeysList = GetSelectedList();
                if ((CurrentHotKeysList != null) && (CurrentHotKeysList.Count > 0))
                {
                    int i = 1;
                    foreach (AttributeSet attribute in CurrentHotKeysList)
                    {
                        if (i <= 9)
                        {
                            attribute.CurrentHotKeyText = i.ToString();
                        }
                        else
                        {
                            if (i == 10)
                                attribute.CurrentHotKeyText = "0";
                        }
                        i++;
                    }
                }

            }
        }

        private void contextMenu_Opened(object sender, RoutedEventArgs e)
        {
            RadTreeViewItem treeViewItem = (sender as RadContextMenu).GetClickedElement<RadTreeViewItem>();
            if (treeViewItem == null)
            {
                (sender as RadContextMenu).IsOpen = false;
                return;
            }
            if (!treeViewItem.IsSelected)
            {
                TreeView.SelectedItems.Clear();
                TreeView.SelectedItems.Add(treeViewItem.Item);
            }
            ReviewSet rs = TreeView.SelectedItem as ReviewSet;
            if (contextMenuReviewSetCopy != null)
            {//can't paste codes if you don't have write access, or in coding only so why copy?
                //can't copy whole code-sets (rs != null)
                if (!ri.HasWriteRights() || ControlContext == "CodingOnly" || rs != null) contextMenuReviewSetCopy.IsEnabled = false;
                else contextMenuReviewSetCopy.IsEnabled = true;
            }
            // Setting the menu for attributes (anything but the root)
            if (rs == null)
            {
                contextMenuCodeSelected.Visibility = CurrentTextDocument != null && ShowCodeTxtOptions ? Visibility.Visible : Visibility.Collapsed;
                contextMenuUncodeSelected.Visibility = CurrentTextDocument != null && ShowCodeTxtOptions ? Visibility.Visible : Visibility.Collapsed;
                contextMenuShowText.Visibility = CurrentTextDocument != null ? Visibility.Visible : Visibility.Collapsed;
                contextMenuReport.Visibility = Visibility.Visible;
                if (contextMenuAddChild != null) contextMenuAddChild.Visibility = Visibility.Visible;
                if (contextMenuNewCodeSet != null) contextMenuNewCodeSet.Visibility = Visibility.Collapsed;
                if (contextMenuDeleteCode != null) contextMenuDeleteCode.Visibility = Visibility.Visible;
                if (contextMenuDeleteCodeSet != null) contextMenuDeleteCodeSet.Visibility = Visibility.Collapsed;
                contextMenuReviewSetPrint.Visibility = Visibility.Collapsed;
                //if (contextMenuAxialCoding != null)
                //    contextMenuAxialCoding.Visibility = Visibility.Visible;
                if (contextMenuInsertInDiagram != null)
                {
                    if (ShowDiagramMenuOptions == true)
                        contextMenuInsertInDiagram.Visibility = Visibility.Visible;
                    else
                        contextMenuInsertInDiagram.Visibility = Visibility.Collapsed;
                }
                if (contextMenuInsertChildCodesInDiagram != null)
                {
                    if (ShowDiagramMenuOptions == true)
                        contextMenuInsertChildCodesInDiagram.Visibility = Visibility.Visible;
                    else
                        contextMenuInsertChildCodesInDiagram.Visibility = Visibility.Collapsed;
                }
                //if (contextMenuInsertInReport != null)
                //{
                //    if (ShowReportMenuOptions != "")
                //        contextMenuInsertInReport.Visibility = Visibility.Visible;
                //    else
                //        contextMenuInsertInReport.Visibility = Visibility.Collapsed;
                //}
                if (contextMenuAssignSearchItems != null)
                {
                    if (ShowSearchMenuOptions == true)
                        contextMenuAssignSearchItems.Visibility = Visibility.Visible;
                    else
                        contextMenuAssignSearchItems.Visibility = Visibility.Collapsed;
                }
                if (contextMenuRemoveSearchItems != null)
                {
                    if (ShowSearchMenuOptions == true)
                        contextMenuRemoveSearchItems.Visibility = Visibility.Visible;
                    else
                        contextMenuRemoveSearchItems.Visibility = Visibility.Collapsed;
                }
                AttributeSet attributeSet = TreeView.SelectedItem as AttributeSet;
                if (attributeSet != null)
                {
                    ReviewSetsList rsl = TreeView.ItemsSource as ReviewSetsList;
                    ReviewSet reviewSet = rsl.GetReviewSet(attributeSet.SetId);
                    if (reviewSet != null)
                    {
                        if (reviewSet.AllowCodingEdits == true && attributeSet.CanHaveChildren)
                        {
                            contextMenuAddChild.IsEnabled = true;
                            contextMenuDeleteCode.IsEnabled = true;
                            if (copiedAttributeSet != 0)
                                contextMenuReviewSetPaste.IsEnabled = true;
                        }
                        else
                        {
                            contextMenuAddChild.IsEnabled = false;
                            contextMenuDeleteCode.IsEnabled = reviewSet.AllowCodingEdits;
                            contextMenuReviewSetPaste.IsEnabled = false;
                        }
                    }
                }
                if (ControlContext == "homeDocuments")
                {
                    if (contextMenuListItems != null)
                    {
                        contextMenuListItems.Visibility = Visibility.Visible;
                    }
                    if (contextMenuListItemsExcluded != null)
                    {
                        contextMenuListItemsExcluded.Visibility = Visibility.Visible;
                    }
                    if (contextMenuListItemsWithout != null)
                    {
                        contextMenuListItemsWithout.Visibility = Visibility.Visible;
                    }
                    if (contextMenuListItemsWithoutExcluded != null)
                    {
                        contextMenuListItemsWithoutExcluded.Visibility = Visibility.Visible;
                    }
                    if (contextMenuAssignItems != null)
                    {
                        if (ShowSearchMenuOptions != true)
                            contextMenuAssignItems.Visibility = Visibility.Visible;
                        else
                            contextMenuAssignItems.Visibility = Visibility.Collapsed;
                    }
                    if (contextMenuRemoveItems != null)
                    {
                        if (ShowSearchMenuOptions != true)
                            contextMenuRemoveItems.Visibility = Visibility.Visible;
                        else
                            contextMenuRemoveItems.Visibility = Visibility.Collapsed;
                    }
                }
            }
            else
                // setting the menu for review sets (the root nodes)
            {
                if (contextMenuInsertInDiagram != null)
                {
                    contextMenuInsertInDiagram.Visibility = Visibility.Collapsed;
                }
                if (contextMenuInsertChildCodesInDiagram != null)
                {
                    if (ShowDiagramMenuOptions == true)
                        contextMenuInsertChildCodesInDiagram.Visibility = Visibility.Visible;
                    else
                        contextMenuInsertChildCodesInDiagram.Visibility = Visibility.Collapsed;
                }
                //if (contextMenuInsertInReport != null)
                //{
                //    if (ShowReportMenuOptions == "Question") // Question = a 'parent' style report
                //        contextMenuInsertInReport.Visibility = Visibility.Visible;
                //    else
                //        contextMenuInsertInReport.Visibility = Visibility.Collapsed;
                //}
                contextMenuCodeSelected.Visibility = Visibility.Collapsed;
                contextMenuUncodeSelected.Visibility = Visibility.Collapsed;
                contextMenuShowText.Visibility = Visibility.Collapsed;
                contextMenuReport.Visibility = Visibility.Collapsed;
                contextMenuAddChild.Visibility = Visibility.Visible;
                contextMenuNewCodeSet.Visibility = Visibility.Visible;
                contextMenuDeleteCode.Visibility = Visibility.Collapsed;
                contextMenuDeleteCodeSet.Visibility = Visibility.Visible;
                contextMenuReviewSetPrint.Visibility = Visibility.Visible;
                //if (contextMenuAxialCoding != null)
                //    contextMenuAxialCoding.Visibility = Visibility.Collapsed;
                if (rs.AllowCodingEdits == true)
                {
                    contextMenuAddChild.IsEnabled = true;
                    contextMenuDeleteCodeSet.IsEnabled = true;
                    if (copiedAttributeSet != 0)
                        contextMenuReviewSetPaste.IsEnabled = true;
                }
                else
                {
                    contextMenuAddChild.IsEnabled = false;
                    contextMenuDeleteCodeSet.IsEnabled = false;
                    contextMenuReviewSetPaste.IsEnabled = false;
                }
                if (ControlContext == "homeDocuments")
                {
                    if (contextMenuListItems != null)
                    {
                        contextMenuListItems.Visibility = Visibility.Collapsed;
                    }
                    if (contextMenuListItemsExcluded != null)
                    {
                        contextMenuListItemsExcluded.Visibility = Visibility.Collapsed;
                    }
                    if (contextMenuAssignItems != null)
                    {
                        contextMenuAssignItems.Visibility = Visibility.Collapsed;
                    }
                    if (contextMenuRemoveItems != null)
                    {
                        contextMenuRemoveItems.Visibility = Visibility.Collapsed;
                    }
                    if (contextMenuListItemsWithout != null)
                    {
                        contextMenuListItemsWithout.Visibility = Visibility.Collapsed;
                    }
                    if (contextMenuListItemsWithoutExcluded != null)
                    {
                        contextMenuListItemsWithoutExcluded.Visibility = Visibility.Collapsed;
                    }
                }
            }
        }

        private void cmdSaveCodeSetSettings_Click(object sender, RoutedEventArgs e)
        {
            ReviewSet reviewSet = windowItemSetProperties.GridItemSetProperties.DataContext as ReviewSet;
            if (reviewSet != null)
            {
                DataPortal<ItemSetCompleteCommand> dp = new DataPortal<ItemSetCompleteCommand>();
                ItemSetCompleteCommand command = new ItemSetCompleteCommand(
                    reviewSet.ItemSetId, reviewSet.ItemSetIsCompleted, reviewSet.ItemSetIsLocked);
                dp.ExecuteCompleted += (o, e2) =>
                {
                    if (e2.Error != null)
                    {
                        MessageBox.Show(e2.Error.Message);
                    }
                    else
                    {
                        if (e2.Object.Successful != true)
                        {
                            MessageBox.Show("Error: could not set as complete. Maybe this item has already had its coding complete with this set?");
                        }
                        reviewSet.SetIsClean();
                        reviewSet.ApplyEdit();
                        BindItem(this.DataContext as Item);
                        windowItemSetProperties.Close();
                    }
                };
                dp.BeginExecute(command);
            }
            else
            {
                MessageBox.Show("Error: could not find data");
            }
        }

        private void cmdCreateNewCodeSet_Click(object sender, RoutedEventArgs e)
        {
            ReviewSet rs = windowNewCodeSet.GridEditOrCreateCodeSet.DataContext as ReviewSet;
            if ((windowNewCodeSet.TextBoxNewCodeSetName.Text != "") && (rs != null))
            {
                SaveNewCodeSet(rs);
            }
            else
            {
                MessageBox.Show("Please enter a name for your code set");
            }
        }

        private void cmdSaveEditCodeSet_Click(object sender, RoutedEventArgs e)
        {
            ReviewSet rs = windowEditCodeSet.GridEditCodeSet.DataContext as ReviewSet;
            if ((windowEditCodeSet.TextBoxEditCodeSetName.Text != "") && (rs != null))
            {
                UpdateCodeset(rs);
            }
            else
            {
                MessageBox.Show("Please enter a name for your code set");
            }
        }

        private void SaveNewCodeSet(ReviewSet rs)
        {
            rs.Saved += (o, e2) =>
            {
                if (e2.NewObject != null)
                {
                    if (rs.SetId == 0)
                    {
                        (TreeView.ItemsSource as ReviewSetsList).Add(e2.NewObject as ReviewSet);
                    }
                }
                else
                    if (e2.Error != null)
                        MessageBox.Show(e2.Error.Message);
                windowNewCodeSet.BusyCreateNewCodeSet.IsRunning = false;
                windowNewCodeSet.cmdCreateNewCodeSet.IsEnabled = true;
                windowNewCodeSet.Close();
            };
            windowNewCodeSet.BusyCreateNewCodeSet.IsRunning = true;
            windowNewCodeSet.cmdCreateNewCodeSet.IsEnabled = false;
            rs.BeginSave();
        }

        private void UpdateCodeset(ReviewSet rs)
        {//!Csla.Rules.BusinessRules.HasPermission( AuthorizationActions.EditObject, this);
            //if (!Csla.Security.AuthorizationRules.CanEditObject(rs.GetType()))
            if (!Csla.Rules.BusinessRules.HasPermission(AuthorizationActions.EditObject, rs))
                return;
            DataPortal<ReviewSetUpdateCommand> dp = new DataPortal<ReviewSetUpdateCommand>();
            ReviewSetUpdateCommand command = new ReviewSetUpdateCommand(
                rs.ReviewSetId,
                rs.SetId,
                rs.AllowCodingEdits,
                rs.CodingIsFinal,
                rs.SetName,
                rs.SetOrder,
                rs.SetDescription
                );
            dp.ExecuteCompleted += (o, e2) =>
            {
                if (e2.Error != null)
                    MessageBox.Show(e2.Error.Message);

                windowEditCodeSet.BusyEditCodeSet.IsRunning = false;
                TreeView.IsEnabled = true;
                rs.SetIsClean();
                rs.ApplyEdit();
                windowEditCodeSet.Close();
            };
            TreeView.IsEnabled = false;
            windowEditCodeSet.BusyEditCodeSet.IsRunning = true;
            dp.BeginExecute(command);
        }

        private void cmdSaveCodeSettings_Click(object sender, RoutedEventArgs e)
        {
            AttributeSet attributeSet = windowCodeProperties.GridCodeProperties.DataContext as AttributeSet;
            ReviewSetsList rsl = TreeView.ItemsSource as ReviewSetsList;
            if ((windowCodeProperties.TextBoxCodeName.Text != "") && (attributeSet != null))
            {
                Csla.NameValueListBase<int, string>.NameValuePair t = (Csla.NameValueListBase<int, string>.NameValuePair)windowCodeProperties.editCodeType.SelectedItem;
                
                attributeSet.AttributeTypeId =t.Key ;
                if (attributeSet.AttributeId == 0)
                {
                    SaveNewAttribute(attributeSet, TreeView.SelectedItem as AttributeSet);
                }
                else
                {
                    UpdateAttribute(attributeSet);
                }
            }
            else
            {
                MessageBox.Show("Please enter a title for your code");
            }
        }

        private void SaveNewAttribute(AttributeSet attributeSet, AttributeSet addTo)
        {
            attributeSet.Saved += (o, e2) =>
            {
                if (e2.Error != null)
                {
                    MessageBox.Show(e2.Error.Message);
                }
                else
                {
                    AttributeSetList attributesToAddTo = null;
                    AttributeSet atSet = TreeView.SelectedItem as AttributeSet;
                    if (atSet != null)
                    {
                        attributesToAddTo = atSet.Attributes;
                    }
                    else
                    {
                        ReviewSet reviewSet = TreeView.SelectedItem as ReviewSet;
                        if (reviewSet != null)
                        {
                            attributesToAddTo = reviewSet.Attributes;
                        }
                    }
                    if (attributesToAddTo != null)
                    {
                        attributesToAddTo.Add(e2.NewObject as AttributeSet);
                        if (atSet != null)
                        {
                            (e2.NewObject as AttributeSet).HostAttribute = atSet;
                        }
                    }
                    else
                    {
                        MessageBox.Show("Error: no attribute set to add this new attribute to");
                    }

                    //rsl.LoadingAttributes = false;
                    windowCodeProperties.cmdSaveCodeSettings.IsEnabled = true;
                    windowCodeProperties.Close();
                }
            };
            windowCodeProperties.cmdSaveCodeSettings.IsEnabled = false;
            //rsl.LoadingAttributes = true;
            attributeSet.ApplyEdit();
            attributeSet.BeginSave();
        }

        private void UpdateAttribute(AttributeSet attributeSet)
        {
            DataPortal<AttributeUpdateCommand> dp = new DataPortal<AttributeUpdateCommand>();
            AttributeUpdateCommand command = new AttributeUpdateCommand(
                attributeSet.AttributeId,
                attributeSet.AttributeSetId,
                attributeSet.AttributeTypeId,
                attributeSet.AttributeName,
                attributeSet.AttributeSetDescription,
                attributeSet.AttributeOrder
                );
            dp.ExecuteCompleted += (o, e2) =>
            {
                if (e2.Error != null)
                    MessageBox.Show(e2.Error.Message);

                BusyLoading.IsRunning = false;
                TreeView.IsEnabled = true;
                attributeSet.SetIsClean();
                attributeSet.ApplyEdit();
                windowCodeProperties.Close();
            };
            TreeView.IsEnabled = false;
            BusyLoading.IsRunning = true;
            dp.BeginExecute(command);
        }

        private void cmdNewCodeSet_Click(object sender, RoutedEventArgs e)
        {
            CreateReviewSet();
        }

        private void cmdCodeProperties_Click(object sender, RoutedEventArgs e)
        {
            DoCodeProperties();
        }

        private void cmdCancelCodeSetSettings_Click(object sender, RoutedEventArgs e)
        {
            ReviewSetsList rsl = TreeView.ItemsSource as ReviewSetsList;
            ReviewSet reviewSet = windowItemSetProperties.GridItemSetProperties.DataContext as ReviewSet;
            if (reviewSet != null)
            {
                rsl.LoadingAttributes = true;
                reviewSet.CancelEdit();
                rsl.LoadingAttributes = false;
                windowItemSetProperties.Close();
            }
        }

        private void cmdCancelCodeSettings_Click(object sender, RoutedEventArgs e)
        {
            AttributeSet attributeSet = windowCodeProperties.GridCodeProperties.DataContext as AttributeSet;
            if (attributeSet != null)
            {
                attributeSet.CancelEdit();
                windowCodeProperties.Close();
            }
        }

        private void cb_Click(object sender, RoutedEventArgs e)
        {
            ReviewSetsList reviewSets = TreeView.ItemsSource as ReviewSetsList;
            ReviewInfo rInfo = ((App)(Application.Current)).GetReviewInfo();
            CheckBox cb = sender as CheckBox;
            if ((cb != null) && (cb.IsEnabled == true))
            {
                AttributeSet iad = cb.DataContext as AttributeSet;
                if (iad == null) return;
                TreeView.SelectedItem = iad;
                if ((!reviewSets.LoadingAttributes) && (cb.IsChecked == true))
                {
                    
                    ReviewSet revS = reviewSets.GetReviewSet(iad.SetId);
                    ItemAttributeData itemData = new ItemAttributeData();
                    if (revS.ItemSetId == 0)
                    {
                        itemData.ItemSetId = 0;
                        MakeBusy();//this is to prevent people from adding another ItemCode to a set that is about to receive and ItemSetID
                    }
                    else
                    {//item already belongs to this set
                        itemData.ItemSetId = revS.ItemSetId;
                    }
                    itemData.ItemAttributeId = 0;
                    itemData.ItemId = (DataContext as Item).ItemId;
                    
                    itemData.SetId = iad.SetId;
                    itemData.AttributeId = iad.AttributeId;
                    itemData.AdditionalText = "";
                    iad.ItemData = itemData;
                    
                    DataPortal<ItemAttributeSaveCommand> dp = new DataPortal<ItemAttributeSaveCommand>();
                    ItemAttributeSaveCommand command = new ItemAttributeSaveCommand("Insert",
                        itemData.AttributeId,
                        itemData.ItemSetId,
                        itemData.AdditionalText,
                        itemData.AttributeId,
                        itemData.SetId,
                        itemData.ItemId,
                        rInfo);
                    dp.ExecuteCompleted += (o, e2) =>
                    {
                        
                        BusyLoading.IsRunning = false;
                        if (e2.Error != null)
                        {
                            //MessageBox.Show(e2.Error.Message);
                            RadWindow.Alert("Warning: there was a problem saving your data." + Environment.NewLine + "Please close this window and try again.");
                            //MessageBox.Show("Warning: there was a problem saving your data. Please close this window and try again.");
                        }
                        else
                        {
                            if (TreeView != null) TreeView.IsEnabled = true;
                            if ((RequestItemAdvance != null) && (CheckBoxAutoAdvance.IsChecked == true))
                            {
                                ShowReminder(iad.AttributeName);
                                RequestItemAdvance.Invoke(sender, e);
                            }
                            else if (itemData.ItemSetId == 0)
                            {//item didn't belong to this set, reload all sets, as this will make sure the current set gets the latest item_set_id
                                 //will become: get the new reviewSet and load just that
                                 //LoadItemAttributes(itemData.ItemId);
                                if (itemData.ItemId == (DataContext as Item).ItemId)
                                {//if this isn't true, it's because the UI has already moved to another item
                                    LoadItemAttributeSet(iad.ItemData.SetId, iad.ItemData.ItemId);
                                }
                            }
                            else
                            {// just fill the data for this attribute

                                itemData.ItemAttributeId = e2.Object.ItemAttributeId;
                                itemData.ItemSetId = e2.Object.ItemSetId;
                                iad.IsSelected = true;
                                iad.ItemData = itemData;
                            }
                        }
                    };
                    BusyLoading.IsRunning = true;
                    dp.BeginExecute(command);
                }
                else
                {
                    if ((!reviewSets.LoadingAttributes) && (cb.IsChecked == false))
                    {
                        if (MessageBox.Show("Are you sure you want to unselect this code? You will also delete any additional text you've entered, as well as any full-text coding and outcome data associated with this code.", "Delete code?", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                        {
                            ReviewSet revS = reviewSets.GetReviewSet(iad.SetId);
                            DataPortal<ItemAttributeSaveCommand> dp = new DataPortal<ItemAttributeSaveCommand>();
                            ItemAttributeSaveCommand command = new ItemAttributeSaveCommand("Delete",
                                iad.ItemData.ItemAttributeId,
                                iad.ItemData.ItemSetId,
                                iad.ItemData.AdditionalText,
                                iad.ItemData.AttributeId,
                                iad.ItemData.SetId,
                                iad.ItemData.ItemId,
                                rInfo);
                            dp.ExecuteCompleted += (o, e2) =>
                            {
                                BusyLoading.IsRunning = false;
                                if (e2.Error != null)
                                {
                                    RadWindow.Alert("Warning: there was a problem saving your data." + Environment.NewLine + "Please close this window and try again.");
                                }
                                else
                                {
                                    if (revS.ItemAttributesCount() < 2)
                                    {//reload from server
                                        //!!NOTE: this isn't ideal as all info is already on client, and could be used to rebuild the correct Item-set relationship.
                                        //!! however, going back to the server ensures that all info is correct so maybe a good idea anyway
                                        //get the new reviewSet and load just that
                                        LoadItemAttributeSet(iad.ItemData.SetId, iad.ItemData.ItemId);
                                    }
                                    //LoadItemAttributes(iad.ItemData.ItemId);
                                    iad.ItemData = null;
                                    iad.IsSelected = false;
                                    this.SelectedItemChanged.Invoke(sender, e);//this is needed to notify the pdf viewer: coding in PDF may have been deleted.
                                }
                            };
                            BusyLoading.IsRunning = true;
                            dp.BeginExecute(command);
                        }
                        else
                        {
                            cb.IsChecked = true;
                        }
                    }
                }
            }
        }
        private void LoadItemAttributeSet(int SetID, Int64 ItemID)
        {
            DataPortal<ItemSet> dp = new DataPortal<ItemSet>();
            dp.FetchCompleted += (o, e2) =>
            {
                if (e2.Object != null)
                {
                    ReviewSetsList reviewSets = TreeView.ItemsSource as ReviewSetsList;
                    //ReviewSet rs = reviewSets.GetReviewSet(SetID);
                    reviewSets.LoadingAttributes = true;
                    reviewSets.SetItemSetData(e2.Object);
                    reviewSets.LoadingAttributes = false;
                    BusyLoading.IsRunning = false;
                    //BusyLoadingAllAttributes.IsRunning = false;
                    //DoLiveComparisons();
                    TreeView.IsEnabled = true;
                }
            };
            BusyLoading.IsRunning = true;
            //BusyLoadingAllAttributes.IsRunning = true;
            TreeView.IsEnabled = false;
            dp.BeginFetch(new ItemSetSelectionCriteria(SetID, ItemID));
        }

        //void provider_DataChanged(object sender, EventArgs e)
        //{
        //    BindItem(DataContext as Item);
        //    BusyLoading.IsRunning = false;
        //    CslaDataProvider provider = (App.Current.Resources["CodeSetsData"] as CslaDataProvider);
        //    provider.DataChanged -= provider_DataChanged;
        //}

        private void Image_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DialogParameters dp = new DialogParameters();
            tempRS = (sender as Image).DataContext as ReviewSet;
            if (tempRS.ItemAttributesCount() == 0)
            {//can't complete an item that has no codes in this set!
                RadWindow.Alert("This item does not have any codes in this set."
                                + Environment.NewLine + "In order to complete the coding, some coding"
                                + Environment.NewLine + "information needs to be present.");
            }
            else
            {
                dp.Header = "Complete coding?";
                dp.Content = "Mark coding as complete?" + Environment.NewLine + "Please note: the coding of the person ‘completing’ this"
                    + Environment.NewLine + "item becomes the completed / agreed version.";
                dp.DialogStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
                dp.Header = "Mark coding as complete?";
                dp.Closed = this.CheckCompletionWClosed;
                RadWindow.Confirm("Mark coding as complete?" + Environment.NewLine + "Please note: the coding of the person ‘completing’ this"
                    + Environment.NewLine + "item becomes the completed / agreed version.", this.CheckCompletionWClosed);
            }
            e.Handled = true;
        }

        private void CheckCompletionWClosed(object sender, WindowClosedEventArgs e)
        {
            //if (MessageBox.Show("Mark coding as complete?", "Complete coding", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            if (e.DialogResult == true)
            {
                ReviewSet reviewSet = tempRS;
                if (reviewSet != null)
                {
                    DataPortal<ItemSetCompleteCommand> dp = new DataPortal<ItemSetCompleteCommand>();
                    ItemSetCompleteCommand command = new ItemSetCompleteCommand(
                        reviewSet.ItemSetId, true, reviewSet.ItemSetIsLocked);
                    dp.ExecuteCompleted += (o, e2) =>
                    {
                        if (e2.Error != null)
                        {
                            RadWindow.Alert(e2.Error.Message);
                        }
                        else
                        {
                            if (e2.Object.Successful != true)
                            {
                                RadWindow.Alert("Error: could not set as complete. Maybe this item has already had its coding complete with this set?");
                            }
                            reviewSet.ItemSetIsCompleted = true;
                            //RadTreeViewItem tvi = TreeView.ItemContainerGenerator.ContainerFromItem(reviewSet) as RadTreeViewItem;
                            //if (tvi != null)
                            //{
                            //    tvi.IsExpanded = true;
                            //}
                        }
                    };
                    dp.BeginExecute(command);
                }
                else
                {
                    RadWindow.Alert("Error: could not find data");
                    //MessageBox.Show("Error: could not find data");
                }
            }
            tempRS = null;
        }

        private void PrintSelectedReviewSet()
        {

            if (WindowPrintCodesetOptions == null)
            {
                WindowPrintCodesetOptions = new RadWPrintCodesetOptions();
                WindowPrintCodesetOptions.RestrictedAreaMargin = new Thickness(20);
                WindowPrintCodesetOptions.cmdButton_OK_Clicked += WindowPrintCodesetOptions_cmdButton_OK_Clicked;

            }
            WindowPrintCodesetOptions.ShowDialog();
        }

        private void WindowPrintCodesetOptions_cmdButton_OK_Clicked(object sender, RoutedEventArgs e)
        {
            if (WindowPrintCodesetOptions == null) return;
            WindowPrintCodesetOptions.Close();
            PrintSelectedReviewSet(WindowPrintCodesetOptions.cbShowIDs.IsChecked == true, 
                WindowPrintCodesetOptions.cbShowDescriptions.IsChecked == true,
                WindowPrintCodesetOptions.cbShowtypes.IsChecked == true);
        }

        private void PrintSelectedReviewSet(bool showIDs, bool showDescriptions, bool ShowTypes)
        {
            
            ReviewSet reviewSet = TreeView.SelectedItem as ReviewSet;
            if (reviewSet == null)
            {
                AttributeSet attributeSet1 = TreeView.SelectedItem as AttributeSet;
                if (attributeSet1 != null)
                {
                    ReviewSetsList setsList = TreeView.ItemsSource as ReviewSetsList;
                    reviewSet = setsList.GetReviewSet(attributeSet1.SetId);
                }
            }
            if (reviewSet != null)
            {
                string report = "<html><body><p><h1>" + reviewSet.SetName;
                if (showIDs) report += " (" + reviewSet.SetId + ")";
                if (ShowTypes) report += " [" + reviewSet.SetType + "]";
                report += "</h1>";
                if (showDescriptions && reviewSet.SetDescription.Trim().Length > 0)
                {
                    string desc = reviewSet.SetDescription.Replace(Environment.NewLine, "<br>");
                    desc = desc.Replace("\r\n", "<br>");
                    desc = desc.Replace("\n", "<br>");
                    desc = desc.Replace("\r", "<br>");
                    report += "<i>" + desc + " </i>";
                }
                report += "<p><ul>";
                foreach (AttributeSet attributeSet in reviewSet.Attributes)
                {
                    report = PrintSelectedReviewSetAddAttributes(report, attributeSet, showIDs, showDescriptions, ShowTypes);
                }
                report += "</ul></p></body></html>";
                //System.Windows.Browser.HtmlPage.Window.Invoke("ShowPopup", report);
                reportViewerControl.SetContent(report);
                windowReports.ShowDialog();
            }
            else
            {
                MessageBox.Show("Error: no code set selected");
            }
        }

        private string PrintSelectedReviewSetAddAttributes(string report, AttributeSet attributeSet, bool showIDs, bool showDescriptions, bool ShowTypes)
        {
            string desc = attributeSet.AttributeSetDescription.Replace(Environment.NewLine, "<br>");
            desc = desc.Replace("\r\n", "<br>");
            desc = desc.Replace("\n", "<br>");
            desc = desc.Replace("\r", "<br>");
            report += "<li>" + attributeSet.AttributeName;
            if (showIDs) report += " (ID = " + attributeSet.AttributeId + ")";
            if (ShowTypes) report += " [" + attributeSet.AttributeType + "]";
            if (showDescriptions && desc.Trim().Length > 0) report += "<br><i>" + desc + " </i>";
            
            if (attributeSet.Attributes != null && attributeSet.Attributes.Count > 0)
            {
                report += "<ul>";
                foreach (AttributeSet child in attributeSet.Attributes)
                {
                    report = PrintSelectedReviewSetAddAttributes(report, child, showIDs, showDescriptions, ShowTypes);
                }
                report += "</ul>";
            }
            report += "</li>";
            return report;
        }

        public void doCodeKeyDown(int index)
        {
            if (CheckBoxHotkeys.IsChecked == true)
            {
                TreeView.SelectionChanged -= TreeView_SelectionChanged;
                object o = TreeView.SelectedItem;
                AttributeSetList asl = GetSelectedList();
                if ((asl != null) && (asl.Count > index))
                {
                    AttributeSet attribute = asl[index];
                    if ((attribute != null) && (attribute.AttributeTypeId > 1))
                    {
                        RadTreeViewItem tvi = TreeView.ContainerFromItemRecursive(attribute) as RadTreeViewItem;
                        if (tvi != null)
                        {
                            CheckBox cb = tvi.FindChildByType<CheckBox>();
                            if (attribute.IsSelected == true)
                            {
                                cb.IsChecked = false;
                            }
                            else
                            {
                                cb.IsChecked = true;
                            }
                            cb_Click(cb, new RoutedEventArgs());
                        }
                    }
                }
                TreeView.SelectedItem = o;
                TreeView.SelectionChanged += new Telerik.Windows.Controls.SelectionChangedEventHandler(TreeView_SelectionChanged);
            }
        }

        private AttributeSetList GetSelectedList()
        {
            
            AttributeSet parent = TreeView.SelectedItem as AttributeSet;
            if (parent != null)
                return parent.Attributes;
            ReviewSet rsParent = TreeView.SelectedItem as ReviewSet;
            if (rsParent != null)
                return rsParent.Attributes;
            return null;
        }

        private void CheckBoxHotkeys_Click(object sender, RoutedEventArgs e)
        {
            SetHotKeys();
        }

        private void cmdCancelNewCodeset_Click(object sender, RoutedEventArgs e)
        {
            (windowNewCodeSet.GridEditOrCreateCodeSet.DataContext as ReviewSet).CancelEdit();
            windowNewCodeSet.Close();
        }

        private void cmdCancelEditCodeset_Click(object sender, RoutedEventArgs e)
        {
            (windowEditCodeSet.GridEditCodeSet.DataContext as ReviewSet).CancelEdit();
            windowEditCodeSet.Close();
        }

        private void cmdPrintCodeSet_Click(object sender, RoutedEventArgs e)
        {
            PrintSelectedReviewSet();
        }

        private void cmdExpandAll_Click(object sender, RoutedEventArgs e)
        {
            TreeView.ExpandAll();
        }

        public void ExpandByID(int SetId)
        {
            if (TreeView == null || TreeView.Items == null) return;
            ItemCollection ic = TreeView.Items;
            SolidColorBrush bl = new SolidColorBrush(Color.FromArgb(255, 187, 221, 255));
            SolidColorBrush wh = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
            foreach (ReviewSet rs in ic)
            {
                RadTreeViewItem rtvi = TreeView.ItemContainerGenerator.ContainerFromItem(rs) as RadTreeViewItem;   
                if (rs.SetId == SetId)
                {
                    if (rtvi == null) //tree only creates the SL containers when they are shown, so if this is the first time the control is loaded, rtvi will not exist
                    {
                        TreeView.Loaded += new RoutedEventHandler(TreeView_Loaded);
                        ToDoSetID = SetId;
                    }
                    else
                    {
                        
                        rtvi.BringIntoView();
                        TreeView.ExpandItemByPath(rtvi.FullPath, "|");
                        rtvi.Background = bl;
                    }
                }
                else
                {
                    if (rtvi != null) rtvi.Background = wh;
                }
            }
        }
        
        void TreeView_Loaded(object sender, RoutedEventArgs e)
        {
            ExpandByID(ToDoSetID);
            TreeView.Loaded -= TreeView_Loaded;
        }

        public void SetReportAppearance(bool forReport)
        {
            if (TreeView != null && TreeView.ItemsSource != null)
            {
                ReviewSetsList reviewSets = TreeView.ItemsSource as ReviewSetsList;
                foreach (ReviewSet rs in reviewSets)
                {
                    if (rs.Attributes.Count > 0)
                    {
                        if (ShowReportMenuOptions == "Question" && forReport == true)
                            rs.DisplayIsParent = forReport;
                        else
                            rs.DisplayIsParent = false;
                        _setReportAppearance(forReport, rs.Attributes);
                    }
                }
            }
        }

        private void _setReportAppearance(bool forReport, AttributeSetList asl)
        {
            foreach (AttributeSet attribute in asl)
            {
                if (forReport == false)
                {
                    attribute.DisplayIsParent = false;
                }
                if (ShowReportMenuOptions == "Question")
                {
                    if (attribute.Attributes.Count > 0)
                        attribute.DisplayIsParent = forReport;
                    else
                        attribute.DisplayIsParent = false;
                }
                else
                {
                    if (ShowReportMenuOptions != "")
                        attribute.DisplayIsParent = forReport;
                }
                _setReportAppearance(forReport, attribute.Attributes);
            }
        }

        //private void TextBoxCodeName_KeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.Key.ToString() == "Enter")
        //    {
        //        cmdSaveCodeSettings.Focus();
        //        //RoutedEventArgs rea = new RoutedEventArgs();
        //        //cmdSaveCodeSettings_Click(sender, rea);
        //    }
        //}

        //private void TextBoxNewCodeSetName_KeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.Key.ToString() == "Enter")
        //    {
        //        cmdCreateNewCodeSet.Focus();
        //        //RoutedEventArgs rea = new RoutedEventArgs();
        //        //cmdCreateNewCodeSet_Click(sender, rea);
        //    }
        //}

        private void HyperLinkChangeMethodToSingle_Click(object sender, RoutedEventArgs e)
        {
            ReviewSet rs = windowEditCodeSet.GridEditCodeSet.DataContext as ReviewSet;
            DataPortal<ReviewSetCheckCodingStatusCommand> dp = new DataPortal<ReviewSetCheckCodingStatusCommand>();
            ReviewSetCheckCodingStatusCommand command = new ReviewSetCheckCodingStatusCommand(rs.SetId);
            dp.ExecuteCompleted += (o, e2) =>
            {
                windowChangeMethodToSingle.BusyChangeMethodToSingle.IsRunning = false;
                if (e2.Error != null)
                {
                    MessageBox.Show(e2.Error.Message);
                }
                else
                {
                    if (e2.Object.ProblematicItemCount == 0)
                    {
                        windowChangeMethodToSingle.TextBlockChangeMethodToSingleComment.Text = "You are about to change your data entry method to 'Normal'. \nThere are no potential data conflicts so it is safe to proceed.";
                        windowChangeMethodToSingle.HyperLinkCancelChangeMethodToSingle.Content = "Cancel this change";
                        windowChangeMethodToSingle.HyperLinkDoChangeMethodToSingle.Content = "Continue: change to Normal data entry";
                    }
                    else
                    {
                        windowChangeMethodToSingle.TextBlockChangeMethodToSingleComment.Text = "You are about to change your data entry method to 'Normal', " + Environment.NewLine
                        +"but there are ‘" +e2.Object.ProblematicItemCount.ToString() +
                        "’ items that should be completed before you proceed." +Environment.NewLine
                        + "You can view these incomplete items from the ‘Review statistics’ tab on the right." +Environment.NewLine;
                        windowChangeMethodToSingle.HyperLinkCancelChangeMethodToSingle.Content = "Cancel: I'll complete the coding for these items first";
                        windowChangeMethodToSingle.HyperLinkDoChangeMethodToSingle.Content = "Carry on: Even if there are uncompleted unreconciled disagreements I know what I’m doing!";
                    }
                    
                }
            };
            windowChangeMethodToSingle.BusyChangeMethodToSingle.IsRunning = true;
            dp.BeginExecute(command);
            windowChangeMethodToSingle.ShowDialog();
        }

        private void HyperLinkChangeMethodToMultiple_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to change to 'Comparison' data entry?" + Environment.NewLine
                +"This implies that you will have multiple users coding the same item using this codeset and then reconciling the disagreements."+ Environment.NewLine
                +"Please ensure you have read the manual to check the implications of this."+ Environment.NewLine
                , "Change to 'Comparison' data entry?", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                ReviewSet rs = windowEditCodeSet.GridEditCodeSet.DataContext as ReviewSet;
                if (rs != null)
                {
                    rs.CodingIsFinal = false;
                    windowEditCodeSet.TextBlockEditCodeSetMethodSingle.Visibility = System.Windows.Visibility.Collapsed;
                    windowEditCodeSet.HyperLinkChangeMethodToMultiple.Visibility = System.Windows.Visibility.Collapsed;
                    windowEditCodeSet.TextBlockEditCodeSetMethodMultiple.Visibility = System.Windows.Visibility.Visible;
                    windowEditCodeSet.HyperLinkChangeMethodToSingle.Visibility = System.Windows.Visibility.Collapsed;
                }
                else
                {
                    MessageBox.Show("Error: could not find review set. Please reload your review from the 'My Info' tab.");
                }
            }
        }

        private void HyperLinkCancelChangeMethodToSingle_Click(object sender, RoutedEventArgs e)
        {
            windowChangeMethodToSingle.Close();
        }

        private void HyperLinkDoChangeMethodToSingle_Click(object sender, RoutedEventArgs e)
        {
            windowChangeMethodToSingle.Close();
            ReviewSet rs = windowEditCodeSet.GridEditCodeSet.DataContext as ReviewSet;
            if (rs != null)
            {
                rs.CodingIsFinal = true;
                windowEditCodeSet.TextBlockEditCodeSetMethodSingle.Visibility = System.Windows.Visibility.Visible;
                windowEditCodeSet.HyperLinkChangeMethodToMultiple.Visibility = System.Windows.Visibility.Collapsed;
                windowEditCodeSet.TextBlockEditCodeSetMethodMultiple.Visibility = System.Windows.Visibility.Collapsed;
                windowEditCodeSet.HyperLinkChangeMethodToSingle.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                MessageBox.Show("Error: could not find review set. Please reload your review from the 'My Info' tab.");
            }
        }
        public void ShowReminder(string AttrName)
        {
            tBlockPreviousCodeReminder.Text = "Last code added:" + Environment.NewLine + AttrName;
            brPreviousCodeReminder.Visibility = System.Windows.Visibility.Visible;
            if (myDispatcherTimer == null)
            {
                myDispatcherTimer = new System.Windows.Threading.DispatcherTimer();
                myDispatcherTimer.Interval = new TimeSpan(0, 0, 0, 3, 0); // 3 seconds
            }
            else myDispatcherTimer.Stop();
            
            myDispatcherTimer.Tick += new EventHandler(Each_Tick);
            myDispatcherTimer.Start();
        }
        // Fires every myDispatcherTimer.Interval while the DispatcherTimer is active.
        public void Each_Tick(object o, EventArgs sender)
        {
            brPreviousCodeReminder.Visibility = System.Windows.Visibility.Collapsed;
            myDispatcherTimer.Tick -= Each_Tick;
            myDispatcherTimer.Stop();
        }
        private class NewLinesHelper
        {//this is used to recalculate indexes when the DB text uses "\n" only.
            //most ER4 docs follow this convention, but text within the RadRichText component is tranformed so that it uses "\r\n" no matter what.
            //as a result we need to compensate!
            //singleNewLines stores the index positions of each single char new line found and is all we need to convert in both directions.
            //actually the first implemntation only needs db to viewer conversions, but I'm keeping the reverse method just in case we'll find we need it after all
            //this is implemented in the UI code as it's UI-specific: it's based on the assumption that \n is generally present on DB, while \r\n is ALWAYS used in the viewer
            //one instance of this class is generated when the active document is set within the codestree object.
            private List<int> singleNewLines =  new List<int>();
            //private List<int> doubleNewLines = new List<int>();
            public NewLinesHelper(string Text)
            {
                Regex re1 = new Regex("(?<!\r)\n");//see http://www.regular-expressions.info/lookaround.html this matches \n when it's not preceded by \r
                Regex re2 = new Regex("\r\n");
                MatchCollection ma1 = re1.Matches(Text);
                //MatchCollection ma2 = re2.Matches(Text);
                foreach (Match ma in ma1)
                {
                    singleNewLines.Add(ma.Index);
                }
                //foreach (Match ma in ma2)
                //{
                //    doubleNewLines.Add(ma.Index);
                //}
            }
            public int FromDBToViewerIndex(int Index)
            {//will add 1 to Index for each single NL that precedes it
                int i = 0, corrector = 0;
                while (i < singleNewLines.Count && singleNewLines[i] < Index)
                {//not acting on Index directly as we don't want to slip past the index of the last newlines!
                    i++;
                    corrector++;
                }
                return Index + corrector;
            }
            public int FromViewerToDBIndex(int Index)
            {//will subtract 1 to Index for each single NL that precedes it
                //acts on Index directly because the initial value is bigger than the wanted result so it may point to
                //a location (on the original txt) that is one or more lines below the needed value
                //by decreasing Index directly we move it towards the right final position within the while circle
                int i = 0;
                while (i < singleNewLines.Count && singleNewLines[i] < Index)
                {
                    i++;
                    Index--;
                }
                return Index;
            }
        }

        private void cmdShowClassificationWindow_Click(object sender, RoutedEventArgs e)
        {
            windowClassifier.ShowDialog();
        }

        private void cmdConfigureCodesets_Click(object sender, RoutedEventArgs e)
        {
            ShowWizard();
        }
        public void ShowWizard()
        {
            if (windowWizard == null)
            {
                windowWizard =  new RadWReviewWizard();
                windowWizard.RestrictedAreaMargin = new Thickness(20); 
                windowWizard.Header = "Setup CodeSets Wizard";
                windowWizard.IsRestricted = true;
                windowWizard.WindowStateChanged += new EventHandler(Helpers.WindowHelper.MaxOnly_WindowStateChanged);
                windowWizard.Style = Application.Current.Resources["CustomRadWindowStyle"] as Style;
                windowWizard.WindowState = WindowState.Maximized;
                windowWizard.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
                windowWizard.CanClose = true;
            }
            if (HasWriteRights) windowWizard.ShowDialog();
        }
        public void UnHookMe()
        {
            CslaDataProvider provider = (App.Current.Resources["CodeSetsData"] as CslaDataProvider);
            if (provider != null)
            {
                provider.DataChanged -= CodeSetsProvider_DataChanged;
                provider.DataChanged -= CodeSetsProvider_DataChanged;
            }
        }
    } // END MAIN CodesTreeControl CLASS
    public class AttributeSetToPaste : IComparable
    {
        public int SetID;
        public Int64 AttributeID;
        Int64 ParentID;
        public AttributeSet Original;
        public int RelativeLevel = 0;

        public AttributeSetToPaste(AttributeSet original, int setID, int relativeLevel)
        {
            Original = original;
            SetID = setID;
            AttributeID = 0;
            ParentID = 0;
            RelativeLevel = relativeLevel;
        }
        public int CompareTo(object o)
        {
            AttributeSetToPaste obj = o as AttributeSetToPaste;
            if (RelativeLevel < obj.RelativeLevel)
            {
                return -1;
            }
            else if (RelativeLevel > obj.RelativeLevel)
            {
                return 1;
            }
            else if (Original.ParentAttributeId == obj.Original.ParentAttributeId)
            {
                if (Original.AttributeOrder < obj.Original.AttributeOrder) return -1;
                else if (Original.AttributeOrder == obj.Original.AttributeOrder) return 0;
                else return 1;
            }
            else
            {
                return 0;
            }
        }
        
    }
    public class AttributeSetToPasteList : List<AttributeSetToPaste>
    {
        int NextIndex = 0;
        public AttributeSetToPasteList()
        {
        }
        //public override void Add(AttributeSetToPaste obj)
        //{
        //    base.Add(obj);
            
        //    NextIndex = 0;
        //}
        //public override void (AttributeSetToPaste obj)
        //{
        //    base.Add(obj);
        //    NextIndex = 0;
        //}
        //maybe get next? so to find the next item that does not have a valid ID?
        public AttributeSetToPaste GetNext()
        {
            NextIndex++;
            if (this.Count <= NextIndex)
            {
                NextIndex = 0;
                return null;
            }
            else
            {
                while (this[NextIndex].AttributeID != 0 && this.Count < NextIndex)
                {
                    NextIndex++;
                }
                if (this.Count < NextIndex)
                {
                    NextIndex = 0;
                    return null;
                }
                else
                return this[NextIndex];
            }
        }
        public AttributeSetToPaste GetCurrent()
        {
            if (this.Count < NextIndex)
            {
                NextIndex = 0;
                return null;
            }
            else
            {
                return this[NextIndex];
            }
        }
        public Int64 getParentID(AttributeSetToPaste child)
        {
            if (child.Original.ParentAttributeId == 0) return 0;
            foreach (AttributeSetToPaste el in this)
            {
                if (el.Original.AttributeId == child.Original.ParentAttributeId)
                {
                    if (el.AttributeID == 0) return -1;
                    return el.AttributeID;
                }
            }
            return -1;
        }
    }
    [ContentProperty("DataTemplate")]
    public class TypedTemplate
    {
        public String TypeName { get; set; }
        public DataTemplate DataTemplate { get; set; }
    }

    [ContentProperty("TypedTemplates")]
    public class AttributeCodingTemplateSelector : Telerik.Windows.Controls.DataTemplateSelector
    {
        public AttributeCodingTemplateSelector()
        {
            TypedTemplates = new List<TypedTemplate>();
        }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is ReviewSet)
            {
                foreach (var contentItem in TypedTemplates)
                {
                    var typedTemplate = contentItem as TypedTemplate;
                    if (typedTemplate.TypeName == "CodeSetTemplate")
                    {
                        return typedTemplate.DataTemplate;
                    }
                }
            }
            else if (item is AttributeSet)
            {
                int AttributeTypeId = (item as AttributeSet).AttributeTypeId;
                bool canEdit = false;

                ReviewSetsList reviewSets = (App.Current.Resources["CodeSetsData"] as CslaDataProvider).Data as ReviewSetsList;
                if (reviewSets != null)
                {
                    ReviewSet rs = reviewSets.GetReviewSet((item as AttributeSet).SetId);
                    if (rs != null)
                    {
                        canEdit = !rs.ItemSetIsLocked;
                    }
                }

                foreach (var contentItem in TypedTemplates)
                {
                    var typedTemplate = contentItem as TypedTemplate;
                    if (typedTemplate.TypeName == "SelectableAttributeTemplate")
                    {
                        return typedTemplate.DataTemplate;
                    }

                    /*
                    if ((typedTemplate.TypeName == "SelectableAttributeTemplate") && (AttributeTypeId > 1))
                    {
                        return typedTemplate.DataTemplate;
                    }
                    else
                    {
                        if ((typedTemplate.TypeName == "NonSelectableAttributeTemplate") && (AttributeTypeId == 1))
                        {
                            return typedTemplate.DataTemplate;
                        }
                    }
                    */
                }
            }
            return null;
        }

        public IList TypedTemplates
        {
            private set;
            get;
        }
    }

    [ContentProperty("DataTemplate")]
    public class ReadOnlyTypedTemplate
    {
        public String TypeName { get; set; }
        public DataTemplate DataTemplate { get; set; }
    }

    [ContentProperty("ReadOnlyTypedTemplates")]
    public class ReadOnlyTemplateSelector : Telerik.Windows.Controls.DataTemplateSelector
    {
        public ReadOnlyTemplateSelector()
        {
            ReadOnlyTypedTemplates = new List<ReadOnlyTypedTemplate>();
        }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is ReviewSet)
            {
                foreach (var contentItem in ReadOnlyTypedTemplates)
                {
                    var typedTemplate = contentItem as ReadOnlyTypedTemplate;
                    if (typedTemplate.TypeName == "CodeSetTemplate")
                    {
                        return typedTemplate.DataTemplate;
                    }
                }
            }
            else if (item is AttributeSet)
            {
                foreach (var contentItem in ReadOnlyTypedTemplates)
                {
                    var typedTemplate = contentItem as ReadOnlyTypedTemplate;

                    if (typedTemplate.TypeName == "NonSelectableAttributeTemplate") 
                    {
                        return typedTemplate.DataTemplate;
                    }
                }
            }

            return null;
        }

        public IList ReadOnlyTypedTemplates
        {
            private set;
            get;
        }
    }

    

    

    


}
