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
            try
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
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "RobotOpenAiHostedService Error, at marking failed jobs");
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
                try
                {
                    await Task.Delay(30000, InternalToken);
                }
                catch (Exception e)
                {
                    Logger.LogException(e, "RobotOpenAiHostedService main loop error in Task.Delay");
                }
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
                    if (res.UseFullTextDocument == false)
                    {
                        CreditWorker = Task<String>.Factory.StartNew((TaskCriteria) => DoGPTWork(res, cancellationToken, new List<MarkdownItemsPdfCommand.MiniPdfDoc>()), res);
                    }
                    else
                    {
                        CreditWorker = Task<String>.Factory.StartNew((TaskCriteria) => DoPDFWork(res, cancellationToken), res);
                    }
                }
                //if (CreditWorker == null) throw new InvalidOperationException("fake for testing");
            }
            catch (Exception e)
            {
                Logger.LogException(e, "RobotOpenAiHostedService FetchAndStartNextCreditJob error");
            }
        }

        /// <summary>
        /// Given a "RobotOpenAiTask" record trigger the preliminary MarkdownItemsPdfCommand child job.
        /// The child job ensures that there is a MarkDown version of all PDFs we'll need to use for GPT coding.
        /// This method meticolously reports progress, so that it can be resumed when cancelled.
        /// Resuming starts from scratch if job was interrupted in preparation/cleanup phases, 
        /// otherwise, if it was interrupted after starting the DataFactory job that does the PDF parsing, it will 
        /// resume by checking the DF job until it finishes or fails.
        /// PDFs in ER are read-only, so they get parsed only once and from them on their MarkDown version is re-used.
        /// </summary>
        /// <param name="RT"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        private string DoPDFWork(RobotOpenAiTaskReadOnly RT, CancellationToken ct) 
        {
            int ChildJobId = 0;
            //first detect the case where we're restarting a cancelled job and the cancellation happened while DataFactory was marking down PDFs - in this case
            //we don't want to start from scratch, but merely figure out when the parsing is done and then continue
            if (RT.Status == "Paused" && RT.CurrentItemId == 0)
            {
                //MarkdownItemsPdfCommand might have been interrupted during DataFactory, we need to dig some more
                try
                {
                    using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand("st_LogReviewJobGetLatestMarkdownJob", connection))
                        {
                            command.CommandType = System.Data.CommandType.StoredProcedure;
                            command.Parameters.Add(new SqlParameter("@ReviewId", RT.ReviewId));
                            using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                            {
                                if (reader.Read())
                                {
                                    string currentState = reader.GetString("CURRENT_STATE");
                                    int success = reader.GetInt32("SUCCESS");
                                    string JobMessage = reader.GetString("JOB_MESSAGE");
                                    if (currentState.StartsWith("Cancelled")
                                        && success == 0 && JobMessage.StartsWith("DF RunId: "))
                                    {//ok, we have everything we need...
                                        ChildJobId = reader.GetInt32("REVIEW_JOB_ID");
                                    }

                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    LogRobotJobException(RT, "RobotOpenAiHostedService DoPDFWork error in resuming markdownItemsPdfCommand execution", true, e);
                    return "Error";
                }
            }
            //start MarkdownItemsPdfCommand, returns a reviewJobId upon itself doing a fire-and-forget thing
            //monitor fire and forget progress, which we can do because we have a ref to the command object, when it's finished we can just move on!
            //finally, we start doing the GPT coding thing!
            
            MarkdownItemsPdfCommand markdownItemsPdfCommand = new MarkdownItemsPdfCommand(RT.ReviewId, RT.JobOwnerId, RT.RawCriteria.Substring(8), RT.RobotApiCallId, ChildJobId);

            try
            {
                markdownItemsPdfCommand = DataPortal.Execute(markdownItemsPdfCommand);
            }
            catch (Exception e)
            {
                //Logger.LogException(e, "RobotOpenAiHostedService DoPDFWork error in Task.Delay");
                if (markdownItemsPdfCommand.JobId > 0)
                {
                    DataFactoryHelper.UpdateReviewJobLog(markdownItemsPdfCommand.JobId, RT.ReviewId, "Failed during the synchronous execution phase", "", "MarkdownItemsPdfCommand (in RobotOpenAiHostedService)", true, false);
                }
                LogRobotJobException(RT, "RobotOpenAiHostedService DoPDFWork error in markdownItemsPdfCommand execution", true, e);
                return "Error";
            }
            //markdownItemsPdfCommand is now running its own fire-and-forget method, but reports how it's going via the Result property
            while (!markdownItemsPdfCommand.Result.StartsWith("Cancelled")
                    && !markdownItemsPdfCommand.Result.StartsWith("Failed")
                    && !markdownItemsPdfCommand.Result.StartsWith("Done")
                    && !markdownItemsPdfCommand.Result.StartsWith("Already Running"))
            {
                if (ct.WaitHandle.WaitOne(5000))//waits 5s or less, if cancellation is requested, in which case, returns true
                {
                    //cancellation was requested, so we stop
                    break;
                }                
            }
            if (markdownItemsPdfCommand.Result.StartsWith("Cancelled") || ct.IsCancellationRequested)
            {//mark the robot job as "paused": we'll start again as the AppPool resumes
                try
                {
                    using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand("st_UpdateRobotApiCallLog", connection))
                        {
                            command.CommandType = System.Data.CommandType.StoredProcedure;
                            command.Parameters.Add(new SqlParameter("@REVIEW_ID", RT.ReviewId));
                            command.Parameters.Add(new SqlParameter("@ROBOT_API_CALL_ID", RT.RobotApiCallId));
                            command.Parameters.Add(new SqlParameter("@STATUS", "Paused"));
                            command.Parameters.Add(new SqlParameter("@CURRENT_ITEM_ID", System.Data.SqlDbType.BigInt));
                            command.Parameters["@CURRENT_ITEM_ID"].Value = 0;
                            command.ExecuteNonQuery();
                        }
                    }
                    if (markdownItemsPdfCommand.JobId > 0)
                    {
                        DataFactoryHelper.UpdateReviewJobLog(markdownItemsPdfCommand.JobId, RT.ReviewId, "Cancelled", "", "MarkdownItemsPdfCommand", true, false);
                    }
                } 
                catch (Exception ex)
                {
                    Logger.LogException(ex, "Failed to mark jobs as paused/cancelled");
                }
                return "Cancelled";
            }
            else if (markdownItemsPdfCommand.Result.StartsWith("Failed"))
            {
                try
                {
                    //Parsing of PDFs failed, we'll put it the stack trace the list of ItemIds for which PDFs were being parsed
                    string FakeStackTrace = "Attempting to parse PDFs For these ItemIds:" + Environment.NewLine;
                    List<Int64> UniqueIds = new List<Int64>();
                    foreach(MarkdownItemsPdfCommand.MiniPdfDoc doc in markdownItemsPdfCommand.DocsToUpload)
                    {
                        if (!UniqueIds.Contains(doc.ItemId)) UniqueIds.Add(doc.ItemId);
                    }
                    foreach (Int64 id in UniqueIds)
                    {
                        FakeStackTrace += id + ",";
                    }
                    if (FakeStackTrace.EndsWith(","))
                    {
                        FakeStackTrace = FakeStackTrace.TrimEnd(',');
                    }
                    using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand("st_UpdateRobotApiCallLog", connection))
                        {
                            command.CommandType = System.Data.CommandType.StoredProcedure;
                            command.Parameters.Add(new SqlParameter("@REVIEW_ID", RT.ReviewId));
                            command.Parameters.Add(new SqlParameter("@ROBOT_API_CALL_ID", RT.RobotApiCallId));
                            command.Parameters.Add(new SqlParameter("@STATUS", "Failed"));
                            command.Parameters.Add(new SqlParameter("@CURRENT_ITEM_ID", System.Data.SqlDbType.BigInt));
                            command.Parameters["@CURRENT_ITEM_ID"].Value = 0;//
                            command.Parameters.Add(new SqlParameter("@ERROR_MESSAGE", "Parsing PDFs failed"));
                            command.Parameters.Add(new SqlParameter("@STACK_TRACE", FakeStackTrace));
                            command.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogException(ex, "Failed to mark GPT(pdf) job as failed");
                }
                Logger.LogError("markdownItemsPdfCommand for robot Job " + RT.RobotApiCallId + " failed, with message: " + markdownItemsPdfCommand.Result);
                return "Failed";
            }
            else if (markdownItemsPdfCommand.Result.StartsWith("Already Running"))
            {//ouch!! This should NEVER happen!
                //not obvious what to do... We'll mark this robot job as failed and hope the user will try again and that it will work, eventually.
                try
                {//we deliberately throw an exception to use LogRobotJobException(...)
                    throw new Exception("markdownItemsPdfCommand for robot Job " + RT.RobotApiCallId + " is ALREADY RUNNING! (Shouldn't happen!)");
                }
                catch (Exception ex)
                {
                    LogRobotJobException(RT, "markdownItemsPdfCommand for robot Job " + RT.RobotApiCallId + " is ALREADY RUNNING! (Shouldn't happen!)", true, ex);
                }
                return "Failed";
            }
            //if we're here, markdownItemsPdfCommand.Result must be "Done"!
            return DoGPTWork(RT, ct, markdownItemsPdfCommand.DocsToProcess);
        }



        /// <summary>
        /// Given a "RobotOpenAiTask" record call GPT on a per item (listed therein) basis
        /// and proceed until the end, or until Cancellation is requested.
        /// This method meticolously reports progress, so that it can be resumed when cancelled.
        /// It will also slow-down when GPT calls fails with "too many requests" error-type.
        /// </summary>
        /// <param name="RT"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        private string DoGPTWork(RobotOpenAiTaskReadOnly RT, CancellationToken ct, List<MarkdownItemsPdfCommand.MiniPdfDoc> Pdfs)
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
                    if (CurrentDelayInMs > ApiLatency)
                    {
                        //wait to respect the "requests per second" limit, if/when necessary
                        //see: http://classport.blogspot.com/2014/05/cancellationtoken-and-threadsleep.html
                        //we can't use Thread.Sleep(ms) because it would ignore the cancellation token and wait the alloted time "no matter what".
                        if (ct.WaitHandle.WaitOne(CurrentDelayInMs - ApiLatency))
                        {
                            //cancellation was requested, so we break the loop, before doing any work.
                            //This is to avoid the risk of running out of time for marking the job as "paused"
                            break;
                        }
                        //wait to respect the "requests per second" limit, if/when necessary
                    }
                    if (ct.IsCancellationRequested) 
                    {
                        //this clause is unlikely to ever get executed, I'm including it as "extra safety"
                        //if cancellation was requested during the "WaitOne" above, we should have "broken te loop" already
                        //we check one more time because it's v. important to mark all interrupted jobs as paused...
                        break; 
                    }
                    string doclist = "";
                    if (RT.UseFullTextDocument)
                    {
                        List<MarkdownItemsPdfCommand.MiniPdfDoc> PDFsForThisItem = Pdfs.FindAll(f => { return f.ItemId == RT.ItemIDsList[done]; });
                        foreach (MarkdownItemsPdfCommand.MiniPdfDoc doc in PDFsForThisItem)
                        {
                            doclist += doc.MarkDownFileName + ",";
                        }
                        doclist = doclist.Substring(0, doclist.Length - 1);
                    }
                    cmd = new RobotOpenAICommand(RT.ReviewSetId, RT.ItemIDsList[done], 0, RT.ItemIDsList.Count == done + 1 ? true : false,
                            RT.RobotApiCallId, RT.RobotContactId, RT.ReviewId, RT.JobOwnerId,
                            RT.OnlyCodeInTheRobotName, RT.LockTheCoding, RT.UseFullTextDocument, doclist,
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
                    else if (cmd.ReturnMessage == "Cancelled")
                    {
                        LogInfo("(+++)Cancel request accepted while running RobotOpenAICommand");
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
                //Last item processed has index "done-1" while "done" is the true val of how many items have been done
                //So we mark as "Paused" only works where cancellation is happening before having processed the last item
                //If the last item has been processed (done == todo) job has been marked as "Finished" by RobotOpenAICommand and we don't need to do anything
                if (ct.IsCancellationRequested && done < todo)
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
                LogRobotJobException(RT, "RobotOpenAiHostedService DoGPTWork error", true, e);
                return "Error";
            }
        }
        private void LogRobotJobException(RobotOpenAiTaskReadOnly RT, string headline, bool IsFatal, Exception e)
        {
            Logger.LogException(e, headline);
            try
            {
                using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
                {
                    string SavedMsg = e.Message;
                    if (SavedMsg.Length > 200) SavedMsg = SavedMsg.Substring(0, 200);
                    connection.Open();
                    using (SqlCommand command = new SqlCommand("st_UpdateRobotApiCallLog", connection))
                    {//this is to update the token numbers, and thus the cost, if we can
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@REVIEW_ID ", RT.ReviewId));
                        command.Parameters.Add(new SqlParameter("@ROBOT_API_CALL_ID", RT.RobotApiCallId));
                        command.Parameters.Add(new SqlParameter("@STATUS", IsFatal ? "Failed" : "Running"));
                        command.Parameters.Add(new SqlParameter("@CURRENT_ITEM_ID", System.Data.SqlDbType.BigInt));
                        command.Parameters["@CURRENT_ITEM_ID"].Value = 0;
                        command.Parameters.Add(new SqlParameter("@INPUT_TOKENS_COUNT", 0));
                        command.Parameters.Add(new SqlParameter("@OUTPUT_TOKENS_COUNT", 0));
                        command.Parameters.Add(new SqlParameter("@ERROR_MESSAGE", SavedMsg));
                        command.Parameters.Add(new SqlParameter("@STACK_TRACE", e.StackTrace));
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex) 
            {
                Logger.LogException(ex, "Failed to log a ROBOT JOB EXCEPTION!!");
            }
        }
        private void LogInfo(string message)
        {
            Debug.WriteLine(message);
            Logger.LogInformation(message);
        }
    }
}
