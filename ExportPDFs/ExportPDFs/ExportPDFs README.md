# ExportPDFs readme

This is a little DotNet Core 2.x app used to Export PDFs in bulk. Given a review, and optional criteria to select a subset of references therein, it will export all PDFs to a folder, which can then be packaged in a large Zip file (or alternative) for re-use elsewhere.

This utility is meant to be used by superusers and is unlikely it will be adapted for use by end users. The reason is that the volume of exported data can easily be huge and might thus not be suitable for normal operation over HTTP protocols.

## Additional features:

In its main operation, this utility will produce a `summary.csv` file listing items considered, whether they had PDFs/Documents to export and the relevant IDs. Optionally, this file can contain additional colums used to report about coding associated with a given item, where each column is a code and will report whether a given item is associated with that code or not.

Filenames of exported Bin/Pdf/Txt files exported takes the form:
`[ItemId]-[BinaryId].[ext]`


Alternatively, this utility can be used to re-process uploaded PDF documents if/when their full-text had failed to be extracted at upload time.

## Parameters

List of parameters (not case-sensitive):

- **[Required]** `RevId:NNN` used to specify the ReviewID. [Without this parameter, utility can't do any work]
- `rebuildtext`: tells the utility to attempt re-extracting the plain text for uploaded PDF documents for which plain text extraction had failed previously. Running the utility in this mode makes it ignore all other parameters except for `RevId:` and `WhatIf`.
- `WhatIf`: run in simulation mode, useful to check if it will work, how long it might take to run, how much data it may produce, etc.
- `incl`, `excl`, `both`: (mutually exclusive) parameters specifying whether the utility should only consider items with the `Include` flag (**Default**), only `Excluded` items or `both`.
- `OnlyThisCode:XXX`: while respecting the 3 parameters above, process items that have only the code specified by the `XXX` AttributeId. [Defaults is to not apply this additional filter.]
- `ExportBin` (default is `FALSE`) makes the utility export the original binary file that was uploaded.
- `ExportTXT` (default is `FALSE`) makes the utility export the extracted plain-text content of files.
- `columns:NNN[,MMM,XXX,YYY]` comma separated list of Attribute IDs (code IDs) to add as columns in `summary.csv`