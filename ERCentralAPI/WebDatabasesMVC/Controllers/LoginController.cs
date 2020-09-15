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
                    SqlParameter par = new SqlParameter("@WEBDB_ID", WebDbId);
                    using (SqlDataReader reader = Program.SqlHelper.ExecuteQuerySP(Program.SqlHelper.DataServiceDB, "st_WebDatabaseGet", par))
                    {
                        if (username == null || password == null || username == "" || password == "")
                        {
                            while (reader.Read())
                            {
                                if (reader["USERNAME"].Equals(DBNull.Value) && reader["PASSWD"].Equals(DBNull.Value)) {
                                    if (int.TryParse(reader["REVIEW_ID"].ToString(), out int Revid))
                                    {
                                        int AttId = -1;
                                        if (!reader.IsDBNull("ATTR_TO_INCLUDE")) AttId = reader.GetInt32("ATTR_TO_INCLUDE");
                                        SetUser(reader["WEBDB_NAME"].ToString(), WebDbId, Revid, AttId);
                                        return Redirect("~/Review/Index");
                                    } 
                                    else
                                    {
                                        return BadRequest();
                                    }
                                }
                            }
                            return BadRequest();
                        }
                        else
                        {
                            while (reader.Read())
                            {
                                if (reader["USERNAME"].Equals(username) && reader["PASSWD"].Equals(password))
                                {
                                    if (int.TryParse(reader["REVIEW_ID"].ToString(), out int Revid))
                                    {
                                        int AttId = -1;
                                        if (!reader.IsDBNull("ATTR_TO_INCLUDE")) AttId = reader.GetInt32("ATTR_TO_INCLUDE");
                                        SetUser(reader["WEBDB_NAME"].ToString(), WebDbId, Revid, AttId);
                                        return Redirect("~/Review/Index");
                                    }
                                    else
                                    {
                                        return BadRequest();
                                    }
                                }
                            }
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
        private void SetUser(string Name, long WebDbID, int revId, int itemsCode)
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