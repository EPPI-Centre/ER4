using BusinessLibrary.Data;
using BusinessLibrary.Security;
using Microsoft.Azure.DataLake.Store;
using Microsoft.Azure.Management.DataFactory;
using Microsoft.Azure.Management.DataLake.Analytics;
using Microsoft.Azure.Management.DataLake.Store;
using Microsoft.Azure.Management.DataFactory.Models;
using Microsoft.Rest;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
#if !CSLA_NETCORE
using Microsoft.Rest.Azure.Authentication;
#else
using EPPIDataServices.Helpers;
#endif

namespace BusinessLibrary.BusinessClasses
{
    class DataFactoryHelper
    {

        private static Boolean AppIsShuttingDown
        {
            get
            {
#if CSLA_NETCORE
                try { return Program.AppIsShuttingDown; }
                catch { return false; }
#else
                return false;
#endif
            }
        }

        public static bool RunDataFactoryProcess(string pipelineName, Dictionary<string, object> parameters, bool doLogging, int ContactId,
            CancellationToken cancellationToken = default(CancellationToken))
        {

            string tenantID = AzureSettings.tenantID;
            string appClientId = AzureSettings.appClientId;
            string appClientSecret = AzureSettings.appClientSecret;
            string subscriptionId = AzureSettings.subscriptionId;
            string resourceGroup = AzureSettings.resourceGroup;
            string dataFactoryName = AzureSettings.dataFactoryName;

            int MagLogId = 0;
            if (doLogging == true)
            {
                MagLogId = MagLog.SaveLogEntry("DataFactory Event", "starting", pipelineName, ContactId);
            }

            var context = new AuthenticationContext("https://login.windows.net/" + tenantID);
            ClientCredential cc = new ClientCredential(appClientId, appClientSecret);
            AuthenticationResult result = context.AcquireTokenAsync("https://management.azure.com/", cc).Result;
            ServiceClientCredentials cred = new TokenCredentials(result.AccessToken);
            var client = new DataFactoryManagementClient(cred)
            {
                SubscriptionId = subscriptionId
            };

            CreateRunResponse runResponse = client.Pipelines.CreateRunWithHttpMessagesAsync(resourceGroup, dataFactoryName, pipelineName, parameters: parameters).Result.Body;

            string runStatus = client.PipelineRuns.GetAsync(resourceGroup, dataFactoryName, runResponse.RunId).Result.Status;
            while (runStatus.Equals("InProgress") || runStatus.Equals("Queued"))
            {
                if (cancellationToken.IsCancellationRequested && doLogging == true)
                {
                    MagLog.UpdateLogEntry("Cancelled by cancellation token", pipelineName, MagLogId);
                    return false;
                }
                int count = 0;
                //Console.WriteLine(runStatus);

                if (DateTime.Now.ToUniversalTime().AddMinutes(5) > result.ExpiresOn) // the token expires after an hour
                {
                    count++;
                    string accessToken = result.AccessToken;
                    result = context.AcquireTokenAsync("https://management.azure.com/", cc).Result;
                    cred = new TokenCredentials(result.AccessToken);
                    client = new DataFactoryManagementClient(cred)
                    {
                        SubscriptionId = subscriptionId
                    };
                    if (accessToken == result.AccessToken && doLogging == true)
                    {
                        MagLog.UpdateLogEntry("Access token not renewed (" + count.ToString() + ")", pipelineName, MagLogId);
                    }
                }

                Thread.Sleep(5 * 1000);
                try
                {
                    PipelineRun pr = client.PipelineRuns.Get(resourceGroup, dataFactoryName, runResponse.RunId); //Microsoft.Rest.Azure.CloudException if token has expired
                    if (pr != null)
                    {
                        runStatus = pr.Status;
                        if (doLogging == true)
                        {
                            MagLog.UpdateLogEntry(runStatus, pipelineName, MagLogId);
                        }
                    }
                    else
                    {
                        if (doLogging == true)
                        {
                            MagLog.UpdateLogEntry("Error getting client", pipelineName, MagLogId);
                        }
                    }
                }
                catch (Microsoft.Rest.Azure.CloudException e)
                {
                    if (e != null)
                    {
                        result = context.AcquireTokenAsync("https://management.azure.com/", cc).Result;
                        cred = new TokenCredentials(result.AccessToken);
                        client = new DataFactoryManagementClient(cred)
                        {
                            SubscriptionId = subscriptionId
                        };
                        if (doLogging == true)
                        {
                            MagLog.UpdateLogEntry("Caught cloud error", pipelineName, MagLogId);
                        }
                    }
                }
            }
            return true;
        }
        public bool RunDataFactoryProcessV2(string pipelineName, Dictionary<string, object> parameters, int ReviewId, int ReviewJobId)
        {

            string tenantID = AzureSettings.tenantID;
            string appClientId = AzureSettings.appClientId;
            string appClientSecret = AzureSettings.appClientSecret;
            string subscriptionId = AzureSettings.subscriptionId;
            string resourceGroup = AzureSettings.resourceGroup;
            string dataFactoryName = AzureSettings.dataFactoryName;

            UpdateReviewJobLog(ReviewJobId, ReviewId, "Starting DF", "ReviewJobId: " + ReviewJobId.ToString());

            var context = new AuthenticationContext("https://login.windows.net/" + tenantID);
            ClientCredential cc = new ClientCredential(appClientId, appClientSecret);
            AuthenticationResult result = context.AcquireTokenAsync("https://management.azure.com/", cc).Result;
            ServiceClientCredentials cred = new TokenCredentials(result.AccessToken);
            var client = new DataFactoryManagementClient(cred)
            {
                SubscriptionId = subscriptionId
            };

            CreateRunResponse runResponse = client.Pipelines.CreateRunWithHttpMessagesAsync(resourceGroup, dataFactoryName, pipelineName, parameters: parameters).Result.Body;

            string runStatus = client.PipelineRuns.GetAsync(resourceGroup, dataFactoryName, runResponse.RunId).Result.Status;
            int count = 0; int errorCount = 0;
            DateTime NextLogUpdateTime = DateTime.Now; //means that we do update the log immediately the 1st time
            while (runStatus.Equals("InProgress") || runStatus.Equals("Queued"))
            {
                if (AppIsShuttingDown)
                {
                    UpdateReviewJobLog(ReviewJobId, ReviewId, "Cancelled during DF", "DF RunId: " + runResponse.RunId, false);
                    return false;
                }

                if (DateTime.Now.ToUniversalTime().AddMinutes(5) > result.ExpiresOn) // the token expires after an hour
                {
                    count++;
                    string accessToken = result.AccessToken;
                    result = context.AcquireTokenAsync("https://management.azure.com/", cc).Result;
                    cred = new TokenCredentials(result.AccessToken);
                    client = new DataFactoryManagementClient(cred)
                    {
                        SubscriptionId = subscriptionId
                    };
                    //Ask James what this is for!!
                    //if (accessToken == result.AccessToken && doLogging == true)
                    //{

                    //    MagLog.UpdateLogEntry("Access token not renewed (" + count.ToString() + ")", pipelineName, MagLogId);
                    //}
                }

                Thread.Sleep(5 * 1000);
                try
                {
                    PipelineRun pr = client.PipelineRuns.Get(resourceGroup, dataFactoryName, runResponse.RunId); //Microsoft.Rest.Azure.CloudException if token has expired
                    if (pr != null)
                    {
                        if (DateTime.Now > NextLogUpdateTime)
                        {
                            runStatus = pr.Status;
                            UpdateReviewJobLog(ReviewJobId, ReviewId, "DF Status: " + runStatus, "DF RunId: " + runResponse.RunId);
                            NextLogUpdateTime = DateTime.Now.AddMinutes(1);//keep updating the log every 1 minute
                        }
                    }
                    else
                    {
                        UpdateReviewJobLog(ReviewJobId, ReviewId, "Error getting DF client", "DF RunId: " + runResponse.RunId
                            + Environment.NewLine + "Pipeline: " + pipelineName);
                        return false;
                    }
                }
                catch (Microsoft.Rest.Azure.CloudException e)
                {
                    if (e != null)
                    {
                        errorCount++;
                        UpdateReviewJobLog(ReviewJobId, ReviewId, "DF cloud error (details in logfile)", "DF RunId: " + runResponse.RunId
                            + Environment.NewLine + "Pipeline: " + pipelineName, errorCount <= 100 ? true : false);
                        LogExceptionToFile(e, ReviewId, ReviewJobId);
                        if (errorCount > 99) return false;
                    }
                }
            }
            return true;
        }
        public static void UpdateReviewJobLog(int LogId, int ReviewID, string Status, string Message, bool Success = true)
        {
            if (LogId > 0)
            {
                try
                {
                    using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand("st_LogReviewJobUpdate", connection))
                        {
                            command.CommandType = System.Data.CommandType.StoredProcedure;
                            command.Parameters.Add(new SqlParameter("@ReviewId", ReviewID));
                            command.Parameters.Add(new SqlParameter("@REVIEW_JOB_ID", LogId));
                            command.Parameters.Add(new SqlParameter("@CurrentState", Status));
                            command.Parameters.Add(new SqlParameter("@JobMessage", Message));
                            command.Parameters.Add(new SqlParameter("@Success", Success));
                            command.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogExceptionToFile(ex, ReviewID, LogId);
                }
            }
        }
        private static void LogExceptionToFile(Exception ex, int ReviewID, int LogId)
        {
#if CSLA_NETCORE
            if (Program.Logger != null && (Program.Logger as ILogger) != null)
            {
                (Program.Logger as ILogger).LogException(ex, "UpdateReviewJobLog in TrainingRunCommandV2 has an error. ReviewId:"
                        + ReviewID + "ReviewJobId:" + LogId);
            }
#endif
        }
    }
}
