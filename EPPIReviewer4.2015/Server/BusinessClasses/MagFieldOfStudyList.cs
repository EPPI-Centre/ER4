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
    public class MagFieldOfStudyList : DynamicBindingListBase<MagFieldOfStudy>
    {
        public static void GetMagFieldOfStudyList(string Ids, EventHandler<DataPortalResult<MagFieldOfStudyList>> handler)
        {
            DataPortal<MagFieldOfStudyList> dp = new DataPortal<MagFieldOfStudyList>();
            dp.FetchCompleted += handler;
            dp.BeginFetch(new SingleCriteria<MagFieldOfStudy, string>(Ids));
        }


#if SILVERLIGHT
        public MagFieldOfStudyList() { }
#else
        private MagFieldOfStudyList() { }
#endif

#if SILVERLIGHT
       
#else

        protected void DataPortal_Fetch(SingleCriteria<MagFieldOfStudy, string> criteria)
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            RaiseListChangedEvents = false;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_AggregateFoSPaperList", connection))
                {
                    command.Parameters.Add(new SqlParameter("@PaperIdList", criteria.Value));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        while (reader.Read())
                        {
                            Add(MagFieldOfStudy.GetMagFieldOfStudy(reader));
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
