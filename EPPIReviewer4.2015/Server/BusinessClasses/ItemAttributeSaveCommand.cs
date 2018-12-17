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
    public class ItemAttributeSaveCommand : CommandBase<ItemAttributeSaveCommand>
    {

    public ItemAttributeSaveCommand(){}


        private string _saveType;
        private Int64 _ItemAttributeId;
        private Int64 _itemSetId;
        private string _additionalText;
        private Int64 _attributeId;
        private int _setId;
        private Int64 _itemId;
        private Int64 _itemArmId;

        public Int64 ItemAttributeId
        {
            get { return _ItemAttributeId; }
        }

        public Int64 ItemSetId
        {
            get { return _itemSetId; }
        }

        public string AdditionalText
        {
            get { return _additionalText;}
        }

        public Int64 AttributeId
        {
            get { return _attributeId; }
        }

        public int SetId
        {
            get { return _setId; }
        }

        public Int64 ItemId
        {
            get { return _itemId; }
        }

        public Int64 ItemArmId
        {
            get { return _itemArmId; }
        }

        public static readonly PropertyInfo<ReviewInfo> RevInfoProperty = RegisterProperty<ReviewInfo>(new PropertyInfo<ReviewInfo>("RevInfo", "RevInfo"));
        public ReviewInfo RevInfo
        {
            get { return ReadProperty(RevInfoProperty); }
            set { LoadProperty(RevInfoProperty, value); }
        }

        public ItemAttributeSaveCommand(string saveType, Int64 itemAttributeId, Int64 itemSetId, string additionalText, Int64 attributeId,
            int setId, Int64 itemId, Int64 itemArmId, ReviewInfo ri)
        {
            _saveType = saveType;
            _ItemAttributeId = itemAttributeId;
            _itemSetId = itemSetId;
            _additionalText = additionalText;
            _attributeId = attributeId;
            _setId = setId;
            _itemId = itemId;
            _itemArmId = itemArmId;
            RevInfo = ri;
        }

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_saveType", _saveType);
            info.AddValue("_ItemAttributeId", _ItemAttributeId);
            info.AddValue("_itemSetId", _itemSetId);
            info.AddValue("_additionalText", _additionalText);
            info.AddValue("_attributeId", _attributeId);
            info.AddValue("_setId", _setId);
            info.AddValue("_itemId", _itemId);
            info.AddValue("_itemArmId", _itemArmId);

        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            _saveType = info.GetValue<string>("_saveType");
            _ItemAttributeId = info.GetValue<Int64>("_ItemAttributeId");
            _itemSetId = info.GetValue<Int64>("_itemSetId");
            _additionalText = info.GetValue<string>("_additionalText");
            _attributeId = info.GetValue<Int64>("_attributeId");
            _setId = info.GetValue<int>("_setId");
            _itemId = info.GetValue<Int64>("_itemId");
            _itemArmId = info.GetValue<Int64>("_itemArmId");
        }


#if !SILVERLIGHT

        protected override void DataPortal_Execute()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ItemAttributeInsert", connection))
                {
                    ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                    int justCheck = ri.ReviewId;
                    
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    switch (_saveType)
                    {
                        case "Update":
                            command.CommandText = "st_ItemAttributeUpdate";
                            command.Parameters.Add(new SqlParameter("@ITEM_ATTRIBUTE_ID", _ItemAttributeId));
                            command.Parameters.Add(new SqlParameter("@ADDITIONAL_TEXT", _additionalText));
                            break;

                        case "Delete":
                            command.Parameters.Add(new SqlParameter("@ITEM_ATTRIBUTE_ID", _ItemAttributeId));
                            command.Parameters.Add(new SqlParameter("@ITEM_SET_ID", _itemSetId));
                            command.CommandText = "st_ItemAttributeDelete";
                            break;

                        case "Insert":
                            command.Parameters.Add(new SqlParameter("@CONTACT_ID", ri.UserId));
                            command.Parameters.Add(new SqlParameter("@ADDITIONAL_TEXT", _additionalText));
                            command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ID", _attributeId));
                            command.Parameters.Add(new SqlParameter("@SET_ID", _setId));
                            command.Parameters.Add(new SqlParameter("@ITEM_ID", _itemId));
                            command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                            command.Parameters.Add(new SqlParameter("@ITEM_ARM_ID", _itemArmId == 0 ? (object)DBNull.Value : _itemArmId));
                            command.Parameters.Add(new SqlParameter("@NEW_ITEM_ATTRIBUTE_ID", 0));
                            command.Parameters["@NEW_ITEM_ATTRIBUTE_ID"].Direction = System.Data.ParameterDirection.Output;
                            command.Parameters.Add(new SqlParameter("@NEW_ITEM_SET_ID", 0));
                            command.Parameters["@NEW_ITEM_SET_ID"].Direction = System.Data.ParameterDirection.Output;
                            break;

                        default:
                            break;
                    }
                    command.ExecuteNonQuery();
                    
                    if (_saveType == "Insert")
                    {
                        _ItemAttributeId = (Int64)command.Parameters["@NEW_ITEM_ATTRIBUTE_ID"].Value;
                        _itemSetId = (Int64)command.Parameters["@NEW_ITEM_SET_ID"].Value;
                    }

                    // auto reconcile / include/exclude for screening
                    if (RevInfo.ShowScreening && SetId == RevInfo.ScreeningCodeSetId) // i.e. we're screening using screening 'rules'
                    {
                        if (_saveType == "Insert")
                        {
                            using (SqlCommand command2 = new SqlCommand("st_ItemAttributeAutoReconcile", connection))
                            {
                                command2.CommandType = System.Data.CommandType.StoredProcedure;
                                command2.Parameters.Add(new SqlParameter("@SET_ID", _setId));
                                command2.Parameters.Add(new SqlParameter("@ITEM_ID", _itemId));
                                command2.Parameters.Add(new SqlParameter("@ATTRIBUTE_ID", _attributeId));
                                command2.Parameters.Add(new SqlParameter("@RECONCILLIATION_TYPE", RevInfo.ScreeningReconcilliation));
                                command2.Parameters.Add(new SqlParameter("@N_PEOPLE", RevInfo.ScreeningNPeople));
                                command2.Parameters.Add(new SqlParameter("@AUTO_EXCLUDE", RevInfo.ScreeningAutoExclude));
                                command2.Parameters.Add(new SqlParameter("@CONTACT_ID", ri.UserId));

                                command2.ExecuteNonQuery();
                            }
                        }
                        if (_saveType == "Delete")
                        {
                            using (SqlCommand command2 = new SqlCommand("st_ItemAttributeAutoReconcileDelete", connection))
                            {
                                command2.CommandType = System.Data.CommandType.StoredProcedure;
                                command2.Parameters.Add(new SqlParameter("@SET_ID", _setId));
                                command2.Parameters.Add(new SqlParameter("@ITEM_ID", _itemId));
                                command2.Parameters.Add(new SqlParameter("@ATTRIBUTE_ID", _attributeId));
                                command2.Parameters.Add(new SqlParameter("@RECONCILLIATION_TYPE", RevInfo.ScreeningReconcilliation));
                                command2.Parameters.Add(new SqlParameter("@N_PEOPLE", RevInfo.ScreeningNPeople));
                                command2.Parameters.Add(new SqlParameter("@AUTO_EXCLUDE", RevInfo.ScreeningAutoExclude));
                                command2.Parameters.Add(new SqlParameter("@CONTACT_ID", ri.UserId));

                                command2.ExecuteNonQuery();
                            }
                        }
                    }
                }
                connection.Close();
            }
        }

#endif
    }
}

/*
 * Rules for saving attributeSets for screening
 * 
 * Single screening: completed by default
 * Comparison screening: no auto-reconcilliation (so no auto exclude)
 * Comparison screening: auto-reconcile at code level.
 *      check whether N reviewers agree on code. If they do, it's completed.
 * Comparison screening: auto-reconcile at include / exclude level.
 *      as above. check whether N reviewers have screened, and if they agree on the include / exclude decision
 * Safety first screening: if any reviewer ticks 'include', it's finalised and removed from the 'todo' list
 * 
 * 
 * Include / exclude automatically - just depends whether something is completed. If this is 'true' and item is complete, then auto-exclude is triggered.
 * 
 * 
*/