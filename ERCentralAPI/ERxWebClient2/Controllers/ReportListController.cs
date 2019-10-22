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

namespace ERxWebClient2.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class ReportListController : CSLAController
    {

        private readonly ILogger _logger;

        public ReportListController(ILogger<ReportListController> logger)
        {
            _logger = logger;
        }

        [HttpGet("[action]")]
        public IActionResult FetchReports()
        {
            try
            {
                SetCSLAUser();
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                DataPortal<ReportList> dp = new DataPortal<ReportList>();
                ReportList result = dp.Fetch();
                return Ok(result);
            }
            catch(Exception e)
            {
                _logger.LogException(e, "Error with ReportList");
                return StatusCode(500, e.Message);
            }
        }
	}

	public class ReportJSON
	{
		public string name { get; set; }
		public int reportId { get; set; }
		public int contactId  { get; set; }
		public string reportType  { get; set; }
	}

}
