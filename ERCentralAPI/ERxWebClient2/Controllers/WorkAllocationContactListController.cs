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

namespace ERxWebClient2.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class WorkAllocationContactListController : CSLAController
    {
        [HttpGet("[action]")]
        public WorkAllocationContactList WorkAllocationContactList()//should receive a reviewID!
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
            return result;
        }

        
    }
}
