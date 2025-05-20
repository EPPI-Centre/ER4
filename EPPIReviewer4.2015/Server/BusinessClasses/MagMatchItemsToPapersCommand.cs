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
using System.Threading.Tasks;

#if !SILVERLIGHT
using System.Data.SqlClient;
using BusinessLibrary.Data;
using BusinessLibrary.Security;
using System.Configuration;
using System.Collections.Concurrent;
//using System.Timers;
#if !CSLA_NETCORE
using System.Web.Hosting;
#endif
#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class MagMatchItemsToPapersCommand : LongLastingFireAndForgetCommand
    {
        public MagMatchItemsToPapersCommand() { }


        private bool _AllInReview;
        private Int64 _ITEM_ID;
        private Int64 _ATTRIBUTE_ID;
        private string _FindOrRemove = "";
        private string _currentStatus = "";
        [Newtonsoft.Json.JsonProperty]
        public string currentStatus
        {
            get
            {
                return _currentStatus;
            }
        }

        public MagMatchItemsToPapersCommand(string FindOrRemove, bool AllInReview, Int64 ItemId, Int64 AttributeId)
        {
            _FindOrRemove = FindOrRemove;
            _AllInReview = AllInReview;
            _ATTRIBUTE_ID = AttributeId;
            _ITEM_ID = ItemId;
        }

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_FindOrRemove", _FindOrRemove);
            info.AddValue("_AllInReview", _AllInReview);
            info.AddValue("_ITEM_ID", _ITEM_ID);
            info.AddValue("_ATTRIBUTE_ID", _ATTRIBUTE_ID);
            info.AddValue("_currentStatus", _currentStatus);
        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            _FindOrRemove = info.GetValue<string>("_FindOrRemove");
            _AllInReview = info.GetValue<bool>("_AllInReview");
            _currentStatus = info.GetValue<string>("_currentStatus");
            _ITEM_ID = info.GetValue<Int64>("_ITEM_ID");
            _ATTRIBUTE_ID = info.GetValue<Int64>("_ATTRIBUTE_ID");
        }


#if !SILVERLIGHT

        protected override void DataPortal_Execute()
        {

            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            if (_FindOrRemove == "FindMatches")
            {
                if (_ITEM_ID > 0)
                {
                    MagPaperItemMatch.MatchItemToMag(_ITEM_ID, ri.ReviewId);
                }
                else
                {
                    //from March 2024: we check if there is a job running already!
                    using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
                    {
                        connection.Open(); 
                        using (SqlCommand command = new SqlCommand("st_MagMatchingCheckOngoingLog", connection))
                        {
                            int MagMatchTimeoutInMinutes;
                            command.CommandType = System.Data.CommandType.StoredProcedure;
                            command.Parameters.Add("@revID", System.Data.SqlDbType.Int);
                            command.Parameters["@revID"].Value = ri.ReviewId;
                            
                            if (int.TryParse(AzureSettings.MagMatchTimeoutInMinutes, out MagMatchTimeoutInMinutes))  
                                command.Parameters.Add(new SqlParameter("@customTimeoutInMinutes", MagMatchTimeoutInMinutes));
                            
                            command.Parameters.Add("@RETURN_VALUE", System.Data.SqlDbType.Int);
                            command.Parameters["@RETURN_VALUE"].Direction = System.Data.ParameterDirection.ReturnValue;
                            command.ExecuteNonQuery();
                            if (command.Parameters["@RETURN_VALUE"].Value.ToString() == "-1")
                            {
                                _currentStatus = "Another matching job is already running for this review.";
                                return;
                            }
                            else if (command.Parameters["@RETURN_VALUE"].Value.ToString() == "2")
                                ErrorLogSink("Check for OA matching job concurrency (st_MagMatchingCheckOngoingLog) returned '2' which is NOT SUPPOSED TO HAPPEN!");
                        }
                    }
                    int MagLogId = MagLog.SaveLogEntry("MAG matching", "Starting", "Review: " + ri.ReviewId +
                                ", Threads: " + AzureSettings.MagMatchItemsMaxThreadCount, ri.UserId);

#if CSLA_NETCORE
                    //see AppIsShuttingDown property to see how we're making graceful shutdown possible in both ER4 and ER6
                    System.Threading.Tasks.Task.Run(() => doMatchItems(ri.ReviewId, MagLogId, Convert.ToInt32(AzureSettings.MagMatchItemsMaxThreadCount), this.CancelToken));
#else
                    //see: https://codingcanvas.com/using-hostingenvironment-queuebackgroundworkitem-to-run-background-tasks-in-asp-net/
                    HostingEnvironment.QueueBackgroundWorkItem(cancellationToken =>
                        doMatchItems(ri.ReviewId, MagLogId, int.Parse(AzureSettings.MagMatchItemsMaxThreadCount), cancellationToken));
#endif
                }
            }
            else
            {
                using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
                {
                    connection.Open();
                    if (_FindOrRemove.Contains("NON-MANUAL"))
                    {
                        using (SqlCommand command = new SqlCommand("st_MagMatchedPapersClearNonManual", connection))
                        {
                            command.CommandType = System.Data.CommandType.StoredProcedure;
                            command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                            command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ID", _ATTRIBUTE_ID));
                            command.ExecuteNonQuery();
                        }
                    }
                    else
                    {
                        using (SqlCommand command = new SqlCommand("st_MagMatchedPapersClear", connection))
                        {
                            command.CommandType = System.Data.CommandType.StoredProcedure;
                            command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                            command.Parameters.Add(new SqlParameter("@ITEM_ID", _ITEM_ID));
                            command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ID", _ATTRIBUTE_ID));
                            command.ExecuteNonQuery();
                        }
                    }
                    connection.Close();
                }
            }
        }

        private async Task doMatchItems(int ReviewId, int MagLogId, int maxThreadCount,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            doMatchingWithThreads(ReviewId, MagLogId, maxThreadCount, cancellationToken);     
        }

        private void doMatchingWithThreads(int ReviewId, int MagLogId, int maxThreadCount,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            int activeThreadCount = 0;
            int errorCount = 0;
            string result;
            int totalCount = 0;
            int partialCount = 0;
            int UpdateLogEveryN_items = 200;
            ConcurrentQueue<string> resultQueue = new ConcurrentQueue<string>();
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_MagMatchItemsGetIdList", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ReviewId));
                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ID", _ATTRIBUTE_ID));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        //Int64 ItemId = -1;
                        //System.Timers.Timer timer = new System.Timers.Timer(20000);//update TB_MAG_LOG every 20 seconds
                        //timer.Elapsed += (source, eventArgs) =>
                        //{
                        //    MagLog.UpdateLogEntry("Running", "Review: " + ReviewId + ", total: " + totalCount.ToString() +
                        //                       ", errors: " + errorCount.ToString() + ", last: " + ItemId.ToString()
                        //                       , MagLogId);
                        //};
                        //timer.AutoReset = true;
                        //timer.Enabled = true;
                        DateTime nextCheckpoint = DateTime.Now.AddSeconds(20);
                        while (reader.Read())
                        {
                            if (cancellationToken.IsCancellationRequested || AppIsShuttingDown)
                            {
                                ErrorLogSink("Cancelling inside MagMatchItemsCommand");
                                MagLog.UpdateLogEntry("CancelToken(1)!", "Review: " + ReviewId + ", totalDone: " + totalCount.ToString() +
                                    ", errors: " + errorCount.ToString() + ", Threads: " + maxThreadCount.ToString(), MagLogId);
                                return;
                            }
                            //if (Program.Logger != null) Program.Logger.Error("please make sense AGAIN! " + Program.cancelling.ToString());

                            totalCount++;
                            if (activeThreadCount < maxThreadCount)
                            {
                                Interlocked.Increment(ref activeThreadCount);
                                Int64 ItemId = reader.GetInt64("ITEM_ID");
                                Task.Run(() =>
                                {
                                    try
                                    {
                                        MagPaperItemMatch.MatchItemToMag(ItemId, ReviewId);
                                    }
                                    catch (Exception e)
                                    {
                                        if (e.Message == "Thread was being aborted.")
                                        {
                                            ErrorLogSink("Cancelling inside MagMatchItemsCommand");
                                            MagLog.UpdateLogEntry("CancelToken(2)!", "Review: " + ReviewId + ", totalDone: " + totalCount.ToString() +
                                                ", errors: " + errorCount.ToString() + ", Threads: " + maxThreadCount.ToString(), MagLogId);
                                            return;
                                        }
                                        else
                                        {
                                            resultQueue.Enqueue("ERROR: " + ItemId);
                                            MagLog.UpdateLogEntry("Running", "Review: " + ReviewId + ", total: " + totalCount.ToString() +
                                                ", errors: " + errorCount.ToString() + ", last: " + ItemId.ToString()
                                                + " Exception: " + e.Message
                                                , MagLogId);// maxThreadCount.ToString(), MagLogId);
                                        }
                                    }
                                    finally
                                    {
                                        Interlocked.Decrement(ref activeThreadCount);
                                    }
                                });
                                partialCount++;
                                if (DateTime.Now > nextCheckpoint || partialCount >= UpdateLogEveryN_items)
                                {
                                    nextCheckpoint = DateTime.Now.AddSeconds(20);
                                    MagLog.UpdateLogEntry("Running", "Review: " + ReviewId + ", total: " + totalCount.ToString() +
                                               ", errors: " + errorCount.ToString() + ", last: " + ItemId.ToString()
                                               + ", processed " + partialCount.ToString() + " since last log-update."
                                               , MagLogId);
                                    partialCount = 0;
                                }
                                while (activeThreadCount >= maxThreadCount)
                                {
                                    // Clear out queue
                                    while (resultQueue.TryDequeue(out result))
                                    {
                                        if (result.Contains("ERROR"))
                                        {
                                            errorCount++;
                                        }
                                    }
                                    Thread.Sleep(50);
                                }
                            }
                        }
                        while (activeThreadCount >= maxThreadCount)
                        {
                            // Clear out queue
                            while (resultQueue.TryDequeue(out result))
                            {
                                if (result.Contains("ERROR"))
                                {
                                    errorCount++;
                                }
                            }
                            Thread.Sleep(50);
                        }
                    }
                }
                connection.Close();
                MagLog.UpdateLogEntry("Complete", "Review: " + ReviewId + ", total: " + totalCount.ToString() +
                    ", errors: " + errorCount.ToString() + ", Threads: " + maxThreadCount.ToString(), MagLogId);
            }
        }

#endif


        }
    }
