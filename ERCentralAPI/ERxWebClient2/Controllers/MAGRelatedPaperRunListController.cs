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

        public MagRelatedPapersRunListController(ILogger<MagRelatedPapersRunListController> logger) : base(logger)
        { }

        [HttpGet("[action]")]
        public IActionResult GetMagRelatedPapersRuns()
        {
            try
            {
                if (!SetCSLAUser()) return Unauthorized();

                DataPortal<MagRelatedPapersRunList> dp = new DataPortal<MagRelatedPapersRunList>();
                MagRelatedPapersRunList result = dp.Fetch();

                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Getting a MagRelatedPapersRuns list has an error");
                return StatusCode(500, e.Message);
            }
        }

        
        [HttpPost("[action]")]
        public IActionResult GetMagRelatedPapersRunsId([FromBody] MVCMagPaperListSelectionCriteria crit)
        {
			try
            {
                if (!SetCSLAUser()) return Unauthorized();

                DataPortal<MagPaperList> dp = new DataPortal<MagPaperList>();
                MagPaperListSelectionCriteria criteria = new MagPaperListSelectionCriteria
                {
                    ListType = crit.listType,
                    PageSize = crit.pageSize,
                    PageNumber = crit.pageNumber,
                    MagRelatedRunId = crit.magRelatedRunId,
                    Included = crit.included

                };

                MagPaperList result = dp.Fetch(criteria);

                return Ok(new MAGList4Json(result));
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Getting a MagRelatedPapersRunId list has an error");
                return StatusCode(500, e.Message);
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
                    DateTime dtFrom = new DateTime();
                    if (magRun.dateFrom == "")
                    {
                        //this is not good
                        newMagRun.DateFrom = null;
                    }
                    else
                    {
                        bool resultDateFrom = DateTime.TryParse(magRun.dateFrom, out dtFrom);
                        if (resultDateFrom)
                        {
                            newMagRun.DateFrom = dtFrom;
                        }
                    }
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
				return StatusCode(500, e.Message);
			}
		}

		[HttpPost("[action]")]
		public IActionResult DeleteMagRelatedPapersRun([FromBody] SingleInt64Criteria Id)
		{
			try
			{
				if (SetCSLAUser4Writing())
				{
					DataPortal<MagRelatedPapersRunList> dp = new DataPortal<MagRelatedPapersRunList>();
					MagRelatedPapersRunList result = dp.Fetch();

					MagRelatedPapersRun currentMagRun = result.FirstOrDefault(x => x.MagRelatedRunId == Id.Value);

					currentMagRun.Delete();
					currentMagRun = currentMagRun.Save();

					return Ok(currentMagRun);

				}else return Forbid();
			}
			catch (Exception e)
			{
				_logger.LogException(e, "Deleting a MagRelatedPapersRun has an error");
				return StatusCode(500, e.Message);
			}
		}

        [HttpPost("[action]")]
        public IActionResult UpdateMagRelatedRun([FromBody] MVCMagRelatedPapersRun magRun)
        {
            try
            {
                if (SetCSLAUser4Writing())
                {
                    DataPortal<MagRelatedPapersRunList> dp = new DataPortal<MagRelatedPapersRunList>();
                    MagRelatedPapersRunList result = dp.Fetch();

                    MagRelatedPapersRun currentMagRun = result.FirstOrDefault(x => x.MagRelatedRunId == magRun.magRelatedRunId);

                    currentMagRun.AllIncluded = Convert.ToBoolean(magRun.allIncluded);
                    currentMagRun.AttributeId = magRun.attributeId;
                    currentMagRun.AutoReRun = Convert.ToBoolean(magRun.autoReRun);
                    DateTime dtFrom = new DateTime();
                    bool resultDateFrom = DateTime.TryParse(magRun.dateFrom, out dtFrom);
                    if (resultDateFrom)
                    {
                        currentMagRun.DateFrom = dtFrom;
                    }
                    currentMagRun.AttributeName = magRun.attributeName;
                    currentMagRun.Filtered = magRun.filtered;
                    currentMagRun.Mode = magRun.mode;
                    currentMagRun.NPapers = magRun.nPapers;
                    currentMagRun.Status = magRun.status;
                    currentMagRun.UserDescription = magRun.userDescription;
                    currentMagRun.UserStatus = magRun.userStatus;

                    currentMagRun = currentMagRun.Save();

                    return Ok(currentMagRun);

                }
                else return Forbid();
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Updating a Mag Related Run Auto-ReRun has an error");
                return StatusCode(500, e.Message);
            }
        }


        [HttpGet("[action]")]
        public IActionResult GetMagAutoUpdateRuns()
        {
            try
            {
                if (!SetCSLAUser()) return Unauthorized();

                DataPortal<MagAutoUpdateRunList> dp = new DataPortal<MagAutoUpdateRunList>();
                MagAutoUpdateRunList result = dp.Fetch();

                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogException(e, "GetMagAutoUpdateRuns error");
                return StatusCode(500, e.Message);
            }
        }
        [HttpGet("[action]")]
        public IActionResult GetMagAutoUpdateList()
        {
            try
            {
                if (!SetCSLAUser()) return Unauthorized();

                DataPortal<MagAutoUpdateList> dp = new DataPortal<MagAutoUpdateList>();
                MagAutoUpdateList result = dp.Fetch();

                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogException(e, "GetMagAutoUpdateList error");
                return StatusCode(500, e.Message);
            }
        }

        [HttpPost("[action]")]
        public IActionResult CreateAutoUpdate([FromBody] MVCMagRelatedPapersRun magRun)
        {
            try
            {
                if (SetCSLAUser4Writing())
                {
                    MagAutoUpdate mao = new MagAutoUpdate();
                    mao.UserDescription = magRun.userDescription;
                    mao.AllIncluded = magRun.allIncluded;
                    mao.AttributeId = magRun.attributeId;
                    mao.AttributeName = magRun.attributeName;
                    mao = mao.Save();
                    return Ok(mao);
                }
                else return Forbid();
            }
            catch (Exception e)
            {
                _logger.LogException(e, "CreateAutoUpdateRun error");
                return StatusCode(500, e.Message);
            }
        }
        //DeleteAutoUpdate
        [HttpPost("[action]")]
        public IActionResult DeleteAutoUpdate([FromBody] SingleInt64Criteria Id)
        {
            try
            {
                if (SetCSLAUser4Writing())
                {
                    DataPortal<MagAutoUpdateList> dp = new DataPortal<MagAutoUpdateList>();
                    MagAutoUpdateList list = dp.Fetch();
                    MagAutoUpdate mao = list.FirstOrDefault(f => f.MagAutoUpdateId == Id.Value);
                    if (mao != null && mao.MagAutoUpdateId == Id.Value)
                    {
                        list.Remove(mao);
                    }
                    return Ok(list);
                }
                else return Forbid();
            }
            catch (Exception e)
            {
                _logger.LogException(e, "DeleteAutoUpdate error. AutoUpdateId: " + Id.Value.ToString());
                return StatusCode(500, e.Message);
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

                    MagItemPaperInsertCommand command = new MagItemPaperInsertCommand("", "RelatedPapersSearch", magRun.magRelatedRunId,
                        0, "", 0, 0, 0, 0, "", "", "");

                    command = dp2.Execute(command);

                    return Ok(command);

                }
                else return Forbid();
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Importing a Mag Related Paper list has an error");
                return StatusCode(500, e.Message);
            }
        }


        [HttpPost("[action]")]
        public IActionResult ImportMagRelatedSelectedPapers([FromBody] SingleStringCriteria magSelectedPapers)
        {
            try
            {
                if (SetCSLAUser4Writing())
                {

                    DataPortal<MagItemPaperInsertCommand> dp2 = new DataPortal<MagItemPaperInsertCommand>();

                    MagItemPaperInsertCommand command = new MagItemPaperInsertCommand(magSelectedPapers.Value, "SelectedPapers", 0,
                        0, "", 0, 0, 0, 0, "", "", "");

                    command = dp2.Execute(command);

                    return Ok(command);

                }
                else return Forbid();
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Importing a Mag Related Selected Paper list has an error");
                return StatusCode(500, e.Message);
            }
        }

    }

    public class MAGList4Json
    {
        private MagPaperList _list;
        public int pagesize
        {
            get { return _list.PageSize; }
        }
        public int pagecount
        {
            get { return _list.PageCount; }
        }
        public int pageindex
        {
            get { return _list.PageIndex; }
        }
        public int totalItemCount
        {
            get { return _list.TotalItemCount; }
        }
        public string paperIds
        {
            get { return _list.PaperIds; }
        }
        public MagPaperList papers
        {
            get { return _list; }
        }
        public MAGList4Json(MagPaperList list)
        { _list = list; }
    }

    public class MVCMagRelatedPapersRun
	{
		
		public int magRelatedRunId = 0;
		public string userDescription = "";
		public int attributeId = 0;
        public string attributeName = "";
		public bool allIncluded = false;
		public string dateRun = "";
		public string dateFrom = "";
        public bool autoReRun = false;
		public string mode = "";
		public string filtered = "";
		public string status = "";
		public string userStatus = "";
		public int nPapers = 0;
		public int reviewIdId = 0;
	}

}

