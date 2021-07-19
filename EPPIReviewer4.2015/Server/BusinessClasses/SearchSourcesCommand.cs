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
    public class SearchSourcesCommand : CommandBase<SearchSourcesCommand>
    {
#if SILVERLIGHT
    public SearchCodesCommand(){}
#else
        public SearchSourcesCommand() { }
#endif

        private string _title;
        private bool _included;
        private string _sourceIds;
        private int _searchId;
        private bool _deleted;
        private bool _duplicates;

        public int SearchId
        {
            get
            {
                return _searchId;
            }
        }

        public SearchSourcesCommand(string title, bool included, string sourceIds, int searchId, bool deleted, bool duplicates)
        {
            _title = title;
            _included = included;
            _sourceIds = sourceIds;
            _searchId = searchId;
            _deleted = deleted;
            _duplicates = duplicates;
        }

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_title", _title);
            info.AddValue("_included", _included);
            info.AddValue("_sourceIds", _sourceIds);
            info.AddValue("_searchId", _searchId);
            info.AddValue("_deleted", _deleted);
            info.AddValue("_duplicates", _duplicates);
        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            _title = info.GetValue<string>("_title");
            _included = info.GetValue<bool>("_included");
            _sourceIds = info.GetValue<string>("_sourceIds");
            _searchId = info.GetValue<int>("_searchId");
            _deleted = info.GetValue<bool>("_deleted");
            _duplicates = info.GetValue<bool>("_duplicates");
        }


#if !SILVERLIGHT

        protected override void DataPortal_Execute()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                using (SqlCommand command = new SqlCommand("st_SearchSources", connection))
                {                   
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@CONTACT_ID", ri.UserId));
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@SEARCH_TITLE", _title));
                    command.Parameters.Add(new SqlParameter("@INCLUDED", _included));
                    command.Parameters.Add(new SqlParameter("@DELETED", _deleted));
                    command.Parameters.Add(new SqlParameter("@DUPLICATES", _duplicates));
                    command.Parameters.Add(new SqlParameter("@SOURCE_IDS", _sourceIds));
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

