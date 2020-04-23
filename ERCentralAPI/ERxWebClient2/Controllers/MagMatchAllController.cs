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
    public class MagMatchAllController : CSLAController
    {

        private readonly ILogger _logger;

		public MagMatchAllController(ILogger<MagMatchAllController> logger)
        {

            _logger = logger;
        }

        [HttpGet("[action]")]
        public IActionResult RunMatchingAlgorithm()
        {
			try
            {
                SetCSLAUser();

                DataPortal<MagMatchItemsToPapersCommand> dp = new DataPortal<MagMatchItemsToPapersCommand>();
                MagMatchItemsToPapersCommand GetMatches = new MagMatchItemsToPapersCommand("FindMatches",
                   true, 0, 0);
                GetMatches = dp.Execute(GetMatches);

                return Ok(GetMatches.currentStatus);
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Getting a MagRelatedPapersRunes list has an error");
                throw;
            }
		}
    }

}

