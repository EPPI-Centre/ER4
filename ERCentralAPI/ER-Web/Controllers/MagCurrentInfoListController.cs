using BusinessLibrary.BusinessClasses;
using Csla;
using EPPIDataServices.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace ERxWebClient2.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class MagCurrentInfoListController : CSLAController
    {

        public MagCurrentInfoListController(ILogger<MagCurrentInfoListController> logger) : base(logger)
        { }


        [EnableRateLimiting("HighCostEndpoints")]
        [HttpGet("[action]")]
        public IActionResult GetMagCurrentInfoList()
        {
            try
            {
                if (SetCSLAUser() && User.HasClaim(cl => cl.Type == "isSiteAdmin" && cl.Value == "True"))
                {
                    DataPortal<MagCurrentInfoList> dp = new DataPortal<MagCurrentInfoList>();
                    var result = dp.Fetch();
                    return Ok(result);
                }
                else return Unauthorized();
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Getting a Mag Current Info List has an error");
                throw;
            }
        }
    }
}

