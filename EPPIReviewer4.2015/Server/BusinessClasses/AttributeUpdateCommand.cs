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
using BusinessLibrary.Security;

#if !SILVERLIGHT
using System.Data.SqlClient;
using BusinessLibrary.Data;
#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class AttributeUpdateCommand : CommandBase<AttributeUpdateCommand>
    {
        public AttributeUpdateCommand(){}


        private Int64 _attributeId;
        private Int64 _attributeSetId;
        private int _attributeTypeId;
        private string _attributeName;
        private string _attributeDescription;
        private int _attributeOrder;
        private string _extURL;
        private string _extType;

        public AttributeUpdateCommand(Int64 attributeId, Int64 attributeSetId, int attributeTypeId, string attributeName, string attributeDescription,
            int attributeOrder, string extURL, string extType)
        {
            _attributeId = attributeId;
            _attributeSetId = attributeSetId;
            _attributeTypeId = attributeTypeId;
            _attributeName = attributeName;
            _attributeDescription = attributeDescription;
            _attributeOrder = attributeOrder;
            _extURL = extURL;
            _extType = extType;
        }

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_attributeId", _attributeId);
            info.AddValue("_attributeSetId", _attributeSetId);
            info.AddValue("_attributeTypeId", _attributeTypeId);
            info.AddValue("_attributeName", _attributeName);
            info.AddValue("_attributeDescription", _attributeDescription);
            info.AddValue("_attributeOrder", _attributeOrder);
            info.AddValue("_extURL", _extURL);
            info.AddValue("_extType", _extType);
        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            _attributeId = info.GetValue<Int64>("_attributeId");
            _attributeSetId = info.GetValue<Int64>("_attributeSetId");
            _attributeTypeId = info.GetValue<int>("_attributeTypeId");
            _attributeName = info.GetValue<string>("_attributeName");
            _attributeDescription = info.GetValue<string>("_attributeDescription");
            _attributeOrder = info.GetValue<int>("_attributeOrder");
            _extURL = info.GetValue<string>("_extURL");
            _extType = info.GetValue<string>("_extType");
        }


#if !SILVERLIGHT

        protected override void DataPortal_Execute()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_AttributeSetLimitedUpdate", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ID", _attributeId));
                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_SET_ID", _attributeSetId));
                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_TYPE_ID", _attributeTypeId));
                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_NAME", _attributeName));
                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_DESCRIPTION", _attributeDescription));
                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ORDER", _attributeOrder));
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@Ext_URL", _extURL));
                    command.Parameters.Add(new SqlParameter("@Ext_Type", _extType));
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

#endif
    }
}
