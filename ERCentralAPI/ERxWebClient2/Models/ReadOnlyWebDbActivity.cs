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

#if(!SILVERLIGHT && !CSLA_NETCORE)
using Csla.Data;
using System.Data.SqlClient;
using BusinessLibrary.Data;
using BusinessLibrary.Security;
#elif(CSLA_NETCORE)
using Csla.Data;
#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class ReadOnlyWebDbActivity : ReadOnlyBase<ReadOnlyWebDbActivity>
    {
        // st_Eppi_Vis_Get_Log
        // Input: @WEBDB_ID int, @FROM datetime, @UNTIL datetime, @TYPE nvarchar(255)
        // Output: tv_webdb_log_identity int, tv_created datetime, tv_log_type nvarchar(25), tv_details nvarchar(max)

        public ReadOnlyWebDbActivity() { }

        public static readonly PropertyInfo<int> WebDBLogIdentityProperty = RegisterProperty<int>(new PropertyInfo<int>("WebDBLogIdentity", "WebDBLogIdentity", 0));
        public int WebDBLogIdentity
        {
            get
            {
                return GetProperty(WebDBLogIdentityProperty);
            }
        }

        public static readonly PropertyInfo<DateTime> DateTimeCreatedProperty = RegisterProperty<DateTime>(new PropertyInfo<DateTime>("DateTimeCreated", "DateTimeCreated"));
        public DateTime DateTimeCreated
        {
            get
            {
                return GetProperty(DateTimeCreatedProperty);
            }
        }

        public static readonly PropertyInfo<string> LogTypeProperty = RegisterProperty<string>(new PropertyInfo<string>("LogType", "LogType", string.Empty));
        public string LogType
        {
            get
            {
                return GetProperty(LogTypeProperty);
            }
        }
        public static readonly PropertyInfo<string> LogDetailsProperty = RegisterProperty<string>(new PropertyInfo<string>("LogDetails", "LogDetails", string.Empty));
        public string LogDetails
        {
            get
            {
                return GetProperty(LogDetailsProperty);
            }
        }



#if !SILVERLIGHT

        public static ReadOnlyWebDbActivity GetReadOnlyWebDBActivity(SafeDataReader reader)
        {
            return DataPortal.FetchChild<ReadOnlyWebDbActivity>(reader);
        }

        private void Child_Fetch(SafeDataReader reader)
        {
            LoadProperty<int>(WebDBLogIdentityProperty, reader.GetInt32("tv_webdb_log_identity"));
            LoadProperty<DateTime>(DateTimeCreatedProperty, reader.GetDateTime("DateTimeCreated"));
            LoadProperty<string>(LogTypeProperty, reader.GetString("tv_log_type"));
            LoadProperty<string>(LogDetailsProperty, reader.GetString("tv_details"));
        }


#endif
    }

}
