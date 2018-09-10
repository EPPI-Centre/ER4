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
    public class ReviewerTermListController : CSLAController
    {
        [HttpGet("[action]")]
        public IActionResult Fetch()
        {
            try
            {
                SetCSLAUser();
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                DataPortal<TrainingReviewerTermList> dp = new DataPortal<TrainingReviewerTermList>();
                TrainingReviewerTermList result = dp.Fetch();
                return Ok(result);
            }
            catch(Exception e)
            {
                //add logging
                return StatusCode(500, e.Message);
            }
        }
    }
}
