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
using Newtonsoft.Json;

namespace ERxWebClient2.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class ReviewInfoController : CSLAController
    {
        
        public ReviewInfoController(ILogger<ReviewInfoController> logger) : base(logger)
        { }

        [HttpGet("[action]")]
        public IActionResult ReviewInfo()
        {

            try
            {

                if (!SetCSLAUser()) return Unauthorized();
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;

                DataPortal<ReviewInfo> dp = new DataPortal<ReviewInfo>();

                ReviewInfo result = dp.Fetch();

                return Ok(result);

            }
            catch (Exception e)
            {
                _logger.LogException(e, "A user idenity issue");
                return StatusCode(500, e.Message);
            }
        }

		[HttpGet("[action]")]
		public IActionResult ReviewMembers()
		{

			try
			{

				if (!SetCSLAUser()) return Unauthorized();
				ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;

				DataPortal<ReviewContactList> dp = new DataPortal<ReviewContactList>();

				ReviewContactList result = dp.Fetch();

				return Ok(result);
				
			}
			catch (Exception e)
			{
				_logger.LogException(e, "A ReviewContactList issue");
				return StatusCode(500, e.Message);
			}
		}
        [HttpPost("[action]")]
        public IActionResult UpdateReviewInfo([FromBody] ReviewInfoMVC revinfo)
        {
            try
            {
                if (SetCSLAUser4Writing())
                {
                    ReviewInfo result = DataPortal.Fetch<ReviewInfo>();
                    result.ShowScreening = revinfo.showScreening;
                    result.ScreeningCodeSetId = revinfo.screeningCodeSetId;
                    result.ScreeningMode = revinfo.screeningMode;
                    result.ScreeningReconcilliation = revinfo.screeningReconcilliation;
                    result.ScreeningWhatAttributeId = revinfo.screeningWhatAttributeId;
                    result.ScreeningNPeople = revinfo.screeningNPeople;
                    result.ScreeningAutoExclude = revinfo.screeningAutoExclude;
                    result.ScreeningIndexed = revinfo.screeningIndexed;

                    //screeningModelRunning;
                    //screeningIndexed;
                    //screeningListIsGood;
                    result = result.Save(true);
                    return Ok(result);
                }
                else return Forbid();
            }
            catch (Exception e)
            {
                string json = JsonConvert.SerializeObject(revinfo);
                _logger.LogError(e, "Dataportal Error with updating ReviewInfo: {0}", json);
                return StatusCode(500, e.Message);
            }
        }
    }
    public class ReviewInfoMVC
    {
        public int reviewId { get; set; }
        public string reviewName { get; set; }
        public bool showScreening { get; set; }
        public int screeningCodeSetId { get; set; }
        public string screeningMode { get; set; }
        public string screeningReconcilliation { get; set; }
        public long screeningWhatAttributeId { get; set; }
        public int screeningNPeople { get; set; }
        public bool screeningAutoExclude { get; set; }
        public bool screeningModelRunning { get; set; }
        public bool screeningIndexed { get; set; }
        public bool screeningListIsGood { get; set; }
        public string bL_ACCOUNT_CODE { get; set; }
        public string bL_AUTH_CODE { get; set; }
        public string bL_TX { get; set; }
        public string bL_CC_ACCOUNT_CODE { get; set; }
        public string bL_CC_AUTH_CODE { get; set; }
        public string bL_CC_TX { get; set; }
        public int magEnabled { get; set; }
    }
}