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
#endif

namespace BusinessLibrary.BusinessClasses
{
    public class AttributeTypesFactory
    {

#if!SILVERLIGHT

        public AttributeTypes FetchAttributeTypes()
        {
            AttributeTypes returnValue = new AttributeTypes();
            returnValue.RaiseListChangedEvents = false;
            returnValue.SetReadOnlyFlag(false);
            returnValue.Add(new AttributeTypes.NameValuePair(0, ""));

            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_AttributeTypes", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        while (reader.Read())
                        {
                            returnValue.Add(new AttributeTypes.NameValuePair(reader.GetInt32("ATTRIBUTE_TYPE_ID"), reader.GetString("ATTRIBUTE_TYPE")));
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
