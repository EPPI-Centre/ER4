using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BusinessLibrary.BusinessClasses;
using Csla;
using EPPIDataServices.Helpers;
using ERxWebClient2.Controllers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WebDatabasesMVC;
using WebDatabasesMVC.ViewModels;

namespace WebDatabasesMVC.Controllers
{
    [Authorize]
    public class FrequenciesController : CSLAController
    {
        
        public FrequenciesController(ILogger<FrequenciesController> logger) : base(logger)
        {}

        
        public IActionResult GetFrequencies([FromForm] long attId, int setId, bool included)
        {
            try
            {
                if (SetCSLAUser())
                {
                    WebDbItemAttributeChildFrequencyList Itm = GetFrequenciesInternal(attId, setId, included);
                    return View(Itm);
                }
                else return Unauthorized();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error in GetFrequencies");
                return StatusCode(500, e.Message);
            }
        }
        [HttpPost("[action]")]
        public IActionResult GetFrequenciesJSON([FromForm] long attId, int setId, bool included)
        {//we provide all items details in a single JSON method, as it makes no sense to get partial item details, so without Arms, Docs, etc.
            try
            {
                if (SetCSLAUser())
                {
                    WebDbItemAttributeChildFrequencyList Itm = GetFrequenciesInternal(attId, setId, included);
                    return Json(Itm);
                }
                else return Unauthorized();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error in GetFrequenciesJSON");
                return StatusCode(500, e.Message);
            }
        }
        internal WebDbItemAttributeChildFrequencyList GetFrequenciesInternal(long attId, int setId, bool included)
        {
            int DBid = -1;
            List<Claim> claims = User.Claims.ToList();
            Claim DBidC = claims.Find(f => f.Type == "WebDbID");
            if (DBidC != null)
            {
                int.TryParse(DBidC.Value, out DBid);
            }
            WebDbItemAttributeChildFrequencyList res = new WebDbItemAttributeChildFrequencyList();
            if (DBid < 1)
            {
                _logger.LogError("Error in GetFrequenciesInternal, no WebDbId!");
                return res;
            }
            WebDbFrequencyCrosstabAndMapSelectionCriteria crit = 
                new WebDbFrequencyCrosstabAndMapSelectionCriteria(DBid, attId, setId, included);
            res = DataPortal.Fetch<WebDbItemAttributeChildFrequencyList>(crit);
            return res;
        }

    }
}