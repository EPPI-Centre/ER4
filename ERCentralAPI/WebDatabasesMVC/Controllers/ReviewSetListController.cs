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
    [Authorize]
    public class ReviewSetListController : CSLAController
    {
        
        public ReviewSetListController(ILogger<ReviewSetListController> logger) : base(logger)
        {}


        public IActionResult FetchJSON()
        {
            try
            {
                if (SetCSLAUser())
                {
                    int DBid = -1;
                    List<Claim> claims = User.Claims.ToList();
                    Claim DBidC = claims.Find(f => f.Type == "WebDbID");
                    if (DBidC != null)
                    {
                        int.TryParse(DBidC.Value, out DBid);
                    }
                    WebDbReviewSetsList reviewSets = null;
                    if (DBid > 0)
                    {
                        reviewSets = DataPortal.Fetch<WebDbReviewSetsList>(new SingleCriteria<WebDbReviewSetsList, int>(DBid));
                    }
                    return Json(reviewSets);
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