using BusinessLibrary.BusinessClasses;
using CsvHelper.TypeConversion;
using ERxWebClient2.Controllers;
using FluentAssertions;
using FluentAssertions.Primitives;
using IntegrationTests.Fixtures;
using IntegrationTests.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace IntegrationTests.By_Controller_Tests
{
    [Collection("Database collection")]
    public class ReviewInfoTests : FixedLoginTest
    {
        //await InnerLoginToReview("bob", "123", 12);
        protected override string username { get; set; }
        protected override string password { get; set; }
        protected override int ReviewId { get; set; }

        public ReviewInfoTests(ApiWebApplicationFactory fixture) : base(fixture) {
            username = "bob";
            password = "123";
            ReviewId = 12;
        }


        [Fact]
        public async Task ReviewInfo()
        {
            //The line below ensures we log on with the default values for this class.
            //every [Fact] should start with this line, so to ensure authentication is done (once, for the whole class).
            (await AuthenticationDone()).Should().Be(true);
            ReviewInfo? Res = await client.GetAndDeserialize<ReviewInfo>("api/ReviewInfo/ReviewInfo");
            Res.Should().NotBeNull();
            Res.ReviewId.Should().Be(12);
            Res.ReviewName.Should().Be("Shared rev1 (id:12)");
        }

        /// <summary>
        /// Demonstration test: shows how to use the "fixedLogin.AuthenticationDone()" 
        /// and also how it's still possible to control (occasionally) who is logged on to what, when needed.
        /// </summary>
        [Fact]
        public async Task ReviewInfoWhileChangingReviews()
        {
            (await AuthenticationDone()).Should().Be(true);
            ReviewInfo? Res = await GetReviewInfo();
            Res.ReviewId.Should().Be(12);
            Res.ReviewName.Should().Be("Shared rev1 (id:12)");

            //To change who's logged on, change _authenticationDone to false (signals that the default authentication has not beend completed!)
            //and then call InnerLoginToReview(...) explicitly.
            _authenticationDone = false;
            await InnerLoginToReview("Jane", "123", 13);
            //So now Jane is logged on, even if Bob is default for this class.
            Res = await GetReviewInfo();
            Res.ReviewId.Should().Be(13);//we changed review!

            //to revert to the default situation, 
            (await AuthenticationDone()).Should().Be(true); 
            Res = await GetReviewInfo();
            Res.ReviewId.Should().Be(12);
            Res.ReviewName.Should().Be("Shared rev1 (id:12)");
        }

        [Fact]
        public async Task ReviewMembers()
        {
            (await AuthenticationDone()).Should().Be(true);
            Contact[]? Res = await GetReviewMembers();
            Res.Length.Should().Be(7);
            Res[0].ToString().Should().Be("Bob Fake");
        }

        [Fact]
        public async Task ReviewInfoConfirmAndUpdateSettings()
        {
            //The line below ensures we log on with the default values for this class.
            //every [Fact] should start with this line, so to ensure authentication is done (once, for the whole class).
            (await AuthenticationDone()).Should().Be(true);
            ReviewInfo? Res = await GetReviewInfo();
            Res.ReviewId.Should().Be(12);
            Res.ReviewName.Should().Be("Shared rev1 (id:12)");
            Res.MagEnabled.Should().Be(0); // 0 -> off
            Res.ShowScreening.Should().BeFalse(); // false -> off

            // update reviewInfo with changed MagEnabled
            Res.MagEnabled = 1;
            ReviewInfo? Res2 = await UpdateRevInfoSettings(Res, "UpdateReviewInfo");

            // get the updated value from the DB
            Res = await GetReviewInfo();
            Res.MagEnabled.Should().Be(1); // 1 -> on
            Res.ShowScreening.Should().BeFalse(); // false -> off
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
        public async Task<ReviewInfo?> GetReviewInfo()
        {
            ReviewInfo? Res = await client.GetAndDeserialize<ReviewInfo>("api/ReviewInfo/ReviewInfo");
            Res.Should().NotBeNull();
            return Res;
        }
        public async Task<Contact[]?> GetReviewMembers()
        {
            Contact[]? res = await client.GetAndDeserialize<Contact[]>("api/ReviewInfo/ReviewMembers");
            res.Should().NotBeNull();
            return res;
        }

        public async Task<ReviewInfo?> UpdateRevInfoSettings(ReviewInfo updatedReviewInfo, string endPoint)
        {
            ReviewInfo? res = await client.PostAndDeserialize<ReviewInfo>("api/ReviewInfo/" + endPoint, updatedReviewInfo);
            res.Should().NotBeNull();
            return res;            
        }
    }
}
