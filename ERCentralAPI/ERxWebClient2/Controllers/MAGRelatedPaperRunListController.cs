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
    public class MagRelatedPapersRunListController : CSLAController
    {

        private readonly ILogger _logger;

		public MagRelatedPapersRunListController(ILogger<MagRelatedPapersRunListController> logger)
        {

            _logger = logger;
        }
        [HttpGet("[action]")]
        public IActionResult GetMagRelatedPapersRuns()
        {
            try
            {
                SetCSLAUser();

                DataPortal<MagRelatedPapersRunList> dp = new DataPortal<MagRelatedPapersRunList>();
                MagRelatedPapersRunList result = dp.Fetch();

                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Getting a MagRelatedPapersRuns list has an error");
                throw;
            }
        }


        [HttpPost("[action]")]
        public IActionResult GetMagRelatedPapersRunsId(MVCMagPaperListSelectionCriteria crit)
        {
			try
            {
                SetCSLAUser();

                DataPortal<MagRelatedPapersRunList> dp = new DataPortal<MagRelatedPapersRunList>();
                MagPaperListSelectionCriteria criteria = new MagPaperListSelectionCriteria();
                criteria.MagRelatedRunId = crit.magRelatedRunId;

				MagRelatedPapersRunList result = dp.Fetch(criteria);

                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Getting a MagRelatedPapersRunId list has an error");
                throw;
            }
		}

		[HttpPost("[action]")]
		public IActionResult CreateMagRelatedPapersRun([FromBody] MVCMagRelatedPapersRun magRun)
		{
			try
			{
				if (SetCSLAUser4Writing())
				{

					MagRelatedPapersRun newMagRun = new MagRelatedPapersRun();
					DataPortal<MagRelatedPapersRun> dp = new DataPortal<MagRelatedPapersRun>();

					newMagRun.AllIncluded = Convert.ToBoolean(magRun.allIncluded);
					newMagRun.AttributeId = magRun.attributeId;
					newMagRun.AutoReRun = Convert.ToBoolean(magRun.autoReRun);
					newMagRun.DateFrom = magRun.dateFrom;
                    newMagRun.AttributeName = magRun.attributeName;
					newMagRun.Filtered = magRun.filtered;
					newMagRun.Mode = magRun.mode;
					newMagRun.NPapers = magRun.nPapers;
					newMagRun.Status = magRun.status;
					newMagRun.UserDescription = magRun.userDescription;
					newMagRun.UserStatus = magRun.userStatus;

					newMagRun = dp.Execute(newMagRun);

					return Ok(newMagRun);

				}
				else return Forbid();
			}
			catch (Exception e)
			{
				_logger.LogException(e, "Creating a MagRelatedPapersRun has an error");
				throw;
			}
		}

		[HttpPost("[action]")]
		public IActionResult DeleteMagRelatedPapersRun([FromBody] MVCMagRelatedPapersRun magRun)
		{
			try
			{
				if (SetCSLAUser4Writing())
				{
					DataPortal<MagRelatedPapersRunList> dp = new DataPortal<MagRelatedPapersRunList>();
					MagRelatedPapersRunList result = dp.Fetch();

					MagRelatedPapersRun currentMagRun = result.FirstOrDefault(x => x.MagRelatedRunId == magRun.magRelatedRunId);

					currentMagRun.Delete();
					currentMagRun = currentMagRun.Save();

					return Ok(currentMagRun);

				}else return Forbid();
			}
			catch (Exception e)
			{
				_logger.LogException(e, "Deleting a MagRelatedPapersRun list has an error");
				throw;
			}
		}


        [HttpPost("[action]")]
        public IActionResult ImportMagRelatedPapers([FromBody] MVCMagRelatedPapersRun magRun)
        {
            try
            {
                if (SetCSLAUser4Writing())
                {
                    int num_in_run = magRun.nPapers;

                    DataPortal<MagItemPaperInsertCommand> dp2 = new DataPortal<MagItemPaperInsertCommand>();

                    MagItemPaperInsertCommand command = new MagItemPaperInsertCommand("", "RelatedPapersSearch", magRun.magRelatedRunId);
                    
                    dp2.BeginExecute(command);

                    return Ok();

                }
                else return Forbid();
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Importing a MagRelatedPapersRun list has an error");
                throw;
            }
        }



    }


	public class MVCMagRelatedPapersRun
	{
		
		public int magRelatedRunId = 0;
		public string userDescription = "";
		public int attributeId = 0;
        public string attributeName = "";
		public bool allIncluded = false;
		public string dateRun = "";
		public DateTime dateFrom = DateTime.Now;
		public string autoReRun = "";
		public string mode = "";
		public string filtered = "";
		public string status = "";
		public string userStatus = "";
		public int nPapers = 0;
		public int reviewIdId = 0;
	}

}

