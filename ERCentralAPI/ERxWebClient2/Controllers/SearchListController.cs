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
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using EPPIDataServices.Helpers;
using Newtonsoft.Json;

namespace ERxWebClient2.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class SearchListController : CSLAController
    {

        private readonly ILogger _logger;
		private SearchCodesCommand cmd;

		public SearchListController(ILogger<ReviewController> logger)
        {

            _logger = logger;
        }

        [HttpPost("[action]")]
        public IActionResult GetSearches()
        {

		
			try
            {
                SetCSLAUser();
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;

                DataPortal<SearchList> dp = new DataPortal<SearchList>();
				SearchList result = dp.Fetch();

                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogException(e, "GetSearches data portal error");
                throw;
            }

		}


		[HttpPost("[action]")]
		public IActionResult SearchCodes(CodeCommand cmdIn)
		{



			try
			{
				SetCSLAUser();
				ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
			
				SearchCodesCommand cmd = new SearchCodesCommand(
					cmdIn.title, cmdIn.answers, cmdIn.included, cmdIn.withCodes
					);
				DataPortal <SearchCodesCommand> dp = new DataPortal<SearchCodesCommand>();
				cmd = dp.Execute(cmd);

				return Ok(cmd);
			}
			catch (Exception e)
			{
				_logger.LogException(e, "GetSearches data portal error");
				throw;
			}

		}

	}

	public class CodeCommand
	{
		public string title = "";
		public string answers = "";
		public bool included = false;
		public bool withCodes = false;
		public int searchId = 0;
	}
}

