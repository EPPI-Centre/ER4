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
using Csla.Silverlight;
using BusinessLibrary.BusinessClasses;
using Csla;
using Csla.Core;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Data;
using Csla.Xaml;

namespace EppiReviewer4
{
    public partial class MetaAnalysisTraining : UserControl
    {

        private OutcomeList outcomeList;

        public MetaAnalysisTraining()
        {
            InitializeComponent();
        }

        public void LoadData()
        {
            outcomeList = new OutcomeList();
            GridData.ItemsSource = outcomeList;
        }

        public void LoadSampleData()
        {
            DataPortal<OutcomeList> dp = new DataPortal<OutcomeList>();
            dp.FetchCompleted += (o, e2) =>
            {
                if (e2.Object != null)
                {
                    outcomeList = e2.Object;
                    GridData.ItemsSource = outcomeList;
                }
            };
            dp.BeginFetch();
        }

        private void CslaDataProvider_OutcomeListDataDataChanged(object sender, EventArgs e)
        {
            CslaDataProvider provider = ((CslaDataProvider)this.Resources["OutcomeListData"]);
            if (provider.Error != null)
                System.Windows.Browser.HtmlPage.Window.Alert(((Csla.Xaml.CslaDataProvider)sender).Error.Message);
        }

        private void Calculate_Click(object sender, RoutedEventArgs e)
        {
            //CslaDataProvider provider = ((CslaDataProvider)this.Resources["OutcomeListData"]);
            if (outcomeList != null)
            {
                MetaAnalysis ma = new MetaAnalysis();
                MetaAnalyse metaAnalyse = new MetaAnalyse();
                
                ma.Outcomes = outcomeList;
                ma.MetaAnalysisTypeId = 0;
                metaAnalyse.MetaAnalysis1 = ma;
                metaAnalyse.MetaAnalysis2 = null;
                metaAnalyse.ShowMetaLabels = false;
                metaAnalyse.ShowPooledES = true;
                metaAnalyse.ShowSummaryLine = true;
                metaAnalyse.LabelLHS = "";
                metaAnalyse.LabelRHS = "";
                ma.run(-1);

                double iSquared = ma.Q == 0 ? 0 : Math.Max(100.0 * (ma.Q - (ma.numStudies - 1)) / ma.Q, 0);
                double p2 = 1 - StatFunctions.pchisq(Convert.ToDouble(ma.Q), Convert.ToDouble(ma.numStudies - 1));

                feResults.Text = "Pooled effect: " + ma.feEffect.ToString("G3") + " (" + ma.feCiLower.ToString("G3") + ", " + ma.feCiUpper.ToString("G3") + ")";
                feHeterogeneity.Text = "Heterogeneity: Q = " + ma.Q.ToString("G3") + "\ndf = " + (ma.numStudies - 1).ToString() +
                    "\np = " + p2.ToString("G3") + "\nI-squared = " + iSquared.ToString("G3") + "%";
                feFileDrawerZ.Text = "File drawer N = " + Math.Abs(ma.FileDrawerZ).ToString("G3");
                reResults.Text = "Pooled effect: " + ma.reEffect.ToString("G3") + " (" + ma.reCiLower.ToString("G3") + ", " + ma.reCiUpper.ToString("G3") + ")";
                reHeterogeneity.Text = feHeterogeneity.Text;

                /*
                metaAnalyse.feEffect = ma.feEffect;
                metaAnalyse.feSE = ma.feSE;
                metaAnalyse.reEffect = ma.reEffect;
                metaAnalyse.reSE = ma.reSE;
                metaAnalyse.feSumWeight = ma.sumWeights;
                metaAnalyse.reSumWeight = ma.sumReWeights;
                metaAnalyse.Outcomes = outcomeList;
                */

                metaAnalyse.Saved += (o, e2) =>
                {
                    if (e2.NewObject != null)
                    {
                        metaAnalyse = e2.NewObject as MetaAnalyse;
                        GridData.ItemsSource = outcomeList;
                        MemoryStream ms = new MemoryStream(metaAnalyse.feForestPlot);
                        BitmapImage bm = new BitmapImage();
                        bm.SetSource(ms);
                        ImageFePlot.Source = bm;

                        MemoryStream ms2 = new MemoryStream(metaAnalyse.reForestPlot);
                        BitmapImage bm2 = new BitmapImage();
                        bm2.SetSource(ms2);
                        ImageRePlot.Source = bm2;

                        MemoryStream ms3 = new MemoryStream(metaAnalyse.feFunnelPlot);
                        BitmapImage bm3 = new BitmapImage();
                        bm3.SetSource(ms3);
                        ImageFunnelPlot.Source = bm3;
                    }
                };
                metaAnalyse.BeginSave(true);
            }
        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            if (outcomeList != null)
            {
                Outcome outcome = new Outcome();
                outcome.OutcomeTypeId = 1;
                outcome.Title = "New study";
                outcome.ShortTitle = "New study";
                outcome.Data1 = 0;
                outcome.Data2 = 0;
                outcome.Data3 = 0;
                outcome.Data4 = 0;
                outcome.Data5 = 0;
                outcome.Data6 = 0;
                outcome.IsSelected = true;
                outcomeList.Add(outcome);
            }
            else
            {
                MessageBox.Show("Generate data first");
            }
        }

        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            if (GridData.SelectedItem != null)
                outcomeList.Remove(GridData.SelectedItem as Outcome);
        }

        private void ButtonLoadData_Click(object sender, RoutedEventArgs e)
        {
            LoadSampleData();
        }

        private void ButtonDeleteAll_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

    }
}
