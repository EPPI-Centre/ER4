using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using BusinessLibrary.BusinessClasses;
using BusinessLibrary.Security;
using Csla;
using Csla.Data;
using ERxWebClient2.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using EPPIDataServices.Helpers;
using Microsoft.Extensions.Logging;

namespace ERxWebClient2.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class WorkAllocationContactListController : CSLAController
    {
        private readonly ILogger _logger;

        public WorkAllocationContactListController(ILogger<WorkAllocationContactListController> logger)
        {
            _logger = logger;
        }

        [HttpGet("[action]")]
        public IActionResult WorkAllocationContactList()//should receive a reviewID!
        {
            try
            {
                SetCSLAUser();
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                DataPortal<WorkAllocationContactList> dp = new DataPortal<WorkAllocationContactList>();
                WorkAllocationContactList result = dp.Fetch();

                //Newtonsoft.Json.JsonSerializerSettings ss = new Newtonsoft.Json.JsonSerializerSettings();
                //string resSt = Newtonsoft.Json.JsonConvert.SerializeObject(result, Newtonsoft.Json.Formatting.None, new Newtonsoft.Json.JsonSerializerSettings
                //{
                //    ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
                //});
                //return resSt;
                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Work Allocation data portal error");
                throw;
            }

        }

		[HttpGet("[action]")]
		public IActionResult WorkAllocations()//should receive a reviewID!
		{
			try
			{
				SetCSLAUser();
				ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
				DataPortal<WorkAllocationList> dp = new DataPortal<WorkAllocationList>();
				WorkAllocationList result = dp.Fetch();

				//Newtonsoft.Json.JsonSerializerSettings ss = new Newtonsoft.Json.JsonSerializerSettings();
				//string resSt = Newtonsoft.Json.JsonConvert.SerializeObject(result, Newtonsoft.Json.Formatting.None, new Newtonsoft.Json.JsonSerializerSettings
				//{
				//    ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
				//});
				//return resSt;
				return Ok(result);
			}
			catch (Exception e)
			{
				_logger.LogException(e, "Work Allocation data portal error");
				throw;
			}

		}


		[HttpPost("[action]")]
		public IActionResult DeleteWorkAllocation([FromBody] SingleIntCriteria workAllocationId)//should receive a reviewID!
		{
			try
			{
				
				SetCSLAUser();
				ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
				DataPortal<WorkAllocation> dp = new DataPortal<WorkAllocation>();
				SingleCriteria<WorkAllocation, int> criteria = new SingleCriteria<WorkAllocation, int>(workAllocationId.Value);
				WorkAllocation result = dp.Fetch(criteria);

				result.Delete();
				result = result.Save();

				return Ok(result);
			}
			catch (Exception e)
			{
				_logger.LogException(e, "Work Allocation data portal error");
				throw;
			}
		}


		[HttpPost("[action]")]
		public IActionResult PerformRandomAllocate([FromBody] PerformRandomAllocateCommandJSON data)
		{
			try
			{
				if (SetCSLAUser4Writing())
				{
					PerformRandomAllocateCommand cmd = new PerformRandomAllocateCommand(
						data.filterType,
						data.attributeIdFilter,
						data.setIdFilter,
						data.attributeId,
						data.setId,
						data.howMany,
						data.numericRandomSample,
						data.randomSampleIncluded);
					DataPortal<PerformRandomAllocateCommand> dp = new DataPortal<PerformRandomAllocateCommand>();
					cmd = dp.Execute(cmd);
					return Ok(cmd);
				}
				else return Forbid();
			}
			catch (Exception e)
			{
				_logger.LogException(e, "PerformRandomAllocate error");
				throw;
			}

		}


	}
	public class WorkAllocationJSON
	{
		public long itemid { get; set; }

	}
	public class PerformRandomAllocateCommandJSON
	{
		public string filterType { get; set; }
		public long attributeIdFilter { get; set; }
		public int setIdFilter { get; set; }
		public long attributeId { get; set; }
		public int setId { get; set; }
		public int howMany { get; set; }
		public int numericRandomSample { get; set; }
		public bool randomSampleIncluded { get; set; }
	}
}
