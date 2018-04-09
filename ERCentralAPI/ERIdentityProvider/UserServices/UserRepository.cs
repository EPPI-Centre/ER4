using System.Collections.Generic;
using System.Linq;
using System;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace ERIdentityProvider.UserServices
{
    public class UserRepository : IUserRepository
    {
        // some dummy data. Replce this with your user persistence. 
        //private readonly List<CustomUser> _users = new List<CustomUser>
        //{
        //    new CustomUser{
        //        SubjectId = "123",
        //        UserName = "damienbod",
        //        //Password = "damienbod",
        //        Email = "damienbod@email.ch"
        //    },
        //    new CustomUser{
        //        SubjectId = "124",
        //        UserName = "raphael",
        //        //Password = "raphael",
        //        Email = "raphael@email.ch"
        //    },
        //};
        public UserRepository(IConfiguration conf)
        {
            _conf = conf;
        }
        private IConfiguration _conf;
        private string AdmConnStr
        {
            get { return _conf["AppSettings:AdmConnStr"]; }
        }
        private string ER4ConnStr
        {
            get { return _conf["AppSettings:ER4ConnStr"]; }
        }
        public async Task<CustomUser> ValidateCredentialsAsync(string username, string password)
        {
            CustomUser res = null;
            using (SqlConnection connection = new SqlConnection(AdmConnStr))
            {
				try
				{
					await connection.OpenAsync();
					using (SqlCommand command = new SqlCommand("st_ContactLogin", connection))
					{
						command.CommandType = System.Data.CommandType.StoredProcedure;
						command.Parameters.Add(new SqlParameter("@USERNAME", username));
						command.Parameters.Add(new SqlParameter("@PASSWORD", password));
						command.Parameters.Add(new SqlParameter("@IP_ADDRESS", "not used..."));

						using (SqlDataReader reader = await command.ExecuteReaderAsync())
						{
							if (reader.Read())
							{
								res = new CustomUser();
								res.UserName = username;
								if (reader["CONTACT_ID"] != null) res.SubjectId = reader["CONTACT_ID"].ToString();
								//if (reader["contact_name"] != null) res.DisplayName = reader["contact_name"].ToString();
								if (reader["EXPIRY_DATE"] != null) res.ExpiresOn = reader["EXPIRY_DATE"].ToString();
								//if (reader["IS_SITE_ADMIN"] != null) res.IsSiteAdmin = reader["IS_SITE_ADMIN"].ToString();
								//if (reader["ARCHIE_ID"] != null) res.IsCochrane = "true";
								//if (reader["EMAIL"] != null) res.Email = reader["EMAIL"].ToString();
								//no point populating much, only contact_id is used...
							}
						}
					}
				}
				catch (Exception e)
				{
					Console.WriteLine(e.Message);
				}
				

            }
            return res;
        }

        public async Task<CustomUser> FindBySubjectIdAsync(string subjectId)
        {
            CustomUser res = null;
            using (SqlConnection connection = new SqlConnection(AdmConnStr))
            {
                await connection.OpenAsync();
                using (SqlCommand command = new SqlCommand("st_ContactDetails", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@CONTACT_ID", subjectId));

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        if (reader.Read())
                        {
                            res = new CustomUser();
                            
							if (reader["USERNAME"] != null) res.UserName = reader["USERNAME"].ToString();
							if (reader["CONTACT_ID"] != null) res.SubjectId = reader["CONTACT_ID"].ToString();
							if (reader["contact_name"] != null) res.DisplayName = reader["contact_name"].ToString();
							if (reader["EXPIRY_DATE"] != null) res.ExpiresOn = reader["EXPIRY_DATE"].ToString();
							if (reader["IS_SITE_ADMIN"] != null) res.IsSiteAdmin = reader["IS_SITE_ADMIN"].ToString();
							//if (reader["ARCHIE_ID"] != null) res.IsCochrane = "true";
							if (reader["EMAIL"] != null) res.Email = reader["EMAIL"].ToString();
							//if (reader["IS_COCHRANE_USER"] != null) res.IsCochrane = reader["IS_COCHRANE_USER"].ToString();
						}
					}
                }

            }
            return res;
        }

        //public CustomUser FindByUsername(string username)
        //{
        //    return _users.FirstOrDefault(x => x.UserName.Equals(username, StringComparison.OrdinalIgnoreCase));
        //}
    }
}
