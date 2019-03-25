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
		//AssignWorkAllocation
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
	}
}
