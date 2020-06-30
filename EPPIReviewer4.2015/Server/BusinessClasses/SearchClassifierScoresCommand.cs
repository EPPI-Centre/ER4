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

#if !SILVERLIGHT
using System.Data.SqlClient;
using BusinessLibrary.Data;
using BusinessLibrary.Security;

#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class SearchClassifierScoresCommand : CommandBase<SearchClassifierScoresCommand>
    {
#if SILVERLIGHT
    public SearchClassifierScoresCommand(){}
#else
        public SearchClassifierScoresCommand() { }
#endif

        private string _searchType;
        private string _searchTitle;
        private int _score1;
        private int _score2;
        private int _originalSearchId;
        private int _searchId;

        public int SearchId
        {
            get { return _searchId; }
        }

        public SearchClassifierScoresCommand(string searchType, int score1, int score2, int originalSearchId,
            string searchTitle)
        {
            _searchType = searchType;
            _score1 = score1;
            _score2 = score2;
            _originalSearchId = originalSearchId;
            _searchTitle = searchTitle;
        }

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_searchType", _searchType);
            info.AddValue("_score1", _score1);
            info.AddValue("_score2", _score2);
            info.AddValue("_originalSearchId", _originalSearchId);
            info.AddValue("_searchTitle", _searchTitle);
        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            _searchType = info.GetValue<string>("_searchType");
            _score1 = info.GetValue<int>("_score1");
            _score2 = info.GetValue<int>("_score2");
            _originalSearchId = info.GetValue<int>("_originalSearchId");
            _searchTitle = info.GetValue<string>("_searchTitle");
        }


#if !SILVERLIGHT

        protected override void DataPortal_Execute()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                using (SqlCommand command = new SqlCommand("stSearchClassifierScores", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@CONTACT_ID", ri.UserId));
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@SEARCH_TITLE", _searchTitle));
                    command.Parameters.Add(new SqlParameter("@SEARCH_TYPE", _searchType));
                    command.Parameters.Add(new SqlParameter("@ORIGINAL_SEARCH_ID", _originalSearchId));
                    command.Parameters.Add(new SqlParameter("@SCORE1", _score1));
                    command.Parameters.Add(new SqlParameter("@SCORE2", _score2));
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
