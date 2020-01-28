Use ReviewerAdmin
GO

select * from TB_ONLINE_HELP
where CONTEXT = 'duplicates'
if @@ROWCOUNT = 0
begin
	insert into TB_ONLINE_HELP (CONTEXT, HELP_HTML)
	VALUES ('duplicates', 'tmp_place_holder')
end
GO
declare @Content nvarchar(max) = '
<p class="mb-1">
	This page provides <b>basic support</b> for the most used deduplication functionalities. It <b>does not</b> yet support all the advanced functionalities available in EPPI-Reviewer 4. The functionalities available here mirror precisely those in EPPI-Reviewer 4 so please refer to the <a href="https://eppi.ioe.ac.uk/cms/Default.aspx?tabid=2933" target="_blank">user manual</a> for further details.
</p>
<div class="container">
	<div class="row">
		<div class="col-md-5 col-12 bg-light rounded mx-0">	
			<p class="font-weight-bold alert alert-success mt-2">
				Supported features:
				<!-- You can use this page to trigger the "Get new Duplicates" routine, and evaluate the results to mark items as "Duplicate" or "Not Duplicate". This page also supports "Mark automatically" and "Advanced Mark Automatically". However, this page <b>Does NOT support</b> the following: -->
			</p>
			<ol>
			 <li>Get new duplicates (finds <em>possible duplicates</em>).</li>
			 <li>Mark as Duplicate / Not a Duplicate.</li>
			 <li>Change "Master" for a Group of Duplicates.</li>
			 <li>Mark automatically.</li>
			 <li>Advanced mark automatically (allows you to change thresholds).</li>
			</ol>
		</div>		
		<div class="mx-1 col-1"></div>
		<br>
		<div class="col-md-5 col-12 bg-light rounded mx-0">	
			<p class=" alert alert-warning mt-2">
				<b>Not supported features</b> (please use <a href="http://eppi.ioe.ac.uk/eppireviewer4/" target="_blank">EPPI-Reviewer 4</a>):
			</p>
			<ol>
			 <li>Manually adding and removing duplicates to an existing group.</li>
			 <li>Manually creating new groups.</li>
			 <li>Searching for groups.</li>
			 <li>Deleting Groups.</li>
			 <li>Resetting the duplicates data.</li>
			</ol>
		</div>
	</div>
</div>

<p class="mt-2 mb-1">
	Until full duplication functionality is available you may still be required to use <a href="http://eppi.ioe.ac.uk/eppireviewer4/" target="_blank">EPPI-Reviewer 4</a>.
</p>
<br>

<div class="container-fluid">
<div class="row">
	<div class="col-sm">		
	<b>Duplicates</b><br>
	<img class="img"  src="Images/Duplicates.gif" />
	</div>
	<div class="col-sm">
	<br>
	After clicking <b>Get new duplicates</b> to identify possible duplicate items, the duplicate groups are listed on the left and the items within a group 
	are displayed on the right. The master item is at the top while the possible duplicate items are listed below it.<br>
	Differences between items are highlighted in different colours based on the degree of difference. The overall difference between the master item and 
	each possible duplicate item is represented by a similarity score. A similarity score of 1.0 indicates an exact duplicate. As the degree of difference 
	increases, the similarity score decreases (0.98, 0.95, etc.)<br>
	Items can be marked as <b>A duplicates</b>, <b>Not a duplicates</b> or <b>Mark as master</b> using the appropriate buttons.<br>
	The <b>Mark automatically</b> function will automatically mark all items with a similarity score of 1.0 as a duplicate. Select 
	<b>Advanced Mark Automatically</b> to lower the similarity score threshold, the coded item threshold and the uploaded documents threshold.
	</div>
</div>
<br><br>

<div class="mx-3 mt-0 rounded border border-danger bg-light">
	<p class="m-2 alert alert-danger">
		<b>If you do not have access to EPPI-Reviewer 4:</b>
	</p> 
	<p class="mx-2">(Applies to users of MacOS "Catalina" from version 10.15.) To ensure you won''t need to use the advanced deduplication functionalities that are not yet available in EPPI-Reviewer Web, it is <b>very important</b> to proceed as follows:
	
	<ol >
	 <li>Import <b>all your search results</b>, without triggering "Get new Duplicates".</li>
	 <li>Once you have imported all items that need deduplicating, click "Get new Duplicates" exactly once.</li>
	</ol>
	<p class="mx-2">
		This will prevent the appearance of "overlapping groups", which (currently) require access to EPPI-Reviewer 4 (see the <a href="https://eppi.ioe.ac.uk/cms/Default.aspx?tabid=2933" target="_blank">user manual</a> for details).
	</p>
</div>
<p>
'
UPDATE TB_ONLINE_HELP
SET [HELP_HTML] = @Content
	WHERE [CONTEXT] = 'duplicates'

GO


declare @Content nvarchar(max) = '
<div class="container-fluid">
<div class="row">
	<div class="col-sm">		
	<b>Review home</b><br>
	<img class="img"  src="Images/CO_Home.gif" />
	</div>
	<div class="col-sm">
	<br>
	From the <b>Review home</b> screen in the coding interface you can access the studies from your coding assignments.<br>
	Your coding assignments are listed on the left of the screen. Clicking on the links for <b>Allocated</b>, <b>Started</b> and <b>Remaining</b> will list the appropriate studies 
	on the right side of the screen.<br>
	You can change the nunber of items in a page and the fields displayed by clicking <b>View options</b> and changing the parameters.<br>
	Click on <b>Go</b> to access an item in the list of studies.<br>
	You can also access your reviews by clicking on the <b>My reviews</b> button. From the list you can access a reviews using either the 
	Coding interface or the Full interface.<br>
	</div>
</div>
<br><br>


<br>
'
UPDATE TB_ONLINE_HELP
SET [HELP_HTML] = @Content
	WHERE [CONTEXT] = '(codingui)main'

GO


select * from TB_ONLINE_HELP
where CONTEXT = '(codingui)itemdetails\pdf'
if @@ROWCOUNT = 0
begin
	insert into TB_ONLINE_HELP (CONTEXT, HELP_HTML)
	VALUES ('(codingui)itemdetails\pdf', 'tmp_place_holder')
end
GO

declare @Content nvarchar(max) = '
<p class="font-weight-bold">PDF Coding</p>
<p>The <b>PDF</b> tab allows line by line coding of textual data to help identify descriptive and analytic themes in your documents.</p>

<div class="container-fluid">
<div class="row">
	<div class="col-sm">		
	<b>PDF Coding</b><br>
	<img class="img"  src="Images/CO_ItemDetails_Pdf.gif" />
	</div>
	<div class="col-sm">
	<br>
	To use the line-by-line PDF coding functionality you must upload your PDF to EPPI-Reviewer. This can be done in the <b>Item Details</b> tab by clicking the <b>Upload</b> button at the bottom of the screen. Uploaded documents are listed at the the bottom of the screen. Clicking the <b>View</b> icon displays the document in the <b>PDF</b> tab.<br>
	To assign text to a code, select a code in the appropriate coding tool, select the corresponding text in the displayed document and then click the <b>A</b>ssign icon. The selected text is then available to the all of the program''s searching and reporting functions.<br> Removing text from a code is the same process but you should click <b>Delete</b> icon.<br>
	</div>
</div>
</div>

<br>
<br>
'
UPDATE TB_ONLINE_HELP
SET [HELP_HTML] = @Content
	WHERE [CONTEXT] = '(codingui)itemdetails\pdf'

GO


declare @Content nvarchar(max) = '
<p class="font-weight-bold">Items details</p>
<p>The <b>Items details</b> screen is where much of the data entry in EPPI-Reviewer takes place. You also have access to Pdf coding using the tab above the citation. Further help is available in the tabs.</p>

<div class="container-fluid">
<div class="row">
	<div class="col-sm">		
	<b>Coding</b><br>
	<img class="img"  src="Images/CO_ItemDetails_Coding.gif" />
	</div>
	<div class="col-sm">
	<br>
	<b>Coding</b> is carried out in the Item Details screen. On the right side of the screen is the citation while on the left are the coding tools.<br> Clicking a check-box next to a code assigns that code to the item. Clicking the <b>Info</b> buttons allows you to add textual information to your selected code.<br>
	To remove coding, de-select a check-box and confirm your action.<br>
	To move through your references use the <b>Previous</b>, <b>Next</b>, <b>First</b> and <b>Last</b> buttons.<br>The <b>Auto-advance</b> option will automatically move to the next item when you select a code.
	</div>
</div>
<br><br>

<div class="row">
	<div class="col-sm">		
	<b>Numeric outcome data</b><br>
	<img class="img"  src="Images/CO_ItemDetails_Outcomes.gif" />
	</div>
	<div class="col-sm">
	<br>
	You can enter numeric outcome data into EPPI-Reviewer using forms specific to the type of outcomes found in your studies.<br>
	Any code with the codetype <b>Comparison</b>, <b>Outcome</b> or <b>Intervention</b> will have an <b>Outcomes</b> button next to it. 
	Clicking on the <b>Outcomes</b> button will open a panel allowing you to edit an existing outcome or create a new outcome.<br>
	To create a new outcome	specifiy the type (continuous or binary) and the format of the numeric outcome data and the form will 
	change accordingly.<br>
	Any predefined arms, time points, interventions, comparisons and outcomes are available for selection. As well you can select any codes 
	within your coding tool to help characterise the outcome data.<br>
	All saved outcome data is available to EPPI-Reviewer''s reporting and meta-analyis functions.
	</div>
</div>	

<br><br>
<div class="row">
<div class="col-sm">		
	<img class="img"  src="Images/FindEdit.png" /><br>
	</div>
	<div class="col-sm">		
	<b>Find on Web</b><br>
	Use the <b>Find on Web</b> dropdown to generate a search for your item in Microsoft Academic, Google, or Google scholar.<br>
	The search will be based on the title and author of the reference and the results will be displayed in a new tab.<br>
	</div>
	<div class="col-sm">
	<b>Edit reference</b><br>
	To edit your reference click on the <b>Edit</b> button. A form will appear allowing you to edit the reference fields.<br>
	</div>
</div>	


<br>
'
UPDATE TB_ONLINE_HELP
SET [HELP_HTML] = @Content
	WHERE [CONTEXT] = '(codingui)itemdetails'

GO