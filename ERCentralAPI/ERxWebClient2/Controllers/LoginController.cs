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
            ri.Token = BuildToken();
            return ri;
        }
        [HttpPost("[action]")]
        public ReviewerIdentityWebClient LoginToReview(string Username, string Password, int ReviewId)
        {
            ReviewerIdentityWebClient ri = ReviewerIdentityWebClient.GetIdentity(Username, Password, ReviewId, "web", "");
            ri.Token = BuildToken();
            return ri;
        }
        private string BuildToken()
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["AppSettings:EPPIApiClientSecret"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(_config["AppSettings:EPPIApiUrl"],
              _config["AppSettings:EPPIApiClientName"],
              expires: DateTime.Now.AddMinutes(120),
              signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}
