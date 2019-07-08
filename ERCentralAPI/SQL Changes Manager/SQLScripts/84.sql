Use ReviewerAdmin
GO

declare @Content nvarchar(max) = '
<p class="font-weight-bold">References</p>
<p>The References page lists your imported items and gives you access to individual items for coding. You can change what items are displayed base on your coding and change what fields are displayed in your list. You can bulk assign and remove codes from items as well as generate coding reports and export your items.</p>

<div class="container">
<div class="row">
	<div class="col-sm">		
	<b>Reference list</b><br>
	<img class="img"  src="Images/ReferenceList.gif" /><br>
	You can select individual or all items from your reference list. Clicking at the top of a column will order the list by that column. By default, the list is paged by 100 items. To list items based on the Include, Exclude and Delete flags click on the <b>I</b>, <b>E</b>, and <b>D</b> buttons. All imported items start with the <b>I</b> flag.<br><br>
	</div>
	<div class="col-sm">		
	<b>View options</b><br>
	<img class="img"  src="Images/ViewOptions.gif" /><br>
	Clicking on <b>View options</b> will allow to change what fields are shown in the reference list and edit the number of items displayed in each page.<br><br>
	</div>	
</div>

<div class="row">
	<div class="col-sm">
	<b>List items by coding</b><br>
	<img class="img"  src="Images/ListItemsByCoding.gif" /><br>
	To list items based on coding, select a code in a coding tool and click on <b>With this code</b>. The <b>With this code (Excluded)</b> option lists the coded items with the <b>E</b> flag.<br><br> 
	</div>
	<div class="col-sm">
	<b>Assign/remove codes</b><br>
	<img class="img"  src="Images/AssignCodeToItems.gif" /><br>
	To assign a code to multiple items, select the items and the code and then click <b>Assign code</b> to apply the code to the items. If you select <b>Remove code from selection</b> the code will be removed from the selected items.<br><br>
	</div>
</div> 

<div class="row">
	<div class="col-sm">
	<b>Coding report</b><br>
	<img class="img"  src="Images/CodingReport.gif" /><br>
	To generate a coding report select the items to include in the report and click <b>Coding report</b>. Next, select the coding tool(s) that will make up the report and click <b>Get report</b> to generate the report. You can <b>Save</b> the report as well as display it in a new tab for printing.<br><br>
	</div>
	<div class="col-sm">
	<b>Cluster</b><br>
	<img class="img"  src="Images/Cluster.png" /><br>
	Cluster uses the Lingo3G clustering engine to automatically generate a coded map of your references based on their abstracts. You can select what items should be included in the map and the complexity of the coding can be adjusted using the available parameters.<br><br>
	</div>
</div>

  <div class="row">	
	<div class="col-sm">
	<b>Quick Question Report</b><br>
	<img class="img"  src="Images/QuickQuestionReport.gif" /><br>
	To generate a question report select the items to include in the report and select <b>Quick Question Report</b>
	from the <b>Coding Report</b> dropdown menu.<br><br>
	</div>	
	<div class="col-sm">
	<br>
	You can select any code that has a child code, from any codeset, and it will become a new column in the report.<br>
	There are options to display the title, Info box text and pdf coded text in your report.<br>
	You can <b>Save</b> the report as well as display it in a new tab for printing.<br><br>
	</div>
</div>


<div class="row">
	<div class="col-sm">
	<b>Other options</b><br>
	Other options on this screen include:<br>
	<b>Import items</b> - takes you to the import items page. More help is available on that page.<br>
	<b>To RIS</b> - allows you to export your selected items as an RIS formatted text file.<br>	
	<br><br> 
	</div>
</div>
</div>
'
UPDATE TB_ONLINE_HELP
SET [HELP_HTML] = @Content
	WHERE [CONTEXT] = 'main\references'
GO


declare @Content nvarchar(max) = '
<p class="font-weight-bold">Review home</p>
<p>The Review home page gives you a summary of what is happening in your review and gives you access to many of the program''s functions.</p>
<div class="container">
  <div class="row">
	<div class="col-sm">
      <b>Coding progress</b><br>
	  <img class="img"  src="Images/CodingSummary.png" /><br>
	  On the left is an up-to-date summary of the coding that has taken place for each coding tool. Clicking on the refresh icon will update the numbers.<br>
	  [See also <em>below</em> for more advanced features.]
    </div>
    
    <div class="col-sm">
		<b>Coding Tools</b><br>
		<img class="img"  src="Images/codes.gif" /><br>
		On the right of the screen is the <b>Codes</b> button. Click on this button to show/hide your coding tools.<br>
    </div>	
  </div>
  <br>
  <div class="row">	
	<div class="col-sm">
	<b>Navigation</b><br>
	<img class="img"  src="Images/TopTabs.png" /><br>
	Along the top of the page are a number of tabs giving you access to different areas of the program. <b>References</b> are where you can see your listed references. 
	<b>Frequencies</b> reports, <b>Crosstabs</b> reports, <b>Search & Classify</b> and <b>Collaborate</b> can also be accessed from here. Further help is available on those pages.<br><br>
	</div>
	<div class="col-sm">
      <b>Reviews, My Work and Sources</b><br>
	  <img class="img"  src="Images/revWrkSources.gif" /><br>
	  In the middle are the <b>My Reviews</b>, <b>My Work</b> and <b>Sources</b> buttons giving you access to your reviews, coding assignments and sources.<br> 
	</div>
  </div>
  <div class="row">
      <div class="col-sm">
		<b>Coding progress Details</b><br>
		<img class="img"  src="Images/ReviewStats.gif" /><br>
		You can click on each coding tool to see the breakdown of completed and incomplete coding on a per Reviewer basis.
		For each reviewer row, you can:<br />
		<b>List Items</b> - click on the per-person number link to produce the corresponding list.<br>
		<b>Change Completions:</b> - click on the "completed" and "incomplete" icons to change the "completion" status of the corresponding references.<br>
	  </div>
		<div class="col-sm">
		<b>Other functions</b><br>
		<img class="img"  src="Images/OtherFunctionsMain.png" /><br>
		Below the tabs are buttons giving you access to 3 important areas of the program.<br>
		<b>List Included and Excluded Items</b> - click on the corresponding numbers in the "Review Items" section.<br>
		<b>Import Items</b> - is where you import your references into EPPI-Reviewer.<br>
		<b>Coding Tools</b> - allows you to set up your coding tools.<br>
		<b>Import Coding Tools</b> - allows you to access coding tools from your other reviews including publicly available coding tools.
		<br><br>Further help is available on those pages. 
	  </div>
   </div>
</div>
'
UPDATE TB_ONLINE_HELP
SET [HELP_HTML] = @Content
	WHERE [CONTEXT] = 'main\reviewhome'
GO