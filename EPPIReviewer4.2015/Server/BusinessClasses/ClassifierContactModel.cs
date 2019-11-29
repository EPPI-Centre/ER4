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
    public class ClassifierContactModel : BusinessBase<ClassifierContactModel>
    {
#if SILVERLIGHT
    public ClassifierContactModel() { }

        
#else
        public ClassifierContactModel() { }
#endif

        public override string ToString()
        {
            return ModelTitle;
        }

        public static readonly PropertyInfo<int> ModelIdProperty = RegisterProperty<int>(new PropertyInfo<int>("ModelId", "ModelId"));
        public int ModelId
        {
            get
            {
                return GetProperty(ModelIdProperty);
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

        public static readonly PropertyInfo<int> ReviewIdProperty = RegisterProperty<int>(new PropertyInfo<int>("ReviewId", "ReviewId"));
        public int ReviewId
        {
            get
            {
                return GetProperty(ReviewIdProperty);
            }
        }

        public static readonly PropertyInfo<string> ReviewNameProperty = RegisterProperty<string>(new PropertyInfo<string>("ReviewName", "ReviewName"));
        public string ReviewName
        {
            get
            {
                return GetProperty(ReviewNameProperty);
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

        public static readonly PropertyInfo<string> ModelTitleProperty = RegisterProperty<string>(new PropertyInfo<string>("ModelTitle", "ModelTitle"));
        public string ModelTitle
        {
            get
            {
                return GetProperty(ModelTitleProperty);
            }
        }

        public static readonly PropertyInfo<string> AttributeOnProperty = RegisterProperty<string>(new PropertyInfo<string>("AttributeOn", "AttributeOn"));
        public string AttributeOn
        {
            get
            {
                return GetProperty(AttributeOnProperty);
            }
        }

        public static readonly PropertyInfo<string> AttributeNotOnProperty = RegisterProperty<string>(new PropertyInfo<string>("AttributeNotOn", "AttributeNotOn"));
        public string AttributeNotOn
        {
            get
            {
                return GetProperty(AttributeNotOnProperty);
            }
        }

        public static readonly PropertyInfo<decimal> AccuracyProperty = RegisterProperty<decimal>(new PropertyInfo<decimal>("Accuracy", "Accuracy"));
        public decimal Accuracy
        {
            get
            {
                return GetProperty(AccuracyProperty);
            }
        }

        public static readonly PropertyInfo<decimal> AucProperty = RegisterProperty<decimal>(new PropertyInfo<decimal>("Auc", "Auc"));
        public decimal Auc
        {
            get
            {
                return GetProperty(AucProperty);
            }
        }

        public static readonly PropertyInfo<decimal> PrecisionProperty = RegisterProperty<decimal>(new PropertyInfo<decimal>("Precision", "Precision"));
        public decimal Precision
        {
            get
            {
                return GetProperty(PrecisionProperty);
            }
        }

        public static readonly PropertyInfo<decimal> RecallProperty = RegisterProperty<decimal>(new PropertyInfo<decimal>("Recall", "Recall"));
        public decimal Recall
        {
            get
            {
                return GetProperty(RecallProperty);
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

        internal static ClassifierContactModel GetClassifierContactModel(SafeDataReader reader)
        {
            ClassifierContactModel returnValue = new ClassifierContactModel();
            returnValue.LoadProperty<int>(ContactIdProperty, reader.GetInt32("CONTACT_ID"));
            returnValue.LoadProperty<int>(ReviewIdProperty, reader.GetInt32("REVIEW_ID"));
            returnValue.LoadProperty<string>(ReviewNameProperty, reader.GetString("REVIEW_NAME"));

            returnValue.LoadProperty<string>(ContactNameProperty, reader.GetString("CONTACT_NAME"));
            returnValue.LoadProperty<decimal>(AccuracyProperty, reader.GetDecimal("ACCURACY"));
            returnValue.LoadProperty<decimal>(AucProperty, reader.GetDecimal("AUC"));
            returnValue.LoadProperty<decimal>(PrecisionProperty, reader.GetDecimal("PRECISION"));
            returnValue.LoadProperty<decimal>(RecallProperty, reader.GetDecimal("RECALL"));

            returnValue.LoadProperty<int>(ModelIdProperty, reader.GetInt32("MODEL_ID"));
            returnValue.LoadProperty<string>(ModelTitleProperty, reader.GetString("MODEL_TITLE"));
            returnValue.LoadProperty<string>(AttributeOnProperty, reader.GetString("ATTRIBUTE_ON"));
            returnValue.LoadProperty<string>(AttributeNotOnProperty, reader.GetString("ATTRIBUTE_NOT_ON"));
            returnValue.MarkOld();
            return returnValue;
        }

#endif

    }
}
