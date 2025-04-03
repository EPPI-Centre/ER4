# EPPI Reviewer Main ReadMe File

(Last updated in *August 2024*)

This repository contains most of the source code of **EPPI Reviewer** and its connected/related apps. EPPI Reviewer is an advanced software to collaboratively produce all types of literature review, including systematic reviews, meta-analyses, 'narrative' reviews and meta-ethnographies. It is suitable for small or large-scale reviews (with some of our existing reviews containing over a million references). 

EPPI Reviewer is developed and maintained by the EPPI Centre, University College London, on a strictly not-for-profit basis, although it does require users to purchase subscriptions (thus paying for part of its development costs, as well as computing costs, maintenance and support).

## Preamble

EPPI Reviewer codebase history starts in 2008 when the project for EPPI-Reviewer4 started. This is/was a project based on two core technologies: Microsoft Silverlight and [CSLA](https://cslanet.com/) (Business Objects). Silverlight allowed to write C#/DotNet code that would run inside browsers supported in both Windows and Apple computers; CSLA allowed to write "self-saving" C# Objects, which could be instantiated on the client side (the Silverlight environment running in a browser) and, thanks to the CSLA framework would be able to traverse the full stack (Database/Webserver/Client) so to Create/Read/Update/Delete (CRUD) records from the Database and display results on the Silverlight user interface.

As of late 2024, EPPI-Reviewer 4 (ER4) still exist, but is only marginally maintained. Its successor shares most of the server-side codebase and still uses a compatible (and old) version of CSLA. The EPPI Reviewer 6 (ER6) client is however written in [Angular](https://angular.dev/) and as such, does not exploit the functionalities provided by the CSLA framework. Most of the server-side code is shared for two reasons:

1. The codebase is big and re-writing it all would require a lot of work for no clear (or immediate) advantage.
1. ER4 and ER6 (originally called EPPI Reviewer Web) needed to co-exist and be 100% compatible.

For these reasons, the source code in this repository is vast (has been written over 16+ years, and counting), complex, and hard to make-sense-of: a lot of complexity is the side effect of the history of the project and/or of the multiple compatibility constraints that it needs to respect.

## Projects/Solutions in this repository

This repository contains the source code for the *visible* parts of EPPI Reviewer. These are: 

- Clients of **EPPI Reviewer versions 4 and 6**
- **Server-side components** for the two main clients
- The **Account Manager**, used to create accounts, buy subscriptions, manage review permissions and more. Super-users can use this Web-app to manage restricted functionalities too
- **EPPI Visualiser**, which is an application that allows to publish Review Data
- Experimental/discared projects in a "side" folder.

On the other hand, it does not (currently) contain the following:

- Source code for the Machine Learning components that drive some of EPPI Reviewer most advanced functionalities.
- Source code for the EPPI Mapper application (used to publish snapshots/maps of the data produced in EPPI Reviewer).

This is because [EPPI Mapper](https://eppimapper.digitalsolutionfoundry.co.za/) is stored in a separate repository and maintained mostly by the [Digital Solutions Foundry](https://eppimapper.digitalsolutionfoundry.co.za/) (a third party). The Machine Learning components, on the other hand, are subject to intense R&D and it has been proven difficult for us to integrate them in here. We expect to release the source code for these too, but when and exactly how is not clear at this time.

## Files and folders structure

Each project comes with its individual ReadMe file, thus, the purpose of this section is to provide a general description of what is where and why.

- The `EPPIReviewer4.2015` folder contains the source code for ER4. This includes a vast amount of code that is used in ER6 and in EPPI Visualiser.
- The `ER4Manager` folder contains the source code for the Account Manager and is self-contained (no shared code).
- The `ERCentralAPI` folder contains the source code for:
    - `ER-Web-API` which is the server-side API that drives ER6.
    - `ER-Web-Client`: the Angular client of ER6.
    - `IntegrationTests`: (very basic/limited) integration tests for ER6.
    - `SQL Changes Manager`: a project used to manage changes to the structure and functionalities of the underlying databases.
    - `EPPI Visualiser`: which is in the folder called `WebDatabasesMVC` and which shares common code with EPPI Reviewer proper (both versions).
- `ExportPDFs`: a small command line utility to export PDFs/binaries from a given review.

 
## Getting started and dependencies
Starting to make sense of a vast code-base is always difficult, and in this case it can be extremely confusing, both because of the number of dependencies that our code-base has accumulated over many years, and because of the complexity that comes from sharing code that uses outdated tech (DotNet 3.5, Silverlight and a compatible version of CSLA).
 
Most projects depend on one or more third party components too, mostly coming from [Progress/Telerik](https://www.telerik.com/). This means that anyone trying to use this repository has the responsibility of obtaining such third party licenses as needed. Please see the ReadMe files of each project for the details.
 
Perhaps more importantly, EPPI Reviewer depends on the Microsoft SQL Database. For development purposes, the "Development" and "Express" versions should work, and can be obtained at no cost. Because systematic reviews entail the creation of links between records (i.e. tying a given reference to a given concept), EPPI Reviewer includes a lot of logic inside the database itself, which in turn means that the Database functionalities are complex and tightly coupled to the rest of the codebase (making the adoption of frameworks such as "[Entity](https://learn.microsoft.com/en-us/ef/)" a non-starter).
For this reason, we wrote the "SQL Changes Manager" project, which applies "changes" to the SQL code, based on numbered scripts. These scripts are supplied by developers along with the code-changes they are coupled to. Other developers then run the project to apply SQL code changes as and when they receive the new code. The same system is used also in production, where the "SQL Changes Manager" application is used to apply all "new" scripts associated with a given release.

EPPI Reviewer depends on the existence of two separate MS SQL Databases: `Reviewer` which contains "core" data (imported and generated by users) and `ReviewerAdmin` which contains data that is considered to be "Ancillary" such as subscription payments, logs of account extensions, short descriptions of each release, and so forth.
 
As a consequence, to get up and running, it's necessary to:
 
1. Create the 2 Databases. This can be done using the scripts that the `IntegrationTests` project uses for this same purpose. We recommend looking into the `\ERCentralAPI\IntegrationTests\Fixtures\DataBaseFixtures.cs` file to see how the `*.sql` files included in the `IntegrationTests` project are used.
    1. Please see the `Integration Tests README.md` for details on the sequence of "high level" steps used for automatic database creation.
    1. The constructor of `TransientDatabase` class contains the exact sequence of SQL script execution used to create the DBs.
2. Run the `SQL Changes Manager` project to "apply" changes.
 
Please note that the `IntegrationTests` project is designed to work within "development" environments (where the two "for development" main databases already exist, and should NOT be altered when running tests) and "Test" environments (where the databases do not pre-exist), which means that the tests project includes logic to temporarily rename databases and handle failures (within reason).

The scripts in the `IntegrationTests` project are the ones **Recommended** to get started because they are the only ones we expect to maintain (so to ensure tests do work), but they operate on databases called `tempTestReviewer` and `tempTestReviewerAdmin`, for things to work, you will need to rename them to `Reviewer` and `ReviewerAdmin` before running the SQL Changes Manager.
 
Once the two Databases exist, contain some data (also done via scripts that can be found in the `IntegrationTests` project) and are up to date (in the sense of their data-structures/functionalities) it is possible to attempt to run EPPI Reviewer 6 (or 4). Please refer to specific readme files to learn how.

On the other hand, after doing the above the 1st time, whenever new code is pulled into your local repository, you will NOT need to re-create the databases. You should "just" run the `SQL Changes Manager` project, to ensure the databases structures and functionalities are matching the rest of the code correctly.

## License, Contributions and related details


The source code included in this repository is covered by the [FSL-1.1-MIT license](LICENSE.md). In a nutshell, this means that **this software is NOT Open Source in a strict sense**, EPPI Reviewer is thus a "Source Available" software, although with significant additional permissions. People can:

- View, download, clone, fork this repository.
- Modify it for their own purposes, **as long as their purpose is NOT to create a for-profit software that is a direct competitor to EPPI Reviewer**.
- Do anything you like with the source code, when it's at least 2 years old - whereby the simple MIT license applies.

Not being quite Open Source, this repository is not Open to external contributions. There are two reasons for this, the first one being sufficient: we are a very small team and do not have the resources necessary to manage external contributions. The second reason is that we are not in the habit of asking for free work in return of "gratitude" and/or nothing substantial.
This is reflected in our [Contributing policy/file](CONTRIBUTING.md).





