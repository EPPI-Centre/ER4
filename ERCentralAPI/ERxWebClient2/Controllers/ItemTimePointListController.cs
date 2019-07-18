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
    public class ItemTimepointListController : CSLAController
    {

        private readonly ILogger _logger;

        public ItemTimepointListController(ILogger<ItemTimepointListController> logger)
        {
            _logger = logger;

        }
		 
		// READ
		[HttpPost("[action]")]
		public IActionResult GetTimePoints([FromBody] SingleInt64Criteria ItemIDCrit)
		{

			try
			{
				SetCSLAUser();
				ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
				DataPortal<ItemTimepointList> dp = new DataPortal<ItemTimepointList>();
				SingleCriteria<Item, Int64> criteria = new SingleCriteria<Item, Int64>(ItemIDCrit.Value);
				ItemTimepointList result = dp.Fetch(criteria);
				result.OrderBy(x => x.ItemTimepointId).ToList();
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
                    ItemTimepoint result = newTimePoint.Save();

                    return Ok(result);
                }
                else
                {
                    return Forbid();
                }
            }
			catch (Exception e)
			{
				_logger.LogError(e, "Error when Creating an TimePoint : {0}", data.itemTimepointId);
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
				_logger.LogError(e, "Error when updating an TimePoint: {0}", data.itemTimepointId);
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
                    ItemTimepointDeleteWarningCommand command = new ItemTimepointDeleteWarningCommand(TimePointJSON.itemId, TimePointJSON.ItemTimepointId);
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
				_logger.LogError(e, "Error when delete warning is called: {0}", TimePointJSON.ItemTimepointId);
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

                    //DataPortal<ItemTimepointDeleteWarningCommand> dp = new DataPortal<ItemTimepointDeleteWarningCommand>();
                    //ItemTimepointDeleteWarningCommand command = new ItemTimepointDeleteWarningCommand(CurrentTimePoint.itemId, CurrentTimePoint.ItemTimepointId);
                    //ItemTimepointDeleteWarningCommand res = dp.Execute(command);

                    //dp.Execute(command);

                    // Actually need to end up with ItemTimepoint  currentTimePoint.Delete()


                    DataPortal<ItemTimepointList> dp = new DataPortal<ItemTimepointList>();
                    SingleCriteria<Item, Int64> criteria = new SingleCriteria<Item, Int64>(CurrentTimePoint.itemId);
                    ItemTimepointList result = dp.Fetch(criteria);

                    ItemTimepoint CurrTimePoint = result.FirstOrDefault(x => x.ItemTimepointId == CurrentTimePoint.itemTimepointId);

                    //result.Remove(CurrTimePoint);

                    CurrTimePoint.Delete();
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
		public long ItemTimepointId { get; set; }
		public long numOutcomes { get; set; }

	}
}
