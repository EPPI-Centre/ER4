using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Claims;
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
    [Authorize]
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
            try
            {
                if (SetCSLAUser())
                {
                    SelectionCriteria crit = critMVC.CSLACriteria();
                    ItemListWithCriteria iList = GetItemList(crit);
                    return Json(iList);
                }
                else return Unauthorized();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error in ItemList ListFromCritJson");
                return StatusCode(500, e.Message);
            }
        }
        
        public IActionResult GetFreqList([FromForm] long attId)
        {
            try
            {
                if (SetCSLAUser())
                {
                    SelectionCriteria crit = new SelectionCriteria();
                    crit.ListType = "WebDbWithThisCode";
                    crit.FilterAttributeId = attId;
                    ItemListWithCriteria iList = GetItemList(crit);
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
        public IActionResult GetFreqListJSon([FromForm] long attId)
        {
            try
            {
                if (SetCSLAUser())
                {
                    SelectionCriteria crit = new SelectionCriteria();
                    crit.ListType = "WebDbWithThisCode";
                    crit.FilterAttributeId = attId;
                    ItemListWithCriteria iList = GetItemList(crit);
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
                                                         string included, long onlyThisAttribute, int webDbId)
        {
            try
            {
                if (SetCSLAUser())
                {
                    SelectionCriteria criteria = new SelectionCriteria();
                    criteria.ListType = "WebDbFrequencyNoneOfTheAbove";
                    criteria.XAxisAttributeId = attributeIdXAxis;
                    criteria.SetId = setId;
                    if (included != "")
                    {
                        criteria.OnlyIncluded = included.ToLower() == "true" ? true : false;
                    }
                    criteria.FilterAttributeId = onlyThisAttribute;
                    ItemListWithCriteria iList = GetItemList(criteria);
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
                                                         string included, long onlyThisAttribute, int webDbId)
        {
            try
            {
                if (SetCSLAUser())
                {
                    SelectionCriteria criteria = new SelectionCriteria();
                    criteria.ListType = "WebDbFrequencyNoneOfTheAbove";
                    criteria.XAxisAttributeId = attributeIdXAxis;
                    criteria.SetId = setId;
                    if (included != "")
                    {
                        criteria.OnlyIncluded = included.ToLower() == "true" ? true : false;
                    }
                    criteria.FilterAttributeId = onlyThisAttribute;
                    ItemListWithCriteria iList = GetItemList(criteria);
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

        internal ItemListWithCriteria GetItemList(SelectionCriteria crit)
        {
            List<Claim> claims = User.Claims.ToList();
            Claim AttIdC = claims.Find(f => f.Type == "ItemsCode");
            Claim DBidC = claims.Find(f => f.Type == "WebDbID");
            if (crit.WebDbId == 0)
            {
                crit.WebDbId = int.Parse(DBidC.Value);
                crit.PageSize = 100;
            }
            else if (int.Parse(DBidC.Value) != crit.WebDbId)
            {
                throw new Exception("WebDbId in ItemList Criteria is not the expected value - possible tampering attempt!");
            }
            //no try here, if an exception happens it's caught by the caller method
            long tmp =  long.Parse(AttIdC.Value);
            if (crit.ListType == "StandardItemList" && tmp > 0 && crit.FilterAttributeId == 0)
            {
                crit.FilterAttributeId = tmp;
                crit.OnlyIncluded = true;
            }
            ItemList res = DataPortal.Fetch<ItemList>(crit);
            return new ItemListWithCriteria { items = res, criteria = new SelCritMVC(crit)   };
        }


        public IActionResult ItemDetails([FromForm] long itemId)
        {
            try
            {
                if (SetCSLAUser())
                {
                    FullItemDetails Itm = GetItemDetails(itemId);
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
        public IActionResult ItemDetailsJSON([FromForm] long itemId)
        {//we provide all items details in a single JSON method, as it makes no sense to get partial item details, so without Arms, Docs, etc.
            try
            {
                if (SetCSLAUser())
                {
                    FullItemDetails Itm = GetItemDetails(itemId);
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
        internal FullItemDetails GetItemDetails(long ItemId)
        {
            Item itm = DataPortal.Fetch<Item>(new SingleCriteria<Item, Int64>(ItemId));
            ItemArmList arms = DataPortal.Fetch<ItemArmList>(new SingleCriteria<Item, Int64>(ItemId));
            itm.Arms = arms;
            ItemTimepointList timepoints = DataPortal.Fetch<ItemTimepointList>(new SingleCriteria<Item, Int64>(ItemId));
            ItemDocumentList docs = DataPortal.Fetch<ItemDocumentList>(new SingleCriteria<ItemDocumentList, Int64>(ItemId));
            ReadOnlySource ros = DataPortal.Fetch<ReadOnlySource>(new SingleCriteria<ReadOnlySource, long>(ItemId));
            ItemDuplicatesReadOnlyList dups = DataPortal.Fetch<ItemDuplicatesReadOnlyList>(new SingleCriteria<ItemDuplicatesReadOnlyList, long>(ItemId));
            FullItemDetails res = new FullItemDetails
            {
                Item = itm,
                Documents = docs,
                Timepoints = timepoints,
                Duplicates = dups,
                Source = ros
            };
            return res;
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
        }
        public bool onlyIncluded { get; set; }
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
            return CSLAcrit;
        }
    }

    
}