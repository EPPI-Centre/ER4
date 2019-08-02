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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static BusinessLibrary.BusinessClasses.ReadOnlyReviewSetControlList;
using static BusinessLibrary.BusinessClasses.ReadOnlyReviewSetInterventionList;
using static BusinessLibrary.BusinessClasses.ReadOnlyReviewSetOutcomeList;

namespace ERxWebClient2.Controllers
{

	[Authorize]
	[Route("api/[controller]")]
	public class OutcomeListController : CSLAController
	{

		private readonly ILogger _logger;

		public OutcomeListController(ILogger<OutcomeListController> logger)
		{
			_logger = logger;
		}

		[HttpPost("[action]")]
		public IActionResult Fetch([FromBody] SingleInt64Criteria ItemIDCrit)
		{
			try
			{
				SetCSLAUser();
				ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
				DataPortal<OutcomeItemList> dp = new DataPortal<OutcomeItemList>();
				SingleCriteria<OutcomeItemList, Int64> criteria = new SingleCriteria<OutcomeItemList, Int64>(ItemIDCrit.Value);
				OutcomeItemList result = dp.Fetch(criteria);

				return Ok(result);
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Fetching OoutcomeList Errors");
				return StatusCode(500, e.Message);
			}
		}

		//FetchReviewSetOutcomeList
		[HttpPost("[action]")]
		public IActionResult FetchReviewSetOutcomeList([FromBody] ReadOnlyReviewSetOutcomeListParams parameters)
		{
			try
			{
				SetCSLAUser();
				ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
				DataPortal<ReadOnlyReviewSetOutcomeList> dp = new DataPortal<ReadOnlyReviewSetOutcomeList>();
				ReadOnlyReviewSetOutcomeListSelectionCriteria criteria =
					new ReadOnlyReviewSetOutcomeListSelectionCriteria(typeof(ReadOnlyReviewSetOutcomeList), parameters.itemSetId,
					parameters.setId);
				ReadOnlyReviewSetOutcomeList result = dp.Fetch(criteria);

				return Ok(result);
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Fetch ReviewSetOutcomeList Errors");
				return StatusCode(500, e.Message);
			}
		}

		//FetchReviewSetInterventionList
		[HttpPost("[action]")]
		public IActionResult FetchReviewSetInterventionList([FromBody] ReadOnlyReviewSetOutcomeListParams parameters)
		{
			try
			{
				SetCSLAUser();
				ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
				DataPortal<ReadOnlyReviewSetInterventionList> dp = new DataPortal<ReadOnlyReviewSetInterventionList>();
				ReadOnlyReviewSetInterventionListSelectionCriteria criteria =
					new ReadOnlyReviewSetInterventionListSelectionCriteria(typeof(ReadOnlyReviewSetInterventionList), parameters.itemSetId,
					parameters.setId);
				ReadOnlyReviewSetInterventionList result = dp.Fetch(criteria);

				return Ok(result);
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Fetch FetchReviewSetInterventionList Errors");
				return StatusCode(500, e.Message);
			}
		}


		//FetchReviewSetControlList
		[HttpPost("[action]")]
		public IActionResult FetchReviewSetControlList([FromBody] ReadOnlyReviewSetOutcomeListParams parameters)
		{
			try
			{
				SetCSLAUser();
				ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
				DataPortal<ReadOnlyReviewSetControlList> dp = new DataPortal<ReadOnlyReviewSetControlList>();
				ReadOnlyReviewSetControlListSelectionCriteria criteria =
					new ReadOnlyReviewSetControlListSelectionCriteria(typeof(ReadOnlyReviewSetControlList), parameters.itemSetId,
					parameters.setId);
				ReadOnlyReviewSetControlList result = dp.Fetch(criteria);

				return Ok(result);
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Fetch ReviewSetControlList Errors");
				return StatusCode(500, e.Message);
			}
		}


		[HttpPost("[action]")]
		public IActionResult UpdateOutcome([FromBody] ItemJSON item)
		{

			try
			{
				if (SetCSLAUser4Writing())
				{
					DataPortal<Outcome> dp = new DataPortal<Outcome>();
					SingleCriteria<Item, Int64> criteria = new SingleCriteria<Item, long>(item.itemId);
					return Ok();
				}
				else
				{
					return Forbid();
				}
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Error when Updating an Outcome : {0}", item);
				return StatusCode(500, e.Message);
			}
		}

		// DELETE
		[HttpPost("[action]")]
		public IActionResult DeleteOutcome([FromBody] OutcomeIds outcome)
		{

			try
			{
				if (SetCSLAUser4Writing())
				{
					ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;

					DataPortal<OutcomeItemList> dp = new DataPortal<OutcomeItemList>();
					SingleCriteria<OutcomeItemList, Int64> criteria = new SingleCriteria<OutcomeItemList, Int64>(outcome.itemSetId);

					OutcomeItemList result = dp.Fetch(criteria);

					Outcome currentOutcome = result.FirstOrDefault(x => x.OutcomeId == outcome.outcomeId);

					currentOutcome.Delete();
					currentOutcome = currentOutcome.Save();

					return Ok();
				}
				else
				{
					return Forbid();
				}
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Error when Deleting an outcome: {0}", outcome);
				return StatusCode(500, e.Message);
			}
		}


	}

	public class ReadOnlyReviewSetOutcomeListParams
	{
		public long itemSetId { get; set; }
		public long setId { get; set; }

	}

	public class OutcomeIds
	{
		public int outcomeId { get; set; }
		public int itemSetId { get; set; }
	}

	public class OutcomeJSON
	{
		public int outcomeId { get; set; }
		public int itemSetId { get; set; }
		public string outcomeTypeName { get; set; }
		public int outcomeTypeId { get; set; }
		public string timepointDisplayValue { get; set; }
		public int itemTimepointId  { get; set; }
		public string itemTimepointMetric  { get; set; }
		public string itemTimepointValue { get; set; }
		public OutcomeItemAttributesList outcomeCodes { get; set; }
		public int itemAttributeIdIntervention { get; set; }
		public int itemAttributeIdControl { get; set; }
		public int itemAttributeIdOutcome { get; set; }
		public int itemArmIdGrp1 { get; set; }
		public int itemArmIdGrp2 { get; set; }
		public int grp1ArmName { get; set; }
		public int grp2ArmName { get; set; }
		public string title { get; set; }
		public string shortTitle { get; set; }
		public string outcomeDescription { get; set; }
		public float data1	{ get; set; }
		public float data2	{ get; set; }
		public float data3	{ get; set; }
		public float data4	{ get; set; }
		public float data5	{ get; set; }
		public float data6	{ get; set; }
		public float data7	{ get; set; }
		public float data8	{ get; set; }
		public float data9	{ get; set; }
		public float data10	{ get; set; }
		public float data11	{ get; set; }
		public float data12	{ get; set; }
		public float data13	{ get; set; }
		public float data14	{ get; set; }
		public string interventionText	{ get; set; }
		public string controlText	{ get; set; }
		public string outcomeText	{ get; set; }
		public float feWeight	{ get; set; }
		public float reWeight	{ get; set; }
		public float smd	{ get; set; }
		public string sesmd	{ get; set; }
		public float r	{ get; set; }
		public float ser	{ get; set; }
		public float oddsRatio	{ get; set; }
		public float seOddsRatio	{ get; set; }
		public float riskRatio	{ get; set; }
		public float seRiskRatio	{ get; set; }
		public string ciUpperSMD	{ get; set; }
		public string ciLowerSMD	{ get; set; }
		public float ciUpperR	{ get; set; }
		public float ciLowerR	{ get; set; }
		public float ciUpperOddsRatio	{ get; set; }
		public float ciLowerOddsRatio	{ get; set; }
		public float ciUpperRiskRatio	{ get; set; }
		public float ciLowerRiskRatio	{ get; set; }
		public float ciUpperRiskDifference	{ get; set; }
		public float ciLowerRiskDifference	{ get; set; }
		public float ciUpperPetoOddsRatio	{ get; set; }
		public float ciLowerPetoOddsRatio	{ get; set; }
		public string ciUpperMeanDifference	{ get; set; }
		public string ciLowerMeanDifference	{ get; set; }
		public float riskDifference	{ get; set; }
		public float seRiskDifference	{ get; set; }
		public float meanDifference	{ get; set; }
		public string seMeanDifference	{ get; set; }
		public float petoOR	{ get; set; }
		public float sePetoOR	{ get; set; }
		public float es	{ get; set; }
		public string sees	{ get; set; }
		public float nRows	{ get; set; }
		public string ciLower	{ get; set; }
		public string ciUpper	{ get; set; }
		public string esDesc { get; set; }
		public string seDesc { get; set; }
		public string data1Desc { get; set; }
		public string data2Desc { get; set; }
		public string data3Desc { get; set; }
		public string data4Desc { get; set; }
		public string data5Desc { get; set; }
		public string data6Desc { get; set; }
		public string data7Desc { get; set; }
		public string data8Desc { get; set; }
		public string data9Desc { get; set; }
		public string data10Desc { get; set; }
		public string data11Desc { get; set; }
		public string data12Desc { get; set; }
		public string data13Desc { get; set; }
		public string data14Desc { get; set; }
	}


	public class OutcomeItemAttributesList
	{
		public iOutcomeItemAttribute[] outcomeItemAttributesList { get; set; }
	}
	public interface iOutcomeItemAttribute
	{
		int outcomeItemAttributeId { get; set; }
		int outcomeId { get; set; }
		int attributeId { get; set; }
		string additionalText { get; set; }
		string attributeName { get; set; }
	}
}
