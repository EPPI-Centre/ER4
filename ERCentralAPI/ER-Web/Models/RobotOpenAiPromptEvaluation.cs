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
    public class RobotOpenAiPromptEvaluation : BusinessBase<RobotOpenAiPromptEvaluation>
    {
#if SILVERLIGHT
    public RobotOpenAiPromptEvaluation() { }

        
#else
        public RobotOpenAiPromptEvaluation() { }
#endif


        public static readonly PropertyInfo<string> TitleProperty = RegisterProperty<string>(new PropertyInfo<string>("Title", "Title"));
        public string Title
        {
            get
            {
                return GetProperty(TitleProperty);
            }
            set
            {
                SetProperty(TitleProperty, value);
            }
        }
        public static readonly PropertyInfo<string> ReviewSetHtmlProperty = RegisterProperty<string>(new PropertyInfo<string>("ReviewSetHtml", "ReviewSetHtml"));
        public string ReviewSetHtml
        {
            get
            {
                return GetProperty(ReviewSetHtmlProperty);
            }
            set
            {
                SetProperty(ReviewSetHtmlProperty, value);
            }
        }
        public static readonly PropertyInfo<string> RobotNameProperty = RegisterProperty<string>(new PropertyInfo<string>("RobotName", "RobotName"));
        public string RobotName
        {
            get
            {
                return GetProperty(RobotNameProperty);
            }
            set
            {
                SetProperty(RobotNameProperty, value);
            }
        }
        public static readonly PropertyInfo<DateTime> WhenRunProperty = RegisterProperty<DateTime>(new PropertyInfo<DateTime>("WhenRun", "WhenRun"));
        public DateTime WhenRun
        {
            get
            {
                return GetProperty(WhenRunProperty);
            }
            set
            {
                SetProperty(WhenRunProperty, value);
            }
        }
        public static readonly PropertyInfo<bool> UsePdfsProperty = RegisterProperty<bool>(new PropertyInfo<bool>("UsePdfs", "UsePdfs"));
        public bool UsePdfs
        {
            get
            {
                return GetProperty(UsePdfsProperty);
            }
            set
            {
                SetProperty(UsePdfsProperty, value);
            }
        }
        public static readonly PropertyInfo<int> NRecordsProperty = RegisterProperty<int>(new PropertyInfo<int>("NRecords", "NRecords"));
        public int NRecords
        {
            get
            {
                return GetProperty(NRecordsProperty);
            }
            set
            {
                SetProperty(NRecordsProperty, value);
            }
        }
        public static readonly PropertyInfo<int> NIterationsProperty = RegisterProperty<int>(new PropertyInfo<int>("NIterations", "NIterations"));
        public int NIterations
        {
            get
            {
                return GetProperty(NIterationsProperty);
            }
            set
            {
                SetProperty(NIterationsProperty, value);
            }
        }
        public static readonly PropertyInfo<int> NCodesProperty = RegisterProperty<int>(new PropertyInfo<int>("NCodes", "NCodes"));
        public int NCodes
        {
            get
            {
                return GetProperty(NCodesProperty);
            }
            set
            {
                SetProperty(NCodesProperty, value);
            }
        }
        public static readonly PropertyInfo<int> TPProperty = RegisterProperty<int>(new PropertyInfo<int>("TP", "TP"));
        public int TP
        {
            get
            {
                return GetProperty(TPProperty);
            }
            set
            {
                SetProperty(TPProperty, value);
            }
        }
        public static readonly PropertyInfo<int> TNProperty = RegisterProperty<int>(new PropertyInfo<int>("TN", "TN"));
        public int TN
        {
            get
            {
                return GetProperty(TNProperty);
            }
            set
            {
                SetProperty(TNProperty, value);
            }
        }
        public static readonly PropertyInfo<int> FPProperty = RegisterProperty<int>(new PropertyInfo<int>("FP", "FP"));
        public int FP
        {
            get
            {
                return GetProperty(FPProperty);
            }
            set
            {
                SetProperty(FPProperty, value);
            }
        }
        public static readonly PropertyInfo<int> FNProperty = RegisterProperty<int>(new PropertyInfo<int>("FN", "FN"));
        public int FN
        {
            get
            {
                return GetProperty(FNProperty);
            }
            set
            {
                SetProperty(FNProperty, value);
            }
        }




#if !SILVERLIGHT

        protected void DataPortal_Fetch(SingleCriteria<PriorityScreeningSimulation, string> criteria) // used to return a specific Paper
        {

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

        protected void DataPortal_Delete(SingleCriteria<RobotOpenAiPromptEvaluation, string> criteria)
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

        internal static RobotOpenAiPromptEvaluation GetRobotOpenAiPromptEvaluation(SafeDataReader reader)
        {
            RobotOpenAiPromptEvaluation returnValue = new RobotOpenAiPromptEvaluation();
            returnValue.LoadProperty<string>(SimulationNameProperty, simulationName);
            returnValue.MarkOld();
            return returnValue;
        }

#endif

    }
}
