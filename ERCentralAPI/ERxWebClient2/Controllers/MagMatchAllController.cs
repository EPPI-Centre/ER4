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

        [HttpPost("[action]")]
        public IActionResult RunMatchingAlgorithm([FromBody] SingleInt64Criteria attributeId)
        {
			try
            {
                SetCSLAUser();

                DataPortal<MagMatchItemsToPapersCommand> dp = new DataPortal<MagMatchItemsToPapersCommand>();
                MagMatchItemsToPapersCommand GetMatches = new MagMatchItemsToPapersCommand("FindMatches",
                   true, 0, attributeId.Value);

                GetMatches = dp.Execute(GetMatches);

                return Ok(GetMatches.currentStatus);
            }
            catch (Exception e)
            {
                _logger.LogException(e, "RunMatchingAlgorithm has an error");
                throw;
            }
		}

        //call from itemdetails tab...
        [HttpPost("[action]")]
        public IActionResult MagMatchItemsToPapers([FromBody] SingleInt64Criteria itemId)
        {
            try
            {
                SetCSLAUser();

                DataPortal<MagMatchItemsToPapersCommand> dp = new DataPortal<MagMatchItemsToPapersCommand>();
                MagMatchItemsToPapersCommand GetMatches = new MagMatchItemsToPapersCommand("FindMatches",
                   false, itemId.Value, 0);

                GetMatches = dp.Execute(GetMatches);

                return Ok(GetMatches);
            }
            catch (Exception e)
            {
                _logger.LogException(e, "MagMatchItemsToPapers has an error");
                throw;
            }
        }
    }

}

