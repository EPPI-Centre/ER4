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
    public class ItemAttributeCountsCommand : CommandBase<ItemAttributeCountsCommand>
    {
#if SILVERLIGHT
        public ItemAttributeCountsCommand() { }
#else
        protected ItemAttributeCountsCommand() { }
#endif

        private string _attributes;

        public string AttributeCounts
        {
            get { return _attributes; }
        }

        public ItemAttributeCountsCommand(string attributes)
        {
            _attributes = attributes;
        }

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_attributes", _attributes);
        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            _attributes = info.GetValue<string>("_attributes");
        }

#if !SILVERLIGHT

        protected override void DataPortal_Execute()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ItemAttributeCounts", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_LIST", _attributes));
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        _attributes = "";
                        while (reader.Read())
                        {
                            if (_attributes == "")
                            {
                                _attributes = reader[0].ToString() + "," + reader[1].ToString();
                            }
                            else
                            {
                                _attributes += "¬" + reader[0].ToString() + "," + reader[1].ToString();
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
