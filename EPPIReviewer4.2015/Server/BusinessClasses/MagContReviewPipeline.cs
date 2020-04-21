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
            string FolderName, string AcceptanceThreshold)
        {
            var appSettings = ConfigurationManager.AppSettings;
            string tenantID = appSettings["tenantID"];
            string appClientId = appSettings["appClientId"];
            string appClientSecret = appSettings["appClientSecret"];
            string subscriptionId = appSettings["subscriptionId"];
            string resourceGroup = appSettings["resourceGroup"];
            string dataFactoryName = appSettings["dataFactoryName"];
            string pipelineName = appSettings["pipelineName"];

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
                ModelFileName, MagContainer, PreFilterThreshold, FolderName, AcceptanceThreshold);

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
            string AcceptanceThreshold)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            var appSettings = ConfigurationManager.AppSettings;

            TrainFileName = TrainFileName == "" ? appSettings["gold_standard_file"] : TrainFileName;
            InferenceFileName = InferenceFileName == "" ? appSettings["new_papers_file"] : InferenceFileName;
            ResultsFileName = ResultsFileName == "" ? appSettings["results_file_name"] : ResultsFileName;
            ModelFileName = ModelFileName == "" ? appSettings["trained_model_filename"] : ModelFileName;
            MagContainer = MagContainer == "" ? appSettings["mag_container"] : MagContainer;
            PreFilterThreshold = PreFilterThreshold == "" ? appSettings["pre_filter_threshold"] : PreFilterThreshold;
            AcceptanceThreshold = AcceptanceThreshold == "" ? appSettings["acceptance_threshold"] : AcceptanceThreshold;

            parameters.Add("interactive", appSettings["interactive"]);
            parameters.Add("make_train_set", appSettings["make_train_set"]);
            parameters.Add("make_inference_set", appSettings["make_inference_set"]);
            parameters.Add("review_run_version", appSettings["review_run_version"]);
            parameters.Add("overwrite_raw_processed_data", appSettings["overwrite_raw_processed_data"]);
            parameters.Add("mag_account_name", appSettings["mag_account_name"]);
            parameters.Add("mag_account_key", appSettings["mag_account_key"]);
            parameters.Add("gold_account_name", appSettings["gold_account_name"]);
            parameters.Add("gold_account_key", appSettings["gold_account_key"]);
            parameters.Add("gold_container", appSettings["gold_container"]);

            parameters.Add("gold_standard_file", FolderName + "/" + TrainFileName);
            parameters.Add("new_papers_file", FolderName + "/" + InferenceFileName);
            parameters.Add("results_file_name", FolderName + "/" + ResultsFileName);
            parameters.Add("trained_model_filename", ModelFileName);

            //mounts/training_datastore_ucl/experiment-v2/Incont2/experiment-v2/Incont2/per_paper_tfidf.pickle'
            parameters.Add("training_set_folder", FolderName);
            parameters.Add("mag_container", MagContainer);
            parameters.Add("ml_experiment_folder", FolderName);

            parameters.Add("training_set_storage_account_name", appSettings["training_set_storage_account_name"]);
            parameters.Add("training_set_storage_key", appSettings["training_set_storage_key"]);
            parameters.Add("training_set_container", appSettings["training_set_container"]);
            parameters.Add("new_papers_account_name", appSettings["new_papers_account_name"]);
            parameters.Add("new_papers_account_key", appSettings["new_papers_account_key"]);
            parameters.Add("new_papers_container", appSettings["new_papers_container"]);
            parameters.Add("acceptance_threshold", AcceptanceThreshold);
            parameters.Add("results_storage_container", appSettings["results_storage_container"]);

            parameters.Add("min_review_size", "5");
            parameters.Add("pre_filter_threshold", PreFilterThreshold);

            return parameters;
        }

    }
}
