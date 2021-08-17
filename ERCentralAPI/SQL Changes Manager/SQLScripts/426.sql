USE [ReviewerAdmin]
GO

select * from TB_ONLINE_HELP
where CONTEXT = 'reconciliation'
if @@ROWCOUNT = 0
begin
	insert into TB_ONLINE_HELP (CONTEXT, HELP_HTML)
	VALUES ('reconciliation', 'tmp_place_holder')
end
GO


declare @Content nvarchar(max) = '
<p class="font-weight-bold">Reconciliation</p>
<p>The Reconciliation screen is optimsed for comparing and reconciling coding in a simple coding tool (such as a screening tool). 
You can choose which reviewer''s coding is correct on an item-by-item basis and mark that version as complete. If you need to compare coding in a more complex coding tool (such as data extraction)
you will want to use the <b>Treeview reconciliation</b> (as explained further down).
<br />
</p>

<div class="container">
	<div class="row">

	<div class="col-sm">
	<p><b class="text-primary">General layout</b></p>
	<img class="img" src="Images/Reconcile01.png"><br><br>
	</div>

	<div class="col-sm">
	<br>
	The comparison is in a table with each reviewer in their own column and each row representing an item from the comparison.<br>
	The coding for each reviewer is displayed along with buttons to mark the correct coding as complete.<br>
	You can either click on the <b>Complete</b> button or the <b>Complete & lock</b> option to choose the correct version. 
	The Complete & lock option will lock the coding so 	other reviewers can not make changes to it.<br>
	Completed coding will have a green background and can be un-completed by clicking the <b>Un-complete</b> button.
	<br><br>
	</div>

	</div>

	<div class="row">
	<div class="col-lg">
	If there is text in a code''s Info box or pdf coding is assigned to the code it will be displayed.<br>
	If an item has arms as part of its coding, the arm the coding is assigned to will be clearly displayed next to the selected code.<br>
	At the bottom of the screen can be found the full reference including the abstract to help with the reconciliation process.<br>
	Clicking on the Item ID link in each row will take you to the Item Details page where more detailed adjustment of the coding can take place.
	<br><br>
	</div>
	</div>

</div> 

<div class="container">
	<div class="row">

	<div class="col-sm">
	<p><b class="text-primary">Complex coding</b></p>
	<img class="img" src="Images/Reconcile02.png"><br><br>
	</div>

	<div class="col-sm">
	<br>
	If you are viewing coding that has more than one level of hierarchy, you can click the code to display the code path.<br>
	Although a more complex coding tool with multiple sections and child codes can be viewed in the <b>Reconcile</b> screen, it is recommended
	that you use the <b>Treeview reconciliation</b> functionality instead.<br>
	The <b>Treeview reconciliation</b> functions will allow you to view and reconcile complex coding in much more detail. It can be accessed by clicking a row in the reconcile screen
	to select an item, and then clicking the <b>Show Detailed Tree-View</b> button. 
	<br>Further help on <b>Treeview reconciliation</b> can be found on that page.
	<br><br>
	</div>

	</div>


</div> 

'
UPDATE TB_ONLINE_HELP
SET [HELP_HTML] = @Content
	WHERE [CONTEXT] = 'reconciliation'

GO








USE [ReviewerAdmin]
GO

select * from TB_ONLINE_HELP
where CONTEXT = 'reconciliation\treesview'
if @@ROWCOUNT = 0
begin
	insert into TB_ONLINE_HELP (CONTEXT, HELP_HTML)
	VALUES ('reconciliation\treesview', 'tmp_place_holder')
end
GO


declare @Content nvarchar(max) = '
<p class="font-weight-bold">Treeview reconciliaton</p>
<p>The Treeview reconciliation screen allows you to compare and reconcile coding in a complex coding tool. You can copy coding, including Info box text
and PDF coding, between reviewers to create an agreed version of the coding.
<br />
</p>
<div class="container">
	<div class="row">

	<div class="col-sm">
	<p><b class="text-primary">General layout</b></p>
	<img class="img" src="Images/Treeview01.png"><br><br>
	</div>

	<div class="col-sm">
	<br>
	Each reviewer in the comparison has their own colum with its own treeview of the coding tool so you can examime each person''s work in detail.<br>
	You can step through the items in the comparison using the <b>First</b>, <b>Previous</b>, <b>Next</b> and <b>Last</b> controls at the top.<br>
	You can also step through the individual disagreements using a similar control.<br>
	Below the treeview are the ciation details including the study abstract.<br><br>
	</div>

	</div>
</div>

<div class="container">
	<div class="row">

	<div class="col-sm">
	<p><b class="text-primary">Disagreements</b></p>
	As you move through the disagreements they will be highlighted in the treeview with further details in the area below.<br>
	The coding for each reviewer is displayed, including the Info box text and any PDF coding (if the <b>Show PDF coding</b> option is selected).<br>
	If you are an administrator in the review you can copy coding between any reviewer in the comparison. If you are not an administrator you can
	only copy another reviewer''s coding to yourself.<br>
	To copy coding from one reviewer (the source) to another reviewer (the destination) click on the <b>Copy</b> button in the source reviewer''s 
	details area.
	<br><br>
	</div>

	<div class="col-sm">
	
	<img class="img" src="Images/Treeview02.png"><br><br>	
	</div>

	</div>
</div> 

<div class="container">
	<div class="row">

	<div class="col-sm">
	<p><b class="text-primary">Copy coding</b></p>
	<img class="img" src="Images/Treeview03.png"><br><br>	
	</div>

	<div class="col-sm">
	<br>
	If there are more than two reviewers in the comparison you can select the destination reviewer.<br>
	A confirmation window will appear that explains what is being copied and to whom.<br>
	If PDF coding is also being copied it will overwrite the destination if there is a clash on a particular page of the PDF. Otherwise,
	the source reviewer''s PDF coding will be merged with the destination reviewer''s PDF coding.<br>
	Although if is possible to view the uploaded PDFs in the citation details area at the bottom of the screen, 
	if more detailed PDF coding adjustments are required it will be necessary to do this on the <b>Item Details</b> page.<br>
	To confirm the operation click the <b>Copy coding</b> button.
	<br><br>
	</div>

	</div>
</div> 

<div class="container">
	<div class="row">

	<div class="col-sm">
	<p><b class="text-primary">Completed coding</b></p>
	Once you have examined the disagreements and are satisfied that a one of the reviewer''s coding is now the agreed version you can mark that 
	reviewer''s coding as complete.<br>
	You can either click on the <b>Complete</b> button or the <b>Complete & lock</b> option. The Complete & lock option will lock the coding so
	other reviewers can not make changes to it.
	<br><br>
	</div>

	<div class="col-sm">
	
	<img class="img" src="Images/Treeview04.png"><br><br>	
	</div>

	</div>
</div> 




 
'
UPDATE TB_ONLINE_HELP
SET [HELP_HTML] = @Content
	WHERE [CONTEXT] = 'reconciliation\treesview'

GO
