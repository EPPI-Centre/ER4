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
    public class OutcomeItemAttributeList : DynamicBindingListBase<OutcomeItemAttribute>
    {
        public static void GetOutcomeItemAttributeList(int OutcomeId, EventHandler<DataPortalResult<OutcomeItemAttributeList>> handler)
        {
            DataPortal<OutcomeItemAttributeList> dp = new DataPortal<OutcomeItemAttributeList>();
            dp.FetchCompleted += handler;
            dp.BeginFetch(new SingleCriteria<AttributeSetList, int>(OutcomeId));
        }

        internal static OutcomeItemAttributeList NewOutcomeItemAttributeList()
        {
            return new OutcomeItemAttributeList();
        }


        public OutcomeItemAttributeList() { }

        [JsonProperty]
        public List<OutcomeItemAttribute> OutcomeItemAttributesList
        {
            get { return this.ToList<OutcomeItemAttribute>(); }
        }

#if !SILVERLIGHT
        protected void DataPortal_Fetch(SingleCriteria<AttributeSetList, int> criteria)
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            RaiseListChangedEvents = false;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_OutcomeItemAttributeList", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@OUTCOME_ID", criteria.Value));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        while (reader.Read())
                        {
                            Add(OutcomeItemAttribute.GetOutcomeItemAttribute(reader));
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
