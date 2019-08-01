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
#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class ReadOnlyReviewSetOutcomeList : ReadOnlyListBase<ReadOnlyReviewSetOutcomeList, ReadOnlyReviewSetOutcome>
    {
#if SILVERLIGHT
    public ReadOnlyReviewSetOutcomeList() { }
#else
        public ReadOnlyReviewSetOutcomeList() { }
#endif

        public static void GetReadOnlyReviewSetOutcomeList(Int64 ItemSetId, Int64 SetId, EventHandler<DataPortalResult<ReadOnlyReviewSetOutcomeList>> handler)
        {
            DataPortal<ReadOnlyReviewSetOutcomeList> dp = new DataPortal<ReadOnlyReviewSetOutcomeList>();
            dp.FetchCompleted += handler;
            dp.BeginFetch(new ReadOnlyReviewSetOutcomeListSelectionCriteria(typeof(ReadOnlyReviewSetControlList),
                ItemSetId, SetId));
        }

#if !SILVERLIGHT

        private void DataPortal_Fetch(ReadOnlyReviewSetOutcomeListSelectionCriteria criteria)
        {
            RaiseListChangedEvents = false;
            IsReadOnly = false;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ReviewSetOutcomes", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@ITEM_SET_ID", criteria.ItemSetId));
                    command.Parameters.Add(new SqlParameter("@SET_ID", criteria.SetId));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        while (reader.Read())
                        {
                            Add(ReadOnlyReviewSetOutcome.GetReadOnlyReviewSetOutcome(reader));
                        }
                    }
                }
                connection.Close();
            }
            IsReadOnly = true;
            RaiseListChangedEvents = true;
        }

#endif

        // used to define the parameters for the query.
        [Serializable]
        public class ReadOnlyReviewSetOutcomeListSelectionCriteria : Csla.CriteriaBase<ReadOnlyReviewSetOutcomeListSelectionCriteria>
        {
            private static PropertyInfo<Int64> ItemSetIdProperty = RegisterProperty<Int64>(typeof(ReadOnlyReviewSetOutcomeListSelectionCriteria), new PropertyInfo<Int64>("ItemSetId", "ItemSetId"));
            public Int64 ItemSetId
            {
                get { return ReadProperty(ItemSetIdProperty); }
            }

            private static PropertyInfo<Int64> SetIdProperty = RegisterProperty<Int64>(typeof(ReadOnlyReviewSetOutcomeListSelectionCriteria), new PropertyInfo<Int64>("SetId", "SetId"));
            public Int64 SetId
            {
                get { return ReadProperty(SetIdProperty); }
            }

            public ReadOnlyReviewSetOutcomeListSelectionCriteria(Type type, Int64 itemSetId, Int64 setId)
                //: base(type)
            {
                LoadProperty(ItemSetIdProperty, itemSetId);
                LoadProperty(SetIdProperty, setId);
            }

            public ReadOnlyReviewSetOutcomeListSelectionCriteria() { }
        }
    }
}
