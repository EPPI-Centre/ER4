﻿using System;
using System.Collections.Generic;
using Microsoft.Rest;
using Microsoft.Azure.Management.ResourceManager;
using Microsoft.Azure.Management.DataFactory;
using Microsoft.Azure.Management.DataFactory.Models;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Threading;
//using Azure.Security.KeyVault.Secrets;
//using Azure.Identity;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Azure.KeyVault.Core;
using System.Configuration;


//using Azure.Identity;
//using Azure.Security.KeyVault.Secrets;

/*  NB keyvault not being used at the moment
* This application will connect to an Azure Data Factory service, obtain secrets from an Azure Key Vault and trigger an ADF pipeline.
* 
* To make sure that this application has permissions to access the Azure Key Vault, add access policy for the Application ID used here 
* See https://docs.microsoft.com/en-us/azure/key-vault/managed-identity for details
*/
namespace BusinessLibrary.BusinessClasses
{
    class MagContReviewPipeline
    {

        // This is available as "DNS Name" from the overview page of the Key Vault.
        //static string keyVaultUri = "https://ucl.vault.azure.net/";

        public static string runADFPipeline(int ContactId, string TrainFileName, string InferenceFileName,
            string ResultsFileName, string ModelFileName, string MagVersion, string OldMagVersion, string PreFilterThreshold,
            string FolderName, string AcceptanceThreshold, string ReviewRunVersion, string OverwriteRawProcessedData,
            string ReviewSampleSize, string prepare_data, string process_train, string process_inference, string train_model,
            string score_papers, CancellationToken cancellationToken = default(CancellationToken))
        {




            string tenantID = AzureSettings.tenantID;
            string appClientId = AzureSettings.appClientId;
            string appClientSecret = AzureSettings.appClientSecret;
            string subscriptionId = AzureSettings.subscriptionId;
            string resourceGroup = AzureSettings.resourceGroup;
            string dataFactoryName = AzureSettings.dataFactoryName;
            string pipelineName = AzureSettings.pipelineName;
            int MagLogId = 0;
            if (prepare_data == "true")
            {
                MagLogId = MagLog.SaveLogEntry("AdfPipeline", "started", "updating parquet", ContactId);
            }
            else
            {
                MagLogId = MagLog.SaveLogEntry("AdfPipeline", "started", "Folder:" + FolderName, ContactId);
            }
            var context = new AuthenticationContext("https://login.windows.net/" + tenantID);
            ClientCredential cc = new ClientCredential(appClientId, appClientSecret);
            AuthenticationResult result = context.AcquireTokenAsync("https://management.azure.com/", cc).Result;
            ServiceClientCredentials cred = new TokenCredentials(result.AccessToken);
            var client = new DataFactoryManagementClient(cred)
            {
                SubscriptionId = subscriptionId
            };

            Dictionary<string, object> parameters = readParameters(TrainFileName, InferenceFileName, ResultsFileName,
                ModelFileName, MagVersion, OldMagVersion, PreFilterThreshold, FolderName, AcceptanceThreshold, ReviewRunVersion,
                OverwriteRawProcessedData, ReviewSampleSize, prepare_data, process_train, process_inference, train_model,
                score_papers);

            CreateRunResponse runResponse = client.Pipelines.CreateRunWithHttpMessagesAsync(resourceGroup, dataFactoryName, pipelineName, parameters: parameters).Result.Body;

            // Get the run status for the pipeline and all the activities
            string runStatus = client.PipelineRuns.GetAsync(resourceGroup, dataFactoryName, runResponse.RunId).Result.Status;
            while (runStatus.Equals("InProgress") || runStatus.Equals("Queued"))
            {
                int count = 0;

                if (cancellationToken.IsCancellationRequested)
                {
                    MagLog.UpdateLogEntry("Cancelled", "Cancellation token received at runADFPipeline", MagLogId);
                    return "Cancelled";
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
                    if (accessToken == result.AccessToken)
                    {
                        MagLog.UpdateLogEntry("Access token not renewed (" + count.ToString() + ")", "RunContReviewProcess", MagLogId);
                    }
                }

                Thread.Sleep(10 * 1000);
                try
                {
                    PipelineRun pr = client.PipelineRuns.Get(resourceGroup, dataFactoryName, runResponse.RunId); //Microsoft.Rest.Azure.CloudException if token has expired
                    if (pr != null)
                    {
                        runStatus = pr.Status;
                    }
                    else
                    {
                        MagLog.UpdateLogEntry("Error getting client", "RunContReviewProcess Folder:" + FolderName,
                            MagLogId);
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
                        MagLog.UpdateLogEntry("Caught cloud error", "RunContReviewProcess Folder:" + FolderName,
                            MagLogId);
                    }
                }
                if (prepare_data == "true")
                {
                    MagLog.UpdateLogEntry(runStatus, "updating parquet", MagLogId);
                }
                else
                {
                    MagLog.UpdateLogEntry(runStatus, "RunContReviewProcess Folder:" + FolderName, MagLogId);
                }
            }
            return runStatus;
        }

        private static Dictionary<string, object> readParameters(string TrainFileName, string InferenceFileName,
            string ResultsFileName, string ModelFileName, string MagVersion, string OldMagVersion, string PreFilterThreshold, string FolderName,
            string AcceptanceThreshold, string ReviewRunVersion, string OverwriteRawProcessedData,
            string ReviewSampleSize, string prepare_data, string process_train, string process_inference, string train_model,
            string score_papers)
        {

            Dictionary<string, object> parameters = new Dictionary<string, object>();


            // NEW PARAMETERS FOR NOTEBOOKS V.2
            // set from client / business objects
            parameters.Add("ml_min_review_size", 5);
            parameters.Add("ml_sample_size", 100000);
            parameters.Add("ml_acceptance_threshold", AcceptanceThreshold);
            parameters.Add("ml_pre_filter_threshold", PreFilterThreshold);
            parameters.Add("wf_prepare_data", prepare_data);
            parameters.Add("wf_process_train", process_train);
            parameters.Add("wf_process_inference", process_inference);
            parameters.Add("wf_train_model", train_model);
            parameters.Add("wf_score_papers", score_papers);
            parameters.Add("db_mag_container", "open-alex");
            parameters.Add("exp_gold_file_name", TrainFileName);
            parameters.Add("exp_new_papers_file_name", InferenceFileName);
            parameters.Add("exp_experiment_label", FolderName);
            parameters.Add("mag_version", "OpenAlexData/" + MagVersion);
            parameters.Add("old_mag_version", "OpenAlexData/" + OldMagVersion);

            // set from web.config
            parameters.Add("databricks_cluster_id", AzureSettings.databricks_cluster_id);
            parameters.Add("DynamicAnnotation", AzureSettings.DynamicAnnotation);
            parameters.Add("ml_pipeline_id", AzureSettings.ml_pipeline_id);
            parameters.Add("sa_account_name", AzureSettings.sa_account_name);
            parameters.Add("sa_secret_scope_name", AzureSettings.sa_secret_scope_name);
            parameters.Add("sa_secret_scope_key", AzureSettings.sa_secret_scope_key);
            parameters.Add("db_database_schema_version", AzureSettings.db_database_schema_version);
            parameters.Add("exp_experiments_container", AzureSettings.exp_experiments_container);
            parameters.Add("exp_run_suffix", AzureSettings.exp_run_suffix);
          
            return parameters;
        }


        /*
         * 
         * Revisit once we can get the right libraries installed!!!
         * 
        public static async Task<CloudBlobContainer> GetNewContRunContainer(string containerName)
        {
#if (CSLA_NETCORE)

            var configuration = ERxWebClient2.Startup.Configuration.GetSection("AzureMagSettings");
            string storageAccountName = configuration["MAGStorageAccount"];
            string storageAccountKey = configuration["MAGStorageAccountKey"];

#else
            string storageAccountName = ConfigurationManager.AppSettings["MAGStorageAccount"];
            string storageAccountKey = ConfigurationManager.AppSettings["MAGStorageAccountKey"];
#endif

            string storageConnectionString = "DefaultEndpointsProtocol=https;AccountName=" + storageAccountName + ";AccountKey=";
            storageConnectionString += storageAccountKey;

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            CloudBlobContainer container = blobClient.GetContainerReference("containerName");

            await container.CreateAsync();

            return container;

            //CloudBlobContainer container = blobClient.GetContainerReference("experiments");

        }
        */
    }
}
