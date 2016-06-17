using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
//using Telerik.Windows.RadUploadHandler;
using Telerik.Windows.Controls;
using BusinessLibrary.BusinessClasses;
using System.Threading;
using System.Data.SqlClient;

namespace WcfHostPortal
{
    public class MyRadUploadHandler : Telerik.Windows.RadUploadHandler
    {
        public override Dictionary<string, object> GetAssociatedData()
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            // insert any custom parameters you want to return to the client
            dict.Add("yourParam1", this.Request.Form["myParam1"]);
            return dict;
        }
        public override bool SaveChunkData(string filePath, long position, byte[] buffer, int contentLength, out int savedBytes)
        {
            HttpRequest req = Request;
            string documentTitle = this.GetQueryParameter("documentTitle");
            string documentExtension = this.GetQueryParameter("documentExtension");
            FileInfo FI = new FileInfo(filePath);
            
            if (!Cryptography.IsAllowedExtension(FI.Extension.ToLower()))
            {
                if (FI.Exists) FI.Delete();
                savedBytes = -1;
                return false;
            }
            Int64 itemId;
            string itemIdSt = this.GetQueryParameter("itemId");
            string GUID = this.GetQueryParameter("tGuid");
            string CID = this.GetQueryParameter("UserId");
            ////many things happen here: RevID and itemID get set from DB values if the supplied values all match (ticket guid, revID and ItemID)
            ////IsAllowed returns false if something did not match.
            //bool res = Cryptography.IsAllowed(tGuid, itemIdSt, revIDst, out RevID, out itemId);
            int iRevID = 0, iCID;
            bool res = true;
            GUID = HttpUtility.UrlDecode(GUID);
            int.TryParse(CID, out iCID);
            Int64.TryParse(itemIdSt, out itemId);
            if (iCID == 0 || itemId == 0)
            {
                if (FI.Exists) FI.Delete();
                savedBytes = -1;
                return false;
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
                            if (FI.Exists) FI.Delete();
                            savedBytes = -1;
                            return false;
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
                            if (FI.Exists) FI.Delete();
                            savedBytes = -1;
                            return false;
                        }
                    }
                }
            }
            catch
            {
                if (FI.Exists) FI.Delete();
                savedBytes = -1;
                return false;
            }

            //if (!res)//if check failed delete whatever temp file we're using and return failure. We are interrupting this before data is saved on the temp folder.
            //{
            //    if (FI.Exists) FI.Delete();
            //    savedBytes = -1;
            //    return false;
            //}
            res = base.SaveChunkData(filePath, position, buffer, contentLength, out savedBytes);
            //Thread.Sleep(500);
            if (this.IsFinalUploadRequest())
            {
                

                if (FI.Extension.ToLower() == ".txt")
                {
                    string fileContent = "";
                    StreamReader reader = new StreamReader(FI.OpenRead());
                    while (!reader.EndOfStream)
                    {
                        fileContent = reader.ReadToEnd();
                    }
                    reader.Close();
                    BusinessLibrary.BusinessClasses.ItemDocumentSaveCommand IdSc = new BusinessLibrary.BusinessClasses.ItemDocumentSaveCommand(itemId, FI.Name, ".txt", fileContent);
                    try
                    {
                        IdSc.doItNow();
                    }
                    catch
                    {
                        //Telerik.Windows.RadUploadHandler.
                        res = false;
                    }
                }
                else
                {
                    FileStream fs = File.OpenRead(filePath);
                    byte[] fileBcontent = new byte[fs.Length];
                    int offset = 0;
                    int remaining = fileBcontent.Length;
                    while (remaining > 0)
                    {
                        int read = fs.Read(fileBcontent, offset, remaining);
                        if (read <= 0)
                            throw new EndOfStreamException
                                (String.Format("End of stream reached with {0} bytes left to read", remaining));
                        remaining -= read;
                        offset += read;
                    }


                    BusinessLibrary.BusinessClasses.ItemDocumentSaveBinCommand IdSbc =
                        new BusinessLibrary.BusinessClasses.ItemDocumentSaveBinCommand(itemId, FI.Name, FI.Extension, fileBcontent);
                    try
                    {
                        IdSbc = IdSbc.doItNow();
                    }
                    catch
                    {
                        res = false;
                    }
                    fs.Close();
                    fs.Dispose();
                }
                FI.Delete();
            }

            return res;
        }
        public override string TargetFolder
        {
            get { return "UserTempUploads"; }
            set { base.TargetFolder = value; }
        }
        //public override void ProcessStream()
        //{
        //    base.ProcessStream();
        //    if (this.IsFinalFileRequest())
        //    {
        //        // Insert here your custom logic for processing the file stream.
        //        // You can read the saved file on the disc and to process it in the way you need.
        //        // You can save the file in DB, compress it, etc.
        //        // you can access the form parameters by using the RadUploadConstants
        //        //string targetFolder = this.Request.Form[RadUploadConstants.ParamNameTargetFolder];
        //        // Or by using the predefined properties/methods in the base handler
        //        string targetFolder2 = this.TargetFolder;
        //        string targetPhysicalFolder = "UserTempUploads";
        //        bool isFinalRequest = this.IsFinalFileRequest();
        //        // to send back a parameter - for example the error message, or the result from the upload operation
        //        // use the AddReturn
        //        this.AddReturnParam(RadUploadConstants.ParamNameSuccess, false);
        //        this.AddReturnParam(RadUploadConstants.ParamNameMessage, "Unable to open the Database");
        //    }
        //}
        public override void CancelRequest()
        {
            base.CancelRequest();
        }
    }
}