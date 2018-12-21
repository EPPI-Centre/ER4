using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BusinessLibrary.BusinessClasses;
using BusinessLibrary.Security;
using Csla;
using ERxWebClient2.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using EPPIDataServices.Helpers;

namespace ERxWebClient2.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class CodesetController : CSLAController
    {

        private readonly ILogger _logger;
        
        public CodesetController(ILogger<CodesetController> logger)
        {
            _logger = logger;
        }

        [HttpGet("[action]")]
        public IActionResult CodesetsByReview()
        {

            try
            {
                SetCSLAUser();
                DataPortal<ReviewSetsList> dp = new DataPortal<ReviewSetsList>();
                ReviewSetsList res = dp.Fetch();
                return Ok(res);
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Dataportal error getting ReviewSets");
                throw;
            }
        }
        [HttpGet("[action]")]
        public IActionResult SetTypes()
        {
            try
            {
                SetCSLAUser();
                DataPortal<ReadOnlySetTypeList> dp = new DataPortal<ReadOnlySetTypeList>();
                ReadOnlySetTypeList res = dp.Fetch();
                return Ok(res);
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Dataportal error getting SetTypes");
                throw;
            }
        }
        [HttpPost("[action]")]
        public IActionResult SaveReviewSet([FromBody] ReviewSetUpdateCommandJSON data)
        {
            try
            {
                if (SetCSLAUser4Writing())
                {
                    ReviewSetUpdateCommand cmd = new ReviewSetUpdateCommand(data.ReviewSetId
                        , data.SetId
                        , data.AllowCodingEdits
                        , data.CodingIsFinal
                        , data.SetName
                        , data.setOrder
                        , data.setDescription);
                    DataPortal<ReviewSetUpdateCommand> dp = new DataPortal<ReviewSetUpdateCommand>();
                    cmd = dp.Execute(cmd);
                    return Ok(data);//if no error, all should be OK.
                }
                else  return Forbid();
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Dataportal error running ReviewSetUpdateCommand");
                throw;
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
                throw;
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
                    newCodeSet.CodingIsFinal = data.CodingIsFinal;
                    newCodeSet.AllowCodingEdits = true;
                    newCodeSet.ReviewId = ri.ReviewId;
                    newCodeSet.SetOrder = data.setOrder;
                    newCodeSet.SetName = data.SetName;
                    newCodeSet.SetTypeId = data.SetTypeId;
                    newCodeSet.SetDescription = data.setDescription;
                    DataPortal<ReadOnlySetTypeList> dp = new DataPortal<ReadOnlySetTypeList>();
                    ReadOnlySetTypeList res = dp.Fetch();
                    ReadOnlySetType rost = new ReadOnlySetType();
                    foreach (ReadOnlySetType rst in res)
                    {
                        if (rst.SetTypeId == data.SetTypeId)
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
                throw;
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
                throw;
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
                    return Ok(cmd.NumItems);
                }
                else return Forbid();
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Dataportal error running AttributeOrSetDeleteCheck");
                throw;
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
                throw;
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
                throw;
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
                        data.attributeOrder);
                    DataPortal<AttributeUpdateCommand> dp = new DataPortal<AttributeUpdateCommand>();
                    cmd = dp.Execute(cmd);
                    return Ok(true);//no point sending back anything, it worked...
                }
                else return Forbid();
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Dataportal error running AttributeUpdate");
                throw;
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
                throw;
            }
        }
    }

    public class ReviewSetUpdateCommandJSON
    {
        public int SetId;
        public int ReviewSetId;
        public string SetName;
        public int setOrder;
        public string setDescription;
        public bool CodingIsFinal;//normal or comparison mode
        public bool AllowCodingEdits; //AllowCodingEdits can edit this codeset...
        public int SetTypeId;
    }
    public class AttributeMoveCommandJSON
    {
        public Int64 FromId;
        public Int64 ToId;
        public Int64 AttributeSetId;
        public int attributeOrder;
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
    }
}
