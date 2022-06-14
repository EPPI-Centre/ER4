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
#if !CSLA_NETCORE
using System.Web.Hosting;
#endif
#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class MagMatchItemsToPapersCommand : CommandBase<MagMatchItemsToPapersCommand>
    {

#if SILVERLIGHT
    public MagMatchItemsToPapersCommand(){}
#else
        public MagMatchItemsToPapersCommand() { }
#endif

        private bool _AllInReview;
        private Int64 _ITEM_ID;
        private Int64 _ATTRIBUTE_ID;
        private string _FindOrRemove;
        private string _currentStatus;
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
                    int MagLogId = MagLog.SaveLogEntry("MAG matching", "Starting", "Review: " + ri.ReviewId +
                        ", Threads: " + AzureSettings.MagMatchItemsMaxThreadCount, ri.UserId);

#if CSLA_NETCORE
            System.Threading.Tasks.Task.Run(() => doMatchItems(ri.ReviewId, MagLogId, Convert.ToInt32(AzureSettings.MagMatchItemsMaxThreadCount)));
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
                        while (reader.Read())
                        {
                            if (cancellationToken.IsCancellationRequested)
                            {
                                MagLog.UpdateLogEntry("CancelToken!!", "Review: " + ReviewId + ", totalDone: " + totalCount.ToString() +
                                    ", errors: " + errorCount.ToString() + ", Threads: " + maxThreadCount.ToString(), MagLogId);
                                return;
                            }
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
                                    catch
                                    {
                                        resultQueue.Enqueue("ERROR: " + ItemId);
                                    }
                                    finally
                                    {
                                        Interlocked.Decrement(ref activeThreadCount);
                                    }
                                });

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

        /*
        private SqlCommand SpecifyCommand(SqlConnection connection, ReviewerIdentity ri)
        {
            SqlCommand command = null;
            switch (_FindOrRemove)
            {
                case "FindMatches":
                    if (_AllInReview == true)
                    {
                        //command = new SqlCommand("st_MatchItemsToPapers", connection); - now doing via worker process
                        command = new SqlCommand("st_MatchItemsToPapersAddJob", connection);
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                    }
                    else if (_ATTRIBUTE_ID != 0)
                    {   // this is currently unused. Should probably add a job rather than run immediately
                        command = new SqlCommand("st_MatchItemsToPapersWithAttribute", connection);
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ID", _ATTRIBUTE_ID));
                    }
                    else if (_ITEM_ID != 0)
                    {
                        command = new SqlCommand("st_MatchItemsToPapersSingleItem", connection);
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@ITEM_ID", _ITEM_ID));
                    }
                    break;
                case "RemoveMatches":
                    if (_AllInReview == true)
                    {
                        command = new SqlCommand("st_MagMatchItemsRemove", connection);
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                    }
                    else if (_ATTRIBUTE_ID != 0)
                    {
                        command = new SqlCommand("st_MagMatchItemsRemove", connection);
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ID", _ATTRIBUTE_ID));
                    }
                    else if (_ITEM_ID != 0)
                    {
                        command = new SqlCommand("st_MagMatchItemsRemove", connection);
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@ITEM_ID", _ITEM_ID));
                    }
                    break;
            }
            return command;
        }
        */

#endif


    }
}
