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
    public class MagClassifierContactModelListController : CSLAController
    {

        private readonly ILogger _logger;

		public MagClassifierContactModelListController(ILogger<MagClassifierContactModelListController> logger)
        {

            _logger = logger;
        }

        [HttpGet("[action]")]
        public IActionResult FetchClassifierContactModelList()
        {
			try
            {
                SetCSLAUser();

                DataPortal<ClassifierContactModelList> dp = new DataPortal<ClassifierContactModelList>();
                ClassifierContactModelList result = dp.Fetch();

                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Fetching a ClassifierContactModel list has an error");
                throw;
            }
		}


		//[HttpPost("[action]")]
		//public IActionResult CreateMagSimulation([FromBody] MVCMagSimulation magSimulation)
		//{
		//	try
		//	{
		//		if (SetCSLAUser4Writing())
		//		{

		//			MagSimulation newMagSimulation = new MagSimulation();
		//			DataPortal<MagSimulation> dp = new DataPortal<MagSimulation>();

  //                  //newMagRun.AllIncluded = Convert.ToBoolean(magRun.allIncluded);
  //                  //newMagRun.AttributeId = magRun.attributeId;
  //                  //newMagRun.AutoReRun = Convert.ToBoolean(magRun.autoReRun);
  //                  //newMagRun.DateFrom = magRun.dateFrom;
  //                  //               newMagRun.AttributeName = magRun.attributeName;
  //                  //newMagRun.Filtered = magRun.filtered;
  //                  //newMagRun.Mode = magRun.mode;
  //                  //newMagRun.NPapers = magRun.nPapers;
  //                  //newMagRun.Status = magRun.status;
  //                  //newMagRun.UserDescription = magRun.userDescription;
  //                  //newMagRun.UserStatus = magRun.userStatus;

  //                  newMagSimulation = dp.Execute(newMagSimulation);

		//			return Ok(newMagSimulation);

		//		}
		//		else return Forbid();
		//	}
		//	catch (Exception e)
		//	{
		//		_logger.LogException(e, "Creating a MagSimulation has an error");
		//		throw;
		//	}
		//}
    }


	public class MagClassifierContactModel
    {
        //public int magSimulationId = 0;
        //public int reviewId= 0;
        //public int year = 0;
        //public DateTime createdDate = DateTime.Now;
	}

}

