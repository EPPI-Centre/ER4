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
using BusinessLibrary.Security;
using System.Security.Cryptography;
using Newtonsoft.Json;



#if !SILVERLIGHT
using Csla.Data;
using System.Data.SqlClient;
using BusinessLibrary.Data;
#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    [JsonObject(MemberSerialization.OptOut)]
    public class RobotCoderReadOnly : ReadOnlyBase<RobotCoderReadOnly>
    {
        public RobotCoderReadOnly() { }


        public static readonly PropertyInfo<int> RobotIdProperty = RegisterProperty<int>(new PropertyInfo<int>("RobotId", "RobotId"));
        public int RobotId
        {
            get
            {
                return GetProperty(RobotIdProperty);
            }
        }
        public static readonly PropertyInfo<int> RobotContactIdProperty = RegisterProperty<int>(new PropertyInfo<int>("RobotContactId", "RobotContactId"));
        public int RobotContactId
        {
            get
            {
                return GetProperty(RobotContactIdProperty);
            }
        }
        public static readonly PropertyInfo<string> RobotNameProperty = RegisterProperty<string>(new PropertyInfo<string>("RobotName", "RobotName"));
        public string RobotName
        {
            get
            {
                return GetProperty(RobotNameProperty);
            }
        }
        public static readonly PropertyInfo<string> EndPointProperty = RegisterProperty<string>(new PropertyInfo<string>("EndPoint", "EndPoint"));
        [JsonIgnore]
        public string EndPoint
        {
            get
            {
                return GetProperty(EndPointProperty);
            }
        }
        public static readonly PropertyInfo<bool> IsPublicProperty = RegisterProperty<bool>(new PropertyInfo<bool>("IsPublic", "IsPublic"));
        public bool IsPublic
        {
            get
            {
                return GetProperty(IsPublicProperty);
            }
        }
        public static readonly PropertyInfo<decimal> TopPProperty = RegisterProperty<decimal>(new PropertyInfo<decimal>("TopP", "TopP"));
        public decimal TopP
        {
            get
            {
                return GetProperty(TopPProperty);
            }
        }
        public static readonly PropertyInfo<decimal> TemperatureProperty = RegisterProperty<decimal>(new PropertyInfo<decimal>("Temperature", "Temperature"));
        public decimal Temperature
        {
            get
            {
                return GetProperty(TemperatureProperty);
            }
        }
        public static readonly PropertyInfo<decimal> FrequencyPenaltyProperty = RegisterProperty<decimal>(new PropertyInfo<decimal>("FrequencyPenalty", "FrequencyPenalty"));
        public decimal FrequencyPenalty
        {
            get
            {
                return GetProperty(FrequencyPenaltyProperty);
            }
        }
        public static readonly PropertyInfo<decimal> PresencePenaltyProperty = RegisterProperty<decimal>(new PropertyInfo<decimal>("PresencePenalty", "PresencePenalty"));
        public decimal PresencePenalty
        {
            get
            {
                return GetProperty(PresencePenaltyProperty);
            }
        }
        public static readonly PropertyInfo<string> DescriptionProperty = RegisterProperty<string>(new PropertyInfo<string>("Description", "Description"));
        public string Description
        {
            get
            {
                return GetProperty(DescriptionProperty);
            }
        }
        public static readonly PropertyInfo<DateTime> RetirementDateProperty = RegisterProperty<DateTime>(new PropertyInfo<DateTime>("RetirementDate", "RetirementDate", DateTime.MinValue));
        public DateTime RetirementDate
        {
            get
            {
                return GetProperty(RetirementDateProperty);
            }
        }
        public bool IsRetired
        {
            get
            {
                if (RetirementDate == DateTime.MinValue) return false; 
                else if (RetirementDate <= DateTime.Now) return true;
                return false;
            }
        }

        //protected override void AddAuthorizationRules()
        //{
        //    //string[] canRead = new string[] { "AdminUser", "RegularUser", "ReadOnlyUser" };
        //    //AuthorizationRules.AllowGet(typeof(ReadOnlyItemAttribute), canRead);
        //    //AuthorizationRules.AllowRead(ItemAttributeIdProperty, canRead);
        //}

#if !SILVERLIGHT



        protected void DataPortal_Fetch(SingleCriteria<RobotCoderReadOnly, string> crit)
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_RobotCoder", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@RobotName", crit.Value));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        if (reader.Read()) Child_Fetch(reader);
                    }
                }
            }
        }

        private void Child_Fetch(SafeDataReader reader)
        {
            LoadProperty<int>(RobotIdProperty, reader.GetInt32("ROBOT_ID"));
            LoadProperty<int>(RobotContactIdProperty, reader.GetInt32("CONTACT_ID"));
            LoadProperty<string>(RobotNameProperty, reader.GetString("CONTACT_NAME"));
            LoadProperty<string>(EndPointProperty, reader.GetString("ENDPOINT"));
            LoadProperty<bool>(IsPublicProperty, reader.GetBoolean("IS_PUBLIC"));
            LoadProperty<decimal>(TopPProperty, reader.GetDecimal("TOP_P"));
            LoadProperty<decimal>(TemperatureProperty, reader.GetDecimal("TEMPERATURE"));
            LoadProperty<decimal>(FrequencyPenaltyProperty, reader.GetDecimal("FREQUENCY_PENALTY"));
            LoadProperty<decimal>(PresencePenaltyProperty, reader.GetDecimal("PRESENCE_PENALTY"));
            LoadProperty<string>(DescriptionProperty, reader.GetString("PUBLIC_DESCRIPTION"));
            LoadProperty<DateTime>(RetirementDateProperty, reader.GetDateTime("RETIREMENT_DATE"));
        }
#endif
    }
}
