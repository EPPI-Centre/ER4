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
using Microsoft.CodeAnalysis.Elfie.Serialization;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using Csla.Data;

#if!SILVERLIGHT
using System.Data.SqlClient;
using BusinessLibrary.Data;
using BusinessLibrary.Security;
#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class ItemDocumentDeleteCommand : CommandBase<ItemDocumentDeleteCommand>
    {
        public ItemDocumentDeleteCommand(){}

        private Int64 _DocumentId;

        public Int64 DocumentId
        {
            get { return _DocumentId; }
        }
        private Int64 _ItemId;

        public Int64 ItemId
        {
            get { return _ItemId; }
        }

        public ItemDocumentDeleteCommand(Int64 documentId, Int64 itemId)
        {
            _DocumentId = documentId;
            _ItemId = itemId;
        }

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_DocumentId", _DocumentId);
            info.AddValue("_ItemDocumentId", _ItemId);
        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            _DocumentId = info.GetValue<Int64>("_DocumentId");
            _ItemId = info.GetValue<Int64>("_ItemId");
        }


#if !SILVERLIGHT

        protected override void DataPortal_Execute()
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            int RevId = ri.ReviewId;
            if (!ri.IsAuthenticated) return;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ItemDocumentDelete", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@DocID", _DocumentId));
                    command.Parameters.Add(new SqlParameter("@ItemID", _ItemId));
                    command.Parameters.Add(new SqlParameter("@RevID", RevId));
                    command.Parameters.Add(new SqlParameter("@IsLast", System.Data.SqlDbType.Bit));
                    command.Parameters["@IsLast"].Direction = System.Data.ParameterDirection.Output;
                    command.Parameters.Add(new SqlParameter("@Extension", System.Data.SqlDbType.Bit));
                    command.Parameters["@Extension"].Direction = System.Data.ParameterDirection.Output;
                    command.ExecuteNonQuery();
                    if (command.Parameters["@IsLast"].Value != null && (bool)command.Parameters["@IsLast"].Value == true)
                    {
                        string Ext = (string)command.Parameters["@Extension"].Value;
                        CheckAndDeleteDocFromBlob(_DocumentId, Ext);
                    } 
                }
                connection.Close();
            }
        }
        public static void CheckAndDeleteDocFromBlob(long ItemDocumentID, string ext)
        {
            if (ext != ".txt" && ext != "")
            {
                string BlobFilename = ItemDocument.DocBlobFileName(ItemDocumentID, ext);
                BlobOperations.DeleteIfExists(AzureSettings.blobConnection, AzureSettings.FullTextDocsBlobContainer, BlobFilename);
            }
        }
#endif
    }
}
