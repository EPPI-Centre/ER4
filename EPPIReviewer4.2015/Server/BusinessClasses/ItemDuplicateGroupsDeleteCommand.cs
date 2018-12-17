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
    public class ItemDuplicateGroupsDeleteCommand : CommandBase<ItemDuplicateGroupsDeleteCommand>
    {
#if SILVERLIGHT
    public ItemDuplicateGroupsDeleteCommand(){}
#else
        protected ItemDuplicateGroupsDeleteCommand() { }
#endif

        private bool _deleteAll;
        

        public bool delete
        {
            get { return _deleteAll; }
        }

        public ItemDuplicateGroupsDeleteCommand(bool DeleteAll)
        {
            _deleteAll = DeleteAll;
            
        }

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_delete", _deleteAll);
        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            _deleteAll = info.GetValue<bool>("_delete");
        }


#if !SILVERLIGHT

        protected override void DataPortal_Execute()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ItemDuplicateWipeData", connection))
                {
                    ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@wipeAll", _deleteAll));
                    command.Parameters.Add(new SqlParameter("@ReviewID", ri.ReviewId));
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

#endif
    }
}
