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
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace IntegrationTests.By_Controller_Tests
{
    [Collection("Database collection")]
    public class AccountManagerTests : FixedLoginTest
    {
        //await InnerLoginToReview("bob", "123", 12);
        protected override string username { get; set; }
        protected override string password { get; set; }
        protected override int ReviewId { get; set; }

        public AccountManagerTests(ApiWebApplicationFactory fixture) : base(fixture)
        {
            username = "bob";
            password = "123";
            ReviewId = 12;
        }


        [Fact]
        public async Task AccountManagerUpdateReviewName()
        {
            //The line below ensures we log on with the default values for this class.
            //every [Fact] should start with this line, so to ensure authentication is done (once, for the whole class).
            (await AuthenticationDone()).Should().Be(true);
            ReviewInfo? Res = await GetReviewInfo();
            Res.ReviewId.Should().Be(12);
            Res.ReviewName.Should().Be("Shared rev1 (id:12)");

            // update review name
            var ReviewName = "Dog days";
            JsonNode? Res2 = await UpdateReviewName(ReviewName);
            (Res2.ToString()).Should().Be("true");

            // not really needed. If the api call comes back true then the change has been made.
            // get the updated value from the DB
            //Res = await GetReviewInfo();
            //Res.ReviewName.Should().Be("Dog days");

            // return the original review name
            ReviewName = "Shared rev1 (id:12)";
            Res2 = await UpdateReviewName(ReviewName);
            (Res2.ToString()).Should().Be("true");

        }


        [Fact]
        public async Task AccountManagerGetUserAccountDetails()
        {
            //The line below ensures we log on with the default values for this class.
            //every [Fact] should start with this line, so to ensure authentication is done (once, for the whole class).
            (await AuthenticationDone()).Should().Be(true);

            var contact_Id = 6;
            Contact? contactDetails = await GetUserAccountDetails(contact_Id);
            contactDetails.ContactId.Should().Be(6);
            contactDetails.contactName.Should().Be("Bob Fake");
        }

        [Fact]
        public async Task AccountManagerUpdateAccount()
        {
            //The line below ensures we log on with the default values for this class.
            //every [Fact] should start with this line, so to ensure authentication is done (once, for the whole class).
            (await AuthenticationDone()).Should().Be(true);

            // get the contact object
            var contact_Id = 6;
            Contact? contactDetails = await GetUserAccountDetails(contact_Id);

            // update the name
            contactDetails.contactName = "Bob Real";
            JsonNode? Res = await UpdateAccount(contactDetails);

            // get and check the contact object
            Contact? contactDetails2 = await GetUserAccountDetails(contact_Id);
            contactDetails2.contactName.Should().Be("Bob Real");

            // return the name back to the original
            contactDetails2.contactName = "Bob Fake";
            JsonNode? Res2 = await UpdateAccount(contactDetails2);

            // get and check the contact object
            Contact? contactDetails3 = await GetUserAccountDetails(contact_Id);
            contactDetails3.contactName.Should().Be("Bob Fake");

        }
        [Fact]
        public async Task AccountManagerForbiddenUpdateAccount()
        {
            (await AuthenticationDone()).Should().Be(true);

            // get the contact object for a DIFFERENT user, not the one logged on!
            var contact_Id = 4;
            Contact? contactDetails = await GetUserAccountDetails(contact_Id);

            // update the name - this MUST fail (on save), as we don't allow changing someone else's account
            contactDetails.contactName = "Can't do this";

            //OK, how do we check for "expected failures"? We need to bypass the code we normally use, because
            //our normal API calls all include a call to: "response.EnsureSuccessStatusCode();"
            //which throws an exception when an API call failed

            //First way to check, suitable in this case, relies on EnsureSuccessStatusCode() rising the EXPECTED exception
            //This might not always be good enough: if we need to check exactly what we get in the response, we must use the second way (below).

            //Dummy object, we put some value here to ensure out request triggers the catch clause
            JsonNode? Res =  JsonNode.Parse("{\"nothing\": null}");
            try
            {
                Res = await client.PostAndDeserialize("api/AccountManager/UpdateAccount", contactDetails);
            } 
            catch (Exception e)
            {
                e.Message.Should().Be("Response status code does not indicate success: 403 (Forbidden).");
                Res = null;
                Debug.WriteLine(e.Message);
            }
            Res.Should().BeNull();


            //Second way to check: we use even less generic methods, so to gain access to the response object, even if the response was NOT a success.

            StringContent strContent = contactDetails.MakeHttpContent();
            var response = await client.PostAsync("api/AccountManager/UpdateAccount", strContent);
            response.Should().NotBeNull();
            //having the full response, we can check the full details:
            response.IsSuccessStatusCode.Should().BeFalse();
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Forbidden);
            response.ReasonPhrase.Should().Be("Forbidden");

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
        public async Task<JsonNode?> UpdateReviewName(string review_Name)
        {
            JsonNode? res = await client.PostAndDeserialize("api/AccountManager/UpdateReviewName", 
                new SingleStringCriteria() { Value = review_Name });
            res.Should().NotBeNull();
            return res;
        }

        public async Task<Contact?> GetUserAccountDetails(int contact_Id)
        {
            Contact? res = await client.PostAndDeserialize<Contact>("api/AccountManager/GetUserAccountDetails", 
                new SingleIntCriteria() { Value = contact_Id });
            res.Should().NotBeNull();
            return res;
        }

        public async Task<JsonNode?> UpdateAccount(Contact contact_details)
        {
            JsonNode? res = await client.PostAndDeserialize("api/AccountManager/UpdateAccount", contact_details);
            res.Should().NotBeNull();
            return res;
        }

    }
}
