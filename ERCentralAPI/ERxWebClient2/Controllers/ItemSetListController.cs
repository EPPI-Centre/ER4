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
using Newtonsoft.Json;

namespace ERxWebClient2.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class ItemSetListController : CSLAController
    {

        private readonly ILogger _logger;

        public ItemSetListController(ILogger<ItemSetListController> logger)
        {
            _logger = logger;
        }


        [HttpPost("[action]")]
        public IActionResult Fetch([FromBody] SingleInt64Criteria ItemIDCrit)
        {
            try
            {

                SetCSLAUser();
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;

                DataPortal<ItemSetList> dp = new DataPortal<ItemSetList>();
                SingleCriteria<ItemSetList, Int64> criteria = new SingleCriteria<ItemSetList, Int64>(ItemIDCrit.Value);
                ItemSetList result = dp.Fetch(criteria);
                //ItemSetList result = dp.Fetch(ItemIDCrit.Value);
                return Ok(result);

            }
            catch(Exception e)
            {               
                _logger.LogError(e, "Error when fetching a set list: {0}", ItemIDCrit.Value);
                return StatusCode(500, e.Message);
            }
        }
        [HttpPost("[action]")]
        public IActionResult ExcecuteItemAttributeSaveCommand([FromBody] MVCItemAttributeSaveCommand MVCcmd)
        {
            try
            {

                SetCSLAUser();
                //ReviewInfo rinf = new ReviewInfo();
                ItemAttributeSaveCommand cmd = new ItemAttributeSaveCommand(
                    MVCcmd.saveType
                    , MVCcmd.itemAttributeId
                    , MVCcmd.itemSetId
                    , MVCcmd.additionalText
                    , MVCcmd.attributeId
                    , MVCcmd.setId
                    , MVCcmd.itemId
                    , MVCcmd.itemArmId
                    , MVCcmd.revInfo.ToCSLAReviewInfo()
                    //,rinf
                    );
                DataPortal<ItemAttributeSaveCommand> dp = new DataPortal<ItemAttributeSaveCommand>();
                cmd = dp.Execute(cmd);
                MVCcmd.additionalText = cmd.AdditionalText;
                MVCcmd.attributeId = cmd.AttributeId;
                MVCcmd.itemArmId = cmd.ItemArmId;
                MVCcmd.itemAttributeId = cmd.ItemAttributeId;
                MVCcmd.itemId = cmd.ItemId;
                MVCcmd.itemSetId = cmd.ItemSetId;
                MVCcmd.setId = cmd.SetId;
                return Ok(MVCcmd);

            }
            catch (Exception e)
            {
                string json = JsonConvert.SerializeObject(MVCcmd);
                _logger.LogError(e, "Dataportal Error with Item Attributes: {0}", json);
                throw;
            }
        }
    }

    public class MVCItemAttributeSaveCommand
    {
        public string saveType { get; set; }
        public Int64 itemAttributeId { get; set; }
        public Int64 itemSetId { get; set; }
        public string additionalText { get; set; }
        public Int64 attributeId { get; set; }
        public int setId { get; set; }
        public Int64 itemId { get; set; }
        public Int64 itemArmId { get; set; }
        public MVCReviewInfo revInfo { get; set; }
    }
    public class MVCReviewInfo
    {
        public int reviewId { get; set; }
        public string reviewName { get; set; }
        public bool showScreening { get; set; }
        public int screeningCodeSetId { get; set; }
        public string screeningMode { get; set; }
        public string screeningReconcilliation { get; set; }
        public Int64 screeningWhatAttributeId { get; set; }
        public int screeningNPeople { get; set; }
        public bool screeningAutoExclude { get; set; }
        public bool screeningModelRunning { get; set; }
        public bool screeningIndexed { get; set; }
        public bool screeningListIsGood { get; set; }
        public string bL_ACCOUNT_CODE { get; set; }
        public string bL_AUTH_CODE { get; set; }
        public string bL_TX { get; set; }
        public string bL_CC_ACCOUNT_CODE { get; set; }
        public string bL_CC_AUTH_CODE { get; set; }
        public string bL_CC_TX { get; set; }
        public ReviewInfo ToCSLAReviewInfo()
        {
            ReviewInfo result = new ReviewInfo();
            result.BL_ACCOUNT_CODE = this.bL_ACCOUNT_CODE;
            result.BL_AUTH_CODE = this.bL_AUTH_CODE;
            result.BL_CC_ACCOUNT_CODE = this.bL_CC_ACCOUNT_CODE;
            result.BL_CC_AUTH_CODE = this.bL_CC_AUTH_CODE;
            result.BL_CC_TX = this.bL_CC_TX;
            result.BL_TX = this.bL_TX;
            result.ReviewId = this.reviewId;
            result.ReviewName = this.reviewName;
            result.ScreeningAutoExclude = this.screeningAutoExclude;
            result.ScreeningCodeSetId = this.screeningCodeSetId;
            result.ScreeningIndexed = this.screeningIndexed;
            result.ScreeningListIsGood = this.screeningListIsGood;
            result.ScreeningMode = this.screeningMode;
            result.ScreeningModelRunning = this.screeningModelRunning;
            result.ScreeningNPeople = this.screeningNPeople;
            result.ScreeningReconcilliation = this.screeningReconcilliation;
            result.ScreeningWhatAttributeId = this.screeningWhatAttributeId;
            result.ShowScreening = this.showScreening;
            return result;
        }
    }
}
