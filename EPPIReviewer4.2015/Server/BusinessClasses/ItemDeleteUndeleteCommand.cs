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
    public class ItemDeleteUndeleteCommand : CommandBase<ItemDeleteUndeleteCommand>
    {
#if SILVERLIGHT
    public ItemDeleteUndeleteCommand(){}
#else
        protected ItemDeleteUndeleteCommand() { }
#endif

        private bool _delete;
        private string _itemIds;

        public bool delete
        {
            get { return _delete; }
        }

        public string ItemIds
        {
            get { return _itemIds; }
        }

        public ItemDeleteUndeleteCommand(bool Delete, string itemIds)
        {
            _delete = Delete;
            _itemIds = itemIds;
        }

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_delete", _delete);
            info.AddValue("_itemIds", _itemIds);

        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            _delete = info.GetValue<bool>("_delete");
            _itemIds = info.GetValue<string>("_itemIds");
        }


#if !SILVERLIGHT

        protected override void DataPortal_Execute()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ItemReviewDeleteUndelete", connection))
                {
                    ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@ITEM_ID_LIST", _itemIds));
                    command.Parameters.Add(new SqlParameter("@DELETE", _delete));
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

#endif
    }
}
