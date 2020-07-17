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
        private readonly ILogger _logger;

		public MagReviewListController(ILogger<MagReviewListController> logger)
        {

            _logger = logger;
        }


        [HttpGet("[action]")]
        public IActionResult GetMagReviewList()
        {
            try
            {
                SetCSLAUser();

                DataPortal<MagReviewList> dp = new DataPortal<MagReviewList>();

                var result = dp.Fetch();

                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Getting a MagReviewList has an error");
                throw;
            }
        }

        [HttpPost("[action]")]
        public IActionResult AddReviewToMagList([FromBody] SingleIntCriteria reviewId)
        {

            try
            {
                SetCSLAUser4Writing();

                MagReview mr = new MagReview();
                mr.ReviewId = reviewId.Value;
                mr.Name = "adding review...";

                DataPortal<MagReviewList> dp2 = new DataPortal<MagReviewList>();
                var magReviewList = dp2.Fetch();
                magReviewList.Add(mr);
                magReviewList.SaveItem(mr);

                return Ok(mr);
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Adding a review to a MagReviewList has an error");
                throw;
            }
        }

        [HttpPost("[action]")]
        public IActionResult DeleteReview([FromBody] SingleIntCriteria reviewId)
        {
            try
            {
                SetCSLAUser4Writing();


                DataPortal<MagReviewList> dp2 = new DataPortal<MagReviewList>();
                var magReviewList = dp2.Fetch();

                var mr = magReviewList.Where(x => x.ReviewId == reviewId.Value).FirstOrDefault();
                magReviewList.Remove(mr);

                return Ok();
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Deleting a review from a MagReviewList has an error");
                throw;
            }
        }
    }
}

