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
using Newtonsoft.Json;

namespace ERxWebClient2.Controllers
{
	[Authorize]
	[Route("api/[controller]")]
	public class ComparisonsController : CSLAController
	{

		public ComparisonsController(ILogger<ComparisonsController> logger) : base(logger)
        { }

        [HttpGet("[action]")]
		public IActionResult ComparisonList()//should receive a reviewID!
		{
			try
			{
				if (!SetCSLAUser()) return Unauthorized();
				ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
				DataPortal<ComparisonList> dp = new DataPortal<ComparisonList>();
				ComparisonList result = dp.Fetch();

				return Ok(result);
			}
			catch (Exception e)
			{
				_logger.LogException(e, "Comparison List data portal error");
				return StatusCode(500, e.Message);
			}

		}

		//ComparisonReport
		[HttpPost("[action]")]
		public IActionResult ComparisonReport([FromBody] ComparisonAttributeSelectionJSON comparisonAttributesCriteria)
		{
			//Comparison comparison = new Comparison();

			//comparison = comparisonAttributesCriteria.comparison;

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
				_logger.LogError(e, "Comparison Report data portal error {0}",
					JsonConvert.SerializeObject(comparisonAttributesCriteria));
				return StatusCode(500, e.Message);
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
				_logger.LogError(e, "Comparison delete data portal error {0}", comparisonId);
				return StatusCode(500, e.Message);
			}
		}

		[HttpPost("[action]")]
		public IActionResult ComparisonStats([FromBody] SingleIntCriteria comparisonId)
		{
			try
			{
				if (SetCSLAUser())
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
				_logger.LogError(e, "Comparison Statistics data portal error {0}", comparisonId);
				return StatusCode(500, e.Message);
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
				_logger.LogError(e, "Comparison Complete data portal error {0}", JsonConvert.SerializeObject(comparisonComplete));
				return StatusCode(500, e.Message);
			}
		}

		[HttpPost("[action]")]
		public IActionResult CreateComparison([FromBody] ComparisonMVCJSON comparison)
		{
			try
			{

				if (SetCSLAUser4Writing())
				{

					ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
					Comparison newComp = new Comparison();
					DataPortal<Comparison> dp = new DataPortal<Comparison>();


					newComp.AttributeName = comparison.attributeName;
					newComp.ComparisonDate = (SmartDate) comparison.comparisonDate;
					newComp.ContactId1 = comparison.contactId1;
					newComp.ContactId2 = comparison.contactId2;
					newComp.ContactId3 = comparison.contactId3;
					newComp.ContactName1 = comparison.contactName1;
					newComp.ContactName2 = comparison.contactName2;
					newComp.ContactName3 = comparison.contactName3;
					newComp.InGroupAttributeId = comparison.inGroupAttributeId;
					newComp.ReviewId = ri.ReviewId;
					newComp.SetId = comparison.setId;
					newComp.SetName = comparison.setName;
					
					newComp = dp.Execute(newComp);

					return Ok();
				}
				else return Forbid();

			}
			catch (Exception e)
			{
				_logger.LogError(e, "Comparison create data portal error {0}", 
					JsonConvert.SerializeObject(comparison), Formatting.Indented);
				return StatusCode(500, e.Message);
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
				return StatusCode(500, e.Message);
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
		//public Comparison comparison { get; set; }
	}


	public class ComparisonMVCJSON
	{
		public int comparisonId  {get; set;}
		public bool isScreening  {get; set;}
		public int reviewId  {get; set;}
		public int  inGroupAttributeId {get; set;}
		public int setId  {get; set;}
		public string comparisonDate {get; set;}
		public int contactId1 {get; set;}
		public int contactId2 { get; set;}
		public int contactId3 { get; set;}
		public string contactName1 { get; set;}
		public string contactName2 {get; set;}
		public string contactName3 { get; set;}
		public string attributeName { get; set;}
		public string setName { get; set;}

	}

}
