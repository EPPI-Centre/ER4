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
                _logger.LogException(e, "Getting a MAG Simulation list has an error");
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

                    newMagSimulation.CreatedDate = new SmartDate(magSimulation.createdDate);
                    newMagSimulation.FilteredByAttribute = magSimulation.filteredByAttribute;
                    newMagSimulation.FilteredByAttributeId = magSimulation.filteredByAttributeId;
                    newMagSimulation.FN = magSimulation.fn;
                    newMagSimulation.FP = magSimulation.fp;
                    newMagSimulation.ScoreThreshold = magSimulation.scoreThreshold;
                    newMagSimulation.FosThreshold = magSimulation.FosThreshold;
                    newMagSimulation.NSeeds = magSimulation.nSeeds;
                    newMagSimulation.SearchMethod = magSimulation.searchMethod;
                    newMagSimulation.Status = magSimulation.status;
                    newMagSimulation.StudyTypeClassifier = magSimulation.studyTypeClassifier;
                    newMagSimulation.TP = magSimulation.tp;
                    newMagSimulation.UserClassifierModel = magSimulation.userClassifierModel;
                    newMagSimulation.UserClassifierModelId = magSimulation.userClassifierModelId;
                    newMagSimulation.WithThisAttribute = magSimulation.withThisAttribute;
                    newMagSimulation.WithThisAttributeId = magSimulation.withThisAttributeId;
                    newMagSimulation.Year = magSimulation.year;
                    newMagSimulation.YearEnd = magSimulation.yearEnd;
                    newMagSimulation.CreatedDateEnd = magSimulation.createdDateEnd;
                    newMagSimulation.UserClassifierReviewId = magSimulation.userClassifierReviewId;

                    newMagSimulation = dp.Execute(newMagSimulation);

					return Ok(newMagSimulation);

				}
				else return Forbid();
			}
			catch (Exception e)
			{
				_logger.LogException(e, "Creating a MAG Simulation has an error");
                //TODO investigating bug
				throw;
			}
		}

        [HttpPost("[action]")]
        public IActionResult DeleteMagSimulation([FromBody] SingleInt64Criteria magSimId)
        {
            try
            {
                if (SetCSLAUser4Writing())
                {
                    DataPortal<MagSimulationList> dp = new DataPortal<MagSimulationList>();
                    MagSimulationList result = dp.Fetch();

                    MagSimulation currentMagSim = result.FirstOrDefault(x => x.MagSimulationId == magSimId.Value);

                    currentMagSim.Delete();
                    currentMagSim = currentMagSim.Save();

                    return Ok(currentMagSim);

                }
                else return Forbid();
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Deleting a MAG Simulation list has an error");
                throw;
            }
        }
    }


    public class MVCMagSimulation
	{

        //    newSimulation.YearEnd = SimulationYearEnd.Year;
        //    newSimulation.CreatedDateEnd = CreatedDateEnd;


        public int magSimulationId = 0;
        public int reviewId= 0;
        public int year = 0;
        public int yearEnd = 0;
        public DateTime createdDateEnd = DateTime.Now;
        public DateTime createdDate = DateTime.Now;
        public int withThisAttributeId = 0;
        public int filteredByAttributeId = 0;
        public string searchMethod = "";
        public double scoreThreshold = 0;
        public double FosThreshold = 0;
        public string thresholds = "";
        public string networkStatistic = "";
        public string studyTypeClassifier  = "";
        public int userClassifierModelId = 0;
        public int userClassifierReviewId = 0;
        public string status = "";
        public string withThisAttribute = "";
        public string filteredByAttribute = "";
        public string userClassifierModel  = "";
        public int nSeeds = 0;
        public int tp = 0;
        public int fp = 0;
        public int fn = 0;
        public int tn = 0;

	}

}

