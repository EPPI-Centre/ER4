﻿using System;
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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WebDatabasesMVC;
using WebDatabasesMVC.ViewModels;

namespace WebDatabasesMVC.Controllers
{
    [Authorize(AuthenticationSchemes = "VawgAuthentication") ]
    public class VawgController : CSLAController
    {

        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly IConfiguration Configuration;

        public VawgController(ILogger<FairController> logger, IWebHostEnvironment hostEnvironment, IConfiguration configuration) : base(logger)
        {
            webHostEnvironment = hostEnvironment;
            Configuration = configuration;
        }
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

        public IActionResult Index()
        {
            try
            {
                if (SetCSLAUser())
                {
                    if (WebDbId < 1) return BadRequest();
                    WebDbWithRevInfo res = ReviewIndexDataGet();

                    if (res.WebDb == null || res.RevInfo == null || res.WebDb.WebDBId < 1 || res.RevInfo.ReviewId < 1) return BadRequest();

                    return View(res);
                }
                else return Unauthorized();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error in FAIR Index");
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
                    WebDbWithRevInfo res = ReviewIndexDataGet();

                    if (res.WebDb == null || res.RevInfo == null || res.WebDb.WebDBId < 1 || res.RevInfo.ReviewId < 1) return BadRequest();

                    return Json(res);
                }
                else return Unauthorized();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error in FAIR Index");
                return StatusCode(500, e.Message);
            }
        }
        private WebDbWithRevInfo ReviewIndexDataGet()
        {
            WebDbWithRevInfo res = new WebDbWithRevInfo();
            WebDB me = DataPortal.Fetch<WebDB>(new SingleCriteria<int>(WebDbId));
            if (me == null || me.WebDBId != WebDbId) return res;
            ReviewInfo rinfo = DataPortal.Fetch<ReviewInfo>();
            res = new WebDbWithRevInfo() { WebDb = me, RevInfo = rinfo };
            return res;
        }

        [AllowAnonymous]
        public IActionResult Login([FromQuery] string? ReturnUrl)
        {
            try
            {
                int WebDbId = Configuration.GetValue<int>("AppSettings:VawgProjectId");
                {
                    Signout();
                    string SP = "st_WebDBgetOpenAccess";
                    List<SqlParameter> pars = new List<SqlParameter>();
                    pars.Add(new SqlParameter("@WebDBid", WebDbId));
                    using (SqlDataReader reader = Program.SqlHelper.ExecuteQuerySP(Program.SqlHelper.ER4DB, SP, pars.ToArray()))
                    {
                        if (reader.Read())
                        {
                            if (int.TryParse(reader["REVIEW_ID"].ToString(), out int Revid))
                            {
                                long AttId = -1;
                                if (!reader.IsDBNull("WITH_ATTRIBUTE_ID")) AttId = reader.GetInt64("WITH_ATTRIBUTE_ID");
                                SetUser(reader["WEBDB_NAME"].ToString(), WebDbId, Revid, AttId, reader);
                                //SetImages(WebDbId, reader);

                                // log to TB_WEBDB_LOG
                                ERxWebClient2.Controllers.CSLAController.logActivityStatic("Login", "Open access", WebDbId, Revid);
                                if (ReturnUrl != null && ReturnUrl != "" && !ReturnUrl.Contains("ListFromCrit"))
                                {
                                    return Redirect(ReturnUrl);
                                }
                                else
                                {
                                    return Redirect("~/Vawg");
                                }
                            }
                            else
                            {
                                return BadRequest();
                            }
                        }
                        else
                        {
                            return BadRequest();
                        }

                    }
                }
            }
            catch (Exception e)
            {

                _logger.LogError(e, "logging on FAIR");
                //Program.Logger.LogException(e, "logging on");
                return Redirect("~/Fair");
            }
        }

        
        private async void SetUser(string Name, int WebDbID, int revId, long itemsCode, SqlDataReader reader)
        {
            var userClaims = new List<Claim>()
            {
                new Claim(ClaimTypes.Role, "WebDbReader"),
                new Claim(ClaimTypes.Name, Name),
                new Claim("reviewId", revId.ToString()),
                new Claim("WebDbID", WebDbID.ToString()),
                //new Claim("ItemsCode", itemsCode.ToString()) //we don't need to store this in the Cookie: the SPs for WebDBs should retrieve it from the DB
            };
            var innerIdentity = new ClaimsIdentity(userClaims, "VawgAuthentication");
            var userPrincipal = new ClaimsPrincipal(new[] { innerIdentity });
            SetImages(WebDbID, reader, innerIdentity);
            await HttpContext.SignInAsync("VawgAuthentication", userPrincipal);
        }
        private async void Signout()
        {
            await HttpContext.SignOutAsync("FairAuthentication");
            await HttpContext.SignOutAsync("CookieAuthentication");
            await HttpContext.SignOutAsync("VawgAuthentication");
            await HttpContext.SignOutAsync();
        }
        private void SetImages(int WebDbID, SqlDataReader reader, ClaimsIdentity innerIdentity)
        {
            bool todo = false;
            if (reader["HEADER_IMAGE_1"] != DBNull.Value)
            {
                ViewBag.Image1 = true;
                string filename = HeaderImagesFolder + @"\Img-" + WebDbID.ToString() + "-1." + reader.GetString("HEADER_IMAGE_EXT_1");
                if (System.IO.File.Exists(filename))
                {
                    System.IO.FileInfo fl = new FileInfo(filename);
                    if (fl.CreationTime < DateTime.Now.AddDays(-1)) todo = true;
                }
                else todo = true;
                if (todo)
                {
                    using (var stream = new FileStream(filename, FileMode.OpenOrCreate))
                    {
                        byte[] image = (byte[])reader["HEADER_IMAGE_1"];
                        stream.Write(image, 0, image.Length);

                    }
                }
            }
            else
            {
                string filename = HeaderImagesFolder + @"\Img-" + WebDbID.ToString() + "-1.jpg";
                try
                {
                    if (System.IO.File.Exists(filename))
                    {
                        System.IO.File.Delete(filename);
                    }
                    filename = HeaderImagesFolder + @"\Img-" + WebDbID.ToString() + "-1.png";
                    if (System.IO.File.Exists(filename))
                    {
                        System.IO.File.Delete(filename);
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "could not delete file: " + filename);
                }
            }
            if (reader["HEADER_IMAGE_2"] != DBNull.Value)
            {
                ViewBag.Image2 = true;
                string filename = HeaderImagesFolder + @"\Img-" + WebDbID.ToString() + "-2." + reader.GetString("HEADER_IMAGE_EXT_2");
                if (System.IO.File.Exists(filename))
                {
                    System.IO.FileInfo fl = new FileInfo(filename);
                    if (fl.CreationTime < DateTime.Now.AddDays(-1)) todo = true;
                }
                else todo = true;
                if (todo)
                {
                    using (var stream = new FileStream(filename, FileMode.OpenOrCreate))
                    {
                        byte[] image = (byte[])reader["HEADER_IMAGE_2"];
                        stream.Write(image, 0, image.Length);
                    }
                }
            }
            else
            {
                string filename = HeaderImagesFolder + @"\Img-" + WebDbID.ToString() + "-2.jpg";
                try
                {
                    if (System.IO.File.Exists(filename))
                    {
                        System.IO.File.Delete(filename);
                    }
                    filename = HeaderImagesFolder + @"\Img-" + WebDbID.ToString() + "-2.png";
                    if (System.IO.File.Exists(filename))
                    {
                        System.IO.File.Delete(filename);
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "could not delete file: " + filename);
                }
            }

            if (reader["HEADER_IMAGE_3"] != DBNull.Value)
            {
                ViewBag.Image3 = true;
                string filename = HeaderImagesFolder + @"\Img-" + WebDbID.ToString() + "-3." + reader.GetString("HEADER_IMAGE_EXT_3");
                if (System.IO.File.Exists(filename))
                {
                    System.IO.FileInfo fl = new FileInfo(filename);
                    if (fl.CreationTime < DateTime.Now.AddDays(-1)) todo = true;
                }
                else todo = true;
                if (todo)
                {
                    using (var stream = new FileStream(filename, FileMode.OpenOrCreate))
                    {
                        byte[] image = (byte[])reader["HEADER_IMAGE_3"];
                        stream.Write(image, 0, image.Length);
                    }
                }
                if (reader["HEADER_IMAGE_3_URL"] != DBNull.Value)
                {
                    string url = reader.GetString("HEADER_IMAGE_3_URL");
                    if (url != "" && (url.ToLower().StartsWith("http://") || url.ToLower().StartsWith("https://")))
                    {
                        Claim CL = new Claim("LogoURL", url);
                        innerIdentity.AddClaim(CL);
                    }
                }
            }
            else
            {
                string filename = HeaderImagesFolder + @"\Img-" + WebDbID.ToString() + "-3.jpg";
                try
                {
                    if (System.IO.File.Exists(filename))
                    {
                        System.IO.File.Delete(filename);
                    }
                    filename = HeaderImagesFolder + @"\Img-" + WebDbID.ToString() + "-3.png";
                    if (System.IO.File.Exists(filename))
                    {
                        System.IO.File.Delete(filename);
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "could not delete file: " + filename);
                }
            }
        }
    }
}
