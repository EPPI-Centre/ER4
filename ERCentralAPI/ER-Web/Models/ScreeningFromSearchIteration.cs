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
    public class ScreeningFromSearchIteration : BusinessBase<ScreeningFromSearchIteration>
    {

    public ScreeningFromSearchIteration() { }



        public static readonly PropertyInfo<int> TrainingFsIdProperty = RegisterProperty<int>(new PropertyInfo<int>("TrainingFsId", "TrainingFsId"));
        public int TrainingFsId
        {
            get
            {
                return GetProperty(TrainingFsIdProperty);
            }
        }

        public static readonly PropertyInfo<int> ContactIdProperty = RegisterProperty<int>(new PropertyInfo<int>("ContactId", "ContactId"));
        public int ContactId
        {
            get
            {
                return GetProperty(ContactIdProperty);
            }
        }
        public static readonly PropertyInfo<int> SearchIdProperty = RegisterProperty<int>(new PropertyInfo<int>("SearchId", "SearchId"));
        public int SearchId
        {
            get
            {
                return GetProperty(SearchIdProperty);
            }
        }
        public static readonly PropertyInfo<DateTime> DateProperty = RegisterProperty<DateTime>(new PropertyInfo<DateTime>("Date", "Date"));
        public DateTime Date
        {
            get
            {
                return GetProperty(DateProperty);
            }
        }

        public static readonly PropertyInfo<int> IterationProperty = RegisterProperty<int>(new PropertyInfo<int>("Iteration", "Iteration"));
        public int Iteration
        {
            get
            {
                return GetProperty(IterationProperty);
            }
        }

        public static readonly PropertyInfo<string> ContactNameProperty = RegisterProperty<string>(new PropertyInfo<string>("ContactName", "ContactName"));
        public string ContactName
        {
            get
            {
                return GetProperty(ContactNameProperty);
            }
        }

        public static readonly PropertyInfo<Int32> TPProperty = RegisterProperty<Int32>(new PropertyInfo<Int32>("TP", "TP"));
        public double TP
        {
            get
            {
                return GetProperty(TPProperty);
            }
        }

        public static readonly PropertyInfo<Int32> TNProperty = RegisterProperty<Int32>(new PropertyInfo<Int32>("TN", "TN"));
        public double TN
        {
            get
            {
                return GetProperty(TNProperty);
            }
        }


        public static readonly PropertyInfo<Int32> TotalNProperty = RegisterProperty<Int32>(new PropertyInfo<Int32>("TotalN", "TotalN"));
        public double TotalN
        {
            get
            {
                return TP + TN;
            }
        }

        public double TotalIncludes
        {
            get
            {
                return TP;
            }
        }

        
        public double TotalExcludes
        {
            get
            {
                return TN;
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

        internal static ScreeningFromSearchIteration GetTraining(SafeDataReader reader)
        {
            ScreeningFromSearchIteration returnValue = new ScreeningFromSearchIteration();
            returnValue.LoadProperty<int>(TrainingFsIdProperty, reader.GetInt32("TRAINING_FS_ID"));
            returnValue.LoadProperty<int>(ContactIdProperty, reader.GetInt32("CONTACT_ID"));
            returnValue.LoadProperty<int>(ContactIdProperty, reader.GetInt32("CONTACT_ID"));
            returnValue.LoadProperty<DateTime>(DateProperty, reader.GetDateTime("DATE"));
            returnValue.LoadProperty<int>(IterationProperty, reader.GetInt32("ITERATION"));
            returnValue.LoadProperty<string>(ContactNameProperty, reader.GetString("CONTACT_NAME"));
            returnValue.LoadProperty<int>(TPProperty, reader.GetInt32("TRUE_POSITIVES"));
            returnValue.LoadProperty<int>(TNProperty, reader.GetInt32("TRUE_NEGATIVES"));
            returnValue.MarkOld();
            return returnValue;
        }

#endif

    }
}
