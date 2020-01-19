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
    public class MagItemMagRelatedPaperInsertCommand : CommandBase<MagItemMagRelatedPaperInsertCommand>
    {

#if SILVERLIGHT
    public MagItemMagRelatedPaperInsertCommand(){}
#else
        public MagItemMagRelatedPaperInsertCommand() { }
#endif

        private int _MagRelatedRunId;
        private int _NImported;

        public MagItemMagRelatedPaperInsertCommand(int MagRelatedRunId)
        {
            _MagRelatedRunId = MagRelatedRunId;
        }

        public int NImported
        {
            get
            {
                return _NImported;
            }
            set
            {
                _NImported = value;
            }
        }

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_MagRelatedRunId", _MagRelatedRunId);
            info.AddValue("_NImported", _NImported);
        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            _MagRelatedRunId = info.GetValue<int>("_MagRelatedRunId");
            _NImported = info.GetValue<int>("_NImported");
        }


#if !SILVERLIGHT

        protected override void DataPortal_Execute()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.AcademicControllerConnectionString))
            {
                connection.Open();
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                using (SqlCommand command = new SqlCommand("st_ItemMagRelatedPaperInsert", connection))
                {
                    command.CommandTimeout = 2000; // 2000 secs = about 2 hours?
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@MAG_RELATED_RUN_ID", _MagRelatedRunId));
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@SOURCE_NAME", "Automated search: " + DateTime.Now.ToShortDateString() +
                        " at " + DateTime.Now.ToLongTimeString()));
                    command.Parameters.Add(new SqlParameter("@CONTACT_ID", ri.UserId));
                    command.Parameters.Add(new SqlParameter("@N_IMPORTED", 0));
                    command.Parameters["@N_IMPORTED"].Direction = System.Data.ParameterDirection.Output;
                    command.ExecuteNonQuery();
                    _NImported = Convert.ToInt32(command.Parameters["@N_IMPORTED"].Value);
                }
                connection.Close();
            }
        }

#endif


    }
}
