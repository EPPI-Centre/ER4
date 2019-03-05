using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using BusinessLibrary.BusinessClasses;
using BusinessLibrary.Security;
using Csla;
using Csla.Data;
using ERxWebClient2.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ERxWebClient2.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class LogonTicketCheckController : Controller
    {

        private readonly ILogger _logger;

        public LogonTicketCheckController(ILogger<LogonTicketCheckController> logger)
        {

            _logger = logger;
        }

        [HttpPost("[action]")]
        public IActionResult ExcecuteCheckTicketExpirationCommand([FromBody] LoginTicketCheck Lgt)
        {

            try
            {

                SetCSLAUserNoReviewCheck();//we don't use the Abstract class method because in this case only we don't want to check the ReviewID!
                CheckTicketExpirationCommand cmd = new CheckTicketExpirationCommand(

                    Lgt.userId,
                    Lgt.GUID

                );
                DataPortal<CheckTicketExpirationCommand> dp = new DataPortal<CheckTicketExpirationCommand>();
                cmd = dp.Execute(cmd);

                Lgt.Result = cmd.Result;
                Lgt.ServerMessage = cmd.ServerMessage;
                if (Lgt.ServerMessage.Contains(@"\n")) Lgt.ServerMessage = Lgt.ServerMessage.Replace(@"\n", @"<br />");

                //Lgt.Result = "Expired";
                //Lgt.ServerMessage = "...";
                return Ok(Lgt);
                //return Forbid();
                //return Unauthorized();
                //return BadRequest();

            }
            catch (Exception e)
            {
                string json = JsonConvert.SerializeObject(Lgt);
                _logger.LogError(e, "Dataportal Error with Check ticket for user: {0}", json);
                throw;
            }
        }
        protected void SetCSLAUserNoReviewCheck()
        {
            try
            {
                ReviewerIdentity ri = ReviewerIdentity.GetIdentity(User);
                ReviewerPrincipal principal = new ReviewerPrincipal(ri);
                Csla.ApplicationContext.User = principal;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Dataportal Check ticket Error as SetCSLA user stage");
                throw e;
            }
        }
    }

    public class LoginTicketCheck
    {
        public int userId { get; set; }
        public string GUID { get; set; }
        public string Result { get; set; }
        public string ServerMessage { get; set; }
    }

}
