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

        public static readonly PropertyInfo<int> InputTokenCostPmProperty = RegisterProperty<int>(new PropertyInfo<int>("InputTokenCostPm", "InputTokenCostPm"));
        public int InputTokenCostPerMillion
        {
            get
            {
                return GetProperty(InputTokenCostPmProperty);
            }
        }

        public static readonly PropertyInfo<int> OutputTokenCostPmProperty = RegisterProperty<int>(new PropertyInfo<int>("OutputTokenCostPm", "OutputTokenCostPm"));
        public int OutputTokenCostPerMillion
        {
            get
            {
                return GetProperty(OutputTokenCostPmProperty);
            }
        }

        public static readonly PropertyInfo<MobileList<RobotCoderSetting>> RobotSettingsProperty = RegisterProperty<MobileList<RobotCoderSetting>>(new PropertyInfo<MobileList<RobotCoderSetting>>("RobotSettings", "RobotSettings"));
        public MobileList<RobotCoderSetting> RobotSettings
        {
            get
            {
                return GetProperty(RobotSettingsProperty);
            }
        }
        public static readonly PropertyInfo<int> RequestsPerMinuteProperty = RegisterProperty<int>(new PropertyInfo<int>("RequestsPerMinute", "RequestsPerMinute"));
        public int RequestsPerMinute
        {
            get
            {
                return GetProperty(RequestsPerMinuteProperty);
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
            LoadProperty<string>(DescriptionProperty, reader.GetString("PUBLIC_DESCRIPTION"));
            LoadProperty<DateTime>(RetirementDateProperty, reader.GetDateTime("RETIREMENT_DATE"));
            LoadProperty<int>(RequestsPerMinuteProperty, (int)reader.GetInt16("REQUESTS_PER_MINUTE"));
            LoadProperty(RobotSettingsProperty, new MobileList<RobotCoderSetting>());
            string tmp = reader.GetString("FOR_SALE_IDs");
            string[] IdsStr = tmp.Split(',');
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("st_RobotCoderForSaleAndSettings", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@RobotId", RobotId));
                    using (Csla.Data.SafeDataReader reader2 = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        if (reader2.Read())
                        {//1st line: input tokens cost
                            LoadProperty<int>(InputTokenCostPmProperty, reader2.GetInt32("PRICE_PER_MONTH"));
                        }
                        if (reader2.Read())
                        {//2nd line: output tokens
                            LoadProperty<int>(OutputTokenCostPmProperty, reader2.GetInt32("PRICE_PER_MONTH"));
                        }
                        reader2.NextResult();
                        while (reader2.Read())
                        {
                            RobotSettings.Add(new RobotCoderSetting(reader2.GetString("SETTING_NAME"), reader2.GetString("SETTING_VALUE")));
                        }
                    }
                }
            }
        }
#endif
    }

    [Serializable]
    public class RobotCoderSetting: BusinessBase<RobotCoderSetting>
    {
        public RobotCoderSetting() { }
        internal RobotCoderSetting(string Name, string Value) {
            SettingName = Name;
            SettingValue = Value;
        }
        public static readonly PropertyInfo<string> SettingNameProperty = RegisterProperty<string>(new PropertyInfo<string>("SettingName", "SettingName"));
        public string SettingName
        {
            get
            {
                return GetProperty(SettingNameProperty);
            }
            set
            {
                SetProperty(SettingNameProperty, value);
            }
        }
        public static readonly PropertyInfo<string> SettingValueProperty = RegisterProperty<string>(new PropertyInfo<string>("SettingValue", "SettingValue"));
        public string SettingValue
        {
            get
            {
                return GetProperty(SettingValueProperty);
            }
            set
            {
                SetProperty(SettingValueProperty, value);
            }
        }
    }
}
