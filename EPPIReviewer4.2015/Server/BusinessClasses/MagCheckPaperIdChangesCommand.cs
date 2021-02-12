using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Csla;

using Csla.Serialization;
using Csla.Silverlight;
using System.ComponentModel;
using Csla.DataPortalClient;
using System.Threading;
using System.Configuration;

#if !SILVERLIGHT
using BusinessLibrary.Data;
using BusinessLibrary.Security;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Data.SqlClient;
using System.Data;
using System.Collections.Concurrent;
#if !CSLA_NETCORE
using System.Web.Hosting;
#endif
#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class MagCheckPaperIdChangesCommand : CommandBase<MagCheckPaperIdChangesCommand>
    {

#if SILVERLIGHT
    public MagCheckPaperIdChangesCommand(){}
#else
        public MagCheckPaperIdChangesCommand() { }
#endif

        private string _LatestMAGName;


        [Newtonsoft.Json.JsonProperty]
        
        public string LatestMAGName
        {
            get
            {
                return _LatestMAGName;
            }
        }

        public MagCheckPaperIdChangesCommand(string LatestMag)
        {
            _LatestMAGName = LatestMag;
        }

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_LatestMAGName", _LatestMAGName);
        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            _LatestMAGName = info.GetValue<string>("_LatestMAGName");
        }


#if !SILVERLIGHT

        const string TempPath = @"UserTempUploads/";

        protected override void DataPortal_Execute()
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            int ReviewId = ri.ReviewId; // we don't use this, but it checks we have a valid ticket
            int ContactId = ri.UserId; // putting to variable in case the user invalidates ticket
            // spin the task off into another thread and return

#if CSLA_NETCORE
            System.Threading.Tasks.Task.Run(() => RunCheckUpdates(ReviewId, ContactId));
#else
            //see: https://codingcanvas.com/using-hostingenvironment-queuebackgroundworkitem-to-run-background-tasks-in-asp-net/
            HostingEnvironment.QueueBackgroundWorkItem(cancellationToken => RunCheckUpdates(ReviewId, ContactId));
#endif
        }

        private async Task RunCheckUpdates(int ReviewId, int ContactId, CancellationToken cancellationToken = default(CancellationToken))
        {
            int currentlyUsed = 0;
            string uploadFileName = "";

            int TaskMagLogId = MagLog.SaveLogEntry("Checking ID changes", "Starting", "", ContactId);
            int missingCount = ChangedPapersNotAlreadyWrittenCheck();
            if (missingCount == -1)
            {
                MagLog.UpdateLogEntry("Stopped", "Error in missingCount", TaskMagLogId);
                return;
            }
            if (missingCount == 0) // if greater than 0, we've already downloaded the file and need to complete the auto-matching
            {

#if (!CSLA_NETCORE)
                uploadFileName = System.Web.HttpRuntime.AppDomainAppPath + TempPath + "CheckPaperIdChanges.csv";
#else
            uploadFileName = TempPath + "CheckPaperIdChanges.csv";

            if (Directory.Exists(TempPath))
            {
                uploadFileName = @"./"+ TempPath + "/" + "CheckPaperIdChanges.csv";
            }
            else
            {
                DirectoryInfo tmpDir = System.IO.Directory.CreateDirectory(TempPath);
                uploadFileName = tmpDir.FullName + "/" + @"./" + TempPath + "/" + "CheckPaperIdChanges.csv";
            }
#endif


                currentlyUsed = WriteCurrentlyUsedPaperIdsToFile(uploadFileName);
                if (currentlyUsed == -1)
                {
                    MagLog.UpdateLogEntry("Stopped", "Error in currentlyUsed", TaskMagLogId);
                    return;
                }
                if (!await UploadIdsFile(uploadFileName))
                {
                    MagLog.UpdateLogEntry("Stopped", "Error in Uploading ids file", TaskMagLogId);
                    return;
                }
                MagLog.UpdateLogEntry("Running", "ID checking: file uploaded n=" + currentlyUsed.ToString(), TaskMagLogId);
                if (!SubmitJob(ContactId, cancellationToken))
                {
                    MagLog.UpdateLogEntry("Stopped", "Error in Uploading ids file", TaskMagLogId);
                    return;
                }
                missingCount = await DownloadMissingIdsFile(uploadFileName);
                if (missingCount == -1)
                {
                    MagLog.UpdateLogEntry("Stopped", "Error in missingCount", TaskMagLogId);
                    return;
                }
            }

            MagLog.UpdateLogEntry("Running", "ID checking: Currently used: " + currentlyUsed.ToString() + " changed: " + missingCount.ToString(),
                TaskMagLogId);
            string lookupResults = LookupMissingIdsInNewMakes(ContactId, TaskMagLogId, missingCount, currentlyUsed);
            if (lookupResults == "ERROR: LookupMissingIdsInNewMakes did not complete")
            {
                MagLog.UpdateLogEntry("Stopped", "Error in looking up missing ids", TaskMagLogId);
                return;
            }
            MagLog.UpdateLogEntry("Running", "ID checking: updating live IDs. (Lookup results: " + lookupResults + ")", TaskMagLogId);
            if (!UpdateLiveIds())
            {
                MagLog.UpdateLogEntry("Stopped", "Error in looking up updating live ids", TaskMagLogId);
                return;
            }
            MagLog.UpdateLogEntry("Complete", "ID checking: " + lookupResults, TaskMagLogId);
        }

        private int ChangedPapersNotAlreadyWrittenCheck()
        {
            try
            {
                int retVal = 0;
                using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand("st_MagChangedPapersNotAlreadyWrittenCheck", connection))
                    {
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@MagVersion", _LatestMAGName));
                        command.Parameters.Add(new SqlParameter("@NumPapers", 0));
                        command.Parameters["@NumPapers"].Direction = System.Data.ParameterDirection.Output;
                        command.ExecuteNonQuery();
                        retVal = Convert.ToInt32(command.Parameters["@NumPapers"].Value);
                    }
                    connection.Close();
                }
                return retVal;
            }
            catch
            {
                return -1;
            }
        }

        private int WriteCurrentlyUsedPaperIdsToFile(string fileName)
        {
            int count = 0;
            try
            {
                using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand("st_MagGetCurrentlyUsedPaperIds", connection))
                    {
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@REVIEW_ID", 0));
                        using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                        {
                            using (StreamWriter file = new StreamWriter(fileName, false))
                            {
                                while (reader.Read())
                                {
                                    file.WriteLine(reader["PaperId"].ToString() + "\t" + reader["ITEM_ID"].ToString());
                                    count++;
                                }
                            }
                        }
                    }
                }
                return count;
            }
            catch
            {
                return -1;
            }
        }

        private async Task<bool> UploadIdsFile(string fileName)
        {

#if (CSLA_NETCORE)

            var configuration = ERxWebClient2.Startup.Configuration.GetSection("AzureMagSettings");
            string storageAccountName = configuration["MAGStorageAccount"];
            string storageAccountKey = configuration["MAGStorageAccountKey"];

#else
            string storageAccountName = ConfigurationManager.AppSettings["MAGStorageAccount"];
            string storageAccountKey = ConfigurationManager.AppSettings["MAGStorageAccountKey"];
#endif

            string storageConnectionString =
                "DefaultEndpointsProtocol=https;AccountName=" + storageAccountName + ";AccountKey=";
            storageConnectionString += storageAccountKey;

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("uploads");
            CloudBlockBlob blockBlobData;

            try
            {
                blockBlobData = container.GetBlockBlobReference(Path.GetFileName(fileName));
                using (var fileStream = System.IO.File.OpenRead(fileName))
                {


#if (!CSLA_NETCORE)
                    blockBlobData.UploadFromStream(fileStream);
#else

				await blockBlobData.UploadFromFileAsync(fileName);


#endif

                }
                File.Delete(fileName);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool SubmitJob(int ContactId, CancellationToken cancellationToken)
        {
            try
            {
                MagDataLakeHelpers.ExecProc(@"[master].[dbo].[GetPaperIdChanges](""" + this.LatestMAGName + "\");", true,
                    "GetPaperIdChanges", ContactId, 9, cancellationToken);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private async Task<int> DownloadMissingIdsFile(string uploadFileName)
        {

#if (CSLA_NETCORE)

            var configuration = ERxWebClient2.Startup.Configuration.GetSection("AzureMagSettings");
            string storageAccountName = configuration["MAGStorageAccount"];
            string storageAccountKey = configuration["MAGStorageAccountKey"];

#else
            string storageAccountName = ConfigurationManager.AppSettings["MAGStorageAccount"];
            string storageAccountKey = ConfigurationManager.AppSettings["MAGStorageAccountKey"];
#endif

            //string storageAccountName = ConfigurationManager.AppSettings["MAGStorageAccount"];
            //string storageAccountKey = ConfigurationManager.AppSettings["MAGStorageAccountKey"];

            string storageConnectionString =
                "DefaultEndpointsProtocol=https;AccountName=" + storageAccountName + ";AccountKey=";
            storageConnectionString += storageAccountKey;

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer containerUp = blobClient.GetContainerReference("uploads");
            CloudBlobContainer containerDown = blobClient.GetContainerReference("results");

            // cleaning up the file that was uploaded
            CloudBlockBlob blockBlobUploadData = containerUp.GetBlockBlobReference(Path.GetFileName(uploadFileName));


#if (!CSLA_NETCORE)
                blockBlobUploadData.DeleteIfExists();
#else

                await blockBlobUploadData.DeleteIfExistsAsync();
#endif



            CloudBlockBlob blockBlobDownloadData = containerDown.GetBlockBlobReference(Path.GetFileName("CheckPaperIdChangesResults.tsv"));

#if (!CSLA_NETCORE)
             byte[] myFile = Encoding.UTF8.GetBytes(blockBlobDownloadData.DownloadText());
#else
            string resultantString = await blockBlobDownloadData.DownloadTextAsync();
            byte[] myFile = Encoding.UTF8.GetBytes(resultantString);

#endif
            MemoryStream ms = new MemoryStream(myFile);

            DataTable dt = new DataTable("Ids");
            dt.Columns.Add("Identity");
            dt.Columns.Add("OldPaperId");
            dt.Columns.Add("LatestMag");
            dt.Columns.Add("NewPaperId");
            dt.Columns.Add("ITEM_ID");
            dt.Columns.Add("NewAutoMatchScore");

            int lineCount = 0;
            using (var reader = new StreamReader(ms))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string OldPaperId = line.Split('\t')[0];
                    string ITEM_ID = line.Split('\t')[1];
                    DataRow newRow = dt.NewRow();
                    newRow["Identity"] = 0; // doesn't matter what is in here - it's auto-assigned in the database
                    newRow["OldPaperId"] = OldPaperId;
                    newRow["LatestMag"] = LatestMAGName;
                    newRow["NewPaperId"] = -1;
                    newRow["ITEM_ID"] = ITEM_ID;
                    newRow["NewAutoMatchScore"] = 0;
                    dt.Rows.Add(newRow);
                    lineCount++;
                }
            }

            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlBulkCopy sbc = new SqlBulkCopy(connection))
                {
                    sbc.DestinationTableName = "TB_MAG_CHANGED_PAPER_IDS";
                    sbc.BatchSize = 1000;
                    sbc.WriteToServer(dt);
                }
                connection.Close();
            }

#if (!CSLA_NETCORE)
                blockBlobDownloadData.DeleteIfExists();
#else

            await blockBlobDownloadData.DeleteIfExistsAsync();
#endif


            return lineCount;
        }

        private string LookupMissingIdsInNewMakes(int ContactId, int MagLogId, int missingCount, int currentlyUsed)
        {
#if (CSLA_NETCORE)

            var configuration = ERxWebClient2.Startup.Configuration.GetSection("AzureContReviewSettings");

#else
            var configuration = ConfigurationManager.AppSettings;

#endif
            int PaperTotalCount = 0;
            int PaperTotalFound = 0;
            int activeThreadCount = 0;
            int notMatchedCount = 0;
            int maxThreadCount = Convert.ToInt32(configuration["MagMatchItemsMaxThreadCount"]);
            int errorCount = 0;
            string result;
            string returnString = "ERROR: LookupMissingIdsInNewMakes did not complete";
            ConcurrentQueue<string> resultQueue = new ConcurrentQueue<string>();
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_MagGetMissingPaperIds", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@MagVersion", LatestMAGName));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        while (reader.Read())
                        {
                            PaperTotalCount++;
                            Int64 SeekingPaperId = Convert.ToInt64(reader["OldPaperId"].ToString());
                            int rowId = Convert.ToInt32(reader["MagChangedPaperIdsId"].ToString());
                            Int64 SeekingITEM_ID = Convert.ToInt64(reader["ITEM_ID"].ToString());

                            if (activeThreadCount < maxThreadCount)
                            {
                                Interlocked.Increment(ref activeThreadCount);
                                Task.Run(() =>
                                {
                                    string res = "";
                                    try
                                    {
                                        res = LookupOneItem(SeekingPaperId, rowId, SeekingITEM_ID);
                                        resultQueue.Enqueue(res);
                                    }
                                    catch
                                    {
                                        resultQueue.Enqueue(rowId.ToString() + ",ERROR");                                      
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
                                        else
                                        {
                                            if (result.Contains("NOT_MATCHED"))
                                            {
                                                notMatchedCount++;
                                            }
                                            else
                                            {
                                                PaperTotalFound++;
                                                string[] resultParsed = result.Split(',');
                                                SaveMatch(result, PaperTotalCount, PaperTotalFound, missingCount,
                                                    MagLogId, currentlyUsed, errorCount);
                                            }
                                        }
                                    }
                                    Thread.Sleep(50);
                                }
                            }
                        }
                       
                        while (activeThreadCount > 0)
                        {
                            // Clear out queue
                            while (resultQueue.TryDequeue(out result))
                            {
                                if (result.Contains("ERROR"))
                                {
                                    errorCount++;
                                }
                                else
                                {
                                    if (result.Contains("NOT_MATCHED"))
                                    {
                                        notMatchedCount++;
                                    }
                                    else
                                    { 
                                        PaperTotalFound++;
                                        string[] resultParsed = result.Split(',');
                                        SaveMatch(result, PaperTotalCount, PaperTotalFound, missingCount,
                                            MagLogId, currentlyUsed, errorCount);
                                    }
                                }
                            }
                            Thread.Sleep(50);
                        }
                    }
                }
                connection.Close();

                returnString = "Currently used: " + currentlyUsed.ToString() + " changed: " + missingCount.ToString() +
                    " processed: " + PaperTotalCount.ToString() + " updated: " +
                    PaperTotalFound.ToString() + " errors: " + errorCount.ToString();
            }
            return returnString;
        }

        private void SaveMatch(string res, int PaperTotalCount, int PaperTotalFound, int missingCount, int MagLogId,
            int currentlyUsed, int errorCount)
        {
            string[] resultParsed = res.Split(',');
            using (SqlConnection connection2 = new SqlConnection(DataConnection.ConnectionString))
            {
                connection2.Open();
                using (SqlCommand command2 = new SqlCommand("st_MagUpdateMissingPaperIds", connection2))
                {
                    command2.CommandType = CommandType.StoredProcedure;
                    command2.Parameters.Add(new SqlParameter("@MagChangedPaperIdsId", resultParsed[0]));
                    command2.Parameters.Add(new SqlParameter("@NewPaperId", resultParsed[2]));
                    command2.Parameters.Add(new SqlParameter("@NewAutoMatchScore", resultParsed[1]));
                    command2.ExecuteNonQuery();
                }
                connection2.Close();
                MagLog.UpdateLogEntry("Running", "ID checking: Currently used: " + currentlyUsed.ToString() + " changed: " + missingCount.ToString() +
                    " processed: " + PaperTotalCount.ToString() + " updated: " +
                    PaperTotalFound.ToString() + " errors: " + errorCount.ToString(), MagLogId);
            }
        }

        private string LookupOneItem(Int64 SeekingPaperId, int rowId, Int64 SeekingITEM_ID)
        {
            // MagPaperItemMatch has similar code to the below. They should keep in sync!
            // MATCHING STEP 1: try to match on the current MAKES deployment
            
                bool matchedOnMAKES = false;
                MagMakesHelpers.PaperMakes pm = MagMakesHelpers.GetPaperMakesFromMakes(SeekingPaperId);
                if (pm != null)
                {
                    List<MagMakesHelpers.PaperMakes> candidatePapersOnDOI = MagMakesHelpers.GetCandidateMatchesOnDOI(pm.DOI, "PENDING");
                    if (candidatePapersOnDOI != null && candidatePapersOnDOI.Count > 0)
                    {
                        foreach (MagMakesHelpers.PaperMakes cpm in candidatePapersOnDOI)
                        {
                            MagPaperItemMatch.doMakesPapersComparison(pm, cpm);
                        }
                    }
                    if (candidatePapersOnDOI.Count == 0 || (candidatePapersOnDOI.Max(t => t.matchingScore) < MagPaperItemMatch.AutoMatchThreshold))
                    {
                        List<MagMakesHelpers.PaperMakes> candidatePapersOnTitle = MagMakesHelpers.GetCandidateMatches(pm.DN, "PENDING");
                        if (candidatePapersOnTitle != null && candidatePapersOnTitle.Count > 0)
                        {
                            foreach (MagMakesHelpers.PaperMakes cpm in candidatePapersOnTitle)
                            {
                                MagPaperItemMatch.doMakesPapersComparison(pm, cpm);
                            }
                            foreach (MagMakesHelpers.PaperMakes cpm in candidatePapersOnTitle)
                            {
                                var found = candidatePapersOnDOI.Find(e => e.Id == cpm.Id);
                                if (found == null && cpm.matchingScore >= MagPaperItemMatch.AutoMatchThreshold)
                                {
                                    candidatePapersOnDOI.Add(cpm);
                                }
                            }
                            // add in matching on journals / authors if we don't have an exact match on title
                            if (candidatePapersOnTitle.Count == 0 || (candidatePapersOnTitle.Max(t => t.matchingScore) < MagPaperItemMatch.AutoMatchThreshold))
                            {
                                List<MagMakesHelpers.PaperMakes> candidatePapersOnAuthorJournal =
                                    MagMakesHelpers.GetCandidateMatches(MagMakesHelpers.getAuthors(pm.AA) + " " + (pm.J != null ? pm.J.JN : ""));
                                foreach (MagMakesHelpers.PaperMakes cpm in candidatePapersOnAuthorJournal)
                                {
                                    MagPaperItemMatch.doMakesPapersComparison(pm, cpm);
                                }
                                foreach (MagMakesHelpers.PaperMakes cpm in candidatePapersOnAuthorJournal)
                                {
                                    var found = candidatePapersOnDOI.Find(e => e.Id == cpm.Id);
                                    if (found == null && cpm.matchingScore >= MagPaperItemMatch.AutoMatchThreshold)
                                    {
                                        candidatePapersOnDOI.Add(cpm);
                                    }
                                }
                            }
                        }
                    }
                    if (candidatePapersOnDOI.Count > 0)
                    {
                        MagMakesHelpers.PaperMakes TopMatch = candidatePapersOnDOI.Aggregate((i1, i2) => i1.matchingScore > i2.matchingScore ? i1 : i2);
                        if (TopMatch.matchingScore > MagPaperItemMatch.AutoMatchThreshold)
                        {
                            matchedOnMAKES = true;
                            return rowId.ToString() + "," + TopMatch.matchingScore + "," + TopMatch.Id;
                        }
                    }
                }
                if (!matchedOnMAKES) // MATCHING STEP 2: pm is null, so we couldn't find it in the last version of MAKES. We fall back to the EPPI-Reviewer record.
                {
                    if (SeekingITEM_ID > 0) // for some items we have no SeekingITEM_ID - they are just lists of e.g. old contreview runs, so we don't try matching them here
                    {
                        Item i = null;
                        using (SqlConnection connection3 = new SqlConnection(DataConnection.ConnectionString))
                        {
                            connection3.Open();
                            using (SqlCommand command3 = new SqlCommand("st_Item", connection3))
                            {
                                command3.CommandType = System.Data.CommandType.StoredProcedure;
                                command3.Parameters.Add(new SqlParameter("@REVIEW_ID", -1));
                                command3.Parameters.Add(new SqlParameter("@ITEM_ID", SeekingITEM_ID));
                                using (Csla.Data.SafeDataReader reader3 = new Csla.Data.SafeDataReader(command3.ExecuteReader()))
                                {
                                    if (reader3.Read())
                                        i = Item.GetItem(reader3);
                                }
                            }
                            connection3.Close();
                        }
                        if (i != null)
                        {
                            // EXACTLY the same code logic is used in MagPaperItemMatch EXCEPT - we use the PENDING deployment of MAKES and we're only interested in matches above the auto-match threshold
                            List<MagMakesHelpers.PaperMakes> candidatePapersOnDOI = MagMakesHelpers.GetCandidateMatchesOnDOI(i.DOI, "PENDING");
                            if (candidatePapersOnDOI != null && candidatePapersOnDOI.Count > 0)
                            {
                                foreach (MagMakesHelpers.PaperMakes cpm in candidatePapersOnDOI)
                                {
                                    MagPaperItemMatch.doComparison(i, cpm);
                                }
                            }
                            if (candidatePapersOnDOI.Count == 0 || (candidatePapersOnDOI.Max(t => t.matchingScore) < MagPaperItemMatch.AutoMatchThreshold))
                            {
                                List<MagMakesHelpers.PaperMakes> candidatePapersOnTitle = MagMakesHelpers.GetCandidateMatches(i.Title, "PENDING", true);
                                if (candidatePapersOnTitle != null && candidatePapersOnTitle.Count > 0)
                                {
                                    foreach (MagMakesHelpers.PaperMakes cpm in candidatePapersOnTitle)
                                    {
                                        MagPaperItemMatch.doComparison(i, cpm);
                                    }
                                }
                                /* JT - don't think we need this, as it just removes stuff below the automatch score - and we filter on this below
                                for (int inn = 0; inn < candidatePapersOnTitle.Count; inn++)
                                {
                                    if (candidatePapersOnTitle[inn].matchingScore < AutoMatchMinScore)
                                    {
                                        candidatePapersOnTitle.RemoveAt(inn);
                                        inn--;
                                    }
                                }
                                */
                                foreach (MagMakesHelpers.PaperMakes cpm in candidatePapersOnTitle)
                                {
                                    var found = candidatePapersOnDOI.Find(e => e.Id == cpm.Id);
                                    if (found == null && cpm.matchingScore >= MagPaperItemMatch.AutoMatchThreshold)
                                    {
                                        candidatePapersOnDOI.Add(cpm);
                                    }
                                }
                                // add in matching on journals / authors if we don't have an exact match on title
                                if (candidatePapersOnTitle.Count == 0 || (candidatePapersOnTitle.Count > 0 && candidatePapersOnTitle.Max(t => t.matchingScore) < MagPaperItemMatch.AutoMatchThreshold))
                                {
                                    List<MagMakesHelpers.PaperMakes> candidatePapersOnAuthorJournal =
                                        MagMakesHelpers.GetCandidateMatches(i.Authors + " " + i.ParentTitle, "PENDING", false);
                                    foreach (MagMakesHelpers.PaperMakes cpm in candidatePapersOnAuthorJournal)
                                    {
                                        MagPaperItemMatch.doComparison(i, cpm);
                                    }
                                    foreach (MagMakesHelpers.PaperMakes cpm in candidatePapersOnAuthorJournal)
                                    {
                                        var found = candidatePapersOnDOI.Find(e => e.Id == cpm.Id);
                                        if (found == null && cpm.matchingScore >= MagPaperItemMatch.AutoMatchThreshold)
                                        {
                                            candidatePapersOnDOI.Add(cpm);
                                        }
                                    }
                                }
                            }
                            if (candidatePapersOnDOI.Count > 0)
                            {
                                MagMakesHelpers.PaperMakes TopMatch = candidatePapersOnDOI.Aggregate((i1, i2) => i1.matchingScore > i2.matchingScore ? i1 : i2);

                                if (TopMatch.matchingScore > MagPaperItemMatch.AutoMatchThreshold)
                                {
                                    return rowId.ToString() + "," + TopMatch.matchingScore + "," + TopMatch.Id;
                                }
                            }
                        }
                    } // if seekingID > 0
                }
            return rowId.ToString() + ",NOT_MATCHED";
        }

        private bool UpdateLiveIds()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand("st_MagUpdateChangedIds", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@MagVersion", _LatestMAGName));
                        command.ExecuteNonQuery();
                    }
                    connection.Close();
                }
                return true;
            }
            catch
            {
                return false;
            }
        }




#endif


    }


}
