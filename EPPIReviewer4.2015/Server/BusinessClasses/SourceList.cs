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
    public class SourceList : DynamicBindingListBase<Source>
    {
#if SILVERLIGHT
    public SourceList() { }
#else
        private SourceList() { }
#endif
        /*private static PropertyInfo<MobileList<string>> SourcesProperty = RegisterProperty(typeof(ReadOnlySources), new PropertyInfo<MobileList<string>>("Sources", "Sources"));
        public MobileList<string> Sources
        {
            get
            {
                return GetProperty(SourcesProperty);
            }
            
        }*/
        public static void GetSources(EventHandler<DataPortalResult<SourceList>> handler)
        {
            DataPortal<SourceList> dp = new DataPortal<SourceList>();
            dp.FetchCompleted += handler;
            dp.BeginFetch();
        }

#if !SILVERLIGHT

        private void DataPortal_Fetch()
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            RaiseListChangedEvents = false;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_SourceFromReview_ID_Extended", connection))
                {

                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@revID", ri.ReviewId));
                    //Sources = new MobileList<string>();
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        while (reader.Read())
                        {
                            Add(Source.GetSource(reader));
                        }
                    }
                }
                connection.Close();
            }
            RaiseListChangedEvents = true;
        }

#endif
    }
}
