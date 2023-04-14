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
    public class CodesetsTests : FixedLoginTest
    {
        //await InnerLoginToReview("bob", "123", 12);
        protected override string username { get; set; }
        protected override string password { get; set; }
        protected override int ReviewId { get; set; }

        public CodesetsTests(ApiWebApplicationFactory fixture) : base(fixture)
        {
            username = "bob";
            password = "123";
            ReviewId = 12;
        }



        [Fact]
        public async Task CodesetsGetCodesets()
        {
            //The line below ensures we log on with the default values for this class.
            //every [Fact] should start with this line, so to ensure authentication is done (once, for the whole class).
            (await AuthenticationDone()).Should().Be(true);
            ReviewInfo? Res = await GetReviewInfo();
            Res.ReviewId.Should().Be(12);
            Res.ReviewName.Should().Be("Shared rev1 (id:12)");

            JsonNode? CSRes = await GetCodesets();
            // assuming this is the first time you count searches...
            CSRes.AsArray().Count().Should().Be(0);

        }




        [Theory]
        [InlineData("20 refs.txt")]
        public async Task CodesetsCreateScreeningTool(string filename, string importFilterName = "RIS")
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

            /* ReviewSetUpdateCommandJSON fields
            public int SetId;
            public int ReviewSetId;
            public string SetName;
            public int setOrder;
            public string setDescription;
            public bool CodingIsFinal;//normal or comparison mode
            public bool AllowCodingEdits; //AllowCodingEdits can edit this codeset...
            public int SetTypeId;
            public bool usersCanEditURLs;
            */


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

            JsonNode? CSRes = await CreateCodeset(rsc);

            CSRes = await GetCodesets();
            CSRes.AsArray().Count().Should().Be(1);

            var setID = (int)CSRes[0]["setId"];
            var reviewSetID = (int)CSRes[0]["reviewSetId"];

            // edit the coding tool
            rsc.SetId = setID;
            rsc.SetName = "test 1111";
            rsc.ReviewSetId = reviewSetID;
            CSRes = await UpdateCodeset(rsc);

            CSRes = await GetCodesets();
            var test = CSRes[0]["setName"].ToString();


            /* AttributeSetCreateOrUpdateJSON fields
            public int setId;
            public Int64 parentAttributeId;
            public int attributeTypeId;
            public int attributeOrder;
            public string attributeName;
            public string attributeSetDescription;
            public int contactId;
            public Int64 originalAttributeID;
            public Int64 attributeSetId;
            public Int64 attributeId;
            public string extURL;
            public string extType;
            */


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

            JsonNode? AttrRes1 = await AddCode(incAttr);
            JsonNode? AttrRes2 = await AddCode(excAttr);

            CSRes = await GetCodesets();
            CSRes.AsArray().Count().Should().Be(1);

            test = CSRes[0]["attributes"]["attributesList"][0]["attributeName"].ToString();
            CSRes[0]["attributes"]["attributesList"][1]["attributeName"].ToString().Should().Be("Exclude 1");

            //update an attribute
            var attributeSetId = int.Parse(CSRes[0]["attributes"]["attributesList"][0]["attributeSetId"].ToString());
            var attributeId = int.Parse(CSRes[0]["attributes"]["attributesList"][0]["attributeId"].ToString());
            incAttr.attributeName = "Include 1111";
            incAttr.attributeSetId = attributeSetId;
            incAttr.attributeId = attributeId;
            JsonNode? AttrRes3 = await UpdateCode(incAttr);

            CSRes = await GetCodesets();
            test = CSRes[0]["attributes"]["attributesList"][0]["attributeName"].ToString();
            CSRes[0]["attributes"]["attributesList"][0]["attributeName"].ToString().Should().Be("Include 1111");

            // delete the attributes
            /* for AttributeDeleteCommandJSON
            public Int64 attributeSetId;
            public Int64 attributeId;
            public Int64 parentAttributeId;
            public int attributeOrder;
            public bool successful;
            */

            AttributeDeleteCommandJSON attrDelete = new AttributeDeleteCommandJSON();
            attrDelete.attributeSetId = int.Parse(CSRes[0]["attributes"]["attributesList"][0]["attributeSetId"].ToString());
            attrDelete.attributeId = int.Parse(CSRes[0]["attributes"]["attributesList"][0]["attributeId"].ToString());
            attrDelete.parentAttributeId = int.Parse(CSRes[0]["attributes"]["attributesList"][0]["parentAttributeId"].ToString());
            attrDelete.attributeOrder = int.Parse(CSRes[0]["attributes"]["attributesList"][0]["attributeOrder"].ToString());
            attrDelete.successful = false;

            JsonNode? AttrRes4 = await DeleteCode(attrDelete);
            CSRes = await GetCodesets();

            // the 'first' code has been deleted so the 'second' code is now the 'first' code.
            attrDelete.attributeSetId = int.Parse(CSRes[0]["attributes"]["attributesList"][0]["attributeSetId"].ToString());
            attrDelete.attributeId = int.Parse(CSRes[0]["attributes"]["attributesList"][0]["attributeId"].ToString());
            attrDelete.parentAttributeId = int.Parse(CSRes[0]["attributes"]["attributesList"][0]["parentAttributeId"].ToString());
            attrDelete.attributeOrder = int.Parse(CSRes[0]["attributes"]["attributesList"][0]["attributeOrder"].ToString());
            attrDelete.successful = false;

            JsonNode? AttrRes5 = await DeleteCode(attrDelete);
            CSRes = await GetCodesets();

            // delete the codesets
            /* needed for ReviewSetDeleteCommandJSON
            public Int64 reviewSetId;
            public bool successful;
            public int setId;
            public int order;
            */
            ReviewSetDeleteCommandJSON rsDelete = new ReviewSetDeleteCommandJSON();
            rsDelete.reviewSetId = (int)CSRes[0]["reviewSetId"]; 
            rsDelete.successful = false;
            rsDelete.setId = (int)CSRes[0]["setId"];  
            rsDelete.order = (int)CSRes[0]["setOrder"]; 
            
            JsonNode? CSRes5 = await DeleteCodeset(rsDelete);

            CSRes = await GetCodesets();
            CSRes.AsArray().Count().Should().Be(0);

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
        public async Task<JsonNode?> GetCodesets()
        {
            JsonNode? res = await client.GetAndDeserialize("api/Codeset/CodesetsByReview");
            res.Should().NotBeNull();
            return res;
        }
        
        public async Task<JsonNode?> CreateCodeset(ReviewSetUpdateCommandJSON rsc)
        {
            JsonNode? res = await client.PostAndDeserialize("api/Codeset/ReviewSetCreate", rsc);
            res.Should().NotBeNull();
            return res;
        }
        public async Task<JsonNode?> UpdateCodeset(ReviewSetUpdateCommandJSON rsc)
        {
            JsonNode? res = await client.PostAndDeserialize("api/Codeset/SaveReviewSet", rsc);
            res.Should().NotBeNull();
            return res;
        }
        public async Task<JsonNode?> DeleteCodeset(ReviewSetDeleteCommandJSON rsc)
        {
            JsonNode? res = await client.PostAndDeserialize("api/Codeset/ReviewSetDelete", rsc);
            res.Should().NotBeNull();
            return res;
        }


        public async Task<JsonNode?> AddCode(AttributeSetCreateOrUpdateJSON rsc)
        {
            JsonNode? res = await client.PostAndDeserialize("api/Codeset/AttributeCreate", rsc);
            res.Should().NotBeNull();
            return res;
        }
        public async Task<JsonNode?> UpdateCode(AttributeSetCreateOrUpdateJSON rsc)
        {
            JsonNode? res = await client.PostAndDeserialize("api/Codeset/AttributeUpdate", rsc);
            res.Should().NotBeNull();
            return res;
        }

        public async Task<JsonNode?> DeleteCode(AttributeDeleteCommandJSON rsc)
        {
            JsonNode? res = await client.PostAndDeserialize("api/Codeset/AttributeDelete", rsc);
            res.Should().NotBeNull();
            return res;
        }

        



    }
}


