//using Microsoft.Extensions.Logging;
//using System;
//using System.Collections.Concurrent;
//using System.Collections.Generic;
//using System.Data.SqlClient;
//using System.IO;

//namespace EPPIDataServices.Helpers
//{
//    // Specific implementation of dotnetcore ILogger
//    public class EPPILoggerOLD : ILogger
//    {

//        private bool SaveLog = false;
//        private string LogFileFullPath = "";
//        readonly CustomLoggerProviderConfiguration loggerConfigK;
//        readonly CustomLoggerProviderConfigurationPubMed loggerConfig;

//        public EPPILogger(CustomLoggerProviderConfiguration loggerConfigK)
//        {
//            this.loggerConfigK = loggerConfigK;
//        }

//        public EPPILogger(string name, CustomLoggerProviderConfigurationPubMed config)
//        {
//            // Need to change this so that the file is used for logging
//            // or not dependent on the setting...
//            SaveLog = true; // SaveLogTofile;

//            loggerConfig = config;
//        }

//        private static string CreateLogFileName()
//        {
//            DirectoryInfo logDir = System.IO.Directory.CreateDirectory("LogFiles");
//            string LogFilename = logDir.FullName + @"\" + "PubmedImportLog-" + DateTime.Now.ToString("dd-MM-yyyy") + ".txt";
//            //if (!System.IO.File.Exists(LogFilename)) System.IO.File.Create(LogFilename);
//            return LogFilename;
//        }
//        public static string Duration(DateTime starttime)
//        {
//            double ms = DateTime.Now.Subtract(starttime).TotalMilliseconds;
//            string duration = Math.Round(ms).ToString() + "ms.";
//            if (ms > 180000)
//            {
//                ms = DateTime.Now.Subtract(starttime).TotalMinutes;
//                duration = Math.Round(ms).ToString() + "m.";
//            }
//            else if (ms > 4000)
//            {
//                duration = Math.Round(ms / 1000).ToString() + "s.";
//            }
//            return duration;
//        }
//        // Main log method, for different types write extensions
//        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
//        {
//            string message = string.Format("{0}: {1} - {2}", logLevel.ToString(), eventId.Id, formatter(state, exception));
//            WriteTextToFile(message);

//        }
//        private void WriteTextToFile(string message)
//        {
//            string filePath = CreateLogFileName();
//            using (StreamWriter streamWriter = new StreamWriter(filePath, true))
//            {
//                streamWriter.WriteLine(message);
//                streamWriter.Close();
//            }
//        }

//        public bool IsEnabled(LogLevel logLevel)
//        {
//            throw new NotImplementedException();
//        }

//        public IDisposable BeginScope<TState>(TState state)
//        {
//            return null;
//        }
//    }

//    public class CustomLoggerProviderConfiguration
//    {
//        public LogLevel LogLevel { get; set; } = LogLevel.Error;
//        public int EventId { get; set; } = 0;
//    }

//    public class CustomLoggerProviderConfigurationPubMed
//    {
//        public LogLevel LogLevel { get; set; } = LogLevel.Error;
//        public int EventId { get; set; } = 0;
//    }

//    // The ILogger provider is described below
//    // see microsoft MSDN for different options here
//    // EPPILogger class is used as the method and properties...
//    public class CustomLoggerProvider : ILoggerProvider
//    {

//        readonly CustomLoggerProviderConfigurationPubMed loggerConfigK;
//        readonly ConcurrentDictionary<string, EPPILogger> loggers =
//         new ConcurrentDictionary<string, EPPILogger>();
//        public CustomLoggerProvider(CustomLoggerProviderConfigurationPubMed config)
//        {
//            loggerConfigK = config;
//        }
//        public ILogger CreateLogger(string category)
//        {
//            return loggers.GetOrAdd(category,
//             name => new EPPILogger(null, loggerConfigK));
//        }
//        public void Dispose()
//        {
//            //Write code here to dispose the resources
//        }
//    }

//    public static class LoggerExtensions
//    {
//        private static readonly Action<ILogger, string, string, Exception> _SQLActionFailed;
//        private static readonly Action<ILogger, string, string, Exception> _FTPActionFailed;
//        public static string SQLParams;
//        public static string strFTP;

//        public static void LogException(this ILogger logger, Exception ex)
//        {
//            if (ex.Message != null && ex.Message != "")
//                logger.LogError("MSG: " + ex.Message);
//            if (ex.StackTrace != null && ex.StackTrace != "")
//                logger.LogError("STACK TRC:" + ex.StackTrace);
//            if (ex.InnerException != null)
//            {
//                logger.LogError("Inner Exception(s): ");
//                Exception ie = ex.InnerException;
//                int i = 0;
//                while (ie != null && i < 10)
//                {
//                    i++;
//                    if (ie.Message != null && ie.Message != "")
//                        logger.LogError("MSG(" + i.ToString() + "): " + ie.Message);
//                    if (ie.StackTrace != null && ie.StackTrace != "")
//                        logger.LogError("STACK TRC(" + i.ToString() + "):" + ie.StackTrace);
//                    ie = ie.InnerException;
//                }
//            }
//        }

//        static LoggerExtensions()
//        {
//            _SQLActionFailed = LoggerMessage.Define<string, string>(
//                LogLevel.Error,
//               new EventId(4, nameof(SQLActionFailed)),
//               "SQL Error detected (message = '{message}' SQLParams= {SQLParams})");

//            _FTPActionFailed = LoggerMessage.Define<string, string>(
//               LogLevel.Error,
//              new EventId(4, nameof(FTPActionFailed)),
//              "FTP Error detected (message = '{strFTP}' doing={doingWhat})");
//        }

//        public static void SQLActionFailed(this ILogger logger, string message, SqlParameter[] parameters, Exception ex)
//        {
//            LogException(logger, ex);
//            SQLParams = "";
//            if (parameters != null)
//            {
//                foreach (var item in parameters)
//                {
//                    if (item != null)
//                    {
//                        SQLParams += item.ParameterName + "," + " SQLValue: " + item.Value.ToString();
//                    }
//                }
//            }
//            _SQLActionFailed(logger, message, SQLParams, ex);
//        }

//        public static void FTPActionFailed(this ILogger logger, List<string> messages, string doingWhat, Exception ex)
//        {
//            if (ex == null || ex.Message == null || ex.Message == "")
//            {
//                messages.Add("Unknown error " + doingWhat);
//            }
//            else
//            {
//                messages.Add("Error " + doingWhat + " At time: " + DateTime.Now.ToString("HH:mm:ss"));
//                messages.Add(ex.Message);
//                messages.Add(ex.StackTrace);
//            }
//            strFTP = "";
//            foreach (var item in messages)
//            {
//                strFTP += item + Environment.NewLine;
//            }
//            _FTPActionFailed(logger, strFTP, doingWhat, ex);
//        }
//    }
//}
