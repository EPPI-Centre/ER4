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
    public class TrainingStatisticsCommand : CommandBase<TrainingStatisticsCommand>
    {
#if SILVERLIGHT
    public TrainingStatisticsCommand(){}
#else
        protected TrainingStatisticsCommand() { }
#endif
        private int _tp;
        private int _fp;
        private int _tn;
        private int _fn;
        private int _trainingId;

        public int TrainingId
        {
            get
            {
                return _trainingId;
            }
        }

        public int TruePositives
        {
            get
            {
                return _tp;
            }
        }

        public int FalsePositives
        {
            get
            {
                return _fp;
            }
        }

        public int TrueNegatives
        {
            get
            {
                return _tn;
            }
        }

        public int FalseNegatives
        {
            get
            {
                return _fn;
            }
        }

        public double FRatio
        {
            get
            {
                if (Precision != 0 || Sensitivity != 0)
                {
                    return 2 * (Precision * Sensitivity) / (Precision + Sensitivity);
                }
                else
                {
                    return 0;
                }
            }
        }

        public double Sensitivity
        {
            get
            {
                if (_tp != 0 || _fn != 0)
                {
                    return Convert.ToDouble(_tp) / Convert.ToDouble(_tp + _fn);
                }
                else
                {
                    return 0;
                }
            }
        }

        public double Specificity
        {
            get
            {
                if (_tn != 0 || _fp != 0)
                {
                    return Convert.ToDouble(_tn) / Convert.ToDouble(_tn + _fp);
                }
                else
                {
                    return 0;
                }
            }
        }

        public double Precision
        {
            get
            {
                if (_tp != 0 || _fp != 0)
                {
                    return Convert.ToDouble(_tp) / Convert.ToDouble(_tp + _fp);
                }
                else
                {
                    return 0;
                }
            }
        }

        public TrainingStatisticsCommand(int TrainingId)
        {
            _trainingId = TrainingId;
        }

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_tp", _tp);
            info.AddValue("_fp", _fp);
            info.AddValue("_tn", _tn);
            info.AddValue("_fn", _fn);
            info.AddValue("_trainingId", _trainingId);
        }

        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            _tp = info.GetValue<int>("_tp");
            _fp = info.GetValue<int>("_fp");
            _tn = info.GetValue<int>("_tn");
            _fn = info.GetValue<int>("_fn");
            _trainingId = info.GetValue<int>("_trainingId");
        }


#if !SILVERLIGHT

        protected override void DataPortal_Execute()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                using (SqlCommand command = new SqlCommand("st_TrainingStatistics", connection))
                {
                    command.CommandTimeout = 600;
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@TRAINING_ID", _trainingId));
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@TRUE_POSITIVE", 0));
                    command.Parameters.Add(new SqlParameter("@FALSE_POSITIVE", 0));
                    command.Parameters.Add(new SqlParameter("@TRUE_NEGATIVE", 0));
                    command.Parameters.Add(new SqlParameter("@FALSE_NEGATIVE", 0));
                    command.Parameters["@TRUE_POSITIVE"].Direction = System.Data.ParameterDirection.Output;
                    command.Parameters["@FALSE_POSITIVE"].Direction = System.Data.ParameterDirection.Output;
                    command.Parameters["@TRUE_NEGATIVE"].Direction = System.Data.ParameterDirection.Output;
                    command.Parameters["@FALSE_NEGATIVE"].Direction = System.Data.ParameterDirection.Output;
                    command.ExecuteNonQuery();
                    _tp = Convert.ToInt32(command.Parameters["@TRUE_POSITIVE"].Value);
                    _fp = Convert.ToInt32(command.Parameters["@FALSE_POSITIVE"].Value);
                    _tn = Convert.ToInt32(command.Parameters["@TRUE_NEGATIVE"].Value);
                    _fn = Convert.ToInt32(command.Parameters["@FALSE_NEGATIVE"].Value);
                }
                connection.Close();
            }
        }

#endif
    }
}
