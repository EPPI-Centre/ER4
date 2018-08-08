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
        private static string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        [HttpPost("[action]"), HttpGet("[action]")]
        public IEnumerable<ReviewSet> CodesetsByReview(int RevId)//should receive a reviewID!
        {
            //int RevId = 7;
            //IEnumerable<Claim> claims = User.Claims;

            //var sss = User.Identity.Name;
            //var fff = User.Claims.First(c => c.Type == "userId").Value;
            //int cID;
            //if (!int.TryParse(fff, out cID))
            //{
            //    return null;
            //}
            //ReviewerIdentityWebClient ri = ReviewerIdentityWebClient.GetIdentity(cID, RevId, User.Identity.Name);
            //if (RevId == null || RevId == 0) RevId = 7;

            SetCSLAUser();

            List<ReviewSet> res = new List<ReviewSet>();
            using (SqlConnection conn = new SqlConnection(Program.SqlHelper.ER4DB))
            {
                SqlParameter RevID = new SqlParameter("@REVIEW_ID", RevId);
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
            IEnumerable<ReviewSet> res2 = res as IEnumerable<ReviewSet>;
            return (IEnumerable<ReviewSet>)res;
        }

        
    }
}
