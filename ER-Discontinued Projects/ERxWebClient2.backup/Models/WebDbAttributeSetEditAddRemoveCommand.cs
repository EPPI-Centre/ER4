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
    public class WebDbAttributeSetEditAddRemoveCommand : CommandBase<WebDbAttributeSetEditAddRemoveCommand>
    {
        public WebDbAttributeSetEditAddRemoveCommand(){}

        private long _attributeId;
        private int _setId;
        private int _webDbId;
        private bool _deleting;
        private string _publicName;
        private string _publicDescription;
        public long attributeId
        {
            get { return _attributeId; }
        }
        public int setId
        {
            get { return _setId; }
        }
        public int webDbId
        {
            get { return _webDbId; }
        }
        public bool deleting
        {
            get { return _deleting; }
        }
        public string publicName
        {
            get { return _publicName; }
        }
        public string publicDescription
        {
            get { return _publicDescription; }
        }

        public WebDbAttributeSetEditAddRemoveCommand(long AttributeId, int SetId, int WebDbId, bool Deleting, string PublicName, string PublicDescription)
        {
            _setId = SetId;
            _attributeId = AttributeId;
            _webDbId = WebDbId;
            _deleting = Deleting;
            _publicDescription = PublicDescription;
            _publicName = PublicName;
        }

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_setId", _setId);
            info.AddValue("_attributeId", _attributeId);
            info.AddValue("_webDbId", _webDbId);
            info.AddValue("_deleting", _deleting);
            info.AddValue("_publicName", _publicName);
            info.AddValue("_publicDescription", _publicDescription);
        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            _setId = info.GetValue<int>("_setId");
            _attributeId = info.GetValue<long>("_attributeId");
            _webDbId = info.GetValue<int>("_webDbId");
            _deleting = info.GetValue<bool>("_deleting");
            _publicName = info.GetValue<string>("_publicName");
            _publicDescription = info.GetValue<string>("_publicDescription");
        }


#if !SILVERLIGHT

        protected override void DataPortal_Execute()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                string cmdText = "st_WebDbAttributeAdd";
                if (_deleting) cmdText = "st_WebDbAttributeDelete";
                else if (_publicName != "" || _publicDescription != "") cmdText = "st_WebDbAttributeEdit";
                using (SqlCommand command = new SqlCommand(cmdText, connection))
                {
                    ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@Set_ID", _setId));
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@WEBDB_ID", _webDbId));
                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ID", _attributeId));
                    if (cmdText == "st_WebDbAttributeEdit")
                    {
                        command.Parameters.Add(new SqlParameter("@Public_Descr", _publicDescription));
                        command.Parameters.Add(new SqlParameter("@Public_Name", _publicName));
                    }
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

#endif
    }
}
