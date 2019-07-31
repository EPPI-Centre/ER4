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
using BusinessLibrary.BusinessClasses;
using Csla;
using Telerik.Windows.Controls;

namespace EppiReviewer4
{
    public partial class dialogMagBrowser : UserControl
    {
        public dialogMagBrowser()
        {
            InitializeComponent();
        }

        public void ShowMagBrowser()
        {
            DataPortal<MagCurrentInfo> dp = new DataPortal<MagCurrentInfo>();
            MagCurrentInfo mci = new MagCurrentInfo();
            dp.FetchCompleted += (o, e2) =>
            {
                //BusyLoading.IsRunning = false;
                if (e2.Error != null)
                {
                    RadWindow.Alert(e2.Error.Message);
                }
                else
                {
                    MagCurrentInfo mci2 = e2.Object as MagCurrentInfo;
                    if (mci2.CurrentAvailability == "available")
                    {
                        tbAcademicTitle.Text = "Microsoft Academic dataset last updated: " + mci2.LastUpdated.ToString();
                    }
                    else
                    {
                        tbAcademicTitle.Text = "Microsoft Academic dataset currently unavailable";
                    }
                    
                }
            };
            //BusyLoading.IsRunning = true;
            dp.BeginFetch(mci);
        }

        private void HLShowSummary_Click(object sender, RoutedEventArgs e)
        {
            StatusGrid.Visibility = Visibility.Visible;
            PaperGrid.Visibility = Visibility.Collapsed;
            TopicsGrid.Visibility = Visibility.Collapsed;
            HistoryGrid.Visibility = Visibility.Collapsed;
        }

        private void HLShowHistory_Click(object sender, RoutedEventArgs e)
        {
            StatusGrid.Visibility = Visibility.Collapsed;
            PaperGrid.Visibility = Visibility.Collapsed;
            TopicsGrid.Visibility = Visibility.Collapsed;
            HistoryGrid.Visibility = Visibility.Visible;
        }

        private void HLShowSelected_Click(object sender, RoutedEventArgs e)
        {
            StatusGrid.Visibility = Visibility.Collapsed;
            PaperGrid.Visibility = Visibility.Visible;
            TopicsGrid.Visibility = Visibility.Collapsed;
            HistoryGrid.Visibility = Visibility.Collapsed;
        }
    }
}
