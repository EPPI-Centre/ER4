using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using BusinessLibrary.BusinessClasses;
using BusinessLibrary.Security;
using Csla;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EPPIDataServices.Helpers;
using Newtonsoft.Json;

namespace ERxWebClient2.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class MetaAnalysisController : CSLAController
    {

        public MetaAnalysisController(ILogger<FrequenciesController> logger) : base(logger)
        { }

        [HttpGet("[action]")]
        public IActionResult GetMAList()
        {
            try
            {
                if (!SetCSLAUser()) return Unauthorized();
                MetaAnalysisList result = DataPortal.Fetch<MetaAnalysisList>();
                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogException(e, "GetMAList data portal error");
                return StatusCode(500, e.Message);
            }

        }
        [HttpPost("[action]")]
        public IActionResult FetchMetaAnalysis([FromBody] MetaAnalysisSelectionCritJSON crit)
        {
            try
            {
                if (!SetCSLAUser()) return Unauthorized();
                MetaAnalysis result = DataPortal.Fetch<MetaAnalysis>(crit.ToCSLACirteria());
                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogException(e, "GetMetaAnalysis data portal error");
                return StatusCode(500, e.Message);
            }

        }
        [HttpGet("[action]")]
        public IActionResult FetchEmptyMetaAnalysis()
        {
            try
            {
                if (!SetCSLAUser()) return Unauthorized();
                MetaAnalysis result = MetaAnalysis.CreateNewMAWithAllChildren();
                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogException(e, "GetMetaAnalysis data portal error");
                return StatusCode(500, e.Message);
            }

        }
        [HttpPost("[action]")]
        public IActionResult SaveMetaAnalysis([FromBody] MetaAnalysisJSON MAjson)
        {

            try
            {
                if (MAjson == null)
                {
                    _logger.LogError("SaveMetaAnalysis error, input object is null");
                    return StatusCode(500, "Invalid input: not recognised or null MA object");
                }
                if (SetCSLAUser4Writing())
                {


                    MetaAnalysisSelectionCrit crit = new MetaAnalysisSelectionCrit();

                    MetaAnalysis toSave ;
                    if (MAjson.metaAnalysisId > 0)
                    {//existing MA, we can fetch it from the DB
                        crit.GetAllDetails = false;
                        crit.MetaAnalysisId = MAjson.metaAnalysisId;
                        toSave = DataPortal.Fetch<MetaAnalysis>(crit);
                        toSave.Outcomes = OutcomeList.GetOutcomeList(toSave.SetId, toSave.AttributeIdIntervention, toSave.AttributeIdControl,
                            toSave.AttributeIdOutcome, toSave.AttributeId, toSave.MetaAnalysisId, toSave.AttributeIdQuestion, toSave.AttributeIdAnswer);
                    }
                    else
                    {//new MA, will do it with an empty MA
                        toSave = MetaAnalysis.CreateNewMAWithAllChildren();
                    }

                    //next, we need to set the MA typeID nice and early, as that determines what Outcomes CAN be "selected" and saved in TB_META_ANALYSIS_OUTCOME
                    toSave.MetaAnalysisTypeId = MAjson.metaAnalysisTypeId;
                    toSave.Outcomes.SetMetaAnalysisType(toSave.MetaAnalysisTypeId);
                    //now we need to pick all the "selected" outcomes, which requires a bit of work
                    foreach (MiniOutcomeForMAsJSON OJ in MAjson.outcomes)
                    {
                        Outcome? outcome = toSave.Outcomes.FirstOrDefault(f => f.OutcomeId == OJ.outcomeId);
                        if (outcome != null) outcome.IsSelected = OJ.isSelected;                        
                    }
                    //similar for filters...
                    List<MetaAnalysisFilterSetting> toSaveSettings = new List<MetaAnalysisFilterSetting>();
                    foreach (FiltersettingsJSON FsJ in MAjson.filterSettingsList)
                    {
                        if (FsJ.metaAnalysisFilterSettingId == 0)
                        {//it's a newly created filter
                            MetaAnalysisFilterSetting newF = new MetaAnalysisFilterSetting(toSave.MetaAnalysisId);
                            newF.ColumnName = FsJ.columnName;
                            newF.Filter1 = FsJ.filter1;
                            newF.Filter1CaseSensitive = FsJ.filter1CaseSensitive;
                            newF.Filter1Operator = FsJ.filter1Operator;
                            newF.Filter2 = FsJ.filter2;
                            newF.Filter2CaseSensitive = FsJ.filter2CaseSensitive;
                            newF.Filter2Operator = FsJ.filter2Operator;
                            newF.FiltersLogicalOperator = FsJ.filtersLogicalOperator;
                            //newF.MetaAnalysisId = toSave.MetaAnalysisId;
                            newF.SelectedValues = FsJ.selectedValues;
                            if (!newF.IsClear) toSaveSettings.Add(newF);//don't add an empty filter!
                        }
                        else
                        {//not a new setting, so we need to find it in the current list
                            MetaAnalysisFilterSetting? toChangeSett = toSave.FilterSettingsList.FirstOrDefault(f => f.MetaAnalysisFilterSettingId == FsJ.metaAnalysisFilterSettingId);
                            if (toChangeSett != null)
                            {
                                toChangeSett.ColumnName = FsJ.columnName;
                                toChangeSett.Filter1 = FsJ.filter1;
                                toChangeSett.Filter1CaseSensitive = FsJ.filter1CaseSensitive;
                                toChangeSett.Filter1Operator = FsJ.filter1Operator;
                                toChangeSett.Filter2 = FsJ.filter2;
                                toChangeSett.Filter2CaseSensitive = FsJ.filter2CaseSensitive;
                                toChangeSett.Filter2Operator = FsJ.filter2Operator;
                                toChangeSett.FiltersLogicalOperator = FsJ.filtersLogicalOperator;
                                toChangeSett.SelectedValues = FsJ.selectedValues;
                                if (toChangeSett.IsClear) toChangeSett.Delete();
                                toSaveSettings.Add(toChangeSett);
                            }
                        }
                    }
                    toSave.FilterSettingsList.Clear();
                    toSave.FilterSettingsList.AddRange(toSaveSettings);
                    toSave.Title = MAjson.title;
                    toSave.MetaAnalysisTypeTitle = MAjson.metaAnalysisTypeTitle;
                    toSave.SortDirection = MAjson.sortDirection;
                    toSave.SortedBy = MAjson.sortedBy;
                    toSave.AttributeIdAnswer = MAjson.attributeIdAnswer;
                    toSave.AttributeIdQuestion = MAjson.attributeIdQuestion;
                    toSave = toSave.Save();
                    crit.GetAllDetails = true;
                    if (crit.MetaAnalysisId < 1) crit.MetaAnalysisId = toSave.MetaAnalysisId;
                    //we need to re-fetch the whole thing, because we re-bind everything on the UI, so we have to send back 100% up-to-date data...
                    toSave = DataPortal.Fetch<MetaAnalysis>(crit);
                    
                    return Ok(toSave);

                }
                else return Forbid();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "SaveMetaAnalysis error, for MA ID: {0}", MAjson.metaAnalysisId);
                    //,  JsonConvert.SerializeObject(comparisonAttributesCriteria));
                return StatusCode(500, e.Message);
            }
        }
        [HttpPost("[action]")]
        public IActionResult DeleteMetaAnalysis([FromBody] MetaAnalysisSelectionCritJSON crit)
        {
            try
            {
                if (SetCSLAUser4Writing())
                {
                    MetaAnalysisSelectionCrit crit2 = crit.ToCSLACirteria();
                    MetaAnalysis toDelete = DataPortal.Fetch<MetaAnalysis>(crit2);
                    toDelete.Delete();
                    toDelete = toDelete.Save();
                    return Ok();
                }
                else return Forbid();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "DeleteMetaAnalysis error, for MA ID: {0}", crit.MetaAnalysisId.ToString());
                return StatusCode(500, e.Message);
            }
        }
        [HttpPost("[action]")]
        public IActionResult RunMetaAnalysis([FromBody] MetaAnalysisJSON MAjson)
        {
            try
            {
                if (MAjson == null)
                {
                    _logger.LogError("RunMetaAnalysis error, input object is null");
                    return StatusCode(500, "Invalid input: not recognised or null MA object");
                }
                if (SetCSLAUser())
                {
                    MetaAnalysisSelectionCrit crit = new MetaAnalysisSelectionCrit();
                    crit.GetAllDetails = true;
                    crit.MetaAnalysisId = MAjson.metaAnalysisId;
                    MetaAnalysis toRun = DataPortal.Fetch<MetaAnalysis>(crit);
                    
                    toRun.MetaAnalysisTypeId = MAjson.metaAnalysisTypeId;
                    toRun.Outcomes.SetMetaAnalysisType(toRun.MetaAnalysisTypeId);
                    //now we need to pick all the "selected" outcomes
                    foreach (MiniOutcomeForMAsJSON OJ in MAjson.outcomes)
                    {
                        Outcome? outcome = toRun.Outcomes.FirstOrDefault(f => f.OutcomeId == OJ.outcomeId);
                        if (outcome != null) outcome.IsSelected = OJ.isSelected;
                    }

                    foreach(MetaAnalysisModeratorJSON modJ in MAjson.metaAnalysisModerators)
                    {
                        MetaAnalysisModerator? mod = toRun.MetaAnalysisModerators.FirstOrDefault(f => f.Name == modJ.name && f.FieldName == modJ.fieldName);
                        if (mod !=  null)
                        {
                            mod.IsSelected = modJ.isSelected;
                            mod.IsFactor = modJ.isFactor;
                            mod.Reference = modJ.reference;
                        }

                    }

                    //all "options for Running MA" don't get saved, so we need to set them from the data received
                    toRun.StatisticalModel = MAjson.statisticalModel;
                    toRun.SignificanceLevel = MAjson.significanceLevel;
                    toRun.Verbose = MAjson.verbose;
                    toRun.DecPlaces = MAjson.decPlaces;
                    toRun.RankCorr = MAjson.rankCorr;
                    toRun.TrimFill = MAjson.trimFill;
                    toRun.Egger = MAjson.egger;
                    toRun.KNHA = MAjson.knha;
                    toRun.FitStats = MAjson.fitStats;
                    toRun.Confint = MAjson.confint;
                    toRun.XAxisTitle = MAjson.xAxisTitle;
                    toRun.SummaryEstimateTitle = MAjson.summaryEstimateTitle;
                    toRun.ShowAnnotations = MAjson.showAnnotations;
                    toRun.ShowAnnotationWeights = MAjson.showAnnotationWeights;
                    toRun.FittedVals = MAjson.fittedVals;
                    toRun.CredInt = MAjson.credInt;
                    toRun.ShowBoxplot = MAjson.showBoxplot;
                    toRun.ShowFunnel = MAjson.showFunnel;
                    toRun.NMAReference = MAjson.nmaReference;
                    toRun.NMAStatisticalModel = MAjson.nmaStatisticalModel;
                    toRun.LargeValuesGood = MAjson.largeValuesGood;

                    toRun.AnalysisType = MAjson.analysisType;

                    MetaAnalysisRunInRCommand RrunCmd = new MetaAnalysisRunInRCommand();
                    RrunCmd.MetaAnalaysisObject = toRun;
                    RrunCmd = DataPortal.Execute<MetaAnalysisRunInRCommand>(RrunCmd);
                    return Ok(RrunCmd);
                }
                else return Forbid();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "RunMetaAnalysis error, for MA ID: {0}", MAjson.metaAnalysisId.ToString());
                if (e.Message == "DataPortal.Update failed (missing value where TRUE/FALSE needed)")
                {
                    string Msg = "Possible inconsistency in your outcomes data. Alternatively, this combination of outcomes (especially re multiple outcomes for the same study) is not (yet) supported. Please consider exporting your data and running your analysis elsewhere.";
                    return StatusCode(500, Msg);
                }
                return StatusCode(500, e.Message);
            }
        }

    }
    public class MetaAnalysisSelectionCritJSON
    {
        public bool GetAllDetails { get; set; }

        public int MetaAnalysisId { get; set; }
        public MetaAnalysisSelectionCrit ToCSLACirteria()
        {
            MetaAnalysisSelectionCrit res = new MetaAnalysisSelectionCrit();
            res.GetAllDetails = GetAllDetails;
            res.MetaAnalysisId = MetaAnalysisId;
            return res;
        }
    }
    public class MetaAnalysisJSON
    {
        public int analysisType { get; set; }
        public string title { get; set; }
        public bool knha { get; set; }
        public bool fitStats { get; set; }
        public bool confint { get; set; }
        public bool egger { get; set; }
        public bool rankCorr { get; set; }
        public bool trimFill { get; set; }
        public int statisticalModel { get; set; }
        public int verbose { get; set; }
        public int significanceLevel { get; set; }
        public int decPlaces { get; set; }
        public string xAxisTitle { get; set; }
        public string summaryEstimateTitle { get; set; }
        public bool showAnnotations { get; set; }
        public bool showAnnotationWeights { get; set; }
        public bool fittedVals { get; set; }
        public bool credInt { get; set; }
        public bool showFunnel { get; set; }
        public bool showBoxplot { get; set; }
        public string sortedBy { get; set; }
        public string sortDirection { get; set; }
        public int nmaStatisticalModel { get; set; }
        public bool largeValuesGood { get; set; }
        public string nmaReference { get; set; }
        public bool exponentiated { get; set; }
        public bool allTreatments { get; set; }
        public int metaAnalysisId { get; set; }
        public int attributeId { get; set; }
        public int setId { get; set; }
        public int attributeIdIntervention { get; set; }
        public int attributeIdControl { get; set; }
        public int attributeIdOutcome { get; set; }
        public int randomised { get; set; }
        public int roB { get; set; }
        public int incon { get; set; }
        public int indirect { get; set; }
        public int imprec { get; set; }
        public int pubBias { get; set; }
        public int certaintyLevel { get; set; }
        public string roBComment { get; set; }
        public bool roBSequence { get; set; }
        public bool roBConcealment { get; set; }
        public bool roBBlindingParticipants { get; set; }
        public bool roBBlindingAssessors { get; set; }
        public bool roBIncomplete { get; set; }
        public bool roBSelective { get; set; }
        public bool roBNoIntention { get; set; }
        public bool roBCarryover { get; set; }
        public bool roBStopped { get; set; }
        public bool roBUnvalidated { get; set; }
        public bool roBOther { get; set; }
        public string inconComment { get; set; }
        public bool inconPoint { get; set; }
        public bool inconCIs { get; set; }
        public bool inconDirection { get; set; }
        public bool inconStatistical { get; set; }
        public bool inconOther { get; set; }
        public string indirectComment { get; set; }
        public bool indirectPopulation { get; set; }
        public bool indirectOutcome { get; set; }
        public bool indirectNoDirect { get; set; }
        public bool indirectIntervention { get; set; }
        public bool indirectTime { get; set; }
        public bool indirectOther { get; set; }
        public string imprecComment { get; set; }
        public bool imprecWide { get; set; }
        public bool imprecFew { get; set; }
        public bool imprecOnlyOne { get; set; }
        public bool imprecOther { get; set; }
        public string pubBiasComment { get; set; }
        public bool pubBiasCommercially { get; set; }
        public bool pubBiasAsymmetrical { get; set; }
        public bool pubBiasLimited { get; set; }
        public bool pubBiasMissing { get; set; }
        public bool pubBiasDiscontinued { get; set; }
        public bool pubBiasDiscrepancy { get; set; }
        public bool pubBiasOther { get; set; }
        public string upgradeComment { get; set; }
        public bool upgradeLarge { get; set; }
        public bool upgradeVeryLarge { get; set; }
        public bool upgradeAllPlausible { get; set; }
        public bool upgradeClear { get; set; }
        public bool upgradeNone { get; set; }
        public string certaintyLevelComment { get; set; }
        public int metaAnalysisTypeId { get; set; }
        public string metaAnalysisTypeTitle { get; set; }
        public string interventionText { get; set; }
        public string controlText { get; set; }
        public string outcomeText { get; set; }
        public string attributeIdQuestion { get; set; }
        public string attributeQuestionText { get; set; }
        public string attributeIdAnswer { get; set; }
        public string attributeAnswerText { get; set; }
        public string gridSettings { get; set; }
        public object feForestPlot { get; set; }
        public object reForestPlot { get; set; }
        public object feFunnelPlot { get; set; }
        public int feSumWeight { get; set; }
        public int reSumWeight { get; set; }
        public int feEffect { get; set; }
        public int feSE { get; set; }
        public int feCiUpper { get; set; }
        public int feCiLower { get; set; }
        public int reEffect { get; set; }
        public int reSE { get; set; }
        public int reCiUpper { get; set; }
        public int reCiLower { get; set; }
        public int tauSquared { get; set; }
        public int q { get; set; }
        public int reQ { get; set; }
        public int numStudies { get; set; }
        public int fileDrawerZ { get; set; }
        public int sumWeightsSquared { get; set; }
        public int reSumWeightsTimesOutcome { get; set; }
        public int wY_squared { get; set; }



        public FiltersettingsJSON[] filterSettingsList { get; set; } = new FiltersettingsJSON[0];
        public MiniOutcomeForMAsJSON[] outcomes { get; set; } = new MiniOutcomeForMAsJSON[0];
        //public MetaAnalysisModeratorJSON[] metaAnalysisModerators { get; set; } = new MetaAnalysisModeratorJSON[0];
        public MetaAnalysisModeratorJSON[] metaAnalysisModerators { get; set; } = new MetaAnalysisModeratorJSON[0];
    }


    public class FiltersettingsJSON
    {
        public bool isClear { get; set; }
        public int metaAnalysisFilterSettingId { get; set; }
        public int metaAnalysisId { get; set; }
        public string columnName { get; set; }
        public string selectedValues { get; set; }
        public string filter1 { get; set; }
        public string filter1Operator { get; set; }
        public bool filter1CaseSensitive { get; set; }
        public string filtersLogicalOperator { get; set; }
        public string filter2 { get; set; }
        public string filter2Operator { get; set; }
        public bool filter2CaseSensitive { get; set; }
    }

   
    public class OutcomeCodesJSON
    {
        public OutcomeItemAttributesListJSON[] outcomeItemAttributesList { get; set; } = new OutcomeItemAttributesListJSON[0];
    }

    public class OutcomeItemAttributesListJSON
    {
        public int outcomeItemAttributeId { get; set; }
        public int outcomeId { get; set; }
        public int attributeId { get; set; }
        public string additionalText { get; set; }
        public string attributeName { get; set; }
    }

    public class MiniOutcomeForMAsJSON
    {
        public int outcomeId { get; set; } = -1;
        //public int itemSetId { get; set; } = -1;
        //public int outcomeTypeId { get; set; } = -1;
        //public string outcomeTypeName { get; set; } = "";
        //public int itemAttributeIdIntervention { get; set; } = -1;
        //public int itemAttributeIdControl { get; set; } = -1;
        //public int itemAttributeIdOutcome { get; set; } = -1;
        //public string title { get; set; } = "";
        //public string shortTitle { get; set; } = "";
        //public string outcomeDescription { get; set; } = "";
        //public double data1 { get; set; } = 0;
        //public double data2 { get; set; } = 0;
        //public double data3 { get; set; } = 0;
        //public double data4 { get; set; } = 0;
        //public double data5 { get; set; } = 0;
        //public double data6 { get; set; } = 0;
        //public double data7 { get; set; } = 0;
        //public double data8 { get; set; } = 0;
        //public double data9 { get; set; } = 0;
        //public double data10 { get; set; } = 0;
        //public double data11 { get; set; } = 0;
        //public double data12 { get; set; } = 0;
        //public double data13 { get; set; } = 0;
        //public double data14 { get; set; } = 0;
        //public string interventionText { get; set; } = "";
        //public string controlText { get; set; } = "";
        //public string outcomeText { get; set; } = "";
        //public int itemTimepointId { get; set; } = 0;
        //public string itemTimepointMetric { get; set; } = "";
        //public string itemTimepointValue { get; set; } = "";
        //public int itemArmIdGrp1 { get; set; } = -1;
        //public int itemArmIdGrp2 { get; set; } = -1;
        //public string timepointDisplayValue { get; set; } = "";
        //public string grp1ArmName { get; set; } = "";
        //public string grp2ArmName { get; set; } = "";
        public bool isSelected { get; set; }
        //public bool canSelect { get; set; }
        //public OutcomeCodesJSON? outcomeCodes { get; set; }
        //public int occ1 { get; set; } = -1;
        //public int occ2 { get; set; } = -1;
        //public int occ3 { get; set; } = -1;
        //public int occ4 { get; set; } = -1;
        //public int occ5 { get; set; } = -1;
        //public int occ6 { get; set; } = -1;
        //public int occ7 { get; set; } = -1;
        //public int occ8 { get; set; } = -1;
        //public int occ9 { get; set; } = -1;
        //public int occ10 { get; set; } = -1;
        //public int occ11 { get; set; } = -1;
        //public int occ12 { get; set; } = -1;
        //public int occ13 { get; set; } = -1;
        //public int occ14 { get; set; } = -1;
        //public int occ15 { get; set; } = -1;
        //public int occ16 { get; set; } = -1;
        //public int occ17 { get; set; } = -1;
        //public int occ18 { get; set; } = -1;
        //public int occ19 { get; set; } = -1;
        //public int occ20 { get; set; } = -1;
        //public int occ21 { get; set; } = -1;
        //public int occ22 { get; set; } = -1;
        //public int occ23 { get; set; } = -1;
        //public int occ24 { get; set; } = -1;
        //public int occ25 { get; set; } = -1;
        //public int occ26 { get; set; } = -1;
        //public int occ27 { get; set; } = -1;
        //public int occ28 { get; set; } = -1;
        //public int occ29 { get; set; } = -1;
        //public int occ30 { get; set; } = -1;
        //public int aa1 { get; set; } = -1;
        //public int aa2 { get; set; } = -1;
        //public int aa3 { get; set; } = -1;
        //public int aa4 { get; set; } = -1;
        //public int aa5 { get; set; } = -1;
        //public int aa6 { get; set; } = -1;
        //public int aa7 { get; set; } = -1;
        //public int aa8 { get; set; } = -1;
        //public int aa9 { get; set; } = -1;
        //public int aa10 { get; set; } = -1;
        //public int aa11 { get; set; } = -1;
        //public int aa12 { get; set; } = -1;
        //public int aa13 { get; set; } = -1;
        //public int aa14 { get; set; } = -1;
        //public int aa15 { get; set; } = -1;
        //public int aa16 { get; set; } = -1;
        //public int aa17 { get; set; } = -1;
        //public int aa18 { get; set; } = -1;
        //public int aa19 { get; set; } = -1;
        //public int aa20 { get; set; } = -1;
        //public string aq1 { get; set; } = "";
        //public string aq2 { get; set; } = "";
        //public string aq3 { get; set; } = "";
        //public string aq4 { get; set; } = "";
        //public string aq5 { get; set; } = "";
        //public string aq6 { get; set; } = "";
        //public string aq7 { get; set; } = "";
        //public string aq8 { get; set; } = "";
        //public string aq9 { get; set; } = "";
        //public string aq10 { get; set; } = "";
        //public string aq11 { get; set; } = "";
        //public string aq12 { get; set; } = "";
        //public string aq13 { get; set; } = "";
        //public string aq14 { get; set; } = "";
        //public string aq15 { get; set; } = "";
        //public string aq16 { get; set; } = "";
        //public string aq17 { get; set; } = "";
        //public string aq18 { get; set; } = "";
        //public string aq19 { get; set; } = "";
        //public string aq20 { get; set; } = "";

        //public double feWeight { get; set; } = 0;
        //public double reWeight { get; set; } = 0;
        //public double smd { get; set; } = 0;
        //public double sesmd { get; set; } = 0;

        //public double r { get; set; } = 0;
        //public double ser { get; set; } = 0;
        //public double oddsRatio { get; set; } = 0;
        //public double seOddsRatio { get; set; } = 0;
        //public double riskRatio { get; set; } = 0;
        //public double seRiskRatio { get; set; } = 0;
        //public double ciUpperSMD { get; set; } = 0;
        //public double ciLowerSMD { get; set; } = 0;
        //public double ciUpperR { get; set; } = 0;
        //public double ciLowerR { get; set; } = 0;


        //public double ciUpperOddsRatio { get; set; } = 0;
        //public double ciLowerOddsRatio { get; set; } = 0;
        //public double ciUpperRiskRatio { get; set; } = 0;
        //public double ciLowerRiskRatio { get; set; } = 0;
        //public double ciUpperRiskDifference { get; set; } = 0;
        //public double ciLowerRiskDifference { get; set; } = 0;
        //public double ciUpperPetoOddsRatio { get; set; } = 0;
        //public double ciLowerPetoOddsRatio { get; set; } = 0;
        //public double ciUpperMeanDifference { get; set; } = 0;
        //public double ciLowerMeanDifference { get; set; } = 0;
        //public double riskDifference { get; set; } = 0;
        //public double seRiskDifference { get; set; } = 0;
        //public double meanDifference { get; set; } = 0;
        //public double seMeanDifference { get; set; } = 0;
        //public double petoOR { get; set; } = 0;
        //public double sePetoOR { get; set; } = 0;
        //public double es { get; set; } = 0;
        //public double sees { get; set; } = 0;
        //public int nRows { get; set; } = 0;
        //public double ciLower { get; set; } = 0;
        //public double ciUpper { get; set; } = 0;
        //public string esDesc { get; set; } = "";
        //public string seDesc { get; set; } = "";
        //public string data1Desc { get; set; } = "";
        //public string data2Desc { get; set; } = "";
        //public string data3Desc { get; set; } = "";
        //public string data4Desc { get; set; } = "";
        //public string data5Desc { get; set; } = "";
        //public string data6Desc { get; set; } = "";
        //public string data7Desc { get; set; } = "";
        //public string data8Desc { get; set; } = "";
        //public string data9Desc { get; set; } = "";
        //public string data10Desc { get; set; } = "";
        //public string data11Desc { get; set; } = "";
        //public string data12Desc { get; set; } = "";
        //public string data13Desc { get; set; } = "";
        //public string data14Desc { get; set; } = "";
    }

    public class ReferenceJSON
    {
        public string name { get; set; }
        public int attributeID { get; set; }
    }

    public class MetaAnalysisModeratorJSON
    {
        public string name { get; set; }
        public string fieldName { get; set; }
        public int attributeID { get; set; }
        public string reference { get; set; }
        public ReferenceJSON[] references { get; set; } = new ReferenceJSON[0];
        public bool isSelected { get; set; }
        public bool isFactor { get; set; }
    }
}



