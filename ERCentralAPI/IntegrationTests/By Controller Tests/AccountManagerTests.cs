using BusinessLibrary.BusinessClasses;
using CsvHelper.TypeConversion;
using ERxWebClient2.Controllers;
using FluentAssertions;
using FluentAssertions.Primitives;
using IntegrationTests.Fixtures;
using IntegrationTests.Utils;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace IntegrationTests.By_Controller_Tests
{
    [Collection("Database collection")]
    public class AccountManagerTests : FixedLoginTest
    {
        //await InnerLoginToReview("bob", "123", 12);
        protected override string username { get; set; }
        protected override string password { get; set; }
        protected override int ReviewId { get; set; }

        public AccountManagerTests(ApiWebApplicationFactory fixture) : base(fixture)
        {
            username = "bob";
            password = "123";
            ReviewId = 12;
        }


        [Fact]
        public async Task AccountManagerConfirmAndUpdateSettings()
        {
            //The line below ensures we log on with the default values for this class.
            //every [Fact] should start with this line, so to ensure authentication is done (once, for the whole class).
            (await AuthenticationDone()).Should().Be(true);
            ReviewInfo? Res = await GetReviewInfo();
            Res.ReviewId.Should().Be(12);
            Res.ReviewName.Should().Be("Shared rev1 (id:12)");

            // update review name
            var ReviewName = "Dog days";
            JsonNode? Res2 = await UpdateReviewName(ReviewName);
            (Res2.ToString()).Should().Be("true");

            // get the updated value from the DB
            Res = await GetReviewInfo();
            Res.ReviewName.Should().Be("Dog days");

            // return the original review name
            ReviewName = "Shared rev1 (id:12)";
            Res2 = await UpdateReviewName(ReviewName);
            (Res2.ToString()).Should().Be("true");

        }


    }
}
namespace IntegrationTests.Fixtures
{
    /// <summary>
    /// All tests inherit from IntegrationTest, thus, we add methods to exchange data with the DB in the parent class
    /// in this way, they are available to "story tests", as/when needed.
    /// </summary>
    public abstract partial class IntegrationTest : IClassFixture<ApiWebApplicationFactory>
    {
        public async Task<JsonNode?> UpdateReviewName(string review_Name)
        {
            JsonNode? res = await client.PostAndDeserialize("api/AccountManager/UpdateReviewName", new SingleStringCriteria() { Value = review_Name });
            res.Should().NotBeNull();
            return res;
        }
    }
}
