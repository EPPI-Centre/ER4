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
    public class MagAutoUpdateRunCountResultsCommand : CommandBase<MagAutoUpdateRunCountResultsCommand>
    {

#if SILVERLIGHT
    public MagAutoUpdateRunCountResultsCommand(){}
#else
        public MagAutoUpdateRunCountResultsCommand() { }
#endif

        private int _MagAutoUpdateRunId;
        private double _AutoUpdateScore;
        private double _StudyTypeClassifierScore;
        private double _UserClassifierScore;
        private int _ResultsCount;
        [Newtonsoft.Json.JsonProperty]
        public int ResultsCount
        {
            get
            {
                return _ResultsCount;
            }
        }

        public MagAutoUpdateRunCountResultsCommand(int MagAutoUpdateRunId, double AutoUpdateScore, double StudyTypeClassifierScore,
            double UserClassifierScore)
        {
            _MagAutoUpdateRunId = MagAutoUpdateRunId;
            _AutoUpdateScore = AutoUpdateScore;
            _StudyTypeClassifierScore = StudyTypeClassifierScore;
            _UserClassifierScore = UserClassifierScore;
        }

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_MagAutoUpdateRunId", _MagAutoUpdateRunId);
            info.AddValue("_AutoUpdateScore", _AutoUpdateScore);
            info.AddValue("_StudyTypeClassifierScore", _StudyTypeClassifierScore);
            info.AddValue("_UserClassifierScore", _UserClassifierScore);
            info.AddValue("_ResultsCount", _ResultsCount);
        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            _MagAutoUpdateRunId = info.GetValue<int>("_MagAutoUpdateRunId");
            _AutoUpdateScore = info.GetValue<double>("_AutoUpdateScore");
            _StudyTypeClassifierScore = info.GetValue<double>("_StudyTypeClassifierScore");
            _UserClassifierScore = info.GetValue<double>("_UserClassifierScore");
            _ResultsCount = info.GetValue<int>("_ResultsCount");
        }


#if !SILVERLIGHT

        protected override void DataPortal_Execute()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                using (SqlCommand command = new SqlCommand("st_MagAutoUpdateRunCountResults", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@MagAutoUpdateRunId", _MagAutoUpdateRunId));
                    command.Parameters.Add(new SqlParameter("@AutoUpdateScore", _AutoUpdateScore));
                    command.Parameters.Add(new SqlParameter("@StudyTypeClassifierScore", _StudyTypeClassifierScore));
                    command.Parameters.Add(new SqlParameter("@UserClassifierScore", _UserClassifierScore));
                    command.Parameters.Add(new SqlParameter("@ResultCount", 0));
                    command.Parameters["@ResultCount"].Direction = System.Data.ParameterDirection.Output;
                    command.ExecuteNonQuery();
                    var n = Convert.ToInt32(command.Parameters["@ResultCount"].Value);
                    _ResultsCount = int.Parse(n.ToString());
                }
                connection.Close();
            }
        }


#endif


    }
}
