USE [ReviewerAdmin]
GO




declare @Content nvarchar(max) = '
<p class="font-weight-bold">Items details</p>
<p>The <b>Items details</b> screen is where much of the data entry in EPPI-Reviewer takes place. You also have access to a number of other areas in the program using the tabs on the right side of the screen. Further help is available in those tabs.</p>

<div class="container-fluid">
<div class="row">
	<div class="col-sm">		
	<b>Coding</b><br>
	<img class="img"  src="Images/ItemDetails-Coding.gif" />
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
	<b>Edit coding tools</b><br>
	<img class="img"  src="Images/ItemDetails-EditCodes.gif" />
	</div>
	<div class="col-sm">
	<br>
	You can <b>add</b> and <b>edit</b> codes using the buttons on the left side of the screen (sitting above the coding tools).<br>
	To add a code select the coding tool and a location in the coding tool (if the tool has multiple levels). Click on the <b>+</b> button and enter a code type, code name and code description. Click <b>Create</b> and you will see the new code in the coding tool.<br>
	You can reposition the code by clicking on the <b>Up</b> and <b>Down</b> arrow buttons.<br>
	To edit a code, select the code and click the <b>Edit</b> button (looks like a pencil). This will allow you to edit the code type, code name and description. To save your edits click <b>Update</b>.<br>
	Deleting a code is the same as editing a code except you must click the <b>Delete Code</b> button and confirm your actions. 
	</div>
</div>	
</div>

<br><br>
'
UPDATE TB_ONLINE_HELP
SET [HELP_HTML] = @Content
	WHERE [CONTEXT] = 'itemdetails'

GO



select * from TB_ONLINE_HELP
where CONTEXT = 'itemdetails\codingrecord'
if @@ROWCOUNT = 0
begin
	insert into TB_ONLINE_HELP (CONTEXT, HELP_HTML)
	VALUES ('itemdetails\codingrecord', 'tmp_place_holder')
end
GO


declare @Content nvarchar(max) = '
<p class="font-weight-bold">Coding Record</p>
<p>The <b>Coding Record</b> tab provides a summary of all coding that has taken place on an item across all coding tools. You can also compare multiple reviewer''s coding when comparison coding has taken place.</p>

<div class="container-fluid">
<div class="row">
	<div class="col-sm">		
	<b>Coding Record</b><br>
	<img class="img"  src="Images/ItemDetails-CodingRecord.gif" />
	</div>
	<div class="col-sm">
	<br>
	The coding is divided up based on coding tools and the reviewer who did the coding and is displayed in a table.<br>
	To view the full coding for an item based on a particular coding tool and reviewer combination click on the appropriate <b>View</b> button.  The coding is displayed in a new browser tab. Clicking on the <b>Save</b> icon will save the coding as an html file that can be opened in Word or Excel.<br>
	To display a live comparison of coding select the coding tool (or section in the coding tool) and click the <b>Live comparison</b> button. The coding, including any text entered in the Info box, for each person will be displayed. Live comparison is useful when multiple reviewers (3+) have screened an item and you need to view everyones work in one screen.<br>
	For comparing coding in complex coding tools select the required coding tool/reviewer combinations and click the <b>Run comparison</b> button. A comparison will be displayed in a new tab with each reviewers coding in a different colour.
	</div>
</div>
</div>

<br>
<br>
'
UPDATE TB_ONLINE_HELP
SET [HELP_HTML] = @Content
	WHERE [CONTEXT] = 'itemdetails\codingrecord'

GO


select * from TB_ONLINE_HELP
where CONTEXT = 'itemdetails\pdf'
if @@ROWCOUNT = 0
begin
	insert into TB_ONLINE_HELP (CONTEXT, HELP_HTML)
	VALUES ('itemdetails\pdf', 'tmp_place_holder')
end
GO



declare @Content nvarchar(max) = '
<p class="font-weight-bold">PDF Coding</p>
<p>The <b>PDF</b> tab allows line by line coding of textual data to help identify descriptive and analytic themes in your documents.</p>

<div class="container-fluid">
<div class="row">
	<div class="col-sm">		
	<b>PDF Coding</b><br>
	<img class="img"  src="Images/ItemDetails-PdfCoding.gif" />
	</div>
	<div class="col-sm">
	<br>
	To use the line-by-line PDF coding functionality you must upload your PDF to EPPI-Reviewer. This can be done in the <b>Item Details</b> tab by clicking the <b>Upload</b> button at the bottom of the screen. Uploaded documents are listed at the the bottom of the screen. Clicking the <b>View</b> icon displays the document in the <b>PDF</b> tab.<br>
	To assign text to a code, select a code in the appropriate coding tool, select the corresponding text in the displayed document and then click the <b>A</b>ssign icon. The selected text is then available to the all of the program''s searching and reporting functions.<br> Removing text from a code is the same process but you should click <b>Delete</b> icon.<br>
	Collecting descriptive and analytic themes often requires creating codes on-the-fly as you move through your document. You can dynamically add codes to your coding tool by clicking the <b>+</b> icon that can be found above the coding tool panel.
	</div>
</div>
</div>

<br>
<br>
'
UPDATE TB_ONLINE_HELP
SET [HELP_HTML] = @Content
	WHERE [CONTEXT] = 'itemdetails\pdf'

GO







declare @Content nvarchar(max) = '
<p class="font-weight-bold">Arms and Timepoints</p>
<p>The <b>Arms and Timepoints</b> tab has functionality to assist in dealing with multiple arm trials and the entry of numeric outcome data at specific timepoints.</p>

<div class="container-fluid">
<div class="row">
	<div class="col-sm">		
	<b>Arms and Timepoints</b><br>
	<img class="img"  src="Images/ItemDetails-Arms.gif" />
	</div>
	<div class="col-sm">
	<br>
	To create a new <b>arm</b> enter a name for the arm and click the <b>Add New Study Arm</b> button. The new arm will be displayed in a table where you can also <b>Edit</b> or <b>Remove</b> the arm.<br>
	The new arm will also be listed in a dropdown menu at the top of the coding tool panel. An arm is specific to the item it was created for so all coding carried out on the item will be specific to the selected arm. The <b>Whole Study</b> option is for coding not tied to a specific arm.<br><br>  
	To create a <b>timepoint</b> enter a numeric value for the duration, select the time unit from the dropdown menu and click the <b>Add Timepoint</b> button. The new timepoint will be displayed in a table where you can also <b>Edit</b> or <b>Remove</b> the timepoint.<br>
	Timepoints are specific to the item it was created for and are selectable when entering numeric outcome data using <b>Outcome</b>, <b>Intervention</b> and <b>Comparison</b> codes.<br><br>
	When entering numeric outcome data any arms created for the item are also selectable in the on-screen numeric outcome data entry form.<br> 
	</div>
</div>
</div>

<br>
<br>
'
UPDATE TB_ONLINE_HELP
SET [HELP_HTML] = @Content
	WHERE [CONTEXT] = 'itemdetails\arms'

GO





declare @Content nvarchar(max) = '
<p class="font-weight-bold">Search and Classify</p>
<p><b>Searching</b> - all data entered into EPPI-Reviewer is held in a powerful SQL database enabling quick, sophisticated and sensitive searching.<br>
<b>Classifiers</b> allow you to identify items based on previous coding. You can click <b>Build Model</b> to create your own custom classifiers and 
click <b>Classify</b> to run your model or one of EPPI-Reviewer''s pre-configured classifiers.</p>

<div class="container-fluid">

<div class="row">
	<div class="col-sm">		
	<b>Searching</b><br>
	<img class="img"  src="Images/Search.gif" /><br><br>
	</div>
	<div class="col-sm">
	<br>
	Click <b>New Search</b> and select the type of search from the dropdown menu. Depending on the type of search being run, choose a code or 
	coding tool or enter some text, and then click <b>Run Search</b>.<br>
	Each search is stored as a row in the search table allowing you to access the results.<br>
	To combine results, select the relevant searches and choose a Boolean operator from the <b>Combine</b> dropdown.<br>
	To remove a selected search click <b>Delete Selected</b>.<br>  
	Searches are restricted to references with the <b>I</b> or <b>E</b> flags.<br><br>
	</div>
</div>	

<div class="row">
	<div class="col-sm">		
	<b>Build Model</b><br>
	Clicking on the <b>Build Model</b> button will take you to the Build Model page.<br> 
	Further help on building a model can be found on that screen.<br><br>
	</div>
</div>

<div class="row">
	<div class="col-sm">		
	<b>Classify</b><br>
	<img class="img"  src="Images/Classify.gif" /><br><br>
	</div>
	<div class="col-sm">
	<br>
	After clicking <b>Classify</b> you should select from either a preconfigured model or any 
	custom models that you have created previously.<br>
	Next, select what items you want to run the classifier against and click <b>Run Model</b>.<br>
	The results will be displayd in the search table where you can list and order the items by their score.
	You can also display the scores as a histogram divided into 10 even sections.<br><br>
	</div>
</div>

</div>
'
UPDATE TB_ONLINE_HELP
SET [HELP_HTML] = @Content
	WHERE [CONTEXT] = 'main\search'

GO






