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
#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class ReviewSetMoveCommand : CommandBase<ReviewSetMoveCommand>
    {
        public ReviewSetMoveCommand() { }

        private Int64 _ReviewSetId;
        private int _ReviewSetOrder;

        public ReviewSetMoveCommand(Int64 ReviewSetId, int ReviewSetOrder)
        {
            _ReviewSetId = ReviewSetId;
            _ReviewSetOrder = ReviewSetOrder;
        }

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_ReviewSetId", _ReviewSetId);
            info.AddValue("_ReviewSetOrder", _ReviewSetOrder);
        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            _ReviewSetId = info.GetValue<Int64>("_ReviewSetId");
            _ReviewSetOrder = info.GetValue<int>("_ReviewSetOrder");
        }


#if !SILVERLIGHT

        protected override void DataPortal_Execute()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ReviewSetMove", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_SET_ID", _ReviewSetId));
                    command.Parameters.Add(new SqlParameter("@NEW_SET_ORDER", _ReviewSetOrder));
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

#endif
    }
}
