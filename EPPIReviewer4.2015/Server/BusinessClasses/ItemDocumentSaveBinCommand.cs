using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Csla;
using Csla.Security;
using Csla.Core;
using Csla.Serialization;
using Csla.Silverlight;
//using Csla.Validation;
using System.ComponentModel;
using Csla.DataPortalClient;
using System.Threading;
using static System.Net.Mime.MediaTypeNames;

#if!SILVERLIGHT
using System.Data.SqlClient;
using BusinessLibrary.Data;
using BusinessLibrary.Security;
using EPPIiFilter;
#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class ItemDocumentSaveBinCommand : CommandBase<ItemDocumentSaveBinCommand>
    {
        public ItemDocumentSaveBinCommand(){}

        private string _documentTitle;
        private string _documentExtension;
        private Int64 _itemId;
        private byte[] _docbin;
        private string _ZoteroKey;
        private Int64 _itemDocumentId = -1;


        public string DocumentTitle
        {
            get { return _documentTitle; }
        }

        public string DocumentExtension
        {
            get { return _documentExtension; }
        }
        public Int64 ItemId
        {
            get { return _itemId; }
        }
        public byte[] docbin
        {
            get { return _docbin; }
        }
        public string ZoteroKey
        {
            get { return _ZoteroKey; }
        }
        public Int64 ItemDocumentId
        {
            get { return _itemDocumentId; }
        }

        public ItemDocumentSaveBinCommand(Int64 itemId, string documentTitle, string documentExtension, byte[] docbin)
        {
            _itemId = itemId;
            _documentTitle = documentTitle;
            _documentExtension = documentExtension;
            _docbin = docbin;
            _ZoteroKey = "";
        }
        public ItemDocumentSaveBinCommand(Int64 itemId, string documentTitle, string documentExtension, byte[] docbin, string zoteroKey)
        {
            _itemId = itemId;
            _documentTitle = documentTitle;
            _documentExtension = documentExtension;
            _docbin = docbin;
            _ZoteroKey = zoteroKey;
        }

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_documentTitle", _documentTitle);
            info.AddValue("_documentExtension", _documentExtension);
            info.AddValue("_docbin", _docbin);
            info.AddValue("_itemId", _itemId);
            info.AddValue("_ZoteroKey", _ZoteroKey);
            info.AddValue("_itemDocumentId", _itemDocumentId); 


        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            _documentTitle = info.GetValue<string>("_documentTitle");
            _documentExtension = info.GetValue<string>("_documentExtension");
            _docbin = info.GetValue<byte[]>("_docbin");
            _itemId = info.GetValue<Int64>("_itemId");
            _ZoteroKey = info.GetValue<string>("_ZoteroKey");
            _itemDocumentId = info.GetValue<Int64>("_itemDocumentId");
        }

#if !SILVERLIGHT

        protected override void DataPortal_Execute()
        {
            string _documentText;
            EPPIiFilter.FilterResults res = EPPIiFilter.TextFilter.TextFilter1(_docbin, _documentExtension);
            string hashed = "";
            if (res.ReturnState != "OK")
            {
                _documentText = res.ReturnState;
                //return;
            }
            else
            {
                _documentText = ImportItems.ImportRefs.StripIllegalChars(res.SimpleText);
            }
            if (_documentText.Length > 200) hashed = HashString(_documentText);
            else
            {
                //0x1C209ADD594DF6B37167F1F668D582D1F37658F7
                hashed = "0x0000000000000000000000000000000000000000";
            }

            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ItemDocumentBinInsert", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@ITEM_ID", _itemId));
                    command.Parameters.Add(new SqlParameter("@DOCUMENT_TITLE", _documentTitle));
                    command.Parameters.Add(new SqlParameter("@BIN", System.Data.SqlDbType.Image));
                    command.Parameters[2].Value = System.DBNull.Value;
                    command.Parameters.Add(new SqlParameter("@DOCUMENT_EXTENSION", _documentExtension));
                    command.Parameters.Add(new SqlParameter("@DOCUMENT_TEXT", _documentText));
                    command.Parameters.Add(new SqlParameter("@ZoteroKey", _ZoteroKey));
                    command.Parameters.Add(new SqlParameter("@HashString", hashed));
                    command.Parameters.Add(new SqlParameter("@ItemDocumentId", System.Data.SqlDbType.BigInt));
                    command.Parameters["@ItemDocumentId"].Direction = System.Data.ParameterDirection.Output;
                    command.ExecuteNonQuery();
                    //if (_ZoteroKey != "")
                    //{
                        _itemDocumentId = (long)command.Parameters["@ItemDocumentId"].Value;
                    //}
                }
                connection.Close();
            }
            string blobname = ItemDocument.DocBlobFileName(ItemDocumentId, _documentExtension);
            MemoryStream ms = new MemoryStream(_docbin);
            BlobOperations.UploadStream(AzureSettings.blobConnection, AzureSettings.FullTextDocsBlobContainer, blobname, ms);
        }
        public ItemDocumentSaveBinCommand doItNow()
        {
            DataPortal_Execute();
            return this;
        }
        private static string HashString(string input)
        {
            string res = "";
            //var sha1 = new System.Security.Cryptography.SHA1.;
            byte[] plaintextBytes = Encoding.Unicode.GetBytes(input);
            byte[]? hashBytes = System.Security.Cryptography.SHA1.HashData(plaintextBytes);

            if (hashBytes != null)
            {
                System.Text.StringBuilder s = new System.Text.StringBuilder();
                s.Append("0x");
                foreach (byte b in hashBytes)
                {
                    s.Append(b.ToString("x2").ToUpper());
                }
                //res = System.Convert.ToBase64String(hashBytes);
                res = s.ToString();
            }
            return res;
        }
#endif
    }

}
