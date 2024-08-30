# ER Central API Readme
The purpose of this file is to provide a short description of the projects, folders and files found in the `ERCentralAPI` folder of this repository.

## Visual Studio Solutions and Projects

The ERCentralAPI folder contains a single Visual Studio "Solution" with multiple projects inside it. The solution file `ERCentralAPI.sln` as you'd expect.

Projects:

- `ER-Web-API` (corresponds to the `ER-Web` folder), this project is a DotNet6 (as of Aug 2024) application which is loosely based on an MVC paradigm: it has controllers, models, and serves JSON in response to GET and POST requests. This project/API functions as the EPPI Reviewer 6 (ER6) API.
- `ER-Web-Client` is an Angular project that functions as the ER6 Client. To learn how these two projects "work together" please see the [ER6-README](ER6-README.md) file.
- `IntegrationTests` is a project with very limited integration tests for ER6. Data consumed/used in ER overwhelmingly takes the form of "links", as in "applying a concept to a reference", for this reason, we find that most bugs show up when a change in the logic governing one kind of "link" has unexpected consequences on something else. For this reason, UnitTests are thought of being of limited value for this project, while integration tests (where integration refers to the full stack, but also to integration over time!) could potentially be of maximum value.
- `SQL Changes Manager` is a "Utility" DotNet Core2.1 Command line Application used to Apply ordered changes to the ER Databases. All SQL changes get applied "in order of appearance" to development, test and production databases. These appear as numbered scripts inside this project. Running this project applies "new" changes automatically.
- `WebDatabasesMVC` is the project functioning as "EPPI Visualiser". It is a DotNet 6 app, following the MVC pattern quite closely, although the views rely heavily on Javascript/Ajax calls to make the user experience as smooth as possible.

## Models used by ER-Web-API and WebDatabasesMVC

In the [general readme](../README.md) we mentioned that much of the (server side) code is shared between EPPI Reviewer 4 (ER4), ER6 and EPPI Visualiser. This shared code consists mostly of [CSLA Business Objects](https://www.nuget.org/packages/CSLA-ASPNETCORE-MVC/4.7.200), which were, for the most part written well before ER6 came into existence. For this reason they exist in the ER4 folder (`[repository root]\EPPIReviewer4.2015\Server\BusinessClasses`) and are then added to ER6 or EPPI Vis projects as "existing file\add as link". Controllers in these newer "MVC" projects then use these files as if they were standard MVC Models. They however are **not** standard models, as CSLA include their own Create/Read/Update/Delete (logic), so they follow a paradigm closer to that of "active records".

For simplicitly\clarity, within the MVC projects, we add the CSLA objects in a "Models" folder. 

In the case of ER6, when writing a new functionality and when the developers are sure that the new functionality will never be implemented in ER4, they sometimes add files to the `[..]\ERCentralAPI\ER-Web\Models\` folder, this is especially useful when the code therein makes use of syntax/functionalities that are not compatible with .Net 3.5/Silverlight as it eliminates the need of making sure it compiles also in the ER4 environment. As a result, the source code for "Models" used may sit in two (far from one-another) folders.

In the case of EPPI Visualiser (`WebDatabasesMVC`), the authentication/authorisation system it uses is radically different from ER4-6, and the corresponding "alternative" files / Business Objects are stored in `ERCentralAPI\WebDatabasesMVC\Models\Security\`.

## Other files/folders

The `Dlls` folder contains a single file called `EPPIiFilter.dll` on which both ER4 and ER-Web-API depend. This file is used when uploading PDFs and allows ER to load and use (if one is installed in the Operative System) a PDF-Filter that extracts simple text from PDFs (this text is mostly used in the search functions of ER).

The `SQLScripts` folder contains mostly outdated scripts which were used at different points in ER's complicated history. The one file which is still of use is `ConditionalCheckTemplates.sql` which developers consult to write the numbered "SQl changes" files consumed by the `SQL Changes Manager` utility. These files need to "re-runnable" without producing errors, which in turn is done by "conditional checks", for which we maintain a library of examples in in this file.