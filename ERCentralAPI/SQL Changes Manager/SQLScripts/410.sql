USE [ReviewerAdmin]
GO

select * from TB_ONLINE_HELP
where CONTEXT = 'main\reports'
if @@ROWCOUNT = 0
begin
	insert into TB_ONLINE_HELP (CONTEXT, HELP_HTML)
	VALUES ('main\reports', 'tmp_place_holder')
end
GO


declare @Content nvarchar(max) = '
<p class="font-weight-bold">Reports</p>
<p>From the Reports tab you can generate <b>Frequency</b> and <b>Crosstab</b> reports as well as create, edit and run <b>Configurable</b> reports.<br />
</p>
<div class="container">
	<div class="row">
	<div class="col-lg">
	<b class="text-primary">Frequency report</b><br>
	A frequency report will display the number of records associated with each code.<br><br>
	</div>
	</div>

	<div class="row">
	<div class="col-sm">		
	
	<img class="img" src="Images/Frequency01.png"><br>
	To create a frequency report select a coding tool or a code with child codes and click <b>Set</b> for <b>Rows</b>.<br>
	You can restrict the records in the frequency report based on a particular code by selecting that code and clicking <b>Set filter</b>.<br>
	The report can be restricted to references with the (<b>I</b>)ncluded or (<b>E</b>)xcluded flags or Both.<br>
	Click <b>Get Frequencies</b> to generate a table of codes and counts.<br>
	Clicking a counts link will display thoses records in the <b>References</b> tab.<br><br>
	</div>

	<div class="col-sm">			
	<img class="img" src="Images/Frequency02.png"><br>	
	Select one of the 3 available options to display the report as a <b>Table</b>, <b>Pie chart</b> or <b>Bar chart</b>.<br>
	The <b>None of the above</b> option shows the number of items not assigned any of the codes in the report.<br>
	Click the <b>Export</b> button to download an Excel table of the report.<br><br>
	</div>
	</div>
</div>

<div class="container">
	<div class="row">
	<div class="col-lg">
	<hr>
	<b class="text-primary">Crosstab report</b><br>
	A Crosstab report will cross-tabulate the answers of one question against another into a matrix to find the relationship between certain answers.<br>
	Each node in the report displays the number of records that have been assinged the row and column codes.
	<br><br>
	</div>
	</div>

	<div class="row">
	<div class="col-sm">
	<img class="img" src="Images/Crosstab01.png"><br><br>	
	</div>

	<div class="col-sm">
	To create a crosstab report select a coding tool or a code with child codes for the <b>Row</b> axis and click <b>Set</b>.<br>
	Do the same for the <b>Column</b> axis. It is possible to have the same selection in both axes.<br>
	You can restrict the records in the crosstab report based on a particular code by selecting that code and clicking <b>Set filter</b>.<br>
	The report can include items with the (<b>I</b>)ncluded or (<b>E</b>)xcluded flags or Both.<br>
	Click <b>Get Crosstabs</b> to generate the report.<br>
	Clicking a node link will display thoses records in the <b>References</b> tab.<br>	
	The <b>None of the above</b> value is the number of records not assigned one of the row codes.<br>
	The <b>None of these</b> value is the number of records not assigned one of column codes.<br>
	Click the <b>Export</b> button to download an Excel table of the report.<br><br>
	</div>
	</div>
</div> 


<div class="container">
	<div class="row">
	<div class="col-lg">
	<hr>
	<b class="text-primary">Configurable report</b><br>
	A configurable report allows you to display your extracted data in many different formats. It is normally set up as a table with 
	each column representing the codes or coding tools used the extract the study data. Once the report structure is saved, 
	it can be run multiple times against different combinations of studies to display the extracted data.
	<br><br>
	</div>
	</div>

	<div class="row">
	<div class="col-sm">
	<b>Creating a new configurable report</b><br>
	<img class="img" src="Images/ConfigReport01.png"><br>
	Click the <b>Configurable report</b> button and then the <b>New report</b> button.<br>
	Give the report a name and select the report type.<br> 	
	A <b>Question</b> report summarises the responses for all of the codes directly below a Question/Parent code.<br>
	An <b>Answer report</b> displays an individual code''s responses.<br>
	Click <b>Continue</b> to save the new report.<br>
	Note: the <b>Save</b> and <b>Save and Close</b> buttons will be enabled whenever you make a change.<br><br>
	</div>

	<div class="col-sm">
	<b>Adding columns and codes to the report</b><br>
	<img class="img" src="Images/ConfigReport02.png"><br>
	To add a column to the report click the <b>Add column</b> button.<br>
	To add a code or codes to the column, select the relevant code from the code panel on the right and click the ''<b>+</b>'' (add selected code to column) button within the column.<br>
	If you are creating a <b>Question</b> report you will need to select a code with child codes since it is the extracted data of the child codes that will be displayed when the report is run.
	You can think of the selected code as the ''Question'' and the child codes as the ''Answers''.<br>
	If you are creating an <b>Answer</b> report you will only be able to add one ''answer'' code to each column.<br>
	<br>
	</div>
	</div>

	<div class="row">
	<div class="col-sm">
	<b>Edit a column''s content</b><br>
	<img class="img" src="Images/ConfigReport03.png"><br>
	Click the ''edit'' icon (Edit column content) in the column to edit the column content such as the column name and order of the codes.<br><br>	
	<div class="row">
	
	</div>

	</div>

	<div class="col-sm">
	<br>
	<img class="img" src="Images/ConfigReport04.png"><br>
	</div>
	<div class="col-sm">
	<br>
	Each code in the column can be edited by clicking its ''edit'' icon.<br>
	You can change the wording of the code name as it appears in the report.<br>
	You can also specify what extracted data will be displayed in the report.<br>
	(note: ''coded text'' is the pdf highlighted extracted text)<br><br>
	</div>
	</div>
</div> 


<div class="container">
	<div class="row">
	<div class="col-lg">
	<hr>
	<b class="text-primary">Running a configurable report</b><br>
	Once a configurable report has been saved the report structure can be used multiple times to run reports using different combinations of studies. The completed report
	can be imported into other software such as Word or Excel for futher formatting.
	<br><br>
	</div>
	</div>


	<div class="row">
	<div class="col-sm">
	<b>Select the report and paramters</b><br>
	<img class="img" src="Images/RunConfig01.png"><br>
	
	</div>

	<div class="col-sm">
	<br>
	Click the <b>Run reports</b> button to open the report panel.<br>	 	
	Decide what references to include in the report and the order. These can be items selected in the <b>References</b> tab or based 
	on an identifying code.<br>
	Select the type of report: <b>Standard</b> (default), <b>Risk of Bias</b> or <b>Outcome</b>.<br>
	Select the report to run from the dropdown menu.<br>
	<b>Display options:</b><br>
	Choose the fields to include in the report (each selected field will be a new column).<br>
	You can specify whether ''uncoded'' items are visible in the report.<br>
	There are options for bullet points and user-defined tags for identifying extracted pdf text.<br>
	<b>Alignment options:</b><br> 
	<ul>
	  <li><b>Horizontal</b> - report is in a table format with each reference as a row and extracted data in the columns.</li>
	  <li><b>Vertical</b> - The report is vertically stacked. This is useful for summary reports with long descriptive text.</li>
	</ul>
	<br><br>
	</div>
	</div>


	<div class="row">
	<div class="col-sm">
	<img class="img" src="Images/RunConfig03.png"><br><br>
	Click <b>Run/View</b> to generate the report and display it in a new tab.<br> 
	Clicking the ''Save'' icon will download the report as an html file.<br>
	You can open an html file using programs such as Word and Excel to add further formatting.<br><br>
	A <b>Question</b> report can be set up with each column as a theme and have multiple related questions in each column. The extracted data displayed is tied to the 
	''answer'' (child codes) of each question.<br>
	In an <b>Answer</b> report each column will have a single ''answer'' code. This type of report might used when examining a single question.<br><br>
	</div>

	<div class="col-sm">
	<b>Types of report: Standard, Horizontal Question report</b><br>
	<img class="img" src="Images/RunConfig02.png"><br>

	<br>
	<br>
	</div>
	</div>

	<div class="row">

	<div class="col-sm">
	<b>Types of report: Risk of Bias report</b><br>
	<img class="img" src="Images/RunConfig04.png"><br><br><br>
	</div>

	<div class="col-sm">
	<br>
	A Risk of Bias (RoB) report can be used to summarise reviewer judgements about the reliability of the studies in a review.<br>
	There are existing RoB coding tools can be imported into your review but you can also create your own. The order of the codes in each question must be:
	<il>
	  <li>Low risk</li>
	  <li>high risk</li>
	  <li>Unclear</li>
	</il>
	When running the report click the <b>Risk of Bias</b> tab, select what fields to display and click <b>Run/View</b>.
	Two RoB diagrams will be displayed. The first is an item by item summary of the ratings. The second shows the percentage of items assigned each rating.<br>
	As with a standard report, clicking the ''Save'' icon will download the report as an html file that can be loaded into other software for further formatting.<br><br>
	<br>
	</div>
	</div>

	<div class="row">
	<div class="col-sm">
	If you have entered numeric outcome data into EPPI-Reviewer you can run an outcome report.<br>
	Create an <b>Answer</b> report and place the outcome codes in each column.<br> 
	To run the report click the <b>Outcomes</b> tab, select what fields to display and click <b>Run/View</b>.<br>
	The outcome data will be shown in a table with each value in its own column.<br>
	Clicking the ''Save'' icon will download the report as an html file that can be loaded into other software such as Excel for exporting into other statistical software.<br><br>
	</div>

	<div class="col-sm">
	<b>Types of report: Outcomes</b><br>
	<img class="img" src="Images/RunConfig05.png"><br>
	</div>

	</div>


</div> 

'
UPDATE TB_ONLINE_HELP
SET [HELP_HTML] = @Content
	WHERE [CONTEXT] = 'main\reports'

GO








USE [ReviewerAdmin]
GO

select * from TB_ONLINE_HELP
where CONTEXT = 'main\references'
if @@ROWCOUNT = 0
begin
	insert into TB_ONLINE_HELP (CONTEXT, HELP_HTML)
	VALUES ('main\references', 'tmp_place_holder')
end
GO


declare @Content nvarchar(max) = '
<div class="container">
	<div class="row">
		<div class="col-sm">		
		<b class="text-primary">Reference list</b><br>
		<img class="img"  src="Images/ReferenceList.gif" /><br>
		You can select individual or all items from your reference list. Clicking at the top of a column will order the list by that column. By default, the list is paged by 100 items. To list items based on the Include, Exclude and Delete flags click on the <b>I</b>, <b>E</b>, and <b>D</b> buttons. All imported items start with the <b>I</b> flag.<br><br>
		</div>
		<div class="col-sm">		
		<b class="text-primary">View options</b><br>
		<img class="img"  src="Images/ViewOptions.gif" /><br>
		Clicking on <b>View options</b> will allow to change what fields are shown in the reference list and edit the number of items displayed in each page.<br><br>
		</div>	
	</div>

	<div class="row">
		<div class="col-sm">
		<b class="text-primary">List items by coding</b><br>
		<img class="img"  src="Images/ListItemsByCoding.gif" /><br>
		To list items based on coding, select a code in a coding tool and click on <b>With this code</b>. The <b>With this code (Excluded)</b> option lists the coded items with the <b>E</b> flag.<br><br> 
		</div>
		<div class="col-sm">
		<b class="text-primary">Assign/remove codes</b><br>
		<img class="img"  src="Images/AssignCodeToItems.gif" /><br>
		To assign a code to multiple items, select the items and the code and then click <b>Assign code</b> to apply the code to the items. If you select <b>Remove code from selection</b> the code will be removed from the selected items.<br><br>
		</div>
	</div> 

	<div class="row">
		<div class="col-sm">
		<b class="text-primary">Coding report</b><br>
		<img class="img"  src="Images/CodingReport.gif" /><br>
		To generate a coding report select the items to include in the report and click <b>Coding report</b>. Next, select the coding tool(s) that will make up the report and click <b>Get report</b> to generate the report. You can <b>Save</b> the report as well as display it in a new tab for printing.<br><br>
		</div>
		<div class="col-sm">
		<b class="text-primary">Cluster</b><br>
		<img class="img"  src="Images/Cluster.png" /><br>
		Cluster uses the Lingo3G clustering engine to automatically generate a coded map of your references based on their abstracts. You can select what items should be included in the map and the complexity of the coding can be adjusted using the available parameters.<br><br>
		</div>
	</div>

	  <div class="row">	
		<div class="col-sm">
		<b class="text-primary">Quick Question Report</b><br>
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
		<div class="col">
		<b class="text-primary">Mark Items as Include/Excluded or Deleted</b><br>
		<img class="img"  src="Images/IncludeExcludeDelete.gif" /><br>
		By default, Items are imported as "<em>Included</em>" ("I" status flag). You can use this screen to change the status flag to sideline items in bulk, by flagging them as "Excluded" or "Deleted".<br>
		</div>
		<div class="col-md-12 col-lg-5">
		<br />
		The "In/Exclude" button allows to <b>mark items as included or excluded</b>. You can pick the items to be modified either by selecting them or by picking the "Documents with this code" option. <br />
		The "Trash" icon button allows to <b>mark as deleted</b> the items selected in the current list.<br />
		To "undelete" some deleted items, you can use the "In/Exclude" button to re-include the items in question or flag them as Excluded.<br />
		<b>Note:</b> excluded items can participate in frequency reports, if explicitly required; deleted items are ignored by most reporting features.<br />
		 Deleted items are usually understood as items that should not have been created/imported. Excluded items (optionally) represent items that have been screened and excluded.
		</div>
	</div>
	<br />
	<div class="row mt-2">
		<div class="col-sm">
		<b class="text-primary">Other options</b><br>
		Other options on this screen include:<br>
		<b>Import items</b> - takes you to the import items page. More help is available on that page.<br>
		<b>To RIS</b> - allows you to export your selected items as an RIS formatted text file.<br>	
		<br><br> 
		</div>
	</div>
</div>

<div class="container">
	<div class="row">
	<div class="col-lg">
	<hr>
	<b class="text-primary">Running a configurable report</b><br>
	Once a configurable report has been saved the report structure can be used multiple times to run reports using different combinations of studies. The completed report
	can be imported into other software such as Word or Excel for futher formatting.
	<br><br>
	</div>
	</div>


	<div class="row">
	<div class="col-sm">
	<b>Select the report and paramters</b><br>
	<img class="img" src="Images/RunConfig01R.png"><br>
	
	</div>

	<div class="col-sm">
	<br>
	Click the <b>Run reports</b> button to open the report panel.<br>	 	
	Decide what references to include in the report and the order. These can be items selected in the <b>References</b> tab or based 
	on an identifying code.<br>
	Select the type of report: <b>Standard</b> (default), <b>Risk of Bias</b> or <b>Outcome</b>.<br>
	Select the report to run from the dropdown menu.<br>
	<b>Display options:</b><br>
	Choose the fields to include in the report (each selected field will be a new column).<br>
	You can specify whether ''uncoded'' items are visible in the report.<br>
	There are options for bullet points and user-defined tags for identifying extracted pdf text.<br>
	<b>Alignment options:</b><br> 
	<ul>
	  <li><b>Horizontal</b> - report is in a table format with each reference as a row and extracted data in the columns.</li>
	  <li><b>Vertical</b> - The report is vertically stacked. This is useful for summary reports with long descriptive text.</li>
	</ul>
	<br><br>
	</div>
	</div>


	<div class="row">
	<div class="col-sm">
	<img class="img" src="Images/RunConfig03.png"><br><br>
	Click <b>Run/View</b> to generate the report and display it in a new tab.<br> 
	Clicking the ''Save'' icon will download the report as an html file.<br>
	You can open an html file using programs such as Word and Excel to add further formatting.<br><br>
	A <b>Question</b> report can be set up with each column as a theme and have multiple related questions in each column. The extracted data displayed is tied to the 
	''answer'' (child codes) of each question.<br>
	In an <b>Answer</b> report each column will have a single ''answer'' code. This type of report might used when examining a single question.<br><br>
	</div>

	<div class="col-sm">
	<b>Types of report: Standard, Horizontal Question report</b><br>
	<img class="img" src="Images/RunConfig02.png"><br>

	<br>
	<br>
	</div>
	</div>

	<div class="row">

	<div class="col-sm">
	<b>Types of report: Risk of Bias report</b><br>
	<img class="img" src="Images/RunConfig04.png"><br><br><br>
	</div>

	<div class="col-sm">
	<br>
	A Risk of Bias (RoB) report can be used to summarise reviewer judgements about the reliability of the studies in a review.<br>
	There are existing RoB coding tools can be imported into your review but you can also create your own. The order of the codes in each question must be:
	<il>
	  <li>Low risk</li>
	  <li>high risk</li>
	  <li>Unclear</li>
	</il>
	When running the report click the <b>Risk of Bias</b> tab, select what fields to display and click <b>Run/View</b>.
	Two RoB diagrams will be displayed. The first is an item by item summary of the ratings. The second shows the percentage of items assigned each rating.<br>
	As with a standard report, clicking the ''Save'' icon will download the report as an html file that can be loaded into other software for further formatting.<br><br>
	<br>
	</div>
	</div>

	<div class="row">
	<div class="col-sm">
	If you have entered numeric outcome data into EPPI-Reviewer you can run an outcome report.<br>
	Create an <b>Answer</b> report and place the outcome codes in each column.<br> 
	To run the report click the <b>Outcomes</b> tab, select what fields to display and click <b>Run/View</b>.<br>
	The outcome data will be shown in a table with each value in its own column.<br>
	Clicking the ''Save'' icon will download the report as an html file that can be loaded into other software such as Excel for exporting into other statistical software.<br><br>
	</div>

	<div class="col-sm">
	<b>Types of report: Outcomes</b><br>
	<img class="img" src="Images/RunConfig05.png"><br>
	</div>

	</div>


</div> 



'
UPDATE TB_ONLINE_HELP
SET [HELP_HTML] = @Content
	WHERE [CONTEXT] = 'main\references'

GO
