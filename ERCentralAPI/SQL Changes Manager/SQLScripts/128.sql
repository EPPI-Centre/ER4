USE [ReviewerAdmin]
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
	Searches in the search table are <b>fixed in time</b> and represent the data at the time the search was run. They do not reflect any changes to
	the data that have taken place since the search was run.<br>  
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
	You can display the scores as a histogram divided into 10 even sections and automatically assign codes to the items in each section.<br><br>
	</div>
</div>

</div>
'
UPDATE TB_ONLINE_HELP
SET [HELP_HTML] = @Content
	WHERE [CONTEXT] = 'main\search'

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
	The coding information is displayed in a table and is divided up based on coding tools and the reviewer who did the coding.<br>
	To view the full coding for an item based on a particular coding tool and reviewer combination click on the appropriate <b>View</b> button.  The coding is displayed in a new browser tab. Clicking on the <b>Save</b> icon will save the coding as an html file that can be opened in Word or Excel.<br>
	To display a live comparison of coding select the coding tool (or section in the coding tool) and click the <b>Live comparison</b> button. The coding, including any text entered in the Info box, for each person will be displayed. Live comparison is useful when multiple reviewers (3+) have screened an item and you need to view everyones work in one screen.<br>
	For comparing coding in complex coding tools select the required coding tool/reviewer combinations and click the <b>Run comparison</b> button. A comparison will be displayed in a new tab with each reviewer''s coding in a different colour.</br>
	<b>Note</b>: You must have popups enabled in your browser to <b>View</b> coding and <b>Run comparisons</b> as they are displayed in a new tab.
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
