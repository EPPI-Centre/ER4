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
    public class ComparisonCompleteCommand : CommandBase<ComparisonCompleteCommand>
    {
#if SILVERLIGHT
        public ComparisonCompleteCommand() { }
#else
        public ComparisonCompleteCommand() { }
#endif

        private int _comparisonId;
        private string _whichReviewers;
        private int _numAffected;
        private int _contactId;
        private string _lockCoding = null;

        public ComparisonCompleteCommand(int comparisonId, string WhichReviewers, int contactId)
        {
            _comparisonId = comparisonId;
            _whichReviewers = WhichReviewers;
            _contactId = contactId;
        }
        public ComparisonCompleteCommand(int comparisonId, string WhichReviewers, int contactId, string lockCoding)
        {
            _comparisonId = comparisonId;
            _whichReviewers = WhichReviewers;
            _contactId = contactId;
            _lockCoding = lockCoding;
        }

        public int NumberAffected
        {
            get { return _numAffected; }
        }

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_comparisonId", _comparisonId);
            info.AddValue("_whichReviewers", _whichReviewers);
            info.AddValue("_numAffected", _numAffected);
            info.AddValue("_contactId", _contactId);
            info.AddValue("_lockCoding", _lockCoding);
        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            _comparisonId = info.GetValue<int>("_comparisonId");
            _whichReviewers = info.GetValue<string>("_whichReviewers");
            _numAffected = info.GetValue<int>("_numAffected");
            _contactId = info.GetValue<int>("_contactId");
            _lockCoding = info.GetValue<string>("_lockCoding");
        }


#if !SILVERLIGHT

        protected override void DataPortal_Execute()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ComparisonComplete", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@COMPARISON_ID", _comparisonId));
                    command.Parameters.Add(new SqlParameter("@CONTACT_ID", _contactId));
                    command.Parameters.Add(new SqlParameter("@WHICH_REVIEWERS", _whichReviewers));
                    if (_lockCoding != null && _lockCoding.ToLower() == "true")
                    {
                        command.Parameters.Add(new SqlParameter("@IS_LOCKED", 1));
                    }
                    else if (_lockCoding != null && _lockCoding.ToLower() == "false")
                    {
                        command.Parameters.Add(new SqlParameter("@IS_LOCKED", 0));
                    }
					command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        if (reader.Read())
                        {
                            _numAffected = Convert.ToInt32(reader[0].ToString());
                        }
                    }
                }
                connection.Close();
            }
        }

#endif
    }
}
