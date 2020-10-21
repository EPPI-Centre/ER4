using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using EPPIDataServices.Helpers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
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
        // GET: Login

        private readonly ILogger _logger;

        public LoginController(ILogger<LoginController> logger)
        {
            _logger = logger;
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
                if (long.TryParse(id, out long WebDbId))
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
        [HttpGet]
        //[ValidateAntiForgeryToken]
        public IActionResult Open([FromQuery] string WebDBid)
        {
            try
            {
                if (long.TryParse(WebDBid, out long WebDbId))
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
        private void SetUser(string Name, long WebDbID, int revId, long itemsCode)
        {
            var userClaims = new List<Claim>()
            {
                new Claim(ClaimTypes.Role, "WebDbReader"),
                new Claim(ClaimTypes.Name, Name),
                new Claim("reviewId", revId.ToString()),
                new Claim("WebDbID", WebDbID.ToString()),
                new Claim("ItemsCode", itemsCode.ToString())
            };
            var innerIdentity = new ClaimsIdentity(userClaims, "User Identity");
            var userPrincipal = new ClaimsPrincipal(new[] { innerIdentity });
            HttpContext.SignInAsync(userPrincipal);
        }
    }
}