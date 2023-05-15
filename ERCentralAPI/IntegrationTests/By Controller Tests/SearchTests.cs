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
using System.Composition;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace IntegrationTests.By_Controller_Tests
{
    [Collection("Database collection")]
    public class SearchTests : FixedLoginTest
    {
        //await InnerLoginToReview("bob", "123", 12);
        protected override string username { get; set; }
        protected override string password { get; set; }
        protected override int ReviewId { get; set; }

        public SearchTests(ApiWebApplicationFactory fixture) : base(fixture)
        {
            username = "bob";
            password = "123";
            ReviewId = 12;
        }


        [Fact]
        public async Task SearchesGetSearches()
        {
            //The line below ensures we log on with the default values for this class.
            //every [Fact] should start with this line, so to ensure authentication is done (once, for the whole class).
            (await AuthenticationDone()).Should().Be(true);
            ReviewInfo? Res = await GetReviewInfo();
            Res.ReviewId.Should().Be(12);
            Res.ReviewName.Should().Be("Shared rev1 (id:12)");

            JsonNode? SRes = await GetSearches();
            // assuming this is the first time you count searches...
            SRes.AsArray().Count().Should().Be(0);

        }

        [Theory]
        [InlineData("20 refs.txt")]
        public async Task SearchesInternalIDs(string filename, string importFilterName = "RIS")
        {
            //every [Fact] should start with this line
            (await AuthenticationDone()).Should().Be(true);

            //upload the file, check the answer
            string fileCont = File.ReadAllText(@"Files\" + filename);
            _ = fileCont.Should().NotBeNullOrEmpty();
            UploadOrCheckSource data = new UploadOrCheckSource() { fileContent = fileCont, importFilter = importFilterName, source_Name = filename };
            IncomingItemsListJSON? SOres = await client.PostAndDeserialize<IncomingItemsListJSON>("api/Sources/UploadSource", data);
            SOres.Should().NotBeNull();
            SOres.totalReferences.Should().Be(20);
            SOres.incomingItems.Count.Should().Be(0);

            // set up the initial search parameters
            var search1 = new CodeCommand();
            search1._searches = "";
            search1._logicType = "";
            search1._setID = 0;
            search1._searchText = "";
            search1._IDs = "10";  // to be set
            search1._title = "10";  // to be set
            search1._answers = "";
            search1._included = true;
            search1._withCodes = false;
            search1._searchId = 0;
            search1._contactId = 0;
            search1._contactName = "";
            search1._searchType = "";
            search1._scoreOne = 0;
            search1._scoreTwo = 0;
            search1._sourceIds = "";
            search1._searchWhat = "";

            // run the search
            JsonNode? SRes = await SearchInternalIDs(search1);
            SRes.ToString().Should().Be("1");

            JsonNode? ASRes = await GetSearches();
            ASRes.AsArray().Count().Should().Be(1);
            ASRes[0]["hitsNo"].ToString().Should().Be("1");
            

            // try comma separated IDs
            search1._IDs = "10,11,12";
            search1._title = "10,11,12";

            // run the search
            SRes = await SearchInternalIDs(search1);
            SRes.ToString().Should().Be("2");

            // check the HitsNo of the second search
            ASRes = await GetSearches();
            ASRes.AsArray().Count().Should().Be(2);
            ASRes[1]["hitsNo"].ToString().Should().Be("3");
            

            // cleanup -  delete search 1
            var search_ID = ASRes[1]["searchId"].ToString();
            JsonNode? res3 = await SearchDelete(search_ID);
            res3.Should().NotBeNull();

            // cleanup -  delete search 0
            search_ID = ASRes[0]["searchId"].ToString();
            res3 = await SearchDelete(search_ID);
            res3.Should().NotBeNull();

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
        public async Task<JsonNode?> GetSearches()
        {
            JsonNode? res = await client.GetAndDeserialize("api/SearchList/GetSearches");
            res.Should().NotBeNull();
            return res;
        }

        public async Task<JsonNode?> SearchInternalIDs(CodeCommand search1)
        {
            JsonNode? res = await client.PostAndDeserialize("api/SearchList/SearchIDs", search1);
            res.Should().NotBeNull();
            return res;
        }

        public async Task<JsonNode?> SearchDelete(string search_id)
        {
            JsonNode? res = await client.PostAndDeserialize("api/SearchList/DeleteSearch", new SingleStringCriteria() { Value = search_id });
            res.Should().NotBeNull();
            return res;
        }
    }
}
