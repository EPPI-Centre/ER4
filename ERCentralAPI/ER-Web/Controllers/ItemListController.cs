using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using BusinessLibrary.BusinessClasses;
using BusinessLibrary.Security;
using Csla;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace ERxWebClient2.Controllers
{
    

    [Authorize]
    [Route("api/[controller]")]
    public class ItemListController : CSLAController
    {
        

        public ItemListController(ILogger<ItemListController> logger) : base(logger)
        { }


        //[HttpGet("[action]")]
        //public ItemList4Json IncludedItems()//should receive a reviewID!
        //{
        //    try
        //    {
        //        if (!SetCSLAUser()) return Unauthorized();
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
        //        return StatusCode(500, e.Message);
        //    }
            
        //}

        [HttpPost("[action]")]
        public IActionResult Fetch([FromBody] SelCritMVC crit )
        {
            try
            {

                if (!SetCSLAUser()) return Unauthorized();

                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;

                DataPortal<ItemList> dp = new DataPortal<ItemList>();
                SelectionCriteria CSLAcrit = crit.CSLACriteria;
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

        [HttpGet("[action]")]
        public IActionResult ItemTypes()
        {
            try
            {
                if (!SetCSLAUser()) return Unauthorized();
                ItemTypeNVLFactory factory = new ItemTypeNVLFactory();
                ItemTypeNVL res = factory.FetchItemTypeNVL();
                //DataPortal<ItemTypeNVL> dp = new DataPortal<ItemTypeNVL>();
                //ItemTypeNVL res = dp.Fetch();
                return Ok(res);
            }
            catch(Exception e)
            {
                _logger.LogError(e, "Error fetching ItemTypes");
                return StatusCode(500, e.Message);
            }
        }
        [HttpPost("[action]")]
        public IActionResult FetchAdditionalItemData([FromBody] SingleInt64Criteria itemID)
        {
            try
            {
                if (!SetCSLAUser()) return Unauthorized();
                DataPortal<ReadOnlySource> dps = new DataPortal<ReadOnlySource>();
                DataPortal<ItemDuplicatesReadOnlyList> pdp = new DataPortal<ItemDuplicatesReadOnlyList>();
                ReadOnlySource ros = dps.Fetch(new SingleCriteria<ReadOnlySource, long>(itemID.Value));
                ItemDuplicatesReadOnlyList dups = pdp.Fetch(new SingleCriteria<ItemDuplicatesReadOnlyList, long>(itemID.Value));
                AdditionalItemData res = new AdditionalItemData(itemID.Value, ros, dups);
                return Ok(res);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error fetching ItemTypes");
                return StatusCode(500, e.Message);
            }
        }

        [HttpPost("[action]")]
        public IActionResult UpdateItem([FromBody] ItemJSON item)
        {

            try
            {
                if (SetCSLAUser4Writing())
                {
                    DataPortal<Item> dp = new DataPortal<Item>();
                    SingleCriteria<Item, Int64> criteria = new SingleCriteria<Item, long>(item.itemId);
                    Item CSLAItem = item.itemId == 0 ? new Item() : dp.Fetch(criteria);
                    UpdateItemData(item, CSLAItem);
                    CSLAItem = CSLAItem.Save();
                    return Ok(CSLAItem);
                }
                else
                {
                    return Forbid();
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error when Updating an Item : {0}", item);
                return StatusCode(500, e.Message);
            }
        }
        private void UpdateItemData(ItemJSON item, Item CSLAItem)
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            if (item.itemId == 0)
            {
                CSLAItem.CreatedBy = ri.Name;
                CSLAItem.DateCreated = DateTime.Now;
            }
            CSLAItem.BeginEdit();
            CSLAItem.Title = item.title;
            CSLAItem.DateEdited = DateTime.Now;
            CSLAItem.TypeId = item.typeId;
            CSLAItem.ParentTitle = item.parentTitle;
            CSLAItem.ShortTitle = item.shortTitle;
            CSLAItem.EditedBy = ri.Name;
            CSLAItem.Year = item.year;
            CSLAItem.Month = item.month;
            CSLAItem.StandardNumber = item.standardNumber;
            CSLAItem.City = item.city;
            CSLAItem.Country = item.country;
            CSLAItem.Publisher = item.publisher;
            CSLAItem.Institution = item.institution;
            CSLAItem.Volume = item.volume;
            CSLAItem.Pages = item.pages;
            CSLAItem.Edition = item.edition;
            CSLAItem.Issue = item.issue;
            CSLAItem.Availability = item.availability;
            CSLAItem.URL = item.url;
            CSLAItem.Comments = item.comments;
            if (item.itemStatus.ToUpper() == "I" || item.itemStatus.ToUpper() == "")
            {
                CSLAItem.IsItemDeleted = false;
                CSLAItem.IsIncluded = true;
            }
            else if (item.itemStatus.ToUpper() == "D")
            {
                CSLAItem.IsItemDeleted = true;
                CSLAItem.IsIncluded = false;
            }
            else if (item.itemStatus.ToUpper() == "E")
            {
                CSLAItem.IsItemDeleted = false;
                CSLAItem.IsIncluded = false;
            }
            CSLAItem.DOI = item.doi;
            CSLAItem.Keywords = item.keywords;
            CSLAItem.Authors = item.authors;
            CSLAItem.ParentAuthors = item.parentAuthors;
            CSLAItem.Abstract = item.@abstract;
            CSLAItem.ApplyEdit();
        }


		[HttpPost("[action]")]
		public IActionResult DeleteSelectedItems([FromBody] long[] ItemIds)
		{
			try
			{
				//string strItemIds = ItemIds.ItemIds;



				if (SetCSLAUser4Writing())
				{
					DataPortal<ItemDeleteUndeleteCommand> dp = new DataPortal<ItemDeleteUndeleteCommand>();
					ItemDeleteUndeleteCommand command = new ItemDeleteUndeleteCommand(
						true, string.Join(",", ItemIds));

					command = dp.Execute(command);
					return Ok(command);
				}
				else
				{
					return Forbid();
				}
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Error when deleting items: {0}", ItemIds == null ? "no data" : ItemIds.ToString() );
				return StatusCode(500, e.Message);
			}
		}

		[HttpPost("[action]")]
		public IActionResult AssignDocumentsToIncOrExc([FromBody] MVCAssignItems assignMvc)
		{
			try
			{
				if (SetCSLAUser4Writing())
				{
					bool inc = assignMvc.Include == "true" ? true: false;
					if (assignMvc.attributeid > 0)
					{
						assignMvc.itemids = "";
					}
				
						DataPortal<ItemIncludeExcludeCommand> dp = new DataPortal<ItemIncludeExcludeCommand>();
						ItemIncludeExcludeCommand command = new ItemIncludeExcludeCommand(
						inc,
						assignMvc.itemids,
						assignMvc.attributeid,
						assignMvc.setid
						);
						command = dp.Execute(command);
						return Ok(command);
				}
				else
				{
					return Forbid();
				}
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Error when Assigning items: {0}", assignMvc);
				return StatusCode(500, e.Message);
			}
		}

        [HttpPost("[action]")]
        public IActionResult GetSingleItem([FromBody] SingleInt64Criteria ItemIDCrit)
        {

            try
            {
                if (!SetCSLAUser()) return Unauthorized();
                SingleCriteria<Item, Int64> criteria = new SingleCriteria<Item, Int64>(ItemIDCrit.Value);
                Item result = DataPortal.Fetch<Item>(criteria);
                return Ok(result);

            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error in GetSingleItem: {0}", ItemIDCrit.Value);
                return StatusCode(500, e.Message);
            }
        }
    }
	
	public class SelCritMVC
    {
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

        public string withOutAttributesIdsList { get; set; }
        public string withAttributesIds { get; set; }
        public string withSetIdsList { get; set; }
        public string withOutSetIdsList { get; set; }

        public SelectionCriteria CSLACriteria 
        {
            get
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
                CSLAcrit.WithAttributesIds = withAttributesIds;
                CSLAcrit.WithOutAttributesIdsList = withOutAttributesIdsList;
                CSLAcrit.WithOutSetIdsList = withOutSetIdsList;
                CSLAcrit.WithSetIdsList = withSetIdsList;
                return CSLAcrit;
            }
        }
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
    public class ItemJSON
    {
        public Int64 itemId;
        public string title;
        public int typeId;
        public string itemStatus;
        public string parentTitle;
        public string shortTitle;
        public string editedBy;
        public string year;
        public string month;
        public string standardNumber;
        public string city;
        public string country;
        public string publisher;
        public string institution;
        public string volume;
        public string pages;
        public string edition;
        public string issue;
        public string availability;
        public string url;
        public string comments;
        public string doi;
        public string keywords;
        public string authors;
        public string parentAuthors;
        public string @abstract;

        public bool isIncluded;
        public bool isItemDeleted;
        public string isLocal;

        public string quickCitation;
    }
    public class AdditionalItemData
    {
        public AdditionalItemData(long ItemId, ReadOnlySource Source, ItemDuplicatesReadOnlyList Duplicates)
        {
            this.itemID = ItemId;
            this.source = Source;
            this.duplicates = Duplicates;
        }
        public long itemID;
        public ReadOnlySource source;
        public ItemDuplicatesReadOnlyList duplicates;
    }

	public class MVCAssignItems
	{
		public string Include {get; set;}
		public string itemids { get; set; }
		public int attributeid { get; set; }
		public int setid { get; set; }

	}

}
