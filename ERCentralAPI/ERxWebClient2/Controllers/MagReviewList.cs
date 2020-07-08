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
    public class MagReviewListController : CSLAController
    {

        private readonly ILogger _logger;

		public MagReviewListController(ILogger<MagReviewListController> logger)
        {

            _logger = logger;
        }


        [HttpGet("[action]")]
        public IActionResult GetMagReviewList()
        {
            try
            {
                SetCSLAUser();

                DataPortal<MagReviewList> dp = new DataPortal<MagReviewList>();

                var result = dp.Fetch();

                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Getting a MagReviewList has an error");
                throw;
            }
        }

    }

}

