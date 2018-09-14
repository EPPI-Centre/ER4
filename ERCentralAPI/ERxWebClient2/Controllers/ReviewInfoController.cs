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
    public class ReviewInfoController : CSLAController
    {

        [HttpGet("[action]")]
        public IActionResult ReviewInfo()
        {
            SetCSLAUser();
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;

            DataPortal<ReviewInfo> dp = new DataPortal<ReviewInfo>();

            ReviewInfo result = dp.Fetch();

            return Ok(result);
        }

    }
}