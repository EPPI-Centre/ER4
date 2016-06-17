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
    public class ItemAttributeTextList : DynamicBindingListBase<ItemAttributeText>
    {
#if SILVERLIGHT
    public ItemAttributeTextList() { }
#else
        private ItemAttributeTextList() { }
#endif
        internal static ItemAttributeTextList NewItemAttributeTextList()
        {
            return new ItemAttributeTextList();
        }
#if !SILVERLIGHT

        public static ItemAttributeTextList GetReadOnlyItemAttributeTextList(Int64 ItemAttributeId)
        {
            ItemAttributeTextList returnValue = new ItemAttributeTextList();
            returnValue.RaiseListChangedEvents = false;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ItemAttributeText", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@ITEM_ATTRIBUTE_ID", ItemAttributeId));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        while (reader.Read())
                        {
                            returnValue.Add(ItemAttributeText.GetItemAttributeText(reader));
                        }
                    }
                }
                connection.Close();
            }
            returnValue.RaiseListChangedEvents = true;
            return returnValue;
        }

        private void DataPortal_Fetch(SingleCriteria<ItemAttributeTextList, Int64> criteria)
        {
            RaiseListChangedEvents = false;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ItemAttributeText", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@ITEM_ATTRIBUTE_ID", criteria.Value));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        while (reader.Read())
                        {
                            Add(ItemAttributeText.GetItemAttributeText(reader));
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
