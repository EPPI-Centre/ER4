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

        
        public IActionResult GetFrequencies([FromForm] long attId, int setId, string included)
        {
            try
            {
                if (SetCSLAUser())
                {
                    FrequencyResultWithCriteria Itm = GetFrequenciesInternal(attId, setId, included);
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
        public IActionResult GetFrequenciesJSON([FromForm] long attId, int setId, string included)
        {//we provide all items details in a single JSON method, as it makes no sense to get partial item details, so without Arms, Docs, etc.
            try
            {
                if (SetCSLAUser())
                {
                    FrequencyResultWithCriteria Itm = GetFrequenciesInternal(attId, setId, included);
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
        internal FrequencyResultWithCriteria GetFrequenciesInternal(long attId, int setId, string included)
        {
            int DBid = -1;
            List<Claim> claims = User.Claims.ToList();
            Claim DBidC = claims.Find(f => f.Type == "WebDbID");
            if (DBidC != null)
            {
                int.TryParse(DBidC.Value, out DBid);
            }
            if (DBid < 1)
            {
                _logger.LogError("Error in GetFrequenciesInternal, no WebDbId!");
                return null;
            }
            FrequencyResultWithCriteria res = new FrequencyResultWithCriteria();
            res.criteria = 
                new WebDbFrequencyCrosstabAndMapSelectionCriteria(DBid, attId, setId, included);
            res.results = DataPortal.Fetch<WebDbItemAttributeChildFrequencyList>(res.criteria);
            return res;
        }

        public IActionResult GetCrosstab([FromForm] long attIdx, int setIdx, long attIdy, int setIdy, string included)
        {

            try
            {
                if (SetCSLAUser())
                {
                    WebDbItemAttributeCrosstabList Itm = GetCrosstabInternal(attIdx, setIdx, attIdy, setIdy, included);
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
        public IActionResult GetCrosstabJSON([FromForm] long attIdx, int setIdx, long attIdy, int setIdy, string included)
        {//we provide all items details in a single JSON method, as it makes no sense to get partial item details, so without Arms, Docs, etc.
            try
            {
                if (SetCSLAUser())
                {
                    WebDbItemAttributeCrosstabList Itm = GetCrosstabInternal(attIdx, setIdx, attIdy, setIdy, included);
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
        internal WebDbItemAttributeCrosstabList GetCrosstabInternal(long attIdx, int setIdx, long attIdy, int setIdy, string included, long onlyThisAtt= 0)
        {
            int DBid = -1;
            List<Claim> claims = User.Claims.ToList();
            Claim DBidC = claims.Find(f => f.Type == "WebDbID");
            if (DBidC != null)
            {
                int.TryParse(DBidC.Value, out DBid);
            }
            if (DBid < 1)
            {
                _logger.LogError("Error in GetFrequenciesInternal, no WebDbId!");
                return null;
            }
            //FrequencyResultWithCriteria res = new FrequencyResultWithCriteria();
            //res.criteria =
            //    new WebDbFrequencyCrosstabAndMapSelectionCriteria(DBid, attIdx, setIdy, included, onlyThisAtt, attIdy, setIdx);
            //res.results = DataPortal.Fetch<WebDbItemAttributeChildFrequencyList>(res.criteria);
            WebDbFrequencyCrosstabAndMapSelectionCriteria crit = new WebDbFrequencyCrosstabAndMapSelectionCriteria(DBid, attIdx, setIdx, included, onlyThisAtt, attIdy, setIdy);
            WebDbItemAttributeCrosstabList res = DataPortal.Fetch<WebDbItemAttributeCrosstabList>(crit);
            return res;
        }

    }
    public class WebDbFrequencyCrosstabAndMapSelectionCriteriaMVC
    {
        public WebDbFrequencyCrosstabAndMapSelectionCriteriaMVC() { }
        public WebDbFrequencyCrosstabAndMapSelectionCriteriaMVC(WebDbFrequencyCrosstabAndMapSelectionCriteria crit)
        {
            webDbId = crit.webDbId;
            attributeIdXAxis = crit.attributeIdXAxis;
            setIdXAxis = crit.setIdXAxis;
            attributeIdYAxis = crit.attributeIdYAxis;
            setIdYAxis = crit.setIdYAxis;
            included = crit.included;
            segmentsParent = crit.segmentsParent;
            setIdSegments = crit.setIdSegments;
            onlyThisAttribute = crit.onlyThisAttribute;
        }
        public WebDbFrequencyCrosstabAndMapSelectionCriteria GetWebDbFrequencyCrosstabAndMapSelectionCriteria()
        {
            WebDbFrequencyCrosstabAndMapSelectionCriteria res = new WebDbFrequencyCrosstabAndMapSelectionCriteria(
                                                                        webDbId, attributeIdXAxis, setIdXAxis,
                                                                        included, onlyThisAttribute,
                                                                        attributeIdYAxis, setIdYAxis,
                                                                        segmentsParent, setIdSegments);
            return res;
        }
        public int webDbId { get; set; }
        public Int64 attributeIdXAxis { get; set; }
        public int setIdXAxis { get; set; }
        public Int64 attributeIdYAxis { get; set; }
        public int setIdYAxis { get; set; }
        public string included { get; set; }
        public Int64 segmentsParent { get; set; }
        public int setIdSegments { get; set; }
        public Int64 onlyThisAttribute { get; set; }
    }
}