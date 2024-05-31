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
using System.ComponentModel;
using Csla.DataPortalClient;
using System.Threading;
using System.Diagnostics;


using System.Data.SqlClient;
using BusinessLibrary.Data;
using BusinessLibrary.Security;
using Microsoft.Azure.Search.Models;
using System.Reflection;
using EPPIDataServices.Helpers;


namespace BusinessLibrary.BusinessClasses
{
    public class RobotOpenAiHostedService : BackgroundService
    {
        private readonly ILogger<RobotOpenAiHostedService> Logger;
        private List<Task<String>> ApiKeyTasks = new List<Task<String>>();
        private CancellationTokenSource TokenSource = new CancellationTokenSource();
        private Task<string>? CreditWorker = null;
        private int ApiKeyTaskCount = 0;
        //private CancellationToken token = new CancellationTokenSource(5 * 60 * 1000).Token;
        public RobotOpenAiHostedService(ILogger<RobotOpenAiHostedService> logger)
        {
            this.Logger = logger;
        }
        public override Task StartAsync(CancellationToken cancellationToken)
        {
            LogInfo("RobotOpenAiHostedService is starting.");
            Task.Run(()=>ExecuteAsync(cancellationToken));
            return Task.CompletedTask;
        }
        public override Task StopAsync(CancellationToken cancellationToken)
        {
            LogInfo("RobotOpenAiHostedService is stopping.");
            TokenSource.Cancel();
            Task.WaitAll(ApiKeyTasks.ToArray());
            if (CreditWorker != null) CreditWorker.Wait();
            return Task.CompletedTask;
        }
        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_RobotApiCallLogMarkOldJobsAsFailed", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.ExecuteNonQuery();
                }
            }
            await Task.Delay(1000);
            CancellationToken InternalToken = TokenSource.Token;
            while (!cancellationToken.IsCancellationRequested && !InternalToken.IsCancellationRequested)
            {
                try
                {
                    if (CreditWorker == null || (CreditWorker.AsyncState as RobotOpenAiTaskReadOnly) == null ||
                    (CreditWorker.AsyncState as RobotOpenAiTaskReadOnly).RobotApiCallId == 0 ||
                    CreditWorker.Status == TaskStatus.RanToCompletion ||
                    CreditWorker.Status == TaskStatus.Faulted || CreditWorker.Status == TaskStatus.Canceled)
                    {
                        CreditWorker = null;
                        FetchAndStartNextCreditJob(InternalToken);
                        //if (CreditWorker == null) throw new InvalidOperationException("fake for testing");
                    }
                } 
                catch (Exception e)
                {
                    Logger.LogException(e, "RobotOpenAiHostedService main loop error");
                }          
                await Task.Delay(30000, InternalToken);
            }
            Task.WaitAll(ApiKeyTasks.ToArray());
            if (CreditWorker != null) CreditWorker.Wait();
        }

        private void FetchAndStartNextCreditJob(CancellationToken cancellationToken)
        {
            try
            {
                RobotOpenAiTaskReadOnly res = DataPortal.Fetch<RobotOpenAiTaskReadOnly>(RobotOpenAiTaskCriteria.NewNextCreditTaskCriteria());
                if (res != null && res.RobotApiCallId > 0)
                {
                    CreditWorker = Task<String>.Factory.StartNew((TaskCriteria) => DoWork(res, cancellationToken), res);
                }
                //if (CreditWorker == null) throw new InvalidOperationException("fake for testing");
            }
            catch (Exception e)
            {
                Logger.LogException(e, "RobotOpenAiHostedService FetchAndStartNextCreditJob error");
            }
        }

        private string DoWork(RobotOpenAiTaskReadOnly RT, CancellationToken ct)
        {
            try
            {
                //if (CreditWorker != null) throw new InvalidOperationException("fake for testing");
                LogInfo("Starting Batch " + RT.RobotApiCallId.ToString());
                RobotOpenAICommand cmd = new RobotOpenAICommand();
                int DefaultDelayInMs = 10000;
                int CurrentDelayInMs;
                int DelayedCallsWithoutError = 0;//used to decide when to decrement
                double RobotOpenAIRequestsPerMinute;
                if (double.TryParse(AzureSettings.RobotOpenAIRequestsPerMinute, out RobotOpenAIRequestsPerMinute))
                {
                    if (RobotOpenAIRequestsPerMinute > 0) DefaultDelayInMs = (int)(1000 * 60 / RobotOpenAIRequestsPerMinute);
                }

                int done = 0; //how many items we have done already, which is also the index value of the NEXT item to do
                int todo = 0;
                CurrentDelayInMs = DefaultDelayInMs;
                int delayIncrement = CurrentDelayInMs / 2;//we increment delays when we get "Error: Too Many Requests" answers

                if (RT.Status == "Queued" && RT.ItemIDsList.Count > 0)
                {//easy one, we just need to start
                    todo = RT.ItemIDsList.Count;
                }
                else if (RT.Status == "Paused" && RT.ItemIDsList.Count > 0)
                {//we want to resume, so need to find the last Item we processed
                    if (RT.CurrentItemId == 0)
                    {//got stopped before starting!
                        todo = RT.ItemIDsList.Count;
                    }
                    else
                    {
                        int index = RT.ItemIDsList.IndexOf(RT.CurrentItemId);
                        if (index >= 0 && index + 1 < RT.ItemIDsList.Count)
                        {
                            todo = RT.ItemIDsList.Count;
                            done = index + 1;
                        }
                    }
                }
                int ApiLatency = 0; DateTime start;
                while (done < todo && !ct.IsCancellationRequested)
                {
                    if (CurrentDelayInMs > DefaultDelayInMs + delayIncrement && DelayedCallsWithoutError >= 10)
                    {
                        //if we started throttling and processed 10 items without errors, we reduce the throttling, but without going back to our default value
                        //this is because failed requests slow down progress the most, so given we KNOW that at the default rate we are getting "too many request"
                        //we err on the safe side and and proceed a little slower than planned.
                        LogInfo("(-)Decrementing CurrentDelayInMs before Item = " + RT.ItemIDsList[done].ToString());
                        CurrentDelayInMs = CurrentDelayInMs - delayIncrement;
                    }
                    if (CurrentDelayInMs > ApiLatency) Thread.Sleep(CurrentDelayInMs - ApiLatency);//wait to respect the "requests per second" limit, if/when necessary
                    cmd = new RobotOpenAICommand(RT.ReviewSetId, RT.ItemIDsList[done], 0, RT.ItemIDsList.Count == done + 1 ? true : false,
                            RT.RobotApiCallId, RT.RobotContactId, RT.ReviewId, RT.JobOwnerId,
                            RT.OnlyCodeInTheRobotName, RT.LockTheCoding,
                            AzureSettings.RobotOpenAIBatchEndpoint, AzureSettings.RobotOpenAIBatchKey);
                    LogInfo("Submitting ItemId: " + RT.ItemIDsList[done].ToString());
                    start = DateTime.Now;
                    cmd = DataPortal.Execute<RobotOpenAICommand>(cmd);
                    ApiLatency = (int)((DateTime.Now.Ticks - start.Ticks) / 10000) - 50; //how long the cmd execution took, in Ms, minus 50ms to stay safe...
                    if (cmd.ReturnMessage == "Error: Too Many Requests")
                    {

                        LogInfo("(+)Incrementing CurrentDelayInMs at Item = " + RT.ItemIDsList[done].ToString());
                        CurrentDelayInMs += delayIncrement;
                        DelayedCallsWithoutError = 0;
                    }
                    else
                    {
                        if (CurrentDelayInMs > DefaultDelayInMs + delayIncrement)
                        {
                            LogInfo("[DelayedCallsWithoutError Incrementing at Item] = " + RT.ItemIDsList[done].ToString());
                            DelayedCallsWithoutError++;
                        }
                        done++;
                    }
                }
                if (ct.IsCancellationRequested && done - 1 < todo)
                {
                    long ItemId = (done - 1 == 0) ? 0 : RT.ItemIDsList[done - 1];//the last ID that was actually done
                    using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand("st_UpdateRobotApiCallLog", connection))
                        {
                            command.CommandType = System.Data.CommandType.StoredProcedure;
                            command.Parameters.Add(new SqlParameter("@REVIEW_ID", RT.ReviewId));
                            command.Parameters.Add(new SqlParameter("@ROBOT_API_CALL_ID", RT.RobotApiCallId));
                            command.Parameters.Add(new SqlParameter("@STATUS", "Paused"));
                            command.Parameters.Add(new SqlParameter("@CURRENT_ITEM_ID", ItemId));
                            command.Parameters.Add(new SqlParameter("@INPUT_TOKENS_COUNT", 0));
                            command.Parameters.Add(new SqlParameter("@OUTPUT_TOKENS_COUNT", 0));
                            command.ExecuteNonQuery();
                        }
                    }
                }
                LogInfo("Batch finished. CurrentDelayInMs = " + CurrentDelayInMs.ToString()
                    + "; DefaultDelayInMs = " + DefaultDelayInMs.ToString()
                    + "; DelayedCallsWithoutError = " + DelayedCallsWithoutError.ToString());
                return "Done";
            }
            catch (Exception e)
            {
                Logger.LogException(e, "RobotOpenAiHostedService DoWork error");
                return "Error";
            }
        }

        private void LogInfo(string message)
        {
            Debug.WriteLine(message);
            Logger.LogInformation(message);
        }
    }
}
