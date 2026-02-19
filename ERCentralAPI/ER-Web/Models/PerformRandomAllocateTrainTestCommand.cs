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
    public class PerformRandomAllocateTrainTestCommand : CommandBase<PerformRandomAllocateTrainTestCommand>
    {
#if SILVERLIGHT
    public PerformRandomAllocateTrainTestCommand(){}
#else
        public PerformRandomAllocateTrainTestCommand() { }
#endif

        private Int64 _attributeIdGoldStandard = 0;
        private Int64 _attributeIdPlaceBelow = 0;
        private int _setIdPlaceBelow = 0;
        private int _howManyToTrain = 0;

        public PerformRandomAllocateTrainTestCommand(Int64 AttributeIdGoldStandard, Int64 AttributeIdPlaceBelow, int SetIdPlaceBelow,
            int HowManyToTrain)
        {
            _attributeIdGoldStandard = AttributeIdGoldStandard;
            _attributeIdPlaceBelow = AttributeIdPlaceBelow;
            _setIdPlaceBelow = SetIdPlaceBelow;
            _howManyToTrain = HowManyToTrain;
        }

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_attributeIdGoldStandard", _attributeIdGoldStandard);
            info.AddValue("_attributeIdPlaceBelow", _attributeIdPlaceBelow);
            info.AddValue("_setIdPlaceBelow", _setIdPlaceBelow);
            info.AddValue("_howManyToTrain", _howManyToTrain);
        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            _attributeIdGoldStandard = info.GetValue<Int64>("_attributeIdGoldStandard");
            _attributeIdPlaceBelow = info.GetValue<Int64>("_attributeIdPlaceBelow");
            _setIdPlaceBelow = info.GetValue<int>("_setIdPlaceBelow");
            _howManyToTrain = info.GetValue<int>("_howManyToTrain");
        }


#if !SILVERLIGHT

        protected override void DataPortal_Execute()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                using (SqlCommand command = new SqlCommand("st_RandomAllocateTrainTest", connection))
                {
                    command.CommandTimeout = 500; // if you are allocating thousands of items, it can take a while
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@CONTACT_ID", ri.UserId));
                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ID_GOLD_STANDARD", _attributeIdGoldStandard));
                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ID_PLACE_BELOW", _attributeIdPlaceBelow));
                    command.Parameters.Add(new SqlParameter("@SET_ID_PLACE_BELOW", _setIdPlaceBelow));
                    command.Parameters.Add(new SqlParameter("@HOW_MANY_TO_TRAIN", _howManyToTrain));
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

#endif
    }
}
