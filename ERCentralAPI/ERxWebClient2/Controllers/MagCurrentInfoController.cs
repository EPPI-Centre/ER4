using System;
using BusinessLibrary.BusinessClasses;
using BusinessLibrary.Security;
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

                DataPortal<MagCurrentInfo > dp = new DataPortal<MagCurrentInfo>();
				MagCurrentInfo result = dp.Fetch();

                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Getting a MagRelatedPapersRunes list has an error");
                throw;
            }
		}

	

		
	}

}

