using BusinessLibrary.BusinessClasses;
using CsvHelper.TypeConversion;
using FluentAssertions;
using FluentAssertions.Primitives;
using IntegrationTests.Fixtures;
using IntegrationTests.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationTests
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
            ReviewInfo? Res = await _client.GetAndDeserialize<ReviewInfo>("api/ReviewInfo/ReviewInfo");
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
            ReviewInfo? Res = await _client.GetAndDeserialize<ReviewInfo>("api/ReviewInfo/ReviewInfo");
            Res.Should().NotBeNull();
            Res.ReviewId.Should().Be(12);
            Res.ReviewName.Should().Be("Shared rev1 (id:12)");

            //To change who's logged on, change _authenticationDone to false (signals that the default authentication has not beend completed!)
            //and then call InnerLoginToReview(...) explicitly.
            _authenticationDone = false;
            await InnerLoginToReview("Jane", "123", 13);
            //So now Jane is logged on, even if Bob is default for this class.
            Res = await _client.GetAndDeserialize<ReviewInfo>("api/ReviewInfo/ReviewInfo");
            Res.Should().NotBeNull();
            Res.ReviewId.Should().Be(13);

            //to revert to the default situation, 
            (await AuthenticationDone()).Should().Be(true); Res = await _client.GetAndDeserialize<ReviewInfo>("api/ReviewInfo/ReviewInfo");
            Res.Should().NotBeNull();
            Res.ReviewId.Should().Be(12);
            Res.ReviewName.Should().Be("Shared rev1 (id:12)");
        }

        [Fact]
        public async Task ReviewMembers()
        {
            (await AuthenticationDone()).Should().Be(true);
            Contact[]? Res = await _client.GetAndDeserialize<Contact[]>("api/ReviewInfo/ReviewMembers");
            Res.Should().NotBeNull();
            Res.Length.Should().Be(7);
            Res[0].ToString().Should().Be("Bob Fake");
        }
    }
}
