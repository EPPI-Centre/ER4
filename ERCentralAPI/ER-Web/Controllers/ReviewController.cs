using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusinessLibrary.BusinessClasses;
using BusinessLibrary.Security;
using Csla;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EPPIDataServices.Helpers;

namespace ERxWebClient2.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class ReviewController : CSLAController
    {
        
        public ReviewController(ILogger<ReviewController> logger) : base(logger)
        { }

        [HttpPost("[action]")]
		public IActionResult CreateReview([FromBody] reviewJson data)
		{
			try
            {
                if (SetCSLAUser4Writing())
                {
				
					Review review = new Review(data.reviewName, data.userId);
					review = review.Save();
					return Ok(review.ReviewId);
				}
                else return Forbid();
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Reviews data portal error");
                return StatusCode(500, e.Message);
            }
        }


		[HttpGet("[action]")]
        public IActionResult ReadOnlyReviews()//should receive a reviewID!
        {

            try
            {
                if (!SetCSLAUser()) return Unauthorized();
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;

                DataPortal<ReadOnlyReviewList> dp = new DataPortal<ReadOnlyReviewList>();
                SingleCriteria<ReadOnlyReviewList, int> criteria = new SingleCriteria<ReadOnlyReviewList, int>(ri.UserId);
                ReadOnlyReviewList result = dp.Fetch(criteria);

                //ReadOnlyReviewList returnValue = new ReadOnlyReviewList();
                //Action<ReviewerIdentity, ReadOnlyReviewList> Action = new Action<ReviewerIdentity, ReadOnlyReviewList>(Doit);
                //Action.Invoke(ri, returnValue);

                //return returnValue;

                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogException(e, "ReadOnlyReviews data portal error");
                return StatusCode(500, e.Message);
            }
        }
        [HttpGet("[action]")]
        public IActionResult GetReadOnlyTemplateReviews()
        {
            try
            {
                if (!SetCSLAUser()) return Unauthorized();
                ReadOnlyTemplateReviewList res = new ReadOnlyTemplateReviewList();
                DataPortal<ReadOnlyTemplateReviewList> dp = new DataPortal<ReadOnlyTemplateReviewList>();
                res = dp.Fetch();
                return Ok(res);
            }
            catch (Exception e)
            {
                _logger.LogException(e, "GetReadOnlyTemplateReviews data portal error");
                return StatusCode(500, e.Message);
            }
        }
        [HttpGet("[action]")]
        public IActionResult ReadOnlyArchieReviews()//should receive a reviewID!
        {

            try
            {
                if (!SetCSLAUser()) return Unauthorized();
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                if (ri.IsCochraneUser)
                {
                    DataPortal<ReadOnlyArchieReviewList> dp = new DataPortal<ReadOnlyArchieReviewList>();

                    ReadOnlyArchieReviewList result = dp.Fetch();
                    if (result.Count == 0 && result.archieIdentity.Error != "")
                    {
                        
                        var rrr = new
                        {
                            error = result.archieIdentity.Error,
                            reason = result.archieIdentity.ErrorReason
                        };

                        return Ok(rrr);
                    }
                    else return Ok(result);
                }
                else return Forbid();
            }
            catch (Exception e)
            {
                _logger.LogException(e, "ReadOnlyArchieReviews error");
                return StatusCode(500, e.Message);
            }
        }
        [Authorize(Roles = "CochraneUser")]
        [HttpPost("[action]")]
        public IActionResult ArchieReviewPrepare([FromBody] SingleStringCriteria ArchieRevIDCrit)
        {//this also checks review out if needed.
            try
            {
                if (!SetCSLAUser()) return Unauthorized();
                
                ArchieReviewPrepareCommand cmd = new ArchieReviewPrepareCommand();
                cmd.ArchieReviewID = ArchieRevIDCrit.Value;
                DataPortal<ArchieReviewPrepareCommand> dp = new DataPortal<ArchieReviewPrepareCommand>();
                cmd = dp.Execute(cmd);
                return Ok(cmd);
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Error occured when Preparing (/checkout) ArchieReview ");
                return StatusCode(500, e.Message);
            }
        }
        [Authorize(Roles = "CochraneUser")]
        [HttpPost("[action]")]
        public IActionResult ArchieReviewUndoCheckout([FromBody] SingleStringCriteria ArchieRevIDCrit)
        {//this also checks review out if needed.
            try
            {
                if (!SetCSLAUser()) return Unauthorized();
                ArchieReviewUndoCheckoutCommand cmd = new ArchieReviewUndoCheckoutCommand();
                cmd.ArchieReviewID = ArchieRevIDCrit.Value;
                DataPortal<ArchieReviewUndoCheckoutCommand> dp = new DataPortal<ArchieReviewUndoCheckoutCommand>();
                cmd = dp.Execute(cmd);
                return Ok(cmd);
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Error occured when undoing Archie review Checkout");
                return StatusCode(500, e.Message);
            }
        }
    }

	public class reviewJson
	{
		public string reviewName { get; set; }
		public int userId { get; set; }
	}
}
