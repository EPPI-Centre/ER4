## Documentation on calling the EPPI Reviewer machine learning API through Azure Data Factory

The API endpoint in ADF is a multi-parameter endpoint that executes numerous pipelines on Azure Machine Learning.

There are many similarities between different features on the EPPI Reviewer side. Broadly speaking, the usual process is as follows:
* Retrieve a set of items from the EPPI Reviewer database (with, optionally 'labels')
* Save this set of items locally to a temp file on the webserver
* Upload this file to blob storage (to a specified location, that is passed as a parameter to ADF)
* Delete the local file
* Identify the appropriate parameters to use
* Call the ADF API using the appropriate parameters
* Wait until the process is run
* Then, either process results, saving to the database, or simply exit

ClassifierV2 is broken broadly into the following stages:

1. DataPortal_execute: this is the entry point for executing the object server-side

2. Check the process can run

3. Either QueryDbAndSaveTempFileWithLabels is called OR QueryDbAndSaveTempFileWithoutLabels (+ version for OpenAlex updates) - depending on whether the file will be used for training a classifier or not.

4. UploadBlob is then called, with the local file deleted once this is done

5. TriggerADF is then called, and the process sometimes exits here

6. Finally, DownloadResults is called to retrieve the file deposited by the API and the code branches depending on what needs doing with the results.

## API parameters

*EPPIReviewerApiRunId*: a GUID to identify the API 'run' (saved to the Azure SQL database)

File locations:
* *DataFile*: the file that is uploaded by UploadBlob
* *ScoresFile*: the file that is downloaded by DownloadResults
* *VecFile*: location where custom model vectorizer is stored
* *ClfFile*: location where custom model classifier is stored
* *doc_folder*: location where pdf files for parsing are stored
* *doc_output_folder*: location where markdown following pdf parsing should be stored
* *doc_container*: the production container

API calls for building models (use CreateLocalFileWithLabels); all apart from the one used for priority screening do not download and process results files:
* *do_build_and_score_log_reg* (priority screening)
* *do_build_log_reg*
* *do_check_screening*
* *do_priority_screening_simulation*

API calls for scoring models (use CreateLocalFileWithoutLabels) and process data on completion:
* *do_score_log_reg*
* *do_cochrane_rct_classifier_b*
* *do_original_rct_classifier_b*
* *do_economic_eval_classifier_b*
* *do_systematic_reviews_classifier_b*
* *do_covid_map_categories_b*
* *do_long_covid_svm_b*
* *do_pubmed_study_designs_b*
* *do_progress_plus_b*

Concerned with parsing pdfs / RAG pipeline:
* *do_parse_pdf*
* *doc_folder*
* *doc_output_folder*
* *doc_container*

