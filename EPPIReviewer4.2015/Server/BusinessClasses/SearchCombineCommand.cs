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
    public class SearchCombineCommand : CommandBase<SearchCombineCommand>
    {
#if SILVERLIGHT
    public SearchCombineCommand(){}
#else
        protected SearchCombineCommand() { }
#endif

        private string _title;
        private string _searches;
        private string _combineType;
        private bool _included;

        public SearchCombineCommand(string Title, string Searches, string CombineType, bool Included)
        {
            _title = Title;
            _searches = Searches;
            _combineType = CombineType;
            _included = Included;
        }

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_title", _title);
            info.AddValue("_searches", _searches);
            info.AddValue("_combineType", _combineType);
            info.AddValue("_included", _included);
        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            _title = info.GetValue<string>("_title");
            _searches = info.GetValue<string>("_searches");
            _combineType = info.GetValue<string>("_combineType");
            _included = info.GetValue<bool>("_included");
        }


#if !SILVERLIGHT

        protected override void DataPortal_Execute()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                using (SqlCommand command = new SqlCommand("st_SearchCombine", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@CONTACT_ID", ri.UserId));
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@SEARCH_TITLE", _title));
                    command.Parameters.Add(new SqlParameter("@SEARCHES", _searches));
                    command.Parameters.Add(new SqlParameter("@COMBINE_TYPE", _combineType));
                    command.Parameters.Add(new SqlParameter("@INCLUDED", _included));
                    command.Parameters.Add(new SqlParameter("@SEARCH_ID", 0));
                    command.Parameters["@SEARCH_ID"].Direction = System.Data.ParameterDirection.Output;
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

#endif
    }
}
