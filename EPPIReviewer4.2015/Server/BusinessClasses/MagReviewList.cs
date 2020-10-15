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
    public class MagReviewList : DynamicBindingListBase<MagReview>
    {
        public static void GetMagReviewList(EventHandler<DataPortalResult<MagReviewList>> handler)
        {
            DataPortal<MagReviewList> dp = new DataPortal<MagReviewList>();
            dp.FetchCompleted += handler;
            dp.BeginFetch();
        }


#if SILVERLIGHT
        public MagReviewList() { }
#else
        public MagReviewList() { }
#endif

#if SILVERLIGHT
       
#else

        protected void DataPortal_Fetch()
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            RaiseListChangedEvents = false;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ReviewMagEnabled", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        while (reader.Read())
                        {
                            Add(MagReview.GetMagReview(reader));
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
