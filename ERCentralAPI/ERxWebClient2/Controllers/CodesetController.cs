using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using ERxWebClient2.Models;
using Microsoft.AspNetCore.Mvc;

namespace ERxWebClient2.Controllers
{
    [Route("api/[controller]")]
    public class CodesetController : Controller
    {
        private static string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        [HttpGet("[action]")]
        public IEnumerable<ReviewSet> CodesetsByReview()//should receive a reviewID!
        {
            int RevId = 7;
            if (RevId == null || RevId == 0) RevId = 7;
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
