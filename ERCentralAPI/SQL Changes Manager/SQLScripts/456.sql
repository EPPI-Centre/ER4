USE [ReviewerAdmin]
GO

select * from TB_ONLINE_HELP
where CONTEXT = 'sources\file'
if @@ROWCOUNT = 0
begin
	insert into TB_ONLINE_HELP (CONTEXT, HELP_HTML)
	VALUES ('sources\file', 'tmp_place_holder')
end
GO


declare @Content nvarchar(max) = '
<p class="font-weight-bold">Import Items</p>
<p>Most databases will allow you to download your search results into a file. If possible, try to save the results into an RIS formated file. RIS is a common file format for transferring references between different reference management software. You might also have your search results saved in reference management software such as EndNote. From EndNote you will be able to export your references into an RIS formated file. </p>

<div class="container-fluid">

<div class="row">
	<div class="col-sm">		
	<b>Select file</b><br>
	<img class="img"  src="Images/ImportItems01.png" /><br>
	Select the format of the file to be imported. RIS is the default format although other formats are available. If the needed format is not listed you can use the <a href="https://eppi.ioe.ac.uk/cms/Default.aspx?tabid=2934" target="_blank">RIS EXPORT</a> utility on Gateway to convert your file to RIS.<br>
	Click <b>Select file</b> to upload your file.<br><br>
	</div>
	<div class="col-sm">		
	<b>Preview</b><br>
	<img class="img"  src="Images/ImportItems02.png" /><br>
	Once the file is uploaded click <b>Show Preview</b> to see a summary of the items you will be importing. <br><br>
	</div>
</div>	

<div class="row">
<div class="col-sm">		
	<b>Import</b><br>
	<img class="img"  src="Images/ImportItems03.png" /><br>
	</div>
	<div class="col-sm">
	<br>The imported items will be grouped as a source. You can edit the <b>Source name</b> to make it unique.<br>
	There are also text boxes available to enter a description and notes regarding the search that has been carried out. These details will become part of the sources log. <br>
	Click <b>Import</b> to save your edits and bring the records into your review.<br><br> 
</div>
</div>

</div>

'
UPDATE TB_ONLINE_HELP
SET [HELP_HTML] = @Content
	WHERE [CONTEXT] = 'sources\file'

GO


USE [ReviewerAdmin]
GO

select * from TB_ONLINE_HELP
where CONTEXT = 'sources\managesources'
if @@ROWCOUNT = 0
begin
	insert into TB_ONLINE_HELP (CONTEXT, HELP_HTML)
	VALUES ('sources\managesources', 'tmp_place_holder')
end
GO


declare @Content nvarchar(max) = '
<p class="font-weight-bold">Source Management</p>
<p>From the <b>Manage sources</b> tab you can manage, view and edit the details of each imported source.</p>

<div class="container-fluid">

<div class="row">
	<div class="col-sm">		
	<b>Source details</b><br>
	<img class="img"  src="Images/ManageSources01.png" /><br>
	<br><br>
	</div>
	<div class="col-sm">		
	<br>
	Select a source from the right side of the screen to display the details of that source.<br>
	The statistics of the source are listed in the <b>Source Stats</b> panel.<br>
	On the left side of the screen you can edit the source details such as the Source name, database,
	Description and Notes. Be sure to click <b>Save Changes</b> when finished making edits.<br>
	<br>
	<b>Deleting a source</b><br>
	Permanently deleting a source is a 2-step process.
	<ol>
	  <li>It must first be marked as deleted on the <b>Review home</b> page in the sources panel.</li>
	  <li>Once marked as deleted and the source does not contain any master items of duplicate references, 
	  the <b>Delete</b> button will be enabled and available to permanently delete the source.</li>
	</ol>
</div>
</div>	

</div>

'
UPDATE TB_ONLINE_HELP
SET [HELP_HTML] = @Content
	WHERE [CONTEXT] = 'sources\managesources'

GO

USE [ReviewerAdmin]
GO

select * from TB_ONLINE_HELP
where CONTEXT = 'sources\pubmed'
if @@ROWCOUNT = 0
begin
	insert into TB_ONLINE_HELP (CONTEXT, HELP_HTML)
	VALUES ('sources\pubmed', 'tmp_place_holder')
end
GO


declare @Content nvarchar(max) = '
<p class="font-weight-bold">PubMed Import</p>
<p>Some databases such as PubMed allow external programs to access their data directly. EPPI-Reviewer allows you to connect to this database to run your searches and return your search results.</p>

<div class="container-fluid">

<div class="row">
	<div class="col-sm">		
	<b>Search string</b><br>
	<img class="img"  src="Images/PubMed01.png" /><br>
	Enter you search string in the text box. The format of the search string should be identical to how you would enter it on the PubMed website (<a href="https://pubmed.ncbi.nlm.nih.gov" target="_blank">https://pubmed.ncbi.nlm.nih.gov</a>). Click <b>Search PubMed</b> to start the search.<br><br>
	</div>
	<div class="col-sm">		
	<b>Preview results</b><br>
	<img class="img"  src="Images/PubMed02.png" /><br>
	Once the search is complete click <b>Show Preview</b> to see a summary of the items you will be importing. You can adjust the range of the preview using the <b>from</b> and <b>to</b> number pickers.<br><br>
	</div>
</div>	

<div class="row">
<div class="col-sm">		
	<b>Import</b><br>
	<img class="img"  src="Images/PubMed03.png" /><br>
	</div>
	<div class="col-sm">
	<br>The imported items will be grouped as a source. You can edit the <b>Source name</b> to make it unique.<br>
	There are also text boxes available to enter a description and notes regarding the search that has been carried out. These details will become part of the sources log. <br>
	You can restrict the number of records to import using the <b>from</b> and <b>to</b> number pickers. You are restricted to batches of 10,000 items at a time.<br>
	Click <b>Import</b> to save your edits and bring the records into your review.<br><br> 
</div>
</div>

</div>

'
UPDATE TB_ONLINE_HELP
SET [HELP_HTML] = @Content
	WHERE [CONTEXT] = 'sources\pubmed'

GO


