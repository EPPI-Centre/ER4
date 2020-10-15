USE [ReviewerAdmin]
GO




declare @Content nvarchar(max) = '
<p class="font-weight-bold">Collaborate</p>
<p>The collaborate tab allows to, create, manage and use <b>Coding Assignments</b> and <b>Comparisons</b>. The process of creating Coding Assignments starts with creating or identifying the code(s) which will contain the group of items used for an assignment. Users can then create coding assignments using this code(s) as the <em>Reference group to assign</em>.<br />
Comparisons are used to identify and reconcile disagreements resulting from double and multiple coding exercises (where more than one reviewer will code/screen the same items independently).
</p>
<div class="container">
<div class="row">
	<div class="col-sm">		
	<b>Expand/Collapse the area of interest.</b><br>
	<img class="img" src="Images/CollaborateExpandCollapse.gif"><br>
	You can <b>expand</b> or <b>collapse</b> the two main areas of the screen, depending on what you need to use.<br><br>
	</div>
	<div class="col-sm">		
	<b>Toolbar commands.</b><br>
	<img class="img" src="Images/CollaborateToolbar.gif"><br>
	The main toolbar allows to: <b>Create Reference Groups</b>, which are used to drive Coding Assignments; <b>Create a new code</b>, which might be useful to organise "Reference Groups"; <b>Create Coding Assignments</b> and <b>Create Comparisons</b>.<br><br>
	</div>	
</div>

<div class="row">
	<div class="col-sm">
	<b>Create Reference Groups</b><br>
	<img class="img" src="Images/CollaborateCreateRNDcodes.gif"><br>
	This option allows to create groups of randomly selected references, using various criteria to identify the relevant references. These groups can then be used to assign work (Create Coding Assignments). Once created, it is <b>recommended to rename the new codes</b> so to make their purpose clear.<br><br> 
	</div>
	<div class="col-sm">
	<b>Create Coding Assignments</b><br>
	<img class="img" src="Images/CollaborateCreateCodingAssignments.gif"><br>
	Coding Assignments allow to <b>assign work</b> to a given reviewer and to <b>keep track</b> of the progress made. For double screening the same reference Group can be used to create two assignments, one for each reviewer.<br><br>
	</div>
</div> 

<div class="row">
	<div class="col-sm">
	<b>Create Comparisons</b><br>
	<img class="img" src="Images/CollaborateCreateComparison.gif"><br>
	Comparisons are used to <b>find, count and resolve disagreements</b> in double/multiple coding exercises. Comparisons also provide a <b>snapshot</b> of the Agreements Vs. Disagreement counts as present <b>when the comparison was created</b>.<br><br>
	</div>
	<div class="col-sm">
	<b>Using Comparisons</b><br>
	<img class="img" src="Images/CollaborateUsingComparisons.gif"><br>
	Using Comparisons, you can <b>list the Agreed and Disagreed</b> references. You can also "<b>Complete</b>" the Agreed coding, and access the <b>Reconcile Page</b> to assess the Disagreed coding in detail. Please note that the Reconcile Page is optimised to deal with (simple) screening disagreements.<br><br>
	</div>
</div>
</div>


<p class="font-weight-bold">Distribute Work Wizard</p>
<p>The Distribute Work Wizard will guide you through the process of creating coding assignments for the members of your review team. It has the flexibility to accommodate almost any variation of coding assignment using normal or comparison coding. Through each step you can preview the items to be assigned and who the work is assigned to.</p>

<div class="container-fluid">
<div class="row">
	<div class="col-sm">		
	<b>Distribute Work Wizard</b><br>
	<img class="img"  src="Images/DistributeWork.gif" /><br><br>
	</div>
	<div class="col-sm">
	<br>
	<b>Step 1</b> - select the references to be assigned based on previous coding or choose all items. You can also choose a percentage of the selected items and then preview the total number of items selected.<br>
	<b>Step 2</b> - select the coding tool to be used and specify whether it is normal or comparison coding. The wizard will create groups of items for each reviewer so specify where those groups will reside.<br>
	<b>Step 3</b> - assign the items for coding to specific individuals and chose whether the assignments are distributed evenly or manually, and whether groups are shared or one per person. You must preview the final assignments before allocating the work to your team.<br><br>
	The newly created coding assignments can be found in the <b>Coding Assignments</b> table
	</div>
</div>	
</div>
'
UPDATE TB_ONLINE_HELP
SET [HELP_HTML] = @Content
	WHERE [CONTEXT] = 'main\collaborate'

GO