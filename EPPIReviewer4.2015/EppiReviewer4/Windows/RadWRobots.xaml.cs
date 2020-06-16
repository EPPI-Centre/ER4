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
using System.Threading;

namespace EppiReviewer4
{
    public partial class RadWRobots : RadWindow, INotifyPropertyChanged
    {
        public event EventHandler<RoutedEventArgs> closeWindowRobots;
        public ItemDocument SelectedItemDocument;
        public string SelectedTitle;
        public string SelectedAbstract;
        public RadWRobots()
        {
            InitializeComponent();
        }



        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            this.closeWindowRobots.Invoke(sender, e);
        }

        private void HyperlinkButton_Click_1(object sender, RoutedEventArgs e)
        {
            if (rbRobotReviewer.IsChecked == true)
            {
                DoRobotReviewer(sender, e);
            }
            else
            {
                DoHBCP(sender, e);
            }
        }

        private void DoRobotReviewer(object sender, RoutedEventArgs e)
        {
            ReviewSet rs = dialogRobotsComboSelectCodeSet.SelectedItem as ReviewSet;
            if (rs != null)
            {
                if (rs.RobotReviewerValidated() == false)
                {
                    RadWindow.Alert("Please select a RobotReviewer compatible coding tool");
                    return;
                }

                DataPortal<RobotReviewerCommand> dp2 = new DataPortal<RobotReviewerCommand>();
                RobotReviewerCommand rr = new RobotReviewerCommand(SelectedTitle, SelectedAbstract);
                rr.SelectedReviewSet = rs;
                rr.SelectedItemDocument = SelectedItemDocument;
                dp2.ExecuteCompleted += (o, e2) =>
                {
                    busyIndicatorRobots.IsBusy = false;
                    hlCancel.IsEnabled = true;
                    hlGo.IsEnabled = true;
                    if (e2.Error != null)
                    {
                        RadWindow.Alert(e2.Error.Message);
                    }
                    else
                    {
                        RobotReviewerCommand rr2 = e2.Object as RobotReviewerCommand;
                        this.closeWindowRobots.Invoke(sender, e);
                        //RadWindow.Alert(rr2.ReturnMessage);
                    }
                };
                busyIndicatorRobots.IsBusy = true;
                hlCancel.IsEnabled = false;
                hlGo.IsEnabled = false;
                dp2.BeginExecute(rr);
            }
        }

        private void DoHBCP(object sender, RoutedEventArgs e)
        {
            ReviewSet rs = dialogRobotsComboSelectCodeSet.SelectedItem as ReviewSet;
            if (rs != null)
            {
                /* No validation check at the moment. If a given attribute isn't present it just doesn't get a result
                if (rs.HBCPValidated() == false)
                {
                    RadWindow.Alert("Please select a HBCP compatible coding tool");
                    return;
                }
                */
                DataPortal<RobotHBCPCommand> dp2 = new DataPortal<RobotHBCPCommand>();
                RobotHBCPCommand rh = new RobotHBCPCommand("5", "10%2C20", "0.2");
                rh.SelectedReviewSet = rs;
                rh.SelectedItemDocument = SelectedItemDocument;
                dp2.ExecuteCompleted += (o, e2) =>
                {
                    Thread.Sleep(3000); // same hack as above. I prefer to do the sleep here than server side
                    busyIndicatorRobots.IsBusy = false;
                    hlCancel.IsEnabled = true;
                    hlGo.IsEnabled = true;
                    if (e2.Error != null)
                    {
                        RadWindow.Alert(e2.Error.Message);
                    }
                    else
                    {
                        RobotHBCPCommand rh2 = e2.Object as RobotHBCPCommand;
                        this.closeWindowRobots.Invoke(sender, e);
                        //RadWindow.Alert(rr2.ReturnMessage);
                    }
                };
                busyIndicatorRobots.IsBusy = true;
                hlCancel.IsEnabled = false;
                hlGo.IsEnabled = false;
                dp2.BeginExecute(rh);
            }
        }
    }
}
