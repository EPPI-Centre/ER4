using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BusinessLibrary.BusinessClasses;
using BusinessLibrary.Security;
using Csla;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EPPIDataServices.Helpers;

namespace ERxWebClient2.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class CodesetController : CSLAController
    {
        
        public CodesetController(ILogger<CodesetController> logger) : base(logger)
        {}

        [HttpGet("[action]")]
        public IActionResult CodesetsByReview()
        {

            try
            {
                if (!SetCSLAUser()) return Unauthorized();
                DataPortal<ReviewSetsList> dp = new DataPortal<ReviewSetsList>();
                ReviewSetsList res = dp.Fetch();
                return Ok(res);
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Dataportal error getting ReviewSets");
                return StatusCode(500, e.Message);
            }
        }
        [HttpGet("[action]")]
        public IActionResult SetTypes()
        {
            try
            {
                if (!SetCSLAUser()) return Unauthorized();
                DataPortal<ReadOnlySetTypeList> dp = new DataPortal<ReadOnlySetTypeList>();
                ReadOnlySetTypeList res = dp.Fetch();
                return Ok(res);
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Dataportal error getting SetTypes");
                return StatusCode(500, e.Message);
            }
        }
        [HttpPost("[action]")]
        public IActionResult SaveReviewSet([FromBody] ReviewSetUpdateCommandJSON data)
        {
            try
            {
                if (SetCSLAUser4Writing())
                {
                    ReviewSetUpdateCommand cmd = new ReviewSetUpdateCommand(data.reviewSetId
                        , data.setId
                        , data.allowCodingEdits
                        , data.codingIsFinal
                        , data.setName
                        , data.setOrder
                        , data.setDescription
                        , data.usersCanEditURLs);//i.e. no way to make sets accept URLs for codes in ERx, until we implement this...
                    DataPortal<ReviewSetUpdateCommand> dp = new DataPortal<ReviewSetUpdateCommand>();
                    cmd = dp.Execute(cmd);
                    return Ok(data);//if no error, all should be OK.
                }
                else  return Forbid();
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Dataportal error running ReviewSetUpdateCommand");
                return StatusCode(500, e.Message);
            }
        }

        [HttpPost("[action]")]
        public IActionResult AttributeSetMove([FromBody] AttributeMoveCommandJSON data)
        {
            try
            {
                if (SetCSLAUser4Writing())
                {
                    AttributeMoveCommand cmd = new AttributeMoveCommand(data.FromId, data.ToId, data.AttributeSetId, data.attributeOrder);
                    DataPortal<AttributeMoveCommand> dp = new DataPortal<AttributeMoveCommand>();
                    cmd = dp.Execute(cmd);
                    return Ok(data);//if no error, all should be OK.
                }
                else return Forbid();
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Dataportal error running ReviewSetUpdateCommand");
                return StatusCode(500, e.Message);
            }
        }

        [HttpPost("[action]")]
        public IActionResult ReviewSetMove([FromBody] ReviewSetMoveCommandJSON data)
        {
            try
            {
                if (SetCSLAUser4Writing())
                {
                    ReviewSetMoveCommand cmd = new ReviewSetMoveCommand(data.ReviewSetId, data.ReviewSetOrder);
                    DataPortal<ReviewSetMoveCommand> dp = new DataPortal<ReviewSetMoveCommand>();
                    cmd = dp.Execute(cmd);
                    return Ok(data);//if no error, all should be OK.
                }
                else return Forbid();
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Dataportal error running ReviewSetUpdateCommand");
                return StatusCode(500, e.Message);
            }
        }

        [HttpPost("[action]")]
        public IActionResult ReviewSetCreate([FromBody] ReviewSetUpdateCommandJSON data)
        {//we use the ReviewSetUpdateCommandJSON object because it contains all the data we need.
            try
            {
                if (SetCSLAUser4Writing())
                {
                    ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                    ReviewSet newCodeSet = new ReviewSet();
                    newCodeSet.Attributes = new AttributeSetList();
                    newCodeSet.CodingIsFinal = data.codingIsFinal;
                    newCodeSet.AllowCodingEdits = true;
                    newCodeSet.ReviewId = ri.ReviewId;
                    newCodeSet.SetOrder = data.setOrder;
                    newCodeSet.SetName = data.setName;
                    newCodeSet.SetTypeId = data.setTypeId;
                    newCodeSet.SetDescription = data.setDescription;
                    DataPortal<ReadOnlySetTypeList> dp = new DataPortal<ReadOnlySetTypeList>();
                    ReadOnlySetTypeList res = dp.Fetch();
                    ReadOnlySetType rost = new ReadOnlySetType();
                    foreach (ReadOnlySetType rst in res)
                    {
                        if (rst.SetTypeId == data.setTypeId)
                        {
                            rost = rst;
                            break;
                        }
                    }
                    newCodeSet.SetType = rost;
                    newCodeSet = newCodeSet.Save();
                    return Ok(newCodeSet);//will be used on client side!
                }
                else return Forbid();
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Dataportal error running ReviewSetCreate");
                return StatusCode(500, e.Message);
            }
        }
        [HttpPost("[action]")]
        public IActionResult ReviewSetCheckCodingStatus([FromBody] SingleIntCriteria crit)
        {//used before moving from Comparison to Normal Data Entry
            try
            {
                if (SetCSLAUser4Writing())
                {
                    ReviewSetCheckCodingStatusCommand cmd = new ReviewSetCheckCodingStatusCommand(crit.Value);
                    DataPortal<ReviewSetCheckCodingStatusCommand> dp = new DataPortal<ReviewSetCheckCodingStatusCommand>();
                    cmd = dp.Execute(cmd);
                    return Ok(cmd.ProblematicItemCount);
                }
                else return Forbid();
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Dataportal error running ReviewSetCheckCodingStatus");
                return StatusCode(500, e.Message);
            }
        }
        [HttpPost("[action]")]
        public IActionResult AttributeOrSetDeleteCheck([FromBody] AttributeOrSetDeleteCheckCommandJSON data)
        {//used before deleting a code/attribute or a set
            try
            {
                if (SetCSLAUser4Writing())
                {
                    AttributeSetDeleteWarningCommand cmd = new AttributeSetDeleteWarningCommand(data.attributeSetId, data.setId);
                    DataPortal<AttributeSetDeleteWarningCommand> dp = new DataPortal<AttributeSetDeleteWarningCommand>();
                    cmd = dp.Execute(cmd);
                    AttributeSetDeleteWarningCommandResult result = new AttributeSetDeleteWarningCommandResult();
                    result.NumAllocations = cmd.NumAllocations;
                    result.NumItems = cmd.NumItems;
                    return Ok(result);
                }
                else return Forbid();
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Dataportal error running AttributeOrSetDeleteCheck");
                return StatusCode(500, e.Message);
            }
        }
        [HttpPost("[action]")]
        public IActionResult ReviewSetDelete([FromBody] ReviewSetDeleteCommandJSON jsonCMD)
        {
            try
            {
                if (SetCSLAUser4Writing())
                {
                    ReviewSetDeleteCommand cmd = new ReviewSetDeleteCommand(jsonCMD.reviewSetId, jsonCMD.setId, jsonCMD.order);
                    DataPortal<ReviewSetDeleteCommand> dp = new DataPortal<ReviewSetDeleteCommand>();
                    cmd = dp.Execute(cmd);
                    return Ok(cmd);
                }
                else return Forbid();
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Dataportal error running ReviewSetDelete");
                return StatusCode(500, e.Message);
            }
        }
        [HttpPost("[action]")]
        public IActionResult AttributeCreate([FromBody] AttributeSetCreateOrUpdateJSON data)
        {//we use the ReviewSetUpdateCommandJSON object because it contains all the data we need.
            try
            {
                if (SetCSLAUser4Writing())
                {
                    ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                    AttributeSet newCode = new AttributeSet();
                    newCode.Attributes = new AttributeSetList();
                    newCode.AttributeDescription = "";//????
                    newCode.AttributeId = data.attributeId;
                    newCode.AttributeName = data.attributeName;
                    newCode.AttributeOrder = data.attributeOrder;
                    newCode.AttributeSetDescription = data.attributeSetDescription;
                    //newCode.AttributeSetId = data.attributeSetId;
                    newCode.AttributeTypeId = data.attributeTypeId;
                    newCode.ContactId = data.contactId;
                    newCode.OriginalAttributeID = data.originalAttributeID;
                    newCode.ParentAttributeId = data.parentAttributeId;
                    newCode.SetId = data.setId;
                    newCode = newCode.Save();
                    return Ok(newCode);//will be used on client side!
                }
                else return Forbid();
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Dataportal error running AttributeCreate");
                return StatusCode(500, e.Message);
            }
        }
        [HttpPost("[action]")]
        public IActionResult AttributeUpdate([FromBody] AttributeSetCreateOrUpdateJSON data)
        {//we use the ReviewSetUpdateCommandJSON object because it contains all the data we need.
            try
            {
                if (SetCSLAUser4Writing())
                {//GetReviewSet(int criteria)
                    //ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                    AttributeUpdateCommand cmd = new AttributeUpdateCommand(data.attributeId, 
                        data.attributeSetId, 
                        data.attributeTypeId, 
                        data.attributeName, 
                        data.attributeSetDescription, 
                        data.attributeOrder,
                        data.extURL,
                        data.extType);
                    DataPortal<AttributeUpdateCommand> dp = new DataPortal<AttributeUpdateCommand>();
                    cmd = dp.Execute(cmd);
                    return Ok(true);//no point sending back anything, it worked...
                }
                else return Forbid();
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Dataportal error running AttributeUpdate");
                return StatusCode(500, e.Message);
            }
        }
        [HttpPost("[action]")]
        public IActionResult AttributeDelete([FromBody] AttributeDeleteCommandJSON jsonCMD)
        {
            try
            {
                if (SetCSLAUser4Writing())
                {
                    AttributeSetDeleteCommand cmd = new AttributeSetDeleteCommand(jsonCMD.attributeSetId, jsonCMD.parentAttributeId, jsonCMD.attributeId, jsonCMD.attributeOrder);
                    DataPortal<AttributeSetDeleteCommand> dp = new DataPortal<AttributeSetDeleteCommand>();
                    cmd = dp.Execute(cmd);
                    return Ok(cmd);
                }
                else return Forbid();
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Dataportal error running AttributeDelete");
                return StatusCode(500, e.Message);
            }
        }
        [HttpPost("[action]")]
        public IActionResult ReviewSetCopy([FromBody] ReviewSetCopyCommandJSON data)
        {//we use the ReviewSetUpdateCommandJSON object because it contains all the data we need.
            try
            {
                if (SetCSLAUser4Writing())
                {
                    ReviewSetCopyCommand res = new ReviewSetCopyCommand(data.reviewSetId, data.order);
                    DataPortal<ReviewSetCopyCommand> dp = new DataPortal<ReviewSetCopyCommand>();
                    res = dp.Execute(res);
                    return Ok(data);//nothing new to send back, command worked.
                }
                else return Forbid();
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Dataportal error running ReviewSetCreate");
                return StatusCode(500, e.Message);
            }
        }
		[HttpPost("[action]")]
		public IActionResult CreateVisualiseCodeSet([FromBody] ClassifierCreateCodes data)
		{
			try
			{
				if (SetCSLAUser4Writing())
				{
					//SearchDeleteCommand cmd = new SearchVisualise(_searchId);
					DataPortal<ClassifierCreateCodesCommand> dp = new DataPortal<ClassifierCreateCodesCommand>();
					ClassifierCreateCodesCommand command = new ClassifierCreateCodesCommand
																(data.searchId,
																data.searchName,
																data.attributeId = data.attributeId,
																data.setId = data.setId);
					dp.Execute(command);
					return Ok();
				}
				else return Forbid();
			}
			catch (Exception e)
			{
				_logger.LogException(e, "CreateVisualiseCodeSet of Search Data has failed");
				return StatusCode(500, e.Message);
			}
		}
		[HttpPost("[action]")]
        public IActionResult GetReviewSetsForCopying([FromBody] SingleBoolCriteria GetPrivateSets)
        {//we use the ReviewSetUpdateCommandJSON object because it contains all the data we need.
            try
            {
                if (SetCSLAUser4Writing())//not strictly necessary, but why give this away to readonly users?
                {
                    DataPortal<ReviewSetsList> dp = new DataPortal<ReviewSetsList>();
                    ReviewSetsList data = dp.Fetch(new SingleCriteria<ReviewSetsList, bool>(GetPrivateSets.Value));
                    return Ok(data);//nothing new to send back, command worked.
                }
                else return Forbid();
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Dataportal error running GetReviewSetsForCopying");
                return StatusCode(500, e.Message);
            }
        }
        [HttpPost("[action]")]
        public IActionResult PerformClusterCommand([FromBody] PerformClusterCommandJSON JsonCmd)
        {
            try
            {
                if (SetCSLAUser4Writing())//not strictly necessary, but why give this away to readonly users?
                {
                    PerformClusterCommand cmd = new PerformClusterCommand(JsonCmd.itemList,
                                                                        JsonCmd.maxHierarchyDepth,
                                                                        JsonCmd.minClusterSize,
                                                                        JsonCmd.maxClusterSize,
                                                                        JsonCmd.singleWordLabelWeight,
                                                                        JsonCmd.minLabelLength,
                                                                        JsonCmd.attributeSetList,
                                                                        JsonCmd.useUploadedDocs,
                                                                        JsonCmd.reviewSetIndex);
                    DataPortal <PerformClusterCommand> dp = new DataPortal<PerformClusterCommand>();
                    cmd = dp.Execute(cmd);
                    return Ok();
                }
                else return Forbid();
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Dataportal error running PerformClusterCommand");
                return StatusCode(500, e.Message);
            }
        }
		[HttpPost("[action]")]
		public IActionResult PerformRandomAllocate([FromBody] PerformRandomAllocateCommandJSON data)
		{
			try
			{
				if (SetCSLAUser4Writing())
				{
					PerformRandomAllocateCommand cmd = new PerformRandomAllocateCommand(
						data.filterType,
						data.attributeIdFilter,
						data.setIdFilter,
						data.attributeId,
						data.setId,
						data.howMany,
						data.numericRandomSample,
						data.randomSampleIncluded);
					DataPortal<PerformRandomAllocateCommand> dp = new DataPortal<PerformRandomAllocateCommand>();
					cmd = dp.Execute(cmd);
					return Ok(cmd);
				}
				else return Forbid();
			}
			catch (Exception e)
			{
				_logger.LogException(e, "PerformRandomAllocate error");
				return StatusCode(500, e.Message);
			}
		}

	}

    public class ReviewSetUpdateCommandJSON
    {
        public int setId;
        public int reviewSetId;
        public string setName = "";
        public int setOrder;
        public string setDescription = "";
        public bool codingIsFinal;//normal or comparison mode
        public bool allowCodingEdits; //AllowCodingEdits can edit this codeset...
        public int setTypeId;
        public bool usersCanEditURLs;
    }
    public class AttributeMoveCommandJSON
    {
        public Int64 FromId;
        public Int64 ToId;
        public Int64 AttributeSetId;
        public int attributeOrder;
    }
    public class ReviewSetMoveCommandJSON
    {
        public Int64 ReviewSetId;
        public int ReviewSetOrder;
    }
    public class ReviewSetDeleteCommandJSON
    {
        public Int64 reviewSetId;
        public bool successful;
        public int setId;
        public int order;
    }
    public class AttributeDeleteCommandJSON
    {
        public Int64 attributeSetId;
        public Int64 attributeId;
        public Int64 parentAttributeId;
        public int attributeOrder;
        public bool successful;
    }
    public class AttributeOrSetDeleteCheckCommandJSON
    {
        public Int64 attributeSetId;
        public int setId;
    }
    public class AttributeSetDeleteWarningCommandResult
    {
        public Int64 NumItems;
        public int NumAllocations;
    }
    public class AttributeSetCreateOrUpdateJSON
    {
        public int setId;
        public Int64 parentAttributeId;
        public int attributeTypeId;
        //AttributeSetDescription
        public int attributeOrder;
        public string attributeName;
        public string attributeSetDescription;
        public int contactId;
        public Int64 originalAttributeID;
        //two fields used as input only when updating
        public Int64 attributeSetId;
        public Int64 attributeId;
        public string extURL;
        public string extType;
    }
    public class ReviewSetCopyCommandJSON
    {
        public int reviewSetId;
        public int order;
    }
    public class PerformClusterCommandJSON
    {
        public string itemList;
        public string attributeSetList;
        public int maxHierarchyDepth;
        public double minClusterSize;
        public double maxClusterSize;
        public double singleWordLabelWeight;
        public int minLabelLength;
        public bool useUploadedDocs;
        public int reviewSetIndex;
    }
	public class ClassifierCreateCodes
	{
		public string searchName { get; set; }
		public int searchId { get; set; }
		public long attributeId { get; set; }
		public int setId { get; set; }
	}
	public class PerformRandomAllocateCommandJSON
	{
		public string filterType { get; set; }
		public long attributeIdFilter { get; set; }
		public int setIdFilter { get; set; }
		public long attributeId { get; set; }
		public int setId { get; set; }
		public int howMany { get; set; }
		public int numericRandomSample { get; set; }
		public bool randomSampleIncluded { get; set; }
	}
}
