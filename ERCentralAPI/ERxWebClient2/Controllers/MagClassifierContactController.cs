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
    public class MagClassifierContactController : CSLAController
    {

        private readonly ILogger _logger;

        public MagClassifierContactController(ILogger<MagClassifierContactController> logger)
        {

            _logger = logger;
        }

        [HttpGet("[action]")]
        public IActionResult FetchClassifierContactList()
        {
            try
            {
                SetCSLAUser();

                DataPortal<ClassifierContactModelList> dp = new DataPortal<ClassifierContactModelList>();
                ClassifierContactModelList result = dp.Fetch();

                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Fetching a ClassifierContactModel list has an error");
                throw;
            }
        }

    }
}

