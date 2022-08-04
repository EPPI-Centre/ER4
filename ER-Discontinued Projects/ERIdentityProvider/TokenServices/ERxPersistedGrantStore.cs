using IdentityServer4.Models;
using IdentityServer4.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace ERIdentityProvider.TokenServices
{
    public class ERxPersistedGrantStore : IPersistedGrantStore
    {
        private static IConfiguration _conf;
        private static string AdmConnStr
        {
            get { return _conf["AppSettings:AdmConnStr"]; }
        }
        private static string ER4ConnStr
        {
            get { return _conf["AppSettings:ER4ConnStr"]; }
        }
        public ERxPersistedGrantStore(IConfiguration conf)
        {
            _conf = conf;
        }
        public async Task<IEnumerable<PersistedGrant>> GetAllAsync(string subjectId)
        {
            //return Task.Run(() => GetAllFunc(subjectId));
            List<PersistedGrant> res = new List<PersistedGrant>();
            using (SqlConnection connection = new SqlConnection(AdmConnStr))
            {
                await connection.OpenAsync();
                using (SqlCommand command = new SqlCommand("st_PersistedGrantGetAll", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@CONTACT_ID", int.Parse(subjectId)));
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        if (reader.Read())
                        {
                            res.Add(GrantFromReader(reader));
                        }
                        else return null;
                    }
                }
            }
            return res;
        }
        private static PersistedGrant GrantFromReader(SqlDataReader reader)
        {
            PersistedGrant res = new PersistedGrant();
            GrantFromReader(res, reader);
            return res;
        }
        private static void GrantFromReader(PersistedGrant res, SqlDataReader reader)
        {
            if (reader["KEY"] != null) res.Key = reader["KEY"].ToString();
            if (reader["TYPE"] != null) res.Type = reader["TYPE"].ToString();
            if (reader["CLIENT_ID"] != null) res.ClientId = reader["CLIENT_ID"].ToString();
            if (reader["DATE_CREATED"] != null) res.CreationTime = DateTime.Parse(reader["DATE_CREATED"].ToString());
            if (reader["DATA"] != null) res.Data = reader["DATA"].ToString();
            if (reader["EXPIRATION"] != null) res.Expiration = DateTime.Parse(reader["EXPIRATION"].ToString());
            if (reader["CONTACT_ID"] != null) res.SubjectId = reader["CONTACT_ID"].ToString();
        }

        public async Task<PersistedGrant> GetAsync(string key)
        {
            //return Task.Run(() => GetAsyncFunc(key));
            PersistedGrant res = new PersistedGrant();
            using (SqlConnection connection = new SqlConnection(AdmConnStr))
            {
                await connection.OpenAsync();
                using (SqlCommand command = new SqlCommand("st_PersistedGrantGet", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@KEY", key));
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        if (reader.Read())
                        {
                            GrantFromReader(res, reader);
                        }
                        else return null;
                    }
                }
            }
            return res;
        }

        public async Task RemoveAllAsync(string subjectId, string clientId)
        {
            //return Task.Run(() => RemoveAllAsyncFunc(subjectId, clientId));
            await RemoveAllAsync(subjectId, clientId, "ALLTYPES");
        }
        public async Task RemoveAllAsync(string subjectId, string clientId, string type)
        {
            //return Task.Run(() => RemoveAllAsyncFunc(subjectId, clientId, type));
            using (SqlConnection connection = new SqlConnection(AdmConnStr))
            {
                await connection.OpenAsync();
                using (SqlCommand command = new SqlCommand("st_PersistedGrantRemoveAll", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@CONTACT_ID", int.Parse(subjectId)));
                    command.Parameters.Add(new SqlParameter("@CLIENT_ID", clientId));
                    command.Parameters.Add(new SqlParameter("@TYPE", type));
                    await command.ExecuteReaderAsync();
                }
            }
            return;
        }
        
        public async Task RemoveAsync(string key)
        {
            //return Task.Run(() => RemoveAsyncFunc(key));
            using (SqlConnection connection = new SqlConnection(AdmConnStr))
            {
                await connection.OpenAsync();
                using (SqlCommand command = new SqlCommand("st_PersistedGrantRemove", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@KEY", key));
                    await command.ExecuteReaderAsync();
                }
            }
            return;
        }
        
        public async Task StoreAsync(PersistedGrant grant)
        {
            //return Task.Run(() => StoreAsyncFunc(grant));
            using (SqlConnection connection = new SqlConnection(AdmConnStr))
            {
                await connection.OpenAsync();
                using (SqlCommand command = new SqlCommand("st_PersistedGrantAdd", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@KEY", grant.Key));
                    command.Parameters.Add(new SqlParameter("@TYPE", grant.Type));
                    command.Parameters.Add(new SqlParameter("@CLIENT_ID", grant.ClientId));
                    command.Parameters.Add(new SqlParameter("@DATE_CREATED", grant.CreationTime));
                    command.Parameters.Add(new SqlParameter("@DATA", grant.Data));
                    command.Parameters.Add(new SqlParameter("@EXPIRATION", grant.Expiration));
                    command.Parameters.Add(new SqlParameter("@CONTACT_ID", int.Parse(grant.SubjectId)));
                    await command.ExecuteReaderAsync();
                }
            }
            return;
        }
    }
}
