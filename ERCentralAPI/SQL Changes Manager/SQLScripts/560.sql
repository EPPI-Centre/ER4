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
	<img class="img"  src="assets/Images/ItemDetails-Coding.gif" />
	</div>
	<div class="col-sm">
	<br>
	<b>Coding</b> is carried out in the Item Details screen. On the right side of the screen is the citation while on the left are the coding tools.<br> 
	Clicking a check-box next to a code assigns that code to the item. Clicking the <b>Info</b> buttons allows you to add textual information to your selected code.<br>
	To remove coding, de-select a check-box and confirm your action.<br>
	To move through your references use the <b>Previous</b>, <b>Next</b>, <b>First</b> and <b>Last</b> buttons.<br>The <b>Auto-advance</b> option will automatically 
	move to the next item when you select a code.<br><br>
	<table>
		<tr>
		<td>
			<img class="img"  src="assets/Images/HotKeys.png" />
		</td>
		<td style="vertical-align:top; padding-left: 5px;">
			<b>Hot keys</b> - for ''mouse free'' coding.<br>
			Select a coding tool or section of coding tool and click the "Hot key" icon.<br>
			This will place numbers next to the child codes.<br>
			Clicking <b>Alt</b> and the number will select that code.
		</td>
		</tr>
	</table>
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
	<img class="img"  src="assets/Images/ItemDetails-TermHighlight.gif" />
	</div>
	<div class="col-sm">
	<br>
	You can highlight relevant and non-relevant terms in the title and abstract to assist in the screening process.<br>
	Check <b>Show terms</b> to make the highlighted terms visible.<br>
	To highlight terms, select words in the title and/or abstract and click either the <b>Add relevant term</b> or <b>Add irrelevant term</b> buttons.<br>
	To edit the list of terms click the <b>Show/Hide terms</b> button. You can now add, edit and delete terms and change whether they are 
	relevant or irrelevant terms.<br>
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
<br>

'
UPDATE TB_ONLINE_HELP
SET [HELP_HTML] = @Content
	WHERE [CONTEXT] = 'itemdetails'

GO




USE [ReviewerAdmin]
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


<p class="font-weight-bold">Duplicates</p>
<p>The <b>Duplicates</b> checking functionality in EPPI Reviewer is a very flexible and powerful tool for identifying duplicate items in your review. 
</p>

<div class="container-fluid">
<div class="row">
	<div class="col-sm">		
	<b>Get new duplicates</b><br>
	<img class="img"  src="assets/Images/Duplicates01.png" />
	</div>
	<div class="col-sm">
	<br>
	After clicking <b>Get New Duplicates</b>, similar items are placed in duplicates groups by looking at a number of fields in a structured 
	and "weighted" way. The duplicate groups are listed in a table on the left.
	Selecting a duplicate group will display the items on the right. The master item is at the top while the possible duplicate items are listed below it.<br>
	The master item is based on:
	<ol>
	<li>If the to-be-created group has no coded items, the oldest item is the master.</li>
	<li>If the to-be-created group has one or more coded items, the oldest coded item is the master</li>
	</ol>
	Differences between items are highlighted in different colours based on the degree of difference. The overall difference between the master item and 
	each possible duplicate item is represented by a similarity score. A similarity score of 1.0 indicates an exact duplicate. As the degree of difference 
	increases, the similarity score decreases.<br>
	Items can be manually marked as <b>A duplicates</b>, <b>Not a duplicates</b> or <b>Mark as master</b> using the appropriate buttons.<br>	
	</div>
</div>
<br><br>
<div class="row">
	<div class="col-sm">		
	<b>Mark automatically</b><br>
	<img class="img"  src="assets/Images/Duplicates02.png" />
	</div>
	<div class="col-sm">
	<br>	
	The <b>Mark automatically</b> function will automatically mark all items with a similarity score of 1.0 as a duplicate.<br>
	Select <b>Advanced Mark Automatically</b> to lower the similarity score threshold, the coded item threshold and the uploaded documents threshold.<br>
	<b>Note</b>: Use the Advanced options with care or you might create false duplicates.
	</div>
</div>	

<br><br>
<div class="row">
	<div class="col-sm">		
	<b>Duplicate groups</b><br>
	<img class="img"  src="assets/Images/Duplicates03.png" />
	</div>
	<div class="col-sm">
	<br>	
	When all of the items in a group are marked as either <b>A duplicate</b> or <b>Not a duplicate</b> the group is marked as <b>Done</b> in the
	first column of the duplicate groups table.<br>
	A large review might have 1000s of duplicate groups so the table is paged. You can adjust the page size by clicking the <b>Tools</b> button.
	Other options in Tools include:
	<ul>
	<li><b>1st to-Do</b> will move you to the next duplicate group for checking.</li>
	<li><b>Auto-Advance</b> will move you to the next duplicate group to check when all of the items in a group have been marked.</li>
	</ul>
	</div>
</div>	

<br><br>
<div class="row">
	<div class="col-sm">		
	<b>More - Find Related Groups</b><br>
	<img class="img"  src="assets/Images/Duplicates04.png" />
	<br><br>
	<b>Show items</b><br>
	<img class="img"  src="assets/Images/Duplicates05.png" />
	</div>
	<div class="col-sm">
	<br>
	When <b>Get new duplicates</b> is run multiple times due to new items entering the review it is possible that overlapping duplicate groups are created. 
	This means that the same item may appear in multiple groups.<br> 
	The <b>Find Related groups</b> dropdown has 3 functions to help identify overlapping duplicate groups. If overlapping groups are found, they will be listed in the duplicate group table.<br>
	<ol>
	<li><b>Find related groups</b> - This option will identify any existing duplicate groups that contain items that are also in the currently selected duplicate group.</li>
	<li><b>Find Groups by ItemIDs</b> - This option will allow you to enter Item IDs (comman separated). Clicking <b>Find</b> will list the existng duplicate groups that 
	contain the requested item IDs.</li>
	<li><b>Find Groups by Selected Item</b> - This option identifies existing duplicate groups based on any references selected on the Review home page reference list.</li>
	</ol>
	You can click the <b>Show items</b> button (in the <b>More</b> section) to display the reference list. Once displayed you can select/deselect further items.<br>
	</div>
</div>	

<br><br>
<div class="row">
	<div class="col-sm">		
	<b>More - Reset</b><br>
	If you decide that what you have done while duplicate checking is all wrong and wish to start again there are 2 reset 
	options in the <b>More</b> section<br><br>
	<img class="img"  src="assets/Images/Duplicates06.png" />
	<br><br>

	</div>
	<div class="col-sm">
	<br>

	<b>Soft Reset</b><br>
	This will delete all duplicate groups but keep information about references already marked as duplicates and give you a fresh start to re-evaluate 
	duplicates without losing the work you''ve done already.<br>
	Any documents already marked as duplicates will not be re-evaluated, and this will have a few consequences:
	<ul> 
	<li>When you run <b>Get new Duplicates</b> again you should get a smaller number of groups as the already "completed" groups will not reappear.</li>
	<li>Information about the old groups will be lost (ex. you will not be able to find out the similarity scores of items you have already marked as duplicates).</li>
	</ul>

	<b>Hard Reset</b><br>
	This will delete all of your duplicate data including all duplicate groups and  all information about what has / has not been marked as duplicates.<br>
	Any references already marked as duplicates will reappear (marked as Included).<br>
	You might wish to use the Hard Reset option when you have used Advanced Mark Automatically with too permissive thresholds and you 
	have created too many false positive duplicates.

	<br>
	</div>
</div>	

<br><br>
<div class="row">
	<div class="col-sm">		
	<b>More - Delete, Add to and Create Groups</b><br>
	<img class="img"  src="assets/Images/Duplicates07.png" />
	<br><br>
	</div>
	<div class="col-sm">
	<br>
	<b>Delete This Group</b><br>
	This option will delete the selected duplicate group.<br>
	<br>
	<b>Add to group</b><br>
	Click the <b>Show items</b> button to display the reference list. If you select an item or items from the list and then click <b>Add to group</b>,
	the selected items will be added to the presently selected duplicate group<br>
	<br>
	<b>Create Group</b><br>
	Click the <b>Show items</b> button to display the reference list. If you select 2 or more items from the list and click <b>Create Group</b>, the
	items will be displayed in the duplicate screen on the right side. Difference in the items will be highlighted and you will be able to decide which item should be the 
	Master item. You can also click <b>Get Related Groups</b> if the function believes an item is already in a duplicate group. To create the new duplicate group click <b>Create Group!</b>.
	<br>
	</div>
</div>	


<br><br>

'
UPDATE TB_ONLINE_HELP
SET [HELP_HTML] = @Content
	WHERE [CONTEXT] = 'duplicates'

GO