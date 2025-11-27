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
            CSRes.Should().NotBeNull();
            CSRes.AsArray().Count().Should().BeGreaterThanOrEqualTo(0);

        }




        [Fact]
        public async Task CRUDScreeningToolandAttributes()
        {
            //every [Fact] should start with this line
            (await AuthenticationDone()).Should().Be(true);



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

            /* default values
            ReviewSetUpdateCommandJSON rsc = new ReviewSetUpdateCommandJSON();
            rsc.SetId = -1;
            rsc.ReviewSetId = -1;
            rsc.SetName = "test 1";
            rsc.setOrder = 0;
            rsc.setDescription = "";
            rsc.CodingIsFinal = true;
            rsc.AllowCodingEdits = true;
            rsc.SetTypeId = 5; // screening tool
            rsc.usersCanEditURLs = false;
            */

            var codesetName = "test 1";
            var setTypeID = 5; // screening tool
            var allowCodingEdits = true;
            var codingIsFinal = true; // single coding
            JsonNode? CSRes = await CreateCodeset(codesetName, setTypeID, allowCodingEdits, codingIsFinal);

            CSRes = await GetCodesets();
            CSRes.AsArray().Count().Should().BeGreaterThanOrEqualTo(1);
            var ourTool = CSRes.AsArray().FirstOrDefault(f => f["setName"].ToString() == codesetName);
            ourTool.Should().NotBeNull();
            var setId = (int)ourTool["setId"];
            var reviewSetId = (int)ourTool["reviewSetId"];
            codesetName = "test 1111";
            // edit the coding tool name
            ReviewSetUpdateCommandJSON rsc = new ReviewSetUpdateCommandJSON();
            rsc.setId = setId;
            rsc.setName = codesetName;
            rsc.reviewSetId = reviewSetId;
            rsc.allowCodingEdits = (bool)ourTool["allowCodingEdits"];
            rsc.setTypeId = 5;
            rsc.setDescription = "";

            CSRes = await UpdateCodeset(rsc);

            CSRes = await GetCodesets();
            var test = ourTool["setName"].ToString();


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

            // add an exclude code
            var attributeName = "Exclude 1";
            var attributeTypeId = 10; // exclude code
            var parentAtrributeId = 0;
            JsonNode? AttrRes1 = await AddCode(setId, reviewSetId, parentAtrributeId, attributeName, attributeTypeId);

            // add an include code
            attributeName = "Include 1";
            attributeTypeId = 11; // include code
            JsonNode? AttrRes2 = await AddCode(setId, reviewSetId, parentAtrributeId, attributeName, attributeTypeId);


            /* 
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

            JsonNode? AttrRes1 = await AddCode(incAttr);

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

            
            JsonNode? AttrRes2 = await AddCode(excAttr);
            */


            
            CSRes = await GetCodesets();
            CSRes.AsArray().Count().Should().BeGreaterThanOrEqualTo(1);

            ourTool = CSRes.AsArray().FirstOrDefault(f => f["setName"].ToString() == codesetName);
            ourTool.Should().NotBeNull();

            test = ourTool["attributes"]["attributesList"][0]["attributeName"].ToString();
            ourTool["attributes"]["attributesList"][1]["attributeName"].ToString().Should().Be("Include 1");
            


            //update an attribute
            AttributeSetCreateOrUpdateJSON incAttr = new AttributeSetCreateOrUpdateJSON();
            incAttr.setId = setId;
            incAttr.parentAttributeId = 0;
            incAttr.attributeTypeId = 10;
            incAttr.attributeOrder = 1;
            incAttr.attributeName = "Include 1111";
            incAttr.attributeSetDescription = "";
            incAttr.contactId = 5;  // this is the default login person
            incAttr.originalAttributeID = 0;
            incAttr.attributeSetId = int.Parse(ourTool["attributes"]["attributesList"][1]["attributeSetId"].ToString());
            incAttr.attributeId = int.Parse(ourTool["attributes"]["attributesList"][1]["attributeId"].ToString());
            incAttr.extURL = "";
            incAttr.extType = "";

            JsonNode? AttrRes3 = await UpdateCode(incAttr);

            CSRes = await GetCodesets();

            ourTool = CSRes.AsArray().FirstOrDefault(f => f["setName"].ToString() == codesetName);
            ourTool.Should().NotBeNull();

            test = ourTool["attributes"]["attributesList"][0]["attributeName"].ToString();
            ourTool["attributes"]["attributesList"][1]["attributeName"].ToString().Should().Be("Include 1111");
            



            // delete the attributes
            
            
            /* for AttributeDeleteCommandJSON
            public Int64 attributeSetId;
            public Int64 attributeId;
            public Int64 parentAttributeId;
            public int attributeOrder;
            public bool successful;
            */

            CSRes = await GetCodesets();
            ourTool = CSRes.AsArray().FirstOrDefault(f => f["setName"].ToString() == codesetName);
            ourTool.Should().NotBeNull();

            AttributeDeleteCommandJSON attrDelete = new AttributeDeleteCommandJSON();
            attrDelete.attributeSetId = int.Parse(ourTool["attributes"]["attributesList"][0]["attributeSetId"].ToString());
            attrDelete.attributeId = int.Parse(ourTool["attributes"]["attributesList"][0]["attributeId"].ToString());
            attrDelete.parentAttributeId = int.Parse(ourTool["attributes"]["attributesList"][0]["parentAttributeId"].ToString());
            attrDelete.attributeOrder = int.Parse(ourTool["attributes"]["attributesList"][0]["attributeOrder"].ToString());
            attrDelete.successful = false;

            JsonNode? AttrRes4 = await DeleteCode(attrDelete);
            CSRes = await GetCodesets();
            ourTool = CSRes.AsArray().FirstOrDefault(f => f["setName"].ToString() == codesetName);
            ourTool.Should().NotBeNull();

            // the 'first' code has been deleted so the 'second' code is now the 'first' code.
            attrDelete.attributeSetId = int.Parse(ourTool["attributes"]["attributesList"][0]["attributeSetId"].ToString());
            attrDelete.attributeId = int.Parse(ourTool["attributes"]["attributesList"][0]["attributeId"].ToString());
            attrDelete.parentAttributeId = int.Parse(ourTool["attributes"]["attributesList"][0]["parentAttributeId"].ToString());
            attrDelete.attributeOrder = int.Parse(ourTool["attributes"]["attributesList"][0]["attributeOrder"].ToString());
            attrDelete.successful = false;

            JsonNode? AttrRes5 = await DeleteCode(attrDelete);
            CSRes = await GetCodesets();
            int InterimToolsCount = CSRes.AsArray().Count();
            ourTool = CSRes.AsArray().FirstOrDefault(f => f["setName"].ToString() == codesetName);
            ourTool.Should().NotBeNull();

            // delete the codesets
            /* needed for ReviewSetDeleteCommandJSON
            public Int64 reviewSetId;
            public bool successful;
            public int setId;
            public int order;
            */
            ReviewSetDeleteCommandJSON rsDelete = new ReviewSetDeleteCommandJSON();
            rsDelete.reviewSetId = (int)ourTool["reviewSetId"]; 
            rsDelete.successful = false;
            rsDelete.setId = (int)ourTool["setId"];  
            rsDelete.order = (int)ourTool["setOrder"]; 
            
            JsonNode? CSRes5 = await DeleteCodeset(rsDelete);

            CSRes = await GetCodesets();
            CSRes.AsArray().Count().Should().Be(InterimToolsCount - 1);



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
        
        public async Task<JsonNode?> CreateCodeset(string setName, int setTypeId, bool allowCodingEdits, bool codingIsFinal)
        {
            ReviewSetUpdateCommandJSON newCT = new ReviewSetUpdateCommandJSON();
            // settings for a new screening tool
            newCT.setId = -1;
            newCT.reviewSetId = -1;
            newCT.setName = setName;
            newCT.setOrder = 0;
            newCT.setDescription = "";
            newCT.codingIsFinal = codingIsFinal;
            newCT.allowCodingEdits = allowCodingEdits;
            newCT.setTypeId = setTypeId; 
            newCT.usersCanEditURLs = false;


            JsonNode? res = await client.PostAndDeserialize("api/Codeset/ReviewSetCreate", newCT);
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


        public async Task<JsonNode?> AddCode(int setId, int reviewSetId, int parentAtrributeId, string attributeName, int attributeTypeId)
        {
            AttributeSetCreateOrUpdateJSON attr = new AttributeSetCreateOrUpdateJSON();
            attr.setId = setId;
            attr.parentAttributeId = parentAtrributeId;
            attr.attributeTypeId = attributeTypeId;
            attr.attributeOrder = 0;
            attr.attributeName = attributeName;
            attr.attributeSetDescription = "";
            attr.contactId = 5;  // this is the default login person
            attr.originalAttributeID = 0;
            attr.attributeSetId = 0;
            attr.attributeId = 0;
            attr.extURL = "";
            attr.extType = "";

            JsonNode? res = await client.PostAndDeserialize("api/Codeset/AttributeCreate", attr);
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


