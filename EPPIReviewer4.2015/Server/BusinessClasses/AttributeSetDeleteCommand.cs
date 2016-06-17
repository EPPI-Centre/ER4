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
    public class AttributeSetDeleteCommand : CommandBase<AttributeSetDeleteCommand>
    {
#if SILVERLIGHT
    public AttributeSetDeleteCommand(){}
#else
        protected AttributeSetDeleteCommand() { }
#endif

        private Int64 _attributeSetId;
        private Int64 _attributeId;
        private Int64 _parentAttributeId;
        private int _attributeOrder;
        private bool _successful;

        public Int64 AttributeSetId
        {
            get { return _attributeSetId; }
        }

        public Int64 AttributeId
        {
            get { return _attributeId; }
        }

        public Int64 ParentAttributeId
        {
            get { return _parentAttributeId; }
        }

        public bool Successful
        {
            get { return _successful; }
        }

        public int AttributeOrder
        {
            get { return _attributeOrder; }
        }

        public AttributeSetDeleteCommand(Int64 attributeSetId, Int64 parentAttributeId, Int64 attributeId, int attributeOrder)
        {
            _attributeSetId = attributeSetId;
            _attributeId = attributeId;
            _parentAttributeId = parentAttributeId;
            _attributeOrder = attributeOrder;
        }

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_attributeSetId", _attributeSetId);
            info.AddValue("_attributeId", _attributeId);
            info.AddValue("_parentAttributeId", _parentAttributeId);
            info.AddValue("_attributeOrder", _attributeOrder);
            info.AddValue("_successful", _successful);
        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            _attributeSetId = info.GetValue<Int64>("_attributeSetId");
            _attributeId = info.GetValue<Int64>("_attributeId");
            _parentAttributeId = info.GetValue<Int64>("_parentAttributeId");
            _attributeOrder = info.GetValue<int>("_attributeOrder");
            _successful = info.GetValue<bool>("_successful");
        }


#if !SILVERLIGHT

        protected override void DataPortal_Execute()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_AttributeSetDelete", connection))
                {
                    ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.CommandTimeout = 120;
                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_SET_ID", _attributeSetId));
                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ID", _attributeId));
                    command.Parameters.Add(new SqlParameter("@PARENT_ATTRIBUTE_ID", _parentAttributeId));
                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ORDER", _attributeOrder));
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.ExecuteNonQuery();
                }
                connection.Close();
                _successful = true;
            }
        }

#endif
    }
}
