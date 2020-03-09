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
    public class MagDownloadSimulationDataCommand : CommandBase<MagDownloadSimulationDataCommand>
    {

#if SILVERLIGHT
    public MagDownloadSimulationDataCommand(){}
#else
        public MagDownloadSimulationDataCommand() { }
#endif

        private int _MagSimulationId;
        private string _data;

       
        

        
        [Newtonsoft.Json.JsonProperty]
        public int MagSimulationId
        {
            get
            {
                return _MagSimulationId;
            }
        }

        [Newtonsoft.Json.JsonProperty]
        public string Data
        {
            get
            {
                return _data;
            }
        }


        public MagDownloadSimulationDataCommand(int magSimulationId)
        {
            _MagSimulationId = magSimulationId;
        }

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_MagSimulationId", _MagSimulationId);
            info.AddValue("_data", _data);
        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            _MagSimulationId = info.GetValue<int>("_MagSimulationId");
            _data = info.GetValue<string>("_data");
        }


#if !SILVERLIGHT

        protected override void DataPortal_Execute()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                using (SqlCommand command = new SqlCommand("st_MagSimulationResults", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@MagSimulationId", _MagSimulationId));
                    command.Parameters.Add(new SqlParameter("@OrderBy", "Network"));
                    StringBuilder sb = new StringBuilder();
                    sb.Append("SimulationId\tPaperId\tIncluded\tFound\tStudyTypeClassifier\tUserClassifier\tNetworkStatistic\tFieldOfStudyDistance\tEnsembleScore");
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        while (reader.Read())
                        {
                            sb.Append(Environment.NewLine +
                            (!reader.IsDBNull("MAG_SIMULATION_ID") ? reader.GetInt32("MAG_SIMULATION_ID").ToString() : "") + "\t" +
                            (!reader.IsDBNull("PaperId") ? reader.GetInt64("PaperId").ToString() : "") + "\t" +
                            (!reader.IsDBNull("INCLUDED") ? reader.GetBoolean("INCLUDED").ToString() : "") + "\t" +
                            (!reader.IsDBNull("FOUND") ? reader.GetBoolean("FOUND").ToString() : "") + "\t" +
                            (!reader.IsDBNull("STUDY_TYPE_CLASSIFIER_SCORE") ? reader.GetDouble("STUDY_TYPE_CLASSIFIER_SCORE").ToString() : "") + "\t" +
                            (!reader.IsDBNull("USER_CLASSIFIER_MODEL_SCORE") ? reader.GetDouble("USER_CLASSIFIER_MODEL_SCORE").ToString() : "") + "\t" +
                            (!reader.IsDBNull("NETWORK_STATISTIC_SCORE") ? reader.GetDouble("NETWORK_STATISTIC_SCORE").ToString() : "") + "\t" +
                            (!reader.IsDBNull("FOS_DISTANCE_SCORE") ? reader.GetDouble("FOS_DISTANCE_SCORE").ToString() : "") + "\t" +
                            (!reader.IsDBNull("ENSEMBLE_SCORE") ? reader.GetDouble("ENSEMBLE_SCORE").ToString() : ""));
                        }
                    }
                    _data = sb.ToString();
                }
                connection.Close();
            }
        }


#endif


    }
}
