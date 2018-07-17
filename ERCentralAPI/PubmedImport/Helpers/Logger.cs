using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Text;

namespace EPPIDataServices.Helpers
{
    public class EPPILogger : ILogger
    {
        private bool SaveLog = false;
        private string LogFileFullPath = "";
        private string name;
        readonly CustomLoggerProviderConfiguration loggerConfigK;
        readonly CustomLoggerProviderConfigurationPubMed loggerConfig;

        public EPPILogger(CustomLoggerProviderConfiguration loggerConfigK)
        {
            this.loggerConfigK = loggerConfigK;
        }

        public EPPILogger(string name, CustomLoggerProviderConfigurationPubMed config)
        {
            SaveLog = true; // SaveLogTofile;

            loggerConfig = config;
        }
        private static void LogFTPexceptionSafely(Exception e, List<string> messages, string doingWhat)
        {
            if (e == null || e.Message == null || e.Message == "")
            {
                messages.Add("Unknown error " + doingWhat);
            }
            else
            {
                messages.Add("Error " + doingWhat + " At time: " + DateTime.Now.ToString("HH:mm:ss"));
                //_logger.LogInformation("Error " + doingWhat);
                messages.Add(e.Message);
                //_logger.LogInformation(e.Message);
                //_logger.LogInformation(e.StackTrace);
                messages.Add(e.StackTrace);
            }
        }


        public void LogSQLException(Exception e, string Description, params SqlParameter[] parameters)
        {
            LogException(e, Description);
            LogMessageLine("SQL parameters:");
            foreach (SqlParameter par in parameters)
            {
                LogMessageLine("Param NAME: " + par.ParameterName + "; Param Value: " + par.Value.ToString());
            }
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

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            // need a switch for various log messges we want

            // Implement the SQL exceptions in here.
            string message = string.Format("{0}: {1} - {2}", logLevel.ToString(), eventId.Id, formatter(state, exception));
            WriteTextToFile(message);
        }
        private void WriteTextToFile(string message)
        {
            string filePath = CreateLogFileName(); //"D:\\IDGLog.txt";
            using (StreamWriter streamWriter = new StreamWriter(filePath, true))
            {
                streamWriter.WriteLine(message);
                streamWriter.Close();
            }
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            throw new NotImplementedException();
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }
    }

    public class CustomLoggerProviderConfiguration
    {
        public LogLevel LogLevel { get; set; } = LogLevel.Warning;
        public int EventId { get; set; } = 0;
    }

    public class CustomLoggerProviderConfigurationPubMed
    {
        public LogLevel LogLevel { get; set; } = LogLevel.Warning;
        public int EventId { get; set; } = 0;
    }

    public class CustomLoggerProvider : ILoggerProvider
    {

        readonly CustomLoggerProviderConfigurationPubMed loggerConfigK;
        readonly ConcurrentDictionary<string, EPPILogger> loggers =
         new ConcurrentDictionary<string, EPPILogger>();
        public CustomLoggerProvider(CustomLoggerProviderConfigurationPubMed config)
        {
            loggerConfigK = config;
        }
        public ILogger CreateLogger(string category)
        {
            return loggers.GetOrAdd(category,
             name => new EPPILogger(null, loggerConfigK));
        }
        public void Dispose()
        {
            //Write code here to dispose the resources
        }
    }
}
