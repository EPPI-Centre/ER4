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

//#if SILVERLIGHT
//        public MetaAnalysis() {
//            FilterSettingsList = new MetaAnalysisFilterSettingList();
//            Random rnd = new Random();
//            _instance = rnd.Next(1, 101);
//        }
//        private int _instance;
//        public int Instance { get { return _instance; } }
//#else 
        public MetaAnalysis()
        {
            //Outcomes = new OutcomeList();
            FilterSettingsList = new MetaAnalysisFilterSettingList();
        }

        public void SetOutcomesList(OutcomeList outcomes)
        {
            bool wasDirty = this.IsDirty;
            this.Outcomes = outcomes;
            if (wasDirty == false && this.IsDirty == true)
            {
                this.MarkClean();
            }
        }
        /// <summary>
        /// Called when we modify a child in a list (FilterSettingsList) and want the MA to realise it has changes to save
        /// For some reason with the code as is, it doesn't on its onw :-(
        /// </summary>
        public void DoMarkDirty()
        {
            this.MarkDirty();
        }
//#endif
        /*
         * Sets which type of analysis we're dealing with
         * 0 = meta-analysis
         * 1 = network meta-analysis
         * 2 = QCA
         */

        private static PropertyInfo<int> AnalysisTypeProperty = RegisterProperty<int>(new PropertyInfo<int>("AnalysisType", "AnalysisType", 0));
        /// <summary>
        /// Not Saved in the db
        /// </summary>
        public int AnalysisType
        {
            get
            {
                return GetProperty(AnalysisTypeProperty);
            }
            set
            {
                LoadProperty(AnalysisTypeProperty, value);
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
        /// <summary>
        /// Not Saved in the db
        /// </summary>
        private static PropertyInfo<bool> KNHAProperty = RegisterProperty<bool>(new PropertyInfo<bool>("KNHA", "KNHA", false));
        public bool KNHA
        {
            get
            {
                return GetProperty(KNHAProperty);
            }
            set
            {
                LoadProperty(KNHAProperty, value);
            }
        }

        private static PropertyInfo<bool> FitStatsProperty = RegisterProperty<bool>(new PropertyInfo<bool>("FitStats", "FitStats", true));
        /// <summary>
        /// Not Saved in the db
        /// </summary>
        public bool FitStats
        {
            get
            {
                return GetProperty(FitStatsProperty);
            }
            set
            {
                LoadProperty(FitStatsProperty, value);
            }
        }

        private static PropertyInfo<bool> ConfintProperty = RegisterProperty<bool>(new PropertyInfo<bool>("Confint", "Confint", false));
        /// <summary>
        /// Not Saved in the db
        /// </summary>
        public bool Confint
        {
            get
            {
                return GetProperty(ConfintProperty);
            }
            set
            {
                LoadProperty(ConfintProperty, value);
            }
        }

        private static PropertyInfo<bool> EggerProperty = RegisterProperty<bool>(new PropertyInfo<bool>("Egger", "Egger", false));
        /// <summary>
        /// Not Saved in the db
        /// </summary>
        public bool Egger
        {
            get
            {
                return GetProperty(EggerProperty);
            }
            set
            {
                LoadProperty(EggerProperty, value);
            }
        }

        private static PropertyInfo<bool> RankCorrProperty = RegisterProperty<bool>(new PropertyInfo<bool>("RankCorr", "RankCorr", false));
        /// <summary>
        /// Not Saved in the db
        /// </summary>
        public bool RankCorr
        {
            get
            {
                return GetProperty(RankCorrProperty);
            }
            set
            {
                LoadProperty(RankCorrProperty, value);
            }
        }

        private static PropertyInfo<bool> TrimFillProperty = RegisterProperty<bool>(new PropertyInfo<bool>("TrimFill", "TrimFill", false));
        /// <summary>
        /// Not Saved in the db
        /// </summary>
        public bool TrimFill
        {
            get
            {
                return GetProperty(TrimFillProperty);
            }
            set
            {
                LoadProperty(TrimFillProperty, value);
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

        /// <summary>
        /// Not Saved in the db
        /// </summary>
        public int StatisticalModel
        {
            get
            {
                return GetProperty(StatisticalModelProperty);
            }
            set
            {
                LoadProperty(StatisticalModelProperty, value);
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
        /// <summary>
        /// Not Saved in the db
        /// </summary>
        public int Verbose
        {
            get
            {
                return GetProperty(VerboseProperty);
            }
            set
            {
                LoadProperty(VerboseProperty, value);
            }
        }

        private static PropertyInfo<int> SignificanceLevelProperty = RegisterProperty<int>(new PropertyInfo<int>("SignificanceLevel", "SignificanceLevel", 95));
        /// <summary>
        /// Not Saved in the db
        /// </summary>
        public int SignificanceLevel
        {
            get
            {
                return GetProperty(SignificanceLevelProperty);
            }
            set
            {
                LoadProperty(SignificanceLevelProperty, value);
            }
        }

        /// <summary>
        /// Not Saved in the db
        /// </summary>
        private static PropertyInfo<int> DecPlacesProperty = RegisterProperty<int>(new PropertyInfo<int>("DecPlaces", "DecPlaces", 4));
        public int DecPlaces
        {
            get
            {
                return GetProperty(DecPlacesProperty);
            }
            set
            {
                LoadProperty(DecPlacesProperty, value);
            }
        }

        private static PropertyInfo<string> XAxisTitleProperty = RegisterProperty<string>(new PropertyInfo<string>("XAxisTitle", "XAxisTitle", string.Empty));
        /// <summary>
        /// Not Saved in the db
        /// </summary>
        public string XAxisTitle
        {
            get
            {
                return GetProperty(XAxisTitleProperty);
            }
            set
            {
                LoadProperty(XAxisTitleProperty, value);
            }
        }

        private static PropertyInfo<string> SummaryEstimateTitleProperty = RegisterProperty<string>(new PropertyInfo<string>("SummaryEstimateTitle", "SummaryEstimateTitle", string.Empty));
        /// <summary>
        /// Not Saved in the db
        /// </summary>
        public string SummaryEstimateTitle
        {
            get
            {
                return GetProperty(SummaryEstimateTitleProperty);
            }
            set
            {
                LoadProperty(SummaryEstimateTitleProperty, value);
            }
        }

        private static PropertyInfo<bool> ShowAnnotationsProperty = RegisterProperty<bool>(new PropertyInfo<bool>("ShowAnnotations", "ShowAnnotations", true));
        /// <summary>
        /// Not Saved in the db
        /// </summary>
        public bool ShowAnnotations
        {
            get
            {
                return GetProperty(ShowAnnotationsProperty);
            }
            set
            {
                LoadProperty(ShowAnnotationsProperty, value);
            }
        }

        private static PropertyInfo<bool> ShowAnnotationWeightsProperty = RegisterProperty<bool>(new PropertyInfo<bool>("ShowAnnotationWeights", "ShowAnnotationWeights", false));
        /// <summary>
        /// Not Saved in the db
        /// </summary>
        public bool ShowAnnotationWeights
        {
            get
            {
                return GetProperty(ShowAnnotationWeightsProperty);
            }
            set
            {
                LoadProperty(ShowAnnotationWeightsProperty, value);
            }
        }

        private static PropertyInfo<bool> FittedValsProperty = RegisterProperty<bool>(new PropertyInfo<bool>("FittedVals", "FittedVals", true));
        /// <summary>
        /// Not Saved in the db
        /// </summary>
        public bool FittedVals
        {
            get
            {
                return GetProperty(FittedValsProperty);
            }
            set
            {
                LoadProperty(FittedValsProperty, value);
            }
        }

        private static PropertyInfo<bool> CredIntProperty = RegisterProperty<bool>(new PropertyInfo<bool>("CredInt", "CredInt", false));
        /// <summary>
        /// Not Saved in the db
        /// </summary>
        public bool CredInt
        {
            get
            {
                return GetProperty(CredIntProperty);
            }
            set
            {
                LoadProperty(CredIntProperty, value);
            }
        }

        private static PropertyInfo<bool> ShowFunnelProperty = RegisterProperty<bool>(new PropertyInfo<bool>("ShowFunnel", "ShowFunnel", false));
        /// <summary>
        /// Not Saved in the db
        /// </summary>
        public bool ShowFunnel
        {
            get
            {
                return GetProperty(ShowFunnelProperty);
            }
            set
            {
                LoadProperty(ShowFunnelProperty, value);
            }
        }

        private static PropertyInfo<bool> ShowBoxplotProperty = RegisterProperty<bool>(new PropertyInfo<bool>("ShowBoxplot", "ShowBoxplot", false));
        /// <summary>
        /// Not Saved in the db
        /// </summary>
        public bool ShowBoxplot
        {
            get
            {
                return GetProperty(ShowBoxplotProperty);
            }
            set
            {
                LoadProperty(ShowBoxplotProperty, value);
            }
        }

        public void SetupModeratorList()
        {
            bool wasDirty = this.IsDirty;
            if (MetaAnalysisModerators == null) MetaAnalysisModerators = new MetaAnalysisModeratorList();
            else MetaAnalysisModerators.Clear();
            MetaAnalysisModerator mam = new MetaAnalysisModerator();
            mam.Name = "Intervention";
            mam.IsSelected = false;
            mam.AttributeID = 0;
            mam.FieldName = "InterventionText";
            GetReferenceValues(mam, "InterventionText");
            //mam.DoMarkAsOld();
            MetaAnalysisModerators.Add(mam);
            mam = new MetaAnalysisModerator();
            mam.Name = "Comparison";
            mam.IsSelected = false;
            mam.AttributeID = 0;
            mam.FieldName = "ControlText";
            GetReferenceValues(mam, "ControlText");
            //mam.DoMarkAsOld();
            MetaAnalysisModerators.Add(mam);
            mam = new MetaAnalysisModerator();
            mam.Name = "Outcome";
            mam.IsSelected = false;
            mam.AttributeID = 0;
            mam.FieldName = "OutcomeText";
            GetReferenceValues(mam, "OutcomeText");
            //mam.DoMarkAsOld();
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
                    //mam.DoMarkAsOld();
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
                    //mam.DoMarkAsOld();
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
                    //mam.DoMarkAsOld();
                    MetaAnalysisModerators.Add(mam);
                }
            }
            if (wasDirty == false && this.IsDirty == true) MarkClean();
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
        /// <summary>
        /// Not Saved in the db
        /// </summary>
        public int NMAStatisticalModel
        {
            get
            {
                return GetProperty(NMAStatisticalModelProperty);
            }
            set
            {
                LoadProperty(NMAStatisticalModelProperty, value);
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
        /// <summary>
        /// Not Saved in the db
        /// </summary>
        public string NMAReference
        {
            get
            {
                return GetProperty(NMAReferenceProperty);
            }
            set
            {
                LoadProperty(NMAReferenceProperty, value);//"SetProperty" marks obj as dirty, "LoadProperty" doesn't...
                //this property isn't saved in the DB, so we DON'T want it "tracked". Others would be like this, but this one
                //gets set upon loading in the ER4 UI, hence the need.
            }
        }

        private static PropertyInfo<bool> ExponentiatedProperty = RegisterProperty<bool>(new PropertyInfo<bool>("Exponentiated", "Exponentiated", false));
        /// <summary>
        /// Not Saved in the db
        /// </summary>
        public bool Exponentiated
        {
            get
            {
                return GetProperty(ExponentiatedProperty);
            }
            set
            {
                LoadProperty(ExponentiatedProperty, value);
            }
        }

        private static PropertyInfo<bool> AllTreatmentsProperty = RegisterProperty<bool>(new PropertyInfo<bool>("AllTreatments", "AllTreatments", true));
        public bool AllTreatments
        {
            get
            {
                return GetProperty(AllTreatmentsProperty);
            }
            set
            {
                SetProperty(AllTreatmentsProperty, value);
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

        // ****************************** THESE FIELDS ARE FOR THE GRADE ASSESSMENT OF THIS META-ANALYSIS *************************

        private static PropertyInfo<int> RandomisedProperty = RegisterProperty<int>(new PropertyInfo<int>("Randomised", "Randomised"));
        public int Randomised
        {
            get
            {
                return GetProperty(RandomisedProperty);
            }
            set
            {
                SetProperty(RandomisedProperty, value);
            }
        }

        private static PropertyInfo<int> RoBProperty = RegisterProperty<int>(new PropertyInfo<int>("RoB", "RoB"));
        public int RoB
        {
            get
            {
                return GetProperty(RoBProperty);
            }
            set
            {
                SetProperty(RoBProperty, value);
            }
        }

        private static PropertyInfo<int> InconProperty = RegisterProperty<int>(new PropertyInfo<int>("Incon", "Incon"));
        public int Incon
        {
            get
            {
                return GetProperty(InconProperty);
            }
            set
            {
                SetProperty(InconProperty, value);
            }
        }

        private static PropertyInfo<int> IndirectProperty = RegisterProperty<int>(new PropertyInfo<int>("Indirect", "Indirect"));
        public int Indirect
        {
            get
            {
                return GetProperty(IndirectProperty);
            }
            set
            {
                SetProperty(IndirectProperty, value);
            }
        }

        private static PropertyInfo<int> ImprecProperty = RegisterProperty<int>(new PropertyInfo<int>("Imprec", "Imprec"));
        public int Imprec
        {
            get
            {
                return GetProperty(ImprecProperty);
            }
            set
            {
                SetProperty(ImprecProperty, value);
            }
        }

        private static PropertyInfo<int> PubBiasProperty = RegisterProperty<int>(new PropertyInfo<int>("PubBias", "PubBias"));
        public int PubBias
        {
            get
            {
                return GetProperty(PubBiasProperty);
            }
            set
            {
                SetProperty(PubBiasProperty, value);
            }
        }

        private static PropertyInfo<int> CertaintyLevelProperty = RegisterProperty<int>(new PropertyInfo<int>("CertaintyLevel", "CertaintyLevel"));
        public int CertaintyLevel
        {
            get
            {
                return GetProperty(CertaintyLevelProperty);
            }
            set
            {
                SetProperty(CertaintyLevelProperty, value);
            }
        }

        private static PropertyInfo<string> RoBCommentProperty = RegisterProperty<string>(new PropertyInfo<string>("RoBComment", "RoBComment", string.Empty));
        public string RoBComment
        {
            get
            {
                return GetProperty(RoBCommentProperty);
            }
            set
            {
                SetProperty(RoBCommentProperty, value);
            }
        }

        private static PropertyInfo<bool> RoBSequenceProperty = RegisterProperty<bool>(new PropertyInfo<bool>("RoBSequence", "RoBSequence", false));
        public bool RoBSequence
        {
            get
            {
                return GetProperty(RoBSequenceProperty);
            }
            set
            {
                SetProperty(RoBSequenceProperty, value);
            }
        }

        private static PropertyInfo<bool> RoBConcealmentProperty = RegisterProperty<bool>(new PropertyInfo<bool>("RoBConcealment", "RoBConcealment", false));
        public bool RoBConcealment
        {
            get
            {
                return GetProperty(RoBConcealmentProperty);
            }
            set
            {
                SetProperty(RoBConcealmentProperty, value);
            }
        }

        private static PropertyInfo<bool> RoBBlindingParticipantsProperty = RegisterProperty<bool>(new PropertyInfo<bool>("RoBBlindingParticipants", "RoBBlindingParticipants", false));
        public bool RoBBlindingParticipants
        {
            get
            {
                return GetProperty(RoBBlindingParticipantsProperty);
            }
            set
            {
                SetProperty(RoBBlindingParticipantsProperty, value);
            }
        }

        private static PropertyInfo<bool> RoBBlindingAssessorsProperty = RegisterProperty<bool>(new PropertyInfo<bool>("RoBBlindingAssessors", "RoBBlindingAssessors", false));
        public bool RoBBlindingAssessors
        {
            get
            {
                return GetProperty(RoBBlindingAssessorsProperty);
            }
            set
            {
                SetProperty(RoBBlindingAssessorsProperty, value);
            }
        }

        private static PropertyInfo<bool> RoBIncompleteProperty = RegisterProperty<bool>(new PropertyInfo<bool>("RoBIncomplete", "RoBIncomplete", false));
        public bool RoBIncomplete
        {
            get
            {
                return GetProperty(RoBIncompleteProperty);
            }
            set
            {
                SetProperty(RoBIncompleteProperty, value);
            }
        }

        private static PropertyInfo<bool> RoBSelectiveProperty = RegisterProperty<bool>(new PropertyInfo<bool>("RoBSelective", "RoBSelective", false));
        public bool RoBSelective
        {
            get
            {
                return GetProperty(RoBSelectiveProperty);
            }
            set
            {
                SetProperty(RoBSelectiveProperty, value);
            }
        }

        private static PropertyInfo<bool> RoBNoIntentionProperty = RegisterProperty<bool>(new PropertyInfo<bool>("RoBNoIntention", "RoBNoIntention", false));
        public bool RoBNoIntention
        {
            get
            {
                return GetProperty(RoBNoIntentionProperty);
            }
            set
            {
                SetProperty(RoBNoIntentionProperty, value);
            }
        }

        private static PropertyInfo<bool> RoBCarryoverProperty = RegisterProperty<bool>(new PropertyInfo<bool>("RoBCarryover", "RoBCarryover", false));
        public bool RoBCarryover
        {
            get
            {
                return GetProperty(RoBCarryoverProperty);
            }
            set
            {
                SetProperty(RoBCarryoverProperty, value);
            }
        }

        private static PropertyInfo<bool> RoBStoppedProperty = RegisterProperty<bool>(new PropertyInfo<bool>("RoBStopped", "RoBStopped", false));
        public bool RoBStopped
        {
            get
            {
                return GetProperty(RoBStoppedProperty);
            }
            set
            {
                SetProperty(RoBStoppedProperty, value);
            }
        }

        private static PropertyInfo<bool> RoBUnvalidatedProperty = RegisterProperty<bool>(new PropertyInfo<bool>("RoBUnvalidated", "RoBUnvalidated", false));
        public bool RoBUnvalidated
        {
            get
            {
                return GetProperty(RoBUnvalidatedProperty);
            }
            set
            {
                SetProperty(RoBUnvalidatedProperty, value);
            }
        }

        private static PropertyInfo<bool> RoBOtherProperty = RegisterProperty<bool>(new PropertyInfo<bool>("RoBOther", "RoBOther", false));
        public bool RoBOther
        {
            get
            {
                return GetProperty(RoBOtherProperty);
            }
            set
            {
                SetProperty(RoBOtherProperty, value);
            }
        }

        private static PropertyInfo<string> InconCommentProperty = RegisterProperty<string>(new PropertyInfo<string>("InconComment", "InconComment", string.Empty));
        public string InconComment
        {
            get
            {
                return GetProperty(InconCommentProperty);
            }
            set
            {
                SetProperty(InconCommentProperty, value);
            }
        }

        private static PropertyInfo<bool> InconPointProperty = RegisterProperty<bool>(new PropertyInfo<bool>("InconPoint", "InconPoint", false));
        public bool InconPoint
        {
            get
            {
                return GetProperty(InconPointProperty);
            }
            set
            {
                SetProperty(InconPointProperty, value);
            }
        }

        private static PropertyInfo<bool> InconCIsProperty = RegisterProperty<bool>(new PropertyInfo<bool>("InconCIs", "InconCIs", false));
        public bool InconCIs
        {
            get
            {
                return GetProperty(InconCIsProperty);
            }
            set
            {
                SetProperty(InconCIsProperty, value);
            }
        }

        private static PropertyInfo<bool> InconDirectionProperty = RegisterProperty<bool>(new PropertyInfo<bool>("InconDirection", "InconDirection", false));
        public bool InconDirection
        {
            get
            {
                return GetProperty(InconDirectionProperty);
            }
            set
            {
                SetProperty(InconDirectionProperty, value);
            }
        }

        private static PropertyInfo<bool> InconStatisticalProperty = RegisterProperty<bool>(new PropertyInfo<bool>("InconStatistical", "InconStatistical", false));
        public bool InconStatistical
        {
            get
            {
                return GetProperty(InconStatisticalProperty);
            }
            set
            {
                SetProperty(InconStatisticalProperty, value);
            }
        }

        private static PropertyInfo<bool> InconOtherProperty = RegisterProperty<bool>(new PropertyInfo<bool>("InconOther", "InconOther", false));
        public bool InconOther
        {
            get
            {
                return GetProperty(InconOtherProperty);
            }
            set
            {
                SetProperty(InconOtherProperty, value);
            }
        }

        private static PropertyInfo<string> IndirectCommentProperty = RegisterProperty<string>(new PropertyInfo<string>("IndirectComment", "IndirectComment", string.Empty));
        public string IndirectComment
        {
            get
            {
                return GetProperty(IndirectCommentProperty);
            }
            set
            {
                SetProperty(IndirectCommentProperty, value);
            }
        }

        private static PropertyInfo<bool> IndirectPopulationProperty = RegisterProperty<bool>(new PropertyInfo<bool>("IndirectPopulation", "IndirectPopulation", false));
        public bool IndirectPopulation
        {
            get
            {
                return GetProperty(IndirectPopulationProperty);
            }
            set
            {
                SetProperty(IndirectPopulationProperty, value);
            }
        }

        private static PropertyInfo<bool> IndirectOutcomeProperty = RegisterProperty<bool>(new PropertyInfo<bool>("IndirectOutcome", "IndirectOutcome", false));
        public bool IndirectOutcome
        {
            get
            {
                return GetProperty(IndirectOutcomeProperty);
            }
            set
            {
                SetProperty(IndirectOutcomeProperty, value);
            }
        }

        private static PropertyInfo<bool> IndirectNoDirectProperty = RegisterProperty<bool>(new PropertyInfo<bool>("IndirectNoDirect", "IndirectNoDirect", false));
        public bool IndirectNoDirect
        {
            get
            {
                return GetProperty(IndirectNoDirectProperty);
            }
            set
            {
                SetProperty(IndirectNoDirectProperty, value);
            }
        }

        private static PropertyInfo<bool> IndirectInterventionProperty = RegisterProperty<bool>(new PropertyInfo<bool>("IndirectIntervention", "IndirectIntervention", false));
        public bool IndirectIntervention
        {
            get
            {
                return GetProperty(IndirectInterventionProperty);
            }
            set
            {
                SetProperty(IndirectInterventionProperty, value);
            }
        }

        private static PropertyInfo<bool> IndirectTimeProperty = RegisterProperty<bool>(new PropertyInfo<bool>("IndirectTime", "IndirectTime", false));
        public bool IndirectTime
        {
            get
            {
                return GetProperty(IndirectTimeProperty);
            }
            set
            {
                SetProperty(IndirectTimeProperty, value);
            }
        }

        private static PropertyInfo<bool> IndirectOtherProperty = RegisterProperty<bool>(new PropertyInfo<bool>("IndirectOther", "IndirectOther", false));
        public bool IndirectOther
        {
            get
            {
                return GetProperty(IndirectOtherProperty);
            }
            set
            {
                SetProperty(IndirectOtherProperty, value);
            }
        }

        private static PropertyInfo<string> ImprecCommentProperty = RegisterProperty<string>(new PropertyInfo<string>("ImprecComment", "ImprecComment", string.Empty));
        public string ImprecComment
        {
            get
            {
                return GetProperty(ImprecCommentProperty);
            }
            set
            {
                SetProperty(ImprecCommentProperty, value);
            }
        }

        private static PropertyInfo<bool> ImprecWideProperty = RegisterProperty<bool>(new PropertyInfo<bool>("ImprecWide", "ImprecWide", false));
        public bool ImprecWide
        {
            get
            {
                return GetProperty(ImprecWideProperty);
            }
            set
            {
                SetProperty(ImprecWideProperty, value);
            }
        }

        private static PropertyInfo<bool> ImprecFewProperty = RegisterProperty<bool>(new PropertyInfo<bool>("ImprecFew", "ImprecFew", false));
        public bool ImprecFew
        {
            get
            {
                return GetProperty(ImprecFewProperty);
            }
            set
            {
                SetProperty(ImprecFewProperty, value);
            }
        }

        private static PropertyInfo<bool> ImprecOnlyOneProperty = RegisterProperty<bool>(new PropertyInfo<bool>("ImprecOnlyOne", "ImprecOnlyOne", false));
        public bool ImprecOnlyOne
        {
            get
            {
                return GetProperty(ImprecOnlyOneProperty);
            }
            set
            {
                SetProperty(ImprecOnlyOneProperty, value);
            }
        }

        private static PropertyInfo<bool> ImprecOtherProperty = RegisterProperty<bool>(new PropertyInfo<bool>("ImprecOther", "ImprecOther", false));
        public bool ImprecOther
        {
            get
            {
                return GetProperty(ImprecOtherProperty);
            }
            set
            {
                SetProperty(ImprecOtherProperty, value);
            }
        }

        private static PropertyInfo<string> PubBiasCommentProperty = RegisterProperty<string>(new PropertyInfo<string>("PubBiasComment", "PubBiasComment", string.Empty));
        public string PubBiasComment
        {
            get
            {
                return GetProperty(PubBiasCommentProperty);
            }
            set
            {
                SetProperty(PubBiasCommentProperty, value);
            }
        }

        private static PropertyInfo<bool> PubBiasCommerciallyProperty = RegisterProperty<bool>(new PropertyInfo<bool>("PubBiasCommercially", "PubBiasCommercially", false));
        public bool PubBiasCommercially
        {
            get
            {
                return GetProperty(PubBiasCommerciallyProperty);
            }
            set
            {
                SetProperty(PubBiasCommerciallyProperty, value);
            }
        }

        private static PropertyInfo<bool> PubBiasAsymmetricalProperty = RegisterProperty<bool>(new PropertyInfo<bool>("PubBiasAsymmetrical", "PubBiasAsymmetrical", false));
        public bool PubBiasAsymmetrical
        {
            get
            {
                return GetProperty(PubBiasAsymmetricalProperty);
            }
            set
            {
                SetProperty(PubBiasAsymmetricalProperty, value);
            }
        }

        private static PropertyInfo<bool> PubBiasLimitedProperty = RegisterProperty<bool>(new PropertyInfo<bool>("PubBiasLimited", "PubBiasLimited", false));
        public bool PubBiasLimited
        {
            get
            {
                return GetProperty(PubBiasLimitedProperty);
            }
            set
            {
                SetProperty(PubBiasLimitedProperty, value);
            }
        }

        private static PropertyInfo<bool> PubBiasMissingProperty = RegisterProperty<bool>(new PropertyInfo<bool>("PubBiasMissing", "PubBiasMissing", false));
        public bool PubBiasMissing
        {
            get
            {
                return GetProperty(PubBiasMissingProperty);
            }
            set
            {
                SetProperty(PubBiasMissingProperty, value);
            }
        }

        private static PropertyInfo<bool> PubBiasDiscontinuedProperty = RegisterProperty<bool>(new PropertyInfo<bool>("PubBiasDiscontinued", "PubBiasDiscontinued", false));
        public bool PubBiasDiscontinued
        {
            get
            {
                return GetProperty(PubBiasDiscontinuedProperty);
            }
            set
            {
                SetProperty(PubBiasDiscontinuedProperty, value);
            }
        }

        private static PropertyInfo<bool> PubBiasDiscrepancyProperty = RegisterProperty<bool>(new PropertyInfo<bool>("PubBiasDiscrepancy", "PubBiasDiscrepancy", false));
        public bool PubBiasDiscrepancy
        {
            get
            {
                return GetProperty(PubBiasDiscrepancyProperty);
            }
            set
            {
                SetProperty(PubBiasDiscrepancyProperty, value);
            }
        }

        private static PropertyInfo<bool> PubBiasOtherProperty = RegisterProperty<bool>(new PropertyInfo<bool>("PubBiasOther", "PubBiasOther", false));
        public bool PubBiasOther
        {
            get
            {
                return GetProperty(PubBiasOtherProperty);
            }
            set
            {
                SetProperty(PubBiasOtherProperty, value);
            }
        }

        private static PropertyInfo<string> UpgradeCommentProperty = RegisterProperty<string>(new PropertyInfo<string>("UpgradeComment", "UpgradeComment", string.Empty));
        public string UpgradeComment
        {
            get
            {
                return GetProperty(UpgradeCommentProperty);
            }
            set
            {
                SetProperty(UpgradeCommentProperty, value);
            }
        }

        private static PropertyInfo<bool> UpgradeLargeProperty = RegisterProperty<bool>(new PropertyInfo<bool>("UpgradeLarge", "UpgradeLarge", false));
        public bool UpgradeLarge
        {
            get
            {
                return GetProperty(UpgradeLargeProperty);
            }
            set
            {
                SetProperty(UpgradeLargeProperty, value);
            }
        }

        private static PropertyInfo<bool> UpgradeVeryLargeProperty = RegisterProperty<bool>(new PropertyInfo<bool>("UpgradeVeryLarge", "UpgradeVeryLarge", false));
        public bool UpgradeVeryLarge
        {
            get
            {
                return GetProperty(UpgradeVeryLargeProperty);
            }
            set
            {
                SetProperty(UpgradeVeryLargeProperty, value);
            }
        }

        private static PropertyInfo<bool> UpgradeAllPlausibleProperty = RegisterProperty<bool>(new PropertyInfo<bool>("UpgradeAllPlausible", "UpgradeAllPlausible", false));
        public bool UpgradeAllPlausible
        {
            get
            {
                return GetProperty(UpgradeAllPlausibleProperty);
            }
            set
            {
                SetProperty(UpgradeAllPlausibleProperty, value);
            }
        }

        private static PropertyInfo<bool> UpgradeClearProperty = RegisterProperty<bool>(new PropertyInfo<bool>("UpgradeClear", "UpgradeClear", false));
        public bool UpgradeClear
        {
            get
            {
                return GetProperty(UpgradeClearProperty);
            }
            set
            {
                SetProperty(UpgradeClearProperty, value);
            }
        }

        private static PropertyInfo<bool> UpgradeNoneProperty = RegisterProperty<bool>(new PropertyInfo<bool>("UpgradeNone", "UpgradeNone", false));
        public bool UpgradeNone
        {
            get
            {
                return GetProperty(UpgradeNoneProperty);
            }
            set
            {
                SetProperty(UpgradeNoneProperty, value);
            }
        }

        private static PropertyInfo<string> CertaintyLevelCommentProperty = RegisterProperty<string>(new PropertyInfo<string>("CertaintyLevelComment", "CertaintyLevelComment", string.Empty));
        public string CertaintyLevelComment
        {
            get
            {
                return GetProperty(CertaintyLevelCommentProperty);
            }
            set
            {
                SetProperty(CertaintyLevelCommentProperty, value);
            }
        }


        // ***************************************** END FIELDS FOR GRADE ASSESSMENT ********************************************

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
            set
            {
                SetProperty(MetaAnalysisTypeTitleProperty, value);
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


        private static PropertyInfo<MetaAnalysisFilterSettingList> FilterSettingsListProperty = RegisterProperty<MetaAnalysisFilterSettingList>(new PropertyInfo<MetaAnalysisFilterSettingList>("FilterSettingsList", "FilterSettingsList"));
        public MetaAnalysisFilterSettingList FilterSettingsList
        {
            get
            {
                return GetProperty(FilterSettingsListProperty);
            }
            set
            {
                SetProperty(FilterSettingsListProperty, value);
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

        public static readonly string[] levels = {"No serious", "Serious", "Very serious" };

        public static readonly string[] cbRoB = { "Sequence generation", "Concealment", "Blinding participants / personnel", "Blinding assessors", "Incomplete data",
            "Selective outcome reporting", "Intention-to-treat", "Carryover effects", "Stopped early", "Unvalidated measures", "Other issue"};

        public static readonly string[] cbIncon = {"Point estimates vary widely", "CIs not overlapping", "Direction not consistent",
                "Statistical heterogeneity", "Other issue"};
        public static readonly string[] cbIndirect = {"Population dissimilarity",  "Outcome dissimilarity", "No direct comparison",
            "Intervention / comparator dissimilarity", "Time frame insufficient", "Other issue"};

        public static readonly string[] cbImprec = {"Wide confidence intervals", "Few patients", "Only one study", "Other issue"};

        public static readonly string[] cbPubBias = {"Commercially funded", "Asymmetrical funnel plot", "Limited search", "Missing gray literature",
                "Discontinued studies", "Discrepancy published vs. unpublished", "Other issue"};

        public static readonly string[] cbUpgrade = {"Large magnitude of effect", "Very large magnitude of effect",
                "All plausible confounding would have reduced the effect", "Clear dose-response gradient", "None"};

       


        public string GRADEReport()
        {
            string _Randomised = "";
            string _CertaintyLevel = "";
            if (Randomised == 0)
            {
                _Randomised = "Randomised controlled";
            }
            if (Randomised == 1)
            {
                _Randomised = "Observational (non-randomised)";
            }
            switch (CertaintyLevel)
            {
                case 0: _CertaintyLevel = "High";
                    break;
                case 1: _CertaintyLevel = "Moderate";
                    break;
                case 2: _CertaintyLevel = "Low";
                    break;
                case 3: _CertaintyLevel = "Very low";
                    break;
                default:
                    break;
            }
            string report = "<p><h1>GRADE report</h1><h2>" + this.Title + "</h2>";
            report += "<h3>1. Are the studies you took results from randomised?</h3>";
            report += "<p><table border='1'><tr><td>STUDY TYPE</td><td>" + _Randomised + "</td></tr></table></p>";
            report += "<h3>2. Downgrade factors</h3>";
            report += "<p><table border = '1'><tr><td>FACTORS</td><td>PROBLEM AREAS</td><td>COMMENT</td></tr>";
            report += "<td>RISK OF BIAS: " + levels[RoB] + "</td><td><ul>" +
                (RoBSequence == true ? "<li>" + cbRoB[0] + "</li>" : "") +
                (RoBConcealment == true ? "<li>" + cbRoB[1] + "</li>" : "") +
                (RoBBlindingParticipants == true ? "<li>" + cbRoB[2] + "</li>" : "") +
                (RoBBlindingAssessors == true ? "<li>" + cbRoB[3] + "</li>" : "") +
                (RoBIncomplete == true ? "<li>" + cbRoB[4] + "</li>" : "") +
                (RoBSelective == true ? "<li>" + cbRoB[5] + "</li>" : "") +
                (RoBNoIntention == true ? "<li>" + cbRoB[6] + "</li>" : "") +
                (RoBCarryover == true ? "<li>" + cbRoB[7] + "</li>" : "") +
                (RoBStopped == true ? "<li>" + cbRoB[8] + "</li>" : "") +
                (RoBUnvalidated == true ? "<li>" + cbRoB[9] + "</li>" : "") +
                (RoBOther == true ? "<li>" + cbRoB[10] + "</li>" : "");
            report += "</ul></td><td>" + RoBComment + "</td></tr>";
            report += "<td>INCONSISTENCY: " + levels[Incon] + "</td><td><ul>" +
                (InconPoint == true ? "<li>" + cbIncon[0] + "</li>" : "") +
                (InconCIs == true ? "<li>" + cbIncon[1] + "</li>" : "") +
                (InconDirection == true ? "<li>" + cbIncon[2] + "</li>" : "") +
                (InconStatistical == true ? "<li>" + cbIncon[3] + "</li>" : "") +
                (InconOther == true ? "<li>" + cbIncon[4] + "</li>" : "");
            report += "</ul></td><td>" + InconComment + "</td></tr>";
            report += "<td>INDIRECTNESS: " + levels[Indirect] + "</td><td><ul>" +
                (IndirectPopulation == true ? "<li>" + cbIndirect[0] + "</li>" : "") +
                (IndirectOutcome == true ? "<li>" + cbIndirect[1] + "</li>" : "") +
                (IndirectNoDirect == true ? "<li>" + cbIndirect[2] + "</li>" : "") +
                (IndirectIntervention == true ? "<li>" + cbIndirect[3] + "</li>" : "") +
                (IndirectTime == true ? "<li>" + cbIndirect[4] + "</li>" : "")+
                (IndirectOther == true ? "<li>" + cbIndirect[5] + "</li>" : "");
            report += "</ul></td><td>" + IndirectComment + "</td></tr>";
            report += "<td>IMPRECISION: " + levels[Imprec] + "</td><td><ul>" +
                (ImprecWide == true ? "<li>" + cbImprec[0] + "</li>" : "") +
                (ImprecFew == true ? "<li>" + cbImprec[1] + "</li>" : "") +
                (ImprecOnlyOne == true ? "<li>" + cbImprec[2] + "</li>" : "") +
                (ImprecOther == true ? "<li>" + cbImprec[3] + "</li>" : "");
            report += "</ul></td><td>" + ImprecComment + "</td></tr>";
            report += "<td>PUBLICATION BIAS: " + levels[PubBias] + "</td><td><ul>" +
                (PubBiasCommercially == true ? "<li>" + cbPubBias[0] + "</li>" : "") +
                (PubBiasAsymmetrical == true ? "<li>" + cbPubBias[1] + "</li>" : "") +
                (PubBiasLimited == true ? "<li>" + cbPubBias[2] + "</li>" : "") +
                (PubBiasMissing == true ? "<li>" + cbPubBias[3] + "</li>" : "") +
                (PubBiasDiscontinued == true ? "<li>" + cbPubBias[4] + "</li>" : "") +
                (PubBiasDiscrepancy == true ? "<li>" + cbPubBias[5] + "</li>" : "") +
                (PubBiasOther == true ? "<li>" + cbPubBias[6] + "</li>" : "");
            report += "</ul></td><td>" + PubBiasComment + "</td></tr>";
            report += "</table></p>";
            report += "<h3>3. Upgrade factors (if relevant)</h3>";
            report += "<p><table border = '1'><tr><td>&nbsp</td><td>UPGRADE AREAS</td><td>COMMENT</td></tr>";
            report += "<td>&nbsp</td><td><ul>" +
                (UpgradeLarge == true ? "<li>" + cbUpgrade[0] + "</li>" : "") +
                (UpgradeVeryLarge == true ? "<li>" + cbUpgrade[1] + "</li>" : "") +
                (UpgradeAllPlausible == true ? "<li>" + cbUpgrade[2] + "</li>" : "") +
                (UpgradeClear == true ? "<li>" + cbUpgrade[3] + "</li>" : "") +
                (UpgradeNone == true ? "<li>" + cbUpgrade[4] + "</li>" : "");
            report += "</ul></td><td>" + UpgradeComment + "</td></tr>";
            report += "</table></p>";
            report += "<h3>4. Certainty level</h3>";
            report += "<p><table border = '1'><tr><td>CERTAINTY LEVEL</td><td>COMMENT</td></tr>";
            report += "<tr><td>" + _CertaintyLevel + "</td><td>" + CertaintyLevelComment + "</td></tr>";
            report += "</table></p>";
            return report;
        }


#if !SILVERLIGHT
    
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
                    //command.Parameters.Add(new SqlParameter("@GRID_SETTINGS", ReadProperty(GridSettingsProperty)));
                    command.Parameters.Add(new SqlParameter("@OUTCOME_IDS", OutcomeIds()));
                    command.Parameters.Add(new SqlParameter("@Randomised", ReadProperty(RandomisedProperty)));
                    command.Parameters.Add(new SqlParameter("@RoB", ReadProperty(RoBProperty)));
                    command.Parameters.Add(new SqlParameter("@RoBComment", ReadProperty(RoBCommentProperty)));
                    command.Parameters.Add(new SqlParameter("@RoBSequence", ReadProperty(RoBSequenceProperty)));
                    command.Parameters.Add(new SqlParameter("@RoBConcealment", ReadProperty(RoBConcealmentProperty)));
                    command.Parameters.Add(new SqlParameter("@RoBBlindingParticipants", ReadProperty(RoBBlindingParticipantsProperty)));
                    command.Parameters.Add(new SqlParameter("@RoBBlindingAssessors", ReadProperty(RoBBlindingAssessorsProperty)));
                    command.Parameters.Add(new SqlParameter("@RoBIncomplete", ReadProperty(RoBIncompleteProperty)));
                    command.Parameters.Add(new SqlParameter("@RoBSelective", ReadProperty(RoBSelectiveProperty)));
                    command.Parameters.Add(new SqlParameter("@RoBNoIntention", ReadProperty(RoBNoIntentionProperty)));
                    command.Parameters.Add(new SqlParameter("@RoBCarryover", ReadProperty(RoBCarryoverProperty)));
                    command.Parameters.Add(new SqlParameter("@RoBStopped", ReadProperty(RoBStoppedProperty)));
                    command.Parameters.Add(new SqlParameter("@RoBUnvalidated", ReadProperty(RoBUnvalidatedProperty)));
                    command.Parameters.Add(new SqlParameter("@RoBOther", ReadProperty(RoBOtherProperty)));
                    command.Parameters.Add(new SqlParameter("@Incon", ReadProperty(InconProperty)));
                    command.Parameters.Add(new SqlParameter("@InconComment", ReadProperty(InconCommentProperty)));
                    command.Parameters.Add(new SqlParameter("@InconPoint", ReadProperty(InconPointProperty)));
                    command.Parameters.Add(new SqlParameter("@InconCIs", ReadProperty(InconCIsProperty)));
                    command.Parameters.Add(new SqlParameter("@InconDirection", ReadProperty(InconDirectionProperty)));
                    command.Parameters.Add(new SqlParameter("@InconStatistical", ReadProperty(InconStatisticalProperty)));
                    command.Parameters.Add(new SqlParameter("@InconOther", ReadProperty(InconOtherProperty)));
                    command.Parameters.Add(new SqlParameter("@Indirect", ReadProperty(IndirectProperty)));
                    command.Parameters.Add(new SqlParameter("@IndirectComment", ReadProperty(IndirectCommentProperty)));
                    command.Parameters.Add(new SqlParameter("@IndirectPopulation", ReadProperty(IndirectPopulationProperty)));
                    command.Parameters.Add(new SqlParameter("@IndirectOutcome", ReadProperty(IndirectOutcomeProperty)));
                    command.Parameters.Add(new SqlParameter("@IndirectNoDirect", ReadProperty(IndirectNoDirectProperty)));
                    command.Parameters.Add(new SqlParameter("@IndirectIntervention", ReadProperty(IndirectInterventionProperty)));
                    command.Parameters.Add(new SqlParameter("@IndirectTime", ReadProperty(IndirectTimeProperty)));
                    command.Parameters.Add(new SqlParameter("@IndirectOther", ReadProperty(IndirectOtherProperty)));
                    command.Parameters.Add(new SqlParameter("@Imprec", ReadProperty(ImprecProperty)));
                    command.Parameters.Add(new SqlParameter("@ImprecComment", ReadProperty(ImprecCommentProperty)));
                    command.Parameters.Add(new SqlParameter("@ImprecWide", ReadProperty(ImprecWideProperty)));
                    command.Parameters.Add(new SqlParameter("@ImprecFew", ReadProperty(ImprecFewProperty)));
                    command.Parameters.Add(new SqlParameter("@ImprecOnlyOne", ReadProperty(ImprecOnlyOneProperty)));
                    command.Parameters.Add(new SqlParameter("@ImprecOther", ReadProperty(ImprecOtherProperty)));
                    command.Parameters.Add(new SqlParameter("@PubBias", ReadProperty(PubBiasProperty)));
                    command.Parameters.Add(new SqlParameter("@PubBiasComment", ReadProperty(PubBiasCommentProperty)));
                    command.Parameters.Add(new SqlParameter("@PubBiasCommercially", ReadProperty(PubBiasCommerciallyProperty)));
                    command.Parameters.Add(new SqlParameter("@PubBiasAsymmetrical", ReadProperty(PubBiasAsymmetricalProperty)));
                    command.Parameters.Add(new SqlParameter("@PubBiasLimited", ReadProperty(PubBiasLimitedProperty)));
                    command.Parameters.Add(new SqlParameter("@PubBiasMissing", ReadProperty(PubBiasMissingProperty)));
                    command.Parameters.Add(new SqlParameter("@PubBiasDiscontinued", ReadProperty(PubBiasDiscontinuedProperty)));
                    command.Parameters.Add(new SqlParameter("@PubBiasDiscrepancy", ReadProperty(PubBiasDiscrepancyProperty)));
                    command.Parameters.Add(new SqlParameter("@PubBiasOther", ReadProperty(PubBiasOtherProperty)));
                    command.Parameters.Add(new SqlParameter("@UpgradeComment", ReadProperty(UpgradeCommentProperty)));
                    command.Parameters.Add(new SqlParameter("@UpgradeLarge", ReadProperty(UpgradeLargeProperty)));
                    command.Parameters.Add(new SqlParameter("@UpgradeVeryLarge", ReadProperty(UpgradeVeryLargeProperty)));
                    command.Parameters.Add(new SqlParameter("@UpgradeAllPlausible", ReadProperty(UpgradeAllPlausibleProperty)));
                    command.Parameters.Add(new SqlParameter("@UpgradeClear", ReadProperty(UpgradeClearProperty)));
                    command.Parameters.Add(new SqlParameter("@UpgradeNone", ReadProperty(UpgradeNoneProperty)));
                    command.Parameters.Add(new SqlParameter("@CertaintyLevel", ReadProperty(CertaintyLevelProperty)));
                    command.Parameters.Add(new SqlParameter("@CertaintyLevelComment", ReadProperty(CertaintyLevelCommentProperty)));

                    command.Parameters.Add(new SqlParameter("@SORTED_FIELD", ReadProperty(SortedByProperty)));
                    command.Parameters.Add(new SqlParameter("@SORT_DIRECTION", System.Data.SqlDbType.Bit));
                    if (SortDirection == "") command.Parameters["@SORT_DIRECTION"].Value = null;
                    else if (SortDirection == "Ascending") command.Parameters["@SORT_DIRECTION"].Value = true;
                    else command.Parameters["@SORT_DIRECTION"].Value = false;

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
                SaveFilterSettings();
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
                    //command.Parameters.Add(new SqlParameter("@GRID_SETTINGS", ReadProperty(GridSettingsProperty)));
                    command.Parameters.Add(new SqlParameter("@Randomised", ReadProperty(RandomisedProperty)));
                    command.Parameters.Add(new SqlParameter("@RoB", ReadProperty(RoBProperty)));
                    command.Parameters.Add(new SqlParameter("@RoBComment", ReadProperty(RoBCommentProperty)));
                    command.Parameters.Add(new SqlParameter("@RoBSequence", ReadProperty(RoBSequenceProperty)));
                    command.Parameters.Add(new SqlParameter("@RoBConcealment", ReadProperty(RoBConcealmentProperty)));
                    command.Parameters.Add(new SqlParameter("@RoBBlindingParticipants", ReadProperty(RoBBlindingParticipantsProperty)));
                    command.Parameters.Add(new SqlParameter("@RoBBlindingAssessors", ReadProperty(RoBBlindingAssessorsProperty)));
                    command.Parameters.Add(new SqlParameter("@RoBIncomplete", ReadProperty(RoBIncompleteProperty)));
                    command.Parameters.Add(new SqlParameter("@RoBSelective", ReadProperty(RoBSelectiveProperty)));
                    command.Parameters.Add(new SqlParameter("@RoBNoIntention", ReadProperty(RoBNoIntentionProperty)));
                    command.Parameters.Add(new SqlParameter("@RoBCarryover", ReadProperty(RoBCarryoverProperty)));
                    command.Parameters.Add(new SqlParameter("@RoBStopped", ReadProperty(RoBStoppedProperty)));
                    command.Parameters.Add(new SqlParameter("@RoBUnvalidated", ReadProperty(RoBUnvalidatedProperty)));
                    command.Parameters.Add(new SqlParameter("@RoBOther", ReadProperty(RoBOtherProperty)));
                    command.Parameters.Add(new SqlParameter("@Incon", ReadProperty(InconProperty)));
                    command.Parameters.Add(new SqlParameter("@InconComment", ReadProperty(InconCommentProperty)));
                    command.Parameters.Add(new SqlParameter("@InconPoint", ReadProperty(InconPointProperty)));
                    command.Parameters.Add(new SqlParameter("@InconCIs", ReadProperty(InconCIsProperty)));
                    command.Parameters.Add(new SqlParameter("@InconDirection", ReadProperty(InconDirectionProperty)));
                    command.Parameters.Add(new SqlParameter("@InconStatistical", ReadProperty(InconStatisticalProperty)));
                    command.Parameters.Add(new SqlParameter("@InconOther", ReadProperty(InconOtherProperty)));
                    command.Parameters.Add(new SqlParameter("@Indirect", ReadProperty(IndirectProperty)));
                    command.Parameters.Add(new SqlParameter("@IndirectComment", ReadProperty(IndirectCommentProperty)));
                    command.Parameters.Add(new SqlParameter("@IndirectPopulation", ReadProperty(IndirectPopulationProperty)));
                    command.Parameters.Add(new SqlParameter("@IndirectOutcome", ReadProperty(IndirectOutcomeProperty)));
                    command.Parameters.Add(new SqlParameter("@IndirectNoDirect", ReadProperty(IndirectNoDirectProperty)));
                    command.Parameters.Add(new SqlParameter("@IndirectIntervention", ReadProperty(IndirectInterventionProperty)));
                    command.Parameters.Add(new SqlParameter("@IndirectTime", ReadProperty(IndirectTimeProperty)));
                    command.Parameters.Add(new SqlParameter("@IndirectOther", ReadProperty(IndirectOtherProperty)));
                    command.Parameters.Add(new SqlParameter("@Imprec", ReadProperty(ImprecProperty)));
                    command.Parameters.Add(new SqlParameter("@ImprecComment", ReadProperty(ImprecCommentProperty)));
                    command.Parameters.Add(new SqlParameter("@ImprecWide", ReadProperty(ImprecWideProperty)));
                    command.Parameters.Add(new SqlParameter("@ImprecFew", ReadProperty(ImprecFewProperty)));
                    command.Parameters.Add(new SqlParameter("@ImprecOnlyOne", ReadProperty(ImprecOnlyOneProperty)));
                    command.Parameters.Add(new SqlParameter("@ImprecOther", ReadProperty(ImprecOtherProperty)));
                    command.Parameters.Add(new SqlParameter("@PubBias", ReadProperty(PubBiasProperty)));
                    command.Parameters.Add(new SqlParameter("@PubBiasComment", ReadProperty(PubBiasCommentProperty)));
                    command.Parameters.Add(new SqlParameter("@PubBiasCommercially", ReadProperty(PubBiasCommerciallyProperty)));
                    command.Parameters.Add(new SqlParameter("@PubBiasAsymmetrical", ReadProperty(PubBiasAsymmetricalProperty)));
                    command.Parameters.Add(new SqlParameter("@PubBiasLimited", ReadProperty(PubBiasLimitedProperty)));
                    command.Parameters.Add(new SqlParameter("@PubBiasMissing", ReadProperty(PubBiasMissingProperty)));
                    command.Parameters.Add(new SqlParameter("@PubBiasDiscontinued", ReadProperty(PubBiasDiscontinuedProperty)));
                    command.Parameters.Add(new SqlParameter("@PubBiasDiscrepancy", ReadProperty(PubBiasDiscrepancyProperty)));
                    command.Parameters.Add(new SqlParameter("@PubBiasOther", ReadProperty(PubBiasOtherProperty)));
                    command.Parameters.Add(new SqlParameter("@UpgradeComment", ReadProperty(UpgradeCommentProperty)));
                    command.Parameters.Add(new SqlParameter("@UpgradeLarge", ReadProperty(UpgradeLargeProperty)));
                    command.Parameters.Add(new SqlParameter("@UpgradeVeryLarge", ReadProperty(UpgradeVeryLargeProperty)));
                    command.Parameters.Add(new SqlParameter("@UpgradeAllPlausible", ReadProperty(UpgradeAllPlausibleProperty)));
                    command.Parameters.Add(new SqlParameter("@UpgradeClear", ReadProperty(UpgradeClearProperty)));
                    command.Parameters.Add(new SqlParameter("@UpgradeNone", ReadProperty(UpgradeNoneProperty)));
                    command.Parameters.Add(new SqlParameter("@CertaintyLevel", ReadProperty(CertaintyLevelProperty)));
                    command.Parameters.Add(new SqlParameter("@CertaintyLevelComment", ReadProperty(CertaintyLevelCommentProperty)));
                    
                    command.Parameters.Add(new SqlParameter("@SORTED_FIELD", ReadProperty(SortedByProperty)));
                    command.Parameters.Add(new SqlParameter("@SORT_DIRECTION", System.Data.SqlDbType.Bit));
                    command.Parameters["@SORT_DIRECTION"].IsNullable = true;
                    if (SortDirection == "") command.Parameters["@SORT_DIRECTION"].Value = null;
                    else if (SortDirection == "Ascending") command.Parameters["@SORT_DIRECTION"].Value = true;
                    else command.Parameters["@SORT_DIRECTION"].Value = false;

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
                    SaveFilterSettings();
                    Outcomes = OutcomeList.GetOutcomeList(SetId, AttributeIdIntervention, AttributeIdControl,
                        AttributeIdOutcome, AttributeId, MetaAnalysisId, AttributeIdQuestion, AttributeIdAnswer);
                    MetaAnalysisModerators = MetaAnalysisModeratorList.GetMetaAnalysisModeratorList();
                }
                connection.Close();
            }
        }
        private void SaveFilterSettings()
        {
            bool settingsSaved = false;
            foreach (MetaAnalysisFilterSetting el in FilterSettingsList)
            {
                if (el.IsDirty == true)
                {
                    settingsSaved = true;
                    MetaAnalysisFilterSetting throwAway = el.Save();
                }
            }
            if (settingsSaved)
            {
                this.FilterSettingsList = MetaAnalysisFilterSettingList.GetMetaAnalysisFilterSettingList(MetaAnalysisId);
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
            //returnValue.LoadProperty<string>(GridSettingsProperty, reader.GetString("GRID_SETTINGS"));
            if (returnValue.AttributeIdAnswer != "")
                returnValue.LoadProperty<string>(AttributeAnswerTextProperty, reader.GetString("ATTRIBUTE_ANSWER_TEXT"));
            else
                returnValue.LoadProperty<string>(AttributeAnswerTextProperty, "");
            if (returnValue.AttributeIdQuestion != "")
                returnValue.LoadProperty<string>(AttributeQuestionTextProperty, reader.GetString("ATTRIBUTE_QUESTION_TEXT"));
            else
                returnValue.LoadProperty<string>(AttributeQuestionTextProperty, "");

            returnValue.LoadProperty<int>(RandomisedProperty, reader.GetInt32("Randomised"));
            returnValue.LoadProperty<string>(RoBCommentProperty, reader.GetString("RoBComment"));
            returnValue.LoadProperty<int>(RoBProperty, reader.GetInt32("RoB"));
            returnValue.LoadProperty<int>(InconProperty, reader.GetInt32("Incon"));
            returnValue.LoadProperty<string>(InconCommentProperty, reader.GetString("InconComment"));
            returnValue.LoadProperty<bool>(RoBSequenceProperty, reader.GetBoolean("RoBSequence"));
            returnValue.LoadProperty<bool>(RoBConcealmentProperty, reader.GetBoolean("RoBConcealment"));
            returnValue.LoadProperty<bool>(RoBBlindingParticipantsProperty, reader.GetBoolean("RoBBlindingParticipants"));
            returnValue.LoadProperty<bool>(RoBBlindingAssessorsProperty, reader.GetBoolean("RoBBlindingAssessors"));
            returnValue.LoadProperty<bool>(RoBIncompleteProperty, reader.GetBoolean("RoBIncomplete"));
            returnValue.LoadProperty<bool>(RoBSelectiveProperty, reader.GetBoolean("RoBSelective"));
            returnValue.LoadProperty<bool>(RoBNoIntentionProperty, reader.GetBoolean("RoBNoIntention"));
            returnValue.LoadProperty<bool>(RoBCarryoverProperty, reader.GetBoolean("RoBCarryover"));
            returnValue.LoadProperty<bool>(RoBStoppedProperty, reader.GetBoolean("RoBStopped"));
            returnValue.LoadProperty<bool>(RoBUnvalidatedProperty, reader.GetBoolean("RoBUnvalidated"));
            returnValue.LoadProperty<bool>(RoBOtherProperty, reader.GetBoolean("RoBOther"));
            returnValue.LoadProperty<bool>(InconPointProperty, reader.GetBoolean("InconPoint"));
            returnValue.LoadProperty<bool>(InconCIsProperty, reader.GetBoolean("InconCIs"));
            returnValue.LoadProperty<bool>(InconDirectionProperty, reader.GetBoolean("InconDirection"));
            returnValue.LoadProperty<bool>(InconStatisticalProperty, reader.GetBoolean("InconStatistical"));
            returnValue.LoadProperty<bool>(InconOtherProperty, reader.GetBoolean("InconOther"));
            returnValue.LoadProperty<int>(IndirectProperty, reader.GetInt32("Indirect"));
            returnValue.LoadProperty<string>(IndirectCommentProperty, reader.GetString("IndirectComment"));
            returnValue.LoadProperty<bool>(IndirectPopulationProperty, reader.GetBoolean("IndirectPopulation"));
            returnValue.LoadProperty<bool>(IndirectOutcomeProperty, reader.GetBoolean("IndirectOutcome"));
            returnValue.LoadProperty<bool>(IndirectNoDirectProperty, reader.GetBoolean("IndirectNoDirect"));
            returnValue.LoadProperty<bool>(IndirectInterventionProperty, reader.GetBoolean("IndirectIntervention"));
            returnValue.LoadProperty<bool>(IndirectTimeProperty, reader.GetBoolean("IndirectTime"));
            returnValue.LoadProperty<bool>(IndirectOtherProperty, reader.GetBoolean("IndirectOther"));
            returnValue.LoadProperty<int>(ImprecProperty, reader.GetInt32("Imprec"));
            returnValue.LoadProperty<string>(ImprecCommentProperty, reader.GetString("ImprecComment"));
            returnValue.LoadProperty<bool>(ImprecWideProperty, reader.GetBoolean("ImprecWide"));
            returnValue.LoadProperty<bool>(ImprecFewProperty, reader.GetBoolean("ImprecFew"));
            returnValue.LoadProperty<bool>(ImprecOnlyOneProperty, reader.GetBoolean("ImprecOnlyOne"));
            returnValue.LoadProperty<bool>(ImprecOtherProperty, reader.GetBoolean("ImprecOther"));
            returnValue.LoadProperty<int>(PubBiasProperty, reader.GetInt32("PubBias"));
            returnValue.LoadProperty<string>(PubBiasCommentProperty, reader.GetString("PubBiasComment"));
            returnValue.LoadProperty<bool>(PubBiasCommerciallyProperty, reader.GetBoolean("PubBiasCommercially"));
            returnValue.LoadProperty<bool>(PubBiasAsymmetricalProperty, reader.GetBoolean("PubBiasAsymmetrical"));
            returnValue.LoadProperty<bool>(PubBiasLimitedProperty, reader.GetBoolean("PubBiasLimited"));
            returnValue.LoadProperty<bool>(PubBiasMissingProperty, reader.GetBoolean("PubBiasMissing"));
            returnValue.LoadProperty<bool>(PubBiasDiscontinuedProperty, reader.GetBoolean("PubBiasDiscontinued"));
            returnValue.LoadProperty<bool>(PubBiasDiscrepancyProperty, reader.GetBoolean("PubBiasDiscrepancy"));
            returnValue.LoadProperty<bool>(PubBiasOtherProperty, reader.GetBoolean("PubBiasOther"));
            returnValue.LoadProperty<string>(UpgradeCommentProperty, reader.GetString("UpgradeComment"));
            returnValue.LoadProperty<bool>(UpgradeLargeProperty, reader.GetBoolean("UpgradeLarge"));
            returnValue.LoadProperty<bool>(UpgradeVeryLargeProperty, reader.GetBoolean("UpgradeVeryLarge"));
            returnValue.LoadProperty<bool>(UpgradeAllPlausibleProperty, reader.GetBoolean("UpgradeAllPlausible"));
            returnValue.LoadProperty<bool>(UpgradeClearProperty, reader.GetBoolean("UpgradeClear"));
            returnValue.LoadProperty<bool>(UpgradeNoneProperty, reader.GetBoolean("UpgradeNone"));
            returnValue.LoadProperty<int>(CertaintyLevelProperty, reader.GetInt32("CertaintyLevel"));
            returnValue.LoadProperty<string>(CertaintyLevelCommentProperty, reader.GetString("CertaintyLevelComment"));
            
            returnValue.LoadProperty<string>(SortedByProperty, reader.GetString("SORTED_FIELD"));
            bool? sortdir = reader.GetValue("SORT_DIRECTION") as bool?;
            if (sortdir == null) returnValue.LoadProperty<string>(SortDirectionProperty, "");
            else if (sortdir == true) returnValue.LoadProperty<string>(SortDirectionProperty, "Ascending");
            else  returnValue.LoadProperty<string>(SortDirectionProperty, "Descending");

            //loading on the fly now, as they take too long if there's a lot of outcomes / meta-analyses
            //returnValue.Outcomes = OutcomeList.GetOutcomeList(returnValue.SetId, returnValue.AttributeIdIntervention, returnValue.AttributeIdControl,
            //    returnValue.AttributeIdOutcome, returnValue.AttributeId, returnValue.MetaAnalysisId, returnValue.AttributeIdQuestion, returnValue.AttributeIdAnswer);

            returnValue.Outcomes = new OutcomeList();//to prevent "null" errors, just in case!

            returnValue.MetaAnalysisModerators = MetaAnalysisModeratorList.GetMetaAnalysisModeratorList();
            returnValue.FilterSettingsList = MetaAnalysisFilterSettingList.GetMetaAnalysisFilterSettingList(returnValue.MetaAnalysisId);

            returnValue.MarkOld();

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