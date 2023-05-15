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
using System.Data.SqlClient;
using BusinessLibrary.Data;
using BusinessLibrary.Security;
#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class MetaAnalysisFilterSettingList : DynamicBindingListBase<MetaAnalysisFilterSetting>
    {

        public MetaAnalysisFilterSettingList() { }
        
        

#if !SILVERLIGHT

        protected void DataPortal_Fetch(SingleCriteria<MetaAnalysisFilterSettingList, int> criteria)
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            RaiseListChangedEvents = false;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_MetaAnalysisFilterSettings", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@META_ANALYSIS_ID", criteria.Value));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        while (reader.Read())
                        {
                            Add(MetaAnalysisFilterSetting.GetMetaAnalysisFilterSetting(reader));
                        }
                    }
                }
                connection.Close();
            }
            RaiseListChangedEvents = true;
        }
        public static MetaAnalysisFilterSettingList GetMetaAnalysisFilterSettingList(int MetaAnalysisId)
        {
            MetaAnalysisFilterSettingList res = new MetaAnalysisFilterSettingList();
            res.DataPortal_Fetch(new SingleCriteria<MetaAnalysisFilterSettingList, int>(MetaAnalysisId));
            return res;
        }
#endif
    }
}
