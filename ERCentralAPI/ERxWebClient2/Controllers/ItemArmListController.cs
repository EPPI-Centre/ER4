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
    public class ItemArmListController : CSLAController
    {

        private readonly ILogger _logger;

        public ItemArmListController(ILogger<ItemArmListController> logger)
        {
            _logger = logger;

        }
		 
		// READ
		[HttpPost("[action]")]
		public IActionResult GetArms([FromBody] SingleInt64Criteria ItemIDCrit)
		{

			try
			{
				SetCSLAUser();
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
				_logger.LogError(e, "Error when updating an arm: {0}", data.itemArmId);
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

}
