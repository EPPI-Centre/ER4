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

        public ItemDocumentSaveBinCommand(Int64 itemId, string documentTitle, string documentExtension, byte[] docbin)
        {
            _itemId = itemId;
            _documentTitle = documentTitle;
            _documentExtension = documentExtension;
            _docbin = docbin;
        }

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_documentTitle", _documentTitle);
            info.AddValue("_documentExtension", _documentExtension);
            info.AddValue("_docbin", _docbin);
            info.AddValue("_itemId", _itemId);

        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            _documentTitle = info.GetValue<string>("_documentTitle");
            _documentExtension = info.GetValue<string>("_documentExtension");
            _docbin = info.GetValue<byte[]>("_docbin");
            _itemId = info.GetValue<Int64>("_itemId");
        }

#if !SILVERLIGHT

        protected override void DataPortal_Execute()
        {
            string _documentText;
            EPPIiFilter.FilterResults res = EPPIiFilter.TextFilter.TextFilter1(_docbin, _documentExtension);
            if (res.ReturnState != "OK")
            {
                _documentText = res.ReturnState;
                //return;
            } 
            else 
            {
                _documentText = ImportItems.ImportRefs.StripIllegalChars(res.SimpleText);
            }
                using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand("st_ItemDocumentBinInsert", connection))
                    {
                        ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@ITEM_ID", _itemId));
                        command.Parameters.Add(new SqlParameter("@DOCUMENT_TITLE", _documentTitle));
                        command.Parameters.Add(new SqlParameter("@BIN", _docbin));
                        command.Parameters.Add(new SqlParameter("@DOCUMENT_EXTENSION", _documentExtension));
                        command.Parameters.Add(new SqlParameter("@DOCUMENT_TEXT", _documentText));
                        command.ExecuteNonQuery();
                    }
                    connection.Close();
                }
        }
        public ItemDocumentSaveBinCommand doItNow()
        {
            DataPortal_Execute();
            return this;
        }

#endif
    }

}
