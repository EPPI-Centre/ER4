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
    public class ItemArmDeleteWarningCommand : CommandBase<ItemArmDeleteWarningCommand>
    {
        public ItemArmDeleteWarningCommand() { }

        private Int64 _itemId;
        private Int64 _armId;
        private int _numCodings;


        public ItemArmDeleteWarningCommand(Int64 itemId, Int64 armId)
        {
            _itemId = itemId;
            _armId = armId;
        }

        public int NumCodings
        {
            get { return _numCodings; }
        }

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_itemId", _itemId);
            info.AddValue("_armId", _armId);
            info.AddValue("_numCodings", _numCodings);
        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            _itemId = info.GetValue<Int64>("_itemId");
            _armId = info.GetValue<Int64>("_armId");
            _numCodings = info.GetValue<int>("_numCodings");
        }


#if !SILVERLIGHT

        protected override void DataPortal_Execute()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ItemArmDeleteWarning", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    SqlParameter output = new SqlParameter("@NUM_CODINGS", System.Data.SqlDbType.Int);
                    output.Direction = System.Data.ParameterDirection.Output;
                    command.Parameters.Add(output);

                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@ARM_ID", _armId));
                    command.Parameters.Add(new SqlParameter("@ITEM_ID", _itemId));
                    command.ExecuteNonQuery();
                    int? tmp = output.Value as int?;
                    if (tmp == null) _numCodings = 0;
                    else _numCodings = (int)tmp;
                }
                connection.Close();
            }
        }

#endif
    }
}
