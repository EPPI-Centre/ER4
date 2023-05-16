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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EPPIDataServices.Helpers;

namespace ERxWebClient2.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class MetaAnalysisController : CSLAController
    {
        
        public MetaAnalysisController(ILogger<FrequenciesController> logger) : base(logger)
        { }

        [HttpGet("[action]")]
        public IActionResult GetMAList()
        {
			try
            {
                if (!SetCSLAUser()) return Unauthorized();
                MetaAnalysisList result = DataPortal.Fetch<MetaAnalysisList>();
                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogException(e, "GetFrequencies data portal error");
                return StatusCode(500, e.Message);
            }

		}
               
    }
}



