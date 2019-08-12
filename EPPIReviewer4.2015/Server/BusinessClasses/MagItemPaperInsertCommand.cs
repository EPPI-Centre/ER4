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
    public class MagItemPaperInsertCommand : CommandBase<MagItemPaperInsertCommand>
    {

#if SILVERLIGHT
    public MagItemPaperInsertCommand(){}
#else
        public MagItemPaperInsertCommand() { }
#endif

        private string _PaperIds;

        public MagItemPaperInsertCommand(string PaperIds)
        {
            _PaperIds = PaperIds;
        }

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_PaperIds", _PaperIds);
        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            _PaperIds = info.GetValue<string>("_PaperIds");
        }


#if !SILVERLIGHT

        protected override void DataPortal_Execute()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.AcademicControllerConnectionString))
            {
                connection.Open();
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                using (SqlCommand command = new SqlCommand("st_ItemPaperInsert", connection))
                {
                    command.CommandTimeout = 500; // should make this a nice long time
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@PaperIds", _PaperIds));
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@SOURCE_NAME", "Selected items from MAG on " + DateTime.Now.ToShortDateString() +
                        " at " + DateTime.Now.ToLongTimeString()));
                    command.Parameters.Add(new SqlParameter("@CONTACT_ID", ri.UserId));
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

#endif


    }
}
