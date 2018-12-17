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
    public class ItemDuplicateUnDUPCommand : CommandBase<ItemDuplicateUnDUPCommand>
    {
#if SILVERLIGHT
    public ItemDuplicateUnDUPCommand(){}
#else
        protected ItemDuplicateUnDUPCommand() { }
#endif

        private Int64 _ItemID;


        public Int64 ItemID
        {
            get { return _ItemID; }
        }

        public ItemDuplicateUnDUPCommand(Int64 ItemID)
        {
            _ItemID = ItemID;

        }

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_ItemID", _ItemID);
        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            _ItemID = info.GetValue<Int64>("_ItemID");
        }


#if !SILVERLIGHT

        protected override void DataPortal_Execute()
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            int RevID = ri.ReviewId;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ItemDuplicateGroupManualRemoveItem", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add("@DuplicateItemID", System.Data.SqlDbType.BigInt);
                    command.Parameters["@DuplicateItemID"].Value = ItemID;
                    command.Parameters.Add("@RevID", System.Data.SqlDbType.Int);
                    command.Parameters["@RevID"].Value = RevID;
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

#endif
    }
}
