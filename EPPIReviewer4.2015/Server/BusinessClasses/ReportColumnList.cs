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
    //public class ReportColumnList : BusinessListBase<ReportColumnList, ReportColumn>
    public class ReportColumnList : DynamicBindingListBase<ReportColumn>
    {
        


#if SILVERLIGHT
        public ReportColumnList() { }

    protected override void AddNewCore()
    {
        //Add(ReportColumn.NewReportColumn());
    }

#else
        private ReportColumnList() { }
#endif

        internal static ReportColumnList NewReportColumnList()
        {
            return new ReportColumnList();
        }

        

#if SILVERLIGHT
    public static void GetReportColumnList(EventHandler<DataPortalResult<ReportColumnList>> handler, int reviewId)
    {
      DataPortal<ReportColumnList> dp = new DataPortal<ReportColumnList>();
      dp.FetchCompleted += handler;
      dp.BeginFetch(new SingleCriteria<ReportColumnList, int>(reviewId));
    }
#else
        protected void DataPortal_Fetch(SingleCriteria<ReportColumnList, int> criteria)
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as BusinessLibrary.Security.ReviewerIdentity;
            RaiseListChangedEvents = false;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ReportColumns", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REPORT_ID", ri.ReviewId));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        while (reader.Read())
                        {
                            ReportColumn newReportColumn = ReportColumn.GetReportColumn(reader);
                            Add(newReportColumn);
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
