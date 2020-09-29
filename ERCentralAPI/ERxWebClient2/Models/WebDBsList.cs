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
    [Serializable]
    public class WebDBsList : DynamicBindingListBase<WebDB>
    {

        public WebDBsList() { }

        public WebDB GetWebDB(int webDBId)
        {
            WebDB returnValue;
            foreach (WebDB db in this)
            {
                if (db.WebDBId == webDBId)
                    return db;
            }
            return null;
        }

        


#if !SILVERLIGHT
    

        protected void DataPortal_Fetch()
        {
            RaiseListChangedEvents = false;
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_WebDbListGet", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@RevId", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@ContactId", ri.UserId));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        while (reader.Read())
                        {
                            WebDB DB = WebDB.GetWebDb(reader);
                            Add(DB);
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
