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
				if (SetCSLAUser4Writing())
				{
					ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
					DataPortal<WorkAllocation> dp = new DataPortal<WorkAllocation>();
					SingleCriteria<WorkAllocation, int> criteria = new SingleCriteria<WorkAllocation, int>(workAllocationId.Value);
					WorkAllocation result = dp.Fetch(criteria);

					result.Delete();
					result = result.Save();

					return Ok(result);
				}
				else return Forbid();
			}
			catch (Exception e)
			{
				_logger.LogException(e, "Work Allocation data portal error");
				throw;
			}
		}
		//AssignWorkAllocation
		[HttpPost("[action]")]
		public IActionResult AssignWorkAllocation([FromBody] WorkAllocationJSON workAllocation)//should receive a reviewID!
		{
			try
			{

				if (SetCSLAUser4Writing())
				{

					ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
					WorkAllocation newWA = new WorkAllocation();
					DataPortal<WorkAllocation> dp = new DataPortal<WorkAllocation>();

					newWA.AttributeId = workAllocation.attributeId;
					newWA.SetId = workAllocation.setId;
					int contactID = 0;
					bool res = int.TryParse(workAllocation.contactId, out contactID);
					if (res)
					{
						newWA.ContactId = contactID;
					}
					newWA = newWA.Save();

					return Ok();
				}
				else return Forbid();
				
			}
			catch (Exception e)
			{
				_logger.LogException(e, "Assign Work Allocation data portal error");
				throw;
			}
		}
	}
	public class WorkAllocationJSON
	{
		public int workAllocationId { get; set; }
		public string contactName{ get; set; }
		public string contactId { get; set; }
		public string setName { get; set; }
		public int setId { get; set; }
		public string attributeName { get; set; }
		public int attributeId { get; set; }
		public int totalAllocation { get; set; }
		public int totalStarted { get; set; }
		public int totalRemaining { get; set; }

	}
}
