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
                if (SetCSLAUser4Writing())
                {
                    ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                    // FOR NOW on the MagCurrentInfo business object we contact Azure and get the info to update the DB
                    // TODO change logic to receive this info from the user in the above MVC object
                    // after we have listed the info on the UI
                    MagCurrentInfo.UpdateMagCurrentInfoStatic();
                    DataPortal<MagCurrentInfo> dp = new DataPortal<MagCurrentInfo>();
                    var magSQLCurrentInfo = MagCurrentInfo.GetMagCurrentInfoServerSide("Live");

                    return Ok(magSQLCurrentInfo);

                }
                else return Forbid();
            }
            catch (Exception e)
            {
                _logger.LogException(e, "updating the  MagCurrent Info has an error");
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
                _logger.LogException(e, "Getting a MagReviewMag Info Command has an error");
                throw;
            }
        }

        //move the following two to their own controller
                
        [HttpGet("[action]")]
        public IActionResult MagCheckContReviewRunningCommand()
        {
            try
            {
                if (SetCSLAUser4Writing())
                {
                    ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                    DataPortal<MagCheckContReviewRunningCommand> dp = new DataPortal<MagCheckContReviewRunningCommand>();
                    MagCheckContReviewRunningCommand check = new MagCheckContReviewRunningCommand();

                    check = dp.Execute(check);

                    return Ok(check);

                }
                else return Forbid();
            }
            catch (Exception e)
            {
                _logger.LogException(e, "MagCheckContReviewRunning Command has an error");
                throw;
            }
        }


        [HttpPost("[action]")]
        public IActionResult DoRunContReviewPipeline([FromBody] MVCContReviewPipeLineCommand pipelineParams)
        {
            try
            {
                if (SetCSLAUser4Writing())
                {
                    ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                    DataPortal<MagContReviewPipelineRunCommand> dp2 = new DataPortal<MagContReviewPipelineRunCommand>();
                    MagContReviewPipelineRunCommand runPipelineCommand =
                        new MagContReviewPipelineRunCommand(
                            pipelineParams.previousVersion,
                            pipelineParams.magVersion,
                            pipelineParams.editScoreThreshold,
                            pipelineParams.editFoSThreshold,
                            pipelineParams.specificFolder,
                            pipelineParams.magLogId,
                            Convert.ToInt32(pipelineParams.editReviewSampleSize));

                    runPipelineCommand = dp2.Execute(runPipelineCommand);

                    return Ok(runPipelineCommand);

                }
                else return Forbid();
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Do Run ContReview Pipeline has an error");
                throw;
            }
        }


        //=====================================above two methods to their own controller TODO



        [HttpPost("[action]")]
        public IActionResult DoCheckChangedPaperIds([FromBody] SingleStringCriteria latestMag)
        {
            try
            {
                SetCSLAUser();
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
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
                _logger.LogException(e, "Getting a MAGBlobCommand has an error");
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

public class MVCContReviewPipeLineCommand
{
    public string previousVersion { get; set; }
    public string magVersion { get; set; }
    public double editScoreThreshold { get; set; }
    internal double editFoSThreshold { get; set; }
    public string specificFolder { get; set; }
    public int magLogId { get; set; }
    public int editReviewSampleSize { get; set; }
}

