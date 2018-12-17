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
    public class ReviewSetMoveCommand : CommandBase<ReviewSetMoveCommand>
    {
#if SILVERLIGHT
    public ReviewSetMoveCommand(){}
#else
        protected ReviewSetMoveCommand() { }
#endif

        private int _reviewSetId;
        private int _oldSetOrder;
        private int _newSetOrder;

        public ReviewSetMoveCommand(int ReviewSetId, int OldSetOrder, int NewSetOrder)
        {
            _reviewSetId = ReviewSetId;
            _oldSetOrder = OldSetOrder;
            _newSetOrder = NewSetOrder;
        }

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_reviewSetId", _reviewSetId);
            info.AddValue("_oldSetOrder", _oldSetOrder);
            info.AddValue("_newSetOrder", _newSetOrder);
        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            _reviewSetId = info.GetValue<int>("_reviewSetId");
            _oldSetOrder = info.GetValue<int>("_oldSetOrder");
            _newSetOrder = info.GetValue<int>("_newSetOrder");
        }


#if !SILVERLIGHT

        protected override void DataPortal_Execute()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ReviewSetUpdateOrder", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_SET_ID", _reviewSetId));
                    command.Parameters.Add(new SqlParameter("@OLD_SET_ORDER", _oldSetOrder));
                    command.Parameters.Add(new SqlParameter("@NEW_SET_ORDER", _newSetOrder));
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

#endif
    }
}
