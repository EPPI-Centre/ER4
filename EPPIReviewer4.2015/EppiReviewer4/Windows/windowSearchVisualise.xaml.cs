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

        /*
        private void Provider_DataChanged(object sender, EventArgs e)
        {
            
            CslaDataProvider provider = sender as CslaDataProvider;
            SearchVisualiseList svl = provider.Data as SearchVisualiseList;
            chart.Series.Clear();
            BarSeries barSeries = new BarSeries() { ShowLabels = true };
            barSeries.CategoryBinding = new PropertyNameDataPointBinding() { PropertyName = "Range" };
            barSeries.ValueBinding = new GenericDataPointBinding<SearchVisualise, Int32>() { ValueSelector = SearchVisualise => SearchVisualise.Count };
            barSeries.ItemsSource = svl;
            chart.Series.Add(barSeries);
            provider.DataChanged -= Provider_DataChanged;
        }
        */

        public int SearchId { get; set; }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void ChartSelectionBehavior_SelectionChanged(object sender, ChartSelectionChangedEventArgs e)
        {

        }
    }
}

