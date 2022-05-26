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
    public class PriorirtyScreeningController : CSLAController
    {
        
        public PriorirtyScreeningController(ILogger<PriorirtyScreeningController> logger) : base(logger)
        { }


        [HttpGet("[action]")]
        public IActionResult TrainingList()
        {
            try
            {
                if (!SetCSLAUser()) return Unauthorized();
                DataPortal<TrainingList> dp = new DataPortal<TrainingList>();
                TrainingList result = dp.Fetch();
                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Error with the dataportal training list logic");
                return StatusCode(500, e.Message);
            }
        }
        [HttpPost("[action]")]
        public IActionResult TrainingNextItem([FromBody] SingleIntCriteria crit)
        {
            try
            {
                if (SetCSLAUser4Writing())
                {
                    DataPortal<TrainingNextItem> dp = new DataPortal<TrainingNextItem>();
                    TrainingNextItem result = dp.Fetch(new SingleCriteria<TrainingNextItem, int>(crit.Value));
                    return Ok(result);
                }
                else return Forbid();
            }
            catch (Exception e)
            {
                string json = JsonConvert.SerializeObject(crit);
                _logger.LogError(e, "Dataportal Error with Training of the next item: {0}", json);
                return StatusCode(500, e.Message);
            }
        }
        [HttpPost("[action]")]
        public IActionResult TrainingPreviousItem([FromBody] SingleInt64Criteria crit)
        {
            try
            {
                if (SetCSLAUser4Writing())
                {
                    DataPortal<TrainingPreviousItem> dp = new DataPortal<TrainingPreviousItem>();
                    TrainingPreviousItem result = dp.Fetch(new SingleCriteria<TrainingPreviousItem, Int64>(crit.Value));
                    return Ok(result);
                }
                else return Forbid();
            }
            catch (Exception e)
            {
                string json = JsonConvert.SerializeObject(crit);
                _logger.LogError(e, "Dataportal Error with Training of the previous item: {0}", json);
                return StatusCode(500, e.Message);
            }
        }
        [HttpGet("[action]")]
        public IActionResult TrainingRunCommand()
        {
            try
            {
                if (SetCSLAUser4Writing())
                {
                    TrainingRunCommand command = new TrainingRunCommand();
                    ReviewInfo revInfo = DataPortal.Fetch<ReviewInfo>();
                    command.RevInfo = revInfo;
                    DataPortal<TrainingRunCommand> dp = new DataPortal<TrainingRunCommand>();
                    //Task<TrainingRunCommand> doIt = new Task<TrainingRunCommand>(() => dp.Execute(command), );
                    //doIt.Start();
                    TrainingRunCommand result = dp.Execute(command);
                    System.Threading.Thread.Sleep(15*1000);
                    ReviewInfo rInfo = DataPortal.Fetch<ReviewInfo>();
                    result.RevInfo = rInfo;
                    //return Ok(result);
                    return Ok(result);
                }
                else return Forbid();
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Error with the dataportal training run command!");
                return StatusCode(500, e.Message);
            }
        }
        [HttpGet("[action]")]
        public IActionResult GetTrainingScreeningCriteriaList()
        {
            try
            {
                if (!SetCSLAUser()) return Unauthorized();
                TrainingScreeningCriteriaList result = DataPortal.Fetch<TrainingScreeningCriteriaList>();
                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Error with the dataportal TrainingScreeningCriteriaList");
                return StatusCode(500, e.Message);
            }
        }
        [HttpPost("[action]")]
        public IActionResult UpdateTrainingScreeningCriteria([FromBody] TrainingScreeningCriteriaMVC crit)
        {
            try
            {
                if (SetCSLAUser4Writing())
                {
                    TrainingScreeningCriteriaList result = DataPortal.Fetch<TrainingScreeningCriteriaList>();
                    TrainingScreeningCriteria updating = result.First(found => found.TrainingScreeningCriteriaId == crit.trainingScreeningCriteriaId);
                    if (updating == null) return NotFound();
                    if (crit.deleted)
                    {
                        updating.Delete();
                        TrainingScreeningCriteria deleted = updating.Save(true);
                        result.Remove(updating);
                    }
                    else
                    {
                        updating.Included = crit.included;
                        updating = updating.Save(true);
                    }
                    return Ok(result);
                }
                else return Forbid();
            }
            catch (Exception e)
            {
                string json = JsonConvert.SerializeObject(crit);
                _logger.LogError(e, "Dataportal Error with updating TrainingScreeningCriteria: {0}", json);
                return StatusCode(500, e.Message);
            }
        }
        [HttpPost("[action]")]
        public IActionResult AddTrainingScreeningCriteria([FromBody] TrainingScreeningCriteriaMVC crit)
        {
            try
            {
                if (SetCSLAUser4Writing())
                {
                    TrainingScreeningCriteria newC = new TrainingScreeningCriteria();
                    newC.AttributeId = crit.trainingScreeningCriteriaId;//we are cheating here, and using the CriteriaId to store the attributeID...
                    newC.Included = crit.included;
                    newC.ApplyEdit();
                    newC = DataPortal.Update(newC);
                    
                    TrainingScreeningCriteriaList result = DataPortal.Fetch<TrainingScreeningCriteriaList>();
                    return Ok(result);
                }
                else return Forbid();
            }
            catch (Exception e)
            {
                string json = JsonConvert.SerializeObject(crit);
                _logger.LogError(e, "Dataportal Error with updating AddTrainingScreeningCriteria: {0}", json);
                return StatusCode(500, e.Message);
            }
        }
        [HttpPost("[action]")]
        public IActionResult ReplaceTrainingScreeningCriteriaList([FromBody] TrainingScreeningCriteriaMVC[] crit)
        {
            try
            {
                if (SetCSLAUser4Writing())
                {
                    TrainingScreeningCriteriaListDeleteAllCommand cmd = new TrainingScreeningCriteriaListDeleteAllCommand();
                    cmd = DataPortal.Execute<TrainingScreeningCriteriaListDeleteAllCommand>(cmd);
                    foreach (TrainingScreeningCriteriaMVC input in crit)
                    {
                        TrainingScreeningCriteria newC = new TrainingScreeningCriteria();
                        newC.AttributeId = input.trainingScreeningCriteriaId;//we are cheating here, and using the CriteriaId to store the attributeID...
                        newC.Included = input.included;
                        newC.ApplyEdit();
                        newC = DataPortal.Update(newC);
                    }
                    TrainingScreeningCriteriaList result = DataPortal.Fetch<TrainingScreeningCriteriaList>();
                    return Ok(result);
                }
                else return Forbid();
            }
            catch (Exception e)
            {
                string json = JsonConvert.SerializeObject(crit);
                _logger.LogError(e, "Dataportal Error with updating ReplaceTrainingScreeningCriteriaList: {0}", json);
                return StatusCode(500, e.Message);
            }
        }
    }
    public class TrainingScreeningCriteriaMVC
    {
        public long trainingScreeningCriteriaId { get; set; }
        public bool included { get; set; }
        public bool deleted { get; set; }
    }
}