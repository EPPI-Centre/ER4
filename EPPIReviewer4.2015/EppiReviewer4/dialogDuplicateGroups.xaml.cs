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
using Telerik.Windows.Data;
using Telerik.Windows.Controls.GridView;
using Csla;
using Telerik.Windows.Controls;
using Csla.Xaml;


namespace EppiReviewer4
{
    public partial class dialogDuplicateGroups : UserControl
    {
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

        private RadWduplgr_AddGroup duplgr_AddGroupWindow = new RadWduplgr_AddGroup();
        private RadWduplgr_customThresholdAutoAssign duplgr_customThresholdAutoAssignWindow = new RadWduplgr_customThresholdAutoAssign();
        private RadWduplgr_Find duplgr_FindWindow = new RadWduplgr_Find();
        private RadWduplgr_ConfirmWipeOut duplgr_ConfirmWipeOutWindow = new RadWduplgr_ConfirmWipeOut();
        private RadWduplgr_DeleteGroup duplgr_DeleteGroupWindow = new RadWduplgr_DeleteGroup();
        private RadWduplgr_CreateGroupWindow1 duplgr_CreateGroupWindow1 = new RadWduplgr_CreateGroupWindow1();
        private RadWduplgr_CreateGroupWindow2 duplgr_CreateGroupWindow2 = new RadWduplgr_CreateGroupWindow2();
        public RadWGettingDuplicates1 windowGettingDuplicates1 = new RadWGettingDuplicates1();

        private SolidColorBrush pink = new SolidColorBrush(Color.FromArgb(255, 255, 105, 180));
        private SolidColorBrush light = new SolidColorBrush(Color.FromArgb(255, 255, 253, 173));
        private SolidColorBrush white = new SolidColorBrush(Colors.White);
        private SolidColorBrush orange = new SolidColorBrush(Colors.Orange);

        public dialogDuplicateGroups()
        {
            InitializeComponent();
            //add <Button x:Name="isEn" IsEnabled="{Binding HasWriteRights, Mode=OneWay}" ></Button>
            //to the xaml resources section!!
            ri = Csla.ApplicationContext.User.Identity as BusinessLibrary.Security.ReviewerIdentity;
            isEn.DataContext = this;
            //end of read-only ui hack

            //hook radwindow events
            duplgr_AddGroupWindow.duplgr_Tbox_AddGroupIDwnd_TextChange +=new EventHandler<TextChangedEventArgs>(duplgr_Tbox_AddGroupIDwnd_TextChanged);
            duplgr_AddGroupWindow.duplgr_AddGroupGOcommand_Clicked +=new EventHandler<RoutedEventArgs>(duplgr_AddGroupGOcommand_Click);
            duplgr_customThresholdAutoAssignWindow.Dupl_DoAutoAssignFromCustomWindwo_Clicked +=new EventHandler<RoutedEventArgs>(Dupl_DoAutoAssignFromCustomWindwo_Click);
            duplgr_FindWindow.duplgr_cmdFindRelatedWindow_Clicked +=new EventHandler<RoutedEventArgs>(duplgr_cmdFindRelatedWindow_Click);
            duplgr_ConfirmWipeOutWindow.duplgr_cmdWipeOutGroupsWindow_Clicked +=new EventHandler<RoutedEventArgs>(duplgr_cmdWipeOutGroupsWindow_Click);
            duplgr_DeleteGroupWindow.duplgr_cmdCancelDeleteGroup_Clicked +=new EventHandler<RoutedEventArgs>(duplgr_cmdCancelDeleteGroup_Click);
            duplgr_DeleteGroupWindow.duplgr_cmdDoDeleteGroup_Clicked +=new EventHandler<RoutedEventArgs>(duplgr_cmdDoDeleteGroup_Click);

            duplgr_CreateGroupWindow1.WindowStateChanged += new EventHandler(Helpers.WindowHelper.MaxOnly_WindowStateChanged);
            duplgr_CreateGroupWindow1.Style = Application.Current.Resources["CustomRadWindowStyle"] as Style;
            duplgr_CreateGroupWindow1.duplgr_NewGroupItemsListButton_Clicked +=new EventHandler<RoutedEventArgs>(duplgr_NewGroupItemsListButton_Click);
            duplgr_CreateGroupWindow1.duplgr_NewGroupSelectedItemsButton_Clicked +=new EventHandler<RoutedEventArgs>(duplgr_NewGroupSelectedItemsButton_Click);
            duplgr_CreateGroupWindow2.WindowStateChanged += new EventHandler(Helpers.WindowHelper.MaxOnly_WindowStateChanged);
            duplgr_CreateGroupWindow2.Style = Application.Current.Resources["CustomRadWindowStyle"] as Style;
            duplgr_CreateGroupWindow2.HyperlinkButton_Clicked +=new EventHandler<RoutedEventArgs>(HyperlinkButton_Click);
            duplgr_CreateGroupWindow2.duplgr_NewGroupradgrid_itemsList2BackB_Clicked +=new EventHandler<RoutedEventArgs>(duplgr_NewGroupradgrid_itemsList2BackB_Click);
            duplgr_CreateGroupWindow2.duplgr_NewGroupradgrid_itemsList2FinishB_Clicked +=new EventHandler<RoutedEventArgs>(duplgr_NewGroupradgrid_itemsList2FinishB_Click);
            windowGettingDuplicates1.CancelAutoProcessButton_Clicked +=new EventHandler<RoutedEventArgs>(CancelAutoProcessButton_Click);

            //end hooking

        }
        private ItemDuplicateReadOnlyGroupList ROGroupList;
        private int AutoProgressCounter = 0;
        private double AutoScoreTR = 1;
        private int CodedTR = 0, DocumentsTR = 0;
        
        
        private void CslaDataProvider_DataChanged(object sender, EventArgs e)
        {
            windowGettingDuplicates1.BusyLoading1.IsRunning = false;
            windowGettingDuplicates1.Close();
            Csla.DataPortalResult<ItemDuplicateReadOnlyGroupList> ea = new DataPortalResult<ItemDuplicateReadOnlyGroupList>(null, null, null);
            if (sender.Equals(this.Resources["GroupsList"]))
            {
                CslaDataProvider provider = ((CslaDataProvider)this.Resources["GroupsList"]);
                ea = new DataPortalResult<ItemDuplicateReadOnlyGroupList>(provider.Data as ItemDuplicateReadOnlyGroupList, provider.Error, null);
            }
            else
            {
                ea = (Csla.DataPortalResult<ItemDuplicateReadOnlyGroupList>)e;
            }
            if (ea.Object != null)
            {
                ROGroupList = ea.Object;
                GroupDuplicatesGrid1.ItemsSource = ROGroupList;
                string groupsN = (ea.Object as ItemDuplicateReadOnlyGroupList).Count.ToString();
                TextBlockDuplicateListCount1.Text = groupsN + " groups of possible duplicates loaded.";
                TextBlockCompletedListCount1.Text = ROGroupList.CompletedCount + " Group" + (ROGroupList.CompletedCount == 1 ? " " : "s ") + "marked as completed.";
                windowGettingDuplicates1.TotalTxt1.Text = groupsN;
                ItemDuplicateGroup oldselGo = GroupDetails1.DataContext as ItemDuplicateGroup;
                int oldSelG = -33;
                if (oldselGo != null)
                {
                     oldSelG = oldselGo.GroupID;
                }
                ItemDuplicateReadOnlyGroup firstTodo = null;
                List<ItemDuplicateReadOnlyGroup> lis = new List<ItemDuplicateReadOnlyGroup>();
                foreach (ItemDuplicateReadOnlyGroup cg in ROGroupList)
                {
                    if (cg.GroupId == oldSelG)
                    {
                        lis.Add(cg);
                            
                        oldSelG = -22;
                        break;
                    }
                    else if (firstTodo == null && cg.IsComplete == false)
                    {
                        firstTodo = cg;
                        lis.Add(firstTodo);
                        if (oldSelG == -33) break;
                    }
                }
                if (lis.Count == 0)
                {
                    if (ROGroupList.Count == 0)
                    {
                        MasterDetails1.DataContext = null;
                        GroupDetails1.DataContext = null;
                        GroupDetails1.ItemsSource = new ItemDuplicateGroup();
                        duplgr_radgrid_AdvgroupMembers.DataContext = null;
                        duplgr_radgrid_AdvgroupMembers.ItemsSource = null;
                        MasterDetails2.DataContext = null;
                        duplgr_radgrid_ManualMembers.ItemsSource = null;
                        duplgr_radgrid_MainManualMembers.ItemsSource = null;
                    }
                    else
                    {
                        GroupDuplicatesGrid1.SelectedItem = ROGroupList[0];
                        GroupDuplicatesGrid1.BringIndexIntoView(GroupDuplicatesGrid1.Items.IndexOf(ROGroupList[0]));
                    }
                }
                else
                { 
                    GroupDuplicatesGrid1.SelectedItem = lis[0];
                    GroupDuplicatesGrid1.BringIndexIntoView(GroupDuplicatesGrid1.Items.IndexOf(lis[0])); 
                }
                
            }
            else if (ea.Error != null)
            {
                TextBlockDuplicateListCount1.Text = "";
                if (ea.Error.Message == "DataPortal.Fetch failed (Execution still Running)")
                {
                    GroupDuplicatesGrid1.ItemsSource = null;
                    ((this.Parent as Grid).Parent as Telerik.Windows.Controls.RadWindow).Close();
                    Telerik.Windows.Controls.RadWindow.Alert("Sorry, the duplicate checking is still running on the background."
                        + System.Environment.NewLine + "Unfortunately long execution times are to be expected"
                        + System.Environment.NewLine + "when checking thousands of items."
                        + System.Environment.NewLine + "The 'Manage duplicates' function will be accessible as soon as the"
                        + System.Environment.NewLine + "system has finished compiling your list of possible duplicates."
                        + System.Environment.NewLine + "In the meantime, the rest of the programs functions are available."
                        + System.Environment.NewLine + "If this operation has taken longer than 12 hours"
                        + System.Environment.NewLine + "please contact us at eppisupport@ioe.ac.uk.");
                }
                else if(ea.Error.Message.IndexOf("Previous Execution failed.") != -1)
                {
                    ((this.Parent as Grid).Parent as Telerik.Windows.Controls.RadWindow).Close();
                    string tmp = ea.Error.Message.Replace("DataPortal.Fetch failed (", "");
                    Telerik.Windows.Controls.RadWindow.Alert(tmp.Trim(')'));
                }
                else
                {
                    ((this.Parent as Grid).Parent as Telerik.Windows.Controls.RadWindow).Close();
                    Telerik.Windows.Controls.RadWindow.Alert(ea.Error.Message);
                }
            }
            else
            {
                GroupDuplicatesGrid1.ItemsSource = null;
                ((this.Parent as Grid).Parent as Telerik.Windows.Controls.RadWindow).Close();
                    Telerik.Windows.Controls.RadWindow.Alert("Unspecified error, please get in contact with the Support Team");
            }
        }

        private void cmdRefreshDuplicateList_Click(object sender, RoutedEventArgs e)
        {
            //MasterDetails1.DataContext = null;
            
            //GroupDetails1.DataContext = null;
            //GroupDetails1.ItemsSource = new ItemDuplicateReadOnlyGroupList();
            //MasterDetails2.DataContext = null;

            duplgr_radgrid_AdvgroupMembers.DataContext = null;
            duplgr_radgrid_AdvgroupMembers.ItemsSource = new ItemDuplicateReadOnlyGroupList();
            //FilterDescriptor descriptor = new FilterDescriptor();
            //descriptor.Member = "IsMaster";
            //descriptor.Operator = FilterOperator.IsEqualTo;
            //descriptor.Value = "False";
            //GroupDetails1.FilterDescriptors.Clear();
            //GroupDetails1.FilterDescriptors.Add(descriptor);
            //GroupDetails1.Rebind();
            //duplgr_radgrid_AdvgroupMembers.FilterDescriptors.Clear();
            //duplgr_radgrid_AdvgroupMembers.FilterDescriptors.Add(descriptor);
            //duplgr_radgrid_AdvgroupMembers.Rebind();
            RefreshDuplicates();
            
        }
        public void RefreshDuplicates()
        {
            windowGettingDuplicates1.Header = "Refreshing Duplicates List...";
            windowGettingDuplicates1.BusyLoading1.IsRunning = true;
            CslaDataProvider provider = ((CslaDataProvider)this.Resources["GroupsList"]);
            //Csla.DataPortal<ItemDuplicateReadOnlyGroupList> dp = new Csla.DataPortal<ItemDuplicateReadOnlyGroupList>();
            //dp.FetchCompleted -= CslaDataProvider_DataChanged; 
            //dp.FetchCompleted += CslaDataProvider_DataChanged;
            //dp.BeginFetch(new Csla.SingleCriteria<ItemDuplicateReadOnlyGroupList, bool>(false));
            if (!provider.IsBusy) provider.Refresh();
            windowGettingDuplicates1.ShowDialog();
            windowGettingDuplicates1.BringToFront();
        }
        public void RefreshDuplicates(GroupListSelectionCriteria Criteria)
        {
            windowGettingDuplicates1.Header = "Refreshing Duplicates List...";
            windowGettingDuplicates1.BusyLoading1.IsRunning = true;
            windowGettingDuplicates1.ShowDialog();
            Csla.DataPortal<ItemDuplicateReadOnlyGroupList> dp = new Csla.DataPortal<ItemDuplicateReadOnlyGroupList>();
            dp.FetchCompleted -= CslaDataProvider_DataChanged;
            dp.FetchCompleted += CslaDataProvider_DataChanged;
            dp.BeginFetch(Criteria);
        }
        private void cmdGetNewDuplicates_Click(object sender, RoutedEventArgs e)
        {
            DialogParameters parameters = new DialogParameters();
            parameters.Header = "Get New Duplicates?";
            parameters.Content = "Are you sure you want to Get New Duplicates?"
                + Environment.NewLine + "This procedure can take a long time, hours for "
                + Environment.NewLine + "reviews with 20,000 references or more."
                + Environment.NewLine + "If your review contains more than 200,000"
                + Environment.NewLine + "references please contact us (EPPISupport@ioe.ac.uk).";
            parameters.Closed = cmdGetNewDuplicatesDialogClosed;
            RadWindow.Confirm(parameters);
        }
        private void cmdGetNewDuplicatesDialogClosed(object sender, WindowClosedEventArgs e)
        {
            if (e.DialogResult == true)
            {
                windowGettingDuplicates1.Header = "Identifying duplicates (this can take quite a while)...";
                windowGettingDuplicates1.BusyLoading1.IsRunning = true;
                windowGettingDuplicates1.ShowDialog();
                Csla.DataPortal<ItemDuplicateReadOnlyGroupList> dp = new Csla.DataPortal<ItemDuplicateReadOnlyGroupList>();
                dp.FetchCompleted -= CslaDataProvider_DataChanged;
                dp.FetchCompleted += CslaDataProvider_DataChanged;
                dp.BeginFetch(new Csla.SingleCriteria<ItemDuplicateReadOnlyGroupList, bool>(true));
            }
        }
        private void GroupDuplicatesGrid_SelectionChanged(object sender, Telerik.Windows.Controls.SelectionChangeEventArgs e)
        {
            if (GroupDuplicatesGrid1.Items != null && GroupDuplicatesGrid1.SelectedItem != null)
            {
                int id = (GroupDuplicatesGrid1.SelectedItem as ItemDuplicateReadOnlyGroup).GroupId;
                duplgr_AddGroupWindow.duplgr_tblGroupIDAddGroupWindow.Text = id.ToString();
                Csla.DataPortal<ItemDuplicateGroup> dp = new Csla.DataPortal<ItemDuplicateGroup>();
                dp.FetchCompleted += new EventHandler<Csla.DataPortalResult<ItemDuplicateGroup>>(dp_FetchCompleted);
                dp.BeginFetch(new Csla.SingleCriteria<ItemDuplicateGroup, int>(id));
            }
            else GroupDetails1.ItemsSource = null;
        }

        void dp_FetchCompleted(object sender, Csla.DataPortalResult<ItemDuplicateGroup> e)
        {
            if (e.Error != null)
            {
                ((this.Parent as Grid).Parent as Telerik.Windows.Controls.RadWindow).Close();
                Telerik.Windows.Controls.RadWindow.Alert("Could not Retrieve the current group details."
                        + Environment.NewLine + e.Error.Message + Environment.NewLine + "Please Contact the support team.");
            }
            else
            {
                ItemDuplicateGroup IDG = (e.Object as ItemDuplicateGroup);
                GroupDetails1.DataContext = IDG;
                GroupDetails1.IsEnabled = IDG.IsEditable;
                duplgr_radgrid_AdvgroupMembers.DataContext = IDG;
                GroupDetails1.ItemsSource = IDG.Members;
                duplgr_radgrid_AdvgroupMembers.ItemsSource = IDG.Members;
                FilterDescriptor descriptor = new FilterDescriptor();
                descriptor.Member = "IsMaster";
                descriptor.Operator = FilterOperator.IsEqualTo;
                descriptor.Value = "False";
                GroupDetails1.FilterDescriptors.Clear();
                duplgr_radgrid_AdvgroupMembers.FilterDescriptors.Clear();
                GroupDetails1.FilterDescriptors.Add(descriptor);
                duplgr_radgrid_AdvgroupMembers.FilterDescriptors.Add(descriptor);
                GroupDetails1.Rebind();
                duplgr_radgrid_AdvgroupMembers.Rebind();
                ItemDuplicateGroupMember gm = IDG.getMaster();
                MasterDetails1.DataContext = gm;
                MasterDetails2.DataContext = gm;
                duplgr_radgrid_ManualMembers.ItemsSource = IDG.ManualMembers;
                duplgr_radgrid_MainManualMembers.ItemsSource = IDG.ManualMembers;
                if (GroupDetails1.Items.Count > 0) 
                    GroupDetails1.SelectedItem = GroupDetails1.Items[0];
            }
        }

        private void GroupDetails_RowLoaded(object sender, Telerik.Windows.Controls.GridView.RowLoadedEventArgs e)
        {
            GridViewRow row = e.Row as GridViewRow;
            if (row == null) return;
            row.RemoveHandler(GridViewRow.MouseLeftButtonUpEvent, new MouseButtonEventHandler(row_MouseLeftButtonUp));
            row.AddHandler(GridViewRow.MouseLeftButtonUpEvent, new MouseButtonEventHandler(row_MouseLeftButtonUp), true); 
            
        }

        void row_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            GridViewRow row = sender as GridViewRow;
            //if (row.IsSelected) row.IsSelected = false;
            if (row.DetailsVisibility == null || row.DetailsVisibility == Visibility.Collapsed) row.DetailsVisibility = Visibility.Visible;
            //else row.DetailsVisibility = Visibility.Collapsed;
        }
        
        public void ChangeMaster(object sender, RoutedEventArgs e)
        {
            ItemDuplicateGroup IDG = GroupDetails1.DataContext as ItemDuplicateGroup;
            long originalMaster = IDG.getMaster().ItemId;
            Csla.Core.MobileList<ItemDuplicateGroupMember> list = IDG.Members;
            foreach (ItemDuplicateGroupMember gm in list)
            {
                if (gm.ItemId == originalMaster)
                {
                    gm.IsMaster = false;
                    gm.IsChecked = false;
                    gm.IsDuplicate = false;
                }
                else if (gm.ItemId == ((sender as Button).DataContext as ItemDuplicateGroupMember).ItemId)
                {
                    gm.IsMaster = true;
                    gm.IsChecked = true;
                    gm.IsDuplicate = false;
                    MasterDetails1.DataContext = gm;
                }
            }
            IDG.Members = null;
            IDG.Members = list;
            
            //GroupDetails1.DataContext = IDG;
            //GroupDetails1.ItemsSource = IDG.Members;
            //GroupDetails1.Rebind();
            if (IDG.isComplete() != (GroupDuplicatesGrid1.SelectedItem as ItemDuplicateReadOnlyGroup).IsComplete)
            {
                (GroupDuplicatesGrid1.SelectedItem as ItemDuplicateReadOnlyGroup).IsComplete = !(GroupDuplicatesGrid1.SelectedItem as ItemDuplicateReadOnlyGroup).IsComplete;
                TextBlockCompletedListCount1.Text = ROGroupList.CompletedCount + " Group" + (ROGroupList.CompletedCount == 1 ? " " : "s ") + "marked as completed.";
                //GroupDuplicatesGrid1.Rebind();
               
            }
            GoOn(IDG);
            //IDG.BeginSave(true);
            
        }
        public void MarkAsDuplicate(object sender, RoutedEventArgs e)
        {
            ItemDuplicateGroup IDG = GroupDetails1.DataContext as ItemDuplicateGroup;
            Csla.Core.MobileList<ItemDuplicateGroupMember> list = IDG.Members;
            foreach (ItemDuplicateGroupMember gm in list)
            {
                if (gm.ItemId == ((sender as Button).DataContext as ItemDuplicateGroupMember).ItemId)
                {
                    gm.IsChecked = true;
                    gm.IsDuplicate = true;
                }
            }
            IDG.Members = null;
            IDG.Members = list;
            //GroupDetails1.DataContext = IDG;
            //GroupDetails1.ItemsSource = IDG.Members;
            //GroupDetails1.Rebind();
            //IDG.BeginSave(true);

            if (IDG.isComplete() != (GroupDuplicatesGrid1.SelectedItem as ItemDuplicateReadOnlyGroup).IsComplete)
            {
                (GroupDuplicatesGrid1.SelectedItem as ItemDuplicateReadOnlyGroup).IsComplete = true;
                TextBlockCompletedListCount1.Text = ROGroupList.CompletedCount + " Group" + (ROGroupList.CompletedCount == 1 ? " " : "s ") + "marked as completed.";
                //GroupDuplicatesGrid1.Rebind();
            }
            GoOn(IDG);
        }
        public void UnMarkAsDuplicate(object sender, RoutedEventArgs e)
        {
            ItemDuplicateGroup IDG = GroupDetails1.DataContext as ItemDuplicateGroup;
            Csla.Core.MobileList<ItemDuplicateGroupMember> list = IDG.Members;
            foreach (ItemDuplicateGroupMember gm in list)
            {
                if (gm.ItemId == ((sender as Button).DataContext as ItemDuplicateGroupMember).ItemId)
                {
                    gm.IsChecked = true;
                    gm.IsDuplicate = false;
                }
            }
            IDG.Members = null;
            IDG.Members = list;
            //GroupDetails1.DataContext = IDG;
            //GroupDetails1.ItemsSource = IDG.Members;
            //GroupDetails1.Rebind();
            //IDG.BeginSave(true);
            if (IDG.isComplete() != (GroupDuplicatesGrid1.SelectedItem as ItemDuplicateReadOnlyGroup).IsComplete)
            {
                (GroupDuplicatesGrid1.SelectedItem as ItemDuplicateReadOnlyGroup).IsComplete = true;
                TextBlockCompletedListCount1.Text = ROGroupList.CompletedCount + " Group" + (ROGroupList.CompletedCount == 1 ? " " : "s ") + "marked as completed.";
                //GroupDuplicatesGrid1.Rebind();
            }
            GoOn(IDG);
        }
        private void GoOn (ItemDuplicateGroup IDg)
        {
            IDg.Saved -= IDg_Saved;
            IDg.Saved += new EventHandler<Csla.Core.SavedEventArgs>(IDg_Saved);
            windowGettingDuplicates1.Header = "Refreshing Group";
            windowGettingDuplicates1.BusyLoading1.IsRunning = true;
            windowGettingDuplicates1.ShowDialog();
            IDg.BeginSave(true);
        }

        void IDg_Saved(object sender, Csla.Core.SavedEventArgs e)
        {
            windowGettingDuplicates1.Close();
            windowGettingDuplicates1.BusyLoading1.IsRunning = false;
            windowGettingDuplicates1.Header = "Refreshing Duplicates List...";
             if (e.Error != null )
            {
                Telerik.Windows.Controls.RadWindow.Alert("The Last Command FAILED:" + Environment.NewLine
                    + e.Error.Message + Environment.NewLine + "Please contact the support team.");
            }
             else if (e.NewObject as ItemDuplicateGroup != null)
             {
                 //int selI = -1;
                 //if (GroupDetails1.Items.Count > 0 && GroupDetails1.SelectedItem != null) selI = (GroupDetails1.SelectedItem as ItemDuplicateGroupMember).ItemDuplicateId;

                 GroupDetails1.ItemsSource = null;
                 GroupDetails1.SelectedItem = null;
                 ItemDuplicateGroup IDG = (e.NewObject as ItemDuplicateGroup);
                 GroupDetails1.DataContext = IDG;
                 duplgr_radgrid_AdvgroupMembers.DataContext = IDG;
                 GroupDetails1.ItemsSource = IDG.Members;
                 duplgr_radgrid_AdvgroupMembers.ItemsSource = IDG.Members;
                 FilterDescriptor descriptor = new FilterDescriptor();
                 descriptor.Member = "IsMaster";
                 descriptor.Operator = FilterOperator.IsEqualTo;
                 descriptor.Value = "False";
                 GroupDetails1.FilterDescriptors.Clear();
                 duplgr_radgrid_AdvgroupMembers.FilterDescriptors.Clear();
                 GroupDetails1.FilterDescriptors.Add(descriptor);
                 duplgr_radgrid_AdvgroupMembers.FilterDescriptors.Add(descriptor);
                 GroupDetails1.Rebind();
                 duplgr_radgrid_AdvgroupMembers.Rebind();
                 ItemDuplicateGroupMember gm = IDG.getMaster();
                 MasterDetails1.DataContext = gm;
                 MasterDetails2.DataContext = gm;
                 duplgr_radgrid_ManualMembers.ItemsSource = IDG.ManualMembers;
                 duplgr_radgrid_MainManualMembers.ItemsSource = IDG.ManualMembers;
                 if (GroupDetails1.SelectedItem == null)
                 {
                    foreach (ItemDuplicateGroupMember ggm in GroupDetails1.Items)
                    {
                        if (!ggm.IsChecked)
                        {
                            GroupDetails1.SelectedItem = ggm;
                            GroupDetails1.BringIndexIntoView(GroupDetails1.Items.IndexOf(ggm));
                            break;
                        }
                    }
                    if (GroupDetails1.SelectedItem == null && GroupDetails1.Items != null && GroupDetails1.Items.Count != 0)
                    {
                        GroupDetails1.SelectedItem = (GroupDetails1.Items[0]);
                        GroupDetails1.BringIndexIntoView(0);
                    }
                 }
             }
             else 
             {
                 Telerik.Windows.Controls.RadWindow.Alert("The Last Command FAILED:" + Environment.NewLine
                    + "No error was returned, but updated details of current group" + 
                    Environment.NewLine + "could not be retrieved." + Environment.NewLine + "Please contact the support team.");
             }
        }
        
        private void detailsTmp_ScoreTxtLoaded(object sender, RoutedEventArgs e)
        {
            if (sender != null && GroupDetails1.DataContext != null && sender.GetType() == typeof(TextBlock))
            {
                if ((GroupDetails1.DataContext as ItemDuplicateGroup).ShowScores() )
                    (sender as TextBlock).Visibility = System.Windows.Visibility.Visible;
                else 
                    (sender as TextBlock).Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void AutoAssign_Click(object sender, RoutedEventArgs e)
        {
            windowGettingDuplicates1.Header = "Auto assign in progress, please wait...";
            windowGettingDuplicates1.BusyLoading1.IsRunning = true;
            windowGettingDuplicates1.AutoProgress1.Visibility = System.Windows.Visibility.Visible;
            windowGettingDuplicates1.DoneTxt1.Text = "0";
            windowGettingDuplicates1.TotalTxt1.Text = "...";
            windowGettingDuplicates1.ShowDialog();
            AutoProcessGroup();            
        }
        private void AutoProcessGroup()
        {
            AutoProgressCounter = 0;
            if (ROGroupList != null && ROGroupList.Count > AutoProgressCounter)
            {
                RefreshDuplicates4Auto();
            }
            else
            {
                //GroupDetails.ItemsSource = null;
            }
        }
        
        public void RefreshDuplicates4Auto()
        {
            Csla.DataPortal<ItemDuplicateReadOnlyGroupList> dp = new Csla.DataPortal<ItemDuplicateReadOnlyGroupList>();
            dp.FetchCompleted -= CslaDataProvider_DataChanged4Auto;
            dp.FetchCompleted += CslaDataProvider_DataChanged4Auto;
            dp.BeginFetch(new Csla.SingleCriteria<ItemDuplicateReadOnlyGroupList, bool>(false));
        }
        private void CslaDataProvider_DataChanged4Auto(object sender, EventArgs e)
        {
            Csla.DataPortalResult<ItemDuplicateReadOnlyGroupList> ea = (Csla.DataPortalResult<ItemDuplicateReadOnlyGroupList>)e;
            if (ea.Object != null)
            {
                ROGroupList = ea.Object;
                windowGettingDuplicates1.TotalTxt1.Text = ROGroupList.Count.ToString();
                GroupDuplicatesGrid1.ItemsSource = ROGroupList;
                int id = getNextGroupToProcess();
                if (id > 0)
                {
                    Csla.DataPortal<ItemDuplicateGroup> dp = new Csla.DataPortal<ItemDuplicateGroup>();
                    dp.FetchCompleted += new EventHandler<Csla.DataPortalResult<ItemDuplicateGroup>>(dp_FetchCompleted_4autoAssign);
                    dp.BeginFetch(new Csla.SingleCriteria<ItemDuplicateGroup, int>(id));
                }
                else
                {
                    EndAutoAssign();
                }
            }
            else if (ea.Error != null)
            {
                EndAutoAssign();
                Telerik.Windows.Controls.RadWindow.Alert("MARK AUTOMATICALLY FAILED:" + Environment.NewLine + "Could not refresh the current group list."
                        + Environment.NewLine + ea.Error.Message + Environment.NewLine + "Please contact the support team.");
            }
            else
            {
                EndAutoAssign();
                Telerik.Windows.Controls.RadWindow.Alert("MARK AUTOMATICALLY FAILED:" + Environment.NewLine + "Could not refresh the current group list."
                        + Environment.NewLine + "List is empty but no error was raised" + Environment.NewLine + "Please contact the support team.");
            }
        }
        private int getNextGroupToProcess()
        {
            while (AutoProgressCounter < ROGroupList.Count)
            {
                if (!ROGroupList[AutoProgressCounter].IsComplete) return ROGroupList[AutoProgressCounter].GroupId;
                AutoProgressCounter++;
                windowGettingDuplicates1.DoneTxt1.Text = AutoProgressCounter.ToString();
            }
            return -1;
        }
        void dp_FetchCompleted_4autoAssign(object sender, Csla.DataPortalResult<ItemDuplicateGroup> e)
        {
            if (e.Error != null)
            {
                EndAutoAssign();
                Telerik.Windows.Controls.RadWindow.Alert("MARK AUTOMATICALLY FAILED:" +Environment.NewLine + "Could not Retrieve the current group details."
                        + Environment.NewLine + e.Error.Message + Environment.NewLine + "Please Contact the support team.");
            }
            else
            {
                bool toSave = false;
                ItemDuplicateGroup IDG = (e.Object as ItemDuplicateGroup);
                ItemDuplicateGroupMember gm = IDG.getMaster();
                Csla.Core.MobileList<ItemDuplicateGroupMember> list = IDG.Members;
                foreach (ItemDuplicateGroupMember cm in list)
                {
                    
                    if (
                            !cm.IsMaster 
                            && !cm.IsDuplicate 
                            && !cm.IsChecked
                            && !cm.IsExported
                            && cm.SimilarityScore >= AutoScoreTR 
                            && cm.CodedCount <= CodedTR 
                            && cm.DocCount <= DocumentsTR
                            && IDG.ShowScores() 
                        )
                    {
                        cm.IsChecked = true;
                        cm.IsDuplicate = true;
                        toSave = true;
                    }
                }
                if (toSave)
                {
                    IDG.Members = null;
                    IDG.Members = list;
                    IDG.Saved += new EventHandler<Csla.Core.SavedEventArgs>(IDG_Saved_4autoAssign);
                    IDG.BeginSave(true);
                }
                else
                {
                    IDG_Saved_4autoAssign (this, new Csla.Core.SavedEventArgs(new ItemDuplicateGroup(), null, null));
                }
            }
        }

        void IDG_Saved_4autoAssign(object sender, Csla.Core.SavedEventArgs e)
        {
            if (e.Error != null)
            {
                EndAutoAssign();
                Telerik.Windows.Controls.RadWindow.Alert("MARK AUTOMATICALLY FAILED:" + Environment.NewLine + "Could not Save the current group details."
                        + Environment.NewLine + e.Error.Message + Environment.NewLine + "Please Contact the support team.");
            }
            else
            {
                AutoProgressCounter++;
                windowGettingDuplicates1.DoneTxt1.Text = AutoProgressCounter.ToString();
                int id = getNextGroupToProcess();
                if (ROGroupList != null && ROGroupList.Count > AutoProgressCounter && id > 0)
                {
                    Csla.DataPortal<ItemDuplicateGroup> dp = new Csla.DataPortal<ItemDuplicateGroup>();
                    dp.FetchCompleted += new EventHandler<Csla.DataPortalResult<ItemDuplicateGroup>>(dp_FetchCompleted_4autoAssign);
                    dp.BeginFetch(new Csla.SingleCriteria<ItemDuplicateGroup, int>(id));
                }
                else if (ROGroupList.Count <= AutoProgressCounter)
                {
                    EndAutoAssign();
                }
            }
        }
        private void EndAutoAssign()
        {
            AutoProgressCounter = 0;
            AutoScoreTR = 1;
            CodedTR = 0;
            DocumentsTR = 0;
            windowGettingDuplicates1.AutoProgress1.Visibility = System.Windows.Visibility.Collapsed;
            windowGettingDuplicates1.Header = "Identifying duplicates (this can take quite a while)...";
            windowGettingDuplicates1.BusyLoading1.IsRunning = false;
            windowGettingDuplicates1.Close();
            RefreshDuplicates();
        }

        private void CancelAutoProcessButton_Click(object sender, RoutedEventArgs e)
        {
            AutoProgressCounter = ROGroupList.Count;
        }

        private void windowGettingDuplicates_LostFocus(object sender, RoutedEventArgs e)
        {
            if(windowGettingDuplicates1.IsOpen) windowGettingDuplicates1.BringToFront();
        }      
        public void getItemsListData(DataItemCollection AllItems, System.Collections.ObjectModel.ObservableCollection<object> selected)
        {
            duplgr_radgrid_itemsList.ItemsSource = AllItems;
            foreach (Item it in selected)
            {
                duplgr_radgrid_itemsList.SelectedItems.Add(it);
            }
        }
        private void cmdDuplGrRemoveDupl_Click(object sender, RoutedEventArgs e)
        {
            ItemDuplicateManualGroupMember it = (sender as Control).DataContext as ItemDuplicateManualGroupMember;
            ItemDuplicateGroup IDG = GroupDetails1.DataContext as ItemDuplicateGroup;
            IDG.RemoveItemID = it.ItemId;
            IDG.Saved += new EventHandler<Csla.Core.SavedEventArgs>(IDG_Saved);
            IDG.BeginSave();
        }
        private void duplgr_addSelectedItemsButton_Click(object sender, RoutedEventArgs e)
        {
            ItemDuplicateGroup IDG = GroupDetails1.DataContext as ItemDuplicateGroup;
            if (IDG == null || duplgr_radgrid_itemsList.SelectedItems.Count == 0) return;
            long currmas = IDG.getMaster().ItemId;
            IDG.AddItems = new Csla.Core.MobileList<long>();
            foreach (Item it in duplgr_radgrid_itemsList.SelectedItems)
            {
                if(it.ItemId != currmas) IDG.AddItems.Add(it.ItemId);
            }
            IDG.Saved += new EventHandler<Csla.Core.SavedEventArgs>(IDG_Saved);
            IDG.BeginSave();
        }

        void IDG_Saved(object sender, Csla.Core.SavedEventArgs e)
        {
            windowGettingDuplicates1.Close();
            duplgr_AddGroupWindow.Close();
            duplgr_customThresholdAutoAssignWindow.Close();
            if (e.Error != null)
            {
                ((this.Parent as Grid).Parent as Telerik.Windows.Controls.RadWindow).Close();
                Telerik.Windows.Controls.RadWindow.Alert("Could not Retrieve the current group details."
                        + Environment.NewLine + e.Error.Message + Environment.NewLine + "Please Contact the support team.");
            }
            else
            {
                ItemDuplicateGroup IDG = (e.NewObject as ItemDuplicateGroup);
                GroupDetails1.DataContext = IDG;
                duplgr_radgrid_AdvgroupMembers.DataContext = IDG;
                GroupDetails1.ItemsSource = IDG.Members;
                duplgr_radgrid_AdvgroupMembers.ItemsSource = IDG.Members;
                FilterDescriptor descriptor = new FilterDescriptor();
                descriptor.Member = "IsMaster";
                descriptor.Operator = FilterOperator.IsEqualTo;
                descriptor.Value = "False";
                GroupDetails1.FilterDescriptors.Clear();
                duplgr_radgrid_AdvgroupMembers.FilterDescriptors.Clear();
                GroupDetails1.FilterDescriptors.Add(descriptor);
                duplgr_radgrid_AdvgroupMembers.FilterDescriptors.Add(descriptor);
                GroupDetails1.Rebind();
                duplgr_radgrid_AdvgroupMembers.Rebind();
                ItemDuplicateGroupMember gm = IDG.getMaster();
                MasterDetails1.DataContext = gm;
                MasterDetails2.DataContext = gm;
                duplgr_radgrid_ManualMembers.ItemsSource = IDG.ManualMembers;
                duplgr_radgrid_MainManualMembers.ItemsSource = IDG.ManualMembers;
            }
        }
        private void duplgr_Tbox_AddGroupIDwnd_TextChanged(object sender, TextChangedEventArgs e)
        {
            int i;
            if (Int32.TryParse(duplgr_AddGroupWindow.duplgr_Tbox_AddGroupIDwnd.Text, out i))
            {
                foreach (ItemDuplicateReadOnlyGroup idRO in (GroupDuplicatesGrid1.ItemsSource as ItemDuplicateReadOnlyGroupList))
                {
                    if (i == idRO.GroupId)
                    {
                        duplgr_AddGroupWindow.duplgr_AddGroupGOcommand.IsEnabled = true;
                        return;
                    }
                }

            }
            duplgr_AddGroupWindow.duplgr_AddGroupGOcommand.IsEnabled = false;
        }

        private void duplgr_AddGroupGOcommand_Click(object sender, RoutedEventArgs e)
        {
            ItemDuplicateGroup IDG = GroupDetails1.DataContext as ItemDuplicateGroup;
            IDG.AddGroupID = Int32.Parse(duplgr_AddGroupWindow.duplgr_Tbox_AddGroupIDwnd.Text);
            IDG.Saved += new EventHandler<Csla.Core.SavedEventArgs>(IDG_Saved);
            IDG.BeginSave();
        }

        private void duplgr_addGroupToSelectedButton_Click(object sender, RoutedEventArgs e)
        {
            ItemDuplicateGroup IDG = GroupDetails1.DataContext as ItemDuplicateGroup;
            if (IDG == null) return;
            duplgr_AddGroupWindow.ShowDialog();
        }

        private void duplgr_AutoAssignWithCustomThresholdButton_Click(object sender, RoutedEventArgs e)
        {
            duplgr_customThresholdAutoAssignWindow.ShowDialog();
        }

        private void Dupl_DoAutoAssignFromCustomWindwo_Click(object sender, RoutedEventArgs e)
        {
            duplgr_customThresholdAutoAssignWindow.Close();
            AutoScoreTR = Convert.ToDouble(duplgr_customThresholdAutoAssignWindow.duplGr_SimilarityThresholdnum.Value);
            CodedTR = Convert.ToInt32(duplgr_customThresholdAutoAssignWindow.duplGr_codedThresholdnum.Value);
            DocumentsTR = Convert.ToInt32(duplgr_customThresholdAutoAssignWindow.duplGr_DocumentsThresholdnum.Value);
            AutoAssign_Click(sender, e);
        }

        private void duplgr_Find_Click(object sender, RoutedEventArgs e)
        {
            duplgr_FindWindow.ShowDialog();
        }

        private void duplgr_cmdFindRelatedWindow_Click(object sender, RoutedEventArgs e)
        {
            Button send = sender as Button;
            ItemDuplicateGroup IDG = (GroupDetails1.DataContext as ItemDuplicateGroup);
            BusinessLibrary.BusinessClasses.GroupListSelectionCriteria sel;
            if ((IDG == null && send.Tag.ToString() != "Selected") || send == null) return;
            //case 1 "Find Related"
            if (send.Tag.ToString() == "Related")
            {
                sel = new GroupListSelectionCriteria(typeof(ItemDuplicateReadOnlyGroupList), IDG.GroupID);
            }
            //case 2 "Find Selected"
            else if (send.Tag.ToString() == "Selected")
            {
                string IDs = "";
                foreach(Item It in duplgr_radgrid_itemsList.SelectedItems)
                {
                    IDs += It.ItemId.ToString() + ",";
                }
                if (IDs == "") return;
                sel = new GroupListSelectionCriteria(typeof(ItemDuplicateReadOnlyGroupList), IDs);
            }
            //case 3 "Find IDs"
            else 
            {
                //step 1, parse user's input
                Int64 chk;
                string ToSend = "";
                string[] IDs = duplgr_FindWindow.duplgr_tbIDlistWindow.Text.Split(',');
                foreach (string ID in IDs)
                {
                    try
                    {
                        chk = Int64.Parse(ID);
                        if (chk > 0) ToSend += ID + ",";
                    }
                    catch { chk = -1; }
                }
                if (ToSend == "") return;
                sel = new GroupListSelectionCriteria(typeof(ItemDuplicateReadOnlyGroupList), ToSend);
            }
            duplgr_FindWindow.Close();
            RefreshDuplicates(sel);
        }

        private void duplgr_cmdWipeOutGroupsWindow_Click(object sender, RoutedEventArgs e)
        {
            Button bt = sender as Button;
            ItemDuplicateGroupsDeleteCommand command = new ItemDuplicateGroupsDeleteCommand(false);
            if (bt == null) return;
            else if (bt.Tag.ToString() == "All")
            {//wipe all data, we don't evaluate the other option: if bt.Tag.ToString() == "Groups" because it's the default set in the command declaration
                command = new ItemDuplicateGroupsDeleteCommand(true);
            }
            duplgr_ConfirmWipeOutWindow.duplgr_ConfirmWipeOutBusyWindow.IsRunning = true;
            duplgr_ConfirmWipeOutWindow.duplgr_tbRemoveGroupsWindow.IsEnabled = false;
            duplgr_ConfirmWipeOutWindow.duplgr_tbWipeAllWindow.IsEnabled = false;
            duplgr_ConfirmWipeOutWindow.duplgr_cmdWipeOutGroupsWindow.IsEnabled = false;
            duplgr_ConfirmWipeOutWindow.duplgr_cmdWipeAllWindow.IsEnabled = false;
            DataPortal<ItemDuplicateGroupsDeleteCommand> dp = new DataPortal<ItemDuplicateGroupsDeleteCommand>();
            
            dp.ExecuteCompleted += (o, e2) =>
            {
                duplgr_ConfirmWipeOutWindow.duplgr_ConfirmWipeOutBusyWindow.IsRunning = false;
                duplgr_ConfirmWipeOutWindow.duplgr_tbRemoveGroupsWindow.IsEnabled = true;
                duplgr_ConfirmWipeOutWindow.duplgr_tbWipeAllWindow.IsEnabled = true;
                duplgr_ConfirmWipeOutWindow.duplgr_cmdWipeOutGroupsWindow.IsEnabled = true;
                duplgr_ConfirmWipeOutWindow.duplgr_cmdWipeAllWindow.IsEnabled = true;
                duplgr_ConfirmWipeOutWindow.Close();
                if (e2.Error != null)
                {
                    Telerik.Windows.Controls.RadWindow.Alert(e2.Error.Message);
                }
                else
                {
                    RefreshDuplicates();
                    //cmdRefreshDuplicateList_Click(o, e);
                }
                
            };
            dp.BeginExecute(command);
        }

        private void duplgr_WipeButton_Click(object sender, RoutedEventArgs e)
        { 
            duplgr_ConfirmWipeOutWindow.duplgr_tbRemoveGroupsWindow.Text = "";
            duplgr_ConfirmWipeOutWindow.duplgr_tbWipeAllWindow.Text = "";
            duplgr_ConfirmWipeOutWindow.duplgr_cmdWipeOutGroupsWindow.IsEnabled = false;
            duplgr_ConfirmWipeOutWindow.duplgr_cmdWipeAllWindow.IsEnabled = false;
            duplgr_ConfirmWipeOutWindow.ShowDialog();
        }

        //private void duplgr_ConfirmWipeOutWindow_Opened(object sender, RoutedEventArgs e)
        //{
            
        //}

        

        private void duplgr_DeleteGroupButton_Click(object sender, RoutedEventArgs e)
        {
            duplgr_DeleteGroupWindow.IsEnabled = true;
            duplgr_DeleteGroupWindow.DataContext = GroupDetails1.DataContext;
            duplgr_DeleteGroupWindow.ShowDialog();
        }

        private void duplgr_cmdDoDeleteGroup_Click(object sender, RoutedEventArgs e)
        {
            ItemDuplicateGroup idg = duplgr_DeleteGroupWindow.DataContext as ItemDuplicateGroup;
            if (idg != null)
            {
                duplgr_DeleteGroupWindow.IsEnabled = false;
                idg.Delete();
                idg.Saved -= idg_Saved;
                idg.Saved += new EventHandler<Csla.Core.SavedEventArgs>(idg_Saved);
                idg.BeginSave(true);
            }
        }

        void idg_Saved(object sender, Csla.Core.SavedEventArgs e)
        {
            duplgr_DeleteGroupWindow.Close();
            duplgr_DeleteGroupWindow.IsEnabled = true;
            RefreshDuplicates();
        }

        private void duplgr_cmdCancelDeleteGroup_Click(object sender, RoutedEventArgs e)
        {
            duplgr_DeleteGroupWindow.Close();
            duplgr_DeleteGroupWindow.IsEnabled = true;
        }
        private void duplgr_AddNewGroupButton_Click(object sender, RoutedEventArgs e)
        {
            duplgr_CreateGroupWindow1.duplgr_NewGroupradgrid_itemsList.SelectedItems.Clear();
            duplgr_CreateGroupWindow1.duplgr_NewGroupradgrid_itemsList.ItemsSource = duplgr_radgrid_itemsList.ItemsSource;
            foreach (Item it in duplgr_radgrid_itemsList.SelectedItems)
            {
                duplgr_CreateGroupWindow1.duplgr_NewGroupradgrid_itemsList.SelectedItems.Add(it);
            }
            duplgr_CreateGroupWindow1.ShowDialog();
        }
        
        
        
        
        private void duplgr_NewGroupSelectedItemsButton_Click(object sender, RoutedEventArgs e)
        {
            string st = "";
            if (duplgr_CreateGroupWindow1.duplgr_NewGroupradgrid_itemsList.SelectedItems == null
                || duplgr_CreateGroupWindow1.duplgr_NewGroupradgrid_itemsList.SelectedItems.Count == 0
                || duplgr_CreateGroupWindow1.duplgr_NewGroupradgrid_itemsList.SelectedItems.Count > 50) return;
            foreach (Item it in duplgr_CreateGroupWindow1.duplgr_NewGroupradgrid_itemsList.SelectedItems)
            {
                st += it.ItemId.ToString() + ",";
            }
            fetch_newDirtyGroup(st);
        }

        private void duplgr_NewGroupItemsListButton_Click(object sender, RoutedEventArgs e)
        {
            fetch_newDirtyGroup(duplgr_CreateGroupWindow1.duplgr_NewGroup_IDsList.Text);
        }
        private void fetch_newDirtyGroup(string IDs)
        {
            duplgr_CreateGroupWindow1.duplgr_CreateGroupWindow1Busy.IsRunning = true;
            duplgr_CreateGroupWindow1.duplgr_NewGroupItemsListButton.IsEnabled = false;
            duplgr_CreateGroupWindow1.duplgr_NewGroupSelectedItemsButton.IsEnabled = false;
            Csla.DataPortal<ItemDuplicateDirtyGroup> dp = new Csla.DataPortal<ItemDuplicateDirtyGroup>();
            dp.FetchCompleted += ItemDuplicateDirtyGroup_DataChanged;
            dp.BeginFetch(new SingleCriteria<ItemDuplicateDirtyGroup, string>(IDs));
            //dp.BeginFetch( (new Csla.SingleCriteria<ItemDuplicateDirtyGroup, string>(IDs));
        }

        private void ItemDuplicateDirtyGroup_DataChanged(object sender, EventArgs e)
        {
            duplgr_CreateGroupWindow2.duplgr_CreateGroupWindow2GeneralCommentTxt.Text = "";
            duplgr_CreateGroupWindow1.duplgr_CreateGroupWindow1Busy.IsRunning = false;
            //List<object> fake = new List<object>();
            //fake.Add(1);
            //Telerik.Windows.Controls.SelectionChangeEventArgs fe = new Telerik.Windows.Controls.SelectionChangeEventArgs(fake, fake);
            //duplgr_NewGroupradgrid_itemsList_SelectionChanged(null, null);
            ///duplgr_NewGroup_IDsList_TextChanged(null, null);
            Csla.DataPortalResult<ItemDuplicateDirtyGroup> ea = (Csla.DataPortalResult<ItemDuplicateDirtyGroup>)e;
            if (ea.Object != null && (ea.Object as ItemDuplicateDirtyGroup).Members != null)
            {
                duplgr_CreateGroupWindow2.DataContext = ea.Object;
                //duplgr_NewGroupradgrid_itemsList2.DataContext = ea.Object;// (ea.Object as ItemDuplicateDirtyGroup).Members;
                ItemDuplicateDirtyGroup no = (ea.Object as ItemDuplicateDirtyGroup);
                duplgr_CreateGroupWindow2.duplgr_NewGroupradgrid_itemsList2.ItemsSource = no.Members;
                if (no.getMaster() != null) duplgr_CreateGroupWindow2.duplgr_NewGroupradgrid_MasterLine.ItemsSource = no.Members;
                makeGeneralComment();
                duplgr_CreateGroupWindow2.ShowDialog();
                duplgr_CreateGroupWindow1.Close();
                //duplgr_CreateGroupWindow2.ShowDialog();
                
            }
            else if (ea.Error != null)
            {
                duplgr_CreateGroupWindow2.DataContext = null;
                Telerik.Windows.Controls.RadWindow.Alert(ea.Error.Message);
            }
            else
            {
                duplgr_CreateGroupWindow2.DataContext = null;
                Telerik.Windows.Controls.RadWindow.Alert("Unspecified Error");
            }
        }

        private void duplgr_NewGroupradgrid_itemsList2_BindingValidationError(object sender, ValidationErrorEventArgs e)
        {
            MessageBox.Show("shift happens");
        }

        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            HyperlinkButton hlb = (sender as HyperlinkButton);
            if (hlb == null || hlb.Content == null) return;
            
            string IDs = hlb.Tag.ToString();
            
            if (IDs == "") return;
            duplgr_CreateGroupWindow2.Close();
            GroupListSelectionCriteria sel = new GroupListSelectionCriteria(typeof(ItemDuplicateReadOnlyGroupList), IDs);
            RefreshDuplicates(sel);
        }

        //private void Button_Click(object sender, RoutedEventArgs e)
        //{
        //    Button bt = sender as Button;
        //    if (bt == null) return;
        //    ItemDuplicateDirtyGroup iddg = duplgr_CreateGroupWindow2.duplgr_NewGroupradgrid_itemsList2.DataContext as ItemDuplicateDirtyGroup;
        //    Int64 i;
        //    if (Int64.TryParse(bt.Tag.ToString(), out i))
        //    {
        //        duplgr_CreateGroupWindow2.duplgr_NewGroupradgrid_itemsList2.ItemsSource = null;
        //        duplgr_CreateGroupWindow2.duplgr_NewGroupradgrid_MasterLine.ItemsSource = null;
        //        iddg.setMaster(i);
            
        //        FilterDescriptor descriptor = new FilterDescriptor();
        //        descriptor.Member = "IsMaster";
        //        descriptor.Operator = FilterOperator.IsEqualTo;
        //        descriptor.Value = "False";
        //        duplgr_CreateGroupWindow2.duplgr_NewGroupradgrid_itemsList2.FilterDescriptors.Clear();
        //        duplgr_CreateGroupWindow2.duplgr_NewGroupradgrid_MasterLine.FilterDescriptors.Clear();
        //        duplgr_CreateGroupWindow2.duplgr_NewGroupradgrid_itemsList2.FilterDescriptors.Add(descriptor);
        //        FilterDescriptor descriptor2 = new FilterDescriptor();
        //        descriptor2.Member = "IsMaster";
        //        descriptor2.Operator = FilterOperator.IsEqualTo;
        //        descriptor2.Value = "True";
        //        duplgr_CreateGroupWindow2.duplgr_NewGroupradgrid_MasterLine.FilterDescriptors.Add(descriptor2);
        //        duplgr_CreateGroupWindow2.duplgr_NewGroupradgrid_MasterLine.ItemsSource = iddg.Members;
        //        duplgr_CreateGroupWindow2.duplgr_NewGroupradgrid_itemsList2.ItemsSource = iddg.Members;
        //        duplgr_CreateGroupWindow2.duplgr_NewGroupradgrid_itemsList2.Rebind();
        //        duplgr_CreateGroupWindow2.duplgr_NewGroupradgrid_MasterLine.Rebind();
        //        makeGeneralComment();
        //    }
        //}

        private void duplgr_NewGroupradgrid_itemsList2FinishB_Click(object sender, RoutedEventArgs e)
        {
            ItemDuplicateDirtyGroup iddg = duplgr_CreateGroupWindow2.duplgr_NewGroupradgrid_itemsList2.DataContext as ItemDuplicateDirtyGroup;
            iddg.Saved += new EventHandler<Csla.Core.SavedEventArgs>(iddg_Saved);
            duplgr_CreateGroupWindow2.Close();
            windowGettingDuplicates1.Header = "Refreshing Duplicates List...";
            windowGettingDuplicates1.BusyLoading1.IsRunning = true;
            windowGettingDuplicates1.ShowDialog();
            iddg.BeginSave(true);
        }

        void iddg_Saved(object sender, Csla.Core.SavedEventArgs e)
        {
            //ItemDuplicateDirtyGroup iddg = e.NewObject as ItemDuplicateDirtyGroup;
            if (e.Error != null)
            {
                Telerik.Windows.Controls.RadWindow.Alert(e.Error.Message);
            }
            RefreshDuplicates();
            
        }

        //private void duplgr_NewGroupradgrid_itemsList2_Filtered(object sender, GridViewFilteredEventArgs e)
        //{
        //    duplgr_NewGroupradgrid_MasterLine.ItemsSource = duplgr_NewGroupradgrid_itemsList2.ItemsSource;
        //}

        private void duplgr_NewGroupradgrid_itemsList2BackB_Click(object sender, RoutedEventArgs e)
        {
            duplgr_CreateGroupWindow2.Close();
            duplgr_CreateGroupWindow1.ShowDialog();
        }
        private void makeGeneralComment()
        {
            ItemDuplicateDirtyGroup iddg = duplgr_CreateGroupWindow2.duplgr_NewGroupradgrid_itemsList2.DataContext as ItemDuplicateDirtyGroup;
            string comment = "";
            duplgr_CreateGroupWindow2.duplgr_NewGroupradgrid_itemsList2FinishB.IsEnabled = false;
            if (iddg != null)
            {
                if (iddg.getMaster() != null)
                {
                    bool goodMaster = !iddg.getMaster().IsExported;
                    if (!iddg.IsUsable) comment = "<- Resulting Group is two small, you need at least one useable master and one valid duplicate. Please click back and select more items";
                    else if (!goodMaster)
                    {
                        comment = "Current Master appears in other groups: consider using its own group instead of manually creating a new group.";
                        duplgr_CreateGroupWindow2.duplgr_NewGroupradgrid_itemsList2FinishB.IsEnabled = true;
                    }
                    else if (goodMaster)
                    {
                        comment = "The Group appears to be fine: click next to create. ->";
                        duplgr_CreateGroupWindow2.duplgr_NewGroupradgrid_itemsList2FinishB.IsEnabled = true;
                    }
                }
                //else if (!iddg.IsUsable) 
                else comment = "<- The group does not have a suitable Master: Please click back and select more items.";
            }
            duplgr_CreateGroupWindow2.duplgr_CreateGroupWindow2GeneralCommentTxt.Text = comment;
        }
        public static double Compute(string s, string t)
        { //adapted from http://www.dotnetperls.com/levenshtein
            s = s.Trim();
            t = t.Trim();
            int len = Math.Max(s.Length, t.Length);

            if (len == 0) return (double)1;//strings are the same 'cause they are both empty
            if (s.Length == 0 || t.Length == 0) return 0.95;//one sting is empty, the other is not: can't really say = some difference.
            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];
            // Step 1
            if (n == 0)
            {
                return m;
            }
            if (m == 0)
            {
                return n;
            }
            // Step 2
            for (int i = 0; i <= n; d[i, 0] = i++)
            {
            }
            for (int j = 0; j <= m; d[0, j] = j++)
            {
            }
            // Step 3
            for (int i = 1; i <= n; i++)
            {
                //Step 4
                for (int j = 1; j <= m; j++)
                {
                    // Step 5
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;

                    // Step 6
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }
            // Step 7
            int dist =  d[n, m];
            double tmp = (double)dist / (double)len;
            double res = 1 - tmp;
            return res;
        }

        private void GroupDetails1_SelectionChanged(object sender, SelectionChangeEventArgs e)
        {
            System.Collections.ObjectModel.ReadOnlyCollection<object> sel = e.AddedItems;
            if (sel.Count == 0) return;
            ItemDuplicateGroupMember igm = sel[0] as ItemDuplicateGroupMember;
            if (igm == null) return;
            ItemDuplicateGroup IDG = (GroupDetails1.DataContext as ItemDuplicateGroup);
            ItemDuplicateGroupMember mast = IDG.getMaster();
            double score = Compute(igm.Title, mast.Title);
            Brush b = getBrushFromScore(score);
            brMTitle.Background = b;
            score = Compute(igm.Authors, mast.Authors);
            b = getBrushFromScore(score);
            brMAuthors.Background = b;

            score = Compute(igm.Month, mast.Month);
            b = getBrushFromScore(score);
            brMMonth.Background = b;

            score = Compute(igm.Pages, mast.Pages);
            b = getBrushFromScore(score);
            brMPages.Background = b;

            score = Compute(igm.ParentTitle, mast.ParentTitle);
            b = getBrushFromScore(score);
            brMPtitle.Background = b;

            score = Compute(igm.Year, mast.Year);
            b = getBrushFromScore(score);
            brMYear.Background = b;

            

        }
        private Brush getBrushFromScore(double score)
        {
            if (score >= 1) return white;
            else if (score > 0.9) return light;
            else if (score > 0.75) return orange;
            else return pink;
        }
    }
}
