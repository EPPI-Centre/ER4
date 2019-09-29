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
using Csla;
using BusinessLibrary.Security;
using System.ComponentModel;

namespace EppiReviewer4
{
   
    public partial class RadWCheckTimepointDelete : RadWindow, INotifyPropertyChanged
    {
        #region EVENTS
        //put one event for each code-behind handler declared in XAML
        //EXAMPLE: 
        //public event EventHandler<RoutedEventArgs> cmdButton_Clicked;
        //
        #endregion
        public event EventHandler<RoutedEventArgs> cmdTimepointDeletedInWindow;
        public RadWCheckTimepointDelete()
        {
            InitializeComponent();
            isEn.DataContext = this;
            this.DataContext = this;
        }

        private ItemTimepoint CurrentTimepoint = new ItemTimepoint();
        private ItemTimepointDeleteWarningCommand WarningCommand = new ItemTimepointDeleteWarningCommand();
        private ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
        private SolidColorBrush WarningBg = new SolidColorBrush(Colors.Orange);
        private SolidColorBrush AllClearBg = new SolidColorBrush(Colors.Transparent);
        public event PropertyChangedEventHandler PropertyChanged;

        public void StartChecking(ItemTimepoint currentTimepoint)
        {
            this.WarningCommand = null;
            this.CurrentTimepoint = currentTimepoint;
            if (currentTimepoint == null || currentTimepoint.ItemTimepointId == 0)
            {
                this.Close();
                return;
            }
            TextBlockCheckDeleteTimepoint.Text = "Are you sure you want to delete the '" + this.CurrentTimepoint.TimepointDisplayValue + "' timepoint from this study?";
            DataPortal<ItemTimepointDeleteWarningCommand> dp = new DataPortal<ItemTimepointDeleteWarningCommand>();
            TextBlockCheckDeleteTimepointDetails.Text = "";
            WarningBorder.Background = this.AllClearBg;
            txtBoxConfirm.Text = "";
            txtBoxConfirm.Visibility = Visibility.Collapsed;
            ItemTimepointDeleteWarningCommand command = new ItemTimepointDeleteWarningCommand(CurrentTimepoint.ItemId, CurrentTimepoint.ItemTimepointId);
            dp.ExecuteCompleted += (o, e2) =>
            {
                this.BusyCheckTimepointDelete.IsRunning = false;
                if (e2.Error != null)
                {
                    MessageBox.Show(e2.Error.Message);
                }
                else
                {
                    WarningCommand = e2.Object;//we only have something here if the command has finished
                    NotifyPropertyChanged("CanDeleteTimepoint");
                    if (e2.Object.NumOutcomes > 0)
                    {
                        TextBlockCheckDeleteTimepointDetails.Text = "Deleting a Timepoint is a permanent operation which will remove the timepoint from all outcomes associated with it (whether added by you, or someone else)."
                            + Environment.NewLine + "This Timepoint is associated with " + e2.Object.NumOutcomes.ToString() + " outcome(s)."
                            + Environment.NewLine + "Please type 'I confirm' in the box below if you are sure you want to proceed.";
                        WarningBorder.Background = this.WarningBg;
                        txtBoxConfirm.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        TextBlockCheckDeleteTimepointDetails.Text = "This timepoint is NOT associated with any outcomes: it's safe to delete it.";
                    }
                }
            };
            BusyCheckTimepointDelete.IsRunning = true;
            dp.BeginExecute(command);
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
        #endregion

        private void cmdDeleteTimepoint_Click(object sender, RoutedEventArgs e)
        {
            CurrentTimepoint.Delete();
            if (cmdTimepointDeletedInWindow != null) cmdTimepointDeletedInWindow.Invoke(this.CurrentTimepoint, e);
        }

        private void cmdCancelDeleteTimepoint_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public bool CanDeleteTimepoint
        {//used to bind the "enabled" prop of the DoIt button
            get
            {
                if (!ri.HasWriteRights()) return false;
                if (BusyCheckTimepointDelete.IsRunning || WarningCommand == null) return false;//command to check is still running
                else//command isn't running
                {
                    if (WarningCommand.NumOutcomes == 0) return true;
                    else if (txtBoxConfirm.Text.ToLower() == "i confirm") return true;
                }
                return false;
            }
        }
        public void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void txtBoxConfirm_TextChanged(object sender, TextChangedEventArgs e)
        {
            NotifyPropertyChanged("CanDeleteTimepoint");
        }
    }
}
