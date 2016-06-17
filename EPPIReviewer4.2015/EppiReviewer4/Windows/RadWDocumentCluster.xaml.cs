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
    }
}

