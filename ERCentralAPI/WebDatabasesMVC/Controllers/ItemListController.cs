﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using BusinessLibrary.BusinessClasses;
using Csla;
using EPPIDataServices.Helpers;
using ERxWebClient2.Controllers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using WebDatabasesMVC;
using WebDatabasesMVC.ViewModels;
/// <summary>
/// This is the first controller to use the agreed approach, see the Index() method.
/// This serves a view (and HTML page) using the ItemList data obtained by the internal method ItemList() which is where data is fetched.
/// Concurrently a second method IndexJSON() uses the same internal method ItemList() to fetch the data and, instead, return a JSON version of the object.
/// The logic to set the CSLA user is in the public method, as the same controller might need to fetch multiple CSLA objects.
/// The logic to fetch data is in the "internal" method.
/// We should use this pattern always, and create a "clean" public method that returns just the fetched object (as JSON) for each private method that returns a CSLA object.
/// On contrast, when a view requires multiple BOs we'll create (usually) separate methods (internal) to fetch each object. 
/// </summary>
namespace WebDatabasesMVC.Controllers
{
    [Authorize(AuthenticationSchemes = "CookieAuthentication")]
    public class ItemListController : CSLAController
    {
        
        public ItemListController(ILogger<ItemListController> logger) : base(logger)
        {}


        public IActionResult Index()
        {
            try
            {
                if (SetCSLAUser())
                {
                    SelectionCriteria crit = new SelectionCriteria();
                    crit.Description = "Listing all items";
                    ItemListWithCriteria iList = GetItemList(crit);
                    return View(iList);
                }
                else return Unauthorized();
            } 
            catch (Exception e)
            {
                _logger.LogError(e, "Error in ItemList Index");
                return StatusCode(500, e.Message);
            }
        }
        public IActionResult IndexJSON()
        {
            try
            {
                if (SetCSLAUser())
                {
                    SelectionCriteria crit = new SelectionCriteria();
                    crit.Description = "Listing all items";
                    ItemListWithCriteria iList = GetItemList(crit);
                    return Json(iList);
                }
                else return Unauthorized();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error in ItemList IndexJSON");
                return StatusCode(500, e.Message);
            }
        }
        public IActionResult Page([FromForm] int PageN)
        {
            try
            {
                if (SetCSLAUser())
                {
                    SelectionCriteria crit = new SelectionCriteria();
                    crit.Description = "Listing all items";
                    crit.PageNumber = PageN;
                    ItemListWithCriteria iList = GetItemList(crit);
                    return View("Index", iList);//supplying the view name, otherwise MVC would try to auto-discover a view called Page.
                }
                else return Unauthorized();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error in ItemList PageN");
                return StatusCode(500, e.Message);
            }
        }
        public IActionResult PageJSON([FromForm] int PageN)
        {
            try
            {
                if (SetCSLAUser())
                {
                    SelectionCriteria crit = new SelectionCriteria();
                    crit.Description = "Listing all items";
                    crit.PageNumber = PageN;
                    ItemListWithCriteria iList = GetItemList(crit);
                    return Json(iList);
                }
                else return Unauthorized();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error in ItemList PageJSON");
                return StatusCode(500, e.Message);
            }
        }
        [HttpPost]
        public IActionResult ListFromCrit(SelCritMVC critMVC)
        {
            try
            {
                if (SetCSLAUser())
                {
                    SelectionCriteria crit = critMVC.CSLACriteria();
                    ItemListWithCriteria iList = GetItemList(crit);
                    return View("Index", iList);//supplying the view name, otherwise MVC would try to auto-discover a view called Page.
                }
                else return Unauthorized();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error in ItemList ListFromCrit");
                return StatusCode(500, e.Message);
            }
        }
        [HttpPost]
        public IActionResult ListFromCritJson(SelCritMVC critMVC)
        {
            return internalListFromCritJson(critMVC);
        }
        [Authorize(AuthenticationSchemes = "FairAuthentication")]
        [HttpPost]
        public IActionResult FairListFromCritJson(SelCritMVC critMVC)
        {
            return internalListFromCritJson(critMVC);
        }
        private IActionResult internalListFromCritJson(SelCritMVC critMVC)
        {
            try
            {
                if (SetCSLAUser())
                {
                    SelectionCriteria crit = critMVC.CSLACriteria();
                    ItemListWithCriteria iList = GetItemList(crit);
                    var res = Json(iList, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                    return res;// Json(iList, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                }
                else return Unauthorized();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error in ItemList ListFromCritJson");
                return StatusCode(500, e.Message);
            }
        }
        public IActionResult GetFreqList([FromForm] long attId, string attName)
        {
            try
            {
                if (SetCSLAUser())
                {
                    SelectionCriteria crit = new SelectionCriteria();
                    crit.ListType = "WebDbWithThisCode";
                    crit.FilterAttributeId = attId;
                    //crit.Description = "Listing items with code: " + attName;
                    crit.Description = attName;
                    ItemListWithCriteria iList = GetItemList(crit);

                    // log to TB_WEBDB_LOG
                    logActivity("GetItemList", crit.Description);

                    return View("Index", iList);//supplying the view name, otherwise MVC would try to auto-discover a view called Page.
                }
                else return Unauthorized();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error in ItemList GetFreqList");
                return StatusCode(500, e.Message);
            }
        }
        [Authorize(AuthenticationSchemes = "VawgAuthentication")]
        [Authorize(AuthenticationSchemes = "CookieAuthentication")]
        public IActionResult GetFreqListJSon([FromForm] long attId, string attName)
        {
            return InternalGetFreqListJSon(attId, attName);
        }
        [Authorize(AuthenticationSchemes = "FairAuthentication")]
        public IActionResult FairGetFreqListJSon([FromForm] long attId, string attName)
        {
            return InternalGetFreqListJSon(attId, attName);
        }
        private IActionResult InternalGetFreqListJSon(long attId, string attName)
        {
            try
            {
                if (SetCSLAUser())
                {
                    SelectionCriteria crit = new SelectionCriteria();
                    crit.ListType = "WebDbWithThisCode";
                    //crit.Description = "Listing items with code: " + attName;
                    crit.Description = attName;
                    crit.FilterAttributeId = attId;
                    ItemListWithCriteria iList = GetItemList(crit);

                    // log to TB_WEBDB_LOG
                    logActivity("GetItemList", crit.Description);

                    return Json(iList);//supplying the view name, otherwise MVC would try to auto-discover a view called Page.
                }
                else return Unauthorized();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error in ItemList GetFreqListJSon");
                return StatusCode(500, e.Message);
            }
        }

        [HttpPost]
        public IActionResult GetFreqListNoneOfTheAbove([FromForm] long attributeIdXAxis, int setId,
                                                         string included, long onlyThisAttribute, int webDbId, string attName)
        {
            try
            {
                if (SetCSLAUser())
                {
                    SelectionCriteria criteria = new SelectionCriteria();
                    criteria.ListType = "WebDbFrequencyNoneOfTheAbove";
                    //criteria.Description = "Listing items from \"none of the children\" of code: " + attName;
                    //criteria.Description = "Records without " + attName;
                    criteria.Description = attName;
                    criteria.XAxisAttributeId = attributeIdXAxis;
                    criteria.SetId = setId;
                    if (included != "")
                    {
                        criteria.OnlyIncluded = included.ToLower() == "true" ? true : false;
                    }
                    criteria.FilterAttributeId = onlyThisAttribute;
                    ItemListWithCriteria iList = GetItemList(criteria);

                    // log to TB_WEBDB_LOG
                    logActivity("GetItemList", criteria.Description);

                    return View("Index", iList);//supplying the view name, otherwise MVC would try to auto-discover a view called Page.
                }
                else return Unauthorized();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error in ItemList GetFreqListNoneOfTheAbove");
                return StatusCode(500, e.Message);
            }
        }
        [HttpPost]
        public IActionResult GetFreqListNoneOfTheAboveJSon([FromForm] long attributeIdXAxis, int setId,
                                                         string included, long onlyThisAttribute, int webDbId, string attName)
        {
            try
            {
                if (SetCSLAUser())
                {
                    SelectionCriteria criteria = new SelectionCriteria();
                    criteria.ListType = "WebDbFrequencyNoneOfTheAbove";
                    criteria.Description = attName;
                    criteria.XAxisAttributeId = attributeIdXAxis;
                    criteria.SetId = setId;
                    if (included != "")
                    {
                        criteria.OnlyIncluded = included.ToLower() == "true" ? true : false;
                    }
                    criteria.FilterAttributeId = onlyThisAttribute;
                    ItemListWithCriteria iList = GetItemList(criteria);

                    // log to TB_WEBDB_LOG
                    logActivity("GetItemList", criteria.Description);

                    return Json(iList);//supplying the view name, otherwise MVC would try to auto-discover a view called Page.
                }
                else return Unauthorized();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error in ItemList GetFreqListNoneOfTheAboveJSon");
                return StatusCode(500, e.Message);
            }
        }
        
        [HttpPost]
        public IActionResult GetListWithWithoutAtts([FromForm] string WithAttIds, string WithSetId, string WithoutAttIds, string WithoutSetId, string included, string Description = "")
        {
            try
            {
                if (SetCSLAUser())
                {
                    //basic check: number of atts and sets match...
                    if (WithAttIds.Count(c => c== ',') != WithSetId.Count(c => c == ',')
                        || (
                            WithoutAttIds != null && WithoutSetId != null &&
                            WithoutAttIds.Count(c => c == ',') != WithoutSetId.Count(c => c == ',')
                            )
                        )
                    {
                        _logger.LogError("Error in ItemList GetListWithWithoutAtts: number of AttIDs didn't match number os SetIDs");
                        return BadRequest("Request parameters appear to be malformed.");
                    }
                    SelectionCriteria criteria = new SelectionCriteria();
                    criteria.ListType = "WebDbWithWithoutCodes";
                    criteria.WithAttributesIds = WithAttIds;
                    criteria.WithSetIdsList = WithSetId;
                    criteria.WithOutAttributesIdsList = WithoutAttIds;
                    criteria.WithOutSetIdsList = WithoutSetId;
                    criteria.Description = Description;
                    if (included != "")
                    {
                        criteria.OnlyIncluded = included.ToLower() == "true" ? true : false;
                    }
                    ItemListWithCriteria iList = GetItemList(criteria);
                    return View("Index", iList);//supplying the view name, otherwise MVC would try to auto-discover a view called Page.
                }
                else return Unauthorized();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error in ItemList GetListWithWithoutAtts");
                return StatusCode(500, e.Message);
            }
        }
        [HttpPost]
        public IActionResult GetListWithWithoutAttsJSON([FromForm] string WithAttIds, string WithSetId, string WithoutAttIds, string WithoutSetId, string included, string Description = "")
        {
            return internalGetListWithWithoutAttsJSON(WithAttIds, WithSetId, WithoutAttIds, WithoutSetId, included, Description);
        }

        [Authorize(AuthenticationSchemes = "FairAuthentication")]
        [HttpPost]
        public IActionResult FairGetListWithWithoutAttsJSON([FromForm] string WithAttIds, string WithSetId, string WithoutAttIds, string WithoutSetId, string included, string Description = "")
        {
            return internalGetListWithWithoutAttsJSON(WithAttIds, WithSetId, WithoutAttIds, WithoutSetId, included, Description);
        }
        private IActionResult internalGetListWithWithoutAttsJSON([FromForm] string WithAttIds, string WithSetId, string WithoutAttIds, string WithoutSetId, string included, string Description)
        {
            try
            {
                if (SetCSLAUser())
                {
                    //basic check: number of atts and sets match...
                    if (WithAttIds.Count(c => c == ',') != WithSetId.Count(c => c == ',')
                        || (
                            WithoutAttIds != null && WithoutSetId != null &&
                            WithoutAttIds.Count(c => c == ',') != WithoutSetId.Count(c => c == ',')
                            )
                        )
                    {
                        _logger.LogError("Error in ItemList GetListWithWithoutAtts: number of AttIDs didn't match number os SetIDs");
                        return BadRequest("Request parameters appear to be malformed.");
                    }
                    SelectionCriteria criteria = new SelectionCriteria();
                    criteria.ListType = "WebDbWithWithoutCodes";
                    criteria.WithAttributesIds = WithAttIds;
                    criteria.WithSetIdsList = WithSetId;
                    criteria.WithOutAttributesIdsList = WithoutAttIds;
                    criteria.WithOutSetIdsList = WithoutSetId;
                    criteria.Description = Description;
                    if (included != "")
                    {
                        criteria.OnlyIncluded = included.ToLower() == "true" ? true : false;
                    }
                    ItemListWithCriteria iList = GetItemList(criteria);

                    // log to TB_WEBDB_LOG
                    logActivity("GetItemList", criteria.Description);

                    return Json(iList);
                }
                else return Unauthorized();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error in ItemList GetListWithWithoutAttsJSON");
                return StatusCode(500, e.Message);
            }
        }



        public IActionResult GetListSearchResults([FromForm] string SearchString, string SearchWhat, string included)
        {
            try
            {
                if (SetCSLAUser())
                {
                    string errorMsg;
                    SelectionCriteria criteria = ItemListController.GetCriterionForSearch(SearchString, SearchWhat, included, out errorMsg);
                    if (errorMsg != "")
                    {
                        _logger.LogError("Error in ItemList GetListSearchResults: search parameters appear to be malformed.");
                        return BadRequest("Request parameters appear to be malformed.");
                    }
                    ItemListWithCriteria iList = GetItemList(criteria);
                    // log to TB_WEBDB_LOG
                    logActivity("Search", criteria.Description);

                    return View("Index", iList);//supplying the view name, otherwise MVC would try to auto-discover a view called Page.
                }
                else return Unauthorized();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error in ItemList GetListSearchResults");
                return StatusCode(500, e.Message);
            }
        }
        [HttpPost]
        public IActionResult GetListSearchResultsJSON([FromForm] string SearchString, string SearchWhat, string included)
        {
            try
            {
                if (SetCSLAUser())
                {
                    string errorMsg;
                    SelectionCriteria criteria = ItemListController.GetCriterionForSearch(SearchString, SearchWhat, included, out errorMsg);
                    if (errorMsg != "")
                    {
                        _logger.LogError("Error in ItemList GetListSearchResultsJSON: search parameters appear to be malformed.");
                        return BadRequest("Request parameters appear to be malformed.");
                    }
                    ItemListWithCriteria iList = GetItemList(criteria);
                    // log to TB_WEBDB_LOG
                    logActivity("Search", criteria.Description);
                    return Json(iList);
                }
                else return Unauthorized();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error in ItemList GetListSearchResultsJSON");
                return StatusCode(500, e.Message);
            }
        }

        internal static SelectionCriteria GetCriterionForSearch(string SearchString, string SearchWhat, string included, out string ErrorMessage)
        {
            ErrorMessage = "";
            SelectionCriteria criteria = new SelectionCriteria();
            string[] sTypes = {
                    "TitleAbstract"
                    ,"Title"
                    ,"Abstract"
                    ,"PubYear"
                    ,"AdditionalText"
                    ,"ItemId"
                    ,"OldItemId"
                    ,"Authors"};
            //basic check: is the list type supported?
            if (!sTypes.Contains(SearchWhat) || SearchString == null || SearchString == "")
            {
                ErrorMessage = "Error in ItemList GetListSearchResults: search parameters appear to be malformed.";
                return criteria;
            }
            //basic check: number of atts and sets match...
            criteria.ListType = "WebDbSearch";
            criteria.SearchString = SearchString;
            criteria.SearchWhat = SearchWhat;
            string descr = "Search results (in ";
            if (SearchWhat == "TitleAbstract") descr += "Title and Abstract" + ") for: ";
            else if (SearchWhat == "AdditionalText") descr += "\"Coded\" Text" + ") for: ";
            else if (SearchWhat == "PubYear") descr += "Publication Year" + ") for: ";
            else if (SearchWhat == "OldItemId") descr += "Imported ID(s)" + ") for: ";
            else descr += SearchWhat + ") for: ";
            if (SearchString.Length > 30)
            {
                int i = SearchString.IndexOf(' ', 15);
                if (i > 0) descr += SearchString.Substring(0, i) + " [...]";
                else descr += SearchString.Substring(0, 30) + " [...]";
            }
            else descr += SearchString;
            criteria.Description = descr;
            if (included != "")
            {
                criteria.OnlyIncluded = included.ToLower() == "true" ? true : false;
            }
            return criteria;
        }


        [HttpPost]
        public IActionResult ItemDetails(ItemSelCritMVC crit)
        {
            try
            {
                if (SetCSLAUser())
                {
                    FullItemDetails Itm = GetItemDetails(crit);

                    // log to TB_WEBDB_LOG
                    logActivity("ItemDetailsFromList", crit.itemID.ToString());

                    return View(Itm);
                }
                else return Unauthorized();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error in ItemDetails");
                return StatusCode(500, e.Message);
            }
        }
        [HttpPost]
        public IActionResult ItemDetailsJSON(ItemSelCritMVC crit)
        {//we provide all items details in a single JSON method, as it makes no sense to get partial item details, so without Arms, Docs, etc.
            try
            {
                if (SetCSLAUser())
                {
                    FullItemDetails Itm = GetItemDetails(crit);
                    return Json(Itm);
                }
                else return Unauthorized();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error in ItemDetailsJSON");
                return StatusCode(500, e.Message);
            }
        }
        [Authorize(AuthenticationSchemes = "VawgAuthentication")]
        [HttpPost]
        public IActionResult VawgItemDetails(ItemSelCritMVC crit)
        {
            try
            {
                if (SetCSLAUser())
                {
                    return View("VawgItemDetails", crit);
                }
                else return Unauthorized();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error in ItemDetails");
                return StatusCode(500, e.Message);
            }
        }


    }
    public class SelCritMVC
    {
        public SelCritMVC() { }
        public SelCritMVC(SelectionCriteria CSLAcrit)
        {
            onlyIncluded = CSLAcrit.OnlyIncluded;
            showDeleted = CSLAcrit.ShowDeleted;
            sourceId = CSLAcrit.SourceId;
            searchId = CSLAcrit.SearchId;
            xAxisSetId = CSLAcrit.XAxisSetId;
            xAxisAttributeId = CSLAcrit.XAxisAttributeId;
            yAxisSetId = CSLAcrit.YAxisSetId;
            yAxisAttributeId = CSLAcrit.YAxisAttributeId;
            filterSetId = CSLAcrit.FilterSetId;
            filterAttributeId = CSLAcrit.FilterAttributeId;
            attributeSetIdList = CSLAcrit.AttributeSetIdList;
            listType = CSLAcrit.ListType;
            pageNumber = CSLAcrit.PageNumber;
            pageSize = CSLAcrit.PageSize;
            workAllocationId = CSLAcrit.WorkAllocationId;
            magSimulationId = CSLAcrit.MagSimulationId;
            comparisonId = CSLAcrit.ComparisonId;
            description = CSLAcrit.Description;
            contactId = CSLAcrit.ContactId;
            setId = CSLAcrit.SetId;
            showInfoColumn = CSLAcrit.ShowInfoColumn;
            showScoreColumn = CSLAcrit.ShowScoreColumn;
            webDbId = CSLAcrit.WebDbId;
            withAttributesIds = CSLAcrit.WithAttributesIds;
            withSetIdsList = CSLAcrit.WithSetIdsList;
            withOutAttributesIdsList = CSLAcrit.WithOutAttributesIdsList;
            withOutSetIdsList = CSLAcrit.WithOutSetIdsList;
            searchString = CSLAcrit.SearchString;
            searchWhat = CSLAcrit.SearchWhat;
        }
        public bool? onlyIncluded { get; set; }
        public bool showDeleted { get; set; }
        public int sourceId { get; set; }
        public int searchId { get; set; }
        public Int64 xAxisSetId { get; set; }
        public Int64 xAxisAttributeId { get; set; }
        public Int64 yAxisSetId { get; set; }
        public Int64 yAxisAttributeId { get; set; }
        public Int64 filterSetId { get; set; }
        public Int64 filterAttributeId { get; set; }
        public string attributeSetIdList { get; set; }
        public string listType { get; set; }
        public int pageNumber { get; set; }
        public int pageSize { get; set; }
        public int totalItems { get; set; }
        public int startPage { get; set; }
        public int endPage { get; set; }
        public int startIndex { get; set; }
        public int endIndex { get; set; }
        public int workAllocationId { get; set; }
        public int comparisonId { get; set; }
        public int magSimulationId { get; set; }
        public string description { get; set; }
        public int contactId { get; set; }
        public int setId { get; set; }
        public bool showInfoColumn { get; set; }
        public bool showScoreColumn { get; set; }
        public int webDbId { get; set; }
        public string withAttributesIds { get; set; }
        public string withSetIdsList { get; set; }
        public string withOutAttributesIdsList { get; set; }
        public string withOutSetIdsList { get; set; }
        public string searchWhat { get; set; }
        public string searchString { get; set; }
        public SelectionCriteria CSLACriteria() 
        {
            SelectionCriteria CSLAcrit = new SelectionCriteria();
            CSLAcrit.OnlyIncluded = onlyIncluded;
            CSLAcrit.ShowDeleted = showDeleted;
            CSLAcrit.SourceId = sourceId;
            CSLAcrit.SearchId = searchId;
            CSLAcrit.XAxisSetId = xAxisSetId;
            CSLAcrit.XAxisAttributeId = xAxisAttributeId;
            CSLAcrit.YAxisSetId = yAxisSetId;
            CSLAcrit.YAxisAttributeId = yAxisAttributeId;
            CSLAcrit.FilterSetId = filterSetId;
            CSLAcrit.FilterAttributeId = filterAttributeId;
            CSLAcrit.AttributeSetIdList = attributeSetIdList;
            CSLAcrit.ListType = listType;
            CSLAcrit.PageNumber = pageNumber;
            CSLAcrit.PageSize = pageSize;
            CSLAcrit.WorkAllocationId = workAllocationId;
            CSLAcrit.MagSimulationId = magSimulationId;
            CSLAcrit.ComparisonId = comparisonId;
            CSLAcrit.Description = description;
            CSLAcrit.ContactId = contactId;
            CSLAcrit.SetId = setId;
            CSLAcrit.ShowInfoColumn = showInfoColumn;
            CSLAcrit.ShowScoreColumn = showScoreColumn;
            CSLAcrit.WebDbId = webDbId;
            CSLAcrit.WithAttributesIds = withAttributesIds;
            CSLAcrit.WithSetIdsList = withSetIdsList;
            CSLAcrit.WithOutAttributesIdsList = withOutAttributesIdsList;
            CSLAcrit.WithOutSetIdsList = withOutSetIdsList;
            CSLAcrit.SearchWhat = searchWhat;
            CSLAcrit.SearchString = searchString;
            return CSLAcrit;
        }
    }

    public class ItemSelCritMVC: SelCritMVC
    {
        public long itemID { get; set; }
        public string itemIds { get; set; }
    }
}