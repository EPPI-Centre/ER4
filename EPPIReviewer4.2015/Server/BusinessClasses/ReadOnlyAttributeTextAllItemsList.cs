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
    public class ReadOnlyAttributeTextAllItemsList : ReadOnlyListBase<ReadOnlyAttributeTextAllItemsList, ReadOnlyAttributeTextAllItems>
    {
#if SILVERLIGHT
    public ReadOnlyAttributeTextAllItemsList() { }
#else
        private ReadOnlyAttributeTextAllItemsList() { }
#endif


#if !SILVERLIGHT

        private void DataPortal_Fetch(SingleCriteria<ReadOnlyAttributeTextAllItemsList, Int64> criteria)
        {
            RaiseListChangedEvents = false;
            IsReadOnly = false;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("[st_AttributeTextAllItems]", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_SET_ID", criteria.Value));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        while (reader.Read())
                        {
                            Add(ReadOnlyAttributeTextAllItems.GetReadOnlyAttributeTextAllItems(reader));
                        }
                    }
                }
                connection.Close();
            }
            IsReadOnly = true;
            RaiseListChangedEvents = true;
        }

#endif



    }
  
}
