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
using Newtonsoft.Json.Linq;
using EPPIDataServices.Helpers;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ERxWebClient2.Controllers
{
	[Authorize]
	[Route("api/[controller]")]
	public class ComparisonsController : CSLAController
	{
		private readonly ILogger _logger;

		public ComparisonsController(ILogger<ComparisonsController> logger)
		{
			_logger = logger;
		}

		[HttpGet("[action]")]
		public IActionResult ComparisonList()//should receive a reviewID!
		{
			try
			{
				SetCSLAUser();
				ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
				DataPortal<ComparisonList> dp = new DataPortal<ComparisonList>();
				ComparisonList result = dp.Fetch();

				return Ok(result);
			}
			catch (Exception e)
			{
				_logger.LogException(e, "Comparison List data portal error");
				throw;
			}

		}

		//ComparisonReport
		[HttpPost("[action]")]
		public IActionResult ComparisonReport([FromBody] ComparisonAttributeSelectionJSON comparisonAttributesCriteria)
		{
			Comparison comparison = new Comparison();

			comparison = comparisonAttributesCriteria.comparison;

			try
			{
				if (SetCSLAUser4Writing())
				{
					ComparisonAttributeSelectionCriteria crit = new ComparisonAttributeSelectionCriteria(
						typeof(ComparisonAttributeList),
						comparisonAttributesCriteria.comparisonid,
						comparisonAttributesCriteria.parentAttributeId,
						comparisonAttributesCriteria.setId
						);

					DataPortal<ComparisonAttributeList> dp = new DataPortal<ComparisonAttributeList>();
					ComparisonAttributeList reportList = dp.Fetch(crit);


					return Ok(reportList);

				}
				else return Forbid();
			}
			catch (Exception e)
			{
				_logger.LogException(e, "Comparison Report data portal error");
				throw;
			}
		}

		[HttpPost("[action]")]
		public IActionResult DeleteComparison([FromBody] SingleIntCriteria comparisonId)
		{
			try
			{
				if (SetCSLAUser4Writing())
				{
					ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
					DataPortal<ComparisonList> dp = new DataPortal<ComparisonList>();
					ComparisonList result = dp.Fetch();

					Comparison currentComparison = result.FirstOrDefault(x => x.ComparisonId == comparisonId.Value);

					currentComparison.Delete();
					currentComparison = currentComparison.Save();

					return Ok();
				}
				else return Forbid();
			}
			catch (Exception e)
			{
				_logger.LogException(e, "Comparison delete data portal error");
				throw;
			}
		}

		[HttpPost("[action]")]
		public IActionResult ComparisonStats([FromBody] SingleIntCriteria comparisonId)
		{
			try
			{
				if (SetCSLAUser4Writing())
				{

					ComparisonStatsCommand cmd = new ComparisonStatsCommand(comparisonId.Value);
					DataPortal<ComparisonStatsCommand> dp = new DataPortal<ComparisonStatsCommand>();
					cmd = dp.Execute(cmd);

					return Ok(cmd);
				}
				else return Forbid();
			}
			catch (Exception e)
			{
				_logger.LogException(e, "Comparison Statistics data portal error");
				throw;
			}
		}

		[HttpPost("[action]")]
		public IActionResult CompleteComparison([FromBody] ComparisonCompleteJSON comparisonComplete)
		{
			try
			{
				if (SetCSLAUser4Writing())
				{

					if (comparisonComplete.whichReviewers.Contains("Sc"))
					{
                        ComparisonScreeningCompleteCommand cmd = new ComparisonScreeningCompleteCommand(
                            comparisonComplete.comparisonid,
                            comparisonComplete.whichReviewers,
                            comparisonComplete.contactId,
                            comparisonComplete.lockCoding
                        );
						DataPortal<ComparisonScreeningCompleteCommand> dp = new DataPortal<ComparisonScreeningCompleteCommand>();
						cmd = dp.Execute(cmd);

						return Ok(cmd);
					}
					else
					{
						ComparisonCompleteCommand cmd = new ComparisonCompleteCommand(
							comparisonComplete.comparisonid,
							comparisonComplete.whichReviewers,
							comparisonComplete.contactId,
                            comparisonComplete.lockCoding
                        );
						DataPortal<ComparisonCompleteCommand> dp = new DataPortal<ComparisonCompleteCommand>();
						cmd = dp.Execute(cmd);

						return Ok(cmd);
					}

				}
				else return Forbid();
			}
			catch (Exception e)
			{
				_logger.LogException(e, "Comparison Complete data portal error");
				throw;
			}
		}

		[HttpPost("[action]")]
		public IActionResult CreateComparison([FromBody] JObject comparison)
		{
			try
			{

				if (SetCSLAUser4Writing())
				{

					ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
					Comparison newComp = new Comparison();
					DataPortal<Comparison> dp = new DataPortal<Comparison>();

					newComp = comparison.ToObject<Comparison>();

					newComp = dp.Execute(newComp);

					return Ok();
				}
				else return Forbid();

			}
			catch (Exception e)
			{
				_logger.LogException(e, "Comparison create data portal error");
				throw;
			}
		}

		[HttpPost("[action]")]
		public IActionResult ItemAttributesFullTextData([FromBody] long itemid)
		{
			try
			{

				if (SetCSLAUser4Writing())
				{

					ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
					DataPortal<ItemAttributesAllFullTextDetailsList> dp = 
						new DataPortal<ItemAttributesAllFullTextDetailsList>();
					SingleCriteria<ItemAttributesAllFullTextDetailsList, Int64> criteria =
						new SingleCriteria<ItemAttributesAllFullTextDetailsList, Int64>(itemid);

					//new SingleCriteria<ItemAttributesAllFullTextDetailsList, Int64>(itemid)
					var result = dp.Fetch(criteria);

					return Ok(result);
				}
				else return Forbid();

			}
			catch (Exception e)
			{
				_logger.LogException(e, "Comparison create data portal error");
				throw;
			}
		}

	}

	public class ComparisonCompleteJSON
	{
		public int comparisonid {get; set;}
		public string whichReviewers { get; set; }
		public int contactId { get; set; }
        public string lockCoding { get; set; }
    }

	public class ComparisonAttributeSelectionJSON
	{
		public int comparisonid { get; set; }
		public Int64 parentAttributeId { get; set; }
		public int setId { get; set; }
		public Comparison comparison { get; set; }
	}


}
