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
using Microsoft.AspNetCore.Mvc.ModelBinding;
using static Csla.Security.MembershipIdentity;



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
            dp.BeginFetch();
        }


#if SILVERLIGHT
        public PriorityScreeningSimulationList() { }
#else
        public PriorityScreeningSimulationList() { }
#endif


#if SILVERLIGHT
       
#else
        protected void DataPortal_Fetch()
        {
            RaiseListChangedEvents = false;
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;

            string FolderAndFileName = DataFactoryHelper.NameBase + "ReviewId" + ri.ReviewId.ToString();
            string RemoteFolder = "priority_screening_simulation/" + FolderAndFileName + "/";

            string blobConnection = AzureSettings.blobConnection;
            List<BlobInHierarchy> myblobs = BlobOperations.Blobfilenames(blobConnection, "eppi-reviewer-data", RemoteFolder);
            foreach (BlobInHierarchy b in myblobs)
            {
                Add(PriorityScreeningSimulation.GetPriorityScreeningSimulation(b.BlobName.Replace(RemoteFolder, "")));
            }
            RaiseListChangedEvents = true;
        }

#endif



    }
}
