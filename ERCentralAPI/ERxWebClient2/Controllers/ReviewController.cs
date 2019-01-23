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
    public class ReviewController : CSLAController
    {

        private readonly ILogger _logger;

        public ReviewController(ILogger<ReviewController> logger)
        {

            _logger = logger;
        }

		[HttpPost("[action]")]
		public IActionResult CreateReview([FromBody] reviewJson data)
		{
			if (SetCSLAUser4Writing())
			{
				try
				{
					Review review = new Review(data.reviewName, data.userId);

					review.Saved += (o, e2) =>
					{
						if (e2.NewObject != null)
						{
							Review rv = e2.NewObject as Review;
							//ReviewSelected.Invoke(this, new ReviewSelectedEventArgs(rv.ReviewId, rv.ReviewName));
						}
					};


					review.BeginSave();

					return Ok(review.ReviewId);
				}
				catch (Exception e)
				{
					_logger.LogException(e, "Reviews data portal error");
					throw;
				}
			}
			else
				return Forbid();
		}


		[HttpGet("[action]")]
        public IActionResult ReadOnlyReviews()//should receive a reviewID!
        {

            try
            {
                SetCSLAUser();
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
                throw;
            }
        }
        [HttpGet("[action]")]
        public IActionResult GetReadOnlyTemplateReviews()
        {
            try
            {
                SetCSLAUser();
                ReadOnlyTemplateReviewList res = new ReadOnlyTemplateReviewList();
                DataPortal<ReadOnlyTemplateReviewList> dp = new DataPortal<ReadOnlyTemplateReviewList>();
                res = dp.Fetch();
                return Ok(res);
            }
            catch (Exception e)
            {
                _logger.LogException(e, "GetReadOnlyTemplateReviews data portal error");
                throw;
            }
        }

	}

	public class reviewJson
	{
		public string reviewName { get; set; }
		public int userId { get; set; }
	}
}
