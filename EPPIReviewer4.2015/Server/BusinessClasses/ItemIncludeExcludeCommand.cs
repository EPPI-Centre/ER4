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
    public class ItemIncludeExcludeCommand : CommandBase<ItemIncludeExcludeCommand>
    {
#if SILVERLIGHT
    public ItemIncludeExcludeCommand(){}
#else
        public ItemIncludeExcludeCommand() { }
#endif

        private bool _include;
        private string _itemIds;
        private Int64 _attributeId;
        private int _setId;

        public bool Include
        {
            get { return _include; }
        }

        public string ItemIds
        {
            get { return _itemIds; }
        }

        public ItemIncludeExcludeCommand(bool Include, string itemIds, Int64 attributeId, int setId)
        {
            _include = Include;
            _itemIds = itemIds;
            _attributeId = attributeId;
            _setId = setId;
        }

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_include", _include);
            info.AddValue("_itemIds", _itemIds);
            info.AddValue("_attributeId", _attributeId);
            info.AddValue("_setId", _setId);

        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            _include = info.GetValue<bool>("_include");
            _itemIds = info.GetValue<string>("_itemIds");
            _attributeId = info.GetValue<Int64>("_attributeId");
            _setId = info.GetValue<int>("_setId");
        }


#if !SILVERLIGHT

        protected override void DataPortal_Execute()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ItemIncludeExclude", connection))
                {
                    ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@ITEM_ID_LIST", _itemIds));
                    command.Parameters.Add(new SqlParameter("@INCLUDE", _include));
                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ID", _attributeId));
                    command.Parameters.Add(new SqlParameter("@SET_ID", _setId));
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

#endif
    }
}
