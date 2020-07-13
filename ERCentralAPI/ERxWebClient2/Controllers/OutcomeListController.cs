using System;
using System.Collections.Generic;
using System.Linq;
using BusinessLibrary.BusinessClasses;
using BusinessLibrary.Security;
using Csla;
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
				SingleCriteria<OutcomeItemList, Int64> criteria =
					new SingleCriteria<OutcomeItemList, Int64>(ItemIDCrit.Value);
				OutcomeItemList result = dp.Fetch(criteria);

				return Ok(result);
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Fetching OutcomeList Errors");
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

					if (result.OutcomeId > 0)
					{
						int itemSetId = Convert.ToInt32(outcomeData["itemSetId"].ToString());
						int outcomeId = result.OutcomeId;

						var jtokenArrOutcomeCodes =
							outcomeData["outcomeCodes"]["outcomeItemAttributesList"].Select(x => x).ToArray();

						string attributes = "";
						foreach (var item in jtokenArrOutcomeCodes)
						{
							var obj = item.ToObject<OutcomeItemAttribute>();

							if (attributes == "")
							{
								attributes = obj.AttributeId.ToString();
							}
							else
							{
								attributes += "," + obj.AttributeId.ToString();
							}
						}
                        if (attributes != "")
                        {
                            DataPortal<OutcomeItemAttributesCommand> dpCmd = new DataPortal<OutcomeItemAttributesCommand>();
                            OutcomeItemAttributesCommand command = new OutcomeItemAttributesCommand(
                                outcomeId, attributes);

                            command = dpCmd.Execute(command);
                        }
					}

					return Ok(result);
				}
				else
				{
					return Forbid();
				}
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Error when Creating an Outcome : " + JsonConvert.SerializeObject(outcomeData));
				return StatusCode(500, e.Message);
			}
		}

		// UPDATE
		[HttpPost("[action]")]
		public IActionResult UpdateOutcome([FromBody] JObject outcomeData)
		{
			try
			{
				if (SetCSLAUser4Writing())
				{
					ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;

					int itemSetId = Convert.ToInt32(outcomeData["itemSetId"].ToString());
					int outcomeId = Convert.ToInt32(outcomeData["outcomeId"].ToString());

					var jtokenArrOutcomeCodes =
						outcomeData["outcomeCodes"]["outcomeItemAttributesList"].Select(x => x).ToArray();

					string attributes = "";
					foreach (var item in jtokenArrOutcomeCodes)
					{
						var obj = item.ToObject<OutcomeItemAttribute>();

						if (attributes == "")
						{
							attributes = obj.AttributeId.ToString();
						}
						else
						{
							attributes += "," + obj.AttributeId.ToString();
						}
					}

					DataPortal<OutcomeItemAttributesCommand> dpCmd = new DataPortal<OutcomeItemAttributesCommand>();
					OutcomeItemAttributesCommand command = new OutcomeItemAttributesCommand(outcomeId, attributes);

					command = dpCmd.Execute(command);

					
					SingleCriteria<OutcomeItemList, Int64> criteria =
						new SingleCriteria<OutcomeItemList, Int64>(itemSetId);
					DataPortal<OutcomeItemList> dp = new DataPortal<OutcomeItemList>();
					OutcomeItemList result = dp.Fetch(criteria);
					Outcome editOutcome = result.FirstOrDefault(x => x.OutcomeId == outcomeId);
					
					outcomeData.Populate(editOutcome);
					
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
				_logger.LogError(e, "Error when updating an Outcome: " + JsonConvert.SerializeObject(outcomeData));
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
				_logger.LogError(e, "Error when Deleting an outcome: {0}" + JsonConvert.SerializeObject(outcome));
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


	public static class JsonExtensions
	{
		public static void Populate<T>(this JToken value, T target) where T : class
		{

			using (var sr = value.CreateReader())
			{
				JsonSerializer.CreateDefault().Populate(sr, target); // Uses the system default JsonSerializerSettings
			}
		}
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
