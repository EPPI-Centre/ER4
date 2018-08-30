using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using BusinessLibrary.BusinessClasses;
using Csla.Data;
using EPPIDataServices.Helpers;
using ERxWebClient2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace ERxWebClient2.Controllers
{
    [Route("api/[controller]")]
    public class ReviewController : Controller
    {

        private readonly ILogger _logger;

        public ReviewController(ILogger<EPPILogger> logger)
        {
            _logger = logger;
        }

        [HttpPost("[action]")]
        public IEnumerable<ReadOnlyReview> ReviewsByContact(int contactId)//should receive a reviewID!
        {
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
                    SqlParameter[] sqlParams = new SqlParameter[1];
                    sqlParams[0] = cid;
                    _logger.SQLActionFailed("Error fetching list of codesets", sqlParams, e);
                }
            }
            IEnumerable<ReviewSet> res2 = res as IEnumerable<ReviewSet>;
            return (IEnumerable<ReadOnlyReview>)res;
        }

        
    }
}
