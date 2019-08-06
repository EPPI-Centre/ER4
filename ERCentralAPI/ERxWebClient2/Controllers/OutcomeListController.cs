using System;
using System.Linq;
using BusinessLibrary.BusinessClasses;
using BusinessLibrary.Security;
using Csla;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
				SingleCriteria<OutcomeItemList, Int64> criteria =
					new SingleCriteria<OutcomeItemList, Int64>(ItemIDCrit.Value);
				OutcomeItemList result = dp.Fetch(criteria);

				return Ok(result);
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Fetching OoutcomeList Errors");
				return StatusCode(500, e.Message);
			}
		}

		// CREATE
		//adds an Outcome to the list and then calls data portal insert
		[HttpPost("[action]")]
		public IActionResult CreateOutcome([FromBody] JObject outcomeData)
		{

			try
			{
				if (SetCSLAUser4Writing())
				{
					ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;

					Outcome outcome = new Outcome();
					outcome = outcomeData.ToObject<Outcome>();
					Outcome result = outcome.Save();

					return Ok(result);
				}
				else
				{
					return Forbid();
				}
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Error when Creating an Outcome : {0}");
				return StatusCode(500, e.Message);
			}
		}

		// UPDATE
		[HttpPost("[action]")]
		public IActionResult UpdateOutcome([FromBody] OutcomeJSON outcomeData)
		{
			try
			{
				if (SetCSLAUser4Writing())
				{
					ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;

					DataPortal<OutcomeItemList> dp = new DataPortal<OutcomeItemList>();
					long itemSetId = outcomeData.ItemSetId;//Convert.ToInt64(outcomeData["itemSetId"].ToString());
					long outcomeId = outcomeData.OutcomeId; //Convert.ToInt64(outcomeData["outcomeId"].ToString());
					SingleCriteria<OutcomeItemList, Int64> criteria = 
						new SingleCriteria<OutcomeItemList, Int64>(itemSetId);
					OutcomeItemList result = dp.Fetch(criteria);

					Outcome editOutcome = result.FirstOrDefault(x => x.OutcomeId == outcomeId);
					//editOutcome = outcomeData.ToObject<Outcome>();
					var test = editOutcome.OutcomeId;
					editOutcome = editOutcome.Save();

					return Ok(editOutcome);
				}
				else
				{
					return Forbid();
				}
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Error when updating an Outcome: {0}");
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
				_logger.LogError(e, "Fetch ReviewSetInterventionList Errors");
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

		//FetchItemArmList
		[HttpPost("[action]")]
		public IActionResult FetchItemArmList([FromBody] SingleIntCriteria itemId)
		{
			try
			{
				SetCSLAUser();
				ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
				DataPortal<ItemArmList> dp = new DataPortal<ItemArmList>();
				SingleCriteria<Item, Int64> criteria =
					new SingleCriteria<Item, Int64>(itemId.Value);
				ItemArmList result = dp.Fetch(criteria);

				return Ok(result);
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Fetch FetchItemArmList Errors");
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
					SingleCriteria<OutcomeItemList, Int64> criteria = 
						new SingleCriteria<OutcomeItemList, Int64>(outcome.itemSetId);

					OutcomeItemList result = dp.Fetch(criteria);
					Outcome currentOutcome = result.FirstOrDefault(x => x.OutcomeId == outcome.outcomeId);
					currentOutcome.Delete();
					currentOutcome = currentOutcome.Save();

					return Ok(result);
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


	// Make all of this captilised and try to update again...
	public class OutcomeJSON
	{
		public int OutcomeId { get; set; }
		public int ItemSetId { get; set; }
		public string OutcomeTypeName { get; set; }
		public int OutcomeTypeId { get; set; }
		public string TimepointDisplayValue { get; set; }
		public int ItemTimepointId  { get; set; }
		public string ItemTimepointMetric  { get; set; }
		public string ItemTimepointValue { get; set; }
		public OutcomeItemAttributesList OutcomeCodes { get; set; }
		public int ItemAttributeIdIntervention { get; set; }
		public int ItemAttributeIdControl { get; set; }
		public int ItemAttributeIdOutcome { get; set; }
		public int ItemArmIdGrp1 { get; set; }
		public int ItemArmIdGrp2 { get; set; }
		public int Grp1ArmName { get; set; }
		public int Grp2ArmName { get; set; }
		public string Title { get; set; }
		public string ShortTitle { get; set; }
		public string OutcomeDescription { get; set; }
		public float Data1	{ get; set; }
		public float Data2	{ get; set; }
		public float Data3	{ get; set; }
		public float Data4	{ get; set; }
		public float Data5	{ get; set; }
		public float Data6	{ get; set; }
		public float Data7	{ get; set; }
		public float Data8	{ get; set; }
		public float Data9	{ get; set; }
		public float Data10	{ get; set; }
		public float Data11	{ get; set; }
		public float Data12	{ get; set; }
		public float Data13	{ get; set; }
		public float Data14	{ get; set; }
		public string InterventionText	{ get; set; }
		public string ControlText	{ get; set; }
		public string OutcomeText	{ get; set; }
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
		public string Data1Desc { get; set; }
		public string Data2Desc { get; set; }
		public string Data3Desc { get; set; }
		public string Data4Desc { get; set; }
		public string Data5Desc { get; set; }
		public string Data6Desc { get; set; }
		public string Data7Desc { get; set; }
		public string Data8Desc { get; set; }
		public string Data9Desc { get; set; }
		public string Data10Desc { get; set; }
		public string Data11Desc { get; set; }
		public string Data12Desc { get; set; }
		public string Data13Desc { get; set; }
		public string Data14Desc { get; set; }
	}


	public class OutcomeItemAttributesList
	{
		public iOutcomeItemAttribute[] outcomeItemAttributesList { get; set; }
	}
	public interface iOutcomeItemAttribute
	{
		int OutcomeItemAttributeId { get; set; }
		int OutcomeId { get; set; }
		int AttributeId { get; set; }
		string AdditionalText { get; set; }
		string AttributeName { get; set; }
	}
}
