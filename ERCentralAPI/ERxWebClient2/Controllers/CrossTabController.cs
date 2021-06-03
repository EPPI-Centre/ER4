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
        
        public CrossTabController(ILogger<CrossTabController> logger) : base(logger)
        { }

        [HttpPost("[action]")]
        public IActionResult GetCrossTabs([FromBody] CrossTabCriteria data)
        {

			Type type = null;

			try
            {
                if (!SetCSLAUser()) return Unauthorized();
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;

                //DataPortal<ReadOnlyItemAttributeCrosstabList> dp = new DataPortal<ReadOnlyItemAttributeCrosstabList>();

                //ItemAttributeCrosstabSelectionCriteria criteria = new ItemAttributeCrosstabSelectionCriteria(type, data.attributeIdXAxis, data.setIdXAxis, 
                //	data.attributeIdYAxis, data.setIdYAxis,
                //	data.attributeIdFilter, data.setIdFilter, data.nxaxis);
                //ReadOnlyItemAttributeCrosstabList result = dp.Fetch(criteria);

                WebDbFrequencyCrosstabAndMapSelectionCriteria criteria = new WebDbFrequencyCrosstabAndMapSelectionCriteria(0, data.attributeIdXAxis
                                            , data.setIdXAxis, "", data.onlyIncluded, "", data.attributeIdFilter, data.attributeIdYAxis, data.setIdYAxis, "");
                WebDbItemAttributeCrosstabList result = DataPortal.Fetch<WebDbItemAttributeCrosstabList>(criteria);

                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogException(e, "GetFrequencies data portal error");
                return StatusCode(500, e.Message);
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
    public string onlyIncluded { get; set; }

}

