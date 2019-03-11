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
using Csla;
using Csla.DataPortalClient;
using Csla.Silverlight;
using BusinessLibrary.BusinessClasses;
using Telerik.Windows.Controls;
using Csla.Xaml;

namespace EppiReviewer4
{
    public partial class dialogEditOutcomes : UserControl
    {
        private Outcome _selectedOutcome;

        public Int64 CurrentItemId;
        
        private RadWindow windowEditOutcome = new RadWindow();
        private Grid GridEditOutcome = new Grid();
        private dialogEditOutcome dialogEditOutcomeControl = new dialogEditOutcome();
        private RadWConfirmOutcomeDelete windowConfirmOutcomeDelete = new RadWConfirmOutcomeDelete();

        public dialogEditOutcomes()
        {
            InitializeComponent();

            dialogEditOutcomeControl.CloseWindowRequest +=new EventHandler(dialogEditOutcomeControl_CloseWindowRequest);
            GridEditOutcome.Children.Add(dialogEditOutcomeControl);
            windowEditOutcome.Header="Edit / create outcome";
            windowEditOutcome.CanClose= true;
            windowEditOutcome.ResizeMode= ResizeMode.NoResize;
            windowEditOutcome.WindowStartupLocation= Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
            windowEditOutcome.WindowState = WindowState.Maximized;
            windowEditOutcome.RestrictedAreaMargin = new Thickness(10);
            windowEditOutcome.IsRestricted = true;
            windowEditOutcome.WindowStateChanged += new EventHandler(Helpers.WindowHelper.MaxOnly_WindowStateChanged);
            windowEditOutcome.Style = Application.Current.Resources["CustomRadWindowStyle"] as Style;
            windowEditOutcome.Content = GridEditOutcome;

            windowConfirmOutcomeDelete.cmdDeleteOutcome_Click_1ed +=new EventHandler<RoutedEventArgs>(cmdDeleteOutcome_Click_1);
            windowConfirmOutcomeDelete.cmdCancelDeleteOutcome_Clicked +=new EventHandler<RoutedEventArgs>(cmdCancelDeleteOutcome_Click);
        }

        public void RefreshDataProvider()
        {
            Int64 itemSetid = (this.DataContext as AttributeSet).ItemData.ItemSetId;
            CslaDataProvider provider = this.Resources["OutcomeItemListData"] as CslaDataProvider;
            provider.FactoryParameters.Clear();
            provider.FactoryParameters.Add(itemSetid);
            provider.FactoryMethod = "GetOutcomeItemList";
            provider.Refresh();
        }

        private void CslaDataProviderItemList_DataChanged(object sender, EventArgs e)
        {
            CslaDataProvider provider = ((CslaDataProvider)this.Resources["OutcomeItemListData"]);
            if (provider.Error != null)
                System.Windows.Browser.HtmlPage.Window.Alert(((Csla.Xaml.CslaDataProvider)sender).Error.Message);
        }

        private void cmdNewOutcome_Click(object sender, RoutedEventArgs e)
        {
            Int64 itemSetid = (this.DataContext as AttributeSet).ItemData.ItemSetId;
            Outcome o = new Outcome();
            o.ItemSetId = itemSetid;
            o.OutcomeTypeId = 1;
            GridEditOutcome.DataContext = o;
            dialogEditOutcomeControl.CurrentItemId = CurrentItemId;
            dialogEditOutcomeControl.CurrentOutcomeItemList = GridOutcomes.ItemsSource as OutcomeItemList;
            dialogEditOutcomeControl.BuildOutcomeClassificationList(this.DataContext as AttributeSet);
            dialogEditOutcomeControl.RefreshProviders();
            windowEditOutcome.ShowDialog();
        }

        private void cmdEditOutcome_Click(object sender, RoutedEventArgs e)
        {
            Outcome o = ((Button)(sender)).DataContext as Outcome;
            o.BeginEdit();
            GridEditOutcome.DataContext = o;
            dialogEditOutcomeControl.CurrentItemId = CurrentItemId;
            dialogEditOutcomeControl.CurrentOutcomeItemList = GridOutcomes.ItemsSource as OutcomeItemList;
            dialogEditOutcomeControl.BuildOutcomeClassificationList(this.DataContext as AttributeSet);
            dialogEditOutcomeControl.RefreshProviders();
            windowEditOutcome.ShowDialog();
        }

        private void cmdDeleteOutcome_Click(object sender, RoutedEventArgs e)
        {
            _selectedOutcome = ((Button)(sender)).DataContext as Outcome;
            windowConfirmOutcomeDelete.ShowDialog();
        }

        private void cmdCancelDeleteOutcome_Click(object sender, RoutedEventArgs e)
        {
            windowConfirmOutcomeDelete.Close();
        }

        private void cmdDeleteOutcome_Click_1(object sender, RoutedEventArgs e)
        {
            windowConfirmOutcomeDelete.BusyCheckOutcomeDelete.IsRunning = true;
            if (_selectedOutcome != null)
            {
                _selectedOutcome.Delete();
                _selectedOutcome.Saved += (o, e2) =>
                {
                    windowConfirmOutcomeDelete.BusyCheckOutcomeDelete.IsRunning = false;
                    if (e2.Error != null)
                        MessageBox.Show(e2.Error.Message);
                    RefreshDataProvider();
                    windowConfirmOutcomeDelete.Close();
                };
                _selectedOutcome.BeginSave(true);
            }
        }

        void dialogEditOutcomeControl_CloseWindowRequest(object sender, EventArgs e)
        {
            RefreshDataProvider();
            windowEditOutcome.Close();
        }

        //private void windowEditOutcome_Opened(object sender, RoutedEventArgs e)
        //{
            
        //    dialogEditOutcomeControl.RefreshProviders();
        //}

    }
}
