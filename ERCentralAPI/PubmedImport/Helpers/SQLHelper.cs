#if CSLA_NETCORE
using Csla.Data;
using ERxWebClient2;
#endif
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace EPPIDataServices.Helpers
{    

    public class SQLHelper 
    {
        public readonly string DataServiceDB = "";
        public readonly string ER4DB = "";
        public readonly string ER4AdminDB = "";
        // EPPILogger Logger;
        private readonly ILogger _logger;

        public SQLHelper (IConfiguration configuration, ILogger logger)
        {
            //DatabaseName = configuration["AppSettings:DatabaseName"];
            //Logger = logger;
            _logger = logger;
            DataServiceDB = configuration["AppSettings:DataServiceDB"];
            ER4DB = configuration["AppSettings:ER4DB"];
            ER4AdminDB = configuration["AppSettings:ER4AdminDB"];
        }
        public SQLHelper (ILogger logger)
        {
            _logger = logger;
            DataServiceDB = "Server=localhost;Database=DataService;Integrated Security=True;";
        }
        /// <summary> 
        /// Call this when you want to open and close the SQLConnection in a single call
        /// </summary> 
        public int ExecuteNonQuerySP(string connSt, string SPname, params SqlParameter[] parameters)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connSt))
                {
                    connection.Open();
                    return ExecuteNonQuerySP(connSt, SPname, parameters);
                }
            }
            catch (Exception e)
            {
                _logger.SQLActionFailed("Error exectuing SP: " + SPname, parameters, e) ;
                return -2;
            }
        }
        /// <summary> 
        /// Call this when you want to use the same connection for multiple commands, will try opening the connection if it isn't already
        /// </summary> 
        public int ExecuteNonQuerySP( SqlConnection connection, string SPname, params SqlParameter[] parameters)
        {
            try
            {
                CheckConnection(connection);
                using (SqlCommand command = new SqlCommand(SPname, connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.AddRange(parameters);
                    return command.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {
                _logger.SQLActionFailed("Error exectuing SP: " + SPname, parameters, e);
                return -2;
            }
        }


        public int ExecuteNonQuerySPWtrans(SqlConnection connection, string SPname, SqlTransaction transaction, params SqlParameter[] parameters)
        {
            try
            {
                //CheckConnection(connection);
                using (SqlCommand command = new SqlCommand(SPname, connection))
                {
                    command.Transaction = transaction;
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.AddRange(parameters);
                    return command.ExecuteNonQuery();
                }
               
            }
            catch (Exception e)
            {
                _logger.SQLActionFailed("Error exectuing SP: " + SPname, parameters, e);
                return -2;
            }
        }

        /// <summary> 
        /// Call this when you want to open and close the SQLConnection in a single call
        /// Connection will close when you close the reader, hence
        /// ALWAYS use the reader in a *using* clasue:
        /// using (SqlDataReader reader = SqlHelper.ExecuteQuerySP(...)) {...}
        /// </summary> 
        public SqlDataReader ExecuteQuerySP(string connSt, string SPname, params SqlParameter[] parameters)
        {
            try
            {
                SqlConnection connection = new SqlConnection(connSt);
                
                connection.Open();
                using (SqlCommand command = new SqlCommand(SPname, connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.AddRange(parameters);
                    return command.ExecuteReader(System.Data.CommandBehavior.CloseConnection);
                }
                
            }
            catch (Exception e)
            {
                _logger.SQLActionFailed("Error exectuing SP: " + SPname, parameters, e);
                return null;
            }
        }

        /// <summary> 
        /// Call this when you want to use the same connection for multiple commands, will try opening the connection if it isn't already
        /// You need to make sure you'll close the connection within whatever code calls this!
        /// </summary> 
        public SqlDataReader ExecuteQuerySP(SqlConnection connection, string SPname, params SqlParameter[] parameters)
        {
            try
            {
                CheckConnection(connection);
                using (SqlCommand command = new SqlCommand(SPname, connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    if (parameters != null && parameters.Length > 0) command.Parameters.AddRange(parameters);
                    return command.ExecuteReader();
                }
            }
            catch (Exception e)
            {
                _logger.SQLActionFailed("Error exectuing SP: " + SPname, parameters, e);
                return null;
            }
        }
#if CSLA_NETCORE
        /// <summary> 
        /// Call this when you want to use the same connection for multiple commands, will try opening the connection if it isn't already
        /// You need to make sure you'll close the connection within whatever code calls this!
        /// </summary> 
        public SafeDataReader ExecuteCSLAQuerySP(SqlConnection connection, string SPname, params SqlParameter[] parameters)
        {
            try
            {
                CheckConnection(connection);
                using (SqlCommand command = new SqlCommand(SPname, connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    if (parameters != null && parameters.Length > 0) command.Parameters.AddRange(parameters);
                    return new Csla.Data.SafeDataReader(command.ExecuteReader());
                }
            }
            catch (Exception e)
            {
                _logger.LogError(null, e, "Error fetching list of type:", null);
                //Logger.LogSQLException(e, "Error exectuing SP: " + SPname, parameters);
                return null;
            }
        }
#endif
        private void CheckConnection(SqlConnection connection)
        {
            if (connection.State != System.Data.ConnectionState.Open)
            {
                if (connection.State == System.Data.ConnectionState.Broken)
                    throw new Exception("SQL connection to " + connection.ConnectionString + " is broken.");
                else if (connection.State > System.Data.ConnectionState.Open)
                {//try waiting...
                    for (int i = 9; i <= 36; i = i * 3)
                    {
                        System.Threading.Thread.Sleep(i * 1000); //9s, 18s, 36s will wait for up to 63s
                        if (connection.State == System.Data.ConnectionState.Open) break;
                    }
                    if (connection.State != System.Data.ConnectionState.Open)
                    {
                        try
                        {
                            connection.Open();
                        }
                        catch (Exception e)
                        {
                            _logger.LogError("Could not open SQL connection to " + connection.ConnectionString + ".", e);
                            Exception ee = new Exception("Could not open SQL connection to " + connection.ConnectionString + ".", e);
                            throw ee;
                        }
                    }
                }
                else connection.Open();
            }
        }


    }
}
#if(CSLA_NETCORE)
namespace BusinessLibrary.Data
{
    public static class DataConnection
    {
        public static string ConnectionString
        {
            get
            {
                return Program.SqlHelper.ER4DB;
            }
        }

        public static string AdmConnectionString
        {
            get
            {
                return Program.SqlHelper.ER4AdminDB;
            }
        }
    }
}
#endif
