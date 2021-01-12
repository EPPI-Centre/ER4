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
    public class WebDbYearFrequency : ReadOnlyBase<WebDbYearFrequency>
    {

        public WebDbYearFrequency() 
        {
            LoadProperty<int>(CountProperty, 0);
            LoadProperty<string>(YearProperty, "");
        }

        public static readonly PropertyInfo<int> CountProperty = RegisterProperty<int>(new PropertyInfo<int>("Count", "Count", 0));
        public int Count
        {
            get
            {
                return GetProperty(CountProperty);
            }
        }

        public static readonly PropertyInfo<string> YearProperty = RegisterProperty<string>(new PropertyInfo<string>("Year", "Year", string.Empty));
        public string Year
        {
            get
            {
                return GetProperty(YearProperty);
            }
        }
        
#if !SILVERLIGHT

        public static WebDbYearFrequency GetWebDbYearFrequency(SafeDataReader reader)
        {
            return DataPortal.FetchChild<WebDbYearFrequency>(reader);
        }

        private void Child_Fetch(SafeDataReader reader)
        {
            int count = reader.GetInt32("Count");
            string year = reader.GetString("year").Trim();
            if (year == "" || year == "0") year = "Unknown";
            LoadProperty<int>(CountProperty, count);
            LoadProperty<string>(YearProperty, year);
        }
        internal void Merge(WebDbYearFrequency incoming)
        {
            LoadProperty(CountProperty, Count + incoming.Count);
        } 
#endif
    }

}
