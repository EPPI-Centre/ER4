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
using Csla.Xaml;
using BusinessLibrary.BusinessClasses;
using Csla.Silverlight;
using Csla;
using Telerik.Windows.Controls.ChartView;
using System.IO;

namespace EppiReviewer4.Windows
{
    public partial class windowSearchVisualise : ChildWindow
    {
        public windowSearchVisualise()
        {
            InitializeComponent();
        }

        public void getSearchData(int searchId)
        {
            CslaDataProvider provider = App.Current.Resources["SearchVisualiseData"] as CslaDataProvider;
            if (provider != null)
            {
                provider.FactoryParameters.Clear();
                provider.FactoryParameters.Add(searchId);
                provider.FactoryMethod = "GetSearchVisualiseList";
                provider.Refresh();
            }
        }
        //private void Provider_DataChanged(object sender, EventArgs e)
        //{
            
        //    CslaDataProvider provider = sender as CslaDataProvider;
        //    SearchVisualiseList svl = provider.Data as SearchVisualiseList;
        //    chart.Series.Clear();
        //    BarSeries barSeries = new BarSeries() { ShowLabels = true };
        //    barSeries.CategoryBinding = new PropertyNameDataPointBinding() { PropertyName = "Range" };
        //    barSeries.ValueBinding = new GenericDataPointBinding<SearchVisualise, Int32>() { ValueSelector = SearchVisualise => SearchVisualise.Count };
        //    barSeries.ItemsSource = svl;
        //    chart.Series.Add(barSeries);
        //    provider.DataChanged -= Provider_DataChanged;
        //}

        public int SearchId { get; set; }
        private string _SearchName;
        public string SearchName
        {
            get { return _SearchName; }
            set
            {
                _SearchName = value.Replace("Items classified according to model: ", "");
                this.Title = "Distribution of classifier scores - Model: " + _SearchName;
            }
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.DefaultExt = "*.png";
            dialog.Filter = "Files(*.png)|*.png";
            if (!(bool)dialog.ShowDialog())
                return;
            Telerik.Windows.Media.Imaging.PngBitmapEncoder enc = new Telerik.Windows.Media.Imaging.PngBitmapEncoder();
            using (Stream fileStream = dialog.OpenFile())
            {
                Telerik.Windows.Media.Imaging.ExportExtensions.ExportToImage(chart, fileStream, enc);
            }
        }

        private void ChartSelectionBehavior_SelectionChanged(object sender, ChartSelectionChangedEventArgs e)
        {

        }
    }
}

