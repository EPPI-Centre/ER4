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

namespace IntegrationTests
{
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
        //
        [Fact]
        public async Task VerifyFile()
        {
            (await AuthenticationDone()).Should().Be(true);
            string f20refs = File.ReadAllText(@"Files\20 refs.txt");
            _ = f20refs.Should().NotBeNullOrEmpty();
            UploadOrCheckSource data = new UploadOrCheckSource() { fileContent = f20refs, importFilter = "RIS", source_Name = "20 refs.txt" };
            IncomingItemsListJSON? res = await _client.PostAndDeserialize<IncomingItemsListJSON>("api/Sources/VerifyFile", data);
            res.Should().NotBeNull();
            res.totalReferences.Should().Be(20);
            res.incomingItems.Count.Should().Be(20);
            res.incomingItems[0].Title.Should().Be("Behavioral activation and therapeutic exposure for bereavement in older adults.");
        }
        [Fact]
        public async Task UploadSourceThenDeleteIt()
        {
            (await AuthenticationDone()).Should().Be(true);
            string f20refs = File.ReadAllText(@"Files\20 refs.txt");
            _ = f20refs.Should().NotBeNullOrEmpty();
            UploadOrCheckSource data = new UploadOrCheckSource() { fileContent = f20refs, importFilter = "RIS", source_Name= "20 refs.txt" };
            IncomingItemsListJSON? res = await _client.PostAndDeserialize<IncomingItemsListJSON>("api/Sources/UploadSource", data);
            res.Should().NotBeNull();
            res.totalReferences.Should().Be(20);
            res.incomingItems.Count.Should().Be(0);
            
            JsonNode? res2 = await _client.GetAndDeserialize("api/Sources/GetSources");
            res2.Should().NotBeNull();
            res2["sources"].Should().NotBeNull();
            JsonArray? ross = (JsonArray ? )res2["sources"]  ;
            ross.Should().NotBeNull();
            ross.Count.Should().Be(2);
            ross[0]["source_Name"].Should().NotBeNull();
            ross[0]["source_Name"].ToString().Should().Be("20 refs.txt");
            int source_ID = (int)ross[0]["source_ID"];
            JsonNode? res3 = await _client.PostAndDeserialize("api/Sources/DeleteUndeleteSource", new SingleIntCriteria() { Value = source_ID } );
            res3.Should().NotBeNull();
            res3 = await _client.PostAndDeserialize("api/Sources/DeleteSourceForever", new SingleIntCriteria() { Value = source_ID });
            res3.Should().NotBeNull();
        }
        [Fact]
        public async Task NewPubMedSearchPreview()
        {
            (await AuthenticationDone()).Should().Be(true); 
            SingleStringCriteria data = new SingleStringCriteria() { Value = "Pippo" };
            PubMedSearch? res = await _client.PostAndDeserialize<PubMedSearch>("api/Sources/NewPubMedSearchPreview", data);
            res.Should().NotBeNull();
            res.ItemsList.IncomingItems.Count.Should().BeGreaterThan(0);
            
        }
    }

}
