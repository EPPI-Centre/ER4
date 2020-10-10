using System;
using BusinessLibrary.BusinessClasses;
using Csla;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using EPPIDataServices.Helpers;
using System.Linq;

namespace ERxWebClient2.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class MagReviewListController : CSLAController
    {

        public MagReviewListController(ILogger<MagReviewListController> logger) : base(logger)
        { }


        [HttpGet("[action]")]
        public IActionResult GetMagReviewList()
        {
            try
            {
                if (SetCSLAUser() && User.HasClaim(cl => cl.Type == "isSiteAdmin" && cl.Value == "True"))
                {
                    DataPortal<MagReviewList> dp = new DataPortal<MagReviewList>();
                    var result = dp.Fetch();
                    return Ok(result);
                }
                else return Unauthorized();
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Getting a Mag Review List has an error");
                throw;
            }
        }

        [HttpPost("[action]")]
        public IActionResult AddReviewToMagList([FromBody] SingleIntCriteria reviewId)
        {

            try
            {
                if (SetCSLAUser4Writing() && User.HasClaim(cl => cl.Type == "isSiteAdmin" && cl.Value == "True"))
                {
                    MagReview mr = new MagReview();
                    mr.ReviewId = reviewId.Value;
                    mr.Name = "adding review...";

                    DataPortal<MagReviewList> dp2 = new DataPortal<MagReviewList>();
                    var magReviewList = dp2.Fetch();
                    magReviewList.Add(mr);
                    magReviewList.SaveItem(mr);

                    return Ok(mr);
                }

                else return Forbid();
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Adding a review to a Mag Review List has an error");
                throw;
            }
        }

        [HttpPost("[action]")]
        public IActionResult DeleteReview([FromBody] SingleIntCriteria reviewId)
        {
            try
            {
                if (SetCSLAUser4Writing() && User.HasClaim(cl => cl.Type == "isSiteAdmin" && cl.Value == "True"))
                {

                    DataPortal<MagReviewList> dp2 = new DataPortal<MagReviewList>();
                    var magReviewList = dp2.Fetch();

                    var mr = magReviewList.Where(x => x.ReviewId == reviewId.Value).FirstOrDefault();
                    if (mr != null)
                    {
                        magReviewList.Remove(mr);

                    }
                    return Ok();
                }

                else return Forbid();
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Deleting a review from a Mag Review List has an error");
                throw;
            }
        }
    }
}

