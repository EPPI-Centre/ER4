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
    public class ReadOnlyItemAttributeCrosstabList : ReadOnlyListBase<ReadOnlyItemAttributeCrosstabList, ReadOnlyItemAttributeCrosstab>
    {
#if SILVERLIGHT
    public ReadOnlyItemAttributeCrosstabList() { }
#else
        private ReadOnlyItemAttributeCrosstabList() { }
#endif

        public static void GetItemAttributeChildCrosstabList(Int64 attributeIdXAxis, int setIdXAxis, Int64 attributeIdYAxis, int setIdYAxis,
            Int64 attributeIdFilter, int setIdFilter, int nxaxis, EventHandler<DataPortalResult<ReadOnlyItemAttributeCrosstabList>> handler)
        {
            DataPortal<ReadOnlyItemAttributeCrosstabList> dp = new DataPortal<ReadOnlyItemAttributeCrosstabList>();
            dp.FetchCompleted += handler;
            dp.BeginFetch(new ItemAttributeCrosstabSelectionCriteria(typeof(ReadOnlyItemAttributeCrosstabList), attributeIdXAxis,
                setIdXAxis, attributeIdYAxis, setIdYAxis, attributeIdFilter, setIdFilter, nxaxis));
        }

#if !SILVERLIGHT

        private void DataPortal_Fetch(ItemAttributeCrosstabSelectionCriteria criteria)
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            RaiseListChangedEvents = false;
            IsReadOnly = false;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("[st_ItemAttributeCrosstabs]", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@PARENT_ATTRIBUTE_ID1", criteria.AttributeIdXAxis));
                    command.Parameters.Add(new SqlParameter("@PARENT_SET_ID1", criteria.SetIdXAxis));
                    command.Parameters.Add(new SqlParameter("@PARENT_SET_ID2", criteria.SetIdYAxis));
                    command.Parameters.Add(new SqlParameter("@PARENT_ATTRIBUTE_ID2", criteria.AttributeIdYAxis));
                    command.Parameters.Add(new SqlParameter("@FILTER_ATTRIBUTE_ID", criteria.AttributeIdFilter));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        while (reader.Read())
                        {
                            Add(ReadOnlyItemAttributeCrosstab.GetReadOnlyItemAttributeCrosstab(reader, criteria.NXAxis));
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
    public class ItemAttributeCrosstabSelectionCriteria : Csla.CriteriaBase<ItemAttributeCrosstabSelectionCriteria>
    {
        private static PropertyInfo<Int64> AttributeIdXAxisProperty = RegisterProperty<Int64>(typeof(ItemAttributeCrosstabSelectionCriteria), new PropertyInfo<Int64>("AttributeIdXAxis", "AttributeIdXAxis"));
        public Int64 AttributeIdXAxis
        {
            get { return ReadProperty(AttributeIdXAxisProperty); }
        }

        private static PropertyInfo<int> SetIdXAxisProperty = RegisterProperty<int>(typeof(ItemAttributeCrosstabSelectionCriteria), new PropertyInfo<int>("SetIdXAxis", "SetIdXAxis"));
        public int SetIdXAxis
        {
            get { return ReadProperty(SetIdXAxisProperty); }
        }

        private static PropertyInfo<Int64> AttributeIdYAxisProperty = RegisterProperty<Int64>(typeof(ItemAttributeCrosstabSelectionCriteria), new PropertyInfo<Int64>("AttributeIdYAxis", "AttributeIdYAxis"));
        public Int64 AttributeIdYAxis
        {
            get { return ReadProperty(AttributeIdYAxisProperty); }
        }

        private static PropertyInfo<int> SetIdYAxisProperty = RegisterProperty<int>(typeof(ItemAttributeCrosstabSelectionCriteria), new PropertyInfo<int>("SetIdYAxis", "SetIdYAxis"));
        public int SetIdYAxis
        {
            get { return ReadProperty(SetIdYAxisProperty); }
        }

        private static PropertyInfo<Int64> AttributeIdFilterProperty = RegisterProperty<Int64>(typeof(ItemAttributeCrosstabSelectionCriteria), new PropertyInfo<Int64>("AttributeIdFilter", "AttributeIdFilter"));
        public Int64 AttributeIdFilter
        {
            get { return ReadProperty(AttributeIdFilterProperty); }
        }

        private static PropertyInfo<int> SetIdFilterProperty = RegisterProperty<int>(typeof(ItemAttributeCrosstabSelectionCriteria), new PropertyInfo<int>("SetIdFilter", "SetIdFilter"));
        public int SetIdFilter
        {
            get { return ReadProperty(SetIdFilterProperty); }
        }

        private static PropertyInfo<int> NXAxisProperty = RegisterProperty<int>(typeof(ItemAttributeCrosstabSelectionCriteria), new PropertyInfo<int>("NXAxis", "NXAxis"));
        public int NXAxis
        {
            get { return ReadProperty(NXAxisProperty); }
        }

        public ItemAttributeCrosstabSelectionCriteria(Type type, Int64 attributeIdXAxis, int setIdXAxis, Int64 attributeIdYAxis, int setIdYAxis,
            Int64 attributeIdFilter, int setIdFilter, int nxAxis)
            //: base(type)
        {
            LoadProperty(AttributeIdXAxisProperty, attributeIdXAxis);
            LoadProperty(SetIdXAxisProperty, setIdXAxis);
            LoadProperty(AttributeIdYAxisProperty, attributeIdYAxis);
            LoadProperty(SetIdYAxisProperty, setIdYAxis);
            LoadProperty(AttributeIdFilterProperty, attributeIdFilter);
            LoadProperty(SetIdFilterProperty, setIdFilter);
            LoadProperty(NXAxisProperty, nxAxis);
        }

        public ItemAttributeCrosstabSelectionCriteria() { }
    }

}
