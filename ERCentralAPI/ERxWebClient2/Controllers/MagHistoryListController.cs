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
    public class MagBrowseHistoryListController : CSLAController
    {

        private readonly ILogger _logger;

		public MagBrowseHistoryListController(ILogger<MagBrowseHistoryListController> logger)
        {

            _logger = logger;
        }


        [HttpPost("[action]")]
        public IActionResult AddToBrowseHistory([FromBody] MVCMagBrowseHistoryItem mVCMagBrowseHistoryItem)
        {
            try
            {
                if (SetCSLAUser4Writing())
                {
                    DataPortal<MagBrowseHistoryList> dp = new DataPortal<MagBrowseHistoryList>();
                    var currentBrowseHistoryList = dp.Fetch();

                    MagBrowseHistory magBrowseHistory = new MagBrowseHistory();
                    magBrowseHistory.AttributeIds = mVCMagBrowseHistoryItem.attributeIds;
                    magBrowseHistory.ContactId = mVCMagBrowseHistoryItem.contactId;
                    magBrowseHistory.DateBrowsed = mVCMagBrowseHistoryItem.dateBrowsed;
                    magBrowseHistory.FieldOfStudy = mVCMagBrowseHistoryItem.fieldOfStudy;
                    magBrowseHistory.FieldOfStudyId = mVCMagBrowseHistoryItem.fieldOfStudyId;
                    magBrowseHistory.FindOnWeb = mVCMagBrowseHistoryItem.findOnWeb;
                    magBrowseHistory.LinkedITEM_ID = mVCMagBrowseHistoryItem.linkedITEM_ID;
                    magBrowseHistory.MagRelatedRunId = mVCMagBrowseHistoryItem.magRelatedRunId;
                    magBrowseHistory.PaperAbstract = mVCMagBrowseHistoryItem.paperAbstract;
                    magBrowseHistory.PaperFullRecord = mVCMagBrowseHistoryItem.paperFullRecord;
                    magBrowseHistory.PaperId = mVCMagBrowseHistoryItem.paperId;
                    magBrowseHistory.Title = mVCMagBrowseHistoryItem.title;
                    magBrowseHistory.URLs = mVCMagBrowseHistoryItem.uRLs;
                    currentBrowseHistoryList.Add(magBrowseHistory);

                    return Ok();
                }
                else
                {
                    return Forbid();
                }                
            }
            catch (Exception e)
            {
                _logger.LogException(e, "AddToBrowseHistory has an error");
                throw;
            }
        }

        //[HttpPost("[action]")]
        //public IActionResult GetMagBrowseHistoryList([FromBody] MVCMagBrowseHistoryItem MagBrowseHistoryItem)
        //{
        //    try
        //    {
        //        SetCSLAUser();

        //        DataPortal<MagBrowseHistoryList> dp = new DataPortal<MagBrowseHistoryList>();

        //        MagBrowseHistoryListSelectionCriteria selectionCriteria =
        //            new MagBrowseHistoryListSelectionCriteria
        //            {
        //                AttributeIds = crit.attributeIds,
        //                AuthorId = crit.authorId,
        //                FieldOfStudyId = crit.fieldOfStudyId,
        //                Included = crit.included,
        //                ITEM_ID = crit.iTEM_ID,
        //                ListType = crit.listType,
        //                MagPaperId = crit.magPaperId,
        //                MagRelatedRunId = crit.magRelatedRunId,
        //                NumResults = crit.numResults,
        //                PageNumber = crit.pageNumber,
        //                PageSize = crit.pageSize,
        //                PaperIds = crit.paperIds,
        //                DateFrom = crit.dateFrom,
        //                DateTo = crit.dateTo
        //            };

        //        var result = dp.Fetch(selectionCriteria);
        //        return Ok(new MAGList4Json(result));
        //    }
        //    catch (Exception e)
        //    {
        //        _logger.LogException(e, "Getting a MagBrowseHistoryList has an error");
        //        throw;
        //    }
        //}

    }
    public class MVCMagBrowseHistoryItem
    {
        public string title { get; set; }
        public string browseType { get; set; }
        public long paperId { get; set; }
        public string paperFullRecord { get; set; }
        public string paperAbstract  { get; set; }
        public long fieldOfStudyId  { get; set; }
        public string fieldOfStudy  { get; set; }
        public string attributeIds  { get; set; }
        public int magRelatedRunId  { get; set; }
        public long linkedITEM_ID  { get; set; }
        public string uRLs  { get; set; }
        public string findOnWeb  { get; set; }
        
        public long contactId  { get; set; }
        public DateTime dateBrowsed  { get; set; }
    }

}

