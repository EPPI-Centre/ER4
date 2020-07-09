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

        private readonly ILogger _logger;

		public MagLogListController(ILogger<MagLogListController> logger)
        {

            _logger = logger;
        }


        [HttpGet("[action]")]
        public IActionResult GetMagLogList()
        {
            try
            {
                SetCSLAUser();

                DataPortal<MagLogList> dp = new DataPortal<MagLogList>();

                var result = dp.Fetch();

                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Getting a MagLogList has an error");
                throw;
            }
        }

    }

}

