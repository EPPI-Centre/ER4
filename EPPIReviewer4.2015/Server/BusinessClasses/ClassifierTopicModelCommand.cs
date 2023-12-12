using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using Csla;
using Csla.Security;
using Csla.Core;
using Csla.Serialization;
using Csla.Silverlight;
//using Csla.Validation;
using System.ComponentModel;
using Csla.DataPortalClient;


#if !SILVERLIGHT
using System.Data.SqlClient;
using BusinessLibrary.Data;
using BusinessLibrary.Security;
//using SVM;
using System.IO;
using System.Xml;

using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Diagnostics;
using CsvHelper;

using System.Threading;
using System.Configuration;
using Microsoft.Azure.Management.DataFactory;
using Microsoft.Azure.Management.DataFactory.Models;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Rest;
using System.Data;
using Newtonsoft.Json;



#if (!CSLA_NETCORE)
using Microsoft.VisualBasic.FileIO;
#else
using System.Net.Http.Json;
#endif


#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class ClassifierTopicModelCommand : CommandBase<ClassifierTopicModelCommand>
    {
#if SILVERLIGHT
    public ClassifierTopicModelCommand(){}
#else
        public ClassifierTopicModelCommand() { }
#endif
        private string _title;
        private Int64 _attributeIdOn;
        private Int64 _attributeIdNotOn;
        private Int64 _attributeIdClassifyTo;
        private int _sourceId;

        // variables for applying the classifier
        private int _classifierId = -1;

        private string _returnMessage;

        public ClassifierTopicModelCommand(string title, Int64 attributeIdOn, Int64 attributeIdNotOn, Int64 attributeIdClassifyTo, int classiferId, int sourceId)
        {
            _title = title;
            _attributeIdOn = attributeIdOn;
            _attributeIdNotOn = attributeIdNotOn;
            _returnMessage = "Success";
            _classifierId = classiferId;
            _attributeIdClassifyTo = attributeIdClassifyTo;
            _sourceId = sourceId;
        }

        public string ReturnMessage
        {
            get
            {
                return _returnMessage;
            }
        }

        public static readonly PropertyInfo<ReviewInfo> RevInfoProperty = RegisterProperty<ReviewInfo>(new PropertyInfo<ReviewInfo>("RevInfo", "RevInfo"));
        public ReviewInfo RevInfo
        {
            get { return ReadProperty(RevInfoProperty); }
            set { LoadProperty(RevInfoProperty, value); }
        }

        public static readonly PropertyInfo<string> ReportBackProperty = RegisterProperty<string>(new PropertyInfo<string>("ReportBack", "ReportBack"));
        public string ReportBack
        {
            get { return ReadProperty(ReportBackProperty); }
            set { LoadProperty(ReportBackProperty, value); }
        }

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_title", _title);
            info.AddValue("_attributeIdOn", _attributeIdOn);
            info.AddValue("_attributeIdNotOn", _attributeIdNotOn);
            info.AddValue("_returnMessage", _returnMessage);
            info.AddValue("_classifierId", _classifierId);
            info.AddValue("_attributeIdClassifyTo", _attributeIdClassifyTo);
            info.AddValue("_sourceId", _sourceId);
        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            _title = info.GetValue<string>("_title");
            _attributeIdOn = info.GetValue<Int64>("_attributeIdOn");
            _attributeIdNotOn = info.GetValue<Int64>("_attributeIdNotOn");
            _returnMessage = info.GetValue<string>("_returnMessage");
            _classifierId = info.GetValue<int>("_classifierId");
            _attributeIdClassifyTo = info.GetValue<Int64>("_attributeIdClassifyTo");
            _sourceId = info.GetValue<int>("_sourceId");
        }


#if !SILVERLIGHT

        protected override void DataPortal_Execute()
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            System.Threading.Tasks.Task.Run(() => RunTopicModels(ri.ReviewId, ri.UserId));
        }

        static string blobConnection = AzureSettings.blobConnection;
        static string BaseUrlScoreModel = AzureSettings.BaseUrlScoreModel;
        static string apiKeyScoreModel = AzureSettings.apiKeyScoreModel;
        static string BaseUrlBuildModel = AzureSettings.BaseUrlBuildModel;
        static string apiKeyBuildModel = AzureSettings.apiKeyBuildModel;
        static string BaseUrlScoreNewRCTModel = AzureSettings.BaseUrlScoreNewRCTModel;
        static string apiKeyScoreNewRCTModel = AzureSettings.apiKeyScoreNewRCTModel;// Cochrane RCT Classifier v.2 (ensemble) blob storage
        const string TempPath = @"UserTempUploads/ContactId";

        const int TimeOutInMilliseconds = 360 * 50000; // 5 hours?


        private async Task RunTopicModels(int ReviewId, int ContactId)
        {

            string BatchGuid = Guid.NewGuid().ToString();
            int rowcount = 0;

            // 1) write the data to the Azure SQL database
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ClassifierGetClassificationDataToSQL", connection))
                {
                    command.CommandTimeout = 6000; // 10 minutes - if there are tens of thousands of items it can take a while
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ReviewId));
                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ID_CLASSIFY_TO", _attributeIdClassifyTo));
                    command.Parameters.Add(new SqlParameter("@ITEM_ID_LIST", ""));
                    command.Parameters.Add(new SqlParameter("@SOURCE_ID", _sourceId));
                    command.Parameters.Add(new SqlParameter("@BatchGuid", BatchGuid));
                    command.Parameters.Add(new SqlParameter("@ContactId", ContactId));
                    command.Parameters.Add(new SqlParameter("@MachineName", TrainingRunCommand.NameBase));
                    command.Parameters.Add(new SqlParameter("@ROWCOUNT", 0));
                    command.Parameters["@ROWCOUNT"].Direction = System.Data.ParameterDirection.Output;
                    command.ExecuteNonQuery();
                    rowcount = Convert.ToInt32(command.Parameters["@ROWCOUNT"].Value);
                }
            }
            if (rowcount == 0)
            {
                _returnMessage = "Error, Zero rows to score!";
                return;
            }
            if (rowcount < 50)
            {
                _returnMessage = "Not enough records to model";
                return;
            }

            // 2) trigger the data factory run (which in turn calls the Azure ML pipeline)

            string tenantID = AzureSettings.tenantID;
            string appClientId = AzureSettings.appClientId;
            string appClientSecret = AzureSettings.appClientSecret;
            string subscriptionId = AzureSettings.subscriptionId;
            string resourceGroup = AzureSettings.resourceGroup;
            string dataFactoryName = AzureSettings.dataFactoryName;

            string PipelineName = "EPPI Topic models v1";

            var context = new AuthenticationContext("https://login.windows.net/" + tenantID);
            ClientCredential cc = new ClientCredential(appClientId, appClientSecret);
            AuthenticationResult result = context.AcquireTokenAsync("https://management.azure.com/", cc).Result;
            ServiceClientCredentials cred = new TokenCredentials(result.AccessToken);
            var client = new DataFactoryManagementClient(cred)
            {
                SubscriptionId = subscriptionId
            };
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"BatchGuid", BatchGuid}
            };
            CancellationTokenSource source = new CancellationTokenSource();
            CancellationToken token = source.Token;
            DataFactoryHelper.RunDataFactoryProcess(PipelineName, parameters, true, ContactId, token);

            // 3) download the scores and insert them into the Reviewer database. This stored proc also cleans up the data in the Azure SQL database (i.e. deletes rows associated with this BatchGuid)
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ClassifierInsertSearchAndScores", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.CommandTimeout = 300; // 5 mins to be safe. I've seen queries with large numbers of searches / items take about 30 seconds, which times out live
                    command.Parameters.Add(new SqlParameter("@BatchGuid", BatchGuid));
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ReviewId));
                    command.Parameters.Add(new SqlParameter("@CONTACT_ID", ContactId));
                    command.Parameters.Add(new SqlParameter("@SearchTitle", "Topic: "));
                    command.ExecuteNonQuery();
                }
            }
        }

#endif
    }
}

