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
    public class AttributeMoveCommand : CommandBase<AttributeMoveCommand>
    {
        public AttributeMoveCommand(){}

        private Int64 _FromId;
        private Int64 _ToId;
        private Int64 _AttributeSetId;
        private int _attributeOrder;

        public AttributeMoveCommand(Int64 fromAttributeSetId, Int64 toAttributeSetId, Int64 AttributeSetId, int attributeOrder)
        {
            _FromId = fromAttributeSetId;
            _ToId = toAttributeSetId;
            _AttributeSetId = AttributeSetId;
            _attributeOrder = attributeOrder;
        }

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_FromId", _FromId);
            info.AddValue("_ToId", _ToId);
            info.AddValue("_AttributeSetId", _AttributeSetId);
            info.AddValue("_attributeOrder", _attributeOrder);
        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            _FromId = info.GetValue<Int64>("_FromId");
            _ToId = info.GetValue<Int64>("_ToId");
            _AttributeSetId = info.GetValue<Int64>("_AttributeSetId");
            _attributeOrder = info.GetValue<int>("_attributeOrder");
        }
        

#if !SILVERLIGHT

        protected override void DataPortal_Execute()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_AttributeSetMove", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@FROM", _FromId));
                    command.Parameters.Add(new SqlParameter("@TO", _ToId));
                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_SET_ID", _AttributeSetId));
                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ORDER", _attributeOrder));
                    command.ExecuteNonQuery(); 
                }
                connection.Close();
            }
        }

#endif
    }
}
