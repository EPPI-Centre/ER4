using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
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
using Newtonsoft.Json.Linq;
using EPPIDataServices.Helpers;
using Newtonsoft.Json;

using BusinessLibrary.BusinessClasses.ImportItems;
using Csla.Core;

namespace ERxWebClient2.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class AccountManagerController : CSLAController
    {
        
        public AccountManagerController(ILogger<ReviewController> logger) : base(logger)
        { }


        [HttpPost("[action]")]
        public IActionResult GetUserAccountDetails([FromBody] SingleIntCriteria ContactId)
        {
            try
            {
                if (!SetCSLAUser()) return Unauthorized();
                DataPortal<Contact> dp = new DataPortal<Contact>();
                Contact result = dp.Fetch(new SingleCriteria<Contact, int>(ContactId.Value));
                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogException(e, "GetContactDetails data portal error");
                return StatusCode(500, e.Message);
            }

        }

        
        [HttpPost("[action]")]
        public IActionResult UpdateAccount([FromBody] JSONAccount data)
        {
            try
            {
                if (SetCSLAUser4Writing())
                {
                    DataPortal<Contact> dp = new DataPortal<Contact>();
                    Contact res = dp.Fetch(new SingleCriteria<Contact, int>(data.contactId));
                    res.ContactId = data.contactId;
                    res.contactName = data.ContactName;
                    res.Username = data.username;
                    res.Email = data.email;
                    res.OldPassword = data.OldPassword;
                    res.NewPassword = data.NewPassword;    
                    
                    res = res.Save(); // asking object to save itself
                    return Ok(res.Result);
                }
                else return Forbid();
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Contact data portal error");
                return StatusCode(500, e.Message);
            }
        }
        

    }



    public class JSONAccount
    {
        public int contactId = 0;
        public string ContactName = "";
        public string username = "";
        public string email = "";
        public string OldPassword = "";
        public string NewPassword = "";
    }

}
