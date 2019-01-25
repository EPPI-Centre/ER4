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
using Csla.Data;
using ERxWebClient2.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ERxWebClient2.Controllers
{
    

    [Authorize]
    [Route("api/[controller]")]
    public class ItemListController : CSLAController
    {

        private readonly ILogger _logger;

        public ItemListController(ILogger<ItemListController> logger)
        {
            _logger = logger;

        }


        //[HttpGet("[action]")]
        //public ItemList4Json IncludedItems()//should receive a reviewID!
        //{
        //    try
        //    {
        //        SetCSLAUser();
        //        ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;

        //        DataPortal<ItemList> dp = new DataPortal<ItemList>();
        //        SelectionCriteria crit = new SelectionCriteria();
        //        //crit = new SelectionCriteria();
        //        crit.ListType = "StandardItemList";
        //        crit.OnlyIncluded = true;
        //        crit.ShowDeleted = false;
        //        crit.AttributeSetIdList = "";
        //        crit.PageSize = 5;
        //        crit.PageNumber = 0;
        //        ItemList result = dp.Fetch(crit);
        //        return new ItemList4Json(result);
        //    }
        //    catch (Exception e)
        //    {
        //        _logger.LogError(e, "Included Items dataportal error");
        //        throw;
        //    }
            
        //}

        [HttpPost("[action]")]
        public IActionResult Fetch([FromBody] SelCritMVC crit )
        {
            try
            {

                SetCSLAUser();
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;

                DataPortal<ItemList> dp = new DataPortal<ItemList>();
                SelectionCriteria CSLAcrit = new SelectionCriteria();
                CSLAcrit.OnlyIncluded = crit.onlyIncluded;
                CSLAcrit.ShowDeleted = crit.showDeleted;
                CSLAcrit.SourceId = crit.sourceId;
                CSLAcrit.SearchId = crit.searchId;
                CSLAcrit.XAxisSetId = crit.xAxisSetId;
                CSLAcrit.XAxisAttributeId = crit.xAxisAttributeId;
                CSLAcrit.YAxisSetId = crit.yAxisSetId;
                CSLAcrit.YAxisAttributeId = crit.yAxisAttributeId;
                CSLAcrit.FilterSetId = crit.filterSetId;
                CSLAcrit.FilterAttributeId = crit.filterAttributeId;
                CSLAcrit.AttributeSetIdList = crit.attributeSetIdList;
                CSLAcrit.ListType = crit.listType;
                CSLAcrit.PageNumber = crit.pageNumber;
                CSLAcrit.PageSize = crit.pageSize;
                CSLAcrit.WorkAllocationId = crit.workAllocationId;
                CSLAcrit.ComparisonId = crit.comparisonId;
                CSLAcrit.Description = crit.description;
                CSLAcrit.ContactId = crit.contactId;
                CSLAcrit.SetId = crit.setId;
                CSLAcrit.ShowInfoColumn = crit.showInfoColumn;
                CSLAcrit.ShowScoreColumn = crit.showScoreColumn;
                ItemList result = dp.Fetch(CSLAcrit);

                //return Json(result);
                return Ok(new ItemList4Json(result));
            }
            catch (Exception e)
            {
                string json = JsonConvert.SerializeObject(crit);
                _logger.LogError(e, "Fetching criteria: {0}", json);
                return StatusCode(500, e.Message);
            }
        }
        
        [HttpPost("[action]")]
        public IActionResult WorkAllocation(int AllocationId, string ListType, int pageSize, int pageNumber)
        {
            try
            {
                SetCSLAUser();
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;

                DataPortal<ItemList> dp = new DataPortal<ItemList>();
                SelectionCriteria crit = new SelectionCriteria();
                crit.WorkAllocationId = AllocationId;
                crit.ListType = ListType;
                crit.PageSize = pageSize;
                crit.PageNumber = pageNumber;
                ItemList result = dp.Fetch(crit);
                //return Json(result);

                return Ok(new ItemList4Json(result));
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
    public class SelCritMVC
    {
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
        public string description { get; set; }
        public int contactId { get; set; }
        public int setId { get; set; }
        public bool showInfoColumn { get; set; }
        public bool showScoreColumn { get; set; }
    }
    //{"onlyIncluded":true,"showDeleted":false,"sourceId":0,"searchId":0,"xAxisSetId":0,"xAxisAttributeId":0,"yAxisSetId":0,"yAxisAttributeId":0,"filterSetId":0,"filterAttributeId":0,"attributeSetIdList":"","listType":"GetItemWorkAllocationListRemaining","pageNumber":0,"pageSize":100,"workAllocationId":500,"comparisonId":0,"description":"","contactId":0,"setId":0,"showInfoColumn":true,"showScoreColumn":true}
    public class ItemList4Json
    {
        private ItemList _list;
        public int pagesize
        {
            get { return _list.PageSize; }
        }
        public int pagecount
        {
            get { return _list.PageCount; }
        }
        public int pageindex
        {
            get { return _list.PageIndex; }
        }
        public int totalItemCount
        {
            get { return _list.TotalItemCount; }
        }
        public ItemList Items
        {
            get { return _list; }
        }
        public ItemList4Json(ItemList list)
        { _list = list; }
    }
}
