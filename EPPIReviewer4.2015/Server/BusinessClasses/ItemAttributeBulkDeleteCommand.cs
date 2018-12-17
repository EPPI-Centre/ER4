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
    public class ItemAttributeBulkDeleteCommand : CommandBase<ItemAttributeBulkDeleteCommand>
    {
#if SILVERLIGHT
    public ItemAttributeBulkDeleteCommand(){}
#else
        protected ItemAttributeBulkDeleteCommand() { }
#endif

        private Int64 _attributeId;
        private string _itemIds;
        private int _setId;
        private string _SearchIds;

        public Int64 AttributeId
        {
            get { return _attributeId; }
        }

        public string ItemIds
        {
            get { return _itemIds; }
        }

        public ItemAttributeBulkDeleteCommand(Int64 attributeId, string itemIds, int setId, string searchIds)
        {
            _attributeId = attributeId;
            _itemIds = itemIds;
            _setId = setId;
            _SearchIds = searchIds;
        }

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_attributeId", _attributeId);
            info.AddValue("_itemIds", _itemIds);
            info.AddValue("_setId", _setId);
            info.AddValue("_SearchIds", _SearchIds);
        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            _attributeId = info.GetValue<Int64>("_attributeId");
            _itemIds = info.GetValue<string>("_itemIds");
            _setId = info.GetValue<int>("_setId");
            _SearchIds = info.GetValue<string>("_SearchIds");
        }


#if !SILVERLIGHT

        protected override void DataPortal_Execute()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ItemAttributeBulkDelete", connection))
                {
                    ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ID", _attributeId));
                    command.Parameters.Add(new SqlParameter("@ITEM_ID_LIST", _itemIds));
                    command.Parameters.Add(new SqlParameter("@SEARCH_ID_LIST", _SearchIds));
                    command.Parameters.Add(new SqlParameter("@SET_ID", _setId));
                    command.Parameters.Add(new SqlParameter("@CONTACT_ID", ri.UserId));
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

#endif
    }
}
