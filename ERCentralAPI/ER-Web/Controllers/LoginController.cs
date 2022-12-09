using System;
using System.Collections.Generic;
using System.Linq;
using BusinessLibrary.BusinessClasses;
using BusinessLibrary.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Security.Principal;
using Microsoft.AspNetCore.Authorization;
using Csla;
using Microsoft.Extensions.Logging;
using EPPIDataServices.Helpers;

namespace ERxWebClient2.Controllers
{
    [Route("api/[controller]")]
    public class LoginController : Microsoft.AspNetCore.Mvc.Controller
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
                if (lc.code.Length < 20 || lc.state.Length < 10) return Forbid();
                ReviewerIdentity ri = null;
                if (lc.reviewerIdentity != null 
                    && User.Identity.IsAuthenticated//this line ensures no tampering is possible, requires the encrypted token...
                    && lc.reviewerIdentity.UserId == 0
                    && User.IsInRole("CochraneUser")//ditto
                    )
                {//special case, get the RI via code and state, but looking into local DB.
                    ri = ReviewerIdentity.GetIdentity(lc.code, lc.state, "Archie0", 0);
                }
                else
                {
                    ri = ReviewerIdentity.GetIdentity(lc.code, lc.state, "Archie", 0);
                }
                if (ri != null && ri.IsAuthenticated)
                {
                    ri.Token = BuildToken(ri);
                    lc.reviewerIdentity = ri;
                    return Ok(lc);
                }
                else {
                    if (ri != null && ri.Ticket != "")
                    {
                        _logger.LogError("Archie authentication didn't work, ri.ticket is: " + ri.Ticket);
                    }
                    return Forbid();
                }
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
                    bool isArchie = User.IsInRole("CochraneUser");
                    ReviewerIdentity ri = ReviewerIdentity.GetIdentity(cID, RevIDCrit.Value, User.Identity.Name, isArchie);
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
        {//login to any review as super user
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


        [Authorize(Roles = "CochraneUser")]
        [HttpPost("[action]")]
        public IActionResult LinkToExistingAccount([FromBody] ArchieLoginCreds lc)
        {
            try
            {
                if (lc.loginCreds == null || lc.loginCreds.Username == "" || lc.loginCreds.Password == ""
                    || lc.state == "" || lc.code == ""
                    )
                {
                    return Forbid();
                }
                ReviewerIdentity ri = ReviewerIdentity.GetIdentity(lc.loginCreds.Username, lc.loginCreds.Password, lc.code, lc.state);
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

        [Authorize(Roles = "CochraneUser")]
        [HttpPost("[action]")]
        public IActionResult CreateER4ContactViaArchie([FromBody] CreateER4ContactViaArchieCommandJSON command)
        {
            try
            {
                CreateER4ContactViaArchieCommand cmd = command.CreateCSLAER4ContactViaArchieCommand();
                DataPortal<CreateER4ContactViaArchieCommand> dp = new DataPortal<CreateER4ContactViaArchieCommand>();
                cmd = dp.Execute(cmd);
                command.result = cmd.Result;
                return Ok(command);
            }
            catch (Exception e)
            {
                _logger.LogException(e, "CreateER4ContactViaArchie exception!");
                return StatusCode(500, e.Message);
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
            riCI.AddClaim(new Claim(Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
            
            foreach (var userRole in ri.Roles)
            {
                riCI.AddClaim(new Claim(ClaimTypes.Role, userRole));
            }
            var token = new JwtSecurityToken(_config["AppSettings:EPPIApiUrl"],
              _config["AppSettings:EPPIApiClientName"],
              riCI.Claims,
              notBefore: DateTime.Now.AddSeconds(-60),
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
                return StatusCode(500, e.Message);
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
        public LoginCreds loginCreds { get; set; }
        public ReviewerIdentity reviewerIdentity { get; set; }
    }
    public class LoginCredsSA
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public int RevId { get; set; }
    }
    public class CreateER4ContactViaArchieCommandJSON
    {
        public CreateER4ContactViaArchieCommand CreateCSLAER4ContactViaArchieCommand()
        {
            CreateER4ContactViaArchieCommand res = new CreateER4ContactViaArchieCommand(this.code,
                this.status, this.username, this.email, this.fullname, this.password, this.sendNewsletter, this.createExampleReview);
            return res;
        }
        public string code { get; set; }
        public string status { get; set; }
        public string username { get; set; }
        public string email { get; set; }
        public string fullname { get; set; }
        public string password { get; set; }
        public bool sendNewsletter { get; set; }
        public bool createExampleReview { get; set; }
        public string result { get; set; }
    }
}
