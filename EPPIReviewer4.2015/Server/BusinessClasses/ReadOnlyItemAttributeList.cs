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
    public class ReadOnlyItemAttributeList : ReadOnlyListBase<ReadOnlyItemAttributeList, ReadOnlyItemAttribute>
    {
#if SILVERLIGHT
    public ReadOnlyItemAttributeList() { }
#else
        private ReadOnlyItemAttributeList() { }
#endif

        
        //public static void GetItemAttributeList(int item_id, EventHandler<DataPortalResult<ReadOnlyItemAttributeList>> handler)
        //{
        //    DataPortal<ReadOnlyItemAttributeList> dp = new DataPortal<ReadOnlyItemAttributeList>();
        //    dp.FetchCompleted += handler;
        //    dp.BeginFetch(new SingleCriteria<ReadOnlyItemAttributeList, Int64>(item_id));
        //}
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

        private void DataPortal_Fetch(ItemAttributesSelectionCriteria criteria)
        {
            int i = 0;
            int makeError = 1 / i;
            /*
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            RaiseListChangedEvents = false;
            IsReadOnly = false;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("[st_ItemAttributes]", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@CONTACT_ID", criteria.ContactId));
                    command.Parameters.Add(new SqlParameter("@ITEM_ID", criteria.ItemId));
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        while (reader.Read())
                        {
                            Add(ReadOnlyItemAttribute.GetReadOnlyItemAttribute(reader));
                        }
                    }
                }
                connection.Close();
            }
            IsReadOnly = true;
            RaiseListChangedEvents = true;
            */
        }

        public static ReadOnlyItemAttributeList GetReadOnlyItemAttributeList(Int64 ItemSetId)
        {
            ReadOnlyItemAttributeList returnValue = new ReadOnlyItemAttributeList();
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            returnValue.RaiseListChangedEvents = false;
            returnValue.IsReadOnly = false;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ItemAttributes", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@ITEM_SET_ID", ItemSetId));
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
    [Serializable]
    public class ItemAttributesSelectionCriteria : Csla.CriteriaBase<ItemAttributesSelectionCriteria>
    {
        private static PropertyInfo<Int64> ItemIdProperty = RegisterProperty<Int64>(typeof(ItemAttributesSelectionCriteria), new PropertyInfo<Int64>("ItemId", "ItemId"));
        public Int64 ItemId
        {
            get { return ReadProperty(ItemIdProperty); }
        }

        private static PropertyInfo<int> ContactIdProperty = RegisterProperty<int>(typeof(ItemAttributesSelectionCriteria), new PropertyInfo<int>("ContactId", "ContactId"));
        public int ContactId
        {
            get { return ReadProperty(ContactIdProperty); }
        }

        public ItemAttributesSelectionCriteria(Type type, Int64 itemId, int contactId)
            //: base(type)
        {
            LoadProperty(ItemIdProperty, itemId);
            LoadProperty(ContactIdProperty, contactId);
        }

        public ItemAttributesSelectionCriteria() { }
    }
}
