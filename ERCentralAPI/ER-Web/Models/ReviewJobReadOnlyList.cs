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
    public class ReviewJobReadOnlyList : ReadOnlyListBase<ReviewJobReadOnlyList, ReviewJobReadOnly>
    {
        public ReviewJobReadOnlyList() 
        {
        }
        

#if !SILVERLIGHT

        private void DataPortal_Fetch()
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            RaiseListChangedEvents = false;
            IsReadOnly = false;
            int rid = ri.ReviewId;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ReviewJobList", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", rid));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        Child_Fetch(reader, ri.IsSiteAdmin);
                    }
                }
            }
            IsReadOnly = true;
            RaiseListChangedEvents = true;
        }
       
        private void Child_Fetch(SafeDataReader reader, bool IsSiteAdmin)
        {
            RaiseListChangedEvents = false;
            IsReadOnly = false;
            while (reader.Read())
            {
                Add(DataPortal.FetchChild<ReviewJobReadOnly>(reader, IsSiteAdmin));
            }
            IsReadOnly = true;
            RaiseListChangedEvents = true;
        }
#endif
    }
}
