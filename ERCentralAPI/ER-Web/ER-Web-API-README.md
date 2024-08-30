# ER-Web-API readme

The purpose of this file is mostly to mention dependencies and licensing requirements for the ER-Web-API project, which constitutes the ER6 API / server-side project.

For more details about how the ER6 Client "works together" with the ER6 API, please see the [ER6-README](../ER6-README.md) file.

## Dependencies/licenses

Most dependencies are delivered as NuGet packages, and as such they will be "restored" automatically upon running/debugging this project. There are three required DLLs that will not be downloaded as NuGet packages:
- DeployR7.4
- DeployRBroker7.4
- EPPIiFilter

The first two are stored in `[..]\EPPIReviewer4.2015\Solution Items\DeployR7.4.dll` (they are shared with ER4) and the latter one in `[..]\ERCentralAPI\Dlls` (same source code as the one used in ER4, but compiled for x64 processors). Visual studio (via the Project file) should know where to find them, so they should all "work" right out of the box and not require any special setup operation.

DeployR was an open source project which is now (very) defunct and I could not find any trace of its original documentation. To the best of my knowledge, you do not need to purchase a license in order to use it.

## Other dependencies

ER6 naturally depends on a ever-increasing number of external / third party APIs and services. These incude the Machine Learning (ML) pipelines and storage services that drive many ML-powered EPPI Reviewer features. For the most part, these are hosted on Azure, and paid-for by the EPPI Centre. 

To make features that depend on Azure paid-for services, ER needs to be instructed about their URL/Endpoint and given an access key of one sort or the other. These values are stored in the `appsettings.json` file. Naturally, at least the access key (or similar) needs to remain secret and are thus not included in this project (they appear as `"**Placeholder**"` or `"**Removed**"` in the source code). For this reason, all these functionalities cannot work "out of the box".

It is our firm intention to release the source code for all our Machine Learning-driven functionalities (AKA, all those things that run in the Azure Machine Learning environment), however at this time, we could not sort out all the technical obstacles involved, meaning that we don't currently know if we'll eventually release the source code within this same repository, in a "twin" repository, or somewhere else (sorry!).

ER depends also on other third party services, such as PubMed, OpenAlex and more. Where we have purchased and API-Key or similar, the same limitation applies: whatever functionality of EPPI Reviewer that requires priviledged access to the relevant API might not (is likely to not) work until you'll provide your own (possibly paid for) API Key.

Examples (might not cover all 3rd party / external services!):

- Zotero Integration: this requires a ClientID and a ClientKey. They are obtainable from the Zotero website at no cost.
- OpenAi GPT API: ER can consume Azure-deployments of the GPT API (paid for via the Azure platform) or the native GPT API. In both case, keys are required and the service is paid-for.
- OpenAlex: we have procured an API Key for the OpenAlex API, which provides access to personalised support and allows to enjoy faster API response times. 
- DeployR: this is used to run Meta-Analyses in R and relies on a separate system deployed in Azure (Which is paid for by the EPPI Centre).
- Clustering: as above, this relies on a third part product (Lingo3G) for which we bought the license and which runs in Azure.

The latter 2 dependencies are _expected_ do be changed in the short-medium term. The current plan (as of Aug 2024) is to re-implement these functionalities inside the Azure Machine Learning environment.