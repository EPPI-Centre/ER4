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
using Csla.Data;
using System.Data.SqlClient;
using BusinessLibrary.Data;
using BusinessLibrary.Security;
#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class WebDbYearFrequencyList : ReadOnlyListBase<WebDbYearFrequencyList, WebDbYearFrequency>
    {

        public WebDbYearFrequencyList() { }

        
#if !SILVERLIGHT

        private void DataPortal_Fetch(WebDbYearFrequencyCrit criteria)
        {

            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            RaiseListChangedEvents = false;
            IsReadOnly = false;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_WebDbYearFrequency", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@RevId", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@WebDbId", criteria.WebDbId));
                    command.Parameters.Add(new SqlParameter("@included", criteria.Included));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        WebDbYearFrequency unknowns = new WebDbYearFrequency();
                        while (reader.Read())
                        {
                            WebDbYearFrequency current = WebDbYearFrequency.GetWebDbYearFrequency(reader);
                            if (current.Year == "Unknown" || current.Year =="")
                            {
                                current.Merge(unknowns);
                                unknowns = current;
                            }
                            else
                            {
                                Add(current);
                            }
                        }
                        if (unknowns.Count > 0) Add(unknowns);
                    }
                }
                connection.Close();
            }
            IsReadOnly = true;
            RaiseListChangedEvents = true;
        }

#endif
    }
    [Serializable]
    public class WebDbYearFrequencyCrit : CriteriaBase<WebDbYearFrequencyCrit>
    {
        public WebDbYearFrequencyCrit(int WebDbId, bool included)
        //: base(type)
        {
            LoadProperty(WebDbIdProperty, WebDbId);
            LoadProperty(IncludedProperty, included);
        }
        public WebDbYearFrequencyCrit() { }

        private static PropertyInfo<int> WebDbIdProperty = RegisterProperty<int>(new PropertyInfo<int>("WebDbId", "WebDbId"));
        public int WebDbId
        {
            get { return ReadProperty(WebDbIdProperty); }
            set { }
        }
        private static PropertyInfo<bool> IncludedProperty = RegisterProperty<bool>(new PropertyInfo<bool>("Included", "Included"));
        public bool Included
        {
            get { return ReadProperty(IncludedProperty); }
        }
    }
}
