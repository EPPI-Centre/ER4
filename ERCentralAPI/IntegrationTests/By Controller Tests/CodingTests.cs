using BusinessLibrary.BusinessClasses;
using CsvHelper.TypeConversion;
using ERxWebClient2.Controllers;
using FluentAssertions;
using FluentAssertions.Primitives;
using IntegrationTests.Fixtures;
using IntegrationTests.Utils;
using Microsoft.Azure.DataLake.Store;
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
    public class CodingTests : FixedLoginTest
    {
        //await InnerLoginToReview("bob", "123", 12);
        protected override string username { get; set; }
        protected override string password { get; set; }
        protected override int ReviewId { get; set; }

        public CodingTests(ApiWebApplicationFactory fixture) : base(fixture)
        {
            username = "bob";
            password = "123";
            ReviewId = 12;
        }

        /*
        [Theory]
        [InlineData("20 refs.txt")]
        public async Task CodingAssignCodeToItem(string filename, string importFilterName = "RIS")
        {
            //every [Fact] should start with this line
            (await AuthenticationDone()).Should().Be(true);

            //upload the file, check the answer
            string fileCont = File.ReadAllText(@"Files\" + filename);
            _ = fileCont.Should().NotBeNullOrEmpty();
            UploadOrCheckSource data = new UploadOrCheckSource() { fileContent = fileCont, importFilter = importFilterName, source_Name = filename };
            IncomingItemsListJSON? resS = await client.PostAndDeserialize<IncomingItemsListJSON>("api/Sources/UploadSource", data);
            resS.Should().NotBeNull();

            resS.totalReferences.Should().Be(20);
            resS.incomingItems.Count.Should().Be(0);
            //var itemID = resS.

            // create screening tool

            ReviewSetUpdateCommandJSON rsc = new ReviewSetUpdateCommandJSON();
            // settings for a new screening tool
            rsc.SetId = -1;
            rsc.ReviewSetId = -1;
            rsc.SetName = "test 1";
            rsc.setOrder = 0;
            rsc.setDescription = "";
            rsc.CodingIsFinal = true;
            rsc.AllowCodingEdits = true;
            rsc.SetTypeId = 5; // screening tool
            rsc.usersCanEditURLs = false;

            JsonNode? resCT = await client.PostAndDeserialize("api/Codeset/ReviewSetCreate", rsc);
            resCT.Should().NotBeNull();

            JsonNode? CSRes = await client.GetAndDeserialize("api/Codeset/CodesetsByReview");
            CSRes.AsArray().Count().Should().Be(1);
            var setID = (int)CSRes[0]["setId"];
            var reviewSetID = (int)CSRes[0]["reviewSetId"];

            // add a couple of codes to the tool
            AttributeSetCreateOrUpdateJSON incAttr = new AttributeSetCreateOrUpdateJSON();
            // settings for a new 'Include' code
            incAttr.setId = setID;
            incAttr.parentAttributeId = 0;
            incAttr.attributeTypeId = 10;
            incAttr.attributeOrder = 0;
            incAttr.attributeName = "Include 1";
            incAttr.attributeSetDescription = "";
            incAttr.contactId = 5;  // this is the default login person
            incAttr.originalAttributeID = 0;
            incAttr.attributeSetId = 0;
            incAttr.attributeId = 0;
            incAttr.extURL = "";
            incAttr.extType = "";

            AttributeSetCreateOrUpdateJSON excAttr = new AttributeSetCreateOrUpdateJSON();
            // settings for a new 'Exclude' code
            excAttr.setId = setID;
            excAttr.parentAttributeId = 0;
            excAttr.attributeTypeId = 11;
            excAttr.attributeOrder = 1;
            excAttr.attributeName = "Exclude 1";
            excAttr.attributeSetDescription = "";
            excAttr.contactId = 5;  // this is the default login person
            excAttr.originalAttributeID = 0;
            excAttr.attributeSetId = 0;
            excAttr.attributeId = 0;
            excAttr.extURL = "";
            excAttr.extType = "";

            JsonNode? resCI = await client.PostAndDeserialize("api/Codeset/AttributeCreate", incAttr);
            resCI.Should().NotBeNull();

            JsonNode? resCE = await client.PostAndDeserialize("api/Codeset/AttributeCreate", excAttr);
            resCE.Should().NotBeNull();

            JsonNode? res = await client.GetAndDeserialize("api/Codeset/CodesetsByReview");
            var attributeName = res[0]["attributes"]["attributesList"][1]["attributeName"].ToString();
            var attributeId = res[0]["attributes"]["attributesList"][1]["attributeId"].ToString();

            ReviewInfo? Res = await GetReviewInfo();
            MVCReviewInfo def = new MVCReviewInfo();
            def.bL_AUTH_CODE = Res.BL_AUTH_CODE;
            def.bL_ACCOUNT_CODE = Res.BL_ACCOUNT_CODE;
            def.bL_CC_AUTH_CODE = Res.BL_CC_AUTH_CODE;
            def.bL_CC_TX = Res.BL_CC_TX;
            def.bL_TX = Res.BL_TX;
            def.reviewId = Res.ReviewId;
            def.reviewName = Res.ReviewName;
            def.screeningAutoExclude = Res.ScreeningAutoExclude;
            def.screeningCodeSetId = Res.ScreeningCodeSetId;
            def.screeningIndexed = Res.ScreeningIndexed;
            def.screeningListIsGood = Res.ScreeningListIsGood;
            def.screeningMode = Res.ScreeningMode;
            def.screeningMode = Res.ScreeningMode;
            def.screeningModelRunning = Res.ScreeningModelRunning;
            def.screeningNPeople = Res.ScreeningNPeople;
            def.screeningReconcilliation = Res.ScreeningReconcilliation;

            MVCItemAttributeSaveCommand abc = new MVCItemAttributeSaveCommand();
            abc.additionalText = "";
            abc.attributeId = int.Parse(attributeId);
            abc.itemArmId = 0;
            abc.itemAttributeId = 0;
            abc.itemId = 3; // hardcoded itemId
            abc.itemSetId = 0;
            abc.saveType = "Insert";
            abc.setId = setID;
            abc.revInfo = def;

            // assign code to item
            JsonNode? ghi = await AddItemAttribute(abc);
            var itemAttributeId = ghi["itemAttributeId"].ToString();
            var itemSetId = ghi["itemSetId"].ToString();
            

            // update coding
            abc.additionalText = "some new interesting text...";
            abc.itemAttributeId = int.Parse(itemAttributeId);
            abc.itemSetId = int.Parse(itemSetId);
            abc.saveType = "Update";
            JsonNode? jkl = await UpdateItemAttribute(abc);


        }
        */


    }
}

namespace IntegrationTests.Fixtures
{
    public abstract partial class IntegrationTest : IClassFixture<ApiWebApplicationFactory>
    {
        
        /*
        public async Task<JsonNode?> AddItemAttribute(MVCItemAttributeSaveCommand rsc)
        {
            JsonNode? res = await client.PostAndDeserialize("api/ItemSetList/ExcecuteItemAttributeSaveCommand", rsc);
            res.Should().NotBeNull();
            return res;
        }

        public async Task<JsonNode?> UpdateItemAttribute(MVCItemAttributeSaveCommand rsc)
        {
            JsonNode? res = await client.PostAndDeserialize("api/ItemSetList/ExcecuteItemAttributeSaveCommand", rsc);
            res.Should().NotBeNull();
            return res;
        }
        */
    }
}
