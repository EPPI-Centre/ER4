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
    //public class ReportColumnCodeList : BusinessListBase<ReportColumnCodeList, ReportColumnCode>
    public class ReportColumnCodeList : DynamicBindingListBase<ReportColumnCode>
    {



#if SILVERLIGHT
        public ReportColumnCodeList() { }

    protected override void AddNewCore()
    {
        //Add(ReportColumnCode.NewReportColumnCode());
    }

#else
        private ReportColumnCodeList() { }
#endif

        internal static ReportColumnCodeList NewReportColumnCodeList()
        {
            return new ReportColumnCodeList();
        }


#if SILVERLIGHT
    public static void GetReportColumnCodeList(EventHandler<DataPortalResult<ReportColumnCodeList>> handler, int reviewId)
    {
      DataPortal<ReportColumnCodeList> dp = new DataPortal<ReportColumnCodeList>();
      dp.FetchCompleted += handler;
      dp.BeginFetch(new SingleCriteria<ReportColumnCodeList, int>(reviewId));
    }
#else
        protected void DataPortal_Fetch(SingleCriteria<ReportColumnCodeList, int> criteria)
        {
            
        }


#endif

    }
}
