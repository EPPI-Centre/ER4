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
using static Csla.Security.MembershipIdentity;

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
        [InlineData("20 demo refs.txt")]
        public async Task BuildDemoReview(string filename, string importFilterName = "RIS")
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


            /*
             * Create the coding tools
             * 1. Screen on title and abstract
             * 2. Screen on full report
             * 3. Data extraction tool
             * 
             */

            // 1.
            // create a 'screening on title and abstract' tool
            var codesetName = "Screen on Title & Abstract";
            var setTypeID = 5; // screening tool
            var allowCodingEdits = true; // not locked
            var codingIsFinal = true; // single coding
            JsonNode? CSRes = await CreateCodeset(codesetName, setTypeID, allowCodingEdits, codingIsFinal);

            // get the new setId and reviewSetId
            CSRes = await GetCodesets();
            var setIdTA = (int)CSRes[0]["setId"];
            var reviewSetIdTA = (int)CSRes[0]["reviewSetId"];

            JsonNode? CreateAttrRes;
            // create an exclude code
            var attributeName = "EXCLUDE on date";
            var attributeTypeId = 11; // exclude code
            var parentAtrributeId = 0;
            CreateAttrRes = await AddCode(setIdTA, reviewSetIdTA, parentAtrributeId,attributeName, attributeTypeId);

            // create an exclude code
            attributeName = "EXCLUDE on country";
            attributeTypeId = 11; // exclude code
            CreateAttrRes = await AddCode(setIdTA, reviewSetIdTA, parentAtrributeId, attributeName, attributeTypeId);

            // create an exclude code
            attributeName = "EXCLUDE on topic";
            attributeTypeId = 11; // exclude code
            CreateAttrRes = await AddCode(setIdTA, reviewSetIdTA, parentAtrributeId, attributeName, attributeTypeId);

            // create an exclude code
            attributeName = "EXCLUDE on intervention";
            attributeTypeId = 11; // exclude code
            CreateAttrRes = await AddCode(setIdTA, reviewSetIdTA, parentAtrributeId, attributeName, attributeTypeId);

            // create an include code
            attributeName = "INCLUDE based on title & abstract";
            attributeTypeId = 10; // include code
            CreateAttrRes = await AddCode(setIdTA, reviewSetIdTA, parentAtrributeId, attributeName, attributeTypeId);
          
            CSRes = await GetCodesets();
            var attributeIdExc1TA = int.Parse(CSRes[0]["attributes"]["attributesList"][0]["attributeId"].ToString());
            var attributeIdExc2TA = int.Parse(CSRes[0]["attributes"]["attributesList"][1]["attributeId"].ToString());
            var attributeIdExc3TA = int.Parse(CSRes[0]["attributes"]["attributesList"][2]["attributeId"].ToString());
            var attributeIdExc4TA = int.Parse(CSRes[0]["attributes"]["attributesList"][3]["attributeId"].ToString());
            var attributeIdInc1TA = int.Parse(CSRes[0]["attributes"]["attributesList"][4]["attributeId"].ToString());


            // 2.
            // create a 'screening on full report' tool
            codesetName = "Screen on full report";
            setTypeID = 5; // screening tool
            allowCodingEdits = true; // not locked
            codingIsFinal = true; // single coding
            CSRes = await CreateCodeset(codesetName, setTypeID, allowCodingEdits, codingIsFinal);

            // get the new setId and reviewSetId
            CSRes = await GetCodesets();
            var setIdFR = (int)CSRes[1]["setId"];
            var reviewSetIdFR = (int)CSRes[1]["reviewSetId"];

            // create an exclude code
            parentAtrributeId = 0;
            attributeName = "EXCLUDE on date";
            attributeTypeId = 11; // exclude code
            CreateAttrRes = await AddCode(setIdFR, reviewSetIdFR, parentAtrributeId, attributeName, attributeTypeId);

            // create an exclude code
            attributeName = "EXCLUDE on country";
            attributeTypeId = 11; // exclude code
            CreateAttrRes = await AddCode(setIdFR, reviewSetIdFR, parentAtrributeId, attributeName, attributeTypeId);

            // create an exclude code
            attributeName = "EXCLUDE on topic";
            attributeTypeId = 11; // exclude code
            CreateAttrRes = await AddCode(setIdFR, reviewSetIdFR, parentAtrributeId, attributeName, attributeTypeId);

            // create an exclude code
            attributeName = "EXCLUDE on intervention";
            attributeTypeId = 11; // exclude code
            CreateAttrRes = await AddCode(setIdFR, reviewSetIdFR, parentAtrributeId, attributeName, attributeTypeId);

            // create an include code
            attributeName = "INCLUDE based on full report";
            attributeTypeId = 10; // include code
            CreateAttrRes = await AddCode(setIdFR, reviewSetIdFR, parentAtrributeId, attributeName, attributeTypeId);

            CSRes = await GetCodesets();
            var attributeIdExc1FR = int.Parse(CSRes[1]["attributes"]["attributesList"][0]["attributeId"].ToString());
            var attributeIdExc2FR = int.Parse(CSRes[1]["attributes"]["attributesList"][1]["attributeId"].ToString());
            var attributeIdExc3FR = int.Parse(CSRes[1]["attributes"]["attributesList"][2]["attributeId"].ToString());
            var attributeIdExc4FR = int.Parse(CSRes[1]["attributes"]["attributesList"][3]["attributeId"].ToString());
            var attributeIdInc1FR = int.Parse(CSRes[1]["attributes"]["attributesList"][4]["attributeId"].ToString());


            // 3.
            // create a data extraction tool
            codesetName = "Data extraction tool";
            setTypeID = 3; // standard tool
            allowCodingEdits = true; // not locked
            codingIsFinal = true; // single coding
            CSRes = await CreateCodeset(codesetName, setTypeID, allowCodingEdits, codingIsFinal);

            // get the new setId and reviewSetId
            CSRes = await GetCodesets();
            var setIdDE = (int)CSRes[2]["setId"];
            var reviewSetIdDE = (int)CSRes[2]["reviewSetId"];

            ////////////////////////////////////////////////////////////////////
            // create the S1 level code
            attributeName = "How can the study be identified";
            attributeTypeId = 1; // not selectable
            parentAtrributeId = 0;
            CreateAttrRes = await AddCode(setIdDE, reviewSetIdDE, parentAtrributeId, attributeName, attributeTypeId);

            CSRes = await GetCodesets();
            var S1AttributeId = int.Parse(CSRes[2]["attributes"]["attributesList"][0]["attributeId"].ToString());

            // create the S1Q1 question code
            attributeName = "Which search strategy was used to identifiy this report?";
            attributeTypeId = 1; // not selectable
            var S1Q1ParentAttributeId = S1AttributeId;
            CreateAttrRes = await AddCode(setIdDE, reviewSetIdDE, S1Q1ParentAttributeId, attributeName, attributeTypeId);
            CSRes = await GetCodesets();
            var S1Q1AttributeId = int.Parse(CSRes[2]["attributes"]["attributesList"][0]["attributes"]["attributesList"][0]["attributeId"].ToString());
            var S1Q1AxParentAttributeID = S1Q1AttributeId;

            // create the S1Q2 question code
            attributeName = "Language of the report";
            attributeTypeId = 1; // not selectable
            CreateAttrRes = await AddCode(setIdDE, reviewSetIdDE, S1AttributeId, attributeName, attributeTypeId);
            CSRes = await GetCodesets();
            var S1Q2AttributeId = int.Parse(CSRes[2]["attributes"]["attributesList"][0]["attributes"]["attributesList"][1]["attributeId"].ToString());
            var S1Q2AxParentAttributeId = S1Q2AttributeId;

            // create the S1Q3 question code
            attributeName = "Status of report";
            attributeTypeId = 1; // not selectable
            var S1Q3ParentAttributeId = S1Q1ParentAttributeId;
            CreateAttrRes = await AddCode(setIdDE, reviewSetIdDE, S1AttributeId, attributeName, attributeTypeId);
            CSRes = await GetCodesets();
            var S1Q3AttributeId = int.Parse(CSRes[2]["attributes"]["attributesList"][0]["attributes"]["attributesList"][2]["attributeId"].ToString());
            var S1Q3AxParentAttributeId = S1Q3AttributeId;


            // create the S1Q4 question code
            attributeName = "Year of report";
            attributeTypeId = 1; // not selectable
            var S1Q4ParentAttributeId = S1Q1ParentAttributeId;
            CreateAttrRes = await AddCode(setIdDE, reviewSetIdDE, S1Q4ParentAttributeId, attributeName, attributeTypeId);
            CSRes = await GetCodesets();
            var S1Q4AttributeId = int.Parse(CSRes[2]["attributes"]["attributesList"][0]["attributes"]["attributesList"][3]["attributeId"].ToString());
            var S1Q4AxParentAttributeId = S1Q4AttributeId;


            // create the S1Q5 question code
            attributeName = "Abstract/Summary of key findings";
            attributeTypeId = 1; // not selectable
            var S1Q5ParentAttributeId = S1Q1ParentAttributeId;
            CreateAttrRes = await AddCode(setIdDE, reviewSetIdDE, S1Q5ParentAttributeId, attributeName, attributeTypeId);
            CSRes = await GetCodesets();
            var S1Q5AttributeId = int.Parse(CSRes[2]["attributes"]["attributesList"][0]["attributes"]["attributesList"][4]["attributeId"].ToString());
            var S1Q5AxParentAttributeId = S1Q5AttributeId;

            // create the S1Q6 question code
            attributeName = "Support for the study";
            attributeTypeId = 1; // not selectable
            var S1Q6ParentAttributeId = S1Q1ParentAttributeId;
            CreateAttrRes = await AddCode(setIdDE, reviewSetIdDE, S1Q6ParentAttributeId, attributeName, attributeTypeId);
            CSRes = await GetCodesets();
            var S1Q6AttributeId = int.Parse(CSRes[2]["attributes"]["attributesList"][0]["attributes"]["attributesList"][5]["attributeId"].ToString());
            var S1Q6AxParentAttributeId = S1Q6AttributeId;

            // create the S1Q7 question code
            attributeName = "What type of study is this report?";
            attributeTypeId = 1; // not selectable
            var S1Q7ParentAttributeId = S1Q1ParentAttributeId;
            CreateAttrRes = await AddCode(setIdDE, reviewSetIdDE, S1Q7ParentAttributeId, attributeName, attributeTypeId);
            CSRes = await GetCodesets();
            var S1Q7AttributeId = int.Parse(CSRes[2]["attributes"]["attributesList"][0]["attributes"]["attributesList"][6]["attributeId"].ToString());
            var S1Q7AxParentAttributeId = S1Q7AttributeId;

            // create the S1Q8 question code
            attributeName = "Keywords";
            attributeTypeId = 1; // not selectable
            var S1Q8ParentAttributeId = S1Q1ParentAttributeId;
            CreateAttrRes = await AddCode(setIdDE, reviewSetIdDE, S1Q8ParentAttributeId, attributeName, attributeTypeId);
            CSRes = await GetCodesets();
            var S1Q8AttributeId = int.Parse(CSRes[2]["attributes"]["attributesList"][0]["attributes"]["attributesList"][7]["attributeId"].ToString());
            var S1Q8AxParentAttributeId = S1Q8AttributeId;



            // create answer codes for S1Q1
            attributeName = "search 1";
            attributeTypeId = 2; // selectable
            CreateAttrRes = await AddCode(setIdDE, reviewSetIdDE, S1Q1AxParentAttributeID, attributeName, attributeTypeId);
            attributeName = "search 2";
            CreateAttrRes = await AddCode(setIdDE, reviewSetIdDE, S1Q1AxParentAttributeID, attributeName, attributeTypeId);
            attributeName = "search 3";
            CreateAttrRes = await AddCode(setIdDE, reviewSetIdDE, S1Q1AxParentAttributeID, attributeName, attributeTypeId);
            attributeName = "other";
            CreateAttrRes = await AddCode(setIdDE, reviewSetIdDE, S1Q1AxParentAttributeID, attributeName, attributeTypeId);
            CSRes = await GetCodesets();           
            var S1Q1A1AttributeId = int.Parse(CSRes[2]["attributes"]["attributesList"][0]["attributes"]["attributesList"][0]["attributes"]["attributesList"][0]["attributeId"].ToString());
            var S1Q1A2AttributeId = int.Parse(CSRes[2]["attributes"]["attributesList"][0]["attributes"]["attributesList"][0]["attributes"]["attributesList"][1]["attributeId"].ToString());
            var S1Q1A3AttributeId = int.Parse(CSRes[2]["attributes"]["attributesList"][0]["attributes"]["attributesList"][0]["attributes"]["attributesList"][2]["attributeId"].ToString());
            var S1Q1A4AttributeId = int.Parse(CSRes[2]["attributes"]["attributesList"][0]["attributes"]["attributesList"][0]["attributes"]["attributesList"][3]["attributeId"].ToString());
            
         
            // create answer codes for S1Q2
            attributeName = "English";
            attributeTypeId = 2; // selectable
            CreateAttrRes = await AddCode(setIdDE, reviewSetIdDE, S1Q2AxParentAttributeId, attributeName, attributeTypeId);
            attributeName = "Other (specify)";
            CreateAttrRes = await AddCode(setIdDE, reviewSetIdDE, S1Q2AxParentAttributeId, attributeName, attributeTypeId);
            CSRes = await GetCodesets();
            var S1Q2A1AttributeId = int.Parse(CSRes[2]["attributes"]["attributesList"][0]["attributes"]["attributesList"][1]["attributes"]["attributesList"][0]["attributeId"].ToString());
            var S1Q2A2AttributeId = int.Parse(CSRes[2]["attributes"]["attributesList"][0]["attributes"]["attributesList"][1]["attributes"]["attributesList"][1]["attributeId"].ToString());

           
            // create answer codes for S1Q3
            attributeName = "Published";
            attributeTypeId = 2; // selectable
            CreateAttrRes = await AddCode(setIdDE, reviewSetIdDE, S1Q3AxParentAttributeId, attributeName, attributeTypeId);
            attributeName = "In press";
            CreateAttrRes = await AddCode(setIdDE, reviewSetIdDE, S1Q3AxParentAttributeId, attributeName, attributeTypeId);
            attributeName = "Unpublished";
            CreateAttrRes = await AddCode(setIdDE, reviewSetIdDE, S1Q3AxParentAttributeId, attributeName, attributeTypeId);
            CSRes = await GetCodesets();
            var S1Q3A1AttributeId = int.Parse(CSRes[2]["attributes"]["attributesList"][0]["attributes"]["attributesList"][2]["attributes"]["attributesList"][0]["attributeId"].ToString());
            var S1Q3A2AttributeId = int.Parse(CSRes[2]["attributes"]["attributesList"][0]["attributes"]["attributesList"][2]["attributes"]["attributesList"][1]["attributeId"].ToString());
            var S1Q3A3AttributeId = int.Parse(CSRes[2]["attributes"]["attributesList"][0]["attributes"]["attributesList"][2]["attributes"]["attributesList"][2]["attributeId"].ToString());

            
            // create answer codes for S1Q4
            attributeName = "Up to 1975 (please specify)";
            attributeTypeId = 2; // selectable
            CreateAttrRes = await AddCode(setIdDE, reviewSetIdDE, S1Q4AxParentAttributeId, attributeName, attributeTypeId);
            attributeName = "1976 - 1980";
            CreateAttrRes = await AddCode(setIdDE, reviewSetIdDE, S1Q4AxParentAttributeId, attributeName, attributeTypeId);
            attributeName = "1981 - 1985";
            CreateAttrRes = await AddCode(setIdDE, reviewSetIdDE, S1Q4AxParentAttributeId, attributeName, attributeTypeId);
            attributeName = "1986 - 1990";
            CreateAttrRes = await AddCode(setIdDE, reviewSetIdDE, S1Q4AxParentAttributeId, attributeName, attributeTypeId);
            attributeName = "1991 - 1995";
            CreateAttrRes = await AddCode(setIdDE, reviewSetIdDE, S1Q4AxParentAttributeId, attributeName, attributeTypeId);
            attributeName = "1996 - 2000";
            CreateAttrRes = await AddCode(setIdDE, reviewSetIdDE, S1Q4AxParentAttributeId, attributeName, attributeTypeId);
            attributeName = "2001 - 2005";
            CreateAttrRes = await AddCode(setIdDE, reviewSetIdDE, S1Q4AxParentAttributeId, attributeName, attributeTypeId);
            attributeName = "2006 - 2010";
            CreateAttrRes = await AddCode(setIdDE, reviewSetIdDE, S1Q4AxParentAttributeId, attributeName, attributeTypeId);
            attributeName = "2011+";
            CreateAttrRes = await AddCode(setIdDE, reviewSetIdDE, S1Q4AxParentAttributeId, attributeName, attributeTypeId);
            CSRes = await GetCodesets();
            var S1Q4A1AttributeId = int.Parse(CSRes[2]["attributes"]["attributesList"][0]["attributes"]["attributesList"][3]["attributes"]["attributesList"][0]["attributeId"].ToString());
            var S1Q4A2AttributeId = int.Parse(CSRes[2]["attributes"]["attributesList"][0]["attributes"]["attributesList"][3]["attributes"]["attributesList"][1]["attributeId"].ToString());
            var S1Q4A3AttributeId = int.Parse(CSRes[2]["attributes"]["attributesList"][0]["attributes"]["attributesList"][3]["attributes"]["attributesList"][2]["attributeId"].ToString());
            var S1Q4A4AttributeId = int.Parse(CSRes[2]["attributes"]["attributesList"][0]["attributes"]["attributesList"][3]["attributes"]["attributesList"][3]["attributeId"].ToString());
            var S1Q4A5AttributeId = int.Parse(CSRes[2]["attributes"]["attributesList"][0]["attributes"]["attributesList"][3]["attributes"]["attributesList"][4]["attributeId"].ToString());
            var S1Q4A6AttributeId = int.Parse(CSRes[2]["attributes"]["attributesList"][0]["attributes"]["attributesList"][3]["attributes"]["attributesList"][5]["attributeId"].ToString());
            var S1Q4A7AttributeId = int.Parse(CSRes[2]["attributes"]["attributesList"][0]["attributes"]["attributesList"][3]["attributes"]["attributesList"][6]["attributeId"].ToString());
            var S1Q4A8AttributeId = int.Parse(CSRes[2]["attributes"]["attributesList"][0]["attributes"]["attributesList"][3]["attributes"]["attributesList"][7]["attributeId"].ToString());
            var S1Q4A9AttributeId = int.Parse(CSRes[2]["attributes"]["attributesList"][0]["attributes"]["attributesList"][3]["attributes"]["attributesList"][8]["attributeId"].ToString());


            // create answer codes for S1Q5
            attributeName = "Included";
            attributeTypeId = 2; // selectable
            CreateAttrRes = await AddCode(setIdDE, reviewSetIdDE, S1Q5AxParentAttributeId, attributeName, attributeTypeId);
            attributeName = "Not included";
            CreateAttrRes = await AddCode(setIdDE, reviewSetIdDE, S1Q5AxParentAttributeId, attributeName, attributeTypeId);
            CSRes = await GetCodesets();
            var S1Q5A1AttributeId = int.Parse(CSRes[2]["attributes"]["attributesList"][0]["attributes"]["attributesList"][4]["attributes"]["attributesList"][0]["attributeId"].ToString());
            var S1Q5A2AttributeId = int.Parse(CSRes[2]["attributes"]["attributesList"][0]["attributes"]["attributesList"][4]["attributes"]["attributesList"][1]["attributeId"].ToString());


            // create answer codes for S1Q6
            attributeName = "Source of the funding stated";
            attributeTypeId = 2; // selectable
            CreateAttrRes = await AddCode(setIdDE, reviewSetIdDE, S1Q6AxParentAttributeId, attributeName, attributeTypeId);
            attributeName = "Source of the funding not stated";
            CreateAttrRes = await AddCode(setIdDE, reviewSetIdDE, S1Q6AxParentAttributeId, attributeName, attributeTypeId);
            CSRes = await GetCodesets();
            var S1Q6A1AttributeId = int.Parse(CSRes[2]["attributes"]["attributesList"][0]["attributes"]["attributesList"][5]["attributes"]["attributesList"][0]["attributeId"].ToString());
            var S1Q6A2AttributeId = int.Parse(CSRes[2]["attributes"]["attributesList"][0]["attributes"]["attributesList"][5]["attributes"]["attributesList"][1]["attributeId"].ToString());


            // create answer codes for S1Q7
            attributeName = "Process evaluation";
            attributeTypeId = 2; // selectable
            CreateAttrRes = await AddCode(setIdDE, reviewSetIdDE, S1Q7AxParentAttributeId, attributeName, attributeTypeId);
            attributeName = "Outcome evaluation";
            CreateAttrRes = await AddCode(setIdDE, reviewSetIdDE, S1Q7AxParentAttributeId, attributeName, attributeTypeId);
            attributeName = "Retrospective study";
            CreateAttrRes = await AddCode(setIdDE, reviewSetIdDE, S1Q7AxParentAttributeId, attributeName, attributeTypeId);
            attributeName = "Prospective study";
            CreateAttrRes = await AddCode(setIdDE, reviewSetIdDE, S1Q7AxParentAttributeId, attributeName, attributeTypeId);
            CSRes = await GetCodesets();
            var S1Q7A1AttributeId = int.Parse(CSRes[2]["attributes"]["attributesList"][0]["attributes"]["attributesList"][6]["attributes"]["attributesList"][0]["attributeId"].ToString());
            var S1Q7A2AttributeId = int.Parse(CSRes[2]["attributes"]["attributesList"][0]["attributes"]["attributesList"][6]["attributes"]["attributesList"][1]["attributeId"].ToString());
            var S1Q7A3AttributeId = int.Parse(CSRes[2]["attributes"]["attributesList"][0]["attributes"]["attributesList"][6]["attributes"]["attributesList"][2]["attributeId"].ToString());
            var S1Q7A4AttributeId = int.Parse(CSRes[2]["attributes"]["attributesList"][0]["attributes"]["attributesList"][6]["attributes"]["attributesList"][3]["attributeId"].ToString());


            // create answer codes for S1Q8
            attributeName = "Not included";
            attributeTypeId = 2; // selectable
            CreateAttrRes = await AddCode(setIdDE, reviewSetIdDE, S1Q8AxParentAttributeId, attributeName, attributeTypeId);
            attributeName = "Included (write in)";
            CreateAttrRes = await AddCode(setIdDE, reviewSetIdDE, S1Q8AxParentAttributeId, attributeName, attributeTypeId);
            CSRes = await GetCodesets();
            var S1Q8A1AttributeId = int.Parse(CSRes[2]["attributes"]["attributesList"][0]["attributes"]["attributesList"][7]["attributes"]["attributesList"][0]["attributeId"].ToString());
            var S1Q8A2AttributeId = int.Parse(CSRes[2]["attributes"]["attributesList"][0]["attributes"]["attributesList"][7]["attributes"]["attributesList"][1]["attributeId"].ToString());

            CSRes = await GetCodesets();

            /*
             * Assign coding to the items
             * 
             */


            ReviewInfo? Res = await GetReviewInfo();
            // assign a screening code to 20 items (match the demo review)  
            var additionalText = "";
            JsonNode? item;
            // itemId = 1 is a seed item so start with itemId = 2
            var itemId = 2; // Include 1
            item = await AddItemAttribute(Res, setIdTA, attributeIdInc1TA, itemId, additionalText);          
            itemId = 3;  // Include 1
            item = await AddItemAttribute(Res, setIdTA, attributeIdInc1TA, itemId, additionalText);
            itemId = 4; // Exclude 1
            item = await AddItemAttribute(Res, setIdTA, attributeIdExc1TA, itemId, additionalText);
            itemId = 5; // Exclude 4
            item = await AddItemAttribute(Res, setIdTA, attributeIdExc4TA, itemId, additionalText);
            itemId = 6;  // Exclude 2
            item = await AddItemAttribute(Res, setIdTA, attributeIdExc2TA, itemId, additionalText);
            itemId = 7;  // Exclude 3
            item = await AddItemAttribute(Res, setIdTA, attributeIdExc3TA, itemId, additionalText);
            itemId = 8;  // Include 1
            item = await AddItemAttribute(Res, setIdTA, attributeIdInc1TA, itemId, additionalText);
            itemId = 9;  // Exclude 2
            item = await AddItemAttribute(Res, setIdTA, attributeIdExc2TA, itemId, additionalText);
            itemId = 10;  // Include 1
            item = await AddItemAttribute(Res, setIdTA, attributeIdInc1TA, itemId, additionalText);
            itemId = 11;  // Exclude 3
            item = await AddItemAttribute(Res, setIdTA, attributeIdExc3TA, itemId, additionalText);
            itemId = 12;  // Include 1
            item = await AddItemAttribute(Res, setIdTA, attributeIdInc1TA, itemId, additionalText);
            itemId = 13;  // Include 1
            item = await AddItemAttribute(Res, setIdTA, attributeIdInc1TA, itemId, additionalText);
            itemId = 14;  // Exclude 2
            item = await AddItemAttribute(Res, setIdTA, attributeIdExc2TA, itemId, additionalText);
            itemId = 15;  // Exclude 3
            item = await AddItemAttribute(Res, setIdTA, attributeIdExc3TA, itemId, additionalText);
            itemId = 16;  // Exclude 1
            item = await AddItemAttribute(Res, setIdTA, attributeIdExc1TA, itemId, additionalText);
            itemId = 17;  // Include 1
            item = await AddItemAttribute(Res, setIdTA, attributeIdInc1TA, itemId, additionalText);
            itemId = 18;  // Exclude 3
            item = await AddItemAttribute(Res, setIdTA, attributeIdExc3TA, itemId, additionalText);
            itemId = 19;  // Exclude 1
            item = await AddItemAttribute(Res, setIdTA, attributeIdExc1TA, itemId, additionalText);
            itemId = 20;  // Include 1
            item = await AddItemAttribute(Res, setIdTA, attributeIdInc1TA, itemId, additionalText);
            itemId = 21;  // Exclude 2
            item = await AddItemAttribute(Res, setIdTA, attributeIdExc2TA, itemId, additionalText);


            // get items with 'Inc1' code AND 'Exc1' code
            // there should be 8 + 3 = 11
            JsonNode?  ItemList = await FetchItemsWithThisCode(attributeIdInc1TA.ToString() + "," + attributeIdExc1TA.ToString());
            var itemCount = ItemList["totalItemCount"].ToString();
            ItemList["totalItemCount"].ToString().Should().Be("11");

            // get items with the Inc1 code...
            // there should be 8 items with attributeIdInc1
            ItemList = await FetchItemsWithThisCode(attributeIdInc1TA.ToString());
            itemCount = ItemList["totalItemCount"].ToString();
            ItemList["totalItemCount"].ToString().Should().Be("8");
            //ItemList["totalItemCount"].Should().Be(8); // this throws an error. Why????

            // create a visual list of the items marked as 'include'
            var list = ItemList["items"][0]["quickCitation"].ToString() + "\r\n";
            list = list + ItemList["items"][1]["quickCitation"].ToString() + "\r\n";
            list = list + ItemList["items"][2]["quickCitation"].ToString() + "\r\n";
            list = list + ItemList["items"][3]["quickCitation"].ToString() + "\r\n";
            list = list + ItemList["items"][4]["quickCitation"].ToString() + "\r\n";
            list = list + ItemList["items"][5]["quickCitation"].ToString() + "\r\n";
            list = list + ItemList["items"][6]["quickCitation"].ToString() + "\r\n";
            list = list + ItemList["items"][7]["quickCitation"].ToString() + "\r\n";


            // code the 8 'included' items in ItemList with codes from 'Screen on full report' 
            additionalText = "";
            itemId = int.Parse(ItemList["items"][0]["itemId"].ToString()); // Exclude 3
            item = await AddItemAttribute(Res, setIdFR, attributeIdExc3FR, itemId, additionalText);
            itemId = int.Parse(ItemList["items"][1]["itemId"].ToString()); // Include 1
            item = await AddItemAttribute(Res, setIdFR, attributeIdInc1FR, itemId, additionalText);
            itemId = int.Parse(ItemList["items"][2]["itemId"].ToString()); // Exclude 4
            item = await AddItemAttribute(Res, setIdFR, attributeIdExc4FR, itemId, additionalText);
            itemId = int.Parse(ItemList["items"][3]["itemId"].ToString()); // Exclude 4
            item = await AddItemAttribute(Res, setIdFR, attributeIdExc4FR, itemId, additionalText);
            itemId = int.Parse(ItemList["items"][4]["itemId"].ToString()); // Include 1
            item = await AddItemAttribute(Res, setIdFR, attributeIdInc1FR, itemId, additionalText);
            itemId = int.Parse(ItemList["items"][5]["itemId"].ToString()); // Include 1
            item = await AddItemAttribute(Res, setIdFR, attributeIdInc1FR, itemId, additionalText);
            itemId = int.Parse(ItemList["items"][6]["itemId"].ToString()); // Exclude 3
            item = await AddItemAttribute(Res, setIdFR, attributeIdExc3FR, itemId, additionalText);
            itemId = int.Parse(ItemList["items"][7]["itemId"].ToString()); // Exclude 4
            item = await AddItemAttribute(Res, setIdFR, attributeIdExc4FR, itemId, additionalText);

            // get items with the Inc1 code...
            // there should be 3 items with attributeIdInc1FR
            ItemList = await FetchItemsWithThisCode(attributeIdInc1FR.ToString());
            itemCount = ItemList["totalItemCount"].ToString();
            ItemList["totalItemCount"].ToString().Should().Be("3");

            // create a visual list of the items marked as 'include'
            list = ItemList["items"][0]["quickCitation"].ToString() + "\r\n";
            list = list + ItemList["items"][1]["quickCitation"].ToString() + "\r\n";
            list = list + ItemList["items"][2]["quickCitation"].ToString() + "\r\n";



            // code the 3 'included' items in ItemList with codes from 'Data extraction tool' 


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
            //def.screeningIndexed = Res.ScreeningIndexed;
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

        public async Task<JsonNode?> FetchItemsWithThisCode(string attributeSetIdList)
        {

            /*
            bool showDeleted;
            int sourceId;
            int searchId;
            Int64 xAxisSetId;
            Int64 xAxisAttributeId;
            Int64 yAxisSetId;
            Int64 yAxisAttributeId;
            Int64 filterSetId;
            Int64 filterAttributeId;
            string attributeSetIdList;
            string listType;
            int pageNumber;
            int pageSize;
            int totalItems;
            int startPage;
            int endPage;
            int startIndex;
            int endIndex;
            int workAllocationId;
            int comparisonId;

            int magSimulationId;
            string description;
            int contactId;
            int setId;
            bool showInfoColumn;
            bool showScoreColumn;

            string withOutAttributesIdsList;
            string withAttributesIds;
            string withSetIdsList;
            string withOutSetIdsList;
            */

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
    