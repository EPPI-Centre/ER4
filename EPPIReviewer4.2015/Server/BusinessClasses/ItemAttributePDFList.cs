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
    public class ItemAttributePDFList : DynamicBindingListBase<ItemAttributePDF>
    {
        public ItemAttributePDFList() { }
#if SILVERLIGHT
    
    public ItemAttributePDF FindPerPage(int page)
    {

        foreach (ItemAttributePDF pp in this)
        {
            if (pp.Page == page) return pp;
        }
        return null;
    }
    public bool containsPage(int page)
    {
        foreach (ItemAttributePDF pp in this)
        {
            if (pp.Page == page) return true;
        }
        return false;
    }

#endif
    public static void GetItemAttributePDFList(Int64 ItemDocumentId, Int64 ItemAttributeId, EventHandler<DataPortalResult<ItemAttributePDFList>> handler)
    {
        DataPortal<ItemAttributePDFList> dp = new DataPortal<ItemAttributePDFList>();
        dp.FetchCompleted += handler;
        dp.BeginFetch(new iaPDFListSelCrit(ItemDocumentId, ItemAttributeId));
    }
#if !SILVERLIGHT


        private void DataPortal_Fetch(iaPDFListSelCrit criteria)
        {
            RaiseListChangedEvents = false;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ItemAttributePDF", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@ITEM_ATTRIBUTE_ID", criteria.ItemAttributeId));
                    command.Parameters.Add(new SqlParameter("@ITEM_DOCUMENT_ID", criteria.ItemDocumentId));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        while (reader.Read())
                        {
                            Add(ItemAttributePDF.GetItemAttributePDF(reader));
                        }
                    }
                }
                connection.Close();
            }
            RaiseListChangedEvents = true;
        }

#endif



    }
    [Serializable]
    public class iaPDFListSelCrit : BusinessBase //Csla.CriteriaBase
    {
        public static readonly PropertyInfo<Int64> ItemDocumentIdProperty = RegisterProperty<Int64>(typeof(iaPDFListSelCrit), new PropertyInfo<Int64>("ItemDocumentId", "ItemDocumentId"));
        public Int64 ItemDocumentId
        {
            get { return ReadProperty(ItemDocumentIdProperty); }
            set
            {
                SetProperty(ItemDocumentIdProperty, value);
            }
        }

        public static readonly PropertyInfo<Int64> ItemAttributeIdProperty = RegisterProperty<Int64>(typeof(iaPDFListSelCrit), new PropertyInfo<Int64>("ItemAttributeId", "ItemAttributeId"));
        public Int64 ItemAttributeId
        {
            get { return ReadProperty(ItemAttributeIdProperty); }
            set
            {
                SetProperty(ItemAttributeIdProperty, value);
            }
        }
        public iaPDFListSelCrit()
        { }
        public iaPDFListSelCrit(Int64 itemDocumentId, Int64 itemAttributeId) 
        {
            this.ItemAttributeId = itemAttributeId;
            this.ItemDocumentId = itemDocumentId;
        }

    }

}