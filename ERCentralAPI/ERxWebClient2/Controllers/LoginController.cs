using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using BusinessLibrary.BusinessClasses;
using BusinessLibrary.Security;
using Csla.Data;
using ERxWebClient2.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Configuration;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Csla.Security;
using System.Security.Principal;
using System.Security.Claims;

namespace ERxWebClient2.Controllers
{
    [Route("api/[controller]")]
    public class LoginController : Controller
    {
        private IConfiguration _config;
        public LoginController(IConfiguration conf)
        {
            _config = conf;
        }
        [HttpPost("[action]")]
        public ReviewerIdentityWebClient Login(string Username, string Password)
        {
            ReviewerIdentityWebClient ri = ReviewerIdentityWebClient.GetIdentity(Username, Password, 0, "web", "");

            //var userIdentity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);

            //Task<(bool, TokenResponse)> task = Task.Run(() => IdentityServer4Client.LoginAsync(username, password, userIdentity));
            //bool CorrectCredentials = task.Result.Item1;
            //if (!CorrectCredentials) return Redirect("~/Login"); //DoFail();
            //ClaimsPrincipal user = new ClaimsPrincipal(userIdentity);
            

            ri.Token = BuildToken(ri);
            return ri;
        }
        [HttpPost("[action]")]
        public ReviewerIdentityWebClient LoginToReview(string Username, string Password, int ReviewId)
        {
            ReviewerIdentityWebClient ri = ReviewerIdentityWebClient.GetIdentity(Username, Password, ReviewId, "web", "");
            ri.Token = BuildToken(ri);
            return ri;
        }
        private string BuildToken(ReviewerIdentityWebClient ri)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["AppSettings:EPPIApiClientSecret"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            IIdentity id = ri as IIdentity;
            ClaimsIdentity riCI = new ClaimsIdentity(id);
            IEnumerable<Claim> claims = riCI.Claims;
            riCI.AddClaim(new Claim("reviewId", ri.ReviewId.ToString()));
            riCI.AddClaim(new Claim("userId", ri.UserId.ToString()));
            riCI.AddClaim(new Claim("name", ri.Name));

            var token = new JwtSecurityToken(_config["AppSettings:EPPIApiUrl"],
              _config["AppSettings:EPPIApiClientName"],
              riCI.Claims,
              expires: DateTime.Now.AddMinutes(120),
              signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}
