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
using Newtonsoft.Json;

#if!SILVERLIGHT
using System.Data.SqlClient;
using BusinessLibrary.Data;
using Csla.Data;
using BusinessLibrary.Security;
#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public class ScreeningFromSearchIteration : BusinessBase<ScreeningFromSearchIteration>
    {

    public ScreeningFromSearchIteration() { }



        public static readonly PropertyInfo<int> TrainingFsIdProperty = RegisterProperty<int>(new PropertyInfo<int>("TrainingFsId", "TrainingFsId"));
        [JsonProperty]
        public int TrainingFsId
        {
            get
            {
                return GetProperty(TrainingFsIdProperty);
            }
        }

        public static readonly PropertyInfo<int> ContactIdProperty = RegisterProperty<int>(new PropertyInfo<int>("ContactId", "ContactId"));
        [JsonProperty]
        public int ContactId
        {
            get
            {
                return GetProperty(ContactIdProperty);
            }
        }
        public static readonly PropertyInfo<int> SearchIdProperty = RegisterProperty<int>(new PropertyInfo<int>("SearchId", "SearchId"));
        [JsonProperty]
        public int SearchId
        {
            get
            {
                return GetProperty(SearchIdProperty);
            }
        }
        public static readonly PropertyInfo<DateTime> DateProperty = RegisterProperty<DateTime>(new PropertyInfo<DateTime>("Date", "Date"));
        [JsonProperty]
        public DateTime Date
        {
            get
            {
                return GetProperty(DateProperty);
            }
        }

        public static readonly PropertyInfo<int> IterationProperty = RegisterProperty<int>(new PropertyInfo<int>("Iteration", "Iteration"));
        [JsonProperty]
        public int Iteration
        {
            get
            {
                return GetProperty(IterationProperty);
            }
        }

        public static readonly PropertyInfo<string> ContactNameProperty = RegisterProperty<string>(new PropertyInfo<string>("ContactName", "ContactName"));
        [JsonProperty]
        public string ContactName
        {
            get
            {
                return GetProperty(ContactNameProperty);
            }
        }

        public static readonly PropertyInfo<Int32> TPProperty = RegisterProperty<Int32>(new PropertyInfo<Int32>("TP", "TP"));
        [JsonProperty]
        public int TP
        {
            get
            {
                return GetProperty(TPProperty);
            }
        }

        public static readonly PropertyInfo<Int32> TNProperty = RegisterProperty<Int32>(new PropertyInfo<Int32>("TN", "TN"));
        [JsonProperty]
        public int TN
        {
            get
            {
                return GetProperty(TNProperty);
            }
        }

        public static readonly PropertyInfo<Int32> TotalScreenedProperty = RegisterProperty<Int32>(new PropertyInfo<Int32>("TotalScreened", "TotalScreened"));
        [JsonProperty]
        public int TotalScreened
        {
            get
            {
                return TP + TN;
            }
        }

        public static readonly PropertyInfo<Int32> LocalTNProperty = RegisterProperty<Int32>(new PropertyInfo<Int32>("LocalTN", "LocalTN"));
        [JsonProperty]
        public int LocalTN
        {
            get
            {
                return GetProperty(LocalTNProperty);
            }
        }

        public static readonly PropertyInfo<Int32> LocalTPProperty = RegisterProperty<Int32>(new PropertyInfo<Int32>("LocalTP", "LocalTP"));
        [JsonProperty]
        public int LocalTP
        {
            get
            {
                return GetProperty(LocalTPProperty);
            }
        }

        public static readonly PropertyInfo<Int32> TotItemsInListProperty = RegisterProperty<Int32>(new PropertyInfo<Int32>("TotItemsInList", "TotItemsInList"));
        [JsonProperty]
        public int TotItemsInList
        {
            get
            {
                return GetProperty(TotItemsInListProperty);
            }
        }
        public static readonly PropertyInfo<Int32> ScreenedFromListProperty = RegisterProperty<Int32>(new PropertyInfo<Int32>("ScreenedFromList", "ScreenedFromList"));
        [JsonProperty]
        public int ScreenedFromList
        {
            get
            {
                return LocalTP + LocalTN;
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
            returnValue.LoadProperty<int>(SearchIdProperty, reader.GetInt32("SEARCH_ID"));
            returnValue.LoadProperty<DateTime>(DateProperty, reader.GetDateTime("DATE"));
            returnValue.LoadProperty<int>(IterationProperty, reader.GetInt32("ITERATION"));
            returnValue.LoadProperty<string>(ContactNameProperty, reader.GetString("CONTACT_NAME"));
            returnValue.LoadProperty<int>(TPProperty, reader.GetInt32("TRUE_POSITIVES"));
            returnValue.LoadProperty<int>(TNProperty, reader.GetInt32("TRUE_NEGATIVES"));
            returnValue.LoadProperty<int>(LocalTPProperty, reader.GetInt32("LOCAL_TP"));
            returnValue.LoadProperty<int>(LocalTNProperty, reader.GetInt32("LOCAL_TN"));
            returnValue.LoadProperty<int>(TotItemsInListProperty, reader.GetInt32("LOCAL_TOT"));
            returnValue.MarkOld();
            return returnValue;
        }

#endif

    }
}
