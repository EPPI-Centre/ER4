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
    public class SearchImportedIDsCommand : CommandBase<SearchImportedIDsCommand>
    {
#if SILVERLIGHT
    public SearchImportedIDsCommand(){}
#else
        public SearchImportedIDsCommand() { }
#endif

        private string _title;
        private string _IDs;
        private bool _included;
        private int _searchId;

        public int SearchId
        {
            get
            {
                return _searchId;
            }
        }
        public string Title
        {
            get { return _title; }
        }
        public SearchImportedIDsCommand(string IDs, bool Included)
        {
            _title = "Search imported IDs: ";
            _IDs = "";
            string[] split = IDs.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            bool done = false, valid = false;
            string temp = "";
            foreach (string s in split)
            {
                temp = s.Trim();
                if (temp.Length > 0)
                {
                    valid = true;
                    _IDs += temp + ",";
                    if (_title.Length < 40)
                    {
                        _title += temp + ", ";
                    }
                    else if (!done)
                    {
                        _title = _title.Trim(',', ' ') + " [...]";
                        done = true;
                    }
                }
            }
            _IDs = _IDs.Trim(',', ' ');
            if (valid)
            {
                _title = _title.Trim(',', ' ');
                _title += " (" + split.Length.ToString() + " values entered)";
            }
            else
            {
                _title = "No valid ID was found";
            }
            _included = Included;
        }

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_title", _title);
            info.AddValue("_IDs", _IDs);
            info.AddValue("_included", _included);
            info.AddValue("_searchId", _searchId);
        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            _title = info.GetValue<string>("_title");
            _IDs = info.GetValue<string>("_IDs");
            _included = info.GetValue<bool>("_included");
            _searchId = info.GetValue<int>("_searchId");
        }


#if !SILVERLIGHT

        protected override void DataPortal_Execute()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                using (SqlCommand command = new SqlCommand("st_SearchImportedIDs", connection))
                {

                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@CONTACT_ID", ri.UserId));
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@SEARCH_TITLE", _title));
                    command.Parameters.Add(new SqlParameter("@ITEM_ID_LIST", _IDs));
                    command.Parameters.Add(new SqlParameter("@INCLUDED", _included));
                    command.Parameters.Add(new SqlParameter("@SEARCH_ID", 0));
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
