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
    public class MagSimulation : BusinessBase<MagSimulation>
    {
#if SILVERLIGHT
    public MagSimulation() { }

        
#else
        private MagSimulation() { }
#endif

        private static PropertyInfo<int> MagSimulationIdProperty = RegisterProperty<int>(new PropertyInfo<int>("MagSimulationId", "MagSimulationId", 0));
        public int MagSimulationId
        {
            get
            {
                return GetProperty(MagSimulationIdProperty);
            }
        }

        private static PropertyInfo<int> ReviewIdProperty = RegisterProperty<int>(new PropertyInfo<int>("ReviewId", "ReviewId", 0));
        public int ReviewId
        {
            get
            {
                return GetProperty(ReviewIdProperty);
            }
        }

        private static PropertyInfo<int> YearProperty = RegisterProperty<int>(new PropertyInfo<int>("Year", "Year"));
        public int Year
        {
            get
            {
                return GetProperty(YearProperty);
            }
            set
            {
                SetProperty(YearProperty, value);
            }
        }

        private static PropertyInfo<SmartDate> CreatedDateProperty = RegisterProperty<SmartDate>(new PropertyInfo<SmartDate>("CreatedDate", "CreatedDate"));
        public SmartDate CreatedDate
        {
            get
            {
                return GetProperty(CreatedDateProperty);
            }
            set
            {
                SetProperty(CreatedDateProperty, value);
            }
        }

        private static PropertyInfo<Int64> WithThisAttributeIdProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("WithThisAttributeId", "WithThisAttributeId"));
        public Int64 WithThisAttributeId
        {
            get
            {
                return GetProperty(WithThisAttributeIdProperty);
            }
            set
            {
                SetProperty(WithThisAttributeIdProperty, value);
            }
        }

        private static PropertyInfo<Int64> FilteredByAttributeIdProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("FilteredByAttributeId", "FilteredByAttributeId"));
        public Int64 FilteredByAttributeId
        {
            get
            {
                return GetProperty(FilteredByAttributeIdProperty);
            }
            set
            {
                SetProperty(FilteredByAttributeIdProperty, value);
            }
        }

        private static PropertyInfo<string> SearchMethodProperty = RegisterProperty<string>(new PropertyInfo<string>("SearchMethod", "SearchMethod"));
        public string SearchMethod
        {
            get
            {
                return GetProperty(SearchMethodProperty);
            }
            set
            {
                SetProperty(SearchMethodProperty, value);
            }
        }

        private static PropertyInfo<string> NetworkStatisticProperty = RegisterProperty<string>(new PropertyInfo<string>("NetworkStatistic", "NetworkStatistic"));
        public string NetworkStatistic
        {
            get
            {
                return GetProperty(NetworkStatisticProperty);
            }
            set
            {
                SetProperty(NetworkStatisticProperty, value);
            }
        }

        private static PropertyInfo<string> StudyTypeClassifierProperty = RegisterProperty<string>(new PropertyInfo<string>("StudyTypeClassifier", "StudyTypeClassifier"));
        public string StudyTypeClassifier
        {
            get
            {
                return GetProperty(StudyTypeClassifierProperty);
            }
            set
            {
                SetProperty(StudyTypeClassifierProperty, value);
            }
        }

        private static PropertyInfo<int> UserClassifierModelIdProperty = RegisterProperty<int>(new PropertyInfo<int>("UserClassifierModelId", "UserClassifierModelId"));
        public int UserClassifierModelId
        {
            get
            {
                return GetProperty(UserClassifierModelIdProperty);
            }
            set
            {
                SetProperty(UserClassifierModelIdProperty, value);
            }
        }

        private static PropertyInfo<string> StatusProperty = RegisterProperty<string>(new PropertyInfo<string>("Status", "Status", ""));
        public string Status
        {
            get
            {
                return GetProperty(StatusProperty);
            }
            set
            {
                SetProperty(StatusProperty, value);
            }
        }

        private static PropertyInfo<string> WithThisAttributeProperty = RegisterProperty<string>(new PropertyInfo<string>("WithThisAttribute", "WithThisAttribute", ""));
        public string WithThisAttribute
        {
            get
            {
                return GetProperty(WithThisAttributeProperty);
            }
            set
            {
                SetProperty(WithThisAttributeProperty, value);
            }
        }

        private static PropertyInfo<string> SeedTextProperty = RegisterProperty<string>(new PropertyInfo<string>("SeedText", "SeedText", ""));
        public string SeedText
        {
            get
            {
                if (GetProperty(YearProperty) != 1753)
                {
                    return "Publication before: " + GetProperty(YearProperty);
                }
                if (GetProperty(CreatedDateProperty) != Convert.ToDateTime("1/1/1753"))
                {
                    return "Created before: " + GetProperty(CreatedDateProperty).ToString();
                }
                return "With code: " + GetProperty(WithThisAttributeProperty);
            }
        }

        private static PropertyInfo<string> FilteredByAttributeProperty = RegisterProperty<string>(new PropertyInfo<string>("FilteredByAttribute", "FilteredByAttribute", ""));
        public string FilteredByAttribute
        {
            get
            {
                return GetProperty(FilteredByAttributeProperty);
            }
            set
            {
                SetProperty(FilteredByAttributeProperty, value);
            }
        }

        private static PropertyInfo<string> UserClassifierModelProperty = RegisterProperty<string>(new PropertyInfo<string>("UserClassifierModel", "UserClassifierModel", ""));
        public string UserClassifierModel
        {
            get
            {
                return GetProperty(UserClassifierModelProperty);
            }
            set
            {
                SetProperty(UserClassifierModelProperty, value);
            }
        }

        private static PropertyInfo<int> TPProperty = RegisterProperty<int>(new PropertyInfo<int>("TP", "TP"));
        public int TP
        {
            get
            {
                return GetProperty(TPProperty);
            }
            set
            {
                SetProperty(TPProperty, value);
            }
        }

        private static PropertyInfo<int> FPProperty = RegisterProperty<int>(new PropertyInfo<int>("FP", "FP"));
        public int FP
        {
            get
            {
                return GetProperty(FPProperty);
            }
            set
            {
                SetProperty(FPProperty, value);
            }
        }

        private static PropertyInfo<int> FNProperty = RegisterProperty<int>(new PropertyInfo<int>("FN", "FN"));
        public int FN
        {
            get
            {
                return GetProperty(FNProperty);
            }
            set
            {
                SetProperty(FNProperty, value);
            }
        }

        private static PropertyInfo<int> NSeedsProperty = RegisterProperty<int>(new PropertyInfo<int>("NSeeds", "NSeeds"));
        public int NSeeds
        {
            get
            {
                return GetProperty(NSeedsProperty);
            }
            set
            {
                SetProperty(NSeedsProperty, value);
            }
        }

        private static PropertyInfo<float> PrecisionProperty = RegisterProperty<float>(new PropertyInfo<float>("Precision", "Precision"));
        public float Precision
        {
            get
            {
                return TP / (TP + FP);
            }
        }

        private static PropertyInfo<float> RecallProperty = RegisterProperty<float>(new PropertyInfo<float>("Recall", "Recall"));
        public float Recall
        {
            get
            {
                return TP / (TP + FN);
            }
        }


        //protected override void AddAuthorizationRules()
        //{
        //    //string[] canWrite = new string[] { "AdminUser", "RegularUser" };
        //    //string[] canRead = new string[] { "AdminUser", "RegularUser", "ReadOnlyUser" };
        //    //string[] admin = new string[] { "AdminUser" };
        //    //AuthorizationRules.AllowCreate(typeof(MagSimulation), admin);
        //    //AuthorizationRules.AllowDelete(typeof(MagSimulation), admin);
        //    //AuthorizationRules.AllowEdit(typeof(MagSimulation), canWrite);
        //    //AuthorizationRules.AllowGet(typeof(MagSimulation), canRead);

        //    //AuthorizationRules.AllowRead(MagSimulationIdProperty, canRead);
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
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_MagSimulationInsert", connection))
                {
                    int newid = 0;
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@YEAR", ReadProperty(YearProperty)));
                    command.Parameters.Add(new SqlParameter("@CREATED_DATE", CreatedDate.DBValue));
                    command.Parameters.Add(new SqlParameter("@WITH_THIS_ATTRIBUTE_ID", ReadProperty(WithThisAttributeIdProperty)));
                    command.Parameters.Add(new SqlParameter("@FILTERED_BY_ATTRIBUTE_ID", ReadProperty(FilteredByAttributeIdProperty)));
                    command.Parameters.Add(new SqlParameter("@SEARCH_METHOD", ReadProperty(SearchMethodProperty)));
                    command.Parameters.Add(new SqlParameter("@NETWORK_STATISTIC", ReadProperty(NetworkStatisticProperty)));
                    command.Parameters.Add(new SqlParameter("@STUDY_TYPE_CLASSIFIER", ReadProperty(StudyTypeClassifierProperty)));
                    command.Parameters.Add(new SqlParameter("@USER_CLASSIFIER_MODEL_ID", ReadProperty(UserClassifierModelIdProperty)));
                    command.Parameters.Add(new SqlParameter("@STATUS", ReadProperty(StatusProperty)));
                    command.Parameters.Add(new SqlParameter("@MAG_SIMULATION_ID", newid));
                    command.Parameters["@MAG_SIMULATION_ID"].Direction = System.Data.ParameterDirection.Output;
                    command.ExecuteNonQuery();
                    LoadProperty(MagSimulationIdProperty, command.Parameters["@MAG_SIMULATION_ID"].Value);
                }
                connection.Close();
            }
        }

        protected override void DataPortal_Update()
        {
            // There's nothing to update
        }

        protected override void DataPortal_DeleteSelf()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_MagSimulationDelete", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@MAG_SIMULATION_ID", ReadProperty(MagSimulationIdProperty)));
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        protected void DataPortal_Fetch(SingleCriteria<MagSimulation, Int64> criteria)
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_MagSimulation", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@MAG_SIMULATION_ID", criteria.Value));
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        if (reader.Read())
                        {
                            LoadProperty<Int32>(ReviewIdProperty, reader.GetInt32("REVIEW_ID"));
                            LoadProperty<Int32>(MagSimulationIdProperty, reader.GetInt32("MAG_SIMULATION_ID"));
                            LoadProperty<Int32>(YearProperty, reader.GetInt32("YEAR"));
                            LoadProperty<SmartDate>(CreatedDateProperty, reader.GetSmartDate("CREATED_DATE"));
                            LoadProperty<Int64>(WithThisAttributeIdProperty, reader.GetInt64("WITH_THIS_ATTRIBUTE_ID"));
                            LoadProperty<Int64>(FilteredByAttributeIdProperty, reader.GetInt64("FILTERED_BY_ATTRIBUTE_ID"));
                            LoadProperty<string>(SearchMethodProperty, reader.GetString("SEARCH_METHOD"));
                            LoadProperty<string>(NetworkStatisticProperty, reader.GetString("NETWORK_STATISTIC"));
                            LoadProperty<string>(StudyTypeClassifierProperty, reader.GetString("STUDY_TYPE_CLASSIFIER"));
                            LoadProperty<Int32>(UserClassifierModelIdProperty, reader.GetInt32("USER_CLASSIFIER_MODEL_ID"));
                            LoadProperty<string>(StatusProperty, reader.GetString("STATUS"));
                            LoadProperty<Int32>(TPProperty, reader.GetInt32("TP"));
                            LoadProperty<Int32>(FPProperty, reader.GetInt32("FP"));
                            LoadProperty<Int32>(FNProperty, reader.GetInt32("FN"));
                            LoadProperty<Int32>(NSeedsProperty, reader.GetInt32("NSEEDS"));
                            LoadProperty<string>(WithThisAttributeProperty, reader.GetString("WithThisAttribute"));
                            LoadProperty<string>(FilteredByAttributeProperty, reader.GetString("FilteredByAttribute"));
                            LoadProperty<string>(UserClassifierModelProperty, reader.GetString("MODEL_TITLE"));
                        }
                    }
                }
                connection.Close();
            }
        }

        internal static MagSimulation GetMagSimulation(SafeDataReader reader)
        {
            MagSimulation returnValue = new MagSimulation();
            returnValue.LoadProperty<Int32>(ReviewIdProperty, reader.GetInt32("REVIEW_ID"));
            returnValue.LoadProperty<Int32>(MagSimulationIdProperty, reader.GetInt32("MAG_SIMULATION_ID"));
            returnValue.LoadProperty<Int32>(YearProperty, reader.GetInt32("YEAR"));
            returnValue.LoadProperty<SmartDate>(CreatedDateProperty, reader.GetSmartDate("CREATED_DATE"));
            returnValue.LoadProperty<Int64>(WithThisAttributeIdProperty, reader.GetInt64("WITH_THIS_ATTRIBUTE_ID"));
            returnValue.LoadProperty<Int64>(FilteredByAttributeIdProperty, reader.GetInt64("FILTERED_BY_ATTRIBUTE_ID"));
            returnValue.LoadProperty<string>(SearchMethodProperty, reader.GetString("SEARCH_METHOD"));
            returnValue.LoadProperty<string>(NetworkStatisticProperty, reader.GetString("NETWORK_STATISTIC"));
            returnValue.LoadProperty<string>(StudyTypeClassifierProperty, reader.GetString("STUDY_TYPE_CLASSIFIER"));
            returnValue.LoadProperty<Int32>(UserClassifierModelIdProperty, reader.GetInt32("USER_CLASSIFIER_MODEL_ID"));
            returnValue.LoadProperty<string>(StatusProperty, reader.GetString("STATUS"));
            returnValue.LoadProperty<Int32>(TPProperty, reader.GetInt32("TP"));
            returnValue.LoadProperty<Int32>(FPProperty, reader.GetInt32("FP"));
            returnValue.LoadProperty<Int32>(FNProperty, reader.GetInt32("FN"));
            returnValue.LoadProperty<Int32>(NSeedsProperty, reader.GetInt32("NSEEDS"));
            returnValue.LoadProperty<string>(WithThisAttributeProperty, reader.GetString("WithThisAttribute"));
            returnValue.LoadProperty<string>(FilteredByAttributeProperty, reader.GetString("FilteredByAttribute"));
            returnValue.LoadProperty<string>(UserClassifierModelProperty, reader.GetString("MODEL_TITLE"));
            returnValue.MarkOld();
            return returnValue;
        }

#endif
    }
}