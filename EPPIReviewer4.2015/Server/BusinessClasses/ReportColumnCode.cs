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
    public class ReportColumnCode : BusinessBase<ReportColumnCode>
    {
#if SILVERLIGHT
    public ReportColumnCode() { }

        
#else
        private ReportColumnCode() { }
#endif

        public override string ToString()
        {
            return UserDefText;
        }

        public void SetCodeAsNew()
        {
            this.MarkNew();
        }

        private static PropertyInfo<int> ReportColumnCodeIdProperty = RegisterProperty<int>(new PropertyInfo<int>("ReportColumnCodeId", "ReportColumnCodeId"));
        public int ReportColumnCodeId
        {
            get
            {
                return GetProperty(ReportColumnCodeIdProperty);
            }
        }

        private static PropertyInfo<int> ReportColumnIdProperty = RegisterProperty<int>(new PropertyInfo<int>("ReportColumnId", "ReportColumnId"));
        public int ReportColumnId
        {
            get
            {
                return GetProperty(ReportColumnIdProperty);
            }
            set
            {
                SetProperty(ReportColumnIdProperty, value);
            }
        }

        private static PropertyInfo<int> CodeOrderProperty = RegisterProperty<int>(new PropertyInfo<int>("CodeOrder", "CodeOrder", 0));
        public int CodeOrder
        {
            get
            {
                return GetProperty(CodeOrderProperty);
            }
            set
            {
                SetProperty(CodeOrderProperty, value);
            }
        }

        private static PropertyInfo<int> SetIdProperty = RegisterProperty<int>(new PropertyInfo<int>("SetId", "SetId", 0));
        public int SetId
        {
            get
            {
                return GetProperty(SetIdProperty);
            }
            set
            {
                SetProperty(SetIdProperty, value);
            }
        }

        private static PropertyInfo<Int64> AttributeIdProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("AttributeId", "AttributeId"));
        public Int64 AttributeId
        {
            get
            {
                return GetProperty(AttributeIdProperty);
            }
            set
            {
                SetProperty(AttributeIdProperty, value);
            }
        }

        private static PropertyInfo<Int64> ParentAttributeIdProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("ParentAttributeId", "ParentAttributeId"));
        public Int64 ParentAttributeId
        {
            get
            {
                return GetProperty(ParentAttributeIdProperty);
            }
            set
            {
                SetProperty(ParentAttributeIdProperty, value);
            }
        }

        private static PropertyInfo<string> ParentAttributeTextProperty = RegisterProperty<string>(new PropertyInfo<string>("ParentAttributeText", "ParentAttributeText", string.Empty));
        public string ParentAttributeText
        {
            get
            {
                return GetProperty(ParentAttributeTextProperty);
            }
            set
            {
                SetProperty(ParentAttributeTextProperty, value);
            }
        }

        private static PropertyInfo<string> UserDefTextProperty = RegisterProperty<string>(new PropertyInfo<string>("UserDefText", "UserDefText", string.Empty));
        public string UserDefText
        {
            get
            {
                return GetProperty(UserDefTextProperty);
            }
            set
            {
                SetProperty(UserDefTextProperty, value);
            }
        }

        private static PropertyInfo<bool> DisplayCodeProperty = RegisterProperty<bool>(new PropertyInfo<bool>("DisplayCode", "DisplayCode", false));
        public bool DisplayCode
        {
            get
            {
                return GetProperty(DisplayCodeProperty);
            }
            set
            {
                SetProperty(DisplayCodeProperty, value);
            }
        }

        private static PropertyInfo<bool> DisplayAdditionalTextProperty = RegisterProperty<bool>(new PropertyInfo<bool>("DisplayAdditionalText", "DisplayAdditionalText", false));
        public bool DisplayAdditionalText
        {
            get
            {
                return GetProperty(DisplayAdditionalTextProperty);
            }
            set
            {
                SetProperty(DisplayAdditionalTextProperty, value);
            }
        }

        private static PropertyInfo<bool> DisplayCodedTextProperty = RegisterProperty<bool>(new PropertyInfo<bool>("DisplayCodedText", "DisplayCodedText", false));
        public bool DisplayCodedText
        {
            get
            {
                return GetProperty(DisplayCodedTextProperty);
            }
            set
            {
                SetProperty(DisplayCodedTextProperty, value);
            }
        }

        //protected override void AddAuthorizationRules()
        //{
        //    //string[] canWrite = new string[] { "AdminUser", "RegularUser" };
        //    //string[] canRead = new string[] { "AdminUser", "RegularUser", "ReadOnlyUser" };
        //    //string[] admin = new string[] { "AdminUser" };
        //    //AuthorizationRules.AllowCreate(typeof(ReportColumnCode), admin);
        //    //AuthorizationRules.AllowDelete(typeof(ReportColumnCode), admin);
        //    //AuthorizationRules.AllowEdit(typeof(ReportColumnCode), canWrite);
        //    //AuthorizationRules.AllowGet(typeof(ReportColumnCode), canRead);

        //    //AuthorizationRules.AllowRead(ReportColumnCodeIdProperty, canRead);
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
            // HANDLED ELSEWHERE
        }

        protected override void DataPortal_Update()
        {
            // NOT NEEDED
        }

        protected override void DataPortal_DeleteSelf()
        {
            // NOT NEEDED - DELETED ON UPDATE OF PARENT AND RE-CREATED   
        }

        internal static ReportColumnCode GetReportColumnCode(SafeDataReader reader)
        {
            ReportColumnCode returnValue = new ReportColumnCode();
            returnValue.LoadProperty<int>(ReportColumnCodeIdProperty, reader.GetInt32("REPORT_COLUMN_CODE_ID"));
            returnValue.LoadProperty<int>(ReportColumnIdProperty, reader.GetInt32("REPORT_COLUMN_ID"));
            returnValue.LoadProperty<int>(CodeOrderProperty, reader.GetInt32("CODE_ORDER"));
            returnValue.LoadProperty<int>(SetIdProperty, reader.GetInt32("SET_ID"));
            returnValue.LoadProperty<Int64>(AttributeIdProperty, reader.GetInt64("ATTRIBUTE_ID"));
            returnValue.LoadProperty<Int64>(ParentAttributeIdProperty, reader.GetInt64("PARENT_ATTRIBUTE_ID"));
            returnValue.LoadProperty<string>(ParentAttributeTextProperty, reader.GetString("PARENT_ATTRIBUTE_TEXT"));
            returnValue.LoadProperty<string>(UserDefTextProperty, reader.GetString("USER_DEF_TEXT"));
            returnValue.LoadProperty<bool>(DisplayCodeProperty, reader.GetBoolean("DISPLAY_CODE"));
            returnValue.LoadProperty<bool>(DisplayAdditionalTextProperty, reader.GetBoolean("DISPLAY_ADDITIONAL_TEXT"));
            returnValue.LoadProperty<bool>(DisplayCodedTextProperty, reader.GetBoolean("DISPLAY_CODED_TEXT"));

            returnValue.MarkOld();
            return returnValue;
        }

#endif

    }
}
