using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;

namespace EPPIDataServices.Helpers
{

    public class CustomLoggerProviderConfigurationPubMed
    {
        public LogLevel LogLevel { get; set; } = LogLevel.Error;
        public int EventId { get; set; } = 0;
    }
      
    public static class LoggerExtensions
    {
        private static readonly Action<ILogger, string, string, Exception> _SQLActionFailed;
        private static readonly Action<ILogger, string, string, Exception> _FTPActionFailed;
        public static string SQLParams;
        public static string strFTP;

        public static void LogException(this ILogger logger, Exception ex, string message = "")
        {
            if (message != "") logger.LogError(message);
            if (ex.Message != null && ex.Message != "")
                logger.LogError("MSG: " + ex.Message);
            if (ex.StackTrace != null && ex.StackTrace != "")
                logger.LogError("STACK TRC:" + ex.StackTrace);
            if (ex.InnerException != null)
            {
                logger.LogError("Inner Exception(s): ");
                Exception ie = ex.InnerException;
                int i = 0;
                while (ie != null && i < 10)
                {
                    i++;
                    if (ie.Message != null && ie.Message != "")
                        logger.LogError("MSG(" + i.ToString() + "): " + ie.Message);
                    if (ie.StackTrace != null && ie.StackTrace != "")
                        logger.LogError("STACK TRC(" + i.ToString() + "):" + ie.StackTrace);
                    ie = ie.InnerException;
                }
            }
            logger.LogError("END" + Environment.NewLine);
        }

        public static void LogException(this Serilog.ILogger logger, Exception ex, string message = "")
        {
            if (message != "") logger.Error(message);
            if (ex.Message != null && ex.Message != "")
                logger.Error("MSG: " + ex.Message);
            if (ex.StackTrace != null && ex.StackTrace != "")
                logger.Error("STACK TRC:" + ex.StackTrace);
            if (ex.InnerException != null)
            {
                logger.Error("Inner Exception(s): ");
                Exception ie = ex.InnerException;
                int i = 0;
                while (ie != null && i < 10)
                {
                    i++;
                    if (ie.Message != null && ie.Message != "")
                        logger.Error("MSG(" + i.ToString() + "): " + ie.Message);
                    if (ie.StackTrace != null && ie.StackTrace != "")
                        logger.Error("STACK TRC(" + i.ToString() + "):" + ie.StackTrace);
                    ie = ie.InnerException;
                }
            }
            logger.Error("END" + Environment.NewLine);
        }

        static LoggerExtensions()
        {
            _SQLActionFailed = LoggerMessage.Define<string, string>(
                LogLevel.Error,
               new EventId(4, nameof(SQLActionFailed)),
               "SQL Error detected." + Environment.NewLine 
               + "(message = '{message}' " + Environment.NewLine + " SQLParams= {SQLParams})");

            _FTPActionFailed = LoggerMessage.Define<string, string>(
               LogLevel.Error,
              new EventId(4, nameof(FTPActionFailed)),
              "FTP Error detected (message = '{strFTP}' doing={doingWhat})"); 
        }

        public static void SQLActionFailed(this ILogger logger, string message, SqlParameter[] parameters, Exception ex)
        {
            LogException(logger, ex);
            SQLParams = "";
            if (parameters != null)
            {
                foreach (var item in parameters)
                {
                    if (item != null)
                    {
                        SQLParams += item.ParameterName + ", SQLValue: ";
                        if (item.Value == null) SQLParams += "NULL" + Environment.NewLine;
                        else SQLParams +=  item.Value.ToString() + Environment.NewLine;
                    }
                }
            }
            _SQLActionFailed(logger, message, SQLParams, ex);
        }

        public static void FTPActionFailed(this ILogger logger, List<string> messages, string doingWhat, Exception ex)
        {
            LogException(logger, ex);
            if (ex == null || ex.Message == null || ex.Message == "")
            {
                messages.Add("Unknown error " + doingWhat);
            }
            else
            {
                messages.Add("Error " + doingWhat + " At time: " + DateTime.Now.ToString("HH:mm:ss"));
                messages.Add(ex.Message);
                messages.Add(ex.StackTrace);
            }
            strFTP = "";
            foreach (var item in messages)
            {
                strFTP += item + Environment.NewLine;
            }
            _FTPActionFailed(logger, strFTP, doingWhat, ex);
        }
    }
}
