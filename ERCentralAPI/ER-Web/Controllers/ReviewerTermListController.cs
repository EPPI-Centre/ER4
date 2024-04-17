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
    public class ReviewerTermListController : CSLAController
    {
        
        public ReviewerTermListController(ILogger<ReviewerTermListController> logger) : base(logger)
        { }

        [HttpGet("[action]")]
        public IActionResult Fetch()
        {
            try
            {
                if (!SetCSLAUser()) return Unauthorized();
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                DataPortal<TrainingReviewerTermList> dp = new DataPortal<TrainingReviewerTermList>();
                TrainingReviewerTermList result = dp.Fetch();
                return Ok(result);
            }
            catch(Exception e)
            {
                _logger.LogException(e, "Error with TrainingReviewerTermList");
                return StatusCode(500, e.Message);
            }
        }


		[HttpPost("[action]")]
		public IActionResult CreateReviewerTerm([FromBody] TrainingReviewerTermJSON data)
		{
			try
			{
                if (SetCSLAUser4Writing())
                {
                    ReviewerIdentity ri = ApplicationContext.User.Identity as ReviewerIdentity;
					DataPortal<TrainingReviewerTerm> dp = new DataPortal<TrainingReviewerTerm>();
					TrainingReviewerTerm cmd = new TrainingReviewerTerm
					{
						Included = data.included,
						Term = data.term,
						ReviewerTerm = data.term
					};
					cmd = dp.Execute(cmd);

					return Ok(cmd);
				}
                else return Forbid();
            }
			catch (Exception e)
			{
				_logger.LogException(e, "Error with creating TrainingReviewerTerm");
				return StatusCode(500, e.Message);
			}
		}

		[HttpPost("[action]")]
		public IActionResult DeleteReviewerTerm([FromBody] SingleStringCriteria termId)
		{
			try
			{
				if (SetCSLAUser4Writing())
				{
					ReviewerIdentity ri = ApplicationContext.User.Identity as ReviewerIdentity;
					DataPortal<TrainingReviewerTermList> dp = new DataPortal<TrainingReviewerTermList>();
					TrainingReviewerTermList result = dp.Fetch();
					TrainingReviewerTerm cmd = new TrainingReviewerTerm();
					cmd = result.FirstOrDefault(x => x.TrainingReviewerTermId == Convert.ToInt32(termId.Value));

					cmd.Delete();
					cmd = cmd.Save();

					return Ok(termId.Value);
				}
                else return Forbid();
            }
			catch (Exception e)
			{
				_logger.LogException(e, "Error with deleting TrainingReviewerTerm");
				return StatusCode(500, e.Message);
			}
		}


		[HttpPost("[action]")]
		public IActionResult UpdateReviewerTerm([FromBody] TrainingReviewerTermJSON data)
		{
			try
			{
				if (SetCSLAUser4Writing())
				{
					ReviewerIdentity ri = ApplicationContext.User.Identity as ReviewerIdentity;
					DataPortal<TrainingReviewerTermList> dp = new DataPortal<TrainingReviewerTermList>();
					TrainingReviewerTermList result = dp.Fetch();
					TrainingReviewerTerm cmd = new TrainingReviewerTerm();
					cmd = result.FirstOrDefault(x => x.TrainingReviewerTermId == data.trainingReviewerTermId);

					cmd.ReviewerTerm = data.reviewerTerm;
					cmd.Included = data.included;
					cmd = cmd.Save();

					return Ok(cmd);
				}
                else return Forbid();
            }
			catch (Exception e)
			{
				_logger.LogException(e, "Error with updating TrainingReviewerTerm");
				return StatusCode(500, e.Message);
			}
		}



	}

	public class TrainingReviewerTermJSON
	{
		public int trainingReviewerTermId { get; set; }
		public int itemTermDictionaryId { get; set; }
		public string reviewerTerm { get; set; }
		public bool included { get; set; }
		public string term { get; set; }
	}

}
