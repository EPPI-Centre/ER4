using System;
using System.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace Klasifiki
{
    public static class LoggerExtensions
    {
        private static readonly Action<ILogger, string, SqlParameter[], Exception> _SQLActionFailed;

        static LoggerExtensions()
        {
             _SQLActionFailed = LoggerMessage.Define<string, SqlParameter[]>(
                 LogLevel.Error,
                new EventId(4, nameof(SQLActionFailed)),
                "SQL Error detected (message = '{message}' Param1= {parameters[0]})");
        }

        public static void SQLActionFailed(this ILogger logger,string message, SqlParameter[] parameters, Exception ex)
        {
            _SQLActionFailed(logger,  message, parameters, ex);
        }
    }
}
