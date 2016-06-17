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
    public class ReadOnlyItemAttributeChildFrequencyList : ReadOnlyListBase<ReadOnlyItemAttributeChildFrequencyList, ReadOnlyItemAttributeChildFrequency>
    {
#if SILVERLIGHT
    public ReadOnlyItemAttributeChildFrequencyList() { }
#else
        private ReadOnlyItemAttributeChildFrequencyList() { }
#endif

        public static void GetItemAttributeChildFrequencyList(int set_id, Int64 attribute_id, bool isIncluded, Int64 filterAttributeId, EventHandler<DataPortalResult<ReadOnlyItemAttributeChildFrequencyList>> handler)
        {
            DataPortal<ReadOnlyItemAttributeChildFrequencyList> dp = new DataPortal<ReadOnlyItemAttributeChildFrequencyList>();
            dp.FetchCompleted += handler;
            dp.BeginFetch(new ItemAttributeChildFrequencySelectionCriteria(typeof(ReadOnlyItemAttributeChildFrequencyList), attribute_id, set_id, isIncluded, filterAttributeId));
        }

#if !SILVERLIGHT

        private void DataPortal_Fetch(ItemAttributeChildFrequencySelectionCriteria criteria)
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            RaiseListChangedEvents = false;
            IsReadOnly = false;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ItemAttributeChildFrequencies", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ID", criteria.AttributeId));
                    command.Parameters.Add(new SqlParameter("@SET_ID", criteria.SetId));
                    command.Parameters.Add(new SqlParameter("@IS_INCLUDED", criteria.Included));
                    command.Parameters.Add(new SqlParameter("@FILTER_ATTRIBUTE_ID", criteria.FilterAttributeId));
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        while (reader.Read())
                        {
                            Add(ReadOnlyItemAttributeChildFrequency.GetReadOnlyItemAttributeChildFrequency(reader, criteria.SetId, criteria.FilterAttributeId, criteria.Included));
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

    [Serializable]
    public class ItemAttributeChildFrequencySelectionCriteria : Csla.CriteriaBase<ItemAttributeChildFrequencySelectionCriteria>
    {
        private static PropertyInfo<Int64> AttributeIdProperty = RegisterProperty<Int64>(typeof(ItemAttributeChildFrequencySelectionCriteria), new PropertyInfo<Int64>("AttributeId", "AttributeId"));
        public Int64 AttributeId
        {
            get { return ReadProperty(AttributeIdProperty); }
        }

        private static PropertyInfo<int> SetIdProperty = RegisterProperty<int>(typeof(ItemAttributeChildFrequencySelectionCriteria), new PropertyInfo<int>("SetId", "SetId"));
        public int SetId
        {
            get { return ReadProperty(SetIdProperty); }
        }

        private static PropertyInfo<bool> IncludedProperty = RegisterProperty<bool>(typeof(ItemAttributeChildFrequencySelectionCriteria), new PropertyInfo<bool>("Included", "Included"));
        public bool Included
        {
            get { return ReadProperty(IncludedProperty); }
        }

        private static PropertyInfo<Int64> FilterAttributeIdProperty = RegisterProperty<Int64>(typeof(ItemAttributeChildFrequencySelectionCriteria), new PropertyInfo<Int64>("FilterAttributeId", "FilterAttributeId"));
        public Int64 FilterAttributeId
        {
            get { return ReadProperty(FilterAttributeIdProperty); }
        }

        public ItemAttributeChildFrequencySelectionCriteria(Type type, Int64 attributeId, int setId, bool isIncluded, Int64 filterAttributeId)
            //: base(type)
        {
            LoadProperty(AttributeIdProperty, attributeId);
            LoadProperty(SetIdProperty, setId);
            LoadProperty(IncludedProperty, isIncluded);
            LoadProperty(FilterAttributeIdProperty, filterAttributeId);
        }

        public ItemAttributeChildFrequencySelectionCriteria() { }
    }
    
}