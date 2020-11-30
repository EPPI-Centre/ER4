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
    public class WebDBDeleteHeaderImageCommand : CommandBase<WebDBDeleteHeaderImageCommand>
    {
    public WebDBDeleteHeaderImageCommand(){}

        private int _WebDBId;
        public int WebDBId
        {
            get { return WebDBId; }
        }
        private short _ImageNumber;
        public short ImageNumber
        {
            get { return _ImageNumber; }
        }

        public WebDBDeleteHeaderImageCommand(int WebDBId, short imageNumber)
        {
            _WebDBId = WebDBId;
            _ImageNumber = imageNumber;
        }

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_WebDBId", _WebDBId);
            info.AddValue("_ImageNumber", _ImageNumber);

        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            _WebDBId = info.GetValue<int>("_WebDBId");
            _ImageNumber = info.GetValue<short>("_ImageNumber");
        }

#if !SILVERLIGHT

        protected override void DataPortal_Execute()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_WebDBDeleteHeaderImage", connection))
                {
                    ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                    int revID = ri.ReviewId;
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@WebDbId", _WebDBId));
                    command.Parameters.Add(new SqlParameter("@RevId", revID));
                    command.Parameters.Add(new SqlParameter("@ImageN", _ImageNumber));
                    //command.CommandTimeout = 60;
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

#endif
    }
}
