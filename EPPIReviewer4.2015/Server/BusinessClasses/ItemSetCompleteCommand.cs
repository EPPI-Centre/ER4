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
    public class ItemSetCompleteCommand : CommandBase<ItemSetCompleteCommand>
    {
#if SILVERLIGHT
        public ItemSetCompleteCommand() { }
#else
		public ItemSetCompleteCommand() { }
#endif

        private Int64 _ItemSetId;
        private bool _complete;
        private bool _successful;
        private bool _isLocked;

        public ItemSetCompleteCommand(Int64 ItemSetId, bool Complete, bool isLocked)
        {
            _ItemSetId = ItemSetId;
            _complete = Complete;
            _isLocked = isLocked;
        }

        public bool Successful
        {
            get { return _successful; }
        }

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_ItemSetId", _ItemSetId);
            info.AddValue("_successful", _successful);
            info.AddValue("_complete", _complete);
            info.AddValue("_isLocked", _isLocked);
        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            _ItemSetId = info.GetValue<Int64>("_ItemSetId");
            _successful = info.GetValue<bool>("_successful");
            _complete = info.GetValue<bool>("_complete");
            _isLocked = info.GetValue<bool>("_isLocked");
        }


#if !SILVERLIGHT

        protected override void DataPortal_Execute()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ItemSetComplete", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@ITEM_SET_ID", _ItemSetId));
                    command.Parameters.Add(new SqlParameter("@COMPLETE", _complete));
                    command.Parameters.Add(new SqlParameter("@IS_LOCKED", _isLocked));
                    command.Parameters.Add(new SqlParameter("@SUCCESSFUL", _complete));
                    command.Parameters["@SUCCESSFUL"].Direction = System.Data.ParameterDirection.Output;
                    command.ExecuteNonQuery();
                    _successful = Convert.ToBoolean(command.Parameters["@SUCCESSFUL"].Value);
                }
                connection.Close();
            }
        }

#endif
    }
}
