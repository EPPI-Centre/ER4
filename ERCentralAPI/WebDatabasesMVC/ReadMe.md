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
This project is written in DotNet Core 3.x using the ASP.MVC framework. To help consolidating the code-base, the model data is **always** fetched via CSLA business objects (BOs). Some of these objects will be shared with EPPI-Reviewer itself, but a good proportion will need to be ad-hoc BOs, given the specific requirements of this project.  
The overaraching rule is:

> This project will NOT access data directly, but **will always do so via CSLA BOs**, which in turn means that the underlying "data access" logic will always remain "compatible" with ER in general.  

This introduces some technical complications. First of all, the actual physical location of BOs. By convention, BOs that have been created in ER4 times, reside in this same GitHub repository, but a different solution (as Silverlight isn't supported in recent VS editions), the relative path is thus `..\..\EPPIReviewer4.2015\Server\BusinessClasses\*.cs` in most cases.  
However, some BOs, created and used by ER-Web only, can be found in `..\ERxWebClient2\Models\*.cs`. As mentioned above, some BOs will need to be created for this specific project, thus to avoid "spreading" the source code in multiple places, it is recommended to create them in `..\ERxWebClient2\Models\*.cs` as well, this might be useful as it's possible that ER-Web will need to consume these object as well.

#### Special BOs
Compared to ER, the WebDatabases have simpler authentication/authorisation requirements. However once established, a "session" should provide access to one and only one Review (in ER terms), thus it is possible to retain the general "internal" check approach where each BO gets to know what the review_id is, based on the Authenticated user, but, at the same time, how this is done needed to change. For this reason, the 2 following BOs are new, and effectively _replicate_ existing ER BOs. They are:
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
This is done at two levels, the first one operates via the MVC framework, the second via the ad-hoc files in `~\Models\Security`. For the first, we expect that users will need to specify a "username and password" pair to access a password protected WebDB, or simply visit a WebDB unique URL (/Login/Open?WebDBid=XXX) to access an open-access WebDB. In both cases, this will result in creating an authenticated session, which is kept on the client in the form of a cookie. The (encrypted) cookie contains the IDs for: the WebDB, the corresponding ReviewID and the AttributeID for the "only items with this code" feature (if used). 

Note: most Stored Procedures used by WebDB[...] business objects will look and cater for the "only items with this code" AttributeID.  

All these IDs are stored in the form of "Claims" and are thus available via the ClaimsPrincipal `User` object, available in all controllers. All controller classes (with the few exeptions of the controllers that can authenticate people on their arrival) should start with the **`[Authorise]`** decorator, so to deny all requests that do not come authenticated with a valid cookie.

(Note also the shape of the controller constructor and how it inherits from `CSLAController`.)

### Opening a DB and Logging in.
There are two routes to open a webDB. There is a "login" form that sends data as POST request, but for databases that are not password protected, one can also go to the url:
`https://[base]/Login/Open?/WebDBid=N` where "N" is the webDBid.

### Getting to "see" something.
Project ignores the old WebDatabases system, it is now **all implemented in EPPI-Reviewer Web**. In the review home page, there is a "setup WebDatabase" which allows to set up a WebDb for the current review. You can pick a name, a description, password protect the DB or not, pick a code to filter items and pick what codesets will be seen in the WebDb. 
You can also remove some codes from a visible codeset and you can rename some codes (and give them a new description).



### Accessing Data, Organising views
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

The **ItemListController.cs** is of particular interest. A few methods in there will indeed produce an ItemList (with no criteria, these serve the unfiltered item list, and only allow to change page). These are:
- `Index()`
- `IndexJSON()`
- `Page([FromForm] int PageN)`
- `PageJSON([FromForm] int PageN)`

You'll see that each method above implements very little logic, beyond the required structure shown above. This is because they all access the same kind of data, which can (and should!) be fetched by a common method (`internal ItemList GetItemList(SelectionCriteria crit)`). This method is "internal" because it cannot be accessed via an HTTP request, but is not private, because it might be useful to other controllers elsewhere.  
In short, the 4 methods listed above do nothing else that prepare a SelectioCriteria object, enriched with the specific detail that caracterises the method/request. All the rest of the work is done inside the internal method.  
These methods are organised in pairs: with one method (JSON suffix) returning a JSON object (not an HTML view), so to act as an API endpoint.

The arrangement above exemplifies how we use a common protected method to instantiates the models, and how we then use the model directly to return a JSON represetation, or pass the model to a view, to produce HTML. Note also that the Page(...) method "names" the view it will use (autodiscovery of the view cannot work, for this method): `return View("Index", iList);`.

Three other methods in ItemListController.cs show another common pattern, they are:
- `ItemDetails([FromForm] long itemId)`
- `ItemDetailsJSON([FromForm] long itemId)`
- `GetItemDetails(long ItemId)`

As before, the latter is used by the previous two, to create the Model they will need. Unlike the previous case, however, in this case, we want to present the ItemDetails data, which, alas, is not all contained inside a single BO. Thus, we use an ad-hoc "viewModel" (i.e. a model that exists specifically to support one or more views). This is a very simple object, located here: `~\ViewModels\FullItemDetails.cs`.  
As you can see, it has some simple members that are standard BOs, plus a few methods that use the BO data to procude some convenient values, which can then be used directly in the view.  
The corresponding view (`~\Views\ItemList\ItemDetails.cshtml`) shows how to access the separate BOs and also how to include some simple JS (uses jQuery), so to exemplify how to add a little bit of client-side interactivity.  
The `GetItemDetails(long ItemId)` is also simple, all it does is to call the required DataPortal.Fetch<...>(...) methods to collect the needed BOs and then package them together in a new instance of `FullItemDetails`. These FullItemDetails are then used by `ItemDetails(...)` and `ItemDetailsJSON(...)` to produce a view or a JSON response, as before.

Moreover, lots of additional methods exist. These are organised as follows:

a) A pair of "entry" methods, called by a specific page (frequencies, crosstabs) and passing do the method the data required to create a new (ItemList) criterion and fetch the list. An example of such a pair is `GetListWithWithoutAtts / GetListWithWithoutAttsJSON`, which are called from crosstabs page (and should be suitable for item lists made via Maps).
a) The "model" these controllers use is of type `\ViewModels\ItemListWithCriteria` this is nothing else than a simple class that includes the existing ItemList BO and the criteria used to fetch it, in an MVC-friendly shape (`SelCritMVC` see below).
a) The ViewModel data includes the criteria, so that the resulting ItemList VIEW can re-use it to allow browsing through pages. This is done by using a catch-all controller method `ListFromCrit(SelCritMVC critMVC)` and potentially its twin `ListFromCritJson(SelCritMVC critMVC)`
a) `SelCritMVC` is class defined in the controller file, which is used to "convert" the `[HttpPost]` input to the public controller method to and from its equivalent CSLA criteria (which is needed to fetch CSLA item lists).
a) All these methods include only the logic required to specify the Criterion object, and then rely on `internal ItemList GetItemList(SelectionCriteria crit)` to actually do the "data fetch" work.


#### Interactivity
One possibility, which does not require to license additional products is to use Kendo for jQuery components.

**Pro**: they are immediately "optimised" for feeding them data through Ajax calls. 

**Con**: they require more Javascript fiddling, to ensure the data fits into the expected mold. [This is also a pro, as it's not clear we would not need equivalent fiddling on the server side, to make BOs fit in the telerik mold of Kendo for MVC.]

In fact, the ReviewHome page now exemplifies both pro and con sides. The project includes a controller: `ReviewSetListController`.
        
This controller has one method `public IActionResult FetchJSON()` which fetches the `WebDbReviewSetsList` (this object is able to _only_ retrieve the codesets and codes that belong to the WebDB, ignoring coding tools that have not been added to the WebDB AND the codes thereing that have been hidden away) and returns it as a JSON object.  
Concurrently, the "~/Review/Index.cshtm" view has acquired some JavaScript that:
1. Requests data from ReviewSetsList.FetchJSON()
2. When data is received, parses it to construt new JavaScript Objects which extend the "kendo.data.Node" JavaScript Type, so that it can now contain our codetree data.
3. The data is then fed into the kendoTreeView "widget".

This demonstrates the possibility of using Kendo components for which we already have a license. The alternative would be to add a license for also the "(Kendo) UI for ASP.NET MVC" product which [appears to be](https://www.telerik.com/blogs/kendo-ui-vs-ui-for-asp-net-mvc) just KendoUI with added server-side (C#) automation so to create the required JS automatically via Razor. The **pro** of this option is: in straightforard situations, there is less code to write. The **con**: I am pretty sure we have direct lower-level access via Kendo for jQuery, meaning that it _should_ allow to do more, as demonstrated in `~/Review/Index.cshtm`.

#### Using Javascript
Some examples of how JavaScript is used are in ReviewHome (most), but also in Frequencies and Crosstab results. They all revolve around the `function postwith(to, p)` javascript method, which resides in `_Layout.cshtml` (making it available in all pages).
In all present cases, the code is organised in a similar way:

- The page itself contains one or more javascript methods used to identify and format the data that needs to be sent to a controller.
- This might require user interaction (generating frequencies / crosstabs) or is simply logic that can be delegated to the client (fetching ItemLists from frequencies and crosstab results).
- Once data is digested, it is sent to `postwith(to, p)` which generates and submits a "form", sending the user to the `to` controller, and feeding the `p` list of parameters to it.

This kind of arrangement is useful when we are asking for a (new) view (i.e. we'll navigate to a different page). 

Cases where we'd want to feed some data to a JS visualisation, will probably use $.Load(...) (or some other [jQuery method](https://www.tutorialspoint.com/jquery/jquery-ajax.htm)).


