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
    public class ReviewSetDeleteCommand : CommandBase<ReviewSetDeleteCommand>
    {
        public ReviewSetDeleteCommand(){}

        private Int64 _ReviewSetId;
        private bool _successful;
        private int _SetId;
        private int _Order;

        public bool Successful
        {
            get { return _successful; }
        }

        public ReviewSetDeleteCommand(Int64 ReviewSetId, int SetId, int Order)
        {
            _ReviewSetId = ReviewSetId;
            _SetId = SetId;
            _Order = Order;
        }

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_ReviewSetId", _ReviewSetId);
            info.AddValue("_successful", _successful);
            info.AddValue("_Order", _Order);
            info.AddValue("_SetId", _SetId);
        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            _ReviewSetId = info.GetValue<Int64>("_ReviewSetId");
            _successful = info.GetValue<bool>("_successful");
            _Order = info.GetValue<int>("_Order");
            _SetId = info.GetValue<int>("_SetId");
        }


#if !SILVERLIGHT

        protected override void DataPortal_Execute()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ReviewSetDelete", connection))
                {
                    ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_SET_ID", _ReviewSetId));
                    command.Parameters.Add(new SqlParameter("@SET_ID", _SetId));
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@SET_ORDER", _Order));
                    command.ExecuteNonQuery();
                }
                connection.Close();
                _successful = true;
            }
        }

#endif
    }
}
