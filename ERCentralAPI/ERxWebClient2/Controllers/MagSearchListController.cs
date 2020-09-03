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
        public IActionResult CreateMagSearch([FromBody] MVCMagSearch mVCMagSearch)
        {

            try
            {
                if (!SetCSLAUser()) return Unauthorized();

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
                                newSearch.GetSearchTextPubDateExactly(mVCMagSearch.magSearchDate1.ToString("yyyy-MM-dd")) + ")";
                            newSearch.SearchText += " AND published on: " + mVCMagSearch.magSearchDate1.ToString("yyyy-MM-dd");
                            break;
                        case 2:
                            newSearch.MagSearchText = "AND(" + newSearch.MagSearchText + "," +
                                newSearch.GetSearchTextPubDateBefore(mVCMagSearch.magSearchDate1.ToString("yyyy-MM-dd")) + ")";
                            newSearch.SearchText += " AND published before: " + mVCMagSearch.magSearchDate1.ToString("yyyy-MM-dd");
                            break;
                        case 3:
                            newSearch.MagSearchText = "AND(" + newSearch.MagSearchText + "," +
                                newSearch.GetSearchTextPubDateFrom(mVCMagSearch.magSearchDate1.ToString("yyyy-MM-dd")) + ")";
                            newSearch.SearchText += " AND published after: " + mVCMagSearch.magSearchDate1.ToString("yyyy-MM-dd");
                            break;
                        case 4:
                            newSearch.MagSearchText = "AND(" + newSearch.MagSearchText + "," +
                                newSearch.GetSearchTextPubDateBetween(mVCMagSearch.magSearchDate1.ToString("yyyy-MM-dd"),
                                    mVCMagSearch.magSearchDate2.ToString("yyyy-MM-dd")) + ")";
                            newSearch.SearchText += " AND published between: " + mVCMagSearch.magSearchDate1.ToString("yyyy-MM-dd") + " and " +
                                mVCMagSearch.magSearchDate2.ToString("yyyy-MM-dd");
                            break;
                        case 5:
                            newSearch.MagSearchText = "AND(" + newSearch.MagSearchText + "," +
                                newSearch.GetSearchTextYearExactly(mVCMagSearch.magSearchDate1.Year.ToString()) + ")";
                            newSearch.SearchText += " AND year of publication: " + mVCMagSearch.magSearchDate1.Year.ToString();
                            break;
                        case 6:
                            newSearch.MagSearchText = "AND(" + newSearch.MagSearchText + "," +
                                newSearch.GetSearchTextYearBefore(mVCMagSearch.magSearchDate1.Year.ToString()) + ")";
                            newSearch.SearchText += " AND year of publication before: " + mVCMagSearch.magSearchDate1.Year.ToString();
                            break;
                        case 7:
                            newSearch.MagSearchText = "AND(" + newSearch.MagSearchText + "," +
                                newSearch.GetSearchTextYearAfter(mVCMagSearch.magSearchDate1.Year.ToString()) + ")";
                            newSearch.SearchText += " AND year of publication after: " + mVCMagSearch.magSearchDate1.Year.ToString();
                            break;
                        case 8:
                            newSearch.MagSearchText = "AND(" + newSearch.MagSearchText + "," +
                                newSearch.GetSearchTextYearBetween(mVCMagSearch.magSearchDate1.Year.ToString(),
                                    mVCMagSearch.magSearchDate2.Year.ToString()) + ")";
                            newSearch.SearchText += " AND year of publication between: " + mVCMagSearch.magSearchDate1.Year.ToString() + " and " +
                                mVCMagSearch.magSearchDate2.Year.ToString();
                            break;
                    }
                }

                if (mVCMagSearch.publicationTypeSelection > 0)
                {
                    newSearch.MagSearchText = "AND(" + newSearch.MagSearchText + "," +
                        newSearch.GetSearchTextPublicationType((mVCMagSearch.publicationTypeSelection - 1).ToString()) + ")";
                    newSearch.SearchText += " AND publication type: " + newSearch.GetPublicationType(mVCMagSearch.publicationTypeSelection - 1);
                }
                
                newSearch.BeginSave();

                DataPortal<MagSearchList> dp = new DataPortal<MagSearchList>();

                var result = dp.Fetch();

                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Fetching a Creating a MagSearch has an error");
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
        public DateTime magSearchDate1 { get; set; }
        public DateTime magSearchDate2 { get; set; }

        public string magSearchCurrentTopic { get; set; }
    }
}

