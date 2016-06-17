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
    public class SearchWeightedTermsCommand : CommandBase<SearchWeightedTermsCommand>
    {
#if SILVERLIGHT
    public SearchWeightedTermsCommand(){}
#else
        protected SearchWeightedTermsCommand() { }
#endif

        private string _title;
        private string _terms;
        private string _answers;
        private string _filterType;
        private int _searchId;
        private bool _included;

        public int SearchId
        {
            get
            {
                return _searchId;
            }
        }

        public SearchWeightedTermsCommand(string Title, string Terms, string Answers, string FilterType, bool Included)
        {
            _title = Title;
            _terms = Terms;
            _answers = Answers;
            _filterType = FilterType;
            _included = Included;
        }

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_title", _title);
            info.AddValue("_terms", _terms);
            info.AddValue("_answers", _answers);
            info.AddValue("_filterType", _filterType);
            info.AddValue("_searchId", _searchId);
            info.AddValue("_included", _included);
        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            _title = info.GetValue<string>("_title");
            _terms = info.GetValue<string>("_terms");
            _answers = info.GetValue<string>("_answers");
            _filterType = info.GetValue<string>("_filterType");
            _searchId = info.GetValue<int>("_searchId");
            _included = info.GetValue<bool>("_included");
        }


#if !SILVERLIGHT

        protected override void DataPortal_Execute()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                try
                {
                    using (SqlCommand command = new SqlCommand("st_SearchWeightedTerms", connection))
                    {
                        
                        command.CommandTimeout = 120;
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@CONTACT_ID", ri.UserId));
                        command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                        command.Parameters.Add(new SqlParameter("@SEARCH_TITLE", _title));
                        command.Parameters.Add(new SqlParameter("@TERMS", "ISABOUT (" + _terms + ")"));
                        command.Parameters.Add(new SqlParameter("@ANSWERS", _answers));
                        command.Parameters.Add(new SqlParameter("@FILTER_TYPE", _filterType));
                        command.Parameters.Add(new SqlParameter("@INCLUDED", _included));
                        command.Parameters.Add(new SqlParameter("@SEARCH_ID", 0));
                        command.Parameters["@SEARCH_ID"].Direction = System.Data.ParameterDirection.Output;
                        command.ExecuteNonQuery();
                        _searchId = Convert.ToInt32(command.Parameters["@SEARCH_ID"].Value);
                    }
                }
                catch
                {
                    using (SqlCommand command = new SqlCommand("st_SearchWeightedTerms", connection))
                    {
                        string Shortened;
                        Shortened = _terms.Substring(0, _terms.IndexOf("), ", (int)(_terms.Length / 2)) + 1);
                        command.CommandTimeout = 240;
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@CONTACT_ID", ri.UserId));
                        command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                        command.Parameters.Add(new SqlParameter("@SEARCH_TITLE", _title));
                        command.Parameters.Add(new SqlParameter("@TERMS", "ISABOUT (" + Shortened + ")"));
                        command.Parameters.Add(new SqlParameter("@ANSWERS", _answers));
                        command.Parameters.Add(new SqlParameter("@FILTER_TYPE", _filterType));
                        command.Parameters.Add(new SqlParameter("@INCLUDED", _included));
                        command.Parameters.Add(new SqlParameter("@SEARCH_ID", 0));
                        command.Parameters["@SEARCH_ID"].Direction = System.Data.ParameterDirection.Output;
                        command.ExecuteNonQuery();
                        _searchId = Convert.ToInt32(command.Parameters["@SEARCH_ID"].Value);
                    }
                }
                connection.Close();
            }
        }

#endif
    }
}
