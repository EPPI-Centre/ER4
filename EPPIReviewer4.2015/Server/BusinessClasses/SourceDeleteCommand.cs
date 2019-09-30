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
    public class SourceDeleteCommand : CommandBase<SourceDeleteCommand>
    {
    public SourceDeleteCommand(){}

        private int _SourceId;
        
        public int SourceId
        {
            get { return _SourceId; }
        }

        public SourceDeleteCommand(int SourceId)
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
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            int toCheck = ri.ReviewId;

            if (_SourceId == -1)//we are deleting sourceless items in bulk, use different sproc!
            {
                using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand("st_SourceLessDelete", connection))
                    {
                        
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@rev_ID", toCheck));
                        command.ExecuteNonQuery();
                    }
                    connection.Close();
                }
            }
            else
            {
                using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
                {
					
					connection.Open();
                    using (SqlCommand command = new SqlCommand("st_SourceDelete", connection))
                    {
                        
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@source_ID", _SourceId));
						command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
						command.ExecuteNonQuery();
                    }
                    connection.Close();
                }
            }
        }

#endif
    }
}
