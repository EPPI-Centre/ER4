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
    public class MagLogListController : CSLAController
    {

        public MagLogListController(ILogger<MagLogListController> logger) : base(logger)
        { }

        [HttpGet("[action]")]
        public IActionResult GetMagLogList()
        {
            try
            {
                if (SetCSLAUser() && User.HasClaim(cl => cl.Type == "isSiteAdmin" && cl.Value == "True"))
                {
                    DataPortal<MagLogList> dp = new DataPortal<MagLogList>();
                    var result = dp.Fetch();
                    return Ok(result);
                }
                else return Unauthorized();
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Getting a MagLog List has an error");
                throw;
            }
        }

    }

}

