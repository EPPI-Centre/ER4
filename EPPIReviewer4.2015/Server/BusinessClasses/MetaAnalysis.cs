using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Csla;
using Csla.Security;
using Csla.Core;
using Csla.Serialization;
using Csla.Silverlight;
using System.Text.RegularExpressions;
using BusinessLibrary.BusinessClasses;
//using Csla.Validation;

#if!SILVERLIGHT
using System.Data.SqlClient;
using BusinessLibrary.Data;
using Csla.Data;
using BusinessLibrary.Security;
#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class MetaAnalysis : BusinessBase<MetaAnalysis>
    {
        public static void GetMetaAnalysis(int Id, EventHandler<DataPortalResult<MetaAnalysis>> handler)
        {
            DataPortal<MetaAnalysis> dp = new DataPortal<MetaAnalysis>();
            dp.FetchCompleted += handler;
            dp.BeginFetch(new SingleCriteria<MetaAnalysis, int>(Id));
        }

#if SILVERLIGHT
        public MetaAnalysis()
        {
            
        }

#else
        public MetaAnalysis() { }
#endif

        /*
         * Sets which type of analysis we're dealing with
         * 0 = meta-analysis
         * 1 = network meta-analysis
         * 2 = QCA
         */

        private static PropertyInfo<int> AnalysisTypeProperty = RegisterProperty<int>(new PropertyInfo<int>("AnalysisType", "AnalysisType", 0));
        public int AnalysisType
        {
            get
            {
                return GetProperty(AnalysisTypeProperty);
            }
            set
            {
                SetProperty(AnalysisTypeProperty, value);
            }
        }


        /* ************* Properties to bind to the UI - to generate the R code ***************/

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

        private static PropertyInfo<bool> KNHAProperty = RegisterProperty<bool>(new PropertyInfo<bool>("KNHA", "KNHA", false));
        public bool KNHA
        {
            get
            {
                return GetProperty(KNHAProperty);
            }
            set
            {
                SetProperty(KNHAProperty, value);
            }
        }

        private static PropertyInfo<bool> FitStatsProperty = RegisterProperty<bool>(new PropertyInfo<bool>("FitStats", "FitStats", true));
        public bool FitStats
        {
            get
            {
                return GetProperty(FitStatsProperty);
            }
            set
            {
                SetProperty(FitStatsProperty, value);
            }
        }

        private static PropertyInfo<bool> ConfintProperty = RegisterProperty<bool>(new PropertyInfo<bool>("Confint", "Confint", false));
        public bool Confint
        {
            get
            {
                return GetProperty(ConfintProperty);
            }
            set
            {
                SetProperty(ConfintProperty, value);
            }
        }

        private static PropertyInfo<bool> EggerProperty = RegisterProperty<bool>(new PropertyInfo<bool>("Egger", "Egger", false));
        public bool Egger
        {
            get
            {
                return GetProperty(EggerProperty);
            }
            set
            {
                SetProperty(EggerProperty, value);
            }
        }

        private static PropertyInfo<bool> RankCorrProperty = RegisterProperty<bool>(new PropertyInfo<bool>("RankCorr", "RankCorr", false));
        public bool RankCorr
        {
            get
            {
                return GetProperty(RankCorrProperty);
            }
            set
            {
                SetProperty(RankCorrProperty, value);
            }
        }

        private static PropertyInfo<bool> TrimFillProperty = RegisterProperty<bool>(new PropertyInfo<bool>("TrimFill", "TrimFill", false));
        public bool TrimFill
        {
            get
            {
                return GetProperty(TrimFillProperty);
            }
            set
            {
                SetProperty(TrimFillProperty, value);
            }
        }

        /* statistical models are set in a combobox
         <ComboBoxItem Content="&quot;FE&quot;: fixed effect" Tag="FE"/>
            <ComboBoxItem Content="&quot;DL&quot;: DerSimonian-Laird estimator" Tag="DL"/>
            <ComboBoxItem Content="&quot;ML&quot;: Maximum-likelihood estimator" Tag="ML"/>
            <ComboBoxItem Content="&quot;REML&quot; Restricted maximum likelihood" Tag="REML"/>
            <ComboBoxItem Content="&quot;EB&quot;: Empirical Bayes estimator" Tag="EB"/>
        */
        private static PropertyInfo<int> StatisticalModelProperty = RegisterProperty<int>(new PropertyInfo<int>("StatisticalModel", "StatisticalModel"));
        public int StatisticalModel
        {
            get
            {
                return GetProperty(StatisticalModelProperty);
            }
            set
            {
                SetProperty(StatisticalModelProperty, value);
            }
        }

        public string GetStatisticalModelText()
        {
            switch(StatisticalModel)
            {
                case 0: return "FE";
                case 1: return "DL";
                case 2: return "ML";
                case 3: return "REML";
                case 4: return "EB";
                default: return "FE";
            }
        }

        private static PropertyInfo<int> VerboseProperty = RegisterProperty<int>(new PropertyInfo<int>("Verbose", "Verbose", 0));
        public int Verbose
        {
            get
            {
                return GetProperty(VerboseProperty);
            }
            set
            {
                SetProperty(VerboseProperty, value);
            }
        }

        private static PropertyInfo<int> SignificanceLevelProperty = RegisterProperty<int>(new PropertyInfo<int>("SignificanceLevel", "SignificanceLevel", 95));
        public int SignificanceLevel
        {
            get
            {
                return GetProperty(SignificanceLevelProperty);
            }
            set
            {
                SetProperty(SignificanceLevelProperty, value);
            }
        }

        private static PropertyInfo<int> DecPlacesProperty = RegisterProperty<int>(new PropertyInfo<int>("DecPlaces", "DecPlaces", 4));
        public int DecPlaces
        {
            get
            {
                return GetProperty(DecPlacesProperty);
            }
            set
            {
                SetProperty(DecPlacesProperty, value);
            }
        }

        private static PropertyInfo<string> XAxisTitleProperty = RegisterProperty<string>(new PropertyInfo<string>("XAxisTitle", "XAxisTitle", string.Empty));
        public string XAxisTitle
        {
            get
            {
                return GetProperty(XAxisTitleProperty);
            }
            set
            {
                SetProperty(XAxisTitleProperty, value);
            }
        }

        private static PropertyInfo<string> SummaryEstimateTitleProperty = RegisterProperty<string>(new PropertyInfo<string>("SummaryEstimateTitle", "SummaryEstimateTitle", string.Empty));
        public string SummaryEstimateTitle
        {
            get
            {
                return GetProperty(SummaryEstimateTitleProperty);
            }
            set
            {
                SetProperty(SummaryEstimateTitleProperty, value);
            }
        }

        private static PropertyInfo<bool> ShowAnnotationsProperty = RegisterProperty<bool>(new PropertyInfo<bool>("ShowAnnotations", "ShowAnnotations", true));
        public bool ShowAnnotations
        {
            get
            {
                return GetProperty(ShowAnnotationsProperty);
            }
            set
            {
                SetProperty(ShowAnnotationsProperty, value);
            }
        }

        private static PropertyInfo<bool> ShowAnnotationWeightsProperty = RegisterProperty<bool>(new PropertyInfo<bool>("ShowAnnotationWeights", "ShowAnnotationWeights", false));
        public bool ShowAnnotationWeights
        {
            get
            {
                return GetProperty(ShowAnnotationWeightsProperty);
            }
            set
            {
                SetProperty(ShowAnnotationWeightsProperty, value);
            }
        }

        private static PropertyInfo<bool> FittedValsProperty = RegisterProperty<bool>(new PropertyInfo<bool>("FittedVals", "FittedVals", true));
        public bool FittedVals
        {
            get
            {
                return GetProperty(FittedValsProperty);
            }
            set
            {
                SetProperty(FittedValsProperty, value);
            }
        }

        private static PropertyInfo<bool> CredIntProperty = RegisterProperty<bool>(new PropertyInfo<bool>("CredInt", "CredInt", false));
        public bool CredInt
        {
            get
            {
                return GetProperty(CredIntProperty);
            }
            set
            {
                SetProperty(CredIntProperty, value);
            }
        }

        private static PropertyInfo<bool> ShowFunnelProperty = RegisterProperty<bool>(new PropertyInfo<bool>("ShowFunnel", "ShowFunnel", false));
        public bool ShowFunnel
        {
            get
            {
                return GetProperty(ShowFunnelProperty);
            }
            set
            {
                SetProperty(ShowFunnelProperty, value);
            }
        }

        private static PropertyInfo<bool> ShowBoxplotProperty = RegisterProperty<bool>(new PropertyInfo<bool>("ShowBoxplot", "ShowBoxplot", false));
        public bool ShowBoxplot
        {
            get
            {
                return GetProperty(ShowBoxplotProperty);
            }
            set
            {
                SetProperty(ShowBoxplotProperty, value);
            }
        }

        public void SetupModeratorList()
        {
            MetaAnalysisModerators.Clear();
            MetaAnalysisModerator mam = new MetaAnalysisModerator();
            mam.Name = "Intervention";
            mam.IsSelected = false;
            mam.AttributeID = 0;
            mam.FieldName = "InterventionText";
            GetReferenceValues(mam, "InterventionText");
            MetaAnalysisModerators.Add(mam);
            mam = new MetaAnalysisModerator();
            mam.Name = "Comparison";
            mam.IsSelected = false;
            mam.AttributeID = 0;
            mam.FieldName = "ControlText";
            GetReferenceValues(mam, "ControlText");
            MetaAnalysisModerators.Add(mam);
            mam = new MetaAnalysisModerator();
            mam.Name = "Outcome";
            mam.IsSelected = false;
            mam.AttributeID = 0;
            mam.FieldName = "OutcomeText";
            GetReferenceValues(mam, "OutcomeText");
            MetaAnalysisModerators.Add(mam);
            int z = 1;
            if (Outcomes != null)
            {
                Dictionary<long, string> fieldNames = Outcomes.GetOutcomeClassificationFieldList();
                foreach (KeyValuePair<long, string> currField in fieldNames)
                {
                    mam = new MetaAnalysisModerator();
                    mam.Name = Regex.Replace(currField.Value, @"[^A-Za-z0-9]+", "");
                    mam.IsSelected = false;
                    mam.AttributeID = currField.Key;
                    mam.FieldName = "occ" + z.ToString();
                    mam.addReferenceValue("0", 0);
                    mam.addReferenceValue("1", 1);
                    MetaAnalysisModerators.Add(mam);
                    z++;
                }
            }
            string[] AttributeNames = AttributeAnswerText.Split('¬');
            for (int i = 0; i < 20; i++)
            {
                if (i < AttributeNames.Length && AttributeNames[i] != "")
                {
                    mam = new MetaAnalysisModerator();
                    mam.Name = Regex.Replace(AttributeNames[i], @"[^A-Za-z0-9]+", "");
                    mam.IsSelected = false;
                    mam.AttributeID = -1;
                    mam.FieldName = "aa" + (i + 1).ToString();
                    mam.addReferenceValue("0", 0);
                    mam.addReferenceValue("1", 1);
                    //GetReferenceValues(mam, "aa" + (i + 1).ToString());
                    MetaAnalysisModerators.Add(mam);
                }
            }
            string[] QuestionNames = AttributeQuestionText.Split('¬');
            for (int i = 0; i < 20; i++)
            {
                if (i < QuestionNames.Length && QuestionNames[i] != "")
                {
                    mam = new MetaAnalysisModerator();
                    mam.Name = Regex.Replace(QuestionNames[i], @"[^A-Za-z0-9]+", "");
                    mam.IsSelected = false;
                    mam.AttributeID = -1;
                    mam.FieldName = "aq" + (i + 1).ToString();
                    GetReferenceValues(mam, "aq" + (i + 1).ToString());
                    MetaAnalysisModerators.Add(mam);
                }
            }
        }

        private void GetReferenceValues(MetaAnalysisModerator mam, string propertyName)
        {
            int i = 0;
            foreach (Outcome o in Outcomes)
            {
                string s = o.GetType().GetProperty(propertyName).GetValue(o, null).ToString();
                if (!mam.hasRefValue(s))
                {
                    mam.addReferenceValue(s, i);// we don't use the attribute id at the moment but giving it an index seems a good idea
                    i++;
                }
            }
        }

        public bool CheckValidModerators()
        {
            bool retVal = true;
            foreach (MetaAnalysisModerator mam in this.MetaAnalysisModerators)
            {
                if (mam.IsSelected == true)
                {
                    // check for empty values in the outcomes list for a given moderator
                    if (mam.FieldName.StartsWith("aq") == true || mam.FieldName == "InterventionText" || mam.FieldName == "ControlText" || mam.FieldName == "OutcomeText")
                    {
                        foreach (Outcome o in this.Outcomes)
                        {
                            if (o.IsSelected == true && o.GetType().GetProperty(mam.FieldName).GetValue(o, null).ToString() == "")
                            {
                                return false;
                            }
                        }
                    }
                    // check for filtered out reference values and that we have at least two factors on which to compare
                    retVal = false;
                    bool haveAnother = false;
                    foreach (Outcome o in this.Outcomes)
                    {
                        if (o.IsSelected == true && o.GetType().GetProperty(mam.FieldName).GetValue(o, null).ToString() == mam.Reference)
                        {
                            retVal = true;
                        }
                        if (o.IsSelected == true && o.GetType().GetProperty(mam.FieldName).GetValue(o, null).ToString() != mam.Reference)
                        {
                            haveAnother = true;
                        }
                    }
                    if (retVal == false || haveAnother == false)
                    {
                        return false;
                    }
                }
            }
            return retVal;
        }

        private static PropertyInfo<string> SortedByProperty = RegisterProperty<string>(new PropertyInfo<string>("SortedBy", "SortedBy", string.Empty));
        public string SortedBy
        {
            get
            {
                return GetProperty(SortedByProperty);
            }
            set
            {
                SetProperty(SortedByProperty, value);
            }
        }

        private static PropertyInfo<string> SortDirectionProperty = RegisterProperty<string>(new PropertyInfo<string>("SortDirection", "SortDirection", string.Empty));
        public string SortDirection
        {
            get
            {
                return GetProperty(SortDirectionProperty);
            }
            set
            {
                SetProperty(SortDirectionProperty, value);
            }
        }


        /* ************* NETWORK META-ANALYSIS PROPERTIES ********************/

        private static PropertyInfo<int> NMAStatisticalModelProperty = RegisterProperty<int>(new PropertyInfo<int>("NMAStatisticalModel", "NMAStatisticalModel"));
        public int NMAStatisticalModel
        {
            get
            {
                return GetProperty(NMAStatisticalModelProperty);
            }
            set
            {
                SetProperty(NMAStatisticalModelProperty, value);
            }
        }

        private static PropertyInfo<bool> LargeValuesGoodProperty = RegisterProperty<bool>(new PropertyInfo<bool>("LargeValuesGood", "LargeValuesGood", false));
        public bool LargeValuesGood
        {
            get
            {
                return GetProperty(LargeValuesGoodProperty);
            }
            set
            {
                SetProperty(LargeValuesGoodProperty, value);
            }
        }

        private static PropertyInfo<string> NMAReferenceProperty = RegisterProperty<string>(new PropertyInfo<string>("NMAReference", "NMAReference", string.Empty));
        public string NMAReference
        {
            get
            {
                return GetProperty(NMAReferenceProperty);
            }
            set
            {
                SetProperty(NMAReferenceProperty, value);
            }
        }


        /* ************* Database properties *****************/

        private static PropertyInfo<int> MetaAnalysisIdProperty = RegisterProperty<int>(new PropertyInfo<int>("MetaAnalysisId", "MetaAnalysisId", 0));
        public int MetaAnalysisId
        {
            get
            {
                return GetProperty(MetaAnalysisIdProperty);
            }
        }

        

        private static PropertyInfo<Int64> AttributeIdProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("AttributeId", "AttributeId"));
        public Int64 AttributeId
        {
            get
            {
                return GetProperty(AttributeIdProperty);
            }
            set
            {
                SetProperty(AttributeIdProperty, value);
            }
        }

        private static PropertyInfo<int> SetIdProperty = RegisterProperty<int>(new PropertyInfo<int>("SetId", "SetId"));
        public int SetId
        {
            get
            {
                return GetProperty(SetIdProperty);
            }
            set
            {
                SetProperty(SetIdProperty, value);
            }
        }

        private static PropertyInfo<Int64> AttributeIdInterventionProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("AttributeIdIntervention", "AttributeIdIntervention"));
        public Int64 AttributeIdIntervention
        {
            get
            {
                return GetProperty(AttributeIdInterventionProperty);
            }
            set
            {
                SetProperty(AttributeIdInterventionProperty, value);
            }
        }

        private static PropertyInfo<Int64> AttributeIdControlProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("AttributeIdControl", "AttributeIdControl"));
        public Int64 AttributeIdControl
        {
            get
            {
                return GetProperty(AttributeIdControlProperty);
            }
            set
            {
                SetProperty(AttributeIdControlProperty, value);
            }
        }

        private static PropertyInfo<Int64> AttributeIdOutcomeProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("AttributeIdOutcome", "AttributeIdOutcome"));
        public Int64 AttributeIdOutcome
        {
            get
            {
                return GetProperty(AttributeIdOutcomeProperty);
            }
            set
            {
                SetProperty(AttributeIdOutcomeProperty, value);
            }
        }

        /* Meta-analysis types:
         *     1: Continuous: d (Hedges g)
         *     2: Continuous: r
         *     3: Binary: odds ratio
         *     4: Binary: risk ratio
         *     5: Binary: risk difference
         *     6: Binary: diagnostic test OR
         *     7: Binary: Peto OR
         *     8: Continuous: mean difference
         */

        public string GetMetaAnalysisTypeText()
        {
            switch (MetaAnalysisTypeId + 1)
            {
                case 1: return "SMD";
                case 2: return "COR"; // not sure this is correct? needs Z transformation prior to meta-analysis - but maybe metafor does this - check
                case 3: return "OR";
                case 4: return "RR";
                case 5: return "RD";
                case 6: return "OR";
                case 7: return "OR";
                case 8: return "MD";
                default: return "";
            }
        }

        private static PropertyInfo<int> MetaAnalysisTypeIdProperty = RegisterProperty<int>(new PropertyInfo<int>("MetaAnalysisTypeId", "MetaAnalysisTypeId"));
        public int MetaAnalysisTypeId
        {
            get
            {
                return GetProperty(MetaAnalysisTypeIdProperty);
            }
            set
            {
                SetProperty(MetaAnalysisTypeIdProperty, value);
            }
        }

        private static PropertyInfo<string> MetaAnalysisTypeTitleProperty = RegisterProperty<string>(new PropertyInfo<string>("MetaAnalysisTypeTitle", "MetaAnalysisTypeTitle", string.Empty));
        public string MetaAnalysisTypeTitle
        {
            get
            {
                return GetProperty(MetaAnalysisTypeTitleProperty);
            }
        }

        private static PropertyInfo<string> InterventionTextProperty = RegisterProperty<string>(new PropertyInfo<string>("InterventionText", "InterventionText", string.Empty));
        public string InterventionText
        {
            get
            {
                return GetProperty(InterventionTextProperty);
            }
        }

        private static PropertyInfo<string> ControlTextProperty = RegisterProperty<string>(new PropertyInfo<string>("ControlText", "ControlText", string.Empty));
        public string ControlText
        {
            get
            {
                return GetProperty(ControlTextProperty);
            }
        }

        private static PropertyInfo<string> OutcomeTextProperty = RegisterProperty<string>(new PropertyInfo<string>("OutcomeText", "OutcomeText", string.Empty));
        public string OutcomeText
        {
            get
            {
                return GetProperty(OutcomeTextProperty);
            }
        }

        private static PropertyInfo<OutcomeList> OutcomesProperty = RegisterProperty<OutcomeList>(new PropertyInfo<OutcomeList>("Outcomes", "Outcomes"));
        public OutcomeList Outcomes
        {
            get
            {
                return GetProperty(OutcomesProperty);
            }
            set
            {
                SetProperty(OutcomesProperty, value);
            }
        }

        private static PropertyInfo<MetaAnalysisModeratorList> MetaAnalysisModeratorsProperty = RegisterProperty<MetaAnalysisModeratorList>(new PropertyInfo<MetaAnalysisModeratorList>("MetaAnalysisModerators", "MetaAnalysisModerators"));
        public MetaAnalysisModeratorList MetaAnalysisModerators
        {
            get
            {
                return GetProperty(MetaAnalysisModeratorsProperty);
            }
            set
            {
                SetProperty(MetaAnalysisModeratorsProperty, value);
            }
        }

        private static PropertyInfo<string> AttributeIdQuestionProperty = RegisterProperty<string>(new PropertyInfo<string>("AttributeIdQuestion", "AttributeIdQuestion", string.Empty));
        public string AttributeIdQuestion
        {
            get
            {
                return GetProperty(AttributeIdQuestionProperty);
            }
            set
            {
                SetProperty(AttributeIdQuestionProperty, value);
            }
        }

        private static PropertyInfo<string> AttributeQuestionTextProperty = RegisterProperty<string>(new PropertyInfo<string>("AttributeQuestionText", "AttributeQuestionText", string.Empty));
        public string AttributeQuestionText
        {
            get
            {
                return GetProperty(AttributeQuestionTextProperty);
            }
            set
            {
                SetProperty(AttributeQuestionTextProperty, value);
            }
        }

        private static PropertyInfo<string> AttributeIdAnswerProperty = RegisterProperty<string>(new PropertyInfo<string>("AttributeIdAnswer", "AttributeIdAnswer", string.Empty));
        public string AttributeIdAnswer
        {
            get
            {
                return GetProperty(AttributeIdAnswerProperty);
            }
            set
            {
                SetProperty(AttributeIdAnswerProperty, value);
            }
        }

        private static PropertyInfo<string> AttributeAnswerTextProperty = RegisterProperty<string>(new PropertyInfo<string>("AttributeAnswerText", "AttributeAnswerText", string.Empty));
        public string AttributeAnswerText
        {
            get
            {
                return GetProperty(AttributeAnswerTextProperty);
            }
            set
            {
                SetProperty(AttributeAnswerTextProperty, value);
            }
        }

        private static PropertyInfo<string> GridSettingsProperty = RegisterProperty<string>(new PropertyInfo<string>("GridSettings", "GridSettings", string.Empty));
        public string GridSettings
        {
            get
            {
                return GetProperty(GridSettingsProperty);
            }
            set
            {
                SetProperty(GridSettingsProperty, value);
            }
        }


        /* ************* Calculated properties *****************/

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

        private static PropertyInfo<double> feSumWeightProperty = RegisterProperty<double>(new PropertyInfo<double>("feSumWeight", "feSumWeight"));
        public double feSumWeight // just used when sending objects to the forest plotter.
        {
            get
            {
                return GetProperty(feSumWeightProperty);
            }
            set
            {
                SetProperty(feSumWeightProperty, value);
            }
        }

        private static PropertyInfo<double> reSumWeightProperty = RegisterProperty<double>(new PropertyInfo<double>("reSumWeight", "reSumWeight"));
        public double reSumWeight // just used when sending objects to the forest plotter.
        {
            get
            {
                return GetProperty(reSumWeightProperty);
            }
            set
            {
                SetProperty(reSumWeightProperty, value);
            }
        }

        private static PropertyInfo<double> _feEffect = RegisterProperty<double>(new PropertyInfo<double>("feEffect", "feEffect"));
        public double feEffect
        {
            get { return ReadProperty(_feEffect); }
            set { SetProperty(_feEffect, value); }
        }

        private static PropertyInfo<double> _feSE = RegisterProperty<double>(new PropertyInfo<double>("feSE", "feSE"));
        public double feSE
        {
            get { return ReadProperty(_feSE); }
            set { SetProperty(_feSE, value); }
        }

        private static PropertyInfo<double> _feCiUpper = RegisterProperty<double>(new PropertyInfo<double>("_feCiUpper", "_feCiUpper"));
        public double feCiUpper
        {
            get { return ReadProperty(_feCiUpper); }
            set { SetProperty(_feCiUpper, value); }
        }

        private static PropertyInfo<double> _feCiLower = RegisterProperty<double>(new PropertyInfo<double>("_feCiLower", "_feCiLower"));
        public double feCiLower
        {
            get { return ReadProperty(_feCiLower); }
            set { SetProperty(_feCiLower, value); }
        }

        private static PropertyInfo<double> _reEffect = RegisterProperty<double>(new PropertyInfo<double>("reEffect", "reEffect"));
        public double reEffect
        {
            get { return ReadProperty(_reEffect); }
            set { SetProperty(_reEffect, value); }
        }

        private static PropertyInfo<double> _reSE = RegisterProperty<double>(new PropertyInfo<double>("reSE", "reSE"));
        public double reSE
        {
            get { return ReadProperty(_reSE); }
            set { SetProperty(_reSE, value); }
        }

        private static PropertyInfo<double> _reCiUpper = RegisterProperty<double>(new PropertyInfo<double>("_reCiUpper", "_reCiUpper"));
        public double reCiUpper
        {
            get { return ReadProperty(_reCiUpper); }
            set { SetProperty(_reCiUpper, value); }
        }

        private static PropertyInfo<double> _reCiLower = RegisterProperty<double>(new PropertyInfo<double>("_reCiLower", "_reCiLower"));
        public double reCiLower
        {
            get { return ReadProperty(_reCiLower); }
            set { SetProperty(_reCiLower, value); }
        }

        private static PropertyInfo<double> _tauSquared = RegisterProperty<double>(new PropertyInfo<double>("_tauSquared", "_tauSquared"));
        public double tauSquared
        {
            get { return ReadProperty(_tauSquared); }
            set { SetProperty(_tauSquared, value); }
        }

        private static PropertyInfo<double> _Q = RegisterProperty<double>(new PropertyInfo<double>("_Q", "_Q"));
        public double Q
        {
            get { return ReadProperty(_Q); }
            set { SetProperty(_Q, value); }
        }

        private static PropertyInfo<double> _reQ = RegisterProperty<double>(new PropertyInfo<double>("_reQ", "_reQ"));
        public double reQ
        {
            get { return ReadProperty(_reQ); }
            set { SetProperty(_reQ, value); }
        }

        private static PropertyInfo<double> _numStudies = RegisterProperty<double>(new PropertyInfo<double>("_numStudies", "_numStudies"));
        public double numStudies
        {
            get { return ReadProperty(_numStudies); }
            set { SetProperty(_numStudies, value); }
        }

        private static PropertyInfo<double> _FileDrawerZ = RegisterProperty<double>(new PropertyInfo<double>("_FileDrawerZ", "_FileDrawerZ"));
        public double FileDrawerZ
        {
            get { return ReadProperty(_FileDrawerZ); }
            set { SetProperty(_FileDrawerZ, value); }
        }

        private static PropertyInfo<double> _sumWeightsSquared = RegisterProperty<double>(new PropertyInfo<double>("_sumWeightsSquared", "_sumWeightsSquared"));
        public double sumWeightsSquared
        {
            get { return ReadProperty(_sumWeightsSquared); }
            set { SetProperty(_sumWeightsSquared, value); }
        }

        private static PropertyInfo<double> _reSumWeightsTimesOutcome = RegisterProperty<double>(new PropertyInfo<double>("_reSumWeightsTimesOutcome", "_reSumWeightsTimesOutcome"));
        public double reSumWeightsTimesOutcome
        {
            get { return ReadProperty(_reSumWeightsTimesOutcome); }
            set { SetProperty(_reSumWeightsTimesOutcome, value); }
        }

        private static PropertyInfo<double> _WY_squared = RegisterProperty<double>(new PropertyInfo<double>("_WY_squared", "_WY_squared"));
        public double WY_squared
        {
            get { return ReadProperty(_WY_squared); }
            set { SetProperty(_WY_squared, value); }
        }

        public double C()
        {
            return feSumWeight - (sumWeightsSquared / feSumWeight);
        }


#if SILVERLIGHT
    
#else
        protected override void DataPortal_Insert()
        {
            insert_object();
        }

        protected override void DataPortal_Update()
        {
            if (MetaAnalysisId == 0)
            {
                insert_object();
            }
            else
            {
                update_object();
            }
        }

        protected void update_object()
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_MetaAnalysisUpdate", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@META_ANALYSIS_ID", ReadProperty(MetaAnalysisIdProperty)));
                    command.Parameters.Add(new SqlParameter("@TITLE", ReadProperty(TitleProperty)));
                    command.Parameters.Add(new SqlParameter("@CONTACT_ID", ri.UserId));
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ID", ReadProperty(AttributeIdProperty)));
                    command.Parameters.Add(new SqlParameter("@SET_ID", ReadProperty(SetIdProperty)));
                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ID_INTERVENTION", ReadProperty(AttributeIdInterventionProperty)));
                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ID_CONTROL", ReadProperty(AttributeIdControlProperty)));
                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ID_OUTCOME", ReadProperty(AttributeIdOutcomeProperty)));
                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ID_ANSWER", ReadProperty(AttributeIdAnswerProperty)));
                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ID_QUESTION", ReadProperty(AttributeIdQuestionProperty)));
                    command.Parameters.Add(new SqlParameter("@META_ANALYSIS_TYPE_ID", ReadProperty(MetaAnalysisTypeIdProperty)));
                    command.Parameters.Add(new SqlParameter("@GRID_SETTINGS", ReadProperty(GridSettingsProperty)));
                    command.Parameters.Add(new SqlParameter("@OUTCOME_IDS", OutcomeIds()));
                    SqlParameter par = new SqlParameter("@ATTRIBUTE_ANSWER_TEXT", System.Data.SqlDbType.NVarChar);
                    par.Size = 4000;
                    command.Parameters.Add(par);
                    command.Parameters["@ATTRIBUTE_ANSWER_TEXT"].Direction = System.Data.ParameterDirection.Output;
                    SqlParameter par3 = new SqlParameter("@ATTRIBUTE_QUESTION_TEXT", System.Data.SqlDbType.NVarChar);
                    par3.Size = 4000;
                    command.Parameters.Add(par3);
                    command.Parameters["@ATTRIBUTE_QUESTION_TEXT"].Direction = System.Data.ParameterDirection.Output;
                    command.ExecuteNonQuery();
                    if (AttributeIdAnswer != "")
                        LoadProperty(AttributeAnswerTextProperty, command.Parameters["@ATTRIBUTE_ANSWER_TEXT"].Value);
                    else
                        LoadProperty(AttributeAnswerTextProperty, "");
                    if (AttributeIdQuestion != "")
                        LoadProperty(AttributeQuestionTextProperty, command.Parameters["@ATTRIBUTE_QUESTION_TEXT"].Value);
                    else
                        LoadProperty(AttributeQuestionTextProperty, "");
                }
                connection.Close();
                Outcomes = OutcomeList.GetOutcomeList(SetId, AttributeIdIntervention, AttributeIdControl,
                        AttributeIdOutcome, AttributeId, MetaAnalysisId, AttributeIdQuestion, AttributeIdAnswer);
                MetaAnalysisModerators = MetaAnalysisModeratorList.GetMetaAnalysisModeratorList();
            }
        }

        protected void insert_object()
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_MetaAnalysisInsert", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;

                    command.Parameters.Add(new SqlParameter("@TITLE", ReadProperty(TitleProperty)));
                    command.Parameters.Add(new SqlParameter("@CONTACT_ID", ri.UserId));
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ID", ReadProperty(AttributeIdProperty)));
                    command.Parameters.Add(new SqlParameter("@SET_ID", ReadProperty(SetIdProperty)));
                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ID_INTERVENTION", ReadProperty(AttributeIdInterventionProperty)));
                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ID_CONTROL", ReadProperty(AttributeIdControlProperty)));
                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ID_OUTCOME", ReadProperty(AttributeIdOutcomeProperty)));
                    command.Parameters.Add(new SqlParameter("@META_ANALYSIS_TYPE_ID", ReadProperty(MetaAnalysisTypeIdProperty) >= 0 ? ReadProperty(MetaAnalysisTypeIdProperty) : 0));
                    command.Parameters.Add(new SqlParameter("@OUTCOME_IDS", OutcomeIds()));
                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ID_ANSWER", ReadProperty(AttributeIdAnswerProperty)));
                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ID_QUESTION", ReadProperty(AttributeIdQuestionProperty)));
                    command.Parameters.Add(new SqlParameter("@GRID_SETTINGS", ReadProperty(GridSettingsProperty)));
                    SqlParameter par = new SqlParameter("@NEW_META_ANALYSIS_ID", System.Data.SqlDbType.Int);
                    par.Value = 0;
                    command.Parameters.Add(par);
                    command.Parameters["@NEW_META_ANALYSIS_ID"].Direction = System.Data.ParameterDirection.Output;
                    SqlParameter par2 = new SqlParameter("@ATTRIBUTE_ANSWER_TEXT", System.Data.SqlDbType.NVarChar);
                    par2.Size = 4000;
                    command.Parameters.Add(par2);
                    command.Parameters["@ATTRIBUTE_ANSWER_TEXT"].Direction = System.Data.ParameterDirection.Output;
                    SqlParameter par3 = new SqlParameter("@ATTRIBUTE_QUESTION_TEXT", System.Data.SqlDbType.NVarChar);
                    par3.Size = 4000;
                    command.Parameters.Add(par3);
                    command.Parameters["@ATTRIBUTE_QUESTION_TEXT"].Direction = System.Data.ParameterDirection.Output;
                    command.ExecuteNonQuery();
                    LoadProperty(MetaAnalysisIdProperty, command.Parameters["@NEW_META_ANALYSIS_ID"].Value);
                    if (AttributeIdAnswer != "")
                        LoadProperty(AttributeAnswerTextProperty, command.Parameters["@ATTRIBUTE_ANSWER_TEXT"].Value);
                    else
                        LoadProperty(AttributeAnswerTextProperty, "");
                    if (AttributeIdQuestion != "")
                        LoadProperty(AttributeQuestionTextProperty, command.Parameters["@ATTRIBUTE_QUESTION_TEXT"].Value);
                    else
                        LoadProperty(AttributeQuestionTextProperty, "");

                    Outcomes = OutcomeList.GetOutcomeList(SetId, AttributeIdIntervention, AttributeIdControl,
                        AttributeIdOutcome, AttributeId, MetaAnalysisId, AttributeIdQuestion, AttributeIdAnswer);
                    MetaAnalysisModerators = MetaAnalysisModeratorList.GetMetaAnalysisModeratorList();
                }
                connection.Close();
            }
        }

        protected string OutcomeIds()
        {
            string retVal = "";
            if (this.Outcomes != null)
            {
                foreach (Outcome o in this.Outcomes)
                {
                    if (o.IsSelected == true)
                    {
                        if (retVal == "")
                            retVal = o.OutcomeId.ToString();
                        else
                            retVal += "," + o.OutcomeId.ToString();
                    }
                }
            }
            return retVal;
        }

        protected void DataPortal_Fetch(SingleCriteria<MetaAnalysis, int> criteria)
        {
            
        }

        protected override void DataPortal_DeleteSelf()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_MetaAnalysisDelete", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@META_ANALYSIS_ID", ReadProperty(MetaAnalysisIdProperty)));
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        /* SHOULD BE POSSIBLE TO REMOVE PROPERTIES RELATING TO INTERVENTION / OUTCOME / CONTROL ONCE WE ARE USING THE NEW MA INTERFACE */
        internal static MetaAnalysis GetMetaAnalysis(SafeDataReader reader)
        {
            MetaAnalysis returnValue = new MetaAnalysis();
            returnValue.LoadProperty<int>(MetaAnalysisIdProperty, reader.GetInt32("META_ANALYSIS_ID"));
            returnValue.LoadProperty<string>(TitleProperty, reader.GetString("META_ANALYSIS_TITLE"));
            returnValue.LoadProperty<Int64>(AttributeIdProperty, reader.GetInt64("ATTRIBUTE_ID"));
            returnValue.LoadProperty<int>(SetIdProperty, reader.GetInt32("SET_ID"));
            returnValue.LoadProperty<Int64>(AttributeIdInterventionProperty, reader.GetInt64("ATTRIBUTE_ID_INTERVENTION")); /* <--- ie remove these */
            returnValue.LoadProperty<Int64>(AttributeIdControlProperty, reader.GetInt64("ATTRIBUTE_ID_CONTROL"));
            returnValue.LoadProperty<Int64>(AttributeIdOutcomeProperty, reader.GetInt64("ATTRIBUTE_ID_OUTCOME"));
            returnValue.LoadProperty<int>(MetaAnalysisTypeIdProperty, reader.GetInt32("META_ANALYSIS_TYPE_ID"));
            returnValue.LoadProperty<string>(MetaAnalysisTypeTitleProperty, reader.GetString("META_ANALYSIS_TYPE_TITLE"));
            //returnValue.LoadProperty<string>(InterventionTextProperty, reader.GetString("INTERVENTION_TEXT"));
            //returnValue.LoadProperty<string>(ControlTextProperty, reader.GetString("CONTROL_TEXT"));
            //returnValue.LoadProperty<string>(OutcomeTextProperty, reader.GetString("OUTCOME_TEXT"));
            returnValue.LoadProperty<string>(AttributeIdAnswerProperty, reader.GetString("ATTRIBUTE_ID_ANSWER"));
            returnValue.LoadProperty<string>(AttributeIdQuestionProperty, reader.GetString("ATTRIBUTE_ID_QUESTION"));
            returnValue.LoadProperty<string>(GridSettingsProperty, reader.GetString("GRID_SETTINGS"));
            if (returnValue.AttributeIdAnswer != "")
                returnValue.LoadProperty<string>(AttributeAnswerTextProperty, reader.GetString("ATTRIBUTE_ANSWER_TEXT"));
            else
                returnValue.LoadProperty<string>(AttributeAnswerTextProperty, "");
            if (returnValue.AttributeIdQuestion != "")
                returnValue.LoadProperty<string>(AttributeQuestionTextProperty, reader.GetString("ATTRIBUTE_QUESTION_TEXT"));
            else
                returnValue.LoadProperty<string>(AttributeQuestionTextProperty, "");
            returnValue.MarkOld();

            //loading on the fly now, as they take too long if there's a lot of outcomes / meta-analyses
            //returnValue.Outcomes = OutcomeList.GetOutcomeList(returnValue.SetId, returnValue.AttributeIdIntervention, returnValue.AttributeIdControl,
            //    returnValue.AttributeIdOutcome, returnValue.AttributeId, returnValue.MetaAnalysisId, returnValue.AttributeIdQuestion, returnValue.AttributeIdAnswer);

            returnValue.MetaAnalysisModerators = MetaAnalysisModeratorList.GetMetaAnalysisModeratorList();

            return returnValue;
        }


#endif



        public void run(double setTauSquared)
        {
            CalculateFixedEffect();
            CalculateQ();
            if (setTauSquared == -1)
            {
                CalculateTauSquared();
            }
            else
            {
                tauSquared = setTauSquared;
            }
            CalculateRandomEffects();
            CalculateFileDrawerZ();
            if (EffectMeasureType() == "binary")
            {
                feCiUpper = Math.Exp(feEffect + (1.96 * feSE));
                feCiLower = Math.Exp(feEffect - (1.96 * feSE));
                reCiUpper = Math.Exp(reEffect + (1.96 * reSE));
                reCiLower = Math.Exp(reEffect - (1.96 * reSE));
                feEffect = Math.Exp(feEffect);
                feSE = Math.Exp(feSE);
                reEffect = Math.Exp(reEffect);
                reSE = Math.Exp(reSE);
            }
            else if (EffectMeasureType() == "standard")
            {
                feCiUpper = feEffect + 1.96 * feSE;
                feCiLower = feEffect - 1.96 * feSE;
                reCiUpper = reEffect + 1.96 * reSE;
                reCiLower = reEffect - 1.96 * reSE;
            }
            else // correlation
            {
                feCiUpper = ZToCorrelation(feEffect + (1.96 * feSE));
                feCiLower = ZToCorrelation(feEffect - (1.96 * feSE));
                reCiUpper = ZToCorrelation(reEffect + (1.96 * reSE));
                reCiLower = ZToCorrelation(reEffect - (1.96 * reSE));
                feEffect = ZToCorrelation(feEffect);
                feSE = ZToCorrelation(feSE);
                reEffect = ZToCorrelation(reEffect);
                reSE = ZToCorrelation(reSE);
            }

        }

        double ZToCorrelation(double z)
        {
            return (Math.Exp(z * 2) - 1) / (Math.Exp(z * 2) + 1);
        }

        public void CalculateFixedEffect()
        {
            double top = 0;
            feSumWeight = 0;
            sumWeightsSquared = 0;
            numStudies = 0;
            foreach (Outcome outcome in Outcomes)
            {
                if (outcome.IsSelected == true)
                {
                    if (outcome.GetStandardErrorCombining(MetaAnalysisTypeId) != 0)
                    {
                        double weight = 1.0d / Math.Pow(outcome.GetStandardErrorCombining(MetaAnalysisTypeId), 2);
                        outcome.feWeight = weight;
                        top += weight * outcome.GetEffectSizeCombining(MetaAnalysisTypeId);
                        feSumWeight += weight;
                        sumWeightsSquared += weight * weight;
                    }
                    numStudies++;
                }
            }
            if (feSumWeight != 0)
            {
                feEffect = top / feSumWeight;
                feSE = 1.0d / Math.Sqrt(feSumWeight);
            }
            else
            {
                feEffect = 0;
                feSE = 0;
            }
        }

        // Q AND file drawer Z for SMDs
        public void CalculateQ()
        {
            Q = 0;
            foreach (Outcome outcome in Outcomes)
            {
                if (outcome.IsSelected == true)
                {
                    if (outcome.GetStandardErrorCombining(MetaAnalysisTypeId) != 0)
                    {
                        double weight = 1.0d / Math.Pow(outcome.GetStandardErrorCombining(MetaAnalysisTypeId), 2);
                        Q += weight * Math.Pow((outcome.GetEffectSizeCombining(MetaAnalysisTypeId) - feEffect), 2);
                    }
                }
            }
        }

        public void CalculateTauSquared()
        {
            tauSquared = 0;
            if (Q >= numStudies - 1)
            {
                if (feSumWeight != 0)
                {
                    tauSquared = (Q - (Convert.ToDouble(numStudies) - 1.0)) / (feSumWeight - (sumWeightsSquared / feSumWeight));
                }
            }
        }

        public void CalculateRandomEffects()
        {
            double top = 0;
            double bottom = 0;
            WY_squared = 0;
            foreach (Outcome outcome in Outcomes)
            {
                if (outcome.IsSelected == true)
                {
                    if (outcome.GetStandardErrorCombining(MetaAnalysisTypeId) != 0)
                    {
                        double weight = 1.0 / ((outcome.GetStandardErrorCombining(MetaAnalysisTypeId) * outcome.GetStandardErrorCombining(MetaAnalysisTypeId)) + tauSquared);
                        outcome.reWeight = weight;
                        top += weight * outcome.GetEffectSizeCombining(MetaAnalysisTypeId);
                        bottom += weight;
                        WY_squared += (outcome.GetEffectSizeCombining(MetaAnalysisTypeId) * outcome.GetEffectSizeCombining(MetaAnalysisTypeId)) * weight;
                    }
                }
            }
            reSumWeight = bottom; // for forest plot weight calculation
            reSumWeightsTimesOutcome = top;
            if (bottom != 0)
            {
                reEffect = top / bottom;
                reSE = 1.0 / Math.Sqrt(bottom);
                reQ = WY_squared - ((reSumWeightsTimesOutcome * reSumWeightsTimesOutcome) / reSumWeight);
            }
            else
            {
                reEffect = 0;
                reSE = 0;
            }
        }

        public void CalculateFileDrawerZ()
        {
            FileDrawerZ = (numStudies / 2.706d) * ((numStudies * feEffect * feEffect) - 2.706d);
        }

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
        private string EffectMeasureType()
        {
            if (MetaAnalysisTypeId == 1)
                return "correlation";
            if (MetaAnalysisTypeId == 0 ||  MetaAnalysisTypeId == 4 || MetaAnalysisTypeId == 7)
                return "standard";
            return "binary";
        }

     }

   

}