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

        private readonly ILogger _logger;

        public PriorirtyScreeningController(ILogger<PriorirtyScreeningController> logger)
        {

            _logger = logger;
        }


        [HttpGet("[action]")]
        public IActionResult TrainingList()
        {
            try
            {
                SetCSLAUser();
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
                    DataPortal<ReviewInfo> dpInfo = new DataPortal<ReviewInfo>();
                    ReviewInfo revInfo = dpInfo.Fetch();
                    command.RevInfo = revInfo;
                    DataPortal<TrainingRunCommand> dp = new DataPortal<TrainingRunCommand>();
                    //Task<TrainingRunCommand> doIt = new Task<TrainingRunCommand>(() => dp.Execute(command), );
                    //doIt.Start();
                    TrainingRunCommand result = dp.Execute(command);

                    //return Ok(result);
                    return Ok(command);
                }
                else return Forbid();
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Error with the dataportal training run command!");
                return StatusCode(500, e.Message);
            }
        }
    }
}