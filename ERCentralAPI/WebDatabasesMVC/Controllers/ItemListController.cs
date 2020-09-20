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
                    ItemList iList = GetItemList(crit);
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
                    ItemList iList = GetItemList(crit);
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
                    ItemList iList = GetItemList(crit);
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
                    ItemList iList = GetItemList(crit);
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
        internal ItemList GetItemList(SelectionCriteria crit)
        {
            List<Claim> claims = User.Claims.ToList();
            Claim AttIdC = claims.Find(f => f.Type == "ItemsCode");
            //no try here, if an exception happens it's caught by the caller method
            crit.FilterAttributeId = long.Parse(AttIdC.Value);
            crit.PageSize = 100;
            crit.OnlyIncluded = true;
            ItemList res = DataPortal.Fetch<ItemList>(crit);
            return res;
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
}