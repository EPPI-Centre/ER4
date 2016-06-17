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
    public class SearchDeleteCommand : CommandBase<SearchDeleteCommand>
    {
#if SILVERLIGHT
    public SearchDeleteCommand(){}
#else
        protected SearchDeleteCommand() { }
#endif

        private string _searches;


        public SearchDeleteCommand(string searches)
        {
            _searches = searches;
        }

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_searches", _searches);

        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            _searches = info.GetValue<string>("_searches");
        }

#if !SILVERLIGHT

        protected override void DataPortal_Execute()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_SearchDelete", connection))
                {
                    ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@SEARCHES", _searches));
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

#endif
    }
}
