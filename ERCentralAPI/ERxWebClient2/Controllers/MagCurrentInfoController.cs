using System;
using BusinessLibrary.BusinessClasses;
using Csla;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using EPPIDataServices.Helpers;

namespace ERxWebClient2.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class MagCurrentInfoController : CSLAController
    {

        private readonly ILogger _logger;

		public MagCurrentInfoController(ILogger<MagCurrentInfoController> logger)
        {

            _logger = logger;
        }

        [HttpGet("[action]")]
        public IActionResult GetMagCurrentInfo()
        {
			try
            {
                SetCSLAUser();

                DataPortal<MagCurrentInfo> dp = new DataPortal<MagCurrentInfo>();
				MagCurrentInfo result = dp.Fetch();

                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Getting a MagCurrentInfo has an error");
                throw;
            }
		}

        [HttpGet("[action]")]
        public IActionResult GetMagReviewMagInfo()
        {
            try
            {
                SetCSLAUser();
                MAgReviewMagInfoCommand cmd = new MAgReviewMagInfoCommand();
                DataPortal<MAgReviewMagInfoCommand> dp = new DataPortal<MAgReviewMagInfoCommand>();
                cmd = dp.Execute(cmd);

                return Ok(cmd);
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Getting a MagReviewMagInfo Command has an error");
                throw;
            }
        }
    }
}

