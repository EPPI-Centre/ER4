using System;
using BusinessLibrary.BusinessClasses;
using BusinessLibrary.Security;
using Csla;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using EPPIDataServices.Helpers;
using System.Linq;
using Microsoft.EntityFrameworkCore.Internal;

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
        public IActionResult ReRunMagSearch([FromBody] MVCMagReRun  magReRun)
        {
            try
            {
                if (SetCSLAUser4Writing())
                {

                    DataPortal<MagSearchList> dp = new DataPortal<MagSearchList>();
                    MagSearchList result = dp.Fetch();

                    MagSearch resultToReRun = result.Where(x => x.SearchText == magReRun.searchText && x.MagSearchText == magReRun.magSearchText).FirstOrDefault();
                    if (resultToReRun != null)
                    {
                        MagSearch newS = new MagSearch();
                        newS.SetToRerun(resultToReRun);
                        newS = newS.Save();
                        result = dp.Fetch();
                    }
                    
                    return Ok(result);
                }
                else return Forbid();

            }
            catch (Exception e)
            {
                _logger.LogException(e, "Fetching a ReRun MagSearch list has an error");
                return StatusCode(500, e.Message);
            }
        }

        [HttpPost("[action]")]
        public IActionResult DeleteMagSearch([FromBody] MVCMagSearch4Delete[] magSearches)
        {
            try
            {
                if (SetCSLAUser4Writing())
                {
                    DataPortal<MagSearchList> dp = new DataPortal<MagSearchList>();
                    MagSearchList msList = dp.Fetch();

                    foreach (var item in magSearches)
                    {
                        msList.RaiseListChangedEvents = false;
                        MagSearch ToDelete = msList.FirstOr(found => found.MagSearchId == item.magSearchId, null);
                        if (ToDelete != null)
                        {
                            int index = msList.IndexOf(ToDelete);
                            ToDelete.BeginEdit();
                            ToDelete.Delete();
                            ToDelete.ApplyEdit();
                            msList.RemoveAt(index);
                        }
                        msList.RaiseListChangedEvents = true;
                    }
                    return Ok(msList);

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
        public IActionResult CreateMagSearch([FromBody] MVCMagSearchBuilder mVCMagSearch)
        {

            try
            {
                if (!SetCSLAUser4Writing()) return Forbid();
                
                MagSearch newSearch = mVCMagSearch.toMagNewSearch();
                DataPortal<MagSearch> dp = new DataPortal<MagSearch>();
                newSearch = dp.Execute(newSearch);
                //newSearch isn't complete with all of the data, so we just get the full list and return that, in one go (because getting the list is cheap!).
                DataPortal<MagSearchList> dp2 = new DataPortal<MagSearchList>();
                MagSearchList result = dp2.Fetch();

                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Fetching a Creating a MagSearch has an error");
                return StatusCode(500, e.Message);
            }



        }

        [HttpPost("[action]")]
        public IActionResult RunMagSearch([FromBody] MVCMagSearch mVCMagSearch)
        {

            try
            {
                if (!SetCSLAUser4Writing()) return Forbid();
               


                MagSearch newSearch = mVCMagSearch.toMagSearch();
                

                DataPortal<MagSearch> dp = new DataPortal<MagSearch>();
                newSearch = dp.Execute(newSearch);

                DataPortal<MagSearchList> dp2 = new DataPortal<MagSearchList>();
                MagSearchList result = dp2.Fetch();

                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Error in RunMagSearch");
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
                        0, 0, "", 0, 0, 0, 0, magSearch.FilterOutJournal.Trim(), magSearch.FilterOutDOI.Trim(),
                        magSearch.FilterOutURL.Trim(), magSearch.FilterOutTitle.Trim(), magSearch.magSearchText, "OpenAlex search: " + magSearch.searchText);

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

                        MagSearch newSearch = new MagSearch();
                    //following James' rewriting 27/06/2022, temporary(?) NON-fix
                    //newSearch.SetCombinedSearches(mVCMagCombinedSearch.magSearchListCombine.ToList(), mVCMagCombinedSearch.logicalOperator);
                        
                    //    DataPortal<MagSearch> dp = new DataPortal<MagSearch>();
                    //    newSearch = dp.Execute(newSearch);

                    return Ok(newSearch);
               
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
        public MagSearch toMagSearch()
        {
            MagSearch res = new MagSearch();
            res.SearchText = searchText;
            res.MagSearchText = magSearchText;
            return res;
        }
        public int magSearchId
        {
            get; set;
        }
        public int reviewId
        {
            get; set;
        }

        public int contactId
        {
            get; set;
        }

        public string searchText
        {
            get; set;
        }

        public int searchNo
        {
            get; set;
        }

        public int hitsNo
        {
            get; set;
        }

        public DateTime searchDate
        {
            get; set;
        }

        public string magFolder
        {
            get; set;
        }

        public string magSearchText
        {
            get; set;
        }

        public string contactName
        {
            get; set;
        }
    }

    public class MVCMagSearchBuilder
    {
        public MagSearch toMagNewSearch()
        {
            MagSearch res = new MagSearch();
            res.SearchText = searchText;
            res.MagSearchText = magSearchText;
            res.DateFilter = dateFilter;
            res.Date1 = date1;
            res.Date2 = date2;
            return res;
        }
        public string searchText { get; set; }
        public string magSearchText { get; set; }
        public string dateFilter { get; set; }
        public string date1 { get; set; }
        public string date2 { get; set; }

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
        public string FilterOutJournal { get; set; }
        public string FilterOutURL { get; set; }
        public string FilterOutDOI { get; set; }
        public string FilterOutTitle { get; set; }
    }
    public class MVCMagSearch4Delete
    {
        public int magSearchId { get; set; }
        public int reviewId { get; set; }
        public int contactId { get; set; }
        public string searchText { get; set; }
    }

    public class MVCMagReRun
    {
        public string searchText { get; set; }
        public string magSearchText { get; set; }

    }
}

