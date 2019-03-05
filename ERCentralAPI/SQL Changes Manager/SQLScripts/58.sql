USE [ReviewerAdmin]
GO


declare @Content nvarchar(max) = '
<p class="font-weight-bold">Welcome Page</p>
<p>From the Welcome page you can access your reviews and create a new review.</p>
<p>To access your review click on the review title.</p>
<p>To access your review using the Coding UI interface (optimised for smaller screens), click on the <b>Coding UI</b> button.</p>
<p>To create a new <i>private</i> review click on the <b>Create Review</b> button.<br>To make your new review a <i>shareable</i> review (so you can invite others into it) you will need to purchase a subscription in the 
<a href="https://eppi.ioe.ac.uk/cms/Default.aspx?tabid=2935" target="_blank">online shop</a>.</p>
'
UPDATE TB_ONLINE_HELP
SET [HELP_HTML] = @Content
	WHERE [CONTEXT] = 'intropage'          
GO


declare @Content nvarchar(max) = '
<p class="font-weight-bold">Review home</p>
<p>The Review home page gives you a summary of what is happening in your review and gives you access to many of the program''s functions.</p>
<div class="container">
  <div class="row">
	<div class="col-sm">
      <b>Coding progress</b><br>
	  <img class="img"  src="Images/CodingSummary.png" /><br>
	  On the left is an up-to-date summary of the coding that has taken place for each coding tool.<br> 
    </div>
    <div class="col-sm">
      <b>Reviews, My Work and Sources</b><br>
	  <img class="img"  src="Images/revWrkSources.gif" /><br>
	  In the middle are the <b>My Reviews</b>, <b>My Work</b> and <b>Sources</b> buttons giving you access to your reviews, coding assignments and sources.<br> 
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
	Along the top of the page are a number of tabs giving you access to different areas of the program. <b>References</b> are where you can see your references listed. <b>Frequencies</b> reports, <b>Crosstabs</b> reports and <b>Search</b> can be accessed from here. Further help is available on those pages.<br><br>
	</div>	
	<div class="col-sm">
	<b>Other functions</b><br>
	Below the tabs are buttons giving you access to 3 importent areas of the program.<br>
	<b>Import Items</b> - is where you import your references into EPPI-Reviewer.<br>
	<b>Edit Codes</b> - allows you to set up your coding tools.<br><b>Import Codesets</b> - allows you to access coding tools from your other reviews including publicly available codesets.
	<br>Further help is available on those pages. 
	</div>
</div>
</div>
'
UPDATE TB_ONLINE_HELP
SET [HELP_HTML] = @Content
	WHERE [CONTEXT] = 'main\reviewhome'

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
	To list items based on coding select a code in a coding tool and click on <b>With this code</b>. The <b>With this code (Excluded)</b> option lists the coded items with the <b>E</b> flag.<br><br> 
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
	To generate a coding report select the items to include in the report and click <b>Coding report</b>. Next, select the coding tool(s) that will make up the report and click <b>Get report</b> to generate the report. You can Save the report as well as display it in a new tab for printing.<br><br>
	</div>
	<div class="col-sm">
	<b>Cluster</b><br>
	<img class="img"  src="Images/Cluster.png" /><br>
	Cluster uses the 3gLingo clustering engine to automatically generate a coded map of your references based on their abstracts. It can be used to quickly create an overview of your studies. The complexity of the coding can be adjusted using the available parameters.<br><br>
	</div>
</div>

<div class="row">
	<div class="col-sm">
	<b>Other options</b><br>
	Other options on this screen include:<br>
	<b>To RIS</b> - allows you to export your selected items as an RIS formatted text file.<br>
	<b>Import items</b> - takes you to the import items page.<br>
	<br> 
	</div>
</div>
</div>
'
UPDATE TB_ONLINE_HELP
SET [HELP_HTML] = @Content
	WHERE [CONTEXT] = 'main\references'
GO
