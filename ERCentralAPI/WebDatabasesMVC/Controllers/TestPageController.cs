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
using WebDatabasesMVC.ViewModels;

namespace WebDatabasesMVC.Controllers
{
    public class TestPageController : CSLAController
    {
        // GET: TestPageController




        private readonly IWebHostEnvironment webHostEnvironment;

        public TestPageController(ILogger<ReviewController> logger, IWebHostEnvironment hostEnvironment) : base(logger)
        {
            webHostEnvironment = hostEnvironment;
        }


        public IActionResult Index()
        {
            try
            {
                if (SetCSLAUser())
                {
                    if (WebDbId < 1) return BadRequest();
                    WebDbWithRevInfo res = ReviewIndexDataGet(false);

                    if (res.WebDb == null || res.RevInfo == null || res.WebDb.WebDBId < 1 || res.RevInfo.ReviewId < 1) return BadRequest();

                    return View(res);
                }
                else return Unauthorized();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error in Review Index");
                return StatusCode(500, e.Message);
            }
        }
        public IActionResult IndexJSON()
        {
            try
            {
                if (SetCSLAUser())
                {
                    if (WebDbId < 1) return BadRequest();
                    WebDbWithRevInfo res = ReviewIndexDataGet(true);

                    if (res.WebDb == null || res.RevInfo == null || res.WebDb.WebDBId < 1 || res.RevInfo.ReviewId < 1) return BadRequest();

                    return View(res);
                }
                else return Unauthorized();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error in Review IndexJSON");
                return StatusCode(500, e.Message);
            }
        }
        private WebDbWithRevInfo ReviewIndexDataGet(bool IsJson)
        {
            WebDbWithRevInfo res = new WebDbWithRevInfo();
            WebDB me = DataPortal.Fetch<WebDB>(new SingleCriteria<int>(WebDbId));
            if (me == null || me.WebDBId != WebDbId) return res;
            if (!IsJson)
            {
                ReviewSetsList rssl = DataPortal.Fetch<ReviewSetsList>();
                AttributeSet aSet = rssl.GetAttributeSetFromAttributeId(me.AttributeIdFilter);
                if (aSet != null)
                {
                    ViewBag.AttName = aSet.AttributeName;
                }
                else ViewBag.AttName = "Unknown";
            }
            ReviewInfo rinfo = DataPortal.Fetch<ReviewInfo>();
            res = new WebDbWithRevInfo() { WebDb = me, RevInfo = rinfo };
            return res;
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



        // GET: TestPageController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: TestPageController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: TestPageController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: TestPageController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: TestPageController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: TestPageController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: TestPageController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
