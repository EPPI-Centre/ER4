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
#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class ReviewSetUpdateCommand : CommandBase<ReviewSetUpdateCommand>
    {
    public ReviewSetUpdateCommand(){}

        private int _ReviewSetId;
        private int _SetId;
        private bool _AllowCodingEdits;
        private bool _CodingIsFinal;
        private string _SetName;
        private string _setDescription;
        private int _setOrder;

        public ReviewSetUpdateCommand(int reviewSetId, int setId, bool allowCodingEdits, bool codingIsFinal, string setName, int SetOrder, string setDescription)
        {
            _ReviewSetId = reviewSetId;
            _SetId = setId;
            _AllowCodingEdits = allowCodingEdits;
            _CodingIsFinal = codingIsFinal;
            _SetName = setName;
            _setOrder = SetOrder;
            _setDescription = setDescription;
        }

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_ReviewSetId", _ReviewSetId);
            info.AddValue("_SetId", _SetId);
            info.AddValue("_AllowCodingEdits", _AllowCodingEdits);
            info.AddValue("_CodingIsFinal", _CodingIsFinal);
            info.AddValue("_SetName", _SetName);
            info.AddValue("_setOrder", _setOrder);
            info.AddValue("_setDescription", _setDescription);
        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            _ReviewSetId = info.GetValue<int>("_ReviewSetId");
            _SetId = info.GetValue<int>("_SetId");
            _AllowCodingEdits = info.GetValue<bool>("_AllowCodingEdits");
            _CodingIsFinal = info.GetValue<bool>("_CodingIsFinal");
            _SetName = info.GetValue<string>("_SetName");
            _setOrder = info.GetValue<int>("_setOrder");
            _setDescription = info.GetValue<string>("_setDescription");
        }


#if !SILVERLIGHT

        protected override void DataPortal_Execute()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ReviewSetUpdate", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_SET_ID", _ReviewSetId));
                    command.Parameters.Add(new SqlParameter("@SET_ID", _SetId));
                    command.Parameters.Add(new SqlParameter("@ALLOW_CODING_EDITS", _AllowCodingEdits));
                    command.Parameters.Add(new SqlParameter("@CODING_IS_FINAL", _CodingIsFinal));
                    command.Parameters.Add(new SqlParameter("@SET_NAME", _SetName));
                    command.Parameters.Add(new SqlParameter("@SET_ORDER", _setOrder));
                    command.Parameters.Add(new SqlParameter("@SET_DESCRIPTION", _setDescription));
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

#endif
    }
}
