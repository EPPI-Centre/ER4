using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;


namespace PubmedImport
{
    class SQLHelper
    {
        public readonly string DataServiceDB = "";
        public readonly string ER4DB = "";
        public readonly string ER4AdminDB = "";
        public SQLHelper (IConfigurationRoot configuration)
        {
            //DatabaseName = configuration["AppSettings:DatabaseName"];
            DataServiceDB = configuration["AppSettings:DataServiceDB"];
            ER4DB = configuration["AppSettings:ER4DB"];
            ER4AdminDB = configuration["AppSettings:ER4AdminDB"];
        }
        /// <summary> 
        /// Call this when you want to open and close the SQLConnection in a single call
        /// </summary> 
        public static int ExecuteNonQuerySP(string connSt, string SPname, params SqlParameter[] parameters)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connSt))
                {
                    connection.Open();
                    return ExecuteNonQuerySP(connection, SPname, parameters);
                }
            }
            catch (Exception e)
            {
                Program.LogException(e, "Error exectuing SP: " + SPname);
                return -1;
            }
        }
        /// <summary> 
        /// Call this when you want to use the same connection for multiple commands, will try opening the connection if it isn't already
        /// </summary> 
        public static int ExecuteNonQuerySP(SqlConnection connection, string SPname, params SqlParameter[] parameters)
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
                Program.LogException(e, "Error exectuing SP: " + SPname);
                return -1;
            }
        }

        /// <summary> 
        /// Call this when you want to open and close the SQLConnection in a single call
        /// Connection will close when you close the reader, hence
        /// ALWAYS use the reader in a *using* clasue:
        /// using (SqlDataReader reader = SqlHelper.ExecuteQuerySP(...)) {...}
        /// </summary> 
        public static SqlDataReader ExecuteQuerySP(string connSt, string SPname, params SqlParameter[] parameters)
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
                Program.LogException(e, "Error exectuing SP: " + SPname);
                return null;
            }
        }

        /// <summary> 
        /// Call this when you want to use the same connection for multiple commands, will try opening the connection if it isn't already
        /// You need to make sure you'll close the connection within whatever code calls this!
        /// </summary> 
        public static SqlDataReader ExecuteQuerySP(SqlConnection connection, string SPname, params SqlParameter[] parameters)
        {
            try
            {
                CheckConnection(connection);
                using (SqlCommand command = new SqlCommand(SPname, connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.AddRange(parameters);
                    return command.ExecuteReader();
                }
            }
            catch (Exception e)
            {
                Program.LogException(e, "Error exectuing SP: " + SPname);
                return null;
            }
        }
        private static void CheckConnection(SqlConnection connection)
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

