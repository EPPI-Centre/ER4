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
    
    public partial class RadWCheckArmDelete : RadWindow, INotifyPropertyChanged 
    {
        public event EventHandler<RoutedEventArgs> cmdArmDeletedInWindow;
        public RadWCheckArmDelete()
        {
            InitializeComponent();
            isEn.DataContext = this;
            this.DataContext = this;
        }
        private ItemArm CurrentArm = new ItemArm();
        private ItemArmDeleteWarningCommand WarningCommand = new ItemArmDeleteWarningCommand();
        private ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
        private SolidColorBrush WarningBg = new SolidColorBrush(Colors.Orange);
        private SolidColorBrush AllClearBg = new SolidColorBrush(Colors.Transparent);
        public event PropertyChangedEventHandler PropertyChanged;
        public void StartChecking(ItemArm currentArm)
        {
            this.WarningCommand = null;
            this.CurrentArm = currentArm;
            if (currentArm == null || currentArm.ItemArmId == 0)
            {
                this.Close();
                return;
            }
            TextBlockCheckDeleteArm.Text = "Are you sure you want to delete the '" + this.CurrentArm.Title +"' arm from this study?";
            DataPortal<ItemArmDeleteWarningCommand> dp = new DataPortal<ItemArmDeleteWarningCommand>();
            TextBlockCheckDeleteArmDetails.Text = "";
            WarningBorder.Background = this.AllClearBg;
            txtBoxConfirm.Text = "";
            txtBoxConfirm.Visibility = Visibility.Collapsed;
            ItemArmDeleteWarningCommand command = new ItemArmDeleteWarningCommand(CurrentArm.ItemId, CurrentArm.ItemArmId);
            dp.ExecuteCompleted += (o, e2) =>
            {
                this.BusyCheckArmDelete.IsRunning = false;
                if (e2.Error != null)
                {
                    MessageBox.Show(e2.Error.Message);
                }
                else
                {
                    WarningCommand = e2.Object;//we only have something here if the command has finished
                    NotifyPropertyChanged("CanDeleteArm");
                    if (e2.Object.NumCodings > 0)
                    {
                        TextBlockCheckDeleteArmDetails.Text = "Deleting an Arm is a permanent operation and will delete all coding associated with the Arm."
                            + Environment.NewLine + "This Arm is associated with " + e2.Object.NumCodings.ToString() + " codes."
                            + Environment.NewLine + "This Arm is associated with " + e2.Object.NumOutcomes.ToString() + " outcomes."
                            + Environment.NewLine + "Please type 'I confirm' in the box below if you are sure you want to proceed.";
                        WarningBorder.Background = this.WarningBg;
                        txtBoxConfirm.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        TextBlockCheckDeleteArmDetails.Text = "This arm is NOT associated with any code: it's safe to delete it.";
                    }
                }
            };
            BusyCheckArmDelete.IsRunning = true;
            dp.BeginExecute(command);
        }

       
        private void cmdDeleteArm_Click(object sender, RoutedEventArgs e)
        {
            CurrentArm.Delete();
            if (cmdArmDeletedInWindow != null) cmdArmDeletedInWindow.Invoke(this.CurrentArm, e);
        }

        private void cmdCancelDeleteArm_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        public bool CanDeleteArm
        {//used to bind the "enabled" prop of the DoIt button
            get
            {
                if (!ri.HasWriteRights()) return false;
                if (BusyCheckArmDelete.IsRunning || WarningCommand == null) return false;//command to check is still running
                else//command isn't running
                {
                    if (WarningCommand.NumCodings == 0) return true;
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
            NotifyPropertyChanged("CanDeleteArm");
        }
    }
}
