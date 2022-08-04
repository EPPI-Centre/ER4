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

		public SearchListController(ILogger<SearchListController> logger) : base(logger)
		{ }

		[HttpGet("[action]")]
		public IActionResult GetSearches()
		{
			try
			{
				if (!SetCSLAUser()) return Unauthorized();

				DataPortal<SearchList> dp = new DataPortal<SearchList>();
				SearchList result = dp.Fetch();

				return Ok(result);
			}
			catch (Exception e)
			{
				_logger.LogException(e, "Getting a searches list has an error");
				return StatusCode(500, e.Message);
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
				return StatusCode(500, e.Message);
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
				return StatusCode(500, e.Message);
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
				return StatusCode(500, e.Message);
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
					DataPortal<SearchCodesCommand> dp = new DataPortal<SearchCodesCommand>();
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
				return StatusCode(500, e.Message);
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
				return StatusCode(500, e.Message);
			}

		}

		[HttpPost("[action]")]
		public IActionResult SearchOneFile([FromBody] CodeCommand cmdIn)
		{
			try
			{
				if (SetCSLAUser4Writing())
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
				return StatusCode(500, e.Message);
			}
		}

		[HttpPost("[action]")]
		public IActionResult SearchWithLinkedReferences([FromBody] CodeCommand cmdIn)
		{
			try
			{
				if (SetCSLAUser4Writing())
				{

					SearchForItemsWithLinkedRefsCommand cmd = new SearchForItemsWithLinkedRefsCommand(
						cmdIn._title,
						cmdIn._included
						);
					DataPortal<SearchForItemsWithLinkedRefsCommand> dp = new DataPortal<SearchForItemsWithLinkedRefsCommand>();
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
				return StatusCode(500, e.Message);
			}
		}


		[HttpPost("[action]")]
		public IActionResult SearchText([FromBody] CodeCommand cmdIn)
		{

			try
			{
				if (SetCSLAUser4Writing())
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
				return StatusCode(500, e.Message);
			}

		}


		[HttpPost("[action]")]
		public IActionResult SearchCodeSetCheck([FromBody] CodeCommand cmdIn)
		{

			try
			{
				if (SetCSLAUser4Writing())
				{
					SearchCodeSetCheckCommand cmd = new SearchCodeSetCheckCommand(
						cmdIn._setID,
						cmdIn._included,
						cmdIn._withCodes,
						cmdIn._title,
						cmdIn._contactId,
						cmdIn._contactName);
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
				return StatusCode(500, e.Message);
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
				return StatusCode(500, e.Message);
			}
		}


		[HttpPost("[action]")]
		public IActionResult SearchClassifierScores([FromBody] CodeCommand cmdIn)
		{
			try
			{
				if (SetCSLAUser4Writing())
				{
					SearchClassifierScoresCommand cmd = new SearchClassifierScoresCommand(
						cmdIn._searchType,
						cmdIn._scoreOne,
						cmdIn._scoreTwo,
						cmdIn._searchId,
						cmdIn._searchText
						);
					DataPortal<SearchClassifierScoresCommand> dp = new DataPortal<SearchClassifierScoresCommand>();
					
					cmd = dp.Execute(cmd);

					return Ok(cmd);
				}
				else return Forbid();
			}
			catch (Exception e)
			{
				_logger.LogException(e, "Searches based on Classifier Scores parameters has failed");
				return StatusCode(400, e.Message);
			}
		}

		[HttpPost("[action]")]
		public IActionResult SearchSources([FromBody] CodeCommand cmdIn)
		{
			try
			{
				if (SetCSLAUser4Writing())
				{
					SearchSourcesCommand cmd = new SearchSourcesCommand(
						 cmdIn._title,
						 cmdIn._sourceIds,
						 cmdIn._searchId,
						 cmdIn._searchWhat
						);
					DataPortal<SearchSourcesCommand> dp = new DataPortal<SearchSourcesCommand>();

					cmd = dp.Execute(cmd);

					return Ok(cmd);
				}
				else return Forbid();
			}
			catch (Exception e)
			{
				_logger.LogException(e, "Searches based on Source parameters has failed");
				return StatusCode(400, e.Message);
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
				return StatusCode(500, e.Message);
			}
		}

		[HttpPost("[action]")]
		public IActionResult CreateVisualiseData([FromBody] SearchID ID)
		{

			try
			{
				if (!SetCSLAUser()) return Unauthorized();

				//SearchDeleteCommand cmd = new SearchVisualise(_searchId);
				DataPortal<SearchVisualiseList> dp = new DataPortal<SearchVisualiseList>();
				SingleCriteria<SearchVisualiseList, int> criteria = new SingleCriteria<SearchVisualiseList, int>(ID.searchId);
				SearchVisualiseList result = dp.Fetch(criteria);
				return Ok(result);


			}
			catch (Exception e)
			{
				_logger.LogException(e, "CreateVisualiseData of Search Data has failed");
				return StatusCode(500, e.Message);
			}
		}


		[HttpPost("[action]")]
		public IActionResult UpdateSearchName([FromBody] JSONSearchName data)
		{
			try
			{
				if (SetCSLAUser4Writing())
				{
					DataPortal<Search> dp = new DataPortal<Search>();
					Search res = dp.Fetch(new SingleCriteria<Search, int>(data.SearchID));
					res.Title = data.SearchName;
					res = res.Save(); // asking object to save itself
					//return Ok(res.Result);
					return Ok(true);
				}
				else return Forbid();
			}
			catch (Exception e)
			{
				_logger.LogException(e, "Contact data portal error");
				return StatusCode(500, e.Message);
			}
		}


	}

	public class JSONSearchName
	{
		public int SearchID = 0;
		public string SearchName = "";
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
		public bool _deleted = false;
		public bool _withCodes = false;
		public int _searchId = 0;
		public string _searches = "";
		public int _contactId = 0;
		public string _contactName = "";
		public string _searchType = "";
		public string _sourceIds = "";
		public int _scoreOne = 0;
		public int _scoreTwo = 0;
		public string _searchWhat = "";
	}
}

