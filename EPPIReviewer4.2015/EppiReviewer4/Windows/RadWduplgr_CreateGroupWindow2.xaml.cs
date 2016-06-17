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

using Telerik.Windows.Controls;
using BusinessLibrary.BusinessClasses;
using Telerik.Windows.Data;

namespace EppiReviewer4
{
    /// <summary>
    /// The process of creating a new window works as follow:
    /// 1) add new (template) in the Windows folder
    /// 2) find the original declaration in XAML and move it into the new XAML file, save the x:Name attribute for later. See xaml template for details. Also copy or move Reference entries as described in XAML.
    /// 3) in the old code-behind file, create a private instance of the new window type:
    ///     EXAMPLE: (this is a class member, don't place it in a method or porperty; change name of class and object appropriately)
    ///     private RadWduplgr_CreateGroupWindow2 RadWExample = new RadWduplgr_CreateGroupWindow2(); 
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
    public partial class RadWduplgr_CreateGroupWindow2 : RadWindow //change this to give it a unque name, must inherit from Radwindow.
    {
        #region EVENTS
        //put one event for each code-behind handler declared in XAML
        //EXAMPLE: 
        //public event EventHandler<RoutedEventArgs> cmdButton_Clicked;
        //
        public event EventHandler<RoutedEventArgs> HyperlinkButton_Clicked;
        public event EventHandler<RoutedEventArgs> duplgr_NewGroupradgrid_itemsList2BackB_Clicked;
        public event EventHandler<RoutedEventArgs> duplgr_NewGroupradgrid_itemsList2FinishB_Clicked;
        #endregion
        public RadWduplgr_CreateGroupWindow2()
        {
            InitializeComponent();
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

        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            if (HyperlinkButton_Clicked != null) HyperlinkButton_Clicked.Invoke(sender, e);
        }

        private void duplgr_NewGroupradgrid_itemsList2BackB_Click(object sender, RoutedEventArgs e)
        {
            if (duplgr_NewGroupradgrid_itemsList2BackB_Clicked != null) duplgr_NewGroupradgrid_itemsList2BackB_Clicked.Invoke(sender, e);
        }

        private void duplgr_NewGroupradgrid_itemsList2FinishB_Click(object sender, RoutedEventArgs e)
        {
            if (duplgr_NewGroupradgrid_itemsList2FinishB_Clicked != null) duplgr_NewGroupradgrid_itemsList2FinishB_Clicked.Invoke(sender, e);
        }
        
        #endregion
        private void duplgr_NewGroupradgrid_itemsList2_Filtered(object sender, Telerik.Windows.Controls.GridView.GridViewFilteredEventArgs e)
        {
            duplgr_NewGroupradgrid_MasterLine.ItemsSource = duplgr_NewGroupradgrid_itemsList2.ItemsSource;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button bt = sender as Button;
            if (bt == null) return;
            ItemDuplicateDirtyGroup iddg = duplgr_NewGroupradgrid_itemsList2.DataContext as ItemDuplicateDirtyGroup;
            Int64 i;
            if (Int64.TryParse(bt.Tag.ToString(), out i))
            {
                duplgr_NewGroupradgrid_itemsList2.ItemsSource = null;
                duplgr_NewGroupradgrid_MasterLine.ItemsSource = null;
                iddg.setMaster(i);

                FilterDescriptor descriptor = new FilterDescriptor();
                descriptor.Member = "IsMaster";
                descriptor.Operator = FilterOperator.IsEqualTo;
                descriptor.Value = "False";
                duplgr_NewGroupradgrid_itemsList2.FilterDescriptors.Clear();
                duplgr_NewGroupradgrid_MasterLine.FilterDescriptors.Clear();
                duplgr_NewGroupradgrid_itemsList2.FilterDescriptors.Add(descriptor);
                FilterDescriptor descriptor2 = new FilterDescriptor();
                descriptor2.Member = "IsMaster";
                descriptor2.Operator = FilterOperator.IsEqualTo;
                descriptor2.Value = "True";
                duplgr_NewGroupradgrid_MasterLine.FilterDescriptors.Add(descriptor2);
                duplgr_NewGroupradgrid_MasterLine.ItemsSource = iddg.Members;
                duplgr_NewGroupradgrid_itemsList2.ItemsSource = iddg.Members;
                duplgr_NewGroupradgrid_itemsList2.Rebind();
                duplgr_NewGroupradgrid_MasterLine.Rebind();
                makeGeneralComment();
            }
        }
        private void makeGeneralComment()
        {
            ItemDuplicateDirtyGroup iddg = duplgr_NewGroupradgrid_itemsList2.DataContext as ItemDuplicateDirtyGroup;
            string comment = "";
            duplgr_NewGroupradgrid_itemsList2FinishB.IsEnabled = false;
            if (iddg != null)
            {
                if (iddg.getMaster() != null)
                {
                    bool goodMaster = !iddg.getMaster().IsExported;
                    if (!iddg.IsUsable) comment = "<- Resulting Group is two small, you need at least one useable master and one valid duplicate. Please click back and select more items";
                    else if (!goodMaster)
                    {
                        comment = "Current Master appears in other groups: consider using its own group instead of manually creating a new group.";
                        duplgr_NewGroupradgrid_itemsList2FinishB.IsEnabled = true;
                    }
                    else if (goodMaster)
                    {
                        comment = "The Group appears to be fine: click next to create. ->";
                        duplgr_NewGroupradgrid_itemsList2FinishB.IsEnabled = true;
                    }
                }
                //else if (!iddg.IsUsable) 
                else comment = "<- The group does not have a suitable Master: Please click back and select more items.";
            }
            duplgr_CreateGroupWindow2GeneralCommentTxt.Text = comment;
        }

        
    }
}
