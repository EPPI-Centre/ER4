using BusinessLibrary.BusinessClasses;
using ERxWebClient2.Controllers;
using FluentAssertions;
using IntegrationTests.Fixtures;
using IntegrationTests.Utils;
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
    /// First example of a review story, following these steps:
    /// 1. Create review, upload some Items
    /// 2. Get new duplicates
    /// 3. Mark automatically (default thresholds)
    /// 4. Inspect result: 21 groups expected, of which 20 are "completed"
    /// 
    /// To test dedup, we need to pre-load items data, get new duplicates, then operate on DuplicateGroups.
    /// Thus, it doesn't really make sense to write code to test each endpoint individually:
    /// a. What they do makes most sense inside an activity flow
    /// b. Would make tests slow, as we'd need to add-remove data for each "unit" test...
    /// </summary>
    [Collection("Database collection")]
    public class MiniDedupStory : FixedLoginTest
    {
        protected override string username { get; set; }
        protected override string password { get; set; }
        protected override int ReviewId { get; set; }
        public MiniDedupStory(ApiWebApplicationFactory fixture) : base(fixture) 
        {
            username = "bob";
            password = "123";
            ReviewId = 12;
        }
        [Fact]
        public async Task MiniDedupReviewStory()
        {
            (await AuthenticationDone()).Should().Be(true);
            int? NewRevId = (int?)(await CreateReview("ReviewFor Dedup" + DateTime.Now.Millisecond.ToString(), ContactId));
            NewRevId.Should().NotBeNull();
            ReviewId = NewRevId.Value;//we change review, so: (1) change our intended review
            _authenticationDone = false; //(2) tell this class we need to authenticate
            (await AuthenticationDone()).Should().Be(true); //(3) login again

            //first, find out what's in there, we expect 1 source (manual items)...
            JsonNode? srcs = await GetSources();
            srcs["sources"].Should().NotBeNull();
            JsonArray? ross = (JsonArray?)srcs["sources"];
            ross.Should().NotBeNull();
            ross.Count.Should().Be(1);

            //upload 1 file, check the answer
            IncomingItemsListJSON? res = await SendThisRefsFile("20 refs.txt", "RIS", "UploadSource");
            res.Should().NotBeNull();
            res.totalReferences.Should().Be(20);
            //and again... (creating 20 duplicates)
            res = await SendThisRefsFile("20 refs.txt", "RIS", "UploadSource");
            res.Should().NotBeNull();
            res.totalReferences.Should().Be(20);

            //add some background noise, to make it more real
            res = await SendThisRefsFile("60 refs.txt", "RIS", "UploadSource");
            res.Should().NotBeNull();
            res.totalReferences.Should().Be(60);


            //res = await SendThisRefsFile("F1.txt", "RIS", "UploadSource");
            //res.Should().NotBeNull();

            JsonNode? jRes = await FetchGroups(true, 5); //we expect 21 groups
            jRes.Should().NotBeNull();

            JsonArray? roDupGroups = (JsonArray?)jRes;
            roDupGroups.Count.Should().Be(21);

            //we have the groups, now we cycle through them and auto-mark anything with similarity = 1
            await AutoMarkGroups(roDupGroups, 1.0, 0, 0);

            //check the expected results: 1 groups should not be completed as it didn't have similarity = 1
            jRes = await FetchGroups(false);
            jRes.Should().NotBeNull();
            roDupGroups = (JsonArray?)jRes;
            List<JsonNode?> incompleteGroups = roDupGroups.Where(f => (bool)f["isComplete"] == false).ToList();
            incompleteGroups.Count.Should().Be(1);
        }
        
    }
}
namespace IntegrationTests.Fixtures
{
    public abstract partial class IntegrationTest : IClassFixture<ApiWebApplicationFactory>
    {
        protected async Task<JsonNode?> FetchGroups(bool GetNew, int maxTries = 1 )
        {
            //dedup currently produces and exception if it's taking too long...
            bool done = false; int tryCount = 0;
            while (tryCount <= maxTries)
            {
                tryCount++;
                try 
                {
                    SingleBoolCriteria data = new SingleBoolCriteria() { Value = GetNew };
                    JsonNode? res = await client.PostAndDeserialize("api/Duplicates/FetchGroups", data);
                    res.Should().NotBeNull();
                    return res;
                }
                catch (Exception e)
                {
                    if (e.Message == "Execution still Running")
                    {
                        GetNew = false;
                        Thread.Sleep(30*1000);
                        continue;
                    }
                }
            }
            return null;
        }
        protected async Task<JsonNode?> FetchGroupDetails(int GroupId)
        {
            SingleIntCriteria data = new SingleIntCriteria() { Value = GroupId };
            JsonNode? res = await client.PostAndDeserialize("api/Duplicates/FetchGroupDetails", data);
            res.Should().NotBeNull();
            return res;
        }
        protected async Task<JsonNode?> MarkUnmarkMemberAsDuplicate(MarkUnmarkItemAsDuplicate crit)
        {
            JsonNode? res = await client.PostAndDeserialize("api/Duplicates/MarkUnmarkMemberAsDuplicate", crit);
            res.Should().NotBeNull();
            return res;
        }
        protected async Task AutoMarkGroups(JsonArray roDupGroups, double SimilarityThreshold, int CodedCountThreshold, int DocsThreshold)
        {
            JsonNode? currGroup;
            JsonArray? members;
            foreach (JsonNode group in roDupGroups)
            {
                if ((bool)group["isComplete"] == false)
                {
                    currGroup = await FetchGroupDetails((int)group["groupId"]);
                    currGroup.Should().NotBeNull();

                    MarkUnmarkItemAsDuplicate crit = new MarkUnmarkItemAsDuplicate() { groupId = (int)group["groupId"], isDuplicate = true, itemDuplicateIds = Array.Empty<int>() };
                    List<int> membersToMark = new List<int>();

                    members = (JsonArray?)currGroup["members"];
                    members.Should().NotBeNull();
                    foreach (JsonNode Jmember in members)
                    {
                        if (Jmember != null)
                        {
                            if ((bool)Jmember["isMaster"] == false
                                && (double)Jmember["similarityScore"] >= SimilarityThreshold
                                && (bool)Jmember["isChecked"] == false
                                && (bool)Jmember["isDuplicate"] == false
                                && (int)Jmember["codedCount"] <= CodedCountThreshold
                                && (int)Jmember["docCount"] <= DocsThreshold)
                            {
                                membersToMark.Add((int)Jmember["itemDuplicateId"]);
                            }
                        }
                    }
                    if (membersToMark.Count > 0)
                    {
                        crit.itemDuplicateIds = membersToMark.ToArray();
                        JsonNode? savedG = await MarkUnmarkMemberAsDuplicate(crit);
                        savedG.Should().NotBeNull();
                    }
                }
            }
        }
    }
}
