using BusinessLibrary.BusinessClasses;
using EPPIDataServices.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Serilog;
using System;
using static System.Net.Mime.MediaTypeNames;

namespace PDF_hashing
{
    internal partial class Program
    {
        private static void DoDocsMigration()
        {
            Log.Warning("DoDocsMigration is starting.");
            Console.WriteLine("DoDocsMigration is starting");
            int counter = 0;
            string prefix = "";
            if (AddHostNameToBlobFiles) prefix = Environment.MachineName + "-";
            long currentid = FindStartingPoint(prefix);


            while ((counter < MaxDocsToProcess || MaxDocsToProcess == 0)
                && currentid != -1)
            {
                currentid = ProcessNextItem(prefix, currentid);
                counter++;
                Thread.Sleep(MillisecondsToSleep);
            }


            bool res = BlobOperations.ThisBlobExist(blobConnection, BlobContainer, "bbb");

            Log.Warning("");
            Log.Warning("Processed: "+ counter.ToString() + " docs.");
            Log.Warning("DoDocsMigration has ended.");
            Log.Warning("");
            Console.WriteLine("");
            Console.WriteLine("Processed: " + counter.ToString() + " docs.");
            Console.WriteLine("DoDocsMigration has ended.");
            Console.WriteLine("");
        }
        
        private static long FindStartingPoint(string prefix)
        {
            Log.Warning("DoDocsMigration: find starting point.");
            Console.WriteLine("DoDocsMigration: find starting point.");
            long res = 0;
            if (IgnoreDocsFromId != -1)
            {
                List<BlobInHierarchy> ExistingFiles = BlobOperations.Blobfilenames(blobConnection, BlobContainer, prefix);
                res = FindHighestID(prefix, ExistingFiles);
            }

            Log.Warning("DoDocsMigration, starting ID is:" + res.ToString());
            Console.WriteLine("DoDocsMigration, starting ID is:" + res.ToString());
            return res;
        }
        private static long FindHighestID(string prefix, List<BlobInHierarchy> blobs)
        {
            long res = 0;
            if (IgnoreDocsFromId == -1) return res;//we check everything from the oldest doc in the DB
            long tVal;
            string tString = "";
            foreach (BlobInHierarchy blob in blobs) 
            {
                if (blob.IsVirtualFolder) continue;
                tString = blob.BlobName.Replace(prefix, string.Empty);
                tString = tString.Substring(0,tString.IndexOf('.'));
                if (long.TryParse(tString, out tVal))
                {
                    if (tVal > res && (IgnoreDocsFromId == 0 || tVal < IgnoreDocsFromId)) res = tVal;
                }
            }
            return res;
        }
        private static long ProcessNextItem(string prefix, long lastDocid = 0)
        {
            string cmd = "SELECT top 1 ITEM_DOCUMENT_ID, DOCUMENT_BINARY, DOCUMENT_EXTENSION from TB_ITEM_DOCUMENT where ITEM_DOCUMENT_ID > "
                + lastDocid.ToString();
            if (IgnoreDocsFromId > 0) cmd += " AND ITEM_DOCUMENT_ID < " + IgnoreDocsFromId.ToString();
            cmd += " ORDER by ITEM_DOCUMENT_ID";
            long id = -1;
            byte[] binaryDoc = [];
            string extension = "";
            using (SqlConnection conn = new SqlConnection(Program.SqlHelper.ER4DB))
            {
                using (SqlDataReader reader = SqlHelper.ExecuteQueryNonSP(conn, cmd))
                {
                    if (reader == null)
                    {
                        Log.Error("FAIL: could not fetch next doc to upload. Aborting.");
                        return -1;
                    }
                    else if (reader.Read())// && ItemIDs.Count < 5000)
                    {
                        id = (long)reader["ITEM_DOCUMENT_ID"];
                        if (reader["DOCUMENT_EXTENSION"] == System.DBNull.Value) extension = "";
                        else extension = (string)reader["DOCUMENT_EXTENSION"];
                        if (reader["DOCUMENT_BINARY"] == System.DBNull.Value) binaryDoc = [];
                        else binaryDoc = (byte[])reader["DOCUMENT_BINARY"];
                    }
                    else
                    {
                        Log.Error("FAIL: could not fetch next doc to upload. Aborting.");
                        return -1;
                    }
                }
            }
            if (id != -1)
            {
                if (extension == ".txt" || binaryDoc.Length == 0)
                {
                    Console.Write(".");
                    return id;
                }
                string filename = prefix + id.ToString() + extension;
                Stream BinaryStream = new MemoryStream(binaryDoc);
                if (!BlobOperations.ThisBlobExist(blobConnection, BlobContainer, filename))
                {
                    try
                    {
                        BlobOperations.UploadStream(blobConnection, BlobContainer, filename, BinaryStream);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogException(ex, "Error in BlobOperations.UploadStream");
                        Console.WriteLine("Error in BlobOperations.UploadStream:");
                        Console.WriteLine(ex.Message);
                        if (ex.StackTrace != null) Console.WriteLine(ex.StackTrace.ToString());
                        Console.WriteLine("");
                        Console.WriteLine("Aborting...");
                        Console.WriteLine("");
                        Console.WriteLine("");
                        return -1;
                    }
                    Console.Write(id.ToString() + ".");
                }
                return id;
            }
            return -1;
        }
    }
}