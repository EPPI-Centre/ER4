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
using Newtonsoft.Json.Linq;
using EPPIDataServices.Helpers;
using Newtonsoft.Json;

namespace ERxWebClient2.Controllers
{
	[Authorize]
	[Route("api/[controller]")]
	public class ReviewStatisticsController : CSLAController
	{

		private readonly ILogger _logger;

		public ReviewStatisticsController(ILogger<ReviewController> logger)
		{

			_logger = logger;
		}


		[HttpGet("[action]")]
		public IActionResult ExcecuteReviewStatisticsCountCommand()
		{
			try
			{
				SetCSLAUser();
				ReviewStatisticsCountsCommand cmd = new ReviewStatisticsCountsCommand();
				DataPortal<ReviewStatisticsCountsCommand> dp = new DataPortal<ReviewStatisticsCountsCommand>();
				cmd = dp.Execute(cmd);

				return Ok(cmd);

			}
			catch (Exception e)
			{
				ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
				_logger.LogError(e, "Dataportal Error for Review Statistics RevID: {0}", ri.ReviewId);
				throw;
			}
		}


		[HttpPost("[action]")]
		public IActionResult FetchCounts([FromBody]  SingleCriteria<bool> crit)
		{
			try
			{
				SetCSLAUser();
				ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;

				DataPortal<ReviewStatisticsCodeSetList> dp = new DataPortal<ReviewStatisticsCodeSetList>();

				ReviewStatisticsCodeSetList result = dp.Fetch(new SingleCriteria<ReviewStatisticsCodeSetList, bool>(crit.Value));

				//return Json(result);
				return Ok(result);
			}
			catch (Exception e)
			{
				string json = JsonConvert.SerializeObject("");
				_logger.LogError(e, "Fetching criteria: {0}", json);
				return StatusCode(500, e.Message);
			}
		}

		[HttpPost("[action]")]
		public IActionResult ExcecuteItemSetBulkCompleteCommand([FromBody]  MVCItemSetBulkCompleteCommand MVCcmd)
		{
			try
			{
				SetCSLAUser();
				
				ItemSetBulkCompleteCommand cmd = new ItemSetBulkCompleteCommand(
					MVCcmd.setID, MVCcmd.contactID, MVCcmd.completeOrNot);
				DataPortal<ItemSetBulkCompleteCommand> dp = new DataPortal<ItemSetBulkCompleteCommand>();
				cmd = dp.Execute(cmd);

				return Ok(cmd);

			}
			catch (Exception e)
			{
				ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
				_logger.LogError(e, "Dataportal Error for Review Statistics RevID: {0}", ri.ReviewId);
				throw;
			}
		}


		[HttpPost("[action]")]
		public IActionResult PreviewCompleteUncompleteCommand([FromBody]  MVCBulkCompleteUncompleteCommand MVCcmd)
		{
			try
			{
				SetCSLAUser();

				BulkCompleteUncompleteCommand cmd = new BulkCompleteUncompleteCommand(
					MVCcmd.attributeId, MVCcmd.isCompleting, MVCcmd.setId, MVCcmd.reviewerId, MVCcmd.isPreview);

				DataPortal<BulkCompleteUncompleteCommand> dp = new DataPortal<BulkCompleteUncompleteCommand>();
				cmd = dp.Execute(cmd);

				return Ok(cmd);

			}
			catch (Exception e)
			{
				ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
				_logger.LogError(e, "Dataportal Error for Review Statistics RevID: {0}", ri.ReviewId);
				throw;
			}
		}

		public class MVCBulkCompleteUncompleteCommand
		{

			public Int64 attributeId { get; set; }
			public bool isCompleting { get; set; }
			public int setId { get; set; }
			public int reviewerId { get; set; }
			public bool isPreview { get; set; }

		}

		public class MVCItemSetBulkCompleteCommand
		{

			public bool completeOrNot { get; set; }
			public int setID { get; set; }
			public int contactID { get; set; }

		}
	}
}
