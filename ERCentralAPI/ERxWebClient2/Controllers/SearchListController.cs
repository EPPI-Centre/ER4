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
		public IActionResult SearchCodes([FromBody] CodeCommand cmdIn)
		{



			try
			{
				SetCSLAUser();
				ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
			
				SearchCodesCommand cmd = new SearchCodesCommand(
					cmdIn._title, cmdIn._answers, cmdIn._included, cmdIn._withCodes
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
		public string _title = "";
		public string _answers = "";
		public bool _included = false;
		public bool _withCodes = false;
		public int _searchId = 0;
	}
}

