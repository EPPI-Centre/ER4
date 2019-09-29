
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
    public class OutcomeItemAttributesCommand : CommandBase<OutcomeItemAttributesCommand>
    {
#if SILVERLIGHT
    public OutcomeItemAttributesCommand(){}
#else
        public OutcomeItemAttributesCommand() { }
#endif

        private string _attributeIds;
        private int _outcomeId;

        public OutcomeItemAttributesCommand(int outcomeId, string attributeIds)
        {
            _outcomeId = outcomeId;
            _attributeIds = attributeIds;
        }

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_outcomeId", _outcomeId);
            info.AddValue("_attributeIds", _attributeIds);
        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            _attributeIds = info.GetValue<string>("_attributeIds");
            _outcomeId = info.GetValue<int>("_outcomeId");
        }


#if !SILVERLIGHT

        protected override void DataPortal_Execute()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_OutcomeItemAttributesSave", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@OUTCOME_ID", _outcomeId));
                    command.Parameters.Add(new SqlParameter("@ATTRIBUTES", _attributeIds));
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

#endif
    }
}
