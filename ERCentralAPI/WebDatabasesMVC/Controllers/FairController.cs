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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WebDatabasesMVC;
using WebDatabasesMVC.ViewModels;

namespace WebDatabasesMVC.Controllers
{
    [Authorize(AuthenticationSchemes = "FairAuthentication") ]
    public class FairController : CSLAController
    {

        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly IConfiguration Configuration;

        public FairController(ILogger<FairController> logger, IWebHostEnvironment hostEnvironment, IConfiguration configuration) : base(logger)
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
                int WebDbId = Configuration.GetValue<int>("AppSettings:FAIRprojectId");
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
                                    return Redirect("~/Fair");
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

        
        public IActionResult Topic([FromQuery] long? TopicId)
        {
            try
            {
                if (SetCSLAUser())
                {
                    if (TopicId == null || TopicId < 1) return BadRequest();
                    int DBid = WebDbId;
                    if (DBid < 1)
                    {
                        _logger.LogError("Error in LoggedOn, no WebDbId!");
                        return BadRequest();
                    }
                    WebDbReviewSetsList reviewSets = null;
                    reviewSets = DataPortal.Fetch<WebDbReviewSetsList>(new SingleCriteria<WebDbReviewSetsList, int>(DBid));
                    AttributeSet aSet = reviewSets.GetAttributeSetFromAttributeId((long)TopicId);
                    WebDbReviewSet ProgressSet = null;
                    foreach(WebDbReviewSet rs in reviewSets)
                    {
                        if (rs.OriginalSetId == 180878)
                        {
                            ProgressSet = rs;
                            break;
                        }
                    }
                    string imgBaseUrl = Configuration.GetValue<string>("AppSettings:FAIRImagesRoot");
                    if (reviewSets != null && reviewSets.Count > 0 && aSet != null && ProgressSet != null && imgBaseUrl != null && imgBaseUrl != "")
                    {
                        FairTopicVM res = new FairTopicVM(reviewSets, ProgressSet.SetId, aSet, imgBaseUrl);
                        return View(res);    
                    }
                    else return BadRequest();
                }
                else return Unauthorized();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error in GetFrequencies");
                return StatusCode(500, e.Message);
            }
        }

        [HttpPost]
        public IActionResult ListFromCrit(SelCritMVC critMVC)
        {
            try
            {
                if (SetCSLAUser())
                {
                    SelectionCriteria crit = critMVC.CSLACriteria();
                    ItemListWithCriteria iList = GetItemList(crit);
                    return View(iList);//supplying the view name, otherwise MVC would try to auto-discover a view called Page.
                }
                else return Unauthorized();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error in ItemList ListFromCrit");
                return StatusCode(500, e.Message);
            }
        }


        [HttpPost]
        public IActionResult ItemDetails(ItemSelCritMVC crit)
        {
            try
            {
                if (SetCSLAUser())
                {
                    FullItemDetails Itm = GetItemDetails(crit);

                    // log to TB_WEBDB_LOG
                    logActivity("ItemDetailsFromList", crit.itemID.ToString());
                    ViewBag.isFair = true;
                    return View("Views/ItemList/ItemDetails.cshtml", Itm);
                }
                else return Unauthorized();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error in ItemDetails");
                return StatusCode(500, e.Message);
            }
        }
        public IActionResult GetListSearchResults([FromForm] string SearchString, string SearchWhat, string included)
        {
            try
            {
                if (SetCSLAUser())
                {
                    string errorMsg;
                    SelectionCriteria criteria = ItemListController.GetCriterionForSearch(SearchString, SearchWhat, included, out errorMsg);
                    if (errorMsg != "")
                    {
                        _logger.LogError("Error in ItemList (FAIR) GetListSearchResults: search parameters appear to be malformed.");
                        return BadRequest("Request parameters appear to be malformed.");
                    }
                    ItemListWithCriteria iList = GetItemList(criteria);
                    
                    return View("ListFromCrit", iList);//supplying the view name, otherwise MVC would try to auto-discover a view called Page.
                }
                else return Unauthorized();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error in ItemList GetListSearchResults");
                return StatusCode(500, e.Message);
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
            var innerIdentity = new ClaimsIdentity(userClaims, "FairAuthentication");
            var userPrincipal = new ClaimsPrincipal(new[] { innerIdentity });
            SetImages(WebDbID, reader, innerIdentity);
            await HttpContext.SignInAsync("FairAuthentication", userPrincipal);
        }
        private async void Signout()
        {
            await HttpContext.SignOutAsync("FairAuthentication");
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
