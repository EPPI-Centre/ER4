using BusinessLibrary.Data;
using BusinessLibrary.Security;
using Microsoft.Azure.DataLake.Store;
using Microsoft.Azure.Management.DataFactory;
using Microsoft.Azure.Management.DataLake.Analytics;
using Microsoft.Azure.Management.DataLake.Analytics.Models;
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
#endif

namespace BusinessLibrary.BusinessClasses
{
    class DataFactoryHelper
    {

        //public static bool RunDataFactoryProcess(string pipelineName, Dictionary<string, object> parameters, bool doLogging, int ContactId,
        //    CancellationToken cancellationToken = default(CancellationToken))
        //{
        //    //var configuration = ERxWebClient2.Startup.Configuration.GetSection("AzureContReviewSettings");
        //    throw new NotImplementedException();
        //    //return false;
        //}

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


    }
}
