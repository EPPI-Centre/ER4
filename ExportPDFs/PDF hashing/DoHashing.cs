using Microsoft.Data.SqlClient;
using Serilog;
using System.Text;

namespace PDF_hashing
{
    internal partial class Program
    {
        private static void DoHashing()
        {
            Log.Warning("DoHashing is starting.");
            Console.WriteLine("DoHashing is starting.");
            int counter = 0; long currentid = 0;
            while ((counter < MaxDocsToProcess || MaxDocsToProcess == 0)
                && currentid != -1)
            {
                currentid = HashNextDoc(currentid);
                counter++;
                Thread.Sleep(MillisecondsToSleep);
            }
            Console.WriteLine("");
            Log.Warning("Processed: " + (counter - 1).ToString() + " docs.");
            Console.WriteLine("Processed: " + (counter - 1).ToString() + " docs.");
            if (counter >= MaxDocsToProcess && MaxDocsToProcess != 0)
            {
                Log.Warning("Processing ends: reached MaxDocsToProcess");
                Console.WriteLine("Processing ends: reached MaxDocsToProcess");
            }
            else if (currentid == -1)
            {
                Log.Warning("Processing ends: no more docs");
                Console.WriteLine("Processing ends: no more docs");
            }
            else
            {
                Log.Warning("Processing ends: unkown reason");
                Console.WriteLine("Processing ends: unkown reason");
            }
        }
        private static long HashNextDoc(long lastDocid = 0)
        {
            string que = @"SELECT TOP(1) ITEM_DOCUMENT_ID, DOCUMENT_TEXT from TB_ITEM_DOCUMENT where ITEM_DOCUMENT_ID > " + lastDocid.ToString()
                + " AND TXT_HASH is null order by ITEM_DOCUMENT_ID";
            long id = -1;
            //"Select distinct(r.ITEM_ID) from TB_ITEM_DOCUMENT d
            //            inner join TB_ITEM_REVIEW r on d.ITEM_ID = r.ITEM_ID and d.DOCUMENT_TEXT = 'Error: could not find/load an appropriate filter!'
            //             AND DOCUMENT_EXTENSION = '.pdf' AND REVIEW_ID = " + RevId;// + " AND IS_INCLUDED = 1 and IS_DELETED = 0 and MASTER_ITEM_ID is null";
            using (SqlConnection conn = new SqlConnection(Program.SqlHelper.ER4DB))
            {
                string text = "";
                string CMD = "";
                string hashed = "";
                using (SqlDataReader reader = SqlHelper.ExecuteQueryNonSP(conn, que))
                {
                    if (reader == null)
                    {
                        Log.Error("FAIL: could not fetch next doc to hash. Aborting.");
                        return -1;
                    }
                    else if (reader.Read())// && ItemIDs.Count < 5000)
                    {
                        id = (Int64)reader["ITEM_DOCUMENT_ID"];
                        text = (string)reader["DOCUMENT_TEXT"];
                    }
                }
                if (text != null && id > -1)
                {
                    if (text.Length > 4000) text = text.Substring(0, 4000);
                    //has to be, because in SQL HASHBYTES('SHA1', @txt) needs @txt to be 8000 bytes max
                    //IN SQL this will be something like:
                    //if DATALENGTH(@txt) > 8000 -- 2 bytes per nvarchar character
                    //BEGIN
                    //  Set @toHash = SUBSTRING(@txt, 0, 4000)
                    //END
                    //ELSE
                    //BEGIN
                    //    Set @toHash = @txt
                    //END

                    //SQL to get the same hash from truncated txt is:
                    //DATALENGTH(SUBSTRING(DOCUMENT_TEXT, 0, 4001)) dunno why, but that's how it looks


                    if (text.Length > 200) hashed = HashString(text);
                    else
                    {
                        //0x1C209ADD594DF6B37167F1F668D582D1F37658F7
                        hashed = "0x0000000000000000000000000000000000000000";
                    }
                    CMD = "UPDATE TB_ITEM_DOCUMENT set TXT_HASH = CONVERT(varbinary(20), '"
                                    + hashed
                                    + "', 1) WHERE ITEM_DOCUMENT_ID =" + id.ToString();

                    //debug: adding the line below makes every call fail, used to check for error handling
                    //CMD += Environment.NewLine + "WAITFOR DELAY '00:00:31'";
                    int res = SqlHelper.ExecuteNonQueryNonSP(conn, CMD);
                    if (res == -2) //we didn't save this hash and error should be investigated
                    {//we will however continue processing
                        Log.Error("DID NOT SAVE HASH for: " + id.ToString() + ". Hash is: " + hashed + " Text starts with: " + (text.Length > 20 ? text.Substring(0, 20) : text));
                        Log.Error("SQL Command was: " + Environment.NewLine + CMD + "");
                        Console.WriteLine("DID NOT SAVE HASH for: " + id.ToString() + ".");
                    }
                    else
                    {
                        Log.Information("hashed " + id.ToString() + ". Hash is: " + hashed + " Text starts with: " + (text.Length > 20 ? text.Substring(0, 20) : text));
                        Console.Write(id.ToString() + ".");
                    }
                }

            }
            return id;
        }
        private static string HashString(string input)
        {
            string res = "";
            //var sha1 = new System.Security.Cryptography.SHA1.;
            byte[] plaintextBytes = Encoding.Unicode.GetBytes(input);
            byte[]? hashBytes = System.Security.Cryptography.SHA1.HashData(plaintextBytes);

            if (hashBytes != null)
            {
                System.Text.StringBuilder s = new System.Text.StringBuilder();
                s.Append("0x");
                foreach (byte b in hashBytes)
                {
                    s.Append(b.ToString("x2").ToUpper());
                }
                //res = System.Convert.ToBase64String(hashBytes);
                res = s.ToString();
            }
            return res;
        }
    }
}