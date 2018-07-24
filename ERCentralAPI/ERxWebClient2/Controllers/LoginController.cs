using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using BusinessLibrary.BusinessClasses;
using BusinessLibrary.Security;
using Csla.Data;
using ERxWebClient2.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace ERxWebClient2.Controllers
{
    [Route("api/[controller]")]
    public class LoginController : Controller
    {
        [HttpPost("[action]")]
        public ReviewerIdentityWebClient Login(string Username, string Password)
        {
            ReviewerIdentityWebClient ri = ReviewerIdentityWebClient.GetIdentity(Username, Password, 0, "web", "");
            return ri;
        }
        [HttpPost("[action]")]
        public ReviewerIdentityWebClient LoginToReview(string Username, string Password, int ReviewId)
        {
            ReviewerIdentityWebClient ri = ReviewerIdentityWebClient.GetIdentity(Username, Password, ReviewId, "web", "");
            return ri;
        }

    }
}
