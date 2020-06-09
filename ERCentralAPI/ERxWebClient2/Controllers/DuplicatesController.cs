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
using EPPIDataServices.Helpers;
using System.Threading;

namespace ERxWebClient2.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class DuplicatesController : CSLAController
    {

        private readonly ILogger _logger;
        

        public DuplicatesController(ILogger<ReviewerTermListController> logger)
        {
            _logger = logger;
        }

        [HttpPost("[action]")]
        public IActionResult FetchGroups([FromBody] SingleBoolCriteria crit)
        {
            try
            {
                SetCSLAUser();
                if (crit.Value)
                {
                    //we may need to stop the long-lasting "get new duplicates" task, so we pass the 
                    _logger.LogWarning("I'm logging a request to get new dups");
                }
                DataPortal<ItemDuplicateReadOnlyGroupList> dp = new DataPortal<ItemDuplicateReadOnlyGroupList>();
                ItemDuplicateReadOnlyGroupList result = dp.Fetch(new SingleCriteria<ItemDuplicateReadOnlyGroupList, bool>(crit.Value));
                return Ok(result);
            }
            catch(Exception e)
            {
                _logger.LogException(e, "Error Fetching DuplicateGroupsList. GetNew = " + crit.Value.ToString());
                return StatusCode(500, e.Message);
            }
        }


		[HttpPost("[action]")]
		public IActionResult FetchGroupsWithCriteria([FromBody] TrainingReviewerTermJSON data)
		{
			try
			{
                SetCSLAUser();
				

				return Ok();
			}
			catch (Exception e)
			{
				_logger.LogException(e, "Error with creating TrainingReviewerTerm");
				return StatusCode(500, e.Message);
			}
		}

        [HttpPost("[action]")]
        public IActionResult FetchGroupDetails([FromBody] SingleIntCriteria crit)
        {
            try
            {
                SetCSLAUser();
                DataPortal<ItemDuplicateGroup> dp = new DataPortal<ItemDuplicateGroup>();
                ItemDuplicateGroup result = dp.Fetch(new SingleCriteria<ItemDuplicateGroup, int>(crit.Value));
                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Error Fetching GroupDetails. GroupId = " + crit.Value.ToString());
                return StatusCode(500, e.Message);
            }
        }
        [HttpPost("[action]")]
        public IActionResult MarkUnmarkMemberAsDuplicate([FromBody] MarkUnmarkItemAsDuplicate crit)
        {
            try
            {
                if (SetCSLAUser4Writing())
                {
                    DataPortal<ItemDuplicateGroup> dp = new DataPortal<ItemDuplicateGroup>();
                    ItemDuplicateGroup IDG = dp.Fetch(new SingleCriteria<ItemDuplicateGroup, int>(crit.groupId));
                    bool ToSave = false;
                    foreach (int mId in crit.itemDuplicateIds)
                    {
                        ItemDuplicateGroupMember item = IDG.Members.Find(found => found.ItemDuplicateId == mId);
                        if (item != null)
                        {
                            item.IsDuplicate = crit.isDuplicate;
                            item.IsChecked = true;
                            ToSave = true;
                            //item = item.Save();
                        }
                        else
                        {
                            return StatusCode(500, "Error: did not find member in this group");
                        }
                    }
                    if (ToSave)
                    {
                        IDG = IDG.Save();
                        return Ok();
                    }
                    else
                    {
                        return StatusCode(500, "Error: did not find member in this group");
                    }
                }
                else
                {
                    return Forbid();
                }
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Error in MarkUnmarkMemberAsDuplicate. GroupId = " + crit.groupId + " memberID: " + crit.itemDuplicateIds);
                return StatusCode(500, e.Message);
            }
        }
        [HttpPost("[action]")]
        public IActionResult MarkMemberAsMaster([FromBody] MarkUnmarkItemAsDuplicate crit)
        {
            try
            {
                if (SetCSLAUser4Writing())
                {
                    if (crit.itemDuplicateIds == null || crit.itemDuplicateIds.Length != 1)
                    {
                        return StatusCode(500, "Error: malformed request.");
                    }
                    DataPortal<ItemDuplicateGroup> dp = new DataPortal<ItemDuplicateGroup>();
                    ItemDuplicateGroup IDG = dp.Fetch(new SingleCriteria<ItemDuplicateGroup, int>(crit.groupId));

                    ItemDuplicateGroupMember item = IDG.Members.Find(found => found.ItemDuplicateId == crit.itemDuplicateIds[0]);
                    long originalMaster = IDG.getMaster().ItemId;
                    Csla.Core.MobileList<ItemDuplicateGroupMember> list = IDG.Members;
                    foreach (ItemDuplicateGroupMember gm in list)
                    {
                        if (gm.ItemId == originalMaster)
                        {
                            gm.IsMaster = false;
                            gm.IsChecked = false;
                            gm.IsDuplicate = false;
                        }
                        else if (gm.ItemId == item.ItemId)
                        {
                            gm.IsMaster = true;
                            gm.IsChecked = true;
                            gm.IsDuplicate = false;
                        }
                    }
                    IDG.Members = null;
                    IDG.Members = list;
                    IDG = IDG.Save();
                    return Ok();
                }
                else
                {
                    return Forbid();
                }
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Error in MarkMemberAsMaster. GroupId = " + crit.groupId + " memberID: " + crit.itemDuplicateIds);
                return StatusCode(500, e.Message);
            }
        }
        [HttpPost("[action]")]
        public IActionResult RemoveManualMember([FromBody] ManualMember crit)
        {
            try
            {
                if (SetCSLAUser4Writing())
                {
                    DataPortal<ItemDuplicateGroup> dp = new DataPortal<ItemDuplicateGroup>();
                    ItemDuplicateGroup IDG = dp.Fetch(new SingleCriteria<ItemDuplicateGroup, int>(crit.groupId));
                    IDG.RemoveItemID = crit.itemId;
                    IDG = IDG.Save();
                    return Ok();
                }
                else
                {
                    return Forbid();
                }
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Error in RemoveManualMember. GroupId = " + crit.groupId + " itemId: " + crit.itemId);
                return StatusCode(500, e.Message);
            }
        }
    }

	public class MarkUnmarkItemAsDuplicate
    {
		public int groupId { get; set; }
		public int[] itemDuplicateIds { get; set; }
		public bool isDuplicate { get; set; }
	}
    public class ManualMember
    {
        public int groupId { get; set; }
        public long itemId { get; set; }
    }
}
