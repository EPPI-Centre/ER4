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
    public class MagSearchListController : CSLAController
    {
        
		public MagSearchListController(ILogger<MagSearchListController> logger) : base(logger)
        { }

        [HttpGet("[action]")]
        public IActionResult FetchMagSearchList()
        {
			try
            {
                if (!SetCSLAUser()) return Unauthorized();

                DataPortal<MagSearchList> dp = new DataPortal<MagSearchList>();
                MagSearchList result = dp.Fetch();

                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Fetching a MagSearch list has an error");
                return StatusCode(500, e.Message);
            }
		}

	
    }

}

