using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Csla;
using Csla.Security;
using Csla.Core;
using Csla.Serialization;
using Csla.Silverlight;
////using Csla.Validation;

#if!SILVERLIGHT
using Csla.Data;
using System.Data.SqlClient;
using BusinessLibrary.Data;
using BusinessLibrary.Security;
#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class WebDbReadOnlyItemAttributeList : ReadOnlyListBase<WebDbReadOnlyItemAttributeList, ReadOnlyItemAttribute>
    {

    public WebDbReadOnlyItemAttributeList() { }

        
        
        internal ReadOnlyItemAttribute getReadOnlyItemAttribute(Int64 ItemAttributeID)
        {
            foreach (ReadOnlyItemAttribute IA in this)
            {
                if (IA.ItemAttributeId == ItemAttributeID) return IA;
            }
            return null;
        }

        public void AddToReadOnlyItemAttributeList(ReadOnlyItemAttribute roia)
        {
            this.IsReadOnly = false;
            this.Add(roia);
            this.IsReadOnly = true;
        }

        public void RemoveFromReadOnlyItemAttributeList(ReadOnlyItemAttribute item)
        {
            this.IsReadOnly = false;
            this.Remove(item);
            this.IsReadOnly = true;
        }

#if !SILVERLIGHT

        

        public static WebDbReadOnlyItemAttributeList GetReadOnlyItemAttributeList(Int64 ItemSetId, int WebDbId)
        {
            WebDbReadOnlyItemAttributeList returnValue = new WebDbReadOnlyItemAttributeList();
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            returnValue.RaiseListChangedEvents = false;
            returnValue.IsReadOnly = false;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_WebDbItemAttributes", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@ITEM_SET_ID", ItemSetId));
                    command.Parameters.Add(new SqlParameter("@RevId", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@WebDbId", WebDbId));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        while (reader.Read())
                        {
                            returnValue.Add(ReadOnlyItemAttribute.GetReadOnlyItemAttribute(reader));
                        }
                    }
                }
                connection.Close();
            }
            returnValue.IsReadOnly = true;
            returnValue.RaiseListChangedEvents = true;
            return returnValue;
        }

#endif

        
        

    }
    
    //[Serializable]
    //public class ItemAttributesSelectionCriteria : Csla.CriteriaBase<ItemAttributesSelectionCriteria>
    //{
    //    private static PropertyInfo<Int64> ItemIdProperty = RegisterProperty<Int64>(typeof(ItemAttributesSelectionCriteria), new PropertyInfo<Int64>("ItemId", "ItemId"));
    //    public Int64 ItemId
    //    {
    //        get { return ReadProperty(ItemIdProperty); }
    //    }

    //    private static PropertyInfo<int> ContactIdProperty = RegisterProperty<int>(typeof(ItemAttributesSelectionCriteria), new PropertyInfo<int>("ContactId", "ContactId"));
    //    public int ContactId
    //    {
    //        get { return ReadProperty(ContactIdProperty); }
    //    }

    //    public ItemAttributesSelectionCriteria(Type type, Int64 itemId, int contactId)
    //        //: base(type)
    //    {
    //        LoadProperty(ItemIdProperty, itemId);
    //        LoadProperty(ContactIdProperty, contactId);
    //    }

    //    public ItemAttributesSelectionCriteria() { }
    //}
}
