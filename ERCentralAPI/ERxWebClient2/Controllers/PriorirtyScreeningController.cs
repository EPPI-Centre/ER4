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

namespace ERxWebClient2.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class PriorirtyScreeningController : CSLAController
    {

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
                //add logging
                return StatusCode(500, e.Message);
            }
        }
        [HttpPost("[action]")]
        public IActionResult TrainingNextItem([FromBody] SingleIntCriteria crit)
        {
            try
            {
                SetCSLAUser();
                DataPortal<TrainingNextItem> dp = new DataPortal<TrainingNextItem>();
                TrainingNextItem result = dp.Fetch(new SingleCriteria<TrainingNextItem, int>(crit.Value));
                return Ok(result);
            }
            catch (Exception e)
            {
                //add logging
                return StatusCode(500, e.Message);
            }
        }
        [HttpPost("[action]")]
        public IActionResult TrainingPreviousItem([FromBody] SingleInt64Criteria crit)
        {
            try
            {
                SetCSLAUser();
                DataPortal<TrainingPreviousItem> dp = new DataPortal<TrainingPreviousItem>();
                TrainingPreviousItem result = dp.Fetch(new SingleCriteria<TrainingPreviousItem, Int64>(crit.Value));
                return Ok(result);
            }
            catch (Exception e)
            {
                //add logging
                return StatusCode(500, e.Message);
            }
        }
    }
}