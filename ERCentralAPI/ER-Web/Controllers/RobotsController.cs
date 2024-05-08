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

namespace ERxWebClient2.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class RobotsController : CSLAController
    {
        
        public RobotsController(ILogger<FrequenciesController> logger) : base(logger)
        { }

        [HttpPost("[action]")]
        public IActionResult RunRobotOpenAICommand([FromBody] RobotOpenAICommandJson data)
        {

			try
            {
                if (!SetCSLAUser4Writing()) return Unauthorized();
                RobotOpenAICommand res = DataPortal.Execute<RobotOpenAICommand>(data.GetRobotOpenAICommand());
                return Ok(res);
            }
            catch (Exception e)
            {
                _logger.LogException(e, "RunRobotOpenAICommand error");
                return StatusCode(500, e.Message);
            }

		}
               
    }
}


public class RobotOpenAICommandJson
{
    public int reviewSetId;
    public Int64  itemDocumentId;
    public Int64  itemId;
    public bool onlyCodeInTheRobotName { get; set; }
    public bool lockTheCoding { get; set; }
    public string returnMessage = "";
    public RobotOpenAICommand GetRobotOpenAICommand()
    {
        RobotOpenAICommand res = new RobotOpenAICommand(reviewSetId, itemId, itemDocumentId, onlyCodeInTheRobotName, lockTheCoding);
        return res;
    }
}

