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
    public class SearchForUploadedFilesCommand : CommandBase<SearchForUploadedFilesCommand>
    {
#if SILVERLIGHT
    public SearchForUploadedFilesCommand(){}
#else
        public SearchForUploadedFilesCommand() { }
#endif

        private string _title;
        private bool _included;
        private bool _present;
        private int _searchId;

        public int SearchId
        {
            get
            {
                return _searchId;
            }
        }

        public SearchForUploadedFilesCommand(string Title, bool Included, bool Present)
        {
            _title = Title;
            _included = Included;
            _present = Present;
        }

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_title", _title);
            info.AddValue("_included", _included);
            info.AddValue("_present", _present);
            info.AddValue("_searchId", _searchId);
        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            _title = info.GetValue<string>("_title");
            _included = info.GetValue<bool>("_included");
            _present = info.GetValue<bool>("_present");
            _searchId = info.GetValue<int>("_searchId");
        }


#if !SILVERLIGHT

        protected override void DataPortal_Execute()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                using (SqlCommand command = new SqlCommand("st_SearchForUploadedFiles", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@CONTACT_ID", ri.UserId));
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@SEARCH_TITLE", _title));
                    command.Parameters.Add(new SqlParameter("@INCLUDED", _included));
                    command.Parameters.Add(new SqlParameter("@PRESENT", _present));
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
