using System;
using System.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace Klasifiki
{
    public static class LoggerExtensions
    {
        private static readonly Action<ILogger, string, string, Exception> _SQLActionFailed;
        public static string SQLParams;

        static LoggerExtensions()
        {
            _SQLActionFailed = LoggerMessage.Define<string, string>(
                LogLevel.Error,
               new EventId(4, nameof(SQLActionFailed)),
               "SQL Error detected (message = '{message}' SQLParams= {SQLParams})");
        }

        public static void SQLActionFailed(this ILogger logger, string message, SqlParameter[] parameters, Exception ex)
        {
            SQLParams = String.Format("{0}, {1}",
                                   parameters[0].ParameterName, parameters[1].ParameterName);
            _SQLActionFailed(logger, message, SQLParams, ex);
        }
    }
}
