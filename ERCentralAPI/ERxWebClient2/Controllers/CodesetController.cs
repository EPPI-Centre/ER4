using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using EPPIDataServices.Helpers;
using ERxWebClient2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ERxWebClient2.Controllers
{
    [Route("api/[controller]")]
    public class CodesetController : Controller
    {

        private readonly ILogger _logger;

        public CodesetController(ILogger<EPPILogger> logger)
        {
            _logger = logger;
        }

        private static string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        [HttpPost("[action]"), HttpGet("[action]")]
        public IEnumerable<ReviewSet> CodesetsByReview(int RevId)//should receive a reviewID!
        {
            //int RevId = 7;
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
                    SqlParameter[] sqlParams = new SqlParameter[1];
                    sqlParams[0] = RevID;
                    _logger.SQLActionFailed("Error fetching list of codesets", sqlParams, e);
                }
            }
            IEnumerable<ReviewSet> res2 = res as IEnumerable<ReviewSet>;
            return (IEnumerable<ReviewSet>)res;
        }
    }
}
