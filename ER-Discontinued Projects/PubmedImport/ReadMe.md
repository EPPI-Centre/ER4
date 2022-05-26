

# PubmedImport For EPPI-Reviewer5.

### 1. Objective: This utility is developed to import PubMed data in bulk, [NIH documentation is here](https://www.nlm.nih.gov/databases/download/pubmed_medline.html): 

Following are the requirements that this utility aims to address:
* Download and import the "Annual Baseline files"
* Download and import "Daily Updates"
* Download and import an arbitrary PubMed XML file (via FTP only).


### 2. How-to import:
Production modes: in production, the following workflow is anticipated.

1. Once a year, after the "__Annual Baseline files__" are published, run a full import:

`dotnet PubmedImport.dll DoWhat:ftpbaselinefolder`

This command sends the program looking for the 'FTPBaselineFolder' indicated in the appsettings.json file. Utility will parse all XML files, processing 24+ million records. It will take many hours probably a few days to finish.

Note: This utility always checks if a citation with a given PubMed Id (PMID) is already present in the ER5 database. In such cases, the record will be updated, so there is no need to delete all known records beforehand.
PubMed files contains the history of given records, the same PMID appears in versioned copies (starting from 1), this means that during the import, for short periods, existing citations will revert to older versions and get re-enriched as the parsing progresses.

2. Once a day (or on a slightly more relaxed schedule), fetch the __daily updates__.

`dotnet PubmedImport.dll DoWhat:ftpupdatefolder`

This command downloads and processes all files found in the 'FTPUpdatesFolder' indicated in the appsettings.json file. It will record what files have been already imported succesfully, so will ignore all the files that correspond to existing PubMedUpdateFileImports records in the ER5 database. In normal coniditions, Utility will thus process new files and ignore the ones that have been imported already. When a citation is already known (based on PMID), it will be updated.

3. The two commands below are treated in the same way, even if the first refers to an "Annual Baseline" file and the second to an "update" file. The indicated file will be processed and imported.

`dotnet PubmedImport.dll DoWhat:singlefile file:ftp://ftp.ncbi.nlm.nih.gov/pubmed/baseline/pubmed18n0715.xml.gz`

`dotnet PubmedImport.dll DoWhat:singlefile file:ftp://ftp.ncbi.nlm.nih.gov/pubmed/updatefiles/pubmed18n0931.xml.gz`






### 3. Useful info: 

Utility outputs logging messages to the console. To save a log add the "savelog" option:
`dotnet PubmedImport.dll DoWhat:ftpupdatefolder savelog`
This will save (/append) a log to: [workingDir]\Logfiles\PubmedImportLog-DD-MM-YYYY.txt
