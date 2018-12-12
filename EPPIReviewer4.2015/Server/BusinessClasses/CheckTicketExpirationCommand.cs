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
    public class CheckTicketExpirationCommand : CommandBase<CheckTicketExpirationCommand>
    {
#if SILVERLIGHT
    public CheckTicketExpirationCommand(){}
#else
        public CheckTicketExpirationCommand() { }
#endif

        private string _GUID;
        private string _Result;
        private string _ServerMessage;
        private int _userId;

        public string GUID
        {
            get { return _GUID; }
            set { _GUID = value; }
        }

        public string Result
        {
            get { return _Result; }
        }

        public string ServerMessage
        {
            get { return _ServerMessage; }
        }

        public int userId
        {
            get { return _userId; }
        }

        public CheckTicketExpirationCommand(int userId, string GUID)
        {
            _userId = userId;
            _GUID = GUID;
            _Result = "";
            _ServerMessage = "";
        }

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_GUID", _GUID);
            info.AddValue("_Result", _Result);
            info.AddValue("_ServerMessage", _ServerMessage);
            info.AddValue("_userId", _userId);

        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            _GUID = info.GetValue<string>("_GUID");
            _Result = info.GetValue<string>("_Result");
            _ServerMessage = info.GetValue<string>("_ServerMessage");
            _userId = info.GetValue<int>("_userId");
        }

#if !SILVERLIGHT

        protected override void DataPortal_Execute()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.AdmConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_LogonTicket_Check_Expiration", connection))
                {
                    ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@c_ID", _userId));
                    command.Parameters.Add(new SqlParameter("@guid", new System.Data.SqlTypes.SqlGuid(_GUID)));
                    command.Parameters.Add(new SqlParameter("@result", _Result));
                    command.Parameters["@result"].SqlDbType = System.Data.SqlDbType.NVarChar;
                    command.Parameters["@result"].Size = 9;
                    command.Parameters["@result"].Direction = System.Data.ParameterDirection.Output;
                    command.Parameters.Add(new SqlParameter("@message", _ServerMessage));
                    command.Parameters["@message"].SqlDbType = System.Data.SqlDbType.NVarChar;
                    command.Parameters["@message"].Size = 4000;
                    command.Parameters["@message"].Direction = System.Data.ParameterDirection.Output;
                    command.ExecuteNonQuery();
                    _Result = command.Parameters["@result"].Value.ToString();
                    _ServerMessage = command.Parameters["@message"].Value.ToString();
                }
                connection.Close();
            }
        }

#endif
    }
}
