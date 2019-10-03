using System;
using BusinessLibrary.BusinessClasses;
using BusinessLibrary.Security;
using Csla;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using EPPIDataServices.Helpers;

namespace ERxWebClient2.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class SearchListController : CSLAController
    {

        private readonly ILogger _logger;

		public SearchListController(ILogger<SearchListController> logger)
        {

            _logger = logger;
        }

        [HttpGet("[action]")]
        public IActionResult GetSearches()
        {
			try
            {
                SetCSLAUser();

                DataPortal<SearchList> dp = new DataPortal<SearchList>();
				SearchList result = dp.Fetch();

                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Getting a searches list has an error");
                throw;
            }
		}

		[HttpPost("[action]")]
		public IActionResult SearchIDs([FromBody] CodeCommand cmdIn)
		{

			try
			{
				if (SetCSLAUser4Writing())
				{
					SearchIDsCommand cmd = new SearchIDsCommand(
						cmdIn._title, cmdIn._included
						);
					DataPortal<SearchIDsCommand> dp = new DataPortal<SearchIDsCommand>();
					cmd = dp.Execute(cmd);

					return Ok(cmd.SearchId);
				}
				else
				{
					return Forbid();
				}
			}
			catch (Exception e)
			{
				_logger.LogException(e, "Creating searches using IDs has an error");
				throw;
			}

		}


		[HttpPost("[action]")]
		public IActionResult SearchImportedIDs([FromBody] CodeCommand cmdIn)
		{

			try
			{
				if (SetCSLAUser4Writing())
				{
					
					SearchImportedIDsCommand cmd = new SearchImportedIDsCommand(
						cmdIn._title, cmdIn._included
						);
					DataPortal<SearchImportedIDsCommand> dp = new DataPortal<SearchImportedIDsCommand>();
					cmd = dp.Execute(cmd);

					return Ok(cmd.SearchId);
				}
				else
				{
					return Forbid();
				}
			}
			catch (Exception e)
			{
				_logger.LogException(e, "Searches based on imported IDs has failed");
				throw;
			}

		}

		[HttpPost("[action]")]
		public IActionResult SearchNoAbstract([FromBody] CodeCommand cmdIn)
		{
			try
			{
				if (SetCSLAUser4Writing())
				{
					SearchNullAbstractCommand cmd = new SearchNullAbstractCommand(
						cmdIn._included
					);
					DataPortal<SearchNullAbstractCommand> dp = new DataPortal<SearchNullAbstractCommand>();
					cmd = dp.Execute(cmd);

					return Ok(cmd.SearchId);
				}
				else
				{

					return Forbid();
				}
			}
			catch (Exception e)
			{
				_logger.LogException(e, "Creating searches with no abstract has failed");
				throw;
			}

		}


		[HttpPost("[action]")]
		public IActionResult SearchCodes([FromBody] CodeCommand cmdIn)
		{

			try
			{
				if (SetCSLAUser4Writing())
				{
						
					SearchCodesCommand cmd = new SearchCodesCommand(
						cmdIn._title, cmdIn._answers, cmdIn._included, cmdIn._withCodes
						);
					DataPortal <SearchCodesCommand> dp = new DataPortal<SearchCodesCommand>();
					cmd = dp.Execute(cmd);

					return Ok(cmd.SearchId);
				}
				else
				{

					return Forbid();
				}
			}
			catch (Exception e)
			{
				_logger.LogException(e, "Search codes has failed");
				throw;
			}

		}

		[HttpPost("[action]")]
		public IActionResult SearchNoFiles([FromBody] CodeCommand cmdIn)
		{

			try
			{
				if (SetCSLAUser4Writing())
				{
					
					SearchForUploadedFilesCommand cmd = new SearchForUploadedFilesCommand(
						cmdIn._title,
						cmdIn._included,
						false
						);
					DataPortal<SearchForUploadedFilesCommand> dp = new DataPortal<SearchForUploadedFilesCommand>();
					cmd = dp.Execute(cmd);

					return Ok(cmd.SearchId);
				}
				else
				{

					return Forbid();
				}
			}
			catch (Exception e)
			{
				_logger.LogException(e, "Searches containing no files has failed");
				throw;
			}

		}

		[HttpPost("[action]")]
		public IActionResult SearchOneFile([FromBody] CodeCommand cmdIn)
		{
			try
			{
				if(SetCSLAUser4Writing())
				{
					
					SearchForUploadedFilesCommand cmd = new SearchForUploadedFilesCommand(
						cmdIn._title,
						cmdIn._included,
						true
						);
					DataPortal<SearchForUploadedFilesCommand> dp = new DataPortal<SearchForUploadedFilesCommand>();
					cmd = dp.Execute(cmd);

					return Ok(cmd.SearchId);

				}
				else
				{

				return Forbid();
				}
			}
			catch (Exception e)
			{
				_logger.LogException(e, "Searches containing just one file has failed");
				throw;
			}
		}

		[HttpPost("[action]")]
		public IActionResult SearchText([FromBody] CodeCommand cmdIn)
		{

			try
			{
                if(SetCSLAUser4Writing())
				{
					
					SearchFreeTextCommand cmd = new SearchFreeTextCommand(
						cmdIn._title, cmdIn._included, cmdIn._searchText
						);
					DataPortal<SearchFreeTextCommand> dp = new DataPortal<SearchFreeTextCommand>();
					cmd = dp.Execute(cmd);

					return Ok(cmd.SearchId);

				}
				else
				{

					return Forbid();
				}
		}
			catch (Exception e)
			{
				_logger.LogException(e, "Creating searches based on text has failed");
				throw;
			}

		}


		[HttpPost("[action]")]
		public IActionResult SearchCodeSetCheck([FromBody] CodeCommand cmdIn)
		{

			try
			{
                if(SetCSLAUser4Writing())
				{
					
					SearchCodeSetCheckCommand cmd = new SearchCodeSetCheckCommand(
						cmdIn._setID,
						cmdIn._included,
						cmdIn._withCodes,
						cmdIn._title,
                        0,
                        "");
					DataPortal<SearchCodeSetCheckCommand> dp = new DataPortal<SearchCodeSetCheckCommand>();
					cmd = dp.Execute(cmd);

					return Ok(cmd.SearchId);

				}
				else
				{

				return Forbid();
			}
		}
			catch (Exception e)
			{
				_logger.LogException(e, "Searches with a code set check has failed");
				throw;
			}

		}


		[HttpPost("[action]")]
		public IActionResult SearchCodeLogic([FromBody] CodeCommand cmdIn)
		{
			try
			{
                if (SetCSLAUser4Writing())
                {

                    SearchCombineCommand cmd = new SearchCombineCommand(
                    cmdIn._title,
                    cmdIn._searches,
                    cmdIn._logicType,
                    cmdIn._included
                    );
                    DataPortal<SearchCombineCommand> dp = new DataPortal<SearchCombineCommand>();
                    cmd = dp.Execute(cmd);

                    return Ok(cmd);
                }
                else return Forbid();
			}
			catch (Exception e)
			{
				_logger.LogException(e, "Searches based on logic parameters has failed");
				throw;
			}
		}

		[HttpPost("[action]")]
		public IActionResult DeleteSearch([FromBody] SingleStringCriteria _searches)
		{
			
			try
			{
				if (SetCSLAUser4Writing())
				{
					SearchDeleteCommand cmd = new SearchDeleteCommand(_searches.Value);
					DataPortal<SearchDeleteCommand> dp = new DataPortal<SearchDeleteCommand>();
					cmd = dp.Execute(cmd);
					return Ok(cmd);
				}
				else return Forbid();
				
			}
			catch (Exception e)
			{
				_logger.LogException(e, "Deletion of searches has failed");
				throw;
			}
		}

		[HttpPost("[action]")]
		public IActionResult CreateVisualiseData([FromBody] SearchID ID)
		{

			try
			{
                SetCSLAUser();
				
				//SearchDeleteCommand cmd = new SearchVisualise(_searchId);
				DataPortal<SearchVisualiseList> dp = new DataPortal<SearchVisualiseList>();
				SingleCriteria<SearchVisualiseList, int> criteria = new SingleCriteria<SearchVisualiseList, int>(ID.searchId);
				SearchVisualiseList result = dp.Fetch(criteria);
				return Ok(result);
				

			}
			catch (Exception e)
			{
				_logger.LogException(e, "CreateVisualiseData of Search Data has failed");
				throw;
			}
		}
				

	}

	
	public class SearchID
	{
		public int searchId = 0;
	}

	public class CodeCommand
	{
		public string _logicType = "";
		public int _setID = 0;
		public string _searchText = "";
		public string _IDs = "";
		public string _title = "";
		public string _answers = "";
		public bool _included = false;
		public bool _withCodes = false;
		public int _searchId = 0;
		public string _searches = "";
	}
}

