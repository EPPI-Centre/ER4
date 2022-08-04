using System;
using System.Linq;
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
                if (crit.Value)// write rights are required to "get new duplicates", not needed otherwise...
                {
                    if (!SetCSLAUser4Writing()) return Forbid();
                }
                else
                {
                    if (!SetCSLAUser()) return Unauthorized();
                }

                //for testing the protection against concurrenty, comment out the next two lines, uncomment the block below.
                DataPortal<ItemDuplicateReadOnlyGroupList> dp = new DataPortal<ItemDuplicateReadOnlyGroupList>();
                ItemDuplicateReadOnlyGroupList result = dp.Fetch(new SingleCriteria<ItemDuplicateReadOnlyGroupList, bool>(crit.Value));

                //START of block of code that can be used to test protection against concurrenty
                //---------------------
                //ItemDuplicateReadOnlyGroupList result = null;
                ////change/increase the last number below to produce more aggressive "concurrency testing".
                //int instances = crit.Value == false ? 1 : 5;//for testing, we'll start 5 instances of "get new duplicates" if API call asks for "get new duplicates"...
                //Parallel.For(0, instances, index =>
                //{
                //    SetCSLAUser4Writing();
                //    DataPortal<ItemDuplicateReadOnlyGroupList> dp = new DataPortal<ItemDuplicateReadOnlyGroupList>();
                //    result = dp.Fetch(new SingleCriteria<ItemDuplicateReadOnlyGroupList, bool>(crit.Value));
                //});
                //---------------------
                //END of block of code that can be used to test protection against concurrenty

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
                        return StatusCode(400, "Error: malformed request.");
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
                        return StatusCode(400, "Error: malformed request.");
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
                        return StatusCode(400, "Error: malformed request.");
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
                        return StatusCode(400, "Error: malformed request.");
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
        [HttpPost("[action]")]
        public IActionResult CreateNewGroup([FromBody] IncomingDirtyGroupMemberMVC[] members)
        {
            try
            {
                if (SetCSLAUser4Writing())
                {
                    if (members == null || members.Length < 2)
                    {
                        return StatusCode(400, "Error: malformed request.");
                    }
                    string Ids = "";
                    IncomingDirtyGroupMemberMVC master = new IncomingDirtyGroupMemberMVC() { isMaster = true, itemId = -1 };
                    foreach (IncomingDirtyGroupMemberMVC chk in members)
                    {
                        if (chk.itemId < 1)
                        {
                            return StatusCode(400, "Error: malformed request.");
                        }
                        Ids += chk.itemId.ToString() + ",";
                        if (chk.isMaster == true) master = chk;
                    }
                    Ids = Ids.TrimEnd(',');
                    ItemDuplicateDirtyGroup grp = DataPortal.Fetch<ItemDuplicateDirtyGroup>(new SingleCriteria<ItemDuplicateDirtyGroup, string>(Ids));
                    //we check that all is good and we set the current master if needed
                    if (
                        grp.Members.Count > 1 && master.itemId > 0 //enough members and the input gave us a master
                            &&
                            (
                                master.itemId == grp.getMaster().ItemId //master is the default one from the DB
                                || //OR
                                grp.setMaster(master.itemId) //setting the new master succeeded
                            )
                            && grp.IsUsable //group can be created
                        )
                    {//all is good, we can do this!
                        grp = grp.Save();
                        //we'll now fetch the new group, hoping it exists...
                        GroupListSelectionCriteria crit = new GroupListSelectionCriteria(0, master.itemId.ToString());
                        ItemDuplicateReadOnlyGroupList result = DataPortal.Fetch<ItemDuplicateReadOnlyGroupList>(crit);
                        if (result.Count < 1)
                        {
                            //not good, could not find any group containing the current master!?!?!
                            _logger.LogError("Error in CreateNewGroup: attempt to create new group failed", members);
                            return StatusCode(500, "Error: attempt to create new group failed");
                        }
                        else
                        {//result could contain more than one group, in which case, the client will find the new one and add only that to the local list.
                            return Ok(result);
                        }
                    }
                    else 
                    {
                        return StatusCode(400, "Error: malformed request.");
                    }
                }
                else
                {
                    return Forbid();
                }
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Error in CreateNewGroup");
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

    public class IncomingDirtyGroupMemberMVC
    {
        public long itemId { get; set; }
        public bool isMaster { get; set; }
    }
}
