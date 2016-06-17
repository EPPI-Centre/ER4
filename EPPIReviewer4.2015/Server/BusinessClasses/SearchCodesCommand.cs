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
    public class SearchCodesCommand : CommandBase<SearchCodesCommand>
    {
#if SILVERLIGHT
    public SearchCodesCommand(){}
#else
        protected SearchCodesCommand() { }
#endif

        private string _title;
        private string _answers;
        private bool _included;
        private bool _withCodes;
        private int _searchId;

        public int SearchId
        {
            get
            {
                return _searchId;
            }
        }

        public SearchCodesCommand(string Title, string Answers, bool Included, bool withCodes)
        {
            _title = Title;
            _answers = Answers;
            _included = Included;
            _withCodes = withCodes;
        }

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_title", _title);
            info.AddValue("_answers", _answers);
            info.AddValue("_included", _included);
            info.AddValue("_searchId", _searchId);
            info.AddValue("_withCodes", _withCodes);
        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            _title = info.GetValue<string>("_title");
            _answers = info.GetValue<string>("_answers");
            _included = info.GetValue<bool>("_included");
            _searchId = info.GetValue<int>("_searchId");
            _withCodes = info.GetValue<bool>("_withCodes");
        }


#if !SILVERLIGHT

        protected override void DataPortal_Execute()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                using (SqlCommand command = new SqlCommand("st_SearchCodes", connection))
                {
                    if (_withCodes == false)
                    {
                        command.CommandText = "st_SearchWithoutCodes";
                    }
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@CONTACT_ID", ri.UserId));
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@SEARCH_TITLE", _title));
                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_SET_ID_LIST", _answers));
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
