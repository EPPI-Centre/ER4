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
    
    public partial class RadWEditAnnotation : RadWindow 
    {

        public RadWEditAnnotation()
        {
            InitializeComponent();
        }

        private void btSaveNote_Click(object sender, RoutedEventArgs e)
        {
            (this.DataContext as xAnnotation).Text = this.Text.Text;
            if ((string)this.Header == "New Annotation")
            {
                (this.DataContext as xAnnotation).isDeleted = false;
            }
            this.Close();
        }

        private void btDelete_Click(object sender, RoutedEventArgs e)
        {
            (this.DataContext as xAnnotation).isDeleted = true;
            this.Close();
        }

        private void btCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
