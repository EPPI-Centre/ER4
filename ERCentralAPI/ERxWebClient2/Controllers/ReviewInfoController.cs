using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using BusinessLibrary.BusinessClasses;
using BusinessLibrary.Security;
using Csla;
using Csla.Data;
using ERxWebClient2.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using EPPIDataServices.Helpers;
using Microsoft.Extensions.Logging;

namespace ERxWebClient2.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class ReviewInfoController : CSLAController
    {

        private readonly ILogger _logger;

        public ReviewInfoController(ILogger<ReviewInfoController> logger)
        {
            _logger = logger;
        }

        [HttpGet("[action]")]
        public IActionResult ReviewInfo()
        {

            try
            {

                SetCSLAUser();
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;

                DataPortal<ReviewInfo> dp = new DataPortal<ReviewInfo>();

                ReviewInfo result = dp.Fetch();

                return Ok(result);

            }
            catch (Exception e)
            {
                _logger.LogException(e, "A user idenity issue");
                throw;
            }
        }

    }
}