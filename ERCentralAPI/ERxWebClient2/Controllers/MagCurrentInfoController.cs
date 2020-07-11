using System;
using BusinessLibrary.BusinessClasses;
using BusinessLibrary.Security;
using Csla;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using EPPIDataServices.Helpers;
using System.Linq;

namespace ERxWebClient2.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class MagCurrentInfoController : CSLAController
    {

        private readonly ILogger _logger;

		public MagCurrentInfoController(ILogger<MagCurrentInfoController> logger)
        {

            _logger = logger;
        }

        [HttpGet("[action]")]
        public ActionResult<MagCurrentInfo> GetMagCurrentInfo()
        {
			try
            {
                SetCSLAUser();

                DataPortal<MagCurrentInfo> dp = new DataPortal<MagCurrentInfo>();
				MagCurrentInfo result = dp.Fetch();

                return result;
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Getting a MagCurrentInfo has an error");
                throw;
            }
		}

        [HttpGet("[action]")]
        public IActionResult UpdateMagCurrentInfo()
        {
            try
            {
                //Some issue here with the way that I am calling Update
                //it is not the correct way with Data_Portal constraints.
                SetCSLAUser();
                //var currentInfo = GetMagCurrentInfo();

                //MagCurrentInfo obj = currentInfo.Value;
                //DataPortal<MagCurrentInfo> dp = new DataPortal<MagCurrentInfo>();
                //obj = dp.Update(obj);
                return Ok();
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Getting a MagCurrentInfo has an error");
                throw;
            }
        }

        [HttpGet("[action]")]
        public IActionResult GetMagReviewMagInfo()
        {
            try
            {
                SetCSLAUser();
                MAgReviewMagInfoCommand cmd = new MAgReviewMagInfoCommand();
                DataPortal<MAgReviewMagInfoCommand> dp = new DataPortal<MAgReviewMagInfoCommand>();
                cmd = dp.Execute(cmd);

                return Ok(cmd);
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Getting a MagReviewMagInfo Command has an error");
                throw;
            }
        }

        [HttpGet("[action]")]
        public IActionResult MagCheckContReviewRunningCommand()
        {
            try
            {
                if (SetCSLAUser4Writing())
                {
                    DataPortal<MagCheckContReviewRunningCommand> dp = new DataPortal<MagCheckContReviewRunningCommand>();
                    MagCheckContReviewRunningCommand check = new MagCheckContReviewRunningCommand();

                    check = dp.Execute(check);

                    return Ok(check);

                }
                else return Forbid();
            }
            catch (Exception e)
            {
                _logger.LogException(e, "MagCheckContReviewRunningCommand has an error");
                throw;
            }
        }

        [HttpPost("[action]")]
        public IActionResult DoCheckChangedPaperIds([FromBody] SingleStringCriteria latestMag)
        {
            try
            {
                SetCSLAUser();

                DataPortal<MagCheckPaperIdChangesCommand> dp = new DataPortal<MagCheckPaperIdChangesCommand>();
                MagCheckPaperIdChangesCommand magCheck = new MagCheckPaperIdChangesCommand(latestMag.Value);


                magCheck = dp.Execute(magCheck);

                return Ok(magCheck.LatestMAGName);
            }
            catch (Exception e)
            {
                _logger.LogException(e, "DoCheckChangedPaperIds has an error");
                throw;
            }
        }


        [HttpGet("[action]")]
        public IActionResult GetMAGBlobCommand()
        {
            try
            {
                SetCSLAUser();

                DataPortal<MagBlobDataCommand> dp = new DataPortal<MagBlobDataCommand>();
                MagBlobDataCommand command = new MagBlobDataCommand();


                command = dp.Execute(command);

                return Ok(command);
            }
            catch (Exception e)
            {
                _logger.LogException(e, "GetMAGBlobCommand has an error");
                throw;
            }
        }




    }

  
}

