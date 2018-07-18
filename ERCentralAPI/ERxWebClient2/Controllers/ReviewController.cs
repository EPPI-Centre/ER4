using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using BusinessLibrary.BusinessClasses;
using Csla.Data;
using ERxWebClient2.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace ERxWebClient2.Controllers
{
    [Route("api/[controller]")]
    public class ReviewController : Controller
    {
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
                    Program.Logger.LogSQLException(e, "Error fetching list of codesets", cid);
                }
            }
            IEnumerable<ReviewSet> res2 = res as IEnumerable<ReviewSet>;
            return (IEnumerable<ReadOnlyReview>)res;
        }

        
    }
}
