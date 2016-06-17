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
    public class ItemAttributePDFResetCommand : CommandBase<ItemAttributePDFResetCommand>
    {
#if SILVERLIGHT
    public ItemAttributePDFResetCommand(){}
#else
        protected ItemAttributePDFResetCommand() { }
#endif

        private Int64 _ItemAttributeID;
        private Int64 _ItemDocumentID;
        private int _Page;

        public ItemAttributePDFResetCommand(Int64 itemAttributeID, Int64 itemDocumentID, int page)
        {
            _ItemAttributeID = itemAttributeID;
            _ItemDocumentID = itemDocumentID;
            _Page = page;
        }
        public ItemAttributePDFResetCommand(Int64 itemAttributeID, Int64 itemDocumentID)
        {
            _ItemAttributeID = itemAttributeID;
            _ItemDocumentID = itemDocumentID;
            _Page = 0;
        }
        

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_ItemAttributeID", _ItemAttributeID);
            info.AddValue("_ItemDocumentID", _ItemDocumentID);
            info.AddValue("_Page", _Page);
        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            _ItemAttributeID = info.GetValue<Int64>("_ItemAttributeID");
            _ItemDocumentID = info.GetValue<Int64>("_ItemDocumentID");
            _Page = info.GetValue<int>("_Page");
        }


#if !SILVERLIGHT

        protected override void DataPortal_Execute()
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ItemAttributePDFReset", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@ITEM_ATTRIBUTE_ID", _ItemAttributeID));
                    command.Parameters.Add(new SqlParameter("@ITEM_DOCUMENT_ID", _ItemDocumentID));
                    command.Parameters.Add(new SqlParameter("@PAGE", _Page));
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

#endif
    }
}
