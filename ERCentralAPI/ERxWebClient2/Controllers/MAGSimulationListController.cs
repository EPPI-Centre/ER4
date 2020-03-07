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
    public class MagSimulationListController : CSLAController
    {

        private readonly ILogger _logger;

		public MagSimulationListController(ILogger<MagSimulationListController> logger)
        {

            _logger = logger;
        }

        [HttpGet("[action]")]
        public IActionResult GetMagSimulationList()
        {
			try
            {
                SetCSLAUser();

                DataPortal<MagSimulationList> dp = new DataPortal<MagSimulationList>();
				MagSimulationList result = dp.Fetch();

                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Getting a magSimulation list has an error");
                throw;
            }
		}

		[HttpPost("[action]")]
		public IActionResult CreateMagSimulation([FromBody] MVCMagSimulation magSimulation)
		{
			try
			{
				if (SetCSLAUser4Writing())
				{

					MagSimulation newMagSimulation = new MagSimulation();
					DataPortal<MagSimulation> dp = new DataPortal<MagSimulation>();

                    //newMagRun.AllIncluded = Convert.ToBoolean(magRun.allIncluded);
                    //newMagRun.AttributeId = magRun.attributeId;
                    //newMagRun.AutoReRun = Convert.ToBoolean(magRun.autoReRun);
                    //newMagRun.DateFrom = magRun.dateFrom;
                    //               newMagRun.AttributeName = magRun.attributeName;
                    //newMagRun.Filtered = magRun.filtered;
                    //newMagRun.Mode = magRun.mode;
                    //newMagRun.NPapers = magRun.nPapers;
                    //newMagRun.Status = magRun.status;
                    //newMagRun.UserDescription = magRun.userDescription;
                    //newMagRun.UserStatus = magRun.userStatus;

                    newMagSimulation = dp.Execute(newMagSimulation);

					return Ok(newMagSimulation);

				}
				else return Forbid();
			}
			catch (Exception e)
			{
				_logger.LogException(e, "Creating a MagSimulation has an error");
				throw;
			}
		}

		//[HttpPost("[action]")]
		//public IActionResult DeleteMagRelatedPapersRun([FromBody] MVCMagRelatedPapersRun magRun)
		//{
		//	try
		//	{
		//		if (SetCSLAUser4Writing())
		//		{
		//			DataPortal<MagSimulationList> dp = new DataPortal<MagSimulationList>();
		//			MagSimulationList result = dp.Fetch();

		//			MagRelatedPapersRun currentMagRun = result.FirstOrDefault(x => x.MagRelatedRunId == magRun.magRelatedRunId);

		//			currentMagRun.Delete();
		//			currentMagRun = currentMagRun.Save();

		//			return Ok(currentMagRun);

		//		}else return Forbid();
		//	}
		//	catch (Exception e)
		//	{
		//		_logger.LogException(e, "Deleting a MagRelatedPapersRun list has an error");
		//		throw;
		//	}
		//}
	}


	public class MVCMagSimulation
	{
        public int magSimulationId = 0;
        public int reviewId= 0;
        public int year = 0;
        public DateTime createdDate = DateTime.Now;
        public int withThisAttributeId = 0;
        public int filteredByAttributeId = 0;
        public string searchMethod = "";
        public string networkStatistic = "";
        public string studyTypeClassifier  = "";
        public int userClassifierModelId = 0;
        public string status = "";
        public string withThisAttribute = "";
        public string filteredByAttribute = "";
        public string userClassifierModel  = "";
        public int TP = 0;
        public int FP = 0;
        public int FN = 0;
        public int TN = 0;
	}

}

