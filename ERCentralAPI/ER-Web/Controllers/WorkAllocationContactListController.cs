using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusinessLibrary.BusinessClasses;
using BusinessLibrary.Security;
using Csla;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EPPIDataServices.Helpers;

namespace ERxWebClient2.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class WorkAllocationContactListController : CSLAController
    {

        public WorkAllocationContactListController(ILogger<WorkAllocationContactListController> logger) : base(logger)
        { }

        [HttpGet("[action]")]
        public IActionResult WorkAllocationContactList()//should receive a reviewID!
        {
            try
            {
                if (!SetCSLAUser()) return Unauthorized();
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
                return StatusCode(500, e.Message);
            }

        }

		[HttpGet("[action]")]
		public IActionResult WorkAllocations()
		{
			try
			{
				if (!SetCSLAUser()) return Unauthorized();
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
                return StatusCode(500, e.Message);
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
				return StatusCode(500, e.Message);
			}
		}

		//AssignWorkAllocation
		[HttpPost("[action]")]
		public IActionResult AssignWorkAllocation([FromBody] WorkAllocationJSON workAllocation)
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
				return StatusCode(500, e.Message);
			}
		}
        [HttpPost("[action]")]
        public IActionResult ExecuteWorkAllocationFromWizardCommand([FromBody] WorkAllocationFromWizardCommandJSON cmdJSON)
        {
            try
            {

                if (SetCSLAUser4Writing())
                {

                    WorkAllocationFromWizardCommand cmd;
                    if (cmdJSON.isPreview == 1)
                    {
                        cmd = new WorkAllocationFromWizardCommand
                            (
                                cmdJSON.filterType,
                                cmdJSON.attributeIdFilter,
                                cmdJSON.setIdFilter,
                                cmdJSON.destination_Attribute_ID,
                                cmdJSON.destination_Set_ID,
                                cmdJSON.percentageOfWholePot,
                                cmdJSON.included
                            );
                    }
                    else
                    {
                        cmd = new WorkAllocationFromWizardCommand
                            (
                                cmdJSON.filterType,
                                cmdJSON.attributeIdFilter,
                                cmdJSON.setIdFilter,
                                cmdJSON.destination_Attribute_ID,
                                cmdJSON.destination_Set_ID,
                                cmdJSON.percentageOfWholePot,
                                cmdJSON.included,
                                cmdJSON.isPreview,
                                cmdJSON.work_to_do_setID,
                                cmdJSON.oneGroupPerPerson,
                                cmdJSON.peoplePerItem,
                                cmdJSON.reviewersIds,
                                cmdJSON.reviewerNames,
                                cmdJSON.itemsPerEachReviewer,
                                cmdJSON.groupsPrefix,
                                cmdJSON.numberOfItemsToAssign
                            );
                    }
                    DataPortal<WorkAllocationFromWizardCommand> dp = new DataPortal<WorkAllocationFromWizardCommand>();
                    cmd = dp.Execute(cmd);
                    WorkAllocationFromWizardCommandResult res = new WorkAllocationFromWizardCommandResult();
                    if (cmdJSON.isPreview == 2)
                    {
                        res.preview = new List<List<string>>();
                        foreach (Csla.Core.MobileList<string> ml in cmd.preview)
                        {
                            res.preview.Add(ml.ToList<string>());
                        }
                    }
                    res.isSuccess = cmd.IsSuccess;
                    res.numberOfAffectedItems = cmd.NumberOfAffectedItems;
                    return Ok(res);
                }
                else return Forbid();

            }
            catch (Exception e)
            {
                _logger.LogException(e, "ExecuteWorkAllocationFromWizardCommand error");
                return StatusCode(500, e.Message);
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
    public class WorkAllocationFromWizardCommandJSON
    {
        //partially based on PerformRandomAllocateCommandJSON 
        public string filterType { get; set; }
        public long attributeIdFilter { get; set; }
        public int setIdFilter { get; set; }
        public long destination_Attribute_ID { get; set; }
        public int destination_Set_ID { get; set; }
        public int percentageOfWholePot { get; set; }
        public bool included { get; set; }

        //public List<List<string>> preview = new List<List<string>>();
        public int isPreview = 1;
        public int work_to_do_setID; //"assign codes from this set"
        public bool oneGroupPerPerson = false;
        public int peoplePerItem = 1;
        public string reviewersIds = "";
        public string reviewerNames = "";
        public string itemsPerEachReviewer = "";
        public string groupsPrefix = "";
        public int numberOfItemsToAssign = 0;
        //public int _numberOfAffectedItems = 0;
        //public bool isSuccess = false;
    }
    public class WorkAllocationFromWizardCommandResult
    {
        //partially based on PerformRandomAllocateCommandJSON 
        //public string filterType { get; set; }
        //public long attributeIdFilter { get; set; }
        //public int setIdFilter { get; set; }
        //public long destination_Attribute_ID { get; set; }
        //public int destination_Set_ID { get; set; }
        //public int percentageOfWholePot { get; set; }
        //public bool included { get; set; }

        public List<List<string>> preview = new List<List<string>>();
        //public int isPreview = 1;
        //public int work_to_do_setID; //"assign codes from this set"
        //public bool oneGroupPerPerson = false;
        //public int peoplePerItem = 1;
        //public string reviewersIds = "";
        //public string reviewerNames = "";
        //public string itemsPerEachReviewer = "";
        //public string groupsPrefix = "";
        //public int numberOfItemsToAssign = 0;
        public int numberOfAffectedItems = 0;
        public bool isSuccess = false;
    }
}
