# Web Databases MVC
## Introduction:
This project is dedicated to providing read-only access to live EPPI-Reviewer data.  
In turn, this is expected to require the following:

1. Setup the WebDB in ER-Web, including, but not limited to the following :
   1. Turn the WebDB on or off
   2. Turn Password protection on or off. (If password protected, allow access to uploaded PDFs or not.)
   3. Specify that the WebDB is limited to "Items with this code" or not.
   4. Add and customise (remove parts, rename codes) coding tools, to specify what coding data is available.
1. Comply with the settings defined in ER-Web (point above).
3. Allow to explore the data in the following ways:
   1. Items lists, which provide access to item details (with "Export to RIS" function?).
   1. Item details, which includes some kind of coding report.
   1. Frequencies (as in ER), which produce numbers and provide access to lists (items with this code, in effect).
	1. Crosstabs, as above.
    1. Evidence (gap) maps. (Crosstabs with an additional dimension within cells)
    6. We also expect that Frequencies, Crosstabs and Maps will have alternative "graphic" presentations modes, probably implemented via Telerik components.
4. Also allow to access all data available to the WebDatabases via a corresponding API. [This API should share as much code as possible with the rest of the app, so to ensure it will work correctly for third parties.]

## Technical Implementation
This project is written in DotNet Core 3.x using the ASP.MVC framework. To help consolidating the code-base, the model data is **always** fetched via CSLA business objects (BOs). Most of these objects will be shared with EPPI-Reviewer itself, but a good proportion will need to be ad-hoc BOs, given the specific requirements of this project.  
The overaraching rule is:

> This project will NOT access data directly, but **will always do so via CSLA BOs**, which in turn means that the underlying "data access" logic will always remain "compatible" with ER in general.  

This introduces some technical complications. First of all, the actual physical location of BOs. By convention, BOs that have been created in ER4 times, reside in this same GitHub repository, but a different solution (as Silverlight isn't supported in recent VS editions), the relative path is thus `..\..\EPPIReviewer4.2015\Server\BusinessClasses\*.cs` in most cases.  
However, some BOs, created and used by ER-Web only, can be found in `..\ERxWebClient2\Models\*.cs`. As mentioned above, some BOs will need to be created for this specific project, thus to avoid "spreading" the source code in multiple places, it is recommended to create them in `..\ERxWebClient2\Models\*.cs` as well, this might be useful as it's possible that ER-Web will need to consume these object as well.

#### Special BOs
Compared to ER, the WebDatabases have simpler authentication/authorisation requirements. However once established, a "session" should provide access to one and only one Review (in ER terms), thus it is possible to retain the general "internal" check approach where each BO gets to know what the review_id is, based on the Authenticated user, but, at the same time, how this is done needed to change. For this reason, the 2 following BOs are news, and effectively _replicate_ existing ER BOs. They are:
- `~\Models\Security\ReviewerPrincipal.cs`
- `~\Models\Security\ReviewerIdentity.cs`

These two files are therefore replicating some of the functionalities of their ER counterparts, but, given that they are specifically required to support Authentication and Authorisation in _this project_, they reside within the project.

#### Adding existing BOs
As development progresses, we will need to add more existing BOs to the project. In this project, BOs behave essentially as passive "records", they are representations of the Data we need to present, as such they belong in the `~/Models` folder. Moreover, adding them to this project has to be done by adding them "As link" (right click on the "models" folder, "Add", "Existing item", browse/select the file(s) and then click on **"Add as link"** sub option).  
This detail is **Extremely Important!!** Failing to do so will produce really hard to spot bugs, so please be quite careful while adding files for BOs.

#### Creating New BOs
New BOs should be created in `..\ERxWebClient2\Models\*.cs` and then imported in this project as above (for "existing" BOs).

## Using MVC with CSLA BOs
### Security (Authentication/Authorisation)
This is done at two levels, the first one operates via the MVC framework, the second via the ad-hoc files in `~\Models\Security`. For the first, we expect that users will need to specify a "username and password" pair to access a password protected WebDB, or simply visit a WebDB unique URL to access an open-access WebDB. In both cases, this will result in creating an authenticated session, which is kept on the client in the form of a cookie. The (encrypted) cookie contains the IDs for: the WebDB, the corresponding ReviewID and the AttributeID for the "only items with this code" feature (if used).  
All these IDs are stored in the form of "Claims" and are thus available via the ClaimsPrincipal `User` object, available in all controllers. All controller classes (with the few exeptions of the controllers that can authenticate people on their arrival) should start with the **`[Authorise]`** decorator, so to deny all requests that do not come authenticated with a valid cookie.



(Note also the shape of the controller constructor and how it inherits from `CSLAController`.)

#### Accessing Data, Organising views
If you are unfamiliar with MVC, you may want to refer to [this tutorial](https://asp.mvc-tutorial.com/), which contains a well organised overview of the various elements of ASP.MVC.  
We already know that we are using (existing, linked) BOs as our models. Basic examples on how to fetch the BOs are shown in the ReviewController and ItemListController files.

First and foremost, all public controller methods need to wrap their actual code within the following structure:

```
try
{
    if (SetCSLAUser())
    {
        [... Your code here ...]
    }
    else return Unauthorized();
} 
catch (Exception e)
{
    _logger.LogError(e, "Error in name of controller method");
    return StatusCode(500, e.Message);
}

```

The **ItemListController.cs** is of particular interest. A few methods in there will indeed produce an ItemList. These are:
- `Index()`
- `IndexJSON()`
- `Page([FromForm] int PageN)`
- `PageJSON([FromForm] int PageN)`

You'll see that each method above implements very little logic, beyond the required structure shown above. This is because they all access the same kind of data, which can (and should!) be fetched by a common method (`internal ItemList GetItemList(SelectionCriteria crit)`). This method is "internal" because it cannot be accessed via an HTTP request, but is not private, because it might be useful to other controllers elsewhere.  
In short, the 4 methods listed above do nothing else that prepare a SelectioCriteria object, enriched with the specific detail that caracterises the method/request. All the rest of the work is done inside the internal method.  
Finally, these methods are organised in pairs: with one method (JSON suffix) returning a JSON object (not an HTML view), so to act as an API endpoint.

The arrangement above exemplifies how we use a common protected method to instantiates the models, and how we then use the model directly to return a JSON represetation, or pass the model to a view, to produce HTML. Note also that the Page(...) method "names" the view it will use (autodiscovery of the view cannot work, for this method): `return View("Index", iList);`.

Three other methods in ItemListController.cs show another common pattern, they are:
- `ItemDetails([FromForm] long itemId)`
- `ItemDetailsJSON([FromForm] long itemId)`
- `GetItemDetails(long ItemId)`
As before, the latter is used by the previous two, to create the Model they will need. Unlike the previous case, however, in this case, we want to present the ItemDetails data, which, alas, is not all contained inside a single BO. Thus, we use an ad-hoc "viewModel" (i.e. a model that exists specifically to support one or more views). This is a very simple object, located here: `~\ViewModels\FullItemDetails.cs`.  
As you can see, it has some simple members that are standard BOs, plus a few methods that use the BO data to procude some convenient values, which can then be used directly in the view.  
The corresponding view (`~\Views\ItemList\ItemDetails.cshtml`) shows how to access the separate BOs and also how to include some simple JS (uses jQuery), so to exemplify how to add a little bit of client-side interactivity.  
The `GetItemDetails(long ItemId)` is also simple, all it does is to call the required DataPortal.Fetch<...>(...) methods to collect the needed BOs and then package them together in a new instance of `FullItemDetails`. These FullItemDetails are then used by `ItemDetails(...)` and `ItemDetailsJSON(...)` to produce a view or a JSON response, as before.

#### Interactivity
We think we'll use Telerik comonents to provide useful features like expandable codetrees (with frequencies loaded on demand?), graphs for frequencies and crosstabs, and graphic elements for Evidence Maps.  
At this time, it's not clear wether we'll use Telerik MVC components or Kendo for jQuery components (for which we already have a license).  
What **is clear** is that we most likely fetch the data on demand, via Ajax calls to the JSON endpoints, in most cases when "interactivity" is required. This is because we certainly don't want to to have to re-fetch all data used by something complex like a crosstab of a map at each user-click, which would be necessary if we were re-creating the view at each click from the user.

