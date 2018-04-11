using Microsoft.Extensions.Configuration;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;

namespace SQL_Changes_Manager
{
    class Program
    {
        private static string AdmConnStr = "";
        private static string ER4ConnStr = "";
        private static string ScriptsFolder = "";
        private static string LogFileFullPath = "";
        private static bool SaveLog = true;//we might use this via parameters, eventually.

        static void Main(string[] args)
        {
            if (GetAppSettings()) DoWork();
        }
        static void DoWork()
        {
            int CurrentVersionNumber = -1;
            using (SqlConnection connection = new SqlConnection(AdmConnStr))
            {
                try
                {
                    LogMessageLine("Retrieving V. Number of DataBase.");
                    connection.Open();
                    using (SqlCommand command = new SqlCommand("st_DbVersionGet", connection))
                    {
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                if (reader["VERSION_NUMBER"] != null) CurrentVersionNumber = (int)reader["VERSION_NUMBER"];
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    LogException(e, "Get Current Version Number");
                    return;
                }
            }
            List<int> scriptVns = GetFiles(CurrentVersionNumber);
            if (scriptVns != null) LogMessageLine("Found " + scriptVns.Count.ToString() + " files to porcess.");
            else LogMessageLine("No new files to process found, current V. N. is: " + CurrentVersionNumber.ToString() + ".");
            bool iserror = false;
            foreach (int vNumber in scriptVns)
            {
                if (!ProcessFile(vNumber))
                {
                    iserror = true;
                    LogMessageLine("Error encountered, terminating.");
                    break;
                }
            }
            if (!iserror)
            {
                if (scriptVns != null && scriptVns.Count > 0)
                {
                    LogMessageLine("UPDATING succeeded. " + scriptVns.Count.ToString() + " files processed.");
                    LogMessageLine("New Database Version Number is: " + scriptVns[scriptVns.Count - 1].ToString() + ".");
                }
            }
            LogMessageLine("END." + Environment.NewLine + Environment.NewLine + Environment.NewLine);
        }
        static List<int> GetFiles(int CurrentVersionNumber)
        {
            LogMessageLine("Retrieving Script Files.");
            List<int> Res = new List<int>();
            try
            {
                IEnumerable<string> scriptFiles = Directory.EnumerateFiles(ScriptsFolder, "*.sql");
                foreach (string scriptFile in scriptFiles)
                {
                    int vNumber;
                    string fileName = scriptFile.Substring(ScriptsFolder.Length + 1).ToLower();
                    fileName = fileName.Replace(".sql", "");
                    if (int.TryParse(fileName, out vNumber))
                    {//filename can be parsed to an integer
                        if (vNumber > CurrentVersionNumber) Res.Add(vNumber);
                    }
                }
            }
            catch (Exception e)
            {
                LogException(e, "Retrieving Script Files");
                return new List<int>();//don't want to apply changes if something went wrong...
            }
            Res.Sort();
            return Res;
        }

        static bool ProcessFile(int vNumber)
        {
            LogMessageLine("Processing File: \"" + vNumber.ToString() + ".sql\"");
            string fullfilename = ScriptsFolder + "\\" + vNumber.ToString() + ".sql";
            string SQLScript = "BEGIN TRANSACTION" + Environment.NewLine 
                +  File.ReadAllText(fullfilename) + Environment.NewLine
                + "COMMIT" + Environment.NewLine + "GO";
            try
            {
                using (SqlConnection connection = new SqlConnection(AdmConnStr))
                {
                    ServerConnection svrConnection = new ServerConnection(connection);
                    Server server = new Server(svrConnection);
                    server.ConnectionContext.ExecuteNonQuery(SQLScript);
                }
                using (SqlConnection connection = new SqlConnection(AdmConnStr))
                {//need to use a separate connection because the previous one might have moved to Reviewer from ReviewerAdmin
                    connection.Open();
                    using (SqlCommand command = new SqlCommand("st_DbVersionAdd", connection))
                    {
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@VERSION_NUMBER", vNumber));
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch(Exception e)
            {
                LogException(e, "Processing File: '" + vNumber.ToString() + ".sql'");
                return false;
            }
            return true;
        }

        static bool GetAppSettings()
        {
            LogMessageLine("SQL Changes Manager LOG.");
            LogMessageLine("Reading Configuration.");
            try
            {
                DirectoryInfo sourceDirectory = System.IO.Directory.CreateDirectory("SQLScripts");
                ScriptsFolder = sourceDirectory.FullName.ToLower();
                //sourceDirectory = System.IO.Directory.CreateDirectory("Logs");
                //LogsFolder = sourceDirectory.FullName.ToLower();
            }
            catch (Exception e)
            {
                LogException(e, "Find SQLScripts Folder");
                return false;
            }
                var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            IConfigurationRoot configuration = builder.Build();
            try
            {
                AdmConnStr = configuration["AppSettings:AdmConnStr"];
                if (AdmConnStr == null || AdmConnStr == "")
                    throw new Exception("ERROR: could not get value for AdmConnStr, please check appsettings.json file.");
                ER4ConnStr = configuration["AppSettings:ER4ConnStr"];
                if (ER4ConnStr == null || ER4ConnStr == "")
                    throw new Exception("ERROR: could not get value for ER4ConnStr, please check appsettings.json file.");
            }
            catch (Exception e)
            {
                LogException(e, "Read Configuration");
                return false;
            }
            return true;
        }
        static void LogException(Exception e, string Phase)
        {
            LogMessageLine("Error in \"" + Phase + "\" phase. Error message(s):");
            if (e.Message != null && e.Message != "")
                LogMessageLine("MSG: " + e.Message);
            if (e.StackTrace != null && e.StackTrace != "")
                LogMessageLine("STACK TRC:" + e.StackTrace);
            if (e.InnerException != null)
            {
                LogMessageLine("Inner Exception(s): ");
                Exception ie = e.InnerException;
                int i = 0;
                while (ie != null && i < 10)
                {
                    i++;
                    if (ie.Message != null && ie.Message != "")
                        LogMessageLine("MSG(" + i.ToString() +"): " + ie.Message);
                    if (ie.StackTrace != null && ie.StackTrace != "")
                        LogMessageLine("STACK TRC(" + i.ToString() + "):" + ie.StackTrace);
                    ie = ie.InnerException;
                }
            }
        }
        static void LogMessageLine(string line)
        {//will also log multiple lines, TBH - this method ensures line ends with NewLine
            if (!line.EndsWith(Environment.NewLine)) line += Environment.NewLine;
            LogMessageString(line);
        }
        static void LogMessageString(string MessageToLog)
        {//can be used when we want to progressively append without adding a newline after each addition
            if (MessageToLog == null || MessageToLog == "") return;
            if (SaveLog)
            {
                if (LogFileFullPath == "")
                {
                    LogFileFullPath = CreateLogFileName();
                }
                File.AppendAllText(LogFileFullPath, MessageToLog);
            }
            Console.Write(MessageToLog);
        }
        private static string CreateLogFileName()
        {
            DirectoryInfo logDir = System.IO.Directory.CreateDirectory("Logs");
            string LogFilename = logDir.FullName + @"\" + "SQL Changes LOG -" + DateTime.Now.ToString("dd-MM-yyyy") + ".txt";
            //if (!System.IO.File.Exists(LogFilename)) System.IO.File.Create(LogFilename);
            return LogFilename;
        }
    }
}
