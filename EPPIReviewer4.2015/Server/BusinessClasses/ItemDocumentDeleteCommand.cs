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

        public ItemDocumentDeleteCommand(Int64 DocumentId)
        {
            _DocumentId = DocumentId;
        }

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_DocumentId", _DocumentId);
        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            _DocumentId = info.GetValue<Int64>("_DocumentId");
        }


#if !SILVERLIGHT

        protected override void DataPortal_Execute()
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            int RevId = ri.ReviewId;
            if (!ri.IsAuthenticated) return;
            CheckAndDeleteDocFromBlob(_DocumentId, RevId);
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ItemDocumentDelete", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@DocID", _DocumentId));
                    command.Parameters.Add(new SqlParameter("@RevID", RevId));
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }
        public static void CheckAndDeleteDocFromBlob(long ItemDocumentID, int RevId)
        {
            string ext = "";
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ItemDocumentBin", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@DOC_ID", ItemDocumentID));
                    command.Parameters.Add(new SqlParameter("@REV_ID", RevId));
                    using (SafeDataReader reader = new SafeDataReader(command.ExecuteReader()))
                    {
                        if (reader.Read())
                        {
                            ext = reader.GetString("DOCUMENT_EXTENSION");
                        }
                    }
                    connection.Close();
                }
            }
            if (ext != ".txt" && ext != "")
            {
                string BlobFilename = ItemDocument.DocBlobFileName(ItemDocumentID, ext);
                BlobOperations.DeleteIfExists(AzureSettings.blobConnection, AzureSettings.FullTextDocsBlobContainer, BlobFilename);
            }
        }

#endif
    }
}
