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
using System.Security.Cryptography;
using Csla.Data;




#if !SILVERLIGHT
using System.Data.SqlClient;
using BusinessLibrary.Data;
using BusinessLibrary.Security;
#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class SourceDeleteForeverCommand : LongLastingFireAndForgetCommand<SourceDeleteForeverCommand>, iResumableLongLastingTask
    {
    public SourceDeleteForeverCommand(){}

        public SourceDeleteForeverCommand(int SourceId)
        {
            _SourceId = SourceId;
        }
        private int _SourceId;
        public int SourceId
        {
            get { return _SourceId; }
        }

        private string _Result = "";
        public string Result
        {
            get { return _Result; }
        }

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_SourceId", _SourceId);
            info.AddValue("_Result", _Result);

        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            _SourceId = info.GetValue<int>("_SourceId");
            _Result = info.GetValue<string>("_Result");
        }

#if !SILVERLIGHT

        protected override void DataPortal_Execute()
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            int Rid = ri.ReviewId;
            int JobId;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("sp_SourceDeleteForever_start", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", Rid));
                    command.Parameters.Add(new SqlParameter("@source_ID", _SourceId));
                    command.Parameters.Add(new SqlParameter("@contactID", ri.UserId));
                    command.Parameters.Add(new SqlParameter("@Result", System.Data.SqlDbType.Int));
                    command.Parameters["@Result"].Direction = System.Data.ParameterDirection.Output;
                    command.ExecuteNonQuery();
                    int? res = command.Parameters["@Result"].Value as int?;
                    if (res == null || res < 1)
                    {//didn't work: either we got -1 (another deletion is running or should be resumed) or calling the SP failed
                        _Result = "Task is already running for a different source.";
                        return;
                    }
                    else
                    {
                        JobId = (int)res;
                    }
                }
            }
            System.Threading.Tasks.Task.Run(() => FireAndForgetExcecuteCommand(Rid, ri.UserId, JobId));//fire and forget. We don't wait to see what happens.
            _Result = "Deletion running for SourceId: " + _SourceId.ToString();
            //if (SourceId > 0)
            //{
            //    //we triggered this to actually delete a source (other option is to make sure an interrupted source deletion gets a chance to resume, if necessary)
            //    //so we'll wait a bit to see if the deletion ends in 30s
            //    for (int count = 0; count < 3; count++)
            //    {
            //        System.Threading.Thread.Sleep(10 * 1000);//wait 10s
            //        using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            //        {
            //            connection.Open();
            //            using (SqlCommand command = new SqlCommand("st_SourceDeleteForeverIsRunning", connection))
            //            {
            //                command.CommandType = System.Data.CommandType.StoredProcedure;
            //                command.Parameters.Add(new SqlParameter("@revID", Rid));
            //                command.Parameters.Add(new SqlParameter("@result", System.Data.SqlDbType.Int));
            //                command.Parameters["@result"].Direction = System.Data.ParameterDirection.Output;
            //                command.ExecuteNonQuery();
            //                int? res = command.Parameters["@result"].Value as int?;

            //                if (res != null && res != 0)
            //                {
            //                    if (res != SourceId)
            //                    {//a deletion is running for another source, which can always happen because of concurrent usage
            //                        _Result = "Deletion is  already running for a different source (id: " + res.ToString() + ")";
            //                        //we can stop checking, current source isn't going to be deleted!
            //                        count = 100;
            //                    }
            //                    else _Result = "Deletion running for SourceId: " + res.ToString();
            //                }
            //                else
            //                {//must have finished already!
            //                    _Result = "No deletion is running";
            //                    count = 100;//end the loop!
            //                }
            //            }
            //            connection.Close();
            //        }
            //    }
            //}
            //else _Result = "Task fired and forgotten - not checking if a deletion is running";
        }
        private async void FireAndForgetExcecuteCommand(int revID, int ContactId, int JobId)
        {
            try 
            {
                using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand("st_SourceGetAllDocsIDs", connection))
                    {
                        //need to at least try to delete docs from blobs
                        //if docs are deduped, then we should get ONLY the docs that belong only to this source
                        Dictionary<long, string> DocsToDelete = new Dictionary<long, string>();
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@source_ID", _SourceId));
                        command.Parameters.Add(new SqlParameter("@REVIEW_ID", revID));
                        using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                        {
                            while (reader.Read())
                            {
                                long id = reader.GetInt64("ITEM_DOCUMENT_ID");
                                if (!DocsToDelete.ContainsKey(id))
                                {
                                    DocsToDelete.Add(id, reader.GetString("DOCUMENT_EXTENSION"));
                                }
                            }
                        }
                        if (AppIsShuttingDown)
                        {
                            CancelJobWithStatusMessage(JobId, revID, "Paused (at deleting docs) while ER restarts");
                        }
                        if (DocsToDelete.Count > 0)
                        {
                            bool docsdeleted = DeleteDocsFromBlob(DocsToDelete, JobId, revID);//we'll do this first!
                            if (docsdeleted == false)
                            {
                                return;
                            }
                        }
                    }
                    int? result = 1;
                    int loopCount = 0;
                    DateTime start;
                    int batchSize = 50; //nice and small, so we'll call the "delete in batches" SP many times, but be quick about it each time
                    //and we wait between batches!
                    while (result > 0 && loopCount < 1000000)//we put a hard limit on the number of repeats, for safety...
                    {//this will break if we try to delete a source with 50*1M records! should be safe enough!
                        start = DateTime.Now;
                        using (SqlCommand command = new SqlCommand("st_SourceDeleteForeverInBatches", connection))
                        {
                            //st_SourceDeleteForeverInBatches is not fast.
                            //this is because all deletions include multiple tables and are wrapped in a transaction (no partial deletions are possible)
                            //consequence is that SP "locks" lots of tables, thus, between batches we stop to let other queries execute.
                            //Deleting rows from TB_ITEM is slow because of all "CASCADE on delete" foreign keys on ITEM_ID.
                            command.CommandType = System.Data.CommandType.StoredProcedure;
                            command.Parameters.Add(new SqlParameter("@srcID", _SourceId));
                            command.Parameters.Add(new SqlParameter("@revID", revID));
                            command.Parameters.Add(new SqlParameter("@contactID", ContactId));
                            command.Parameters.Add(new SqlParameter("@JobId", JobId));
                            command.Parameters.Add(new SqlParameter("@batchSize", batchSize));
                            command.Parameters.Add(new SqlParameter("@result", System.Data.SqlDbType.Int));
                            command.Parameters["@result"].Direction = System.Data.ParameterDirection.Output;
                            command.CommandTimeout = 60;//1 min
                            command.ExecuteNonQuery();
                            
                            //Debug: put a breakpoint here to "see" the result value
                            result = command.Parameters["@result"].Value as int?;
                            if (result == null)
                            {//don't know what to do, should not happen!!
                                break;
                            }
                        }
                        if (AppIsShuttingDown)
                        {
                            CancelJobWithStatusMessage(JobId, revID, "Paused (at deleting DB records) while ER restarts");
                            break;
                        }
                        if (result > 0)
                        {//we'll loop again so we wait a bit, to give the DB time to recover from locking many tables
                            TimeSpan time = DateTime.Now - start;
                            try
                            { //we wait for twice as long as it took to delete a single batch
                                time = time * 2;
                                await Task.Delay(time, CancelToken);
                            }
                            catch//if we get to cancel the delay, it triggers an exception!
                            {
                                if (AppIsShuttingDown)
                                {
                                    CancelJobWithStatusMessage(JobId, revID, "Paused (at deleting DB records) while ER restarts");
                                }
                            }
                        }
                        loopCount++;
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                MarkJobAsFailed(JobId, revID, "Failed at FireAndForget stage", ex);
            }
        }
        private void CancelJobWithStatusMessage(int JobId, int RevId, string message)
        {
            DataFactoryHelper.UpdateReviewJobLog(JobId, RevId, "Cancelled", message, "SourceDeleteForever");//resume information is already in the record
        }
        private void MarkJobAsRunning(int JobId, int RevId)
        {
            DataFactoryHelper.UpdateReviewJobLog(JobId, RevId, "running", "", "SourceDeleteForever");//resume information is already in the record
        }
        private void MarkJobAsFailed(int JobId, int RevId, string message, Exception ex)
        {
            DataFactoryHelper.LogExceptionToFile(ex, RevId, JobId, "SourceDeleteForever");
            DataFactoryHelper.UpdateReviewJobLog(JobId, RevId, "Failed", message, "SourceDeleteForever", true, false);//resume information is already in the record
        }
        private bool DeleteDocsFromBlob(Dictionary<long, string> DocsToDelete, int JobId, int revID)
        {
            try
            {
                foreach(KeyValuePair<long, string> kvp in DocsToDelete)
                {
                    if (kvp.Value != ".txt" && kvp.Value != "")
                    {
                        string BlobFilename = ItemDocument.DocBlobFileName(kvp.Key, kvp.Value);
                        BlobOperations.DeleteIfExists(AzureSettings.blobConnection, AzureSettings.FullTextDocsBlobContainer, BlobFilename);
                    }
                    if (AppIsShuttingDown)
                    {
                        CancelJobWithStatusMessage(JobId, revID, "Paused (at deleting docs) while ER restarts");
                    }
                }
            }
            catch(Exception ex)
            {
                MarkJobAsFailed(JobId, revID, "Failed at DeleteDocsFromBlob stage", ex);
                return false;
            }
            return true;
        }
#if !ER4
        public void ResumeJob(ER_Web.Services.RawTaskToResume rttr) 
        {
            try
            {
                string IdString = rttr.ParamsInJson.Replace("SourceId: ", "");
                int recoveredId;
                if (int.TryParse(IdString, out recoveredId)) this._SourceId = recoveredId;
                else
                {
                    DataFactoryHelper.UpdateReviewJobLog(rttr.JobId, rttr.ReviewId, "Failed", "Failed to resume task - source ID is missing"
                        , "SourceDeleteForever", true, false);
                    return;
                }
                MarkJobAsRunning(rttr.JobId, rttr.ReviewId);
                System.Threading.Tasks.Task.Run(() => FireAndForgetExcecuteCommand(rttr.ReviewId, rttr.ContactId, rttr.JobId));
            }
            catch (Exception ex)
            {
                DataFactoryHelper.UpdateReviewJobLog(rttr.JobId, rttr.ReviewId, "Failed", "Failed to resume task", "SourceDeleteForever", true, false);
                DataFactoryHelper.LogExceptionToFile(ex, rttr.ReviewId, rttr.JobId, "SourceDeleteForever");
                return;
            }
        }
#endif
#endif
    }
}
