using BusinessLibrary.BusinessClasses;
using ERxWebClient2.Controllers;
using FluentAssertions;
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
    public class ReviewTests : IntegrationTest
    {
        public ReviewTests(ApiWebApplicationFactory fixture) : base(fixture) { }

        [Fact]
        public async Task CreateAndOpenReview()
        {
            await InnerLoginToReview("bob", "123", 12);
            JsonNode? res = await ReadOnlyReviews();
            JsonArray? JA = res as JsonArray;

            JA.Should().NotBeNull();
            int revsCount = JA.Count;

            string revName = "Bob's new review - " + DateTime.Now.Millisecond.ToString();
            res = await CreateReview(revName, ContactId);//Returns the new RevId
            int newRevIdres = (int)res;

            res = await ReadOnlyReviews();
            JA = res as JsonArray;
            JA.Should().NotBeNull();
            JA.Count.Should().Be(revsCount + 1);//we added one review!

            JsonNode? newRev = JA.FirstOrDefault(f => f["reviewName"].ToString() == revName);//can we find it?
            newRev.Should().NotBeNull();
            int newRevId = (int)newRev["reviewId"];
            newRevId.Should().Be(newRevIdres);//is the ID what we expected?
            await InnerLoginToReview("bob", "123", newRevId);
            RevId.Should().Be(newRevId);//is the ID of the review we've just opened the expected?
        }


    }
}
namespace IntegrationTests.Fixtures
{
    public abstract partial class IntegrationTest : IClassFixture<ApiWebApplicationFactory>
    {
        public  async Task<JsonNode?> CreateReview( string ReviewName, int ContactId)
        {
            reviewJson data = new reviewJson() { reviewName = ReviewName, userId = ContactId };
            JsonNode? res = await client.PostAndDeserialize("api/Review/CreateReview", data);
            res.Should().NotBeNull();
            return res;
        }
        public  async Task<JsonNode?> ReadOnlyReviews()
        {
            JsonNode? res = await client.GetAndDeserialize("api/Review/ReadOnlyReviews");
            res.Should().NotBeNull();
            return res;
        }
    }
}
