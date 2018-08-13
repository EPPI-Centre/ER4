using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BusinessLibrary.Security;
using ERxWebClient2.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERxWebClient2.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class CodesetController : CSLAController
    {
        [HttpGet("[action]")]
        public IEnumerable<ReviewSet> CodesetsByReview()
        {//not using CSLA object!! this needs revising
            SetCSLAUser();
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            List<ReviewSet> res = new List<ReviewSet>();
            using (SqlConnection conn = new SqlConnection(Program.SqlHelper.ER4DB))
            {
                SqlParameter RevID = new SqlParameter("@REVIEW_ID", ri.ReviewId);
                try
                {
                    using (SqlDataReader reader = Program.SqlHelper.ExecuteQuerySP(conn, "st_ReviewSets", RevID))
                    {
                        if (reader != null)
                        {
                            res = ReviewSet.GetReviewSets(conn, reader);
                        }
                    }
                }
                catch (Exception e)
                {
                    Program.Logger.LogSQLException(e, "Error fetching list of codesets", RevID);
                }
            }
            return (IEnumerable<ReviewSet>)res;
        }
    }
}
