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
    public class ReviewSetCheckCodingStatusCommand : CommandBase<ReviewSetCheckCodingStatusCommand>
    {
#if SILVERLIGHT
    public ReviewSetCheckCodingStatusCommand(){}
#else
        protected ReviewSetCheckCodingStatusCommand() { }
#endif

        private int _num_problematic;
        private int _SetId;

        public int ProblematicItemCount
        {
            get { return _num_problematic; }
        }

        public ReviewSetCheckCodingStatusCommand(int SetId)
        {
            _SetId = SetId;
        }

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_SetId", _SetId);
            info.AddValue("_num_problematic", _num_problematic);
        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            _SetId = info.GetValue<int>("_SetId");
            _num_problematic = info.GetValue<int>("_num_problematic");
        }


#if !SILVERLIGHT

        protected override void DataPortal_Execute()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ReviewSetCheckCodingStatus", connection))
                {
                    ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@SET_ID", _SetId));
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@PROBLEMATIC_ITEM_COUNT", 0));
                    command.Parameters["@PROBLEMATIC_ITEM_COUNT"].Direction = System.Data.ParameterDirection.Output;
                    command.ExecuteNonQuery();
                    _num_problematic = Convert.ToInt32(command.Parameters["@PROBLEMATIC_ITEM_COUNT"].Value);
                }
                connection.Close();
            }
        }

#endif
    }
}
