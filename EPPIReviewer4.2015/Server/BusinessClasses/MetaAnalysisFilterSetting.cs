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
    public class MetaAnalysisFilterSetting : BusinessBase<MetaAnalysisFilterSetting>
    {

        public MetaAnalysisFilterSetting() { }
        public MetaAnalysisFilterSetting(int metaAnalysisId) {
            LoadProperty<int>(MetaAnalysisIdProperty, metaAnalysisId);
        }

        /// <summary>
        /// removes all filter setting, keeping IDs and column name
        /// </summary>
        public void Clear()
        {
            SelectedValues = "";
            Filter1 = "";
            Filter1CaseSensitive = false;
            Filter1Operator = "IsEqualTo";

            Filter2 = "";
            Filter2CaseSensitive = false;
            Filter2Operator = "IsEqualTo";

            FiltersLogicalOperator = "And";
            this.MarkDirty();
        }
        public bool IsClear
        {
            get
            {
                if (SelectedValues == ""
                && Filter1 == ""
                && Filter1CaseSensitive == false
                && Filter1Operator == "IsEqualTo"
                && Filter2 == ""
                && Filter2CaseSensitive == false
                && Filter2Operator == "IsEqualTo"
                && FiltersLogicalOperator == "And") return true;
                else return false;
            }
        }

        public static readonly PropertyInfo<int> MetaAnalysisFilterSettingIdProperty = RegisterProperty<int>(new PropertyInfo<int>("MetaAnalysisFilterSettingId", "MetaAnalysisFilterSettingId"));
        public int MetaAnalysisFilterSettingId
        {
            get
            {
                return GetProperty(MetaAnalysisFilterSettingIdProperty);
            }
        }

        public static readonly PropertyInfo<int> MetaAnalysisIdProperty = RegisterProperty<int>(new PropertyInfo<int>("MetaAnalysisId", "MetaAnalysisId", 0));
        public int MetaAnalysisId
        {
            get
            {
                return GetProperty(MetaAnalysisIdProperty);
            }
        }

        public static readonly PropertyInfo<string> ColumnNameProperty = RegisterProperty<string>(new PropertyInfo<string>("ColumnName", "ColumnName", string.Empty));
        public string ColumnName
        {
            get
            {
                return GetProperty(ColumnNameProperty);
            }
            set
            {
                SetProperty(ColumnNameProperty, value);
            }
        }

        /// <summary>
        /// list of values separated by '¬'...
        /// </summary>
        public static readonly PropertyInfo<string> SelectedValuesProperty = RegisterProperty<string>(new PropertyInfo<string>("SelectedValues", "SelectedValues"));
        public string SelectedValues
        {
            get
            {
                return GetProperty(SelectedValuesProperty);
            }
            set
            {
                SetProperty(SelectedValuesProperty, value);
            }
        }

        public static readonly PropertyInfo<string> Filter1Property = RegisterProperty<string>(new PropertyInfo<string>("Filter1", "Filter1", string.Empty));
        public string Filter1
        {
            get
            {
                return GetProperty(Filter1Property);
            }
            set
            {
                SetProperty(Filter1Property, value);
            }
        }

        public static readonly PropertyInfo<string> Filter1OperatorProperty = RegisterProperty<string>(new PropertyInfo<string>("Filter1Operator", "Filter1Operator", string.Empty));
        public string Filter1Operator
        {
            get
            {
                return GetProperty(Filter1OperatorProperty);
            }
            set
            {
                SetProperty(Filter1OperatorProperty, value);
            }
        }
        public static readonly PropertyInfo<bool> Filter1CaseSensitiveProperty = RegisterProperty<bool>(new PropertyInfo<bool>("Filter1CaseSensitive", "Filter1CaseSensitive", false));
        public bool Filter1CaseSensitive
        {
            get
            {
                return GetProperty(Filter1CaseSensitiveProperty);
            }
            set
            {
                SetProperty(Filter1CaseSensitiveProperty, value);
            }
        }
        /// <summary>
        /// Combining filters 1 and 2 with "And" or "Or"
        /// </summary>
        public static readonly PropertyInfo<string> FiltersLogicalOperatorProperty = RegisterProperty<string>(new PropertyInfo<string>("FiltersLogicalOperator", "FiltersLogicalOperator", "Or"));
        public string FiltersLogicalOperator
        {
            get
            {
                return GetProperty(FiltersLogicalOperatorProperty);
            }
            set
            {
                SetProperty(FiltersLogicalOperatorProperty, value);
            }
        }
        public static readonly PropertyInfo<string> Filter2Property = RegisterProperty<string>(new PropertyInfo<string>("Filter2", "Filter2", string.Empty));
        public string Filter2
        {
            get
            {
                return GetProperty(Filter2Property);
            }
            set
            {
                SetProperty(Filter2Property, value);
            }
        }

        public static readonly PropertyInfo<string> Filter2OperatorProperty = RegisterProperty<string>(new PropertyInfo<string>("Filter2Operator", "Filter2Operator", string.Empty));
        public string Filter2Operator
        {
            get
            {
                return GetProperty(Filter2OperatorProperty);
            }
            set
            {
                SetProperty(Filter2OperatorProperty, value);
            }
        }
        public static readonly PropertyInfo<bool> Filter2CaseSensitiveProperty = RegisterProperty<bool>(new PropertyInfo<bool>("Filter2CaseSensitive", "Filter2CaseSensitive", false));
        public bool Filter2CaseSensitive
        {
            get
            {
                return GetProperty(Filter2CaseSensitiveProperty);
            }
            set
            {
                SetProperty(Filter2CaseSensitiveProperty, value);
            }
        }

        

        protected override void AddBusinessRules()
        { }

#if !SILVERLIGHT

        protected override void DataPortal_Insert()
        {
            if (!this.IsClear)
            {
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand("st_MetaAnalysisFilterSettingCreate", connection))
                    {
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                        command.Parameters.Add(new SqlParameter("@META_ANALYSIS_ID", ReadProperty(MetaAnalysisIdProperty)));
                        command.Parameters.Add(new SqlParameter("@COLUMN_NAME", ReadProperty(ColumnNameProperty)));
                        command.Parameters.Add(new SqlParameter("@SELECTED_VALUES", ReadProperty(SelectedValuesProperty)));
                        command.Parameters.Add(new SqlParameter("@FILTER_1_VALUE", ReadProperty(Filter1Property)));
                        command.Parameters.Add(new SqlParameter("@FILTER_1_OPERATOR", ReadProperty(Filter1OperatorProperty)));
                        command.Parameters.Add(new SqlParameter("@FILTER_1_CASE_SENSITIVE", ReadProperty(Filter1CaseSensitiveProperty)));
                        command.Parameters.Add(new SqlParameter("@FIELD_FILTER_LOGICAL_OPERATOR", ReadProperty(FiltersLogicalOperatorProperty)));
                        command.Parameters.Add(new SqlParameter("@FILTER_2_VALUE", ReadProperty(Filter2Property)));
                        command.Parameters.Add(new SqlParameter("@FILTER_2_OPERATOR", ReadProperty(Filter2OperatorProperty)));
                        command.Parameters.Add(new SqlParameter("@FILTER_2_CASE_SENSITIVE", ReadProperty(Filter2CaseSensitiveProperty)));

                        command.Parameters.Add(new SqlParameter("@META_ANALYSIS_FILTER_SETTING_ID", System.Data.SqlDbType.Int));
                        command.Parameters["@META_ANALYSIS_FILTER_SETTING_ID"].Direction = System.Data.ParameterDirection.Output;
                        command.ExecuteNonQuery();

                        LoadProperty(MetaAnalysisFilterSettingIdProperty, command.Parameters["@META_ANALYSIS_FILTER_SETTING_ID"].Value);// -1 would mean it failed
                    }
                    connection.Close();
                }
            }
        }

        protected override void DataPortal_Update()
        {
            if (this.IsClear)
            {
                //safety measure, to be very sure we don't "keep" cleared filters.
                this.DataPortal_DeleteSelf();
                return;
            }
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
				ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
				connection.Open();
                using (SqlCommand command = new SqlCommand("st_MetaAnalysisFilterSettingUpdate", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@META_ANALYSIS_FILTER_SETTING_ID", ReadProperty(MetaAnalysisFilterSettingIdProperty)));
                    command.Parameters.Add(new SqlParameter("@META_ANALYSIS_ID", ReadProperty(MetaAnalysisIdProperty)));
                    command.Parameters.Add(new SqlParameter("@COLUMN_NAME", ReadProperty(ColumnNameProperty)));
                    command.Parameters.Add(new SqlParameter("@SELECTED_VALUES", ReadProperty(SelectedValuesProperty)));
                    command.Parameters.Add(new SqlParameter("@FILTER_1_VALUE", ReadProperty(Filter1Property)));
                    command.Parameters.Add(new SqlParameter("@FILTER_1_OPERATOR", ReadProperty(Filter1OperatorProperty)));
                    command.Parameters.Add(new SqlParameter("@FILTER_1_CASE_SENSITIVE", ReadProperty(Filter1CaseSensitiveProperty)));
                    command.Parameters.Add(new SqlParameter("@FIELD_FILTER_LOGICAL_OPERATOR", ReadProperty(FiltersLogicalOperatorProperty)));
                    command.Parameters.Add(new SqlParameter("@FILTER_2_VALUE", ReadProperty(Filter2Property)));
                    command.Parameters.Add(new SqlParameter("@FILTER_2_OPERATOR", ReadProperty(Filter2OperatorProperty)));
                    command.Parameters.Add(new SqlParameter("@FILTER_2_CASE_SENSITIVE", ReadProperty(Filter2CaseSensitiveProperty)));

                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        protected override void DataPortal_DeleteSelf()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_MetaAnalysisFilterSettingDelete", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@META_ANALYSIS_FILTER_SETTING_ID", ReadProperty(MetaAnalysisFilterSettingIdProperty)));
                    command.Parameters.Add(new SqlParameter("@META_ANALYSIS_ID", ReadProperty(MetaAnalysisIdProperty)));
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        internal static MetaAnalysisFilterSetting GetMetaAnalysisFilterSetting(SafeDataReader reader)
        {
            MetaAnalysisFilterSetting returnValue = new MetaAnalysisFilterSetting();
            returnValue.LoadProperty<int>(MetaAnalysisFilterSettingIdProperty, reader.GetInt32("META_ANALYSIS_FILTER_SETTING_ID"));
            returnValue.LoadProperty<int>(MetaAnalysisIdProperty, reader.GetInt32("META_ANALYSIS_ID"));
            returnValue.LoadProperty<string>(ColumnNameProperty, reader.GetString("COLUMN_NAME"));
            returnValue.LoadProperty<string>(SelectedValuesProperty, reader.GetString("SELECTED_VALUES"));
            returnValue.LoadProperty<string>(Filter1Property, reader.GetString("FILTER_1_VALUE"));
            returnValue.LoadProperty<string>(Filter1OperatorProperty, reader.GetString("FILTER_1_OPERATOR"));
            returnValue.LoadProperty<bool>(Filter1CaseSensitiveProperty, reader.GetBoolean("FILTER_1_CASE_SENSITIVE"));
            returnValue.LoadProperty<string>(FiltersLogicalOperatorProperty, reader.GetString("FIELD_FILTER_LOGICAL_OPERATOR"));
            returnValue.LoadProperty<string>(Filter2Property, reader.GetString("FILTER_2_VALUE"));
            returnValue.LoadProperty<string>(Filter2OperatorProperty, reader.GetString("FILTER_2_OPERATOR"));
            returnValue.LoadProperty<bool>(Filter2CaseSensitiveProperty, reader.GetBoolean("FILTER_2_CASE_SENSITIVE"));
            returnValue.MarkOld();
            return returnValue;
        }

#endif

    }
}
