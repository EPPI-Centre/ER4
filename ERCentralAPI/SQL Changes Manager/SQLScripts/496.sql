USE [ReviewerAdmin]
GO

select * from TB_ONLINE_HELP
where CONTEXT = 'webdbs'
if @@ROWCOUNT = 0
begin
	insert into TB_ONLINE_HELP (CONTEXT, HELP_HTML)
	VALUES ('webdbs', 'tmp_place_holder')
end
GO


declare @Content nvarchar(max) = '
<p class="font-weight-bold">Links, Arms and Timepoints</p>
<p>The <b>Links Arms Timepoints</b> tab has functionality to assist in linking related documents, dealing with multiple arm trials and the entry of numeric outcome data at specific timepoints.</p>
<div class="container-fluid">
<div class="row">
	<div class="col-sm">		
	<b>Linked items</b><br>
	<img class="img"  src="Images/ItemDetails-Link.gif" />
	</div>
	<div class="col-sm">
	<br>
	The <b>Linking</b> function allows you to link <b>secondary</b> items to a <b>primary</b> item. This might be appropriate where there are multiple items/references describing the same study and you wish to carry out data extraction just once per study. The primary item could contain the full study data extraction and the secondary items will be linked to that primary item for easier access.<br>
	Starting from the primary item, create a new <b>link</b> by clicking the <b>Add new Link</b> button. Enter the ID of the secondary item, click <b>Find item</b> to check you have the correct item, enter a description and then click <b>Save</b>. You can link multiple secondary items to the primary item.<br>
	Once an item is linked you can <b>Edit</b> the link, view 👁️ the full reference or delete 🗑️ the link.<br>
	<b>Report</b> will display the primary item and the linked items in a new browser tab.
	</div>
</div>
<br>
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
	WHERE [CONTEXT] = 'webdbs'

GO