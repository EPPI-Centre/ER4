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
    /// <summary>
    /// This class of tests is written in a way that supports recycling API-Accessing code for "story mode" tests,
    /// Where, unlike in "atomic tests", we simulate the story of a review, 
    /// thus modifying data in the database as we move along the story
    /// it also uses "theory" decorations to show how to re-run the same test code multiple times, chaning the input
    /// </summary>
    [Collection("Database collection")]
    public class SourcesTests : FixedLoginTest
    {
        protected override string username { get; set; }
        protected override string password { get; set; }
        protected override int ReviewId { get; set; }
        public SourcesTests(ApiWebApplicationFactory fixture) : base(fixture)
        {
            username = "bob";
            password = "123";
            ReviewId = 12;
        }
        [Theory]
        [InlineData("20 refs.txt", 20, "Behavioral activation of religious behaviors (BARB): Randomized trial with depressed college students.")]
        [InlineData("60 refs.txt", 60, "Two short forms of the Agnew Relationship Measure: The ARM-5 and ARM-12.")]
        public async Task VerifyFile(string filename, int expectedRefsCount, string expectedLastTitle = "", string importFilterName = "RIS")
        {
            (await AuthenticationDone()).Should().Be(true);
            IncomingItemsListJSON res = await SendThisRefsFile(filename, importFilterName, "VerifyFile");
            res.totalReferences.Should().Be(expectedRefsCount);
            res.incomingItems.Count.Should().Be(expectedRefsCount);
            if (expectedLastTitle != "")
            {
                res.incomingItems[res.incomingItems.Count - 1].Title.Should().Be(expectedLastTitle);
            }
        }



        [Theory]
        [InlineData("20 refs.txt")]
        public async Task UploadSourceThenDeleteIt(string filename, string importFilterName = "RIS")
        {
            (await AuthenticationDone()).Should().Be(true);

            //first, find out what's in there...
            JsonNode? srcs = await GetSources();
            srcs["sources"].Should().NotBeNull();
            JsonArray? ross = (JsonArray?)srcs["sources"];
            ross.Should().NotBeNull();
            ross.Count.Should().BeGreaterThanOrEqualTo(1);
            int CurrentSrcsCount = ross.Count;
            ross[0]["source_Name"].Should().NotBeNull();

            //upload the file, check the answer
            IncomingItemsListJSON res = await SendThisRefsFile(filename, importFilterName, "UploadSource");
            res.Should().NotBeNull();
            res.totalReferences.Should().Be(20);
            res.incomingItems.Count.Should().Be(0);

            //check sources again, make sure our new source exists
            srcs = await GetSources();
            srcs["sources"].Should().NotBeNull();
            ross = (JsonArray?)srcs["sources"];
            ross.Should().NotBeNull();
            ross.Count.Should().BeGreaterThanOrEqualTo(CurrentSrcsCount);

            var newSrc = ross.FirstOrDefault<JsonNode?>(f => filename == f["source_Name"].ToString());
            newSrc.Should().NotBeNull();
            int source_ID = (int)newSrc["source_ID"];
            JsonNode? res3 = await DeleteUndeleteSource(source_ID);
            res3.Should().NotBeNull();

            //finally, we delete forever, but without waiting for the result, another test might check this - we don't wait because this is SLOW!
            Task<JsonNode?> throwAway = DeleteSourceForever(source_ID);

        }

        [Theory]
        [InlineData("Pippo")]
        public async Task getNewPubMedSearchPreview(string SearchSt)
        {
            (await AuthenticationDone()).Should().Be(true);
            PubMedSearch? res = await NewPubMedSearchPreview(SearchSt);
            res.ItemsList.IncomingItems.Count.Should().BeGreaterThan(0);

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
        //The methods below are public to allow calling them from "Story-like" strings of tests
        public async Task<PubMedSearch?> NewPubMedSearchPreview(string SearchSt)
        {
            SingleStringCriteria data = new SingleStringCriteria() { Value = SearchSt };
            PubMedSearch? res = await client.PostAndDeserialize<PubMedSearch>("api/Sources/NewPubMedSearchPreview", data);
            res.Should().NotBeNull();
            return res;
        }
        public async Task<IncomingItemsListJSON?> SendThisRefsFile(string filename, string importFilterName, string endPoint)
        {
            string fileCont = File.ReadAllText(@"Files\" + filename);
            _ = fileCont.Should().NotBeNullOrEmpty();
            UploadOrCheckSource data = new UploadOrCheckSource() { fileContent = fileCont, importFilter = importFilterName, source_Name = filename };
            IncomingItemsListJSON? res = await client.PostAndDeserialize<IncomingItemsListJSON>("api/Sources/" + endPoint, data);
            res.Should().NotBeNull();
            return res;
        }
        public async Task<JsonNode?> GetSources()
        {
            JsonNode? res = await client.GetAndDeserialize("api/Sources/GetSources");
            res.Should().NotBeNull();
            return res;
        }
        public async Task<JsonNode?> DeleteUndeleteSource(int source_ID)
        {
            JsonNode? res = await client.PostAndDeserialize("api/Sources/DeleteUndeleteSource", new SingleIntCriteria() { Value = source_ID });
            res.Should().NotBeNull();
            return res;
        }
        public async Task<JsonNode?> DeleteSourceForever(int source_ID)
        {
            JsonNode? res = await client.PostAndDeserialize("api/Sources/DeleteSourceForever", new SingleIntCriteria() { Value = source_ID });
            res.Should().NotBeNull();
            return res;
        }
    }

}
