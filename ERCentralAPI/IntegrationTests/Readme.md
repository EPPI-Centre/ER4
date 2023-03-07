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

# Writing new tests

Writing good tests is an art in itself, and I'm not sufficiently experienced about it, but I do know about a few gotchas that we do need to keep in mind.

## Data dependencies
Often, we may like a single test to be well "isolated" and to test one very precise "thing" (example: a single API endpoint). So we could try to "mark these items as excluded" (T1), for example, which may require us to first import some items.

But we might also have a test _for_ importing items (T2). Which may import them, then check that the list of sources now contains 2 sources (the always present "manually added items" source, plus the one we uploaded). That's all OK, except that if we do import some items in T1, and after that, the test for importing items is run (T2), it might fail, as it will find 3 sources, not 2. 

So perhaps we should control the order in which tests are run, [which we can](https://learn.microsoft.com/en-us/dotnet/core/testing/order-unit-tests?pivots=xunit), but it does not feel very "sustainable". From another point of view, it feels like our "single purpose" tests should leave the DB "as they found it" and thus cleanup after themselves. This however would mean that we'll have a lot of redundant code, where enpoints are hit one time to "test them" and many other times to "cleanup" after a single-purpose test. And also this feels wrong: takes more time to write the tests, and makes test execution slower.

## Read-only business objects
This project includes 4 extension methods, which we can/should use to "collect" results form API endpoints: `GetAndDeserialize<T>`, `GetAndDeserialize`, `PostAndDeserialize<T>` and `PostAndDeserialize<T>`. Why do we need 2 different methods for the `GET` verb and two for `POST` requests? Because the API endpoints return JSON, so in the test methods, the compiler doesn't know in advance the "shape" of the objects we expect to receive.

To tell the compiler (and intellisense) what we'll get, we can use the generic methods `<T>`: this will ensure our results are de-serialised again to an instance of whatever type we indicated in the `<T>` part of the code calling get/post API endpoints. That's handy, as we can then write code like `result.propertyname.Should().Be(expectedvalue);`.

Fact is, it doesn't always work: our generic (with `<T>` indication) methods will receive a JSON string, create a new empty object of type `<T>` and then attempt to fill it with data via "Set {...}" methods for all the properties. And this will fail, anytime the BO in question does not have "Set {...}" methods for the properties we'll check in our tests.

Thus, we need the "non generic" Get/Post methods too. They always (try to) return an object of `JsonNode?` type. As a consequence, for testing values within the object we receive in such cases, we need to use this syntax:
```
_ = result.Should().NotBeNull();
_ = ((expectedValueType)result["propertyname"]).Should().Be(expectedvalue);
```

In the above, we check for Null, to ensure test fails gracefully if indeed result is null. We use the `_ = ` trick to keep intellisense happy, not sure why it's needed.
We also often need to cast values to our desired type: `((expectedValueType)...)`. And clearly, we access properties via the `["propertyname"]` syntax. Remember that the serialiser will start property names with a small-caps letter!!



# Implementation details

## Database setup and disposal
We use [xUnit](https://xunit.net/docs) as the main testing package, which comes with its own system to allow for "one-time only" setup and dispose to be executed for a whole range of tests, across multiple test classes. How this is done is explained [here](https://xunit.net/docs/shared-context#collection-fixture).

In `\fixtures\DataBaseFixtures.cs` two classes are declared:

- `DatabaseCollection` is just a placeholder needed for the inner working of dependency injection in xUnit. What matters is its decoration: `[CollectionDefinition("Database collection")]`, which we'll mirror in the test classes that need to share the same one time only setup/disposal.
- `TransientDatabase` is the class that does the work, and needs to implement `IDisposable` to ensure cleanup will happen.

**Details for `TransientDatabase`**:

It all happens in the constructor (and Dispose() method). xUnit will create one instance of this class whenever a test contained in a class decorated with `[Collection("Database collection")]` needs to be executed. We can add a `TransientDatabase` parameter to the test constructor methods, but we don't need to, so we don't. What matters is that by using the "Collection" decorators, we obtain 3 effects.

First, xUnit will create **one and only one** instance of `TransientDatabase`, ensuring the code in its constructor is executed once.

Second, having all our tests in the same "Database collection" ensures they do not run in parallel, which is important, as they can (and do) change data in the database, so could easily interfere with one another.

Third, when all tests are run, xUnit will release the `TransientDatabase` for garbage collection, so the C# runtime will then call the `Dispose()` method, because it implements `IDisposable`.

What happens in the `TransientDatabase` Constructor? This class has a method with error-handling, that allows it apply SQL scripts and report success/failure. If a failure occurs, we'd call a "cleanup" method and fail the whole test-execution batch.

1. Run scripts to: create the temporary databases (`tempTestReviewer` and `tempTestReviewerAdmin`), create their data structures, add some seeding data that is required for ER to function (one record in some tables, so that they have 1 identity value, data for import filters, code/set types, etc.).
1. Rename `Reviewer` and `ReviewerAdmin` if present, to `ReviewerSetAside` and `ReviewerAdminSetAside` to get them out of the way. Rename `tempTestReviewer` and `tempTestReviewerAdmin` to `Reviewer` and `ReviewerAdmin`.
1. Run the SQL-Changes-Manager project. This brings the DBs structure (and SPs) to the "up-to-date" state. At this stage, we need these to be called `Reviewer` and `ReviewerAdmin` **because the SQL changes script refer to these names**! 
1. Swap the DBs names back. 
1. Change "database Synonyms". Stored procs in both DBs can and do refer to objects _in the other_ database, and we need our tests to run in a situation where these cross references point to `tempTestReviewer` and `tempTestReviewerAdmin`, not `Reviewer` and `ReviewerAdmin`. For this to work, our SPs now point to objects in the other DB via `SQL synonyms` like this:
    1. if `reviewer.dbo.st_something` refers to `ReviewerAdmin.dbo.TB_SOMETHING` we create a `sTB_SOMETHING` synonym in `Reviewer` and then use the synonym within the stored procedure.
    1. When running tests, we then _change_ all synonyms to refer to `tempTestReviewer` or `tempTestReviewerAdmin`. In this way, we "swap" the reference destination, without having to change the Stored Procedures themselves.
1. Add some more data: (for the moment) this is limited to reviews, one per user, plus 2 shared reviews, to which we also add review members.

Each step (except the SQL manager execution) reports success/failure, so we can stop the execution, after calling the cleanup method, if needed.

The cleanup method will attempt to restore the initial DBs situation, so to rename `ReviewerSetAside` and `ReviewerAdminSetAside` if they are present, and delete `tempTestReviewer` and `tempTestReviewerAdmin`. This same method is called by the `Dispose()` method.




## Authentication

In EPPI-Reviewer, there is a non-standard system that control what review any given authenticated user is accessing. Thus, the API supports logging on in a two-phases patter. First, the user authenticates with username and password. At which point, the user can receive their list(s) of reviews and/or create a new review (and nothing else). Second, the user can "open" a review, at which point they receive a "token" representing their (CSLA)"ReviewerIdentity" security principal. This class has a "ReviewId" property, which is "normal" on the client side. However, when read from the server side, the ReviewId value is obtained by querying ReviewerAdmin with the (GUI)"LogonTicket" value (which is in common with ER4 and is separate from the "token" mentioned above). This system ensures that the Client systems cannot "pretend" to be accessing arbitrary reviews, that any given user can have one and only one review open at a given time, and that at any give time, a user can be fully logged on from one client and one client only.

This system does ensure we always have user/review isolation (and more) but presents a problem for tests since:

1. We clearly do WANT to test for user/review isolation and access control, thus, we DO NOT want to mock the authentication layer. (We also _can't_ mock it, because the version of CSLA currently in use does not understand dependency injection.)
1. We need to get the API to do the authentication work, *before* being able to send any request for/to review data!

For this reason, the base class `IntegrationTest` includes the `protected async Task InnerLoginToReview(string uname, string pw, int revId)` method, which allows to authenticate a user and to open a given review, from all Test classes (as long as they do inherit from `IntegrationTest`, as they should). Thus, you can call that method from within any "[Fact]" method providing granular control of who is logged on what review at any give step of a test.

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

