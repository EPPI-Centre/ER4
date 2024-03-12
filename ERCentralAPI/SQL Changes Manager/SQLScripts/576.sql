USE [ReviewerAdmin]
GO

select * from TB_ONLINE_HELP
where CONTEXT = 'itemdetails'
if @@ROWCOUNT = 0
begin
	insert into TB_ONLINE_HELP (CONTEXT, HELP_HTML)
	VALUES ('itemdetails', 'tmp_place_holder')
end
GO


declare @Content nvarchar(max) = '


<p class="font-weight-bold">Items details</p>
<p>The <b>Items details</b> screen is where much of the data entry in EPPI-Reviewer takes place. You also have access to a number of other areas in the 
program using the tabs on the right side of the screen. Further help is available in those tabs.</p>

<div class="container-fluid">
<div class="row">
	<div class="col-sm">		
	<b>Coding</b><br>
	<b>Coding</b> is carried out in the Item Details screen. On the right side of the screen is the citation while on the left are the coding tools.<br>
	<img class="img"  src="assets/Images/ItemDetails-Coding.gif" />
	</div>
	<div class="col-sm">
	<br>	 
	Clicking a check-box next to a code assigns that code to the item. Clicking the <b>Info</b> buttons allows you to add textual information to your selected code.<br>
	To remove coding, de-select a check-box and confirm your action.<br>
	To move through your references use the <b>Previous</b>, <b>Next</b>, <b>First</b> and <b>Last</b> buttons.<br>The <b>Auto-advance</b> option will automatically 
	move to the next item when you select a code.<br>
	<b>Hot keys</b> - for ''mouse free'' coding.<br>
	<img class="img"  src="assets/Images/HotKeys.png" /><br>	
	Select a coding tool or section of coding tool and click the "Hot key" icon. This will place numbers next to the child codes. Clicking <b>Alt</b> and the number will select that code.
	</div>
</div>
<br><br>
<div class="row">
	<div class="col-sm">		
	<b>Edit coding tools</b><br>
	<img class="img"  src="assets/Images/ItemDetails-EditCodes.gif" />
	</div>
	<div class="col-sm">
	<br>
	You can <b>add</b> and <b>edit</b> codes using the buttons on the left side of the screen (sitting above the coding tools).<br>
	To add a code select the coding tool and a location in the coding tool (if the tool has multiple levels). Click on the <b>+</b> button and enter a code type, 
	code name and code description. Click <b>Create</b> and you will see the new code in the coding tool.<br>
	<img class="img"  src="assets/Images/CodeTreeIcons.png" /><br>	
	You can reposition the code by clicking on the <b>Up</b> and <b>Down</b> <i>arrow</i> buttons.<br>
	The <b>Up</b> and <b>Down</b> <i>chevron</i> buttons will move the selected code or coding tool to the top and bottom of the list.<br>
	To edit a code, select the code and click the <b>Edit</b> button (looks like a pencil). This will allow you to edit the code type, code name and description. 
	To save your edits click <b>Update</b>.<br>
	Deleting a code is the same as editing a code except you must click the <b>Delete Code</b> button and confirm your actions. 
	</div>
</div>	

<br><br>
<div class="row">
	<div class="col-sm">		
	<b>Term highlighting</b><br>
	You can highlight relevant and non-relevant terms in the title and abstract to assist in the screening process.<br>
	<img class="img"  src="assets/Images/ItemDetails-TermHighlight.gif" />
	</div>
	<div class="col-sm">
	<br>	
	Check <b>Show terms</b> to make the highlighted terms visible.<br>
	To highlight terms, select words in the title and/or abstract and click either the <b>Add relevant term</b> or <b>Add irrelevant term</b> buttons.<br>
	To edit the list of terms click the <b>Show/Hide terms</b> button. You can now add, edit and delete terms and change whether they are 
	relevant or irrelevant terms.<br>
	If a term is edited, you can save or cancel the change by clicking the icons.<br><br> 
	<img class="img"  src="assets/Images/TermEditing.png" /><br><br>
	The style of highlighting can be changed using the <b>Change style</b> dropdown menu.<br> 	
	</div>
</div>	

<br><br>
<div class="row">
	<div class="col-sm">		
	<b>Numeric outcome data</b><br>
	<img class="img"  src="assets/Images/ItemDetails-outcomes01.png" />
	
	</div>
	<div class="col-sm">
	<br>
	You can enter numeric outcome data into EPPI-Reviewer using forms specific to the type of outcomes found in your studies.<br>
	Any code with the codetype <b>Comparison</b>, <b>Outcome</b> or <b>Intervention</b> will have an <b>Outcomes</b> button next to it.<br> 
	Clicking on the <b>Outcomes</b> button will open a panel allowing you to <b>Edit</b> an existing outcome or create a <b>New outcome</b>.<br>
	<br>
	<img class="img"  src="assets/Images/ItemDetails-outcomes02.png" /><br><br>
	If the outcome you wish to create is similar to an already existing outcome, click to the <b>copy</b> icon.
	</div>
</div>	

<br><br>
<div class="row">
	<div class="col-sm">
	<b>Create or edit an outcome</b><br>
	<img class="img"  src="assets/Images/ItemDetails-outcomes03.png" />
	
	</div>
	<div class="col-sm">
	<br>
	Enter a name and specify the outcome type (continuous or binary). A number of different formats for entering the data are available and the form will 
	change accordingly.<br>
	<img class="img"  src="assets/Images/ItemDetails-outcomes04.png" /><br><br>
	Any predefined arms, time points, interventions, comparisons and outcomes are available for selection. As well you can select any codes 
	within your coding tool to help characterise the outcome data.<br>
	<img class="img"  src="assets/Images/ItemDetails-outcomes05.png" /><br><br>
	All saved outcome data is available to EPPI Reviewer''s reporting and meta-analyis functions.
	</div>
</div>	


<br><br>
<div class="row">
<div class="col-sm">		
	<img class="img"  src="assets/Images/FindEdit.png" /><br>
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
</div>

<br><br>

'
UPDATE TB_ONLINE_HELP
SET [HELP_HTML] = @Content
	WHERE [CONTEXT] = 'itemdetails'

GO