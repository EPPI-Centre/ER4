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

namespace ERxWebClient2.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class ReviewController : CSLAController
    {

        private readonly ILogger _logger;

        public ReviewController(ILogger<ReviewController> logger)
        {

            _logger = logger;
        }

        [HttpGet("[action]")]
        public IActionResult ReadOnlyReviews()//should receive a reviewID!
        {

            try
            {
                SetCSLAUser();
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;

                DataPortal<ReadOnlyReviewList> dp = new DataPortal<ReadOnlyReviewList>();
                SingleCriteria<ReadOnlyReviewList, int> criteria = new SingleCriteria<ReadOnlyReviewList, int>(ri.UserId);
                ReadOnlyReviewList result = dp.Fetch(criteria);

                //ReadOnlyReviewList returnValue = new ReadOnlyReviewList();
                //Action<ReviewerIdentity, ReadOnlyReviewList> Action = new Action<ReviewerIdentity, ReadOnlyReviewList>(Doit);
                //Action.Invoke(ri, returnValue);

                //return returnValue;
                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogException(e, "ReadOnlyReviews data portal error");
                throw;
            }
        }



        [HttpPost("[action]")]
        public IActionResult ExcecuteReviewStatisticsCountCommand([FromBody] MVCReviewStatisticsCountsCommand MVCcmd)
        {
            try
            {
                SetCSLAUser();
                ReviewStatisticsCountsCommand cmd = new ReviewStatisticsCountsCommand();
                DataPortal<ReviewStatisticsCountsCommand> dp = new DataPortal<ReviewStatisticsCountsCommand>();
                cmd = dp.Execute(cmd);
                //MVCcmd.DuplicateItems = cmd.DuplicateItems;
                //MVCcmd.ItemsDeleted = cmd.ItemsDeleted;
                //MVCcmd.ItemsExcluded = cmd.ItemsExcluded;
                //MVCcmd.ItemsIncluded = cmd.ItemsIncluded;
                return Ok(cmd);

            }
            catch (Exception e)
            {
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                _logger.LogError(e, "Dataportal Error for Review Statistics RevID: {0}", ri.ReviewId);
                throw;
            }
        }


        [HttpPost("[action]")]
        public IActionResult FetchCounts([FromBody]  SingleCriteria<bool> crit)
        {
            try
            {
                SetCSLAUser();
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;

                DataPortal<ReviewStatisticsCodeSetList> dp = new DataPortal<ReviewStatisticsCodeSetList>();

                ReviewStatisticsCodeSetList result = dp.Fetch(new SingleCriteria<ReviewStatisticsCodeSetList, bool>(crit.Value));

                //return Json(result);
                return Ok(result);
            }
            catch (Exception e)
            {
                string json = JsonConvert.SerializeObject("");
                _logger.LogError(e, "Fetching criteria: {0}", json);
                return StatusCode(500, e.Message);
            }
        }
    }

    public class MVCReviewStatisticsCountsCommand
    {
        public int ItemsIncluded;
        public int ItemsExcluded;
        public int ItemsDeleted;
        public int DuplicateItems;
    }

}
