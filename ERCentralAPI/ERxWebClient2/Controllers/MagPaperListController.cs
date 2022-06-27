using System;
using BusinessLibrary.BusinessClasses;
using Csla;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using EPPIDataServices.Helpers;


namespace ERxWebClient2.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class MagPaperListController : CSLAController
    {
        
		public MagPaperListController(ILogger<MagPaperListController> logger) : base(logger)
        { }


        [HttpPost("[action]")]
        public IActionResult GetMagPaper([FromBody] SingleInt64Criteria Id)
        {
            try
            {
                if (!SetCSLAUser()) return Unauthorized();

                DataPortal<MagPaper> dp = new DataPortal<MagPaper>();
                SingleCriteria<MagPaper, Int64> criteria =
                    new SingleCriteria<MagPaper, Int64>(Id.Value);

                var result = dp.Fetch(criteria);

                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Getting a MagPaper has an error");
                return StatusCode(500, e.Message);
            }
        }

        // TODO: this needs fixing why is LinkedITEM_ID not > 0
        [HttpPost("[action]")]
        public IActionResult UpdateMagPaper([FromBody] MVCMagPaperCorrectnessState magPaperState)
        {
            try
            {
                if (SetCSLAUser4Writing())
                {

                    DataPortal<MagPaper> dp = new DataPortal<MagPaper>();
                    SingleCriteria<MagPaper, Int64> criteria =
                        new SingleCriteria<MagPaper, Int64>(magPaperState.magPaperId);

                    var magPaper = dp.Fetch(criteria);


                    var manualFalseMatchProperty = !magPaperState.manualTrueMatchProperty;
                    DataPortal<MagPaper> dp2 = new DataPortal<MagPaper>();
                    MagMatchItemToPaperManualCommand cmd = new MagMatchItemToPaperManualCommand(magPaperState.itemId,
                        magPaperState.magPaperId, magPaperState.manualTrueMatchProperty, manualFalseMatchProperty);

                    magPaper.LinkedITEM_ID = magPaperState.itemId;
                    if (magPaper.LinkedITEM_ID > 0)
                    {
                        magPaper.ManualFalseMatch = !magPaperState.manualTrueMatchProperty;
                        magPaper.ManualTrueMatch = magPaperState.manualTrueMatchProperty;
                        magPaper = dp.Update(magPaper);
                        return Ok(magPaper);
                    }
                    else
                    {
                        throw new Exception("magPaper has a LinkedITEM_ID of 0!");
                    }

                }
                else return Forbid();

            }
            catch (Exception e)
            {
                _logger.LogException(e, "Updating a Mag Paper has produced error!");
                return StatusCode(500, e.Message);
            }
        }


        [HttpPost("[action]")]
        public IActionResult GetMagPaperList([FromBody] MVCMagPaperListSelectionCriteria crit)
        {
            try
            {
                if (!SetCSLAUser()) return Unauthorized();

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
                        PaperIds = crit.paperIds,
                        DateFrom = crit.dateFrom,
                        DateTo = crit.dateTo,
                        //following James' rewriting 27/06/2022, temporary(?) fix
                        //MagSearchText = crit.magSearchText,
                        MagAutoUpdateRunId = crit.magAutoUpdateRunId,
                        AutoUpdateOrderBy = crit.autoUpdateOrderBy,
                        AutoUpdateAutoUpdateScore = crit.autoUpdateAutoUpdateScore,
                        AutoUpdateStudyTypeClassifierScore = crit.autoUpdateStudyTypeClassifierScore,
                        AutoUpdateUserClassifierScore = crit.autoUpdateUserClassifierScore,
                        AutoUpdateUserTopN = crit.autoUpdateUserTopN
                    };

                var result = dp.Fetch(selectionCriteria);
                return Ok(new MAGList4Json(result));
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Getting a MagPaperList has an error");
                return StatusCode(500, e.Message);
            }
        }

    }
    public class MVCMagPaperCorrectnessState
    {
        public Int64 itemId { get; set; }
        public bool manualTrueMatchProperty { get; set; }

        public Int64 magPaperId { get; set; }
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
        public string dateFrom { get; set; }
        public string dateTo { get; set; }
        public string magSearchText { get; set; }
        public int magAutoUpdateRunId { get; set; }
        public string autoUpdateOrderBy { get; set; }        
        public double autoUpdateAutoUpdateScore { get; set; }  
        public double autoUpdateStudyTypeClassifierScore { get; set; }  
        public double autoUpdateUserClassifierScore { get; set; }  
        public int autoUpdateUserTopN { get; set; }  
    }

}

