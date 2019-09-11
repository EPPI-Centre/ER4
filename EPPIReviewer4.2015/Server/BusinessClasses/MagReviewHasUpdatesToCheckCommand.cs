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
    public class MagReviewHasUpdatesToCheckCommand : CommandBase<MagReviewHasUpdatesToCheckCommand>
    {

#if SILVERLIGHT
    public MagReviewHasUpdatesToCheckCommand(){}
#else
        public MagReviewHasUpdatesToCheckCommand() { }
#endif

        private bool _HasUpdates;
        private int _NUpdates;
        [Newtonsoft.Json.JsonProperty]
        public bool HasUpdates
        {
            get
            {
                return _HasUpdates;
            }
        }
        public int NUpdates
        {
            get
            {
                return _NUpdates;
            }
        }

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_HasUpdates", _HasUpdates);
            info.AddValue("_NUpdates", _NUpdates);
        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            _HasUpdates = info.GetValue<bool>("_HasUpdates");
            _NUpdates = info.GetValue<int>("_NUpdates");
        }


#if !SILVERLIGHT

        protected override void DataPortal_Execute()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.AcademicControllerConnectionString))
            {
                connection.Open();
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                using (SqlCommand command = new SqlCommand("st_CheckReviewHasUpdates", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@NUpdates", 0));
                    command.Parameters["@NUpdates"].Direction = System.Data.ParameterDirection.Output;
                    command.ExecuteNonQuery();
                    var n = Convert.ToInt32(command.Parameters["@NUpdates"].Value);
                    _NUpdates = int.Parse(n.ToString());
                    _HasUpdates = _NUpdates > 0 ? true : false;
                }
                connection.Close();
            }
        }


#endif


    }
}
