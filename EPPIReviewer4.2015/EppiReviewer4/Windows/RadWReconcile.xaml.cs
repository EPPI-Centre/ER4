using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using BusinessLibrary.BusinessClasses;


using Telerik.Windows.Controls;
using System.ComponentModel;
using Csla;
using Csla.Xaml;
using System.Diagnostics;

namespace EppiReviewer4
{
    /// <summary>
    /// The process of creating a new window works as follow:
    /// 1) add new (template) in the Windows folder
    /// 2) find the original declaration in XAML and move it into the new XAML file, save the x:Name attribute for later. See xaml template for details. Also copy or move Reference entries as described in XAML.
    /// 3) in the old code-behind file, create a private instance of the new window type:
    ///     EXAMPLE: (this is a class member, don't place it in a method or porperty; change name of class and object appropriately)
    ///     private RadWReconcile RadWExample = new RadWReconcile(); 
    ///     the name of the private object should match the original name of the window (XAML declaration: x:Name="window...")
    /// 4) find all the event handlers declared in xaml,
    ///     FOREACH event handler
    ///         4a) in the new code-behind file, create an event, be sure to use the right EventArgs (see example below)
    ///         4b) in the new code-behind file, create a handler (see example below)
    ///         4c) in the old code-behind file, constructor method, hook up the old handler to the event created in 4a), place it after the InitializeComponent() call
    ///             EXAMPLE:
    ///             Public OldPage()
    ///             {
    ///                 InitializeComponent();
    ///                 [...]
    ///                 RadWExample.cmdButton_Clicked += new EventHandler<RoutedEventArgs>(Original_Handling_Method_In_OldPage);
    ///                 [...]
    ///             }
    ///     LOOP
    /// 5) Build app: this will probably fail, generating a list of errors where code in OldPage references elements in what used to be in local XAML.
    ///     Fix all errors, this is usually done by adding a prefix to elements that are now in the new xaml file.
    ///     EXAMPLE:
    ///     SomeList.SelectedIndex >-becomes-> RadWExample.SomeList.SelectedIndex
    ///     BEWARE: this is where things may go wrong. If the same window (or elements of it) is(/are) used in more than one way, changed dynamically, recylced, or who knows what,
    ///     then the new version of the code may insert bugs. You should understand the code you're changing!!
    /// 6) If you have copied or moved Resources into the new XAML file (point 2):
    ///     FOREACH Moved Resource
    ///         6a) If the Resouce was moved or copied into the new XAML,
    ///             go to the old file code-behind and search for the moved Resources name, fix their reference:
    ///             EXAMPLE:
    ///             this.Resources["ResName"] >-becomes-> RadWExample.Resources["ResName"] 
    ///             WARNING!!:If you have copied (not Moved!!) the resource across (because it's needed in both places, but you don't want it in App.xaml), 
    ///                    !! then you should fix some references BUT not all (!!!)
    ///                    !! some code will need access to the old reference, some to the one in the new page, so pay attention!
    ///                    !! it would certainly be easier to move them onto App.xaml
    ///         6b) If you have moved Resources into App.XAML, search for the name of the moved resouce in the old file code-behind, fix the references:
    ///             EXAMPLE:
    ///         this.Resources["ResName"] >-becomes-> App.Current.Resources["ResName"]
    /// 7) test it all! And I mean test each different usage scenario.
    /// 8) END
    /// </summary>
    public partial class RadWReconcile : RadWindow //change this to give it a unque name, must inherit from Radwindow.
    {
        #region EVENTS
        //put one event for each code-behind handler declared in XAML
        //EXAMPLE: 
        //public event EventHandler<RoutedEventArgs> cmdButton_Clicked;
        //
        public event EventHandler<PageIndexChangingEventArgs> ItemsGridDataPager_PageIndexChangingEvent;
        public event EventHandler<RoutedEventArgs> HyperlinkButton_Clicked;
        #endregion

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
        private RadWReconcileHelp WindRecHelp = new RadWReconcileHelp();
        public RadWReconcile()
        {
            InitializeComponent();
            //ItemsGrid.DataContext = localList;
        }
        #region HANDLERS
        //put each XAML-declared handler in here, make it fire the corresponding event
        //EXAMPLE:
        //private void cmdButton_Click(object sender, RoutedEventArgs e)
        //{
        //    if (cmdButton_Clicked != null) cmdCluster_Clicked.Invoke(sender, e);
        //      //the if statement makes sure the event fires only if someone is listening
        //      //this is necessary as the event will be hooked up in the parent page (the page that will Show this window)
        //      //and there is no guarantee that it will be hooked up before the XAML fires the triggering event
        //}
        private void ItemsGridDataPager_PageIndexChanging(object sender, PageIndexChangingEventArgs e)
        {
            if (ItemsGridDataPager_PageIndexChangingEvent != null) ItemsGridDataPager_PageIndexChangingEvent.Invoke(sender, e);
        }
        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            if (HyperlinkButton_Clicked != null) HyperlinkButton_Clicked.Invoke(sender, e);
        }
        #endregion
        RadWComparisonReportOptions WindowComparisonRepOptions = new RadWComparisonReportOptions();
        int CurrentItem = 0;
        DataPortal<ItemSetList> dp;
        DataPortal<ItemArmList> dpItemArms = new DataPortal<ItemArmList>();
        ReconcilingItemList localList;
        public Comparison comparison;
        public ReviewSet reviewSet;
        public string ComparisonDescription = "";
        //DataPortal<ItemArmList> ArmsDataPortal = new DataPortal<ItemArmList>();
        public void StartGettingData()
        {
            items = this.DataContext as ItemList;
            ProgressReportText.Text = "Loading item 1 of " + items.Count + ".";
            IsBusy.IsRunning = true;
            ProgressReport.Visibility = Visibility.Visible;
            if (comparison == null || items == null || items.Count == 0 || reviewSet == null) return;
            CurrentItem = 0;
            localList = new ReconcilingItemList(reviewSet, comparison, ComparisonDescription);
            ItemsGridDataPager.IsEnabled = false;
            //ItemsGrid.DataContext = localList;

            //ItemsGrid.Columns["Rev1Col"].Header = localList.Comparison.ContactName1;
            //ItemsGrid.Columns["Rev2Col"].Header = localList.Comparison.ContactName2;
            //ItemsGrid.Columns["Rev3Col"].Header = localList.Comparison.ContactName3;
            ItemsGrid.Columns["Rev3Col"].IsVisible = localList.ShowReviewer3;

            dp = new DataPortal<ItemSetList>();
            dp.FetchCompleted += new EventHandler<DataPortalResult<ItemSetList>>(DataFetched);
            //dpItemArms.FetchCompleted -= DpItemArms_FetchCompleted;
            //dpItemArms.FetchCompleted += new EventHandler<DataPortalResult<ItemArmList>>(DpItemArms_FetchCompleted);
            dp.BeginFetch(new SingleCriteria<ItemSetList, Int64>(items[0].ItemId));
            this.items = (DataContext as ItemList);
            GetItemDocumentList(items[0]);
        }
        ItemList items;
        private void DpItemArms_FetchCompleted(object sender, EventArgs e)
        {
            //add itemArm data, if not finished, start fetching ItemSetData.
            CslaDataProvider provider = ((CslaDataProvider)this.Resources["ItemArmsData"]);
            if (provider.Data != null && provider.Data as ItemArmList != null) items[CurrentItem].Arms = provider.Data as ItemArmList;
            CurrentItem++;
            if (CurrentItem >= items.Count)//all done
            {
                IsBusy.IsRunning = false;
                ProgressReport.Visibility = Visibility.Collapsed;
                ItemsGrid.DataContext = localList;
                ItemsGrid.ItemsSource = localList.Items;
                localListX = localList;
                ItemsGridDataPager.IsEnabled = true;
                WindowComparisonRepOptions.DataContext = localList;
                ControlReviewer1.Tag = localList.Comparison.ContactId1;
                ControlReviewer2.Tag = localList.Comparison.ContactId2;
                ControlReviewer3.Tag = localList.Comparison.ContactId3;
                ItemsGrid.SelectedItem = ItemsGrid.Items[0];
                ItemsGrid.IsEnabled = true;
            }
            else
            {
                ProgressReportText.Text = "Loading item " + CurrentItem + " of " + items.Count + ".";
                IsBusy.IsRunning = true;
                dp.BeginFetch(new SingleCriteria<ItemSetList, Int64>(items[CurrentItem].ItemId));
            }

        }
        //private void DpItemArms_FetchCompleted(object sender, DataPortalResult<ItemArmList> e)
        //{
        //    //add itemArm data, if not finished, start fetching ItemSetData.
        //    if (e.Object != null) items[CurrentItem].Arms = e.Object;
        //    CurrentItem++;
        //    if (CurrentItem >= items.Count)//all done
        //    {
        //        IsBusy.IsRunning = true;
        //        //ProgressReport.Visibility = Visibility.Collapsed;
        //        ItemsGrid.DataContext = localList;
        //        ItemsGrid.ItemsSource = localList.Items;
        //        localListX = localList;
        //        ItemsGridDataPager.IsEnabled = true;
        //        WindowComparisonRepOptions.DataContext = localList;
        //        ControlReviewer1.Tag = localList.Comparison.ContactId1;
        //        ControlReviewer2.Tag = localList.Comparison.ContactId2;
        //        ControlReviewer3.Tag = localList.Comparison.ContactId3;
        //        ItemsGrid.SelectedItem = ItemsGrid.Items[0];
        //        ItemsGrid.IsEnabled = true;
        //    }
        //    else
        //    {
        //        ProgressReportText.Text = "Loading item " + CurrentItem + " of " + items.Count + ".";
        //        IsBusy.IsRunning = true;
        //        dp.BeginFetch(new SingleCriteria<ItemSetList, Int64>(items[CurrentItem].ItemId));
        //    }

        //}

        private void DataFetched(object o, DataPortalResult<ItemSetList> e2)
        {
            
            if (e2.Error != null)
            {
                RadWindow.Alert(e2.Error.Message);
            }
            else if (e2.Object != null)
            {
                //add ItemSet data, start fetching ItemArm data
                ItemSetList isl = e2.Object as ItemSetList;
                localList.AddItem(items[CurrentItem], isl);
                //dpItemArms.BeginFetch(new SingleCriteria<Item, Int64>(items[CurrentItem].ItemId));
                GetItemArmList(items[CurrentItem]);

            }
        }

        private void GetItemArmList(Item item)
        {
            CslaDataProvider provider = this.Resources["ItemArmsData"] as CslaDataProvider;
            provider.FactoryParameters.Clear();
            provider.FactoryParameters.Add(item.ItemId);
            provider.FactoryMethod = "GetItemArmList";
            provider.Refresh();
        }

        private void ButtonComplete_Click(object sender, RoutedEventArgs e)
        {
            Button bt = sender as Button;
            if (bt == null) return;
            ReconcilingItem data = bt.DataContext as ReconcilingItem;
            if (data == null) return;
            if (data.IsCompleted)
            {
                if ((int)bt.Tag != data.CompletedByID)
                {//dialog to confirm if this is really a good idea: we are changing the completor, will need some logic here to uncomplete the current etc
                }
                else
                {//do nothing? we are re-completing on the same person...
                }
            }
            else
            {//do complete this!
                DataPortal<ItemSetCompleteCommand> dp = new DataPortal<ItemSetCompleteCommand>();
                long isi = -1; string completor = "";
                if (comparison.ContactId1 == (int)bt.Tag)
                {
                    isi = data.ItemSetR1;
                    completor = comparison.ContactName1;
                }
                else if (comparison.ContactId2 == (int)bt.Tag)
                {
                    isi = data.ItemSetR2;
                    completor = comparison.ContactName2;
                }
                else if (comparison.ContactId3 == (int)bt.Tag)
                {
                    isi = data.ItemSetR3;
                    completor = comparison.ContactName3;
                }
                ItemSetCompleteCommand command = new ItemSetCompleteCommand(isi, true, false);
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
                            RadWindow.Alert("Error: could not set as complete." + Environment.NewLine
                            + "Please reload the comparison and/or review before retrying.");
                        }
                        data.IsCompleted = true;
                        data.CompletedByID = (int)bt.Tag;
                        data.CompletedByName = completor;
                        data.CompletedItemSetID = isi;
                    }
                };
                dp.BeginExecute(command);
            }

        }
        private void ButtonUnComplete_Click(object sender, RoutedEventArgs e)
        {
            Button bt = sender as Button;
            if (bt == null) return;
            ReconcilingItem data = bt.DataContext as ReconcilingItem;
            if (data == null) return;
            if (data.IsCompleted)
            {
                //do un-complete this!
                DataPortal<ItemSetCompleteCommand> dp = new DataPortal<ItemSetCompleteCommand>();
                long isi = -1; string completor = "";
                if (comparison.ContactId1 == (int)bt.Tag)
                {
                    isi = data.ItemSetR1;
                }
                else if (comparison.ContactId2 == (int)bt.Tag)
                {
                    isi = data.ItemSetR2;
                }
                else if (comparison.ContactId3 == (int)bt.Tag)
                {
                    isi = data.ItemSetR3;
                }
                else
                {
                    isi = data.CompletedItemSetID;
                }
                ItemSetCompleteCommand command = new ItemSetCompleteCommand(isi, false, false);
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
                            RadWindow.Alert("Error: could not set as complete." + Environment.NewLine
                            + "Please reload the comparison and/or review before retrying.");
                        }
                        data.IsCompleted = false;
                        data.CompletedByID = 0;
                        data.CompletedByName = completor;
                        data.CompletedItemSetID = 0;
                    }
                };
                dp.BeginExecute(command);
            }
        }
        private void ItemsGrid_SelectionChanged(object sender, SelectionChangeEventArgs e)
        {
            ReconcileRoot.DataContext = (ItemsGrid.SelectedItem as ReconcilingItem).item;
            GetItemDocumentList((ItemsGrid.SelectedItem as ReconcilingItem).item);
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
        private void CslaDataProvider_DataChanged(object sender, EventArgs e)
        {
            CslaDataProvider provider = ((CslaDataProvider)this.Resources["ItemDocumentListData"]);
            if (provider.Error != null)
                Telerik.Windows.Controls.RadWindow.Alert(((Csla.Xaml.CslaDataProvider)sender).Error.Message);
            if (provider.IsBusy == false)
                GridDocuments.IsEnabled = true;
            if (GridDocuments.Items.Count < 1)
                GridDocuments.Visibility = System.Windows.Visibility.Collapsed;
            else
                GridDocuments.Visibility = System.Windows.Visibility.Visible;
        }

        private void btReconcileHelp_Click(object sender, RoutedEventArgs e)
        {
            WindRecHelp.ShowDialog();
        }

        private void cmdSaveReconcTable_Click(object sender, RoutedEventArgs e)
        {
            WindowComparisonRepOptions.ShowDialog();
        }
        

    }//end of control

    //a single item: a row in the table, contains a list<ReconcilingCode> for each possible reviewer (up to 3)
    public class ReconcilingItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        private Item _item;
        public Item item
        {
            get
            {
                return _item;
            }
        }
        private bool _IsCompleted = false;
        public bool IsCompleted
        {
            get { return _IsCompleted; }
            set 
            {
                _IsCompleted = value;
                NotifyPropertyChanged("IsCompleted");
            }
        }
        private int _CompletedByID;
        public int CompletedByID
        {
            get { return _CompletedByID; }
            set
            {
                _CompletedByID = value; ;
                NotifyPropertyChanged("CompletedByID");
                NotifyPropertyChanged(null);
            }
        }
        private long _CompletedItemSetID;
        public long CompletedItemSetID
        {
            get { return _CompletedItemSetID; }
            set
            {
                _CompletedItemSetID = value; ;
                NotifyPropertyChanged("CompletedItemSetID");
                NotifyPropertyChanged(null);
            }
        }
        private string _CompletedByName;
        public string CompletedByName
        {
            get { return _CompletedByName; }
            set
            {
                _CompletedByName = value; ;
                NotifyPropertyChanged("CompletedByName");
            }
        }
        private List<ReconcilingCode> _CodesReviewer1;
        public List<ReconcilingCode> CodesReviewer1
        {
            get
            {
                return _CodesReviewer1;
            }
        }
        private List<ReconcilingCode> _CodesReviewer2;
        public List<ReconcilingCode> CodesReviewer2
        {
            get
            {
                return _CodesReviewer2;
            }
        }
        private List<ReconcilingCode> _CodesReviewer3;
        public List<ReconcilingCode> CodesReviewer3
        {
            get
            {
                return _CodesReviewer3;
            }
        }
        private long _ItemSetR1;
        public long ItemSetR1
        {
            get
            {
                return _ItemSetR1;
            }
        }
        private long _ItemSetR2;
        public long ItemSetR2
        {
            get
            {
                return _ItemSetR2;
            }
        }
        private long _ItemSetR3;
        public long ItemSetR3
        {
            get
            {
                return _ItemSetR3;
            }
        }
        public List<ItemArm> ItemArms
        {
            get
            {
                return this.item.Arms.ToList();
            }
        }

        public ReconcilingItem(Item item, bool isCompleted, List<ReconcilingCode> codesReviewer1, List<ReconcilingCode> codesReviewer2, List<ReconcilingCode> codesReviewer3
            , string completedby, int completedbyID, long completedItemSetID
            , long itemsetR1, long itemsetR2, long itemsetR3)
        {
            _item = item;
            _CodesReviewer1 = codesReviewer1;
            _CodesReviewer2 = codesReviewer2;
            _CodesReviewer3 = codesReviewer3;
            _IsCompleted = isCompleted;
            _CompletedByName = completedby;
            _CompletedByID = completedbyID;
            _CompletedItemSetID = completedItemSetID;
            _ItemSetR1 = itemsetR1;
            _ItemSetR2 = itemsetR2;
            _ItemSetR3 = itemsetR3;
        }
    }
    //a single code for a single contact
    public class ReconcilingCode
    {
        private Int64 _ID, _AttSetID, _ArmID;
        private string _Name;
        private string _ArmName;
        private string _Fullpath;
        private string _InfoBox;
        public Int64 ID
        { get { return _ID; } }
        public Int64 AttributeSetID
        { get { return _AttSetID; } }
        public Int64 ArmID
        {
            get { return _ArmID; }
            set { _ArmID = value; }
        }
        public string ArmName
        {
            get { return _ArmName; }
            set { _ArmName = value; }
        }
        public string Name
        { get { return _Name; } }
        public string Fullpath
        { get { return _Fullpath; } }
        public string InfoBox
        { 
            get { return _InfoBox; }
            set { _InfoBox = value; }
        }
        public ReconcilingCode Clone()
        {
            ReconcilingCode res = new ReconcilingCode(this.ID, this.AttributeSetID, this.Name, this.Fullpath);
            return res;
        }
        public ReconcilingCode(Int64 AttributeID, Int64 attributeSetID, string name, string fullpath)
        {
            _ID = AttributeID;
            _Name = name;
            _Fullpath = fullpath;
            _AttSetID = attributeSetID;
        }
    }
    //the root object: contains the list of attributes and the list of items
    public class ReconcilingItemList : INotifyPropertyChanged
    {
        public  event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        private List<ReconcilingCode> _Attributes;
        public List<ReconcilingCode> Attributes
        { get { return _Attributes; } }

        private List<ReconcilingItem> _Items;
        public List<ReconcilingItem> Items
        { get { return _Items; } }
        public string Description;
        private Comparison _Comparison;
        public Comparison Comparison
        {
            get { return _Comparison; }
        }
        public bool ShowReviewer3
        {
            get
            {
                if (Comparison == null || Comparison.ContactId3 < 1) return false;
                else return true;
            }
        }
        public Visibility Reviewer3Visibility
        {
            get
            {
                if (Comparison == null || Comparison.ContactId3 < 1) return Visibility.Collapsed;
                else return Visibility.Visible;
            }
        }
        public string Reviewer1
        {
            get
            {
                if (Comparison == null) return "";
                return Comparison.ContactName1;
            }
        }
        public ReconcilingCode GetReconcilingCodeFromID(Int64 AttributeID)
        {
            if (_Attributes == null) return null;
            foreach (ReconcilingCode rcc in _Attributes)
            {
                if (rcc.ID == AttributeID) return rcc;
            }
            return null;
        }
        public ReconcilingItemList()
        {
        }
        public ReconcilingItemList(ReviewSet Set, Comparison comp, string Descr)
        {
            _Items = new List<ReconcilingItem>();
            _Attributes = new List<ReconcilingCode>();
            _Comparison = comp;
            Description = Descr;
            NotifyPropertyChanged("Comparison");
            NotifyPropertyChanged("Items");
            NotifyPropertyChanged("Attributes");
            NotifyPropertyChanged("ShowReviewer3");
            NotifyPropertyChanged("Reviewer1");
            foreach (AttributeSet CaSet in Set.Attributes)
            {
                buildToPasteFlatUnsortedList(CaSet, "");
            }
        }
        public void AddItem(Item item, ItemSetList itemSetList)
        {
            if (_Comparison == null || itemSetList == null || itemSetList.Count == 0) return;
            bool isCompleted = false;
            string CompletedBy = "";
            int CompletedByID = 0; long CompletedItemSetID = 0;
            List<ReconcilingCode> r1 = new List<ReconcilingCode>();
            List<ReconcilingCode> r2 = new List<ReconcilingCode>();
            List<ReconcilingCode> r3 = new List<ReconcilingCode>();
            long itSetR1 = -1, itSetR2 = -1, itSetR3 = -1;
            foreach (ItemSet iSet in itemSetList)
            {
                if (iSet.SetId != this.Comparison.SetId) continue;
                else
                {
                    if (iSet.IsCompleted)
                    {
                        isCompleted = iSet.IsCompleted;
                        CompletedBy = iSet.ContactName;
                        CompletedByID = iSet.ContactId;
                        CompletedItemSetID = iSet.ItemSetId;
                    }
                    if (iSet.ContactId == _Comparison.ContactId1)
                    {
                        itSetR1 = iSet.ItemSetId;
                        foreach (ReadOnlyItemAttribute roia in iSet.ItemAttributes)
                        {
                            ReconcilingCode r0 = GetReconcilingCodeFromID(roia.AttributeId);
                            if (r0 != null)//this is necessary to avoid trying to add a code that belongs to the item, but is coming from a dead branch (a code for wich one of the parents got deleted)!
                            {//in such situations r0 is null
                                ReconcilingCode r = r0.Clone();
                                r.InfoBox = roia.AdditionalText;
                                r.ArmID = roia.ArmId;
                                r.ArmName = roia.ArmTitle;
                                r1.Add(r);
								//if (roia.ArmTitle != "")
								//{
								//	Debug.WriteLine("We have these arms: " + iSet.ItemSetId + "" + roia.ArmTitle);
								//}
                            }
                        }
                    }
                    else if (iSet.ContactId == _Comparison.ContactId2)
                    {
                        itSetR2 = iSet.ItemSetId;
                        foreach (ReadOnlyItemAttribute roia in iSet.ItemAttributes)
                        {
                            ReconcilingCode r0 = GetReconcilingCodeFromID(roia.AttributeId);
                            if (r0 != null)//this is necessary to avoid trying to add a code that belongs to the item, but is coming from a dead branch (a code for wich one of the parents got deleted)!
                            {//in such situations r0 is null
                                ReconcilingCode r = r0.Clone();
                                r.InfoBox = roia.AdditionalText;
                                r.ArmID = roia.ArmId;
                                r.ArmName = roia.ArmTitle;
                                r2.Add(r);
								//if (roia.ArmTitle != "")
								//{
								//	Debug.WriteLine("We have these arms: " + iSet.ItemSetId + "" + roia.ArmTitle);
								//}
							}
                        }
                    }
                    else if (iSet.ContactId == _Comparison.ContactId3)
                    {
                        itSetR3 = iSet.ItemSetId;
                        foreach (ReadOnlyItemAttribute roia in iSet.ItemAttributes)
                        {
                            ReconcilingCode r0 = GetReconcilingCodeFromID(roia.AttributeId);
                            if (r0 != null)//this is necessary to avoid trying to add a code that belongs to the item, but is coming from a dead branch (a code for wich one of the parents got deleted)!
                            {//in such situations r0 is null
                                ReconcilingCode r = r0.Clone();
                                r.InfoBox = roia.AdditionalText;
                                r.ArmID = roia.ArmId;
                                r.ArmName = roia.ArmTitle;
                                r3.Add(r);
								//if (roia.ArmTitle != "")
								//{
								//	Debug.WriteLine("We have these arms: " + iSet.ItemSetId + "" + roia.ArmTitle);
								//}
							}
                        }
                    }
                }
            }
            this._Items.Add(new ReconcilingItem(item, isCompleted, r1, r2, r3, CompletedBy, CompletedByID, CompletedItemSetID, itSetR1, itSetR2, itSetR3));
            NotifyPropertyChanged("Items");
        }
        public string Report(bool showPath, bool showInfo, bool showST, bool showFT, bool showAbst)
        {
            string res = this.Description;
            res += "<table border=1><TR><TH width='7%'><p style='margin-left:3px; margin-right:3px;'>ID</p></TH>";
            if (showST)
            {
                res += "<TH width='8%'><p style='margin-left:3px; margin-right:3px;'>Short Title</p></TH>";
            }
            if (showFT)
            {
                res += "<TH><p style='margin-left:3px; margin-right:3px;'>Title</p></TH>";
            }
            if (showAbst)
            {
                res += "<TH><p style='margin-left:3px; margin-right:3px;'>Abstract</p></TH>";
            }

            res += "<TH><p style='margin-left:3px; margin-right:3px;'>" + this.Comparison.ContactName1 + "</p></TH>";
            res += "<TH><p style='margin-left:3px; margin-right:3px;'>" + this.Comparison.ContactName2 + "</p></TH>";
            if (ShowReviewer3)
            {
                res += "<TH><p style='margin-left:3px; margin-right:3px;'>" + this.Comparison.ContactName3 + "</p></TH>";
            }
            res += "</TR>";
            foreach (ReconcilingItem rli in this.Items)
            {
                res += "<TR><td><p style='margin-left:3px; margin-right:3px;'>" + rli.item.ItemId.ToString() + "</p></td>";
                if (showST)
                {
                    res += "<td><p style='margin-left:3px; margin-right:3px;'>" + rli.item.ShortTitle + "</p></td>";
                }
                if (showFT)
                {
                    res += "<td><p style='margin-left:3px; margin-right:3px;'>" + rli.item.Title + "</p></td>";
                }
                if (showAbst)
                {
                    res += "<td><p style='margin-left:3px; margin-right:3px;'>" + rli.item.Abstract + "</p></td>";
                }
                res += "<td><p style='margin-left:3px; margin-right:3px;'>";//codes for R1
                if (rli.CompletedByID == this.Comparison.ContactId1)
                {
                    res += "<span style='color:rgb(45, 140, 45);'>*Completed*</span><br />";
                }
                foreach (ReconcilingCode rcc in rli.CodesReviewer1)
                {
                    res += "&#8226; ";
                    if (showPath && rcc.Fullpath.Length > 0)
                    {
                        res += "<span style='color:rgb(100, 100, 100);'>(" + rcc.Fullpath.Replace("<¬sep¬>", "\\") + ")</span>";
                    }
                    if (rcc.ArmID != 0)
                    {
                        IEnumerable<ItemArm> arms = rli.ItemArms.Where(x => x.ItemArmId == rcc.ArmID);
                        if (arms != null && arms.Count() > 0) res += rcc.Name + " <span style='font-family:Times, serif; font-size: 76%;'>[Arm: " + arms.First().Title + "]</span><BR />";
                        else res += rcc.Name + " <span style='font-family:Times, serif; font-size: 76%;'>[Arm ID: " + rcc.ArmID + "]</span><BR />";
                    }
                    else res += rcc.Name + "<BR />";
                    if (showInfo && rcc.InfoBox.Length > 0)
                    {
                        res += "<span style='font-family:Times, serif; font-size: 76%;'>[Info:] " + rcc.InfoBox + "</span><br />";
                    }
                }
                res += "</p></td>";
                res += "<td><p style='margin-left:3px; margin-right:3px;'>";//codes for R2
                if (rli.CompletedByID == this.Comparison.ContactId2)
                {
                    res += "<span style='color:rgb(45, 140, 45);'>*Completed*</span><br />";
                }
                foreach (ReconcilingCode rcc in rli.CodesReviewer2)
                {
                    res += "&#8226; ";
                    if (showPath && rcc.Fullpath.Length > 0)
                    {
                        res += "<span style='color:rgb(100, 100, 100);'>(" + rcc.Fullpath.Replace("<¬sep¬>", "\\") + ")</span>";
                    }
                    if (rcc.ArmID != 0)
                    {
                        IEnumerable<ItemArm> arms = rli.ItemArms.Where(x => x.ItemArmId == rcc.ArmID);
                        if (arms != null && arms.Count() > 0) res += rcc.Name + " <span style='font-family:Times, serif; font-size: 76%;'>[Arm: " + arms.First().Title + "]</span><BR />";
                        else res += rcc.Name + " <span style='font-family:Times, serif; font-size: 76%;'>[Arm ID: " + rcc.ArmID + "]</span><BR />";
                    }
                    else res += rcc.Name + "<BR />";
                    if (showInfo && rcc.InfoBox.Length > 0)
                    {
                        res += "<span style='font-family:Times, serif; font-size: 76%;'>[Info:] " + rcc.InfoBox + "</span><br />";
                    }
                }
                res += "</p></td>";
                if (ShowReviewer3)
                {
                    res += "<td><p style='margin-left:3px; margin-right:3px;'>";//codes for R3
                    if (rli.CompletedByID == this.Comparison.ContactId3)
                    {
                        res += "<span style='color:rgb(45, 140, 45);'>*Completed*</span><br />";
                    } 
                    foreach (ReconcilingCode rcc in rli.CodesReviewer3)
                    {
                        res += "&#8226; ";
                        if (showPath && rcc.Fullpath.Length > 0)
                        {
                            res += "<span style='color:rgb(100, 100, 100);'>(" + rcc.Fullpath.Replace("<¬sep¬>", "\\") + ")</span>";
                        }
                        if (rcc.ArmID != 0)
                        {
                            IEnumerable<ItemArm> arms = rli.ItemArms.Where(x => x.ItemArmId == rcc.ArmID);
                            if (arms != null && arms.Count() > 0) res += rcc.Name + " <span style='font-family:Times, serif; font-size: 76%;'>[Arm: " + arms.First().Title + "]</span><BR />";
                            else res += rcc.Name + " <span style='font-family:Times, serif; font-size: 76%;'>[Arm ID: " + rcc.ArmID + "]</span><BR />";
                        }
                        else res += rcc.Name + "<BR />";
                        if (showInfo && rcc.InfoBox.Length > 0)
                        {
                            res += "<span style='font-family:Times, serif; font-size: 76%;'>[Info:] " + rcc.InfoBox + "</span><br />";
                        }
                    }
                    res += "</p></td>";
                }
                res += "</TR>";
            }
            res += "</TABLE>";

            return res;
        }


        private void buildToPasteFlatUnsortedList(AttributeSet aSet, string path)
        {//this is recursive!!
            ReconcilingCode astp = new ReconcilingCode(aSet.AttributeId, aSet.AttributeSetId, aSet.AttributeName, path);
            _Attributes.Add(astp);
            foreach (AttributeSet CaSet in aSet.Attributes)
            {
                buildToPasteFlatUnsortedList(CaSet, path + "<¬sep¬>" + aSet.AttributeName);
            }

        }
    }
}
