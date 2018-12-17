using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using EPPIDataServices.Helpers;
using IdentityModel.Client;
using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Klasifiki.Controllers
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
        public IActionResult DoLogin([FromForm] string username, [FromForm] string password)
        {
            try
            {
                var userIdentity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
                
                Task<(bool, TokenResponse)> task = Task.Run(() => IdentityServer4Client.LoginAsync(username, password, userIdentity));
                bool CorrectCredentials = task.Result.Item1;
                if (!CorrectCredentials) return Redirect("~/Login"); //DoFail();
                ClaimsPrincipal user = new ClaimsPrincipal(userIdentity);
                HttpContext.SignInAsync(user);

                _logger.LogInformation("Logged on: username");

                return Redirect("~/Home");


                //var globalId = (from c in userIdentity.Claims
                //                where c.Type == ClaimTypes.PrimarySid
                //                select c.Value).FirstOrDefault();

                //AuthenticationProperties props = null;
                
                //props = new AuthenticationProperties
                //{
                //    IsPersistent = true,
                //    ExpiresUtc = DateTimeOffset.UtcNow.Add(TimeSpan.FromHours(3))
                //};
                //AuthenticationProperties aprops = new AuthenticationProperties();
                

                //var tokens = new[] { new AuthenticationToken { Name = "id_token", Value = task.Result.Item2.IdentityToken },
                //                     new AuthenticationToken { Name = "access_token", Value = task.Result.Item2.AccessToken }
                //                    };
                //props.StoreTokens(tokens);
                
                
                //var s1 = HttpContext.AuthenticateAsync();
                //s1.Wait();
                //var s11 = s1.Result;
                //HttpContext.SignInAsync(user);
                //var t = user.Identity.AuthenticationType;
                //var s3 = HttpContext.ChallengeAsync();
                //s3.Wait(); ;
                //// "Cookies");
                //var s2 = s1.Result;
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
        // GET: Login/Edit/5
        //public ActionResult Edit(int id)
        //{
        //    return View();
        //}

        //// POST: Login/Edit/5
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Edit(int id, IFormCollection collection)
        //{
        //    try
        //    {
        //        // TODO: Add update logic here

        //        return RedirectToAction(nameof(Login));
        //    }
        //    catch
        //    {
        //        return View();
        //    }
        //}

        //// GET: Login/Delete/5
        //public ActionResult Delete(int id)
        //{
        //    return View();
        //}

        //// POST: Login/Delete/5
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Delete(int id, IFormCollection collection)
        //{
        //    try
        //    {
        //        // TODO: Add delete logic here

        //        return RedirectToAction(nameof(Login));
        //    }
        //    catch
        //    {
        //        return View();
        //    }
        //}
    }
}