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

namespace WebDatabasesMVC.Controllers
{
    [Authorize(AuthenticationSchemes = "CookieAuthentication,FairAuthentication,VawgAuthentication")]
    public class CodingController : CSLAController
    {
        
        public CodingController(ILogger<CodingController> logger) : base(logger)
        {}

        [HttpPost]
        public IActionResult FetchItemCodingJSON([FromForm] long ItemId)
        {
            try
            {
                if (SetCSLAUser())
                {
                    int DBid = WebDbId;
                    ItemCodingWithCodingTools res = new ItemCodingWithCodingTools();
                    if (DBid > 0)
                    {
                        res.reviewSets  = DataPortal.Fetch<WebDbReviewSetsList>(new SingleCriteria<WebDbReviewSetsList, int>(DBid));
                        WebDbItemSetListSelectionCriteria crit = new WebDbItemSetListSelectionCriteria(DBid, ItemId);
                        res.itemSetList = DataPortal.Fetch<WebDbItemSetList>(crit);
                        
                        //bit of logic that removes outcomes in case all code-types that relate to outcomes are hidden (4,5,6,9)
                        foreach (WebDbReviewSet rSet in res.reviewSets)
                        {
                            if (!rSet.CanHaveOutcomes())
                            {
                                List<WebDbItemSet> list = res.itemSetList.ToList().FindAll(f => f.SetId == rSet.SetId);
                                foreach (WebDbItemSet iSet in list)
                                {
                                    iSet.OutcomeItemList.Clear();
                                }
                            }
                        }

                    }
                    return Json(res);
                }
                else return Unauthorized();
            } 
            catch (Exception e)
            {
                _logger.LogError(e, "Error in ReviewSetList FetchJSON");
                return StatusCode(500, e.Message);
            }
        }
    }
}