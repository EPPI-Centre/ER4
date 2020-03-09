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
    public class MagSimulationResult : ReadOnlyBase<MagSimulationResult>
    {
#if SILVERLIGHT
    public MagSimulationResult() { }

        
#else
        private MagSimulationResult() { }
#endif
        /* Seems odd not to have this, but trying to save as much memory as possible - and we don't actually need it
        private static PropertyInfo<int> MagSimulationIdProperty = RegisterProperty<int>(new PropertyInfo<int>("MagSimulationId", "MagSimulationId", 0));
        public int MagSimulationId
        {
            get
            {
                return GetProperty(MagSimulationIdProperty);
            }
        }
        
        private static PropertyInfo<Int64> PaperIdProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("PaperId", "PaperId"));
        public Int64 PaperId
        {
            get
            {
                return GetProperty(PaperIdProperty);
            }
        }

        

        private static PropertyInfo<bool> FoundProperty = RegisterProperty<bool>(new PropertyInfo<bool>("Found", "Found", 0));
        public bool Found
        {
            get
            {
                return GetProperty(FoundProperty);
            }
        }

        private static PropertyInfo<double> StudyTypeClassifierProperty = RegisterProperty<double>(new PropertyInfo<double>("StudyTypeClassifier", "StudyTypeClassifier"));
        public double StudyTypeClassifier
        {
            get
            {
                return GetProperty(StudyTypeClassifierProperty);
            }
        }

        private static PropertyInfo<double> UserClassifierProperty = RegisterProperty<double>(new PropertyInfo<double>("UserClassifier", "UserClassifier"));
        public double UserClassifier
        {
            get
            {
                return GetProperty(UserClassifierProperty);
            }
        }

        private static PropertyInfo<double> NetworkStatisticScoreProperty = RegisterProperty<double>(new PropertyInfo<double>("NetworkStatisticScore", "NetworkStatisticScore"));
        public double NetworkStatisticScore
        {
            get
            {
                return GetProperty(NetworkStatisticScoreProperty);
            }
        }

        private static PropertyInfo<double> FieldOfStudyDistanceProperty = RegisterProperty<double>(new PropertyInfo<double>("FieldOfStudyDistance", "FieldOfStudyDistance"));
        public double FieldOfStudyDistance
        {
            get
            {
                return GetProperty(FieldOfStudyDistanceProperty);
            }
        }

        private static PropertyInfo<double> EnsembleScoreProperty = RegisterProperty<double>(new PropertyInfo<double>("EnsembleScore", "EnsembleScore"));
        public double EnsembleScore
        {
            get
            {
                return GetProperty(EnsembleScoreProperty);
            }
        }
        */

        private static PropertyInfo<bool> IncludedProperty = RegisterProperty<bool>(new PropertyInfo<bool>("Included", "Included", 0));
        public bool Included
        {
            get
            {
                return GetProperty(IncludedProperty);
            }
        }

        private static PropertyInfo<int> NumberScreenedProperty = RegisterProperty<int>(new PropertyInfo<int>("NumberScreened", "NumberScreened", 0));
        public int NumberScreened
        {
            get
            {
                return GetProperty(NumberScreenedProperty);
            }
        }

        private static PropertyInfo<int> CumulativeIncludeCountProperty = RegisterProperty<int>(new PropertyInfo<int>("CumulativeIncludeCount", "CumulativeIncludeCount", 0));
        public int CumulativeIncludeCount
        {
            get
            {
                return GetProperty(CumulativeIncludeCountProperty);
            }
        }






        //protected override void AddAuthorizationRules()
        //{
        //    //string[] canWrite = new string[] { "AdminUser", "RegularUser" };
        //    //string[] canRead = new string[] { "AdminUser", "RegularUser", "ReadOnlyUser" };
        //    //string[] admin = new string[] { "AdminUser" };
        //    //AuthorizationRules.AllowCreate(typeof(MagSimulationResult), admin);
        //    //AuthorizationRules.AllowDelete(typeof(MagSimulationResult), admin);
        //    //AuthorizationRules.AllowEdit(typeof(MagSimulationResult), canWrite);
        //    //AuthorizationRules.AllowGet(typeof(MagSimulationResult), canRead);

        //    //AuthorizationRules.AllowRead(MagSimulationResultIdProperty, canRead);
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
        /*
        protected override void DataPortal_Insert()
        {
            // won't be called
        }

        protected override void DataPortal_Update()
        {
            // unlikely ever to need this
        }

        protected override void DataPortal_DeleteSelf()
        {
            // unlikely ever to be called
        }
        */
        protected void DataPortal_Fetch(SingleCriteria<MagSimulationResult, Int64> criteria)
        {
            // ditto - won't need
        }

        internal static MagSimulationResult GetMagSimulationResult(SafeDataReader reader, int index, int CumulativeIncludeCount)
        {
            MagSimulationResult returnValue = new MagSimulationResult();
            //returnValue.LoadProperty<Int32>(MagSimulationIdProperty, reader.GetInt32("MAG_SIMULATION_ID"));
            //returnValue.LoadProperty<Int64>(PaperIdProperty, reader.GetInt64("PaperId"));
            returnValue.LoadProperty<bool>(IncludedProperty, reader.GetBoolean("INCLUDED"));
            //returnValue.LoadProperty<bool>(FoundProperty, reader.GetBoolean("FOUND"));
            //returnValue.LoadProperty<double>(StudyTypeClassifierProperty, Math.Round(reader.GetDouble("STUDY_TYPE_CLASSIFIER_SCORE"), 3));
            //returnValue.LoadProperty<double>(UserClassifierProperty, Math.Round(reader.GetDouble("USER_CLASSIFIER_MODEL_SCORE"), 3));
            //returnValue.LoadProperty<double>(NetworkStatisticScoreProperty, Math.Round(reader.GetDouble("NETWORK_STATISTIC_SCORE"), 3));
            //returnValue.LoadProperty<double>(FieldOfStudyDistanceProperty, Math.Round(reader.GetDouble("FOS_DISTANCE_SCORE"), 3));
            //returnValue.LoadProperty<double>(EnsembleScoreProperty, Math.Round(reader.GetDouble("ENSEMBLE_SCORE"), 3));
            returnValue.LoadProperty<int>(NumberScreenedProperty, index);
            if (returnValue.Included == true)
                returnValue.LoadProperty<int>(CumulativeIncludeCountProperty, CumulativeIncludeCount + 1);
            else
                returnValue.LoadProperty<int>(CumulativeIncludeCountProperty, CumulativeIncludeCount);
            //returnValue.MarkOld();
            return returnValue;
        }

#endif
    }
}