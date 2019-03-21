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
    public class ComparisonStatsCommand : CommandBase<ComparisonStatsCommand>
    {
#if SILVERLIGHT
        public ComparisonStatsCommand() { }
#else
		public ComparisonStatsCommand() { }
#endif

        private int _comparisonId;
        private int _N1vs2;
        private int _disagreements1vs2;
        private int _N2vs3;
        private int _disagreements2vs3;
        private int _N1vs3;
        private int _disagreements1vs3;
        private int _Ncoded1;
        private int _Ncoded2;
        private int _Ncoded3;
        private bool _CanComplete1vs2;
        private bool _CanComplete1vs3;
        private bool _CanComplete2vs3;

        private bool _isScreening;

        //additional data for comparisons
        private int _Scdisagreements1vs2 = 0;
        private int _Scdisagreements2vs3 = 0;
        private int _Scdisagreements1vs3 = 0;
        
        public ComparisonStatsCommand(int comparisonId)
        {
            _comparisonId = comparisonId;
        }
        public bool isScreening
        {
            get { return _isScreening; }
        }
        public int N1vs2
        {
            get { return _N1vs2; }
        }

        public int N2vs3
        {
            get { return _N2vs3; }
        }

        public int N1vs3
        {
            get { return _N1vs3; }
        }

        public int Disagreements1vs2
        {
            get { return _disagreements1vs2; }
        }

        public int Disagreements2vs3
        {
            get { return _disagreements2vs3; }
        }

        public int Disagreements1vs3
        {
            get { return _disagreements1vs3; }
        }

        public int NCoded1
        {
            get { return _Ncoded1; }
        }

        public int NCoded2
        {
            get { return _Ncoded2; }
        }

        public int NCoded3
        {
            get { return _Ncoded3; }
        }
        public bool CanComplete1vs2
        {
            get { return _CanComplete1vs2; }
        }
        public bool CanComplete1vs3
        {
            get { return _CanComplete1vs3; }
        }
        public bool CanComplete2vs3
        {
            get { return _CanComplete2vs3; }
        }

        //additional data for comparisons
        public int ScDisagreements1vs2
        {
            get { return _Scdisagreements1vs2; }
        }

        public int ScDisagreements2vs3
        {
            get { return _Scdisagreements2vs3; }
        }

        public int ScDisagreements1vs3
        {
            get { return _Scdisagreements1vs3; }
        }

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_comparisonId", _comparisonId);
            info.AddValue("_N1vs2", _N1vs2);
            info.AddValue("_N2vs3", _N2vs3);
            info.AddValue("_N1vs3", _N1vs3);
            info.AddValue("_disagreements1vs2", _disagreements1vs2);
            info.AddValue("_disagreements2vs3", _disagreements2vs3);
            info.AddValue("_disagreements1vs3", _disagreements1vs3);
            info.AddValue("_Ncoded1", _Ncoded1);
            info.AddValue("_Ncoded2", _Ncoded2);
            info.AddValue("_Ncoded3", _Ncoded3);
            info.AddValue("_CanComplete1vs2", _CanComplete1vs2);
            info.AddValue("_CanComplete1vs3", _CanComplete1vs3);
            info.AddValue("_CanComplete2vs3", _CanComplete2vs3);
            info.AddValue("_Scdisagreements1vs2", _Scdisagreements1vs2);
            info.AddValue("_Scdisagreements2vs3", _Scdisagreements2vs3);
            info.AddValue("_Scdisagreements1vs3", _Scdisagreements1vs3);
            info.AddValue("_isScreening", _isScreening);
            
        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            _comparisonId = info.GetValue<int>("_comparisonId");
            _N1vs2 = info.GetValue<int>("_N1vs2");
            _N2vs3 = info.GetValue<int>("_N2vs3");
            _N1vs3 = info.GetValue<int>("_N1vs3");
            _disagreements1vs2 = info.GetValue<int>("_disagreements1vs2");
            _disagreements2vs3 = info.GetValue<int>("_disagreements2vs3");
            _disagreements1vs3 = info.GetValue<int>("_disagreements1vs3");
            _Ncoded1 = info.GetValue<int>("_Ncoded1");
            _Ncoded2 = info.GetValue<int>("_Ncoded2");
            _Ncoded3 = info.GetValue<int>("_Ncoded3");
            _CanComplete1vs2 = info.GetValue<bool>("_CanComplete1vs2");
            _CanComplete1vs3 = info.GetValue<bool>("_CanComplete1vs3");
            _CanComplete2vs3 = info.GetValue<bool>("_CanComplete2vs3");
            _Scdisagreements1vs2 = info.GetValue<int>("_Scdisagreements1vs2");
            _Scdisagreements2vs3 = info.GetValue<int>("_Scdisagreements2vs3");
            _Scdisagreements1vs3 = info.GetValue<int>("_Scdisagreements1vs3");
            _isScreening = info.GetValue<bool>("_isScreening");

        }


#if !SILVERLIGHT

        protected override void DataPortal_Execute()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                connection.Open();
                
                using (SqlCommand command = new SqlCommand("st_ComparisonStats", connection))
                {

                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@COMPARISON_ID", _comparisonId));
                    SqlParameter par2 = new SqlParameter("@Is_Screening", System.Data.SqlDbType.Bit);
                    par2.Value = 0;
                    par2.Direction = System.Data.ParameterDirection.Output;
                    command.Parameters.Add(par2);
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        if (reader.Read())
                        {
                            _Ncoded1 = Convert.ToInt32(reader[0].ToString());
                        }
                        reader.NextResult();
                        if (reader.Read())
                        {
                            _Ncoded2 = Convert.ToInt32(reader[0].ToString());
                        }
                        reader.NextResult();
                        if (reader.Read())
                        {
                            _Ncoded3 = Convert.ToInt32(reader[0].ToString());
                        }
                        reader.NextResult();
                        if (reader.Read())
                        {
                            _N1vs2 = Convert.ToInt32(reader[0].ToString());
                        }
                        reader.NextResult();
                        if (reader.Read())
                        {
                            _disagreements1vs2 = Convert.ToInt32(reader[0].ToString());
                        }
                        reader.NextResult();
                        if (reader.Read())
                        {
                            _N2vs3 = Convert.ToInt32(reader[0].ToString());
                        }
                        reader.NextResult();
                        if (reader.Read())
                        {
                            _disagreements2vs3 = Convert.ToInt32(reader[0].ToString());
                        }
                        reader.NextResult();
                        if (reader.Read())
                        {
                            _N1vs3 = Convert.ToInt32(reader[0].ToString());
                        }
                        reader.NextResult();
                        if (reader.Read())
                        {
                            _disagreements1vs3 = Convert.ToInt32(reader[0].ToString());
                        }
                        reader.NextResult();
                        if (reader.Read())
                        {
                            _CanComplete1vs2 = (reader[0].ToString() == "0");
                        }
                        reader.NextResult();
                        if (reader.Read())
                        {
                            _CanComplete1vs3 = (reader[0].ToString() == "0");
                        }
                        reader.NextResult();
                        if (reader.Read())
                        {
                            _CanComplete2vs3 = (reader[0].ToString() == "0");
                        }
                        reader.NextResult();//this finally arrives to the "results" that contain the output parameter
                        _isScreening = (bool)par2.Value;
                    }
                }
                if (_isScreening)
                {
                    using (SqlCommand command = new SqlCommand("st_ComparisonScreeningStats", connection))
                    {

                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@COMPARISON_ID", _comparisonId));
                        SqlParameter par2 = new SqlParameter("@Dis1v2", System.Data.SqlDbType.Int);
                        par2.Value = 0;
                        par2.Direction = System.Data.ParameterDirection.Output;
                        command.Parameters.Add(par2);
                        SqlParameter par3 = new SqlParameter("@Dis2v3", System.Data.SqlDbType.Int);
                        par3.Value = 0;
                        par3.Direction = System.Data.ParameterDirection.Output;
                        command.Parameters.Add(par3);
                        SqlParameter par4 = new SqlParameter("@Dis1v3", System.Data.SqlDbType.Int);
                        par4.Value = 0;
                        par4.Direction = System.Data.ParameterDirection.Output;
                        command.Parameters.Add(par4);
                        SqlParameter par5 = new SqlParameter("@CodesChanged", System.Data.SqlDbType.Bit);
                        //par5.Value = 0;
                        par5.Direction = System.Data.ParameterDirection.Output;
                        command.Parameters.Add(par5);
                        command.ExecuteNonQuery();
                        _Scdisagreements1vs2 = (int)par2.Value;
                        _Scdisagreements2vs3 = (int)par3.Value;
                        _Scdisagreements1vs3 = (int)par4.Value;
                        if ((bool)par5.Value)
                        {
                            _CanComplete1vs2 = false;
                            _CanComplete1vs3 = false;
                            _CanComplete2vs3 = false;
                        }
                    }
                }
                connection.Close();
            }
        }

#endif
    }
}
