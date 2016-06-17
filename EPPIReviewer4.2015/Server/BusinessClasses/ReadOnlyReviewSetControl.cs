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

#if!SILVERLIGHT
using System.Data.SqlClient;
using BusinessLibrary.Data;
using Csla.Data;
#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class ReadOnlyReviewSetControl : ReadOnlyBase<ReadOnlyReviewSetControl>
    {
#if SILVERLIGHT
    public ReadOnlyReviewSetControl()
    {
        
    }
#else
        private ReadOnlyReviewSetControl()
        {
            //LoadProperty(ReadOnlyReviewSetControlProperty, ReadOnlyReviewSetControl.NewReadOnlyReviewSetControl());
        }
#endif
        public override string ToString()
        {
            return AttributeName;
        }

        internal static ReadOnlyReviewSetControl NewReadOnlyReviewSetControl()
        {
            ReadOnlyReviewSetControl returnValue = new ReadOnlyReviewSetControl();
            return returnValue;
        }

        private static PropertyInfo<Int64> AttributeIdProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("AttributeId", "AttributeId"));
        public Int64 AttributeId
        {
            get
            {
                return GetProperty(AttributeIdProperty);
            }
        }

        private static PropertyInfo<string> AttributeNameProperty = RegisterProperty<string>(new PropertyInfo<string>("AttributeName", "AttributeName", string.Empty));
        public string AttributeName
        {
            get
            {
                return GetProperty(AttributeNameProperty);
            }
        }

        //protected override void AddAuthorizationRules()
        //{
        //    //string[] canWrite = new string[] { "AdminUser", "RegularUser" };
        //    //string[] canRead = new string[] { "AdminUser", "RegularUser", "ReadOnlyUser" };
        //    //string[] admin = new string[] { "AdminUser" };
        //    //AuthorizationRules.AllowCreate(typeof(ReadOnlyReviewSetControl), admin);
        //    //AuthorizationRules.AllowDelete(typeof(ReadOnlyReviewSetControl), admin);
        //    //AuthorizationRules.AllowEdit(typeof(ReadOnlyReviewSetControl), canWrite);
        //    //AuthorizationRules.AllowGet(typeof(ReadOnlyReviewSetControl), canRead);

        //    //AuthorizationRules.AllowRead(AttributeIdProperty, canRead);
        //    //AuthorizationRules.AllowRead(AttributeNameProperty, canRead);
        //}



#if !SILVERLIGHT

        internal static ReadOnlyReviewSetControl GetReadOnlyReviewSetControl(SafeDataReader reader)
        {
            ReadOnlyReviewSetControl returnValue = new ReadOnlyReviewSetControl();
            returnValue.LoadProperty<Int64>(AttributeIdProperty, reader.GetInt64("ATTRIBUTE_ID"));
            returnValue.LoadProperty<string>(AttributeNameProperty, reader.GetString("ATTRIBUTE_NAME"));
            return returnValue;
        }

#endif
    }
}
