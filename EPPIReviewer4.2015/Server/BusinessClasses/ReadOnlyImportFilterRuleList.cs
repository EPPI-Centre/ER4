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
    public class ReadOnlyImportFilterRuleList : ReadOnlyListBase<ReadOnlyImportFilterRuleList, ReadOnlyImportFilterRule>
    {
        public ReadOnlyImportFilterRuleList() { }

        internal static ReadOnlyImportFilterRuleList NewReadOnlyImportFilterRuleList()
        {
            return new ReadOnlyImportFilterRuleList();
        }

#if SILVERLIGHT
    public static void GetReadOnlyImportFilterRuleList(EventHandler<DataPortalResult<ReadOnlyImportFilterRuleList>> handler)
    {
      DataPortal<ReadOnlyImportFilterRuleList> dp = new DataPortal<ReadOnlyImportFilterRuleList>();
      dp.FetchCompleted += handler;
      dp.BeginFetch();
    }
#else
        protected void DataPortal_Fetch()
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            int review_ID = ri.ReviewId;
            if (review_ID == 0) return;
            RaiseListChangedEvents = false;
            IsReadOnly = false;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("select * from TB_IMPORT_FILTER", connection))
                {
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        while (reader.Read())
                        {
                            Add(ReadOnlyImportFilterRule.GetReadOnlyImportFilterRule(reader));
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
}
