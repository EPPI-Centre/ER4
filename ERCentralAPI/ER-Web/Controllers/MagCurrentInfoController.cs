using BusinessLibrary.BusinessClasses;
using BusinessLibrary.Security;
using Csla;
using EPPIDataServices.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Logging;
using System;

namespace ERxWebClient2.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class MagCurrentInfoController : CSLAController
    {

        public MagCurrentInfoController(ILogger<MagCurrentInfoController> logger) : base(logger)
        { }

        [HttpGet("[action]")]
        public ActionResult<MagCurrentInfo> GetMagCurrentInfo()
        {
            try
            {
                if (!SetCSLAUser()) return Unauthorized();

                DataPortal<MagCurrentInfo> dp = new DataPortal<MagCurrentInfo>();
                MagCurrentInfo result = dp.Fetch();

                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Getting a MagCurrentInfo has an error");
                return StatusCode(500, e.Message);
            }
        }

        [HttpPost("[action]")]
        public IActionResult UpdateMagCurrentInfo([FromBody] MVCMagCurrentInfo magCurrentInfo)
        {
            try
            {

                if (SetCSLAUser4Writing() && User.HasClaim(cl => cl.Type == "isSiteAdmin" && cl.Value == "True"))
                {
                    ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;

                    //MagCurrentInfo.UpdateMagCurrentInfoStatic();
                    DataPortal<MagCurrentInfo> dp = new DataPortal<MagCurrentInfo>();

                    // get data from the user
                    var magSQLCurrentInfo = MagCurrentInfo.GetMagCurrentInfoServerSide("Live");
                    if (magSQLCurrentInfo.MagFolder == "")
                    {
                        //insert from client
                        MagCurrentInfo newMagCurrentInfo = new MagCurrentInfo();
                        newMagCurrentInfo.WhenLive = DateTime.Now;
                        newMagCurrentInfo.MatchingAvailable = true;
                        newMagCurrentInfo.MakesDeploymentStatus = "LIVE";
                        newMagCurrentInfo.MagFolder = magCurrentInfo.magVersion;
                        newMagCurrentInfo.MakesEndPoint = magCurrentInfo.makesEndPoint;

                        newMagCurrentInfo = dp.Execute(newMagCurrentInfo);

                        return Ok(newMagCurrentInfo);

                    }
                    else
                    {
                        //update from client
                        MagCurrentInfo.UpdateSQLMagCurrentInfoTable(magCurrentInfo.magVersion, magCurrentInfo.makesEndPoint);

                        return Ok(magSQLCurrentInfo);

                    }
                }
                else return Forbid();

            }
            catch (Exception e)
            {
                _logger.LogException(e, "updating the  MagCurrent Info has an error");
                return StatusCode(500, e.Message);
            }
        }
        [HttpPost("[action]")]
        public IActionResult UpdateThisMagCurrentInfo([FromBody] MVCMagCurrentInfo magCurrentInfo)
        {
            try
            {
                if (SetCSLAUser4Writing() && User.HasClaim(cl => cl.Type == "isSiteAdmin" && cl.Value == "True"))
                {
                    ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                    MagCurrentInfo mci = new MagCurrentInfo();
                    mci.MagCurrentInfoId = magCurrentInfo.magCurrentInfoId;
                    mci.WhenLive = magCurrentInfo.whenLive;
                    mci.MakesDeploymentStatus = magCurrentInfo.makesDeploymentStatus;
                    mci.MagOnline = magCurrentInfo.magOnline;
                    

                    //MagCurrentInfo.UpdateMagCurrentInfoStatic();
                    DataPortal<MagCurrentInfo> dp = new DataPortal<MagCurrentInfo>();
                    MagCurrentInfo updated = dp.Update(mci);

                    MVCMagCurrentInfo response = new MVCMagCurrentInfo
                    {
                        magCurrentInfoId = updated.MagCurrentInfoId,
                        magFolder = updated.MagFolder,
                        magVersion = updated.MagFolder,
                        whenLive = updated.WhenLive,
                        matchingAvailable = updated.MatchingAvailable,
                        magOnline = updated.MagOnline,
                        makesEndPoint = updated.MakesEndPoint,
                        makesDeploymentStatus = updated.MakesDeploymentStatus
                    };

                    return Ok(response);
                }
                else return Forbid();

            }
            catch (Exception e)
            {
                _logger.LogException(e, "updating the  MagCurrent Info has an error");
                return StatusCode(500, e.Message);
            }
        }
        [HttpPost("[action]")]
        public IActionResult DeleteMagCurrentInfo([FromBody] SingleIntCriteria magCurrentInfoId)
        {
            try
            {
                if (SetCSLAUser4Writing() && User.HasClaim(cl => cl.Type == "isSiteAdmin" && cl.Value == "True"))
                {

                    DataPortal<MagCurrentInfoList> dp2 = new DataPortal<MagCurrentInfoList>();
                    var magCurrentInfoList = dp2.Fetch();

                    var mr = magCurrentInfoList.Where(x => x.MagCurrentInfoId == magCurrentInfoId.Value).FirstOrDefault();
                    if (mr != null)
                    {
                        magCurrentInfoList.Remove(mr);

                    }
                    return Ok();
                }

                else return Forbid();
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Deleting a review from a Mag Review List has an error");
                throw;
            }
        }


        [HttpGet("[action]")]
        public IActionResult GetMagReviewMagInfo()
        {
            try
            {
                if (!SetCSLAUser()) return Unauthorized();
                MagReviewMagInfoCommand cmd = new MagReviewMagInfoCommand();
                DataPortal<MagReviewMagInfoCommand> dp = new DataPortal<MagReviewMagInfoCommand>();
                cmd = dp.Execute(cmd);

                return Ok(cmd);
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Getting a MagReviewMagInfo Command has an error");
                return StatusCode(500, e.Message);
            }
        }

        [HttpGet("[action]")]
        public IActionResult GetMagCurrentInfoList()
        {
            try
            {
                if (!SetCSLAUser()) return Unauthorized();
                MagCurrentInfoList mco = new MagCurrentInfoList();
                DataPortal<MagCurrentInfoList> dp = new DataPortal<MagCurrentInfoList>();
                mco = dp.Fetch();

                return Ok(mco);
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Getting a MagReviewMagInfo get has an error");
                return StatusCode(500, e.Message);
            }
        }

        //move the following two to their own controller

        [EnableRateLimiting("HighCostEndpoints")]
        [HttpGet("[action]")]
        public IActionResult MagCheckContReviewRunningCommand()
        {
            try
            {
                if (SetCSLAUser4Writing() && User.HasClaim(cl => cl.Type == "isSiteAdmin" && cl.Value == "True"))
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
                return StatusCode(500, e.Message);
            }
        }


        [EnableRateLimiting("MaxCostEndpoints")]
        [HttpPost("[action]")]
        public IActionResult DoRunContReviewPipeline([FromBody] MVCContReviewPipeLineCommand pipelineParams)
        {
            try
            {
                if (SetCSLAUser4Writing() && User.HasClaim(cl => cl.Type == "isSiteAdmin" && cl.Value == "True"))
                {
                    ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                    DataPortal<MagContReviewPipelineRunCommand> dp2 = new DataPortal<MagContReviewPipelineRunCommand>();
                    MagContReviewPipelineRunCommand runPipelineCommand =
                        new MagContReviewPipelineRunCommand(
                            pipelineParams.previousVersion,
                            pipelineParams.currentVersion,
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
                return StatusCode(500, e.Message);
            }
        }


        //[HttpPost("[action]")]
        //public IActionResult DoCheckChangedPaperIds([FromBody] SingleStringCriteria latestMag)
        //{
        //    try
        //    {
        //        if (SetCSLAUser4Writing() && User.HasClaim(cl => cl.Type == "isSiteAdmin" && cl.Value == "True"))
        //        {
        //            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
        //            DataPortal<MagCheckPaperIdChangesCommand> dp = new DataPortal<MagCheckPaperIdChangesCommand>();
        //            MagCheckPaperIdChangesCommand magCheck = new MagCheckPaperIdChangesCommand(latestMag.Value);


        //            magCheck = dp.Execute(magCheck);

        //            return Ok(magCheck.LatestMAGName);
        //        }
        //        else return Forbid();
        //    }
        //    catch (Exception e)
        //    {
        //        _logger.LogException(e, "DoCheckChangedPaperIds has an error");
        //        return StatusCode(500, e.Message);
        //    }
        //}


        //[HttpGet("[action]")]
        //public IActionResult GetMAGBlobCommand()
        //{
        //    try
        //    {
        //        if (SetCSLAUser4Writing() && User.HasClaim(cl => cl.Type == "isSiteAdmin" && cl.Value == "True"))
        //        {

        //            DataPortal<MagBlobDataCommand> dp = new DataPortal<MagBlobDataCommand>();
        //            MagBlobDataCommand command = new MagBlobDataCommand();


        //            command = dp.Execute(command);

        //            return Ok(command);
        //        }
        //        else return Forbid();
        //    }
        //    catch (Exception e)
        //    {
        //        _logger.LogException(e, "Getting a MAGBlobCommand has an error");
        //        return StatusCode(500, e.Message);
        //    }
        //}
    }
}

public class MVCMagCurrentInfo
{
    public int magCurrentInfoId { get; set; }
    public string magFolder { get; set; }
    public string magVersion { get; set; }
    public DateTime whenLive { get; set; }
    public bool matchingAvailable { get; set; }
    public bool magOnline { get; set; }
    public string makesEndPoint { get; set; }
    public string makesDeploymentStatus { get; set; }

}

public class MVCContReviewPipeLineCommand
{
    public string previousVersion { get; set; }
    public string currentVersion { get; set; }
    public double editScoreThreshold { get; set; }
    internal double editFoSThreshold { get; set; }
    public string specificFolder { get; set; }
    public int magLogId { get; set; }
    public int editReviewSampleSize { get; set; }
}

