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
#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class ItemDocumentSaveCommand : CommandBase<ItemDocumentSaveCommand>
    {
#if SILVERLIGHT
    public ItemDocumentSaveCommand(){}
#else
        protected ItemDocumentSaveCommand() { }
#endif

        private string _documentTitle;
        private string _documentExtension;
        private string _documentText;
        private Int64 _itemId;

        public string DocumentTitle
        {
            get { return _documentTitle; }
        }

        public string DocumentExtension
        {
            get { return _documentExtension; }
        }

        public string DocumentText
        {
            get { return _documentText; }
        }

        public Int64 ItemId
        {
            get { return _itemId; }
        }

        public ItemDocumentSaveCommand(Int64 itemId, string documentTitle, string documentExtension, string documentText)
        {
            _itemId = itemId;
            _documentTitle = documentTitle;
            _documentExtension = documentExtension;
            _documentText = ImportItems.ImportRefs.StripIllegalChars(documentText);
        }

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_documentTitle", _documentTitle);
            info.AddValue("_documentExtension", _documentExtension);
            info.AddValue("_documentText", _documentText);
            info.AddValue("_itemId", _itemId);

        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            _documentTitle = info.GetValue<string>("_documentTitle");
            _documentExtension = info.GetValue<string>("_documentExtension");
            _documentText = info.GetValue<string>("_documentText");
            _itemId = info.GetValue<Int64>("_itemId");
        }

#if !SILVERLIGHT

        protected override void DataPortal_Execute()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ItemDocumentInsert", connection))
                {
                    ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@ITEM_ID", _itemId));
                    command.Parameters.Add(new SqlParameter("@DOCUMENT_TITLE", _documentTitle));
                    command.Parameters.Add(new SqlParameter("@DOCUMENT_EXTENSION", _documentExtension));
                    command.Parameters.Add(new SqlParameter("@DOCUMENT_TEXT", _documentText));
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }
        public ItemDocumentSaveCommand doItNow()
        {
            DataPortal_Execute();
            return this;
        }

#endif
    }
}
