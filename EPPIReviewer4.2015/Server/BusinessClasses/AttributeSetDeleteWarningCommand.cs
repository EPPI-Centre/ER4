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
    public class AttributeSetDeleteWarningCommand : CommandBase<AttributeSetDeleteWarningCommand>
    {
#if SILVERLIGHT
        public AttributeSetDeleteWarningCommand() { }
#else
        protected AttributeSetDeleteWarningCommand() { }
#endif

        private Int64 _attributeSetId;
        private int _setId;
        private Int64 _numItems;


        public AttributeSetDeleteWarningCommand(Int64 attributeSetId, int setId)
        {
            _attributeSetId = attributeSetId;
            _setId = setId;
        }

        public Int64 NumItems
        {
            get { return _numItems; }
        }

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_attributeSetId", _attributeSetId);
            info.AddValue("_setId", _setId);
            info.AddValue("_numItems", _numItems);
        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            _attributeSetId = info.GetValue<Int64>("_attributeSetId");
            _numItems = info.GetValue<Int64>("_numItems");
            _setId = info.GetValue<int>("_setId");
        }


#if !SILVERLIGHT

        protected override void DataPortal_Execute()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_AttributeSetDeleteWarning", connection))
                {
                    if (_setId != 0)
                    {
                        command.CommandText = "st_ReviewSetDeleteWarning";
                    }
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@NUM_ITEMS", 0));
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_SET_ID", _attributeSetId));
                    command.Parameters.Add(new SqlParameter("@SET_ID", _setId));
                    command.Parameters["@NUM_ITEMS"].Direction = System.Data.ParameterDirection.Output;
                    command.ExecuteNonQuery();
                    _numItems = (Int64)command.Parameters["@NUM_ITEMS"].Value;
                }
                connection.Close();
            }
        }

#endif
    }
}
