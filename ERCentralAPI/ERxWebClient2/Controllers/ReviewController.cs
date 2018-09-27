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
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                if (!ri.HasWriteRights()) return Unauthorized();

                ReviewStatisticsCountsCommand cmd = new ReviewStatisticsCountsCommand(
                    MVCcmd.ItemsDeleted
                    , MVCcmd.ItemsExcluded
                    , MVCcmd.ItemsIncluded
                    , MVCcmd.DuplicateItems

                    );
                DataPortal<ReviewStatisticsCountsCommand> dp = new DataPortal<ReviewStatisticsCountsCommand>();
                cmd = dp.Execute(cmd);
                MVCcmd.DuplicateItems = cmd.DuplicateItems;
                MVCcmd.ItemsDeleted = cmd.ItemsDeleted;
                MVCcmd.ItemsExcluded = cmd.ItemsExcluded;
                MVCcmd.ItemsIncluded = cmd.ItemsIncluded;

                return Ok(MVCcmd);

            }
            catch (Exception e)
            {
                string json = JsonConvert.SerializeObject(MVCcmd);
                _logger.LogError(e, "Dataportal Error with Review Statistics Counts: {0}", json);
                throw;
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
