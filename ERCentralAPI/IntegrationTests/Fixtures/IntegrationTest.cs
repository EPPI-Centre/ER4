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
    public abstract class IntegrationTest : IClassFixture<ApiWebApplicationFactory>
    {
        protected readonly ApiWebApplicationFactory _factory;
        protected readonly HttpClient _client;
        protected int RevId = 0;

        public IntegrationTest(ApiWebApplicationFactory fixture)
        {
            _factory = fixture;
            _client = _factory.CreateClient();
            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // if needed, reset the DB
            //_checkpoint.Reset(_factory.Configuration.GetConnectionString("SQL")).Wait();
        }
        protected void SetCookieHeaderVal(string val)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", val);
            //_client.DefaultRequestHeaders.Accept = new   'Content-Type': 'application/json; charset=utf-8'
        }

        protected async Task InnerLoginToReview(string uname, string pw, int revId)
        {
            LoginCreds loginCreds = new LoginCreds() { Password = pw, Username = uname };
            var ParsedResponse = await _client.PostAndDeserialize("api/Login/Login", loginCreds);
            string token = ParsedResponse["token"].ToString();
            SetCookieHeaderVal(token);
            SingleIntCriteria rc = new SingleIntCriteria() { Value = revId };
            ParsedResponse = await _client.PostAndDeserialize("api/Login/LoginToReview", rc);
            token = ParsedResponse["token"].ToString();
            RevId = (int)ParsedResponse["reviewId"];
            SetCookieHeaderVal(token);
        }
    }
}
