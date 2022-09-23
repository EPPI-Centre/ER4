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
    public class MagFieldOfStudyListController : CSLAController
    {
		public MagFieldOfStudyListController(ILogger<MagFieldOfStudyListController> logger) : base(logger)
        { }


        [HttpPost("[action]")]
        public IActionResult GetMagFieldOfStudyList([FromBody] MVCMagFieldOfStudyListSelectionCriteria crit)
        {
            try
            {
                if (!SetCSLAUser()) return Unauthorized();

                DataPortal<MagFieldOfStudyList> dp = new DataPortal<MagFieldOfStudyList>();

                MagFieldOfStudyListSelectionCriteria selectionCriteria =
                    new MagFieldOfStudyListSelectionCriteria
                    {
                        FieldOfStudyId = crit.fieldOfStudyId,
                        ListType = crit.listType,
                        PaperIdList = crit.paperIdList,
                        SearchText = crit.SearchTextTopics,
                        PageNumber = crit.pageNumber
                    };

                var result = dp.Fetch(selectionCriteria);

                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Getting a MagFieldOfStudyList has an error");
                return StatusCode(500, e.Message);
            }
        }

    }

    public class MVCMagFieldOfStudyListSelectionCriteria
    {
        public Int64 fieldOfStudyId { get; set; }
        public string listType { get; set; }
        public string paperIdList { get; set; }
        public string SearchTextTopics { get; set; }
        public Int64 pageNumber { get; set; }

    }
}

