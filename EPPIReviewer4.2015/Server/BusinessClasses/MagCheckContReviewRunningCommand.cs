using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Csla;

using Csla.Serialization;
using Csla.Silverlight;
using System.ComponentModel;
using Csla.DataPortalClient;
using System.Threading;
using System.Configuration;

#if !SILVERLIGHT
using BusinessLibrary.Data;
using BusinessLibrary.Security;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Data.SqlClient;
using System.Data;
#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class MagCheckContReviewRunningCommand : CommandBase<MagCheckContReviewRunningCommand>
    {

#if SILVERLIGHT
    public MagCheckContReviewRunningCommand(){}
#else
        public MagCheckContReviewRunningCommand() { }
#endif

        private string _IsRunning;
        [Newtonsoft.Json.JsonProperty]

        public string IsRunningMessage
        {
            get
            {
                return _IsRunning;
            }
        }

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_IsRunning", _IsRunning);
        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            _IsRunning = info.GetValue<string>("_IsRunning");
        }


#if !SILVERLIGHT

        const string TempPath = @"UserTempUploads/";

        protected override void DataPortal_Execute()
        {
            _IsRunning = "false";
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_MagCheckContReviewRunning", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        if (reader.Read())
                        {
                            _IsRunning = reader.GetString("JOB_STATUS");
                        }
                    }
                }
                connection.Close();
            }
        }

        


#endif


    }


}
