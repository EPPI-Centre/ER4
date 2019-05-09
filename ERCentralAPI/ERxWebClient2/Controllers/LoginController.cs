using System;
using System.Collections.Generic;
using System.Linq;
using BusinessLibrary.BusinessClasses;
using BusinessLibrary.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Security.Principal;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Csla;
using Microsoft.Extensions.Logging;
using EPPIDataServices.Helpers;

namespace ERxWebClient2.Controllers
{
    [Route("api/[controller]")]
    public class LoginController : Controller
    {
        private IConfiguration _config;
        private readonly ILogger _logger;

        public LoginController(IConfiguration conf, ILogger<LoginController> logger)
        {
            _logger = logger;
            _config = conf;
        }
        
        [HttpPost("[action]")]
        public IActionResult Login([FromBody] LoginCreds lc)
        {

            try
            {

                ReviewerIdentity ri = ReviewerIdentity.GetIdentity(lc.Username, lc.Password, 0, "web", "");
                if (ri.IsAuthenticated)
                {
                    ri.Token = BuildToken(ri);
                    return Ok(ri);
                }
                else { return Forbid(); }
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Logging in exception!");
                return Forbid();
            }
        }
        [HttpPost("[action]")]
        public IActionResult LoginFromArchie([FromBody] ArchieLoginCreds lc)
        {

            try
            {
                ReviewerIdentity ri = ReviewerIdentity.GetIdentity(lc.code, lc.state, "Archie", 0);
                if (ri.IsAuthenticated)
                {
                    ri.Token = BuildToken(ri);
                    lc.reviewerIdentity = ri;
                    return Ok(lc);
                }
                else { return Forbid(); }
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Logging in exception!");
                return Forbid();
            }
        }

        [Authorize]
        [HttpPost("[action]")]
        public IActionResult LoginToReview([FromBody] SingleIntCriteria RevIDCrit)
        {
            try
            { 
                var userId = User.Claims.First(c => c.Type == "userId").Value;
                int cID;
                bool canProceed = true;
                canProceed = int.TryParse(userId, out cID);
                if (canProceed)
                {
                    ReviewerIdentity ri = ReviewerIdentity.GetIdentity(cID, RevIDCrit.Value, User.Identity.Name);
                    int Rid = ri.ReviewId;
                    if (ri.Ticket == "")
                    {
                        return Unauthorized();
                    }
                    ri.Token = BuildToken(ri);
                    return Ok(ri);

                }
                else  
                {
                    return StatusCode(500, "login to review failed");
                }
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Error occured when logging into a review");
                return Forbid();
            }
        }

        [Authorize]
        [HttpPost("[action]")]
        public IActionResult LoginToReviewSA([FromBody] LoginCredsSA lcsa)
        {
            try
            {
                if (!(User.Claims.First(c => c.Type == "isSiteAdmin").Value == "True")) return Forbid();
                
                ReviewerIdentity ri = ReviewerIdentity.GetIdentity(lcsa.Username, lcsa.Password, -lcsa.RevId, "web", "");
                if (ri.UserId.ToString() != User.Claims.First(c => c.Type == "userId").Value) return Forbid();
                if (ri.IsAuthenticated)
                {
                    ri.Token = BuildToken(ri);
                    return Ok(ri);
                }
                else { return Forbid(); }
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Logging on exception!");
                return Forbid();
            }
        }


        private string BuildToken(ReviewerIdentity ri)
        {

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["AppSettings:EPPIApiClientSecret"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            IIdentity id = ri as IIdentity;
            ClaimsIdentity riCI = new ClaimsIdentity(id);
            IEnumerable<Claim> claims = riCI.Claims;
            riCI.AddClaim(new Claim("reviewId", ri.ReviewId.ToString()));
            riCI.AddClaim(new Claim("userId", ri.UserId.ToString()));
            riCI.AddClaim(new Claim("name", ri.Name));
            riCI.AddClaim(new Claim("reviewTicket", ri.Ticket));
            riCI.AddClaim(new Claim("isSiteAdmin", ri.IsSiteAdmin.ToString()));
            foreach (var userRole in ri.Roles)
            {
                riCI.AddClaim(new Claim(ClaimTypes.Role, userRole));
            }
            var token = new JwtSecurityToken(_config["AppSettings:EPPIApiUrl"],
              _config["AppSettings:EPPIApiClientName"],
              riCI.Claims,
              expires: DateTime.Now.AddHours(6),
              //expires: DateTime.Now.AddSeconds(15),
              signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [HttpGet("[action]")]
        public IActionResult VersionInfo()
        {
            try
            {

                DataPortal<GetLatestUpdateMsgCommand> dp = new DataPortal<GetLatestUpdateMsgCommand>();
                GetLatestUpdateMsgCommand command = new GetLatestUpdateMsgCommand();
                command =  dp.Execute(command);
                return Ok(command);

            }
            catch (Exception e)
            {
                _logger.LogException(e, "Error with the dataportal for version info in CSLA");
                throw;
            }
        }
        
    }
    public class LoginCreds
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
    public class ArchieLoginCreds
    {
        public string code { get; set; }
        public string state { get; set; }
        public string error { get; set; }
        public ReviewerIdentity reviewerIdentity { get; set; }
    }
    public class LoginCredsSA
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public int RevId { get; set; }
    }
}
