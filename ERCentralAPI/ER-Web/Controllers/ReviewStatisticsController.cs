using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
	public class ReviewStatisticsController : CSLAController
	{
        
		public ReviewStatisticsController(ILogger<ReviewController> logger) : base(logger)
        { }


        [HttpGet("[action]")]
		public IActionResult ExcecuteReviewStatisticsCountCommand()
		{
			try
			{
				if (!SetCSLAUser()) return Unauthorized();
				ReviewStatisticsCountsCommand cmd = new ReviewStatisticsCountsCommand();
				DataPortal<ReviewStatisticsCountsCommand> dp = new DataPortal<ReviewStatisticsCountsCommand>();
				cmd = dp.Execute(cmd);

				return Ok(cmd);

			}
			catch (Exception e)
			{
				ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
				_logger.LogError(e, "Dataportal Error for Review Statistics RevID: {0}", ri.ReviewId);
				return StatusCode(500, e.Message);
			}
		}


		[HttpPost("[action]")]
		public IActionResult FetchCounts([FromBody]  SingleCriteria<bool> crit)
		{
			try
			{
				if (!SetCSLAUser()) return Unauthorized();
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

        [HttpGet("[action]")]
        public IActionResult FetchAllCounts()
        {
            try
            {
                if (!SetCSLAUser()) return Unauthorized();
                
                DataPortal<ReviewStatisticsCodeSetList2> dp = new DataPortal<ReviewStatisticsCodeSetList2>();

                ReviewStatisticsCodeSetList2 result = dp.Fetch();

                //return Json(result);
                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error in FetchAllCounts");
                return StatusCode(500, e.Message);
            }
        }

        [HttpPost("[action]")]
		public IActionResult ExcecuteItemSetBulkCompleteCommand([FromBody]  MVCItemSetBulkCompleteCommand MVCcmd)
		{
			try
			{
				if (!SetCSLAUser()) return Unauthorized();
				
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
				return StatusCode(500, e.Message);
			}
		}


		[HttpPost("[action]")]
		public IActionResult PreviewCompleteUncompleteCommand([FromBody]  MVCBulkCompleteUncompleteCommand MVCcmd)
		{
			try
			{
                if (SetCSLAUser4Writing())
                {
                    ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                    if (ri.Roles.Contains("AdminUser"))
                    {
                        BulkCompleteUncompleteCommand cmd = new BulkCompleteUncompleteCommand(
                            MVCcmd.attributeId, MVCcmd.isCompleting, MVCcmd.setId, MVCcmd.reviewerId, MVCcmd.isPreview);
                        DataPortal<BulkCompleteUncompleteCommand> dp = new DataPortal<BulkCompleteUncompleteCommand>();
                        cmd = dp.Execute(cmd);
                        return Ok(cmd);
                    }
                    else return Forbid();
                }
                else return Forbid();
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Dataportal Error for PreviewCompleteUncompleteCommand.");
				return StatusCode(500, e.Message);
			}
		}
        [HttpPost("[action]")]
        public IActionResult BulkDeleteCodingCommand([FromBody] MVCBulkDeleteCodingCommand MVCcmd)
        {
            try
            {
                if (SetCSLAUser4Writing())
                {
                    ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                    if (ri.Roles.Contains("AdminUser"))
                    {
                        BulkDeleteCodingCommand cmd = new BulkDeleteCodingCommand(MVCcmd.setId, MVCcmd.reviewerId, MVCcmd.isPreview);
                        DataPortal<BulkDeleteCodingCommand> dp = new DataPortal<BulkDeleteCodingCommand>();
                        cmd = dp.Execute(cmd);
                        return Ok(cmd);
                    }
                    else return Forbid();
                }
                else return Forbid();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Dataportal Error for BulkDeleteCodingCommand.");
                return StatusCode(500, e.Message);
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
        public class MVCBulkDeleteCodingCommand
        {
            public int setId { get; set; }
            public int reviewerId { get; set; }
            public bool isPreview { get; set; }
            public int totalItemsAffected { get; set; }
            public int completedCodingToBeDeleted { get; set; }
            public int incompletedCodingToBeDeleted { get; set; }
            public int hiddenIncompletedCodingToBeDeleted { get; set; }

        }

        public class MVCItemSetBulkCompleteCommand
		{

			public bool completeOrNot { get; set; }
			public int setID { get; set; }
			public int contactID { get; set; }

		}
	}
}
