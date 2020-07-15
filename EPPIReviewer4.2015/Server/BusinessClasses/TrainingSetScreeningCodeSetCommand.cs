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
//using SVM;
using System.IO;

#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class TrainingSetScreeningCodeSetCommand : CommandBase<TrainingSetScreeningCodeSetCommand>
    {
        public TrainingSetScreeningCodeSetCommand() { }

        private int _codeSetId;

        public TrainingSetScreeningCodeSetCommand(int CodeSetId)
        {
            _codeSetId = CodeSetId;
        }

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_codeSetId", _codeSetId);
        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            _codeSetId = info.GetValue<int>("_codeSetId");
        }


#if !SILVERLIGHT

        protected override void DataPortal_Execute()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                using (SqlCommand command = new SqlCommand("st_TrainingSetScreeningCodeSet", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@CODE_SET_ID", _codeSetId));
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

#endif
    }
}
