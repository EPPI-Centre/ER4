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
using System.Net;

namespace ERxWebClient2.Controllers
{

    [Authorize]
    [Route("api/[controller]")]
    public class ItemDocumentListController : CSLAController
    {

        private IConfiguration _configuration;
        private readonly ILogger<EPPILogger> _Logger;

        public ItemDocumentListController(IConfiguration configuration, ILogger<EPPILogger> logger)
        {
            _configuration = configuration;
            _Logger = logger;
        }

        [HttpPost("[action]")]
        public ItemDocumentList GetDocuments([FromBody] SingleInt64Criteria ItemIDCrit)
        {

            SetCSLAUser();

            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            int rid = ri.ReviewId;
            DataPortal<ItemDocumentList> dp = new DataPortal<ItemDocumentList>();
            SingleCriteria<ItemDocumentList, Int64> criteria = new SingleCriteria<ItemDocumentList, Int64>(ItemIDCrit.Value);

            ItemDocumentList result = dp.Fetch(criteria);
            
            return result;

        }


        [HttpGet("[action]")]
        public ActionResult GetItemDocument(int ItemDocumentID)
        {
           
            SetCSLAUser();

            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;

            SQLHelper sQLHelper = new SQLHelper(_configuration, _Logger);

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
                    
                        SqlDataReader dr = sQLHelper.ExecuteQuerySP(conn, "st_ItemDocumentBin", DOC_ID, REV_ID);
                                            
                        dr.Read();
                        // CHANGE THIS AT THE END
                        if (!dr.HasRows) return NotFound();
                        Response.Headers.Clear();

                        string type = (string)dr["DOCUMENT_EXTENSION"];
                        string name = (string)dr["DOCUMENT_TITLE"];

                        name = name.Replace(type, "") + type;
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

                        Response.Headers.Add("Content-Length", ((byte[])dr["DOCUMENT_BINARY"]).Length.ToString());
                        FileContentResult result = new FileContentResult((byte[])dr["DOCUMENT_BINARY"], Response.ContentType)
                        {
                            FileDownloadName = name
                        };
                        return result;

                }
            }
            catch (Exception e)
            {
                // Testing Logging
                _Logger.Log(LogLevel.Information, 0, "Shutting Down...", e, Formatter);
                _Logger.SQLActionFailed("Error...", parameters, e);
                return NotFound();
            }
        }

        public static string Formatter<TState>(TState state, Exception e)
        {
            return "{0}" + e.Message.ToString();
        }
    }
}

