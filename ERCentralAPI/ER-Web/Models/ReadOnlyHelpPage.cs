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
using BusinessLibrary.Security;

#if !SILVERLIGHT
using Csla.Data;
using System.Data.SqlClient;
using BusinessLibrary.Data;
#endif


namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class ReadOnlyHelpPage : ReadOnlyBase<ReadOnlyHelpPage>
    {
        public ReadOnlyHelpPage() { }


        public static readonly PropertyInfo<string> Context_NameProperty = RegisterProperty<string>(new PropertyInfo<string>("Context_Name", "Context_Name"));
        public string Context_Name
        {
            get
            {
                return GetProperty(Context_NameProperty);
            }
        }

        public static readonly PropertyInfo<int> OnlineHelp_IDProperty = RegisterProperty<int>(new PropertyInfo<int>("OnlineHelp_ID", "OnlineHelp_ID"));
        public int OnlineHelp_ID
        {
            get
            {
                return GetProperty(OnlineHelp_IDProperty);
            }
        }


#if !SILVERLIGHT

        public static ReadOnlyHelpPage GetReadOnlyHelpPage(SafeDataReader reader)
        {
            return DataPortal.FetchChild<ReadOnlyHelpPage>(reader);
        }

        private void Child_Fetch(SafeDataReader reader)
        {
            LoadProperty<string>(Context_NameProperty, reader.GetString("tv_context"));
            LoadProperty<int>(OnlineHelp_IDProperty, reader.GetInt32("tv_online_help_id"));
        }


#endif
    }
}
