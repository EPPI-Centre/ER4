using System;
using BusinessLibrary.BusinessClasses;
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
                if (SetCSLAUser4Writing())
                {

                    DataPortal<MagMatchItemsToPapersCommand> dp = new DataPortal<MagMatchItemsToPapersCommand>();
                MagMatchItemsToPapersCommand GetMatches = new MagMatchItemsToPapersCommand("FindMatches",
                   true, 0, attributeId.Value);

                GetMatches = dp.Execute(GetMatches);

                return Ok(GetMatches.currentStatus);

                }

                else return Forbid();
            }
            catch (Exception e)
            {
                _logger.LogException(e, "RunMatchingAlgorithm has an error");
                throw;
            }
		}

       
        [HttpPost("[action]")]
        public IActionResult MagMatchItemsToPapers([FromBody] SingleInt64Criteria itemId)
        {
            try
            {
                if (SetCSLAUser4Writing())
                {

                    DataPortal<MagMatchItemsToPapersCommand> dp = new DataPortal<MagMatchItemsToPapersCommand>();
                    MagMatchItemsToPapersCommand GetMatches = new MagMatchItemsToPapersCommand("FindMatches",
                       false, itemId.Value, 0);

                    GetMatches = dp.Execute(GetMatches);

                    return Ok(GetMatches);

                }

                else return Forbid();
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Matching MAG Items To Papers has an error");
                throw;
            }
        }


        [HttpPost("[action]")]
        public IActionResult ClearMagMatchItemsToPapers([FromBody] SingleInt64Criteria itemId)
        {
            try
            {
                if (SetCSLAUser4Writing())
                {

                    DataPortal<MagMatchItemsToPapersCommand> dp = new DataPortal<MagMatchItemsToPapersCommand>();
                    MagMatchItemsToPapersCommand GetMatches = new MagMatchItemsToPapersCommand("Clear",
                       false, itemId.Value, 0);

                    GetMatches = dp.Execute(GetMatches);

                    return Ok(GetMatches.currentStatus);

                }

                else return Forbid();
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Clearing MAG MatchItemsToPapers has an error");
                throw;
            }
        }

        [HttpPost("[action]")]
        public IActionResult ClearAllMAGMatches([FromBody] SingleInt64Criteria attributeId)
        {
            try
            {
                if (SetCSLAUser4Writing())
                {

                    DataPortal<MagMatchItemsToPapersCommand> dp = new DataPortal<MagMatchItemsToPapersCommand>();
                     MagMatchItemsToPapersCommand GetMatches = new MagMatchItemsToPapersCommand("Clear",
                   true, 0, attributeId.Value);

                    GetMatches = dp.Execute(GetMatches);

                    return Ok(GetMatches.currentStatus);

                }

                else return Forbid();
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Clearing ALL MAG MatchItemsToPapers has an error");
                throw;
            }
        }


    }

}

