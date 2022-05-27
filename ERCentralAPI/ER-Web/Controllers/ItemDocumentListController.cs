using BusinessLibrary.BusinessClasses;
using BusinessLibrary.Security;
using Csla;
using EPPIDataServices.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Net;
//using Telerik.Windows.Documents.Fixed.FormatProviders.Pdf;
//using Telerik.Windows.Documents.Fixed.FormatProviders.Text;
//using Telerik.Windows.Documents.Fixed.Model;

namespace ERxWebClient2.Controllers
{

    [Authorize]
    [Route("api/[controller]")]
    public class ItemDocumentListController : CSLAController
    {

        private IConfiguration _configuration;
        public ItemDocumentListController(IConfiguration configuration, ILogger<ItemDocumentListController> logger) : base(logger)
        {
            _configuration = configuration;
        }

        [HttpPost("[action]")]
        public IActionResult GetDocuments([FromBody] SingleInt64Criteria ItemIDCrit)
        {
            try
            {
                if (!SetCSLAUser()) return Unauthorized();

                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                int rid = ri.ReviewId;
                DataPortal<ItemDocumentList> dp = new DataPortal<ItemDocumentList>();
                SingleCriteria<ItemDocumentList, Int64> criteria = new SingleCriteria<ItemDocumentList, Int64>(ItemIDCrit.Value);

                ItemDocumentList result = dp.Fetch(criteria);
                //System.Threading.Thread.Sleep(2000);
                return Ok(result);

            }
            catch (Exception e)
            {
                _logger.LogError(e,"DocumentList data portal controller error",  ItemIDCrit.Value);
                return StatusCode(500, e.Message);
            }

        }


        [HttpGet("[action]")]
        public Microsoft.AspNetCore.Mvc.ActionResult GetItemDocument(int ItemDocumentID)
        {
            
            if (!SetCSLAUser()) return Unauthorized();

            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;

            SQLHelper sQLHelper = new SQLHelper(_configuration, _logger);

            SqlParameter DOC_ID = new SqlParameter("@DOC_ID", SqlDbType.Int);
            SqlParameter REV_ID = new SqlParameter("@REV_ID", SqlDbType.Int);

            SqlParameter[] parameters = new SqlParameter[2];
            parameters[0] = DOC_ID;
            parameters[1] = REV_ID;

            try
            {
                
                DOC_ID.Value = ItemDocumentID;
                REV_ID.Value = ri.ReviewId;

                using (SqlConnection conn = new SqlConnection(sQLHelper.ER4DB))
                {
                        conn.Open();

                    using (SqlDataReader dr = sQLHelper.ExecuteQuerySP(conn, "st_ItemDocumentBin", DOC_ID, REV_ID))
                    {


                        dr.Read();
                        // CHANGE THIS AT THE END
                        if (!dr.HasRows) return NotFound();
                        Response.Headers.Clear();

                        string type = (string)dr["DOCUMENT_EXTENSION"];
                        string name = (string)dr["DOCUMENT_TITLE"];

                        name = System.Web.HttpUtility.UrlEncode( name.Replace(type, "") + type);
                        if (name.IndexOf(type) == -1) name = name + type;

                        switch (type.ToLower())
                        {
                            case ".pdf":
                                Response.Headers.Clear();
                                Response.ContentType = @"application/pdf";
                                Response.Headers.Add("Content-Disposition", "inline; filename=" + name);
                                break;
                            case ".doc":
                                Response.Headers.Clear();
                                Response.ContentType = @"application/msword";
                                Response.Headers.Add("Content-Disposition", "attachment; filename=" + name);
                                break;
                            case ".docx":
                                Response.Headers.Clear();
                                Response.ContentType = @"application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                                Response.Headers.Add("Content-Disposition", "attachment; filename=" + name);
                                break;
                            case ".ppt":
                                Response.ContentType = @"application/vnd.ms-powerpoint";
                                Response.Headers.Add("Content-Disposition", "attachment; filename=" + name);
                                break;
                            case ".pps":
                                Response.ContentType = @"application/vnd.ms-powerpoint";
                                Response.Headers.Add("Content-Disposition", "attachment; filename=" + name);
                                break;
                            case ".pptx":
                                Response.ContentType = @"application/vnd.openxmlformats-officedocument.presentationml.presentation";
                                Response.Headers.Add("Content-Disposition", "attachment; filename=" + name);
                                break;
                            case ".ppsx":
                                Response.ContentType = @"application/vnd.openxmlformats-officedocument.presentationml.slideshow";
                                Response.Headers.Add("Content-Disposition", "attachment; filename=" + name);
                                break;
                            case ".xls":
                                Response.ContentType = @"application/vnd.ms-excel";
                                Response.Headers.Add("Content-Disposition", "attachment; filename=" + name);
                                break;
                            case ".xlsx":
                                Response.ContentType = @"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                                Response.Headers.Add("Content-Disposition", "attachment; filename=" + name);
                                break;
                            case ".htm":
                                Response.ContentType = @"text/html";
                                Response.Headers.Add("Content-Disposition", "attachment; filename=" + name);
                                break;
                            case ".html":
                                Response.ContentType = @"text/html";
                                Response.Headers.Add("Content-Disposition", "attachment; filename=" + name);
                                break;
                            case ".odt":
                                Response.ContentType = @"application/vnd.oasis.opendocument.text";
                                Response.Headers.Add("Content-Disposition", "attachment; filename=" + name);
                                break;
                            case ".ods":
                                Response.ContentType = @"application/vnd.oasis.opendocument.spreadsheet";
                                Response.Headers.Add("Content-Disposition", "attachment; filename=" + name);
                                break;
                            case ".odp":
                                Response.ContentType = @"application/vnd.oasis.opendocument.presentation";
                                Response.Headers.Add("Content-Disposition", "attachment; filename=" + name);
                                break;
                            case ".ps":
                                Response.ContentType = @"application/postscript";
                                Response.Headers.Add("Content-Disposition", "attachment; filename=" + name);
                                break;
                            case ".eps":
                                Response.ContentType = @"application/postscript";
                                Response.Headers.Add("Content-Disposition", "attachment; filename=" + name);
                                break;
                            case ".csv":
                                Response.ContentType = @"application/vnd.ms-excel";
                                Response.Headers.Add("Content-Disposition", "attachment; filename=" + name);
                                break;
                            case "txt":
                            case ".txt":
                                Response.ContentType = @"text/plain";
                                Response.Headers.Add("Content-Disposition", "attachment; filename=" + name);
                                break;
                            default:
                                Response.ContentType = @"text/plain";
                                //Response.Body.("page not found");
                                // CHANGE THIS STUFF
                                return null;
                        }
                        FileContentResult result;
                        if (type.ToLower() != ".txt")
                        {
                            Response.Headers.Add("Content-Length", ((byte[])dr["DOCUMENT_BINARY"]).Length.ToString());
                            result = new FileContentResult((byte[])dr["DOCUMENT_BINARY"], Response.ContentType)
                            {
                                FileDownloadName = name
                            };
                        }
                        else
                        {
                            byte[] stBytes = System.Text.Encoding.UTF8.GetBytes(dr["DOCUMENT_TEXT"].ToString());
                            Response.Headers.Add("Content-Length", stBytes.Length.ToString());
                            result = new FileContentResult(stBytes, Response.ContentType)
                            {
                                FileDownloadName = name
                            };
                        }
                        //provisional test: can we get the extracted text?
                        if (type.ToLower() == ".pdf")
                        {
                            
                            //var bin = dr["DOCUMENT_BINARY"];
                            //PdfFormatProvider provider = new PdfFormatProvider();
                            //RadFixedDocument document = provider.Import((byte[])dr["DOCUMENT_BINARY"]);
                            //TextFormatProvider textFormatProvider = new TextFormatProvider();
                            //string text = textFormatProvider.Export(document);
                        }
                        return result;
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "File download failure DOCID: {0}, REVID: {1}", parameters[0], parameters[1]);
                _logger.SQLActionFailed("Download Error...", parameters, e);
                return NotFound();
            }
        }

        [HttpPost("[action]")]
        public IActionResult Upload([FromForm] UploadDoc incoming)
        {

            try
            {
                if (SetCSLAUser4Writing())
                {
                    string filename = incoming.files[0].FileName;
                    int ind = filename.LastIndexOf(".");
                    string ext = filename.Substring(ind);
                    Stream stream = incoming.files[0].OpenReadStream();
                    byte[] Binary = new byte[stream.Length];
                    stream.Read(Binary, 0, (int)stream.Length);
                    if (ext.ToLower() == ".txt") {
                        string SimpleText = System.Text.Encoding.UTF8.GetString(Binary);
                        ItemDocumentSaveCommand cmd = new ItemDocumentSaveCommand(incoming.itemID,
                            filename,
                            ext,
                            SimpleText
                            );
                        cmd.doItNow();
                    }
                    else
                    {
                        ItemDocumentSaveBinCommand cmd = new ItemDocumentSaveBinCommand(incoming.itemID,
                            filename,
                            ext,
                            Binary
                            );
                        cmd.doItNow();
                    }
                    return Ok();
                }
                else return Forbid();
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Upload doc file error");
                return StatusCode(500, e.Message);
            }

        }

        // DELETE WARNING COMMAND OBJECT
        [HttpPost("[action]")]
        public IActionResult DeleteDocWarning([FromBody] SingleInt64Criteria id)
        {

            try
            {
                if (SetCSLAUser4Writing())
                {
                    DataPortal<ItemDocumentDeleteWarningCommand> dp = new DataPortal<ItemDocumentDeleteWarningCommand>();
                    ItemDocumentDeleteWarningCommand command = new ItemDocumentDeleteWarningCommand(id.Value);
                    command = dp.Execute(command);
                    return Ok(command.NumCodings);
                }
                else
                {
                    return Forbid();
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error when delete doc warning is called: {0}", id.Value);
                return StatusCode(500, e.Message);
            }

        }

        [HttpPost("[action]")]
        public IActionResult DeleteDoc([FromBody] SingleInt64Criteria id)
        {

            try
            {
                if (SetCSLAUser4Writing())
                {
                    ItemDocumentDeleteCommand cmd = new ItemDocumentDeleteCommand(id.Value);
                    DataPortal<ItemDocumentDeleteCommand> dp = new DataPortal<ItemDocumentDeleteCommand>();
                    cmd = dp.Execute(cmd);

                    return Ok(cmd);
                }
                else
                {
                    return Forbid();
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error when Deleting uploaded Document: {0}", id.Value);
                return StatusCode(500, e.Message);
            }
        }
    }
    public class UploadDoc
    {
        public long itemID { get; set; }
        public IFormFile[] files { get; set; } 
    }
}

