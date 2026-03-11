using BusinessLibrary.BusinessClasses;
using BusinessLibrary.Security;
using Csla;
using EPPIDataServices.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Scripting.Hosting;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
                //TestCode();
                return Ok(res);
            }
            catch (Exception e)
            {
                _logger.LogException(e, "GetPastJobs error");
                return StatusCode(500, e.Message);
            }

        }
        [HttpGet("[action]")]
        public IActionResult GetAllPastJobs()
        {

            try
            {
                if (!SetCSLAUser()) return Unauthorized();
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                if (ri.IsSiteAdmin == false) return Unauthorized();
                RobotOpenAiTaskCriteria crit = RobotOpenAiTaskCriteria.NewAllPastJobsCriteria();
                RobotOpenAiTaskReadOnlyList res = DataPortal.Fetch<RobotOpenAiTaskReadOnlyList>(crit);
                //TestCode();
                return Ok(res);
            }
            catch (Exception e)
            {
                _logger.LogException(e, "GetPastJobs error");
                return StatusCode(500, e.Message);
            }

        }
        //private void TestCode()
        //{
        //    Dictionary<string, string> names = new Dictionary<string, string>();
        //    names.Add("Topolino", "1");
        //    names.Add("Pippo", "2");
        //    names.Add("Paperi.Paperone", "3");
        //    names.Add("Paperi.Qui", "4");
        //    names.Add("Paperi.Quo", "5");
        //    names.Add("Paperi.Qua", "6");
        //    names.Add("Fattoria.LupoAlberto", "7");
        //    names.Add("Fattoria.Marta", "8");
        //    names.Add("Fattoria.Talpe.Enrico", "9");
        //    names.Add("Fattoria.Talpe.Cesira", "10");
        //    JObject res = new JObject();

        //}

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
                    foreach(CreditForRobots cfb in rinfo.CreditForRobotsList)
                    {
                        if (cfb.AmountRemaining > 0.01) CreditId = cfb.CreditPurchaseId;
                    }
                    if (CreditId == 0)//added for safety
                    {
                        data.returnMessage = "Error. Could not find credit available to spend on the OpenAI Robot.";
                        return Ok(data);
                    }
                    RobotOpenAiQueueBatchJobCommand res = new RobotOpenAiQueueBatchJobCommand(data.robotName, data.criteria, CreditId,
                        data.reviewSetId, data.onlyCodeInTheRobotName, data.lockTheCoding, data.useFullTextDocument);
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
        // ************************************ Below here is OpenAi Prompt Evaluation *******************************
        [HttpPost("[action]")]
        public IActionResult EnqueueRobotOpenAIBatchJobEvaluation([FromBody] RobotOpenAiQueueBatchJobEvaluationCommandJson data)
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
                    foreach (CreditForRobots cfb in rinfo.CreditForRobotsList)
                    {
                        if (cfb.AmountRemaining > 0.01) CreditId = cfb.CreditPurchaseId;
                    }
                    if (CreditId == 0)//added for safety
                    {
                        data.returnMessage = "Error. Could not find credit available to spend on the OpenAI Robot.";
                        return Ok(data);
                    }
                    RobotOpenAiQueueBatchJobEvaluationCommand res = new RobotOpenAiQueueBatchJobEvaluationCommand(data.evaluationName, data.robotName
                        , CreditId, data.reviewSetId, data.reviewSetHtml, data.goldStandardAttributeId
                        , data.goldStandardAttributeName, data.useFullTextDocument, data.nIterations, data.nCodes);
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
        [HttpGet("[action]")]
        public IActionResult FetchRobotOpenAiPromptEvaluationList()
        {
            try
            {
                if (!SetCSLAUser()) return Unauthorized();
                DataPortal<RobotOpenAiPromptEvaluationList> dp = new DataPortal<RobotOpenAiPromptEvaluationList>();
                RobotOpenAiPromptEvaluationList result = dp.Fetch();
                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Error in FetchRobotOpenAiPromptEvaluationList");
                return StatusCode(500, e.Message);
            }
        }

        [HttpPost("[action]")]
        public IActionResult FetchRobotOpenAiPromptEvaluationDataList([FromBody] string crit)
        {
            try
            {
                if (!SetCSLAUser()) return Unauthorized();
                SingleCriteria<RobotOpenAiPromptEvaluationDataList, int> criteria =
                    new SingleCriteria<RobotOpenAiPromptEvaluationDataList, int>(Convert.ToInt32(crit));

                DataPortal<RobotOpenAiPromptEvaluationDataList> dp = new DataPortal<RobotOpenAiPromptEvaluationDataList>();
                RobotOpenAiPromptEvaluationDataList result = dp.Fetch(criteria);
                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Error in FetchRobotOpenAiPromptEvaluationDataList");
                return StatusCode(500, e.Message);
            }
        }

        [HttpPost("[action]")]
        public IActionResult DeleteRobotOpenAiPromptEvaluation([FromBody] string crit)
        {
            try
            {
                if (!SetCSLAUser4Writing()) return Unauthorized();
                SingleCriteria<RobotOpenAiPromptEvaluation, string> criteria =
                    new SingleCriteria<RobotOpenAiPromptEvaluation, string>(crit);

                DataPortal<RobotOpenAiPromptEvaluation> dp = new DataPortal<RobotOpenAiPromptEvaluation>();
                dp.Delete(criteria);
                return Ok();
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Error in DeleteRobotOpenAiPromptEvaluation");
                return StatusCode(500, e.Message);
            }
        }

        // ******************************* end OpenAI prompt evaluation ********************************************
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
    public int nIterations {get; set;}
    public LLMRobotCommand GetRobotOpenAICommand()
    {
        RobotCoderReadOnly robot = DataPortal.Fetch<RobotCoderReadOnly>(new SingleCriteria<RobotCoderReadOnly, string>(robotName));
        LLMRobotCommand res = LLM_Factory.GetRobot(robot, reviewSetId, itemId, onlyCodeInTheRobotName, lockTheCoding, useFullTextDocument, nIterations);
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
        RobotCoderReadOnly robot = DataPortal.Fetch<RobotCoderReadOnly>(new SingleCriteria<RobotCoderReadOnly, string>(robotName));
        RobotInvestigateCommand res = new RobotInvestigateCommand(robot, queryForRobot, getTextFrom, itemsWithThisAttribute, textFromThisAttribute, sampleSize);
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

public class RobotOpenAiQueueBatchJobEvaluationCommandJson
{
    public int reviewSetId;
    public string evaluationName { get; set; } = "";
    public string robotName { get; set; } = "OpenAI GPT4";
    public bool useFullTextDocument { get; set; }
    public string returnMessage = "";
    public int nIterations { get; set; }
    public int nCodes { get; set; }
    public string reviewSetHtml { get; set; } = "";
    public Int64 goldStandardAttributeId;
    public string goldStandardAttributeName { get; set; } = "";
}
