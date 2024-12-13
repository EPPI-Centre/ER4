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
using System.Data;



#if !SILVERLIGHT
using System.Data.SqlClient;
using BusinessLibrary.Data;
using BusinessLibrary.Security;
#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class MarkdownItemsPdfCommand : LongLastingFireAndForgetCommand<MarkdownItemsPdfCommand>
    {
        public MarkdownItemsPdfCommand(){}

        private List<long> _ItemIds = new List<long>();
        private string _ItemIdsString = "";
        private int _RobotJobId;
        private string _result = "";
        private int _jobId = 0;
        private int _reviewId = 0;
        private int _ownerId = 0;
        private string _DFjobId = "";
        public List<MiniPdfDoc> DocsToProcess = new List<MiniPdfDoc>();
        public List<MiniPdfDoc> DocsToUpload = new List<MiniPdfDoc>();


        public int JobId
        {
            get { return _jobId; }
        }

        public string Result
        {
            get { return _result; }
        }
        public List<long> ItemIds
        {
            get
            {
                if (_ItemIdsString != "" && _ItemIds.Count == 0)
                {
                    string[] Ids = _ItemIdsString.Split(',');
                    foreach (string Id in Ids)
                    {
                        long val;
                        if (long.TryParse(Id, out val))
                        {
                            if (!_ItemIds.Contains(val)) _ItemIds.Add(val);
                        }
                    }
                }
                return _ItemIds;
            }
        }
        public MarkdownItemsPdfCommand(string ItemIdsString, int RobotJobId, int jobId = 0)
        {
            _RobotJobId = RobotJobId;
            _jobId = jobId;
            _ItemIdsString = ItemIdsString;
        }
        public MarkdownItemsPdfCommand(int reviewId, int ownerId, string ItemIdsString, int RobotJobId, int jobId = 0)
        {
            _reviewId = reviewId;
            _ownerId = ownerId;
            _RobotJobId = RobotJobId;
            _jobId = jobId;
            _ItemIdsString = ItemIdsString;
        }

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_ItemIdsString", _ItemIdsString);
            info.AddValue("_reviewId", _reviewId);
            info.AddValue("_ownerId", _ownerId); 
            info.AddValue("_RobotJobId", _RobotJobId); 
            info.AddValue("_jobId", _jobId);
            info.AddValue("_result", _result); 
        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            _ItemIdsString = info.GetValue<string>("_ItemIdsString");
            _reviewId = info.GetValue<int>("_reviewId");
            _ownerId = info.GetValue<int>("_ownerId");
            _RobotJobId = info.GetValue<int>("_RobotJobId");
            _jobId = info.GetValue<int>("_jobId");
            _result = info.GetValue<string>("_result"); 
        }


#if !SILVERLIGHT

        protected override void DataPortal_Execute()
        {
            //unlike most objects, this one is expected to be called by RobotOpenAiHostedService in a way that does make it hard to log and handle exceptions
            //for this reason, even in this method (which runs synchronously, until it calls ProcessFullList(...)) we try...catch,
            //if we created a record in TB_REVIEW_JOB, so to mark the job as failed therein

            //update the RobotJobId - mark it with status "Parsing PDFs"

            // IF JobId == 0 create Job record for this (sub) job, in TB_REVIEW_JOB
            // ELSE go and check the job progress
            //  IF job progress is "Cancelled during DF" and we have the AzureJob ID use new datafactoryhelper method to monitor when the job moves from queued/running to Succeeded or failed
            //  then update job as finished and exit (failure is logged in DF job, success isn't, as it may require more steps!)
            //GET list of ItemDocumentIds
            //check if app is shutting down
            //begin fire and forget return the ReviewJobId for the parent job to monitor
            //Per Doc, check if their markdown version is already in blob storage, keep the ID if not
            //check if app is shutting down
            //Per kept ID upload PDF to cloud
            //check if app is shutting down
            //start RunDataFactoryProcessV2 (returns a bool)
            //do we need to update the job record??
            if (_reviewId == 0 && Csla.ApplicationContext.User.Identity != null)
            {//precaution, we go and get it from the reviewer identity, in case this object will start getting called "directly"!
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                _reviewId = ri.ReviewId;
                _ownerId = ri.UserId;
            }


            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_UpdateRobotApiCallLog", connection))
                {//update the RobotJobId - mark it with status "Running"
                    string status = "Running";
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", _reviewId));
                    command.Parameters.Add(new SqlParameter("@ROBOT_API_CALL_ID", _RobotJobId));
                    command.Parameters.Add(new SqlParameter("@STATUS", status));
                    command.Parameters.Add(new SqlParameter("@CURRENT_ITEM_ID", SqlDbType.BigInt));
                    command.Parameters["@CURRENT_ITEM_ID"].Value = 0;
                    command.Parameters.Add(new SqlParameter("@INPUT_TOKENS_COUNT", 0));
                    command.Parameters.Add(new SqlParameter("@OUTPUT_TOKENS_COUNT", 0));
                    command.ExecuteNonQuery();
                }
                if (JobId == 0)
                {
                    //creates new review_job record if there isn't one already running
                    using (SqlCommand command = new SqlCommand("st_ParsePDFsCheckOngoingLog", connection))
                    {
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.Add("@revID", System.Data.SqlDbType.Int);
                        command.Parameters["@revID"].Value = _reviewId;
                        command.Parameters.Add(new SqlParameter("@CONTACT_ID", _ownerId));
                        //since we have a multi-workers queue for GPT batches, we need to allow concurrent "parse PDFs" jobs, even within the same review,
                        //hence: the @AllowConcurrent parameter is set to 1
                        command.Parameters.Add(new SqlParameter("@AllowConcurrent", 1));
                        command.Parameters.Add(new SqlParameter("@NewJobId", System.Data.SqlDbType.Int)); 
                        command.Parameters["@NewJobId"].Direction = System.Data.ParameterDirection.Output;
                        command.Parameters.Add("@RETURN_VALUE", System.Data.SqlDbType.Int);
                        command.Parameters["@RETURN_VALUE"].Direction = System.Data.ParameterDirection.ReturnValue;
                        command.ExecuteNonQuery();
                        string retVal = command.Parameters["@RETURN_VALUE"].Value.ToString();
                        if (retVal == "-1")
                        {
                            this._result = "Already Running";
                            return;
                        }
                        else if (retVal == "1")
                        {//either all good, or prev. attempt failed and we try again
                            this._result = "Starting...";
                            _jobId = (int)command.Parameters["@NewJobId"].Value;
                        }
                        else //we assume this will never happen, SP must have returned -4!
                        {
                            throw new DataPortalException("Unable to check if ParsingPDFs is running!" + Environment.NewLine
                                + "This indicates there is a problem with this review." + Environment.NewLine
                                + "Please contact EPPI Reviewer Support.", this);
                        }
                    }
                    StartFullProcessing(false);
                }
                else //jobId is known, implying we're resuming it
                {
                    //we need the DataFactory Job ID to go ask if it's finished there;
                    using (SqlCommand command = new SqlCommand("st_LogReviewJobGetById", connection))
                    {
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@ReviewId", _reviewId));
                        command.Parameters.Add(new SqlParameter("@JobId", JobId));
                        using (SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                        {
                            if (reader.Read())
                            {
                                string jobMessage = reader.GetString("JOB_MESSAGE");
                                string[] msgLines = jobMessage.Split(Environment.NewLine);
                                foreach (string line in msgLines)
                                {
                                    if (line.StartsWith("DF RunId: "))
                                    {
                                        _DFjobId = line.Substring(10);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    if (_DFjobId != "")
                    {
                        //call the resume fire and forget method
                        _result = "Starting...";
                        StartFullProcessing(true);
                    }
                    else
                    {
                        _result = "Failed: could not resume cancelled job";
                    }
                    return;
                }
            }
        }
        private void StartFullProcessing(bool isResuming) 
        {
            //if we're here, it's because we're starting a new job.
            //get all doc IDs
            try
            {
                using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand("st_GetItemDocumentIdsFromItemIds", connection))
                    {
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@ReviewId", _reviewId));
                        command.Parameters.Add(new SqlParameter("@ItemIds", _ItemIdsString));
                        command.Parameters.Add(new SqlParameter("@AlsoFetchFromLinkedItems", SqlDbType.Bit));
                        command.Parameters["@AlsoFetchFromLinkedItems"].Value = 0;//we ignore linked items/docs, for now
                        using (SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                        {
                            while (reader.Read())
                            {
                                if (reader.GetString("DOCUMENT_EXTENSION") == ".pdf")
                                {
                                    MiniPdfDoc toAdd = new MiniPdfDoc(reader);
                                    if (!DocsToProcess.Contains(toAdd)) DocsToProcess.Add(toAdd);
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                //we need to update tb_REVIEW_JOB here, as the calling code in RobotOpenAiHostedService can't receive the JobId if we 
                DataFactoryHelper.UpdateReviewJobLog(JobId, _reviewId, "Failed to get list of PDFs", "", "MarkdownItemsPdfCommand", true, false);
                _result = "Failed";
                throw;
            }

            if (DocsToProcess.Count > 0)
            {
                _result = "Starting...";
                //call full fire and forget method
                if (isResuming == false)
                {
                    Task.Run(() => ProcessFullList(_reviewId));
                }
                else
                {
                    //we're resuming and don't know what docs we uploaded in this run (some might have been marked down already)
                    //so we lie and claim that we uploaded ALL docs to process, which isn't always true
                    //but we can't know for sure, so we claim all docs were uploaded.
                    //Effect is that CleanupUploadedPdfs(...) will attempt to delete ALL the docs we might have uploaded, ensuring cleanup does happen.
                    DocsToUpload = new List<MiniPdfDoc>();
                    foreach (MiniPdfDoc doc in DocsToProcess) DocsToUpload.Add(doc);

                    Task.Run(() => MonitorRunningDFJobUntilFinishes(_reviewId));
                }
                return;
            }
            else
            {
                _result = "Finished";
                return;
            }
        }
        private async void ProcessFullList(int ReviewId)
        {
            string blobConnection = AzureSettings.blobConnection;
            string containerName = "eppi-reviewer-data";
            string FileNamePrefix = "eppi-rag-pdfs/" + DataFactoryHelper.NameBase + "ReviewId" + ReviewId + "/";
            
            int count = 0;
            try
            {

                foreach (MiniPdfDoc doc in DocsToProcess)
                {
                    if (!await BlobOperations.ThisBlobExistTask(blobConnection, containerName, FileNamePrefix + doc.MarkDownFileName))
                    {
                        DocsToUpload.Add(doc);
                        count++;
                        if (count == 10)
                        {
                            count = 0;
                            if (AppIsShuttingDown)
                            {
                                DataFactoryHelper.UpdateReviewJobLog(JobId, ReviewId, "Cancelled before upload", "", "MarkdownItemsPdfCommand", true, false);
                                _result = "Cancelled";
                                return;
                            }
                            else
                            {
                                DataFactoryHelper.UpdateReviewJobLog(JobId, ReviewId, "Checking PDFs to upload", "", "MarkdownItemsPdfCommand", false);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DataFactoryHelper.UpdateReviewJobLog(JobId, ReviewId, "Failed at Checking PDFs", "", "MarkdownItemsPdfCommand", true, false);
                DataFactoryHelper.LogExceptionToFile(ex, ReviewId, JobId, "MarkdownItemsPdfCommand");
                _result = "Failed";
                return;
            }
            count = 0;
            try
            {
                using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
                {
                    connection.Open();
                    foreach (MiniPdfDoc doc in DocsToUpload)
                    {                    
                        using (SqlCommand command = new SqlCommand("st_ItemDocumentBin", connection))
                        {
                            command.CommandType = CommandType.StoredProcedure;
                            command.Parameters.Add(new SqlParameter("@REV_ID", ReviewId));
                            command.Parameters.Add(new SqlParameter("@DOC_ID", doc.ItemDocumentId));
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    count++;
                                    BlobOperations.UploadStream(blobConnection, containerName, FileNamePrefix + doc.FileName, reader.GetStream("DOCUMENT_BINARY"));
                                    if (count == 10)
                                    {
                                        count = 0;
                                        if (AppIsShuttingDown)
                                        {
                                            DataFactoryHelper.UpdateReviewJobLog(JobId, ReviewId, "Cancelled during upload", "", "MarkdownItemsPdfCommand", true, false);
                                            _result = "Cancelled";
                                            return;
                                        }
                                        else
                                        {
                                            DataFactoryHelper.UpdateReviewJobLog(JobId, ReviewId, "Uploading", "", "MarkdownItemsPdfCommand", false);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DataFactoryHelper.UpdateReviewJobLog(JobId, ReviewId, "Failed at Uploading PDFs", "", "MarkdownItemsPdfCommand", true, false);
                DataFactoryHelper.LogExceptionToFile(ex, ReviewId, JobId, "MarkdownItemsPdfCommand");
                _result = "Failed";
                return;
            }
            bool DataFactoryRes = false;
            if (DocsToUpload.Count == 0)
            {//no need to spend ages waiting for DataFactory to figure it has nothing to do!
                DataFactoryRes = true;
            }
            else
            {
                try
                {
                    DataFactoryHelper DFH = new DataFactoryHelper();
                    string BatchGuid = Guid.NewGuid().ToString();
                    Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                        //pipeline_job_container_name='eppi-rag-pipeline',
                        //pipeline_job_pdf_files_path = 'pdf_files', doc_folder
                        //pipeline_job_parsed_pdf_path = 'parsed_pdfs',
                    { "do_parse_pdf", true },
                    {"doc_container", containerName },
                    {"doc_folder", FileNamePrefix.Substring(0, FileNamePrefix.Length - 1) },
                    {"doc_output_folder", FileNamePrefix.Substring(0, FileNamePrefix.Length - 1) },
                    {"EPPIReviewerApiRunId", BatchGuid}
                    //{"doc_folder", FileNamePrefix.Substring(0, FileNamePrefix.Length - 1) },
                    //{"doc_output_folder", FileNamePrefix.Substring(0, FileNamePrefix.Length - 1) },
                };
                    DataFactoryRes = await DFH.RunDataFactoryProcessV2("EPPI-Reviewer_API", parameters, ReviewId, _jobId, "MarkdownItemsPdfCommand", this.CancelToken);
                }
                catch (Exception ex)
                {
                    DataFactoryHelper.UpdateReviewJobLog(_jobId, ReviewId, "Failed to run DF", ex.Message, "MarkdownItemsPdfCommand", true, false);
                    DataFactoryHelper.LogExceptionToFile(ex, ReviewId, _jobId, "MarkdownItemsPdfCommand");
                    _result = "Failed";
                    return;
                }
            }
            if (DataFactoryRes == false && (AppIsShuttingDown || CancelToken.IsCancellationRequested))
            {//first check: did it fail because it was cancelled? If so, we'll stop before deleting the PDFs in blob storage (they might be needed!)
                _result = "Cancelled";
                return;
            }
            try
            {
                CleanupUploadedPdfs(blobConnection, containerName, FileNamePrefix, ReviewId);
            }
            catch (Exception ex)
            {
                DataFactoryHelper.UpdateReviewJobLog(_jobId, ReviewId, "Failed during cleanup", ex.Message, "MarkdownItemsPdfCommand", true, false);
                DataFactoryHelper.LogExceptionToFile(ex, ReviewId, _jobId, "MarkdownItemsPdfCommand");
                _result = "Done";
                return;
            }
            if (DataFactoryRes == false)
            {
                DataFactoryHelper.UpdateReviewJobLog(JobId, ReviewId, "Failed during DF", "", "MarkdownItemsPdfCommand", true, false);
                _result = "Failed";
                return;
            }
            if (_result != "Cancelled") //this can happen if we stopped during CleanupUploadedPdfs(...)
            { 
                DataFactoryHelper.UpdateReviewJobLog(_jobId, ReviewId, "Ended", "", "MarkdownItemsPdfCommand", true, true);
                _result = "Done";
            }
        }

        private async void MonitorRunningDFJobUntilFinishes(int ReviewId)
        {
            bool DataFactoryRes = false;
            try
            {
                
                DataFactoryHelper DFH = new DataFactoryHelper();
                DataFactoryRes = await DFH.ResumeDataFactoryProcessV2("EPPI-Reviewer_API", _DFjobId, ReviewId, _jobId, "", this.CancelToken);
                if (DataFactoryRes == false)
                {//no need to log: DFH does that already
                    if (AppIsShuttingDown || CancelToken.IsCancellationRequested)
                    {
                        _result = "Cancelled";
                        return;
                    }
                    else
                    {
                        _result = "Failed";
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                DataFactoryHelper.UpdateReviewJobLog(_jobId, ReviewId, "Failed at resuming/monitoring DF job", ex.Message, "MarkdownItemsPdfCommand", true, false);
                DataFactoryHelper.LogExceptionToFile(ex, ReviewId, _jobId, "MarkdownItemsPdfCommand");
                _result = "Failed";
                return;
            }
            try
            {
                string blobConnection = AzureSettings.blobConnection;
                string containerName = "eppi-reviewer-data";
                string FileNamePrefix = "eppi-rag-pdfs/" + DataFactoryHelper.NameBase + "ReviewId" + ReviewId + "/";
                CleanupUploadedPdfs(blobConnection, containerName, FileNamePrefix, ReviewId);
            }
            catch (Exception ex)
            {
                //we mark the review_job as failed as it did fail somehow, although the main task (marking down) has worked (if we got here!)
                DataFactoryHelper.UpdateReviewJobLog(_jobId, ReviewId, "Error at cleanup after resuming", ex.Message, "MarkdownItemsPdfCommand", true, false);
                DataFactoryHelper.LogExceptionToFile(ex, ReviewId, _jobId, "MarkdownItemsPdfCommand");
                //we let RobotOpenAiHostedService believe a
                _result = "Done";
                return;
            }
            DataFactoryHelper.UpdateReviewJobLog(_jobId, ReviewId, "Ended", "", "MarkdownItemsPdfCommand", true, true);
            _result = "Done";               
        }
        private void CleanupUploadedPdfs(string blobConnection, string containerName, string FileNamePrefix, int ReviewId)
        {
            int count = 0;
            foreach (MiniPdfDoc doc in DocsToUpload)
            {
                BlobOperations.DeleteIfExists(blobConnection, containerName, FileNamePrefix + doc.FileName);
                if (count == 50)
                {
                    count = 0;
                    if (AppIsShuttingDown)
                    {
                        DataFactoryHelper.UpdateReviewJobLog(JobId, ReviewId, "Cancelled during cleanup", "", "MarkdownItemsPdfCommand", true, false);
                        _result = "Cancelled";
                        return;
                    }
                    else
                    {
                        DataFactoryHelper.UpdateReviewJobLog(JobId, ReviewId, "Cleanup", "", "MarkdownItemsPdfCommand", false);
                    }
                }
            }
        }
        public class MiniPdfDoc
        {
            public long ItemDocumentId { get; private set; } = 0;
            public long ItemId { get; private set; } = 0;


            public string FileName
            {
                get
                {
                    return ItemId.ToString() + "-" + ItemDocumentId.ToString() + ".pdf";
                }
            }
            public string MarkDownFileName
            {
                get
                {
                    return ItemId.ToString() + "-" + ItemDocumentId.ToString() + ".md";
                }
            }
            public MiniPdfDoc(SafeDataReader reader)
            {
                ItemDocumentId = reader.GetInt64("ITEM_DOCUMENT_ID");
                ItemId = reader.GetInt64("ITEM_ID");
            }
            public MiniPdfDoc(long itemId, long itemDocId)
            {
                ItemDocumentId = itemDocId;
                ItemId = itemId;
            }
        }
#endif
    }
}
