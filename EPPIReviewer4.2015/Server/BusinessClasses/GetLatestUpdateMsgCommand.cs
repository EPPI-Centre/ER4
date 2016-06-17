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
    public class GetLatestUpdateMsgCommand : CommandBase<GetLatestUpdateMsgCommand>
    {
#if SILVERLIGHT
    public GetLatestUpdateMsgCommand(){}
#else
        protected GetLatestUpdateMsgCommand() { }
#endif

        private string _Date;
        private string _Description;
        private string _URL;
        private string _VersionN;

        public string Date
        {
            get { return _Date; }
        }
        public string Description
        {
            get { return _Description; }
        }
        public string URL
        {
            get { return _URL; }
        }
        public string VersionN
        {
            get { return _VersionN; }
        }
        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_Date", _Date);
            info.AddValue("_Description", _Description);
            info.AddValue("_URL", _URL);
            info.AddValue("_VersionN", _VersionN);
        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            _Date = info.GetValue<string>("_Date");
            _Description = info.GetValue<string>("_Description");
            _URL = info.GetValue<string>("_URL");
            _VersionN = info.GetValue<string>("_VersionN");
        }
        

#if !SILVERLIGHT

        protected override void DataPortal_Execute()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.AdmConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_GetLatestUpdateMsg", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        while (reader.Read())
                        {

                            SmartDate sd = reader.GetSmartDate("DATE");
                            _Date = sd.Date.ToString(new System.Globalization.CultureInfo("en-GB"));
                            //reader.GetSmartDate("DATE").Text;
                            _VersionN = reader.GetString("VERSION_NUMBER");
                            _Description = reader.GetString("DESCRIPTION");
                            _URL = reader.GetString("URL");
                        }
                    }
                }
                connection.Close();
            }
        }

#endif
    }
}
