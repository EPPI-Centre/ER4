using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Csla;
using Csla.Security;
using Csla.Core;
using Csla.Serialization;
using Csla.Silverlight;
using System.ComponentModel;
//using Csla.Validation;

#if !SILVERLIGHT
using System.Data.SqlClient;
using BusinessLibrary.Data;
using BusinessLibrary.Security;
#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class MagRelatedPapersRunList : DynamicBindingListBase<MagRelatedPapersRun>
    {
        public static void GetMagRelatedPapersRunList(EventHandler<DataPortalResult<MagRelatedPapersRunList>> handler)
        {
            DataPortal<MagRelatedPapersRunList> dp = new DataPortal<MagRelatedPapersRunList>();
            dp.FetchCompleted += handler;
            dp.BeginFetch();
        }

#if SILVERLIGHT
        public MagRelatedPapersRunList() { }
#else
        private MagRelatedPapersRunList() { }
#endif

#if SILVERLIGHT
       
#else

        protected void DataPortal_Fetch()
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            RaiseListChangedEvents = false;
            using (SqlConnection connection = new SqlConnection(DataConnection.AcademicControllerConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_MagRelatedPapersRuns", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        while (reader.Read())
                        {
                            Add(MagRelatedPapersRun.GetMagRelatedPapersRun(reader));
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
