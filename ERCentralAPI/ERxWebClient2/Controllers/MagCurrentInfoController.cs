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
    public class MagCurrentInfoController : CSLAController
    {

        private readonly ILogger _logger;

		public MagCurrentInfoController(ILogger<MagCurrentInfoController> logger)
        {

            _logger = logger;
        }

        [HttpGet("[action]")]
        public IActionResult GetMagCurrentInfo()
        {
			try
            {
                SetCSLAUser();

                DataPortal<MagCurrentInfo> dp = new DataPortal<MagCurrentInfo>();
				MagCurrentInfo result = dp.Fetch();

                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Getting a GetMagCurrentInfo has an error");
                throw;
            }
		}

        [HttpGet("[action]")]
        public IActionResult GetMagReviewMagInfo()
        {
            try
            {
                SetCSLAUser();
                MAgReviewMagInfoCommand cmd = new MAgReviewMagInfoCommand();
                DataPortal<MAgReviewMagInfoCommand> dp = new DataPortal<MAgReviewMagInfoCommand>();
                cmd = dp.Execute(cmd);

                return Ok(cmd);
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Getting a GetMagReviewMagInfo has an error");
                throw;
            }
        }

        [HttpPost("[action]")]
        public IActionResult GetMagPaper([FromBody] SingleInt64Criteria Id)
        {
            try
            {
                SetCSLAUser();

                DataPortal<MagPaper> dp = new DataPortal<MagPaper>();
                SingleCriteria<MagPaper, Int64> criteria =
                    new SingleCriteria<MagPaper, Int64>(Id.Value);

                var result = dp.Fetch(criteria);

                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Getting a GetMagPaper has an error");
                throw;
            }
        }

        [HttpPost("[action]")]
        public IActionResult GetMagPaperList([FromBody] MVCMagPaperListSelectionCriteria crit)
        {
            try
            {
                SetCSLAUser();

                DataPortal<MagPaperList> dp = new DataPortal<MagPaperList>();

                MagPaperListSelectionCriteria selectionCriteria =
                    new MagPaperListSelectionCriteria
                    {
                        AttributeIds = crit.attributeIds,
                        AuthorId = crit.authorId,
                        FieldOfStudyId = crit.fieldOfStudyId,
                        Included = crit.included,
                        ITEM_ID = crit.iTEM_ID,
                        ListType = crit.listType,
                        MagPaperId = crit.magPaperId,
                        MagRelatedRunId = crit.magRelatedRunId,
                        NumResults = crit.numResults,
                        PageNumber = crit.pageNumber,
                        PageSize = crit.pageSize,
                        PaperIds = crit.paperIds
                    };

                var result = dp.Fetch(selectionCriteria);

                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Getting a GetMagPaperList has an error");
                throw;
            }
        }


        [HttpPost("[action]")]
        public IActionResult GetMagFieldOfStudyList([FromBody] MVCMagFieldOfStudyListSelectionCriteria crit)
        {
            try
            {
                SetCSLAUser();

                DataPortal<MagFieldOfStudyList> dp = new DataPortal<MagFieldOfStudyList>();

                MagFieldOfStudyListSelectionCriteria selectionCriteria =
                    new MagFieldOfStudyListSelectionCriteria
                    {
                        FieldOfStudyId = crit.fieldOfStudyId,
                        ListType = crit.listType,
                        PaperIdList = crit.paperIdList,
                        SearchText = crit.searchText                        
                        
                    };

                var result = dp.Fetch(selectionCriteria);

                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Getting a MagFieldOfStudyList has an error");
                throw;
            }
        }

    }


    public class MVCMagFieldOfStudyListSelectionCriteria
    {
        public Int64 fieldOfStudyId { get; set; }
        public string listType { get; set; }
        public string paperIdList { get; set; }
        public string searchText { get; set; }

    }


    public class MVCMagPaperListSelectionCriteria
    {
        public Int64 magPaperId { get; set; }
        public Int64 iTEM_ID { get; set; }
        public string listType { get; set; }
        public Int64 fieldOfStudyId { get; set; }
        public Int64 authorId { get; set; }
        public int magRelatedRunId { get; set; }
        public string paperIds { get; set; }
        public string attributeIds { get; set; }
        public string included { get; set; }
        public int pageNumber { get; set; }
        public int pageSize { get; set; }
        public int numResults { get; set; }

    }

}

