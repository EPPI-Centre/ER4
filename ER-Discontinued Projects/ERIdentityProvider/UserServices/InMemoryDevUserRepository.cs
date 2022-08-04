using System.Collections.Generic;
using System.Linq;
using System;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

namespace ERIdentityProvider.UserServices
{
    public class InMemoryDevUserRepository : IUserRepository
	{
		//this implementation is only used when running in "Development" environment and setting the "AppSettings:UseInMemoryStores" flag to true
		//IOW, it is used for development purposes. Allows to have a bunch of users (and change them, if desired) without the need of having SQL to provide persistence.	
		//precaution: SubjectIds are kept in the negative range so to avoid clashes with users from SQL...


		private List<CustomUser> _Users;
		private List<CustomUser> Users
		{
			get
			{
				if (_Users == null || _Users.Count == 0)
				{
					using (StreamReader r = new StreamReader("UserServices\\InMemoryDevUserRepositoryData.json"))
					{
						string json = r.ReadToEnd();
						_Users = JsonConvert.DeserializeObject<List<CustomUser>>(json);
					}
					foreach (CustomUser u in _Users)
					{//making sure some are expired, to see what difference it makes...
						//also: dynamicallly set expiry dates (in the future)
						if (u.Email.Contains("expired"))
						{
							u.ExpiresOn = DateTime.Now.AddDays(-10).ToString();
						}
						else
						{
							u.ExpiresOn = DateTime.Now.AddYears(1).ToString();
						}
					}
				}
				return _Users;
			}
		}

		public InMemoryDevUserRepository(IConfiguration conf)
        {
            _conf = conf;
        }
        private IConfiguration _conf;
        
        public async Task<CustomUser> ValidateCredentialsAsync(string username, string password)
        {
            CustomUser res = null;
			
			foreach (CustomUser u in Users)
			{
				if (u.UserName.ToLower() == username.ToLower()) return u;
			}
            return res;
        }

        public async Task<CustomUser> FindBySubjectIdAsync(string subjectId)
        {
            CustomUser res = null;
			foreach (CustomUser u in Users)
			{
				if (u.SubjectId == subjectId) return u;
			}
			return res;
        }

        //public CustomUser FindByUsername(string username)
        //{
        //    return _users.FirstOrDefault(x => x.UserName.Equals(username, StringComparison.OrdinalIgnoreCase));
        //}
    }
}
