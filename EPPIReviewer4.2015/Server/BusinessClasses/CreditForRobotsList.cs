using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Csla;
using Csla.Security;
using Csla.Core;
using Csla.Serialization;
using Csla.Silverlight;
using Csla.Data;

//using Csla.Validation;

#if !SILVERLIGHT
using System.Data.SqlClient;
using BusinessLibrary.Data;
using BusinessLibrary.Security;
#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class CreditForRobotsList: ReadOnlyBindingList<CreditForRobots>
    {
        public CreditForRobotsList() { }

        public bool HasCredit
        {
            get
            {
                foreach (CreditForRobots cfr in this)
                {
                    if (cfr.AmountRemaining > 0.01) return true;
                }
                return false;
            }
        }

#if !SILVERLIGHT

        private void DataPortal_Fetch()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                using (SqlCommand command = new SqlCommand("st_ReviewInfo", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        if (reader.Read())
                        {
                            reader.NextResult();
                            Child_Fetch(reader);
                        }
                    }
                }
            }
        }

        private void Child_Fetch(SafeDataReader reader)
        {
            RaiseListChangedEvents = false;
            IsReadOnly = false;
            while (reader.Read())
            {
                Add(DataPortal.FetchChild<CreditForRobots>(reader.GetInt32("CREDIT_PURCHASE_ID")));
            }
            IsReadOnly = true;
            RaiseListChangedEvents = true;
        }
#endif



    }
}
