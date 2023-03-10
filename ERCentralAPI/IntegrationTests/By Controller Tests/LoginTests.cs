using Azure;
using BusinessLibrary.BusinessClasses;
using BusinessLibrary.Security;
using ERxWebClient2.Controllers;
using FluentAssertions;
using IntegrationTests.Fixtures;
using IntegrationTests.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Text.Json.Nodes;

namespace IntegrationTests.By_Controller_Tests { 
    [Collection("Database collection")]
    public class LoginTests : IntegrationTest
    {
        public LoginTests(ApiWebApplicationFactory fixture): base(fixture) { }

        [Fact]
        public async Task VersionInfo()
        {
            //var forecast = await client.GetAndDeserialize<IActionResult>("/Login/VersionInfo");
            //var response = await client.GetAsync("api/Login/VersionInfo");
            //response.StatusCode.Should().Be(HttpStatusCode.OK);
            JsonNode? ParsedResponse = await client.GetAndDeserialize("api/Login/VersionInfo");
            ParsedResponse.Should().NotBeNull();
            _ = ParsedResponse["versionN"].Should().NotBeNull();
            _ = ParsedResponse["versionN"].ToString().Should().NotBeNullOrEmpty();
        }
        [Fact]
        public async Task Login()
        {
            LoginCreds loginCreds = new LoginCreds() { Password = "123", Username = "alice" };
            var ParsedResponse = await client.PostAndDeserialize("api/Login/Login", loginCreds);
            _ = ((bool)ParsedResponse["isAuthenticated"]).Should().Be(true);
            _ = ParsedResponse["token"].ToString().Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task LoginToReview()
        {
            LoginCreds loginCreds = new LoginCreds() { Password = "123", Username = "alice" };
            var ParsedResponse = await client.PostAndDeserialize("api/Login/Login", loginCreds);
            _ = ((bool)ParsedResponse["isAuthenticated"]).Should().Be(true);
            _ = ParsedResponse["token"].ToString().Should().NotBeNullOrEmpty();
            string token = ParsedResponse["token"].ToString();
            SetCookieHeaderVal(token);
            SingleIntCriteria rc = new SingleIntCriteria() { Value = 5};
            ParsedResponse = await client.PostAndDeserialize("api/Login/LoginToReview", rc);
            _ = ((bool)ParsedResponse["isAuthenticated"]).Should().Be(true);
            _ = ParsedResponse["token"].ToString().Should().NotBeNullOrEmpty();

            SetCookieHeaderVal(token);
        }

        
    }
}