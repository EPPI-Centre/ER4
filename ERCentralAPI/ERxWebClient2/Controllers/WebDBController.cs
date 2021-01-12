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
using Microsoft.AspNetCore.Http;
using System.IO;

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


        [HttpPost("[action]")]
        public IActionResult GetWebDbReviewSetsList([FromBody] SingleIntCriteria crit)
        {

            try
            {
                if (!SetCSLAUser()) return Unauthorized();
                WebDbReviewSetsList res = DataPortal.Fetch<WebDbReviewSetsList>(new SingleCriteria<WebDbReviewSetsList, int>(crit.Value));
                return Ok(res);
            }
            catch (Exception e)
            {
                _logger.LogException(e, "GetWebDbReviewSetsList data portal error");
                return StatusCode(500, e.Message);
            }
        }
        [HttpPost("[action]")]
        public IActionResult AddWebDbReviewSet([FromBody] WebDbReviewSetJson data)
        {
            try
            {
                if (SetCSLAUser4Writing())
                {
                    WebDbReviewSet res = new WebDbReviewSet();
                    res.WebDBId = data.webDBId;
                    res.SetId = data.setId;
                    res = res.Save();
                    return Ok(res);
                }
                else return Forbid();
            }
            catch (Exception e)
            {
                _logger.LogException(e, "AddWebDbReviewSet data portal error");
                return StatusCode(500, e.Message);
            }
        }
        [HttpPost("[action]")]
        public IActionResult RemoveWebDbReviewSet([FromBody] WebDbReviewSetJson data)
        {
            try
            {
                if (SetCSLAUser4Writing())
                {
                    WebDBReviewSetCrit cr = data.GetFetchCriteria();

                    WebDbReviewSet toDel = DataPortal.Fetch<WebDbReviewSet>(cr);
                    toDel.Delete();
                    toDel = toDel.Save(true);
                    return Ok();
                }
                else return Forbid();
            }
            catch (Exception e)
            {
                _logger.LogException(e, "AddWebDbReviewSet data portal error");
                return StatusCode(500, e.Message);
            }
        }
        [HttpPost("[action]")]
        public IActionResult UpdateWebDbReviewSet([FromBody] WebDbReviewSetJson data)
        {
            try
            {
                if (SetCSLAUser4Writing())
                {
                    WebDBReviewSetCrit cr = data.GetFetchCriteria();
                    WebDbReviewSet editing = DataPortal.Fetch<WebDbReviewSet>(cr);
                    editing.SetName = data.setName;
                    editing.SetDescription = data.setDescription;
                    editing = editing.Save(true);
                    return Ok(editing);
                }
                else return Forbid();
            }
            catch (Exception e)
            {
                _logger.LogException(e, "AddWebDbReviewSet data portal error");
                return StatusCode(500, e.Message);
            }
        }

        [HttpPost("[action]")]
        public IActionResult EditAddRemoveWebDbAttribute([FromBody] WebDbAttributeSetEditAddRemoveCommandJson data)
        {
            try
            {
                if (SetCSLAUser4Writing())
                {
                    WebDbAttributeSetEditAddRemoveCommand cmd = data.GetCSLACommand();
                    cmd = DataPortal.Execute<WebDbAttributeSetEditAddRemoveCommand>(cmd);
                    WebDBReviewSetCrit cr = data.GetFetchCriteria();
                    WebDbReviewSet edited = DataPortal.Fetch<WebDbReviewSet>(cr);
                    return Ok(edited);
                }
                else return Forbid();
            }
            catch (Exception e)
            {
                _logger.LogException(e, "EditAddRemoveWebDbAttribute data portal error");
                return StatusCode(500, e.Message);
            }
        }
        [HttpPost("[action]")]
        public IActionResult UploadImage([FromForm] UploadImage incoming)
        {
            try
            {
                if (SetCSLAUser4Writing())
                {
                    string filename = incoming.files[0].FileName;
                    int ind = filename.LastIndexOf(".");
                    string ext = filename.Substring(ind).TrimStart('.');
                    Stream stream = incoming.files[0].OpenReadStream();
                    byte[] Binary = new byte[stream.Length];
                    stream.Read(Binary, 0, (int)stream.Length);

                    WebDbImageSaveCommand cmd = new WebDbImageSaveCommand(incoming.webDbId, incoming.imageNumber, ext,
                        Binary
                        );
                    cmd.doItNow();
                    
                    return Ok();
                }
                else return Forbid();
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Upload Image (webDB) file error");
                return StatusCode(500, e.Message);
            }
        }

        [HttpPost("[action]")]
        public IActionResult DeleteHeaderImage([FromBody] DeleteImage JsonC)
        {
            try
            {
                if (SetCSLAUser4Writing())
                {
                    WebDBDeleteHeaderImageCommand cmd = new WebDBDeleteHeaderImageCommand(JsonC.WebDbId, JsonC.imageNumber);
                    cmd = DataPortal.Execute(cmd);
                    return Ok();
                }
                else return Forbid();
            }
            catch (Exception e)
            {
                _logger.LogException(e, "DeleteImage (webDB) error");
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

    public class WebDbReviewSetJson
    {
        public int webDBId;
        public int setId;
        public int webDBSetId;
        public string setName;
        public string setDescription;
        public WebDBReviewSetCrit GetFetchCriteria()
        {
            WebDBReviewSetCrit res = new WebDBReviewSetCrit(webDBId, webDBSetId);
            return res;
        }
    }
    public class WebDbAttributeSetEditAddRemoveCommandJson
    {
        public long attributeId;
        public int setId;
        public int webDbId;
        public int webDBSetId;
        public bool deleting;
        public string publicName;
        public string publicDescription;
        public WebDbAttributeSetEditAddRemoveCommand GetCSLACommand()
        {
            WebDbAttributeSetEditAddRemoveCommand res = new WebDbAttributeSetEditAddRemoveCommand(attributeId, setId, webDbId, deleting, publicName, publicDescription);
            return res;
        }
        public WebDBReviewSetCrit GetFetchCriteria()
        {
            WebDBReviewSetCrit res = new WebDBReviewSetCrit(webDbId, webDBSetId);
            return res;
        }
    }
    public class UploadImage
    {
        public short imageNumber { get; set; }
        public int webDbId { get; set; }
        public IFormFile[] files { get; set; }
    }
    public class DeleteImage
    {
        public int WebDbId { get; set; }
        public short imageNumber { get; set; }
    }
}
