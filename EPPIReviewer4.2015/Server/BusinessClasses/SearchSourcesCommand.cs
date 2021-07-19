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
        private string _sourceIds;
        private int _searchId;
        private string _searchWhat;

        public int SearchId
        {
            get
            {
                return _searchId;
            }
        }

        public SearchSourcesCommand(string title, string sourceIds, int searchId, string searchWhat)
        {
            _title = title;
            _sourceIds = sourceIds;
            _searchId = searchId;
            _searchWhat = searchWhat;
        }

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_title", _title);
            info.AddValue("_sourceIds", _sourceIds);
            info.AddValue("_searchId", _searchId);
            info.AddValue("_searchWhat", _searchWhat);
        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            _title = info.GetValue<string>("_title");
            _sourceIds = info.GetValue<string>("_sourceIds");
            _searchId = info.GetValue<int>("_searchId");
            _searchWhat = info.GetValue<string>("_searchWhat");

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
                    command.Parameters.Add(new SqlParameter("@SEARCH_WHAT", _searchWhat));
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

