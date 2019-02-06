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
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using EPPIDataServices.Helpers;
using Newtonsoft.Json;

namespace ERxWebClient2.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class HelpController : CSLAController
    {
        //OnlineHelpCriteria
        private readonly ILogger _logger;

        public HelpController(ILogger<ReviewController> logger)
        {
            _logger = logger;
        }

		[HttpPost("[action]")]
		public IActionResult FetchHelpContent([FromBody] SingleStringCriteria crit)
		{
            SetCSLAUser();
			try
			{
                OnlineHelpContent res  = new OnlineHelpContent();
                DataPortal<OnlineHelpContent> dp = new DataPortal<OnlineHelpContent>();
                res = dp.Fetch(new OnlineHelpCriteria(crit.Value));
				return Ok(res);
			}
			catch (Exception e)
			{
				_logger.LogException(e, "Reviews data portal error");
				throw;
			}
			
		}
	}
    
}
