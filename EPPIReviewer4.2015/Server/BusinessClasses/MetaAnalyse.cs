using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Csla;
using Csla.Security;
using Csla.Core;
using Csla.Serialization;
using Csla.Silverlight;
//using Csla.Validation;
using Csla.DataPortalClient;

#if!SILVERLIGHT
using System.Data.SqlClient;
using BusinessLibrary.Data;
using Csla.Data;
using BusinessLibrary.Security;
using MetaGraphs.ForestPlot;
using MetaGraphs.FunnelPlot;
using System.Drawing;
using System.IO;
#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class MetaAnalyse : BusinessBase<MetaAnalyse>
    {
        public static void GetMetaAnalysis(EventHandler<DataPortalResult<MetaAnalyse>> handler)
        {
            DataPortal<MetaAnalyse> dp = new DataPortal<MetaAnalyse>();
            dp.FetchCompleted += handler;
            dp.BeginFetch();
        }

#if SILVERLIGHT
    public MetaAnalyse()
        {
            
        }

        
#else
        private MetaAnalyse() { }
#endif
        //comparers used to sortBY different qualities
        public static int CompareOutcomesShortTitle(Outcome x, Outcome y)
        {
            if (x == null)
            {
                if (y == null)
                {
                    // If x is null and y is null, they're
                    // equal. 
                    return 0;
                }
                else
                {
                    // If x is null and y is not null, y
                    // is greater. 
                    return -1;
                }
            }
            else
            {
                // If x is not null...
                //
                if (y == null)
                // ...and y is null, x is greater.
                {
                    return 1;
                }
                else
                {
                    // ...and y is not null, compare the 
                    // lengths of the two strings.
                    //
                    return x.ShortTitle.CompareTo(y.ShortTitle);
                }
            }
        }
        public static int CompareOutcomesES(Outcome x, Outcome y)
        {
            if (x == null)
            {
                if (y == null)
                {
                    // If x is null and y is null, they're
                    // equal. 
                    return 0;
                }
                else
                {
                    // If x is null and y is not null, y
                    // is greater. 
                    return -1;
                }
            }
            else
            {
                // If x is not null...
                //
                if (y == null)
                // ...and y is null, x is greater.
                {
                    return 1;
                }
                else
                {
                    // ...and y is not null, compare the 
                    // lengths of the two strings.
                    //
                    return x.ES.CompareTo(y.ES);
                }
            }
        }
        //end of comparers
        private static PropertyInfo<string> TitleProperty = RegisterProperty<string>(new PropertyInfo<string>("Title", "Title", string.Empty));
        public string Title
        {
            get
            {
                return GetProperty(TitleProperty);
            }
            set
            {
                SetProperty(TitleProperty, value);
            }
        }

        private static PropertyInfo<Byte[]> feForestPlotProperty = RegisterProperty<Byte[]>(new PropertyInfo<Byte[]>("feForestPlot", "feForestPlot"));
        public Byte[] feForestPlot
        {
            get
            {
                return GetProperty(feForestPlotProperty);
            }
            set
            {
                SetProperty(feForestPlotProperty, value);
            }
        }

        private static PropertyInfo<Byte[]> reForestPlotProperty = RegisterProperty<Byte[]>(new PropertyInfo<Byte[]>("reForestPlot", "reForestPlot"));
        public Byte[] reForestPlot
        {
            get
            {
                return GetProperty(reForestPlotProperty);
            }
            set
            {
                SetProperty(reForestPlotProperty, value);
            }
        }

        private static PropertyInfo<Byte[]> feFunnelPlotProperty = RegisterProperty<Byte[]>(new PropertyInfo<Byte[]>("feFunnelPlot", "feFunnelPlot"));
        public Byte[] feFunnelPlot
        {
            get
            {
                return GetProperty(feFunnelPlotProperty);
            }
            set
            {
                SetProperty(feFunnelPlotProperty, value);
            }
        }

        private static PropertyInfo<MetaAnalysis> MetaAnalysis1Property = RegisterProperty<MetaAnalysis>(new PropertyInfo<MetaAnalysis>("MetaAnalysis1", "MetaAnalysis1"));
        public MetaAnalysis MetaAnalysis1
        {
            get
            {
                return GetProperty(MetaAnalysis1Property);
            }
            set
            {
                SetProperty(MetaAnalysis1Property, value);
            }
        }

        private static PropertyInfo<MetaAnalysis> MetaAnalysis2Property = RegisterProperty<MetaAnalysis>(new PropertyInfo<MetaAnalysis>("MetaAnalysis2", "MetaAnalysis2"));
        public MetaAnalysis MetaAnalysis2
        {
            get
            {
                return GetProperty(MetaAnalysis2Property);
            }
            set
            {
                SetProperty(MetaAnalysis2Property, value);
            }
        }

        private static PropertyInfo<MetaAnalysis> MetaAnalysisSubGroupProperty = RegisterProperty<MetaAnalysis>(new PropertyInfo<MetaAnalysis>("MetaAnalysisSubGroup", "MetaAnalysisSubGroup"));
        public MetaAnalysis MetaAnalysisSubGroup
        {
            get
            {
                return GetProperty(MetaAnalysisSubGroupProperty);
            }
            set
            {
                SetProperty(MetaAnalysisSubGroupProperty, value);
            }
        }

        private static PropertyInfo<string> LabelLHSProperty = RegisterProperty<string>(new PropertyInfo<string>("LabelLHS", "LabelLHS"));
        public string LabelLHS
        {
            get
            {
                return GetProperty(LabelLHSProperty);
            }
            set
            {
                SetProperty(LabelLHSProperty, value);
            }
        }

        private static PropertyInfo<string> LabelRHSProperty = RegisterProperty<string>(new PropertyInfo<string>("LabelRHS", "LabelRHS"));
        public string LabelRHS
        {
            get
            {
                return GetProperty(LabelRHSProperty);
            }
            set
            {
                SetProperty(LabelRHSProperty, value);
            }
        }

        private static PropertyInfo<string> SortByProperty = RegisterProperty<string>(new PropertyInfo<string>("SortBy", "SortBy"));
        public string SortBy
        {
            get
            {
                return GetProperty(SortByProperty);
            }
            set
            {
                SetProperty(SortByProperty, value);
            }
        }

        private static PropertyInfo<bool> ShowPooledESProperty = RegisterProperty<bool>(new PropertyInfo<bool>("ShowPooledES", "ShowPooledES"));
        public bool ShowPooledES
        {
            get
            {
                return GetProperty(ShowPooledESProperty);
            }
            set
            {
                SetProperty(ShowPooledESProperty, value);
            }
        }

        private static PropertyInfo<bool> ShowSummaryLineProperty = RegisterProperty<bool>(new PropertyInfo<bool>("ShowSummaryLine", "ShowSummaryLine"));
        public bool ShowSummaryLine
        {
            get
            {
                return GetProperty(ShowSummaryLineProperty);
            }
            set
            {
                SetProperty(ShowSummaryLineProperty, value);
            }
        }

        private static PropertyInfo<bool> ShowMetaLabelsProperty = RegisterProperty<bool>(new PropertyInfo<bool>("ShowMetaLabels", "ShowMetaLabels"));
        public bool ShowMetaLabels
        {
            get
            {
                return GetProperty(ShowMetaLabelsProperty);
            }
            set
            {
                SetProperty(ShowMetaLabelsProperty, value);
            }
        }

        public void SubGroupMA()
        {
            if (MetaAnalysis1 != null && MetaAnalysis2 != null)
            {
                MetaAnalysis1.run(-1);
                MetaAnalysis2.run(-1);

                double sumWeight = MetaAnalysis1.feSumWeight + MetaAnalysis2.feSumWeight;
                double sumSquaredWeight = Math.Pow(MetaAnalysis1.feSumWeight, 2) + Math.Pow(MetaAnalysis2.feSumWeight, 2);
                double C = sumWeight - (sumSquaredWeight / sumWeight);
                double tauSquared = ((MetaAnalysis1.Q + MetaAnalysis2.Q) - (MetaAnalysis1.numStudies + MetaAnalysis2.numStudies - 1)) / sumWeight;

                MetaAnalysisSubGroup = new MetaAnalysis();
                MetaAnalysisSubGroup.MetaAnalysisTypeId = MetaAnalysis1.MetaAnalysisTypeId;
                MetaAnalysisSubGroup.Outcomes = new OutcomeList();
                foreach (Outcome o in MetaAnalysis1.Outcomes)
                {
                    MetaAnalysisSubGroup.Outcomes.Add(o);
                }
                foreach (Outcome o in MetaAnalysis2.Outcomes)
                {
                    MetaAnalysisSubGroup.Outcomes.Add(o);
                }
                // Gives us a meta-analysis over all the studies with a pooled value for Tau-squared.
                //MetaAnalysisSubGroup.run(tauSquared);
                // Now redo the two analyses with the pooled tau-squared
                //MetaAnalysis1.run(-1);
                //MetaAnalysis2.run(-1);

                // Not using pooling - just weights from combining all studies in one big meta-analysis
                MetaAnalysisSubGroup.run(-1);
            }
        }

        public string SubGroupFeDifference()
        {
            string retVal = "";
            if (MetaAnalysis1 != null && MetaAnalysis2 != null)
            {
                double feDifference = Math.Abs(MetaAnalysis1.feEffect - MetaAnalysis2.feEffect);
                double feV = Math.Pow(MetaAnalysis1.feSE, 2) + Math.Pow(MetaAnalysis2.feSE, 2);
                double feSeDiff = Math.Sqrt(feV);
                double feZ = feDifference / feSeDiff;
                double feP = StatFunctions.pnorm(Math.Abs(feZ), true) * 2;

                double Q_within = MetaAnalysis1.Q + MetaAnalysis2.Q;
                double Q_between = this.MetaAnalysisSubGroup.Q - Q_within;

                retVal = "Difference: " + feDifference.ToString("G3") + "; SE difference: " + feSeDiff.ToString("G3") + "; Z: " +
                    feZ.ToString("G3") + "; p = " + feP.ToString("G3") + "; Q within: " + Q_within.ToString("G3") + "; Q between: " + Q_between.ToString("G3") + ".";
                    
            }
            return retVal;
        }

        public string SubGroupReDifference()
        {
            string retVal = "";
            if (MetaAnalysis1 != null && MetaAnalysis2 != null)
            {
                double reDifrerence = Math.Abs(MetaAnalysis1.reEffect - MetaAnalysis2.reEffect);
                double reV = Math.Pow(MetaAnalysis1.reSE, 2) + Math.Pow(MetaAnalysis2.reSE, 2);
                double reSeDiff = Math.Sqrt(reV);
                double reZ = reDifrerence / reSeDiff;
                double reP = StatFunctions.pnorm(Math.Abs(reZ), true) * 2;

                double reQTotal = (MetaAnalysis1.WY_squared + MetaAnalysis2.WY_squared) - (Math.Pow(MetaAnalysis1.reSumWeightsTimesOutcome + MetaAnalysis2.reSumWeightsTimesOutcome, 2) / (MetaAnalysis1.reSumWeight + MetaAnalysis2.reSumWeight));
                double Q_within = MetaAnalysis1.reQ + MetaAnalysis2.reQ;
                double Q_between = reQTotal - Q_within;

                //double sumWeight = MetaAnalysis1.feSumWeight + MetaAnalysis2.feSumWeight;
                //double sumSquaredWeight = Math.Pow(MetaAnalysis1.feSumWeight, 2) + Math.Pow(MetaAnalysis2.feSumWeight, 2);
                //double C = sumWeight - (sumSquaredWeight / sumWeight);

                double sumWeight = MetaAnalysis1.feSumWeight + MetaAnalysis2.feSumWeight;
                double sumSquaredWeight = Math.Pow(MetaAnalysis1.feSumWeight, 2) + Math.Pow(MetaAnalysis2.feSumWeight, 2);
                double C = sumWeight - (sumSquaredWeight / sumWeight);
                double tauSquared = ((MetaAnalysis1.Q + MetaAnalysis2.Q) - (MetaAnalysis1.numStudies + MetaAnalysis2.numStudies - 2)) / (MetaAnalysis1.C() + MetaAnalysis2.C());
                if (tauSquared < 0)
                    tauSquared = 0;
                double r_squared = 1 - (tauSquared / MetaAnalysisSubGroup.tauSquared);
                r_squared = Math.Max(0, r_squared);
                r_squared = Math.Min(1, r_squared) * 100;
                r_squared = Math.Round(r_squared);

                retVal = "Difference: " + reDifrerence.ToString("G3") + "; SE difference: " + reSeDiff.ToString("G3") + "; Z: " +
                    reZ.ToString("G3") + "; p = " + reP.ToString("G3") + "; Q* within: " + Q_within.ToString("G3") + "; Q* between: " + Q_between.ToString("G3") +
                    "; (Group 1 Q*: " + MetaAnalysis1.reQ.ToString("G3") + "; Group 2 Q*: " + MetaAnalysis2.reQ.ToString("G3") +
                    "); heterogeneity explained: " + r_squared.ToString("G3") + "%." + Environment.NewLine + "* N.B. the Q* statistics are calculated using random effects weights and are only used for the analysis of variance.";
            }
            return retVal;
        }

        //protected override void AddAuthorizationRules()
        //{
        //    /*
        //    string[] canWrite = new string[] { "AdminUser", "RegularUser" };
        //    string[] canRead = new string[] { "AdminUser", "RegularUser", "ReadOnlyUser" };
        //    string[] admin = new string[] { "AdminUser" };
        //    AuthorizationRules.AllowCreate(typeof(Item), admin);
        //    AuthorizationRules.AllowDelete(typeof(Item), admin);
        //    AuthorizationRules.AllowEdit(typeof(Item), canWrite);
        //    AuthorizationRules.AllowGet(typeof(Item), canRead);

        //    AuthorizationRules.AllowRead(TitleProperty, canRead);

        //    AuthorizationRules.AllowWrite(TitleProperty, canWrite);
        //    */
        //}

        protected override void AddBusinessRules()
        {
            //ValidationRules.AddRule(Csla.Validation.CommonRules.MaxValue<decimal>, new Csla.Validation.CommonRules.MaxValueRuleArgs<decimal>(ReviewCodeSetFte1Property, 1));
            //ValidationRules.AddRule(Csla.Validation.CommonRules.StringRequired, new Csla.Validation.RuleArgs(ReviewCodeSetNameProperty));
            //ValidationRules.AddRule(Csla.Validation.CommonRules.StringMaxLength, new Csla.Validation.CommonRules.MaxLengthRuleArgs(ReviewCodeSetNameProperty, 20));
        }

#if !SILVERLIGHT

        protected override void DataPortal_Insert()
        {
            
        }

        protected override void DataPortal_Update()
        {
            if (MetaAnalysis2 == null)
            {
                DoOneGroup();
            }
            else
            {
                DoTwoGroups();
            }
        }

        private void DoOneGroup()
        {
            // Options for both graphs
            GraphOptions gOpt = new GraphOptions();
            gOpt.AutoScale = true;
            gOpt.showGroups = false;
            gOpt.ShowTotal = ShowPooledES;
            gOpt.ShowSummaryLine = ShowSummaryLine;
            gOpt.Min = 0;
            gOpt.Max = 0;
            gOpt.ShowEffectLabels = ShowMetaLabels;
            gOpt.EffectLabel1 = LabelLHS;
            gOpt.EffectLabel2 = LabelRHS;

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
            string kind = "";
            switch (MetaAnalysis1.MetaAnalysisTypeId)
            {
                case 0: kind = "Hedges G"; break;
                case 1: kind = "Hedges G"; break;
                case 2: kind = "Odds Ratio"; break;
                case 3: kind = "Odds Ratio"; break;
                case 4: kind = "Hedges G"; break;
                case 5: kind = "Odds Ratio"; break;
                case 6: kind = "Odds Ratio"; break;
                case 7: kind = "Hedges G"; break;
            }

            // Fixed effect graph
            List<linesData> Al = new List<linesData>();
            List<linesData> Tl = new List<linesData>();
            linesData ToT = new linesData("Total", "", MetaAnalysis1.feEffect, MetaAnalysis1.feCiLower, MetaAnalysis1.feCiUpper, 0, 0);
            IEnumerable<Outcome> query = new List<Outcome>();
            switch (SortBy)
            {
                case "Short Title (desc)":
                    query = MetaAnalysis1.Outcomes.OrderByDescending(Outc => Outc.ShortTitle);
                    break;
                case "Effect Size (desc)":
                    query = MetaAnalysis1.Outcomes.OrderByDescending(Outc => Outc.ES);
                    break;
                case "Effect Size (ascending)":
                    query = MetaAnalysis1.Outcomes.OrderBy(Outc => Outc.ES);
                    break;
                default:
                    query = MetaAnalysis1.Outcomes.OrderBy(Outc => Outc.ShortTitle);
                    break;//<telerik:RadComboBoxItem Content="Effect Size" />
                //<telerik:RadComboBoxItem Content="Effect Size (desc)"  />
                //IEnumerable<Outcome> query = MetaAnalysis1.Outcomes.OrderBy(Outc => Outc.ES);
                

            }
            foreach (Outcome outcome in query)
            {
                if (outcome.IsSelected == true)
                {
                    Al.Add(OutcomeLinesData(outcome, MetaAnalysis1, MetaAnalysis1.feSumWeight, true));
                }

            }
            //else
            //{
            //    foreach (Outcome outcome in MetaAnalysis1.Outcomes)
            //    {
            //        if (outcome.IsSelected == true)
            //        {
            //            /*
            //            double CiUpperO = 0;
            //            double CiLowerO = 0;
            //            /* Meta-analysis types:
            //        *     0: Continuous: d (Hedges g)
            //        *     1: Continuous: r
            //        *     2: Binary: odds ratio
            //        *     3: Binary: risk ratio
            //        *     4: Binary: risk difference
            //        *     5: Binary: diagnostic test OR
            //        *     6: Binary: Peto OR
            //        *     7: Continuous: mean difference
                
            //            switch (MetaAnalysis1.MetaAnalysisTypeId)
            //            {
            //                case 0: CiUpperO = outcome.CIUpperSMD; CiLowerO = outcome.CILowerSMD; break;
            //                case 1: CiUpperO = 0; CiLowerO = 0; break;
            //                case 2: CiUpperO = outcome.CIUpperOddsRatio; CiLowerO = outcome.CILowerOddsRatio; break;
            //                case 3: CiUpperO = outcome.CIUpperRiskRatio; CiLowerO = outcome.CILowerRiskRatio; break;
            //                case 4: CiUpperO = outcome.CIUpperRiskDifference; CiLowerO = outcome.CILowerRiskDifference; break;
            //                case 5: CiUpperO = outcome.CIUpperOddsRatio; CiLowerO = outcome.CILowerOddsRatio; break;
            //                case 6: CiUpperO = outcome.CIUpperPetoOddsRatio; CiLowerO = outcome.CILowerPetoOddsRatio; break;
            //                case 7: CiUpperO = outcome.CIUpperMeanDifference; CiLowerO = outcome.CILowerMeanDifference; break;
            //            }
            //            double weight = (outcome.feWeight / MetaAnalysis1.feSumWeight) * 100;
            //            Al.Add(new linesData(outcome.ShortTitle, "", outcome.GetEffectSizeDisplaying(MetaAnalysis1.MetaAnalysisTypeId),
            //                CiLowerO,
            //                CiUpperO,
            //                float.Parse(weight.ToString()), Convert.ToInt32(outcome.Data1 + outcome.Data2)));
            //             */
            //            Al.Add(OutcomeLinesData(outcome, MetaAnalysis1, MetaAnalysis1.feSumWeight, true));
            //        }

            //    }
            //}//!!!!Still temp thing to sort as Jan needs!!
            MetaGraphs.ForestPlot.GraphAllData GAD = new MetaGraphs.ForestPlot.GraphAllData(Al, Tl, ToT, "Fixed effect plot", kind);
            Image img = ForestPL.makeForestPl(GAD, gOpt);

            MemoryStream ms = new MemoryStream();
            img.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            feForestPlot = ms.ToArray();

            // Random effects graph
            Al.Clear();
            Tl.Clear();
            linesData ToT2 = new linesData("Total", "", MetaAnalysis1.reEffect, MetaAnalysis1.reCiLower, MetaAnalysis1.reCiUpper, 0, 0);
            //if (Environment.MachineName.ToLower() == "ssru38") //!!!!temp thing to sort as Jan needs!!
            //{
            //    IEnumerable<Outcome> query = MetaAnalysis1.Outcomes.OrderBy(Outc => Outc.ES);
            //    foreach (Outcome outcome in query)
            //    {
            //        if (outcome.IsSelected == true)
            //        {
            //            Al.Add(OutcomeLinesData(outcome, MetaAnalysis1, MetaAnalysis1.feSumWeight, true));
            //        }

            //    }

            //}
            //else
            //{//!!!!Still temp thing to sort as Jan needs!!
                //foreach (Outcome outcome in MetaAnalysis1.Outcomes)
                //{
                //    if (outcome.IsSelected == true)
                //    {
                //        Al.Add(OutcomeLinesData(outcome, MetaAnalysis1, MetaAnalysis1.reSumWeight, false));
                //    }
                //}
            //}//!!!!Still temp thing to sort as Jan needs!!
                foreach (Outcome outcome in query)
                {
                    if (outcome.IsSelected == true)
                    {
                        Al.Add(OutcomeLinesData(outcome, MetaAnalysis1, MetaAnalysis1.feSumWeight, true));
                    }

                }
            MetaGraphs.ForestPlot.GraphAllData GAD2 = new MetaGraphs.ForestPlot.GraphAllData(Al, Tl, ToT2, "Random effects plot", kind);

            Image img2 = ForestPL.makeForestPl(GAD2, gOpt);

            MemoryStream ms2 = new MemoryStream();
            img2.Save(ms2, System.Drawing.Imaging.ImageFormat.Png);
            reForestPlot = ms2.ToArray();

            // funnel plot code
            double totEff = 1;
            List<fPoint> pnts = new List<fPoint>();
            foreach (Outcome outcome in MetaAnalysis1.Outcomes)
            {
                if (outcome.IsSelected == true)
                {
                    pnts.Add(new fPoint(outcome.ES, outcome.SEES));
                }
            }
            totEff = MetaAnalysis1.feEffect;
            MetaGraphs.FunnelPlot.GraphAllData funGAD =
                new MetaGraphs.FunnelPlot.GraphAllData(pnts, "Standard error", "Effect size", kind, totEff);
            Image imgFunnel = FunnelPL.MakeFunnelPlot(funGAD);

            MemoryStream ms3 = new MemoryStream();
            imgFunnel.Save(ms3, System.Drawing.Imaging.ImageFormat.Png);
            feFunnelPlot = ms3.ToArray();
        }        
        private linesData OutcomeLinesData(Outcome outcome, MetaAnalysis ma, double sumWeight, bool fixedEffect)
        {
            double CiUpperO = 0;
            double CiLowerO = 0;
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
            switch (ma.MetaAnalysisTypeId)
            {
                case 0: CiUpperO = outcome.CIUpperSMD; CiLowerO = outcome.CILowerSMD; break;
                case 1: CiUpperO = outcome.CIUpperR; CiLowerO = outcome.CILowerR; break;
                case 2: CiUpperO = outcome.CIUpperOddsRatio; CiLowerO = outcome.CILowerOddsRatio; break;
                case 3: CiUpperO = outcome.CIUpperRiskRatio; CiLowerO = outcome.CILowerRiskRatio; break;
                case 4: CiUpperO = outcome.CIUpperRiskDifference; CiLowerO = outcome.CILowerRiskDifference; break;
                case 5: CiUpperO = outcome.CIUpperOddsRatio; CiLowerO = outcome.CILowerOddsRatio; break;
                case 6: CiUpperO = outcome.CIUpperPetoOddsRatio; CiLowerO = outcome.CILowerPetoOddsRatio; break;
                case 7: CiUpperO = outcome.CIUpperMeanDifference; CiLowerO = outcome.CILowerMeanDifference; break;
            }
            double weight = ((fixedEffect == true ? outcome.feWeight : outcome.reWeight) / sumWeight) * 100;
            return (new linesData(outcome.ShortTitle, ma.Title, outcome.GetEffectSizeDisplaying(MetaAnalysis1.MetaAnalysisTypeId),
                CiLowerO,
                CiUpperO,
                float.Parse(weight.ToString()),
                Convert.ToInt32(outcome.Data1 + outcome.Data2)));
        }

        private void DoTwoGroups()
        {
            // Options for both graphs
            GraphOptions gOpt = new GraphOptions();
            gOpt.AutoScale = true;
            gOpt.showGroups = true;
            gOpt.ShowTotal = ShowPooledES;
            gOpt.ShowSummaryLine = ShowSummaryLine;
            gOpt.Min = 0;
            gOpt.Max = 0;
            gOpt.ShowEffectLabels = ShowMetaLabels;
            gOpt.EffectLabel1 = LabelLHS;
            gOpt.EffectLabel2 = LabelRHS;

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
            string kind = "";
            switch (MetaAnalysis1.MetaAnalysisTypeId)
            {
                case 0: kind = "Hedges G"; break;
                case 1: kind = "Hedges G"; break;
                case 2: kind = "Odds Ratio"; break;
                case 3: kind = "Odds Ratio"; break;
                case 4: kind = "Hedges G"; break;
                case 5: kind = "Odds Ratio"; break;
                case 6: kind = "Odds Ratio"; break;
                case 7: kind = "Hedges G"; break;
            }

            // Fixed effect graph
            List<linesData> Al = new List<linesData>();
            List<linesData> Tl = new List<linesData>();
            linesData ToT = new linesData("Total", "", MetaAnalysisSubGroup.feEffect, MetaAnalysisSubGroup.feCiLower, MetaAnalysisSubGroup.feCiUpper, 0, 0);
            double sumWeight = MetaAnalysisSubGroup.feSumWeight; //MetaAnalysis1.feSumWeight + MetaAnalysis2.feSumWeight;
            IEnumerable<Outcome> query = new List<Outcome>();
            switch (SortBy)
            {
                case "Short Title (desc)":
                    query = MetaAnalysis1.Outcomes.OrderByDescending(Outc => Outc.ShortTitle);
                    break;
                case "Effect Size (desc)":
                    query = MetaAnalysis1.Outcomes.OrderByDescending(Outc => Outc.ES);
                    break;
                case "Effect Size (ascending)":
                    query = MetaAnalysis1.Outcomes.OrderBy(Outc => Outc.ES);
                    break;
                default:
                    query = MetaAnalysis1.Outcomes.OrderBy(Outc => Outc.ShortTitle);
                    break;
            }
            foreach (Outcome outcome in query)
            {
                if (outcome.IsSelected == true)
                {
                    Al.Add(OutcomeLinesData(outcome, MetaAnalysis1, MetaAnalysis1.feSumWeight, true));
                }

            }
            //foreach (Outcome outcome in MetaAnalysis1.Outcomes)
            //{
            //    if (outcome.IsSelected == true)
            //    {
            //        Al.Add(OutcomeLinesData(outcome, MetaAnalysis1, sumWeight, true));
            //    }
            //}
            IEnumerable<Outcome> query2 = new List<Outcome>();
            switch (SortBy)
            {
                case "Short Title (desc)":
                    query2 = MetaAnalysis2.Outcomes.OrderByDescending(Outc => Outc.ShortTitle);
                    break;
                case "Effect Size (desc)":
                    query2 = MetaAnalysis2.Outcomes.OrderByDescending(Outc => Outc.ES);
                    break;
                case "Effect Size (ascending)":
                    query2 = MetaAnalysis2.Outcomes.OrderBy(Outc => Outc.ES);
                    break;
                default:
                    query2 = MetaAnalysis2.Outcomes.OrderBy(Outc => Outc.ShortTitle);
                    break;
            }
            foreach (Outcome outcome in query2)
            {
                if (outcome.IsSelected == true)
                {
                    Al.Add(OutcomeLinesData(outcome, MetaAnalysis2, MetaAnalysis2.feSumWeight, true));
                }

            }
            //foreach (Outcome outcome in MetaAnalysis2.Outcomes)
            //{
            //    if (outcome.IsSelected == true)
            //    {
            //        Al.Add(OutcomeLinesData(outcome, MetaAnalysis2, sumWeight, true));
            //    }
            //}
            Tl.Add(new linesData(MetaAnalysis1.Title, MetaAnalysis1.feEffect, MetaAnalysis1.feCiLower, MetaAnalysis1.feCiUpper, 10, 10));
            Tl.Add(new linesData(MetaAnalysis2.Title, MetaAnalysis2.feEffect, MetaAnalysis2.feCiLower, MetaAnalysis2.feCiUpper, 10, 10));
            MetaGraphs.ForestPlot.GraphAllData GAD = new MetaGraphs.ForestPlot.GraphAllData(Al, Tl, ToT, "Fixed effect plot", kind);
            Image img = ForestPL.makeForestPl(GAD, gOpt);

            MemoryStream ms = new MemoryStream();
            img.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            feForestPlot = ms.ToArray();

            // Random effects graph
            Al.Clear();
            Tl.Clear();
            linesData ToT2 = new linesData("Total", "", MetaAnalysisSubGroup.reEffect, MetaAnalysisSubGroup.reCiLower, MetaAnalysisSubGroup.reCiUpper, 0, 0);
            sumWeight = MetaAnalysisSubGroup.reSumWeight;

            foreach (Outcome outcome in query)
            {
                if (outcome.IsSelected == true)
                {
                    Al.Add(OutcomeLinesData(outcome, MetaAnalysis1, sumWeight, false));
                }
            }
            foreach (Outcome outcome in query2)
            {
                if (outcome.IsSelected == true)
                {
                    Al.Add(OutcomeLinesData(outcome, MetaAnalysis2, sumWeight, false));
                }
            }
            Tl.Add(new linesData(MetaAnalysis1.Title, MetaAnalysis1.reEffect, MetaAnalysis1.reCiLower, MetaAnalysis1.reCiUpper, 10, 10));
            Tl.Add(new linesData(MetaAnalysis2.Title, MetaAnalysis2.reEffect, MetaAnalysis2.reCiLower, MetaAnalysis2.reCiUpper, 10, 10));
            MetaGraphs.ForestPlot.GraphAllData GAD2 = new MetaGraphs.ForestPlot.GraphAllData(Al, Tl, ToT2, "Random effects plot", kind);

            Image img2 = ForestPL.makeForestPl(GAD2, gOpt);

            MemoryStream ms2 = new MemoryStream();
            img2.Save(ms2, System.Drawing.Imaging.ImageFormat.Png);
            reForestPlot = ms2.ToArray();
        }

        protected override void DataPortal_DeleteSelf()
        {
            /*
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ItemDelete", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_SET_ID", ReadProperty(ItemIdProperty)));
                    command.Parameters.Add(new SqlParameter("@SET_ID", ReadProperty(SetIdProperty)));
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
             */
        }

        protected void DataPortal_Fetch() // used to return a specific item
        {
            this.Title = "Meta-analysis";
            //this.Outcomes = OutcomeList.GetOutcomes();
        }

#endif

        
    }
}
