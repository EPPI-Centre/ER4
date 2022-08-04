using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using BusinessLibrary.BusinessClasses;
using BusinessLibrary.Security;
using Csla;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;


namespace ERxWebClient2.Controllers
{
    
    [Authorize]
    [Route("api/[controller]")]
    public class ArmTimepointLinkListController : CSLAController
    {
        
        public ArmTimepointLinkListController(ILogger<ArmTimepointLinkListController> logger) : base(logger)
        { }

		// READ ALL three
		[HttpPost("[action]")]
		public IActionResult GetArmTimepointLinkLists([FromBody] SingleInt64Criteria ItemIDCrit)
		{

			try
			{
				if (!SetCSLAUser()) return Unauthorized();
				ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
				SingleCriteria<Item, Int64> criteria = new SingleCriteria<Item, Int64>(ItemIDCrit.Value);
				ItemArmTimepointLinkLists result = new ItemArmTimepointLinkLists();
				result.timePoints = DataPortal.Fetch<ItemTimepointList>(criteria);
				result.timePoints.OrderBy(x => x.ItemTimepointId).ToList();
				result.arms = DataPortal.Fetch<ItemArmList>(criteria);
				result.arms.OrderBy(x => x.ItemArmId);
				result.links = DataPortal.Fetch<ItemLinkList>(criteria);
				result.links.OrderBy(x => x.ItemLinkId);
				return Ok(result);

			}
			catch (Exception e)
			{
				_logger.LogError(e, "Error when fetching ArmTimepointLinkLists: {0}", ItemIDCrit.Value);
				return StatusCode(500, e.Message);
			}
		}

		// READ
		[HttpPost("[action]")]
		public IActionResult GetTimePoints([FromBody] SingleInt64Criteria ItemIDCrit)
		{

			try
			{
				if (!SetCSLAUser()) return Unauthorized();
				ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
				DataPortal<ItemTimepointList> dp = new DataPortal<ItemTimepointList>();
				SingleCriteria<Item, Int64> criteria = new SingleCriteria<Item, Int64>(ItemIDCrit.Value);
				ItemTimepointList result = dp.Fetch(criteria);
				//result.OrderBy(x => x.ItemTimepointId).ToList();
				return Ok(result);

			}
			catch (Exception e)
			{
				_logger.LogError(e, "Error when fetching a TimePoint list: {0}", ItemIDCrit.Value);
				return StatusCode(500, e.Message);
			}
		}

		// CREATE
		//adds an TimePoint to the list and then calls data portal insert
		[HttpPost("[action]")]
		public IActionResult CreateTimePoint([FromBody] TimePointJSON data)
		{

			try
			{
                if (SetCSLAUser4Writing())
                {
                    ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;

                    ItemTimepoint newTimePoint = new ItemTimepoint();
					newTimePoint.ItemId = data.itemId;
					newTimePoint.TimepointMetric = data.timepointMetric;
					newTimePoint.TimepointValue = Convert.ToSingle(data.timepointValue);
                    newTimePoint.ApplyEdit();
					newTimePoint = newTimePoint.Save();
					ItemTimepointList result = DataPortal.Fetch<ItemTimepointList>(new SingleCriteria<Item, Int64>(newTimePoint.ItemId));
					return Ok(result);
                }
                else
                {
                    return Forbid();
                }
            }
			catch (Exception e)
			{
				_logger.LogError(e, "Error when Creating a TimePoint : {0}", data.itemTimepointId);
				return StatusCode(500, e.Message);
			}
		}


		// UPDATE
		[HttpPost("[action]")]
		public IActionResult UpdateTimePoint([FromBody] TimePointJSON data)
		{

			try
			{
                if (SetCSLAUser4Writing())
                {
                    ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;

                    DataPortal<ItemTimepointList> dp = new DataPortal<ItemTimepointList>();
                    SingleCriteria<Item, Int64> criteria = new SingleCriteria<Item, Int64>(data.itemId);
                    ItemTimepointList result = dp.Fetch(criteria);

                    ItemTimepoint editTimePoint = result.FirstOrDefault(x => x.ItemTimepointId == data.itemTimepointId);
					editTimePoint.TimepointMetric = data.timepointMetric;
					editTimePoint.TimepointValue = Convert.ToSingle(data.timepointValue);
					editTimePoint = editTimePoint.Save();

                    return Ok(editTimePoint);
                }
                else
                {
                    return Forbid();
                }
            }
			catch (Exception e)
			{
				_logger.LogError(e, "Error when updating a TimePoint: {0}", data.itemTimepointId);
				return StatusCode(500, e.Message);
			}
		}


		// DELETE WARNING COMMAND OBJECT
		[HttpPost("[action]")]
		public IActionResult DeleteWarningTimePoint([FromBody] ItemTimepointDeleteWarningCommandJSON TimePointJSON)
		{

			try
			{
                if (SetCSLAUser4Writing())
                {
                    ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;

                    DataPortal<ItemTimepointDeleteWarningCommand> dp = new DataPortal<ItemTimepointDeleteWarningCommand>();
                    ItemTimepointDeleteWarningCommand command = new ItemTimepointDeleteWarningCommand(TimePointJSON.itemId, TimePointJSON.itemTimepointId);
                    ItemTimepointDeleteWarningCommand result = dp.Execute(command);


                    return Ok(result);
                }
                else
                {
                    return Forbid();
                }
            }
			catch (Exception e)
			{
				_logger.LogError(e, "Error when delete warning is called: {0}", TimePointJSON.itemTimepointId);
				return StatusCode(500, e.Message);
			}

		}

		// DELETE
		[HttpPost("[action]")]
		public IActionResult DeleteTimePoint([FromBody] TimePointJSON CurrentTimePoint)
		{

			try
			{
                if (SetCSLAUser4Writing())
                {
                    ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;

                    DataPortal<ItemTimepointList> dp = new DataPortal<ItemTimepointList>();
                    SingleCriteria<Item, Int64> criteria = new SingleCriteria<Item, Int64>(CurrentTimePoint.itemId);
                    ItemTimepointList result = dp.Fetch(criteria);

                    ItemTimepoint CurrTimePoint = result.FirstOrDefault(x => x.ItemTimepointId == CurrentTimePoint.itemTimepointId);

                    CurrTimePoint.Delete();
					result.AllowRemove = true;
					result.Remove(CurrTimePoint);
                    CurrTimePoint = CurrTimePoint.Save();
                    return Ok(result);
                }
                else
                {
                    return Forbid();
                }
            }
			catch (Exception e)
			{
				_logger.LogError(e, "Error when Deleting an TimePoint: {0}", CurrentTimePoint.itemTimepointId);
				return StatusCode(500, e.Message);
			}
		}

		[HttpPost("[action]")]
		public IActionResult GetArms([FromBody] SingleInt64Criteria ItemIDCrit)
		{

			try
			{
				if (!SetCSLAUser()) return Unauthorized();
				ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;

				DataPortal<ItemArmList> dp = new DataPortal<ItemArmList>();
				SingleCriteria<Item, Int64> criteria = new SingleCriteria<Item, Int64>(ItemIDCrit.Value);
				ItemArmList result = dp.Fetch(criteria);
				result.OrderBy(x => x.ItemArmId).ToList();
				return Ok(result);

			}
			catch (Exception e)
			{
				_logger.LogError(e, "Error when fetching an arm list: {0}", ItemIDCrit.Value);
				return StatusCode(500, e.Message);
			}
		}

		// CREATE
		//adds an arm to the list and then calls data portal insert
		[HttpPost("[action]")]
		public IActionResult CreateArm([FromBody] ArmJSON data)
		{

			try
			{
				if (SetCSLAUser4Writing())
				{
					ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;

					ItemArm newArm = new ItemArm();
					newArm.Title = data.title;
					newArm.ItemId = data.itemId;
					newArm.BeginEdit();
					newArm.ApplyEdit();

					ItemArm result = newArm.Save();

					return Ok(result);
				}
				else
				{
					return Forbid();
				}
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Error when Creating an Arm : {0}", data.itemArmId);
				return StatusCode(500, e.Message);
			}
		}


		// UPDATE
		[HttpPost("[action]")]
		public IActionResult UpdateArm([FromBody] ArmJSON data)
		{

			try
			{
				if (SetCSLAUser4Writing())
				{
					ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;

					DataPortal<ItemArmList> dp = new DataPortal<ItemArmList>();
					SingleCriteria<Item, Int64> criteria = new SingleCriteria<Item, Int64>(data.itemId);
					ItemArmList result = dp.Fetch(criteria);

					ItemArm editArm = result.FirstOrDefault(x => x.ItemArmId == data.itemArmId);
					editArm.Title = data.title;
					editArm = editArm.Save();

					return Ok(editArm);
				}
				else
				{
					return Forbid();
				}
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Error when updating an arm: {0}", data == null ? "data for arm is null" : data.itemArmId.ToString());
				return StatusCode(500, e.Message);
			}
		}


		// DELETE WARNING COMMAND OBJECT
		[HttpPost("[action]")]
		public IActionResult DeleteWarningArm([FromBody] ItemArmDeleteWarningCommandJSON ArmJSON)
		{

			try
			{
				if (SetCSLAUser4Writing())
				{
					ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;

					DataPortal<ItemArmDeleteWarningCommand> dp = new DataPortal<ItemArmDeleteWarningCommand>();
					ItemArmDeleteWarningCommand command = new ItemArmDeleteWarningCommand(ArmJSON.itemId, ArmJSON.itemArmId);
					ItemArmDeleteWarningCommand result = dp.Execute(command);


					return Ok(result);
				}
				else
				{
					return Forbid();
				}
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Error when delete warning is called: {0}", ArmJSON.itemArmId);
				return StatusCode(500, e.Message);
			}

		}

		// DELETE
		[HttpPost("[action]")]
		public IActionResult DeleteArm([FromBody] ArmJSON CurrentArm)
		{

			try
			{
				if (SetCSLAUser4Writing())
				{
					ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;

					//DataPortal<ItemArmDeleteWarningCommand> dp = new DataPortal<ItemArmDeleteWarningCommand>();
					//ItemArmDeleteWarningCommand command = new ItemArmDeleteWarningCommand(CurrentArm.itemId, CurrentArm.itemArmId);
					//ItemArmDeleteWarningCommand res = dp.Execute(command);

					//dp.Execute(command);

					// Actually need to end up with ItemArm  currentArm.Delete()


					DataPortal<ItemArmList> dp = new DataPortal<ItemArmList>();
					SingleCriteria<Item, Int64> criteria = new SingleCriteria<Item, Int64>(CurrentArm.itemId);
					ItemArmList result = dp.Fetch(criteria);

					ItemArm CurrArm = result.FirstOrDefault(x => x.ItemArmId == CurrentArm.itemArmId);

					//result.Remove(CurrArm);

					CurrArm.Delete();
					CurrArm = CurrArm.Save();

					return Ok(result);
				}
				else
				{
					return Forbid();
				}
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Error when Deleting an arm: {0}", CurrentArm.itemArmId);
				return StatusCode(500, e.Message);
			}
		}


		// READ Links
		[HttpPost("[action]")]
		public IActionResult GetLinkLists([FromBody] SingleInt64Criteria ItemIDCrit)
		{

			try
			{
				if (!SetCSLAUser()) return Unauthorized();
				ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
				SingleCriteria<Item, Int64> criteria = new SingleCriteria<Item, Int64>(ItemIDCrit.Value);
				ItemLinkLists result = new ItemLinkLists();
				result.links = DataPortal.Fetch<ItemLinkList>(criteria);
				result.links.OrderBy(x => x.ItemLinkId);
				return Ok(result.links);

			}
			catch (Exception e)
			{
				_logger.LogError(e, "Error when fetching LinkLists: {0}", ItemIDCrit.Value);
				return StatusCode(500, e.Message);
			}
		}


		// CREATE
		//adds an arm to the list and then calls data portal insert
		[HttpPost("[action]")]
		public IActionResult CreateItemLink([FromBody] ItemLinkJSON data)
		{

			try
			{
				if (SetCSLAUser4Writing())
				{
					//we'll check both IDs provided belong to the current review...
					SingleCriteria<Item, Int64> pc = new SingleCriteria<Item, Int64>(data.itemIdPrimary);
					Item primary = DataPortal.Fetch<Item>(pc);
					if (primary == null || primary.ItemId < 1) return StatusCode(500, "Bad input: no such item(s)");
					Item secondary = DataPortal.Fetch<Item>(new SingleCriteria<Item, Int64>(data.itemIdSecondary));
					if (secondary == null || secondary.ItemId < 1) return StatusCode(500, "Bad input: no such item(s)");
					//ok, we can create and save the link...
					ItemLink newLink = new ItemLink();
					newLink.ItemIdPrimary = data.itemIdPrimary;
					newLink.ItemIdSecondary = data.itemIdSecondary;
					newLink.Description = data.description;
					newLink = newLink.Save();
					//because it's cheap, we return the whole new list!
					ItemLinkList result = DataPortal.Fetch<ItemLinkList>(pc);
					return Ok(result);
				}
				else
				{
					return Forbid();
				}
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Error creating ItemLink : {0}, {1}", data.itemIdPrimary, data.itemIdSecondary);
				return StatusCode(500, e.Message);
			}
		}


		// UPDATE
		[HttpPost("[action]")]
		public IActionResult UpdateItemLink([FromBody] ItemLinkJSON data)
		{

			try
			{
				if (SetCSLAUser4Writing())
				{
					//SingleCriteria<Item, Int64> crit = 
					ItemLinkList list = DataPortal.Fetch<ItemLinkList>(new SingleCriteria<Item, Int64>(data.itemIdPrimary));
					if (list == null || list.Count == 0 || data.itemLinkId < 1) return StatusCode(500, "Bad input: no such ItemLink");
					ItemLink editing = list.FirstOrDefault(f=>f.ItemLinkId == data.itemLinkId);
					if (editing == null || editing.ItemLinkId != data.itemLinkId) return StatusCode(500, "Bad input: no such ItemLink");
					int i = list.IndexOf(editing);
					if (i ==-1 ) return StatusCode(500, "Bad input: no such ItemLink");

					editing.BeginEdit();
					editing.ItemIdSecondary = data.itemIdSecondary;
					editing.Description = data.description;
					editing.ApplyEdit();
					//editing = editing.Save();
					list.SaveItem(i);
					return Ok(list);
				}
				else
				{
					return Forbid();
				}
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Error in UpdateItemLink: {0}, {1}", data.itemIdPrimary, data.itemIdSecondary);
				return StatusCode(500, e.Message);
			}
		}

		// DELETE
		[HttpPost("[action]")]
		public IActionResult DeleteItemLink([FromBody] ItemLinkJSON data)
		{

			try
			{
				if (SetCSLAUser4Writing())
				{
					ItemLinkList list = DataPortal.Fetch<ItemLinkList>(new SingleCriteria<Item, Int64>(data.itemIdPrimary));
					if (list == null || list.Count == 0 || data.itemLinkId < 1) return StatusCode(500, "Bad input: no such ItemLink");
					ItemLink editing = list.FirstOrDefault(f => f.ItemLinkId == data.itemLinkId);
					if (editing == null || editing.ItemLinkId != data.itemLinkId) return StatusCode(500, "Bad input: no such ItemLink");
					int i = list.IndexOf(editing);
					if (i == -1) return StatusCode(500, "Bad input: no such ItemLink");

					editing.BeginEdit();
					editing.Delete();
					editing.ApplyEdit();
					list = DataPortal.Fetch<ItemLinkList>(new SingleCriteria<Item, Int64>(data.itemIdPrimary));
					return Ok(list);
				}
				else
				{
					return Forbid();
				}
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Error in DeleteItemLink: {0}, {1}", data.itemIdPrimary, data.itemIdSecondary);
				return StatusCode(500, e.Message);
			}
		}

	}

	public class ItemArmTimepointLinkLists
    {
		public ItemArmList arms { get; set; }
		public ItemTimepointList timePoints { get; set; }
		public ItemLinkList links { get; set; }
    }

	public class ItemLinkLists
	{
		public ItemLinkList links { get; set; }
	}

	public class TimePointJSON
	{
		public long itemId { get; set; }
		public string timepointValue { get; set; }
		public string timepointMetric = "";
		public long itemTimepointId { get; set; }

	}

	public class ItemTimepointDeleteWarningCommandJSON
	{
		public long itemId { get; set; }
		public long itemTimepointId { get; set; }
		public long numOutcomes { get; set; }

	}
	public class ArmJSON
	{
		public long key { get; set; }
		public long itemId { get; set; }
		public string title { get; set; }
		public long itemArmId { get; set; }
		public int ordering { get; set; }

	}

	public class ItemArmDeleteWarningCommandJSON
	{
		public long itemId { get; set; }
		public long itemArmId { get; set; }
		public long numCodings { get; set; }

	}

	public class ItemLinkJSON
    {
		public int itemLinkId { get; set; }
		public Int64 itemIdPrimary { get; set; }
		public Int64 itemIdSecondary { get; set; }

		public string title { get; set; }
		public string shortTitle { get; set; }
		public string description { get; set; }

	}
}
