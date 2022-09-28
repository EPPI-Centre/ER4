using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusinessLibrary.BusinessClasses;
using BusinessLibrary.Security;
using Csla;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EPPIDataServices.Helpers;
using BusinessLibrary.BusinessClasses.ImportItems;
using Csla.Core;

namespace ERxWebClient2.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class SourcesController : CSLAController
    {
        
		public SourcesController(ILogger<Controller> logger) : base(logger)
        { }

        [HttpGet("[action]")]
        public IActionResult GetSources()
        {
			try
            {
                if (!SetCSLAUser()) return Unauthorized();
                DataPortal<ReadOnlySourceList> dp = new DataPortal<ReadOnlySourceList>();
                ReadOnlySourceList result = dp.Fetch();

                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogException(e, "GetSources data portal error");
                return StatusCode(500, e.Message);
            }
		}
        [HttpPost("[action]")]
        public IActionResult GetSource([FromBody] SingleIntCriteria SourceId)
        {
            try
            {
                if (!SetCSLAUser()) return Unauthorized();
                DataPortal<Source> dp = new DataPortal<Source>();
                Source result = dp.Fetch(new SingleCriteria<Source, int>(SourceId.Value));
                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogException(e, "GetSource data portal error");
                return StatusCode(500, e.Message);
            }
        }
        [HttpGet("[action]")]
        public IActionResult GetImportFilters()
        {
            try
            {
                if (!SetCSLAUser()) return Unauthorized();
                DataPortal<ReadOnlyImportFilterRuleList> dp = new DataPortal<ReadOnlyImportFilterRuleList>();
                ReadOnlyImportFilterRuleList result = dp.Fetch();
                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogException(e, "GetImportFilters data portal error");
                return StatusCode(500, e.Message);
            }

        }
        [HttpPost("[action]"), RequestSizeLimit(52_428_800)]        
        public IActionResult VerifyFile([FromBody] UploadOrCheckSource incoming)
		{
			try
			{
                if (SetCSLAUser4Writing())
                {
                    int RuleID;
                    FilterRules rules = GetFilterRules(incoming.importFilter, out RuleID);
                    List<ItemIncomingData> FullRes = ImportRefs.Imp(incoming.fileContent, rules);
                    IncomingItemsListJSON res = new IncomingItemsListJSON();
                    res.totalReferences = FullRes.Count;
                    if (FullRes.Count > 100)
                    {//send back only 100 results...
                        res.incomingItems = FullRes.GetRange(0, 100);
                    }
                    else
                    {
                        res.incomingItems = FullRes;
                    }
                    return Ok(res);
                }
                else return Forbid();
            }
			catch (Exception e)
			{
				_logger.LogException(e, "Verify import file before uploading error");
                return StatusCode(500, e.Message);
            }

        }
        [HttpPost("[action]"), RequestSizeLimit(52_428_800)]
        public IActionResult UploadSource([FromBody] UploadOrCheckSource incoming)
		{

			try
			{
                if (SetCSLAUser4Writing())
                {
                    int RuleID;
                    FilterRules rules = GetFilterRules(incoming.importFilter, out RuleID);
                    MobileList<ItemIncomingData> FullRes = ImportRefs.Imp(incoming.fileContent, rules);
                    IncomingItemsListJSON res = new IncomingItemsListJSON();
                    res.totalReferences = FullRes.Count;
                    IncomingItemsList forSaving = new IncomingItemsList();
                    forSaving.FilterID = RuleID;
                    forSaving.SourceName = incoming.source_Name;
                    forSaving.SourceDB = incoming.sourceDataBase;
                    forSaving.DateOfImport = DateTime.Parse(incoming.dateOfImport);
                    forSaving.DateOfSearch = DateTime.Parse(incoming.dateOfSerach);
                    forSaving.Included = true;
                    forSaving.Notes = incoming.notes;
                    forSaving.SearchDescr = incoming.searchDescription;
                    forSaving.SearchStr = incoming.searchString;
                    forSaving.IncomingItems = FullRes;
                    forSaving.buildShortTitles();
                    forSaving = forSaving.Save();
                    return Ok(res);
                }
                else return Forbid();
            }
			catch (Exception e)
			{
				_logger.LogException(e, "Upload import file error");
                return StatusCode(500, e.Message);
            }

		}
        [HttpPost("[action]")]
        public IActionResult UpdateSource([FromBody] JSONSource incoming)
        {

            try
            {
                if (SetCSLAUser4Writing())
                {
                    DataPortal<Source> dp = new DataPortal<Source>();
                    Source res = dp.Fetch(new SingleCriteria<Source, int>(incoming.source_ID));
                    if (res.Source_ID == 0) return NotFound();//Would happen if the sourceID of incoming obj was wrong (does not belong to review) 
                    res.DateOfSerach = DateTime.Parse(incoming.dateOfSerach);
                    res.Notes = incoming.notes;
                    res.SearchDescription = incoming.searchDescription;
                    res.SearchString = incoming.searchString;
                    res.SourceDataBase = incoming.sourceDataBase;
                    res.Source_Name = incoming.source_Name;
                    res = res.Save();
                    return Ok(res);
                }
                else return Forbid();
            }
            catch (Exception e)
            {
				var errMsg = incoming == null ? "incoming data is null" : incoming.ToString();
				_logger.LogError(e, "Upload import file error" + errMsg);
                return StatusCode(500, e.Message);
            }

        }
        [HttpPost("[action]")]
        public IActionResult DeleteUndeleteSource([FromBody] SingleIntCriteria sourceId)
        {
            if (sourceId.Value != -1 && sourceId.Value < 1)
            {//if -1 is valid: delete sourceless items in bulk, anything else doesn't work.
                return NotFound();
            }
            try
            {
                if (SetCSLAUser4Writing())
                {
                    SourceDeleteCommand cmd = new SourceDeleteCommand(sourceId.Value);
                    DataPortal<SourceDeleteCommand> dp = new DataPortal<SourceDeleteCommand>();
                    cmd = dp.Execute(cmd);
                    return Ok(cmd.SourceId);
                }
                else return Forbid();
            }
            catch (Exception e)
            {
                _logger.LogException(e, "DeleteUndeleteSource error");
                return StatusCode(500, e.Message);
            }

        }
        [HttpPost("[action]")]
        public IActionResult DeleteSourceForever([FromBody] SingleIntCriteria sourceId)
        {
            if (sourceId.Value < 1)
            {
                return NotFound();
            }
            try
            {
                if (SetCSLAUser4Writing())
                {
                    //we want extra protections here. The command itself does not check if the source belongs to the review the user is logged on to...
                    DataPortal<ReadOnlySourceList> dp = new DataPortal<ReadOnlySourceList>();
                    ReadOnlySourceList ROSL = dp.Fetch();
                    bool SourceIsInReview = false;
                    foreach (ReadOnlySource src in ROSL.Sources)
                    {
                        if (src.Source_ID == sourceId.Value)
                        {
                            SourceIsInReview = true;
                            break;
                        }
                    }
                    if (SourceIsInReview == false) return NotFound();
                    SourceDeleteForeverCommand cmd = new SourceDeleteForeverCommand(sourceId.Value);
                    DataPortal<SourceDeleteForeverCommand> dp2 = new DataPortal<SourceDeleteForeverCommand>();
                    cmd = dp2.Execute(cmd);
                    return Json(cmd.Result);
                }
                else return Forbid();
            }
            catch (Exception e)
            {
                _logger.LogException(e, "DeleteUndeleteSource error");
                return StatusCode(500, e.Message);
            }

        }

        [HttpPost("[action]")]
        public IActionResult NewPubMedSearchPreview([FromBody] SingleStringCriteria SearchSt)
        {
            //called the first time we run a given search (assumed new search string)
            try
            {
                if (SetCSLAUser4Writing())
                {
                    DataPortal<PubMedSearch> dp = new DataPortal<PubMedSearch>();
                    PubMedSearch res = dp.Fetch(new SingleCriteria<PubMedSearch, string>(SearchSt.Value));
                    //if (FullRes.Count > 100)
                    //{//send back only 100 results...
                    //    res.incomingItems = FullRes.GetRange(0, 100);
                    //}
                    //else
                    //{
                    //    res.incomingItems = FullRes;
                    //}
                    return Ok(res);
                }
                else return Forbid();
            }
            catch (Exception e)
            {
                _logger.LogException(e, "New PubMed Search Preview error");
                return StatusCode(500, e.Message);
            }

        }
        [HttpPost("[action]")]
        public IActionResult ActOnPubMedSearchPreview([FromBody] PubMedSearchJSON PmSearchJSON)
        {
            //called to fetch a different subset of results in existing search OR to save(import) the search
            //difference is in the values of ShowStart/End and SaveStart/End.
            try
            {
                if (SetCSLAUser4Writing())
                {
                    PubMedSearch res = new PubMedSearch();
                    res.QueMax = PmSearchJSON.queMax;
                    res.QueryKey = PmSearchJSON.queryKey;
                    res.QueryStr = PmSearchJSON.queryStr;
                    res.saveEnd = PmSearchJSON.saveEnd;
                    res.saveStart = PmSearchJSON.saveStart;
                    res.showEnd = PmSearchJSON.showEnd;
                    res.showStart = PmSearchJSON.showStart;
                    res.Summary = PmSearchJSON.summary;
                    res.WebEnv = PmSearchJSON.webEnv;
                    res.ItemsList = new IncomingItemsList();
                    res.ItemsList.Included = true;
                    res.ItemsList.SourceName = PmSearchJSON.ItemsList.SourceName;
                    res.ItemsList.SearchDescr = PmSearchJSON.ItemsList.SearchDescr;
                    res.ItemsList.SearchStr = PmSearchJSON.queryStr;
                    res.ItemsList.SourceDB = PmSearchJSON.ItemsList.SourceDB;
                    res.ItemsList.DateOfImport = PmSearchJSON.ItemsList.DateOfImport == new DateTime() ? DateTime.Now : PmSearchJSON.ItemsList.DateOfImport;
                    res.ItemsList.DateOfSearch = PmSearchJSON.ItemsList.DateOfSearch == new DateTime() ? DateTime.Now : PmSearchJSON.ItemsList.DateOfSearch;
                    res = res.Save();
                    return Ok(res);
                }
                else return Forbid();
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Act on PubMed Search (import or fetch specific subset) error");
                return StatusCode(500, e.Message);
            }

        }


        private FilterRules GetFilterRules(string FilterName, out int RuleID)
        {
            RuleID = -1;
            FilterRules rules = new FilterRules();
            DataPortal<ReadOnlyImportFilterRuleList> dp = new DataPortal<ReadOnlyImportFilterRuleList>();
            ReadOnlyImportFilterRuleList roifrList = dp.Fetch();

            ReadOnlyImportFilterRule inRules = new ReadOnlyImportFilterRule();
            foreach (ReadOnlyImportFilterRule rule in roifrList)
            {
                if (rule.RuleName == FilterName)
                {
                    inRules = rule;
                    RuleID = rule.FilterID;
                    break;
                }
            }
            if (inRules.RuleName == "") throw new Exception("Filter name was not found, operation is ABORTED");
            foreach (BusinessLibrary.BusinessClasses.TypeRules tprs in inRules.typeRules)
            {
                rules.typeRules.Add(new BusinessLibrary.BusinessClasses.ImportItems.TypeRules(tprs.Type_ID, tprs.RuleName, tprs.RuleRegexSt));
            }
            foreach (KeyValuePair<int, string> kvp in inRules.typesMap)
            {
                rules.AddTypeDef(kvp.Key, kvp.Value);
            }
            rules.Abstract_Set(inRules.Abstract);
            rules.author_Set(inRules.Author);
            rules.Availability_Set(inRules.Availability);
            rules.City_Set(inRules.City);
            rules.date_Set(inRules.Date);
            rules.DefaultTypeCode = inRules.DefaultTypeCode;
            rules.Edition_Set(inRules.Edition);
            rules.EndPage_Set(inRules.EndPage);
            rules.Institution_Set(inRules.Institution);
            rules.Issue_Set(inRules.Issue);
            rules.month_Set(inRules.Month);
            rules.Notes_Set(inRules.Notes);
            rules.OldItemID_Set(inRules.OldItemId);
            rules.Pages_Set(inRules.Pages);
            rules.pAuthor_Set(inRules.ParentAuthor);
            rules.pTitle_Set(inRules.pTitle);
            rules.Publisher_Set(inRules.Publisher);
            rules.shortTitle_Set(inRules.shortTitle);
            rules.StandardN_Set(inRules.StandardN);
            rules.startOfNewField_Set(inRules.StartOfNewField);
            rules.startOfNewRec_Set(inRules.StartOfNewRec);
            rules.StartPage_Set(inRules.StartPage);
            rules.title_Set(inRules.Title);
            rules.typeField_Set(inRules.typeField);
            rules.Url_Set(inRules.Url);
            rules.Volume_Set(inRules.Volume);
            rules.DOI_Set(inRules.DOI);
            rules.Keywords_Set(inRules.Keywords);
            return rules;
        }

    }
    public class JSONSource
    {
        public string source_Name = "";
        public string dateOfSerach = DateTime.Now.ToLongDateString();
        public string dateOfImport = DateTime.Now.ToLongDateString();
        public string sourceDataBase = "";
        public string searchDescription = "";
        public string searchString = "";
        public string notes = "";
        public string importFilter = "";
        public int total_Items = 0;
        public int deleted_Items = 0;
        public int source_ID = -1;
        public bool isFlagDeleted = false;
        public int codes = 0;
        public int inductiveCodes = 0;
        public int attachedFiles = 0;
        public int duplicates = 0;
        public int isMasterOf = 0;
        public int outcomes = 0;
    }
    public class UploadOrCheckSource : JSONSource
    {
        //public string FilterName = "";
        //public string Source_Name = "";
        //public int Total_Items = 0;
        //public int Deleted_Items = 0;
        //public int Duplicates = 0;
        //public int Source_ID = 0;
        //public bool IsDeleted = false;
        public string fileContent = "";
        
    }
    public class IncomingItemsListJSON
    {
        public int totalReferences = 0;
        public List<ItemIncomingData> incomingItems = new List<ItemIncomingData>();
    }

    public class PubMedSearchJSON
    {
        public string queryStr = "";
        public string webEnv = "";
        public int queMax = 0;
        public int showStart = 0;
        public int showEnd = 0;
        public int saveStart = 0;
        public int saveEnd = 0;
        public string summary = "";
        //public MobileList<string> SavedIndexes = null;
        public IncomingItemsList ItemsList = new IncomingItemsList();
        public int queryKey = 0;
    }

//    public string Source_Name
//public DateTime DateOfSerach
//public DateTime DateOfImport
//public string SourceDataBase
//public string SearchDescription
//public string SearchString
//public string Notes
//public string ImportFilter
//public int Total_Items
//public int Deleted_Items
//public int Source_ID
//public bool IsFlagDeleted
//public int Codes
//public int InductiveCodes
//public int AttachedFiles
//public int Duplicates
//public int isMasterOf
//public int Outcomes
}

