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
using System.ComponentModel;

#if!SILVERLIGHT
using Csla.Data;
using System.Data.SqlClient;
using BusinessLibrary.Data;
#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class WebDbItemAttributeCrosstabRow : ReadOnlyBase<WebDbItemAttributeCrosstabRow>
    {


        public WebDbItemAttributeCrosstabRow() { }


        public static readonly PropertyInfo<Int64> AttributeIdProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("AttributeId", "AttributeId"));
        public Int64 AttributeId
        {
            get
            {
                return GetProperty(AttributeIdProperty);
            }
        }

		public static readonly PropertyInfo<string> AttributeNameProperty = RegisterProperty<string>(new PropertyInfo<string>("AttributeName", "AttributeName", string.Empty));
        public string AttributeName
        {
            get
            {
                return GetProperty(AttributeNameProperty);
            }
        }

        public static readonly PropertyInfo<MobileList<int>> CountsProperty = RegisterProperty<MobileList<int>>(new PropertyInfo<MobileList<int>>("Counts", "Counts", new MobileList<int>()));
        public MobileList<int> Counts
        {
            get
            {
                return GetProperty(CountsProperty);
            }
        }
        //protected override void AddAuthorizationRules()
        //{
        //    //string[] canRead = new string[] { "AdminUser", "RegularUser", "ReadOnlyUser" };
        //    //AuthorizationRules.AllowGet(typeof(ReadOnlyItemAttributeCrosstab), canRead);
        //}

#if !SILVERLIGHT

        public static WebDbItemAttributeCrosstabRow GetReadOnlyItemAttributeCrosstabRow(long AttId, string AttName)
        {
            return DataPortal.FetchChild<WebDbItemAttributeCrosstabRow>(AttId, AttName);
        }

        private void Child_Fetch(long AttId, string AttName)
        {
            LoadProperty<Int64>(AttributeIdProperty, AttId);
            LoadProperty<string>(AttributeNameProperty, AttName);
            LoadProperty<MobileList<int>>(CountsProperty, new MobileList<int>());
        }


#endif
    }
}
