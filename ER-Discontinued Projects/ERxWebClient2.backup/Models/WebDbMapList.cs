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
    public class WebDbMapList : DynamicBindingListBase<WebDbMap>
    {

        public WebDbMapList() { }

        public WebDbMap GetWebDB(int webDBMapId)
        {
            foreach (WebDbMap map in this)
            {
                if (map.WebDBMapId == webDBMapId)
                    return map;
            }
            return null;
        }

        


#if !SILVERLIGHT
    

        protected void DataPortal_Fetch(WebDBMapCriteria crit)
        {
            RaiseListChangedEvents = false;
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_WebDbMapsList", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@WEBDB_ID", crit.WebDbId));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        while (reader.Read())
                        {
                            WebDbMap map = WebDbMap.GetWebDbMap(reader);
                            Add(map);
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
