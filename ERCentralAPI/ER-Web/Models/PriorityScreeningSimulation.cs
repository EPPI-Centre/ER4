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
using Csla.DataPortalClient;
using System.IO;
using static Csla.Security.MembershipIdentity;

#if!SILVERLIGHT
using System.Data.SqlClient;
using BusinessLibrary.Data;
using Csla.Data;
using BusinessLibrary.Security;
using Azure.Storage.Blobs;
using BusinessLibrary.BusinessClasses;
#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class PriorityScreeningSimulation : BusinessBase<PriorityScreeningSimulation>
    {
#if SILVERLIGHT
    public PriorityScreeningSimulation() { }

        
#else
        public PriorityScreeningSimulation() { }
#endif


        public static readonly PropertyInfo<string> SimulationNameProperty = RegisterProperty<string>(new PropertyInfo<string>("simulationName", "simulationName"));
        public string simulationName
        {
            get
            {
                return GetProperty(SimulationNameProperty);
            }
        }
        public static readonly PropertyInfo<string> BlobProperty = RegisterProperty<string>(new PropertyInfo<string>("blob", "blob"));
        public string blob
        {
            get
            {
                return GetProperty(BlobProperty);
            }
            set
            {
                SetProperty(BlobProperty, value);
            }
        }

        //protected override void AddAuthorizationRules()
        //{
        //    //string[] canWrite = new string[] { "AdminUser", "RegularUser" };
        //    //string[] canRead = new string[] { "AdminUser", "RegularUser", "ReadOnlyUser" };
        //    //string[] admin = new string[] { "AdminUser" };
        //    //AuthorizationRules.AllowCreate(typeof(PriorityScreeningSimulation), admin);
        //    //AuthorizationRules.AllowDelete(typeof(PriorityScreeningSimulation), admin);
        //    //AuthorizationRules.AllowEdit(typeof(PriorityScreeningSimulation), canWrite);
        //    //AuthorizationRules.AllowGet(typeof(PriorityScreeningSimulation), canRead);

        //    //AuthorizationRules.AllowRead(PriorityScreeningSimulationIdProperty, canRead);
        //    //AuthorizationRules.AllowRead(ReviewIdProperty, canRead);

        //    ////AuthorizationRules.AllowWrite(NameProperty, canWrite);
        //}

        protected override void AddBusinessRules()
        {
            //ValidationRules.AddRule(Csla.Validation.CommonRules.MaxValue<decimal>, new Csla.Validation.CommonRules.MaxValueRuleArgs<decimal>(ReviewCodeSetFte1Property, 1));
            //ValidationRules.AddRule(Csla.Validation.CommonRules.StringRequired, new Csla.Validation.RuleArgs(ReviewCodeSetNameProperty));
            //ValidationRules.AddRule(Csla.Validation.CommonRules.StringMaxLength, new Csla.Validation.CommonRules.MaxLengthRuleArgs(ReviewCodeSetNameProperty, 20));
        }

#if !SILVERLIGHT

        protected void DataPortal_Fetch(SingleCriteria<PriorityScreeningSimulation, string> criteria) // used to return a specific Paper
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;

            string FolderAndFileName = DataFactoryHelper.NameBase + "ReviewId" + ri.ReviewId.ToString();
            string RemoteFolder = "priority_screening_simulation/" + FolderAndFileName + "/";
            string ScoresFile = RemoteFolder + criteria.Value;

            string blobConnection = AzureSettings.blobConnection;
            MemoryStream downloadedBlob = BlobOperations.DownloadBlobAsMemoryStream(blobConnection, "eppi-reviewer-data", ScoresFile);
            this.blob = Encoding.UTF8.GetString(downloadedBlob.GetBuffer(), 0, (int)downloadedBlob.Length);
        }
        protected override void DataPortal_Insert()
        {
            /*
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            LoadProperty(ReviewIdProperty, ri.ReviewId);
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_PriorityScreeningSimulationInsert", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ReadProperty(ReviewIdProperty)));
                    command.Parameters.Add(new SqlParameter("@PriorityScreeningSimulation_NAME", ReadProperty(NameProperty)));
                    command.Parameters.Add(new SqlParameter("@PriorityScreeningSimulation_DETAIL", ReadProperty(DetailProperty)));
                    command.Parameters.Add(new SqlParameter("@NEW_PriorityScreeningSimulation_ID", 0));
                    command.Parameters["@NEW_PriorityScreeningSimulation_ID"].Direction = System.Data.ParameterDirection.Output;
                    command.ExecuteNonQuery();
                    LoadProperty(PriorityScreeningSimulationIdProperty, command.Parameters["@NEW_PriorityScreeningSimulation_ID"].Value);
                }
                connection.Close();
            }
             */
        }

        protected override void DataPortal_Update()
        {

        }

        protected override void DataPortal_DeleteSelf()
        {

        }

        protected void DataPortal_Delete(SingleCriteria<PriorityScreeningSimulation, string> criteria)
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;

            string FolderAndFileName = DataFactoryHelper.NameBase + "ReviewId" + ri.ReviewId.ToString();
            string RemoteFolder = "priority_screening_simulation/" + FolderAndFileName + "/";
            string ScoresFile = RemoteFolder + criteria.Value;

            string blobConnection = AzureSettings.blobConnection;
            if (BlobOperations.ThisBlobExist(blobConnection, "eppi-reviewer-data", ScoresFile))
            {
                BlobOperations.DeleteIfExists(blobConnection, "eppi-reviewer-data", ScoresFile);
            }
        }

        internal static PriorityScreeningSimulation GetPriorityScreeningSimulation(string simulationName)
        {
            PriorityScreeningSimulation returnValue = new PriorityScreeningSimulation();
            returnValue.LoadProperty<string>(SimulationNameProperty, simulationName);
            returnValue.MarkOld();
            return returnValue;
        }

#endif

    }
}
