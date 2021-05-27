using System;
using BusinessLibrary.BusinessClasses;
using BusinessLibrary.Security;
using Csla;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using EPPIDataServices.Helpers;
using System.Collections.Generic;

namespace ERxWebClient2.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class ReportListController : CSLAController
    {
        
        public ReportListController(ILogger<ReportListController> logger) : base(logger)
        { }

        [HttpGet("[action]")]
        public IActionResult FetchReports()
        {
            try
            {
                if (!SetCSLAUser()) return Unauthorized();
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
		public IActionResult CreateReport([FromBody] ReportJSON rep)
		{
			try
			{
				if (SetCSLAUser4Writing())
				{
					Report newR = rep.ToCSLAReport();
					newR = newR.Save();
					newR = DataPortal.Fetch<Report>(new SingleCriteria<int>(newR.ReportId));//re-fetching to ensure we return the truth...
					return Ok(newR);//if no error, all should be OK.
				}
				else return Forbid();
			}
			catch (Exception e)
			{
				_logger.LogException(e, "Error running UpdateReport, RepID:" + rep.reportId);
				return StatusCode(500, e.Message);
			}
		}
		[HttpPost("[action]")]
		public IActionResult UpdateReport([FromBody] ReportJSON rep)
		{
			try
			{
				if (SetCSLAUser4Writing())
				{
					Report actual = DataPortal.Fetch<Report>(new SingleCriteria<int>(rep.reportId));
					if (actual.Name != rep.name) actual.Name = rep.name;
					//bit of work to do here!
					//we need to figure what cols have been created or deleted,
					//we also need to apply any change done to columns that (might) have been edited.
					List<ReportColumn> DeletedColumns = new List<ReportColumn>();
					List<ReportColumnJSON> AddedColumns = rep.columns.ToList();
					Dictionary<ReportColumn, ReportColumnJSON> ExistingColumns = new Dictionary<ReportColumn, ReportColumnJSON>();
					foreach (ReportColumn col in actual.Columns)
					{
						ReportColumnJSON cJ = rep.columns.FirstOrDefault((f) => f.reportColumnId == col.ReportColumnId);
						if (cJ != null)
						{//we have this column in both lists... We list it as existent and remove from the list "potentially new" columns
							ExistingColumns.Add(col, cJ);
							AddedColumns.Remove(cJ);
						}
						else
						{
							col.Delete();//this col is not in the object received from the client, so it has been deleted from there...
							DeletedColumns.Add(col);
							actual.Detail = "edited!!";
						}
					}
					//OK, let's add the new columns...
					foreach (ReportColumnJSON newCol in AddedColumns)
					{
						ReportColumn adding = newCol.ToCSLAReportColumn();
						actual.Columns.Add(adding);
						actual.Detail = "edited!!";
					}
					foreach (KeyValuePair<ReportColumn, ReportColumnJSON> pair in ExistingColumns)
					{
						if (!pair.Value.IsIdenticalTo(pair.Key))
						{
							pair.Key.Delete();
							actual.Columns.Add(pair.Value.ToCSLAReportColumn());
							actual.Detail = "edited!!";
						}
					}
					actual = actual.Save(true);
					actual = DataPortal.Fetch<Report>(new SingleCriteria<int>(rep.reportId));//re-fetching to ensure we return the truth...
					return Ok(actual);//if no error, all should be OK.
				}
				else return Forbid();
			}
			catch (Exception e)
			{
				_logger.LogException(e, "Error running UpdateReport, RepID:" + rep.reportId);
				return StatusCode(500, e.Message);
			}
		}

		[HttpPost("[action]")]
		public IActionResult FetchStandardReport([FromBody] ReportStandardJSON args)
		{
			try
			{
				if (!SetCSLAUser()) return Unauthorized();
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

				return Json(htmlString);
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Error with FetchStandardReport", e.InnerException);
                return StatusCode(500, e.Message);
       
			}
		}
		
		[HttpPost("[action]")]
		public IActionResult FetchROBReport([FromBody] ReportRiskOfBiasJSON args)
		{
			try
			{
				if (!SetCSLAUser()) return Unauthorized();
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
					,! args.showUncodedItems,
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
				return StatusCode(500, e.Message);
			}
		}
		
		[HttpPost("[action]")]
		public IActionResult FetchOutcomesReport([FromBody] ReportOutcomesJSON args)
		{
			try
			{
				if (!SetCSLAUser()) return Unauthorized();
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
		public string contactName { get; set; }
		public string reportType { get; set; }
		public bool isAnswer { get; set; }
		public ReportColumnJSON[] columns { get; set; }
		public bool IsIdenticalTo(Report rep)
        {
			if (this.name != rep.Name) return false;
			else if (this.reportType != rep.ReportType) return false;
			if (this.columns.Length != rep.Columns.Count) return false;
			else
            {
				int matchTarget = this.columns.Length;
				int matched = 0;
				foreach (ReportColumn col in rep.Columns)
                {
					ReportColumnJSON tmp = this.columns.FirstOrDefault((f) => f.IsIdenticalTo(col));
					if (tmp != null)
					{
						matched++;
					}
				}
				if (matched == matchTarget) return true;
			}
			return false;
        }
		public Report ToCSLAReport()
        {
			Report res = new Report();
			res.ContactName = this.contactName;
			res.Name = this.name;
			res.ReportType = this.reportType == "Answer" ? "Answer" : "Question";
			res.Columns = new ReportColumnList();
			foreach (ReportColumnJSON rc in this.columns)
            {
				res.Columns.Add(rc.ToCSLAReportColumn());
            }
			return res;
		}
	}
	public class ReportColumnJSON
	{
		public int reportColumnId { get; set; }
		public int columnOrder { get; set; }
		public string name { get; set; }
		public ReportColumnCodeJSON[] codes { get; set; }
		public bool IsIdenticalTo(ReportColumn col)
		{
			if (col.ColumnOrder != this.columnOrder || col.Name != this.name) return false;
			else if (this.codes.Length != col.Codes.Count) return false;
			else 
			{
				int matchTarget = this.codes.Length;
				int matched = 0;
				foreach (ReportColumnCode rcc in col.Codes)
                {
					ReportColumnCodeJSON tmp = this.codes.FirstOrDefault((f) => f.IsIdenticalTo(rcc));
					if (tmp != null)
                    {
						matched++;
                    }
                }
				if (matched == matchTarget) return true;
			}
			return false;
		}
		public ReportColumn ToCSLAReportColumn()
        {
			ReportColumn res = new ReportColumn();
			res.ColumnOrder = columnOrder;
			res.Name = name;
			res.Codes = new ReportColumnCodeList();
			foreach (ReportColumnCodeJSON newCode in codes)
			{
				res.Codes.Add(newCode.ToCSLAReportColumnCode());
			}
			return res;
		}
	}
	public class ReportColumnCodeJSON
	{

		public Int64 attributeId { get; set; }
		public int codeOrder { get; set; }
		public bool displayAdditionalText { get; set; }
		public bool displayCode { get; set; }
		public bool displayCodedText { get; set; }
		public Int64 parentAttributeId { get; set; }
		public string parentAttributeText { get; set; }
		public int reportColumnCodeId { get; set; }
		public int reportColumnId { get; set; }
		public int setId { get; set; }
		public string userDefText { get; set; }
		public bool IsIdenticalTo(ReportColumnCode col)
		{
			if (this.reportColumnCodeId != col.ReportColumnCodeId
				|| this.attributeId != col.AttributeId
				|| this.codeOrder != col.CodeOrder
				|| this.displayAdditionalText != col.DisplayAdditionalText
				|| this.displayCode != col.DisplayCode
				|| this.displayCodedText != col.DisplayCodedText
				|| this.reportColumnId != col.ReportColumnId
				|| this.setId != col.SetId
				|| this.userDefText != col.UserDefText
				) return false;
			else return true;
		}
		public ReportColumnCode ToCSLAReportColumnCode()
        {
			ReportColumnCode res = new ReportColumnCode();
			res.AttributeId = this.attributeId;
			res.CodeOrder = this.codeOrder;
			res.DisplayAdditionalText = this.displayAdditionalText;
			res.DisplayCode = this.displayCode;
			res.DisplayCodedText = this.displayCodedText;
			res.ReportColumnId = this.reportColumnId;
			res.SetId = this.setId;
			res.UserDefText = this.userDefText;
			return res;
		}
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
