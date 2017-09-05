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
using System.Collections.ObjectModel;
using BusinessLibrary.BusinessClasses.ImportItems;
using BusinessLibrary.BusinessClasses;
using BusinessLibrary.Security;
using Csla.Silverlight;
using Csla;
using Csla.DataPortalClient;
using Telerik.Windows.Controls;
using Telerik.Windows.Data;
using Csla.Xaml;

namespace EppiReviewer4
{
    public partial class ImportItems : UserControl
    {
        //first bunch of lines to make the read-only UI work
        private BusinessLibrary.Security.ReviewerIdentity ri;
        private string TxtFileContent;
        private RadWDeleteSourceForeverDialog DeleteSourceForeverDialogWindow = new RadWDeleteSourceForeverDialog();
        OpenFileDialog fileDialog;
        public bool HasWriteRights
        {
            get
            {
                if (ri == null) return false;
                else return ri.HasWriteRights();
            }
        }
        //end of read-only ui hack
        
        public ImportItems()
        {
            InitializeComponent();
            //this.grView1.ItemsSource = GetObservableObjectData();
            //this.rb1.IsChecked = true;
            
            //two lines to make the read-only UI work
            //add <Button x:Name="isEn" IsEnabled="{Binding HasWriteRights, Mode=OneWay}" ></Button>
            //to the xaml resources section!!
            ri = Csla.ApplicationContext.User.Identity as BusinessLibrary.Security.ReviewerIdentity;
            isEn.DataContext = this;
            SaveSrcCMD.DataContext = this;
            //end of read-only ui hack
            
            //this.Height = Double.NaN;
            //b1.IsEnabled = false;

            DeleteSourceForeverDialogWindow.CancelDeleteSourceForeverButton_Clicked +=new EventHandler<RoutedEventArgs>(CancelDeleteSourceForeverButton_Click);
            DeleteSourceForeverDialogWindow.ConfirmDeleteSourceForeverButton_Clicked +=new EventHandler<RoutedEventArgs>(ConfirmDeleteSourceForeverButton_Click);

            b2.IsEnabled = false;
            txtb0.IsEnabled = false;
            grView1.AutoGenerateColumns = false;
            SearchDateIn.SelectedDate = DateTime.Now;
            ShowEndInt.NumberDecimalDigits = 0;
            ShowStartInt.NumberDecimalDigits = 0;
            StartInt.NumberDecimalDigits = 0;
            EndInt.NumberDecimalDigits = 0;
            grView1.RowIndicatorVisibility = Visibility.Collapsed;
            grView0.VerticalAlignment = VerticalAlignment.Top;
            grView0.RowIndicatorVisibility = Visibility.Collapsed;
            //grView0.ShowColumnHeaders = false;
            grView0.IsReadOnly = true;
            grView0.DataLoaded += new EventHandler<EventArgs>(grView0_DataLoaded);

            FilterDescriptor descriptor = new FilterDescriptor();
            descriptor.Member = "Source_ID";
            descriptor.Operator = FilterOperator.IsGreaterThan;
            descriptor.Value = "-1";
            grView0.FilterDescriptors.Clear();
            grView0.FilterDescriptors.Add(descriptor);

            GetrulesROL();
            refreshSources();
            if (FilterRuleCB.Items != null) FilterRuleCB.SelectedIndex = 0;
            if (grView0.Columns.Count == 0)
            {
                List<string> holder = new List<string>();
                holder.Add("");
                grView0.ItemsSource = holder;
            }
            
            //SLControl = new SourceList();
            //ListSources.Children.Add(SLControl);
            //refreshSources();
            
        }

        void grView0_DataLoaded(object sender, EventArgs e)
        {
            // If added by James 21/12/09 as was giving error
            if (grView0.Items.Count > 0)
                grView0.SelectedItem = grView0.Items[0];
        }
        private IncomingItemsList IIL = new IncomingItemsList();
        private IncomingItemsList PartialIIL;//used to send large lists in chunks
        private int PartialStart = 0, PartialEnd = 2500, batchesN = 0, currentBatch=1;//counters to keep track of what has been sent.
        DateTime startTime = DateTime.Now;
        private void b1_onclick(object sender, RoutedEventArgs e)
        {
            loadingInItemsAnimation.IsRunning = true;
            IIL = new IncomingItemsList();
            //IIL.Saved += new EventHandler<Csla.Core.SavedEventArgs>(IIL_Saved);
            b2.IsEnabled = false;
            fileDialog = new OpenFileDialog();
            fileDialog.Multiselect = false;
            bool? result = fileDialog.ShowDialog();

            // Open the file if OK was clicked in the dialog
            if (result == true)
            {
                System.IO.FileStream fileStream = fileDialog.File.OpenRead();
                System.IO.StreamReader myFile = new System.IO.StreamReader(fileStream);
                TxtFileContent = myFile.ReadToEnd();
                TxtFileContent = ImportRefs.StripIllegalChars(TxtFileContent);
                myFile.Close();
                fileStream.Close();
                ReadInputTxtFile();
            }
            loadingInItemsAnimation.IsRunning = false;
        }
        private void b2_onclick(object sender, RoutedEventArgs e)
        {//what happens here depends on how many items we are trying to import!
            //check, do nothing if there are no items to import
            if (IIL == null || IIL.IncomingItems == null || IIL.IncomingItems.Count == 0)
                return;
            b2.IsEnabled = false;
            loadingInItemsAnimation.IsRunning = true;
            SourcesMainPaneGroup.IsEnabled = false;
            
            startTime = DateTime.Now;
            IIL.SourceName = txtb0.Text;
            IIL.DateOfImport = DateTime.Now;
            IIL.DateOfSearch = SearchDateIn.SelectedDate == null ? DateTime.Now : (DateTime)SearchDateIn.SelectedDate;
            IIL.SourceDB = TBSourceDB.Text;
            IIL.SearchStr = TBsString.Text;
            IIL.SearchDescr = TBDescr.Text;
            IIL.Included = (bool)(cbIncludeTxt.IsChecked);
            IIL.Notes = TBNotes.Text;
            //first case, items are less than 1500, we assume it's safe to send the list directly
            if (IIL.IncomingItems.Count <= 1500 && cbSafeImport.IsChecked != true)
            {
                IIL.Saved += new EventHandler<Csla.Core.SavedEventArgs>(IIL_Saved);
                IIL.BeginSave();
            }
            else //second case: we need to send the list in chunks
            {
                PartialIIL = new IncomingItemsList();
                PartialStart = 0;
                PartialEnd = cbSafeImport.IsChecked == true ? 249 : 999; //if using safe mode, send 250 items per chunk
                batchesN = (int)(Math.Ceiling((double)IIL.IncomingItems.Count / (double)(PartialEnd + 1)));
                currentBatch = 1;
                PartialIIL.IncomingItems = new Csla.Core.MobileList<ItemIncomingData>();
                PartialIIL.IncomingItems.AddRange(IIL.IncomingItems.GetRange(0, PartialEnd + 1));
                PartialIIL.IsLast = false;
                PartialIIL.SourceID = 0;
                PartialIIL.SourceName = txtb0.Text;
                PartialIIL.DateOfImport = DateTime.Now;
                PartialIIL.DateOfSearch = SearchDateIn.SelectedDate == null ? DateTime.Now : (DateTime)SearchDateIn.SelectedDate;
                PartialIIL.SourceDB = TBSourceDB.Text;
                PartialIIL.SearchStr = TBsString.Text;
                PartialIIL.SearchDescr = TBDescr.Text;
                PartialIIL.Included = (bool)(cbIncludeTxt.IsChecked);
                PartialIIL.Notes = TBNotes.Text;
                PartialIIL.Saved += new EventHandler<Csla.Core.SavedEventArgs>(PartialIIL_Saved);
                SetResultMessage("Uploading batch " + currentBatch.ToString() + " of " + batchesN.ToString() + ".");
                PartialIIL.BeginSave();
            }
        }
       void IIL_Saved(object sender, Csla.Core.SavedEventArgs e)
        {
            //IncomingItemsList oo = (IncomingItemsList)sender;
            //IncomingItemsList resultL = (IncomingItemsList)sender;
            if (e.Error != null)
            {
                if (IIL.IncomingItems.Count > 500)
                    RadWindow.Alert("Could not save." + Environment.NewLine + "It is likely that the file to import is too big." + Environment.NewLine + "Please try importing using the safe mode option.");
                else RadWindow.Alert(e.Error.Message);
                SourcesMainPaneGroup.IsEnabled = true;
                this.grView1.ItemsSource = null;
                SetResultMessage("");
            }
            else
            {
                IncomingItemsList resultL = (IncomingItemsList)e.NewObject;
                //DateTime endTime = DateTime.Now;
                //TimeSpan ts = endTime - startTime;
                //string resTimes = resultL.SourceName + "\r\n" + "Total time (ms): " + ts.TotalMilliseconds;
                //MessageBox.Show(resTimes);
                this.grView1.ItemsSource = resultL.IncomingItems;
                this.grView1.Rebind();
                this.b2.IsEnabled = false;
                this.txtb0.Text = "";
                //NEw Add Sept 2017: mark the review as in need of indexing, if required.
                UpdateReviewInfo();
                refreshSources();
            }
            IIL.IncomingItems.Clear();
            loadingInItemsAnimation.IsRunning = false;
        }
       void PartialIIL_Saved(object sender, Csla.Core.SavedEventArgs e)
       {
           if (e.Error != null)
           {
               RadWindow.Alert(e.Error.Message);
               SourcesMainPaneGroup.IsEnabled = true;
               this.grView1.ItemsSource = null;
               SetResultMessage("");
           }
           else
           {
               
               IncomingItemsList resultL = (IncomingItemsList)e.NewObject;
               //check if there is still data to be sent
               if (!resultL.IsLast || (PartialEnd < IIL.IncomingItems.Count - 1))
               {
                   PartialIIL.IsFirst = false;
                   currentBatch++;
                   SetResultMessage("Uploading batch " + currentBatch.ToString() + " of " + batchesN.ToString() + ".");
                   PartialStart = PartialStart + PartialIIL.IncomingItems.Count;
                   if (PartialEnd + PartialIIL.IncomingItems.Count >= IIL.IncomingItems.Count - 1)
                   {//this is the last batch
                       PartialEnd = IIL.IncomingItems.Count - 1;
                       PartialIIL.IsLast = true;
                   }
                   else
                   {//this is another partial batch
                       PartialEnd = PartialEnd + PartialIIL.IncomingItems.Count;
                   }
                   PartialIIL.IncomingItems.Clear();
                   PartialIIL.IncomingItems.AddRange(IIL.IncomingItems.GetRange(PartialStart, PartialEnd - PartialStart + 1));
                   PartialIIL.SourceID = resultL.SourceID;
                   PartialIIL.BeginSave(true);
               }
               //otherwise, clean up
               else
               {
                   //DateTime endTime = DateTime.Now;
                   //TimeSpan ts = endTime - startTime;
                   //string resTimes = resultL.SourceName + "\r\n" + "Total time (ms): " + ts.TotalMilliseconds;
                   //MessageBox.Show(resTimes);
                   this.b2.IsEnabled = false;
                   this.txtb0.Text = "";
                   SetResultMessage("");
                    //NEw Add Sept 2017: mark the review as in need of indexing, if required.
                    UpdateReviewInfo();
                    refreshSources();
                   IIL.IncomingItems.Clear();
                   this.grView1.ItemsSource = null;
                   this.grView1.Rebind();
                   loadingInItemsAnimation.IsRunning = false;
                   PartialIIL.Saved -= PartialIIL_Saved;
               }
           }
           //IIL.IncomingItems.Clear();
           //loadingInItemsAnimation.IsRunning = false;
       }
        private void UpdateReviewInfo()
        {
            //NEw Add Sept 2017: mark the review as in need of indexing, if required.
            CslaDataProvider provider = App.Current.Resources["ReviewInfoData"] as CslaDataProvider;
            if (provider == null || provider.Data == null) return;
            ReviewInfo RevInfo = provider.Data as ReviewInfo;
            if (RevInfo == null) return;
            if (RevInfo.ShowScreening && RevInfo.ScreeningCodeSetId > 0)
            {
                RevInfo.Saved += (o, e2) =>
                {
                    provider.Refresh();
                };
                RevInfo.ScreeningIndexed = false;
                RevInfo.ApplyEdit();
                RevInfo.BeginSave(true);
            }
        }
        void SetResultMessage(string Msg)
       {
           if (Msg == "" || Msg.Contains(" items found.") || Msg == null)
           {
               boderHighlight.Background = null;
           }
           else
           {
               boderHighlight.Background = new SolidColorBrush(Colors.Yellow);
           }
           txtb1.Text = Msg;
       }
        private void txtb0_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (grView1.HasItems) b2.IsEnabled = HasWriteRights & checkSourceNames(this.txtb0);
        }
        
        public void refreshSources()
        {
            txtb0.IsEnabled = false;
            //SLControl.refreshSources();
            //if (grView0.Columns.Count > 0) txtb0.IsEnabled = true;
            CslaDataProvider provider = this.Resources["SourcesData"] as CslaDataProvider;
            provider.FactoryMethod = "GetSources";
            provider.Refresh();
            if (grView0.Columns.Count > 0)
            {
                grView0.Columns[0].Width = new Telerik.Windows.Controls.GridViewLength(0);
                //txtb0.IsEnabled = true;
            }
        }
        public void GetrulesROL()
        {

            CslaDataProvider provider = this.Resources["RulesData"] as CslaDataProvider;
            provider.FactoryMethod = "GetReadOnlyImportFilterRuleList";
            provider.Refresh();
        }
        private void CslaDataProvider_DataChanged(object sender, EventArgs e)
        {
            SourcesMainPaneGroup.IsEnabled = true;
            CslaDataProvider provider = ((CslaDataProvider)this.Resources["SourcesData"]);
            if (provider == null)
            {
                //System.Windows.Browser.HtmlPage.Window.Alert("no sources");
                return;
            }
            if (provider.Error != null)
                System.Windows.Browser.HtmlPage.Window.Alert(((Csla.Xaml.CslaDataProvider)sender).Error.Message);
            else
            {
                if (provider.Data != null)

                    //this.grView0.ItemsSource = ((BusinessLibrary.BusinessClasses.ReadOnlySourceList)provider.Data);
                    //this.grView0.Rebind();//TextBlockDocCount.Text = (provider.Data as ItemList).Count.ToString() + " documents loaded.";
                if (grView0.Columns.Count > 0)
                {
                    grView0.Columns[0].Width = new Telerik.Windows.Controls.GridViewLength(0, Telerik.Windows.Controls.GridViewLengthUnitType.Auto);
                    txtb0.IsEnabled = true;
                }
            }
        }
        private void CslaDataProvider_RulesDataChanged(object sender, EventArgs e)
        {
            CslaDataProvider provider = ((CslaDataProvider)this.Resources["RulesData"]);
            if (provider == null)
            {
                //System.Windows.Browser.HtmlPage.Window.Alert("no sources");
                return;
            }
            if (provider.Error != null)
                System.Windows.Browser.HtmlPage.Window.Alert(((Csla.Xaml.CslaDataProvider)sender).Error.Message);
            else
            {
                if (provider.Data != null)
                {
                    //this.FilterRuleCB.ItemsSource = ((BusinessLibrary.BusinessClasses.ReadOnlyImportFilterRuleList)provider.Data);
                    FilterRuleCB.SelectedIndex = 0;
                }
                if (grView0.Columns.Count > 0)
                {
                    grView0.Columns[0].Width = new Telerik.Windows.Controls.GridViewLength(0, Telerik.Windows.Controls.GridViewLengthUnitType.Auto);
                    txtb0.IsEnabled = true;
                }
            }
        }

        private void grView0_SelectionChanged(object sender, Telerik.Windows.Controls.SelectionChangeEventArgs e)
        {
            //SourceDetails.DataContext = grView0.SelectedItem;
            //Source src = grView0.SelectedItem as Source;
            //if (src == null) DeleteSourceForeverButton.IsEnabled = false;
            //else DeleteSourceForeverButton.IsEnabled = src.IsFlagDeleted & (src.isMasterOf == 0) & HasWriteRights;
            DeleteSourceForeverButton.IsEnabled = false;
            if (grView0.Items != null && grView0.SelectedItem != null)
            {
                int id = (grView0.SelectedItem as ReadOnlySource).Source_ID;
                Csla.DataPortal<Source> dp = new Csla.DataPortal<Source>();
                dp.FetchCompleted += new EventHandler<Csla.DataPortalResult<Source>>(dp_FetchCompleted);
                GettingSourceDetails.IsRunning = true;
                SaveSrcCMD.IsEnabled = false;

                dp.BeginFetch(new Csla.SingleCriteria<Source, int>(id));
            }
            else
            {
                SourceDetails.DataContext = null;
                
            }
        }
        void dp_FetchCompleted(object sender, Csla.DataPortalResult<Source> e)
        {
            GettingSourceDetails.IsRunning = false;
            if (e.Error != null)
            {
                Telerik.Windows.Controls.RadWindow.Alert("Could not Retrieve the source details."
                        + Environment.NewLine + e.Error.Message + Environment.NewLine + "Please Contact the support team.");
            }
            else
            {
                Source src = (e.Object as Source);
                SourceDetails.DataContext = src;
                SaveSrcCMD.DataContext = src;
                ListSources.DataContext = src;
                if (src == null) DeleteSourceForeverButton.IsEnabled = false;
                else
                {
                    DeleteSourceForeverButton.IsEnabled = src.IsFlagDeleted & (src.isMasterOf == 0) & HasWriteRights;
                    SaveSrcCMD.IsEnabled = HasWriteRights;
                }
                
                //Source IDG = (e.Object as Source);
                //GroupDetails1.DataContext = IDG;
                //GroupDetails1.IsEnabled = IDG.IsEditable;
                //duplgr_radgrid_AdvgroupMembers.DataContext = IDG;
                //GroupDetails1.ItemsSource = IDG.Members;
                //duplgr_radgrid_AdvgroupMembers.ItemsSource = IDG.Members;
                //FilterDescriptor descriptor = new FilterDescriptor();
                //descriptor.Member = "IsMaster";
                //descriptor.Operator = FilterOperator.IsEqualTo;
                //descriptor.Value = "False";
                //GroupDetails1.FilterDescriptors.Clear();
                //duplgr_radgrid_AdvgroupMembers.FilterDescriptors.Clear();
                //GroupDetails1.FilterDescriptors.Add(descriptor);
                //duplgr_radgrid_AdvgroupMembers.FilterDescriptors.Add(descriptor);
                //GroupDetails1.Rebind();
                //duplgr_radgrid_AdvgroupMembers.Rebind();
                //ItemDuplicateGroupMember gm = IDG.getMaster();
                //MasterDetails1.DataContext = gm;
                //MasterDetails2.DataContext = gm;
                //duplgr_radgrid_ManualMembers.ItemsSource = IDG.ManualMembers;
                //duplgr_radgrid_MainManualMembers.ItemsSource = IDG.ManualMembers;
            }
        }
        private void SaveSrcCMD_Click(object sender, RoutedEventArgs e)
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            Source thisSource = (sender as Button).DataContext as Source;
            CslaDataProvider provider = ((CslaDataProvider)this.Resources["SourcesData"]);


            thisSource.Saved += (o, e2) =>
            {
                //SourcesMainPaneGroup.IsEnabled = true;
                if (e2.Error != null) 
                    Telerik.Windows.Controls.RadWindow.Alert("Sorry, changes to source could not be saved."
                        + Environment.NewLine + e2.Error.Message);
                else provider.Refresh();
            };
            SourcesMainPaneGroup.IsEnabled = false;
            thisSource.BeginSave();
        }
        private void Search_Click(object sender, RoutedEventArgs e)
        {

            pubmedBusy.IsRunning = true;
            SourcesMainPaneGroup.IsEnabled = false;
            PubMedSearch.GetPubMedSearch(SearchStr.Text, (o, e2) =>
            {
                SourcesMainPaneGroup.IsEnabled = true;
                pubmedBusy.IsRunning = false;
                if (e2.Error != null) SearchRes.Text = e2.Error.Message;
                else
                {
                    SearchRes.Text = e2.Object.Summary;
                    string Tname = e2.Object.ItemsList.SourceName;
                    grViewWebSearch.DataContext = e2.Object as PubMedSearch;
                    grViewWebSearch.ItemsSource = e2.Object.ItemsList.IncomingItems;
                    //BusinessLibrary.BusinessClasses.SourceList ROSL = (BusinessLibrary.BusinessClasses.SourceList)provider.Data;
                    //for (int i=0; i < grView0.Items.Count; i++)
                    //{
                    //    BusinessLibrary.BusinessClasses.Source ROS = (Source)grView0.Items[i];
                    //    if (ROS.Source_Name == Tname)
                    //    {
                    //        SaveWebSearch.IsEnabled = false;
                    //    }
                    //}
                    //grViewWebSearch.DataContext = e2.Object;
                    PubmSearchName.Text = PubmSearchName.Text == "" ? e2.Object.ItemsList.SourceName : PubmSearchName.Text;
                    //grViewWebSearch.Rebind();
                }
            });
            //dp.BeginExecute(command);
        }

        private void SaveWebSearch_Click(object sender, RoutedEventArgs e)
        {
            pubmedBusy.IsRunning = true;
            SourcesMainPaneGroup.IsEnabled = false;
            PubMedSearch pms = (PubMedSearch)grViewWebSearch.DataContext;
            pms.ItemsList.SourceName = PubmSearchName.Text;
            pms.showStart = 0;
            pms.showEnd = 0;
            pms.saveStart = (int)StartInt.Value;
            pms.saveEnd = (int)EndInt.Value;
            pms.ItemsList.Notes = PubmNotes.Text;
            pms.ItemsList.SearchDescr = PubmDescr.Text;
            pms.ItemsList.IncomingItems.Clear();
            pms.ItemsList.Included = (bool)(cbInclude.IsChecked);
            pms.Saved += new EventHandler<Csla.Core.SavedEventArgs>(pms_Saved);
            pms.BeginSave();
        }

        void pms_Saved(object sender, Csla.Core.SavedEventArgs e)
        {
            pubmedBusy.IsRunning = false;
            PubMedSearch pms = e.NewObject as PubMedSearch;
            if (pms == null)
            {
                SearchRes.Text = "Nothing was saved, the server returned an error: " + e.Error.Message;
                SourcesMainPaneGroup.IsEnabled = false;
                return;
            }
            SearchRes.Text = pms.Summary;
            if (pms.Summary == "Nothing was saved: items requested could not be fetched from PubMed")
            {
                SourcesMainPaneGroup.IsEnabled = true;
                return;
            }
            if (pms.ItemsList.IncomingItems == null)
            {
                ResetSearch();
                UpdateReviewInfo();
                refreshSources();
            }
            else SourcesMainPaneGroup.IsEnabled = true;
            grViewWebSearch.DataContext = pms as PubMedSearch;
            grViewWebSearch.ItemsSource = pms.ItemsList.IncomingItems;
            
        }

        private void PubmSearchName_LostFocus(object sender, RoutedEventArgs e)
        {
            //checkSourceNames(SearchRes);
        }
        private bool checkSourceNames(object sender)
        {
            bool enalble = true;
            string reason = "";
            TextBox tb = (sender as TextBox);
            CslaDataProvider provider = this.Resources["SourcesData"] as CslaDataProvider;
            BusinessLibrary.BusinessClasses.ReadOnlySourceList ROSL = (BusinessLibrary.BusinessClasses.ReadOnlySourceList)provider.Data;

            if (tb.Text == "" | tb.Text == "Insert Source Name")
            {
                enalble = false;
                reason = "Source Name is empty or invalid, please change";
            }
            if (ROSL != null)
            {
                foreach (BusinessLibrary.BusinessClasses.ReadOnlySource ROS in ROSL.Sources)
                {
                    if (ROS.Source_Name == tb.Text)
                    {
                        enalble = false;
                        reason = "Source Name is already in use, please change";
                        break;
                    }
                }
            }
            if (tb.Name == "txtb0")
            {
                //b1.IsEnabled = enalble;
                if (reason != "") SetResultMessage(reason);
                else if (grView1.Items != null && grView1.Items.Count > 0) SetResultMessage(grView1.Items.Count.ToString() + " items found.");
                return enalble;
            }
            else if (tb.Name == "PubmSearchName")
            {
                SaveWebSearch.IsEnabled = enalble;
                //this.txtb1.Text = reason;
            }
            return enalble;
        }

        private void grViewWebSearch_DataLoaded(object sender, EventArgs e)
        {
            SaveWebSearch.IsEnabled = HasWriteRights;
            ShowOther.IsEnabled = true;
            NewSearch.IsEnabled = true;
            StartInt.Value = 1;
            PubMedSearch pms = (sender as Telerik.Windows.Controls.RadGridView).DataContext as PubMedSearch;
            EndInt.Maximum = pms == null ? 1 : pms.QueMax;
            EndInt.Value = EndInt.Maximum;
            StartInt.Value = 1;
            StartInt.Maximum = EndInt.Maximum;
            ShowStartInt.Value = pms == null ? 1 : pms.showStart;
            ShowStartInt.Maximum = EndInt.Maximum;
            ShowEndInt.Value = pms == null ? 1 : pms.showEnd;
            ShowEndInt.Maximum = EndInt.Maximum > 1000 ? 1000 : EndInt.Maximum;
            Search.IsEnabled = false;
            SearchStr.IsEnabled = false;
            pubmedBusy.IsRunning = false;
        }

        private void CheckGetInterval(object sender, Telerik.Windows.Controls.RadRangeBaseValueChangedEventArgs e)
        {
            if (ShowStartInt == null || ShowEndInt == null || grViewWebSearch.DataContext == null) return;
            ShowEndInt.Maximum = ( (double)ShowStartInt.Value + 1000) > EndInt.Maximum ? EndInt.Maximum : (double)ShowStartInt.Value + 1000;
            if (ShowStartInt.Value > ShowEndInt.Value)
                ShowOther.IsEnabled = false;
            else ShowOther.IsEnabled = true;
        }
        private void CheckSaveInterval(object sender, Telerik.Windows.Controls.RadRangeBaseValueChangedEventArgs e)
        {
            if (StartInt == null || EndInt == null || grViewWebSearch.DataContext == null) return;
            if (StartInt.Value > EndInt.Value)
                SaveWebSearch.IsEnabled = false;
            else SaveWebSearch.IsEnabled = HasWriteRights;
        }
        private void NewSearch_Click(object sender, RoutedEventArgs e)
        {
            ResetSearch();
        }
        private void ResetSearch()
        {
            grViewWebSearch.DataContext = null;
            grViewWebSearch.ItemsSource = null;
            SearchRes.Text = "";
            //SearchStr.Text = "";
            PubmSearchName.Text = "";
            SaveWebSearch.IsEnabled = false;
            Search.IsEnabled = true;
            ShowOther.IsEnabled = false;
            NewSearch.IsEnabled = false;
            SearchStr.IsEnabled = true;
            ShowStartInt.Value = 1;
            ShowEndInt.Value = 1;
            StartInt.Value = 1;
            EndInt.Value = 1;
        }
        private void ShowOther_Click(object sender, RoutedEventArgs e)
        {
            pubmedBusy.IsRunning = true;
            SourcesMainPaneGroup.IsEnabled = false;
            PubMedSearch pms = (PubMedSearch)grViewWebSearch.DataContext;
            pms.saveEnd = 0;
            pms.saveStart = 0;
            pms.showStart = (int)ShowStartInt.Value;
            pms.showEnd = (int)ShowEndInt.Value;
            pms.ItemsList.IncomingItems.Clear();
            pms.Saved += new EventHandler<Csla.Core.SavedEventArgs>(pms_Saved);
            pms.BeginSave();
        }
        private void SearchStr_KeyDown(object sender, KeyEventArgs e)
        {//was used by SearchStr text box but dismissed. Consider deleting it!
            if (e.Key.ToString() == "Enter")
            {
                RoutedEventArgs rea = new RoutedEventArgs();
                Search_Click(sender, rea);
            }
        }

        private void FilterRuleCB_SelectionChanged(object sender, Telerik.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (fileDialog != null && fileDialog.File != null)
            {
                loadingInItemsAnimation.IsRunning = true;
                if (grView1.HasItems)
                {
                    grView1.ItemsSource = null;
                    b2.IsEnabled = false;

                }
                ReadInputTxtFile();
                loadingInItemsAnimation.IsRunning = false;
            }
        }
        private void ReadInputTxtFile()
        {
            
            CslaDataProvider provider = this.Resources["RulesData"] as CslaDataProvider;
            BusinessLibrary.BusinessClasses.ReadOnlyImportFilterRule inRules =
                (provider.Data as BusinessLibrary.BusinessClasses.ReadOnlyImportFilterRuleList)[FilterRuleCB.SelectedIndex];
            //this.txtb1.Text = filetxt;
            //fileStream.Close();
            FilterRules rules = new FilterRules();
            foreach (BusinessLibrary.BusinessClasses.TypeRules tprs in inRules.typeRules)
            {
                rules.typeRules.Add(new BusinessLibrary.BusinessClasses.ImportItems.TypeRules(tprs.Type_ID, tprs.RuleName, tprs.RuleRegexSt));
            }
            foreach (KeyValuePair<int, string> kvp in inRules.typesMap)
            {
                rules.AddTypeDef(kvp.Key, kvp.Value);
            }
            rules.Abstract_Set(inRules.Abstract);
            rules.author_Set(inRules.Author);
            rules.Availability_Set(inRules.Availability);
            rules.City_Set(inRules.City);
            rules.date_Set(inRules.Date);
            rules.DefaultTypeCode = inRules.DefaultTypeCode;
            rules.Edition_Set(inRules.Edition);
            rules.EndPage_Set(inRules.EndPage);
            rules.Institution_Set(inRules.Institution);
            rules.Issue_Set(inRules.Issue);
            rules.month_Set(inRules.Month);
            rules.Notes_Set(inRules.Notes);
            rules.OldItemID_Set(inRules.OldItemId);
            rules.Pages_Set(inRules.Pages);
            rules.pAuthor_Set(inRules.ParentAuthor);
            rules.pTitle_Set(inRules.pTitle);
            rules.Publisher_Set(inRules.Publisher);
            rules.shortTitle_Set(inRules.shortTitle);
            rules.StandardN_Set(inRules.StandardN);
            rules.startOfNewField_Set(inRules.StartOfNewField);
            rules.startOfNewRec_Set(inRules.StartOfNewRec);
            rules.StartPage_Set(inRules.StartPage);
            rules.title_Set(inRules.Title);
            rules.typeField_Set(inRules.typeField);
            rules.Url_Set(inRules.Url);
            rules.Volume_Set(inRules.Volume);
            rules.DOI_Set(inRules.DOI);
            rules.Keywords_Set(inRules.Keywords);


            /*if (FilterRuleCB.SelectedIndex == 0)
            {
                rules.AddTypeDef(0, "JOUR|JFULL");
                rules.AddTypeDef(1, "RPRT|PAMP|PCOMM|UNPB");
                rules.AddTypeDef(2, "BOOK");
                rules.AddTypeDef(3, "CHAP");
                rules.AddTypeDef(4, "THES");
                rules.AddTypeDef(5, "CONF");
                rules.AddTypeDef(6, "ELEC|ICOMM");
                rules.AddTypeDef(8, "ADVS|VIDEO|ART|MPCT|MUSIC|SOUND|SLIDE");
                rules.AddTypeDef(10, "MGZN|NEWS");
                rules.AddTypeDef(12, "ABST|BILL|CASE|COMP|CTLG|DATA|GEN|HEAR|INPR|MAP|PAT|STAT|UNBILl");
                rules.DefaultTypeCode = 0;
                rules.typeRules.Add(new TypeRules(2, "Edition", "VL  -"));
                rules.typeRules.Add(new TypeRules(2, "Volume", null));
                rules.typeRules.Add(new TypeRules(4, "Edition", "VL  -"));
                rules.typeRules.Add(new TypeRules(4, "Volume", null));
                rules.typeRules.Add(new TypeRules(4, "Institution", "PB  -"));
                rules.typeRules.Add(new TypeRules(4, "Publisher", null));
                rules.startOfNewRec_Set("TY  -");
                rules.typeField_Set("TY  -");
                rules.startOfNewField_Set("[A-Z][0-Z]  -");
                rules.title_Set("(T1|TI|CT)  -");
                rules.shortTitle_Set("(T1|TI|CT)  -");
                rules.pTitle_Set("(JA|T2|BT|T3)  -");
                rules.date_Set("[YP][1Y]  -");
                rules.month_Set("Y2  -");
                rules.author_Set("A[U1]  -");
                rules.pAuthor_Set("(A2|ED)  -");
                rules.StandardN_Set("SN  -");
                rules.City_Set("CY  -");
                rules.Publisher_Set("PB  -");
                rules.Volume_Set("VL  -");
                rules.Edition_Set(@"\\M\\w");
                rules.Issue_Set("(IS|CP)  -");
                rules.StartPage_Set("SP  -");
                rules.EndPage_Set("EP  -");
                rules.Availability_Set("AV  -");
                rules.Url_Set("(UR|L1|L2)  -");
                rules.Abstract_Set("(N2|AB)  -");
                rules.Notes_Set("N1  -");
            }
            else if (FilterRuleCB.SelectedIndex == 1)
            {
                rules.AddTypeDef(0, "Journal Article");
                /*rules.AddTypeDef(1, "RPRT|PAMP|PCOMM|UNPB");
                rules.AddTypeDef(2, "BOOK");
                rules.AddTypeDef(3, "CHAP");
                rules.AddTypeDef(4, "THES");
                rules.AddTypeDef(5, "CONF");
                rules.AddTypeDef(6, "ELEC|ICOMM");
                rules.AddTypeDef(8, "ADVS|VIDEO|ART|MPCT|MUSIC|SOUND|SLIDE");
                rules.AddTypeDef(10, "MGZN|NEWS");
                rules.AddTypeDef(12, "ABST|BILL|CASE|COMP|CTLG|DATA|GEN|HEAR|INPR|MAP|PAT|STAT|UNBILl");
                rules.typeRules.Add(new TypeRules(2, "Edition", "VL  -"));
                rules.typeRules.Add(new TypeRules(2, "Volume", null));
                rules.typeRules.Add(new TypeRules(4, "Edition", "VL  -"));
                rules.typeRules.Add(new TypeRules(4, "Volume", null));
                rules.typeRules.Add(new TypeRules(4, "Institution", "PB  -"));
                rules.typeRules.Add(new TypeRules(4, "Publisher", null));
                rules.DefaultTypeCode = 0;
                rules.startOfNewRec_Set("PMID-");
                rules.typeField_Set("PT  -");
                rules.startOfNewField_Set(@"^[A-Za-z][A-Za-z][A-Za-z\s][A-Za-z\s]-");
                rules.title_Set("(T1|TI|CT)  -");
                rules.shortTitle_Set("(T1|TI|CT)  -");
                rules.pTitle_Set("JT  -");
                rules.date_Set("DP  -");
                rules.author_Set("A[U1]  -");
                rules.pAuthor_Set("(A2|ED)  -");
                rules.StandardN_Set("IS  -");
                rules.City_Set("PL  -");
                rules.Publisher_Set("PB  -");
                rules.Institution_Set("CN  -");
                rules.Volume_Set("VI  -");
                rules.Edition_Set(@"\\M\\w");
                rules.Issue_Set("IP  -");
                rules.StartPage_Set("SP  -");
                rules.EndPage_Set("EP  -");
                rules.Pages_Set("PG  -");
                rules.Availability_Set("AV  -");
                rules.Url_Set("(UR|L1|L2)  -");
                rules.Abstract_Set("(N2|AB)  -");
                rules.Notes_Set("GN  -");
            }
            else if (FilterRuleCB.SelectedIndex == 2)
            {//refworks
                rules.AddTypeDef(0, "Journal Article|Journal, Electronic|Abstract");
                rules.AddTypeDef(1, "Report|Unpublished Material|Personal Communication");
                rules.AddTypeDef(2, "Book, Edited|Book, Whole|Monograph");
                rules.AddTypeDef(3, "Book, Section");
                rules.AddTypeDef(4, "Dissertation|Thesis|Dissertation/Thesis|Dissertation/Thesis, Unpublished|Dissertation, Unpublished|Thesis, Unpublished");
                rules.AddTypeDef(5, "Conference Proceedings");
                rules.AddTypeDef(6, "Web Page");
                rules.AddTypeDef(8, "Artwork|Motion Picture|Music Score|Sound Recording|Video/DVD|Video|DVD");
                rules.AddTypeDef(9, "Grant");
                rules.AddTypeDef(10, "Magazine Article|Newspaper Article");
                rules.AddTypeDef(12, "Bills/Resolutions|Bills|Resolutions|Case/Court Decisions|Case Decisions|Court Decisions|Computer Program|Generic|Hearing|Laws|Laws/Statutes|Statutes|Map|Online Discussion Forum|Patent");
                rules.DefaultTypeCode = 12;
                /*rules.typeRules.Add(new TypeRules(2, "Edition", "VL  -"));
                rules.typeRules.Add(new TypeRules(2, "Volume", null));
                rules.typeRules.Add(new TypeRules(4, "Edition", "VL  -"));
                rules.typeRules.Add(new TypeRules(4, "Volume", null));
                rules.typeRules.Add(new TypeRules(4, "Institution", "PB "));
                rules.typeRules.Add(new TypeRules(4, "Publisher", null));
                rules.startOfNewRec_Set("^RT ");
                rules.typeField_Set("RT ");
                rules.startOfNewField_Set("^[A-Z][0-Z] ");
                rules.title_Set("T1 ");
                rules.shortTitle_Set("T1|ST ");
                rules.pTitle_Set("(JF|T2) ");
                rules.date_Set("YR ");
                rules.month_Set("FD ");
                rules.author_Set("A1 ");
                rules.pAuthor_Set("A[2-5] ");
                rules.StandardN_Set("SN ");
                rules.City_Set("PP ");
                rules.Publisher_Set("PB ");
                rules.Volume_Set("VL ");
                rules.Edition_Set("ED ");
                rules.Institution_Set(@"\\M\\w"); 
                rules.Issue_Set("IS ");
                rules.StartPage_Set("SP ");
                rules.EndPage_Set("OP ");
                rules.Availability_Set("AV ");
                rules.Url_Set("UL ");
                rules.Abstract_Set("AB ");
                rules.Notes_Set("NO ");
            }*/
            
            //myFile.Close();
            if (!checkSourceNames(this.txtb0))
            {
                SetResultMessage("");
                txtb0.Text = fileDialog.File.Name;
            }
            
            List<ItemIncomingData> res = ImportRefs.Imp(TxtFileContent, rules);
            //this.txtb1.Text = "File: " + fileDialog.File.Name + " Records: " + res.Count;
            if (res.Count > 0) b2.IsEnabled = HasWriteRights & checkSourceNames(this.txtb0);
            IIL.IncomingItems = (Csla.Core.MobileList<ItemIncomingData>)res;
            if (txtb1.Text == null || txtb1.Text.Length == 0)
            {
                SetResultMessage(IIL.IncomingItems.Count + " items found.");
            }
            IIL.FilterID = (FilterRuleCB.SelectedItem as ReadOnlyImportFilterRule).FilterID;
            IIL.buildShortTitles();
            this.grView1.ItemsSource = IIL.IncomingItems;
        }

        private void DeleteSourceForeverButton_Click(object sender, RoutedEventArgs e)
        {
            DeleteSourceForeverDialogWindow.ConfirmDeleteSourceForeverButton.Tag = (grView0.SelectedItem as ReadOnlySource).Source_ID;
            DeleteSourceForeverDialogWindow.ShowDialog();
        }

        private void UploadURLsButton_Click(object sender, RoutedEventArgs e)
        {
            fileDialog = new OpenFileDialog();
            fileDialog.Multiselect = false;
            bool? result = fileDialog.ShowDialog();

            // Open the file if OK was clicked in the dialog
            if (result == true)
            {
                ImportURLsCommand command = new ImportURLsCommand();
                System.IO.FileStream fileStream = fileDialog.File.OpenRead();
                System.IO.StreamReader myFile = new System.IO.StreamReader(fileStream);
                string line;
                while (!myFile.EndOfStream)
                {
                    line = myFile.ReadLine().Trim();
                    if (line != "")
                    {
                        if (!command.AddLine(line))
                        {
                            RadWindow.Alert("ERROR:" +Environment.NewLine
                                + "Could not digest the file correctly, please try" + Environment.NewLine
                                + "to download the file again, if the problem persists," + Environment.NewLine
                                + "please send the file to EPPISupport@ucl.ac.uk.");
                            return;
                        }
                    }
                }
                DataPortal<ImportURLsCommand> dp = new DataPortal<ImportURLsCommand>();
                dp.ExecuteCompleted += (o, e2) =>
                {
                    this.IsEnabled = true;
                    if (e2.Error != null)
                    {
                        RadWindow.Alert("ERROR:" + Environment.NewLine
                               + "The upload operation failed with an unexpected error." + Environment.NewLine
                               + "It's possible that no information was saved correctly. Please try again." + Environment.NewLine
                               + "Error details are:" + Environment.NewLine
                               + e2.Error.Message + Environment.NewLine
                               + "If the problem persists, please contact EPPISupport@ucl.ac.uk.");
                        return;
                    }
                    ImportURLsCommand RetComm = (e2.Object as ImportURLsCommand);
                    //if (RetComm == null) return;//could show an error, but if this happpens CSLA isn't working!
                    if (RetComm.Count != 0 || RetComm.Result.IndexOf("Error for item(s): ") == 0)
                    {//something is wrong, object content should only contain data associated with errors
                        string ErrorMSG = "Your upload didn't complete without errors." + Environment.NewLine;
                        ErrorMSG += "Please review the details below." + Environment.NewLine;
                        if (RetComm.Result.IndexOf("Error for item(s): ") == 0)
                        {
                            ErrorMSG += "When saving to the database, exceptions were recorded" + Environment.NewLine;
                            ErrorMSG += RetComm.Result + Environment.NewLine;
                        }
                        if (RetComm.Count != 0)
                        {
                            ErrorMSG += "The following lines of your imput file did not match" + Environment.NewLine;
                            ErrorMSG += "any item in the current review:" + Environment.NewLine;
                            ErrorMSG += RetComm.ToString();
                        }
                        RadWindow.Alert(ErrorMSG);
                    }
                };
                this.IsEnabled = false;
                dp.BeginExecute(command);
            }
        }

        private void ConfirmDeleteSourceForeverButton_Click(object sender, RoutedEventArgs e)
        {
            DeleteSourceForeverDialogWindow.Close();
            SourcesMainPaneGroup.IsEnabled = false;
            Int32 sourcetodelete ;
            Int32.TryParse(DeleteSourceForeverDialogWindow.ConfirmDeleteSourceForeverButton.Tag.ToString(), out sourcetodelete);
            DataPortal<SourceDeleteForeverCommand> dp = new DataPortal<SourceDeleteForeverCommand>();
            SourceDeleteForeverCommand command = new SourceDeleteForeverCommand(sourcetodelete);
            dp.ExecuteCompleted += (o, e2) =>
            {
                if (e2.Error != null)
                {
                    SourcesMainPaneGroup.IsEnabled = true;
                    RadWindow.Alert(e2.Error.Message);
                }
                else refreshSources();
            };
            dp.BeginExecute(command);
        }

        private void CancelDeleteSourceForeverButton_Click(object sender, RoutedEventArgs e)
        {
            DeleteSourceForeverDialogWindow.Close();
        }

        private void UserControl_GotFocus(object sender, RoutedEventArgs e)
        {
            if (DeleteSourceForeverDialogWindow.IsOpen) DeleteSourceForeverDialogWindow.Close();
        }

        
    }
}
