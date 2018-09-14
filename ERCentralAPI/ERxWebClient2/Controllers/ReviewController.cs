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
    public class ReviewController : CSLAController
    {

        [HttpGet("[action]")]
        public IActionResult ReadOnlyReviews()//should receive a reviewID!
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

        //[HttpGet("[action]")]
        //public ReadOnlyReviewList ReadOnlyReviews()//should receive a reviewID!
        //{
        //    SetCSLAUser();
        //    ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;

        //    DataPortal<ReadOnlyReviewList> dp = new DataPortal<ReadOnlyReviewList>();
        //    SingleCriteria<ReadOnlyReviewList, int> criteria = new SingleCriteria<ReadOnlyReviewList, int>(ri.UserId);
        //    ReadOnlyReviewList result = dp.Fetch(criteria);

       
        //    return result;
        //}

    }
}
