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
    public class ItemListTests : FixedLoginTest
    {
        protected override string username { get; set; }
        protected override string password { get; set; }
        protected override int ReviewId { get; set; }
        public ItemListTests(ApiWebApplicationFactory fixture) : base(fixture)
        {
            username = "bob";
            password = "123";
            ReviewId = 12;
        }
    
        [Theory]
        [InlineData("20 refs.txt")]
        public async Task UploadSourceGetAllItems(string filename, string importFilterName = "RIS")
        {
            (await AuthenticationDone()).Should().Be(true);
            //first, find out what's in there... expect nothing, so one empty source for manually created items
            JsonNode? srcs = await GetSources();
            srcs["sources"].Should().NotBeNull();
            JsonArray? ross = (JsonArray?)srcs["sources"];
            ross.Should().NotBeNull();
            ross.Count.Should().BeGreaterThanOrEqualTo(1);
            int CurrentSrcsCount = ross.Count;
            ross[0]["source_Name"].Should().NotBeNull();
            int CurrentSourcesCount = ross.Count;

            //add 20 items - we need something "to get"!
            IncomingItemsListJSON res = await SendThisRefsFile(filename, importFilterName, "UploadSource");
            res.Should().NotBeNull();
            res.totalReferences.Should().Be(20);
            res.incomingItems.Count.Should().Be(0);

            //get sources again, expect one more!
            srcs = await GetSources();
            srcs["sources"].Should().NotBeNull();
            ross = (JsonArray?)srcs["sources"];
            ross.Should().NotBeNull();
            ross.Count.Should().BeGreaterThan(CurrentSrcsCount);

            //get all items, expect 20 or more
            JsonNode? ItemList = await FetchIncludedItems();
            int itemCount = (int)ItemList["totalItemCount"];
            itemCount.Should().BeGreaterThanOrEqualTo(20);


            //get first 5 items, expect 5!
            ItemList = await FetchIncludedItems(5);
            JsonArray? items = (JsonArray?)ItemList["items"];
            items.Should().NotBeNull();
            itemCount = items.Count;
            itemCount.Should().Be(5);

            //get the ItemId of the 1st item, expect it to be greater than 1
            var itemId = (int?)items[0]["itemId"];
            itemId.Should().NotBeNull();
            itemId.Should().BeGreaterThan(1);
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
        
        public async Task<JsonNode?> FetchIncludedItems(int PageSize = 100)
        {
            SelCritMVC crit = new SelCritMVC();
            crit.onlyIncluded = true;
            crit.showDeleted = false;
            crit.sourceId = 0;
            crit.searchId = 0;
            crit.xAxisSetId = 0;
            crit.xAxisAttributeId = 0;
            crit.yAxisSetId = 0;
            crit.yAxisAttributeId = 0;
            crit.filterSetId = 0;
            crit.filterAttributeId = 0;
            crit.attributeSetIdList = "";
            crit.listType = "StandardItemList";
            crit.pageNumber = 0;
            crit.pageSize = PageSize;
            crit.totalItems = 0;
            crit.startPage = 0;
            crit.endPage = 0;
            crit.startIndex = 0;
            crit.endIndex = 0;
            crit.workAllocationId = 0;
            crit.comparisonId = 0;

            crit.magSimulationId = 0;
            crit.description = "";
            crit.contactId = 0;
            crit.setId = 0;
            crit.showInfoColumn = true;
            crit.showScoreColumn = false;

            crit.withOutAttributesIdsList = "";
            crit.withAttributesIds = "";
            crit.withSetIdsList = "";
            crit.withOutSetIdsList = "";

            JsonNode? res = await client.PostAndDeserialize("api/ItemList/Fetch", crit);
            res.Should().NotBeNull();
            return res;
        }
        public async Task<JsonNode?> FetchItemsWithThisCode(string attributeSetIdList)
        {
            SelCritMVC crit = new SelCritMVC();
            crit.onlyIncluded = true;
            crit.showDeleted = false;
            crit.sourceId = 0;
            crit.searchId = 0;
            crit.xAxisSetId = 0;
            crit.xAxisAttributeId = 0;
            crit.yAxisSetId = 0;
            crit.yAxisAttributeId = 0;
            crit.filterSetId = 0;
            crit.filterAttributeId = 0;
            crit.attributeSetIdList = attributeSetIdList; // this is what to search for. It can be a list of items.
            crit.listType = "StandardItemList";
            crit.pageNumber = 0;
            crit.pageSize = 100;
            crit.totalItems = 0;
            crit.startPage = 0;
            crit.endPage = 0;
            crit.startIndex = 0;
            crit.endIndex = 0;
            crit.workAllocationId = 0;
            crit.comparisonId = 0;

            crit.magSimulationId = 0;
            crit.description = "";
            crit.contactId = 0;
            crit.setId = 0;
            crit.showInfoColumn = true;
            crit.showScoreColumn = false;

            crit.withOutAttributesIdsList = "";
            crit.withAttributesIds = "";
            crit.withSetIdsList = "";
            crit.withOutSetIdsList = "";



            JsonNode? res = await client.PostAndDeserialize("api/ItemList/Fetch", crit);
            res.Should().NotBeNull();
            return res;


        }
    }
}
