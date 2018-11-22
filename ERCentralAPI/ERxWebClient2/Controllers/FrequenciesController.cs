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
using Newtonsoft.Json;

namespace ERxWebClient2.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class FrequenciesController : CSLAController
    {

        private readonly ILogger _logger;

        public FrequenciesController(ILogger<ReviewController> logger)
        {

            _logger = logger;
        }

        [HttpPost("[action]")]
        public IActionResult GetFrequencies([FromBody] Criteria data)
        {

			Type type = null;

			try
            {
                SetCSLAUser();
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;

                DataPortal<ReadOnlyItemAttributeChildFrequencyList> dp = new DataPortal<ReadOnlyItemAttributeChildFrequencyList>();

				ItemAttributeChildFrequencySelectionCriteria criteria = new ItemAttributeChildFrequencySelectionCriteria(type, Convert.ToInt64(data.AttributeId), Convert.ToInt32( data.SetId), data.Included, data.FilterAttributeId);
				ReadOnlyItemAttributeChildFrequencyList result = dp.Fetch(criteria);


                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogException(e, "GetFrequencies data portal error");
                throw;
            }

		}
               
    }
}


public class Criteria
{
	public string AttributeId { get; set; }
	public string SetId { get; set; }
	public bool Included { get; set; }
	public int FilterAttributeId { get; set; } 
	
}

