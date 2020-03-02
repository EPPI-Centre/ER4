using System;
using BusinessLibrary.BusinessClasses;
using BusinessLibrary.Security;
using Csla;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using EPPIDataServices.Helpers;

namespace ERxWebClient2.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class MagRelatedPapersRunListController : CSLAController
    {

        private readonly ILogger _logger;

		public MagRelatedPapersRunListController(ILogger<MagRelatedPapersRunListController> logger)
        {

            _logger = logger;
        }

        [HttpGet("[action]")]
        public IActionResult GetMagRelatedPapersRuns()
        {
			try
            {
                SetCSLAUser();

                DataPortal<MagRelatedPapersRunList> dp = new DataPortal<MagRelatedPapersRunList>();
				MagRelatedPapersRunList result = dp.Fetch();

                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Getting a MagRelatedPapersRunes list has an error");
                throw;
            }
		}

		[HttpPost("[action]")]
		public IActionResult DeleteMagRelatedPapersRun(MVCMagRelatedPapersRun magRelatedPapersRun)
		{
			try
			{
				SetCSLAUser4Writing();

				DataPortal<MagRelatedPapersRunList> dp = new DataPortal<MagRelatedPapersRunList>();
				MagRelatedPapersRunList result = dp.Fetch();

				//var magRun = result.


				//result.Remove(magRelatedPapersRun);
				
				return Ok();
			}
			catch (Exception e)
			{
				_logger.LogException(e, "Getting a MagRelatedPapersRunes list has an error");
				throw;
			}
		}

	}


	public class MVCMagRelatedPapersRun
	{

		public int magRelatedRunId = 0;
		public string userDescription = "";
		public string paperIdList = "";
		public int attributeId = 0;
		public string allIncluded = "";
		public string dateFrom = "";
		public string autoReRun = "";
		public string mode = "";
		public string filtered = "";
		public string status = "";
		public string userStatus = "";
		public long nPapers = 0;
	}

}

