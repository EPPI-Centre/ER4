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
            // spin the task off into another thread and return
            Task.Run(() => { RunCheckUpdates(); });
        }

        private void RunCheckUpdates()
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            int ReviewId = ri.ReviewId; // we don't use this, but it checks we have a valid ticket
            int ContactId = ri.UserId; // putting to variable in case the user invalidates ticket
            MagLog.SaveLogEntry("Started ID checking", "Started", "", ContactId);
            string uploadFileName = System.Web.HttpRuntime.AppDomainAppPath + TempPath + "CheckPaperIdChanges.csv";

            WriteCurrentlyUsedPaperIdsToFile(uploadFileName);
            UploadIdsFile(uploadFileName);
            MagLog.SaveLogEntry("File Upload", "Complete", "", ContactId);
            MagLog.SaveLogEntry("uSQL query", "Started", "", ContactId);
            SubmitJob(ContactId);
            MagLog.SaveLogEntry("uSQL query", "Complete", "", ContactId);
            DownloadMissingIdsFile(uploadFileName);
            MagLog.SaveLogEntry("Downloaded results", "Complete", "", ContactId);
            MagLog.SaveLogEntry("Auto-match IDs", "Started", "", ContactId);
            LookupMissingIdsInNewMakes(ContactId);
            MagLog.SaveLogEntry("Auto-match IDs", "Complete", "", ContactId);
            UpdateLiveIds();
            MagLog.SaveLogEntry("Live IDs updated", "Complete", "", ContactId);
        }

        private void WriteCurrentlyUsedPaperIdsToFile(string fileName)
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_MagGetCurrentlyUsedPaperIds", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        using (StreamWriter file = new StreamWriter(fileName, false))
                        {
                            while (reader.Read())
                            {
                                file.WriteLine(reader["PaperId"].ToString() + "\t" + reader["ITEM_ID"].ToString());
                            }
                        }
                    }
                }
            }
        }

        private void UploadIdsFile(string fileName)
        {
            string storageAccountName = ConfigurationManager.AppSettings["MAGStorageAccount"];
            string storageAccountKey = ConfigurationManager.AppSettings["MAGStorageAccountKey"];

            string storageConnectionString =
                "DefaultEndpointsProtocol=https;AccountName=" + storageAccountName + ";AccountKey=";
            storageConnectionString += storageAccountKey;

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("uploads");
            CloudBlockBlob blockBlobData;

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
        }

        private void SubmitJob(int ContactId)
        {
            MagDataLakeHelpers.ExecProc(@"[master].[dbo].[GetPaperIdChanges](""not used"");", true, "GetPaperIdChanges", ContactId, 9);
        }

        private void DownloadMissingIdsFile(string uploadFileName)
        {
            string storageAccountName = ConfigurationManager.AppSettings["MAGStorageAccount"];
            string storageAccountKey = ConfigurationManager.AppSettings["MAGStorageAccountKey"];

            string storageConnectionString =
                "DefaultEndpointsProtocol=https;AccountName=" + storageAccountName + ";AccountKey=";
            storageConnectionString += storageAccountKey;

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer containerUp = blobClient.GetContainerReference("uploads");
            CloudBlobContainer containerDown = blobClient.GetContainerReference("results");

            // cleaning up the file that was uploaded
            CloudBlockBlob blockBlobUploadData = containerUp.GetBlockBlobReference(Path.GetFileName(uploadFileName));
            blockBlobUploadData.DeleteIfExists();

            CloudBlockBlob blockBlobDownloadData = containerDown.GetBlockBlobReference(Path.GetFileName("CheckPaperIdChangesResults.tsv"));
            byte[] myFile = Encoding.UTF8.GetBytes(blockBlobDownloadData.DownloadText());

            MemoryStream ms = new MemoryStream(myFile);

            DataTable dt = new DataTable("Ids");
            dt.Columns.Add("Identity");
            dt.Columns.Add("OldPaperId");
            dt.Columns.Add("LatestMag");
            dt.Columns.Add("NewPaperId");
            dt.Columns.Add("ITEM_ID");
            dt.Columns.Add("NewAutoMatchScore");

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
            blockBlobDownloadData.DeleteIfExists();
        }

        private void LookupMissingIdsInNewMakes(int ContactId)
        {
            int PaperTotalCount = 0;
            int PaperTotalFound = 0;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_MagGetMissingPaperIds", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        while (reader.Read())
                        {
                            PaperTotalCount++;
                            Int64 SeekingPaperId = Convert.ToInt64(reader["OldPaperId"].ToString());
                            int rowId = Convert.ToInt32(reader["MagChangedPaperIdsId"].ToString());
                            Int64 SeekingITEM_ID = Convert.ToInt64(reader["ITEM_ID"].ToString());

                            // MATCHING STEP 1: try to match on the current MAKES deployment
                            MagMakesHelpers.PaperMakes pm = MagMakesHelpers.GetPaperMakesFromMakes(SeekingPaperId);
                            if (pm != null)
                            {
                                // MagPaperItemMatch has similar code to the below
                                List<MagMakesHelpers.PaperMakes> candidatePapersOnTitle = MagMakesHelpers.GetCandidateMatches(pm.DN, "PENDING");

                                if (candidatePapersOnTitle != null && candidatePapersOnTitle.Count > 0)
                                {
                                    foreach (MagMakesHelpers.PaperMakes cpm in candidatePapersOnTitle)
                                    {
                                        MagPaperItemMatch.doMakesPapersComparison(pm, cpm);
                                    }
                                }
                                // add in matching on journals / authors if we don't have an exact match on title
                                if (candidatePapersOnTitle.Count == 0 || (candidatePapersOnTitle.Max(t => t.matchingScore) < 0.7))
                                {
                                    List<MagMakesHelpers.PaperMakes> candidatePapersOnAuthorJournal =
                                        MagMakesHelpers.GetCandidateMatches(MagMakesHelpers.getAuthors(pm.AA) + " " + pm.J != null ? pm.J.JN : "");
                                    foreach (MagMakesHelpers.PaperMakes cpm in candidatePapersOnAuthorJournal)
                                    {
                                        MagPaperItemMatch.doMakesPapersComparison(pm, cpm);
                                    }
                                    foreach (MagMakesHelpers.PaperMakes cpm in candidatePapersOnAuthorJournal)
                                    {
                                        var found = candidatePapersOnTitle.Find(e => e.Id == cpm.Id);
                                        if (found == null)
                                        {
                                            candidatePapersOnTitle.Add(cpm);
                                        }
                                    }
                                }
                                MagMakesHelpers.PaperMakes TopMatch = candidatePapersOnTitle.Aggregate((i1, i2) => i1.matchingScore > i2.matchingScore ? i1 : i2);

                                if (TopMatch.matchingScore > 0.2)
                                {
                                    PaperTotalFound++;
                                    using (SqlConnection connection2 = new SqlConnection(DataConnection.ConnectionString))
                                    {
                                        connection2.Open();
                                        using (SqlCommand command2 = new SqlCommand("st_MagUpdateMissingPaperIds", connection2))
                                        {
                                            command2.CommandType = CommandType.StoredProcedure;
                                            command2.Parameters.Add(new SqlParameter("@MagChangedPaperIdsId", rowId));
                                            command2.Parameters.Add(new SqlParameter("@NewPaperId", TopMatch.Id));
                                            command2.ExecuteNonQuery();
                                        }
                                        connection2.Close();
                                    }
                                }

                            }
                            else // MATCHING STEP 2: pm is null, so we couldn't find it in the last version of MAKES. We fall back to the EPPI-Reviewer record
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
                                    List<MagMakesHelpers.PaperMakes> candidatePapersOnTitle = MagMakesHelpers.GetCandidateMatches(i.Title);
                                    foreach (MagMakesHelpers.PaperMakes cpm in candidatePapersOnTitle)
                                    {
                                        MagPaperItemMatch.doComparison(i, cpm);
                                    }
                                    // add in matching on journals / authors if we don't have an exact match on title
                                    if (candidatePapersOnTitle.Count == 0 || (candidatePapersOnTitle.Max(t => t.matchingScore) < 0.7))
                                    {
                                        List<MagMakesHelpers.PaperMakes> candidatePapersOnAuthorJournal = MagMakesHelpers.GetCandidateMatches(i.Authors + " " + i.ParentTitle);
                                        foreach (MagMakesHelpers.PaperMakes cpm in candidatePapersOnAuthorJournal)
                                        {
                                            MagPaperItemMatch.doComparison(i, cpm);
                                        }
                                        foreach (MagMakesHelpers.PaperMakes cpm in candidatePapersOnAuthorJournal)
                                        {
                                            var found = candidatePapersOnTitle.Find(e => e.Id == cpm.Id);
                                            if (found == null)
                                            {
                                                candidatePapersOnTitle.Add(cpm);
                                            }
                                        }
                                    }
                                    if (candidatePapersOnTitle.Count > 0)
                                    {
                                        MagMakesHelpers.PaperMakes TopMatch = candidatePapersOnTitle.Aggregate((i1, i2) => i1.matchingScore > i2.matchingScore ? i1 : i2);
                                        if (TopMatch.matchingScore > 0.2) // not sure what the threshold should be???
                                        {
                                            PaperTotalFound++;
                                            using (SqlConnection connection2 = new SqlConnection(DataConnection.ConnectionString))
                                            {
                                                connection2.Open();
                                                using (SqlCommand command2 = new SqlCommand("st_MagUpdateMissingPaperIds", connection2))
                                                {
                                                    command2.CommandType = CommandType.StoredProcedure;
                                                    command2.Parameters.Add(new SqlParameter("@MagChangedPaperIdsId", rowId));
                                                    command2.Parameters.Add(new SqlParameter("@NewAutoMatchScore", TopMatch.matchingScore));
                                                    command2.Parameters.Add(new SqlParameter("@NewPaperId", TopMatch.Id));
                                                    command2.ExecuteNonQuery();
                                                }
                                                connection2.Close();
                                            }

                                        }
                                    }
                                }
                            }

                        }

                    }
                }
                connection.Close();
                MagLog.SaveLogEntry("Updated " + PaperTotalFound.ToString() + " / " + PaperTotalCount.ToString() + " papers", "Complete", "", ContactId);
            }
        }

        private void UpdateLiveIds()
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
        }




#endif


    }


}
