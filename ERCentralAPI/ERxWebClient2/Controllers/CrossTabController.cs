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
    public class CrossTabController : CSLAController
    {

        private readonly ILogger _logger;

        public CrossTabController(ILogger<CrossTabController> logger)
        {

            _logger = logger;
        }

        [HttpPost("[action]")]
        public IActionResult GetCrossTabs([FromBody] CrossTabCriteria data)
        {

			Type type = null;

			try
            {
                SetCSLAUser();
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;

                DataPortal<ReadOnlyItemAttributeCrosstabList> dp = new DataPortal<ReadOnlyItemAttributeCrosstabList>();

				ItemAttributeCrosstabSelectionCriteria criteria = new ItemAttributeCrosstabSelectionCriteria(type, data.attributeIdXAxis, data.setIdXAxis, data.attributeIdYAxis, data.setIdYAxis,
					data.attributeIdFilter, data.setIdFilter, data.nxaxis);
				ReadOnlyItemAttributeCrosstabList result = dp.Fetch(criteria);

				//var info = typeof(ReadOnlyItemAttributeCrosstabList).GetProperties();

				//for (int i = 0; i < info.Count(); i++)
				//{
				//	if (info[i].Name.Contains("Field")
				//	{

				//	}
				//}


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


public class CrossTabCriteria
{
	public long attributeIdXAxis { get; set; }
	public int setIdXAxis { get; set; }
	public long attributeIdYAxis { get; set; }
	public int setIdYAxis { get; set; }
	public long attributeIdFilter { get; set; }
	public int setIdFilter { get; set; }
	public int nxaxis { get; set; }

}

