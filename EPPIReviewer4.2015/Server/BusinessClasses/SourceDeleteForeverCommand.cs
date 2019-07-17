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
    public class SourceDeleteForeverCommand : CommandBase<SourceDeleteForeverCommand>
    {
    public SourceDeleteForeverCommand(){}

        private int _SourceId;
        public int SourceId
        {
            get { return _SourceId; }
        }

        public SourceDeleteForeverCommand(int SourceId)
        {
            _SourceId = SourceId;
        }

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_SourceId", _SourceId);

        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            _SourceId = info.GetValue<int>("_SourceId");
        }

#if !SILVERLIGHT

        protected override void DataPortal_Execute()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_SourceDeleteForever", connection))
                {
                    ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                    int revID = ri.ReviewId;
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@srcID", _SourceId));
                    command.Parameters.Add(new SqlParameter("@revID", revID));
                    command.CommandTimeout = 60;
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

#endif
    }
}
