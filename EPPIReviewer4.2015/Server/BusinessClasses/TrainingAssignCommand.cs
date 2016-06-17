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
    public class TrainingAssigncommand : CommandBase<TrainingAssigncommand>
    {
#if SILVERLIGHT
    public TrainingAssigncommand(){}
#else
        protected TrainingAssigncommand() { }
#endif

        private bool _included;
        private Int64 _attributeId;
        private int _setId;
        private int _trainingId;

        public TrainingAssigncommand(Int64 attributeId, int setId, bool included, int trainingId)
        {
            _attributeId = attributeId;
            _setId = setId;
            _included = included;
            _trainingId = trainingId;
        }

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_attributeId", _attributeId);
            info.AddValue("_setId", _setId);
            info.AddValue("_included", _included);
            info.AddValue("_trainingId", _trainingId);
        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            _attributeId = info.GetValue<Int64>("_attributeId");
            _setId = info.GetValue<int>("_setId");
            _included = info.GetValue<bool>("_included");
            _trainingId = info.GetValue<int>("_trainingId");
        }


#if !SILVERLIGHT

        protected override void DataPortal_Execute()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_TrainingItemAttributeBulkInsert", connection))
                {
                    ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.CommandTimeout = 120;
                    command.Parameters.Add(new SqlParameter("@CONTACT_ID", ri.UserId));
                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ID", _attributeId));
                    command.Parameters.Add(new SqlParameter("@SET_ID", _setId));
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@INCLUDED", _included));
                    command.Parameters.Add(new SqlParameter("@TRAINING_ID", _trainingId));
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

#endif
    }
}
