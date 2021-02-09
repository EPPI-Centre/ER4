using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using EPPIDataServices.Helpers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WebDatabasesMVC;

namespace WebDatabasesMVC.Controllers
{
    //[Route("Login")]
    //[Route("Login/Login")]
    public class LoginController : Controller
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

        private readonly ILogger _logger;

        public LoginController(ILogger<LoginController> logger, IWebHostEnvironment hostEnvironment)
        {
            _logger = logger;
            webHostEnvironment = hostEnvironment;
        }


        public IActionResult Index()
        {
            return View();
        }
        
        public IActionResult Logout()
        {
            HttpContext.SignOutAsync();
            return Redirect("~/Login/Index");
        }
        // POST: Login/Create
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public IActionResult DoLogin([FromForm] string username, [FromForm] string id, [FromForm] string password)
        {
            try
            {
                if (int.TryParse(id, out int WebDbId))
                {
                    string SP = "st_WebDBgetOpenAccess";
                    List<SqlParameter> pars = new List<SqlParameter>();
                    pars.Add(new SqlParameter("@WebDBid", WebDbId));
                    if (username != null && username != "" && password != null &&  password != "")
                    {
                        SP = "st_WebDBgetClosedAccess";
                        pars.Add(new SqlParameter("@userName", username));
                        pars.Add(new SqlParameter("@Password", password));
                    }

                    using (SqlDataReader reader = Program.SqlHelper.ExecuteQuerySP(Program.SqlHelper.ER4DB, SP, pars.ToArray())) 
                    {
                        
                        if (reader.Read()) {
                            if (int.TryParse(reader["REVIEW_ID"].ToString(), out int Revid))
                            {
                                long AttId = -1;
                                if (!reader.IsDBNull("WITH_ATTRIBUTE_ID")) AttId = reader.GetInt64("WITH_ATTRIBUTE_ID");
                                SetUser(reader["WEBDB_NAME"].ToString(), WebDbId, Revid, AttId);
                                SetImages(WebDbId, reader);
                                return Redirect("~/Review/Index");
                            } 
                            else
                            {
                                //return BadRequest();
                                return Redirect("~/Login/Logout");
                            }
                        }
                        else
                        {
                            //return BadRequest("test");
                            return Redirect("~/Login/Logout");
                        }
                        
                    }
                }
                //else return BadRequest();
                else return Redirect("~/Login/Logout");

            }
            catch (Exception e)
            {

                _logger.LogError(e, "logging on");
                //Program.Logger.LogException(e, "logging on");
                return Redirect("~/Login/Index");
            }
        }
        [HttpGet]
        //[ValidateAntiForgeryToken]
        public IActionResult Open([FromQuery] string WebDBid)
        {
            try
            {
                if (int.TryParse(WebDBid, out int WebDbId))
                {
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
                                SetUser(reader["WEBDB_NAME"].ToString(), WebDbId, Revid, AttId);
                                SetImages(WebDbId, reader);
                                return Redirect("~/Review/Index");
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
                else return BadRequest();

            }
            catch (Exception e)
            {

                _logger.LogError(e, "logging on");
                //Program.Logger.LogException(e, "logging on");
                return Redirect("~/Login/Index");
            }
        }

        ActionResult DoFail()
        {
            return Forbid();
        }
        private void SetUser(string Name, int WebDbID, int revId, long itemsCode)
        {
            var userClaims = new List<Claim>()
            {
                new Claim(ClaimTypes.Role, "WebDbReader"),
                new Claim(ClaimTypes.Name, Name),
                new Claim("reviewId", revId.ToString()),
                new Claim("WebDbID", WebDbID.ToString()),
                //new Claim("ItemsCode", itemsCode.ToString()) //we don't need to store this in the Cookie: the SPs for WebDBs should retrieve it from the DB
            };
            var innerIdentity = new ClaimsIdentity(userClaims, "User Identity");
            var userPrincipal = new ClaimsPrincipal(new[] { innerIdentity });
            HttpContext.SignInAsync(userPrincipal);
        }
        private void SetImages(int WebDbID, SqlDataReader reader)
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
        }
    }
}