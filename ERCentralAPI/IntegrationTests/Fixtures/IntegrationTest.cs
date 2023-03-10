using BusinessLibrary.BusinessClasses;
using ERxWebClient2.Controllers;
using IntegrationTests.Utils;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationTests.Fixtures
{
    [Trait("Category", "Integration")]
    public abstract partial class IntegrationTest : IClassFixture<ApiWebApplicationFactory>
    {
        protected readonly ApiWebApplicationFactory _factory;
        internal readonly HttpClient client;
        protected int RevId = 0;
        protected int ContactId = 0;

        public IntegrationTest(ApiWebApplicationFactory fixture)
        {
            _factory = fixture;
            client = _factory.CreateClient();
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // if needed, reset the DB
            //_checkpoint.Reset(_factory.Configuration.GetConnectionString("SQL")).Wait();
        }
        protected void SetCookieHeaderVal(string val)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", val);
            //client.DefaultRequestHeaders.Accept = new   'Content-Type': 'application/json; charset=utf-8'
        }

        protected async Task InnerLoginToReview(string uname, string pw, int revId)
        {
            LoginCreds loginCreds = new LoginCreds() { Password = pw, Username = uname };
            var ParsedResponse = await client.PostAndDeserialize("api/Login/Login", loginCreds);
            string token = ParsedResponse["token"].ToString();
            SetCookieHeaderVal(token);
            SingleIntCriteria rc = new SingleIntCriteria() { Value = revId };
            ParsedResponse = await client.PostAndDeserialize("api/Login/LoginToReview", rc);
            token = ParsedResponse["token"].ToString();
            RevId = (int)ParsedResponse["reviewId"];
            ContactId = (int)ParsedResponse["userId"];
            SetCookieHeaderVal(token);
        }
    }
}
