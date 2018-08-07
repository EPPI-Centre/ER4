using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using BusinessLibrary.BusinessClasses;
using BusinessLibrary.Security;
using Csla.Data;
using ERxWebClient2.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace ERxWebClient2.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class ReviewController : Controller
    {
        public ReviewController()
        {
            
            
        }
        private void SetCSLAUser()
        {//this needs to be a Controller Extension, then, in controller methods, we call it when needed (i.e. when using CSLA objects that require reviewerIdentity)
            var sss = User.Identity.Name;
            User.Claims.Append(new Claim("Name", sss));
            var userId = User.Claims.First(c => c.Type == "userId").Value;
            int cID;
            bool canProceed = true;
            canProceed = int.TryParse(userId, out cID);

            if (canProceed)
            {
                var revId0 = User.Claims.First(c => c.Type == "reviewId");
                string revId = "0";
                if (revId0 == null || revId0.Value == null) canProceed = false;
                else
                {
                    revId = revId0.Value;
                    int RevId;
                    canProceed = int.TryParse(revId, out RevId);
                    if (canProceed)
                    {
                        ReviewerIdentityWebClient ri = ReviewerIdentityWebClient.GetIdentity(cID, RevId);
                        //ReviewerPrincipal principal = new ReviewerPrincipal(ri as System.Security.Principal.IIdentity);
                        ReviewerPrincipal principal = new ReviewerPrincipal(ri);
                        Csla.ApplicationContext.User = principal;
                        //
                    }
                }
            }
        }
        [HttpPost("[action]")]
        public IEnumerable<ReadOnlyReview> ReviewsByContact(int contactId)//should receive a reviewID!
        {
            SetCSLAUser();
            ReadOnlyReviewList returnValue = new ReadOnlyReviewList();
            ReadOnlyReviewList.GetReviewList(contactId, (o, e) =>
            {
                if (e.Error == null)
                {
                    returnValue = e.Object;
                }
            });



            List<ReadOnlyReview> res = new List<ReadOnlyReview>();
            using (SqlConnection conn = new SqlConnection(Program.SqlHelper.ER4DB))
            {
                SqlParameter cid = new SqlParameter("@CONTACT_ID", contactId);
                try
                {
                    using (SafeDataReader reader = Program.SqlHelper.ExecuteCSLAQuerySP(conn, "st_ReviewContact", cid))
                    {
                        if (reader != null)
                        {
                            while (reader.Read()) res.Add(ReadOnlyReview.GetReadOnlyReview(reader));
                        }
                    }
                }
                catch (Exception e)
                {
                    Program.Logger.LogSQLException(e, "Error fetching list of codesets", cid);
                }
            }
            IEnumerable<ReviewSet> res2 = res as IEnumerable<ReviewSet>;
            return (IEnumerable<ReadOnlyReview>)res;
        }

        
    }
}
