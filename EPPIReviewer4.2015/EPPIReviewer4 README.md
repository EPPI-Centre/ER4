# EPPI Reviewer 4 (ER4) Readme

EPPI Reviewer 4 is the first version of EPPI Reviewer which was explicitly written to support usage from external people not affiliated with the EPPI Centre.

It relies on Microsoft Silverlight, an outaded technology which has gone out of support a long time ago.

Therefore, its code is all 100% legacy code and it is not recommended to use it/inspect it.

At the time of writing (Aug 2024), ER4 is still in production, mostly because it contains some superuser functions that EPPI Centre people need to access on occasion, as they have not yet been implemented in EPPI Reviewer 6.

Thus, this readme file contains minimal information, of a "you really need to know" basis.

## Dependencies, requirements and licensing

First and foremost, to run this project within a development environment, you need Visual Studio version 2015(!!) and the Silverlight development runtime. We (at the EPPI Centre) have _preserved_ our access to both for legacy purposes, but we do not know how third parties may obtain either from scratch.

Visual Studio 2015 is paid-for software, so obtaining a license might pose a substantial problem, given how old it is.

`Silverlight` is terminally out of support and we believe that Microsoft has stopped supplying its runtime files, although we believe it's probably possible to obtain them from 3rd parties.

The ER4 client uses the `Telerik UI for Silverlight` (Version 2016.3.1024.1050) these controls are paid-for software and a license should be obtained by anyone wishing to use/modify the ER4 source code.

The CSLA framework is not "paid for" software. ER4 relies on its ancient (out of support/deprecated) version v4.3.13.0

The `Solution Items` folder contains additional Dlls which were/are required, most notably:

- `DevExpress.[..].Dll`: these files are not required anymore and you will not need a license for these.
- `MindFusion` DLLs are _required_ and you might be able to obtain a license from [Mindfusion](https://www.mindfusion.eu/) itself.
- Other DLLs (`EPPIiFilter.dll`, `Graphs1.dll`, `RefParsingLib.dll`) are assemblies developed in house (EPPI Centre) and you don't need a separate license to use them. 
- `DeployR7.4`, `Newtonsoft.Json` and `SharpZipLib`: these are supplied for convenience, and you do not need to pruchase licenses to use them (to the best of our knowledge).

## How it works

There are a number of separate projects, which all "work together" to allow the flow of "data and logic" between server and client.

These are:
- `Server`: this project is compiled into a `dll` on which the component running in the webserver depends. It contains the CSLA objects that carry data and logic to-from the client side.
- `Client`: the equivalent project to `Server`, but in this case it's the Silverlight client project that depends on it.
- `WcfHostPortal`: is a WCF project that delivers the Sliverlight binary to client and serves a WCF API to send/receive data. Depends on the `Server` project. [To run ER4 you need to configure the solution to have this project as the "startup" one].
- `EppiReviewer4` is the Sliverlight client that users interact with. Depends on the `Client` project.
- `CircularRelationshipGraph` an old open source project (not written by us) used by the Sliverlight Client.

