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
using System.IO;
using System.Windows.Media.Imaging;
using Telerik.Windows.Controls;
using Csla.Silverlight;
using Csla;

namespace EppiReviewer4
{
    public partial class dialogMetaAnalysis : UserControl
    {
        public dialogMetaAnalysis()
        {
            InitializeComponent();
        }

        //public event EventHandler<ItemListRefreshEventArgs> RefreshItemList;

        public void RunMetaAnalysis(MetaAnalysis ma)
        {
            TextBlockTitle.Text = ma.Title;
            this.DataContext = ma;

            DataPortal<OutcomeList> dp = new DataPortal<OutcomeList>();
            dp.FetchCompleted += (o, e2) =>
            {
                if (e2.Error != null)
                {
                    MessageBox.Show(e2.Error.Message);
                }
                else
                {
                    ma = this.DataContext as MetaAnalysis;
                    ma.Outcomes = e2.Object as OutcomeList;
                    SetGridColumns();
                    TextBlockDetails.Text =
                        (ma.OutcomeText != "" ? "Outcome: " + ma.OutcomeText + ", " : "") +
                        (ma.InterventionText != "" ? "Intervention: " + ma.InterventionText + ", " : "") +
                        (ma.ControlText != "" ? "Comparison: " + ma.ControlText + ", " : "") +
                        (ma.MetaAnalysisTypeTitle != "" ? "measure: " + ma.MetaAnalysisTypeTitle : "");
                    MetaAnalyse metaAnalyse = new MetaAnalyse();
                    ma.run(-1);

                    double iSquared = ma.Q == 0 ? 0 : Math.Max(100.0 * (ma.Q - (ma.numStudies - 1)) / ma.Q, 0);
                    double p2 = 1 - StatFunctions.pchisq(Convert.ToDouble(ma.Q), Convert.ToDouble(ma.numStudies - 1));

                    feResults.Text = "Pooled effect: " + ma.feEffect.ToString("G3") + " (" + ma.feCiLower.ToString("G3") + ", " + ma.feCiUpper.ToString("G3") + ")";
                    feHeterogeneity.Text = "Heterogeneity: Q = " + ma.Q.ToString("G3") + "\ndf = " + (ma.numStudies - 1).ToString() +
                        "\np = " + p2.ToString("G3") + "\nI-squared = " + iSquared.ToString("G3") + "%";
                    feFileDrawerZ.Text = "File drawer N = " + Math.Abs(ma.FileDrawerZ).ToString("G3");
                    reResults.Text = "Pooled effect: " + ma.reEffect.ToString("G3") + " (" + ma.reCiLower.ToString("G3") + ", " + ma.reCiUpper.ToString("G3") + ")";
                    reHeterogeneity.Text = feHeterogeneity.Text;

                    metaAnalyse.Saved += (o2, e22) =>
                    {
                        if (e22.NewObject != null)
                        {
                            metaAnalyse = e22.NewObject as MetaAnalyse;
                            GridData.ItemsSource = ma.Outcomes;
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
            };
            dp.BeginFetch(new BusinessLibrary.BusinessClasses.OutcomeList.OutcomeListSelectionCriteria(typeof(OutcomeList), ma.SetId, ma.AttributeIdIntervention,
                ma.AttributeIdControl, ma.AttributeIdOutcome, 0, ma.MetaAnalysisId, ma.AttributeIdQuestion, ma.AttributeIdAnswer));
        }

        private void cmdReCalculate_Click(object sender, RoutedEventArgs e)
        {
            RunMetaAnalysis(this.DataContext as MetaAnalysis);
        }

        private void SetGridColumns()
        {
            /* Meta-analysis types:
                *     0: Continuous: d (Hedges g)
                *     1: Continuous: r
                *     2: Binary: odds ratio
                *     3: Binary: risk ratio
                *     4: Binary: risk difference
                *     5: Binary: diagnostic test OR
                *     6: Binary: Peto OR
                *     7: Continuous: mean difference
                */
            switch ((this.DataContext as MetaAnalysis).MetaAnalysisTypeId)
            {
                case 0: // SMD
                    GridData.Columns[3].IsVisible = true;
                    GridData.Columns[4].IsVisible = true;
                    GridData.Columns[5].IsVisible = false;
                    GridData.Columns[6].IsVisible = false;
                    GridData.Columns[7].IsVisible = false;
                    GridData.Columns[8].IsVisible = false;
                    GridData.Columns[9].IsVisible = false;
                    GridData.Columns[10].IsVisible = false;
                    GridData.Columns[11].IsVisible = false;
                    GridData.Columns[12].IsVisible = false;
                    GridData.Columns[13].IsVisible = false;
                    GridData.Columns[14].IsVisible = false;
                    GridData.Columns[15].IsVisible = false;
                    GridData.Columns[16].IsVisible = false;
                    (GridData.Columns[17] as GridViewDataColumn).DataMemberBinding = new System.Windows.Data.Binding("CILowerSMD");
                    (GridData.Columns[18] as GridViewDataColumn).DataMemberBinding = new System.Windows.Data.Binding("CIUpperSMD"); 
                    break;
                case 1:
                    GridData.Columns[3].IsVisible = false;
                    GridData.Columns[4].IsVisible = false;
                    GridData.Columns[5].IsVisible = true;
                    GridData.Columns[6].IsVisible = true;
                    GridData.Columns[7].IsVisible = false;
                    GridData.Columns[8].IsVisible = false;
                    GridData.Columns[9].IsVisible = false;
                    GridData.Columns[10].IsVisible = false;
                    GridData.Columns[11].IsVisible = false;
                    GridData.Columns[12].IsVisible = false;
                    GridData.Columns[13].IsVisible = false;
                    GridData.Columns[14].IsVisible = false;
                    GridData.Columns[15].IsVisible = false;
                    GridData.Columns[16].IsVisible = false;
                    (GridData.Columns[17] as GridViewDataColumn).DataMemberBinding = new System.Windows.Data.Binding("");
                    (GridData.Columns[18] as GridViewDataColumn).DataMemberBinding = new System.Windows.Data.Binding(""); 
                    break;
                case 2:
                    GridData.Columns[3].IsVisible = false;
                    GridData.Columns[4].IsVisible = false;
                    GridData.Columns[5].IsVisible = false;
                    GridData.Columns[6].IsVisible = false;
                    GridData.Columns[7].IsVisible = true;
                    GridData.Columns[8].IsVisible = true;
                    GridData.Columns[9].IsVisible = false;
                    GridData.Columns[10].IsVisible = false;
                    GridData.Columns[11].IsVisible = false;
                    GridData.Columns[12].IsVisible = false;
                    GridData.Columns[13].IsVisible = false;
                    GridData.Columns[14].IsVisible = false;
                    GridData.Columns[15].IsVisible = false;
                    GridData.Columns[16].IsVisible = false;
                    (GridData.Columns[17] as GridViewDataColumn).DataMemberBinding = new System.Windows.Data.Binding("CILowerOddsRatio");
                    (GridData.Columns[18] as GridViewDataColumn).DataMemberBinding = new System.Windows.Data.Binding("CIUpperOddsRatio"); 
                    break;
                case 3:
                    GridData.Columns[3].IsVisible = false;
                    GridData.Columns[4].IsVisible = false;
                    GridData.Columns[5].IsVisible = false;
                    GridData.Columns[6].IsVisible = false;
                    GridData.Columns[7].IsVisible = false;
                    GridData.Columns[8].IsVisible = false;
                    GridData.Columns[9].IsVisible = true;
                    GridData.Columns[10].IsVisible = true;
                    GridData.Columns[11].IsVisible = false;
                    GridData.Columns[12].IsVisible = false;
                    GridData.Columns[13].IsVisible = false;
                    GridData.Columns[14].IsVisible = false;
                    GridData.Columns[15].IsVisible = false;
                    GridData.Columns[16].IsVisible = false;
                    (GridData.Columns[17] as GridViewDataColumn).DataMemberBinding = new System.Windows.Data.Binding("CILowerRiskRatio");
                    (GridData.Columns[18] as GridViewDataColumn).DataMemberBinding = new System.Windows.Data.Binding("CIUpperRiskRatio"); 
                    break;
                case 4:
                    GridData.Columns[3].IsVisible = false;
                    GridData.Columns[4].IsVisible = false;
                    GridData.Columns[5].IsVisible = false;
                    GridData.Columns[6].IsVisible = false;
                    GridData.Columns[7].IsVisible = false;
                    GridData.Columns[8].IsVisible = false;
                    GridData.Columns[9].IsVisible = false;
                    GridData.Columns[10].IsVisible = false;
                    GridData.Columns[11].IsVisible = true;
                    GridData.Columns[12].IsVisible = true;
                    GridData.Columns[13].IsVisible = false;
                    GridData.Columns[14].IsVisible = false;
                    GridData.Columns[15].IsVisible = false;
                    GridData.Columns[16].IsVisible = false;
                    (GridData.Columns[17] as GridViewDataColumn).DataMemberBinding = new System.Windows.Data.Binding("CILowerRiskDifference");
                    (GridData.Columns[18] as GridViewDataColumn).DataMemberBinding = new System.Windows.Data.Binding("CIUpperRiskDifference"); 
                    break;
                case 5:
                    GridData.Columns[3].IsVisible = false;
                    GridData.Columns[4].IsVisible = false;
                    GridData.Columns[5].IsVisible = false;
                    GridData.Columns[6].IsVisible = false;
                    GridData.Columns[7].IsVisible = true;
                    GridData.Columns[8].IsVisible = true;
                    GridData.Columns[9].IsVisible = false;
                    GridData.Columns[10].IsVisible = false;
                    GridData.Columns[11].IsVisible = false;
                    GridData.Columns[12].IsVisible = false;
                    GridData.Columns[13].IsVisible = false;
                    GridData.Columns[14].IsVisible = false;
                    GridData.Columns[15].IsVisible = false;
                    GridData.Columns[16].IsVisible = false;
                    (GridData.Columns[17] as GridViewDataColumn).DataMemberBinding = new System.Windows.Data.Binding("CILowerOddsRatio");
                    (GridData.Columns[18] as GridViewDataColumn).DataMemberBinding = new System.Windows.Data.Binding("CIUpperOddsRatio"); 
                    break;
                case 6:
                    GridData.Columns[3].IsVisible = false;
                    GridData.Columns[4].IsVisible = false;
                    GridData.Columns[5].IsVisible = false;
                    GridData.Columns[6].IsVisible = false;
                    GridData.Columns[7].IsVisible = false;
                    GridData.Columns[8].IsVisible = false;
                    GridData.Columns[9].IsVisible = false;
                    GridData.Columns[10].IsVisible = false;
                    GridData.Columns[11].IsVisible = false;
                    GridData.Columns[12].IsVisible = false;
                    GridData.Columns[13].IsVisible = true;
                    GridData.Columns[14].IsVisible = true;
                    GridData.Columns[15].IsVisible = false;
                    GridData.Columns[16].IsVisible = false;
                    (GridData.Columns[17] as GridViewDataColumn).DataMemberBinding = new System.Windows.Data.Binding("CILowerPetoOddsRatio");
                    (GridData.Columns[18] as GridViewDataColumn).DataMemberBinding = new System.Windows.Data.Binding("CIUpperPetoOddsRatio"); 
                    break;
                case 7:
                    GridData.Columns[3].IsVisible = false;
                    GridData.Columns[4].IsVisible = false;
                    GridData.Columns[5].IsVisible = false;
                    GridData.Columns[6].IsVisible = false;
                    GridData.Columns[7].IsVisible = false;
                    GridData.Columns[8].IsVisible = false;
                    GridData.Columns[9].IsVisible = false;
                    GridData.Columns[10].IsVisible = false;
                    GridData.Columns[11].IsVisible = false;
                    GridData.Columns[12].IsVisible = false;
                    GridData.Columns[13].IsVisible = false;
                    GridData.Columns[14].IsVisible = false;
                    GridData.Columns[15].IsVisible = true;
                    GridData.Columns[16].IsVisible = true;
                    (GridData.Columns[17] as GridViewDataColumn).DataMemberBinding = new System.Windows.Data.Binding("CILowerMeanDifference");
                    (GridData.Columns[18] as GridViewDataColumn).DataMemberBinding = new System.Windows.Data.Binding("CIUpperMeanDifference"); 
                    break;
            }
        }
        
    }
}
