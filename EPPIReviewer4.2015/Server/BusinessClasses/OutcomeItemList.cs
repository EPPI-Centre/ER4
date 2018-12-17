using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Csla;
using Csla.Security;
using Csla.Core;
using Csla.Serialization;
using Csla.Silverlight;
using Newtonsoft.Json;
//using Csla.Validation;

#if !SILVERLIGHT
using System.Data.SqlClient;
using BusinessLibrary.Data;
using BusinessLibrary.Security;
#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public class OutcomeItemList : DynamicBindingListBase<Outcome>
    {
        public static void GetOutcomeItemList(Int64 itemSetId, EventHandler<DataPortalResult<OutcomeItemList>> handler)
        {
            DataPortal<OutcomeItemList> dp = new DataPortal<OutcomeItemList>();
            dp.FetchCompleted += handler;
            dp.BeginFetch(new SingleCriteria<OutcomeItemList, Int64>(itemSetId));
        }

        public string OutcomesTable()
        {
            string retVal = "";
            int i = -1;

            IEnumerable<Outcome> sortedOutcomes = this.OrderBy(outcome => outcome.OutcomeTypeId);
            foreach (Outcome o in sortedOutcomes)
            {
                if (i != o.OutcomeTypeId)
                {
                    if (retVal == "")
                    {
                        retVal = "<p><b>Outcomes</b></p><table border='1'>";
                    }
                    else
                    {
                        retVal += "</table><table border='1'>";
                    }
                    i = o.OutcomeTypeId;
                    retVal += o.GetOutcomeHeaders();
                }
                retVal += o.GetOutcomeStats();
            }
            return retVal + "</table>";
        }

        public bool HasSavedHandler = false;

        [JsonProperty]
        public List<Outcome> OutcomesList
        {
            get { return this.ToList<Outcome>(); }
        }

        public OutcomeItemList() { }
#if SILVERLIGHT
    protected override void AddNewCore()
    {
        Add(Outcome.NewOutcome());
    }

#endif
        



#if SILVERLIGHT
       
#else
        protected void DataPortal_Fetch(SingleCriteria<OutcomeItemList, Int64> criteria)
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            RaiseListChangedEvents = false;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_OutcomeItemList", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@ITEM_SET_ID", criteria.Value));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        while (reader.Read())
                        {
                            Add(Outcome.GetOutcome(reader));
                        }
                    }
                }
                connection.Close();
            }
            RaiseListChangedEvents = true;
        }

        public static OutcomeItemList GetOutcomeItemList(Int64 itemSetId)
        {
            OutcomeItemList oil = new OutcomeItemList();
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            oil.RaiseListChangedEvents = false;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_OutcomeItemList", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@ITEM_SET_ID", itemSetId));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        while (reader.Read())
                        {
                            oil.Add(Outcome.GetOutcome(reader));
                        }
                    }
                }
                connection.Close();
            }
            oil.RaiseListChangedEvents = true;
            return oil;
        }
#endif

        /*
        // used to define the parameters for the query.
        [Serializable]
        public class OutcomeItemListSelectionCriteria : Csla.CriteriaBase
        {
            private static PropertyInfo<Int64> ItemIdProperty = RegisterProperty<Int64>(typeof(OutcomeItemListSelectionCriteria), new PropertyInfo<Int64>("ItemId", "ItemId"));
            public Int64 ItemId
            {
                get { return ReadProperty(ItemIdProperty); }
            }

            private static PropertyInfo<int> ItemSetIdProperty = RegisterProperty<int>(typeof(OutcomeItemListSelectionCriteria), new PropertyInfo<int>("ItemSetId", "ItemSetId"));
            public int ItemSetId
            {
                get { return ReadProperty(ItemSetIdProperty); }
            }

            public OutcomeItemListSelectionCriteria(Type type, Int64 itemId, int reviewSetId ): base(type)
            {
                LoadProperty(ItemIdProperty, itemId);
                LoadProperty(ItemSetIdProperty, reviewSetId);
            }

            public OutcomeItemListSelectionCriteria() { }
        }
        */


    }
}
