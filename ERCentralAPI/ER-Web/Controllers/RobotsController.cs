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
using System.Numerics;

namespace ERxWebClient2.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class RobotsController : CSLAController
    {
        
        public RobotsController(ILogger<RobotsController> logger) : base(logger)
        { }
        [HttpGet("[action]")]
        public IActionResult GetRobotsList()
        {

            try
            {
                if (!SetCSLAUser()) return Unauthorized();
                RobotCoderReadOnlyList res = DataPortal.Fetch<RobotCoderReadOnlyList>();
                return Ok(res);
            }
            catch (Exception e)
            {
                _logger.LogException(e, "GetRobotsList error");
                return StatusCode(500, e.Message);
            }

        }


        [HttpGet("[action]")]
        public IActionResult GetCurrentJobsQueue()
        {

            try
            {
                if (!SetCSLAUser()) return Unauthorized();
                RobotOpenAiTaskReadOnlyList res = DataPortal.Fetch<RobotOpenAiTaskReadOnlyList>();
                return Ok(res);
            }
            catch (Exception e)
            {
                _logger.LogException(e, "GetCurrentJobsQueue error");
                return StatusCode(500, e.Message);
            }

        }
        [HttpGet("[action]")]
        public IActionResult GetPastJobs()
        {

            try
            {
                if (!SetCSLAUser()) return Unauthorized();
                RobotOpenAiTaskCriteria crit = RobotOpenAiTaskCriteria.NewPastJobsCriteria();
                RobotOpenAiTaskReadOnlyList res = DataPortal.Fetch<RobotOpenAiTaskReadOnlyList>(crit);
                return Ok(res);
            }
            catch (Exception e)
            {
                _logger.LogException(e, "GetPastJobs error");
                return StatusCode(500, e.Message);
            }

        }
        [HttpPost("[action]")]
        public IActionResult RunRobotOpenAICommand([FromBody] RobotOpenAICommandJson data)
        {

			try
            {
                if (!SetCSLAUser4Writing()) return Unauthorized();
                LLMRobotCommand res = DataPortal.Execute<LLMRobotCommand>(data.GetRobotOpenAICommand());
                return Ok(res);
            }
            catch (Exception e)
            {
                _logger.LogException(e, "RunRobotOpenAICommand error");
                return StatusCode(500, e.Message);
            }

		}
        [HttpPost("[action]")]
        public IActionResult RunRobotInvestigateCommand([FromBody] RobotInvestigateCommandJson data)
        {

            try
            {
                if (!SetCSLAUser4Writing()) return Unauthorized();
                RobotInvestigateCommand res = DataPortal.Execute<RobotInvestigateCommand>(data.GetRobotInvestigateCommand());
                return Ok(res);
            }
            catch (Exception e)
            {
                _logger.LogException(e, "RobotInvestigateCommand error");
                return StatusCode(500, e.Message);
            }

        }
        [HttpPost("[action]")]
        public IActionResult EnqueueRobotOpenAIBatch([FromBody] RobotOpenAiQueueBatchJobCommandJson data)
        {

            try
            {
                if (!SetCSLAUser4Writing()) return Unauthorized();
                else
                {
                    ReviewInfo rinfo = DataPortal.Fetch<ReviewInfo>();
                    if (rinfo == null) return Unauthorized();
                    if (rinfo.CanUseRobots == false)
                    {
                        data.returnMessage = "Error. Could not find credit available to spend on the OpenAI Robot.";
                        return Ok(data);
                    }
                    int CreditId = 0;
                    if (rinfo.CreditForRobotsList.Count > 0)
                    {
                        CreditId = rinfo.CreditForRobotsList[0].CreditPurchaseId;
                    }
                    RobotOpenAiQueueBatchJobCommand res = new RobotOpenAiQueueBatchJobCommand(data.robotName, data.criteria, CreditId, data.reviewSetId, data.onlyCodeInTheRobotName, data.lockTheCoding, data.useFullTextDocument);
                    res = DataPortal.Execute(res);
                    data.returnMessage = res.Result;
                    return Ok(data);
                }
            }
            catch (Exception e)
            {
                _logger.LogException(e, "EnqueueRobotOpenAIBatch error");
                return StatusCode(500, e.Message);
            }

        }
        [HttpPost("[action]")]
        public IActionResult CancelRobotOpenAIBatch([FromBody] SingleIntCriteria crit)
        {
            try
            {
                if (!SetCSLAUser4Writing()) return Unauthorized();
                else
                {
                    RobotOpenAiCancelQueuedBatchJobCommand res = new RobotOpenAiCancelQueuedBatchJobCommand(crit.Value);
                    res = DataPortal.Execute(res);
                    return Ok(res);
                }
            }
            catch (Exception e)
            {
                _logger.LogException(e, "CancelRobotOpenAIBatch error");
                return StatusCode(500, e.Message);
            }

        }
    }
}


public class RobotOpenAICommandJson
{
    public string robotName { get; set; } = "OpenAI GPT4";
    public string endPoint { get; set; } = "";
    public int reviewSetId;
    public Int64 itemId;
    public bool onlyCodeInTheRobotName { get; set; }
    public bool lockTheCoding { get; set; }
    public bool useFullTextDocument { get; set; }
    public string returnMessage = "";
    public LLMRobotCommand GetRobotOpenAICommand()
    {
        RobotCoderReadOnly robot = DataPortal.Fetch<RobotCoderReadOnly>(new SingleCriteria<RobotCoderReadOnly, string>(robotName));
        LLMRobotCommand res = LLM_Factory.GetRobot(robot, reviewSetId, itemId, onlyCodeInTheRobotName, lockTheCoding, useFullTextDocument);
        return res;
    }
}

public class RobotInvestigateCommandJson
{
    public string robotName { get; set; } = "OpenAI GPT4";
    public string queryForRobot = "";
    public string getTextFrom = "";
    public Int64 itemsWithThisAttribute;
    public Int64 textFromThisAttribute;
    public int sampleSize = 20;
    public string returnMessage = "";
    public string returnResultText = "";
    public string returnItemIdList = "";
    public RobotInvestigateCommand GetRobotInvestigateCommand()
    {
        RobotInvestigateCommand res = new RobotInvestigateCommand( queryForRobot, getTextFrom, itemsWithThisAttribute, textFromThisAttribute, sampleSize);
        return res;
    }
}

public class RobotOpenAiQueueBatchJobCommandJson
{
    public int reviewSetId;
    public Int64 itemDocumentId;
    public string criteria { get; set; } = "";
    public string robotName { get; set; } = "OpenAI GPT4";
    public bool onlyCodeInTheRobotName { get; set; }
    public bool lockTheCoding { get; set; }
    public bool useFullTextDocument { get; set; }
    public string returnMessage = "";
    
}

