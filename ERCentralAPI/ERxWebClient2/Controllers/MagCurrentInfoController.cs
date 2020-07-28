using System;
using BusinessLibrary.BusinessClasses;
using BusinessLibrary.Security;
using Csla;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using EPPIDataServices.Helpers;
using System.Linq;
using Microsoft.Azure.Management.DataFactory.Models;

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

                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Getting a MagCurrentInfo has an error");
                throw;
            }
		}

        [HttpPost("[action]")]
        public IActionResult UpdateMagCurrentInfo([FromBody] MVCMagCurrentInfo magCurrentInfo)
        {
            try
            {
                SetCSLAUser();
                
                                
                //magSQLCurrentInfo.MagVersion = magCurrentInfo.magVersion;
                //magSQLCurrentInfo.MakesDeploymentStatus = magCurrentInfo.makesDeploymentStatus;
                //magSQLCurrentInfo.MakesEndPoint = magCurrentInfo.makesEndPoint;
                //magSQLCurrentInfo.MagFolder = magCurrentInfo.magFolder;
                //magSQLCurrentInfo.MagOnline = Convert.ToBoolean(magCurrentInfo.magOnline);
                //magSQLCurrentInfo.MatchingAvailable = Convert.ToBoolean(magCurrentInfo.matchingAvailable);

                MagCurrentInfo.UpdateMagCurrentInfoStatic();
                DataPortal<MagCurrentInfo> dp = new DataPortal<MagCurrentInfo>();
                var magSQLCurrentInfo = MagCurrentInfo.GetMagCurrentInfoServerSide("Live");

                //magSQLCurrentInfo = magSQLCurrentInfo.Save(true);

                return Ok(magSQLCurrentInfo);
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

public class MVCMagCurrentInfo
{
    public int magCurrentInfoId { get; set; }
    public string magFolder { get; set; }
    public string magVersion { get; set; }
    public string whenLive { get; set; }
    public string matchingAvailable { get; set; }
    public string magOnline { get; set; }
    public string makesEndPoint { get; set; }
    public string makesDeploymentStatus { get; set; }

}

