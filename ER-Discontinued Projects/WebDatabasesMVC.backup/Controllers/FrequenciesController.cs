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
    [Authorize(AuthenticationSchemes = "CookieAuthentication")]
    public class FrequenciesController : CSLAController
    {
        
        public FrequenciesController(ILogger<FrequenciesController> logger) : base(logger)
        {}

        
        public IActionResult GetFrequencies([FromForm] long attId, int setId, string parentName, string included, long onlyThisAttribute = 0)
        {
            try
            {
                if (SetCSLAUser())
                {
                    int DBid = WebDbId;
                    if (DBid < 1)
                    {
                        _logger.LogError("Error in GetFrequencies, no WebDbId!");
                        return null;
                    }
                    WebDbFrequencyCrosstabAndMapSelectionCriteria res = new WebDbFrequencyCrosstabAndMapSelectionCriteria(DBid, attId, setId, parentName, included);
                    return View(res);
                }
                else return Unauthorized();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error in GetFrequencies");
                return StatusCode(500, e.Message);
            }
        }

        public IActionResult GetFrequenciesJSON([FromForm] long attId, int setId, string parentName, string included, long onlyThisAttribute = 0)
        {
            return internalGetFrequenciesJSON(attId, setId, parentName, included, onlyThisAttribute);
        }
        [Authorize(AuthenticationSchemes = "FairAuthentication")]
        public IActionResult FairGetFrequenciesJSON([FromForm] long attId, int setId, string parentName, string included, long onlyThisAttribute = 0)
        {
            return internalGetFrequenciesJSON(attId, setId, parentName, included, onlyThisAttribute);
        }
        private IActionResult internalGetFrequenciesJSON([FromForm] long attId, int setId, string parentName, string included, long onlyThisAttribute)
        {//we provide all items details in a single JSON method, as it makes no sense to get partial item details, so without Arms, Docs, etc.
            try
            {
                if (SetCSLAUser())
                {
                    FrequencyResultWithCriteria Itm = GetFrequenciesInternal(attId, setId, parentName, included, onlyThisAttribute);

                    // log to TB_WEBDB_LOG
                    string type = "GetFrequency";
                    string details = Itm.criteria.attributeIdXAxis.ToString();
                    if (Itm.criteria.attributeIdXAxis.ToString() == "0")
                    {
                        type = "GetSetFrequency";
                        details = Itm.criteria.setIdXAxis.ToString();
                    }
                    logActivity(type, details);

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


        [HttpPost]
        public IActionResult GetFrequenciesResultsJSON(WebDbFrequencyCrosstabAndMapSelectionCriteriaMVC crit)
        {//we provide all items details in a single JSON method, as it makes no sense to get partial item details, so without Arms, Docs, etc.
            try
            {
                if (SetCSLAUser())
                {
                    int DBid = WebDbId;
                    if (DBid < 1)
                    {
                        _logger.LogError("Error in GetFrequenciesResultsJSON, no WebDbId!");
                        return null;
                    }
                    else crit.webDbId = DBid;
                    WebDbItemAttributeChildFrequencyList results = DataPortal.Fetch<WebDbItemAttributeChildFrequencyList>(crit.GetWebDbFrequencyCrosstabAndMapSelectionCriteria());

                    // log to TB_WEBDB_LOG
                    logActivity("GetFrequencyNewPage", crit.nameXAxis);

                    return Json(results);
                }
                else return Unauthorized();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error in GetFrequenciesResultsJSON");
                return StatusCode(500, e.Message);
            }
        }
        internal FrequencyResultWithCriteria GetFrequenciesInternal(long attId, int setId, string parentName, string included, long onlyThisAttribute)
        {
            int DBid = WebDbId;
            if (DBid < 1)
            {
                _logger.LogError("Error in GetFrequenciesInternal, no WebDbId!");
                return null;
            }
            FrequencyResultWithCriteria res = new FrequencyResultWithCriteria();
            res.criteria = 
                new WebDbFrequencyCrosstabAndMapSelectionCriteria(DBid, attId, setId, parentName, included, "", onlyThisAttribute);
            res.results = DataPortal.Fetch<WebDbItemAttributeChildFrequencyList>(res.criteria);
            return res;
        }

        public IActionResult GetCrosstab([FromForm] long attIdx, int setIdx, string nameXaxis, long attIdy, int setIdy, string nameYaxis, string included, string graphic)
        {

            try
            {
                if (SetCSLAUser())
                {
                    int DBid = WebDbId;
                    if (DBid < 1)
                        if (DBid < 1)
                        {
                            _logger.LogError("Error in GetFrequenciesInternal, no WebDbId!");
                            return null;
                        }

                    WebDbFrequencyCrosstabAndMapSelectionCriteria crit = new WebDbFrequencyCrosstabAndMapSelectionCriteria(DBid, attIdx, setIdx, nameXaxis, included, graphic, 0, attIdy, setIdy, nameYaxis);
                    return View(crit);
                }
                else return Unauthorized();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error in GetCrosstab");
                return StatusCode(500, e.Message);
            }
        }
        [HttpPost]
        public IActionResult GetCrosstabJSON(long attIdx, int setIdx, string nameXaxis, long attIdy, int setIdy, string nameYaxis, string included, string graphic)
        {//we provide all items details in a single JSON method, as it makes no sense to get partial item details, so without Arms, Docs, etc.
            try
            {
                if (SetCSLAUser())
                {
                    WebDbItemAttributeCrosstabList Itm = GetCrosstabInternal(attIdx, setIdx, nameXaxis, attIdy, setIdy, nameYaxis, included, graphic);

                    // log to TB_WEBDB_LOG
                    string type = "GetCrosstab";
                    string details = "";
                    if ((Itm.AttibuteIdX == 0) && (Itm.AttibuteIdY == 0))
                    {
                        details = "(Column) " + Itm.SetIdXName + " vs (Row) " + Itm.SetIdYName;
                    }
                    if ((Itm.AttibuteIdX != 0) && (Itm.AttibuteIdY != 0))
                    {
                        details = "(Column) " + Itm.AttibuteIdXName + " vs (Row) " + Itm.AttibuteIdYName;
                    }
                    if ((Itm.AttibuteIdX == 0) && (Itm.AttibuteIdY != 0))
                    {
                        details = "(Column) " + Itm.SetIdXName + " vs (Row) " + Itm.AttibuteIdYName;
                    }
                    if ((Itm.AttibuteIdX != 0) && (Itm.AttibuteIdY == 0))
                    {
                        details = "(Column) " + Itm.AttibuteIdXName + " vs (Row) " + Itm.SetIdYName;
                    }
                    logActivity(type, details);

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
        internal WebDbItemAttributeCrosstabList GetCrosstabInternal(long attIdx, int setIdx, string nameXaxis, long attIdy, int setIdy, string nameYaxis, 
            string included, string graphic, long onlyThisAtt= 0)
        {
            int DBid = WebDbId;
            if (DBid < 1)
                if (DBid < 1)
            {
                _logger.LogError("Error in GetFrequenciesInternal, no WebDbId!");
                return null;
            }
            
            WebDbFrequencyCrosstabAndMapSelectionCriteria crit = new WebDbFrequencyCrosstabAndMapSelectionCriteria(DBid, attIdx, setIdx, nameXaxis, included, graphic, onlyThisAtt, attIdy, setIdy, nameYaxis);
            WebDbItemAttributeCrosstabList res = DataPortal.Fetch<WebDbItemAttributeCrosstabList>(crit);
            return res;
        }

        //[HttpPost("[action]")]
        public IActionResult GetMap([FromForm] long attIdx, int setIdx, string nameXaxis, long attIdy, int setIdy, string nameYaxis,
                                    string included, string graphic, long segmentsParent, int setIdSegments)
        {
            try
            {
                if (SetCSLAUser())
                {
                    int DBid = WebDbId;
                    if (DBid < 1)
                    {
                        _logger.LogError("Error in GetAjaxMap, no WebDbId!");
                        return Unauthorized();
                    }
                    WebDbFrequencyCrosstabAndMapSelectionCriteria crit = new WebDbFrequencyCrosstabAndMapSelectionCriteria(DBid, attIdx, setIdx, nameXaxis, included, graphic, 0, attIdy, setIdy, nameYaxis, segmentsParent, setIdSegments);
                    return View(crit);
                }
                else return Unauthorized();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error in GetAjaxMap");
                return StatusCode(500, e.Message);
            }
        }
       
        [HttpPost]
        public IActionResult GetMapJSON(WebDbFrequencyCrosstabAndMapSelectionCriteriaMVC crit)
        {//we provide all items details in a single JSON method, as it makes no sense to get partial item details, so without Arms, Docs, etc.
            try
            {
                if (SetCSLAUser())
                {
                    int DBid = WebDbId;
                    if (DBid < 1)
                    {
                        _logger.LogError("Error in GetAjaxMap, no WebDbId!");
                        return Unauthorized();
                    } else
                    {
                        crit.webDbId = DBid;
                    }
                    WebDbItemAttributeCrosstabList Itm = DataPortal.Fetch<WebDbItemAttributeCrosstabList>(crit.GetWebDbFrequencyCrosstabAndMapSelectionCriteria());

                    // log to TB_WEBDB_LOG
                    string type = "GetMap";
                    string details = "";
                    if ((Itm.AttibuteIdX == 0) && (Itm.AttibuteIdY == 0))
                    {
                        details = "(Column) " + Itm.SetIdXName + " vs (Row) " + Itm.SetIdYName + ", Segments: " + Itm.AttibuteIdSegmentsName;
                    }
                    if ((Itm.AttibuteIdX != 0) && (Itm.AttibuteIdY != 0))
                    {
                        details = "(Column) " + Itm.AttibuteIdXName + " vs (Row) " + Itm.AttibuteIdYName + ", Segments: " + Itm.AttibuteIdSegmentsName;
                    }
                    if ((Itm.AttibuteIdX == 0) && (Itm.AttibuteIdY != 0))
                    {
                        details = "(Column) " + Itm.SetIdXName + " vs (Row) " + Itm.AttibuteIdYName + ", Segments: " + Itm.AttibuteIdSegmentsName;
                    }
                    if ((Itm.AttibuteIdX != 0) && (Itm.AttibuteIdY == 0))
                    {
                        details = "(Column) " + Itm.AttibuteIdXName + " vs (Row) " + Itm.SetIdYName + ", Segments: " + Itm.AttibuteIdSegmentsName;
                    }
                    logActivity(type, details);

                    return Json(Itm);
                }
                else return Unauthorized();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error in GetMapJSON");
                return StatusCode(500, e.Message);
            }
        }
        public IActionResult GetMapById([FromForm] int mapId)
        {
            try
            {
                if (SetCSLAUser())
                {
                    int DBid = WebDbId;
                    if (DBid < 1)
                    {
                        _logger.LogError("Error in GetMapById, no WebDbId!");
                        return Unauthorized();
                    }

                    WebDbMap map = DataPortal.Fetch<WebDbMap>(new WebDBMapCriteria(DBid, mapId));
                    if (map == null || map.WebDBMapId < 1)
                    {
                        _logger.LogError("Error in GetMapById, no such map! MapId: " + mapId);
                        return Unauthorized();
                    }
                    WebDbFrequencyCrosstabAndMapSelectionCriteria crit 
                        = new WebDbFrequencyCrosstabAndMapSelectionCriteria(DBid, map.ColumnsAttributeID, map.ColumnsSetID
                            , map.ColumnsPublicAttributeName != "" ? map.ColumnsPublicAttributeName : map.ColumnsPublicSetName, "true"
                            , "bubble", 0, map.RowsAttributeID, map.RowsSetID
                            , map.RowsPublicAttributeName != "" ? map.RowsPublicAttributeName : map.RowsPublicSetName
                            , map.SegmentsAttributeID, map.SegmentsSetID);
                    return View("GetMap", crit);
                }
                else return Unauthorized();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error in GetMapById");
                return StatusCode(500, e.Message);
            }
        }

        public IActionResult GetEPPIMapperMap([FromQuery] string eppiMapperMapUrl)
        {
            string url = eppiMapperMapUrl;
            WebDbFrequencyCrosstabAndMapSelectionCriteria crit = 
                new WebDbFrequencyCrosstabAndMapSelectionCriteria(0, 1, 1, url, "", "", 0, 1, 1, "");
            return View(crit);
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
                                                                        webDbId, attributeIdXAxis, setIdXAxis, nameXAxis,
                                                                        included, graphic, onlyThisAttribute,
                                                                        attributeIdYAxis, setIdYAxis, nameYAxis,
                                                                        segmentsParent, setIdSegments);
            return res;
        }
        public int webDbId { get; set; }
        public Int64 attributeIdXAxis { get; set; }
        public int setIdXAxis { get; set; }
        public string nameXAxis { get; set; }
        public string graphic { get; set; }
        public Int64 attributeIdYAxis { get; set; }
        public int setIdYAxis { get; set; }
        public string nameYAxis { get; set; }
        public string included { get; set; }
        public Int64 segmentsParent { get; set; }
        public int setIdSegments { get; set; }
        public Int64 onlyThisAttribute { get; set; }
    }
}