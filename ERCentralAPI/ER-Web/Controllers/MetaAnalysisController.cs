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
                _logger.LogException(e, "GetMAList data portal error");
                return StatusCode(500, e.Message);
            }

		}
        [HttpPost("[action]")]
        public IActionResult FetchMetaAnalysis([FromBody] MetaAnalysisSelectionCritJSON crit)
        {
            try
            {
                if (!SetCSLAUser()) return Unauthorized();
                MetaAnalysis result = DataPortal.Fetch<MetaAnalysis>(crit.ToCSLACirteria());
                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogException(e, "GetMetaAnalysis data portal error");
                return StatusCode(500, e.Message);
            }

        }
        public class MetaAnalysisSelectionCritJSON
        {
            public bool GetAllDetails { get; set;}

            public int MetaAnalysisId { get; set; }
            public MetaAnalysisSelectionCrit ToCSLACirteria()
            {
                MetaAnalysisSelectionCrit res = new MetaAnalysisSelectionCrit();
                res.GetAllDetails = GetAllDetails;
                res.MetaAnalysisId= MetaAnalysisId;
                return res;
            }
        }
    }
}



