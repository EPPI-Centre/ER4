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
    public class ItemTimepoint : BusinessBase<ItemTimepoint>
    {

        public ItemTimepoint() { }

        public override string ToString()
        {
            return TimepointDisplayValue;
        }


        public static readonly PropertyInfo<Int64> ItemTimepointIdProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("ItemTimepointId", "ItemTimepointId"));
        public Int64 ItemTimepointId
        {
            get
            {
                return GetProperty(ItemTimepointIdProperty);
            }
        }

        public static readonly PropertyInfo<Int64> ItemIdProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("ItemId", "ItemId"));
        public Int64 ItemId
        {
            get
            {
                return GetProperty(ItemIdProperty);
            }
            set
            {
                SetProperty(ItemIdProperty, value);
            }
        }

        public static readonly PropertyInfo<float> TimepointValueProperty = RegisterProperty<float>(new PropertyInfo<float>("TimepointValue", "TimepointValue", 0.0));
        public float TimepointValue
        {
            get
            {
                return GetProperty(TimepointValueProperty);
            }
            set
            {
                SetProperty(TimepointValueProperty, value);
            }
        }

        public static readonly PropertyInfo<string> TimepointMetricProperty = RegisterProperty<string>(new PropertyInfo<string>("TimepointMetric", "TimepointMetric", string.Empty));
        public string TimepointMetric
        {
            get
            {
                return GetProperty(TimepointMetricProperty);
            }
            set
            {
                SetProperty(TimepointMetricProperty, value);
            }
        }

        public string TimepointDisplayValue
        {
            get
            {
                return this.ReadProperty(TimepointValueProperty) + " " + this.ReadProperty(TimepointMetricProperty);
            }
        }

        



        //protected override void AddAuthorizationRules()
        //{
        //    //string[] canWrite = new string[] { "AdminUser", "RegularUser" };
        //    //string[] canRead = new string[] { "AdminUser", "RegularUser", "ReadOnlyUser" };
        //    //string[] admin = new string[] { "AdminUser" };
        //    //AuthorizationRules.AllowCreate(typeof(ItemTimepoint), admin);
        //    //AuthorizationRules.AllowDelete(typeof(ItemTimepoint), admin);
        //    //AuthorizationRules.AllowEdit(typeof(ItemTimepoint), canWrite);
        //    //AuthorizationRules.AllowGet(typeof(ItemTimepoint), canRead);

        //    //AuthorizationRules.AllowRead(ItemTimepointIdProperty, canRead);
        //    //AuthorizationRules.AllowRead(ReviewIdProperty, canRead);

        //    ////AuthorizationRules.AllowWrite(NameProperty, canWrite);
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
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ItemTimepointCreate", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@ITEM_ID", ReadProperty(ItemIdProperty)));
                    command.Parameters.Add(new SqlParameter("@TIMEPOINT_VALUE", ReadProperty(TimepointValueProperty)));
                    command.Parameters.Add(new SqlParameter("@TIMEPOINT_METRIC", ReadProperty(TimepointMetricProperty)));
                    command.Parameters.Add(new SqlParameter("@NEW_ITEM_TIMEPOINT_ID", 0));
                    command.Parameters["@NEW_ITEM_TIMEPOINT_ID"].Direction = System.Data.ParameterDirection.Output;
                    command.ExecuteNonQuery();
                    LoadProperty(ItemTimepointIdProperty, command.Parameters["@NEW_ITEM_TIMEPOINT_ID"].Value);
                }
                connection.Close();
            }
        }

        protected override void DataPortal_Update()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ItemTimepointUpdate", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@ITEM_TIMEPOINT_ID", ReadProperty(ItemTimepointIdProperty)));
                    command.Parameters.Add(new SqlParameter("@TIMEPOINT_VALUE", ReadProperty(TimepointValueProperty)));
                    command.Parameters.Add(new SqlParameter("@TIMEPOINT_METRIC", ReadProperty(TimepointMetricProperty)));
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        protected override void DataPortal_DeleteSelf()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ItemTimepointDelete", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@ITEM_TIMEPOINT_ID", ReadProperty(ItemTimepointIdProperty)));
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        internal static ItemTimepoint GetItemTimepoint(SafeDataReader reader)
        {
            ItemTimepoint returnValue = new ItemTimepoint();
            returnValue.LoadProperty<Int64>(ItemTimepointIdProperty, reader.GetInt64("ITEM_TIMEPOINT_ID"));
            returnValue.LoadProperty<Int64>(ItemIdProperty, reader.GetInt64("ITEM_ID"));
            returnValue.LoadProperty<float>(TimepointValueProperty, (float) reader.GetDouble("TIMEPOINT_VALUE"));
            returnValue.LoadProperty<string>(TimepointMetricProperty, reader.GetString("TIMEPOINT_METRIC"));

            returnValue.MarkOld();
            return returnValue;
        }

#endif

    }
}
