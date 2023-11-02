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
	<img class="img"  src="assets/Images/ManageSources01.png" /><br>
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
	Click on the Trash icon in the list of sources to <b>temporarily delete</b> a source. If you click the icon a second time the source will be undeleted.<br>
	<b>Permanently deleting</b> a source is a 2-step process.
	<ol>
	  <li>It must first be marked as deleted by clicking the Trash icon in ther list of sources. This can also be done on the <b>Review home</b> page 
	  in the sources panel.</li>
	  <li>Once marked as deleted and the source does not contain any master items of duplicate references, 
	  the <b>Delete forever</b> button will be enabled and available to permanently delete the source.</li>
	</ol>
</div>
</div>

<br>
<br>
'
UPDATE TB_ONLINE_HELP
SET [HELP_HTML] = @Content
	WHERE [CONTEXT] = 'sources\managesources'

GO


------------------------------------------------------------------


USE [ReviewerAdmin]
GO

select * from TB_ONLINE_HELP
where CONTEXT = 'editref'
if @@ROWCOUNT = 0
begin
	insert into TB_ONLINE_HELP (CONTEXT, HELP_HTML)
	VALUES ('editref', 'tmp_place_holder')
end
GO


declare @Content nvarchar(max) = '

<div class="container-fluid">
<div class="row">
	<div class="col-sm">	
	<br>
	<b>Edit Reference</b><br>
	<img class="img"  src="assets/Images/EditRef.png" /><br>
	<br><br>
	</div>
	<div class="col-sm">		
	<br><br>
	From the Edit Reference screen you can change the reference type using the dropdown menu.<br>
	The fields displayed will change depending the reference type selected.<br>
	If you toggle the <b>Show option fields</b> option, more reference fields will become visible.<br>
	Click <b>Save</b> to write your changes to the database.<br>
	Click <b>Save and Close</b> to save your changes and return to the Items Details page.<br>
</div>
</div>

<br>
<br>
'
UPDATE TB_ONLINE_HELP
SET [HELP_HTML] = @Content
	WHERE [CONTEXT] = 'editref'

GO

----------------------------------------------------------------------------------------

USE [ReviewerAdmin]
GO

select * from TB_ONLINE_HELP
where CONTEXT = 'editcodesets'
if @@ROWCOUNT = 0
begin
	insert into TB_ONLINE_HELP (CONTEXT, HELP_HTML)
	VALUES ('editcodesets', 'tmp_place_holder')
end
GO


declare @Content nvarchar(max) = '

<p><b>Edit Coding Tools</b> is where you can create and edit the coding tools in your review.<br />
</p>
<div class="container">
<div class="row">
	<div class="col-sm">		
	<b>Create a coding tool.</b><br>
	<img class="img" src="assets/Images/CodingToolCreate.png"><br><br>
	</div>
	<div class="col-sm">
	<br>
	Click <b>Add Coding Tool</b> to create a new coding tool.<br>
	Select the Coding Tool Type from the dropdown menu.<br>
	<ul>
	  <li><b>Standard</b> - for coding and data-extraction: allows coding tools with multiple levels of hierarchy.</li>
	  <li><b>Screening</b> - for screening: permits one level of hierarchy (to simplify comparisons) and only codes of "Include" and "Exclude" types.</li>
	  <li><b>Administration</b> - for administrative coding, such as allocations. Allows only codes of "Selectable" and "Non Selectable" types and cannot be used for comparisons.</li>
	</ul>
	Please see the example review placed in your account for examples of these coding tool types.<br>
	</div>	
</div>
<div class="row">
	<div class="col-sm">	
	Enter a name for the coding tool and select the Data Entry mode (for Screening and Standard tool types).<br>
	<ul>
	  <li><b>Normal</b> - single coding - coding is marked as complete when a code is applied.</li>
	  <li><b>Comparison</b> - multiple coding - assumes multiple reviewers will be coding an item and a comparison of coding will take place.</li>
	</ul>
	A coding tool in <b>Normal</b> mode will display 1 head while a coding tool in <b>Comparison</b> mode will have 2 heads.<br>
	</div>	
</div>
<div class="row">
	<div class="col-sm">	
	<img class="img" src="assets/Images/SingleVsComparisonTool.png"><br>	
	</div>	
	<div class="col-sm">
	<br>
	You can also enter a description of the coding tool.<br>
	Be sure to click <b>Create</b> when you are finished.<br>
	</div>	
</div>
<br><br>
<div class="row">
	<div class="col-sm">
	<b>Building a Screening tool</b><br>
	<img class="img" src="assets/Images/ScreeningToolCreate.png"><br>
	</div>
	<div class="col-sm">
	<br>
	After creating the screening tool (as described earlier) you can add the individual codes by clicking <b>Add Child</b>.<br> 
	From the drop down menu you can select Exclude or Include codes. These code types are always selectable meaning there is a checkbox next to the code.<br>
	Give the code a name. You can also provide a description of the code for the reviewers guidance.<br>
	Click <b>Create</b> to add the code to the coding tool.<br>
	You can add as many codes as you wish but since it is a screening tool it can only have one level of hierarchy.<br>
	</div>
</div> 
<br><br>
<div class="row">
	<div class="col-sm">
	<b>Building a Standard coding tool</b><br>
	<img class="img" src="assets/Images/StandardToolCreate.png"><br>
	<br><br>
	Give the code a name. You can also provide a description of the code for reviewer guidance.<br>
	Click <b>Create</b> to add the code to the coding tool.<br>	
	</div>
	<div class="col-sm"><br>
	After creating a standard tool (as described earlier) you can add the individual codes by clicking <b>Add Child</b>.<br>
	A standard tool can have multiple levels of hierarchy. The data extraction tool in your example review (that you can find under your account) 
	is an example of a multi-level coding tool using different code types.<br>
	Select the type of code to add from the dropdown menu.<br>
	<ul>
	  <li><b>Not Selectable</b> - this code type does not have a checkbox. It is normally used as a heading in the coding tool.</li>
	  <li><b>Selectable</b> - this code type does have a checkbox. It is normally used as an answer code.</li>
	  <li><b>Outcome</b> - this code type is for entering numeric outcome data and for grouping similar outcomes.</li>
	  <li><b>Intervention</b> - this code type is for entering numeric outcome data and for grouping similar interventions.</li>
	  <li><b>Comparison</b> - this code type is for entering numeric outcome data and for grouping similar comparisons.</li>
	  <li><b>Outcome classification code</b> - this code type is for classifying outcomes and is selectable when creating meta-analyses.</li>
	</ul>
	</div>
</div>
<br><br>

<div class="row">
	<div class="col-sm">
	<b>Editing Coding tools</b><br>
	<img class="img" src="assets/Images/EditCodingTool.png"><br>
	</div>
	<div class="col-sm"><br>
	Click <b>Edit Coding Tool</b> to make changes to a coding tool (see <b>Edit Code</b> for changing individual codes).<br />
	To make changes the coding tool must be marked as Unlocked using the toggle switch.<br>
	You can edit the coding tool name<br>
	You can also change the Data Entry mode between Normal and Comparison. You will need to confirm this change as it has review 
	process implications. This option is only available for Screening and Standard code types.<br>
	You can edit the coding tool description.<br>
	Be sure to click <b>Save</b> to register your changes.<br>
	If you wish to delete the coding tool, and all coding associated with it, click <b>Delete Coding Tool</b>.
	</div>
</div>
<br><br>
<div class="row">
	<div class="col-sm">
	<b>Edit Code</b><br>
	<img class="img" src="assets/Images/EditCode.png"><br>
	</div>
	<div class="col-sm"><br>
	Click <b>Edit Code</b> to make changes to individual codes in a coding tool.<br>
	You can change the Code type using the dropdown menu. Be careful with this option as it can have implications with already completed coding.<br>
	You can also edit the code name and the code description<br>
	Be sure to click <b>Update</b> to register your changes.<br>
	If you wish to delete the code, and all coding associated with it, click <b>Delete Code</b>.<br>
	Details on <b>Move</b> can be found under <b>Moving Codes</b> and <b>Moving Coding Tools.</b>
	</div>
</div>
<br><br>
<div class="row">
	<div class="col-sm">
	<b>Moving Coding Tools</b><br>
	<img class="img" src="assets/Images/CodingToolMove.png"><br>
	</div>
	<div class="col-sm"><br>
	To the right of each coding tool there are arrow and chevron icons.<br>
	To move a coding tool up and down 1 position, click the up or down arrow icon.<br>
	To move a coding tool to the top or bottom of the the list, click the up or down chevron icon.<br>
	Below the coding tools there is a legend of what the different icons do when clicked.<br>
	</div>
</div>
<br><br>
<div class="row">
	<div class="col-sm">
	<b>Moving Codes</b><br>
	<img class="img" src="assets/Images/CodeMove.png"><br>
	</div>
	<div class="col-sm"><br>
	You can move codes, and their child codes, to different positions within a coding tool.<br>
	Select the Move/Place icon <img class="img" src="assets/Images/CodePlaneIcon.png"> of the code you wish to move.<br>
	In the screen shot, the <b>Language of report</b> code, and all of it''s child codes will be moved from the <b>Description of the sample</b> 
	into the <b>How can the study be identified</b> section.<br>
	None of the coding will be affected by this move.<br>
	You can select any code with the place move <img class="img" src="assets/Images/CodeMoveIcon.png"> as the destination code.<br>
    Not all codes can be destination codes. This is because some moves are impossible (ex. you cannot move a code into one of its own children) 
	while others would break coding tool depth restrictions.<br>
	</div>
</div>
<br><br>
<div class="row">
	<div class="col-sm">
	<b>Placing Codes</b><br>
	<img class="img" src="assets/Images/CodePlace.png"><br>
	</div>
	<div class="col-sm"><br>
	You can place a code below another code within the same level of hierarchy using the <b>Place</b> option.<br>
	This is useful when you have a long list of codes (ex. a list of countries) and you don''t want to click the up or down icon multiple times.<br>
	In the screen shot, the <b>Thailand</b> code has been selected (using the Move/Place icon <img class="img" src="assets/Images/CodePlaneIcon.png">), and is to be placed below the <b>Chile</b> code.<br>
	You can select any code with the place icon <img class="img" src="assets/Images/CodePlaceIcon.png"> as the destination code.
	Once the source code and destination code are selected, click the <b>Place</b> button.<br>
	</div>

</div>
<br><br>

<p>
You can import existing coding tools into your review by clicking the <b>Import Coding Tool(s)</b> button.<br>
Further details are available on that page.<br>
</p>

</div>


<br>
<br>

'
UPDATE TB_ONLINE_HELP
SET [HELP_HTML] = @Content
	WHERE [CONTEXT] = 'editcodesets'

GO


--------------------------------------------------------------------------


USE [ReviewerAdmin]
GO

select * from TB_ONLINE_HELP
where CONTEXT = 'importcodesets'
if @@ROWCOUNT = 0
begin
	insert into TB_ONLINE_HELP (CONTEXT, HELP_HTML)
	VALUES ('importcodesets', 'tmp_place_holder')
end
GO


declare @Content nvarchar(max) = '

<p><b>Import Coding Tools</b> is where you add coding tools from your other reviews or from a list of publicly available tools.<br />
</p>
<div class="container">

<div class="row">
	<div class="col-sm">
	<b>Import Coding Tools</b><br>
	<img class="img" src="assets/Images/CodingToolImport01.png"><br><br>
	</div>
	<div class="col-sm"><br>
	You can pick from a list of publicly available coding tools or coding tools from any of your existing reviews.<br>
	The Standard review selection of tools will be similar to those found in the example review.<br>
	</div>
</div>

<div class="row">
	<div class="col-sm">
	<b>Publicly Available Coding Tools</b><br>
	<img class="img" src="assets/Images/CodingToolImport02.png"><br>
	</div>
	<div class="col-sm"><br>
	You can preview the coding tools before importing them as well as compare them to any existing coding tools in your review.<br>
	If you know of any coding tools that would benefit other reviewers, please let us know and we can add them.<br> 
	</div>
</div>


</div>


<br>
<br>

'
UPDATE TB_ONLINE_HELP
SET [HELP_HTML] = @Content
	WHERE [CONTEXT] = 'importcodesets'

GO

---------------------------------------------------------------------------------------