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
		public JsonResult FetchQuestionReport([FromBody] ArgsQuestionReportJSON args)
		{
			try
			{
				SetCSLAUser4Writing();
				ReviewerIdentity ri = ApplicationContext.User.Identity as ReviewerIdentity;

				DataPortal<ReviewSetsList> dpFirst = new DataPortal<ReviewSetsList>();
				ReviewSetsList reviewSets = dpFirst.Fetch();


				DataPortal<ReportData> dp = new DataPortal<ReportData>();
				ReportData report = dp.Fetch(new ReportDataSelectionCriteria(
					  args.isQuestion
					, args.items
					, args.reportId
					, args.orderBy
					, args.attributeId
					, args.setId
					, args.isHorizantal
					, args.showItemId
					, args.showOldID
					, args.showOutcomes
					, args.showFullTitle
					, args.showAbstract
					, args.showYear
					, args.showShortTitle
					));


				string htmlString = report.ReportContent(
					args.isHorizantal,
					args.showItemId
					, args.showOldID
					, args.showUncodedItems,
					args.showBullets,
					args.txtInfoTag
					, args.orderBy
					, args.showRiskOfBias
					, reviewSets
					, args.showFullTitle
					, args.showAbstract
					, args.showYear
					, args.showShortTitle
					);

				return Json(htmlString);
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Error with FetchQuestionReport", e.InnerException);
				//return StatusCode(500, e.Message);
				return Json(e.Message);
			}
		}


		[HttpPost("[action]")]
		public IActionResult FetchAnswerReport([FromBody] ArgsAnswerReportJSON args)
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
					args.showItemId == true ? true : false,
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


	public class ArgsAnswerReportJSON
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
		public bool showRiskOfBias { get; set; }
		public bool showUncodedItems { get; set; }
		public bool showBullets { get; set; }
		public string txtInfoTag { get; set; }

	}

	public class ArgsQuestionReportJSON
	{
		public bool isQuestion { get; set; }
		public string items { get; set; }
		public int reportId { get; set; }
		public string orderBy { get; set; }
		public int attributeId { get; set; }
		public int setId { get; set; }
		public bool isHorizantal { get; set; }
		public bool showItemId { get; set; }
		public bool showOldID { get; set; }
		public bool showOutcomes { get; set; }
		public bool showFullTitle { get; set; }
		public bool showAbstract { get; set; }
		public bool showYear { get; set; }
		public bool showShortTitle { get; set; }
		public bool showRiskOfBias { get; set; }
		public bool showUncodedItems { get; set; }
		public bool showBullets { get; set; }
		public string txtInfoTag { get; set; }

	}

}
