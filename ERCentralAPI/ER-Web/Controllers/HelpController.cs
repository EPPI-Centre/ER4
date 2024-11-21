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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EPPIDataServices.Helpers;

namespace ERxWebClient2.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class HelpController : CSLAController
    {
        //OnlineHelpCriteria
        public HelpController(ILogger<ReviewController> logger) : base(logger)
        { }

        [HttpPost("[action]")]
		public IActionResult FetchHelpContent([FromBody] SingleStringCriteria crit)
		{
            
			try
			{
                if (!SetCSLAUser()) return Unauthorized();
                OnlineHelpContent res  = new OnlineHelpContent();
                DataPortal<OnlineHelpContent> dp = new DataPortal<OnlineHelpContent>();
                res = dp.Fetch(new OnlineHelpCriteria(crit.Value));
				return Ok(res);
			}
			catch (Exception e)
			{
				_logger.LogException(e, "FetchHelpContent data portal error");
				return StatusCode(500, e.Message);
			}
			
		}
        /*
        [HttpPost("[action]")]
        public IActionResult UpdateHelpcontent([FromBody] HelpContentJSON crit)
        {

            try
            {
                if (!SetCSLAUser()) return Unauthorized();
                HelpContentJSON res = FeedbackAndClientError.CreateFeedbackAndClientError(crit.context, crit.helpContent);
                res = res.Save();
                return Ok(res);
            }
            catch (Exception e)
            {
                _logger.LogException(e, "CreateFeedbackMessage data portal error");
                return StatusCode(500, e.Message);
            }

        }
        */
        [HttpPost("[action]")]
        public IActionResult CreateFeedbackMessage([FromBody] FeedbackAndClientErrorJSON crit)
        {
            
            try
            {
                if (!SetCSLAUser()) return Unauthorized();
                FeedbackAndClientError res = FeedbackAndClientError.CreateFeedbackAndClientError(crit.contactId, crit.context, crit.isError, crit.message);
                res = res.Save();
                return Ok(res);
            }
            catch (Exception e)
            {
                _logger.LogException(e, "CreateFeedbackMessage data portal error");
                return StatusCode(500, e.Message);
            }

        }
        [HttpGet("[action]")]
        public IActionResult FeedbackMessageList()
        {
            
            try
            {
                if (!SetCSLAUser()) return Unauthorized();
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                if (ri.ReviewId == 0 || !ri.IsSiteAdmin) return Forbid();
                DataPortal<FeedbackAndClientErrorList> dp = new DataPortal<FeedbackAndClientErrorList>();
                FeedbackAndClientErrorList res = dp.Fetch();
                return Ok(res);
            }
            catch (Exception e)
            {
                _logger.LogException(e, "FeedbackMessageList data portal error");
                return StatusCode(500, e.Message);
            }
        }
    }
    public class FeedbackAndClientErrorJSON
    {
        public int contactId;
        public string context;
        public bool isError;
        public string message;
    }

    /*
    public class HelpContentJSON
    {
        public string context;
        public string helpContent;
    }
    */
}
