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
using System.Data.SqlClient;
using BusinessLibrary.Data;
using BusinessLibrary.Security;
#endif

namespace BusinessLibrary.BusinessClasses
{
    public class ReviewContactNVLFactory
    {

#if!SILVERLIGHT

        public ReviewContactNVL FetchReviewContactNVL()
        {
            ReviewContactNVL returnValue = new ReviewContactNVL();
            returnValue.RaiseListChangedEvents = false;
            returnValue.SetReadOnlyFlag(false);
            returnValue.Add(new ReviewContactNVL.NameValuePair(0, ""));
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;

            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ReviewContactList", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        while (reader.Read())
                        {
                            returnValue.Add(new ReviewContactNVL.NameValuePair(reader.GetInt32("CONTACT_ID"), reader.GetString("CONTACT_NAME")));
                        }
                    }
                }
                connection.Close();
            }

            returnValue.SetReadOnlyFlag(true);
            returnValue.RaiseListChangedEvents = true;
            return returnValue;
        }
#endif
    }
}
