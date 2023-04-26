using BusinessLibrary.BusinessClasses;
using ERxWebClient2.Controllers;
using FluentAssertions;
using IntegrationTests.Fixtures;
using IntegrationTests.Utils;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace IntegrationTests.Story_Tests
{
    /// <summary>
    /// 1. Create review, upload 20 Items
    /// 2. Create screening tool
    /// 3. Code 20 items
    /// 4. Inspect result using report? frequency? not sure yet
    /// 

    /// </summary>
    [Collection("Database collection")]
    public class BuildDemoReviewStory : FixedLoginTest
    {
        protected override string username { get; set; }
        protected override string password { get; set; }
        protected override int ReviewId { get; set; }
        public BuildDemoReviewStory(ApiWebApplicationFactory fixture) : base(fixture)
        {
            username = "bob";
            password = "123";
            ReviewId = 12;
        }

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

            // create screening tool
            var codesetName = "Screen on T&A";
            var setTypeID = 5; // screening tool
            var allowCodingEdits = true; // not locked
            var codingIsFinal = true; // single coding
            JsonNode? CSRes = await CreateCodeset(codesetName, setTypeID, allowCodingEdits, codingIsFinal);

            // get the new setId and reviewSetId
            CSRes = await GetCodesets();
            var setId = (int)CSRes[0]["setId"];
            var reviewSetId = (int)CSRes[0]["reviewSetId"];

            // add an exclude code
            var attributeName = "Exclude 1";
            var attributeTypeId = 11; // exclude code
            JsonNode? AttrRes1 = await AddCode(setId, reviewSetId, attributeName, attributeTypeId);

            // add an exclude code
            attributeName = "Exclude 2";
            attributeTypeId = 11; // exclude code
            JsonNode? AttrRes2 = await AddCode(setId, reviewSetId, attributeName, attributeTypeId);

            // add an exclude code
            attributeName = "Exclude 3";
            attributeTypeId = 11; // exclude code
            JsonNode? AttrRes3 = await AddCode(setId, reviewSetId, attributeName, attributeTypeId);

            // add an include code
            attributeName = "Include 1";
            attributeTypeId = 10; // include code
            JsonNode? AttrRes4 = await AddCode(setId, reviewSetId, attributeName, attributeTypeId);

            CSRes = await GetCodesets();
            var attributeIdExc1 = int.Parse(CSRes[0]["attributes"]["attributesList"][0]["attributeId"].ToString());
            var attributeIdExc2 = int.Parse(CSRes[0]["attributes"]["attributesList"][1]["attributeId"].ToString());
            var attributeIdExc3 = int.Parse(CSRes[0]["attributes"]["attributesList"][2]["attributeId"].ToString());
            var attributeIdInc1 = int.Parse(CSRes[0]["attributes"]["attributesList"][0]["attributeId"].ToString());


            ReviewInfo? Res = await GetReviewInfo();

            // assign screening codes to 5 items
            var itemId = 1;
            var additionalText = "";
            JsonNode? item1 = await AddItemAttribute(Res, setId, attributeIdExc1, itemId, additionalText);
            itemId = 2;
            additionalText = "";
            JsonNode? item2 = await AddItemAttribute(Res, setId, attributeIdExc2, itemId, additionalText);
            itemId = 3;
            additionalText = "";
            JsonNode? item3 = await AddItemAttribute(Res, setId, attributeIdExc3, itemId, additionalText);
            itemId = 4;
            additionalText = "not sure about this one";
            JsonNode? item4 = await AddItemAttribute(Res, setId, attributeIdInc1, itemId, additionalText);
            itemId = 5;
            additionalText = "";
            JsonNode? item5 = await AddItemAttribute(Res, setId, attributeIdExc1, itemId, additionalText);


            // get items with this code...



            //var itemAttributeId = ghi["itemAttributeId"].ToString();
            //var itemSetId = ghi["itemSetId"].ToString();


        }


    }
}

namespace IntegrationTests.Fixtures
{
    public abstract partial class IntegrationTest : IClassFixture<ApiWebApplicationFactory>
    {
        public async Task<JsonNode?> AddItemAttribute(ReviewInfo Res, int setId, int attributeId, int itemId, string additionalText)
        {
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
            abc.additionalText = additionalText;
            abc.attributeId = attributeId;
            abc.itemArmId = 0;
            abc.itemAttributeId = 0;
            abc.itemId = itemId; 
            abc.itemSetId = 0;
            abc.saveType = "Insert";
            abc.setId = setId;
            abc.revInfo = def;

            JsonNode? res = await client.PostAndDeserialize("api/ItemSetList/ExcecuteItemAttributeSaveCommand", abc);
            res.Should().NotBeNull();
            return res;
        }
    }
}
    