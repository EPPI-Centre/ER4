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
    public class ReadOnlySourceList : ReadOnlyBase<ReadOnlySourceList> //ReadOnlyListBase<ReadOnlySourceList, ReadOnlySource>
    {
        //private int _SourcelessItems;
        private static PropertyInfo<int> Sourceless_ItemsProperty = RegisterProperty<int>(new PropertyInfo<int>("Sourceless_Items", "Sourceless_Items"));
        public int Sourceless_Items
        {
            get
            {
                return GetProperty(Sourceless_ItemsProperty);
            }
        }
        private static PropertyInfo<MobileList<ReadOnlySource>> SourcesProperty = RegisterProperty(new PropertyInfo<MobileList<ReadOnlySource>>("Sources", "Sources"));
        public MobileList<ReadOnlySource> Sources
        {
            get
            {
                return GetProperty(SourcesProperty);
            }
            
        }

#if SILVERLIGHT
    public ReadOnlySourceList() { }
#else
        private ReadOnlySourceList() 
        {
            LoadProperty( SourcesProperty, new MobileList<ReadOnlySource>());
        }
#endif
        /**/
        public static void GetSources(EventHandler<DataPortalResult<ReadOnlySourceList>> handler)
        {
            DataPortal<ReadOnlySourceList> dp = new DataPortal<ReadOnlySourceList>();
            dp.FetchCompleted += handler;
            dp.BeginFetch();
        }

#if !SILVERLIGHT

        private void DataPortal_Fetch()
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            //RaiseListChangedEvents = false;
            //IsReadOnly = false;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_SourceFromReview_ID", connection))
                {
                    
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@revID", ri.ReviewId));
                    //command.CommandTimeout = 100;
                    //Sources = new MobileList<string>();
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        while (reader.Read())
                        {
                            Sources.Add(ReadOnlySource.GetReadOnlySource(reader));
                        }
                        //reader.NextResult();
                        //reader.Read();
                        //LoadProperty(Sourceless_ItemsProperty, reader.GetInt32("Total_Items"));
                    }
                }
                connection.Close();
            }
            //IsReadOnly = true;
           // RaiseListChangedEvents = true;
        }

#endif
    }
}
