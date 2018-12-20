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
    public class AttributeOrSetDeleteCheckCommandJSON
    {
        public Int64 attributeSetId;
        public int setId;
    }
}
