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

		[HttpPost("[action]")]
		public IActionResult FetchQuestionReport([FromBody] ArgsReportJSON args)
		{
			try
			{
				SetCSLAUser4Writing();
				ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
				DataPortal<ReportList> dp = new DataPortal<ReportList>();
				ReportList result = dp.Fetch();
				return Ok(result);
			}
			catch (Exception e)
			{
				_logger.LogException(e, "Error with FetchQuestionReport");
				return StatusCode(500, e.Message);
			}
		}



		[HttpPost("[action]")]
		public IActionResult FetchAnswerReport([FromBody] ArgsReportJSON args)
		{
			try
			{
				SetCSLAUser4Writing();
				ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
				DataPortal<ReportExecuteCommand> dp = new DataPortal<ReportExecuteCommand>();
				ReportExecuteCommand command = new ReportExecuteCommand(
					args.reportType,
					args.codes,
					args.reportId,
					args.showItemId == true? true : false,
					args.showOldItemId == true ? true : false,
					args.showOutcomes == true ? true : false,
					args.isHorizontal == "true" ? true : false,
					args.orderBy,
					args.title,
					args.attributeId,
					args.setId);
				command = dp.Execute(command);

				return Ok(command);
			}
			catch (Exception e)
			{
				_logger.LogException(e, "Error with FetchAnswerReport");
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


	public class ArgsReportJSON
	{

		public string reportType { get;  set; }
		public string codes { get; set; }
		public int reportId  { get; set; }
		public bool showItemId  { get; set; }
		public bool showOldItemId  { get; set; }
		public bool showOutcomes { get; set; }
		public string isHorizontal { get; set; }
		public string orderBy { get; set; }
		public string title { get; set; }
		public int attributeId  { get; set; }
		public int setId  { get; set; }

	}

}
