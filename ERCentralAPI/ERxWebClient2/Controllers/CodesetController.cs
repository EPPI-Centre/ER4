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
    }
    public class AttributeMoveCommandJSON
    {
        public Int64 FromId;
        public Int64 ToId;
        public Int64 AttributeSetId;
        public int attributeOrder;
    }
}
