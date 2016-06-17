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
    public class GetItemsWithTheseCodesCommand : CommandBase<GetItemsWithTheseCodesCommand>
    {
#if SILVERLIGHT
    public GetItemsWithTheseCodesCommand(){}
#else
        protected GetItemsWithTheseCodesCommand() { }
#endif

        private string _codes;
        private string _codes_from;
        private string _return_codes;

        public GetItemsWithTheseCodesCommand(string codes, string codes_from)
        {
            _codes = codes;
            _codes_from = codes_from;
            _return_codes = "";
        }

        public string Codes
        {
            get { return _codes; }
        }

        public string CodesFrom
        {
            get { return _codes_from; }
        }

        public string ReturnCodes
        {
            get { return _return_codes; }
        }

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_codes", _codes);
            info.AddValue("_codes_from", _codes_from);
            info.AddValue("_return_codes", _return_codes);
        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            _codes = info.GetValue<string>("_codes");
            _codes_from = info.GetValue<string>("_codes_from");
            _return_codes = info.GetValue<string>("_return_codes");
        }


#if !SILVERLIGHT

        protected override void DataPortal_Execute()
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ItemsWithCodes", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@CODES", _codes));
                    command.Parameters.Add(new SqlParameter("@CODES_FROM", _codes_from));
                    _return_codes = "";
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        while (reader.Read())
                        {
                            if (_return_codes == "")
                            {
                                _return_codes = reader[0].ToString();
                            }
                            else
                            {
                                _return_codes += "," + reader[0].ToString();
                            }
                        }
                    }
                    
                }
                connection.Close();
            }
        }

#endif
    }
}
