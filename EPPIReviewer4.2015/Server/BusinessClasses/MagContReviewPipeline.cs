using System;
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

        public static string runADFPieline(int ContactId, string TrainFileName, string InferenceFileName,
            string ResultsFileName, string ModelFileName, string MagContainer, string PreFilterThreshold,
            string FolderName, string AcceptanceThreshold, string ReviewRunVersion, string OverwriteRawProcessedData)
        {


#if (CSLA_NETCORE)

            var configuration = ERxWebClient2.Startup.Configuration.GetSection("AzureContReviewSettings");

#else
            var configuration = ConfigurationManager.AppSettings;

#endif

            string tenantID = configuration["tenantID"];
            string appClientId = configuration["appClientId"];
            string appClientSecret = configuration["appClientSecret"];
            string subscriptionId = configuration["subscriptionId"];
            string resourceGroup = configuration["resourceGroup"];
            string dataFactoryName = configuration["dataFactoryName"];
            string pipelineName = configuration["pipelineName"];

            int MagLogId = MagLog.SaveLogEntry("RunContReviewProcess", "started", "", ContactId);
            var context = new AuthenticationContext("https://login.windows.net/" + tenantID);
            ClientCredential cc = new ClientCredential(appClientId, appClientSecret);
            AuthenticationResult result = context.AcquireTokenAsync("https://management.azure.com/", cc).Result;
            ServiceClientCredentials cred = new TokenCredentials(result.AccessToken);
            var client = new DataFactoryManagementClient(cred)
            {
                SubscriptionId = subscriptionId
            };

            Dictionary<string, object> parameters = readParameters(TrainFileName, InferenceFileName, ResultsFileName,
                ModelFileName, MagContainer, PreFilterThreshold, FolderName, AcceptanceThreshold, ReviewRunVersion, OverwriteRawProcessedData);

            CreateRunResponse runResponse = client.Pipelines.CreateRunWithHttpMessagesAsync(resourceGroup, dataFactoryName, pipelineName, parameters: parameters).Result.Body;
            //Console.WriteLine("Pipeline run ID: " + runResponse.RunId);

            // Get the run status for the pipeline and all the activities
            string runStatus = client.PipelineRuns.GetAsync(resourceGroup, dataFactoryName, runResponse.RunId).Result.Status;
            while (runStatus.Equals("InProgress") || runStatus.Equals("Queued"))
            {
                int count = 0;
                //Console.WriteLine("Pipeline " + pipelineName + " " + runStatus);

                //RunFilterParameters filterParams = new RunFilterParameters(DateTime.UtcNow.AddMinutes(-10), DateTime.UtcNow.AddMinutes(10));
                //IList<ActivityRun> runs = client.ActivityRuns.QueryByPipelineRunAsync(resourceGroup, dataFactoryName, runResponse.RunId, filterParams).Result.Value;
                //foreach (ActivityRun run in runs)
                //{
                //Console.WriteLine(" Activity " + run.ActivityName + " " + run.Status);
                //}

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
                        MagLog.UpdateLogEntry("Error getting client", "RunContReviewProcess", MagLogId);
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
                        MagLog.UpdateLogEntry("Caught cloud error", "RunContReviewProcess", MagLogId);
                    }
                }

                MagLog.UpdateLogEntry(runStatus, "RunContReviewProcess", MagLogId);
            }
            return runStatus;
        }

        private static Dictionary<string, object> readParameters(string TrainFileName, string InferenceFileName,
            string ResultsFileName, string ModelFileName, string MagContainer, string PreFilterThreshold, string FolderName,
            string AcceptanceThreshold, string ReviewRunVersion, string OverwriteRawProcessedData)
        {


            Dictionary<string, object> parameters = new Dictionary<string, object>();


#if (CSLA_NETCORE)

            var configuration = ERxWebClient2.Startup.Configuration.GetSection("AzureMagSettings");

#else
            var configuration = ConfigurationManager.AppSettings;

#endif

            TrainFileName = TrainFileName == "" ? configuration["gold_standard_file"] : TrainFileName;
            InferenceFileName = InferenceFileName == "" ? configuration["new_papers_file"] : InferenceFileName;
            ResultsFileName = ResultsFileName == "" ? configuration["results_file_name"] : ResultsFileName;
            ModelFileName = ModelFileName == "" ? configuration["trained_model_filename"] : ModelFileName;
            MagContainer = MagContainer == "" ? configuration["mag_container"] : MagContainer;
            PreFilterThreshold = PreFilterThreshold == "" ? configuration["pre_filter_threshold"] : PreFilterThreshold;
            AcceptanceThreshold = AcceptanceThreshold == "" ? configuration["acceptance_threshold"] : AcceptanceThreshold;

            parameters.Add("interactive", configuration["interactive"]);
            parameters.Add("make_train_set", configuration["make_train_set"]);
            parameters.Add("make_inference_set", configuration["make_inference_set"]);
            //parameters.Add("review_run_version", configuration["review_run_version"]);
            //parameters.Add("overwrite_raw_processed_data", configuration["overwrite_raw_processed_data"]);
            parameters.Add("mag_account_name", configuration["mag_account_name"]);
            parameters.Add("mag_account_key", configuration["mag_account_key"]);
            parameters.Add("gold_account_name", configuration["gold_account_name"]);
            parameters.Add("gold_account_key", configuration["gold_account_key"]);
            parameters.Add("gold_container", configuration["gold_container"]);

            parameters.Add("gold_standard_file", FolderName + "/" + TrainFileName);
            parameters.Add("new_papers_file", FolderName + "/" + InferenceFileName);
            parameters.Add("results_file_name", FolderName + "/" + ResultsFileName);
            parameters.Add("trained_model_filename", ModelFileName);
            parameters.Add("overwrite_raw_processed_data", OverwriteRawProcessedData);
            parameters.Add("review_run_version", ReviewRunVersion);
            parameters.Add("acceptance_threshold", AcceptanceThreshold);
            parameters.Add("pre_filter_threshold", PreFilterThreshold);

            //mounts/training_datastore_ucl/experiment-v2/Incont2/experiment-v2/Incont2/per_paper_tfidf.pickle'
            parameters.Add("training_set_folder", FolderName);
            parameters.Add("mag_container", MagContainer);
            parameters.Add("ml_experiment_folder", FolderName);
            parameters.Add("min_review_size", "5");

            parameters.Add("training_set_storage_account_name", configuration["training_set_storage_account_name"]);
            parameters.Add("training_set_storage_key", configuration["training_set_storage_key"]);
            parameters.Add("training_set_container", configuration["training_set_container"]);
            parameters.Add("new_papers_account_name", configuration["new_papers_account_name"]);
            parameters.Add("new_papers_account_key", configuration["new_papers_account_key"]);
            parameters.Add("new_papers_container", configuration["new_papers_container"]);
            parameters.Add("results_storage_container", configuration["results_storage_container"]);

            
            

            return parameters;
        }

    }
}
