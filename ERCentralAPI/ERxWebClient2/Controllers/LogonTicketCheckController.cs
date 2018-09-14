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
using Newtonsoft.Json.Linq;

namespace ERxWebClient2.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class LogonTicketCheckController : CSLAController
    {
        
        [HttpPost("[action]")]
        public IActionResult ExcecuteCheckTicketExpirationCommand([FromBody] LoginTicketCheck Lgt)
        {
            SetCSLAUser();
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
    }

    public class LoginTicketCheck
    {
        public int userId { get; set; }
        public string GUID { get; set; }
        public string Result { get; set; }
        public string ServerMessage { get; set; }
    }

}
