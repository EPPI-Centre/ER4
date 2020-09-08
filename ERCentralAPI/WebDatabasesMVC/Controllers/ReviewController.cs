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

namespace Klasifiki.Controllers
{
    [Authorize]
    public class ReviewController : CSLAController
    {
        
        public ReviewController(ILogger<LoginController> logger) : base(logger)
        {}


        public IActionResult Index()
        {
            try
            {
                if (SetCSLAUser())
                {
                    long attId = -1;
                    List<Claim> claims = User.Claims.ToList();
                    Claim AttIdC = claims.Find(f => f.Type == "ItemsCode");
                    if (AttIdC != null)
                    {
                        long.TryParse(AttIdC.Value, out attId);
                    }
                    ViewBag.AttId = attId;
                    ReviewSetsList rssl = DataPortal.Fetch<ReviewSetsList>();
                    AttributeSet aSet = rssl.GetAttributeSetFromAttributeId(attId);
                    if (aSet != null)
                    {
                        ViewBag.AttName = aSet.AttributeName;
                    }
                    else ViewBag.AttName = "Unknown";
                    ReviewInfo rinfo = DataPortal.Fetch<ReviewInfo>();
                    return View(rinfo);
                }
                else return Unauthorized();
            } 
            catch (Exception e)
            {
                _logger.LogError(e, "Error in Review Index");
                return StatusCode(500, e.Message);
            }
        }
    }
}