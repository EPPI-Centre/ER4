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
    public class ReadOnlyReviewSetOutcome : ReadOnlyBase<ReadOnlyReviewSetOutcome>
    {
#if SILVERLIGHT
    public ReadOnlyReviewSetOutcome()
    {
        
    }
#else
        private ReadOnlyReviewSetOutcome()
        {
            //LoadProperty(ReadOnlyReviewSetOutcomeProperty, ReadOnlyReviewSetOutcome.NewReadOnlyReviewSetOutcome());
        }
#endif
        public override string ToString()
        {
            return AttributeName;
        }

        internal static ReadOnlyReviewSetOutcome NewReadOnlyReviewSetOutcome()
        {
            ReadOnlyReviewSetOutcome returnValue = new ReadOnlyReviewSetOutcome();
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
        //    //AuthorizationRules.AllowCreate(typeof(ReadOnlyReviewSetOutcome), admin);
        //    //AuthorizationRules.AllowDelete(typeof(ReadOnlyReviewSetOutcome), admin);
        //    //AuthorizationRules.AllowEdit(typeof(ReadOnlyReviewSetOutcome), canWrite);
        //    //AuthorizationRules.AllowGet(typeof(ReadOnlyReviewSetOutcome), canRead);

        //    //AuthorizationRules.AllowRead(AttributeIdProperty, canRead);
        //    //AuthorizationRules.AllowRead(AttributeNameProperty, canRead);
        //}



#if !SILVERLIGHT

        internal static ReadOnlyReviewSetOutcome GetReadOnlyReviewSetOutcome(SafeDataReader reader)
        {
            ReadOnlyReviewSetOutcome returnValue = new ReadOnlyReviewSetOutcome();
            returnValue.LoadProperty<Int64>(AttributeIdProperty, reader.GetInt64("ATTRIBUTE_ID"));
            returnValue.LoadProperty<string>(AttributeNameProperty, reader.GetString("ATTRIBUTE_NAME"));
            return returnValue;
        }

#endif
    }
}
