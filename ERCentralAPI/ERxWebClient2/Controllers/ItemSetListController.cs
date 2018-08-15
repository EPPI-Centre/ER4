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
    public class ItemSetListController : CSLAController
    {
        [HttpPost("[action]")]
        public IActionResult Fetch([FromBody] SingleInt64Criteria ItemIDCrit)
        {
            try
            {
                SetCSLAUser();
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;

                DataPortal<ItemSetList> dp = new DataPortal<ItemSetList>();
                SingleCriteria<ItemSetList, Int64> criteria = new SingleCriteria<ItemSetList, Int64>(ItemIDCrit.Value);
                ItemSetList result = dp.Fetch(criteria);
                //ItemSetList result = dp.Fetch(ItemIDCrit.Value);
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
