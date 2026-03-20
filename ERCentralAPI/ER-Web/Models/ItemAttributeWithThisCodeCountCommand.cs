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
    public class ItemAttributeWithThisCodeCountCommand : CommandBase<AttributeSetDeleteWarningCommand>
    {
        public ItemAttributeWithThisCodeCountCommand() { }

        private Int64 _attributeSetId;
        private int _setId;
        private int _numIncluded;
        private int _numExcluded;
        private int _numDeletedOrShadow;


        public ItemAttributeWithThisCodeCountCommand(Int64 attributeSetId, int setId)
        {
            _attributeSetId = attributeSetId;
            _setId = setId;
        }

        public int numIncluded
        {
            get { return _numIncluded; }
        }
        public int numExcluded
        {
            get { return _numExcluded; }
        }

        public int numDeletedOrShadow
        {
            get { return _numDeletedOrShadow; }
        }

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_attributeSetId", _attributeSetId);
            info.AddValue("_setId", _setId);
            info.AddValue("_numIncluded", _numIncluded);
            info.AddValue("_numExcluded", _numExcluded);
            info.AddValue("_numDeletedOrShadow", _numDeletedOrShadow);
        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            _attributeSetId = info.GetValue<Int64>("_attributeSetId");
            _numIncluded = info.GetValue<int>("_numIncluded");
            _numExcluded = info.GetValue<int>("_numExcluded");
            _numDeletedOrShadow = info.GetValue<int>("_numDeletedOrShadow");
            _setId = info.GetValue<int>("_setId");
        }


#if !SILVERLIGHT

        protected override void DataPortal_Execute()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ItemsWithThisCodeCount", connection))
                {
                    
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                   
                    command.Parameters.Add(new SqlParameter("@ReviewId", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@AttributeSetId", _attributeSetId));
                    command.Parameters.Add(new SqlParameter("@SetId", _setId)); 
                    command.Parameters.Add(new SqlParameter("@IncludedCount", System.Data.SqlDbType.Int));
                    command.Parameters["@IncludedCount"].Direction = System.Data.ParameterDirection.Output;
                    command.Parameters.Add(new SqlParameter("@ExcludedCount", System.Data.SqlDbType.Int));
                    command.Parameters["@ExcludedCount"].Direction = System.Data.ParameterDirection.Output;
                    command.Parameters.Add(new SqlParameter("@DeletedOrShadowCount", System.Data.SqlDbType.Int));
                    command.Parameters["@DeletedOrShadowCount"].Direction = System.Data.ParameterDirection.Output;
                    command.ExecuteNonQuery();
                    _numIncluded = (int)command.Parameters["@IncludedCount"].Value;
                    _numExcluded = (int)command.Parameters["@ExcludedCount"].Value;
                    _numDeletedOrShadow = (int)command.Parameters["@DeletedOrShadowCount"].Value;
                }
                connection.Close();
            }
        }

#endif
    }
}
