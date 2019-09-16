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
    public class MagRunSimulationCommand : CommandBase<MagRunSimulationCommand>
    {

#if SILVERLIGHT
    public MagRunSimulationCommand(){}
#else
        public MagRunSimulationCommand() { }
#endif

        private DateTime _DateFrom;
        private DateTime _CreatedDate;
        private Int64 _AttributeId;

        private int _recommended;
        private int _reverse_recommended;
        private int _birecommended;
        private int _bibliography;
        private int _citations;
        private int _bicitations;
        private int _total_recommended;
        private int _total_reverse_recommended;
        private int _total_birecommended;
        private int _total_citations;
        private int _total_bicitations;
        private int _total_bibliography;
        private int _both;
        private int _total_both;
        private int _N_Seeds;
        private int _N_Seeking;

        [Newtonsoft.Json.JsonProperty]
        public int recommended
        {
            get
            {
                return _recommended;
            }
        }
        public int reverse_recommended
        {
            get
            {
                return _reverse_recommended;
            }
        }
        public int birecommended
        {
            get
            {
                return _birecommended;
            }
        }
        public int bibliography
        {
            get
            {
                return _bibliography;
            }
        }
        public int citations
        {
            get
            {
                return _citations;
            }
        }
        public int bicitations
        {
            get
            {
                return _bicitations;
            }
        }
        public int total_recommended
        {
            get
            {
                return _total_recommended;
            }
        }
        public int total_reverse_recommended
        {
            get
            {
                return _total_reverse_recommended;
            }
        }
        public int total_birecommended
        {
            get
            {
                return _total_birecommended;
            }
        }
        public int total_citations
        {
            get
            {
                return _total_citations;
            }
        }
        public int total_bicitations
        {
            get
            {
                return _total_bicitations;
            }
        }
        public int total_bibliography
        {
            get
            {
                return _total_bibliography;
            }
        }
        public int both
        {
            get
            {
                return _both;
            }
        }
        public int total_both
        {
            get
            {
                return _total_both;
            }
        }
        public int N_Seeds
        {
            get
            {
                return _N_Seeds;
            }
        }
        public int N_Seeking
        {
            get
            {
                return _N_Seeking;
            }
        }

        public MagRunSimulationCommand(DateTime dateFrom, DateTime createdDate, Int64 attributeid)
        {
            _recommended = 0;
            _reverse_recommended = 0;
            _birecommended = 0;
            _bibliography = 0;
            _citations = 0;
            _bicitations = 0;
            _total_recommended = 0;
            _total_reverse_recommended = 0;
            _total_birecommended = 0;
            _total_citations = 0;
            _total_bicitations = 0;
            _total_bibliography = 0;
            _both = 0;
            _total_both = 0;

            _DateFrom = dateFrom;
            _CreatedDate = createdDate;
            _AttributeId = attributeid;
        }

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_DateFrom", _DateFrom);
            info.AddValue("_CreatedDate", _CreatedDate);
            info.AddValue("_AttributeId", _AttributeId);
            info.AddValue("_recommended", _recommended);
            info.AddValue("_reverse_recommended", _reverse_recommended);
            info.AddValue("_birecommended", _birecommended);
            info.AddValue("_bibliography", _bibliography);
            info.AddValue("_citations", _citations);
            info.AddValue("_bicitations", _bicitations);
            info.AddValue("_total_recommended", _total_recommended);
            info.AddValue("_total_reverse_recommended", _total_reverse_recommended);
            info.AddValue("_total_birecommended", _total_birecommended);
            info.AddValue("_total_citations", _total_citations);
            info.AddValue("_total_bicitations", _total_bicitations);
            info.AddValue("_total_bibliography", _total_bibliography);
            info.AddValue("_both", _both);
            info.AddValue("_total_both", _total_both);
            info.AddValue("_N_Seeds", _N_Seeds);
            info.AddValue("_N_Seeking", _N_Seeking);
        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            _recommended = info.GetValue<int>("_recommended");
            _reverse_recommended = info.GetValue<int>("_reverse_recommended");
            _birecommended = info.GetValue<int>("_birecommended");
            _bibliography = info.GetValue<int>("_bibliography");
            _citations = info.GetValue<int>("_citations");
            _bicitations = info.GetValue<int>("_bicitations");
            _total_recommended = info.GetValue<int>("_total_recommended");
            _total_reverse_recommended = info.GetValue<int>("_total_reverse_recommended");
            _total_birecommended = info.GetValue<int>("_total_birecommended");
            _total_citations = info.GetValue<int>("_total_citations");
            _total_bicitations = info.GetValue<int>("_total_bicitations");
            _total_bibliography = info.GetValue<int>("_total_bibliography");
            _both = info.GetValue<int>("_both");
            _total_both = info.GetValue<int>("_total_both");
            _DateFrom = info.GetValue<DateTime>("_DateFrom");
            _CreatedDate = info.GetValue<DateTime>("_CreatedDate");
            _AttributeId = info.GetValue<Int64>("_AttributeId");
            _N_Seeds = info.GetValue<int>("_N_Seeds");
            _N_Seeking = info.GetValue<int>("_N_Seeking");
        }


#if !SILVERLIGHT

        protected override void DataPortal_Execute()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.AcademicControllerConnectionString))
            {
                connection.Open();
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                using (SqlCommand command = new SqlCommand("st_Simulation", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@DateFrom", _DateFrom));
                    command.Parameters.Add(new SqlParameter("@CreatedDate", _CreatedDate));
                    command.Parameters.Add(new SqlParameter("@AttributeId", _AttributeId));

                    command.Parameters.Add(new SqlParameter("@Recommended", 0));
                    command.Parameters.Add(new SqlParameter("@Reverse_recommended", 0));
                    command.Parameters.Add(new SqlParameter("@Birecommended", 0));
                    command.Parameters.Add(new SqlParameter("@Bibliography", 0));
                    command.Parameters.Add(new SqlParameter("@Citations", 0));
                    command.Parameters.Add(new SqlParameter("@Bicitations", 0));
                    command.Parameters.Add(new SqlParameter("@Total_recommended", 0));
                    command.Parameters.Add(new SqlParameter("@Total_reverse_recommended", 0));
                    command.Parameters.Add(new SqlParameter("@Total_birecommended", 0));
                    command.Parameters.Add(new SqlParameter("@Total_citations", 0));
                    command.Parameters.Add(new SqlParameter("@Total_bicitations", 0));
                    command.Parameters.Add(new SqlParameter("@Total_bibliography", 0));
                    command.Parameters.Add(new SqlParameter("@Both", 0));
                    command.Parameters.Add(new SqlParameter("@Total_both", 0));
                    command.Parameters.Add(new SqlParameter("@N_Seeds", 0));
                    command.Parameters.Add(new SqlParameter("@N_Seeking", 0));

                    command.Parameters["@Recommended"].Direction = System.Data.ParameterDirection.Output;
                    command.Parameters["@Reverse_recommended"].Direction = System.Data.ParameterDirection.Output;
                    command.Parameters["@Birecommended"].Direction = System.Data.ParameterDirection.Output;
                    command.Parameters["@Bibliography"].Direction = System.Data.ParameterDirection.Output;
                    command.Parameters["@Citations"].Direction = System.Data.ParameterDirection.Output;
                    command.Parameters["@Bicitations"].Direction = System.Data.ParameterDirection.Output;
                    command.Parameters["@Total_recommended"].Direction = System.Data.ParameterDirection.Output;
                    command.Parameters["@Total_reverse_recommended"].Direction = System.Data.ParameterDirection.Output;
                    command.Parameters["@Total_birecommended"].Direction = System.Data.ParameterDirection.Output;
                    command.Parameters["@Total_citations"].Direction = System.Data.ParameterDirection.Output;
                    command.Parameters["@Total_bicitations"].Direction = System.Data.ParameterDirection.Output;
                    command.Parameters["@Total_bibliography"].Direction = System.Data.ParameterDirection.Output;
                    command.Parameters["@Both"].Direction = System.Data.ParameterDirection.Output;
                    command.Parameters["@Total_both"].Direction = System.Data.ParameterDirection.Output;
                    command.Parameters["@N_Seeds"].Direction = System.Data.ParameterDirection.Output;
                    command.Parameters["@N_Seeking"].Direction = System.Data.ParameterDirection.Output;
                    command.ExecuteNonQuery();

                    _recommended = Convert.ToInt32(command.Parameters["@Recommended"].Value);
                    _reverse_recommended = Convert.ToInt32(command.Parameters["@Reverse_recommended"].Value);
                    _birecommended = Convert.ToInt32(command.Parameters["@Birecommended"].Value);
                    _bibliography = Convert.ToInt32(command.Parameters["@Bibliography"].Value);
                    _citations = Convert.ToInt32(command.Parameters["@Citations"].Value);
                    _bicitations = Convert.ToInt32(command.Parameters["@Bicitations"].Value);
                    _total_recommended = Convert.ToInt32(command.Parameters["@Total_recommended"].Value);
                    _total_reverse_recommended = Convert.ToInt32(command.Parameters["@Total_reverse_recommended"].Value);
                    _total_birecommended = Convert.ToInt32(command.Parameters["@Total_birecommended"].Value);
                    _total_citations = Convert.ToInt32(command.Parameters["@Total_citations"].Value);
                    _total_bicitations = Convert.ToInt32(command.Parameters["@Total_bicitations"].Value);
                    _total_bibliography = Convert.ToInt32(command.Parameters["@Total_bibliography"].Value);
                    _both = Convert.ToInt32(command.Parameters["@Both"].Value);
                    _total_both = Convert.ToInt32(command.Parameters["@Total_both"].Value);
                    _N_Seeds = Convert.ToInt32(command.Parameters["@N_Seeds"].Value);
                    _N_Seeking = Convert.ToInt32(command.Parameters["@N_Seeking"].Value);
                }
                connection.Close();
            }
        }


#endif


    }
}
