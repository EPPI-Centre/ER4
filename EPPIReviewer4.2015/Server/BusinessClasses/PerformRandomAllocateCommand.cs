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
    public class PerformRandomAllocateCommand : CommandBase<PerformRandomAllocateCommand>
    {
#if SILVERLIGHT
    public PerformRandomAllocateCommand(){}
#else
        protected PerformRandomAllocateCommand() { }
#endif

        private string _filterType;
        private Int64 _attributeIdFilter;
        private int _setIdFilter;
        private Int64 _attributeId;
        private int _setId;
        private int _howMany;
        private int _sampleNo;
        private bool _included;

        public PerformRandomAllocateCommand(string filterType, Int64 attributeIdFilter, int setIdFilter, Int64 attributeId, int setId, int howMany, int sampleNo, bool included)
        {
            _filterType = filterType;
            _attributeIdFilter = attributeIdFilter;
            _setIdFilter = setIdFilter;
            _attributeId = attributeId;
            _setId = setId;
            _howMany = howMany;
            _sampleNo = sampleNo;
            _included = included;
        }

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_filterType", _filterType);
            info.AddValue("_attributeIdFilter", _attributeIdFilter);
            info.AddValue("_setIdFilter", _setIdFilter);
            info.AddValue("_attributeId", _attributeId);
            info.AddValue("_setId", _setId);
            info.AddValue("_howMany", _howMany);
            info.AddValue("_sampleNo", _sampleNo);
            info.AddValue("_included", _included);
        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            _filterType = info.GetValue<string>("_filterType");
            _attributeIdFilter = info.GetValue<Int64>("_attributeIdFilter");
            _setIdFilter = info.GetValue<int>("_setIdFilter");
            _attributeId = info.GetValue<Int64>("_attributeId");
            _setId = info.GetValue<int>("_setId");
            _howMany = info.GetValue<int>("_howMany");
            _sampleNo = info.GetValue<int>("_sampleNo");
            _included = info.GetValue<bool>("_included");
        }


#if !SILVERLIGHT

        protected override void DataPortal_Execute()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                using (SqlCommand command = new SqlCommand("st_RandomAllocate", connection))
                {
                    command.CommandTimeout = 500; // if you are allocating thousands of items, it can take a while
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@CONTACT_ID", ri.UserId));
                    command.Parameters.Add(new SqlParameter("@FILTER_TYPE", _filterType));
                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ID_FILTER", _attributeIdFilter));
                    command.Parameters.Add(new SqlParameter("@SET_ID_FILTER", _setIdFilter));
                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ID", _attributeId));
                    command.Parameters.Add(new SqlParameter("@SET_ID", _setId));
                    command.Parameters.Add(new SqlParameter("@HOW_MANY", _howMany));
                    command.Parameters.Add(new SqlParameter("@SAMPLE_NO", _sampleNo));
                    command.Parameters.Add(new SqlParameter("@INCLUDED", _included));
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

#endif
    }
}
