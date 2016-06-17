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
    public class ItemSetBulkCompleteCommand : CommandBase<ItemSetBulkCompleteCommand>
    {
#if SILVERLIGHT
        public ItemSetBulkCompleteCommand() { }
#else
        protected ItemSetBulkCompleteCommand() { }
#endif

        private bool _complete;
        private int _setId;
        private int _contact_id;

        public ItemSetBulkCompleteCommand(int SetId, int ContactId, bool Complete)
        {
            _setId = SetId;
            _complete = Complete;
            _contact_id = ContactId;
        }

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_complete", _complete);
            info.AddValue("_contact_id", _contact_id);
            info.AddValue("_setId", _setId);
        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            _complete = info.GetValue<bool>("_complete");
            _contact_id = info.GetValue<int>("_contact_id");
            _setId = info.GetValue<int>("_setId");
        }


#if !SILVERLIGHT

        protected override void DataPortal_Execute()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ItemSetBulkComplete", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@SET_ID", _setId));
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@COMPLETE", _complete));
                    command.Parameters.Add(new SqlParameter("@CONTACT_ID", _contact_id));
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

#endif
    }
}
