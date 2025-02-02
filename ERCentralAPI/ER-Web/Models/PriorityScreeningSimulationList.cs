using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Csla;
using Csla.Security;
using Csla.Core;
using Csla.Serialization;
using Csla.Silverlight;
using Azure.Storage.Sas;

//using Csla.Validation;

#if !SILVERLIGHT
using System.Data.SqlClient;
using BusinessLibrary.Data;
using BusinessLibrary.Security;
using BusinessLibrary.BusinessClasses;
#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class PriorityScreeningSimulationList : DynamicBindingListBase<PriorityScreeningSimulation>
    //public class PriorityScreeningSimulationList : BusinessListBase<PriorityScreeningSimulationList, PriorityScreeningSimulation>
    {

        public static void GetPriorityScreeningSimulationList(string SimulationName, EventHandler<DataPortalResult<PriorityScreeningSimulationList>> handler)
        {
            DataPortal<PriorityScreeningSimulationList> dp = new DataPortal<PriorityScreeningSimulationList>();
            dp.FetchCompleted += handler;
            dp.BeginFetch(new SingleCriteria<PriorityScreeningSimulationList, string>(SimulationName));
        }


#if SILVERLIGHT
        public PriorityScreeningSimulationList() { }
#else
        public PriorityScreeningSimulationList() { }
#endif


#if SILVERLIGHT
       
#else
        protected void DataPortal_Fetch(SingleCriteria<PriorityScreeningSimulationList, string> criteria)
        {
            RaiseListChangedEvents = false;
            string blobConnection = AzureSettings.blobConnection;
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            List<BlobInHierarchy> myblobs = BlobOperations.Blobfilenames(blobConnection, "eppi-reviewer-data", "/test-pdf-parsed-pdfs");
            foreach (BlobInHierarchy b in myblobs)
            {
                Add(PriorityScreeningSimulation.GetPriorityScreeningSimulation(b.BlobName));
            }
            RaiseListChangedEvents = true;
        }

#endif



    }
}
