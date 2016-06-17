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
    public class ItemAttributeTextSaveCommand : CommandBase<ItemAttributeTextSaveCommand>
    {
#if SILVERLIGHT
    public ItemAttributeTextSaveCommand(){}
#else
        protected ItemAttributeTextSaveCommand() { }
#endif

        private string _saveType;
        private Int64 _ItemAttributeTextId;
        private Int64 _ItemAttributeId;
        private Int64 _itemDocumentId;
        private int _textFrom;
        private int _textTo;

        public Int64 ItemAttributeTextId
        {
            get { return _ItemAttributeTextId; }
        }
        
        public Int64 ItemAttributeId
        {
            get { return _ItemAttributeId; }
        }

        public Int64 ItemDocumentId
        {
            get { return _itemDocumentId; }
        }

        public int TextFrom
        {
            get { return _textFrom; }
        }

        public int TextTo
        {
            get { return _textTo; }
        }

        public ItemAttributeTextSaveCommand(string saveType, Int64 itemAttributeId, Int64 itemDocumentId, int textFrom, int textTo,
            Int64 itemAttributeTextId)
        {
            _saveType = saveType;
            _ItemAttributeId = itemAttributeId;
            _itemDocumentId = itemDocumentId;
            _textFrom = textFrom;
            _textTo = textTo;
            _ItemAttributeTextId = itemAttributeTextId;
        }

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_saveType", _saveType);
            info.AddValue("_ItemAttributeId", _ItemAttributeId);
            info.AddValue("_itemDocumentId", _itemDocumentId);
            info.AddValue("_textTo", _textTo);
            info.AddValue("_textFrom", _textFrom);
            info.AddValue("_ItemAttributeTextId", _ItemAttributeTextId);
        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            _saveType = info.GetValue<string>("_saveType");
            _ItemAttributeId = info.GetValue<Int64>("_ItemAttributeId");
            _itemDocumentId = info.GetValue<Int64>("_itemDocumentId");
            _textFrom = info.GetValue<int>("_textFrom");
            _textTo = info.GetValue<int>("_textTo");
            _ItemAttributeTextId = info.GetValue<Int64>("_ItemAttributeTextId");
        }


#if !SILVERLIGHT

        protected override void DataPortal_Execute()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ItemAttributeTextInsert", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@ITEM_ATTRIBUTE_ID", _ItemAttributeId));
                    command.Parameters.Add(new SqlParameter("@ITEM_DOCUMENT_ID", _itemDocumentId));
                    command.Parameters.Add(new SqlParameter("@START_AT", _textFrom));
                    command.Parameters.Add(new SqlParameter("@END_AT", _textTo));
                    if (_saveType == "UnCode")
                    {
                        command.CommandText = "st_ItemAttributeTextDelete";
                    }
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

#endif
    }
}
