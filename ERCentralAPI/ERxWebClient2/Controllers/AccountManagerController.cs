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
                    ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;

                    //safety check: one can only edit their own account!
                    //No need to check for reviewId as well (to make sure user login status is currently valid) as this happens in SetCSLAUser4Writing()
                    if (data.contactId != ri.UserId) return Forbid();
                    DataPortal<Contact> dp = new DataPortal<Contact>();
                    Contact res = dp.Fetch(new SingleCriteria<Contact, int>(data.contactId));
                    //res.ContactId = data.contactId; //contact ID is fixed, no point in allowing it to change

                    res.contactName = data.ContactName;
                    res.Username = data.username;
                    res.Email = data.email;
                    if (data.OldPassword.Trim().Length > 1 && data.NewPassword.Trim().Length > 7)
                    {//we only attempt to change password IF input data seems sensible
                        res.OldPassword = data.OldPassword;
                        res.NewPassword = data.NewPassword;
                    }
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
        

        [HttpPost("[action]")]
        public IActionResult UpdateReviewName([FromBody] SingleStringCriteria data)
        {
            try
            {
                if (SetCSLAUser4Writing())
                {
                    Review res = new Review(data.Value);
                    res = res.Save(); 
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


        [HttpPost("[action]")]
        public IActionResult UpdateReviewerRole([FromBody] JSONAccount data)
        {
            try
            {
                if (SetCSLAUser4Writing() && UserIsAdmin())
                {
                    ReviewMembers res = new ReviewMembers(data.role, data.contactId);
                    res = res.Save();
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



        [HttpPost("[action]")]
        public IActionResult AddReviewer([FromBody] SingleStringCriteria data)
        {
            try
            {
                if (SetCSLAUser4Writing() && UserIsAdmin())
                {
                    ReviewMembers res = new ReviewMembers(data.Value);
                    res = res.Save();
                    return Ok(res.ResultValue);
                }
                else return Forbid();
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Contact data portal error");
                return StatusCode(500, e.Message);
            }
        }


        [HttpPost("[action]")]
        public IActionResult RemoveReviewer([FromBody] SingleStringCriteria data)
        {
            try
            {
                if (SetCSLAUser4Writing() && UserIsAdmin())
                {
                    //ReviewMembers res = new ReviewMembers(data.Value);
                    ReviewMembers res = DataPortal.Fetch<ReviewMembers>(new SingleCriteria<ReviewMembers, string>(data.Value));
                    res.Delete();
                    res = res.Save();
                    return Ok(res.Result);
                    //return Ok(0);
                }
                else return Forbid();
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Contact data portal error");
                return StatusCode(500, e.Message);
            }
        }

        private bool UserIsAdmin()
        {
            ReviewerIdentity ri = ReviewerIdentity.GetIdentity(User);
            return ri.Roles.Contains("AdminUser");
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

    public string role = "";
}


