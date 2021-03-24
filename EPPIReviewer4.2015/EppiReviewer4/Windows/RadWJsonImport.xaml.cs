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
using Csla.Data;
using BusinessLibrary.Security;
using System.ComponentModel;
using System.Threading;
using Csla.Xaml;

namespace EppiReviewer4
{
    public partial class RadWJsonImport : RadWindow, INotifyPropertyChanged
    {
        public event EventHandler<RoutedEventArgs> closeWindowJsonImport;
        
        public RadWJsonImport()
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
            this.closeWindowJsonImport.Invoke(sender, e);
        }

        private void cmdGo_Click(object sender, RoutedEventArgs e)
        {
            CslaDataProvider provider = (App.Current.Resources["CodeSetsData"] as CslaDataProvider);
            if (provider != null)
            {
                ReviewSetsList rsl = provider.Data as ReviewSetsList;
                if (rsl != null)
                {
                    DataPortal<ImportJsonCommand> dp2 = new DataPortal<ImportJsonCommand>();
                    ImportJsonCommand ij = new ImportJsonCommand(tbPath.Text, "");
                    ij.ReviewSets = rsl;
                    dp2.ExecuteCompleted += (o, e2) =>
                    {
                        BusyDoingStuff.IsBusy = false;
                        cmdGo.IsEnabled = true;
                        {
                            ImportJsonCommand rr2 = e2.Object as ImportJsonCommand;
                            //this.closeWindowRobots.Invoke(sender, e);
                            RadWindow.Alert("Imported. Refresh codesets and items lists");
                        }
                    };
                    BusyDoingStuff.IsBusy = true;
                    cmdGo.IsEnabled = false;
                    dp2.BeginExecute(ij);
                }
            }
        }
    }
}
