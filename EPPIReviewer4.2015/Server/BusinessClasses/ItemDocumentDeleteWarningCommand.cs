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
    public class ItemDocumentDeleteWarningCommand : CommandBase<ItemDocumentDeleteWarningCommand>
    {
        public ItemDocumentDeleteWarningCommand() { }
        public ItemDocumentDeleteWarningCommand(long docId, long itemId) {
            _ItemId = itemId;
            _DocumentId = docId;        
        }

        
        private int _numCodings;


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

        public int NumCodings
        {
            get { return _numCodings; }
        }

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_DocumentId", _DocumentId);
            info.AddValue("_ItemId", _ItemId);
            info.AddValue("_numCodings", _numCodings);
        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            _ItemId = info.GetValue<Int64>("_ItemId");
            _DocumentId = info.GetValue<Int64>("_DocumentId");
            _numCodings = info.GetValue<int>("_numCodings");
        }


#if !SILVERLIGHT

        protected override void DataPortal_Execute()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ItemDocumentDeleteWarning", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    SqlParameter output = new SqlParameter("@NUM_CODING", System.Data.SqlDbType.Int);
                    output.Direction = System.Data.ParameterDirection.Output;
                    command.Parameters.Add(output);

                    command.Parameters.Add(new SqlParameter("@ITEM_DOCUMENT_ID", _DocumentId));
                    command.Parameters.Add(new SqlParameter("@ITEM_ID", _ItemId));
                    command.ExecuteNonQuery();
                    int? tmp = output.Value as int?;
                    if (tmp == null) _numCodings = 0;
                    else _numCodings = (int)tmp;
                }
                connection.Close();
            }
        }

#endif
    }
}
