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
    public class WebDbController : CSLAController
    {
        private IConfiguration _Configuration;
        public WebDbController(ILogger<WebDbController> logger, IConfiguration configuration) : base(logger)
        { _Configuration = configuration; }

        //[HttpGet("[action]")]
        /*public IActionResult GetWebDBLogs()
        {
            try
            {
                if (!SetCSLAUser()) return Unauthorized();
                int WebDBID = 1002;
                DateTime From = new DateTime(2021, 01, 01, 12, 12, 12);
                DateTime Until = new DateTime(1980, 01, 01, 00, 00, 00);
                string Type = "all";

                ReadOnlyWebDbActivityListSelectionCrit crit = new ReadOnlyWebDbActivityListSelectionCrit(WebDBID, From, Until, Type);
                ReadOnlyWebDbActivityList res = DataPortal.Fetch<ReadOnlyWebDbActivityList>(crit);
                return Ok(res);
            }
            catch (Exception e)
            {
                _logger.LogException(e, "GetWebDBLogs data portal error");
                return StatusCode(500, e.Message);
            }
        }*/

        [HttpPost("[action]")]
        public IActionResult GetWebDBLogs([FromBody] ReadOnlyWebDbActivityListSelectionCritJson critJson)
        {
            try
            {
                if (SetCSLAUser())
                {    
                    ReadOnlyWebDbActivityListSelectionCrit crit = critJson.GetFetchCriteria();
                    ReadOnlyWebDbActivityList res = DataPortal.Fetch<ReadOnlyWebDbActivityList>(crit);
                    return Ok(res);
                }
                else return Forbid();
            }
            catch (Exception e)
            {
                _logger.LogException(e, "GetWebDBLogs data portal error");
                return StatusCode(500, e.Message);
            }
        }

        [HttpGet("[action]")]
        public IActionResult GetWebDBs()
        {

            try
            {
                if (!SetCSLAUser()) return Unauthorized();
                WebDBsList res = DataPortal.Fetch<WebDBsList>();
                string eppiVisBaseUrltxt = _Configuration.GetValue<string>("AppSettings:EPPIVisUrl");
                WebDbListWithUrl res2 = new WebDbListWithUrl() { webDbList = res, eppiVisBaseUrl = eppiVisBaseUrltxt };
                return Ok(res2);
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

        [HttpPost("[action]")]
        public IActionResult GetWebDBMaps([FromBody] SingleIntCriteria crit)
        {
            try
            {
                if (SetCSLAUser())
                {
                    WebDbMapList res = DataPortal.Fetch<WebDbMapList>(new WebDBMapCriteria(crit.Value, 0));
                    return Ok(res);
                }
                else return Forbid();
            }
            catch (Exception e)
            {
                _logger.LogException(e, "GetWebDBMaps error");
                return StatusCode(500, e.Message);
            }
        }
        [HttpPost("[action]")]
        public IActionResult CreateWebDBMap([FromBody] WebDbMapMVC map)
        {
            try
            {
                if (SetCSLAUser4Writing())
                {
                    if (map.webDBMapId != 0 || map.webDBId < 1)
                    {
                        throw new Exception("Inconsistent data, can't proceed");
                    }
                    WebDbMap mapC = map.toCSLAWebDbMap();
                    mapC = mapC.Save();
                    //we'll return the whole list of results, as it's cheap and gives us all data
                    WebDbMapList res = DataPortal.Fetch<WebDbMapList>(new WebDBMapCriteria(map.webDBId, 0));
                    return Ok(res);
                }
                else return Forbid();
            }
            catch (Exception e)
            {
                _logger.LogException(e, "CreateWebDBMap error");
                return StatusCode(500, e.Message);
            }
        }
        [HttpPost("[action]")]
        public IActionResult UpdateWebDBMap([FromBody] WebDbMapMVC map)
        {
            try
            {
                if (SetCSLAUser4Writing())
                {
                    if (map.webDBMapId < 1 || map.webDBId < 1)
                    {
                        throw new Exception("Inconsistent data, can't proceed");
                    }
                    WebDbMap mapC = map.toCSLAWebDbMap();//object comes out already "old" and "dirty", see method...
                    mapC = mapC.Save();
                    //we'll return the whole list of results, as it's cheap and gives us all data
                    WebDbMapList res = DataPortal.Fetch<WebDbMapList>(new WebDBMapCriteria(map.webDBId, 0));
                    return Ok(res);
                }
                else return Forbid();
            }
            catch (Exception e)
            {
                _logger.LogException(e, "UpdateWebDBMap error");
                return StatusCode(500, e.Message);
            }
        }
        [HttpPost("[action]")]
        public IActionResult DeleteWebDBMap([FromBody] WebDbMapMVC map)
        {
            try
            {
                if (SetCSLAUser4Writing())
                {
                    if (map.webDBMapId < 1 || map.webDBId < 1)
                    {
                        throw new Exception("Inconsistent data, can't proceed");
                    }
                    WebDbMap mapC = map.toCSLAWebDbMap();//object comes out already "old" and "dirty", see method...
                    mapC.Delete();
                    mapC = mapC.Save();
                    //we'll return the whole list of results, as it's cheap and gives us all data
                    WebDbMapList res = DataPortal.Fetch<WebDbMapList>(new WebDBMapCriteria(map.webDBId, 0));
                    return Ok(res);
                }
                else return Forbid();
            }
            catch (Exception e)
            {
                _logger.LogException(e, "DeleteWebDBMap error");
                return StatusCode(500, e.Message);
            }
        }
    }
    public class WebDbListWithUrl
    {
        public WebDBsList webDbList { get; set; }
        public string eppiVisBaseUrl { get; set; }
    }
	public class WebDbJson
	{
        public int webDBId;
        public string webDBName;
        public string subtitle;
        public string webDBDescription;
        public long attributeIdFilter;
        public bool isOpen;
        public string userName;
        public string password;
        public string createdBy;
        public string editedBy;
        public string headerImage1Url;
        public string headerImage2Url;
        public string headerImage3Url;
        public WebDB GetWebDBCSLA()
        {
            WebDB res = new WebDB();
            res.WebDBId = webDBId;
            if (webDBId != 0) res.MarkAsOldAndDirty();//sends the "save()" method to "dataportalUpdate()", otherwise it's an insert.
            res.WebDBName = webDBName;
            res.Subtitle = subtitle;
            res.WebDBDescription = webDBDescription;
            res.AttributeIdFilter = attributeIdFilter;
            res.IsOpen = isOpen;
            res.UserName = userName;
            res.Password = password;
            res.CreatedBy = createdBy;
            res.EditedBy = editedBy;
            res.HeaderImage1Url = headerImage1Url;
            res.HeaderImage2Url = headerImage2Url;
            res.HeaderImage3Url = headerImage3Url;
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

    public class WebDbMapMVC
    {
        public WebDbMap toCSLAWebDbMap()
        {
            WebDbMap res = new WebDbMap();
            res.ColumnsAttributeID = columnsAttributeID;
            res.ColumnsPublicAttributeID = columnsPublicAttributeID;
            //res.ColumnsPublicAttributeName = columnsPublicAttributeName;
            res.ColumnsPublicSetID = columnsPublicSetID;
            //res.ColumnsPublicSetName = columnsPublicSetName;
            res.ColumnsSetID = columnsSetID;
            res.RowsAttributeID = rowsAttributeID;
            res.RowsPublicAttributeID = rowsPublicAttributeID;
            res.RowsPublicSetID = rowsPublicSetID;
            res.RowsSetID = rowsSetID;
            res.SegmentsAttributeID = segmentsAttributeID;
            res.SegmentsPublicAttributeID = segmentsPublicAttributeID;
            res.SegmentsPublicSetID = segmentsPublicSetID;
            res.SegmentsSetID = segmentsSetID;
            res.WebDBId = webDBId;
            res.WebDBMapDescription = webDBMapDescription;
            res.WebDBMapId = webDBMapId;
            res.WebDBMapName = webDBMapName;
            if (webDBId > 0 && webDBMapId > 0) res.MarkAsOldAndDirty();
            return res;
        }
        public long columnsAttributeID { get; set; }
        public int columnsPublicAttributeID { get; set; }
        public string columnsPublicAttributeName { get; set; }
        public int columnsPublicSetID { get; set; }
        public string columnsPublicSetName { get; set; }
        public int columnsSetID { get; set; }
        public long rowsAttributeID { get; set; }
        public int rowsPublicAttributeID { get; set; }
        public string rowsPublicAttributeName { get; set; }
        public int rowsPublicSetID { get; set; }
        public string rowsPublicSetName { get; set; }
        public int rowsSetID { get; set; }
        public long segmentsAttributeID { get; set; }
        public int segmentsPublicAttributeID { get; set; }
        public string segmentsPublicAttributeName { get; set; }
        public int segmentsPublicSetID { get; set; }
        public string segmentsPublicSetName { get; set; }
        public int segmentsSetID { get; set; }
        public int webDBId { get; set; }
        public string webDBMapDescription { get; set; }
        public int webDBMapId { get; set; }
        public string webDBMapName { get; set; }
    }

    public class ReadOnlyWebDbActivityListSelectionCritJson
    {
        public int wedDBId;
        public string from;
        public string until;
        public string logType;

        public ReadOnlyWebDbActivityListSelectionCrit GetFetchCriteria()
        {

            DateTime From = DateTime.Parse(from);
            DateTime Until = DateTime.Parse(until);

            ReadOnlyWebDbActivityListSelectionCrit res = new ReadOnlyWebDbActivityListSelectionCrit(wedDBId, From, Until, logType);
            return res;
        }
    }
}
