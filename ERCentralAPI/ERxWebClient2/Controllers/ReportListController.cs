using System;
using BusinessLibrary.BusinessClasses;
using BusinessLibrary.Security;
using Csla;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
                ReviewerIdentity ri = ApplicationContext.User.Identity as ReviewerIdentity;
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
		public IActionResult FetchStandardReport([FromBody] ReportStandardJSON args)
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
					, args.isHorizontal
					, args.showItemId
					, args.showOldItemId
					, args.showOutcomes
					, args.showFullTitle
					, args.showAbstract
					, args.showYear
					, args.showShortTitle
					));


				string htmlString = report.ReportContent(
					args.isHorizontal,
					args.showItemId
					, args.showOldItemId
					, !args.showUncodedItems,
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
                //Random random = new Random();
                //if (random.Next() != 34543) throw new Exception("I'm an error!!!!");
                return Json(htmlString);
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Error with FetchStandardReport", e.InnerException);
                return StatusCode(500, e.Message);
                //return Json(e.Message);
			}
		}
		
		[HttpPost("[action]")]
		public IActionResult FetchROBReport([FromBody] ReportRiskOfBiasJSON args)
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
					, args.isHorizontal
					, args.showItemId
					, args.showOldID
					, args.showOutcomes
					, args.showFullTitle
					, args.showAbstract
					, args.showYear
					, args.showShortTitle
					));


				string htmlString = report.ReportContent(
					args.isHorizontal,
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
				_logger.LogError(e, "Error with FetchROBReport", e.InnerException);
				return Json(e.Message);
			}
		}
		
		[HttpPost("[action]")]
		public IActionResult FetchOutcomesReport([FromBody] ReportOutcomesJSON args)
		{
			try
			{
				SetCSLAUser4Writing();
				ReviewerIdentity ri = ApplicationContext.User.Identity as ReviewerIdentity;
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
				_logger.LogException(e, "Error with FetchOutcomesReport");
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
	   
	public class ReportStandardJSON
	{

			public string items { get; set; }
			public bool showFullTitle { get; set; }
			public bool showAbstract { get; set; }
			public bool showYear { get; set; }
			public bool showShortTitle { get; set; }
			public int reportId { get; set; }
			public bool showItemId { get; set; }
			public bool showOldItemId { get; set; }
			public bool showOutcomes { get; set; }
			public bool isHorizontal { get; set; }
			public string orderBy { get; set; }
			public bool isQuestion { get; set; }
			public int attributeId { get; set; }
			public int setId { get; set; }
			public bool showRiskOfBias { get; set; } 
			public bool showUncodedItems { get; set; }
			public bool showBullets { get; set; }
			public string txtInfoTag { get; set; }

	}

	public class ReportRiskOfBiasJSON
	{
		public string items { get; set; }
		public bool showFullTitle { get; set; }
		public bool showAbstract { get; set; }
		public bool showYear { get; set; }
		public bool showShortTitle { get; set; }
		public int reportId { get; set; }
		public bool showItemId { get; set; }
		public bool showOldID { get; set; }
		public bool showOutcomes { get; set; }
		public bool isHorizontal { get; set; }
		public string orderBy { get; set; }
		public bool isQuestion { get; set; }
		public int attributeId { get; set; }
		public int setId { get; set; }
		public bool showRiskOfBias { get; set; }
		public bool showUncodedItems { get; set; }
		public bool showBullets { get; set; }
		public string txtInfoTag { get; set; }

	}

	public class ReportOutcomesJSON
	{
		    public string reportType { get; set; }
		    public string codes  { get; set; }
		    public int reportId { get; set; }
		    public bool showItemId  { get; set; }
			public bool showOldItemId { get; set; }
			public bool showOutcomes { get; set; }
			public string isHorizontal { get; set; }
			public string orderBy { get; set; }
			public string title { get; set; }
			public int attributeId { get; set; }
			public int setId { get; set; }

	}

}
