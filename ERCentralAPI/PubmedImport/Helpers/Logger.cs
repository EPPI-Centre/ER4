using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EPPIDataServices.Helpers
{
    public class EPPILogger
    {
        private bool SaveLog = false;
        private string LogFileFullPath = "";
        public EPPILogger(bool SaveLogTofile)
        {
            SaveLog = SaveLogTofile;
        }
        public void LogException(Exception e, string Description)
        {
            LogMessageLine(Description);
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
                        LogMessageLine("MSG(" + i.ToString() + "): " + ie.Message);
                    if (ie.StackTrace != null && ie.StackTrace != "")
                        LogMessageLine("STACK TRC(" + i.ToString() + "):" + ie.StackTrace);
                    ie = ie.InnerException;
                }
            }
        }
        public void LogMessageLine(string line)
        {//will also log multiple lines, TBH - this method ensures line ends with NewLine
            if (!line.EndsWith(Environment.NewLine)) line += Environment.NewLine;
            LogMessageString(line);
        }
        public void LogMessageString(string MessageToLog)
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
            DirectoryInfo logDir = System.IO.Directory.CreateDirectory("LogFiles");
            string LogFilename = logDir.FullName + @"\" + "PubmedImportLog-" + DateTime.Now.ToString("dd-MM-yyyy") + ".txt";
            //if (!System.IO.File.Exists(LogFilename)) System.IO.File.Create(LogFilename);
            return LogFilename;
        }
        public static string Duration(DateTime starttime)
        {
            double ms = DateTime.Now.Subtract(starttime).TotalMilliseconds;
            string duration = Math.Round(ms).ToString() + "ms.";
            if (ms > 180000)
            {
                ms = DateTime.Now.Subtract(starttime).TotalMinutes;
                duration = Math.Round(ms).ToString() + "m.";
            }
            else if (ms > 4000)
            {
                duration = Math.Round(ms / 1000).ToString() + "s.";
            }
            return duration;
        }
    }
}
