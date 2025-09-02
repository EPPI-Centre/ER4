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
    public class SearchFromOpenAlexImportCommand : CommandBase<SearchFromOpenAlexImportCommand>
    {
        private string _title = "";
        private int _searchId;
        private Int64 _magAutoUpdateRunId;
        private string _IncEx = "";
        private string _ScoresToUse = "A";

        public SearchFromOpenAlexImportCommand() { }
        public SearchFromOpenAlexImportCommand(string title, Int64 magAutoUpdateRunId, string IncEx, string ScoresToUse) {
            _title = title;
            _magAutoUpdateRunId = magAutoUpdateRunId;
            _IncEx = IncEx;
            _ScoresToUse = ScoresToUse;
        }

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
        // _magAutoUpdateRunId _IncEx _ScoresToUse


        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_title", _title);
            info.AddValue("_searchId", _searchId);
            info.AddValue("_magAutoUpdateRunId", _magAutoUpdateRunId);
            info.AddValue("_IncEx", _IncEx);
            info.AddValue("_ScoresToUse", _ScoresToUse);
        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            _title = info.GetValue<string>("_title");
            _searchId = info.GetValue<int>("_searchId");
            _magAutoUpdateRunId = info.GetValue<Int64>("_magAutoUpdateRunId");
            _IncEx = info.GetValue<string>("_IncEx");
            _ScoresToUse = info.GetValue<string>("_ScoresToUse");
        }


#if !SILVERLIGHT

        protected override void DataPortal_Execute()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                using (SqlCommand command = new SqlCommand("st_SearchFromOpenAlexAutoUpdate", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@CONTACT_ID", ri.UserId));
                    command.Parameters.Add(new SqlParameter("@SEARCH_TITLE", _title));
                    command.Parameters.Add(new SqlParameter("@MAG_AUTO_UPDATE_RUN_ID", _magAutoUpdateRunId));
                    command.Parameters.Add(new SqlParameter("@ScoresToUse", _ScoresToUse));
                    command.Parameters.Add(new SqlParameter("@IncExc", _IncEx));
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
