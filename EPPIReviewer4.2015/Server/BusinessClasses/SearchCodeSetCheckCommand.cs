﻿using System;
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
    public class SearchCodeSetCheckCommand : CommandBase<SearchCodeSetCheckCommand>
    {
#if SILVERLIGHT
    public SearchCodeSetCheckCommand(){}
#else
        public SearchCodeSetCheckCommand() { }
#endif

        private int _setId;
        private bool _included;
        private bool _isCoded;
        private string _setName;
        private int _searchId;
        private int _coded_by_contactId;
        private string _coded_by_contactName;

        public int SearchId
        {
            get
            {
                return _searchId;
            }
        }

        public SearchCodeSetCheckCommand(int setId, bool Included, bool isCoded,
            string setName, int codedByContactId, string codedByContactName)
        {
            _setId = setId;
            _included = Included;
            _isCoded = isCoded;
            _setName = setName;
            _coded_by_contactId = codedByContactId;
            _coded_by_contactName = codedByContactName;
        }

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_setId", _setId);
            info.AddValue("_included", _included);
            info.AddValue("_isCoded", _isCoded);
            info.AddValue("_setName", _setName);
            info.AddValue("_searchId", _searchId);
            info.AddValue("_coded_by_contactId", _coded_by_contactId);
            info.AddValue("_coded_by_contactName", _coded_by_contactName);
        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            _setId = info.GetValue<int>("_setId");
            _included = info.GetValue<bool>("_included");
            _isCoded = info.GetValue<bool>("_isCoded");
            _setName = info.GetValue<string>("_setName");
            _searchId = info.GetValue<int>("_searchId");
            _coded_by_contactId = info.GetValue<int>("_coded_by_contactId");
            _coded_by_contactName = info.GetValue<string>("_coded_by_contactName");
        }


#if !SILVERLIGHT

        protected override void DataPortal_Execute()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                using (SqlCommand command = new SqlCommand("st_SearchCodeSetCheck", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@CONTACT_ID", ri.UserId));
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@SET_ID", _setId));
                    command.Parameters.Add(new SqlParameter("@IS_CODED", _isCoded));
                    command.Parameters.Add(new SqlParameter("@INCLUDED", _included));
                    command.Parameters.Add(new SqlParameter("@SEARCH_TITLE", (_isCoded ?
                        "Coded with: " + _setName : "Not coded with: " + _setName) +
                        (_coded_by_contactId == 0 ? "" : " [Coded by " + _coded_by_contactName + "]")));
                    command.Parameters.Add(new SqlParameter("@SEARCH_ID", 0));
                    command.Parameters.Add(new SqlParameter("@CODED_BY_CONTACT_ID", _coded_by_contactId));
                    command.Parameters["@SEARCH_ID"].Direction = System.Data.ParameterDirection.Output;
                    command.ExecuteNonQuery();
                    _searchId = Convert.ToInt32(command.Parameters["@SEARCH_ID"].Value);
                }
                connection.Close();
            }
        }

#endif
    }
}
