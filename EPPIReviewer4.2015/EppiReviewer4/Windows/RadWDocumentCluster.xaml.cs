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

namespace EppiReviewer4
{
    /// <summary>
    /// Interaction logic for RadWbyS.xaml
    /// </summary>
    public partial class RadWDocumentCluster : RadWindow
    {
        public event EventHandler<System.Windows.Controls.SelectionChangedEventArgs> ClusterWhat_SelectionChanged;
        public event EventHandler<RoutedEventArgs> cmdCluster_Clicked;
        public event EventHandler<RoutedEventArgs> cmdGetMicrosoftAcademicTopics_Clicked;


        public RadWDocumentCluster()
        {
            InitializeComponent();
        }
        
        private void ComboClusterWhat_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (ClusterWhat_SelectionChanged != null) ClusterWhat_SelectionChanged.Invoke(sender, e);
        }
        private void cmdCluster_Click(object sender, RoutedEventArgs e)
        {
            if (cmdCluster_Clicked != null) cmdCluster_Clicked.Invoke(sender, e);
        }

        private void cmdGetMicrosoftAcademicTopics_Click(object sender, RoutedEventArgs e)
        {
            if (cmdGetMicrosoftAcademicTopics_Clicked != null) cmdGetMicrosoftAcademicTopics_Clicked.Invoke(sender, e);
        }

        private void rbClusterExistingCodeSet_Checked(object sender, RoutedEventArgs e)
        {
            dialogClusterComboSelectCodeSet.IsEnabled = true;
        }

        private void rbClusterNewCodeSet_Checked(object sender, RoutedEventArgs e)
        {
            if (dialogClusterComboSelectCodeSet != null)
                    dialogClusterComboSelectCodeSet.IsEnabled = false;
        }

        private void dialogClusterMinItems_ValueChanged(object sender, RadRangeBaseValueChangedEventArgs e)
        {
            if (dialogClusterMinItems != null && dialogClusterThresholdWarning != null && dialogClusterMinItems.Value.Value < 5)
            {
                dialogClusterThresholdWarning.Visibility = Visibility.Visible;
            }
            else
            {
                if (dialogClusterThresholdWarning != null)
                    dialogClusterThresholdWarning.Visibility = Visibility.Collapsed;
            }
        }
    }
}

