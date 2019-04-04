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
    public class ItemTimepointDeleteWarningCommand : CommandBase<ItemTimepointDeleteWarningCommand>
    {
        public ItemTimepointDeleteWarningCommand() { }

        private Int64 _itemId;
        private Int64 _TimepointId;
        private int _numOutcomes;


        public ItemTimepointDeleteWarningCommand(Int64 itemId, Int64 TimepointId)
        {
            _itemId = itemId;
            _TimepointId = TimepointId;
        }

        public int NumOutcomes
        {
            get { return _numOutcomes; }
        }

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_itemId", _itemId);
            info.AddValue("_TimepointId", _TimepointId);
            info.AddValue("_numOutcomes", _numOutcomes);
        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            _itemId = info.GetValue<Int64>("_itemId");
            _TimepointId = info.GetValue<Int64>("_TimepointId");
            _numOutcomes = info.GetValue<int>("_numOutcomes");
        }


#if !SILVERLIGHT

        protected override void DataPortal_Execute()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ItemTimepointDeleteWarning", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;

                    SqlParameter output2 = new SqlParameter("@NUM_OUTCOMES", System.Data.SqlDbType.Int);
                    output2.Direction = System.Data.ParameterDirection.Output;
                    command.Parameters.Add(output2);

                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@Timepoint_ID", _TimepointId));
                    command.Parameters.Add(new SqlParameter("@ITEM_ID", _itemId));
                    command.ExecuteNonQuery();
                    
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
