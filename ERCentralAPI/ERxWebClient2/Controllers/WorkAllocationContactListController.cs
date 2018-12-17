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
using Newtonsoft.Json.Linq;
using EPPIDataServices.Helpers;
using Microsoft.Extensions.Logging;

namespace ERxWebClient2.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class WorkAllocationContactListController : CSLAController
    {
        private readonly ILogger _logger;

        public WorkAllocationContactListController(ILogger<WorkAllocationContactListController> logger)
        {
            _logger = logger;
        }

        [HttpGet("[action]")]
        public IActionResult WorkAllocationContactList()//should receive a reviewID!
        {
            try
            {
                SetCSLAUser();
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                DataPortal<WorkAllocationContactList> dp = new DataPortal<WorkAllocationContactList>();
                WorkAllocationContactList result = dp.Fetch();

                //Newtonsoft.Json.JsonSerializerSettings ss = new Newtonsoft.Json.JsonSerializerSettings();
                //string resSt = Newtonsoft.Json.JsonConvert.SerializeObject(result, Newtonsoft.Json.Formatting.None, new Newtonsoft.Json.JsonSerializerSettings
                //{
                //    ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
                //});
                //return resSt;
                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Work Allocation data portal error");
                throw;
            }

        }

        
    }
}
