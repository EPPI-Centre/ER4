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
    public class Training : BusinessBase<Training>
    {
#if SILVERLIGHT
    public Training() { }

        
#else
        private Training() { }
#endif

        private static PropertyInfo<int> TrainingIdProperty = RegisterProperty<int>(new PropertyInfo<int>("TrainingId", "TrainingId"));
        public int TrainingId
        {
            get
            {
                return GetProperty(TrainingIdProperty);
            }
        }

        private static PropertyInfo<int> ContactIdProperty = RegisterProperty<int>(new PropertyInfo<int>("ContactId", "ContactId"));
        public int ContactId
        {
            get
            {
                return GetProperty(ContactIdProperty);
            }
        }

        private static PropertyInfo<DateTime> StartTimeProperty = RegisterProperty<DateTime>(new PropertyInfo<DateTime>("StartTime", "StartTime"));
        public DateTime StartTime
        {
            get
            {
                return GetProperty(StartTimeProperty);
            }
        }

        private static PropertyInfo<DateTime> EndTimeProperty = RegisterProperty<DateTime>(new PropertyInfo<DateTime>("EndTime", "EndTime"));
        public DateTime EndTime
        {
            get
            {
                return GetProperty(EndTimeProperty);
            }
        }

        private static PropertyInfo<int> IterationProperty = RegisterProperty<int>(new PropertyInfo<int>("Iteration", "Iteration"));
        public int Iteration
        {
            get
            {
                return GetProperty(IterationProperty);
            }
        }

        private static PropertyInfo<int> NTrainingItemsIncProperty = RegisterProperty<int>(new PropertyInfo<int>("NTrainingItemsInc", "NTrainingItemsInc"));
        public int NTrainingItemsInc
        {
            get
            {
                return GetProperty(NTrainingItemsIncProperty);
            }
        }

        private static PropertyInfo<int> NTrainingItemsExcProperty = RegisterProperty<int>(new PropertyInfo<int>("NTrainingItemsExc", "NTrainingItemsExc"));
        public int NTrainingItemsExc
        {
            get
            {
                return GetProperty(NTrainingItemsExcProperty);
            }
        }

        private static PropertyInfo<string> ContactNameProperty = RegisterProperty<string>(new PropertyInfo<string>("ContactName", "ContactName"));
        public string ContactName
        {
            get
            {
                return GetProperty(ContactNameProperty);
            }
        }

        private static PropertyInfo<double> CProperty = RegisterProperty<double>(new PropertyInfo<double>("C", "C"));
        public double C
        {
            get
            {
                return GetProperty(CProperty);
            }
        }

        private static PropertyInfo<Int32> TPProperty = RegisterProperty<Int32>(new PropertyInfo<Int32>("TP", "TP"));
        public double TP
        {
            get
            {
                return GetProperty(TPProperty);
            }
        }

        private static PropertyInfo<Int32> TNProperty = RegisterProperty<Int32>(new PropertyInfo<Int32>("TN", "TN"));
        public double TN
        {
            get
            {
                return GetProperty(TNProperty);
            }
        }

        private static PropertyInfo<Int32> FPProperty = RegisterProperty<Int32>(new PropertyInfo<Int32>("FP", "FP"));
        public double FP
        {
            get
            {
                return GetProperty(FPProperty);
            }
        }

        private static PropertyInfo<Int32> FNProperty = RegisterProperty<Int32>(new PropertyInfo<Int32>("FN", "FN"));
        public double FN
        {
            get
            {
                return GetProperty(FNProperty);
            }
        }

        private static PropertyInfo<Int32> TotalNProperty = RegisterProperty<Int32>(new PropertyInfo<Int32>("TotalN", "TotalN"));
        public double TotalN
        {
            get
            {
                return TP + TN + FP + FN;
            }
        }

        private static PropertyInfo<Int32> TotalIncludesProperty = RegisterProperty<Int32>(new PropertyInfo<Int32>("TotalIncludes", "TotalIncludes"));
        public double TotalIncludes
        {
            get
            {
                return TP + FN;
            }
        }

        private static PropertyInfo<Int32> TotalExcludesProperty = RegisterProperty<Int32>(new PropertyInfo<Int32>("TotalExcludes", "TotalExcludes"));
        public double TotalExcludes
        {
            get
            {
                return FP + TN;
            }
        }


        //protected override void AddAuthorizationRules()
        //{
        //    //string[] canWrite = new string[] { "AdminUser", "RegularUser" };
        //    //string[] canRead = new string[] { "AdminUser", "RegularUser", "ReadOnlyUser" };
        //    //string[] admin = new string[] { "AdminUser" };
        //    //AuthorizationRules.AllowCreate(typeof(Training), admin);
        //    //AuthorizationRules.AllowDelete(typeof(Training), admin);
        //    //AuthorizationRules.AllowEdit(typeof(Training), canWrite);
        //    //AuthorizationRules.AllowGet(typeof(Training), canRead);

        //    //AuthorizationRules.AllowRead(TrainingIdProperty, canRead);
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
            
        }

        protected override void DataPortal_Update()
        {
            
        }

        protected override void DataPortal_DeleteSelf()
        {
            
        }

        internal static Training GetTraining(SafeDataReader reader)
        {
            Training returnValue = new Training();
            returnValue.LoadProperty<int>(TrainingIdProperty, reader.GetInt32("TRAINING_ID"));
            returnValue.LoadProperty<int>(ContactIdProperty, reader.GetInt32("CONTACT_ID"));
            returnValue.LoadProperty<DateTime>(StartTimeProperty, reader.GetDateTime("TIME_STARTED"));
            returnValue.LoadProperty<DateTime>(EndTimeProperty, reader.GetDateTime("TIME_ENDED"));
            returnValue.LoadProperty<int>(IterationProperty, reader.GetInt32("ITERATION"));
            returnValue.LoadProperty<int>(NTrainingItemsIncProperty, reader.GetInt32("N_TRAINING_INC"));
            returnValue.LoadProperty<int>(NTrainingItemsExcProperty, reader.GetInt32("N_TRAINING_EXC"));
            returnValue.LoadProperty<string>(ContactNameProperty, reader.GetString("CONTACT_NAME"));
            returnValue.LoadProperty<double>(CProperty, reader.GetDouble("C"));
            returnValue.LoadProperty<int>(TPProperty, reader.GetInt32("TRUE_POSITIVES"));
            returnValue.LoadProperty<int>(TNProperty, reader.GetInt32("TRUE_NEGATIVES"));
            returnValue.LoadProperty<int>(FPProperty, reader.GetInt32("FALSE_POSITIVES"));
            returnValue.LoadProperty<int>(FNProperty, reader.GetInt32("FALSE_NEGATIVES"));
            returnValue.MarkOld();
            return returnValue;
        }

#endif

    }
}
