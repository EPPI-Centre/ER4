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
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using EPPIDataServices.Helpers;
using Newtonsoft.Json;
using Microsoft.Azure.Management.Compute.Fluent.VirtualMachine.Definition;

namespace ERxWebClient2.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class WebDbController : CSLAController
    {
        
        public WebDbController(ILogger<WebDbController> logger) : base(logger)
        { }

        [HttpGet("[action]")]
        public IActionResult GetWebDBs()
        {

            try
            {
                if (!SetCSLAUser()) return Unauthorized();
                WebDBsList res = DataPortal.Fetch<WebDBsList>();
                return Ok(res);
            }
            catch (Exception e)
            {
                _logger.LogException(e, "GetWebDBs data portal error");
                return StatusCode(500, e.Message);
            }
        }

        [HttpPost("[action]")]
		public IActionResult CreateOrEditWebDB([FromBody] WebDbJson data)
		{
			try
            {
                if (SetCSLAUser4Writing())
                {
                    WebDB wdb = data.GetWebDBCSLA();
                    wdb = wdb.Save(true);
                    WebDBsList res = DataPortal.Fetch<WebDBsList>();
                    //we return all WebDbs as it's cheap and allows to keep things simple
                    return Ok(res);
				}
                else return Forbid();
            }
            catch (Exception e)
            {
                _logger.LogException(e, "CreateOrEditWebDB data portal error");
                return StatusCode(500, e.Message);
            }
        }

       

        

        [HttpPost("[action]")]
        public IActionResult DeleteWebDB([FromBody] SingleIntCriteria crit)
        {
            try
            {
                if (SetCSLAUser4Writing())
                {
                    WebDBsList res = DataPortal.Fetch<WebDBsList>();
                    WebDB toDel = res.ToList().Find(found => found.WebDBId == crit.Value);
                    if (toDel != null && toDel.WebDBId == crit.Value)
                    {
                        toDel.BeginEdit();
                        toDel.Delete();
                        toDel.ApplyEdit();
                        WebDB done = toDel.Save();
                    }
                    res = DataPortal.Fetch<WebDBsList>();
                    return Ok(res);
                }
                else return Forbid();
            }
            catch (Exception e)
            {
                _logger.LogException(e, "DeleteWebDB data portal error");
                return StatusCode(500, e.Message);
            }
        }

    }

	public class WebDbJson
	{
        public int webDBId;
        public string webDBName;
        public string webDBDescription;
        public long attributeIdFilter;
        public bool isOpen;
        public string userName;
        public string password;
        public string createdBy;
        public string editedBy;
        public WebDB GetWebDBCSLA()
        {
            WebDB res = new WebDB();
            res.WebDBId = webDBId;
            if (webDBId != 0) res.MarkAsOldAndDirty();//sends the "save()" method to "dataportalUpdate()", otherwise it's an insert.
            res.WebDBName = webDBName;
            res.WebDBDescription = webDBDescription;
            res.AttributeIdFilter = attributeIdFilter;
            res.IsOpen = isOpen;
            res.UserName = userName;
            res.Password = password;
            res.CreatedBy = createdBy;
            res.EditedBy = editedBy;
            return res;
        }

    }
}
