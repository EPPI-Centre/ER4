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
using System.Diagnostics.Metrics;

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
    public class WebDbCountryFrequency : ReadOnlyBase<WebDbCountryFrequency>
    {

        public WebDbCountryFrequency()
        {
            LoadProperty<int>(ValueProperty, 0);
            LoadProperty<string>(CountryProperty, "");
        }

        public static readonly PropertyInfo<int> ValueProperty = RegisterProperty<int>(new PropertyInfo<int>("value", "value", 0));
        public int Value
        {
            get
            {
                return GetProperty(ValueProperty);
            }
        }

        public static readonly PropertyInfo<string> CountryProperty = RegisterProperty<string>(new PropertyInfo<string>("country", "country", string.Empty));
        public string Country
        {
            get
            {
                return GetProperty(CountryProperty);
            }
        }

        public static readonly PropertyInfo<double> LatProperty = RegisterProperty<double>(new PropertyInfo<double>("lat", "lat", 0.0));
        public double Lat
        {
            get
            {
                return GetProperty(LatProperty);
            }
        }

        public static readonly PropertyInfo<double> LngProperty = RegisterProperty<double>(new PropertyInfo<double>("lng", "lng", 0.0));
        public double Lng
        {
            get
            {
                return GetProperty(LngProperty);
            }
        }

        public MobileList<double> Location
        {
            get
            {
                MobileList<double> res = new MobileList<double>();
                res.Add(Lat);
                res.Add(Lng);
                return res;
            }
        }


#if !SILVERLIGHT

        public static WebDbCountryFrequency GetWebDbCountryFrequency(SafeDataReader reader)
        {
            return DataPortal.FetchChild<WebDbCountryFrequency>(reader);
        }

        private void Child_Fetch(SafeDataReader reader)
        {
            int value = reader.GetInt32("value");
            string country = reader.GetString("country").Trim();
            double lat = reader.GetDouble("lat");
            double lng = reader.GetDouble("lng");
            if (country == "" || country == "0") country = "Unknown";
            LoadProperty<int>(ValueProperty, value);
            LoadProperty<string>(CountryProperty, country);
            LoadProperty<double>(LatProperty, lat);
            LoadProperty<double>(LngProperty, lng);
        }
        internal void Merge(WebDbCountryFrequency incoming)
        {
            LoadProperty(ValueProperty, Value + incoming.Value);
        }
#endif
    }

}
