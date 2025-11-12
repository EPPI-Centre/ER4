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
    public class ReadOnlyHelpPageList : ReadOnlyBase<ReadOnlyHelpPageList> 
    {

        public static readonly PropertyInfo<MobileList<ReadOnlyHelpPage>> HelpPagesProperty = RegisterProperty(new PropertyInfo<MobileList<ReadOnlyHelpPage>>("HelpPages", "HelpPages"));
        public MobileList<ReadOnlyHelpPage> HelpPages
        {
            get
            {
                return GetProperty(HelpPagesProperty);
            }

        }

        public static readonly PropertyInfo<string> ContextProperty = RegisterProperty<string>(new PropertyInfo<string>("Context", "Context", string.Empty));
        public string Context
        {
            get
            {
                return GetProperty(ContextProperty);
            }
        }



#if SILVERLIGHT
    public ReadOnlySourceList() { }
#else
        public ReadOnlyHelpPageList()
        {
            LoadProperty(HelpPagesProperty, new MobileList<ReadOnlyHelpPage>());
        }
#endif
        /**/
        public static void GetHelpPages(EventHandler<DataPortalResult<ReadOnlyHelpPageList>> handler)
        {
            DataPortal<ReadOnlyHelpPageList> dp = new DataPortal<ReadOnlyHelpPageList>();
            dp.FetchCompleted += handler;
            dp.BeginFetch();
        }


#if !SILVERLIGHT

        private void DataPortal_Fetch(OnlineHelpCriteria crit)
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.AdmConnectionString))
            {
                LoadProperty<string>(ContextProperty, crit.Context);//we do this in all cases...               
                using (SqlCommand command = new SqlCommand("st_OnlineHelpPages", connection))
                {
                    connection.Open();
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@CONTEXT", crit.Context));
                    using (Csla.Data.SafeDataReader reader1 = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        while (reader1.Read())
                        {                          
                            HelpPages.Add(ReadOnlyHelpPage.GetReadOnlyHelpPage(reader1));
                        }
                        reader1.NextResult();
                    }
                }
                connection.Close();
            }
        }


#endif
    }
}
