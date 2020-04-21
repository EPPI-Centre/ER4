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
using Csla.DataPortalClient;

#if!SILVERLIGHT
using System.Data.SqlClient;
using BusinessLibrary.Data;
using Csla.Data;
using BusinessLibrary.Security;
#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class MagLog : BusinessBase<MagLog>
    {
#if SILVERLIGHT
    public MagLog() { }

        
#else
        public MagLog() { }
#endif

        public static readonly PropertyInfo<int> MagLogIdProperty = RegisterProperty<int>(new PropertyInfo<int>("MagLogId", "MagLogId"));
        public int MagLogId
        {
            get
            {
                return GetProperty(MagLogIdProperty);
            }
        }

        public static readonly PropertyInfo<string> JobTypeProperty = RegisterProperty<string>(new PropertyInfo<string>("JobType", "JobType", string.Empty));
        public string JobType
        {
            get
            {
                return GetProperty(JobTypeProperty);
            }
            set
            {
                SetProperty(JobTypeProperty, value);
            }
        }

        public static readonly PropertyInfo<string> JobStatusProperty = RegisterProperty<string>(new PropertyInfo<string>("JobStatus", "JobStatus", string.Empty));
        public string JobStatus
        {
            get
            {
                return GetProperty(JobStatusProperty);
            }
            set
            {
                SetProperty(JobStatusProperty, value);
            }
        }

        public static readonly PropertyInfo<string> JobMessageProperty = RegisterProperty<string>(new PropertyInfo<string>("JobMessage", "JobMessage", string.Empty));
        public string JobMessage
        {
            get
            {
                return GetProperty(JobMessageProperty);
            }
            set
            {
                SetProperty(JobMessageProperty, value);
            }
        }

        public static readonly PropertyInfo<string> ContactNameProperty = RegisterProperty<string>(new PropertyInfo<string>("ContactName", "ContactName", string.Empty));
        public string ContactName
        {
            get
            {
                return GetProperty(ContactNameProperty);
            }
            set
            {
                SetProperty(ContactNameProperty, value);
            }
        }

        public static readonly PropertyInfo<int> ContactIdProperty = RegisterProperty<int>(new PropertyInfo<int>("ContactId", "ContactId", 0));
        public int ContactId
        {
            get
            {
                return GetProperty(ContactIdProperty);
            }
            set
            {
                SetProperty(ContactIdProperty, value);
            }
        }

        public static readonly PropertyInfo<DateTime> TimeSubmittedProperty = RegisterProperty<DateTime>(new PropertyInfo<DateTime>("TimeSubmitted", "TimeSubmitted", 0));
        public DateTime TimeSubmitted
        {
            get
            {
                return GetProperty(TimeSubmittedProperty);
            }
            set
            {
                SetProperty(TimeSubmittedProperty, value);
            }
        }

        private static PropertyInfo<DateTime> TimeUpdatedProperty = RegisterProperty<DateTime>(new PropertyInfo<DateTime>("TimeUpdated", "TimeUpdated", 0));
        public DateTime TimeUpdated
        {
            get
            {
                return GetProperty(TimeUpdatedProperty);
            }
            set
            {
                SetProperty(TimeUpdatedProperty, value);
            }
        }

        //protected override void AddAuthorizationRules()
        //{
        //    //string[] canWrite = new string[] { "AdminUser", "RegularUser" };
        //    //string[] canRead = new string[] { "AdminUser", "RegularUser", "ReadOnlyUser" };
        //    //string[] admin = new string[] { "AdminUser" };
        //    //AuthorizationRules.AllowCreate(typeof(MagLog), admin);
        //    //AuthorizationRules.AllowDelete(typeof(MagLog), admin);
        //    //AuthorizationRules.AllowEdit(typeof(MagLog), canWrite);
        //    //AuthorizationRules.AllowGet(typeof(MagLog), canRead);

        //    //AuthorizationRules.AllowRead(MagLogIdProperty, canRead);
        //    //AuthorizationRules.AllowRead(ReviewIdProperty, canRead);
        //    //AuthorizationRules.AllowRead(NameProperty, canRead);
        //    //AuthorizationRules.AllowRead(DetailProperty, canRead);

        //    //AuthorizationRules.AllowWrite(NameProperty, canWrite);
        //    //AuthorizationRules.AllowRead(DetailProperty, canRead);
        //}

        protected override void AddBusinessRules()
        {
            //ValidationRules.AddRule(Csla.Validation.CommonRules.MaxValue<decimal>, new Csla.Validation.CommonRules.MaxValueRuleArgs<decimal>(ReviewCodeSetFte1Property, 1));
            //ValidationRules.AddRule(Csla.Validation.CommonRules.StringRequired, new Csla.Validation.RuleArgs(ReviewCodeSetNameProperty));
            //ValidationRules.AddRule(Csla.Validation.CommonRules.StringMaxLength, new Csla.Validation.CommonRules.MaxLengthRuleArgs(ReviewCodeSetNameProperty, 20));
        }

#if !SILVERLIGHT

        protected override void DataPortal_Insert()
        {
            // insert handled elsewhere
        }

        protected override void DataPortal_Update()
        {
            // handled elsewhere
        }

        protected override void DataPortal_DeleteSelf()
        {
            // not sure we need this?
        }

        internal static MagLog GetMagLog(SafeDataReader reader)
        {
            MagLog returnValue = new MagLog();
            returnValue.LoadProperty<int>(MagLogIdProperty, reader.GetInt32("MAG_LOG_ID"));
            returnValue.LoadProperty<DateTime>(TimeSubmittedProperty, reader.GetDateTime("TIME_SUBMITTED"));
            returnValue.LoadProperty<DateTime>(TimeUpdatedProperty, reader.GetDateTime("TIME_UPDATED"));
            returnValue.LoadProperty<string>(ContactNameProperty, reader.GetString("CONTACT_NAME"));
            returnValue.LoadProperty<int>(ContactIdProperty, reader.GetInt32("CONTACT_ID"));
            returnValue.LoadProperty<string>(JobTypeProperty, reader.GetString("JOB_TYPE"));
            returnValue.LoadProperty<string>(JobStatusProperty, reader.GetString("JOB_STATUS"));
            returnValue.LoadProperty<string>(JobMessageProperty, reader.GetString("JOB_MESSAGE"));

            returnValue.MarkOld();
            return returnValue;
        }

        public static int SaveLogEntry(string jobType, string status, string message, int ContactId)
        {
            int newLogId = 0;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_MagLogInsert", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@CONTACT_ID", ContactId));
                    command.Parameters.Add(new SqlParameter("@JOB_TYPE", jobType));
                    command.Parameters.Add(new SqlParameter("@JOB_STATUS", status));
                    command.Parameters.Add(new SqlParameter("@JOB_MESSAGE", message));
                    command.Parameters.Add(new SqlParameter("@MAG_LOG_ID", 0));
                    command.Parameters["@MAG_LOG_ID"].Direction = System.Data.ParameterDirection.Output;
                    command.ExecuteNonQuery();
                    newLogId = Convert.ToInt32(command.Parameters["@MAG_LOG_ID"].Value.ToString());
                }
                connection.Close();
            }
            return newLogId;
        }

        public static void UpdateLogEntry(string status, string message, int MagLogId)
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_MagLogUpdate", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@MAG_LOG_ID", MagLogId));
                    command.Parameters.Add(new SqlParameter("@JOB_STATUS", status));
                    command.Parameters.Add(new SqlParameter("@JOB_MESSAGE", message));
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }



#endif

    }
}
