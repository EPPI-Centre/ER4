
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using System.Data.SqlClient;
using System.Diagnostics;

namespace IntegrationTests.Fixtures
{
    public class TransientDatabase : IDisposable
    {
        readonly string MainDbName = "tempTestReviewer";
        readonly string AdminDbName = "tempTestReviewerAdmin";
        readonly string ScriptsFolder = "SQL";
        readonly string systemConn = "Data Source=localhost;Initial Catalog=master;Integrated Security=True;TrustServerCertificate=True";
        readonly string mainConn = "Data Source=localhost;Initial Catalog=tempTestReviewer;Integrated Security=True";
        readonly string adminConn = "Data Source=localhost;Initial Catalog=tempTestReviewerAdmin;Integrated Security=True";
        private bool databasesSwapped = false;
        public TransientDatabase()
        {
            bool success = false;
            //create main DB
            success = ProcessFile("DBsCreate.sql", systemConn, false);
            if (!success) HandleFailure("main database create failure");
            success = ProcessFile("DBsCreate2.sql", systemConn, false);
            if (!success) HandleFailure("alter database files failure");
            //create data structures
            success = ProcessFile("ReviewerGenerateScript.sql", systemConn, false);
            if (!success) HandleFailure("main database generate1 failure");
            success = ProcessFile("ReviewerGenerateScript2.sql", systemConn);
            if (!success) HandleFailure("main database generate2 failure");
            success = ProcessFile("ReviewerAdminGenerateScript.sql", systemConn);
            if (!success) HandleFailure("Admin database generate failure");

            //put in the minimal data...
            success = ProcessFile("generate DATA.sql", systemConn);
            if (!success) HandleFailure("generate DATA failure");

            //before running the SQL changes scripts, we give the "normal" names to our new temp DBs, we'll restore things to normal ASAP.
            databasesSwapped = ProcessFile("SwapDatabases.sql", systemConn);
            if (!databasesSwapped) HandleFailure("Swapping DBs failure");

            //APPLY SQL changes scripts!!
            SQL_Changes_Manager.Program.Main(Array.Empty<string>());
            
            //SWAP DBs back in their correct place
            if (ProcessFile("SwapDatabasesBack.sql", systemConn))
            {//we swapped the DBs back in place, so:
                databasesSwapped = false;
            }
            //if databasesSwapped == true, at this point, things went wrong!
            if (databasesSwapped) HandleFailure("BIG FAIL!! Swapping DBs BACK failure");


            success = ProcessFile("ChangeSynonyms.sql", systemConn);
            if (!success) HandleFailure("change synonyms failure"); 

            success = ProcessFile("AddReviews.sql", systemConn);
            if (!success) HandleFailure("generate DATA failure");
        }

        public void Dispose()
        {
            DeleteDBs();
            // ... clean up test data from the database ...
        }
        private void DeleteDBs()
        {
            if (databasesSwapped) ProcessFile("SwapDatabasesBack.sql", systemConn);
            Debug.WriteLine("Deleting DBs...");
            ProcessFile("DeleteDBs.sql", systemConn, false);
            ProcessFile("DeleteDBs2.sql", systemConn, false);
            Debug.WriteLine("...");
            Debug.WriteLine("");
            Debug.WriteLine("");
        }
        private void HandleFailure(string failMsg)
        {
            DeleteDBs();
            throw new Exception(failMsg);
        }
        private bool ProcessFile(string FileName, string connStr, bool multiLineCommand = true)
        {

            string fullfilename = ScriptsFolder + "\\" + FileName;
            string SQLScript;
            if (multiLineCommand)
                SQLScript =  File.ReadAllText(fullfilename) + Environment.NewLine
                + "GO" + Environment.NewLine;
            //SQLScript = "BEGIN TRANSACTION" + Environment.NewLine
            //   + File.ReadAllText(fullfilename) + Environment.NewLine
            //   + "GO" + Environment.NewLine
            //   + "COMMIT" + Environment.NewLine + "GO";
            else SQLScript = File.ReadAllText(fullfilename);
            try
            {
                using (Microsoft.Data.SqlClient.SqlConnection connection = new Microsoft.Data.SqlClient.SqlConnection(connStr))
                {
                    ServerConnection svrConnection = new ServerConnection(connection);
                    Server server = new Server(svrConnection);
                    server.ConnectionContext.ExecuteNonQuery(SQLScript);
                }
            }
            catch (Exception e)
            {
                //LogException(e, "Processing File: '" + vNumber.ToString() + ".sql'");
                return false;
            }
            return true;
        }
    }
    [CollectionDefinition("Database collection")]
    public class DatabaseCollection : ICollectionFixture<TransientDatabase>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }
}