using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
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
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WebDatabasesMVC;

namespace WebDatabasesMVC.Controllers
{
    [Authorize]
    public class ReviewController : CSLAController
    {
        private static string _HeaderImagesFolder;
        private string HeaderImagesFolder
        {
            get
            {
                if (_HeaderImagesFolder == null)
                {
                    _HeaderImagesFolder = Path.Combine(webHostEnvironment.WebRootPath, "HeaderImages");
                }
                return _HeaderImagesFolder;
            }
        }

        private readonly IWebHostEnvironment webHostEnvironment;

        public ReviewController(ILogger<ReviewController> logger, IWebHostEnvironment hostEnvironment) : base(logger)
        {
            webHostEnvironment = hostEnvironment;
        }


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
                    int WebDbId = -1;
                    Claim DBidC = claims.Find(f => f.Type == "WebDbID");
                    if (DBidC != null)
                    {
                        int.TryParse(DBidC.Value, out WebDbId);
                    }
                    if (WebDbId < 1)
                    {
                        return BadRequest();
                    }
                     
                    //if (System.IO.File.Exists(HeaderImagesFolder + @"\Img-" + WebDbId.ToString() + "-1.jpg"))
                    //{
                    //    ViewBag.Img1 = @"Img-" + WebDbId.ToString() + "-1.jpg";
                    //}
                    //else if (System.IO.File.Exists(HeaderImagesFolder + @"\Img-" + WebDbId.ToString() + "-1.png"))
                    //{
                    //    ViewBag.Img1 = @"Img-" + WebDbId.ToString() + "-1.png";
                    //}
                    //if (System.IO.File.Exists(HeaderImagesFolder + @"\Img-" + WebDbId.ToString() + "-2.jpg"))
                    //{
                    //    ViewBag.Img2 = @"Img-" + WebDbId.ToString() + "-2.jpg";
                    //}
                    //else if (System.IO.File.Exists(HeaderImagesFolder + @"\Img-" + WebDbId.ToString() + "-2.png"))
                    //{
                    //    ViewBag.Img2 = @"Img-" + WebDbId.ToString() + "-2.png";
                    //}
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
        public IActionResult YearHistogramJSON()
        {
            try
            {
                if (SetCSLAUser())
                {
                    WebDbYearFrequencyCrit crit = new WebDbYearFrequencyCrit(WebDbId, true);
                    WebDbYearFrequencyList res = DataPortal.Fetch<WebDbYearFrequencyList>(crit);
                    return Json(res);
                }
                else return Unauthorized();
            } 
            catch (Exception e)
            {
                _logger.LogError(e, "Error in YearHistogramJSON");
                return StatusCode(500, e.Message);
            }
        }
    }
}