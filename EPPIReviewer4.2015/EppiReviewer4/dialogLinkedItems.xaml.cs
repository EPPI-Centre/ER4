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
using Csla;
using BusinessLibrary.BusinessClasses;
using Telerik.Windows.Controls;
using System.ComponentModel;
using Csla.Xaml;

namespace EppiReviewer4
{
    public partial class dialogLinkedItems : UserControl, INotifyPropertyChanged
    {//INotifyPropertyChanged is needed to notify UI that haswriterights may have changed (rebind isEn)
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }  
        private bool CodingOnlyMode = false;
        //first bunch of lines to make the read-only UI work
        private BusinessLibrary.Security.ReviewerIdentity ri;
        public bool HasWriteRights
        {
            get
            {
                if (ri == null) return false;
                else return ri.HasWriteRights() && !CodingOnlyMode;//false if user does not have write rights or if control is set for coding only mode.
            }
        }
        //end of read-only ui hack

        
        private RadWindow windowLinkedItem = new RadWindow();
        private Grid windowLinkedItemGrid = new Grid();
        private dialogLinkedItemDetail dialogLinkedItemDetailControl = new dialogLinkedItemDetail();
        private RadWCreateEditLink windowCreateEditLink = new RadWCreateEditLink();


        public dialogLinkedItems()
        {
           
            InitializeComponent(); 
            //two lines to make the read-only UI work
            //add <Button x:Name="isEn" IsEnabled="{Binding HasWriteRights, Mode=OneWay}" ></Button>
            //to the xaml resources section!!
            ri = Csla.ApplicationContext.User.Identity as BusinessLibrary.Security.ReviewerIdentity;
            isEn.DataContext = this;
            //end of read-only ui hack

            
            windowLinkedItem.Header="Linked item";
            windowLinkedItem.Width=700;
            windowLinkedItem.Height=500;
            windowLinkedItem.ResizeMode = ResizeMode.CanResize;
            windowLinkedItem.WindowStateChanged += new EventHandler(Helpers.WindowHelper.MaxOnly_WindowStateChanged);
            windowLinkedItem.Style = Application.Current.Resources["CustomRadWindowStyle"] as Style;
            windowLinkedItem.RestrictedAreaMargin= new Thickness(20);
            windowLinkedItem.IsRestricted=true;
            windowLinkedItem.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
            windowLinkedItemGrid.Children.Add(dialogLinkedItemDetailControl);
            windowLinkedItem.Content = windowLinkedItemGrid;

            windowCreateEditLink.cmdGetItem_Clicked +=new EventHandler<RoutedEventArgs>(cmdGetItem_Click);
            windowCreateEditLink.cmdSaveLink_Clicked +=new EventHandler<RoutedEventArgs>(cmdSaveLink_Click);

        }
        public void PrepareCodingOnly()
        {
            CodingOnlyMode = true;
            NotifyPropertyChanged("HasWriteRights");//coding only changes the value of haswriterights
        }
        private ItemLink CurrentItemLink = null;

        private void CslaDataProvider_DataChanged(object sender, EventArgs e)
        {
            CslaDataProvider provider = ((CslaDataProvider)this.Resources["ItemLinkListData"]);
            if (provider.Error != null)
                MessageBox.Show(((Csla.Xaml.CslaDataProvider)sender).Error.Message);
        }

        public void RefreshLinkedItemList()
        {
            CslaDataProvider provider = ((CslaDataProvider)this.Resources["ItemLinkListData"]);
            if (provider != null)
            {
                Item i = DataContext as Item;
                if (i != null)
                {
                    provider.FactoryParameters.Clear();
                    provider.FactoryParameters.Add(i.ItemId);
                    provider.FactoryMethod = "GetItemLinkList";
                    provider.Refresh();
                }
            }
        }

        private void cmdGetItem_Click(object sender, RoutedEventArgs e)
        {
            Int64 ItemId = -1;
            bool checkParse = Int64.TryParse(windowCreateEditLink.TextBoxItemId.Text, out ItemId);
            if (checkParse == true)
            {
                DataPortal<Item> dp = new DataPortal<Item>();
                dp.FetchCompleted += (o, e2) =>
                {
                    windowCreateEditLink.BusyGetitem.IsBusy = false;
                    if (e2.Object != null)
                    {
                        windowCreateEditLink.TextBlockItemId.Text = e2.Object.ItemId.ToString();
                        windowCreateEditLink.TextBlockShortTitle.Text = e2.Object.ShortTitle;
                        windowCreateEditLink.TextBlockTitle.Text = e2.Object.Title;
                    }
                };
                windowCreateEditLink.BusyGetitem.IsBusy = true;
                dp.BeginFetch(new SingleCriteria<Item, Int64>(ItemId));
            }
            else
            {
                MessageBox.Show("Please enter a valid number");
            }
        }

        private void cmdNewLink_Click(object sender, RoutedEventArgs e)
        {
            CurrentItemLink = null;
            windowCreateEditLink.TextBoxDescription.Text = "";
            windowCreateEditLink.TextBoxItemId.Text = "";
            windowCreateEditLink.TextBlockShortTitle.Text = "";
            windowCreateEditLink.TextBlockTitle.Text = "";
            windowCreateEditLink.TextBlockItemId.Text = "";
            windowCreateEditLink.ShowDialog();
        }

        private void cmdSaveLink_Click(object sender, RoutedEventArgs e)
        {
            Int64 ItemIdSecondary = 0;
            if ((Int64.TryParse(windowCreateEditLink.TextBlockItemId.Text, out ItemIdSecondary) == true) && (ItemIdSecondary != 0))
            {
                if (CurrentItemLink == null)
                {
                    CurrentItemLink = new ItemLink();
                    CslaDataProvider provider = ((CslaDataProvider)this.Resources["ItemLinkListData"]);
                    if (provider != null)
                    {
                        (provider.Data as ItemLinkList).Add(CurrentItemLink);
                    }
                }
                CurrentItemLink.BeginEdit();
                CurrentItemLink.ItemIdPrimary = (DataContext as Item).ItemId;
                CurrentItemLink.ItemIdSecondary = ItemIdSecondary;
                CurrentItemLink.Description = windowCreateEditLink.TextBoxDescription.Text;
                CurrentItemLink.ShortTitle = windowCreateEditLink.TextBlockShortTitle.Text;
                CurrentItemLink.Title = windowCreateEditLink.TextBlockTitle.Text;
                CurrentItemLink.ApplyEdit();
                windowCreateEditLink.Close();
            }
        }

        private void cmdEditLink_Click(object sender, RoutedEventArgs e)
        {
            CurrentItemLink = (sender as Button).DataContext as ItemLink;
            if (CurrentItemLink != null)
            {
                windowCreateEditLink.TextBoxDescription.Text = CurrentItemLink.Description;
                windowCreateEditLink.TextBoxItemId.Text = CurrentItemLink.ItemIdSecondary.ToString();
                windowCreateEditLink.TextBlockItemId.Text = CurrentItemLink.ItemIdSecondary.ToString();
                windowCreateEditLink.TextBlockShortTitle.Text = CurrentItemLink.ShortTitle;
                windowCreateEditLink.TextBlockTitle.Text = CurrentItemLink.Title;
                windowCreateEditLink.ShowDialog();
            }
        }

        private void cmdDeleteLink_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to delete this link?", "Confirm delete", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                CurrentItemLink = (sender as Button).DataContext as ItemLink;
                CurrentItemLink.BeginEdit();
                CurrentItemLink.Delete();
                CurrentItemLink.ApplyEdit();
                CurrentItemLink = null;
            }
        }

        private void cmdViewLinkedItem_Click(object sender, RoutedEventArgs e)
        {
            dialogLinkedItemDetailControl.BindItem((sender as Button).DataContext as ItemLink);
            windowLinkedItem.ShowDialog();
        }
    }
}
