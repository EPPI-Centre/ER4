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

        [HttpGet("[action]")]
        public IActionResult GetFrequencies(int set_id, Int64 attribute_id, bool isIncluded, Int64 filterAttributeId )
        {

			Type type = null;
			set_id = 27;
			attribute_id = 0;
			isIncluded = true;
			filterAttributeId = -1;


			try
            {
                SetCSLAUser();
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;

                DataPortal<ReadOnlyItemAttributeChildFrequencyList> dp = new DataPortal<ReadOnlyItemAttributeChildFrequencyList>();

				ItemAttributeChildFrequencySelectionCriteria criteria = new ItemAttributeChildFrequencySelectionCriteria(type, attribute_id, set_id, isIncluded, filterAttributeId);
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
