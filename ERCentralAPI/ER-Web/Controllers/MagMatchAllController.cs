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
		public MagMatchAllController(ILogger<MagMatchAllController> logger) : base(logger)
        { }

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

                    return Ok(GetMatches);
                }
                else return Forbid();

            }
            catch (Exception e)
            {
                _logger.LogException(e, "RunMatchingAlgorithm has an error");
                return StatusCode(500, e.Message);
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
                _logger.LogException(e, "MagMatchItemsToPapers has an error");
                return StatusCode(500, e.Message);
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
                    MagMatchItemsToPapersCommand GetMatches = new MagMatchItemsToPapersCommand("ALL",
                       false, itemId.Value, 0);

                    GetMatches = dp.Execute(GetMatches);

                    return Ok(GetMatches.currentStatus);

                }
                else return Forbid();

            }
            catch (Exception e)
            {
                _logger.LogException(e, "MagMatchItemsToPapers has an error");
                return StatusCode(500, e.Message);
            }
        }

        [HttpPost("[action]")]
        public IActionResult ClearAllMAGMatches([FromBody] SingleInt64Criteria attributeId)
        {
            try
            {
                if (SetCSLAUser4Writing())
                {
                    string filter = "";
                    if (attributeId.Value == 0)
                    {
                        filter = "ALL";
                    }
                    else
                    {
                        filter = "ALL WITH THIS CODE";
                    }
                    DataPortal<MagMatchItemsToPapersCommand> dp = new DataPortal<MagMatchItemsToPapersCommand>();
                    MagMatchItemsToPapersCommand GetMatches = new MagMatchItemsToPapersCommand(filter,
                  true, 0, attributeId.Value);

                    GetMatches = dp.Execute(GetMatches);

                    return Ok(GetMatches.currentStatus);

                }
                else return Forbid();

            }
            catch (Exception e)
            {
                _logger.LogException(e, "MagMatchItemsToPapers has an error");
                return StatusCode(500, e.Message);
            }
        }

        [HttpPost("[action]")]
        public IActionResult ClearAllNonManualMAGMatches([FromBody] SingleInt64Criteria attributeId)
        {
            try
            {
                if (SetCSLAUser4Writing())
                {
                    string filter = "";
                    if (attributeId.Value == 0)
                    {
                        filter = "ALL NON-MANUAL";
                    }
                    else
                    {
                        filter = "ALL NON-MANUAL WITH THIS CODE";
                    }
                    DataPortal<MagMatchItemsToPapersCommand> dp = new DataPortal<MagMatchItemsToPapersCommand>();
                    MagMatchItemsToPapersCommand GetMatches = new MagMatchItemsToPapersCommand(filter,
                  true, 0, attributeId.Value);

                    GetMatches = dp.Execute(GetMatches);

                    return Ok(GetMatches.currentStatus);
                }
                else return Forbid();

            }
            catch (Exception e)
            {
                _logger.LogException(e, "MagMatchItemsToPapers has an error");
                return StatusCode(500, e.Message);
            }
        }

    }

}

