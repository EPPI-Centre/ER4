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
    //[Authorize]
    //[Route("api/[controller]")]
    //public class ReviewController : CSLAController
    //{
    //    [HttpPost("[action]")]
    //    public IEnumerable<ReadOnlyReview> ReviewsByContact(int contactId)//should receive a reviewID!
    //    {
    //        SetCSLAUser();
    //        ReadOnlyReviewList returnValue = new ReadOnlyReviewList();
    //        ReadOnlyReviewList.GetReviewList(contactId, (o, e) =>
    //        {
    //            if (e.Error == null)
    //            {
    //                returnValue = e.Object;
    //            }
    //        });



    //        List<ReadOnlyReview> res = new List<ReadOnlyReview>();
    //        using (SqlConnection conn = new SqlConnection(Program.SqlHelper.ER4DB))
    //        {
    //            SqlParameter cid = new SqlParameter("@CONTACT_ID", contactId);
    //            try
    //            {
    //                using (SafeDataReader reader = Program.SqlHelper.ExecuteCSLAQuerySP(conn, "st_ReviewContact", cid))
    //                {
    //                    if (reader != null)
    //                    {
    //                        while (reader.Read()) res.Add(ReadOnlyReview.GetReadOnlyReview(reader));
    //                    }
    //                }
    //            }
    //            catch (Exception e)
    //            {
    //                Program.Logger.LogSQLException(e, "Error fetching list of codesets", cid);
    //            }
    //        }
    //        IEnumerable<ReviewSet> res2 = res as IEnumerable<ReviewSet>;
    //        return (IEnumerable<ReadOnlyReview>)res;
    //    }


    //}

    [Authorize]
    [Route("api/[controller]")]
    public class ReviewController : CSLAController
    {

        [HttpGet("[action]")]
        public ReadOnlyReviewList ReadOnlyReviews()//should receive a reviewID!
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
            return result;
        }

    }
}
