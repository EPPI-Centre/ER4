using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;


namespace MapDatabase.Pages
{
    public class AboutModel : PageModel
    {
        private readonly ILogger _logger;

        public AboutModel(ILogger<AboutModel> logger)
        {
            _logger = logger;
        }

        public string Message { get; set; }
        public string Message2 { get; set; }
        public string btnText { get; set; }

        public void OnGet()
        {
            using (SqlConnection connection = new SqlConnection(Program.SqlHelper.DataServiceDB))
            {
                connection.Open();

                SqlParameter extName = new SqlParameter("@ExternalIDName", "pubmed");
                SqlParameter pmid = new SqlParameter("@ExternalIDValue", 123);
                try
                {
                    using (SqlDataReader reader = Program.SqlHelper.ExecuteQuerySP(connection, "st_FindCitationByExternalIDBogus", extName, pmid))
                    {

                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Error fetching existing ref and/or creating local object.");
                }


            }

            // testing

            //_logger. .LogInformation("hello Patrick");
            _logger.LogError(null, "logging test #2");

            Message = "Your application description page.";
            Message2 = "Your application description page.";
            btnText = "My button text";
        }

        protected void cmdSearch_Click(object sender, EventArgs e)
        {

        }


    }





}
