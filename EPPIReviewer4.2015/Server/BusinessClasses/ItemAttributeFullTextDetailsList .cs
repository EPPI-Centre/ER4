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
    public class ItemAttributeFullTextDetailsList : DynamicBindingListBase<ItemAttributeFullTextDetails>
    {
#if SILVERLIGHT
        public ItemAttributeFullTextDetailsList () { }
        
#else
        private ItemAttributeFullTextDetailsList() { }
        
#endif
        private List<ItemAttributeFullTextDetails> _SimpleTextList;
        public List<ItemAttributeFullTextDetails> SimpleTextList
        {//used to get the simpletext list to be used when adding other codes to simple text.
            get
            {
                if (this.Count > 0)
                {
                    _SimpleTextList = new List<ItemAttributeFullTextDetails>();
                    foreach (ItemAttributeFullTextDetails el in this)
                    {
                        if (!el.IsFromPDF)
                        {
                            _SimpleTextList.Add(el);
                        }
                    }
                    return _SimpleTextList;
                }
                return null;
            }
        }
        public static ItemAttributeFullTextDetailsList NewItemAttributeFullTextDetailsList()
        {
            return new ItemAttributeFullTextDetailsList();
        }
        public ItemAttributeFullTextDetails GetItemAttributeFullTextDetails(bool isPDF, Int64 itemAttributeTextId)
        {
            foreach (ItemAttributeFullTextDetails ftd in this)
            {
                if (ftd.IsFromPDF == isPDF && ftd.ItemAttributeTextId == itemAttributeTextId)
                {
                    return ftd;
                }
            }
            return null;
        }
#if !SILVERLIGHT

        public static ItemAttributeFullTextDetailsList GetReadOnlyItemAttributeTextList(Int64 ItemAttributeId)
        {
            ItemAttributeFullTextDetailsList returnValue = new ItemAttributeFullTextDetailsList();
            returnValue.RaiseListChangedEvents = false;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                //using (SqlCommand command = new SqlCommand("st_ItemAttributeText", connection))
                //{
                //    command.CommandType = System.Data.CommandType.StoredProcedure;
                //    command.Parameters.Add(new SqlParameter("@ITEM_ATTRIBUTE_ID", ItemAttributeId));
                //    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                //    {
                //        while (reader.Read())
                //        {
                //            returnValue.Add(ItemAttributeText.GetItemAttributeText(reader));
                //        }
                //    }
                //}
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
                //using (SqlCommand command = new SqlCommand("st_ItemAttributeText", connection))
                //{
                //    command.CommandType = System.Data.CommandType.StoredProcedure;
                //    command.Parameters.Add(new SqlParameter("@ITEM_ATTRIBUTE_ID", criteria.Value));
                //    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                //    {
                //        while (reader.Read())
                //        {
                //            Add(ItemAttributeText.GetItemAttributeText(reader));
                //        }
                //    }
                //}
                connection.Close();
            }
            RaiseListChangedEvents = true;
        }

#endif



    }
    
}
