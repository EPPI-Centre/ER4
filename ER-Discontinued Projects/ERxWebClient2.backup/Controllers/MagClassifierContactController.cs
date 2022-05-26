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
    public class MagClassifierContactController : CSLAController
    {
        
		public MagClassifierContactController(ILogger<MagClassifierContactController> logger) : base(logger)
        { }

        [HttpGet("[action]")]
        public IActionResult FetchClassifierContactList()
        {
			try
            {
                if (!SetCSLAUser()) return Unauthorized();

                DataPortal<ClassifierContactModelList> dp = new DataPortal<ClassifierContactModelList>();
                ClassifierContactModelList result = dp.Fetch();

                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Fetching a ClassifierContactModel list has an error");
                return StatusCode(500, e.Message);
            }
		}

	
    }

}

