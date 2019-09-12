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
        private int _numOutcomes;


        public ItemArmDeleteWarningCommand(Int64 itemId, Int64 armId)
        {
            _itemId = itemId;
            _armId = armId;
        }

        public int NumCodings
        {
            get { return _numCodings; }
        }

        public int NumOutcomes
        {
            get { return _numOutcomes; }
        }

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_itemId", _itemId);
            info.AddValue("_armId", _armId);
            info.AddValue("_numCodings", _numCodings);
            info.AddValue("_numOutcomes", _numOutcomes);
        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            _itemId = info.GetValue<Int64>("_itemId");
            _armId = info.GetValue<Int64>("_armId");
            _numCodings = info.GetValue<int>("_numCodings");
            _numOutcomes = info.GetValue<int>("_numOutcomes");
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

                    SqlParameter output2 = new SqlParameter("@NUM_OUTCOMES", System.Data.SqlDbType.Int);
                    output2.Direction = System.Data.ParameterDirection.Output;
                    command.Parameters.Add(output2);

                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@ARM_ID", _armId));
                    command.Parameters.Add(new SqlParameter("@ITEM_ID", _itemId));
                    command.ExecuteNonQuery();
                    int? tmp = output.Value as int?;
                    if (tmp == null) _numCodings = 0;
                    else _numCodings = (int)tmp;

                    int? tmp2 = output2.Value as int?;
                    if (tmp2 == null) _numOutcomes = 0;
                    else _numOutcomes = (int)tmp2;
                }
                connection.Close();
            }
        }

#endif
    }
}
