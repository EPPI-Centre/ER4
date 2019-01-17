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

        // Gets relevant arms for the item in question 
        [HttpPost("[action]")]
        public IActionResult GetArms([FromBody] SingleInt64Criteria ItemIDCrit)
        {
            //return Forbid();

            try
            {
                // try with dummy item id here
                SetCSLAUser();
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;

                DataPortal<ItemArmList> dp = new DataPortal<ItemArmList>();
                SingleCriteria<Item, Int64> criteria = new SingleCriteria<Item, Int64>(ItemIDCrit.Value);
                ItemArmList result = dp.Fetch(criteria);
                
                return Ok(result);

            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error when fetching an arm list: {0}", ItemIDCrit.Value);
                return StatusCode(500, e.Message);
            }
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
                if (SetCSLAUser4Writing())
                {
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
                else return Forbid();

            }
            catch (Exception e)
            {
                string json = JsonConvert.SerializeObject(MVCcmd);
                _logger.LogError(e, "Dataportal Error with Item Attributes: {0}", json);
                throw;
            }
        }
        [HttpPost("[action]")]
        public IActionResult ExecuteItemAttributeBulkInsertCommand([FromBody] MVCItemAttributeBulkSaveCommand MVCcmd)
        {//method is "..BulkInsert.." rather than "BulkSave" 'cause we NEVER use the CSLA object (ItemAttributeBulkSaveCommand) to delete (code in there wouldn't work!).
            try
            {
                if (SetCSLAUser4Writing())
                {
                    ItemAttributeBulkSaveCommand cmd = new ItemAttributeBulkSaveCommand(
                        "Insert"
                        , MVCcmd.attributeId
                        , MVCcmd.setId
                        , MVCcmd.itemIds.Trim(',')
                        , MVCcmd.searchIds.Trim(',')
                        );
                    DataPortal<ItemAttributeBulkSaveCommand> dp = new DataPortal<ItemAttributeBulkSaveCommand>();
                    cmd = dp.Execute(cmd);
                    return Ok(MVCcmd);//command is mute, doesn't tell us anything
                }
                else return Forbid();

            }
            catch (Exception e)
            {
                string json = JsonConvert.SerializeObject(MVCcmd);
                _logger.LogError(e, "Dataportal Error with Item Attributes: {0}", json);
                throw;
            }
        }
        [HttpPost("[action]")]
        public IActionResult ExecuteItemAttributeBulkDeleteCommand([FromBody] MVCItemAttributeBulkSaveCommand MVCcmd)
        {
            try
            {
                if (SetCSLAUser4Writing())
                {
                    ItemAttributeBulkDeleteCommand cmd = new ItemAttributeBulkDeleteCommand(
                        MVCcmd.attributeId
                        , MVCcmd.itemIds.Trim(',')
                        , MVCcmd.setId
                        , MVCcmd.searchIds.Trim(',')
                        );
                    DataPortal<ItemAttributeBulkDeleteCommand> dp = new DataPortal<ItemAttributeBulkDeleteCommand>();
                    cmd = dp.Execute(cmd);
                    return Ok(MVCcmd);//command is mute, doesn't tell us anything
                }
                else return Forbid();

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
    public class MVCItemAttributeBulkSaveCommand
    {
        public long attributeId;
        public int setId;
        public string itemIds;
        public string searchIds;
    }
}
