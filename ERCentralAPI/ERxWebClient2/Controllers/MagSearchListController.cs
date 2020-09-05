using System;
using BusinessLibrary.BusinessClasses;
using BusinessLibrary.Security;
using Csla;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using EPPIDataServices.Helpers;
using System.Linq;

namespace ERxWebClient2.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class MagSearchListController : CSLAController
    {
        
		public MagSearchListController(ILogger<MagSearchListController> logger) : base(logger)
        { }

        [HttpGet("[action]")]
        public IActionResult FetchMagSearchList()
        {
			try
            {
                if (!SetCSLAUser()) return Unauthorized();

                DataPortal<MagSearchList> dp = new DataPortal<MagSearchList>();
                MagSearchList result = dp.Fetch();

                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Fetching a MagSearch list has an error");
                return StatusCode(500, e.Message);
            }
		}

        [HttpPost("[action]")]
        public IActionResult DeleteMagSearch([FromBody] SingleInt64Criteria magSearchId)
        {
            try
            {
                if (SetCSLAUser4Writing())
                {
                    DataPortal<MagSearchList> dp = new DataPortal<MagSearchList>();
                    MagSearchList result = dp.Fetch();

                    MagSearch currentMagSearch = result.FirstOrDefault(x => x.MagSearchId == magSearchId.Value);

                    currentMagSearch.Delete();
                    currentMagSearch = currentMagSearch.Save();

                    return Ok(currentMagSearch);

                }
                else return Forbid();
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Deleting a MAG Simulation list has an error");
                return StatusCode(500, e.Message);
            }
        }

        [HttpPost("[action]")]
        public IActionResult CreateMagSearch([FromBody] MVCMagSearch mVCMagSearch)
        {

            try
            {
                if (!SetCSLAUser4Writing()) return Forbid();
                DateTime magSearchDate1 = DateTime.Parse(mVCMagSearch.magSearchDate1); 
                DateTime magSearchDate2 = DateTime.Parse(mVCMagSearch.magSearchDate2); 


                MagSearch newSearch = new MagSearch();
                switch (mVCMagSearch.wordsInSelection)
                {
                    case 0:
                        newSearch.MagSearchText = newSearch.GetSearchTextTitle(mVCMagSearch.magSearchInput);
                        newSearch.SearchText = "Title: " + mVCMagSearch.magSearchInput;
                        break;
                    case 1:
                        newSearch.MagSearchText = newSearch.GetSearchTextAbstract(mVCMagSearch.magSearchInput);
                        newSearch.SearchText = "Abstract: " + mVCMagSearch.magSearchInput;
                        break;
                    case 2:
                        newSearch.MagSearchText = newSearch.GetSearchTextAuthors(mVCMagSearch.magSearchInput);
                        newSearch.SearchText = "Authors: " + mVCMagSearch.magSearchInput;
                        break;
                    case 3:
                        newSearch.MagSearchText = newSearch.GetSearchTextFieldOfStudy(mVCMagSearch.magSearchCurrentTopic);
                        newSearch.SearchText = "Topic: " + mVCMagSearch.magSearchInput;
                        break;
                    case 4:
                        newSearch.MagSearchText = newSearch.GetSearchTextMagIds(mVCMagSearch.magSearchInput);
                        newSearch.SearchText = "MAG ID(s): " + mVCMagSearch.magSearchInput;
                        break;
                    case 5:
                        newSearch.MagSearchText = newSearch.GetSearchTextJournals(mVCMagSearch.magSearchInput);
                        newSearch.SearchText = "Journal: " + mVCMagSearch.magSearchInput;
                        break;
                    default:
                        newSearch.MagSearchText = mVCMagSearch.magSearchInput;
                        newSearch.SearchText = "Custom: " + mVCMagSearch.magSearchInput;
                        break;
                }

                if (mVCMagSearch.dateLimitSelection > 0)
                {
                    switch (mVCMagSearch.dateLimitSelection)
                    {
                        case 1:
                            newSearch.MagSearchText = "AND(" + newSearch.MagSearchText + "," +
                                newSearch.GetSearchTextPubDateExactly(magSearchDate1.ToString("yyyy-MM-dd")) + ")";
                            newSearch.SearchText += " AND published on: " + magSearchDate1.ToString("yyyy-MM-dd");
                            break;
                        case 2:
                            newSearch.MagSearchText = "AND(" + newSearch.MagSearchText + "," +
                                newSearch.GetSearchTextPubDateBefore(magSearchDate1.ToString("yyyy-MM-dd")) + ")";
                            newSearch.SearchText += " AND published before: " + magSearchDate1.ToString("yyyy-MM-dd");
                            break;
                        case 3:
                            newSearch.MagSearchText = "AND(" + newSearch.MagSearchText + "," +
                                newSearch.GetSearchTextPubDateFrom(magSearchDate1.ToString("yyyy-MM-dd")) + ")";
                            newSearch.SearchText += " AND published after: " + magSearchDate1.ToString("yyyy-MM-dd");
                            break;
                        case 4:
                            newSearch.MagSearchText = "AND(" + newSearch.MagSearchText + "," +
                                newSearch.GetSearchTextPubDateBetween(magSearchDate1.ToString("yyyy-MM-dd"),
                                    magSearchDate2.ToString("yyyy-MM-dd")) + ")";
                            newSearch.SearchText += " AND published between: " + magSearchDate1.ToString("yyyy-MM-dd") + " and " +
                                magSearchDate2.ToString("yyyy-MM-dd");
                            break;
                        case 5:
                            newSearch.MagSearchText = "AND(" + newSearch.MagSearchText + "," +
                                newSearch.GetSearchTextYearExactly(magSearchDate1.Year.ToString()) + ")";
                            newSearch.SearchText += " AND year of publication: " + magSearchDate1.Year.ToString();
                            break;
                        case 6:
                            newSearch.MagSearchText = "AND(" + newSearch.MagSearchText + "," +
                                newSearch.GetSearchTextYearBefore(magSearchDate1.Year.ToString()) + ")";
                            newSearch.SearchText += " AND year of publication before: " + magSearchDate1.Year.ToString();
                            break;
                        case 7:
                            newSearch.MagSearchText = "AND(" + newSearch.MagSearchText + "," +
                                newSearch.GetSearchTextYearAfter(magSearchDate1.Year.ToString()) + ")";
                            newSearch.SearchText += " AND year of publication after: " + magSearchDate1.Year.ToString();
                            break;
                        case 8:
                            newSearch.MagSearchText = "AND(" + newSearch.MagSearchText + "," +
                                newSearch.GetSearchTextYearBetween(magSearchDate1.Year.ToString(),
                                    magSearchDate2.Year.ToString()) + ")";
                            newSearch.SearchText += " AND year of publication between: " + magSearchDate1.Year.ToString() + " and " +
                                magSearchDate2.Year.ToString();
                            break;
                    }
                }

                if (mVCMagSearch.publicationTypeSelection > 0)
                {
                    newSearch.MagSearchText = "AND(" + newSearch.MagSearchText + "," +
                        newSearch.GetSearchTextPublicationType((mVCMagSearch.publicationTypeSelection - 1).ToString()) + ")";
                    newSearch.SearchText += " AND publication type: " + newSearch.GetPublicationType(mVCMagSearch.publicationTypeSelection - 1);
                }

                DataPortal<MagSearch> dp = new DataPortal<MagSearch>();
                newSearch = dp.Execute(newSearch);

                return Ok(newSearch);
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Fetching a Creating a MagSearch has an error");
                return StatusCode(500, e.Message);
            }



        }

        [HttpPost("[action]")]
        public IActionResult ImportMagSearchPapers([FromBody] MVCMagSearchText magSearch)
        {
            try
            {
                if (SetCSLAUser4Writing())
                {

                    DataPortal<MagItemPaperInsertCommand> dp2 = new DataPortal<MagItemPaperInsertCommand>();
                    MagItemPaperInsertCommand command = new MagItemPaperInsertCommand("", "MagSearchResults",
                        0, "MAG search: " + magSearch.searchText, magSearch.magSearchText);

                    command = dp2.Execute(command);

                    return Ok(command.NImported);

                }
                else return Forbid();
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Importing a Mag Related Paper list has an error");
                return StatusCode(500, e.Message);
            }
        }


        [HttpPost("[action]")]
        public IActionResult CombineMagSearches([FromBody] MVCMagCombinedSearch mVCMagCombinedSearch)
        {

            try
            {
                if (SetCSLAUser4Writing())
                {

                    if (mVCMagCombinedSearch.magSearchListCombine.Length > 0)
                    {
                        MagSearch newSearch = new MagSearch();
                        newSearch.SetCombinedSearches(mVCMagCombinedSearch.magSearchListCombine.ToList(), mVCMagCombinedSearch.logicalOperator);
                        newSearch.BeginSave();
                    }
                    return Ok();
                } 
                else
                {
                    return Forbid();
                }
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Combining MagSearches has an error");
                return StatusCode(500, e.Message);
            }


        }


    }

    public class MVCMagSearch
    {
        public int wordsInSelection { get; set; }
        public string magSearchInput { get; set; }
        public int publicationTypeSelection { get; set; }

        public int dateLimitSelection { get; set; }
        public string magSearchDate1 { get; set; }
        public string magSearchDate2 { get; set; }

        public string magSearchCurrentTopic { get; set; }
    }

    public class MVCMagCombinedSearch
    {
        public MagSearch[] magSearchListCombine  { get; set; }
        public string logicalOperator  { get; set; }
    }

    public class MVCMagSearchText
    {
        public string magSearchText { get; set; }

        public string searchText { get; set; }
    }
}

