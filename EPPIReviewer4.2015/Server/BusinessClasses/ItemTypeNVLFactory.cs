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
    public class ItemTypeNVLFactory
    {

#if!SILVERLIGHT

        public ItemTypeNVL FetchItemTypeNVL()
        {
            ItemTypeNVL returnValue = new ItemTypeNVL();
            returnValue.RaiseListChangedEvents = false;
            returnValue.SetReadOnlyFlag(false);
            returnValue.Add(new ItemTypeNVL.NameValuePair(0, ""));

            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ItemTypeList", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        while (reader.Read())
                        {
                            returnValue.Add(new ItemTypeNVL.NameValuePair(reader.GetInt32("TYPE_ID"), reader.GetString("TYPE_NAME")));
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
