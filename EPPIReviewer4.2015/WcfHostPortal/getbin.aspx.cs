using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Security.Cryptography;
using System.IO;
using System.Text;
using BusinessLibrary.BusinessClasses;

//using pdftron;
//using pdftron.Common;
//using pdftron.Filters;
//using pdftron.SDF;
//using pdftron.PDF;
using BusinessLibrary.Security;

namespace WcfHostPortal
{
    public partial class getbin : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Request.QueryString["U"] != null & Request.QueryString["ID"] != null & Request.QueryString["DID"] != null)
            {
                getDoc(Request.QueryString["U"], Request.QueryString["ID"], Request.QueryString["DID"]);
            }
            else if (Request.QueryString["V"] != null & Request.QueryString["ID"] != null & Request.QueryString["DID"] != null)
            {
                getDox(Request.QueryString["V"], Request.QueryString["ID"], Request.QueryString["DID"]);
            }
            else if (Request.QueryString["P"] != null & Request.QueryString["ID"] != null & Request.QueryString["DID"] != null)
            {
                getPDF(Request.QueryString["P"], Request.QueryString["ID"], Request.QueryString["DID"]);
            }
        }
        private void getDoc(string GUID, string CID, string DocID)
        {//used for the download fuction
            int iRevID = 0, iCID;
            Int64 iDocID;
            GUID = HttpUtility.UrlDecode(GUID);
            int.TryParse(CID, out iCID);
            Int64.TryParse(DocID, out iDocID);
            if (iCID == 0 || iDocID == 0)
            {
                Response.Write("page not found");
                return;
            }
            try
            {
                using (SqlConnection aConn = new SqlConnection(BusinessLibrary.Data.DataConnection.AdmConnectionString))
                {
                    aConn.Open();
                    using (SqlCommand command = new SqlCommand("st_LogonTicket_Check_Ticket", aConn))
                    {//first additional check use the ticket to get REVIEW_ID, this fails if ticket is not found or marked as expired
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@guid", new System.Data.SqlTypes.SqlGuid(GUID)));
                        command.Parameters.Add(new SqlParameter("@c_ID", iCID));
                        command.Parameters.Add("@RID", System.Data.SqlDbType.Int);
                        command.Parameters["@RID"].Direction = System.Data.ParameterDirection.Output;
                        command.ExecuteNonQuery();
                        iRevID = (int)command.Parameters["@RID"].Value;
                        if (iRevID == 0)
                        {
                            Response.Write("page not found");
                            return;
                        }
                    }
                    using (SqlCommand command = new SqlCommand("st_LogonTicket_Check_Expiration", aConn))
                    {//second check: see if ticket is young enough
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@c_ID", iCID));
                        command.Parameters.Add(new SqlParameter("@guid", new System.Data.SqlTypes.SqlGuid(GUID)));
                        command.Parameters.Add(new SqlParameter("@result", System.Data.SqlDbType.NVarChar));
                        command.Parameters["@result"].Size = 9;
                        command.Parameters["@result"].Direction = System.Data.ParameterDirection.Output;
                        command.Parameters.Add(new SqlParameter("@message", System.Data.SqlDbType.NVarChar));
                        command.Parameters["@message"].Size = 4000;
                        command.Parameters["@message"].Direction = System.Data.ParameterDirection.Output;
                        command.ExecuteNonQuery();
                        if (command.Parameters["@result"].Value.ToString() != "Valid")
                        {
                            Response.Write("page not found");
                            return;
                        }
                    }
                }
                using (SqlConnection conn = new SqlConnection(BusinessLibrary.Data.DataConnection.ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand Cmd = new SqlCommand("st_ItemDocumentBin", conn))
                    {
                        SqlParameter DOC_ID = Cmd.Parameters.Add("@DOC_ID", SqlDbType.Int);
                        SqlParameter REV_ID = Cmd.Parameters.Add("@REV_ID", SqlDbType.Int);
                        Cmd.CommandType = CommandType.StoredProcedure;
                        DOC_ID.Value = iDocID;
                        REV_ID.Value = iRevID;
                        SqlDataReader dr = Cmd.ExecuteReader();
                        dr.Read();
                        if (!dr.HasRows) return;
                        Response.ClearHeaders();
                        Response.ClearContent();
                        Response.Clear();
                        string type = (string)dr["DOCUMENT_EXTENSION"];
                        string name = (string)dr["DOCUMENT_TITLE"];

                        name = name.Replace(type, "") + type;
                        if (name.IndexOf(type) == -1) name = name + type;

                        switch (type.ToLower())
                        {
                            case ".pdf":
                                Response.ClearHeaders();
                                Response.ContentType = @"application/pdf";
                                Response.AddHeader("Content-Disposition", "inline; filename=" + name);
                                break;
                            case ".doc":
                                Response.ClearHeaders();
                                Response.ContentType = @"application/msword";
                                Response.AddHeader("Content-Disposition", "attachment; filename=" + name);
                                break;
                            case ".docx":
                                //Response.ClearHeaders();
                                Response.ContentType = @"application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                                Response.AddHeader("Content-Disposition", "attachment; filename=" + name);
                                break;
                            case ".ppt":
                                Response.ContentType = @"application/vnd.ms-powerpoint";
                                Response.AddHeader("Content-Disposition", "attachment; filename=" + name);
                                break;
                            case ".pps":
                                Response.ContentType = @"application/vnd.ms-powerpoint";
                                Response.AddHeader("Content-Disposition", "attachment; filename=" + name);
                                break;
                            case ".pptx":
                                Response.ContentType = @"application/vnd.openxmlformats-officedocument.presentationml.presentation";
                                Response.AddHeader("Content-Disposition", "attachment; filename=" + name);
                                break;
                            case ".ppsx":
                                Response.ContentType = @"application/vnd.openxmlformats-officedocument.presentationml.slideshow";
                                Response.AddHeader("Content-Disposition", "attachment; filename=" + name);
                                break;
                            case ".xls":
                                Response.ContentType = @"application/vnd.ms-excel";
                                Response.AddHeader("Content-Disposition", "attachment; filename=" + name);
                                break;
                            case ".xlsx":
                                Response.ContentType = @"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                                Response.AddHeader("Content-Disposition", "attachment; filename=" + name);
                                break;
                            case ".htm":
                                Response.ContentType = @"text/html";
                                Response.AddHeader("Content-Disposition", "attachment; filename=" + name);
                                break;
                            case ".html":
                                Response.ContentType = @"text/html";
                                Response.AddHeader("Content-Disposition", "attachment; filename=" + name);
                                break;
                            case ".odt":
                                Response.ContentType = @"application/vnd.oasis.opendocument.text";
                                Response.AddHeader("Content-Disposition", "attachment; filename=" + name);
                                break;
                            case ".ods":
                                Response.ContentType = @"application/vnd.oasis.opendocument.spreadsheet";
                                Response.AddHeader("Content-Disposition", "attachment; filename=" + name);
                                break;
                            case ".odp":
                                Response.ContentType = @"application/vnd.oasis.opendocument.presentation";
                                Response.AddHeader("Content-Disposition", "attachment; filename=" + name);
                                break;
                            case ".ps":
                                Response.ContentType = @"application/postscript";
                                Response.AddHeader("Content-Disposition", "attachment; filename=" + name);
                                break;
                            case ".eps":
                                Response.ContentType = @"application/postscript";
                                Response.AddHeader("Content-Disposition", "attachment; filename=" + name);
                                break;
                            case ".csv":
                                Response.ContentType = @"application/vnd.ms-excel";
                                Response.AddHeader("Content-Disposition", "attachment; filename=" + name);
                                break;
                            case "txt":
                            case ".txt":
                                Response.ContentType = @"text/plain";
                                Response.AddHeader("Content-Disposition", "attachment; filename=" + name);
                                break;
                            default:
                                Response.ContentType = @"text/plain";
                                Response.Write("page not found");
                                return;
                        }
                        Response.AddHeader("Content-Length", ((byte[])dr["DOCUMENT_BINARY"]).Length.ToString());
                        Response.BinaryWrite((byte[])dr["DOCUMENT_BINARY"]);
                        Response.Flush();
                        Response.Close();
                        conn.Close();
                        //Response.End();
                    }
                }
            }
            catch
            {
                Response.Write("page not found");
            }
        }
        private void getPDF(string GUID, string CID, string DocID)
        {//used for the internal viewer
            int iRevID = 0, iCID;
            Int64 iDocID;
            GUID = HttpUtility.UrlDecode(GUID);
            int.TryParse(CID, out iCID);
            Int64.TryParse(DocID, out iDocID);
            if (iCID == 0 || iDocID == 0)
            {
                Response.Write("page not found");
                return;
            }
            try
            {
                using (SqlConnection aConn = new SqlConnection(BusinessLibrary.Data.DataConnection.AdmConnectionString))
                {
                    aConn.Open();
                    using (SqlCommand command = new SqlCommand("st_LogonTicket_Check_Ticket", aConn))
                    {//first additional check use the ticket to get REVIEW_ID, this fails if ticket is not found or marked as expired
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@guid", new System.Data.SqlTypes.SqlGuid(GUID)));
                        command.Parameters.Add(new SqlParameter("@c_ID", iCID));
                        command.Parameters.Add("@RID", System.Data.SqlDbType.Int);
                        command.Parameters["@RID"].Direction = System.Data.ParameterDirection.Output;
                        command.ExecuteNonQuery();
                        iRevID = (int)command.Parameters["@RID"].Value;
                        if (iRevID == 0)
                        {
                            Response.Write("page not found");
                            return;
                        }
                    }
                    using (SqlCommand command = new SqlCommand("st_LogonTicket_Check_Expiration", aConn))
                    {//second check: see if ticket is young enough
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@c_ID", iCID));
                        command.Parameters.Add(new SqlParameter("@guid", new System.Data.SqlTypes.SqlGuid(GUID)));
                        command.Parameters.Add(new SqlParameter("@result", System.Data.SqlDbType.NVarChar));
                        command.Parameters["@result"].Size = 9;
                        command.Parameters["@result"].Direction = System.Data.ParameterDirection.Output;
                        command.Parameters.Add(new SqlParameter("@message", System.Data.SqlDbType.NVarChar));
                        command.Parameters["@message"].Size = 4000;
                        command.Parameters["@message"].Direction = System.Data.ParameterDirection.Output;
                        command.ExecuteNonQuery();
                        if (command.Parameters["@result"].Value.ToString() != "Valid")
                        {
                            Response.Write("page not found");
                            return;
                        }
                    }
                }
                using (SqlConnection conn = new SqlConnection(BusinessLibrary.Data.DataConnection.ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand Cmd = new SqlCommand("st_ItemDocumentBin", conn))
                    {
                        SqlParameter DOC_ID = Cmd.Parameters.Add("@DOC_ID", SqlDbType.Int);
                        SqlParameter REV_ID = Cmd.Parameters.Add("@REV_ID", SqlDbType.Int);
                        Cmd.CommandType = CommandType.StoredProcedure;
                        DOC_ID.Value = iDocID;
                        REV_ID.Value = iRevID;
                        using (SqlDataReader dr = Cmd.ExecuteReader())
                        {
                            dr.Read();
                            if (!dr.HasRows) return;
                            Response.ClearHeaders();
                            Response.ClearContent();
                            Response.Clear();
                            string type = (string)dr["DOCUMENT_EXTENSION"];
                            string name = (string)dr["DOCUMENT_TITLE"];
                            name = name.Replace(type, "") + type;

                            byte[] buf = (byte[])dr["DOCUMENT_BINARY"];
                            //Byte[] tt = new Byte[300 * 1024];

                            if (type.ToLower() != ".pdf")
                            {
                                Response.Write("page not found");
                            }
                            else
                            {
                                Response.ContentType = @"application/pdf";
                                Response.AddHeader("Content-Disposition", "inline; filename=" + name);
                                Response.AddHeader("Content-Length", ((byte[])dr["DOCUMENT_BINARY"]).Length.ToString());
                                Response.BinaryWrite((byte[])dr["DOCUMENT_BINARY"]);
                            }
                        }
                    }
                }
                //Response.End();
            }
            catch
            {
                Response.Write("page not found");

            }
            finally
            {
                Response.Flush();
                Response.Close();
            }
        }
        
        
        
        //private string getPass(int UserID)
        //{
        //    ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
        //    string res;
        //    SqlConnection conn = new SqlConnection(BusinessLibrary.Data.DataConnection.ConnectionString);
        //    try
        //    {
        //        conn.Open();
        //        SqlCommand Cmd = new SqlCommand("st_ContactPasswordFromID", conn);
        //        SqlParameter ID = Cmd.Parameters.Add("@ID", SqlDbType.Int);
        //        Cmd.CommandType = CommandType.StoredProcedure;
        //        ID.Value = UserID;
        //        SqlDataReader dr = Cmd.ExecuteReader();
        //        dr.Read();
        //        res = (string)dr[0];
        //    }
        //    finally
        //    {
        //        if (conn.State != System.Data.ConnectionState.Closed)
        //        {
        //            conn.Close();
        //        }
        //    }
        //    return res;
        //}
        
        //old procedures that used encryption of GET values
        private void getDocOLD(string GUID, string ReviewID, string DocID)
        {//used for the download fuction
            //int iRevID, iDocID;
            //string t = HttpUtility.UrlDecode(DocID);
            //if (!Cryptography.IsAllowed(HttpUtility.UrlDecode(GUID), ReviewID, DocID, out iRevID, out iDocID)) return;

            ////if (password == null) return;
            ////ReviewID = HttpUtility.UrlDecode(ReviewID);
            ////DocID = HttpUtility.UrlDecode(DocID);
            //if (iRevID < 1 || iDocID < 1)
            //{
            //    Response.Write("page not found");
            //    return;
            //}
            //try
            //{
            //    SqlConnection conn = new SqlConnection(BusinessLibrary.Data.DataConnection.ConnectionString);
            //    conn.Open();
            //    SqlCommand Cmd = new SqlCommand("st_ItemDocumentBin", conn);
            //    SqlParameter DOC_ID = Cmd.Parameters.Add("@DOC_ID", SqlDbType.Int);
            //    SqlParameter REV_ID = Cmd.Parameters.Add("@REV_ID", SqlDbType.Int);
            //    Cmd.CommandType = CommandType.StoredProcedure;
            //    DOC_ID.Value = iDocID;
            //    REV_ID.Value = iRevID;
            //    SqlDataReader dr = Cmd.ExecuteReader();
            //    dr.Read();
            //    if (!dr.HasRows) return;
            //    Response.ClearHeaders();
            //    Response.ClearContent();
            //    Response.Clear();
            //    string type = (string)dr["DOCUMENT_EXTENSION"];
            //    string name = (string)dr["DOCUMENT_TITLE"];

            //    name = name.Replace(type, "") + type;
            //    if (name.IndexOf(type) == -1) name = name + type;

            //    switch (type.ToLower())
            //    {
            //        case ".pdf":
            //            Response.ClearHeaders();
            //            Response.ContentType = @"application/pdf";
            //            Response.AddHeader("Content-Disposition", "inline; filename=" + name);
            //            break;
            //        case ".doc":
            //            Response.ClearHeaders();
            //            Response.ContentType = @"application/msword";
            //            Response.AddHeader("Content-Disposition", "attachment; filename=" + name);
            //            break;
            //        case ".docx":
            //            //Response.ClearHeaders();
            //            Response.ContentType = @"application/vnd.openxmlformats-officedocument.wordprocessingml.document";
            //            Response.AddHeader("Content-Disposition", "attachment; filename=" + name);
            //            break;
            //        case ".ppt":
            //            Response.ContentType = @"application/vnd.ms-powerpoint";
            //            Response.AddHeader("Content-Disposition", "attachment; filename=" + name);
            //            break;
            //        case ".pps":
            //            Response.ContentType = @"application/vnd.ms-powerpoint";
            //            Response.AddHeader("Content-Disposition", "attachment; filename=" + name);
            //            break;
            //        case ".pptx":
            //            Response.ContentType = @"application/vnd.openxmlformats-officedocument.presentationml.presentation";
            //            Response.AddHeader("Content-Disposition", "attachment; filename=" + name);
            //            break;
            //        case ".ppsx":
            //            Response.ContentType = @"application/vnd.openxmlformats-officedocument.presentationml.slideshow";
            //            Response.AddHeader("Content-Disposition", "attachment; filename=" + name);
            //            break;
            //        case ".xls":
            //            Response.ContentType = @"application/vnd.ms-excel";
            //            Response.AddHeader("Content-Disposition", "attachment; filename=" + name);
            //            break;
            //        case ".xlsx":
            //            Response.ContentType = @"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            //            Response.AddHeader("Content-Disposition", "attachment; filename=" + name);
            //            break;
            //        case ".htm":
            //            Response.ContentType = @"text/html";
            //            Response.AddHeader("Content-Disposition", "attachment; filename=" + name);
            //            break;
            //        case ".html":
            //            Response.ContentType = @"text/html";
            //            Response.AddHeader("Content-Disposition", "attachment; filename=" + name);
            //            break;
            //        case ".odt":
            //            Response.ContentType = @"application/vnd.oasis.opendocument.text";
            //            Response.AddHeader("Content-Disposition", "attachment; filename=" + name);
            //            break;
            //        case ".ods":
            //            Response.ContentType = @"application/vnd.oasis.opendocument.spreadsheet";
            //            Response.AddHeader("Content-Disposition", "attachment; filename=" + name);
            //            break;
            //        case ".odp":
            //            Response.ContentType = @"application/vnd.oasis.opendocument.presentation";
            //            Response.AddHeader("Content-Disposition", "attachment; filename=" + name);
            //            break;
            //        case ".ps":
            //            Response.ContentType = @"application/postscript";
            //            Response.AddHeader("Content-Disposition", "attachment; filename=" + name);
            //            break;
            //        case ".eps":
            //            Response.ContentType = @"application/postscript";
            //            Response.AddHeader("Content-Disposition", "attachment; filename=" + name);
            //            break;
            //        case ".csv":
            //            Response.ContentType = @"application/vnd.ms-excel";
            //            Response.AddHeader("Content-Disposition", "attachment; filename=" + name);
            //            break;
            //        case "txt":
            //        case ".txt":
            //            Response.ContentType = @"text/plain";
            //            Response.AddHeader("Content-Disposition", "attachment; filename=" + name);
            //            break;
            //        default:
            //            Response.ContentType = @"text/plain";
            //            Response.Write("page not found");
            //            return;
            //    }
            //    Response.AddHeader("Content-Length", ((byte[])dr["DOCUMENT_BINARY"]).Length.ToString());
            //    Response.BinaryWrite((byte[])dr["DOCUMENT_BINARY"]);
            //    Response.Flush();
            //    Response.Close();
            //    conn.Close();
            //    //Response.End();
            //}
            //catch
            //{
            //    Response.Write("page not found");
            //}
        }
        private void getPDFOLD(string GUID, string ReviewID, string DocID)
        {//used for the internal viewer
            //int iRevID, iDocID;
            //string t = HttpUtility.UrlDecode(DocID);
            //if (!Cryptography.IsAllowed(HttpUtility.UrlDecode(GUID), ReviewID, DocID, out iRevID, out iDocID)) return;

            //if (iRevID < 1 || iDocID < 1)
            //{
            //    Response.Write("page not found");
            //    return;
            //}
            //try
            //{
            //    using (SqlConnection conn = new SqlConnection(BusinessLibrary.Data.DataConnection.ConnectionString))
            //    {
            //        conn.Open();



            //        SqlCommand Cmd = new SqlCommand("st_ItemDocumentBin", conn);
            //        SqlParameter DOC_ID = Cmd.Parameters.Add("@DOC_ID", SqlDbType.Int);
            //        SqlParameter REV_ID = Cmd.Parameters.Add("@REV_ID", SqlDbType.Int);
            //        Cmd.CommandType = CommandType.StoredProcedure;
            //        DOC_ID.Value = iDocID;
            //        REV_ID.Value = iRevID;
            //        using (SqlDataReader dr = Cmd.ExecuteReader())
            //        {
            //            dr.Read();
            //            if (!dr.HasRows) return;
            //            Response.ClearHeaders();
            //            Response.ClearContent();
            //            Response.Clear();
            //            string type = (string)dr["DOCUMENT_EXTENSION"];
            //            string name = (string)dr["DOCUMENT_TITLE"];
            //            name = name.Replace(type, "") + type;

            //            byte[] buf = (byte[])dr["DOCUMENT_BINARY"];
            //            //Byte[] tt = new Byte[300 * 1024];

            //            if (type.ToLower() != ".pdf")
            //            {
            //                Response.Write("page not found");
            //            }
            //            else
            //            {
            //                Response.ContentType = @"application/pdf";
            //                Response.AddHeader("Content-Disposition", "inline; filename=" + name);
            //                Response.AddHeader("Content-Length", ((byte[])dr["DOCUMENT_BINARY"]).Length.ToString());
            //                Response.BinaryWrite((byte[])dr["DOCUMENT_BINARY"]);
            //            }



            //            //Response.BinaryWrite((byte[])dr["DOCUMENT_BINARY"]);
            //        }
            //    }
            //    //Response.End();
            //}
            //catch
            //{
            //    Response.Write("page not found");

            //}
            //finally
            //{
            //    Response.Flush();
            //    Response.Close();
            //}
        }
        private void getDox(string GUID, string ReviewID, string DocID)
        {//uses PDF.net, so not relevant anymore, may
            //int iRevID, iDocID;
            //string tFname = "";
            //string t = HttpUtility.UrlDecode(DocID);
            //if (!Cryptography.IsAllowed(HttpUtility.UrlDecode(GUID), ReviewID, DocID, out iRevID, out iDocID)) return;

            ////if (password == null) return;
            ////ReviewID = HttpUtility.UrlDecode(ReviewID);
            ////DocID = HttpUtility.UrlDecode(DocID);
            //if ( iRevID < 1 || iDocID < 1)
            //{
            //    Response.Write("page not found");
            //    return;
            //}
            //try
            //{
            //    PDFNet.Initialize("Institute of Education(ioe.ac.uk):CPU:4::W:AMC(20111207):BC5E29CE25B5B1CA11B45266109CC29A23BC2A4152491DA14E67E0F1F0FA");
            //    SqlConnection conn = new SqlConnection(BusinessLibrary.Data.DataConnection.ConnectionString);
            //    conn.Open();
            //    PDFDoc doc;
            //    pdftron.Filters.Filter filter;

            //    SqlCommand Cmd = new SqlCommand("st_ItemDocumentBin", conn);
            //    SqlParameter DOC_ID = Cmd.Parameters.Add("@DOC_ID", SqlDbType.Int);
            //    SqlParameter REV_ID = Cmd.Parameters.Add("@REV_ID", SqlDbType.Int);
            //    Cmd.CommandType = CommandType.StoredProcedure;
            //    DOC_ID.Value = iDocID;
            //    REV_ID.Value = iRevID;
            //    SqlDataReader dr = Cmd.ExecuteReader();
            //    dr.Read();
            //    if (!dr.HasRows) return;
            //    Response.ClearHeaders();
            //    Response.ClearContent();
            //    Response.Clear();
            //    string type = (string)dr["DOCUMENT_EXTENSION"];
            //    string name = (string)dr["DOCUMENT_TITLE"];
            //    name = name.Replace(type, "") + type;

            //    byte[] buf = (byte[])dr["DOCUMENT_BINARY"];
            //    //Byte[] tt = new Byte[300 * 1024];
            //    doc = new PDFDoc();
            //    if (type.ToLower() != ".pdf")
            //    {

            //        tFname = name + (new Guid()).ToString() + type.ToLower();
            //        using (FileStream stream = new FileStream(Request.PhysicalApplicationPath + "\\UserTempUploads\\" + tFname, FileMode.Create))
            //        {
            //            using (BinaryWriter writer = new BinaryWriter(stream))
            //            {
            //                writer.Write(buf);
            //                writer.Close();
            //            }
            //        }

            //        filter = pdftron.PDF.Convert.ToSilverlight(Request.PhysicalApplicationPath + "UserTempUploads\\" + tFname);
            //    }
            //    else
            //    {
            //        doc = new PDFDoc(buf, buf.Length);
            //        filter = pdftron.PDF.Convert.ToSilverlight(doc);
            //    }
            //    pdftron.Filters.FilterReader fr = new pdftron.Filters.FilterReader(filter);
            //    //Response.AddHeader("content-disposition", "inline; filename=" + name);
            //    Byte[] tt = new Byte[filter.Size()];
            //    //Byte[] tt = new Byte[300 * 1024];

            //    while (fr.Read(tt) == tt.Length)
            //    {
            //        Response.BinaryWrite(tt);
            //    }
            //    Response.BinaryWrite(tt);
            //    doc.Close();
            //    //Response.BinaryWrite((byte[])dr["DOCUMENT_BINARY"]);


            //    conn.Close();

            //    //Response.End();
            //}
            //catch
            //{
            //    Response.Write("page not found");

            //}
            //finally
            //{
            //    Response.Flush();
            //    Response.Close();
            //    if (tFname != "")
            //        File.Delete(tFname);
            //}
        }
  }
}
