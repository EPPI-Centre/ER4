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
        public async Task<bool> RunDataFactoryProcessV2(string pipelineName, Dictionary<string, object> parameters, int ReviewId, int ReviewJobId, string Origin)
        {
            //Random r = new Random();
            //if (r.Next() > 0.0000001) throw new Exception("done manually for testing purpose...", new Exception("this is the inner exception"));
            
            int count = 0; int errorCount = 0;

            string tenantID = AzureSettings.tenantID;
            string appClientId = AzureSettings.appClientId;
            string appClientSecret = AzureSettings.appClientSecret;
            string subscriptionId = AzureSettings.subscriptionId;
            string resourceGroup = AzureSettings.resourceGroup;
            string dataFactoryName = AzureSettings.dataFactoryName;

            UpdateReviewJobLog(ReviewJobId, ReviewId, "Starting DF", "", Origin);

            var context = new AuthenticationContext("https://login.windows.net/" + tenantID);
            ClientCredential cc = new ClientCredential(appClientId, appClientSecret);
            AuthenticationResult AccessTokenResult = context.AcquireTokenAsync("https://management.azure.com/", cc).Result;
            ServiceClientCredentials cred = new TokenCredentials(AccessTokenResult.AccessToken);
            var client = new DataFactoryManagementClient(cred)
            {
                SubscriptionId = subscriptionId
            };
            Microsoft.Rest.Azure.AzureOperationResponse<CreateRunResponse> run = new Microsoft.Rest.Azure.AzureOperationResponse<CreateRunResponse>();
            try
            {
                run = await client.Pipelines.CreateRunWithHttpMessagesAsync(resourceGroup, dataFactoryName, pipelineName, parameters: parameters);
            }
            catch (Exception ex)
            {
                bool recovered = false;
                errorCount++;
                LogExceptionToFile(ex, ReviewId, ReviewJobId, Origin);
                while (recovered == false && errorCount < 10)
                {
                    Thread.Sleep(15 * 1000);
                    try
                    {
                        run = await client.Pipelines.CreateRunWithHttpMessagesAsync(resourceGroup, dataFactoryName, pipelineName, parameters: parameters);
                        recovered = true;
                    }
                    catch (Exception ex2)
                    {
                        LogExceptionToFile(ex2, ReviewId, ReviewJobId, Origin);
                        errorCount++;
                    }
                }
                if (recovered == false)
                {
                    UpdateReviewJobLog(ReviewJobId, ReviewId, "DF cloud error(s) (details in logfile)", "Pipeline: " + pipelineName, Origin, true, false);
                    return false;
                }
            }
            CreateRunResponse runResponse = run.Body;// client.Pipelines.CreateRunWithHttpMessagesAsync(resourceGroup, dataFactoryName, pipelineName, parameters: parameters).Result.Body;

            string runStatus = client.PipelineRuns.GetAsync(resourceGroup, dataFactoryName, runResponse.RunId).Result.Status;
            
            DateTime NextLogUpdateTime = DateTime.Now; //means that we do update the log immediately the 1st time
            while (runStatus.Equals("InProgress") || runStatus.Equals("Queued"))
            {
                if (AppIsShuttingDown)
                {
                    UpdateReviewJobLog(ReviewJobId, ReviewId, "Cancelled during DF", "DF RunId: " + runResponse.RunId, Origin, true, false);
                    return false;
                }

                if (DateTime.Now.ToUniversalTime().AddMinutes(5) > AccessTokenResult.ExpiresOn) // the token expires after an hour
                {
                    count++;
                    string accessToken = AccessTokenResult.AccessToken;
                    AccessTokenResult = context.AcquireTokenAsync("https://management.azure.com/", cc).Result;
                    cred = new TokenCredentials(AccessTokenResult.AccessToken);
                    client = new DataFactoryManagementClient(cred)
                    {
                        SubscriptionId = subscriptionId
                    };
                }

                Thread.Sleep(5 * 1000);
                if (AppIsShuttingDown)//checking again, because we just paused 5s or more!
                {
                    UpdateReviewJobLog(ReviewJobId, ReviewId, "Cancelled during DF", "DF RunId: " + runResponse.RunId, Origin, true, false);
                    return false;
                }
                try
                {
                    PipelineRun pr = client.PipelineRuns.Get(resourceGroup, dataFactoryName, runResponse.RunId); //Microsoft.Rest.Azure.CloudException if token has expired
                    if (pr != null)
                    {
                        runStatus = pr.Status;
                        if (DateTime.Now > NextLogUpdateTime)
                        {
                            
                            UpdateReviewJobLog(ReviewJobId, ReviewId, "DF Status: " + runStatus, "DF RunId: " + runResponse.RunId, Origin);
                            NextLogUpdateTime = DateTime.Now.AddMinutes(1);//keep updating the log every 1 minute
                        }
                    }
                    else
                    {
                        UpdateReviewJobLog(ReviewJobId, ReviewId, "Error getting DF client", "DF RunId: " + runResponse.RunId
                            + Environment.NewLine + "Pipeline: " + pipelineName, Origin, true, false);
                        return false;
                    }
                }
                catch (Exception e)
                {
                    if (e != null &&
                        (e is Microsoft.Rest.Azure.CloudException
                        || e is HttpRequestException
                        ))
                    {
                        errorCount++;
                        bool ShouldGiveUp = (errorCount >= 100);
                        if (ShouldGiveUp)
                        {
                            UpdateReviewJobLog(ReviewJobId, ReviewId, "DF cloud error (details in logfile)", "DF RunId: " + runResponse.RunId
                                + Environment.NewLine + "Pipeline: " + pipelineName, Origin, true, false);
                        }
                        else
                        {
                            UpdateReviewJobLog(ReviewJobId, ReviewId, "DF cloud error (details in logfile)", "DF RunId: " + runResponse.RunId
                                + Environment.NewLine + "Pipeline: " + pipelineName, Origin);
                        }
                        LogExceptionToFile(e, ReviewId, ReviewJobId, Origin);
                        if (ShouldGiveUp) return false;
                    }
                    else throw;
                }
            }
            if (runStatus == "Failed") return false;
            return true;
        }


        /// <summary>
        /// Updates a record in tb_REVIEW_JOB.
        /// </summary>
        /// <param name="LogId">The ID in tb_REVIEW_JOB</param>
        /// <param name="ReviewID"></param>
        /// <param name="Status">String describing where execution is</param>
        /// <param name="Message">After returning the DataFactory part, please leave empty, so to not overwrite the DF RunId, which may be needed if something went wrong </param>
        /// <param name="SetSuccess">False by default, to avoid setting the "SUCCESS" field (below) to "not null", before the task is finished... 
        /// We want to set this field to TRUE OR FALSE only when we know which one it is. 
        /// We detect "currently running" tasks by looking for NULL in this field.
        /// </param>
        /// <param name="SuccessValue">In the table, this value should be NULL if we're not finished. TRUE if we finished and it worked, FALSE if it failed/got interrupted</param>
        public static void UpdateReviewJobLog(int LogId, int ReviewID, string Status, string Message,
            string Origin, bool SetSuccess = false, bool SuccessValue = true)
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
                            if (SetSuccess) command.Parameters.Add(new SqlParameter("@Success", SuccessValue));
                            else command.Parameters.Add(new SqlParameter("@Success", System.DBNull.Value));
                            command.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogExceptionToFile(ex, ReviewID, LogId , "Datafactory UpdateReviewJobLog, from " + Origin);
                }
            }
        }
        public static void LogExceptionToFile(Exception ex, int ReviewID, int LogId, string Origin)
        {
#if CSLA_NETCORE
            if (Program.Logger != null && (Program.Logger as Serilog.ILogger) != null)
            {
                (Program.Logger as Serilog.ILogger).LogException(ex, Origin + " has an error. ReviewId:"
                        + ReviewID + " ReviewJobId:" + LogId);
            }
#endif
        }
        public static string NameBase
        {//used to generate different files on the cloud, based on where this is running, as it could be any dev/test machine as well as the live one (EPI3).
            get
            {
                if (AzureSettings.AddHostNamePrefixToBlobs.ToLower() != "false")
                {
                    return Environment.MachineName;
                }
                else return "";
            }
        }
    }
}
