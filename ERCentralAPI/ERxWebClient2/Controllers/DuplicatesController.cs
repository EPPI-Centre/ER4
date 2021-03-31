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

namespace ERxWebClient2.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class DuplicatesController : CSLAController
    {
        
        public DuplicatesController(ILogger<ReviewerTermListController> logger) : base(logger)
        { }

        [HttpPost("[action]")]
        public IActionResult FetchGroups([FromBody] SingleBoolCriteria crit)
        {
            try
            {
                if (!SetCSLAUser()) return Unauthorized();
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
		public IActionResult FetchGroupsWithCriteria([FromBody] GroupListSelectionCriteriaMVC data)
		{
			try
			{
                if (!SetCSLAUser()) return Unauthorized();
                GroupListSelectionCriteria crit = new GroupListSelectionCriteria(data.groupId, data.itemIds);
                ItemDuplicateReadOnlyGroupList result = DataPortal.Fetch<ItemDuplicateReadOnlyGroupList>(crit);
				return Ok(result);
			}
			catch (Exception e)
			{
				_logger.LogException(e, "Error in FetchGroupsWithCriteria: groupId = " + data.groupId + ", itemIds = " + data.itemIds);
                return StatusCode(500, e.Message);
			}
		}

        [HttpPost("[action]")]
        public IActionResult FetchGroupDetails([FromBody] SingleIntCriteria crit)
        {
            try
            {
                if (!SetCSLAUser()) return Unauthorized();
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
                        return Ok(IDG);
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
                    return Ok(IDG);
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

        [HttpPost("[action]")]
        public IActionResult AddManualMembers([FromBody] GroupListSelectionCriteriaMVC crit)
        {
            try
            {
                if (SetCSLAUser4Writing())
                {
                    DataPortal<ItemDuplicateGroup> dp = new DataPortal<ItemDuplicateGroup>();
                    ItemDuplicateGroup IDG = dp.Fetch(new SingleCriteria<ItemDuplicateGroup, int>(crit.groupId));
                    long currmas = IDG.getMaster().ItemId;
                    IDG.AddItems = new Csla.Core.MobileList<long>();
                    string[] list = crit.itemIds.Split(',');
                    foreach (string it in list)
                    {
                        Int64 ItemId;
                        if (Int64.TryParse(it, out ItemId))
                        {
                            if (ItemId != currmas && ItemId > 0) IDG.AddItems.Add(ItemId);
                        }
                    }
                    IDG = IDG.Save();
                    return Ok(IDG);
                }
                else
                {
                    return Forbid();
                }
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Error in AddManualMembers. GroupId = " + crit.groupId + " itemIds: " + crit.itemIds);
                return StatusCode(500, e.Message);
            }
        }


        [HttpPost("[action]")]
        public IActionResult DeleteGroup([FromBody] SingleIntCriteria crit)
        {
            try
            {
                if (SetCSLAUser4Writing())
                {
                    if (crit  == null || crit.Value < 1)
                    {
                        return StatusCode(500, "Error: malformed request.");
                    }
                    DataPortal<ItemDuplicateGroup> dp = new DataPortal<ItemDuplicateGroup>();
                    ItemDuplicateGroup IDG = dp.Fetch(new SingleCriteria<ItemDuplicateGroup, int>(crit.Value));
                    if (IDG == null || IDG.Members == null || IDG.Members.Count == 0)
                    {
                        return StatusCode(404, "Group Not Found.");
                    }
                    IDG.Delete();
                    IDG.ApplyEdit();
                    IDG = IDG.Save();
                    return Ok(crit.Value);
                }
                else
                {
                    return Forbid();
                }
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Error in DeleteGroup. GroupId = " + crit.Value);
                return StatusCode(500, e.Message);
            }
        }

        [HttpPost("[action]")]
        public IActionResult DeleteAllGroups([FromBody] SingleBoolCriteria crit)
        {
            try
            {
                if (SetCSLAUser4Writing())
                {
                    if (crit == null)
                    {
                        return StatusCode(500, "Error: malformed request.");
                    }
                    ItemDuplicateGroupsDeleteCommand command = new ItemDuplicateGroupsDeleteCommand(crit.Value);
                    command = DataPortal.Execute<ItemDuplicateGroupsDeleteCommand>(command);
                    return Ok();
                }
                else
                {
                    return Forbid();
                }
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Error in Delete All Groups. Hard Reset = " + crit.Value);
                return StatusCode(500, e.Message);
            }
        }
        [HttpPost("[action]")]
        public IActionResult FetchDirtyGroup([FromBody] SingleStringCriteria crit)
        {
            try
            {
                if (!SetCSLAUser()) return Unauthorized();

                //let's check what we have is sensible...
                string[] Ids = crit.Value.Split(',');
                foreach (string id in Ids)
                {
                    Int64 chk;
                    if (!Int64.TryParse(id, out chk) || chk < 1)
                    {
                        return StatusCode(400, "Bad request, input is malformed.");
                    }
                }
                DataPortal<ItemDuplicateDirtyGroup> dp = new DataPortal<ItemDuplicateDirtyGroup>();
                ItemDuplicateDirtyGroup result = dp.Fetch(new SingleCriteria<ItemDuplicateDirtyGroup, string>(crit.Value));
                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Error Fetching DirtyGroup. Crit = " + crit.Value);
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
    public class GroupListSelectionCriteriaMVC
    {
        public int groupId { get; set; }
        public string itemIds { get; set; }
    }
}
