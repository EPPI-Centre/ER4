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
    public class ItemAttributeBulkSaveCommand : CommandBase<ItemAttributeBulkSaveCommand>
    {
#if SILVERLIGHT
    public ItemAttributeBulkSaveCommand(){}
#else
        protected ItemAttributeBulkSaveCommand() { }
#endif

        private string _saveType;
        private Int64 _ItemAttributeId;
        private Int64 _itemSetId;
        private Int64 _attributeId;
        private int _setId;
        private string _itemIds;
        private string _SearchIds;

        public Int64 ItemAttributeId
        {
            get { return _ItemAttributeId; }
        }

        public Int64 ItemSetId
        {
            get { return _itemSetId; }
        }

        public Int64 AttributeId
        {
            get { return _attributeId; }
        }

        public int SetId
        {
            get { return _setId; }
        }

        public string ItemIds
        {
            get { return _itemIds; }
        }

        public ItemAttributeBulkSaveCommand(string saveType, Int64 attributeId, int setId, string itemIds, string searchIds)
        {
            _saveType = saveType;
            _attributeId = attributeId;
            _setId = setId;
            _itemIds = itemIds;
            _SearchIds = searchIds;
        }

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_saveType", _saveType);
            info.AddValue("_ItemAttributeId", _ItemAttributeId);
            info.AddValue("_itemSetId", _itemSetId);
            info.AddValue("_attributeId", _attributeId);
            info.AddValue("_setId", _setId);
            info.AddValue("_itemIds", _itemIds);
            info.AddValue("_SearchIds", _SearchIds);
        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            _saveType = info.GetValue<string>("_saveType");
            _ItemAttributeId = info.GetValue<Int64>("_ItemAttributeId");
            _itemSetId = info.GetValue<Int64>("_itemSetId");
            _attributeId = info.GetValue<Int64>("_attributeId");
            _setId = info.GetValue<int>("_setId");
            _itemIds = info.GetValue<string>("_itemIds");
            _SearchIds = info.GetValue<string>("_SearchIds");
        }


#if !SILVERLIGHT

        protected override void DataPortal_Execute()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ItemAttributeBulkInsert", connection)) // NB: command also used in the sp used in TrainingAssignCommand
                {
                    ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    switch (_saveType)
                    {
                        case "Delete":
                            command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ID", _ItemAttributeId));
                            command.Parameters.Add(new SqlParameter("@SET_ID", _itemSetId));
                            command.CommandText = "st_ItemAttributeBulkDelete";
                            break;

                        case "Insert":
                            command.Parameters.Add(new SqlParameter("@CONTACT_ID", ri.UserId));
                            command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ID", _attributeId));
                            command.Parameters.Add(new SqlParameter("@SET_ID", _setId));
                            command.Parameters.Add(new SqlParameter("@ITEM_ID_LIST", _itemIds));
                            command.Parameters.Add(new SqlParameter("@SEARCH_ID_LIST", _SearchIds));
                            command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                            command.Parameters.Add(new SqlParameter("@IS_COMPLETED", true));
                            break;

                        default:
                            break;
                    }
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

#endif
    }
}
