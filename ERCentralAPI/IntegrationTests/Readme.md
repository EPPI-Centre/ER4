# Introduction: Integration tests for EPPI-Reviewer Web API

This project contains integration tests for the **EPPI-Reviewer Web API**. This means that it tests the full stack, from API endpoint "inwards", thus covers: Controller Methods, CSLA business objects, Database access and logic (there is a lot of logic in our stored procedures!).

How it works, in a nutshell:

Upon running any number of tests, the following things will happen once (and only once):

1. Test dabases are created: `tempTestReviewer` and `tempTestReviewerAdmin`
1. Create their structure as it was at a "known" point in time (see below)	
1. Seed some data, including some users.
1. Rename databases:
	- If `Reviewer` and `ReviewerAdmin` exist, they get their own "temporary name" (`ReviewerSetAside` and `ReviewerAdminSetAside`)
	- The 2 new/temp databases are renamed to Reviewer and ReviewerAdmin
1. Run the "SQL Changes Manager" project, this brings the structure of our new/temp DBs up to date, from the "known" point in time, up to the latest "version".
1. Rename databases back to how they were before pt.4. 
1. **Run the tests!** (Could be many tests, or just one, doesn't matter how many.)
    - Running the tests implies "hitting" the ER-Web API endpoints, which can and does change data in the DBs
	- The database/tests setup system described here ensures data-changes produced by the tests happen in insulated databases created for tests and tests only.
1. Delete `tempTestReviewerAdmin` and `tempTestReviewer` when done.

The general architecture of this **integration tests** project is inspired mostly from: https://timdeschryver.dev/blog/how-to-test-your-csharp-web-api and the associated GitHub project (https://github.com/timdeschryver/HowToTestYourCsharpWebApi).

## Quick how-to: writing new tests

To ensure the steps described in the introduction will happen upon executing new tests, any new test class needs to:

a. Inherit from `IntegrationTest` as in `public class MyNewTests : IntegrationTest`
a. Receive an `ApiWebApplicationFactory` parameter in its constructor, and pass it on to `base` as in: `public MyNewTests(ApiWebApplicationFactory fixture): base(fixture) { }`. 

Point "a." ensures the test class (and all tests therein) will have access to the protected methods/members of `IntegrationTest`, which are **required** for authentication.

Point "b." passes the injected instance of ApiWebApplicationFactory to the `IntegrationTest` base class, ensuring its constructor and Dispose() method are executed once and only once.

### Authentication requirements

If your test/class of tests need fine control over who is authenticaticated to what review and when, then you can call `await InnerLoginToReview(username, password, ReviewId);` at any time and change who (if any) is logged on.

Otherwise, if you think that within a class most or all tests will always use the same login, you can inherit from `FixedLoginTest` implement and set the "username, password, reviewId" properties and then call ` (await AuthenticationDone()).Should().Be(true);` at the start of each test method. This ensures that authentication is done **once and only once** for the whole class.

You can still _change_ who is logged on (to what) within any such class if you wish to. See below for details.


# Implementation details

## Authentication

In EPPI-Reviewer, there is a non-standard system that control what review any given authenticated user is accessing. Thus, the API supports logging on in a two-phases patter. First, the user authenticates with username and password. At which point, the user can receive their list(s) of reviews and/or create a new review (and nothing else). Second, the user can "open" a review, at which point they receive a "token" representing their (CSLA)"ReviewerIdentity" security principal. This class has a "ReviewId" property, which is "normal" on the client side. However, when read from the server side, the ReviewId value is obtained by querying ReviewerAdmin with the (GUID)"LogonTicket" value. This system ensures that the Client systems cannot "pretend" to be accessing arbitrary reviews, that any given user can have one and only one review open at a given time, and that at any give time, a user can be fully logged on from one client and one client only.

This system guarantees user/review isolation (and more) but presents a problem for tests since:

1. We clearly do WANT to test for user/review isolation and access control, thus, we DO NOT want to mock the authentication layer.
1. We need to get the API to do the authentication work, *before* being able to send any request for/to review data!

For this reason, the base class `IntegrationTest` includes the `protected async Task InnerLoginToReview(string uname, string pw, int revId)` method, which allows to authenticate a user and to open a given review, from all Test classes (as long as they do inherit from `IntegrationTest` as they should). Thus, you can call that method from within any "[Fact]" method providing granular control of who is logged on what review at any give step of a test.

Sometimes, you may have a class of tests where the logged on user doesn't need to change, or needs to be changed very rarely. In such cases, you can create a class by inheriting from `FixedLoginTest`.
You'd then implement the following in your NewClass:

```
public class MyNewClass : FixedLoginTest
{
    protected override string username { get; set; }
    protected override string password { get; set; }
    protected override int ReviewId { get; set; }
    
    public MyNewClass(ApiWebApplicationFactory fixture) : base (fixture) {
        username = "bob";
        password = "123";
        ReviewId = 12;
    }
    [...]
}
```

For an example of how this works, see `ReviewInfoTests`. In there, you can also see the `ReviewInfoWhileChangingReviews()` method, demonstrating how to override (and re-establish) the default login data, if/as needed.

